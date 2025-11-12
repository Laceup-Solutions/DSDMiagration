using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SAPOrderReportPage : ContentPage
    {
        private readonly SAPOrderReportPageViewModel _viewModel;

        public SAPOrderReportPage(SAPOrderReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

