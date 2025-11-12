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
        private string _totalText = "Total: $0.00";

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
                await _dialogService.ShowAlertAsync("Order not found.", "Error");
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

        private void LoadProductData()
        {
            if (_product == null || _order == null)
                return;

            ProductName = _product.Name;
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
                SelectedUom = UnitOfMeasures.FirstOrDefault(x => x.IsDefault) ?? UnitOfMeasures.FirstOrDefault();
            }
            else
            {
                ShowUom = false;
            }

            // Get price
            bool cameFromOffer = false;
            var price = Product.GetPriceForProduct(_product, _order, out cameFromOffer, _asCreditItem, _creditType == 1, SelectedUom?.UnitOfMeasure);
            PriceText = price.ToString("F2");
            ExpectedPrice = price;

            // Get list price
            var listPrice = _product.PriceLevel0;
            ListPriceText = listPrice > 0 ? $"List Price: ${listPrice:F2}" : string.Empty;

            // Get on hand
            var onHand = _product.GetInventory(_order.AsPresale);
            OnHandText = $"On Hand: {onHand}";

            // Check if product is sold by weight
            if (_product.SoldByWeight)
            {
                ShowWeight = true;
                ShowQuantity = false;
            }

            ShowLot =!_order.AsPresale && !IsDamaged && (_product.UseLot || _product.UseLotAsReference);

            CanEditPrice = Config.CanChangePrice(_order, _product, _asCreditItem);

            // Set credit item flags
            if (_asCreditItem)
            {
                IsDamaged = _creditType == 1;
                IsReturn = _creditType == 2;
            }

            UpdateTotal();
        }

        public async Task RefreshAsync()
        {
            if (_order == null || _product == null)
                return;

            // Refresh product data (inventory, price, etc.)
            var onHand = _product.GetInventory(_order.AsPresale);
            OnHandText = $"On Hand: {onHand}";

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

            var total = price * qty;
            TotalText = $"Total: ${total:F2}";
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

                // For consignment counting
                if (_consignmentCounting)
                {
                    detail.ConsignmentCounted = true;
                    detail.ConsignmentCount = qty;
                }

                _order.AddDetail(detail);
            }

            _order.Save();

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
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

