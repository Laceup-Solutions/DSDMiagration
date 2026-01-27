using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using LaceupMigration.Business.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class FullCategoryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly IScannerService _scannerService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private Client? _client;
        private Order? _order;
        private bool _initialized;
        private int? _orderId;
        private int? _categoryId;
        private string? _productSearch;
        private bool _comingFromSearch;
        private bool _asCreditItem;
        private bool _asReturnItem;
        private int? _productId;
        private bool _consignmentCounting;
        private string? _comingFrom; // "Credit" or "PreviouslyOrdered"
        private int? _scannedProductId; // Product ID from camera scan
        private bool _fromLoadOrder = false; // Flag to indicate coming from Load Order (Prod button)

        public ObservableCollection<CategoryViewModel> Categories { get; } = new();
        public ObservableCollection<ProductViewModel> Products { get; } = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _searchByCategory = true;

        [ObservableProperty]
        private bool _searchByProduct;

        [ObservableProperty]
        private bool _showNoCategories;

        [ObservableProperty]
        private bool _showCategories = true;

        [ObservableProperty]
        private bool _showProducts;

        [ObservableProperty]
        private string _title = "Product Catalog";

        public FullCategoryPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService, ICameraBarcodeScannerService cameraBarcodeScanner, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
            _advancedOptionsService = advancedOptionsService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
        }

        public async Task InitializeAsync(int? clientId = null, int? orderId = null, int? categoryId = null, 
            string? productSearch = null, bool comingFromSearch = false, bool asCreditItem = false, 
            bool asReturnItem = false, int? productId = null, bool consignmentCounting = false, string? comingFrom = null, bool fromLoadOrder = false)
        {
            
            if (!string.IsNullOrEmpty(productSearch))
                productSearch = Uri.UnescapeDataString(productSearch);
            
            
            _orderId = orderId;
            _categoryId = categoryId;
            _productSearch = productSearch;
            _comingFromSearch = comingFromSearch;
            _asCreditItem = asCreditItem;
            _asReturnItem = asReturnItem;
            _productId = productId;
            _consignmentCounting = consignmentCounting;
            _comingFrom = comingFrom;
            _fromLoadOrder = fromLoadOrder;
            _scannedProductId = null; // Clear scanned product ID on initialization
            
            if (comingFromSearch && !string.IsNullOrEmpty(productSearch))
            {
                SearchByProduct = true;
                SearchByCategory = false;
                SearchQuery = productSearch;
            }
            if (orderId.HasValue)
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId.Value);
                if (_order == null)
                {
                    // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                    return;
                }
                _client = _order.Client;

                // Check config to determine which catalog to use
                if (Config.UseLaceupAdvancedCatalog)
                {
                    // Redirect to AdvancedCatalog - pass fromLoadOrder flag
                    var route = $"advancedcatalog?orderId={orderId.Value}";
                    if (categoryId.HasValue)
                        route += $"&categoryId={categoryId.Value}";
                    if (!string.IsNullOrEmpty(productSearch))
                        route += $"&search={Uri.EscapeDataString(productSearch)}";
                    if (asCreditItem)
                        route += "&itemType=1"; // 0=Sales, 1=Dump, 2=Return
                    if (asReturnItem)
                        route += "&itemType=2";
                    if (fromLoadOrder)
                        route += "&fromLoadOrder=1";
                    await Shell.Current.GoToAsync(route);
                    return;
                }
                else if (Config.UseCatalog)
                {
                    // If categoryId is provided, navigate directly to ProductCatalog
                    // Otherwise, show categories first (don't redirect)
                    if (categoryId.HasValue || !string.IsNullOrEmpty(productSearch) || comingFromSearch)
                    {
                        // Redirect to ProductCatalog
                        var route = $"productcatalog?orderId={orderId.Value}";
                        if (categoryId.HasValue)
                            route += $"&categoryId={categoryId.Value}";
                        if (!string.IsNullOrEmpty(productSearch))
                            route += $"&productSearch={Uri.EscapeDataString(productSearch)}";
                        if (comingFromSearch)
                            route += "&comingFromSearch=yes";
                        if (asCreditItem)
                            route += "&asCreditItem=1";
                        if (asReturnItem)
                            route += "&asReturnItem=1";
                        if (productId.HasValue)
                            route += $"&productId={productId.Value}";
                        if (consignmentCounting)
                            route += "&consignmentCounting=1";
                        if (!string.IsNullOrEmpty(_comingFrom))
                            route += $"&comingFrom={Uri.EscapeDataString(_comingFrom)}";
                        await Shell.Current.GoToAsync(route);
                        return;
                    }
                    // Otherwise, show categories (fall through to show categories)
                }

                // If categoryId is provided, show products; otherwise show categories
                _initialized = true;
                if ((categoryId.HasValue && categoryId.Value > 0) || (comingFromSearch && !string.IsNullOrEmpty(productSearch)))
                {
                    ShowCategories = false;
                    ShowProducts = true;
                    Title = "Products";
                    LoadProducts();
                }
                else
                {
                    ShowCategories = true;
                    ShowProducts = false;
                    Title = "Categories";
                    LoadCategories();
                }
            }
            else if (clientId.HasValue)
            {
                if (_initialized && _client?.ClientId == clientId.Value)
                {
                    await RefreshAsync();
                    return;
                }

                _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId.Value);
                if (_client == null)
                {
                    await _dialogService.ShowAlertAsync("Client not found.", "Error");
                    return;
                }

                _initialized = true;
                // If categoryId is provided or coming from search, show products; otherwise show categories
                if ((categoryId.HasValue && categoryId.Value > 0) || (comingFromSearch && !string.IsNullOrEmpty(productSearch)))
                {
                    ShowCategories = false;
                    ShowProducts = true;
                    Title = "Products";
                    LoadProducts();
                }
                else
                {
                    ShowCategories = true;
                    ShowProducts = false;
                    Title = "Product Catalog";
                    LoadCategories();
                }
            }
            else
            {
                // No clientId or orderId - show products if categoryId provided or coming from search, otherwise show categories (matches Xamarin behavior)
                _initialized = true;
                if ((categoryId.HasValue && categoryId.Value > 0) || (comingFromSearch && !string.IsNullOrEmpty(productSearch)))
                {
                    // Show products for this category or search (matches Xamarin FullProductListActivity when clientId is 0)
                    ShowCategories = false;
                    ShowProducts = true;
                    Title = "Products";
                    LoadProducts();
                }
                else
                {
                    ShowCategories = true;
                    ShowProducts = false;
                    Title = "Categories";
                    LoadCategories();
                }
            }
        }

        public async Task OnAppearingAsync()
        {
            // If not initialized, initialize with no parameters (matches Xamarin behavior)
            if (!_initialized)
            {
                await InitializeAsync();
                return;
            }

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            // Always show categories when initialized with orderId (unless categoryId/productSearch is provided)
            if (_orderId.HasValue && !_categoryId.HasValue && string.IsNullOrEmpty(_productSearch) && !_comingFromSearch)
            {
                ShowCategories = true;
                ShowProducts = false;
                LoadCategories();
            }
            else if (ShowProducts)
            {
                LoadProducts();
            }
            else
            {
                LoadCategories();
            }
            await Task.CompletedTask;
        }

        private void LoadCategories()
        {
            
            var allCategories = Category.Categories.ToList();
            var categoryList = new System.Collections.Generic.List<CategoryItem>();

            // Get parent categories (matches Xamarin FillCategoryList lines 158-161)
            foreach (var category in allCategories.Where(x => x.ParentCategoryId == 0))
            {
                var item = new CategoryItem { Category = category };

                // Add subcategories (matches Xamarin FillCategoryList lines 163-168)
                foreach (var subcat in allCategories.Where(x => x.ParentCategoryId == category.CategoryId))
                {
                    item.Subcategories.Add(subcat);
                }

                categoryList.Add(item);
            }

            // Filter by site if Config.SalesmanCanChangeSite is enabled (matches Xamarin's FillCategoryList lines 170-179)
            if (Config.SalesmanCanChangeSite)
            {
                if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                {
                    var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                    var allow_categories = Product.Products.Where(x => allowedProducts.Contains(x.ProductId)).Select(x => x.CategoryId).Distinct().ToList();

                    categoryList = categoryList.Where(x => allow_categories.Contains(x.Category.CategoryId)).ToList();
                }
            }

            // Apply search filter (matches Xamarin RefreshListView lines 221-222 exactly)
            // Note: In Xamarin, templateCriteria is set to e.NewText (not lowercased) in QueryTextChange
            // but the comparison uses ToLowerInvariant() on both sides
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Match Xamarin line 222: x.Category.Name.ToLowerInvariant().Contains(templateCriteria.ToLowerInvariant())
                var searchTerm = SearchQuery.Trim().ToLowerInvariant();
                categoryList = categoryList.Where(x => x.Category.Name.ToLowerInvariant().Contains(searchTerm)).ToList();
            }

            // Update UI (matches Xamarin: categoryList is already ordered in FillCategoryList line 181)
            Categories.Clear();
            foreach (var item in categoryList.OrderBy(x => x.Category.Name))
            {
                var subcategoriesText = item.Subcategories.Count > 0
                    ? $"Subcategories: {string.Join(", ", item.Subcategories.Select(s => s.Name))}"
                    : string.Empty;

                var viewModel = new CategoryViewModel
                {
                    Category = item.Category,
                    Name = item.Category.Name,
                    SubcategoriesText = subcategoriesText,
                    ShowSubcategories = !string.IsNullOrEmpty(subcategoriesText),
                    IsExpanded = item.Category.Expanded // Use category's expanded state
                };
                
                // Add subcategories to the view model
                foreach (var subcat in item.Subcategories)
                {
                    viewModel.Subcategories.Add(subcat);
                }
                
                Categories.Add(viewModel);
            }

            ShowNoCategories = ShowCategories && Categories.Count == 0;
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Clear scanned product ID when user manually changes search query
            if (SearchByProduct && !string.IsNullOrEmpty(value))
            {
                _scannedProductId = null;
                LoadProducts();
                return;
            }
            
            if (SearchByProduct)
                return;
            
            // When searching by category, ensure we're showing categories
            if (ShowProducts)
            {
                ShowCategories = true;
                ShowProducts = false;
                Title = "Categories";
            }
            
            LoadCategories();
        }

        partial void OnSearchByCategoryChanged(bool value)
        {
            if (value)
            {
                SearchByProduct = false;
                // When switching to "By Category", always show categories and reload them
                // Clear search query and reset search-related flags when switching from product search to category search
                // This ensures categories are shown without product search filter
                SearchQuery = string.Empty;
                _productSearch = null;
                _comingFromSearch = false;
                ShowCategories = true;
                ShowProducts = false;
                Title = "Categories";
                LoadCategories();
            }
        }

        partial void OnSearchByProductChanged(bool value)
        {
            if (value)
            {
                SearchByCategory = false;
                // Clear scanned product ID when switching to product search mode manually
                _scannedProductId = null;
            }
        }

        public async Task HandleProductSearchSubmitAsync()
        {
            var templateCriteria = SearchQuery.Trim().ToLower();

            if (!SearchByProduct || string.IsNullOrWhiteSpace(templateCriteria)) 
                return;
    
            var route = $"fullcategory?productSearch={Uri.EscapeDataString(templateCriteria)}&comingFromSearch=yes";

            if (_orderId.HasValue)
            {
                route =
                    $"fullcategory?orderId={_orderId.Value}&productSearch={Uri.EscapeDataString(templateCriteria)}&comingFromSearch=yes";
            }
            else if (_client != null)
            {
                route =
                    $"fullcategory?clientId={_client.ClientId}&productSearch={Uri.EscapeDataString(templateCriteria)}&comingFromSearch=yes";
            }

            await Shell.Current.GoToAsync(route);
        }
        
        private async Task SelectCreditTypeAsync(int productId, int defaultType = 0)
        {
            List<CreditType> items = new List<CreditType>();
    
            if (defaultType == 0)
            {
                items.Add(new CreditType { Description = "Dump", Damaged = true });
                items.Add(new CreditType { Description = "Return", Damaged = false });
            }

            if (Config.CreditReasonInLine)
            {
                List<Reason> reason = new List<Reason>();
                if (defaultType == 0 || defaultType == 1)
                    reason.AddRange(Reason.GetReasonsByType(ReasonType.Dump));
                if (defaultType == 0 || defaultType == 2)
                    reason.AddRange(Reason.GetReasonsByType(ReasonType.Return));

                if (reason.Count > 0)
                {
                    items = new List<CreditType>();
                    foreach (var r in reason)
                        items.Add(new CreditType 
                        { 
                            Description = r.Description, 
                            Damaged = (r.AvailableIn & (int)ReasonType.Dump) > 0, 
                            ReasonId = r.Id 
                        });
                }
            }

            if (items.Count == 0)
            {
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={productId}&asCreditItem=0");
                return;
            }

            var choice = await _dialogService.ShowActionSheetAsync(
                "Type of Credit Item", 
                "Cancel", 
                null, 
                items.Select(x => x.Description).ToArray());

            if (choice == null || choice == "Cancel")
                return;

            var selectedItem = items.FirstOrDefault(x => x.Description == choice);
            if (selectedItem != null)
            {
                var type = selectedItem.Damaged ? 1 : 0;
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={productId}&asCreditItem=1&type={type}");
            }
        }


        public async Task ToggleCategoryExpanded(CategoryViewModel viewModel)
        {
            if (viewModel.Subcategories.Count == 0)
            {
                // No subcategories, navigate directly
                await CategorySelectedAsync(viewModel);
                return;
            }

            // Toggle expansion
            viewModel.IsExpanded = !viewModel.IsExpanded;
            viewModel.Category.Expanded = viewModel.IsExpanded;
        }

        [RelayCommand]
        public async Task CategorySelectedAsync(CategoryViewModel? item)
        {
            if (item == null || item.Category == null)
                return;

            // Match Xamarin's GoToCategory behavior - always navigate to products for the category
            // This is called when a category with no subcategories is clicked
            int categoryId = item.Category.CategoryId;

            if (Config.UseCatalog && _orderId.HasValue)
            {
                var route = $"productcatalog?orderId={_orderId.Value}&categoryId={categoryId}";
                if (!string.IsNullOrEmpty(_comingFrom))
                    route += $"&comingFrom={Uri.EscapeDataString(_comingFrom)}";
                if (_asCreditItem)
                    route += "&asCreditItem=1";
                if (_asReturnItem)
                    route += "&asReturnItem=1";
                await Shell.Current.GoToAsync(route);
                return;
            }

            // Both configs OFF - navigate to FullCategoryPage with products (matches Xamarin's FullProductListActivity)
            if (_orderId.HasValue)
            {
                await Shell.Current.GoToAsync($"fullcategory?orderId={_orderId.Value}&categoryId={categoryId}");
            }
            else if (_client != null)
            {
                await Shell.Current.GoToAsync($"fullcategory?clientId={_client.ClientId}&categoryId={categoryId}");
            }
            else
            {
                // No order or client - navigate with just categoryId (matches Xamarin when clientId is 0)
                await Shell.Current.GoToAsync($"fullcategory?categoryId={categoryId}");
            }
        }

        [RelayCommand]
        public async Task ProductImageClickedAsync(ProductViewModel? item)
        {
            if (item == null || item.Product == null)
                return;

            // Match Xamarin's ImageClicked behavior - navigate to ProductImageActivity
            // Navigate to ProductImagePage (matches Xamarin's ProductImageActivity)
            await Shell.Current.GoToAsync($"productimage?productId={item.Product.ProductId}");
        }

        [RelayCommand]
        public async Task ViewProductDetailsAsync(ProductViewModel? item)
        {
            if (item == null || item.Product == null)
                return;

            // Match Xamarin's ProductNameClickedHandler behavior - navigate to ProductDetailsActivity
            var route = $"productdetails?productId={item.Product.ProductId}";
            if (_client != null)
            {
                route += $"&clientId={_client.ClientId}";
            }
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        public async Task ProductSelectedAsync(ProductViewModel? item)
        {
            if (item == null || item.Product == null || _order == null)
                return;

            if (Config.UseCatalog)
            {
                // Navigate to ProductCatalogPage with the selected product
                var route = $"productcatalog?orderId={_order.OrderId}&productId={item.Product.ProductId}";
                if (_categoryId.HasValue)
                    route += $"&categoryId={_categoryId.Value}";
                await Shell.Current.GoToAsync(route);
                return;
            }

            // Both configs OFF - navigate to AddItemPage (default behavior)
            // Check inventory
            if (item.Product.GetInventory(_order.AsPresale) <= 0)
            {
                await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                return;
            }

            // Navigate to AddItemPage
            // But first, let's check if it's a credit item and needs type selection
            if (_asCreditItem)
            {
                // Show credit type selection
                var creditTypes = new List<string> { "Dump", "Return" };
                if (Config.CreditReasonInLine)
                {
                    var reasons = new List<Reason>();
                    reasons.AddRange(Reason.GetReasonsByType(ReasonType.Dump));
                    reasons.AddRange(Reason.GetReasonsByType(ReasonType.Return));
                    if (reasons.Count > 0)
                    {
                        creditTypes = reasons.Select(r => r.Description).ToList();
                    }
                }

                if (creditTypes.Count > 1)
                {
                    var choice = await _dialogService.ShowActionSheetAsync("Select Credit Type", "Cancel", null, creditTypes.ToArray());
                    if (choice == null || choice == "Cancel")
                        return;

                    // Determine type and reason
                    int type = 0;
                    int reasonId = 0;
                    if (Config.CreditReasonInLine)
                    {
                        var reason = Reason.GetReasonsByType(ReasonType.Dump).Concat(Reason.GetReasonsByType(ReasonType.Return))
                            .FirstOrDefault(r => r.Description == choice);
                        if (reason != null)
                        {
                            reasonId = reason.Id;
                            type = (reason.AvailableIn & (int)ReasonType.Dump) > 0 ? 1 : 2;
                        }
                    }
                    else
                    {
                        type = choice == "Dump" ? 1 : 2;
                    }

                    // Navigate to AddItemPage with credit type
                    await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={item.Product.ProductId}&asCreditItem=1&type={type}&reasonId={reasonId}");
                }
                else
                {
                    // Navigate directly to AddItemPage
                    await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&productId={item.Product.ProductId}&asCreditItem=1");
                }
            }
            else
            {
                // Regular product - navigate to AddItemPage
                var route = $"additem?orderId={_order.OrderId}&productId={item.Product.ProductId}";
                if (_consignmentCounting)
                {
                    route += "&consignmentCounting=1";
                }
                await Shell.Current.GoToAsync(route);
            }
        }

        private void LoadProducts()
        {
            List<Product> products;
            
            if (_order != null)
            {
                // Use order-based product list
                products = Product.GetProductListForOrder(_order, _asCreditItem, _categoryId ?? 0).ToList();
            }
            else if (_client != null)
            {
                // No order but have client - use GetProductListForClient (matches Xamarin line 63)
                // Match Xamarin: pass empty string for search, filter after (lines 79-82)
                products = SortDetails.SortedDetails(Product.GetProductListForClient(_client, _categoryId ?? 0, string.Empty)).ToList();
            }
            else
            {
                // No order or client - load all products filtered by categoryId (matches Xamarin FullProductListActivity lines 53-56)
                if (_categoryId.HasValue && _categoryId.Value > 0)
                {
                    products = SortDetails.SortedDetails(Product.Products.Where(x => x.CategoryId == _categoryId.Value).ToList()).ToList();
                }
                else
                {
                    products = SortDetails.SortedDetails(Product.Products.Where(x => x.CategoryId > 0).ToList()).ToList();
                }
            }

            // Apply site filtering if enabled (matches Xamarin lines 66-73)
            if (Config.SalesmanCanChangeSite)
            {
                if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                {
                    var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                    products = products.Where(x => allowedProducts.Contains(x.ProductId)).ToList();
                }
            }

            // If we have a scanned product ID, filter to show only that product
            if (_scannedProductId.HasValue)
            {
                products = products.Where(x => x.ProductId == _scannedProductId.Value).ToList();
            }
            else
            {
                // Apply search filter if provided (matches Xamarin lines 79-86)
                // Match Xamarin: only filter by Name, and only if searchIntent is provided
                var searchTerm = !string.IsNullOrWhiteSpace(SearchQuery) ? SearchQuery.Trim() : (_productSearch?.Trim() ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLowerInvariant();
                    // Match Xamarin lines 81-82: filter by Name only
                    var products_temp = products.Where(x => x.CategoryId > 0);
                    products = products_temp.Where(x => x.Name.ToLowerInvariant().Contains(searchLower)).ToList();
                }
                else
                {
                    // Match Xamarin line 90: if no search, show all products with CategoryId > 0
                    products = products.Where(x => x.CategoryId > 0).ToList();
                }
            }

            // Apply category filter if provided (this is already handled in the initial product loading above)
            // Note: In Xamarin, categoryId is used when getting products, not as a separate filter after

            // Sort products (already sorted above, but ensure CategoryId > 0 filter is applied)
            // Note: Xamarin doesn't re-sort after search filter, but we ensure CategoryId > 0

            // Update UI
            Products.Clear();
            foreach (var product in products)
            {
                bool cameFromOffer = false;
                // Default to PriceLevel0 (matches Xamarin: var pp = product.PriceLevel0;)
                decimal price = (decimal)product.PriceLevel0;
                
                if (_order != null)
                {
                    price = (decimal)Product.GetPriceForProduct(product, _order, out cameFromOffer, false, false, null);
                }
                else if (_client != null)
                {
                    // Calculate price for client (matches Xamarin: Product.GetPriceForProduct(product, activity.client, false, false))
                    price = (decimal)Product.GetPriceForProduct(product, _client, false, false);
                }
                
                // Get inventory (matches Xamarin: Config.TrackInventory ? product.CurrentInventory : product.CurrentWarehouseInventory)
                var rawInventory = Config.TrackInventory ? product.CurrentInventory : product.CurrentWarehouseInventory;
                
                // Get product image
                var imagePath = ProductImage.GetProductImage(product.ProductId);
                var hasImage = !string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath);
                
                // Get packaging
                double package = 1;
                if (!string.IsNullOrEmpty(product.Package))
                    double.TryParse(product.Package, out package);
                
                // Get weight
                var showWeight = product.Weight > 0;
                
                // Get UoM information
                var familyUoms = product.UnitOfMeasures;
                var showUom = familyUoms != null && familyUoms.Count > 0;
                var showUom1 = false;
                var showPrice1 = false;
                var showOnHand1 = false;
                string uom = string.Empty;
                string uom1 = string.Empty;
                string price1 = string.Empty;
                string onHand1 = string.Empty;
                decimal basePrice = price;
                double displayInventory = rawInventory;
                
                if (showUom)
                {
                    if (familyUoms.Count == 1)
                    {
                        var tempUom = familyUoms.FirstOrDefault(x => x.IsBase);
                        if (tempUom != null)
                        {
                            uom = tempUom.Name;
                            // Adjust price and inventory for UoM
                            basePrice = price * (decimal)tempUom.Conversion;
                            displayInventory = rawInventory / tempUom.Conversion;
                        }
                    }
                    else
                    {
                        var baseUom = familyUoms.FirstOrDefault(x => x.IsBase);
                        var bigUom = familyUoms.OrderByDescending(x => x.Conversion).FirstOrDefault();
                        
                        double weightFactor = 1;
                        if (Config.IncludeAvgWeightInCatalogPrice && product.Weight > 0)
                            weightFactor = product.Weight;
                        
                        if (bigUom != null && bigUom != baseUom)
                        {
                            // Show both UoMs
                            showUom1 = true;
                            showPrice1 = true;
                            showOnHand1 = true;
                            
                            uom = bigUom.Name;
                            uom1 = baseUom?.Name ?? string.Empty;
                            
                            // Big UoM price and inventory
                            basePrice = price * (decimal)(bigUom.Conversion * weightFactor);
                            displayInventory = rawInventory / bigUom.Conversion;
                            
                            // Base UoM price and inventory
                            price1 = (price * (decimal)weightFactor).ToString("C");
                            onHand1 = Math.Round(rawInventory / (baseUom?.Conversion ?? 1), 2).ToString();
                        }
                        else if (baseUom != null)
                        {
                            uom = baseUom.Name;
                            basePrice = price * (decimal)(baseUom.Conversion * weightFactor);
                            displayInventory = rawInventory / baseUom.Conversion;
                        }
                    }
                }
                
                // Format inventory text (matches Xamarin: activity.GetString(Resource.String.onHandData) + inventory.ToString(CultureInfo.CurrentCulture))
                var onHandText = Math.Round(displayInventory, 2).ToString();
                
                Products.Add(new ProductViewModel
                {
                    Product = product,
                    Name = product.Name,
                    Upc = product.Upc ?? string.Empty,
                    Price = basePrice.ToString("C"),
                    Inventory = rawInventory.ToString(),
                    OnHandText = onHandText,
                    ProductImagePath = imagePath ?? string.Empty,
                    HasImage = hasImage,
                    Packaging = package.ToString(),
                    Weight = product.Weight > 0 ? product.Weight.ToString() : string.Empty,
                    ShowWeight = showWeight,
                    Uom = uom,
                    Uom1 = uom1,
                    Price1 = price1,
                    OnHand1 = onHand1,
                    ShowUom = showUom,
                    ShowUom1 = showUom1,
                    ShowPrice1 = showPrice1,
                    ShowOnHand1 = showOnHand1,
                    ShowPrice = !Config.HidePriceInTransaction
                });
            }

            ShowNoCategories = ShowProducts && Products.Count == 0;
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
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
                    // Set the scanned product ID to filter to only this product
                    _scannedProductId = product.ProductId;
                    
                    // Switch to product search mode and show products view
                    SearchByProduct = true;
                    SearchByCategory = false;
                    ShowProducts = true;
                    ShowCategories = false;
                    
                    // Clear search query since we're filtering by product ID
                    SearchQuery = string.Empty;
                    
                    // Reload products - will filter to show only the scanned product
                    LoadProducts();
                    
                    // If we have an order, navigate to add item page
                    if (_order != null)
                    {
                        var route = $"additem?orderId={_order.OrderId}&productId={product.ProductId}";
                        if (_asCreditItem)
                            route += "&asCreditItem=1";
                        if (_asReturnItem)
                            route += "&asReturnItem=1";
                        if (_consignmentCounting)
                            route += "&consignmentCounting=1";
                        await Shell.Current.GoToAsync(route);
                    }
                    // If no order, the product will be shown filtered in the list
                }
                else
                {
                    // Product not found - clear scanned product ID and use search query
                    _scannedProductId = null;
                    
                    // Ensure we're showing products view (not categories)
                    ShowProducts = true;
                    ShowCategories = false;
                    SearchQuery = scanResult;
                    SearchByProduct = true;
                    SearchByCategory = false;
                    LoadProducts();
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
        private async Task ShowMenuAsync()
        {
            var options = new[] { "Send by Email", "Advanced Options" };
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options);
            
            switch (choice)
            {
                case "Send by Email":
                    await SendByEmailAsync();
                    break;
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

                // Try to get PDF from server first, then fall back to local generation (matches Xamarin logic)
                int priceLevel = _client != null ? _client.PriceLevel : 0;

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
                    pdfFile = pdfHelper.GeneratePdfCatalog(null, filteredProducts, _client, showPrice, showUPC, showUoM);
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
    }

    public partial class CategoryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _subcategoriesText = string.Empty;

        [ObservableProperty]
        private bool _showSubcategories;

        [ObservableProperty]
        private bool _isExpanded;

        public Category Category { get; set; } = null!;
        
        public System.Collections.Generic.List<Category> Subcategories { get; } = new();
    }

    public partial class ProductViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _upc = string.Empty;

        [ObservableProperty]
        private string _price = string.Empty;

        [ObservableProperty]
        private string _inventory = string.Empty;

        [ObservableProperty]
        private string _productImagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        [ObservableProperty]
        private string _packaging = string.Empty;

        [ObservableProperty]
        private string _weight = string.Empty;

        [ObservableProperty]
        private bool _showWeight = false;

        [ObservableProperty]
        private string _uom = string.Empty;

        [ObservableProperty]
        private string _uom1 = string.Empty;

        [ObservableProperty]
        private string _price1 = string.Empty;

        [ObservableProperty]
        private string _onHand1 = string.Empty;

        [ObservableProperty]
        private bool _showUom = false;

        [ObservableProperty]
        private bool _showUom1 = false;

        [ObservableProperty]
        private bool _showPrice1 = false;

        [ObservableProperty]
        private bool _showOnHand1 = false;

        [ObservableProperty]
        private string _onHandText = string.Empty;

        [ObservableProperty]
        private bool _showPrice = true;

        public Product Product { get; set; } = null!;
    }
}

