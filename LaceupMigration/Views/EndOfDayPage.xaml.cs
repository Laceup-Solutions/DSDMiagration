using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndOfDayPage : ContentPage
    {
        private readonly EndOfDayPageViewModel _viewModel;

        public EndOfDayPage(EndOfDayPageViewModel viewModel)
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
    }
}

