using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class TransferOnOffPage : LaceupContentPage, IQueryAttributable
    {
        private readonly TransferOnOffPageViewModel _viewModel;
        private readonly IScannerService _scannerService;

        public TransferOnOffPage(TransferOnOffPageViewModel viewModel, IScannerService scannerService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _scannerService = scannerService;
            BindingContext = _viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TransferOnOffPageViewModel.ScannedLineToFocus) && _viewModel.ScannedLineToFocus is { } line)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Delay(100);
                        LinesCollectionView.ScrollTo(line, position: ScrollToPosition.Center, animate: true);
                    });
                }
            };
        }

        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            return _viewModel.BuildMenuOptions();
        }

        protected override string? GetRouteName() => "transferonoff";

        protected override async void GoBack()
        {
            // Match Xamarin TransferOnOffActivity OnKeyDown(Back): confirm unsaved changes, print mandatory, comment required
            if (await _viewModel.OnBackButtonPressedAsync())
                return; // User cancelled or must complete action; stay on page

            if (_viewModel.ReadOnly && !string.IsNullOrEmpty(_viewModel.GetTempFilePath()))
            {
                var tempFile = _viewModel.GetTempFilePath();
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }
            base.GoBack();
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

            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.InitScanner();
                _scannerService.StartScanning();
                _scannerService.OnDataScanned += OnDecodeData;
                _scannerService.OnDataScannedQR += OnDecodeDataQR;
            }
            
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

            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.OnDataScanned -= OnDecodeData;
                _scannerService.OnDataScannedQR -= OnDecodeDataQR;
                _scannerService.StopScanning();
            }
            
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

        public async void OnDecodeData(object sender, string data)
        {
            if (string.IsNullOrEmpty(data) || _viewModel == null) return;
            _scannerService?.SetScannerWorking();
            try
            {
                await _viewModel.HandleScannedBarcodeAsync(data);
            }
            finally
            {
                _scannerService?.ReleaseScanner();
            }
        }

        public async void OnDecodeDataQR(object sender, BarcodeDecoder decoder)
        {
            if (decoder == null || _viewModel == null) return;
            _scannerService?.SetScannerWorking();
            try
            {
                await _viewModel.HandleScannedQRCodeAsync(decoder);
            }
            finally
            {
                _scannerService?.ReleaseScanner();
            }
        }
    }
}

