using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class FinalizeBatchPage : LaceupContentPage, IQueryAttributable
    {
        private readonly FinalizeBatchPageViewModel _viewModel;

        public FinalizeBatchPage(FinalizeBatchPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query == null)
                return;

            int clientId = 0;
            string ordersId = string.Empty;
            bool printed = false;

            if (query.TryGetValue("ordersId", out var ordersIdValue) && ordersIdValue != null)
            {
                var raw = ordersIdValue.ToString() ?? string.Empty;
                ordersId = string.IsNullOrEmpty(raw) ? raw : Uri.UnescapeDataString(raw);
            }
            
            if (query.TryGetValue("clientId", out var clientIdValue) && clientIdValue != null)
            {
                int.TryParse(clientIdValue.ToString(), out clientId);
            }

            if (query.TryGetValue("printed", out var printedValue) && printedValue != null)
            {
                if (int.TryParse(printedValue.ToString(), out var printedInt))
                {
                    printed = printedInt > 0;
                }
            }

            if (!string.IsNullOrEmpty(ordersId))
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(ordersId, clientId, printed));
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "finalizebatch";
            if (query != null && query.Count > 0)
            {
                var queryParams = query
                    .Where(kvp => kvp.Value != null)
                    .Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString())}")
                    .ToArray();
                if (queryParams.Length > 0)
                {
                    route += "?" + string.Join("&", queryParams);
                }
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>
        /// Override GoBack so the navigation bar back runs the same "can't leave until finalizing" check as the physical back button.
        /// When payment was collected or printing was done, block and show "You must click Done to leave this screen."
        /// </summary>
        /// <summary>
        /// Both physical and nav bar back use this. Block when payment/printed; otherwise remove state and navigate.
        /// </summary>
        protected override void GoBack()
        {
            if (!_viewModel.CanLeaveScreen)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return;
            }
            Helpers.NavigationHelper.RemoveNavigationState("finalizebatch");
            base.GoBack();
        }
    }
}

