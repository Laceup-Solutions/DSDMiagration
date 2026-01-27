using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class NoServicePage : LaceupContentPage, IQueryAttributable
    {
        private readonly NoServicePageViewModel _viewModel;

        public NoServicePage(NoServicePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
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

        /// <summary>
        /// Override GoBack to handle cleanup when user navigates back without completing the NoService order.
        /// This matches Xamarin NoServiceActivity behavior - if order is not completed, delete it.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // Call ViewModel's GoBackAsync which handles cleanup logic
            // This is async, but GoBack() is synchronous, so we fire and forget
            _ = _viewModel.GoBackAsync();
        }

    }
}
