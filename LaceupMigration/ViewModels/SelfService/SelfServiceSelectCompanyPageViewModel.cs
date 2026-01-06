using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceSelectCompanyPageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty]
        private ObservableCollection<SelfServiceCompany> _companies = new();

        public SelfServiceSelectCompanyPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            LoadCompanies();
        }

        private void LoadCompanies()
        {
            SelfServiceCompany.Load();
            Companies.Clear();
            foreach (var company in SelfServiceCompany.List)
            {
                Companies.Add(company);
            }
        }

        [RelayCommand]
        private async Task AddCompany()
        {
            var serverAddress = await _dialogService.ShowPromptAsync("Login into New Company", "Server Address", "OK", "Cancel", "");
            if (string.IsNullOrEmpty(serverAddress) || serverAddress == "Cancel")
                return;

            var portString = await _dialogService.ShowPromptAsync("Login into New Company", "Port ID", "OK", "Cancel", "");
            if (string.IsNullOrEmpty(portString) || portString == "Cancel")
                return;

            var userIdString = await _dialogService.ShowPromptAsync("Login into New Company", "User ID", "OK", "Cancel", "");
            if (string.IsNullOrEmpty(userIdString) || userIdString == "Cancel")
                return;

            if (!int.TryParse(portString, out var port) || !int.TryParse(userIdString, out var userId) || port <= 0 || userId <= 0)
            {
                await _dialogService.ShowAlertAsync("The Company Id or Salesman Id is invalid", "Alert", "OK");
                return;
            }

            await ContinueToLoginAsync(serverAddress.Trim(), port, userId);
        }

        public async Task SelectCompanyAsync(SelfServiceCompany company)
        {
            var result = await _dialogService.ShowConfirmationAsync($"Are you sure you want to login with {company.CompanyName}?", "Confirm", "Yes", "No");
            if (result)
            {
                await ContinueToLoginAsync(company.ServerId, company.PortId, company.UserId);
            }
        }

        private async Task ContinueToLoginAsync(string server, int port, int userId)
        {
            Config.SelfService = true;
            Config.SignedInSelfService = true;

            await LoginAsync(server, port, userId);
        }

        private async Task LoginAsync(string server, int port, int userId)
        {
            try
            {
                if (Config.EnableSelfServiceModule)
                {
                    if (!string.IsNullOrEmpty(server) && port > 0 && userId > 0)
                    {
                        await SaveConfigurationAsync(server, port, userId);
                    }

                    await ContinueSignInAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Please check your configuration", "Alert", "OK");
                Logger.CreateLog($"Error trying to autologin self service ==> {ex}");
            }
        }

        private async Task<bool> SaveConfigurationAsync(string serverAdd, int portId, int userId)
        {
            if (string.IsNullOrEmpty(serverAdd))
                return false;

            Config.IPAddressGateway = serverAdd;

            try
            {
                Config.Port = portId;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return false;
            }

            var serverPortConfig = await ServerHelper.GetIdForServer(Config.IPAddressGateway, Config.Port);
            Config.IPAddressGateway = serverPortConfig.Item1;
            Config.Port = serverPortConfig.Item2;

            try
            {
                Config.SalesmanId = userId;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return false;
            }

            return true;
        }

        private async Task ContinueSignInAsync()
        {
            int error = 0;

            try
            {
                DataAccess.GetUserSettingLine();
                DataAccess.CheckAuthorization();
                error = 1;

                if (!Config.AuthorizationFailed)
                {
                    DataAccess.GetSalesmanSettings(false);
                    error = 2;

                    if (Config.EnableSelfServiceModule)
                    {
                        DataAccess.DownloadStaticData();
                    }

                    error = 3;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

            if (error == 0)
            {
                await _dialogService.ShowAlertAsync("Connection error.", "Alert", "OK");
                return;
            }

            if (Config.AuthorizationFailed)
            {
                await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
                return;
            }

            if (error < 3)
            {
                await _dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
                return;
            }

            Config.SignedIn = !Config.EnableSelfServiceModule;
            Config.SaveSettings();

            if (Config.EnableSelfServiceModule)
            {
                try
                {
                    if (Client.Clients.Count == 0)
                    {
                        await _dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
                        return;
                    }

                    var company = CompanyInfo.GetMasterCompany();
                    var ssc = new SelfServiceCompany
                    {
                        UserId = Config.SalesmanId,
                        PortId = Config.Port,
                        ServerId = Config.IPAddressGateway,
                        CompanyName = company?.CompanyName ?? string.Empty,
                        CompanyAddress = company?.CompanyAddress1 ?? string.Empty,
                        CompanyPhone = company?.CompanyPhone ?? string.Empty
                    };

                    if (!SelfServiceCompany.Find(ssc))
                        SelfServiceCompany.Add(ssc);

                    if (Client.Clients.Count > 1)
                    {
                        await Shell.Current.GoToAsync("selfservice/clientlist");
                    }
                    else
                    {
                        var client = Client.Clients.First();
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
                        }

                        UpdateOrderPrices();

                        var route = order.Details.Count == 0 ? "//selfservice/template" : "//selfservice/checkout";
                        await Shell.Current.GoToAsync($"{route}?orderId={order.OrderId}");
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
                    Logger.CreateLog(ex);
                }
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
}

