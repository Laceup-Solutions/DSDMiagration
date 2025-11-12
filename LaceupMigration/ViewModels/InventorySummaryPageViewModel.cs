using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class InventorySummaryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<InventorySummaryProductViewModel> _products = new();
        [ObservableProperty] private string _totalQtyText = string.Empty;
        [ObservableProperty] private string _totalWeightText = string.Empty;
        [ObservableProperty] private string _totalPriceText = string.Empty;
        [ObservableProperty] private bool _showPrice;
        [ObservableProperty] private bool _showWeight;
        [ObservableProperty] private bool _showPrintButton;
        [ObservableProperty] private string _selectedUnitType = "Base";
        [ObservableProperty] private bool _showingAll;

        public InventorySummaryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowPrice = Config.ShowPricesInInventorySummary;
            ShowWeight = Config.ShowWeightOnInventorySummary || Config.UsePallets;
            ShowPrintButton = Config.PrinterAvailable;
        }

        public async Task OnAppearingAsync()
        {
            await LoadInventoryAsync();
        }

        private async Task LoadInventoryAsync()
        {
            await Task.Run(() =>
            {
                var products = Product.Products
                    .Where(x => x.ProductType == ProductType.Inventory)
                    .Where(x => !(x.CategoryId == 0 && x.RequestedLoadInventory == 0))
                    .ToList();

                var weightItems = products.Where(x => x.CurrentInventory > 0 && x.SoldByWeight);
                double weight = 0;
                foreach (var w in weightItems)
                {
                    if (w.ProductInv?.TruckInventories != null && w.ProductInv.TruckInventories.Count > 0)
                    {
                        foreach (var i in w.ProductInv.TruckInventories)
                        {
                            weight += (i.CurrentQty * i.Weight);
                        }
                    }
                }

                var totalQty = products.Sum(x => x.CurrentInventory);
                var totalPrice = products.Sum(x => x.CurrentInventory * x.PriceLevel0);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(new InventorySummaryProductViewModel(product));
                    }
                    TotalQtyText = $"Current Inventory Total: {Math.Round(totalQty, Config.Round)}";
                    TotalWeightText = $"Current Inventory Weight Total: {Math.Round(weight, Config.Round)}";
                    TotalPriceText = $"Inventory Total Price: {Math.Round(totalPrice, Config.Round).ToCustomString()}";
                });
            });
        }

        [RelayCommand]
        private void Filter()
        {
            ShowingAll = !ShowingAll;
            // TODO: Implement filter logic
        }

        [RelayCommand]
        private async Task Print()
        {
            // TODO: Implement print functionality
            await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
        }
    }

    public partial class InventorySummaryProductViewModel : ObservableObject
    {
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private double _currentInventory;

        public Product Product { get; }

        public InventorySummaryProductViewModel(Product product)
        {
            Product = product;
            ProductName = product.Name;
            CurrentInventory = product.CurrentInventory;
        }
    }
}

