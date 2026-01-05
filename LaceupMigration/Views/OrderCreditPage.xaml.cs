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

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("asPresale", out var asPresaleValue) && asPresaleValue != null)
            {
                bool.TryParse(asPresaleValue.ToString(), out asPresale);
            }

            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, asPresale));
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

        /// <summary>
        /// Override GoBack to handle order finalization before navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("ordercredit");
            
            // Call ViewModel's DoneAsync which handles finalization logic
            // This is async, but GoBack() is synchronous, so we fire and forget
            _ = _viewModel.DoneAsync();
        }
    }
}
