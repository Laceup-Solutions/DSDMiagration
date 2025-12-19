using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class ConfigurationPage : ContentPage
    {
        private readonly ConfigurationPageViewModel _viewModel;

        public ConfigurationPage(ConfigurationPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Set up back button override - works for both physical back button and navigation bar back button
            BackButtonOverride();
        }

        /// <summary>
        /// Override back button behavior for both physical back button and navigation bar back button.
        /// </summary>
        private void BackButtonOverride()
        {
            var backCommand = new Command(GoBack);

            // Set the back button behavior for this specific page
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = backCommand
            });
        }

        /// <summary>
        /// Handle back navigation logic for both physical and navigation bar back buttons.
        /// </summary>
        private void GoBack()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("configuration");
            
            // Navigate back
            Shell.Current.GoToAsync("..");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // [ACTIVITY STATE]: Save navigation state for this page
            // This allows restoration if the app is force-quit
            Helpers.NavigationHelper.SaveNavigationState("configuration");
            
            await _viewModel.OnAppearingAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Call GoBack which handles both state removal and navigation
            GoBack();
            return true; // Prevent default navigation (GoBack handles it)
        }
    }
}

