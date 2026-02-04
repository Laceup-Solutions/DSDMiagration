using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace LaceupMigration.Views
{
    public partial class PaymentSetValuesPage : LaceupContentPage, IQueryAttributable
    {
        private readonly PaymentSetValuesPageViewModel _viewModel;

        public PaymentSetValuesPage()
        {
            InitializeComponent();
            _viewModel = App.Services.GetRequiredService<PaymentSetValuesPageViewModel>();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _ = _viewModel.OnNavigatedTo(query);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "paymentsetvalues";
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
            
            // After initialization, save temp file path to ActivityState
            // Use Dispatcher to ensure ViewModel is initialized
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                SaveTempFilePathToState();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Debug: Check button state
            if (DeleteButton != null)
            {
                System.Diagnostics.Debug.WriteLine($"OnAppearing: DeleteButton.IsEnabled = {DeleteButton.IsEnabled}");
                System.Diagnostics.Debug.WriteLine($"OnAppearing: CanDeletePayment = {_viewModel?.CanDeletePayment}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnAppearing: DeleteButton is null!");
            }
            
            // [ACTIVITY STATE]: Save state periodically to preserve progress
            // Match Xamarin PaymentSetValuesActivity: saves state on OnResume/OnPause
            // Only save if ViewModel has been initialized (temp file path is set)
            if (!string.IsNullOrEmpty(_viewModel.GetTempFilePath()))
            {
                _viewModel.SaveState();
                
                // Update ActivityState with current temp file path
                SaveTempFilePathToState();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // [ACTIVITY STATE]: Save state when leaving page to preserve progress
            // Match Xamarin PaymentSetValuesActivity: saves state on OnPause
            // Only save if ViewModel has been initialized (temp file path is set)
            if (!string.IsNullOrEmpty(_viewModel.GetTempFilePath()))
            {
                _viewModel.SaveState();
                
                // Update ActivityState with current temp file path
                SaveTempFilePathToState();
            }
        }

        /// <summary>
        /// Saves the temp file path to ActivityState.State to preserve progress across app restarts.
        /// Match Xamarin PaymentSetValuesActivity: saves temp file path in ActivityState.State["tempFilePath"]
        /// </summary>
        private void SaveTempFilePathToState()
        {
            var state = LaceupMigration.ActivityState.GetState("PaymentSetValuesActivity");
            if (state != null && state.State != null)
            {
                var tempFilePath = _viewModel.GetTempFilePath();
                if (!string.IsNullOrEmpty(tempFilePath))
                {
                    state.State["tempFilePath"] = tempFilePath;
                    LaceupMigration.ActivityState.Save();
                }
            }
        }

        /// <summary>Both physical and nav bar back use this; remove state then navigate.</summary>
        protected override void GoBack()
        {
            Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
            base.GoBack();
        }

        private static PaymentComponentViewModel? GetComponentFromSender(object sender)
        {
            if (sender is BindableObject bo)
                return bo.BindingContext as PaymentComponentViewModel;
            return null;
        }

        private async void PaymentMethod_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditPaymentMethodAsync(component);
        }

        private async void Amount_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditAmountAsync(component);
        }

        private async void Comments_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditCommentsAsync(component);
        }

        private async void Ref_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditRefAsync(component);
        }

        private async void PostedDate_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditPostedDateAsync(component);
        }

        private async void Bank_Clicked(object sender, EventArgs e)
        {
            var component = GetComponentFromSender(sender);
            if (component != null) await _viewModel.EditBankAsync(component);
        }

        private async void AddImage_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var component = button?.BindingContext as PaymentComponentViewModel;
            if (component == null) return;
            if (component.HasImage)
                await _viewModel.ViewPaymentImageAsync(component);
            else
                await _viewModel.AddPaymentImageAsync(component);
        }
    }
}

