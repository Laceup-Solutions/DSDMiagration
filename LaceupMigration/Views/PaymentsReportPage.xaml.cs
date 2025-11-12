using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class PaymentsReportPage : ContentPage
    {
        private readonly PaymentsReportPageViewModel _viewModel;

        public PaymentsReportPage(PaymentsReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

