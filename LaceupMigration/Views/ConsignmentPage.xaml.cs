using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class ConsignmentPage : IQueryAttributable
    {
        private readonly ConsignmentPageViewModel _viewModel;

        public ConsignmentPage(ConsignmentPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var orderId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId));
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}
