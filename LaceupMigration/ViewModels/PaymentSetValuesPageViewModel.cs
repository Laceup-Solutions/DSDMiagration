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
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LaceupMigration.ViewModels
{
    public partial class PaymentSetValuesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Client? _client;
        public InvoicePayment? _invoicePayment;
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
        
        public readonly List<PaymentComponentViewModel> _componentsToRemove = new();

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
        private bool _canSendByEmail = true;

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

                // Button states will be set by UpdateButtonStates() below
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

            // Set button states based on payment status
            UpdateButtonStates();
            
            // Update read-only state for all payment components
            UpdatePaymentComponentsReadOnlyState();

            RefreshLabels();
        }
        
        private void UpdatePaymentComponentsReadOnlyState()
        {
            // Set IsReadOnly to true if payment is printed, false otherwise
            var isReadOnly = _invoicePayment != null && _invoicePayment.Printed;
            foreach (var component in PaymentComponents)
            {
                component.IsReadOnly = isReadOnly;
                // Payment components should ALWAYS be visible, never hidden
                component.IsVisible = true;
            }
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
            if (_invoicePayment != null && _invoicePayment.Printed)
            {
                CanAddPayment = false; // Can't add if printed
            }
            else
            {
                CanAddPayment = !hasEmptyComponents && 
                               (_creditAccount || ((_invoicePayment == null || !_invoicePayment.Printed) && open > 0));
            }
            
            // Update other button states (Save, Delete, Print)
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // If opening an existing payment (paymentId > 0), Print and Send By Email should always be enabled
            if (_paymentId > 0)
            {
                CanPrintPayment = true;
                CanSendByEmail = true;
            }
            
            // If payment is printed, disable editing buttons (matches Xamarin behavior)
            // Reload payment from list if needed to ensure we have the latest state
            if (_invoicePayment != null && _invoicePayment.Id > 0)
            {
                // Reload payment from list to ensure we have the latest Printed status
                var reloadedPayment = InvoicePayment.List.FirstOrDefault(x => x.Id == _invoicePayment.Id);
                if (reloadedPayment != null)
                {
                    _invoicePayment = reloadedPayment; // Update reference to reloaded payment
                }
            }
            
            if (_invoicePayment != null && _invoicePayment.Printed)
            {
                CanSavePayment = false;
                CanDeletePayment = false;
                // Print and Send By Email buttons remain enabled (can re-print/re-send)
                CanPrintPayment = true;
                CanSendByEmail = true;
            }
            else
            {
                // If payment exists but not printed, can save and delete
                if (_invoicePayment != null)
                {
                    CanSavePayment = true;
                    CanDeletePayment = true;
                    CanPrintPayment = true; // Can print after saving
                    CanSendByEmail = true; // Can send by email
                }
                else
                {
                    // No payment saved yet, but check if there are payment components with amounts
                    var hasPaymentAmounts = PaymentComponents.Any(x => x.Amount > 0);
                    var hasPaymentComponents = PaymentComponents.Count > 0;
                    
                    CanSavePayment = true;
                    // Delete button is enabled when there are payment components (user can clear them) or when there's a saved payment
                    // This allows users to clear unsaved payment components they've entered
                    CanDeletePayment = hasPaymentComponents;
                    
                    // Enable Print and Send By Email if there are payment amounts (will prompt to save if needed)
                    // This matches Xamarin behavior - Print is enabled when there's a payment amount to print
                    CanPrintPayment = hasPaymentAmounts || _paymentId > 0;
                    CanSendByEmail = true; // Can send by email (will save first if needed)
                }
            }
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

        public async Task SelectPaymentMethodForComponentAsync(PaymentComponentViewModel component)
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
            component.PaymentMethod = method;

            RefreshLabels();
            SaveState(); // Save state after changing payment method
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
            SaveState(); // Save state after editing payment
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
        public async Task TakePhotoForComponentAsync(PaymentComponentViewModel component)
        {
            if (component == null)
                return;
            
            // If read-only but has image, show the image
            if (component.IsReadOnly && component.HasImage)
            {
                await ViewImageAsync(component);
                return;
            }
            
            // If read-only without image, do nothing
            if (component.IsReadOnly)
                return;

            try
            {
                // Show action sheet: Take Photo or Choose from Gallery
                var options = new[] { "Take Photo", "Choose from Gallery" };
                var choice = await _dialogService.ShowActionSheetAsync("Add Image", "Cancel", null, options);
                
                if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                    return;

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
                else if (choice == "Choose from Gallery")
                {
                    photo = await MediaPicker.PickPhotoAsync();
                }

                if (photo == null)
                    return;

                // Save photo to PaymentImagesPath
                var imageId = Guid.NewGuid().ToString("N");
                var filePath = Path.Combine(Config.PaymentImagesPath, $"{imageId}.png");

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Delete old image if exists
                if (component.HasImage && !string.IsNullOrEmpty(component.ImagePath) && File.Exists(component.ImagePath))
                {
                    try
                    {
                        File.Delete(component.ImagePath);
                    }
                    catch { /* Ignore deletion errors */ }
                }

                using (var sourceStream = await photo.OpenReadAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await sourceStream.CopyToAsync(fileStream);
                }

                // Store image path in component's ExtraFields
                component.Component.ExtraFields = DataAccess.SyncSingleUDF("Image", filePath, component.Component.ExtraFields ?? string.Empty);
                component.ImagePath = filePath;
                component.HasImage = true;

                RefreshLabels();
                SaveState(); // Save state after adding image
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error taking/selecting photo: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error taking/selecting photo.", "Error", "OK");
            }
        }

        public async Task ViewImageAsync(PaymentComponentViewModel component)
        {
            if (component == null || !component.HasImage || string.IsNullOrEmpty(component.ImagePath))
                return;

            if (!File.Exists(component.ImagePath))
            {
                await _dialogService.ShowAlertAsync("Image file not found.", "Error", "OK");
                return;
            }

            // Show image in a simple alert (or navigate to image viewer page if available)
            await _dialogService.ShowAlertAsync($"Image: {Path.GetFileName(component.ImagePath)}", "Image", "OK");
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
                        await _dialogService.ShowAlertAsync("You must select a Bank to be able to save this payment.",
                            "Alert", "OK");
                        return false;
                    }
                }
            }

            if (Config.MustEnterPostedDate)
            {
                var checkPayments = PaymentComponents.Where(x => x.PaymentMethod == InvoicePaymentMethod.Check)
                    .ToList();
                if (checkPayments.Any(x => x.PostedDate == DateTime.MinValue))
                {
                    await _dialogService.ShowAlertAsync("You must enter a Posted Date for Check Payments.", "Alert",
                        "OK");
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

            // FIXED: Always save all components from PaymentComponents
            // The UI-level removal (OnAmountChanged) already handles removing zero-amount components
            // when appropriate (i.e., when payment is not printed)
            _invoicePayment.Components.AddRange(PaymentComponents.Select(x => x.Component));

            // Clean up zero-amount components from the UI collection after saving
            // This only removes from PaymentComponents if payment is NOT printed
            RemoveZeroAmountComponents();

            // Add location to payments
            foreach (var component in _invoicePayment.Components)
            {
                component.ExtraFields = DataAccess.SyncSingleUDF("location",
                    $"{DataAccess.LastLatitude},{DataAccess.LastLongitude}",
                    component.ExtraFields);
            }

            _invoicePayment.Save();

            // Update _paymentId after saving so the payment can be found later
            if (_paymentId == 0 && _invoicePayment.Id > 0)
            {
                _paymentId = _invoicePayment.Id;
            }

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

                    // If payment was deleted, reset state
                    _invoicePayment = null;
                    _paymentId = 0;
                }
            }

            // Update button states after saving (if payment is printed, disable editing buttons)
            if (_invoicePayment != null && _invoicePayment.Printed)
            {
                CanAddPayment = false;
                CanSavePayment = false;
                CanDeletePayment = false;
                // Print and Send By Email remain enabled
                CanPrintPayment = true;
                CanSendByEmail = true;

                // Update read-only state for all payment components (disable editing)
                UpdatePaymentComponentsReadOnlyState();
            }
            else
            {
                // If not printed, refresh labels to update CanAddPayment based on open amount
                // This will also update button states including CanDeletePayment
                RefreshLabels();
            }

            return true;
        }

        // private async Task<bool> SavePaymentInternal()
        // {
        //     if (Config.PaymentBankIsMandatory)
        //     {
        //         foreach (var component in PaymentComponents)
        //         {
        //             if (string.IsNullOrEmpty(component.BankName) && 
        //                 (component.PaymentMethod == InvoicePaymentMethod.Check || 
        //                  component.PaymentMethod == InvoicePaymentMethod.Credit_Card))
        //             {
        //                 await _dialogService.ShowAlertAsync("You must select a Bank to be able to save this payment.", "Alert", "OK");
        //                 return false;
        //             }
        //         }
        //     }
        //
        //     if (Config.MustEnterPostedDate)
        //     {
        //         var checkPayments = PaymentComponents.Where(x => x.PaymentMethod == InvoicePaymentMethod.Check).ToList();
        //         if (checkPayments.Any(x => x.PostedDate == DateTime.MinValue))
        //         {
        //             await _dialogService.ShowAlertAsync("You must enter a Posted Date for Check Payments.", "Alert", "OK");
        //             return false;
        //         }
        //     }
        //
        //     var currentPaymentAmount = PaymentComponents.Sum(x => x.Amount);
        //     var open = _amount - currentPaymentAmount;
        //
        //     if (_invoicePayment == null)
        //     {
        //         _invoicePayment = new InvoicePayment(_client!);
        //         _invoicePayment.InvoicesId = _invoicesId;
        //
        //         if (!string.IsNullOrEmpty(_ordersId))
        //         {
        //             _invoicePayment.OrderId = string.Empty;
        //             foreach (var idAsString in _ordersId.Split(','))
        //             {
        //                 var id = Convert.ToInt32(idAsString);
        //                 var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
        //                 if (order != null)
        //                 {
        //                     if (!string.IsNullOrEmpty(_invoicePayment.OrderId))
        //                         _invoicePayment.OrderId += ",";
        //                     _invoicePayment.OrderId += order.UniqueId;
        //                 }
        //             }
        //         }
        //     }
        //
        //     _invoicePayment.DiscountApplied = _totalDiscount;
        //     _invoicePayment.Components.Clear();
        //     
        //     // Filter out components with zero amount when saving
        //     // Exception: For printed payments, include all components (even with 0 amount) to preserve state
        //     var isPaymentPrinted = _invoicePayment.Printed;
        //     if (isPaymentPrinted)
        //     {
        //         // For printed payments, include all components to preserve the exact state
        //         _invoicePayment.Components.AddRange(PaymentComponents.Select(x => x.Component));
        //     }
        //     else
        //     {
        //         // For non-printed payments, only include components with amount > 0
        //         _invoicePayment.Components.AddRange(PaymentComponents.Where(x => x.Amount > 0).Select(x => x.Component));
        //     }
        //     
        //     // Clean up zero-amount components from the collection after saving
        //     // This will only remove them if payment is NOT printed
        //     RemoveZeroAmountComponents();
        //
        //     // Add location to payments
        //     foreach (var component in _invoicePayment.Components)
        //     {
        //         component.ExtraFields = DataAccess.SyncSingleUDF("location", 
        //             $"{DataAccess.LastLatitude},{DataAccess.LastLongitude}", 
        //             component.ExtraFields);
        //     }
        //
        //     _invoicePayment.Save();
        //     
        //     // Update _paymentId after saving so the payment can be found later
        //     if (_paymentId == 0 && _invoicePayment.Id > 0)
        //     {
        //         _paymentId = _invoicePayment.Id;
        //     }
        //
        //     if (_invoicePayment.Components.All(x => x.Amount == 0))
        //     {
        //         var confirmed = await _dialogService.ShowConfirmationAsync("Empty Payment", 
        //             "This payment has no amount. Do you want to delete it?", "Yes", "No");
        //         if (confirmed)
        //         {
        //             if (Config.VoidPayments)
        //                 _invoicePayment.Void();
        //             else
        //                 _invoicePayment.Delete();
        //             
        //             // If payment was deleted, reset state
        //             _invoicePayment = null;
        //             _paymentId = 0;
        //         }
        //     }
        //
        //     // Update button states after saving (if payment is printed, disable editing buttons)
        //     if (_invoicePayment != null && _invoicePayment.Printed)
        //     {
        //         CanAddPayment = false;
        //         CanSavePayment = false;
        //         CanDeletePayment = false;
        //         // Print and Send By Email remain enabled
        //         CanPrintPayment = true;
        //         CanSendByEmail = true;
        //         
        //         // Update read-only state for all payment components (disable editing)
        //         UpdatePaymentComponentsReadOnlyState();
        //     }
        //     else
        //     {
        //         // If not printed, refresh labels to update CanAddPayment based on open amount
        //         // This will also update button states including CanDeletePayment
        //         RefreshLabels();
        //     }
        //
        //     return true;
        // }

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
                    // Print and Send By Email remain enabled
                    CanPrintPayment = true;
                    CanSendByEmail = true;
                    
                    // Update read-only state for all payment components (disable editing)
                    UpdatePaymentComponentsReadOnlyState();
                    
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
                toEmail = DataAccess.GetSingleUDF("email", _invoicePayment.Client.ExtraPropertiesAsString);
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
            // If there's a saved payment, delete it
            if (_invoicePayment != null)
            {
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
            // If no saved payment but there are payment components, clear them
            else if (PaymentComponents.Count > 0)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Clear Payment", 
                    "Are you sure you want to clear all payment components?", "Yes", "No");
                if (confirmed)
                {
                    PaymentComponents.Clear();
                    RefreshLabels();
                    SaveState(); // Save state after clearing
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("No payment to delete.", "Alert", "OK");
            }
        }

        [RelayCommand]
        private async Task PrintPayment()
        {
            // Check if payment needs to be saved first
            if (_invoicePayment == null || !_invoicePayment.Printed)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Print Before Finalize", 
                    "You must save the payment before printing. Save now?", "Yes", "No");
                if (confirmed)
                {
                    // Use SavePaymentInternal instead of SavePayment to avoid navigating away
                    // This matches the pattern used in SendByEmail
                    if (!await SavePaymentInternal())
                        return; // Save failed, don't proceed with printing
                    
                    // Verify payment was saved and is still valid
                    if (_invoicePayment == null)
                        return;
                }
                else
                {
                    return; // User cancelled, don't proceed
                }
            }

            // Verify payment is still valid before proceeding
            if (_invoicePayment == null)
                return;

            // Handle company selection (matches Xamarin behavior)
            if (CompanyInfo.Companies.Count > 1)
            {
                var companyNames = CompanyInfo.Companies.Select(x => x.CompanyName).ToArray();
                var selectedIndex = await _dialogService.ShowSelectionAsync("Select Company", companyNames);
                if (selectedIndex >= 0 && selectedIndex < CompanyInfo.Companies.Count)
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[selectedIndex];
                }
                else
                {
                    // User cancelled company selection
                    return;
                }
            }
            else if (CompanyInfo.Companies.Count > 0)
            {
                // Only one company, use it automatically
                CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
            }

            // Final verification before showing print dialog
            if (_invoicePayment == null)
                return;

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    // Verify payment is still valid inside the print callback
                    if (_invoicePayment == null)
                        return "Payment no longer available";

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
                    
                    // Update button states after printing (matches Xamarin behavior)
                    // Must update on main thread to ensure UI updates correctly
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Reload payment data to ensure we have the latest invoice/order amounts
                        // This is needed to correctly calculate the Open label after printing
                        // Note: LoadExistingPaymentData() doesn't clear PaymentComponents, it only updates _amount
                        LoadExistingPaymentData();
                        
                        // Reload payment components from saved payment to ensure we have the latest state
                        // This ensures components stay visible after printing (matching Xamarin behavior)
                        // Components are reloaded so they reflect the saved state and remain visible
                        PaymentComponents.Clear();
                        foreach (var component in _invoicePayment!.Components)
                        {
                            var componentViewModel = new PaymentComponentViewModel(component, this);
                            // Payment components should ALWAYS be visible
                            componentViewModel.IsVisible = true;
                            // Set read-only if payment is printed
                            if (_invoicePayment.Printed)
                            {
                                componentViewModel.IsReadOnly = true;
                            }
                            PaymentComponents.Add(componentViewModel);
                        }
                        
                        // Explicitly disable editing buttons after printing
                        CanAddPayment = false;
                        CanSavePayment = false;
                        CanDeletePayment = false;
                        // Print and Send By Email buttons remain enabled for re-printing/re-sending
                        CanPrintPayment = true;
                        CanSendByEmail = true;
                        
                        // Update read-only state for all payment components (disable editing)
                        // This ensures components are visible but disabled (matching Xamarin behavior)
                        // This is called again as a safeguard to ensure all components are properly set
                        UpdatePaymentComponentsReadOnlyState();
                        
                        // Refresh labels to update amounts (including Open label), then update button states
                        RefreshLabels();
                        // Explicitly call UpdateButtonStates again to ensure delete button is disabled
                        // This ensures the state is correct even if RefreshLabels has any side effects
                        UpdateButtonStates();
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

        public void RemoveZeroAmountComponents()
        {
            // Remove components with zero amount, but ONLY if payment is NOT printed
            // For printed payments, keep all components visible (even with 0 amount)
            var isPaymentPrinted = _invoicePayment != null && _invoicePayment.Printed;
            
            if (isPaymentPrinted)
            {
                // Don't remove components if payment is printed - they should stay visible
                return;
            }
            
            // Only remove zero-amount components if payment is not printed
            var componentsToRemove = PaymentComponents
                .Where(x => Math.Abs(x.Amount) < 0.001)
                .ToList();

            foreach (var component in componentsToRemove)
            {
                PaymentComponents.Remove(component);
            }

            if (componentsToRemove.Count > 0)
            {
                RefreshLabels();
                SaveState();
            }
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
                                extraFields = DataAccess.SyncSingleUDF("PostedDate", component.PostedDate.Ticks.ToString(), extraFields);
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
                                component.ExtraFields = DataAccess.SyncSingleUDF("BankName", bankName, component.ExtraFields ?? string.Empty);
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
                                var postedDate = DataAccess.GetSingleUDF("PostedDate", component.ExtraFields);
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
        private bool _isUpdatingFromAmountText = false;
        private bool _isProcessingTextChange = false;
        
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
        private bool _isVisible = true;

        [ObservableProperty]
        private bool _isReadOnly = false;

        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        public PaymentComponentViewModel(PaymentComponent component, PaymentSetValuesPageViewModel parent)
        {
            _component = component;
            _parent = parent;
            
            // Check if payment is printed FIRST, before setting Amount (which triggers OnAmountChanged)
            // This ensures IsVisible is set correctly for printed payments
            var isPaymentPrinted = _parent._invoicePayment != null && _parent._invoicePayment.Printed;
            
            PaymentMethod = component.PaymentMethod;
            // Set Amount - this will trigger OnAmountChanged, but we've already checked if printed
            Amount = component.Amount;
            Ref = component.Ref ?? string.Empty;
            Comments = component.Comments ?? string.Empty;
            BankName = component.BankName ?? string.Empty;
            PostedDate = component.PostedDate;
            
            // Set initial visibility and read-only state
            // Payment components should ALWAYS be visible, never hidden
            IsVisible = true;
            
            if (isPaymentPrinted)
            {
                // For printed payments, set read-only immediately
                IsReadOnly = true;
            }
            
            // Load image path from ExtraFields
            if (!string.IsNullOrEmpty(component.ExtraFields))
            {
                var imagePath = DataAccess.GetSingleUDF("Image", component.ExtraFields);
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    ImagePath = imagePath;
                    HasImage = true;
                }
            }
            
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
            // Only update AmountText if not updating from AmountText to avoid circular updates
            // Also format to 2 decimal places
            if (!_isUpdatingFromAmountText)
            {
                var formattedValue = value.ToString("F2");
                // Only update if different to avoid triggering text change during typing
                if (AmountText != formattedValue)
                {
                    AmountText = formattedValue;
                }
            }
            
            // Check if payment is printed
            var isPaymentPrinted = _parent._invoicePayment != null && _parent._invoicePayment.Printed;
            
            // If amount is 0 and payment is NOT printed: remove the component
            // If amount is 0 and payment IS printed: keep it visible but disabled
            if (Math.Abs(value) < 0.001 && !isPaymentPrinted)
            {
                // Only remove if component is already in the collection (not during initialization)
                // Use MainThread to avoid UI update issues
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Double-check that component is still in collection and payment is still not printed
                    // (payment might have been printed between the check and the removal)
                    if (_parent.PaymentComponents.Contains(this) && 
                        (_parent._invoicePayment == null || !_parent._invoicePayment.Printed))
                    {
                        _parent.PaymentComponents.Remove(this);
                        _parent.RefreshLabels();
                        _parent.SaveState();
                    }
                });
                return; // Exit early since component will be removed
            }
            
            // For printed payments or non-zero amounts: keep component visible
            IsVisible = true;
            
            _parent.OnComponentAmountChanged();
        }

        partial void OnAmountTextChanged(string value)
        {
            // Don't process if we're updating from Amount to avoid circular updates
            if (_isUpdatingFromAmountText || _isProcessingTextChange)
                return;

            try
            {
                _isProcessingTextChange = true;

                // Handle empty or whitespace - defer to unfocus handler to avoid EmojiCompat errors
                // Don't process empty text during typing - let user finish editing
                if (string.IsNullOrWhiteSpace(value))
                {
                    // Don't update during typing - will be handled on unfocus
                    return;
                }

                // Parse the formatted string back to double
                // Only process if the text is a valid number to avoid errors during typing
                if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var amount))
                {
                    // Only update if different to avoid infinite loop
                    if (Math.Abs(Amount - amount) > 0.001)
                    {
                        _isUpdatingFromAmountText = true;
                        Amount = amount; // This will trigger OnAmountChanged
                        _isUpdatingFromAmountText = false;
                    }
                }
                // If parsing fails, don't update - let the user continue typing
            }
            finally
            {
                _isProcessingTextChange = false;
            }
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
            AmountText = Amount.ToString("F2");
            ShowRef = PaymentMethod != InvoicePaymentMethod.Cash;
            ShowPostedDate = PaymentMethod == InvoicePaymentMethod.Check;
            ShowBank = PaymentMethod == InvoicePaymentMethod.Check || PaymentMethod == InvoicePaymentMethod.Credit_Card;
        }

        [RelayCommand]
        private async Task SelectPaymentMethodAsync()
        {
            await _parent.SelectPaymentMethodForComponentAsync(this);
        }
    }
}

