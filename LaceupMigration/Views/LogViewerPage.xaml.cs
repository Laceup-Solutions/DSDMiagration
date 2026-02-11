using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class LogViewerPage 
    {
        public LogViewerPage(LogViewerPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override string? GetRouteName() => "logviewer";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is LogViewerPageViewModel viewModel)
            {
                await viewModel.LoadLogAsync();
            }
        }
    }
}

