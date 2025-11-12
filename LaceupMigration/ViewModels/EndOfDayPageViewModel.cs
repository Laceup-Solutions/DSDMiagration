using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EndOfDayPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private bool _showExpenses;

        public EndOfDayPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            ShowExpenses = Config.ShowExpensesInEOD;
        }

        [RelayCommand]
        private async Task RouteReturns()
        {
            await Shell.Current.GoToAsync("routereturns");
        }

        [RelayCommand]
        private async Task EndInventory()
        {
            await Shell.Current.GoToAsync("endinventory");
        }

        [RelayCommand]
        private async Task CycleCount()
        {
            await Shell.Current.GoToAsync("cyclecount");
        }

        [RelayCommand]
        private async Task PrintReports()
        {
            await Shell.Current.GoToAsync("printreports");
        }

        [RelayCommand]
        private async Task EndOfDay()
        {
            await Shell.Current.GoToAsync("endofdayprocess");
        }

        [RelayCommand]
        private async Task LoadOrder()
        {
            try
            {
                // Create or get load order for salesman
                var client = Client.Clients.FirstOrDefault(x => x.ClientName.StartsWith("Salesman: "));
                if (client == null)
                    client = Client.CreateSalesmanClient();

                Order order = Order.Orders.FirstOrDefault(x => x.OrderType == OrderType.Load && !x.PendingLoad);
                if (order == null)
                {
                    order = new Order(client) { OrderType = OrderType.Load };
                    order.SalesmanId = 0;
                }

                order.Save();

                var query = new Dictionary<string, object> { { "orderId", order.OrderId } };
                await Shell.Current.GoToAsync("newloadordertemplate", query);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error creating load order: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task SetParLevel()
        {
            await Shell.Current.GoToAsync("setparlevel");
        }

        [RelayCommand]
        private async Task ClockOut()
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Clock Out",
                "Clock out for the day?",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            try
            {
                // Close salesman session
                SalesmanSession.CloseSession();
                
                await _dialogService.ShowAlertAsync("Clocked out successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error clocking out: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task RouteExpenses()
        {
            await Shell.Current.GoToAsync("routeexpenses");
        }
    }
}
