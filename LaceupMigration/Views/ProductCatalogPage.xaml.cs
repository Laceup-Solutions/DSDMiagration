using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ProductCatalogPage : IQueryAttributable
    {
        private readonly ProductCatalogPageViewModel _viewModel;

        public ProductCatalogPage(ProductCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            // Prevent LaceupContentPage from adding its own menu - we use the ViewModel's menu instead
            UseCustomMenu = true;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "productcatalog";
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>
        /// Override GoBack to remove navigation state when navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            var currentRoute = Shell.Current.CurrentState?.Location?.OriginalString ?? "";
            if (currentRoute.Contains("productcatalog"))
            {
                Helpers.NavigationHelper.RemoveNavigationState(currentRoute);
            }
            else
            {
                // Fallback: try to remove by route name
                Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
            }
            
            // Navigate back
            Shell.Current.GoToAsync("..");
        }

        protected override bool OnBackButtonPressed()
        {
            // Handle physical back button - call GoBack which will remove state
            GoBack();
            return true; // Prevent default back navigation (we handle it in GoBack)
        }

        private async void OnCellTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is CatalogItemViewModel item)
            {
                await _viewModel.NavigateToAddItemAsync(item);
            }
        }
    }
}

