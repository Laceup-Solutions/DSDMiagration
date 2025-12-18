using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class PrintReportsPage 
    {
        private readonly PrintReportsPageViewModel _viewModel;

        public PrintReportsPage(PrintReportsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

