using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class AddPOSToRoutePage : ContentPage
    {
        private readonly AddPOSToRoutePageViewModel _viewModel;

        public AddPOSToRoutePage(AddPOSToRoutePageViewModel viewModel)
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
            _viewModel.FilterClients(e.NewTextValue);
        }
    }
}

