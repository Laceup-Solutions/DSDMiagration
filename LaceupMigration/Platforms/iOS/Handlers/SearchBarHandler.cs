using UIKit;
using Microsoft.Maui.Handlers;
using Foundation;

namespace LaceupMigration.Platforms.iOS.Handlers
{
    public static class SearchBarHandlerMapper
    {
        public static void MapSearchBar(ISearchBarHandler handler, Microsoft.Maui.ISearchBar searchBar)
        {
            if (handler.PlatformView is UISearchBar platformView)
            {
                ConfigureSearchBar(platformView);
            }
        }

        private static void ConfigureSearchBar(UISearchBar platformView)
        {
            // Set white background
            SetWhiteBackground(platformView);
            
            // Add Done button to keyboard
            AddDoneButtonToKeyboard(platformView);
        }

        private static void AddDoneButtonToKeyboard(UISearchBar platformView)
        {
            // Create toolbar with Done button
            var toolbar = new UIToolbar(new CoreGraphics.CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
            {
                BarStyle = UIBarStyle.Default,
                Translucent = true
            };

            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) =>
            {
                platformView.ResignFirstResponder();
            });

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                doneButton
            };

            // Set input accessory view for the search bar's text field
            // Access the search field using ValueForKey
            var searchField = platformView.ValueForKey(new NSString("searchField")) as UITextField;
            if (searchField != null)
            {
                searchField.InputAccessoryView = toolbar;
            }
            else
            {
                // If search field is not available yet, try again after layout
                // Use a small delay to ensure the search field is initialized
                NSTimer.CreateScheduledTimer(0.1, (timer) =>
                {
                    var delayedSearchField = platformView.ValueForKey(new NSString("searchField")) as UITextField;
                    if (delayedSearchField != null)
                    {
                        delayedSearchField.InputAccessoryView = toolbar;
                    }
                    timer.Invalidate();
                });
            }
        }

        private static void SetWhiteBackground(UISearchBar platformView)
        {
            // Set search bar background to white
            platformView.BackgroundColor = UIColor.White;
            platformView.BarTintColor = UIColor.White;
            
            // Set text field background to white
            var searchField = platformView.ValueForKey(new NSString("searchField")) as UITextField;
            if (searchField != null)
            {
                searchField.BackgroundColor = UIColor.White;
            }
        }
    }
}

