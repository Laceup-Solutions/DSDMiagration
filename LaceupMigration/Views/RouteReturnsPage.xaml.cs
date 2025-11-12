using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class RouteReturnsPage : ContentPage
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
            await _viewModel.OnAppearingAsync();
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

