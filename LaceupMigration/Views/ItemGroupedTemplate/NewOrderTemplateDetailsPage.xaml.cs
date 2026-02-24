using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views.ItemGroupedTemplate
{
    public partial class NewOrderTemplateDetailsPage : LaceupContentPage, IQueryAttributable
    {
        private readonly NewOrderTemplateDetailsPageViewModel _viewModel;

        public NewOrderTemplateDetailsPage(NewOrderTemplateDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
            var route = "newordertemplatedetails";
            if (query != null && query.Count > 0)
            {
                var q = query.Where(kvp => kvp.Value != null).Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString()!)}").ToArray();
                if (q.Length > 0) route += "?" + string.Join("&", q);
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override string? GetRouteName() => "newordertemplatedetails";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
