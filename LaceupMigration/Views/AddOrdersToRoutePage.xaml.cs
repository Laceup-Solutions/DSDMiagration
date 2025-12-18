using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class AddOrdersToRoutePage 
    {
        private readonly AddOrdersToRoutePageViewModel _viewModel;

        public AddOrdersToRoutePage(AddOrdersToRoutePageViewModel viewModel)
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
            _viewModel.FilterOrders(e.NewTextValue);
        }
    }
}

