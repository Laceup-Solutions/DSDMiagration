using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCreditTemplatePageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<OrderLineItemViewModel> _orderLines = new();

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
        private bool _canEdit = true;

        public SelfServiceCreditTemplatePageViewModel(IDialogService dialogService, ILaceupAppService appService)
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
                    // Xamarin PreviouslyOrderedTemplateActivity logic:
                    // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
                    CanEdit = !(!_order.AsPresale && (_order.Finished || _order.Voided));
                    
                    ClientName = _order.Client?.ClientName ?? string.Empty;
                    LoadOrderLines();
                    RefreshTotals();
                }
            }
        }

        public void OnAppearing()
        {
            if (_order != null)
            {
                // Xamarin PreviouslyOrderedTemplateActivity logic:
                // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
                CanEdit = !(!_order.AsPresale && (_order.Finished || _order.Voided));
            }
            LoadOrderLines();
            RefreshTotals();
        }

        private void LoadOrderLines()
        {
            if (_order == null)
                return;

            OrderLines.Clear();
            foreach (var detail in _order.Details)
            {
                OrderLines.Add(new OrderLineItemViewModel(detail));
            }
        }

        private void RefreshTotals()
        {
            if (_order == null)
                return;

            LinesText = $"Lines: {_order.Details.Count}";
            QtyText = $"Qty: {_order.Details.Sum(x => x.Qty)}";
            TermText = $"Term: {_order.Term ?? "N/A"}";
            SubtotalText = $"Subtotal: {_order.CalculateItemCost().ToCustomString()}";
            DiscountText = $"Discount: {_order.CalculateDiscount().ToCustomString()}";
            TaxText = $"Tax: {_order.CalculateTax().ToCustomString()}";
            TotalText = $"Total: {_order.OrderTotalCost().ToCustomString()}";
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task History()
        {
            if (_order == null || !CanEdit)
                return;

            await Shell.Current.GoToAsync($"selfservice/template?orderId={_order.OrderId}");
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task Catalog()
        {
            if (_order == null || !CanEdit)
                return;

            await Shell.Current.GoToAsync($"selfservice/catalog?orderId={_order.OrderId}");
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task Search()
        {
            if (_order == null || !CanEdit)
                return;

            await Shell.Current.GoToAsync($"selfservice/categories?orderId={_order.OrderId}");
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task Send()
        {
            if (_order == null || !CanEdit)
                return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Are you sure you want to send this credit order?",
                "Confirm Send",
                "Yes",
                "No");

            if (!confirm)
                return;

            try
            {
                // Send the credit order
                _order.Save();
                DataAccess.SendTheOrders(new System.Collections.Generic.List<Batch>(), new System.Collections.Generic.List<string> { _order.OrderId.ToString() });

                await _dialogService.ShowAlertAsync("Credit order sent successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("selfservice/checkout");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error sending credit order: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task RemoveLine(OrderLineItemViewModel lineItem)
        {
            if (lineItem == null || _order == null || !CanEdit)
                return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Are you sure you want to remove this item?",
                "Confirm Remove",
                "Yes",
                "No");

            if (!confirm)
                return;

            _order.Details.Remove(lineItem.Detail);
            _order.Save();
            LoadOrderLines();
            RefreshTotals();
        }
    }

    public partial class OrderLineItemViewModel : ObservableObject
    {
        public OrderDetail Detail { get; }

        [ObservableProperty]
        private string _productName;

        [ObservableProperty]
        private string _qtyText;

        [ObservableProperty]
        private string _priceText;

        public OrderLineItemViewModel(OrderDetail detail)
        {
            Detail = detail;
            ProductName = detail.Product?.Name ?? "Unknown";
            QtyText = $"Qty: {detail.Qty}";
            PriceText = $"Price: {detail.Price.ToCustomString()}";
        }
    }
}

