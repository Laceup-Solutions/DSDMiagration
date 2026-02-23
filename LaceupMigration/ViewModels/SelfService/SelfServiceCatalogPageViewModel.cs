using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration.Business.Interfaces;
using LaceupMigration;
using LaceupMigration.Helpers;
using LaceupMigration.ViewModels;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCatalogPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private readonly AdvancedOptionsService _advancedOptionsService;

        private Order _order;
        private Category _category;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _categoryName = "All Categories";

        /// <summary>Full list of catalog items (with sublines). Same model as ProductCatalogPage.</summary>
        public ObservableCollection<CatalogItemViewModel> Products { get; } = new();
        /// <summary>Filtered by search; bound to the list. Same as ProductCatalogPage.</summary>
        public ObservableCollection<CatalogItemViewModel> FilteredProducts { get; } = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _pageTitle = "Products";

        [ObservableProperty]
        private bool _showImages = true;

        [ObservableProperty]
        private string _filterText = "Filter: All";

        [ObservableProperty]
        private string _sortByText = "Sort: Name";

        /// <summary>When true (UseLaceupAdvancedCatalogKey=1), cells use Advanced Catalog style (image, OH green, price blue, [-] qty [+]). When false, ProductCatalog style with sublines.</summary>
        [ObservableProperty]
        private bool _useAdvancedCatalogStyle;

        /// <summary>When false (HidePriceInSelfServiceKey=1), all prices and totals are hidden in self service.</summary>
        [ObservableProperty]
        private bool _showPrices = true;

        /// <summary>When false (HideOHinSelfServiceKey=1), all on-hand/inventory labels are hidden in self service.</summary>
        [ObservableProperty]
        private bool _showOnHand = true;

        private string _searchCriteria = string.Empty;

        private int _currentFilter = 0; // 0 = All, 2 = In Stock, 4 = Not In Order, etc.
        private int _currentSort = 0; // 0 = Name, 1 = Category, 2 = In Stock
        private bool _listInitialized;
        private bool _atLeastOneImage;

        /// <summary>True while the product list is loading (page shows first, then list).</summary>
        [ObservableProperty]
        private bool _isLoadingList;

        /// <summary>Set when navigating with productId (e.g. from scan in categories) or when scan matches an item; view scrolls to it. Match AdvancedCatalogPageViewModel.ScannedItemToFocus.</summary>
        [ObservableProperty]
        private CatalogItemViewModel? _scannedItemToFocus;

        private int? _productIdToFocus; // From query (e.g. scan from categories page); applied after list loads.
        private bool _fromCheckout; // True when opened from Checkout via Search/scan (no categories in stack); back only pops once.

        public SelfServiceCatalogPageViewModel(IDialogService dialogService, ILaceupAppService appService, ICameraBarcodeScannerService cameraBarcodeScanner, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            _advancedOptionsService = advancedOptionsService;
            
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
            ShowPrices = !Config.HidePriceInSelfService;
            ShowOnHand = !Config.HideOHinSelfService;
        }

        public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                    ClientName = _order.Client?.ClientName ?? string.Empty;
            }

            if (query.TryGetValue("categoryId", out var categoryIdObj) && int.TryParse(categoryIdObj?.ToString(), out var categoryId))
            {
                _category = Category.Categories.FirstOrDefault(x => x.CategoryId == categoryId);
                if (_category != null)
                    CategoryName = _category.Name;
            }

            if (query.TryGetValue("productSearch", out var productSearchObj) && productSearchObj is string productSearch && !string.IsNullOrWhiteSpace(productSearch))
            {
                try { productSearch = Uri.UnescapeDataString(productSearch); } catch { }
                SearchText = productSearch.Trim();
                SearchQuery = SearchText;
            }

            if (query.TryGetValue("productId", out var productIdObj) && productIdObj != null && int.TryParse(productIdObj.ToString(), out var productId) && productId > 0)
            {
                _productIdToFocus = productId;
            }

            _fromCheckout = false;
            if (query.TryGetValue("fromCheckout", out var fromCheckoutObj) && fromCheckoutObj != null &&
                (string.Equals(fromCheckoutObj.ToString(), "1", StringComparison.Ordinal) || string.Equals(fromCheckoutObj.ToString(), "true", StringComparison.OrdinalIgnoreCase)))
            {
                _fromCheckout = true;
            }

            PageTitle = _category != null ? _category.Name : "Products";
            _searchCriteria = SearchText?.Trim() ?? string.Empty;
        }

        public void OnAppearing()
        {
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
            ShowPrices = !Config.HidePriceInSelfService;
            ShowOnHand = !Config.HideOHinSelfService;
            if (_order == null) return;
            _ = LoadCatalogAsync();
        }

        /// <summary>Load catalog list asynchronously. Heavy work (BuildCatalogItemsList) runs on background thread; only assigning to Products and ApplyFilter run on UI thread.</summary>
        private async Task LoadCatalogAsync()
        {
            if (_order == null) return;
            IsLoadingList = true;
            try
            {
                var (items, atLeastOneImage) = await Task.Run(() => BuildCatalogItemsList()).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Products.Clear();
                    if (items != null && items.Count > 0)
                        foreach (var item in items) Products.Add(item);
                    _atLeastOneImage = atLeastOneImage;
                    ShowImages = _atLeastOneImage;
                    _listInitialized = true;
                    ApplyFilter();
                    // When opened with productId (e.g. from scan in categories), scroll to that product once list is ready.
                    if (_productIdToFocus.HasValue)
                    {
                        var itemToFocus = FilteredProducts.FirstOrDefault(i => i.ProductId == _productIdToFocus.Value);
                        if (itemToFocus != null)
                        {
                            HighlightItemOnly(itemToFocus);
                            ScannedItemToFocus = itemToFocus;
                        }
                        _productIdToFocus = null;
                    }
                    IsLoadingList = false;
                }).ConfigureAwait(false);
            }
            finally
            {
                if (IsLoadingList) await MainThread.InvokeOnMainThreadAsync(() => IsLoadingList = false).ConfigureAwait(false);
            }
        }

        /// <summary>Builds the catalog items list and atLeastOneImage flag. Safe to run on background thread; does not touch Products collection.</summary>
        private (List<CatalogItemViewModel> items, bool atLeastOneImage) BuildCatalogItemsList()
        {
            if (_order == null) return (new List<CatalogItemViewModel>(), false);

            var categoryId = _category?.CategoryId ?? 0;
            var productsForOrder = Product.GetProductListForOrder(_order, false, categoryId).ToList();

            if ((_currentFilter & 2) != 0)
                productsForOrder = productsForOrder.Where(x => x.OnHand > 0).ToList();
            if ((_currentFilter & 4) != 0)
            {
                var orderProductIds = _order.Details.Where(d => !d.IsCredit).Select(x => x.Product.ProductId).ToList();
                productsForOrder = productsForOrder.Where(x => !orderProductIds.Contains(x.ProductId)).ToList();
            }
            if ((_currentFilter & 8) != 0)
            {
                var orderProductIds = _order.Details.Where(d => !d.IsCredit).Select(x => x.Product.ProductId).ToList();
                productsForOrder = productsForOrder.Where(x => orderProductIds.Contains(x.ProductId)).ToList();
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                productsForOrder = productsForOrder.Where(x =>
                    (x.Name?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (x.Upc?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (x.Sku?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (x.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (x.Code?.ToLowerInvariant().Contains(searchLower) ?? false)).ToList();
            }

            switch (_currentSort)
            {
                case 1: productsForOrder = productsForOrder.OrderBy(x => x.CategoryId).ThenBy(x => x.Name).ToList(); break;
                case 2: productsForOrder = productsForOrder.OrderByDescending(x => x.OnHand).ThenBy(x => x.Name).ToList(); break;
                default: productsForOrder = productsForOrder.OrderBy(x => x.Name).ToList(); break;
            }

            if (_order.Client?.ClientProductHistory == null && _order.Client != null)
                _order.Client.ClientProductHistory = InvoiceDetail.GetProductHistoryDictionary(_order.Client);
            var clientSource = _order.Client?.ClientProductHistory ?? new Dictionary<Product, List<InvoiceDetail>>();
            var catalogItems = new List<CatalogItemViewModel>();

            foreach (var product in productsForOrder)
            {
                var img = ProductImage.GetProductImage(product.ProductId);
                var catalogItem = new CatalogItemViewModel
                {
                    ProductId = product.ProductId, 
                    OrderId = _order.OrderId, 
                    ProductName = product.Name,
                    Upc = product.Upc ?? "",
                    Sku = product.Sku ?? "",
                    Code = product.Code ?? "",
                    ProductImg = img,
                    HasImage = !string.IsNullOrEmpty(img),
                    Inventory = Math.Round(product.GetInventory(_order.AsPresale), 2).ToString()
                };
                
                var clientSourceKey = clientSource.Keys.FirstOrDefault(x => x.ProductId == product.ProductId);
                catalogItem.Line.History = clientSourceKey != null ? clientSource[clientSourceKey].OrderByDescending(x => x.Date).ToList() : new List<InvoiceDetail>();
                catalogItem.Line.LastInvoiceDetail = catalogItem.Line.History?.FirstOrDefault();
                catalogItem.Line.Product = product;
                catalogItem.Line.ExpectedPrice = Product.GetPriceForProduct(product, _order, false, false);
                catalogItem.Line.IsCredit = false;

                if (!string.IsNullOrEmpty(product.UoMFamily))
                {
                    var uom = _order.Client?.UseBaseUoM == true
                        ? UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsBase)
                        : UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsDefault);
                    if (Config.UseLastUoM && catalogItem.Line.LastInvoiceDetail != null)
                        uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == catalogItem.Line.LastInvoiceDetail.UnitOfMeasureId) ?? uom;
                    catalogItem.Line.UoM = uom;
                    if (catalogItem.Line.UoM != null)
                        catalogItem.Line.ExpectedPrice *= catalogItem.Line.UoM.Conversion;
                }
                if (Config.UseLSP && catalogItem.Line.LastInvoiceDetail != null)
                    catalogItem.Line.PreviousOrderedPrice = Math.Round(catalogItem.Line.LastInvoiceDetail.Price, Config.Round);
                if (Config.ShowAvgInCatalog && _order.Client != null)
                {
                    catalogItem.Line.AvgSale = _order.Client.Average(product.ProductId);
                    if (catalogItem.Line.AvgSale == -1 && catalogItem.Line.History?.Count > 0)
                        catalogItem.Line.AvgSale = catalogItem.Line.History.Sum(x => x.Quantity) / catalogItem.Line.History.Count;
                }

                foreach (var orderDetail in _order.Details.Where(d => !d.IsCredit && d.Product?.ProductId == product.ProductId))
                {
                    catalogItem.Values.Add(new OdLine
                    {
                        OrderDetail = orderDetail,
                        Product = product,
                        Qty = orderDetail.Qty,
                        Weight = orderDetail.Weight,
                        ExpectedPrice = orderDetail.ExpectedPrice,
                        Price = orderDetail.Price,
                        Comments = orderDetail.Comments,
                        Lot = orderDetail.Lot,
                        Damaged = orderDetail.Damaged,
                        UoM = orderDetail.UnitOfMeasure,
                        FreeItem = orderDetail.IsFreeItem,
                        IsCredit = false,
                        IsPriceFromSpecial = orderDetail.FromOfferPrice,
                        Allowance = orderDetail.Allowance,
                        Discount = orderDetail.Discount,
                        DiscountType = orderDetail.DiscountType,
                        ReasonId = orderDetail.ReasonId,
                        LotExp = orderDetail.LotExpiration,
                        ManuallyChanged = orderDetail.ModifiedManually,
                        PriceLevelSelected = orderDetail.PriceLevelSelected
                    });
                }
                catalogItem.UpdateDisplay();
                catalogItems.Add(catalogItem);
            }

            var catalogItemsForSort = catalogItems.Select(x => new CatalogItem { Product = Product.Find(x.ProductId), Line = x.Line, Values = x.Values.ToList() }).ToList();
            var sorted = SortDetails.SortedDetailsInProduct(catalogItemsForSort).ToList();
            var sortedViewModels = sorted.Select(s => catalogItems.FirstOrDefault(x => x.ProductId == s.Product?.ProductId)).Where(x => x != null).ToList();
            foreach (var item in catalogItems)
                if (!sortedViewModels.Contains(item)) sortedViewModels.Add(item);

            var productIds = sortedViewModels.Select(x => x.ProductId).Where(id => id > 0).ToList();
            var atLeastOneImage = productIds.Count > 0 && ProductImage.AtLeastOneProductHasImg(productIds);
            return (sortedViewModels, atLeastOneImage);
        }

        /// <summary>Build catalog items with sublines (same as ProductCatalogPage). Used by Filter, Sort, Search, and after add/edit. Replaces Products in one shot to avoid N Add() layout cost.</summary>
        private void PrepareProductList()
        {
            if (_order == null) return;
            var (items, atLeastOneImage) = BuildCatalogItemsList();
            Products.Clear();
            if (items != null && items.Count > 0)
                foreach (var item in items) Products.Add(item);
            _atLeastOneImage = atLeastOneImage;
            ShowImages = _atLeastOneImage;
        }

        private void ApplyFilter()
        {
            var list = Products.ToList();
            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                var searchUpper = _searchCriteria.Trim().ToUpperInvariant();
                list = list.Where(x =>
                    x.ProductName.ToUpperInvariant().Contains(searchUpper) ||
                    x.Upc.ToUpperInvariant().Contains(searchUpper) ||
                    x.Sku.ToUpperInvariant().Contains(searchUpper) ||
                    x.Code.ToUpperInvariant().Contains(searchUpper)).ToList();
            }
            FilteredProducts.Clear();
            foreach (var item in list) FilteredProducts.Add(item);
        }

        partial void OnSearchQueryChanged(string value)
        {
            _searchCriteria = value?.Trim() ?? string.Empty;
            ApplyFilter();
        }

        [RelayCommand]
        private async Task Scan()
        {
            if (_order == null)
                return;
            try
            {
                ScannedItemToFocus = null;
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult)) return;
                var product = Product.Products.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.Upc) && p.Upc.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Sku) && p.Sku.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Equals(scanResult, StringComparison.OrdinalIgnoreCase)));
                if (product != null)
                {
                    SearchQuery = scanResult;
                    SearchText = scanResult;
                    _searchCriteria = scanResult;

                    var catalogItem = FilteredProducts.FirstOrDefault(i => i.ProductId == product.ProductId);
                    if (catalogItem != null)
                    {
                        HighlightItemOnly(catalogItem);
                        ScannedItemToFocus = catalogItem;
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error scanning barcode: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            }
        }

        [RelayCommand]
        private async Task Filter()
        {
            var options = new[] { "All", "In Stock", "Not In Order", "In Order" };
            var selected = await _dialogService.ShowActionSheetAsync("Filter Products", "", "Cancel", options);
            if (selected == "Cancel" || string.IsNullOrEmpty(selected)) return;
            _currentFilter = selected switch { "In Stock" => 2, "Not In Order" => 4, "In Order" => 8, _ => 0 };
            FilterText = $"Filter: {selected}";
            PrepareProductList();
            ApplyFilter();
        }

        [RelayCommand]
        private async Task SortBy()
        {
            var options = new[] { "Name", "Category", "In Stock" };
            var selected = await _dialogService.ShowActionSheetAsync("Sort By", "", "Cancel", options);
            if (selected == "Cancel" || string.IsNullOrEmpty(selected)) return;
            _currentSort = selected switch { "Category" => 1, "In Stock" => 2, _ => 0 };
            SortByText = $"Sort: {selected}";
            PrepareProductList();
            ApplyFilter();
        }

        /// <summary>+ button: open RestOfTheAddDialog (same as ProductCatalogPage). Adds new line/subline.</summary>
        [RelayCommand]
        private async Task AddButtonClick(CatalogItemViewModel item)
        {
            if (item == null || _order == null) return;
            await AddProductWithRestOfTheAddDialogAsync(item);
        }

        /// <summary>Edit subline (tap on order detail row). Same as ProductCatalogPage EditSublineAsync.</summary>
        [RelayCommand]
        private async Task EditSubline(OdLine line)
        {
            if (line?.OrderDetail == null || _order == null) return;
            var existingDetail = line.OrderDetail;
            var product = existingDetail.Product;
            if (product == null) return;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(product, _order, existingDetail, isCredit: false, isDamaged: existingDetail.Damaged, isDelivery: _order.IsDelivery);
            if (result.Cancelled) return;
            if (result.Qty == 0)
            {
                _order.DeleteDetail(existingDetail);
                _order.Save();
                PrepareProductList();
                ApplyFilter();
                return;
            }
            existingDetail.Qty = result.Qty;
            existingDetail.Weight = result.Weight;
            existingDetail.Lot = result.Lot;
            if (result.LotExpiration.HasValue) existingDetail.LotExpiration = result.LotExpiration.Value;
            existingDetail.Comments = result.Comments;
            existingDetail.Price = result.Price;
            existingDetail.UnitOfMeasure = result.SelectedUoM;
            existingDetail.IsFreeItem = result.IsFreeItem;
            existingDetail.ReasonId = result.ReasonId;
            existingDetail.Discount = result.Discount;
            existingDetail.DiscountType = result.DiscountType;
            if (result.PriceLevelSelected > 0)
                existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
            OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, existingDetail);
            OrderDetail.UpdateRelated(existingDetail, _order);
            _order.RecalculateDiscounts();
            _order.Save();
            PrepareProductList();
            ApplyFilter();
            var editedItem = FilteredProducts.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (editedItem != null) HighlightItemOnly(editedItem);
        }

        /// <summary>+ button for Advanced Catalog style: add 1 to first detail or create new detail with qty 1.</summary>
        [RelayCommand]
        private void IncrementQuantity(CatalogItemViewModel item)
        {
            if (_order == null) return;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == item.ProductId && !x.IsCredit);
            if (existingDetail != null)
            {
                existingDetail.Qty += 1f;
                OrderDetail.UpdateRelated(existingDetail, _order);
            }
            else
            {
                var product = Product.Find(item.ProductId);
                
                var detail = new OrderDetail(product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(product, _order, false, false);
                if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out var price))
                { detail.Price = price; detail.FromOfferPrice = true; }
                else
                { detail.Price = expectedPrice; detail.FromOfferPrice = false; }
                detail.ExpectedPrice = expectedPrice;
                detail.UnitOfMeasure = product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault);
                detail.Qty = 1f;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                OrderDetail.UpdateRelated(detail, _order);
            }
            _order.RecalculateDiscounts();
            _order.Save();
            RefreshCatalogItem(item);
            HighlightItemOnly(item);
        }

        /// <summary>- button for Advanced Catalog style: subtract 1 or remove detail if qty becomes 0.</summary>
        [RelayCommand]
        private void DecrementQuantity(CatalogItemViewModel item)
        {

            if (_order == null) return;
            
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == item.ProductId && !x.IsCredit);
            if (existingDetail == null) return;
            if (existingDetail.Qty <= 1f)
            {
                _order.DeleteDetail(existingDetail);
                _order.RecalculateDiscounts();
                _order.Save();
            }
            else
            {
                existingDetail.Qty -= 1f;
                OrderDetail.UpdateRelated(existingDetail, _order);
                _order.RecalculateDiscounts();
                _order.Save();
            }
            RefreshCatalogItem(item);
            HighlightItemOnly(item);
        }

        /// <summary>Moves highlight to the given item (light blue background) without scrolling. Use when user adds/decrements. Match AdvancedCatalogPageViewModel.HighlightItemOnly.</summary>
        private void HighlightItemOnly(CatalogItemViewModel? item)
        {
            foreach (var i in Products)
                i.IsHighlightedFromScan = false;
            if (item != null)
                item.IsHighlightedFromScan = true;
        }

        private void RefreshCatalogItem(CatalogItemViewModel item)
        {
            if (item == null) return;
            item.Values.Clear();
            var product = Product.Find(item.ProductId);
            if (product == null) return;
            foreach (var orderDetail in _order.Details.Where(d => !d.IsCredit && d.Product?.ProductId == product.ProductId))
            {
                item.Values.Add(new OdLine
                {
                    OrderDetail = orderDetail,
                    Product = product,
                    Qty = orderDetail.Qty,
                    Weight = orderDetail.Weight,
                    ExpectedPrice = orderDetail.ExpectedPrice,
                    Price = orderDetail.Price,
                    Comments = orderDetail.Comments,
                    Lot = orderDetail.Lot,
                    Damaged = orderDetail.Damaged,
                    UoM = orderDetail.UnitOfMeasure,
                    FreeItem = orderDetail.IsFreeItem,
                    IsCredit = false,
                    IsPriceFromSpecial = orderDetail.FromOfferPrice,
                    Allowance = orderDetail.Allowance,
                    Discount = orderDetail.Discount,
                    DiscountType = orderDetail.DiscountType,
                    ReasonId = orderDetail.ReasonId,
                    LotExp = orderDetail.LotExpiration,
                    ManuallyChanged = orderDetail.ModifiedManually,
                    PriceLevelSelected = orderDetail.PriceLevelSelected
                });
            }
            item.UpdateDisplay();
        }

        private async Task AddProductWithRestOfTheAddDialogAsync(CatalogItemViewModel item)
        {
            if (_order == null) return;
            
            var product = Product.Find(item.ProductId);

            OrderDetail existingDetail = null;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(product, _order, existingDetail, isCredit: false, isDamaged: false, isDelivery: _order.IsDelivery);
            if (result.Cancelled) return;
            if (result.Qty == 0) { PrepareProductList(); ApplyFilter(); return; }
            var detail = new OrderDetail(product, 0, _order);
            double expectedPrice = Product.GetPriceForProduct(product, _order, false, false);
            double price = result.Price;
            if (result.UseLastSoldPrice && _order.Client != null)
            {
                var clientHistory = InvoiceDetail.ClientProduct(_order.Client.ClientId, item.ProductId);
                var lastInvoiceDetail = clientHistory?.OrderByDescending(x => x.Date).FirstOrDefault();
                if (lastInvoiceDetail != null) price = lastInvoiceDetail.Price;
            }
            else if (price == 0)
            {
                if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out var offerPrice))
                { detail.Price = offerPrice; detail.FromOfferPrice = true; }
                else
                { detail.Price = expectedPrice; detail.FromOfferPrice = false; }
            }
            else
            { detail.Price = price; detail.FromOfferPrice = false; }
            detail.ExpectedPrice = expectedPrice;
            detail.UnitOfMeasure = result.SelectedUoM ?? product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault);
            detail.Qty = result.Qty;
            detail.Weight = result.Weight;
            detail.Lot = result.Lot;
            if (result.LotExpiration.HasValue) detail.LotExpiration = result.LotExpiration.Value;
            detail.Comments = result.Comments;
            detail.IsFreeItem = result.IsFreeItem;
            detail.ReasonId = result.ReasonId;
            detail.Discount = result.Discount;
            detail.DiscountType = result.DiscountType;
            if (result.PriceLevelSelected > 0)
                detail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), detail.ExtraFields);
            detail.CalculateOfferDetail();
            _order.AddDetail(detail);
            OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, detail);
            OrderDetail.UpdateRelated(detail, _order);
            _order.RecalculateDiscounts();
            _order.Save();
            PrepareProductList();
            ApplyFilter();
            var addedItem = FilteredProducts.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (addedItem != null) HighlightItemOnly(addedItem);
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchCriteria = value?.Trim() ?? string.Empty;
            SearchQuery = value ?? string.Empty;
            PrepareProductList();
            ApplyFilter();
        }

        /// <summary>Return to SelfServiceCheckOutPage (toolbar Checkout button). When entered from Search/scan (fromCheckout), only pop once; when from Categories, pop twice.</summary>
        [RelayCommand]
        private async Task GoToCheckout()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync("..");
            if (!_fromCheckout && _category != null)
                await Shell.Current.GoToAsync("..");
        }
        
        /// <summary>Toolbar: show help/menu (Sync Data, Advanced Options, Sign Out).</summary>
        [RelayCommand]
        private async Task ShowToolbarMenuAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }
    }
}

