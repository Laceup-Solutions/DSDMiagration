using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
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

        public FullCategoryPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
            _advancedOptionsService = advancedOptionsService;
        }

        public async Task InitializeAsync(int? clientId = null, int? orderId = null, int? categoryId = null, 
            string? productSearch = null, bool comingFromSearch = false, bool asCreditItem = false, 
            bool asReturnItem = false, int? productId = null, bool consignmentCounting = false, string? comingFrom = null)
        {
            _orderId = orderId;
            _categoryId = categoryId;
            _productSearch = productSearch;
            _comingFromSearch = comingFromSearch;
            _asCreditItem = asCreditItem;
            _asReturnItem = asReturnItem;
            _productId = productId;
            _consignmentCounting = consignmentCounting;
            _comingFrom = comingFrom;

            if (orderId.HasValue)
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId.Value);
                if (_order == null)
                {
                    await _dialogService.ShowAlertAsync("Order not found.", "Error");
                    return;
                }
                _client = _order.Client;

                // Check config to determine which catalog to use
                if (Config.UseLaceupAdvancedCatalog)
                {
                    // Redirect to AdvancedCatalog
                    var route = $"advancedcatalog?orderId={orderId.Value}";
                    if (categoryId.HasValue)
                        route += $"&categoryId={categoryId.Value}";
                    if (!string.IsNullOrEmpty(productSearch))
                        route += $"&search={Uri.EscapeDataString(productSearch)}";
                    if (asCreditItem)
                        route += "&itemType=1"; // 0=Sales, 1=Dump, 2=Return
                    if (asReturnItem)
                        route += "&itemType=2";
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
                if (categoryId.HasValue && categoryId.Value > 0)
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
                // If categoryId is provided, show products; otherwise show categories
                if (categoryId.HasValue && categoryId.Value > 0)
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
                // No clientId or orderId - show products if categoryId provided, otherwise show categories (matches Xamarin behavior)
                _initialized = true;
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    // Show products for this category (matches Xamarin FullProductListActivity when clientId is 0)
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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchLower = SearchQuery.ToLowerInvariant();
                if (SearchByCategory)
                {
                    categoryList = categoryList.Where(x => 
                        x.Category.Name.ToLowerInvariant().Contains(searchLower) ||
                        x.Subcategories.Any(s => s.Name.ToLowerInvariant().Contains(searchLower))).ToList();
                }
                else
                {
                    // Search by product - filter categories that have matching products
                    var matchingProductIds = Product.Products
                        .Where(p => p.Name.ToLowerInvariant().Contains(searchLower) || 
                                   p.Upc.ToLowerInvariant().Contains(searchLower))
                        .Select(p => p.CategoryId)
                        .Distinct()
                        .ToList();

                    categoryList = categoryList.Where(x => 
                        matchingProductIds.Contains(x.Category.CategoryId) ||
                        x.Subcategories.Any(s => matchingProductIds.Contains(s.CategoryId))).ToList();
                }
            }

            // Update UI
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

            ShowNoCategories = Categories.Count == 0;
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Match Xamarin's SearchBar_QueryTextChange behavior
            if (ShowProducts)
            {
                // When showing products (FullProductListActivity), filter products in real-time
                LoadProducts();
            }
            else if (ShowCategories)
            {
                // When showing categories (FullCategoryActivity), only filter categories in real-time when "By Category" is selected
                // When "By Product" is selected, do nothing on text change (only on submit)
                if (SearchByCategory)
                {
                    LoadCategories();
                }
                // When SearchByProduct is selected, do nothing on text change
                // Navigation will happen on SearchButtonPressed (matches Xamarin's SearchView_QueryTextSubmit)
            }
        }

        partial void OnSearchByCategoryChanged(bool value)
        {
            if (value)
                SearchByProduct = false;
        }

        partial void OnSearchByProductChanged(bool value)
        {
            if (value)
                SearchByCategory = false;
        }

        public async Task HandleProductSearchSubmitAsync()
        {
            // Match Xamarin's SearchView_QueryTextSubmit behavior
            // When "By Product" is selected and search button is pressed, navigate to FullProductListActivity (FullCategoryPage)
            // Xamarin always navigates to FullProductListActivity with clientId and search term, regardless of orderId
            if (SearchByProduct && !string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchTerm = SearchQuery.ToLowerInvariant();
                
                // Always navigate to FullCategoryPage (FullProductListActivity) with search term (matches Xamarin exactly)
                var route = $"fullcategory?productSearch={Uri.EscapeDataString(searchTerm)}&comingFromSearch=yes";
                
                if (_orderId.HasValue)
                {
                    // Include orderId if available
                    route = $"fullcategory?orderId={_orderId.Value}&productSearch={Uri.EscapeDataString(searchTerm)}&comingFromSearch=yes";
                    if (_asCreditItem)
                        route += "&asCreditItem=1";
                    if (_asReturnItem)
                        route += "&asReturnItem=1";
                    if (!string.IsNullOrEmpty(_comingFrom))
                        route += $"&comingFrom={Uri.EscapeDataString(_comingFrom)}";
                }
                else if (_client != null)
                {
                    // Include clientId if available (matches Xamarin's clientIdIntent)
                    route = $"fullcategory?clientId={_client.ClientId}&productSearch={Uri.EscapeDataString(searchTerm)}&comingFromSearch=yes";
                }
                
                await Shell.Current.GoToAsync(route);
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
                // No order but have client - use GetProductListForClient (matches Xamarin behavior)
                products = Product.GetProductListForClient(_client, _categoryId ?? 0, _productSearch ?? string.Empty).ToList();
            }
            else
            {
                // No order or client - load all products filtered by categoryId (matches Xamarin FullProductListActivity when clientId is 0)
                if (_categoryId.HasValue && _categoryId.Value > 0)
                {
                    products = Product.Products.Where(x => x.CategoryId == _categoryId.Value).ToList();
                }
                else
                {
                    products = Product.Products.Where(x => x.CategoryId > 0).ToList();
                }
            }

            // Apply search filter if provided (from initialization or current search query)
            var searchTerm = !string.IsNullOrWhiteSpace(SearchQuery) ? SearchQuery : _productSearch;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLowerInvariant();
                products = products.Where(x =>
                    x.Name.ToLowerInvariant().IndexOf(searchLower) != -1 ||
                    x.Upc.ToLowerInvariant().Contains(searchLower) ||
                    x.Sku.ToLowerInvariant().Contains(searchLower) ||
                    x.Code.ToLowerInvariant().Contains(searchLower) ||
                    x.Description.ToLowerInvariant().Contains(searchLower)
                ).ToList();
            }

            // Apply category filter if provided
            if (_categoryId.HasValue && _categoryId.Value > 0)
            {
                products = products.Where(x => x.CategoryId == _categoryId.Value).ToList();
            }

            // Sort products
            products = SortDetails.SortedDetails(products.Where(x => x.CategoryId > 0).ToList()).ToList();

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

            ShowNoCategories = Products.Count == 0;
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
            // try
            // {
            //     var scanResult = await _scannerService.ScanAsync();
            //     if (string.IsNullOrEmpty(scanResult))
            //         return;
            //
            //     // Find product by barcode
            //     var product = Product.Products.FirstOrDefault(p =>
            //         p.Barcode == scanResult ||
            //         p.Barcode2 == scanResult ||
            //         p.Barcode3 == scanResult);
            //
            //     if (product != null)
            //     {
            //         // TODO: Navigate to product details or add to order
            //         await _dialogService.ShowAlertAsync($"Found product: {product.Name}", "Info");
            //     }
            //     else
            //     {
            //         await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Logger.CreateLog($"Error scanning: {ex.Message}");
            //     await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            // }
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
            await _dialogService.ShowAlertAsync("Send by Email functionality is not yet fully implemented in the MAUI version.", "Info");
            // TODO: Implement catalog PDF generation and email sending
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

