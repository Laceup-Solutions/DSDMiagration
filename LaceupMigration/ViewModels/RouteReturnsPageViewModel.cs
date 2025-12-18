using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class RouteReturnsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<RouteReturnViewModel> _allReturns = new();
        private string _fileName = string.Empty;
        private List<RRTemplateLine> _lines = new();
        private bool _saved = false;
        private bool _changed = false;
        private bool _emptyTruckOption = false;

        [ObservableProperty] private ObservableCollection<RouteReturnViewModel> _returns = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _showingAll = false;
        [ObservableProperty] private bool _isSaved;
        [ObservableProperty] private string _saveButtonText = "Save";
        [ObservableProperty] private string _filterButtonText = "Filter";
        
        partial void OnSearchTextChanged(string value)
        {
            FilterReturns(value);
        }
        
        partial void OnShowingAllChanged(bool value)
        {
            FilterButtonText = value ? "All" : "Filter";
            FilterReturns(SearchText);
        }

        public RouteReturnsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _fileName = Path.Combine(Config.DataPath, "routeReturn.xml");
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // Check if already saved (from file existence or state)
                var routeReturnFile = Path.Combine(Config.DataPath, "routeReturn.xml");
                _saved = File.Exists(routeReturnFile);
                IsSaved = _saved;
                
                // Set button text based on EmptyTruckAtEndOfDay
                if (Config.EmptyTruckAtEndOfDay)
                {
                    SaveButtonText = "Validate";
                }
                else
                {
                    SaveButtonText = "Save";
                }
                
                await Task.Run(() =>
                {
                    PrepareList();
                    LoadState();
                });
                
                // Convert RRTemplateLine to RouteReturnViewModel for display
                UpdateReturnsList();
                
                ShowingAll = false;
                FilterReturns(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading returns: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
        
        private void UpdateReturnsList()
        {
            _allReturns.Clear();
            foreach (var line in _lines)
            {
                var returnItem = new RouteReturnViewModel
                {
                    ProductId = line.Product.ProductId,
                    ProductName = line.Product.Name ?? "Unknown",
                    Reships = line.Reships,
                    Returns = line.Returns,
                    Dumps = line.Dumps,
                    DamagedInTruck = line.DamagedInTruck,
                    Unload = line.Unload,
                    Lot = line.Lot,
                    Weight = line.Weight
                };
                _allReturns.Add(returnItem);
            }
        }
        
        private void PrepareList()
        {
            _lines.Clear();
            
            // Support ButlerCustomization
            if (Config.ButlerCustomization)
            {
                _lines = PrepareListForButler(Config.EmptyTruckAtEndOfDay && Config.CalculateInvForEmptyTruck);
                return;
            }
            
            // Prepare list based on inventory products and orders
            var emptyTruck = Config.EmptyTruckAtEndOfDay && Config.CalculateInvForEmptyTruck;
            
            foreach (var item in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
            {
                if (item.CategoryId == 0 && item.RequestedLoadInventory == 0)
                    continue;
                
                RRTemplateLine line;
                
                if (item.UseLot || item.SoldByWeight)
                {
                    line = new RRGroupedLine() { Product = item };
                    _lines.Add(line);
                    
                    if (emptyTruck)
                    {
                        foreach (var itemLot in item.ProductInv.TruckInventories)
                        {
                            if (itemLot.CurrentQty <= 0)
                                continue;
                            
                            var qty = itemLot.CurrentQty < 0 ? 0 : itemLot.CurrentQty;
                            var detail = new RouteReturnLine() 
                            { 
                                Product = item, 
                                Lot = itemLot.Lot, 
                                Unload = qty, 
                                Weight = itemLot.Weight 
                            };
                            (line as RRGroupedLine).Details.Add(detail);
                        }
                    }
                }
                else
                {
                    line = new RRSingleLine() 
                    { 
                        Product = item, 
                        Detail = new RouteReturnLine() 
                        { 
                            Product = item, 
                            Lot = string.Empty, 
                            Weight = item.Weight 
                        } 
                    };
                    _lines.Add(line);
                    
                    if (emptyTruck)
                    {
                        (line as RRSingleLine).AddUnload(item.CurrentInventory < 0 ? 0 : item.CurrentInventory);
                    }
                }
            }
            
            // Add reships and returns from orders
            foreach (var o in Order.Orders)
            {
                if (!o.AsPresale && !o.Voided)
                {
                    foreach (var od in o.Details)
                    {
                        if (o.Reshipped)
                        {
                            var line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId);
                            
                            // Support UsePallets logic
                            if (Config.UsePallets)
                            {
                                if (od.Product.SoldByWeight && od.Product.UseLot)
                                {
                                    line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Lot == od.Lot && x.Weight == od.Weight);
                                }
                                else if (od.Product.SoldByWeight && !od.Product.UseLot)
                                {
                                    line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Weight == od.Weight);
                                }
                            }
                            
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                _lines.Add(line);
                            }
                            
                            float factor = 1;
                            if (od.UnitOfMeasure != null)
                                factor = od.UnitOfMeasure.Conversion;
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (line is RRSingleLine)
                                (line as RRSingleLine).AddReships(qty * factor);
                            else
                                (line as RRGroupedLine).AddReships(qty * factor, od.Lot, od.Weight);
                        }
                        else if (od.IsCredit)
                        {
                            var line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId);
                            
                            // Support UsePallets logic
                            if (Config.UsePallets)
                            {
                                if (od.Product.SoldByWeight && od.Product.UseLot)
                                {
                                    line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Lot == od.Lot && x.Weight == od.Weight);
                                }
                                else if (od.Product.SoldByWeight && !od.Product.UseLot)
                                {
                                    line = _lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Weight == od.Weight);
                                }
                            }
                            
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                _lines.Add(line);
                            }
                            
                            float factor = 1;
                            if (od.UnitOfMeasure != null)
                                factor = od.UnitOfMeasure.Conversion;
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (od.Damaged)
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddDumps(qty * factor);
                                else
                                    (line as RRGroupedLine).AddDumps(qty * factor, od.Lot, od.Weight);
                            }
                            else
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddReturns(qty * factor);
                                else
                                    (line as RRGroupedLine).AddReturns(qty * factor, od.Lot, od.Weight);
                            }
                        }
                    }
                }
            }
            
            _lines = _lines.OrderBy(x => x.Product.Name).ToList();
        }
        
        private List<RRTemplateLine> PrepareListForButler(bool emptyTruck)
        {
            var lines = new List<RRTemplateLine>();
            
            foreach (var item in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
            {
                if (item.CategoryId == 0 && item.RequestedLoadInventory == 0)
                    continue;
                
                RRTemplateLine line;
                
                if (item.UseLot)
                {
                    if (emptyTruck)
                    {
                        foreach (var itemLot in item.ProductInv.TruckInventories)
                        {
                            line = new RRSingleLine() { Product = item };
                            var qty = itemLot.CurrentQty < 0 ? 0 : itemLot.CurrentQty;
                            var detail = new RouteReturnLine() { Product = item, Lot = itemLot.Lot, Unload = qty };
                            (line as RRSingleLine).Detail = detail;
                            lines.Add(line);
                        }
                    }
                }
                else
                {
                    if (item.UnitOfMeasures.Count > 0)
                    {
                        foreach (var uom in item.UnitOfMeasures)
                        {
                            line = new RRSingleLine() { Product = item, Detail = new RouteReturnLine() { Product = item, UoM = uom } };
                            lines.Add(line);
                            
                            float inventoryInDefault = 0;
                            if (!uom.IsBase)
                            {
                                inventoryInDefault = item.CurrentInventory / uom.Conversion;
                            }
                            
                            if (emptyTruck)
                            {
                                if (uom.IsBase)
                                    (line as RRSingleLine).AddUnload(item.CurrentInventory < 0 ? 0 : item.CurrentInventory);
                                else
                                    (line as RRSingleLine).AddUnload(inventoryInDefault < 0 ? 0 : inventoryInDefault);
                            }
                        }
                    }
                    else
                    {
                        line = new RRSingleLine() { Product = item, Detail = new RouteReturnLine() { Product = item } };
                        lines.Add(line);
                        
                        if (emptyTruck)
                            (line as RRSingleLine).AddUnload(item.CurrentInventory < 0 ? 0 : item.CurrentInventory);
                    }
                }
            }
            
            // Add reships and returns from orders
            foreach (var o in Order.Orders)
            {
                if (!o.AsPresale && !o.Voided)
                {
                    foreach (var od in o.Details)
                    {
                        if (o.Reshipped)
                        {
                            var line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && 
                                (x as RRSingleLine).Detail != null && (x as RRSingleLine).Detail.UoM == od.UnitOfMeasure);
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                lines.Add(line);
                            }
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (line is RRSingleLine)
                                (line as RRSingleLine).AddReships(qty);
                            else
                                (line as RRGroupedLine).AddReships(qty, od.Lot, od.Weight);
                        }
                        else if (od.IsCredit)
                        {
                            var line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && 
                                (x as RRSingleLine).Detail != null && (x as RRSingleLine).Detail.UoM == od.UnitOfMeasure);
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                lines.Add(line);
                            }
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (od.Damaged)
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddDumps(qty);
                            }
                            else
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddReturns(qty);
                            }
                        }
                    }
                }
            }
            
            return lines.OrderBy(x => x.Product.Name).ToList();
        }
        
        private void LoadState()
        {
            if (!File.Exists(_fileName))
                return;
            
            using (StreamReader reader = new StreamReader(_fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(new char[] { ',' });
                    int productId = Convert.ToInt32(parts[0]);
                    var item = _lines.FirstOrDefault(x => x.Product.ProductId == productId);
                    
                    if (item != null)
                    {
                        if (item is RRSingleLine)
                        {
                            var detail = (item as RRSingleLine).Detail;
                            if (detail == null)
                                detail = new RouteReturnLine() { Product = item.Product };
                            
                            detail.DamagedInTruck = Convert.ToSingle(parts[1]);
                            detail.Unload = Convert.ToSingle(parts[2]);
                            detail.Dumps = Convert.ToSingle(parts[3]);
                            detail.Returns = parts.Length > 4 ? Convert.ToSingle(parts[4]) : 0;
                            detail.Lot = parts.Length > 5 ? parts[5] : "";
                            detail.Expiration = parts.Length > 6 ? new DateTime(Convert.ToInt64(parts[6])) : DateTime.MinValue;
                            detail.Reships = parts.Length > 7 ? Convert.ToSingle(parts[7]) : 0;
                            detail.Weight = parts.Length > 8 ? Convert.ToDouble(parts[8]) : 0;
                        }
                        else
                        {
                            var detail = new RouteReturnLine { Product = item.Product };
                            detail.DamagedInTruck = Convert.ToSingle(parts[1]);
                            detail.Unload = Convert.ToSingle(parts[2]);
                            detail.Dumps = Convert.ToSingle(parts[3]);
                            detail.Returns = parts.Length > 4 ? Convert.ToSingle(parts[4]) : 0;
                            detail.Lot = parts.Length > 5 ? parts[5] : "";
                            detail.Expiration = parts.Length > 6 ? new DateTime(Convert.ToInt64(parts[6])) : DateTime.MinValue;
                            detail.Reships = parts.Length > 7 ? Convert.ToSingle(parts[7]) : 0;
                            detail.Weight = parts.Length > 8 ? Convert.ToDouble(parts[8]) : 0;
                            
                            // Support UsePallets logic (from Xamarin)
                            if (Config.UsePallets)
                            {
                                RRTemplateLine ll = null;
                                if (detail.Product.SoldByWeight && detail.Product.UseLot)
                                    ll = _lines.FirstOrDefault(x => x.Product.ProductId == detail.Product.ProductId && x.Lot == detail.Lot && x.Weight == detail.Weight);
                                else if (detail.Product.SoldByWeight && !detail.Product.UseLot)
                                    ll = _lines.FirstOrDefault(x => x.Product.ProductId == detail.Product.ProductId && x.Weight == detail.Weight);
                                
                                if (ll != null)
                                {
                                    var f = (ll as RRGroupedLine);
                                    f.AddUnload(detail.Unload, detail.Lot, detail.Weight);
                                    f.AddDamagedInTruck(detail.DamagedInTruck, detail.Lot, detail.Weight);
                                }
                                else
                                {
                                    (item as RRGroupedLine).Details.Add(detail);
                                }
                            }
                            else
                            {
                                (item as RRGroupedLine).Details.Add(detail);
                            }
                        }
                    }
                }
            }
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

        public void FilterReturns(string searchText)
        {
            Returns.Clear();

            IEnumerable<RouteReturnViewModel> filtered;
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var upper = searchText.ToUpperInvariant();
                filtered = _allReturns.Where(x => 
                    x.ProductName?.ToUpperInvariant().Contains(upper) == true).ToList();
            }
            else if (!ShowingAll)
            {
                // Show only items with values
                filtered = _allReturns.Where(x => 
                    x.Reships != 0 || x.Dumps != 0 || x.Returns != 0 || 
                    x.DamagedInTruck != 0 || x.Unload != 0).ToList();
                
                // Support ButlerCustomization: if filtering, show all products with same ProductId
                if (Config.ButlerCustomization)
                {
                    var tempListToAdd = new List<RouteReturnViewModel>();
                    foreach (var line in filtered)
                    {
                        var samePId = _allReturns.Where(x => x.ProductId == line.ProductId);
                        if (samePId.Count() > 1)
                        {
                            foreach (var s in samePId)
                            {
                                if (!filtered.Contains(s))
                                    tempListToAdd.Add(s);
                            }
                        }
                    }
                    
                    if (tempListToAdd.Count > 0)
                        filtered = filtered.Concat(tempListToAdd).OrderBy(x => x.ProductName).ToList();
                }
            }
            else
            {
                filtered = _allReturns;
            }

            foreach (var returnItem in filtered)
            {
                Returns.Add(returnItem);
            }
        }
        
        [RelayCommand]
        private void ToggleFilter()
        {
            ShowingAll = !ShowingAll;
            FilterReturns(SearchText);
        }

        [RelayCommand]
        private async Task AddReturn()
        {
            // Route returns are automatically populated from orders and inventory
            // User can edit quantities directly in the list
            await _dialogService.ShowAlertAsync("Route returns are automatically populated from orders and inventory. Edit quantities directly in the list.", "Info", "OK");
        }
        
        [RelayCommand]
        private async Task EditDamaged(RouteReturnViewModel returnItem)
        {
            if (returnItem == null || _saved)
                return;
            
            await EditField(returnItem, "Damaged", returnItem.DamagedInTruck, (line, qty) =>
            {
                if (line is RRSingleLine)
                    (line as RRSingleLine).Detail.DamagedInTruck = qty;
                returnItem.DamagedInTruck = qty;
            });
        }
        
        [RelayCommand]
        private async Task EditUnload(RouteReturnViewModel returnItem)
        {
            if (returnItem == null || _saved)
                return;
            
            await EditField(returnItem, "Unload", returnItem.Unload, (line, qty) =>
            {
                if (line is RRSingleLine)
                    (line as RRSingleLine).Detail.Unload = qty;
                returnItem.Unload = qty;
            });
        }
        
        private async Task EditField(RouteReturnViewModel returnItem, string fieldName, float currentValue, Action<RRTemplateLine, float> updateAction)
        {
            var qtyText = await _dialogService.ShowPromptAsync(
                $"Set {fieldName} Quantity",
                returnItem.ProductName,
                "OK",
                "Cancel",
                currentValue.ToString(),
                -1,
                "",
                Keyboard.Numeric);
            
            if (string.IsNullOrWhiteSpace(qtyText) || !float.TryParse(qtyText, out var qty))
                return;
            
            // Update the corresponding line
            var line = _lines.FirstOrDefault(x => x.Product.ProductId == returnItem.ProductId);
            if (line != null)
            {
                updateAction(line, qty);
                SaveState();
                _changed = true;
            }
        }
        
        [RelayCommand]
        private async Task EditReturn(RouteReturnViewModel returnItem)
        {
            if (returnItem == null)
                return;
            
            var action = await _dialogService.ShowActionSheetAsync(
                $"Edit {returnItem.ProductName}",
                "Cancel",
                null,
                new[] { "Edit Unload", "Edit Damaged", "Edit Returns", "Edit Dumps" });
            
            if (string.IsNullOrEmpty(action) || action == "Cancel")
                return;
            
            float currentValue = 0;
            string promptTitle = "";
            
            switch (action)
            {
                case "Edit Unload":
                    currentValue = returnItem.Unload;
                    promptTitle = "Enter Unload Quantity";
                    break;
                case "Edit Damaged":
                    currentValue = returnItem.DamagedInTruck;
                    promptTitle = "Enter Damaged Quantity";
                    break;
                case "Edit Returns":
                    currentValue = returnItem.Returns;
                    promptTitle = "Enter Returns Quantity";
                    break;
                case "Edit Dumps":
                    currentValue = returnItem.Dumps;
                    promptTitle = "Enter Dumps Quantity";
                    break;
            }
            
            var qtyText = await _dialogService.ShowPromptAsync(promptTitle, returnItem.ProductName, "OK", "Cancel", currentValue.ToString(), -1, "");
            if (string.IsNullOrWhiteSpace(qtyText) || !float.TryParse(qtyText, out var qty))
                return;
            
            // Update the corresponding line
            var line = _lines.FirstOrDefault(x => x.Product.ProductId == returnItem.ProductId);
            if (line != null)
            {
                switch (action)
                {
                    case "Edit Unload":
                        if (line is RRSingleLine)
                            (line as RRSingleLine).Detail.Unload = qty;
                        returnItem.Unload = qty;
                        break;
                    case "Edit Damaged":
                        if (line is RRSingleLine)
                            (line as RRSingleLine).Detail.DamagedInTruck = qty;
                        returnItem.DamagedInTruck = qty;
                        break;
                    case "Edit Returns":
                        if (line is RRSingleLine)
                            (line as RRSingleLine).Detail.Returns = qty;
                        returnItem.Returns = qty;
                        break;
                    case "Edit Dumps":
                        if (line is RRSingleLine)
                            (line as RRSingleLine).Detail.Dumps = qty;
                        returnItem.Dumps = qty;
                        break;
                }
                
                SaveState();
                _changed = true;
            }
        }
        
        public async Task<bool> OnBackButtonPressed()
        {
            // Return true to prevent navigation, false to allow it
            if (!_saved)
            {
                if (Config.EmptyTruckAtEndOfDay)
                {
                    await _dialogService.ShowAlertAsync("You must validate returns before leaving.", "Alert", "OK");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("You must save route returns before leaving.", "Alert", "OK");
                }
                return true; // Prevent navigation
            }
            
            if (_changed)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Leave anyway?",
                    "Yes",
                    "No");
                
                if (!confirmed)
                    return true; // Prevent navigation
            }
            
            // Delete the file if it exists (Xamarin behavior)
            if (File.Exists(_fileName))
                File.Delete(_fileName);
            
            // [ACTIVITY STATE]: Remove state when properly exiting
            Helpers.NavigationHelper.RemoveNavigationState("routereturns");
            
            // Allow navigation
            await Shell.Current.GoToAsync("..");
            return false;
        }
        
        [RelayCommand]
        private async Task Done()
        {
            // Only delete the file if it hasn't been saved (temp/unsaved state)
            // If saved, the file should remain to indicate route returns are completed
            if (!_saved && File.Exists(_fileName))
                File.Delete(_fileName);
            
            // [ACTIVITY STATE]: Remove state when properly exiting
            Helpers.NavigationHelper.RemoveNavigationState("routereturns");
            
            // If emptyTruckOption, pass it back to EndOfDay page
            if (_emptyTruckOption)
            {
                // TODO: Pass emptyTruckOption back to EndOfDay page via query parameters
            }
            
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task Save()
        {
            // If EmptyTruckAtEndOfDay, this is actually Validate button
            if (Config.EmptyTruckAtEndOfDay)
            {
                await Validate();
                return;
            }
            
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Save Route Returns",
                "Are you sure you want to save the route returns? This will update inventory.",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            await SaveHandler();
        }
        
        private async Task Validate()
        {
            // Ask for password first
            if (!string.IsNullOrEmpty(Config.AddInventoryPassword))
            {
                var password = await _dialogService.ShowPromptAsync(
                    "Enter Password",
                    "Enter password to validate returns",
                    "OK",
                    "Cancel",
                    "Password",
                    -1,
                    "",
                    Keyboard.Default);
                
                if (string.IsNullOrEmpty(password))
                    return;
                
                if (string.Compare(password, Config.AddInventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                    return;
                }
            }
            
            await SaveHandler();
        }
        
        private async Task SaveHandler()
        {
            try
            {
                // Persist changes to products
                PersistChanges(_lines);
                
                // Save state
                SaveState();
                
                // Update saved state
                _saved = true;
                _changed = false;
                IsSaved = true;
                
                await _dialogService.ShowAlertAsync("Returns saved successfully.", "Success", "OK");
                
                // Update the list to show only items with values after save
                UpdateReturnsList();
                FilterReturns(SearchText);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving returns: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
        
        private void PersistChanges(List<RRTemplateLine> lines)
        {
            if (Config.ButlerCustomization)
            {
                var alreadyProcessed = new List<int>();
                foreach (var l in lines)
                {
                    if (alreadyProcessed.Any(x => x == l.Product.ProductId))
                        continue;
                    
                    var allLines = lines.Where(x => x.Product.ProductId == l.Product.ProductId);
                    if (allLines.Count() > 1)
                    {
                        float totalDumps = 0;
                        float totalReturns = 0;
                        float totalUnload = 0;
                        float totalDamagedInTruck = 0;
                        double weight = 0;
                        
                        foreach (var al in allLines)
                        {
                            var alSS = (al as RRSingleLine);
                            var conversion = alSS.Detail?.UoM != null ? alSS.Detail.UoM.Conversion : 1;
                            totalDumps += (alSS.Dumps * conversion);
                            totalReturns += (alSS.Returns * conversion);
                            totalUnload += (alSS.Unload * conversion);
                            totalDamagedInTruck += (alSS.DamagedInTruck * conversion);
                            weight = alSS.Weight;
                        }
                        
                        l.Product.SetOnCreditDump(totalDumps, "", l.Weight);
                        l.Product.SetOnCreditReturn(totalReturns, "", l.Weight);
                        l.Product.SetUnloadInventory(totalUnload, "", l.Weight);
                        l.Product.SetDamagedInTruckInventory(totalDamagedInTruck, "", l.Weight);
                        l.Product.UpdateInventory(totalUnload + totalDamagedInTruck, null, -1, weight);
                        
                        if (Config.EmptyTruckAtEndOfDay || _emptyTruckOption)
                            l.Product.SetCurrentInventory(0, "", l.Weight);
                    }
                    else
                    {
                        l.Product.SetOnCreditDump(l.Dumps, "", l.Weight);
                        l.Product.SetOnCreditReturn(l.Returns, "", l.Weight);
                        l.Product.SetUnloadInventory(l.Unload, "", l.Weight);
                        l.Product.SetDamagedInTruckInventory(l.DamagedInTruck, "", l.Weight);
                        l.Product.UpdateInventory(l.Unload + l.DamagedInTruck, null, -1, l.Weight);
                        
                        if (Config.EmptyTruckAtEndOfDay || _emptyTruckOption)
                            l.Product.SetCurrentInventory(0, "", l.Weight);
                    }
                    
                    alreadyProcessed.Add(l.Product.ProductId);
                }
            }
            else
            {
                foreach (var l in lines)
                {
                    if (l.Product.UseLot || l.Product.SoldByWeight)
                    {
                        var line = l as RRGroupedLine;
                        foreach (var item in line.Details)
                        {
                            l.Product.SetOnCreditDump(item.Dumps, item.Lot, item.Weight);
                            l.Product.SetOnCreditReturn(item.Returns, item.Lot, item.Weight);
                            l.Product.SetUnloadInventory(item.Unload, item.Lot, item.Weight);
                            l.Product.SetDamagedInTruckInventory(item.DamagedInTruck, item.Lot, item.Weight);
                            l.Product.UpdateInventory(item.Unload + item.DamagedInTruck, null, item.Lot, item.Expiration, -1, item.Weight);
                            
                            if (Config.EmptyTruckAtEndOfDay || _emptyTruckOption)
                                l.Product.SetCurrentInventory(0, item.Lot, item.Weight);
                        }
                    }
                    else
                    {
                        l.Product.SetOnCreditDump(l.Dumps, "", l.Weight);
                        l.Product.SetOnCreditReturn(l.Returns, "", l.Weight);
                        l.Product.SetUnloadInventory(l.Unload, "", l.Weight);
                        l.Product.SetDamagedInTruckInventory(l.DamagedInTruck, "", l.Weight);
                        l.Product.UpdateInventory(l.Unload + l.DamagedInTruck, null, -1, l.Weight);
                        
                        if (Config.EmptyTruckAtEndOfDay || _emptyTruckOption)
                            l.Product.SetCurrentInventory(0, "", l.Weight);
                    }
                }
            }
            
            DataAccess.SaveInventory();
        }
        
        [RelayCommand]
        private async Task EmptyTruck()
        {
            if (_saved || Config.EmptyTruckAtEndOfDay)
                return;
            
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Empty Truck",
                "This will set all current inventory as unload. Continue?",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            _emptyTruckOption = true;
            _changed = true;
            
            // Rebuild list with empty truck
            _lines.Clear();
            _lines = PrepareLineList(true);
            SaveState();
            
            UpdateReturnsList();
            ShowingAll = false;
            FilterReturns(SearchText);
        }
        
        private List<RRTemplateLine> PrepareLineList(bool emptyTruck)
        {
            // This is the same as PrepareList but with explicit emptyTruck parameter
            // Used when empty truck option is selected
            var lines = new List<RRTemplateLine>();
            
            foreach (var item in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
            {
                if (item.CategoryId == 0 && item.RequestedLoadInventory == 0)
                    continue;
                
                RRTemplateLine line;
                
                if (item.UseLot || item.SoldByWeight)
                {
                    line = new RRGroupedLine() { Product = item };
                    lines.Add(line);
                    
                    if (emptyTruck)
                    {
                        foreach (var itemLot in item.ProductInv.TruckInventories)
                        {
                            if (itemLot.CurrentQty <= 0)
                                continue;
                            
                            var qty = itemLot.CurrentQty < 0 ? 0 : itemLot.CurrentQty;
                            var detail = new RouteReturnLine() 
                            { 
                                Product = item, 
                                Lot = itemLot.Lot, 
                                Unload = qty, 
                                Weight = itemLot.Weight 
                            };
                            (line as RRGroupedLine).Details.Add(detail);
                        }
                    }
                }
                else
                {
                    line = new RRSingleLine() 
                    { 
                        Product = item, 
                        Detail = new RouteReturnLine() 
                        { 
                            Product = item, 
                            Lot = string.Empty, 
                            Weight = item.Weight 
                        } 
                    };
                    lines.Add(line);
                    
                    if (emptyTruck)
                    {
                        (line as RRSingleLine).AddUnload(item.CurrentInventory < 0 ? 0 : item.CurrentInventory);
                    }
                }
            }
            
            // Add reships and returns from orders (same logic as PrepareList)
            foreach (var o in Order.Orders)
            {
                if (!o.AsPresale && !o.Voided)
                {
                    foreach (var od in o.Details)
                    {
                        if (o.Reshipped)
                        {
                            var line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId);
                            
                            if (Config.UsePallets)
                            {
                                if (od.Product.SoldByWeight && od.Product.UseLot)
                                {
                                    line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Lot == od.Lot && x.Weight == od.Weight);
                                }
                                else if (od.Product.SoldByWeight && !od.Product.UseLot)
                                {
                                    line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Weight == od.Weight);
                                }
                            }
                            
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                lines.Add(line);
                            }
                            
                            float factor = 1;
                            if (od.UnitOfMeasure != null)
                                factor = od.UnitOfMeasure.Conversion;
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (line is RRSingleLine)
                                (line as RRSingleLine).AddReships(qty * factor);
                            else
                                (line as RRGroupedLine).AddReships(qty * factor, od.Lot, od.Weight);
                        }
                        else if (od.IsCredit)
                        {
                            var line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId);
                            
                            if (Config.UsePallets)
                            {
                                if (od.Product.SoldByWeight && od.Product.UseLot)
                                {
                                    line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Lot == od.Lot && x.Weight == od.Weight);
                                }
                                else if (od.Product.SoldByWeight && !od.Product.UseLot)
                                {
                                    line = lines.FirstOrDefault(x => x.Product.ProductId == od.Product.ProductId && x.Weight == od.Weight);
                                }
                            }
                            
                            if (line == null)
                            {
                                if (od.Product.SoldByWeight || od.Product.UseLot)
                                    line = new RRGroupedLine();
                                else
                                    line = new RRSingleLine() { Detail = new RouteReturnLine() { Product = od.Product } };
                                
                                line.Product = od.Product;
                                lines.Add(line);
                            }
                            
                            float factor = 1;
                            if (od.UnitOfMeasure != null)
                                factor = od.UnitOfMeasure.Conversion;
                            
                            var qty = od.Qty;
                            if (od.Product.SoldByWeight && od.Product.InventoryByWeight)
                                qty = od.Weight;
                            
                            if (od.Damaged)
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddDumps(qty * factor);
                                else
                                    (line as RRGroupedLine).AddDumps(qty * factor, od.Lot, od.Weight);
                            }
                            else
                            {
                                if (line is RRSingleLine)
                                    (line as RRSingleLine).AddReturns(qty * factor);
                                else
                                    (line as RRGroupedLine).AddReturns(qty * factor, od.Lot, od.Weight);
                            }
                        }
                    }
                }
            }
            
            return lines.OrderBy(x => x.Product.Name).ToList();
        }
        
        [RelayCommand]
        private async Task Print()
        {
            try
            {
                var toPrint = new List<RouteReturnLine>();
                
                foreach (var item in _lines.Where(x => x.Reships != 0 || x.Dumps != 0 || x.Returns != 0 || x.DamagedInTruck != 0 || x.Unload != 0))
                {
                    if (item is RRSingleLine)
                        toPrint.Add((item as RRSingleLine).Detail);
                    else
                        toPrint.AddRange((item as RRGroupedLine).Details);
                }
                
                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    
                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintRouteReturn(toPrint, false);
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

        [RelayCommand]
        private void DeleteReturn(RouteReturnViewModel returnItem)
        {
            if (returnItem != null)
            {
                Returns.Remove(returnItem);
                _allReturns.Remove(returnItem);
            }
        }
    }

    public partial class RouteReturnViewModel : ObservableObject
    {
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private float _quantity;
        [ObservableProperty] private float _reships;
        [ObservableProperty] private float _returns;
        [ObservableProperty] private float _dumps;
        [ObservableProperty] private float _damagedInTruck;
        [ObservableProperty] private float _unload;
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string _lot = string.Empty;
        [ObservableProperty] private double _weight;
    }
}

