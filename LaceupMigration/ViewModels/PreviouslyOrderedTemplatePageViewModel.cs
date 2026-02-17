using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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
        
        private bool inventoryUpdated = false;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;
        private SortDetails.SortCriteria _sortCriteria = SortDetails.SortCriteria.ProductName;
        private bool _justOrdered = false;
        private int? _pendingOrderId = null;
        private bool _pendingAsPresale = false;
        
        // WhatToViewInList enum and field (matches TemplateActivity)
        private enum WhatToViewInList
        {
            All,
            Selected
        }
        
        private WhatToViewInList _whatToViewInList = WhatToViewInList.All;

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

        private string GetSortCriteriaName(SortDetails.SortCriteria criteria)
        {
            return criteria switch
            {
                SortDetails.SortCriteria.ProductName => "Product Name",
                SortDetails.SortCriteria.ProductCode => "Product Code",
                SortDetails.SortCriteria.Category => "By Category",
                SortDetails.SortCriteria.InStock => "In Stock",
                SortDetails.SortCriteria.Qty => "Qty",
                SortDetails.SortCriteria.Descending => "Descending",
                SortDetails.SortCriteria.OrderOfEntry => "Order of Entry",
                SortDetails.SortCriteria.WarehouseLocation => "Warehouse Location",
                SortDetails.SortCriteria.CategoryThenByCode => "Category then by Code",
                _ => "Product Name"
            };
        }

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

        [ObservableProperty]
        private string _actionButtonsColumnDefinitions = "*,*,*,*";

        partial void OnShowAddCreditChanged(bool value)
        {
            ActionButtonsColumnDefinitions = value ? "*,*,*,*,*" : "*,*,*,*";
        }

        public PreviouslyOrderedTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotals = !Config.HidePriceInTransaction;
            
            // Subscribe to sort criteria messages
            MessagingCenter.Subscribe<SortByDialogViewModel, Tuple<SortDetails.SortCriteria, bool>>(
                this, "SortCriteriaApplied", (sender, args) =>
                {
                    ApplySortCriteria(args.Item1, args.Item2);
                });
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
            
            // Initialize sort criteria from Config (matches Xamarin behavior)
            _sortCriteria = SortDetails.GetCriteriaFromName(Config.PrintInvoiceSort);
            SortByText = $"Sort By: {GetSortCriteriaName(_sortCriteria)}";
            
            // Initialize Just Ordered filter based on whether order has details
            if (_order.Details.Count > 0)
            {
                _whatToViewInList = WhatToViewInList.Selected; // Show only ordered items if order has details
                _justOrdered = true; // Keep for backward compatibility with SortByDialog
            }
            
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

            // Xamarin PreviouslyOrderedTemplateActivity: when creating Activity, if UpdateInventoryInPresale && AsPresale run inventory update then refresh, else just refresh
            // Skip the one-time inventory update during state restoration so we don't block navigating through to the restored page (e.g. ProductCatalogPage)
            if (!ActivityStateRestorationService.IsRestoringState && !inventoryUpdated && _order != null && Config.UpdateInventoryInPresale && _order.AsPresale)
            {
                inventoryUpdated = true;
                await RunPresaleInventoryUpdateAsync();
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

        /// <summary>Runs presale inventory update in background (matches Xamarin PreviouslyOrderedTemplateActivity when creating Activity). Shows "Updating Inventory" dialog. Caller refreshes list after.</summary>
        public async Task RunPresaleInventoryUpdateAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return;
            await _dialogService.ShowLoadingAsync("Updating Inventory");
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var forSite = ((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0) || Config.PresaleUseInventorySite;
                        DataProvider.RunInventorySync(forSite, true);
                        var validOrders = Order.Orders.Where(x => x.AsPresale && (x.OrderType == OrderType.Order || x.OrderType == OrderType.Return || x.OrderType == OrderType.Credit)).ToList();
                        foreach (var order in validOrders)
                        {
                            foreach (var o in order.Details)
                                order.UpdateInventory(o, -1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex.ToString());
                    }
                });
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }
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

            // One line per order detail: create one row for each OrderDetail in the current order
            var itemsToAdd = new List<PreviouslyOrderedProductViewModel>();
            var productIdsInOrder = new HashSet<int>();

            foreach (var orderDetail in _order.Details)
            {
                if (orderDetail.Product == null)
                    continue;

                // Skip default item if it's the only item
                if (orderDetail.Product.ProductId == Config.DefaultItem && _order.Details.Count == 1)
                    continue;

                productIdsInOrder.Add(orderDetail.Product.ProductId);

                var viewModel = CreatePreviouslyOrderedProductViewModelFromOrderDetail(orderDetail);
                viewModel.IsCreditLine = orderDetail.IsCredit;
                viewModel.Quantity = (double)orderDetail.Qty;
                viewModel.UpdateFromOrderDetail(orderDetail);

                if (orderDetail.IsCredit)
                {
                    viewModel.ProductNameColor = Colors.Orange;
                    viewModel.TypeText = orderDetail.Damaged ? "Dump" : "Return";
                    viewModel.ShowTypeText = true;
                }
                else
                {
                    viewModel.ProductNameColor = Colors.Black;
                    viewModel.TypeText = string.Empty;
                    viewModel.ShowTypeText = false;
                }

                viewModel.IsEnabled = CanEdit;
                itemsToAdd.Add(viewModel);
            }

            // Add one row per product from history that is NOT yet in the order (so user can tap to add)
            if (_order.Client.OrderedList != null && _order.Client.OrderedList.Count > 0)
            {
                var orderedProducts = _order.Client.OrderedList
                    .OrderByDescending(x => x.Last.Date)
                    .Take(100)
                    .ToList();

                foreach (var orderedItem in orderedProducts)
                {
                    if (orderedItem.Last?.Product == null)
                        continue;

                    var productId = orderedItem.Last.ProductId;
                    if (productIdsInOrder.Contains(productId))
                        continue; // already have at least one row for this product from order details

                    var productViewModel = CreatePreviouslyOrderedProductViewModel(orderedItem.Last.Product, orderedItem);
                    productViewModel.IsEnabled = CanEdit;
                    itemsToAdd.Add(productViewModel);
                }
            }

            // Apply "Just Ordered" filter: when selected, only show rows that are in the current order
            if (_whatToViewInList == WhatToViewInList.Selected)
            {
                itemsToAdd = itemsToAdd.Where(x => x.OrderDetailId != null).ToList();
            }

            PreviouslyOrderedProducts.Clear();
            foreach (var item in itemsToAdd)
            {
                PreviouslyOrderedProducts.Add(item);
            }

            SortProducts();
        }

        private PreviouslyOrderedProductViewModel CreatePreviouslyOrderedProductViewModel(Product product, LastTwoDetails orderedItem)
        {
            // When order is not AsPresale, OH = truck inventory (GetInventory returns truck); otherwise warehouse. No UOM yet (history-only row).
            var baseInv = _order != null ? product.GetInventory(_order.AsPresale, false) : product.CurrentWarehouseInventory;
            var onHand = baseInv; // no UOM for history-only row
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

            // Check if product is suggested
            var isSuggested = _order != null && _order.Client != null && Product.IsSuggestedForClient(_order.Client, product);
            string suggestedLabelText = string.Empty;
            if (isSuggested)
            {
                suggestedLabelText = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                    ? "Suggested Products" 
                    : $"{Config.ProductCategoryNameIdentifier} Products";
            }

            return new PreviouslyOrderedProductViewModel(this)
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                OnHandText = $"OH: {onHand:F0}",
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                UomDisplayText = string.Empty,
                ShowUomLabel = false,
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PerWeekText = $"Per week: {perWeek:F2}",
                ShowPerWeek = showPerWeek,
                PriceText = $"Price: {expectedPrice.ToCustomString()}",
                TotalText = $"Total: {total.ToCustomString()}",
                DefaultQuantityFromHistory = orderedItem?.Last?.Quantity,
                OrderId = _order.OrderId,
                OrderDetailId = existingDetail?.OrderDetailId,
                IsCreditLine = false,
                Quantity = qty,
                ProductNameColor = productNameColor,
                TypeText = typeText,
                ShowTypeText = showTypeText,
                IsSuggested = isSuggested,
                SuggestedLabelText = suggestedLabelText
            };
        }

        private PreviouslyOrderedProductViewModel CreatePreviouslyOrderedProductViewModelFromOrderDetail(OrderDetail orderDetail)
        {
            var product = orderDetail.Product;
            // OH in this detail's UOM: base inventory / UOM conversion (so it updates as qty is modified)
            var baseInv = _order != null ? product.GetInventory(_order.AsPresale, false) : product.CurrentWarehouseInventory;
            var conversion = orderDetail.UnitOfMeasure?.Conversion ?? 1f;
            var onHandInUom = conversion != 0 ? baseInv / conversion : baseInv;
            var onHand = onHandInUom;
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

            // Check if product is suggested
            var isSuggested = _order != null && _order.Client != null && Product.IsSuggestedForClient(_order.Client, product);
            string suggestedLabelText = string.Empty;
            if (isSuggested)
            {
                suggestedLabelText = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                    ? "Suggested Products" 
                    : $"{Config.ProductCategoryNameIdentifier} Products";
            }

            var uomText = orderDetail.UnitOfMeasure != null ? orderDetail.UnitOfMeasure.Name : string.Empty;
            var uomDisplayText = orderDetail.UnitOfMeasure != null ? "UOM: " + orderDetail.UnitOfMeasure.Name : string.Empty;
            var showUomLabel = orderDetail.UnitOfMeasure != null;

            var vm = new PreviouslyOrderedProductViewModel(this)
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                OnHandText = $"OH: {onHand:F0}",
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                UomDisplayText = uomDisplayText,
                ShowUomLabel = showUomLabel,
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PerWeekText = $"Per week: {perWeek:F2}",
                ShowPerWeek = showPerWeek,
                PriceText = $"Price: {orderDetail.Price.ToCustomString()}",
                UomText = uomText,
                TotalText = $"Total: {total.ToCustomString()}",
                DefaultQuantityFromHistory = orderedItem?.Last?.Quantity,
                OrderId = _order.OrderId,
                OrderDetailId = orderDetail.OrderDetailId,
                IsCreditLine = orderDetail.IsCredit,
                Quantity = qty,
                ProductNameColor = productNameColor,
                TypeText = typeText,
                ShowTypeText = showTypeText,
                IsSuggested = isSuggested,
                SuggestedLabelText = suggestedLabelText
            };
            vm.UpdateOrgQtyFromOrder();
            return vm;
        }

        private void SortProducts()
        {
            if (_order == null)
                return;

            List<PreviouslyOrderedProductViewModel> sorted = _sortCriteria switch
            {
                SortDetails.SortCriteria.ProductName => PreviouslyOrderedProducts.OrderBy(x => x.ProductName).ToList(),
                SortDetails.SortCriteria.ProductCode => PreviouslyOrderedProducts.OrderBy(x => GetProductById(x.ProductId)?.Code ?? "").ToList(),
                SortDetails.SortCriteria.Category => PreviouslyOrderedProducts
                    .OrderBy(x => GetProductById(x.ProductId)?.CategoryId ?? 0)
                    .ThenBy(x => x.ProductName)
                    .ToList(),
                SortDetails.SortCriteria.InStock => PreviouslyOrderedProducts
                    .OrderByDescending(x => GetProductById(x.ProductId)?.GetInventory(_order.AsPresale, false) ?? 0)
                    .ToList(),
                SortDetails.SortCriteria.Qty => PreviouslyOrderedProducts
                    .OrderByDescending(x => x.Quantity)
                    .ToList(),
                SortDetails.SortCriteria.Descending => PreviouslyOrderedProducts
                    .OrderByDescending(x => x.ProductName)
                    .ToList(),
                SortDetails.SortCriteria.OrderOfEntry => PreviouslyOrderedProducts
                    .OrderBy(x => x.OrderDetailId ?? int.MaxValue)
                    .ToList(),
                SortDetails.SortCriteria.WarehouseLocation => PreviouslyOrderedProducts
                    .OrderBy(x => string.IsNullOrEmpty(GetProductById(x.ProductId)?.WarehouseLocation) ? 1 : 0)
                    .ThenBy(x => GetProductById(x.ProductId)?.WarehouseLocation ?? "")
                    .ToList(),
                SortDetails.SortCriteria.CategoryThenByCode => PreviouslyOrderedProducts
                    .OrderBy(x => GetProductById(x.ProductId)?.CategoryId ?? 0)
                    .ThenBy(x => GetProductById(x.ProductId)?.Code ?? "")
                    .ToList(),
                _ => PreviouslyOrderedProducts.ToList()
            };

            PreviouslyOrderedProducts.Clear();
            foreach (var item in sorted)
            {
                PreviouslyOrderedProducts.Add(item);
            }
        }

        /// <summary>Resolve Product by id (from order details or product list). Used so list ViewModels only hold ProductId.</summary>
        internal Product? GetProductById(int productId)
        {
            if (_order == null) return null;
            var fromDetail = _order.Details.FirstOrDefault(d => d.Product?.ProductId == productId)?.Product;
            if (fromDetail != null) return fromDetail;
            var list = Product.GetProductListForOrder(_order, false, 0);
            return list.FirstOrDefault(p => p.ProductId == productId);
        }

        /// <summary>Resolve OrderDetail by id. Used so list ViewModels only hold OrderDetailId.</summary>
        internal OrderDetail? GetOrderDetailById(int? orderDetailId)
        {
            if (_order == null || !orderDetailId.HasValue) return null;
            return _order.Details.FirstOrDefault(d => d.OrderDetailId == orderDetailId.Value);
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
            var choice = await _dialogService.ShowActionSheetAsync("Search", "", "Cancel", searchOptions);
            
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
            // Get current sort criteria from Config (matches Xamarin behavior)
            var currentCriteria = SortDetails.GetCriteriaFromName(Config.PrintInvoiceSort);
            
            // Navigate to SortByDialogPage
            await Shell.Current.GoToAsync($"sortbydialog?currentCriteria={(int)currentCriteria}&justOrdered={(_whatToViewInList == WhatToViewInList.Selected ? 1 : 0)}&callback=PreviouslyOrderedTemplate");
        }

        public void ApplySortCriteria(SortDetails.SortCriteria criteria, bool justOrdered)
        {
            _sortCriteria = criteria;
            _whatToViewInList = justOrdered ? WhatToViewInList.Selected : WhatToViewInList.All;
            _justOrdered = justOrdered; // Keep for backward compatibility
            
            // Save to Config (matches Xamarin SaveSortCriteria)
            SortDetails.SaveSortCriteria(criteria);
            
            // Update UI
            SortByText = $"Sort By: {GetSortCriteriaName(criteria)}";
            
            // Reload and sort products
            LoadPreviouslyOrderedProducts();
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
                    var shipVia = UDFHelper.GetSingleUDF("ShipVia", _order.ExtraFields);
                    if (string.IsNullOrEmpty(shipVia))
                    {
                        await _dialogService.ShowAlertAsync("Must add ShipVia.", "Alert");
                        return;
                    }
                }

                // Check DisolSurvey
                if (Config.UseDisolSurvey && _order.OrderType == OrderType.Order && !_order.HasDisolSurvey)
                {
                    string survey = UDFHelper.GetSingleUDF("Survey", _order.Client.ExtraPropertiesAsString);
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
                    if (!DataProvider.CheckIfShipdateIsValid(new List<DateTime>() { _order.ShipDate }, ref lockedDates))
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

                // Set client Editable to false when sending presale orders (fixes Xamarin bug)
                // Only for locally created clients (ClientId <= 0)
                if (_order.AsPresale && _order.Client != null && _order.Client.ClientId <= 0)
                {
                    _order.Client.Editable = false;
                    Client.Save();
                }

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
                    string survey = UDFHelper.GetSingleUDF("Survey", _order.Client.ExtraPropertiesAsString);
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

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_order == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", options.Select(o => o.Title).ToArray());
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

            var finalized = _order.Finished;
            var voided = _order.Voided;
            var canEdit = CanEdit;
            var locked = _order.Locked();
            var asset = UDFHelper.GetSingleUDF("workOrderAsset", _order.ExtraFields);
            var isWorkOrder = _order != null && _order.IsWorkOrder;
            var hasOrderDiscounts = OrderDiscount.List.Count > 0 && !isWorkOrder;
            var allowDiscount = _order.Client.UseDiscount && !OrderDiscount.HasDiscounts;
            var isSplitClient = _order.Client.SplitInvoices.Count > 0;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), only show Print option
            bool isReadOnly = !_order.AsPresale && (finalized || voided);
            
            if (isReadOnly)
            {
                // Only allow Print when read-only
                options.Add(new MenuOption("Print", async () =>
                {
                    await PrintAsync();
                }));
                return options;
            }

            // Scanner (only if Config.ScannerToUse == 4)
            if (Config.ScannerToUse == 4)
            {
                options.Add(new MenuOption("Scan", async () =>
                {
                    await GotoBarcodeReaderAsync();
                }));
            }

            // Presale-specific menu items
            if (_order.AsPresale)
            {
                
                // Set Ship Date (presale)
                if (Config.SetShipDate)
                {
                    options.Add(new MenuOption("Set Ship Date", async () =>
                    {
                        await SetShipDateAsync();
                    }));
                }
                
                
                // Set PO (presale)
                options.Add(new MenuOption(_order.OrderType == OrderType.Bill ? "Set Bill Number" : "Set PO", async () =>
                {
                    await GetPONumberAsync();
                }));

                
                // Comments (presale)
                options.Add(new MenuOption("Add Comments", async () =>
                {
                    await ShowCommentsDialogAsync();
                }));
                
                // Signature (presale) - Xamarin order: Ordered By before Send (presaleMenu.xml)
                options.Add(new MenuOption("Ordered By", async () =>
                {
                    await SignAsync();
                }));
                
                // Send Order (presale)
                options.Add(new MenuOption("Send", async () =>
                {
                    await SendOrderAsync();
                }));
                
                // Email (presale) - Xamarin: Send by Email visible when !isSplitClient
                if (!isSplitClient)
                {
                    options.Add(new MenuOption("Send by Email", async () =>
                    {
                        await SendByEmailAsync();
                    }));
                }

                options.Add(new MenuOption("Select Driver", async () =>
                {
                    await SelectSalesmanAsync();
                }));
                
                // Other Charges - Xamarin order: after Select Driver (presaleMenu.xml)
                if (!string.IsNullOrEmpty(asset) || Config.AllowOtherCharges)
                {
                    options.Add(new MenuOption("Other Charges", async () =>
                    {
                        await OtherChargesAsync();
                    }));
                }
                    
                // What To View (presale)
                options.Add(new MenuOption(_whatToViewInList == WhatToViewInList.All ? "Added/All" : "Just Ordered", async () =>
                {
                    await WhatToViewClickedAsync();
                }));
                    
                // Offers vs Order Discount (presale)
                if (!hasOrderDiscounts)
                {
                    options.Add(new MenuOption("Offers", async () =>
                    {
                        await ShowOffersAsync();
                    }));
                }
                else
                {
                    options.Add(new MenuOption("Discount Offers", async () =>
                    {
                        await GoToOrderDiscountAsync();
                    }));

                    if (!Config.CalculateOffersAutomatically)
                    {
                        options.Add(new MenuOption("Calculate Offers", async () =>
                        {
                            await CalculateDiscountOffersAsync();
                        }));
                    }
                }

                // Discount (presale) - Xamarin order: Add Discount before Print (presaleMenu.xml)
                if (allowDiscount)
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        await ApplyDiscountAsync();
                    }));
                }

                // Print (presale)
                if (Config.PrinterAvailable)
                {
                    options.Add(new MenuOption("Print", async () =>
                    {
                        await PrintAsync();
                    }));
                }

                // Delete (presale) - Xamarin shows "Delete" (deleteLabel)
                options.Add(new MenuOption("Delete", async () =>
                {
                    await DeleteOrderAsync();
                }));

                // Share (presale) - Xamarin: visible when !isSplitClient (presaleMenu.xml order after Delete)
                if (!isSplitClient)
                {
                    options.Add(new MenuOption("Share", async () =>
                    {
                        await SharePdfAsync();
                    }));
                }

                // Ship Via (presale) - Config.ShowShipVia
                if (Config.ShowShipVia)
                {
                    options.Add(new MenuOption("Ship Via", async () =>
                    {
                        await AddEditShipViaAsync();
                    }));
                }

                // Convert Quote to Sales Order - Config.CanModifyQuotes && IsQuote && !IsDelivery
                if (Config.CanModifyQuotes && _order.IsQuote && !_order.IsDelivery)
                {
                    options.Add(new MenuOption("Convert to Order", async () =>
                    {
                        await ConvertQuoteToSalesOrderAsync();
                    }));
                }

                // View Captures / Take Picture (presale) - Config.CaptureImages && !(finalized && MustAddImageToFinalized)
                bool isVisible = true;
                if (finalized && Config.MustAddImageToFinalized)
                    isVisible = false;
                if (Config.CaptureImages && isVisible)
                {
                    options.Add(new MenuOption("Take Picture", async () =>
                    {
                        await ViewCapturedImagesAsync();
                    }));
                }
            }
            else
            {
                // Non-presale menu order matches Xamarin previouslyOrderedMenu.xml
                // Print (non-presale)
                if (!Config.LockOrderAfterPrinted)
                {
                    options.Add(new MenuOption("Print", async () =>
                    {
                        await PrintAsync();
                    }));
                }

                // Offers vs Order Discount (non-presale)
                if (!finalized && !locked)
                {
                    if (!hasOrderDiscounts)
                    {
                        options.Add(new MenuOption("Offers", async () =>
                        {
                            await ShowOffersAsync();
                        }));
                    }
                    else
                    {
                        options.Add(new MenuOption("Discount Offers", async () =>
                        {
                            await GoToOrderDiscountAsync();
                        }));

                        if (!Config.CalculateOffersAutomatically)
                        {
                            options.Add(new MenuOption("Calculate Offers", async () =>
                            {
                                await CalculateDiscountOffersAsync();
                            }));
                        }
                    }
                }

                // Add Payment (non-presale) - Xamarin order: after Offers, before Added All
                var hasPayment = InvoicePayment.List.FirstOrDefault(x => x.OrderId != null && x.OrderId.IndexOf(_order.UniqueId) >= 0) != null;
                if (finalized && !hasPayment && !Config.HidePriceInTransaction)
                {
                    options.Add(new MenuOption("Add Payment", async () =>
                    {
                        await GetPaymentAsync();
                    }));
                }

                // What To View (non-presale)
                if (!finalized)
                {
                    options.Add(new MenuOption(_whatToViewInList == WhatToViewInList.All ? "Added/All" : "Just Ordered", async () =>
                    {
                        await WhatToViewClickedAsync();
                    }));
                }

                // Add Comments (non-presale)
                options.Add(new MenuOption("Add Comments", async () =>
                {
                    await ShowCommentsDialogAsync();
                }));

                // Other Charges (non-presale)
                if (!string.IsNullOrEmpty(asset) || Config.AllowOtherCharges)
                {
                    options.Add(new MenuOption("Other Charges", async () =>
                    {
                        await OtherChargesAsync();
                    }));
                }

                // Set PO (non-presale) - Xamarin order: after Other Charges, before Add Discount
                if ((_order.OrderType == OrderType.Bill || Config.SetPO || Config.POIsMandatory) && !finalized)
                {
                    options.Add(new MenuOption(_order.OrderType == OrderType.Bill ? "Set Bill Number" : "Set PO", async () =>
                    {
                        await GetPONumberAsync();
                    }));
                }

                // Add Discount (non-presale)
                if (!finalized && allowDiscount && !locked)
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        await ApplyDiscountAsync();
                    }));
                }

                // Email/Share (non-presale) - Xamarin: both visible when !isSplitClient
                if (!isSplitClient)
                {
                    options.Add(new MenuOption("Send by Email", async () =>
                    {
                        await SendByEmailAsync();
                    }));
                    options.Add(new MenuOption("Share", async () =>
                    {
                        await SharePdfAsync();
                    }));
                }

                // Service Report (non-presale)
                if (!finalized && Config.ShowServiceReport)
                {
                    options.Add(new MenuOption("Service Report", async () =>
                    {
                        await GoToServiceReportAsync();
                    }));
                }

                // View Captures (non-presale)
                bool isVisible = true;
                if (finalized && Config.MustAddImageToFinalized)
                    isVisible = false;
                if (Config.CaptureImages && isVisible)
                {
                    options.Add(new MenuOption("Take Picture", async () =>
                    {
                        await ViewCapturedImagesAsync();
                    }));
                }
            }

            // Crate In/Out
            var prods = Product.Products.Where(x => x.ExtraPropertiesAsString.Contains("caseInOut"));
            var prodIn = prods.FirstOrDefault(x => x.ExtraPropertiesAsString.Contains("caseInOut=1"));
            var prodOut = prods.FirstOrDefault(x => x.ExtraPropertiesAsString.Contains("caseInOut=0"));
            if (Config.MustEnterCaseInOut && (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Credit) && (prodIn != null || prodOut != null))
            {
                options.Add(new MenuOption("Enter Crate In/Out", async () =>
                {
                    await EnterCasesInOutAsync(false);
                }));
            }

            // Use LSP
            if (Config.LspInAllLines)
            {
                options.Add(new MenuOption("Use LSP", async () =>
                {
                    await UseLspInAllLinesAsync();
                }));
            }


            // Select Price Level
            if (Config.AllowSelectPriceLevel)
            {
                options.Add(new MenuOption("Select Price Level", async () =>
                {
                    await SelectPriceLevelAsync();
                }));
            }

            // Take Survey
            if (!string.IsNullOrEmpty(Config.SurveyQuestions))
            {
                options.Add(new MenuOption("Take Survey", async () =>
                {
                    await TakeSurveyAsync();
                }));
            }

            // Disol Client Values
            if (Config.DisolCrap)
            {
                options.Add(new MenuOption("Edit Client Values", async () =>
                {
                    await EnterEditDisolValuesAsync();
                }));
            }

            // Suggested Products
            bool suggestedVisible = false;
            if (!string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier))
            {
                if (SuggestedClientCategory.List.Count > 0 && _order != null && _order.Client != null)
                {
                    var suggestedForThisClient = SuggestedClientCategory.List.FirstOrDefault(x => x.SuggestedClientCategoryClients.Any(y => y.ClientId == _order.Client.ClientId));
                    if (suggestedForThisClient != null && suggestedForThisClient.SuggestedClientCategoryProducts.Count > 0)
                        suggestedVisible = true;
                }
            }
            if (suggestedVisible && !finalized && !locked)
            {
                var title = !string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                    ? $"{Config.ProductCategoryNameIdentifier} Products" 
                    : "Suggested Products";
                options.Add(new MenuOption(title, async () =>
                {
                    await ShowSuggestedProductsAsync();
                }));
            }

            // Delete Weight Lines
            if (Config.DeleteWeightItemsMenu)
            {
                options.Add(new MenuOption("Delete Weight Lines", async () =>
                {
                    await Delete0WeightItemsAsync();
                }));
            }

            // Reset Order
            if (Config.AllowReset)
            {
                var resetValue = UDFHelper.GetSingleUDF("reset", _order.ExtraFields);
                var resetTitle = (!string.IsNullOrEmpty(resetValue) && resetValue == "1") ? "Undo Reset Order" : "Reset";
                options.Add(new MenuOption(resetTitle, async () =>
                {
                    await ResetOrderAsync();
                }));
            }

            // Edit Client (hidden by default in Xamarin, but available)
            // options.Add(new MenuOption("Edit Client", async () => { await EditClientAsync(); }));

            return options;
        }

        [RelayCommand]
        public async Task NavigateToAddItemAsync(PreviouslyOrderedProductViewModel? item)
        {
            if (item == null || _order == null || !CanEdit)
                return;
            var product = item.GetProduct();
            if (product == null)
                return;

            // When order is not AsPresale and adding new item: restrict if no inventory (same as AdvancedCatalogPageViewModel)
            if (item.GetExistingDetail() == null && !_order.AsPresale && !Config.CanGoBelow0)
            {
                var oh = product.GetInventory(_order.AsPresale, false);
                if (oh <= 0)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            // Match Xamarin PreviouslyOrderedTemplateActivity behavior:
            // If line has OrderDetail, pass orderDetail and asCreditItem (from the detail itself)
            // If line doesn't have OrderDetail, pass productId
            var existingDetail = item.GetExistingDetail();
            if (existingDetail != null)
            {
                // Navigate with orderDetail (editing existing detail)
                // Use the detail's IsCredit flag, not the order type
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&orderDetail={existingDetail.OrderDetailId}&asCreditItem={(existingDetail.IsCredit ? 1 : 0)}");
            }
            else
            {
                // Navigate with productId (adding new item)
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={item.ProductId}");
            }
        }

        /// <summary>
        /// Handles scanned barcode data (non-QR codes)
        /// Similar to ScannerActivity.OnDecodeData implementation
        /// </summary>
        public async Task HandleScannedBarcodeAsync(string data)
        {
            if (string.IsNullOrEmpty(data) || _order == null || !CanEdit)
                return;

            // Find product using the same logic as other pages
            var product = ActivityExtensionMethods.GetProduct(_order, data);

            if (product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                return;
            }

            // Check if product is in previously ordered products list
            var previouslyOrderedItem = PreviouslyOrderedProducts.FirstOrDefault(x => x.ProductId == product.ProductId);
            
            if (previouslyOrderedItem != null)
            {
                // Product is in the list, navigate to add item page
                await NavigateToAddItemAsync(previouslyOrderedItem);
            }
            else
            {
                // Product not in previously ordered list, but we can still add it
                // Navigate directly to add item page with the product
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={product.ProductId}");
            }
        }

        /// <summary>
        /// Handles scanned QR code data
        /// Similar to ScannerActivity.OnDecodeDataQR implementation
        /// </summary>
        public async Task HandleScannedQRCodeAsync(BarcodeDecoder decoder)
        {
            if (decoder == null || _order == null || !CanEdit)
                return;

            // QR codes can contain product information directly
            if (decoder.Product != null)
            {
                var product = decoder.Product;
                
                // Check if product is in previously ordered products list
                var previouslyOrderedItem = PreviouslyOrderedProducts.FirstOrDefault(x => x.ProductId == product.ProductId);
                
                if (previouslyOrderedItem != null)
                {
                    // Product is in the list, navigate to add item page
                    await NavigateToAddItemAsync(previouslyOrderedItem);
                }
                else
                {
                    // Product not in previously ordered list, but we can still add it
                    // Navigate directly to add item page with the product
                    await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={product.ProductId}");
                }
            }
            else if (!string.IsNullOrEmpty(decoder.UPC))
            {
                // Try to find product by UPC from QR code
                await HandleScannedBarcodeAsync(decoder.UPC);
            }
            else if (!string.IsNullOrEmpty(decoder.Data))
            {
                // Fallback to using the raw data
                await HandleScannedBarcodeAsync(decoder.Data);
            }
        }

        // Menu action methods
        private async Task GotoBarcodeReaderAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"barcodereader?orderId={_order.OrderId}");
        }

        private async Task ShowOffersAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"offers?orderId={_order.OrderId}");
        }

        private async Task GoToOrderDiscountAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"orderdiscount?orderId={_order.OrderId}");
        }

        private async Task CalculateDiscountOffersAsync()
        {
            if (_order == null)
                return;
            // TODO: Implement CalculateDiscountOffers logic
            await _dialogService.ShowAlertAsync("Calculate Offers functionality is not yet fully implemented.", "Info");
        }

        private async Task ApplyDiscountAsync()
        {
            if (_order == null)
                return;

            if (_order.DiscountAmount > 0)
            {
                await AddEditDiscountAsync(_order.DiscountType);
            }
            else
            {
                _appService.RecordEvent("menu option Discount");
                var choice = await _dialogService.ShowActionSheetAsync("Select Type Discount", "", "Cancel", new[] { "Amount", "Percentage" });
                if (choice == "Amount")
                    await AddEditDiscountAsync(DiscountType.Amount);
                else if (choice == "Percentage")
                    await AddEditDiscountAsync(DiscountType.Percent);
            }
        }

        private async Task AddEditDiscountAsync(DiscountType discountType)
        {
            if (_order == null)
                return;

            var discountAmount = _order.DiscountAmount;
            if (_order.DiscountType == DiscountType.Percent && discountAmount > 0)
                discountAmount *= 100;

            var discountValue = await _dialogService.ShowPromptAsync(
                "Enter Discount",
                $"Enter Discount ({discountType}):",
                "OK",
                "Cancel",
                keyboard: Keyboard.Numeric,
                initialValue: discountAmount > 0 ? discountAmount.ToString() : "");

            if (string.IsNullOrWhiteSpace(discountValue))
                return;

            if (!float.TryParse(discountValue, out float discount))
                return;

            if (!_order.CanAddDiscount(discount, discountType))
            {
                await _dialogService.ShowAlertAsync($"Cannot give more than {Config.MaxDiscountPerOrder}% discount to the order.", "Alert");
                return;
            }

            if (discountType == DiscountType.Percent)
            {
                if (_order.OrderTotalCost() <= 0)
                {
                    await _dialogService.ShowAlertAsync("Discount cannot be bigger than order total.", "Alert");
                    return;
                }
                _order.DiscountAmount = discount / 100;
                _order.DiscountType = DiscountType.Percent;
            }
            else
            {
                if (discount > _order.OrderTotalCost())
                {
                    await _dialogService.ShowAlertAsync("Discount cannot be bigger than order total.", "Alert");
                    return;
                }
                _order.DiscountAmount = discount;
                _order.DiscountType = DiscountType.Amount;
            }

            // Get comment
            var comment = await _dialogService.ShowPromptAsync(
                "Enter Comment",
                "Enter Comment:",
                "OK",
                "Cancel",
                keyboard: Keyboard.Default,
                initialValue: _order.DiscountComment ?? "");

            if (comment != null)
            {
                _order.DiscountComment = comment;
            }

            _order.Save();
            LoadOrderData();
        }

        private async Task GetPONumberAsync()
        {
            if (_order == null)
                return;

            var title = _order.OrderType == OrderType.Bill ? "Set Bill Number" : "Enter PO Number";
            var message = _order.OrderType == OrderType.Bill ? "Set Bill Number:" : "PO Number:";
            
            var po = await _dialogService.ShowPromptAsync(title, message, "OK", "Cancel", 
                keyboard: Keyboard.Default, 
                initialValue: _order.PONumber ?? "");

            if (po != null)
            {
                if (Config.PONumberMaxLength > 0 && po.Length > Config.PONumberMaxLength)
                {
                    await _dialogService.ShowAlertAsync($"The PO Number allows only {Config.PONumberMaxLength} characters.", "Alert");
                    return;
                }

                _order.PONumber = po;
                _order.Save();
                LoadOrderData();
            }
        }

        private async Task SetShipDateAsync()
        {
            if (_order == null)
                return;

            var currentDate = _order.ShipDate == DateTime.MinValue ? DateTime.Today : _order.ShipDate;
            // TODO: Implement date picker dialog
            await _dialogService.ShowAlertAsync("Set Ship Date functionality is not yet fully implemented. Please use a date picker.", "Info");
        }

        private async Task ShowCommentsDialogAsync()
        {
            if (_order == null)
                return;

            var comments = await _dialogService.ShowPromptAsync(
                "Add Comments",
                "Enter comments:",
                "OK",
                "Cancel",
                keyboard: Keyboard.Default,
                initialValue: _order.Comments ?? "");

            if (comments != null)
            {
                _order.Comments = comments;
                _order.Save();
            }
        }

        private async Task WhatToViewClickedAsync()
        {
            _whatToViewInList = _whatToViewInList == WhatToViewInList.All ? WhatToViewInList.Selected : WhatToViewInList.All;
            LoadPreviouslyOrderedProducts();
        }

        private async Task SignAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"ordersignature?ordersId={_order.OrderId}");
        }

        private async Task DeleteOrderAsync()
        {
            if (_order == null)
                return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Are you sure you want to delete this order?",
                "Alert",
                "Yes",
                "No");

            if (!confirmed)
                return;

            if (_order.AsPresale)
                UpdateRoute(false);

            var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
            if (batch != null)
                batch.Delete();

            _order.Delete();

            // Remove state
            Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");

            await Shell.Current.GoToAsync("..");
        }

        private async Task SelectCompanyAsync()
        {
            if (_order == null)
                return;

            // TODO: Implement company selection dialog
            await _dialogService.ShowAlertAsync("Select Company functionality is not yet fully implemented.", "Info");
        }

        private async Task EnterCasesInOutAsync(bool exit)
        {
            if (_order == null)
                return;

            var prods = Product.Products.Where(x => x.ExtraPropertiesAsString.Contains("caseInOut"));
            var prodIn = prods.FirstOrDefault(x => x.ExtraPropertiesAsString.Contains("caseInOut=1"));
            var prodOut = prods.FirstOrDefault(x => x.ExtraPropertiesAsString.Contains("caseInOut=0"));

            if (prodIn == null && prodOut == null)
            {
                if (exit)
                    await Shell.Current.GoToAsync("..");
                return;
            }

            var crateInLine = _order.Details.FirstOrDefault(x => x.Product.ProductId == prodIn?.ProductId);
            var crateOutLine = _order.Details.FirstOrDefault(x => x.Product.ProductId == prodOut?.ProductId);

            // TODO: Implement crate in/out dialog
            await _dialogService.ShowAlertAsync("Enter Crate In/Out functionality is not yet fully implemented.", "Info");
        }

        private async Task UseLspInAllLinesAsync()
        {
            if (_order == null)
                return;
            // TODO: Implement UseLSP logic
            await _dialogService.ShowAlertAsync("Use LSP functionality is not yet fully implemented.", "Info");
        }

        private async Task ViewCapturedImagesAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"viewcapturedimages?orderId={_order.OrderId}");
        }

        private async Task SelectSalesmanAsync()
        {
            if (_order == null)
                return;
            // TODO: Implement salesman selection dialog
            await _dialogService.ShowAlertAsync("Select Driver functionality is not yet fully implemented.", "Info");
        }

        private async Task SelectPriceLevelAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"selectpricelevel?orderId={_order.OrderId}");
        }

        private async Task TakeSurveyAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"survey?orderId={_order.OrderId}");
        }

        private async Task EnterEditDisolValuesAsync()
        {
            if (_order == null)
                return;

            var rtn = UDFHelper.GetSingleUDF("RTN", _order.ExtraFields);
            var realName = UDFHelper.GetSingleUDF("RealName", _order.ExtraFields);

            // TODO: Implement dialog with RTN and RealName fields
            await _dialogService.ShowAlertAsync("Edit Client Values functionality is not yet fully implemented.", "Info");
        }

        private async Task ShowSuggestedProductsAsync()
        {
            if (_order == null)
                return;

            // Navigate to ProductCatalogPage with isShowingSuggested flag
            // Pass comingFrom=PreviouslyOrdered so ProductCatalog knows to add as sales (no credit prompt)
            await Shell.Current.GoToAsync($"productcatalog?orderId={_order.OrderId}&isShowingSuggested=1&comingFrom=PreviouslyOrdered");
        }

        private async Task OtherChargesAsync()
        {
            if (_order == null)
                return;
            // TODO: Implement other charges dialog
            await _dialogService.ShowAlertAsync("Other Charges functionality is not yet fully implemented.", "Info");
        }

        private async Task Delete0WeightItemsAsync()
        {
            if (_order == null)
                return;

            var toDelete = _order.Details.Where(x => x.Product.SoldByWeight && x.Weight == 0).ToList();
            if (toDelete.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No weight items with 0 weight found.", "Info");
                return;
            }

            var confirmed = await _dialogService.ShowConfirmAsync(
                $"Are you sure you want to delete {toDelete.Count} weight item(s) with 0 weight?",
                "Confirm",
                "Yes",
                "No");

            if (!confirmed)
                return;

            foreach (var d in toDelete)
            {
                _order.UpdateInventory(d, 1);
                _order.Details.Remove(d);
            }

            _order.Save();
            LoadPreviouslyOrderedProducts();
            LoadOrderData();
        }

        private async Task ResetOrderAsync()
        {
            if (_order == null)
                return;

            var resetValue = UDFHelper.GetSingleUDF("reset", _order.ExtraFields);

            if (!string.IsNullOrEmpty(resetValue) && resetValue == "1")
            {
                _order.ExtraFields = UDFHelper.RemoveSingleUDF("reset", _order.ExtraFields);
                _order.ExtraFields = UDFHelper.RemoveSingleUDF("resetdate", _order.ExtraFields);
                _order.ExtraFields = UDFHelper.RemoveSingleUDF("delmode", _order.ExtraFields);
                _order.Save();
                await _dialogService.ShowAlertAsync("Order reset has been undone.", "Alert");
                return;
            }

            // TODO: Implement date picker and delivery mode selection
            await _dialogService.ShowAlertAsync("Reset Order functionality is not yet fully implemented.", "Info");
        }

        private async Task GoToServiceReportAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"servicereport?orderId={_order.OrderId}");
        }

        private async Task GetPaymentAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"paymentsetvalues?ordersId={_order.OrderId}&commingFromFinalize=1");
        }

        private async Task AddEditShipViaAsync()
        {
            if (_order == null)
                return;

            var shipVia = UDFHelper.GetSingleUDF("ShipVia", _order.ExtraFields);
            var newShipVia = await _dialogService.ShowPromptAsync(
                "Ship Via",
                "Enter Ship Via:",
                "OK",
                "Cancel",
                keyboard: Keyboard.Default,
                initialValue: shipVia ?? "");

            if (newShipVia != null)
            {
                _order.ExtraFields = UDFHelper.SyncSingleUDF("ShipVia", newShipVia, _order.ExtraFields);
                _order.Save();
            }
        }

        private async Task ConvertQuoteToSalesOrderAsync()
        {
            if (_order == null)
                return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Are you sure you want to convert this quote to a sales order?",
                "Alert",
                "Yes",
                "No");

            if (!confirmed)
                return;

            _order.IsQuote = false;
            _order.Save();
            LoadOrderData();
        }

        private void UpdateRoute(bool close)
        {
            if (!Config.CloseRouteInPresale || _order == null)
                return;

            var stop = RouteEx.Routes.FirstOrDefault(x => 
                x.Date.Date == DateTime.Today && 
                x.Client != null && 
                x.Client.ClientId == _order.Client.ClientId);
            
            if (stop != null)
            {
                if (close)
                    stop.AddOrderToStop(_order.UniqueId);
                else
                    stop.RemoveOrderFromStop(_order.UniqueId);
            }
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

                var choice = await _dialogService.ShowActionSheetAsync("Action Options", "", "Cancel", options);
                
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

        public int ProductId { get; set; }
        /// <summary>Default quantity from last order (history). Used when adding new credit line.</summary>
        public double? DefaultQuantityFromHistory { get; set; }
        public int OrderId { get; set; }
        public int? OrderDetailId { get; set; }

        /// <summary>True when this row is a return/dump (credit) line. Used so we treat it as credit even when order is OrderType.Order.</summary>
        public bool IsCreditLine { get; set; }

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
        private string _uomText = string.Empty;

        /// <summary>Display text "UOM: name" for the detail's unit of measure. Empty when UOM is null (hide label).</summary>
        [ObservableProperty]
        private string _uomDisplayText = string.Empty;

        /// <summary>True when the detail has a UOM so the UOM label is shown above list price.</summary>
        [ObservableProperty]
        private bool _showUomLabel;

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

        /// <summary>Original quantity text for delivery orders (e.g. "Org. Qty=5"). Visible only when order is delivery and product had Ordered > 0.</summary>
        [ObservableProperty]
        private string _orgQtyText = string.Empty;

        [ObservableProperty]
        private bool _showOrgQty;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private bool _isSuggested = false;

        [ObservableProperty]
        private string _suggestedLabelText = string.Empty;

        public PreviouslyOrderedProductViewModel(PreviouslyOrderedTemplatePageViewModel parent)
        {
            _parent = parent;
        }

        /// <summary>Resolve Product from parent by ProductId. Avoids holding full Product in list items.</summary>
        internal Product? GetProduct() => _parent.GetProductById(ProductId);

        /// <summary>Resolve OrderDetail from parent by OrderDetailId. Avoids holding full OrderDetail in list items.</summary>
        internal OrderDetail? GetExistingDetail() => _parent.GetOrderDetailById(OrderDetailId);

        partial void OnQuantityChanged(double value)
        {
            QuantityButtonText = value > 0 ? value.ToString("F0") : "+";
            UpdateTotal();
        }

        public void UpdateFromOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail == null)
                return;

            OrderDetailId = orderDetail.OrderDetailId;

            // Update price and UoM from order detail
            PriceText = $"Price: {orderDetail.Price.ToCustomString()}";
            UomText = orderDetail.UnitOfMeasure != null ? orderDetail.UnitOfMeasure.Name : string.Empty;
            UomDisplayText = orderDetail.UnitOfMeasure != null ? "UOM: " + orderDetail.UnitOfMeasure.Name : string.Empty;
            ShowUomLabel = orderDetail.UnitOfMeasure != null;

            // Note: Quantity is set in the sync loop, but we ensure it's correct here too
            // This ensures the quantity is always from the actual order detail
            if (Math.Abs(Quantity - (double)orderDetail.Qty) > 0.001) // Only update if different (avoid unnecessary property change)
            {
                Quantity = (double)orderDetail.Qty; // Convert float to double, use ACTUAL order qty
            }

            // Refresh OH for this detail's UOM (inventory in base / UOM conversion so it reflects changes as qty is modified)
            var product = GetProduct();
            if (_parent._order != null && product != null)
            {
                var baseInv = product.GetInventory(_parent._order.AsPresale, false);
                var conversion = orderDetail.UnitOfMeasure?.Conversion ?? 1f;
                var onHandInUom = conversion != 0 ? baseInv / conversion : baseInv;
                OnHandText = $"OH: {onHandInUom:F0}";
            }

            UpdateOrgQtyFromOrder();
            
            // Update total - this must use the order detail's price and quantity
            UpdateTotal();
        }

        /// <summary>Sets OrgQtyText and ShowOrgQty for delivery orders when product had original ordered qty > 0 (matches Xamarin PreviouslyOrderedTemplateActivity).</summary>
        internal void UpdateOrgQtyFromOrder()
        {
            var product = GetProduct();
            var existingDetail = GetExistingDetail();
            if (_parent._order == null || product == null || !_parent._order.IsDelivery || existingDetail == null || existingDetail.Ordered <= 0)
            {
                ShowOrgQty = false;
                OrgQtyText = string.Empty;
                return;
            }
            ShowOrgQty = true;
            if (product.SoldByWeight && Config.NewAddItemRandomWeight)
            {
                var deletedCount = _parent._order.DeletedDetails?.Count(x => x.Product?.ProductId == ProductId) ?? 0;
                OrgQtyText = $"Org. Qty={(float)(existingDetail.Ordered + deletedCount)}";
            }
            else
            {
                OrgQtyText = $"Org. Qty={existingDetail.Ordered}";
            }
        }

        public void UpdateTotal()
        {
            var existingDetail = GetExistingDetail();
            var product = GetProduct();
            // Always use ExistingDetail.Price if available (from order detail)
            // This ensures the total reflects the actual order detail price
            if (existingDetail != null)
            {
                var total = Quantity * existingDetail.Price;
                TotalText = $"Total: {total.ToCustomString()}";
            }
            else if (Quantity > 0 && product != null && _parent._order != null)
            {
                // If no order detail but quantity > 0, use expected price
                var price = Product.GetPriceForProduct(product, _parent._order, false, false);
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
            if (item == null || item.OrderId == 0 || _parent._order == null)
                return;
            var product = item.GetProduct();
            if (product == null)
                return;

            var order = Order.Orders.FirstOrDefault(x => x.OrderId == item.OrderId);
            if (order == null)
                return;

            // Editing existing return/dump line: use item type (IsCreditLine) so we handle it even when order is OrderType.Order
            var credit_existingDetail = item.GetExistingDetail();
            if (item.IsCreditLine && credit_existingDetail != null)
                {
                    var creditDetail = credit_existingDetail;
                    var result_credit = await _parent._dialogService.ShowRestOfTheAddDialogAsync(
                        product,
                        order,
                        creditDetail,
                        isCredit: true,
                        isDamaged: creditDetail.Damaged,
                        isDelivery: order.IsDelivery);

                    if (result_credit.Cancelled)
                        return;

                    if (result_credit.Qty == 0)
                    {
                        order.DeleteDetail(creditDetail);
                        order.Save();
                        _parent.LoadOrderData();
                        _parent.RefreshProductList();
                        return;
                    }

                    creditDetail.Qty = result_credit.Qty;
                    creditDetail.Weight = result_credit.Weight;
                    creditDetail.Lot = result_credit.Lot;
                    if (result_credit.LotExpiration.HasValue)
                        creditDetail.LotExpiration = result_credit.LotExpiration.Value;
                    creditDetail.Comments = result_credit.Comments;
                    creditDetail.Price = result_credit.Price;
                    creditDetail.UnitOfMeasure = result_credit.SelectedUoM;
                    creditDetail.IsFreeItem = result_credit.IsFreeItem;
                    creditDetail.ReasonId = result_credit.ReasonId;
                    if (result_credit.PriceLevelSelected > 0)
                    {
                        creditDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result_credit.PriceLevelSelected.ToString(), creditDetail.ExtraFields);
                    }
                    OrderDetailMergeHelper.TryMergeDuplicateDetail(order, creditDetail);
                    OrderDetail.UpdateRelated(creditDetail, order);
                    order.RecalculateDiscounts();
                    order.Save();
                    _parent.LoadOrderData();
                    _parent.RefreshProductList();
                    return;
                }

            // Add/new credit: when order is Credit or Return type, show type picker then dialog/prompt
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                await _parent.SelectCreditTypeAndAddAsync(item);
                return;
            }

            // Sales path: use the row's specific order detail so editing "Dozen" edits Dozen, not "Each"
            OrderDetail? existingDetail = item.GetExistingDetail();
            existingDetail = existingDetail != null && !existingDetail.IsCredit ? existingDetail : null;
            var result = await _parent._dialogService.ShowRestOfTheAddDialogAsync(
                product,
                order,
                existingDetail,
                isCredit: false,
                isDamaged: false,
                isDelivery: order.IsDelivery);

            if (result.Cancelled)
                return;

            // Handle qty == 0 - always delete the detail
            if (result.Qty == 0)
            {
                if (existingDetail != null)
                {
                    order.DeleteDetail(existingDetail);
                    order.Save();
                }
                _parent.LoadOrderData();
                _parent.RefreshProductList();
                return;
            }

            // When order is not AsPresale: restrict addition based on inventory and settings (same as AdvancedCatalogPageViewModel)
            if (!order.AsPresale && !Config.CanGoBelow0)
            {
                var currentOH = product.GetInventory(order.AsPresale, false);
                var resultBaseQty = (double)result.Qty;
                if (result.SelectedUoM != null)
                    resultBaseQty *= result.SelectedUoM.Conversion;
                var totalBaseQtyInOrder = order.Details
                    .Where(d => d.Product.ProductId == item.ProductId && !d.IsCredit && d != existingDetail)
                    .Sum(d =>
                    {
                        var q = (double)d.Qty;
                        if (d.UnitOfMeasure != null)
                            q *= d.UnitOfMeasure.Conversion;
                        return q;
                    });
                if (totalBaseQtyInOrder + resultBaseQty > currentOH)
                {
                    await _parent._dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            // qty > 0 - normal flow (matches DoTheThing1 logic)
            OrderDetail? updatedDetail = null;
            if (existingDetail != null)
            {
                // Add back current qty to inventory before changing, then subtract new qty (so OH display updates correctly)
                order.UpdateInventory(existingDetail, 1);
                // Update existing detail
                existingDetail.Qty = result.Qty;
                existingDetail.Weight = result.Weight;
                existingDetail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    existingDetail.LotExpiration = result.LotExpiration.Value;
                existingDetail.Comments = result.Comments;
                existingDetail.Price = result.Price;
                existingDetail.UnitOfMeasure = result.SelectedUoM;
                existingDetail.IsFreeItem = result.IsFreeItem;
                existingDetail.ReasonId = result.ReasonId;
                if (result.PriceLevelSelected > 0)
                {
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                }
                order.UpdateInventory(existingDetail, -1);
                updatedDetail = existingDetail;
            }
            else
            {
                // Create new detail
                var detail = new OrderDetail(product, 0, order);
                double expectedPrice = Product.GetPriceForProduct(product, order, false, false);
                double price = result.Price;
                
                // If UseLastSoldPrice, get from last invoice detail (from client history)
                if (result.UseLastSoldPrice && order.Client != null)
                {
                    var clientHistory = InvoiceDetail.ClientProduct(order.Client.ClientId, item.ProductId);
                    if (clientHistory != null && clientHistory.Count > 0)
                    {
                        var lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
                        if (lastInvoiceDetail != null)
                            price = lastInvoiceDetail.Price;
                    }
                }
                else if (price == 0)
                {
                    // Get price from offers or default
                    double offerPrice = 0;
                    if (Offer.ProductHasSpecialPriceForClient(product, order.Client, out offerPrice))
                    {
                        detail.Price = offerPrice;
                        detail.FromOfferPrice = true;
                    }
                    else
                    {
                        detail.Price = expectedPrice;
                        detail.FromOfferPrice = false;
                    }
                }
                else
                {
                    detail.Price = price;
                    detail.FromOfferPrice = false;
                }

                detail.ExpectedPrice = expectedPrice;
                detail.UnitOfMeasure = result.SelectedUoM ?? product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = result.Qty;
                detail.Weight = result.Weight;
                detail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    detail.LotExpiration = result.LotExpiration.Value;
                detail.Comments = result.Comments;
                detail.IsFreeItem = result.IsFreeItem;
                detail.ReasonId = result.ReasonId;
                if (result.PriceLevelSelected > 0)
                {
                    detail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), detail.ExtraFields);
                }
                detail.CalculateOfferDetail();
                order.AddDetail(detail);
                order.UpdateInventory(detail, -1);
                updatedDetail = detail;
            }

            if (updatedDetail != null)
            {
                OrderDetailMergeHelper.TryMergeDuplicateDetail(order, updatedDetail);
                OrderDetail.UpdateRelated(updatedDetail, order);
                order.RecalculateDiscounts();
            }

            order.Save();
            _parent.LoadOrderData();
            _parent.RefreshProductList();
        }
    }

    public partial class PreviouslyOrderedTemplatePageViewModel
    {
        public async Task SelectCreditTypeAndAddAsync(PreviouslyOrderedProductViewModel item)
        {
            if (_order == null || item?.GetProduct() == null)
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
            var selected = await _dialogService.ShowActionSheetAsync("Type of Credit Item", "", "Cancel", options);
            
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
            if (_order == null)
                return;
            var product = item.GetProduct();
            if (product == null)
                return;

            // Use the row's specific order detail so editing one credit line (e.g. Dozen Return) edits that one, not another
            var itemExisting = item.GetExistingDetail();
            OrderDetail? existingDetail = itemExisting != null && itemExisting.IsCredit && itemExisting.Damaged == damaged
                ? itemExisting
                : _order.Details.FirstOrDefault(x => x.Product.ProductId == item.ProductId && x.IsCredit && x.Damaged == damaged);

            // When editing an existing credit line, use RestOfTheAddDialog so popup is pre-filled with detail
            if (existingDetail != null)
            {
                var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                    product,
                    _order,
                    existingDetail,
                    isCredit: true,
                    isDamaged: damaged,
                    isDelivery: _order.IsDelivery);

                if (result.Cancelled)
                    return;

                if (result.Qty == 0)
                {
                    _order.DeleteDetail(existingDetail);
                    _order.Save();
                    LoadOrderData();
                    RefreshProductList();
                    return;
                }

                existingDetail.Qty = result.Qty;
                existingDetail.Weight = result.Weight;
                existingDetail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    existingDetail.LotExpiration = result.LotExpiration.Value;
                existingDetail.Comments = result.Comments;
                existingDetail.Price = result.Price;
                existingDetail.UnitOfMeasure = result.SelectedUoM;
                existingDetail.IsFreeItem = result.IsFreeItem;
                existingDetail.ReasonId = result.ReasonId;
                if (result.PriceLevelSelected > 0)
                {
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                }
                OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, existingDetail);
                OrderDetail.UpdateRelated(existingDetail, _order);
                _order.RecalculateDiscounts();
                _order.Save();
                LoadOrderData();
                RefreshProductList();
                return;
            }

            // New credit line: show quantity prompt then create detail
            var defaultQty = item.Quantity > 0 ? item.Quantity.ToString() : (item.DefaultQuantityFromHistory ?? 1).ToString();
            var qtyInput = await _dialogService.ShowPromptAsync(
                $"Enter Quantity for {item.ProductName}",
                "Quantity",
                "OK",
                "Cancel",
                defaultQty,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(qtyInput) || !double.TryParse(qtyInput, out var qty) || qty < 0)
                return;

            // Handle qty == 0 - nothing to add
            if (qty == 0)
            {
                LoadOrderData();
                RefreshProductList();
                return;
            }

            // Create new credit detail
            var detail = new OrderDetail(product, 0, _order);
            detail.IsCredit = true;
            detail.Damaged = damaged;
            detail.ReasonId = reasonId;
            double expectedPrice = Product.GetPriceForProduct(product, _order, true, false);
            double price = 0;
            if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
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
            detail.UnitOfMeasure = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
            detail.Qty = (float)qty;
            detail.CalculateOfferDetail();
            _order.AddDetail(detail);

            OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, detail);
            OrderDetail.UpdateRelated(detail, _order);
            _order.RecalculateDiscounts();
            _order.Save();
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

        private async Task SharePdfAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to share.", "Alert", "OK");
                return;
            }

            try
            {
                await _dialogService.ShowLoadingAsync("Generating PDF...");

                string pdfFile;
                if (Config.ShowBillOfLadingPdf)
                {
                    // Show dialog to select document type
                    var choice = await _dialogService.ShowActionSheetAsync("Select Type Document", "", "Cancel", new[] { "Invoice", "Bill Of Lading" });
                    if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                    {
                        await _dialogService.HideLoadingAsync();
                        return;
                    }

                    pdfFile = choice == "Invoice" 
                        ? PdfHelper.GetOrderPdf(_order) 
                        : PdfHelper.GetOrderPdfBillOfLadin(_order);
                }
                else
                {
                    pdfFile = PdfHelper.GetOrderPdf(_order);
                }

                await _dialogService.HideLoadingAsync();

                if (string.IsNullOrEmpty(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("PDF could not be opened.", "Alert", "OK");
                    return;
                }

                await SharePdfFileAsync(pdfFile);
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sharing PDF.", "Alert", "OK");
            }
        }

        private async Task SharePdfFileAsync(string pdfFile)
        {
            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Order as Pdf",
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

