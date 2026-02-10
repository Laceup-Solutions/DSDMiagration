using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// Helper class for navigation that automatically tracks ActivityState and prevents double-tap navigation.
    /// 
    /// IMPORTANT: Use NavigationHelper.GoToAsync instead of Shell.Current.GoToAsync throughout the app
    /// to prevent double-tap issues that can cause multiple pages to open.
    /// 
    /// Examples:
    ///   // Simple route
    ///   await NavigationHelper.GoToAsync("clientdetails?clientId=123");
    ///   
    ///   // With parameters dictionary
    ///   var parameters = new Dictionary&lt;string, object&gt; { { "clientId", 123 } };
    ///   await NavigationHelper.GoToAsync("clientdetails", parameters);
    /// 
    /// This helper automatically:
    ///   - Prevents duplicate navigations within 500ms (debouncing)
    ///   - Uses a semaphore lock to prevent concurrent navigations
    ///   - Tracks navigation state for restoration
    /// </summary>
    public static class NavigationHelper
    {
        private static bool _isNavigating = false;
        private static DateTime _lastNavigationTime = DateTime.MinValue;
        private static string _lastNavigationRoute = string.Empty;
        private static readonly SemaphoreSlim _navigationSemaphore = new SemaphoreSlim(1, 1);
        private const int NavigationDebounceMs = 500; // Prevent navigation within 500ms of previous navigation
        /// <summary>
        /// Maps Shell routes to ActivityType names (reverse of ActivityStateRestorationService).
        /// </summary>
        private static readonly Dictionary<string, string> RouteToActivityTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "///MainPage", "MainActivity" },
            { "login", "LoginActivity" },
            { "loginconfig", "LoginConfigActivity" },
            { "newlogin", "NewLoginActivity" },
            { "bottlelogin", "BottleLoginActivity" },
            { "termsandconditions", "TermsAndConditionsActivity" },
            { "orderdetails", "OrderDetailsActivity" },
            { "ordercredit", "OrderCreditActivity" },
            { "superordertemplate", "SuperOrderTemplateActivity" },
            { "previouslyorderedtemplate", "PreviouslyOrderedTemplateActivity" },
            { "batch", "BatchActivity" },
            { "finalizebatch", "FinalizeBatchActivity" },
            { "ordersignature", "OrderSignatureActivity" },
            { "batchdepartment", "BatchDepartmentActivity" },
            { "workorder", "WorkOrderActivity" },
            { "consignment", "ConsignmentActivity" },
            { "createdeposit", "CreateDepositActivity" },
            { "noservice", "NoServiceActivity" },
            { "clientdetails", "ClientDetailsActivity" },
            { "editclient", "EditClientActivity" },
            { "addclient", "AddClientActivity" },
            { "selectinvoice", "SelectInvoiceActivity" },
            { "invoicedetails", "InvoiceDetailsActivity" },
            { "paymentselectclient", "PaymentSelectClientActivity" },
            { "paymentsetvalues", "PaymentSetValuesActivity" },
            { "advancedcatalog", "AdvancedCatalogActivity" },
            { "productcatalog", "ProductCatalogActivity" },
            { "fullcategory", "FullCategoryActivity" },
            { "productdetails", "ProductDetailsActivity" },
            { "productimage", "ProductImageActivity" },
            { "additem", "AddItemActivity" },
            { "inventory", "InventoryMainActivity" },
            { "viewprintinventory", "ViewPrintInventoryActivity" },
            { "checkinventory", "CheckInventoryActivity" },
            { "transferonoff", "TransferOnOffActivity" },
            { "setparlevel", "SetParLevelActivity" },
            { "cyclecount", "CycleCountActivity" },
            { "inventorysummary", "InventorySummaryActivity" },
            { "endinventory", "EndInventoryActivity" },
            { "acceptload", "AcceptLoadActivity" },
            { "acceptloadeditdelivery", "AcceptLoadEditDeliveryActivity" },
            { "viewloadorder", "ViewLoadOrderActivity" },
            { "newloadordertemplate", "NewLoadOrderTemplateActivity" },
            { "routereturns", "RouteReturnsActivity" },
            { "routemanagement", "RouteManagementActivity" },
            { "routeexpenses", "RouteExpensesActivity" },
            { "routemap", "RouteMapActivity" },
            { "addorderstoroute", "AddOrdersToRouteActivity" },
            { "addpostoroute", "AddPOSToRouteActivity" },
            { "endofday", "EndOfDayActivity" },
            { "endofdayprocess", "EndOfDayProcessActivity" },
            { "reports", "ReportsActivity" },
            { "salesreport", "SalesReportActivity" },
            { "paymentsreport", "PaymentsReportActivity" },
            { "commissionreport", "CommissionReportActivity" },
            { "salesmencommissionreport", "SalesmenCommissionReportActivity" },
            { "qtyprodsalesreport", "QtyProdSalesReportActivity" },
            { "salesproductcatreport", "SalesProductCatReportActivity" },
            { "transmissionreport", "TransmissionReportActivity" },
            { "loadorderreport", "LoadOrderReportActivity" },
            { "saporderreport", "SAPOrderReportActivity" },
            { "printreports", "PrintReportsActivity" },
            { "configuration", "ConfigurationActivity" },
            { "timesheet", "TimeSheetActivity" },
            { "sentorders", "SentOrdersActivity" },
            { "sentordersorderslist", "SentOrdersOrdersListActivity" },
            { "sentpayments", "SentPaymentsActivity" },
            { "sentpaymentsinpackage", "SentPaymentsInPackageActivity" },
            { "vieworderstatus", "ViewOrderStatusActivity" },
            { "vieworderstatusdetails", "ViewOrderStatusDetailsActivity" },
            { "viewcapturedimages", "ViewCapturedImagesActivity" },
            { "goals", "GoalsActivity" },
            { "goalfilter", "GoalFilterActivity" },
            { "goaldetails", "GoalDetailsActivity" },
            { "selectpricelevel", "SelectPriceLevelActivity" },
            { "selectretailpricelevel", "SelectRetailPriceLevelActivity" },
            { "selectterms", "SelectTermsActivity" },
            { "setupprinter", "SetupPrinterActivity" },
            { "setupscanner", "SetupScannerActivity" },
            { "addclientbillto", "AddClientBillToActivity" },
            { "logviewer", "LogViewerActivity" },
            { "clientimages", "ClientImagesActivity" },
            { "managedepartments", "ManageDepartmentsActivity" },
        };

        /// <summary>
        /// Navigates to a route and saves ActivityState.
        /// Prevents double-tap navigation by debouncing and using a semaphore lock.
        /// </summary>
        public static async Task GoToAsync(string route, bool saveState = true)
        {
            // Build route string from route
            var routeString = route;
            await GoToAsyncInternal(routeString, null, saveState);
        }

        /// <summary>
        /// Navigates to a route with parameters and saves ActivityState.
        /// Prevents double-tap navigation by debouncing and using a semaphore lock.
        /// </summary>
        public static async Task GoToAsync(string route, IDictionary<string, object> parameters, bool saveState = true)
        {
            // Build route string with query parameters
            var routeString = route;
            if (parameters != null && parameters.Count > 0)
            {
                var queryParams = string.Join("&", parameters.Select(kvp => 
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value?.ToString() ?? string.Empty)}"));
                routeString = $"{route}?{queryParams}";
            }
            
            await GoToAsyncInternal(routeString, parameters, saveState);
        }

        /// <summary>
        /// Internal method that handles the actual navigation with debouncing.
        /// </summary>
        private static async Task GoToAsyncInternal(string routeString, IDictionary<string, object> parameters, bool saveState)
        {
            // Normalize route for comparison (remove query parameters for duplicate check)
            var routeForComparison = routeString.Split('?')[0].TrimStart('/');
            if (routeForComparison.StartsWith("//"))
            {
                routeForComparison = routeForComparison.TrimStart('/');
            }

            // Check if we're already navigating or if this is a duplicate navigation within debounce window
            var timeSinceLastNavigation = (DateTime.Now - _lastNavigationTime).TotalMilliseconds;
            if (_isNavigating || 
                (timeSinceLastNavigation < NavigationDebounceMs && _lastNavigationRoute == routeForComparison))
            {
                // Skip duplicate navigation
                return;
            }

            // Try to acquire the semaphore (non-blocking check first)
            if (!await _navigationSemaphore.WaitAsync(0))
            {
                // Another navigation is in progress, skip this one
                return;
            }

            try
            {
                _isNavigating = true;
                _lastNavigationTime = DateTime.Now;
                _lastNavigationRoute = routeForComparison;

                if (saveState)
                {
                    SaveNavigationState(routeString);
                }

                if (parameters != null)
                {
                    await Shell.Current.GoToAsync(routeString, parameters);
                }
                else
                {
                    await Shell.Current.GoToAsync(routeString);
                }
            }
            finally
            {
                // Release the semaphore after a short delay to prevent rapid successive navigations
                await Task.Delay(NavigationDebounceMs);
                _isNavigating = false;
                _navigationSemaphore.Release();
            }
        }

        /// <summary>
        /// Saves the current navigation state to ActivityState.
        /// </summary>
        public static void SaveNavigationState(string route)
        {
            if (string.IsNullOrEmpty(route))
                return;

            // Extract base route and query parameters
            var routeParts = route.Split('?');
            var baseRoute = routeParts[0].TrimStart('/');
            
            // Skip navigation prefixes like "///" for MainPage
            if (baseRoute.StartsWith("//"))
            {
                baseRoute = baseRoute.TrimStart('/');
            }

            // Get ActivityType from route
            if (!RouteToActivityTypeMap.TryGetValue(baseRoute, out var activityType))
            {
                // Skip unmappable routes like "Splash"
                if (baseRoute.Equals("Splash", StringComparison.OrdinalIgnoreCase) ||
                    baseRoute.Equals("splash", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationHelper] Skipping unmappable route: {baseRoute}");
                    return;
                }
                
                // Try to find by route name directly
                activityType = baseRoute;
            }

            // Parse query parameters
            var state = new ActivityState
            {
                ActivityType = activityType,
                State = new SerializableDictionary<string, string>()
            };

            if (routeParts.Length > 1)
            {
                var queryParams = routeParts[1];
                foreach (var param in queryParams.Split('&'))
                {
                    var parts = param.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = Uri.UnescapeDataString(parts[0]);
                        var value = Uri.UnescapeDataString(parts[1]);
                        state.State[key] = value;
                    }
                }
            }

            // Check if this activity type already exists in the stack
            var existingState = ActivityState.GetState(activityType);
            if (existingState != null)
            {
                // If it's the last state in the stack, update it (same page, different params)
                // But only if the new state has query parameters or the existing one doesn't
                // This prevents NavigationTracker from overwriting explicit saves with query params
                var lastState = ActivityState.States.LastOrDefault();
                if (existingState == lastState)
                {
                    // If existing state has query parameters and new one doesn't, don't overwrite
                    // (This means the page explicitly saved state with params, don't overwrite with NavigationTracker's version)
                    if (existingState.State != null && existingState.State.Count > 0 && state.State.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NavigationHelper] Skipping update for {activityType} - existing state has params, new one doesn't");
                        return;
                    }
                    
                    // Update existing state (same page, different parameters)
                    existingState.State = state.State;
                    ActivityState.Save();
                    System.Diagnostics.Debug.WriteLine($"[NavigationHelper] Updated state for {activityType} with route {route}");
                }
                else
                {
                    // Navigation forward - remove all states after this one, then add new
                    var existingIndex = ActivityState.States.IndexOf(existingState);
                    if (existingIndex >= 0)
                    {
                        // Remove all states after the existing one
                        for (int i = ActivityState.States.Count - 1; i > existingIndex; i--)
                        {
                            ActivityState.States.RemoveAt(i);
                        }
                    }
                    // Add new state
                    ActivityState.AddState(state);
                    System.Diagnostics.Debug.WriteLine($"[NavigationHelper] Added new state for {activityType} with route {route} (removed {ActivityState.States.Count - existingIndex - 1} states after)");
                }
            }
            else
            {
                // Add new state to stack (navigation forward)
                ActivityState.AddState(state);
                System.Diagnostics.Debug.WriteLine($"[NavigationHelper] Added new state for {activityType} with route {route} (stack now has {ActivityState.States.Count} states)");
            }
        }

        /// <summary>
        /// Removes the ActivityState for a given route when navigating away.
        /// </summary>
        public static void RemoveNavigationState(string route)
        {
            if (string.IsNullOrEmpty(route))
                return;

            var routeParts = route.Split('?');
            var baseRoute = routeParts[0].TrimStart('/');
            
            if (baseRoute.StartsWith("//"))
            {
                baseRoute = baseRoute.TrimStart('/');
            }

            // If route is a full path (e.g. "//MainPage/Orders/ordercredit/productcatalog"), use the last segment for lookup
            // so we correctly remove the current page's state (e.g. ProductCatalog when going back from productcatalog)
            if (baseRoute.Contains("/"))
            {
                var segments = baseRoute.Split('/');
                baseRoute = segments[segments.Length - 1];
            }

            if (RouteToActivityTypeMap.TryGetValue(baseRoute, out var activityType))
            {
                var state = ActivityState.GetState(activityType);
                if (state != null)
                {
                    ActivityState.RemoveState(state);
                    System.Diagnostics.Debug.WriteLine($"[NavigationHelper] RemoveNavigationState: removed {baseRoute} (ActivityType={activityType}), stack count now {ActivityState.States?.Count ?? 0}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationHelper] RemoveNavigationState: {baseRoute} (ActivityType={activityType}) had no state in stack, nothing removed");
                }
            }
        }

        /// <summary>
        /// Removes the given route's ActivityState and pops one level (GoToAsync("..")).
        /// Use this from ViewModels or any "exit this screen" logic so back and programmatic exit both update state consistently.
        /// </summary>
        public static async Task GoBackFromAsync(string routeName)
        {
            RemoveNavigationState(routeName ?? string.Empty);
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Removes state for the given routes (in order) and pops once per route.
        /// Use when leaving a page that was pushed on top of another (e.g. ProductCatalog on FullCategory: pass "productcatalog", "fullcategory").
        /// </summary>
        public static async Task GoBackFromAsync(string routeToRemoveFirst, string routeToRemoveSecond)
        {
            RemoveNavigationState(routeToRemoveFirst);
            await Shell.Current.GoToAsync("..");
            RemoveNavigationState(routeToRemoveSecond);
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Clears all navigation state (e.g., on logout).
        /// </summary>
        public static void ClearAllNavigationState()
        {
            ActivityState.RemoveAll();
        }

        /// <summary>
        /// Logs all current navigation states for diagnostics.
        /// Useful for debugging state restoration issues.
        /// </summary>
        public static void LogAllNavigationStates()
        {
            if (ActivityState.States == null || ActivityState.States.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine("[NavigationHelper] NO NAVIGATION STATES FOUND");
                System.Diagnostics.Debug.WriteLine("========================================");
                Logger.CreateLog("[NavigationHelper] NO NAVIGATION STATES FOUND");
                return;
            }

            System.Diagnostics.Debug.WriteLine("========================================");
            System.Diagnostics.Debug.WriteLine($"[NavigationHelper] CURRENT NAVIGATION STATES ({ActivityState.States.Count} total)");
            System.Diagnostics.Debug.WriteLine("========================================");

            var logBuilder = new System.Text.StringBuilder();
            logBuilder.AppendLine("========================================");
            logBuilder.AppendLine($"[NavigationHelper] CURRENT NAVIGATION STATES ({ActivityState.States.Count} total)");
            logBuilder.AppendLine("========================================");

            for (int i = 0; i < ActivityState.States.Count; i++)
            {
                var state = ActivityState.States[i];
                var isLast = i == ActivityState.States.Count - 1;
                var marker = isLast ? " <-- DEEPEST (will restore to this)" : "";

                System.Diagnostics.Debug.WriteLine($"[{i}] ActivityType: {state.ActivityType}{marker}");

                // Try to get the route from ActivityType
                var route = GetRouteFromActivityType(state.ActivityType);
                if (!string.IsNullOrEmpty(route))
                {
                    System.Diagnostics.Debug.WriteLine($"     Route: {route}");
                }

                if (state.State != null && state.State.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"     Query Parameters ({state.State.Count}):");
                    foreach (var kvp in state.State)
                    {
                        System.Diagnostics.Debug.WriteLine($"       - {kvp.Key} = {kvp.Value}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"     Query Parameters: None");
                }

                // Build log string
                logBuilder.AppendLine($"[{i}] ActivityType: {state.ActivityType}{marker}");
                if (!string.IsNullOrEmpty(route))
                {
                    logBuilder.AppendLine($"     Route: {route}");
                }
                if (state.State != null && state.State.Count > 0)
                {
                    logBuilder.AppendLine($"     Query Parameters ({state.State.Count}):");
                    foreach (var kvp in state.State)
                    {
                        logBuilder.AppendLine($"       - {kvp.Key} = {kvp.Value}");
                    }
                }
                else
                {
                    logBuilder.AppendLine($"     Query Parameters: None");
                }
                logBuilder.AppendLine();
            }

            System.Diagnostics.Debug.WriteLine("========================================");

            // Also log to Logger for file logging
            Logger.CreateLog(logBuilder.ToString());
        }

        /// <summary>
        /// Gets the route from an ActivityType (reverse lookup).
        /// </summary>
        private static string GetRouteFromActivityType(string activityType)
        {
            if (string.IsNullOrEmpty(activityType))
                return null;

            // Reverse lookup in RouteToActivityTypeMap
            var route = RouteToActivityTypeMap.FirstOrDefault(kvp => 
                kvp.Value.Equals(activityType, StringComparison.OrdinalIgnoreCase)).Key;

            if (!string.IsNullOrEmpty(route))
            {
                // Build full route with query parameters if available
                var state = ActivityState.GetState(activityType);
                if (state?.State != null && state.State.Count > 0)
                {
                    var queryParams = string.Join("&", state.State.Select(kvp => 
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                    return $"{route}?{queryParams}";
                }
                return route;
            }

            // If not found in map, return the activity type as-is (might be a direct route)
            return activityType;
        }
    }
}

