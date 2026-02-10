using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class OrderDetailsPage : IQueryAttributable
    {
        private readonly OrderDetailsPageViewModel _viewModel;

        public OrderDetailsPage(OrderDetailsPageViewModel viewModel)
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

        protected override string? GetRouteName() => "orderdetails";

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
            var route = "orderdetails";
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

        protected override bool OnBackButtonPressed()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("orderdetails");
            return false; // Allow navigation
        }
    }
}
