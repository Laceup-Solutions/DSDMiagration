using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class CommissionReportPage : ContentPage
    {
        private readonly CommissionReportPageViewModel _viewModel;

        public CommissionReportPage(CommissionReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

