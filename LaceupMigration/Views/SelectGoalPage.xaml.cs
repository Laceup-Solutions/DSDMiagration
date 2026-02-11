using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SelectGoalPage 
    {
        private readonly SelectGoalPageViewModel _viewModel;

        public SelectGoalPage(SelectGoalPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "goals";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterGoals(e.NewTextValue);
        }
    }
}

