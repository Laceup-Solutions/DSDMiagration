using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class BatchDepartmentPage : ContentPage, IQueryAttributable
    {
        private readonly BatchDepartmentPageViewModel _viewModel;

        public BatchDepartmentPage(BatchDepartmentPageViewModel viewModel)
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
            int clientId = 0;
            int batchId = 0;

            if (query.TryGetValue("clientId", out var clientIdValue) && clientIdValue != null)
            {
                int.TryParse(clientIdValue.ToString(), out clientId);
            }

            if (query.TryGetValue("batchId", out var batchIdValue) && batchIdValue != null)
            {
                int.TryParse(batchIdValue.ToString(), out batchId);
            }

            if (clientId > 0 && batchId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(clientId, batchId));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
