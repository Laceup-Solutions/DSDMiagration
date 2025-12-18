using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class AddItemPage : IQueryAttributable
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
            bool asCreditItem = false;
            int type = 0;
            int reasonId = 0;
            bool consignmentCounting = false;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    orderId = oId;
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

            if (orderId.HasValue && productId.HasValue)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(
                    orderId.Value,
                    productId.Value,
                    asCreditItem,
                    type,
                    reasonId,
                    consignmentCounting));
            }
        }
    }
}

