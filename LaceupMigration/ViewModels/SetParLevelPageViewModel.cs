using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SetParLevelPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<InventoryLine> _allProductList = new();
        private string _searchCriteria = string.Empty;
        private bool _currentlyDisplayingAll = false;

        [ObservableProperty] private ObservableCollection<ParLevelLineViewModel> _parLevelLines = new();
        [ObservableProperty] private DateTime _setDate = DateTime.Now;
        [ObservableProperty] private string _setDateText = string.Empty;
        [ObservableProperty] private bool _readOnly;
        [ObservableProperty] private string _filterButtonText = "All";

        public SetParLevelPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            SetDateText = SetDate.ToShortDateString();
        }

        public async Task OnAppearingAsync()
        {
            await LoadParLevelsAsync();
        }

        private async Task LoadParLevelsAsync()
        {
            await Task.Run(() =>
            {
                var productList = new System.Collections.Generic.List<InventoryLine>();

                foreach (var detail in ParLevel.List)
                {
                    productList.Add(new InventoryLine
                    {
                        Product = detail.Product,
                        Real = detail.Qty,
                        Starting = -1 // Mark as existing par level
                    });
                }

                foreach (var p in Product.Products.Where(x => x.CategoryId > 0))
                {
                    var existing = productList.FirstOrDefault(x => x.Product.ProductId == p.ProductId);
                    if (existing == null)
                    {
                        productList.Add(new InventoryLine
                        {
                            Product = p,
                            Real = 0,
                            Starting = 0
                        });
                    }
                }

                if (Config.LoadOrderEmpty && ParLevel.List.Count == 0)
                {
                    foreach (var detail in productList)
                        detail.Real = 0;
                }

                var sorted = SortDetails.SortedDetails(productList).ToList();
                _allProductList = sorted;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ApplyFilter();
                });
            });
        }

        private void ApplyFilter()
        {
            ParLevelLines.Clear();
            
            IEnumerable<InventoryLine> filtered = _allProductList;
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(_searchCriteria))
            {
                var searchUpper = _searchCriteria.ToUpperInvariant();
                filtered = filtered.Where(x => 
                    x.Product.Name.ToUpperInvariant().Contains(searchUpper) ||
                    x.Product.Code?.ToUpperInvariant().Contains(searchUpper) == true ||
                    x.Product.Sku?.ToUpperInvariant().Contains(searchUpper) == true ||
                    x.Product.Upc?.ToUpperInvariant().Contains(searchUpper) == true
                );
            }
            
            // Apply showing all filter
            if (_currentlyDisplayingAll)
            {
                // Show all products
                filtered = filtered;
            }
            else
            {
                // Show only items with Real > 0 or Starting == -1 (existing par levels)
                filtered = filtered.Where(x => x.Real > 0 || x.Starting == -1);
            }
            
            foreach (var line in filtered)
            {
                var existing = ParLevelLines.FirstOrDefault(x => x.InventoryLine.Product.ProductId == line.Product.ProductId);
                if (existing != null)
                {
                    // Update existing
                    existing.Qty = line.Real;
                }
                else
                {
                    ParLevelLines.Add(new ParLevelLineViewModel(line, this));
                }
            }
        }

        [RelayCommand]
        private void Filter()
        {
            _currentlyDisplayingAll = !_currentlyDisplayingAll;
            FilterButtonText = _currentlyDisplayingAll ? "Current" : "All";
            ApplyFilter();
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Save Par Level",
                    "Save par level data?",
                    "Yes",
                    "No");
                
                if (!confirmed)
                    return;
                
                // Update ParLevel.List with current values from all products
                var itemsToAdd = new List<ParLevel>();
                foreach (var line in _allProductList)
                {
                    if (line.Real > 0)
                    {
                        itemsToAdd.Add(new ParLevel 
                        { 
                            Product = line.Product, 
                            Qty = line.Real 
                        });
                    }
                }
                
                ParLevel.List.Clear();
                foreach (var item in itemsToAdd)
                {
                    ParLevel.List.Add(item);
                }
                
                ParLevel.SaveList();
                
                await _dialogService.ShowAlertAsync("Par level saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving par level: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            try
            {
                // Update ParLevel.List temporarily for printing
                var originalList = ParLevel.List.ToList();
                var itemsToAdd = new List<ParLevel>();
                foreach (var line in _allProductList.Where(x => x.Real > 0))
                {
                    itemsToAdd.Add(new ParLevel 
                    { 
                        Product = line.Product, 
                        Qty = line.Real 
                    });
                }
                
                if (itemsToAdd.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("No items to print.", "Info", "OK");
                    return;
                }
                
                ParLevel.List.Clear();
                foreach (var item in itemsToAdd)
                {
                    ParLevel.List.Add(item);
                }
                
                PrinterProvider.PrintDocument((int copies) =>
                {
                    if (copies < 1)
                        return "Valid number of copies required";
                    
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    
                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintOrderLoad(ReadOnly);
                        if (!result)
                        {
                            // Restore original list
                            ParLevel.List.Clear();
                            foreach (var item in originalList)
                            {
                                ParLevel.List.Add(item);
                            }
                            return "Error printing";
                        }
                    }
                    
                    // Restore original list
                    ParLevel.List.Clear();
                    foreach (var item in originalList)
                    {
                        ParLevel.List.Add(item);
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

        [RelayCommand]
        private async Task Search()
        {
            var searchText = await _dialogService.ShowPromptAsync("Search", "Enter product name", "OK", "Cancel", "", -1, "");
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                _searchCriteria = searchText;
                ApplyFilter();
            }
            else if (string.IsNullOrEmpty(searchText))
            {
                // Clear search
                _searchCriteria = string.Empty;
                ApplyFilter();
            }
        }

        partial void OnSetDateChanged(DateTime value)
        {
            SetDateText = SetDate.ToShortDateString();
        }
    }

    public partial class ParLevelLineViewModel : ObservableObject
    {
        private readonly SetParLevelPageViewModel _parent;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private float _qty;

        public InventoryLine InventoryLine { get; }

        public ParLevelLineViewModel(InventoryLine line, SetParLevelPageViewModel parent)
        {
            InventoryLine = line;
            _parent = parent;
            ProductName = line.Product.Name;
            Qty = line.Real;
        }

        partial void OnQtyChanged(float value)
        {
            // Update the underlying InventoryLine when Qty changes
            if (InventoryLine != null)
            {
                InventoryLine.Real = value;
            }
        }
    }
}

