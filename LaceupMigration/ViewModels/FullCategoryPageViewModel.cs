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

        public FullCategoryPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
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

                // Always show categories when orderId is provided (not products)
                _initialized = true;
                ShowCategories = true;
                ShowProducts = false;
                Title = "Categories";
                LoadCategories();
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
                ShowCategories = true;
                ShowProducts = false;
                Title = "Product Catalog";
                LoadCategories();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Client ID or Order ID is required.", "Error");
            }
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

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

            // Filter by client category if applicable
            if (_client != null && _client.CategoryId > 0)
            {
                var clientCategoryProducts = ClientCategoryProducts.Find(_client.CategoryId);
                if (clientCategoryProducts != null)
                {
                    var allowedCategoryIds = clientCategoryProducts.Products.Select(x => x.CategoryId).Distinct().ToList();
                    categoryList = categoryList.Where(x => allowedCategoryIds.Contains(x.Category.CategoryId)).ToList();
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
                    ShowSubcategories = !string.IsNullOrEmpty(subcategoriesText)
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
            if (SearchByCategory)
            {
                LoadCategories();
            }
            else if (SearchByProduct && !string.IsNullOrWhiteSpace(value) && _orderId.HasValue)
            {
                // When searching by product, navigate to ProductCatalog with search term
                // This matches Xamarin's SearchView_QueryTextSubmit behavior
                var route = $"productcatalog?orderId={_orderId.Value}&productSearch={Uri.EscapeDataString(value)}&comingFromSearch=yes";
                if (_asCreditItem)
                    route += "&asCreditItem=1";
                if (_asReturnItem)
                    route += "&asReturnItem=1";
                if (!string.IsNullOrEmpty(_comingFrom))
                    route += $"&comingFrom={Uri.EscapeDataString(_comingFrom)}";
                MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(route));
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

        [RelayCommand]
        public async Task CategorySelectedAsync(CategoryViewModel? item)
        {
            if (item == null || item.Category == null)
                return;

            if (Config.UseCatalog && _orderId.HasValue)
            {
                var route = $"productcatalog?orderId={_orderId.Value}&categoryId={item.Category.CategoryId}";
                if (!string.IsNullOrEmpty(_comingFrom))
                    route += $"&comingFrom={Uri.EscapeDataString(_comingFrom)}";
                if (_asCreditItem)
                    route += "&asCreditItem=1";
                if (_asReturnItem)
                    route += "&asReturnItem=1";
                await Shell.Current.GoToAsync(route);
                return;
            }

            // Both configs OFF - navigate to FullCategoryPage with products (default behavior)
            // If we have subcategories, show them or navigate to products
            if (item.Subcategories.Count > 0)
            {
                // For now, navigate to products in the main category
                // TODO: Could show subcategory selection dialog
                if (_orderId.HasValue)
                {
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_orderId.Value}&categoryId={item.Category.CategoryId}");
                }
                else if (_client != null)
                {
                    await Shell.Current.GoToAsync($"fullcategory?clientId={_client.ClientId}&categoryId={item.Category.CategoryId}");
                }
            }
            else
            {
                // Navigate to products in this category
                if (_orderId.HasValue)
                {
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_orderId.Value}&categoryId={item.Category.CategoryId}");
                }
                else if (_client != null)
                {
                    await Shell.Current.GoToAsync($"fullcategory?clientId={_client.ClientId}&categoryId={item.Category.CategoryId}");
                }
            }
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
            if (_order == null)
                return;

            var products = Product.GetProductListForOrder(_order, _asCreditItem, _categoryId ?? 0).ToList();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(_productSearch))
            {
                var searchLower = _productSearch.ToLowerInvariant();
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
                var price = Product.GetPriceForProduct(product, _order, out cameFromOffer, false, false, null);
                Products.Add(new ProductViewModel
                {
                    Product = product,
                    Name = product.Name,
                    Upc = product.Upc ?? string.Empty,
                    Price = price.ToString("C"),
                    Inventory = product.GetInventory(_order.AsPresale).ToString()
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

        public Product Product { get; set; } = null!;
    }
}

