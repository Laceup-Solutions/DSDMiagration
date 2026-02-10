using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndOfDayPage : LaceupContentPage
    {
        private readonly EndOfDayPageViewModel _viewModel;

        public EndOfDayPage(EndOfDayPageViewModel viewModel)
        {
            InitializeComponent();

            OverrideBaseMenu = true;
            
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this page
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("endofday");
            
            await _viewModel.OnAppearingAsync();
        }

        protected override string? GetRouteName() => "endofday";

        protected override void GoBack()
        {
            if (!_viewModel.CanLeaveScreen)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return;
            }
            base.GoBack();
        }
    }
}

