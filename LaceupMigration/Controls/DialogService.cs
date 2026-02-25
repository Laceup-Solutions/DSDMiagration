using Microsoft.Maui.Controls.Shapes;
using LaceupMigration;
using LaceupMigration.Helpers;
using LaceupMigration.ViewModels;
using LaceupMigration.Views;
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

    public async Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "", Keyboard keyboard = null, bool showScanIcon = false, Func<Task<string>> scanAction = null, bool selectAllText = false)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        // If scan icon is requested, use custom dialog that looks like native prompt
        if (showScanIcon && scanAction != null)
        {
            return await ShowPromptWithScanAsync(title, message, scanAction, acceptText, cancelText, placeholder, initialValue, maxLength, keyboard);
        }

        // If select-all is requested, use custom dialog so we can select all text when focused
        if (selectAllText)
        {
            return await ShowPromptWithSelectAllAsync(title, message, acceptText, cancelText, placeholder, initialValue, maxLength, keyboard);
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

        // Handle scan button click: always close the popup first, then run scan (camera). Caller handles navigate/alert.
        scanButton.Clicked += async (s, e) =>
        {
            try
            {
                // Close popup immediately so it is never visible behind the camera or after scan result
                if (page != null && page.Navigation.ModalStack.Count > 0)
                    await page.Navigation.PopModalAsync();
                tcs.SetResult(null);

                // Run scan after popup is closed (opens camera; caller navigates or shows alert)
                var scanResult = await scanAction();
                // Result is ignored here; caller already navigated or showed "not found"
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

    private async Task<string> ShowPromptWithSelectAllAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", string initialValue = "", int maxLength = -1, Keyboard keyboard = null)
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var tcs = new TaskCompletionSource<string>();

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
            entry.MaxLength = maxLength;

        var underline = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(0, 4, 0, 0)
        };

        var inputWithUnderline = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { entry, underline }
        };

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

        var buttonSeparatorLine = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(0)
        };

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

        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var dialogWidth = screenWidth * 0.70;

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

        entry.Focused += (s, e) =>
        {
            if (s is Entry en && !string.IsNullOrEmpty(en.Text))
            {
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    en.CursorPosition = 0;
                    en.SelectionLength = en.Text.Length;
                });
            }
        };
        entry.Focus();

        return await tcs.Task;
    }

    public async Task<(string serverAddress, int port, int salesmanId)?> ShowLoginNewCompanyDialogAsync()
    {
        var page = GetCurrentPage();
        if (page == null)
            return null;

        var tcs = new TaskCompletionSource<(string serverAddress, int port, int salesmanId)?>();

        var titleLabel = new Label
        {
            Text = "Login into new company",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0379cb"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        var titleSection = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(24, 20),
            Children = { titleLabel }
        };

        var titleLine = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#0379cb"),
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var serverEntry = new Entry
        {
            Placeholder = "Server Address",
            FontSize = 16,
            BackgroundColor = Colors.White,
            HeightRequest = 44,
            Margin = new Thickness(0, 0, 0, 8)
        };
        var portEntry = new Entry
        {
            Placeholder = "Port",
            Keyboard = Keyboard.Numeric,
            FontSize = 16,
            BackgroundColor = Colors.White,
            HeightRequest = 44,
            Margin = new Thickness(0, 0, 0, 8)
        };
        var salesmanEntry = new Entry
        {
            Placeholder = "Salesman Id",
            Keyboard = Keyboard.Numeric,
            FontSize = 16,
            BackgroundColor = Colors.White,
            HeightRequest = 44,
            Margin = new Thickness(0, 0, 0, 0)
        };
        var entriesSection = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(24, 0, 24, 16),
            Children = { serverEntry, portEntry, salesmanEntry }
        };

        var buttonSeparatorLine = new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            HorizontalOptions = LayoutOptions.Fill
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            FontSize = 14,
            TextColor = Color.FromArgb("#424242"),
            BackgroundColor = Colors.White,
            BorderWidth = 0
        };
        var signInButton = new Button
        {
            Text = "Sign In",
            FontSize = 14,
            TextColor = Color.FromArgb("#424242"),
            BackgroundColor = Colors.White,
            BorderWidth = 0
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 0
        };
        var buttonDivider = new BoxView { WidthRequest = 1, Color = Color.FromArgb("#E0E0E0"), VerticalOptions = LayoutOptions.Fill };
        Grid.SetColumn(cancelButton, 0);
        Grid.SetColumn(buttonDivider, 1);
        Grid.SetColumn(signInButton, 2);
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(buttonDivider);
        buttonRow.Children.Add(signInButton);

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(0),
            Children =
            {
                titleSection,
                titleLine,
                entriesSection,
                buttonSeparatorLine,
                buttonRow
            }
        };

        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var dialogWidth = Math.Min(screenWidth * 0.85, 400);

        var dialogBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 0,
            Margin = new Thickness(20),
            WidthRequest = dialogWidth,
            MaximumWidthRequest = dialogWidth,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = content
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

        async Task CloseAsync()
        {
            try
            {
                if (page != null && page.Navigation.ModalStack.Count > 0)
                    await page.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing Login New Company dialog: {ex.Message}");
            }
        }

        signInButton.Clicked += async (s, e) =>
        {
            var server = serverEntry.Text?.Trim() ?? "";
            var portStr = portEntry.Text?.Trim() ?? "";
            var salesmanStr = salesmanEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(server))
            {
                await ShowAlertAsync("Please enter Server Address.", "Login into new company");
                return;
            }
            if (!int.TryParse(portStr, out var port) || port <= 0)
            {
                await ShowAlertAsync("Please enter a valid Port.", "Login into new company");
                return;
            }
            if (!int.TryParse(salesmanStr, out var salesmanId) || salesmanId <= 0)
            {
                await ShowAlertAsync("Please enter a valid Salesman Id.", "Login into new company");
                return;
            }
            await CloseAsync();
            tcs.SetResult((server, port, salesmanId));
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await CloseAsync();
            tcs.SetResult(null);
        };

        await page.Navigation.PushModalAsync(dialog);
        serverEntry.Focus();

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
        // Use platform-specific native date picker (Android: DatePickerDialog, iOS: UIDatePicker)
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

        // Fallback: text prompt when native picker not available
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

        var commVersion = Config.CheckCommunicatorVersion("46.2.0");
        // Show In PDF section: visible when communicator supports it, or when we hid the category section (already in a category)
        bool filtersVisible = commVersion || hideCategorySection;

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

    /// <summary>Shows the line discount popup (percentage or amount). Called from RestOfTheAddDialog when user taps Discount link.</summary>
    private async Task<(double discount, DiscountType type)> ShowLineDiscountPopupAsync(
        Product product,
        Order order,
        OrderDetail? existingDetail,
        double currentDiscount,
        DiscountType currentDiscountType,
        double qtyVal,
        double priceVal)
    {
        var parentPage = GetCurrentPage();
        if (parentPage == null) return (currentDiscount, currentDiscountType);

        var tcs = new TaskCompletionSource<(double, DiscountType)>();
        string FormatDiscountText(double d, DiscountType t)
        {
            if (d == 0) return "Discount = $0.00";
            if (t == DiscountType.Percent) return "Discount = " + (d * 100).ToString("F0") + "%";
            return "Discount = $" + d.ToString("F2");
        }

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
        SelectAllOnFocusForEntry(percEntry);
        SelectAllOnFocusForEntry(amountEntry);
        var calcLabel = new Label { Text = "", FontSize = 12, TextColor = Colors.Gray };
        void UpdateCalc()
        {
            if (percRadio.IsChecked && double.TryParse(percEntry.Text, out var perc))
                calcLabel.Text = "= $" + (priceVal * qtyVal * perc / 100).ToString("F2");
        }
        percEntry.TextChanged += (_, _) => UpdateCalc();
        percRadio.CheckedChanged += (_, _) => { if (percRadio.IsChecked) { amountEntry.Text = ""; UpdateCalc(); percEntry.Focus(); } };
        amountRadio.CheckedChanged += (_, _) => { if (amountRadio.IsChecked) { percEntry.Text = ""; calcLabel.Text = ""; amountEntry.Focus(); } };

        var addBtn = new Button { Text = "Add", HorizontalOptions = LayoutOptions.End };
        var cancelBtn = new Button { Text = "Cancel", HorizontalOptions = LayoutOptions.Start };
        addBtn.Clicked += async (_, _) =>
        {
            var lineTotal = qtyVal * priceVal;
            double newDiscount = currentDiscount;
            var newType = currentDiscountType;
            if (percRadio.IsChecked)
            {
                if (!double.TryParse(percEntry.Text, out var percent)) percent = 0;
                if (percent > 100) { await parentPage.DisplayAlert("Alert", "Discount cannot exceed 100% of the line total.", "OK"); return; }
                if (!order.CanAddLineDiscount(percent, DiscountType.Percent, qtyVal, priceVal, existingDetail))
                { await parentPage.DisplayAlert("Alert", "Cannot give more than " + Config.MaxDiscountPerOrder + "% discount to the order.", "OK"); return; }
                newDiscount = percent / 100;
                newType = DiscountType.Percent;
            }
            else
            {
                if (!double.TryParse(amountEntry.Text, out var amt)) amt = 0;
                if (amt > lineTotal) { await parentPage.DisplayAlert("Alert", "Discount cannot exceed the line total ($" + lineTotal.ToString("F2") + ").", "OK"); return; }
                if (!order.CanAddLineDiscount(amt, DiscountType.Amount, qtyVal, priceVal, existingDetail))
                { await parentPage.DisplayAlert("Alert", "Cannot give more than " + Config.MaxDiscountPerOrder + "% discount to the order.", "OK"); return; }
                newDiscount = amt;
                newType = DiscountType.Amount;
            }
            await parentPage.Navigation.PopModalAsync();
            tcs.TrySetResult((newDiscount, newType));
        };
        cancelBtn.Clicked += async (_, _) =>
        {
            await parentPage.Navigation.PopModalAsync();
            tcs.TrySetResult((currentDiscount, currentDiscountType));
        };

        var discountTitleBar = new BoxView { BackgroundColor = Color.FromArgb("#E3F2FD"), HeightRequest = 40 };
        var discountTitleLabel = new Label { Text = product.Name, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center, Padding = new Thickness(12, 0, 12, 0) };
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
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            Padding = new Thickness(24)
        };
        Grid.SetRow(discountPopupBorder, 1);
        Grid.SetColumn(discountPopupBorder, 1);
        discountOverlay.Children.Add(discountPopupBorder);
        var discountPage = new ContentPage { BackgroundColor = Colors.Transparent, Content = discountOverlay };
        discountPage.Appearing += (_, _) => { if (currentDiscountType == DiscountType.Percent) percEntry.Focus(); else amountEntry.Focus(); };
        UpdateCalc();
        await parentPage.Navigation.PushModalAsync(discountPage);
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
        
        InvoiceDetail? lastInvoiceDetail = null;
        if (order.Client != null)
        {
            var clientHistory = InvoiceDetail.ClientProduct(order.Client.ClientId, product.ProductId);
            if (clientHistory != null && clientHistory.Count > 0)
                lastInvoiceDetail = clientHistory.OrderByDescending(x => x.Date).FirstOrDefault();
        }
        if (Config.UseLSP && lastInvoiceDetail != null)
            initialUseLSP = Math.Abs(initialPrice - lastInvoiceDetail.Price) < 0.01;
        var initialReasonId = existingDetail != null ? existingDetail.ReasonId : 0;
        var initialPriceLevelSelected = existingDetail != null ? (existingDetail.ExtraFields != null ? int.TryParse(UDFHelper.GetSingleUDF("priceLevelSelected", existingDetail.ExtraFields), out var pl) ? pl : 0 : 0) : 0;
        var initialDiscount = existingDetail != null
            ? (existingDetail.DiscountType == DiscountType.Amount && existingDetail.Qty > 0 ? existingDetail.Discount * existingDetail.Qty : existingDetail.Discount)
            : 0;
        var initialDiscountType = existingDetail != null ? existingDetail.DiscountType : DiscountType.Amount;

        var vm = new RestOfTheAddDialogViewModel();
        vm.Initialize(
            product, order, existingDetail, isCredit, isDamaged, isDelivery,
            lastInvoiceDetail,
            initialQty, initialWeight, initialLot, initialComments, initialPrice,
            initialUoM, initialFreeItem, initialUseLSP, initialPriceLevelSelected,
            initialDiscount, initialDiscountType, initialReasonId,
            onCompleteAsync: async (result) =>
            {
                await page.Navigation.PopModalAsync();
                tcs.TrySetResult(result);
            },
            showDiscountPopupAsync: (d, t, qtyVal, priceVal) => ShowLineDiscountPopupAsync(product, order, existingDetail, d, t, qtyVal, priceVal));

        var dialogPage = new RestOfTheAddDialogPage(vm);
        await page.Navigation.PushModalAsync(dialogPage);
        return await tcs.Task;
    }
    
    private static void SelectAllOnFocusForEntry(Entry entry)
    {
        if (entry == null) return;
        entry.Focused += (s, e) =>
        {
            if (s is Entry en)
            {
                en.Dispatcher?.Dispatch(() =>
                {
                    en.CursorPosition = 0;
                    en.SelectionLength = en.Text?.Length ?? 0;
                });
            }
        };
    }

    private Page GetCurrentPage()
    {
        // Try Shell first (most common in MAUI)
        if (Shell.Current?.CurrentPage != null)
            return Shell.Current.CurrentPage;
        
        // Fallback to Application Windows
        return Application.Current?.Windows?.FirstOrDefault()?.Page;
    }

    /// <summary>
    /// Shows a single popup to input all add-qty fields (Items/Weight, Price, Weight, Lot) matching the old app.
    /// Returns null if cancelled.
    /// </summary>
    public async Task<LaceupMigration.ViewModels.TemplateAddQtyResult?> ShowTemplateAddQtyPopupAsync(
        string productName,
        bool isSoldByWeight,
        double defaultPrice,
        bool showPrice,
        string lastSoldPriceDisplay,
        bool needLot,
        System.Collections.Generic.List<(string Lot, DateTime Exp)> lots,
        bool useLotExpiration)
    {
        var page = GetCurrentPage();
        if (page == null) return null;

        var tcs = new TaskCompletionSource<LaceupMigration.ViewModels.TemplateAddQtyResult?>();
        var itemsEntry = new Entry
        {
            Text = "1",
            Keyboard = Keyboard.Numeric,
            Placeholder = "1"
        };
        var qtyEntry = new Entry
        {
            Text = "",
            Keyboard = Keyboard.Numeric,
            Placeholder = isSoldByWeight ? "Weight" : "1",
            IsVisible = isSoldByWeight
        };
        var priceEntry = new Entry
        {
            Text = defaultPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Keyboard = Keyboard.Numeric,
            Placeholder = "0",
            IsVisible = showPrice
        };
        Entry? lotEntry = null;
        Button? lotPickerButton = null;
        if (needLot && lots != null && lots.Count > 0)
        {
            lotPickerButton = new Button { Text = "Select Lot" };
            lotEntry = new Entry { Placeholder = "Lot", IsVisible = false };
        }
        else if (needLot)
            lotEntry = new Entry { Placeholder = "Lot" };

        string selectedLot = "";
        DateTime selectedExp = DateTime.MinValue;
        if (lotPickerButton != null && lots != null && lots.Count > 0)
        {
            lotPickerButton.Clicked += async (s, e) =>
            {
                var names = lots.Select(x => "Lot: " + x.Lot + (x.Exp != DateTime.MinValue ? "  Exp: " + x.Exp.ToShortDateString() : "")).ToArray();
                var choice = await ShowActionSheetAsync("Select Lot", null, "Cancel", names);
                if (!string.IsNullOrEmpty(choice) && choice != "Cancel")
                {
                    var idx = names.ToList().IndexOf(choice);
                    if (idx >= 0 && idx < lots.Count)
                    {
                        selectedLot = lots[idx].Lot;
                        selectedExp = lots[idx].Exp;
                        lotPickerButton.Text = selectedLot;
                    }
                }
            };
        }

        var addBtn = new Button { Text = "Add", BackgroundColor = Color.FromArgb("#0379cb"), TextColor = Colors.White };
        var cancelBtn = new Button { Text = "Cancel", BackgroundColor = Colors.White, TextColor = Colors.Black };

        cancelBtn.Clicked += (s, e) =>
        {
            tcs.TrySetResult(null);
            page.Navigation.PopModalAsync();
        };

        addBtn.Clicked += async (s, e) =>
        {
            if (!int.TryParse(itemsEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lineCount) || lineCount < 1)
            {
                await page.DisplayAlert("Alert", "Please enter how many lines to add (Items must be at least 1).", "OK");
                return;
            }
            double weight = 0;
            float qty = 1f;
            if (isSoldByWeight)
            {
                if (!double.TryParse(qtyEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out weight) || weight <= 0)
                {
                    await page.DisplayAlert("Alert", "You must enter the weight to add the items.", "OK");
                    return;
                }
            }
            double price = defaultPrice;
            if (showPrice && priceEntry.IsVisible && !string.IsNullOrEmpty(priceEntry.Text))
            {
                if (!double.TryParse(priceEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
                {
                    await page.DisplayAlert("Alert", "Not a valid number.", "OK");
                    return;
                }
            }
            string lot = selectedLot;
            if (lotEntry != null && lotEntry.IsVisible && string.IsNullOrEmpty(lot))
                lot = lotEntry.Text?.Trim() ?? "";

            tcs.TrySetResult(new LaceupMigration.ViewModels.TemplateAddQtyResult
            {
                LineCount = lineCount,
                Qty = qty,
                Price = Math.Round(price, Config.Round),
                Weight = weight,
                Lot = lot,
                LotExpiration = selectedExp
            });
            await page.Navigation.PopModalAsync();
        };

        var promptLabel = new Label
        {
            Text = "How many items do you want to add?",
            FontSize = 14,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 0, 0, 8)
        };
        var itemsLabel = new Label { Text = "Items:", FontSize = 14 };
        var priceLabel = new Label { Text = "Price:", FontSize = 14, IsVisible = showPrice };
        var weightLabel = new Label { Text = "Weight:", FontSize = 14, IsVisible = isSoldByWeight };
        var lspLabel = new Label
        {
            Text = lastSoldPriceDisplay,
            FontSize = 13,
            TextColor = Color.FromArgb("#3FBC4D"),
            IsVisible = !string.IsNullOrEmpty(lastSoldPriceDisplay),
            Margin = new Thickness(0, 4, 0, 0)
        };

        var form = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(16, 12, 16, 16),
            Children =
            {
                new Label { Text = productName, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0379cb") },
                promptLabel,
                itemsLabel,
                itemsEntry,
                weightLabel,
                qtyEntry,
                priceLabel,
                priceEntry,
                lspLabel
            }
        };
        if (lotPickerButton != null)
        {
            form.Children.Add(new Label { Text = "Lot:", FontSize = 14 });
            form.Children.Add(lotPickerButton);
        }
        else if (lotEntry != null)
        {
            form.Children.Add(new Label { Text = "Lot:", FontSize = 14 });
            form.Children.Add(lotEntry);
        }
        var buttonRow = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition() }, ColumnSpacing = 8, Margin = new Thickness(0, 16, 0, 0) };
        buttonRow.Children.Add(cancelBtn);
        buttonRow.Children.Add(addBtn);
        Grid.SetColumn(addBtn, 1);
        form.Children.Add(buttonRow);

        var border = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E0E0E0"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(0),
            Content = form
        };
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition { Width = new GridLength(320) }, new ColumnDefinition() },
            Padding = new Thickness(24)
        };
        Grid.SetRow(border, 1);
        Grid.SetColumn(border, 1);
        overlay.Children.Add(border);

        var popupPage = new ContentPage { BackgroundColor = Colors.Transparent, Content = overlay };
        popupPage.Appearing += (_, __) => itemsEntry.Focus();

        await page.Navigation.PushModalAsync(popupPage);
        return await tcs.Task;
    }

    /// <summary>
    /// Shows a single popup to edit a line: Price, Weight, Qty, Comments, FreeItem checkbox (and optionally Lot, UoM).
    /// Returns null if cancelled.
    /// </summary>
    public async Task<LaceupMigration.ViewModels.TemplateEditLineResult?> ShowTemplateEditLinePopupAsync(
        string productName,
        bool isSoldByWeight,
        bool showPrice,
        bool allowFreeItems,
        float initialQty,
        double initialWeight,
        double initialPrice,
        string initialComments,
        bool initialFreeItem,
        bool needLot,
        System.Collections.Generic.List<(string Lot, DateTime Exp)>? lots,
        string initialLot,
        DateTime initialLotExp,
        Product product,
        UnitOfMeasure? initialUom = null)
    {
        var page = GetCurrentPage();
        if (page == null) return null;

        var tcs = new TaskCompletionSource<LaceupMigration.ViewModels.TemplateEditLineResult?>();

        var qtyWeightEntry = new Entry
        {
            Text = isSoldByWeight ? initialWeight.ToString(System.Globalization.CultureInfo.InvariantCulture) : initialQty.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Keyboard = Keyboard.Numeric,
            Placeholder = isSoldByWeight ? "Weight" : "Qty"
        };
        var priceEntry = new Entry
        {
            Text = initialPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Keyboard = Keyboard.Numeric,
            Placeholder = "0",
            IsVisible = showPrice
        };
        var commentsEntry = new Entry
        {
            Text = initialComments ?? "",
            Placeholder = "Comments"
        };
        var freeItemCheckbox = new CheckBox
        {
            IsChecked = initialFreeItem,
            IsVisible = allowFreeItems
        };
        var freeItemLabel = new Label { Text = "Free item", FontSize = 14, IsVisible = allowFreeItems };
        if (allowFreeItems)
        {
            freeItemCheckbox.CheckedChanged += (s, e) =>
            {
                priceEntry.IsEnabled = !freeItemCheckbox.IsChecked;
                if (freeItemCheckbox.IsChecked) priceEntry.Text = "0";
            };
            priceEntry.IsEnabled = !initialFreeItem;
        }

        Entry? lotEntry = null;
        Button? lotPickerButton = null;
        if (needLot && lots != null && lots.Count > 0)
        {
            lotPickerButton = new Button { Text = string.IsNullOrEmpty(initialLot) ? "Select Lot" : initialLot };
            lotEntry = new Entry { Placeholder = "Lot", IsVisible = false };
        }
        else if (needLot)
            lotEntry = new Entry { Text = initialLot ?? "", Placeholder = "Lot" };

        string selectedLot = initialLot ?? "";
        DateTime selectedExp = initialLotExp;
        if (lotPickerButton != null && lots != null && lots.Count > 0)
        {
            lotPickerButton.Clicked += async (s, e) =>
            {
                var names = lots.Select(x => "Lot: " + x.Lot + (x.Exp != DateTime.MinValue ? "  Exp: " + x.Exp.ToShortDateString() : "")).ToArray();
                var choice = await ShowActionSheetAsync("Select Lot", null, "Cancel", names);
                if (!string.IsNullOrEmpty(choice) && choice != "Cancel")
                {
                    var idx = names.ToList().IndexOf(choice);
                    if (idx >= 0 && idx < lots.Count)
                    {
                        selectedLot = lots[idx].Lot;
                        selectedExp = lots[idx].Exp;
                        lotPickerButton.Text = selectedLot;
                    }
                }
            };
        }

        UnitOfMeasure? selectedUom = initialUom;
        Picker? uomPicker = null;
        var uomList = product?.UnitOfMeasures;
        if (uomList != null && uomList.Count > 0)
        {
            uomPicker = new Picker { Title = "Select UoM" };
            for (int i = 0; i < uomList.Count; i++)
                uomPicker.Items.Add(uomList[i].Name ?? "");
            var toSelect = initialUom ?? product.UnitOfMeasures?.FirstOrDefault();
            if (toSelect != null)
            {
                var idx = uomList.IndexOf(toSelect);
                if (idx >= 0) uomPicker.SelectedIndex = idx;
            }
        }

        var saveBtn = new Button { Text = "Save", BackgroundColor = Color.FromArgb("#0379cb"), TextColor = Colors.White };
        var cancelBtn = new Button { Text = "Cancel", BackgroundColor = Colors.White, TextColor = Colors.Black };

        cancelBtn.Clicked += (s, e) =>
        {
            tcs.TrySetResult(null);
            page.Navigation.PopModalAsync();
        };

        saveBtn.Clicked += async (s, e) =>
        {
            double weight = 0;
            float qty;
            if (isSoldByWeight)
            {
                if (!double.TryParse(qtyWeightEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out weight) || weight <= 0)
                {
                    await page.DisplayAlert("Alert", "Please enter a valid weight.", "OK");
                    return;
                }
                qty = 1f;
            }
            else
            {
                if (!float.TryParse(qtyWeightEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qty) || qty <= 0)
                {
                    await page.DisplayAlert("Alert", "Please enter a valid quantity.", "OK");
                    return;
                }
            }
            qty = (float)Math.Round(qty, Config.Round);
            if (isSoldByWeight) weight = Math.Round(weight, Config.Round);

            double price = initialPrice;
            if (showPrice && priceEntry.IsVisible)
            {
                if (!double.TryParse(priceEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
                {
                    await page.DisplayAlert("Alert", "Not a valid price.", "OK");
                    return;
                }
                price = Math.Round(price, Config.Round);
            }
            if (freeItemCheckbox.IsChecked) price = 0;

            string lot = selectedLot;
            if (lotEntry != null && lotEntry.IsVisible && string.IsNullOrEmpty(lot))
                lot = lotEntry.Text?.Trim() ?? "";

            if (uomPicker != null && uomPicker.SelectedIndex >= 0 && uomList != null && uomPicker.SelectedIndex < uomList.Count)
                selectedUom = uomList[uomPicker.SelectedIndex];

            tcs.TrySetResult(new LaceupMigration.ViewModels.TemplateEditLineResult
            {
                Qty = qty,
                Weight = weight,
                Price = price,
                Comments = commentsEntry.Text?.Trim() ?? "",
                FreeItem = freeItemCheckbox.IsChecked,
                Lot = lot,
                LotExpiration = selectedExp,
                SelectedUoM = selectedUom
            });
            await page.Navigation.PopModalAsync();
        };

        var form = new VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(16, 12, 16, 16),
            Children =
            {
                new Label { Text = productName, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0379cb") },
                new Label { Text = isSoldByWeight ? "Weight:" : "Qty:", FontSize = 14 },
                qtyWeightEntry,
                new Label { Text = "Comments:", FontSize = 14 },
                commentsEntry
            }
        };
        if (showPrice)
        {
            form.Children.Insert(3, new Label { Text = "Price:", FontSize = 14 });
            form.Children.Insert(4, priceEntry);
        }
        if (allowFreeItems)
        {
            var freeRow = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition() }, ColumnSpacing = 8 };
            freeRow.Children.Add(freeItemCheckbox);
            freeRow.Children.Add(freeItemLabel);
            Grid.SetColumn(freeItemLabel, 1);
            form.Children.Add(freeRow);
        }
        if (lotPickerButton != null)
        {
            form.Children.Add(new Label { Text = "Lot:", FontSize = 14 });
            form.Children.Add(lotPickerButton);
        }
        else if (lotEntry != null)
        {
            form.Children.Add(new Label { Text = "Lot:", FontSize = 14 });
            form.Children.Add(lotEntry);
        }
        if (uomPicker != null)
        {
            form.Children.Add(new Label { Text = "UoM:", FontSize = 14 });
            form.Children.Add(uomPicker);
        }
        var buttonRow = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition() }, ColumnSpacing = 8, Margin = new Thickness(0, 16, 0, 0) };
        buttonRow.Children.Add(cancelBtn);
        buttonRow.Children.Add(saveBtn);
        Grid.SetColumn(saveBtn, 1);
        form.Children.Add(buttonRow);

        var border = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E0E0E0"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(0),
            Content = new ScrollView { Content = form }
        };
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition { Width = new GridLength(320) }, new ColumnDefinition() },
            Padding = new Thickness(24)
        };
        Grid.SetRow(border, 1);
        Grid.SetColumn(border, 1);
        overlay.Children.Add(border);

        var popupPage = new ContentPage { BackgroundColor = Colors.Transparent, Content = overlay };
        popupPage.Appearing += (_, __) => qtyWeightEntry.Focus();

        await page.Navigation.PushModalAsync(popupPage);
        return await tcs.Task;
    }
    
}