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
    public partial class AcceptLoadPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private Order? _loadOrder;
        private int _orderId;
        private List<InventoryLineViewModel> _allInventoryLines = new();

        [ObservableProperty] private ObservableCollection<InventoryLineViewModel> _inventoryLines = new();
        [ObservableProperty] private string _totalLoadText = string.Empty;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _readOnly;

        public AcceptLoadPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(int orderId)
        {
            _orderId = orderId;
            _loadOrder = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);

            if (_loadOrder == null)
            {
                await _dialogService.ShowAlertAsync("Load order not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            LoadInventoryLines();
        }

        private void LoadInventoryLines()
        {
            if (_loadOrder == null)
                return;

            InventoryLines.Clear();

            var inventoryLines = _loadOrder.Details
                .Where(x => !x.IsCredit)
                .Select(x => new InventoryLine
                {
                    Product = x.Product,
                    Real = x.Qty,
                    Starting = x.Qty,
                    UoM = x.UnitOfMeasure,
                    UniqueId = x.OriginalId,
                    Weight = x.Weight,
                    Lot = x.Lot
                })
                .ToList();

            // Sort using SortDetails
            var sortedLines = SortDetails.SortedDetails(inventoryLines).ToList();

            var productList = sortedLines.Select(x => new InventoryLineViewModel(x, this)).ToList();
            _allInventoryLines = productList;

            foreach (var item in productList)
            {
                InventoryLines.Add(item);
            }

            UpdateTotalLoad();
        }

        public void UpdateTotalLoad()
        {
            var total = InventoryLines.Sum(x => x.Real);
            TotalLoadText = $"Total Load: {total}";
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_loadOrder == null)
                return;

            var confirmed = await _dialogService.ShowConfirmationAsync("Warning", "No more changes allowed. Continue?", "Yes", "No");
            if (!confirmed)
                return;

            try
            {
                foreach (var line in InventoryLines)
                {
                    line.InventoryLine.Product.UpdateInventory(line.InventoryLine.Real, line.InventoryLine.UoM, line.InventoryLine.Lot, line.InventoryLine.Expiration, 1, line.InventoryLine.Weight);
                }

                DataAccess.SaveInventory();
                await _dialogService.ShowAlertAsync("Inventory saved successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task ViewPrint()
        {
            if (_loadOrder == null)
                return;

            try
            {
                var source = InventoryLines
                    .Where(x => x.Real > 0 || x.Starting > 0)
                    .Select(x => x.InventoryLine);

                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    
                    for (int i = 0; i < copies; i++)
                    {
                        if (!_loadOrder.IsDelivery)
                            result = printer.PrintAcceptLoad(source, _loadOrder.PrintedOrderId, ReadOnly);
                        else
                            result = printer.PrintOrder(_loadOrder, true);

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
        private async Task Activate()
        {
            if (_loadOrder == null)
                return;

            var confirmed = await _dialogService.ShowConfirmationAsync("Warning", "Finalize load? No more changes allowed.", "Yes", "No");
            if (!confirmed)
                return;

            try
            {
                foreach (var line in InventoryLines)
                {
                    line.InventoryLine.Product.UpdateInventory(line.InventoryLine.Real, line.InventoryLine.UoM, line.InventoryLine.Lot, line.InventoryLine.Expiration, 1, line.InventoryLine.Weight);
                    line.InventoryLine.Product.AddRequestedInventory(line.InventoryLine.Starting, line.InventoryLine.UoM, line.InventoryLine.Lot, line.InventoryLine.Expiration, line.InventoryLine.Weight);
                    line.InventoryLine.Product.AddLoadedInventory(line.InventoryLine.Real, line.InventoryLine.UoM, line.InventoryLine.Lot, line.InventoryLine.Expiration, line.InventoryLine.Weight);

                    if (!string.IsNullOrEmpty(line.InventoryLine.UniqueId))
                    {
                        var det = _loadOrder.Details.FirstOrDefault(x => x.OriginalId == line.InventoryLine.UniqueId);
                        if (det != null)
                        {
                            det.Qty = line.InventoryLine.Real;
                            det.UnitOfMeasure = line.InventoryLine.UoM;
                        }
                    }
                }

                DataAccess.SaveInventory();
                DataAccess.AcceptLoadOrders(_loadOrder, GetValuesChangedPerOrder(_loadOrder));

                if (_loadOrder.IsDelivery)
                    _loadOrder.PendingLoad = false;
                else
                    _loadOrder.Finished = true;

                ReadOnly = true;
                _loadOrder.Save();

                await _dialogService.ShowAlertAsync("Load finalized successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error finalizing load: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            var searchText = await _dialogService.ShowPromptAsync("Search", "Enter product name (leave empty to show all)", "OK", "Cancel", "", -1, "");
            
            InventoryLines.Clear();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all items
                foreach (var item in _allInventoryLines)
                {
                    InventoryLines.Add(item);
                }
            }
            else
            {
                var upper = searchText.ToUpper();
                var filtered = _allInventoryLines.Where(x => x.ProductName.ToUpper().Contains(upper)).ToList();
                
                if (filtered.Count > 0)
                {
                    foreach (var item in filtered)
                    {
                        InventoryLines.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Product not found.", "Alert", "OK");
                    // Restore all items
                    foreach (var item in _allInventoryLines)
                    {
                        InventoryLines.Add(item);
                    }
                }
            }
            
            UpdateTotalLoad();
        }

        [RelayCommand]
        private async Task Done()
        {
            if (_loadOrder == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (!_loadOrder.Finished && _loadOrder.PendingLoad)
            {
                await Activate();
                return;
            }

            if (!_loadOrder.IsDelivery)
                _loadOrder.ForceDelete();
            else
            {
                DataAccess.AddDeliveryClient(_loadOrder.Client);
            }

            await Shell.Current.GoToAsync("..");
        }

        private string GetValuesChangedPerOrder(Order order)
        {
            string result = "";
            foreach (var det in order.Details)
                result += string.Format("{1}{0}{2}{0}{3}|", (char)20, det.OriginalId, det.Qty, det.UnitOfMeasure != null ? det.UnitOfMeasure.Id : 0);
            return result;
        }
    }

    public partial class InventoryLineViewModel : ObservableObject
    {
        private readonly AcceptLoadPageViewModel? _parent;

        [ObservableProperty] private Product _product = null!;
        [ObservableProperty] private float _real;
        [ObservableProperty] private float _starting;
        [ObservableProperty] private UnitOfMeasure? _uom;
        [ObservableProperty] private string _uniqueId = string.Empty;
        [ObservableProperty] private float _weight;
        [ObservableProperty] private string _lot = string.Empty;
        [ObservableProperty] private DateTime _expiration;

        public InventoryLine InventoryLine { get; set; } = null!;

        public InventoryLineViewModel()
        {
        }

        public InventoryLineViewModel(InventoryLine line, AcceptLoadPageViewModel? parent = null)
        {
            _parent = parent;
            Product = line.Product;
            Real = line.Real;
            Starting = line.Starting;
            Uom = line.UoM;
            UniqueId = line.UniqueId;
            Weight = line.Weight;
            Lot = line.Lot;
            Expiration = line.Expiration;
            InventoryLine = line;
        }

        public string ProductName => Product?.Name ?? "Unknown";
        public string QuantityText => $"Qty: {Real}";
        
        partial void OnRealChanged(float value)
        {
            if (InventoryLine != null)
            {
                InventoryLine.Real = value;
                _parent?.UpdateTotalLoad();
            }
        }
    }
}
