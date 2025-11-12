using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class RouteExpensesPage : ContentPage
    {
        private readonly RouteExpensesPageViewModel _viewModel;

        public RouteExpensesPage(RouteExpensesPageViewModel viewModel)
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

