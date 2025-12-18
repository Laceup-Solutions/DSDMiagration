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
                    
                    // Create main container with header and list
                    var mainContainer = new AndroidLinearLayout(activity)
                    {
                        Orientation = global::Android.Widget.Orientation.Vertical
                    };
                    mainContainer.SetBackgroundColor(global::Android.Graphics.Color.White);
                    
                    // Create header section with title
                    if (!string.IsNullOrEmpty(title))
                    {
                        var headerContainer = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Vertical
                        };
                        headerContainer.SetPadding(
                            (int)(20 * activity.Resources.DisplayMetrics.Density), // 20dp left/right
                            (int)(16 * activity.Resources.DisplayMetrics.Density), // 16dp top
                            (int)(20 * activity.Resources.DisplayMetrics.Density),
                            (int)(12 * activity.Resources.DisplayMetrics.Density) // 12dp bottom
                        );
                        headerContainer.SetBackgroundColor(global::Android.Graphics.Color.White);
                        
                        // Title text view - gray, smaller, centered horizontally
                        var titleView = new AndroidTextView(activity)
                        {
                            Text = title,
                            TextSize = 14f, // Smaller text
                            Typeface = global::Android.Graphics.Typeface.Default,
                            Gravity = global::Android.Views.GravityFlags.CenterHorizontal | global::Android.Views.GravityFlags.CenterVertical
                        };
                        titleView.SetTextColor(global::Android.Graphics.Color.Rgb(128, 128, 128)); // Gray color
                        titleView.SetPadding(0, 0, 0, 0);
                        
                        headerContainer.AddView(titleView);
                        mainContainer.AddView(headerContainer);
                    }

                    // Create custom layout for the list items
                    var layout = new AndroidLinearLayout(activity)
                    {
                        Orientation = global::Android.Widget.Orientation.Vertical
                    };
                    layout.SetPadding(0, 0, 0, 0);

                    // Ensure cancel text has a default value
                    var finalCancelText = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;

                    // Create combined array with all buttons plus cancel button as last item
                    var allButtons = new string[buttons.Length + 1];
                    Array.Copy(buttons, allButtons, buttons.Length);
                    allButtons[buttons.Length] = finalCancelText;

                    // Store button texts and clickable items for click handlers
                    var buttonTexts = new string[allButtons.Length];
                    Array.Copy(allButtons, buttonTexts, allButtons.Length);
                    var clickableItems = new AndroidLinearLayout[allButtons.Length];

                    // Create list of items with separators and reduced margins
                    for (int i = 0; i < allButtons.Length; i++)
                    {
                        var button = allButtons[i];
                        
                        // Add separator line before item (except for first item) - full width, no padding
                        if (i > 0)
                        {
                            var separator = new AndroidView(activity)
                            {
                                LayoutParameters = new AndroidLinearLayout.LayoutParams(
                                    global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                                    (int)(1 * activity.Resources.DisplayMetrics.Density) // 1dp height
                                )
                            };
                            separator.SetBackgroundColor(global::Android.Graphics.Color.Rgb(224, 224, 224)); // Light gray separator
                            separator.SetPadding(0, 0, 0, 0); // No padding - full width
                            layout.AddView(separator);
                        }
                        
                        // Create item container with modern padding and vertical centering
                        var itemContainer = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Vertical,
                        };
                        
                        itemContainer.SetGravity(GravityFlags.CenterVertical);
                        
                        itemContainer.SetPadding(
                            (int)(20 * activity.Resources.DisplayMetrics.Density), // 20dp left/right
                            (int)(16 * activity.Resources.DisplayMetrics.Density), // 16dp top/bottom
                            (int)(20 * activity.Resources.DisplayMetrics.Density),
                            (int)(16 * activity.Resources.DisplayMetrics.Density)
                        );
                        
                        // Set minimum height for better vertical centering
                        var containerLayoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent
                        );
                        containerLayoutParams.Gravity = global::Android.Views.GravityFlags.CenterVertical;
                        itemContainer.LayoutParameters = containerLayoutParams;

                        // Create text view for the item with modern styling - vertically centered
                        var textView = new AndroidTextView(activity)
                        {
                            Text = button,
                            TextSize = 16f,
                            Gravity = global::Android.Views.GravityFlags.Start | global::Android.Views.GravityFlags.CenterVertical
                        };
                        
                        textView.SetTextColor(global::Android.Graphics.Color.Rgb(33, 33, 33)); // Dark gray text
                        textView.SetPadding(0, 0, 0, 0);
                        textView.SetTypeface(global::Android.Graphics.Typeface.Default, global::Android.Graphics.TypefaceStyle.Normal);
                        
                        // Set minimum height to ensure vertical centering works properly
                        var layoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent
                        );
                        layoutParams.Gravity = global::Android.Views.GravityFlags.CenterVertical;
                        textView.LayoutParameters = layoutParams;

                        itemContainer.AddView(textView);

                        // Create clickable item with vertical centering
                        var clickableItem = new AndroidLinearLayout(activity)
                        {
                            Orientation = global::Android.Widget.Orientation.Vertical,
                            Clickable = true,
                            Focusable = true,
                        };
                        
                        clickableItem.SetGravity(GravityFlags.CenterVertical);

                        // Use custom drawable with Gray100 (#E1E1E1) instead of default orange
                        // Create ripple drawable with Gray100 for selection highlight
                        var gray100Color = global::Android.Graphics.Color.ParseColor("#E1E1E1");
                        var colorStateList = global::Android.Content.Res.ColorStateList.ValueOf(gray100Color);
                        var contentDrawable = new global::Android.Graphics.Drawables.ColorDrawable(global::Android.Graphics.Color.Transparent);
                        var rippleDrawable = new global::Android.Graphics.Drawables.RippleDrawable(colorStateList, contentDrawable, null);
                        clickableItem.Background = rippleDrawable;
                        
                        var clickableLayoutParams = new AndroidLinearLayout.LayoutParams(
                            global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                            global::Android.Views.ViewGroup.LayoutParams.WrapContent
                        );
                        clickableLayoutParams.Gravity = global::Android.Views.GravityFlags.CenterVertical;
                        clickableItem.LayoutParameters = clickableLayoutParams;
                        
                        clickableItem.AddView(itemContainer);
                        
                        // Store clickable item for later click handler attachment
                        clickableItems[i] = clickableItem;

                        layout.AddView(clickableItem);
                    }

                    // Add list to main container
                    mainContainer.AddView(layout);
                    
                    // Create scroll view if needed (for many items)
                    AndroidScrollView scrollView = null;
                    if (allButtons.Length > 6)
                    {
                        scrollView = new AndroidScrollView(activity);
                        scrollView.AddView(mainContainer);
                    }

                    // Set the custom view
                    builder.SetView(scrollView ?? (AndroidView)mainContainer);

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

                    // Attach click handlers to items (after dialog is created)
                    // Use the stored clickableItems array instead of iterating layout children
                    // because separators are now separate views in the layout
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
