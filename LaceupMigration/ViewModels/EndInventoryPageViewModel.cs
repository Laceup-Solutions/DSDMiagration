using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndInventoryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<EndInventoryItemViewModel> _allItems = new();

        [ObservableProperty] private ObservableCollection<EndInventoryItemViewModel> _inventoryItems = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public EndInventoryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // Load current inventory from ProductInventory
                ProductInventory.Load();
                
                _allItems = new List<EndInventoryItemViewModel>();
                
                // Get inventory from ProductInventory.CurrentInventories
                foreach (var product in Product.Products.Where(p => p.OnHand > 0 || ProductInventory.CurrentInventories.ContainsKey(p.ProductId)))
                {
                    var productInv = ProductInventory.GetInventoryForProduct(product.ProductId);
                    float currentQty = product.OnHand;
                    
                    // If we have ProductInventory data, use warehouse inventory
                    if (productInv != null)
                    {
                        currentQty = productInv.WarehouseInventory;
                    }
                    
                    _allItems.Add(new EndInventoryItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name ?? "Unknown",
                        CurrentQuantity = currentQty,
                        EndingQuantity = currentQty
                    });
                }

                FilterInventory(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading inventory: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterInventory(string searchText)
        {
            InventoryItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allItems
                : _allItems.Where(x => x.ProductName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var item in filtered)
            {
                InventoryItems.Add(item);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                // Update ProductInventory with ending quantities
                foreach (var item in _allItems)
                {
                    var productInv = ProductInventory.GetInventoryForProduct(item.ProductId);
                    if (productInv == null)
                    {
                        productInv = new ProductInventory
                        {
                            ProductId = item.ProductId,
                            WarehouseInventory = item.EndingQuantity
                        };
                        ProductInventory.CurrentInventories[item.ProductId] = productInv;
                    }
                    else
                    {
                        productInv.WarehouseInventory = item.EndingQuantity;
                    }
                }
                
                // Save ProductInventory
                ProductInventory.Save();
                
                // Also update Product.OnHand for consistency
                foreach (var item in _allItems)
                {
                    var product = Product.Find(item.ProductId);
                    if (product != null)
                    {
                        product.OnHand = item.EndingQuantity;
                    }
                }
                
                await _dialogService.ShowAlertAsync("Ending inventory saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving inventory: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            try
            {
                var inventoryLines = _allItems.Select(x =>
                {
                    var product = Product.Find(x.ProductId);
                    return new InventoryLine
                    {
                        Product = product,
                        Real = x.EndingQuantity,
                        Starting = x.CurrentQuantity
                    };
                }).Where(x => x.Product != null);

                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    
                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintSetInventory(inventoryLines);
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

    public partial class EndInventoryItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private float _currentQuantity;
        [ObservableProperty] private float _endingQuantity;
    }
}

