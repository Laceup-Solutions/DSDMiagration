using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using LaceupMigration.Services;

namespace LaceupMigration.ViewModels
{
	public partial class SplashPageViewModel : ObservableObject
	{
		private readonly ILaceupAppService _appService;

		public SplashPageViewModel(ILaceupAppService appService)
		{
			_appService = appService;
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

					DataAccess.WaitBeforeStart = 1;
					Config.SaveAppStatus();
					return;
				}

				if (Config.EnableSelfServiceModule)
				{
					if (Config.EnableLogin)
					{
						await GoToAsyncOrMainAsync("login");
					}
					else
					{
						if (SelfServiceCompany.List != null && SelfServiceCompany.List.Count >= 1 && !Config.SignedInSelfService)
						{
							await GoToAsyncOrMainAsync("selfservice/selectcompany");
						}
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
				if (Config.EnableLogin)
				{
					await GoToAsyncOrMainAsync("login");
				}
				else
				{
					var route = Config.EnableAdvancedLogin ? "newlogin" : "loginconfig";
					await GoToAsyncOrMainAsync(route);
				}
			}

			DataAccess.WaitBeforeStart = 1;
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

