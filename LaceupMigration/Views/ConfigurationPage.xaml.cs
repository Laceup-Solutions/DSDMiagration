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

        protected override string? GetRouteName() => "configuration";

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

