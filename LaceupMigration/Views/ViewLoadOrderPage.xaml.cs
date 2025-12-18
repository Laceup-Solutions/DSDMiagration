using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ViewLoadOrderPage 
    {
        private readonly ViewLoadOrderPageViewModel _viewModel;

        public ViewLoadOrderPage(ViewLoadOrderPageViewModel viewModel)
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

