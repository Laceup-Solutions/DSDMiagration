using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadEditDeliveryPage : ContentPage, IQueryAttributable
    {
        private readonly AcceptLoadEditDeliveryPageViewModel _viewModel;

        public AcceptLoadEditDeliveryPage(AcceptLoadEditDeliveryPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Match Xamarin: Intent.Extras.Get(ordersIdsIntent)
            string ordersIds = string.Empty;
            bool changed = false;
            bool inventoryAccepted = false;
            bool canLeave = true;
            string uniqueId = null;

            if (query.TryGetValue("orderIds", out var orderIdsValue))
                ordersIds = orderIdsValue?.ToString() ?? string.Empty;

            if (query.TryGetValue("changed", out var changedValue))
                bool.TryParse(changedValue?.ToString(), out changed);

            if (query.TryGetValue("inventoryAccepted", out var acceptedValue))
                bool.TryParse(acceptedValue?.ToString(), out inventoryAccepted);

            if (query.TryGetValue("canLeave", out var canLeaveValue))
                bool.TryParse(canLeaveValue?.ToString(), out canLeave);

            if (query.TryGetValue("uniqueId", out var uniqueIdValue))
                uniqueId = uniqueIdValue?.ToString();

            Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(ordersIds, changed, inventoryAccepted, canLeave, uniqueId));
        }

        protected override bool OnBackButtonPressed()
        {
            // Match Xamarin OnKeyDown logic - navigation handled in ViewModel
            return base.OnBackButtonPressed();
        }
    }
}

