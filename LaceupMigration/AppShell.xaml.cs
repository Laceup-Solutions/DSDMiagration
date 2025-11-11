using LaceupMigration.ViewModels;

namespace LaceupMigration
{
    public partial class AppShell : Shell
    {
        private readonly MainPageViewModel _mainPageViewModel;

        public AppShell(MainPageViewModel mainPageViewModel)
        {
            InitializeComponent();

            _mainPageViewModel = mainPageViewModel;

            // Wire up the menu toolbar item
            if (MenuToolbarItem != null)
            {
                MenuToolbarItem.Command = _mainPageViewModel.ShowMenuCommand;
            }

			Routing.RegisterRoute("loginconfig", typeof(LoginConfigPage));
			Routing.RegisterRoute("termsandconditions", typeof(TermsAndConditionsPage));

			// Register tab routes
			Routing.RegisterRoute("Clients", typeof(ClientsPage));
			Routing.RegisterRoute("Invoices", typeof(InvoicesPage));
			Routing.RegisterRoute("Orders", typeof(OrdersPage));
			Routing.RegisterRoute("Payments", typeof(PaymentsPage));
        }
    }
}
