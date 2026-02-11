using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SalesProductCatReportPage 
    {
        private readonly SalesProductCatReportPageViewModel _viewModel;

        public SalesProductCatReportPage(SalesProductCatReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "salesproductcatreport";
    }
}

