using LaceupMigration.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// Helper class that mimics Xamarin's ProgressDialog.Show() behavior exactly.
    /// Uses RootGrid approach to avoid reparenting content.
    /// </summary>
    public static class ProgressDialogHelper
    {
        private static LoadingPopup? _currentOverlay;
        private static ContentPage? _currentPage;
        private static Grid? _rootGrid;

        /// <summary>
        /// The helper makes it work like the old Xamarin ProgressDialog - call it from anywhere, no setup needed!
        /// Shows a loading overlay on the current page (matches ProgressDialog.Show())
        /// Blocks interaction, shows spinner, dims background
        /// </summary>
        public static void Show(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var page = GetCurrentPage();
                if (page == null || _currentOverlay != null)
                    return;

                // Only works with ContentPage
                if (page is not ContentPage contentPage)
                    return;

                _currentPage = contentPage;

                // Find or create RootGrid
                _rootGrid = FindOrCreateRootGrid(contentPage);
                if (_rootGrid == null)
                    return;

                // Create and add overlay
                _currentOverlay = new LoadingPopup
                {
                    Message = message,
                    IsVisible = true
                };

                // Add overlay as top layer (last child = on top)
                _rootGrid.Children.Add(_currentOverlay);
            });
        }

        /// <summary>
        /// Updates the loading message (matches progressDialog.SetMessage())
        /// </summary>
        public static void SetMessage(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_currentOverlay != null)
                {
                    _currentOverlay.Message = message;
                }
            });
        }

        /// <summary>
        /// Hides the loading overlay (matches progressDialog.Hide())
        /// </summary>
        public static void Hide()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_rootGrid == null || _currentOverlay == null)
                {
                    Cleanup();
                    return;
                }

                // Remove overlay from RootGrid
                if (_rootGrid.Children.Contains(_currentOverlay))
                {
                    _rootGrid.Children.Remove(_currentOverlay);
                }

                Cleanup();
            });
        }

        private static void Cleanup()
        {
            _currentOverlay = null;
            _currentPage = null;
            _rootGrid = null;
        }

        /// <summary>
        /// Finds RootGrid by name, or creates one if page content is not already a Grid.
        /// This is called ONCE per page - never reparents existing content.
        /// </summary>
        private static Grid? FindOrCreateRootGrid(ContentPage page)
        {
            // Try to find RootGrid by name
            var rootGrid = page.FindByName<Grid>("RootGrid");
            if (rootGrid != null)
                return rootGrid;

            // If page content is already a Grid, use it as RootGrid
            if (page.Content is Grid existingGrid)
            {
                // This Grid will be used as RootGrid
                // Note: If it has x:Name="RootGrid" in XAML, FindByName would have found it above
                return existingGrid;
            }

            // Page content is not a Grid - wrap it ONCE
            // This only happens once at first Show() call
            var originalContent = page.Content;
            if (originalContent == null)
                return null;

            // Create RootGrid and wrap original content
            var newRootGrid = new Grid();
            newRootGrid.Children.Add(originalContent);

            // Set as page content (only happens once)
            page.Content = newRootGrid;

            return newRootGrid;
        }

        private static Page? GetCurrentPage()
        {
            // For .NET MAUI Shell applications
            if (Shell.Current?.CurrentPage != null)
                return Shell.Current.CurrentPage;

            // Fallback for non-Shell apps
            return Application.Current?.Windows?.FirstOrDefault()?.Page;
        }
    }
}
