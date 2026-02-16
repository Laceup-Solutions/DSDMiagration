using LaceupMigration.Controls;
using MauiIcons.Core;

namespace LaceupMigration
{
	public partial class App : Application
	{
		public static IServiceProvider Services { get; private set; } = default!;
		private readonly AppShell _appShell;
		private static AppShell? _mainShellRef;

		public App(IServiceProvider serviceProvider, AppShell appShell)
		{
			if (Current != null)
				Current.UserAppTheme = AppTheme.Light;

			try
			{
				_ = new MauiIcon();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MIGRATION] MauiIcon init: {ex.Message}");
			}

			InitializeComponent();

			Services = serviceProvider;
			_appShell = appShell;
			_mainShellRef = appShell;

			Config.helper = serviceProvider.GetService<IInterfaceHelper>();
			CurrentScanner.scanner = serviceProvider.GetService<IScannerService>();
			DialogHelper._dialogService = serviceProvider.GetService<IDialogService>();

			try
			{
				Config.Initialize();
			}
			catch (Exception ex)
			{
				// Log but don't crash - directories will be created on-demand if this fails
				System.Diagnostics.Debug.WriteLine($"[MIGRATION] Config.Initialize() error: {ex.Message}");
			}

		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			var window = new Window(_appShell);

			Helpers.NavigationTracker.Initialize(_appShell);
			
			return window;
		}

		/// <summary>
		/// Switch to the Self Service shell and optionally navigate to a route.
		/// Use when Config.EnableSelfServiceModule and after login or from splash.
		/// Removes login pages from ActivityState so they are not in the stack when the app closes and reopens.
		/// </summary>
		public static async Task SwitchToSelfServiceShellAsync(string route)
		{
			// Remove login from navigation state so on app close/reopen we don't restore or show login in the stack
			RemoveLoginFromActivityState();

			var shell = Services.GetRequiredService<Views.SelfService.SelfServiceShell>();
			Current!.MainPage = shell;
			try
			{
				await shell.GoToAsync(route);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[SelfService] SwitchToSelfServiceShellAsync navigation failed: {ex.Message}");
			}
		}

		/// <summary>
		/// Removes all login-related ActivityState entries so they are not persisted or restored when in self-service.
		/// </summary>
		private static void RemoveLoginFromActivityState()
		{
			var loginTypes = new[] { "LoginActivity", "LoginConfigActivity", "NewLoginActivity", "BottleLoginActivity" };
			foreach (var activityType in loginTypes)
			{
				var state = ActivityState.GetState(activityType);
				if (state != null)
				{
					ActivityState.RemoveState(state);
					System.Diagnostics.Debug.WriteLine($"[SelfService] Removed {activityType} from ActivityState");
				}
			}
		}

		public static void SwitchToMainShell()
		{
			if (_mainShellRef != null)
				Current!.MainPage = _mainShellRef;
		}
	}
}