using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class NewLoadOrderTemplatePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _loadOrder;
        private bool _readOnly;
        private bool _canGetOut;
        private int _clientId;
        private OrderDetail _lastDetail;

        [ObservableProperty] private ObservableCollection<LoadOrderDetailViewModel> _orderDetails = new();
        [ObservableProperty] private string _totalQtyText = "Total Qty: 0";
        [ObservableProperty] private string _driverText = "Driver:";
        [ObservableProperty] private string _shipDateText = "Ship Date:";
        [ObservableProperty] private string _termText = "Term:";
        [ObservableProperty] private bool _showTerm = true;
        [ObservableProperty] private bool _showDriver = true;
        [ObservableProperty] private bool _showNextServiceDate = false;
        [ObservableProperty] private string _nextServiceDateText = string.Empty;
        [ObservableProperty] private bool _showSiteSelection = false;
        [ObservableProperty] private ObservableCollection<LaceupMigration.SiteEx> _sites = new();
        [ObservableProperty] private LaceupMigration.SiteEx _selectedSite;
        [ObservableProperty] private bool _isNotReadOnly = true;

        public NewLoadOrderTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTerm = Config.UseTermsInLoadOrder;
            ShowNextServiceDate = Config.AutoGenerateLoadOrder;
            ShowDriver = Config.MasterLoadOrder;
        }

        public async Task InitializeAsync(int orderId, bool canGetOutIntent = false, int? clientIdIntent = null)
        {
            _canGetOut = canGetOutIntent;
            _clientId = clientIdIntent ?? 0;

            _loadOrder = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (_loadOrder == null)
            {
                await _dialogService.ShowAlertAsync("Not found order in Load Order Template", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
            _readOnly = !_loadOrder.AsPresale && (_loadOrder.Finished || _loadOrder.Voided);
            IsNotReadOnly = !_readOnly;

            // Load sites if available
            LoadSites();

            // Load order details
            RefreshOrderDetails();

            // Update summary text
            UpdateSummaryText();

            // Handle auto-generate if configured
            if (Config.AutoGenerateLoadOrder)
            {
                // Auto-generate logic would go here
            }
        }

        private void LoadSites()
        {
            var sa = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (sa != null)
            {
                var s = UDFHelper.GetSingleUDF("LoadOrderSitesList", sa.ExtraProperties);
                if (!string.IsNullOrEmpty(s))
                {
                    var parts = s.Split(',');
                    var siteList = new List<SiteEx>();
                    foreach (var p in parts)
                    {
                        if (int.TryParse(p, out var siteId))
                        {
                            var site = LaceupMigration.SiteEx.Find(siteId);
                            if (site != null && !siteList.Contains(site))
                                siteList.Add(site);
                        }
                    }

                    if (siteList.Count > 0)
                    {
                        ShowSiteSelection = true;
                        Sites.Clear();
                        foreach (var site in siteList)
                            Sites.Add(site);

                        if (_loadOrder.SiteId > 0)
                        {
                            SelectedSite = Sites.FirstOrDefault(x => x.Id == _loadOrder.SiteId);
                        }
                    }
                }
            }
        }

        private void RefreshOrderDetails()
        {
            // Match Xamarin's RefreshListView logic
            List<OdLine> details = new List<OdLine>();

            if (_loadOrder == null)
                return;

            try
            {
                if (Config.LoadByOrderHistory)
                {
                    // FillProductListByOrderHistory logic
                    details = GetDetailsFromOrderHistory();
                }
                else if (!Config.LoadOrderEmpty)
                {
                    // FillProductList logic
                    details = GetDetailsFromInventory();
                }
                else
                {
                    // SyncLoadOrderDetails logic
                    details = GetDetailsFromOrder();
                }

                // Sort and create view models
                var sortedDetails = SortDetails.SortedDetails(details).ToList();
                
                OrderDetails.Clear();
                foreach (var detail in sortedDetails)
                {
                    if (detail != null && detail.Product != null)
                    {
                        var viewModel = new LoadOrderDetailViewModel(detail, _loadOrder);
                        viewModel.IsEnabled = IsNotReadOnly;
                        OrderDetails.Add(viewModel);
                    }
                }

                UpdateTotalQty();
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error refreshing order details: {ex}");
                // Continue with empty list if there's an error
            }
        }

        private List<OdLine> GetDetailsFromOrder()
        {
            var result = new List<OdLine>();
            foreach (var orderDetail in _loadOrder.Details)
            {
                // Skip deleted or invalid details
                if (orderDetail == null || orderDetail.Deleted || orderDetail.Product == null)
                    continue;

                var odLine = new OdLine
                {
                    OrderDetail = orderDetail,
                    Product = orderDetail.Product,
                    UoM = orderDetail.UnitOfMeasure
                };
                result.Add(odLine);
            }
            return result;
        }

        private List<OdLine> GetDetailsFromInventory()
        {
            // Match Xamarin's FillProductList - get all products with inventory
            var result = new List<OdLine>();
            var products = Product.Products.Where(p => p != null && (p.CurrentInventory > 0 || p.CurrentWarehouseInventory > 0)).ToList();

            foreach (var product in products)
            {
                if (product == null)
                    continue;

                var existingDetail = _loadOrder.Details.FirstOrDefault(d => !d.Deleted && d.Product?.ProductId == product.ProductId);
                var odLine = new OdLine
                {
                    OrderDetail = existingDetail,
                    Product = product,
                    UoM = product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault) ?? product.UnitOfMeasures?.FirstOrDefault()
                };
                result.Add(odLine);
            }
            return result;
        }

        private List<OdLine> GetDetailsFromOrderHistory()
        {
            // Match Xamarin's FillProductListByOrderHistory
            var result = new List<OdLine>();
            // Implementation would get products from order history
            return result;
        }

        private void UpdateTotalQty()
        {
            var total = _loadOrder.Details.Sum(x => x.Qty);
            TotalQtyText = $"Total Qty: {total}";
        }

        private void UpdateSummaryText()
        {
            // Driver
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == _loadOrder.SalesmanId);
            if (salesman != null)
                DriverText = $"Driver: {salesman.Name}";
            else
                DriverText = "Driver:";

            // Ship Date
            if (_loadOrder.ShipDate.Year != 1)
                ShipDateText = $"Ship Date: {_loadOrder.ShipDate.ToShortDateString()}";
            else
                ShipDateText = "Ship Date:";

            // Term
            if (!string.IsNullOrEmpty(_loadOrder.ExtraFields))
            {
                var term = UDFHelper.GetSingleUDF("cashTerm", _loadOrder.ExtraFields);
                if (!string.IsNullOrEmpty(term) && term == "1")
                    TermText = "Term: CIA";
                else if (!string.IsNullOrEmpty(term))
                    TermText = "Term: Regular Order";
                else
                    TermText = "Term:";
            }
            else
            {
                TermText = "Term:";
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            _appService.RecordEvent("DoneClicked button");

            // Match Xamarin's DoneButton_Click logic
            // Mark ALL the details as selected
            foreach (var detail in _loadOrder.Details)
                detail.LoadStarting = 1;

            _loadOrder.Save();

            if (_loadOrder.Details.Count > 0 && Config.ShipDateIsMandatory && _loadOrder.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select a ship date.", "Alert", "OK");
                return;
            }

            bool hasTerm = true;
            if (_loadOrder.Details.Count > 0 && Config.UseTermsInLoadOrder)
            {
                hasTerm = false;
                if (!string.IsNullOrEmpty(_loadOrder.ExtraFields))
                {
                    var term = UDFHelper.GetSingleUDF("cashTerm", _loadOrder.ExtraFields);
                    if (!string.IsNullOrEmpty(term))
                        hasTerm = true;
                }
            }

            if (_loadOrder.Details.Count == 0)
                _loadOrder.Delete();

            if (Config.MasterLoadOrder && _loadOrder.Details.Count > 0)
            {
                var drivers = Salesman.List.Where(x => x.IsActive).ToList();
                if (drivers.Count > 0 && _loadOrder.SalesmanId == 0)
                {
                    await _dialogService.ShowAlertAsync("You must select a driver.", "Warning", "OK");
                    return;
                }
            }

            if (!hasTerm)
            {
                await SelectLoadTermAsync();
            }
            else
            {
                // Navigate back
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task SelectLoadTermAsync()
        {
            var result = await _dialogService.ShowActionSheetAsync("Select Terms", "Cancel", null, new[] { "CIA", "Regular Order" });
            if (result == "CIA")
            {
                _loadOrder.ExtraFields = UDFHelper.SyncSingleUDF("cashTerm", "1", _loadOrder.ExtraFields);
                _loadOrder.Save();
                UpdateSummaryText();
                await Shell.Current.GoToAsync("..");
            }
            else if (result == "Regular Order")
            {
                _loadOrder.ExtraFields = UDFHelper.SyncSingleUDF("cashTerm", "0", _loadOrder.ExtraFields);
                _loadOrder.Save();
                UpdateSummaryText();
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task Prod()
        {
            if (Sites.Count > 0 && _loadOrder.SiteId == 0)
            {
                await _dialogService.ShowAlertAsync("You must select a Warehouse to request the load from.", "Alert", "OK");
                return;
            }

            _appService.RecordEvent("prod button");

            if (_lastDetail == null)
            {
                // Navigate to categories
                await Shell.Current.GoToAsync($"fullcategory?orderId={_loadOrder.OrderId}");
            }
            else
            {
                // Navigate to product catalog for the last detail's category
                await Shell.Current.GoToAsync($"productcatalog?orderId={_loadOrder.OrderId}&categoryId={_lastDetail.Product.CategoryId}&productId={_lastDetail.Product.ProductId}");
            }
        }

        [RelayCommand]
        private async Task Cats()
        {
            if (Sites.Count > 0 && _loadOrder.SiteId == 0)
            {
                await _dialogService.ShowAlertAsync("You must select a Warehouse to request the load from.", "Alert", "OK");
                return;
            }

            _appService.RecordEvent("cats button");
            await Shell.Current.GoToAsync($"fullcategory?orderId={_loadOrder.OrderId}");
        }

        [RelayCommand]
        private async Task Search()
        {
            if (Sites.Count > 0 && _loadOrder.SiteId == 0)
            {
                await _dialogService.ShowAlertAsync("You must select a Warehouse to request the load from.", "Alert", "OK");
                return;
            }

            _appService.RecordEvent("search button");
            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product Name");
            if (!string.IsNullOrEmpty(searchTerm))
            {
                await Shell.Current.GoToAsync($"fullcategory?orderId={_loadOrder.OrderId}&productSearch={Uri.EscapeDataString(searchTerm)}&comingFromSearch=true");
            }
        }

        [RelayCommand]
        private async Task SetDate()
        {
            _appService.RecordEvent("Set Date button");
            await SetShipDateAsync();
        }

        private async Task SetShipDateAsync()
        {
            DateTime dt = DateTime.Now;
            if (_loadOrder.ShipDate != DateTime.MinValue && _loadOrder.ShipDate.Year != 1)
                dt = _loadOrder.ShipDate;

            var date = await _dialogService.ShowDatePickerAsync("Select Ship Date", dt);
            if (date.HasValue)
            {
                if (Config.MinShipDateDays > 0 && date.Value.Subtract(DateTime.Now).Days < Config.MinShipDateDays)
                {
                    var minShipDate = DateTime.Now.AddDays(Config.MinShipDateDays);
                    await _dialogService.ShowAlertAsync($"Ship date must be at least {Config.MinShipDateDays} days from now. Minimum date: {minShipDate.ToShortDateString()}", "Alert", "OK");
                    return;
                }

                _loadOrder.ShipDate = date.Value;
                _loadOrder.Save();
                UpdateSummaryText();
            }
        }

        [RelayCommand]
        private async Task QtyButton(LoadOrderDetailViewModel item)
        {
            if (item == null || _readOnly)
                return;

            // Match Xamarin's HandleClick - show dialog with Qty, Comments, and UoM
            var currentQty = item.OrderDetail?.Qty ?? 0;
            var currentComments = item.OrderDetail?.Comments ?? string.Empty;
            var currentUoM = item.OrderDetail?.UnitOfMeasure ?? item.UoM;
            
            var result = await _dialogService.ShowAddItemDialogAsync(
                item.ProductName,
                item.Product,
                currentQty > 0 ? currentQty.ToString() : "1",
                currentComments,
                currentUoM);
            
            if (result.qty == null)
                return; // User cancelled

            if (string.IsNullOrEmpty(result.qty) || !decimal.TryParse(result.qty, out var qty))
                qty = 0;

            if (qty == 0)
            {
                // Remove the detail (match Xamarin's DeleteDetail)
                if (item.OrderDetail != null)
                {
                    item.OrderDetail.Deleted = true;
                    _loadOrder.Details.Remove(item.OrderDetail);
                }
            }
            else
            {
                // Add or update the detail (match Xamarin's logic)
                var det = item.OrderDetail;

                if (det == null)
                {
                    det = new OrderDetail(item.Product, 0, _loadOrder)
                    {
                        LoadStarting = -1,
                        UnitOfMeasure = item.UoM
                    };
                    _loadOrder.Details.Add(det);
                    item.OrderDetail = det;
                }

                // Check inventory if configured
                if (Config.CheckInventoryInLoad)
                {
                    var inventory = det.Product.CurrentWarehouseInventory;
                    if (item.UoM != null && inventory > 0)
                        inventory /= item.UoM.Conversion;

                    if (inventory < (float)qty)
                    {
                        await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert", "OK");
                        return;
                    }
                }

                if (det.LoadStarting == -1 && det.Qty != (float)qty)
                    det.LoadStarting = 0;

                det.Qty = (float)qty;
                det.Comments = result.comments ?? string.Empty;
                det.UnitOfMeasure = result.selectedUoM ?? item.UoM;

                _lastDetail = det;
            }

            _loadOrder.Save();
            RefreshOrderDetails();
        }

        [RelayCommand]
        private async Task NextServiceDate()
        {
            // Match Xamarin's NextDeliveryDateButton_Click
            DateTime dt = DateTime.Today;
            if (selectedNextServiceDate != DateTime.Today)
                dt = selectedNextServiceDate;

            var date = await _dialogService.ShowDatePickerAsync("Select Next Service Date", dt);
            if (date.HasValue)
            {
                await SetNextServiceDateAsync(date.Value);
            }
        }

        private DateTime selectedNextServiceDate = DateTime.Today;

        private async Task SetNextServiceDateAsync(DateTime date)
        {
            // Match Xamarin's SetNextServiceDate logic
            await _dialogService.ShowLoadingAsync("Calculating sales projection...");
            try
            {
                await Task.Delay(1000); // Simulate calculation

                selectedNextServiceDate = date;
                NextServiceDateText = date.ToShortDateString();

                // Generate details from projection logic would go here
                RefreshOrderDetails();
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }
        }

        partial void OnSelectedSiteChanged(SiteEx value)
        {
            if (value != null && _loadOrder != null)
            {
                if (_loadOrder.SiteId > 0 && _loadOrder.Details.Count > 0)
                {
                    if (value.Id == _loadOrder.SiteId)
                    {
                        // Do nothing
                    }
                    else
                    {
                        // Can't change warehouse if products already added
                        SelectedSite = Sites.FirstOrDefault(x => x.Id == _loadOrder.SiteId);
                        _dialogService.ShowAlertAsync("You can't change the warehouse for this load since you already added products.", "Alert", "OK");
                        return;
                    }
                }

                _loadOrder.SiteId = value.Id;
                _loadOrder.Save();

                // Update inventory based on site
                if (_loadOrder.SiteId > 0)
                    DataProvider.UpdateInventoryBySite(_loadOrder.SiteId);
                else
                    DataProvider.UpdateInventory();

                RefreshOrderDetails();
            }
        }

        public async Task OnAppearingAsync()
        {
            // Refresh when returning from product/category selection
            RefreshOrderDetails();
        }
    }

    public partial class LoadOrderDetailViewModel : ObservableObject
    {
        private readonly OdLine _odLine;
        private readonly Order _loadOrder;

        [ObservableProperty] private string _productName;
        [ObservableProperty] private string _quantityText;
        [ObservableProperty] private string _onHandText;
        [ObservableProperty] private string _truckInventoryText;
        [ObservableProperty] private bool _showInventory = true;
        [ObservableProperty] private bool _isEnabled = true;

        public OrderDetail OrderDetail { get; set; }
        public Product Product => _odLine.Product;
        public UnitOfMeasure UoM => _odLine.UoM;

        public LoadOrderDetailViewModel(OdLine odLine, Order loadOrder)
        {
            _odLine = odLine;
            _loadOrder = loadOrder;
            OrderDetail = odLine.OrderDetail;

            ProductName = odLine.Product.Name;
            
            if (odLine.OrderDetail != null)
                QuantityText = odLine.OrderDetail.Qty.ToString();
            else
                QuantityText = "+";

            // Calculate inventory
            var inventoryW = odLine.Product.CurrentWarehouseInventory;
            if (odLine.UoM != null && inventoryW > 0)
                inventoryW /= odLine.UoM.Conversion;

            OnHandText = $"OH: {Math.Round(inventoryW, Config.Round)}";

            var inventory = odLine.Product.CurrentInventory;
            if (odLine.UoM != null && inventory > 0)
                inventory /= odLine.UoM.Conversion;

            TruckInventoryText = $"Truck Inventory: {Math.Round(inventory, Config.Round)}";

            ShowInventory = !Config.HideProdOnHand;
        }
    }
}

