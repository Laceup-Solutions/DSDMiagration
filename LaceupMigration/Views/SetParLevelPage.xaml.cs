using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class SetParLevelPage 
    {
        private readonly SetParLevelPageViewModel _viewModel;

        public SetParLevelPage(SetParLevelPageViewModel viewModel)
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

