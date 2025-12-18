using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class CheckInventoryPage 
    {
        private readonly CheckInventoryPageViewModel _viewModel;

        public CheckInventoryPage(CheckInventoryPageViewModel viewModel)
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

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            await _viewModel.OnDisappearing();
        }
    }
}

