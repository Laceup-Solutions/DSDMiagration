using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using LaceupMigration.Views;

namespace LaceupMigration.ViewModels
{
	public partial class MainPageViewModel : ObservableObject
	{
		private readonly IDialogService _dialogService;
		private readonly ILaceupAppService _appService;
		private readonly AdvancedOptionsService _advancedOptionsService;

		[ObservableProperty] private string _companyName = "Laceup";
		[ObservableProperty] private bool _showNotificationIcon;
		[ObservableProperty] private bool _showAcceptLoadMenuItem;

		public MainPageViewModel(IDialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
		{
			_dialogService = dialogService;
			_appService = appService;
			_advancedOptionsService = advancedOptionsService;
			
			// [MIGRATION]: Initialize menu visibility in constructor
			// This ensures menu is correct when ViewModel is created (e.g., after app restart)
			// Config.Initialize() is called in App.xaml.cs constructor, so DataAccess.ReceivedData should be loaded
			RefreshMenuVisibility();
		}

		public void OnAppearing()
		{
			UpdateCompanyName();
			// [MIGRATION]: Refresh menu visibility on appearing to ensure it's up-to-date
			// This handles cases where user changes or sync happens between page navigations
			RefreshMenuVisibility();
		}

		private void UpdateCompanyName()
		{
			var company = CompanyInfo.GetMasterCompany();
			CompanyName = company?.CompanyName ?? "Laceup";
			
			// [MIGRATION]: Update Shell title with company name (matches Xamarin MainActivity.UpdateCompanyName)
			// This ensures the company name appears in the header across all pages
			if (Shell.Current is AppShell appShell)
			{
				appShell.UpdateCompanyName();
			}
		}

		private void RefreshMenuVisibility()
		{
			ShowNotificationIcon = Config.DidCloseAlert;
			ShowAcceptLoadMenuItem = Config.ReceivedData && (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand) && !Config.OnlyPresale;
		}

		#region Menu Commands

		[RelayCommand]
		private async Task SyncData()
		{
			_appService.RecordEvent("mainMenuSyncData menu");
			
			if (DataProvider.MustEndOfDay())
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
			
			if (!DataProvider.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			// Navigate to AddClient page when created
			await Shell.Current.GoToAsync("addclient");
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
			if (!Config.ReceivedData)
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
				await _dialogService.ShowAlertAsync("There is a pending transfer in the system that you haven't finalized. You must either submit it or delete it.", "Alert", "OK");
				// Match Xamarin MainActivity: take user to the transfer screen so they can submit or delete
				var onTempFile = Path.Combine(Config.DataPath, "On_temp_LoadOrderPath.xml");
				var action = File.Exists(onTempFile) ? "transferOn" : "transferOff";
				await Shell.Current.GoToAsync($"transferonoff?action={action}");
				
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

			await Shell.Current.GoToAsync("sentorders");
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

			await Shell.Current.GoToAsync("sentpayments");
		}

		[RelayCommand]
		private async Task ViewSentTransactions()
		{
			_appService.RecordEvent("mainMenuViewSentTransactions menu");
			
			if (Config.AuthorizationFailed)
			{
				await _dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
				return;
			}

			// Show submenu with Sent Orders and Sent Payments options
			var subMenuOptions = new List<string>();
			
			// Always show Sent Orders when View Sent Transactions is available
			subMenuOptions.Add("View Sent Orders");
			
			// Show Sent Payments if not hidden
			if (!Config.HidePriceInTransaction)
				subMenuOptions.Add("View Sent Payments");

			if (subMenuOptions.Count == 0)
				return;

			var choice = await _dialogService.ShowActionSheetAsync("View Sent Transactions", "", "Cancel", subMenuOptions.ToArray());

			switch (choice)
			{
				case "Sent Orders":
					await SentOrders();
					break;
				case "Sent Payments":
					await SentPayments();
					break;
			}
		}

		[RelayCommand]
		private async Task ProductCatalog()
		{
			_appService.RecordEvent("mainMenuProductCatalog menu");
			
			if (!DataProvider.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			// Navigate to FullCategoryPage (same as product catalog); show Send by Email when from menu
			await Shell.Current.GoToAsync("fullcategory?showSendByEmail=1");
		}

		[RelayCommand]
		private async Task ViewOrderStatus()
		{
			_appService.RecordEvent("mainMenuViewOrderStatus menu");
			
			if (!DataProvider.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			await Shell.Current.GoToAsync("vieworderstatus");
		}

		[RelayCommand]
		private async Task Inventory()
		{
			if (!DataProvider.CanUseApplication() || !Config.ReceivedData)
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			_appService.RecordEvent("mainMenuInventory menu");
			await Shell.Current.GoToAsync("inventory");
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
			await Shell.Current.GoToAsync("reports");
		}

		[RelayCommand]
		private async Task AcceptLoad()
		{
			_appService.RecordEvent("mainMenuAcceptLoad menu");
			
			if (!DataProvider.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("Must sync data before continuing.", "Warning", "OK");
				return;
			}

			await AcceptLoadAsync();
		}

		[RelayCommand]
		private async Task TimeSheet()
		{
			await Shell.Current.GoToAsync("timesheet");
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
			await Shell.Current.GoToAsync("routemanagement");
		}

		[RelayCommand]
		private async Task Goals()
		{
			await Shell.Current.GoToAsync("goals");
		}

		[RelayCommand]
		private async Task SelectSite()
		{
			await SelectSiteHandlerAsync();
		}

		[RelayCommand]
		private async Task ShowMenu()
		{
			// [MIGRATION]: Refresh menu visibility before showing menu
			// This ensures menu is correct even if user changed or config updated since last refresh
			RefreshMenuVisibility();
			
			var menuItems = new List<string>();
			
			// Match Xamarin mainMenu.xml order exactly
			// 1. Sync Data
			menuItems.Add("Sync Data");
			
			// 2. Update Product Images
			menuItems.Add("Update Product Images");
			
			// 3. Accept Load
			if (ShowAcceptLoadMenuItem)
				menuItems.Add("Accept Load");
			
			// 4. Time Sheet
			if (Config.TimeSheetCustomization)
				menuItems.Add("Time Sheet");
			
			// 5. Clock Out
			if (Config.UseClockInOut)
				menuItems.Add("Clock Out");
			
			// 6. Send All / Send Orders
			if (!Config.TrackInventory || Config.PreSale)
			{
				var sendAllText = Config.TrackInventory && Config.PreSale ? "Send Orders" : "Send All";
				menuItems.Add(sendAllText);
			}
			
			// 7. End Of Day Close
			if (Config.TrackInventory)
				menuItems.Add("End Of Day Close");
			
			// 8. Inventory Management
			if (Config.TrackInventory && !Config.HideProdOnHand)
				menuItems.Add("Inventory Management");
			
			// 9. Add Client
			if (Config.CanAddClient)
				menuItems.Add("Add New Customer");
			
			// 10. Product Catalog
			if (Config.ProductCatalog)
				menuItems.Add("Product Catalog");
			
			// 11. Goals
			if (Config.ViewGoals)
				menuItems.Add("View Goals");
			
			// 12. View Order Status
			if (Config.ShowOrderStatus)
				menuItems.Add("View Order Status");
			
			// 13. View Sent Transactions (with submenu: Sent Orders, Sent Payments)
			if (Config.ShowSentTransactions)
				menuItems.Add("View Sent Transactions →"); // Arrow indicates it opens a submenu
			
			// 14. Sent Payments (only show if ShowSentTransactions is false but HidePriceInTransaction is false)
			if (!Config.ShowSentTransactions && !Config.HidePriceInTransaction)
				menuItems.Add("Sent Payments");
			
			// 15. Show Reports
			if (Config.ShowReports)
				menuItems.Add("Show Reports");
			
			// 16. Select Site
			if (Config.SalesmanCanChangeSite && !Config.HideSelectSitesFromMenu)
				menuItems.Add("Select Site");
			
			// 17. Route Management
			if (Config.RouteManagement)
				menuItems.Add("Route Management");
			
			// 18. Advanced Options
			menuItems.Add("Advanced Options");
			
			// 19. Configuration
			menuItems.Add("Configuration");
			
			// 20. Sign Out
			if (Config.CanLogout && !Config.NeedAccessForConfiguration)
				menuItems.Add("Sign Out");
			
			// 21. About Laceup Solutions
			menuItems.Add("About Laceup Solutions");

			var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", menuItems.ToArray());
			
			// Strip arrow indicator if present (for menu items that open submenus)
			if (!string.IsNullOrEmpty(choice))
				choice = choice.Replace(" →", "").Trim();
			
			switch (choice)
			{
				case "Add Client":
				case "Add New Customer":
					await AddClient();
					break;
				case "Sync Data":
					await SyncData();
					break;
				case "Update Product Images":
					await UpdateProductImages();
					break;
				case "Accept Load":
					await AcceptLoad();
					break;
				case "Time Sheet":
					await TimeSheet();
					break;
				case "Clock Out":
					await ClockOut();
					break;
				case "Send All":
				case "Send Orders":
					await SendAll();
					break;
				case "End Of Day Close":
					await Reports();
					break;
				case "Inventory Management":
					await Inventory();
					break;
				case "Product Catalog":
					await ProductCatalog();
					break;
				case "View Goals":
					await Goals();
					break;
				case "View Order Status":
					await ViewOrderStatus();
					break;
				case "View Sent Transactions": // Handles both with and without arrow
					await ViewSentTransactions();
					break;
				case "Sent Orders":
					await SentOrders();
					break;
				case "Sent Payments":
					await SentPayments();
					break;
				case "Show Reports":
					await ShowReports();
					break;
				case "Select Site":
					await SelectSite();
					break;
				case "Route Management":
					await RouteManagement();
					break;
				case "Advanced Options":
					await AdvancedLog();
					break;
				case "Configuration":
					await Configuration();
					break;
				case "Sign Out":
					await SignOut();
					break;
				case "About Laceup Solutions":
					await About();
					break;
			}
		}

		[RelayCommand]
		private async Task AdvancedLog()
		{
			await _advancedOptionsService.ShowAdvancedOptionsAsync();
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

		public async Task DownloadDataAsync(bool updateInventory, int oldScanner = 0, bool isAutomatic = false)
		{
			// [MIGRATION]: Auto sync logic from Xamarin
			// Matches Xamarin MainActivity.DownloadData() method (line 1574-1707)
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

						DataProvider.CheckAuthorization();
						if (Config.AuthorizationFailed)
							throw new Exception("Not authorized");

						if (!DataProvider.CheckSyncAuthInfo())
							throw new Exception("Wait before sync");

						Logger.CreateLog("called MenuHandlerSyncData");

						responseMessage = DataProvider.DownloadData(true, !Config.TrackInventory || updateInventory);

						// [MIGRATION]: Auto sync logic from Xamarin
						// Matches Xamarin MainActivity.DownloadData() lines 1628-1636
						// When called automatically after sign-in, save vendor name
						if (isAutomatic)
						{
							var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
							if (salesman != null)
							{
								Config.VendorName = salesman.Name;
								Config.SaveSystemSettings();
							}
						}
					}
					catch (Exception ee)
					{
						errorDownloadingData = true;
						Logger.CreateLog(ee);
						
						var message = ee.Message;
						if (message.Contains("Invalid auth info"))
							message = "Not authorized";
						
						responseMessage = message.Replace("(305)-381-1123", "(786) 437-4380");
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
			
			// [MIGRATION]: Auto sync logic from Xamarin
			// Matches Xamarin MainActivity.DownloadData() line 1687
			// Subscribe to notifications after successful sync
			if (!errorDownloadingData)
			{
				SubscribeToNotifications();
			}

			// Fix: Add delivery clients for already accepted deliveries after sync
			// This ensures clients are saved to file so DeletePendingLoads won't delete them
			if (!errorDownloadingData)
			{
				var acceptedDeliveryOrders = Order.Orders.Where(x => x.IsDelivery && !x.PendingLoad).ToList();
				var processedClientIds = new HashSet<int>();
				foreach (var order in acceptedDeliveryOrders)
				{
					if (order.Client != null && !processedClientIds.Contains(order.Client.ClientId))
					{
						DataProvider.AddDeliveryClient(order.Client);
						processedClientIds.Add(order.Client.ClientId);
					}
				}
			}

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1733-1738
			// Check for NewSyncLoadOnDemand and RouteOrdersCount
			if (!errorDownloadingData && Config.NewSyncLoadOnDemand && Config.RouteOrdersCount > 0)
			{
				// Match Xamarin GoToAcceptLoad(DateTime.Now, true) - download load orders before navigating
				// When fromDownloadData=true, it skips DownloadProducts/DownloadClients but still calls GetPendingLoadOrders
				await _dialogService.ShowLoadingAsync("Downloading load orders...");
				string loadOrdersResponseMessage = null;
				
				try
				{
					await Task.Run(() =>
					{
						try
						{
							// Products and clients already downloaded in sync, just get pending load orders
							// Match Xamarin: DataAccess.GetPendingLoadOrders(date, Config.ShowAllAvailableLoads)
							DataProvider.GetPendingLoadOrders(DateTime.Now, Config.ShowAllAvailableLoads);
						}
						catch (Exception e)
						{
							Logger.CreateLog(e);
							loadOrdersResponseMessage = "Error downloading load orders.";
						}
					});
				}
				finally
				{
					await _dialogService.HideLoadingAsync();
				}

				if (!string.IsNullOrEmpty(loadOrdersResponseMessage))
				{
					await _dialogService.ShowAlertAsync(loadOrdersResponseMessage, "Alert", "OK");
				}
				else
				{
					// Navigate to AcceptLoad page with today's date (matches Xamarin: auto go to accept load for today when has loads to download)
					await Shell.Current.GoToAsync($"acceptload?loadDate={DateTime.Now.Ticks}");
				}
				return;
			}

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() - when SyncLoadOnDemand (old flow), fetch load orders for today and auto-navigate if any
			// NewSyncLoadOnDemand sets RouteOrdersCount during DownloadData; SyncLoadOnDemand does not, so we fetch and check here
			if (!errorDownloadingData && Config.SyncLoadOnDemand && !Config.NewSyncLoadOnDemand && !Config.OnlyPresale)
			{
				await _dialogService.ShowLoadingAsync("Downloading load orders...");
				string loadOrdersResponseMessage = null;
				bool hasLoadsForToday = false;

				try
				{
					await Task.Run(() =>
					{
						try
						{
							DataProvider.GetPendingLoadOrders(DateTime.Now, Config.ShowAllAvailableLoads);
							var pendingOrders = Order.Orders.Where(x => (x.OrderType == OrderType.Load || x.IsDelivery) && x.PendingLoad).ToList();
							hasLoadsForToday = pendingOrders.Any();
						}
						catch (Exception e)
						{
							Logger.CreateLog(e);
							loadOrdersResponseMessage = "Error downloading load orders.";
						}
					});
				}
				finally
				{
					await _dialogService.HideLoadingAsync();
				}

				if (!string.IsNullOrEmpty(loadOrdersResponseMessage))
				{
					await _dialogService.ShowAlertAsync(loadOrdersResponseMessage, "Alert", "OK");
				}
				else if (hasLoadsForToday)
				{
					// Auto-navigate to accept load for today (matches Xamarin MainActivity after download when has loads to download)
					await Shell.Current.GoToAsync($"acceptload?loadDate={DateTime.Now.Ticks}");
					return;
				}
			}

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1740-1769
			// Check for pending load to accept after sync
			if (!errorDownloadingData && Config.TrackInventory && updateInventory && Config.PendingLoadToAccept)
			{
			if (Config.AutoAcceptLoad)
			{
				// Auto-accept load logic (Xamarin lines 1744-1758)
				// Update inventory for all products with RequestedLoadInventory > 0
				foreach (var product in Product.Products)
				{
					if (product.RequestedLoadInventory > 0)
					{
						product.UpdateInventory(product.RequestedLoadInventory, null, 1, 0);
						product.AddLoadedInventory(product.RequestedLoadInventory, null, 0);
					}
				}

				// Fix: Call AddDeliveryClient for accepted delivery orders (missing in Xamarin too)
				// This saves delivery clients to file so they can be loaded on app restart
				var acceptedDeliveryOrders = Order.Orders.Where(x => x.IsDelivery && !x.PendingLoad).ToList();
				var processedClientIds = new HashSet<int>();
				foreach (var order in acceptedDeliveryOrders)
				{
					if (order.Client != null && !processedClientIds.Contains(order.Client.ClientId))
					{
						DataProvider.AddDeliveryClient(order.Client);
						processedClientIds.Add(order.Client.ClientId);
					}
				}

				Config.PendingLoadToAccept = false;
				Config.SaveAppStatus();
				ProductInventory.Save();
			}
				else
				{
					// Navigate to inventory page (Xamarin lines 1762-1765)
					// Pass actionIntent=1 to trigger AcceptLoad automatically
					await Shell.Current.GoToAsync("inventorymain?actionIntent=1");
				}
			}

			// Check for deliveries after sync and navigate to accept load if AutoAcceptLoad is OFF
			// Only check if PendingLoadToAccept flag was set during DownloadData (prevents checking every sync)
			if (!errorDownloadingData && !Config.AutoAcceptLoad && Config.PendingLoadToAccept)
			{
				await _dialogService.ShowLoadingAsync("Checking for deliveries...");
				string loadOrdersResponseMessage = null;
				bool hasDeliveries = false;
				
				try
				{
					await Task.Run(() =>
					{
						try
						{
							// Get pending load orders for today
							DataProvider.GetPendingLoadOrders(DateTime.Now, Config.ShowAllAvailableLoads);
							
							// Check if there are any pending load orders
							var pendingOrders = Order.Orders.Where(x => (x.OrderType == OrderType.Load || x.IsDelivery) && x.PendingLoad).ToList();
							hasDeliveries = pendingOrders.Any();
						}
						catch (Exception e)
						{
							Logger.CreateLog(e);
							loadOrdersResponseMessage = "Error checking for deliveries.";
						}
					});
				}
				finally
				{
					await _dialogService.HideLoadingAsync();
				}

				if (!string.IsNullOrEmpty(loadOrdersResponseMessage))
				{
					await _dialogService.ShowAlertAsync(loadOrdersResponseMessage, "Alert", "OK");
				}
				else if (hasDeliveries)
				{
					// Navigate to AcceptLoad page with today's date
					await Shell.Current.GoToAsync($"acceptload?loadDate={DateTime.Now.Ticks}");
					return;
				}
			}
		}
		
		// [MIGRATION]: Auto sync logic from Xamarin
		// Matches Xamarin MainActivity.SubscribeToNotifications() method (line 1709-1722)
		private void SubscribeToNotifications()
		{
			if (Config.CheckCommunicatorVersion("29.94"))
			{
				if (Config.EnableLiveData || Config.AllowWorkOrder || Config.AllowNotifications)
				{
					DataProvider.GetTopic();
				}
				else
				{
					DataProvider.Unsubscribe();
				}
			}
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
				await _dialogService.ShowAlertAsync("There are no orders in the system to be sent.", "Alert", "OK");
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
						DataProvider.SendTheOrders(batches, orders.Select(x => x.OrderId.ToString()).ToList());

						// Set client Editable to false when sending presale orders (fixes Xamarin bug)
						// Only for locally created clients (ClientId <= 0)
						// Only set once per client (use first presale order's client)
						var firstPresaleOrder = orders.FirstOrDefault(o => o.AsPresale && o.Client != null && o.Client.ClientId <= 0);
						if (firstPresaleOrder?.Client != null)
						{
							firstPresaleOrder.Client.Editable = false;
							Client.Save();
						}

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
			var result = await _dialogService.ShowConfirmationAsync("Alert", "Are you sure that you would like to transmit all the information?");
			if (result)
				await ContinueEndOfDayAsync();
		}

		private async Task ContinueEndOfDayAsync()
		{
			await _dialogService.ShowLoadingAsync("Sending all information...");
			string responseMessage = null;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						DataProvider.SendAll();

						if (Session.session != null)
							Session.ClockOutCurrentSession();

						Config.LastEndOfDay = DateTime.Now;
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

			// Lock the app after successful end of day - mimic Xamarin behavior
			// This matches SetDefaultsAfterSendAll() in Xamarin MainActivity and EndOfDayHandler in EndOfDayPageViewModel
			if (string.IsNullOrEmpty(responseMessage))
			{
				// Set ReceivedData = false to lock the app until sync happens
				// This matches Xamarin behavior where SetDefaultsAfterSendAll() sets ReceivedData = false
				Config.PendingLoadToAccept = false;
				Config.ReceivedData = false;
				Config.LastEndOfDay = DateTime.Now;
				Config.SaveAppStatus();

				// Navigate to MainPage (Clients tab)
				await Shell.Current.GoToAsync("///MainPage");

				// Force clear client list immediately - don't wait for OnAppearing
				// Wait a moment for navigation to complete
				await Task.Delay(200);
				
				MainThread.BeginInvokeOnMainThread(() =>
				{
					try
					{
						// Get the current page from Shell
						var currentPage = Shell.Current.CurrentPage;
						
						// Check if we're on ClientsPage
						if (currentPage is ClientsPage clientsPage)
						{
							clientsPage.ViewModel?.ClearClientListAndLock();
						}
					}
					catch (Exception ex)
					{
						// Log but don't crash - the OnAppearing will handle it
						System.Diagnostics.Debug.WriteLine($"Error forcing client list clear: {ex.Message}");
					}
				});
			}
		}

		private async Task MenuHandlerReportsAsync()
		{
			var result = await _dialogService.ShowConfirmationAsync("Warning", "Are you sure you want to End of Day? If you continue, the system will assume that you want to close the day and you will not be able to exit this screen until the process is finished. Press YES to continue or NO to go back to the main screen.", "Yes", "No");
			if (result)
			{
				// Check if all orders are finished
				foreach (var o in Order.Orders)
				{
					if (o.OrderType == OrderType.Load) continue;
					if (Config.MustEndOrders && !o.Finished && !o.Voided && !o.AsPresale && o.Details.Count > 0)
					{
						var msg = $"You have invoices in the system that are not finalized. In order to close your day, you must either finalize," +
						          $" void or no service your remaining invoices. The Invoice is for: {(o.Client != null ? o.Client.ClientName : "Customer not found")}";
						if (!string.IsNullOrEmpty(o.PrintedOrderId))
							msg += $"\nInvoice #: {o.PrintedOrderId}";
						await _dialogService.ShowAlertAsync(msg, "Alert", "OK");
						return;
					}
				}

				await Shell.Current.GoToAsync("endofday");
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

			await Shell.Current.GoToAsync("configuration");
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
						DataProvider.UpdateProductImagesMap();
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

		// [MIGRATION]: Sign Out logic from Xamarin
		// This method replicates Xamarin MainActivity.MenuHandlerSignOut() exactly
		// Xamarin source: MainActivity.cs lines 474-569
		private async Task MenuHandlerSignOutAsync()
		{
			// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 476-482
			// Check if orders exist - must send all transactions before signing out
			int c = Order.Orders.Count(x => x.OrderType == OrderType.NoService || x.Details.Count() > 0) + InvoicePayment.List.Count;

			if (c > 0)
			{
				await _dialogService.ShowAlertAsync("You must send all transactions before signing out.", "Alert", "OK");
				return;
			}

			// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 485
			// Show progress dialog
			await _dialogService.ShowLoadingAsync("Signing out...");
			bool isButler = false;

			// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 487
			// ThreadPool.QueueUserWorkItem for background work
			bool signOutError = false;
			try
			{
				await Task.Run(() =>
				{
					try
					{
						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 491-500
						// Save configuration values before clearing (preserve all required values)
						isButler = Config.ButlerCustomization;

						var acceptedTerms = Config.AcceptedTermsAndConditions;
						var enabledlogin = Config.EnableLogin;
						var butlerCustomization = Config.ButlerCustomization; // Preserve ButlerCustomization

						var serverAdd = Config.IPAddressGateway;
						var lanAdd = Config.LanAddress;
						var port = Config.Port;
						var salesmanId = Config.SalesmanId;
						var advancedLogin = Config.EnableAdvancedLogin;
						var ssid = Config.SSID; // Preserve SSID - needed for ConnectionAddress (WhichAddressToUse)

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 502
						BackgroundDataSync.ForceBackup();

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 504
						ActivityState.RemoveAll();

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 506
						Config.ClearSettings();

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 508
						DataProvider.ClearData();

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 509-510
						Config.Initialize();
						DataProvider.Initialize();

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 512-520
						// Restore saved configuration values (must restore before SaveSettings)
						// These values are required for NetAccess.OpenConnection() to work correctly
						Config.AcceptedTermsAndConditions = acceptedTerms;
						Config.EnableLogin = enabledlogin;
						Config.EnableAdvancedLogin = advancedLogin;
						Config.IPAddressGateway = serverAdd; // Required for ConnectionAddress
						Config.LanAddress = lanAdd; // Required for ConnectionAddress (if SSID matches)
						Config.Port = port; // Required for OpenConnection
						Config.SalesmanId = salesmanId; // Required for authentication
						Config.SSID = ssid; // Required for ConnectionAddress (WhichAddressToUse)
						Config.ButlerCustomization = butlerCustomization; // Restore ButlerCustomization
						Config.ButlerSignedIn = false;

						// [MIGRATION]: Explicitly set SignedIn to false (ensures clean sign-out state)
						// This matches the intent of Xamarin's sign-out flow
						Config.SignedIn = false;

						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() line 521
						// SaveSettings must be called AFTER restoring all values
						Config.SaveSettings();
					}
					catch (Exception ee)
					{
						// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 523-534
						// Error handling - hide dialog and show error message
						signOutError = true;
						Logger.CreateLog(ee);

						MainThread.BeginInvokeOnMainThread(async () =>
						{
							await _dialogService.HideLoadingAsync();
							await _dialogService.ShowAlertAsync("An error ocurred trying to sing out. Please try again.", "Alert", "OK");
						});
					}
				});
			}
			finally
			{
				// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 535-567
				// Hide progress dialog
				await _dialogService.HideLoadingAsync();
			}

			// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 535-567
			// Navigate to login page (done outside finally to avoid return statement issue)
			if (!signOutError)
			{
				// [MIGRATION]: Ensure ShouldGetPinBeforeSync is also reset
				Config.ShouldGetPinBeforeSync = false;
				Config.SaveSettings();

				// [MIGRATION]: Debug log before navigation
				Console.WriteLine("[DEBUG] Sign out complete. Config.SignedIn = " + Config.SignedIn + ", navigating to login...");

				// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 537-566
				// Use MainThread.BeginInvokeOnMainThread to ensure navigation happens synchronously
				// This prevents OnAppearing logic from running before navigation completes
				// Xamarin uses RunOnUiThread to ensure UI operations happen on main thread
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSignOut() lines 556-563
					// Navigate to Splash using absolute route to clear entire navigation stack
					// SplashPage will detect Config.SignedIn = false and immediately redirect to login
					// This provides absolute routing (///Splash) while going directly to login page
					Console.WriteLine("[DEBUG] Before navigation to Splash (will redirect to login). Config.SignedIn = " + Config.SignedIn);
					await Shell.Current.GoToAsync("///Splash");
					Console.WriteLine("[DEBUG] After navigation to Splash (should have redirected to login).");
				});
			}
		}

		private async Task AcceptLoadAsync()
		{
			// [MIGRATION]: Matches Xamarin InventoryMainActivity.AcceptLoad_Click() logic
			if (string.IsNullOrEmpty(Config.AddInventoryPassword))
			{
				if (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand)
				{
					// AcceptLoadOnDemand - show date picker first, then download and navigate
					await AcceptLoadOnDemandAsync();
				}
				else
				{
					// Navigate to ReceiveLoadActivity (old accept load page - not the list)
					await Shell.Current.GoToAsync("acceptload");
				}
				return;
			}

			// Ask for password first
			var password = await _dialogService.ShowPromptAsync("Accept Load", "Enter Password", "OK", "Cancel", "Password", keyboard: Keyboard.Default);
			if (string.IsNullOrEmpty(password))
				return; // User cancelled

			if (string.Compare(password, Config.AddInventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
			{
				await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
				return;
			}

			// Password is correct
			// [MIGRATION]: Match Xamarin - when password is correct, only check SyncLoadOnDemand (not NewSyncLoadOnDemand)
			if (Config.SyncLoadOnDemand)
			{
				// AcceptLoadOnDemand - show date picker first, then download and navigate
				await AcceptLoadOnDemandAsync();
			}
			else
			{
				// Navigate to ReceiveLoadActivity (old accept load page)
				await Shell.Current.GoToAsync("acceptload");
			}
		}

		private async Task AcceptLoadOnDemandAsync()
		{
			try
			{
				// [MIGRATION]: Match Xamarin AcceptLoadOnDemand() - show date picker with today's date
				// Match Xamarin: DateTime dt = DateTime.Today; var dialog = new DatePickerDialogFragment(this, dt);
				DateTime selectedDate = DateTime.Today;
				var date = await _dialogService.ShowDatePickerAsync("Select Date", selectedDate);
				
				if (date.HasValue)
				{
					// Match Xamarin Refresh(date) - download and navigate
					await RefreshAndNavigateToAcceptLoadAsync(date.Value);
				}
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error showing date picker: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		private async Task RefreshAndNavigateToAcceptLoadAsync(DateTime date)
		{
			// [MIGRATION]: Match Xamarin Refresh(DateTime date) method
			await _dialogService.ShowLoadingAsync("Downloading load orders...");
			string responseMessage = null;

			try
			{
				await Task.Run(() =>
				{
					try
					{
						// Download products first
						DataProvider.DownloadProducts();

						// Get pending load orders for the selected date
						DataProvider.GetPendingLoadOrders(date);
					}
					catch (Exception e)
					{
						Logger.CreateLog(e);
						responseMessage = "Error downloading load orders.";
					}
				});

				await _dialogService.HideLoadingAsync();

				if (!string.IsNullOrEmpty(responseMessage))
				{
					await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
				}
				else
				{
					// Navigate to AcceptLoadOrderList (AcceptLoadPage) with the selected date
					// Match Xamarin: activity.PutExtra("loadDate", date.Ticks.ToString());
					await Shell.Current.GoToAsync($"acceptload?loadDate={date.Ticks}");
				}
			}
			catch (Exception ex)
			{
				await _dialogService.HideLoadingAsync();
				await _dialogService.ShowAlertAsync("Error refreshing load orders.", "Alert", "OK");
				_appService.TrackError(ex);
			}
		}

		private async Task SelectSiteHandlerAsync()
		{
			var sites = SiteEx.Sites.Where(x => x.SiteType == SiteType.Main).ToList();
			var siteNames = sites.Select(x => x.Name).ToArray();
			var selected = await _dialogService.ShowActionSheetAsync("Select Driver", "", "Cancel", siteNames);
			
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

