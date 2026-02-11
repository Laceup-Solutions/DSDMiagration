using LaceupMigration.ViewModels;
using System.Linq;
using System;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadPage : LaceupContentPage, IQueryAttributable
    {
        private readonly AcceptLoadPageViewModel _viewModel;
        private DateTime? _lastInitializedDate; // Track the last date we initialized with

        public AcceptLoadPage(AcceptLoadPageViewModel viewModel)
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

        public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query)
        {
            // Match Xamarin: activity.PutExtra("loadDate", date.Ticks.ToString());
            if (query.TryGetValue("loadDate", out var value) && value != null)
            {
                if (long.TryParse(value.ToString(), out var ticks))
                {
                    var date = new System.DateTime(ticks);
                    
                    // [MIGRATION]: Only initialize if this is a new date or first time
                    // This prevents re-initialization with an old date after refresh completes
                    // If we've already initialized with this date, skip to avoid resetting
                    if (!_lastInitializedDate.HasValue || _lastInitializedDate.Value.Date != date.Date)
                    {
                        _lastInitializedDate = date;
                        Dispatcher.Dispatch(async () => await _viewModel.InitializeWithDateAsync(date));
                    }
                }
            }

            // Handle orderId parameter (from newloadordertemplate route)
            // Match Xamarin: activity.PutExtra(NewLoadOrderTemplateActivity.orderIdIntent, order.OrderId);
            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                if (int.TryParse(orderIdValue.ToString(), out var orderId) && orderId > 0)
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeWithOrderIdAsync(orderId));
                }
            }

            // Handle needRefresh parameter (from AcceptLoadEditDelivery)
            // Match Xamarin: activity.PutExtra("needRefresh", refresh ? "1" : "0");
            // When needRefresh=true, call Refresh(true) which will go back to main if RouteOrdersCount == 0
            if (query.TryGetValue("needRefresh", out var needRefreshValue) && needRefreshValue != null)
            {
                if (needRefreshValue.ToString() == "1")
                {
                    // Match Xamarin: Refresh(true) - exit=true means go back to main if no more orders
                    Dispatcher.Dispatch(async () => await _viewModel.RefreshAsync(true));
                }
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "acceptload";
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

        protected override string? GetRouteName() => "acceptload";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
            
            // Wire up date picker visibility change
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.ShowDatePicker) && _viewModel.ShowDatePicker)
                {
                    // Focus the date picker to open native calendar
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
                    {
                        DatePickerControl.Focus();
                    });
                }
            };
        }

        /// <summary>Both physical and nav bar back use this; delete pending loads, remove state, then navigate.</summary>
        protected override void GoBack()
        {
            // Match Xamarin: OnKeyDown - when back button is pressed, delete pending loads
            _viewModel.DeletePendingLoads();
            var currentRoute = Shell.Current.CurrentState?.Location?.OriginalString ?? "";
            if (currentRoute.Contains("acceptload"))
                Helpers.NavigationHelper.RemoveNavigationState(currentRoute);
            else
                Helpers.NavigationHelper.RemoveNavigationState("acceptload");
            base.GoBack();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Match Xamarin: Handle force quit or app termination scenarios
            // OnDisappearing is called when the page is removed from the navigation stack,
            // including force quit scenarios where OnBackButtonPressed might not be called
            _viewModel.OnDisappearing();
        }

        private void SelectAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _viewModel.UpdateSelectAll(e.Value);
        }

        private void SelectAllLabel_Tapped(object sender, EventArgs e)
        {
            _viewModel.SelectAll = !_viewModel.SelectAll;
            _viewModel.UpdateSelectAll(_viewModel.SelectAll);
        }

        private void OrderCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is AcceptLoadOrderItemViewModel orderViewModel)
            {
                orderViewModel.IsSelected = e.Value;
            }
        }

        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            // Hide the date picker and notify the view model
            _viewModel.OnDateSelected(e.NewDate);
        }
    }
}
