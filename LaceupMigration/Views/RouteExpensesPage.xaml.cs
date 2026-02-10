using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class RouteExpensesPage 
    {
        private readonly RouteExpensesPageViewModel _viewModel;

        public RouteExpensesPage(RouteExpensesPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "routeexpenses";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

