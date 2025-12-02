using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ViewPrintInventoryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<InventoryProductViewModel> _products = new();
        [ObservableProperty] private string _totalQtyText = string.Empty;
        [ObservableProperty] private string _totalAmountText = string.Empty;
        [ObservableProperty] private bool _showAmount;
        [ObservableProperty] private bool _showPrintButton;

        public ViewPrintInventoryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowAmount = !Config.HidePriceInTransaction && !Config.Wstco;
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
                var productList = new System.Collections.Generic.List<InventoryProductViewModel>();
                double amount = 0;
                double qty = 0;

                var sortedProducts = SortDetails.SortedDetails(
                    Product.Products.Where(x => 
                        Math.Round(x.CurrentInventory, Config.Round) != 0 && 
                        x.ProductType == ProductType.Inventory).ToList());

                foreach (var p in sortedProducts)
                {
                    if (p.CategoryId == 0 && p.RequestedLoadInventory == 0)
                        continue;

                    var viewModel = new InventoryProductViewModel
                    {
                        Product = p,
                        ProductName = p.Name
                    };

                    if (p.UseLot && p.ProductInv?.TruckInventories != null)
                    {
                        var lots = string.Empty;
                        foreach (var item in p.ProductInv.TruckInventories)
                        {
                            lots += $"Lot: {item.Lot}     Qty: {item.CurrentQty}\n";
                            qty += item.CurrentQty;
                            amount += item.CurrentQty * p.PriceLevel0;
                            viewModel.Qty += item.CurrentQty;
                        }
                        viewModel.Lots = lots;
                        viewModel.ShowLots = true;
                    }
                    else
                    {
                        qty += p.CurrentInventory;
                        amount += p.CurrentInventory * p.PriceLevel0;
                        viewModel.Qty = p.CurrentInventory;
                        viewModel.ShowLots = false;
                    }

                    productList.Add(viewModel);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Products.Clear();
                    foreach (var item in productList)
                    {
                        Products.Add(item);
                    }
                    TotalQtyText = $"Qty: {Math.Round(qty, 2)}";
                    TotalAmountText = $"Amount: {amount.ToCustomString()}";
                });
            });
        }

        [RelayCommand]
        private async Task Search()
        {
            var searchText = await _dialogService.ShowPromptAsync("Search", "Enter product name", "OK", "Cancel", "", -1, "");
            if (!string.IsNullOrEmpty(searchText))
            {
                var upper = searchText.ToUpper();
                var filtered = Products.Where(x => x.ProductName.ToUpper().Contains(upper)).ToList();
                
                if (filtered.Count > 0)
                {
                    Products.Clear();
                    foreach (var item in filtered)
                    {
                        Products.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Product not found.", "Alert", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    // Convert Products to InventoryProd list
                    var productList = Products.Select(p => new InventoryProd
                    {
                        Product = p.Product,
                        Qty = (float)p.Qty,
                        Lots = p.Lots ?? string.Empty,
                        ProdLots = p.Product.UseLot && p.Product.ProductInv?.TruckInventories != null
                            ? p.Product.ProductInv.TruckInventories
                            : new List<TruckInventory>()
                    }).ToList();

                    for (int i = 0; i < number; i++)
                    {
                        if (!printer.PrintInventoryProd(productList))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing inventory.";
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

    public partial class InventoryProductViewModel : ObservableObject
    {
        [ObservableProperty] private Product _product = null!;
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private double _qty;
        [ObservableProperty] private string _lots = string.Empty;
        [ObservableProperty] private bool _showLots;

        public string QtyText => Math.Round(Qty, Config.Round).ToString();
    }
}

