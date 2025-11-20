using LaceupMigration.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class FullCategoryPage : ContentPage, IQueryAttributable
    {
        private readonly FullCategoryPageViewModel _viewModel;

        public FullCategoryPage(FullCategoryPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }

            // Wire up CollectionView selection handlers
            var categoriesView = this.FindByName<CollectionView>("CategoriesCollectionView");
            if (categoriesView != null)
            {
                categoriesView.SelectionChanged += CategoriesCollectionView_SelectionChanged;
            }

            var productsView = this.FindByName<CollectionView>("ProductsCollectionView");
            if (productsView != null)
            {
                productsView.SelectionChanged += ProductsCollectionView_SelectionChanged;
            }
        }

        private async void CategoriesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CategoryViewModel item)
            {
                await _viewModel.CategorySelectedAsync(item);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
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
                comingFrom: comingFrom));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (sender is SearchBar searchBar)
            {
                searchBar.Unfocus();
            }
        }
    }
}
