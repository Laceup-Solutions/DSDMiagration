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

        [ObservableProperty] private ObservableCollection<RouteReturnViewModel> _returns = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _showingAll = false;

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
                await Task.Run(() =>
                {
                    PrepareList();
                    LoadState();
                });
                
                // Convert RRTemplateLine to RouteReturnViewModel for display
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
                
                ShowingAll = false;
                FilterReturns(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading returns: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
        
        private void PrepareList()
        {
            _lines.Clear();
            
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
                            
                            (item as RRGroupedLine).Details.Add(detail);
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
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Save Route Returns",
                "Are you sure you want to save the route returns? This will update inventory.",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            try
            {
                // Persist changes to products
                PersistChanges(_lines);
                
                // Save state
                SaveState();
                
                await _dialogService.ShowAlertAsync("Returns saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving returns: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
        
        private void PersistChanges(List<RRTemplateLine> lines)
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
                        
                        if (Config.EmptyTruckAtEndOfDay)
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
                    
                    if (Config.EmptyTruckAtEndOfDay)
                        l.Product.SetCurrentInventory(0, "", l.Weight);
                }
            }
            
            DataAccess.SaveInventory();
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

