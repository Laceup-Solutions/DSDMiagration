using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using System.Collections.ObjectModel;

namespace LaceupMigration.ViewModels
{
    public partial class LoginConfigPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        [ObservableProperty] private string _field1 = string.Empty;
        [ObservableProperty] private string _field1Placeholder = string.Empty;
        [ObservableProperty] private string _field2 = string.Empty;
        [ObservableProperty] private string _field2Placeholder = string.Empty;
        [ObservableProperty] private string _truck = string.Empty;
        [ObservableProperty] private string _truckPlaceholder = string.Empty;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private bool _loginWithTruck;
        [ObservableProperty] private bool _field2Visible;
        [ObservableProperty] private ObservableCollection<SalesmanTruckDTO> _truckSuggestions = new();
        [ObservableProperty] private ObservableCollection<SalesmanTruckDTO> _filteredTruckSuggestions = new();
        [ObservableProperty] private bool _showSuggestions = false;

        public SalesmanTruckDTO? SelectedTruckSuggestion
        {
            get => _selectedTruckSuggestion;
            set
            {
                if (SetProperty(ref _selectedTruckSuggestion, value) && value != null)
                {
                    Truck = value.Name.ToString();
                }
            }
        }
        private SalesmanTruckDTO? _selectedTruckSuggestion;

        public LoginConfigPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;

            LoginWithTruck = false;
            Field1Placeholder = "Salesman ID";

            if (Config.LoginType == LoginType.RouteTruck)
            {
                DataProvider.Instance();
                LoadSalesmanSuggestions();
            }

            UpdateFieldsByLoginType();

            if (!Config.ApplicationIsInDemoMode)
            {
                try
                {
                    if (Config.EnableSelfServiceModule && Config.SignedInSelfService)
                        MainThread.BeginInvokeOnMainThread(() => ContinueSignInAsync());
                }
                catch
                {
                }
            }
        }

        public void OnAppearing()
        {
            if (Config.SignedIn)
            {
                MainThread.BeginInvokeOnMainThread(async () => await GoToAsyncOrMainAsync("///MainPage"));
            }
        }

        public void FilterSuggestions(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                FilteredTruckSuggestions.Clear();
                ShowSuggestions = false;
                return;
            }

            var filtered = TruckSuggestions
                .Where(s => s.Name.ToString().StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Name)
                .Take(10) // Limit to 10 suggestions for better performance
                .ToList();

            FilteredTruckSuggestions.Clear();
            foreach (var item in filtered)
            {
                FilteredTruckSuggestions.Add(item);
            }

            ShowSuggestions = FilteredTruckSuggestions.Count > 0;
        }

        public void SelectSuggestion(SalesmanTruckDTO item)
        {
            SelectedTruckSuggestion = item;
            Truck = item.Name.ToString();
            ShowSuggestions = false;
            FilteredTruckSuggestions.Clear();
        }

        [RelayCommand]
        private async Task SupportContact()
        {
            try
            {
                var uri = new Uri("tel:17864374380");
                await Launcher.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await DialogHelper._dialogService.ShowAlertAsync("Error trying to call Laceup Support. Please make sure your device can make calls.", "Alert", "OK");
            }
        }

        [RelayCommand]
        private async Task NeedHelp()
        {
            var options = new[] { "Send log file", "Export data", "Remote control" };
            var choice = await DialogHelper._dialogService.ShowActionSheetAsync("Advanced options", "Cancel", null, options);
            switch (choice)
            {
                case "Send log file":
                    TryRun(SendLog);
                    break;
                case "Export data":
                    TryRun(ExportData);
                    break;
                case "Remote control":
                    await RemoteControl();
                    break;
            }
        }

        [RelayCommand]
        private async Task Configuration()
        {
            try
            {
                // Show dialog and wait for result - only proceed if Save was clicked
                var saved = await _dialogService.ShowConfigDialogInLoginAsync();
                
                // Only execute these functions if Save was clicked
                if (!saved)
                    return;

                // Small delay to ensure the page is fully available after modal closes (Android timing issue)
                await Task.Delay(100);

                GetFieldsToLogin();

                // Ensure we're on the main thread before updating UI properties
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        // Verify the page is still available before updating
                        var currentPage = Shell.Current?.CurrentPage;
                        if (currentPage == null)
                            return;

                        UpdateFieldsByLoginType();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        void GetFieldsToLogin()
        {
            DataProvider.Instance();
            DataProvider.GetLoginType();

            if(Config.LoginType == LoginType.RouteTruck)
                LoadSalesmanSuggestions();
        }

        private void UpdateFieldsByLoginType()
        {
            try
            {
                Field2Visible = LoginWithTruck = false;

                switch (Config.LoginType)
                {
                    case LoginType.SalesmanId:
                        Field1Placeholder = "Salesman ID";
                        break;
                    case LoginType.UsernamePassword:
                        Field1Placeholder = "Username";
                        Field2Placeholder = "Password";
                        Field2Visible = true;
                        break;
                    case LoginType.RouteNumber:
                        Field1Placeholder = "Route Number";
                        break;
                    case LoginType.RouteTruck:
                        Field1Placeholder = "Route Number";
                        TruckPlaceholder = "Truck";
                        LoginWithTruck = true;

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private void LoadSalesmanSuggestions()
        {
            try
            {
                var trucks = DataProvider.GetSalesmanTrucks().OrderBy(x => x.Name).ToList();

                TruckSuggestions.Clear();
                foreach (var item in trucks)
                    TruckSuggestions.Add(item);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        [RelayCommand]
        private async Task SignIn()
        {
            var saved = await SaveConfigurationAsync();

            if (saved)
                await ContinueSignInAsync();
        }

        async Task ContinueSignInAsync()
        {
            await _dialogService.ShowLoadingAsync("Loading...");

            string responseMessage = string.Empty;
            bool errorDownloadingData = false;
            Order selfServiceOrd = null;

            try
            {
                await Task.Run(() =>
                {

                    try
                    {
                        DataProvider.Initialize();

                        DataProvider.CheckAuthorization();

                        if(Config.AuthorizationFailed)
                        {
                            errorDownloadingData = true;
                            responseMessage = "Not authorized";
                            return;
                        }

                        DataProvider.GetUserSettingLine();
                        DataProvider.GetSalesmanSettings(false);

                        if (Config.EnableSelfServiceModule)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await _dialogService.ShowLoadingAsync("Downloading data...");
                            });

                            DataProvider.DownloadStaticData();

                            if(Client.Clients.Count == 0)
                            {
                                errorDownloadingData = true;
                                responseMessage = "Error downloading data.";
                                return;
                            }

                            var company = CompanyInfo.GetMasterCompany();

                            string companyName = string.Empty;
                            string companyAddress = string.Empty;
                            string companyPhone = string.Empty;
                            if (company != null)
                            {
                                companyName = company.CompanyName;
                                companyAddress = company.CompanyAddress1;
                                companyPhone = company.CompanyPhone;
                            }

                            //create self service company
                            var ssc = new SelfServiceCompany
                            {
                                UserId = Config.SalesmanId,
                                PortId = Config.Port,
                                ServerId = Config.IPAddressGateway,
                                CompanyName = companyName ?? string.Empty,
                                CompanyAddress = companyAddress ?? string.Empty,
                                CompanyPhone = companyPhone ?? string.Empty
                            };

                            if (!SelfServiceCompany.Find(ssc))
                                SelfServiceCompany.Add(ssc);

                            if(Client.Clients.Count == 1)
                            {
                                var client = Client.Clients.FirstOrDefault();

                                client.EnsureInvoicesAreLoaded();
                                client.EnsurePreviouslyOrdered();

                                selfServiceOrd = Order.Orders.FirstOrDefault(x => x.Client.ClientId == client.ClientId);

                                if (selfServiceOrd == null)
                                {
                                    var batch = new Batch(client);
                                    batch.Client = client;
                                    batch.ClockedIn = DateTime.Now;
                                    batch.Save();

                                    var companies = SalesmanAvailableCompany.GetCompanies(Config.SalesmanId, client.ClientId);

                                    selfServiceOrd = new Order(client) { AsPresale = true, OrderType = OrderType.Order, SalesmanId = Config.SalesmanId, BatchId = batch.Id };

                                    if (companies.Count > 0)
                                    {
                                        selfServiceOrd.CompanyName = companies[0].CompanyName;
                                        selfServiceOrd.CompanyId = companies[0].CompanyId;
                                    }

                                    selfServiceOrd.Save();
                                }

                                UpdateOrderPrices();
                            }
                        }

                        Config.SignedIn = !Config.EnableSelfServiceModule;
                        Config.LastSignIn = DateTime.Now.Ticks.ToString();
                        Config.SaveSettings();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);

                        errorDownloadingData = true;
                        responseMessage = "Error processing request.";
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }

            var title = errorDownloadingData ? "Warning" : "Info";

            if (string.IsNullOrEmpty(responseMessage))
                responseMessage = "Data downloaded.";

            await _dialogService.ShowAlertAsync(responseMessage, title, "OK");

            if (errorDownloadingData)
                return;

            if(Config.EnableSelfServiceModule)
            {
                if (Client.Clients.Count > 1)
                {
                    await GoToAsyncOrMainAsync("selfservice/clientlist");
                }
                else if(Client.Clients.Count == 1 && selfServiceOrd != null)
                {
                    var route = selfServiceOrd.Details.Count == 0 ? "//selfservice/template" : "//selfservice/checkout";
                    await GoToAsyncOrMainAsync($"{route}?orderId={selfServiceOrd.OrderId}");
                }

                return;
            }

            if (Config.RequestAuthPinForLogin)
            {
                Config.ShouldGetPinBeforeSync = true;
                Config.SaveSettings();
            }

            await GoToAsyncOrMainAsync("///MainPage?shouldSyncData=true");
        }

        private static void UpdateOrderPrices()
        {
            foreach (var order in Order.Orders)
            {
                foreach (var item in order.Details)
                {
                    var expectedPrice = Product.GetPriceForProduct(item.Product, order, false, false);
                    item.ExpectedPrice = expectedPrice;
                    double price;
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

        private async Task<bool> SaveConfigurationAsync()
        {
            string result = string.Empty;

            switch (Config.LoginType)
            {
                case LoginType.SalesmanId:
                    if (string.IsNullOrEmpty(Field1))
                    {
                        await DialogHelper._dialogService.ShowAlertAsync("Please enter Salesman ID.", "Alert", "OK");
                        return false;
                    }
                    result = DataProvider.GetSalesmanIdFromLogin(Field1, "");
                    Config.RouteName = "";
                    break;
                case LoginType.UsernamePassword:
                    if (string.IsNullOrEmpty(Field1) || string.IsNullOrEmpty(Field2))
                    {
                        await DialogHelper._dialogService.ShowAlertAsync("Please enter a valid username and password.", "Alert", "OK");
                        return false;
                    }
                    result = DataProvider.GetSalesmanIdFromLogin(Field1, Field2);
                    Config.RouteName = "";
                    break;
                case LoginType.RouteNumber:
                    if (string.IsNullOrEmpty(Field1))
                    {
                        await DialogHelper._dialogService.ShowAlertAsync("Please enter the route number.", "Alert", "OK");
                        return false;
                    }
                    result = DataProvider.GetSalesmanIdFromLogin(Field1, "");
                    Config.RouteName = Field1;
                    break;
                case LoginType.RouteTruck:
                    if (string.IsNullOrEmpty(Field1) || string.IsNullOrEmpty(Truck) || SelectedTruckSuggestion == null)
                    {
                        await DialogHelper._dialogService.ShowAlertAsync("Please enter a valid route number and truck.", "Alert", "OK");
                        return false;
                    }

                    result = DataProvider.GetSalesmanIdFromLogin(Field1, SelectedTruckSuggestion.Id.ToString());
                    Config.RouteName = Field1;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(result))
            {
                await DialogHelper._dialogService.ShowAlertAsync(result, "Alert", "OK");
                return false;
            }

            Preferences.Set("VendorId", Config.SalesmanId);
            Preferences.Get("RouteNameKey", Config.RouteName);

            return true;
        }

        private static async Task GoToAsyncOrMainAsync(string route)
        {
            try
            {
                await Shell.Current.GoToAsync(route);
            }
            catch
            {
                await Shell.Current.GoToAsync("///MainPage");
            }
        }

        private static void TryRun(Action action)
        {
            try { action(); } catch (Exception ex) { Logger.CreateLog(ex); }
        }

        private static void SendLog() => LaceupActivityHelper.SendLog();
        
        private static void ExportData() => LaceupActivityHelper.ExportData();
        
        private static async Task RemoteControl() => await LaceupActivityHelper.RemoteControl();

    }
}

