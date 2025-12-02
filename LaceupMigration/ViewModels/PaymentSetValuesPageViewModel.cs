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

            if (PaymentComponents.Count == 0 && _amount > 0 && _paymentId == 0)
            {
                PaymentComponents.Add(new PaymentComponentViewModel(new PaymentComponent { Amount = _amount }, this));
            }

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

        private void RefreshLabels()
        {
            var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
            var open = _amount - currentPaymentAmount;

            AmountLabel = (_amount + _totalDiscount).ToCustomString();
            OpenLabel = open.ToCustomString();
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
        }

        public async Task EditPayment(PaymentComponentViewModel component)
        {
            if (component == null)
                return;

            var paymentMethods = Enum.GetNames(typeof(InvoicePaymentMethod))
                .Select(x => x.Replace("_", " "))
                .ToArray();

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
            }
        }

        [RelayCommand]
        private async Task SavePayment()
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
                        return;
                    }
                }
            }

            if (Config.MustEnterPostedDate)
            {
                var checkPayments = PaymentComponents.Where(x => x.PaymentMethod == InvoicePaymentMethod.Check).ToList();
                if (checkPayments.Any(x => x.PostedDate == DateTime.MinValue))
                {
                    await _dialogService.ShowAlertAsync("You must enter a Posted Date for Check Payments.", "Alert", "OK");
                    return;
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
                component.ExtraFields = DataAccess.SyncSingleUDF("location", 
                    $"{DataAccess.LastLatitude},{DataAccess.LastLongitude}", 
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

            await FinishProcessAsync();
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
                _component.ExtraFields = DataAccess.SyncSingleUDF("BankName", value, _component.ExtraFields);
            else
                _component.ExtraFields = DataAccess.RemoveSingleUDF("BankName", _component.ExtraFields);
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
            ShowBank = PaymentMethod == InvoicePaymentMethod.Check || PaymentMethod == InvoicePaymentMethod.Credit_Card;
        }
    }
}

