using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class ProductCatalogPage : IQueryAttributable
    {
        private readonly ProductCatalogPageViewModel _viewModel;
        private readonly IScannerService _scannerService;

        public ProductCatalogPage(ProductCatalogPageViewModel viewModel, IScannerService scannerService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _scannerService = scannerService;
            BindingContext = _viewModel;
            // Prevent LaceupContentPage from adding its own menu - we use the ViewModel's menu instead
            UseCustomMenu = true;
        }

        private void UpdateToolbar()
        {
            ToolbarItems.Clear();
            if (_viewModel.IsFromSelfService && _viewModel.SelfServiceOrderId is int orderId)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Checkout",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(async () => await Shell.Current.GoToAsync($"//selfservice/checkout?orderId={orderId}"))
                });
            }
            else if (_viewModel.IsFromLoadOrder)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Add To Order",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(() => _ = _viewModel.AddItemsCommand.ExecuteAsync(null))
                });
            }
            else if (_viewModel.ShowDoneButton)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Done",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(() => _ = _viewModel.DoneCommand.ExecuteAsync(null))
                });
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Menu",
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0,
                    Command = new Command(() => _ = _viewModel.ShowMenuCommand.ExecuteAsync(null))
                });
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
            
            // [ACTIVITY STATE]: Save navigation state with query parameters
            // Build route with query parameters for state saving
            var route = "productcatalog";
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
            
            UpdateToolbar();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();

            if (_scannerService != null && Config.ScannerToUse != 4)
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
            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.OnDataScanned -= OnDecodeData;
                _scannerService.OnDataScannedQR -= OnDecodeDataQR;
                _scannerService.StopScanning();
            }
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

        protected override string? GetRouteName() => "productcatalog";

        private async void OnCellTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is CatalogItemViewModel item)
            {
                await _viewModel.NavigateToAddItemAsync(item);
            }
        }
    }
}

