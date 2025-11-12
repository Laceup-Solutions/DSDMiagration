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
    public partial class ViewLoadOrderPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<LoadOrderItemViewModel> _loadOrders = new();
        [ObservableProperty] private DateTime _selectedDate = DateTime.Now;
        [ObservableProperty] private string _selectedDateText = string.Empty;
        [ObservableProperty] private bool _showAllAvailable;
        [ObservableProperty] private bool _isLoading;

        public ViewLoadOrderPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            SelectedDateText = SelectedDate.ToShortDateString();
            ShowAllAvailable = Config.ShowAllAvailableLoads;
        }

        public async Task OnAppearingAsync()
        {
            await LoadLoadOrdersAsync();
        }

        private async Task LoadLoadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    var orders = Order.Orders
                        .Where(x => x.OrderType == OrderType.Load)
                        .OrderByDescending(x => x.Date)
                        .ToList();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadOrders.Clear();
                        foreach (var order in orders)
                        {
                            LoadOrders.Add(new LoadOrderItemViewModel(order, this));
                        }
                    });
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            SelectedDateText = SelectedDate.ToShortDateString();
            LoadLoadOrdersAsync();
        }

        [RelayCommand]
        private async Task ViewLoadOrder(LoadOrderItemViewModel item)
        {
            if (item?.Order != null)
            {
                await Shell.Current.GoToAsync($"acceptload?orderId={item.Order.OrderId}");
            }
        }
    }

    public partial class LoadOrderItemViewModel : ObservableObject
    {
        private readonly Order _order;
        private readonly ViewLoadOrderPageViewModel _parent;

        [ObservableProperty] private bool _isSelected;

        public LoadOrderItemViewModel(Order order, ViewLoadOrderPageViewModel parent)
        {
            _order = order;
            _parent = parent;
        }

        public Order Order => _order;
        public string OrderNumberText => !string.IsNullOrEmpty(_order.PrintedOrderId) ? _order.PrintedOrderId : $"Order #{_order.OrderId}";
        public string DateText => $"Date: {_order.Date.ToShortDateString()}";
        public string ClientName => _order.Client?.ClientName ?? "Unknown";
    }
}

