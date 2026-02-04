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
        [ObservableProperty] private string _propOrderType = string.Empty;
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
            PropOrderType = GetTransactionType(_order);

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

            var orderType = _order.OrderType;

            if (!await CanCreateOrderAsync(orderType))
                return;

            await CreateOrderAsync(_order);
        }

        private async Task<bool> CanCreateOrderAsync(OrderType orderType)
        {
            if (_client == null)
                return false;

            // Skip unpaid invoice checks for Credit and Return orders (they reduce balance, so allowed)
            // Also skip for NoService (matches Xamarin - they don't go through CanCreateOrder check)
            bool skipUnpaidInvoiceCheck = orderType == OrderType.Credit || 
                                         orderType == OrderType.Return || 
                                         orderType == OrderType.NoService;

            if (!skipUnpaidInvoiceCheck)
            {
                // Check due invoices
                if (Config.CheckDueInvoicesInCreateOrder || Config.CheckDueInvoicesQtyInCreateOrder > 0)
                {
                    var openInvoices = _client.Invoices?.Where(x => x.Balance > 0 && x.DueDate < DateTime.Today).ToList() ?? new List<Invoice>();
                    int count = 0;

                    foreach (var invoice in openInvoices)
                    {
                        var payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) &&
                            x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null);

                        if (payment == null)
                        {
                            count++;
                            continue;
                        }

                        var paid = payment.Components.Sum(x => x.Amount);
                        if (paid < invoice.Balance)
                        {
                            count++;
                        }
                    }

                    if (Config.CheckDueInvoicesQtyInCreateOrder == 0 && count > 0)
                    {
                        await _dialogService.ShowAlertAsync("You must collect payment for due invoices before creating an order.", "Alert");
                        return false;
                    }

                    if (Config.CheckDueInvoicesQtyInCreateOrder > 0 && count >= Config.CheckDueInvoicesQtyInCreateOrder)
                    {
                        await _dialogService.ShowAlertAsync("You must collect payment for due invoices before creating an order.", "Alert");
                        return false;
                    }
                }

                // Check unpaid invoices over 90 days
                if (Config.CannotOrderWithUnpaidInvoices)
                {
                    var unpaidInvoices = _client.Invoices?.Where(x => x.Balance > 0 && x.DueDate.AddDays(90) < DateTime.Now.Date).ToList() ?? new List<Invoice>();
                    int count = 0;

                    foreach (var invoice in unpaidInvoices)
                    {
                        var payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) &&
                            x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null);

                        if (payment == null)
                        {
                            count++;
                            continue;
                        }

                        var paid = payment.Components.Sum(x => x.Amount);
                        if (paid < invoice.Balance)
                        {
                            count++;
                        }
                    }

                    if (count > 0)
                    {
                        await _dialogService.ShowAlertAsync("You cannot create an order until payments are collected for invoices over 90 days past due.", "Alert");
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task CreateOrderAsync(SentOrder sentOrder)
        {
            if (_client == null)
                return;

            var orderType = sentOrder.OrderType;

            // Check credit limit for Order type (not for Credit/Return)
            if (orderType == OrderType.Order && _client.IsOverCreditLimit())
            {
                await _dialogService.ShowAlertAsync("Customer is over credit limit. Cannot create new order.", "Alert");
                return;
            }

            if (_client.OnCreditHold && Config.CustomerInCreditHold)
            {
                await _dialogService.ShowAlertAsync("This client is on Credit Hold, you cannot create an order", "Info");
                return;
            }

            // Handle SalesByDepartment
            if (Config.SalesByDepartment && orderType == OrderType.Order)
            {
                var batch = Batch.List.FirstOrDefault(x => 
                    x.Client != null && 
                    x.Client.ClientId == _client.ClientId && 
                    !x.Orders().Any(o => o.Finished));

                if (batch == null)
                {
                    batch = new Batch(_client);
                    batch.Client = _client;
                    batch.ClockedIn = DateTime.Now;
                    batch.Save();
                }

                await Shell.Current.GoToAsync($"batchdepartment?clientId={_client.ClientId}&batchId={batch.Id}");
                return;
            }

            // Create batch
            var newBatch = new Batch(_client);
            newBatch.Client = _client;
            newBatch.ClockedIn = DateTime.Now;
            newBatch.Save();

            // Handle Consignment
            if (orderType == OrderType.Consignment)
            {
                var consignment = Order.Orders.FirstOrDefault(x => x.OrderType == OrderType.Consignment && x.Client.ClientId == _client.ClientId && x.AsPresale);
                if (consignment == null)
                {
                    consignment = new Order(_client);
                    consignment.BatchId = newBatch.Id;
                    consignment.OrderType = OrderType.Consignment;
                    consignment.AsPresale = true;

                    if (_client.ConsignmentTemplate != null)
                    {
                        foreach (var previous in _client.ConsignmentTemplate)
                        {
                            var detail = new OrderDetail(previous.Product, 0, consignment);
                            consignment.AddDetail(detail);
                            detail.ConsignmentOld = previous.Qty;
                            detail.ConsignmentSet = false;

                            detail.ExpectedPrice = detail.ConsignmentNewPrice;
                            if (Config.ConsignmentKeepPrice)
                            {
                                detail.Price = previous.Price;
                                detail.ConsignmentNewPrice = detail.Price;
                            }
                            else
                            {
                                detail.Price = Product.GetPriceForProduct(detail.Product, consignment.Client, true);
                                detail.ConsignmentNewPrice = Product.GetPriceForProduct(detail.Product, consignment.Client, true);
                            }
                        }
                    }

                    if (Config.ConsignmentBeta)
                        consignment.ExtraFields = UDFHelper.SyncSingleUDF("cosignmentOrder", "1", consignment.ExtraFields);

                    if (Config.UseFullConsignment)
                    {
                        consignment.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentCount", "1", consignment.ExtraFields);
                        consignment.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentSet", "1", consignment.ExtraFields);
                    }

                    if (Config.ParInConsignment || Config.ConsignmentBeta)
                        consignment.AddParInConsignment();

                    consignment.Save();
                }
                else
                {
                    newBatch.Delete();
                }

                await Shell.Current.GoToAsync($"consignment?orderId={consignment.OrderId}");
                return;
            }

            // Handle NoService
            if (orderType == OrderType.NoService)
            {
                // Navigate to NoService page - it will handle creation
                await Shell.Current.GoToAsync($"noservice?clientId={_client.ClientId}&batchId={newBatch.Id}");
                return;
            }

            // Create order based on type
            Order? order = null;

            switch (orderType)
            {
                case OrderType.Order:
                    order = new Order(_client) { OrderType = OrderType.Order };
                    // Add details
                    foreach (var det in sentOrder.Details)
                    {
                        var p = det.GetProduct;
                        if (p.IsDiscountItem)
                            continue;

                        var detail = new OrderDetail(det.GetProduct, det.Qty, order);
                        detail.Price = det.Price;
                        detail.ExpectedPrice = det.Price;
                        detail.IsCredit = det.IsCredit;
                        detail.Damaged = det.Damaged;
                        detail.Discount = det.Discount;
                        detail.DiscountType = det.DiscountType;
                        detail.UnitOfMeasure = det.UoM;
                        detail.Comments = det.Comments;

                        CheckForOfferAndFreeItem(order, det, detail);
                    }

                    order.DiscountAmount = sentOrder.DiscountAmount;
                    order.DiscountType = sentOrder.DiscountType;
                    order.Freight = sentOrder.Freight;
                    order.OtherCharges = sentOrder.OtherCharges;
                    order.FreightType = sentOrder.FreightType;
                    order.OtherChargesType = sentOrder.OtherChargesType;
                    order.OtherChargesComment = sentOrder.OtherChargesComment;
                    order.FreightComment = sentOrder.FreightComment;
                    order.AsPresale = sentOrder.AsPresale;

                    order.Save();
                    Logger.CreateLog("1 Created order id" + order.OrderId);
                    break;

                case OrderType.Credit:
                    order = new Order(_client) { OrderType = OrderType.Credit };
                    // Add details
                    foreach (var det in sentOrder.Details)
                    {
                        var p = det.GetProduct;
                        if (p.IsDiscountItem)
                            continue;

                        var detail = new OrderDetail(det.GetProduct, det.Qty, order);
                        detail.Price = det.Price;
                        detail.ExpectedPrice = det.Price;
                        detail.IsCredit = det.IsCredit;
                        detail.Damaged = det.Damaged;
                        detail.Discount = det.Discount;
                        detail.DiscountType = det.DiscountType;
                        detail.UnitOfMeasure = det.UoM;
                        detail.Comments = det.Comments;

                        CheckForOfferAndFreeItem(order, det, detail);
                    }

                    order.DiscountAmount = sentOrder.DiscountAmount;
                    order.DiscountType = sentOrder.DiscountType;
                    order.Freight = sentOrder.Freight;
                    order.OtherCharges = sentOrder.OtherCharges;
                    order.FreightType = sentOrder.FreightType;
                    order.OtherChargesType = sentOrder.OtherChargesType;
                    order.OtherChargesComment = sentOrder.OtherChargesComment;
                    order.FreightComment = sentOrder.FreightComment;
                    order.AsPresale = sentOrder.AsPresale;

                    order.Save();
                    Logger.CreateLog("2 Created order id" + order.OrderId);
                    break;

                case OrderType.Quote:
                    order = new Order(_client) { OrderType = OrderType.Order, IsQuote = true };
                    // Add details
                    foreach (var det in sentOrder.Details)
                    {
                        var detail = new OrderDetail(det.GetProduct, det.Qty, order);
                        detail.Price = det.Price;
                        detail.ExpectedPrice = det.Price;
                        detail.IsCredit = det.IsCredit;
                        detail.Damaged = det.Damaged;
                        detail.Discount = det.Discount;
                        detail.DiscountType = det.DiscountType;
                        detail.UnitOfMeasure = det.UoM;
                        detail.Comments = det.Comments;

                        CheckForOfferAndFreeItem(order, det, detail);
                    }

                    order.Save();
                    Logger.CreateLog("1 Created order quote id" + order.OrderId);
                    break;
            }

            if (order != null)
            {
                order.BatchId = newBatch.Id;
                CompanyInfo.AssignCompanyToOrder(order);

                order.SalesmanId = Config.SalesmanId;
                order.Save();

                // Navigate to order details page
                await NavigateToOrderAsync(newBatch, order);
            }
        }

        private void CheckForOfferAndFreeItem(Order order, SentOrderDetail det, OrderDetail detail)
        {
            if (!detail.IsCredit)
            {
                var myoffer = new Offer();
                var hasOffer = myoffer.ProductHasOffer(det.GetProduct);

                var priceToCompare = Product.GetPriceForProduct(detail.Product, order.Client, true);
                if (det.Price != priceToCompare && det.Price != 0)
                {
                    if (hasOffer != null)
                    {
                        detail.FromOffer = true;
                        order.AddDetail(detail);
                    }
                    else
                    {
                        detail.ExpectedPrice = det.Price;
                        order.AddDetail(detail);
                    }
                }
                else
                {
                    if (det.Price == 0)
                    {
                        if (hasOffer != null)
                        {
                            detail.FromOffer = true;
                            order.AddDetail(detail);
                        }
                        else
                        {
                            detail.IsFreeItem = true;
                            order.AddDetail(detail);
                        }
                    }
                    else
                    {
                        order.AddDetail(detail);
                    }
                }
            }
            else
            {
                order.AddDetail(detail);
            }
        }

        private async Task NavigateToOrderAsync(Batch batch, Order order)
        {
            if (_client == null)
                return;

            // Handle NoService orders
            if (order.OrderType == OrderType.NoService)
            {
                if (Config.CaptureImages)
                {
                    await Shell.Current.GoToAsync($"noservice?orderId={order.OrderId}");
                }
                return;
            }

            // Handle Consignment orders
            if (order.OrderType == OrderType.Consignment)
            {
                await Shell.Current.GoToAsync($"consignment?orderId={order.OrderId}");
                return;
            }

            // Handle Quote orders
            if (order.OrderType == OrderType.Quote || order.IsQuote)
            {
                // TODO: Navigate to QuotePage when implemented
                await _dialogService.ShowAlertAsync("Quote viewing is not yet implemented in MAUI version.", "Info");
                return;
            }

            // Handle finished orders (non-presale) - navigate to BatchPage
            if (!order.AsPresale || order.Finished)
            {
                await Shell.Current.GoToAsync($"batch?batchId={batch.Id}");
                return;
            }

            // Handle Credit or Return orders
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                if (Config.UseLaceupAdvancedCatalog)
                {
                    await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
                }
                else
                    await Shell.Current.GoToAsync($"ordercredit?orderId={order.OrderId}&asPresale=0&fromOneDoc=0");
                return;
            }

            // Handle Full Template logic
            if (Config.UseFullTemplateForClient(_client) && !_client.AllowOneDoc)
            {
                order.RelationUniqueId = Guid.NewGuid().ToString("N");

                var credit = new Order(_client) { OrderType = OrderType.Credit };
                credit.BatchId = batch.Id;
                credit.RelationUniqueId = order.RelationUniqueId;
                CompanyInfo.AssignCompanyToOrder(credit);
                credit.Save();

                await Shell.Current.GoToAsync($"superordertemplate?asPresale=1&orderId={order.OrderId}&creditId={credit.OrderId}");
                return;
            }

            // Handle SalesByDepartment
            if (Config.SalesByDepartment)
            {
                await Shell.Current.GoToAsync($"batchdepartment?clientId={_client.ClientId}&batchId={batch.Id}");
                return;
            }

            // Default navigation
            if (Config.UseLaceupAdvancedCatalog)
            {
                await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
            }
            else
                await Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={order.OrderId}&asPresale=1");
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
                        ZipMethods.ZipFile(dstFile, dstFileZipped);
                        DataProvider.SendTheOrders(dstFileZipped);
                        System.IO.File.Delete(dstFileZipped);

                        var signatureFileZipped = dstFile + ".signature.zip";
                        if (System.IO.File.Exists(signatureFileZipped))
                        {
                            DataProvider.SendTheSignatures(signatureFileZipped);
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

