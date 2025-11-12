using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SelectGoalPage : ContentPage
    {
        private readonly SelectGoalPageViewModel _viewModel;

        public SelectGoalPage(SelectGoalPageViewModel viewModel)
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
    }
}

