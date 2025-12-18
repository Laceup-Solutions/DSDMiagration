using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Views
{
    public partial class PreviouslyOrderedTemplatePage : LaceupContentPage, IQueryAttributable
    {
        private readonly PreviouslyOrderedTemplatePageViewModel _viewModel;

        public PreviouslyOrderedTemplatePage(PreviouslyOrderedTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        // Override to integrate ViewModel menu with base menu
        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            // Get menu options from ViewModel - BuildMenuOptions returns List<MenuOption>
            // But the ViewModel uses a private record MenuOption, so we need to convert it
            // Actually, let's check if BuildMenuOptions is accessible and returns the right type
            return _viewModel.BuildMenuOptions();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "previouslyorderedtemplate";
            if (query != null && query.Count > 0)
            {
                var queryParams = query
                    .Where(kvp => kvp.Value != null)
                    .Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString())}")
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
            
            // Update menu toolbar item after order is loaded (important for state restoration)
            // This ensures the menu appears even when loading from state
            UpdateMenuToolbarItem();
        }

        /// <summary>
        /// Override GoBack to handle order finalization before navigating away.
        /// This is called by both the physical back button and navigation bar back button.
        /// </summary>
        protected override void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            // Build route from current state or use saved route
            var currentRoute = Shell.Current.CurrentState?.Location?.OriginalString ?? "";
            if (currentRoute.Contains("previouslyorderedtemplate"))
            {
                Helpers.NavigationHelper.RemoveNavigationState(currentRoute);
            }
            else
            {
                // Fallback: try to remove by route name (will remove any previouslyorderedtemplate state)
                Helpers.NavigationHelper.RemoveNavigationState("previouslyorderedtemplate");
            }
            
            // Call ViewModel's GoBackAsync which handles finalization logic
            // This is async, but GoBack() is synchronous, so we fire and forget
            _ = _viewModel.GoBackAsync();
        }
    }
}

