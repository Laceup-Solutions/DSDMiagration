using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class ProductDetailsPage : ContentPage, IQueryAttributable
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

