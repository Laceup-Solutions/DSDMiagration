using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ProductDetailsPage : LaceupContentPage, IQueryAttributable
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Refresh product data when appearing to ensure warehouse inventory (OH) is up-to-date
            // This is especially important after accepting loads, as warehouse inventory is updated on the server
            // and needs to be refreshed from the server
            await _viewModel.RefreshProductDataAsync();
        }

        /// <summary>Both physical and nav bar back use this; remove state then navigate.</summary>
        protected override void GoBack()
        {
            Helpers.NavigationHelper.RemoveNavigationState("productdetails");
            base.GoBack();
        }

        private async void ProductImage_Tapped(object sender, EventArgs e)
        {
            if (_viewModel.Product != null)
            {
                await Shell.Current.GoToAsync($"productimage?productId={_viewModel.Product.ProductId}");
            }
        }
        
        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            return new List<MenuOption>
            {
                new MenuOption("Print Label", async () => 
                {
                    // Call the ViewModel method
                    await _viewModel.PrintProductLabelAsync();
                })
            };
        }
    }
}

