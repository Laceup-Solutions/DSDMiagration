using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class FullProductListPage : LaceupContentPage, IQueryAttributable
    {
        private readonly FullProductListPageViewModel _viewModel;

        public FullProductListPage(FullProductListPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            int? categoryId = null;
            int? clientId = null;
            bool fromCreditTemplate = false;
            bool asPresale = false;
            string? productSearch = null;

            if (query.TryGetValue("orderId", out var o) && o != null && int.TryParse(o.ToString(), out var oid))
                orderId = oid;
            if (query.TryGetValue("categoryId", out var c) && c != null && int.TryParse(c.ToString(), out var cid))
                categoryId = cid;
            if (query.TryGetValue("clientId", out var cl) && cl != null && int.TryParse(cl.ToString(), out var clid))
                clientId = clid;
            if (query.TryGetValue("fromCreditTemplate", out var fc) && fc != null)
                fromCreditTemplate = fc.ToString() == "1" || string.Equals(fc.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            if (query.TryGetValue("asPresale", out var ap) && ap != null)
                asPresale = ap.ToString() == "1" || string.Equals(ap.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            if (query.TryGetValue("productSearch", out var ps) && ps != null)
                productSearch = ps.ToString();
            string? comingFrom = null;
            if (query.TryGetValue("comingFrom", out var cf) && cf != null)
                comingFrom = cf.ToString();
            string? returnToRoute = null;
            if (query.TryGetValue("returnToRoute", out var rtr) && rtr != null && !string.IsNullOrWhiteSpace(rtr.ToString()))
                returnToRoute = rtr.ToString();

            _viewModel.SetNavigationQuery(orderId, categoryId, clientId, fromCreditTemplate, asPresale, productSearch, comingFrom, returnToRoute);
            _viewModel.LoadProducts();

            var route = "fullproductlist";
            if (query != null && query.Count > 0)
            {
                var q = query.Where(kvp => kvp.Value != null).Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString()!)}").ToArray();
                if (q.Length > 0) route += "?" + string.Join("&", q);
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override string? GetRouteName() => "fullproductlist";
    }
}
