using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndOfDayProcessPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private bool _hasRouteReturns;
        [ObservableProperty] private bool _hasEndInventory;
        [ObservableProperty] private bool _hasExpenses;
        [ObservableProperty] private bool _hasReportsPrinted;
        [ObservableProperty] private bool _canProcess;
        [ObservableProperty] private string _statusMessage = "Please complete all required steps.";
        [ObservableProperty] private Color _statusColor = Colors.Black;

        public EndOfDayProcessPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            await ValidateStatus();
        }

        private async Task ValidateStatus()
        {
            await Task.Run(() =>
            {
                // Check if route returns file exists (Route Returns saves to routeReturn.xml)
                var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
                HasRouteReturns = File.Exists(routeReturnFile);
                
                // Check if end inventory was completed
                // End inventory saves to ProductInventoriesFile, check if it exists and has data
                var productInventoriesFile = Config.ProductInventoriesFile;
                HasEndInventory = false;
                if (File.Exists(productInventoriesFile))
                {
                    try
                    {
                        // Load ProductInventory to check if warehouse inventory was set
                        ProductInventory.Load();
                        // Check if any product has warehouse inventory set (end inventory sets this)
                        HasEndInventory = ProductInventory.CurrentInventories.Values
                            .Any(x => x.WarehouseInventory > 0 || x.TruckInventories.Count > 0);
                    }
                    catch
                    {
                        HasEndInventory = false;
                    }
                }
                
                // If EmptyTruckAtEndOfDay is enabled, end inventory is auto-completed
                if (Config.EmptyTruckAtEndOfDay)
                {
                    HasEndInventory = true;
                }
                
                // If DisableRouteReturn is enabled, route returns are auto-completed
                if (Config.DisableRouteReturn)
                {
                    HasRouteReturns = true;
                }
                
                // Check if expenses exist
                HasExpenses = Config.ShowExpensesInEOD 
                    ? (RouteExpenses.CurrentExpenses != null && RouteExpenses.CurrentExpenses.Details.Count > 0)
                    : true; // If expenses not required, mark as done
                
                // Check if reports were printed
                // If DisablePrintEndOfDayReport is enabled, reports are considered printed
                HasReportsPrinted = Config.DisablePrintEndOfDayReport;
                
                // Also check if a flag file exists (set when reports are printed)
                if (!HasReportsPrinted)
                {
                    var reportsPrintedFile = Path.Combine(Config.DataPath, "reportsPrinted.flag");
                    HasReportsPrinted = File.Exists(reportsPrintedFile);
                }
            });

            UpdateCanProcess();
        }

        partial void OnHasRouteReturnsChanged(bool value)
        {
            UpdateCanProcess();
        }

        partial void OnHasEndInventoryChanged(bool value)
        {
            UpdateCanProcess();
        }

        partial void OnHasExpensesChanged(bool value)
        {
            UpdateCanProcess();
        }

        partial void OnHasReportsPrintedChanged(bool value)
        {
            UpdateCanProcess();
        }

        private void UpdateCanProcess()
        {
            var required = HasRouteReturns && HasEndInventory && HasReportsPrinted;
            if (Config.ShowExpensesInEOD)
                required = required && HasExpenses;

            CanProcess = required;

            if (CanProcess)
            {
                StatusMessage = "All validations passed. Ready to process.";
                StatusColor = Colors.Green;
            }
            else
            {
                StatusMessage = "Please complete all required steps before processing.";
                StatusColor = Colors.Red;
            }
        }

        [RelayCommand]
        private async Task ProcessEndOfDay()
        {
            if (!CanProcess)
            {
                await _dialogService.ShowAlertAsync("Please complete all required steps.", "Validation Error", "OK");
                return;
            }

            // Validate all requirements
            if (!HasRouteReturns && Config.RouteReturnIsMandatory)
            {
                await _dialogService.ShowAlertAsync("Route returns must be completed before processing end of day.", "Validation Error", "OK");
                return;
            }

            if (!HasEndInventory && Config.EndingInvIsMandatory)
            {
                await _dialogService.ShowAlertAsync("End inventory must be completed before processing end of day.", "Validation Error", "OK");
                return;
            }

            if (!HasReportsPrinted && Config.PrintReportsRequired)
            {
                await _dialogService.ShowAlertAsync("All reports must be printed before processing end of day.", "Validation Error", "OK");
                return;
            }

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Confirm End of Day",
                "This will finalize the end of day process and send all data. Continue?",
                "Yes",
                "No");

            if (!confirmed)
                return;

            try
            {
                // Show loading indicator
                await _dialogService.ShowLoadingAsync("Processing end of day...");
                IsLoading = true;
                StatusMessage = "Processing end of day...";
                StatusColor = Colors.Blue;

                await Task.Run(() =>
                {
                    // Execute SendAll which handles all end of day operations
                    DataProvider.SendAll();
                    
                    // Update status
                    Config.PendingLoadToAccept = false;
                    Config.ReceivedData = false;
                    Config.LastEndOfDay = DateTime.Now;
                    
                    // Clear session
                    Config.SessionId = string.Empty;
                    Config.SaveSessionId();
                    
                    // Clear product inventory
                    ProductInventory.ClearAll();
                    
                    // Clean up EOD flag files
                    var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
                    if (File.Exists(routeReturnFile))
                        File.Delete(routeReturnFile);
                    
                    var reportsPrintedFile = Path.Combine(Config.DataPath, "reportsPrinted.flag");
                    if (File.Exists(reportsPrintedFile))
                        File.Delete(reportsPrintedFile);
                });

                // Hide loading indicator
                await _dialogService.HideLoadingAsync();
                IsLoading = false;

                StatusMessage = "End of day process completed successfully!";
                StatusColor = Colors.Green;

                // Show success message and navigate to MainPage after OK is clicked
                await _dialogService.ShowAlertAsync("End of day process completed successfully! All data has been sent.", "Success", "OK");
                
                // Navigate to MainPage (absolute route to clear navigation stack)
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                // Hide loading indicator on error
                await _dialogService.HideLoadingAsync();
                IsLoading = false;
                
                StatusMessage = $"Error: {ex.Message}";
                StatusColor = Colors.Red;
                await _dialogService.ShowAlertAsync($"Error processing end of day: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await ValidateStatus();
        }

        [ObservableProperty] private bool _isLoading;
    }
}

