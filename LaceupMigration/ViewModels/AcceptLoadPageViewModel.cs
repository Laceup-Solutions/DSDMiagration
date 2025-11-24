using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;

namespace LaceupMigration.ViewModels
{
    public partial class AcceptLoadPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private DateTime _currentDate = DateTime.Now;
        private List<Order> _sourceOrders = new();
        private bool[] _selectedOrders = Array.Empty<bool>();
        private bool _ordersAlreadyLoaded = false;

        [ObservableProperty] private ObservableCollection<AcceptLoadOrderItemViewModel> _orders = new();
        [ObservableProperty] private string _selectedDateText = string.Empty;
        [ObservableProperty] private bool _viewAll = false;
        [ObservableProperty] private bool _showViewAllToggle = false;
        [ObservableProperty] private bool _selectAll = true;
        [ObservableProperty] private bool _isLoading = false;

        // Totals
        [ObservableProperty] private string _totalLoads = "0";
        [ObservableProperty] private string _totalStops = "0";
        [ObservableProperty] private string _totalDeliveries = "0";
        [ObservableProperty] private string _totalItems = "0";
        [ObservableProperty] private string _totalQty = "0";
        [ObservableProperty] private string _totalWeight = "0";
        [ObservableProperty] private string _totalAmount = "$0.00";
        [ObservableProperty] private bool _showTotalQty = true;
        [ObservableProperty] private bool _showTotalWeight = true;

        public AcceptLoadPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _currentDate = DateTime.Now;
            SelectedDateText = _currentDate.ToShortDateString();
            ShowViewAllToggle = Config.ShowAllAvailableLoads;
            ViewAll = Config.ShowAllAvailableLoads;
        }

        public async Task InitializeWithDateAsync(DateTime date)
        {
            // Called when navigating from InventoryMainPage with a selected date
            // Match Xamarin: activity.PutExtra("loadDate", date.Ticks.ToString());
            _currentDate = date;
            SelectedDateText = _currentDate.ToShortDateString();
            // Orders are already loaded by RefreshAndNavigateToAcceptLoadAsync in InventoryMainPageViewModel
            // Just refresh the UI with the existing orders
            _ordersAlreadyLoaded = true;
            RefreshOrders();
        }

        public async Task OnAppearingAsync()
        {
            // On appearing, if we don't have orders yet, show date picker first
            // Match Xamarin behavior: if SyncLoadOnDemand, show date picker on appearing
            if (Orders.Count == 0 && !_ordersAlreadyLoaded)
            {
                // Show date picker first, then refresh
                await SelectDateAsync();
            }
            else if (!_ordersAlreadyLoaded)
            {
                await RefreshAsync(false);
            }
            _ordersAlreadyLoaded = false; // Reset for next time
        }

        private void RefreshOrders()
        {
            _sourceOrders = GetSortedSource();
            _selectedOrders = new bool[_sourceOrders.Count];

            // Select all by default
            for (int i = 0; i < _selectedOrders.Length; i++)
                _selectedOrders[i] = true;

            Orders.Clear();
            foreach (var order in _sourceOrders)
            {
                var index = _sourceOrders.IndexOf(order);
                Orders.Add(new AcceptLoadOrderItemViewModel(order, this, index));
            }

            SelectAll = _selectedOrders.All(x => x);
            RefreshTotals();
        }

        private List<Order> GetSortedSource()
        {
            var list = Order.Orders.Where(x => (x.OrderType == OrderType.Load || x.IsDelivery) && x.PendingLoad).ToList();

            if (Config.DontDeleteEmptyDeliveries)
            {
                var ordersToRemove = new List<Order>();
                var ordersToCheck = list.Where(x => x.Details.Sum(x => x.Qty) == 0).ToList();

                foreach (var o in ordersToCheck)
                {
                    if (o.OrderType == OrderType.Order)
                    {
                        ordersToRemove.Add(o);
                        continue;
                    }

                    if (o.OrderType == OrderType.Credit || o.OrderType == OrderType.Return)
                    {
                        var anyOrder = list.Where(x => x.Client.ClientId == o.Client.ClientId && x.OrderType == OrderType.Order).ToList();
                        if (anyOrder.Count == 0)
                        {
                            ordersToRemove.Add(o);
                        }
                    }
                }

                foreach (var i in ordersToRemove)
                {
                    list.Remove(i);
                }
            }

            var loads = list.Where(x => x.OrderType == OrderType.Load && x.OrderType != OrderType.WorkOrder);
            var deliveries = list.Where(x => x.OrderType != OrderType.Load && x.OrderType != OrderType.WorkOrder);
            var workOrders = list.Where(x => x.OrderType == OrderType.WorkOrder);

            var result = new List<Order>();
            result.AddRange(loads.OrderBy(x => x.PrintedOrderId ?? ""));
            result.AddRange(deliveries.OrderBy(x => x.Client.ClientName));
            result.AddRange(workOrders.OrderBy(x => x.PrintedOrderId ?? ""));

            return result;
        }

        [ObservableProperty]
        private bool showDatePicker;

        [RelayCommand]
        private async Task SelectDateAsync()
        {
            // Match Xamarin DateButton_Click logic
            if (ViewAll)
            {
                ViewAll = false;
                return;
            }

            // Show the date picker on the page - it will auto-focus and open native calendar
            ShowDatePicker = true;
        }

        public async void OnDateSelected(DateTime date)
        {
            ShowDatePicker = false;
            _currentDate = date;
            SelectedDateText = _currentDate.ToShortDateString();
            await RefreshAsync(false);
        }

        partial void OnViewAllChanged(bool value)
        {
            // Match Xamarin ViewAll_CheckedChange logic
            if (value)
            {
                // Show all - set date text to --/--/---- and refresh
                SelectedDateText = "--/--/----";
                // Fire and forget - matching Xamarin's ThreadPool.QueueUserWorkItem behavior
                _ = RefreshAsync(false);
            }
            else
            {
                // Only for this vendor - show date picker
                _ = SelectDateAsync();
            }
        }

        private async Task RefreshAsync(bool exit = false)
        {
            // Match Xamarin Refresh method logic - show progress dialog
            await _dialogService.ShowLoadingAsync("Downloading load orders...");
            string responseMessage = null;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Download products first
                        DataAccess.DownloadProducts();

                        // Get pending load orders based on ViewAll flag
                        if (ViewAll)
                            DataAccess.GetPendingLoadOrders(_currentDate, true);
                        else
                            DataAccess.GetPendingLoadOrders(_currentDate);
                    }
                    catch (Exception e)
                    {
                        Logger.CreateLog(e);
                        responseMessage = "Error downloading load orders.";
                    }
                });

                await _dialogService.HideLoadingAsync();

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
                else
                {
                    // Update date button text (unless ViewAll is checked)
                    if (!ViewAll)
                        SelectedDateText = _currentDate.ToShortDateString();

                    // Refresh the orders list
                    RefreshOrders();

                    // Update RouteOrdersCount and save app status (matching Xamarin)
                    DataAccess.RouteOrdersCount = _sourceOrders.Count;
                    Config.SaveAppStatus();

                    // If RouteOrdersCount is 0 and exit is true, go back to main (matching Xamarin)
                    if (DataAccess.RouteOrdersCount == 0 && exit)
                    {
                        await _appService.GoBackToMainAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Error refreshing load orders.", "Alert", "OK");
                _appService.TrackError(ex);
            }
        }

        public void UpdateOrderSelection(int index, bool isSelected)
        {
            if (index >= 0 && index < _selectedOrders.Length)
            {
                _selectedOrders[index] = isSelected;
                SelectAll = _selectedOrders.All(x => x);
                RefreshTotals();
            }
        }

        public void UpdateSelectAll(bool isSelected)
        {
            for (int i = 0; i < _selectedOrders.Length; i++)
                _selectedOrders[i] = isSelected;

            foreach (var order in Orders)
            {
                order.IsSelected = isSelected;
            }

            RefreshTotals();
        }

        private void RefreshTotals()
        {
            double totalAmount = 0;
            float totalItems = 0;
            float totalQty = 0;
            double totalWeight = 0;

            int loads = 0;
            int deliveries = 0;
            var stops = new List<int>();

            for (int i = 0; i < _selectedOrders.Length; i++)
            {
                if (!_selectedOrders[i])
                    continue;

                var o = _sourceOrders[i];

                float sectionItems = 0;
                float sectionQty = 0;
                double sectionWeight = 0;
                double sectionAmount = o.OrderTotalCost();

                foreach (var item in o.Details)
                {
                    if (item.Product.SoldByWeight && item.Product.InventoryByWeight)
                        sectionWeight += item.Weight;
                    else
                    {
                        var x = item.Qty;
                        if (item.UnitOfMeasure != null)
                            x *= item.UnitOfMeasure.Conversion;
                        sectionQty += x;
                    }

                    sectionItems += item.Qty;
                }

                totalAmount += sectionAmount;

                if (o.OrderType != OrderType.Load)
                {
                    if (!stops.Contains(o.Client.ClientId))
                        stops.Add(o.Client.ClientId);
                    deliveries++;
                }
                else
                    loads++;

                totalQty += sectionQty;
                totalWeight += sectionWeight;
                totalItems += sectionItems;
            }

            TotalLoads = loads.ToString();
            TotalDeliveries = deliveries.ToString();
            TotalStops = stops.Count.ToString();

            TotalWeight = Math.Round(totalWeight, Config.Round).ToString();
            TotalQty = Math.Round(totalQty, Config.Round).ToString();
            TotalItems = Math.Round(totalItems, Config.Round).ToString();
            TotalAmount = totalAmount.ToCustomString();

            ShowTotalWeight = totalWeight > 0;
            ShowTotalQty = totalQty > 0;
        }

        [RelayCommand]
        private async Task AcceptAsync()
        {
            var selectedOrders = GetSelectedOrders().ToList();

            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("There are no orders selected. Please select one or more orders to continue.", "Alert", "OK");
                return;
            }

            var orderIds = string.Join("|", selectedOrders.Select(x => x.OrderId.ToString()));
            await Shell.Current.GoToAsync($"acceptinventoryresume?orderIds={orderIds}");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            DataAccess.DeletePengingLoads();
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            var options = new List<string>();

            if (Config.AcceptLoadEditable)
            {
                options.Add("Edit");
            }

            options.Add("Print");
            options.Add("Advanced Options");

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.ToArray());

            switch (choice)
            {
                case "Edit":
                    await EditDeliveryQtyAsync();
                    break;
                case "Print":
                    await PrintAsync();
                    break;
                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task EditDeliveryQtyAsync()
        {
            var selectedOrders = GetSelectedOrders().ToList();

            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("There are no orders selected. Please select one or more orders to continue.", "Alert", "OK");
                return;
            }

            var orderIds = string.Join("|", selectedOrders.Select(x => x.OrderId.ToString()));
            await Shell.Current.GoToAsync($"acceptloadeditdelivery?orderIds={orderIds}");
        }

        private async Task PrintAsync()
        {
            var selectedOrders = GetSelectedOrders().ToList();

            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("There are no orders selected. Please select one or more orders to continue.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    if (copies < 1)
                        return "Please enter a valid number of copies.";

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;

                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintAcceptedOrders(selectedOrders, false);

                        if (Config.OldPrinter > 0)
                            System.Threading.Thread.Sleep(2000);

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

        private IEnumerable<Order> GetSelectedOrders()
        {
            for (int i = 0; i < _selectedOrders.Length; i++)
                if (_selectedOrders[i])
                    yield return _sourceOrders[i];
        }
    }

    public partial class AcceptLoadOrderItemViewModel : ObservableObject
    {
        private readonly Order _order;
        private readonly AcceptLoadPageViewModel _parent;
        private readonly int _index;

        [ObservableProperty] private bool _isSelected = true;

        public AcceptLoadOrderItemViewModel(Order order, AcceptLoadPageViewModel parent, int index)
        {
            _order = order;
            _parent = parent;
            _index = index;
        }

        public Order Order => _order;

        public string DocTypeLabel
        {
            get
            {
                return _order.OrderType switch
                {
                    OrderType.Load => "Doc Type: Load Order",
                    OrderType.Credit => "Doc Type: Credit",
                    OrderType.Return => "Doc Type: Return",
                    OrderType.WorkOrder => "Doc Type: Work Order",
                    _ => "Doc Type: Order"
                };
            }
        }

        public string OrderNumberLabel => $"Number: {_order.PrintedOrderId}";
        public Color OrderNumberColor => _order.Details.Any(x => x.Qty != x.Ordered) 
            ? Color.FromArgb("#FF0000") 
            : Color.FromArgb("#007CBA");

        public string ClientName => _order.Client?.ClientName ?? "Unknown";
        public bool ShowClientName => _order.OrderType != OrderType.Load;

        public string AssetText
        {
            get
            {
                if (_order.OrderType == OrderType.WorkOrder)
                {
                    var asset = Asset.FindById(_order.AssetId);
                    if (asset != null)
                        return $"Asset Assigned: {asset.Product.Description} Part Number: {asset.SerialNumber}";
                }
                return string.Empty;
            }
        }
        public bool ShowAsset => _order.OrderType == OrderType.WorkOrder && !string.IsNullOrEmpty(AssetText);

        public string TotalItemsLabel
        {
            get
            {
                var items = _order.Details.Sum(x => x.Qty);
                return $"Total Items: {Math.Round(items, 2)}";
            }
        }

        public string TotalWeightLabel
        {
            get
            {
                var weight = _order.Details
                    .Where(x => x.Product.SoldByWeight && x.Product.InventoryByWeight)
                    .Sum(x => x.Weight);
                return $"Total Weight: {Math.Round(weight, 2)}";
            }
        }

        public string TotalQtyLabel
        {
            get
            {
                var qty = 0f;
                foreach (var item in _order.Details)
                {
                    if (!(item.Product.SoldByWeight && item.Product.InventoryByWeight))
                    {
                        var x = item.Qty;
                        if (item.UnitOfMeasure != null)
                            x *= item.UnitOfMeasure.Conversion;
                        qty += x;
                    }
                }
                return $"Total Qty: {Math.Round(qty, 2)}";
            }
        }
        public bool ShowTotalQty => !_order.Details.All(x => x.Product.SoldByWeight && x.Product.InventoryByWeight);

        public string AmountLabel => $"Amount: {_order.OrderTotalCost().ToCustomString()}";

        public string CommentText => $"Comments: {_order.Comments}";
        public bool ShowComment => (_order.OrderType == OrderType.WorkOrder || _order.IsWorkOrder) && !string.IsNullOrEmpty(_order.Comments);

        partial void OnIsSelectedChanged(bool value)
        {
            _parent.UpdateOrderSelection(_index, value);
        }
    }
}
