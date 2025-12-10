using LaceupMigration.ViewModels;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class InventorySummaryPage : ContentPage
    {
        private readonly InventorySummaryPageViewModel _viewModel;

        public InventorySummaryPage(InventorySummaryPageViewModel viewModel)
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

