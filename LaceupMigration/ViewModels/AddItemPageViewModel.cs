using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class AddItemPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order? _order;
        private Product? _product;
        private OrderDetail? _existingDetail;
        private bool _initialized;
        private bool _asCreditItem;
        private int _creditType; // 0 = normal, 1 = damaged, 2 = return
        private int _reasonId;
        private bool _consignmentCounting;
        private string _returnToRoute = "";

        /// <summary>When set (e.g. from PreviouslyOrderedTemplate or OrderCredit), Add success will pop back to this route instead of single ..</summary>
        public void SetReturnToRoute(string? route) => _returnToRoute = route ?? "";

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _productUpc = string.Empty;

        [ObservableProperty]
        private string _quantityText = "1";

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private string _listPriceText = string.Empty;

        [ObservableProperty]
        private string _onHandText = string.Empty;

        [ObservableProperty]
        private string _totalText = "$0.00";

        [ObservableProperty]
        private string _commentsText = string.Empty;

        [ObservableProperty]
        private bool _showQuantity = true;

        [ObservableProperty]
        private bool _showPrice = true;

        [ObservableProperty]
        private bool _showUom = true;

        [ObservableProperty]
        private bool _showComments = true;

        [ObservableProperty]
        private bool _showLot = false;

        [ObservableProperty]
        private bool _showWeight = false;

        [ObservableProperty]
        private bool _isFreeItem = false;

        [ObservableProperty]
        private bool _isDamaged = false;

        [ObservableProperty]
        private bool _isReturn = false;

        [ObservableProperty]
        private bool _canEditPrice = false;

        /// <summary>Show inline discount per line (Percentage/Amount) when client allows it. Matches Xamarin AddItemActivity.</summary>
        [ObservableProperty]
        private bool _showDiscountPerLine = false;

        [ObservableProperty]
        private bool _isDiscountPercentage = false;

        [ObservableProperty]
        private bool _isDiscountAmount = true;

        [ObservableProperty]
        private string _discountPercentText = string.Empty;

        [ObservableProperty]
        private string _discountAmountText = string.Empty;

        /// <summary>Computed discount amount for percentage (e.g. "1.10" when 20% of 5.50). Shown as "% = [value]".</summary>
        [ObservableProperty]
        private string _discountPercentEqualsText = string.Empty;

        [ObservableProperty]
        private string _weightText = string.Empty;

        [ObservableProperty]
        private string _lotText = string.Empty;

        public ObservableCollection<UnitOfMeasureViewModel> UnitOfMeasures { get; } = new();
        
        [ObservableProperty]
        private UnitOfMeasureViewModel? _selectedUom;

        public AddItemPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(int orderId, int productId, bool asCreditItem = false, 
            int type = 0, int reasonId = 0, bool consignmentCounting = false)
        {
            if (_initialized && _order?.OrderId == orderId && _product?.ProductId == productId)
            {
                await RefreshAsync();
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (_order == null)
            {
                // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            _product = Product.Products.FirstOrDefault(x => x.ProductId == productId);
            if (_product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found.", "Error");
                return;
            }

            _asCreditItem = asCreditItem;
            _creditType = type;
            _reasonId = reasonId;
            _consignmentCounting = consignmentCounting;

            _initialized = true;
            LoadProductData();
        }

        public async Task InitializeWithOrderDetailAsync(int orderId, int orderDetailId, bool asCreditItem = false,
            int type = 0, int reasonId = 0, bool consignmentCounting = false)
        {
            if (_initialized && _order?.OrderId == orderId && _existingDetail?.OrderDetailId == orderDetailId)
            {
                await RefreshAsync();
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (_order == null)
            {
                // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            _existingDetail = _order.Details.FirstOrDefault(x => x.OrderDetailId == orderDetailId);
            if (_existingDetail == null)
            {
                await _dialogService.ShowAlertAsync("Order detail not found.", "Error");
                return;
            }

            // Get product from order detail (matches Xamarin AddItemActivity behavior)
            _product = _existingDetail.Product;
            if (_product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found.", "Error");
                return;
            }

            _asCreditItem = asCreditItem || _existingDetail.IsCredit;
            _creditType = type;
            _reasonId = reasonId;
            _consignmentCounting = consignmentCounting;

            _initialized = true;
            LoadProductData();
        }

        private void LoadProductData()
        {
            if (_product == null || _order == null)
                return;

            // Format product name with full description (Code + Name + Package + UPC)
            // Example: "100756 Tostada Raspada Tonantzin 12/9.88oz 850000239005 (36Pp)"
            var parts = new List<string>();
            
            // Add code if available
            if (!string.IsNullOrEmpty(_product.Code))
            {
                parts.Add(_product.Code);
            }
            
            // Add product name
            parts.Add(_product.Name);
            
            // Add UPC if available
            if (!string.IsNullOrEmpty(_product.Upc))
            {
                parts.Add(_product.Upc);
            }
            
            // Add package info if available and not default
            if (!string.IsNullOrEmpty(_product.Package) && _product.Package != "1")
            {
                parts.Add($"({_product.Package}Pp)");
            }
            
            ProductName = string.Join(" ", parts);
            ProductUpc = _product.Upc ?? string.Empty;

            // Load UOMs
            UnitOfMeasures.Clear();
            if (_product.UnitOfMeasures != null && _product.UnitOfMeasures.Count > 0)
            {
                foreach (var uom in _product.UnitOfMeasures)
                {
                    UnitOfMeasures.Add(new UnitOfMeasureViewModel
                    {
                        UnitOfMeasure = uom,
                        Name = uom.Name,
                        IsDefault = uom.IsDefault
                    });
                }
                
                // If editing existing detail, use its UOM, otherwise use default
                if (_existingDetail != null && _existingDetail.UnitOfMeasure != null)
                {
                    SelectedUom = UnitOfMeasures.FirstOrDefault(x => x.UnitOfMeasure.Id == _existingDetail.UnitOfMeasure.Id) 
                        ?? UnitOfMeasures.FirstOrDefault(x => x.IsDefault) 
                        ?? UnitOfMeasures.FirstOrDefault();
                }
                else
                {
                    SelectedUom = UnitOfMeasures.FirstOrDefault(x => x.IsDefault) ?? UnitOfMeasures.FirstOrDefault();
                }
            }
            else
            {
                ShowUom = false;
            }

            // If editing existing detail, load values from it (matches Xamarin AddItemActivity behavior)
            if (_existingDetail != null)
            {
                // Load quantity/weight from order detail
                if (_product.SoldByWeight)
                {
                    WeightText = _existingDetail.Weight > 0 ? _existingDetail.Weight.ToString("F2") : _existingDetail.Qty.ToString("F2");
                    ShowWeight = true;
                    ShowQuantity = false;
                }
                else
                {
                    QuantityText = _existingDetail.Qty.ToString("F0");
                    ShowWeight = false;
                    ShowQuantity = true;
                }

                // Load price from order detail
                PriceText = _existingDetail.Price.ToString("F2");
                ExpectedPrice = _existingDetail.ExpectedPrice > 0 ? _existingDetail.ExpectedPrice : _existingDetail.Price;

                // Load comments
                CommentsText = _existingDetail.Comments ?? string.Empty;

                // Load lot
                LotText = _existingDetail.Lot ?? string.Empty;

                // Load free item flag
                IsFreeItem = _existingDetail.IsFreeItem;

                // Set credit item flags from order detail
                if (_existingDetail.IsCredit)
                {
                    IsDamaged = _existingDetail.Damaged;
                    IsReturn = !_existingDetail.Damaged;
                    _reasonId = _existingDetail.ReasonId;
                }
            }
            else
            {
                // New item - get price
                bool cameFromOffer = false;
                var price = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, SelectedUom?.UnitOfMeasure);
                PriceText = price.ToString("F2");
                ExpectedPrice = price;

                // Set default quantity
                if (_product.SoldByWeight)
                {
                    ShowWeight = true;
                    ShowQuantity = false;
                    WeightText = "1";
                }
                else
                {
                    ShowWeight = false;
                    ShowQuantity = true;
                    QuantityText = "1";
                }

                // Set credit item flags
                if (_asCreditItem)
                {
                    IsDamaged = _creditType == 1;
                    IsReturn = _creditType == 2;
                }
            }

            // Get list price
            var listPrice = _product.PriceLevel0;
            ListPriceText = listPrice > 0 ? $"List Price: ${listPrice:F2}" : string.Empty;

            // Get on hand (format as integer like in the image: OH:3001)
            var onHand = _product.GetInventory(_order.AsPresale);
            OnHandText = ((int)onHand).ToString();

            ShowLot =!_order.AsPresale && !IsDamaged && (_product.UseLot || _product.UseLotAsReference);

            CanEditPrice = Config.CanChangePrice(_order, _product, _asCreditItem);

            // Discount per line (inline, matches Xamarin AddItemActivity)
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
                    DiscountAmountText = string.Empty;
                }
                else
                {
                    IsDiscountPercentage = false;
                    IsDiscountAmount = true;
                    DiscountAmountText = _existingDetail.Discount > 0 ? _existingDetail.Discount.ToString("F2") : string.Empty;
                    DiscountPercentText = string.Empty;
                }
            }
            else if (ShowDiscountPerLine)
            {
                IsDiscountPercentage = false;
                IsDiscountAmount = true;
                DiscountPercentText = string.Empty;
                DiscountAmountText = string.Empty;
            }

            UpdateTotal();
        }

        public async Task RefreshAsync()
        {
            if (_order == null || _product == null)
                return;

            // Refresh product data (inventory, price, etc.)
            var onHand = _product.GetInventory(_order.AsPresale);
            OnHandText = ((int)onHand).ToString();

            // Recalculate price if UOM changed
            if (SelectedUom != null)
            {
                bool cameFromOffer = false;
                var price = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, SelectedUom.UnitOfMeasure);
                if (!IsFreeItem)
                {
                    PriceText = price.ToString("F2");
                    ExpectedPrice = price;
                }
            }

            UpdateTotal();
            await Task.CompletedTask;
        }

        private double ExpectedPrice { get; set; }

        partial void OnQuantityTextChanged(string value)
        {
            UpdateTotal();
        }

        partial void OnPriceTextChanged(string value)
        {
            UpdateTotal();
        }

        partial void OnWeightTextChanged(string value)
        {
            UpdateTotal();
        }

        partial void OnIsFreeItemChanged(bool value)
        {
            if (value)
            {
                PriceText = "0.00";
                CanEditPrice = false;
            }
            else
            {
                PriceText = ExpectedPrice.ToString("F2");
                CanEditPrice = Config.CanChangePrice(_order, _product, _asCreditItem) && _order != null && !_order.Locked();
            }
            UpdateTotal();
        }

        partial void OnSelectedUomChanged(UnitOfMeasureViewModel? value)
        {
            if (value != null && _product != null && _order != null)
            {
                bool cameFromOffer = false;
                var price = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, value.UnitOfMeasure);
                PriceText = price.ToString("F2");
                ExpectedPrice = price;
                UpdateTotal();
            }
        }

        partial void OnIsDiscountPercentageChanged(bool value)
        {
            if (value)
                IsDiscountAmount = false;
            UpdateTotal();
        }

        partial void OnIsDiscountAmountChanged(bool value)
        {
            if (value)
                IsDiscountPercentage = false;
            UpdateTotal();
        }

        partial void OnDiscountPercentTextChanged(string value)
        {
            UpdateTotal();
        }

        partial void OnDiscountAmountTextChanged(string value)
        {
            UpdateTotal();
        }

        /// <summary>Apply inline discount to detail. Percent: store fraction (e.g. 0.10). Amount: store per unit (Order multiplies by Qty). Matches Xamarin AddItemActivity.</summary>
        private void ApplyDiscountToDetail(OrderDetail detail, float qty)
        {
            if (!ShowDiscountPerLine)
            {
                detail.Discount = 0;
                return;
            }

            if (IsDiscountPercentage && double.TryParse(DiscountPercentText, out var percent))
            {
                detail.DiscountType = DiscountType.Percent;
                detail.Discount = Math.Round(percent / 100.0, 4);
            }
            else if (IsDiscountAmount && double.TryParse(DiscountAmountText, out var amt))
            {
                detail.DiscountType = DiscountType.Amount;
                detail.Discount = Math.Round(amt, 4); // per unit; Order multiplies by Qty
            }
            else
            {
                detail.Discount = 0;
            }
        }

        private void UpdateTotal()
        {
            if (_product == null)
                return;

            double price = 0;
            if (double.TryParse(PriceText, out var p))
                price = p;

            float qty = 1;
            if (_product.SoldByWeight)
            {
                if (float.TryParse(WeightText, out var w))
                    qty = w;
            }
            else
            {
                if (float.TryParse(QuantityText, out var q))
                    qty = q;
            }

            var lineTotal = price * qty;
            var discountAmount = 0.0;

            if (ShowDiscountPerLine)
            {
                if (IsDiscountPercentage && double.TryParse(DiscountPercentText, out var percent))
                {
                    discountAmount = lineTotal * (percent / 100.0);
                    DiscountPercentEqualsText = discountAmount.ToString("F2");
                }
                else if (IsDiscountAmount && double.TryParse(DiscountAmountText, out var amt))
                {
                    // Amount is per unit (Order multiplies by Qty)
                    discountAmount = amt * qty;
                    DiscountPercentEqualsText = string.Empty;
                }
            }

            var total = Math.Max(0, lineTotal - discountAmount);
            TotalText = $"${total:F2}";
        }

        [RelayCommand]
        private async Task AddToOrderAsync()
        {
            if (_order == null || _product == null)
                return;

            // Validate quantity/weight
            float qty = 1;
            if (_product.SoldByWeight)
            {
                if (!float.TryParse(WeightText, out qty) || qty <= 0)
                {
                    await _dialogService.ShowAlertAsync("Please enter a valid weight.", "Validation");
                    return;
                }
            }
            else
            {
                if (!float.TryParse(QuantityText, out qty) || qty <= 0)
                {
                    await _dialogService.ShowAlertAsync("Please enter a valid quantity.", "Validation");
                    return;
                }
            }

            // Validate price
            double price = 0;
            if (!double.TryParse(PriceText, out price))
            {
                await _dialogService.ShowAlertAsync("Please enter a valid price.", "Validation");
                return;
            }

            // Check inventory
            var onHand = _product.GetInventory(_order.AsPresale);
            if (onHand < qty && !_order.AsPresale)
            {
                var result = await _dialogService.ShowConfirmAsync(
                    $"Not enough inventory. On hand: {onHand}, Requested: {qty}. Continue anyway?",
                    "Warning",
                    "Yes",
                    "No");
                if (!result)
                    return;
            }

            // Create or update order detail
            OrderDetail? updatedDetail = null;
            if (_existingDetail != null)
            {
                // Update existing detail
                _existingDetail.Qty = qty;
                _existingDetail.Price = price;
                _existingDetail.Weight = _product.SoldByWeight ? qty : 0;
                _existingDetail.UnitOfMeasure = SelectedUom?.UnitOfMeasure;
                _existingDetail.Comments = CommentsText;
                _existingDetail.Lot = LotText;
                _existingDetail.IsCredit = _asCreditItem;
                _existingDetail.Damaged = IsDamaged;
                _existingDetail.ReasonId = _reasonId;
                ApplyDiscountToDetail(_existingDetail, qty);
                updatedDetail = _existingDetail;
            }
            else
            {
                // Create new detail
                var detail = new OrderDetail(_product, qty, _order);
                detail.Price = price;
                detail.ExpectedPrice = ExpectedPrice;
                detail.Weight = _product.SoldByWeight ? qty : 0;
                detail.UnitOfMeasure = SelectedUom?.UnitOfMeasure;
                detail.Comments = CommentsText;
                detail.Lot = LotText;
                detail.IsCredit = _asCreditItem;
                detail.Damaged = IsDamaged;
                detail.ReasonId = _reasonId;
                ApplyDiscountToDetail(detail, qty);

                // For consignment counting
                if (_consignmentCounting)
                {
                    detail.ConsignmentCounted = true;
                    detail.ConsignmentCount = qty;
                }

                _order.AddDetail(detail);
                updatedDetail = detail;
            }

            // Update related details and recalculate discounts (matches Xamarin behavior)
            if (updatedDetail != null)
            {
                OrderDetail.UpdateRelated(updatedDetail, _order);
                _order.RecalculateDiscounts();
            }

            _order.Save();

            if (!string.IsNullOrEmpty(_returnToRoute))
                await Helpers.NavigationHelper.PopBackToRouteAndClearIntermediateAsync(_returnToRoute, "additem");
            else
                await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ViewProductDetailsAsync()
        {
            if (_product == null || _order == null)
                return;

            // Navigate to product details page
            await Shell.Current.GoToAsync($"productdetails?productId={_product.ProductId}&orderId={_order.OrderId}");
        }

    }

    public partial class UnitOfMeasureViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isDefault;

        public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    }
}

