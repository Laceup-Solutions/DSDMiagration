using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class PaymentSetValuesPage : IQueryAttributable
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

        protected override bool OnBackButtonPressed()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("paymentsetvalues");
            return false; // Allow navigation
        }

        private async void EditField_Clicked(object sender, EventArgs e)
        {
            PaymentComponentViewModel? component = null;
            string? fieldName = null;
            
            if (sender is Button button)
            {
                component = button.BindingContext as PaymentComponentViewModel;
                fieldName = button.CommandParameter?.ToString();
            }
            else if (sender is Frame frame)
            {
                component = frame.BindingContext as PaymentComponentViewModel;
            }
            
            if (component == null)
                return;

            // Route to the appropriate method based on the field name
            switch (fieldName)
            {
                case "PaymentMethod":
                    await _viewModel.EditPaymentMethod(component);
                    break;
                case "Amount":
                    await _viewModel.EditAmount(component);
                    break;
                case "Comments":
                    await _viewModel.EditComments(component);
                    break;
                case "RefNumber":
                    await _viewModel.EditRefNumber(component);
                    break;
                case "CreditCardNumber":
                    await _viewModel.EditCreditCardNumber(component);
                    break;
                case "BankName":
                    await _viewModel.EditBankName(component);
                    break;
                case "PostedDate":
                    await _viewModel.EditPostedDate(component);
                    break;
                default:
                    // Fallback to full edit if no parameter specified
                    await _viewModel.EditPayment(component);
                    break;
            }
        }

        private async void EditPayment_Clicked(object sender, EventArgs e)
        {
            PaymentComponentViewModel? component = null;
            
            if (sender is Button button)
            {
                component = button.BindingContext as PaymentComponentViewModel;
            }
            else if (sender is Frame frame)
            {
                component = frame.BindingContext as PaymentComponentViewModel;
            }
            
            if (component != null)
            {
                await _viewModel.EditPayment(component);
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            PaymentComponentViewModel? component = null;

            if (sender is Frame frame)
            {
                component = frame.BindingContext as PaymentComponentViewModel;
            }
            else if (sender is Image image)
            {
                component = image.BindingContext as PaymentComponentViewModel;
            }
            else if (sender is TapGestureRecognizer recognizer)
            {
                var frameParent = recognizer.Parent as Frame;
                if (frameParent != null)
                {
                    component = frameParent.BindingContext as PaymentComponentViewModel;
                }
            }

            if (component == null)
                return;

            // If image exists, view it; otherwise, show image source dialog (matches Xamarin behavior)
            if (component.HasImage && !string.IsNullOrEmpty(component.ImagePath))
            {
                await _viewModel.ViewPaymentImageAsync(component);
            }
            else
            {
                await _viewModel.ShowImageSourceDialog(component);
            }
        }
    }
}

