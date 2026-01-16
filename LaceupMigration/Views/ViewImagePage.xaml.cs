using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ViewImagePage : IQueryAttributable
    {
        private readonly ViewImagePageViewModel _viewModel;

        public ViewImagePage(ViewImagePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            string? imagePath = null;

            if (query.TryGetValue("imagePath", out var imagePathValue) && imagePathValue != null)
            {
                imagePath = Uri.UnescapeDataString(imagePathValue.ToString() ?? string.Empty);
            }

            // Initialize immediately if we have a path (matches ProductImagePage pattern)
            // This ensures initialization happens even when Shell reuses the page instance
            if (!string.IsNullOrEmpty(imagePath))
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(imagePath));
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "viewimage";
            if (query != null && query.Count > 0)
            {
                var queryParams = query
                    .Where(kvp => kvp.Value != null)
                    .Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString())}")
                    .ToArray();
                if (queryParams.Length > 0)
                {
                    route += "?" + string.Join("&", queryParams);
                }
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override bool OnBackButtonPressed()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("viewimage");
            return false; // Allow navigation
        }
    }
}
