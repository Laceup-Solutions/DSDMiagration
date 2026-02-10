using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class EditClientPage : IQueryAttributable
    {
        private readonly EditClientPageViewModel _viewModel;

        public EditClientPage(EditClientPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("clientId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var clientId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(clientId));
                }
            }
            
            // Handle Bill To address result from AddClientBillToPage
            _viewModel.OnBillToSelected(query);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "editclient";
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

        protected override string? GetRouteName() => "editclient";
    }
}

