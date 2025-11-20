# ProgressDialogHelper Usage Guide

This document shows how to use the MAUI loading overlay that matches Xamarin's `ProgressDialog.Show()` behavior.

## Overview

The `ProgressDialogHelper` provides a modal loading overlay that:
- Blocks UI interaction (modal)
- Shows a dim gray overlay behind the loading panel
- Displays a spinner and text label (text updates via `SetMessage()`)
- Appears immediately when called
- Hides only when `Hide()` is called
- Uses RootGrid approach to avoid reparenting content

## Setup: Adding RootGrid to ContentPage

Every ContentPage that needs the loading overlay should have a `RootGrid` as the root element:

### Example 1: LoginConfigPage (already updated)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="LaceupMigration.LoginConfigPage">
    <Grid x:Name="RootGrid">
        <!-- Your page content here -->
        <ScrollView>
            <VerticalStackLayout>
                <!-- Content -->
            </VerticalStackLayout>
        </ScrollView>
    </Grid>
</ContentPage>
```

### Example 2: Page with existing Grid

If your page already has a Grid as root, just add `x:Name="RootGrid"`:

```xml
<ContentPage>
    <Grid x:Name="RootGrid" RowDefinitions="Auto,*,Auto">
        <!-- Your existing content -->
    </Grid>
</ContentPage>
```

### Example 3: Page without Grid

If your page doesn't have a Grid, wrap the content:

```xml
<ContentPage>
    <Grid x:Name="RootGrid">
        <!-- Wrap your existing content -->
        <ScrollView>
            <!-- Original content -->
        </ScrollView>
    </Grid>
</ContentPage>
```

**Note:** The helper will automatically wrap non-Grid content ONCE on first `Show()` call, but it's better to add RootGrid in XAML.

## Usage: Show, SetMessage, Hide

### Basic Usage (matches Xamarin pattern)

```csharp
using LaceupMigration.Helpers;

// Show loading overlay (matches ProgressDialog.Show())
ProgressDialogHelper.Show("Loading...");

// Run work on background thread
ThreadPool.QueueUserWorkItem(delegate (object stt)
{
    try
    {
        // Do work...
        
        // Update message (matches progressDialog.SetMessage())
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressDialogHelper.SetMessage("Downloading data...");
        });
        
        // More work...
    }
    catch (Exception ex)
    {
        Logger.CreateLog(ex);
    }
    finally
    {
        // Hide overlay (matches progressDialog.Hide())
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressDialogHelper.Hide();
        });
        
        // Handle errors/navigation on main thread
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Show alerts, navigate, etc.
        });
    }
});
```

### Complete Example: LoginConfigPageViewModel.ContinueSignInAsync()

```csharp
private void ContinueSignInAsync()
{
    // Show loading overlay (matches Xamarin ProgressDialog.Show)
    ProgressDialogHelper.Show("Loading...");

    // Run work on background thread (matches Xamarin ThreadPool.QueueUserWorkItem)
    ThreadPool.QueueUserWorkItem(delegate (object stt)
    {
        int error = 0;
        try
        {
            DataAccess.GetUserSettingLine();
            DataAccess.CheckAuthorization();
            error = 1;

            if (!Config.AuthorizationFailed)
            {
                DataAccess.GetSalesmanSettings(false);
                error = 2;

                if (Config.EnableSelfServiceModule)
                {
                    // Update message when downloading data (matches Xamarin progressDialog.SetMessage)
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProgressDialogHelper.SetMessage("Downloading data.");
                    });
                    DataAccessEx.DownloadStaticData();
                }

                error = 3;
            }
        }
        catch (Exception ex)
        {
            Logger.CreateLog(ex);
        }
        finally
        {
            // Hide loading overlay (matches Xamarin progressDialog.Hide)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressDialogHelper.Hide();
            });

            // Handle errors and navigation on main thread (matches Xamarin RunOnUiThread)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (error == 0)
                {
                    await DialogHelper._dialogService.ShowAlertAsync("Connection error.", "Alert", "OK");
                    return;
                }

                if (Config.AuthorizationFailed)
                {
                    await DialogHelper._dialogService.ShowAlertAsync("Not authorized.", "Alert", "OK");
                    return;
                }

                // Continue with navigation...
            });
        }
    });
}
```

## API Reference

### `ProgressDialogHelper.Show(string message = "Loading...")`

Shows the loading overlay on the current page.
- **Parameters:**
  - `message` (optional): Initial message to display. Defaults to "Loading...".
- **Behavior:**
  - Finds or creates RootGrid
  - Adds LoadingOverlay as top layer
  - Blocks all UI interaction
  - Runs on main thread automatically

### `ProgressDialogHelper.SetMessage(string message)`

Updates the loading message text.
- **Parameters:**
  - `message`: New message to display.
- **Behavior:**
  - Updates label text immediately
  - Runs on main thread automatically
  - Safe to call from background threads

### `ProgressDialogHelper.Hide()`

Hides the loading overlay.
- **Behavior:**
  - Removes overlay from RootGrid
  - Restores UI interaction
  - Cleans up references
  - Runs on main thread automatically

## Important Notes

1. **RootGrid Required:** Every page must have a `RootGrid` (either in XAML or created automatically).

2. **No Reparenting:** The helper never reparents existing content. It only adds/removes the overlay as a Grid child.

3. **Thread Safety:** All methods use `MainThread.BeginInvokeOnMainThread()` internally, so they're safe to call from any thread.

4. **Single Overlay:** Only one overlay can be shown at a time. Calling `Show()` while one is already visible does nothing.

5. **Android Safe:** This approach avoids `IllegalStateException` by never moving views between parents.

## Files

- `LaceupMigration/Controls/LoadingOverlay.xaml` - Overlay UI
- `LaceupMigration/Controls/LoadingOverlay.xaml.cs` - Overlay code-behind
- `LaceupMigration/Helpers/ProgressDialogHelper.cs` - Helper class
- `LaceupMigration/Views/LoginConfigPage.xaml` - Example page with RootGrid

