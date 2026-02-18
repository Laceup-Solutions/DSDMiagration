using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndOfDayPageViewModel : ObservableObject
    {
        // EOD send state keys (match Xamarin EndOfDayActivity so we don't resend on retry)
        private const string OrdersSentIntent = "ordersSentIntent";
        private const string PaymentsSentIntent = "paymentsSentIntent";
        private const string LeftOverSentIntent = "leftOverSentIntent";
        private const string BuildToQtySentIntent = "buildToQtySentIntent";
        private const string DayReportSentIntent = "dayReportSentIntent";
        private const string LoadOrderSentIntent = "loadOrderSentIntent";
        private const string ParlevelSentIntent = "parlevelSentIntent";
        private const string DailyParlevelSentIntent = "dailyParlevelSentIntent";
        private const string TransfersSentIntent = "transfersSentIntent";
        private static readonly string SendAllTempFile = Path.Combine(Config.DataPath, "sendAllTempFile");

        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        
        // State tracking (matches Xamarin static variables)
        private bool _routeReturns = false;
        private bool _endInventory = false;
        private bool _reportsPrinted = false;
        private bool _clockedOut = false;
        private bool _isUsingMilagroPrinter = false;
        private bool _sentAll = false;
        private bool _loadOrder = false;
        private bool _parLevel = false;
        private bool _canLeaveScreen = true;

        // Public property for CanLeaveScreen (used by OnBackButtonPressed)
        public bool CanLeaveScreen => _canLeaveScreen;

        [ObservableProperty] private bool _showExpenses;
        [ObservableProperty] private bool _showRouteReturns;
        [ObservableProperty] private bool _showEndInventory;
        [ObservableProperty] private bool _showCycleCount;
        [ObservableProperty] private bool _showPrintReports;
        [ObservableProperty] private bool _showLoadOrder;
        [ObservableProperty] private bool _showSetParLevel;
        [ObservableProperty] private bool _showClockOut;

        // Button enabled states (inverse of completion - buttons are disabled when task is completed)
        [ObservableProperty] private bool _isRouteReturnsEnabled = true;
        [ObservableProperty] private bool _isEndInventoryEnabled = true;
        [ObservableProperty] private bool _isPrintReportsEnabled = true;
        [ObservableProperty] private bool _isClockOutEnabled = true;
        [ObservableProperty] private bool _isLoadOrderEnabled = true;
        [ObservableProperty] private bool _isSetParLevelEnabled = true;
        [ObservableProperty] private bool _isRouteExpensesEnabled = true;

        public EndOfDayPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
        }

        public async Task OnAppearingAsync()
        {
            // Set visibility based on Xamarin EndOfDayActivity logic
            ShowExpenses = Config.ShowExpensesInEOD;
            ShowRouteReturns = !Config.DisableRouteReturn;
            ShowEndInventory = !Config.EmptyTruckAtEndOfDay;
            ShowCycleCount = false; // Always hidden in Xamarin (ViewStates.Gone)
            ShowPrintReports = !Config.DisablePrintEndOfDayReport;
            ShowLoadOrder = Config.LoadRequest;
            ShowSetParLevel = Config.SetParLevel;
            ShowClockOut = Config.UseClockInOut || Config.TimeSheetCustomization;
            
            // Check if Milagro printer is being used
            _isUsingMilagroPrinter = Config.PrinterToUse?.ToLower().Contains("milagro") == true;
            
            // Check state from files
            var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
            _routeReturns = File.Exists(routeReturnFile);
            
            // Check if end inventory is completed
            // Use Config.EndingInventoryCounted (set when end inventory is saved)
            // Also check if EmptyTruckAtEndOfDay is enabled (auto-completes end inventory)
            _endInventory = Config.EndingInventoryCounted || Config.EmptyTruckAtEndOfDay;
            
            // Check if load order is completed (check if there's a load order that's finished or not pending)
            // _loadOrder = Order.Orders.Any(x => x.OrderType == OrderType.Load && (x.Finished || !x.PendingLoad));
            
            // Check if par level is completed (check if ParLevel file exists or if temp file doesn't exist)
            var parLevelFile = Config.ParLevelFile;
            var tempParLevelFile = Path.Combine(Config.DataPath, "temp_ParLevelPath.xml");
            _parLevel = File.Exists(parLevelFile) || (!File.Exists(tempParLevelFile) && ParLevel.List.Count == 0);
            
            // Check if clocked out using SalesmanSession
            _clockedOut = SalesmanSession.ClockedOut;
            
            // Check if reports were printed (check flag file)
            if (Config.DisablePrintEndOfDayReport)
            {
                _reportsPrinted = true;
            }
            else
            {
                var reportsPrintedFile = Path.Combine(Config.DataPath, "reportsPrinted.flag");
                _reportsPrinted = File.Exists(reportsPrintedFile);
            }
            
            // Auto-calculate route returns if disabled and not done
            if (!_routeReturns && Config.DisableRouteReturn)
            {
                AutoCalculateRouteReturn(false);
                if (Config.EmptyTruckAtEndOfDay)
                    _endInventory = true;
            }
            
            // Reset canLeaveScreen if End of Day hasn't been completed
            // canLeaveScreen is only true when End of Day completes successfully (Xamarin line 983)
            // Otherwise, if any action was started, it should be false
            if (!_sentAll)
            {
                // If any action was completed, canLeaveScreen should be false until End of Day completes
                // This prevents leaving the screen until End of Day is done
                if (_routeReturns || _endInventory || _reportsPrinted || _clockedOut || _loadOrder || _parLevel)
                {
                    _canLeaveScreen = false;
                }
            }
            
            // Update button enabled states (buttons are disabled when task is completed)
            // This matches Xamarin OnResume logic (lines 1089-1092)
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // Buttons are disabled when their corresponding task is completed
            // Following Xamarin EndOfDayActivity logic:
            // - RouteReturns: disabled when completed
            // - EndInventory: disabled when completed
            // - PrintReports: ALWAYS enabled (can print multiple times)
            // - ClockOut: disabled when completed
            // - LoadOrder: disabled when completed
            // - SetParLevel: disabled when completed
            // - RouteExpenses: disabled when reportsPrinted (Xamarin line 1092: routeExpenses.Enabled = !reportsPrinted)
            IsRouteReturnsEnabled = !_routeReturns;
            IsEndInventoryEnabled = !_endInventory;
            IsPrintReportsEnabled = true; // Always enabled - can print multiple times (Xamarin never disables this)
            IsClockOutEnabled = !_clockedOut;
            IsLoadOrderEnabled = !_loadOrder;
            IsSetParLevelEnabled = !_parLevel;
            // Route expenses is disabled when reports are printed (Xamarin EndOfDayActivity line 1092)
            IsRouteExpensesEnabled = !_reportsPrinted;
            
            // Notify commands that their CanExecute status may have changed
            RouteReturnsCommand.NotifyCanExecuteChanged();
            EndInventoryCommand.NotifyCanExecuteChanged();
            PrintReportsCommand.NotifyCanExecuteChanged();
            ClockOutCommand.NotifyCanExecuteChanged();
            LoadOrderCommand.NotifyCanExecuteChanged();
            SetParLevelCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(IsRouteReturnsEnabled))]
        private async Task RouteReturns()
        {
            _appService.RecordEvent("routeReturnsButton button");
            
            // Double-check if route returns is already completed (defensive check)
            var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
            if (File.Exists(routeReturnFile))
            {
                // Update state and button
                _routeReturns = true;
                UpdateButtonStates();
                await _dialogService.ShowAlertAsync("You already did the route returns. You cannot modify it.", "Alert", "OK");
                return;
            }
            
            // Check for password if configured
            if (!string.IsNullOrEmpty(Config.RouteReturnPassword))
            {
                var password = await _dialogService.ShowPromptAsync(
                    "Enter Password",
                    "Enter password to access route returns",
                    "OK",
                    "Cancel",
                    "Password",
                    -1,
                    "",
                    Keyboard.Default);
                
                if (string.IsNullOrEmpty(password))
                    return;
                
                if (string.Compare(password, Config.RouteReturnPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                    return;
                }
            }
            
            // Navigate to route returns page
            // If ButlerCustomization, should navigate to ButlerRouteReturnActivity equivalent
            // For now, use the same route - can be updated if ButlerRouteReturnPage exists
            if (Config.ButlerCustomization)
            {
                // TODO: Navigate to ButlerRouteReturnPage if it exists
                // await Shell.Current.GoToAsync("butlerroutereturns");
                await Shell.Current.GoToAsync("routereturns");
            }
            else
            {
                await Shell.Current.GoToAsync("routereturns");
            }
        }

        [RelayCommand(CanExecute = nameof(IsEndInventoryEnabled))]
        private async Task EndInventory()
        {
            // Double-check if end inventory is already completed (defensive check)
            if (Config.EndingInventoryCounted || Config.EmptyTruckAtEndOfDay)
            {
                // Update state and button
                _endInventory = true;
                UpdateButtonStates();
                await _dialogService.ShowAlertAsync("You already did the end inventory. You cannot modify it.", "Alert", "OK");
                return;
            }
            
            if (!_routeReturns)
            {
                if (Config.RouteReturnIsMandatory)
                {
                    await _dialogService.ShowAlertAsync("You must complete the Route Returns first.", "Alert", "OK");
                    return;
                }

                if (Config.EmptyTruckAtEndOfDay)
                    AutoCalculateRouteReturn();
            }
            
            if (!string.IsNullOrEmpty(Config.AutoEndInventoryPassword))
            {
                var result = await _dialogService.ShowPromptAsync("Enter Password", "", "Ok", "Cancel", "", -1, "", Keyboard.Default);

                if (result != null)
                {
                    if (string.Compare(result, Config.AutoEndInventoryPassword, StringComparison.CurrentCultureIgnoreCase) == 0)
                        await Shell.Current.GoToAsync("endinventory");
                    else
                        await _dialogService.ShowAlertAsync("Invalid Password", "Alert", "OK");
                }
            }
            else
            {
                await Shell.Current.GoToAsync("endinventory");
            }
        }

        [RelayCommand]
        private async Task CycleCount()
        {
            await Shell.Current.GoToAsync("cyclecount");
        }

        [RelayCommand]
        private async Task PrintReports()
        {
            var reportsPrintedFile = Path.Combine(Config.DataPath, "reportsPrinted.flag");

            _appService.RecordEvent("creditReportButton button");

            // Check if reports printing is disabled
            if (Config.DisablePrintEndOfDayReport)
            {
                await _dialogService.ShowAlertAsync("Reports printing is disabled.", "Alert", "OK");
                return;
            }
            
            // Note: Allow printing even if already printed (button remains enabled)
            // The flag file is just for tracking, not for preventing re-printing

            // Check prerequisites (matches Xamarin PrintReportsButtonHandler)
            if (!_routeReturns)
            {
                if (Config.RouteReturnIsMandatory)
                {
                    await _dialogService.ShowAlertAsync("Complete route return before printing.", "Alert", "OK");
                    return;
                }

                AutoCalculateRouteReturn();
            }

            if (!_endInventory && Config.EndingInvIsMandatory)
            {
                await _dialogService.ShowAlertAsync("Complete end inventory before printing.", "Alert", "OK");
                return;
            }

            if (Config.UseClockInOut && !_clockedOut)
            {
                await _dialogService.ShowAlertAsync("Clock out before printing.", "Alert", "OK");
                return;
            }

            _reportsPrinted = true;
            
            
            File.WriteAllText(reportsPrintedFile, DateTime.Now.ToString());
            
            UpdateButtonStates();

            // If using Milagro printer, print directly
            if (_isUsingMilagroPrinter)
            {
                PrinterProvider.PrintDocument((int copies) => PrintAllReports(copies));
                return;
            }

            // Show dialog with options: "Send by Email" or "Print Reports"
            var options = new[] { "Send by Email", "Print Reports" };
            var choice = await _dialogService.ShowActionSheetAsync("Print Options", "", "Cancel", options);

            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                return;

            if (choice == "Send by Email")
            {
                await SendReportsByEmailAsync();
            }
            else if (choice == "Print Reports")
            {
                PrinterProvider.PrintDocument((int copies) => PrintAllReports(copies));
            }
        }

        private string PrintAllReports(int number)
        {
            if (number < 1)
                return "Valid number of copies required";

            if (CompanyInfo.Companies.Count == 0)
            {
                CompanyInfo.Companies.Add(CompanyInfo.CreateDefaultCompany());
            }

            CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

            var map = DataProvider.ExtendedSendTheLeftOverInventory();

            int index = 1;

            int count = 1;

            var listOfPayments = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;

            if (Config.PaymentAvailable && (listOfPayments.Count > 0 || Config.PrintPaymentRegardless))
                count++;
            if (Config.UseBattery)
                count++;
            if (Config.PrintInvSettReport && (map.Count > 0 || Config.PrintInveSettlementRegardless))
                count++;

            var loadingErrorCount = RefusalMap();

            if (loadingErrorCount > 0)
                count++;

            if (Config.PrintCreditReport)
                count++;

            if (Config.RequestVehicleInformation)
                count++;

            IPrinter printer = PrinterProvider.CurrentPrinter();
            for (int i = 0; i < number; i++)
            {
                if (!printer.PrintOrdersCreatedReport(index, count))
                    return "Error printing";

                index++;

                if (Config.PaymentAvailable && (listOfPayments.Count > 0 || Config.PrintPaymentRegardless))
                {
                    if (!printer.PrintReceivedPaymentsReport(index, count))
                        return "Error printing";
                    index++;
                }

                if (Config.UseBattery)
                {
                    if (Config.CoreAsCredit)
                    {
                        var batPrinter = new BatteryPrinter();
                        if (!batPrinter.BatteryInventorySettlement(index, count))
                            return "Error printing";
                        return string.Empty;
                    }
                    else if (!printer.PrintBatteryEndOfDay(index, count))
                        return "Error printing";

                    index++;
                }

                if (Config.PrintInvSettReport)
                {
                    if (map.Count > 0 || Config.PrintInveSettlementRegardless)
                    {
                        if (!printer.InventorySettlement(index, count))
                            return "Error printing";
                        index++;
                    }
                }

                if (loadingErrorCount > 0)
                {
                    if (!printer.PrintRefusalReport(index, count))
                        return "Error printing";
                    index++;
                }

                if (Config.PrintCreditReport)
                {
                    if (!printer.PrintCreditReport(index, count))
                        return "Error printing";
                    index++;
                }

                if (Config.RequestVehicleInformation)
                {
                    if (!printer.PrintVehicleInformation(true, index, count, true))
                        return "Error printing";
                    index++;
                }
            }

            return string.Empty;
        }

        private int RefusalMap()
        {
            var refusedOrders = Order.Orders.Where(x => !x.Voided).ToList();

            if (Config.PrintRefusalReportByStore)
            {
                var count = 0;

                Dictionary<int, List<Order>> groupedOrders = new Dictionary<int, List<Order>>();

                foreach (var o in refusedOrders)
                {
                    if (!groupedOrders.ContainsKey(o.Client.ClientId))
                        groupedOrders.Add(o.Client.ClientId, new List<Order>() { o });
                    else
                        groupedOrders[o.Client.ClientId].Add(o);
                }

                var no_delivery_list = Reason.GetReasonsByType(ReasonType.No_Delivery);

                foreach (var group in groupedOrders)
                {
                    foreach (var order in group.Value)
                    {
                        foreach (var detail in order.Details)
                        {
                            if (detail.Reason != null && no_delivery_list.Any(x => x.Id == detail.Reason.Id))
                                count++;
                        }
                    }
                }

                return count;
            }
            else
            {
                var no_delivery_list = Reason.GetReasonsByType(ReasonType.No_Delivery);
                var count = 0;

                foreach (var o in refusedOrders)
                {
                    foreach (var detail in o.Details)
                    {
                        if (detail.Reason != null && no_delivery_list.Any(x => x.Id == detail.Reason.Id))
                            count++;
                    }
                }

                return count;
            }
        }

        private void AutoCalculateRouteReturn(bool update = true)
        {
            List<int> ids = new List<int>();
            var lines = new List<RouteReturnLine>();
            foreach (var o in Order.Orders)
                if (!o.Voided)
                    foreach (var od in o.Details)
                    {
                        if (od.IsCredit)
                        {
                            if (od.Product.CategoryId == 0 && od.Product.RequestedLoadInventory == 0)
                                continue;

                            var line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Lot == (od.Lot ?? ""));
                            if (line == null)
                            {
                                line = new RouteReturnLine();
                                line.Product = od.Product;
                                line.Lot = od.Lot ?? "";
                                ids.Add(line.Product.ProductId);
                                lines.Add(line);
                            }
                            float factor = 1;
                            if (od.UnitOfMeasure != null)
                                factor = od.UnitOfMeasure.Conversion;
                            if (od.Damaged)
                                line.Dumps += od.Qty * factor;
                            else
                                line.Returns += od.Qty * factor;
                        }
                    }

            // if empty track, create all the current inv as "returns", complete the above list with returns equals to current inventory
            if (Config.EmptyTruckAtEndOfDay)
                foreach (var product in Product.Products)
                {
                    if (product.CategoryId == 0 && product.RequestedLoadInventory == 0)
                        continue;

                    foreach (var invLine in product.ProductInv.TruckInventories)
                    {
                        var line = lines.FirstOrDefault(x => x.Product.ProductId == product.ProductId && x.Lot == invLine.Lot);
                        if (line == null)
                        {
                            line = new RouteReturnLine();
                            line.Product = product;
                            line.Lot = invLine.Lot;
                            line.Expiration = invLine.Expiration;
                            line.Weight = invLine.Weight;
                            lines.Add(line);
                            ids.Add(line.Product.ProductId);
                        }
                        line.Unload = invLine.CurrentQty < 0 ? 0 : invLine.CurrentQty;
                    }
                }

            foreach (var l in lines)
            {
                l.Product.SetOnCreditDump(l.Dumps, l.Lot, l.Expiration, l.Weight);
                l.Product.SetOnCreditReturn(l.Returns, l.Lot, l.Expiration, l.Weight);

                l.Product.SetUnloadInventory(l.Unload, l.Lot, l.Expiration, l.Weight);

                if (Config.EmptyTruckAtEndOfDay)
                    l.Product.SetCurrentInventory(0, l.Lot, l.Expiration, l.Weight);
                else
                {
                    float currentInv = l.Product.GetInventory(l.Lot, l.Weight);

                    l.Product.SetCurrentInventory(currentInv - l.Unload, l.Lot, l.Expiration, l.Weight);
                }
            }

            ProductInventory.Save();

            _routeReturns = true;
            UpdateButtonStates();
        }

        [RelayCommand]
        private async Task EndOfDay()
        {
            _appService.RecordEvent("endOfDayButton button");

            if (_sentAll)
            {
                // [ACTIVITY STATE]: Remove state when end of day was already completed
                Helpers.NavigationHelper.RemoveNavigationState("endofday");
                
                await _dialogService.ShowAlertAsync("You already did end of day.", "Alert", "OK");
                await Shell.Current.GoToAsync("///MainPage");
                return;
            }

            if (Config.TimeSheetCustomization && !_clockedOut && !Config.TimeSheetAutomaticClockIn)
            {
                await _dialogService.ShowAlertAsync("You must clock out before sending all the information.", "Alert", "OK");
                return;
            }

            if (!_routeReturns)
            {
                if (Config.RouteReturnIsMandatory)
                {
                    await _dialogService.ShowAlertAsync("You must complete route returns before sending.", "Alert", "OK");
                    return;
                }

                AutoCalculateRouteReturn(true);
            }

            if (!_endInventory && Config.EndingInvIsMandatory)
            {
                await _dialogService.ShowAlertAsync("You must complete end inventory before sending.", "Alert", "OK");
                return;
            }

            if (Config.UseClockInOut && !_clockedOut)
            {
                await _dialogService.ShowAlertAsync("You must clock out before sending.", "Alert", "OK");
                return;
            }

            if (Config.LoadRequired)
            {
                if (!Config.SetParLevel && !_loadOrder)
                {
                    await _dialogService.ShowAlertAsync("You must complete load order before sending.", "Alert", "OK");
                    return;
                }
                if (Config.SetParLevel && !_parLevel)
                {
                    await _dialogService.ShowAlertAsync("You must complete par level before sending.", "Alert", "OK");
                    return;
                }
            }

            if (Config.SetParLevel && !_parLevel)
            {
                var tempParLevelFile = Path.Combine(Config.DataPath, "temp_ParLevelPath.xml");
                if (File.Exists(tempParLevelFile))
                {
                    await _dialogService.ShowAlertAsync("Par level has not been saved.", "Alert", "OK");
                    return;
                }
            }

            if (!_reportsPrinted && Config.PrintReportsRequired)
            {
                await _dialogService.ShowAlertAsync("You must print all reports before sending.", "Alert", "OK");
                return;
            }
            else if (!_reportsPrinted)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Warning",
                    "You have not printed all reports. Continue anyway?",
                    "Yes",
                    "No");
                
                if (!confirmed)
                    return;
            }

            // ButlerCustomization: Set negative inventories to 0
            if (Config.ButlerCustomization)
            {
                try
                {
                    var products = new List<Product>();
                    foreach (var item in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
                    {
                        if (item.CategoryId == 0 && item.RequestedLoadInventory == 0)
                            continue;

                        products.Add(item);
                    }

                    products = products.Where(x => x.ProductInv.TruckInventories.Count > 0).ToList();

                    var below0Products = products.Where(x => x.CurrentInventory < 0).ToList();
                    foreach (var p in below0Products)
                        p.SetCurrentInventory(0);

                    ProductInventory.Save();
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }

            // EmptyTruckAtEndOfDay: Check for negative inventories
            if (Config.EmptyTruckAtEndOfDay)
            {
                try
                {
                    var products = new List<Product>();
                    foreach (var item in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
                    {
                        if (item.CategoryId == 0 && item.RequestedLoadInventory == 0)
                            continue;

                        products.Add(item);
                    }

                    products = products.Where(x => x.ProductInv.TruckInventories.Count > 0).ToList();

                    if (products.Any(x => x.CurrentInventory < 0))
                    {
                        var confirmed = await _dialogService.ShowConfirmationAsync(
                            "Alert",
                            "You have negative inventories in the truck. Would you like to continue sending?",
                            "Yes",
                            "No");
                        
                        if (!confirmed)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }

            // Final confirmation
            var finalConfirmed = await _dialogService.ShowConfirmationAsync(
                "Alert",
                "Are you sure that you would like to transmit all the information?");
            
            if (!finalConfirmed)
                return;

            // Execute EndOfDayHandler
            await EndOfDayHandler();
        }

        private async Task EndOfDayHandler()
        {
            var reportsPrintedFile = Path.Combine(Config.DataPath, "reportsPrinted.flag");

            await _dialogService.ShowLoadingAsync("Sending all information...");

            string responseMessage = null;
            bool errorDownloadingData = false;
            DateTime now = DateTime.Now;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Get or create EOD state so we don't resend steps that already succeeded (match Xamarin EndOfDayActivity.SendAll)
                        var state = ActivityState.GetState("EndOfDayActivity");
                        if (state == null)
                        {
                            state = new ActivityState { ActivityType = "EndOfDayActivity" };
                            ActivityState.AddState(state);
                        }

                        // Build ordersTotals and handle duplicates (match Xamarin lines 1131-1165)
                        var ordersTotals = new Dictionary<string, double>();
                        foreach (var order in Order.Orders.Where(x => x.OrderType != OrderType.Load).ToList())
                        {
                            if (ordersTotals.ContainsKey(order.UniqueId))
                            {
                                Logger.CreateLog("Found two orders with the same UniqueId");
                                var other = Order.Orders.FirstOrDefault(x => x.UniqueId == order.UniqueId && x.Filename != order.Filename);
                                if (other != null && other.Details.Count == order.Details.Count)
                                {
                                    Logger.CreateLog("Will delete the duplicated order as they have the same number of details");
                                    order.ForceDelete();
                                }
                            }
                            else
                                ordersTotals.Add(order.UniqueId, order.OrderTotalCost());
                        }

                        bool ordersSent = state.State.ContainsKey(OrdersSentIntent) && state.State[OrdersSentIntent] == "1";
                        var extendedMap = DataProvider.ExtendedSendTheLeftOverInventory(true);

                        if (ordersTotals.Count > 0)
                        {
                            SaveSendAllTempFile(ordersTotals, extendedMap);
                            SendOrdersStep(state);
                        }
                        else if (ordersSent)
                        {
                            LoadFromSendAllTempFile(out ordersTotals, out extendedMap);
                        }

                        if (Config.SendPaymentsInEOD)
                            SendInvoicePaymentsStep(state, ordersTotals);

                        if (File.Exists(Config.ClientProdSortFile))
                        {
                            if (Config.UseDraggableTemplate)
                                DataProvider.SendClientProdSort();
                            else
                                File.Delete(Config.ClientProdSortFile);
                        }

                        if (Config.ButlerCustomization)
                            DataProvider.SendButlerTransfers();
                        else if (File.Exists(Config.TransferOnFile) || File.Exists(Config.TransferOffFile))
                            SendTransfersStep(state);

                        if (Session.session != null)
                            Session.ClockOutCurrentSession();

                        if (Config.Delivery)
                            SendTheLeftOverInventoryStep(state, extendedMap);

                        if (BuildToQty.List.Count > 0)
                            SendBuildToQtyStep(state);

                        if (LaceupMigration.RouteExpenses.CurrentExpenses != null && File.Exists(Config.ExpensesPath))
                        {
                            // SendRouteExpenses not exposed on DataProvider; skip or call if added later
                        }

                        SendDayReportStep(state);

                        SendLoadOrderStep(state);

                        if (Config.SetParLevel)
                            SendParLevelStep(state);

                        if (File.Exists(Config.SavedDailyParLevelFile))
                            SendDailyParLevelStep(state);

                        BackgroundDataSync.SendRoute(true);

                        if (Config.SalesByDepartment && ClientDepartment.Departments.Any(x => x.Updated == true))
                            DataProvider.SendClientDepartments();

                        if (Config.AssetTracking)
                        {
                            // SendAssetTracking not exposed on DataProvider; skip if not added
                        }

                        if (Config.RequestVehicleInformation && VehicleInformation.EODVehicleInformation != null)
                        {
                            // SendVehicleInformation not exposed on DataProvider; skip if not added
                        }

                        DataProvider.DeleteTransferFiles();

                        if (File.Exists(Config.InventoryStoreFile))
                            File.Delete(Config.InventoryStoreFile);

                        if (File.Exists(SendAllTempFile))
                            File.Delete(SendAllTempFile);

                        if (File.Exists(Config.DeliveryFile))
                            File.Delete(Config.DeliveryFile);

                        Config.SessionId = string.Empty;
                        Config.SaveSessionId();
                        Config.EndingInventoryCounted = false;
                        ProductInventory.ClearAll();

                        _sentAll = true;
                        _canLeaveScreen = true;
                        Config.PendingLoadToAccept = false;
                        Config.ReceivedData = false;
                        Config.LastEndOfDay = DateTime.Now;
                        VehicleInformation.Clear();

                        var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
                        if (File.Exists(routeReturnFile))
                            File.Delete(routeReturnFile);
                        if (File.Exists(reportsPrintedFile))
                            File.Delete(reportsPrintedFile);
                        _routeReturns = false;
                        _endInventory = false;
                        _reportsPrinted = false;

                        // Remove EOD send state so next EOD starts fresh
                        var eodState = ActivityState.GetState("EndOfDayActivity");
                        if (eodState != null)
                            ActivityState.RemoveState(eodState);

                        Config.SaveAppStatus();
                    }
                    catch (Exception e)
                    {
                        errorDownloadingData = true;
                        responseMessage = "Error sending all data." + Environment.NewLine + "Error opening connection." + Environment.NewLine + "Error checking internet.";
                        Logger.CreateLog(e);
                    }
                });
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }

            string title = "Alert";
            if (string.IsNullOrEmpty(responseMessage))
            {
                TimeSpan ts = DateTime.Now.Subtract(now);
                responseMessage = $"Data downloaded in {ts.TotalSeconds} seconds.";
                title = "Info";
            }

            if (errorDownloadingData)
            {
                await _dialogService.ShowAlertAsync(responseMessage, title, "OK");
            }
            else
            {
                Config.SaveLastEndOfDay();
                UpdateButtonStates();
                Helpers.NavigationHelper.RemoveNavigationState("endofday");
                await _dialogService.ShowAlertAsync("Data successfully transmitted.", "Success", "OK");
                await Shell.Current.GoToAsync("///MainPage");
            }
        }

        private static void SaveSendAllTempFile(Dictionary<string, double> ordersTotals, List<InventorySettlementRow> extendedMap)
        {
            if (File.Exists(SendAllTempFile))
                File.Delete(SendAllTempFile);
            using (var writer = new StreamWriter(SendAllTempFile, false))
            {
                var isFirst = true;
                foreach (var item in ordersTotals)
                {
                    if (!isFirst) writer.Write((char)20);
                    else isFirst = false;
                    writer.Write(item.Key + "|" + item.Value.ToString(CultureInfo.InvariantCulture));
                }
                writer.WriteLine();
                isFirst = true;
                foreach (var item in extendedMap)
                {
                    if (!isFirst) writer.Write((char)20);
                    else isFirst = false;
                    writer.Write("0|" + item.Serialize());
                }
                writer.WriteLine();
            }
        }

        private static void LoadFromSendAllTempFile(out Dictionary<string, double> ordersTotals, out List<InventorySettlementRow> extendedMap)
        {
            ordersTotals = new Dictionary<string, double>();
            extendedMap = new List<InventorySettlementRow>();
            if (!File.Exists(SendAllTempFile)) return;
            using (var reader = new StreamReader(SendAllTempFile, false))
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var parts = line.Split((char)20);
                    foreach (var item in parts)
                    {
                        var ot = item.Split('|');
                        if (ot.Length == 2)
                            ordersTotals.Add(ot[0], Convert.ToDouble(ot[1], CultureInfo.InvariantCulture));
                    }
                }
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line)) return;
                var parts2 = line.Split((char)20);
                foreach (var item in parts2)
                {
                    var ot = item.Split('|');
                    if (ot.Length < 14) continue;
                    var row = new InventorySettlementRow();
                    var prodId = Convert.ToInt32(ot[1], CultureInfo.InvariantCulture);
                    row.Product = Product.Find(prodId);
                    if (row.Product == null) continue;
                    row.BegInv = Convert.ToSingle(ot[2], CultureInfo.InvariantCulture);
                    row.LoadOut = Convert.ToSingle(ot[3], CultureInfo.InvariantCulture);
                    row.Adj = Convert.ToSingle(ot[4], CultureInfo.InvariantCulture);
                    row.TransferOn = Convert.ToSingle(ot[5], CultureInfo.InvariantCulture);
                    row.TransferOff = Convert.ToSingle(ot[6], CultureInfo.InvariantCulture);
                    row.Sales = Convert.ToSingle(ot[7], CultureInfo.InvariantCulture);
                    row.Dump = Convert.ToSingle(ot[8], CultureInfo.InvariantCulture);
                    row.Unload = Convert.ToSingle(ot[9], CultureInfo.InvariantCulture);
                    row.CreditDump = Convert.ToSingle(ot[10], CultureInfo.InvariantCulture);
                    row.CreditReturns = Convert.ToSingle(ot[11], CultureInfo.InvariantCulture);
                    row.EndInventory = Convert.ToSingle(ot[12], CultureInfo.InvariantCulture);
                    row.DamagedInTruck = Convert.ToSingle(ot[13], CultureInfo.InvariantCulture);
                    if (ot.Length > 14) row.SkipRelated = Convert.ToBoolean(ot[14]);
                    if (ot.Length > 15) row.Lot = ot[15];
                    if (ot.Length > 16) row.LoadingError = Convert.ToSingle(ot[16], CultureInfo.InvariantCulture);
                    if (ot.Length > 17) row.Reshipped = Convert.ToSingle(ot[17], CultureInfo.InvariantCulture);
                    extendedMap.Add(row);
                }
            }
        }

        private static void SendOrdersStep(ActivityState state)
        {
            if (state.State.ContainsKey(OrdersSentIntent) && state.State[OrdersSentIntent] == "1") return;
            try
            {
                var orders = Order.Orders.Where(x => x.OrderType != OrderType.Load && !(x.IsQuote && x.QuoteModified)).ToList();
                var batches = Batch.List.Where(x => orders.Any(y => y.BatchId == x.Id));
                foreach (var order in orders)
                {
                    if (order.EndDate == DateTime.MinValue) order.EndDate = DateTime.Now;
                    if (order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(order.PrintedOrderId))
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                    order.Save();
                }
                DataProvider.SendTheOrders(batches, orders.Select(x => x.OrderId.ToString()).ToList());
                RouteEx.ClearAll();
                state.State[OrdersSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[OrdersSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendInvoicePaymentsStep(ActivityState state, Dictionary<string, double> ordersTotals)
        {
            if (state.State.ContainsKey(PaymentsSentIntent) && state.State[PaymentsSentIntent] == "1") return;
            try
            {
                DataProvider.SendInvoicePayments(ordersTotals);
                state.State[PaymentsSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[PaymentsSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendTransfersStep(ActivityState state)
        {
            if (state.State.ContainsKey(TransfersSentIntent) && state.State[TransfersSentIntent] == "1") return;
            try
            {
                DataProvider.SendTransfers();
                state.State[TransfersSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[TransfersSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendTheLeftOverInventoryStep(ActivityState state, List<InventorySettlementRow> extendedMap)
        {
            if (state.State.ContainsKey(LeftOverSentIntent) && state.State[LeftOverSentIntent] == "1") return;
            try
            {
                DataProvider.SendTheLeftOverInventory(extendedMap);
                var currentInventoryTotal = Product.Products.Sum(x => x.CurrentInventory);
                BackgroundDataSync.SendInventory(currentInventoryTotal);
                state.State[LeftOverSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[LeftOverSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendBuildToQtyStep(ActivityState state)
        {
            if (state.State.ContainsKey(BuildToQtySentIntent) && state.State[BuildToQtySentIntent] == "1") return;
            try
            {
                DataProvider.SendBuildToQty();
                state.State[BuildToQtySentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[BuildToQtySentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendDayReportStep(ActivityState state)
        {
            if (state.State.ContainsKey(DayReportSentIntent) && state.State[DayReportSentIntent] == "1") return;
            try
            {
                if (!SalesmanSession.ClockedOut)
                    SalesmanSession.CloseSession();
                DataProvider.SendDayReport(Config.SessionId);
                state.State[DayReportSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[DayReportSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendLoadOrderStep(ActivityState state)
        {
            if (state.State.ContainsKey(LoadOrderSentIntent) && state.State[LoadOrderSentIntent] == "1") return;
            try
            {
                LaceupMigration.LoadOrder.SaveListFromOrders();
                DataProvider.SendLoadOrder();
                state.State[LoadOrderSentIntent] = "1";
                ActivityState.Save();
                foreach (var o in Order.Orders.ToList())
                    o.ForceDelete();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[LoadOrderSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendParLevelStep(ActivityState state)
        {
            if (state.State.ContainsKey(ParlevelSentIntent) && state.State[ParlevelSentIntent] == "1") return;
            try
            {
                ParLevel.LoadList();
                DataProvider.SendParLevel();
                state.State[ParlevelSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[ParlevelSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }

        private static void SendDailyParLevelStep(ActivityState state)
        {
            if (state.State.ContainsKey(DailyParlevelSentIntent) && state.State[DailyParlevelSentIntent] == "1") return;
            try
            {
                if (File.Exists(Config.SavedDailyParLevelFile))
                {
                    using (var reader = new StreamReader(Config.SavedDailyParLevelFile))
                    {
                        if (string.IsNullOrEmpty(reader.ReadToEnd()))
                        {
                            state.State[DailyParlevelSentIntent] = "1";
                            ActivityState.Save();
                            return;
                        }
                    }
                }
                DataProvider.SendDailyParLevel();
                state.State[DailyParlevelSentIntent] = "1";
                ActivityState.Save();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                state.State[DailyParlevelSentIntent] = "0";
                ActivityState.Save();
                throw;
            }
        }


        [RelayCommand(CanExecute = nameof(IsLoadOrderEnabled))]
        private async Task LoadOrder()
        {
            _appService.RecordEvent("LoadOrderButton button");
            
            try
            {
                _canLeaveScreen = false;

                var client = Client.Clients.FirstOrDefault(x => x.ClientName.StartsWith("Salesman: "));
                if (client == null)
                    client = Client.CreateSalesmanClient();

                Order order = Order.Orders.FirstOrDefault(x => x.OrderType == OrderType.Load && !x.PendingLoad);
                if (order == null)
                {
                    order = new Order(client) { OrderType = OrderType.Load };
                    order.SalesmanId = 0;
                }

                order.Save();

                var query = new Dictionary<string, object> { { "orderId", order.OrderId } };
                await Shell.Current.GoToAsync("newloadordertemplate", query);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error creating load order: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(IsSetParLevelEnabled))]
        private async Task SetParLevel()
        {
            // Double-check if par level is already completed (defensive check)
            var parLevelFile = Config.ParLevelFile;
            var tempParLevelFile = Path.Combine(Config.DataPath, "temp_ParLevelPath.xml");
            if (File.Exists(parLevelFile) || (!File.Exists(tempParLevelFile) && ParLevel.List.Count == 0))
            {
                _parLevel = true;
                UpdateButtonStates();
                await _dialogService.ShowAlertAsync("Par level is already set.", "Alert", "OK");
                return;
            }
            
            _parLevel = true;
            _canLeaveScreen = false; // Xamarin line 374: canLeaveScreen = false when ParLevel clicked
            
            await Shell.Current.GoToAsync("setparlevel");
        }

        [RelayCommand(CanExecute = nameof(IsClockOutEnabled))]
        private async Task ClockOut()
        {
            // Double-check if already clocked out (defensive check)
            if (SalesmanSession.ClockedOut)
            {
                _clockedOut = true;
                UpdateButtonStates();
                await _dialogService.ShowAlertAsync("You are already clocked out.", "Alert", "OK");
                return;
            }
            
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Clock Out",
                "Clock out for the day?",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            try
            {
                // Close salesman session
                SalesmanSession.CloseSession();
                
                _clockedOut = true;
                UpdateButtonStates();
                
                await _dialogService.ShowAlertAsync("Clocked out successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error clocking out: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(IsRouteExpensesEnabled))]
        private async Task RouteExpenses()
        {
            // Xamarin doesn't check if reports are printed before allowing route expenses
            // But the button is disabled when reportsPrinted (line 1092)
            await Shell.Current.GoToAsync("routeexpenses");
        }

        public async Task ShowCannotLeaveDialog()
        {
            await _dialogService.ShowAlertAsync("You cannot leave until end of day is completed.", "Alert", "OK");
        }

        [RelayCommand]
        private async Task ShowMenu()
        {
            var menuItems = new List<string>
            {
                "Configuration",
                "Sync Data",
                "Advanced Options"
            };

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", menuItems.ToArray());

            switch (choice)
            {
                case "Configuration":
                    await Configuration();
                    break;
                case "Sync Data":
                    await SyncData();
                    break;
                case "Advanced Options":
                    await AdvancedLog();
                    break;
            }
        }

        [RelayCommand]
        private async Task Configuration()
        {
            if (Config.NeedAccessForConfiguration)
            {
                // TODO: Implement access code request
                await _dialogService.ShowAlertAsync("Access code request functionality to be implemented.", "Alert", "OK");
                return;
            }

            await Shell.Current.GoToAsync("configuration");
        }

        [RelayCommand]
        private async Task SyncData()
        {
            if (DataProvider.MustEndOfDay())
            {
                await _dialogService.ShowAlertAsync("Do end of day.", "Warning", "OK");
                return;
            }

            await _dialogService.ShowLoadingAsync("Downloading data...");
            string responseMessage = null;
            bool errorDownloadingData = false;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (var access = new NetAccess())
                        {
                            access.OpenConnection();
                            access.CloseConnection();
                        }

                        DataProvider.CheckAuthorization();
                        if (Config.AuthorizationFailed)
                            throw new Exception("Not authorized");

                        if (!DataProvider.CheckSyncAuthInfo())
                            throw new Exception("Wait before sync");

                        responseMessage = DataProvider.DownloadData(true, !Config.TrackInventory || true);
                    }
                    catch (Exception ee)
                    {
                        errorDownloadingData = true;
                        Logger.CreateLog(ee);
                        
                        var message = ee.Message;
                        if (message.Contains("Invalid auth info"))
                            message = "Not authorized";
                        
                        responseMessage = message.Replace("(305)-381-1123", "(786) 437-4380");
                    }
                });
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }

            var title = errorDownloadingData ? "Alert" : "Info";
            if (string.IsNullOrEmpty(responseMessage))
                responseMessage = "Data downloaded.";

            await _dialogService.ShowAlertAsync(responseMessage, title, "OK");
        }

        [RelayCommand]
        private async Task AdvancedLog()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        private async Task SendReportsByEmailAsync()
        {
            string pdfFile = null;
            try
            {
                await _dialogService.ShowLoadingAsync("Generating reports...");

                // Generate PDF for all reports (similar to PrintAllReports but create PDF instead)
                if (CompanyInfo.Companies.Count == 0)
                {
                    CompanyInfo.Companies.Add(CompanyInfo.CreateDefaultCompany());
                }

                CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                // Generate the end-of-day report PDF using the PDF provider
                // This matches the Xamarin EndOfDayActivity implementation
                await Task.Run(() =>
                {
                    try
                    {
                        // Get the PDF provider instance (same pattern as EmailHelper)
                        IPdfProvider pdfGenerator = GetPdfProvider();
                        
                        // Generate the PDF report
                        pdfFile = pdfGenerator.GetReportPdf();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        throw;
                    }
                });

                await _dialogService.HideLoadingAsync();

                // Check if PDF was generated successfully
                if (string.IsNullOrEmpty(pdfFile) || !File.Exists(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("Error generating report PDF.", "Alert", "OK");
                    return;
                }

                // Send the PDF by email using the helper
                if (Config.helper != null)
                {
                    try
                    {
                        Config.helper.SendReportByEmail(pdfFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        await _dialogService.ShowAlertAsync("Error occurred sending reports by email: " + ex.Message, "Alert", "OK");
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Email helper not available.", "Alert", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending reports by email: " + ex.Message, "Alert", "OK");
            }
        }

        /// <summary>
        /// Gets the PDF provider instance (matches EmailHelper.GetPdfProvider pattern)
        /// </summary>
        private static IPdfProvider GetPdfProvider()
        {
            try
            {
                IPdfProvider provider;

                // Instantiate selected Pdf Provider
                Type t = Type.GetType(Config.PdfProvider);
                if (t == null)
                {
                    Logger.CreateLog("could not instantiate pdf provider " + Config.PdfProvider + " using DefaultPdfProvider instead");
                    provider = new DefaultPdfProvider();
                }
                else
                {
                    provider = Activator.CreateInstance(t) as IPdfProvider;
                    if (provider == null)
                    {
                        Logger.CreateLog("could not instantiate pdf provider " + Config.PdfProvider + " using DefaultPdfProvider instead");
                        provider = new DefaultPdfProvider();
                    }
                }

                return provider;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                return new DefaultPdfProvider();
            }
        }
    }
}
