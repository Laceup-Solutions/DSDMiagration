using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class CycleCountPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<CycleCountLineViewModel> _cycleCountLines = new();
        [ObservableProperty] private bool _showingAll;
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private bool _showPrintButton;

        public CycleCountPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowPrintButton = Config.PrinterAvailable;
        }

        public async Task OnAppearingAsync()
        {
            await LoadCycleCountAsync();
        }

        private async Task LoadCycleCountAsync()
        {
            await Task.Run(() =>
            {
                var lines = new System.Collections.Generic.List<CycleCountLineViewModel>();

                foreach (var p in Product.Products.Where(x => x.ProductType == ProductType.Inventory && x.CategoryId > 0))
                {
                    lines.Add(new CycleCountLineViewModel
                    {
                        Product = p,
                        ProductName = p.Name,
                        BeginningInventory = p.BeginigInventory,
                        CurrentInventory = p.CurrentInventory,
                        Real = 0
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CycleCountLines.Clear();
                    foreach (var line in lines)
                    {
                        CycleCountLines.Add(line);
                    }
                    Filter();
                });
            });
        }

        partial void OnSearchQueryChanged(string value)
        {
            Filter();
        }

        [RelayCommand]
        private void Filter()
        {
            ShowingAll = !ShowingAll;
            
            // Filter to show all or only items with inventory
            var allLines = CycleCountLines.ToList();
            CycleCountLines.Clear();
            
            if (ShowingAll)
            {
                foreach (var line in allLines)
                {
                    CycleCountLines.Add(line);
                }
            }
            else
            {
                // Show only items with inventory
                foreach (var line in allLines.Where(x => x.CurrentInventory > 0 || x.Real > 0))
                {
                    CycleCountLines.Add(line);
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Save Cycle Count",
                    "Save cycle count data?",
                    "Yes",
                    "No");
                
                if (!confirmed)
                    return;
                
                // Save cycle count items
                var fileName = Path.Combine(Config.DataPath, "cycleCount.xml");
                
                if (File.Exists(fileName))
                    File.Delete(fileName);
                
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var line in CycleCountLines)
                    {
                        var item = new CycleCountItem
                        {
                            Product = line.Product,
                            Qty = line.Real,
                            Lot = string.Empty,
                            Expiration = DateTime.MinValue,
                            UoM = null,
                            Weight = 0
                        };
                        item.Serialize(writer);
                    }
                }
                
                await _dialogService.ShowAlertAsync("Cycle count saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving cycle count: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            try
            {
                var items = CycleCountLines.Select(x => new CycleCountItem
                {
                    Product = x.Product,
                    Qty = x.Real,
                    Lot = string.Empty,
                    Expiration = DateTime.MinValue,
                    UoM = null,
                    Weight = 0
                }).ToList();
                
                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    
                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintInventoryCount(items);
                        if (!result)
                            return "Error printing";
                    }
                    
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public partial class CycleCountLineViewModel : ObservableObject
    {
        [ObservableProperty] private Product _product = null!;
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private float _beginningInventory;
        [ObservableProperty] private float _currentInventory;
        [ObservableProperty] private float _real;
    }
}

