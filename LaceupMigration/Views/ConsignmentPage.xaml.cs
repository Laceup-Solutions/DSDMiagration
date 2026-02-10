using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ConsignmentPage : IQueryAttributable
    {
        private readonly ConsignmentPageViewModel _viewModel;

        public ConsignmentPage(ConsignmentPageViewModel viewModel)
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

        protected override string? GetRouteName() => "consignment";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var orderId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId));
                }
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "consignment";
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
            Helpers.NavigationHelper.RemoveNavigationState("consignment");
            return false; // Allow navigation
        }
    }
}
