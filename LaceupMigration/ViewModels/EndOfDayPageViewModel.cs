using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndOfDayPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        
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

        [ObservableProperty] private bool _showExpenses;
        [ObservableProperty] private bool _showRouteReturns;
        [ObservableProperty] private bool _showEndInventory;
        [ObservableProperty] private bool _showCycleCount;
        [ObservableProperty] private bool _showPrintReports;
        [ObservableProperty] private bool _showLoadOrder;
        [ObservableProperty] private bool _showSetParLevel;
        [ObservableProperty] private bool _showClockOut;

        public EndOfDayPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
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
            
            var endingInventoryFile = Path.Combine(Config.DataPath, "endingInventory.xml");
            _endInventory = File.Exists(endingInventoryFile);
            
            // Check if load order is completed (check if there's a load order that's finished or not pending)
            _loadOrder = Order.Orders.Any(x => x.OrderType == OrderType.Load && (x.Finished || !x.PendingLoad));
            
            // Check if par level is completed (check if ParLevel file exists or if temp file doesn't exist)
            var parLevelFile = Config.ParLevelFile;
            var tempParLevelFile = Path.Combine(Config.DataPath, "temp_ParLevelPath.xml");
            _parLevel = File.Exists(parLevelFile) || (!File.Exists(tempParLevelFile) && ParLevel.List.Count == 0);
            
            // Check if clocked out (would need to check SalesmanSession or similar)
            // For now, assume false unless we have a way to check
            
            // If DisablePrintEndOfDayReport, automatically set reportsPrinted
            if (Config.DisablePrintEndOfDayReport)
            {
                _reportsPrinted = true;
            }
            
            // Auto-calculate route returns if disabled and not done
            if (!_routeReturns && Config.DisableRouteReturn)
            {
                AutoCalculateRouteReturn(false);
                if (Config.EmptyTruckAtEndOfDay)
                    _endInventory = true;
            }
        }

        [RelayCommand]
        private async Task RouteReturns()
        {
            _appService.RecordEvent("routeReturnsButton button");
            
            // Check if route returns is already completed
            var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
            if (File.Exists(routeReturnFile))
            {
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

        [RelayCommand]
        private async Task EndInventory()
        {
            await Shell.Current.GoToAsync("endinventory");
        }

        [RelayCommand]
        private async Task CycleCount()
        {
            await Shell.Current.GoToAsync("cyclecount");
        }

        [RelayCommand]
        private async Task PrintReports()
        {
            _appService.RecordEvent("creditReportButton button");

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

            // If using Milagro printer, print directly
            if (_isUsingMilagroPrinter)
            {
                PrinterProvider.PrintDocument((int copies) => PrintAllReports(copies));
                return;
            }

            // Show dialog with options: "Send by Email" or "Print Reports"
            var options = new[] { "Send by Email", "Print Reports" };
            var choice = await _dialogService.ShowActionSheetAsync("Print Options", "Cancel", null, options);

            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                return;

            if (choice == "Send by Email")
            {
                // Navigate to FinalReportOfDayPage if it exists, otherwise show message
                // TODO: Check if FinalReportOfDayPage exists
                await _dialogService.ShowAlertAsync("Send by Email functionality will navigate to report page.", "Info", "OK");
                // await Shell.Current.GoToAsync("finalreportofday");
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

            var map = DataAccess.ExtendedSendTheLeftOverInventory();

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

            DataAccess.SaveInventory();

            _routeReturns = true;
        }

        [RelayCommand]
        private async Task EndOfDay()
        {
            _appService.RecordEvent("endOfDayButton button");

            if (_sentAll)
            {
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
                "Sure would like to transmit all?",
                "Yes",
                "No");
            
            if (!finalConfirmed)
                return;

            // Execute EndOfDayHandler
            await EndOfDayHandler();
        }

        private async Task EndOfDayHandler()
        {
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
                        DataAccess.SendAll();

                        // TODO: test this
                        if (Config.DexAvailable)
                        {
                            // Refresh.RefreshDexLicense(this);
                            // TODO: Implement RefreshDexLicense if needed
                        }

                        _sentAll = true;
                        _canLeaveScreen = true;

                        DataAccess.PendingLoadToAccept = false;
                        DataAccess.ReceivedData = false;
                        DataAccess.LastEndOfDay = DateTime.Now;

                        VehicleInformation.Clear();

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

                await _dialogService.ShowAlertAsync("Data successfully transmitted.", "Success", "OK");
                
                // Navigate to MainPage
                await Shell.Current.GoToAsync("///MainPage");
            }
        }

        [RelayCommand]
        private async Task LoadOrder()
        {
            _appService.RecordEvent("LoadOrderButton button");
            
            try
            {
                _loadOrder = true;
                _canLeaveScreen = false;

                // Create or get load order for salesman
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

        [RelayCommand]
        private async Task SetParLevel()
        {
            await Shell.Current.GoToAsync("setparlevel");
        }

        [RelayCommand]
        private async Task ClockOut()
        {
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
                
                await _dialogService.ShowAlertAsync("Clocked out successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error clocking out: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task RouteExpenses()
        {
            await Shell.Current.GoToAsync("routeexpenses");
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

            var choice = await Application.Current!.MainPage!.DisplayActionSheet("Menu", "Cancel", null, menuItems.ToArray());

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
            if (DataAccess.MustEndOfDay())
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

                        DataAccess.CheckAuthorization();
                        if (Config.AuthorizationFailed)
                            throw new Exception("Not authorized");

                        if (!DataAccess.CheckSyncAuthInfo())
                            throw new Exception("Wait before sync");

                        responseMessage = DataAccessEx.DownloadData(true, !Config.TrackInventory || true);
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
            var options = new[] { "Update settings", "Send log file", "Export data", "Remote control", "Setup printer" };
            if (Config.GoToMain)
            {
                var list = options.ToList();
                list.Add("Go to main activity");
                options = list.ToArray();
            }

            var choice = await Application.Current!.MainPage!.DisplayActionSheet("Advanced options", "Cancel", null, options);
            
            switch (choice)
            {
                case "Update settings":
                    await _appService.UpdateSalesmanSettingsAsync();
                    await _dialogService.ShowAlertAsync("Settings updated.", "Info", "OK");
                    break;
                case "Send log file":
                    await _appService.SendLogAsync();
                    await _dialogService.ShowAlertAsync("Log sent.", "Info", "OK");
                    break;
                case "Export data":
                    await _appService.ExportDataAsync();
                    await _dialogService.ShowAlertAsync("Data exported.", "Info", "OK");
                    break;
                case "Remote control":
                    await _appService.RemoteControlAsync();
                    break;
                case "Setup printer":
                    // TODO: Implement printer setup
                    break;
                case "Go to main activity":
                    await _appService.GoBackToMainAsync();
                    break;
            }
        }
    }
}
