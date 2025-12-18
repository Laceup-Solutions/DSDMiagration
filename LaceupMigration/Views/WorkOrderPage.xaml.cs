using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class WorkOrderPage : IQueryAttributable
    {
        private readonly WorkOrderPageViewModel _viewModel;

        public WorkOrderPage(WorkOrderPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int clientId = 0;
            int orderId = 0;
            bool asPresale = false;

            if (query.TryGetValue("clientId", out var clientIdValue) && clientIdValue != null)
            {
                int.TryParse(clientIdValue.ToString(), out clientId);
            }

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("asPresale", out var asPresaleValue) && asPresaleValue != null)
            {
                bool.TryParse(asPresaleValue.ToString(), out asPresale);
            }

            if (clientId > 0 && orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(clientId, orderId, asPresale));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
