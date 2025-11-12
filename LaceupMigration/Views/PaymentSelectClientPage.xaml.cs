using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaceupMigration.Views
{
    public partial class PaymentSelectClientPage : ContentPage, IQueryAttributable
    {
        private readonly PaymentSelectClientPageViewModel _viewModel;

        public PaymentSelectClientPage()
        {
            InitializeComponent();
            _viewModel = App.Services.GetRequiredService<PaymentSelectClientPageViewModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // No query parameters needed for this page
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (sender is SearchBar searchBar)
            {
                searchBar.Unfocus();
            }
        }

        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is ClientItemViewModel item)
            {
                await _viewModel.SelectClientAsync(item.ClientId);
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}

