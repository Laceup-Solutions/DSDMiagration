using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ProductImagePage : LaceupContentPage, IQueryAttributable
    {
        private readonly ProductImagePageViewModel _viewModel;

        public ProductImagePage(ProductImagePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? productId = null;
            string? imagePath = null;

            if (query.TryGetValue("productId", out var productIdValue) && productIdValue != null)
            {
                if (int.TryParse(productIdValue.ToString(), out var id))
                {
                    productId = id;
                }
            }

            if (query.TryGetValue("imagePath", out var imagePathValue) && imagePathValue != null)
            {
                imagePath = Uri.UnescapeDataString(imagePathValue.ToString() ?? string.Empty);
            }

            if (productId.HasValue || !string.IsNullOrEmpty(imagePath))
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(productId, imagePath));
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "productimage";
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

        protected override string? GetRouteName() => "productimage";
    }
}

