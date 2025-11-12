using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCatalogPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;
        private Category _category;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _categoryName = "All Categories";

        [ObservableProperty]
        private ObservableCollection<ProductItemViewModel> _products = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _filterText = "Filter: All";

        [ObservableProperty]
        private string _sortByText = "Sort: Name";

        private int _currentFilter = 0; // 0 = All, 2 = In Stock, 4 = Not In Order, etc.
        private int _currentSort = 0; // 0 = Name, 1 = Category, 2 = In Stock

        public SelfServiceCatalogPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
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

            if (query.TryGetValue("categoryId", out var categoryIdObj) && int.TryParse(categoryIdObj?.ToString(), out var categoryId))
            {
                _category = Category.Categories.FirstOrDefault(x => x.CategoryId == categoryId);
                if (_category != null)
                {
                    CategoryName = _category.Name;
                }
            }

            LoadProducts();
        }

        public void OnAppearing()
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            if (_order == null)
                return;

            Products.Clear();

            var productList = Product.Products.ToList();

            // Filter by category if selected
            if (_category != null)
            {
                productList = productList.Where(x => x.CategoryId == _category.CategoryId).ToList();
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                productList = productList.Where(x => x.Name.ToLowerInvariant().Contains(searchLower)).ToList();
            }

            // Apply additional filters
            if ((_currentFilter & 2) != 0) // In Stock
            {
                productList = productList.Where(x => x.OnHand > 0).ToList();
            }
            if ((_currentFilter & 4) != 0) // Not In Order
            {
                var orderProductIds = _order.Details.Select(x => x.Product.ProductId).ToList();
                productList = productList.Where(x => !orderProductIds.Contains(x.ProductId)).ToList();
            }
            if ((_currentFilter & 8) != 0) // In Order
            {
                var orderProductIds = _order.Details.Select(x => x.Product.ProductId).ToList();
                productList = productList.Where(x => orderProductIds.Contains(x.ProductId)).ToList();
            }

            // Sort
            switch (_currentSort)
            {
                case 1: // Category
                    productList = productList.OrderBy(x => x.CategoryId).ThenBy(x => x.Name).ToList();
                    break;
                case 2: // In Stock
                    productList = productList.OrderByDescending(x => x.OnHand).ThenBy(x => x.Name).ToList();
                    break;
                default: // Name
                    productList = productList.OrderBy(x => x.Name).ToList();
                    break;
            }

            foreach (var product in productList)
            {
                var qtyInOrder = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId)?.Qty ?? 0;
                Products.Add(new ProductItemViewModel(product, qtyInOrder));
            }
        }

        [RelayCommand]
        private async Task Scan()
        {
            // TODO: Implement barcode scanning
            await _dialogService.ShowAlertAsync("Barcode scanning functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Filter()
        {
            var options = new[] { "All", "In Stock", "Not In Order", "In Order", "Previously Ordered", "Never Ordered", "In Offer" };
            var selected = await _dialogService.ShowActionSheetAsync("Filter Products", "", "Cancel", options);

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            _currentFilter = selected switch
            {
                "In Stock" => 2,
                "Not In Order" => 4,
                "In Order" => 8,
                "Previously Ordered" => 16,
                "Never Ordered" => 32,
                "In Offer" => 64,
                _ => 0
            };

            FilterText = $"Filter: {selected}";
            LoadProducts();
        }

        [RelayCommand]
        private async Task SortBy()
        {
            var options = new[] { "Name", "Category", "In Stock" };
            var selected = await _dialogService.ShowActionSheetAsync("Sort By", "", "Cancel", options);

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            _currentSort = selected switch
            {
                "Category" => 1,
                "In Stock" => 2,
                _ => 0
            };

            SortByText = $"Sort: {selected}";
            LoadProducts();
        }

        [RelayCommand]
        private async Task SelectProduct(ProductItemViewModel productItem)
        {
            if (productItem == null || _order == null)
                return;

            // Navigate to order details to add product
            await Shell.Current.GoToAsync($"orderdetails?orderId={_order.OrderId}&productId={productItem.Product.ProductId}");
        }

        partial void OnSearchTextChanged(string value)
        {
            LoadProducts();
        }
    }

    public partial class ProductItemViewModel : ObservableObject
    {
        public Product Product { get; }

        [ObservableProperty]
        private string _productName;

        [ObservableProperty]
        private string _categoryName;

        [ObservableProperty]
        private string _priceText;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private Color _statusColor;

        [ObservableProperty]
        private string _qtyInOrder;

        public ProductItemViewModel(Product product, double qtyInOrder)
        {
            Product = product;
            ProductName = product.Name;
            CategoryName = Category.Categories.FirstOrDefault(x => x.CategoryId == product.CategoryId)?.Name ?? "Uncategorized";
            PriceText = $"Price: {product.PriceLevel0.ToCustomString()}";
            StatusText = product.OnHand > 0 ? "In Stock" : "Out of Stock";
            StatusColor = product.OnHand > 0 ? Colors.Green : Colors.Red;
            QtyInOrder = qtyInOrder > 0 ? qtyInOrder.ToString("F0") : "";
        }
    }
}

