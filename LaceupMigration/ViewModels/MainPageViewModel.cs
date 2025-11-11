using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System.Linq;

namespace LaceupMigration.ViewModels
{
	public partial class MainPageViewModel : ObservableObject
	{
		private readonly IDialogService _dialogService;
		private readonly ILaceupAppService _appService;

		[ObservableProperty] private string _companyName = "Laceup";
		[ObservableProperty] private bool _showNotificationIcon;
		[ObservableProperty] private bool _showAcceptLoadMenuItem;

		public MainPageViewModel(IDialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
		}

		public void OnAppearing()
		{
			UpdateCompanyName();
			RefreshMenuVisibility();
		}

		private void UpdateCompanyName()
		{
			var company = CompanyInfo.GetMasterCompany();
			CompanyName = company?.CompanyName ?? "Laceup";
		}

		private void RefreshMenuVisibility()
		{
			ShowNotificationIcon = Config.DidCloseAlert;
			ShowAcceptLoadMenuItem = DataAccess.ReceivedData && (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand) && !Config.OnlyPresale;
		}

		#region Menu Commands

		[RelayCommand]
		private async Task SyncData()
		{
			_appService.RecordEvent("mainMenuSyncData menu");
			
			if (DataAccess.MustEndOfDay())
			{
				await _dialogService.ShowAlertAsync("Do end of day.", "Warning", "OK");
				return;
			}

			await MenuHandlerSyncDataAsync();
		}

		[RelayCommand]
		private async Task AddClient()
		{
			_appService.RecordEvent("mainMenuAddClient menu");
			
			if (!DataAccess.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			// Navigate to AddClient page when created
			await Shell.Current.GoToAsync("//addclient");
		}

		[RelayCommand]
		private async Task ClockOut()
		{
			_appService.RecordEvent("mainMenuClockOut menu");
			await ClockOutHandlerAsync();
		}

		[RelayCommand]
		private async Task Notification()
		{
			_appService.RecordEvent("mainMenuSyncData menu");
			await _dialogService.ShowAlertAsync("There is Newer Information. You should Sync before continuing working.", "Alert", "OK");
		}

		[RelayCommand]
		private async Task Reports()
		{
			if (!DataAccess.ReceivedData)
			{
				await _dialogService.ShowAlertAsync("Must sync data.", "Warning", "OK");
				return;
			}

			if (Config.AuthorizationFailed)
			{
				await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
				return;
			}

			if (Config.MustCompleteRoute && AnyRouteNotClose())
			{
				var message = Config.ButlerCustomization 
					? $"Not completed your route.\n{GetOpenRoutes()}" 
					: "Not completed your route.";
				await _dialogService.ShowAlertAsync(message, "Warning", "OK");
				return;
			}

			if (Config.ExistPendingTransfer)
			{
				await _dialogService.ShowAlertAsync("Pending transfers must be submitted.", "Alert", "OK");
				
				await Shell.Current.GoToAsync("//transfer");
				
				return;
			}

			_appService.RecordEvent("mainMenuReports menu");
			await MenuHandlerReportsAsync();
		}

		[RelayCommand]
		private async Task SendAll()
		{
			_appService.RecordEvent("mainMenuSendAll menu");
			
			if (Config.AuthorizationFailed)
			{
				await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
				return;
			}

			await MenuHandlerSendAllAsync();
		}

		[RelayCommand]
		private async Task SentOrders()
		{
			_appService.RecordEvent("mainMenuSentOders menu");
			
			if (Config.AuthorizationFailed)
			{
				await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
				return;
			}

			await Shell.Current.GoToAsync("//sentorders");
		}

		[RelayCommand]
		private async Task SentPayments()
		{
			_appService.RecordEvent("mainMenuSentPayments menu");
			
			if (Config.AuthorizationFailed)
			{
				await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
				return;
			}

			await Shell.Current.GoToAsync("//sentpayments");
		}

		[RelayCommand]
		private async Task ProductCatalog()
		{
			_appService.RecordEvent("mainMenuProductCatalog menu");
			
			if (!DataAccess.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			await Shell.Current.GoToAsync("//productcatalog");
		}

		[RelayCommand]
		private async Task ViewOrderStatus()
		{
			_appService.RecordEvent("mainMenuViewOrderStatus menu");
			
			if (!DataAccess.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			await Shell.Current.GoToAsync("//vieworderstatus");
		}

		[RelayCommand]
		private async Task Inventory()
		{
			if (!DataAccess.CanUseApplication() || !DataAccess.ReceivedData)
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			_appService.RecordEvent("mainMenuInventory menu");
			await Shell.Current.GoToAsync("//inventory");
		}

		[RelayCommand]
		private async Task Configuration()
		{
			_appService.RecordEvent("mainMenuConfiguration menu");
			await MenuHandlerConfigurationAsync();
		}

		[RelayCommand]
		private async Task UpdateProductImages()
		{
			_appService.RecordEvent("mainMenUpdateProductsImages menu");
			await MenuHandlerUpdateProductImagesAsync();
		}

		[RelayCommand]
		private async Task About()
		{
			_appService.RecordEvent("mainMenuAbout menu");
			var uri = new Uri("http://www.laceupsolutions.com");
			await Launcher.OpenAsync(uri);
		}

		[RelayCommand]
		private async Task ShowReports()
		{
			_appService.RecordEvent("mainMenuShowReports menu");
			await Shell.Current.GoToAsync("//reports");
		}

		[RelayCommand]
		private async Task AcceptLoad()
		{
			_appService.RecordEvent("mainMenuAcceptLoad menu");
			
			if (!DataAccess.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			await AcceptLoadAsync();
		}

		[RelayCommand]
		private async Task TimeSheet()
		{
			await Shell.Current.GoToAsync("//timesheet");
		}

		[RelayCommand]
		private async Task SignOut()
		{
			_appService.RecordEvent("mainMenuSignOut menu");
			await MenuHandlerSignOutAsync();
		}

		[RelayCommand]
		private async Task RouteManagement()
		{
			await Shell.Current.GoToAsync("//routemanagement");
		}

		[RelayCommand]
		private async Task Goals()
		{
			await Shell.Current.GoToAsync("//goals");
		}

		[RelayCommand]
		private async Task SelectSite()
		{
			await SelectSiteHandlerAsync();
		}

		[RelayCommand]
		private async Task ShowMenu()
		{
			var menuItems = new List<string>();
			
			if (Config.CanAddClient)
				menuItems.Add("Add Client");
			menuItems.Add("Sync Data");
			if (Config.UseClockInOut)
				menuItems.Add("Clock Out");
			if (Config.TrackInventory && !Config.HideProdOnHand)
				menuItems.Add("Inventory");
			if (Config.TrackInventory)
				menuItems.Add("Reports");
			if (!Config.TrackInventory || Config.PreSale)
				menuItems.Add("Send All");
			if (Config.ProductCatalog)
				menuItems.Add("Product Catalog");
			if (!Config.HidePriceInTransaction)
				menuItems.Add("Sent Payments");
			if (Config.ShowOrderStatus)
				menuItems.Add("View Order Status");
			if (Config.ShowReports)
				menuItems.Add("Show Reports");
			if (Config.TimeSheetCustomization)
				menuItems.Add("Time Sheet");
			if (Config.RouteManagement)
				menuItems.Add("Route Management");
			if (Config.ViewGoals)
				menuItems.Add("Goals");
			if (Config.SalesmanCanChangeSite && !Config.HideSelectSitesFromMenu)
				menuItems.Add("Select Site");
			if (Config.CanLogout && !Config.NeedAccessForConfiguration)
				menuItems.Add("Sign Out");
			menuItems.Add("Advanced Options");
			menuItems.Add("Configuration");
			menuItems.Add("About");

			var choice = await Application.Current!.MainPage!.DisplayActionSheet("Menu", "Cancel", null, menuItems.ToArray());
			
			switch (choice)
			{
				case "Add Client":
					await AddClient();
					break;
				case "Sync Data":
					await SyncData();
					break;
				case "Clock Out":
					await ClockOut();
					break;
				case "Inventory":
					await Inventory();
					break;
				case "Reports":
					await Reports();
					break;
				case "Send All":
					await SendAll();
					break;
				case "Product Catalog":
					await ProductCatalog();
					break;
				case "Sent Payments":
					await SentPayments();
					break;
				case "View Order Status":
					await ViewOrderStatus();
					break;
				case "Show Reports":
					await ShowReports();
					break;
				case "Time Sheet":
					await TimeSheet();
					break;
				case "Route Management":
					await RouteManagement();
					break;
				case "Goals":
					await Goals();
					break;
				case "Select Site":
					await SelectSite();
					break;
				case "Sign Out":
					await SignOut();
					break;
				case "Advanced Options":
					await AdvancedLog();
					break;
				case "Configuration":
					await Configuration();
					break;
				case "About":
					await About();
					break;
			}
		}

		[RelayCommand]
		private async Task AdvancedLog()
		{
			var options = new[] { "Update settings", "Send log file", "Export data", "Remote control", "Setup printer" };
			if (Config.GoToMain)
			{
				var list = options.ToList();
				list.Add("Go to main activity");
				options = list.ToArray();
			}

			var choice = await Application.Current!.MainPage!.DisplayActionSheet("Advanced options", "Cancel", null, options);
			
			switch (choice)
			{
				case "Update settings":
					await _appService.UpdateSalesmanSettingsAsync();
					await _dialogService.ShowAlertAsync("Settings updated.", "Info", "OK");
					break;
				case "Send log file":
					await _appService.SendLogAsync();
					await _dialogService.ShowAlertAsync("Log sent.", "Info", "OK");
					break;
				case "Export data":
					await _appService.ExportDataAsync();
					await _dialogService.ShowAlertAsync("Data exported.", "Info", "OK");
					break;
				case "Remote control":
					await _appService.RemoteControlAsync();
					break;
				case "Setup printer":
					// TODO: Implement printer setup
					break;
				case "Go to main activity":
					await _appService.GoBackToMainAsync();
					break;
			}
		}

		#endregion

		#region Private Handlers

		private async Task MenuHandlerSyncDataAsync()
		{
			if (Config.ShouldGetPinBeforeSync)
			{
				await MenuHandlerConfigurationAsync(true);
				return;
			}

			var orderCount = Order.Orders.Count(x => x.OrderType == OrderType.NoService || x.Details.Count() > 0) + InvoicePayment.List.Count;
			var inventoryModified = ProductInventory.CurrentInventories.Values.Any(x => x.InventoryChanged);
			if (inventoryModified && Config.UpdateInventoryRegardless)
				inventoryModified = false;

			if (Config.TrackInventory && inventoryModified)
			{
				var result = await _dialogService.ShowConfirmationAsync("Inventory changes detected. Continue?", "Warning", "Yes", "No");
				if (result)
					await DownloadDataAsync(!inventoryModified);
				return;
			}

			if (Config.MasterDevice || Config.SupervisorId > 0)
			{
				// Handle master device/supervisor sync selection
				await DownloadDataAsync(!inventoryModified);
				return;
			}

			await DownloadDataAsync(!inventoryModified);
		}

		public async Task DownloadDataAsync(bool updateInventory, int oldScanner = 0)
		{
			await _dialogService.ShowLoadingAsync("Downloading data...");
			string responseMessage = null;
			bool errorDownloadingData = false;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						using (var access = new NetAccess())
						{
							access.OpenConnection();
							access.CloseConnection();
						}

						DataAccess.CheckAuthorization();
						if (Config.AuthorizationFailed)
							throw new Exception("Not authorized");

						if (!DataAccess.CheckSyncAuthInfo())
							throw new Exception("Wait before sync");

						responseMessage = DataAccessEx.DownloadData(true, !Config.TrackInventory || updateInventory);
					}
					catch (Exception ee)
					{
						errorDownloadingData = true;
						Logger.CreateLog(ee);
						responseMessage = ee.Message.Replace("(305)-381-1123", "(786) 437-4380");
					}
				});
			}
			finally
			{
				await _dialogService.HideLoadingAsync();
			}

			var title = errorDownloadingData ? "Warning" : "Info";
			if (string.IsNullOrEmpty(responseMessage))
				responseMessage = "Data downloaded.";

			await _dialogService.ShowAlertAsync(responseMessage, title, "OK");

			RefreshMenuVisibility();
			UpdateCompanyName();
		}

		private async Task ClockOutHandlerAsync()
		{
			var result = await _dialogService.ShowConfirmationAsync("Sure like to clock out?", "Question", "Yes", "No");
			if (result)
			{
				SalesmanSession.CloseSession();
				
				await _dialogService.ShowAlertAsync("Must clock in to continue.", "Alert", "Clock In");
				
				var clockIn = await _dialogService.ShowConfirmationAsync("Sure to clock back in?", "Alert", "Yes", "No");
				if (clockIn)
				{
					SalesmanSession.StartSession();
				}
				else
				{
					await ClockOutHandlerAsync();
				}
			}
		}

		private async Task MenuHandlerSendAllAsync()
		{
			if (!Config.TrackInventory)
				await EndOfDayAsync();
			else
				await SendAllPresaleOrdersAsync();
		}

		private async Task SendAllPresaleOrdersAsync()
		{
			var orders = Order.Orders.Where(x => x.AsPresale && (x.OrderType == OrderType.NoService || x.Details.Count > 0)).ToList();

			if (orders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("No orders to be sent.", "Alert", "OK");
				return;
			}

			await _dialogService.ShowLoadingAsync("Sending orders...");
			string responseMessage = null;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						var batches = Batch.List.Where(x => orders.Any(y => y.BatchId == x.Id));
						DataAccess.SendTheOrders(batches, orders.Select(x => x.OrderId.ToString()).ToList());

						if (Session.session != null)
							Session.ClockOutCurrentSession();
					}
					catch (Exception ee)
					{
						responseMessage = "Error sending orders.";
						Logger.CreateLog(ee);
					}
				});
			}
			finally
			{
				await _dialogService.HideLoadingAsync();
			}

			var title = string.IsNullOrEmpty(responseMessage) ? "Success" : "Alert";
			var message = string.IsNullOrEmpty(responseMessage) ? "Order sent successfully." : responseMessage;
			await _dialogService.ShowAlertAsync(message, title, "OK");
		}

		private async Task EndOfDayAsync()
		{
			var result = await _dialogService.ShowConfirmationAsync("Sure would like to transmit all?", "Alert", "Yes", "No");
			if (result)
				await ContinueEndOfDayAsync();
		}

		private async Task ContinueEndOfDayAsync()
		{
			await _dialogService.ShowLoadingAsync("Transmitting data...");
			string responseMessage = null;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						DataAccess.SendAll();

						if (Session.session != null)
							Session.ClockOutCurrentSession();

						DataAccess.LastEndOfDay = DateTime.Now;
						Config.SaveAppStatus();
					}
					catch (Exception ee)
					{
						responseMessage = "Error sending orders.";
						Logger.CreateLog(ee);
					}
				});
			}
			finally
			{
				await _dialogService.HideLoadingAsync();
			}

			var title = string.IsNullOrEmpty(responseMessage) ? "Success" : "Alert";
			var message = string.IsNullOrEmpty(responseMessage) ? "Data successfully transmitted." : responseMessage;
			await _dialogService.ShowAlertAsync(message, title, "OK");
		}

		private async Task MenuHandlerReportsAsync()
		{
			var result = await _dialogService.ShowConfirmationAsync("End of day dialog message.", "Warning", "Yes", "No");
			if (result)
			{
				// Check if all orders are finished
				foreach (var o in Order.Orders)
				{
					if (o.OrderType == OrderType.Load) continue;
					if (Config.MustEndOrders && !o.Finished && !o.Voided && !o.AsPresale && o.Details.Count > 0)
					{
						var msg = $"Have not finalized invoices {(o.Client != null ? o.Client.ClientName : "Customer not found")}";
						if (!string.IsNullOrEmpty(o.PrintedOrderId))
							msg += $"\nInvoice number: {o.PrintedOrderId}";
						await _dialogService.ShowAlertAsync(msg, "Alert", "OK");
						return;
					}
				}

				await Shell.Current.GoToAsync("//endofday");
			}
		}

		private async Task MenuHandlerConfigurationAsync(bool isFirstTime = false)
		{
			if (Config.RequestAuthPinForLogin && !Config.NeedAccessForConfiguration && isFirstTime)
			{
				// Show access code request dialog
				await RequestAccessAsync(isFirstTime);
				return;
			}

			if (Config.NeedAccessForConfiguration)
			{
				await RequestAccessAsync(isFirstTime);
				return;
			}

			await Shell.Current.GoToAsync("//configuration");
		}

		private async Task MenuHandlerUpdateProductImagesAsync()
		{
			await _dialogService.ShowLoadingAsync("Updating product images...");
			string responseMessage = null;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						DataAccess.UpdateProductImagesMap();
					}
					catch (Exception ee)
					{
						Logger.CreateLog(ee);
						responseMessage = "Error downloading data.";
					}
				});
			}
			finally
			{
				await _dialogService.HideLoadingAsync();
			}

			var title = string.IsNullOrEmpty(responseMessage) ? "Info" : "Alert";
			var message = string.IsNullOrEmpty(responseMessage) ? "Data downloaded." : responseMessage;
			await _dialogService.ShowAlertAsync(message, title, "OK");
		}

		private async Task MenuHandlerSignOutAsync()
		{
			var orderCount = Order.Orders.Count(x => x.OrderType == OrderType.NoService || x.Details.Count() > 0) + InvoicePayment.List.Count;
			if (orderCount > 0)
			{
				await _dialogService.ShowAlertAsync("You must send all transactions before signing out.", "Alert", "OK");
				return;
			}

			await _dialogService.ShowLoadingAsync("Signing out...");
			bool isButler = false;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						isButler = Config.ButlerCustomization;
						var acceptedTerms = Config.AcceptedTermsAndConditions;
						var enabledLogin = Config.EnableLogin;
						var serverAdd = Config.IPAddressGateway;
						var lanAdd = Config.LanAddress;
						var port = Config.Port;
						var salesmanId = Config.SalesmanId;
						var advancedLogin = Config.EnableAdvancedLogin;

						BackgroundDataSync.ForceBackup();
						ActivityState.RemoveAll();
						Config.ClearSettings();
						DataAccess.ClearData();
						Config.Initialize();
						DataAccess.Initialize();

						Config.AcceptedTermsAndConditions = acceptedTerms;
						Config.EnableLogin = enabledLogin;
						Config.IPAddressGateway = serverAdd;
						Config.LanAddress = lanAdd;
						Config.Port = port;
						Config.SalesmanId = salesmanId;
						Config.ButlerSignedIn = false;
						Config.EnableAdvancedLogin = advancedLogin;
						Config.SaveSettings();
					}
					catch (Exception ee)
					{
						Logger.CreateLog(ee);
					}
				});
			}
			finally
			{
				await _dialogService.HideLoadingAsync();
			}

			if (isButler)
				await Shell.Current.GoToAsync("//bottlelogin");
			else
			{
				var route = Config.EnableLogin ? "login" : (Config.EnableAdvancedLogin ? "newlogin" : "loginconfig");
				await Shell.Current.GoToAsync($"//{route}");
			}
		}

		private async Task AcceptLoadAsync()
		{
			if (!string.IsNullOrEmpty(Config.AddInventoryPassword))
			{
				// Show password dialog
				var password = await _dialogService.ShowPromptAsync("Enter password", "Accept Load", "OK", "Cancel", "Password", keyboard: Keyboard.Default);
				if (password != null && string.Compare(password, Config.AddInventoryPassword, StringComparison.CurrentCultureIgnoreCase) == 0)
					await ContinueAcceptLoadAsync();
				else
					await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
			}
			else
				await ContinueAcceptLoadAsync();
		}

		private async Task ContinueAcceptLoadAsync()
		{
			// Navigate to accept load page when created
			await Shell.Current.GoToAsync("//acceptload");
		}

		private async Task SelectSiteHandlerAsync()
		{
			var sites = SiteEx.Sites.Where(x => x.SiteType == SiteType.Main).ToList();
			var siteNames = sites.Select(x => x.Name).ToArray();
			var selected = await Application.Current!.MainPage!.DisplayActionSheet("Select Driver", "Cancel", null, siteNames);
			
			var index = Array.IndexOf(siteNames, selected);
			if (index >= 0)
			{
				if (Order.Orders.Count > 0)
				{
					await _dialogService.ShowAlertAsync("You have orders in the device that belong to another Site. Please send the orders before changing the Site.", "Alert", "OK");
					return;
				}

				var site = sites[index];
				Config.SalesmanSelectedSite = site.Id;
				Config.SaveSettings();
			}
		}

		private async Task RequestAccessAsync(bool fromFirstTime = false)
		{
			// TODO: Implement access code request via email
			await _dialogService.ShowAlertAsync("Access code request functionality to be implemented.", "Alert", "OK");
		}

		private bool AnyRouteNotClose()
		{
			var routeDate = DateTime.Today;
			var addedClients = new List<int>();

			foreach (var route in RouteEx.Routes.OrderBy(x => x.Stop))
			{
				if (route == null) continue;
				if (route.Date.Date > routeDate.AddDays(1)) continue;
				if (route.FromDelivery == false && route.Date.Date != routeDate) continue;
				if (route.Order == null && route.Date.Date != routeDate) continue;

				var client = route.Client != null ? route.Client : route.Order.Client;
				if (client == null) continue;

				if (route.Closed && !addedClients.Contains(client.ClientId))
					addedClients.Add(client.ClientId);
				else if (!route.Closed && !addedClients.Contains(client.ClientId))
					return true;
			}

			return false;
		}

		private string GetOpenRoutes()
		{
			var routeDate = DateTime.Today;
			var addedClients = new List<int>();
			var msgToReturn = string.Empty;

			foreach (var route in RouteEx.Routes.OrderBy(x => x.Stop))
			{
				if (route == null) continue;
				if (route.Date.Date > routeDate.AddDays(1)) continue;
				if (route.FromDelivery == false && route.Date.Date != routeDate) continue;
				if (route.Order == null && route.Date.Date != routeDate) continue;

				var client = route.Client != null ? route.Client : route.Order.Client;
				if (client == null) continue;

				if (route.Closed && !addedClients.Contains(client.ClientId))
					addedClients.Add(client.ClientId);
				else if (!route.Closed && !addedClients.Contains(client.ClientId))
					msgToReturn += client.ClientName + "\n";
			}

			return msgToReturn;
		}

		#endregion
	}
}

