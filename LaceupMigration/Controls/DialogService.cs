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
#if ANDROID
        // Use custom Android implementation with separators and reduced margins
        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity != null)
            {
                return await LaceupMigration.Platforms.Android.ActionSheetService.ShowActionSheetAsync(
                    activity, title, message, cancelText, buttons);
            }
        }
        catch (Exception ex)
        {
            // Fallback to default implementation on error
            System.Diagnostics.Debug.WriteLine($"Error showing custom action sheet: {ex.Message}");
        }
#endif
        // Default implementation for other platforms or fallback
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

    /// <summary>
    /// Custom quantity prompt dialog that automatically selects all text when opened.
    /// This makes it easier to edit quantity values.
    /// </summary>
    public async Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "Yes", string cancelText = "No")
    {
        return await ShowConfirmationAsync(title, message, acceptText, cancelText);
    }

    public async Task<int> ShowSelectionAsync(string title, string[] options)
    {
        var page = GetCurrentPage();
        if (page != null && options != null)
        {
            // Always show the popup, even if options is empty (will show just "Cancel")
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
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        var addButton = new Button
        {
            Text = "Add",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        // Create separator line between buttons
        var buttonSeparator = new BoxView
        {
            WidthRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 1 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 0), // No margin right, left, or bottom
            BackgroundColor = Colors.White
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonSeparator, 1);
        Grid.SetColumn(addButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonSeparator);
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
            Padding = new Thickness(0),
            Margin = new Thickness(0,0,0,0),
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
                scrollContent,
                // Gray line on top of buttons with no margin or padding - outside scrollContent to avoid padding
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) },
                buttonRow
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
        // Don't add tap gesture - let user use Cancel button (matching Xamarin: SetCancelable(false))
        // Xamarin code has: responseDialogBuilder.SetCancelable(false);

        // Show as modal
        await page.Navigation.PushModalAsync(dialog);
        
        // Focus qty entry and select all text (matching Xamarin: SetSelectAllOnFocus)
        qtyEntry.Focus();
        if (!string.IsNullOrEmpty(qtyEntry.Text))
        {
            qtyEntry.CursorPosition = 0;
            qtyEntry.SelectionLength = qtyEntry.Text.Length;
        }

        return await tcs.Task;
    }

    public async Task<(string qty, UnitOfMeasure selectedUoM)> ShowTransferQtyDialogAsync(string productName, Product product, string initialQty = "1", UnitOfMeasure initialUoM = null, string buttonText = "Add")
    {
        var page = GetCurrentPage();
        if (page == null)
            return (null, null);

        var tcs = new TaskCompletionSource<(string qty, UnitOfMeasure selectedUoM)>();

        // Quantity input
        var qtyEntry = new Entry
        {
            Text = initialQty,
            Keyboard = Keyboard.Numeric,
            FontSize = 16,
            BackgroundColor = Colors.White
        };

        // Unit of Measure selector (if product has UoMFamily and Config allows it)
        Picker uomPicker = null;
        UnitOfMeasure selectedUoM = initialUoM;
        
        if (product != null && !string.IsNullOrEmpty(product.UoMFamily) && Config.CanChangeUoMInTransfer)
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

        // Qty row
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
                Text = "UoM:",
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(uomLabel, 0);
            Grid.SetColumn(uomPicker, 1);
            uomRow.Children.Add(uomLabel);
            uomRow.Children.Add(uomPicker);
        }

        // Create buttons row (Cancel on left, Add/Save on right)
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        var addButton = new Button
        {
            Text = buttonText,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        // Create separator line between buttons
        var buttonSeparator = new BoxView
        {
            WidthRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 1 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 0), // No margin right, left, or bottom
            BackgroundColor = Colors.White
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonSeparator, 1);
        Grid.SetColumn(addButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonSeparator);
        buttonRow.Children.Add(addButton);

        // Main content - vertical stack
        var content = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Children = { qtyRow }
        };

        if (uomRow != null)
            content.Children.Add(uomRow);

        // Create a popup-style dialog (centered overlay, not full page)
        var scrollContent = new ScrollView
        {
            Content = content,
            MaximumWidthRequest = 300,
            MaximumHeightRequest = 500
        };

        // Main container with header and content
        var mainContainer = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Padding = new Thickness(0),
            Margin = new Thickness(0,0,0,0),
            Children =
            {
                new Label
                {
                    Text = productName,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#017CBA"), // Blue text matching image
                    Padding = new Thickness(20, 20, 20, 15),
                    BackgroundColor = Colors.White
                },
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") },
                scrollContent,
                // Gray line on top of buttons with no margin or padding - outside scrollContent to avoid padding
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) },
                buttonRow
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
            BackgroundColor = Colors.Transparent,
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
                    if (currentPage == dialog && currentPage.Navigation.ModalStack.Count > 0)
                    {
                        await currentPage.Navigation.PopModalAsync();
                    }
                    else if (page != null && page.Navigation.ModalStack.Count > 0)
                    {
                        await page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error popping modal: {ex.Message}");
            }
        }

        addButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult((qtyEntry.Text, selectedUoM));
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult((null, null));
        };

        // Show as modal
        await page.Navigation.PushModalAsync(dialog);
        
        // Focus qty entry and select all text
        qtyEntry.Focus();
        if (!string.IsNullOrEmpty(qtyEntry.Text))
        {
            qtyEntry.CursorPosition = 0;
            qtyEntry.SelectionLength = qtyEntry.Text.Length;
        }

        return await tcs.Task;
    }

    public async Task<(int? priceLevelId, string price, string comments)> ShowPriceLevelDialogAsync(
        string productName, Product product, Order order, UnitOfMeasure uom, int currentPriceLevelSelected, string initialPrice = "", string initialComments = "")
    {
        // Get ProductPrice entries for this product
        var productPrices = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId).ToList();
        
        if (!productPrices.Any())
            return (null, null, null);

        var priceLevelOptions = new List<string>();
        var productPriceList = new List<ProductPrice>(); // Store ProductPrice entries to access price when selected
        var conversion = uom != null ? uom.Conversion : 1.0;
        
        // Build price level list from ProductPrice entries
        foreach (var pp in productPrices)
        {
            var priceLevel = PriceLevel.List.FirstOrDefault(x => x.Id == pp.PriceLevelId);
            if (priceLevel != null)
            {
                // Apply UoM conversion
                var convertedPrice = Math.Round(pp.Price * conversion, Config.Round);
                priceLevelOptions.Add($"{priceLevel.Name}: {convertedPrice.ToCustomString()}");
                productPriceList.Add(pp);
            }
        }

        if (!priceLevelOptions.Any())
            return (null, null, null);

        // Show selection popup
        var selectedIndex = await ShowSelectionAsync("Select Price Level", priceLevelOptions.ToArray());
        
        if (selectedIndex < 0 || selectedIndex >= productPriceList.Count)
            return (null, null, null);

        // Get selected price level and calculate price
        var selectedProductPrice = productPriceList[selectedIndex];
        var selectedPriceLevelId = selectedProductPrice.PriceLevelId;
        var calculatedPrice = Math.Round(selectedProductPrice.Price * conversion, Config.Round);
        var priceString = calculatedPrice.ToString("F2");

        // Optionally prompt for comments (if initial comments provided, use them; otherwise return empty)
        var comments = initialComments ?? "";

        return (selectedPriceLevelId, priceString, comments);
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
    
    public async Task<(List<int> selectedCategoryIds, bool selectAll, bool showPrice, bool showUPC, bool showUoM)?> ShowCatalogFilterDialogAsync(List<Category> categories)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var tcs = new TaskCompletionSource<(List<int> selectedCategoryIds, bool selectAll, bool showPrice, bool showUPC, bool showUoM)?>();
        
        var categoriesLabel = new Label
        {
            Text = "Categories:",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 10, 0, 8)
        };

        // Create checkboxes for categories
        var categoryCheckboxes = new List<(CheckBox checkbox, Label label, int categoryId)>();
        var allCategoriesCheckbox = new CheckBox
        {
            IsChecked = true
        };
        var allCategoriesLabel = new Label
        {
            Text = "All Categories",
            VerticalOptions = LayoutOptions.Center
        };

        // Add category checkboxes
        foreach (var category in categories)
        {
            var checkbox = new CheckBox
            {
                IsChecked = false
            };
            var label = new Label
            {
                Text = category.Name,
                VerticalOptions = LayoutOptions.Center
            };
            categoryCheckboxes.Add((checkbox, label, category.CategoryId));
        }

        // Create "Show In PDF" checkboxes
        var showPriceCheckbox = new CheckBox
        {
            IsChecked = true
        };
        var showPriceLabel = new Label
        {
            Text = "Show Price",
            VerticalOptions = LayoutOptions.Center
        };

        var showUPCCheckbox = new CheckBox
        {
            IsChecked = true
        };
        var showUPCLabel = new Label
        {
            Text = "Show UPC",
            VerticalOptions = LayoutOptions.Center
        };

        var showUoMCheckbox = new CheckBox
        {
            IsChecked = true
        };
        var showUoMLabel = new Label
        {
            Text = "Show UoM",
            VerticalOptions = LayoutOptions.Center
        };

        // Check if filters should be visible (matches Xamarin: DataAccess.CheckCommunicatorVersion >= "46.2.0")
        bool filtersVisible = Config.CheckCommunicatorVersion("46.2.0");

        // Handle price checkbox visibility/state (matches Xamarin logic)
        if (Config.DisableSendCatalogWithPrices || Config.HidePriceInTransaction)
        {
            showPriceCheckbox.IsEnabled = false;
            showPriceCheckbox.IsChecked = false;
            showPriceCheckbox.IsVisible = false;
        }

        // Category selection layout
        var categoryLayout = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(20, 0, 20, 0)
        };
        
        categoryLayout.Children.Add(categoriesLabel);
        
        var allCategoriesRow = new HorizontalStackLayout
        {
            Spacing = 2
        };
        allCategoriesRow.Children.Add(allCategoriesCheckbox);
        allCategoriesRow.Children.Add(allCategoriesLabel);
        categoryLayout.Children.Add(allCategoriesRow);

        foreach (var (checkbox, label, _) in categoryCheckboxes)
        {
            var row = new HorizontalStackLayout
            {
                Spacing = 8
            };
            row.Children.Add(checkbox);
            row.Children.Add(label);
            categoryLayout.Children.Add(row);
        }

        // Filters layout (Show In PDF options)
        var filtersLayout = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(20, 0, 20, 0),
            IsVisible = filtersVisible
        };

        var filtersLabel = new Label
        {
            Text = "Show In PDF:",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 0, 0, 8)
        };
        filtersLayout.Children.Add(filtersLabel);

        var showPriceRow = new HorizontalStackLayout
        {
            Spacing = 8
        };
        showPriceRow.Children.Add(showPriceCheckbox);
        showPriceRow.Children.Add(showPriceLabel);
        filtersLayout.Children.Add(showPriceRow);

        var showUPCRow = new HorizontalStackLayout
        {
            Spacing = 8
        };
        showUPCRow.Children.Add(showUPCCheckbox);
        showUPCRow.Children.Add(showUPCLabel);
        filtersLayout.Children.Add(showUPCRow);

        var showUoMRow = new HorizontalStackLayout
        {
            Spacing = 8
        };
        showUoMRow.Children.Add(showUoMCheckbox);
        showUoMRow.Children.Add(showUoMLabel);
        filtersLayout.Children.Add(showUoMRow);

        // Handle "All Categories" checkbox - when checked, disable individual checkboxes
        allCategoriesCheckbox.CheckedChanged += (s, e) =>
        {
            bool isChecked = allCategoriesCheckbox.IsChecked;
            foreach (var (checkbox, label, _) in categoryCheckboxes)
            {
                checkbox.IsEnabled = !isChecked;
                label.Opacity = isChecked ? 0.5 : 1.0; // Visual feedback for disabled state
                if (isChecked)
                    checkbox.IsChecked = false;
            }
        };

        // Scrollable content
        var scrollContent = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    categoryLayout,
                    new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0, 10) },
                    filtersLayout
                }
            }
        };

        // Main container - Use Grid to ensure buttons are always visible
        var mainContainer = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Auto }, // Separator
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Scrollable content
                new RowDefinition { Height = GridLength.Auto }, // Separator
                new RowDefinition { Height = GridLength.Auto }  // Buttons
            },
            BackgroundColor = Colors.White,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };

        var headerLabel = new Label
        {
            Text = "Filter",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Padding = new Thickness(20, 20, 20, 15),
            BackgroundColor = Colors.White
        };
        Grid.SetRow(headerLabel, 0);
        mainContainer.Children.Add(headerLabel);

        var headerSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") };
        Grid.SetRow(headerSeparator, 1);
        mainContainer.Children.Add(headerSeparator);

        Grid.SetRow(scrollContent, 2);
        mainContainer.Children.Add(scrollContent);

        // Buttons
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        var okButton = new Button
        {
            Text = "OK",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        // Create separator line between buttons
        var buttonSeparator = new BoxView
        {
            WidthRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 1 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 0), // No margin right, left, or bottom
            BackgroundColor = Colors.White
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonSeparator, 1);
        Grid.SetColumn(okButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonSeparator);
        buttonRow.Children.Add(okButton);

        // Gray line on top of buttons with no margin or padding
        var buttonSeparatorLine = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) };
        Grid.SetRow(buttonSeparatorLine, 3);
        mainContainer.Children.Add(buttonSeparatorLine);
        
        Grid.SetRow(buttonRow, 4);
        mainContainer.Children.Add(buttonRow);

        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = new Thickness(0),
            Margin = new Thickness(20),
            WidthRequest = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * 0.85,
            MaximumHeightRequest = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.9,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = mainContainer
        };

        var overlayGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
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

        Grid.SetRow(dialogBorder, 1);
        Grid.SetColumn(dialogBorder, 1);
        overlayGrid.Children.Add(dialogBorder);

        var dialog = new ContentPage
        {
            BackgroundColor = Colors.Transparent,
            Content = overlayGrid
        };

        async Task SafePopModalAsync()
        {
            try
            {
                var currentPage = GetCurrentPage();
                if (currentPage != null)
                {
                    if (currentPage == dialog && currentPage.Navigation.ModalStack.Count > 0)
                    {
                        await currentPage.Navigation.PopModalAsync();
                    }
                    else if (page != null && page.Navigation.ModalStack.Count > 0)
                    {
                        await page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error popping modal: {ex.Message}");
            }
        }

        okButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            
            bool selectAll = allCategoriesCheckbox.IsChecked;
            List<int> selectedCategoryIds = new List<int>();

            if (!selectAll)
            {
                foreach (var (checkbox, label, categoryId) in categoryCheckboxes)
                {
                    if (checkbox.IsChecked)
                        selectedCategoryIds.Add(categoryId);
                }
            }

            tcs.SetResult((selectedCategoryIds, selectAll, showPriceCheckbox.IsChecked, showUPCCheckbox.IsChecked, showUoMCheckbox.IsChecked));
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult(null);
        };

        await page.Navigation.PushModalAsync(dialog);
        return await tcs.Task;
    }

    /// <summary>
    /// Shows a single-choice dialog with radio buttons, matching Xamarin's SetSingleChoiceItems behavior.
    /// Returns the selected index, or -1 if canceled.
    /// </summary>
    public async Task<int> ShowSingleChoiceDialogAsync(string title, string[] options, int selectedIndex = 0)
    {
        var page = GetCurrentPage();
        if (page == null || options == null || options.Length == 0)
            return -1;

        var tcs = new TaskCompletionSource<int>();

        // Create main container
        var mainContainer = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Auto }, // Separator
                new RowDefinition { Height = GridLength.Auto }, // Options
                new RowDefinition { Height = GridLength.Auto }, // Separator for buttons
                new RowDefinition { Height = GridLength.Auto }  // Buttons
            },
            BackgroundColor = Colors.White,
            Padding = new Thickness(0)
        };

        // Header
        var headerLabel = new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#017CBA"), // Laceup blue
            Padding = new Thickness(20, 20, 20, 15),
            BackgroundColor = Colors.White
        };
        Grid.SetRow(headerLabel, 0);
        mainContainer.Children.Add(headerLabel);

        // Header separator
        var headerSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#017CBA") };
        Grid.SetRow(headerSeparator, 1);
        mainContainer.Children.Add(headerSeparator);

        // Options container with radio buttons
        var optionsContainer = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(0),
            BackgroundColor = Colors.White
        };

        int currentSelectedIndex = selectedIndex;
        var radioButtons = new List<RadioButton>();

        for (int i = 0; i < options.Length; i++)
        {
            // Create a grid to hold radio button and label with spacing
            var optionGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto }, // Radio button
                    new ColumnDefinition { Width = 2 }, // Spacing
                    new ColumnDefinition { Width = GridLength.Star } // Label
                },
                Padding = new Thickness(10, 5, 10, 5)
            };

            // Create radio button (without content, we'll add label separately)
            var radioButton = new RadioButton
            {
                IsChecked = i == selectedIndex,
                GroupName = "ActionOptions",
                VerticalOptions = LayoutOptions.Center
            };

            // Create label for the text
            var optionLabel = new Label
            {
                Text = options[i],
                FontSize = 16,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };

            // Add to grid
            Grid.SetColumn(radioButton, 0);
            Grid.SetColumn(optionLabel, 2);
            optionGrid.Children.Add(radioButton);
            optionGrid.Children.Add(optionLabel);

            int index = i; // Capture for closure
            radioButton.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    currentSelectedIndex = index;
                    // Uncheck other radio buttons
                    foreach (var rb in radioButtons)
                    {
                        if (rb != radioButton)
                            rb.IsChecked = false;
                    }
                }
            };

            radioButtons.Add(radioButton);
            optionsContainer.Children.Add(optionGrid);

            // Add separator line between options (except after last)
            if (i < options.Length - 1)
            {
                var separator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(20, 0) };
                optionsContainer.Children.Add(separator);
            }
        }

        Grid.SetRow(optionsContainer, 2);
        mainContainer.Children.Add(optionsContainer);

        // Button separator
        var buttonTopSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) };
        Grid.SetRow(buttonTopSeparator, 3);
        mainContainer.Children.Add(buttonTopSeparator);

        // Buttons
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.Black,
            FontSize = 16,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        var okButton = new Button
        {
            Text = "Ok",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 1 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0),
            BackgroundColor = Colors.White
        };

        var buttonSeparator = new BoxView
        {
            WidthRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonSeparator, 1);
        Grid.SetColumn(okButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonSeparator);
        buttonRow.Children.Add(okButton);

        Grid.SetRow(buttonRow, 4);
        mainContainer.Children.Add(buttonRow);

        // Set width to 80% of screen width (regardless of screen size)
        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 0,
            Margin = new Thickness(20),
            WidthRequest = screenWidth * 0.80,
            MaximumHeightRequest = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.9,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = mainContainer
        };

        // Overlay grid
        var overlayGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
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

        Grid.SetRow(dialogBorder, 1);
        Grid.SetColumn(dialogBorder, 1);
        overlayGrid.Children.Add(dialogBorder);

        var dialog = new ContentPage
        {
            BackgroundColor = Colors.Transparent,
            Content = overlayGrid
        };

        async Task SafePopModalAsync()
        {
            try
            {
                var currentPage = GetCurrentPage();
                if (currentPage != null)
                {
                    if (currentPage == dialog && currentPage.Navigation.ModalStack.Count > 0)
                    {
                        await currentPage.Navigation.PopModalAsync();
                    }
                    else if (page != null && page.Navigation.ModalStack.Count > 0)
                    {
                        await page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error popping modal: {ex.Message}");
            }
        }

        okButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult(currentSelectedIndex);
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult(-1);
        };

        // Don't allow canceling by tapping outside (matching Xamarin: SetCancelable(false))
        await page.Navigation.PushModalAsync(dialog);
        return await tcs.Task;
    }

    // Result class for RestOfTheAddDialog
    public class RestOfTheAddDialogResult
    {
        public float Qty { get; set; }
        public float Weight { get; set; }
        public string Lot { get; set; } = string.Empty;
        public DateTime? LotExpiration { get; set; }
        public string Comments { get; set; } = string.Empty;
        public double Price { get; set; }
        public UnitOfMeasure? SelectedUoM { get; set; }
        public bool IsFreeItem { get; set; }
        public bool UseLastSoldPrice { get; set; }
        public int ReasonId { get; set; }
        public int PriceLevelSelected { get; set; }
        public bool Cancelled { get; set; }
    }

    public async Task<RestOfTheAddDialogResult> ShowRestOfTheAddDialogAsync(
        Product product, 
        Order order, 
        OrderDetail? existingDetail = null,
        bool isCredit = false,
        bool isDamaged = false,
        bool isDelivery = false)
    {
        var page = GetCurrentPage();
        if (page == null)
            return new RestOfTheAddDialogResult { Cancelled = true };

        var tcs = new TaskCompletionSource<RestOfTheAddDialogResult>();

        // Initialize values from existing detail or defaults
        var initialQty = existingDetail != null ? existingDetail.Qty : (product.SoldByWeight && !order.AsPresale ? 0f : 1f);
        if (initialQty == 0 && !(Config.DeliveryReasonInLine && isDelivery && existingDetail != null && existingDetail.Ordered > 0))
            initialQty = 1f;

        var initialWeight = existingDetail != null ? existingDetail.Weight : 0f;
        var initialLot = existingDetail != null ? (existingDetail.Lot ?? string.Empty) : string.Empty;
        var initialComments = existingDetail != null ? (existingDetail.Comments ?? string.Empty) : string.Empty;
        var initialPrice = existingDetail != null ? existingDetail.Price : Product.GetPriceForProduct(product, order, isCredit, isDamaged);
        var initialUoM = existingDetail != null ? existingDetail.UnitOfMeasure : product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
        var initialFreeItem = existingDetail != null && existingDetail.IsFreeItem;
        var initialUseLSP = false;
        
        // Get last invoice detail from client history (matches Xamarin l.Line.LastInvoiceDetail)
        InvoiceDetail? lastInvoiceDetail = null;
        if (order.Client != null)
        {
            var clientHistory = InvoiceDetail.ClientProduct(order.Client.ClientId, product.ProductId);
            if (clientHistory != null && clientHistory.Count > 0)
            {
                lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
            }
        }
        
        if (Config.UseLSP && lastInvoiceDetail != null)
        {
            var previousPrice = lastInvoiceDetail.Price;
            initialUseLSP = Math.Abs(initialPrice - previousPrice) < 0.01;
        }
        var initialReasonId = existingDetail != null ? existingDetail.ReasonId : 0;
        var initialPriceLevelSelected = existingDetail != null ? (existingDetail.ExtraFields != null ? 
            int.TryParse(UDFHelper.GetSingleUDF("priceLevelSelected", existingDetail.ExtraFields), out var pl) ? pl : 0 : 0) : 0;

        // Create dialog content - this is a simplified version, full implementation would need all fields
        // For now, I'll create the essential fields and you can expand as needed
        var scrollView = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Padding = new Thickness(16, 12, 16, 12)
            },
            MaximumHeightRequest = 600 // Limit height so it can scroll if needed
        };

        var content = (VerticalStackLayout)scrollView.Content;

        // Title
        var titleLabel = new Label
        {
            Text = product.Name,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 0, 0, 8)
        };
        content.Children.Add(titleLabel);

        // Quantity/Weight Entry
        Entry qtyEntry = null;
        Label qtyLabel = null;
        if (!order.AsPresale && product.SoldByWeight)
        {
            qtyLabel = new Label { Text = "Weight:", FontSize = 14, TextColor = Colors.Black };
            qtyEntry = new Entry
            {
                Text = initialWeight.ToString("F2"),
                Keyboard = Keyboard.Numeric,
                FontSize = 16
            };
        }
        else
        {
            qtyLabel = new Label { Text = "Qty:", FontSize = 14, TextColor = Colors.Black };
            qtyEntry = new Entry
            {
                Text = initialQty.ToString("F0"),
                Keyboard = Config.DontAllowDecimalsInQty ? Keyboard.Numeric : Keyboard.Numeric,
                FontSize = 16
            };
        }

        var qtyRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            Margin = new Thickness(0, 0, 0, 4)
        };
        Grid.SetColumn(qtyLabel, 0);
        Grid.SetColumn(qtyEntry, 1);
        qtyRow.Children.Add(qtyLabel);
        qtyRow.Children.Add(qtyEntry);
        content.Children.Add(qtyRow);

        // Weight Entry (if EnterWeightInCredits)
        Entry weightEntry = null;
        if (Config.EnterWeightInCredits && product.SoldByWeight && order.AsPresale && isCredit)
        {
            var weightLabel = new Label { Text = "Weight:", FontSize = 14, TextColor = Colors.Black };
            weightEntry = new Entry
            {
                Text = initialWeight.ToString("F2"),
                Keyboard = Keyboard.Numeric,
                FontSize = 16
            };
            qtyEntry.IsEnabled = false;
            qtyEntry.Text = "1";

            var weightRow = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Margin = new Thickness(0, 0, 0, 4)
            };
            Grid.SetColumn(weightLabel, 0);
            Grid.SetColumn(weightEntry, 1);
            weightRow.Children.Add(weightLabel);
            weightRow.Children.Add(weightEntry);
            content.Children.Add(weightRow);
        }

        // Lot Entry
        Entry lotEntry = null;
        Button lotButton = null;
        Button expButton = null;
        if (!order.AsPresale && !isDamaged && (product.UseLot || product.UseLotAsReference))
        {
            if (product.UseLot)
            {
                // Product lot button
                lotButton = new Button
                {
                    Text = initialLot,
                    FontSize = 16,
                    BackgroundColor = Colors.LightGray
                };
                // TODO: Add lot selection logic
                content.Children.Add(new Label { Text = "Lot:", FontSize = 14, TextColor = Colors.Black, Margin = new Thickness(0, 4, 0, 2) });
                lotButton.Margin = new Thickness(0, 0, 0, 4);
                content.Children.Add(lotButton);

                if (Config.UseLotExpiration)
                {
                    expButton = new Button
                    {
                        Text = existingDetail?.LotExpiration != null && existingDetail.LotExpiration != DateTime.MinValue 
                            ? existingDetail.LotExpiration.ToShortDateString() 
                            : "",
                        FontSize = 16,
                        BackgroundColor = Colors.LightGray
                    };
                    // TODO: Add date picker logic
                    content.Children.Add(new Label { Text = "Expiration:", FontSize = 14, TextColor = Colors.Black, Margin = new Thickness(0, 4, 0, 2) });
                    expButton.Margin = new Thickness(0, 0, 0, 4);
                    content.Children.Add(expButton);
                }
            }
            else
            {
                // Simple lot entry
                var lotLabel = new Label { Text = "Lot:", FontSize = 14, TextColor = Colors.Black };
                lotEntry = new Entry
                {
                    Text = initialLot,
                    FontSize = 16
                };
                var lotRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetColumn(lotLabel, 0);
                Grid.SetColumn(lotEntry, 1);
                lotRow.Children.Add(lotLabel);
                lotRow.Children.Add(lotEntry);
                content.Children.Add(lotRow);
            }
        }

        // Comments
        Editor commentEntry = null;
        if (!Config.HideItemComment || (order.OrderType != OrderType.Order && order.OrderType != OrderType.Credit))
        {
            var commentLabel = new Label { Text = "Comments:", FontSize = 14, TextColor = Colors.Black, Margin = new Thickness(0, 4, 0, 2) };
            commentEntry = new Editor
            {
                Text = initialComments,
                Placeholder = "Enter comments",
                HeightRequest = 60,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 4)
            };
            content.Children.Add(commentLabel);
            content.Children.Add(commentEntry);
        }

        // Price Entry (if can change price)
            Entry priceEntry = null;
            bool canChangePrice = Config.CanChangePrice(order, product, isCredit);
            // Note: isVendor is a field in TemplateActivity, not a property on Order
            // For now, we'll just check canChangePrice
            if (canChangePrice)
            {
                var priceLabel = new Label { Text = "Price:", FontSize = 14, TextColor = Colors.Black };
                priceEntry = new Entry
                {
                    Text = initialPrice.ToString("F2"),
                    Keyboard = Keyboard.Numeric,
                    FontSize = 16
                };
                var priceRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetColumn(priceLabel, 0);
                Grid.SetColumn(priceEntry, 1);
                priceRow.Children.Add(priceLabel);
                priceRow.Children.Add(priceEntry);
                content.Children.Add(priceRow);
            }

        // UoM Spinner
            Picker uomPicker = null;
            UnitOfMeasure selectedUoM = initialUoM;
            if (!product.SoldByWeight && !string.IsNullOrEmpty(product.UoMFamily))
            {
                var familyItems = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily).ToList();
                if (familyItems.Count > 0)
                {
                    var uomLabel = new Label { Text = "Unit of Measure:", FontSize = 14, TextColor = Colors.Black, Margin = new Thickness(0, 4, 0, 2) };
                    uomPicker = new Picker
                    {
                        FontSize = 16,
                        ItemsSource = familyItems,
                        ItemDisplayBinding = new Binding("Name"),
                        Margin = new Thickness(0, 0, 0, 4)
                    };

                    if (initialUoM != null)
                    {
                        var index = familyItems.FindIndex(x => x.Id == initialUoM.Id);
                        if (index >= 0)
                            uomPicker.SelectedIndex = index;
                    }

                    uomPicker.SelectedIndexChanged += (s, e) =>
                    {
                        if (uomPicker.SelectedIndex >= 0 && uomPicker.SelectedIndex < familyItems.Count)
                            selectedUoM = familyItems[uomPicker.SelectedIndex];
                    };

                    content.Children.Add(uomLabel);
                    content.Children.Add(uomPicker);
                }
            }

        // Free Item Checkbox
            CheckBox freeItemCheckbox = null;
            if (order.OrderType == OrderType.Order && Config.AllowFreeItems)
            {
                freeItemCheckbox = new CheckBox
                {
                    IsChecked = initialFreeItem
                };
                var freeItemRow = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 4, 0, 4) };
                freeItemRow.Children.Add(freeItemCheckbox);
                freeItemRow.Children.Add(new Label { Text = "Free Item", FontSize = 14, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center });
                content.Children.Add(freeItemRow);

                if (priceEntry != null)
                {
                    freeItemCheckbox.CheckedChanged += (s, e) =>
                    {
                        priceEntry.IsEnabled = !freeItemCheckbox.IsChecked;
                        if (freeItemCheckbox.IsChecked)
                            priceEntry.Text = "0.00";
                    };
                }
            }

        // Use LSP Checkbox
        CheckBox useLspCheckbox = null;
        if (Config.UseLSP && lastInvoiceDetail != null)
        {
            useLspCheckbox = new CheckBox
            {
                IsChecked = initialUseLSP
            };
            var useLspRow = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 4, 0, 4) };
            useLspRow.Children.Add(useLspCheckbox);
            useLspRow.Children.Add(new Label { Text = "Use Last Sold Price", FontSize = 14, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center });
            content.Children.Add(useLspRow);

            if (priceEntry != null)
            {
                useLspCheckbox.CheckedChanged += (s, e) =>
                {
                    priceEntry.IsEnabled = !useLspCheckbox.IsChecked;
                    if (useLspCheckbox.IsChecked && lastInvoiceDetail != null)
                        priceEntry.Text = lastInvoiceDetail.Price.ToString("F2");
                };
            }
        }

        // Buttons with separator line above
        var topSeparator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 8, 0, 0)
        };
        content.Children.Add(topSeparator);

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            HeightRequest = 44,
            Margin = new Thickness(0)
        };

        var addButton = new Button
        {
            Text = "Add",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            HeightRequest = 44,
            Margin = new Thickness(0)
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 1 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 0,
            Margin = new Thickness(0, 0, 0, 0),
            HeightRequest = 44
        };

        var verticalSeparator = new BoxView
        {
            WidthRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(verticalSeparator, 1);
        Grid.SetColumn(addButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(verticalSeparator);
        buttonRow.Children.Add(addButton);
        content.Children.Add(buttonRow);

            // Create dialog
            var dialogBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                WidthRequest = 360,
                MaximumWidthRequest = 460,
                Padding = new Thickness(0),
                Margin = new Thickness(30, 20, 30, 20), // More left/right margin for separation
                Content = scrollView
            };

            var overlayGrid = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
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
                },
                Padding = new Thickness(10)
            };

            Grid.SetRow(dialogBorder, 1);
            Grid.SetColumn(dialogBorder, 1);
            overlayGrid.Children.Add(dialogBorder);

            var dialog = new ContentPage
            {
                BackgroundColor = Colors.Transparent,
                Content = overlayGrid
            };

            addButton.Clicked += async (s, e) =>
            {
                var result = new RestOfTheAddDialogResult();

                // Parse qty
                if (float.TryParse(qtyEntry.Text, out var qty))
                    result.Qty = qty;
                else
                    result.Qty = 0;

                // Parse weight
                if (weightEntry != null && float.TryParse(weightEntry.Text, out var weight))
                    result.Weight = weight;
                else
                    result.Weight = result.Qty; // Default to qty if no weight entry

                // Get lot
                if (lotEntry != null)
                    result.Lot = lotEntry.Text ?? string.Empty;
                else if (lotButton != null)
                    result.Lot = lotButton.Text ?? string.Empty;

                // Get comments
                result.Comments = commentEntry?.Text ?? string.Empty;

                // Parse price
                if (priceEntry != null && double.TryParse(priceEntry.Text, out var price))
                    result.Price = price;
                else
                    result.Price = initialPrice;

                // Get UoM
                result.SelectedUoM = selectedUoM;

                // Get checkboxes
                result.IsFreeItem = freeItemCheckbox?.IsChecked ?? false;
                result.UseLastSoldPrice = useLspCheckbox?.IsChecked ?? false;

                // TODO: Add reason and price level logic
                result.ReasonId = initialReasonId;
                result.PriceLevelSelected = initialPriceLevelSelected;

                result.Cancelled = false;

                await page.Navigation.PopModalAsync();
                tcs.SetResult(result);
            };

            cancelButton.Clicked += async (s, e) =>
            {
                await page.Navigation.PopModalAsync();
                tcs.SetResult(new RestOfTheAddDialogResult { Cancelled = true });
            };

        await page.Navigation.PushModalAsync(dialog);
        return await tcs.Task;
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