using iText.Kernel.XMP.Impl.XPath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace LaceupMigration
{
    public class DataAccessEx
    {
        #region Product Index

        static int ProductIDIndex = 0;
        static int ProductNameIndex = 1;
        static int ProductUPCIndex = 2;

        static int ProductCommentIndex = 3;
        static int ProductPackageIndex = 4;
        static int ProductDescriptionIndex = 5;
        static int ProductPriceLevel0Index = 6;
        static int ProductPriceLevel1Index = 7;
        static int ProductPriceLevel2Index = 8;
        static int ProductPriceLevel3Index = 9;
        static int ProductPriceLevel4Index = 10;
        static int ProductPriceLevel5Index = 11;
        static int ProductPriceLevel6Index = 12;
        static int ProductPriceLevel7Index = 13;
        static int ProductPriceLevel8Index = 14;
        static int ProductPriceLevel9Index = 15;
        static int ProductCategoryIDIndex = 16;
        static int ProductOriginalIDIndex = 17;
        static int ProductOnHandIndex = 18;
        static int ProductExtraFieldsIndex = 19;
        static int ProductTaxableIndex = 20;
        static int ProductItemTypeIndex = 21;
        static int ProductNonVisibleExtraFieldIndex = 22;
        static int ProductUoMIndex = 23;
        static int ProductSoldByWeightIndex = 24;
        static int ProductWLocationIndex = 26;
        static int CodeIndex = 25;
        static int ProductCostIndex = 27;
        static int ProductOrderInCategoryIndex = 30;
        static int ProductTaxRateIndex = 34;
        static int ProductDiscountCategoryIndex = 35;
        static int ProductPriceCategoryIdIndex = 36;

        #endregion

        public static bool LoadingData { get; set; }

        public static void Initialize()
        {
            LoadingData = true;

            try
            {
                ProductInventory.Load();

                bool gotUnitOfMeasures = false;

                if (File.Exists(Config.UnitOfMeasuresFile))
                {
                    LoadUoM(Config.UnitOfMeasuresFile);
                    gotUnitOfMeasures = true;
                }

                if (File.Exists(Config.ProductStoreFile))
                {
                    LoadData__(Config.ProductStoreFile, false, !gotUnitOfMeasures);
                    try
                    {
                        ProductImage.LoadMap();
                    }
                    catch (Exception ex1)
                    {
                        Logger.CreateLog(ex1);
                    }
                }

                if (File.Exists(Config.ClientStoreFile))
                    LoadData__(Config.ClientStoreFile, false, !gotUnitOfMeasures);

                if (Config.Delivery)
                    if (File.Exists(Config.DeliveryFile))
                        ProcessDeliveryFile(Config.DeliveryFile, false, false);

                if (Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand)
                    DataAccess.LoadNewDeliveryClients();

                try
                {
                    Client.LoadClients();
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

                ClientDepartment.LoadFromFile();

                DataAccess.LoadBatches();

                DataAccess.LoadOrders();

                PrintedOrderZPL.LoadZPL();

                DataAccess.LoadPayments();

                Client.LoadNotes();

                BuildToQty.LoadList();

                LoadOrder.LoadList();

                ParLevel.LoadList();

                DataAccess.LoadFutureRoutes();

                if (File.Exists(Config.RouteExFile))
                    RouteEx.Load();

                DataAccess.LoadReasons();

                ClientDailyParLevel.LoadCreatedParLevels(true);

                DataAccess.LoadParLevelHistory();

                DataAccess.LoadOrderHistory();

                DataAccess.LoadProjectionValues();

                SalesmanSession.LoadSessions();

                ClientProdSort.Load();

                AssetTracking.Load();

                AssetTrackingHistory.Load();

                ClientSalesProjection.Load();

                Truck.Load();

                ClientProjectionDetail.Load();

                if (Config.ButlerCustomization)
                {
                    RoutesWarehouseRelation.Load();
                }

                BankAccount.LoadBanks();

                TemporalInvoicePayment.Load();

                SelfServiceCompany.Load();

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "45.0.0"))
                {
                    BankDeposit.Load();
                }

                if (Config.ShowOrderStatus)
                    LoadOrdersStatus();

                if (Config.BringBranchInventories || Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales || Config.MilagroCustomization || Config.DicosaCustomization)
                {
                    LoadSites();
                    LoadInventories();
                }

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.0.0") && Config.ViewGoals)
                {
                    LoadGoalProgress();
                    LoadGoalProgressDetail();
                }

                Session.InitializeSession();

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.0.0"))
                    LoadTerms();

                if (Config.SAPOrderStatusReport)
                    SapStatus.Load();

                if (Config.RequestVehicleInformation)
                    VehicleInformation.Load();

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0.0"))
                    DataAccess.AsignLogosToCompanies();

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0.0"))
                    LoadProductLabelDecoder();
                
                RouteExpenses.LoadExpenses();

                var batchesToDelete = new List<Batch>();
                var emptyBatches = Batch.List.Where(x => x.Orders().Count == 0);
                foreach (var item in emptyBatches)
                    batchesToDelete.Add(item);

                foreach (var d in batchesToDelete)
                    d.Delete();

                if (Config.SiteId == 0)
                {
                    var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                    if (salesman != null)
                    {
                        Config.SiteId = salesman.InventorySiteId;
                        Config.SaveSettings();
                    }
                }

                if (OrderDiscount.HasDiscounts)
                    BackgroundDataSync.AssignDefaultUomInBackground();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

            LoadingData = false;
        }

        public static string DownloadData(bool getDeliveries, bool updateInventory)
        {
            LoadingData = true;

            Config.BranchSiteId = Config.SiteId = 0;

            TemporalInvoicePayment.Delete();
            Config.dataDownloaded = true;

            Config.DidCloseAlert = false;

            BackgroundDataSync.GetImages();

            if (string.IsNullOrEmpty(Config.SessionId))
                GetSessionId();

            var ordersId = RouteEx.Routes.Where(x => x.Order != null && !x.Order.Finished).Select(x => x.Order.UniqueId).ToList();

            DateTime start = DateTime.Now;

            // If an End of day has never been done, mark it now as it
            var lastTime = Config.GetLastEndOfDay();
            if (lastTime.Year == 1 || (Order.Orders.Where(x => x.OrderType != OrderType.Load).ToList().Count == 0 && InvoicePayment.List.Count == 0))
                Config.SaveLastEndOfDay();

            try
            {
                DateTime now = DateTime.Now;
                NetAccess.GetCommunicatorVersion();
                Logger.CreateLog("Communicator version got " + DataAccess.CommunicatorVersion + " in " + DateTime.Now.Subtract(now).TotalSeconds);

                now = DateTime.Now;
                DataAccess.GetSalesmanSettings();
                Logger.CreateLog("Salesman Settings downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                DataAccess.SendSalesmanDeviceInfo();

                //syncloadondemand is always active for new versions
                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "26.0.0.0"))
                {
                    Config.NewSyncLoadOnDemand = Config.Delivery;
                    Config.SaveSettings();
                }
                else
                {
                    Config.NewSyncLoadOnDemand = false;
                    Config.SaveSettings();
                }

                if ((!Config.TimeSheetCustomization || (Config.TimeSheetCustomization && Config.TimeSheetAutomaticClockIn)) && Session.session == null)
                    Session.CreateSession();

                DataAccess.AcceptInventoryReadOnly = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "31.0.0.0");

                string basefileP = Path.Combine(Config.DataPath, "reading_fileP.zip");
                string targetfileP = Config.ProductStoreFile;
                string basefileC = Path.Combine(Config.DataPath, "reading_fileC.zip");
                string targetfileC = Config.ClientStoreFile;

                if (updateInventory)
                {
                    ProductInventory.ClearAll();
                    Logger.CreateLog("Inventories Deleted");
                }

                var gotUnitOfMeasures = GetUnitOfMeasures();

                using (NetAccess netaccess = new NetAccess())
                {
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    if (updateInventory && Config.AutoAcceptLoad && DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.9.0.0"))
                    {
                        try
                        {
                            netaccess.WriteStringToNetwork("AutoAcceptLoadCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + DateTime.Now.Ticks);
                        }
                        catch
                        {
                            Logger.CreateLog("Error autoaccepting the load");
                        }
                    }

                    netaccess.WriteStringToNetwork("Products");

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileP) == 0)
                        return "Error Downloading Products";
                    Logger.CreateLog("Products downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    netaccess.WriteStringToNetwork("Clients");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileC) == 0)
                        return "Error Downloading Customers";
                    Logger.CreateLog("Clients downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    bool inventoryOnDemand = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "13.0.0.0");

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileP, targetfileP);
                    LoadData__(Config.ProductStoreFile, updateInventory && !inventoryOnDemand, !gotUnitOfMeasures);
                    Logger.CreateLog("Products processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileP);

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileC, targetfileC);
                    Client.DeleteClients();
                    LoadData__(Config.ClientStoreFile, updateInventory && !inventoryOnDemand, !gotUnitOfMeasures);
                    Logger.CreateLog("Clients processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileC);

                    //this was changed especifically for PICKOLE / monrovia Config.RecalculateRoutesOnSyncData
                    if ((!Config.SyncLoadOnDemand && !Config.NewSyncLoadOnDemand) || Config.RecalculateRoutesOnSyncData)
                        RouteEx.Clear();

                    if (Config.UseFutureRouteEx)
                        GetFutureRouteEx();

                    if (Config.SetParLevel)
                    {
                        now = DateTime.Now;
                        try
                        {
                            string tmp = Path.Combine(Config.DataPath, "reading_tmp.xml");
                            netaccess.WriteStringToNetwork("GetParLevelCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                            netaccess.ReceiveFile(tmp);

                            DataAccess.LoadParLevels(tmp);

                            if (File.Exists(tmp))
                                File.Delete(tmp);
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Error getting GetParLevelCommand. " + ee.Message);
                            LoadingData = false;
                            return "Error Downloading Salesman Par Levels";
                        }
                        Logger.CreateLog("Salesman Par Level processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    GetReasons();

                    var customerListString = Client.GetCustomerListString();

                    if (Config.ClientDailyPL || Config.SalesByDepartment)
                    {
                        now = DateTime.Now;
                        if (!string.IsNullOrEmpty(customerListString))
                        {
                            try
                            {
                                if (!DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "12.7.0.0"))
                                {
                                    Logger.CreateLog("ClientDailyParLevel connection closed. Communicator Version " + DataAccess.CommunicatorVersion);
                                }
                                else
                                {
                                    if (File.Exists(Config.DailyParLevelFile))
                                        File.Delete(Config.DailyParLevelFile);

                                    netaccess.WriteStringToNetwork("ClientDailyParLevelGetCommand");
                                    netaccess.WriteStringToNetwork(customerListString);

                                    netaccess.ReceiveFile(Config.DailyParLevelFile);

                                    DataAccess.LoadClientDailyParLevel(Config.DailyParLevelFile);
                                }
                            }
                            catch (Exception ee)
                            {
                                Logger.CreateLog(ee);
                                LoadingData = false;
                                return "Error Downloading Client Daily Par Levels";
                            }

                            // Get Client Daily Par Level History
                            try
                            {
                                netaccess.WriteStringToNetwork("ClientDailyParLevelHistoryCommand");
                                netaccess.WriteStringToNetwork(Config.ParLevelHistoryDays.ToString(CultureInfo.InvariantCulture));

                                netaccess.ReceiveFile(Config.ParLevelHistoryFile);

                                DataAccess.LoadParLevelHistory();
                            }
                            catch (Exception ee)
                            {
                                Logger.CreateLog(ee);
                                LoadingData = false;
                                return "Error Downloading Client Daily Par Level Histories";
                            }
                        }
                        else
                            Logger.CreateLog("No par level requested because client list is empty");
                        Logger.CreateLog("Client Par Level processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    if (Config.UseFullTemplate && !string.IsNullOrEmpty(customerListString))
                    {
                        now = DateTime.Now;

                        if (File.Exists(Config.OrderHistoryFile))
                            File.Delete(Config.OrderHistoryFile);

                        // Get Order Details Projection History
                        try
                        {
                            if (Config.OrderHistoryByClient)
                                netaccess.WriteStringToNetwork("GetOrderDetailHistoryByClientCommand");
                            else
                                netaccess.WriteStringToNetwork("GetOrderDetailHistoryCommand");

                            netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + customerListString);

                            netaccess.ReceiveFile(Config.OrderHistoryFile);

                            DataAccess.LoadOrderHistory();
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Error getting OrderHistoryCommand. " + ee.Message);
                            LoadingData = false;
                            return "Error Downloading Transaction Histories";
                        }
                        Logger.CreateLog("Order History processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    if (Config.GenerateProjection && !string.IsNullOrEmpty(customerListString))
                    {
                        now = DateTime.Now;

                        if (File.Exists(Config.ProjectionFile))
                            File.Delete(Config.ProjectionFile);

                        // Get Projection
                        try
                        {
                            netaccess.WriteStringToNetwork("GetProjectionCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + customerListString);

                            netaccess.ReceiveFile(Config.ProjectionFile);

                            DataAccess.LoadProjectionValues();
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Error getting Projection. " + ee.Message);
                            LoadingData = false;
                            return "Error Downloading Projections";
                        }
                        Logger.CreateLog("Projection processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    if (Config.SalesByDepartment && !string.IsNullOrEmpty(customerListString))
                    {
                        now = DateTime.Now;
                        var tempFile = Path.GetTempFileName();

                        try
                        {
                            netaccess.WriteStringToNetwork("GetClientDepartmentsCommand");
                            netaccess.WriteStringToNetwork(customerListString);

                            netaccess.ReceiveFile(tempFile);

                            DataAccess.LoadClientDepartsFile(tempFile);
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Error getting GetClientDepartmentsCommand. " + ee.Message);
                            LoadingData = false;
                            return "Error Downloading Client Departments";
                        }

                        if (File.Exists(tempFile))
                            File.Delete(tempFile);

                        Logger.CreateLog("Client Departments processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    if (Config.AssetTracking)
                    {
                        if (File.Exists(Config.AssetTrackingHistoriesFile))
                            File.Delete(Config.AssetTrackingHistoriesFile);

                        now = DateTime.Now;

                        try
                        {
                            netaccess.WriteStringToNetwork("GetAssetTrackingCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                            netaccess.ReceiveFile(Config.AssetTrackingHistoriesFile);

                            AssetTrackingHistory.Load();
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex);
                            LoadingData = false;
                            return "Error Downloading Assets";
                        }

                        Logger.CreateLog("Assets processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    }

                    if (Config.NewSyncLoadOnDemand)
                    {
                        netaccess.WriteStringToNetwork("GetRouteOrdersCountCommand");
                        var showAll = Config.ShowAllAvailableLoads ? "1" : "0";
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + DateTime.Today.ToString(CultureInfo.InvariantCulture) + "," + showAll);
                        string result = netaccess.ReadStringFromNetwork();
                        int routeCount = 0;
                        int.TryParse(result, out routeCount);
                        DataAccess.RouteOrdersCount = routeCount;

                        try
                        {
                            string deliveriesInSite = System.IO.Path.GetTempFileName();
                            netaccess.WriteStringToNetwork("GetDeliveriesInSalesmanSiteCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ",yes");
                            netaccess.ReceiveFile(deliveriesInSite);

                            DataAccess.LoadDeliveriesInSite(deliveriesInSite);
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex);
                            LoadingData = false;
                            return "Error Downloading the deliveries in route";
                        }
                    }

                    if (updateInventory)
                    {
                        if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "13.0.0.0"))
                        {
                            now = DateTime.Now;

                            if (File.Exists(Config.InventoryOnDemandStoreFile))
                                File.Delete(Config.InventoryOnDemandStoreFile);

                            if (!Config.UsePairLotQty)
                            {
                                try
                                {
                                    netaccess.WriteStringToNetwork("InventorySiteCommand");
                                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                                    netaccess.ReceiveFile(Config.InventoryOnDemandStoreFile);

                                    LoadInventoryOnDemand();
                                }
                                catch (Exception ee)
                                {
                                    Logger.CreateLog("Error getting InventorySiteCommand. " + ee.Message);
                                    LoadingData = false;
                                    return "Error Downloading Inventories";
                                }
                            }
                            else
                            {
                                try
                                {
                                    netaccess.WriteStringToNetwork("SalesmanProductLotsCommand");
                                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                                    netaccess.ReceiveFile(Config.InventoryOnDemandStoreFile);

                                    LoadInventoryOnDemandForLot();
                                }
                                catch (Exception ee)
                                {
                                    Logger.CreateLog("Error getting SalesmanProductLotsCommand. " + ee.Message);
                                    LoadingData = false;
                                    return "Error Downloading Inventories";
                                }
                            }

                            Logger.CreateLog("InventoryOnDemand processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                        }
                    }

                    if (DataAccess.CommunicatorVersion == "12.93")
                    {
                        try
                        {
                            // Get delivery file
                            if (Config.Delivery && getDeliveries && !Config.SyncLoadOnDemand && !Config.NewSyncLoadOnDemand)
                            {
                                now = DateTime.Now;
                                netaccess.WriteStringToNetwork("RouteInformation");
                                netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + DateTime.Today.ToString(CultureInfo.InvariantCulture) + "," + (updateInventory ? "yes" : "no"));
                                var tempFile = Config.DeliveryFile;
                                netaccess.ReceiveFile(tempFile);

                                ProcessDeliveryFile(tempFile, true, updateInventory);

                                Logger.CreateLog("Delivery processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                            }
                        }
                        catch (Exception e51)
                        {
                            Logger.CreateLog(e51);
                        }

                        //for yogusto only
                        if (updateInventory)
                        {
                            try
                            {
                                netaccess.WriteStringToNetwork("SalesmanProductLotsCommand");
                                netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                                netaccess.ReceiveFile(Config.InventoryOnDemandStoreFile);

                                LoadInventoryOnDemandForLot();

                                DataAccess.PendingLoadToAccept = false;
                            }
                            catch (Exception ee)
                            {
                                Logger.CreateLog("Error getting SalesmanProductLotsCommand. " + ee.Message);
                                LoadingData = false;
                                return "Error Downloading Inventories";
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            // Get delivery file
                            if (Config.Delivery && getDeliveries && !Config.SyncLoadOnDemand && !Config.NewSyncLoadOnDemand && !Config.UsePairLotQty)
                            {
                                now = DateTime.Now;
                                netaccess.WriteStringToNetwork("RouteInformation");
                                netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + DateTime.Today.ToString(CultureInfo.InvariantCulture) + "," + (updateInventory ? "yes" : "no"));
                                var tempFile = Config.DeliveryFile;
                                netaccess.ReceiveFile(tempFile);

                                ProcessDeliveryFile(tempFile, true, updateInventory);

                                Logger.CreateLog("Delivery processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                            }
                        }
                        catch (Exception e51)
                        {
                            Logger.CreateLog(e51);
                        }
                    }

                    if (RouteEx.Routes.Count > 0)
                        LockRouteEx(netaccess);

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (Config.UpdateInventoryRegardless)
                    {
                        UpdateInventoryBasedOnTransactions();
                    }

                    if (OrderDiscount.HasDiscounts)
                        BackgroundDataSync.AssignDefaultUomInBackground();

                    GetTrucks();

                    if (Config.ButlerCustomization)
                    {
                        GetRouteRelations();
                    }

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "45.0.0"))
                        GetBanks();

                    if (Config.ShowOrderStatus)
                        GetOrderStatus();

                    if (Config.BringBranchInventories || Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales || Config.MilagroCustomization || Config.DicosaCustomization)
                    {
                        GetSites();
                        GetInventoryForSite();
                    }

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "42.4.0") && Config.CanModifyQuotes)
                        GetQuotesCreated();

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.0.0") && Config.ViewGoals)
                    {
                        LoadGoalProgress();
                        LoadGoalProgressDetail();
                    }

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.0.0"))
                        GetTerms();

                    if (Order.Orders.Count > 0)
                        FixOrders();

                    if (Config.SAPOrderStatusReport)
                        GetSapOrderStatus();

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0"))
                    {
                        var deliveries = Order.Orders.Where(x => x.IsDelivery && (x.SignaturePoints == null || x.SignaturePoints.Count == 0));
                        foreach (var d in deliveries)
                            DataAccess.GetDeliverySignature(d);
                    }

                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0.0"))
                        DataAccess.GetCompaniesInfo();
                    
                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0"))
                        GetProductLabelDecoder();
                    
                    DataAccess.ReceivedData = true;
                    Config.SaveAppStatus();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog("Main try");
                Logger.CreateLog(e);
                LoadingData = false;
                throw new Exception("Error Downloading Data");
            }

            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time downloading : " + ts.TotalSeconds);

            foreach (var r in RouteEx.Routes)
                if (r.Order != null && ordersId.Contains(r.Order.UniqueId))
                    ordersId.Remove(r.Order.UniqueId);

            List<Order> toRemoveOrders = new List<Order>();
            foreach (var order in Order.Orders)
                if (ordersId.Contains(order.UniqueId))
                    toRemoveOrders.Add(order);


            foreach (var order in toRemoveOrders)
                if (!order.Finished && !order.Modified)
                    order.Delete();

            List<Batch> toRemoveBatch = Batch.List.Where(x => x.Orders().Count == 0).ToList();
            foreach (var batch in toRemoveBatch)
                if (batch.Orders().Count == 0)
                    Batch.List.Remove(batch);

            // finally , remove any load order
            foreach (var order in Order.Orders.Where(x => x.OrderType == OrderType.Load && x.PendingLoad).ToList())
            {
                var r = RouteEx.Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == order.OrderId);
                if (r != null)
                    RouteEx.Routes.Remove(r);
                order.Delete();
                var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
                if (batch != null)
                    batch.Delete();
            }

            RouteEx.Save();

            if (Config.SupervisorId == 0)
            {
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (salesman != null &&
                    !string.IsNullOrEmpty(salesman.ExtraProperties) &&
                    salesman.ExtraProperties.ToLowerInvariant().Contains("supervisorsm"))
                {
                    Config.SupervisorId = Config.SalesmanId;
                    Config.SaveSettings();
                }
            }

            if (updateInventory)
            {
                if (!Config.TrackInventory)
                    foreach (var item in Product.Products)
                    {
                        item.ProductInv.ClearProductInventory();
                        item.UpdateInventory(item.CurrentWarehouseInventory, null, 1, item.Weight);
                    }

                SaveInventory();
            }

            SaveLastSync();
            
            LoadingData = false;

            if (Config.SiteId == 0)
            {
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (salesman != null)
                {
                    Config.SiteId = salesman.InventorySiteId;
                    Config.SaveSettings();
                }
            }

            if (Config.SalesmanCanChangeSite)
            {
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (salesman != null)
                {
                    Config.SalesmanSelectedSite = salesman.InventorySiteId;
                    Config.SaveSettings();
                }
            }

            if (Config.RecalculateOrdersAfterSync)
            {
                foreach (var o in Order.Orders)
                {
                    if (Config.Simone)
                        o.SimoneCalculateDiscount();
                    else
                        o.RecalculateDiscounts();

                    o.Save();
                }
            }

            return null;
        }


        #region  product label decoder 

        
        private static void GetProductLabelDecoder()
        {
            if(File.Exists(Config.ProductLabelFormatPath))
                File.Delete(Config.ProductLabelFormatPath);
            
            Logger.CreateLog("Getting Product Label Decoders");
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetLabelDecodersCommand");

                    var confirmation = netaccess.ReadStringFromNetwork();
                    if (confirmation != "sending")
                        return;

                    netaccess.ReceiveFile(Config.ProductLabelFormatPath);
                    
                    LoadProductLabelDecoder();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog("Error getting GetLabelDecodersCommand. " + ee.ToString());
            }
            finally
            {
            }
        }

        public static void LoadProductLabelDecoder()
        {
            if(!File.Exists(Config.ProductLabelFormatPath))
                return;
            
            try
            {
                using (StreamReader reader = new StreamReader(Config.ProductLabelFormatPath))
                {
                    string currentline;
                    int currenttable = -1;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        switch (currentline)
                        {
                            case "EndOfTable":
                                currenttable = -1;
                                continue;
                            case "ProductLabel":
                                ProductLabel.ProductLabels.Clear();
                                currenttable = 1;
                                continue;
                            case "ProductLabelParameter":
                                ProductLabelParameter.ProductLabelParameters.Clear();
                                currenttable = 2;
                                continue;
                            case "ProductLabelParameterValue":
                                ProductLabelParameterValue.ProductLabelParameterValues.Clear();
                                currenttable = 3;
                                continue;
                            case "ProductLabelProduct":
                                ProductLabelProduct.ProductLabelProducts.Clear();
                                currenttable = 4;
                                continue;
                            case "ProductLabelVendor":
                                ProductLabelVendor.ProductLabelVendors.Clear();
                                currenttable = 5;
                                continue;
                        }

                        string[] currentrow = null; // currentline.Split(DataLineSplitter);

                        if (currenttable == 24 && !currentline.Contains((char)20)) continue;

                        currentrow = currentline.Split(DataAccess.DataLineSplitter);
                        switch (currenttable)
                        {
                            case 1:
                                CreateProductLabel(currentrow);
                                continue;
                            case 2:
                                CreateProductLabelParameter(currentrow);
                                continue;
                            case 3:
                                CreateProductLabelParameterValue(currentrow);
                                continue;
                            case 4:
                                CreateProductLabelProduct(currentrow);
                                continue;
                            case 5:
                                CreateProductLabelVendor(currentrow);
                                continue;
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex.ToString());
            }
        }

        private static void CreateProductLabelVendor(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var vendorId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var productLabelId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var extraFields = currentrow[3];

            var item = new ProductLabelVendor()
            {
                Id = id,
                VendorId = vendorId,
                ProductLabelId = productLabelId,
                ExtraFields = extraFields
            };

            ProductLabelVendor.ProductLabelVendors.Add(item);

            var label = ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == productLabelId);
            if (label != null)
                label.ProductLabelVendors.Add(item);
        }

        private static void CreateProductLabelProduct(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var productId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var productLabelId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var extraFields = currentrow[3];

            var item = new ProductLabelProduct()
            {
                Id = id,
                ProductId = productId,
                ProductLabelId = productLabelId,
                ExtraFields = extraFields
            };

            ProductLabelProduct.ProductLabelProducts.Add(item);

            var label = ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == productLabelId);
            if (label != null)
                label.ProductLabelProducts.Add(item);
        }

        private static void CreateProductLabelParameterValue(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var labelId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var parameterId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var position = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
            var format = currentrow[4];
            var qty = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture);

            var item = new ProductLabelParameterValue()
            {
                Id = id,
                LabelId = labelId,
                ParameterId = parameterId,
                Position = position,
                Format = format,
                Qty = qty,
            };

            ProductLabelParameterValue.ProductLabelParameterValues.Add(item);

            var label = ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == labelId);
            if (label != null)
                label.ProductLabelParameterValues.Add(item);
        }

        private static void CreateProductLabelParameter(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var name = currentrow[1];
            var format = currentrow[2];
            var qty = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
            var single = Convert.ToInt32(currentrow[4]) > 0;
            var type = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture);

            ProductLabelParameter.ProductLabelParameters.Add(
                new ProductLabelParameter()
                {
                    Id = id,
                    Name = name,
                    Format = format,
                    Qty = qty,
                    Single = single,
                    Type = type
                });
        }

        private static void CreateProductLabel(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var name = currentrow[1];
            var category = currentrow[2];
            var active = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture) > 0;
            var comments = currentrow[4];
            var extraFields = currentrow[5];
            var defaultVendor = Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
            var labelType = Convert.ToInt32(currentrow[7], CultureInfo.InvariantCulture);

            ProductLabel.ProductLabels.Add(
                new ProductLabel()
                {
                    Id = id,
                    Name = name,
                    Category = category,
                    Active = active,
                    Comments = comments,
                    ExtraFields = extraFields,
                    DefaultVendorId = defaultVendor,
                    LabelType = labelType
                });
        }

        #endregion

        public static void SaveLastSync()
        {
            if (File.Exists(Config.LastSyncDate))
                File.Delete(Config.LastSyncDate);

            //FileOperationsLocker.InUse = true;
            using (StreamWriter writer = new StreamWriter(Config.LastSyncDate))
            {
                writer.WriteLine(DateTime.Now.ToUniversalTime().Ticks);
                writer.Close();
            }
        }

        public static DateTime LoadLastSync()
        {
            if (!File.Exists(Config.LastSyncDate))
                return DateTime.MinValue;

            using (StreamReader reader = new StreamReader(Config.LastSyncDate))
            {
                var line = reader.ReadLine();
                if (line != null && !string.IsNullOrEmpty(line))
                {
                    var longDate = Convert.ToInt64(line);
                    return new DateTime(longDate);
                }

                reader.Close();
            }

            return DateTime.MinValue;
        }

        private static void GetSapOrderStatus()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetSapOrderStatusCommand");

                    var statuses = access.ReadStringFromNetwork();

                    access.CloseConnection();

                    SapStatus.Save(statuses);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error loading banks" + ex.ToString());
            }
        }

        public static void FixOrders()
        {
            foreach (var o in Order.Orders)
            {
                bool wasChanged = false;

                foreach (var d in o.Details)
                {
                    if (d.UnitOfMeasure != null && UnitOfMeasure.InactiveUoM.Any(x => x.Id == d.UnitOfMeasure.Id))
                    {
                        var units = UnitOfMeasure.List.Where(x => x.FamilyId == d.Product.UoMFamily);

                        if (units != null && units.Count() > 0)
                        {
                            var similar = units.FirstOrDefault(x => x.Conversion == d.UnitOfMeasure.Conversion);
                            if (similar != null)
                            {
                                d.UnitOfMeasure = similar;
                                wasChanged = true;
                            }
                        }
                    }
                }

                if (wasChanged)
                    o.Save();
            }
        }

        private static void LoadTerms()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Config.TermsPath))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);

                        Term term = new Term();
                        term.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                        term.DiscountPercentage = Convert.ToDouble(currentrow[1]);
                        term.ExtraFields = currentrow[2];
                        term.IsActive = Convert.ToInt32(currentrow[3]) > 0;
                        term.Name = currentrow[4];
                        term.OriginalId = currentrow[5];
                        term.StandardDiscountDays = Convert.ToInt32(currentrow[6]);
                        term.StandardDueDates = Convert.ToInt32(currentrow[7]);

                        Term.List.Add(term);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private static void GetTerms()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    Term.List.Clear();

                    if (File.Exists(Config.TermsPath))
                        File.Delete(Config.TermsPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetPaymentTermsCommand");

                    access.ReceiveFile(Config.TermsPath);

                    access.CloseConnection();

                    LoadTerms();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error loading banks" + ex.ToString());
            }
        }

        private static void GetOrderStatus()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    OrdersInOS.List.Clear();

                    if (File.Exists(Config.OrderStatusPath))
                        File.Delete(Config.OrderStatusPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetOrderStatusCommand");

                    access.WriteStringToNetwork(Config.SalesmanId + "," + Config.DaysToBringOrderStatus);

                    access.ReceiveFile(Config.OrderStatusPath);

                    access.CloseConnection();

                    LoadOrdersStatus();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error loading banks" + ex.ToString());
            }
        }

        private static void LoadOrdersStatus()
        {
            try
            {
                OrdersInOS.List.Clear();

                if (!File.Exists(Config.OrderStatusPath))
                    return;

                using (StreamReader reader = new StreamReader(Config.OrderStatusPath))
                {
                    string line;
                    bool nextIsOrder = false;
                    bool nextIsDetail = false;
                    OrdersInOS order = null;

                    var allOrders = new List<OrdersInOS>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "EndOfOrder")
                        {
                            OrdersInOS.List.Add(order);
                        }

                        if (line == "NewOrder")
                        {
                            nextIsDetail = false;
                            nextIsOrder = true;
                            continue;
                        }
                        if (line == "OrderDetails")
                        {
                            nextIsDetail = true;
                            continue;
                        }

                        if (nextIsOrder)
                        {
                            string[] parts = line.Split(new char[] { (char)20 });

                            order = CreateOrderStatus(parts);
                            nextIsOrder = false;
                            continue;
                        }

                        if (nextIsDetail)
                        {
                            string[] parts = line.Split(new char[] { (char)20 });

                            CreateOrderStatusDetails(parts, order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void CreateOrderStatusDetails(string[] currentrow, OrdersInOS order)
        {
            if (order == null)
                return;

            try
            {
                var product = Product.Find(Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture));
                if (product == null)
                {
                    Logger.CreateLog("product with ID " + currentrow[2] + " was not found");
                    return;
                }

                var qty = Convert.ToSingle(currentrow[3], CultureInfo.InvariantCulture);
                var price = Convert.ToDouble(currentrow[4], CultureInfo.InvariantCulture);
                var detail = new StatusOrderDetail();
                detail.Product = product;
                detail.Order = order;
                detail.Price = price;
                detail.ExpectedPrice = price;
                detail.Comments = currentrow[5];

                try
                {
                    if (currentrow.Length > 17)
                        detail.Weight = (float)Convert.ToDouble(currentrow[17]);
                    else
                        detail.Weight = qty;
                }
                catch
                {
                    detail.Weight = qty;
                }

                if (product.SoldByWeight)
                {
                    detail.Qty = 1;
                    detail.Ordered = 1;
                    detail.Weight = detail.Weight;

                    if (product.FixedWeight && Config.NewAddItemRandomWeight)
                    {
                        detail.Weight = (float)product.Weight;
                    }
                }
                else
                {
                    detail.Qty = qty;
                    detail.Ordered = qty;
                    detail.Weight = 0;
                }

                if (!string.IsNullOrEmpty(currentrow[6]))
                    detail.FromOffer = Convert.ToBoolean(currentrow[6], CultureInfo.InvariantCulture);
                else
                    detail.FromOffer = false;

                if (currentrow.Length > 7)
                    detail.OriginalId = currentrow[7];
                else
                    detail.OriginalId = string.Empty;
                if (currentrow.Length > 8)
                    detail.IsCredit = Convert.ToInt32(currentrow[8], CultureInfo.InvariantCulture) > 0;
                else
                    detail.IsCredit = false;
                if (currentrow.Length > 9)
                {
                    var uomid = Convert.ToInt32(currentrow[9], CultureInfo.InvariantCulture);
                    var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomid);
                    if (uom != null)
                        detail.UnitOfMeasure = uom;
                    else
                        detail.UnitOfMeasure = null;
                }
                else
                    detail.UnitOfMeasure = null;
                if (currentrow.Length > 10)
                {
                    detail.Damaged = Convert.ToInt32(currentrow[10], CultureInfo.InvariantCulture) > 0;
                }
                else
                    detail.Damaged = false;

                if (currentrow.Length > 11)
                {
                    detail.Lot = currentrow[11];
                }
                else
                    detail.Lot = "";

                if (currentrow.Length > 12)
                {
                    detail.ExtraFields = currentrow[12];
                }
                else
                    detail.ExtraFields = string.Empty;

                order.Details.Add(detail);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        private static OrdersInOS CreateOrderStatus(string[] currentrow)
        {
            try
            {
                var orderId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                var client = Client.Find(clientId);
                if (client == null)
                {
                    Logger.CreateLog(" CreateOrder had reference to a non existing client: " + currentrow[1] + " client id: " + currentrow[1]);
                    client = Client.CreateTemporalClient(clientId);
                    client.SalesmanClient = true;
                }

                var order = new OrdersInOS();
                order.OrderId = orderId;
                order.Client = client;
                order.OriginalOrderId = orderId;
                order.OriginalSalesmanId = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture); //original salesman Id
                order.Date = Convert.ToDateTime(currentrow[5], CultureInfo.InvariantCulture);
                order.OrderType = (OrderType)Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
                order.OrderStatus = (OrderStatus)Convert.ToInt32(currentrow[7], CultureInfo.InvariantCulture);
                order.PrintedOrderId = currentrow[9];

                var batchId = Convert.ToInt32(currentrow[20], CultureInfo.InvariantCulture);
                order.Comments = currentrow[12];
                order.CompanyName = string.Empty;
                order.Latitude = 0;
                order.Longitude = 0;
                order.TaxRate = Convert.ToSingle(currentrow[14], CultureInfo.InvariantCulture);
                order.PONumber = currentrow[18];
                order.SalesmanId = Config.SalesmanId;
                order.ShipDate = Convert.ToDateTime(currentrow[19], CultureInfo.InvariantCulture);

                order.UniqueId = currentrow[21];
                order.ExtraFields = currentrow[15];

                if (currentrow.Length > 22)
                {
                    order.DiscountAmount = Convert.ToSingle(currentrow[22], CultureInfo.InvariantCulture);
                }
                if (currentrow.Length > 23)
                {
                    order.DiscountType = (DiscountType)Convert.ToInt32(currentrow[23], CultureInfo.InvariantCulture);
                }
                if (currentrow.Length > 26)
                {
                    order.DiscountComment = currentrow[26];
                }

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                    order.PrintedOrderId = orderId.ToString();
                return order;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                //Xamarin.Insights.Report(e);
                return null;
            }

        }

        private static void GetBanks()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    if (File.Exists(Config.BanksAccountFile))
                        File.Delete(Config.BanksAccountFile);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetBanksListCommand");

                    access.ReceiveFile(Config.BanksAccountFile);

                    access.CloseConnection();

                    BankAccount.LoadBanks();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error loading banks" + ex.ToString());
            }
        }

        private static void LockRouteEx(NetAccess access)
        {
            try
            {
                access.WriteStringToNetwork("LockRouteByIdCommand");

                string ids = string.Empty;
                foreach (var routes in RouteEx.Routes)
                {
                    ids += routes.Id + "|";
                }

                ids = ids.Remove(ids.Length - 1);

                access.WriteStringToNetwork(ids);
            }
            catch (Exception ex)
            {
            }
        }

        static void GetFutureRouteEx()
        {
            if (File.Exists(Config.FutureRoutesFile))
                File.Delete(Config.FutureRoutesFile);

            var now = DateTime.Now;
            try
            {
                using (NetAccess access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("FutureRoutingCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                    access.ReceiveFile(Config.FutureRoutesFile);

                    access.CloseConnection();

                    DataAccess.LoadFutureRoutes();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog("Error getting FutureRoutingCommand. " + ee.Message);
                //return "Error Downloading Routes";
            }

            Logger.CreateLog("Future RouteEx processed in " + DateTime.Now.Subtract(now).TotalSeconds);
        }

        static void GetReasons()
        {
            if (File.Exists(Config.ReasonsFile))
                File.Delete(Config.ReasonsFile);

            var now = DateTime.Now;
            try
            {
                using (NetAccess access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("ReasonsCommand");

                    access.ReceiveFile(Config.ReasonsFile);

                    DataAccess.LoadReasons();

                    access.CloseConnection();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog("Error getting ReasonsCommand. " + ee.Message);
                //return "Error Downloading Reasons";
            }
            Logger.CreateLog("Reasons processed in " + DateTime.Now.Subtract(now).TotalSeconds);
        }

        public static void GetSessionId()
        {
            if (!DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "35.0.0.0"))
            {
                Config.SessionId = Guid.NewGuid().ToString();
                Config.SaveSessionId();
                return;
            }

            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // send the orders
                    netaccess.WriteStringToNetwork("StartSalesmanSessionCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());

                    string s = netaccess.ReadStringFromNetwork();

                    if (s == "error")
                        throw new Exception("Error creating session");

                    Config.SessionId = s;
                    Config.SaveSessionId();

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Salesman Device Info sent");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw ex;
            }
        }

        private static void UpdateInventoryBasedOnTransactions()
        {
            foreach (var order in Order.Orders.Where(x => !x.AsPresale && !x.Reshipped && !x.Voided))
            {
                foreach (var detail in order.Details)
                {
                    if (detail.Substracted)
                        order.UpdateInventory(detail, -1);
                }
            }

            if (File.Exists(Config.TransferOnFile))
            {
                using (StreamReader reader = new StreamReader(Config.TransferOnFile))
                {
                    string line = reader.ReadLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var prodId = Convert.ToInt32(parts[2]);
                        float qty = Convert.ToSingle(parts[3]);
                        var uomId = Convert.ToInt32(parts[6]);

                        var product = Product.Find(prodId);
                        if (product == null)
                            continue;

                        double Weight = 0;
                        if (parts.Length > 10)
                            Weight = Convert.ToDouble(parts[10]);

                        var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                        product.UpdateInventory(qty, uom, 1, Weight);

                        product.AddTransferredInventory(qty, uom, 1, Weight);
                    }
                }
            }

            if (File.Exists(Config.TransferOffFile))
            {
                using (StreamReader reader = new StreamReader(Config.TransferOffFile))
                {
                    string line = reader.ReadLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var prodId = Convert.ToInt32(parts[2]);
                        float qty = Convert.ToSingle(parts[3]);
                        var uomId = Convert.ToInt32(parts[6]);

                        var product = Product.Find(prodId);
                        if (product == null)
                            continue;

                        double Weight = 0;
                        if (parts.Length > 10)
                            Weight = Convert.ToDouble(parts[10]);

                        var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                        // Match Xamarin: Transfer Off uses factor -1
                        product.UpdateInventory(qty, uom, -1, Weight);

                        product.AddTransferredInventory(qty, uom, -1, Weight);
                    }
                }
            }
        }

        public static string DownloadStaticData()
        {
            //miche request need insta refresh images everytime
            //BackgroundDataSync.GetImages();

            DataAccess.UpdateProductImagesMap();

            DateTime start = DateTime.Now;

            try
            {
                DateTime now = DateTime.Now;
                DataAccess.GetSalesmanSettings();
                Logger.CreateLog("Salesman Settings downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                bool gotUnitOfMeasure = GetUnitOfMeasures();

                Logger.CreateLog("downloading static data");
                using (NetAccess netaccess = new NetAccess())
                {
                    //Set the target files
                    string basefileP = Path.Combine(Config.DataPath, "reading_fileP.zip");
                    string targetfileP = Config.ProductStoreFile;
                    string basefileC = Path.Combine(Config.DataPath, "reading_fileC.zip");
                    string targetfileC = Config.ClientStoreFile;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("Products");

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileP) == 0)
                        return "Error Downloading Products";
                    Logger.CreateLog("Products downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    netaccess.WriteStringToNetwork("Clients");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileC) == 0)
                        return "Error Downloading Customers";
                    Logger.CreateLog("Clients downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileP, targetfileP);
                    LoadData__(Config.ProductStoreFile, true, !gotUnitOfMeasure);
                    Logger.CreateLog("Products processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileP);

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileC, targetfileC);
                    Client.DeleteClients();
                    LoadData__(Config.ClientStoreFile, true, !gotUnitOfMeasure);
                    Logger.CreateLog("Clients processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileC);

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog("Main try");
                Logger.CreateLog(e);
                throw;
            }
            finally
            {
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time downloading : " + ts.TotalSeconds);

            return null;
        }

        private static bool GetUnitOfMeasures()
        {
            if (!Config.GetUOMSOnCommand)
                return false;

            if (File.Exists(Config.UnitOfMeasuresFile))
                File.Delete(Config.UnitOfMeasuresFile);

            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    string uomFile = Path.GetTempFileName();

                    netaccess.WriteStringToNetwork("UoMsCommand");
                    netaccess.ReceiveFile(uomFile);

                    LoadUoM(uomFile);

                    netaccess.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);

                if (File.Exists(Config.UnitOfMeasuresFile))
                    File.Delete(Config.UnitOfMeasuresFile);

                return false;
            }

            return true;
        }

        private static void LoadUoM(string uomFile)
        {
            UnitOfMeasure.List.Clear();
            UnitOfMeasure.InactiveUoM.Clear();

            try
            {

                using (StreamReader reader = new StreamReader(uomFile))
                {
                    string line;
                    string[] parts;

                    while ((line = reader.ReadLine()) != null)
                    {
                        parts = line.Split(new char[] { (char)20 });

                        bool isActive = true;
                        if (parts.Length > 8)
                            isActive = Convert.ToBoolean(parts[8], CultureInfo.InvariantCulture);

                        UnitOfMeasure unit = new UnitOfMeasure();
                        unit.Id = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                        unit.Name = parts[1];
                        unit.Conversion = Convert.ToSingle(parts[2], CultureInfo.InvariantCulture);
                        unit.FamilyId = parts[3];
                        unit.IsBase = parts[4] == "1";
                        unit.IsDefault = parts[5] == "1";
                        unit.OriginalId = parts[6];

                        if (parts.Length > 7)
                            unit.DefaultPurchase = parts[7];

                        unit.IsActive = isActive; //8

                        if (parts.Length > 9)
                            unit.CreatedLocally = parts[9] == "1";

                        if (parts.Length > 10)
                            unit.FamilyName = parts[10] ?? string.Empty;

                        if (parts.Length > 11)
                            unit.ExtraFields = parts[11] ?? string.Empty;

                        if (!isActive)
                            UnitOfMeasure.InactiveUoM.Add(unit);
                        else
                            UnitOfMeasure.List.Add(unit);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private static void LoadData__(string dataFile, bool updateInventory, bool loadUnitOfMeasures)
        {
            DateTime startDate = DateTime.Now;

            bool productReaded = false;
            bool clientReaded = false;

            List<int> usedCategories = null;
            Dictionary<int, List<Product>> clientProductList = null;
            List<ProductVisibleSalesman> visibleProducts = new List<ProductVisibleSalesman>();

            using (StreamReader reader = new StreamReader(dataFile))
            {
                string currentline;
                int currenttable = -1;
                int c;
                int readedEntities = 0;
                while ((currentline = reader.ReadLine()) != null)
                {
                    switch (currentline)
                    {
                        case "EndOfTable":
                            currenttable = -1;
                            continue;
                        case "DefaultInventory":
                            foreach (var item in Product.Products)
                                item.ProductInv.WarehouseInventory = 0;
                            currenttable = 1;
                            continue;
                        case "ProductTaxability":
                            currenttable = 2;
                            continue;
                        case "RetailPriceLevels":
                            currenttable = 3;
                            RetailPriceLevel.Clear(10000);
                            continue;
                        case "ProductVisibleSalesman":
                            currenttable = 4;
                            continue;
                        case "Consignment":
                            currenttable = 5;
                            continue;
                        case "UnitOfMeasures":
                            if (loadUnitOfMeasures)
                            {
                                UnitOfMeasure.List.Clear();
                                UnitOfMeasure.InactiveUoM.Clear();
                            }
                            currenttable = 6;
                            continue;
                        case "BuildToQty":
                            BuildToQty.List.Clear();
                            currenttable = 7;
                            continue;
                        case "SiteInventory":
                            currenttable = 8;
                            continue;
                        case "Category":
                            startDate = DateTime.Now;
                            currenttable = 9;
                            c = Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture);
                            Category.Clear(c);
                            continue;
                        case "Product":
                            startDate = DateTime.Now;
                            if (usedCategories == null)
                                usedCategories = new List<int>();
                            else
                                usedCategories.Clear();
                            currenttable = 10;
                            c = Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture);
                            Product.Clear();
                            Offer.Clear(1);
                            readedEntities = 0;
                            productReaded = true;
                            continue;
                        case "Offers":
                            startDate = DateTime.Now;
                            currenttable = 11;
                            continue;
                        case "ProductPrice":
                            startDate = DateTime.Now;
                            currenttable = 12;
                            c = Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture);
                            ProductPrice.Clear(c);
                            continue;

                        case "Client":
                            startDate = DateTime.Now;
                            currenttable = 13;
                            c = Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture);
                            Client.Clear();
                            Invoice.Clear(1);
                            readedEntities = 0;
                            clientReaded = true;
                            continue;
                        case "ProductsInClientCategory":
                            ClientCategoryProducts.ResetList();
                            startDate = DateTime.Now;
                            currenttable = 14;
                            clientProductList = new Dictionary<int, List<Product>>();
                            continue;
                        case "ClientsOffers":
                            startDate = DateTime.Now;
                            currenttable = 15;
                            ClientsOffer.Clear(Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture));
                            continue;
                        case "Route":
                            startDate = DateTime.Now;
                            currenttable = 16;
                            continue;
                        case "Salesmen":
                            Salesman.Clear();
                            startDate = DateTime.Now;
                            currenttable = 17;
                            continue;
                        case "ProductLots":
                            if (updateInventory)
                                foreach (var item in Product.Products)
                                    item.ProductInv.ClearProductInventory();
                            startDate = DateTime.Now;
                            currenttable = 18;
                            continue;
                        case "ProductLotsSite":
                            startDate = DateTime.Now;
                            if (updateInventory)
                                foreach (var item in Product.Products)
                                    item.ProductInv.ClearProductInventory();
                            currenttable = 19;
                            continue;
                        case "RetailProductPrices":
                            currenttable = 20;
                            RetailProductPrice.Clear(10000);
                            continue;
                        case "ClientDailyParLevel":
                            ClientDailyParLevel.Clear();
                            currenttable = 21;
                            continue;
                        case "Branches":
                            currenttable = 22;
                            Branch.List.Clear();
                            continue;
                        case "AvailableCompany":
                            CompanyInfo.Remove(true);
                            currenttable = 23;
                            continue;
                        case "ClientAvailableCompany":
                            ClientAvailableCompany.Clear();
                            currenttable = 24;
                            continue;
                        case "SalesmanAvailableCompany":
                            SalesmanAvailableCompany.Clear();
                            currenttable = 25;
                            continue;
                        case "OpenInvoices":
                            currenttable = 26;
                            startDate = DateTime.Now;
                            c = Convert.ToInt32(reader.ReadLine(), CultureInfo.InvariantCulture);
                            Invoice.Clear(10000);

                            Invoice.InvoiceTypeDic.Clear();
                            // delete the folders
                            Directory.Delete(Config.InvoicesPath, true);
                            // recreate it
                            Directory.CreateDirectory(Config.InvoicesPath);
                            continue;
                        case "OpenInvoicesDetails":
                            //Logger.CreateLog ("Started reading OpenInvoicesDetails at " + DateTime.Now.ToString ());
                            currenttable = 27;
                            readedEntities = 0;
                            Config.HasOpenInvoiceDetails = true;
                            InvoiceDetail.Clear(40000);
                            Product.ProductsInHistory.Clear();
                            continue;

                        case "OfferEx":
                            OfferEx.List.Clear();
                            currenttable = 28;
                            continue;
                        case "DiscountCategory":
                            DiscountCategory.DiscountCategories.Clear();
                            currenttable = 29;
                            continue;
                        case "ProductOfferEx":
                            ProductOfferEx.List.Clear();
                            currenttable = 30;
                            continue;
                        case "ClientOfferEx":
                            ClientOfferEx.List.Clear();
                            currenttable = 31;
                            continue;
                        case "PriceLevel":
                            PriceLevel.List.Clear();
                            currenttable = 32;
                            continue;
                        case "CategoryProduct":
                            CategoryProduct.List.Clear();
                            currenttable = 33;
                            continue;
                        case "FileInfo":
                            currenttable = 100;
                            continue;
                        case "SuggestedClientCategories":
                            currenttable = 45;
                            SuggestedClientCategory.List.Clear();
                            continue;
                        case "SuggestedClientCategoryClient":
                            currenttable = 46;
                            SuggestedClientCategoryClient.List.Clear();
                            continue;
                        case "SuggestedClientCategoryProduct":
                            currenttable = 47;
                            SuggestedClientCategoryProduct.List.Clear();
                            continue;
                        case "OrderDiscount":
                            OrderDiscount.List.Clear();
                            currenttable = 50;
                            continue;
                        case "OrderDiscountClient":
                            OrderDiscountClient.List.Clear();
                            currenttable = 51;
                            continue;
                        case "OrderDiscountClientArea":
                            OrderDiscountClientArea.List.Clear();
                            currenttable = 52;
                            continue;
                        case "OrderDiscountProduct":
                            OrderDiscountProduct.List.Clear();
                            currenttable = 53;
                            continue;
                        case "OrderDiscountVendor":
                            currenttable = 54;
                            continue;
                        case "OrderDiscountBreak":
                            currenttable = 55;
                            continue;
                        case "OrderDiscountProductBreak":
                            currenttable = 56;
                            continue;
                        case "OrderDiscountVendorBreak":
                            currenttable = 57;
                            continue;
                        case "Area":
                            Area.List.Clear();
                            currenttable = 58;
                            continue;
                        case "Vendor":
                            Vendor.List.Clear();
                            currenttable = 59;
                            continue;
                        case "AreaClient":
                            AreaClient.List.Clear();
                            currenttable = 60;
                            continue;
                        case "OrderDiscountCategory":
                            OrderDiscountCategory.List.Clear();
                            currenttable = 61;
                            continue;
                        case "OrderDiscountCategoryBreak":
                            currenttable = 62;
                            continue;
                        case "ClientCategory":
                            ClientCategoryEx.List.Clear();
                            currenttable = 63;
                            continue;
                        case "ClientDepartmentGroups":
                            ClientDepartmentGroup.List.Clear();
                            currenttable = 64;
                            continue;
                        case "ClientClientDepartmentGroups":
                            ClientClientDepartmentGroup.List.Clear();
                            currenttable = 65;
                            continue;
                        case "DepartmentClientDepartmentGroups":
                            DepartmentClientDepartmentGroup.List.Clear();
                            currenttable = 66;
                            continue;
                        case "DepartmertClientCategories":
                            DepartmertClientCategory.List.Clear();
                            currenttable = 67;
                            continue;
                        case "DepartmentProduct":
                            DepartmentProduct.List.Clear();
                            currenttable = 68;
                            continue;
                        case "OrderDiscountClientPriceLevels":
                            OrderDiscountClientPriceLevel.List.Clear();
                            currenttable = 69;
                            continue;
                        case "ProductVisibleCompany":
                            ProductVisibleCompany.List.Clear();
                            currenttable = 70;
                            continue;
                        case "Assets":
                            Asset.List.Clear();
                            currenttable = 71;
                            continue;
                        case "ClientAssetTrack":
                            ClientAssetTrack.List.Clear();
                            currenttable = 72;
                            continue;
                        case "ProductAllowedSites":
                            ProductAllowedSite.List.Clear();
                            currenttable = 73;
                            continue;
                    }

                    string[] currentrow = null;// currentline.Split(DataLineSplitter);

                    if (currenttable == 24 && !currentline.Contains((char)20))
                        continue;

                    currentrow = currentline.Split(DataAccess.DataLineSplitter);
                    switch (currenttable)
                    {
                        case 1:
                            CreateDefaultInventory(currentrow);
                            continue;
                        case 2:
                            DataAccess.CreateProductTaxability(currentrow);
                            continue;
                        case 3:
                            DataAccess.CreateRetailPriceLevel(currentrow);
                            continue;
                        case 4:
                            visibleProducts.Add(DataAccess.CreateProductVisibleToClient(currentrow));
                            continue;
                        case 5:
                            DataAccess.CreateConsignment(currentrow);
                            continue;
                        case 6:
                            if (loadUnitOfMeasures)
                                DataAccess.CreateUnitOfMeasure(currentrow);
                            continue;
                        case 7:
                            DataAccess.LoadBuildToQty(currentrow);
                            continue;
                        case 8:
                            if (updateInventory && !Config.UsePairLotQty)
                                LoadInventorySite(currentrow);
                            continue;
                        case 9:
                            DataAccess.CreateCategory(currentrow);
                            continue;
                        case 10:
                            if (Config.IsTest)
                                if (readedEntities == 20)
                                    continue;
                            readedEntities++;
                            CreateProduct(usedCategories, currentrow);
                            continue;
                        case 11:
                            DataAccess.CreateOffer(currentrow);
                            continue;
                        case 12:
                            DataAccess.CreateProductPrice(currentrow);
                            continue;
                        case 13:
                            if (Config.IsTest)
                                if (readedEntities == DataAccess.EntitiesToReadInTestMode)
                                    continue;
                            readedEntities++;
                            DataAccess.CreateClient(currentrow, false);
                            break;
                        case 14:
                            if (currentrow.Length == 1)
                                continue;
                            int catId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                            int productId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                            if (!clientProductList.ContainsKey(catId))
                                clientProductList.Add(catId, new List<Product>());
                            Product p = Product.Find(productId);
                            if (p != null)
                                clientProductList[catId].Add(p);
                            break;
                        case 15:
                            DataAccess.CreateClientOffer(currentrow);
                            break;
                        case 16:
                            DataAccess.CreateRoute(currentrow);
                            continue;
                        case 17:
                            DataAccess.CreateSalesman(currentrow);
                            continue;
                        case 18:
                            if (updateInventory)
                                CreateProductLot(currentrow);
                            continue;
                        case 19:
                            if (updateInventory)
                                CreateProductLot(currentrow);
                            continue;
                        case 20:
                            DataAccess.CreateRetailProductPrices(currentrow);
                            continue;
                        case 21:
                            DataAccess.CreateClientDailyParLevel(currentrow);
                            continue;
                        case 22:
                            DataAccess.CreateBranch(currentrow);
                            continue;
                        case 23:
                            DataAccess.CreateCompanyInfo(currentrow);
                            continue;
                        case 24:
                            DataAccess.CreateClientAvailableCompany(currentrow);
                            continue;
                        case 25:
                            DataAccess.CreateSalesmanAvailableCompany(currentrow);
                            continue;
                        case 26:
                            var clientPath = Path.Combine(Config.InvoicesPath, currentrow[DataAccess.OpenInvoiceClientIdIndex]);
                            if (!Directory.Exists(clientPath))
                                Directory.CreateDirectory(clientPath);

                            var finalInvoiceFile = Path.Combine(clientPath, "invoices.xml");
                            using (var writer = new StreamWriter(finalInvoiceFile, true))
                                writer.WriteLine(currentline);

                            DataAccess.CreateInvoice(currentrow);
                            continue;
                        case 27:
                            var clientPath_ = Path.Combine(Config.InvoicesPath, currentrow[5]);
                            if (!Directory.Exists(clientPath_))
                                Directory.CreateDirectory(clientPath_);
                            var finalInvoiceFile_ = Path.Combine(clientPath_, "invoices.xml");
                            using (var writer = new StreamWriter(finalInvoiceFile_, true))
                            {
                                if (readedEntities == 0)
                                    writer.WriteLine("EndOfTable");
                                writer.WriteLine(currentline);
                            }

                            if (Config.LoadByOrderHistory)
                            {
                                int x = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
                                if (!Product.ProductsInHistory.ContainsKey(x))
                                {
                                    Product prod = Product.Find(x);
                                    if (prod != null)
                                        Product.ProductsInHistory.Add(x, prod);
                                }
                            }
                            continue;
                        case 28:
                            DataAccess.CreateOfferEx(currentrow);
                            continue;
                        case 29:
                            DataAccess.CreateDiscountCategory(currentrow);
                            continue;
                        case 30:
                            DataAccess.CreateProductOfferEx(currentrow);
                            continue;
                        case 31:
                            DataAccess.CreateClientOfferEx(currentrow);
                            continue;
                        case 32:
                            DataAccess.CreatePriceLevel(currentrow);
                            continue;
                        case 33:
                            DataAccess.CreateCategoryProduct(currentrow);
                            continue;
                        case 100:
                            try
                            {
                                long ticks = 0;
                                Int64.TryParse(currentrow[0], out ticks);
                                Config.ProductFileCreationDate = ticks;
                            }
                            catch (Exception ex)
                            {
                                Config.ProductFileCreationDate = 0;
                            }
                            continue;
                        case 45:
                            DataAccess.CreateSuggestedClientCategory(currentrow);
                            continue;
                        case 46:
                            DataAccess.CreateSuggestedClientCategoryClient(currentrow);
                            continue;
                        case 47:
                            DataAccess.CreateSuggestedClientCategoryProduct(currentrow);
                            continue;
                        case 50:
                            DataAccess.CreateOrderDiscount(currentrow);
                            continue;
                        case 51:
                            DataAccess.CreateOrderDiscountClient(currentrow);
                            continue;
                        case 52:
                            DataAccess.CreateOrderDiscountClientArea(currentrow);
                            continue;
                        case 53:
                            DataAccess.CreateOrderDiscountProduct(currentrow);
                            continue;
                        case 54:
                            DataAccess.CreateOrderDiscountVendor(currentrow);
                            continue;
                        case 55:
                            DataAccess.CreateOrderDiscountBreak(currentrow);
                            continue;
                        case 56:
                            DataAccess.CreateOrderDiscountProductBreak(currentrow);
                            continue;
                        case 57:
                            DataAccess.CreateOrderDiscountVendorBreak(currentrow);
                            continue;
                        case 58:
                            DataAccess.CreateArea(currentrow);
                            continue;
                        case 59:
                            DataAccess.CreateVendor(currentrow);
                            continue;
                        case 60:
                            DataAccess.CreateAreaClient(currentrow);
                            continue;
                        case 61:
                            DataAccess.CreateOrderDiscountCategory(currentrow);
                            continue;
                        case 62:
                            DataAccess.CreateOrderDiscountCategoryBreak(currentrow);
                            continue;
                        case 63:
                            DataAccess.CreateClientCategoryEx(currentrow);
                            continue;
                        case 64:
                            DataAccess.CreateClientDepartmentGroup(currentrow);
                            continue;
                        case 65:
                            DataAccess.CreateClientClientDepartmentGroup(currentrow);
                            continue;
                        case 66:
                            DataAccess.CreateDepartmentClientDepartmentGroup(currentrow);
                            continue;
                        case 67:
                            DataAccess.CreateDepartmertClientCategories(currentrow);
                            continue;
                        case 68:
                            DataAccess.CreateDepartmentProduct(currentrow);
                            continue;
                        case 69:
                            DataAccess.CreateOrderDiscountClientPriceLevel(currentrow);
                            continue;
                        case 70:
                            DataAccess.CreateProductVisibleCompany(currentrow);
                            continue;
                        case 71:
                            DataAccess.CreateAsset(currentrow);
                            continue;
                        case 72:
                            DataAccess.CreateClientAssetTrack(currentrow);
                            continue;
                        case 73:
                            DataAccess.CreateProductAllowedSites(currentrow);
                            continue;
                    }
                }

                reader.Close();
            }

            if (clientProductList != null)
                foreach (int catId in clientProductList.Keys)
                {
                    ClientCategoryProducts ccp = new ClientCategoryProducts(catId, clientProductList[catId]);
                    ClientCategoryProducts.AddToList(ccp);
                }

            //removed not used categories
            if (!Config.ProductInMultipleCategory)
            {
                if (usedCategories != null)
                {
                    List<Category> removables = (from category in Category.Categories
                                                 where !usedCategories.Contains(category.CategoryId)
                                                 select category).ToList();
                    foreach (Category c in removables)
                    {
                        if (Category.Categories.Any(x => x.ParentCategoryId == c.CategoryId))
                            continue;
                        Category.RemoveCategory(c);
                    }
                }
            }

            // now add back to the list of clients those clients from order 
            if (clientReaded)
            {
                if (Order.Orders.Count > 0 || InvoicePayment.List.Count > 0)
                {
                    bool clientAdded = false;
                    foreach (var order in Order.Orders)
                    {
                        var newClient = Client.Clients.FirstOrDefault(x => !string.IsNullOrEmpty(order.Client.UniqueId) && x.UniqueId == order.Client.UniqueId);
                        if (newClient == null)
                        {
                            Client.AddClient(order.Client);
                            clientAdded = true;
                        }
                        else
                        {
                            var update = order.Client.ClientId != newClient.ClientId;
                            order.Client = newClient;
                            if (update)
                                order.Save();

                            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
                            if (batch != null)
                            {
                                update = batch.Client.ClientId != newClient.ClientId;
                                batch.Client = newClient;
                                if (update)
                                    batch.Save();
                            }
                        }
                    }
                    foreach (var payment in InvoicePayment.List)
                    {
                        var newClient = Client.Clients.FirstOrDefault(x => !string.IsNullOrEmpty(payment.Client.UniqueId) && x.UniqueId == payment.Client.UniqueId);
                        if (newClient == null)
                        {
                            Client.AddClient(payment.Client);
                            clientAdded = true;
                        }
                        else
                            payment.Client = newClient;
                    }

                    if (clientAdded)
                        Client.Save();

                    Client.Clients.OrderBy(x => x.ClientName);
                }
            }

            if (productReaded)
            {
                var c = Product.Products.Count;
                if (Order.Orders.Count > 0)
                {
                    foreach (var order in Order.Orders)
                        foreach (var orderDetail in order.Details)
                        {
                            var product = Product.Products.FirstOrDefault(x => x.ProductId == orderDetail.Product.ProductId);
                            if (product == null)
                                Product.AddProduct(orderDetail.Product);
                            else
                                orderDetail.Product = product;
                        }
                    Product.Products.OrderBy(x => x.Name);
                }
                Product.LoadLots();

                DataAccess.SetRelatedProducts();
            }

            if (visibleProducts.Count > 0)
            {
                Product.RemoveNonVisible(visibleProducts);
                ClientCategoryProducts.RemoveNonVisible(visibleProducts);
            }

            if (CompanyInfo.Companies.Count(x => x.FromFile) > 0)
                CompanyInfo.Remove(false);

            var c5 = ProductOfferEx.List.Count;
            var c1 = ClientOfferEx.List.Count;
            var c2 = OfferEx.List.Count;
            var c3 = DiscountCategory.DiscountCategories.Count;
            Logger.CreateLog("df");
        }

        private static void LoadInventorySite(string[] currentrow)
        {
            int pid = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            float qty = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
            float left = Convert.ToSingle(currentrow[3], CultureInfo.InvariantCulture);
            float total = qty + left;
            string lot = "";
            DateTime exp = DateTime.MinValue;

            if (currentrow.Length > 4)
                lot = currentrow[4];

            if (currentrow.Length > 5)
                exp = new DateTime(Convert.ToInt64(currentrow[5]));

            double Weight = 0;
            if (currentrow.Length > 6)
                Weight = Convert.ToDouble(currentrow[6]);

            var prodInventory = ProductInventory.GetInventoryForProduct(pid);
            if (prodInventory != null)
            {
                prodInventory.TruckInventories.Add(new TruckInventory()
                {
                    Lot = lot,
                    Expiration = exp,
                    BeginingInventory = left,
                    RequestedLoad = qty,
                    /*Loaded = qty, */
                    CurrentQty = left,
                    Weight = Weight
                });
            }

            if (qty > 0)
                DataAccess.PendingLoadToAccept = true;
        }

        private static void CreateProductLot(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var prodId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var lot = currentrow[2];
            var qty = Convert.ToSingle(currentrow[3], CultureInfo.InvariantCulture);
            int siteid = 0;

            if (currentrow.Length > 4 && !string.IsNullOrEmpty(currentrow[4]))
            {
                siteid = Convert.ToInt32(currentrow[4], CultureInfo.InvariantCulture);

                if (Salesman.CurrentSalesman != null && siteid != Salesman.CurrentSalesman.InventorySiteId)
                    return;

                var prodInv = ProductInventory.GetInventoryForProduct(prodId);

                if (prodInv != null)
                    prodInv.TruckInventories.Add(new TruckInventory() { Lot = lot, BeginingInventory = qty, CurrentQty = qty });
            }
        }

        private static void CreateDefaultInventory(string[] currentrow)
        {
            var productId = Convert.ToInt32(currentrow[1]);
            var qty = Convert.ToSingle(currentrow[3]);

            var inv = ProductInventory.GetInventoryForProduct(productId);
            if (inv != null)
                inv.WarehouseInventory = qty;
        }

        static void CreateProduct(List<int> usedCategories, string[] currentrow)
        {
            try
            {
                Product prod = new Product();

                prod.ProductId = Convert.ToInt32(currentrow[ProductIDIndex], CultureInfo.InvariantCulture);
                prod.Name = currentrow[ProductNameIndex];
                prod.Upc = currentrow[ProductUPCIndex];
                prod.Comment = currentrow[ProductCommentIndex];
                prod.Package = currentrow[ProductPackageIndex];

                if (string.IsNullOrEmpty(prod.Package))
                    prod.Package = "1";

                prod.Description = currentrow[ProductDescriptionIndex];
                try
                {
                    prod.PriceLevel0 = Convert.ToDouble(currentrow[ProductPriceLevel0Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel1 = currentrow[ProductPriceLevel1Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel1Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel2 = currentrow[ProductPriceLevel2Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel2Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel3 = currentrow[ProductPriceLevel3Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel3Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel4 = currentrow[ProductPriceLevel4Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel4Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel5 = currentrow[ProductPriceLevel5Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel5Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel6 = currentrow[ProductPriceLevel6Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel6Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel7 = currentrow[ProductPriceLevel7Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel7Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel8 = currentrow[ProductPriceLevel8Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel8Index], CultureInfo.InvariantCulture);
                    prod.LowestAcceptablePrice = currentrow[ProductPriceLevel9Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel9Index], CultureInfo.InvariantCulture);

                    prod.CategoryId = Convert.ToInt32(currentrow[ProductCategoryIDIndex], CultureInfo.InvariantCulture);

                    if (Category.Categories.FirstOrDefault(x => x.CategoryId == prod.CategoryId) == null && prod.CategoryId > 0)
                        return;

                    if (!usedCategories.Contains(prod.CategoryId))
                        usedCategories.Add(prod.CategoryId);
                }
                catch
                {

                    prod.PriceLevel0 = 0;
                    prod.PriceLevel1 = 0;
                    prod.PriceLevel2 = 0;
                    prod.PriceLevel3 = 0;
                    prod.PriceLevel4 = 0;
                    prod.PriceLevel5 = 0;
                    prod.PriceLevel6 = 0;
                    prod.PriceLevel7 = 0;
                    prod.PriceLevel8 = 0;
                    prod.LowestAcceptablePrice = 0;
                    prod.CategoryId = 1;
                }

                if (currentrow.Length > ProductOnHandIndex)
                {
                    float onHand = 0;
                    float.TryParse(currentrow[ProductOnHandIndex], out onHand);
                    prod.OnHand = onHand;
                }

                prod.OriginalId = currentrow[ProductOriginalIDIndex];

                if (currentrow.Length > ProductExtraFieldsIndex)
                    prod.ExtraPropertiesAsString = currentrow[ProductExtraFieldsIndex];
                else
                    prod.ExtraPropertiesAsString = string.Empty;

                if (currentrow.Length > ProductTaxableIndex)
                {
                    prod.Taxable = currentrow[ProductTaxableIndex] == "1";
                }
                else
                    // by default ALL products are taxables
                    prod.Taxable = true;

                if (currentrow.Length > ProductItemTypeIndex)
                    prod.ProductType = (ProductType)Convert.ToInt32(currentrow[ProductItemTypeIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.ProductType = ProductType.Inventory;
                //if (prod.ProductType == ProductType.Discount)
                //    Logger.CreateLog("A discount " + prod.Name);
                if (currentrow.Length > ProductUoMIndex)
                    prod.UoMFamily = currentrow[ProductUoMIndex];
                else
                    prod.UoMFamily = null;

                if (currentrow.Length > ProductNonVisibleExtraFieldIndex)
                    prod.NonVisibleExtraFieldsAsString = currentrow[ProductNonVisibleExtraFieldIndex];
                else
                    prod.NonVisibleExtraFieldsAsString = string.Empty;

                //LOWELL FOOD
                if (!string.IsNullOrEmpty(prod.NonVisibleExtraFieldsAsString) && prod.NonVisibleExtraFieldsAsString.Contains("color type="))
                {
                    Config.ProductNameHasDifferentColor = true;
                    //need to save settings ?
                }

                if (currentrow.Length > ProductSoldByWeightIndex)
                    prod.SoldByWeight = Convert.ToInt32(currentrow[ProductSoldByWeightIndex], CultureInfo.InvariantCulture) > 0;
                else
                    prod.SoldByWeight = false;

                if (currentrow.Length > CodeIndex)
                    prod.Code = currentrow[CodeIndex];
                else
                    prod.Code = string.Empty;

                if (currentrow.Length > ProductCostIndex)
                    prod.Cost = Convert.ToDouble(currentrow[ProductCostIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.Cost = prod.LowestAcceptablePrice;

                if (currentrow.Length > ProductOrderInCategoryIndex)
                    prod.OrderInCategory = Convert.ToInt32(currentrow[ProductOrderInCategoryIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.OrderInCategory = 0;

                if (currentrow.Length > 31)
                    prod.RetailPrice = Convert.ToDouble(currentrow[31]);

                if (currentrow.Length > ProductWLocationIndex)
                    prod.WarehouseLocation = currentrow[ProductWLocationIndex];
                else
                    prod.WarehouseLocation = string.Empty;

                if (currentrow.Length > 28)
                    prod.Weight = Convert.ToDouble(currentrow[28]);

                if (currentrow.Length > ProductTaxRateIndex)
                    prod.TaxRate = Convert.ToDouble(currentrow[ProductTaxRateIndex], CultureInfo.InvariantCulture);
                else
                    prod.TaxRate = 0;

                if (currentrow.Length > ProductDiscountCategoryIndex)
                    prod.DiscountCategoryId = Convert.ToInt32(currentrow[ProductDiscountCategoryIndex], CultureInfo.InvariantCulture);
                else
                    prod.DiscountCategoryId = 0;

                if (currentrow.Length > ProductPriceCategoryIdIndex)
                    prod.PriceCategoryId = Convert.ToInt32(currentrow[ProductPriceCategoryIdIndex], CultureInfo.InvariantCulture);
                else
                    prod.PriceCategoryId = 0;

                if (currentrow.Length > 32)
                    prod.Sku = currentrow[32];
                else
                    prod.Sku = string.Empty;

                if (currentrow.Length > 33 && DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "30.0.0.0"))
                    prod.UseLot = Convert.ToInt32(currentrow[33]) > 0;

                if (currentrow.Length > 37)
                    prod.PalletSize = Convert.ToDouble(currentrow[37]);

                if (currentrow.Length > 38)
                {
                    if (currentrow[38].ToLowerInvariant() == "true" || currentrow[38].ToLowerInvariant() == "false")
                        prod.UseLotAsReference = Convert.ToBoolean(currentrow[38]);
                    else
                        prod.UseLotAsReference = Convert.ToInt32(currentrow[38]) > 0;
                }

                if (currentrow.Length > 39)
                {
                    prod.VendorId = Convert.ToInt32(currentrow[39]);
                }

                if (!string.IsNullOrEmpty(prod.NonVisibleExtraFieldsAsString))
                {
                    var nef = DataAccess.GetSingleUDF("inventoryByWeight", prod.NonVisibleExtraFieldsAsString);
                    prod.InventoryByWeight = !string.IsNullOrEmpty(nef) && nef == "1";
                }

                if (!string.IsNullOrEmpty(prod.ExtraPropertiesAsString))
                {
                    var caseCount = DataAccess.GetSingleUDF("CASECOUNT", prod.ExtraPropertiesAsString);
                    int cc = 1;
                    int.TryParse(caseCount, out cc);
                    prod.CaseCount = cc;
                }

                var inv = ProductInventory.GetInventoryForProduct(prod.ProductId);
                if (inv == null)
                {
                    inv = new ProductInventory() { ProductId = prod.ProductId, WarehouseInventory = prod.OnHand };
                    ProductInventory.CurrentInventories.Add(prod.ProductId, inv);
                }

                prod.ProductInv = inv;

                Product.AddProduct(prod);

            }
            catch (Exception e)
            {
                Logger.CreateLog("Error creating product " + currentrow[0]);
                
                Logger.CreateLog(e);
                Logger.CreateLog(DataAccess.Concatenate(currentrow));
            }
        }

        private static void LoadInventoryOnDemand()
        {
            foreach (var item in ProductInventory.CurrentInventories.Values)
                item.ClearProductInventory();

            if (File.Exists(Config.InventoryOnDemandStoreFile))
            {
                using (StreamReader reader = new StreamReader(Config.InventoryOnDemandStoreFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(DataAccess.DataLineSplitter);
                        LoadInventorySite(parts);
                    }
                }
            }
        }

        private static void LoadInventoryOnDemandForLot()
        {
            foreach (var item in ProductInventory.CurrentInventories.Values)
                item.ClearProductInventory();

            if (File.Exists(Config.InventoryOnDemandStoreFile))
            {
                using (StreamReader reader = new StreamReader(Config.InventoryOnDemandStoreFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(DataAccess.DataLineSplitter);
                        CreateProductLot(parts);
                    }
                }
            }
        }

        public static void GetTrucks()
        {
            //get trucks
            lock (FileOperationsLocker.lockFilesObject)
            {
                var tempFile = Config.TrucksStoreFile;

                try
                {
                    using (var access = new NetAccess())
                    {
                        access.OpenConnection();
                        access.WriteStringToNetwork("HELO");
                        access.WriteStringToNetwork(Config.GetAuthString());
                        access.WriteStringToNetwork("GetAllTrucksCommand");
                        access.ReceiveFile(tempFile);

                        Truck.DeserializeTrucks(tempFile);

                        access.WriteStringToNetwork("Goodbye");
                        access.CloseConnection();
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                }
            }
        }

        public static void GetRouteRelations()
        {
            //get trucks
            lock (FileOperationsLocker.lockFilesObject)
            {
                var tempFile = Config.RouteRelationStorePath;

                try
                {
                    using (var access = new NetAccess())
                    {
                        access.OpenConnection();
                        access.WriteStringToNetwork("HELO");
                        access.WriteStringToNetwork(Config.GetAuthString());
                        access.WriteStringToNetwork("GetBulterRoutesRelationCommand");

                        if (access.ReadStringFromNetwork() == "got routes")
                        {
                            access.ReceiveFile(tempFile);

                            RoutesWarehouseRelation.DeserializeRouteRelation(tempFile);
                        }

                        access.WriteStringToNetwork("Goodbye");
                        access.CloseConnection();

                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                }
            }
        }

        private static void ProcessDeliveryFile(string file, bool fromDownload, bool updateInventory)
        {
            try
            {
                Shipment.CurrentShipment = null;

                Dictionary<int, Batch> createdBatches = new Dictionary<int, Batch>();
                Dictionary<int, Order> createdOrders = new Dictionary<int, Order>();

                if (fromDownload && updateInventory)
                    DataAccess.PendingLoadToAccept = false;

                using (StreamReader reader = new StreamReader(file))
                {
                    string s = reader.ReadToEnd();
                }
                using (StreamReader reader = new StreamReader(file))
                {
                    string currentline;
                    int currenttable = -1;
                    while ((currentline = reader.ReadLine()) != null)
                    {
                        switch (currentline)
                        {
                            case "NEWCLIENTS":
                                currenttable = 1;
                                continue;
                            case "NEWOFFERS":
                                currenttable = 23;
                                continue;
                            case "BATCHES":
                                currenttable = 2;
                                continue;
                            case "ROUTEORDERS":
                                currenttable = 3;
                                createdOrders.Clear();
                                continue;
                            case "ROUTEEX":
                                currenttable = 4;
                                continue;
                            case "ROUTEORDERDETAILS":
                                currenttable = 5;
                                continue;
                            case "INVENTORY":
                                currenttable = 6;
                                if (updateInventory)
                                    foreach (var item in ProductInventory.CurrentInventories.Values)
                                        item.ClearProductInventory();
                                continue;
                            case "SHIPMENT":
                                currenttable = 7;
                                continue;
                            case "ENDOFTABLE":
                                currenttable = -1;
                                continue;
                            case "OpenInvoices":
                                currenttable = 21;
                                continue;
                        }

                        string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);

                        switch (currenttable)
                        {
                            case 21:
                                DataAccess.CreateInvoice(currentrow);
                                continue;
                            case 1:
                                DataAccess.CreateClient(currentrow, true);
                                continue;
                            case 2:
                                if (fromDownload)
                                    DataAccess.CreateBatch(currentrow, createdBatches);
                                continue;
                            case 3:
                                if (fromDownload)
                                    DataAccess.CreateOrder(currentrow, createdBatches, createdOrders);
                                continue;
                            case 4:
                                if (fromDownload)
                                    DataAccess.CreateRouteEX(currentrow);
                                continue;
                            case 5:
                                if (fromDownload)
                                    DataAccess.CreateOrderDetails(currentrow, createdBatches, createdOrders);
                                continue;
                            case 6:
                                if (fromDownload && updateInventory)
                                    LoadInventorySite(currentrow);
                                continue;
                            case 7:
                                DataAccess.CreateShipment(currentrow);
                                continue;
                            case 23:
                                DataAccess.CreateOffer(currentrow, true);
                                continue;
                        }
                    }
                }
                if (fromDownload)
                {
                    foreach (var order in createdOrders.Values)
                    {
                        if (order.IsDelivery && order.OrderType == OrderType.Consignment && !Config.ParInConsignment)
                            order.ConvertConsignmentPar();

                        order.Save();
                    }
                    // delete any bathc that contains no order
                    List<Batch> deleted = new List<Batch>();
                    foreach (var batch in createdBatches.Values)
                        if (createdOrders.Values.FirstOrDefault(x => x.BatchId == batch.Id) != null)
                            batch.Save();
                        else
                            deleted.Add(batch);
                    foreach (var batch in deleted)
                        batch.Delete();
                }

                // The related Items lines came in a different way, readjust it to the expected way
                foreach (var order in Order.Orders)
                {
                    bool needSave = false;
                    foreach (var detail in order.Details)
                        if (detail.ExtraFields.IndexOf("RelatedDetail") != -1)
                        {
                            var pair = DataAccess.ExplodeExtraProperties(detail.ExtraFields).FirstOrDefault(x => x.Key == "RelatedDetail");
                            if (pair != null)
                            {
                                var relatedDetail = order.Details.FirstOrDefault(x => x.OriginalId == pair.Value);
                                if (relatedDetail != null)
                                {
                                    detail.RelatedOrderDetail = relatedDetail.OrderDetailId;
                                    needSave = true;
                                }
                            }
                        }
                    if (needSave)
                        order.Save();
                }
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                Logger.CreateLog("LaceUPMobileClassesIOS.DataAccess.ProcessDeliveryFile LINE=" + line + "\n" + ex.Message);
            }
        }

        public static void UpdateClientPrices(int clientId)
        {
            var CheckInternetThread = new Thread(delegate () { GetClientPricesInBackground(clientId); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }

        public static void UpdateOffersForClient(int clientId)
        {
            var CheckInternetThread = new Thread(delegate () { GetOffersInBackground(clientId); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }

        public static void UpdateInventoryBySite()
        {
            var CheckInternetThread = new Thread(delegate () { GetInventoryInBackgroundForSite(); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }
        public static void UpdateInventoryBySite(int SiteId)
        {
            var CheckInternetThread = new Thread(delegate () { GetInventoryInBackgroundForSite(SiteId); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }

        public static void UpdateInventory()
        {
            var CheckInternetThread = new Thread(delegate () { GetInventoryInBackground(); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }

        public static void UpdateInventory(bool isPresale = false)
        {
            var CheckInternetThread = new Thread(delegate () { GetInventoryInBackground(isPresale); });
            CheckInternetThread.IsBackground = true;
            CheckInternetThread.Start();
        }

        public static async void GetInventoryInBackgroundAsync(bool isPresale = false)
        {
            Task.Run(() => GetInventoryInBackground(isPresale)).Wait();
        }

        public static async void GetInventoryForSiteInBackgroundAsync()
        {
            Task.Run(() => GetInventoryInBackgroundForSite()).Wait();
        }

        public static void GetInventoryInBackground(bool isPresale = false)
        {
            Logger.CreateLog("getting the inventory");
            try
            {
                NetAccess.GetCommunicatorVersion();

                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    if (!Config.TrackInventory || isPresale)
                    {
                        string tempFile = Path.GetTempFileName();

                        if (Config.WarehouseInventoryOnDemand || DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.1.0.0"))
                        {
                            // bring the default inventory on demand
                            if (!isPresale)
                                netaccess.WriteStringToNetwork("DefaultSiteInventoryCommand");
                            else
                                netaccess.WriteStringToNetwork("DefaultSiteInventoryAsPresale");

                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                InventorySiteInventory.Map.Clear();
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    var item = new InventorySiteInventory();
                                    item.ProductId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                                    item.LeftOver = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                    var product = Product.Find(item.ProductId);
                                    if (product != null)
                                    {
                                        product.ProductInv.WarehouseInventory = (float)item.LeftOver;

                                        if (currentrow.Length > 2)
                                            product.OnPO = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                        }
                        else
                        {
                            netaccess.WriteStringToNetwork("ProductsInventoryCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    int pId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                                    var inv = ProductInventory.GetInventoryForProduct(pId);
                                    if (inv != null)
                                        inv.WarehouseInventory = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                }
                            }
                        }

                        File.Delete(tempFile);
                    }

                    // the default inventory 

                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            Logger.CreateLog("got the inventory");
        }

        static void GetInventoryInBackgroundForSite(int siteId)
        {
            Logger.CreateLog("getting the inventory");
            try
            {
                NetAccess.GetCommunicatorVersion();

                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    string tempFile = Path.GetTempFileName();

                    if (Config.WarehouseInventoryOnDemand || DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.1.0.0"))
                    {
                        // bring the default inventory on demand
                        netaccess.WriteStringToNetwork("GetPresaleInventoryForSiteCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                        netaccess.WriteStringToNetwork(siteId.ToString(CultureInfo.InvariantCulture));

                        netaccess.ReceiveFile(tempFile);

                        using (StreamReader reader = new StreamReader(tempFile))
                        {
                            InventorySiteInventory.Map.Clear();
                            string currentline;

                            foreach (var item in Product.Products)
                                item.ProductInv.WarehouseInventory = 0;

                            while ((currentline = reader.ReadLine()) != null)
                            {
                                string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                var item = new InventorySiteInventory();
                                item.ProductId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                                item.LeftOver = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                var product = Product.Find(item.ProductId);
                                if (product != null)
                                {
                                    product.ProductInv.WarehouseInventory = (float)item.LeftOver;

                                    if (currentrow.Length > 2)
                                        product.OnPO = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                    else
                    {
                        netaccess.WriteStringToNetwork("ProductsInventoryCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                        netaccess.ReceiveFile(tempFile);

                        using (StreamReader reader = new StreamReader(tempFile))
                        {
                            string currentline;

                            foreach (var item in Product.Products)
                                item.ProductInv.WarehouseInventory = 0;

                            while ((currentline = reader.ReadLine()) != null)
                            {
                                string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                int pId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                                var inv = ProductInventory.GetInventoryForProduct(pId);
                                if (inv != null)
                                    inv.WarehouseInventory = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    File.Delete(tempFile);
                }


            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            Logger.CreateLog("got the inventory");
        }

        static void GetClientPricesInBackground(int clientId)
        {
            string tempFile = Path.GetTempFileName();

            Logger.CreateLog("getting new prices");
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("PricingInfoCommand");
                    netaccess.WriteStringToNetwork(clientId.ToString());

                    var confirmation = netaccess.ReadStringFromNetwork();
                    if (confirmation == "nothing")
                        return;

                    netaccess.ReceiveFile(tempFile);

                    using (StreamReader reader = new StreamReader(tempFile))
                    {
                        int priceLevelId = -1;
                        bool isFirstLine = true;
                        string currentline;

                        while ((currentline = reader.ReadLine()) != null)
                        {
                            if (isFirstLine)
                            {
                                //add 10 to pricelevel
                                priceLevelId = (Convert.ToInt32(currentline) + 10);
                                isFirstLine = false;
                                continue;
                            }

                            if (currentline == "PriceLevel")
                                continue;

                            if (currentline == "EndOfTable")
                                break;

                            string[] currentrow = currentline.Split(',');

                            int productId = Convert.ToInt32(currentrow[0]);
                            double price = Convert.ToDouble(currentrow[1]);

                            double allowance = 0;
                            if (currentrow.Length > 2)
                                allowance = Convert.ToDouble(currentrow[2]);

                            var pp = ProductPrice.Pricelist.FirstOrDefault(x => x.PriceLevelId == priceLevelId && x.ProductId == productId);
                            if (pp != null)
                            {
                                pp.Price = price;
                                pp.Allowance = allowance;
                            }
                        }
                    }
                }

            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            Logger.CreateLog("got updated prices");
        }

        static void GetOffersInBackground(int clientId)
        {
            DateTime startTime = DateTime.Now;

            string tempFile = Path.GetTempFileName();

            Logger.CreateLog("getting offers");
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("CustomerOffersCommand");
                    netaccess.WriteStringToNetwork(clientId.ToString());

                    var confirmation = netaccess.ReadStringFromNetwork();
                    if (confirmation == "nothing")
                        return;

                    netaccess.ReceiveFile(tempFile);

                    using (StreamReader reader = new StreamReader(tempFile))
                    {
                        string currentline;

                        while ((currentline = reader.ReadLine()) != null)
                        {
                            if (currentline == "Offers")
                                continue;

                            if (currentline == "EndOfTable")
                                break;

                            string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);

                            var offer = new Offer();
                            offer.OfferId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);


                            if (currentrow.Length > 12)
                                offer.OriginalId = currentrow[12];

                            if (!string.IsNullOrEmpty(offer.OriginalId))
                            {
                                var alreadyAdded1 = Offer.OfferList.FirstOrDefault(x => x.OriginalId == offer.OriginalId);
                                if (alreadyAdded1 != null)
                                {
                                    var price = Convert.ToDouble(currentrow[6], CultureInfo.InvariantCulture);
                                    alreadyAdded1.Price = price;
                                    continue;
                                }
                            }
                            else
                            {
                                var alreadyAdded = Offer.OfferList.FirstOrDefault(x => x.OfferId == offer.OfferId);
                                if (alreadyAdded != null)
                                {
                                    var price = Convert.ToDouble(currentrow[6], CultureInfo.InvariantCulture);
                                    alreadyAdded.Price = price;
                                    continue;
                                }
                            }

                            offer.FromDate = Convert.ToDateTime(currentrow[1], CultureInfo.InvariantCulture);
                            offer.ToDate = Convert.ToDateTime(currentrow[2], CultureInfo.InvariantCulture);
                            offer.ToDate = offer.ToDate.Date.AddMinutes(1399);

                            var offerType = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                            if (offerType == 11)
                                offerType = 0;
                            offer.Type = (OfferType)offerType;
                            offer.ProductId = Convert.ToInt32(currentrow[4], CultureInfo.InvariantCulture);
                            if (offer.Type == OfferType.Discount || offer.Type == OfferType.DiscountQty || offer.Type == OfferType.DiscountAmount)
                                offer.Product = Product.Find(offer.ProductId, true);
                            else
                                offer.Product = Product.Find(offer.ProductId);
                            if (offer.Product == null)
                            {
                                Logger.CreateLog("Offer has a null product: " + offer.ProductId.ToString(CultureInfo.InvariantCulture));
                                return;
                            }
                            offer.MinimunQty = Convert.ToSingle(currentrow[5], CultureInfo.InvariantCulture);
                            offer.Price = Convert.ToDouble(currentrow[6], CultureInfo.InvariantCulture);
                            offer.FreeQty = Convert.ToSingle(currentrow[7], CultureInfo.InvariantCulture);
                            if (currentrow.Length > 8)
                                offer.ClienBased = Convert.ToInt32(currentrow[8], CultureInfo.InvariantCulture) > 0;
                            if (currentrow.Length > 9)
                                offer.UnitOfMeasureId = Convert.ToInt32(currentrow[9], CultureInfo.InvariantCulture);

                            if (currentrow.Length > 10)
                                offer.ExtraFields = currentrow[10];

                            Offer.AddOffer(offer);

                        }
                    }
                }

            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

            }
            Logger.CreateLog("got updated offers");
        }

        static void GetInventoryInBackgroundForSite()
        {
            Logger.CreateLog("getting the inventory");
            try
            {
                NetAccess.GetCommunicatorVersion();

                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    if (!Config.TrackInventory)
                    {
                        string tempFile = Path.GetTempFileName();

                        if (Config.WarehouseInventoryOnDemand || DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.1.0.0"))
                        {
                            // bring the default inventory on demand
                            netaccess.WriteStringToNetwork("GetPresaleInventoryForSiteCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                            var siteId = Config.SalesmanSelectedSite;
                            if (Config.PresaleUseInventorySite)
                            {
                                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                                if (salesman != null)
                                    siteId = salesman.InventorySiteId;
                            }

                            netaccess.WriteStringToNetwork(siteId.ToString(CultureInfo.InvariantCulture));

                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                InventorySiteInventory.Map.Clear();
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    var item = new InventorySiteInventory();
                                    item.ProductId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                                    item.LeftOver = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                    var product = Product.Find(item.ProductId);
                                    if (product != null)
                                    {
                                        product.ProductInv.WarehouseInventory = (float)item.LeftOver;

                                        if (currentrow.Length > 2)
                                            product.OnPO = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                        }
                        else
                        {
                            netaccess.WriteStringToNetwork("ProductsInventoryCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    int pId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                                    var inv = ProductInventory.GetInventoryForProduct(pId);
                                    if (inv != null)
                                        inv.WarehouseInventory = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                }
                            }
                        }

                        File.Delete(tempFile);
                    }
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            Logger.CreateLog("got the inventory");
        }
        static void GetInventoryInBackground()
        {
            Logger.CreateLog("getting the inventory");
            try
            {
                NetAccess.GetCommunicatorVersion();

                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    if (!Config.TrackInventory)
                    {
                        string tempFile = Path.GetTempFileName();

                        if (Config.WarehouseInventoryOnDemand || DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.1.0.0"))
                        {
                            // bring the default inventory on demand
                            netaccess.WriteStringToNetwork("DefaultSiteInventoryCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                InventorySiteInventory.Map.Clear();
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    var item = new InventorySiteInventory();
                                    item.ProductId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                                    item.LeftOver = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                    var product = Product.Find(item.ProductId);
                                    if (product != null)
                                    {
                                        product.ProductInv.WarehouseInventory = (float)item.LeftOver;

                                        if (currentrow.Length > 2)
                                            product.OnPO = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                        }
                        else
                        {
                            netaccess.WriteStringToNetwork("ProductsInventoryCommand");
                            netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                            netaccess.ReceiveFile(tempFile);

                            using (StreamReader reader = new StreamReader(tempFile))
                            {
                                string currentline;

                                foreach (var item in Product.Products)
                                    item.ProductInv.WarehouseInventory = 0;

                                while ((currentline = reader.ReadLine()) != null)
                                {
                                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                    int pId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                                    var inv = ProductInventory.GetInventoryForProduct(pId);
                                    if (inv != null)
                                        inv.WarehouseInventory = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);
                                }
                            }
                        }

                        File.Delete(tempFile);
                    }

                    // the default inventory 

                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            Logger.CreateLog("got the inventory");
        }

        public static void SaveInventory()
        {
            ProductInventory.Save();
        }

        public static string GetAllActiveClients()
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetAllActiveClientsCommand");

                    string basefileC = Path.Combine(Config.DataPath, "reading_fileAllActiveClients.cvs");

                    if (netaccess.ReceiveFile(basefileC) == 0)
                        return "Error Downloading Customers";

                    using (StreamReader reader = new StreamReader(basefileC))
                    {
                        string currentline;
                        while ((currentline = reader.ReadLine()) != null)
                        {
                            string[] currentrow = null;// currentline.Split(DataLineSplitter);
                            currentrow = currentline.Split(DataAccess.DataLineSplitter);
                            DataAccess.CreateClientEX(currentrow, false, false);
                        }
                    }
                    return "Success";

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region EnableLiveDataKey = 1
        public static string DownloadProducts()
        {
            bool gotUnitOfMeasure = GetUnitOfMeasures();

            DateTime start = DateTime.Now;

            DateTime now = DateTime.Now;

            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    bool gotUnitOfMeasures = GetUnitOfMeasures();

                    //Set the target files
                    string basefileP = Path.Combine(Config.DataPath, "reading_fileP.zip");
                    string targetfileP = Config.ProductStoreFile;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        return ee.Message;
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("Products");

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileP) == 0)
                        return "Error Downloading Products";
                    Logger.CreateLog("Products downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    bool inventoryOnDemand = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "13.0.0.0");

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileP, targetfileP);
                    LoadData__(Config.ProductStoreFile, true && !inventoryOnDemand, !gotUnitOfMeasures);
                    Logger.CreateLog("Products processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileP);

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string DownloadClients()
        {
            bool gotUnitOfMeasure = GetUnitOfMeasures();

            DateTime start = DateTime.Now;

            DateTime now = DateTime.Now;

            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    bool gotUnitOfMeasures = GetUnitOfMeasures();

                    string basefileC = Path.Combine(Config.DataPath, "reading_fileC.zip");
                    string targetfileC = Config.ClientStoreFile;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        return ee.Message;
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("Clients");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));

                    now = DateTime.Now;
                    if (netaccess.ReceiveFile(basefileC) == 0)
                        return "Error Downloading Customers";
                    Logger.CreateLog("Clients downloaded in " + DateTime.Now.Subtract(now).TotalSeconds);

                    bool inventoryOnDemand = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "13.0.0.0");

                    now = DateTime.Now;
                    DataAccess.UnzipFile(basefileC, targetfileC);
                    Client.DeleteClients();
                    LoadData__(Config.ClientStoreFile, true && !inventoryOnDemand, !gotUnitOfMeasures);
                    Logger.CreateLog("Clients processed in " + DateTime.Now.Subtract(now).TotalSeconds);
                    File.Delete(basefileC);

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetTopic()
        {
            // try
            // {
            //     using (var netaccess = new NetAccess())
            //     {
            //         netaccess.OpenConnection();
            //         netaccess.WriteStringToNetwork("HELO");
            //         netaccess.WriteStringToNetwork(Config.GetAuthString());
            //         netaccess.WriteStringToNetwork("GetServiceNameCommand");
            //
            //         var topic = netaccess.ReadStringFromNetwork();
            //
            //         topic = topic.ToLowerInvariant();
            //
            //         topic = topic.Trim();
            //
            //         if (!string.IsNullOrEmpty(topic))
            //         {
            //             FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
            //             FirebaseMessaging.Instance.SubscribeToTopic(topic);
            //
            //             Logger.CreateLog("Subscribing to notifications on topic => " + topic);
            //
            //             if (Config.AllowWorkOrder || Config.AllowNotifications || Config.NotificationsInSelfService)
            //             {
            //                 FirebaseMessaging.Instance.UnsubscribeFromTopic(topic + Config.SalesmanId);
            //                 FirebaseMessaging.Instance.SubscribeToTopic(topic + Config.SalesmanId);
            //
            //                 Logger.CreateLog("Subscribing to notifications from work order on topic => " + topic + Config.SalesmanId);
            //             }
            //         }
            //
            //         return topic;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Logger.CreateLog("Trying to subscribe to notifications failed => " + ex.ToString());
            //     return string.Empty;
            // }
            return string.Empty;
        }

        public static void Unsubscribe()
        {
            // try
            // {
            //     using (var netaccess = new NetAccess())
            //     {
            //         netaccess.OpenConnection();
            //         netaccess.WriteStringToNetwork("HELO");
            //         netaccess.WriteStringToNetwork(Config.GetAuthString());
            //         netaccess.WriteStringToNetwork("GetServiceNameCommand");
            //
            //         var topic = netaccess.ReadStringFromNetwork();
            //
            //         topic = topic.ToLowerInvariant();
            //
            //         topic = topic.Trim();
            //
            //         if (!string.IsNullOrEmpty(topic))
            //         {
            //             FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
            //
            //             if (Config.AllowWorkOrder || Config.AllowNotifications || Config.NotificationsInSelfService)
            //                 FirebaseMessaging.Instance.UnsubscribeFromTopic(topic + "/" + Config.SalesmanId);
            //         }
            //
            //     }
            // }
            // catch (Exception ex)
            // {
            // }
        }
        #endregion


        private static void GetSites()
        {
            try
            {
                Logger.CreateLog("Downloading data in DataAccessEx");

                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    string sitesFile = Config.SiteExPath;

                    if (File.Exists(sitesFile))
                        File.Delete(sitesFile);

                    netaccess.WriteStringToNetwork("WarehouseSitesForBranchCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                    netaccess.ReceiveFile(sitesFile);

                    LoadSites();

                    netaccess.CloseConnection();
                }
            }
            catch
            {
                throw;
            }
        }

        public static char[] DataLineSplitter1 = new char[] { (char)20 };

        private static void LoadSites()
        {
            SiteEx.Clear();

            try
            {
                using (StreamReader reader = new StreamReader(Config.SiteExPath))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter1);

                        bool createdLocally = Convert.ToBoolean(currentrow[3], CultureInfo.InvariantCulture);

                        SiteEx site = new SiteEx();
                        site.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                        site.Name = currentrow[1];
                        site.ExtraFields = currentrow[6];
                        site.ParentSiteId = Convert.ToInt32(currentrow[7], CultureInfo.InvariantCulture);

                        if (currentrow.Length > 9)
                            site.SiteType = (SiteType)Convert.ToInt32(currentrow[9]);

                        if (currentrow.Length > 10)
                            site.Code = string.IsNullOrEmpty(currentrow[10]) ? site.Name : currentrow[10];

                        if (currentrow.Length > 11)
                            site.Color = currentrow[11];

                        if (currentrow.Length > 12)
                            site.UsedForPicking = Convert.ToInt32(currentrow[12]) > 0;

                        if (currentrow.Length > 13)
                            site.MaxQty = Convert.ToDouble(currentrow[13]);

                        if (currentrow.Length > 14)
                            site.PickingOrder = Convert.ToInt32(currentrow[14]);

                        if (currentrow.Length > 16)
                            site.RestrictionFree = Convert.ToInt32(currentrow[16]) > 0;

                        if (currentrow.Length > 18 && Convert.ToInt32(currentrow[18]) > 0 && site.SiteType != SiteType.ReturnArea)
                            site.SiteType = SiteType.ReturnArea;

                        if (currentrow.Length > 19)
                            site.OnlyAllowedProducts = Convert.ToInt32(currentrow[19]) > 0;

                        if (currentrow.Length > 20)
                            site.ProductLotRestriction = (ProductLotRestrictionsEnum)(Convert.ToInt32(currentrow[20]));

                        if (site.SiteType == SiteType.NotDefined && !Config.DicosaCustomization)
                            continue;

                        SiteEx.AddSite(site);
                    }
                }

                if(!Config.DicosaCustomization) 
                    SiteEx.AdjustParents();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        static void GetInventoryForSite()
        {
            try
            {
                var branchId = Config.BranchSiteId;

                var site = SiteEx.Find(branchId);
                string sites = "";

                if(Config.DicosaCustomization)
                    sites = string.Join(',', SiteEx.Sites.Select(x => x.Id).ToList());
                else
                {
                    foreach (var item in site.GetLeafSites())
                    {
                        if (!string.IsNullOrEmpty(sites))
                            sites += ',';
                        sites += item.Id;
                    }
                }
                
                //add the branch sites anyways for when is not advanced wh / maybe should bring the setitng and add here if (!Config.AdvancedWarehouse)
                if (!string.IsNullOrEmpty(sites))
                    sites += ',' + branchId.ToString();
                else
                    sites = branchId.ToString();

                if (File.Exists(Config.InventorySiteExPath))
                    File.Delete(Config.InventorySiteExPath);

                using (NetAccess netaccess = new NetAccess())
                {
                    //open the connection
                    netaccess.OpenConnection();

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("WarehouseGetInventoryForSiteCommand");
                    netaccess.WriteStringToNetwork(sites);

                    string invFile = Config.InventorySiteExPath;

                    netaccess.ReceiveFile(invFile);

                    netaccess.CloseConnection();

                    LoadInventories();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                throw;
            }
        }

        static void LoadInventories()
        {
            try
            {
                var branchId = Config.BranchSiteId;

                var site = SiteEx.Find(branchId);

                var leafs = site.GetLeafSites();

                if (leafs.Count == 0)
                    site.ClearInventories();
                else
                    foreach (var item in site.GetLeafSites())
                    {
                        item.ClearInventories();

                        Logger.CreateLog(string.Format("Clear inventories for site ", item.Name));
                    }

                using (StreamReader reader = new StreamReader(Config.InventorySiteExPath))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        var currentrow = currentline.Split('|');

                        var siteId = Convert.ToInt32(currentrow[0]);
                        var prodId = Convert.ToInt32(currentrow[1]);
                        var qty = Convert.ToDouble(currentrow[2]);
                        string lot = currentrow[3];
                        DateTime expiration = new DateTime(Convert.ToInt64(currentrow[4]));

                        string extrafields = "";
                        for (int i = 5; i < currentrow.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(extrafields))
                                extrafields += "|";
                            extrafields += currentrow[i];
                        }

                        var ss = SiteEx.Find(siteId);
                        ss.AddInventory(prodId, qty, lot, expiration, extrafields);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }


        #region get quotes

        private static void GetQuotesCreated()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    if (File.Exists(Config.QuotesPath))
                        File.Delete(Config.QuotesPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetQuotesCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

                    access.ReceiveFile(Config.QuotesPath);

                    access.CloseConnection();

                    LoadQuotes();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static readonly char[] _delim = { (char)20 };
        private static readonly NumberFormatInfo _nfi = CultureInfo.InvariantCulture.NumberFormat;

        public static void LoadQuotes()
        {
            List<Order> toDelete = new List<Order>();
            toDelete = Order.Orders.Where(x => x.QuoteModified).ToList();

            foreach (var o in toDelete)
                o.Delete();


            // 1-b) Build in-memory caches used by the inner loops
            var existingIds = new HashSet<string>(Order.Orders.Select(o => o.UniqueId));
            var productsById = Product.Products.ToDictionary(p => p.ProductId);
            var uomById = UnitOfMeasure.List.ToDictionary(u => u.Id);

            Order? order = null;
            bool nextIsOrder = false;
            bool nextIsDetail = false;

            foreach (string line in File.ReadLines(Config.QuotesPath))
            {
                switch (line)
                {
                    case "NewOrder":
                        FlushOrder(order);            // save the *previous* order once
                        nextIsOrder = true;
                        nextIsDetail = false;
                        continue;

                    case "OrderDetails":
                        nextIsDetail = true;
                        continue;
                }

                if (nextIsOrder)
                {
                    order = CreateQuote(line.Split(_delim), existingIds);
                    nextIsOrder = false;
                    continue;
                }

                if (nextIsDetail && order != null)
                {
                    CreateQuoteDetails(
                        line.Split(_delim),
                        order,
                        productsById,
                        uomById);
                }
            }

            FlushOrder(order);                        // save the final order
        }

        // ──────────────────────────────────────────────────────────────────────────────
        // 2. CreateQuote  ─ creates an Order object from the “header” row
        // ──────────────────────────────────────────────────────────────────────────────
        private static Order? CreateQuote(string[] row, HashSet<string> existingIds)
        {
            try
            {
                // prevent duplicates
                if (existingIds.Contains(row[21]))
                {
                    Logger.CreateLog($"CreateOrder referenced existing order. UniqueId: {row[21]}  ID: {row[0]}");
                    return null;
                }

                int orderId = int.Parse(row[0], _nfi);
                int clientId = int.Parse(row[1], _nfi);
                Client client = Client.Find(clientId) ?? Client.CreateTemporalClient(clientId);

                var order = new Order(client, false)
                {
                    OriginalOrderId = orderId,
                    OriginalSalesmanId = int.Parse(row[3], _nfi),
                    Date = DateTime.Parse(row[5], _nfi),
                    PrintedOrderId = row[9],
                    OrderType = OrderType.Order,
                    IsQuote = true,
                    AsPresale = true,
                    Comments = row[12],
                    TaxRate = double.Parse(row[14], _nfi),
                    PONumber = row[18],
                    SalesmanId = Config.SalesmanId,
                    ShipDate = DateTime.Parse(row[19], _nfi),
                    UniqueId = row[21],
                    ExtraFields = row[15],
                    QuoteModified = true
                };

                if (row.Length > 22) order.DiscountAmount = float.Parse(row[22], _nfi);
                if (row.Length > 23) order.DiscountType = (DiscountType)int.Parse(row[23], _nfi);
                if (row.Length > 26) order.DiscountComment = row[26];

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                    order.PrintedOrderId = orderId.ToString();

                return order;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return null;
            }
        }

        // ──────────────────────────────────────────────────────────────────────────────
        // 3. CreateQuoteDetails  ─ adds a single OrderDetail to the current Order
        // ──────────────────────────────────────────────────────────────────────────────
        private static void CreateQuoteDetails(
            string[] row,
            Order order,
            IDictionary<int, Product> productsById,
            IDictionary<int, UnitOfMeasure> uomById)
        {
            if (order == null) return;

            try
            {
                int prodId = int.Parse(row[2], _nfi);
                if (!productsById.TryGetValue(prodId, out Product product))
                {
                    Logger.CreateLog($"Product with ID {row[2]} not found");
                    return;
                }

                float qty = float.Parse(row[3], _nfi);
                double price = double.Parse(row[4], _nfi);

                var detail = new OrderDetail(product, 0, order)
                {
                    Price = price,
                    ExpectedPrice = price,
                    Comments = row[5]
                };

                // weight
                detail.Weight = (row.Length > 17)
                    ? (float)double.Parse(row[17], _nfi)
                    : qty;

                if (product.SoldByWeight)
                {
                    detail.Qty = 1;
                    detail.Ordered = 1;
                    if (product.FixedWeight && Config.NewAddItemRandomWeight)
                        detail.Weight = (float)product.Weight;
                }
                else
                {
                    detail.Qty = qty;
                    detail.Ordered = qty;
                    detail.Weight = 0;
                }

                detail.FromOffer = !string.IsNullOrEmpty(row[6]) && bool.Parse(row[6]);

                detail.OriginalId = row.Length > 7 ? row[7] : string.Empty;
                detail.IsCredit = row.Length > 8 && int.Parse(row[8], _nfi) > 0;
                detail.UnitOfMeasure = (row.Length > 9 && int.TryParse(row[9], out int uomId) && uomById.TryGetValue(uomId, out var uom)) ? uom : null;
                detail.Damaged = row.Length > 10 && int.Parse(row[10], _nfi) > 0;
                detail.Lot = row.Length > 11 ? row[11] : string.Empty;
                detail.ExtraFields = row.Length > 12 ? row[12] : string.Empty;

                order.AddDetail(detail);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private static void FlushOrder(Order? order)
        {
            if (order == null) return;

            if (order.BatchId == 0)
            {
                var batch = new Batch(order.Client)
                {
                    ClockedIn = order.Date
                };
                batch.Save();
                order.BatchId = batch.Id;
            }
            order.Save();
        }

        #endregion

        #region get Goals

        private static void GetGoals()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    if (File.Exists(Config.GoalsPath))
                        File.Delete(Config.GoalsPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetGoalsCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

                    access.ReceiveFile(Config.GoalsPath);

                    access.CloseConnection();

                    LoadGoals();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void LoadGoals()
        {
            Goal.List.Clear();

            using (StreamReader reader = new StreamReader(Config.GoalsPath))
            {
                string line;
                bool nextIsGoal = false;
                bool nextIsDetail = false;
                Goal goal = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "EndOfGoal")
                    {
                        nextIsGoal = false;
                        nextIsDetail = false;
                        goal = null;
                        continue;
                    }

                    if (line == "Goal")
                    {
                        nextIsDetail = false;
                        nextIsGoal = true;
                        continue;

                    }

                    if (line == "GoalDetail")
                    {
                        nextIsDetail = true;
                        continue;
                    }

                    if (nextIsGoal)
                    {
                        string[] parts = line.Split(new char[] { (char)20 });

                        goal = CreateGoal(parts);
                        nextIsGoal = false;

                        if (!Goal.List.Contains(goal))
                            Goal.List.Add(goal);

                        continue;
                    }

                    if (nextIsDetail)
                    {
                        string[] parts = line.Split(new char[] { (char)20 });

                        CreateGoalDetails(parts, goal);
                    }

                }
            }
        }

        private static void CreateGoalDetails(string[] parts, Goal goal)
        {

            var id = Convert.ToInt32(parts[0]);
            var productId = Convert.ToInt32(parts[1]);
            var salesmanId = Convert.ToInt32(parts[2]);
            var Qty = Convert.ToInt32(parts[3]);
            var Amount = Convert.ToInt32(parts[4]);
            var unitOfMeasureId = Convert.ToInt32(parts[5]);

            GoalDetail detail = new GoalDetail()
            {
                Id = id,
                ProductId = productId,
                SalesmanId = salesmanId,
                Qty = Qty,
                Amount = Amount,
                UnitOfMeasureId = unitOfMeasureId,
                Goal = goal,
                GoalId = goal.Id
            };

            goal.GoalDetails.Add(detail);
        }

        private static Goal CreateGoal(string[] parts)
        {
            var id = Convert.ToInt32(parts[0]);
            var name = parts[1];
            var type = Convert.ToInt32(parts[2]);
            var startdate = new DateTime(Convert.ToInt64(parts[3]));
            var endDate = new DateTime(Convert.ToInt64(parts[4]));
            var workingDays = Convert.ToInt32(parts[5]);
            var criteria = Convert.ToInt32(parts[7]);

            Goal goal = new Goal()
            {
                Id = id,
                Name = name,
                Type = type,
                StartDate = startdate,
                EndDate = endDate,
                WorkingDays = workingDays,
                Criteria = criteria
            };

            return goal;
        }

        public static void LoadGoalProgress()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    if (File.Exists(Config.GoalProgressPath))
                        File.Delete(Config.GoalProgressPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetGoalsProgressCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

                    access.ReceiveFile(Config.GoalProgressPath);

                    access.CloseConnection();

                    LoadProgress(Config.GoalProgressPath);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static char[] DataLineSplitter = new char[] { (char)20 };
        private static void LoadProgress(string tempPath)
        {
            GoalProgressDTO.List.Clear();

            try
            {
                using (StreamReader reader = new StreamReader(tempPath))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);

                        GoalProgressDTO header = new GoalProgressDTO();
                        header.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                        header.Name = currentrow[1];
                        header.Type = (GoalType)Convert.ToInt32(currentrow[2]);
                        header.StartDate = new DateTime(Convert.ToInt64(currentrow[3]));
                        header.EndDate = new DateTime(Convert.ToInt64(currentrow[4]));
                        header.Criteria = (GoalCriteria)Convert.ToInt32(currentrow[5]);
                        header.WorkingDays = Convert.ToInt32(currentrow[6]);
                        header.WorkedDays = Convert.ToInt32(currentrow[7]);
                        header.QuantityOrAmount = Convert.ToDouble(currentrow[8]);
                        header.Sold = Convert.ToDouble(currentrow[9]);
                        header.SalesOrder = Convert.ToDouble(currentrow[10]);

                        if (currentrow.Length > 11)
                        {
                            header.IsActive = Convert.ToInt32(currentrow[11]) > 0;
                        }

                        if (currentrow.Length > 12)
                        {
                            header.CreditInvoice = Convert.ToDouble(currentrow[12]);
                        }

                        GoalProgressDTO.List.Add(header);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

            File.Delete(tempPath);
        }


        public static void LoadGoalProgressDetail()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    if (File.Exists(Config.GoalProgressPath))
                        File.Delete(Config.GoalProgressPath);

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetGoalsProgressDetailCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

                    access.ReceiveFile(Config.GoalProgressPath);

                    access.CloseConnection();

                    LoadProgressDetail(Config.GoalProgressPath);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void LoadProgressDetail(string goalProgressPath)
        {
            GoalDetailDTO.List.Clear();

            try
            {
                using (StreamReader reader = new StreamReader(goalProgressPath))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);

                        GoalDetailDTO header = new GoalDetailDTO();
                        header.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                        header.Name = currentrow[1];
                        header.Type = (GoalType)Convert.ToInt32(currentrow[2]);
                        header.Criteria = (GoalCriteria)Convert.ToInt32(currentrow[3]);
                        header.ProductId = Convert.ToInt32(currentrow[4]);
                        header.QuantityOrAmount = Convert.ToDouble(currentrow[5]);
                        header.Sold = Convert.ToDouble(currentrow[6]);
                        header.SalesOrder = Convert.ToDouble(currentrow[7]);
                        header.WorkedDays = Convert.ToInt32(currentrow[8]);
                        header.WorkingDays = Convert.ToInt32(currentrow[9]);

                        if (currentrow.Length > 10)
                        {
                            var id = Convert.ToInt32(currentrow[10]);
                            header.ExternalInvoice = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceId == id);
                        }

                        if (currentrow.Length > 11)
                        {
                            var cId = Convert.ToInt32(currentrow[11]);
                            header.ClientId = cId;
                        }

                        if (currentrow.Length > 12)
                        {
                            var goalid = Convert.ToInt32(currentrow[12]);
                            header.GoalId = goalid;
                        }

                        if (currentrow.Length > 13)
                        {
                            var uomId = Convert.ToInt32(currentrow[13]);

                            if (uomId > 0)
                            {
                                var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                                if (uom != null)
                                    header.UoM = uom;
                            }
                        }

                        if (currentrow.Length > 14)
                        {
                            var credit = Convert.ToDouble(currentrow[14]);
                            header.CreditInvoice = credit;
                        }

                        GoalDetailDTO.List.Add(header);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

            File.Delete(goalProgressPath);
        }

        #endregion

        public static bool CheckIfShipdateIsValid(List<DateTime> shipDates, ref List<DateTime> lockedDates)
        {
            try
            {
                using (var access = new NetAccess())
                {
                    lockedDates = new List<DateTime>();

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());


                    access.WriteStringToNetwork("CheckIfShipdateLockedCommand");

                    string toSend = string.Empty;

                    var ticks = shipDates.Select(x => x.Ticks).Distinct().ToList();

                    toSend = string.Join(',', ticks);

                    access.WriteStringToNetwork(toSend.ToString(CultureInfo.InvariantCulture));

                    string response = access.ReadStringFromNetwork()?.Trim(); // response a list of locked dates 
                    access.CloseConnection();

                    if (string.Equals(response, "unlocked", StringComparison.OrdinalIgnoreCase))
                        return true;
                    else
                    {
                        var parts = response.Split(',');

                        foreach (var p in parts)
                        {
                            var newDate = new DateTime(Convert.ToInt64(p));

                            if (!lockedDates.Any(x => x.Date == newDate.Date))
                                lockedDates.Add(newDate);
                        }

                        return false;

                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CheckIfOrdersAreStillValid(List<Order> orders)
        {
            try
            {
                var date = orders[0].ShipDate.Date;

                var oo = orders.Where(x => x.OrderType != OrderType.Credit && x.OrderType != OrderType.Return).ToList();

                if (oo.Count > 0)
                    date = oo[0].ShipDate.Date;

                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("CheckIfLoadsAreValidCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "|" + date.ToString(CultureInfo.InvariantCulture));

                    string toSend = string.Empty;

                    var ticks = orders.Select(x => x.UniqueId).Distinct().ToList();

                    toSend = string.Join(',', ticks);

                    access.WriteStringToNetwork(toSend.ToString(CultureInfo.InvariantCulture));

                    string response = access.ReadStringFromNetwork()?.Trim();
                    access.CloseConnection();

                    if (response == "valid")
                        return true;

                    return false;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }


        public static bool DeletePayment(string uniqueId)
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    //open the connection
                    netaccess.OpenConnection();

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("DeleteTempPaymentCommand");
                    netaccess.WriteStringToNetwork(uniqueId);

                    var result = netaccess.ReadStringFromNetwork();

                    netaccess.CloseConnection();

                    return result == "done";
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error deleting payment from OS => " + ex.ToString());
                return false;
            }
        }



        static Product CreateProductInactive(string[] currentrow)
        {
            try
            {
                var prod = new Product();

                prod.ProductId = Convert.ToInt32(currentrow[ProductIDIndex], CultureInfo.InvariantCulture);
                prod.Name = currentrow[ProductNameIndex];
                prod.Upc = currentrow[ProductUPCIndex];
                prod.Comment = currentrow[ProductCommentIndex];
                prod.Package = currentrow[ProductPackageIndex];

                if (string.IsNullOrEmpty(prod.Package))
                    prod.Package = "1";

                prod.Description = currentrow[ProductDescriptionIndex];
                try
                {
                    prod.PriceLevel0 = Convert.ToDouble(currentrow[ProductPriceLevel0Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel1 = currentrow[ProductPriceLevel1Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel1Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel2 = currentrow[ProductPriceLevel2Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel2Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel3 = currentrow[ProductPriceLevel3Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel3Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel4 = currentrow[ProductPriceLevel4Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel4Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel5 = currentrow[ProductPriceLevel5Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel5Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel6 = currentrow[ProductPriceLevel6Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel6Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel7 = currentrow[ProductPriceLevel7Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel7Index], CultureInfo.InvariantCulture);
                    prod.PriceLevel8 = currentrow[ProductPriceLevel8Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel8Index], CultureInfo.InvariantCulture);
                    prod.LowestAcceptablePrice = currentrow[ProductPriceLevel9Index].Length == 0 ? 0 : Convert.ToDouble(currentrow[ProductPriceLevel9Index], CultureInfo.InvariantCulture);

                    prod.CategoryId = Convert.ToInt32(currentrow[ProductCategoryIDIndex], CultureInfo.InvariantCulture);
                }
                catch
                {

                    prod.PriceLevel0 = 0;
                    prod.PriceLevel1 = 0;
                    prod.PriceLevel2 = 0;
                    prod.PriceLevel3 = 0;
                    prod.PriceLevel4 = 0;
                    prod.PriceLevel5 = 0;
                    prod.PriceLevel6 = 0;
                    prod.PriceLevel7 = 0;
                    prod.PriceLevel8 = 0;
                    prod.LowestAcceptablePrice = 0;
                    prod.CategoryId = 1;
                }

                if (currentrow.Length > ProductOnHandIndex)
                {
                    float onHand = 0;
                    float.TryParse(currentrow[ProductOnHandIndex], out onHand);
                    prod.OnHand = onHand;
                }

                prod.OriginalId = currentrow[ProductOriginalIDIndex];

                if (currentrow.Length > ProductExtraFieldsIndex)
                    prod.ExtraPropertiesAsString = currentrow[ProductExtraFieldsIndex];
                else
                    prod.ExtraPropertiesAsString = string.Empty;

                if (currentrow.Length > ProductTaxableIndex)
                {
                    prod.Taxable = currentrow[ProductTaxableIndex] == "1";
                }
                else
                    // by default ALL products are taxables
                    prod.Taxable = true;

                if (currentrow.Length > ProductItemTypeIndex)
                    prod.ProductType = (ProductType)Convert.ToInt32(currentrow[ProductItemTypeIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.ProductType = ProductType.Inventory;
                //if (prod.ProductType == ProductType.Discount)
                //    Logger.CreateLog("A discount " + prod.Name);
                if (currentrow.Length > ProductUoMIndex)
                    prod.UoMFamily = currentrow[ProductUoMIndex];
                else
                    prod.UoMFamily = null;

                if (currentrow.Length > ProductNonVisibleExtraFieldIndex)
                    prod.NonVisibleExtraFieldsAsString = currentrow[ProductNonVisibleExtraFieldIndex];
                else
                    prod.NonVisibleExtraFieldsAsString = string.Empty;

                //LOWELL FOOD
                if (!string.IsNullOrEmpty(prod.NonVisibleExtraFieldsAsString) && prod.NonVisibleExtraFieldsAsString.Contains("color type="))
                {
                    Config.ProductNameHasDifferentColor = true;
                }

                if (currentrow.Length > ProductSoldByWeightIndex)
                    prod.SoldByWeight = Convert.ToInt32(currentrow[ProductSoldByWeightIndex], CultureInfo.InvariantCulture) > 0;
                else
                    prod.SoldByWeight = false;

                if (currentrow.Length > CodeIndex)
                    prod.Code = currentrow[CodeIndex];
                else
                    prod.Code = string.Empty;

                if (currentrow.Length > ProductCostIndex)
                    prod.Cost = Convert.ToDouble(currentrow[ProductCostIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.Cost = prod.LowestAcceptablePrice;

                if (currentrow.Length > ProductOrderInCategoryIndex)
                    prod.OrderInCategory = Convert.ToInt32(currentrow[ProductOrderInCategoryIndex], CultureInfo.InvariantCulture);
                else
                    // by default ALL products are inventory
                    prod.OrderInCategory = 0;

                if (currentrow.Length > 31)
                    prod.RetailPrice = Convert.ToDouble(currentrow[31]);

                if (currentrow.Length > ProductWLocationIndex)
                    prod.WarehouseLocation = currentrow[ProductWLocationIndex];
                else
                    prod.WarehouseLocation = string.Empty;

                if (currentrow.Length > 28)
                    prod.Weight = Convert.ToDouble(currentrow[28]);

                if (currentrow.Length > ProductTaxRateIndex)
                    prod.TaxRate = Convert.ToDouble(currentrow[ProductTaxRateIndex], CultureInfo.InvariantCulture);
                else
                    prod.TaxRate = 0;

                if (currentrow.Length > ProductDiscountCategoryIndex)
                    prod.DiscountCategoryId = Convert.ToInt32(currentrow[ProductDiscountCategoryIndex], CultureInfo.InvariantCulture);
                else
                    prod.DiscountCategoryId = 0;

                if (currentrow.Length > ProductPriceCategoryIdIndex)
                    prod.PriceCategoryId = Convert.ToInt32(currentrow[ProductPriceCategoryIdIndex], CultureInfo.InvariantCulture);
                else
                    prod.PriceCategoryId = 0;

                if (currentrow.Length > 32)
                    prod.Sku = currentrow[32];
                else
                    prod.Sku = string.Empty;

                if (currentrow.Length > 33 && DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "30.0.0.0"))
                    prod.UseLot = Convert.ToInt32(currentrow[33]) > 0;

                if (currentrow.Length > 37)
                    prod.PalletSize = Convert.ToDouble(currentrow[37]);

                if (currentrow.Length > 38)
                {
                    if (currentrow[38].ToLowerInvariant() == "true" || currentrow[38].ToLowerInvariant() == "false")
                        prod.UseLotAsReference = Convert.ToBoolean(currentrow[38]);
                    else
                        prod.UseLotAsReference = Convert.ToInt32(currentrow[38]) > 0;
                }

                if (currentrow.Length > 39)
                {
                    prod.VendorId = Convert.ToInt32(currentrow[39]);
                }

                if (!string.IsNullOrEmpty(prod.NonVisibleExtraFieldsAsString))
                {
                    var nef = DataAccess.GetSingleUDF("inventoryByWeight", prod.NonVisibleExtraFieldsAsString);
                    prod.InventoryByWeight = !string.IsNullOrEmpty(nef) && nef == "1";
                }

                if (!string.IsNullOrEmpty(prod.ExtraPropertiesAsString))
                {
                    var caseCount = DataAccess.GetSingleUDF("CASECOUNT", prod.ExtraPropertiesAsString);
                    int cc = 1;
                    int.TryParse(caseCount, out cc);
                    prod.CaseCount = cc;
                }

                var inv = ProductInventory.GetInventoryForProduct(prod.ProductId);
                if (inv == null)
                {
                    inv = new ProductInventory() { ProductId = prod.ProductId, WarehouseInventory = prod.OnHand };
                    ProductInventory.CurrentInventories.Add(prod.ProductId, inv);
                }

                prod.ProductInv = inv;

                //make sure is not visible for orders 
                prod.CategoryId = 0;

                return prod;

            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                Logger.CreateLog(DataAccess.Concatenate(currentrow));
                return null;
            }
        }

        public static void GetInactiveProducts(List<int> inactiveProd)
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    var tempFile = Path.GetTempFileName();
                    //open the connection
                    netaccess.OpenConnection();

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetInactiveProductsCommand");
                    netaccess.WriteStringToNetwork(string.Join(',', inactiveProd));

                    var result = netaccess.ReceiveFile(tempFile);

                    using (StreamReader reader = new StreamReader(tempFile))
                    {
                        string currentline;

                        while ((currentline = reader.ReadLine()) != null)
                        {
                            string[] currentrow = currentline.Split(DataLineSplitter);

                            var pid = Convert.ToInt32(currentrow[ProductIDIndex], CultureInfo.InvariantCulture);

                            if (Product.Find(pid) != null)
                                continue;

                            var product = CreateProductInactive(currentrow);

                            if (product != null)
                                Product.UpdateProduct(product);
                        }
                    }

                    netaccess.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                //Logger.CreateLog("Error getting => " + ex.ToString());
            }
        }
        
        
        public static void ExportData(string subject = "")
        {
            //if (FileOperationsLocker.InUse)
            //{
            //    ActivityExtensionMethods.DisplayDialog(this, GetString(Resource.String.alert), GetString(Resource.String.errorSendingLogFileData));
            //    return;
            //}

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    var fastZip = new FastZip();

                    bool recurse = true;  // Include all files by recursing through the directory structure
                    string filter = null; // Dont filter any files at all

                    // Serialize the config
                    var sb = new StringBuilder();

                    foreach (var line in Config.SerializeConfig().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = line.Split(new char[] { '=' });
                        if (parts.Length == 2)
                        {
                            if (sb.Length > 0)
                                sb.Append("|");
                            sb.Append(parts[0]);
                            sb.Append("=");
                            switch (parts[1])
                            {
                                case "True":
                                    sb.Append("1");
                                    break;
                                case "False":
                                    sb.Append("0");
                                    break;

                                default:
                                    sb.Append(parts[1]);
                                    break;
                            }
                        }
                    }
                    string p = Path.Combine(Config.CodeBase, "serialized_config");

                    using (var writer = new StreamWriter(p))
                        writer.Write(sb.ToString());

                    fastZip.CreateZip(Config.ExportImportPath, Config.CodeBase, recurse, filter);

                    var l = new FileInfo(Config.ExportImportPath).Length;

                    try
                    {
                        NetAccess access = new NetAccess();

                        access.OpenConnection("app.laceupsolutions.com", 9999);
                        access.WriteStringToNetwork("SendLogFile");

                        var serializedConfig = subject + Config.SerializeConfig().Replace(System.Environment.NewLine, "<br>");
                        serializedConfig = serializedConfig.Replace("'", "");
                        serializedConfig = serializedConfig.Replace("’", "");

                        access.WriteStringToNetwork(serializedConfig);

                        access.SendFile(Config.ExportImportPath);
                        access.WriteStringToNetwork("Goodbye");

                        Thread.Sleep(1000);
                        access.CloseConnection();

                        DialogHelper._dialogService.ShowAlertAsync("Debug Data Sent!");
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        DialogHelper._dialogService.ShowAlertAsync($"Error exporting data. {ex.Message}");

                    }
                    finally
                    {
                        File.Delete(Config.ExportImportPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    DialogHelper._dialogService.ShowAlertAsync($"Error exporting data. {ex.Message}");
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }
        
        public static async void RemoteControl()
        {
            try
            {
                string appId = string.Empty;
                string storeUri = string.Empty;

                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    appId = "661649585";
                    storeUri = $"https://apps.apple.com/app/id{appId}";
                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    appId = "com.teamviewer.quicksupport.market";
                    storeUri = $"https://play.google.com/store/apps/details?id={appId}";
                }

                if (!string.IsNullOrEmpty(storeUri))
                {
                    await Launcher.OpenAsync(new Uri(storeUri));
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }
    }
}