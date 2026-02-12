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

			// MauiIcons init can fail in release (e.g. font/reflection); don't crash app startup
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

			// [MIGRATION]: Initialize Config at app startup (matches Xamarin LaceupActivity.InitializeApplication)
			// This creates ALL directories (SessionPath, InvoicesPath, DataPath, etc.) BEFORE any pages load
			// This prevents "Could not find a part of the path" errors on Android tablet emulators
			// Must be called synchronously here to ensure directories exist before any file operations
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
			
			// [ACTIVITY STATE]: Initialize navigation tracking after window is created
			// Shell is now available
			Helpers.NavigationTracker.Initialize(_appShell);
			
			return window;
		}

		/// <summary>
		/// Switch to the Self Service shell and optionally navigate to a route.
		/// Use when Config.EnableSelfServiceModule and after login or from splash.
		/// </summary>
		public static async Task SwitchToSelfServiceShellAsync(string route)
		{
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
		/// Switch back to the main app shell (e.g. when signing out of self service).
		/// </summary>
		public static void SwitchToMainShell()
		{
			if (_mainShellRef != null)
				Current!.MainPage = _mainShellRef;
		}
	}
}