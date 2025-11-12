using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class SentOrdersOrdersListPage : ContentPage, IQueryAttributable
    {
        private readonly SentOrdersOrdersListPageViewModel _viewModel;

        public SentOrdersOrdersListPage(SentOrdersOrdersListPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("packagePath", out var packagePathValue) && 
                query.TryGetValue("orderId", out var orderIdValue))
            {
                var packagePath = packagePathValue?.ToString() ?? string.Empty;
                if (int.TryParse(orderIdValue?.ToString(), out var orderId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(packagePath, orderId));
                }
            }
        }
    }
}

