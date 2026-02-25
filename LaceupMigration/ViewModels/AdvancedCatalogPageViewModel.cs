using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using LaceupMigration.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using LaceupMigration;
using System.Threading;

namespace LaceupMigration.ViewModels
{
    public partial class AdvancedCatalogPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly IScannerService _scannerService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private Order? _order;
        private Category? _category;
        private bool _initialized;
        private int _itemType = 0; // 0=Sales, 1=Dump, 2=Return
        private string _searchCriteria = string.Empty;
        private string _searchCriteriaFromCategories = string.Empty;
        private int _currentFilter = 0;
        private SortCriteria _selectedCriteria = SortCriteria.ProductName;
        private bool _isScanning = false;
        private int _lastScannedProductId = 0;
        private List<int> _relatedLines = new();
        private bool inventoryUpdated = false;

        /// <summary>When true, next OnAppearingAsync skips refresh to preserve scroll position (returning from ProductDetails or ViewImage).</summary>
        private bool _skipNextOnAppearingRefresh;

        // WhatToViewInList enum and field (matches AdvancedTemplateActivity)
        private enum WhatToViewInList
        {
            All,
            Selected
        }
        
        private WhatToViewInList _whatToViewInList = WhatToViewInList.All;
        
        // Performance optimization: caching
        private List<AdvancedCatalogItemViewModel>? _cachedItems = null;
        private int? _cachedItemType = null;
        private int? _cachedCategoryId = null;
        private string? _cachedSearchFromCategories = null;
        private Dictionary<Product, List<InvoiceDetail>>? _cachedClientHistory = null;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceMs = 300;
        private string? _comingFrom; // e.g. "LoadOrderTemplate" — when set, pop to return to NewLoadOrderTemplatePage
        private int _loadOrderReturnDepth = 2; // When comingFrom=LoadOrderTemplate: 1=direct from template, 2=via fullcategory
        private int? _lastAppliedOrderId; // Dedupe: avoid re-initializing when ApplyQueryAttributes is called multiple times (Shell/restoration)
        private int? _lastAppliedCategoryId;

        public ObservableCollection<AdvancedCatalogItemViewModel> Items { get; } = new();
        public ObservableCollection<AdvancedCatalogItemViewModel> FilteredItems { get; } = new();
        public ObservableCollection<AdvancedCatalogItemViewModel.AdvancedCatalogLineItemViewModel> LineItems { get; } = new();

        /// <summary>Set when a scan matches an item; view scrolls to it. Match TransferOnOffPage.ScannedLineToFocus.</summary>
        [ObservableProperty] private AdvancedCatalogItemViewModel? _scannedItemToFocus;

        /// <summary>Moves highlight to the given item (light blue background) without scrolling. Use when user manually adds/decrements/edits a line.</summary>
        private void HighlightItemOnly(AdvancedCatalogItemViewModel? item)
        {
            if (item == null) return;
            foreach (var i in FilteredItems)
                i.IsHighlightedFromScan = false;
            item.IsHighlightedFromScan = true;
        }

        [ObservableProperty] private string _searchQuery = string.Empty;

        [ObservableProperty] private string _clientName = string.Empty;

        [ObservableProperty] private string _orderTypeText = string.Empty;

        [ObservableProperty] private string _linesText = "Lines: 0";

        [ObservableProperty] private bool _canEdit = true;

        [ObservableProperty] private string _selectedItemTypeText = "Sales";

        [ObservableProperty] private bool _showSendButton = true;

        [ObservableProperty] private bool _showSalesButton = true;
        partial void OnShowSalesButtonChanged(bool value) => UpdateButtonGridColumns();

        [ObservableProperty] private bool _showDumpsButton = true;
        partial void OnShowDumpsButtonChanged(bool value) => UpdateButtonGridColumns();

        [ObservableProperty] private bool _showReturnsButton = true;
        partial void OnShowReturnsButtonChanged(bool value) => UpdateButtonGridColumns();

        [ObservableProperty] private bool _showButtonGrid = true;

        [ObservableProperty] private bool _showOrderSummary = true;

        [ObservableProperty] private bool _isFromLoadOrder;

        [ObservableProperty] private string _buttonGridColumns = "*,*,*";

        [ObservableProperty] private int _salesButtonColumn = 0;

        [ObservableProperty] private int _dumpsButtonColumn = 1;

        [ObservableProperty] private int _returnsButtonColumn = 2;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _salesButtonColor = Microsoft.Maui.Graphics.Colors.Transparent;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _dumpsButtonColor = Microsoft.Maui.Graphics.Colors.Transparent;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _returnsButtonColor = Microsoft.Maui.Graphics.Colors.Transparent;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _salesButtonTextColor = Microsoft.Maui.Graphics.Colors.Black;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _dumpsButtonTextColor = Microsoft.Maui.Graphics.Colors.Black;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _returnsButtonTextColor = Microsoft.Maui.Graphics.Colors.Black;

        [ObservableProperty] private string _itemsTotalText = "Items: 0";

        [ObservableProperty] private bool _isOrderSummaryExpanded = false;

        public bool ShowTotalInHeader => ShowTotals && !IsOrderSummaryExpanded;

        partial void OnIsOrderSummaryExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowTotalInHeader));
        }

        [ObservableProperty] private string _termsText = "Terms: ";
        
        [ObservableProperty] private bool _termsVisible = true;

        [ObservableProperty] private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty] private string _discountText = "Discount: $0.00";

        [ObservableProperty] private string _taxText = "Tax: $0.00";

        [ObservableProperty] private string _totalText = "Total: $0.00";

        [ObservableProperty] private string _qtySoldText = "Qty Sold: 0";

        [ObservableProperty] private string _orderAmountText = "Order: $0.00";

        [ObservableProperty] private string _creditAmountText = "Credit: $0.00";

        [ObservableProperty] private string _companyText = string.Empty;

        [ObservableProperty] private bool _showCompany = false;

        [ObservableProperty] private bool _showTotals = true;

        [ObservableProperty] private bool _showDiscount = true;

        /// <summary>Client's AllowOneDoc. When false (Order type), totals use simple layout without Credit line.</summary>
        [ObservableProperty] private bool _allowOneDoc = true;

        /// <summary>True when totals should use simple 2-row layout: Lines|Subtotal|Tax and Qty Sold|Discount|Total (no Credit).</summary>
        public bool UseSimpleTotalsLayout => _order != null && _order.OrderType == OrderType.Order && !AllowOneDoc;

        public bool ShowOneDocTotalsLayout => !UseSimpleTotalsLayout;

        partial void OnAllowOneDocChanged(bool value)
        {
            OnPropertyChanged(nameof(UseSimpleTotalsLayout));
            OnPropertyChanged(nameof(ShowOneDocTotalsLayout));
        }

        [ObservableProperty] private string _sortByText = "Sort By: Product Name";

        [ObservableProperty] private string _filterText = "Filter";

        /// <summary>Text for the WhatToView button: "Added/All" or "Just Ordered"</summary>
        public string WhatToViewButtonText => _whatToViewInList == WhatToViewInList.All ? "Added/All" : "Just Ordered";

        /// <summary>Visibility for the WhatToView button: always visible for presale, only when not finalized for non-presale</summary>
        public bool ShowWhatToViewButton => _order != null && (_order.AsPresale || !_order.Finished);

        [ObservableProperty] private bool _showPrices = true;
        partial void OnShowPricesChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowPriceAndHistoryInCells));
            OnPropertyChanged(nameof(ShowUomColumnInCells));
        }
        partial void OnIsFromLoadOrderChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowPriceAndHistoryInCells));
            OnPropertyChanged(nameof(ShowUomColumnInCells));
        }

        /// <summary>Show price, history, type, etc. in product cells. False when load order (cells show only OH + Truck Inventory).</summary>
        public bool ShowPriceAndHistoryInCells => ShowPrices && !IsFromLoadOrder;

        /// <summary>Show the right column that contains price (when not load order) and UOM button. True for both normal and load order so UOM works the same.</summary>
        public bool ShowUomColumnInCells => ShowPriceAndHistoryInCells || IsFromLoadOrder;

        public AdvancedCatalogPageViewModel(DialogService dialogService, ILaceupAppService appService,
            IScannerService scannerService, ICameraBarcodeScannerService cameraBarcodeScanner,
            AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
            _advancedOptionsService = advancedOptionsService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            ShowPrices = !Config.HidePriceInTransaction;
            ShowTotals = !Config.HidePriceInTransaction;
        }

        ~AdvancedCatalogPageViewModel()
        {
            _searchDebounceTimer?.Dispose();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            int? categoryId = null;
            string? search = null;
            int? itemType = null;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
            }

            if (query.TryGetValue("categoryId", out var catValue) && catValue != null)
            {
                if (int.TryParse(catValue.ToString(), out var catId))
                    categoryId = catId;
            }

            if (query.TryGetValue("search", out var searchValue) && searchValue != null)
            {
                search = searchValue.ToString();
            }

            if (query.TryGetValue("itemType", out var itemTypeValue) && itemTypeValue != null)
            {
                if (int.TryParse(itemTypeValue.ToString(), out var it))
                    itemType = it;
            }

            if (query.TryGetValue("comingFrom", out var fromValue) && fromValue != null)
            {
                _comingFrom = fromValue.ToString();
                IsFromLoadOrder = _comingFrom == "LoadOrderTemplate";
            }

            if (query.TryGetValue("loadOrderReturnDepth", out var depthValue) && depthValue != null)
            {
                if (int.TryParse(depthValue.ToString(), out var d) && d >= 1)
                    _loadOrderReturnDepth = d;
            }
            else if (_comingFrom == "LoadOrderTemplate")
            {
                _loadOrderReturnDepth = 2; // Default when from FullCategory redirect
            }

            // Dedupe: Shell/activity restoration can call ApplyQueryAttributes multiple times with the same query.
            // Only schedule InitializeAsync once per distinct (orderId, categoryId) to avoid duplicate loads and bad behavior.
            if (orderId == _lastAppliedOrderId && categoryId == _lastAppliedCategoryId)
                return;
            _lastAppliedOrderId = orderId;
            _lastAppliedCategoryId = categoryId;

            MainThread.BeginInvokeOnMainThread(async () =>
                await InitializeAsync(orderId, categoryId, search, itemType));
        }

        /// <summary>Navigate back from catalog. When opened from LoadOrderTemplate, pop loadOrderReturnDepth times to return to NewLoadOrderTemplatePage.</summary>
        private async Task NavigateBackFromCatalogAsync()
        {
            if (_comingFrom == "LoadOrderTemplate")
            {
                for (int i = 0; i < _loadOrderReturnDepth; i++)
                    await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        /// <summary>When from load order: pop one level (back to Categories if came from categories, or to LoadOrderTemplate if came directly).</summary>
        private async Task NavigateBackOneAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>When from load order and user taps "Add To Order": always go back to LoadOrderTemplate (pop loadOrderReturnDepth times).</summary>
        private async Task NavigateToLoadOrderTemplateAsync()
        {
            for (int i = 0; i < _loadOrderReturnDepth; i++)
                await Shell.Current.GoToAsync("..");
        }

        public async Task InitializeAsync(int? orderId, int? categoryId, string? search, int? itemType)
        {
            if (!orderId.HasValue)
            {
                await _dialogService.ShowAlertAsync("Order ID is required.", "Error");
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId.Value);
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("Order not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (categoryId.HasValue)
                _category = Category.Find(categoryId.Value);

            if (!string.IsNullOrEmpty(search))
                _searchCriteriaFromCategories = search;

            if (itemType.HasValue)
                _itemType = itemType.Value;

            _initialized = true;
            IsFromLoadOrder = _comingFrom == "LoadOrderTemplate";
            if (IsFromLoadOrder)
            {
                ShowButtonGrid = false;
                ShowOrderSummary = false;
            }

            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            OrderTypeText = GetOrderTypeText(_order);
            TermsText = "Terms: " + _order.Term;
            TermsVisible = !string.IsNullOrEmpty(_order.Term);
            AllowOneDoc = _order.Client?.AllowOneDoc ?? false;

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

            // Initialize What To View filter: load order always shows All (in order + to be added); else Selected when order has details
            if (IsFromLoadOrder)
            {
                _whatToViewInList = WhatToViewInList.All;
            }
            else if (_order.Details.Count > 0)
            {
                _whatToViewInList = WhatToViewInList.Selected; // Show only ordered items if order has details
            }
            OnPropertyChanged(nameof(WhatToViewButtonText)); // Notify UI that button text changed
            OnPropertyChanged(nameof(ShowWhatToViewButton)); // Notify UI that button visibility changed

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

            // Set button visibility based on OrderType and AllowOneDoc
            // If OrderType is Return or Credit, show Dumps and Returns buttons (hide Sales)
            if (_order.OrderType == OrderType.Return || _order.OrderType == OrderType.Credit)
            {
                ShowSalesButton = false;
                ShowDumpsButton = true;
                ShowReturnsButton = true;
                // Default to Dumps (1) for credit/return orders, but allow switching to Returns (2)
                if (!itemType.HasValue)
                {
                    _itemType = 1; // Default to Dumps for credit/return orders
                }
            }
            else if (_order.OrderType == OrderType.Order)
            {
                // If AllowOneDoc is true, show Sales, Returns, and Dumps buttons
                // If AllowOneDoc is false, don't show any buttons (only can add sales)
                var allowOneDoc = _order.Client?.AllowOneDoc ?? false;
                if (allowOneDoc)
                {
                    ShowSalesButton = true;
                    ShowDumpsButton = true;
                    ShowReturnsButton = true;
                }
                else
                {
                    ShowSalesButton = false;
                    ShowDumpsButton = false;
                    ShowReturnsButton = false;
                }

                // Default to Sales (0) for regular orders
                if (!itemType.HasValue)
                {
                    _itemType = 0;
                }
            }
            else
            {
                // Other order types - default behavior
                ShowSalesButton = true;
                ShowDumpsButton = false;
                ShowReturnsButton = false;
                if (!itemType.HasValue)
                {
                    _itemType = 0;
                }
            }

            UpdateButtonGridColumns();
            UpdateItemTypeText();
            PrepareList();
            RefreshTotals();
            Filter();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            // Skip refresh when returning from ProductDetails or ViewImage so scroll position is preserved
            if (_skipNextOnAppearingRefresh)
            {
                _skipNextOnAppearingRefresh = false;
                return;
            }

            // Xamarin: when creating Activity, if UpdateInventoryInPresale && AsPresale run inventory update then refresh, else just refresh
            if (!inventoryUpdated && _order != null && Config.UpdateInventoryInPresale && _order.AsPresale)
            {
                inventoryUpdated = true;
                await RunPresaleInventoryUpdateAsync();
            }

            await RefreshAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (_order != null)
            {
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
            }

            // Clear cache to force rebuild
            _cachedItems = null;
            _cachedItemType = null;

            PrepareList();
            Filter();
            RefreshTotals();
            LoadLineItems();
            await Task.CompletedTask;
        }

        private void PrepareList()
        {
            if (_order == null)
                return;

            // Check if we can use cached items
            var categoryId = _category?.CategoryId ?? 0;
            if (_cachedItems != null &&
                _cachedItemType == _itemType &&
                _cachedCategoryId == categoryId &&
                _cachedSearchFromCategories == _searchCriteriaFromCategories)
            {
                // Use cached items, just update display properties
                Items.Clear();
                foreach (var item in _cachedItems)
                {
                    // Update display properties in case order details changed
                    UpdateItemFromOrderDetails(item);
                    item.UpdateDisplayProperties();
                    Items.Add(item);
                }

                return;
            }

            var items = new List<AdvancedCatalogItemViewModel>();

            var products = Product.GetProductListForOrder(_order, _itemType > 0, categoryId, true);

            if (!string.IsNullOrEmpty(_searchCriteriaFromCategories))
            {
                var searchLower = _searchCriteriaFromCategories.Replace(" ", "").ToLowerInvariant();
                products = products.Where(x =>
                    x.Name.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                    x.Upc.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                    x.Sku.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                    x.Description.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                    x.Code.ToLowerInvariant().Replace(" ", "").Contains(searchLower)
                ).ToList();
            }

            // Cache client history (needed for inventory filter to show products with history when OH <= 0)
            if (_cachedClientHistory == null)
            {
                if (_order.Client.ClientProductHistory == null)
                    _order.Client.ClientProductHistory = InvoiceDetail.GetProductHistoryDictionary(_order.Client);
                _cachedClientHistory = _order.Client.ClientProductHistory;
            }

            // Filter products based on inventory when !AsPresale and Sales (itemType == 0)
            // For load order (IsFromLoadOrder) show all products; for Dump (1) or Return (2), show all products
            if (!IsFromLoadOrder && !_order.AsPresale && _itemType == 0)
            {
                var productsWithOrderDetails = _order.Details
                    .Where(d => !d.IsCredit && d.Product != null)
                    .Select(d => d.Product.ProductId)
                    .ToHashSet();

                var productsWithHistory = _cachedClientHistory?
                    .Where(kvp => kvp.Key != null && kvp.Value != null && kvp.Value.Count > 0)
                    .Select(kvp => kvp.Key.ProductId)
                    .ToHashSet() ?? new HashSet<int>();

                products = products.Where(x =>
                {
                    if (productsWithOrderDetails.Contains(x.ProductId))
                        return true;

                    if (productsWithHistory.Contains(x.ProductId))
                        return true;

                    var oh = x.GetInventory(_order.AsPresale, false);
                    // Show product if CanGoBelow0 is true OR if inventory is greater than 0
                    return Config.CanGoBelow0 || oh > 0;
                }).ToList();
            }

            var clientSource = _cachedClientHistory;
            if (clientSource == null)
                return;

            foreach (var product in products)
            {
                var item = new AdvancedCatalogItemViewModel
                {
                    ProductId = product.ProductId,
                    ItemType = _itemType,
                    OrderId = _order.OrderId,
                    IsLoadOrderDisplay = _order.OrderType == OrderType.Load,
                    ProductName = product.Name,
                    ProductCode = product.Code,
                    CategoryId  = product.CategoryId,
                    Description = product.Description,
                    SKU = product.Sku ?? "",
                    UPC = product.Upc ?? "",
                    DiscountCategoryId = product.DiscountCategoryId,
                    CurrentWarehouseInventory = product.CurrentWarehouseInventory
                };
                // Access Details to trigger handler setup
                _ = item.Details;


                // Can Change Price
                if (_order != null)
                {
                    item.CanChangePrice = Config.CanChangePrice(_order, product, _itemType > 0);

                    // Show Select PL only if Config.ShowLowestPriceLevel is true AND product has ProductPrice entries
                    if (Config.ShowLowestPriceLevel)
                    {
                        var productPrices = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId);
                        item.ShowSelectPL = productPrices.Any();
                    }
                    else
                    {
                        item.ShowSelectPL = false;
                    }
                }
                else
                {
                    item.CanChangePrice = false;
                    item.ShowSelectPL = false;
                }

                // Use more efficient lookup - try to find by ProductId directly
                var clientSourceKey =
                    clientSource.Keys.FirstOrDefault(x => x != null && x.ProductId == product.ProductId);
                if (clientSourceKey != null && clientSource.TryGetValue(clientSourceKey, out var history))
                {
                    item.History.Clear();
                    // Only take recent history to improve performance
                    item.History.AddRange(history.OrderByDescending(x => x.Date).Take(10));
                }

                // Create details for each UoM or single detail if no UoM
                if (product.UnitOfMeasures.Count == 0)
                {
                    var detail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                    {
                        ProductId = product.ProductId,
                        ExpectedPrice = Product.GetPriceForProduct(product, _order, _itemType > 0, _itemType == 1)
                    };

                    double price = 0;
                    if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
                    {
                        detail.ExpectedPrice = price;
                        detail.Price = price;
                        detail.IsFromEspecial = true;
                    }
                    else
                    {
                        detail.Price = detail.ExpectedPrice;
                    }

                    item.Details.Add(detail);
                }
                else
                {
                    foreach (var uom in product.UnitOfMeasures)
                    {
                        if (product.UseDefaultUOM && _order.OrderType == OrderType.Order && !uom.IsDefault &&
                            _itemType == 0)
                            continue;

                        var detail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                        {
                            ProductId = product.ProductId,
                            ExpectedPrice = Product.GetPriceForProduct(product, _order, _itemType > 0, _itemType == 1),
                            UoM = uom
                        };

                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
                        {
                            detail.ExpectedPrice = price;
                            detail.Price = price;
                            detail.IsFromEspecial = true;
                        }
                        else
                        {
                            detail.Price = detail.ExpectedPrice;
                        }

                        detail.ExpectedPrice *= uom.Conversion;
                        detail.Price *= uom.Conversion;

                        item.Details.Add(detail);
                    }
                }

                // Sync with existing order details - optimize by filtering first
                var relevantDetails = _order.Details
                    .Where(d =>
                    {
                        var it = d.IsCredit ? (d.Damaged ? 1 : 2) : 0;
                        return it == _itemType && d.Product.ProductId == product.ProductId;
                    })
                    .ToList();

                foreach (var orderDetail in relevantDetails)
                {
                    var isFreeItem = orderDetail.IsFreeItem || (!string.IsNullOrEmpty(orderDetail.ExtraFields) &&
                                                   orderDetail.ExtraFields.Contains("productfree"));
                    
                    if (isFreeItem)
                    {
                        // Check if this free item detail already exists in the Details collection by OrderDetailId
                        var existingFreeDetail = item.Details.FirstOrDefault(d => 
                            d.Detail != null && d.Detail.OrderDetailId == orderDetail.OrderDetailId);
                        
                        if (existingFreeDetail != null)
                        {
                            // Update existing free item detail
                            existingFreeDetail.Price = orderDetail.Price;
                            existingFreeDetail.ExpectedPrice = orderDetail.ExpectedPrice;
                        }
                        else
                        {
                            // Check if there's already ANY free item for this product (we should only have one free item per product)
                            var existingFreeItem = item.Details.FirstOrDefault(d => 
                                d.Detail != null && (d.Detail.IsFreeItem || 
                                (!string.IsNullOrEmpty(d.Detail.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"))));
                            
                            if (existingFreeItem == null)
                            {
                                // No free item exists, add this one
                                var d = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                                {
                                    ProductId = product.ProductId,
                                    Price = orderDetail.Price,
                                    ExpectedPrice = orderDetail.ExpectedPrice,
                                    UoM = orderDetail.UnitOfMeasure,
                                    Detail = orderDetail
                                };
                                item.Details.Add(d);
                            }
                            else
                            {
                                // Free item already exists, update it to point to this orderDetail (in case it changed)
                                existingFreeItem.Price = orderDetail.Price;
                                existingFreeItem.ExpectedPrice = orderDetail.ExpectedPrice;
                                existingFreeItem.Detail = orderDetail;
                            }
                        }
                        continue;
                    }

                    // For non-free items, update or create the primary detail
                    // The primary detail should always exist and be updated, not duplicated
                    var detail = item.Details.FirstOrDefault(x => EqualsUom(x.UoM, orderDetail.UnitOfMeasure) && 
                        (x.Detail == null || (!x.Detail.IsFreeItem && (string.IsNullOrEmpty(x.Detail.ExtraFields) || !x.Detail.ExtraFields.Contains("productfree")))));
                    
                    if (detail != null)
                    {
                        // Update existing non-free detail (this should be the primary detail)
                        detail.Price = orderDetail.Price;
                        detail.Detail = orderDetail;
                        detail.ExtraFields = orderDetail.ExtraFields;

                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price,
                                orderDetail.UnitOfMeasure))
                        {
                            detail.IsFromEspecial = detail.Price == price;
                        }
                    }
                    else
                    {
                        // No matching detail found, create a new one for the primary detail
                        // This should only happen if the primary detail doesn't exist yet
                        var newDetail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                        {
                            ProductId = product.ProductId,
                            Price = orderDetail.Price,
                            ExpectedPrice = orderDetail.ExpectedPrice,
                            UoM = orderDetail.UnitOfMeasure,
                            Detail = orderDetail,
                            ExtraFields = orderDetail.ExtraFields
                        };

                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price,
                                orderDetail.UnitOfMeasure))
                        {
                            newDetail.IsFromEspecial = newDetail.Price == price;
                        }

                        item.Details.Add(newDetail);
                    }
                }

                // Main line always uses default UOM only. PrimaryDetail must never point to a subline (non-default UOM or free item).
                item.PrimaryDetail = GetDefaultUomDetail(item);

                // Update display properties
                item.UpdateDisplayProperties();
                
                // Access Details to trigger CollectionChanged handler which will notify property changes
                _ = item.Details;

                // Apply "Just Ordered" filter (matches Xamarin whatToViewInList == WhatToViewInList.Selected)
                if (_whatToViewInList == WhatToViewInList.Selected)
                {
                    // Check if this product has any non-free order details for the current item type
                    var hasOrderDetail = item.Details.Any(d => d.Detail != null && !d.Detail.IsFreeItem && 
                        (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")));
                    
                    if (!hasOrderDetail)
                    {
                        // Skip items not in the current order when showing "Just Ordered"
                        continue;
                    }
                }

                items.Add(item);
            }

            // Sort items (this is fast, can stay on current thread)
            var sortedItems = SortByCriteria(_selectedCriteria, items);

            // Cache the items
            _cachedItems = sortedItems;
            _cachedItemType = _itemType;
            _cachedCategoryId = categoryId;
            _cachedSearchFromCategories = _searchCriteriaFromCategories;

            // Update UI on main thread with batched updates
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Batch clear and add for better performance
                Items.Clear();
                // Add items in batch to reduce UI updates
                var itemsToAdd = sortedItems.ToList();
                foreach (var item in itemsToAdd)
                {
                    Items.Add(item);
                }
            });

            _relatedLines = _order.Details.Where(x => x.RelatedOrderDetail > 0).Select(x => x.RelatedOrderDetail)
                .ToList();
        }

        private void UpdateItemFromOrderDetails(AdvancedCatalogItemViewModel item)
        {
            if (_order == null)
                return;

            var toRemove = new List<OrderDetail>(); // main-line default UOM with qty 0: don't persist

            // Sync with existing order details for this item type
            foreach (var orderDetail in _order.Details)
            {
                var it = orderDetail.IsCredit ? (orderDetail.Damaged ? 1 : 2) : 0;
                if (it != _itemType)
                    continue;

                if (orderDetail.Product.ProductId != item.ProductId)
                    continue;

                var isFreeItem = orderDetail.IsFreeItem || (!string.IsNullOrEmpty(orderDetail.ExtraFields) &&
                                                   orderDetail.ExtraFields.Contains("productfree"));

                // Main line (default UOM) with qty 0 should not have an OrderDetail — remove and skip attaching
                if (!isFreeItem && orderDetail.Qty == 0 && EqualsUom(item.DefaultUom, orderDetail.UnitOfMeasure))
                {
                    toRemove.Add(orderDetail);
                    continue;
                }

                if (isFreeItem)
                {
                    // Check if this free item detail already exists in the Details collection by OrderDetailId
                    var existingFreeDetail = item.Details.FirstOrDefault(d => 
                        d.Detail != null && d.Detail.OrderDetailId == orderDetail.OrderDetailId);
                    
                        if (existingFreeDetail != null)
                        {
                            // Update existing free item detail
                            existingFreeDetail.Price = orderDetail.Price;
                            existingFreeDetail.ExpectedPrice = orderDetail.ExpectedPrice;
                        }
                        else
                        {
                            // Check if there's already ANY free item for this product (we should only have one free item per product)
                            var existingFreeItem = item.Details.FirstOrDefault(d => 
                                d.Detail != null && (d.Detail.IsFreeItem || 
                                (!string.IsNullOrEmpty(d.Detail.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"))));
                            
                            if (existingFreeItem == null)
                            {
                                // No free item exists, add this one
                                var d = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                                {
                                    ProductId = item.ProductId,
                                    Price = orderDetail.Price,
                                    ExpectedPrice = orderDetail.ExpectedPrice,
                                    UoM = orderDetail.UnitOfMeasure,
                                    Detail = orderDetail
                                };
                                item.Details.Add(d);
                            }
                            else
                            {
                                // Free item already exists, update it to point to this orderDetail (in case it changed)
                                existingFreeItem.Price = orderDetail.Price;
                                existingFreeItem.ExpectedPrice = orderDetail.ExpectedPrice;
                                existingFreeItem.Detail = orderDetail;
                            }
                        }
                    continue;
                }

                var detail = item.Details.FirstOrDefault(x => EqualsUom(x.UoM, orderDetail.UnitOfMeasure));
                if (detail != null)
                {
                    detail.Price = orderDetail.Price;
                    detail.Detail = orderDetail;
                    detail.ExtraFields = orderDetail.ExtraFields;

                    double price = 0;
                    if (Offer.ProductHasSpecialPriceForClient(Product.Find(item.ProductId), _order.Client, out price,
                            orderDetail.UnitOfMeasure))
                    {
                        detail.IsFromEspecial = detail.Price == price;
                    }
                }
                else
                {
                    // Create a new detail if it doesn't exist (non-free items)
                    var newDetail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                    {
                        ProductId = item.ProductId,
                        Price = orderDetail.Price,
                        ExpectedPrice = orderDetail.ExpectedPrice,
                        UoM = orderDetail.UnitOfMeasure,
                        Detail = orderDetail,
                        ExtraFields = orderDetail.ExtraFields
                    };

                    double price = 0;
                    if (Offer.ProductHasSpecialPriceForClient(Product.Find(item.ProductId), _order.Client, out price,
                            orderDetail.UnitOfMeasure))
                    {
                        newDetail.IsFromEspecial = newDetail.Price == price;
                    }

                    item.Details.Add(newDetail);
                }
            }

            foreach (var d in toRemove)
                _order.Details.Remove(d);

            // Main line always uses default UOM only.
            item.PrimaryDetail = GetDefaultUomDetail(item);
        }

        private bool EqualsUom(UnitOfMeasure? uom1, UnitOfMeasure? uom2)
        {
            if (uom1 == null && uom2 == null)
                return true;
            if (uom1 != null && uom2 != null)
                return uom1.Id == uom2.Id;
            return false;
        }

        /// <summary>Returns the detail that must be the main line: the one with default UOM. Creates an empty default-UOM detail if none exists. PrimaryDetail must always point to this.</summary>
        private AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel GetDefaultUomDetail(AdvancedCatalogItemViewModel item)
        {
            var defaultUom = item.DefaultUom;
            var d = defaultUom != null
                ? item.Details.FirstOrDefault(x => EqualsUom(x.UoM, defaultUom))
                : item.Details.FirstOrDefault(x => x.UoM == null);
            if (d != null)
                return d;
            var prod = Product.Find(item.ProductId);
            var defUom = prod?.UnitOfMeasures?.FirstOrDefault(u => u.IsDefault) ?? prod?.UnitOfMeasures?.FirstOrDefault();
            var empty = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
            {
                ProductId = item.ProductId,
                UoM = defUom,
                ExpectedPrice = Product.GetPriceForProduct(prod, _order, _itemType > 0, _itemType == 1),
                Price = Product.GetPriceForProduct(prod, _order, _itemType > 0, _itemType == 1)
            };
            if (defUom != null)
            {
                empty.ExpectedPrice *= defUom.Conversion;
                empty.Price *= defUom.Conversion;
            }
            item.Details.Add(empty);
            return empty;
        }

        private List<AdvancedCatalogItemViewModel> SortByCriteria(SortCriteria criteria,
            List<AdvancedCatalogItemViewModel> source)
        {
            return criteria switch
            {
                SortCriteria.ProductName => source.OrderBy(x => x.ProductName).ToList(),
                SortCriteria.ProductCode => source.OrderBy(x => x.ProductCode).ToList(),
                SortCriteria.Category => source.OrderBy(x => x.CategoryId).ThenBy(x => x.ProductName).ToList(),
                SortCriteria.Qty => source.OrderByDescending(x => x.Details.Sum(d => d.Detail?.Qty ?? 0)).ToList(),
                SortCriteria.InStock => source.OrderByDescending(x => GetCurrentInventory(Product.Find(x.ProductId))).ToList(),
                SortCriteria.Descending => source.OrderByDescending(x => x.ProductName).ToList(),
                SortCriteria.OrderOfEntry => source.OrderBy(x =>
                    x.Details.FirstOrDefault(d => d.Detail != null)?.Detail?.OrderDetailId ?? int.MaxValue).ToList(),
                SortCriteria.Description => source.OrderBy(x => x.Description).ToList(),
                _ => source.OrderBy(x => x.ProductName).ToList()
            };
        }

        private double GetCurrentInventory(Product product)
        {
            if (_order == null)
                return 0;
            var oh = product.GetInventory(_order.AsPresale, false);
            return Math.Round(oh, Config.Round);
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            var options = new List<string> { "In Stock", "Previously Ordered", "Never Ordered", "In Offer" };
            var selected = await _dialogService.ShowActionSheetAsync("Filter", "", "Cancel", options.ToArray());

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            // Toggle filter
            var filterValue = selected switch
            {
                "In Stock" => 2,
                "Previously Ordered" => 4,
                "Never Ordered" => 8,
                "In Offer" => 16,
                _ => 0
            };

            if ((_currentFilter & filterValue) > 0)
                _currentFilter &= ~filterValue;
            else
                _currentFilter |= filterValue;

            UpdateFilterText();
            Filter();
        }

        private void UpdateFilterText()
        {
            var filters = new List<string>();
            if ((_currentFilter & 2) > 0) filters.Add("In Stock");
            if ((_currentFilter & 4) > 0) filters.Add("Previously Ordered");
            if ((_currentFilter & 8) > 0) filters.Add("Never Ordered");
            if ((_currentFilter & 16) > 0) filters.Add("In Offer");

            FilterText = filters.Count > 0 ? $"Filter: {string.Join(", ", filters)}" : "Filter";
        }

        [RelayCommand]
        private async Task SortAsync()
        {
            var options = new[]
            {
                "Product Name", "Product Code", "Category", "In Stock", "Qty", "Descending", "Order of Entry",
                "Description"
            };
            var selected = await _dialogService.ShowActionSheetAsync("Sort By", "", "Cancel", options);

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            _selectedCriteria = selected switch
            {
                "Product Code" => SortCriteria.ProductCode,
                "Category" => SortCriteria.Category,
                "In Stock" => SortCriteria.InStock,
                "Qty" => SortCriteria.Qty,
                "Descending" => SortCriteria.Descending,
                "Order of Entry" => SortCriteria.OrderOfEntry,
                "Description" => SortCriteria.Description,
                _ => SortCriteria.ProductName
            };

            SortByText = $"Sort: {selected}";

            // Re-sort existing items instead of rebuilding
            if (_cachedItems != null && Items.Count > 0)
            {
                // Sort off UI thread
                Task.Run(() =>
                {
                    var sorted = SortByCriteria(_selectedCriteria, Items.ToList());

                    // Update UI on main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Items.Clear();
                        foreach (var item in sorted)
                        {
                            Items.Add(item);
                        }

                        Filter();
                    });
                });
            }
            else
            {
                PrepareList();
                Filter();
            }
        }

        private void Filter()
        {
            if (_order == null)
                return;

            // Run filtering off UI thread for better performance
            Task.Run(() =>
            {
                // Use IEnumerable to avoid multiple ToList() calls - much more efficient
                IEnumerable<AdvancedCatalogItemViewModel> filtered = Items;

                // Apply filters in sequence without creating intermediate lists
                if (!IsFromLoadOrder && (_currentFilter & 2) > 0) // In Stock (skip when load order - show all)
                    filtered = filtered.Where(x => x.CurrentWarehouseInventory > 0);

                if ((_currentFilter & 4) > 0) // Previously Ordered
                    filtered = filtered.Where(x => x.History.Count > 0);

                if ((_currentFilter & 8) > 0) // Never Ordered
                    filtered = filtered.Where(x => x.History.Count == 0);

                if ((_currentFilter & 16) > 0) // In Offer
                {
                    // Pre-compute offers once
                    var offers = Offer.GetOffersVisibleToClient(_order.Client).ToList();
                    var offerProductIds = offers.Select(x => x.ProductId).ToHashSet();
                    var offerCategoryIds = offers.Select(x => x.Product?.DiscountCategoryId ?? 0).Where(x => x > 0)
                        .ToHashSet();

                    filtered = filtered.Where(x =>
                        offerProductIds.Contains(x.ProductId) ||
                        (x.DiscountCategoryId > 0 && offerCategoryIds.Contains(x.DiscountCategoryId)));
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(_searchCriteria))
                {
                    var searchLower = _searchCriteria.Replace(" ", "").ToLowerInvariant();
                    filtered = filtered.Where(x =>
                        x.ProductName.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.UPC.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.SKU.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.Description.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.ProductCode.ToLowerInvariant().Replace(" ", "").Contains(searchLower)
                    );
                }

                // Only call ToList() once at the end
                var result = filtered.ToList();

                // Update UI on main thread with batched updates
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Batch clear and add for better performance
                    FilteredItems.Clear();
                    // Pre-create list to reduce individual Add() calls overhead
                    var itemsToAdd = result.ToList();
                    foreach (var item in itemsToAdd)
                    {
                        FilteredItems.Add(item);
                    }
                });
            });
        }

        partial void OnSearchQueryChanged(string value)
        {
            _searchCriteria = value?.ToLowerInvariant() ?? string.Empty;

            // Debounce search
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(_ => { MainThread.BeginInvokeOnMainThread(() => { Filter(); }); }, null,
                SearchDebounceMs, Timeout.Infinite);
        }

        /// <summary>Runs presale inventory update in background (matches Xamarin when creating Activity). Shows "Updating Inventory" dialog. Caller refreshes list after.</summary>
        private async Task RunPresaleInventoryUpdateAsync()
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

        private void RefreshTotals()
        {
            if (_order == null)
                return;

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
            
            var creditStr = creditAmount > 0 ? (creditAmount * -1).ToCustomString() : creditAmount.ToCustomString();
            CreditAmountText = $"Credit: {creditStr}";

            SubtotalText = $"Subtotal: {subtotal.ToCustomString()}";
            DiscountText = $"Discount: {discount.ToCustomString()}";
            TaxText = $"Tax: {tax.ToCustomString()}";
            TotalText = $"Total: {total.ToCustomString()}";

            ShowTotals = !Config.HidePriceInTransaction;
            ShowDiscount = _order.Client?.UseDiscount == true || _order.Client?.UseDiscountPerLine == true ||
                           _order.IsDelivery;
        }

        private void LoadLineItems()
        {
            if (_order == null)
                return;

            LineItems.Clear();
            var sortedDetails = SortDetails.SortedDetails(_order.Details).ToList();

            foreach (var detail in sortedDetails)
            {
                var lineItem = CreateLineItemViewModel(detail);
                LineItems.Add(lineItem);
            }
        }

        private AdvancedCatalogItemViewModel.AdvancedCatalogLineItemViewModel CreateLineItemViewModel(OrderDetail detail)
        {
            var qtyText = detail.Product.SoldByWeight && detail.Weight > 0
                ? $"Qty: {detail.Qty} (Weight: {detail.Weight})"
                : $"Qty: {detail.Qty}";

            var priceText = detail.Price > 0
                ? $"Price: {detail.Price.ToCustomString()}"
                : string.Empty;

            var amountText = (detail.Qty * detail.Price).ToCustomString();

            var typeText = string.Empty;
            var typeColor = Microsoft.Maui.Graphics.Colors.Transparent;
            var showType = false;

            if (detail.IsCredit)
            {
                typeText = detail.Damaged ? "Dump" : "Return";
                typeColor = Microsoft.Maui.Graphics.Colors.Orange;
                showType = true;
            }

            return new AdvancedCatalogItemViewModel.AdvancedCatalogLineItemViewModel
            {
                Detail = detail,
                ProductName = detail.Product.Name,
                QtyText = qtyText,
                PriceText = priceText,
                AmountText = amountText,
                TypeText = typeText,
                TypeColor = typeColor,
                ShowType = showType
            };
        }

        [RelayCommand]
        public async Task DoneAsync()
        {
            if (IsFromLoadOrder)
            {
                _order?.Save();
                await NavigateToLoadOrderTemplateAsync(); // Add To Order always goes to load order template
                return;
            }
            var canNavigate = await FinalizeOrderAsync();
            if (canNavigate)
            {
                await NavigateBackFromCatalogAsync();
            }
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order available.", "Error");
                return;
            }

            ScannedItemToFocus = null;
            
            _skipNextOnAppearingRefresh = true;

            try
            {
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult))
                    return;

                // Find product by barcode (check UPC, SKU, Code)
                var product = Product.Products.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.Upc) && p.Upc.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Sku) && p.Sku.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Equals(scanResult, StringComparison.OrdinalIgnoreCase)));

                if (product != null)
                {
                    // From load order: if product is in the current list, add qty 1 and stay; otherwise add and exit to load template
                    if (IsFromLoadOrder)
                    {
                        var catalogItem = FilteredItems.FirstOrDefault(i => i.ProductId == product.ProductId);
                        if (catalogItem != null)
                        {
                            var detail = catalogItem.PrimaryDetail ?? catalogItem.Details.FirstOrDefault();
                            if (detail != null)
                            {
                                await IncrementQuantityAsync(detail);
                                if (detail.Detail != null)
                                    detail.Detail.LoadStarting = -1;
                                _order.Save();
                            }
                            // Highlight the matched line (light blue) and scroll it into focus; match TransferOnOffPage.ScannerDoTheThingAsync
                            foreach (var i in FilteredItems)
                                i.IsHighlightedFromScan = false;
                            catalogItem.IsHighlightedFromScan = true;
                            ScannedItemToFocus = catalogItem;
                            return;
                        }
                        AddSingleProductToLoadOrder(product, null);
                        _order.Save();
                        await NavigateToLoadOrderTemplateAsync();
                        return;
                    }

                    var cItem = FilteredItems.FirstOrDefault(i => i.ProductId == product.ProductId);

                    foreach (var i in FilteredItems)
                        i.IsHighlightedFromScan = false;
                    cItem.IsHighlightedFromScan = true;
                    ScannedItemToFocus = cItem;
                    
                    SearchQuery = scanResult;
                    await Task.Delay(500);
                }
                else
                {
                    SearchQuery = scanResult;
                    await Task.Delay(500);
                    await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error scanning barcode: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            }
        }

        /// <summary>Add one product with qty 1 to load order. Same logic as NewLoadOrderTemplatePageViewModel.ScanSingleProductAsync.</summary>
        private void AddSingleProductToLoadOrder(Product product, UnitOfMeasure uom = null)
        {
            if (product == null || _order == null)
                return;
            if (uom == null && !string.IsNullOrEmpty(product.UoMFamily))
                uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsDefault);
            if (uom == null && product.UnitOfMeasures != null)
                uom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault) ?? product.UnitOfMeasures.FirstOrDefault();

            var detail = _order.Details.FirstOrDefault(x => !x.Deleted && x.Product?.ProductId == product.ProductId && x.UnitOfMeasure == uom);
            if (detail == null)
            {
                detail = new OrderDetail(product, 0, _order)
                {
                    LoadStarting = -1,
                    UnitOfMeasure = uom
                };
                _order.Details.Add(detail);
            }
            detail.Qty += 1;
        }

        private async Task AddItemFromScannerAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel detail, bool excludeOffer)
        {
            if (_order == null)
                return;

            var prod = Product.Find(detail.ProductId);
            
            var currentOH = prod.GetInventory(_order.AsPresale, false);
            var baseQty = 1.0;
            if (detail.UoM != null)
                baseQty *= detail.UoM.Conversion;

            var isCredit = detail.Detail != null ? detail.Detail.IsCredit : false;
            if (!IsFromLoadOrder && !Config.CanGoBelow0 && !isCredit && _itemType == 0)
            {
                if (currentOH - baseQty < 0)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            if (Config.AskOffersBeforeAdding && !excludeOffer)
            {
                if (detail.Detail == null || (detail.Detail != null && !detail.Detail.AlreadyAskedForOffers))
                {
                    var hasOffer = _order.ProductHasOffer(prod, detail.UoM);
                    if (hasOffer)
                    {
                        var applyOffer = await _dialogService.ShowConfirmationAsync("Alert",
                            "This product has an offer, would you like to apply it?", "Yes", "No");
                        if (applyOffer)
                        {
                            await AddItemFromScannerAsync(detail, false);
                        }
                        else
                        {
                            await AddItemFromScannerAsync(detail, true);
                        }

                        return;
                    }
                }
            }

            if (detail.Detail == null)
            {
                detail.Detail = new OrderDetail(prod, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }

            _order.UpdateInventory(detail.Detail, 1);
            detail.Detail.Qty++;
            _order.UpdateInventory(detail.Detail, -1);

            if (!OrderDiscount.HasDiscounts && detail.Detail != null)
            {
                if (detail.Detail.CalculateOfferDetail())
                    detail.Price = detail.Detail.Price;
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();

            var item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
            if (item != null)
            {
                item.PrimaryDetail = GetDefaultUomDetail(item);
                item.UpdateDisplayProperties();
            }

            // Don't call Filter() - it resets scroll position. The item is already in the filtered list.
        }

        [RelayCommand]
        private async Task ItemDetailSelectedAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || _order == null)
                return;

            // Show quantity edit dialog or increment
            await IncrementQuantityAsync(detail);
        }

        [RelayCommand]
        private async Task IncrementQuantityAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || _order == null)
            {
                return;
            }

            var prod = Product.Find(detail.ProductId);

            var isFreeItem = detail.Detail != null && (detail.Detail.IsFreeItem || 
                             (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree")));

            // Check inventory before incrementing for Sales (!AsPresale and itemType == 0); skip when load order
            var isCredit = detail.Detail != null ? detail.Detail.IsCredit : _itemType > 0;
            if (!IsFromLoadOrder && !Config.CanGoBelow0 && !isCredit && _itemType == 0 && !_order.AsPresale)
            {
                var currentOH = prod.GetInventory(_order.AsPresale, false);
                var incrementBaseQty = 1.0;
                if (detail.UoM != null)
                    incrementBaseQty *= detail.UoM.Conversion;
                
                // Check if adding this quantity would exceed available inventory
                if (incrementBaseQty > currentOH)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }
            
            if (detail.Detail == null)
            {
                detail.Detail = new OrderDetail(prod, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }

            _order.UpdateInventory(detail.Detail, 1);
            detail.Detail.Qty++;
            _order.UpdateInventory(detail.Detail, -1);

            if (!OrderDiscount.HasDiscounts && detail.Detail != null)
            {
                if (detail.Detail.CalculateOfferDetail())
                    detail.Price = detail.Detail.Price;
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();
            LoadLineItems(); // Update line items

            // Update the item's display properties
            // Find item by matching OrderDetailId (not by reference, since detail might be from different instance)
            var orderDetailId = detail.Detail?.OrderDetailId ?? 0;
            var item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
            
            if (item != null)
            {
                var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                if (detailInItem != null)
                {
                    detailInItem.UpdateQuantityDisplay();
                    item.NotifySublinesChanged();
                }
                item.PrimaryDetail = GetDefaultUomDetail(item);
                item.UpdateDisplayProperties();
            }
            HighlightItemOnly(item);
        }

        [RelayCommand]
        private async Task DecrementQuantityAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || detail.Detail == null || _order == null)
                return;
            var orderDetailId = detail.Detail.OrderDetailId;
            var item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
            var itemToHighlight = item;

            if (_relatedLines.Any(x => x == detail.Detail.OrderDetailId) && !Config.AllowEditRelated)
                return;

            var isFreeItem = detail.Detail.IsFreeItem || 
                            (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree"));
            var isDefaultUomLine = !isFreeItem && item != null && EqualsUom(detail.UoM, item.DefaultUom);

            // Main line cannot go below 0
            if (isDefaultUomLine && detail.Detail.Qty <= 0)
                return;

            _order.UpdateInventory(detail.Detail, 1);
            detail.Detail.Qty--;
            
            if (detail.Detail.Qty <= 0)
            {
                if (item == null) item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
                // Main line at 0: remove OrderDetail (no OrderDetail with qty 0); keep the main-line slot (detail VM with Detail == null)
                if (isDefaultUomLine)
                {
                    _order.Details.Remove(detail.Detail);
                    var detailInItem = item?.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailInItem != null)
                    {
                        detailInItem.Detail = null;
                        detailInItem.UpdateQuantityDisplay();
                        if (item != null) item.NotifySublinesChanged();
                    }
                    if (item != null)
                    {
                        item.PrimaryDetail = GetDefaultUomDetail(item);
                        item.UpdateDisplayProperties();
                    }
                    if (Config.Simone) _order.SimoneCalculateDiscount(); else _order.RecalculateDiscounts();
                    _order.Save();
                    RefreshTotals();
                    LoadLineItems();
                    HighlightItemOnly(item);
                    
                    if(_whatToViewInList == WhatToViewInList.Selected && item != null)
                        FilteredItems.Remove(item);
                    return;
                }

                _order.Details.Remove(detail.Detail);
                
                if (item == null) item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
                if (item != null)
                {
                    // Find and remove the detail from the collection (subline only; main line stays)
                    var detailToRemove = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailToRemove != null)
                    {
                        item.Details.Remove(detailToRemove);
                    }
                    detail.Detail = null;
                    
                    item.PrimaryDetail = GetDefaultUomDetail(item);
                    item.NotifySublinesChanged();
                    item.UpdateDisplayProperties();
                    LoadLineItems(); // Update line items list
                }
                else
                {
                    detail.Detail = null;
                }
            }
            else
            {
                _order.UpdateInventory(detail.Detail, -1);
                
                if (item != null)
                {
                    // Find the detail in the item's Details collection by OrderDetailId
                    var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailInItem != null)
                    {
                        detailInItem.UpdateQuantityDisplay();
                        item.NotifySublinesChanged();
                    }
                    item.PrimaryDetail = GetDefaultUomDetail(item);
                    item.UpdateDisplayProperties();
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();
            HighlightItemOnly(itemToHighlight);
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditQuantityAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || _order == null || !CanEdit)
                return;
            var orderDetailId = detail.Detail?.OrderDetailId ?? 0;
            var itemToHighlight = orderDetailId != 0
                ? Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId))
                : Items.FirstOrDefault(i => i.Details.Any(d => d == detail));

            var currentQty = detail.Detail?.Qty ?? 0;
            var qtyText = await _dialogService.ShowPromptAsync("Enter Quantity", "Quantity", "OK", "Cancel", "", -1,
                currentQty.ToString());

            if (string.IsNullOrEmpty(qtyText) || !float.TryParse(qtyText, out var qty))
                return;

            if (qty < 0)
                qty = 0;
            var isFreeItem = detail.Detail != null && (detail.Detail.IsFreeItem || 
                            (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree")));

            var prod = Product.Find(detail.ProductId);
            
            // Check inventory before setting quantity for Sales (!AsPresale and itemType == 0); skip when load order
            var isCredit = detail.Detail != null ? detail.Detail.IsCredit : _itemType > 0;
            if (!IsFromLoadOrder && !Config.CanGoBelow0 && !isCredit && _itemType == 0 && !_order.AsPresale && qty > 0)
            {
                var currentOH = prod.GetInventory(_order.AsPresale, false);
                var baseQty = (double)qty;
                if (detail.UoM != null)
                    baseQty *= detail.UoM.Conversion;

                double totalBaseQtyInOrder = 0;
                if (detail.Detail != null)
                {
                    // Calculate total base quantity already in order for this product (excluding current detail)
                    totalBaseQtyInOrder = _order.Details
                        .Where(d => d.Product.ProductId == detail.ProductId && 
                                    !d.IsCredit && 
                                    d.OrderDetailId != detail.Detail.OrderDetailId)
                        .Sum(d => 
                        {
                            var q = d.Qty;
                            if (d.UnitOfMeasure != null)
                                q *= (float)d.UnitOfMeasure.Conversion;
                            return q;
                        });
                }
                // Check if setting this quantity would exceed available inventory
                if (totalBaseQtyInOrder + baseQty > currentOH)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }
            
            if (detail.Detail == null)
            {
                if (qty == 0)
                    return;
                
                detail.Detail = new OrderDetail(prod, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }


            var oldQty = detail.Detail.Qty;
            _order.UpdateInventory(detail.Detail, 1);
            detail.Detail.Qty = qty;
            _order.UpdateInventory(detail.Detail, -1);

            if (detail.Detail.Qty <= 0)
            {
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
                var isFreeItemHere = detail.Detail.IsFreeItem || (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree"));
                var isDefaultUomLine = !isFreeItemHere && item != null && EqualsUom(detail.UoM, item.DefaultUom);
                if (isDefaultUomLine)
                {
                    // Main line at 0: remove OrderDetail (no OrderDetail with qty 0); keep the main-line slot
                    _order.Details.Remove(detail.Detail);
                    detail.Detail = null;
                    detail.UpdateQuantityDisplay();
                    if (item != null)
                    {
                        item.NotifySublinesChanged();
                        item.PrimaryDetail = GetDefaultUomDetail(item);
                        item.UpdateDisplayProperties();
                    }
                    if (Config.Simone) _order.SimoneCalculateDiscount(); else _order.RecalculateDiscounts();
                    _order.Save();
                    RefreshTotals();
                    LoadLineItems();
                    HighlightItemOnly(item);
                    
                    if(_whatToViewInList == WhatToViewInList.Selected && item != null)
                        FilteredItems.Remove(item);
                    return;
                }
                // Remove subline from order
                _order.Details.Remove(detail.Detail);
                
                if (item == null) item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
                if (item != null)
                {
                    var detailToRemove = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailToRemove != null)
                        item.Details.Remove(detailToRemove);
                    detail.Detail = null;
                    item.PrimaryDetail = GetDefaultUomDetail(item);
                    item.UpdateDisplayProperties();
                    LoadLineItems();
                }
                else
                {
                    detail.Detail = null;
                }
            }
            else
            {
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
                if (item != null)
                {
                    var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailInItem != null)
                    {
                        detailInItem.UpdateQuantityDisplay();
                        item.NotifySublinesChanged();
                    }
                    item.PrimaryDetail = GetDefaultUomDetail(item);
                    item.UpdateDisplayProperties();
                    LoadLineItems();
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();
            HighlightItemOnly(itemToHighlight);
        }

        [RelayCommand]
        private async Task IncrementItemQuantityAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null)
                return;

            await IncrementQuantityAsync(item.PrimaryDetail);
        }

        [RelayCommand]
        private async Task DecrementItemQuantityAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null)
                return;

            await DecrementQuantityAsync(item.PrimaryDetail);
        }

        [RelayCommand]
        private async Task EditItemQuantityAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null)
                return;

            await EditQuantityAsync(item.PrimaryDetail);
        }

        [RelayCommand]
        private async Task ChangePriceAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null)
                return;

            var detail = item.PrimaryDetail;
            var currentPrice = detail.Detail?.Price ?? detail.Price;

            // Show simple price input dialog
            var priceText = await _dialogService.ShowPromptAsync(
                "Price",
                "Price:",
                "Set",
                "Cancel",
                "",
                -1,
                currentPrice.ToString("F2"),
                Keyboard.Numeric);

            if (string.IsNullOrEmpty(priceText) || !double.TryParse(priceText, out var newPrice))
                return;

            var prod = Product.Find(detail.ProductId);
            // Ensure detail exists
            if (detail.Detail == null)
            {
                detail.Detail = new OrderDetail(prod, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }

            // Update price
            detail.Detail.Price = newPrice;
            detail.Price = newPrice;

            // Mark as price changed
            if (Math.Round(newPrice, Config.Round) != Math.Round(detail.Detail.ExpectedPrice, Config.Round))
            {
                detail.Detail.ExtraFields = UDFHelper.SyncSingleUDF("pricechanged", "yes", detail.Detail.ExtraFields);
            }

            _order.Save();

            // Update display properties
            item.UpdateDisplayProperties();
            RefreshTotals();
        }

        [RelayCommand]
        private async Task SelectPriceLevelAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || _order == null)
                return;


            var detail = item.PrimaryDetail;
            var prod = Product.Find(detail.ProductId);

            var currentPrice = detail.Detail != null ? detail.Detail.Price : detail.Price;
            var currentPriceLevelSelected =
                detail.Detail != null ? detail.Detail.PriceLevelSelected : detail.PriceLevelSelected;
            var currentComments = detail.Detail != null ? detail.Detail.Comments : detail.Comment;

            // Show price level selection dialog
            var result = await _dialogService.ShowPriceLevelDialogAsync(
                item.ProductName,
                prod,
                _order,
                detail?.UoM,
                currentPriceLevelSelected,
                Math.Round(currentPrice, Config.Round).ToString(),
                currentComments);

            if (result.priceLevelId == null)
                return;

            // Update client price level if changed
            if (_order.Client != null && result.priceLevelId.Value != _order.Client.PriceLevel)
            {
                _order.Client.PriceLevel = result.priceLevelId.Value;
            }

            // Ensure detail exists
            if (detail != null)
            {
                if (detail.Detail == null)
                {
                    detail.Detail = new OrderDetail(prod, 0, _order);
                    detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                    detail.Detail.Price = detail.Price;
                    detail.Detail.UnitOfMeasure = detail.UoM;
                    detail.Detail.IsCredit = _itemType > 0;
                    detail.Detail.Damaged = _itemType == 1;
                    _order.Details.Add(detail.Detail);
                }

                // Update price level selected
                if (result.priceLevelId.HasValue)
                {
                    detail.Detail.PriceLevelSelected = result.priceLevelId.Value;
                    detail.PriceLevelSelected = result.priceLevelId.Value;
                }

                // Update price if provided
                if (!string.IsNullOrEmpty(result.price) && double.TryParse(result.price, out var newPrice))
                {
                    detail.Detail.Price = newPrice;
                    detail.Price = newPrice;

                    // Mark as price changed if different from expected
                    if (Math.Round(newPrice, Config.Round) != Math.Round(detail.Detail.ExpectedPrice, Config.Round))
                    {
                        detail.Detail.ExtraFields =
                            UDFHelper.SyncSingleUDF("pricechanged", "yes", detail.Detail.ExtraFields);
                    }
                }

                // Update comments if provided
                if (!string.IsNullOrEmpty(result.comments))
                {
                    detail.Detail.Comments = result.comments;
                    detail.Comment = result.comments;
                }
            }

            _order.Save();

            // Update display properties
            item.UpdateDisplayProperties();
            RefreshTotals();
        }

        [RelayCommand]
        public async Task AddFreeItemAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null) return;

            var detail = item.PrimaryDetail;

            var prod = Product.Find(detail.ProductId);
            // Validation: Check if detail exists and has qty > 0
            if (detail.Detail == null || detail.Detail.Qty == 0)
            {
                await _dialogService.ShowAlertAsync("Please add qty to make it a free product.", "Alert");
                return;
            }

            // Validation: Check if free item already exists
            if (item.HasFreeItemDetail)
            {
                await _dialogService.ShowAlertAsync("This product already has a free item.", "Alert");
                return;
            }

            // Validation: Check if order is locked or item is from offer
            if (_order.Locked() || (detail.Detail != null && detail.Detail.FromOffer))
            {
                await _dialogService.ShowAlertAsync("This item cannot be modified.", "Alert");
                return;
            }

            // Get the qty from the existing detail
            var qtyToUse = detail.Detail.Qty;
            var orderDetailIdToRemove = detail.Detail.OrderDetailId;

            // Remove any existing free items for this product to ensure only one free item exists
            var existingFreeItems = _order.Details.Where(d => 
                d.Product.ProductId == item.ProductId && 
                (d.IsFreeItem || (!string.IsNullOrEmpty(d.ExtraFields) && d.ExtraFields.Contains("productfree"))) &&
                (d.IsCredit ? (d.Damaged ? 1 : 2) : 0) == _itemType).ToList();
            
            if (existingFreeItems.Any())
            {
                foreach (var existingFree in existingFreeItems)
                {
                    _order.Details.Remove(existingFree);
                }
            }

            // Update inventory: remove qty from the main detail before removing it
            _order.UpdateInventory(detail.Detail, 1);

            // Remove the main line's OrderDetail from the order (qty is being moved to free item)
            _order.Details.Remove(detail.Detail);

            // Create a new OrderDetail for the free item
            var newDetail = new OrderDetail(prod, qtyToUse, _order);
            newDetail.ExpectedPrice = 0;
            newDetail.Price = 0;
            newDetail.UnitOfMeasure = detail.UoM;
            newDetail.IsCredit = _itemType > 0;
            newDetail.Damaged = _itemType == 1;
            newDetail.IsFreeItem = true;

            // Add the new free detail
            _order.Details.Add(newDetail);
            
            // Update inventory: add qty to the free item
            _order.UpdateInventory(newDetail, -1);

            // Handle related items
            var related = _order.Details.FirstOrDefault(x => newDetail.RelatedOrderDetail == x.OrderDetailId);
            if (related == null)
            {
                var f = _order.Details.FirstOrDefault(x => x.Product.ProductId == newDetail.Product.ProductId);
                if (f != null)
                    related = _order.Details.FirstOrDefault(x => x.OrderDetailId == f.RelatedOrderDetail);
            }

            if (related == null)
            {
                related = OrderDetail.AddRelatedItem(newDetail, _order);
            }
            else
            {
                related.Qty += qtyToUse;
            }

            // Recalculate discounts
            if (!OrderDiscount.HasDiscounts)
            {
                var nonFreeDetail = _order.Details.FirstOrDefault(x =>
                    x.Product.ProductId == item.ProductId && !x.IsFreeItem &&
                    (string.IsNullOrEmpty(x.ExtraFields) || !x.ExtraFields.Contains("productfree")));

                if (nonFreeDetail != null)
                {
                    if (nonFreeDetail.CalculateOfferDetail())
                    {
                        var detailVm = item.Details.FirstOrDefault(d =>
                            d.Detail != null && d.Detail.OrderDetailId == nonFreeDetail.OrderDetailId);
                        if (detailVm != null) detailVm.Price = nonFreeDetail.Price;
                    }
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            
            // Remove the OrderDetail from the primary detail and reset it to empty
            // Set the primary detail's Detail to null (we know this is the one we just removed)
            detail.Detail = null;
            
            // Also find and reset any other detail in the item's Details collection that references the removed OrderDetail
            var detailToReset = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailIdToRemove);
            if (detailToReset != null)
            {
                detailToReset.Detail = null;
            }
            
            // Ensure PrimaryDetail is set to an empty detail (Detail == null)
            // PrimaryDetail can NEVER be a free item
            var emptyDetail = item.Details.FirstOrDefault(d => d.Detail == null);
            if (emptyDetail == null)
            {
                // Create a new empty detail if none exists
                emptyDetail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                {
                    ProductId = item.ProductId,
                    ExpectedPrice = Product.GetPriceForProduct(prod, _order, _itemType > 0, _itemType == 1),
                    Price = Product.GetPriceForProduct(prod, _order, _itemType > 0, _itemType == 1)
                };
                
                // Set UoM if product has UoMs
                if (prod.UnitOfMeasures.Count > 0)
                {
                    var defaultUom = prod.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                     ?? prod.UnitOfMeasures.FirstOrDefault();
                    if (defaultUom != null)
                    {
                        emptyDetail.UoM = defaultUom;
                        emptyDetail.ExpectedPrice *= defaultUom.Conversion;
                        emptyDetail.Price *= defaultUom.Conversion;
                    }
                }
                
                item.Details.Add(emptyDetail);
            }
            
            item.PrimaryDetail = GetDefaultUomDetail(item);
            
            // Optimize: Update the specific item instead of rebuilding the entire list
            UpdateItemFromOrderDetails(item);
            item.UpdateDisplayProperties();
            
            // Update totals and line items in background
            RefreshTotals();
            LoadLineItems();
            HighlightItemOnly(item);
        }

        /// <summary>Shows UOM picker for the product. If user selects default UOM, nothing happens. If another UOM, adds a UOM subline.</summary>
        [RelayCommand]
        public async Task PickUomAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || _order == null) return;
            var prod = Product.Find(item.ProductId);
            if (prod?.UnitOfMeasures == null || prod.UnitOfMeasures.Count == 0) return;
            var defaultUom = prod.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) ?? prod.UnitOfMeasures.FirstOrDefault();
            var selected = await _dialogService.ShowPickUomForProductAsync(prod, "Select UOM");
            if (selected == null) return;
            if (EqualsUom(selected, defaultUom)) return; // default = main line, do nothing
            await AddUomSublineAsync(item, selected);
        }

        /// <summary>Adds a subline for the given non-default UOM (new OrderDetail, displayed under main line). One line per UOM.</summary>
        public async Task AddUomSublineAsync(AdvancedCatalogItemViewModel item, UnitOfMeasure uom)
        {
            if (item == null || uom == null || _order == null) return;
            var prod = Product.Find(item.ProductId);
            if (prod == null) return;
            if (_order.Locked())
            {
                await _dialogService.ShowAlertAsync("This order cannot be modified.", "Alert");
                return;
            }
            // Already have a subline for this UOM (with an OrderDetail)?
            var existingWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && EqualsUom(d.UoM, uom));
            if (existingWithDetail != null)
            {
                await _dialogService.ShowAlertAsync("This UOM already has a line.", "Alert");
                return;
            }
            var expectedPrice = Product.GetPriceForProduct(prod, _order, _itemType > 0, _itemType == 1);
            var price = expectedPrice;
            if (uom.Conversion != 0)
            {
                expectedPrice *= uom.Conversion;
                price *= uom.Conversion;
            }
            // Sublines are created with qty 1 (they cannot exist with qty < 1)
            var newOrderDetail = new OrderDetail(prod, 1, _order)
            {
                ExpectedPrice = expectedPrice,
                Price = price,
                UnitOfMeasure = uom,
                IsCredit = _itemType > 0,
                Damaged = _itemType == 1
            };
            _order.Details.Add(newOrderDetail);
            _order.UpdateInventory(newOrderDetail, -1);

            // Reuse existing empty detail for this UOM if present (e.g. from PrepareList); otherwise add one. Ensures only one subline per UOM.
            var existingEmpty = item.Details.FirstOrDefault(d => d.Detail == null && EqualsUom(d.UoM, uom));
            if (existingEmpty != null)
            {
                existingEmpty.Detail = newOrderDetail;
                existingEmpty.Price = price;
                existingEmpty.ExpectedPrice = expectedPrice;
            }
            else
            {
                var detailVm = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                {
                    ProductId = item.ProductId,
                    ExpectedPrice = expectedPrice,
                    Price = price,
                    UoM = uom,
                    Detail = newOrderDetail
                };
                item.Details.Add(detailVm);
            }

            item.PrimaryDetail = GetDefaultUomDetail(item);
            _order.Save();
            UpdateItemFromOrderDetails(item);
            item.UpdateDisplayProperties();
            // Force sublines list to refresh (DetailsWithOrderDetail) so the new subline appears immediately
            item.NotifySublinesChanged();
            RefreshTotals();
            LoadLineItems();
            HighlightItemOnly(item);
            MainThread.BeginInvokeOnMainThread(() => item.NotifySublinesChanged());
        }

        [RelayCommand]
        private async Task LineItemSelectedAsync(AdvancedCatalogItemViewModel.AdvancedCatalogLineItemViewModel? item)
        {
            if (item == null || item.Detail == null || _order == null)
                return;

            // Navigate to AddItemPage to edit the line item
            await Shell.Current.GoToAsync(
                $"additem?orderId={_order.OrderId}&orderDetailId={item.Detail.OrderDetailId}");
        }

        [RelayCommand]
        private async Task ViewProductDetailsAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null)
                return;

            _skipNextOnAppearingRefresh = true;
            // Match Xamarin's ProductNameClickedHandler behavior - navigate to ProductDetailsActivity
            var route = $"productdetails?productId={item.ProductId}";
            if (_order?.Client != null)
            {
                route += $"&clientId={_order.Client.ClientId}";
            }
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        private async Task ViewImageAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || !item.HasImage || string.IsNullOrEmpty(item.ProductImagePath))
                return;

            _skipNextOnAppearingRefresh = true;
            await Shell.Current.GoToAsync($"viewimage?imagePath={Uri.EscapeDataString(item.ProductImagePath)}");
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (_order == null)
                return;

            OrderDetail? lastDetail = null;

            if (Config.CatalogQuickAdd)
            {
                lastDetail = _order.Details.OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }
            else
            {
                lastDetail = _order.Details.OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }

            if (lastDetail != null)
            {
                await Shell.Current.GoToAsync(
                    $"fullcategory?orderId={_order.OrderId}&categoryId={lastDetail.Product.CategoryId}&productId={lastDetail.Product.ProductId}");
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

            if (Category.Categories.Count == 1)
            {
                var category = Category.Categories.FirstOrDefault();
                if (category != null)
                {
                    await Shell.Current.GoToAsync(
                        $"fullcategory?orderId={_order.OrderId}&categoryId={category.CategoryId}");
                }
            }
            else
            {
                await Shell.Current.GoToAsync(
                    $"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}");
            }
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

            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel",
                "Product name, UPC, SKU, or code");

            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            searchTerm = searchTerm.ToLowerInvariant().Trim();

            if (choice == "Search in Current Transaction")
            {
                var matchingLine = LineItems.FirstOrDefault(x =>
                    x.ProductName.ToLowerInvariant().IndexOf(searchTerm) != -1);

                if (matchingLine != null)
                {
                    await _dialogService.ShowAlertAsync($"Found: {matchingLine.ProductName}", "Search Result");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("No products found in current transaction.", "Search");
                }
            }
            else
            {
                // Search in product list - update search query
                SearchQuery = searchTerm;
            }
        }

        [RelayCommand]
        private async Task SelectItemTypeAsync(string itemType)
        {
            var newItemType = itemType switch
            {
                "Sales" => 0,
                "Dumps" => 1,
                "Returns" => 2,
                _ => 0
            };

            // Only rebuild if item type actually changed
            if (_itemType == newItemType)
                return;

            _itemType = newItemType;

            // Clear cache when item type changes
            _cachedItems = null;
            _cachedItemType = null;

            UpdateItemTypeText();
            PrepareList();
            Filter();
            RefreshTotals();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void ToggleOrderSummary()
        {
            IsOrderSummaryExpanded = !IsOrderSummaryExpanded;
        }

        private void UpdateButtonGridColumns()
        {
            // Count visible buttons
            int visibleCount = 0;
            if (ShowSalesButton) visibleCount++;
            if (ShowDumpsButton) visibleCount++;
            if (ShowReturnsButton) visibleCount++;

            // Show the grid only if at least one button is visible (always hide when from load order - catalog only)
            ShowButtonGrid = !IsFromLoadOrder && visibleCount > 0;

            // Set column definitions based on visible count
            ButtonGridColumns = visibleCount switch
            {
                1 => "*",
                2 => "*,*",
                3 => "*,*,*",
                _ => "*"
            };

            // Set column positions for each button based on visibility
            int currentColumn = 0;

            if (ShowSalesButton)
            {
                SalesButtonColumn = currentColumn++;
            }
            else
            {
                SalesButtonColumn = -1; // Hidden
            }

            if (ShowDumpsButton)
            {
                DumpsButtonColumn = currentColumn++;
            }
            else
            {
                DumpsButtonColumn = -1; // Hidden
            }

            if (ShowReturnsButton)
            {
                ReturnsButtonColumn = currentColumn++;
            }
            else
            {
                ReturnsButtonColumn = -1; // Hidden
            }
        }

        private void UpdateItemTypeText()
        {
            SelectedItemTypeText = _itemType switch
            {
                1 => "Dumps",
                2 => "Returns",
                _ => "Sales"
            };

            // Update button colors - selected uses PrimaryDark, unselected uses transparent
            var primaryDark = Microsoft.Maui.Graphics.Color.FromArgb("#0379cb");
            var white = Microsoft.Maui.Graphics.Colors.White;
            var black = Microsoft.Maui.Graphics.Colors.Black;
            var transparent = Microsoft.Maui.Graphics.Colors.Transparent;

            // Sales button
            if (_itemType == 0)
            {
                SalesButtonColor = primaryDark;
                SalesButtonTextColor = white;
            }
            else
            {
                SalesButtonColor = transparent;
                SalesButtonTextColor = black;
            }

            // Dumps button
            if (_itemType == 1)
            {
                DumpsButtonColor = primaryDark;
                DumpsButtonTextColor = white;
            }
            else
            {
                DumpsButtonColor = transparent;
                DumpsButtonTextColor = black;
            }

            // Returns button
            if (_itemType == 2)
            {
                ReturnsButtonColor = primaryDark;
                ReturnsButtonTextColor = white;
            }
            else
            {
                ReturnsButtonColor = transparent;
                ReturnsButtonTextColor = black;
            }
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

            await ContinueSendingOrderAsync();
        }

        private async Task ContinueSendingOrderAsync()
        {
            if (_order == null)
                return;

            // Show confirmation dialog (matching Xamarin ContinueSendingOrder)
            var result = await _dialogService.ShowConfirmAsync(
                "Continue sending order?",
                "Warning",
                "Yes",
                "No");
            if (!result)
                return;

            // Check ship date requirements
            if (Config.PresaleShipDate && _order.ShipDate.Year == 1)
            {
                // TODO: Implement SendWithShipDate
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return;
            }
            else if (Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return;
            }

            // Validate order minimum
            bool valid = _order.ValidateOrderMinimum();
            if (!valid)
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

                DataProvider.SendTheOrders(new Batch[] { batch });

                if (_order.AsPresale && _order.Client != null && _order.Client.ClientId <= 0)
                {
                    _order.Client.Editable = false;
                    Client.Save();
                }

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success");

                // [ACTIVITY STATE]: Remove state when properly exiting
                var route = "advancedcatalog";
                if (_order != null)
                {
                    route += $"?orderId={_order.OrderId}";
                }
                Helpers.NavigationHelper.RemoveNavigationState(route);

                await NavigateBackFromCatalogAsync();
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
            }

            // Check lot mandatory
            if (Config.LotIsMandatoryBeforeFinalize && _order.Details.Any(x => string.IsNullOrEmpty(x.Lot)))
            {
                await _dialogService.ShowAlertAsync("Lot is mandatory.", "Alert");
                return;
            }

            _order.Modified = true;
            _order.Save();

            // [ACTIVITY STATE]: Remove state when properly exiting
            // Remove the order page state so it doesn't get restored when navigating back
            var route = "advancedcatalog";
            if (_order != null)
            {
                route += $"?orderId={_order.OrderId}";
            }
            Helpers.NavigationHelper.RemoveNavigationState(route);

            // Also remove from ActivityState directly to ensure it's completely removed
            var orderState = ActivityState.GetState("AdvancedCatalogActivity");
            if (orderState != null)
            {
                ActivityState.RemoveState(orderState);
            }

            // Navigate back - matches Xamarin's Finish() behavior which just closes the current Activity
            // and returns to the previous one (ClientDetailsPage), or to NewLoadOrderTemplatePage when comingFrom=LoadOrderTemplate
            await NavigateBackFromCatalogAsync();
        }

        private async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            // Check if order is voided
            if (_order.Voided)
            {
                return true; // Allow navigation
            }

            // Check if order is empty
            bool isEmpty = _order.Details.Count == 0 ||
                           (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
                // Remove if empty order
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
                    return true; // Allow navigation
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
                        OnPropertyChanged(nameof(ShowWhatToViewButton)); // Notify UI that button visibility changed
                        _order.Void();
                        _order.Save();
                        return true; // Allow navigation
                    }

                    return false; // Don't navigate
                }
            }
            else
            {
                // Make sure that all the weighted lines have value
                if (!_order.AsPresale)
                {
                    if (_order.Details.Any(x => x.Product.SoldByWeight && x.Weight == 0) &&
                        !Config.MustSetWeightInDelivery)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "Some weight items don't have weight. Do you want to delete them?",
                            "Warning",
                            "Yes",
                            "No");
                        if (result)
                        {
                            // Delete 0 weight items
                            var toDelete = _order.Details.Where(x => x.Product.SoldByWeight && x.Weight == 0).ToList();
                            foreach (var detail in toDelete)
                            {
                                _order.Details.Remove(detail);
                            }

                            _order.Save();
                            // Recursively call FinalizeOrder
                            return await FinalizeOrderAsync();
                        }

                        return false; // Don't navigate
                    }
                }

                // Check mandatory image
                if (Config.ImageInOrderMandatory && _order.ImageList.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory.", "Alert");
                    return false; // Don't navigate
                }

                // Check mandatory PO
                if ((Config.POIsMandatory || _order.Client?.POIsMandatory == true) &&
                    string.IsNullOrEmpty(_order.PONumber) && _order.Client?.POIsMandatory == true)
                {
                    await _dialogService.ShowAlertAsync("You need to enter PO number.", "Alert");
                    return false; // Don't navigate
                }

                // Check Bill number
                if (_order.OrderType == OrderType.Bill && string.IsNullOrEmpty(_order.PONumber) &&
                    Config.BillNumRequired)
                {
                    await _dialogService.ShowAlertAsync("You need to enter Bill number.", "Alert");
                    return false; // Don't navigate
                }

                // Check mandatory ship date
                if (_order.AsPresale && Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
                {
                    await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                    return false; // Don't navigate
                }

                // Check if order must be sent
                if (_order.AsPresale && Config.SendOrderIsMandatory)
                {
                    await _dialogService.ShowAlertAsync("Order must be sent.", "Alert");
                    return false; // Don't navigate
                }

                // Check case in/out requirements
                if (Config.MustEnterCaseInOut &&
                    (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Credit) && !_order.AsPresale)
                {
                    // TODO: Implement EnterCasesInOut
                    await _dialogService.ShowAlertAsync("Case in/out functionality is not yet fully implemented.",
                        "Info");
                    return false; // Don't navigate
                }

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

                // Add to session
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
        public async Task ShowMenuAsync()
        {
            if (_order == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice =
                await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel",
                    options.Select(o => o.Title).ToArray());
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

            if (_comingFrom == "LoadOrderTemplate")
            {
                options.Add(new MenuOption("Add To Order", async () => await DoneAsync()));
                return options;
            }

            var finalized = _order.Finished;
            var voided = _order.Voided;
            var canEdit = CanEdit;
            var locked = _order.Locked();
            var asset = UDFHelper.GetSingleUDF("workOrderAsset", _order.ExtraFields);
            var isWorkOrder = _order != null && _order.IsWorkOrder;
            var hasOrderDiscounts = OrderDiscount.List.Count > 0 && !isWorkOrder;
            var allowDiscount = _order.Client.UseDiscount && !OrderDiscount.HasDiscounts;
            var isSplitClient = _order.Client.SplitInvoices.Count > 0;
            var asPresale = _order.AsPresale;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), only show Print option
            bool isReadOnly = !_order.AsPresale && (finalized || voided);
            
            if (isReadOnly)
            {
                // Only allow Print and Advanced Options when read-only
                options.Add(new MenuOption("Print", async () =>
                {
                    await PrintAsync();
                }));
                options.Add(new MenuOption("Advanced Options", async () => await ShowAdvancedOptionsAsync()));
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

            // Presale-specific menu items (matching AdvancedTemplateActivity order)
            if (asPresale)
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
                
                // Select Driver - visible only when presale and no work order asset (matches TemplateActivity OnPrepareOptionsMenu)
                if (string.IsNullOrEmpty(asset))
                {
                    options.Add(new MenuOption("Select Driver", async () =>
                    {
                        await SelectSalesmanAsync();
                    }));
                }
                
                // Other Charges - Xamarin order: after Select Driver (presaleMenu.xml)
                if (!string.IsNullOrEmpty(asset) || Config.AllowOtherCharges)
                {
                    options.Add(new MenuOption("Other Charges", async () =>
                    {
                        await OtherChargesAsync();
                    }));
                }
                
                // What To View option moved to button in Search and Sort section
                // options.Add(new MenuOption(_whatToViewInList == WhatToViewInList.All ? "Added/All" : "Just Ordered", async () =>
                // {
                //     await WhatToViewClickedAsync();
                // }));
                
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
                bool isVisible_ = true;
                if (finalized && Config.MustAddImageToFinalized)
                    isVisible_ = false;
                if (Config.CaptureImages && isVisible_)
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

                // What To View (non-presale) - moved to button in Search and Sort section
                // if (!finalized)
                // {
                //     options.Add(new MenuOption(_whatToViewInList == WhatToViewInList.All ? "Added/All" : "Just Ordered", async () =>
                //     {
                //         await WhatToViewClickedAsync();
                //     }));
                // }

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
                
                // Set PO (presale)
                options.Add(new MenuOption(_order.OrderType == OrderType.Bill ? "Set Bill Number" : "Set PO", async () =>
                {
                    await GetPONumberAsync();
                }));

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
                bool isVisible_ = true;
                if (finalized && Config.MustAddImageToFinalized)
                    isVisible_ = false;
                if (Config.CaptureImages && isVisible_)
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
                options.Add(new MenuOption("Delete Lines With No Weight", async () =>
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

            // Advanced Options - same as LaceupContentPage.GetCommonMenuOptions(), so it appears when using custom menu
            options.Add(new MenuOption("Advanced Options", async () => await ShowAdvancedOptionsAsync()));

            return options;
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        // Menu option methods - matching PreviouslyOrderedTemplatePageViewModel
        private async Task GotoBarcodeReaderAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"barcodereader?orderId={_order.OrderId}");
        }

        private async Task ApplyDiscountAsync()
        {
            if (_order == null)
                return;
            await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.", "Info");
        }

        private async Task GetPONumberAsync()
        {
            if (_order == null)
                return;
            var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:",
                initialValue: _order.PONumber ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(po))
            {
                _order.PONumber = po;
                _order.Save();
                await _dialogService.ShowAlertAsync("PO number set.", "Success");
            }
        }

        private async Task AddEditShipViaAsync()
        {
            if (_order == null)
                return;
            await _dialogService.ShowAlertAsync("Ship Via functionality is not yet fully implemented.", "Info");
        }

        private async Task SetShipDateAsync()
        {
            if (_order == null)
                return;
            var currentShipDate = _order.ShipDate.Year == 1 ? DateTime.Now : _order.ShipDate;
            var selectedDate = await _dialogService.ShowDatePickerAsync("Set Ship Date", currentShipDate, DateTime.Now, null);
            if (selectedDate.HasValue)
            {
                _order.ShipDate = selectedDate.Value;
                _order.Save();
            }
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
            await _dialogService.ShowAlertAsync("Calculate Offers functionality is not yet fully implemented.", "Info");
        }

        private async Task ConvertQuoteToSalesOrderAsync()
        {
            if (_order == null)
                return;
            await _dialogService.ShowAlertAsync("Convert Quote to Sales Order functionality is not yet fully implemented.", "Info");
        }

        private async Task SignAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"ordersignature?ordersId={_order.OrderId}");
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
            {
                // UpdateRoute(false); // TODO: Implement UpdateRoute
            }

            var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
            if (batch != null)
                batch.Delete();

            _order.Delete();

            // Remove state
            Helpers.NavigationHelper.RemoveNavigationState("advancedcatalog");

            await NavigateBackFromCatalogAsync();
        }

        private async Task GetPaymentAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"payment?orderId={_order.OrderId}");
        }

        private async Task GoToServiceReportAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"servicereport?orderId={_order.OrderId}");
        }

        private async Task ShowCommentsDialogAsync()
        {
            if (_order == null)
                return;
            var comments = await _dialogService.ShowPromptAsync("Add Comments", "Enter comments:",
                initialValue: _order.Comments ?? string.Empty);
            if (comments != null)
            {
                _order.Comments = comments;
                _order.Save();
                await _dialogService.ShowAlertAsync("Comments saved.", "Success");
            }
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
                    await NavigateBackFromCatalogAsync();
                return;
            }

            await _dialogService.ShowAlertAsync("Enter Crate In/Out functionality is not yet fully implemented.", "Info");
        }

        private async Task UseLspInAllLinesAsync()
        {
            if (_order == null)
                return;
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

            // Match TemplateActivity.SelectSalesman(): Driver | DSD, InventorySiteId > 0 and != BranchSiteId
            var selectedRole = SalesmanRole.Driver | SalesmanRole.DSD;
            var salesmen = Salesman.List
                .Where(x => ((int)x.Roles & (int)selectedRole) > 0)
                .Where(x => x.InventorySiteId > 0 && x.InventorySiteId != Config.BranchSiteId)
                .OrderBy(x => x.Name)
                .ToList();

            if (salesmen.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No drivers available to select.", "Select Driver");
                return;
            }

            var driverNames = salesmen.Select(x => x.Name).ToArray();
            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Driver", driverNames);
            if (selectedIndex < 0 || selectedIndex >= salesmen.Count)
                return;

            _order.ExtraFields = UDFHelper.SyncSingleUDF("Salesman", salesmen[selectedIndex].Id.ToString(), _order.ExtraFields);
            _order.Save();
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
            await _dialogService.ShowAlertAsync("Edit Client Values functionality is not yet fully implemented.", "Info");
        }

        private async Task ShowSuggestedProductsAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"productcatalog?orderId={_order.OrderId}&isShowingSuggested=1&comingFrom=AdvancedCatalog");
        }

        [RelayCommand]
        private async Task WhatToViewClickedAsync()
        {
            _whatToViewInList = _whatToViewInList == WhatToViewInList.All ? WhatToViewInList.Selected : WhatToViewInList.All;
            // Clear cache to force reload with new filter
            _cachedItems = null;
            OnPropertyChanged(nameof(WhatToViewButtonText)); // Notify UI that button text changed
            await RefreshAsync();
        }

        private async Task OtherChargesAsync()
        {
            if (_order == null)
                return;
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
                "Alert",
                "Yes",
                "No");

            if (!confirmed)
                return;

            foreach (var detail in toDelete)
            {
                _order.DeleteDetail(detail);
            }

            _order.Save();
            await RefreshAsync();
            await _dialogService.ShowAlertAsync($"{toDelete.Count} weight item(s) deleted.", "Success");
        }

        private async Task ResetOrderAsync()
        {
            if (_order == null)
                return;

            var resetValue = UDFHelper.GetSingleUDF("reset", _order.ExtraFields);
            var isReset = !string.IsNullOrEmpty(resetValue) && resetValue == "1";

            var confirmed = await _dialogService.ShowConfirmAsync(
                isReset ? "Are you sure you want to undo the reset?" : "Are you sure you want to reset this order?",
                "Alert",
                "Yes",
                "No");

            if (!confirmed)
                return;

            if (isReset)
            {
                _order.ExtraFields = UDFHelper.SyncSingleUDF("reset", "0", _order.ExtraFields);
            }
            else
            {
                _order.ExtraFields = UDFHelper.SyncSingleUDF("reset", "1", _order.ExtraFields);
            }

            _order.Save();
            await RefreshAsync();
        }

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

        [RelayCommand]
        public async Task GoBackAsync()
        {
            if (_order == null)
            {
                await NavigateBackFromCatalogAsync();
                return;
            }

            // From load order: never finalize or delete; back goes to previous page (categories if from categories, else load order template)
            if (IsFromLoadOrder)
            {
                _order.Save();
                var route = "advancedcatalog";
                if (_order != null)
                    route += $"?orderId={_order.OrderId}";
                Helpers.NavigationHelper.RemoveNavigationState(route);
                await NavigateBackOneAsync(); // Back: one pop → categories or load template depending how we got here
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
                    // [ACTIVITY STATE]: Remove state when properly exiting
                    var route = "advancedcatalog";
                    if (_order != null)
                    {
                        route += $"?orderId={_order.OrderId}";
                    }
                    Helpers.NavigationHelper.RemoveNavigationState(route);
                    
                    await NavigateBackFromCatalogAsync();
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
                            // [ACTIVITY STATE]: Remove state when properly exiting
                            var route = "advancedcatalog";
                            if (_order != null)
                            {
                                route += $"?orderId={_order.OrderId}";
                            }
                            Helpers.NavigationHelper.RemoveNavigationState(route);
                            
                            await NavigateBackFromCatalogAsync();
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
                    // [ACTIVITY STATE]: Remove state when properly exiting
                    var route = "advancedcatalog";
                    if (_order != null)
                    {
                        route += $"?orderId={_order.OrderId}";
                    }
                    Helpers.NavigationHelper.RemoveNavigationState(route);
                    
                    await NavigateBackFromCatalogAsync();
                }
            }
        }

        private string GetOrderTypeText(Order order)
        {
            if (order.OrderType == OrderType.Order)
            {
                if (order.AsPresale)
                {
                    return order.IsQuote ? "Quote" : "Sales Order";
                }

                return "Sales Invoice";
            }
            else if (order.OrderType == OrderType.Credit)
            {
                return order.AsPresale ? "Credit Order" : "Credit Invoice";
            }
            else if (order.OrderType == OrderType.Return)
            {
                return order.AsPresale ? "Return Order" : "Return Invoice";
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

        private enum SortCriteria
        {
            ProductName = 0,
            ProductCode = 1,
            Description = 2,
            Category = 3,
            InStock = 4,
            Qty = 5,
            Descending = 6,
            OrderOfEntry = 7
        }
    }

    public partial class AdvancedCatalogItemViewModel : ObservableObject
    {
        /// <summary>Formats OH/inventory for display: whole number with no decimals; if it has decimals round to 2 and show (trailing zeros trimmed).</summary>
        private static string FormatOhDisplay(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "0";
            var rounded = Math.Round(value, 2);
            if (Math.Abs(rounded - Math.Truncate(rounded)) < 1e-10)
                return ((long)Math.Round(rounded)).ToString();
            return rounded.ToString("0.##");
        }

        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string UPC { get; set; }
        public string SKU { get; set; }
        
        public int DiscountCategoryId { get; set; }
        public double CurrentWarehouseInventory { get; set; }
        public int ProductId { get; set; }
        public int ItemType { get; set; }
        public List<InvoiceDetail> History { get; } = new();
        private ObservableCollection<AdvancedCatalogDetailViewModel> _details = new();
        public ObservableCollection<AdvancedCatalogDetailViewModel> Details 
        { 
            get
            {
                // Set up CollectionChanged handler on first access if not already set
                if (_details != null && !_hasDetailsHandlerSet)
                {
                    _details.CollectionChanged += Details_CollectionChanged;
                    _hasDetailsHandlerSet = true;
                }
                return _details;
            }
            set
            {
                if (_details != null)
                    _details.CollectionChanged -= Details_CollectionChanged;
                    
                if (SetProperty(ref _details, value))
                {
                    if (_details != null)
                    {
                        _details.CollectionChanged += Details_CollectionChanged;
                        _hasDetailsHandlerSet = true;
                    }
                    OnPropertyChanged(nameof(HasMultipleDetails));
                    OnPropertyChanged(nameof(HasSingleDetail));
                }
            }
        }
        
        private bool _hasDetailsHandlerSet = false;
        
        private void Details_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasMultipleDetails));
            OnPropertyChanged(nameof(HasSingleDetail));
            OnPropertyChanged(nameof(HasFreeItemDetail));
            OnPropertyChanged(nameof(HasUomSublines));
            OnPropertyChanged(nameof(ShowFreeItemDetail));
            OnPropertyChanged(nameof(ShowSublinesInCell));
            OnPropertyChanged(nameof(DetailsWithOrderDetail));
            OnPropertyChanged(nameof(FreeItemDetail));
            OnPropertyChanged(nameof(CanAddFreeItem));
            OnPropertyChanged(nameof(FreeItemButtonText));
            OnPropertyChanged(nameof(FreeItemButtonColor));
        }
        
        public int OrderId { get; set; }

        /// <summary>True when this catalog is used for a load order: cell shows only OH and Truck Inventory.</summary>
        public bool IsLoadOrderDisplay { get; set; }

        /// <summary>True when price, history, type, and other non-load info should be shown.</summary>
        public bool ShowPriceHistoryType => !IsLoadOrderDisplay;

        /// <summary>True when type text should be shown (not load and has type).</summary>
        public bool ShowTypeInCell => ShowPriceHistoryType && ShowTypeText;

        /// <summary>True when free item section should be shown (not load and has free item).</summary>
        public bool ShowFreeItemDetailInCell => ShowPriceHistoryType && ShowFreeItemDetail;

        /// <summary>True when any sublines (free item or UOM) should be shown.</summary>
        /// <summary>True when sublines (free item or UOM) should be shown. UOM sublines show for both load order and normal; free item only when not load order.</summary>
        public bool ShowSublinesInCell => (ShowPriceHistoryType && HasFreeItemDetail) || HasUomSublines;

        /// <summary>True when Add Free Item button should be shown (not load).</summary>
        public bool ShowFreeItemButtonInCell => ShowPriceHistoryType && ShowFreeItemButton;
        
        private AdvancedCatalogDetailViewModel? _primaryDetail;

        public AdvancedCatalogDetailViewModel? PrimaryDetail
        {
            get => _primaryDetail;
            set
            {
                if (SetProperty(ref _primaryDetail, value))
                {
                    UpdateDisplayProperties();
                }
            }
        }

        [ObservableProperty] private string _productDisplayName = string.Empty;

        [ObservableProperty] private string _onHandText = string.Empty;
        
        [ObservableProperty] private Color _ohColor = Color.FromArgb("#1f1f1f");
        
        [ObservableProperty] private string _truckInventoryText = string.Empty;

        [ObservableProperty] private string _listPriceText = string.Empty;

        [ObservableProperty] private string _currentPriceText = string.Empty;

        [ObservableProperty] private string _historyText = string.Empty;

        [ObservableProperty] private bool _hasHistory;

        [ObservableProperty] private bool _hasNoHistory;

        [ObservableProperty] private string _productImagePath = string.Empty;

        [ObservableProperty] private bool _hasImage;
        
        [ObservableProperty] private bool _canAddFreeItem = false;

        [ObservableProperty] private bool _showFreeItemButton = false;

        [ObservableProperty] private string _freeItemButtonText = "Add Free Item";

        [ObservableProperty] private Microsoft.Maui.Graphics.Color _freeItemButtonColor = Microsoft.Maui.Graphics.Color.FromArgb("#007AFF"); // Blue

        [ObservableProperty] private string _quantityText = "0";

        [ObservableProperty] private string _typeText = string.Empty;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _typeColor = Microsoft.Maui.Graphics.Colors.Transparent;

        [ObservableProperty] private bool _showTypeText = false;

        [ObservableProperty] private bool _canChangePrice = false;

        [ObservableProperty] private bool _showSelectPL = false;

        /// <summary>Original quantity text for delivery orders (e.g. "Org. Qty=5"). Visible only when order is delivery and product had Ordered > 0.</summary>
        [ObservableProperty] private string _orgQtyText = string.Empty;

        [ObservableProperty] private bool _showOrgQty;

        /// <summary>True when this line was just scanned (ScanAsync); row shows light blue background. Match TransferOnOffPage.</summary>
        [ObservableProperty] private bool _isHighlightedFromScan;

        partial void OnIsHighlightedFromScanChanged(bool value) => OnPropertyChanged(nameof(RowBackgroundColor));

        /// <summary>Row background color; light blue when highlighted from scan, else white. Match TransferOnOffPage.</summary>
        public Microsoft.Maui.Graphics.Color RowBackgroundColor => IsHighlightedFromScan ? Microsoft.Maui.Graphics.Color.FromArgb("#ADD8E6") : Microsoft.Maui.Graphics.Colors.White;

        public bool HasMultipleDetails => HasFreeItemDetail || HasUomSublines;
        public bool HasSingleDetail => !HasFreeItemDetail && !HasUomSublines;
        
        public bool HasFreeItemDetail => Details.Any(d => d.Detail != null && (d.Detail.IsFreeItem || 
            (!string.IsNullOrEmpty(d.Detail.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"))));
        
        /// <summary>True when product has multiple UOMs (show UOM link under price).</summary>
        public bool HasUoms => (Product.Find(ProductId)?.UnitOfMeasures?.Count ?? 0) > 0;
        
        /// <summary>Default UOM for this product (main line uses this).</summary>
        public UnitOfMeasure? DefaultUom => Product.Find(ProductId)?.UnitOfMeasures?.FirstOrDefault(u => u.IsDefault) 
            ?? Product.Find(ProductId)?.UnitOfMeasures?.FirstOrDefault();
        
        private static bool IsDefaultUom(UnitOfMeasure? uom, UnitOfMeasure? defaultUom)
        {
            if (uom == null && defaultUom == null) return true;
            return uom != null && defaultUom != null && uom.Id == defaultUom.Id;
        }
        private static bool IsFreeItemDetail(AdvancedCatalogDetailViewModel d) =>
            d.Detail?.IsFreeItem == true || (!string.IsNullOrEmpty(d.Detail?.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"));
        
        /// <summary>True when there are UOM sublines (non-default UOM details with OrderDetail).</summary>
        public bool HasUomSublines => Details.Any(d => d.Detail != null && !IsFreeItemDetail(d) && !IsDefaultUom(d.UoM, DefaultUom));
        
        // Show free item detail section only if free item exists AND it's a Sales item in Order order
        public bool ShowFreeItemDetail => HasFreeItemDetail && ShowFreeItemButton;
        
        // Only show details that have OrderDetail attached (for sublines display): free items + UOM sublines (non-default UOM). Primary/default line is NOT included.
        public IEnumerable<AdvancedCatalogDetailViewModel> DetailsWithOrderDetail => 
            Details.Where(d => d.Detail != null && (IsFreeItemDetail(d) || !IsDefaultUom(d.UoM, DefaultUom)))
                .OrderBy(d => d.Detail?.IsFreeItem == true ? 0 : 1)
                .ThenBy(d => d.UomText);
        
        // Single free item detail for Grid binding (first free item, if any)
        public AdvancedCatalogDetailViewModel? FreeItemDetail => 
            DetailsWithOrderDetail.FirstOrDefault(IsFreeItemDetail);

        /// <summary>Call after adding/updating/removing a subline so the BindableLayout and visibility refresh.</summary>
        public void NotifySublinesChanged()
        {
            OnPropertyChanged(nameof(HasUomSublines));
            OnPropertyChanged(nameof(HasMultipleDetails));
            OnPropertyChanged(nameof(DetailsWithOrderDetail));
            OnPropertyChanged(nameof(ShowSublinesInCell));
            OnPropertyChanged(nameof(FreeItemDetail));
        }

        public void UpdateDisplayProperties(bool skipFreeItemUpdate = false)
        {
            // Use stored order if not provided
            var order = Order.Find(OrderId);
            if(order != null)
            {
                var prod = Product.Find(ProductId);
                // Product Display Name (Code + Name)
                ProductDisplayName = $"{ProductName}";

                // Use PrimaryDetail's UoM if available, otherwise use first detail's UoM
                var detailForUom = PrimaryDetail ?? Details.FirstOrDefault();
                string uomName = detailForUom?.UoM?.Name ?? string.Empty;
                double uomConversion = detailForUom?.UoM?.Conversion ?? 1.0;

                // On-Hand (OH) and Truck Inventory: for load order show only those two; otherwise use GetInventory
                // OH format: whole number with no decimals; if it has decimals show them (no rounding, no trailing zeros)
                var isLoadOrder = order != null && order.OrderType == OrderType.Load;
                if (isLoadOrder)
                {
                    var ohBase = (double)prod.CurrentWarehouseInventory;
                    var truckBase = (double)prod.CurrentInventory;
                    var ohDisplay = ohBase / uomConversion;
                    var truckDisplay = truckBase / uomConversion;
                    var ohStr = FormatOhDisplay(ohDisplay);
                    var truckStr = FormatOhDisplay(truckDisplay);
                    
                    var color = ohDisplay > 0 ? Color.FromArgb("#3FBC4D") : Color.FromArgb("#BA2D0B");
                    OhColor = color;

                    OnHandText = string.IsNullOrEmpty(uomName)
                        ? $"OH: {ohStr}"
                        : $"OH: {ohStr} {uomName}";
                    TruckInventoryText = string.IsNullOrEmpty(uomName)
                        ? $"Truck Inventory: {truckStr}"
                        : $"Truck Inventory: {truckStr} {uomName}";
                }
                else
                {
                    var aspresale = order != null ? order.AsPresale : true;
                    var ohBase = prod.GetInventory(aspresale, false);
                    double ohDisplay = ohBase / uomConversion;
                    var ohStr = FormatOhDisplay(ohDisplay);
                    OnHandText = string.IsNullOrEmpty(uomName)
                        ? $"OH: {ohStr}"
                        : $"OH: {ohStr} {uomName}";
                    TruckInventoryText = string.Empty;
                    
                    var color = ohDisplay > 0 ? Color.FromArgb("#3FBC4D") : Color.FromArgb("#BA2D0B");
                    OhColor = color;
                }

                // Org. Qty: only for delivery orders when product had original ordered qty > 0 (matches Xamarin PreviouslyOrderedTemplateActivity)
                var mainDetail = PrimaryDetail?.Detail ?? Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem)?.Detail;
                if (order != null && order.IsDelivery && mainDetail != null && mainDetail.Ordered > 0)
                {
                    ShowOrgQty = true;
                    if (prod.SoldByWeight && Config.NewAddItemRandomWeight)
                    {
                        var deletedCount = order.DeletedDetails?.Count(x => x.Product?.ProductId == ProductId) ?? 0;
                        OrgQtyText = $"Org. Qty={(float)(mainDetail.Ordered + deletedCount)}";
                    }
                    else
                    {
                        OrgQtyText = $"Org. Qty={mainDetail.Ordered}";
                    }
                }
                else
                {
                    ShowOrgQty = false;
                    OrgQtyText = string.Empty;
                }

                // List Price
                var listPrice = prod.PriceLevel0;
                ListPriceText = $"List Price: {listPrice.ToCustomString()}";

                // ShowFreeItemButton: only visible for Sales items (ItemType == 0) and OrderType.Order (not Credit or Return)
                ShowFreeItemButton = order != null && 
                                     order.OrderType == OrderType.Order && 
                                     ItemType == 0 && Config.AllowFreeItems;

                // CanAddFreeItem: enabled if main line has qty > 0 and no free items exist yet
                // Only available for Sales items in Order orders
                var mainDetailQty = PrimaryDetail?.Detail?.Qty ?? 0;
                var hasFreeItems = HasFreeItemDetail;
                CanAddFreeItem = ShowFreeItemButton &&
                                 Config.AllowFreeItems && 
                                 !order.Locked() && 
                                 mainDetailQty > 0 && 
                                 !hasFreeItems;
                
                // Notify ShowFreeItemDetail property change
                OnPropertyChanged(nameof(ShowFreeItemDetail));

                // Update Free Item button text and color
                if (hasFreeItems)
                {
                    FreeItemButtonText = "Free Item Added";
                    FreeItemButtonColor = Microsoft.Maui.Graphics.Color.FromArgb("#34C759"); // Green
                }
                else
                {
                    FreeItemButtonText = "Add Free Item";
                    FreeItemButtonColor = Microsoft.Maui.Graphics.Color.FromArgb("#007AFF"); // Blue
                }

                // Main line must always be the default UOM detail. Never point to free item or non-default UOM.
                if (PrimaryDetail != null && (IsFreeItemDetail(PrimaryDetail) || !IsDefaultUom(PrimaryDetail.UoM, DefaultUom)))
                {
                    var defaultDetail = Details.FirstOrDefault(d => !IsFreeItemDetail(d) && IsDefaultUom(d.UoM, DefaultUom));
                    if (defaultDetail != null)
                        PrimaryDetail = defaultDetail;
                }
                
                if (PrimaryDetail != null && PrimaryDetail.Detail != null && 
                    !PrimaryDetail.Detail.IsFreeItem &&
                    (string.IsNullOrEmpty(PrimaryDetail.Detail.ExtraFields) || !PrimaryDetail.Detail.ExtraFields.Contains("productfree")))
                {
                    CurrentPriceText = $"Price:{PrimaryDetail.Detail.Price.ToCustomString()}";
                    QuantityText = PrimaryDetail.Detail.Qty.ToString("F0");
                    
                    // Update CanAddFreeItem
                    var mainQty = PrimaryDetail.Detail.Qty;
                    CanAddFreeItem = Config.AllowFreeItems && 
                                     order != null && 
                                     !order.Locked() && 
                                     mainQty > 0 && 
                                     !HasFreeItemDetail;

                    // Set type text based on detail
                    if (PrimaryDetail.Detail.IsCredit)
                    {
                        TypeText = PrimaryDetail.Detail.Damaged ? "Dump" : "Return";
                        TypeColor = Microsoft.Maui.Graphics.Colors.Orange;
                        ShowTypeText = true;
                    }
                    else
                    {
                        TypeText = string.Empty;
                        TypeColor = Microsoft.Maui.Graphics.Colors.Transparent;
                        ShowTypeText = false;
                    }
                }
                else if (PrimaryDetail != null)
                {
                    CurrentPriceText = $"Price:{PrimaryDetail.Price.ToCustomString()}";
                    QuantityText = "0";
                    CanAddFreeItem = false; // No qty, can't add free item
                    TypeText = string.Empty;
                    TypeColor = Microsoft.Maui.Graphics.Colors.Transparent;
                    ShowTypeText = false;
                }
                else
                {
                    // Use order's client if available, otherwise null
                    var client = order?.Client;
                    var price = Product.GetPriceForProduct(prod, client, ItemType > 0, ItemType == 1);
                    CurrentPriceText = $"Price:{price.ToCustomString()}";
                    QuantityText = "0";
                    CanAddFreeItem = false; // No qty, can't add free item
                    TypeText = string.Empty;
                    TypeColor = Microsoft.Maui.Graphics.Colors.Transparent;
                    ShowTypeText = false;
                }

                // History Text
                HasHistory = History != null && History.Count > 0;
                HasNoHistory = !HasHistory;

                if (HasHistory)
                {
                    var historyLines = History.Take(3).Select(h =>
                        $"{h.Date:M/d/yyyy} {h.Quantity} @ {h.Price.ToCustomString()}");
                    HistoryText = string.Join(", ", historyLines);
                }
                else
                {
                    HistoryText = string.Empty;
                }

                // Product Image (use thumbnails / small files for list performance; list cell shows 60x60)
                var imagePath = ProductImage.GetProductImage(ProductId);
                HasImage = !string.IsNullOrEmpty(imagePath);
                ProductImagePath = imagePath ?? string.Empty;

                OnPropertyChanged(nameof(IsLoadOrderDisplay));
                OnPropertyChanged(nameof(ShowTypeInCell));
                OnPropertyChanged(nameof(ShowFreeItemDetailInCell));
                OnPropertyChanged(nameof(ShowSublinesInCell));
                OnPropertyChanged(nameof(ShowFreeItemButtonInCell));
                // Force main cell bindings to refresh (QuantityText, CurrentPriceText) after qty/price changes
                OnPropertyChanged(nameof(QuantityText));
                OnPropertyChanged(nameof(CurrentPriceText));
            }
        }

        public partial class AdvancedCatalogDetailViewModel : ObservableObject
        {
            public int ProductId { get; set; }

            private UnitOfMeasure? _uom;

            public UnitOfMeasure? UoM
            {
                get => _uom;
                set
                {
                    if (SetProperty(ref _uom, value))
                    {
                        UomText = value != null ? value.Name : string.Empty;
                    }
                }
            }

            [ObservableProperty] private double _expectedPrice;

            [ObservableProperty] private double _price;

            [ObservableProperty] private bool _isFromEspecial;

            [ObservableProperty] private OrderDetail? _detail;

            public int ItemType { get; set; }
            public int PriceLevelSelected { get; set; }
            public double InStore { get; set; }
            public string ExtraFields { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;

            [ObservableProperty] private string _quantityText = "0";

            [ObservableProperty] private string _priceText = "$0.00";

            [ObservableProperty] private string _uomText = string.Empty;

            partial void OnDetailChanged(OrderDetail? value)
            {
                if (value != null)
                {
                    QuantityText = value.Qty.ToString();
                    PriceText = value.Price.ToCustomString();
                    // Subscribe to property changes on the OrderDetail if possible
                    // For now, we'll update manually in the increment/decrement methods
                }
                else
                {
                    QuantityText = "0";
                    PriceText = Price.ToCustomString();
                }
            }
            
            // Method to update display when qty changes (ensures UI refreshes for subline row)
            public void UpdateQuantityDisplay()
            {
                if (Detail != null)
                {
                    var qtyStr = Detail.Qty.ToString();
                    var priceStr = Detail.Price.ToCustomString();
                    if (QuantityText != qtyStr) { QuantityText = qtyStr; OnPropertyChanged(nameof(QuantityText)); }
                    if (PriceText != priceStr) { PriceText = priceStr; OnPropertyChanged(nameof(PriceText)); }
                }
            }
        }

        public partial class AdvancedCatalogLineItemViewModel : ObservableObject
        {
            [ObservableProperty] private string _productName = string.Empty;

            [ObservableProperty] private string _qtyText = string.Empty;

            [ObservableProperty] private string _priceText = string.Empty;

            [ObservableProperty] private string _amountText = string.Empty;

            [ObservableProperty] private string _typeText = string.Empty;

            [ObservableProperty] private Microsoft.Maui.Graphics.Color _typeColor;

            [ObservableProperty] private bool _showType;

            public OrderDetail Detail { get; set; } = null!;
        }
    }
}
