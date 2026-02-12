using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class BatchPage : LaceupContentPage, IQueryAttributable
    {
        private readonly BatchPageViewModel _viewModel;

        public BatchPage(BatchPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Use custom menu (ViewModel-based) - this prevents LaceupContentPage from removing the menu item
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

        protected override string? GetRouteName() => "batch";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>Both physical and nav bar back use this; Xamarin BatchActivity OnKeyDown logic.</summary>
        protected override void GoBack()
        {
            if (!_viewModel.CanLeaveScreen)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return;
            }
            var allOrders = _viewModel.Orders.Select(x => x.Order).ToList();
            bool canLeave = allOrders.Count == 0 || (Config.CanLeaveBatch && allOrders.Any(x => !x.Finished));
            if (canLeave)
            {
                var batchId = _viewModel?.GetBatchId();
                if (batchId.HasValue)
                    Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={batchId.Value}");
                base.GoBack();
                return;
            }
            if (!allOrders.All(x => x.Voided || x.Finished))
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return;
            }
            // Xamarin: exclude No_Service from clock-out requirement - when only No_Service, can leave without clock out
            var validOrders = allOrders.Where(x => x.OrderType != OrderType.NoService).ToList();
            var batch = Batch.List.FirstOrDefault(x => x.Id == _viewModel.GetBatchId());
            if (batch != null && batch.ClockedOut == DateTime.MinValue && validOrders.Count > 0)
            {
                _ = _viewModel.ShowCannotLeaveDialog();
                return;
            }
            if (Config.ButlerCustomization && batch != null)
            {
                var voided = allOrders.Where(x => x.Voided).ToList();
                if (voided.Count > 0 && !voided.All(x => x.PrintedCopies > 0))
                {
                    _ = _viewModel.ShowCannotLeaveDialog();
                    return;
                }
            }
            var batchIdToRemove = _viewModel?.GetBatchId();
            if (batchIdToRemove.HasValue)
                Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={batchIdToRemove.Value}");
            base.GoBack();
        }
    }
}
