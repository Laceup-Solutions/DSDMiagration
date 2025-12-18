using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class BatchPage : IQueryAttributable
    {
        private readonly BatchPageViewModel _viewModel;

        public BatchPage(BatchPageViewModel viewModel)
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
            if (query.TryGetValue("batchId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var batchId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(batchId));
                }
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "batch";
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
            // Xamarin BatchActivity OnKeyDown logic (lines 3686-3747)
            // Check if we can leave based on canLeaveScreen and order states
            
            // If canLeaveScreen is false (operation in progress), prevent navigation
            if (!_viewModel.CanLeaveScreen)
            {
                // Show dialog asynchronously (fire and forget)
                _ = _viewModel.ShowCannotLeaveDialog();
                return true; // Prevent navigation
            }
            
            // Check if we can leave based on Xamarin logic:
            // canLeave = adapter.Source.Count == 0 || (Config.CanLeaveBatch && adapter.Source.Any(x => !x.Finished))
            var allOrders = _viewModel.Orders.Select(x => x.Order).ToList();
            bool canLeave = allOrders.Count == 0 || (Config.CanLeaveBatch && allOrders.Any(x => !x.Finished));
            
            if (canLeave)
            {
                // Xamarin: If canGetOut || canLeave, delete batch if empty and finish
                var batchId = _viewModel?.GetBatchId();
                if (batchId.HasValue)
                {
                    Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={batchId.Value}");
                }
                return false; // Allow navigation
            }
            
            // Check if all orders are finalized or voided
            if (!allOrders.All(x => x.Voided || x.Finished))
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return true; // Prevent navigation
            }
            
            // Check if batch is clocked out
            var batch = Batch.List.FirstOrDefault(x => x.Id == _viewModel.GetBatchId());
            if (batch != null && batch.ClockedOut == DateTime.MinValue)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return true; // Prevent navigation
            }
            
            // ButlerCustomization check: all voided orders must be printed
            if (Config.ButlerCustomization && batch != null)
            {
                var voided = allOrders.Where(x => x.Voided).ToList();
                if (voided.Count > 0 && !voided.All(x => x.PrintedCopies > 0))
                {
                    // Show dialog asynchronously (fire and forget)
                    _ = _viewModel.ShowCannotLeaveDialog();
                    return true; // Prevent navigation
                }
            }
            
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            var batchIdToRemove = _viewModel?.GetBatchId();
            if (batchIdToRemove.HasValue)
            {
                Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={batchIdToRemove.Value}");
            }
            
            // Allow default back navigation
            return false;
        }
    }
}
