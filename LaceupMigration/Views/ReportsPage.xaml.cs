using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ReportsPage : ContentPage
    {
        public ReportsPage(ReportsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

