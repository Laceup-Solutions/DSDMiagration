using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndOfDayPage : LaceupContentPage
    {
        private readonly EndOfDayPageViewModel _viewModel;

        public EndOfDayPage(EndOfDayPageViewModel viewModel)
        {
            InitializeComponent();

            OverrideBaseMenu = true;
            
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this page
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("endofday");
            
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>
        /// Override GoBack to handle back navigation logic for both physical and navigation bar back buttons.
        /// This includes checking if the user can leave the screen and removing navigation state.
        /// </summary>
        protected override void GoBack()
        {
            // Xamarin EndOfDayActivity OnKeyDown logic (lines 1049-1064)
            // Prevent back navigation if canLeaveScreen is false
            if (!_viewModel.CanLeaveScreen)
            {
                // Show dialog asynchronously (fire and forget)
                _ = _viewModel.ShowCannotLeaveDialog();
                return; // Prevent navigation
            }
            
            // [ACTIVITY STATE]: Remove state when properly exiting
            Helpers.NavigationHelper.RemoveNavigationState("endofday");
            
            // Navigate back
            base.GoBack();
        }
    }
}

