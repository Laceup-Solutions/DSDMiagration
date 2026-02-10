using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.Views
{
    public partial class OrderCreditPage : LaceupContentPage, IQueryAttributable
    {
        private readonly OrderCreditPageViewModel _viewModel;

        public OrderCreditPage(OrderCreditPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        // Override to integrate ViewModel menu with base menu
        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            // Get menu options from ViewModel - they're already MenuOption type
            return _viewModel.BuildMenuOptions();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0;
            bool asPresale = false;
            bool fromOneDoc = false;

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("asPresale", out var asPresaleValue) && asPresaleValue != null)
            {
                var s = asPresaleValue.ToString();
                asPresale = s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
            }

            if (query.TryGetValue("fromOneDoc", out var fromOneDocValue) && fromOneDocValue != null)
            {
                var s = fromOneDocValue.ToString();
                fromOneDoc = s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
            }

            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, asPresale, fromOneDoc));
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "ordercredit";
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

        protected override string? GetRouteName() => "ordercredit";

        protected override void GoBack()
        {
            RemoveNavigationState();
            _ = _viewModel.DoneAsync();
        }
    }
}
