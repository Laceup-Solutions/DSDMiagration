using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SetupPrinterPage 
    {
        private readonly SetupPrinterPageViewModel _viewModel;

        protected override string? GetRouteName() => "setupprinter";

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

