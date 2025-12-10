using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class InventoryMainPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;

        [ObservableProperty] private bool _showAcceptLoad;
        [ObservableProperty] private bool _acceptLoadEnabled;
        [ObservableProperty] private bool _showPendingLoad;
        [ObservableProperty] private bool _showLoadOrder;
        [ObservableProperty] private bool _showParLevel;
        [ObservableProperty] private bool _showTransfers;
        [ObservableProperty] private bool _showViewPrintInventory;
        [ObservableProperty] private bool _showCheckInventory;
        [ObservableProperty] private bool _showCycleCount;
        [ObservableProperty] private string _viewPrintText = "View/Print Inventory";

        public InventoryMainPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
        }

        public async Task OnAppearingAsync()
        {
            BackgroundDataSync.UpdateInvValuesFromOrders();

            // Match Xamarin's visibility conditions exactly
            // View/Print Inventory - always hidden (line 89: viewPrintButton.Visibility = ViewStates.Gone;)
            ShowViewPrintInventory = false;

            // Check Inventory - hidden by default in layout (android:visibility="gone")
            ShowCheckInventory = false;

            // Set Par Level - visible if Config.SetParLevel (line 83)
            ShowParLevel = Config.SetParLevel;

            // Create Load Order - visible if Config.LoadRequest (line 82)
            ShowLoadOrder = Config.LoadRequest;

            // Pending Load to Accept label - visible if DataAccess.PendingLoadToAccept (lines 70-73)
            ShowPendingLoad = DataAccess.PendingLoadToAccept;

            // Accept Load - visible if Config.Delivery (line 67)
            ShowAcceptLoad = Config.Delivery;
            AcceptLoadEnabled = Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand || DataAccess.PendingLoadToAccept;

            // Transfer On/Off - hidden if Config.HideTransfers (lines 64-65)
            ShowTransfers = !Config.HideTransfers;

            // Inventory Count (Cycle Count) - always hidden (line 85: cycleCountButton.Visibility = ViewStates.Gone;)
            ShowCycleCount = false;

            // Inventory Summary - always visible (no visibility check in Xamarin)

            ViewPrintText = Config.PrinterAvailable ? "View/Print Inventory" : "View Inventory";
        }

        [RelayCommand]
        private async Task ViewPrintInventory()
        {
            _appService.RecordEvent("View/Print inventory button");

            // Match Xamarin's ViewPrintInventoryClick - check for password if configured
            if (!string.IsNullOrEmpty(Config.ViewPrintInvPassword))
            {
                var password = await _dialogService.ShowPromptAsync("View/Print Inventory", "Enter Password", "OK", "Cancel", "Password");
                if (string.IsNullOrEmpty(password))
                    return; // User cancelled

                if (string.Compare(password, Config.ViewPrintInvPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert");
                    return;
                }
            }

            await Shell.Current.GoToAsync("viewprintinventory");
        }

        [RelayCommand]
        private async Task CheckInventory()
        {
            _appService.RecordEvent("Check inventory button");
            await Shell.Current.GoToAsync("checkinventory");
        }

        [RelayCommand]
        private async Task AcceptLoad()
        {
            _appService.RecordEvent("Accept load");
            
            if (string.IsNullOrEmpty(Config.AddInventoryPassword))
            {
                if (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand)
                {
                    // AcceptLoadOnDemand - show date picker first, then download and navigate
                    await AcceptLoadOnDemandAsync();
                }
                else
                {
                    // Navigate to ReceiveLoadActivity (old accept load page - not the list)
                    // For now, navigate to acceptload which handles both cases
                    await Shell.Current.GoToAsync("acceptload");
                }
                return;
            }

            // Ask for password first
            var password = await _dialogService.ShowPromptAsync("Accept Load", "Enter Password", "OK", "Cancel", "Password", keyboard: Keyboard.Default);
            if (string.IsNullOrEmpty(password))
                return; // User cancelled

            if (string.Compare(password, Config.AddInventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                return;
            }

            // Password is correct
            if (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand)
            {
                // AcceptLoadOnDemand - show date picker first, then download and navigate
                await AcceptLoadOnDemandAsync();
            }
            else
            {
                // Navigate to ReceiveLoadActivity (old accept load page)
                await Shell.Current.GoToAsync("acceptload");
            }
        }

        private async Task AcceptLoadOnDemandAsync()
        {
            try
            {
                // Match Xamarin AcceptLoadOnDemand() - show date picker with today's date
                // Match Xamarin: DateTime dt = DateTime.Today; var dialog = new DatePickerDialogFragment(this, dt);
                DateTime selectedDate = DateTime.Today;
                var date = await _dialogService.ShowDatePickerAsync("Select Date", selectedDate);
                
                if (date.HasValue)
                {
                    // Match Xamarin Refresh(date) - download and navigate
                    await RefreshAndNavigateToAcceptLoadAsync(date.Value);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error showing date picker: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task RefreshAndNavigateToAcceptLoadAsync(DateTime date)
        {
            // Match Xamarin Refresh(DateTime date) method
            await _dialogService.ShowLoadingAsync("Downloading load orders...");
            string responseMessage = null;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Download products first
                        DataAccess.DownloadProducts();

                        // Get pending load orders for the selected date
                        DataAccess.GetPendingLoadOrders(date);
                    }
                    catch (Exception e)
                    {
                        Logger.CreateLog(e);
                        responseMessage = "Error downloading load orders.";
                    }
                });

                await _dialogService.HideLoadingAsync();

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
                else
                {
                    // Navigate to AcceptLoadOrderList (AcceptLoadPage) with the selected date
                    // Match Xamarin: activity.PutExtra("loadDate", date.Ticks.ToString());
                    await Shell.Current.GoToAsync($"acceptload?loadDate={date.Ticks}");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Error refreshing load orders.", "Alert", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task TransferOn()
        {
            _appService.RecordEvent("TransferOnButton");

            // Match Xamarin's TransferOnButton_Click logic exactly
            if (Config.TransferPasswordAtSaving)
            {
                // Navigate directly to transfer activity (password will be asked at saving)
                await Shell.Current.GoToAsync("transferonoff?action=transferOn");
                return;
            }

            // Ask for password first - ALWAYS show prompt when TransferPasswordAtSaving is false
            // Match Xamarin: always shows password dialog, even if password is empty
            var password = await _dialogService.ShowPromptAsync("Transfer On", "Enter Password", "OK", "Cancel", "Password");
            
            // If password is null, user cancelled - return early (match Xamarin Cancel behavior)
            if (password == null)
                return;

            // Compare entered password with config password
            // Match Xamarin: string.Compare(password, Config.TransferPassword, StringComparison.CurrentCultureIgnoreCase) == 0
            // Empty password can match empty config password
            if (string.Compare(password ?? "", Config.TransferPassword ?? "", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                await _dialogService.ShowAlertAsync("Invalid password.", "Alert");
                return;
            }

            // Navigate to transfer activity
            // Note: Xamarin checks Config.LoadLotInTransfer and Config.ButlerCustomization to determine activity type
            // For now, use transferonoff route which should handle these cases
            await Shell.Current.GoToAsync("transferonoff?action=transferOn");
        }

        [RelayCommand]
        private async Task TransferOff()
        {
            _appService.RecordEvent("TransferOffButton");

            // Match Xamarin's TransferOffButton_Click logic exactly
            if (Config.TransferPasswordAtSaving)
            {
                // Navigate directly to transfer activity (password will be asked at saving)
                await Shell.Current.GoToAsync("transferonoff?action=transferOff");
                return;
            }

            // Ask for password first - ALWAYS show prompt when TransferPasswordAtSaving is false
            // Match Xamarin: always shows password dialog, even if password is empty
            var password = await _dialogService.ShowPromptAsync("Transfer Off", "Enter Password", "OK", "Cancel", "Password");
            
            // If password is null, user cancelled - return early (match Xamarin Cancel behavior)
            if (password == null)
                return;

            // Compare entered password with config password
            // Match Xamarin: string.Compare(password, Config.TransferOffPassword, StringComparison.CurrentCultureIgnoreCase) == 0
            // Empty password can match empty config password
            if (string.Compare(password ?? "", Config.TransferOffPassword ?? "", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                await _dialogService.ShowAlertAsync("Invalid password.", "Alert");
                return;
            }

            // Navigate to transfer activity
            // Note: Xamarin checks Config.LoadLotInTransfer and Config.ButlerCustomization to determine activity type
            // For now, use transferonoff route which should handle these cases
            await Shell.Current.GoToAsync("transferonoff?action=transferOff");
        }

        [RelayCommand]
        private async Task ViewLoadOrder()
        {
            _appService.RecordEvent("loadOrderButton");

            // Match Xamarin's loadOrderButton_Click logic - create or find Load order first
            try
            {
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

                // Navigate to NewLoadOrderTemplateActivity with orderId and canGetOutIntent="1"
                // Check if newloadordertemplate route exists, otherwise use viewloadorder
                var route = $"newloadordertemplate?orderId={order.OrderId}&canGetOutIntent=1";
                await Shell.Current.GoToAsync(route);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error creating load order: {ex.Message}", "Error");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task SetParLevel()
        {
            _appService.RecordEvent("setParLevelButton");

            // Match Xamarin's parLevelButton_Click - pass canGetOutIntent="1"
            await Shell.Current.GoToAsync("setparlevel?canGetOutIntent=1");
        }

        [RelayCommand]
        private async Task CycleCount()
        {
            // Note: In Xamarin, cycleCountButton.Visibility = ViewStates.Gone (line 85)
            // So this button is hidden by default in Xamarin
            await Shell.Current.GoToAsync("cyclecount");
        }

        [RelayCommand]
        private async Task InventorySummary()
        {
            // Match Xamarin's InventorySummary_Click - navigates to CurrentInventorySummaryActivity
            await Shell.Current.GoToAsync("inventorysummary");
        }

        [RelayCommand]
        private async Task ShowMenu()
        {
            var options = new List<string>();
            
            // Advanced Options
            options.Add("Advanced Options");
            
            if (options.Count == 0)
                return;
            
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.ToArray());
            
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;
            
            switch (choice)
            {
                case "Advanced Options":
                    await _advancedOptionsService.ShowAdvancedOptionsAsync();
                    break;
            }
        }
    }
}
