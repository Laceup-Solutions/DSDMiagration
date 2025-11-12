using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class LoadOrderReportPage : ContentPage
    {
        private readonly LoadOrderReportPageViewModel _viewModel;

        public LoadOrderReportPage(LoadOrderReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

