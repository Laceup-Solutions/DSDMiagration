using LaceupMigration.ViewModels;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class AdvancedCatalogPage : LaceupContentPage, IQueryAttributable
    {
        private readonly AdvancedCatalogPageViewModel _viewModel;

        public AdvancedCatalogPage(AdvancedCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Subscribe to property changes to update Grid columns
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        // Override to integrate ViewModel menu with base menu
        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            // Get menu options from ViewModel - BuildMenuOptions returns List<MenuOption>
            return _viewModel.BuildMenuOptions();
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
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "advancedcatalog";
            if (query != null && query.Count > 0)
            {
                var queryParams = query
                    .Where(kvp => kvp.Value != null)
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}")
                    .ToArray();
                if (queryParams.Length > 0)
                {
                    route += "?" + string.Join("&", queryParams);
                }
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();

            UpdateMenuToolbarItem();
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
        }

        /// <summary>
        /// Override GoBack to handle order finalization before navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            var currentRoute = Shell.Current.CurrentState?.Location?.OriginalString ?? "";
            if (currentRoute.Contains("advancedcatalog"))
            {
                Helpers.NavigationHelper.RemoveNavigationState(currentRoute);
            }
            else
            {
                // Fallback: try to remove by route name
                Helpers.NavigationHelper.RemoveNavigationState("advancedcatalog");
            }
            
            // Call ViewModel's GoBackAsync which handles finalization logic
            // This is async, but GoBack() is synchronous, so we fire and forget
            _ = _viewModel.GoBackAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Handle physical back button - call GoBack which will finalize the order
            GoBack();
            return true; // Prevent default back navigation
        }

    }
}

