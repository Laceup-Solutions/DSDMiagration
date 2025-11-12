using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SetupPrinterPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<PrinterTypeViewModel> _printerTypes = new();
        [ObservableProperty] private PrinterTypeViewModel? _selectedPrinterType;
        [ObservableProperty] private string _printerName = string.Empty;
        [ObservableProperty] private string _printerAddress = string.Empty;
        [ObservableProperty] private int _printerPort;
        [ObservableProperty] private bool _isDefaultPrinter;
        [ObservableProperty] private int _paperWidth = 80;
        [ObservableProperty] private int _paperHeight = 200;

        public SetupPrinterPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // Load printer types
                PrinterTypes.Clear();
                PrinterTypes.Add(new PrinterTypeViewModel { Id = 1, Name = "Zebra" });
                PrinterTypes.Add(new PrinterTypeViewModel { Id = 2, Name = "Printek" });
                PrinterTypes.Add(new PrinterTypeViewModel { Id = 3, Name = "Generic" });
                PrinterTypes.Add(new PrinterTypeViewModel { Id = 4, Name = "Network" });

                // TODO: Load existing printer configuration from Config
                // SelectedPrinterType = PrinterTypes.FirstOrDefault(x => x.Id == Config.PrinterType);
                // PrinterName = Config.PrinterName;
                // etc.
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading printer settings: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task TestPrint()
        {
            if (string.IsNullOrWhiteSpace(PrinterName) || string.IsNullOrWhiteSpace(PrinterAddress))
            {
                await _dialogService.ShowAlertAsync("Please enter printer name and address.", "Validation Error", "OK");
                return;
            }

            try
            {
                // TODO: Implement test print
                // PrinterUtilities.TestPrint(SelectedPrinterType, PrinterAddress, PrinterPort);
                
                await _dialogService.ShowAlertAsync("Test print sent. Check your printer.", "Info", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error sending test print: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(PrinterName) || string.IsNullOrWhiteSpace(PrinterAddress))
            {
                await _dialogService.ShowAlertAsync("Please enter printer name and address.", "Validation Error", "OK");
                return;
            }

            try
            {
                // TODO: Save printer configuration to Config
                // Config.PrinterType = SelectedPrinterType?.Id ?? 0;
                // Config.PrinterName = PrinterName;
                // Config.PrinterAddress = PrinterAddress;
                // Config.PrinterPort = PrinterPort;
                // Config.Save();
                
                await _dialogService.ShowAlertAsync("Printer settings saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving printer settings: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public partial class PrinterTypeViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = string.Empty;
    }
}

