using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndInventoryPage : ContentPage
    {
        private readonly EndInventoryPageViewModel _viewModel;

        public EndInventoryPage(EndInventoryPageViewModel viewModel)
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

        protected override bool OnBackButtonPressed()
        {
            // Handle back button - call ViewModel's OnBackButtonPressed
            // Return true to prevent default back navigation, false to allow it
            bool preventNavigation = _viewModel.OnBackButtonPressed().GetAwaiter().GetResult();
            return preventNavigation;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterInventory(e.NewTextValue);
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is EndInventoryItemViewModel itemViewModel)
            {
                if (float.TryParse(e.NewTextValue, out var qty))
                {
                    _viewModel.UpdateQuantity(itemViewModel, qty);
                }
            }
        }
    }
}

