using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceClientListPageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;

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

        public SelfServiceClientListPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _showAddress = Config.ShowAddrInClientList;
            LoadClients();
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
                Clients.Add(new ClientItemViewModel(client, ShowAddress));
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

                await Shell.Current.GoToAsync($"selfservice/template?orderId={order.OrderId}");
            }
            else
            {
                await Shell.Current.GoToAsync($"selfservice/checkout?orderId={order.OrderId}");
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

    public partial class ClientItemViewModel : ObservableObject
    {
        public Client Client { get; }
        public string ClientName => Client?.ClientName ?? string.Empty;
        public string ShipToAddress => Client?.ShipToAddress?.Replace("|", " ") ?? string.Empty;
        public bool ShowAddress { get; }

        public ClientItemViewModel(Client client, bool showAddress)
        {
            Client = client;
            ShowAddress = showAddress;
        }
    }
}

