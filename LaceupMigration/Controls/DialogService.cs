using Microsoft.Maui.Controls.Shapes;
using LaceupMigration;
using LaceupMigration.Helpers;
using MauiIcons.Core;
using MauiIcons.Material.Outlined;
using MauiIcons.Material; 
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
    /// <summary>When a scan action returns this value, the prompt closes immediately without filling the text (caller already handled add).</summary>
    public const string ScanResultAddedAndClose = "{{SCAN_ADDED_CLOSE}}";

    private ContentPage _loadingPage;
    private Label _loadingLabel;

    public async Task ShowAlertAsync(string message, string title = "Alert", string acceptText = "OK")
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            try
            {
                await page.DisplayAlert(title, message, acceptText);
            }
            catch (Exception ex)
            {
                // Log the error and try fallback
                System.Diagnostics.Debug.WriteLine($"Error showing alert on page: {ex.Message}");
                // Try using Application.Current.MainPage as fallback
                if (Application.Current?.MainPage != null && Application.Current.MainPage != page)
                {
                    try
                    {
                        await Application.Current.MainPage.DisplayAlert(title, message, acceptText);
                    }
                    catch
                    {
                        // If both fail, log it
                        System.Diagnostics.Debug.WriteLine($"Failed to show alert on MainPage as well");
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        else
        {
            // If GetCurrentPage returns null, try Application.Current.MainPage as fallback
            if (Application.Current?.MainPage != null)
            {
                try
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, acceptText);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing alert on MainPage: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // No page available - log error
                System.Diagnostics.Debug.WriteLine($"Cannot show alert: No current page available. Title: {title}, Message: {message}");
                throw new InvalidOperationException("No current page available to show alert");
            }
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
#elif IOS
        // Use custom iOS implementation to remove top cancel button
        try
        {
            return await LaceupMigration.Platforms.iOS.ActionSheetService.ShowActionSheetAsync(
                title, message, cancelText, buttons);
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

    /// <summary>Shows a dialog to pick one of the product's UOMs. Returns selected UOM or null if cancelled.</summary>
    public async Task<UnitOfMeasure?> ShowPickUomForProductAsync(Product product, string title = "Select UOM")
    {
        if (product?.UnitOfMeasures == null || product.UnitOfMeasures.Count == 0)
            return null;
        var uoms = product.UnitOfMeasures;
        var buttons = uoms.Select(u => u.Name).ToArray();
        var cancelText = "Cancel";
        var result = await ShowActionSheetAsync(title, null, cancelText, buttons);
        if (string.IsNullOrEmpty(result) || result == cancelText)
            return null;
        var index = uoms.FindIndex(u => u.Name == result);
        return index >= 0 ? uoms[index] : null;
    }

    public async Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "", Keyboard keyboard = null, bool showScanIcon = false, Func<Task<string>> scanAction = null)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        // If scan icon is requested, use custom dialog that looks like native prompt
        if (showScanIcon && scanAction != null)
        {
            return await ShowPromptWithScanAsync(title, message, scanAction, acceptText, cancelText, placeholder, initialValue, maxLength, keyboard);
        }

        // Otherwise, use default native implementation
        return await page.DisplayPromptAsync(title, message, acceptText, cancelText, placeholder, maxLength, initialValue: initialValue, keyboard: keyboard);
    }

    private async Task<string> ShowPromptWithScanAsync(string title, string message, Func<Task<string>> scanAction, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", string initialValue = "", int maxLength = -1, Keyboard keyboard = null)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var tcs = new TaskCompletionSource<string>();

        // Create entry field - match native prompt style (underline, no border)
        var entry = new Entry
        {
            Text = initialValue,
            Placeholder = placeholder,
            FontSize = 16,
            Keyboard = keyboard ?? Keyboard.Default,
            BackgroundColor = Colors.Transparent,
            PlaceholderColor = Colors.Gray
        };

        if (maxLength > 0)
        {
            entry.MaxLength = maxLength;
        }

        // Create scan icon button - use QrCodeScanner icon from MauiIcons
        // Use FontImageSource directly with the correct glyph code for QrCodeScanner
        // Material Icons QrCodeScanner Unicode: \uE8B6 (qr_code_scanner)
        var scanButton = new ImageButton
        {
            Source = MaterialIconHelper.GetImageSource(MaterialOutlinedIcons.QrCodeScanner, Colors.Black),
            WidthRequest = 40,
            HeightRequest = 40,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Padding = new Thickness(8, 0, 0, 0)
        };

        // Handle scan button click: if scan returns ScanResultAddedAndClose, close without filling text
        scanButton.Clicked += async (s, e) =>
        {
            try
            {
                var scanResult = await scanAction();
                if (scanResult == ScanResultAddedAndClose)
                {
                    if (page != null && page.Navigation.ModalStack.Count > 0)
                        await page.Navigation.PopModalAsync();
                    tcs.SetResult(null);
                    return;
                }
                if (!string.IsNullOrEmpty(scanResult))
                    entry.Text = scanResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning: {ex.Message}");
            }
        };

        // Create layout with entry and scan button on same row - entry takes most space, button on right
        var inputRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 0,
            Padding = new Thickness(0, 4, 0, 0)
        };
        Grid.SetColumn(entry, 0);
        Grid.SetColumn(scanButton, 1);
        inputRow.Children.Add(entry);
        inputRow.Children.Add(scanButton);

        // Underline below entry (match "input field indicated by an underline" design)
        var underline = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(0, 4, 0, 0)
        };

        var inputWithUnderline = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { inputRow, underline }
        };

        // Content - title area uses "message" as the field label (e.g. "Search")
        var content = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(24, 0, 24, 8),
            Children =
            {
                new Label
                {
                    Text = message,
                    FontSize = 14,
                    TextColor = Colors.Black
                },
                inputWithUnderline
            }
        };

        // Separator line above buttons (Android AlertDialog style)
        var buttonSeparatorLine = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(0)
        };

        // Buttons - smaller, Android style: text-only, less height, with vertical divider
        var cancelButton = new Button
        {
            Text = cancelText,
            FontSize = 14,
            TextColor = Color.FromArgb("#424242"),
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = new Thickness(12, 8),
            MinimumHeightRequest = 0,
            MinimumWidthRequest = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        var okButton = new Button
        {
            Text = acceptText,
            FontSize = 14,
            TextColor = Color.FromArgb("#424242"),
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = new Thickness(12, 8),
            MinimumHeightRequest = 0,
            MinimumWidthRequest = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        var verticalDivider = new BoxView
        {
            WidthRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            VerticalOptions = LayoutOptions.Fill
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            Padding = new Thickness(8, 6, 8, 10),
            ColumnSpacing = 0,
            MinimumHeightRequest = 0
        };
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(verticalDivider);
        buttonRow.Children.Add(okButton);
        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(verticalDivider, 1);
        Grid.SetColumn(okButton, 2);

        var buttonSection = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { buttonSeparatorLine, buttonRow }
        };

        // Main container - title at top, then label + input row, then separator line + buttons
        var mainContainer = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(24, 24, 24, 8),
                    TextColor = Colors.Black
                },
                content,
                buttonSection
            }
        };

        // Calculate 70% of screen width
        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var dialogWidth = screenWidth * 0.70;

        // Border - match native prompt style (rounded corners, shadow effect)
        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 0,
            Margin = new Thickness(20),
            WidthRequest = dialogWidth,
            MaximumWidthRequest = dialogWidth,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
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
            tcs.SetResult(entry.Text);
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            tcs.SetResult(null);
        };

        await page.Navigation.PushModalAsync(dialog);
        
        // Focus entry
        entry.Focus();
        if (!string.IsNullOrEmpty(entry.Text))
        {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text.Length;
        }

        return await tcs.Task;
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
            FontSize = 14,
            BackgroundColor = Colors.White,
            HeightRequest = 36
        };

        // Comments input (no label, just editor with placeholder)
        var commentsEntry = new Editor
        {
            Text = initialComments,
            Placeholder = "Enter comments",
            HeightRequest = 50,
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
                    FontSize = 14,
                    BackgroundColor = Colors.White,
                    ItemsSource = familyItems,
                    ItemDisplayBinding = new Binding("Name"),
                    HeightRequest = 36
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
            Margin = new Thickness(0, 4, 0, 2),
            ColumnSpacing = 8
        };

        var qtyLabel = new Label
        {
            Text = "Qty:",
            FontSize = 14,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center
        };
        qtyEntry.VerticalOptions = LayoutOptions.Center;
        Grid.SetColumn(qtyLabel, 0);
        Grid.SetColumn(qtyEntry, 1);
        qtyRow.Children.Add(qtyLabel);
        qtyRow.Children.Add(qtyEntry);

        // Comments (no label, just editor)
        var commentsRow = new VerticalStackLayout
        {
            Spacing = 0,
            Margin = new Thickness(0, 4, 0, 2)
        };
        commentsRow.Children.Add(commentsEntry);

        // UoM row (if applicable) - label and picker in same row
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
                Margin = new Thickness(0, 4, 0, 2),
                ColumnSpacing = 8
            };

            var uomLabel = new Label
            {
                Text = "UoM:",
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };
            uomPicker.VerticalOptions = LayoutOptions.Center;
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
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0,
            HeightRequest = 40
        };

        var addButton = new Button
        {
            Text = "Add",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"), // Primary blue color
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0),
            CornerRadius = 0,
            HeightRequest = 40
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
            BackgroundColor = Colors.White,
            HeightRequest = 40
        };

        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonSeparator, 1);
        Grid.SetColumn(addButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonSeparator);
        buttonRow.Children.Add(addButton);

        // Main content - simple vertical stack with compact spacing
        var content = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.White,
            Padding = new Thickness(12, 8, 12, 8),
            Children = { qtyRow }
        };

        if (uomRow != null)
            content.Children.Add(uomRow);

        content.Children.Add(commentsRow);

        // Create a popup-style dialog (centered overlay, not full page)
        var scrollContent = new ScrollView
        {
            Content = content,
            MaximumWidthRequest = 320, // Compact width
            MaximumHeightRequest = 500 // Limit height
        };

        // Title in light blue header bar (matching RestOfTheAddDialog)
        var titleHeader = new BoxView
        {
            BackgroundColor = Color.FromArgb("#E3F2FD"), // Light blue
            HeightRequest = 40,
            VerticalOptions = LayoutOptions.Start
        };
        
        var titleLabel = new Label
        {
            Text = productName,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(12, 0, 12, 0),
            LineBreakMode = LineBreakMode.WordWrap // Show full product name
        };
        
        var titleContainer = new Grid
        {
            Children = { titleHeader, titleLabel }
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
                titleContainer,
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0") },
                scrollContent,
                // Gray line on top of buttons with no margin or padding - outside scrollContent to avoid padding
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0, 4, 0, 0) },
                buttonRow
            }
        };

        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E0E0E0"),
            Padding = 0,
            Margin = new Thickness(20, 20, 20, 20), // Compact margins
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            WidthRequest = 320,
            MaximumWidthRequest = 400,
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
            },
            Padding = new Thickness(8)
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
    
    public async Task<(List<int> selectedCategoryIds, bool selectAll, bool showPrice, bool showUPC, bool showUoM)?> ShowCatalogFilterDialogAsync(List<Category> categories, bool hideCategorySection = false, IReadOnlyList<int>? categoryIdsWhenCategorySectionHidden = null)
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

        // Create checkboxes for categories (not used when hideCategorySection)
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

        // Show In PDF section: visible when communicator supports it, or when we hid the category section (already in a category)
        bool filtersVisible = Config.CheckCommunicatorVersion("46.2.0") || hideCategorySection;

        // Handle price checkbox visibility/state (matches Xamarin logic)
        if (Config.DisableSendCatalogWithPrices || Config.HidePriceInTransaction)
        {
            showPriceCheckbox.IsEnabled = false;
            showPriceCheckbox.IsChecked = false;
            showPriceCheckbox.IsVisible = false;
        }

        // Category selection layout (hidden when already inside a category - no need to re-select)
        var categoryLayout = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(20, 0, 20, 0),
            IsVisible = !hideCategorySection
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

        // Separator between category and filters (hidden when category section is hidden)
        var categoryFiltersSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0, 10), IsVisible = !hideCategorySection };
        // Content: when category section hidden use only filters (sizes to content); otherwise use ScrollView for full list
        View row2Content;
        if (hideCategorySection)
        {
            row2Content = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(0, 10, 0, 10),
                Children = { filtersLayout }
            };
        }
        else
        {
            row2Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children =
                    {
                        categoryLayout,
                        categoryFiltersSeparator,
                        filtersLayout
                    }
                }
            };
        }

        // Main container - When category section is hidden, use Auto for content row so dialog shrinks to fit
        var mainContainer = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Auto }, // Separator
                new RowDefinition { Height = hideCategorySection ? GridLength.Auto : new GridLength(1, GridUnitType.Star) }, // Scrollable content
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

        Grid.SetRow(row2Content, 2);
        mainContainer.Children.Add(row2Content);

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
            
            bool selectAll;
            List<int> selectedCategoryIds;
            if (hideCategorySection && categoryIdsWhenCategorySectionHidden != null)
            {
                // Already in a category: use the passed-in category ID(s)
                selectedCategoryIds = categoryIdsWhenCategorySectionHidden.ToList();
                selectAll = false;
            }
            else
            {
                selectAll = allCategoriesCheckbox.IsChecked;
                selectedCategoryIds = new List<int>();
                if (!selectAll)
                {
                    foreach (var (checkbox, label, categoryId) in categoryCheckboxes)
                    {
                        if (checkbox.IsChecked)
                            selectedCategoryIds.Add(categoryId);
                    }
                }
            }

            bool showPrice = showPriceCheckbox.IsChecked;
            bool showUPC = showUPCCheckbox.IsChecked;
            bool showUoM = showUoMCheckbox.IsChecked;
            tcs.SetResult((selectedCategoryIds, selectAll, showPrice, showUPC, showUoM));
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

    /// <summary>
    /// Shows a single-choice dialog with radio buttons and optional subtitle per option. Returns the selected index, or -1 if canceled.
    /// </summary>
    public async Task<int> ShowSingleChoiceDialogAsync(string title, (string Title, string Subtitle)[] options, int selectedIndex = 0)
    {
        var page = GetCurrentPage();
        if (page == null || options == null || options.Length == 0)
            return -1;

        var tcs = new TaskCompletionSource<int>();

        var mainContainer = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            BackgroundColor = Colors.White,
            Padding = new Thickness(0)
        };

        var headerLabel = new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#017CBA"),
            Padding = new Thickness(20, 20, 20, 15),
            BackgroundColor = Colors.White
        };
        Grid.SetRow(headerLabel, 0);
        mainContainer.Children.Add(headerLabel);

        var headerSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#017CBA") };
        Grid.SetRow(headerSeparator, 1);
        mainContainer.Children.Add(headerSeparator);

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
            var (optionTitle, optionSubtitle) = options[i];

            var optionGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = 2 },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Padding = new Thickness(10, 8, 10, 8)
            };

            var radioButton = new RadioButton
            {
                IsChecked = i == selectedIndex,
                GroupName = "ActionOptions",
                VerticalOptions = LayoutOptions.Start
            };

            View optionContent;
            if (string.IsNullOrEmpty(optionSubtitle))
            {
                optionContent = new Label
                {
                    Text = optionTitle ?? string.Empty,
                    FontSize = 16,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                };
            }
            else
            {
                optionContent = new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = optionTitle ?? string.Empty,
                            FontSize = 16,
                            TextColor = Colors.Black
                        },
                        new Label
                        {
                            Text = optionSubtitle,
                            FontSize = 13,
                            TextColor = Color.FromArgb("#696969")
                        }
                    }
                };
            }

            optionContent.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(optionContent, 0);
            Grid.SetColumn(radioButton, 2);
            optionGrid.Children.Add(optionContent);
            optionGrid.Children.Add(radioButton);

            int index = i;
            radioButton.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    currentSelectedIndex = index;
                    foreach (var rb in radioButtons)
                    {
                        if (rb != radioButton)
                            rb.IsChecked = false;
                    }
                }
            };

            radioButtons.Add(radioButton);
            optionsContainer.Children.Add(optionGrid);

            if (i < options.Length - 1)
            {
                var separator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(20, 0) };
                optionsContainer.Children.Add(separator);
            }
        }

        Grid.SetRow(optionsContainer, 2);
        mainContainer.Children.Add(optionsContainer);

        var buttonTopSeparator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) };
        Grid.SetRow(buttonTopSeparator, 3);
        mainContainer.Children.Add(buttonTopSeparator);

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
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(currentSelectedIndex);
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await SafePopModalAsync();
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(-1);
        };

        await page.Navigation.PushModalAsync(dialog);
        return await tcs.Task;
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
        var initialDiscount = existingDetail != null ? existingDetail.Discount : 0;
        var initialDiscountType = existingDetail != null ? existingDetail.DiscountType : DiscountType.Amount;

        // Create dialog content - compact layout matching the "before" image
        var scrollView = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(12, 8, 12, 8)
            },
            MaximumHeightRequest = 500 // Limit height so it can scroll if needed
        };

        var content = (VerticalStackLayout)scrollView.Content;

        // Title in light blue header bar (matching "before" image)
        var titleHeader = new BoxView
        {
            BackgroundColor = Color.FromArgb("#E3F2FD"), // Light blue
            HeightRequest = 40,
            VerticalOptions = LayoutOptions.Start
        };
        
        var titleLabel = new Label
        {
            Text = product.Name,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(12, 0, 12, 0),
            LineBreakMode = LineBreakMode.WordWrap // Show full product name
        };
        
        var titleContainer = new Grid
        {
            Children = { titleHeader, titleLabel }
        };
        content.Children.Add(titleContainer);

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
                FontSize = 14,
                HeightRequest = 36
            };
        }
        else
        {
            qtyLabel = new Label { Text = "Qty:", FontSize = 14, TextColor = Colors.Black };
            qtyEntry = new Entry
            {
                Text = initialQty.ToString("F0"),
                Keyboard = Config.DontAllowDecimalsInQty ? Keyboard.Numeric : Keyboard.Numeric,
                FontSize = 14,
                HeightRequest = 36
            };
        }

        var qtyRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            Margin = new Thickness(0, 4, 0, 2),
            ColumnSpacing = 8,
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } }
        };
        qtyLabel.VerticalOptions = LayoutOptions.Center;
        qtyEntry.VerticalOptions = LayoutOptions.Center;
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
                FontSize = 14,
                HeightRequest = 36
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
                Margin = new Thickness(0, 4, 0, 2),
                ColumnSpacing = 8
            };
            weightLabel.VerticalOptions = LayoutOptions.Center;
            weightEntry.VerticalOptions = LayoutOptions.Center;
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
                    FontSize = 14,
                    BackgroundColor = Colors.LightGray,
                    HeightRequest = 36
                };
                // TODO: Add lot selection logic
                var lotLabel = new Label { Text = "Lot:", FontSize = 14, TextColor = Colors.Black };
                lotButton.HeightRequest = 36;
                var lotButtonRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    Margin = new Thickness(0, 4, 0, 2),
                    ColumnSpacing = 8
                };
                lotLabel.VerticalOptions = LayoutOptions.Center;
                lotButton.VerticalOptions = LayoutOptions.Center;
                Grid.SetColumn(lotLabel, 0);
                Grid.SetColumn(lotButton, 1);
                lotButtonRow.Children.Add(lotLabel);
                lotButtonRow.Children.Add(lotButton);
                content.Children.Add(lotButtonRow);

                if (Config.UseLotExpiration)
                {
                    expButton = new Button
                    {
                        Text = existingDetail?.LotExpiration != null && existingDetail.LotExpiration != DateTime.MinValue 
                            ? existingDetail.LotExpiration.ToShortDateString() 
                            : "",
                        FontSize = 14,
                        BackgroundColor = Colors.LightGray,
                        HeightRequest = 36
                    };
                    // TODO: Add date picker logic
                    var expLabel = new Label { Text = "Expiration:", FontSize = 14, TextColor = Colors.Black };
                    expButton.HeightRequest = 36;
                    var expButtonRow = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                        Margin = new Thickness(0, 4, 0, 2),
                        ColumnSpacing = 8
                    };
                    expLabel.VerticalOptions = LayoutOptions.Center;
                    expButton.VerticalOptions = LayoutOptions.Center;
                    Grid.SetColumn(expLabel, 0);
                    Grid.SetColumn(expButton, 1);
                    expButtonRow.Children.Add(expLabel);
                    expButtonRow.Children.Add(expButton);
                    content.Children.Add(expButtonRow);
                }
            }
            else
            {
                // Simple lot entry
                var lotLabel = new Label { Text = "Lot:", FontSize = 14, TextColor = Colors.Black };
                lotEntry = new Entry
                {
                    Text = initialLot,
                    FontSize = 14,
                    HeightRequest = 36
                };
                var lotRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    Margin = new Thickness(0, 4, 0, 2),
                    ColumnSpacing = 8
                };
                lotLabel.VerticalOptions = LayoutOptions.Center;
                lotEntry.VerticalOptions = LayoutOptions.Center;
                Grid.SetColumn(lotLabel, 0);
                Grid.SetColumn(lotEntry, 1);
                lotRow.Children.Add(lotLabel);
                lotRow.Children.Add(lotEntry);
                content.Children.Add(lotRow);
            }
        }

        // Comments (no label, just the editor with placeholder)
        Editor commentEntry = null;
        if (!Config.HideItemComment || (order.OrderType != OrderType.Order && order.OrderType != OrderType.Credit))
        {
            commentEntry = new Editor
            {
                Text = initialComments,
                Placeholder = "Enter comments",
                HeightRequest = 50,
                FontSize = 14,
                Margin = new Thickness(0, 4, 0, 2)
            };
            content.Children.Add(commentEntry);
        }

        // Price Entry (if can change price)
            Entry priceEntry = null;
            bool canChangePrice = Config.CanChangePrice(order, product, isCredit);
            // When product has UOM and we're adding (no existing detail), GetPriceForProduct returns base price - display price for initial UOM
            var initialDisplayPrice = initialPrice;
            if (existingDetail == null && initialUoM != null && !string.IsNullOrEmpty(product.UoMFamily))
                initialDisplayPrice = Math.Round(initialPrice * initialUoM.Conversion, Config.Round);
            // Note: isVendor is a field in TemplateActivity, not a property on Order
            // For now, we'll just check canChangePrice
            if (canChangePrice)
            {
                var priceLabel = new Label { Text = "Price:", FontSize = 14, TextColor = Colors.Black };
                priceEntry = new Entry
                {
                    Text = initialDisplayPrice.ToString("F2"),
                    Keyboard = Keyboard.Numeric,
                    FontSize = 14,
                    HeightRequest = 36
                };
                var priceRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    Margin = new Thickness(0, 4, 0, 2),
                    ColumnSpacing = 8
                };
                priceLabel.VerticalOptions = LayoutOptions.Center;
                priceEntry.VerticalOptions = LayoutOptions.Center;
                Grid.SetColumn(priceLabel, 0);
                Grid.SetColumn(priceEntry, 1);
                priceRow.Children.Add(priceLabel);
                priceRow.Children.Add(priceEntry);
                content.Children.Add(priceRow);
            }

        // UoM Spinner (in same row as label)
            Picker uomPicker = null;
            UnitOfMeasure selectedUoM = initialUoM;
            if (!product.SoldByWeight && !string.IsNullOrEmpty(product.UoMFamily))
            {
                var familyItems = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily).ToList();
                if (familyItems.Count > 0)
                {
                    var uomLabel = new Label { Text = "UoM:", FontSize = 14, TextColor = Colors.Black };
                    uomPicker = new Picker
                    {
                        FontSize = 14,
                        ItemsSource = familyItems,
                        ItemDisplayBinding = new Binding("Name"),
                        HeightRequest = 36
                    };

                    if (initialUoM != null)
                    {
                        var index = familyItems.FindIndex(x => x.Id == initialUoM.Id);
                        if (index >= 0)
                            uomPicker.SelectedIndex = index;
                    }

                    uomPicker.SelectedIndexChanged += (s, e) =>
                    {
                        if (uomPicker.SelectedIndex < 0 || uomPicker.SelectedIndex >= familyItems.Count)
                            return;
                        var previousUoM = selectedUoM;
                        selectedUoM = familyItems[uomPicker.SelectedIndex];
                        // Update price to the conversion for the selected UOM (price per new UOM = price per old UOM * (newConversion / oldConversion))
                        if (priceEntry != null && previousUoM != null && selectedUoM != null && double.TryParse(priceEntry.Text, out var currentPrice))
                        {
                            var newPrice = Math.Round(currentPrice * (selectedUoM.Conversion / previousUoM.Conversion), Config.Round);
                            priceEntry.Text = newPrice.ToString("F2");
                        }
                    };

                    var uomRow = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                        Margin = new Thickness(0, 4, 0, 2),
                        ColumnSpacing = 8
                    };
                    uomLabel.VerticalOptions = LayoutOptions.Center;
                    uomPicker.VerticalOptions = LayoutOptions.Center;
                    Grid.SetColumn(uomLabel, 0);
                    Grid.SetColumn(uomPicker, 1);
                    uomRow.Children.Add(uomLabel);
                    uomRow.Children.Add(uomPicker);
                    content.Children.Add(uomRow);
                }
            }

        // Price Level Picker
        Picker priceLevelPicker = null;
        int selectedPriceLevelId = initialPriceLevelSelected;
        if (canChangePrice)
        {
            // Get ProductPrice entries for this product
            var productPrices = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId).ToList();
            if (productPrices.Any())
            {
                var priceLevelOptions = new List<string> { "Select a Price Level" };
                var priceLevelIds = new List<int> { 0 };
                var productPriceList = new List<ProductPrice>();
                var conversion = selectedUoM != null ? selectedUoM.Conversion : 1.0;

                // Build price level list from ProductPrice entries
                foreach (var pp in productPrices)
                {
                    var priceLevel = PriceLevel.List.FirstOrDefault(x => x.Id == pp.PriceLevelId);
                    if (priceLevel != null)
                    {
                        var convertedPrice = Math.Round(pp.Price * conversion, Config.Round);
                        priceLevelOptions.Add($"{priceLevel.Name}: {convertedPrice.ToCustomString()}");
                        priceLevelIds.Add(pp.PriceLevelId);
                        productPriceList.Add(pp);
                    }
                }

                if (priceLevelOptions.Count > 1) // More than just "Select a Price Level"
                {
                    var priceLevelLabel = new Label { Text = "Price Level:", FontSize = 14, TextColor = Colors.Black };
                    priceLevelPicker = new Picker
                    {
                        FontSize = 14,
                        ItemsSource = priceLevelOptions,
                        HeightRequest = 36
                    };

                    // Set initial selection
                    if (initialPriceLevelSelected > 0)
                    {
                        var index = priceLevelIds.IndexOf(initialPriceLevelSelected);
                        if (index > 0) // index 0 is "Select a Price Level"
                            priceLevelPicker.SelectedIndex = index;
                        else
                            priceLevelPicker.SelectedIndex = 0;
                    }
                    else
                    {
                        priceLevelPicker.SelectedIndex = 0; // "Select a Price Level"
                    }

                    priceLevelPicker.SelectedIndexChanged += (s, e) =>
                    {
                        if (priceLevelPicker.SelectedIndex > 0 && priceLevelPicker.SelectedIndex <= priceLevelIds.Count)
                        {
                            selectedPriceLevelId = priceLevelIds[priceLevelPicker.SelectedIndex];
                            // Update price when price level changes
                            if (priceEntry != null && productPriceList.Count > priceLevelPicker.SelectedIndex - 1)
                            {
                                var selectedPP = productPriceList[priceLevelPicker.SelectedIndex - 1];
                                var currentConversion = selectedUoM != null ? selectedUoM.Conversion : 1.0;
                                var newPrice = Math.Round(selectedPP.Price * currentConversion, Config.Round);
                                priceEntry.Text = newPrice.ToString("F2");
                            }
                        }
                        else
                        {
                            selectedPriceLevelId = 0;
                        }
                    };

                    var priceLevelRow = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                        Margin = new Thickness(0, 4, 0, 2),
                        ColumnSpacing = 8
                    };
                    priceLevelLabel.VerticalOptions = LayoutOptions.Center;
                    priceLevelPicker.VerticalOptions = LayoutOptions.Center;
                    Grid.SetColumn(priceLevelLabel, 0);
                    Grid.SetColumn(priceLevelPicker, 1);
                    priceLevelRow.Children.Add(priceLevelLabel);
                    priceLevelRow.Children.Add(priceLevelPicker);
                    content.Children.Add(priceLevelRow);
                }
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
                var freeItemRow = new HorizontalStackLayout { Spacing = 6, Margin = new Thickness(0, 2, 0, 2) };
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
            var useLspRow = new HorizontalStackLayout { Spacing = 6, Margin = new Thickness(0, 2, 0, 2) };
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

        // Discount per line (link) - matches Xamarin PreviouslyOrderedTemplateActivity CatalogAddDiscountLink / DiscountLink_Click
        double currentDiscount = initialDiscount;
        DiscountType currentDiscountType = initialDiscountType;
        ContentPage restOfTheDialog = null; // set when dialog is created; discount tap uses restOfTheDialog.Navigation
        bool showDiscountLink = Config.AllowDiscountPerLine
            && order.Client?.UseDiscountPerLine == true
            && (order.OrderType == OrderType.Order || order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            && !Config.HidePriceInTransaction
            && !initialFreeItem;

        Label discountLinkLabel = null;
        if (showDiscountLink)
        {
            string FormatDiscountText(double discount, DiscountType dType)
            {
                if (discount == 0) return "Discount = $0.00";
                if (dType == DiscountType.Percent)
                    return "Discount = " + (discount * 100).ToString("F0") + "%";
                return "Discount = $" + discount.ToString("F2");
            }

            discountLinkLabel = new Label
            {
                Text = FormatDiscountText(currentDiscount, currentDiscountType),
                FontSize = 14,
                TextColor = Color.FromArgb("#017CBA"),
                TextDecorations = TextDecorations.Underline,
                Margin = new Thickness(0, 6, 0, 2)
            };
            var discountTap = new TapGestureRecognizer();
            discountTap.Tapped += async (s, e) =>
            {
                // Get current qty and price from form (for validation)
                double qtyVal = 1;
                float.TryParse(qtyEntry?.Text, out var qtyF);
                qtyVal = qtyF;
                if (product.SoldByWeight && order.AsPresale)
                    qtyVal *= product.Weight;
                double priceVal = initialPrice;
                if (priceEntry != null && double.TryParse(priceEntry.Text, out var p))
                    priceVal = p;

                var percRadio = new RadioButton { Content = "Percentage", IsChecked = currentDiscountType == DiscountType.Percent };
                var percEntry = new Entry
                {
                    Text = currentDiscountType == DiscountType.Percent ? (currentDiscount * 100).ToString("F0") : "",
                    Keyboard = Keyboard.Numeric,
                    Placeholder = "0"
                };
                var amountRadio = new RadioButton { Content = "Amount", IsChecked = currentDiscountType == DiscountType.Amount };
                var amountEntry = new Entry
                {
                    Text = currentDiscountType == DiscountType.Amount ? currentDiscount.ToString("F2") : "",
                    Keyboard = Keyboard.Numeric,
                    Placeholder = "0.00"
                };
                var calcLabel = new Label { Text = "", FontSize = 12, TextColor = Colors.Gray };
                void UpdateCalc()
                {
                    if (percRadio.IsChecked && double.TryParse(percEntry.Text, out var perc))
                        calcLabel.Text = "= $" + (priceVal * qtyVal * perc / 100).ToString("F2");
                }
                percEntry.TextChanged += (_, __) => UpdateCalc();
                percRadio.CheckedChanged += (_, __) => { if (percRadio.IsChecked) { amountEntry.Text = ""; UpdateCalc(); } };
                amountRadio.CheckedChanged += (_, __) => { if (amountRadio.IsChecked) { percEntry.Text = ""; calcLabel.Text = ""; } };

                var addBtn = new Button { Text = "Add", HorizontalOptions = LayoutOptions.End };
                var cancelBtn = new Button { Text = "Cancel", HorizontalOptions = LayoutOptions.Start };
                addBtn.Clicked += async (s, e) =>
                {
                    var lineTotal = qtyVal * priceVal;
                    if (percRadio.IsChecked)
                    {
                        if (!double.TryParse(percEntry.Text, out var percent)) percent = 0;
                        if (percent > 100)
                        {
                            await GetCurrentPage()?.DisplayAlert("Alert", "Discount cannot exceed 100% of the line total.", "OK");
                            return;
                        }
                        if (!order.CanAddLineDiscount(percent, DiscountType.Percent, qtyVal, priceVal, existingDetail))
                        {
                            await GetCurrentPage()?.DisplayAlert("Alert", "Cannot give more than " + Config.MaxDiscountPerOrder + "% discount to the order.", "OK");
                            return;
                        }
                        currentDiscount = percent / 100;
                        currentDiscountType = DiscountType.Percent;
                    }
                    else
                    {
                        if (!double.TryParse(amountEntry.Text, out var amt)) amt = 0;
                        if (amt > lineTotal)
                        {
                            await GetCurrentPage()?.DisplayAlert("Alert", "Discount cannot exceed the line total ($" + lineTotal.ToString("F2") + ").", "OK");
                            return;
                        }
                        if (!order.CanAddLineDiscount(amt, DiscountType.Amount, qtyVal, priceVal, existingDetail))
                        {
                            await GetCurrentPage()?.DisplayAlert("Alert", "Cannot give more than " + Config.MaxDiscountPerOrder + "% discount to the order.", "OK");
                            return;
                        }
                        currentDiscount = amt;
                        currentDiscountType = DiscountType.Amount;
                    }
                    discountLinkLabel.Text = FormatDiscountText(currentDiscount, currentDiscountType);
                    await restOfTheDialog?.Navigation.PopModalAsync();
                };
                cancelBtn.Clicked += async (s, e) => await restOfTheDialog?.Navigation.PopModalAsync();

                // Popup content: title bar + form (match RestOfThe dialog style)
                var discountTitleBar = new BoxView { BackgroundColor = Color.FromArgb("#E3F2FD"), HeightRequest = 40 };
                var discountTitleLabel = new Label
                {
                    Text = product.Name,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(12, 0, 12, 0)
                };
                var discountTitleGrid = new Grid { Children = { discountTitleBar, discountTitleLabel } };

                var discountForm = new VerticalStackLayout
                {
                    Spacing = 8,
                    Padding = new Thickness(16, 12, 16, 16),
                    Children =
                    {
                        discountTitleGrid,
                        percRadio,
                        percEntry,
                        calcLabel,
                        amountRadio,
                        amountEntry,
                        new Grid
                        {
                            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
                            ColumnSpacing = 8,
                            Children = { cancelBtn, addBtn },
                            Margin = new Thickness(0, 8, 0, 0)
                        }
                    }
                };
                Grid.SetColumn(cancelBtn, 0);
                Grid.SetColumn(addBtn, 1);

                var discountPopupBorder = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 1,
                    Stroke = Color.FromArgb("#E0E0E0"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    WidthRequest = 280,
                    MaximumWidthRequest = 340,
                    Padding = new Thickness(0),
                    Content = discountForm
                };

                var discountOverlay = new Grid
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
                    Padding = new Thickness(24)
                };
                Grid.SetRow(discountPopupBorder, 1);
                Grid.SetColumn(discountPopupBorder, 1);
                discountOverlay.Children.Add(discountPopupBorder);

                var discountPage = new ContentPage
                {
                    BackgroundColor = Colors.Transparent,
                    Content = discountOverlay
                };

                UpdateCalc();
                await restOfTheDialog?.Navigation.PushModalAsync(discountPage);
            };
            discountLinkLabel.GestureRecognizers.Add(discountTap);
            content.Children.Add(discountLinkLabel);

            if (freeItemCheckbox != null)
            {
                freeItemCheckbox.CheckedChanged += (s, e) =>
                {
                    if (discountLinkLabel != null)
                        discountLinkLabel.IsVisible = !(freeItemCheckbox.IsChecked);
                };
            }
        }

        // Buttons with separator line above
        var topSeparator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 4, 0, 0)
        };
        content.Children.Add(topSeparator);

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            HeightRequest = 40,
            Margin = new Thickness(0)
        };

        var addButton = new Button
        {
            Text = "Add",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#017CBA"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            HeightRequest = 40,
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
            HeightRequest = 40
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

            // Create dialog - compact size
            var dialogBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                WidthRequest = 320,
                MaximumWidthRequest = 400,
                Padding = new Thickness(0),
                Margin = new Thickness(20, 20, 20, 20), // Reduced margins
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
                Padding = new Thickness(8)
            };

            Grid.SetRow(dialogBorder, 1);
            Grid.SetColumn(dialogBorder, 1);
            overlayGrid.Children.Add(dialogBorder);

            restOfTheDialog = new ContentPage
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

                // Get price level
                result.PriceLevelSelected = selectedPriceLevelId;
                
                // TODO: Add reason logic
                result.ReasonId = initialReasonId;

                result.Discount = currentDiscount;
                result.DiscountType = currentDiscountType;

                result.Cancelled = false;

                await page.Navigation.PopModalAsync();
                tcs.SetResult(result);
            };

            cancelButton.Clicked += async (s, e) =>
            {
                await page.Navigation.PopModalAsync();
                tcs.SetResult(new RestOfTheAddDialogResult { Cancelled = true });
            };

        await page.Navigation.PushModalAsync(restOfTheDialog);
        
        // Auto-focus the Qty/Weight field when dialog appears and select all text
        // If weight entry is shown, focus that instead (since qty is disabled in that case)
        Entry fieldToFocus = weightEntry != null ? weightEntry : qtyEntry;
        if (fieldToFocus != null)
        {
            // Use a small delay to ensure the dialog is fully rendered before focusing
            await Task.Delay(100);
            fieldToFocus.Focus();
            // Select all text so typing replaces the value (matching Xamarin: SetSelectAllOnFocus)
            if (!string.IsNullOrEmpty(fieldToFocus.Text))
            {
                fieldToFocus.CursorPosition = 0;
                fieldToFocus.SelectionLength = fieldToFocus.Text.Length;
            }
        }
        
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