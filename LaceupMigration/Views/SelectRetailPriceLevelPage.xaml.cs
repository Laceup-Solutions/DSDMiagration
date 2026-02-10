using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SelectRetailPriceLevelPage 
    {
        private readonly SelectRetailPriceLevelPageViewModel _viewModel;

        public SelectRetailPriceLevelPage(SelectRetailPriceLevelPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "selectretailpricelevel";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterRetailPriceLevels(e.NewTextValue);
        }
    }
}

