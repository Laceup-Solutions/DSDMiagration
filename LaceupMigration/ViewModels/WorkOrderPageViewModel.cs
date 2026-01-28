using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class WorkOrderPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly IScannerService _scannerService;
        private Order? _order;
        private Client? _client;
        private Asset? _selectedAsset;
        private bool _initialized;
        private bool _asPresale;
        private List<Asset> _availableAssets = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _selectedAssetText = string.Empty;

        [ObservableProperty]
        private bool _showSelectedAsset;

        [ObservableProperty]
        private bool _showNoAssetMessage = true;

        [ObservableProperty]
        private bool _canScan = true;

        public WorkOrderPageViewModel(DialogService dialogService, ILaceupAppService appService, IScannerService scannerService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _scannerService = scannerService;
        }

        public async Task InitializeAsync(int clientId, int orderId, bool asPresale)
        {
            if (_initialized && _order?.OrderId == orderId)
            {
                await RefreshAsync();
                return;
            }

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            
            if (_client == null || _order == null)
            {
                await _dialogService.ShowAlertAsync("Client or order not found.", "Error");
                return;
            }

            _asPresale = asPresale;
            _initialized = true;

            LoadClientAssets();
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            LoadOrderData();
            await Task.CompletedTask;
        }

        private void LoadClientAssets()
        {
            var clientAssetTracks = ClientAssetTrack.List
                .Where(x => x.ClientId == _client.ClientId && x.Active)
                .Select(x => x.AssetId)
                .ToList();

            _availableAssets = Asset.List
                .Where(x => clientAssetTracks.Contains(x.Id))
                .ToList();
        }

        private async void LoadOrderData()
        {
            if (_order == null || _client == null)
                return;

            ClientName = _client.ClientName;

            var assetSerialNumber = UDFHelper.GetSingleUDF("workOrderAsset", _order.ExtraFields);
            if (!string.IsNullOrEmpty(assetSerialNumber))
            {
                _selectedAsset = Asset.Find(assetSerialNumber);
                if (_selectedAsset != null)
                {
                    var product = Product.Find(_selectedAsset.ProductId);
                    SelectedAssetText = $"Selected: {product?.Name ?? "Unknown"} - Part Number: {_selectedAsset.SerialNumber}";
                    ShowSelectedAsset = true;
                    ShowNoAssetMessage = false;

                    // Navigate to order details
                    await NavigateToOrderDetailsAsync();
                }
            }
            else
            {
                ShowSelectedAsset = false;
                ShowNoAssetMessage = true;
            }

            CanScan = !_order.Locked() && _availableAssets.Count > 0;
        }

        [RelayCommand]
        private async Task SelectAssetAsync()
        {
            if (_availableAssets.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No assets available for this client.", "Info");
                return;
            }

            var assetOptions = _availableAssets.Select(a =>
            {
                var product = Product.Find(a.ProductId);
                return $"{product?.Name ?? "Unknown"} - {a.SerialNumber}";
            }).ToArray();

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Asset", assetOptions);
            if (selectedIndex >= 0 && selectedIndex < _availableAssets.Count)
            {
                var selected = _availableAssets[selectedIndex];
                await ProductSelectedAsync(selected.SerialNumber);
            }
        }

        [RelayCommand]
        private async Task ScanAssetAsync()
        {
            // if (_order == null || _order.Locked())
            // {
            //     await _dialogService.ShowAlertAsync("Order cannot be edited.", "Alert");
            //     return;
            // }
            //
            // try
            // {
            //     var scanResult = await _scannerService.ScanAsync();
            //     if (string.IsNullOrEmpty(scanResult))
            //         return;
            //
            //     // Find scanned products
            //     var products = FindScannedProducts(scanResult);
            //     var asset = _availableAssets.FirstOrDefault(x => 
            //         products.Any(y => y.ProductId == x.ProductId));
            //
            //     if (asset != null)
            //     {
            //         await ProductSelectedAsync(asset.SerialNumber);
            //     }
            //     else
            //     {
            //         await _dialogService.ShowAlertAsync("This client does not have an asset for the scanned product.", "Alert");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Logger.CreateLog($"Error scanning: {ex.Message}");
            //     await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error");
            // }
        }

        private async Task ProductSelectedAsync(string serialNumber)
        {
            if (_order == null)
                return;

            var asset = Asset.Find(serialNumber);
            if (asset == null)
            {
                await _dialogService.ShowAlertAsync("Asset not found.", "Error");
                return;
            }

            _order.ExtraFields = UDFHelper.SyncSingleUDF("workOrderAsset", serialNumber, _order.ExtraFields);

            var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
            if (batch == null)
            {
                batch = new Batch(_order.Client);
                batch.Client = _order.Client;
                batch.ClockedIn = DateTime.Now;
                batch.Save();
                _order.BatchId = batch.Id;
            }

            _order.AssetId = asset.Id;
            _order.Save();

            await NavigateToOrderDetailsAsync();
        }

        private async Task NavigateToOrderDetailsAsync()
        {
            if (_order == null)
                return;

            // Navigate to appropriate order detail page
            if (_order.OrderType == OrderType.Credit || _order.OrderType == OrderType.Return)
            {
                // Navigate to advancedcatalog, previouslyorderedtemplate, or orderdetails
                if (Config.UseLaceupAdvancedCatalog)
                {
                    await Shell.Current.GoToAsync($"advancedcatalog?orderId={_order.OrderId}");
                }
                else if (Config.UseCatalog)
                {
                    await Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"orderdetails?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}");
                }
                return;
            }
            else if (Config.UseFullTemplateForClient(_order.Client) && !_order.Client.AllowOneDoc)
            {
                // Create credit order if needed
                var credit = new Order(_order.Client) { OrderType = OrderType.Credit };
                credit.BatchId = _order.BatchId;
                credit.AsPresale = true;
                credit.RelationUniqueId = Guid.NewGuid().ToString("N");
                _order.RelationUniqueId = credit.RelationUniqueId;
                CompanyInfo.AssignCompanyToOrder(credit);

                credit.Save();
                _order.Save();

                Shell.Current.GoToAsync($"superordertemplate?orderId={_order.OrderId}&creditId={credit.OrderId}&asPresale={(_asPresale ? 1 : 0)}");
            }
            else
            {
                // Navigate based on config
                if (Config.UseLaceupAdvancedCatalog)
                {
                    Shell.Current.GoToAsync($"advancedcatalog?orderId={_order.OrderId}");
                }
                else if (Config.UseCatalog)
                {
                    Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}");
                }
                else
                {
                    Shell.Current.GoToAsync($"orderdetails?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}");
                }
            }
        }

        private List<Product> FindScannedProducts(string barcode)
        {
            // var products = new List<Product>();
            //
            // // Try to find product by barcode
            // var product = Product.Products.FirstOrDefault(p => 
            //     p.Barcode == barcode || 
            //     p.Barcode2 == barcode ||
            //     p.Barcode3 == barcode);
            //
            // if (product != null)
            // {
            //     products.Add(product);
            // }
            //
            // return products;

            return new List<Product>();
        }
    }
}

