using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadEditDeliveryPage : LaceupContentPage, IQueryAttributable
    {
        private readonly AcceptLoadEditDeliveryPageViewModel _viewModel;

        public AcceptLoadEditDeliveryPage(AcceptLoadEditDeliveryPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Match Xamarin: Intent.Extras.Get(ordersIdsIntent)
            string ordersIds = string.Empty;
            bool changed = false;
            bool inventoryAccepted = false;
            bool canLeave = true;
            string uniqueId = null;

            if (query.TryGetValue("orderIds", out var orderIdsValue))
            {
                var raw = orderIdsValue?.ToString() ?? string.Empty;
                // Shell passes query values URL-encoded (e.g. 3|4 becomes 3%7C4); decode so we get the pipe-separated list back
                ordersIds = string.IsNullOrEmpty(raw) ? raw : System.Uri.UnescapeDataString(raw);
            }

            if (query.TryGetValue("changed", out var changedValue))
                bool.TryParse(changedValue?.ToString(), out changed);

            if (query.TryGetValue("inventoryAccepted", out var acceptedValue))
                bool.TryParse(acceptedValue?.ToString(), out inventoryAccepted);

            if (query.TryGetValue("canLeave", out var canLeaveValue))
                bool.TryParse(canLeaveValue?.ToString(), out canLeave);

            if (query.TryGetValue("uniqueId", out var uniqueIdValue))
                uniqueId = uniqueIdValue?.ToString();

            Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(ordersIds, changed, inventoryAccepted, canLeave, uniqueId));
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "acceptloadeditdelivery";
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

        protected override string? GetRouteName() => "acceptloadeditdelivery";
    }
}

