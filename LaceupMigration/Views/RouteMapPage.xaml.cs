using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class RouteMapPage : ContentPage
    {
        private readonly RouteMapPageViewModel _viewModel;

        public RouteMapPage(RouteMapPageViewModel viewModel)
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

