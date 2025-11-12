using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SelectTermsPage : ContentPage
    {
        private readonly SelectTermsPageViewModel _viewModel;

        public SelectTermsPage(SelectTermsPageViewModel viewModel)
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
            _viewModel.FilterTerms(e.NewTextValue);
        }
    }
}

