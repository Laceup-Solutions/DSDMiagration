using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class TransmissionReportPage 
    {
        private readonly TransmissionReportPageViewModel _viewModel;

        public TransmissionReportPage(TransmissionReportPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}

