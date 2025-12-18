using LaceupMigration.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.Views
{
    public partial class RouteReturnsPage 
    {
        private readonly RouteReturnsPageViewModel _viewModel;

        public RouteReturnsPage(RouteReturnsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this protected screen
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("routereturns");
            
            await _viewModel.OnAppearingAsync();
        }
        
        protected override bool OnBackButtonPressed()
        {
            // Handle back button - call ViewModel's OnBackButtonPressed
            // Return true to prevent default back navigation, false to allow it
            // Note: We need to block here because OnBackButtonPressed must return synchronously
            bool preventNavigation = _viewModel.OnBackButtonPressed().GetAwaiter().GetResult();
            
            // [ACTIVITY STATE]: If navigation is allowed, remove state
            if (!preventNavigation)
            {
                Helpers.NavigationHelper.RemoveNavigationState("routereturns");
            }
            
            return preventNavigation;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterReturns(e.NewTextValue);
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is RouteReturnViewModel returnViewModel)
            {
                if (float.TryParse(e.NewTextValue, out var qty))
                {
                    returnViewModel.Quantity = qty;
                }
            }
        }
    }
}

