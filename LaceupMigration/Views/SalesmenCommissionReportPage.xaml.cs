using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SalesmenCommissionReportPage : ContentPage
    {
        private readonly SalesmenCommissionReportPageViewModel _viewModel;

        public SalesmenCommissionReportPage(SalesmenCommissionReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

