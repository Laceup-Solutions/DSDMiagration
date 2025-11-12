using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LaceupMigration
{
    public sealed class ActivityNames
    {
        public const string OrderDetailsActivity = "OrderDetailsActivity";
        public const string AddItemActivity = "AddItemActivity";
        public const string ProductListActivity = "ProductListActivity";
        public const string OrderCreditActivity = "OrderCreditActivity";
    }

    public class ActivityProvider
    {
        private List<ActivityInfo> viewRepository = new List<ActivityInfo>();

        private const string CurrentIdiom = "Phone";
        private const string OrderDetailsActivityPhone = "orderdetailsactivityphone";
        private const string OrderDetailsActivityPad = "orderdetailsactivitypad";
        private const string AddItemActivityPhone = "additemactivityphone";
        private const string AddItemActivityPad = "additemactivitypad";
        private const string ProductListActivityPhone = "productlistactivityphone";
        private const string ProductListActivityPad = "productlistactivitypad";
        private const string OrderCreditActivityPhone = "ordercreditactivityphone";
        private const string OrderCreditActivityPad = "ordercreditactivitypad";

        public void AddCustomActivitys(string configSetting)
        {
            if (string.IsNullOrEmpty(configSetting))
                return;
            string[] parts = configSetting.Split(new char[] { ',' });

            for (int index = 0; index < parts.Length / 2; index++)
            {
                string key = parts[index * 2];
                string value = parts[index * 2 + 1];

                switch (key.ToLowerInvariant())
                {
                    case ProductListActivityPhone:
                        AddCustomActivity(ActivityNames.ProductListActivity, "Phone", value);
                        break;
                    case ProductListActivityPad:
                        AddCustomActivity(ActivityNames.ProductListActivity, "Pad", value);
                        break;

                    case OrderDetailsActivityPhone:
                        AddCustomActivity(ActivityNames.OrderDetailsActivity, "Phone", value);
                        break;
                    case OrderDetailsActivityPad:
                        AddCustomActivity(ActivityNames.OrderDetailsActivity, "Pad", value);
                        break;

                    case AddItemActivityPhone:
                        AddCustomActivity(ActivityNames.AddItemActivity, "Phone", value);
                        break;
                    case AddItemActivityPad:
                        AddCustomActivity(ActivityNames.AddItemActivity, "Pad", value);
                        break;

                    case OrderCreditActivityPhone:
                        AddCustomActivity(ActivityNames.OrderCreditActivity, "Phone", value);
                        break;
                    case OrderCreditActivityPad:
                        AddCustomActivity(ActivityNames.OrderCreditActivity, "Pad", value);
                        break;
                }
            }
        }

        void AddCustomActivity(string name, string idiom, string type)
        {
            ActivityInfo vi = FindActivity(name, idiom);
            if (vi == null)
            {
                vi = new ActivityInfo()
                {
                    Name = name,
                    Controller = type,
                    Idiom = idiom
                };
                viewRepository.Add(vi);
            }
            else
            {
                try
                {
                    var t = Type.GetType(type);

                    if (t != null)
                    {
                        vi.Name = name;
                        vi.Idiom = idiom;
                        vi.Controller = type;
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }
        }

        public void DeSerializeFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            string[] parts = value.Split(new char[] { ',' });
            for (int index = 0; index < parts.Length / 3; index++)
            {
                string name = parts[index * 3];
                string idiom = parts[index * 3 + 1];
                string controller = parts[index * 3 + 2];

                AddCustomActivity(name, idiom, controller);
            }
        }

        public string SerializeToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (ActivityInfo vi in viewRepository)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(vi.ToString());
            }

            return sb.ToString();
        }

        ActivityInfo FindActivity(string viewName, string idiom)
        {
            ActivityInfo selectedActivity = null;
            foreach (ActivityInfo vi in viewRepository)
            {
                if (string.Compare(vi.Name, viewName) == 0)
                {
                    selectedActivity = vi;
                    if (idiom != null && string.Compare(vi.Idiom, idiom) == 0)
                        break;
                }
            }
            return selectedActivity;
        }

        ActivityInfo FindActivity(string viewName)
        {
            return FindActivity(viewName, null);
        }

        /// <summary>
        /// Creates an instance of the activity type (for MAUI, returns the Type, not an instance)
        /// </summary>
        public Type CreateActivity(string viewName)
        {
            ActivityInfo selectedActivity = FindActivity(viewName, CurrentIdiom);
            if (selectedActivity == null)
            {
                // Fallback to default if not found
                LoadActivitys();
                selectedActivity = FindActivity(viewName, CurrentIdiom);
            }

            // Override OrderDetailsActivity to PreviouslyOrderedTemplateActivity (which maps to OrderDetailsPage in MAUI)
            if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.OrderDetailsActivity")
            {
                selectedActivity.Controller = "LaceupAndroidApp.PreviouslyOrderedTemplateActivity";
            }

            Type type = Type.GetType(selectedActivity?.Controller);
            if (type == null)
            {
                // Check if it's FullTemplateActivity - map to SuperOrderTemplatePage in MAUI
                if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.FullTemplateActivity")
                {
                    return GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
                
                // Fallback to default MAUI page types
                type = GetDefaultType(viewName);
            }
            else
            {
                // If the type is FullTemplateActivity, map to SuperOrderTemplatePage
                if (type.FullName == "LaceupAndroidApp.FullTemplateActivity")
                {
                    return GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
            }

            return type;
        }

        /// <summary>
        /// Creates an activity type with client-specific logic
        /// </summary>
        public Type CreateActivity(string viewName, Client client)
        {
            ActivityInfo selectedActivity = FindActivity(viewName, CurrentIdiom);
            if (selectedActivity == null)
            {
                LoadActivitys();
                selectedActivity = FindActivity(viewName, CurrentIdiom);
            }

            // Override OrderDetailsActivity to PreviouslyOrderedTemplateActivity
            if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.OrderDetailsActivity")
            {
                selectedActivity.Controller = "LaceupAndroidApp.PreviouslyOrderedTemplateActivity";
            }

            Type type = Type.GetType(selectedActivity?.Controller);
            if (type == null)
            {
                // Check if it's FullTemplateActivity - map to SuperOrderTemplatePage in MAUI
                if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.FullTemplateActivity")
                {
                    type = GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
                else
                {
                    type = GetDefaultType(viewName);
                }
            }
            else
            {
                // If the type is FullTemplateActivity, map to SuperOrderTemplatePage
                if (type.FullName == "LaceupAndroidApp.FullTemplateActivity")
                {
                    type = GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
            }

            // Client-specific override for OrderDetailsActivity
            if (Config.UseFullTemplate && viewName == ActivityNames.OrderDetailsActivity && client != null)
            {
                var useFullTemplate = DataAccess.GetSingleUDF("fullTemplate", client.NonvisibleExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(useFullTemplate) && useFullTemplate == "0")
                {
                    // Use PreviouslyOrderedTemplateActivity (OrderDetailsPage) instead of FullTemplateActivity
                    type = GetDefaultType(ActivityNames.OrderDetailsActivity);
                }
            }

            return type;
        }

        /// <summary>
        /// Gets the activity type without creating an instance
        /// </summary>
        public Type GetActivityType(string viewName)
        {
            ActivityInfo selectedActivity = FindActivity(viewName, CurrentIdiom);
            if (selectedActivity == null)
            {
                LoadActivitys();
                selectedActivity = FindActivity(viewName, CurrentIdiom);
            }

            // Override OrderDetailsActivity to PreviouslyOrderedTemplateActivity
            if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.OrderDetailsActivity")
            {
                selectedActivity.Controller = "LaceupAndroidApp.PreviouslyOrderedTemplateActivity";
            }

            Type type = Type.GetType(selectedActivity?.Controller);
            if (type == null)
            {
                // Check if it's FullTemplateActivity - map to SuperOrderTemplatePage in MAUI
                if (selectedActivity != null && selectedActivity.Controller == "LaceupAndroidApp.FullTemplateActivity")
                {
                    return GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
                
                type = GetDefaultType(viewName);
            }
            else
            {
                // If the type is FullTemplateActivity, map to SuperOrderTemplatePage
                if (type.FullName == "LaceupAndroidApp.FullTemplateActivity")
                {
                    return GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
                }
            }

            return type;
        }

        /// <summary>
        /// Gets the default MAUI page type for an activity name using string-based resolution
        /// </summary>
        private Type GetDefaultType(string viewName)
        {
            // Map Xamarin activities to MAUI pages using fully qualified type names
            string typeName = null;
            
            switch (viewName)
            {
                case ActivityNames.OrderCreditActivity:
                    // In MAUI, OrderCreditActivity maps to OrderCreditPage
                    typeName = "LaceupMigration.Views.OrderCreditPage, LaceupMigration";
                    break;

                case ActivityNames.OrderDetailsActivity:
                    // In MAUI, OrderDetailsActivity maps to OrderDetailsPage
                    typeName = "LaceupMigration.Views.OrderDetailsPage, LaceupMigration";
                    break;

                case ActivityNames.ProductListActivity:
                    // In MAUI, ProductListActivity maps to FullCategoryPage
                    typeName = "LaceupMigration.Views.FullCategoryPage, LaceupMigration";
                    break;

                case ActivityNames.AddItemActivity:
                    // AddItemActivity might not have a direct MAUI equivalent yet
                    // Return OrderDetailsPage as fallback
                    typeName = "LaceupMigration.Views.OrderDetailsPage, LaceupMigration";
                    break;

                default:
                    // Default fallback
                    typeName = "LaceupMigration.Views.OrderDetailsPage, LaceupMigration";
                    break;
            }

            // Use GetTypeByName for consistent type resolution
            return GetTypeByName(typeName);
        }

        /// <summary>
        /// Checks if the activity type is FullTemplateActivity (maps to SuperOrderTemplatePage in MAUI)
        /// </summary>
        public bool IsFullTemplateActivity(Type activityType)
        {
            if (activityType == null)
                return false;

            // In Xamarin, FullTemplateActivity is "LaceupAndroidApp.FullTemplateActivity"
            // In MAUI, this maps to SuperOrderTemplatePage
            var superOrderTemplateType = GetTypeByName("LaceupMigration.Views.SuperOrderTemplatePage, LaceupMigration");
            return activityType.FullName == "LaceupAndroidApp.FullTemplateActivity" ||
                   (superOrderTemplateType != null && activityType == superOrderTemplateType);
        }

        /// <summary>
        /// Gets a type by its fully qualified name, searching across loaded assemblies
        /// </summary>
        private Type GetTypeByName(string fullyQualifiedName)
        {
            // Try direct type resolution first
            Type type = Type.GetType(fullyQualifiedName);
            if (type != null)
                return type;

            // If that fails, try to find in loaded assemblies
            string typeName = fullyQualifiedName.Split(',')[0].Trim();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Continue searching
                }
            }

            return null;
        }

        public ActivityProvider()
        {
            LoadActivitys();
        }

        public void LoadActivitys()
        {
            viewRepository.Clear();

            // Default mappings - these map to MAUI pages
            viewRepository.Add(new ActivityInfo()
            {
                Name = "OrderDetailsActivity",
                Idiom = "Phone",
                Controller = "LaceupAndroidApp.PreviouslyOrderedTemplateActivity" // Maps to OrderDetailsPage in MAUI
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "OrderDetailsActivity",
                Idiom = "Pad",
                Controller = "LaceupAndroidApp.PreviouslyOrderedTemplateActivity" // Maps to OrderDetailsPage in MAUI
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "AddItemActivity",
                Idiom = "Phone",
                Controller = "LaceupAndroidApp.AddItemActivity" // Maps to OrderDetailsPage in MAUI (for now)
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "AddItemActivity",
                Idiom = "Pad",
                Controller = "LaceupAndroidApp.AddItemActivity" // Maps to OrderDetailsPage in MAUI (for now)
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "ProductListActivity",
                Idiom = "Phone",
                Controller = "LaceupAndroidApp.ProductListActivity" // Maps to FullCategoryPage in MAUI
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "ProductListActivity",
                Idiom = "Pad",
                Controller = "LaceupAndroidApp.ProductListActivity" // Maps to FullCategoryPage in MAUI
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "OrderCreditActivity",
                Idiom = "Phone",
                Controller = "LaceupAndroidApp.CreditSectionActivity" // Maps to OrderCreditPage in MAUI
            });
            viewRepository.Add(new ActivityInfo()
            {
                Name = "OrderCreditActivity",
                Idiom = "Pad",
                Controller = "LaceupAndroidApp.CreditSectionActivity" // Maps to OrderCreditPage in MAUI
            });
        }

        private class ActivityInfo
        {
            public string Name { get; set; }

            public string Idiom { get; set; }

            public string Controller { get; set; }

            public override string ToString()
            {
                return string.Format("{0},{1},{2}", Name, Idiom, Controller);
            }
        }
    }
}

