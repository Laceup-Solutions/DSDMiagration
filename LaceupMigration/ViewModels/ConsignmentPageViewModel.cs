using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ConsignmentPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order? _order;
        private bool _counting;
        private bool _initialized;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;

        public ObservableCollection<ConsignmentLineItemViewModel> LineItems { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _pageTitle = "Consignment";

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _soldQtyText = "Sold Qty: 0";

        [ObservableProperty]
        private string _leftQtyText = "Left Qty: 0";

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private bool _showTotals = true;

        [ObservableProperty]
        private bool _isCounting;

        [ObservableProperty]
        private bool _showLeftQty = true;

        [ObservableProperty]
        private bool _showTotal = true;

        [ObservableProperty]
        private bool _canEdit = true;

        [ObservableProperty]
        private bool _showAddProduct = true;

        [ObservableProperty]
        private bool _showViewCategories = true;

        [ObservableProperty]
        private bool _showSearch = true;

        public ConsignmentPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotals = !Config.HidePriceInTransaction;
        }

        public async Task InitializeAsync(int orderId)
        {
            if (_initialized && _order?.OrderId == orderId)
            {
                await RefreshAsync();
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            // Check if counting mode
            _counting = DataAccess.GetSingleUDF("ConsignmentCount", _order.ExtraFields) == "1";

            _initialized = true;
            _lastDetailCount = _order.Details.Count;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            // Equivalent to OnStart - Set order location
            if (_order != null)
            {
                _order.Latitude = DataAccess.LastLatitude;
                _order.Longitude = DataAccess.LastLongitude;
            }

            // Equivalent to OnResume/OnNewIntent - Check if items were added
            if (_order != null)
            {
                if (_order.Details.Count != _lastDetailCount)
                {
                    LoadOrderData();
                    if (_lastDetailId.HasValue)
                    {
                        var lastDetail = _order.Details.FirstOrDefault(x => x.OrderDetailId == _lastDetailId.Value);
                        if (lastDetail != null)
                        {
                            // TODO: Scroll to this detail
                            _lastDetailId = null;
                        }
                    }
                    _lastDetailCount = _order.Details.Count;
                }
            }

            // Update UI state
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (_order == null) return;

            // Update button states based on order state
            var finalized = _order.Finished;
            CanEdit = !finalized && !_order.Dexed && !_order.Voided;
            ShowAddProduct = CanEdit;
            ShowViewCategories = CanEdit && !_order.Dexed;
            ShowSearch = CanEdit;

            if (_order.Dexed || _order.Voided || _order.Finished)
            {
                CanEdit = false;
                ShowAddProduct = false;
                ShowViewCategories = false;
                ShowSearch = false;
            }

            // Handle LoadNextActivity - process pending activities
            // TODO: Implement LoadNextActivity if needed

            // Refresh order data
            LoadOrderData();
            await Task.CompletedTask;
        }

        private void LoadOrderData()
        {
            if (_order == null)
                return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            IsCounting = _counting;
            PageTitle = _counting ? "Consignment Count" : "Consignment Set";

            CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished;
            ShowAddProduct = CanEdit && (!_counting || Config.UseBattery || Config.AddSalesInConsignment);
            ShowViewCategories = CanEdit && !_order.Dexed;
            ShowSearch = CanEdit && (!_counting || Config.UseBattery || Config.AddSalesInConsignment);

            if (_order.Dexed)
            {
                CanEdit = false;
                ShowAddProduct = false;
                ShowViewCategories = false;
                ShowSearch = false;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_order == null)
                return;

            var details = _order.Details;
            LinesText = $"Lines: {details.Count}";

            if (_counting)
            {
                var soldQty = details.Where(x => x.ConsignmentCounted).Sum(x => x.Qty);
                SoldQtyText = $"Sold Qty: {soldQty}";
                ShowLeftQty = false;
            }
            else
            {
                double leftQty = 0;
                double leftTotal = 0;

                foreach (var detail in details)
                {
                    if (detail.ConsignmentUpdated)
                    {
                        leftQty += detail.ConsignmentNew;
                        leftTotal += detail.ConsignmentNew * detail.ConsignmentNewPrice;
                    }
                    else
                    {
                        if (detail.ConsignmentCounted)
                        {
                            leftQty += detail.ConsignmentCount;
                            leftTotal += detail.ConsignmentCount * detail.Price;
                        }
                        else
                        {
                            leftQty += detail.ConsignmentOld;
                            leftTotal += detail.ConsignmentOld * detail.Price;
                        }
                    }
                }

                LeftQtyText = $"Left Qty: {leftQty}";
                ShowLeftQty = true;
            }

            var total = _order.OrderTotalCost();
            TotalText = $"Total: {total.ToCustomString()}";
            ShowTotal = !Config.HidePriceInTransaction;

            // Load line items
            LineItems.Clear();
            var sortedDetails = SortDetails.SortedDetails(details).ToList();

            foreach (var detail in sortedDetails)
            {
                var lineItem = CreateLineItemViewModel(detail);
                LineItems.Add(lineItem);
            }
        }

        private ConsignmentLineItemViewModel CreateLineItemViewModel(OrderDetail detail)
        {
            var oldQtyText = $"Old Qty: {detail.ConsignmentOld}";
            
            string countText = string.Empty;
            bool showCount = false;

            if (_counting)
            {
                if (detail.ConsignmentCounted)
                {
                    countText = $"Count: {detail.ConsignmentCount}";
                    showCount = true;
                }
            }

            string newQtyText = string.Empty;
            bool showNew = false;

            if (!_counting && detail.ConsignmentUpdated)
            {
                newQtyText = $"New Qty: {detail.ConsignmentNew}";
                showNew = true;
            }

            var priceText = detail.Price > 0 ? $"Price: {detail.Price.ToCustomString()}" : string.Empty;

            return new ConsignmentLineItemViewModel
            {
                Detail = detail,
                ProductName = detail.Product.Name,
                OldQtyText = oldQtyText,
                CountText = countText,
                ShowCount = showCount,
                NewQtyText = newQtyText,
                ShowNew = showNew,
                PriceText = priceText,
                ShowPrice = !Config.HidePriceInTransaction && !string.IsNullOrEmpty(priceText)
            };
        }

        [RelayCommand]
        private async Task LineItemSelectedAsync(ConsignmentLineItemViewModel? item)
        {
            if (item == null || item.Detail == null)
                return;

            // TODO: Navigate to line item detail/edit page
            await _dialogService.ShowAlertAsync($"Selected: {item.ProductName}", "Info");
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (_order == null)
                return;

            // Equivalent to ConsignmentOrderctivityLayoutProd_Click
            OrderDetail? lastDetail = _order.Details.OrderByDescending(x => x.OrderDetailId).FirstOrDefault();

            if (lastDetail != null)
            {
                var route = $"fullcategory?orderId={_order.OrderId}&categoryId={lastDetail.Product.CategoryId}&productId={lastDetail.Product.ProductId}";
                if (_counting)
                {
                    route += "&consignmentCounting=1";
                }
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await ViewCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task ViewCategoriesAsync()
        {
            if (_order == null)
                return;

            // Equivalent to ShowCategoryActivity
            if (Category.Categories.Count == 1)
            {
                var category = Category.Categories.FirstOrDefault();
                if (category != null)
                {
                    var route = $"fullcategory?orderId={_order.OrderId}&categoryId={category.CategoryId}";
                    if (_counting)
                    {
                        route += "&consignmentCounting=1";
                    }
                    await Shell.Current.GoToAsync(route);
                }
            }
            else
            {
                var route = $"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}";
                if (_counting)
                {
                    route += "&consignmentCounting=1";
                }
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (_order == null)
                return;

            // Equivalent to ConsignmentOrderctivityLayoutSearch_Click
            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            await DoSearchResultAsync(searchTerm.ToLowerInvariant().Trim());
        }

        private async Task DoSearchResultAsync(string searchedTerm)
        {
            if (_order == null)
                return;

            var detail = _order.Details.FirstOrDefault(x => 
                x.Product.Name.ToLowerInvariant() == searchedTerm);

            if (detail != null)
            {
                // Scroll to detail
                var lineItem = LineItems.FirstOrDefault(x => x.Detail.OrderDetailId == detail.OrderDetailId);
                if (lineItem != null)
                {
                    // TODO: Scroll to item in CollectionView
                    await _dialogService.ShowAlertAsync($"Found: {detail.Product.Name}", "Info");
                }
                return;
            }

            if (!_counting || Config.UseBattery || Config.AddSalesInConsignment)
            {
                await AddItemFromSearchAsync(searchedTerm);
            }
            else
            {
                var detail2 = _order.Details.FirstOrDefault(x => 
                    x.Product.Name.ToLowerInvariant().IndexOf(searchedTerm) != -1 || 
                    x.Product.Upc.Contains(searchedTerm));

                if (detail2 == null)
                {
                    await _dialogService.ShowAlertAsync("Product not found.", "Warning");
                }
                else
                {
                    await _dialogService.ShowAlertAsync($"Found: {detail2.Product.Name}", "Info");
                }
            }
        }

        private async Task AddItemFromSearchAsync(string searchedTerm)
        {
            // Equivalent to AddItemFromSearch in ConsignmentActivity
            IEnumerable<Product> list = Product.Products;
            if (_order.Client.CategoryId != 0)
                list = ClientCategoryProducts.Find(_order.Client.CategoryId).Products;

            var products = list.Where(x => (
                x.Name.ToLowerInvariant().IndexOf(searchedTerm) != -1 ||
                x.Upc.ToLowerInvariant().Contains(searchedTerm)
            ) && (x.CategoryId != 0)).ToList();

            if (products.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No products found.", "Search");
            }
            else if (products.Count == 1)
            {
                var product = products.First();
                if (product.GetInventory(_order.AsPresale) <= 0)
                {
                    await _dialogService.ShowAlertAsync($"Not enough inventory of {product.Name}", "Warning");
                    return;
                }

                // Navigate to AddItemPage
                var route = $"additem?orderId={_order.OrderId}&productId={product.ProductId}";
                if (_counting)
                {
                    route += "&consignmentCounting=1";
                }
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                // Multiple products - navigate to product list with search
                var route = $"fullcategory?orderId={_order.OrderId}&productSearch={searchedTerm}&comingFromSearch=yes";
                if (_counting)
                {
                    route += "&consignmentCounting=1";
                }
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            if (_order == null)
                return;

            var canNavigate = await FinalizeOrderAsync();
            if (canNavigate)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        public async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            // Check if order is empty or has no consignment set items
            if (_order.Details.Count == 0 || _order.Details.Where(x => x.ConsignmentSet).Count() == 0)
            {
                _order.Delete();
                return true; // Allow navigation
            }
            else
            {
                // Set end date if not set
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                    _order.Save();
                }
                return true; // Allow navigation
            }
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_order == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_order == null)
                return options;

            var finalized = _order.Finished;
            var allowDiscount = _order.Client.UseDiscount;
            var hasPayment = InvoicePayment.List.FirstOrDefault(x => 
                x.OrderId != null && 
                x.OrderId.IndexOf(_order.UniqueId) >= 0) != null;
            var isDelivery = RouteEx.ContainsOrder(_order.OrderId);

            // Send Order
            if (_order.AsPresale)
            {
                options.Add(new MenuOption("Send Order", async () =>
                {
                    await SendOrderAsync();
                }));
            }

            // Set PO
            if (Config.SetPO && !finalized)
            {
                options.Add(new MenuOption("Set PO", async () =>
                {
                    var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:", initialValue: _order.PONumber ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(po))
                    {
                        _order.PONumber = po;
                        _order.Save();
                        await _dialogService.ShowAlertAsync("PO number set.", "Success");
                    }
                }));
            }

            // Add Payment
            if ((_order.AsPresale || (!_order.Voided && finalized && !hasPayment)) && !Config.HidePriceInTransaction)
            {
                options.Add(new MenuOption("Add Payment", async () =>
                {
                    await Shell.Current.GoToAsync($"selectinvoice?clientId={_order.Client.ClientId}&fromClientDetails=false&orderId={_order.OrderId}");
                }));
            }

            // Add Discount
            if (!finalized && allowDiscount)
            {
                options.Add(new MenuOption("Add Discount", async () =>
                {
                    await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.", "Info");
                    // TODO: Implement discount dialog
                }));
            }

            // Add Signature
            if (_order.AsPresale)
            {
                options.Add(new MenuOption("Add Signature", async () =>
                {
                    await _dialogService.ShowAlertAsync("Add Signature functionality is not yet fully implemented.", "Info");
                    // TODO: Implement signature capture
                }));
            }

            // Delete Order
            if (_order.AsPresale)
            {
                options.Add(new MenuOption("Delete Order", async () =>
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "Are you sure you want to delete this order?",
                        "Warning",
                        "Yes",
                        "No");

                    if (result)
                    {
                        _order.Delete();
                        await Shell.Current.GoToAsync("..");
                    }
                }));
            }

            // Set All to 0
            if (!finalized && !isDelivery && !Config.ParInConsignment && !Config.ConsignmentBeta)
            {
                options.Add(new MenuOption("Set All to 0", async () =>
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "Set all quantities to 0?",
                        "Warning",
                        "Yes",
                        "No");

                    if (result)
                    {
                        foreach (var detail in _order.Details)
                        {
                            float old = 0;
                            if (detail.ConsignmentUpdated)
                                old = detail.ConsignmentNew;
                            detail.ConsignmentNew = 0;
                            detail.ConsignmentUpdated = true;
                            detail.ConsignmentSet = true;
                        }
                        _order.Save();
                        LoadOrderData();
                    }
                }));
            }

            // Reset
            if (!finalized && !isDelivery)
            {
                options.Add(new MenuOption("Reset", async () =>
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "Reset all values?",
                        "Warning",
                        "Yes",
                        "No");

                    if (result)
                    {
                        var toDelete = new List<OrderDetail>();
                        foreach (var detail in _order.Details)
                        {
                            detail.ConsignmentSet = false;
                            detail.ConsignmentCount = 0;
                            detail.ConsignmentCounted = false;
                            detail.ConsignmentUpdated = false;
                            detail.ConsignmentNew = 0;

                            if (detail.ConsignmentOld == 0 && detail.Qty == 0)
                                toDelete.Add(detail);
                        }

                        foreach (var detail in toDelete)
                        {
                            _order.Details.Remove(detail);
                        }

                        _order.Save();
                        LoadOrderData();
                    }
                }));
            }

            // Send by Email
            if (!Config.ParInConsignment)
            {
                options.Add(new MenuOption("Send by Email", async () =>
                {
                    await SendByEmailAsync();
                }));
            }

            // View PDF
            if (_order.Details.Count > 0)
            {
                options.Add(new MenuOption("View PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("View PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF viewing
                }));

                options.Add(new MenuOption("Share PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF sharing
                }));
            }

            // Advanced Options
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
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

        private async Task SendOrderAsync()
        {
            if (_order == null)
                return;

            // Show confirmation dialog
            var confirm = await _dialogService.ShowConfirmAsync("Continue sending order?", "Warning", "Yes", "No");
            if (!confirm)
                return;

            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            if (_order == null)
                return;

            try
            {
                // Set end date if not set
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                    _order.Save();
                }

                // Send the orders
                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                if (batch != null)
                {
                    DataAccess.SendTheOrders(new Batch[] { batch });
                    await _dialogService.ShowAlertAsync("Order sent successfully.", "Info");
                    await _appService.GoBackToMainAsync();
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error: Batch not found.", "Alert");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending order.", "Alert");
            }
        }

        private async Task SendByEmailAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send consignment by email (matches Xamarin ConsignmentActivity)
                PdfHelper.SendConsignmentByEmail(_order, _counting);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }
    }

    public partial class ConsignmentLineItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _oldQtyText = string.Empty;

        [ObservableProperty]
        private string _countText = string.Empty;

        [ObservableProperty]
        private bool _showCount;

        [ObservableProperty]
        private string _newQtyText = string.Empty;

        [ObservableProperty]
        private bool _showNew;

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private bool _showPrice;

        public OrderDetail Detail { get; set; } = null!;
    }
}

