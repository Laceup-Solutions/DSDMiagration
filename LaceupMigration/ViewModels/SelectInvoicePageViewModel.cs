using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

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
        private string _totalSelectedText = "Total Selected: $0.00";

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

            var term = Term.List.Where(x => x.IsActive).FirstOrDefault(x => x.Id == _client.TermId);
            if (term != null)
            {
                PaymentTermText = $"Payment Term: {term.Name}";
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

        private void LoadInvoices()
        {
            InvoiceItems.Clear();
            if (_client == null)
                return;

            var masterList = new List<SelectInvoiceItemViewModel>();

            var invoicesToIterate = Invoice.OpenInvoices.Where(x => x.ClientId == _client.ClientId).ToList();

            foreach (var invoice in invoicesToIterate)
            {
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
                                    openAmount = 0;
                                    break;
                                }
                                else
                                {
                                    openAmount -= payment.TotalPaid;
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
                            openAmount = 0;
                            break;
                        }
                        else
                        {
                            openAmount = tempPayment.amountPaid;
                        }
                    }
                }

                if (openAmount > 0 || (Config.ShowInvoicesCreditsInPayments && openAmount < 0))
                {
                    var title = invoice.InvoiceType switch
                    {
                        1 => $"Credit: {invoice.InvoiceNumber}",
                        2 => $"Quote: {invoice.InvoiceNumber}",
                        3 => $"Sales Order: {invoice.InvoiceNumber}",
                        _ => $"Invoice: {invoice.InvoiceNumber}"
                    };

                    masterList.Add(new SelectInvoiceItemViewModel
                    {
                        Invoice = invoice,
                        Title = title,
                        DateText = $"Due: {invoice.DueDate.ToShortDateString()}",
                        AmountText = $"Open: {Math.Abs(openAmount).ToCustomString()}",
                        OpenAmount = Math.Abs(openAmount),
                        ShowAmount = !Config.HidePriceInTransaction
                    });
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
                    masterList.Add(new SelectInvoiceItemViewModel
                    {
                        Order = order,
                        Title = $"Order: {order.PrintedOrderId ?? order.OrderId.ToString()}",
                        DateText = $"Date: {order.Date.ToShortDateString()}",
                        AmountText = $"Open: {openAmount.ToCustomString()}",
                        OpenAmount = openAmount,
                        ShowAmount = !Config.HidePriceInTransaction
                    });
                }
            }

            // Sort by due date or order date
            var sortedList = Config.ShowInvoicesCreditsInPayments
                ? masterList.OrderByDescending(x => x.Invoice?.Date ?? x.Order?.Date ?? DateTime.MinValue).ToList()
                : masterList.OrderBy(x => x.Invoice?.DueDate ?? x.Order?.Date ?? DateTime.MaxValue).ToList();

            foreach (var item in sortedList)
            {
                InvoiceItems.Add(item);
            }

            CanAddPayment = InvoiceItems.Count > 0;
            ShowCreditAccount = Config.UseCreditAccount && !Config.HidePriceInTransaction;
            ShowPaymentCard = false;
        }

        private void UpdateTotals()
        {
            var total = InvoiceItems.Where(x => x.IsSelected).Sum(x => x.OpenAmount);
            TotalSelectedText = $"Total Selected: {total.ToCustomString()}";
            CanAddPayment = InvoiceItems.Any(x => x.IsSelected);
            CanCreditAccount = CanAddPayment;
            CanPaymentCard = CanAddPayment;
        }

        [RelayCommand]
        private async Task AddPaymentAsync()
        {
            var selectedItems = InvoiceItems.Where(x => x.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select at least one invoice or order.", "Alert");
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

        public Invoice? Invoice { get; set; }
        public Order? Order { get; set; }
    }
}

