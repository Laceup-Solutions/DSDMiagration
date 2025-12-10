using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class SelectPriceLevelPage : ContentPage, IQueryAttributable
    {
        private readonly SelectPriceLevelPageViewModel _viewModel;

        public SelectPriceLevelPage(SelectPriceLevelPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
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

