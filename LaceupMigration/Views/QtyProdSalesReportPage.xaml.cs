using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class QtyProdSalesReportPage 
    {
        private readonly QtyProdSalesReportPageViewModel _viewModel;

        public QtyProdSalesReportPage(QtyProdSalesReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

