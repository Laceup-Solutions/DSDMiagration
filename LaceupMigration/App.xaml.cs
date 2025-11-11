using LaceupMigration.Controls;
using MauiIcons.Core;

namespace LaceupMigration
{
    public partial class App : Application
	{
		public static IServiceProvider Services { get; private set; } = default!;
		private readonly AppShell _appShell;

		public App(IServiceProvider serviceProvider, AppShell appShell)
		{
			if (Current != null)
				Current.UserAppTheme = AppTheme.Light;

			_ = new MauiIcon();

			InitializeComponent();

			Services = serviceProvider;
			_appShell = appShell;

			Config.helper = serviceProvider.GetService<IInterfaceHelper>();
			CurrentScanner.scanner = serviceProvider.GetService<IScannerService>();
			DialogHelper._dialogService = serviceProvider.GetService<IDialogService>();
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			return new Window(_appShell);
		}
	}
}