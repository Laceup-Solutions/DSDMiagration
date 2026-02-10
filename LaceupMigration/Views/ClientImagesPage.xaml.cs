using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ClientImagesPage : IQueryAttributable
    {
        private readonly ClientImagesPageViewModel _viewModel;

        public ClientImagesPage(ClientImagesPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("clientId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var clientId))
                {
                    // Initialize on main thread to ensure proper execution
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(clientId));
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        protected override string? GetRouteName() => "clientimages";

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is ClientImagesPageViewModel vm)
            {
                vm.GoBackCommand.Execute(null);
                return true; // Prevent default back behavior
            }
            return base.OnBackButtonPressed();
        }
    }
}
