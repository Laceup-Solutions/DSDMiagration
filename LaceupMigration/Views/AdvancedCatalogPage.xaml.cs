using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class AdvancedCatalogPage : ContentPage, IQueryAttributable
    {
        private readonly AdvancedCatalogPageViewModel _viewModel;
        private bool _isNavigatingBack = false;

        public AdvancedCatalogPage(AdvancedCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Subscribe to Shell navigation events
            Shell.Current.Navigating += OnShellNavigating;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Unsubscribe from Shell navigation events
            Shell.Current.Navigating -= OnShellNavigating;
        }

        protected override bool OnBackButtonPressed()
        {
            // Handle physical back button - call GoBackAsync which will finalize the order
            _viewModel.GoBackCommand.ExecuteAsync(null);
            return true; // Prevent default back navigation
        }

        private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            // Only intercept if we're navigating away from this page and it's a back navigation
            if (_isNavigatingBack || e.Target.Location.OriginalString.Contains("advancedcatalog"))
                return;

            // Check if we're navigating back (going to parent)
            var currentRoute = Shell.Current.CurrentState?.Location?.OriginalString ?? "";
            if (e.Target.Location.OriginalString == ".." || 
                (!e.Target.Location.OriginalString.Contains("advancedcatalog") && 
                 !string.IsNullOrEmpty(currentRoute) && currentRoute.Contains("advancedcatalog")))
            {
                _isNavigatingBack = true;
                
                // Cancel the navigation
                e.Cancel();
                
                // Call finalization logic
                await _viewModel.GoBackAsync();
                
                _isNavigatingBack = false;
            }
        }

        private void FreeItemCheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is AdvancedCatalogItemViewModel item)
            {
                _viewModel.ToggleFreeItemCommand.ExecuteAsync(item);
            }
        }
    }
}

