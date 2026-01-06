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
                        UoM = detail.UoM ?? line.UoM,
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
                            UoM = detail.UoM ?? line.UoM,
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
        private async Task EditQuantity(EndInventoryItemViewModel item)
        {
            await ShowEditQuantityDialog(item);
        }

        [RelayCommand]
        private async Task AddQuantity(EndInventoryItemViewModel item)
        {
            await ShowAddQtyDialog(item);
        }

        [RelayCommand]
        private async Task Save()
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Alert",
                "Are you sure you are done counting your inventory? Any uncounted items will be sent as quantity 0.",
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
                    // save the values with UoM conversion
                    foreach (var item in _lines)
                    {
                        if (item is CCSingleTemplateLine)
                        {
                            var i = item as CCSingleTemplateLine;
                            var qty = i.Detail.Qty;
                            // Convert to base UoM if detail has non-base UoM
                            if (i.Detail.UoM != null && !i.Detail.UoM.IsBase)
                                qty *= i.Detail.UoM.Conversion;
                            item.Product.SetCurrentInventory(qty, i.Detail.Lot, i.Detail.Expiration, i.Detail.Weight);
                        }
                        else
                        {
                            var i = item as CCGroupedTemplateLine;
                            if (item.Product.UseLot)
                            {
                                foreach (var det in i.Details)
                                {
                                    var qty = det.Qty;
                                    // Convert to base UoM if detail has non-base UoM
                                    if (det.UoM != null && !det.UoM.IsBase)
                                        qty *= det.UoM.Conversion;
                                    item.Product.SetCurrentInventory(qty, det.Lot, det.Expiration, det.Weight);
                                }
                            }
                            else
                            {
                                var qty = i.Qty;
                                // For grouped lines, use the line's UoM if available
                                if (item.UoM != null && !item.UoM.IsBase)
                                    qty *= item.UoM.Conversion;
                                item.Product.SetCurrentInventory(qty, "", DateTime.MinValue, i.Weight);
                            }
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

                ProductInventory.Save();

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

            // [ACTIVITY STATE]: Remove state when properly exiting
            Helpers.NavigationHelper.RemoveNavigationState("endinventory");

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
        
        public async Task ShowAddQtyDialog(EndInventoryItemViewModel item)
        {
            // Similar to TransferOnOffPageViewModel.ShowAddQtyDialog
            // Find the product - if item has a line, use that product; otherwise find by ProductId
            Product product = null;
            if (item.Line != null)
            {
                product = item.Line.Product;
            }
            else
            {
                product = Product.Products.FirstOrDefault(x => x.ProductId == item.ProductId);
            }

            if (product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found", "Error", "OK");
                return;
            }

            // Get default UoM
            UnitOfMeasure defaultUoM = item.UoM ?? product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);

            // Show dialog with Qty and UoM selection
            var result = await _dialogService.ShowTransferQtyDialogAsync(
                product.Name,
                product,
                "1",
                defaultUoM);

            if (result.qty == null) return; // User cancelled

            if (string.IsNullOrEmpty(result.qty) || !float.TryParse(result.qty, out var qty))
                return;

            // Use absolute value
            qty = Math.Abs(qty);

            // Use the selected UoM from the dialog
            UnitOfMeasure selectedUoM = result.selectedUoM ?? defaultUoM;

            // Find or create the line in _lines (the source of truth)
            CCTemplateLine templateLine = null;
            if (item.Line != null)
            {
                templateLine = _lines.FirstOrDefault(x => x.Product.ProductId == item.Line.Product.ProductId);
            }
            else
            {
                templateLine = _lines.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
            }

            string lot = "";
            DateTime lotExp = DateTime.MinValue;
            double weight = 0;

            // If line exists, check if it's grouped or single
            if (templateLine != null)
            {
                if (templateLine is CCGroupedTemplateLine)
                {
                    // Check if detail with same UoM and Lot already exists
                    var groupedLine = templateLine as CCGroupedTemplateLine;
                    var existingDetail = groupedLine.Details.FirstOrDefault(x => 
                        x.UoM == selectedUoM && x.Lot == lot);

                    if (existingDetail != null)
                    {
                        // Update existing detail
                        existingDetail.Qty = qty;
                        existingDetail.Lot = lot;
                        existingDetail.Weight = weight;
                    }
                    else
                    {
                        // Add new detail to grouped line
                        var newDetail = new CycleCountItem
                        {
                            Product = product,
                            Qty = qty,
                            UoM = selectedUoM,
                            Lot = lot,
                            Expiration = lotExp,
                            Weight = weight
                        };
                        groupedLine.Details.Add(newDetail);
                    }
                }
                else if (templateLine is CCSingleTemplateLine)
                {
                    // Convert single line to grouped line if needed, or update existing
                    var singleLine = templateLine as CCSingleTemplateLine;
                    
                    // If UoM matches, update existing detail
                    if (singleLine.Detail.UoM == selectedUoM && singleLine.Detail.Lot == lot)
                    {
                        singleLine.Detail.Qty = qty;
                        singleLine.Detail.Lot = lot;
                        singleLine.Detail.Weight = weight;
                    }
                    else
                    {
                        // Convert to grouped line and add new detail
                        var groupedLine = new CCGroupedTemplateLine
                        {
                            Product = product,
                            UoM = product.UnitOfMeasures.FirstOrDefault(x => x.IsBase)
                        };
                        
                        // Add existing detail
                        groupedLine.Details.Add(singleLine.Detail);
                        
                        // Add new detail
                        var newDetail = new CycleCountItem
                        {
                            Product = product,
                            Qty = qty,
                            UoM = selectedUoM,
                            Lot = lot,
                            Expiration = lotExp,
                            Weight = weight
                        };
                        groupedLine.Details.Add(newDetail);
                        
                        // Replace single line with grouped line
                        var index = _lines.IndexOf(templateLine);
                        _lines[index] = groupedLine;
                    }
                }
            }
            else
            {
                // Create new line - determine if it should be grouped or single
                if (product.UseLot || product.UnitOfMeasures.Count > 1)
                {
                    // Create grouped line
                    var groupedLine = new CCGroupedTemplateLine
                    {
                        Product = product,
                        UoM = product.UnitOfMeasures.FirstOrDefault(x => x.IsBase)
                    };
                    
                    var newDetail = new CycleCountItem
                    {
                        Product = product,
                        Qty = qty,
                        UoM = selectedUoM,
                        Lot = lot,
                        Expiration = lotExp,
                        Weight = weight
                    };
                    groupedLine.Details.Add(newDetail);
                    _lines.Add(groupedLine);
                }
                else
                {
                    // Create single line
                    var newDetail = new CycleCountItem
                    {
                        Product = product,
                        Qty = qty,
                        UoM = selectedUoM,
                        Lot = lot,
                        Expiration = lotExp,
                        Weight = weight
                    };
                    var singleLine = new CCSingleTemplateLine
                    {
                        Product = product,
                        Detail = newDetail,
                        UoM = selectedUoM
                    };
                    _lines.Add(singleLine);
                }
            }

            SaveState();

            // Refresh the display
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateInventoryList();
                Filter();
            });
        }

        public async Task ShowEditQuantityDialog(EndInventoryItemViewModel item)
        {
            // Match Xamarin EndInventoryActivity: show dialog to edit quantity and UoM
            if (item.Detail == null || item.Line == null)
                return;
            
            var product = item.Line.Product;
            var initialQty = item.EndingQuantity.ToString();
            var initialUoM = item.UoM ?? item.Line.UoM;
            
            // Use ShowTransferQtyDialogAsync for consistent UI with transfer screen
            var result = await _dialogService.ShowTransferQtyDialogAsync(
                product.Name,
                product,
                initialQty,
                initialUoM,
                "Save");

            if (result.qty == null)
                return; // User cancelled

            if (string.IsNullOrEmpty(result.qty) || !float.TryParse(result.qty, out var qty))
                return;

            // Use absolute value
            qty = Math.Abs(qty);

            // Use the selected UoM from the dialog
            UnitOfMeasure selectedUoM = result.selectedUoM ?? initialUoM;
            
            // CRITICAL: Always work with the underlying _lines data structure
            var templateLine = _lines.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
            
            if (templateLine == null)
            {
                await _dialogService.ShowAlertAsync("Line not found in inventory list", "Error", "OK");
                return;
            }

            // Find the detail in the underlying structure
            CycleCountItem underlyingDetail = null;
            
            if (templateLine is CCGroupedTemplateLine)
            {
                var groupedLine = templateLine as CCGroupedTemplateLine;
                // Try to find by matching UoM and Lot
                underlyingDetail = groupedLine.Details.FirstOrDefault(x => 
                    x.UoM == item.Detail.UoM && 
                    x.Lot == item.Detail.Lot &&
                    Math.Abs(x.Qty - item.Detail.Qty) < 0.001f);
                
                // If not found, try just by UoM
                if (underlyingDetail == null)
                {
                    underlyingDetail = groupedLine.Details.FirstOrDefault(x => x.UoM == item.Detail.UoM);
                }
            }
            else if (templateLine is CCSingleTemplateLine)
            {
                var singleLine = templateLine as CCSingleTemplateLine;
                underlyingDetail = singleLine.Detail;
            }

            if (qty == 0)
            {
                // Remove detail if qty is 0
                if (underlyingDetail != null && templateLine is CCGroupedTemplateLine)
                {
                    var groupedLine = templateLine as CCGroupedTemplateLine;
                    groupedLine.Details.Remove(underlyingDetail);
                    
                    // If no details left, remove the line
                    if (groupedLine.Details.Count == 0)
                    {
                        _lines.Remove(templateLine);
                    }
                }
                else if (underlyingDetail != null && templateLine is CCSingleTemplateLine)
                {
                    // For single line, just set qty to 0 (don't remove the line)
                    underlyingDetail.Qty = 0;
                }
            }
            else
            {
                // Check if UoM changed
                bool uomChanged = selectedUoM != item.Detail.UoM;
                
                if (underlyingDetail != null)
                {
                    if (uomChanged && templateLine is CCGroupedTemplateLine)
                    {
                        // Check if detail with new UoM already exists
                        var groupedLine = templateLine as CCGroupedTemplateLine;
                        var existingDetailWithNewUom = groupedLine.Details.FirstOrDefault(x => 
                            x.UoM == selectedUoM && x != underlyingDetail);
                        
                        if (existingDetailWithNewUom != null)
                        {
                            // Merge: update existing detail with new UoM and remove the old one
                            existingDetailWithNewUom.Qty = qty;
                            existingDetailWithNewUom.Lot = item.Detail.Lot;
                            existingDetailWithNewUom.Expiration = item.Detail.Expiration;
                            existingDetailWithNewUom.Weight = item.Detail.Weight;
                            groupedLine.Details.Remove(underlyingDetail);
                        }
                        else
                        {
                            // Just update the UoM
                            underlyingDetail.Qty = qty;
                            underlyingDetail.UoM = selectedUoM;
                            underlyingDetail.Lot = item.Detail.Lot;
                            underlyingDetail.Expiration = item.Detail.Expiration;
                            underlyingDetail.Weight = item.Detail.Weight;
                        }
                    }
                    else
                    {
                        // Update the detail (UoM didn't change)
                        underlyingDetail.Qty = qty;
                        underlyingDetail.Lot = item.Detail.Lot;
                        underlyingDetail.Expiration = item.Detail.Expiration;
                        underlyingDetail.Weight = item.Detail.Weight;
                        
                        if (uomChanged)
                        {
                            underlyingDetail.UoM = selectedUoM;
                        }
                    }
                }
            }
            
            SaveState();
            
            // Refresh the display
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateInventoryList();
                Filter();
            });
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
        private UnitOfMeasure _uom;
        
        // Reference to the underlying line and detail for updates
        public CCTemplateLine Line { get; set; }
        public CycleCountItem Detail { get; set; }
        
        public UnitOfMeasure UoM
        {
            get => _uom ?? Detail?.UoM;
            set
            {
                if (_uom != value)
                {
                    _uom = value;
                    if (Detail != null)
                        Detail.UoM = value;
                    if (Line != null)
                        Line.UoM = value;
                    OnPropertyChanged(nameof(UoM));
                    OnPropertyChanged(nameof(UomText));
                    OnPropertyChanged(nameof(ShowUom));
                }
            }
        }
        
        public string UomText
        {
            get
            {
                if (UoM != null)
                    return $"UoM: {UoM.Name}";
                return string.Empty;
            }
        }
        
        public bool ShowUom => UoM != null;
    }
}
