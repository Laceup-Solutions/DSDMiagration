using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Linq;

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
            
            // Subscribe to property changes to update Grid columns
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Subscribe to Shell navigation events
            Shell.Current.Navigating += OnShellNavigating;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdvancedCatalogPageViewModel.ButtonGridColumns) ||
                e.PropertyName == nameof(AdvancedCatalogPageViewModel.ShowSalesButton) ||
                e.PropertyName == nameof(AdvancedCatalogPageViewModel.ShowDumpsButton) ||
                e.PropertyName == nameof(AdvancedCatalogPageViewModel.ShowReturnsButton))
            {
                UpdateButtonGridColumns();
            }
        }

        private void UpdateButtonGridColumns()
        {
            if (ButtonGrid == null || _viewModel == null)
                return;

            // Parse the column definitions string (e.g., "*,*" or "*,*,*")
            var columnDefs = _viewModel.ButtonGridColumns.Split(',');
            var columnDefinitions = new ColumnDefinitionCollection();
            
            foreach (var def in columnDefs)
            {
                columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            
            ButtonGrid.ColumnDefinitions = columnDefinitions;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
            UpdateButtonGridColumns();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Unsubscribe from property changes
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
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

