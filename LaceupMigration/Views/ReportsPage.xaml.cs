using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ReportsPage 
    {
        public ReportsPage(ReportsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

