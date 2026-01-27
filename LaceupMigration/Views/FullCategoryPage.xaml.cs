using LaceupMigration.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Category = LaceupMigration.Category;

namespace LaceupMigration.Views
{
    public partial class FullCategoryPage : IQueryAttributable
    {
        private readonly FullCategoryPageViewModel _viewModel;

        public FullCategoryPage(FullCategoryPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Prevent LaceupContentPage from adding its own menu - we use the ViewModel's menu instead
            UseCustomMenu = true;

            // Wire up CollectionView selection handlers (only for products, categories use TapGestureRecognizer)
            var productsView = this.FindByName<CollectionView>("ProductsCollectionView");
            if (productsView != null)
            {
                productsView.SelectionChanged += ProductsCollectionView_SelectionChanged;
            }
        }


        private async void ProductsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ProductViewModel item)
            {
                await _viewModel.ProductSelectedAsync(item);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? clientId = null;
            int? orderId = null;
            int? categoryId = null;
            string? productSearch = null;
            bool comingFromSearch = false;
            bool asCreditItem = false;
            bool asReturnItem = false;
            int? productId = null;
            bool consignmentCounting = false;
            string? comingFrom = null;

            if (query.TryGetValue("clientId", out var clientValue) && clientValue != null)
            {
                if (int.TryParse(clientValue.ToString(), out var cId))
                    clientId = cId;
            }

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
            }

            if (query.TryGetValue("categoryId", out var catValue) && catValue != null)
            {
                if (int.TryParse(catValue.ToString(), out var catId))
                    categoryId = catId;
            }

            if (query.TryGetValue("productSearch", out var searchValue) && searchValue != null)
            {
                productSearch = searchValue.ToString();
            }

            if (query.TryGetValue("comingFromSearch", out var fromSearchValue) && fromSearchValue != null)
            {
                comingFromSearch = fromSearchValue.ToString().ToLowerInvariant() == "yes" || fromSearchValue.ToString() == "true";
            }

            if (query.TryGetValue("asCreditItem", out var creditValue) && creditValue != null)
            {
                asCreditItem = creditValue.ToString() == "1" || creditValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("asReturnItem", out var returnValue) && returnValue != null)
            {
                asReturnItem = returnValue.ToString() == "1" || returnValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("productId", out var prodValue) && prodValue != null)
            {
                if (int.TryParse(prodValue.ToString(), out var pId))
                    productId = pId;
            }

            if (query.TryGetValue("consignmentCounting", out var countingValue) && countingValue != null)
            {
                consignmentCounting = countingValue.ToString() == "1" || countingValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("comingFrom", out var fromValue) && fromValue != null)
            {
                comingFrom = fromValue.ToString();
            }

            // Extract fromLoadOrder flag - defaults to false if not present
            bool fromLoadOrder = false;
            if (query.TryGetValue("fromLoadOrder", out var fromLoadValue) && fromLoadValue != null)
            {
                fromLoadOrder = fromLoadValue.ToString() == "1" || fromLoadValue.ToString().ToLowerInvariant() == "true";
            }

            Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(
                clientId: clientId,
                orderId: orderId,
                categoryId: categoryId,
                productSearch: productSearch,
                comingFromSearch: comingFromSearch,
                asCreditItem: asCreditItem,
                asReturnItem: asReturnItem,
                productId: productId,
                consignmentCounting: consignmentCounting,
                comingFrom: comingFrom,
                fromLoadOrder: fromLoadOrder));
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "fullcategory";
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
            if (currentRoute.Contains("fullcategory"))
            {
                Helpers.NavigationHelper.RemoveNavigationState(currentRoute);
            }
            else
            {
                // Fallback: try to remove by route name
                Helpers.NavigationHelper.RemoveNavigationState("fullcategory");
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

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the ViewModel's SearchQuery to trigger OnSearchQueryChanged
            // This ensures the binding works correctly and search filters in real-time
            if (sender is SearchBar searchBar && _viewModel != null)
            {
                _viewModel.SearchQuery = searchBar.Text ?? string.Empty;
            }
        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (sender is SearchBar searchBar)
            {
                searchBar.Unfocus();
                
                // Match Xamarin's SearchView_QueryTextSubmit behavior
                // When "By Product" is selected, navigate to product list with search term
                await _viewModel.HandleProductSearchSubmitAsync();
            }
        }

        private async void Category_Tapped(object sender, EventArgs e)
        {
            // Get the CategoryViewModel from the Grid's BindingContext (matches Xamarin's layout.Tag)
            if (sender is Grid grid && grid.BindingContext is CategoryViewModel viewModel)
            {
                // Match Xamarin's Layout_Click logic exactly
                if (viewModel.Subcategories.Count == 0)
                {
                    // No subcategories - navigate to products (matches Xamarin's GoToCategory)
                    // PaintSelectedRow equivalent - could set background color here if needed
                    await _viewModel.CategorySelectedAsync(viewModel);
                }
                else
                {
                    // Has subcategories - toggle expansion (matches Xamarin's expansion logic)
                    await _viewModel.ToggleCategoryExpanded(viewModel);
                }
            }
        }

        private async void Subcategory_Tapped(object sender, EventArgs e)
        {
            // Get the Category from the Frame's BindingContext (matches Xamarin's view.Tag)
            if (sender is Frame frame && frame.BindingContext is Category subcategory)
            {
                // Navigate to products in this subcategory using ViewModel method (matches Xamarin's View_Click)
                var categoryViewModel = new CategoryViewModel
                {
                    Category = subcategory,
                    Name = subcategory.Name
                };
                await _viewModel.CategorySelectedAsync(categoryViewModel);
            }
        }

        private void Subcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        private async void ProductImage_Tapped(object sender, EventArgs e)
        {
            // Get the ProductViewModel from the Image's BindingContext (matches Xamarin's ImageClicked)
            if (sender is Image image && image.BindingContext is ProductViewModel viewModel)
            {
                await _viewModel.ProductImageClickedAsync(viewModel);
            }
        }

        private async void ProductCell_Tapped(object sender, EventArgs e)
        {
            // Get the ProductViewModel from the Grid's BindingContext (matches Xamarin's ProductNameClickedHandler)
            if (sender is Grid grid && grid.BindingContext is ProductViewModel viewModel)
            {
                await _viewModel.ViewProductDetailsAsync(viewModel);
            }
        }
    }
}
