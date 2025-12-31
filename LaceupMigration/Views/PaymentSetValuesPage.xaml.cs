using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            
            if (component != null && !component.IsReadOnly)
            {
                await _viewModel.EditPayment(component);
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            PaymentComponentViewModel? component = null;
            
            // Get the BindingContext from the Border or its parent
            if (sender is TapGestureRecognizer recognizer)
            {
                // The recognizer's parent should be the Border
                if (recognizer.Parent is Border border)
                {
                    component = border.BindingContext as PaymentComponentViewModel;
                }
                else
                {
                    // Walk up the visual tree to find the Border or Frame with PaymentComponentViewModel
                    var parent = recognizer.Parent;
                    while (parent != null)
                    {
                        if (parent is Border borderParent)
                        {
                            component = borderParent.BindingContext as PaymentComponentViewModel;
                            break;
                        }
                        if (parent is Frame frameParent)
                        {
                            component = frameParent.BindingContext as PaymentComponentViewModel;
                            break;
                        }
                        if (parent is Element element)
                        {
                            parent = element.Parent;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else if (sender is Border border)
            {
                component = border.BindingContext as PaymentComponentViewModel;
            }
            else if (sender is Element element)
            {
                // Walk up the visual tree to find an element with PaymentComponentViewModel as BindingContext
                var current = element;
                while (current != null)
                {
                    if (current.BindingContext is PaymentComponentViewModel vm)
                    {
                        component = vm;
                        break;
                    }
                    current = current.Parent;
                }
            }
            
            if (component != null)
            {
                if (!component.IsReadOnly)
                {
                    await _viewModel.TakePhotoForComponentAsync(component);
                }
                else if (component.HasImage)
                {
                    // If read-only but has image, show the image
                    await _viewModel.ViewImageAsync(component);
                }
            }
        }

        private void AmountEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is PaymentComponentViewModel component)
            {
                // Use a small delay to ensure Android's text processing is complete
                // This prevents the EmojiCompat error
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(50); // Small delay to let Android finish processing
                    
                    // Parse and validate the amount when field loses focus
                    if (double.TryParse(entry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var amount))
                    {
                        // Update AmountText which will trigger OnAmountTextChanged and update Amount
                        // Format to 2 decimal places
                        var formattedAmount = amount.ToString("F2");
                        if (component.AmountText != formattedAmount)
                        {
                            component.AmountText = formattedAmount;
                        }
                        // SaveState is called in OnComponentAmountChanged
                    }
                    else if (string.IsNullOrWhiteSpace(entry.Text))
                    {
                        // If empty, set to 0 - this will trigger hiding of the component
                        component.AmountText = "0.00";
                    }
                    else
                    {
                        // Reset to current amount if parsing fails
                        var currentFormatted = component.Amount.ToString("F2");
                        if (component.AmountText != currentFormatted)
                        {
                            component.AmountText = currentFormatted;
                        }
                    }
                });
            }
        }
    }
}

