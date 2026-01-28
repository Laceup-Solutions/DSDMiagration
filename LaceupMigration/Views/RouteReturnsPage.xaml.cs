using LaceupMigration.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.Views
{
    public partial class RouteReturnsPage : LaceupContentPage
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
        
        /// <summary>Both physical and nav bar back use this; ask ViewModel, then remove state and navigate if allowed.</summary>
        /// <remarks>Must async/await (not GetAwaiter().GetResult()) to avoid deadlock: dialogs in OnBackButtonPressed need the UI thread.</remarks>
        protected override async void GoBack()
        {
            bool preventNavigation = await _viewModel.OnBackButtonPressed();
            if (preventNavigation)
                return;
            Helpers.NavigationHelper.RemoveNavigationState("routereturns");
            base.GoBack();
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

