using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// ViewModel for the Template Details screen (add/edit lines for one product).
    /// Matches Xamarin NewOrdertemplateDetailsActivity: all add/edit done on this screen, never navigates to additem.
    /// </summary>
    public partial class NewOrderTemplateDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private Order? _order;
        private GroupedTemplateLine? _line;
        private bool _fromCreditTemplate;
        private int _lastDetailId;
        private bool _initialized;
        private bool _isVendor;
        private double _baseExpectedPrice;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _onHandText = string.Empty;
        [ObservableProperty] private Color _onHandColor = Colors.Black;
        [ObservableProperty] private string _typeText = string.Empty;
        [ObservableProperty] private Color _typeColor = Colors.Black;
        [ObservableProperty] private string _priceText = string.Empty;
        [ObservableProperty] private Color _priceColor = Colors.Black;
        [ObservableProperty] private string _uomText = string.Empty;
        [ObservableProperty] private string _avgWeightText = string.Empty;
        [ObservableProperty] private string _amountText = string.Empty;
        [ObservableProperty] private string _totalQtyText = "0";
        [ObservableProperty] private bool _showPrice = true;
        [ObservableProperty] private bool _showSoldByWeight;
        [ObservableProperty] private bool _showAvgWeight;
        [ObservableProperty] private bool _canEdit = true;
        [ObservableProperty] private bool _hasDetailLines;

        public ObservableCollection<OrderTemplateDetailRowViewModel> DetailRows { get; } = new();

        public NewOrderTemplateDetailsPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
            ShowPrice = !Config.HidePriceInTransaction;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0, productId = 0, itemType = 0, fromCredit = 0;
            if (query.TryGetValue("orderId", out var v) && v != null) int.TryParse(v.ToString(), out orderId);
            if (query.TryGetValue("productId", out v) && v != null) int.TryParse(v.ToString(), out productId);
            if (query.TryGetValue("itemType", out v) && v != null) int.TryParse(v.ToString(), out itemType);
            if (query.TryGetValue("fromCreditTemplate", out v) && v != null) int.TryParse(v.ToString(), out fromCredit);
            if (orderId > 0 && productId > 0)
            {
                _fromCreditTemplate = fromCredit > 0;
                MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync(orderId, productId, itemType));
            }
        }

        public async Task InitializeAsync(int orderId, int productId, int itemType)
        {
            if (_initialized && _order?.OrderId == orderId && _line?.Product?.ProductId == productId)
            {
                Refresh();
                return;
            }
            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            var product = Product.Find(productId);
            if (_order == null || product == null)
            {
                await _dialogService.ShowAlertAsync("Order or product not found.", "Error");
                return;
            }
            _isVendor = _order.Client?.ExtraProperties?.FirstOrDefault(x => x != null && string.Equals(x.Item1, "VENDOR", StringComparison.OrdinalIgnoreCase)) is { } vendor
                && string.Equals(vendor.Item2, "YES", StringComparison.OrdinalIgnoreCase);
            bool isCredit = itemType > 0;
            bool damaged = itemType == 1;
            _line = new GroupedTemplateLine(_order, product, isCredit, damaged);
            _line.Details = _order.Details.Where(x => x.Product?.ProductId == productId && x.IsCredit == isCredit && x.Damaged == damaged).ToList();
            _baseExpectedPrice = _line.ExpectedPrice;
            if (_line.UoM != null)
                _baseExpectedPrice /= _line.UoM.Conversion;
            _initialized = true;
            Refresh();
            await Task.CompletedTask;
        }

        public void Refresh()
        {
            if (_line == null || _order == null) return;
            ProductName = _line.Product?.Name ?? "";
            OnHandText = "OH: " + _line.OH;
            OnHandColor = _line.OH <= 0 ? Colors.Red : Colors.Black;
            TypeText = _line.IsCredit ? (_line.Damaged ? "Dump" : "Return") : "";
            TypeColor = _line.IsCredit ? Colors.Orange : Colors.Black;
            PriceText = "Sug Price: " + _line.Price.ToCustomString();
            PriceColor = _line.IsPriceFromSpecial ? Colors.Green : Colors.Black;
            UomText = _line.UoM != null ? "Default UoM: " + _line.UoM.Name : "";
            ShowSoldByWeight = _line.Product?.SoldByWeight ?? false;
            ShowAvgWeight = ShowSoldByWeight && _line.Product?.Weight > 0;
            AvgWeightText = "Avg Weight: " + (_line.Product?.Weight ?? 0);
            AmountText = $"Amount: {_line.Amount.ToCustomString()}";
            TotalQtyText = $"Items Total: {_line.TotalQty.ToString()}";
            CanEdit = !_order.Locked();
            HasDetailLines = _line.Details.Count > 0;

            DetailRows.Clear();
            foreach (var d in _line.Details)
            {
                var row = new OrderTemplateDetailRowViewModel { Detail = d, IsHighlighted = d.OrderDetailId == _lastDetailId };
                row.UomText = d.UnitOfMeasure != null ? "UoM: " + d.UnitOfMeasure.Name : "";
                row.PriceText = "Price: " + d.Price.ToCustomString();
                row.LotText = "Lot: " + (d.Lot ?? "");
                row.LotExpText = "Lot Exp: " + (d.LotExpiration == DateTime.MinValue ? "" : d.LotExpiration.ToShortDateString());
                row.CommentText = d.Comments ?? "";
                row.QtyButtonText = d.Product?.SoldByWeight == true ? d.Weight.ToString() : d.Qty.ToString();
                row.StatusText = d.ReadyToFinalize ? "Completed" + (d.CompletedFromScanner ? " (Scanned)" : "") : "Pending";
                row.StatusColor = d.ReadyToFinalize ? Colors.Green : Colors.Red;
                DetailRows.Add(row);
            }
        }

        [RelayCommand]
        private async Task AddQty()
        {
            if (_order == null || _line?.Product == null || !CanEdit) return;

            bool needLot = _line.Product.UseLot && !Config.CanGoBelow0 && _order.OrderType == OrderType.Order;
            var lotsList = (_line.Product.ProductInv?.TruckInventories ?? new List<TruckInventory>())
                .Select(x => (x.Lot, Exp: x.Expiration)).ToList();
            string lastSoldPriceDisplay = _line.PreviouslyOrdered ? ("Last Sold Price: " + _line.PreviouslyOrderedPrice.ToCustomString()) : "";

            var result = await _dialogService.ShowTemplateAddQtyPopupAsync(
                _line.Product.Name,
                _line.Product.SoldByWeight,
                _line.Price,
                ShowPrice,
                lastSoldPriceDisplay,
                needLot,
                lotsList,
                useLotExpiration: needLot);

            if (result == null) return;

            var lineCount = result.LineCount;
            var weight = result.Weight;
            var price = result.Price;
            var currentLot = result.Lot ?? "";
            var currentExp = result.LotExpiration;

            if (!ValidateAddQtyPrice(price)) return;
            if (!_line.IsCredit && needLot && string.IsNullOrEmpty(currentLot))
            {
                await _dialogService.ShowAlertAsync("Lot is mandatory.", "Alert");
                return;
            }

            float baseqty = _line.Product.SoldByWeight && _line.Product.InventoryByWeight
                ? lineCount * (float)weight
                : (float)lineCount;
            if (_line.UoM != null)
                baseqty *= (float)_line.UoM.Conversion;

            if (!_line.IsCredit)
            {
                if (_line.Product.UseLot && !Config.CanGoBelow0 && !string.IsNullOrEmpty(currentLot))
                {
                    var ohForLot = _line.Product.GetInventory(_order.AsPresale, currentLot, true, weight);
                    if (ohForLot - baseqty < 0)
                    {
                        await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                        return;
                    }
                }
                else
                {
                    if (_line.Product.GetInventory(_order.AsPresale) - baseqty < 0)
                    {
                        await _dialogService.ShowAlertAsync("Not enough inventory.", "Alert");
                        return;
                    }
                }
            }

            int count = Math.Max(1, lineCount);
            for (int i = 0; i < count; i++)
            {
                var detail = new OrderDetail(_line.Product, 0, _order)
                {
                    IsCredit = _line.IsCredit,
                    Damaged = _line.Damaged,
                    UnitOfMeasure = _line.UoM
                };
                _order.AddDetail(detail);
                _line.Details.Add(detail);
                detail.Qty = _line.Product.SoldByWeight ? 1 : 1f;
                detail.Weight = (float)weight;
                detail.Price = price;
                detail.ExpectedPrice = _line.ExpectedPrice;
                if (detail.UnitOfMeasure != null)
                    detail.ExpectedPrice *= detail.UnitOfMeasure.Conversion;
                if (!string.IsNullOrEmpty(currentLot))
                {
                    detail.Lot = currentLot;
                    detail.LotExpiration = currentExp;
                }
                _order.UpdateInventory(detail, -1);
                if (weight > 0)
                    detail.WeightEntered = true;
                _lastDetailId = detail.OrderDetailId;
            }
            _order.Save();
            Refresh();
        }

        private bool ValidateAddQtyPrice(double price)
        {
            if (Config.AnyPriceIsAcceptable || _isVendor) return true;
            if (Math.Abs(price - _line.Price) < 0.0001) return true;
            double originalPrice = Product.GetPriceForProduct(_line.Product, _order, _line.IsCredit, _line.Damaged);
            if (_line.UoM != null) originalPrice *= _line.UoM.Conversion;
            if (!_line.IsCredit)
            {
                var lowest = _line.Product.LowestAcceptablePrice;
                if (_line.UoM != null) lowest *= _line.UoM.Conversion;
                if (price < lowest && Math.Round(price, Config.Round) != Math.Round(originalPrice, Config.Round))
                {
                    _ = _dialogService.ShowAlertAsync("Price is below lowest acceptable." + (Config.ShowLowestAcceptableOnWarning ? "\nLowest: " + _line.Product.LowestAcceptablePrice.ToCustomString() : ""), "Alert");
                    return false;
                }
                if (Config.CheckIfCanIncreasePrice(_order, _line.Product) && Math.Round(price, Config.Round) < Math.Round(originalPrice, Config.Round))
                {
                    _ = _dialogService.ShowAlertAsync("Price is too low.", "Alert");
                    return false;
                }
            }
            if (price == 0 && Math.Abs(price - originalPrice) > 0.0001)
            {
                _ = _dialogService.ShowAlertAsync("Zero price not allowed.", "Alert");
                return false;
            }
            return true;
        }

        [RelayCommand]
        private async Task EditDetail(OrderTemplateDetailRowViewModel? row)
        {
            if (row?.Detail == null || _line == null || _order == null || !CanEdit) return;

            var d = row.Detail;
            var initialQty = d.Product.SoldByWeight ? (float)d.Weight : d.Qty;
            var initialWeight = d.Product.SoldByWeight ? d.Weight : 0;
            var lotsList = (_line.Product.ProductInv?.TruckInventories ?? new List<TruckInventory>())
                .Select(x => (x.Lot, Exp: x.Expiration)).ToList();

            var result = await _dialogService.ShowTemplateEditLinePopupAsync(
                _line.Product.Name,
                _line.Product.SoldByWeight,
                ShowPrice,
                Config.AllowFreeItems,
                initialQty,
                initialWeight,
                d.Price,
                d.Comments ?? "",
                d.IsFreeItem,
                _line.Product.UseLot || _line.Product.UseLotAsReference,
                lotsList.Count > 0 ? lotsList : null,
                d.Lot ?? "",
                d.LotExpiration,
                _line.Product,
                d.UnitOfMeasure ?? _line.UoM);

            if (result == null) return;

            var uom = result.SelectedUoM ?? d.UnitOfMeasure ?? _line.UoM;
            float qtyOrWeight = _line.Product.SoldByWeight ? (float)result.Weight : result.Qty;
            AddEditValue(d, qtyOrWeight, result.Lot, result.LotExpiration, result.Price, uom, result.Comments, result.FreeItem, false);
        }

        private void AddEditValue(OrderDetail detail, float qty, string lot, DateTime lotExp, double price, UnitOfMeasure uom, string comment, bool freeItem, bool fromScanner)
        {
            if (_line == null || _order == null) return;

            if (freeItem && Config.FreeItemsNeedComments && string.IsNullOrEmpty(comment))
            {
                _ = _dialogService.ShowAlertAsync("Comment is mandatory for free item.", "Alert");
                return;
            }
            if (!Config.AnyPriceIsAcceptable && !_isVendor && !freeItem)
            {
                double originalPrice = Product.GetPriceForProduct(_line.Product, _order, _line.IsCredit, _line.Damaged);
                if (uom != null) originalPrice *= uom.Conversion;
                if (!_line.IsCredit)
                {
                    var lowest = _line.Product.LowestAcceptablePrice;
                    if (uom != null) lowest *= uom.Conversion;
                    if (price < lowest && Math.Round(price, Config.Round) != Math.Round(originalPrice, Config.Round))
                    {
                        _ = _dialogService.ShowAlertAsync("Price is below lowest acceptable.", "Alert");
                        return;
                    }
                    if (Config.CheckIfCanIncreasePrice(_order, _line.Product) && Math.Round(price, Config.Round) < Math.Round(originalPrice, Config.Round))
                    {
                        _ = _dialogService.ShowAlertAsync("Price is too low.", "Alert");
                        return;
                    }
                }
                if (price == 0 && Math.Abs(price - originalPrice) > 0.0001)
                {
                    _ = _dialogService.ShowAlertAsync("Zero price not allowed.", "Alert");
                    return;
                }
            }

            var cost = (double)qty * price;
            if (_order.Client != null)
            {
                double overCredit = _order.Client.GetOverCreditLimit(detail, cost, _line.IsCredit, _order.AsPresale);
                if (overCredit > 0)
                {
                    _ = _dialogService.ShowAlertAsync(string.Format("Customer credit limit exceeded by {0}.", overCredit.ToCustomString()), "Alert");
                    return;
                }
            }

            if (_line.Product.LotIsMandatory(_order.AsPresale, _line.Damaged) && string.IsNullOrEmpty(lot))
            {
                _ = _dialogService.ShowAlertAsync("Lot is mandatory.", "Alert");
                return;
            }
            if (Config.UseLotExpiration && lotExp == DateTime.MinValue && !string.IsNullOrEmpty(lot))
            {
                _ = _dialogService.ShowAlertAsync("Lot expiration is mandatory.", "Alert");
                return;
            }

            float baseQty = qty;
            if (_line.Product.SoldByWeight && !_line.Product.InventoryByWeight)
                baseQty = 1;
            if (uom != null)
                baseQty *= (float)uom.Conversion;
            float oldQty = 0;
            if (detail != null)
            {
                oldQty = _line.Product.SoldByWeight ? (_line.Product.InventoryByWeight ? detail.Weight : detail.Qty) : detail.Qty;
                if (detail.UnitOfMeasure != null)
                    oldQty *= (float)detail.UnitOfMeasure.Conversion;
                if (!(_line.Product.SoldByWeight && !_line.Product.InventoryByWeight))
                    _order.UpdateInventory(detail, 1);
            }
            var qtyToCheck = baseQty - oldQty;
            if (!Config.CanGoBelow0 && !_line.IsCredit)
            {
                var oh = _line.Product.GetInventory(_order.AsPresale, lot, true, _line.Product.SoldByWeight ? qty : 0);
                if (oh < qtyToCheck)
                {
                    if (detail != null && !(_line.Product.SoldByWeight && !_line.Product.InventoryByWeight))
                    {
                        _line.Details.Remove(detail);
                        _order.Details.Remove(detail);
                        _order.Save();
                    }
                    _ = _dialogService.ShowAlertAsync("Not enough inventory for this lot.", "Alert");
                    Refresh();
                    return;
                }
            }

            if (detail == null)
            {
                detail = new OrderDetail(_line.Product, 0, _order)
                {
                    IsCredit = _line.IsCredit,
                    Damaged = _line.Damaged
                };
                _order.AddDetail(detail);
                _line.Details.Add(detail);
            }

            if (_line.Product.SoldByWeight)
            {
                detail.Qty = 1;
                detail.Weight = qty;
            }
            else
                detail.Qty = qty;
            detail.Price = price;
            detail.IsFreeItem = freeItem;
            detail.Lot = lot;
            detail.LotExpiration = lotExp;
            detail.UnitOfMeasure = uom;
            detail.Comments = comment;
            detail.ExpectedPrice = _baseExpectedPrice;
            if (detail.UnitOfMeasure != null)
                detail.ExpectedPrice *= detail.UnitOfMeasure.Conversion;
            bool shouldUpdateInv = true;
            if (detail.Product.SoldByWeight && !detail.Product.InventoryByWeight && detail.WeightEntered)
                shouldUpdateInv = false;
            if (fromScanner)
                detail.CompletedFromScanner = true;
            detail.WeightEntered = true;
            if (shouldUpdateInv)
                _order.UpdateInventory(detail, -1);

            if (!detail.Product.SoldByWeight)
            {
                var oldDetail = _line.Details.FirstOrDefault(x => x.OrderDetailId != detail.OrderDetailId
                    && x.Lot == detail.Lot
                    && (x.UnitOfMeasure?.Id ?? 0) == (detail.UnitOfMeasure?.Id ?? 0)
                    && Math.Abs(x.Price - detail.Price) < 0.0001);
                if (oldDetail != null)
                {
                    oldDetail.Qty += detail.Qty;
                    _order.Details.Remove(detail);
                    _line.Details.Remove(detail);
                }
            }
            _order.Save();
            _lastDetailId = detail.OrderDetailId;
            Refresh();
        }

        [RelayCommand]
        private async Task DeleteDetail(OrderTemplateDetailRowViewModel? row)
        {
            if (row?.Detail == null || _line == null || _order == null || !CanEdit) return;
            var confirm = await _dialogService.ShowConfirmAsync("Would you like to delete this line?", "Delete", "Yes", "No");
            if (!confirm) return;
            var detail = row.Detail;
            if (detail.Product.SoldByWeight && !_line.Product.InventoryByWeight)
            {
                if (detail.Qty > 0 && detail.Weight > 0)
                    _order.UpdateInventory(detail, 1);
            }
            else
            {
                if (detail.Qty > 0)
                    _order.UpdateInventory(detail, 1);
            }
            _order.Details.Remove(detail);
            _line.Details.Remove(detail);
            _order.Save();
            Refresh();
        }

        [RelayCommand]
        private async Task DeleteAll()
        {
            if (_line == null || _order == null || !CanEdit) return;
            var confirm = await _dialogService.ShowConfirmAsync("Are you sure you want to delete all items for this product?", "Warning", "Yes", "No");
            if (!confirm) return;
            foreach (var item in _line.Details.ToList())
            {
                if (item.Product.SoldByWeight)
                {
                    if (item.Qty > 0 && item.Weight > 0)
                        _order.UpdateInventory(item, 1);
                }
                else
                {
                    if (item.Qty > 0)
                        _order.UpdateInventory(item, 1);
                }
                _order.Details.Remove(item);
            }
            _line.Details.Clear();
            _order.Save();
            Refresh();
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_order != null)
            {
                _order.Modified = true;
                _order.Save();
            }
            await GoBackToTemplateAndClearStackAsync();
        }

        [RelayCommand]
        private async Task OpenProductDetails()
        {
            if (_order?.Client == null || _line?.Product == null) return;
            await Shell.Current.GoToAsync(
                $"productdetails?clientId={_order.Client.ClientId}&productId={_line.Product.ProductId}");
        }

        public async Task GoBackAsync()
        {
            await GoBackToTemplateAndClearStackAsync();
        }

        /// <summary>
        /// Pops all screens in between and returns to NewOrderTemplatePage or NewCreditTemplatePage, clearing navigation state like the rest of the app.
        /// </summary>
        private async Task GoBackToTemplateAndClearStackAsync()
        {
            var templateRoute = _fromCreditTemplate ? "newcredittemplate" : "newordertemplate";
            await LaceupMigration.Helpers.NavigationHelper.PopBackToOrderOrCreditTemplateAsync(templateRoute);
        }

        public async Task OnAppearingAsync()
        {
            if (_order != null && _line != null)
                Refresh();
            await Task.CompletedTask;
        }
    }
}
