using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class ViewOrderStatusPage 
    {
        private readonly ViewOrderStatusPageViewModel _viewModel;

        public ViewOrderStatusPage(ViewOrderStatusPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "vieworderstatus";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (sender is SearchBar searchBar)
            {
                searchBar.Unfocus();
            }
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext == _viewModel && _viewModel != null)
            {
                // Only trigger if this is the "Select All" checkbox (not an item checkbox)
                // The ViewModel's SelectAll method has a guard to prevent loops
                _viewModel.SelectAllCommand.Execute(null);
            }
        }
    }
}

