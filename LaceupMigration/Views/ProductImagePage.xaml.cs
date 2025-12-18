using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ProductImagePage : IQueryAttributable
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
            if (query.TryGetValue("productId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var productId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(productId));
                }
            }
        }
    }
}

