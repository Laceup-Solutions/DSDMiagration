using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ManageDepartmentsPage : IQueryAttributable
    {
        private readonly ManageDepartmentsPageViewModel _viewModel;

        public ManageDepartmentsPage(ManageDepartmentsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override string? GetRouteName() => "managedepartments";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("clientId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var clientId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(clientId));
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
