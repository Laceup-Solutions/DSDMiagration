using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using LaceupMigration.UtilDlls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class FinalizeBatchPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private string _ordersId = string.Empty;
        private List<Order> _orders = new();
        private Client? _client;
        private Batch? _batch;
        private double _balance = 0;
        private bool _printed = false;
        private bool _canLeaveScreen = true;

        [ObservableProperty]
        private bool _showSignatureButton = true;

        [ObservableProperty]
        private bool _isSignatureEnabled = true;

        [ObservableProperty]
        private bool _showPaymentButton = true;

        [ObservableProperty]
        private bool _isPaymentEnabled = true;

        [ObservableProperty]
        private bool _showPrintButton = true;

        [ObservableProperty]
        private bool _showDoneButton = true;

        [ObservableProperty]
        private bool _showSendByEmailButton = false;

        [ObservableProperty]
        private bool _showPrintProofOfDeliveryButton = false;

        [ObservableProperty]
        private bool _showPrintPickTicketButton = false;

        // Public property for CanLeaveScreen (used by OnBackButtonPressed)
        // Xamarin: canLeaveScreen is false if payment was collected or printing was done
        public bool CanLeaveScreen => _canLeaveScreen && !_printed && !HasPayment();

        public FinalizeBatchPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(string ordersId, int clientId, bool printed = false)
        {
            _ordersId = ordersId;
            _printed = printed;

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            
            // Parse order IDs
            var orderIds = new List<int>();
            foreach (var idAsString in ordersId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(idAsString, out var id))
                {
                    orderIds.Add(id);
                }
            }

            if (orderIds.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No valid orders found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Load orders
            _orders = Order.Orders.Where(x => orderIds.Contains(x.OrderId)).ToList();
            if (_orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Orders not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Get batch from first order
            _batch = Batch.List.FirstOrDefault(x => x.Id == _orders[0].BatchId);
            if (_batch == null)
            {
                await _dialogService.ShowAlertAsync("Batch not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Calculate balance
            _balance = 0;
            bool paymentCanBeCollected = false;
            bool atLeastOneBill = false;

            foreach (var order in _orders)
            {
                if (order.OrderType == OrderType.Order || order.OrderType == OrderType.WorkOrder)
                    paymentCanBeCollected = true;

                if (order.OrderType == OrderType.Consignment)
                {
                    if (Config.UseFullConsignment)
                        paymentCanBeCollected = true;
                    else if (Config.UseBattery && order.OrderTotalCost() > 0)
                        paymentCanBeCollected = true;
                    else if (!Config.UseBattery && order.Details.Any(x => x.ConsignmentCounted && x.ConsignmentOld != x.ConsignmentCount))
                        paymentCanBeCollected = true;
                }

                if (order.OrderType == OrderType.Bill)
                    atLeastOneBill = true;

                _balance += Config.DisolCrap ? order.DisolOrderTotalCost() : order.OrderTotalCost();
            }

            // Set button visibility and states
            ShowSignatureButton = true;
            IsSignatureEnabled = !_printed;

            ShowPaymentButton = _batch.Client.AllowToCollectPayment && !Config.HidePriceInTransaction;
            if (atLeastOneBill || !paymentCanBeCollected)
                ShowPaymentButton = false;

            IsPaymentEnabled = _balance > 0;

            ShowPrintButton = Config.PrinterAvailable;
            if (Config.UseSendByEmail)
                ShowPrintButton = false;

            ShowSendByEmailButton = Config.UseSendByEmail || Config.SendByEmailInFinalize;
            ShowPrintProofOfDeliveryButton = Config.UsePrintProofDelivery;
            ShowPrintPickTicketButton = Config.ButlerCustomization || Config.ShowPrintPickTicket;

            // Save navigation state
            Helpers.NavigationHelper.SaveNavigationState($"finalizebatch?ordersId={Uri.EscapeDataString(ordersId)}&printed={(_printed ? "1" : "0")}");
        }

        public async Task OnAppearingAsync()
        {
            // Refresh button states based on current order/payment status
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // Check if signature exists
            bool hasSignature = false;
            foreach (var order in _orders)
            {
                if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                {
                    hasSignature = true;
                    break;
                }
            }
            IsSignatureEnabled = !_printed && !hasSignature;

            // Check if payment exists
            if (HasPayment())
            {
                IsPaymentEnabled = false; // Disable if payment already collected
            }
        }

        private bool HasPayment()
        {
            foreach (var order in _orders)
            {
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment != null)
                {
                    return true;
                }
            }
            return false;
        }

        [RelayCommand]
        private async Task SignatureAsync()
        {
            _appService.RecordEvent("Signature button");

            // Xamarin: Navigate to OrderSignatureActivity with ordersId
            // The signature will be saved to all orders in the list
            await Shell.Current.GoToAsync($"ordersignature?ordersId={Uri.EscapeDataString(_ordersId)}");
            
            // After returning, update button states
            UpdateButtonStates();
        }

        [RelayCommand]
        private async Task PaymentAsync()
        {
            try
            {
                _appService.RecordEvent("Payment button");

                // Match Xamarin FinalizeBatchActivity.PaymentButton_Click: pass ordersId and coming-from-finalize.
                // PaymentSetValuesPage expects ordersId (or orderIds), clientId; amount/details come from loading those orders.
                await Shell.Current.GoToAsync($"paymentsetvalues?ordersId={Uri.EscapeDataString(_ordersId)}&clientId={_client.ClientId}&fromFinalize=true");
                
                // After returning, update button states
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                _appService.TrackError(ex);
                await _dialogService.ShowAlertAsync($"Error opening payment: {ex.Message}", "Error", "OK");
            }
        }

        [RelayCommand]
        private async Task PrintAsync()
        {
            _appService.RecordEvent("Print button");

            if (!await CheckConditionsAsync(false, true))
                return;

            await FinishPrintAsync();
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            if (!await CheckConditionsAsync(true))
                return;

            await FinishFinalizeAsync();
        }

        [RelayCommand]
        private async Task SendByEmailAsync()
        {
            _appService.RecordEvent("Send by Email button");

            if (!await CheckConditionsAsync())
                return;

            await FinishSendByEmailAsync();
        }

        [RelayCommand]
        private async Task PrintProofOfDeliveryAsync()
        {
            try
            {
                PrinterProvider.PrintDocument((int numberOfCopies) =>
                {
                    if (numberOfCopies <= 0)
                        return "Please enter a valid number of copies.";

                    var printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < numberOfCopies; i++)
                    {
                        foreach (var order in _orders)
                        {
                            if (!printer.PrintProofOfDelivery(order))
                            {
                                allWent = false;
                                break;
                            }
                        }
                        if (!allWent) break;
                    }

                    if (!allWent)
                        return "At least one order failed to print.";

                    return string.Empty;
                }, Config.PrintCopiesInFinalizeBatch);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing proof of delivery: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task PrintPickTicketAsync()
        {
            try
            {
                PrinterProvider.PrintDocument((int numberOfCopies) =>
                {
                    if (numberOfCopies <= 0)
                        return "Please enter a valid number of copies.";

                    var printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < numberOfCopies; i++)
                    {
                        foreach (var order in _orders)
                        {
                            if (!printer.PrintPickTicket(order))
                            {
                                allWent = false;
                                break;
                            }
                        }
                        if (!allWent) break;
                    }

                    if (!allWent)
                        return "At least one order failed to print.";

                    return string.Empty;
                }, Config.PrintCopiesInFinalizeBatch);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing pick ticket: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task<bool> CheckConditionsAsync(bool checkPrint = false, bool fromPrint = false)
        {
            // Check if printing is required
            if (checkPrint && !_printed && Config.PrintingRequired)
            {
                await _dialogService.ShowAlertAsync("You must print the order before leaving.", "Alert", "OK");
                return false;
            }

            // Check signature
            bool signed = !IsSignatureEnabled;
            foreach (var order in _orders)
            {
                if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                {
                    signed = true;
                    break;
                }
            }

            if (ShowSignatureButton && IsSignatureEnabled && !signed && Config.SignatureRequired)
            {
                await _dialogService.ShowAlertAsync("Customer signature is required.", "Alert", "OK");
                return false;
            }

            // Check payment
            bool paid = !IsPaymentEnabled;
            bool paidInFull = false;

            foreach (var order in _orders)
            {
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment == null)
                    break;
                else
                {
                    paid = true;
                    paidInFull = payment.TotalPaid >= _balance;
                    break;
                }
            }

            if (_balance > 0 && (!paid || !paidInFull))
            {
                if (Config.UsesTerms)
                {
                    var order = _orders.FirstOrDefault();
                    if (order != null)
                    {
                        var terms = order.Term.ToUpperInvariant().Trim();
                        if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                            || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO" || terms == "CCOD" || terms == "C.C.O.D.")
                        {
                            await _dialogService.ShowAlertAsync("You must collect full payment.", "Alert", "OK");
                            return false;
                        }
                    }
                }

                if (!paid)
                {
                    if (Config.PaymentRequired)
                    {
                        await _dialogService.ShowAlertAsync("You must collect payment.", "Alert", "OK");
                        return false;
                    }

                    if (Config.RemoveWarnings)
                    {
                        if (fromPrint)
                            await FinishPrintAsync();
                        else
                            await FinishFinalizeAsync();
                        return false;
                    }

                    if (_batch != null && _batch.Client.AllowToCollectPayment && !Config.PaymentOrSignatureRequired)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "You are not collecting payment. Continue anyway?",
                            "Warning",
                            "Yes",
                            "No");

                        if (!result)
                            return false;
                    }
                }
            }

            if (Config.PaymentOrSignatureRequired && !signed && (!paid || !paidInFull))
            {
                await _dialogService.ShowAlertAsync("You must collect signature or payment.", "Alert", "OK");
                return false;
            }

            return true;
        }

        private bool CheckConditions(bool checkPrint = false, bool fromPrint = false)
        {
            // Synchronous wrapper - fire and forget for alerts
            if (checkPrint && !_printed && Config.PrintingRequired)
            {
                _ = _dialogService.ShowAlertAsync("You must print the order before leaving.", "Alert", "OK");
                return false;
            }

            // Check signature
            bool signed = !IsSignatureEnabled;
            foreach (var order in _orders)
            {
                if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                {
                    signed = true;
                    break;
                }
            }

            if (ShowSignatureButton && IsSignatureEnabled && !signed && Config.SignatureRequired)
            {
                _ = _dialogService.ShowAlertAsync("Customer signature is required.", "Alert", "OK");
                return false;
            }

            // Check payment
            bool paid = !IsPaymentEnabled;
            bool paidInFull = false;

            foreach (var order in _orders)
            {
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment == null)
                    break;
                else
                {
                    paid = true;
                    paidInFull = payment.TotalPaid >= _balance;
                    break;
                }
            }

            if (_balance > 0 && (!paid || !paidInFull))
            {
                if (Config.UsesTerms)
                {
                    var order = _orders.FirstOrDefault();
                    if (order != null)
                    {
                        var terms = order.Term.ToUpperInvariant().Trim();
                        if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                            || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO" || terms == "CCOD" || terms == "C.C.O.D.")
                        {
                            _ = _dialogService.ShowAlertAsync("You must collect full payment.", "Alert", "OK");
                            return false;
                        }
                    }
                }

                if (!paid)
                {
                    if (Config.PaymentRequired)
                    {
                        _ = _dialogService.ShowAlertAsync("You must collect payment.", "Alert", "OK");
                        return false;
                    }

                    if (Config.RemoveWarnings)
                    {
                        if (fromPrint)
                            _ = FinishPrintAsync();
                        else
                            _ = FinishFinalizeAsync();
                        return false;
                    }
                }
            }

            if (Config.PaymentOrSignatureRequired && !signed && (!paid || !paidInFull))
            {
                _ = _dialogService.ShowAlertAsync("You must collect signature or payment.", "Alert", "OK");
                return false;
            }

            return true;
        }

        private async Task FinishPrintAsync()
        {
            try
            {
                // Check signature requirement for printing
                if (Config.SignatureRequired)
                {
                    var order = _orders.FirstOrDefault();
                    if (order != null && (order.SignaturePoints == null || order.SignaturePoints.Count == 0))
                    {
                        await _dialogService.ShowAlertAsync("Customer signature is required before printing.", "Alert", "OK");
                        return;
                    }
                }

                // Check payment requirement
                bool paymentRequired = false;
                var firstOrder = _orders.FirstOrDefault();
                if (firstOrder != null && firstOrder.Client.ExtraProperties != null)
                {
                    var terms = firstOrder.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "ARType");
                    if (terms != null)
                        paymentRequired = terms.Item2 == "CASH";
                }

                if (_balance > 0 && (Config.PaymentRequired || paymentRequired))
                {
                    foreach (var order in _orders)
                    {
                        var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                        if (payment == null)
                        {
                            await _dialogService.ShowAlertAsync("You must collect payment.", "Alert", "OK");
                            return;
                        }
                        else
                            break;
                    }
                }

                // Check ClientRtnNeededForQty
                if (Config.ClientRtnNeededForQty > 0)
                {
                    foreach (var order in _orders)
                    {
                        if (order != null && order.OrderTotalCost() >= Config.ClientRtnNeededForQty)
                        {
                            await _dialogService.ShowAlertAsync(
                                string.Format("Client RTN is required for orders over {0}.", Config.ClientRtnNeededForQty.ToCustomString()),
                                "Warning",
                                "OK");
                            // Continue to print anyway
                            break;
                        }
                    }
                }

                var numberOfCopies = Config.PrintCopiesInFinalizeBatch;
                if (firstOrder != null && firstOrder.Client != null && firstOrder.Client.CopiesPerInvoice > 0)
                    numberOfCopies = firstOrder.Client.CopiesPerInvoice;

                // Disable buttons during printing
                _canLeaveScreen = false;

                try
                {
                    PrinterProvider.PrintDocument((int copies) =>
                    {
                        if (copies < 1)
                            return "Please enter a valid number of copies.";

                        // Generate PrintedOrderId if needed
                        foreach (var order in _orders)
                        {
                            if (string.IsNullOrEmpty(order.PrintedOrderId))
                            {
                                order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                                order.Save();
                            }
                        }

                        if (!Config.GeneratePreorderNum)
                            _printed = true;

                        var printer = PrinterProvider.CurrentPrinter();
                        bool allWent = true;

                        for (var i = 0; i < copies; i++)
                        {
                            foreach (var order in _orders)
                            {
                                bool result = false;
                                if (order.OrderType == OrderType.Consignment)
                                {
                                    if (Config.UseFullConsignment)
                                    {
                                        result = printer.PrintFullConsignment(order, false);
                                        if (result)
                                            order.PrintedCopies += 1;
                                    }
                                    else
                                    {
                                        result = printer.PrintConsignment(order, false);
                                        if (result)
                                            order.PrintedCopies += 1;
                                    }
                                }
                                else
                                {
                                    result = printer.PrintOrder(order, false, true);
                                    if (result)
                                        order.PrintedCopies += 1;
                                }

                                if (!result)
                                    allWent = false;
                            }
                        }

                        if (!allWent)
                            return "At least one order failed to print.";

                        _printed = true;
                        IsSignatureEnabled = false;
                        IsPaymentEnabled = false;

                        return string.Empty;
                    }, numberOfCopies);
                }
                finally
                {
                    _canLeaveScreen = true;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task FinishFinalizeAsync()
        {
            try
            {
                foreach (var order in _orders)
                {
                    order.Finished = true;
                    order.EndDate = DateTime.Now;

                    // Check for loading error in detail and update inventory
                    if (!order.Reshipped && !order.Voided && order.IsDelivery)
                    {
                        foreach (var detail in order.Details)
                        {
                            if (detail.LoadingError)
                            {
                                var originalQty = detail.Ordered;
                                var currentQty = detail.Qty;
                                var remainingInventory = detail.Ordered - detail.Qty;

                                detail.Product.UpdateInventory(false, remainingInventory, detail.UnitOfMeasure, string.Empty, -1, detail.Weight);
                            }
                        }
                    }

                    if (order.ShipDate.Year == DateTime.MinValue.Year && RouteEx.Routes.Any(x => x.Order != null && x.FromDelivery && x.Order.OrderId == order.OrderId))
                    {
                        order.ShipDate = DateTime.Now;
                    }

                    if (order.OrderType == OrderType.Consignment && !Config.MagnoliaSetConsignment)
                        order.UpdateConsignmentInventory(-1);

                    if (string.IsNullOrEmpty(order.PrintedOrderId))
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);

                    order.Save();

                    if (Session.session != null)
                        Session.session.AddDetailFromOrder(order);
                }

                // Check if there is any payment, if so set it to printed to lock it
                var firstOrder = _orders.FirstOrDefault();
                if (firstOrder != null)
                {
                    var payment = InvoicePayment.List.FirstOrDefault(x => x.OrderId != null && x.OrderId.Contains(firstOrder.UniqueId));
                    if (payment != null && !payment.Printed)
                    {
                        payment.Printed = true;
                        payment.Save();
                    }
                }

                // Remove navigation state
                Helpers.NavigationHelper.RemoveNavigationState($"finalizebatch?ordersId={Uri.EscapeDataString(_ordersId)}&printed={(_printed ? "1" : "0")}");

                // Navigate back
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error saving order.", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task FinishSendByEmailAsync()
        {
            try
            {
                // Generate PrintedOrderId if needed
                foreach (var order in _orders)
                {
                    if (string.IsNullOrEmpty(order.PrintedOrderId))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                await PdfHelper.SendOrdersByEmail(_orders);

                if (!Config.SendByEmailInFinalize)
                {
                    _printed = true;
                    IsPaymentEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync($"Error sending email: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public async Task ShowCannotLeaveDialog()
        {
            // Xamarin: Shows alert if payment was collected or printing was done
            if (_printed || HasPayment())
            {
                await _dialogService.ShowAlertAsync("You must click Done to leave this screen.", "Alert", "OK");
            }
        }
    }
}

