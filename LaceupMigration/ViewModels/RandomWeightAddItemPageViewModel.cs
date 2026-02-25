using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// Add-item screen for random weight flow (Config.NewAddItemRandomWeight).
    /// Mirrors Xamarin AddItemRandomWeightActivity: same intent params, product/order/orderDetail init,
    /// SoldByWeight UI (weight, optional avg weight), UoM, lot, price, free item, discount, add to order then save and navigate back.
    /// </summary>
    public partial class RandomWeightAddItemPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order? _order;
        private Product? _product;
        private OrderDetail? _existingDetail;
        private bool _asCreditItem;
        private int _creditType;
        private int _reasonId;
        private double _expectedPrice;
        private bool _useAvgWeight;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _quantityText = "1";
        [ObservableProperty] private string _weightText = string.Empty;
        [ObservableProperty] private string _priceText = string.Empty;
        [ObservableProperty] private string _listPriceText = string.Empty;
        [ObservableProperty] private string _onHandText = string.Empty;
        [ObservableProperty] private string _totalText = "$0.00";
        [ObservableProperty] private string _commentsText = string.Empty;
        [ObservableProperty] private bool _showQuantity = false;
        [ObservableProperty] private bool _showWeight = true;
        /// <summary>When true (SoldByWeight), Qty shows line count and is read-only.</summary>
        [ObservableProperty] private bool _isQuantityEnabled = true;
        [ObservableProperty] private bool _showPrice = true;
        [ObservableProperty] private bool _showUom = true;
        [ObservableProperty] private bool _showComments = true;
        [ObservableProperty] private bool _showLot = false;
        [ObservableProperty] private bool _isFreeItem = false;
        [ObservableProperty] private bool _canEditPrice = false;
        [ObservableProperty] private bool _showRandomWeightLabel = true;
        [ObservableProperty] private string _avgWeightText = string.Empty;
        [ObservableProperty] private bool _showAvgWeight = false;

        [ObservableProperty] private bool _showDiscountPerLine = false;
        [ObservableProperty] private bool _isDiscountPercentage = false;
        [ObservableProperty] private bool _isDiscountAmount = true;
        [ObservableProperty] private string _discountPercentText = string.Empty;
        [ObservableProperty] private string _discountAmountText = string.Empty;
        [ObservableProperty] private string _discountPercentEqualsText = string.Empty;

        [ObservableProperty] private string _lotText = string.Empty;

        /// <summary>Sum of all line weights. Shown when lines are present.</summary>
        [ObservableProperty] private string _weightTotalText = "0";
        /// <summary>Show the lines list and Add/Remove/Clear when SoldByWeight.</summary>
        [ObservableProperty] private bool _showLinesSection = true;

        /// <summary>True when any line has weight 0. Used to show Add Item (and line buttons) in red.</summary>
        [ObservableProperty] private bool _hasAnyLineWithZeroWeight;

        private readonly List<OrderDetail> _selectedDetails = new();
        private string _returnToRoute = "";

        public ObservableCollection<UnitOfMeasureViewModel> UnitOfMeasures { get; } = new();
        [ObservableProperty] private UnitOfMeasureViewModel? _selectedUom;

        /// <summary>Observable list of weight lines (order details) for this product. Used by CollectionView.</summary>
        public ObservableCollection<RandomWeightLineViewModel> Lines { get; } = new();

        public RandomWeightAddItemPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                if (query == null) return;
                int orderId = 0, orderDetailId = 0, type = 0, productId = 0;
                bool asCreditItem = false;
                if (query.TryGetValue("orderId", out var o) && o != null) int.TryParse(o.ToString(), out orderId);
                if (query.TryGetValue("orderDetail", out var od) && od != null) int.TryParse(od.ToString(), out orderDetailId);
                if (query.TryGetValue("type", out var t) && t != null) int.TryParse(t.ToString(), out type);
                if (query.TryGetValue("asCreditItem", out var ac) && ac != null) asCreditItem = ac.ToString() == "1" || string.Equals(ac.ToString(), "true", StringComparison.OrdinalIgnoreCase);
                if (query.TryGetValue("productId", out var p) && p != null) int.TryParse(p.ToString(), out productId);
                if (query.TryGetValue("reasonId", out var r) && r != null) int.TryParse(r.ToString(), out _reasonId);
                if (query.TryGetValue("returnToRoute", out var returnToVal) && returnToVal != null && !string.IsNullOrWhiteSpace(returnToVal.ToString()))
                    _returnToRoute = returnToVal.ToString() ?? "";

                _creditType = type;
                _asCreditItem = asCreditItem;
                _order = Order.Orders?.FirstOrDefault(x => x.OrderId == orderId);
                if (_order == null) return;

                _existingDetail = orderDetailId > 0 ? (_order.Details?.FirstOrDefault(x => x.OrderDetailId == orderDetailId)) : null;
                _product = _existingDetail?.Product ?? (productId > 0 ? Product.Find(productId) : null);
                if (_product == null) return;

                _useAvgWeight = _product.SoldByWeight && _order.AsPresale && _product.Weight > 0;
                ShowRandomWeightLabel = _product.SoldByWeight;
                ShowLinesSection = _product.SoldByWeight;
                ShowAvgWeight = _useAvgWeight;
                if (_useAvgWeight)
                    AvgWeightText = _product.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);

                ProductName = _product.Name ?? "";
                // SoldByWeight: show Qty (line count) disabled; hide weight entry (weight is per line in list).
                if (_product.SoldByWeight)
                {
                    ShowQuantity = true;
                    ShowWeight = false;
                    IsQuantityEnabled = false;
                }
                else
                {
                    ShowQuantity = false;
                    ShowWeight = true;
                    IsQuantityEnabled = true;
                }
                CanEditPrice = Config.CanChangePrice(_order, _product, _asCreditItem);
                ShowLot = !_order.AsPresale && _creditType != 1 && (_product.UseLot || _product.UseLotAsReference);

                UnitOfMeasures.Clear();
                if (_product.UnitOfMeasures != null && _product.UnitOfMeasures.Count > 0)
                {
                    var familyItems = _product.UnitOfMeasures.OrderBy(x => x.Conversion).ToList();
                    foreach (var uom in familyItems)
                        UnitOfMeasures.Add(new UnitOfMeasureViewModel { UnitOfMeasure = uom, Name = uom.Name, IsDefault = uom.IsDefault });

                    UnitOfMeasure? currentUoM = null;
                    if (_existingDetail != null)
                        currentUoM = _existingDetail.UnitOfMeasure;
                    else
                    {
                        currentUoM = _product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                        if (_order.Client != null && _order.Client.UseBaseUoM)
                            currentUoM = _product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);
                        currentUoM ??= _product.UnitOfMeasures.FirstOrDefault(x => x.IsBase) ?? _product.UnitOfMeasures.FirstOrDefault();
                    }
                    _selectedUom = UnitOfMeasures.FirstOrDefault(x => x.UnitOfMeasure?.Id == currentUoM?.Id) ?? UnitOfMeasures.FirstOrDefault();
                }
                else
                    ShowUom = false;

                if (_existingDetail != null)
                {
                    WeightText = _existingDetail.Weight > 0 ? _existingDetail.Weight.ToString("F2") : _existingDetail.Qty.ToString("F2");
                    QuantityText = _existingDetail.Qty.ToString("F2");
                    PriceText = _existingDetail.Price.ToString("F2");
                    _expectedPrice = _existingDetail.ExpectedPrice > 0 ? _existingDetail.ExpectedPrice : _existingDetail.Price;
                    CommentsText = _existingDetail.Comments ?? "";
                    LotText = _existingDetail.Lot ?? "";
                    IsFreeItem = _existingDetail.IsFreeItem;
                    if (_existingDetail.IsCredit)
                        _reasonId = _existingDetail.ReasonId;
                }
                else
                {
                    bool cameFromOffer = false;
                    _expectedPrice = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, _selectedUom?.UnitOfMeasure);
                    PriceText = _expectedPrice.ToString("F2");
                    WeightText = _product.Weight > 0 ? _product.Weight.ToString("F2") : "1";
                    QuantityText = "1";
                    IsFreeItem = false;
                }

                var oh = _product.GetInventory(_order.AsPresale, false);
                if (_selectedUom?.UnitOfMeasure != null && _selectedUom.UnitOfMeasure.Conversion > 0)
                    oh /= _selectedUom.UnitOfMeasure.Conversion;
                OnHandText = Math.Round(oh, Config.Round).ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (_selectedUom?.UnitOfMeasure != null)
                    OnHandText += " " + _selectedUom.UnitOfMeasure.Name;

                ListPriceText = _product.PriceLevel0 > 0 ? $"List Price: ${_product.PriceLevel0:F2}" : "";

                ShowDiscountPerLine = Config.AllowDiscountPerLine
                    && _order.Client?.UseDiscountPerLine == true
                    && (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Credit || _order.OrderType == OrderType.Return)
                    && !Config.HidePriceInTransaction
                    && ShowPrice;

                if (ShowDiscountPerLine && _existingDetail != null)
                {
                    if (_existingDetail.DiscountType == DiscountType.Percent)
                    {
                        IsDiscountPercentage = true;
                        IsDiscountAmount = false;
                        DiscountPercentText = (_existingDetail.Discount * 100).ToString("F0");
                        DiscountAmountText = "";
                    }
                    else
                    {
                        IsDiscountPercentage = false;
                        IsDiscountAmount = true;
                        DiscountAmountText = _existingDetail.Discount > 0 ? _existingDetail.Discount.ToString("F2") : "";
                        DiscountPercentText = "";
                    }
                }
                else if (ShowDiscountPerLine)
                {
                    IsDiscountPercentage = false;
                    IsDiscountAmount = true;
                    DiscountPercentText = "";
                    DiscountAmountText = "";
                }

                UpdateTotal();
                if (_product.SoldByWeight)
                    RefreshLines();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private List<OrderDetail> GetLinesForThisOrder()
        {
            if (_order?.Details == null || _product == null) return new List<OrderDetail>();
            var damaged = _creditType == 1;
            return _order.Details
                .Where(x => x.Product?.ProductId == _product.ProductId && x.Damaged == damaged && x.IsCredit == _asCreditItem)
                .ToList();
        }

        /// <summary>Rebuild Lines from order details, update WeightTotalText, TotalText, QuantityText.</summary>
        public void RefreshLines()
        {
            if (_order == null || _product == null) return;
            var orderLines = GetLinesForThisOrder();
            Lines.Clear();
            var index = 0;
            foreach (var d in orderLines)
            {
                var lineVm = new RandomWeightLineViewModel(
                    d,
                    index + 1,
                    _selectedDetails.Contains(d),
                    this,
                    _dialogService);
                Lines.Add(lineVm);
                index++;
            }
            QuantityText = orderLines.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var totalWeight = orderLines.Sum(x => x.Weight);
            WeightTotalText = totalWeight.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            HasAnyLineWithZeroWeight = orderLines.Any(x => x.Weight == 0);
            double price = 0;
            var first = orderLines.FirstOrDefault();
            if (first != null) price = first.Price;
            var total = orderLines.Sum(l => Math.Round(l.Weight * price, Config.Round));
            TotalText = total.ToCustomString();
        }

        private void UpdateTotal()
        {
            if (_product == null) return;
            if (!float.TryParse(WeightText, out var w)) w = 0;
            if (!double.TryParse(PriceText, out var price)) price = 0;
            var total = w * price;
            if (_useAvgWeight && _order != null && _order.AsPresale)
                total *= (double)_product.Weight;
            TotalText = $"${total:F2}";
        }

        partial void OnQuantityTextChanged(string value) => UpdateTotal();
        partial void OnWeightTextChanged(string value) => UpdateTotal();
        partial void OnPriceTextChanged(string value) => UpdateTotal();

        partial void OnIsFreeItemChanged(bool value)
        {
            if (value)
                PriceText = "0.00";
            else
                PriceText = _expectedPrice.ToString("F2");
            CanEditPrice = !value && Config.CanChangePrice(_order, _product, _asCreditItem);
            UpdateTotal();
        }

        partial void OnSelectedUomChanged(UnitOfMeasureViewModel? value)
        {
            if (value != null && _product != null && _order != null && !IsFreeItem)
            {
                bool cameFromOffer = false;
                _expectedPrice = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, value.UnitOfMeasure);
                PriceText = _expectedPrice.ToString("F2");
                var oh = _product.GetInventory(_order.AsPresale, false);
                if (value.UnitOfMeasure != null && value.UnitOfMeasure.Conversion > 0)
                    oh /= value.UnitOfMeasure.Conversion;
                OnHandText = Math.Round(oh, Config.Round).ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + value.UnitOfMeasure.Name;
            }
            UpdateTotal();
        }

        partial void OnIsDiscountPercentageChanged(bool value) { if (value) IsDiscountAmount = false; UpdateTotal(); }
        partial void OnIsDiscountAmountChanged(bool value) { if (value) IsDiscountPercentage = false; UpdateTotal(); }
        partial void OnDiscountPercentTextChanged(string value) => UpdateTotal();
        partial void OnDiscountAmountTextChanged(string value) => UpdateTotal();

        private void ApplyDiscountToDetail(OrderDetail detail, float qty)
        {
            if (!ShowDiscountPerLine) { detail.Discount = 0; return; }
            if (IsDiscountPercentage && double.TryParse(DiscountPercentText, out var percent))
            {
                detail.DiscountType = DiscountType.Percent;
                detail.Discount = (float)(percent / 100.0);
            }
            else if (IsDiscountAmount && double.TryParse(DiscountAmountText, out var amt))
            {
                detail.DiscountType = DiscountType.Amount;
                detail.Discount = (float)amt;
            }
            else
                detail.Discount = 0;
        }

        [RelayCommand]
        private async Task ViewProductDetailsAsync()
        {
            if (_product == null || _order == null) return;
            await Shell.Current.GoToAsync($"productdetails?productId={_product.ProductId}&clientId={_order.Client?.ClientId}");
        }

        [RelayCommand]
        private async Task AddToOrderAsync()
        {
            if (_order == null || _product == null) return;

            // When SoldByWeight, weight entry is hidden; user must add lines. "Add to order" saves and goes back.
            if (_product.SoldByWeight)
            {
                if (Lines.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Please add at least one line.", "Validation");
                    return;
                }
                if (_product.UseLotAsReference)
                {
                    var details = GetLinesForThisOrder();
                    foreach (var det in details)
                        det.Lot = LotText;
                }
                if (Config.Simone)
                    _order.SimoneCalculateDiscount();
                else
                    _order.RecalculateDiscounts();
                _order.Save();
                if (!string.IsNullOrEmpty(_returnToRoute))
                    await Helpers.NavigationHelper.PopBackToRouteAndClearIntermediateAsync(_returnToRoute, "randomweightadditem");
                else
                {
                    Helpers.NavigationHelper.RemoveNavigationState("randomweightadditem");
                    await Shell.Current.GoToAsync("..");
                }
                return;
            }

            if (!float.TryParse(WeightText, out var qty) || qty <= 0)
            {
                await _dialogService.ShowAlertAsync("Please enter a valid weight.", "Validation");
                return;
            }
            if (!double.TryParse(PriceText, out var price))
            {
                await _dialogService.ShowAlertAsync("Please enter a valid price.", "Validation");
                return;
            }

            if (IsFreeItem)
            {
                var freeDetail = _order.Details?.FirstOrDefault(x => x.Product?.ProductId == _product.ProductId && x.Price == 0);
                if (freeDetail != null)
                {
                    await _dialogService.ShowAlertAsync("Product is already in the order as free.", "Alert");
                    return;
                }
                if (Config.FreeItemsNeedComments && string.IsNullOrWhiteSpace(CommentsText))
                {
                    await _dialogService.ShowAlertAsync("Comment is required for free items.", "Alert");
                    return;
                }
            }

            var totalCost = qty * price;
            if (_useAvgWeight && _order.AsPresale)
                totalCost *= (double)_product.Weight;
            if (_order.Client != null)
            {
                var overCredit = _order.Client.GetOverCreditLimit(_existingDetail, totalCost, _asCreditItem, _order.AsPresale);
                if (overCredit > 0)
                {
                    await _dialogService.ShowAlertAsync($"Customer credit limit exceeded by {overCredit.ToCustomString()}.", "Alert");
                    return;
                }
            }

            if (_product.LotIsMandatory(_order.AsPresale, _creditType == 1) && string.IsNullOrWhiteSpace(LotText))
            {
                await _dialogService.ShowAlertAsync("Lot is required.", "Alert");
                return;
            }

            OrderDetail? updatedDetail = null;
            if (_existingDetail != null)
            {
                _existingDetail.Qty = _useAvgWeight && _order.AsPresale ? 1 : qty;
                _existingDetail.Weight = qty;
                _existingDetail.Price = price;
                _existingDetail.ExpectedPrice = _expectedPrice;
                _existingDetail.UnitOfMeasure = _selectedUom?.UnitOfMeasure;
                _existingDetail.Comments = CommentsText;
                _existingDetail.Lot = LotText;
                _existingDetail.IsCredit = _asCreditItem;
                _existingDetail.Damaged = _creditType == 1;
                _existingDetail.ReasonId = _reasonId;
                ApplyDiscountToDetail(_existingDetail, qty);
                updatedDetail = _existingDetail;
            }
            else
            {
                var detail = new OrderDetail(_product, _useAvgWeight && _order.AsPresale ? 1 : qty, _order);
                detail.Weight = qty;
                detail.Price = price;
                detail.ExpectedPrice = _expectedPrice;
                detail.UnitOfMeasure = _selectedUom?.UnitOfMeasure;
                detail.Comments = CommentsText;
                detail.Lot = LotText;
                detail.IsCredit = _asCreditItem;
                detail.Damaged = _creditType == 1;
                detail.ReasonId = _reasonId;
                ApplyDiscountToDetail(detail, qty);
                _order.AddDetail(detail);
                updatedDetail = detail;
            }

            if (updatedDetail != null)
            {
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }
            _order.Save();

            if (!string.IsNullOrEmpty(_returnToRoute))
                await Helpers.NavigationHelper.PopBackToRouteAndClearIntermediateAsync(_returnToRoute, "randomweightadditem");
            else
            {
                Helpers.NavigationHelper.RemoveNavigationState("randomweightadditem");
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            Helpers.NavigationHelper.RemoveNavigationState("randomweightadditem");
            await Shell.Current.GoToAsync("..");
        }

        internal void SetLineSelected(OrderDetail detail, bool selected)
        {
            if (selected && !_selectedDetails.Contains(detail))
                _selectedDetails.Add(detail);
            else if (!selected)
                _selectedDetails.Remove(detail);
        }

        [RelayCommand]
        private async Task AddLinesAsync()
        {
            if (_order == null || _product == null) return;
            var result = await _dialogService.ShowPromptAsync("Add lines", "Enter the number of cases:", "Add", "Cancel", "1", 4, "1", Keyboard.Numeric, selectAllText: true);
            if (string.IsNullOrEmpty(result)) return;
            if (!double.TryParse(result, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var qty) || qty <= 0) return;
            var count = (int)Math.Round(qty);
            bool cameFromOffer = false;
            var price = Config.DisplayTaxOnCatalogAndPrint
                ? Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, _selectedUom?.UnitOfMeasure)
                : (double.TryParse(PriceText, out var p) ? p : 0);
            var template = _existingDetail;
            for (int i = 0; i < count; i++)
            {
                var newDetail = new OrderDetail(_product, 1, _order);
                if (template != null)
                {
                    newDetail.Comments = template.Comments;
                    newDetail.Damaged = template.Damaged;
                    newDetail.ExpectedPrice = template.ExpectedPrice;
                    newDetail.IsCredit = template.IsCredit;
                    newDetail.Lot = template.Lot;
                    newDetail.Price = price;
                    newDetail.Qty = 1;
                    newDetail.UnitOfMeasure = template.UnitOfMeasure;
                    newDetail.Weight = 0;
                    newDetail.LotExpiration = template.LotExpiration;
                }
                else
                {
                    newDetail.Comments = CommentsText;
                    newDetail.Damaged = _creditType == 1;
                    newDetail.ExpectedPrice = price;
                    newDetail.IsCredit = _asCreditItem;
                    newDetail.Price = price;
                    newDetail.UnitOfMeasure = _selectedUom?.UnitOfMeasure;
                    newDetail.Weight = 0;
                    newDetail.Lot = LotText;
                }
                _order.AddDetail(newDetail);
            }
            _order.Save();
            RefreshLines();
        }

        [RelayCommand]
        private async Task RemoveLinesAsync()
        {
            if (_selectedDetails.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select one or more lines to remove.", "Alert");
                return;
            }
            foreach (var d in _selectedDetails.ToList())
            {
                _order?.DeleteDetail(d, d.Ordered > 0);
            }
            _selectedDetails.Clear();
            _order?.Save();
            RefreshLines();
        }

        [RelayCommand]
        private async Task ClearLinesAsync()
        {
            if (_order == null || _product == null) return;
            var confirm = await _dialogService.ShowConfirmAsync(
                "This will delete all lines for the product: " + _product.Name + ". Do you want to continue?",
                "Alert",
                "Yes",
                "No");
            if (!confirm) return;
            var damaged = _creditType == 1;
            var details = _order.Details?
                .Where(x => x.Product?.ProductId == _product.ProductId && x.Damaged == damaged && x.IsCredit == _asCreditItem)
                .ToList() ?? new List<OrderDetail>();
            foreach (var d in details)
                _order.DeleteDetail(d, d.Ordered > 0);
            _selectedDetails.Clear();
            _order.Save();
            RefreshLines();
        }

        internal void UpdateLineWeight(OrderDetail detail, float newWeight)
        {
            if (_order == null) return;
            _order.UpdateInventory(detail, 1);
            detail.Weight = newWeight;
            _order.UpdateInventory(detail, -1);
            _order.Save();
            RefreshLines();
        }
    }

    /// <summary>One line in the random weight list (Case #, Weight, Total, selection).</summary>
    public partial class RandomWeightLineViewModel : ObservableObject
    {
        private readonly RandomWeightAddItemPageViewModel _parent;
        private readonly DialogService _dialogService;

        public OrderDetail Detail { get; }

        [ObservableProperty] private int _caseNumber;
        [ObservableProperty] private bool _selected;
        [ObservableProperty] private string _weightText = "0";
        [ObservableProperty] private string _lineTotalText = "$0.00";
        [ObservableProperty] private string _lotText = string.Empty;
        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private bool _isWeightZero = true;

        public RandomWeightLineViewModel(OrderDetail detail, int caseNumber, bool selected, RandomWeightAddItemPageViewModel parent, DialogService dialogService)
        {
            Detail = detail;
            _parent = parent;
            _dialogService = dialogService;
            _caseNumber = caseNumber;
            _selected = selected;
            UpdateFromDetail();
        }

        internal void UpdateFromDetail()
        {
            WeightText = Detail.Weight.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var total = Math.Round(Detail.Weight * Detail.Price, Config.Round);
            LineTotalText = total.ToCustomString();
            LotText = Detail.Lot ?? string.Empty;
            CommentText = Detail.Comments ?? string.Empty;
            IsWeightZero = Detail.Weight == 0;
        }

        partial void OnSelectedChanged(bool value) => _parent?.SetLineSelected(Detail, value);

        [RelayCommand]
        private async Task EditWeightAsync()
        {
            if (Detail.Product?.FixedWeight == true) return;
            var current = Detail.Weight.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var result = await _dialogService.ShowPromptAsync(Detail.Product?.Name ?? "Weight", "Enter weight:", "Save", "Cancel", "0", 10, current, Keyboard.Numeric, selectAllText: true);
            if (string.IsNullOrEmpty(result)) return;
            if (!double.TryParse(result, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var w) || w < 0) return;
            _parent?.UpdateLineWeight(Detail, (float)w);
            UpdateFromDetail();
        }
    }
}
