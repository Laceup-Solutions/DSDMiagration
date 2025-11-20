using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndInventoryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<CCTemplateLine> _lines = new();
        private List<EndInventoryItemViewModel> _allItems = new();
        private string _fileName = string.Empty;
        private string _searchCriteria = string.Empty;
        private bool _validated = false;

        [ObservableProperty] private ObservableCollection<EndInventoryItemViewModel> _inventoryItems = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _showingAll = false;
        [ObservableProperty] private string _filterButtonText = "Filter";

        partial void OnSearchTextChanged(string value)
        {
            _searchCriteria = value?.ToLower() ?? string.Empty;
            Filter();
        }

        partial void OnShowingAllChanged(bool value)
        {
            _showingAll = value;
            FilterButtonText = value ? "Filter" : "All";
            Filter();
        }

        public EndInventoryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _fileName = Path.Combine(Config.DataPath, "endingInventory.xml");
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists(_fileName))
                        LoadState();
                    else
                        PrepareList();
                });

                // Convert CCTemplateLine to EndInventoryItemViewModel for display
                UpdateInventoryList();

                if (Config.ButlerCustomization)
                    ShowingAll = true;

                Filter();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading inventory: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private void PrepareList()
        {
            _lines.Clear();

            if (Config.PrasekCustomization)
            {
                _lines = GeneratePrasekLines();
                return;
            }

            if (Config.ButlerCustomization)
            {
                _lines = GenerateButlerLines();
                return;
            }

            foreach (var p in Product.Products.Where(x => x.ProductType == ProductType.Inventory && x.CategoryId > 0))
            {
                CCTemplateLine line = null;

                if (p.UseLot || p.UnitOfMeasures.Count > 0)
                {
                    line = new CCGroupedTemplateLine() { Product = p, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                    _lines.Add(line);

                    if (Config.EmptyEndingInventory)
                    {
                        if (p.UseLot)
                            foreach (var item in p.ProductInv.TruckInventories)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = item.Lot, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase), Weight = item.Weight };
                                (line as CCGroupedTemplateLine).Details.Add(il);
                            }
                        else
                            foreach (var item in p.UnitOfMeasures)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = "", UoM = item };
                                (line as CCGroupedTemplateLine).Details.Add(il);
                            }
                    }
                    else
                        foreach (var item in p.ProductInv.TruckInventories)
                        {
                            var il = new CycleCountItem() { Product = p, Qty = item.CurrentQty, Lot = item.Lot, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase), Weight = item.Weight };
                            (line as CCGroupedTemplateLine).Details.Add(il);
                        }
                }
                else
                {
                    var qty = p.CurrentInventory;
                    if (Config.EmptyEndingInventory)
                        qty = 0;

                    var il = new CycleCountItem() { Product = p, Qty = qty, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                    line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                    _lines.Add(line);
                }
            }

            _lines = _lines.OrderBy(x => x.Product.Name).ToList();
        }

        private List<CCTemplateLine> GenerateButlerLines()
        {
            var list = new List<CCTemplateLine>();

            foreach (var p in Product.Products.Where(x => x.ProductType == ProductType.Inventory && x.CategoryId > 0))
            {
                CCTemplateLine line = null;

                if (p.UseLot || p.UnitOfMeasures.Count > 0)
                {
                    if (Config.EmptyEndingInventory)
                    {
                        if (p.UseLot)
                            foreach (var item in p.ProductInv.TruckInventories)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = item.Lot, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                list.Add(line);
                            }
                        else
                            foreach (var item in p.UnitOfMeasures)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = "", UoM = item };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = item };
                                list.Add(line);
                            }
                    }
                    else
                    {
                        if (p.UseLot)
                            foreach (var item in p.ProductInv.TruckInventories)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = item.CurrentQty, Lot = item.Lot, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase), Weight = item.Weight };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                list.Add(line);
                            }
                        else
                            foreach (var item in p.UnitOfMeasures)
                            {
                                var qty = p.CurrentInventory;
                                if (!item.IsBase)
                                    qty = p.CurrentInventory / item.Conversion;
                                var il = new CycleCountItem() { Product = p, Qty = qty, Lot = "", UoM = item };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = item };
                                list.Add(line);
                            }
                    }
                }
                else
                {
                    var qty = p.CurrentInventory;
                    if (Config.EmptyEndingInventory)
                        qty = 0;

                    var il = new CycleCountItem() { Product = p, Qty = qty, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                    line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                    list.Add(line);
                }
            }

            return list.OrderBy(x => x.Product.Name).ToList();
        }

        private List<CCTemplateLine> GeneratePrasekLines()
        {
            var list = new List<CCTemplateLine>();

            foreach (var p in Product.Products.Where(x => x.ProductType == ProductType.Inventory && x.CategoryId > 0))
            {
                CCTemplateLine line = null;

                if (Config.PrasekCustomization)
                {
                    if (p.UseLot)
                    {
                        foreach (var item in p.ProductInv.TruckInventories)
                        {
                            if (p.UnitOfMeasures.Count == 0)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = item.Lot, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                list.Add(line);
                            }
                            else
                            {
                                foreach (var uom in p.UnitOfMeasures)
                                {
                                    var il = new CycleCountItem() { Product = p, Qty = 0, Lot = item.Lot, UoM = uom };
                                    line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = uom };
                                    list.Add(line);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (p.UnitOfMeasures.Count == 0)
                        {
                            var il = new CycleCountItem() { Product = p, Qty = 0, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                            line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = p.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                            list.Add(line);
                        }
                        else
                        {
                            foreach (var item in p.UnitOfMeasures)
                            {
                                var il = new CycleCountItem() { Product = p, Qty = 0, Lot = "", UoM = item };
                                line = new CCSingleTemplateLine() { Product = p, Detail = il, UoM = item };
                                list.Add(line);
                            }
                        }
                    }

                    continue;
                }
            }

            return list.OrderBy(x => x.Product.Name).ToList();
        }

        private void LoadState()
        {
            _lines = new List<CCTemplateLine>();

            if (!File.Exists(_fileName))
                return;

            using (StreamReader reader = new StreamReader(_fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(new char[] { (char)20 });

                    int pId = Convert.ToInt32(parts[0]);
                    float qty = Convert.ToSingle(parts[1]);

                    string lot = "";
                    if (parts.Length > 2)
                        lot = parts[2];

                    var product = Product.Products.FirstOrDefault(x => x.ProductId == pId);
                    if (product == null)
                        continue;

                    var cc = new CycleCountItem()
                    {
                        Product = product,
                        Qty = qty,
                        Lot = lot
                    };

                    if (parts.Length > 3)
                        cc.Expiration = new DateTime(Convert.ToInt64(parts[3]));

                    if (parts.Length > 4)
                    {
                        var uomId = Convert.ToInt32(parts[4]);
                        cc.UoM = product.UnitOfMeasures.FirstOrDefault(x => x.Id == uomId);
                    }

                    if (parts.Length > 5)
                    {
                        double Weight = 0;
                        Weight = Convert.ToDouble(parts[5]);
                        cc.Weight = Weight;
                    }

                    if (Config.PrasekCustomization || Config.ButlerCustomization)
                    {
                        var templateLine = new CCSingleTemplateLine() { Product = product, Detail = cc, UoM = cc.UoM };
                        _lines.Add(templateLine);
                    }
                    else
                    {
                        var templateLine = _lines.FirstOrDefault(x => x.Product.ProductId == pId);
                        if (templateLine == null)
                        {
                            if (product.UseLot || product.UnitOfMeasures.Count > 1)
                            {
                                templateLine = new CCGroupedTemplateLine() { Product = product, UoM = product.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };
                                (templateLine as CCGroupedTemplateLine).Details.Add(cc);
                            }
                            else
                                templateLine = new CCSingleTemplateLine() { Product = product, Detail = cc, UoM = product.UnitOfMeasures.FirstOrDefault(x => x.IsBase) };

                            _lines.Add(templateLine);
                        }
                        else if (templateLine is CCGroupedTemplateLine)
                            (templateLine as CCGroupedTemplateLine).Details.Add(cc);
                    }
                }
            }

            _lines = _lines.OrderBy(x => x.Product.Name).ToList();
        }

        private void SaveState()
        {
            if (File.Exists(_fileName))
                File.Delete(_fileName);

            using (StreamWriter writer = new StreamWriter(_fileName))
            {
                foreach (var line in _lines)
                    line.Serialize(writer);
            }
        }

        private void UpdateInventoryList()
        {
            _allItems.Clear();
            foreach (var line in _lines)
            {
                if (line is CCSingleTemplateLine)
                {
                    var singleLine = line as CCSingleTemplateLine;
                    var detail = singleLine.Detail;
                    _allItems.Add(new EndInventoryItemViewModel
                    {
                        ProductId = line.Product.ProductId,
                        ProductName = line.Product.Name ?? "Unknown",
                        CurrentQuantity = line.Product.CurrentInventory,
                        EndingQuantity = detail.Qty,
                        Line = line,
                        Detail = detail
                    });
                }
                else if (line is CCGroupedTemplateLine)
                {
                    var groupedLine = line as CCGroupedTemplateLine;
                    foreach (var detail in groupedLine.Details)
                    {
                        _allItems.Add(new EndInventoryItemViewModel
                        {
                            ProductId = line.Product.ProductId,
                            ProductName = line.Product.Name ?? "Unknown",
                            CurrentQuantity = line.Product.CurrentInventory,
                            EndingQuantity = detail.Qty,
                            Lot = detail.Lot,
                            Weight = detail.Weight,
                            Line = line,
                            Detail = detail
                        });
                    }
                }
            }
        }

        public void FilterInventory(string searchText)
        {
            _searchCriteria = searchText?.ToLower() ?? string.Empty;
            Filter();
        }

        private void Filter()
        {
            InventoryItems.Clear();

            IEnumerable<EndInventoryItemViewModel> filtered;

            if (!_showingAll)
            {
                var filteredList = _allItems.Where(x => 
                    x.ProductName?.ToLower().Contains(_searchCriteria) == true && 
                    x.EndingQuantity != 0);
                filtered = filteredList.ToList();
            }
            else
            {
                var filteredList = _allItems.Where(x => 
                    x.ProductName?.ToLower().Contains(_searchCriteria) == true);
                filtered = filteredList.ToList();
            }

            foreach (var item in filtered)
            {
                InventoryItems.Add(item);
            }
        }

        [RelayCommand]
        private void ToggleFilter()
        {
            ShowingAll = !ShowingAll;
        }

        [RelayCommand]
        private async Task Save()
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Submit Cycle Count",
                "Are you sure you want to submit the cycle count?",
                "Yes",
                "No");

            if (!confirmed)
                return;

            await ContinueSaving();
        }

        private async Task ContinueSaving()
        {
            if (!string.IsNullOrEmpty(Config.InventoryPassword))
            {
                var password = await _dialogService.ShowPromptAsync(
                    "Enter Password",
                    "Enter validation password",
                    "OK",
                    "Cancel",
                    "Password",
                    -1,
                    "",
                    Keyboard.Default);

                if (string.IsNullOrEmpty(password))
                    return;

                if (string.Compare(password, Config.InventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                    return;
                }
            }

            await SaveInventoryAction();
        }

        private async Task SaveInventoryAction()
        {
            try
            {
                _validated = true;
                Config.EndingInventoryCounted = true;

                if (Config.ButlerCustomization)
                {
                    var inventoryItems = new List<InventoryItem>();
                    foreach (var l in _lines)
                    {
                        var item = l as CCSingleTemplateLine;

                        var found = inventoryItems.FirstOrDefault(x => x.ProductId == item.Product.ProductId && x.Lot == item.Detail.Lot);
                        if (found != null)
                        {
                            float factor = 1;
                            if (item.Detail.UoM != null && !item.Detail.UoM.IsBase)
                                factor = item.Detail.UoM.Conversion;

                            var qty = item.Detail.Qty;
                            qty *= factor;

                            found.Qty += qty;
                        }
                        else
                        {
                            float factor = 1;
                            if (item.Detail.UoM != null && !item.Detail.UoM.IsBase)
                                factor = item.Detail.UoM.Conversion;

                            var qty = item.Detail.Qty;
                            qty *= factor;

                            inventoryItems.Add(
                            new InventoryItem()
                            {
                                Qty = qty,
                                Product = item.Product,
                                ProductId = item.Product.ProductId,
                                Lot = item.Detail.Lot,
                                Exp = item.Detail.Expiration,
                                Weight = item.Detail.Weight
                            });
                        }
                    }

                    foreach (var inv in inventoryItems)
                        inv.Product.SetCurrentInventory((float)inv.Qty, inv.Lot, inv.Exp, inv.Weight);
                }
                else if (!Config.PrasekCustomization)
                {
                    // save the values
                    foreach (var item in _lines)
                    {
                        if (item is CCSingleTemplateLine)
                        {
                            var i = item as CCSingleTemplateLine;
                            item.Product.SetCurrentInventory(i.Detail.Qty, i.Detail.Lot, i.Detail.Expiration, i.Detail.Weight);
                        }
                        else
                        {
                            var i = item as CCGroupedTemplateLine;
                            if (item.Product.UseLot)
                                foreach (var det in i.Details)
                                    item.Product.SetCurrentInventory(det.Qty, det.Lot, det.Expiration, det.Weight);
                            else
                                item.Product.SetCurrentInventory(i.Qty, "", DateTime.MinValue, i.Weight);
                        }
                    }
                }
                else
                {
                    foreach (var item in _lines)
                    {
                        if (item is CCSingleTemplateLine)
                        {
                            var i = item as CCSingleTemplateLine;

                            var qty = i.Detail.Qty;
                            if (item.UoM != null && !item.UoM.IsBase)
                                qty *= item.UoM.Conversion;

                            item.Product.SetCurrentInventory(qty, i.Detail.Lot, i.Detail.Expiration, i.Detail.Weight);
                        }
                    }
                }

                DataAccess.SaveInventory();

                if (File.Exists(_fileName))
                    File.Delete(_fileName);

                _validated = true;

                await _dialogService.ShowAlertAsync("Inventory saved successfully.", "Info", "OK");
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

        public async Task<bool> OnBackButtonPressed()
        {
            // Return true to prevent navigation, false to allow it
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Discard Transaction",
                "Backing out will discard the transaction. Continue?",
                "Discard",
                "Cancel");

            if (!confirmed)
                return true; // Prevent navigation

            // Clear list and delete file
            _lines.Clear();

            Config.EndingInventoryCounted = false;

            if (File.Exists(_fileName))
                File.Delete(_fileName);

            // Allow navigation
            await Shell.Current.GoToAsync("..");
            return false;
        }

        public void UpdateQuantity(EndInventoryItemViewModel item, float qty)
        {
            if (item.Detail != null)
            {
                item.Detail.Qty = qty;
                item.EndingQuantity = qty;
                SaveState();
            }
        }

        private class InventoryItem
        {
            public double Qty { get; set; }
            public Product Product { get; set; }
            public int ProductId { get; set; }
            public string Lot { get; set; }
            public DateTime Exp { get; set; }
            public double Weight { get; set; }
        }
    }

    public partial class EndInventoryItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private float _currentQuantity;
        [ObservableProperty] private float _endingQuantity;
        [ObservableProperty] private string _lot = string.Empty;
        [ObservableProperty] private double _weight;
        
        // Reference to the underlying line and detail for updates
        public CCTemplateLine Line { get; set; }
        public CycleCountItem Detail { get; set; }
    }
}
