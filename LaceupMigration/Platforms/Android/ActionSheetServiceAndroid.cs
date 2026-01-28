using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;
using Android.Views;
using IDialogInterface = Android.Content.IDialogInterface;
using AndroidView = Android.Views.View;
using AndroidScrollView = Android.Widget.ScrollView;
using AndroidLinearLayout = Android.Widget.LinearLayout;
using AndroidTextView = Android.Widget.TextView;
using AndroidWidget = Android.Widget;
using AndroidGraphics = Android.Graphics;
using AndroidGraphicsDrawables = Android.Graphics.Drawables;
using AndroidContentRes = Android.Content.Res;

namespace LaceupMigration.Platforms.Android
{
    public class ActionSheetService
    {
        public static Task<string> ShowActionSheetAsync(Context context, string title, string message, string cancelText, string[] buttons)
        {
            var tcs = new TaskCompletionSource<string>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var activity = context as Activity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                    if (activity == null)
                    {
                        var ff = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;
                        tcs.SetResult(ff);
                        return;
                    }

                    // Create AlertDialog builder
                    var builder = new AlertDialog.Builder(activity);
                    
                    // Set width to 320dp (match XAML WidthRequest="320")
                    var widthDp = 320f * activity.Resources.DisplayMetrics.Density;
                    
                    // Create a FrameLayout wrapper to center the dialog content horizontally
                    var wrapperLayout = new global::Android.Widget.FrameLayout(activity);
                    wrapperLayout.SetBackgroundColor(AndroidGraphics.Color.Transparent);
                    var wrapperParams = new global::Android.Widget.FrameLayout.LayoutParams(
                        global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                        global::Android.Views.ViewGroup.LayoutParams.MatchParent
                    );
                    wrapperParams.Gravity = global::Android.Views.GravityFlags.Center;
                    wrapperLayout.LayoutParameters = wrapperParams;
                    
                    // Create main container with rounded corners and iOS-style background
                    var mainContainer = new AndroidLinearLayout(activity)
                    {
                        Orientation = global::Android.Widget.Orientation.Vertical
                    };

                    // Set background color to #F2F2F7 (iOS-style gray)
                    var backgroundColor = AndroidGraphics.Color.ParseColor("#F2F2F7");
                    mainContainer.SetBackgroundColor(backgroundColor);
                    
                    // Create rounded corners drawable (14dp corner radius)
                    var cornerRadius = 14f * activity.Resources.DisplayMetrics.Density;
                    var backgroundDrawable = new AndroidGraphicsDrawables.GradientDrawable();
                    backgroundDrawable.SetColor(backgroundColor);
                    backgroundDrawable.SetCornerRadius(cornerRadius);
                    mainContainer.Background = backgroundDrawable;
                    
                    // Set width to 320dp and center it in the wrapper
                    var mainLayoutParams = new global::Android.Widget.FrameLayout.LayoutParams(
                        (int)widthDp,
                        global::Android.Views.ViewGroup.LayoutParams.WrapContent
                    );
                    mainLayoutParams.Gravity = global::Android.Views.GravityFlags.Center;
                    mainContainer.LayoutParameters = mainLayoutParams;
                    
                    // Add main container to wrapper
                    wrapperLayout.AddView(mainContainer);
                    
                    // Create header section with title and message (match XAML design)
                    if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(message))
                    {
                        var headerContainer = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Vertical
                        };
                        headerContainer.SetPadding(
                            (int)(20 * activity.Resources.DisplayMetrics.Density), // 20dp left/right
                            (int)(20 * activity.Resources.DisplayMetrics.Density), // 20dp top
                            (int)(20 * activity.Resources.DisplayMetrics.Density),
                            (int)(16 * activity.Resources.DisplayMetrics.Density) // 16dp bottom
                        );
                        headerContainer.SetBackgroundColor(AndroidGraphics.Color.Transparent);
                        
                        // Title text view - 18sp, DimGray, centered
                        if (!string.IsNullOrEmpty(title))
                        {
                            var titleView = new AndroidTextView(activity)
                            {
                                Text = title,
                                TextSize = 18f, // 18sp
                                Typeface = AndroidGraphics.Typeface.Default,
                                Gravity = global::Android.Views.GravityFlags.CenterHorizontal | global::Android.Views.GravityFlags.CenterVertical
                            };
                            // DimGray color (#696969 or RGB(105, 105, 105))
                            titleView.SetTextColor(AndroidGraphics.Color.Rgb(105, 105, 105));
                            titleView.SetPadding(0, 0, 0, 0);
                            
                            headerContainer.AddView(titleView);
                        }
                        
                        // Message/subtitle text view - 14sp, lighter gray, centered, below title
                        if (!string.IsNullOrEmpty(message))
                        {
                            var messageView = new AndroidTextView(activity)
                            {
                                Text = message,
                                TextSize = 14f, // 14sp (smaller than title)
                                Typeface = AndroidGraphics.Typeface.Default,
                                Gravity = global::Android.Views.GravityFlags.CenterHorizontal | global::Android.Views.GravityFlags.CenterVertical
                            };
                            // Lighter gray color for subtitle
                            messageView.SetTextColor(AndroidGraphics.Color.Rgb(140, 140, 140));
                            messageView.SetPadding(0, 
                                (int)(4 * activity.Resources.DisplayMetrics.Density), // 4dp top margin from title
                                0, 
                                0);
                            
                            headerContainer.AddView(messageView);
                        }
                        
                        mainContainer.AddView(headerContainer);
                        
                        // Header separator - 0.5dp height, #C6C6C8 color
                        var headerSeparatorHeight = Math.Max(1, (int)(0.5f * activity.Resources.DisplayMetrics.Density)); // At least 1 pixel
                        var headerSeparator = new AndroidView(activity)
                        {
                            LayoutParameters = new AndroidLinearLayout.LayoutParams(
                                global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                                headerSeparatorHeight
                            )
                        };
                        headerSeparator.SetBackgroundColor(AndroidGraphics.Color.ParseColor("#C6C6C8"));
                        headerSeparator.SetMinimumHeight(headerSeparatorHeight);
                        headerSeparator.SetPadding(0, 0, 0, 0);
                        mainContainer.AddView(headerSeparator);
                    }

                    // Create custom layout for the list items
                    var layout = new AndroidLinearLayout(activity)
                    {
                        Orientation = global::Android.Widget.Orientation.Vertical
                    };
                    layout.SetPadding(0, 0, 0, 0);
                    layout.SetBackgroundColor(AndroidGraphics.Color.Transparent);

                    // Ensure cancel text has a default value
                    var finalCancelText = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;

                    // Handle null or empty buttons array
                    if (buttons == null)
                    {
                        buttons = new string[0];
                    }

                    // Store button texts and clickable items for click handlers
                    var buttonTexts = new string[buttons.Length];
                    Array.Copy(buttons, buttonTexts, buttons.Length);
                    var clickableItems = new AndroidLinearLayout[buttons.Length];

                    // Create list of items with separators (match XAML design)
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        var button = buttons[i];
                        
                        // Add separator line before item (except for first item) - 0.5dp height, #C6C6C8 color
                        if (i > 0)
                        {
                            var separatorHeight = Math.Max(1, (int)(0.5f * activity.Resources.DisplayMetrics.Density)); // At least 1 pixel
                            var separator = new AndroidView(activity)
                            {
                                LayoutParameters = new AndroidLinearLayout.LayoutParams(
                                    global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                                    separatorHeight
                                )
                            };
                            separator.SetBackgroundColor(AndroidGraphics.Color.ParseColor("#C6C6C8"));
                            separator.SetPadding(0, 0, 0, 0);
                            separator.SetMinimumHeight(separatorHeight);
                            layout.AddView(separator);
                        }
                        
                        // Create item container - 50dp height, 10dp padding (match XAML)
                        var itemContainer = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Horizontal
                        };
                        
                        itemContainer.SetGravity(GravityFlags.CenterVertical);
                        itemContainer.SetPadding(
                            (int)(10 * activity.Resources.DisplayMetrics.Density), // 10dp all sides
                            (int)(10 * activity.Resources.DisplayMetrics.Density),
                            (int)(10 * activity.Resources.DisplayMetrics.Density),
                            (int)(10 * activity.Resources.DisplayMetrics.Density)
                        );
                        
                        // Set height to 50dp (match XAML HeightRequest="50")
                        var containerLayoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            (int)(50 * activity.Resources.DisplayMetrics.Density)
                        );
                        itemContainer.LayoutParameters = containerLayoutParams;

                        // Create text view for the item - 17sp, #007AFF blue (match XAML)
                        // Center text horizontally and vertically
                        var textView = new AndroidTextView(activity)
                        {
                            Text = button,
                            TextSize = 17f, // 17sp
                            Gravity = global::Android.Views.GravityFlags.Center // Center both horizontally and vertically
                        };
                        
                        // #007AFF blue color (iOS system blue)
                        textView.SetTextColor(AndroidGraphics.Color.ParseColor("#007AFF"));
                        textView.SetPadding(0, 0, 0, 0);
                        textView.SetTypeface(AndroidGraphics.Typeface.Default, AndroidGraphics.TypefaceStyle.Normal);
                        
                        // Use MatchParent width to allow centering within the full width
                        var textLayoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent
                        );
                        textLayoutParams.Gravity = global::Android.Views.GravityFlags.Center;
                        textView.LayoutParameters = textLayoutParams;

                        itemContainer.AddView(textView);

                        // Create clickable item
                        var clickableItem = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Vertical,
                            Clickable = true,
                            Focusable = true,
                        };
                        
                        clickableItem.SetGravity(GravityFlags.CenterVertical);

                        // Create ripple drawable for selection highlight
                        var rippleColor = AndroidGraphics.Color.ParseColor("#E1E1E1");
                        var colorStateList = AndroidContentRes.ColorStateList.ValueOf(rippleColor);
                        var contentDrawable = new AndroidGraphicsDrawables.ColorDrawable(AndroidGraphics.Color.Transparent);
                        var rippleDrawable = new AndroidGraphicsDrawables.RippleDrawable(colorStateList, contentDrawable, null);
                        clickableItem.Background = rippleDrawable;
                        
                        var clickableLayoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent
                        );
                        clickableItem.LayoutParameters = clickableLayoutParams;
                        
                        clickableItem.AddView(itemContainer);
                        
                        // Store clickable item for later click handler attachment
                        clickableItems[i] = clickableItem;

                        layout.AddView(clickableItem);
                    }

                    // Add list to main container
                    mainContainer.AddView(layout);
                    
                    // Add separator before buttons section - ensure it's visible
                    var buttonSeparatorHeight = Math.Max(1, (int)(0.5f * activity.Resources.DisplayMetrics.Density)); // At least 1 pixel
                    var buttonSeparator = new AndroidView(activity)
                    {
                        LayoutParameters = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            buttonSeparatorHeight
                        )
                    };
                    buttonSeparator.SetBackgroundColor(AndroidGraphics.Color.ParseColor("#C6C6C8"));
                    buttonSeparator.SetMinimumHeight(buttonSeparatorHeight);
                    buttonSeparator.SetPadding(0, 0, 0, 0);
                    mainContainer.AddView(buttonSeparator);
                    
                    // Create buttons section - Cancel only
                    var buttonsContainer = new AndroidLinearLayout(activity)
                    {
                        Orientation = global::Android.Widget.Orientation.Horizontal
                    };
                    buttonsContainer.SetBackgroundColor(AndroidGraphics.Color.Transparent);
                    
                    var buttonsLayoutParams = new AndroidLinearLayout.LayoutParams(
                        global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                        (int)(44 * activity.Resources.DisplayMetrics.Density) // 44dp height
                    );
                    buttonsContainer.LayoutParameters = buttonsLayoutParams;
                    
                    // Cancel button - full width
                    var cancelButton = new AndroidTextView(activity)
                    {
                        Text = finalCancelText,
                        TextSize = 17f, // 17sp
                        Gravity = global::Android.Views.GravityFlags.Center,
                        Clickable = true,
                        Focusable = true
                    };
                    cancelButton.SetTextColor(AndroidGraphics.Color.ParseColor("#007AFF")); // Blue
                    cancelButton.SetTypeface(AndroidGraphics.Typeface.Default, AndroidGraphics.TypefaceStyle.Normal); // Normal (not bold)
                    
                    var cancelLayoutParams = new AndroidLinearLayout.LayoutParams(
                        global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                        global::Android.Views.ViewGroup.LayoutParams.MatchParent
                    );
                    cancelButton.LayoutParameters = cancelLayoutParams;
                    
                    // Create ripple for cancel button
                    var cancelRippleDrawable = new AndroidGraphicsDrawables.RippleDrawable(
                        AndroidContentRes.ColorStateList.ValueOf(AndroidGraphics.Color.ParseColor("#E1E1E1")),
                        new AndroidGraphicsDrawables.ColorDrawable(AndroidGraphics.Color.Transparent),
                        null);
                    cancelButton.Background = cancelRippleDrawable;
                    
                    buttonsContainer.AddView(cancelButton);
                    mainContainer.AddView(buttonsContainer);
                    
                    // Create scroll view if needed (for many items)
                    AndroidScrollView scrollView = null;
                    if (buttons.Length > 6)
                    {
                        scrollView = new AndroidScrollView(activity);
                        scrollView.SetBackgroundColor(AndroidGraphics.Color.Transparent);
                        scrollView.FillViewport = false; // Don't fill viewport to allow proper centering
                        
                        // Calculate max height based on screen size (leave some padding for margins)
                        var displayMetrics = activity.Resources.DisplayMetrics;
                        var screenHeight = displayMetrics.HeightPixels;
                        var maxHeight = (int)(screenHeight * 0.75f); // Use 75% of screen height max
                        
                        // Remove mainContainer from wrapper and add to scrollview
                        wrapperLayout.RemoveView(mainContainer);
                        
                        // Update mainContainer layout params for ScrollView (ScrollView extends FrameLayout)
                        // Width should match ScrollView width, height wraps content
                        var mainContainerScrollParams = new global::Android.Widget.FrameLayout.LayoutParams(
                            (int)widthDp, // Fixed width to match design
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent // Height wraps content
                        );
                        mainContainer.LayoutParameters = mainContainerScrollParams;
                        
                        scrollView.AddView(mainContainer);
                        
                        // Add scrollview to wrapper with constrained height to ensure centering
                        // When content is large, use maxHeight instead of WrapContent
                        var scrollParams = new global::Android.Widget.FrameLayout.LayoutParams(
                            (int)widthDp, // Match the main container width
                            maxHeight // Use fixed max height to ensure proper centering
                        );
                        scrollParams.Gravity = global::Android.Views.GravityFlags.Center;
                        scrollView.LayoutParameters = scrollParams;
                        wrapperLayout.AddView(scrollView);
                    }

                    // Set the custom view - use wrapperLayout which centers the content
                    builder.SetView(wrapperLayout);
                    
                    // Remove default dialog background and padding to match XAML design
                    builder.SetCancelable(true);

                    // Create dialog
                    var dialog = builder.Create();
                    dialog.SetCanceledOnTouchOutside(true);
                    dialog.SetOnCancelListener(new DialogCancelListener(() =>
                    {
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(finalCancelText);
                        }
                    }));
                    
                    // Remove default dialog padding and background to match XAML design
                    // Center the dialog on screen
                    var window = dialog.Window;
                    if (window != null)
                    {
                        window.SetBackgroundDrawable(new AndroidGraphicsDrawables.ColorDrawable(AndroidGraphics.Color.Transparent));
                        
                        // Set window attributes to center the dialog
                        var attributes = window.Attributes;
                        if (attributes != null)
                        {
                            // Use MatchParent width so the wrapper can center the content
                            attributes.Width = global::Android.Views.ViewGroup.LayoutParams.MatchParent;
                            attributes.Height = global::Android.Views.ViewGroup.LayoutParams.MatchParent;
                            attributes.Gravity = global::Android.Views.GravityFlags.Center; // Center on screen
                            attributes.HorizontalMargin = 0f;
                            attributes.VerticalMargin = 0f;
                            window.Attributes = attributes;
                        }
                        
                        // Set layout to match parent width so wrapper can center content
                        window.SetLayout(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent
                        );
                        
                        // Ensure the dialog is displayed centered
                        window.SetGravity(global::Android.Views.GravityFlags.Center);
                    }

                    // Attach click handlers to list items
                    for (int j = 0; j < clickableItems.Length; j++)
                    {
                        var clickableItem = clickableItems[j];
                        if (clickableItem != null)
                        {
                            var buttonText = buttonTexts[j]; // Capture button text
                            clickableItem.Click += (s, e) =>
                            {
                                dialog.Dismiss();
                                tcs.SetResult(buttonText);
                            };
                        }
                    }
                    
                    // Attach click handler to Cancel button
                    cancelButton.Click += (s, e) =>
                    {
                        dialog.Dismiss();
                        tcs.SetResult(finalCancelText);
                    };

                    // Show dialog
                    dialog.Show();
                }
                catch (Exception ex)
                {
                    // Fallback to default implementation on error
                    System.Diagnostics.Debug.WriteLine($"Error showing custom action sheet: {ex.Message}");
                    var finalCancelText = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;
                    tcs.SetResult(finalCancelText);
                }
            });

            return tcs.Task;
        }

        private class DialogCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            private readonly Action _onCancel;

            public DialogCancelListener(Action onCancel)
            {
                _onCancel = onCancel;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                _onCancel?.Invoke();
            }
        }
    }
}
