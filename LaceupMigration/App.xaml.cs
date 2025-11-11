using LaceupMigration.Controls;

namespace LaceupMigration
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            if(Current != null)
                Current.UserAppTheme = AppTheme.Light;

            InitializeComponent();

            Config.helper = serviceProvider.GetService<IInterfaceHelper>();
            CurrentScanner.scanner = serviceProvider.GetService<IScannerService>();
            DialogHelper._dialogService = serviceProvider.GetService<DialogService>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}