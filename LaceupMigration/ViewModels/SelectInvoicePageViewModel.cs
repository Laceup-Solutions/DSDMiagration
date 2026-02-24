using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
    public partial class SelectInvoicePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private Client? _client;
        private Order? _order;
        private bool _fromClientDetails;
        private bool _fromPaymentTab;
        private bool _initialized;
        private Term? _term;
        private List<SelectInvoiceItemViewModel> _masterList = new();
        private string _searchCriteria = string.Empty;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceMs = 300;

        public ObservableCollection<SelectInvoiceItemViewModel> InvoiceItems { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private bool _showPaymentTerm;

        [ObservableProperty]
        private string _paymentTermText = string.Empty;

        [ObservableProperty]
        private bool _showPaymentGoal;

        [ObservableProperty]
        private string _paymentGoalText = string.Empty;

        [ObservableProperty]
        private bool _canAddPayment;

        [ObservableProperty]
        private bool _canCreditAccount;

        [ObservableProperty]
        private bool _canPaymentCard;

        [ObservableProperty]
        private bool _showCreditAccount;

        [ObservableProperty]
        private bool _showPaymentCard;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _openBalanceValue = "$0.00";

        [ObservableProperty]
        private string _discountValue = "$0.00";

        [ObservableProperty]
        private string _invoiceValue = "$0.00";

        [ObservableProperty]
        private string _creditValue = "$0.00";

        [ObservableProperty]
        private string _totalValue = "$0.00";

        [ObservableProperty]
        private string _totalBalanceValue = "$0.00";

        public SelectInvoicePageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
            InvoiceItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (SelectInvoiceItemViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(SelectInvoiceItemViewModel.IsSelected))
                            {
                                UpdateTotals();
                            }
                        };
                    }
                }
            };
        }

        public async Task InitializeAsync(int clientId, bool fromClientDetails, bool fromPaymentTab, int orderId)
        {
            if (_initialized && _client?.ClientId == clientId)
                return;

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Client not found.", "Error");
                return;
            }

            _fromClientDetails = fromClientDetails;
            _fromPaymentTab = fromPaymentTab;
            _order = orderId > 0 ? Order.Orders.FirstOrDefault(x => x.OrderId == orderId) : null;

            ClientName = _client.ClientName;

            _term = Term.List.Where(x => x.IsActive).FirstOrDefault(x => x.Id == _client.TermId);
            if (_term != null)
            {
                PaymentTermText = $"Payment Term: {_term.Name}";
                ShowPaymentTerm = true;
            }

            if (Config.ViewGoals && GoalDetailDTO.List.Count > 0)
            {
                var detail = GoalDetailDTO.List.Where(x =>
                    x.Goal != null &&
                    x.ClientId == _client.ClientId &&
                    x.Goal.Criteria == GoalCriteria.Payment &&
                    x.Goal.PendingDays > 0).ToList();

                if (detail.Count > 0)
                {
                    var amount = detail.Sum(x => x.QuantityOrAmountValue);
                    PaymentGoalText = $"Total Payment Goal: {amount.ToCustomString()}";
                    ShowPaymentGoal = true;
                }
            }

            LoadInvoices();
            _initialized = true;
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            LoadInvoices();
            UpdateTotals();
            await Task.CompletedTask;
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Debounce search
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _searchCriteria = value;
                    FilterInvoices();
                });
            }, null, SearchDebounceMs, Timeout.Infinite);
        }

        private void FilterInvoices()
        {
            InvoiceItems.Clear();
            var filtered = _masterList;

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                var searchLower = _searchCriteria.ToLowerInvariant();
                filtered = _masterList.Where(x =>
                    (x.Invoice != null && x.Invoice.InvoiceNumber.ToLowerInvariant().Contains(searchLower)) ||
                    (x.Order != null && x.Order.PrintedOrderId?.ToLowerInvariant().Contains(searchLower) == true)
                ).ToList();
            }

            foreach (var item in filtered)
            {
                InvoiceItems.Add(item);
            }
        }

        private void LoadInvoices()
        {
            InvoiceItems.Clear();
            if (_client == null)
                return;

            _masterList = new List<SelectInvoiceItemViewModel>();

            var invoicesToIterate = Invoice.OpenInvoices.Where(x => x.ClientId == _client.ClientId).ToList();

            if (Config.ShowInvoicesCreditsInPayments)
            {
                // Xamarin logic when ShowInvoicesCreditsInPayments is TRUE (lines 71-178)
                var creditInPayment = new Dictionary<int, List<double>>();

                for (int i = 0; i < invoicesToIterate.Count; i++)
                {
                    // Order invoices by Balance (matches Xamarin line 77)
                    var listOfInvoices = invoicesToIterate.OrderBy(x => x.Balance).ToList();
                    var invoice = listOfInvoices[i];

                    var openAmount = invoice.Balance;

                    // Process payments with credit tracking
                    foreach (var payment in InvoicePayment.List)
                    {
                        if (!string.IsNullOrEmpty(payment.InvoicesId))
                        {
                            var pIdAsString = invoice.InvoiceId.ToString();
                            if (Config.SavePaymentsByInvoiceNumber)
                                pIdAsString = invoice.InvoiceNumber;

                            foreach (var idAsString in payment.InvoicesId.Split(','))
                            {
                                if (pIdAsString == idAsString)
                                {
                                    // Xamarin logic: if openAmount < 0, track in creditInPayment and exclude (lines 91-100)
                                    if (openAmount < 0)
                                    {
                                        if (!creditInPayment.ContainsKey(payment.Id))
                                            creditInPayment.Add(payment.Id, new List<double>() { Math.Abs(openAmount) });
                                        else
                                            creditInPayment[payment.Id].Add(Math.Abs(openAmount));

                                        openAmount = double.MinValue; // Mark as excluded (Xamarin sets t = null)
                                        break;
                                    }

                                    if (payment.TotalPaid >= openAmount)
                                    {
                                        openAmount = double.MinValue; // Mark as excluded (Xamarin sets t = null)
                                        break;
                                    }
                                    else
                                    {
                                        // Xamarin logic: subtract payment and creditMoney (lines 109-113)
                                        double creditMoney = 0;
                                        if (creditInPayment.ContainsKey(payment.Id))
                                            creditMoney = creditInPayment[payment.Id].Sum(x => x);

                                        openAmount -= payment.TotalPaid + creditMoney;
                                    }
                                }
                            }
                        }
                    }

                    // Check temporal payments
                    foreach (var tempPayment in TemporalInvoicePayment.List)
                    {
                        if (tempPayment.invoiceId == invoice.InvoiceId)
                        {
                            if (tempPayment.amountPaid == 0)
                            {
                                openAmount = double.MinValue; // Mark as excluded (Xamarin sets t = null)
                                break;
                            }
                            else
                            {
                                openAmount = tempPayment.amountPaid;
                            }
                        }
                    }

                    // Only add if not excluded and meets criteria (Xamarin lines 137-141)
                    if (openAmount != double.MinValue && (openAmount > 0 || openAmount < 0))
                    {
                    // Determine title based on Xamarin logic: if openAmount < 0, it's "Credit Invoice #", otherwise check InvoiceType
                    // Xamarin: if (line.Open < 0) -> "Credit Invoice #" + InvoiceNumber, else -> "Invoice # " + InvoiceNumber
                    string title;
                    if (openAmount < 0)
                    {
                        title = $"Credit Invoice #{invoice.InvoiceNumber}";
                    }
                    else
                    {
                        // For non-negative amounts, check InvoiceType for other types
                        title = invoice.InvoiceType switch
                        {
                            1 => $"Credit: {invoice.InvoiceNumber}",
                            2 => $"Quote: {invoice.InvoiceNumber}",
                            3 => $"Sales Order: {invoice.InvoiceNumber}",
                            _ => $"Invoice #{invoice.InvoiceNumber}"
                        };
                    }

                    // Calculate discount
                    double discount = 0;
                    bool hasDiscount = false;
                    if (_term != null && Config.UsePaymentDiscount && openAmount > 0)
                    {
                        var daysRemainingForDiscount = Math.Abs((invoice.Date - DateTime.Now.Date).TotalDays);
                        if (_term.StandardDiscountDays >= daysRemainingForDiscount && _term.DiscountPercentage > 0)
                        {
                            discount = openAmount * (_term.DiscountPercentage / 100);
                            hasDiscount = true;
                        }
                    }

                    // Check for goal payment
                    bool hasGoal = false;
                    string goalText = string.Empty;
                    if (Config.ViewGoals && GoalDetailDTO.List.Any(x => 
                        x.Goal != null && 
                        x.Goal.Criteria == GoalCriteria.Payment && 
                        x.ExternalInvoice == invoice && 
                        x.ClientId == invoice.ClientId && 
                        x.Goal.PendingDays > 0))
                    {
                        var detail = GoalDetailDTO.List.FirstOrDefault(x => 
                            x.ExternalInvoice == invoice && 
                            x.ClientId == invoice.ClientId);
                        if (detail != null)
                        {
                            hasGoal = true;
                            goalText = $"Payment Goal: {detail.QuantityOrAmountValue.ToCustomString()}";
                        }
                    }

                    // Check if past due
                    bool isPastDue = invoice.DueDate < DateTime.Today;

                    // Format date text to show both Date and Due Date (matches Xamarin layout: horizontal LinearLayout with both TextViews)
                    string dateText = $"Date:{invoice.Date.ToShortDateString()} Due Date:{invoice.DueDate.ToShortDateString()}";
                    
                    // Format amount text - Xamarin shows line.Open.ToCustomString() directly (line 733), which preserves negative sign
                    // For credits (negative), format with parentheses to match image: "Open:($2.99)"
                    string amountText;
                    if (openAmount < 0)
                    {
                        amountText = $"Open:({Math.Abs(openAmount).ToCustomString()})";
                    }
                    else
                    {
                        amountText = $"Open:{openAmount.ToCustomString()}";
                    }

                    _masterList.Add(new SelectInvoiceItemViewModel
                    {
                        Invoice = invoice,
                        Title = title,
                        DateText = dateText,
                        AmountText = amountText,
                        OpenAmount = openAmount, // Keep sign for credits
                        ShowAmount = !Config.HidePriceInTransaction,
                        DiscountAmount = discount,
                        ShowDiscount = hasDiscount,
                        DiscountText = hasDiscount ? $"Apply for {_term!.DiscountPercentage}% Total: {discount.ToCustomString()}" : string.Empty,
                        ShowGoalPayment = hasGoal,
                        GoalPaymentText = goalText,
                        IsPastDue = isPastDue
                    });
                    }
                }
            }
            else
            {
                // Xamarin logic when ShowInvoicesCreditsInPayments is FALSE (lines 179-274)
                foreach (var invoice in invoicesToIterate)
                {
                    if (invoice.ClientId != _client.ClientId)
                        continue;

                    var openAmount = invoice.Balance;

                    // Subtract payments
                    foreach (var payment in InvoicePayment.List)
                    {
                        if (!string.IsNullOrEmpty(payment.InvoicesId))
                        {
                            var pIdAsString = invoice.InvoiceId.ToString();
                            if (Config.SavePaymentsByInvoiceNumber)
                                pIdAsString = invoice.InvoiceNumber;

                            foreach (var idAsString in payment.InvoicesId.Split(','))
                            {
                                if (pIdAsString == idAsString)
                                {
                                    if (payment.TotalPaid >= openAmount)
                                    {
                                        openAmount = double.MinValue; // Mark as excluded (Xamarin sets t = null)
                                        break;
                                    }
                                    else
                                    {
                                        openAmount -= payment.TotalPaid;

                                        // Xamarin line 208-209: special logic
                                        if (Config.ShowInvoicesCreditsInPayments)
                                            openAmount = 0;
                                    }
                                }
                            }
                        }
                    }

                    // Check temporal payments
                    foreach (var tempPayment in TemporalInvoicePayment.List)
                    {
                        if (tempPayment.invoiceId == invoice.InvoiceId)
                        {
                            if (tempPayment.amountPaid == 0)
                            {
                                openAmount = double.MinValue; // Mark as excluded (Xamarin sets t = null)
                                break;
                            }
                            else
                            {
                                openAmount = tempPayment.amountPaid;
                            }
                        }
                    }

                    // Only add if not excluded and openAmount > 0 (Xamarin line 231)
                    if (openAmount != double.MinValue && openAmount > 0)
                    {
                        // Determine title based on Xamarin logic
                        string title;
                        if (openAmount < 0)
                        {
                            title = $"Credit Invoice #{invoice.InvoiceNumber}";
                        }
                        else
                        {
                            title = invoice.InvoiceType switch
                            {
                                1 => $"Credit: {invoice.InvoiceNumber}",
                                2 => $"Quote: {invoice.InvoiceNumber}",
                                3 => $"Sales Order: {invoice.InvoiceNumber}",
                                _ => $"Invoice #{invoice.InvoiceNumber}"
                            };
                        }

                        // Calculate discount
                        double discount = 0;
                        bool hasDiscount = false;
                        if (_term != null && Config.UsePaymentDiscount && openAmount > 0)
                        {
                            var daysRemainingForDiscount = Math.Abs((invoice.Date - DateTime.Now.Date).TotalDays);
                            if (_term.StandardDiscountDays >= daysRemainingForDiscount && _term.DiscountPercentage > 0)
                            {
                                discount = openAmount * (_term.DiscountPercentage / 100);
                                hasDiscount = true;
                            }
                        }

                        // Check for goal payment
                        bool hasGoal = false;
                        string goalText = string.Empty;
                        if (Config.ViewGoals && GoalDetailDTO.List.Any(x => 
                            x.Goal != null && 
                            x.Goal.Criteria == GoalCriteria.Payment && 
                            x.ExternalInvoice == invoice && 
                            x.ClientId == invoice.ClientId && 
                            x.Goal.PendingDays > 0))
                        {
                            var detail = GoalDetailDTO.List.FirstOrDefault(x => 
                                x.ExternalInvoice == invoice && 
                                x.ClientId == invoice.ClientId);
                            if (detail != null)
                            {
                                hasGoal = true;
                                goalText = $"Payment Goal: {detail.QuantityOrAmountValue.ToCustomString()}";
                            }
                        }

                        // Check if past due
                        bool isPastDue = invoice.DueDate < DateTime.Today;

                        // Format date text to show both Date and Due Date (matches Xamarin layout)
                        string dateText = $"Date:{invoice.Date.ToShortDateString()} Due Date:{invoice.DueDate.ToShortDateString()}";
                        
                        // Format amount text - for credits (negative), format with parentheses
                        string amountText;
                        if (openAmount < 0)
                        {
                            amountText = $"Open:({Math.Abs(openAmount).ToCustomString()})";
                        }
                        else
                        {
                            amountText = $"Open:{openAmount.ToCustomString()}";
                        }

                        _masterList.Add(new SelectInvoiceItemViewModel
                        {
                            Invoice = invoice,
                            Title = title,
                            DateText = dateText,
                            AmountText = amountText,
                            OpenAmount = openAmount,
                            ShowAmount = !Config.HidePriceInTransaction,
                            DiscountAmount = discount,
                            ShowDiscount = hasDiscount,
                            DiscountText = hasDiscount ? $"Apply for {_term!.DiscountPercentage}% Total: {discount.ToCustomString()}" : string.Empty,
                            ShowGoalPayment = hasGoal,
                            GoalPaymentText = goalText,
                            IsPastDue = isPastDue
                        });
                    }

                    // Xamarin line 234: also add if ShowInvoicesCreditsInPayments and openAmount < 0
                    if (Config.ShowInvoicesCreditsInPayments && openAmount != double.MinValue && openAmount < 0)
                    {
                        // Same logic as above but for negative amounts
                        string title = $"Credit Invoice #{invoice.InvoiceNumber}";
                        bool isPastDue = invoice.DueDate < DateTime.Today;
                        
                        // Format date text to show both Date and Due Date (matches Xamarin layout)
                        string dateText = $"Date:{invoice.Date.ToShortDateString()} Due Date:{invoice.DueDate.ToShortDateString()}";
                        
                        // Format amount text with parentheses for credits
                        string amountText = $"Open:({Math.Abs(openAmount).ToCustomString()})";

                        _masterList.Add(new SelectInvoiceItemViewModel
                        {
                            Invoice = invoice,
                            Title = title,
                            DateText = dateText,
                            AmountText = amountText,
                            OpenAmount = openAmount,
                            ShowAmount = !Config.HidePriceInTransaction,
                            DiscountAmount = 0,
                            ShowDiscount = false,
                            DiscountText = string.Empty,
                            ShowGoalPayment = false,
                            GoalPaymentText = string.Empty,
                            IsPastDue = isPastDue
                        });
                    }
                }
            }

            // Add finished orders
            foreach (var order in Order.Orders.Where(x => x.Finished && !x.Voided && x.Client.ClientId == _client.ClientId).ToList())
            {
                var openAmount = order.OrderTotalCost();

                foreach (var payment in InvoicePayment.List)
                {
                    if (payment.Orders().Any(o => o.OrderId == order.OrderId))
                    {
                        if (payment.TotalPaid >= openAmount)
                        {
                            openAmount = 0;
                            break;
                        }
                        else
                        {
                            openAmount -= payment.TotalPaid;
                        }
                    }
                }

                if (openAmount > 0)
                {
                    bool isPastDue = order.DueDate < DateTime.Today;
                    
                    // Format date text to show both Date and Due Date (matches Xamarin: lines 679-680)
                    string dateText = $"Date:{order.Date.ToShortDateString()} Due Date:{order.DueDate.ToShortDateString()}";

                    _masterList.Add(new SelectInvoiceItemViewModel
                    {
                        Order = order,
                        Title = $"Order: {order.PrintedOrderId ?? order.OrderId.ToString()}",
                        DateText = dateText,
                        AmountText = $"Open:{openAmount.ToCustomString()}",
                        OpenAmount = openAmount,
                        ShowAmount = !Config.HidePriceInTransaction,
                        IsPastDue = isPastDue
                    });
                }
            }

            // Sort by due date or order date
            var sortedList = Config.ShowInvoicesCreditsInPayments
                ? _masterList.OrderByDescending(x => x.Invoice?.Date ?? x.Order?.Date ?? DateTime.MinValue).ToList()
                : _masterList.OrderBy(x => x.Invoice?.DueDate ?? x.Order?.Date ?? DateTime.MaxValue).ToList();

            _masterList = sortedList;
            FilterInvoices();

            // Add Payment (and related actions) disabled until at least one invoice is selected (UpdateTotals sets them when selection changes)
            var hasSelection = InvoiceItems.Any(x => x.IsSelected);
            CanAddPayment = hasSelection;
            CanCreditAccount = hasSelection;
            CanPaymentCard = hasSelection;
            ShowCreditAccount = Config.UseCreditAccount && !Config.HidePriceInTransaction;
            ShowPaymentCard = false;
            
            UpdateSummaryTotals();
        }
        
        private void UpdateSummaryTotals()
        {
            // Calculate totals from all items (not just selected)
            var allTotal = _masterList.Sum(x => x.OpenAmount);
            
            // For selected items
            var selected = InvoiceItems.Where(x => x.IsSelected).ToList();
            var selectedOpenAmount = selected.Where(x => x.OpenAmount > 0).Sum(x => x.OpenAmount);
            var selectedCreditAmount = selected.Where(x => x.OpenAmount < 0).Sum(x => x.OpenAmount);
            var selectedTotal = selected.Sum(x => x.OpenAmount);
            
            // Calculate discount for selected items only (matching Xamarin logic)
            double selectedDiscountTotal = 0;
            if (_term != null && Config.UsePaymentDiscount)
            {
                foreach (var item in selected.Where(x => x.Invoice != null && x.OpenAmount > 0))
                {
                    selectedDiscountTotal += item.DiscountAmount;
                }
            }
            
            // Open Balance = Client Balance - Selected Total (matching Xamarin: openBalance.Text = (balance - total).ToCustomString())
            var clientBalance = _client != null ? _client.ClientBalanceInDevice : 0;
            OpenBalanceValue = (clientBalance - selectedTotal).ToCustomString();
            
            // Discount = discount for selected items (matching Xamarin: paymentDiscount.Text = discountTotal.ToCustomString())
            DiscountValue = selectedDiscountTotal.ToCustomString();
            
            // Invoice = sum of selected items where OpenAmount > 0 (matching Xamarin: invoiceTotal.Text = openAmount.ToCustomString())
            InvoiceValue = selectedOpenAmount.ToCustomString();
            
            // Credit = sum of selected items where OpenAmount < 0 (NEGATIVE value, no Math.Abs - matching Xamarin: creditTotal.Text = creditAmount.ToCustomString())
            CreditValue = selectedCreditAmount.ToCustomString();
            
            // Total = sum of all selected items (matching Xamarin: TotalLabel.Text = total.ToCustomString())
            TotalValue = selectedTotal.ToCustomString();
            
            // Total Balance = GrandTotal = all items total - selected total (matching Xamarin: balanceTotal.Text = GrandTotal.ToCustomString() where GrandTotal = masterList.Sum(x => x.Open) - total)
            var grandTotal = allTotal - selectedTotal;
            TotalBalanceValue = grandTotal.ToCustomString();
        }

        private void UpdateTotals()
        {
            var selected = InvoiceItems.Where(x => x.IsSelected).ToList();
            var openAmount = selected.Where(x => x.OpenAmount > 0).Sum(x => x.OpenAmount);
            var creditAmount = selected.Where(x => x.OpenAmount < 0).Sum(x => x.OpenAmount);
            var total = selected.Sum(x => x.OpenAmount);
            
            // Calculate discount for selected items
            double discountTotal = 0;
            if (_term != null && Config.UsePaymentDiscount)
            {
                foreach (var item in selected.Where(x => x.Invoice != null && x.OpenAmount > 0))
                {
                    discountTotal += item.DiscountAmount;
                }
            }

            // Match Xamarin: Button is enabled if items are selected, error message shown on click if total < 0
            CanAddPayment = selected.Count > 0;
            CanCreditAccount = CanAddPayment;
            CanPaymentCard = CanAddPayment;
            
            UpdateSummaryTotals();
        }

        [RelayCommand]
        private async Task AddPaymentAsync()
        {
            var selectedItems = InvoiceItems.Where(x => x.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select at least one invoice.", "Alert");
                return;
            }

            var total = selectedItems.Sum(x => x.OpenAmount);
            if (total < 0)
            {
                await _dialogService.ShowAlertAsync("The amount of the received payment must be over $0.", "Alert");
                return;
            }

            // Check for multiple selection blocking
            if (Config.BlockMultipleCollectPaymets && selectedItems.Count > 1)
            {
                await _dialogService.ShowAlertAsync("Only one invoice per payment is allowed.", "Alert");
                return;
            }

            var invoiceIdStrings = selectedItems.Where(x => x.Invoice != null)
                .Select(x => Config.SavePaymentsByInvoiceNumber ? x.Invoice!.InvoiceNumber : x.Invoice!.InvoiceId.ToString())
                .ToList();
            var orderIds = selectedItems.Where(x => x.Order != null).Select(x => x.Order!.OrderId).ToList();

            var invoiceIdsParam = invoiceIdStrings.Count > 0 ? string.Join(",", invoiceIdStrings) : string.Empty;
            var orderIdsParam = orderIds.Count > 0 ? string.Join(",", orderIds) : string.Empty;

            await Shell.Current.GoToAsync($"paymentsetvalues?clientId={_client.ClientId}&invoiceIds={invoiceIdsParam}&orderIds={orderIdsParam}");
        }

        [RelayCommand]
        private async Task CreditAccountAsync()
        {
            await _dialogService.ShowAlertAsync("Credit account is not yet implemented in the MAUI version.", "Info");
        }

        [RelayCommand]
        private async Task PaymentCardAsync()
        {
            await _dialogService.ShowAlertAsync("Payment card is not yet implemented in the MAUI version.", "Info");
        }
    }

    public partial class SelectInvoiceItemViewModel : ObservableObject
    {
        private bool _isUpdatingSelection = false;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _dateText = string.Empty;

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        private double _openAmount;

        [ObservableProperty]
        private bool _showAmount = true;

        [ObservableProperty]
        private double _discountAmount;

        [ObservableProperty]
        private bool _showDiscount;

        [ObservableProperty]
        private string _discountText = string.Empty;

        [ObservableProperty]
        private bool _showGoalPayment;

        [ObservableProperty]
        private string _goalPaymentText = string.Empty;

        [ObservableProperty]
        private bool _isPastDue;

        [ObservableProperty]
        private Color _textColor = Colors.Black;

        public Invoice? Invoice { get; set; }
        public Order? Order { get; set; }

        partial void OnIsSelectedChanged(bool value)
        {
            // Selection blocking is handled in the parent ViewModel's AddPaymentAsync
        }

        partial void OnIsPastDueChanged(bool value)
        {
            if (value)
                TextColor = Colors.Red;
            else
                TextColor = Colors.Black;
        }
    }
}

