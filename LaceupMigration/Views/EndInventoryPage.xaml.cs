using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndInventoryPage : LaceupContentPage
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
            
            // [ACTIVITY STATE]: Save navigation state for this protected screen
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("endinventory");
            
            await _viewModel.OnAppearingAsync();
        }

        protected override string? GetRouteName() => "endinventory";

        protected override async void GoBack()
        {
            if (await _viewModel.OnBackButtonPressed())
                return;
            base.GoBack();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterInventory(e.NewTextValue);
        }

        private async void OnQuantityButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is EndInventoryItemViewModel itemViewModel)
            {
                await _viewModel.ShowEditQuantityDialog(itemViewModel);
            }
        }
    }
}

