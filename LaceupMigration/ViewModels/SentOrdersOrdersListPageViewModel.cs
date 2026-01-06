using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SentOrdersOrdersListPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private SentOrder? _order;
        private Client? _client;
        private string _packagePath = string.Empty;

        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _orderType = string.Empty;
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private string _shipDateText = string.Empty;
        [ObservableProperty] private bool _showShipDate;
        [ObservableProperty] private bool _showExtraLayout;
        [ObservableProperty] private string _linesText = string.Empty;
        [ObservableProperty] private string _qtySoldText = string.Empty;
        [ObservableProperty] private string _subTotalText = string.Empty;
        [ObservableProperty] private string _discountText = string.Empty;
        [ObservableProperty] private string _taxText = string.Empty;
        [ObservableProperty] private string _extraTotal1Text = string.Empty;
        [ObservableProperty] private string _extraTotal2Text = string.Empty;
        [ObservableProperty] private string _extraTotal3Text = string.Empty;
        [ObservableProperty] private ObservableCollection<SentOrderDetailViewModel> _orderDetails = new();
        [ObservableProperty] private bool _showTotal;

        public SentOrdersOrdersListPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotal = !Config.HidePriceInTransaction;
            ShowExtraLayout = Config.AllowOtherCharges;
        }

        public async Task InitializeAsync(string packagePath, int orderId)
        {
            try
            {
                _packagePath = packagePath;
                
                if (string.IsNullOrEmpty(packagePath))
                {
                    await _dialogService.ShowAlertAsync("Package path is empty.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                
                // First, try to load the package directly if the file exists
                SentOrderPackage package = null;
                if (System.IO.File.Exists(packagePath))
                {
                    try
                    {
                        package = new SentOrderPackage { PackagePath = packagePath };
                        var testOrders = package.PackageOrders().ToList();
                        if (testOrders.Any(x => x.OrderId == orderId))
                        {
                            // Package found and contains the order
                        }
                        else
                        {
                            package = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog($"Error loading package directly: {ex.Message}");
                        package = null;
                    }
                }
                
                // If direct load failed, try searching packages from the date range
                if (package == null)
                {
                    var selectedDate = DateTime.Now.AddDays(-Config.DaysToShowSentOrders);
                    var sentPackages = SentOrderPackage.Packages(selectedDate);
                    package = sentPackages.FirstOrDefault(x => 
                        string.Equals(x.PackagePath, packagePath, StringComparison.InvariantCulture) ||
                        string.Equals(System.IO.Path.GetFileName(x.PackagePath), System.IO.Path.GetFileName(packagePath), StringComparison.InvariantCulture));
                }
                
                if (package == null)
                {
                    Logger.CreateLog($"Package not found. Path: {packagePath}");
                    await _dialogService.ShowAlertAsync("Package not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                var orders = package.PackageOrders().ToList();
                _order = orders.FirstOrDefault(x => x.OrderId == orderId);

                if (_order == null)
                {
                    // await _dialogService.ShowAlertAsync("Order not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                var clientId = _order.ClientId;
                _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);

                if (_client == null)
                {
                    await _dialogService.ShowAlertAsync("Client not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                _client.EnsureInvoicesAreLoaded();

                UpdateUI();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading order: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private void UpdateUI()
        {
            if (_order == null || _client == null)
                return;

            ClientName = _order.ClientName;
            OrderType = GetTransactionType(_order);

            if (_order.ShipDate != DateTime.MinValue)
            {
                ShipDateText = $"Ship Date: {_order.ShipDate.ToShortDateString()}";
                ShowShipDate = true;
            }
            else
            {
                ShowShipDate = false;
            }

            double discount = 0;
            foreach (var d in _order.Details)
            {
                if (d.GetProduct.IsDiscountItem)
                    continue;

                if (d.Discount > 0 || d.IsCredit)
                    continue;

                bool cameFromOffer = false;
                var price = Product.CalculatePriceForProduct(d.GetProduct, _client, d.IsCredit, d.Damaged, d.UoM, false, out cameFromOffer, true, null);

                var total = price * d.Qty;
                var dc = (total - (d.Qty * d.Price));
                if (dc > 0)
                    discount += dc;
            }

            if (Config.AllowOtherCharges)
            {
                LinesText = $"Lines: {_order.Details.Count}";
                QtySoldText = $"Qty: {_order.Details.Sum(x => x.Qty)}";
                SubTotalText = $"Subtotal: {(discount + _order.CalculateItemCost()).ToCustomString()}";
                TaxText = $"Tax: {_order.CalculateTax().ToCustomString()}";
                DiscountText = $"O.Charges: {_order.CalculatedOtherCharges().ToCustomString()}";
                TotalText = $"Freight: {_order.CalculatedFreight().ToCustomString()}";
                ExtraTotal2Text = $"Discount: {_order.CalculateDiscount().ToCustomString()}";
                ExtraTotal3Text = $"Total: {_order.OrderTotalCost().ToCustomString()}";
            }
            else
            {
                TotalText = $"Total: {_order.OrderTotalCost().ToCustomString()}";
                LinesText = $"Lines: {_order.Details.Count}";
                QtySoldText = $"Qty: {_order.Details.Sum(x => x.Qty)}";
                SubTotalText = $"Subtotal: {(discount + _order.CalculateItemCost()).ToCustomString()}";
                DiscountText = $"Discount: {(discount + _order.CalculateDiscount()).ToCustomString()}";
                TaxText = $"Tax: {_order.CalculateTax().ToCustomString()}";
            }

            OrderDetails.Clear();
            foreach (var detail in _order.Details)
            {
                OrderDetails.Add(new SentOrderDetailViewModel(detail, _order));
            }
        }

        private string GetTransactionType(SentOrder order)
        {
            switch (order.OrderType)
            {
                case LaceupMigration.OrderType.Order:
                    switch (order.TransactionType)
                    {
                        case TransactionType.SalesOrder:
                            return "Sales Order";
                        case TransactionType.SalesInvoice:
                            return "Sales Invoice";
                        default:
                            return "Transaction";
                    }
                case LaceupMigration.OrderType.Credit:
                    switch (order.TransactionType)
                    {
                        case TransactionType.CreditOrder:
                            return "Credit Order";
                        case TransactionType.CreditInvoice:
                            return "Credit Invoice";
                        default:
                            return "Transaction";
                    }
                case LaceupMigration.OrderType.Quote:
                    return "Quote";
                case LaceupMigration.OrderType.Consignment:
                    return "Consignment";
                case LaceupMigration.OrderType.Sample:
                    return "Sample";
                case LaceupMigration.OrderType.Return:
                    return "Return";
                case LaceupMigration.OrderType.NoService:
                    return "No Service";
                default:
                    return "Transaction";
            }
        }

        [RelayCommand]
        private async Task Duplicate()
        {
            if (_order == null || _client == null)
                return;

            // TODO: Implement duplicate order creation logic
            // This should create a new order from the sent order details
            await _dialogService.ShowAlertAsync("Duplicate functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Resend()
        {
            if (_order == null)
                return;

            var confirmed = await _dialogService.ShowConfirmationAsync("Continue sending orders?", "Warning", "Yes", "No");
            if (!confirmed)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Sending orders...");
                
                await Task.Run(() =>
                {
                    try
                    {
                        var dstFile = _order.PackagePath;
                        if (string.IsNullOrEmpty(dstFile) || !System.IO.File.Exists(dstFile))
                        {
                            throw new Exception("Package file not found.");
                        }

                        string dstFileZipped = dstFile + ".zip";
                        DataAccess.ZipFile(dstFile, dstFileZipped);
                        DataAccessEx.SendTheOrders(dstFileZipped);
                        System.IO.File.Delete(dstFileZipped);

                        var signatureFileZipped = dstFile + ".signature.zip";
                        if (System.IO.File.Exists(signatureFileZipped))
                        {
                            DataAccessEx.SendTheSignatures(signatureFileZipped);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        throw;
                    }
                });

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Orders sent successfully.", "Info", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync($"Error resending order: {ex.Message}", "Alert", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public partial class SentOrderDetailViewModel : ObservableObject
    {
        private readonly SentOrderDetail _detail;
        private readonly SentOrder _order;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _qtyText = string.Empty;
        [ObservableProperty] private string _priceText = string.Empty;
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private string _comments = string.Empty;
        [ObservableProperty] private bool _isDiscountItem;
        [ObservableProperty] private bool _showPrice;
        [ObservableProperty] private bool _showComments;
        [ObservableProperty] private Color _backgroundColor = Colors.White;

        public SentOrderDetailViewModel(SentOrderDetail detail, SentOrder order)
        {
            _detail = detail;
            _order = order;
            ShowPrice = !Config.HidePriceInTransaction;

            var product = detail.GetProduct;
            ProductName = product != null ? product.Name : "Product Not Found";
            QtyText = $"Qty: {detail.Qty}";
            
            var priceTxt = order.CalculateOneItemTotalCost(detail);
            PriceText = $"Price: {(priceTxt / detail.Qty).ToCustomString()}";
            TotalText = $"Total: {priceTxt.ToCustomString()}";
            Comments = detail.Comments ?? string.Empty;
            ShowComments = !string.IsNullOrEmpty(Comments);
            IsDiscountItem = product?.IsDiscountItem ?? false;
            BackgroundColor = IsDiscountItem ? Colors.LightGreen : Colors.White;
        }
    }
}

