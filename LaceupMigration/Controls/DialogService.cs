using Microsoft.Maui.Controls.Shapes;

namespace LaceupMigration.Controls;

// In your MAUI project
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using LaceupMigration.Business.Interfaces;
#if ANDROID
using Microsoft.Extensions.DependencyInjection;
#endif

public class DialogService : IDialogService
{
    private ContentPage _loadingPage;
    private Label _loadingLabel;

    public async Task ShowAlertAsync(string message, string title = "Alert", string acceptText = "OK")
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            await page.DisplayAlert(title, message, acceptText);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No")
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            return await page.DisplayAlert(title, message, acceptText, cancelText);
        }
        return false;
    }

    public async Task<string> ShowActionSheetAsync(string title, string message, string cancelText, params string[] buttons)
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            return await page.DisplayActionSheet(title, message, cancelText, buttons);
        }
        return cancelText;
    }

    public async Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "", Keyboard keyboard = null)
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            return await page.DisplayPromptAsync(title, message, acceptText, cancelText, placeholder, maxLength, initialValue: initialValue, keyboard: keyboard);
        }
        return null;
    }

    public async Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "Yes", string cancelText = "No")
    {
        return await ShowConfirmationAsync(title, message, acceptText, cancelText);
    }

    public async Task<int> ShowSelectionAsync(string title, string[] options)
    {
        var page = GetCurrentPage();
        if (page != null && options != null && options.Length > 0)
        {
            var result = await page.DisplayActionSheet(title, "Cancel", null, options);
            if (string.IsNullOrEmpty(result) || result == "Cancel")
                return -1;

            var index = Array.IndexOf(options, result);
            return index;
        }
        return -1;
    }

    // public async Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null)
    // {
    //     var page = GetCurrentPage();
    //     if (page == null)
    //         return null;
    //
    //     // Check if we're already on main thread
    //     if (!MainThread.IsMainThread)
    //     {
    //         return await MainThread.InvokeOnMainThreadAsync(() => ShowDatePickerAsync(title, initialDate, minimumDate, maximumDate));
    //     }
    //
    //     var tcs = new TaskCompletionSource<DateTime?>();
    //     var selectedDate = initialDate ?? DateTime.Now;
    //     bool dateSelected = false;
    //     
    //     var datePicker = new DatePicker
    //     {
    //         Date = selectedDate,
    //         Format = "MM/dd/yyyy",
    //         MinimumDate = minimumDate ?? new DateTime(2020, 1, 1),
    //         MaximumDate = maximumDate ?? new DateTime(2025, 12, 31)
    //     };
    //
    //     // Handle date selection - auto-close and return selected date when native picker closes
    //     datePicker.DateSelected += async (s, e) =>
    //     {
    //         if (!dateSelected)
    //         {
    //             dateSelected = true;
    //             // Small delay to let native picker close
    //             await Task.Delay(300);
    //             if (!tcs.Task.IsCompleted)
    //             {
    //                 tcs.SetResult(e.NewDate);
    //             }
    //             if (page.Navigation.ModalStack.Count > 0)
    //             {
    //                 await page.Navigation.PopModalAsync();
    //             }
    //         }
    //     };
    //
    //     // Create a simple modal page with the date picker
    //     // Make it minimal so the native picker is the focus
    //     var datePickerPage = new ContentPage
    //     {
    //         Title = title ?? "Select Date",
    //         BackgroundColor = Colors.White,
    //         Content = new VerticalStackLayout
    //         {
    //             Padding = 20,
    //             Spacing = 20,
    //             Children = { datePicker }
    //         }
    //     };
    //
    //     // Auto-focus the date picker when page appears to open native calendar
    //     datePickerPage.Appearing += async (s, e) =>
    //     {
    //         await Task.Delay(200); // Small delay to ensure page is fully loaded
    //         MainThread.BeginInvokeOnMainThread(() =>
    //         {
    //             try
    //             {
    //                 datePicker.Focus();
    //             }
    //             catch
    //             {
    //                 // If focus fails, return null
    //                 if (!tcs.Task.IsCompleted)
    //                 {
    //                     tcs.SetResult(null);
    //                 }
    //             }
    //         });
    //     };
    //
    //     // Handle cancellation (user goes back) - return null
    //     datePickerPage.Disappearing += (s, e) =>
    //     {
    //         if (!tcs.Task.IsCompleted)
    //         {
    //             tcs.SetResult(null);
    //         }
    //     };
    //
    //     // Check if page supports modal navigation
    //     if (page.Navigation == null)
    //     {
    //         return null;
    //     }
    //
    //     // Check if there's already a modal open
    //     if (page.Navigation.ModalStack.Count > 0)
    //     {
    //         return null;
    //     }
    //
    //     // Push modal - use try-catch to handle any issues
    //     try
    //     {
    //         await page.Navigation.PushModalAsync(datePickerPage);
    //     }
    //     catch (Exception ex)
    //     {
    //         // If PushModalAsync fails, return null
    //         if (!tcs.Task.IsCompleted)
    //         {
    //             tcs.SetResult(null);
    //         }
    //         return null;
    //     }
    //
    //     // Wait for date selection with timeout
    //     var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
    //     var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
    //     
    //     if (completedTask == timeoutTask)
    //     {
    //         // Timeout - close modal and return null
    //         try
    //         {
    //             if (page.Navigation.ModalStack.Count > 0)
    //             {
    //                 await page.Navigation.PopModalAsync();
    //             }
    //         }
    //         catch { }
    //         return null;
    //     }
    //     
    //     return await tcs.Task;
    // }
    
    
    public async Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null)
    {
#if ANDROID
        // Use platform-specific Android date picker
        try
        {
            var datePickerService = App.Services?.GetService<LaceupMigration.Business.Interfaces.IDatePickerService>();
            if (datePickerService != null)
            {
                return await datePickerService.ShowDatePickerAsync(title, initialDate, minimumDate, maximumDate);
            }
        }
        catch
        {
            // Fall through to default implementation if service is not available
        }
#endif

        // Default implementation for other platforms or fallback
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var selectedDate = initialDate ?? DateTime.Now;
        
        // Use a simpler approach: show a prompt dialog for date input
        // Format: MM/dd/yyyy
        var dateString = await page.DisplayPromptAsync(
            title ?? "Select Date",
            "Enter date (MM/dd/yyyy):",
            "OK",
            "Cancel",
            selectedDate.ToString("MM/dd/yyyy"),
            keyboard: Keyboard.Default);

        if (string.IsNullOrEmpty(dateString) || dateString == "Cancel")
            return null;

        // Try to parse the date
        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            // Validate against min/max if provided
            if (minimumDate.HasValue && parsedDate < minimumDate.Value)
                return null;
            if (maximumDate.HasValue && parsedDate > maximumDate.Value)
                return null;
            
            return parsedDate;
        }

        return null;
    }

    
    public async Task ShowLoadingAsync(string message = "Loading...")
    {
        if (_loadingPage != null)
            return; // Already showing

        _loadingLabel = new Label
        {
            Text = message,
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center
        };

        _loadingPage = new ContentPage
        {
            BackgroundColor = Colors.Transparent
        };

        _loadingPage.Content = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"), // Semi-transparent overlay
            Children =
            {
                new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeShape = new RoundRectangle() { CornerRadius = 10 },
                    WidthRequest = 180,
                    HeightRequest = 120,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = new StackLayout
                    {
                        Spacing = 15,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new ActivityIndicator
                            {
                                IsRunning = true,
                                Color = Colors.Blue,
                                WidthRequest = 30,
                                HeightRequest = 30
                            },
                            _loadingLabel
                        }
                    }
                }
            }
        };

        var currentPage = GetCurrentPage();
        if (currentPage != null)
        {
            await currentPage.Navigation.PushModalAsync(_loadingPage);
        }
    }

    public async Task HideLoadingAsync()
    {
        if (_loadingPage == null)
            return;

        var currentPage = GetCurrentPage();
        if (currentPage != null)
        {
            await currentPage.Navigation.PopModalAsync();
        }

        _loadingPage = null;
        _loadingLabel = null;
    }

    public async Task UpdateLoadingMessageAsync(string message)
    {
        if (_loadingLabel != null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _loadingLabel.Text = message;
            });
        }
        else
        {
            // If loading page doesn't exist, show it with the new message
            await ShowLoadingAsync(message);
        }
    }
    
    private Page GetCurrentPage()
    {
        // Try Shell first (most common in MAUI)
        if (Shell.Current?.CurrentPage != null)
            return Shell.Current.CurrentPage;
        
        // Fallback to Application Windows
        return Application.Current?.Windows?.FirstOrDefault()?.Page;
    }
    
    
}