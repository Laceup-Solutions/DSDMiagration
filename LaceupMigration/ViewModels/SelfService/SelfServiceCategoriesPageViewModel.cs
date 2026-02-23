using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;
using LaceupMigration.ViewModels;
using LaceupMigration.Business.Interfaces;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCategoriesPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly MainPageViewModel _mainPageViewModel;
        private Order _order;
        private int? _categoryId;
        private string _productSearch;
        private bool _comingFromSearch;
        private int? _scannedProductId;

        [ObservableProperty]
        private string _clientName = string.Empty;

        private ObservableCollection<CategoryViewModel> _categories = new();
        public ObservableCollection<CategoryViewModel> Categories
        {
            get => _categories;
            private set => SetProperty(ref _categories, value);
        }

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
        private string _title = "Categories";

        public SelfServiceCategoriesPageViewModel(IDialogService dialogService, ILaceupAppService appService, ICameraBarcodeScannerService cameraBarcodeScanner, AdvancedOptionsService advancedOptionsService, MainPageViewModel mainPageViewModel)
        {
            _dialogService = dialogService;
            _appService = appService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            _advancedOptionsService = advancedOptionsService;
            _mainPageViewModel = mainPageViewModel;
        }

        public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    ClientName = _order.Client?.ClientName ?? string.Empty;
                }
            }

            _categoryId = null;
            _productSearch = null;
            _comingFromSearch = false;
            _scannedProductId = null;
            ShowCategories = true;
            ShowProducts = false;
            Title = "Categories";
            SearchByCategory = true;
            SearchByProduct = false;
            SearchQuery = string.Empty;
            LoadCategories();
        }

        public void OnAppearing()
        {
            if (ShowProducts)
                LoadProducts();
            else
                LoadCategories();
        }

        private void LoadCategories()
        {
            if (_order == null)
            {
                Categories = new ObservableCollection<CategoryViewModel>();
                ShowNoCategories = ShowCategories && Categories.Count == 0;
                return;
            }

            var allCategories = Category.Categories.ToList();
            var categoryList = new System.Collections.Generic.List<CategoryItem>();

            foreach (var category in allCategories.Where(x => x.ParentCategoryId == 0))
            {
                var item = new CategoryItem { Category = category };
                foreach (var subcat in allCategories.Where(x => x.ParentCategoryId == category.CategoryId))
                {
                    item.Subcategories.Add(subcat);
                }
                categoryList.Add(item);
            }

            if (Config.SalesmanCanChangeSite)
            {
                if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                {
                    var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                    var allow_categories = Product.Products.Where(x => allowedProducts.Contains(x.ProductId)).Select(x => x.CategoryId).Distinct().ToList();
                    categoryList = categoryList.Where(x => allow_categories.Contains(x.Category.CategoryId)).ToList();
                }
            }

            var productsForOrder = Product.GetProductListForOrder(_order, false, 0, false);
            var categoryIdsWithProducts = productsForOrder.Select(p => p.CategoryId).Where(id => id > 0).Distinct().ToHashSet();
            categoryList = categoryList.Where(item =>
                categoryIdsWithProducts.Contains(item.Category.CategoryId) ||
                item.Subcategories.Any(sub => categoryIdsWithProducts.Contains(sub.CategoryId))).ToList();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchTerm = SearchQuery.Trim().ToLowerInvariant();
                categoryList = categoryList.Where(x => x.Category.Name.ToLowerInvariant().Contains(searchTerm)).ToList();
            }

            var newCategories = new ObservableCollection<CategoryViewModel>();
            foreach (var item in categoryList.OrderBy(x => x.Category.Name))
            {
                var visibleSubcategories = item.Subcategories.Where(s => categoryIdsWithProducts.Contains(s.CategoryId)).ToList();
                var subcategoriesText = visibleSubcategories.Count > 0
                    ? $"Subcategories: {string.Join(", ", visibleSubcategories.Select(s => s.Name))}"
                    : string.Empty;

                var viewModel = new CategoryViewModel
                {
                    Category = item.Category,
                    Name = item.Category.Name,
                    SubcategoriesText = subcategoriesText,
                    ShowSubcategories = !string.IsNullOrEmpty(subcategoriesText),
                    IsExpanded = item.Category.Expanded
                };
                foreach (var subcat in visibleSubcategories)
                {
                    viewModel.Subcategories.Add(subcat);
                }
                newCategories.Add(viewModel);
            }

            Categories = newCategories;
            ShowNoCategories = ShowCategories && Categories.Count == 0;
        }

        private void LoadProducts()
        {
            if (_order == null)
            {
                Products.Clear();
                ShowNoCategories = ShowProducts && Products.Count == 0;
                return;
            }

            var products = Product.GetProductListForOrder(_order, false, _categoryId ?? 0).ToList();

            if (Config.SalesmanCanChangeSite)
            {
                if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                {
                    var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                    products = products.Where(x => allowedProducts.Contains(x.ProductId)).ToList();
                }
            }

            if (_scannedProductId.HasValue)
            {
                products = products.Where(x => x.ProductId == _scannedProductId.Value).ToList();
            }
            else
            {
                var searchTerm = !string.IsNullOrWhiteSpace(SearchQuery) ? SearchQuery.Trim() : (_productSearch?.Trim() ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLowerInvariant();
                    products = products.Where(x => x.CategoryId > 0 && x.Name.ToLowerInvariant().Contains(searchLower)).ToList();
                }
                else
                {
                    products = products.Where(x => x.CategoryId > 0).ToList();
                }
            }

            Products.Clear();
            foreach (var product in products)
            {
                bool cameFromOffer = false;
                decimal price = (decimal)Product.GetPriceForProduct(product, _order, out cameFromOffer, false, false, null);
                var rawInventory = Config.TrackInventory ? product.CurrentInventory : product.CurrentWarehouseInventory;
                var imagePath = ProductImage.GetProductImage(product.ProductId);
                var hasImage = !string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath);
                double package = 1;
                if (!string.IsNullOrEmpty(product.Package))
                    double.TryParse(product.Package, out package);

                var familyUoms = product.UnitOfMeasures;
                var hasUoMFamily = !string.IsNullOrEmpty(product.UoMFamily);
                var showUom = (familyUoms != null && familyUoms.Count > 0) || hasUoMFamily;
                var showUom1 = false;
                var showPrice1 = false;
                var showOnHand1 = false;
                string uom = string.Empty;
                string uom1 = string.Empty;
                string price1 = string.Empty;
                string onHand1 = string.Empty;
                decimal basePrice = price;
                double displayInventory = rawInventory;
                var uomRows = new ObservableCollection<UomRowViewModel>();
                double weightFactor = Config.IncludeAvgWeightInCatalogPrice && product.Weight > 0 ? product.Weight : 1;

                if (showUom && familyUoms != null && familyUoms.Count > 0)
                {
                    var firstUom = familyUoms.FirstOrDefault(x => x.IsBase) ?? familyUoms.FirstOrDefault();
                    if (firstUom != null)
                    {
                        uom = firstUom.Name;
                        basePrice = price * (decimal)(firstUom.Conversion * weightFactor);
                        displayInventory = rawInventory / firstUom.Conversion;
                        if (familyUoms.Count > 1)
                        {
                            var secondUom = familyUoms.Where(x => x != firstUom).FirstOrDefault();
                            if (secondUom != null)
                            {
                                showUom1 = true;
                                showPrice1 = true;
                                showOnHand1 = true;
                                uom1 = secondUom.Name;
                                price1 = (price * (decimal)(secondUom.Conversion * weightFactor)).ToString("C");
                                onHand1 = Math.Round(rawInventory / secondUom.Conversion, 2).ToString();
                            }
                        }
                    }
                }

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
                    ShowWeight = product.Weight > 0,
                    Uom = uom,
                    Uom1 = uom1,
                    Price1 = price1,
                    OnHand1 = onHand1,
                    ShowUom = showUom,
                    ShowUom1 = showUom1,
                    ShowPrice1 = showPrice1,
                    ShowOnHand1 = showOnHand1,
                    ShowPrice = !Config.HidePriceInTransaction,
                    UomRows = uomRows
                });
            }

            ShowNoCategories = ShowProducts && Products.Count == 0;
        }

        partial void OnSearchQueryChanged(string value)
        {
            if (SearchByProduct && !string.IsNullOrEmpty(value))
            {
                _scannedProductId = null;
                LoadProducts();
                return;
            }
            if (SearchByProduct)
                return;
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
                _scannedProductId = null;
            }
        }

        public async Task HandleProductSearchSubmitAsync()
        {
            var templateCriteria = SearchQuery.Trim().ToLower();
            if (!SearchByProduct || string.IsNullOrWhiteSpace(templateCriteria))
                return;
            _productSearch = templateCriteria;
            _comingFromSearch = true;
            ShowCategories = false;
            ShowProducts = true;
            Title = "Products";
            LoadProducts();
            await Task.CompletedTask;
        }

        public async Task ToggleCategoryExpanded(CategoryViewModel viewModel)
        {
            if (viewModel.Subcategories.Count == 0)
            {
                await CategorySelectedAsync(viewModel);
                return;
            }
            viewModel.IsExpanded = !viewModel.IsExpanded;
            viewModel.Category.Expanded = viewModel.IsExpanded;
        }

        [RelayCommand]
        public async Task CategorySelectedAsync(CategoryViewModel item)
        {
            if (item == null || item.Category == null || _order == null)
                return;
            await Shell.Current.GoToAsync($"selfservice/catalog?orderId={_order.OrderId}&categoryId={item.Category.CategoryId}");
        }

        [RelayCommand]
        public async Task ProductImageClickedAsync(ProductViewModel item)
        {
            if (item?.Product == null)
                return;
            await Shell.Current.GoToAsync($"productimage?productId={item.Product.ProductId}");
        }

        [RelayCommand]
        public async Task ViewProductDetailsAsync(ProductViewModel item)
        {
            if (item?.Product == null)
                return;
            var route = $"productdetails?productId={item.Product.ProductId}";
            if (_order?.Client != null)
                route += $"&clientId={_order.Client.ClientId}";
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        public async Task ProductSelectedAsync(ProductViewModel item)
        {
            if (item == null || item.Product == null || _order == null)
                return;
            var catId = _categoryId ?? item.Product.CategoryId;
            var route = $"selfservice/catalog?orderId={_order.OrderId}&categoryId={catId}&productId={item.Product.ProductId}";
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        private async Task ScanAsync()
        {
            try
            {
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult))
                    return;

                var product = Product.Products.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.Upc) && p.Upc.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Sku) && p.Sku.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Equals(scanResult, StringComparison.OrdinalIgnoreCase)));

                if (product != null && _order != null)
                {
                    _scannedProductId = product.ProductId;
                    SearchByProduct = true;
                    SearchByCategory = false;
                    ShowProducts = true;
                    ShowCategories = false;
                    SearchQuery = string.Empty;

                    var route = $"selfservice/catalog?orderId={_order.OrderId}&categoryId={product.CategoryId}&productId={product.ProductId}";
                    await Shell.Current.GoToAsync(route);
                }
                else
                {
                    _scannedProductId = null;
                    ShowProducts = true;
                    ShowCategories = false;
                    SearchByProduct = true;
                    SearchByCategory = false;

                    await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error scanning barcode: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            }
        }

        /// <summary>Toolbar: pop (go back) from categories screen.</summary>
        [RelayCommand]
        private async Task PopAsync()
        {
            await Helpers.NavigationHelper.GoBackFromAsync("selfservice/categories");
        }

        /// <summary>Toolbar: show help/menu (Sync Data, Advanced Options, Sign Out).</summary>
        [RelayCommand]
        private async Task ShowToolbarMenuAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        private class CategoryItem
        {
            public Category Category { get; set; } = null!;
            public System.Collections.Generic.List<Category> Subcategories { get; } = new();
        }
    }
}
