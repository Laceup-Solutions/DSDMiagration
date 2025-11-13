using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ProductCatalogPage : ContentPage, IQueryAttributable
    {
        private readonly ProductCatalogPageViewModel _viewModel;

        public ProductCatalogPage(ProductCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

