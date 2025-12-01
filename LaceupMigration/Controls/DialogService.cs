using Microsoft.Maui.Controls.Shapes;
using LaceupMigration;

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

    public async Task<(string qty, string comments, UnitOfMeasure selectedUoM)> ShowAddItemDialogAsync(string productName, Product product, string initialQty = "1", string initialComments = "", UnitOfMeasure initialUoM = null)
    {
        var page = GetCurrentPage();
        if (page == null)
            return (null, null, null);

        var tcs = new TaskCompletionSource<(string qty, string comments, UnitOfMeasure selectedUoM)>();

        // Simple layout matching Xamarin's catalogAddLayout - just Qty and Comments for load orders
        // Quantity input
        var qtyEntry = new Entry
        {
            Text = initialQty,
            Keyboard = Keyboard.Numeric,
            FontSize = 16,
            BackgroundColor = Colors.White
        };

        // Comments input
        var commentsEntry = new Editor
        {
            Text = initialComments,
            Placeholder = "Enter comments",
            HeightRequest = 80,
            FontSize = 14,
            BackgroundColor = Colors.White
        };

        // Unit of Measure selector (if product has UoMFamily and Config allows it)
        Picker uomPicker = null;
        UnitOfMeasure selectedUoM = initialUoM;
        
        if (product != null && !string.IsNullOrEmpty(product.UoMFamily) && Config.CanChangeUomInLoad)
        {
            var familyItems = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily).ToList();
            if (familyItems.Count > 0)
            {
                uomPicker = new Picker
                {
                    FontSize = 16,
                    BackgroundColor = Colors.White,
                    ItemsSource = familyItems,
                    ItemDisplayBinding = new Binding("Name")
                };

                if (initialUoM != null)
                {
                    var index = familyItems.FindIndex(x => x.Id == initialUoM.Id);
                    if (index >= 0)
                        uomPicker.SelectedIndex = index;
                }
                else
                {
                    var defaultIndex = familyItems.FindIndex(x => x.IsDefault);
                    if (defaultIndex >= 0)
                        uomPicker.SelectedIndex = defaultIndex;
                }

                uomPicker.SelectedIndexChanged += (s, e) =>
                {
                    if (uomPicker.SelectedIndex >= 0 && uomPicker.SelectedIndex < familyItems.Count)
                        selectedUoM = familyItems[uomPicker.SelectedIndex];
                };

                if (uomPicker.SelectedIndex >= 0)
                    selectedUoM = familyItems[uomPicker.SelectedIndex];
            }
        }

        // Simple content layout matching Xamarin - horizontal layout for Qty, vertical for Comments
        var qtyRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            Padding = new Thickness(20, 15, 20, 10)
        };

        var qtyLabel = new Label
        {
            Text = "Qty:",
            FontSize = 14,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(qtyLabel, 0);
        Grid.SetColumn(qtyEntry, 1);
        qtyRow.Children.Add(qtyLabel);
        qtyRow.Children.Add(qtyEntry);

        var commentsRow = new VerticalStackLayout
        {
            Spacing = 5,
            Padding = new Thickness(20, 10, 20, 5)
        };

        var commentsLabel = new Label
        {
            Text = "Comments:",
            FontSize = 14,
            TextColor = Colors.Black
        };
        commentsRow.Children.Add(commentsLabel);
        commentsRow.Children.Add(commentsEntry);

        // UoM row (if applicable)
        Grid uomRow = null;
        if (uomPicker != null)
        {
            uomRow = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Padding = new Thickness(20, 5, 20, 5)
            };

            var uomLabel = new Label
            {
                Text = "Unit of Measure:",
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(uomLabel, 0);
            Grid.SetColumn(uomPicker, 1);
            uomRow.Children.Add(uomLabel);
            uomRow.Children.Add(uomPicker);
        }

        // Create buttons row (match Xamarin: Cancel on left, Add on right)
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            FontSize = 16,
            Margin = new Thickness(0,0,5,10),
            CornerRadius = 0
        };

        var addButton = new Button
        {
            Text = "Add",
            BackgroundColor = Color.FromArgb("#017CBA"), // Match Laceup blue
            TextColor = Colors.White,
            FontSize = 16,
            Margin = new Thickness(5,0,0,10),
            CornerRadius = 0
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0),
            BackgroundColor = Colors.White
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(addButton, 1);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(addButton);

        // Main content - simple vertical stack
        var content = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Children = { qtyRow }
        };

        if (uomRow != null)
            content.Children.Add(uomRow);

        content.Children.Add(commentsRow);
        content.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") });
        content.Children.Add(buttonRow);

        // Create a popup-style dialog (centered overlay, not full page)
        var scrollContent = new ScrollView
        {
            Content = content,
            MaximumWidthRequest = 300, // Limit width for tablet/desktop
            MaximumHeightRequest = 500 // Limit height
        };

        // Main container with header and content
        var mainContainer = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Children =
            {
                new Label
                {
                    Text = productName,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    Padding = new Thickness(20, 20, 20, 15),
                    BackgroundColor = Colors.White
                },
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") },
                scrollContent
            }
        };

        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 0,
            Margin = new Thickness(20),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = mainContainer
        };

        // Create a grid with semi-transparent background and centered content
        var overlayGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"), // Semi-transparent black overlay
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        // Place dialog border in center of overlay
        Grid.SetRow(dialogBorder, 1);
        Grid.SetColumn(dialogBorder, 1);
        overlayGrid.Children.Add(dialogBorder);

        // Create modal page (full screen but styled as popup)
        var dialog = new ContentPage
        {
            BackgroundColor = Colors.Transparent, // Transparent so overlay shows through
            Content = overlayGrid
        };

        // Helper method to safely pop the modal
        async Task SafePopModalAsync()
        {
            try
            {
                var currentPage = GetCurrentPage();
                if (currentPage != null)
                {
                    // Check if we're on the dialog page or need to pop from the original page
                    if (currentPage == dialog && currentPage.Navigation.ModalStack.Count > 0)
                    {
                        await currentPage.Navigation.PopModalAsync();
                    }
                    else if (page != null && page.Navigation.ModalStack.Count > 0)
                    {
                        // Pop from the original page's navigation
                        await page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Modal might already be popped, ignore
                System.Diagnostics.Debug.WriteLine($"Error popping modal: {ex.Message}");
            }
        }

        addButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult((qtyEntry.Text, commentsEntry.Text, selectedUoM));
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult((null, null, null));
        };

        // Tap outside to close (tap on overlay background)
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            // Only close if tapping the overlay background, not the dialog content
            if (s == overlayGrid)
            {
                await SafePopModalAsync();
                tcs.SetResult((null, null, null));
            }
        };
        overlayGrid.GestureRecognizers.Add(tapGesture);

        // Show as modal
        await page.Navigation.PushModalAsync(dialog);
        
        // Focus and select all text in qty entry (match Xamarin: SetSelectAllOnFocus)
        qtyEntry.Focus();
        if (!string.IsNullOrEmpty(qtyEntry.Text))
        {
            qtyEntry.CursorPosition = 0;
            qtyEntry.SelectionLength = qtyEntry.Text.Length;
        }

        return await tcs.Task;
    }

    public async Task HideLoadingAsync()
    {
        if (_loadingPage == null)
            return;

        try
        {
            var currentPage = GetCurrentPage();
            if (currentPage != null && currentPage.Navigation != null)
            {
                // Check if there's actually a modal to pop
                if (currentPage.Navigation.ModalStack.Count > 0)
                {
                    await currentPage.Navigation.PopModalAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // Modal might already be popped or navigation state invalid, ignore
            System.Diagnostics.Debug.WriteLine($"Error hiding loading: {ex.Message}");
        }
        finally
        {
            _loadingPage = null;
            _loadingLabel = null;
        }
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