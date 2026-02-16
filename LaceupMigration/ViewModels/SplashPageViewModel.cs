using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using LaceupMigration.Services;
using System.IO;
using System.Linq;
using System;

namespace LaceupMigration.ViewModels
{
	public partial class SplashPageViewModel : ObservableObject
	{
		private readonly ILaceupAppService _appService;
		private readonly IActivityStateRestorationService _restorationService;
		private static bool _hasRestored = false; // Flag to prevent multiple restorations

		public SplashPageViewModel(ILaceupAppService appService, IActivityStateRestorationService restorationService)
		{
			_appService = appService;
			_restorationService = restorationService;
		}

		// [MIGRATION]: Sign Out fix - immediately redirect to login when signed out
		// This allows ///Splash navigation to clear stack, then immediately go to login page
		// Must check Terms first - if not accepted, redirect to Terms page instead
		public async Task RedirectToLoginAsync()
		{
			// [MIGRATION]: Check Terms first (matches RouteNextAsync logic)
			// If Terms not accepted, redirect to Terms page instead of login
			if (!Config.AcceptedTermsAndConditions)
			{
				Console.WriteLine("[DEBUG] SplashPage redirecting to Terms (not accepted yet)");
				await Shell.Current.GoToAsync("termsandconditions");
				return;
			}

			// Determine target login page based on configuration (matches MainPageViewModel sign-out logic)
			string targetRoute = "loginconfig"; // Default: LoginConfigActivity

			if (Config.EnableLogin)
				targetRoute = "login"; // LoginActivity

			if (Config.EnableAdvancedLogin)
				targetRoute = "newlogin"; // NewLoginActivity

			if (Config.ButlerCustomization)
				targetRoute = "bottlelogin";

			// Navigate directly to login page (Splash acts as transparent redirect)
			Console.WriteLine("[DEBUG] SplashPage redirecting to login: " + targetRoute);
			await Shell.Current.GoToAsync(targetRoute);
		}

		[RelayCommand]
		private async Task Initialize()
		{
			try
			{
				await _appService.InitializeApplicationAsync();
				Config.VersatileDEXVersion = PlatformAppInfo.GetVersatileDexVersion();

				try
				{
					if (!Directory.Exists(Config.LaceupStorage))
						Directory.CreateDirectory(Config.LaceupStorage);

					var path = Path.Combine(Config.LaceupStorage, "datawedge.db");
					using var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("USEDDLLs.datawedge.db");
					if (input != null)
					{
						using var output = File.Open(path, FileMode.Create);
						await input.CopyToAsync(output);
					}
				}
				catch
				{
					// Intentionally ignore, mirrors original behavior
				}

				await RouteNextAsync();
			}
			catch
			{
				await RouteNextAsync();
			}
		}

		private async Task RouteNextAsync()
		{
			if (!Config.AcceptedTermsAndConditions)
			{
				await GoToAsyncOrMainAsync("termsandconditions");
				return;
			}

			var isTimeOut = false;
			if (!string.IsNullOrEmpty(Config.LastSignIn) && Config.EnableAdvancedLogin)
			{
                var ticks = Convert.ToInt64(Config.LastSignIn);
                var date = new DateTime(ticks);
				if (date.AddHours(Config.LoginTimeOut) < DateTime.Now)
					isTimeOut = true;
			}

			if (Config.SignedIn && !isTimeOut)
			{
				// [ACTIVITY STATE RESTORATION]: Try to restore navigation from ActivityState
				// Only restore once per app session (on first launch after force quit)
				if (!_hasRestored)
				{
					var restorationStack = _restorationService.GetRestorationStack();
					if (restorationStack != null && restorationStack.Count > 0)
					{
						// Validate the deepest route (last in stack)
						var deepestRoute = restorationStack.LastOrDefault();
						if (!string.IsNullOrEmpty(deepestRoute))
						{
							var isValid = await _restorationService.ValidateRestorationRoute(deepestRoute);
							if (isValid)
							{
								System.Diagnostics.Debug.WriteLine($"[SplashPage] ActivityState restoration: Restoring navigation stack with {restorationStack.Count} pages. Deepest route: {deepestRoute}");
								
								_hasRestored = true; // Mark as restored to prevent multiple restorations
								
								bool isSelfServiceRoute = deepestRoute.StartsWith("selfservice/", StringComparison.OrdinalIgnoreCase);
								if (isSelfServiceRoute)
								{
									// Self-service routes live on SelfServiceShell; switch to it and navigate
									if (restorationStack.Count == 1)
									{
										var absRoute = deepestRoute.StartsWith("//") ? deepestRoute : "//" + deepestRoute;
										await App.SwitchToSelfServiceShellAsync(absRoute);
									}
									else
									{
										var firstRoute = restorationStack[0];
										var absFirst = (firstRoute?.StartsWith("//") == true) ? firstRoute : "//" + firstRoute;
										await App.SwitchToSelfServiceShellAsync(absFirst);
										for (int i = 1; i < restorationStack.Count; i++)
										{
											var route = restorationStack[i];
											if (!string.IsNullOrEmpty(route))
											{
												await Shell.Current.GoToAsync(route);
												await Task.Delay(200);
											}
										}
									}
								}
								else
								{
									// Main app shell
									if (restorationStack.Count == 1)
									{
										await GoToAsyncOrMainAsync(deepestRoute);
									}
									else
									{
										for (int i = 0; i < restorationStack.Count; i++)
										{
											var route = restorationStack[i];
											if (i == 0)
											{
												if (route?.StartsWith("///") == true)
													await Shell.Current.GoToAsync(route);
												else
													await Shell.Current.GoToAsync($"///MainPage/{route}");
											}
											else
											{
												await Shell.Current.GoToAsync(route);
												await Task.Delay(200);
											}
										}
									}
								}
								
								Config.WaitBeforeStart = 1;
								Config.SaveAppStatus();
								return;
							}
							else
							{
								// Restoration route is invalid, clear it and continue with normal flow
								System.Diagnostics.Debug.WriteLine("[SplashPage] ActivityState restoration: Invalid route, clearing state and using normal flow");
								ActivityState.RemoveAll();
							}
						}
					}
				}

				if (Config.ButlerCustomization)
				{
					if (!Config.ButlerSignedIn)
					{
						await GoToAsyncOrMainAsync("bottlelogin");
					}
					else
					{
						await GoToAsyncOrMainAsync("///MainPage");
					}

					Config.WaitBeforeStart = 1;
					Config.SaveAppStatus();
					return;
				}

				if (Config.EnableSelfServiceModule)
				{
					// Self-service: check signed-out-with-companies first so reopen after sign-out goes to SelectCompany (not main login)
					if (SelfServiceCompany.List != null && SelfServiceCompany.List.Count >= 1 && !Config.SignedInSelfService)
					{
						await App.SwitchToSelfServiceShellAsync("//selfservice/selectcompany");
					}
					else if (Config.SignedInSelfService && Client.Clients != null && Client.Clients.Count > 0)
					{
						// Already signed in: client list or straight to checkout (single client)
						if (Client.Clients.Count > 1)
						{
							await App.SwitchToSelfServiceShellAsync("//selfservice/clientlist");
						}
						else
						{
							// Count == 1: go straight to checkout (never show client list)
							var client = Client.Clients.FirstOrDefault();
							client?.EnsureInvoicesAreLoaded();
							client?.EnsurePreviouslyOrdered();
							var order = Order.Orders.FirstOrDefault(x => x.Client?.ClientId == client?.ClientId);
							if (order == null && client != null)
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
							if (order != null)
								await App.SwitchToSelfServiceShellAsync($"//selfservice/checkout?orderId={order.OrderId}");
							else
								await App.SwitchToSelfServiceShellAsync("//selfservice/clientlist");
						}
					}
					else
					{
						// Need to login: main app login or self-service select company
						if (Config.EnableLogin)
							await GoToAsyncOrMainAsync("login");
						else if (SelfServiceCompany.List != null && SelfServiceCompany.List.Count >= 1)
							await App.SwitchToSelfServiceShellAsync("//selfservice/selectcompany");
						else
						{
							var route = Config.EnableAdvancedLogin ? "newlogin" : "loginconfig";
							await GoToAsyncOrMainAsync(route);
						}
					}
				}
				else
				{
					await GoToAsyncOrMainAsync("///MainPage");
				}
			}
			else
			{
				// Not signed in: in self-service mode go to self-service shell (select company) instead of main app login
				if (Config.EnableSelfServiceModule)
				{
					if (SelfServiceCompany.List != null && SelfServiceCompany.List.Count >= 1)
						await App.SwitchToSelfServiceShellAsync("//selfservice/selectcompany");
					else
					{
						var route = Config.EnableAdvancedLogin ? "newlogin" : "loginconfig";
						await GoToAsyncOrMainAsync(route);
					}
				}
				else if (Config.EnableLogin)
				{
					await GoToAsyncOrMainAsync("login");
				}
				else
				{
					var route = Config.EnableAdvancedLogin ? "newlogin" : "loginconfig";
					await GoToAsyncOrMainAsync(route);
				}
			}

			Config.WaitBeforeStart = 1;
			Config.SaveAppStatus();
		}

		private static async Task GoToAsyncOrMainAsync(string route)
		{
			try
			{
				await Shell.Current.GoToAsync(route);
			}
			catch(Exception ex)
			{
				// Fallback if route isn't registered yet
				await Shell.Current.GoToAsync("///MainPage");
			}
		}
	}

	public static partial class PlatformAppInfo
	{
		public static partial string GetVersatileDexVersion();
	}
}

