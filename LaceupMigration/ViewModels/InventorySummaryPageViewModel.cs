using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

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
        [ObservableProperty] private bool _showFilterButton;
        [ObservableProperty] private string _selectedUnitType = "Base";
        [ObservableProperty] private ObservableCollection<string> _uomTypes = new();
        [ObservableProperty] private int _selectedUomIndex = 0;
        [ObservableProperty] private bool _showingAll = false;
        [ObservableProperty] private string _filterButtonText = "All";
        [ObservableProperty] private string _searchQuery = string.Empty;

        public InventorySummaryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowPrice = Config.ShowPricesInInventorySummary;
            ShowWeight = Config.ShowWeightOnInventorySummary || Config.UsePallets;
            ShowPrintButton = Config.PrinterAvailable;
            ShowFilterButton = Config.ShowWarehouseInvInSummary;
            
            // Initialize UoM types (matches Xamarin: Base, Default, Sales)
            UomTypes.Clear();
            UomTypes.Add("Base");
            UomTypes.Add("Default");
            UomTypes.Add("Sales");
        }

        public async Task OnAppearingAsync()
        {
            await LoadInventoryAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            Filter();
        }

        partial void OnSelectedUomIndexChanged(int value)
        {
            if (value >= 0 && value < UomTypes.Count)
            {
                SelectedUnitType = UomTypes[value];
                Filter();
            }
        }

        private void Filter()
        {
            // Match Xamarin's Filter() logic
            var filteredProducts = Product.Products
                .Where(x => x.ProductType == ProductType.Inventory)
                .Where(x => !(x.CategoryId == 0 && x.RequestedLoadInventory == 0))
                .ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var searchUpper = SearchQuery.ToUpperInvariant();
                filteredProducts = filteredProducts.Where(x => x.Name.ToUpperInvariant().Contains(searchUpper)).ToList();
            }

            // Apply showingAll filter (matches Xamarin's Filter logic)
            if (!ShowingAll)
            {
                // Only show products with truck inventories
                if (string.IsNullOrEmpty(SearchQuery))
                {
                    filteredProducts = filteredProducts.Where(x => x.ProductInv?.TruckInventories != null && x.ProductInv.TruckInventories.Count > 0).ToList();
                }
                else
                {
                    filteredProducts = filteredProducts.Where(x => x.ProductInv?.TruckInventories != null && x.ProductInv.TruckInventories.Count > 0 && x.Name.ToUpperInvariant().Contains(SearchQuery.ToUpperInvariant())).ToList();
                }
            }

            // Calculate totals
            var weightItems = filteredProducts.Where(x => x.CurrentInventory > 0 && x.SoldByWeight);
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

            var totalQty = filteredProducts.Sum(x => x.CurrentInventory);
            var totalPrice = filteredProducts.Sum(x => x.CurrentInventory * x.PriceLevel0);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Products.Clear();
                foreach (var product in filteredProducts)
                {
                    Products.Add(new InventorySummaryProductViewModel(product, SelectedUnitType));
                }
                TotalQtyText = $"Current Inventory Total: {Math.Round(totalQty, Config.Round)}";
                TotalWeightText = $"Current Inventory Weight Total: {Math.Round(weight, Config.Round)}";
                TotalPriceText = $"Inventory Total Price: {Math.Round(totalPrice, Config.Round).ToCustomString()}";
            });
        }

        private async Task LoadInventoryAsync()
        {
            await Task.Run(() =>
            {
                Filter();
            });
        }

        [RelayCommand]
        private void FilterToggle()
        {
            // Match Xamarin's Filter_Click - toggle showingAll
            ShowingAll = !ShowingAll;
            FilterButtonText = ShowingAll ? "Filter" : "All";
            Filter();
        }

        [RelayCommand]
        private async Task Print()
        {
            _appService.RecordEvent("Print inventory summary");
            // TODO: Implement print functionality (matches Xamarin's Print_Click)
            await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task ShowBegInvInfo()
        {
            await _dialogService.ShowAlertAsync(
                "B.I. means Beginning Inventory. Is the inventory received the first time you download data from server.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowLoadReqInfo()
        {
            await _dialogService.ShowAlertAsync(
                "R.L. means requested Load. Is the requested inventory in a load order.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowLoadInvInfo()
        {
            await _dialogService.ShowAlertAsync(
                "LD means Loaded Inventory. Is the inventory loaded into the truck when a load or a delivery is accepted.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowTransOnInfo()
        {
            await _dialogService.ShowAlertAsync(
                "T.ON means Transfer On. Is the inventory transferred into the truck from the main warehouse.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowTransOffInfo()
        {
            await _dialogService.ShowAlertAsync(
                "T.OFF means Transfer Off. IS the inventory transferred from the truck to the main warehouse.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowSalesInvInfo()
        {
            await _dialogService.ShowAlertAsync(
                "S.I. means Sales Invoice. Is the inventory consumed in all the sales invoices.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowCreditInvInfo()
        {
            await _dialogService.ShowAlertAsync(
                "CR.I means Credit(Return) Invoice. Is the inventory in good condition return in all the credit (return) invoices.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowCurrInvInfo()
        {
            await _dialogService.ShowAlertAsync(
                "C.I. means Current Inventory. Is the inventory available in the truck.",
                "Info",
                "OK");
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            var options = new[] { "Advanced Options" };
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options);
            
            switch (choice)
            {
                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            var options = new List<string>
            {
                "Update settings",
                "Send log file",
                "Export data",
                "Remote control"
            };

            if (Config.GoToMain)
            {
                options.Add("Go to main activity");
            }

            var choice = await _dialogService.ShowActionSheetAsync("Advanced options", "Cancel", null, options.ToArray());
            switch (choice)
            {
                case "Update settings":
                    await _appService.UpdateSalesmanSettingsAsync();
                    await _dialogService.ShowAlertAsync("Settings updated.", "Info");
                    break;
                case "Send log file":
                    await _appService.SendLogAsync();
                    await _dialogService.ShowAlertAsync("Log sent.", "Info");
                    break;
                case "Export data":
                    await _appService.ExportDataAsync();
                    await _dialogService.ShowAlertAsync("Data exported.", "Info");
                    break;
                case "Remote control":
                    await _appService.RemoteControlAsync();
                    break;
                case "Go to main activity":
                    await _appService.GoBackToMainAsync();
                    break;
            }
        }
    }

    public partial class InventorySummaryProductViewModel : ObservableObject
    {
        private readonly Product _product;
        private readonly string _selectedUnitType;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _uom = string.Empty;
        [ObservableProperty] private bool _showUom;
        
        // Aggregated inventory values (sum of all truck inventories)
        [ObservableProperty] private string _beginningInventory = "0";
        [ObservableProperty] private string _requestedLoad = "0";
        [ObservableProperty] private string _loaded = "0";
        [ObservableProperty] private string _transferredOn = "0";
        [ObservableProperty] private string _transferredOff = "0";
        [ObservableProperty] private string _onSales = "0";
        [ObservableProperty] private string _onCreditReturn = "0";
        [ObservableProperty] private string _currentInventory = "0";

        public Product Product => _product;

        public InventorySummaryProductViewModel(Product product, string selectedUnitType)
        {
            _product = product;
            _selectedUnitType = selectedUnitType;
            ProductName = product.Name;

            // Calculate UoM factor based on selected unit type
            double uomFactor = 1;
            string uomNameString = string.Empty;

            var uoms = product.UnitOfMeasures?.ToList() ?? new System.Collections.Generic.List<UnitOfMeasure>();
            if (uoms.Count > 0)
            {
                switch (selectedUnitType)
                {
                    case "Base":
                        var uom = uoms.FirstOrDefault(x => x.IsBase);
                        if (uom != null)
                        {
                            uomFactor = 1;
                            uomNameString = uom.Name;
                        }
                        break;
                    case "Sales":
                        var uom1 = uoms.FirstOrDefault(x => x.DefaultPurchase == "True");
                        if (uom1 != null)
                        {
                            uomFactor = uom1.Conversion;
                            uomNameString = uom1.Name;
                        }
                        break;
                    case "Default":
                        var uom2 = uoms.FirstOrDefault(x => x.IsDefault);
                        if (uom2 != null)
                        {
                            uomFactor = uom2.Conversion;
                            uomNameString = uom2.Name;
                        }
                        break;
                }
            }

            if (!string.IsNullOrEmpty(uomNameString))
            {
                Uom = $"UoM: {uomNameString}";
                ShowUom = true;
            }

            // Aggregate values from all truck inventories (matches Xamarin's InventoryAdapter2 logic)
            if (product.ProductInv?.TruckInventories != null && product.ProductInv.TruckInventories.Count > 0)
            {
                // Remove empty inventories (matches Xamarin logic)
                var nonEmptyInventories = product.ProductInv.TruckInventories
                    .Where(x => x.Unloaded != 0 || x.OnCreditReturn != 0 || x.OnSales != 0 || 
                               x.Loaded != 0 || x.RequestedLoad != 0 || x.BeginingInventory != 0 || 
                               x.CurrentQty != 0 || x.TransferredOff != 0 || x.TransferredOn != 0)
                    .ToList();

                if (nonEmptyInventories.Count > 0)
                {
                    // Sum all truck inventories
                    double begInv = nonEmptyInventories.Sum(x => x.BeginingInventory);
                    double reqLoad = nonEmptyInventories.Sum(x => x.RequestedLoad);
                    double loaded = nonEmptyInventories.Sum(x => x.Loaded);
                    double transOn = nonEmptyInventories.Sum(x => x.TransferredOn);
                    double transOff = nonEmptyInventories.Sum(x => x.TransferredOff);
                    double onSales = nonEmptyInventories.Sum(x => x.OnSales);
                    double onCredit = nonEmptyInventories.Sum(x => x.OnCreditReturn);
                    double current = nonEmptyInventories.Sum(x => x.CurrentQty);

                    // Apply UoM factor and round
                    BeginningInventory = Math.Round(begInv / uomFactor, Config.Round).ToString();
                    RequestedLoad = Math.Round(reqLoad / uomFactor, Config.Round).ToString();
                    Loaded = Math.Round(loaded / uomFactor, Config.Round).ToString();
                    TransferredOn = Math.Round(transOn / uomFactor, Config.Round).ToString();
                    TransferredOff = Math.Round(transOff / uomFactor, Config.Round).ToString();
                    OnSales = Math.Round(onSales / uomFactor, Config.Round).ToString();
                    OnCreditReturn = Math.Round(onCredit / uomFactor, Config.Round).ToString();
                    CurrentInventory = Config.DisolCrap ? current.ToString() : Math.Round(current / uomFactor, Config.Round).ToString();
                }
            }
        }
    }
}
