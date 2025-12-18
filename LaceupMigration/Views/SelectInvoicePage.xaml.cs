using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class SelectInvoicePage : IQueryAttributable
    {
        private readonly SelectInvoicePageViewModel _viewModel;

        public SelectInvoicePage(SelectInvoicePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("clientId", out var clientIdValue) && clientIdValue != null)
            {
                var clientId = int.Parse(clientIdValue.ToString() ?? "0");
                var fromClientDetails = query.TryGetValue("fromClientDetails", out var fromClientDetailsValue) && 
                                       fromClientDetailsValue?.ToString() == "1";
                var fromPaymentTab = query.TryGetValue("fromPaymentTab", out var fromPaymentTabValue) && 
                                    fromPaymentTabValue?.ToString() == "1";
                var orderId = query.TryGetValue("orderId", out var orderIdValue) ? 
                             int.Parse(orderIdValue.ToString() ?? "0") : 0;

                Dispatcher.Dispatch(async () => 
                    await _viewModel.InitializeAsync(clientId, fromClientDetails, fromPaymentTab, orderId));
            }
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

