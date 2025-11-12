using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class BatchPage : ContentPage, IQueryAttributable
    {
        private readonly BatchPageViewModel _viewModel;

        public BatchPage(BatchPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("batchId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var batchId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(batchId));
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
