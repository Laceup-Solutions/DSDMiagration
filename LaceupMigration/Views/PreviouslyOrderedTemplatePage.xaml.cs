using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class PreviouslyOrderedTemplatePage : LaceupContentPage, IQueryAttributable
    {
        private readonly PreviouslyOrderedTemplatePageViewModel _viewModel;
        private readonly IScannerService _scannerService;

        public PreviouslyOrderedTemplatePage(PreviouslyOrderedTemplatePageViewModel viewModel, IScannerService scannerService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _scannerService = scannerService;
            BindingContext = _viewModel;
            
            // Subscribe to property changes to update column definitions
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Initialize column definitions
            UpdateColumnDefinitions();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActionButtonsColumnDefinitions" || 
                e.PropertyName == "ShowAddCredit")
            {
                UpdateColumnDefinitions();
            }
        }

        private void UpdateColumnDefinitions()
        {
            if (ActionButtonsGrid == null || _viewModel == null)
                return;

            // Parse the column definitions string and apply it
            var columnDefs = _viewModel.ActionButtonsColumnDefinitions;
            var columns = columnDefs.Split(',');
            
            ActionButtonsGrid.ColumnDefinitions.Clear();
            foreach (var col in columns)
            {
                var colDef = new ColumnDefinition();
                if (col.Trim() == "*")
                {
                    colDef.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    if (double.TryParse(col.Trim(), out var value))
                    {
                        colDef.Width = new GridLength(value);
                    }
                    else
                    {
                        colDef.Width = new GridLength(1, GridUnitType.Star);
                    }
                }
                ActionButtonsGrid.ColumnDefinitions.Add(colDef);
            }

            // Update button column positions based on ShowAddCredit
            if (!_viewModel.ShowAddCredit)
            {
                // Shift buttons left: Prod->0, Cats->1, Search->2, Send->3
                if (ProdButton != null) ProdButton.SetValue(Grid.ColumnProperty, 0);
                if (CatsButton != null) CatsButton.SetValue(Grid.ColumnProperty, 1);
                if (SearchButton != null) SearchButton.SetValue(Grid.ColumnProperty, 2);
                if (SendButton != null) SendButton.SetValue(Grid.ColumnProperty, 3);
            }
            else
            {
                // Normal positions: Add Credit->0, Prod->1, Cats->2, Search->3, Send->4
                if (ProdButton != null) ProdButton.SetValue(Grid.ColumnProperty, 1);
                if (CatsButton != null) CatsButton.SetValue(Grid.ColumnProperty, 2);
                if (SearchButton != null) SearchButton.SetValue(Grid.ColumnProperty, 3);
                if (SendButton != null) SendButton.SetValue(Grid.ColumnProperty, 4);
            }
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
            
            // Update column definitions after ViewModel is fully initialized
            UpdateColumnDefinitions();
            
            // Update menu toolbar item after order is loaded (important for state restoration)
            // This ensures the menu appears even when loading from state
            UpdateMenuToolbarItem();
            
            // Initialize and start scanner (similar to ScannerActivity.OnStart)
            if (_scannerService != null && Config.ScannerToUse != 4) // 4 is camera scanner
            {
                _scannerService.InitScanner();
                _scannerService.StartScanning();
                _scannerService.OnDataScanned += OnDecodeData;
                _scannerService.OnDataScannedQR += OnDecodeDataQR;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Stop scanner (similar to ScannerActivity.OnPause)
            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.OnDataScanned -= OnDecodeData;
                _scannerService.OnDataScannedQR -= OnDecodeDataQR;
                _scannerService.StopScanning();
            }
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

        private async void OnCellTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is PreviouslyOrderedProductViewModel item)
            {
                await _viewModel.NavigateToAddItemAsync(item);
            }
        }

        /// <summary>
        /// Handles barcode scan data (non-QR codes)
        /// Similar to ScannerActivity.OnDecodeData
        /// </summary>
        public async void OnDecodeData(object sender, string data)
        {
            if (string.IsNullOrEmpty(data) || _viewModel == null)
                return;

            // Set scanner as working to prevent duplicate scans
            _scannerService?.SetScannerWorking();

            try
            {
                await _viewModel.HandleScannedBarcodeAsync(data);
            }
            finally
            {
                // Release scanner after processing
                _scannerService?.ReleaseScanner();
            }
        }

        /// <summary>
        /// Handles QR code scan data
        /// Similar to ScannerActivity.OnDecodeDataQR
        /// </summary>
        public async void OnDecodeDataQR(object sender, BarcodeDecoder decoder)
        {
            if (decoder == null || _viewModel == null)
                return;

            // Set scanner as working to prevent duplicate scans
            _scannerService?.SetScannerWorking();

            try
            {
                await _viewModel.HandleScannedQRCodeAsync(decoder);
            }
            finally
            {
                // Release scanner after processing
                _scannerService?.ReleaseScanner();
            }
        }
    }
}

