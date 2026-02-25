using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class RandomWeightAddItemPage : LaceupContentPage, IQueryAttributable
    {
        private readonly RandomWeightAddItemPageViewModel _viewModel;

        public RandomWeightAddItemPage(RandomWeightAddItemPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);

            var route = "randomweightadditem";
            if (query != null && query.Count > 0)
            {
                var q = query.Where(kvp => kvp.Value != null).Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString()!)}").ToArray();
                if (q.Length > 0) route += "?" + string.Join("&", q);
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override string? GetRouteName() => "randomweightadditem";
    }
}
