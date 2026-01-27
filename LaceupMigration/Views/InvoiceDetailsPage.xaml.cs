using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class InvoiceDetailsPage : LaceupContentPage, IQueryAttributable
    {
        private readonly InvoiceDetailsPageViewModel _viewModel;

        public InvoiceDetailsPage(InvoiceDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            if (_viewModel != null)
            {
                // Get menu options from ViewModel
                return _viewModel.BuildMenuOptions();
            }
            return new List<MenuOption>();
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
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "invoicedetails";
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
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>Both physical and nav bar back use this; remove state then navigate.</summary>
        protected override void GoBack()
        {
            Helpers.NavigationHelper.RemoveNavigationState("invoicedetails");
            base.GoBack();
        }
    }
}

