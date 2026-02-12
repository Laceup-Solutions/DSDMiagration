using LaceupMigration.ViewModels.SelfService;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCheckOutPage 
    {
        public SelfServiceCheckOutPage(SelfServiceCheckOutPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            UseCustomMenu = true; // Single Menu toolbar item only (Sync Data, Advanced Options, Sign Out)
            // Hide back arrow when only one client (set early so it applies before first paint)
            if (Client.Clients.Count == 1)
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false, IsEnabled = false });
        }

        protected override string? GetRouteName() => "selfservice/checkout";

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCheckOutPageViewModel vm)
            {
                vm.OnAppearing();
            }
            // Re-apply back button hide when only one client (in case Client.Clients loaded after constructor)
            if (Client.Clients.Count == 1)
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false, IsEnabled = false });
        }

        /// <summary>When only one client, prevent back so we don't navigate to client list (stack should be Splash -> Login -> Checkout only).</summary>
        protected override bool OnBackButtonPressed()
        {
            if (Client.Clients.Count == 1)
                return true; // Handled: do not navigate back
            return base.OnBackButtonPressed();
        }
    }
}

