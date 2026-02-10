using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ViewInvoiceImagesPage : IQueryAttributable
    {
        private readonly ViewInvoiceImagesPageViewModel _viewModel;

        public ViewInvoiceImagesPage(ViewInvoiceImagesPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "viewinvoiceimages";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("invoiceNumber", out var value) && value != null)
            {
                var invoiceNumber = value.ToString() ?? string.Empty;
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(invoiceNumber));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

