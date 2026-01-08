
using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        private readonly MainPageViewModel _viewModel;
        private bool _shouldSyncData = false;
        
        public MainPage(MainPageViewModel viewModel)
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
            if (query.TryGetValue("shouldSyncData", out var shouldSyncDataValue))
            {
                if (bool.TryParse(shouldSyncDataValue?.ToString(), out var shouldSyncData))
                {
                    _shouldSyncData = shouldSyncData;
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync(_shouldSyncData);
            _shouldSyncData = false;
        }
    }
}

