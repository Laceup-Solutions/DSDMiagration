using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
    public partial class ProductCatalogPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly IScannerService _scannerService;
        private Order? _order;
        private bool _initialized;
        private int? _categoryId;
        private string? _productSearch;
        private bool _comingFromSearch;
        private bool _asCreditItem;
        private bool _asReturnItem;
        private int? _productId;
        private bool _consignmentCounting;
        private string _searchCriteria = string.Empty;
        private bool _isCreating = false;
        private bool _atLeastOneImage = false;
        private ViewTypes _currentViewType = ViewTypes.Normal;

        public ObservableCollection<CatalogItemViewModel> Products { get; } = new();
        public ObservableCollection<CatalogItemViewModel> FilteredProducts { get; } = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _showImages = true;

        [ObservableProperty]
        private string _viewTypeIcon = "catalog_two";

        public ProductCatalogPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
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
                _productSearch = searchValue.ToString();
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
                await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            _initialized = true;
            PrepareProductList();
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
            PrepareProductList();
            Filter();
            await Task.CompletedTask;
        }

        private void PrepareProductList()
        {
            if (_order == null)
                return;

            var productsForOrder = Product.GetProductListForOrder(_order, _asCreditItem, _categoryId ?? 0).ToList();

            if (!string.IsNullOrEmpty(_productSearch) && _comingFromSearch)
            {
                var searchLower = _productSearch.ToLowerInvariant();
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
                var catalogItem = new CatalogItemViewModel
                {
                    Product = product
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
                catalogItem.Line.ExpectedPrice = Product.GetPriceForProduct(product, _order, _asCreditItem, false);
                catalogItem.Line.IsCredit = _asCreditItem;

                // Handle UoM
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
                    if (catalogItem.Line.UoM != null)
                        catalogItem.Line.ExpectedPrice *= catalogItem.Line.UoM.Conversion;
                }

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

                catalogItems.Add(catalogItem);
            }

            // Sort products - convert to CatalogItem for sorting
            var catalogItemsForSort = catalogItems.Select(x => new CatalogItem
            {
                Product = x.Product,
                Line = x.Line,
                Values = x.Values.ToList()
            }).ToList();
            var sorted = SortDetails.SortedDetailsInProduct(catalogItemsForSort).ToList();
            
            // Rebuild ViewModels from sorted items maintaining order
            var sortedViewModels = new List<CatalogItemViewModel>();
            foreach (var sortedItem in sorted)
            {
                var vm = catalogItems.FirstOrDefault(x => x.Product.ProductId == sortedItem.Product.ProductId);
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
            var productIds = Products.Select(x => x.Product.ProductId).ToList();
            _atLeastOneImage = ProductImage.AtLeastOneProductHasImg(productIds);
            ShowImages = _atLeastOneImage;
        }

        private void Filter()
        {
            var list = Products.ToList();

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                var searchUpper = _searchCriteria.ToUpperInvariant();
                list = list.Where(x =>
                    x.Product.Name.ToUpperInvariant().Contains(searchUpper) ||
                    x.Product.Upc.ToUpperInvariant().Contains(searchUpper) ||
                    x.Product.Sku.ToUpperInvariant().Contains(searchUpper) ||
                    x.Product.Code.ToLowerInvariant().Contains(searchUpper)
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
            _searchCriteria = value;
            Filter();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            _searchCriteria = searchTerm.ToUpperInvariant();
            SearchQuery = searchTerm;
            Filter();
        }

        [RelayCommand]
        public async Task ScanAsync()
        {
            // try
            // {
            //     var scanResult = await _scannerService.ScanAsync();
            //     if (string.IsNullOrEmpty(scanResult))
            //         return;
            //
            //     await ScannerDoTheThingAsync(scanResult);
            // }
            // catch (Exception ex)
            // {
            //     Logger.CreateLog($"Error scanning: {ex.Message}");
            //     await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            // }
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

            var catalogItem = FilteredProducts.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
            if (catalogItem != null)
            {
                await AddButton_ClickAsync(catalogItem);
            }
        }

        [RelayCommand]
        private async Task AddButtonClickAsync(CatalogItemViewModel? item)
        {
            if (item == null)
                return;

            await AddButton_ClickAsync(item);
        }

        private async Task AddButton_ClickAsync(CatalogItemViewModel item)
        {
            if (_order == null || item.Product == null)
                return;

            // Navigate to AddItemPage for this product
            var route = $"additem?orderId={_order.OrderId}&productId={item.Product.ProductId}";
            if (_asCreditItem)
                route += "&asCreditItem=1";
            if (_consignmentCounting)
                route += "&consignmentCounting=1";
            
            await Shell.Current.GoToAsync(route);
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

            // Navigate back to order template
            var targetRoute = Config.UseFullTemplateForClient(_order.Client) && !_order.Client.AllowOneDoc && _order.OrderType != OrderType.Load
                ? $"superordertemplate?asPresale=0&orderId={_order.OrderId}"
                : $"orderdetails?orderId={_order.OrderId}&asPresale=0";

            if (oneDetail != null)
                targetRoute += $"&productId={oneDetail.Product.ProductId}";

            await Shell.Current.GoToAsync(targetRoute);
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
            DataAccess.SaveInventory();

            // Navigate to load order template
            var route = $"loadordertemplate?orderId={_order.OrderId}";
            if (oneDetail != null)
                route += $"&lastDetail={oneDetail.OrderDetailId}";
            else
                route += "&lastDetail=0";

            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private enum ViewTypes
        {
            Normal = 0,
            Big = 1
        }
    }

    public partial class CatalogItemViewModel : ObservableObject
    {
        private Product? _product;
        public Product? Product 
        { 
            get => _product;
            set
            {
                if (SetProperty(ref _product, value))
                {
                    if (value != null)
                    {
                        ProductName = value.Name;
                        UpdateDisplay();
                    }
                }
            }
        }
        
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
        
        public ObservableCollection<OdLine> Values { get; } = new();

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
        private bool _hasImage = false;

        private void UpdateDisplay()
        {
            if (Product == null || Line == null)
                return;

            // Update on hand
            // This would need order context - simplified for now
            OnHandText = $"OH: {Product.CurrentWarehouseInventory:F2}";

            // Update UoM
            UomText = Line.UoM != null ? Line.UoM.Name : string.Empty;

            // Update list price
            // This would need order context - simplified for now
            ListPriceText = $"List: {Line.ExpectedPrice:C}";

            // Update total
            var total = Values.Sum(v => v.Qty * v.Price);
            TotalText = total.ToString("C");

            // Update avg sale
            if (Config.ShowAvgInCatalog)
            {
                AvgSaleText = $"Avg: {Line.AvgSale:F2}";
            }
        }
    }
}
