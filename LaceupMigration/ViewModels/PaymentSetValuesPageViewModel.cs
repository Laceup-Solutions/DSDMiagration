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
        private List<Invoice> _invoices = new();
        private List<Order> _orders = new();
        private double _amount = 0;
        private double _creditAmount = 0;
        private double _totalDiscount = 0;
        private bool _creditAccount = false;
        private bool _fromPaymentTab = false;
        private bool _fromClientDetails = false;
        private bool _goBackToMain = false;
        private int _paymentId = 0;
        private string _invoicesId = string.Empty;
        private string _ordersId = string.Empty;
        private string _tempFile = string.Empty;

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

            // Parse orderIds
            if (query.TryGetValue("orderIds", out var orderIdsValue) && orderIdsValue != null)
            {
                _ordersId = orderIdsValue.ToString() ?? string.Empty;
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

            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
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
                    PaymentComponents.Add(new PaymentComponentViewModel(component, this));
                }

                if (_invoicePayment.Printed)
                {
                    CanAddPayment = false;
                    CanSavePayment = false;
                    CanDeletePayment = false;
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
            }

            // Set CanDeletePayment based on whether there's a payment to delete
            CanDeletePayment = _paymentId > 0 && (_invoicePayment == null || !_invoicePayment.Printed);

            RefreshLabels();
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

            // Load orders
            if (!string.IsNullOrEmpty(_ordersId))
            {
                foreach (var idAsString in _ordersId.Split(','))
                {
                    var id = Convert.ToInt32(idAsString);
                    var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
                    if (order != null)
                    {
                        if (Config.ShowInvoicesCreditsInPayments)
                        {
                            if (order.OrderTotalCost() < 0)
                            {
                                _creditAmount += order.OrderTotalCost();
                                if (!string.IsNullOrEmpty(order.PrintedOrderId))
                                    creditsDocNumbersList.Add(order.PrintedOrderId);
                            }
                            else
                            {
                                _amount += order.OrderTotalCost();
                                if (!string.IsNullOrEmpty(order.PrintedOrderId))
                                    docNumbersList.Add(order.PrintedOrderId);
                            }
                        }
                        else
                        {
                            _amount += order.OrderTotalCost();
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

        public void RefreshLabels()
        {
            var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
            var open = _amount - currentPaymentAmount;

            AmountLabel = (_amount + _totalDiscount).ToCustomString();
            OpenLabel = open.ToCustomString();
            // Set color: green if $0 or $0.00, otherwise black (matching Xamarin)
            if (open == 0 || OpenLabel == "$0" || OpenLabel == "$0.00")
                OpenLabelColor = Colors.Green;
            else
                OpenLabelColor = Colors.Black;
            PaidLabel = currentPaymentAmount.ToCustomString();

            if (ShowCreditAmount)
                CreditAmountLabel = _creditAmount.ToCustomString();

            DiscountTotal = _totalDiscount.ToCustomString();
            ShowDiscount = _totalDiscount > 0;

            // Disable Add Payment button if:
            // 1. Payment is already printed (can't modify)
            // 2. There's no open amount left to collect (unless it's a credit account)
            // 3. There are empty payment components that need to be filled first
            var hasEmptyComponents = PaymentComponents.Any(x => x.Amount == 0);
            CanAddPayment = !hasEmptyComponents && 
                           (_creditAccount || ((_invoicePayment == null || !_invoicePayment.Printed) && open > 0));
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
            RefreshLabels();
            SaveState(); // Save state after adding payment
        }

        public async Task EditPayment(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var paymentMethods = GetAvailablePaymentMethods();

            var methodChoice = await _dialogService.ShowActionSheetAsync("Select Payment Method", "", "Cancel", paymentMethods);
            if (string.IsNullOrEmpty(methodChoice) || methodChoice == "Cancel")
                return;

            var method = Enum.Parse<InvoicePaymentMethod>(methodChoice.Replace(" ", "_"));

            var amountStr = await _dialogService.ShowPromptAsync("Enter Amount", "Amount", "OK", "Cancel", component.Amount.ToString("F2"), keyboard: Keyboard.Numeric);
            if (string.IsNullOrEmpty(amountStr) || !double.TryParse(amountStr, out var amount))
                return;

            var refNumber = string.Empty;
            if (method != InvoicePaymentMethod.Cash)
            {
                refNumber = await _dialogService.ShowPromptAsync("Enter Reference Number", 
                    method == InvoicePaymentMethod.Check ? "Check Number" : "Card/Reference Number", 
                    "OK", "Cancel", component.Ref ?? string.Empty);
            }

            var comments = await _dialogService.ShowPromptAsync("Enter Comments", "Comments", "OK", "Cancel", component.Comments ?? string.Empty);

            component.PaymentMethod = method;
            component.Amount = amount;
            component.Ref = refNumber ?? string.Empty;
            component.Comments = comments ?? string.Empty;

            RefreshLabels();
            SaveState(); // Save state after editing payment
        }

        private string[] GetAvailablePaymentMethods()
        {
            // Match Xamarin GetInvoicePaymentMethodNames() method exactly (lines 2099-2117)
            var availableMethods = new List<string>();

            // Always include Cash and Check (matches Xamarin lines 2103-2104)
            availableMethods.Add(InvoicePaymentMethod.Cash.ToString().Replace("_", " "));
            availableMethods.Add(InvoicePaymentMethod.Check.ToString().Replace("_", " "));

            // Extended payment methods only show if Config.ExtendedPaymentOptions is true
            // Match Xamarin lines 2106-2114
            if (Config.ExtendedPaymentOptions)
            {
                availableMethods.Add(InvoicePaymentMethod.Credit_Card.ToString().Replace("_", " "));
                // ACH is commented out in Xamarin (line 2109), so never include it
                // result.Add(InvoicePaymentMethod.Ach.ToString(activity)); // <-- Commented in Xamarin
                availableMethods.Add(InvoicePaymentMethod.Money_Order.ToString().Replace("_", " "));
                availableMethods.Add(InvoicePaymentMethod.Transfer.ToString().Replace("_", " "));
                availableMethods.Add(InvoicePaymentMethod.Zelle_Transfer.ToString().Replace("_", " "));
            }

            return availableMethods.ToArray();
        }

        public async Task EditPaymentMethod(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var paymentMethods = GetAvailablePaymentMethods();

            var methodChoice = await _dialogService.ShowActionSheetAsync("Select Payment Method", "", "Cancel", paymentMethods);
            if (string.IsNullOrEmpty(methodChoice) || methodChoice == "Cancel")
                return;

            var method = Enum.Parse<InvoicePaymentMethod>(methodChoice.Replace(" ", "_"));
            component.PaymentMethod = method;

            // If payment method changed to non-Cash, prompt for ref number if not set
            if (method != InvoicePaymentMethod.Cash && string.IsNullOrEmpty(component.Ref))
            {
                if (method == InvoicePaymentMethod.Credit_Card)
                {
                    // Use separate credit card number popup
                    await EditCreditCardNumber(component);
                }
                else
                {
                    // Use reference number or check number popup
                    var label = method == InvoicePaymentMethod.Check ? "Check Number" : "Reference Number";
                    var refNumber = await _dialogService.ShowPromptAsync($"Enter {label}", label, 
                        "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: component.Ref ?? string.Empty);
                    if (!string.IsNullOrEmpty(refNumber))
                        component.Ref = refNumber;
                }
            }

            RefreshLabels();
            SaveState();
        }

        public async Task EditAmount(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var amountStr = await _dialogService.ShowPromptAsync("Enter Amount", "Amount", "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: component.Amount.ToString("F2"), keyboard: Keyboard.Numeric);
            if (string.IsNullOrEmpty(amountStr) || !double.TryParse(amountStr, out var amount))
                return;

            // Round amount to 2 decimal places (matches Xamarin line 2238)
            amount = Math.Round(amount, 2);

            // Match Xamarin CountedHandler logic (lines 2240-2246): remove component if amount is 0
            if (amount == 0)
            {
                PaymentComponents.Remove(component);
                SaveState();
                RefreshLabels();
                return;
            }

            component.Amount = amount;
            RefreshLabels();
            SaveState();
        }

        public async Task EditComments(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var comments = await _dialogService.ShowPromptAsync("Enter Comments", "Comments", "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: component.Comments ?? string.Empty);
            if (comments == null)
                return;

            component.Comments = comments;
            SaveState();
        }

        public async Task EditRefNumber(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            // For Credit Card, use separate "Credit Card Number" popup
            if (component.PaymentMethod == InvoicePaymentMethod.Credit_Card)
            {
                await EditCreditCardNumber(component);
                return;
            }

            // For other payment methods (Check, ACH, etc.), use "Reference Number" or "Check Number"
            var label = component.PaymentMethod == InvoicePaymentMethod.Check ? "Check Number" : "Reference Number";
            var refNumber = await _dialogService.ShowPromptAsync($"Enter {label}", label, 
                "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: component.Ref ?? string.Empty);
            if (refNumber == null)
                return;

            component.Ref = refNumber;
            SaveState();
        }

        public async Task EditCreditCardNumber(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var cardNumber = await _dialogService.ShowPromptAsync("Enter Credit Card Number", "Credit Card Number", 
                "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: component.Ref ?? string.Empty);
            if (cardNumber == null)
                return;

            component.Ref = cardNumber;
            SaveState();
        }

        public async Task EditBankName(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            // Load banks if not already loaded
            if (BankAccount.bankListInDevice.Count == 0)
            {
                BankAccount.LoadBanks();
            }

            // Ensure "None" is always in the list (at index 0)
            if (!BankAccount.bankListInDevice.Contains("None"))
            {
                BankAccount.bankListInDevice.Insert(0, "None");
            }

            // Build list of options: "None" + saved banks + "Add New Bank"
            var bankOptions = new List<string> { "None" };
            
            // Add saved banks (excluding "None" if it appears again)
            foreach (var bank in BankAccount.bankListInDevice)
            {
                if (bank != "None" && !string.IsNullOrEmpty(bank))
                {
                    bankOptions.Add(bank);
                }
            }
            
            // Add "Add New Bank" option to allow custom entry
            bankOptions.Add("Add New Bank");

            // Always show action sheet with at least "None" and "Add New Bank"
            var selectedBank = await _dialogService.ShowActionSheetAsync("Select Bank", "", "Cancel", bankOptions.ToArray());
            if (string.IsNullOrEmpty(selectedBank) || selectedBank == "Cancel")
                return;

            // Handle "None" selection
            if (selectedBank == "None")
            {
                component.BankName = string.Empty;
            }
            // Handle "Add New Bank" - show text input
            else if (selectedBank == "Add New Bank")
            {
                var bankName = await _dialogService.ShowPromptAsync("Enter Bank Name", "Bank Name", "OK", "Cancel", placeholder: "", maxLength: -1, initialValue: string.Empty);
                if (string.IsNullOrEmpty(bankName))
                    return;

                component.BankName = bankName;
                
                // Add to saved banks if not already there
                if (!BankAccount.bankListInDevice.Contains(bankName))
                {
                    BankAccount.AddSavedBank(bankName);
                }
            }
            // Handle selection of existing bank
            else
            {
                component.BankName = selectedBank;
            }

            SaveState();
        }

        public async Task EditPostedDate(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            // For date picker, we'll use a prompt with date format
            var dateStr = await _dialogService.ShowPromptAsync("Enter Posted Date", "Posted Date (MM/DD/YYYY)", "OK", "Cancel", 
                placeholder: "", maxLength: -1, initialValue: component.PostedDate != DateTime.MinValue ? component.PostedDate.ToString("MM/dd/yyyy") : string.Empty);
            if (string.IsNullOrEmpty(dateStr))
                return;

            if (DateTime.TryParse(dateStr, out var postedDate))
            {
                component.PostedDate = postedDate;
                SaveState();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Invalid date format. Please use MM/DD/YYYY.", "Error", "OK");
            }
        }

        // Match Xamarin ShowImageSourceDialog (lines 1889-1907)
        public async Task ShowImageSourceDialog(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var options = new[] { "Take Photo", "Choose from Gallery" };
            var choice = await _dialogService.ShowActionSheetAsync("Select Image Source", "", "Cancel", options);
            
            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                return;

            switch (choice)
            {
                case "Take Photo":
                    await TakePhotoAsync(component);
                    break;
                case "Choose from Gallery":
                    await PickFromGalleryAsync(component);
                    break;
            }
        }

        // Match Xamarin TakePhoto (lines 1909-1923)
        private async Task TakePhotoAsync(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            try
            {
                if (!MediaPicker.IsCaptureSupported)
                {
                    await _dialogService.ShowAlertAsync("Camera is not available on this device.", "Error");
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo == null)
                    return;

                await ProcessImageAsync(photo.FullPath, component);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error taking photo: {ex.Message}", "Error");
            }
        }

        // Match Xamarin PickFromGallery (lines 1925-1936)
        private async Task PickFromGalleryAsync(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null)
                    return;

                await ProcessImageAsync(photo.FullPath, component);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error picking photo: {ex.Message}", "Error");
            }
        }

        // Match Xamarin ProcessImage (lines 1938-1960)
        private async Task ProcessImageAsync(string filePath, PaymentComponentViewModel component)
        {
            if (component == null || string.IsNullOrEmpty(filePath))
                return;

            try
            {
                // Generate unique ID (matches Xamarin line 1943)
                var imageId = Guid.NewGuid().ToString("N");

                // Target path in PaymentImagesPath (matches Xamarin line 1945)
                var targetPath = Path.Combine(Config.PaymentImagesPath, imageId);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Copy file to target location (matches Xamarin lines 1947-1950)
                File.Copy(filePath, targetPath, true);

                // Store imageId in ExtraFields (matches Xamarin lines 1952-1953)
                var updatedExtraFields = UDFHelper.SyncSingleUDF("Image", imageId, component.ExtraFields);
                component.ExtraFields = updatedExtraFields;
                component.Component.ExtraFields = updatedExtraFields;

                // Update component properties to refresh image display
                component.UpdateProperties();

                // Delete source file if it exists (matches Xamarin line 1955)
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog($"Error deleting temp file: {ex.Message}");
                    }
                }

                // Refresh UI (matches Xamarin lines 1957-1959)
                RefreshLabels();
                SaveState();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error processing image: {ex.Message}", "Error");
            }
        }

        // Match Xamarin PaymentImageView_Click (lines 1962-1973)
        public async Task ViewPaymentImageAsync(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            try
            {
                // Get image ID from ExtraFields (matches Xamarin line 1967)
                var uniqueId = UDFHelper.GetSingleUDF("Image", component.ExtraFields);
                if (string.IsNullOrEmpty(uniqueId))
                    return;

                var imagePath = Path.Combine(Config.PaymentImagesPath, uniqueId);
                if (!File.Exists(imagePath))
                {
                    await _dialogService.ShowAlertAsync("Image file not found.", "Error");
                    return;
                }

                // Navigate to view image page (matches Xamarin ViewImageActivity)
                var uniqueRoute = $"viewimage_{Guid.NewGuid():N}";
                Routing.RegisterRoute(uniqueRoute, typeof(Views.ViewImagePage));
                await Shell.Current.GoToAsync($"{uniqueRoute}?imagePath={Uri.EscapeDataString(imagePath)}");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error viewing image: {ex.Message}", "Error");
            }
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

        [RelayCommand]
        private async Task SavePayment()
        {
            await SavePaymentInternal();
            await FinishProcessAsync();
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

        [RelayCommand]
        private async Task SaveAndClose()
        {
            await SavePayment();
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
                    CanAddPayment = false;
                    CanSavePayment = false;
                    CanDeletePayment = false;
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
        private async Task DeleteEntirePayment()
        {
            if (_invoicePayment == null)
            {
                await _dialogService.ShowAlertAsync("No payment to delete.", "Alert", "OK");
                return;
            }

            var confirmed = await _dialogService.ShowConfirmationAsync("Delete Payment", 
                "Are you sure you want to delete this payment?", "Yes", "No");
            if (confirmed)
            {
                if (Config.VoidPayments)
                    _invoicePayment.Void();
                else
                    _invoicePayment.Delete();

                await FinishProcessAsync();
            }
        }

        [RelayCommand]
        private async Task PrintPayment()
        {
            if (_invoicePayment == null || !_invoicePayment.Printed)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Print Before Finalize", 
                    "You must save the payment before printing. Save now?", "Yes", "No");
                if (confirmed)
                {
                    await SavePayment();
                }
                else
                    return;
            }

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
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

            if (_goBackToMain)
            {
                if (_fromPaymentTab)
                    await Shell.Current.GoToAsync("paymentselectclient");
                else if (_fromClientDetails)
                    await Shell.Current.GoToAsync("..");
                else
                    await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        public void OnComponentAmountChanged()
        {
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

        [ObservableProperty]
        private string _refNumberLabel = "Ref Number:";

        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        [ObservableProperty]
        private string _extraFields = string.Empty;

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
            ExtraFields = component.ExtraFields ?? string.Empty;
            UpdateProperties();
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
            
            // Match Xamarin CountedHandler logic (lines 2240-2246): remove component if amount is 0
            if (value == 0)
            {
                _parent.PaymentComponents.Remove(this);
                _parent.SaveState();
                _parent.RefreshLabels();
                return;
            }
            
            _parent.OnComponentAmountChanged();
        }

        partial void OnRefChanged(string value)
        {
            _component.Ref = value;
        }

        partial void OnCommentsChanged(string value)
        {
            _component.Comments = value;
        }

        partial void OnBankNameChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _component.ExtraFields = UDFHelper.SyncSingleUDF("BankName", value, _component.ExtraFields);
                ExtraFields = _component.ExtraFields;
            }
            else
            {
                _component.ExtraFields = UDFHelper.RemoveSingleUDF("BankName", _component.ExtraFields);
                ExtraFields = _component.ExtraFields;
            }
        }

        partial void OnExtraFieldsChanged(string value)
        {
            if (_component != null)
                _component.ExtraFields = value ?? string.Empty;
        }

        partial void OnPostedDateChanged(DateTime value)
        {
            _component.PostedDate = value;
        }

        public void UpdateProperties()
        {
            PaymentMethodText = PaymentMethod.ToString().Replace("_", " ");
            AmountText = Amount.ToCustomString();
            ShowRef = PaymentMethod != InvoicePaymentMethod.Cash;
            ShowPostedDate = PaymentMethod == InvoicePaymentMethod.Check;
            ShowBank = PaymentMethod == InvoicePaymentMethod.Check || PaymentMethod == InvoicePaymentMethod.Credit_Card;
            
            // Set the label text based on payment method
            RefNumberLabel = PaymentMethod switch
            {
                InvoicePaymentMethod.Credit_Card => "Credit Card Number:",
                InvoicePaymentMethod.Check => "Check Number:",
                _ => "Reference Number:"
            };

            // Update image properties (matches Xamarin lines 1717-1740)
            HasImage = false;
            ImagePath = string.Empty;

            // Sync ExtraFields from component
            ExtraFields = _component.ExtraFields ?? string.Empty;

            if (!string.IsNullOrEmpty(ExtraFields) && ExtraFields.Contains("Image"))
            {
                var uniqueId = UDFHelper.GetSingleUDF("Image", ExtraFields);
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    var imagePath = Path.Combine(Config.PaymentImagesPath, uniqueId);
                    if (File.Exists(imagePath))
                    {
                        ImagePath = imagePath;
                        HasImage = true;
                    }
                }
            }
        }
    }
}

