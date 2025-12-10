using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class ConfigurationPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private string _version = string.Empty;
        [ObservableProperty] private string _internetServer = string.Empty;
        [ObservableProperty] private string _lanServer = string.Empty;
        [ObservableProperty] private string _port = string.Empty;
        [ObservableProperty] private string _ssid = string.Empty;
        [ObservableProperty] private string _vendorName = string.Empty;
        [ObservableProperty] private string _vendorId = string.Empty;
        [ObservableProperty] private string _deviceId = string.Empty;
        [ObservableProperty] private string _currentSSID = string.Empty;
        [ObservableProperty] private string _nextOrderId = string.Empty;
        [ObservableProperty] private bool _canModifyConnectionSettings;
        [ObservableProperty] private bool _canChangeSalesmanId;
        [ObservableProperty] private bool _showPrinterButton;
        [ObservableProperty] private bool _showScannerButton;

        public ConfigurationPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            Version = Config.Version;
            InternetServer = ServerHelper.GetServerNumber(Config.IPAddressGateway);
            LanServer = Config.LanAddress;
            Port = Config.Port.ToString(CultureInfo.InvariantCulture);
            Ssid = Config.SSID;
            VendorName = Config.VendorName;
            VendorId = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
            DeviceId = Config.DeviceId;
            CurrentSSID = $"[{Config.GetSSID()}]";

            CanModifyConnectionSettings = Config.CanModifyConnectSett;
            CanChangeSalesmanId = Config.CanChangeSalesmanId && Config.SupervisorId == 0;
            ShowPrinterButton = Config.PrinterAvailable;
            ShowScannerButton = Config.ScannerToUse == 3;
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (CanModifyConnectionSettings)
                {
                    // TODO: Save connection settings
                }

                if (CanChangeSalesmanId)
                {
                    if (int.TryParse(VendorId, out var salesmanId))
                    {
                        Config.SalesmanId = salesmanId;
                    }
                    Config.VendorName = VendorName;
                    
                    // [MIGRATION]: Save SalesmanId to both Preferences and file system
                    // SaveSettings() saves to Preferences (used by Initialize())
                    Config.SaveSettings();
                    // SaveSystemSettings() saves to file (used by LoadSystemSettings() which overrides Preferences)
                    Config.SaveSystemSettings();
                }

                await _dialogService.ShowAlertAsync("Configuration saved successfully.", "Success", "OK");
                
                // Navigate back after successful save
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving configuration: {ex.Message}", "Error", "OK");
            }
        }

        [RelayCommand]
        private async Task SetupPrinter()
        {
            // TODO: Navigate to printer setup
            await _dialogService.ShowAlertAsync("Printer setup functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SetupScanner()
        {
            // TODO: Navigate to scanner setup
            await _dialogService.ShowAlertAsync("Scanner setup functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SelectLogo()
        {
            // TODO: Implement logo selection
            await _dialogService.ShowAlertAsync("Logo selection functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task ResetPassword()
        {
            // TODO: Implement password reset
            await _dialogService.ShowAlertAsync("Password reset functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SeeLog()
        {
            // TODO: Navigate to log viewer
            await _dialogService.ShowAlertAsync("Log viewer functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SendLog()
        {
            await _appService.SendLogAsync();
            await _dialogService.ShowAlertAsync("Log sent.", "Info", "OK");
        }

        [RelayCommand]
        private async Task ExportData()
        {
            await _appService.ExportDataAsync();
            await _dialogService.ShowAlertAsync("Data exported.", "Info", "OK");
        }

        [RelayCommand]
        private async Task RestoreData()
        {
            // TODO: Implement data restore
            await _dialogService.ShowAlertAsync("Data restore functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task CleanData()
        {
            // Match Xamarin CleanButton_Click (line 769-793)
            // Show action sheet with 3 options: Clear Log, Clear Data, Clear All
            var selected = await _dialogService.ShowActionSheetAsync(
                "Clear",
                null,
                "Cancel",
                "Clear Log",
                "Clear Data",
                "Clear All");

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            if (selected == "Clear Log")
            {
                // Match Xamarin ClearLogFile (line 795-799)
                _appService.RecordEvent("Clear log button");
                Logger.ClearFile();
                await _dialogService.ShowAlertAsync("Log cleared.", "Info", "OK");
            }
            else if (selected == "Clear Data")
            {
                // Match Xamarin ClearData(true, false) - line 785
                await ClearDataAsync(false);
            }
            else if (selected == "Clear All")
            {
                // Match Xamarin ClearData(true, true) - line 787
                await ClearDataAsync(true);
            }
        }

        private async Task ClearDataAsync(bool clearAll)
        {
            // Match Xamarin ClearData method (line 801-857)
            // Save config values that need to be preserved
            var acceptedTerms = Config.AcceptedTermsAndConditions;
            var loginConfig = Config.SignedIn;
            var enabledlogin = Config.EnableLogin;

            // Show confirmation dialog - match Xamarin line 816
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Warning",
                "Are you sure you want to clean your data?",
                "Yes",
                "No");

            if (!confirmed)
                return;

            // Show loading dialog - match Xamarin line 818
            await _dialogService.ShowLoadingAsync("Please wait...");

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Force a backup before cleaning - match Xamarin line 825
                        BackgroundDataSync.ForceBackup();

                        // Remove activity state - match Xamarin line 827-829
                        var activityState = ActivityState.GetState(typeof(ConfigurationPageViewModel).FullName);
                        if (activityState != null)
                            ActivityState.RemoveState(activityState);

                        // Clear settings if "Clear All" - match Xamarin line 831-832
                        if (clearAll)
                            Config.ClearSettings();

                        // Clear data - match Xamarin line 834
                        DataAccess.ClearData();

                        // Reinitialize - match Xamarin line 835-836
                        Config.Initialize();
                        DataAccess.Initialize();

                        // Restore saved config values - match Xamarin line 838-840
                        Config.AcceptedTermsAndConditions = acceptedTerms;
                        Config.SignedIn = loginConfig;
                        Config.EnableLogin = enabledlogin;

                        // Save settings - match Xamarin line 842
                        Config.SaveSettings();
                    }
                    catch (Exception ex)
                    {
                        _appService.TrackError(ex);
                        throw;
                    }
                });

                await _dialogService.HideLoadingAsync();

                // Show success message - match Xamarin behavior (no app close message)
                await _dialogService.ShowAlertAsync(
                    "Data cleared successfully.",
                    "Success",
                    "OK");

                // Navigate to MainPage to clean the tabs/navigation stack - match Xamarin behavior
                // This clears the navigation stack and resets to main page
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync(
                    $"Error clearing data: {ex.Message}",
                    "Error",
                    "OK");
                _appService.TrackError(ex);
            }
        }
    }
}
