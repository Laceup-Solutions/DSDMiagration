using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ProductDetailsPage : IQueryAttributable
    {
        private readonly ProductDetailsPageViewModel _viewModel;

        public ProductDetailsPage(ProductDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int productId = 0;
            int? clientId = null;

            if (query.TryGetValue("productId", out var prodValue) && prodValue != null)
            {
                if (int.TryParse(prodValue.ToString(), out var pId))
                    productId = pId;
            }

            if (query.TryGetValue("clientId", out var clientValue) && clientValue != null)
            {
                if (int.TryParse(clientValue.ToString(), out var cId))
                    clientId = cId;
            }

            _viewModel.Initialize(productId, clientId);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "productdetails";
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
            Helpers.NavigationHelper.RemoveNavigationState("productdetails");
            return false; // Allow navigation
        }

        private async void ProductImage_Tapped(object sender, EventArgs e)
        {
            if (_viewModel.Product != null)
            {
                await Shell.Current.GoToAsync($"productimage?productId={_viewModel.Product.ProductId}");
            }
        }
    }
}

