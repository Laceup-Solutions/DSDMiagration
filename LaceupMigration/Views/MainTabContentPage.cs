using LaceupMigration.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration
{
    /// <summary>
    /// Base page for main tab content (Customers, Open Inv, Transactions, Payments).
    /// Intercepts the Android physical back button and shows an exit confirmation instead of exiting immediately.
    /// </summary>
    public abstract class MainTabContentPage : ContentPage
    {
        private DialogService? _dialogService;

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler?.MauiContext != null)
                _dialogService = Handler.MauiContext.Services.GetService<DialogService>();
        }

        /// <summary>
        /// When user presses the physical back button on main tabs, show exit confirmation instead of exiting immediately.
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            ShowExitConfirmation();
            return true; // We handle the back; prevent app from exiting until user confirms
        }

        private async void ShowExitConfirmation()
        {
            var dialog = _dialogService ?? Handler?.MauiContext?.Services.GetService<DialogService>();
            if (dialog == null)
            {
                Application.Current?.Quit();
                return;
            }

            var exit = await dialog.ShowConfirmationAsync(
                "Exit",
                "Do you want to exit the app?",
                "Exit",
                "Cancel");

            if (exit)
                Application.Current?.Quit();
        }
    }
}
