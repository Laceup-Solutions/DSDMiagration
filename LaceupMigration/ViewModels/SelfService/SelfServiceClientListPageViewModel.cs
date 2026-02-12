using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using LaceupMigration.Services;
using LaceupMigration;
using LaceupMigration.ViewModels;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceClientListPageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly MainPageViewModel _mainPageViewModel;

        [ObservableProperty]
        private ObservableCollection<ClientItemViewModel> _clients = new();

        [ObservableProperty]
        private ObservableCollection<ClientItemViewModel> _filteredClients = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _companyName = "Laceup";

        [ObservableProperty]
        private bool _showAddress;

        public string GetFormattedAddress(Client client)
        {
            if (client == null || string.IsNullOrEmpty(client.ShipToAddress))
                return string.Empty;
            return client.ShipToAddress.Replace("|", " ");
        }

        /// <summary>Same behavior as ClientsPage.HandleClientSelectionAsync: overdue alert then navigate.</summary>
        public async Task HandleClientSelectionAsync(ClientItemViewModel item)
        {
            if (item?.Client == null) return;
            _appService.RecordEvent("SelfServiceClientList.CustomerSelected");
            if (Config.CannotOrderWithUnpaidInvoices && item.HasOverdueInvoices)
            {
                await _dialogService.ShowAlertAsync("Customers highlighted in red are 90 days late on their payments. Please contact the customer prior to visiting them.", "Alert");
            }
            await SelectClientAsync(item);
        }

        public SelfServiceClientListPageViewModel(IDialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService, MainPageViewModel mainPageViewModel)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            _mainPageViewModel = mainPageViewModel;
            _showAddress = Config.ShowAddrInClientList;
            LoadClients();
        }

        /// <summary>Top right toolbar menu. Sync Data and Sign Out only when 1 client assigned; Advanced Options always.</summary>
        [RelayCommand]
        private async Task ShowToolbarMenuAsync()
        {
            var options = new List<string> {  "Sync Data From Server", "Advanced Options","Sign Out" };
            var choice = await _dialogService.ShowActionSheetAsync("Menu", null, "Cancel", options.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
            if (choice == "Sync Data From Server")
                await _mainPageViewModel.SyncDataFromMenuAsync();
            else if (choice == "Advanced Options")
                await _advancedOptionsService.ShowAdvancedOptionsAsync();
            else if (choice == "Sign Out")
                await _mainPageViewModel.SignOutFromSelfServiceAsync();
        }

        public void OnAppearing()
        {
            UpdateCompanyName();
            UpdateOrderPrices();
        }

        private void UpdateCompanyName()
        {
            var company = CompanyInfo.GetMasterCompany();
            CompanyName = company?.CompanyName ?? "Laceup";
        }

        private void LoadClients()
        {
            Clients.Clear();
            var clientList = Client.SortedClients().Where(x => !x.SalesmanClient).ToList();
            foreach (var client in clientList)
            {
                Clients.Add(new ClientItemViewModel(client, Config.ShowAddrInClientList));
            }
            FilterClients();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterClients();
        }

        private void FilterClients()
        {
            FilteredClients.Clear();
            var searchLower = SearchText?.ToLowerInvariant() ?? string.Empty;
            var filtered = string.IsNullOrEmpty(searchLower)
                ? Clients
                : Clients.Where(x => x.ClientName.ToLowerInvariant().Contains(searchLower));

            foreach (var client in filtered)
            {
                FilteredClients.Add(client);
            }
        }

        public async Task SelectClientAsync(ClientItemViewModel clientItem)
        {
            var client = clientItem.Client;
            client.EnsureInvoicesAreLoaded();
            client.EnsurePreviouslyOrdered();

            var order = Order.Orders.FirstOrDefault(x => x.Client.ClientId == client.ClientId);

            if (order == null)
            {
                var batch = new Batch(client) { Client = client, ClockedIn = DateTime.Now };
                batch.Save();

                var companies = SalesmanAvailableCompany.GetCompanies(Config.SalesmanId, client.ClientId);
                order = new Order(client) { AsPresale = true, OrderType = OrderType.Order, SalesmanId = Config.SalesmanId, BatchId = batch.Id };

                if (companies.Count > 0)
                {
                    order.CompanyName = companies[0].CompanyName;
                    order.CompanyId = companies[0].CompanyId;
                }

                order.Save();

                // When only one client, replace stack so back doesn't go to client list
                var route = $"{(Client.Clients.Count == 1 ? "//" : "")}selfservice/checkout?orderId={order.OrderId}";
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                var route = $"{(Client.Clients.Count == 1 ? "//" : "")}selfservice/checkout?orderId={order.OrderId}";
                await Shell.Current.GoToAsync(route);
            }
        }

        private void UpdateOrderPrices()
        {
            foreach (var order in Order.Orders)
            {
                foreach (var item in order.Details)
                {
                    double expectedPrice = Product.GetPriceForProduct(item.Product, order, false, false);
                    item.ExpectedPrice = expectedPrice;

                    double price = 0;

                    if (Offer.ProductHasSpecialPriceForClient(item.Product, order.Client, out price))
                    {
                        item.Price = price;
                        item.FromOfferPrice = true;
                    }
                    else
                    {
                        item.Price = item.ExpectedPrice;
                        item.FromOfferPrice = false;
                    }

                    if (item.UnitOfMeasure != null)
                    {
                        item.ExpectedPrice *= item.UnitOfMeasure.Conversion;
                        item.Price *= item.UnitOfMeasure.Conversion;
                    }
                }

                order.RecalculateDiscounts();
                order.Save();
            }
        }
    }

    /// <summary>Same bindable shape as ClientListItemViewModel for ClientsPage row template.</summary>
    public partial class ClientItemViewModel : ObservableObject
    {
        public Client Client { get; }
        public string ClientName => Client?.ClientName ?? string.Empty;
        public string ShipToAddress => Client?.ShipToAddress?.Replace("|", " ") ?? string.Empty;

        public string Name => Client?.ClientName ?? string.Empty;
        public string Address => Client?.ShipToAddress?.Replace("|", " ") ?? string.Empty;
        public bool ShowAddress { get; }
        public string Balance => Client?.ClientBalanceInDevice.ToCustomString() ?? 0.0.ToCustomString();
        public bool HasLeftContent => false;
        public bool ShowStop => false;
        public bool ShowIcon => false;
        public string StopText => string.Empty;
        public Color NameColor { get; }
        public Color AddressColor { get; } = Color.FromArgb("#4A4A4A");
        public Color StopBadgeBackground => Color.FromArgb("#CFD8DC");
        public Color StopBadgeTextColor => Colors.White;
        public ImageSource IconImageSource => null;
        public bool HasOverdueInvoices { get; }

        public ClientItemViewModel(Client client, bool showAddress)
        {
            Client = client;
            ShowAddress = showAddress && !string.IsNullOrEmpty(Address);
            bool hasOverdue = false;
            if (Config.CannotOrderWithUnpaidInvoices && client != null)
            {
                var invoices = (Invoice.OpenInvoices ?? new List<Invoice>())
                    .Where(x => x.Client.ClientId == client.ClientId)
                    .ToList();
                if (invoices.Count > 0)
                    hasOverdue = invoices.Any(x => x.DueDate.AddDays(90) < DateTime.Now.Date && x.Balance > 0);
            }
            HasOverdueInvoices = hasOverdue;
            NameColor = hasOverdue ? Colors.Red : Colors.Black;
        }
    }
}

