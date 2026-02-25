using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using LaceupMigration.Services;
using LaceupMigration;
using LaceupMigration.Helpers;
using LaceupMigration.ViewModels;
using LaceupMigration.Business.Interfaces;
using LaceupMigration.Controls;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCheckOutPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly MainPageViewModel _mainPageViewModel;
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;
        private Order _order;
        private List<SelfServiceOrderProductViewModel> _mergedList = new();
        private bool _needsFullRefresh = true;
        private bool _noNeedToRefresh = false;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _qtySoldText = "Qty Sold: 0";

        [ObservableProperty]
        private string _qtyText = "Qty: 0";

        [ObservableProperty]
        private string _termText = "Term: ";

        [ObservableProperty]
        private string _termsText = "Terms: ";

        [ObservableProperty]
        private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty]
        private string _discountText = "Discount: $0.00";

        [ObservableProperty]
        private string _taxText = "Tax: $0.00";

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private bool _canSendOrder;

        /// <summary>True while the order/recently-ordered list is loading (page shows first, then list).</summary>
        [ObservableProperty]
        private bool _isLoadingList;

        /// <summary>Collapsible order summary (like AdvancedCatalogPage). Collapsed by default.</summary>
        [ObservableProperty]
        private bool _isOrderSummaryExpanded = false;

        /// <summary>Show total in header when summary is collapsed.</summary>
        public bool ShowTotalInHeader => !IsOrderSummaryExpanded;

        /// <summary>Show total in header when summary is collapsed and prices are visible (HidePriceInSelfServiceKey=0).</summary>
        public bool ShowTotalInHeaderWithPrice => ShowTotalInHeader && ShowPrices;

        [RelayCommand]
        private void ToggleOrderSummary()
        {
            IsOrderSummaryExpanded = !IsOrderSummaryExpanded;
            OnPropertyChanged(nameof(ShowTotalInHeader));
        }

        /// <summary>Merged list (order details + previously ordered). Same design as PreviouslyOrderedTemplatePage.</summary>
        [ObservableProperty]
        private ObservableCollection<SelfServiceOrderProductViewModel> _displayProducts = new();

        /// <summary>When true, show full merged list (recently ordered + in order); when false, show only items in order.</summary>
        [ObservableProperty]
        private bool _showRecentlyOrdered = true;

        /// <summary>When true (UseLaceupAdvancedCatalogKey=1), line items use AdvancedCatalog-style layout. When false, use current layout.</summary>
        [ObservableProperty]
        private bool _useAdvancedCatalogStyle;

        /// <summary>When false (HidePriceInSelfServiceKey=1), all prices and totals are hidden in self service.</summary>
        [ObservableProperty]
        private bool _showPrices = true;

        /// <summary>When false (HideOHinSelfServiceKey=1), all on-hand/inventory labels are hidden in self service.</summary>
        [ObservableProperty]
        private bool _showOnHand = true;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _sortByText = "Sort By: Product Name";

        [ObservableProperty]
        private ObservableCollection<SelfServiceSearchProductViewModel> _searchResultItems = new();

        [RelayCommand]
        private void SortBy()
        {
            SortMergedList();
            RefilterDisplay();
        }

        public SelfServiceCheckOutPageViewModel(IDialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService, MainPageViewModel mainPageViewModel, ICameraBarcodeScannerService cameraBarcodeScanner)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            _mainPageViewModel = mainPageViewModel;
            _cameraBarcodeScanner = cameraBarcodeScanner;
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
            ShowPrices = !Config.HidePriceInSelfService;
            ShowOnHand = !Config.HideOHinSelfService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    ClientName = _order.Client.ClientName;
                    _needsFullRefresh = true;
                }
            }
        }

        public void OnAppearing()
        {
            UseAdvancedCatalogStyle = Config.UseLaceupAdvancedCatalog;
            ShowPrices = !Config.HidePriceInSelfService;
            ShowOnHand = !Config.HideOHinSelfService;
            if (_order == null) return;

            if (_noNeedToRefresh)
            {
                _noNeedToRefresh = false;
                return;
            }
            
            if (_needsFullRefresh)
            {
                _ = LoadOrderAsync();
                return;
            }
            RefreshTotals();
        }

        /// <summary>Load order list asynchronously so the page is visible first. Heavy work (LoadMergedList) runs on background thread; only RefilterDisplay + RefreshTotals run on UI thread.</summary>
        private async Task LoadOrderAsync()
        {
            if (_order == null) return;
            IsLoadingList = true;
            try
            {
                await Task.Run(() => _order.Client.EnsurePreviouslyOrdered()).ConfigureAwait(false);
                await Task.Run(() => LoadMergedList()).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    RefilterDisplay();
                    RefreshTotals();
                    IsLoadingList = false;
                    _needsFullRefresh = false;
                }).ConfigureAwait(false);
            }
            finally
            {
                if (IsLoadingList) await MainThread.InvokeOnMainThreadAsync(() => IsLoadingList = false).ConfigureAwait(false);
            }
        }

        private void LoadOrder()
        {
            if (_order == null) return;
            ClientName = _order.Client.ClientName;
            _order.Client.EnsurePreviouslyOrdered();
            LoadMergedList();
            RefilterDisplay();
            RefreshTotals();
        }

        partial void OnShowRecentlyOrderedChanged(bool value)
        {
            RefilterDisplay();
        }

        partial void OnIsOrderSummaryExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowTotalInHeader));
            OnPropertyChanged(nameof(ShowTotalInHeaderWithPrice));
        }

        partial void OnShowPricesChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowTotalInHeaderWithPrice));
        }

        /// <summary>Build merged list: order details first (with qty), then previously ordered not in order (qty 0). Same logic as PreviouslyOrderedTemplatePageViewModel.LoadOrderData.</summary>
        private void LoadMergedList()
        {
            _mergedList.Clear();
            if (_order == null) return;

            var productIdsInOrder = new HashSet<int>();
            foreach (var orderDetail in _order.Details)
            {
                if (orderDetail.Product == null) continue;
                productIdsInOrder.Add(orderDetail.Product.ProductId);
                var vm = CreateRowFromOrderDetail(orderDetail);
                _mergedList.Add(vm);
            }

            if (_order.Client.OrderedList != null)
            {
                foreach (var orderedItem in _order.Client.OrderedList.OrderByDescending(x => x.Last.Date).Take(100))
                {
                    if (orderedItem.Last?.Product == null) continue;
                    if (productIdsInOrder.Contains(orderedItem.Last.ProductId)) continue;
                    var vm = CreateRowFromPreviouslyOrdered(orderedItem);
                    _mergedList.Add(vm);
                }
            }

            SortMergedList();
        }

        private SelfServiceOrderProductViewModel CreateRowFromOrderDetail(OrderDetail orderDetail)
        {
            var product = orderDetail.Product;
            var onHand = _order != null ? product.GetInventory(_order.AsPresale, false) : 0;
            var listPrice = Product.GetPriceForProduct(product, _order, false, false);
            LastTwoDetails orderedItem = null;
            if (_order?.Client?.OrderedList != null)
                orderedItem = _order.Client.OrderedList.FirstOrDefault(x => x.Last?.ProductId == product.ProductId);
            var lastVisitText = string.Empty;
            var showLastVisit = false;
            if (orderedItem?.Last != null && orderedItem.Last.Date != DateTime.MinValue)
            {
                if (Config.HidePriceInSelfService)
                    lastVisitText = $"Last Visit: {orderedItem.Last.Date:MM/dd}, {orderedItem.Last.Quantity}";
                else
                    lastVisitText = $"Last Visit: {orderedItem.Last.Date:MM/dd}, {orderedItem.Last.Quantity}, {orderedItem.Last.Price.ToCustomString()}";
                showLastVisit = true;
            }
            var total = orderDetail.Qty * orderDetail.Price;
            var uomText = orderDetail.UnitOfMeasure != null ? orderDetail.UnitOfMeasure.Name : string.Empty;
            var imagePath = ProductImage.GetProductImageWithPlaceholder(product.ProductId);

            if(orderDetail.UnitOfMeasure != null)
                onHand /= orderDetail.UnitOfMeasure.Conversion;
            
            var ohStr = onHand > 0 ? "In Stock" : "Out of Stock";
            var color = onHand > 0 ? Color.FromArgb("#0a5713") : Color.FromArgb("#BA2D0B");
            
            if (Config.ShowOHQtyInSelfService)
            {
                ohStr = $"OH: {onHand:F0}";
                color = Color.FromArgb("#1f1f1f");
            }
            
            return new SelfServiceOrderProductViewModel(this)
            {
                Product = product,
                ProductName = product.Name,
                OnHandText = ohStr,
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PriceText = $"Price: {orderDetail.Price.ToCustomString()}",
                TotalText = $"Total: {total.ToCustomString()}",
                UomText = uomText,
                ExistingDetail = orderDetail,
                OrderedItem = orderedItem,
                Quantity = (double)orderDetail.Qty,
                OrderId = _order.OrderId,
                ProductImagePath = imagePath ?? string.Empty,
                HasImage = !string.IsNullOrEmpty(ProductImage.GetProductImage(product.ProductId)),
                QuantityText = orderDetail.Qty.ToString("F0"),
                HistoryText = showLastVisit ? lastVisitText : "No previous orders",
                OhColor = color
            };
        }

        private SelfServiceOrderProductViewModel CreateRowFromPreviouslyOrdered(LastTwoDetails orderedItem)
        {
            var product = orderedItem.Last.Product;
            var onHand = _order != null ? product.GetInventory(_order.AsPresale, false) : 0;
            var listPrice = Product.GetPriceForProduct(product, _order, false, false);
            var lastVisitText = string.Empty;
            var showLastVisit = false;
            if (orderedItem.Last != null && orderedItem.Last.Date != DateTime.MinValue)
            {
                if (Config.HidePriceInSelfService)
                    lastVisitText =
                        $"Last Visit: {orderedItem.Last.Date:MM/dd}, {orderedItem.Last.Quantity}";
                else
                    lastVisitText = $"Last Visit: {orderedItem.Last.Date:MM/dd}, {orderedItem.Last.Quantity}, {orderedItem.Last.Price.ToCustomString()}";
                
                showLastVisit = true;
            }
            var isSuggested = _order != null && _order.Client != null && Product.IsSuggestedForClient(_order.Client, product);
            var suggestedLabelText = isSuggested ? (string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) ? "Suggested Products" : $"{Config.ProductCategoryNameIdentifier} Products") : string.Empty;
            var imagePath = ProductImage.GetProductImageWithPlaceholder(product.ProductId);
            
            var ohStr = onHand > 0 ? "In Stock" : "Out of Stock";
            var color = onHand > 0 ? Color.FromArgb("#0a5713") : Color.FromArgb("#BA2D0B");
            
            if (Config.ShowOHQtyInSelfService)
            {
                ohStr = $"OH: {onHand:F0}";
                color = Color.FromArgb("#1f1f1f");
            }

            return new SelfServiceOrderProductViewModel(this)
            {
                Product = product,
                ProductName = product.Name,
                OnHandText = ohStr,
                ListPriceText = $"List Price: {listPrice.ToCustomString()}",
                LastVisitText = lastVisitText,
                ShowLastVisit = showLastVisit,
                PriceText = $"Price: {listPrice.ToCustomString()}",
                TotalText = "Total: $0.00",
                UomText = string.Empty,
                ExistingDetail = null,
                OrderedItem = orderedItem,
                Quantity = 0,
                OrderId = _order.OrderId,
                IsSuggested = isSuggested,
                SuggestedLabelText = suggestedLabelText,
                ProductImagePath = imagePath ?? string.Empty,
                HasImage = !string.IsNullOrEmpty(ProductImage.GetProductImage(product.ProductId)),
                QuantityText = "0",
                HistoryText = showLastVisit ? lastVisitText : "No previous orders",
                OhColor = color
            };
        }

        private void SortMergedList()
        {
            _mergedList = _mergedList.OrderBy(x => x.ProductName).ToList();
        }

        private void RefilterDisplay()
        {
            var toShow = ShowRecentlyOrdered ? _mergedList : _mergedList.Where(x => x.ExistingDetail != null).ToList();
            // Replace collection in one shot to avoid 1 Clear + N Add (each Add triggers UI layout; 18 items = 842 ms).
            DisplayProducts = new ObservableCollection<SelfServiceOrderProductViewModel>(toShow);
        }

        /// <summary>Update one or more rows in place from current order (no full list rebuild). When ShowRecentlyOrdered is false, refilter so removed items (ExistingDetail=null) disappear from the list.</summary>
        private void UpdateAffectedRows(params int[] productIds)
        {
            if (_order == null || productIds == null || productIds.Length == 0) return;
            var ids = new HashSet<int>(productIds);
            var needRefilter = false;
            foreach (var productId in ids)
            {
                var detail = _order.Details.FirstOrDefault(x => x.Product?.ProductId == productId && !x.IsCredit);
                var row = _mergedList.FirstOrDefault(x => x.Product?.ProductId == productId);
                if (row != null)
                {
                    RefreshRowInPlace(row, detail);
                    // When item was removed (detail is null), we must refilter so the row drops out of DisplayProducts when ShowRecentlyOrdered is false.
                    if (detail == null)
                        needRefilter = true;
                }
                else if (detail != null)
                {
                    var newRow = CreateRowFromOrderDetail(detail);
                    _mergedList.Add(newRow);
                    SortMergedList();
                    needRefilter = true;
                }
            }
            if (needRefilter)
                RefilterDisplay();
        }

        private void RefreshRowInPlace(SelfServiceOrderProductViewModel row, OrderDetail detail)
        {
            row?.RefreshFromOrder(detail, _order);
        }

        private void BuildSearchResults()
        {
            SearchResultItems.Clear();
            if (_order == null || string.IsNullOrWhiteSpace(SearchQuery)) return;

            var products = Product.GetProductListForOrder(_order, false, 0).ToList();
            var searchLower = SearchQuery.Trim().ToLowerInvariant();
            var filtered = products.Where(p =>
                (p.Name?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Upc?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Sku?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Code?.ToLowerInvariant().Contains(searchLower) ?? false)
            ).Take(100).ToList();

            foreach (var product in filtered)
                SearchResultItems.Add(new SelfServiceSearchProductViewModel(product, _order));
        }

        [RelayCommand]
        private void AddFromDisplayProduct(SelfServiceOrderProductViewModel item)
        {
            if (item?.Product == null || _order == null) return;
            var product = item.Product;
            var addQty = 1f;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
            if (existingDetail != null)
                existingDetail.Qty += addQty;
            else
            {
                var detail = new OrderDetail(product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(product, _order, false, false);
                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
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
                detail.UnitOfMeasure = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = addQty;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
            }
            OrderDetail.UpdateRelated(existingDetail ?? _order.Details.Last(), _order);
            _order.RecalculateDiscounts();
            _order.Save();
            UpdateAffectedRows(product.ProductId);
            RefreshTotals();
        }

        /// <summary>Show the same add/edit quantity popup as PreviouslyOrderedTemplatePage (ShowRestOfTheAddDialogAsync). Use for both row tap and quantity button.</summary>
        [RelayCommand]
        private async Task ShowAddItemPopup(SelfServiceOrderProductViewModel item)
        {
            if (item?.Product == null || _order == null) return;

            // When adding new item: restrict if no inventory (same as PreviouslyOrderedTemplatePageViewModel)
            if (item.ExistingDetail == null && !_order.AsPresale && !Config.CanGoBelow0)
            {
                var oh = item.Product.GetInventory(_order.AsPresale, false);
                if (oh <= 0)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            _noNeedToRefresh = true;

            OrderDetail existingDetail = item.ExistingDetail != null && !item.ExistingDetail.IsCredit ? item.ExistingDetail : null;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                item.Product,
                _order,
                existingDetail,
                isCredit: false,
                isDamaged: false,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled) return;

            if (result.Qty == 0)
            {
                var confirm = await _dialogService.ShowConfirmAsync("Are you sure you want to delete this item?",
                    "Remove Item", "Yes", "No");

                if (confirm)
                {
                    _noNeedToRefresh = false;

                    item.ExistingDetail = null;
                    
                    if (existingDetail != null)
                    {
                        _order.DeleteDetail(existingDetail);
                        _order.Save();
                    }

                    UpdateAffectedRows(item.Product.ProductId);

                    RefreshTotals();
                }
                
                return;
            }

            _noNeedToRefresh = false;
            
            // Inventory check when adding (same as PreviouslyOrderedTemplatePageViewModel)
            if (!_order.AsPresale && !Config.CanGoBelow0)
            {
                var currentOH = item.Product.GetInventory(_order.AsPresale, false);
                var resultBaseQty = (double)result.Qty;
                if (result.SelectedUoM != null)
                    resultBaseQty *= result.SelectedUoM.Conversion;
                var totalBaseQtyInOrder = _order.Details
                    .Where(d => d.Product.ProductId == item.Product.ProductId && !d.IsCredit && d != existingDetail)
                    .Sum(d =>
                    {
                        var q = (double)d.Qty;
                        if (d.UnitOfMeasure != null)
                            q *= d.UnitOfMeasure.Conversion;
                        return q;
                    });
                if (totalBaseQtyInOrder + resultBaseQty > currentOH)
                {
                    await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                    return;
                }
            }

            OrderDetail updatedDetail = null;
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
                existingDetail.Discount = result.Discount;
                existingDetail.DiscountType = result.DiscountType;
                if (result.PriceLevelSelected > 0)
                {
                    existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
                }
                updatedDetail = existingDetail;
            }
            else
            {
                var detail = new OrderDetail(item.Product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(item.Product, _order, false, false);
                double price = result.Price;
                if (result.UseLastSoldPrice && _order.Client != null)
                {
                    var clientHistory = InvoiceDetail.ClientProduct(_order.Client.ClientId, item.Product.ProductId);
                    if (clientHistory != null && clientHistory.Count > 0)
                    {
                        var lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
                        if (lastInvoiceDetail != null)
                            price = lastInvoiceDetail.Price;
                    }
                }
                else if (price == 0)
                {
                    double offerPrice = 0;
                    if (Offer.ProductHasSpecialPriceForClient(item.Product, _order.Client, out offerPrice))
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
                detail.UnitOfMeasure = result.SelectedUoM ?? item.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
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
                updatedDetail = detail;
            }

            if (updatedDetail != null)
            {
                OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, updatedDetail);
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }
            _order.Save();
            UpdateAffectedRows(item.Product.ProductId);
            RefreshTotals();
        }

        [RelayCommand]
        private void AddFromSearchProduct(SelfServiceSearchProductViewModel searchItem)
        {
            if (searchItem?.Product == null || _order == null) return;
            var product = searchItem.Product;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
            if (existingDetail != null)
                existingDetail.Qty += 1f;
            else
            {
                var detail = new OrderDetail(product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(product, _order, false, false);
                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
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
                detail.UnitOfMeasure = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = 1f;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
            }
            _order.RecalculateDiscounts();
            _order.Save();
            UpdateAffectedRows(product.ProductId);
            RefreshTotals();
        }

        private void RefreshTotals()
        {
            if (_order == null) return;
            LinesText = $"Lines: {_order.Details.Count}";
            QtySoldText = $"Qty Sold: {_order.Details.Sum(x => x.Qty)}";
            QtyText = $"Qty: {_order.Details.Sum(x => x.Qty)}";
            TermText = $"Term: {_order.Term}";
            TermsText = $"Terms: {_order.Term}";
            SubtotalText = $"Subtotal: {_order.CalculateItemCost().ToCustomString()}";
            DiscountText = $"Discount: {_order.CalculateDiscount().ToCustomString()}";
            TaxText = $"Tax: {_order.CalculateTax().ToCustomString()}";
            TotalText = $"Total: {_order.OrderTotalCost().ToCustomString()}";
            CanSendOrder = _order.Details.Count > 0;
        }

        [RelayCommand]
        private async Task SendOrder()
        {
            if (_order == null || _order.Details.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No items in order.", "Alert", "OK");
                return;
            }

            var totalQty = _order.Details.Sum(x => x.Qty);
            if (Config.OrderMinimumQty > 0 && totalQty < Config.OrderMinimumQty)
            {
                await _dialogService.ShowAlertAsync($"Minimum {Config.OrderMinimumQty} items required.", "Warning", "OK");
                return;
            }

            if (Config.OrderMinimumTotalPrice > 0 && _order.OrderTotalCost() < Config.OrderMinimumTotalPrice)
            {
                await _dialogService.ShowAlertAsync($"Minimum total price {Config.OrderMinimumTotalPrice.ToCustomString()} required.", "Warning", "OK");
                return;
            }

            if (Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select a ship date.", "Alert", "OK");
                return;
            }

            var result = await _dialogService.ShowConfirmationAsync("Continue sending order?", "Warning", "Yes", "No");
            if (!result)
                return;

            try
            {
                if (_order.Date.Date != DateTime.Now.Date)
                    _order.Date = DateTime.Now;

                if (_order.EndDate == DateTime.MinValue)
                    _order.EndDate = DateTime.Now;

                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);

                _order.Save();

                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                DataProvider.SendTheOrders(new[] { batch }, new List<string>() { _order.OrderId.ToString() });

                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success", "OK");

                if (Client.Clients.Count > 1)
                {
                    await Shell.Current.GoToAsync("//selfservice/clientlist");
                }
                else
                {
                    // Reset order for single client
                    var client = Client.Clients.First();
                    var newBatch = new Batch(client) { Client = client, ClockedIn = DateTime.Now };
                    newBatch.Save();
                    var companies = SalesmanAvailableCompany.GetCompanies(Config.SalesmanId, client.ClientId);
                    var newOrder = new Order(client) { AsPresale = true, OrderType = OrderType.Order, SalesmanId = Config.SalesmanId, BatchId = newBatch.Id };
                    if (companies.Count > 0)
                    {
                        newOrder.CompanyName = companies[0].CompanyName;
                        newOrder.CompanyId = companies[0].CompanyId;
                    }
                    newOrder.Save();
                    _order = newOrder;
                    LoadMergedList();
                    RefilterDisplay();
                    RefreshTotals();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error sending order: {ex.Message}", "Error", "OK");
                Logger.CreateLog(ex);
                _appService.TrackError(ex);
            }
        }

        /// <summary>Increment quantity by 1 (for advanced catalog-style rows). Add new line with qty 1 if not in order.</summary>
        [RelayCommand]
        private void IncrementQuantity(SelfServiceOrderProductViewModel item)
        {
            if (item?.Product == null || _order == null) return;
            if (item.ExistingDetail != null)
            {
                item.ExistingDetail.Qty += 1f;
                OrderDetail.UpdateRelated(item.ExistingDetail, _order);
                item.Quantity = (double)item.ExistingDetail.Qty;
            }
            else
            {
                AddFromDisplayProduct(item);
            }
            _order.RecalculateDiscounts();
            _order.Save();
            UpdateAffectedRows(item.Product.ProductId);
            RefreshTotals();
        }

        /// <summary>Decrement quantity by 1 (for advanced catalog-style rows). Remove line if qty becomes 0.</summary>
        [RelayCommand]
        private void DecrementQuantity(SelfServiceOrderProductViewModel item)
        {
            if (item?.Product == null || _order == null) return;
            if (item.ExistingDetail == null) return;
            var detail = item.ExistingDetail;
            if (detail.Qty <= 1f)
            {
                item.ExistingDetail = null;

                _order.Details.Remove(detail);
                _order.RecalculateDiscounts();
                _order.Save();
            }
            else
            {
                detail.Qty -= 1f;
                OrderDetail.UpdateRelated(detail, _order);
                item.Quantity = (double)detail.Qty;
                _order.RecalculateDiscounts();
                _order.Save();
            }
            UpdateAffectedRows(item.Product.ProductId);
            RefreshTotals();
        }

        [RelayCommand]
        private async Task EditQtyDisplay(SelfServiceOrderProductViewModel item)
        {
            if (item?.ExistingDetail == null || _order == null) return;
            var detail = item.ExistingDetail;
            var qtyString = await _dialogService.ShowPromptAsync("Edit Quantity", "Quantity", "OK", "Cancel", detail.Qty.ToString());
            if (qtyString == "Cancel" || string.IsNullOrEmpty(qtyString))
                return;

            if (float.TryParse(qtyString, out var qty))
            {
                if (qty == 0)
                {
                    var result = await _dialogService.ShowConfirmationAsync("Are you sure you want to delete this item?", "Warning", "Yes", "No");
                    if (result)
                    {
                        item.ExistingDetail = null;
                        
                        _order.Details.Remove(detail);
                        _order.RecalculateDiscounts();
                        _order.Save();
                        UpdateAffectedRows(item.Product.ProductId);
                        RefreshTotals();
                    }
                }
                else
                {
                    detail.Qty = qty;
                    OrderDetail.UpdateRelated(detail, _order);
                    detail.CalculateOfferDetail();
                    _order.RecalculateDiscounts();
                    _order.Save();
                    UpdateAffectedRows(item.Product.ProductId);
                    RefreshTotals();
                }
            }
        }

        public int? OrderId => _order?.OrderId;

        [RelayCommand]
        private async Task GoToCatalog()
        {
            if (OrderId == null || _order?.Client == null) return;
            await Shell.Current.GoToAsync($"selfservice/categories?orderId={OrderId}");
        }

        /// <summary>Open search popup: user types Name, Code, UPC, or SKU, or taps camera to scan; navigate to product catalog with filter/productId like categories does.</summary>
        [RelayCommand]
        private async Task GoToSearch()
        {
            _noNeedToRefresh = true;
            
            if (OrderId == null || _order?.Client == null) return;
            var searchInput = await _dialogService.ShowPromptAsync(
                "Search Product",
                "Enter Name, Code, UPC, or SKU",
                "OK",
                "Cancel",
                placeholder: "Name, Code, UPC, or SKU",
                showScanIcon: true,
                scanAction: ScanProductForSearchAsync);
            
            if (string.IsNullOrWhiteSpace(searchInput)) return;
            var searchTrim = searchInput.Trim();
            var products = Product.GetProductListForOrder(_order, false, 0).ToList();
            var searchLower = searchTrim.ToLowerInvariant();
            var hasMatch = products.Any(p =>
                (p.Name?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Upc?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Sku?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Code?.ToLowerInvariant().Contains(searchLower) ?? false));
            if (hasMatch)
            {
                var encoded = Uri.EscapeDataString(searchTrim);
                await Shell.Current.GoToAsync($"selfservice/catalog?orderId={OrderId}&productSearch={encoded}");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Product not found.", "Search");
            }
        }

        /// <summary>Scan barcode from Search Product dialog. Popup is closed by DialogService before this runs. If product found, navigate to catalog; else show "Product not found".</summary>
        private async Task<string> ScanProductForSearchAsync()
        {
            try
            {
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult)) return string.Empty;
                var product = Product.Products.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.Upc) && p.Upc.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Sku) && p.Sku.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Equals(scanResult, StringComparison.OrdinalIgnoreCase)));
                if (product != null && _order != null)
                {
                    await Shell.Current.GoToAsync($"selfservice/catalog?orderId={_order.OrderId}&categoryId={product.CategoryId}&productId={product.ProductId}&fromCheckout=1");
                    return DialogService.ScanResultAddedAndClose;
                }
                await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Search");
                return string.Empty;
            }
            catch (System.Exception ex)
            {
                Logger.CreateLog($"Error scanning barcode: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
                return string.Empty;
            }
        }

        /// <summary>More options (bottom bar): View Offers, Select Ship Date, Add Comment, Send by Email, Delete Order, View Captured Images (multi-client). Same as Xamarin SelfServiceCheckOutActivity.</summary>
        [RelayCommand]
        private async Task GoToMore()
        {
            var options = new List<string>
            {
                "View Offers",
                "Select Ship Date",
                "Add Comment",
                "Send by Email",
                "Delete Order"
            };
            if (Config.CaptureImages)
                options.Add("Attach Photo");

            var choice = await _dialogService.ShowActionSheetAsync("More Options", null, "Cancel", options.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

            if (choice == "View Offers")
                await ViewOffersAsync();
            else if (choice == "Select Ship Date")
                await SelectShipDateAsync();
            else if (choice == "Add Comment")
                await AddCommentAsync();
            else if (choice == "Send by Email")
                await SendByEmailAsync();
            else if (choice == "Delete Order")
                await DeleteOrderAsync();
            else if (choice == "Attach Photo")
                await ViewCapturedImagesAsync();
        }

        private async Task ViewOffersAsync()
        {
            if (_order == null) return;
            await Shell.Current.GoToAsync($"offers?orderId={_order.OrderId}");
        }

        private async Task SelectShipDateAsync()
        {
            if (_order == null) return;
            var current = _order.ShipDate.Year == 1 ? DateTime.Now : _order.ShipDate;
            var selected = await _dialogService.ShowDatePickerAsync("Select Ship Date", current, DateTime.Now, null);
            if (selected.HasValue)
            {
                _order.ShipDate = selected.Value;
                _order.Save();
                RefreshTotals();
            }
        }

        private async Task AddCommentAsync()
        {
            if (_order == null) return;
            var comments = await _dialogService.ShowPromptAsync("Add Comment", "Order comments", "OK", "Cancel", _order.Comments ?? "");
            if (comments != null)
            {
                _order.Comments = comments;
                _order.Save();
            }
        }

        private async Task SendByEmailAsync()
        {
            if (_order == null) return;
            try
            {
                await _dialogService.ShowLoadingAsync("Generating PDF...");
                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                    _order.Save();
                }
                var pdfFile = PdfHelper.GetOrderPdf(_order);
                await _dialogService.HideLoadingAsync();
                if (string.IsNullOrEmpty(pdfFile))
                {
                    await _dialogService.ShowAlertAsync("Error generating PDF.", "Alert", "OK");
                    return;
                }
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={_order.OrderId}");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error generating PDF.", "Alert", "OK");
            }
        }

        private async Task DeleteOrderAsync()
        {
            if (_order == null) return;
            var confirm = await _dialogService.ShowConfirmationAsync("Are you sure you want to delete this order?", "Alert", "Yes", "No");
            if (!confirm) return;
            if (Client.Clients.Count == 1)
            {
                _order.Details.Clear();
                _order.Save();
                LoadMergedList();
                RefilterDisplay();
                RefreshTotals();
            }
            else
            {
                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                if (batch != null)
                    batch.Delete();
                _order.Delete();
                await Shell.Current.GoToAsync("//selfservice/clientlist");
            }
        }

        private async Task ViewCapturedImagesAsync()
        {
            if (_order == null) return;
            await Shell.Current.GoToAsync($"viewcapturedimages?orderId={_order.OrderId}");
        }

        /// <summary>Top right toolbar menu. Sync Data and Sign Out only when self service has 1 client assigned; Advanced Options always.</summary>
        [RelayCommand]
        private async Task ShowToolbarMenuAsync()
        {
            var options = new List<string> {  "Sync Data From Server", "Advanced Options" };
            if (Client.Clients.Count == 1)
                options.Add("Sign Out");
            
            var choice = await _dialogService.ShowActionSheetAsync("Menu", null, "Cancel", options.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
            if (choice == "Sync Data From Server")
                await _mainPageViewModel.SyncDataFromMenuAsync();
            else if (choice == "Advanced Options")
                await _advancedOptionsService.ShowAdvancedOptionsAsync();
            else if (choice == "Sign Out")
                await _mainPageViewModel.SignOutFromSelfServiceAsync();
        }
    }

    /// <summary>Row for merged list (order + recently ordered). Same design as PreviouslyOrderedProductViewModel.</summary>
    public partial class SelfServiceOrderProductViewModel : ObservableObject
    {
        private readonly SelfServiceCheckOutPageViewModel _parent;

        public Product Product { get; set; }
        public OrderDetail ExistingDetail { get; set; }
        public LastTwoDetails OrderedItem { get; set; }
        public int OrderId { get; set; }

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _onHandText = "OH: 0";

        [ObservableProperty]
        private string _listPriceText = string.Empty;   
        
        [ObservableProperty]
        private Color _ohColor = Color.FromArgb("#1f1f1f");

        [ObservableProperty]
        private string _lastVisitText = string.Empty;

        [ObservableProperty]
        private bool _showLastVisit;

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private string _uomText = string.Empty;

        [ObservableProperty]
        private double _quantity;

        [ObservableProperty]
        private string _quantityButtonText = "+";

        [ObservableProperty]
        private bool _isSuggested;

        [ObservableProperty]
        private string _suggestedLabelText = string.Empty;

        [ObservableProperty]
        private bool _isEnabled = true;

        /// <summary>For advanced catalog style: product image path (or placeholder).</summary>
        [ObservableProperty]
        private string _productImagePath = string.Empty;

        /// <summary>For advanced catalog style: whether product has a real image (vs placeholder).</summary>
        [ObservableProperty]
        private bool _hasImage;

        /// <summary>For advanced catalog style: quantity as string (e.g. "2").</summary>
        [ObservableProperty]
        private string _quantityText = "0";

        /// <summary>For advanced catalog style: previously ordered text (e.g. "Last Visit: ..." or "No previous orders").</summary>
        [ObservableProperty]
        private string _historyText = string.Empty;

        public Color ProductNameColor { get; set; } = Colors.Black;

        /// <summary>True when item is in the current order (enables decrement in advanced style).</summary>
        public bool IsInOrder => ExistingDetail != null;

        /// <summary>Update row in place from order (avoids full list reload).</summary>
        public void RefreshFromOrder(OrderDetail detail, Order order)
        {
            if (Product == null) return;
            ExistingDetail = detail;
            if (detail != null)
            {
                var total = detail.Qty * detail.Price;
                Quantity = (double)detail.Qty;
                PriceText = $"Price: {detail.Price.ToCustomString()}";
                TotalText = $"Total: {total.ToCustomString()}";
                QuantityText = detail.Qty.ToString("F0");
                UomText = detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : string.Empty;
            }
            else
            {
                var listPrice = Product.GetPriceForProduct(Product, order, false, false);
                Quantity = 0;
                PriceText = $"Price: {listPrice.ToCustomString()}";
                TotalText = "Total: $0.00";
                QuantityText = "0";
                UomText = string.Empty;
            }
            OnPropertyChanged(nameof(IsInOrder));
        }

        public SelfServiceOrderProductViewModel(SelfServiceCheckOutPageViewModel parent)
        {
            _parent = parent;
        }

        partial void OnQuantityChanged(double value)
        {
            QuantityButtonText = value > 0 ? value.ToString("F0") : "+";
            QuantityText = value > 0 ? value.ToString("F0") : "0";
        }
    }

    /// <summary>Product row for search results on self service checkout (AdvancedCatalog-style list).</summary>
    public partial class SelfServiceSearchProductViewModel : ObservableObject
    {
        public Product Product { get; }
        private readonly Order _order;

        public string ProductName => Product?.Name ?? string.Empty;
        public string PriceText => _order != null && Product != null ? Product.GetPriceForProduct(Product, _order, false, false).ToCustomString() : "$0.00";

        public SelfServiceSearchProductViewModel(Product product, Order order)
        {
            Product = product;
            _order = order;
        }
    }
}

