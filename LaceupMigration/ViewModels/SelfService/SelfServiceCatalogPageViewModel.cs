using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration.Business.Interfaces;
using LaceupMigration;
using LaceupMigration.Helpers;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCatalogPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
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

        /// <summary>When true (UseLaceupAdvancedCatalogKey=1), cells use Advanced Catalog style (image, OH green, price blue, [-] qty [+]). When false, cells match ProductCatalog style.</summary>
        [ObservableProperty]
        private bool _useAdvancedCatalogStyle;

        private int _currentFilter = 0; // 0 = All, 2 = In Stock, 4 = Not In Order, etc.
        private int _currentSort = 0; // 0 = Name, 1 = Category, 2 = In Stock
        private bool _listInitialized;
        private const int PageSize = 50;
        private List<Product> _allFilteredProducts = new();
        private int _loadedCount;

        public SelfServiceCatalogPageViewModel(IDialogService dialogService, ILaceupAppService appService, ICameraBarcodeScannerService cameraBarcodeScanner)
        {
            _dialogService = dialogService;
            _appService = appService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
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
            _listInitialized = true;
        }

        public void OnAppearing()
        {
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
            if (!_listInitialized)
            {
                LoadProducts();
                _listInitialized = true;
            }
            else
                RefreshProductQuantitiesFromOrder();
        }

        /// <summary>Update qty/price for all visible items from current order (no full reload).</summary>
        private void RefreshProductQuantitiesFromOrder()
        {
            if (_order == null) return;
            foreach (var item in Products)
            {
                var detail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == item.Product?.ProductId && !x.IsCredit);
                item.UpdateFromOrder(_order, detail);
            }
        }

        /// <summary>Update a single row after add/edit/increment/decrement.</summary>
        private void UpdateAffectedProductRow(int productId)
        {
            var detail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == productId && !x.IsCredit);
            var qty = detail != null ? (double)detail.Qty : 0;
            var item = Products.FirstOrDefault(x => x.Product?.ProductId == productId);
            if (item != null)
                item.UpdateFromOrder(_order, detail);
        }

        private void LoadProducts()
        {
            if (_order == null)
                return;

            Products.Clear();
            _allFilteredProducts.Clear();
            _loadedCount = 0;

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

            if (_order.Client != null)
                _order.Client.EnsurePreviouslyOrdered();

            _allFilteredProducts = productList;
            var toLoad = _allFilteredProducts.Take(PageSize).ToList();
            _loadedCount = toLoad.Count;
            foreach (var product in toLoad)
            {
                var detail = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId && !x.IsCredit);
                var qtyInOrder = detail != null ? (double)detail.Qty : 0;
                Products.Add(ProductItemViewModel.Create(product, _order, detail, qtyInOrder));
            }
        }

        /// <summary>Load next page of products (pagination).</summary>
        [RelayCommand]
        private void LoadMore()
        {
            if (_order == null || _loadedCount >= _allFilteredProducts.Count) return;
            var next = _allFilteredProducts.Skip(_loadedCount).Take(PageSize).ToList();
            _loadedCount += next.Count;
            foreach (var product in next)
            {
                var detail = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId && !x.IsCredit);
                var qtyInOrder = detail != null ? (double)detail.Qty : 0;
                Products.Add(ProductItemViewModel.Create(product, _order, detail, qtyInOrder));
            }
        }

        /// <summary>True when there are more items to load (for RemainingItemsThreshold).</summary>
        public bool HasMoreToLoad => _allFilteredProducts != null && _loadedCount < _allFilteredProducts.Count;

        [RelayCommand]
        private async Task Scan()
        {
            if (_order == null)
                return;

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
                    // Set search text to filter products
                    SearchText = scanResult;
                    
                    // Find the product in the list and add it
                    var productItem = Products.FirstOrDefault(p => p.Product.ProductId == product.ProductId);
                    if (productItem != null)
                    {
                        // Product is in the list, could navigate to it or add it
                        await _dialogService.ShowAlertAsync($"Found product: {product.Name}", "Barcode Scan");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Product not found in current category.", "Info");
                    }
                }
                else
                {
                    // Product not found, but set search text anyway
                    SearchText = scanResult;
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

        /// <summary>Tap row or + to add/edit quantity (RestOfTheAddDialog). Stays on catalog and refreshes list.</summary>
        [RelayCommand]
        private async Task SelectProduct(ProductItemViewModel productItem)
        {
            if (productItem == null || _order == null)
                return;
            await AddOrEditItemAsync(productItem);
        }

        /// <summary>Open add/edit quantity dialog (same as SelfServiceCheckOutPage).</summary>
        [RelayCommand]
        private async Task AddOrEditItem(ProductItemViewModel productItem)
        {
            if (productItem == null || _order == null)
                return;
            await AddOrEditItemAsync(productItem);
        }

        private async Task AddOrEditItemAsync(ProductItemViewModel productItem)
        {
            if (productItem?.Product == null || _order == null)
                return;

            if (productItem.Quantity == 0 && !_order.AsPresale && !Config.CanGoBelow0)
            {
                var oh = productItem.Product.GetInventory(_order.AsPresale, false);
                if (oh <= 0)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == productItem.Product.ProductId && !x.IsCredit);
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                productItem.Product,
                _order,
                existingDetail,
                isCredit: false,
                isDamaged: false,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled)
                return;

            if (result.Qty == 0)
            {
                if (existingDetail != null)
                {
                    _order.DeleteDetail(existingDetail);
                    _order.Save();
                }
                UpdateAffectedProductRow(productItem.Product.ProductId);
                return;
            }

            if (!_order.AsPresale && !Config.CanGoBelow0)
            {
                var currentOH = productItem.Product.GetInventory(_order.AsPresale, false);
                var resultBaseQty = (double)result.Qty;
                if (result.SelectedUoM != null)
                    resultBaseQty *= result.SelectedUoM.Conversion;
                var totalBaseQtyInOrder = _order.Details
                    .Where(d => d.Product.ProductId == productItem.Product.ProductId && !d.IsCredit && d != existingDetail)
                    .Sum(d => (double)d.Qty * (d.UnitOfMeasure?.Conversion ?? 1));
                if (totalBaseQtyInOrder + resultBaseQty > currentOH)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            if (existingDetail != null)
            {
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
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                OrderDetail.UpdateRelated(existingDetail, _order);
            }
            else
            {
                var detail = new OrderDetail(productItem.Product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(productItem.Product, _order, false, false);
                double price = result.Price;
                if (result.UseLastSoldPrice && _order.Client != null)
                {
                    var clientHistory = InvoiceDetail.ClientProduct(_order.Client.ClientId, productItem.Product.ProductId);
                    var lastInvoiceDetail = clientHistory?.OrderByDescending(x => x.Date).FirstOrDefault();
                    if (lastInvoiceDetail != null)
                        price = lastInvoiceDetail.Price;
                }
                else if (price == 0)
                {
                    if (Offer.ProductHasSpecialPriceForClient(productItem.Product, _order.Client, out var offerPrice))
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
                detail.UnitOfMeasure = result.SelectedUoM ?? productItem.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = result.Qty;
                detail.Weight = result.Weight;
                detail.Lot = result.Lot;
                if (result.LotExpiration.HasValue)
                    detail.LotExpiration = result.LotExpiration.Value;
                detail.Comments = result.Comments;
                detail.IsFreeItem = result.IsFreeItem;
                detail.ReasonId = result.ReasonId;
                if (result.PriceLevelSelected > 0)
                    detail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), detail.ExtraFields);
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, detail);
                OrderDetail.UpdateRelated(detail, _order);
            }
            _order.RecalculateDiscounts();
            _order.Save();
            UpdateAffectedProductRow(productItem.Product.ProductId);
        }

        /// <summary>+ button: add 1 to existing line or add new line with qty 1.</summary>
        [RelayCommand]
        private void IncrementQuantity(ProductItemViewModel productItem)
        {
            if (productItem?.Product == null || _order == null)
                return;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == productItem.Product.ProductId && !x.IsCredit);
            if (existingDetail != null)
            {
                existingDetail.Qty += 1f;
                OrderDetail.UpdateRelated(existingDetail, _order);
            }
            else
            {
                var detail = new OrderDetail(productItem.Product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(productItem.Product, _order, false, false);
                if (Offer.ProductHasSpecialPriceForClient(productItem.Product, _order.Client, out var price))
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
                detail.UnitOfMeasure = productItem.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = 1f;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
                OrderDetail.UpdateRelated(detail, _order);
            }
            _order.RecalculateDiscounts();
            _order.Save();
            UpdateAffectedProductRow(productItem.Product.ProductId);
        }

        /// <summary>- button: subtract 1 or remove line if qty becomes 0.</summary>
        [RelayCommand]
        private void DecrementQuantity(ProductItemViewModel productItem)
        {
            if (productItem?.Product == null || _order == null)
                return;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == productItem.Product.ProductId && !x.IsCredit);
            if (existingDetail == null)
                return;
            if (existingDetail.Qty <= 1f)
            {
                _order.Details.Remove(existingDetail);
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
            UpdateAffectedProductRow(productItem.Product.ProductId);
        }

        partial void OnSearchTextChanged(string value)
        {
            LoadProducts();
        }

        /// <summary>Navigate directly to SelfServiceCheckOutPage (toolbar Checkout button). Pass fromCatalog=1 so checkout does a full list refresh.</summary>
        [RelayCommand]
        private async Task GoToCheckout()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"selfservice/checkout?orderId={_order.OrderId}&fromCatalog=1");
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

        /// <summary>Numeric qty in order (for binding and logic).</summary>
        public double Quantity { get; set; }

        /// <summary>True when product is in order (enables minus button).</summary>
        public bool IsInOrder => Quantity > 0;

        /// <summary>Update qty/price in place from order (avoids full list reload).</summary>
        public void UpdateFromOrder(Order order, OrderDetail detail)
        {
            if (Product == null) return;
            Quantity = detail != null ? (double)detail.Qty : 0;
            QtyInOrder = Quantity.ToString("F0");
            if (detail != null)
            {
                PriceText = $"Price: {detail.Price.ToCustomString()}";
                TotalText = $"Total: {(detail.Qty * detail.Price).ToCustomString()}";
                QuantityButtonText = detail.Qty.ToString("F0");
            }
            else
            {
                var listPrice = Product.GetPriceForProduct(Product, order, false, false);
                PriceText = $"Price: {listPrice.ToCustomString()}";
                TotalText = string.Empty;
                QuantityButtonText = "+";
            }
            OnPropertyChanged(nameof(HasValues));
            OnPropertyChanged(nameof(IsInOrder));
        }

        /// <summary>For ProductCatalog-style and Advanced: on-hand text (e.g. "OH: 99").</summary>
        [ObservableProperty]
        private string _onHandText = "OH: 0";

        /// <summary>For ProductCatalog-style and Advanced: list price text.</summary>
        [ObservableProperty]
        private string _listPriceText = string.Empty;

        /// <summary>For Advanced style: "Previously Ordered:" / "Last Visit: ..." or "No previous orders".</summary>
        [ObservableProperty]
        private string _historyText = string.Empty;

        /// <summary>For ProductCatalog-style: total line text when in order.</summary>
        [ObservableProperty]
        private string _totalText = string.Empty;

        /// <summary>True when in order (show total in ProductCatalog style).</summary>
        public bool HasValues => Quantity > 0;

        /// <summary>For ProductCatalog-style: suggested label.</summary>
        [ObservableProperty]
        private string _suggestedLabelText = string.Empty;

        /// <summary>For ProductCatalog-style: "+" or quantity number.</summary>
        [ObservableProperty]
        private string _quantityButtonText = "+";

        /// <summary>Product image path (or placeholder) for Advanced/ProductCatalog style.</summary>
        [ObservableProperty]
        private string _productImagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage;

        /// <summary>Create a fully populated item for both Advanced and ProductCatalog templates.</summary>
        public static ProductItemViewModel Create(Product product, Order order, OrderDetail detail, double qtyInOrder)
        {
            var onHand = order != null ? product.GetInventory(order.AsPresale, false) : 0;
            var listPrice = Product.GetPriceForProduct(product, order, false, false);
            var historyText = "No previous orders";
            if (order?.Client?.OrderedList != null)
            {
                var orderedItem = order.Client.OrderedList.FirstOrDefault(x => x.Last?.ProductId == product.ProductId);
                if (orderedItem?.Last != null && orderedItem.Last.Date != DateTime.MinValue)
                    historyText = $"Last Visit: {orderedItem.Last.Date:MM/dd}, {orderedItem.Last.Quantity}, {orderedItem.Last.Price.ToCustomString()}";
            }
            var priceText = detail != null
                ? $"Price: {detail.Price.ToCustomString()}"
                : $"Price: {listPrice.ToCustomString()}";
            var totalText = detail != null ? $"Total: {(detail.Qty * detail.Price).ToCustomString()}" : string.Empty;
            var isSuggested = order != null && order.Client != null && Product.IsSuggestedForClient(order.Client, product);
            var suggestedLabelText = isSuggested
                ? (string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) ? "Suggested Products" : $"{Config.ProductCategoryNameIdentifier} Products")
                : string.Empty;
            var imagePath = ProductImage.GetProductImageWithPlaceholder(product.ProductId);
            var hasImage = !string.IsNullOrEmpty(ProductImage.GetProductImage(product.ProductId));

            return new ProductItemViewModel(product, qtyInOrder)
            {
                OnHandText = $"OH: {onHand:F0}",
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                HistoryText = historyText,
                PriceText = priceText,
                TotalText = totalText,
                SuggestedLabelText = suggestedLabelText,
                QuantityButtonText = qtyInOrder > 0 ? qtyInOrder.ToString("F0") : "+",
                ProductImagePath = imagePath ?? string.Empty,
                HasImage = hasImage
            };
        }

        private ProductItemViewModel(Product product, double qtyInOrder)
        {
            Product = product;
            Quantity = qtyInOrder;
            ProductName = product.Name;
            CategoryName = Category.Categories.FirstOrDefault(x => x.CategoryId == product.CategoryId)?.Name ?? "Uncategorized";
            PriceText = $"Price: {product.PriceLevel0.ToCustomString()}";
            StatusText = product.OnHand > 0 ? "In Stock" : "Out of Stock";
            StatusColor = product.OnHand > 0 ? Colors.Green : Colors.Red;
            QtyInOrder = qtyInOrder.ToString("F0");
        }
    }
}

