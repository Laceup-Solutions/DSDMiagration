using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class CycleCountPage 
    {
        private readonly CycleCountPageViewModel _viewModel;

        public CycleCountPage(CycleCountPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this page
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("cyclecount");
            
            await _viewModel.OnAppearingAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("cyclecount");
            return false; // Allow navigation
        }
    }
}

