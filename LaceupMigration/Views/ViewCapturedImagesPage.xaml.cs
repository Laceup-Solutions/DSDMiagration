using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ViewCapturedImagesPage : LaceupContentPage, IQueryAttributable
    {
        private readonly ViewCapturedImagesPageViewModel _viewModel;

        public ViewCapturedImagesPage(ViewCapturedImagesPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            bool fromSelfService = false;

            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oid))
                    orderId = oid;
            }

            if (query.TryGetValue("fromSelfService", out var selfValue) && selfValue != null)
            {
                if (int.TryParse(selfValue.ToString(), out var v))
                    fromSelfService = v != 0;
            }

            _viewModel.SetNavigationQuery(orderId);
            Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, fromSelfService));
        }
    }
}
