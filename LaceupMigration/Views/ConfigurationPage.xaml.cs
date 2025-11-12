using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ConfigurationPage : ContentPage
    {
        private readonly ConfigurationPageViewModel _viewModel;

        public ConfigurationPage(ConfigurationPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

