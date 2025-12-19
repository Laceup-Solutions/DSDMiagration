using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class FinalizeBatchPage : IQueryAttributable
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

            string ordersId = string.Empty;
            bool printed = false;

            if (query.TryGetValue("ordersId", out var ordersIdValue) && ordersIdValue != null)
            {
                ordersId = ordersIdValue.ToString() ?? string.Empty;
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
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(ordersId, printed));
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

        protected override bool OnBackButtonPressed()
        {
            // Xamarin FinalizeBatchActivity OnKeyDown logic (lines 759-789)
            // Prevent back navigation if payment was collected or printing was done
            if (!_viewModel.CanLeaveScreen)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return true; // Prevent navigation
            }

            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("finalizebatch");

            return false; // Allow navigation
        }
    }
}

