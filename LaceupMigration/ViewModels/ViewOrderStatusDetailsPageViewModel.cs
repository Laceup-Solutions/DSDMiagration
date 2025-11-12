using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ViewOrderStatusDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private OrdersInOS? _order;

        [ObservableProperty] private string _clientNameText = string.Empty;
        [ObservableProperty] private string _documentNumberText = string.Empty;
        [ObservableProperty] private string _dateText = string.Empty;
        [ObservableProperty] private string _orderTotalText = string.Empty;
        [ObservableProperty] private bool _showTotal = true;
        [ObservableProperty] private string _statusText = string.Empty;
        [ObservableProperty] private Color _statusColor = Colors.Black;
        [ObservableProperty] private bool _showStatus = true;
        [ObservableProperty] private string _commentsText = string.Empty;
        [ObservableProperty] private bool _showComments;

        public ObservableCollection<OrderStatusDetailViewModel> OrderDetails { get; } = new();

        public ViewOrderStatusDetailsPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task InitializeAsync(int orderId)
        {
            _order = OrdersInOS.List.FirstOrDefault(x => x.OrderId == orderId);

            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("Order not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            LoadOrderData();
        }

        private void LoadOrderData()
        {
            if (_order == null)
                return;

            ClientNameText = $"Client: {_order.Client?.ClientName ?? "Unknown"}";
            DocumentNumberText = $"Document Number: {_order.PrintedOrderId ?? "N/A"}";
            DateText = $"Date: {_order.Date.ToShortDateString()}";
            OrderTotalText = $"Total: {_order.OrderTotalCost.ToCustomString()}";
            ShowTotal = !Config.HidePriceInTransaction;

            // Status
            var statusEx = DataAccess.GetSingleUDF("status", _order.ExtraFields);
            if (!string.IsNullOrEmpty(statusEx))
            {
                StatusText = $"Status: {statusEx}";
            }
            else
            {
                StatusText = $"Status: {_order.OrderStatus.ToString().Replace("_", " ")}";
            }

            // Status color
            if (_order.Reshipped)
            {
                StatusColor = Colors.Purple;
            }
            else
            {
                switch ((int)_order.OrderStatus)
                {
                    case 1:
                        StatusColor = Color.FromArgb("#0E86D4");
                        break;
                    case 2:
                        StatusColor = Colors.Brown;
                        break;
                    case 6:
                        StatusColor = Colors.Green;
                        break;
                    case 8:
                        StatusColor = Colors.Blue;
                        break;
                    case 9:
                    case 10:
                        StatusColor = Colors.DarkRed;
                        break;
                    default:
                        StatusColor = Colors.Black;
                        break;
                }
            }

            ShowStatus = Config.ShowOrderStatus;

            // Comments
            if (!string.IsNullOrEmpty(_order.Comments))
            {
                CommentsText = $"Comments: {_order.Comments}";
                ShowComments = true;
            }
            else
            {
                ShowComments = false;
            }

            // Order Details
            OrderDetails.Clear();
            foreach (var detail in _order.Details)
            {
                OrderDetails.Add(new OrderStatusDetailViewModel(detail));
            }
        }
    }

    public partial class OrderStatusDetailViewModel : ObservableObject
    {
        private readonly StatusOrderDetail _detail;

        public OrderStatusDetailViewModel(StatusOrderDetail detail)
        {
            _detail = detail;
        }

        public string ProductName => _detail.Product?.Name ?? "Product not found";
        public string QuantityText => $"Quantity: {_detail.Qty}";
        public string PriceText => $"Price: {_detail.Price.ToCustomString()}";
        public string TotalText => $"Total: {(_detail.Qty * _detail.Price).ToCustomString()}";
        public bool ShowPrice => !Config.HidePriceInTransaction;
        public string UomText => _detail.UnitOfMeasure != null ? $"UoM: {_detail.UnitOfMeasure.Name}" : string.Empty;
        public bool ShowUom => !string.IsNullOrEmpty(UomText);

        public string LineStatusText
        {
            get
            {
                var statusEx = DataAccess.GetSingleUDF("status", _detail.ExtraFields);
                if (!string.IsNullOrEmpty(statusEx))
                    return $"Status: {statusEx}";
                return string.Empty;
            }
        }

        public bool ShowLineStatus => !string.IsNullOrEmpty(LineStatusText);
    }
}

