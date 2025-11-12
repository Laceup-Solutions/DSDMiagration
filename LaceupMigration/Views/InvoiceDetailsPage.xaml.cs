using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class InvoiceDetailsPage : ContentPage, IQueryAttributable
    {
        private readonly InvoiceDetailsPageViewModel _viewModel;

        public InvoiceDetailsPage(InvoiceDetailsPageViewModel viewModel)
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
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("invoiceId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var invoiceId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(invoiceId));
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

