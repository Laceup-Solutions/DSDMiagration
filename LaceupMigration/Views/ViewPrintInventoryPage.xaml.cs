using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ViewPrintInventoryPage : ContentPage
    {
        private readonly ViewPrintInventoryPageViewModel _viewModel;

        public ViewPrintInventoryPage(ViewPrintInventoryPageViewModel viewModel)
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

