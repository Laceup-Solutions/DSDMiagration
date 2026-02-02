using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class NewLoadOrderTemplatePage : LaceupContentPage, IQueryAttributable
    {
        private readonly NewLoadOrderTemplatePageViewModel _viewModel;

        public NewLoadOrderTemplatePage(NewLoadOrderTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Prevent LaceupContentPage from adding hamburger menu
            UseCustomMenu = true;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }
            
            // Set the appropriate template based on IsAdvancedCatalogTemplate
            UpdateItemTemplate();
            
            // Subscribe to property changes to update template if it changes
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.IsAdvancedCatalogTemplate))
                {
                    UpdateItemTemplate();
                }
            };
        }
        
        private void UpdateItemTemplate()
        {
            if (OrderDetailsCollectionView == null)
                return;
                
            if (_viewModel.IsAdvancedCatalogTemplate)
            {
                OrderDetailsCollectionView.ItemTemplate = (DataTemplate)OrderDetailsCollectionView.Resources["AdvancedCatalogLoadOrderTemplate"];
            }
            else
            {
                OrderDetailsCollectionView.ItemTemplate = (DataTemplate)OrderDetailsCollectionView.Resources["StandardLoadOrderTemplate"];
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0;
            bool canGetOutIntent = false;
            int? clientIdIntent = null;

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("canGetOutIntent", out var canGetOutValue) && canGetOutValue != null)
            {
                var str = canGetOutValue.ToString();
                canGetOutIntent = str == "1" || str.ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("clientIdIntent", out var clientIdValue) && clientIdValue != null)
            {
                if (int.TryParse(clientIdValue.ToString(), out var cId))
                    clientIdIntent = cId;
            }

            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, canGetOutIntent, clientIdIntent));
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "newloadordertemplate";
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
            Helpers.NavigationHelper.RemoveNavigationState("newloadordertemplate");
            base.GoBack();
        }

        private async void QtyButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LoadOrderDetailViewModel item)
            {
                await _viewModel.QtyButtonCommand.ExecuteAsync(item);
            }
        }

        private async void ProductImage_Tapped(object sender, EventArgs e)
        {
            // Get the LoadOrderDetailViewModel from the Image's BindingContext
            if (sender is Image image && image.BindingContext is LoadOrderDetailViewModel item && item.Product != null)
            {
                // Navigate to ProductImagePage to expand the image (matches FullCategoryPage pattern)
                await Shell.Current.GoToAsync($"productimage?productId={item.Product.ProductId}");
            }
        }
    }
}

