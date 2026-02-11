using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SalesReportPage 
    {
        private readonly SalesReportPageViewModel _viewModel;

        public SalesReportPage(SalesReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "salesreport";
    }
}

