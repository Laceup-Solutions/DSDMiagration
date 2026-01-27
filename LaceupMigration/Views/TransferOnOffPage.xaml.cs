using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class TransferOnOffPage : IQueryAttributable
    {
        private readonly TransferOnOffPageViewModel _viewModel;

        public TransferOnOffPage(TransferOnOffPageViewModel viewModel)
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
            // If transfer was saved (ReadOnly = true), ensure temp file is deleted
            // This prevents ExistPendingTransfer from being true after saving and going back
            if (_viewModel.ReadOnly && !string.IsNullOrEmpty(_viewModel.GetTempFilePath()))
            {
                var tempFile = _viewModel.GetTempFilePath();
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
            
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("transferonoff");
            
            // Navigate back
            Shell.Current.GoToAsync("..");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("action", out var value) && value != null)
            {
                var action = value.ToString() ?? "transferOn";
                Dispatcher.Dispatch(async () => 
                {
                    await _viewModel.InitializeAsync(action);
                    // After initialization, save temp file path to ActivityState
                    SaveTempFilePathToState();
                });
            }
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "transferonoff";
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
            
            // [ACTIVITY STATE]: Save temp file periodically to preserve progress
            // Match Xamarin TransferActivity: saves state on OnResume/OnPause
            // Only save if ViewModel has been initialized (temp file path is set) and transfer hasn't been saved yet
            if (!string.IsNullOrEmpty(_viewModel.GetTempFilePath()) && !_viewModel.ReadOnly)
            {
                _viewModel.SaveList();
                
                // Update ActivityState with current temp file path
                SaveTempFilePathToState();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // [ACTIVITY STATE]: Save temp file when leaving page to preserve progress
            // Match Xamarin TransferActivity: saves state on OnPause
            // Only save if ViewModel has been initialized (temp file path is set) and transfer hasn't been saved yet
            // If transfer was saved (ReadOnly = true), the temp file should have been deleted and shouldn't be recreated
            if (!string.IsNullOrEmpty(_viewModel.GetTempFilePath()) && !_viewModel.ReadOnly)
            {
                _viewModel.SaveList();
                
                // Update ActivityState with current temp file path
                SaveTempFilePathToState();
            }
        }

        /// <summary>
        /// Saves the temp file path to ActivityState.State to preserve progress across app restarts.
        /// Match Xamarin TransferActivity: saves temp file path in ActivityState.State["tempFilePath"]
        /// </summary>
        private void SaveTempFilePathToState()
        {
            var state = LaceupMigration.ActivityState.GetState("TransferOnOffActivity");
            if (state != null && state.State != null)
            {
                var tempFilePath = _viewModel.GetTempFilePath();
                if (!string.IsNullOrEmpty(tempFilePath))
                {
                    state.State["tempFilePath"] = tempFilePath;
                    LaceupMigration.ActivityState.Save();
                }
            }
        }

        private async void SortButton_Tapped(object sender, EventArgs e)
        {
            await _viewModel.ShowSortDialogAsync();
        }
    }
}

