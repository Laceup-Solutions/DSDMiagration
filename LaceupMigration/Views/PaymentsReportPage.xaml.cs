using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class PaymentsReportPage 
    {
        private readonly PaymentsReportPageViewModel _viewModel;

        public PaymentsReportPage(PaymentsReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "paymentsreport";
    }
}

