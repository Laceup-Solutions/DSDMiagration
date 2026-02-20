using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using LaceupMigration.Business.Interfaces;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
    public partial class ProductCatalogPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly IScannerService _scannerService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private Order? _order;
        private bool _initialized;
        private int? _categoryId;
        private string? _productSearch;
        private bool _comingFromSearch;
        private bool _asCreditItem;
        private bool _asReturnItem;
        private int? _productId;
        private bool _consignmentCounting;
        private string? _comingFrom; // "Credit", "PreviouslyOrdered", or "LoadOrderTemplate"
        private bool _viaFullCategory; // True when we were navigated from FullCategoryPage (stack has FullCategory below us)
        private int _loadOrderReturnDepth = 1; // When comingFrom=LoadOrderTemplate: pops to return (1=direct from template, 2=via fullcategory)
        private string _searchCriteria = string.Empty;
        private bool _isCreating = false;
        private bool _isScanning = false;
        private bool _atLeastOneImage = false;
        private ViewTypes _currentViewType = ViewTypes.Normal;
        private bool _onlyReturn = false;
        private bool _onlyDamage = false;
        private bool _isShowingSuggested = false;

        /// <summary>When true, next OnAppearingAsync skips refresh to preserve scroll position (returning from ProductDetails or ViewImage).</summary>
        private bool _skipNextOnAppearingRefresh;

        public ObservableCollection<CatalogItemViewModel> Products { get; } = new();
        public ObservableCollection<CatalogItemViewModel> FilteredProducts { get; } = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _showImages = true;

        [ObservableProperty]
        private string _viewTypeIcon = "catalog_two";

        [ObservableProperty]
        private string _pageTitle = "Products";

        [ObservableProperty]
        private bool _isFromLoadOrder;

        [ObservableProperty]
        private bool _isFromSelfService;

        /// <summary>When from self service, order ID to navigate back to checkout.</summary>
        public int? SelfServiceOrderId => _order?.OrderId;

        public ProductCatalogPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService, ICameraBarcodeScannerService cameraBarcodeScanner, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
            _advancedOptionsService = advancedOptionsService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            _currentViewType = (ViewTypes)Config.ProductCatalogViewType;
            UpdateViewTypeIcon();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
            }

            if (query.TryGetValue("categoryId", out var catValue) && catValue != null)
            {
                if (int.TryParse(catValue.ToString(), out var catId))
                    _categoryId = catId;
            }

            if (query.TryGetValue("productSearch", out var searchValue) && searchValue != null)
            {
                var searchString = searchValue.ToString();
                // MAUI Shell may not automatically decode URL-encoded query parameters
                // Decode if it contains encoded characters (e.g., %20 for space)
                try
                {
                    if (searchString.Contains("%"))
                    {
                        searchString = Uri.UnescapeDataString(searchString);
                    }
                }
                catch
                {
                    // If decoding fails, use the original string
                }
                _productSearch = searchString.Trim();
                // Set SearchQuery so it appears correctly in the SearchBar
                SearchQuery = _productSearch;
            }

            if (query.TryGetValue("comingFromSearch", out var fromSearchValue) && fromSearchValue != null)
            {
                _comingFromSearch = fromSearchValue.ToString().ToLowerInvariant() == "yes" || fromSearchValue.ToString() == "true";
            }

            if (query.TryGetValue("asCreditItem", out var creditValue) && creditValue != null)
            {
                _asCreditItem = creditValue.ToString() == "1" || creditValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("asReturnItem", out var returnValue) && returnValue != null)
            {
                _asReturnItem = returnValue.ToString() == "1" || returnValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("productId", out var prodValue) && prodValue != null)
            {
                if (int.TryParse(prodValue.ToString(), out var pId))
                    _productId = pId;
            }

            if (query.TryGetValue("consignmentCounting", out var countingValue) && countingValue != null)
            {
                _consignmentCounting = countingValue.ToString() == "1" || countingValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("comingFrom", out var fromValue) && fromValue != null)
            {
                _comingFrom = fromValue.ToString();
            }

            if (query.TryGetValue("viaFullCategory", out var viaFullValue) && viaFullValue != null)
            {
                _viaFullCategory = viaFullValue.ToString() == "1" || string.Equals(viaFullValue.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            }
            System.Diagnostics.Debug.WriteLine($"[ProductCatalog] ApplyQueryAttributes: comingFrom={_comingFrom ?? "(null)"}, viaFullCategory={_viaFullCategory}, loadOrderReturnDepth={_loadOrderReturnDepth}");

            if (query.TryGetValue("loadOrderReturnDepth", out var depthValue) && depthValue != null)
            {
                if (int.TryParse(depthValue.ToString(), out var d) && d >= 1)
                    _loadOrderReturnDepth = d;
            }
            else if (_comingFrom == "LoadOrderTemplate")
            {
                _loadOrderReturnDepth = 1; // Direct from NewLoadOrderTemplate (Prod with last category)
            }

            if (query.TryGetValue("isShowingSuggested", out var suggestedValue) && suggestedValue != null)
            {
                _isShowingSuggested = suggestedValue.ToString() == "1" || suggestedValue.ToString().ToLowerInvariant() == "true";
                
                // Update page title for suggested products
                if (_isShowingSuggested)
                {
                    string categoryName = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                        ? "Suggested Products" 
                        : $"{Config.ProductCategoryNameIdentifier} Products";
                    PageTitle = categoryName;
                }
            }

            MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync(orderId));
        }

        public async Task InitializeAsync(int? orderId)
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
                Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
                System.Diagnostics.Debug.WriteLine("[ProductCatalog] InitializeAsync order not found: removing state productcatalog, popping");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Set onlyReturn and onlyDamage based on existing order details (matching Xamarin logic)
            _onlyReturn = _order.Details.Count > 0 && _order.Details[0].IsCredit && _order.Details[0].Damaged == false;
            _onlyDamage = _order.Details.Count > 0 && _order.Details[0].IsCredit && _order.Details[0].Damaged == true;

            _initialized = true;
            IsFromLoadOrder = _comingFrom == "LoadOrderTemplate";
            IsFromSelfService = _comingFrom == "SelfService";
            PrepareProductList();
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

            await RefreshAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            PrepareProductList();
            Filter();
            await Task.CompletedTask;
        }

        private void PrepareProductList()
        {
            if (_order == null)
                return;

            var productsForOrder = Product.GetProductListForOrder(_order, _asCreditItem, _categoryId ?? 0).ToList();

            // Filter to only suggested products if isShowingSuggested is true
            if (_isShowingSuggested && _order.Client != null)
            {
                var suggestedCategory = SuggestedClientCategory.List.FirstOrDefault(x => 
                    x.SuggestedClientCategoryClients.Any(y => y.ClientId == _order.Client.ClientId));
                
                if (suggestedCategory != null && suggestedCategory.SuggestedClientCategoryProducts.Count > 0)
                {
                    var suggestedProductIds = suggestedCategory.SuggestedClientCategoryProducts
                        .Select(sp => sp.ProductId)
                        .Where(id => id > 0)
                        .ToList();
                    
                    productsForOrder = productsForOrder
                        .Where(p => suggestedProductIds.Contains(p.ProductId))
                        .ToList();
                }
                else
                {
                    // No suggested products available
                    productsForOrder = new List<Product>();
                }
            }

            if (!string.IsNullOrEmpty(_productSearch) && _comingFromSearch)
            {
                var searchLower = _productSearch.Trim().ToLowerInvariant();
                productsForOrder = productsForOrder.Where(x =>
                    x.Name.ToLowerInvariant().IndexOf(searchLower) != -1 ||
                    x.Upc.ToLowerInvariant().Contains(searchLower) ||
                    x.Sku.ToLowerInvariant().Contains(searchLower) ||
                    x.Description.ToLowerInvariant().Contains(searchLower) ||
                    x.Code.ToLowerInvariant().Contains(searchLower)
                ).ToList();
            }

            if (_order.Client.ClientProductHistory == null)
                _order.Client.ClientProductHistory = InvoiceDetail.GetProductHistoryDictionary(_order.Client);

            var clientSource = _order.Client.ClientProductHistory;
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
                if (clientSourceKey != null)
                {
                    catalogItem.Line.History = clientSource[clientSourceKey].OrderByDescending(x => x.Date).ToList();
                }
                else
                {
                    catalogItem.Line.History = new List<InvoiceDetail>();
                }

                catalogItem.Line.LastInvoiceDetail = catalogItem.Line.History?.FirstOrDefault();
                catalogItem.Line.Product = product;
                catalogItem.Line.IsCredit = _asCreditItem;

                // Handle UoM and list price (price per selected UoM)
                if (!string.IsNullOrEmpty(product.UoMFamily))
                {
                    UnitOfMeasure? uom = null;
                    if (_asCreditItem || _order.Client.UseBaseUoM)
                        uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsBase);
                    else
                        uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsDefault);

                    if (Config.UseLastUoM && catalogItem.Line.LastInvoiceDetail != null)
                    {
                        var lastUomId = catalogItem.Line.LastInvoiceDetail.UnitOfMeasureId;
                        uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == lastUomId);
                    }

                    catalogItem.Line.UoM = uom;
                }
                catalogItem.Line.ExpectedPrice = Product.GetPriceForProduct(product, _order, out _, _asCreditItem, false, catalogItem.Line.UoM);

                if (Config.UseLSP && catalogItem.Line.LastInvoiceDetail != null)
                    catalogItem.Line.PreviousOrderedPrice = Math.Round(catalogItem.Line.LastInvoiceDetail.Price, Config.Round);

                if (Config.ShowAvgInCatalog)
                {
                    catalogItem.Line.AvgSale = _order.Client.Average(product.ProductId);
                    if (catalogItem.Line.AvgSale == -1)
                        catalogItem.Line.AvgSale = catalogItem.Line.History.Count > 0 ? catalogItem.Line.History.Sum(x => x.Quantity) / catalogItem.Line.History.Count : 0;
                }

                // Sync with existing order details
                foreach (var orderDetail in _order.Details)
                {
                    if (orderDetail.IsCredit != _asCreditItem)
                        continue;

                    if (orderDetail.Product.ProductId != product.ProductId)
                        continue;

                    var odLine = new OdLine
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
                        IsCredit = orderDetail.IsCredit,
                        IsPriceFromSpecial = orderDetail.FromOfferPrice,
                        Allowance = orderDetail.Allowance,
                        Discount = orderDetail.Discount,
                        DiscountType = orderDetail.DiscountType,
                        ReasonId = orderDetail.ReasonId,
                        LotExp = orderDetail.LotExpiration,
                        ManuallyChanged = orderDetail.ModifiedManually,
                        PriceLevelSelected = orderDetail.PriceLevelSelected
                    };

                    catalogItem.Values.Add(odLine);
                }

                // Update display after adding values
                catalogItem.UpdateDisplay();
                catalogItems.Add(catalogItem);
            }

            // Update display for all items after populating values
            foreach (var item in catalogItems)
            {
                item.UpdateDisplay();
            }

            // Sort products - convert to CatalogItem for sorting
            var catalogItemsForSort = catalogItems.Select(x => new CatalogItem
            {
                Product = Product.Find(x.ProductId),
                Line = x.Line,
                Values = x.Values.ToList()
            }).ToList();
            var sorted = SortDetails.SortedDetailsInProduct(catalogItemsForSort).ToList();
            
            // Rebuild ViewModels from sorted items maintaining order
            // Update display for each item after sorting
            foreach (var item in catalogItems)
            {
                item.UpdateDisplay();
            }
            
            var sortedViewModels = new List<CatalogItemViewModel>();
            foreach (var sortedItem in sorted)
            {
                var vm = catalogItems.FirstOrDefault(x => x.ProductId == sortedItem.Product.ProductId);
                if (vm != null)
                {
                    sortedViewModels.Add(vm);
                }
            }
            // Add any remaining items not in sorted list
            foreach (var item in catalogItems)
            {
                if (!sortedViewModels.Contains(item))
                    sortedViewModels.Add(item);
            }

            Products.Clear();
            foreach (var item in sortedViewModels)
            {
                Products.Add(item);
            }

            // Check for images
            var productIds = Products.Select(x => x.ProductId).ToList();
            _atLeastOneImage = ProductImage.AtLeastOneProductHasImg(productIds);
            ShowImages = _atLeastOneImage;
        }

        private void Filter()
        {
            var list = Products.ToList();

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                var searchUpper = _searchCriteria.Trim().ToUpperInvariant();
                list = list.Where(x =>
                    x.ProductName.ToUpperInvariant().Contains(searchUpper) ||
                    x.Upc.ToUpperInvariant().Contains(searchUpper) ||
                    x.Sku.ToUpperInvariant().Contains(searchUpper) ||
                    x.Code.ToLowerInvariant().Contains(searchUpper)
                ).ToList();
            }

            FilteredProducts.Clear();
            foreach (var item in list)
            {
                FilteredProducts.Add(item);
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            _searchCriteria = value?.Trim() ?? string.Empty;
            Filter();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            var trimmedSearch = searchTerm.Trim();
            _searchCriteria = trimmedSearch.ToUpperInvariant();
            SearchQuery = trimmedSearch;
            Filter();
        }

        [RelayCommand]
        public async Task ScanAsync()
        {
            if (_isScanning || _order == null)
                return;

            try
            {
                _isScanning = true;
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult))
                {
                    _isScanning = false;
                    return;
                }

                await ScannerDoTheThingAsync(scanResult);
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error scanning: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            }
            finally
            {
                _isScanning = false;
            }
        }

        private async Task ScannerDoTheThingAsync(string data)
        {
            if (_order == null)
                return;

            var product = ActivityExtensionMethods.GetProduct(_order, data);

            if (product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                return;
            }

            var visibleProducts = Product.GetProductListForOrder(_order, _asCreditItem, 0).Select(x => x.ProductId).ToList();
            var noInventory = product.GetInventory(_order.AsPresale) <= 0;

            if (noInventory && !_asCreditItem)
            {
                await _dialogService.ShowAlertAsync($"Not enough inventory of {product.Name}", "Alert");
                return;
            }

            if (!noInventory && !visibleProducts.Contains(product.ProductId))
            {
                await _dialogService.ShowAlertAsync("Product not authorized for client.", "Alert");
                return;
            }

            // From load order: if product is in the current list, add qty 1 and stay; otherwise add and exit to load template
            if (_comingFrom == "LoadOrderTemplate" && _order.OrderType == OrderType.Load)
            {
                var catalogItem = FilteredProducts.FirstOrDefault(x => x.ProductId == product.ProductId);
                var uom = catalogItem?.Line?.UoM ?? (!string.IsNullOrEmpty(product.UoMFamily)
                    ? UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsDefault)
                    : null) ?? product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault) ?? product.UnitOfMeasures?.FirstOrDefault();
                AddSingleProductToLoadOrder(product, uom);
                _order.Save();
                if (catalogItem != null)
                {
                    PrepareProductList();
                    Filter();
                    return;
                }
                Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
                for (int i = 0; i < _loadOrderReturnDepth; i++)
                {
                    if (i == 1)
                        Helpers.NavigationHelper.RemoveNavigationState("fullcategory");
                    await Shell.Current.GoToAsync("..");
                }
                return;
            }

            var catalogItemDefault = FilteredProducts.FirstOrDefault(x => x.ProductId == product.ProductId);
            if (catalogItemDefault != null)
            {
                await AddButton_ClickAsync(catalogItemDefault);
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

        [RelayCommand]
        public async Task NavigateToAddItemAsync(CatalogItemViewModel? item)
        {
            if (item == null || _order == null)
                return;

            // Find the first value with an OrderDetail (if editing existing item)
            // Otherwise, use productId (adding new item)
            var firstValueWithDetail = item.Values.FirstOrDefault(v => v.OrderDetail != null);
            
            if (firstValueWithDetail?.OrderDetail != null)
            {
                // Navigate with orderDetail (editing existing detail)
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&orderDetail={firstValueWithDetail.OrderDetail.OrderDetailId}&asCreditItem={(firstValueWithDetail.OrderDetail.IsCredit ? 1 : 0)}");
            }
            else
            {
                // Navigate with productId (adding new item)
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={item.ProductId}");
            }
        }

        [RelayCommand]
        private async Task AddButtonClickAsync(CatalogItemViewModel? item)
        {
            if (item == null)
                return;

            await AddButton_ClickAsync(item);
        }

        /// <summary>Opens the product image in full-screen (ViewImagePage).</summary>
        [RelayCommand]
        private async Task ViewImageAsync(CatalogItemViewModel? item)
        {
            if (item == null || !item.HasImage || string.IsNullOrEmpty(item.ProductImg))
                return;
            _skipNextOnAppearingRefresh = true;
            await Shell.Current.GoToAsync($"viewimage?imagePath={Uri.EscapeDataString(item.ProductImg)}");
        }

        /// <summary>Navigates to product details page.</summary>
        [RelayCommand]
        private async Task ViewProductDetailsAsync(CatalogItemViewModel? item)
        {
            if (item == null || item.ProductId <= 0)
                return;
            _skipNextOnAppearingRefresh = true;
            var route = _order != null
                ? $"productdetails?productId={item.ProductId}&orderId={_order.OrderId}"
                : $"productdetails?productId={item.ProductId}";
            await Shell.Current.GoToAsync(route);
        }
        
        /// <summary>Edit an existing subline (order detail). Opens RestOfTheAddDialog with that detail for editing. Matches Xamarin EditButton_Click on subline.</summary>
        [RelayCommand]
        private async Task EditSublineAsync(OdLine? line)
        {
            if (line == null || line.OrderDetail == null || _order == null)
                return;

            var existingDetail = line.OrderDetail;
            var product = existingDetail.Product;
            if (product == null)
                return;

            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                product,
                _order,
                existingDetail,
                isCredit: existingDetail.IsCredit,
                isDamaged: existingDetail.Damaged,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled)
                return;

            if (result.Qty == 0)
            {
                _order.DeleteDetail(existingDetail);
                _order.Save();
                PrepareProductList();
                Filter();
                return;
            }

            // Add back current qty so inventory updates correctly, then subtract new qty
            _order.UpdateInventory(existingDetail, 1);
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
            existingDetail.Discount = result.Discount;
            existingDetail.DiscountType = result.DiscountType;
            if (result.PriceLevelSelected > 0)
                existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
            _order.UpdateInventory(existingDetail, -1);

            OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, existingDetail);
            OrderDetail.UpdateRelated(existingDetail, _order);
            _order.RecalculateDiscounts();
            _order.Save();
            PrepareProductList();
            Filter();
        }

        private async Task AddButton_ClickAsync(CatalogItemViewModel item)
        {
            if (_order == null)
                return;

            _appService.RecordEvent("AddButton_Click Product Catalog");

            if (_order.OrderType == OrderType.Load)
            {
                // Handle Load order type separately
                await RestOfTheAddDialogLoadAsync(item);
                return;
            }

            // If coming from Credit page, prompt for credit type (Dump/Return) first
            // If coming from PreviouslyOrdered, just add as sales (no prompt needed)
            if (_comingFrom == "Credit" || _asCreditItem)
            {
                // Check conditions for default type selection
                int defaultType = 0;
                
                if (_order.AsPresale && (Config.UseReturnOrder && _order != null && _order.OrderType != OrderType.Order))
                {
                    defaultType = !_asReturnItem ? 1 : 2;
                }
                else if ((Config.UseReturnInvoice || _order.IsExchange) && _order.OrderType == OrderType.Credit)
                {
                    defaultType = !_asReturnItem ? 1 : 2;
                }
                else if (!string.IsNullOrEmpty(Config.DefaultCreditDetType))
                {
                    defaultType = Config.DefaultCreditDetType.ToLower() == "dump" ? 1 : 2;
                }

                // Select credit type (Dump or Return)
                await SelectCreditTypeAsync(item, false, defaultType);
                return;
            }

            // For PreviouslyOrdered (sales items), show RestOfTheAddDialog (same as PreviouslyOrderedTemplatePage row click)
            await AddProductWithRestOfTheAddDialogAsync(item);
        }

        private async Task SelectCreditTypeAsync(CatalogItemViewModel item, bool fromEdit, int defaultType = 0)
        {
            if (_order == null)
                return;

            var items = new List<CreditType>();
            
            if (defaultType == 0)
            {
                if (Config.WarningDumpReturn) // After picking Return || Damage only lets you pick only the same
                {
                    if (_onlyReturn)
                        items.Add(new CreditType() { Description = "Return", Damaged = false });
                    if (_onlyDamage)
                        items.Add(new CreditType() { Description = "Dump", Damaged = true });
                    if (!_onlyDamage && !_onlyReturn)
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
            }

            if (Config.CreditReasonInLine)
            {
                var reasons = new List<Reason>();
                if (defaultType == 0 || defaultType == 1)
                    reasons.AddRange(Reason.GetReasonsByType(ReasonType.Dump));
                if (defaultType == 0 || defaultType == 2)
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
                // No selection needed, proceed with default
                await RestOfTheAddDialogAsync(item, defaultType == 1, fromEdit, 0);
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
                await RestOfTheAddDialogAsync(item, selectedItem.Damaged, fromEdit, selectedItem.ReasonId);
            }
        }

        private async Task RestOfTheAddDialogAsync(CatalogItemViewModel item, bool damaged, bool fromEdit, int reasonId)
        {
            if (_order == null)
                return;

            var product = Product.Find(item.ProductId);
            // When adding from main line (+), always create a new credit line so we can have e.g. 1 case return + 1 each return + 1 case dump + 1 each dump per product.
            // Editing a specific line is done via EditSublineAsync (subline tap), which passes that detail.
            OrderDetail? existingDetail = null;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                product,
                _order,
                existingDetail,
                isCredit: true,
                isDamaged: damaged,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled)
                return;

            // Handle qty == 0 - delete the detail
            if (result.Qty == 0)
            {
                if (existingDetail != null)
                {
                    _order.DeleteDetail(existingDetail);
                    _order.Save();
                }
                PrepareProductList();
                Filter();
                return;
            }

            // qty > 0 - update existing or create new credit detail (same structure as AddProductWithRestOfTheAddDialogAsync)
            OrderDetail? updatedDetail = null;
            if (existingDetail != null)
            {
                _order.UpdateInventory(existingDetail, 1);
                existingDetail.Qty = result.Qty;
                existingDetail.Weight = result.Weight;
                existingDetail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    existingDetail.LotExpiration = result.LotExpiration.Value;
                existingDetail.Comments = result.Comments;
                existingDetail.Price = result.Price;
                existingDetail.UnitOfMeasure = result.SelectedUoM;
                existingDetail.IsFreeItem = result.IsFreeItem;
                existingDetail.Damaged = damaged;
                existingDetail.ReasonId = result.ReasonId;
                existingDetail.Discount = result.Discount;
                existingDetail.DiscountType = result.DiscountType;
                if (result.PriceLevelSelected > 0)
                {
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                }
                _order.UpdateInventory(existingDetail, -1);
                updatedDetail = existingDetail;
            }
            else
            {
                var detail = new OrderDetail(product, 0, _order);
                detail.IsCredit = true;
                detail.Damaged = damaged;
                detail.ReasonId = result.ReasonId;
                detail.Discount = result.Discount;
                detail.DiscountType = result.DiscountType;
                var selectedUoM = result.SelectedUoM ?? product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.UnitOfMeasure = selectedUoM;
                // Price and ExpectedPrice must be in the selected UoM (per selling unit), not base
                double expectedPrice = Product.GetPriceForProduct(product, _order, out _, true, damaged, selectedUoM);
                double price = result.Price;
                if (result.UseLastSoldPrice && _order.Client != null)
                {
                    var clientHistory = InvoiceDetail.ClientProduct(_order.Client.ClientId, item.ProductId);
                    if (clientHistory != null && clientHistory.Count > 0)
                    {
                        var lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
                        if (lastInvoiceDetail != null)
                            price = lastInvoiceDetail.Price;
                    }
                }
                else if (price == 0)
                {
                    if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out var offerPrice))
                    {
                        // Offer price may be base; convert to selected UoM for display/order
                        detail.Price = selectedUoM != null ? Math.Round(offerPrice * selectedUoM.Conversion, Config.Round) : offerPrice;
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
                detail.Qty = result.Qty;
                detail.Weight = result.Weight;
                detail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    detail.LotExpiration = result.LotExpiration.Value;
                detail.Comments = result.Comments;
                detail.IsFreeItem = result.IsFreeItem;
                if (result.PriceLevelSelected > 0)
                {
                    detail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), detail.ExtraFields);
                }
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                _order.UpdateInventory(detail, -1);
                updatedDetail = detail;
            }

            if (updatedDetail != null)
            {
                OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, updatedDetail);
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }

            _order.Save();
            PrepareProductList();
            Filter();
        }

        private async Task AddProductWithRestOfTheAddDialogAsync(CatalogItemViewModel item)
        {
            if (_order == null)
                return;

            var product = Product.Find(item.ProductId);

            // Show RestOfTheAddDialog - pass null so we always ADD a new line (main line + sublines: multiple order details per product)
            OrderDetail? existingDetail = null;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                product,
                _order,
                existingDetail,
                isCredit: false,
                isDamaged: false,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled)
                return;

            // Handle qty == 0 - always delete the detail
            if (result.Qty == 0)
            {
                if (existingDetail != null)
                {
                    _order.DeleteDetail(existingDetail);
                    _order.Save();
                }
                PrepareProductList();
                Filter();
                return;
            }

            // qty > 0 - normal flow (matches PreviouslyOrderedTemplatePage DoTheThing1 logic)
            OrderDetail? updatedDetail = null;
            if (existingDetail != null)
            {
                _order.UpdateInventory(existingDetail, 1);
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
                existingDetail.Discount = result.Discount;
                existingDetail.DiscountType = result.DiscountType;
                if (result.PriceLevelSelected > 0)
                {
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                }
                _order.UpdateInventory(existingDetail, -1);
                updatedDetail = existingDetail;
            }
            else
            {
                // Create new detail
                var detail = new OrderDetail(product, 0, _order);
                var selectedUoM = result.SelectedUoM ?? product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.UnitOfMeasure = selectedUoM;
                // Price and ExpectedPrice must be in the selected UoM (per selling unit), not base
                double expectedPrice = Product.GetPriceForProduct(product, _order, out _, false, false, selectedUoM);
                double price = result.Price;

                // If UseLastSoldPrice, get from last invoice detail (from client history)
                if (result.UseLastSoldPrice && _order.Client != null)
                {
                    var clientHistory = InvoiceDetail.ClientProduct(_order.Client.ClientId, item.ProductId);
                    if (clientHistory != null && clientHistory.Count > 0)
                    {
                        var lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
                        if (lastInvoiceDetail != null)
                            price = lastInvoiceDetail.Price;
                    }
                }
                else if (price == 0)
                {
                    // Get price from offers or default (in selected UoM)
                    double offerPrice = 0;
                    if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out offerPrice))
                    {
                        // Offer price may be base; convert to selected UoM
                        detail.Price = selectedUoM != null ? Math.Round(offerPrice * selectedUoM.Conversion, Config.Round) : offerPrice;
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
                detail.Qty = result.Qty;
                detail.Weight = result.Weight;
                detail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    detail.LotExpiration = result.LotExpiration.Value;
                detail.Comments = result.Comments;
                detail.IsFreeItem = result.IsFreeItem;
                detail.ReasonId = result.ReasonId;
                detail.Discount = result.Discount;
                detail.DiscountType = result.DiscountType;
                if (result.PriceLevelSelected > 0)
                {
                    detail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), detail.ExtraFields);
                }
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                _order.UpdateInventory(detail, -1);
                updatedDetail = detail;
            }

            if (updatedDetail != null)
            {
                OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, updatedDetail);
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }

            _order.Save();
            PrepareProductList();
            Filter();
        }

        private async Task RestOfTheAddDialogLoadAsync(CatalogItemViewModel item)
        {
            if (_order == null)
                return;

            var product = Product.Find(item.ProductId);

            // Handle Load order type - show popup dialog (same as NewLoadOrderTemplatePage)
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == item.ProductId && !x.Deleted);
            
            var currentQty = existingDetail?.Qty ?? 0;
            var currentComments = existingDetail?.Comments ?? string.Empty;
            var currentUoM = existingDetail?.UnitOfMeasure ?? item.Line?.UoM ?? product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
            
            var result = await _dialogService.ShowAddItemDialogAsync(
                item.ProductName,
                product,
                currentQty > 0 ? currentQty.ToString() : "1",
                currentComments,
                currentUoM);
            
            if (result.qty == null)
                return; // User cancelled

            if (string.IsNullOrEmpty(result.qty) || !decimal.TryParse(result.qty, out var qty))
                qty = 0;

            if (qty == 0)
            {
                // Remove the detail
                if (existingDetail != null)
                {
                    existingDetail.Deleted = true;
                    _order.Details.Remove(existingDetail);
                }
            }
            else
            {
                // Add or update the detail; price follows selected UOM (popup has no price field)
                var selectedUoM = result.selectedUoM ?? currentUoM;
                double priceForUoM = Product.GetPriceForProduct(product, _order, out _, false, false, selectedUoM);

                OrderDetail det;
                if (existingDetail == null)
                {
                    det = new OrderDetail(product, (float)qty, _order)
                    {
                        LoadStarting = -1, // Mark as new/modified
                        UnitOfMeasure = selectedUoM,
                        ExpectedPrice = priceForUoM,
                        Price = priceForUoM,
                        Comments = result.comments ?? string.Empty
                    };
                    _order.Details.Add(det);
                }
                else
                {
                    det = existingDetail;
                    // Check inventory if configured
                    if (Config.CheckInventoryInLoad)
                    {
                        if (det.Product.CurrentWarehouseInventory < (float)qty)
                        {
                            await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert", "OK");
                            return;
                        }
                    }

                    bool changedUoM = det.UnitOfMeasure != null && result.selectedUoM != null && det.UnitOfMeasure.Id != result.selectedUoM.Id;
                    if (det.LoadStarting == -1 && (det.Qty != (float)qty || changedUoM)) 
                        det.LoadStarting = 0;

                    det.Qty = (float)qty;
                    det.Comments = result.comments ?? string.Empty;
                    det.UnitOfMeasure = selectedUoM;
                    // When UOM changes (or any update), set price for the selected UOM since popup has no price field
                    det.ExpectedPrice = priceForUoM;
                    det.Price = priceForUoM;
                }
            }

            _order.Save();
            
            // Refresh the product list
            PrepareProductList();
            Filter();
        }

        [RelayCommand]
        private void ToggleViewType()
        {
            if (_currentViewType == ViewTypes.Normal)
            {
                _currentViewType = ViewTypes.Big;
                Config.ProductCatalogViewType = (int)ViewTypes.Big;
            }
            else
            {
                _currentViewType = ViewTypes.Normal;
                Config.ProductCatalogViewType = (int)ViewTypes.Normal;
            }

            Config.SaveSettings();
            UpdateViewTypeIcon();
            OnPropertyChanged(nameof(Products));
        }

        private void UpdateViewTypeIcon()
        {
            ViewTypeIcon = _currentViewType == ViewTypes.Normal ? "catalog_two" : "catalog_one";
        }

        [RelayCommand]
        private async Task AddToOrderAsync()
        {
            if (_order == null || _isCreating)
                return;

            if (_order.OrderType == OrderType.Load)
            {
                await AddToLoadOrderAsync();
                return;
            }

            OrderDetail? oneDetail = null;

            foreach (var item in Products)
            {
                foreach (var line in item.Values)
                {
                    if (line.Qty > 0)
                    {
                        if (line.OrderDetail == null)
                        {
                            line.OrderDetail = new OrderDetail(line.Product, 0, _order);
                            _order.AddDetail(line.OrderDetail);
                            line.OrderDetail.Price = line.Price;

                            if (_asCreditItem)
                            {
                                line.OrderDetail.Damaged = line.Damaged;
                                line.OrderDetail.IsCredit = true;
                            }
                        }
                        else
                        {
                            _order.UpdateInventory(line.OrderDetail, 1);
                        }

                        if (_order.OrderType == OrderType.Consignment)
                        {
                            line.OrderDetail.ConsignmentNew = line.Qty;
                            line.OrderDetail.ConsignmentNewPrice = line.Price;
                            line.OrderDetail.ConsignmentSet = true;
                            line.OrderDetail.ConsignmentUpdated = true;
                        }
                        else
                        {
                            if (line.Product.SoldByWeight && !_order.AsPresale)
                            {
                                if (Config.NewAddItemRandomWeight)
                                {
                                    if (line.OrderDetail.Weight > 0)
                                    {
                                        line.OrderDetail.Weight = line.Qty != 1 ? line.Qty : line.Weight;
                                        line.OrderDetail.Qty = 1;
                                    }
                                    else
                                    {
                                        line.OrderDetail.Qty = 1;
                                        line.OrderDetail.Weight = line.Qty;
                                    }
                                }
                                else
                                    line.OrderDetail.Weight = line.Qty;
                            }
                            else
                                line.OrderDetail.Qty = line.Qty;
                        }

                        line.OrderDetail.Price = line.Price;
                        line.OrderDetail.ExpectedPrice = line.ExpectedPrice;
                        line.OrderDetail.UnitOfMeasure = line.UoM;
                        line.OrderDetail.Comments = line.Comments;
                        line.OrderDetail.Lot = line.Lot;
                        line.OrderDetail.IsFreeItem = line.FreeItem;
                        line.OrderDetail.LotExpiration = line.LotExp;
                        line.OrderDetail.DiscountType = line.DiscountType;
                        line.OrderDetail.Discount = line.Discount;
                        line.OrderDetail.ModifiedManually = line.ManuallyChanged;

                        if (line.DiscountType == DiscountType.Percent && line.Discount > 0)
                            line.OrderDetail.Discount /= 100;

                        _order.UpdateInventory(line.OrderDetail, -1);
                        oneDetail = line.OrderDetail;
                    }
                    else
                    {
                        if (line.OrderDetail != null)
                        {
                            _order.DeleteDetail(line.OrderDetail);
                        }
                    }
                }
            }

            if (Config.Simone)
                _order.SimoneCalculateDiscount();
            else
                _order.RecalculateDiscounts();

            _order.Save();

            // Pop: only pop twice (and remove fullcategory state) when we actually came via FullCategory
            Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
            System.Diagnostics.Debug.WriteLine("[ProductCatalog] ConfirmOrder: removing state productcatalog, popping once");
            await Shell.Current.GoToAsync(".."); // Pop ProductCatalog
            if (_viaFullCategory)
            {
                Helpers.NavigationHelper.RemoveNavigationState("fullcategory");
                System.Diagnostics.Debug.WriteLine("[ProductCatalog] ConfirmOrder: viaFullCategory=true, removing state fullcategory, popping again");
                await Shell.Current.GoToAsync(".."); // Pop FullCategory
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductCatalog] ConfirmOrder: viaFullCategory=false, not popping again (direct to ProductCatalog)");
            }
        }

        private async Task AddToLoadOrderAsync()
        {
            if (_order == null)
                return;

            OrderDetail? oneDetail = null;

            foreach (var item in Products)
            {
                foreach (var line in item.Values)
                {
                    if (line.Qty > 0)
                    {
                        if (line.OrderDetail == null)
                        {
                            line.OrderDetail = new OrderDetail(line.Product, 0, _order);
                            _order.AddDetail(line.OrderDetail);
                        }

                        line.OrderDetail.Qty = line.Qty;
                        line.OrderDetail.Price = line.Price;
                        line.OrderDetail.UnitOfMeasure = line.UoM;
                        line.OrderDetail.Comments = line.Comments;
                        line.OrderDetail.Lot = line.Lot;

                        oneDetail = line.OrderDetail;
                    }
                    else
                    {
                        if (line.OrderDetail != null)
                        {
                            _order.DeleteDetail(line.OrderDetail);
                        }
                    }
                }
            }

            _order.Save();
            ProductInventory.Save();

            // Return to NewLoadOrderTemplatePage when we came from load order (Categories/Products from template)
            if (_comingFrom == "LoadOrderTemplate")
            {
                Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
                System.Diagnostics.Debug.WriteLine($"[ProductCatalog] AddToLoadOrderAsync: removing state productcatalog, popping {_loadOrderReturnDepth} time(s)");
                for (int i = 0; i < _loadOrderReturnDepth; i++)
                {
                    if (i == 1)
                    {
                        Helpers.NavigationHelper.RemoveNavigationState("fullcategory");
                        System.Diagnostics.Debug.WriteLine("[ProductCatalog] AddToLoadOrderAsync: removing state fullcategory (i==1)");
                    }
                    await Shell.Current.GoToAsync("..");
                }
                return;
            }

            // Navigate to load order template when adding to load order from elsewhere (use registered route)
            await Shell.Current.GoToAsync($"newloadordertemplate?orderId={_order.OrderId}");
        }

        [RelayCommand]
        private async Task AddItemsAsync()
        {
            // Add Items button - adds items to load order (same as Xamarin)
            if (_order == null)
                return;

            if (_order.OrderType == OrderType.Load)
            {
                // For load orders, add items and navigate to load order template
                await AddToLoadOrderAsync();
            }
            else
            {
                // For other order types, add items to order
                await AddToOrderAsync();
            }
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            var options = new[] { "Advanced Options" };
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", options);
            
            switch (choice)
            {
                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task SendByEmailAsync()
        {
            try
            {
                // Get category list for filter dialog (matches Xamarin FullCategoryActivity SendByEmail)
                var allCategories = Category.Categories.ToList();
                var categoryList = new List<CategoryItem>();

                // Get parent categories
                foreach (var category in allCategories.Where(x => x.ParentCategoryId == 0))
                {
                    var item = new CategoryItem { Category = category };
                    
                    // Add subcategories
                    foreach (var subcat in allCategories.Where(x => x.ParentCategoryId == category.CategoryId))
                    {
                        item.Subcategories.Add(subcat);
                    }

                    categoryList.Add(item);
                }

                // Filter by site if Config.SalesmanCanChangeSite is enabled (matches Xamarin's FillCategoryList)
                if (Config.SalesmanCanChangeSite)
                {
                    if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                    {
                        var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                        var allow_categories = Product.Products.Where(x => allowedProducts.Contains(x.ProductId)).Select(x => x.CategoryId).Distinct().ToList();

                        categoryList = categoryList.Where(x => allow_categories.Contains(x.Category.CategoryId)).ToList();
                    }
                }

                // Show filter dialog (matches Xamarin FullCategoryActivity SendByEmail)
                var categoriesForDialog = categoryList.Select(x => x.Category).ToList();
                var filterResult = await _dialogService.ShowCatalogFilterDialogAsync(categoriesForDialog);

                if (filterResult == null)
                {
                    // User cancelled
                    return;
                }

                var (selectedCategoryIds, selectAll, showPrice, showUPC, showUoM) = filterResult.Value;

                await _dialogService.ShowLoadingAsync("Generating PDF...");

                // Get all products (matches Xamarin: Product.Products.OrderBy(x => x.Name).Where(p => p.Name.Trim() != "" && p.CategoryId != 0).ToList())
                List<Product> elements = Product.Products.OrderBy(x => x.Name).Where(p => p.Name.Trim() != "" && p.CategoryId != 0).ToList();

                // Filter products by selected categories (matches Xamarin logic)
                var childs = Category.Categories.Where(x => selectedCategoryIds.Contains(x.ParentCategoryId)).Select(x => x.CategoryId).Distinct().ToList();
                var filteredProducts = selectAll ? elements : elements.Where(p => selectedCategoryIds.Contains(p.CategoryId) || childs.Contains(p.CategoryId)).ToList();

                if (filteredProducts.Count == 0)
                {
                    await _dialogService.HideLoadingAsync();
                    await _dialogService.ShowAlertAsync("No products found to include in catalog.", "Info");
                    return;
                }

                // Collect category IDs from filtered products (matches Xamarin FullCategoryActivity SendByEmail)
                List<int> categoriesids = new List<int>();
                foreach (var p in filteredProducts)
                {
                    if (!categoriesids.Contains(p.CategoryId))
                        categoriesids.Add(p.CategoryId);
                }

                string pdfFile = null;

                var _client = _order != null ? _order.Client : null;
                var pricelevel = _client != null ? _client.PriceLevel : 0;
                if (pricelevel > 0)
                    pricelevel -= 10;
                
                int priceLevel = pricelevel > 0 ? pricelevel : 0;

                // First try to get PDF from server (matches Xamarin: DataAccess.GetCatalogPdf)
                if (DataProvider.GetCatalogPdf(priceLevel, showPrice, showUPC, showUoM, categoriesids))
                {
                    pdfFile = Config.CatalogPDFPath;
                }
                else
                {
                    // Fall back to local generation (matches Xamarin: pdfHelper.GeneratePdfCatalog)
                    // Pass the filter options to respect user selections
                    var pdfHelper = new PdfHelper();
                    pdfFile = pdfHelper.GeneratePdfCatalog(null, filteredProducts, _order?.Client, showPrice, showUPC, showUoM);
                }

                await _dialogService.HideLoadingAsync();

                if (string.IsNullOrEmpty(pdfFile) || !System.IO.File.Exists(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("Error generating PDF catalog.", "Alert", "OK");
                    return;
                }

                // Navigate to PDF viewer with the PDF path (matches other email sending implementations)
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}");
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

        private class CategoryItem
        {
            public Category Category { get; set; } = null!;
            public System.Collections.Generic.List<Category> Subcategories { get; } = new();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
            System.Diagnostics.Debug.WriteLine("[ProductCatalog] GoBackAsync: removing state productcatalog, popping");
            await Shell.Current.GoToAsync("..");
            if (_viaFullCategory)
            {
                Helpers.NavigationHelper.RemoveNavigationState("fullcategory");
                await Shell.Current.GoToAsync("..");
            }
        }

        private enum ViewTypes
        {
            Normal = 0,
            Big = 1
        }
    }

    public partial class CatalogItemViewModel : ObservableObject
    {
        
        private OdLine _line = new();
        public OdLine Line 
        { 
            get => _line;
            set
            {
                if (SetProperty(ref _line, value))
                {
                    UpdateDisplay();
                }
            }
        }

        [ObservableProperty] public string _productImg = string.Empty;
        public ObservableCollection<OdLine> Values { get; } = new();

        [ObservableProperty]
        private string _upc = string.Empty;
        [ObservableProperty]
        private string _sku = string.Empty;
        [ObservableProperty]
        private string _code = string.Empty;
        
        [ObservableProperty]
        private int _productId = 0;
        
        [ObservableProperty]
        private int _orderId = 0;
        
        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _onHandText = "OH: 0";

        [ObservableProperty]
        private string _totalText = "$0.00";

        [ObservableProperty]
        private string _avgSaleText = string.Empty;

        [ObservableProperty]
        private string _uomText = string.Empty;

        [ObservableProperty]
        private string _listPriceText = string.Empty;
        
        [ObservableProperty]
        private string _Inventory = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        [ObservableProperty]
        private string _quantityButtonText = "+";

        [ObservableProperty]
        private bool _hasValues = false;

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private string _typeText = string.Empty;

        [ObservableProperty]
        private bool _showTypeText = false;

        [ObservableProperty]
        private Color _productNameColor = Colors.Black;

        [ObservableProperty]
        private bool _isSuggested = false;

        [ObservableProperty]
        private string _suggestedLabelText = string.Empty;

        /// <summary>Sum of qty across all sublines (Values). Used for Advanced Catalog style [-] qty [+] display.</summary>
        [ObservableProperty]
        private float _totalQty;
        
        public void UpdateDisplay()
        {
            if (Line == null)
                return;

            // Update on hand: show in line's UoM when applicable (e.g. cases vs each) so OH matches how user orders
            double baseInv = double.TryParse(_Inventory, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
            double displayInv = baseInv;
            if (Line.UoM != null && Line.UoM.Conversion > 0)
                displayInv = baseInv / Line.UoM.Conversion;
            var oh = displayInv >= 0 && displayInv == Math.Floor(displayInv)
                ? displayInv.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)
                : Math.Round(displayInv, Config.Round).ToString(System.Globalization.CultureInfo.InvariantCulture);
            OnHandText = $"OH:{oh}";

            // Update UoM
            UomText = Line.UoM != null ? Line.UoM.Name : string.Empty;

            // Update list price
            // This would need order context - simplified for now
            ListPriceText = $"List Price: {Line.ExpectedPrice.ToCustomString()}";

            // Update total
            var total = Values.Sum(v => v.Qty * v.Price);
            TotalText = $"Total: {total.ToCustomString()}";

            // Main line shows "+" only (add button); sublines show their qty in the subline list (Xamarin catalog behavior)
            QuantityButtonText = "+";

            // Update has values
            HasValues = Values.Count > 0 && Values.Any(v => v.Qty > 0);

            // Total qty (sum of all sublines) - for Advanced Catalog style single-row display
            TotalQty = (float)Values.Sum(v => v.Qty);

            // Update price and type text from first value with Qty > 0
            var firstValue = Values.FirstOrDefault(v => v.Qty > 0);
            if (firstValue != null)
            {
                PriceText = $"Price: {firstValue.Price.ToCustomString()}";
                if (firstValue.IsCredit)
                {
                    TypeText = $"Type:{(firstValue.Damaged ? "Dump" : "Return")}";
                    ShowTypeText = true;
                    ProductNameColor = Colors.Orange; // Orange for credit items
                }
                else
                {
                    TypeText = string.Empty;
                    ShowTypeText = false;
                    ProductNameColor = Colors.Black; // Black for regular items
                }
            }
            else
            {
                PriceText = string.Empty;
                TypeText = string.Empty;
                ShowTypeText = false;
                ProductNameColor = Colors.Black;
            }

            // Update avg sale
            if (Config.ShowAvgInCatalog)
            {
                AvgSaleText = $"Avg: {Line.AvgSale:F2}";
            }

            // // Check if product is suggested
            // if (Order != null && Order.Client != null && Product != null)
            // {
            //     IsSuggested = Product.IsSuggestedForClient(Order.Client, Product);
            //     if (IsSuggested)
            //     {
            //         SuggestedLabelText = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
            //             ? "Suggested Products" 
            //             : $"{Config.ProductCategoryNameIdentifier} Products";
            //     }
            //     else
            //     {
            //         SuggestedLabelText = string.Empty;
            //     }
            // }
            // else
            // {
            //     IsSuggested = false;
            //     SuggestedLabelText = string.Empty;
            // }
        }
    }
}
