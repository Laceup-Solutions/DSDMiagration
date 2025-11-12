using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class ManageRoutePage : ContentPage, IQueryAttributable
    {
        private readonly ManageRoutePageViewModel _viewModel;

        public ManageRoutePage(ManageRoutePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.OnNavigatedTo(query);
        }
    }
}

