using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ConfigurationPage : LaceupContentPage
    {
        private readonly ConfigurationPageViewModel _viewModel;

        public ConfigurationPage(ConfigurationPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Override menu completely - only show SAVE option
            UseCustomMenu = true;
        }

        /// <summary>
        /// Override GoBack to clear navigation state when navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            // This prevents the app from restoring to ConfigurationPage after closing it
            Helpers.NavigationHelper.RemoveNavigationState("configuration");
            
            // Navigate back
            base.GoBack();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this page
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("configuration");
            
            await _viewModel.OnAppearingAsync();
        }
    }
}

