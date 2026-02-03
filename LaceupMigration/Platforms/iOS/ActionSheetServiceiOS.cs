using UIKit;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;
using CoreGraphics;

namespace LaceupMigration.Platforms.iOS
{
    public class ActionSheetService
    {
        public static Task<string> ShowActionSheetAsync(string title, string message, string cancelText, string[] buttons)
        {
            var tcs = new TaskCompletionSource<string>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Get the current view controller
                    UIViewController viewController = null;
                    
                    // Try to get the key window's root view controller
                    var windows = UIApplication.SharedApplication.Windows;
                    foreach (var window in windows)
                    {
                        if (window.IsKeyWindow)
                        {
                            viewController = window.RootViewController;
                            break;
                        }
                    }
                    
                    // Fallback to first window if no key window found
                    if (viewController == null && windows.Length > 0)
                    {
                        viewController = windows[0].RootViewController;
                    }
                    
                    // Ensure we get the topmost view controller
                    while (viewController?.PresentedViewController != null)
                    {
                        viewController = viewController.PresentedViewController;
                    }

                    if (viewController == null)
                    {
                        var finalCancelText = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;
                        tcs.SetResult(finalCancelText);
                        return;
                    }

                    // Ensure cancel text has a default value
                    var finalCancelTextValue = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;

                    // Handle null or empty buttons array
                    if (buttons == null)
                    {
                        buttons = new string[0];
                    }

                    // Create UIAlertController with ActionSheet style
                    // Use UIAlertControllerStyle.ActionSheet to get the bottom sheet style
                    var alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.ActionSheet);

                    // Add action buttons (without cancel - we'll add it separately at the bottom)
                    foreach (var button in buttons)
                    {
                        if (!string.IsNullOrEmpty(button))
                        {
                            var action = UIAlertAction.Create(button, UIAlertActionStyle.Default, (action) =>
                            {
                                tcs.SetResult(button);
                            });
                            alertController.AddAction(action);
                        }
                    }

                    // Add cancel button at the bottom (this will be the only cancel button)
                    var cancelAction = UIAlertAction.Create(finalCancelTextValue, UIAlertActionStyle.Cancel, (action) =>
                    {
                        tcs.SetResult(finalCancelTextValue);
                    });
                    alertController.AddAction(cancelAction);

                    // Configure for iPad (action sheets need a popover on iPad)
                    if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                    {
                        var popover = alertController.PopoverPresentationController;
                        if (popover != null)
                        {
                            popover.SourceView = viewController.View;
                            popover.SourceRect = new CGRect(
                                viewController.View.Bounds.Width / 2,
                                viewController.View.Bounds.Height / 2,
                                0, 0);
                            popover.PermittedArrowDirections = UIPopoverArrowDirection.Unknown;
                        }
                    }

                    // Present the alert controller
                    viewController.PresentViewController(alertController, true, null);
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
    }
}

