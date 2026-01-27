using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;

namespace LaceupMigration.Views
{
    public partial class LaceupContentPage : ContentPage
    {
        protected DialogService? _dialogService;
        protected AdvancedOptionsService? _advancedOptionsService;
        protected ILaceupAppService? _appService;

        // Virtual method for pages to override and add their own menu options
        // Pages can return their ViewModel's menu options here
        protected virtual List<MenuOption> GetPageSpecificMenuOptions() => new List<MenuOption>();

        // Property for pages to set and completely replace the menu
        // If true, only page-specific options will be shown (no base menu)
        protected bool OverrideBaseMenu { get; set; } = false;

        // Property for pages to completely override the menu behavior
        // If true, base menu logic is bypassed
        protected bool UseCustomMenu { get; set; } = false;

        public LaceupContentPage()
        {
            // Add common menu toolbar item only if page doesn't already have one
            // Pages can remove this in their constructor if they want to use ViewModel menu
            
            // Set up back button override - works for both physical back button and navigation bar back button
            BackButtonOverride();
        }

        /// <summary>
        /// Override back button behavior for both physical back button and navigation bar back button.
        /// Pages can override GoBack() to provide custom back navigation logic.
        /// </summary>
        public void BackButtonOverride()
        {
            var backCommand = new Command(GoBack);

            // Set the back button behavior for this specific page
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = backCommand
            });
        }

        /// <summary>
        /// Virtual method for pages to override and provide custom back navigation logic.
        /// Default implementation removes state and navigates back.
        /// Called by both physical back button (via OnBackButtonPressed) and navigation bar back button (via BackButtonBehavior).
        /// </summary>
        protected virtual void GoBack()
        {
            // [ACTIVITY STATE]: Automatically remove state when navigating away via back button
            // This works for both physical back button and navigation bar back button
            RemoveNavigationState();
            
            // Navigate back
            Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Routes physical back button through GoBack() so both physical and nav bar back use the same logic.
        /// State is removed and navigation happens in GoBack(); override GoBack() for custom behavior.
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            GoBack();
            return true; // We handle back; GoBack() performs RemoveNavigationState + navigation when appropriate
        }

        /// <summary>
        /// Removes the navigation state for the current page.
        /// Automatically detects the route from the current Shell state.
        /// </summary>
        protected virtual void RemoveNavigationState()
        {
            try
            {
                // Get current route from Shell state
                var currentState = Shell.Current.CurrentState;
                if (currentState?.Location != null)
                {
                    var location = currentState.Location.OriginalString;
                    
                    // Extract the last route part (the current page)
                    // Location format: "//MainPage/Clients/clientdetails?clientId=123"
                    var routeParts = location.Split('/')
                        .Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("//"))
                        .ToList();
                    
                    if (routeParts.Count > 0)
                    {
                        // Get the last route part (current page)
                        var lastRoute = routeParts.LastOrDefault();
                        if (!string.IsNullOrEmpty(lastRoute))
                        {
                            // Remove query parameters to get base route
                            var baseRoute = lastRoute.Split('?')[0];
                            
                            // Skip tab routes (they're part of Shell structure, not navigation destinations)
                            var tabRoutes = new[] { "MainPage", "Clients", "Invoices", "Orders", "Payments" };
                            if (!tabRoutes.Contains(baseRoute, StringComparer.OrdinalIgnoreCase))
                            {
                                // Remove state for this route
                                Helpers.NavigationHelper.RemoveNavigationState(baseRoute);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't prevent navigation
                System.Diagnostics.Debug.WriteLine($"[LaceupContentPage] Error removing navigation state: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Update menu toolbar item when page appears (in case options changed)
            // Ensure this runs on the main thread to avoid Android view hierarchy errors
            if (Dispatcher.IsDispatchRequired)
            {
                Dispatcher.Dispatch(() => UpdateMenuToolbarItem());
            }
            else
            {
                UpdateMenuToolbarItem();
            }
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            
            // Initialize services when handler is available
            if (Handler?.MauiContext != null)
            {
                _dialogService = Handler.MauiContext.Services.GetService<DialogService>();
                _appService = Handler.MauiContext.Services.GetService<ILaceupAppService>();
                if (_dialogService != null && _appService != null)
                {
                    _advancedOptionsService = Handler.MauiContext.Services.GetService<AdvancedOptionsService>() 
                        ?? new AdvancedOptionsService(_dialogService, _appService);
                }
            }

            // Update menu toolbar item visibility
            // Ensure this runs on the main thread
            if (Dispatcher.IsDispatchRequired)
            {
                Dispatcher.Dispatch(() => UpdateMenuToolbarItem());
            }
            else
            {
                UpdateMenuToolbarItem();
            }
        }

        protected void UpdateMenuToolbarItem()
        {
            // Don't modify menu if using custom menu
            if (UseCustomMenu)
                return;

            // Remove existing menu item if present
            var existingItem = ToolbarItems.FirstOrDefault(x => x.Text == "MENU" && x.Order == ToolbarItemOrder.Primary);
            if (existingItem != null)
            {
                ToolbarItems.Remove(existingItem);
            }

            // Check if there will be any menu options
            if (!HasMenuOptions())
                return;

            // Add base menu toolbar item
            var menuItem = new ToolbarItem
            {
                Text = "MENU",
                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };
            menuItem.Clicked += async (s, e) => await ShowMenuAsync();
            ToolbarItems.Add(menuItem);
        }

        protected bool HasMenuOptions()
        {
            var options = new List<MenuOption>();

            // If page wants to override the base menu completely, only use page-specific options
            if (OverrideBaseMenu)
            {
                options = GetPageSpecificMenuOptions();
            }
            else
            {
                // Get page-specific menu options first
                var pageOptions = GetPageSpecificMenuOptions();
                options.AddRange(pageOptions);

                // Add common menu options available from every screen
                options.AddRange(GetCommonMenuOptions());
            }

            // Filter out separators and empty options
            var validOptions = options.Where(o => o.Title != "---" && !string.IsNullOrWhiteSpace(o.Title)).ToList();
            return validOptions.Count > 0;
        }

        protected virtual async Task ShowMenuAsync()
        {
            // If page uses custom menu (ViewModel-based), don't show base menu
            if (UseCustomMenu)
                return;

            if (_dialogService == null)
                return;

            var options = new List<MenuOption>();

            // If page wants to override the base menu completely, only use page-specific options
            if (OverrideBaseMenu)
            {
                options = GetPageSpecificMenuOptions();
            }
            else
            {
                // Get page-specific menu options first (they appear first)
                var pageOptions = GetPageSpecificMenuOptions();
                options.AddRange(pageOptions);

                // Add separator if there are page-specific options
                if (pageOptions.Count > 0)
                {
                    // Add common menu options
                    options.Add(new MenuOption("---", async () => { })); // Separator (will be filtered out)
                }

                // Add common menu options available from every screen
                options.AddRange(GetCommonMenuOptions());
            }

            // Filter out separators and empty options
            var validOptions = options.Where(o => o.Title != "---" && !string.IsNullOrWhiteSpace(o.Title)).ToList();

            if (validOptions.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, validOptions.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = validOptions.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        protected virtual List<MenuOption> GetCommonMenuOptions()
        {
            var options = new List<MenuOption>();

            // Advanced Options - always available
            if (_advancedOptionsService != null)
            {
                options.Add(new MenuOption("Advanced Options", async () =>
                {
                    await _advancedOptionsService.ShowAdvancedOptionsAsync();
                }));
            }
            //
            // // Configuration - always available
            // options.Add(new MenuOption("Configuration", async () =>
            // {
            //     await Shell.Current.GoToAsync("configuration");
            // }));
            //
            // // About Laceup Solutions - always available
            // options.Add(new MenuOption("About Laceup Solutions", async () =>
            // {
            //     await ShowAboutAsync();
            // }));

            return options;
        }

        protected virtual async Task ShowAboutAsync()
        {
            if (_dialogService == null)
                return;

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            var uri = new Uri("http://www.laceupsolutions.com");
            await Launcher.OpenAsync(uri);
        }
    }
}

