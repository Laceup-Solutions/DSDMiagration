using LaceupMigration.ViewModels;
using System.Linq;
using System;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadPage : ContentPage, IQueryAttributable
    {
        private readonly AcceptLoadPageViewModel _viewModel;
        private DateTime? _lastInitializedDate; // Track the last date we initialized with

        public AcceptLoadPage(AcceptLoadPageViewModel viewModel)
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
        }

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

        protected override bool OnBackButtonPressed()
        {
            // Match Xamarin: OnKeyDown - when back button is pressed, delete pending loads
            // Match Xamarin AcceptLoadOrderList.cs line 234-247
            // In Xamarin: if (keyCode == Keycode.Back) { ActivityState.RemoveState(state); DataAccess.DeletePengingLoads(); }
            _viewModel.DeletePendingLoads();
            return base.OnBackButtonPressed();
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
