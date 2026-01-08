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
using Microsoft.Maui.ApplicationModel.Communication;

namespace LaceupMigration.ViewModels
{
    public partial class NewLoadOrderTemplatePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
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

        public NewLoadOrderTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
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

        private async Task SelectLoadTermForSendAsync()
        {
            // Select term for sending (matches Xamarin SelectLoadTerm(SendIt))
            var result = await _dialogService.ShowActionSheetAsync("Select Terms", "Cancel", null, new[] { "CIA", "Regular Order" });
            if (result == "CIA")
            {
                _loadOrder.ExtraFields = DataAccess.SyncSingleUDF("cashTerm", "1", _loadOrder.ExtraFields);
                _loadOrder.Save();
                UpdateSummaryText();
                // After selecting term, continue with sending (matches Xamarin SelectLoadTerm(SendIt))
                await SendItAsync();
            }
            else if (result == "Regular Order")
            {
                _loadOrder.ExtraFields = DataAccess.SyncSingleUDF("cashTerm", "0", _loadOrder.ExtraFields);
                _loadOrder.Save();
                UpdateSummaryText();
                // After selecting term, continue with sending (matches Xamarin SelectLoadTerm(SendIt))
                await SendItAsync();
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

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            // Match Xamarin newLoadOrderMenu.xml order exactly:
            // 1. Generate Values (if Config.GenerateValuesLoadOrder)
            // 2. Clear Values (if Config.GenerateValuesLoadOrder)
            // 3. What To View (if Config.LoadByOrderHistory)
            // 4. Print
            // 5. Select Term (if Config.UseTermsInLoadOrder)
            // 6. Select Salesman (if Config.MasterLoadOrder && drivers.Count > 0)
            // 7. Add Comment
            // 8. Send Order (if !readOnly || Config.SendLoadOrder)
            // 9. Send By Email (if !readOnly || Config.SendLoadOrder)
            // 10. Advanced Options
            var options = new List<string>();

            bool canSendLoad = !_readOnly || Config.SendLoadOrder;
            var drivers = Salesman.List.Where(x => x.IsActive).ToList();

            if (Config.GenerateValuesLoadOrder)
            {
                options.Add("Generate Values");
                options.Add("Clear Values");
            }

            if (Config.LoadByOrderHistory)
            {
                options.Add("What To View");
            }

            options.Add("Print");

            if (Config.UseTermsInLoadOrder)
            {
                options.Add("Select Term");
            }

            if (Config.MasterLoadOrder && drivers.Count > 0)
            {
                options.Add("Select Salesman");
            }

            options.Add("Add Comment");

            if (canSendLoad)
            {
                options.Add("Send Order");
                options.Add("Send By Email");
            }

            options.Add("Advanced Options");

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.ToArray());

            switch (choice)
            {
                case "Generate Values":
                    await GenerateValuesAsync();
                    break;
                case "Clear Values":
                    await ClearValuesAsync();
                    break;
                case "What To View":
                    await WhatToViewAsync();
                    break;
                case "Print":
                    await PrintAsync();
                    break;
                case "Select Term":
                    await SelectLoadTermAsync();
                    break;
                case "Select Salesman":
                    await SelectSalesmanAsync();
                    break;
                case "Add Comment":
                    await AddCommentAsync();
                    break;
                case "Send Order":
                    await SendOrderAsync();
                    break;
                case "Send By Email":
                    await SendByEmailAsync();
                    break;
                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task GenerateValuesAsync()
        {
            // TODO: Implement Generate Values dialog
            await _dialogService.ShowAlertAsync("Generate Values functionality is not yet fully implemented.", "Info");
        }

        private async Task ClearValuesAsync()
        {
            var result = await _dialogService.ShowConfirmAsync(
                "Are you sure you want to delete all lines?",
                "Warning",
                "Yes",
                "No");
            
            if (result)
            {
                _loadOrder.Details.Clear();
                _loadOrder.Save();
                RefreshOrderDetails();
            }
        }

        private async Task WhatToViewAsync()
        {
            // TODO: Implement What To View filter
            await _dialogService.ShowAlertAsync("What To View functionality is not yet fully implemented.", "Info");
        }

        private async Task PrintAsync()
        {
            // Match Xamarin PrinterHandler() method
            if (_loadOrder == null)
            {
                await _dialogService.ShowAlertAsync("No load order to print.", "Alert", "OK");
                return;
            }

            // Check ship date if mandatory
            if (Config.ShipDateIsMandatory && _loadOrder.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select a ship date.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    // Match Xamarin Printit() method
                    // Clear and populate LoadOrder static class
                    LoadOrder.List.Clear();
                    LoadOrder.Date = _loadOrder.ShipDate;

                    // Add all details with Qty > 0 to LoadOrder.List
                    foreach (var item in _loadOrder.Details)
                    {
                        if (item.Qty > 0)
                        {
                            LoadOrder.List.Add(new LoadOrderDetail
                            {
                                Product = item.Product,
                                Qty = item.Qty,
                                UoM = item.UnitOfMeasure,
                                Lot = item.Lot,
                                Comments = item.Comments
                            });
                        }
                    }

                    // Set company (matches Xamarin)
                    if (CompanyInfo.Companies.Count > 0)
                        CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                    // Set site ID
                    LoadOrder.SiteId = _loadOrder.SiteId;

                    // Generate printed order ID if needed
                    if (string.IsNullOrEmpty(_loadOrder.PrintedOrderId) && !Config.DontGenerateLoadPrintedId)
                    {
                        if ((_loadOrder.AsPresale && Config.GeneratePresaleNumber) || (!_loadOrder.AsPresale && Config.GeneratePreorderNum))
                        {
                            _loadOrder.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_loadOrder);
                            _loadOrder.Save();
                        }
                    }

                    // Set printed order ID
                    LoadOrder.PrintedOrderId = _loadOrder.PrintedOrderId;

                    // Print using printer
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result;
                    for (int i = 0; i < number; i++)
                    {
                        result = printer.PrintOrderLoad(_readOnly);
                        if (!result)
                            return "Error printing";
                    }

                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                Logger.CreateLog(ex);
                _appService.TrackError(ex);
            }
        }

        private async Task SelectSalesmanAsync()
        {
            var drivers = Salesman.List.Where(x => x.IsActive).ToList();
            if (drivers.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No drivers available.", "Alert", "OK");
                return;
            }

            var driverNames = drivers.Select(x => x.Name).ToArray();
            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Driver", driverNames);
            
            if (selectedIndex >= 0 && selectedIndex < drivers.Count)
            {
                var selectedDriver = drivers[selectedIndex];
                _loadOrder.SalesmanId = selectedDriver.Id;
                _loadOrder.Save();
                UpdateSummaryText();
            }
        }

        private async Task AddCommentAsync()
        {
            var currentComment = _loadOrder.Comments ?? string.Empty;
            var comment = await _dialogService.ShowPromptAsync("Add Comment", "Comment", "OK", "Cancel", initialValue: currentComment);
            
            if (comment != null)
            {
                _loadOrder.Comments = comment;
                _loadOrder.Save();
            }
        }

        private async Task SendOrderAsync()
        {
            // Match Xamarin SendLoadOrder() method
            if (_loadOrder == null)
            {
                await _dialogService.ShowAlertAsync("No load order to send.", "Alert", "OK");
                return;
            }

            var count = _loadOrder.Details.Count;

            // Check ship date if mandatory
            if (count > 0 && (Config.ShipDateIsMandatory || Config.ShipDateIsMandatoryForLoad) && _loadOrder.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select a ship date.", "Alert", "OK");
                return;
            }

            // Check driver selection if MasterLoadOrder
            if (Config.MasterLoadOrder && _loadOrder.Details.Count > 0)
            {
                var drivers = Salesman.List.Where(x => x.IsActive).ToList();
                if (drivers.Count > 0 && _loadOrder.SalesmanId == 0)
                {
                    await _dialogService.ShowAlertAsync("You must select a driver.", "Warning", "OK");
                    return;
                }
            }

            // Check term if required
            bool hasTerm = true;
            if (Config.UseTermsInLoadOrder)
            {
                hasTerm = false;
                if (!string.IsNullOrEmpty(_loadOrder.ExtraFields))
                {
                    var term = DataAccess.GetSingleUDF("cashTerm", _loadOrder.ExtraFields);
                    if (!string.IsNullOrEmpty(term))
                        hasTerm = true;
                }
            }

            if (!hasTerm)
            {
                // Select term first, then send (matches Xamarin SelectLoadTerm(SendIt))
                await SelectLoadTermForSendAsync();
                return;
            }

            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            // Match Xamarin SendIt() method
            if (_loadOrder == null)
                return;

            // Generate printed order ID if needed
            if (string.IsNullOrEmpty(_loadOrder.PrintedOrderId) && !Config.DontGenerateLoadPrintedId)
            {
                if ((_loadOrder.AsPresale && Config.GeneratePresaleNumber) || (!_loadOrder.AsPresale && Config.GeneratePreorderNum))
                {
                    _loadOrder.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_loadOrder);
                    _loadOrder.Save();
                }
            }

            LoadOrder.PrintedOrderId = _loadOrder.PrintedOrderId;

            // Show confirmation dialog (matches Xamarin)
            var result = await _dialogService.ShowConfirmAsync(
                "Send load order back to office?",
                "Warning",
                "Yes",
                "No");

            if (!result)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Sending load order...");

                string responseMessage = null;

                // Run in background thread (matches Xamarin ThreadPool.QueueUserWorkItem)
                await Task.Run(() =>
                {
                    try
                    {
                        // Save LoadOrder from the order (matches Xamarin LoadOrder.SaveListFromOrders())
                        LoadOrder.SaveListFromOrders();

                        // Send the load order (matches Xamarin DataAccess.SendLoadOrder())
                        DataAccess.SendLoadOrder();
                    }
                    catch (Exception ex)
                    {
                        responseMessage = "Error occurred sending load order.";
                        Logger.CreateLog(ex);
                    }
                });

                await _dialogService.HideLoadingAsync();

                // Show result message
                string title = "Alert";
                bool error = true;

                if (string.IsNullOrEmpty(responseMessage))
                {
                    title = "Success";
                    responseMessage = "Load order sent successfully.";
                    error = false;
                }

                await _dialogService.ShowAlertAsync(responseMessage, title, "OK");

                if (!error)
                {
                    // On success (matches Xamarin)
                    // Update inventory if sites exist
                    if (Sites.Count > 0)
                    {
                        await Task.Run(() => DataAccess.UpdateInventory());
                    }

                    // Disable editing (set read-only)
                    _readOnly = true;
                    IsNotReadOnly = false;

                    // Delete the order (matches Xamarin loadOrder.Delete())
                    _loadOrder.Delete();

                    // Navigate back (matches Xamarin FinishMe())
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending load order.", "Alert", "OK");
            }
        }

        private async Task SendByEmailAsync()
        {
            // Match Xamarin SendByEmail() method
            _appService.RecordEvent("EmailSendButton button");

            if (_loadOrder == null)
            {
                await _dialogService.ShowAlertAsync("No load order to send.", "Alert", "OK");
                return;
            }

            // Check ship date if mandatory
            if (_loadOrder.Details.Count > 0 && Config.ShipDateIsMandatory && _loadOrder.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select a ship date.", "Alert", "OK");
                return;
            }

            // Generate printed order ID if needed
            if (string.IsNullOrEmpty(_loadOrder.PrintedOrderId) && !Config.DontGenerateLoadPrintedId)
            {
                if ((_loadOrder.AsPresale && Config.GeneratePresaleNumber) || (!_loadOrder.AsPresale && Config.GeneratePreorderNum))
                {
                    _loadOrder.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_loadOrder);
                    _loadOrder.Save();
                }
            }

            try
            {
                await _dialogService.ShowLoadingAsync("Generating PDF...");

                // Get PDF file (matches Xamarin PdfHelper.SendLoadByEmail)
                string pdfFile = await Task.Run(() => PdfHelper.GetLoadPdf(_loadOrder));

                await _dialogService.HideLoadingAsync();

                if (string.IsNullOrEmpty(pdfFile) || !System.IO.File.Exists(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("Error generating PDF.", "Alert", "OK");
                    return;
                }

                // Compose email (matches Xamarin EmailHelper.SendLoadByEmail)
                try
                {
                    var emailMessage = new EmailMessage();

                    // Set subject and body (matches Xamarin)
                    string orderId = _loadOrder.OrderId.ToString();
                    if (!string.IsNullOrEmpty(_loadOrder.PrintedOrderId))
                        orderId = _loadOrder.PrintedOrderId;

                    emailMessage.Subject = "Load Order Attached ";
                    emailMessage.Body = emailMessage.Subject;

                    // Set recipient if configured (matches Xamarin Config.InventoryRequestEmail)
                    if (!string.IsNullOrEmpty(Config.InventoryRequestEmail))
                    {
                        emailMessage.To.Add(Config.InventoryRequestEmail);
                    }

                    // Add PDF attachment
                    emailMessage.Attachments.Add(new EmailAttachment(pdfFile));

                    await Email.ComposeAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog($"Error sending load order email via Email.ComposeAsync: {ex.Message}");
                    Logger.CreateLog(ex);

                    // Fall back to platform-specific method if Email.ComposeAsync fails
                    try
                    {
                        Config.helper?.SendReportByEmail(pdfFile);
                    }
                    catch (Exception fallbackEx)
                    {
                        Logger.CreateLog($"Error sending load order email via SendReportByEmail: {fallbackEx.Message}");
                        Logger.CreateLog(fallbackEx);
                        await _dialogService.ShowAlertAsync($"Unable to send email: {ex.Message}. Please check if an email client is configured.", "Error", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
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

