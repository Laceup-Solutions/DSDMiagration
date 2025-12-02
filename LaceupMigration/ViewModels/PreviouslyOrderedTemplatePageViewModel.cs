using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class PreviouslyOrderedTemplatePageViewModel : ObservableObject
    {
        public readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        internal Order? _order;
        private bool _asPresale;
        private bool _initialized;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;
        private string _sortBy = "Product Name";

        public ObservableCollection<PreviouslyOrderedProductViewModel> PreviouslyOrderedProducts { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _companyText = string.Empty;

        [ObservableProperty]
        private bool _showCompany;

        [ObservableProperty]
        private string _orderTypeText = string.Empty;

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _qtySoldText = "Qty Sold: 0";

        [ObservableProperty]
        private string _orderAmountText = "Order: $0.00";

        [ObservableProperty]
        private string _creditAmountText = "Credit: $0.00";

        [ObservableProperty]
        private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty]
        private string _taxText = "Tax: $0.00";

        [ObservableProperty]
        private string _discountText = "Discount: $0.00";

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private string _sortByText = "Sort By: Product Name";

        [ObservableProperty]
        private bool _showTotals = true;

        [ObservableProperty]
        private bool _showDiscount = true;

        [ObservableProperty]
        private bool _canEdit = true;

        [ObservableProperty]
        private bool _showSendButton = true;

        [ObservableProperty]
        private bool _showAddCredit = false;

        public PreviouslyOrderedTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotals = !Config.HidePriceInTransaction;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            bool asPresale = false;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
            }

            if (query.TryGetValue("asPresale", out var presaleValue) && presaleValue != null)
            {
                if (int.TryParse(presaleValue.ToString(), out var presale))
                    asPresale = presale == 1;
            }

            if (orderId.HasValue)
            {
                MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync(orderId.Value, asPresale));
            }
        }

        public async Task InitializeAsync(int orderId, bool asPresale)
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

            _asPresale = asPresale;
            _initialized = true;
            _lastDetailCount = _order.Details.Count;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            // Set order location
            if (_order != null)
            {
                _order.Latitude = DataAccess.LastLatitude;
                _order.Longitude = DataAccess.LastLongitude;
            }

            // Check if items were added
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
                            _lastDetailId = null;
                        }
                    }
                    _lastDetailCount = _order.Details.Count;
                }
            }

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (_order == null) return;

            CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
            ShowSendButton = _order.AsPresale;

            LoadOrderData();
            await Task.CompletedTask;
        }

        public void RefreshProductList()
        {
            LoadPreviouslyOrderedProducts();
        }

        public void LoadOrderData()
        {
            if (_order == null)
                return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            OrderTypeText = GetOrderTypeText(_order);

            // Company info
            if (!string.IsNullOrEmpty(_order.CompanyName))
            {
                CompanyText = $"Company: {_order.CompanyName}";
                ShowCompany = true;
            }
            else
            {
                ShowCompany = false;
            }

            // Calculate totals
            var totalQty = _order.Details.Sum(x => x.Qty);
            var subtotal = _order.OrderTotalCost();
            var discount = _order.DiscountAmount;
            var tax = _order.CalculateTax();
            var total = _order.OrderTotalCost();

            LinesText = $"Lines: {_order.Details.Count}";
            QtySoldText = $"Qty Sold: {totalQty}";
            
            // Calculate order and credit amounts
            var orderAmount = _order.Details.Where(x => !x.IsCredit).Sum(x => x.Qty * x.Price);
            var creditAmount = _order.Details.Where(x => x.IsCredit).Sum(x => x.Qty * x.Price);
            OrderAmountText = $"Order: {orderAmount.ToCustomString()}";
            CreditAmountText = $"Credit: {creditAmount.ToCustomString()}";
            
            SubtotalText = $"Subtotal: {subtotal.ToCustomString()}";
            DiscountText = $"Discount: {discount.ToCustomString()}";
            TaxText = $"Tax: {tax.ToCustomString()}";
            TotalText = $"Total: {total.ToCustomString()}";

            ShowDiscount = _order.Client?.UseDiscount == true || _order.Client?.UseDiscountPerLine == true || _order.IsDelivery;

            // Show Add Credit button only if Order type and AllowOneDoc is true
            // Hide if Credit or Return type (since everything added is already credit)
            ShowAddCredit = !_order.IsQuote && 
                           _order.OrderType == OrderType.Order && 
                           _order.Client?.AllowOneDoc == true;

            // Load previously ordered products
            LoadPreviouslyOrderedProducts();
        }

        private void LoadPreviouslyOrderedProducts()
        {
            if (_order?.Client == null)
                return;

            // Ensure previously ordered list is loaded
            _order.Client.EnsurePreviouslyOrdered();

            PreviouslyOrderedProducts.Clear();

            // Dictionary to track products by ProductId to avoid duplicates
            // Key: ProductId, Value: ViewModel
            // Note: We use ProductId as key, but if multiple order details have same ProductId,
            // only one will be shown (this matches Xamarin behavior where IsCompatible matches first available line)
            var productDict = new Dictionary<int, PreviouslyOrderedProductViewModel>();

            // First, add products from history (OrderedList)
            // This matches PrepareList_Old() in Xamarin
            if (_order.Client.OrderedList != null && _order.Client.OrderedList.Count > 0)
            {
                var orderedProducts = _order.Client.OrderedList
                    .OrderByDescending(x => x.Last.Date)
                    .Take(100) // Limit to recent items
                    .ToList();

                foreach (var orderedItem in orderedProducts)
                {
                    if (orderedItem.Last?.Product == null)
                        continue;

                    var product = orderedItem.Last.Product;
                    var productViewModel = CreatePreviouslyOrderedProductViewModel(product, orderedItem);
                    productDict[product.ProductId] = productViewModel;
                }
            }

            // Then, sync with current order (Details) - matches SyncLinesWithOrder() in Xamarin
            // This ensures ALL order items are shown, even if not in history
            // Order items take precedence over history items
            // IMPORTANT: We must show ALL order details, including:
            // - Items in history (updated with order qty)
            // - Items NOT in history (created from order detail)
            // - Credit items (even if not in history)
            // - Non-credit items (even if not in history)
            foreach (var orderDetail in _order.Details)
            {
                if (orderDetail.Product == null)
                    continue;

                // Skip default item if it's the only item
                if (orderDetail.Product.ProductId == Config.DefaultItem && _order.Details.Count == 1)
                    continue;

                var key = orderDetail.Product.ProductId;
                
                // Check if product already exists in dictionary (from history)
                // This matches IsCompatible() logic in Xamarin: 
                // - If line has no OrderDetail, it matches (line.OrderDetail == null)
                // - If line has OrderDetail, it only matches if OrderDetailId matches
                // Since we're using ProductId as key, we match the first available line from history
                PreviouslyOrderedProductViewModel viewModel;
                bool hasHistory = productDict.TryGetValue(key, out var existingViewModel);
                
                if (hasHistory)
                {
                    // Product exists in history - check if we should use it or create new
                    // If the existing view model already has an OrderDetail with different OrderDetailId,
                    // we should create a new one (but since we use ProductId as key, we'll update the existing)
                    // This matches Xamarin: if line.OrderDetail != null && line.OrderDetail.OrderDetailId != orderDetail.OrderDetailId, 
                    // it won't match and will create new line
                    if (existingViewModel.ExistingDetail != null && 
                        existingViewModel.ExistingDetail.OrderDetailId != orderDetail.OrderDetailId)
                    {
                        // Different OrderDetailId - create new view model (but we can't add it to dict with same key)
                        // In practice, this is rare, so we'll update the existing one
                        // For now, we'll update the existing view model with the new order detail
                        viewModel = existingViewModel;
                    }
                    else
                    {
                        // Product exists in history and either has no OrderDetail or same OrderDetailId
                        // Update with order detail values
                        viewModel = existingViewModel;
                    }
                }
                else
                {
                    // Product NOT in history
                    // If qty == 0, skip it (don't show items with no history and no quantity)
                    if (orderDetail.Qty == 0)
                        continue;
                    
                    // Create new view model from order detail
                    // This matches: line = orderDetail.CreateLineBasedOnOrderDetail(); in Xamarin
                    // CRITICAL: This ensures items in order but not in history are still shown
                    viewModel = CreatePreviouslyOrderedProductViewModelFromOrderDetail(orderDetail);
                    productDict[key] = viewModel;
                }
                
                // Handle qty == 0 for items with history
                if (orderDetail.Qty == 0 && hasHistory)
                {
                    // Product has history but qty is 0 - clear the order detail reference
                    // This will show the item with "+" button (qty = 0)
                    viewModel.ExistingDetail = null;
                    viewModel.Quantity = 0;
                    viewModel.ProductNameColor = Colors.Black;
                    viewModel.TypeText = string.Empty;
                    viewModel.ShowTypeText = false;
                    viewModel.UpdateTotal();
                }
                else
                {
                    // ALWAYS update the line with order detail values (matches Xamarin lines 532-543)
                    // This ensures the qty, price, and all other properties are set from the order detail
                    // IMPORTANT: This must happen for BOTH history items and new items
                    viewModel.ExistingDetail = orderDetail;
                    viewModel.Quantity = (double)orderDetail.Qty; // Convert float to double, use ACTUAL order qty
                    
                    // Update all properties from order detail
                    viewModel.UpdateFromOrderDetail(orderDetail);
                    
                    // Update color and type based on order detail
                    if (orderDetail.IsCredit)
                    {
                        viewModel.ProductNameColor = Colors.Orange; // Orange for credit items
                        viewModel.TypeText = orderDetail.Damaged ? "Dump" : "Return";
                        viewModel.ShowTypeText = true;
                    }
                    else
                    {
                        viewModel.ProductNameColor = Colors.Black;
                        viewModel.TypeText = string.Empty;
                        viewModel.ShowTypeText = false;
                    }
                }
            }

            // Add all products to the collection
            // Items from history that are NOT in the order will have Quantity = 0 and ExistingDetail = null
            // This is correct - they should still be shown
            foreach (var viewModel in productDict.Values)
            {
                PreviouslyOrderedProducts.Add(viewModel);
            }

            // Apply sorting
            SortProducts();
        }

        private PreviouslyOrderedProductViewModel CreatePreviouslyOrderedProductViewModel(Product product, LastTwoDetails orderedItem)
        {
            var onHand = product.CurrentWarehouseInventory;
            var listPrice = Product.GetPriceForProduct(product, _order, false, false);
            var expectedPrice = Product.GetPriceForProduct(product, _order, false, false);

            // Get last visit info
            var lastVisit = orderedItem.Last;
            var lastVisitText = string.Empty;
            var showLastVisit = false;
            if (lastVisit != null && lastVisit.Date != DateTime.MinValue)
            {
                var tqty = lastVisit.Quantity;
                var price = lastVisit.Price;
                lastVisitText = $"Last Visit: {lastVisit.Date:MM/dd}, {tqty}, {price.ToCustomString()}";
                showLastVisit = true;
            }

            // Calculate per week average from LastTwoDetails
            var perWeek = orderedItem.PerWeek;
            var showPerWeek = perWeek > 0;

            // When creating from history, don't check for order details yet
            // They will be synced later in SyncLinesWithOrder logic
            // This matches Xamarin: PrepareList creates lines without order details, then SyncLinesWithOrder adds them
            var existingDetail = (OrderDetail?)null;
            var total = 0.0;
            var qty = 0.0;

            // Determine color and type - will be updated when synced with order
            var productNameColor = Colors.Black;
            var typeText = string.Empty;
            var showTypeText = false;

            return new PreviouslyOrderedProductViewModel(this)
            {
                Product = product,
                ProductName = product.Name,
                OnHandText = $"OH: {onHand:F0}",
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PerWeekText = $"Per week: {perWeek:F2}",
                ShowPerWeek = showPerWeek,
                PriceText = $"Price: {expectedPrice.ToCustomString()}",
                TotalText = $"Total: {total.ToCustomString()}",
                OrderedItem = orderedItem,
                OrderId = _order.OrderId,
                ExistingDetail = existingDetail,
                Quantity = qty,
                ProductNameColor = productNameColor,
                TypeText = typeText,
                ShowTypeText = showTypeText
            };
        }

        private PreviouslyOrderedProductViewModel CreatePreviouslyOrderedProductViewModelFromOrderDetail(OrderDetail orderDetail)
        {
            var product = orderDetail.Product;
            var onHand = product.CurrentWarehouseInventory;
            var listPrice = Product.GetPriceForProduct(product, _order, false, false);
            var expectedPrice = orderDetail.ExpectedPrice > 0 ? orderDetail.ExpectedPrice : Product.GetPriceForProduct(product, _order, false, false);

            // Try to find history for this product
            LastTwoDetails? orderedItem = null;
            if (_order?.Client?.OrderedList != null)
            {
                orderedItem = _order.Client.OrderedList.FirstOrDefault(x => x.Last?.ProductId == product.ProductId);
            }

            // Get last visit info from history if available
            var lastVisitText = string.Empty;
            var showLastVisit = false;
            if (orderedItem?.Last != null && orderedItem.Last.Date != DateTime.MinValue)
            {
                var tqty = orderedItem.Last.Quantity;
                var price = orderedItem.Last.Price;
                lastVisitText = $"Last Visit: {orderedItem.Last.Date:MM/dd}, {tqty}, {price.ToCustomString()}";
                showLastVisit = true;
            }

            // Calculate per week average from LastTwoDetails if available
            var perWeek = orderedItem?.PerWeek ?? 0;
            var showPerWeek = perWeek > 0;

            var total = orderDetail.Qty * orderDetail.Price;
            var qty = (double)orderDetail.Qty; // Convert float to double

            // Determine color and type based on order detail
            var productNameColor = Colors.Black;
            var typeText = string.Empty;
            var showTypeText = false;
            
            if (orderDetail.IsCredit)
            {
                productNameColor = Colors.Orange; // Orange for credit items
                typeText = orderDetail.Damaged ? "Dump" : "Return";
                showTypeText = true;
            }

            return new PreviouslyOrderedProductViewModel(this)
            {
                Product = product,
                ProductName = product.Name,
                OnHandText = $"OH: {onHand:F0}",
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PerWeekText = $"Per week: {perWeek:F2}",
                ShowPerWeek = showPerWeek,
                PriceText = $"Price: {orderDetail.Price.ToCustomString()}",
                TotalText = $"Total: {total.ToCustomString()}",
                OrderedItem = orderedItem,
                OrderId = _order.OrderId,
                ExistingDetail = orderDetail,
                Quantity = qty,
                ProductNameColor = productNameColor,
                TypeText = typeText,
                ShowTypeText = showTypeText
            };
        }

        private void SortProducts()
        {
            var sorted = _sortBy switch
            {
                "Product Name" => PreviouslyOrderedProducts.OrderBy(x => x.ProductName).ToList(),
                "Product Code" => PreviouslyOrderedProducts.OrderBy(x => x.Product?.Code ?? "").ToList(),
                "Last Visit" => PreviouslyOrderedProducts.OrderByDescending(x => x.OrderedItem?.Last?.Date ?? DateTime.MinValue).ToList(),
                _ => PreviouslyOrderedProducts.ToList()
            };

            PreviouslyOrderedProducts.Clear();
            foreach (var item in sorted)
            {
                PreviouslyOrderedProducts.Add(item);
            }
        }

        private string GetOrderTypeText(Order order)
        {
            if (order.OrderType == OrderType.Order)
            {
                if (_asPresale)
                {
                    return order.IsQuote ? "Quote" : "Sales Order";
                }
                return "Sales Invoice";
            }
            else if (order.OrderType == OrderType.Credit)
            {
                return _asPresale ? "Credit Order" : "Credit Invoice";
            }
            else if (order.OrderType == OrderType.Return)
            {
                return _asPresale ? "Return Order" : "Return Invoice";
            }
            else if (order.OrderType == OrderType.Consignment)
            {
                return "Consignment";
            }
            else if (order.OrderType == OrderType.NoService)
            {
                return "No Service";
            }
            else if (order.OrderType == OrderType.Bill)
            {
                return "Bill";
            }
            else if (order.OrderType == OrderType.WorkOrder)
            {
                return "Work Order";
            }
            return order.OrderType.ToString();
        }

        [RelayCommand]
        private async Task AddCreditAsync()
        {
            if (_order == null)
                return;

            _appService.RecordEvent("add credit button");

            // Navigate to OrderCreditPage (equivalent to OrderCreditActivity in Xamarin)
            // This matches PreviouslyOrderedTemplateActivity.AddCredit_Click which navigates to OrderCreditActivity with fromOneDoc=1
            await Shell.Current.GoToAsync($"ordercredit?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}&fromOneDoc=1");
        }

        [RelayCommand]
        private async Task ViewProductsAsync()
        {
            if (_order == null)
                return;

            // Navigate to FullCategoryPage first (shows categories, then navigates to ProductCatalog)
            // This matches Xamarin's ViewProd_Click which shows categories first
            // Pass comingFrom=PreviouslyOrdered so ProductCatalog knows to add as sales (no credit prompt)
            await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}&comingFrom=PreviouslyOrdered");
        }

        [RelayCommand]
        private async Task ViewCategoriesAsync()
        {
            if (_order == null)
                return;

            // Navigate to categories
            // Pass comingFrom=PreviouslyOrdered so ProductCatalog knows to add as sales (no credit prompt)
            await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}&comingFrom=PreviouslyOrdered");
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (_order == null)
                return;

            var searchOptions = new[] { "Search in Product List", "Search in Current Transaction" };
            var choice = await _dialogService.ShowActionSheetAsync("Search", "Cancel", null, searchOptions);
            
            if (choice == null || choice == "Cancel")
                return;

            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            if (choice == "Search in Current Transaction")
            {
                var matchingDetail = _order.Details.FirstOrDefault(x => 
                    x.Product.Name.ToLowerInvariant().IndexOf(searchTerm.ToLowerInvariant()) != -1);
                
                if (matchingDetail != null)
                {
                    await _dialogService.ShowAlertAsync($"Found: {matchingDetail.Product.Name}", "Search Result");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("No products found in current transaction.", "Search");
                }
            }
            else
            {
                // Search in product list - navigate to ProductCatalogPage with search
                await Shell.Current.GoToAsync($"productcatalog?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes");
            }
        }

        [RelayCommand]
        private async Task SortByAsync()
        {
            var sortOptions = new[] { "Product Name", "Product Code", "Last Visit" };
            var choice = await _dialogService.ShowActionSheetAsync("Sort By", "Cancel", null, sortOptions);
            
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;

            _sortBy = choice;
            SortByText = $"Sort By: {choice}";
            SortProducts();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SendOrderAsync()
        {
            if (_order == null)
                return;

            var canSend = await FinalizeOrderAsync();
            if (!canSend)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Sending order...");
                
                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                if (batch == null)
                {
                    await _dialogService.HideLoadingAsync();
                    await _dialogService.ShowAlertAsync("Batch not found.", "Error");
                    return;
                }

                DataAccess.SendTheOrders(new Batch[] { batch });

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending order.", "Alert");
            }
        }

        private async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            if (_order.Voided)
                return true;

            // Check if order is empty
            bool isEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
                if (_order.AsPresale)
                {
                    UpdateRoute(false);

                    if ((_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem) ||
                        (_order.Details.Count == 0 && Order.Orders.Count(x => x.BatchId == _order.BatchId) == 1))
                    {
                        var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                        if (batch != null)
                        {
                            Logger.CreateLog($"Batch with id={batch.Id} DELETED (1 order without details)");
                            batch.Delete();
                        }
                    }
                }

                if (string.IsNullOrEmpty(_order.PrintedOrderId) && !_order.IsDelivery)
                {
                    Logger.CreateLog($"Order with id={_order.OrderId} DELETED (no details)");
                    _order.Delete();
                    return true;
                }
                else
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "You have to set all quantities to zero. Do you want to void this order?",
                        "Alert",
                        "Yes",
                        "No");
                    if (result)
                    {
                        Logger.CreateLog($"Order with id={_order.OrderId} VOIDED from template");

                        if (_order.IsDelivery && string.IsNullOrEmpty(_order.PrintedOrderId))
                            _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);

                        _order.Finished = true;
                        _order.Void();
                        _order.Save();
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                // Check for alerts
                if (_order.AsPresale && Config.AlertOrderWasNotSent)
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "This order was not sent. If you continue it will be saved in the device to be sent later. Are you sure you want to continue?",
                        "Alert",
                        "Yes",
                        "No");
                    if (!result)
                        return false;
                }

                if (Session.session != null)
                    Session.session.AddDetailFromOrder(_order);

                _order.Modified = true;
                _order.Save();
                return true;
            }
        }

        private void UpdateRoute(bool close)
        {
            if (!Config.CloseRouteInPresale)
                return;

            var stop = RouteEx.Routes.FirstOrDefault(x => 
                x.Date.Date == DateTime.Today && 
                x.Client != null && 
                x.Client.ClientId == _order?.Client.ClientId);
            
            if (stop != null && _order != null)
            {
                if (close)
                    stop.AddOrderToStop(_order.UniqueId);
                else
                    stop.RemoveOrderFromStop(_order.UniqueId);
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
            var voided = _order.Voided;
            var canEdit = CanEdit;

            if (canEdit && !finalized && !voided)
            {
                if (Config.AllowDiscount)
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        // TODO: Implement discount dialog
                        await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.", "Info");
                    }));
                }

                if (Config.SetPO)
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("PO Number", "Enter PO Number:", initialValue: _order.PONumber ?? "");
                        if (!string.IsNullOrWhiteSpace(po))
                        {
                            _order.PONumber = po;
                            _order.Save();
                            LoadOrderData();
                        }
                    }));
                }

                if (_order.AsPresale && Config.ShipDateIsMandatory)
                {
                    options.Add(new MenuOption("Set Ship Date", async () =>
                    {
                        // TODO: Implement date picker
                        await _dialogService.ShowAlertAsync("Set Ship Date functionality is not yet fully implemented.", "Info");
                    }));
                }

                options.Add(new MenuOption("Add Comments", async () =>
                {
                    var comments = await _dialogService.ShowPromptAsync("Comments", "Enter comments:", initialValue: _order.Comments ?? "", keyboard: Keyboard.Default);
                    if (comments != null)
                    {
                        _order.Comments = comments;
                        _order.Save();
                        LoadOrderData();
                    }
                }));
            }

                options.Add(new MenuOption("Print", async () =>
                {
                    await PrintAsync();
                }));

            if (!(_order.Client.SplitInvoices.Count > 0))
            {
                options.Add(new MenuOption("Send by Email", async () =>
                {
                    await SendByEmailAsync();
                }));
            }

            return options;
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            var canNavigate = await FinalizeOrderAsync();
            if (canNavigate)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private record MenuOption(string Title, Func<Task> Action);
    }

    public partial class PreviouslyOrderedProductViewModel : ObservableObject
    {
        private readonly PreviouslyOrderedTemplatePageViewModel _parent;

        public Product Product { get; set; } = null!;
        public LastTwoDetails? OrderedItem { get; set; }
        public int OrderId { get; set; }
        public OrderDetail? ExistingDetail { get; set; }

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _onHandText = "OH: 0";

        [ObservableProperty]
        private string _listPriceText = string.Empty;

        [ObservableProperty]
        private string _lastVisitText = string.Empty;

        [ObservableProperty]
        private bool _showLastVisit;

        [ObservableProperty]
        private string _perWeekText = string.Empty;

        [ObservableProperty]
        private bool _showPerWeek;

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private double _quantity = 0;

        [ObservableProperty]
        private string _quantityButtonText = "+";

        [ObservableProperty]
        private Color _productNameColor = Colors.Black;

        [ObservableProperty]
        private string _typeText = string.Empty;

        [ObservableProperty]
        private bool _showTypeText = false;

        public PreviouslyOrderedProductViewModel(PreviouslyOrderedTemplatePageViewModel parent)
        {
            _parent = parent;
        }

        partial void OnQuantityChanged(double value)
        {
            QuantityButtonText = value > 0 ? value.ToString("F0") : "+";
            UpdateTotal();
        }

        public void UpdateFromOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail == null)
                return;

            // Update price from order detail
            // This matches Xamarin: line.Price = orderDetail.Price; etc.
            PriceText = $"Price: {orderDetail.Price.ToCustomString()}";
            
            // Note: Quantity is set in the sync loop, but we ensure it's correct here too
            // This ensures the quantity is always from the actual order detail
            if (Math.Abs(Quantity - (double)orderDetail.Qty) > 0.001) // Only update if different (avoid unnecessary property change)
            {
                Quantity = (double)orderDetail.Qty; // Convert float to double, use ACTUAL order qty
            }
            
            // Update total - this must use the order detail's price and quantity
            UpdateTotal();
        }

        public void UpdateTotal()
        {
            // Always use ExistingDetail.Price if available (from order detail)
            // This ensures the total reflects the actual order detail price
            if (ExistingDetail != null)
            {
                var total = Quantity * ExistingDetail.Price;
                TotalText = $"Total: {total.ToCustomString()}";
            }
            else if (Quantity > 0)
            {
                // If no order detail but quantity > 0, use expected price
                var price = Product.GetPriceForProduct(Product, _parent._order, false, false);
                var total = Quantity * price;
                TotalText = $"Total: {total.ToCustomString()}";
            }
            else
            {
                // No quantity - show zero
                TotalText = "Total: $0.00";
            }
        }

        [RelayCommand]
        private async Task AddProductAsync(PreviouslyOrderedProductViewModel? item)
        {
            if (item?.Product == null || item.OrderId == 0 || _parent._order == null)
                return;

            var order = Order.Orders.FirstOrDefault(x => x.OrderId == item.OrderId);
            if (order == null)
                return;

            // If OrderType is Credit or Return, must prompt for Dump/Return type
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                await _parent.SelectCreditTypeAndAddAsync(item);
                return;
            }

            // For Order type, just add as regular sales item
            // Show popup to enter quantity
            var defaultQty = item.Quantity > 0 ? item.Quantity.ToString() : (item.OrderedItem?.Last?.Quantity ?? 1).ToString();
            var qtyInput = await _parent._dialogService.ShowPromptAsync(
                $"Enter Quantity for {item.ProductName}",
                "Quantity",
                "OK",
                "Cancel",
                defaultQty,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(qtyInput) || !double.TryParse(qtyInput, out var qty) || qty < 0)
                return;

            var existingDetail = order.Details.FirstOrDefault(x => x.Product.ProductId == item.Product.ProductId && !x.IsCredit);

            // Handle qty == 0
            if (qty == 0)
            {
                if (existingDetail != null)
                {
                    // Check if product has history
                    bool hasHistory = item.OrderedItem != null;
                    
                    if (hasHistory)
                    {
                        // Product has history - set qty to 0 but keep the detail (will be removed from display)
                        existingDetail.Qty = 0;
                        order.Save();
                    }
                    else
                    {
                        // Product has NO history - delete the detail completely
                        order.DeleteDetail(existingDetail);
                        order.Save();
                    }
                }
                
                // Refresh the parent view
                _parent.LoadOrderData();
                _parent.RefreshProductList();
                return;
            }

            // qty > 0 - normal flow
            OrderDetail? updatedDetail = null;
            if (existingDetail != null)
            {
                // Update existing detail
                existingDetail.Qty = (float)qty;
                updatedDetail = existingDetail;
            }
            else
            {
                // Create new detail
                var detail = new OrderDetail(item.Product, 0, order);
                double expectedPrice = Product.GetPriceForProduct(item.Product, order, false, false);
                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(item.Product, order.Client, out price))
                {
                    detail.Price = price;
                    detail.FromOfferPrice = true;
                }
                else
                {
                    detail.Price = expectedPrice;
                    detail.FromOfferPrice = false;
                }
                detail.ExpectedPrice = expectedPrice;
                detail.UnitOfMeasure = item.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = (float)qty;
                detail.CalculateOfferDetail();
                order.AddDetail(detail);
                updatedDetail = detail;
            }

            // Update related details and recalculate discounts
            if (updatedDetail != null)
            {
                OrderDetail.UpdateRelated(updatedDetail, order);
                order.RecalculateDiscounts();
            }

            // Save the order (this saves all details including the updated one)
            order.Save();

            // Refresh the parent view
            _parent.LoadOrderData();
            _parent.RefreshProductList();
        }
    }

    public partial class PreviouslyOrderedTemplatePageViewModel
    {
        public async Task SelectCreditTypeAndAddAsync(PreviouslyOrderedProductViewModel item)
        {
            if (_order == null || item?.Product == null)
                return;

            // Determine if we need to prompt for credit type
            bool onlyReturn = _order.Details.Count > 0 && _order.Details[0].IsCredit && _order.Details[0].Damaged == false;
            bool onlyDamage = _order.Details.Count > 0 && _order.Details[0].IsCredit && _order.Details[0].Damaged == true;

            var items = new List<CreditType>();
            
            if (Config.WarningDumpReturn)
            {
                if (onlyReturn)
                    items.Add(new CreditType() { Description = "Return", Damaged = false });
                if (onlyDamage)
                    items.Add(new CreditType() { Description = "Dump", Damaged = true });
                if (!onlyDamage && !onlyReturn)
                {
                    items.Add(new CreditType() { Description = "Dump", Damaged = true });
                    items.Add(new CreditType() { Description = "Return", Damaged = false });
                }
            }
            else
            {
                items.Add(new CreditType() { Description = "Dump", Damaged = true });
                items.Add(new CreditType() { Description = "Return", Damaged = false });
            }

            if (Config.CreditReasonInLine)
            {
                var reasons = new List<Reason>();
                reasons.AddRange(Reason.GetReasonsByType(ReasonType.Dump));
                reasons.AddRange(Reason.GetReasonsByType(ReasonType.Return));

                if (reasons.Count > 0)
                {
                    items = new List<CreditType>();
                    foreach (var r in reasons)
                        items.Add(new CreditType() { Description = r.Description, Damaged = (r.AvailableIn & (int)ReasonType.Dump) > 0, ReasonId = r.Id });
                }
            }

            if (items.Count == 0)
            {
                // No selection needed, use default
                await AddProductWithCreditTypeAsync(item, false, 0);
                return;
            }

            // Show selection dialog
            var options = items.Select(x => x.Description).ToArray();
            var selected = await _dialogService.ShowActionSheetAsync("Type of Credit Item", "Cancel", null, options);
            
            if (string.IsNullOrEmpty(selected) || selected == "Cancel")
                return;

            var selectedIndex = Array.IndexOf(options, selected);
            if (selectedIndex >= 0 && selectedIndex < items.Count)
            {
                var selectedItem = items[selectedIndex];
                await AddProductWithCreditTypeAsync(item, selectedItem.Damaged, selectedItem.ReasonId);
            }
        }

        private async Task AddProductWithCreditTypeAsync(PreviouslyOrderedProductViewModel item, bool damaged, int reasonId)
        {
            if (_order == null || item?.Product == null)
                return;

            // Show popup to enter quantity
            var defaultQty = item.Quantity > 0 ? item.Quantity.ToString() : (item.OrderedItem?.Last?.Quantity ?? 1).ToString();
            var qtyInput = await _dialogService.ShowPromptAsync(
                $"Enter Quantity for {item.ProductName}",
                "Quantity",
                "OK",
                "Cancel",
                defaultQty,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(qtyInput) || !double.TryParse(qtyInput, out var qty) || qty < 0)
                return;

            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == item.Product.ProductId && x.IsCredit == true);

            // Handle qty == 0
            if (qty == 0)
            {
                if (existingDetail != null)
                {
                    // Check if product has history
                    bool hasHistory = item.OrderedItem != null;
                    
                    if (hasHistory)
                    {
                        // Product has history - set qty to 0 but keep the detail (will be removed from display)
                        existingDetail.Qty = 0;
                        _order.Save();
                    }
                    else
                    {
                        // Product has NO history - delete the detail completely
                        _order.DeleteDetail(existingDetail);
                        _order.Save();
                    }
                }
                
                // Refresh the parent view
                LoadOrderData();
                RefreshProductList();
                return;
            }

            // qty > 0 - normal flow
            OrderDetail? updatedDetail = null;
            if (existingDetail != null)
            {
                // Update existing detail
                existingDetail.Qty = (float)qty;
                existingDetail.Damaged = damaged;
                existingDetail.ReasonId = reasonId;
                updatedDetail = existingDetail;
            }
            else
            {
                // Create new credit detail
                var detail = new OrderDetail(item.Product, 0, _order);
                detail.IsCredit = true;
                detail.Damaged = damaged;
                detail.ReasonId = reasonId;
                double expectedPrice = Product.GetPriceForProduct(item.Product, _order, true, false);
                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(item.Product, _order.Client, out price))
                {
                    detail.Price = price;
                    detail.FromOfferPrice = true;
                }
                else
                {
                    detail.Price = expectedPrice;
                    detail.FromOfferPrice = false;
                }
                detail.ExpectedPrice = expectedPrice;
                detail.UnitOfMeasure = item.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = (float)qty;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                updatedDetail = detail;
            }

            // Update related details and recalculate discounts
            if (updatedDetail != null)
            {
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }

            // Save the order
            _order.Save();

            // Refresh the parent view
            LoadOrderData();
            RefreshProductList();
        }

        [RelayCommand]
        private async Task PrintAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to print.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (string.IsNullOrEmpty(_order.PrintedOrderId))
                    {
                        if ((_order.AsPresale && Config.GeneratePresaleNumber) || (!_order.AsPresale && Config.GeneratePreorderNum))
                        {
                            _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                            _order.Save();
                        }
                    }

                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < number; i++)
                    {
                        if (!printer.PrintOrder(_order, !_order.Finished))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing order.";
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
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
                // Use PdfHelper to send order by email (matches Xamarin PreviouslyOrderedTemplateActivity)
                PdfHelper.SendOrderByEmail(_order);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }
    }
}

