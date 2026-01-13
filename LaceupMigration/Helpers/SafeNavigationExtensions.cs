using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// Extension methods for safe navigation that prevents double-tap issues.
    /// Use these methods instead of Shell.Current.GoToAsync throughout the app.
    /// </summary>
    public static class SafeNavigationExtensions
    {
        /// <summary>
        /// Safely navigates to a route, preventing double-tap navigation.
        /// This is a drop-in replacement for Shell.Current.GoToAsync.
        /// </summary>
        public static async Task SafeGoToAsync(this Shell shell, string route, bool saveState = true)
        {
            await NavigationHelper.GoToAsync(route, saveState);
        }

        /// <summary>
        /// Safely navigates to a route with parameters, preventing double-tap navigation.
        /// This is a drop-in replacement for Shell.Current.GoToAsync with parameters.
        /// </summary>
        public static async Task SafeGoToAsync(this Shell shell, string route, IDictionary<string, object> parameters, bool saveState = true)
        {
            await NavigationHelper.GoToAsync(route, parameters, saveState);
        }
    }
}

