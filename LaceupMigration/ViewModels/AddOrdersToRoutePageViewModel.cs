using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class AddOrdersToRoutePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<OrderRouteItemViewModel> _allOrders = new();

        [ObservableProperty] private ObservableCollection<OrderRouteItemViewModel> _orders = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public AddOrdersToRoutePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // TODO: Load available orders from Order.Orders
                // Filter orders that are not already in a route
                _allOrders = Order.Orders
                    .Where(o => !o.Finished && o.Client != null)
                    .Select(o => new OrderRouteItemViewModel
                    {
                        OrderId = o.OrderId,
                        ClientName = o.Client?.ClientName ?? "Unknown",
                        OrderDate = o.Date,
                        Total = o.OrderTotalCost(),
                        IsSelected = false
                    })
                    .ToList();

                FilterOrders(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading orders: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterOrders(string searchText)
        {
            Orders.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allOrders
                : _allOrders.Where(x => 
                    x.ClientName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var order in filtered)
            {
                Orders.Add(order);
            }
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var order in Orders)
            {
                order.IsSelected = true;
            }
        }

        [RelayCommand]
        private async Task AddSelected()
        {
            var selectedOrders = Orders.Where(x => x.IsSelected).ToList();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select at least one order.", "Info", "OK");
                return;
            }

            try
            {
                // TODO: Add selected orders to route
                // This should be handled by the parent ManageRoutePage
                var result = new Dictionary<string, object>
                {
                    { "selectedOrderIds", selectedOrders.Select(x => x.OrderId).ToList() }
                };

                await Shell.Current.GoToAsync("..", result);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error adding orders: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public partial class OrderRouteItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _orderId;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private DateTime _orderDate;
        [ObservableProperty] private double _total;
        [ObservableProperty] private bool _isSelected;
    }
}

