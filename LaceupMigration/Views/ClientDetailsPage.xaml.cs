using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ClientDetailsPage : LaceupContentPage, IQueryAttributable
    {
        private readonly ClientDetailsPageViewModel _viewModel;
        private readonly List<ToolbarItem> _savedShellToolbarItems = new();

        public ClientDetailsPage(ClientDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }

            OverrideBaseMenu = true;
            
            OrdersCollectionView.SelectionChanged += OrdersCollectionView_SelectionChanged;
            InvoicesCollectionView.SelectionChanged += InvoicesCollectionView_SelectionChanged;
        }

        /// <summary>
        /// Override GoBack to clear navigation state when navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            // This prevents the app from restoring to ClientDetailsPage after closing it
            Helpers.NavigationHelper.RemoveNavigationState("clientdetails");
            
            // Navigate back
            base.GoBack();
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
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "clientdetails";
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
            
            // Note: State is saved in ApplyQueryAttributes when navigating TO the page
            // We don't save state here to avoid re-saving after GoBack() removes it
        }

        private async void OrdersCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClientOrderViewModel order)
            {
                await _viewModel.HandleOrderSelectedAsync(order);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        private async void InvoicesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClientInvoiceViewModel invoice)
            {
                await _viewModel.HandleInvoiceSelectedAsync(invoice);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }
    }
}
