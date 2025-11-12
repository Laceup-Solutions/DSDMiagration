using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.IO;
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
            // Check actual status - these would be set by the individual pages
            // For now, check if route returns file exists
            var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
            HasRouteReturns = File.Exists(routeReturnFile);
            
            // Check if end inventory was completed (would be in ProductInventory or a flag)
            HasEndInventory = false; // This would need to be tracked
            
            // Check if expenses exist
            HasExpenses = Config.ShowExpensesInEOD 
                ? (RouteExpenses.CurrentExpenses != null && RouteExpenses.CurrentExpenses.Details.Count > 0)
                : true; // If expenses not required, mark as done
            
            // Check if reports were printed (would need to be tracked)
            HasReportsPrinted = false;

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
                IsLoading = true;
                StatusMessage = "Processing end of day...";
                StatusColor = Colors.Blue;

                await Task.Run(() =>
                {
                    // Execute SendAll which handles all end of day operations
                    DataAccess.SendAll();
                    
                    // Update status
                    DataAccess.PendingLoadToAccept = false;
                    DataAccess.ReceivedData = false;
                    DataAccess.LastEndOfDay = DateTime.Now;
                    
                    // Clear session
                    Config.SessionId = string.Empty;
                    Config.SaveSessionId();
                    
                    // Clear product inventory
                    ProductInventory.ClearAll();
                });

                StatusMessage = "End of day process completed successfully!";
                StatusColor = Colors.Green;

                await _dialogService.ShowAlertAsync("End of day process completed successfully! All data has been sent.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusColor = Colors.Red;
                await _dialogService.ShowAlertAsync($"Error processing end of day: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        [ObservableProperty] private bool _isLoading;
    }
}

