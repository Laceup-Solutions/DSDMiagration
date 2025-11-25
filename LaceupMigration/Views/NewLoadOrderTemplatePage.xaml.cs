using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class NewLoadOrderTemplatePage : ContentPage, IQueryAttributable
    {
        private readonly NewLoadOrderTemplatePageViewModel _viewModel;

        public NewLoadOrderTemplatePage(NewLoadOrderTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                // Menu functionality can be added later if needed
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int orderId = 0;
            bool canGetOutIntent = false;
            int? clientIdIntent = null;

            if (query.TryGetValue("orderId", out var orderIdValue) && orderIdValue != null)
            {
                int.TryParse(orderIdValue.ToString(), out orderId);
            }

            if (query.TryGetValue("canGetOutIntent", out var canGetOutValue) && canGetOutValue != null)
            {
                var str = canGetOutValue.ToString();
                canGetOutIntent = str == "1" || str.ToLowerInvariant() == "true";
            }

            if (query.TryGetValue("clientIdIntent", out var clientIdValue) && clientIdValue != null)
            {
                if (int.TryParse(clientIdValue.ToString(), out var cId))
                    clientIdIntent = cId;
            }

            if (orderId > 0)
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId, canGetOutIntent, clientIdIntent));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private async void QtyButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LoadOrderDetailViewModel item)
            {
                await _viewModel.QtyButtonCommand.ExecuteAsync(item);
            }
        }
    }
}

