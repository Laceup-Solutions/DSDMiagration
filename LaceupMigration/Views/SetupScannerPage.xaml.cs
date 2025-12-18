using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SetupScannerPage 
    {
        private readonly SetupScannerPageViewModel _viewModel;

        public SetupScannerPage(SetupScannerPageViewModel viewModel)
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

