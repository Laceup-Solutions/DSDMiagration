using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SetupPrinterPage : ContentPage
    {
        private readonly SetupPrinterPageViewModel _viewModel;

        public SetupPrinterPage(SetupPrinterPageViewModel viewModel)
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

