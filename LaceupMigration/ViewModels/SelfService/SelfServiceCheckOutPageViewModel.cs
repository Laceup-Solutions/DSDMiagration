using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCheckOutPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _qtyText = "Qty: 0";

        [ObservableProperty]
        private string _termText = "Term: ";

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

        [ObservableProperty]
        private ObservableCollection<OrderDetailItemViewModel> _orderDetails = new();

        public SelfServiceCheckOutPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    LoadOrder();
                }
            }
        }

        public void OnAppearing()
        {
            if (_order != null)
            {
                Refresh();
            }
        }

        private void LoadOrder()
        {
            if (_order == null) return;

            ClientName = _order.Client.ClientName;
            Refresh();
        }

        private void Refresh()
        {
            if (_order == null) return;

            LinesText = $"Lines: {_order.Details.Count}";
            QtyText = $"Qty: {_order.Details.Sum(x => x.Qty)}";
            TermText = $"Term: {_order.Term}";
            SubtotalText = $"Subtotal: {_order.CalculateItemCost().ToCustomString()}";
            DiscountText = $"Discount: {_order.CalculateDiscount().ToCustomString()}";
            TaxText = $"Tax: {_order.CalculateTax().ToCustomString()}";
            TotalText = $"Total: {_order.OrderTotalCost().ToCustomString()}";
            CanSendOrder = _order.Details.Count > 0;

            OrderDetails.Clear();
            foreach (var detail in _order.Details.OrderBy(x => x.Product.Name))
            {
                OrderDetails.Add(new OrderDetailItemViewModel(detail));
            }
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
                DataAccess.SendTheOrders(new[] { batch }, new List<string>() { _order.OrderId.ToString() });

                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success", "OK");

                if (Client.Clients.Count > 1)
                {
                    await Shell.Current.GoToAsync("selfservice/clientlist");
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
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error sending order: {ex.Message}", "Error", "OK");
                Logger.CreateLog(ex);
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task EditQty(OrderDetailItemViewModel item)
        {
            if (item?.Detail == null) return;

            var qtyString = await _dialogService.ShowPromptAsync("Edit Quantity", "Quantity", "OK", "Cancel", item.Detail.Qty.ToString());
            if (qtyString == "Cancel" || string.IsNullOrEmpty(qtyString))
                return;

            if (float.TryParse(qtyString, out var qty))
            {
                if (qty == 0)
                {
                    var result = await _dialogService.ShowConfirmationAsync("Are you sure you want to delete this item?", "Warning", "Yes", "No");
                    if (result)
                    {
                        _order.Details.Remove(item.Detail);
                        _order.RecalculateDiscounts();
                        _order.Save();
                        Refresh();
                    }
                }
                else
                {
                    item.Detail.Qty = qty;
                    OrderDetail.UpdateRelated(item.Detail, _order);
                    item.Detail.CalculateOfferDetail();
                    _order.RecalculateDiscounts();
                    _order.Save();
                    Refresh();
                }
            }
        }
    }

    public partial class OrderDetailItemViewModel : ObservableObject
    {
        public OrderDetail Detail { get; }

        public string ProductName => Detail?.Product?.Name ?? string.Empty;
        public string PriceText => $"Price: {Detail?.Price.ToCustomString() ?? "$0.00"}";
        public string UomText => Detail?.UnitOfMeasure != null ? $"UOM: {Detail.UnitOfMeasure.Name}" : string.Empty;
        public string QtyText => Detail?.Qty.ToString() ?? "0";

        public OrderDetailItemViewModel(OrderDetail detail)
        {
            Detail = detail;
        }
    }
}

