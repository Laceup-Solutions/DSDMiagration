using Microsoft.Maui.Controls.Shapes;

namespace LaceupMigration.Controls;

// In your MAUI project
using Microsoft.Maui.Controls;

public class DialogService : IDialogService
{
    private ContentPage _loadingPage;

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

    public async Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var tcs = new TaskCompletionSource<DateTime?>();
        var selectedDate = initialDate ?? DateTime.Now;
        
        var datePicker = new DatePicker
        {
            Date = selectedDate,
            Format = "MM/dd/yyyy"
        };

        if (minimumDate.HasValue)
            datePicker.MinimumDate = minimumDate.Value;
        if (maximumDate.HasValue)
            datePicker.MaximumDate = maximumDate.Value;

        var confirmButton = new Button
        {
            Text = "OK",
            BackgroundColor = Colors.Blue,
            TextColor = Colors.White,
            Margin = new Thickness(10, 5)
        };
        confirmButton.Clicked += async (s, e) =>
        {
            tcs.SetResult(datePicker.Date);
            await page.Navigation.PopModalAsync();
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            Margin = new Thickness(10, 5)
        };
        cancelButton.Clicked += async (s, e) =>
        {
            tcs.SetResult(null);
            await page.Navigation.PopModalAsync();
        };

        var datePickerPage = new ContentPage
        {
            Title = title,
            BackgroundColor = Color.FromArgb("#80000000")
        };

        datePickerPage.Content = new Grid
        {
            Children =
            {
                new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeShape = new RoundRectangle() { CornerRadius = 10 },
                    Padding = new Thickness(20),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 300,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 15,
                        Children =
                        {
                            new Label
                            {
                                Text = title,
                                FontSize = 18,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            datePicker,
                            new HorizontalStackLayout
                            {
                                Spacing = 10,
                                HorizontalOptions = LayoutOptions.Center,
                                Children = { confirmButton, cancelButton }
                            }
                        }
                    }
                }
            }
        };

        await page.Navigation.PushModalAsync(datePickerPage);
        return await tcs.Task;
    }

    
    public async Task ShowLoadingAsync(string message = "Loading...")
    {
        if (_loadingPage != null)
            return; // Already showing

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
                    WidthRequest = 150,
                    HeightRequest = 100,
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
                            new Label
                            {
                                Text = message,
                                FontSize = 14,
                                TextColor = Colors.Gray,
                                HorizontalOptions = LayoutOptions.Center
                            }
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
    }
    
    private Page GetCurrentPage()
    {
        // For .NET MAUI 9.0+
        return Application.Current?.Windows?.FirstOrDefault()?.Page;
        
        // Alternative for Shell applications
        // return Shell.Current?.CurrentPage;
    }
    
    
}