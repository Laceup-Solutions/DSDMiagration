using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class AddItemPage : LaceupContentPage, IQueryAttributable
    {
        private readonly AddItemPageViewModel _viewModel;

        public AddItemPage(AddItemPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            int? productId = null;
            int? orderDetailId = null;
            bool asCreditItem = false;
            int type = 0;
            int reasonId = 0;
            bool consignmentCounting = false;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
            }

            if (query.TryGetValue("orderDetail", out var orderDetailValue) && orderDetailValue != null)
            {
                if (int.TryParse(orderDetailValue.ToString(), out var odId))
                    orderDetailId = odId;
            }

            if (query.TryGetValue("productId", out var productValue) && productValue != null)
            {
                if (int.TryParse(productValue.ToString(), out var pId))
                    productId = pId;
            }

            if (query.TryGetValue("asCreditItem", out var creditValue) && creditValue != null)
            {
                asCreditItem = creditValue.ToString() == "1" || creditValue.ToString().ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("type", out var typeValue) && typeValue != null)
            {
                if (int.TryParse(typeValue.ToString(), out var t))
                    type = t;
            }

            if (query.TryGetValue("reasonId", out var reasonValue) && reasonValue != null)
            {
                if (int.TryParse(reasonValue.ToString(), out var rId))
                    reasonId = rId;
            }

            if (query.TryGetValue("consignmentCounting", out var countingValue) && countingValue != null)
            {
                consignmentCounting = countingValue.ToString() == "1" || countingValue.ToString().ToLowerInvariant() == "true";
            }

            if (orderId.HasValue)
            {
                if (orderDetailId.HasValue)
                {
                    // Initialize with orderDetail (editing existing detail)
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeWithOrderDetailAsync(
                        orderId.Value,
                        orderDetailId.Value,
                        asCreditItem,
                        type,
                        reasonId,
                        consignmentCounting));
                }
                else if (productId.HasValue)
                {
                    // Initialize with productId (adding new item)
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(
                        orderId.Value,
                        productId.Value,
                        asCreditItem,
                        type,
                        reasonId,
                        consignmentCounting));
                }
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "additem";
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

        protected override string? GetRouteName() => "additem";
    }
}

