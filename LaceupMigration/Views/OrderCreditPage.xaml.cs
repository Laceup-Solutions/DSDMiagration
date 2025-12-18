using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.Views
{
    public partial class OrderCreditPage : LaceupContentPage, IQueryAttributable
    {
        private readonly OrderCreditPageViewModel _viewModel;

        public OrderCreditPage(OrderCreditPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        // Override to integrate ViewModel menu with base menu
        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            // Get menu options from ViewModel - they're already MenuOption type
            return _viewModel.BuildMenuOptions();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0;
            bool asPresale = false;

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("asPresale", out var asPresaleValue) && asPresaleValue != null)
            {
                bool.TryParse(asPresaleValue.ToString(), out asPresale);
            }

            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, asPresale));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
