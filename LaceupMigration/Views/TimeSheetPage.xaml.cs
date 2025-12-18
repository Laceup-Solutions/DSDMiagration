using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class TimeSheetPage 
    {
        private readonly TimeSheetPageViewModel _viewModel;

        public TimeSheetPage(TimeSheetPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();
        }
    }
}
