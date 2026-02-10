using LaceupMigration.ViewModels;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class InventorySummaryPage 
    {
        private readonly InventorySummaryPageViewModel _viewModel;

        public InventorySummaryPage(InventorySummaryPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "inventorysummary";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

