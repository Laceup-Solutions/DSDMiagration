using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class BatchPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Batch? _batch;
        private bool _initialized;

        public ObservableCollection<BatchOrderViewModel> Orders { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _clockInText = string.Empty;

        [ObservableProperty]
        private string _clockOutText = string.Empty;

        [ObservableProperty]
        private bool _showClockOut;

        [ObservableProperty]
        private string _amountText = "Amount: $0.00";

        [ObservableProperty]
        private string _qtyOrderedText = "Qty Ordered: 0";

        [ObservableProperty]
        private string _qtyReturnedText = "Qty Returned: 0";

        [ObservableProperty]
        private string _qtyDumpedText = "Qty Dumped: 0";

        [ObservableProperty]
        private bool _showTotals = true;

        [ObservableProperty]
        private bool _canPick;

        [ObservableProperty]
        private bool _canDex;

        [ObservableProperty]
        private bool _canFinalize;

        [ObservableProperty]
        private bool _canVoid;

        [ObservableProperty]
        private bool _canReship;

        [ObservableProperty]
        private bool _canClockOut = true;

        [ObservableProperty]
        private bool _canPrintLabel;

        [ObservableProperty]
        private bool _showPickButton = true;

        [ObservableProperty]
        private bool _showDexButton = true;

        [ObservableProperty]
        private bool _showReshipButton;

        [ObservableProperty]
        private bool _showPrintLabelButton;


        public async Task InitializeAsync(int batchId)
        {
            if (_initialized && _batch?.Id == batchId)
            {
                await RefreshAsync();
                return;
            }

            _batch = Batch.List.FirstOrDefault(x => x.Id == batchId);
            if (_batch == null)
            {
                await _dialogService.ShowAlertAsync("Batch not found.", "Error");
                return;
            }

            _initialized = true;
            LoadBatchData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            LoadBatchData();
            await Task.CompletedTask;
        }

        private void LoadBatchData()
        {
            if (_batch == null)
                return;

            ClientName = _batch.Client?.ClientName ?? "Unknown Client";
            ClockInText = $"Clock In: {_batch.ClockedIn:g}";

            if (_batch.ClockedOut != DateTime.MinValue)
            {
                ClockOutText = $"Clock Out: {_batch.ClockedOut:g}";
                ShowClockOut = true;
            }
            else
            {
                ShowClockOut = false;
            }

            var orders = _batch.Orders().ToList();
            Orders.Clear();

            double totalAmount = 0;
            double qtyOrdered = 0;
            double qtyReturned = 0;
            double qtyDumped = 0;

            foreach (var order in orders)
            {
                var orderTotal = order.OrderTotalCost();
                totalAmount += orderTotal;

                foreach (var detail in order.Details)
                {
                    if (detail.IsCredit)
                    {
                        if (detail.Damaged)
                            qtyDumped += detail.Qty;
                        else
                            qtyReturned += detail.Qty;
                    }
                    else
                    {
                        qtyOrdered += detail.Qty;
                    }
                }

                var orderViewModel = CreateOrderViewModel(order);
                orderViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BatchOrderViewModel.IsSelected))
                    {
                        UpdateButtonStates();
                    }
                };
                Orders.Add(orderViewModel);
            }

            AmountText = $"Amount: {totalAmount.ToCustomString()}";
            QtyOrderedText = $"Qty Ordered: {qtyOrdered}";
            QtyReturnedText = $"Qty Returned: {qtyReturned}";
            QtyDumpedText = $"Qty Dumped: {qtyDumped}";

            UpdateButtonStates();

            // Check for print label visibility
            if (_batch.Client?.ExtraProperties != null)
            {
                var vendor = _batch.Client.ExtraProperties.FirstOrDefault(x => 
                    x.Item1.Equals("printretaillabels", StringComparison.InvariantCultureIgnoreCase));
                ShowPrintLabelButton = vendor != null && vendor.Item2.Equals("1", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private BatchOrderViewModel CreateOrderViewModel(Order order)
        {
            var title = GetOrderTitle(order);
            var totalText = order.OrderType == OrderType.Consignment && order.OrderTotalCost() == 0
                ? title
                : $"{title} - Total: {order.OrderTotalCost().ToCustomString()}";

            var statusText = string.Empty;
            var statusColor = Colors.Transparent;
            var showStatus = false;

            if (order.Reshipped)
            {
                statusText = "Reshipped";
                statusColor = Colors.Purple;
                showStatus = true;
            }
            else if (order.Voided)
            {
                statusText = "Voided";
                statusColor = Colors.Red;
                showStatus = true;
            }
            else if (order.Finished)
            {
                statusText = "Finalized";
                statusColor = Colors.Green;
                showStatus = true;
            }
            else if (Config.ItemGroupedTemplate && order.ReadyToFinalize)
            {
                statusText = "Ready To Finalize";
                statusColor = Colors.Blue;
                showStatus = true;
            }

            var titleColor = order.Dexed ? Colors.Green : (order.Voided ? Colors.Red : Colors.Blue);

            var companyText = string.IsNullOrEmpty(order.CompanyName) 
                ? string.Empty 
                : $"Company: {order.CompanyName}";

            var poNumberText = string.IsNullOrEmpty(order.PONumber)
                ? string.Empty
                : $"PO Number: {order.PONumber}";

            string assetText = string.Empty;
            if (order.OrderType == OrderType.WorkOrder && order.AssetId > 0)
            {
                var asset = Asset.FindById(order.AssetId);
                if (asset != null)
                {
                    assetText = $"{asset.Product.Description} Part Number: {asset.SerialNumber}";
                }
            }

            var canSelect = !order.Voided || Config.CanVoidFOrders;

            return new BatchOrderViewModel
            {
                Order = order,
                OrderTitle = title,
                OrderTitleColor = titleColor,
                TotalText = totalText,
                ShowTotal = !Config.HidePriceInTransaction,
                StatusText = statusText,
                StatusColor = statusColor,
                ShowStatus = showStatus,
                CompanyText = companyText,
                ShowCompany = !string.IsNullOrEmpty(companyText),
                PoNumberText = poNumberText,
                ShowPONumber = !string.IsNullOrEmpty(poNumberText),
                AssetText = assetText,
                ShowAsset = !string.IsNullOrEmpty(assetText),
                CanSelect = canSelect,
                IsSelected = false
            };
        }

        private string GetOrderTitle(Order order)
        {
            if (order.OrderType == OrderType.NoService)
                return "No Service";
            if (order.IsWorkOrder)
                return "Work Order";
            if (order.IsExchange)
                return "Exchange";

            var orderTypeText = order.OrderType.ToString();
            if (!string.IsNullOrEmpty(order.PrintedOrderId))
                return $"{orderTypeText}: {order.PrintedOrderId}";

            return orderTypeText;
        }

        private void UpdateButtonStates()
        {
            var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
            var activeOrders = Orders.Where(x => !x.Order.Voided && !x.Order.Finished).Select(x => x.Order).ToList();

            CanPick = activeOrders.Count > 0 && !Config.HidePrintBatch;
            CanDex = Config.DexAvailable && activeOrders.Count > 0;
            CanFinalize = selectedOrders.Count > 0 && selectedOrders.All(x => !x.Voided && !x.Finished && !x.Reshipped);
            CanVoid = selectedOrders.Count > 0;
            CanReship = Config.UseReship && selectedOrders.Count > 0 && 
                       selectedOrders.Any(x => RouteEx.Routes.Any(r => r.Order != null && r.Order.OrderId == x.OrderId && !r.Order.Finished));
            CanPrintLabel = ShowPrintLabelButton && activeOrders.Count > 0;
            CanClockOut = true;
        }

        public BatchPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotals = !Config.HidePriceInTransaction;
            ShowDexButton = Config.DexAvailable;
            ShowReshipButton = Config.UseReship;

            Orders.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (BatchOrderViewModel order in e.NewItems)
                    {
                        order.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(BatchOrderViewModel.IsSelected))
                            {
                                UpdateButtonStates();
                            }
                        };
                    }
                }
            };
        }

        [RelayCommand]
        private async Task PickAsync()
        {
            var selectedOrders = GetSelectedActiveOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select valid orders to print.", "Alert");
                return;
            }

            var copies = await _dialogService.ShowPromptAsync("Copies to Print", "Enter number of copies:", initialValue: "1", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(copies) || !int.TryParse(copies, out var copiesCount) || copiesCount < 1)
            {
                await _dialogService.ShowAlertAsync("Please enter a valid number of copies.", "Alert");
                return;
            }

            await PrintBatchAsync(selectedOrders, copiesCount, false);
        }

        [RelayCommand]
        private async Task DexAsync()
        {
            await _dialogService.ShowAlertAsync("DEX functionality is not yet implemented in the MAUI version.", "Info");
        }

        [RelayCommand]
        private async Task FinalizeAsync()
        {
            var selectedOrders = GetSelectedActiveOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select valid orders to finalize.", "Alert");
                return;
            }

            if (selectedOrders.Any(x => x.Reshipped || x.Voided || x.Finished))
            {
                await _dialogService.ShowAlertAsync("Cannot finalize orders that are already finalized, voided, or reshipped.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                selectedOrders.Count == 1 ? "Finalize this order?" : $"Finalize these {selectedOrders.Count} orders?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                await FinalizeOrdersAsync(selectedOrders);
            }
        }

        [RelayCommand]
        private async Task VoidAsync()
        {
            var selectedOrders = GetSelectedOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select orders to void.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                selectedOrders.Count == 1 ? "Void this order?" : $"Void these {selectedOrders.Count} orders?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                await VoidOrdersAsync(selectedOrders);
            }
        }

        [RelayCommand]
        private async Task ReshipAsync()
        {
            var selectedOrders = GetSelectedActiveOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select valid orders to reship.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                selectedOrders.Count == 1 ? "Reship this order?" : $"Reship these {selectedOrders.Count} orders?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                await ReshipOrdersAsync(selectedOrders);
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync()
        {
            if (_batch == null)
                return;

            var orders = _batch.Orders().ToList();
            if (orders.Count == 0)
            {
                _batch.Delete();
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (!orders.All(x => x.Voided || x.Finished))
            {
                await _dialogService.ShowAlertAsync("All orders must be finalized or voided before clocking out.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync("Do you want to clock out of this batch?", "Warning", "Yes", "No");
            if (result)
            {
                _batch.ClockedOut = DateTime.Now;
                _batch.Save();

                // Close route stops
                foreach (var order in orders.Where(x => !x.Voided))
                {
                    foreach (var stop in RouteEx.Routes)
                    {
                        if (stop.Order != null && stop.Order.UniqueId == order.UniqueId)
                        {
                            stop.Closed = true;
                            stop.When = DateTime.Now;
                            stop.Latitude = DataAccess.LastLatitude;
                            stop.Longitude = DataAccess.LastLongitude;
                            break;
                        }
                    }
                }

                RouteEx.Save();

                if (_batch.Orders().Count == 0)
                    _batch.Delete();

                BackgroundDataSync.SyncFinalizedOrders();

                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task PrintLabelAsync()
        {
            var selectedOrders = GetSelectedActiveOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select valid orders to print labels.", "Alert");
                return;
            }

            var copies = await _dialogService.ShowPromptAsync("Copies to Print", "Enter number of copies:", initialValue: "1", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(copies) || !int.TryParse(copies, out var copiesCount) || copiesCount < 1)
            {
                await _dialogService.ShowAlertAsync("Please enter a valid number of copies.", "Alert");
                return;
            }

            await PrintLabelsAsync(selectedOrders, copiesCount);
        }

        private List<Order> GetSelectedActiveOrders()
        {
            return Orders.Where(x => x.IsSelected && !x.Order.Voided && !x.Order.Finished)
                .Select(x => x.Order).ToList();
        }

        private List<Order> GetSelectedOrders()
        {
            return Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
        }

        private async Task FinalizeOrdersAsync(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    if (Config.GeneratePreorderNum && string.IsNullOrEmpty(order.PrintedOrderId))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                    }

                    order.Finished = true;
                    order.EndDate = DateTime.Now;
                    order.Save();
                }

                LoadBatchData();
                await _dialogService.ShowAlertAsync("Orders finalized successfully.", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error finalizing orders.", "Error");
            }
        }

        private async Task VoidOrdersAsync(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    if (order.IsDelivery && !Config.DeliveryEditable)
                    {
                        await _dialogService.ShowAlertAsync($"The order {order.PrintedOrderId ?? ""} cannot be voided", "Alert");
                        continue;
                    }

                    if (order.Reshipped)
                    {
                        order.Reshipped = false;
                        order.ReasonId = 0;
                        order.ReshipDate = DateTime.MinValue;

                        foreach (var detail in order.Details)
                        {
                            detail.LoadingError = false;
                            detail.Price = 0;
                            detail.ExpectedPrice = 0;
                            detail.ConsignmentCounted = detail.ConsignmentSet = detail.ConsignmentUpdated = false;
                        }

                        order.Voided = true;
                    }
                    else
                    {
                        order.Void();
                    }

                    if (string.IsNullOrEmpty(order.PrintedOrderId))
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);

                    order.Save();
                }

                LoadBatchData();
                await _dialogService.ShowAlertAsync("Orders voided successfully.", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error voiding orders.", "Error");
            }
        }

        private async Task ReshipOrdersAsync(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    order.Reship();
                }

                LoadBatchData();
                await _dialogService.ShowAlertAsync("Orders reshipped successfully.", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error reshipping orders.", "Error");
            }
        }

        private async Task PrintBatchAsync(List<Order> orders, int copies, bool isPickTicket)
        {
            await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented in the MAUI version.", "Info");
            // TODO: Implement printing
        }

        private async Task PrintLabelsAsync(List<Order> orders, int copies)
        {
            await _dialogService.ShowAlertAsync("Print labels functionality is not yet fully implemented in the MAUI version.", "Info");
            // TODO: Implement label printing
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_batch == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_batch == null)
                return options;

            var orders = _batch.Orders().ToList();
            var hasActiveOrders = orders.Any(x => !x.Voided && !x.Finished);
            var isLocked = _batch.Status == BatchStatus.Locked;
            var creditTemplate = DataAccess.ActivityProvider.CreateActivity(ActivityNames.OrderCreditActivity);
            // Check if it's FullTemplateActivity - in MAUI, this would be SuperOrderTemplatePage
            // The check is based on the activity type name from Xamarin
            var useFullTemplate = Config.UseFullTemplateForClient(_batch.Client) && 
                DataAccess.ActivityProvider.IsFullTemplateActivity(creditTemplate);

            // Credit Order
            if (Config.AllowCreditOrders && !useFullTemplate)
            {
                options.Add(new MenuOption("Credit Invoice", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    var order = new Order(_batch.Client) { OrderType = OrderType.Credit };
                    order.BatchId = _batch.Id;
                    order.Save();
                    await NavigateToOrderAsync(order);
                }));
            }

            // No Service
            if (!Config.PreSale && orders.Count == 0)
            {
                options.Add(new MenuOption("No Service", async () =>
                {
                    if (orders.Count != 0)
                    {
                        await _dialogService.ShowAlertAsync("No service is only available when there are no orders in the batch.", "Alert");
                        return;
                    }

                    var result = await _dialogService.ShowConfirmAsync(
                        "Do you want to record no service for this client?",
                        "Alert",
                        "Yes",
                        "No");

                    if (result)
                    {
                        // Navigate to no service page
                        var noServiceOrder = new Order(_batch.Client) { OrderType = OrderType.NoService };
                        noServiceOrder.BatchId = _batch.Id;
                        noServiceOrder.Save();
                        await Shell.Current.GoToAsync($"noservice?orderId={noServiceOrder.OrderId}");
                    }
                }));
            }

            // Sales Invoice/Order
            options.Add(new MenuOption(useFullTemplate ? "Create Invoice" : "Sales Invoice", async () =>
            {
                if (isLocked)
                {
                    await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                    return;
                }

                var order = new Order(_batch.Client) { OrderType = OrderType.Order };
                order.BatchId = _batch.Id;
                order.Save();
                await NavigateToOrderAsync(order);
            }));

            // Return Invoice
            if (Config.UseReturnInvoice && !useFullTemplate)
            {
                options.Add(new MenuOption("Return Invoice", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    var order = new Order(_batch.Client) { OrderType = OrderType.Return };
                    order.BatchId = _batch.Id;
                    order.Save();
                    await NavigateToOrderAsync(order);
                }));
            }

            // Work Order
            if (Config.AllowWorkOrder)
            {
                options.Add(new MenuOption("Work Order", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    var order = new Order(_batch.Client) { OrderType = OrderType.Order };
                    order.BatchId = _batch.Id;
                    order.Save();
                    await Shell.Current.GoToAsync($"workorder?clientId={_batch.Client.ClientId}&orderId={order.OrderId}&asPresale=0");
                }));
            }

            // Consignment
            var consignmentForClient = true;
            if (!string.IsNullOrEmpty(_batch.Client.NonvisibleExtraPropertiesAsString))
            {
                var item = _batch.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "consignmentenabled");
                if (item != null && item.Item2 == "0")
                    consignmentForClient = false;
            }

            if (consignmentForClient && Config.Consignment)
            {
                if (!Config.UseFullConsignment)
                {
                    var countVisible = !Config.MagnoliaSetConsignment &&
                        ((_batch.Client.ConsignmentTemplate != null && _batch.Client.ConsignmentTemplate.Count > 0) || Config.UseBattery);
                    if (countVisible)
                    {
                        options.Add(new MenuOption("Count", async () => await CreateConsignmentOrderAsync(true)));
                    }

                    var visible = !Config.HideSetConsignment || (Config.HideSetConsignment && _batch.Client.ConsignmentTemplate == null);
                    if (visible)
                    {
                        options.Add(new MenuOption("Consignment Set", async () => await CreateConsignmentOrderAsync(false)));
                    }
                }
                else
                {
                    var title = Config.ParInConsignment ? "Par and Consignment" : "Consignment";
                    options.Add(new MenuOption(title, async () => await CreateConsignmentOrderAsync(false)));
                }
            }

            // Par Level Invoice
            if (Config.ClientDailyPL && !Config.ParInConsignment)
            {
                options.Add(new MenuOption("Par Level Invoice", async () =>
                {
                    var existingOrder = Order.Orders.FirstOrDefault(x => 
                        x.OrderType == OrderType.Order && 
                        x.IsParLevel && 
                        x.Client.ClientId == _batch.Client.ClientId && 
                        !x.Voided);

                    if (existingOrder == null)
                    {
                        var order = new Order(_batch.Client)
                        {
                            OrderType = OrderType.Order,
                            IsParLevel = true,
                            BatchId = _batch.Id
                        };
                        order.Save();
                        await NavigateToOrderAsync(order);
                    }
                    else
                    {
                        await NavigateToOrderAsync(existingOrder);
                    }
                }));
            }

            // View PDF
            if (orders.Count > 0 && !(_batch.Client.SplitInvoices.Count > 0))
            {
                options.Add(new MenuOption("View PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("View PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF viewing
                }));

                options.Add(new MenuOption("Share PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF sharing
                }));
            }

            // Send by Email
            if (!(_batch.Client.SplitInvoices.Count > 0))
            {
                options.Add(new MenuOption("Send by Email", async () =>
                {
                    await _dialogService.ShowAlertAsync("Send by Email functionality is not yet fully implemented.", "Info");
                    // TODO: Implement email sending
                }));
            }

            // Edit Client
            if (_batch.Client.ClientId == 0 || _batch.Client.Editable)
            {
                options.Add(new MenuOption("Edit Customer Info", async () =>
                {
                    await Shell.Current.GoToAsync($"editclient?clientId={_batch.Client.ClientId}");
                }));
            }

            // Attach Photo
            if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "37.0"))
            {
                options.Add(new MenuOption("Attach Photo", async () =>
                {
                    var selectedOrders = GetSelectedActiveOrders();
                    if (selectedOrders.Count == 0)
                    {
                        await _dialogService.ShowAlertAsync("Please select an order to attach photos.", "Alert");
                        return;
                    }

                    if (selectedOrders.Count > 1)
                    {
                        await _dialogService.ShowAlertAsync("Please select only one order to attach photos.", "Alert");
                        return;
                    }

                    await Shell.Current.GoToAsync($"clientimages?orderId={selectedOrders[0].OrderId}");
                }));
            }

            // Payment
            if (isLocked)
            {
                double collected = 0;
                foreach (var order in orders)
                {
                    var payment = InvoicePayment.List.FirstOrDefault(x => 
                        !string.IsNullOrEmpty(x.OrderId) && 
                        x.OrderId.Contains(order.UniqueId));
                    if (payment != null)
                        collected += payment.Components.Sum(x => x.Amount);
                }

                if (collected < _batch.Total() && !Config.HidePriceInTransaction)
                {
                    options.Add(new MenuOption("Receive Payment", async () =>
                    {
                        await Shell.Current.GoToAsync($"selectinvoice?clientId={_batch.Client.ClientId}&fromClientDetails=false");
                    }));
                }
            }

            // Advanced Options
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }

        private async Task CreateConsignmentOrderAsync(bool isCounting)
        {
            if (_batch == null)
                return;

            var order = Order.Orders.FirstOrDefault(x => 
                x.OrderType == OrderType.Consignment && 
                x.Client.ClientId == _batch.Client.ClientId);

            if (order == null)
            {
                order = new Order(_batch.Client);
                order.TaxRate = _batch.Client.TaxRate;
                order.BatchId = _batch.Id;

                if (_batch.Client.ConsignmentTemplate != null)
                {
                    foreach (var previous in _batch.Client.ConsignmentTemplate)
                    {
                        var detail = new OrderDetail(previous.Product, 0, order);
                        order.AddDetail(detail);
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
                            detail.Price = Product.GetPriceForProduct(detail.Product, order.Client, true);
                            detail.ConsignmentNewPrice = Product.GetPriceForProduct(detail.Product, order.Client, true);
                        }
                    }
                }

                if (Config.ParInConsignment || Config.ConsignmentBeta)
                    order.AddParInConsignment();

                order.OrderType = OrderType.Consignment;

                if (Config.ConsignmentBeta)
                    order.ExtraFields = DataAccess.SyncSingleUDF("cosignmentOrder", "1", order.ExtraFields);

                if (Config.UseFullConsignment)
                {
                    order.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentCount", "1", order.ExtraFields);
                    order.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentSet", "1", order.ExtraFields);
                }
                else
                {
                    if (isCounting)
                        order.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentCount", "1", order.ExtraFields);
                    else
                        order.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentSet", "1", order.ExtraFields);
                }

                order.Save();

                if (Config.TrackInventory)
                    DataAccess.SaveInventory();
            }

            await Shell.Current.GoToAsync($"consignment?orderId={order.OrderId}&counting={(isCounting ? "1" : "0")}");
        }

        private async Task NavigateToOrderAsync(Order order)
        {
            if (_batch == null)
                return;

            // Handle Credit or Return orders
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                await Shell.Current.GoToAsync($"ordercredit?orderId={order.OrderId}&asPresale=0");
                return;
            }

            // Handle Full Template logic
            if (Config.UseFullTemplateForClient(_batch.Client) && !_batch.Client.AllowOneDoc && !order.IsProjection)
            {
                // Similar logic to ClientDetailsPageViewModel
                await Shell.Current.GoToAsync($"superordertemplate?asPresale=0&orderId={order.OrderId}");
                return;
            }

            // Default: Navigate to OrderDetailsPage
            await Shell.Current.GoToAsync($"orderdetails?orderId={order.OrderId}&asPresale=0");
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            var options = new List<string>
            {
                "Update settings",
                "Send log file",
                "Export data",
                "Remote control"
            };

            if (Config.GoToMain)
            {
                options.Add("Go to main activity");
            }

            var choice = await _dialogService.ShowActionSheetAsync("Advanced options", "Cancel", null, options.ToArray());
            switch (choice)
            {
                case "Update settings":
                    await _appService.UpdateSalesmanSettingsAsync();
                    await _dialogService.ShowAlertAsync("Settings updated.", "Info");
                    break;
                case "Send log file":
                    await _appService.SendLogAsync();
                    await _dialogService.ShowAlertAsync("Log sent.", "Info");
                    break;
                case "Export data":
                    await _appService.ExportDataAsync();
                    await _dialogService.ShowAlertAsync("Data exported.", "Info");
                    break;
                case "Remote control":
                    await _appService.RemoteControlAsync();
                    break;
                case "Go to main activity":
                    await _appService.GoBackToMainAsync();
                    break;
            }
        }
    }

    public partial class BatchOrderViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _orderTitle = string.Empty;

        [ObservableProperty]
        private Color _orderTitleColor;

        [ObservableProperty]
        private string _totalText = string.Empty;

        [ObservableProperty]
        private bool _showTotal = true;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private Color _statusColor;

        [ObservableProperty]
        private bool _showStatus;

        [ObservableProperty]
        private string _companyText = string.Empty;

        [ObservableProperty]
        private bool _showCompany;

        [ObservableProperty]
        private string _poNumberText = string.Empty;

        [ObservableProperty]
        private bool _showPONumber;

        [ObservableProperty]
        private string _assetText = string.Empty;

        [ObservableProperty]
        private bool _showAsset;

        [ObservableProperty]
        private bool _canSelect = true;

        public Order Order { get; set; } = null!;
    }
}

