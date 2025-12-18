using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// Tracks navigation events to build the full navigation stack.
    /// This ensures we can restore the complete navigation path, not just the current page.
    /// </summary>
    public static class NavigationTracker
    {
        private static bool _initialized = false;
        private static Shell? _shell;

        /// <summary>
        /// Initializes navigation tracking by subscribing to Shell navigation events.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            // Check if Shell.Current is available
            if (Shell.Current == null)
            {
                // Shell not ready yet, will be initialized later
                return;
            }

            _shell = Shell.Current;
            _shell.Navigated += OnShellNavigated;
            _initialized = true;
        }

        /// <summary>
        /// Initializes navigation tracking with a specific Shell instance.
        /// Use this when Shell.Current is not yet available.
        /// </summary>
        public static void Initialize(Shell shell)
        {
            if (_initialized)
                return;

            if (shell == null)
                return;

            _shell = shell;
            _shell.Navigated += OnShellNavigated;
            _initialized = true;
        }

        /// <summary>
        /// Handles Shell navigation events to track the navigation stack.
        /// This ensures intermediate pages in the navigation path are also saved to the stack.
        /// </summary>
        private static void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                var location = e.Current?.Location?.OriginalString;
                if (string.IsNullOrEmpty(location))
                    return;

                // Skip if navigating to splash or login pages (these clear the stack)
                // Also skip if location is empty or just contains navigation separators
                if (string.IsNullOrEmpty(location) ||
                    location.Contains("splash") || 
                    location.Contains("login") || 
                    location.Contains("termsandconditions") ||
                    location == "//" || location == "/")
                {
                    return;
                }

                // Check if this is a back navigation (going to a parent/previous page)
                // If the previous location had more depth, this is likely a back navigation
                var previousLocation = e.Previous?.Location?.OriginalString ?? "";
                var currentDepth = location.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("//")).Count();
                var previousDepth = previousLocation.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("//")).Count();
                
                // If current depth is less than previous, this is likely a back navigation
                // Don't save state on back navigation - pages handle their own state removal
                if (currentDepth < previousDepth && previousDepth > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationTracker] Detected back navigation (depth {previousDepth} -> {currentDepth}), skipping state save");
                    return;
                }

                // Extract all route parts from the location
                // Location format: "//MainPage/Clients/clientdetails?clientId=123/batch?batchId=456"
                var routeParts = location.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("//")).ToList();
                if (routeParts.Count == 0)
                    return;

                // Tab routes that are part of Shell structure
                var tabRoutes = new[] { "MainPage", "Clients", "Invoices", "Orders", "Payments" };

                // Save state for each meaningful route in the path
                // This builds the full navigation stack
                // Only save the last (deepest) route to avoid interfering with explicit state saves
                // Pages that explicitly save state (like BatchPage) will handle their own state
                var lastRoute = routeParts.LastOrDefault();
                if (!string.IsNullOrEmpty(lastRoute))
                {
                    // Skip tab routes (they're part of Shell structure, not navigation destinations)
                    if (!tabRoutes.Contains(lastRoute, StringComparer.OrdinalIgnoreCase))
                    {
                        // Check if this route already has an explicit state save with query parameters
                        // If so, don't save it again (pages like BatchPage save their own state with params)
                        var baseRoute = lastRoute.Split('?')[0];
                        var hasQueryParams = lastRoute.Contains("?");
                        
                        // Only save if the route doesn't have query parameters
                        // Routes with query parameters are saved explicitly by the pages themselves
                        if (!hasQueryParams)
                        {
                            NavigationHelper.SaveNavigationState(lastRoute);
                            System.Diagnostics.Debug.WriteLine($"[NavigationTracker] Saved state for route {lastRoute}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[NavigationTracker] Skipping route {lastRoute} - has query params, page will save explicitly");
                        }
                    }
                    else if (lastRoute.Equals("MainPage", StringComparison.OrdinalIgnoreCase))
                    {
                        NavigationHelper.SaveNavigationState("///MainPage");
                        System.Diagnostics.Debug.WriteLine("[NavigationTracker] Saved state for MainPage");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationTracker] Error tracking navigation: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up navigation tracking.
        /// </summary>
        public static void Cleanup()
        {
            if (_initialized && _shell != null)
            {
                _shell.Navigated -= OnShellNavigated;
                _shell = null;
                _initialized = false;
            }
        }
    }
}

