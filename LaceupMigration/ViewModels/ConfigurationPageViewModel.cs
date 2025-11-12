using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

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
                }

                // TODO: Save all configuration changes
                await _dialogService.ShowAlertAsync("Configuration saved successfully.", "Success", "OK");
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
            var confirmed = await _dialogService.ShowConfirmationAsync("Confirm", "Are you sure you want to clean data? This action cannot be undone.", "Yes", "No");
            if (confirmed)
            {
                // TODO: Implement data cleaning
                await _dialogService.ShowAlertAsync("Data cleaning functionality to be implemented.", "Info", "OK");
            }
        }
    }
}
