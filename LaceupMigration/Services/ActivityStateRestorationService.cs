using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.Services
{
    /// <summary>
    /// Service to restore navigation state from ActivityState.
    /// Handles mapping ActivityType to Shell routes and restoring navigation stack.
    /// </summary>
    public class ActivityStateRestorationService : IActivityStateRestorationService
    {
        /// <summary>
        /// Maps Xamarin ActivityType names to MAUI Shell routes.
        /// ActivityType was typically the class name (e.g., "OrderDetailsActivity" -> "orderdetails")
        /// </summary>
        private static readonly Dictionary<string, string> ActivityTypeToRouteMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Main pages
            { "MainActivity", "///MainPage" },
            { "MainPage", "///MainPage" },
            
            // Login pages
            { "LoginActivity", "login" },
            { "LoginConfigActivity", "loginconfig" },
            { "NewLoginActivity", "newlogin" },
            { "BottleLoginActivity", "bottlelogin" },
            { "TermsAndConditionsActivity", "termsandconditions" },
            
            // Order pages
            { "OrderDetailsActivity", "orderdetails" },
            { "OrderCreditActivity", "ordercredit" },
            { "SuperOrderTemplateActivity", "superordertemplate" },
            { "PreviouslyOrderedTemplateActivity", "previouslyorderedtemplate" },
            { "BatchActivity", "batch" },
            { "FinalizeBatchActivity", "finalizebatch" },
            { "OrderSignatureActivity", "ordersignature" },
            { "BatchDepartmentActivity", "batchdepartment" },
            { "WorkOrderActivity", "workorder" },
            { "ConsignmentActivity", "consignment" },
            { "NoServiceActivity", "noservice" },
            
            // Client pages
            { "ClientDetailsActivity", "clientdetails" },
            { "EditClientActivity", "editclient" },
            { "AddClientActivity", "addclient" },
            { "SelectInvoiceActivity", "selectinvoice" },
            { "InvoiceDetailsActivity", "invoicedetails" },
            { "PaymentSelectClientActivity", "paymentselectclient" },
            { "PaymentSetValuesActivity", "paymentsetvalues" },
            { "CreateDepositActivity", "createdeposit" },
            
            // Catalog pages
            { "AdvancedCatalogActivity", "advancedcatalog" },
            { "ProductCatalogActivity", "productcatalog" },
            { "FullCategoryActivity", "fullcategory" },
            { "ProductDetailsActivity", "productdetails" },
            { "ProductImageActivity", "productimage" },
            { "AddItemActivity", "additem" },
            
            // Inventory pages
            { "InventoryMainActivity", "inventory" },
            { "ViewPrintInventoryActivity", "viewprintinventory" },
            { "CheckInventoryActivity", "checkinventory" },
            { "TransferOnOffActivity", "transferonoff" },
            { "SetParLevelActivity", "setparlevel" },
            { "CycleCountActivity", "cyclecount" },
            { "InventorySummaryActivity", "inventorysummary" },
            { "EndInventoryActivity", "endinventory" },
            
            // Load order pages
            { "AcceptLoadActivity", "acceptload" },
            { "AcceptLoadEditDeliveryActivity", "acceptloadeditdelivery" },
            { "ViewLoadOrderActivity", "viewloadorder" },
            { "NewLoadOrderTemplateActivity", "newloadordertemplate" },
            
            // Route pages
            { "RouteReturnsActivity", "routereturns" },
            { "RouteManagementActivity", "routemanagement" },
            { "RouteExpensesActivity", "routeexpenses" },
            { "RouteMapActivity", "routemap" },
            { "AddOrdersToRouteActivity", "addorderstoroute" },
            { "AddPOSToRouteActivity", "addpostoroute" },
            
            // End of day pages
            { "EndOfDayActivity", "endofday" },
            { "EndOfDayProcessActivity", "endofdayprocess" },
            
            // Reports pages
            { "ReportsActivity", "reports" },
            { "SalesReportActivity", "salesreport" },
            { "PaymentsReportActivity", "paymentsreport" },
            { "CommissionReportActivity", "commissionreport" },
            { "SalesmenCommissionReportActivity", "salesmencommissionreport" },
            { "QtyProdSalesReportActivity", "qtyprodsalesreport" },
            { "SalesProductCatReportActivity", "salesproductcatreport" },
            { "TransmissionReportActivity", "transmissionreport" },
            { "LoadOrderReportActivity", "loadorderreport" },
            { "SAPOrderReportActivity", "saporderreport" },
            { "PrintReportsActivity", "printreports" },
            
            // Configuration
            { "ConfigurationActivity", "configuration" },
            
            // Other pages
            { "TimeSheetActivity", "timesheet" },
            { "SentOrdersActivity", "sentorders" },
            { "SentOrdersOrdersListActivity", "sentordersorderslist" },
            { "SentPaymentsActivity", "sentpayments" },
            { "SentPaymentsInPackageActivity", "sentpaymentsinpackage" },
            { "ViewOrderStatusActivity", "vieworderstatus" },
            { "ViewOrderStatusDetailsActivity", "vieworderstatusdetails" },
            { "GoalsActivity", "goals" },
            { "GoalDetailsActivity", "goaldetails" },
            { "SelectPriceLevelActivity", "selectpricelevel" },
            { "SelectRetailPriceLevelActivity", "selectretailpricelevel" },
            { "SelectTermsActivity", "selectterms" },
            { "SetupPrinterActivity", "setupprinter" },
            { "SetupScannerActivity", "setupscanner" },
            { "AddClientBillToActivity", "addclientbillto" },
            { "LogViewerActivity", "logviewer" },
            { "ClientImagesActivity", "clientimages" },
            { "ManageDepartmentsActivity", "managedepartments" },
            
            // Self Service
            { "SelfServiceSelectCompanyActivity", "selfservice/selectcompany" },
            { "SelfServiceClientListActivity", "selfservice/clientlist" },
            { "SelfServiceCheckOutActivity", "selfservice/checkout" },
            { "SelfServiceTemplateActivity", "selfservice/template" },
            { "SelfServiceCatalogActivity", "selfservice/catalog" },
            { "SelfServiceCategoriesActivity", "selfservice/categories" },
            { "SelfServiceCollectPaymentActivity", "selfservice/collectpayment" },
            { "SelfServiceCreditTemplateActivity", "selfservice/credittemplate" },
        };

        /// <summary>
        /// List of protected screens that cannot be left without proper completion.
        /// These screens should be restored if the app was force-quit.
        /// </summary>
        private static readonly HashSet<string> ProtectedScreens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "routereturns",
            "endinventory",
            "advancedcatalog",
            "acceptload"
        };

        /// <summary>
        /// Gets the Shell route for a given ActivityType.
        /// </summary>
        public string GetRouteForActivityType(string activityType)
        {
            if (string.IsNullOrEmpty(activityType))
                return null;

            // Try exact match first
            if (ActivityTypeToRouteMap.TryGetValue(activityType, out var route))
                return route;

            // Try removing "Activity" suffix if present
            var withoutSuffix = activityType.EndsWith("Activity", StringComparison.OrdinalIgnoreCase)
                ? activityType.Substring(0, activityType.Length - 8)
                : activityType;

            if (ActivityTypeToRouteMap.TryGetValue(withoutSuffix + "Activity", out route))
                return route;

            // Try direct match without "Activity"
            if (ActivityTypeToRouteMap.TryGetValue(withoutSuffix, out route))
                return route;

            // Try converting to lowercase route format (e.g., "OrderDetailsActivity" -> "orderdetails")
            var routeFormat = ConvertToRouteFormat(withoutSuffix);
            if (ActivityTypeToRouteMap.ContainsValue(routeFormat))
                return routeFormat;

            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] No route mapping found for ActivityType: {activityType}");
            return null;
        }

        /// <summary>
        /// Converts a PascalCase string to a lowercase route format.
        /// Example: "OrderDetails" -> "orderdetails"
        /// </summary>
        private string ConvertToRouteFormat(string pascalCase)
        {
            if (string.IsNullOrEmpty(pascalCase))
                return null;

            var result = new System.Text.StringBuilder();
            foreach (var c in pascalCase)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(char.ToLowerInvariant(c));
                else
                    result.Append(char.ToLowerInvariant(c));
            }
            return result.ToString();
        }

        /// <summary>
        /// Builds a navigation route with query parameters from ActivityState.
        /// </summary>
        public string BuildNavigationRoute(ActivityState state)
        {
            if (state == null)
                return null;

            var route = GetRouteForActivityType(state.ActivityType);
            if (string.IsNullOrEmpty(route))
                return null;

            // If route already has query parameters (e.g., "///MainPage"), don't add more
            if (route.Contains("?") || route.StartsWith("///"))
                return route;

            // Build query string from State dictionary
            if (state.State != null && state.State.Count > 0)
            {
                var queryParams = state.State
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")
                    .ToArray();

                if (queryParams.Length > 0)
                {
                    route += "?" + string.Join("&", queryParams);
                }
            }

            return route;
        }

        /// <summary>
        /// Checks if a route represents a protected screen.
        /// </summary>
        public bool IsProtectedScreen(string route)
        {
            if (string.IsNullOrEmpty(route))
                return false;

            // Extract base route (remove query parameters and navigation prefixes)
            var baseRoute = route.Split('?')[0].TrimStart('/');
            
            return ProtectedScreens.Contains(baseRoute);
        }

        /// <summary>
        /// Restores navigation from ActivityState.States.
        /// Returns the route to navigate to (the deepest route in the stack), or null if no restoration is needed.
        /// Note: Shell will maintain the navigation stack when navigating to the deepest route.
        /// </summary>
        public string GetRestorationRoute()
        {
            if (ActivityState.States == null || ActivityState.States.Count == 0)
                return null;

            // Get the last (most recent/deepest) state in the stack
            var lastState = ActivityState.States.LastOrDefault();
            if (lastState == null)
                return null;

            var route = BuildNavigationRoute(lastState);
            if (string.IsNullOrEmpty(route))
                return null;

            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Restoring to {lastState.ActivityType} -> {route} (stack depth: {ActivityState.States.Count})");
            return route;
        }

        /// <summary>
        /// Gets the full navigation stack for restoration.
        /// Returns a list of routes representing the navigation path.
        /// </summary>
        public List<string> GetRestorationStack()
        {
            if (ActivityState.States == null || ActivityState.States.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[ActivityStateRestoration] No states found in stack");
                return new List<string>();
            }

            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Found {ActivityState.States.Count} states in stack");
            var stack = new List<string>();
            foreach (var state in ActivityState.States)
            {
                // Skip Splash and other unmappable states
                if (string.IsNullOrEmpty(state.ActivityType) || 
                    state.ActivityType.Equals("Splash", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Skipping unmappable state: {state.ActivityType}");
                    continue;
                }
                
                var route = BuildNavigationRoute(state);
                if (!string.IsNullOrEmpty(route))
                {
                    stack.Add(route);
                    System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Stack item - {state.ActivityType} -> {route}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Could not build route for {state.ActivityType}");
                }
            }

            return stack;
        }

        /// <summary>
        /// Validates that a restored route is still accessible.
        /// For example, checks if an order still exists before restoring to OrderDetails.
        /// </summary>
        public async Task<bool> ValidateRestorationRoute(string route)
        {
            if (string.IsNullOrEmpty(route))
                return false;

            // Extract route and parameters
            var routeParts = route.Split('?');
            var baseRoute = routeParts[0].TrimStart('/');
            var queryParams = routeParts.Length > 1 ? routeParts[1] : null;

            // Parse query parameters
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(queryParams))
            {
                foreach (var param in queryParams.Split('&'))
                {
                    var parts = param.Split('=');
                    if (parts.Length == 2)
                    {
                        parameters[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
                    }
                }
            }

            // Validate specific routes that require data validation
            switch (baseRoute.ToLowerInvariant())
            {
                case "orderdetails":
                case "ordercredit":
                case "consignment":
                case "noservice":
                    // Check if order still exists
                    if (parameters.TryGetValue("orderId", out var orderIdStr) && int.TryParse(orderIdStr, out var orderId))
                    {
                        var order = Order.Orders.FirstOrDefault(o => o.OrderId == orderId);
                        if (order == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Order {orderId} no longer exists, skipping restoration");
                            return false;
                        }
                    }
                    break;

                case "batch":
                    // Check if batch still exists
                    if (parameters.TryGetValue("batchId", out var batchIdStr) && int.TryParse(batchIdStr, out var batchId))
                    {
                        var batch = Batch.List.FirstOrDefault(b => b.Id == batchId);
                        if (batch == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Batch {batchId} no longer exists, skipping restoration");
                            return false;
                        }
                    }
                    break;

                case "clientdetails":
                    // Check if client still exists
                    if (parameters.TryGetValue("clientId", out var clientIdStr) && int.TryParse(clientIdStr, out var clientId))
                    {
                        var client = Client.Clients.FirstOrDefault(c => c.ClientId == clientId);
                        if (client == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] Client {clientId} no longer exists, skipping restoration");
                            return false;
                        }
                    }
                    break;

                case "finalizebatch":
                    // Check if client still exists and ordersId is present
                    if (parameters.TryGetValue("clientId", out var fbClientIdStr) && int.TryParse(fbClientIdStr, out var fbClientId))
                    {
                        var fbClient = Client.Clients.FirstOrDefault(c => c.ClientId == fbClientId);
                        if (fbClient == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ActivityStateRestoration] FinalizeBatch client {fbClientId} no longer exists, skipping restoration");
                            return false;
                        }
                    }
                    if (!parameters.ContainsKey("ordersId") || string.IsNullOrEmpty(parameters["ordersId"]))
                    {
                        System.Diagnostics.Debug.WriteLine("[ActivityStateRestoration] FinalizeBatch has no ordersId, skipping restoration");
                        return false;
                    }
                    break;
            }

            return true;
        }
    }

    /// <summary>
    /// Interface for ActivityState restoration service.
    /// </summary>
    public interface IActivityStateRestorationService
    {
        string GetRouteForActivityType(string activityType);
        string BuildNavigationRoute(ActivityState state);
        bool IsProtectedScreen(string route);
        string GetRestorationRoute();
        List<string> GetRestorationStack();
        Task<bool> ValidateRestorationRoute(string route);
    }
}

