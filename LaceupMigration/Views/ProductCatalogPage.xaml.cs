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

        private void UpdateToolbar()
        {
            ToolbarItems.Clear();
            if (_viewModel.IsFromLoadOrder)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Add To Order",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(() => _ = _viewModel.AddItemsCommand.ExecuteAsync(null))
                });
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Menu",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(() => _ = _viewModel.ShowMenuCommand.ExecuteAsync(null))
                });
            }
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
            UpdateToolbar();
        }

        /// <summary>
        /// Override GoBack to remove navigation state when navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove current page's state so back doesn't re-push (e.g. from OrderCredit > ProductCatalog, back must remove ProductCatalog state)
            Helpers.NavigationHelper.RemoveNavigationState("productcatalog");
            System.Diagnostics.Debug.WriteLine("[ProductCatalog] GoBack (page): removing state productcatalog, popping");
            Shell.Current.GoToAsync("..");
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

