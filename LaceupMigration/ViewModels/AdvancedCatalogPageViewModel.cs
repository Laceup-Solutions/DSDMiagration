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
        
        // Performance optimization: caching
        private List<AdvancedCatalogItemViewModel>? _cachedItems = null;
        private int? _cachedItemType = null;
        private int? _cachedCategoryId = null;
        private string? _cachedSearchFromCategories = null;
        private Dictionary<Product, List<InvoiceDetail>>? _cachedClientHistory = null;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceMs = 300;

        public ObservableCollection<AdvancedCatalogItemViewModel> Items { get; } = new();
        public ObservableCollection<AdvancedCatalogItemViewModel> FilteredItems { get; } = new();
        public ObservableCollection<AdvancedCatalogItemViewModel.AdvancedCatalogLineItemViewModel> LineItems { get; } = new();

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

        [ObservableProperty] private string _sortByText = "Sort: Product Name";

        [ObservableProperty] private string _filterText = "Filter";

        [ObservableProperty] private bool _showPrices = true;

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

            MainThread.BeginInvokeOnMainThread(async () =>
                await InitializeAsync(orderId, categoryId, search, itemType));
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
                // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            if (categoryId.HasValue)
                _category = Category.Find(categoryId.Value);

            if (!string.IsNullOrEmpty(search))
                _searchCriteriaFromCategories = search;

            if (itemType.HasValue)
                _itemType = itemType.Value;

            _initialized = true;
            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            OrderTypeText = GetOrderTypeText(_order);
            TermsText = "Terms: " + _order.Term;
            
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
                    item.UpdateDisplayProperties(_order);
                    Items.Add(item);
                }

                return;
            }

            var items = new List<AdvancedCatalogItemViewModel>();

            var products = Product.GetProductListForOrder(_order, _itemType > 0, categoryId);

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

            // Cache client history
            if (_cachedClientHistory == null)
            {
                if (_order.Client.ClientProductHistory == null)
                    _order.Client.ClientProductHistory = InvoiceDetail.GetProductHistoryDictionary(_order.Client);
                _cachedClientHistory = _order.Client.ClientProductHistory;
            }

            var clientSource = _cachedClientHistory;
            if (clientSource == null)
                return;

            foreach (var product in products)
            {
                var item = new AdvancedCatalogItemViewModel
                {
                    Product = product,
                    ItemType = _itemType,
                    Order = _order
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
                        Product = product,
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
                            Product = product,
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
                                    Product = product,
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
                            Product = product,
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

                // Set primary detail (prefer non-free detail for main controls)
                // This ensures the primary detail controls always show the regular priced item
                // PrimaryDetail can NEVER be a free item - always prefer non-free details
                // First, try to find a non-free detail with an OrderDetail
                var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem && 
                                    (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")));
                
                item.PrimaryDetail = nonFreeWithDetail;
                
                // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
                if (item.PrimaryDetail == null)
                {
                    item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
                }
                
                // If still null (only free items exist), create a new empty detail for adding new items
                // This ensures PrimaryDetail is always available for operations
                if (item.PrimaryDetail == null)
                {
                    // Use the first UoM or null if no UoMs
                    var defaultUom = product.UnitOfMeasures.FirstOrDefault();
                    var emptyDetail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                    {
                        Product = product,
                        UoM = defaultUom,
                        ExpectedPrice = Product.GetPriceForProduct(product, _order, _itemType > 0, _itemType == 1),
                        Price = Product.GetPriceForProduct(product, _order, _itemType > 0, _itemType == 1)
                    };
                    
                    // Apply UoM conversion if needed
                    if (defaultUom != null)
                    {
                        emptyDetail.ExpectedPrice *= defaultUom.Conversion;
                        emptyDetail.Price *= defaultUom.Conversion;
                    }
                    
                    item.Details.Add(emptyDetail);
                    item.PrimaryDetail = emptyDetail;
                }

                // Update display properties
                item.UpdateDisplayProperties(_order);
                
                // Access Details to trigger CollectionChanged handler which will notify property changes
                _ = item.Details;

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

            // Sync with existing order details for this item type
            foreach (var orderDetail in _order.Details)
            {
                var it = orderDetail.IsCredit ? (orderDetail.Damaged ? 1 : 2) : 0;
                if (it != _itemType)
                    continue;

                if (orderDetail.Product.ProductId != item.Product.ProductId)
                    continue;

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
                                    Product = item.Product,
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
                    if (Offer.ProductHasSpecialPriceForClient(item.Product, _order.Client, out price,
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
                        Product = item.Product,
                        Price = orderDetail.Price,
                        ExpectedPrice = orderDetail.ExpectedPrice,
                        UoM = orderDetail.UnitOfMeasure,
                        Detail = orderDetail,
                        ExtraFields = orderDetail.ExtraFields
                    };

                    double price = 0;
                    if (Offer.ProductHasSpecialPriceForClient(item.Product, _order.Client, out price,
                            orderDetail.UnitOfMeasure))
                    {
                        newDetail.IsFromEspecial = newDetail.Price == price;
                    }

                    item.Details.Add(newDetail);
                }
            }

            // Set PrimaryDetail - can NEVER be a free item
            var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem && 
                                (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")));
            
            item.PrimaryDetail = nonFreeWithDetail;
            
            // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
            if (item.PrimaryDetail == null)
            {
                item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
            }
            
            // If still null (only free items exist), create a new empty detail for adding new items
            if (item.PrimaryDetail == null)
            {
                // Use the first UoM or null if no UoMs
                var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault();
                var emptyDetail = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                {
                    Product = item.Product,
                    UoM = defaultUom,
                    ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                    Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                };
                
                // Apply UoM conversion if needed
                if (defaultUom != null)
                {
                    emptyDetail.ExpectedPrice *= defaultUom.Conversion;
                    emptyDetail.Price *= defaultUom.Conversion;
                }
                
                item.Details.Add(emptyDetail);
                item.PrimaryDetail = emptyDetail;
            }

            // Update primary detail
            // PrimaryDetail can NEVER be a free item
            item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.Qty > 0 && 
                                !d.Detail.IsFreeItem && (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")))
                                 ?? item.Details.FirstOrDefault(d => d.Detail == null);
        }

        private bool EqualsUom(UnitOfMeasure? uom1, UnitOfMeasure? uom2)
        {
            if (uom1 == null && uom2 == null)
                return true;
            if (uom1 != null && uom2 != null)
                return uom1.Id == uom2.Id;
            return false;
        }

        private List<AdvancedCatalogItemViewModel> SortByCriteria(SortCriteria criteria,
            List<AdvancedCatalogItemViewModel> source)
        {
            return criteria switch
            {
                SortCriteria.ProductName => source.OrderBy(x => x.Product.Name).ToList(),
                SortCriteria.ProductCode => source.OrderBy(x => x.Product.Code).ToList(),
                SortCriteria.Category => source.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).ToList(),
                SortCriteria.Qty => source.OrderByDescending(x => x.Details.Sum(d => d.Detail?.Qty ?? 0)).ToList(),
                SortCriteria.InStock => source.OrderByDescending(x => GetCurrentInventory(x.Product)).ToList(),
                SortCriteria.Descending => source.OrderByDescending(x => x.Product.Name).ToList(),
                SortCriteria.OrderOfEntry => source.OrderBy(x =>
                    x.Details.FirstOrDefault(d => d.Detail != null)?.Detail?.OrderDetailId ?? int.MaxValue).ToList(),
                SortCriteria.Description => source.OrderBy(x => x.Product.Description).ToList(),
                _ => source.OrderBy(x => x.Product.Name).ToList()
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
            var selected = await _dialogService.ShowActionSheetAsync("Filter", "Cancel", null, options.ToArray());

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
            var selected = await _dialogService.ShowActionSheetAsync("Sort By", "Cancel", null, options);

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
                if ((_currentFilter & 2) > 0) // In Stock
                    filtered = filtered.Where(x => x.Product.CurrentWarehouseInventory > 0);

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
                        offerProductIds.Contains(x.Product.ProductId) ||
                        (x.Product.DiscountCategoryId > 0 && offerCategoryIds.Contains(x.Product.DiscountCategoryId)));
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(_searchCriteria))
                {
                    var searchLower = _searchCriteria.Replace(" ", "").ToLowerInvariant();
                    filtered = filtered.Where(x =>
                        x.Product.Name.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.Product.Upc.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.Product.Sku.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.Product.Description.ToLowerInvariant().Replace(" ", "").Contains(searchLower) ||
                        x.Product.Code.ToLowerInvariant().Replace(" ", "").Contains(searchLower)
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
            CreditAmountText = $"Credit: {creditAmount.ToCustomString()}";

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
        private async Task DoneAsync()
        {
            var canNavigate = await FinalizeOrderAsync();
            if (canNavigate)
            {
                await Shell.Current.GoToAsync("..");
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
                    // Set search query to the barcode to filter products
                    SearchQuery = scanResult;

                    // Wait for the filter to complete (debounce is 300ms, wait a bit longer to be safe)
                    await Task.Delay(500);

                    // If we have an order, navigate to add item page (same as FullCategoryPageViewModel)
                    var route = $"additem?orderId={_order.OrderId}&productId={product.ProductId}";
                    if (_itemType == 1) // Dump
                        route += "&asCreditItem=1";
                    if (_itemType == 2) // Return
                        route += "&asReturnItem=1";
                    await Shell.Current.GoToAsync(route);
                }
                else
                {
                    // Product not found, but set search query anyway (same as FullCategoryPageViewModel)
                    SearchQuery = scanResult;

                    // Wait for the filter to complete
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

        private async Task AddItemFromScannerAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel detail, bool excludeOffer)
        {
            if (_order == null || detail.Product == null)
                return;

            var currentOH = detail.Product.GetInventory(_order.AsPresale, false);
            var baseQty = 1.0;
            if (detail.UoM != null)
                baseQty *= detail.UoM.Conversion;

            var isCredit = detail.Detail != null ? detail.Detail.IsCredit : false;
            if (!Config.CanGoBelow0 && !isCredit && _itemType == 0)
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
                    var hasOffer = _order.ProductHasOffer(detail.Product, detail.UoM);
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
                detail.Detail = new OrderDetail(detail.Product, 0, _order);
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

            // Update the item's display properties without full refresh
            var item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
            if (item != null)
            {
                // PrimaryDetail can NEVER be a free item
                item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.Qty > 0 && 
                                    !d.Detail.IsFreeItem && (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")))
                                     ?? item.Details.FirstOrDefault(d => d.Detail == null);
                item.UpdateDisplayProperties(_order);
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

            if (detail.Detail == null)
            {
                detail.Detail = new OrderDetail(detail.Product, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }

            var isFreeItem = detail.Detail.IsFreeItem || 
                            (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree"));

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
                // Find the detail in the item's Details collection by OrderDetailId
                var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                
                // If operating on a free item, don't change PrimaryDetail
                // If operating on primary detail, ensure it's not a free item
                if (isFreeItem && detailInItem != null)
                {
                    // Just update display for free item - update the detail's display properties
                    detailInItem.UpdateQuantityDisplay();
                    item.UpdateDisplayProperties(_order);
                }
                else if (!isFreeItem)
                {
                    // Ensure PrimaryDetail is not a free item
                    // PrimaryDetail can NEVER be a free item
                    item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                        (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")) &&
                        d.Detail.Qty > 0)
                        ?? item.Details.FirstOrDefault(d => d.Detail == null);
                    item.UpdateDisplayProperties(_order);
                }
            }
        }

        [RelayCommand]
        private async Task DecrementQuantityAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || detail.Detail == null || _order == null)
            {
                return;
            }

            if (_relatedLines.Any(x => x == detail.Detail.OrderDetailId) && !Config.AllowEditRelated)
                return;

            var isFreeItem = detail.Detail.IsFreeItem || 
                            (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree"));

            _order.UpdateInventory(detail.Detail, 1);
            detail.Detail.Qty--;
            
            if (detail.Detail.Qty <= 0)
            {
                // Remove from order
                var orderDetailId = detail.Detail.OrderDetailId;
                _order.Details.Remove(detail.Detail);
                
                // Remove from item.Details collection
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
                if (item != null)
                {
                    // Find and remove the detail from the collection
                    var detailToRemove = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailToRemove != null)
                    {
                        item.Details.Remove(detailToRemove);
                    }
                    detail.Detail = null;
                    
                    // Ensure PrimaryDetail is not a free item - always use non-free detail
                    // If no details exist, create one for the primary controls
                    if (item.Details.Count == 0)
                    {
                        // Create a detail without OrderDetail for primary controls
                        // Use the same logic as PrepareList() to create details
                        var primaryDetailEntry = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                        {
                            Product = item.Product,
                            ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                            Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                        };
                        
                        // Set UoM if product has UoMs
                        if (item.Product.UnitOfMeasures.Count > 0)
                        {
                            var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                            ?? item.Product.UnitOfMeasures.FirstOrDefault();
                            if (defaultUom != null)
                            {
                                primaryDetailEntry.UoM = defaultUom;
                                primaryDetailEntry.ExpectedPrice *= defaultUom.Conversion;
                                primaryDetailEntry.Price *= defaultUom.Conversion;
                            }
                        }
                        
                        item.Details.Add(primaryDetailEntry);
                        item.PrimaryDetail = primaryDetailEntry;
                    }
                    else
                    {
                        // Ensure PrimaryDetail is not a free item - always use non-free detail
                        // PrimaryDetail can NEVER be a free item
                        var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                            (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")) &&
                            d.Detail.Qty > 0);
                        
                        item.PrimaryDetail = nonFreeWithDetail;
                        
                        // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
                        if (item.PrimaryDetail == null)
                        {
                            item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
                        }
                        
                        // If still null (only free items exist), create a new empty detail for adding new items
                        // This ensures PrimaryDetail is always available and never a free item
                        if (item.PrimaryDetail == null)
                        {
                            var primaryDetailEntry = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                            {
                                Product = item.Product,
                                ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                                Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                            };
                            
                            // Set UoM if product has UoMs
                            if (item.Product.UnitOfMeasures.Count > 0)
                            {
                                var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                                ?? item.Product.UnitOfMeasures.FirstOrDefault();
                                if (defaultUom != null)
                                {
                                    primaryDetailEntry.UoM = defaultUom;
                                    primaryDetailEntry.ExpectedPrice *= defaultUom.Conversion;
                                    primaryDetailEntry.Price *= defaultUom.Conversion;
                                }
                            }
                            
                            item.Details.Add(primaryDetailEntry);
                            item.PrimaryDetail = primaryDetailEntry;
                        }
                    }
                    
                    item.UpdateDisplayProperties(_order);
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
                
                // Update the item's display properties
                // Find item by matching OrderDetailId (not by reference, since detail might be from different instance)
                var orderDetailId = detail.Detail.OrderDetailId;
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
                
                if (item != null)
                {
                    // Find the detail in the item's Details collection by OrderDetailId
                    var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    
                    // Ensure PrimaryDetail is not a free item
                    if (isFreeItem && detailInItem != null)
                    {
                        // Update the detail's display properties
                        detailInItem.UpdateQuantityDisplay();
                        item.UpdateDisplayProperties(_order);
                    }
                    else if (!isFreeItem)
                    {
                        // Ensure PrimaryDetail is not a free item - always use non-free detail
                        // PrimaryDetail can NEVER be a free item
                        var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                            (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")) &&
                            d.Detail.Qty > 0);
                        
                        item.PrimaryDetail = nonFreeWithDetail;
                        
                        // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
                        if (item.PrimaryDetail == null)
                        {
                            item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
                        }
                        
                        // If still null (only free items exist), create a new empty detail for adding new items
                        // This ensures PrimaryDetail is always available and never a free item
                        if (item.PrimaryDetail == null)
                        {
                            var primaryDetailEntry = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                            {
                                Product = item.Product,
                                ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                                Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                            };
                            
                            // Set UoM if product has UoMs
                            if (item.Product.UnitOfMeasures.Count > 0)
                            {
                                var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                                ?? item.Product.UnitOfMeasures.FirstOrDefault();
                                if (defaultUom != null)
                                {
                                    primaryDetailEntry.UoM = defaultUom;
                                    primaryDetailEntry.ExpectedPrice *= defaultUom.Conversion;
                                    primaryDetailEntry.Price *= defaultUom.Conversion;
                                }
                            }
                            
                            item.Details.Add(primaryDetailEntry);
                            item.PrimaryDetail = primaryDetailEntry;
                        }
                        
                        item.UpdateDisplayProperties(_order);
                    }
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditQuantityAsync(AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel? detail)
        {
            if (detail == null || _order == null || !CanEdit)
                return;

            var currentQty = detail.Detail?.Qty ?? 0;
            var qtyText = await _dialogService.ShowPromptAsync("Enter Quantity", "Quantity", "OK", "Cancel", "", -1,
                currentQty.ToString());

            if (string.IsNullOrEmpty(qtyText) || !float.TryParse(qtyText, out var qty))
                return;

            if (qty < 0)
                qty = 0;

            var isFreeItem = detail.Detail != null && (detail.Detail.IsFreeItem || 
                            (!string.IsNullOrEmpty(detail.Detail.ExtraFields) && detail.Detail.ExtraFields.Contains("productfree")));

            if (detail.Detail == null)
            {
                if (qty == 0)
                    return;

                detail.Detail = new OrderDetail(detail.Product, 0, _order);
                detail.Detail.ExpectedPrice = detail.ExpectedPrice;
                detail.Detail.Price = detail.Price;
                detail.Detail.UnitOfMeasure = detail.UoM;
                detail.Detail.IsCredit = _itemType > 0;
                detail.Detail.Damaged = _itemType == 1;
                _order.Details.Add(detail.Detail);
            }

            var oldQty = detail.Detail.Qty;
            _order.UpdateInventory(detail.Detail, (int)oldQty);
            detail.Detail.Qty = qty;
            _order.UpdateInventory(detail.Detail, (int)-qty);

            if (detail.Detail.Qty <= 0)
            {
                // Remove from order
                var orderDetailId = detail.Detail.OrderDetailId;
                _order.Details.Remove(detail.Detail);
                
                // Remove from item.Details collection
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d == detail));
                if (item != null)
                {
                    // Find and remove the detail from the collection
                    var detailToRemove = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                    if (detailToRemove != null)
                    {
                        item.Details.Remove(detailToRemove);
                    }
                    detail.Detail = null;
                    
                    // Ensure PrimaryDetail is not a free item - always use non-free detail
                    // PrimaryDetail can NEVER be a free item
                    var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                        (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")) &&
                        d.Detail.Qty > 0);
                    
                    item.PrimaryDetail = nonFreeWithDetail;
                    
                    // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
                    if (item.PrimaryDetail == null)
                    {
                        item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
                    }
                    
                    // If still null (only free items exist), create a new empty detail for adding new items
                    // This ensures PrimaryDetail is always available and never a free item
                    if (item.PrimaryDetail == null)
                    {
                        var primaryDetailEntry = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                        {
                            Product = item.Product,
                            ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                            Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                        };
                        
                        // Set UoM if product has UoMs
                        if (item.Product.UnitOfMeasures.Count > 0)
                        {
                            var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                            ?? item.Product.UnitOfMeasures.FirstOrDefault();
                            if (defaultUom != null)
                            {
                                primaryDetailEntry.UoM = defaultUom;
                                primaryDetailEntry.ExpectedPrice *= defaultUom.Conversion;
                                primaryDetailEntry.Price *= defaultUom.Conversion;
                            }
                        }
                        
                        item.Details.Add(primaryDetailEntry);
                        item.PrimaryDetail = primaryDetailEntry;
                    }
                    
                    item.UpdateDisplayProperties(_order);
                    LoadLineItems(); // Update line items list
                }
                else
                {
                    detail.Detail = null;
                }
            }
            else
            {
                // Update the item's display properties
                // Find item by matching OrderDetailId (not by reference, since detail might be from different instance)
                var orderDetailId = detail.Detail.OrderDetailId;
                var item = Items.FirstOrDefault(i => i.Details.Any(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId));
                
                if (item != null)
                {
                    if (isFreeItem)
                    {
                        // Find the detail in the item's Details collection by OrderDetailId
                        var detailInItem = item.Details.FirstOrDefault(d => d.Detail != null && d.Detail.OrderDetailId == orderDetailId);
                        
                        if (detailInItem != null)
                        {
                            // Update the detail's display properties (quantity display)
                            detailInItem.UpdateQuantityDisplay();
                        }
                        
                        // Don't change PrimaryDetail if we're operating on a free item
                        item.UpdateDisplayProperties(_order);
                        LoadLineItems(); // Update line items list to reflect quantity changes
                    }
                    else
                    {
                        // Ensure PrimaryDetail is not a free item - always use non-free detail
                        // PrimaryDetail can NEVER be a free item
                        var nonFreeWithDetail = item.Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                            (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")) &&
                            d.Detail.Qty > 0);
                        
                        item.PrimaryDetail = nonFreeWithDetail;
                        
                        // If no non-free detail with OrderDetail, try to find an empty detail (Detail == null)
                        if (item.PrimaryDetail == null)
                        {
                            item.PrimaryDetail = item.Details.FirstOrDefault(d => d.Detail == null);
                        }
                        
                        // If still null (only free items exist), create a new empty detail for adding new items
                        // This ensures PrimaryDetail is always available and never a free item
                        if (item.PrimaryDetail == null)
                        {
                            var primaryDetailEntry = new AdvancedCatalogItemViewModel.AdvancedCatalogDetailViewModel
                            {
                                Product = item.Product,
                                ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                                Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                            };
                            
                            // Set UoM if product has UoMs
                            if (item.Product.UnitOfMeasures.Count > 0)
                            {
                                var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                                ?? item.Product.UnitOfMeasures.FirstOrDefault();
                                if (defaultUom != null)
                                {
                                    primaryDetailEntry.UoM = defaultUom;
                                    primaryDetailEntry.ExpectedPrice *= defaultUom.Conversion;
                                    primaryDetailEntry.Price *= defaultUom.Conversion;
                                }
                            }
                            
                            item.Details.Add(primaryDetailEntry);
                            item.PrimaryDetail = primaryDetailEntry;
                        }
                        
                        item.UpdateDisplayProperties(_order);
                        LoadLineItems(); // Update line items list
                    }
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();
            RefreshTotals();
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

            // Ensure detail exists
            if (detail.Detail == null)
            {
                detail.Detail = new OrderDetail(detail.Product, 0, _order);
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
            item.UpdateDisplayProperties(_order);
            RefreshTotals();
        }

        [RelayCommand]
        private async Task SelectPriceLevelAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || _order == null)
                return;

            var detail = item.PrimaryDetail;
            var currentPrice = detail.Detail != null ? detail.Detail.Price : detail.Price;
            var currentPriceLevelSelected =
                detail.Detail != null ? detail.Detail.PriceLevelSelected : detail.PriceLevelSelected;
            var currentComments = detail.Detail != null ? detail.Detail.Comments : detail.Comment;

            // Show price level selection dialog
            var result = await _dialogService.ShowPriceLevelDialogAsync(
                item.Product.Name,
                item.Product,
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
                    detail.Detail = new OrderDetail(detail.Product, 0, _order);
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
            item.UpdateDisplayProperties(_order);
            RefreshTotals();
        }

        [RelayCommand]
        public async Task AddFreeItemAsync(AdvancedCatalogItemViewModel? item)
        {
            if (item == null || item.PrimaryDetail == null || _order == null) return;

            var detail = item.PrimaryDetail;

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
                d.Product.ProductId == item.Product.ProductId && 
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
            var newDetail = new OrderDetail(detail.Product, qtyToUse, _order);
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
                    x.Product.ProductId == item.Product.ProductId && !x.IsFreeItem &&
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
                    Product = item.Product,
                    ExpectedPrice = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1),
                    Price = Product.GetPriceForProduct(item.Product, _order, _itemType > 0, _itemType == 1)
                };
                
                // Set UoM if product has UoMs
                if (item.Product.UnitOfMeasures.Count > 0)
                {
                    var defaultUom = item.Product.UnitOfMeasures.FirstOrDefault(u => u.IsDefault) 
                                    ?? item.Product.UnitOfMeasures.FirstOrDefault();
                    if (defaultUom != null)
                    {
                        emptyDetail.UoM = defaultUom;
                        emptyDetail.ExpectedPrice *= defaultUom.Conversion;
                        emptyDetail.Price *= defaultUom.Conversion;
                    }
                }
                
                item.Details.Add(emptyDetail);
            }
            
            item.PrimaryDetail = emptyDetail;
            
            // Optimize: Update the specific item instead of rebuilding the entire list
            // This is much faster than PrepareList() which rebuilds everything
            UpdateItemFromOrderDetails(item);
            item.UpdateDisplayProperties(_order);
            
            // Update totals and line items in background
            RefreshTotals();
            LoadLineItems();
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
            var choice = await _dialogService.ShowActionSheetAsync("Search", "Cancel", null, searchOptions);

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

            // Show the grid only if at least one button is visible
            ShowButtonGrid = visibleCount > 0;

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

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success");

                // [ACTIVITY STATE]: Remove state when properly exiting
                var route = "advancedcatalog";
                if (_order != null)
                {
                    route += $"?orderId={_order.OrderId}";
                }
                Helpers.NavigationHelper.RemoveNavigationState(route);

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
            // and returns to the previous one (ClientDetailsPage)
            await Shell.Current.GoToAsync("..");
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
                            await ContinueSendingOrderAsync();
                            return false; // ContinueSendingOrderAsync already navigates, don't navigate again
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

            var choice =
                await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null,
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

            var finalized = _order.Finished;
            var allowDiscount = _order.Client.UseDiscount;
            var asPresale = _order.AsPresale;

            // Add Send Order as first option if applicable
            if (ShowSendButton && CanEdit)
            {
                options.Add(new MenuOption("Send Order", async () => { await SendOrderAsync(); }));
            }

            if (asPresale)
            {
                // Presale menu items
                if (allowDiscount && !_order.Locked())
                {
                    options.Add(new MenuOption("Add Discount",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync(
                                "Add Discount functionality is not yet fully implemented.", "Info");
                        }));
                }

                if (Config.SetPO && (_order.OrderType != OrderType.Order || Config.POIsMandatory))
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:",
                            initialValue: _order.PONumber ?? string.Empty);
                        if (!string.IsNullOrWhiteSpace(po))
                        {
                            _order.PONumber = po;
                            _order.Save();
                            await _dialogService.ShowAlertAsync("PO number set.", "Success");
                        }
                    }));
                }

                if (Config.PrinterAvailable)
                {
                    options.Add(new MenuOption("Print",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented.",
                                "Info");
                        }));
                }

                options.Add(new MenuOption("Set Ship Date", async () =>
                {
                    var currentShipDate = _order.ShipDate.Year == 1 ? DateTime.Now : _order.ShipDate;
                    var selectedDate =
                        await _dialogService.ShowDatePickerAsync("Set Ship Date", currentShipDate, DateTime.Now, null);
                    if (selectedDate.HasValue)
                    {
                        _order.ShipDate = selectedDate.Value;
                        _order.Save();
                    }
                }));

                if (!(_order.Client.SplitInvoices.Count > 0))
                {
                    options.Add(new MenuOption("Send by Email",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync(
                                "Send by Email functionality is not yet fully implemented.", "Info");
                        }));

                    options.Add(new MenuOption("Share PDF",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.",
                                "Info");
                        }));
                }
            }
            else
            {
                // Previously ordered menu items
                if ((Config.SetPO || Config.POIsMandatory) && !finalized)
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:",
                            initialValue: _order.PONumber ?? string.Empty);
                        if (!string.IsNullOrWhiteSpace(po))
                        {
                            _order.PONumber = po;
                            _order.Save();
                            await _dialogService.ShowAlertAsync("PO number set.", "Success");
                        }
                    }));
                }

                if (allowDiscount && !finalized && !_order.Locked())
                {
                    options.Add(new MenuOption("Add Discount",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync(
                                "Add Discount functionality is not yet fully implemented.", "Info");
                        }));
                }

                if (!Config.LockOrderAfterPrinted)
                {
                    var isSplitClient = _order.Client.SplitInvoices.Count > 0;
                    if (!isSplitClient || _order.Finished)
                    {
                        options.Add(new MenuOption("Print",
                            async () =>
                            {
                                await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented.",
                                    "Info");
                            }));
                    }
                }

                if (!(_order.Client.SplitInvoices.Count > 0))
                {
                    options.Add(new MenuOption("Send by Email",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync(
                                "Send by Email functionality is not yet fully implemented.", "Info");
                        }));

                    options.Add(new MenuOption("Share PDF",
                        async () =>
                        {
                            await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.",
                                "Info");
                        }));
                }
            }

            // Common menu items
            options.Add(new MenuOption("Add Comments", async () =>
            {
                var comments = await _dialogService.ShowPromptAsync("Add Comments", "Enter comments:",
                    initialValue: _order.Comments ?? string.Empty);
                if (comments != null)
                {
                    _order.Comments = comments;
                    _order.Save();
                    await _dialogService.ShowAlertAsync("Comments saved.", "Success");
                }
            }));

            // Note: "Advanced Options" is already added by LaceupContentPage.GetCommonMenuOptions()
            // Don't add it here to avoid duplication

            return options;
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
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
                    // [ACTIVITY STATE]: Remove state when properly exiting
                    var route = "advancedcatalog";
                    if (_order != null)
                    {
                        route += $"?orderId={_order.OrderId}";
                    }
                    Helpers.NavigationHelper.RemoveNavigationState(route);
                    
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
                            // [ACTIVITY STATE]: Remove state when properly exiting
                            var route = "advancedcatalog";
                            if (_order != null)
                            {
                                route += $"?orderId={_order.OrderId}";
                            }
                            Helpers.NavigationHelper.RemoveNavigationState(route);
                            
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
                    // [ACTIVITY STATE]: Remove state when properly exiting
                    var route = "advancedcatalog";
                    if (_order != null)
                    {
                        route += $"?orderId={_order.OrderId}";
                    }
                    Helpers.NavigationHelper.RemoveNavigationState(route);
                    
                    await Shell.Current.GoToAsync("..");
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
        public Product Product { get; set; } = null!;
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
            OnPropertyChanged(nameof(ShowFreeItemDetail));
            OnPropertyChanged(nameof(DetailsWithOrderDetail));
            OnPropertyChanged(nameof(FreeItemDetail));
            OnPropertyChanged(nameof(CanAddFreeItem));
            OnPropertyChanged(nameof(FreeItemButtonText));
            OnPropertyChanged(nameof(FreeItemButtonColor));
        }
        
        public Order? Order { get; set; }
        
        private AdvancedCatalogDetailViewModel? _primaryDetail;

        public AdvancedCatalogDetailViewModel? PrimaryDetail
        {
            get => _primaryDetail;
            set
            {
                if (SetProperty(ref _primaryDetail, value))
                {
                    UpdateDisplayProperties(Order);
                }
            }
        }

        [ObservableProperty] private string _productDisplayName = string.Empty;

        [ObservableProperty] private string _onHandText = string.Empty;

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

        public bool HasMultipleDetails => HasFreeItemDetail;
        public bool HasSingleDetail => !HasFreeItemDetail;
        
        public bool HasFreeItemDetail => Details.Any(d => d.Detail != null && (d.Detail.IsFreeItem || 
            (!string.IsNullOrEmpty(d.Detail.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"))));
        
        // Show free item detail section only if free item exists AND it's a Sales item in Order order
        public bool ShowFreeItemDetail => HasFreeItemDetail && ShowFreeItemButton;
        
        // Only show details that have OrderDetail attached (for sublines display)
        // This includes free items - they show as sublines
        // DetailsWithOrderDetail should only show free items as sublines
        // Primary detail should NOT be included here, even if it has an OrderDetail
        public IEnumerable<AdvancedCatalogDetailViewModel> DetailsWithOrderDetail => 
            Details.Where(d => d.Detail != null && 
                (d.Detail.IsFreeItem || (!string.IsNullOrEmpty(d.Detail.ExtraFields) && d.Detail.ExtraFields.Contains("productfree"))));
        
        // Single free item detail for Grid binding (first free item)
        public AdvancedCatalogDetailViewModel? FreeItemDetail => 
            DetailsWithOrderDetail.FirstOrDefault();

        public void UpdateDisplayProperties(Order? order = null, bool skipFreeItemUpdate = false)
        {
            // Use stored order if not provided
            order ??= Order;
            {
                if (Product == null)
                    return;

                // Product Display Name (Code + Name)
                ProductDisplayName = $"{Product.Code} {Product.Name}";

                // On-Hand
                var oh = Product.CurrentWarehouseInventory;
                OnHandText = $"OH: {oh:F0}";

                // List Price
                var listPrice = Product.PriceLevel0;
                ListPriceText = $"List Price: {listPrice.ToCustomString()}";

                // ShowFreeItemButton: only visible for Sales items (ItemType == 0) and OrderType.Order (not Credit or Return)
                ShowFreeItemButton = order != null && 
                                     order.OrderType == OrderType.Order && 
                                     ItemType == 0;

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

                // Current Price (from primary detail or product)
                // Ensure primary detail is not a free item - use non-free detail for main controls
                if (PrimaryDetail != null && PrimaryDetail.Detail != null)
                {
                    // If primary detail is free, find a non-free one
                    if (PrimaryDetail.Detail.IsFreeItem || 
                        (!string.IsNullOrEmpty(PrimaryDetail.Detail.ExtraFields) && 
                         PrimaryDetail.Detail.ExtraFields.Contains("productfree")))
                    {
                        var nonFreeDetail = Details.FirstOrDefault(d => d.Detail != null && !d.Detail.IsFreeItem &&
                            (string.IsNullOrEmpty(d.Detail.ExtraFields) || !d.Detail.ExtraFields.Contains("productfree")));
                        if (nonFreeDetail != null)
                        {
                            PrimaryDetail = nonFreeDetail;
                        }
                        else
                        {
                            // No non-free detail exists, use a detail without OrderDetail
                            var emptyDetail = Details.FirstOrDefault(d => d.Detail == null && d.UoM?.Id == PrimaryDetail.UoM?.Id)
                                             ?? Details.FirstOrDefault(d => d.Detail == null)
                                             ?? PrimaryDetail;
                            PrimaryDetail = emptyDetail;
                        }
                    }
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
                    var price = Product.GetPriceForProduct(Product, client, ItemType > 0, ItemType == 1);
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

                // Product Image
                var imagePath = ProductImage.GetProductImage(Product.ProductId);
                HasImage = !string.IsNullOrEmpty(imagePath);
                ProductImagePath = imagePath ?? string.Empty;
            }
        }

        public partial class AdvancedCatalogDetailViewModel : ObservableObject
        {
            public Product Product { get; set; } = null!;

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
            
            // Method to update display when qty changes
            public void UpdateQuantityDisplay()
            {
                if (Detail != null)
                {
                    QuantityText = Detail.Qty.ToString();
                    PriceText = Detail.Price.ToCustomString();
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
