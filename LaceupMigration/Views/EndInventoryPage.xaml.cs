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
                    itemViewModel.EndingQuantity = qty;
                }
            }
        }
    }
}

