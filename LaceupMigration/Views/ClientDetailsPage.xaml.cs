using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ClientDetailsPage : ContentPage, IQueryAttributable
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

            OrdersCollectionView.SelectionChanged += OrdersCollectionView_SelectionChanged;
            InvoicesCollectionView.SelectionChanged += InvoicesCollectionView_SelectionChanged;
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

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (sender is SearchBar searchBar)
            {
                searchBar.Unfocus();
            }
        }
    }
}
