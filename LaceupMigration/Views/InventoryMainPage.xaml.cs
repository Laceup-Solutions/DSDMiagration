using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class InventoryMainPage : ContentPage
    {
        private readonly InventoryMainPageViewModel _viewModel;

        public InventoryMainPage(InventoryMainPageViewModel viewModel)
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

