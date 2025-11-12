using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SelectPriceLevelPage : ContentPage
    {
        private readonly SelectPriceLevelPageViewModel _viewModel;

        public SelectPriceLevelPage(SelectPriceLevelPageViewModel viewModel)
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

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterPriceLevels(e.NewTextValue);
        }
    }
}

