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
            await _viewModel.OnAppearingAsync();
        }
    }
}

