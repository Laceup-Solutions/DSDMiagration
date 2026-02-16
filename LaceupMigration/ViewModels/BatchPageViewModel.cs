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
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Batch? _batch;
        private bool _initialized;
        private bool _canLeaveScreen = true;

        public ObservableCollection<BatchOrderViewModel> Orders { get; } = new();

        /// <summary>
        /// Gets the current batch ID for ActivityState management.
        /// </summary>
        public int? GetBatchId()
        {
            return _batch?.Id;
        }

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
        private string _totalText = "Total: ($0.00)";

        [ObservableProperty]
        private string _qtyOrderedText = "Qty Ordered: 0";

        [ObservableProperty]
        private string _qtyReturnedText = "Qty Returned: 0";

        [ObservableProperty]
        private string _qtyDumpedText = "Qty Dumped: 0";

        [ObservableProperty]
        private string _salesQtyText = "Sales Qty: 0";

        [ObservableProperty]
        private string _returnQtyText = "Return Qty: 0";

        [ObservableProperty]
        private string _dumpQtyText = "Dump Qty: 0";

        [ObservableProperty]
        private bool _canPrint;

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

        // Public property for CanLeaveScreen (used by OnBackButtonPressed)
        public bool CanLeaveScreen => _canLeaveScreen;


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

            // Recalculate qty based on inventory and subtract inventory for delivery (matches Xamarin BatchActivity OnCreate)
            SubtractInventoryForDeliveryBatch();

            LoadBatchData();
        }

        /// <summary>
        /// When entering the batch, subtract inventory for delivery orders (matches Xamarin BatchActivity OnCreate).
        /// </summary>
        private void SubtractInventoryForDeliveryBatch()
        {
            if (_batch == null)
                return;

            var orders = _batch.Orders().ToList();

            if (Config.TrackInventory)
            {
                foreach (var order in orders)
                {
                    if (Config.ParInConsignment && order.IsDelivery && order.OrderType == OrderType.Consignment)
                        continue;

                    foreach (var detail in order.Details)
                    {
                        if (detail.Substracted)
                            continue;

                        if (order.OrderType == OrderType.Consignment)
                            detail.Substracted = true;

                        if (!detail.Substracted)
                        {
                            if (Config.ScanDeliveryChecking && order.OrderType == OrderType.Order)
                            {
                                detail.Qty = 0;
                                detail.Substracted = true;
                                continue;
                            }

                            if (Config.ScanDeliveryChecking && detail.IsCredit && detail.Product.SoldByWeight)
                            {
                                detail.Qty = 0;
                                detail.Substracted = true;
                                continue;
                            }

                            var qty = detail.Qty;
                            if (detail.Product.SoldByWeight && detail.Product.InventoryByWeight)
                                qty = detail.Weight;

                            if (detail.UnitOfMeasure != null)
                                qty *= detail.UnitOfMeasure.Conversion;

                            if (!detail.IsCredit)
                            {
                                var currentInventory = detail.Product.GetInventory(false, detail.Lot ?? "", true, detail.Weight);

                                if (currentInventory < qty && !Config.CanGoBelow0)
                                {
                                    qty = currentInventory;
                                    if (qty < 0)
                                        qty = 0;

                                    if (qty > 0 && detail.UnitOfMeasure != null)
                                        qty /= detail.UnitOfMeasure.Conversion;

                                    Logger.CreateLog(string.Format("Not enough inventory for Product={0} in the order={1} (Inventory={2}, Qty={3}, NewQty={4}, Lot={5})",
                                        detail.Product.Name, order.OrderId, currentInventory, detail.Qty, qty, detail.Lot ?? ""));

                                    detail.Qty = qty;
                                }
                            }

                            order.UpdateInventory(detail, -1);
                            detail.Substracted = true;
                        }
                    }

                    order.Modified = true;
                    order.Save();
                }
            }
            else if (Config.ScanDeliveryChecking)
            {
                foreach (var order in orders)
                {
                    foreach (var detail in order.Details)
                    {
                        if (detail.Substracted)
                            continue;

                        detail.Qty = 0;
                        detail.Substracted = true;
                    }
                    order.Save();
                }
            }

            ProductInventory.Save();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
            {
                _canLeaveScreen = true; // Initialize on first appearance
                return;
            }

            // Xamarin OnResume: refreshes adapter and calls UpdateStates()
            // Reset canLeaveScreen when returning to screen (unless operation is in progress)
            // Note: If we're returning from FinalizeBatchActivity, the operation should be complete
            _canLeaveScreen = true;
            
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

            // When batch has no clock-in (e.g. delivery batch), set it so BatchPage shows Clock In like regular orders (matches Xamarin)
            if (_batch.ClockedIn == DateTime.MinValue)
            {
                _batch.ClockedIn = DateTime.Now;
                _batch.Save();
            }

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
            TotalText = $"Total: {totalAmount.ToCustomString()}";
            SalesQtyText = $"Sales Qty: {qtyOrdered:F0}";
            ReturnQtyText = $"Return Qty: {qtyReturned:F0}";
            DumpQtyText = $"Dump Qty: {qtyDumped:F0}";
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

            string companyText = string.Empty;
            if (order.CompanyId > 0)
            {
                var companyName = order.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    var company = CompanyInfo.Companies?.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                    companyName = company?.CompanyName ?? string.Empty;
                }
                if (!string.IsNullOrEmpty(companyName))
                    companyText = $"Company: {companyName}";
            }

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

            // No Service: show reason (from ReasonId or Comments when no predefined reasons)
            string reasonText = string.Empty;
            var showReason = false;
            if (order.OrderType == OrderType.NoService)
            {
                var reason = Reason.Find(order.ReasonId);
                reasonText = reason != null ? reason.Description : (order.Comments ?? string.Empty);
                showReason = !string.IsNullOrEmpty(reasonText);
                if (showReason)
                    reasonText = $"Reason: {reasonText}";
            }

            // Keep checkbox enabled at all times, including for voided orders
            var canSelect = true;

            return new BatchOrderViewModel(this)
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
                ReasonText = reasonText,
                ShowReason = showReason,
                CanSelect = canSelect,
                IsSelected = false,
                ClientName = order.Client?.ClientName ?? "Unknown Client",
                InvoiceAmountText = $"Invoice # Amount: {order.OrderTotalCost().ToCustomString()}",
                TermText = $"Term: {order.Term}"
            };
        }

        /// <summary>Matches Xamarin BatchActivity / ClientDetailsActivity: show "Invoice" when finished (or not presale), "Order" when presale or delivery not finished.</summary>
        private string GetOrderTitle(Order order)
        {
            if (order.OrderType == OrderType.NoService)
                return "No Service";
            if (order.IsWorkOrder)
                return "Work Order";
            if (order.IsExchange)
                return "Exchange";

            var number = string.IsNullOrEmpty(order.PrintedOrderId) ? string.Empty : $" #: {order.PrintedOrderId}";
            switch (order.OrderType)
            {
                case OrderType.Order:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return order.IsQuote ? $"Quote{number}" : $"Sales Order{number}";
                    return $"Invoice{number}";
                case OrderType.Consignment:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Sales order{number}";
                    return $"Invoice{number}";
                case OrderType.Credit:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Credit order{number}";
                    return $"Credit invoice{number}";
                case OrderType.Return:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Return order{number}";
                    return $"Return invoice{number}";
                case OrderType.Load:
                    // Delivery (load) orders: show as invoice when finished
                    if (order.AsPresale || !order.Finished)
                        return $"Sales order{number}";
                    return $"Invoice{number}";
                default:
                    return $"Invoice{number}";
            }
        }

        private void UpdateButtonStates()
        {
            if (_batch == null)
                return;

            var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
            var activeOrders = Orders.Where(x => !x.Order.Voided && !x.Order.Finished).Select(x => x.Order).ToList();
            var allOrders = Orders.Select(x => x.Order).ToList();

            // Xamarin UpdateStates logic (lines 2278-2354)
            // Check if batch is clocked out
            bool isClockedOut = _batch.ClockedOut != DateTime.MinValue && (DateTime.Now.Year - _batch.ClockedOut.Year < 2);

            if (isClockedOut)
            {
                // When clocked out, disable all buttons except print
                CanPick = false;
                CanDex = false;
                CanFinalize = false;
                CanVoid = false;
                CanReship = false;
                CanPrintLabel = false;
                CanClockOut = false; // Disable clock out button after clocking out
                // Print should still be enabled if there are orders
                CanPrint = allOrders.Count > 0 && !Config.HidePrintBatch;
            }
            else
            {
                // Normal button states
                CanPick = activeOrders.Count > 0 && !Config.HidePrintBatch;
                CanDex = Config.DexAvailable && activeOrders.Count > 0;
                CanFinalize = selectedOrders.Count > 0 && selectedOrders.All(x => !x.Voided && !x.Finished && !x.Reshipped);
                // Void enabled only when at least one selected order is voidable: not already voided, and (setting allows voiding finalized OR order is not finalized)
                CanVoid = selectedOrders.Any(o => !o.Voided && (Config.CanVoidFOrders || !o.Finished));
                CanReship = Config.UseReship && selectedOrders.Count > 0 && 
                           selectedOrders.Any(x => RouteEx.Routes.Any(r => r.Order != null && r.Order.OrderId == x.OrderId && !r.Order.Finished));
                CanPrintLabel = ShowPrintLabelButton && activeOrders.Count > 0;
                // Print available whenever there are any orders, including when all are voided
                CanPrint = allOrders.Count > 0 && !Config.HidePrintBatch;

                // Clock Out logic (matches Xamarin BatchView.UpdateStates):
                // Enabled when no active orders (empty batch, or all voided/finished) and batch not clocked out
                // Disabled when there are active orders
                if (allOrders.Count == 0)
                {
                    CanClockOut = true; // Enable when empty batch - matches Xamarin
                }
                else if (allOrders.All(x => x.Voided || x.Finished))
                {
                    CanClockOut = true; // Enabled when all orders are finalized/voided
                }
                else
                {
                    CanClockOut = false; // Disabled if there are active orders
                }

                // Special case: NoService order (line 2347-2353)
                if (allOrders.Count == 1 && allOrders[0].OrderType == OrderType.NoService)
                {
                    CanFinalize = false;
                    CanPick = false;
                    CanPrintLabel = false;
                    CanPrint = false;
                    CanClockOut = true;
                }
            }
        }

        public BatchPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
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
        private async Task PrintAsync()
        {
            var selectedOrders = GetSelectedOrders();
            if (selectedOrders.Count == 0)
            {
                // If no orders selected, use all active orders
                selectedOrders = Orders.Where(x => !x.Order.Voided && !x.Order.Finished).Select(x => x.Order).ToList();
            }

            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders to print.", "Alert");
                return;
            }
            
            await PrintBatchAsync(selectedOrders, false);
        }

        [RelayCommand]
        private async Task PickAsync()
        {
            var selectedOrders = GetSelectedOrders();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select valid orders to print.", "Alert");
                return;
            }

            await PrintBatchAsync(selectedOrders, false);
        }

        [RelayCommand]
        private async Task DexAsync()
        {
            await _dialogService.ShowAlertAsync("DEX functionality is not yet implemented in the MAUI version.", "Info");
        }

        [RelayCommand]
        private async Task FinalizeAsync()
        {
            // Xamarin BatchActivity: CannotGetoutNow() is called at the start of FinalizeButton_Click
            _canLeaveScreen = false;

            var selectedOrders = GetSelectedActiveOrders();
            if (selectedOrders.Count == 0)
            {
                _canLeaveScreen = true; // Reset if validation fails
                await _dialogService.ShowAlertAsync("Please select valid orders to finalize.", "Alert");
                return;
            }

            if (selectedOrders.Any(x => x.Reshipped || x.Voided || x.Finished))
            {
                _canLeaveScreen = true; // Reset if validation fails
                await _dialogService.ShowAlertAsync("Cannot finalize orders that are already finalized, voided, or reshipped.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                selectedOrders.Count == 1 ? "Are you sure that you would like to finalize this transaction?" : $"Are you sure you would like to finalize these {selectedOrders.Count} transactions?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                // Xamarin: Navigate to FinalizeBatchActivity instead of directly finalizing
                // Build ordersId string (comma-separated)
                var ordersIdString = string.Join(",", selectedOrders.Select(x => x.OrderId));
                var clientId = _batch?.Client?.ClientId ?? 0;
                
                // Set client Editable to false when finalizing orders (matches Xamarin BatchActivity.cs line 1828)
                // This happens BEFORE navigating to FinalizeBatchActivity, just like in Xamarin
                if (_batch?.Client != null && _batch.Client.ClientId <= 0)
                {
                    _batch.Client.Editable = false;
                    Client.Save();
                }
                
                await Shell.Current.GoToAsync($"finalizebatch?ordersId={ordersIdString}&clientId={clientId}&printed=0");
                
                // Reset canLeaveScreen after navigation (operation will complete in FinalizeBatchPage)
                // Note: canLeaveScreen will be reset when returning from FinalizeBatchPage in OnAppearingAsync
            }
            else
            {
                _canLeaveScreen = true; // Reset if user cancels
            }
        }

        [RelayCommand]
        private async Task VoidAsync()
        {
            // Xamarin BatchActivity: CannotGetoutNow() is called at the start of VoidButton_Click
            _canLeaveScreen = false;

            var selectedOrders = GetSelectedOrders();
            if (selectedOrders.Count == 0)
            {
                _canLeaveScreen = true; // Reset if validation fails
                await _dialogService.ShowAlertAsync("Please select orders to void.", "Alert");
                return;
            }

            // Cannot void already voided orders; respect Config.CanVoidFOrders for finalized orders
            var voidableOrders = selectedOrders
                .Where(o => !o.Voided && (Config.CanVoidFOrders || !o.Finished))
                .ToList();
            if (voidableOrders.Count == 0)
            {
                _canLeaveScreen = true;
                var allVoided = selectedOrders.All(o => o.Voided);
                var msg = allVoided
                    ? "Selected orders are already voided."
                    : "Selected orders cannot be voided. Finalized orders require the \"Can void finalized orders\" setting to be enabled.";
                await _dialogService.ShowAlertAsync(msg, "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                voidableOrders.Count == 1 ? "Void this order?" : $"Void these {voidableOrders.Count} orders?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                await VoidOrdersAsync(voidableOrders);
            }
            else
            {
                _canLeaveScreen = true; // Reset if user cancels
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
                var batchId = _batch.Id; // Save batchId before deletion
                _batch.Delete();
                // [ACTIVITY STATE]: Remove state when clocking out with no orders
                Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={batchId}");
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
                            stop.Latitude = Config.LastLatitude;
                            stop.Longitude = Config.LastLongitude;
                            break;
                        }
                    }
                }

                RouteEx.Save();

                if (_batch.Orders().Count == 0)
                    _batch.Delete();

                BackgroundDataSync.SyncFinalizedOrders();

                // [ACTIVITY STATE]: Remove state when clocking out
                Helpers.NavigationHelper.RemoveNavigationState($"batch?batchId={_batch.Id}");
                
                // Xamarin: ClockOut exits the screen (calls Finish())
                // Reset canLeaveScreen before exiting
                _canLeaveScreen = true;
                
                await Shell.Current.GoToAsync("..");
            }
        }

        public async Task ShowCannotLeaveDialog()
        {
            // Xamarin OnKeyDown: Shows alert if orders are not finalized/voided or batch not clocked out
            var allOrders = Orders.Select(x => x.Order).ToList();
            var validOrders = allOrders.Where(x => x.OrderType != OrderType.NoService).ToList();
            
            if (allOrders.Count == 0)
            {
                // Empty batch - should be able to leave
                return;
            }
            
            if (!allOrders.All(x => x.Voided || x.Finished))
            {
                await _dialogService.ShowAlertAsync("All orders must be finalized or voided before leaving.", "Alert", "OK");
                return;
            }
            
            // Xamarin: clock out required only when validOrders.Count > 0 (excludes No_Service)
            if (_batch != null && _batch.ClockedOut == DateTime.MinValue && validOrders.Count > 0)
            {
                await _dialogService.ShowAlertAsync("You must clock out before leaving.", "Alert", "OK");
                return;
            }
            
            // ButlerCustomization check: all voided orders must be printed
            if (Config.ButlerCustomization && _batch != null)
            {
                var voided = allOrders.Where(x => x.Voided).ToList();
                if (voided.Count > 0 && !voided.All(x => x.PrintedCopies > 0))
                {
                    await _dialogService.ShowAlertAsync("All voided transactions must be printed before continuing.", "Warning", "OK");
                    return;
                }
            }
        }

        [RelayCommand]
        private async Task PrintLabelAsync()
        {
            var selectedOrders = GetSelectedOrders();
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

        // This method is no longer used - finalization now happens in FinalizeBatchPage
        // Keeping it for backward compatibility if needed
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

                // Set client Editable to false when finalizing orders (matches Xamarin BatchActivity.cs line 1828)
                var firstOrder = orders.FirstOrDefault();
                if (firstOrder?.Client != null && firstOrder.Client.ClientId <= 0)
                {
                    firstOrder.Client.Editable = false;
                    Client.Save();
                }

                // Xamarin: adapter.NotifyDataSetChanged() and UpdateStates() are called after finalizing
                // Refresh UI to show finalized status in list
                LoadBatchData();
                
                // Reset canLeaveScreen after operation completes
                _canLeaveScreen = true;
                
                await _dialogService.ShowAlertAsync("Orders finalized successfully.", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                _canLeaveScreen = true; // Reset on error
                await _dialogService.ShowAlertAsync("Error finalizing orders.", "Error");
            }
        }

        private async Task VoidOrdersAsync(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    // Never void an already voided order
                    if (order.Voided)
                        continue;

                    // Respect setting: do not void finalized orders when CanVoidFOrders is false
                    if (order.Finished && !Config.CanVoidFOrders)
                        continue;

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

                // Xamarin: adapter.NotifyDataSetChanged() and UpdateStates() are called after voiding
                // Refresh UI to show voided status in list
                LoadBatchData();
                
                // Reset canLeaveScreen after operation completes
                _canLeaveScreen = true;
                
                await _dialogService.ShowAlertAsync("Orders voided successfully.", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                _canLeaveScreen = true; // Reset on error
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

        private async Task PrintBatchAsync(List<Order> orders, bool isPickTicket)
        {
            if (orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders to print.", "Alert");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    // Generate PrintedOrderId if needed
                    if (Config.GeneratePreorderNum)
                    {
                        foreach (var order in orders)
                        {
                            if (order != null && string.IsNullOrEmpty(order.PrintedOrderId))
                            {
                                order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                                order.Save();
                            }
                        }
                    }

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < number; i++)
                    {
                        foreach (var order in orders)
                        {
                            bool result = false;
                            
                            if (isPickTicket && !order.Finished)
                            {
                                result = printer.PrintPickTicket(order);
                            }
                            else
                            {
                                if (order.OrderType == OrderType.Consignment)
                                {
                                    if (Config.UseFullConsignment)
                                    {
                                        result = printer.PrintFullConsignment(order, !order.Finished);
                                    }
                                    else
                                    {
                                        result = printer.PrintConsignment(order, !order.Finished);
                                    }
                                    
                                    if (result && order.Finished)
                                        order.PrintedCopies += 1;
                                }
                                else
                                {
                                    result = printer.PrintOrder(order, !order.Finished, true);
                                    if (result && (order.Finished || Config.LockOrderAfterPrinted))
                                        order.PrintedCopies += 1;
                                }
                            }

                            if (!result)
                                allWent = false;
                        }
                    }

                    if (!allWent)
                        return "At least one order failed to print.";
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task PrintLabelsAsync(List<Order> orders, int copies)
        {
            if (orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders to print labels for.", "Alert");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    // Generate PrintedOrderId if needed
                    if (Config.GeneratePreorderNum)
                    {
                        foreach (var order in orders)
                        {
                            if (order != null && string.IsNullOrEmpty(order.PrintedOrderId))
                            {
                                order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                                order.Save();
                            }
                        }
                    }

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < number; i++)
                    {
                        if (!printer.PrintLabels(orders))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing labels.";
                    return string.Empty;
                }, copies);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing labels: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_batch == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private async Task SendByEmailAsync()
        {
            // Get selected orders (matches Xamarin BatchActivity.SelectActiveOrders)
            var orders = GetSelectedOrders();

            if (orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders selected to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send orders by email (matches Xamarin BatchActivity.SendByEmail)
                await PdfHelper.SendOrdersByEmail(orders);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
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
            bool isClockedOut = _batch.ClockedOut != DateTime.MinValue && (DateTime.Now.Year - _batch.ClockedOut.Year < 2);
            bool batchHasNoService = orders.Any(x => x.OrderType == OrderType.NoService);

            var useFullTemplate = Config.UseFullTemplateForClient(_batch.Client);

            // 1. Generate Projection (if applicable - not implemented in MAUI yet)
            // Note: This would be added here if implemented

            // 2. Count (Consignment)
            // 3. Consignment Set
            var consignmentForClient = true;
            if (!string.IsNullOrEmpty(_batch.Client.NonvisibleExtraPropertiesAsString))
            {
                var item = _batch.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "consignmentenabled");
                if (item != null && item.Item2 == "0")
                    consignmentForClient = false;
            }

            // Xamarin: when batch has No Service (e.g. finalized), hide Sales Invoice, Credit Invoice, Consignment, etc.
            if (!batchHasNoService && consignmentForClient && Config.Consignment)
            {
                if (!Config.UseFullConsignment)
                {
                    var countVisible = !Config.MagnoliaSetConsignment &&
                        ((_batch.Client.ConsignmentTemplate != null && _batch.Client.ConsignmentTemplate.Count > 0) || Config.UseBattery);
                    if (countVisible)
                    {
                        options.Add(new MenuOption("Count", async () => 
                        {
                            // Check unpaid invoices (matches Xamarin BatchActivity CanCreateOrder)
                            if (!await CanCreateOrderInBatchAsync(OrderType.Consignment))
                                return;
                            await CreateConsignmentOrderAsync(true);
                        }));
                    }

                    var visible = !Config.HideSetConsignment || (Config.HideSetConsignment && _batch.Client.ConsignmentTemplate == null);
                    if (visible)
                    {
                        options.Add(new MenuOption("Consignment Set", async () => 
                        {
                            // Check unpaid invoices (matches Xamarin BatchActivity CanCreateOrder)
                            if (!await CanCreateOrderInBatchAsync(OrderType.Consignment))
                                return;
                            await CreateConsignmentOrderAsync(false);
                        }));
                    }
                }
                else
                {
                    var title = Config.ParInConsignment ? "Par and Consignment" : "Consignment";
                    options.Add(new MenuOption(title, async () => 
                    {
                        // Check unpaid invoices (matches Xamarin BatchActivity CanCreateOrder)
                        if (!await CanCreateOrderInBatchAsync(OrderType.Consignment))
                            return;
                        await CreateConsignmentOrderAsync(false);
                    }));
                }
            }

            // 4. Bill (not implemented in MAUI)
            // 5. Sales Invoice/Order (hidden when batch is clocked out or has No Service - matches Xamarin)
            if (!isClockedOut && !batchHasNoService)
            {
                options.Add(new MenuOption(useFullTemplate ? "Create Invoice" : "Sales Invoice", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    // Check unpaid invoices (matches Xamarin BatchActivity CanCreateOrder method)
                    if (!await CanCreateOrderInBatchAsync(OrderType.Order))
                        return;

                    // Check credit limit (matches Xamarin BatchActivity line 2769)
                    if (_batch.Client.IsOverCreditLimit())
                    {
                        await _dialogService.ShowAlertAsync("This customer is over the credit limit. No new order is allowed", "Alert");
                        return;
                    }
                    
                    if(!await EnsureCompanySelectedAsync())
                        return;

                    var order = new Order(_batch.Client) { OrderType = OrderType.Order };
                    order.BatchId = _batch.Id;
                    CompanyInfo.AssignCompanyToOrder(order);
                    order.Save();
                    await NavigateToOrderAsync(order);
                }));
            }

            // 6. Credit Invoice (hidden when batch is clocked out or has No Service; Credit Invoice doesn't check credit limit - it's a credit that reduces balance)
            if (!batchHasNoService && Config.AllowCreditOrders && !useFullTemplate && !isClockedOut)
            {
                options.Add(new MenuOption("Credit Invoice", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }
                    
                    if(!await EnsureCompanySelectedAsync())
                        return;
                    
                    // Credit Invoice doesn't check credit limit (matches Xamarin BatchActivity line 2796)
                    var order = new Order(_batch.Client) { OrderType = OrderType.Credit };
                    order.BatchId = _batch.Id;
                    CompanyInfo.AssignCompanyToOrder(order);
                    order.Save();
                    await NavigateToOrderAsync(order);
                }));
            }
            
            // 7. Return Invoice (hidden when batch has No Service; Note: Return Invoice doesn't check credit limit in Xamarin - it's a credit, so allowed)
            if (!batchHasNoService && Config.UseReturnInvoice && !useFullTemplate)
            {
                options.Add(new MenuOption("Return Invoice", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    if(!await EnsureCompanySelectedAsync())
                        return;
                    
                    // Return Invoice doesn't check credit limit (it reduces balance, so it's allowed)
                    var order = new Order(_batch.Client) { OrderType = OrderType.Return };
                    order.BatchId = _batch.Id;
                    CompanyInfo.AssignCompanyToOrder(order);
                    order.Save();
                    await NavigateToOrderAsync(order);
                }));
            }

            // Work Order (hidden when batch has No Service)
            if (!batchHasNoService && Config.AllowWorkOrder)
            {
                options.Add(new MenuOption("Work Order", async () =>
                {
                    if (isLocked)
                    {
                        await _dialogService.ShowAlertAsync("Batch is locked. Cannot create new orders.", "Alert");
                        return;
                    }

                    if(!await EnsureCompanySelectedAsync())
                        return;
                    
                    var order = new Order(_batch.Client) { OrderType = OrderType.Order };
                    order.BatchId = _batch.Id;
                    CompanyInfo.AssignCompanyToOrder(order);
                    order.Save();
                    await Shell.Current.GoToAsync($"workorder?clientId={_batch.Client.ClientId}&orderId={order.OrderId}&asPresale=0");
                }));
            }

            // Par Level Invoice (hidden when batch has No Service)
            if (!batchHasNoService && Config.ClientDailyPL && !Config.ParInConsignment)
            {
                options.Add(new MenuOption("Par Level Invoice", async () =>
                {
                    // Check unpaid invoices (matches Xamarin BatchActivity CanCreateOrder)
                    if (!await CanCreateOrderInBatchAsync(OrderType.Order))
                        return;

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
                        CompanyInfo.AssignCompanyToOrder(order);
                        order.Save();
                        await NavigateToOrderAsync(order);
                    }
                    else
                    {
                        await NavigateToOrderAsync(existingOrder);
                    }
                }));
            }

            // 12. Scan Based Trading (not implemented in MAUI)
            // 13. Asset Tracking (not implemented in MAUI)

            // 14. No Service - hidden when batch already has No Service (matches Xamarin: options disabled after No Service finalized)
            var existingNoService = orders.FirstOrDefault(x => x.OrderType == OrderType.NoService && !x.Voided);
            var showNoServiceOption = !batchHasNoService && !Config.PreSale && (orders.Count == 0 || existingNoService != null);
            if (showNoServiceOption)
            {
                options.Add(new MenuOption("No Service/Sales", async () =>
                {
                    if (existingNoService != null)
                    {
                        // Already have No Service - navigate to edit it
                        await Shell.Current.GoToAsync($"noservice?orderId={existingNoService.OrderId}");
                        return;
                    }

                    // No orders yet - create new No Service
                    var result = await _dialogService.ShowConfirmAsync(
                        "Do you want to record no service for this client?",
                        "Alert",
                        "Yes",
                        "No");

                    if (result)
                    {
                        var noServiceOrder = new Order(_batch.Client) { OrderType = OrderType.NoService };
                        noServiceOrder.BatchId = _batch.Id;
                        CompanyInfo.AssignCompanyToOrder(noServiceOrder);
                        noServiceOrder.Save();
                        await Shell.Current.GoToAsync($"noservice?orderId={noServiceOrder.OrderId}");
                    }
                }));
            }

            // 15. Receive Payment
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

            // 16. Set Par Level (not implemented in MAUI)
            // 17. Load Order (not implemented in MAUI)

            // 18. Edit Customer Info
            if (_batch.Client.ClientId == 0 || _batch.Client.Editable)
            {
                options.Add(new MenuOption("Edit Customer Info", async () =>
                {
                    await Shell.Current.GoToAsync($"editclient?clientId={_batch.Client.ClientId}");
                }));
            }

            // Attach Photo (hidden when batch is clocked out)
            if (Config.CheckCommunicatorVersion("37.0") && !isClockedOut)
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

                    await Shell.Current.GoToAsync($"viewcapturedimages?orderId={selectedOrders[0].OrderId}");
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

            // 21. Send by Email
            if (!(_batch.Client.SplitInvoices.Count > 0))
            {
                options.Add(new MenuOption("Send by Email", async () =>
                {
                    await SendByEmailAsync();
                }));
            }

            // 22. View PDF
            if (orders.Count > 0 && !(_batch.Client.SplitInvoices.Count > 0))
            {
                options.Add(new MenuOption("View PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("View PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF viewing
                }));

                // 23. Share PDF
                options.Add(new MenuOption("Share PDF", async () =>
                {
                    await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.", "Info");
                    // TODO: Implement PDF sharing
                }));
            }
            // Advanced Options
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }
        
        private async Task<bool> EnsureCompanySelectedAsync()
        {
            // Already selected? (optional guard)
            if (CompanyInfo.SelectedCompany != null)
                return true;

            var clientCompanies = SalesmanAvailableCompany.GetCompanies(Config.SalesmanId, _batch.Client.ClientId);

            if (clientCompanies.Count > 1)
            {
                var companyOptions = clientCompanies.Select(c =>
                {
                    var subtitleParts = new[] { c.CompanyAddress1, c.CompanyAddress2, c.CompanyPhone }
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                    var subtitle = subtitleParts.Length > 0 ? string.Join("\n", subtitleParts) : null;
                    return (c.CompanyName ?? string.Empty, subtitle);
                }).ToArray();

                var selectedIndex = await _dialogService.ShowSingleChoiceDialogAsync(
                    "Select Company",
                    companyOptions,
                    0);

                if (selectedIndex >= 0 && selectedIndex < clientCompanies.Count)
                {
                    CompanyInfo.SelectedCompany = clientCompanies[selectedIndex];
                    return true;
                }
                else
                    return false;
            }
            else if (clientCompanies.Count == 1)
            {
                CompanyInfo.SelectedCompany = clientCompanies[0];
                return true;
            }
            else
                return true;
        }

        /// <summary>
        /// Checks if an order can be created based on unpaid invoices.
        /// Matches Xamarin BatchActivity.CanCreateOrder method.
        /// Credit and Return orders skip this check (they reduce balance).
        /// </summary>
        private async Task<bool> CanCreateOrderInBatchAsync(OrderType orderType)
        {
            if (_batch == null || _batch.Client == null)
                return true;

            // Skip unpaid invoice checks for Credit and Return orders (they reduce balance, so allowed)
            // This matches Xamarin behavior - Credit and Return orders don't check unpaid invoices
            if (orderType == OrderType.Credit || orderType == OrderType.Return)
                return true;

            // Check due invoices
            if (Config.CheckDueInvoicesInCreateOrder || Config.CheckDueInvoicesQtyInCreateOrder > 0)
            {
                var openInvoices = _batch.Client.Invoices?.Where(x => x.Balance > 0 && x.DueDate < DateTime.Today).ToList() ?? new List<Invoice>();
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
                var unpaidInvoices = _batch.Client.Invoices?.Where(x => x.Balance > 0 && x.DueDate.AddDays(90) < DateTime.Now.Date).ToList() ?? new List<Invoice>();
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

            return true;
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
                CompanyInfo.AssignCompanyToOrder(order);

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
                    order.ExtraFields = UDFHelper.SyncSingleUDF("cosignmentOrder", "1", order.ExtraFields);

                if (Config.UseFullConsignment)
                {
                    order.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentCount", "1", order.ExtraFields);
                    order.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentSet", "1", order.ExtraFields);
                }
                else
                {
                    if (isCounting)
                        order.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentCount", "1", order.ExtraFields);
                    else
                        order.ExtraFields = UDFHelper.SyncSingleUDF("ConsignmentSet", "1", order.ExtraFields);
                }

                order.Save();

                if (Config.TrackInventory)
                    ProductInventory.Save();
            }

            await Shell.Current.GoToAsync($"consignment?orderId={order.OrderId}&counting={(isCounting ? "1" : "0")}");
        }

        public async Task NavigateToOrderAsync(Order order)
        {
            if (_batch == null)
                return;

            // Handle NoService orders - navigate to No Service page (view/edit finalized or in-progress)
            if (order.OrderType == OrderType.NoService)
            {
                await Shell.Current.GoToAsync($"noservice?orderId={order.OrderId}");
                return;
            }

            // Handle Credit or Return orders - navigate to advancedcatalog, previouslyorderedtemplate, or orderdetails
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                // Use the same navigation logic as regular orders
                // The target page will detect OrderType.Credit and hide Sales button
                if (Config.UseLaceupAdvancedCatalog)
                {
                    await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
                }
                else
                    await Shell.Current.GoToAsync($"ordercredit?orderId={order.OrderId}&asPresale=0&fromOneDoc=0");
                
                return;
            }

            // Handle Full Template logic
            if (Config.UseFullTemplateForClient(_batch.Client) && !_batch.Client.AllowOneDoc && !order.IsProjection)
            {
                // Similar logic to ClientDetailsPageViewModel
                await Shell.Current.GoToAsync($"superordertemplate?asPresale=0&orderId={order.OrderId}");
                return;
            }

            // Handle SalesByDepartment
            if (Config.SalesByDepartment)
            {
                await Shell.Current.GoToAsync($"batchdepartment?clientId={_batch.Client.ClientId}&batchId={_batch.Id}");
                return;
            }
            
            // If UseLaceupAdvancedCatalog is TRUE
            if (Config.UseLaceupAdvancedCatalog)
            {
                await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
                return;
            }

            await Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={order.OrderId}&asPresale=0");
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }
    }

    public partial class BatchOrderViewModel : ObservableObject
    {
        private readonly BatchPageViewModel _parent;

        public BatchOrderViewModel(BatchPageViewModel parent)
        {
            _parent = parent;
        }

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
        private string _reasonText = string.Empty;

        [ObservableProperty]
        private bool _showReason;

        [ObservableProperty]
        private bool _canSelect = true;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _invoiceAmountText = string.Empty;

        [ObservableProperty]
        private string _termText = string.Empty;

        public Order Order { get; set; } = null!;

        [RelayCommand]
        private async Task ViewOrderAsync()
        {
            if (Order != null && _parent != null)
            {
                await _parent.NavigateToOrderAsync(Order);
            }
        }
    }
}

