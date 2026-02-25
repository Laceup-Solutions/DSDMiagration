using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace LaceupMigration.ViewModels
{
    public partial class AcceptLoadEditDeliveryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private string _ordersIds = string.Empty;
        private string _file;
        private bool _changed = false;
        private bool _inventoryAccepted = false;
        private bool _canLeave = true;
        private string _uniqueId = Guid.NewGuid().ToString();
        private List<InventoryLine> _allProductList = new();
        private int _currentPosition = -1;

        [ObservableProperty] private ObservableCollection<AcceptLoadEditDeliveryLineViewModel> _productLines = new();
        [ObservableProperty] private string _totalItems = "0";
        [ObservableProperty] private string _totalQty = "0";
        [ObservableProperty] private string _totalWeight = "0";
        [ObservableProperty] private bool _showTotalQty = true;
        [ObservableProperty] private bool _showTotalWeight = true;
        [ObservableProperty] private string _finishButtonText = "Accept";
        [ObservableProperty] private bool _readOnly = false;

        public AcceptLoadEditDeliveryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _file = Path.Combine(Config.DataPath, "accpetInventoryResume.xml");

            // Match Xamarin ReadOnly property logic
            ReadOnly = Config.AcceptInventoryReadOnly || !Config.AcceptLoadEditable || !Config.NewSyncLoadOnDemand ||
                       !_canLeave;
        }

        public async Task InitializeAsync(string ordersIds, bool changed = false, bool inventoryAccepted = false,
            bool canLeave = true, string uniqueId = null)
        {
            _ordersIds = ordersIds ?? string.Empty;
            _changed = changed;
            // Preserve true once we've accepted (ApplyQueryAttributes can be called again with original query and would pass false)
            _inventoryAccepted = _inventoryAccepted || inventoryAccepted;
            _canLeave = canLeave;
            if (!string.IsNullOrEmpty(uniqueId)) _uniqueId = uniqueId;

            if (string.IsNullOrEmpty(_ordersIds))
            {
                Logger.CreateLog("Accept Inventory orders List null");
                await Shell.Current.GoToAsync("..");
                return;
            }

            await LoadProductListAsync();

            if (_inventoryAccepted)
                FinishButtonText = "Done";
            else
                FinishButtonText = "Accept";

            RefreshReadOnly();
        }

        private void RefreshReadOnly()
        {
            ReadOnly = Config.AcceptInventoryReadOnly || !Config.AcceptLoadEditable || !Config.NewSyncLoadOnDemand ||
                       !_canLeave;
        }

        /// <summary>
        /// Persist current page state (including _inventoryAccepted) so app restore loads with the same state and does not accept the load twice.
        /// </summary>
        private void SaveNavigationStateWithCurrentState()
        {
            var parts = new List<string>
            {
                "orderIds=" + Uri.EscapeDataString(_ordersIds ?? string.Empty),
                "changed=" + (_changed ? "true" : "false"),
                "inventoryAccepted=" + (_inventoryAccepted ? "true" : "false"),
                "canLeave=" + (_canLeave ? "true" : "false")
            };
            if (!string.IsNullOrEmpty(_uniqueId))
                parts.Add("uniqueId=" + Uri.EscapeDataString(_uniqueId));
            var route = "acceptloadeditdelivery?" + string.Join("&", parts);
            NavigationHelper.SaveNavigationState(route);
        }

        private async Task LoadProductListAsync()
        {
            await Task.Run(() =>
            {
                List<InventoryLine> productList = new List<InventoryLine>();

                if (File.Exists(_file))
                {
                    LoadListFromFile(productList);
                }
                else
                {
                    var parts = _ordersIds.Split('|');

                    foreach (var item in parts)
                    {
                        if (int.TryParse(item, out int id))
                        {
                            var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
                            if (order == null) continue;

                            foreach (var det in order.Details.Where(x => !x.IsCredit))
                            {
                                var prod = GetSimilar(productList, det);
                                if (prod == null)
                                {
                                    prod = new InventoryLine()
                                    {
                                        Product = det.Product,
                                        UoM = det.UnitOfMeasure,
                                        Weight = det.Weight,
                                        Lot = det.Lot,
                                        Expiration = det.LotExpiration
                                    };
                                    productList.Add(prod);
                                }

                                if (det.Product.SoldByWeight && det.Product.InventoryByWeight)
                                {
                                    prod.Starting += det.Weight;
                                    prod.Real += det.Weight;
                                }
                                else
                                {
                                    prod.Starting += det.Ordered;
                                    prod.Real += det.Qty;
                                }
                            }
                        }
                    }
                }

                _allProductList = SortDetails.SortedDetails(productList).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshProductLines();
                    RefreshLabels();
                });
            });
        }

        private InventoryLine GetSimilar(List<InventoryLine> productList, OrderDetail det)
        {
            return productList.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId &&
                                                   x.Lot == det.Lot && x.Weight == det.Weight &&
                                                   ((x.UoM == null && det.UnitOfMeasure == null) || (x.UoM != null &&
                                                       det.UnitOfMeasure != null && x.UoM.Id == det.UnitOfMeasure.Id)));
        }

        private void LoadListFromFile(List<InventoryLine> productList)
        {
            try
            {
                if (File.Exists(_file))
                {
                    using (StreamReader reader = new StreamReader(_file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 5 && int.TryParse(parts[0], out int pId))
                            {
                                var prod = Product.Products.FirstOrDefault(x => x.ProductId == pId);
                                if (prod != null)
                                {
                                    var item = new InventoryLine() { Product = prod };
                                    productList.Add(item);

                                    if (float.TryParse(parts[1], out float starting)) item.Starting = starting;
                                    if (float.TryParse(parts[2], out float real)) item.Real = real;

                                    if (int.TryParse(parts[3], out int uomId))
                                    {
                                        var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                                        item.UoM = uom;
                                    }

                                    if (float.TryParse(parts[4], out float weight)) item.Weight = weight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private void RefreshProductLines()
        {
            ProductLines.Clear();
            foreach (var line in _allProductList)
            {
                ProductLines.Add(new AcceptLoadEditDeliveryLineViewModel(line, this));
            }
        }

        private void RefreshLabels()
        {
            float items = 0;
            float qty = 0;
            double weight = 0;

            foreach (var item in _allProductList)
            {
                if (item.Product.SoldByWeight && item.Product.InventoryByWeight)
                {
                    weight += item.Real;
                    items++;
                }
                else
                {
                    var _qty = item.Real;
                    if (item.UoM != null) _qty *= item.UoM.Conversion;

                    qty += _qty;
                    items += item.Real;
                }
            }

            TotalItems = Math.Round(items, Config.Round).ToString(CultureInfo.InvariantCulture);
            TotalQty = Math.Round(qty, Config.Round).ToString(CultureInfo.InvariantCulture);
            TotalWeight = Math.Round(weight, Config.Round).ToString(CultureInfo.InvariantCulture);

            ShowTotalQty = qty > 0;
            ShowTotalWeight = weight > 0;
        }

        // UpdateQuantity method removed - logic moved to EditQuantityAsync for better control

        [RelayCommand]
        private async Task PrintAsync()
        {
            _appService.RecordEvent("PrintClicked button");

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    if (copies < 1) return "Please enter a valid number of copies.";

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result = false;
                    var source = _allProductList.Where(x => x.Real > 0 || x.Starting > 0);

                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintAcceptLoad(source, "", false);

                        if (Config.OldPrinter > 0) System.Threading.Thread.Sleep(2000);

                        if (!result) return "Error printing";
                        
                        IPrinter zebraPrinter = PrinterProvider.CurrentPrinter();

                        result = zebraPrinter.PrintAcceptedOrders(GetOrdersList(), _inventoryAccepted);
                        if (!result) return "Error printing";
                    }

                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            _appService.RecordEvent("SearchClicked button");

            // Match Xamarin: show dialog with empty text initially
            var searchText = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel",
                "Product Name", initialValue: "");

            // If user cancelled, don't change the list
            if (searchText == null) return;

            // Match Xamarin: if searchText is empty, show all products (Contains("") matches everything)
            // If searchText has value, filter the list
            string upper = (searchText ?? "").ToUpper();
            List<InventoryLine> filteredList;

            if (string.IsNullOrEmpty(upper))
            {
                // Empty search = show all products
                filteredList = _allProductList;
            }
            else
            {
                // Filter by product name
                filteredList = _allProductList.Where(x => x.Product.Name.ToUpper().Contains(upper)).ToList();
            }

            // Update the displayed list
            ProductLines.Clear();
            foreach (var line in filteredList)
            {
                ProductLines.Add(new AcceptLoadEditDeliveryLineViewModel(line, this));
            }
        }

        [RelayCommand]
        private async Task FinishAsync()
        {
            if (_inventoryAccepted)
            {
                await GoBackToLoadListAsync(true);
                return;
            }

            List<Order> orders = GetOrdersList();

            if (!Config.NewSyncLoadOnDemand)
                await UpdateStatusInOSAsync(orders);
            else
                await NewUpdateStatusInOSAsync(orders);

            // [MIGRATION]: MainActivity.deliveryOnDemand = true; - not needed in MAUI
        }

        private List<Order> GetOrdersList()
        {
            List<Order> orders = new List<Order>();

            var parts = _ordersIds.Split('|');

            foreach (var item in parts)
            {
                if (int.TryParse(item, out int id))
                {
                    var order = Order.Orders.FirstOrDefault(x => x.OrderId == id);
                    if (order != null) orders.Add(order);
                }
            }

            return orders;
        }

        private async Task UpdateStatusInOSAsync(List<Order> list)
        {
            await _dialogService.ShowLoadingAsync("Accepting Inventory");

            string responseMessage = null;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var orders = list.Where(x => x.OrderType == OrderType.Load).ToList();
                        var valuesChanged = GetValuesChangedPerOrder(orders);

                        DataProvider.AcceptLoadOrders(orders.Select(x => x.OriginalOrderId).ToList(), valuesChanged);

                        UpdateInventoryAndOrderStatus(list);
                    }
                    catch (Exception e)
                    {
                        Logger.CreateLog(e);
                        responseMessage = "Error accepting load orders.";
                    }
                });
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }

            if (!string.IsNullOrEmpty(responseMessage))
            {
                await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
            }
            else
            {
                _inventoryAccepted = true;
                SaveNavigationStateWithCurrentState();
                FinishButtonText = "Done";
                RefreshProductLines();

                var print = await _dialogService.ShowConfirmationAsync("Do you want to print the accepted load?",
                    "Alert", "Yes", "No");
                if (print)
                {
                    await PrintAcceptedAsync();
                }
                else
                {
                    // For old sync method, navigate back to Accept Load with needRefresh=1
                    // RefreshAsync(true) will check RouteOrdersCount and navigate to main if 0
                    await GoBackToLoadListAsync(true);
                }
            }
        }

        private async Task NewUpdateStatusInOSAsync(List<Order> orders)
        {
            string result = "";

            bool addSession = Config.CheckCommunicatorVersion("35.0.0.0");

            foreach (var item in orders)
            {
                if (addSession)
                    result += string.Format("NewOrder{0}{1}{0}{2}\n", (char)20, item.OriginalOrderId, Config.SessionId);
                else
                    result += string.Format("NewOrder{0}{1}\n", (char)20, item.OriginalOrderId);

                foreach (var det in item.Details)
                {
                    var qty = det.Qty;
                    // Match Xamarin: if (det.Product.SoldByWeight && !Config.UsePallets && det.Product.InventoryByWeight)
                    if (det.Product.SoldByWeight && !Config.UsePallets && det.Product.InventoryByWeight)
                        qty = det.Weight;

                    result += string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}|\n", (char)20, det.OriginalId, qty,
                        det.UnitOfMeasure != null ? det.UnitOfMeasure.Id : 0, det.Lot, det.Weight);
                }
            }

            result += string.Format("{1}{0}{2}\n", (char)20, "UpdatedInventory", _uniqueId);

            foreach (var item in _allProductList)
                result += string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}|\n", (char)20, item.Product.ProductId, item.Real,
                    item.UoM != null ? item.UoM.Id : 0, item.Lot, item.Weight);

            await _dialogService.ShowLoadingAsync("Accepting Inventory");

            string responseMessage = null;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string tempFile = Path.GetTempFileName();

                        try
                        {
                            Logger.CreateLog("Accepting Load");

                            using (NetAccess netaccess = new NetAccess())
                            {
                                using (StreamWriter writer = new StreamWriter(tempFile)) writer.WriteLine(result);

                                netaccess.OpenConnection();
                                netaccess.WriteStringToNetwork("HELO");
                                netaccess.WriteStringToNetwork(Config.GetAuthString());

                                netaccess.WriteStringToNetwork("AcceptInventoryCommand");
                                netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                                netaccess.SendFile(tempFile);

                                _canLeave = false;

                                var ack = netaccess.ReadStringFromNetwork();
                                if (ack.ToLowerInvariant() != "done")
                                    throw new Exception("Error accepting the load order. Ack=" + ack);

                                netaccess.WriteStringToNetwork("Goodbye");
                                netaccess.CloseConnection();

                                Logger.CreateLog("Load Accepted");

                                _canLeave = true;
                            }

                            if (File.Exists(tempFile)) File.Delete(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex);

                            if (File.Exists(tempFile)) File.Delete(tempFile);

                            _canLeave = false;
                            throw;
                        }

                        UpdateInventoryAndOrderStatus(orders);
                        RecalculateRoutes();
                        UpdateOrdersExtraFields(orders);
                    }
                    catch (Exception e)
                    {
                        Logger.CreateLog(e);
                        responseMessage = "Error accepting load orders.";
                    }
                });
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }

            RefreshReadOnly();

            if (!string.IsNullOrEmpty(responseMessage))
            {
                await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
            }
            else
            {
                // Set on UI thread so it's not lost if ApplyQueryAttributes/InitializeAsync runs again before PrintAcceptedAsync
                _inventoryAccepted = true;
                SaveNavigationStateWithCurrentState();
                FinishButtonText = "Done";
                RefreshProductLines();

                var print = await _dialogService.ShowConfirmationAsync("Do you want to print the accepted load?",
                    "Alert", "Yes", "No");
                if (print)
                {
                    await PrintAcceptedAsync();
                }
                else
                {
                    await GoBackToLoadListAsync(true);
                }
            }
        }

        /// <summary>
        /// Navigate to main and remove accept-load screens from ActivityState so the app won't restore to them next time.
        /// </summary>
        private async Task GoBackToMainAfterAcceptAsync()
        {
            NavigationHelper.RemoveNavigationState("acceptloadeditdelivery");
            NavigationHelper.RemoveNavigationState("acceptload");
            await _appService.GoBackToMainAsync();
        }

        private string GetValuesChangedPerOrder(List<Order> orders)
        {
            string result = "";

            Dictionary<int, List<OrderDetail>> values = orders.ToDictionary(x => x.OriginalOrderId,
                y => y.Details.Where(z => z.Ordered != z.Qty || z.OriginalUoM != z.UnitOfMeasure).ToList());

            foreach (var item in values)
            {
                if (item.Value.Count == 0) continue;

                result += string.Format("NewOrder{0}{1}\n", (char)20, item.Key);
                foreach (var det in item.Value)
                {
                    var qty = det.Qty;
                    if (det.Product.SoldByWeight && det.Product.InventoryByWeight) qty = det.Weight;
                    result += string.Format("{1}{0}{2}{0}{3}{0}{4}|\n", (char)20, det.OriginalId, qty,
                        det.UnitOfMeasure != null ? det.UnitOfMeasure.Id : 0, det.Lot);
                }
            }

            return result;
        }

        private void UpdateOrdersExtraFields(List<Order> orders)
        {
            if (!Config.RequireCodeForVoidInvoices) return;

            using (var netaccess = new NetAccess())
            {
                netaccess.OpenConnection();
                netaccess.WriteStringToNetwork("HELO");
                netaccess.WriteStringToNetwork(Config.GetAuthString());

                netaccess.WriteStringToNetwork("GetUpdatedOrdersExtraFieldsCommand");

                string ordersIds = string.Empty;

                foreach (var o in orders)
                {
                    if (string.IsNullOrEmpty(ordersIds))
                        ordersIds = o.UniqueId.ToString();
                    else
                        ordersIds += "," + o.UniqueId.ToString();
                }

                netaccess.WriteStringToNetwork(ordersIds);

                string result = netaccess.ReadStringFromNetwork();

                try
                {
                    var parts = result.Split((char)20);

                    for (int x = 0; x < orders.Count && x < parts.Length; x++) orders[x].ExtraFields = parts[x];
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
                finally
                {
                    netaccess.CloseConnection();
                }
            }
        }

        void UpdateInventoryAndOrderStatus(List<Order> orders)
        {
            if (_inventoryAccepted) return;

            // Match Xamarin: UpdateInventoryAndOrderStatus - only updates product inventory, not order details
            // Order details are sent to server via AcceptInventoryCommand, server handles the updates
            foreach (var item in _allProductList)
            {
                if (item.Real == 0) continue;

                item.Product.UpdateInventory(item.Real, item.UoM, item.Product.UseLot ? item.Lot : "", item.Expiration,
                    1, item.Weight);

                //sub the inventory in wh (not working before)
                item.Product.UpdateWarehouseInventory(item.Real, item.UoM, item.Product.UseLot ? item.Lot : "",
                    item.Expiration, -1, item.Weight);

                item.Product.AddRequestedInventory(item.Starting, item.UoM, item.Product.UseLot ? item.Lot : "",
                    item.Expiration, item.Weight);
                item.Product.AddLoadedInventory(item.Real, item.UoM, item.Product.UseLot ? item.Lot : "",
                    item.Expiration, item.Weight);
            }

            foreach (var order in orders)
            {
                if (order.IsDelivery)
                {
                    order.PendingLoad = false;
                    DataProvider.AddDeliveryClient(order.Client);

                    if (Config.DeleteZeroItemsOnDelivery)
                    {
                        List<OrderDetail> detailsToRemove =
                            new List<OrderDetail>(order.Details.Where(x => !x.Product.SoldByWeight && x.Qty == 0));
                        foreach (var item in detailsToRemove) order.Details.Remove(item);
                    }
                }
                else
                    order.Finished = true;

                order.Save();
            }

            // Set client Editable to false when finalizing orders (matches Xamarin BatchActivity.cs line 1828)
            // Only set once per client (use first order's client)
            var firstFinishedOrder =
                orders.FirstOrDefault(o => !o.IsDelivery && o.Client != null && o.Client.ClientId <= 0);
            if (firstFinishedOrder?.Client != null)
            {
                firstFinishedOrder.Client.Editable = false;
                Client.Save();
            }

            if (Config.RecalculateStops) RecalculateStops();

            _inventoryAccepted = true;
            ProductInventory.Save();
        }

        void RecalculateStops()
        {
            var l = RouteEx.Routes
                .Where(x => x.Date.Date.Subtract(DateTime.Now).Days <= 0 && x.Order != null && !x.Order.PendingLoad)
                .OrderBy(x => x.Stop)
                .ToList();

            int stop = 1;

            Dictionary<int, int> clientStop = new Dictionary<int, int>();

            foreach (var item in l)
            {
                if (clientStop.ContainsKey(item.Order.Client.ClientId))
                {
                    item.Stop = clientStop[item.Order.Client.ClientId];
                    continue;
                }

                clientStop.Add(item.Order.Client.ClientId, stop);
                item.Stop = stop;
                stop++;
            }

            RouteEx.Save();
        }

        // Match Xamarin: RecalculateRoutes() - reloads deliveries and updates RouteOrdersCount after accepting
        private void RecalculateRoutes()
        {
            if (Config.NewSyncLoadOnDemand)
            {
                try
                {
                    using (var netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());

                        netaccess.WriteStringToNetwork("GetRouteOrdersCountCommand");
                        var showAll = Config.ShowAllAvailableLoads ? "1" : "0";
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," +
                                                       DateTime.Today.ToString(CultureInfo.InvariantCulture) + "," +
                                                       showAll);
                        string result = netaccess.ReadStringFromNetwork();
                        int routeCount = 0;
                        int.TryParse(result, out routeCount);
                        Config.RouteOrdersCount = routeCount;

                        try
                        {
                            string deliveriesInSite = Path.GetTempFileName();
                            netaccess.WriteStringToNetwork("GetDeliveriesInSalesmanSiteCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) +
                                                           "," + DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                                           ",yes");
                            netaccess.ReceiveFile(deliveriesInSite);

                            DataProvider.LoadDeliveriesInSite(deliveriesInSite);

                            if (File.Exists(deliveriesInSite)) File.Delete(deliveriesInSite);
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex);
                        }
                        finally
                        {
                            netaccess.CloseConnection();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't crash - this is called after accepting, so we don't want to fail the whole operation
                    Logger.CreateLog(ex);
                }
            }
        }

        private async Task PrintAcceptedAsync()
        {
            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    if (copies < 1) return "Please enter a valid number of copies.";

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool result;
                    var source = _allProductList.Where(x => x.Real > 0 || x.Starting > 0);

                    for (int i = 0; i < copies; i++)
                    {
                        result = printer.PrintAcceptLoad(source, "", true);

                        if (Config.OldPrinter > 0) System.Threading.Thread.Sleep(2000);

                        if (!result) return "Error printing";

                        IPrinter zebraPrinter = PrinterProvider.CurrentPrinter();
                        
                        result = zebraPrinter.PrintAcceptedOrders(GetOrdersList(), _inventoryAccepted);
                        if (!result) return "Error printing";
                    }

                    return string.Empty;
                }, onSuccessAfterPrint: () => GoBackToLoadListAsync(true));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public async Task GoBackToLoadListAsync(bool refresh = false)
        {
            if (File.Exists(_file)) File.Delete(_file);

            if (refresh) AcceptLoadPageViewModel.PendingNeedRefreshFromEditDelivery = true;

            NavigationHelper.RemoveNavigationState("acceptloadeditdelivery");
            await Shell.Current.GoToAsync("..");
        }

        public async Task EditQuantityAsync(AcceptLoadEditDeliveryLineViewModel lineViewModel)
        {
            var line = lineViewModel.InventoryLine;
            var currentQty = line.Real;
            if (line.Product.SoldByWeight && line.Product.InventoryByWeight) currentQty = (float)line.Weight;

            var qtyText = await _dialogService.ShowPromptAsync("Set Quantity", "Enter Quantity", "OK", "Cancel",
                "Quantity", keyboard: Microsoft.Maui.Keyboard.Numeric,
                initialValue: currentQty.ToString(CultureInfo.InvariantCulture));

            // If user cancelled, don't change anything
            if (qtyText == null) return;

            // Match Xamarin: parse and round the quantity
            float newQty = 0;
            if (!float.TryParse(qtyText, out newQty)) return;

            // Match Xamarin: round the quantity using Config.Round
            newQty = (float)Math.Round(newQty, Config.Round);

            // Store old value to check if changed
            var oldReal = line.Real;
            var oldWeight = line.Weight;

            // Update quantity based on product type
            if (line.Product.SoldByWeight && line.Product.InventoryByWeight)
            {
                // For weight-based products, update both Weight and Real
                line.Weight = newQty;
                line.Real = newQty;
            }
            else
            {
                // For regular products, update Real only
                line.Real = newQty;
            }

            // Mark as changed if quantity actually changed
            if (!_changed && (oldReal != line.Real || oldWeight != line.Weight))
            {
                _changed = true;
            }

            // Refresh labels and update display
            RefreshLabels();
            lineViewModel.UpdateQtyText();
            lineViewModel.UpdateOrderedText();
        }
    }

    public partial class AcceptLoadEditDeliveryLineViewModel : ObservableObject
    {
        private readonly AcceptLoadEditDeliveryPageViewModel _parent;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _qtyText = "0";
        [ObservableProperty] private string _orderedText = string.Empty;
        [ObservableProperty] private Color _orderedColor = Colors.Blue;
        [ObservableProperty] private bool _showUoM = false;
        [ObservableProperty] private string _uomText = string.Empty;
        [ObservableProperty] private bool _showWeight = false;
        [ObservableProperty] private string _weightText = string.Empty;
        [ObservableProperty] private bool _showLot = false;
        [ObservableProperty] private string _lotText = string.Empty;

        public InventoryLine InventoryLine { get; }

        public AcceptLoadEditDeliveryLineViewModel(InventoryLine line, AcceptLoadEditDeliveryPageViewModel parent)
        {
            InventoryLine = line;
            _parent = parent;
            ProductName = line.Product.Name;
            UpdateQtyText();
            UpdateOrderedText();
            UpdateUoM();
            UpdateWeight();
            UpdateLot();
        }

        public void UpdateQtyText()
        {
            if (InventoryLine.Product.SoldByWeight && InventoryLine.Product.InventoryByWeight)
                QtyText = Math.Round(InventoryLine.Weight, Config.Round).ToString(CultureInfo.InvariantCulture);
            else
                QtyText = Math.Round(InventoryLine.Real, Config.Round).ToString(CultureInfo.InvariantCulture);
        }

        public void UpdateOrderedText()
        {
            OrderedText = $"Ordered: {Math.Round(InventoryLine.Starting, Config.Round)}";
            OrderedColor = InventoryLine.Starting != InventoryLine.Real ? Colors.Red : Color.FromArgb("#017CBA");
        }

        private void UpdateUoM()
        {
            if (InventoryLine.UoM != null)
            {
                ShowUoM = true;
                UomText = $"UoM: {InventoryLine.UoM.Name}";
            }
            else
            {
                ShowUoM = false;
            }
        }

        private void UpdateWeight()
        {
            if (InventoryLine.Product.SoldByWeight && !InventoryLine.Product.InventoryByWeight)
            {
                ShowWeight = true;
                WeightText = $"Weight: {InventoryLine.Weight}";
            }
            else
            {
                ShowWeight = false;
            }
        }

        private void UpdateLot()
        {
            if (!string.IsNullOrEmpty(InventoryLine.Lot))
            {
                ShowLot = true;
                LotText = $"Lot: {InventoryLine.Lot}";
            }
            else
            {
                ShowLot = false;
            }
        }

        [RelayCommand]
        private async Task EditQtyAsync()
        {
            await _parent.EditQuantityAsync(this);
        }
    }
}