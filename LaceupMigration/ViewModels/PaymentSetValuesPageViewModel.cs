using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;

namespace LaceupMigration.ViewModels
{
    public partial class PaymentSetValuesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Client? _client;
        private InvoicePayment? _invoicePayment;

        /// <summary>Gets whether the current payment is printed (for use by PaymentComponentViewModel, matches Xamarin line 1787).</summary>
        internal bool GetIsPaymentPrinted() => _invoicePayment?.Printed ?? false;
        private List<Invoice> _invoices = new();
        private List<Order> _orders = new();
        private double _amount = 0;
        private double _creditAmount = 0;
        private double _totalDiscount = 0;
        private bool _creditAccount = false;
        private bool _fromPaymentTab = false;
        private bool _fromClientDetails = false;
        private bool _fromFinalize = false;
        private bool _goBackToMain = false;
        private int _paymentId = 0;
        private string _invoicesId = string.Empty;
        private string _ordersId = string.Empty;
        private string _tempFile = string.Empty;
        private bool _initializationComplete = false;
        private bool _hasInitialized = false;
        /// <summary>True when user has made changes that are not yet saved (prompt on back).</summary>
        private bool _hasUnsavedChanges = false;

        [ObservableProperty]
        private ObservableCollection<PaymentComponentViewModel> _paymentComponents = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _docNumbers = string.Empty;

        [ObservableProperty]
        private string _creditsDocNumbers = string.Empty;

        [ObservableProperty]
        private string _amountLabel = string.Empty;

        [ObservableProperty]
        private string _openLabel = string.Empty;

        [ObservableProperty]
        private Color _openLabelColor = Colors.Black;

        [ObservableProperty]
        private string _paidLabel = string.Empty;

        [ObservableProperty]
        private string _creditAmountLabel = string.Empty;

        [ObservableProperty]
        private string _discountTotal = string.Empty;

        [ObservableProperty]
        private bool _showCreditAmount;

        [ObservableProperty]
        private bool _showCreditsDocNumbers;

        [ObservableProperty]
        private bool _showDiscount;

        [ObservableProperty]
        private bool _canAddPayment = true;

        [ObservableProperty]
        private bool _canSavePayment = true;

        [ObservableProperty]
        private bool _canPrintPayment = true;

        [ObservableProperty]
        private bool _canDeletePayment = true;

        [ObservableProperty]
        private bool _isCreditAccount;

        public PaymentSetValuesPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _tempFile = Path.Combine(Config.DataPath, "paymentActivity.xml");
        }

        public async Task OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query == null)
                return;
            
            // Prevent re-initialization after first initialization
            if (_hasInitialized)
            {
                System.Diagnostics.Debug.WriteLine("OnNavigatedTo: Already initialized, skipping re-initialization");
                return;
            }

            // Parse paymentId
            if (query.TryGetValue("paymentId", out var paymentIdValue) && paymentIdValue != null)
            {
                if (int.TryParse(paymentIdValue.ToString(), out var paymentId))
                {
                    _paymentId = paymentId;
                }
            }

            // Parse invoiceIds
            if (query.TryGetValue("invoiceIds", out var invoiceIdsValue) && invoiceIdsValue != null)
            {
                _invoicesId = invoiceIdsValue.ToString() ?? string.Empty;
            }

            // Parse orderIds (SelectInvoice etc.) or ordersId (FinalizeBatch - matches Xamarin ordersIdIntent)
            // Decode URL-encoded values (e.g. 3|4 -> 3%7C4, 3,4 -> 3%2C4) so parsing works
            if (query.TryGetValue("orderIds", out var orderIdsValue) && orderIdsValue != null)
            {
                var raw = orderIdsValue.ToString() ?? string.Empty;
                _ordersId = string.IsNullOrEmpty(raw) ? raw : Uri.UnescapeDataString(raw);
            }
            if (query.TryGetValue("ordersId", out var ordersIdValue) && ordersIdValue != null)
            {
                var raw = ordersIdValue.ToString() ?? string.Empty;
                _ordersId = string.IsNullOrEmpty(raw) ? raw : Uri.UnescapeDataString(raw);
            }

            // Parse clientId
            if (query.TryGetValue("clientId", out var clientIdValue) && clientIdValue != null)
            {
                if (int.TryParse(clientIdValue.ToString(), out var clientId))
                {
                    _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
                }
            }

            // Parse goBackToMain (can be "1", "true", or boolean)
            if (query.TryGetValue("goBackToMain", out var goBackValue) && goBackValue != null)
            {
                var goBackStr = goBackValue.ToString();
                _goBackToMain = goBackStr == "1" || goBackStr?.ToLowerInvariant() == "true" || 
                               (goBackValue is bool goBackBool && goBackBool);
            }

            // Parse fromPaymentTab
            if (query.TryGetValue("fromPaymentTab", out var fromPaymentTabValue) && fromPaymentTabValue != null)
            {
                var fromPaymentTabStr = fromPaymentTabValue.ToString();
                _fromPaymentTab = fromPaymentTabStr == "1" || fromPaymentTabStr?.ToLowerInvariant() == "true" || 
                                 (fromPaymentTabValue is bool fromPaymentTabBool && fromPaymentTabBool);
            }

            // Parse fromClientDetails
            if (query.TryGetValue("fromClientDetails", out var fromClientDetailsValue) && fromClientDetailsValue != null)
            {
                var fromClientDetailsStr = fromClientDetailsValue.ToString();
                _fromClientDetails = fromClientDetailsStr == "1" || fromClientDetailsStr?.ToLowerInvariant() == "true" || 
                                    (fromClientDetailsValue is bool fromClientDetailsBool && fromClientDetailsBool);
            }

            // Parse fromFinalize (FinalizeBatch Collect Payment - matches Xamarin commingFromFinalizeIntent)
            if (query.TryGetValue("fromFinalize", out var fromFinalizeValue) && fromFinalizeValue != null)
            {
                var fromFinalizeStr = fromFinalizeValue.ToString();
                _fromFinalize = fromFinalizeStr == "1" || fromFinalizeStr?.ToLowerInvariant() == "true" || 
                               (fromFinalizeValue is bool fromFinalizeBool && fromFinalizeBool);
            }

            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _initializationComplete = false;
            if (_paymentId > 0)
            {
                _invoicePayment = InvoicePayment.List.FirstOrDefault(x => x.Id == _paymentId);
                if (_invoicePayment == null)
                {
                    await _dialogService.ShowAlertAsync($"Payment with ID {_paymentId} not found in list of {InvoicePayment.List.Count} payments.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                _client = _invoicePayment.Client;
                if (_client == null)
                {
                    await _dialogService.ShowAlertAsync("Payment has no associated client.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                _invoicesId = _invoicePayment.InvoicesId ?? string.Empty;
                _ordersId = _invoicePayment.OrderId ?? string.Empty;

                // Set client name first
                ClientName = _client.ClientName;
                ShowCreditAmount = Config.ShowInvoicesCreditsInPayments;

                // Load invoice/order data and calculate amounts BEFORE loading components
                LoadExistingPaymentData();

                // Load existing payment components
                PaymentComponents.Clear();
                foreach (var component in _invoicePayment.Components)
                {
                    var componentVm = new PaymentComponentViewModel(component, this);
                    // Ensure IsEditable is set correctly based on printed status (matches Xamarin line 1787)
                    componentVm.IsEditable = !_invoicePayment.Printed;
                    PaymentComponents.Add(componentVm);
                }

                if (_invoicePayment.Printed)
                {
                    CanAddPayment = false;
                    CanSavePayment = false;
                    CanDeletePayment = false;
                }
                else
                {
                    // Delete button is enabled by default (matches Xamarin - only disabled when printed)
                    // The delete command will check if payment exists before deleting
                    CanDeletePayment = true;
                }
            }
            else if (!string.IsNullOrEmpty(_invoicesId) || !string.IsNullOrEmpty(_ordersId))
            {
                LoadInvoicesAndOrders();
            }
            else if (_client != null)
            {
                _creditAccount = true;
                IsCreditAccount = true;
            }

            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Client not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Ensure ClientName is set
            if (string.IsNullOrEmpty(ClientName) && _client != null)
            {
                ClientName = _client.ClientName;
            }

            ShowCreditAmount = Config.ShowInvoicesCreditsInPayments;

            // [ACTIVITY STATE]: Check if restoring from state and load saved payment components
            // Match Xamarin PaymentSetValuesActivity: loads temp file path from ActivityState.State
            var state = LaceupMigration.ActivityState.GetState("PaymentSetValuesActivity");
            if (state != null && state.State != null && state.State.ContainsKey("tempFilePath"))
            {
                var savedTempFilePath = state.State["tempFilePath"];
                if (!string.IsNullOrEmpty(savedTempFilePath) && File.Exists(savedTempFilePath))
                {
                    // Use the saved temp file path from ActivityState
                    _tempFile = savedTempFilePath;
                    // Load state from temp file
                    LoadState();
                }
            }

            if (PaymentComponents.Count == 0 && _amount > 0 && _paymentId == 0)
            {
                PaymentComponents.Add(new PaymentComponentViewModel(new PaymentComponent { Amount = _amount }, this));
                MarkUnsavedChanges(); // New payment with default component has unsaved changes
            }

            // Set CanDeletePayment only if not already set above (for existing payments)
            // Match Xamarin: Delete button is enabled by default, only disabled when printed
            // For new payments or when _invoicePayment is null, enable delete button
            // The delete command will check if payment exists before actually deleting
            if (_paymentId == 0 || (_paymentId > 0 && _invoicePayment == null))
            {
                // Enable delete button by default (matches Xamarin behavior)
                // Delete command handles the case where payment doesn't exist yet
                CanDeletePayment = true;
            }
            // If _paymentId > 0 and _invoicePayment exists, CanDeletePayment was already set above based on Printed status

            RefreshLabels();
            _initializationComplete = true;
            _hasInitialized = true;
        }

        private void LoadExistingPaymentData()
        {
            if (_invoicePayment == null)
                return;

            _invoices.Clear();
            _orders.Clear();
            _amount = 0;
            _creditAmount = 0;
            _totalDiscount = _invoicePayment.DiscountApplied;

            var docNumbersList = new List<string>();
            var creditsDocNumbersList = new List<string>();

            // Use the payment's own Invoices() method to get invoices
            var paymentInvoices = _invoicePayment.Invoices();
            foreach (var invoice in paymentInvoices)
            {
                // Calculate already paid by other payments (excluding current payment)
                var idAsString = Config.SavePaymentsByInvoiceNumber 
                    ? invoice.InvoiceNumber 
                    : invoice.InvoiceId.ToString();

                var paymentsForInvoice = InvoicePayment.List.Where(x => 
                    x.InvoicesId != null && 
                    x.InvoicesId.Contains(idAsString) && 
                    x.Id != _paymentId);
                double alreadyPaidByOthers = 0;

                foreach (var payment in paymentsForInvoice)
                {
                    foreach (var i in payment.Invoices())
                    {
                        bool matches = Config.SavePaymentsByInvoiceNumber 
                            ? i.InvoiceNumber == idAsString 
                            : i.InvoiceId == invoice.InvoiceId;

                        if (matches)
                        {
                            double creditApplied = 0;
                            var creditsInPayment = payment.Invoices().Where(x => x.Balance < 0).ToList();
                            if (creditsInPayment.Count > 0)
                                creditApplied = Math.Abs(creditsInPayment.Sum(x => x.Paid));

                            foreach (var component in payment.Components)
                            {
                                if (invoice.Balance == 0)
                                    continue;

                                double usedInThisInvoice = component.Amount;
                                if (invoice.Balance < 0)
                                    usedInThisInvoice = invoice.Balance;
                                else if (component.Amount > invoice.Balance)
                                    usedInThisInvoice = invoice.Balance;

                                alreadyPaidByOthers += usedInThisInvoice + creditApplied;
                            }
                        }
                    }
                }

                // Match Xamarin logic exactly:
                // Xamarin: amount += invoice.Balance, then amount -= alreadyPaid
                // This calculates: amount = current balance - already paid by others
                // The current balance already excludes this payment, so we get what was owed before this payment
                // Then later: amount -= totalDiscount
                // So final amount = current balance - already paid by others - discount
                var originalInvoiceBalance = invoice.Balance - alreadyPaidByOthers;

                if (Config.ShowInvoicesCreditsInPayments)
                {
                    if (originalInvoiceBalance < 0)
                    {
                        _creditAmount += originalInvoiceBalance;
                        creditsDocNumbersList.Add(invoice.InvoiceNumber);
                    }
                    else
                    {
                        _amount += originalInvoiceBalance;
                        docNumbersList.Add(invoice.InvoiceNumber);
                    }
                }
                else
                {
                    _amount += originalInvoiceBalance;
                    docNumbersList.Add(invoice.InvoiceNumber);
                }

                _invoices.Add(invoice);
            }

            // Use the payment's own Orders() method to get orders
            var paymentOrders = _invoicePayment.Orders();
            foreach (var order in paymentOrders)
            {
                // Calculate already paid by other payments for this order
                var paymentsForOrder = InvoicePayment.List.Where(x => 
                    x.OrderId != null && 
                    x.OrderId.Contains(order.UniqueId) && 
                    x.Id != _paymentId);
                double alreadyPaidByOthers = 0;
                foreach (var payment in paymentsForOrder)
                {
                    if (payment.Orders().Any(o => o.UniqueId == order.UniqueId))
                    {
                        alreadyPaidByOthers += payment.TotalPaid;
                    }
                }

                // Match Xamarin logic: amount = current balance - already paid by others
                var originalOrderAmount = order.OrderTotalCost() - alreadyPaidByOthers;

                if (Config.ShowInvoicesCreditsInPayments)
                {
                    if (originalOrderAmount < 0)
                    {
                        _creditAmount += originalOrderAmount;
                        if (!string.IsNullOrEmpty(order.PrintedOrderId))
                            creditsDocNumbersList.Add(order.PrintedOrderId);
                    }
                    else
                    {
                        _amount += originalOrderAmount;
                        if (!string.IsNullOrEmpty(order.PrintedOrderId))
                            docNumbersList.Add(order.PrintedOrderId);
                    }
                }
                else
                {
                    _amount += originalOrderAmount;
                    if (!string.IsNullOrEmpty(order.PrintedOrderId))
                        docNumbersList.Add(order.PrintedOrderId);
                }

                _orders.Add(order);
            }

            DocNumbers = string.Join(", ", docNumbersList);
            CreditsDocNumbers = string.Join(", ", creditsDocNumbersList);
            ShowCreditsDocNumbers = creditsDocNumbersList.Count > 0;

            if (Config.ShowInvoicesCreditsInPayments)
                _amount += _creditAmount;

            // Apply discount (Xamarin does this after calculating amounts)
            // Note: _totalDiscount was already set from _invoicePayment.DiscountApplied at the start
            // We don't recalculate discount for existing payments, we use the stored value
            // Xamarin: amount -= totalDiscount (line 297)
            _amount -= _totalDiscount;

            // Check if credit account
            if (string.IsNullOrEmpty(_invoicesId) && string.IsNullOrEmpty(_ordersId))
            {
                _creditAccount = true;
                IsCreditAccount = true;
            }
        }

        private void LoadInvoicesAndOrders()
        {
            _invoices.Clear();
            _orders.Clear();
            _amount = 0;
            _creditAmount = 0;
            _totalDiscount = 0;

            var docNumbersList = new List<string>();
            var creditsDocNumbersList = new List<string>();

            // Load invoices
            if (!string.IsNullOrEmpty(_invoicesId))
            {
                foreach (var idAsString in _invoicesId.Split(','))
                {
                    Invoice? invoice = null;
                    if (Config.SavePaymentsByInvoiceNumber)
                    {
                        invoice = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceNumber == idAsString);
                    }
                    else
                    {
                        var id = Convert.ToInt32(idAsString);
                        invoice = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceId == id);
                    }

                    if (invoice != null)
                    {
                        var paymentsForInvoice = InvoicePayment.List.Where(x => x.InvoicesId != null && x.InvoicesId.Contains(idAsString));
                        double alreadyPaid = 0;

                        foreach (var payment in paymentsForInvoice)
                        {
                            foreach (var i in payment.Invoices())
                            {
                                bool matches = Config.SavePaymentsByInvoiceNumber 
                                    ? i.InvoiceNumber == idAsString 
                                    : i.InvoiceId.ToString() == idAsString;

                                if (matches)
                                {
                                    double creditApplied = 0;
                                    var creditsInPayment = payment.Invoices().Where(x => x.Balance < 0).ToList();
                                    if (creditsInPayment.Count > 0)
                                        creditApplied = Math.Abs(creditsInPayment.Sum(x => x.Paid));

                                    foreach (var component in payment.Components)
                                    {
                                        if (invoice.Balance == 0)
                                            continue;

                                        double usedInThisInvoice = component.Amount;
                                        if (invoice.Balance < 0)
                                            usedInThisInvoice = invoice.Balance;
                                        else if (component.Amount > invoice.Balance)
                                            usedInThisInvoice = invoice.Balance;

                                        alreadyPaid += usedInThisInvoice + creditApplied;
                                    }
                                }
                            }
                        }

                        if (Config.ShowInvoicesCreditsInPayments)
                        {
                            if (invoice.Balance < 0)
                            {
                                _creditAmount += invoice.Balance;
                                creditsDocNumbersList.Add(invoice.InvoiceNumber);
                            }
                            else
                            {
                                _amount += invoice.Balance;
                                docNumbersList.Add(invoice.InvoiceNumber);
                            }
                        }
                        else
                        {
                            _amount += invoice.Balance;
                            docNumbersList.Add(invoice.InvoiceNumber);
                        }

                        double discount = 0;
                        if (invoice.Balance > 0)
                        {
                            discount = CalculateTotalDiscount(invoice, invoice.Balance);
                            _totalDiscount += discount;
                        }

                        _amount -= (alreadyPaid + discount);
                        _invoices.Add(invoice);
                    }
                }
            }

            // Load orders (matches Xamarin PaymentSetValuesActivity when !string.IsNullOrEmpty(ordersId) && string.IsNullOrEmpty(invoicesId))
            if (!string.IsNullOrEmpty(_ordersId))
            {
                foreach (var idAsString in _ordersId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = idAsString.Trim();
                    if (string.IsNullOrEmpty(trimmed) || !int.TryParse(trimmed, out var id))
                        continue;
                    var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
                    if (order != null)
                    {
                        double orderTotal = Config.DisolCrap ? order.DisolOrderTotalCost() : order.OrderTotalCost();
                        if (Config.ShowInvoicesCreditsInPayments)
                        {
                            if (orderTotal < 0)
                            {
                                _creditAmount += orderTotal;
                                if (!string.IsNullOrEmpty(order.PrintedOrderId))
                                    creditsDocNumbersList.Add(order.PrintedOrderId);
                            }
                            else
                            {
                                _amount += orderTotal;
                                if (!string.IsNullOrEmpty(order.PrintedOrderId))
                                    docNumbersList.Add(order.PrintedOrderId);
                            }
                        }
                        else
                        {
                            _amount += orderTotal;
                            if (!string.IsNullOrEmpty(order.PrintedOrderId))
                                docNumbersList.Add(order.PrintedOrderId);
                        }

                        _orders.Add(order);
                    }
                }
            }

            DocNumbers = string.Join(", ", docNumbersList);
            CreditsDocNumbers = string.Join(", ", creditsDocNumbersList);
            ShowCreditsDocNumbers = creditsDocNumbersList.Count > 0;

            if (Config.ShowInvoicesCreditsInPayments)
                _amount += _creditAmount;
        }

        private double CalculateTotalDiscount(Invoice invoice, double open)
        {
            try
            {
                if (!Config.UsePaymentDiscount)
                    return 0;

                var term = Term.List.Where(x => x.IsActive).FirstOrDefault(x => x.Id == invoice.Client.TermId);
                if (term == null)
                    return 0;

                var daysRemainingForDiscount = Math.Abs((invoice.Date - DateTime.Now.Date).TotalDays);
                if (term.StandardDiscountDays >= daysRemainingForDiscount)
                    return open * (term.DiscountPercentage / 100);

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private void RefreshLabels()
        {
            var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
            var open = _amount - currentPaymentAmount;

            AmountLabel = (_amount + _totalDiscount).ToCustomString();
            OpenLabel = open.ToCustomString();
            // Red when open > 0, green when paid ($0)
            if (open == 0 || OpenLabel == "$0" || OpenLabel == "$0.00")
                OpenLabelColor = Colors.Green;
            else
                OpenLabelColor = Colors.Red;
            PaidLabel = currentPaymentAmount.ToCustomString();

            if (ShowCreditAmount)
                CreditAmountLabel = _creditAmount.ToCustomString();

            DiscountTotal = _totalDiscount.ToCustomString();
            ShowDiscount = _totalDiscount > 0;

            // Disable Add Payment button if:
            // 1. Payment is already printed (can't modify)
            // 2. There's no open amount left to collect (unless it's a credit account)
            // Add button is enabled as long as Open amount > 0 (mirror PaymentSetvaluesActivity)
            CanAddPayment = _creditAccount || ((_invoicePayment == null || !_invoicePayment.Printed) && open > 0);
        }

        [RelayCommand]
        private async Task AddPayment()
        {
            var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
            var open = _amount - currentPaymentAmount;

            if (PaymentComponents.Any(x => x.Amount == 0))
            {
                await _dialogService.ShowAlertAsync("Please provide amount for empty payment.", "Alert", "OK");
                return;
            }

            var qty = open;
            if (_creditAccount)
                qty = 0;

            PaymentComponents.Add(new PaymentComponentViewModel(new PaymentComponent { Amount = qty }, this));
            MarkUnsavedChanges();
            RefreshLabels();
            SaveState(); // Save state after adding payment
        }

        /// <summary>Payment method options for picker (matches Xamarin GetInvoicePaymentMethodNames: no ACH).</summary>
        private static string[] GetPaymentMethodOptions()
        {
            var result = new List<string>
            {
                InvoicePaymentMethod.Cash.ToString().Replace("_", " "),
                InvoicePaymentMethod.Check.ToString().Replace("_", " ")
            };
            if (Config.ExtendedPaymentOptions)
            {
                result.Add(InvoicePaymentMethod.Credit_Card.ToString().Replace("_", " "));
                result.Add(InvoicePaymentMethod.Money_Order.ToString().Replace("_", " "));
                result.Add(InvoicePaymentMethod.Transfer.ToString().Replace("_", " "));
                result.Add(InvoicePaymentMethod.Zelle_Transfer.ToString().Replace("_", " "));
            }
            return result.ToArray();
        }

        /// <summary>Edit only payment method (matches Xamarin spinner selection).</summary>
        public async Task EditPaymentMethodAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var options = GetPaymentMethodOptions();
            var choice = await _dialogService.ShowActionSheetAsync("Select Payment Method", "Cancel", null, options);
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
            var method = Enum.Parse<InvoicePaymentMethod>(choice.Replace(" ", "_"));
            component.PaymentMethod = method;
            if (method == InvoicePaymentMethod.Check && component.PostedDate == DateTime.MinValue)
                component.PostedDate = DateTime.Now.Date;
            else if (method != InvoicePaymentMethod.Check)
                component.PostedDate = DateTime.MinValue;
            MarkUnsavedChanges();
            RefreshLabels();
            SaveState();
        }

        /// <summary>Edit only amount (matches Xamarin amountButton_Click).</summary>
        public async Task EditAmountAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var amountStr = await _dialogService.ShowPromptAsync("Set Payment Amount", string.Empty, "Set", "Cancel", string.Empty, -1, component.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), Keyboard.Numeric);
            if (string.IsNullOrEmpty(amountStr)) return;
            if (!double.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                await _dialogService.ShowAlertAsync("Not a valid number.", "Alert", "OK");
                return;
            }
            amount = Math.Round(amount, 2);
            var sumOthers = PaymentComponents.Where(x => x != component).Sum(x => x.Amount);
            if (!_creditAccount && !Config.CanPayMoreThanOwned && sumOthers + amount > _amount)
            {
                await _dialogService.ShowAlertAsync($"Invalid amount. Maximum: {(_amount - sumOthers).ToCustomString()}", "Alert", "OK");
                return;
            }
            component.Amount = amount;
            MarkUnsavedChanges();
            // If amount is 0, the line is removed in OnComponentAmountChanged (triggered by Amount set above)
            RefreshLabels();
            SaveState();
        }

        /// <summary>Edit only ref/check/card number (matches Xamarin RefLabel_Click; label title from component).</summary>
        public async Task EditRefAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var title = component.RefLabelText;
            var initial = component.Ref ?? string.Empty;
            var promptTitle = title;
            // When method is Check, use default keyboard so user can enter letters (e.g. check numbers with letters).
            var keyboard = component.PaymentMethod == InvoicePaymentMethod.Check ? Keyboard.Default : Keyboard.Numeric;
            var refStr = await _dialogService.ShowPromptAsync(promptTitle, string.Empty, "Set", "Cancel", string.Empty, 20, initial, keyboard);
            if (refStr == null) return;
            component.Ref = refStr.Trim();
            MarkUnsavedChanges();
            SaveState();
        }

        /// <summary>Edit only comments (matches Xamarin CommentsLabel_Click).</summary>
        public async Task EditCommentsAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var comments = await _dialogService.ShowPromptAsync("Set Comments", string.Empty, "Set", "Cancel", string.Empty, -1, component.Comments ?? string.Empty);
            if (comments == null) return;
            component.Comments = comments ?? string.Empty;
            MarkUnsavedChanges();
            SaveState();
        }

        /// <summary>Edit only posted date (matches Xamarin Date_Click).</summary>
        public async Task EditPostedDateAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var dt = component.PostedDate == DateTime.MinValue ? DateTime.Now : component.PostedDate;
            // Use date picker dialog
            var selectedDate = await _dialogService.ShowDatePickerAsync("Posted Date", dt);
            if (selectedDate.HasValue)
            {
                component.PostedDate = selectedDate.Value.Date;
                MarkUnsavedChanges();
                SaveState();
            }
        }

        /// <summary>Handles Add Bank button click (matches Xamarin AddBank_Click).</summary>
        public async Task AddBankAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            
            // Prompt for new bank name (matches Xamarin AddBank_Click dialog)
            var bankName = await _dialogService.ShowPromptAsync("Alert", "Enter bank name:", "OK", "Cancel", "Ex: Wells Fargo, Chase, Bank of America", -1, string.Empty);
            if (!string.IsNullOrWhiteSpace(bankName))
            {
                // Check if bank already exists (matches Xamarin AddBank_Click validation)
                if (!BankAccount.bankListInDevice.Any(x => x.Equals(bankName, StringComparison.OrdinalIgnoreCase)))
                {
                    BankAccount.AddSavedBank(bankName);
                    
                    // Update bank lists for all components that show bank (matches Xamarin RefreshList after adding bank)
                    foreach (var comp in PaymentComponents.Where(c => c.ShowBank))
                    {
                        comp.UpdateBankList();
                    }
                    
                    // Set the newly added bank as selected for the component that added it
                    component.BankName = bankName;
                    MarkUnsavedChanges();
                    SaveState();
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Bank already exists.", "Alert", "OK");
                }
            }
        }

        /// <summary>Add image to payment component (matches Xamarin PaymentImage_Click / TakePhoto / PickFromGallery / ProcessImage).</summary>
        public async Task AddPaymentImageAsync(PaymentComponentViewModel component)
        {
            if (component == null) return;
            var choice = await _dialogService.ShowActionSheetAsync("Select Image Source", "Cancel", null, new[] { "Take Photo", "Choose from Gallery" });
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
            try
            {
                FileResult? photo = null;
                if (choice == "Take Photo")
                {
                    if (!MediaPicker.IsCaptureSupported)
                    {
                        await _dialogService.ShowAlertAsync("Camera is not available on this device.", "Error", "OK");
                        return;
                    }
                    photo = await MediaPicker.CapturePhotoAsync();
                }
                else
                {
                    photo = await MediaPicker.PickPhotoAsync();
                }
                if (photo == null) return;
                await ProcessPaymentImageAsync(photo, component);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error with image: " + ex.Message, "Error", "OK");
            }
        }

        private async Task ProcessPaymentImageAsync(FileResult photo, PaymentComponentViewModel component)
        {
            // When replacing, delete the old image file first
            if (component.HasImage && !string.IsNullOrEmpty(component.PaymentImagePath) && File.Exists(component.PaymentImagePath))
            {
                try { File.Delete(component.PaymentImagePath); } catch { /* best effort */ }
            }

            var imageId = Guid.NewGuid().ToString("N");
            var targetPath = Path.Combine(Config.PaymentImagesPath, imageId);
            var dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (var sourceStream = await photo.OpenReadAsync())
            using (var fileStream = File.Create(targetPath))
            {
                await sourceStream.CopyToAsync(fileStream);
            }
            component.Component.ExtraFields = UDFHelper.SyncSingleUDF("Image", imageId, component.Component.ExtraFields ?? string.Empty);
            component.RefreshImageDisplay();
            MarkUnsavedChanges();
            RefreshLabels();
            SaveState();
        }

        /// <summary>Remove the image from this payment component (delete file and clear UDF).</summary>
        public async Task RemovePaymentImageAsync(PaymentComponentViewModel component)
        {
            if (component == null || !component.HasImage) return;
            var confirmed = await _dialogService.ShowConfirmationAsync("Remove image?", "Remove the photo from this payment?", "Yes", "No");
            if (!confirmed) return;
            var imageId = UDFHelper.GetSingleUDF("Image", component.Component.ExtraFields ?? string.Empty);
            if (!string.IsNullOrEmpty(imageId))
            {
                var oldPath = Path.Combine(Config.PaymentImagesPath, imageId);
                try { if (File.Exists(oldPath)) File.Delete(oldPath); } catch { /* best effort */ }
            }
            component.Component.ExtraFields = UDFHelper.RemoveSingleUDF("Image", component.Component.ExtraFields ?? string.Empty);
            component.RefreshImageDisplay();
            MarkUnsavedChanges();
            SaveState();
        }

        /// <summary>View full-screen payment image (matches Xamarin PaymentImageView_Click).</summary>
        public async Task ViewPaymentImageAsync(PaymentComponentViewModel component)
        {
            if (component == null || string.IsNullOrEmpty(component.PaymentImagePath) || !File.Exists(component.PaymentImagePath))
                return;
            await Shell.Current.GoToAsync($"viewimage?imagePath={Uri.EscapeDataString(component.PaymentImagePath)}");
        }

        [RelayCommand]
        public async Task DeletePayment(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var confirmed = await _dialogService.ShowConfirmationAsync("Delete Payment", "Are you sure you want to delete this payment?", "Yes", "No");
            if (confirmed)
            {
                PaymentComponents.Remove(component);
                RefreshLabels();
                SaveState(); // Save state after deleting payment
            }
        }

        /// <summary>
        /// Saves the payment only (no navigation). Use SaveAndClose to save and exit.
        /// </summary>
        [RelayCommand]
        private async Task SavePayment()
        {
            await SavePaymentInternal();
        }

        private async Task<bool> SavePaymentInternal()
        {
            if (Config.PaymentBankIsMandatory)
            {
                foreach (var component in PaymentComponents)
                {
                    if (string.IsNullOrEmpty(component.BankName) && 
                        (component.PaymentMethod == InvoicePaymentMethod.Check || 
                         component.PaymentMethod == InvoicePaymentMethod.Credit_Card))
                    {
                        await _dialogService.ShowAlertAsync("You must select a Bank to be able to save this payment.", "Alert", "OK");
                        return false;
                    }
                }
            }

            if (Config.MustEnterPostedDate)
            {
                var checkPayments = PaymentComponents.Where(x => x.PaymentMethod == InvoicePaymentMethod.Check).ToList();
                if (checkPayments.Any(x => x.PostedDate == DateTime.MinValue))
                {
                    await _dialogService.ShowAlertAsync("You must enter a Posted Date for Check Payments.", "Alert", "OK");
                    return false;
                }
            }

            if (PaymentComponents.Any(x => x.PaymentMethod != InvoicePaymentMethod.Cash && string.IsNullOrEmpty(x.Ref)))
            {
                await _dialogService.ShowAlertAsync("You need to provide either a check or a reference number for all payments other than cash.", "Alert", "OK");
                return false;
            }
            
            var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
            var open = _amount - currentPaymentAmount;

            if (_invoicePayment == null)
            {
                _invoicePayment = new InvoicePayment(_client!);
                _invoicePayment.InvoicesId = _invoicesId;

                if (!string.IsNullOrEmpty(_ordersId))
                {
                    _invoicePayment.OrderId = string.Empty;
                    foreach (var idAsString in _ordersId.Split(','))
                    {
                        var id = Convert.ToInt32(idAsString);
                        var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
                        if (order != null)
                        {
                            if (!string.IsNullOrEmpty(_invoicePayment.OrderId))
                                _invoicePayment.OrderId += ",";
                            _invoicePayment.OrderId += order.UniqueId;
                        }
                    }
                }
            }

            _invoicePayment.DiscountApplied = _totalDiscount;
            _invoicePayment.Components.Clear();
            _invoicePayment.Components.AddRange(PaymentComponents.Where(x => x.Amount > 0).Select(x => x.Component));

            // Add location to payments
            foreach (var component in _invoicePayment.Components)
            {
                component.ExtraFields = UDFHelper.SyncSingleUDF("location", 
                    $"{Config.LastLatitude},{Config.LastLongitude}", 
                    component.ExtraFields);
            }

            _invoicePayment.Save();
            _hasUnsavedChanges = false;

            if (_invoicePayment.Components.All(x => x.Amount == 0))
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Empty Payment", 
                    "This payment has no amount. Do you want to delete it?", "Yes", "No");
                if (confirmed)
                {
                    if (Config.VoidPayments)
                        _invoicePayment.Void();
                    else
                        _invoicePayment.Delete();
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the payment and then navigates back (exit screen).
        /// </summary>
        [RelayCommand]
        private async Task SaveAndClose()
        {
            if (!await SavePaymentInternal())
                return;
            await FinishProcessAsync();
        }

        [RelayCommand]
        private async Task SendByEmail()
        {
            try
            {
                // Match Xamarin logic exactly: only show dialog if payment is null (not saved yet)
                if (_invoicePayment == null)
                {
                    var confirmed = await _dialogService.ShowConfirmationAsync("Warning", 
                        "If you send by Email, you won't be able to modify the payment. Are you sure that you'd like to continue?", 
                        "Yes", "No");
                    if (!confirmed)
                        return;

                    if (!await SavePaymentInternal())
                        return;

                    _invoicePayment!.Printed = true;
                    _invoicePayment.Save();
                    // Match Xamarin DisableForm() after sending by email
                    CanAddPayment = false;
                    CanSavePayment = false;
                    CanDeletePayment = false;
                    // Disable all payment component fields (matches Xamarin line 1787)
                    foreach (var component in PaymentComponents)
                    {
                        component.IsEditable = false;
                    }
                    // Note: Print and Send By Email remain enabled (not disabled in Xamarin DisableForm())
                    RefreshLabels();

                    // Proceed to send email after saving
                    await SendByEmailProceed();
                    return;
                }
                // If payment exists, proceed directly without confirmation (matches Xamarin else branch)
                await SendByEmailProceed();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        private async Task SendByEmailProceed()
        {
            try
            {
                // Get payment PDF (matches Xamarin SendByEmailProceed)
                var pdfFile = PdfHelper.GetPaymentPdf(_invoicePayment!);
                
                if (string.IsNullOrEmpty(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("PDF could not be generated.", "Alert", "OK");
                    return;
                }

                // Send email (matches Xamarin SendByEmail method)
                await SendPaymentByEmail(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending payment by email", "Alert", "OK");
            }
        }

        private async Task SendPaymentByEmail(string pdfFile)
        {
            if (string.IsNullOrEmpty(pdfFile))
            {
                await _dialogService.ShowAlertAsync("PDF could not be generated.", "Alert", "OK");
                return;
            }

            // Get client email
            string toEmail = string.Empty;
            if (_invoicePayment!.Client != null)
            {
                toEmail = UDFHelper.GetSingleUDF("email", _invoicePayment.Client.ExtraPropertiesAsString);
            }

            // Determine subject and body (matches Xamarin SendByEmail)
            string subject = string.Empty;
            string body = string.Empty;

            if (Config.EcoSkyWaterCustomEmail)
            {
                subject = "Eco SkyWater Payment";
                body = string.Format(@"<html><body>Thank you for choosing Eco SkyWater the most sustainable bottled water on earth, every contribution helps towards a healthier plastic free environment.<br><br>For more information on how our water and plant based bottles are made, please visit <a href='{0}' >www.ecoskywater.com</a><br><br>Payments can be made through bank transfer using the attached banking information link: <a href='{1}'>https://ecoskywater-my.sharepoint.com/:b:/p/philip/EQdUS4WWb4tMhHQilv2-FzgBRy2w8yEbNW6XSCsp9ww1Vw?e=e65jGO</a><br><br>Please reach out to us with any feedback as we are continually improving our products.<br><br><br>Do Good Live Great,<br><br>Eco SkyWater<br><b> <span style='color:blue'>Local . Sustainable . Pure</span></b><br><br>E: <a href='{2}'>Sales@ecoskywater.com</a><br>IG: @ecoskywater<br>FB: @ecoskyh2o<br>T: 1 (246) 572-4587<br>C: 1 (246) 235-3269<br>Lot 1B Walkes Spring, St. Thomas, Barbados</body></html>", 
                    "www.ecoskywater.com", 
                    "https://ecoskywater-my.sharepoint.com/:b:/p/philip/EQdUS4WWb4tMhHQilv2-FzgBRy2w8yEbNW6XSCsp9ww1Vw?e=e65jGO", 
                    "Sales@ecoskywater.com");
            }
            else
            {
                subject = "Payment Attached";
            }

            // Send email using helper interface (matches Xamarin SendByEmail)
            var toAddresses = new List<string>();
            if (!string.IsNullOrEmpty(toEmail))
            {
                toAddresses.Add(toEmail);
            }

            Config.helper?.SendOrderByEmail(pdfFile, subject, body, toAddresses);
        }

        [RelayCommand]
        internal async Task DeleteEntirePayment()
        {
            System.Diagnostics.Debug.WriteLine("DeleteEntirePayment: Method called!");
            try
            {
                // Match Xamarin DeletePaymentButton_Click: Always show confirmation dialog
                System.Diagnostics.Debug.WriteLine("DeleteEntirePayment: Showing confirmation dialog");
                var confirmed = await _dialogService.ShowConfirmationAsync("Alert", 
                    "Are you sure you want to delete this payment?", "Yes", "No");
                System.Diagnostics.Debug.WriteLine($"DeleteEntirePayment: User confirmed: {confirmed}");
                if (confirmed)
                {
                    if (_invoicePayment != null)
                    {
                        if (Config.VoidPayments)
                            _invoicePayment.Void();
                        else
                        {
                            if (Config.SendBackgroundPayments && Config.SendTempPaymentsInBackground)
                            {
                                // TODO: Implement DeleteTempPaymentFromOS if needed
                                _invoicePayment.Delete();
                            }
                            else
                                _invoicePayment.Delete();
                        }
                    }
                    // Always finish the process, even if there's no payment (new payment scenario)
                    await FinishProcessAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error deleting payment: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task PrintPayment()
        {
            // Save first if needed (save only — do not exit)
            if (_invoicePayment == null || !_invoicePayment.Printed)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Warning", 
                    "If you print, you won't be able to modify the payment. Are you sure you'd like to continue?", "Yes", "No");
                if (confirmed)
                {
                    if (!await SavePaymentInternal())
                        return;
                }
                else
                    return;
            }

            await SelectCompanyAndPrint();
        }

        /// <summary>
        /// Select company for printing (matches Xamarin PaymentSetValuesActivity.SelectCompany).
        /// Uses client-assigned companies when available (like ClientDetailsPage): only shows picker
        /// when this client has more than one company assigned; otherwise uses the single company or global fallback.
        /// </summary>
        private async Task SelectCompanyAndPrint()
        {
            List<CompanyInfo> companiesToUse = null;
            if (_client != null)
            {
                var clientCompanies = SalesmanAvailableCompany.GetCompanies(Config.SalesmanId, _client.ClientId);
                if (clientCompanies.Count > 0)
                    companiesToUse = clientCompanies;
            }

            if (companiesToUse == null)
                companiesToUse = CompanyInfo.Companies.ToList();

            if (companiesToUse.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No company available for printing.", "Alert", "OK");
                return;
            }

            if (companiesToUse.Count == 1 || !Config.PickCompany)
            {
                CompanyInfo.SelectedCompany = companiesToUse[0];
                await DoPrintPaymentAsync();
                return;
            }

            // Multiple companies for this client and PickCompany: show picker (match ClientDetailsPage)
            var companyOptions = companiesToUse.Select(c =>
            {
                var subtitleParts = new[] { c.CompanyAddress1, c.CompanyAddress2, c.CompanyPhone }
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                var subtitle = subtitleParts.Length > 0 ? string.Join("\n", subtitleParts) : null;
                return (c.CompanyName ?? string.Empty, subtitle);
            }).ToArray();

            var selectedIndex = await _dialogService.ShowSingleChoiceDialogAsync("Select Company", companyOptions, 0);
            if (selectedIndex < 0 || selectedIndex >= companiesToUse.Count)
                return;

            CompanyInfo.SelectedCompany = companiesToUse[selectedIndex];
            await DoPrintPaymentAsync();
        }

        private async Task DoPrintPaymentAsync()
        {
            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < copies; i++)
                    {
                        if (!printer.PrintPayment(_invoicePayment!))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing payment";

                    _invoicePayment!.Printed = true;
                    _invoicePayment.Save();
                    
                    //Update UI - Match Xamarin DisableForm() after printing
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // CRITICAL: Update _paymentId so re-initialization works correctly
                        _paymentId = _invoicePayment.Id;
                        
                        // Disable Save, Add, and Delete buttons (matches Xamarin DisableForm())
                        CanSavePayment = false;
                        CanAddPayment = false;
                        CanDeletePayment = false;
                        // Disable all payment component fields (matches Xamarin line 1787)
                        foreach (var component in PaymentComponents)
                        {
                            component.IsEditable = false;
                        }
                        // Note: Print and Send By Email remain enabled (not disabled in Xamarin DisableForm())
                        RefreshLabels();
                    });

                    
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task FinishProcessAsync()
        {
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);

            // Match Xamarin FinishActivityProcess: when coming from FinalizeBatch, just navigate back once
            if (_fromFinalize)
            {
                Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_goBackToMain)
            {
                if (_fromPaymentTab)
                {
                    Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
                    await Shell.Current.GoToAsync("paymentselectclient");
                }
                else if (_fromClientDetails)
                {
                    Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
                await Shell.Current.GoToAsync("..");
            }
        }

        /// <summary>Marks that the user has made changes (prompt on back).</summary>
        private void MarkUnsavedChanges()
        {
            _hasUnsavedChanges = true;
        }

        /// <summary>Called when back is pressed (physical or nav bar). Returns true to prevent navigation, false to allow.</summary>
        public async Task<bool> OnBackButtonPressed()
        {
            if (!_hasUnsavedChanges)
                return false;
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Warning",
                "You will lose any change that you did in this screen, continue?",
                "Yes",
                "No");
            if (!confirmed)
                return true; // Prevent navigation
            return false;
        }

        public void OnComponentAmountChanged()
        {
            // Remove any component with amount 0 (mirror PaymentSetvaluesActivity: setting amount to 0 removes the line)
            var toRemove = PaymentComponents.Where(x => x.Amount == 0).ToList();
            if (toRemove.Count > 0)
            {
                foreach (var c in toRemove)
                    PaymentComponents.Remove(c);
                MarkUnsavedChanges();
            }
            RefreshLabels();
            SaveState(); // Save state when component amounts change
        }

        /// <summary>
        /// Saves the current payment state to a temp file.
        /// Match Xamarin PaymentSetValuesActivity: saves payment components to temp file.
        /// </summary>
        public void SaveState()
        {
            if (string.IsNullOrEmpty(_tempFile))
                return;
            // Avoid overwriting temp file before InitializeAsync has finished (e.g. when OnAppearing runs before init).
            if (!_initializationComplete)
                return;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    if (File.Exists(_tempFile))
                        File.Delete(_tempFile);

                    using (StreamWriter writer = new StreamWriter(_tempFile, false))
                    {
                        // Write header: paymentId, clientId, invoicesId, ordersId, totalDiscount
                        var headerLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                            (char)20,
                            _paymentId,
                            _client?.ClientId ?? 0,
                            _invoicesId ?? string.Empty,
                            _ordersId ?? string.Empty,
                            _totalDiscount);
                        writer.WriteLine(headerLine);

                        // Write each payment component: Ref, Comments, Amount, PaymentMethod, BankName, PostedDate, ExtraFields
                        foreach (var component in PaymentComponents)
                        {
                            // Save PostedDate to ExtraFields if not MinValue
                            string extraFields = component.Component.ExtraFields ?? string.Empty;
                            if (component.PostedDate != DateTime.MinValue)
                            {
                                extraFields = UDFHelper.SyncSingleUDF("PostedDate", component.PostedDate.Ticks.ToString(), extraFields);
                            }

                            string checkNumber = component.Ref ?? string.Empty;
                            checkNumber = checkNumber.Replace((char)13, (char)32).Replace((char)10, (char)32);
                            string comment = component.Comments ?? string.Empty;
                            comment = comment.Replace((char)13, (char)32).Replace((char)10, (char)32);
                            // Get BankName from Component (it reads from ExtraFields)
                            string bankName = component.Component.BankName ?? string.Empty;

                            var componentLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}",
                                (char)20,
                                checkNumber,
                                comment,
                                component.Amount,
                                (int)component.PaymentMethod,
                                bankName,
                                component.PostedDate != DateTime.MinValue ? component.PostedDate.Ticks.ToString() : "0",
                                extraFields);
                            writer.WriteLine(componentLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }
        }

        /// <summary>
        /// Loads the payment state from a temp file.
        /// Match Xamarin PaymentSetValuesActivity: loads payment components from temp file.
        /// </summary>
        private void LoadState()
        {
            if (string.IsNullOrEmpty(_tempFile) || !File.Exists(_tempFile))
                return;

            try
            {
                using (StreamReader reader = new StreamReader(_tempFile))
                {
                    // Read header: paymentId, clientId, invoicesId, ordersId, totalDiscount
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        return;

                    string[] parts = line.Split(new char[] { (char)20 });
                    if (parts.Length >= 1)
                    {
                        if (int.TryParse(parts[0], out var paymentId))
                            _paymentId = paymentId;
                    }
                    if (parts.Length >= 2)
                    {
                        if (int.TryParse(parts[1], out var clientId))
                        {
                            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
                        }
                    }
                    if (parts.Length >= 3)
                        _invoicesId = parts[2] ?? string.Empty;
                    if (parts.Length >= 4)
                        _ordersId = parts[3] ?? string.Empty;
                    if (parts.Length >= 5)
                    {
                        if (double.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var discount))
                            _totalDiscount = discount;
                    }

                    // Load payment components
                    PaymentComponents.Clear();
                    while ((line = reader.ReadLine()) != null)
                    {
                        parts = line.Split(new char[] { (char)20 });
                        if (parts.Length < 4)
                            continue;

                        var component = new PaymentComponent();
                        component.Ref = parts[0] ?? string.Empty;
                        component.Comments = parts[1] ?? string.Empty;
                        
                        if (double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var amount))
                            component.Amount = amount;
                        
                        if (int.TryParse(parts[3], out var methodInt))
                            component.PaymentMethod = (InvoicePaymentMethod)methodInt;

                        if (parts.Length > 4)
                        {
                            // BankName is stored in ExtraFields
                            var bankName = parts[4] ?? string.Empty;
                            if (!string.IsNullOrEmpty(bankName))
                                component.ExtraFields = UDFHelper.SyncSingleUDF("BankName", bankName, component.ExtraFields ?? string.Empty);
                        }

                        if (parts.Length > 5)
                        {
                            if (long.TryParse(parts[5], out var ticks) && ticks > 0)
                                component.PostedDate = new DateTime(ticks);
                        }

                        if (parts.Length > 6)
                        {
                            component.ExtraFields = parts[6] ?? string.Empty;
                            // Also check ExtraFields for PostedDate if not already set
                            if (component.PostedDate == DateTime.MinValue)
                            {
                                var postedDate = UDFHelper.GetSingleUDF("PostedDate", component.ExtraFields);
                                if (!string.IsNullOrEmpty(postedDate) && long.TryParse(postedDate, out var postedTicks))
                                    component.PostedDate = new DateTime(postedTicks);
                            }
                        }

                        PaymentComponents.Add(new PaymentComponentViewModel(component, this));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        /// <summary>
        /// Gets the current temp file path. Used for saving to ActivityState.
        /// </summary>
        public string GetTempFilePath()
        {
            return _tempFile;
        }
    }

    public partial class PaymentComponentViewModel : ObservableObject
    {
        private readonly PaymentSetValuesPageViewModel _parent;
        private bool _isUpdatingBankIndex = false;
        
        [ObservableProperty]
        private PaymentComponent _component;

        [ObservableProperty]
        private InvoicePaymentMethod _paymentMethod;

        [ObservableProperty]
        private double _amount;

        [ObservableProperty]
        private string _ref = string.Empty;

        [ObservableProperty]
        private string _comments = string.Empty;

        [ObservableProperty]
        private string _bankName = string.Empty;

        [ObservableProperty]
        private DateTime _postedDate = DateTime.MinValue;

        [ObservableProperty]
        private string _paymentMethodText = string.Empty;

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        private bool _showRef;

        [ObservableProperty]
        private bool _showPostedDate;

        [ObservableProperty]
        private bool _showBank;

        /// <summary>Whether the payment component fields are editable (disabled when payment is printed, matches Xamarin line 1787).</summary>
        [ObservableProperty]
        private bool _isEditable = true;

        /// <summary>List of banks for the Picker (matches Xamarin bank spinner).</summary>
        [ObservableProperty]
        private ObservableCollection<string> _bankList = new();

        /// <summary>Selected bank index in the Picker (matches Xamarin bank spinner selection).</summary>
        [ObservableProperty]
        private int _selectedBankIndex = -1;

        /// <summary>Label for ref field: "Check Number", "Card Number", or "Ref Number" (matches Xamarin RefName).</summary>
        [ObservableProperty]
        private string _refLabelText = "Ref Number";

        /// <summary>Display value for ref (formatted for credit card as XXXX XXXX XXXX XXXX).</summary>
        [ObservableProperty]
        private string _refDisplayText = string.Empty;

        [ObservableProperty]
        private bool _hasImage;

        [ObservableProperty]
        private string _paymentImagePath = string.Empty;

        /// <summary>ImageSource for thumbnail binding (file path can fail to load on some platforms).</summary>
        [ObservableProperty]
        private ImageSource? _paymentImageSource;

        public PaymentComponentViewModel(PaymentComponent component, PaymentSetValuesPageViewModel parent)
        {
            _component = component;
            _parent = parent;
            PaymentMethod = component.PaymentMethod;
            Amount = component.Amount;
            Ref = component.Ref ?? string.Empty;
            Comments = component.Comments ?? string.Empty;
            BankName = component.BankName ?? string.Empty;
            PostedDate = component.PostedDate;
            // Set IsEditable based on whether payment is printed (matches Xamarin line 1787)
            IsEditable = !_parent.GetIsPaymentPrinted();
            UpdateProperties();
            RefreshImageDisplay();
            // UpdateBankList is called in UpdateProperties when ShowBank is true
        }

        public void RefreshImageDisplay()
        {
            var id = UDFHelper.GetSingleUDF("Image", _component.ExtraFields ?? string.Empty);
            HasImage = !string.IsNullOrEmpty(id);
            var path = HasImage ? Path.Combine(Config.PaymentImagesPath, id) : string.Empty;
            PaymentImagePath = path;
            PaymentImageSource = HasImage && !string.IsNullOrEmpty(path) && File.Exists(path) ? ImageSource.FromFile(path) : null;
        }

        partial void OnPaymentMethodChanged(InvoicePaymentMethod value)
        {
            _component.PaymentMethod = value;
            UpdateProperties();
        }

        partial void OnAmountChanged(double value)
        {
            _component.Amount = value;
            AmountText = value.ToCustomString();
            _parent.OnComponentAmountChanged();
        }

        partial void OnCommentsChanged(string value)
        {
            _component.Comments = value;
        }

        partial void OnBankNameChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _component.ExtraFields = UDFHelper.SyncSingleUDF("BankName", value, _component.ExtraFields);
            else
                _component.ExtraFields = UDFHelper.RemoveSingleUDF("BankName", _component.ExtraFields);
            
            // Update selected index when BankName changes
            UpdateSelectedBankIndex();
        }

        /// <summary>Handles bank Picker selection changed (matches Xamarin bank spinner ItemSelected).</summary>
        partial void OnSelectedBankIndexChanged(int value)
        {
            // Prevent recursive updates when we programmatically set the index
            if (_isUpdatingBankIndex) return;
            if (value < 0 || value >= BankList.Count) return;
            
            var selectedBank = BankList[value];
            BankName = selectedBank == "None" ? string.Empty : selectedBank;
            _parent.SaveState();
        }

        partial void OnPostedDateChanged(DateTime value)
        {
            _component.PostedDate = value;
        }

        private void UpdateProperties()
        {
            PaymentMethodText = PaymentMethod.ToString().Replace("_", " ");
            AmountText = Amount.ToCustomString();
            ShowRef = PaymentMethod != InvoicePaymentMethod.Cash;
            ShowPostedDate = PaymentMethod == InvoicePaymentMethod.Check;
            ShowBank = Config.CheckCommunicatorVersion("45.0.0") && Config.ShowInvoicesCreditsInPayments && (PaymentMethod == InvoicePaymentMethod.Check || PaymentMethod == InvoicePaymentMethod.Credit_Card);
            RefLabelText = PaymentMethod == InvoicePaymentMethod.Check ? "Check Number" :
                PaymentMethod == InvoicePaymentMethod.Credit_Card ? "Card Number" : "Ref Number";
            RefDisplayText = FormatRefForDisplay(Ref);
            RefreshImageDisplay();
            
            // Update bank list when ShowBank changes (matches Xamarin bank spinner adapter)
            if (ShowBank)
            {
                UpdateBankList();
            }
        }

        /// <summary>Updates the bank list for the Picker (matches Xamarin bank spinner adapter setup).</summary>
        public void UpdateBankList()
        {
            BankList.Clear();
            // Use bankListInDevice (from Config.SavedBanks) like Xamarin, not BankAccount.List
            // bankListInDevice already includes "None" at index 0
            foreach (var bank in BankAccount.bankListInDevice)
            {
                BankList.Add(bank);
            }
            
            // Set selected index based on current BankName
            UpdateSelectedBankIndex();
        }

        /// <summary>Updates the selected bank index based on current BankName (matches Xamarin bank spinner selection).</summary>
        private void UpdateSelectedBankIndex()
        {
            _isUpdatingBankIndex = true;
            try
            {
                if (string.IsNullOrEmpty(BankName))
                {
                    SelectedBankIndex = 0; // "None" is at index 0
                }
                else
                {
                    var index = BankList.IndexOf(BankName);
                    SelectedBankIndex = index >= 0 ? index : 0;
                }
            }
            finally
            {
                _isUpdatingBankIndex = false;
            }
        }

        private string FormatRefForDisplay(string refValue)
        {
            if (string.IsNullOrEmpty(refValue)) return string.Empty;
            if (PaymentMethod != InvoicePaymentMethod.Credit_Card) return refValue;
            var digits = new string(refValue.Where(char.IsDigit).ToArray());
            if (digits.Length <= 4) return refValue;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < digits.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(digits[i]);
            }
            return sb.ToString();
        }

        partial void OnRefChanged(string value)
        {
            _component.Ref = value;
            RefDisplayText = FormatRefForDisplay(value);
        }
    }
}

