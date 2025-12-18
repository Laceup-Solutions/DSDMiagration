using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class EndOfDayProcessPage 
    {
        private readonly EndOfDayProcessPageViewModel _viewModel;

        public EndOfDayProcessPage(EndOfDayProcessPageViewModel viewModel)
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

