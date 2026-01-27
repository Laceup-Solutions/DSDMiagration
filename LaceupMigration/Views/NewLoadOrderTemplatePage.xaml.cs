using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class NewLoadOrderTemplatePage : IQueryAttributable
    {
        private readonly NewLoadOrderTemplatePageViewModel _viewModel;

        public NewLoadOrderTemplatePage(NewLoadOrderTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Prevent LaceupContentPage from adding hamburger menu
            UseCustomMenu = true;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0;
            bool canGetOutIntent = false;
            int? clientIdIntent = null;

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("canGetOutIntent", out var canGetOutValue) && canGetOutValue != null)
            {
                var str = canGetOutValue.ToString();
                canGetOutIntent = str == "1" || str.ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("clientIdIntent", out var clientIdValue) && clientIdValue != null)
            {
                if (int.TryParse(clientIdValue.ToString(), out var cId))
                    clientIdIntent = cId;
            }

            // Only initialize if orderId is provided and greater than 0
            // This prevents re-initialization when navigating back if orderId is missing from query
            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, canGetOutIntent, clientIdIntent));
            }
            // Note: If orderId is 0, don't initialize - the ViewModel will handle this gracefully
            // if it's already initialized, or the user will need to navigate back manually
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "newloadordertemplate";
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
            Helpers.NavigationHelper.RemoveNavigationState("newloadordertemplate");
            return false; // Allow navigation
        }

        private async void QtyButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LoadOrderDetailViewModel item)
            {
                await _viewModel.QtyButtonCommand.ExecuteAsync(item);
            }
        }
    }
}

