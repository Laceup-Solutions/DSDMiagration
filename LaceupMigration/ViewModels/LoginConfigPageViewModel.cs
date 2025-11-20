using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace LaceupMigration.ViewModels
{
	public partial class LoginConfigPageViewModel : ObservableObject
	{
		[ObservableProperty] private string _serverAddress = string.Empty;
		[ObservableProperty] private string _port = string.Empty;
		[ObservableProperty] private string _salesmanId = string.Empty;
		[ObservableProperty] private bool _isBusy;

		public LoginConfigPageViewModel()
		{
			if (!Config.ApplicationIsInDemoMode)
			{
#if DEBUG
				// DEBUG mode: Pre-fill Server Address and Port, but never show 0 for Company ID or Salesman ID
				ServerAddress = ServerHelper.GetServerNumber(Config.IPAddressGateway) ?? string.Empty;
				// Port is already pre-filled with 9284 in Config.cs (DEBUG mode default)
				Port = Config.Port != 0 ? Config.Port.ToString() : string.Empty;
				// Never show 0 for Salesman ID - leave blank if 0
				SalesmanId = Config.SalesmanId != 0 ? Config.SalesmanId.ToString() : string.Empty;
#else
				// RELEASE mode: Show empty fields or load from preferences, but never show 0
				// Server Address: Load from preferences if set, otherwise empty
				var serverAddr = ServerHelper.GetServerNumber(Config.IPAddressGateway);
				ServerAddress = !string.IsNullOrEmpty(serverAddr) ? serverAddr : string.Empty;
				
				// Port: Load from preferences if set and not 0, otherwise empty
				Port = Config.Port != 0 ? Config.Port.ToString() : string.Empty;
				
				// Salesman ID: Never show 0 - leave blank if 0
				SalesmanId = Config.SalesmanId != 0 ? Config.SalesmanId.ToString() : string.Empty;
#endif

				try
				{
					if (Config.EnableSelfServiceModule && Config.SignedInSelfService)
						MainThread.BeginInvokeOnMainThread(() => ContinueSignInAsync());
				}
				catch
				{
					// ignored
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
				await DialogHelper._dialogService.ShowAlertAsync("Error trying to call Laceup Support. Please make sure your device can make calls.","Alert", "OK");
			}
		}

		[RelayCommand]
		private async Task NeedHelp()
		{
			var options = new[] { "Send log file", "Export data", "Remote control" };
			var choice = await Application.Current!.MainPage!.DisplayActionSheet("Advanced options", "Cancel", null, options);
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
		private async Task SignIn()
		{
			var saved = await SaveConfigurationAsync();
			if (!saved)
			{
				await DialogHelper._dialogService.ShowAlertAsync("Please check the configuration.", "Alert", "OK");
				return;
			}

			NetAccess.GetCommunicatorVersion();

				if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "80.0.0"))
				{
					var field = DataAccess.GetFieldForLogin();
					if (!string.IsNullOrEmpty(field))
					{
						DataAccess.GetSalesmanList();
						var actualSalesman = GetSalesmanBasedOnField(field);
						if (actualSalesman != null)
						{
							Config.SalesmanId = actualSalesman.Id;
							// [MIGRATION]: Save SalesmanId to preferences (matches Xamarin behavior)
							// Ensures the correct SalesmanId from server response is persisted
							Preferences.Set("VendorId", Config.SalesmanId);
						}
						else
						{
							var baseText = field.ToLower().Contains("auth") ? field : $"Please enter a valid {field.Replace('_', ' ')}. There is no {field.Replace('_', ' ')} matching: {Config.SalesmanId}";
							await DialogHelper._dialogService.ShowAlertAsync(baseText, "Alert", "OK");
							return;
						}
					}
				}

			ContinueSignInAsync();
		}

		private Salesman? GetSalesmanBasedOnField(string field)
		{
			return field switch
			{
				"route_number" => Salesman.List.FirstOrDefault(x => x.RouteNumber == Config.SalesmanId.ToString()),
				"id" => Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId),
				"original_id" => Salesman.List.FirstOrDefault(x => x.OriginalId == Config.SalesmanId.ToString()),
				"name" => Salesman.List.FirstOrDefault(x => x.Name == Config.SalesmanId.ToString()),
				"phone" => Salesman.List.FirstOrDefault(x => x.Phone == Config.SalesmanId.ToString()),
				_ => Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId),
			};
		}

		private void ContinueSignInAsync()
		{
			// Show loading overlay (matches Xamarin ProgressDialog.Show)
			string message = "Loading...";
			ProgressDialogHelper.Show(message );

			// Run work on background thread (matches Xamarin ThreadPool.QueueUserWorkItem)
			ThreadPool.QueueUserWorkItem(delegate (object stt)
			{
				int error = 0;
				try
				{
					// [MIGRATION]: Match Xamarin ContinueSigIn() - does NOT call Config.Initialize()
					// Xamarin only creates directories if needed, without resetting SalesmanId
					// Ensure critical directories exist for file operations (needed for logging and session files)
					if (!Directory.Exists(Config.DataPath))
					{
						Directory.CreateDirectory(Config.DataPath);
					}
					
					// [MIGRATION]: Ensure StaticDataPath exists (SessionPath depends on it)
					// This prevents "Could not find a part of the path" errors on Android tablet emulators
					if (!Directory.Exists(Config.StaticDataPath))
					{
						Directory.CreateDirectory(Config.StaticDataPath);
					}
					
					// [MIGRATION]: Ensure SessionPath exists before Session.CreateSession() is called
					// This prevents crashes on Android tablet emulators where directories might not be initialized
					if (!Directory.Exists(Config.SessionPath))
					{
						Directory.CreateDirectory(Config.SessionPath);
					}
					
					DataAccess.Initialize();

					DataAccess.GetUserSettingLine();
					DataAccess.CheckAuthorization();

					error = 1;

					if (!Config.AuthorizationFailed)
					{
						DataAccess.GetSalesmanSettings(false);

						error = 2;

						if (Config.EnableSelfServiceModule)
						{
							// Update message when downloading data (matches Xamarin progressDialog.SetMessage)
							MainThread.BeginInvokeOnMainThread(() =>
							{
								ProgressDialogHelper.SetMessage("Downloading data.");
							});
							DataAccessEx.DownloadStaticData();
						}

						error = 3;
					}
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
				finally
				{
					// Hide loading overlay (matches Xamarin progressDialog.Hide)
					MainThread.BeginInvokeOnMainThread(() =>
					{
						ProgressDialogHelper.Hide();
					});

					// if (Config.EnableSelfServiceModule)
					// {
					// 	IconManager.ChangeIcon();
					// }

					// Handle errors and navigation on main thread (matches Xamarin RunOnUiThread)
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						if (error == 0)
						{
							await DialogHelper._dialogService.ShowAlertAsync("Connection error.", "Alert", "OK");
							return;
						}

						if (Config.AuthorizationFailed)
						{
							await DialogHelper._dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
							return;
						}

						if (error < 3)
						{
							await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
							return;
						}

						Config.SignedIn = !Config.EnableSelfServiceModule;
						Config.LastSignIn = DateTime.Now.Ticks.ToString();
						Config.SaveSettings();

						if (Config.EnableSelfServiceModule)
						{
							try
							{
								if (Client.Clients.Count == 0)
								{
									await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
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

								if (Client.Clients.Count > 1)
								{
									await GoToAsyncOrMainAsync("selfservice/clientlist");
								}
								else
								{
									var client = Client.Clients.FirstOrDefault();

									client.EnsureInvoicesAreLoaded();
									client.EnsurePreviouslyOrdered();

									var order = Order.Orders.FirstOrDefault(x => x.Client.ClientId == client.ClientId);

									if (order == null)
									{
										var batch = new Batch(client);
										batch.Client = client;
										batch.ClockedIn = DateTime.Now;
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
									await GoToAsyncOrMainAsync($"{route}?orderId={order.OrderId}");
								}
							}
							catch
							{
								await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
							}
						}
						else
						{
							// [MIGRATION]: Auto sync logic from Xamarin
							// In Xamarin, LoginConfigActivity navigates to MainActivity with DownloadDataIntent
							// MainActivity.OnCreate checks downloadDataAutomatically and calls MenuHandlerSyncData
							// We replicate this by calling the sync logic directly here instead of using navigation parameters
							
							if (Config.RequestAuthPinForLogin)
							{
								Config.ShouldGetPinBeforeSync = true;
								Config.SaveSettings();
							}

							// [MIGRATION]: Direct sync call matching Xamarin MainActivity.MenuHandlerSyncData() + DownloadData()
							// Xamarin MainActivity line 2224-2225: if (!Config.ShouldGetPinBeforeSync) MenuHandlerSyncData();
							// Xamarin MenuHandlerSyncData line 1250-1320: checks inventory, then calls DownloadData(!inventoryModified)
							if (!Config.ShouldGetPinBeforeSync)
							{
								// Trigger automatic sync (matches Xamarin MenuHandlerSyncData -> DownloadData flow)
								await TriggerAutoSyncAfterLogin();
							}
							else
							{
								// PIN required - navigate to MainPage without sync (user will sync manually)
								await GoToAsyncOrMainAsync("///MainPage");
							}
						}
					});
				}
			});
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
			if (string.IsNullOrWhiteSpace(ServerAddress))
				return false;

			Config.IPAddressGateway = ServerAddress;

			try
			{
				var p = Convert.ToInt32(Port);
				Config.Port = p;
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
				var id = Convert.ToInt32(SalesmanId);
				Config.SalesmanId = id;
				// [MIGRATION]: Save SalesmanId to preferences (matches Xamarin behavior - saved at end via SaveSettings)
				// We save immediately here to ensure persistence, matching Xamarin's SaveSettings() call after login
				Preferences.Set("VendorId", Config.SalesmanId);
			}
			catch (Exception ee)
			{
				Logger.CreateLog(ee);
				return false;
			}

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

		// [MIGRATION]: Auto sync logic from Xamarin
		// This method replicates Xamarin MainActivity.MenuHandlerSyncData() + DownloadData() flow
		// Xamarin source: MainActivity.cs lines 1250-1707
		private async Task TriggerAutoSyncAfterLogin()
		{
			// [MIGRATION]: Matches Xamarin MainActivity.MenuHandlerSyncData() lines 1258-1320
			// Check for inventory changes (Xamarin line 1259-1270)
			int c = Order.Orders.Count(x => x.OrderType == OrderType.NoService || x.Details.Count() > 0) + InvoicePayment.List.Count;
			var inventoryModified = ProductInventory.CurrentInventories.Values.Any(x => x.InventoryChanged);

			if (inventoryModified && Config.UpdateInventoryRegardless)
				inventoryModified = false;

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() method (line 1574-1707)
			// For auto-sync after login, we skip inventory change dialog and proceed directly
			// Xamarin line 1320: DownloadData(!inventoryModified) is called
			await DownloadDataAfterLogin(!inventoryModified);
		}

		// [MIGRATION]: Auto sync logic from Xamarin
		// This method replicates Xamarin MainActivity.DownloadData() method exactly
		// Xamarin source: MainActivity.cs lines 1574-1707
		private async Task DownloadDataAfterLogin(bool updateInventory)
		{
			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1576
			// DataDownloading = true; (not needed in MAUI as we're already in async context)

			DateTime now = DateTime.Now;

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1581-1588
			// Check if orders must be sent first
			if (!Config.Delivery && Config.MustSendOrdersFirst)
			{
				if (Order.Orders.Count(x => x.OrderType == OrderType.NoService || x.Details.Count() > 0) > 0)
				{
					await DialogHelper._dialogService.ShowAlertAsync("Send orders before sync.", "Alert", "OK");
					await GoToAsyncOrMainAsync("///MainPage");
					return;
				}
			}

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1593-1595
			// Delete empty orders
			foreach (Order order in Order.Orders.ToArray())
				if (order.OrderType != OrderType.NoService && order.Details.Count == 0 && string.IsNullOrEmpty(order.PrintedOrderId))
					order.Delete();

			string responseMessage = null;
			bool errorDownloadingData = false;

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1600
			// Show progress dialog (we already have one from login, but update message)
			ProgressDialogHelper.Show("Downloading data...");

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1602
			// ThreadPool.QueueUserWorkItem for background work
			await Task.Run(() =>
			{
				try
				{
					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1606-1610
					// Test network connection
					using (var access = new NetAccess())
					{
						access.OpenConnection();
						access.CloseConnection();
					}

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1612
					DataAccess.CheckAuthorization();

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1614-1617
					if (Config.AuthorizationFailed)
					{
						throw new Exception("Not authorized");
					}

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1619-1622
					if (!DataAccess.CheckSyncAuthInfo())
					{
						throw new Exception("Wait before sync");
					}

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1624
					Logger.CreateLog("called MenuHandlerSyncData");

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1626
					responseMessage = DataAccessEx.DownloadData(true, !Config.TrackInventory || updateInventory);

					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1628-1636
					// When called automatically after login, save vendor name
					var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
					if (salesman != null)
					{
						Config.VendorName = salesman.Name;
						Config.SaveSystemSettings();
					}
				}
				catch (Exception ee)
				{
					// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1638-1650
					errorDownloadingData = true;
					Logger.CreateLog(ee);

					var message = ee.Message;
					if (message.Contains("Invalid auth info"))
						message = "Not authorized";

					responseMessage = message.Replace("(305)-381-1123", "(786) 437-4380");
				}
			});

			// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1651-1699
			// Handle UI updates on main thread (Xamarin uses RunOnUiThread)
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1656-1663
				string title = "Warning";
				if (string.IsNullOrEmpty(responseMessage))
				{
					TimeSpan ts = DateTime.Now.Subtract(now);
					responseMessage = "Data downloaded.";
					title = "Info";
					Logger.CreateLog("Data downloaded in " + ts.TotalSeconds);
				}

				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1665
				ProgressDialogHelper.Hide();

				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1671-1676
				// Check for load on demand
				if (Config.NewSyncLoadOnDemand && DataAccess.RouteOrdersCount > 0)
				{
					FinishDownloadDataAfterLogin(errorDownloadingData, updateInventory);
					return;
				}

				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1678-1681
				// Show result dialog
				await DialogHelper._dialogService.ShowAlertAsync(responseMessage, title, "OK");

				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() lines 1683-1695
				if (!errorDownloadingData)
				{
					// UpdateCompanyName() - handled by MainPageViewModel.OnAppearing
					// SubscribeToNotifications() - will be called after navigation

					// Note: Master device scanner logic (lines 1690-1694) handled elsewhere
				}

				// [MIGRATION]: Matches Xamarin MainActivity.DownloadData() line 1697
				// DataDownloading = false; (not needed in MAUI)

				// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() call (line 1680)
				FinishDownloadDataAfterLogin(errorDownloadingData, updateInventory);
			});
		}

		// [MIGRATION]: Auto sync logic from Xamarin
		// This method replicates Xamarin MainActivity.FinishDownloadData() method
		// Xamarin source: MainActivity.cs lines 1726-1774
		private async void FinishDownloadDataAfterLogin(bool errorDownloadingData, bool updateInventory)
		{
			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() line 1728
			// needToEnterVehicle = false; (handled by navigation if needed)

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1730-1731
			if (!errorDownloadingData)
				SalesmanSession.StartSession();

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1733-1738
			if (!errorDownloadingData && Config.NewSyncLoadOnDemand && DataAccess.RouteOrdersCount > 0)
			{
				// needToEnterVehicle = true;
				// GoToAcceptLoad(DateTime.Now, true); - navigate to accept load page
				await GoToAsyncOrMainAsync("///MainPage"); // Simplified for now
				return;
			}

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1740-1769
			// Inventory load acceptance logic (simplified - full implementation would navigate to inventory page)
			if (!errorDownloadingData && Config.TrackInventory && updateInventory && DataAccess.PendingLoadToAccept)
			{
				if (Config.AutoAcceptLoad)
				{
					// Auto-accept load logic (Xamarin lines 1744-1758)
					// Simplified for now - full implementation would update inventory
					DataAccess.PendingLoadToAccept = false;
					Config.SaveAppStatus();
					DataAccess.SaveInventory();
				}
				else
				{
					// Navigate to inventory page (Xamarin lines 1762-1765)
					// await GoToAsyncOrMainAsync("inventorymain"); // If page exists
				}
			}

			// [MIGRATION]: Matches Xamarin MainActivity.FinishDownloadData() lines 1771-1772
			// Vehicle information check (simplified)
			// if (!needToEnterVehicle && Config.RequestVehicleInformation && VehicleInformation.CurrentVehicleInformation == null)
			//     OpenVehicleInformation();

			// [MIGRATION]: Navigate to MainPage and subscribe to notifications
			// Matches Xamarin MainActivity.OnCreate() lines 2228-2230
			await GoToAsyncOrMainAsync("///MainPage");
			
			// [MIGRATION]: Update company name in header after login (matches Xamarin UpdateCompanyName call)
			// This ensures the company name appears immediately after login completes
			if (Shell.Current is AppShell appShell)
			{
				appShell.UpdateCompanyName();
			}
			
			// [MIGRATION]: Matches Xamarin MainActivity.SubscribeToNotifications() call (line 2230)
			// SubscribeToNotifications() will be called by MainPageViewModel after navigation
		}
	}
}

