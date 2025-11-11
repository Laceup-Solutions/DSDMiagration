using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using LaceupMigration.Controls;

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
				ServerAddress = ServerHelper.GetServerNumber(Config.IPAddressGateway) ?? string.Empty;
				Port = Config.Port.ToString();
				SalesmanId = Config.SalesmanId.ToString();

				try
				{
					if (Config.EnableSelfServiceModule && Config.SignedInSelfService)
						MainThread.BeginInvokeOnMainThread(async () => await ContinueSignInAsync());
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
				MainThread.BeginInvokeOnMainThread(async () => await GoToAsyncOrMainAsync("//MainPage"));
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
					TryRun(RemoteControl);
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
					}
					else
					{
						var baseText = field.ToLower().Contains("auth") ? field : $"Please enter a valid {field.Replace('_', ' ')}. There is no {field.Replace('_', ' ')} matching: {Config.SalesmanId}";
						await DialogHelper._dialogService.ShowAlertAsync(baseText, "Alert", "OK");
						return;
					}
				}
			}

			await ContinueSignInAsync();
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

		private async Task ContinueSignInAsync()
		{
			IsBusy = true;
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
		    	IsBusy = false;
		    }
		    
			// if (Config.EnableSelfServiceModule)
			// {
			// 	IconManager.ChangeIcon();
			// }

			if (error == 0)
			{
				await DialogHelper._dialogService.ShowAlertAsync("Connection error.", "Alert", "OK");
				return;
			}

			if (Config.AuthorizationFailed)
			{
				await DialogHelper._dialogService.ShowAlertAsync("Not authorized.","Alert", "OK");
				return;
			}

			if (error < 3)
			{
				await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.","Alert", "OK");
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
						await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.","Alert", "OK");
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
						await GoToAsyncOrMainAsync("//selfservice/clientlist");
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

						// NOTE: In MAUI, pass parameters via query or a shared state. Placeholder route:
						var route = order.Details.Count == 0 ? "//selfservice/template" : "//selfservice/checkout";
						await GoToAsyncOrMainAsync(route);
					}
				}
				catch
				{
					await DialogHelper._dialogService.ShowAlertAsync("Error downloading data.", "Alert", "OK");
				}
			}
			else
			{
				if (Config.RequestAuthPinForLogin)
				{
					Config.ShouldGetPinBeforeSync = true;
					Config.SaveSettings();
				}

				await GoToAsyncOrMainAsync($"//MainPage?downloadData=1");
			}
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
				await Shell.Current.GoToAsync("//MainPage");
			}
		}

		private static void TryRun(Action action)
		{
			try { action(); } catch (Exception ex) { Logger.CreateLog(ex); }
		}

		private static void SendLog() => Logger.SendLogFile();
		private static void ExportData() => DataAccessEx.ExportData();
		private static void RemoteControl() => DataAccessEx.RemoteControl();
	}
}

