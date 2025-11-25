using LaceupMigration.ViewModels;
using System.Linq;
using System;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadPage : ContentPage, IQueryAttributable
    {
        private readonly AcceptLoadPageViewModel _viewModel;

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
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeWithDateAsync(date));
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
