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
        private int? _pendingOrderId = null;
        private bool _pendingAsPresale = false;

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
        private string _termsText = "Terms: ";

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

            // Store query parameters for retry in OnAppearingAsync if initialization fails
            if (orderId.HasValue)
            {
                _pendingOrderId = orderId;
                _pendingAsPresale = asPresale;
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
                // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            _asPresale = asPresale;
            _initialized = true;
            _lastDetailCount = _order.Details.Count;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            // If not initialized but we have pending orderId, try to initialize again
            // This handles the case where the app was restored from state but orders weren't loaded yet
            if (!_initialized && _pendingOrderId.HasValue)
            {
                await InitializeAsync(_pendingOrderId.Value, _pendingAsPresale);
                // If still not initialized after retry, return early
                if (!_initialized)
                    return;
            }

            if (!_initialized)
                return;

            // Set order location
            if (_order != null)
            {
                _order.Latitude = Config.LastLatitude;
                _order.Longitude = Config.LastLongitude;
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

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                CanEdit = false;
            }
            else
            {
                CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
            }
            
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
            TermsText = "Terms: " + _order.Term;

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

            // Clean up any existing details with qty=0 (should never exist)
            var detailsToDelete = _order.Details.Where(d => d.Qty == 0).ToList();
            foreach (var detail in detailsToDelete)
            {
                _order.DeleteDetail(detail);
            }
            if (detailsToDelete.Count > 0)
            {
                _order.Save();
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
                    // Create new view model from order detail
                    // This matches: line = orderDetail.CreateLineBasedOnOrderDetail(); in Xamarin
                    // CRITICAL: This ensures items in order but not in history are still shown
                    viewModel = CreatePreviouslyOrderedProductViewModelFromOrderDetail(orderDetail);
                    productDict[key] = viewModel;
                }
                
                // ALWAYS update the line with order detail values (matches Xamarin lines 532-543)
                // This ensures the qty, price, and all other properties are set from the order detail
                // IMPORTANT: This must happen for BOTH history items and new items
                // Note: Details with qty=0 are cleaned up at the start of this method, so we won't see them here
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

            // Add all products to the collection
            // Items from history that are NOT in the order will have Quantity = 0 and ExistingDetail = null
            // This is correct - they should still be shown
            foreach (var viewModel in productDict.Values)
            {
                // Set IsEnabled based on CanEdit (disable if order is read-only)
                viewModel.IsEnabled = CanEdit;
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

            // Check if order has details
            bool isEmpty = _order.Details.Count == 0 ||
                           (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);
            
            if (isEmpty)
            {
                await _dialogService.ShowAlertAsync("You can't send an empty order", "Alert");
                return;
            }

            var canSend = await FinalizeOrderAsync();
            if (!canSend)
                return;

            await ConfirmationSendAsync();
        }

        private async Task ConfirmationSendAsync()
        {
            if (_order == null)
                return;

            if (_order.AsPresale)
            {
                // Check minimum weight
                var totalWeight = _order.TotalWeight;
                if (Config.MinimumWeight > 0 && totalWeight < Config.MinimumWeight)
                {
                    await _dialogService.ShowAlertAsync(
                        $"Order minimum total weight is {Config.MinimumWeight}. Current weight is {totalWeight}.",
                        "Alert");
                    return;
                }

                // Check minimum amount
                var orderCost = _order.OrderTotalCost();
                if (!Config.Simone && Config.MinimumAmount > 0 && orderCost < Config.MinimumAmount)
                {
                    await _dialogService.ShowAlertAsync(
                        $"Order minimum total amount is {Config.MinimumAmount.ToCustomString()}. Current amount is {orderCost.ToCustomString()}.",
                        "Alert");
                    return;
                }

                // Check mandatory comments
                if (string.IsNullOrEmpty(_order.Comments) && Config.PresaleCommMandatory)
                {
                    await _dialogService.ShowAlertAsync("Please provide a comment.", "Alert");
                    return;
                }

                // Check signature name
                if (Config.SignatureNameRequired && string.IsNullOrEmpty(_order.SignatureName))
                {
                    await _dialogService.ShowAlertAsync("Signature name is required.", "Alert");
                    return;
                }

                // Check ShipVia
                if (Config.ShipViaMandatory)
                {
                    var shipVia = DataAccess.GetSingleUDF("ShipVia", _order.ExtraFields);
                    if (string.IsNullOrEmpty(shipVia))
                    {
                        await _dialogService.ShowAlertAsync("Must add ShipVia.", "Alert");
                        return;
                    }
                }

                // Check DisolSurvey
                if (Config.UseDisolSurvey && _order.OrderType == OrderType.Order && !_order.HasDisolSurvey)
                {
                    string survey = DataAccess.GetSingleUDF("Survey", _order.Client.ExtraPropertiesAsString);
                    bool mustFillSurvey = survey == "1";
                    if (mustFillSurvey)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "Debe completar una encuesta antes de continuar. Desea realizar la encuesta ahora?",
                            "Alert",
                            "Yes",
                            "No");
                        if (result)
                        {
                            // TODO: Navigate to survey
                            await _dialogService.ShowAlertAsync("Survey functionality is not yet fully implemented.", "Info");
                        }
                        return;
                    }
                }

                // Check mandatory image
                if (Config.CaptureImages && Config.ImageInOrderMandatory && _order.ImageList.Count <= 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory to send presale.", "Warning");
                    return;
                }

                // Check discounts
                var shipdate = _order.ShipDate != DateTime.MinValue ? _order.ShipDate : DateTime.Now;
                if (OrderDiscount.HasDiscounts && _order.DiscountAmount == 0 && 
                    _order.Details.Any(x => OrderDiscount.ProductHasDiscount(x.Product, x.Qty, _order, shipdate, x.UnitOfMeasure, x.IsFreeItem)))
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "This client has discounts available to apply. Are you sure you want to continue sending it?",
                        "Alert",
                        "Yes",
                        "No");
                    if (!result)
                        return;
                }

                // Check if shipdate is locked
                if (Config.CheckIfShipdateLocked)
                {
                    var lockedDates = new List<DateTime>();
                    if (!DataAccess.CheckIfShipdateIsValid(new List<DateTime>() { _order.ShipDate }, ref lockedDates))
                    {
                        var sb = string.Empty;
                        foreach (var l in lockedDates)
                            sb += '\n' + l.Date.ToShortDateString();
                        await _dialogService.ShowAlertAsync("The selected date is currently locked. Please select a different shipdate", "Alert");
                        return;
                    }
                }

                // Check suggested categories
                if (SuggestedClientCategory.List.Count > 0)
                {
                    var suggestedForthisCLient = SuggestedClientCategory.List.FirstOrDefault(x => 
                        x.SuggestedClientCategoryClients.Any(y => y.ClientId == _order.Client.ClientId));

                    if (suggestedForthisCLient != null)
                    {
                        bool containedInOrder = true;
                        var product_ = Product.GetProductListForOrder(_order, false, 0);

                        foreach (var p in suggestedForthisCLient.SuggestedClientCategoryProducts)
                        {
                            if (!product_.Any(x => x.ProductId == p.ProductId))
                                continue;

                            if (!_order.Details.Any(x => x.Product.ProductId == p.ProductId))
                            {
                                containedInOrder = false;
                                break;
                            }
                        }

                        if (!containedInOrder)
                        {
                            var warning = "Continue sending suggested " + 
                                (string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) ? "suggested" : Config.ProductCategoryNameIdentifier) + 
                                " " + "continue sending suggested part 2";
                            var result = await _dialogService.ShowConfirmAsync(warning, "Alert", "Yes", "No");
                            if (!result)
                            {
                                // TODO: Navigate to suggested
                                await _dialogService.ShowAlertAsync("Suggested functionality is not yet fully implemented.", "Info");
                                return;
                            }
                        }
                    }
                }
            }

            // Show confirmation dialog
            var confirmResult = await _dialogService.ShowConfirmAsync(
                "Are you sure you want to send the order?",
                "Confirm Shipping",
                "Yes",
                "No");
            if (!confirmResult)
                return;

            // Validate order minimum
            bool validQty = _order.ValidateOrderMinimum();
            if (!validQty)
                return;

            // Send the order
            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            if (_order == null)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Sending order...");

                if (_order.EndDate == DateTime.MinValue)
                    _order.EndDate = DateTime.Now;

                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);

                if (_order.AsPresale)
                    UpdateRoute(true);

                _order.Save();

                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                if (batch == null)
                {
                    // Create batch if it doesn't exist
                    batch = new Batch(_order.Client);
                    batch.Client = _order.Client;
                    batch.ClockedIn = DateTime.Now;
                    batch.ClockedOut = DateTime.Now;
                    batch.Save();

                    _order.BatchId = batch.Id;
                    _order.Save();
                }

                DataProvider.SendTheOrders(new Batch[] { batch });

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success");

                // [ACTIVITY STATE]: Remove state when properly exiting
                Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending order.", "Alert");
            }
        }

        private async Task ContinueAfterAlertAsync()
        {
            if (_order == null)
                return;

            // Validate order minimum
            bool validQty = _order.ValidateOrderMinimum();
            if (!validQty)
                return;

            // Set end date if not set
            if (_order.EndDate == DateTime.MinValue)
            {
                _order.EndDate = DateTime.Now;
            }

            // Update route if presale
            if (_order.AsPresale)
            {
                UpdateRoute(true);

                if (Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                }

                // Check DisolSurvey
                if (Config.UseDisolSurvey && _order.OrderType == OrderType.Order && !_order.HasDisolSurvey)
                {
                    string survey = DataAccess.GetSingleUDF("Survey", _order.Client.ExtraPropertiesAsString);
                    bool mustFillSurvey = survey == "1";
                    if (mustFillSurvey)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "Debe completar una encuesta antes de continuar. Desea realizar la encuesta ahora?",
                            "Alert",
                            "Yes",
                            "No");
                        if (result)
                        {
                            // TODO: Navigate to survey
                            await _dialogService.ShowAlertAsync("Survey functionality is not yet fully implemented.", "Info");
                        }
                        return;
                    }
                }
            }

            _order.Modified = true;
            _order.Save();

            // [ACTIVITY STATE]: Remove state when properly exiting
            // Remove the order page state so it doesn't get restored when navigating back
            Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");

            // Also remove from ActivityState directly to ensure it's completely removed
            var orderState = ActivityState.GetState("PreviouslyOrderedTemplateActivity");
            if (orderState != null)
            {
                ActivityState.RemoveState(orderState);
            }

            // Navigate back - matches Xamarin's Finish() behavior which just closes the current Activity
            // and returns to the previous one (ClientDetailsPage)
            await Shell.Current.GoToAsync("..");
        }

        private async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            if (_order.Voided)
                return true;

            // Check if order is empty - matches Xamarin PreviouslyOrderedTemplateActivity logic
            // Empty means: no details, or only default item
            // Note: Details with qty=0 are always deleted, so we don't need to check for them
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

                // Delete empty order if it hasn't been printed and is not a delivery
                // This matches Xamarin PreviouslyOrderedTemplateActivity.FinalizeOrder logic
                if (string.IsNullOrEmpty(_order.PrintedOrderId) && !_order.IsDelivery)
                {
                    Logger.CreateLog($"Order with id={_order.OrderId} DELETED (no details)");
                    _order.Delete();
                    return true;
                }
                else
                {
                    // If order has been printed or is a delivery, ask to void instead
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

                // Show Action Options dialog if presale (matching Xamarin behavior)
                if (_order.AsPresale)
                {
                    var options = new string[]
                    {
                        "Send Order",
                        "Save Order To Send Later",
                        "Stay In The Order"
                    };

                    var selectedIndex = await _dialogService.ShowSingleChoiceDialogAsync("Action Options", options, 1);
                    
                    if (selectedIndex == -1)
                    {
                        // Cancel button clicked - stay in order
                        return false;
                    }

                    switch (selectedIndex)
                    {
                        case 0:
                            // Send Order
                            await ConfirmationSendAsync();
                            return false; // ConfirmationSendAsync already navigates, don't navigate again
                        case 1:
                            // Save Order To Send Later
                            await ContinueAfterAlertAsync();
                            return false; // ContinueAfterAlertAsync already navigates, don't navigate again
                        case 2:
                            // Stay In The Order
                            return false; // Don't navigate
                    }
                }

                // Set end date if not set
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                }

                // Update route if presale
                if (_order.AsPresale)
                {
                    UpdateRoute(true);
                }

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

        public List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_order == null)
                return options;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), only show Print option
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                // Only allow Print when read-only
                options.Add(new MenuOption("Print", async () =>
                {
                    await PrintAsync();
                }));
                return options;
            }

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
            if (_order == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Check if order is empty - if so, handle it and return (matching Xamarin logic)
            bool isEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
                // Handle empty order - this will either delete it or show void dialog
                var canNavigate = await FinalizeOrderAsync();
                if (canNavigate)
                {
                    // [ACTIVITY STATE]: Remove state when navigating away programmatically
                    Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");
                    
                    await Shell.Current.GoToAsync("..");
                }
                return; // Don't show the 3-option dialog for empty orders
            }

            // Check if order is presale and show dialog with 3 options (only for non-empty orders)
            if (_order.AsPresale)
            {
                // Show action options dialog (matching Xamarin PreviouslyOrderedTemplateActivity logic)
                var options = new[]
                {
                    "Send Order",
                    "Save Order To Send Later",
                    "Stay In The Order"
                };

                var choice = await _dialogService.ShowActionSheetAsync("Action Options", "Cancel", null, options);
                
                if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                    return; // User cancelled, stay in order

                switch (choice)
                {
                    case "Send Order":
                        // Call SendOrderAsync which handles validation and sending
                        await SendOrderAsync();
                        break;
                    case "Save Order To Send Later":
                        // Continue after alert - finalize order and navigate
                        var canNavigate = await FinalizeOrderAsync();
                        if (canNavigate)
                        {
                            // [ACTIVITY STATE]: Remove state when navigating away programmatically
                            Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");
                            
                            await Shell.Current.GoToAsync("..");
                        }
                        break;
                    case "Stay In The Order":
                        // Do nothing, stay in the order
                        return;
                }
            }
            else
            {
                // Non-presale order - use normal finalization logic
                var canNavigate = await FinalizeOrderAsync();
                if (canNavigate)
                {
                    // [ACTIVITY STATE]: Remove state when navigating away programmatically
                    Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");
                    
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

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

        [ObservableProperty]
        private bool _isEnabled = true;

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

            // Handle qty == 0 - always delete the detail, never keep details with qty=0
            if (qty == 0)
            {
                if (existingDetail != null)
                {
                    // Always delete the detail when qty is set to 0
                    order.DeleteDetail(existingDetail);
                    order.Save();
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

            // Handle qty == 0 - always delete the detail, never keep details with qty=0
            if (qty == 0)
            {
                if (existingDetail != null)
                {
                    // Always delete the detail when qty is set to 0
                    _order.DeleteDetail(existingDetail);
                    _order.Save();
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

        // private async Task SendByEmailAsync()
        // {
        //     if (_order == null)
        //     {
        //         await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
        //         return;
        //     }
        //
        //     try
        //     {
        //         // Use PdfHelper to send order by email (matches Xamarin PreviouslyOrderedTemplateActivity)
        //         await PdfHelper.SendOrderByEmail(_order);
        //     }
        //     catch (Exception ex)
        //     {
        //         Logger.CreateLog(ex);
        //         await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
        //     }
        // }

        private async Task SendByEmailAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
                return;
            }

            try
            {
                await _dialogService.ShowLoadingAsync("Generating PDF...");
        
                string pdfFile = PdfHelper.GetOrderPdf(_order);
        
                await _dialogService.HideLoadingAsync();
        
                if (string.IsNullOrEmpty(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("Error generating PDF.", "Alert", "OK");
                    return;
                }

                // Navigate to PDF viewer with both PDF path and orderId
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={_order.OrderId}");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred.", "Alert", "OK");
            }
        }

private async Task SharePdfAsync(string pdfFile)
{
    try
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Share Order",
            File = new ShareFile(pdfFile)
        });
    }
    catch (Exception ex)
    {
        Logger.CreateLog(ex);
        await _dialogService.ShowAlertAsync("Error sharing PDF.", "Alert", "OK");
    }
}
    }
}

