using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Security.Policy;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace LaceupMigration
{
    public class DataAccess : DataAccessEx
    {
        public const int EntitiesToReadInTestMode = 100;

        #region OpenInvoice Index

        public static int OpenInvoiceClientIdIndex = 0;
        static int OpenInvoiceInvoiceNumberIndex = 1;
        static int OpenInvoiceAmountIndex = 2;
        static int OpenInvoiceBalanceIndex = 3;
        static int OpenInvoiceDateIndex = 4;
        static int OpenInvoiceDueDateIndex = 5;

        #endregion

        public static bool ExternalInvoicesLoaded = true;

        public static char[] ExtraPropertiesSeparator = new char[] { '|' };
        public static char[] DataLineSplitter = new char[] { (char)20 };
        public static char[] ExtraPropertySeparator = new char[] { '=' };
        
        public static bool PendingLoadToAccept { get; set; }

        public static bool ReceivedData { get; set; }

        public static double LastLongitude { get; set; }

        public static double LastLatitude { get; set; }

        public static int RouteOrdersCount { get; set; }

        public static string CommunicatorVersion { get; set; }

        static DateTime lastEndOfDay = DateTime.MinValue;
        public static DateTime LastEndOfDay { get { return lastEndOfDay; } set { lastEndOfDay = value; } }

        static int waitBeforeStartSec = 5;
        public static int WaitBeforeStart
        {
            get { return waitBeforeStartSec; }
            set { waitBeforeStartSec = value; }
        }

        public static bool DeliveryAccepted = false;

        public static bool AcceptInventoryReadOnly { get; set; }

        static Dictionary<int, Product> notFoundProducts = new Dictionary<int, Product>();

        public static void LoadDeliveriesInSite(string file)
        {
            try
            {

                Dictionary<int, Batch> createdBatches = new Dictionary<int, Batch>();
                Dictionary<int, Order> createdOrders = new Dictionary<int, Order>();

                if (File.Exists(Config.TmpDeliveryClientsFile))
                    File.Delete(Config.TmpDeliveryClientsFile);

                using (StreamWriter writer = new StreamWriter(Config.TmpDeliveryClientsFile))
                {
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
                                case "NEWOFFERS":
                                    currenttable = 6;
                                    continue;
                                case "ENDOFTABLE":
                                    currenttable = -1;
                                    continue;
                            }

                            string[] currentrow = currentline.Split(DataLineSplitter);

                            switch (currenttable)
                            {
                                case 1:
                                    CreateClient(currentrow, true, true);
                                    writer.WriteLine(currentline);
                                    continue;
                                case 2:
                                    CreateBatch(currentrow, createdBatches);
                                    continue;
                                case 3:
                                    CreateOrder(currentrow, createdBatches, createdOrders);
                                    continue;
                                case 4:
                                    CreateRouteEX(currentrow);
                                    continue;
                                case 5:
                                    CreateOrderDetails(currentrow, createdBatches, createdOrders);
                                    continue;
                                case 6:
                                    CreateOffer(currentrow, true);
                                    continue;
                            }
                        }
                    }
                }

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

                RouteEx.Save();

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

        public static bool SendCurrentSession(string file)
        {
            try
            {
                NetAccess.SendSession(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SendDeposit()
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SendBankDepositCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(Config.BankDepositPath);

                    var deposit = BankDeposit.currentDeposit;
                    if (deposit != null && !string.IsNullOrEmpty(deposit.ImageId))
                    {
                        access.WriteStringToNetwork("image");

                        var images = SerializeDepositImages();
                        access.SendFile(images);
                    }
                    else
                        access.WriteStringToNetwork("no image");


                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                    throw;
                }
            }
        }

        private static string SerializeDepositImages()
        {
            try
            {
                var tempPathFile = Path.Combine(Path.GetTempPath(), "depositImages.zip");

                if (File.Exists(tempPathFile))
                    File.Delete(tempPathFile);

                string tempPathFolder = Path.Combine(Path.GetTempPath(), "DepositImages");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                Directory.CreateDirectory(tempPathFolder);

                DirectoryInfo dir = new DirectoryInfo(Config.DepositImagesPath);
                if (!dir.Exists)
                {
                    Logger.CreateLog(Config.DepositImagesPath + " directory not found");
                    return string.Empty;
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(tempPathFolder, file.Name);
                    file.CopyTo(temppath, false);
                }

                var fastZip = new FastZip();
                fastZip.CreateZip(tempPathFile, tempPathFolder, true, null);

                return tempPathFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static void LoadReasons()
        {
            try
            {
                Reason.Clear();

                if (File.Exists(Config.ReasonsFile))
                {
                    using (var reader = new StreamReader(Config.ReasonsFile))
                    {
                        string currentline;

                        while ((currentline = reader.ReadLine()) != null)
                        {
                            string[] currentrow = currentline.Split(DataLineSplitter);

                            var reason = new Reason()
                            {
                                Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture),
                                Description = currentrow[1],
                                AvailableIn = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture)
                            };

                            if (currentrow.Length > 4)
                                reason.Language = currentrow[4];

                            if (currentrow.Length > 5)
                                reason.LoadingError = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture) > 0;

                            Reason.Add(reason);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendScannerToUse()
        {
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
                    netaccess.WriteStringToNetwork("UpdateScannerToUseCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + Config.ScannerToUse.ToString());

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Salesman New Scanner Info Sent");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendPrinterToUse()
        {
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
                    netaccess.WriteStringToNetwork("UpdatePrinterUseCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + Config.PrinterToUse.ToString());

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Salesman New Printer Info Sent");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }
        public static void LoadParLevels(string tmpFile)
        {
            ParLevel.Clear();

            if (File.Exists(Config.ParLevelFile))
            {
                using (var reader = new StreamReader(Config.ParLevelFile))
                {
                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);
                        CreateParLevel(currentrow);
                    }
                }
            }

            using (var reader = new StreamReader(tmpFile))
            {
                string currentline;

                while ((currentline = reader.ReadLine()) != null)
                {
                    string[] currentrow = currentline.Split(DataLineSplitter);
                    CreateParLevel(currentrow);
                }
            }
        }

        public static void LoadFutureRoutes()
        {
            if (!File.Exists(Config.FutureRoutesFile))
                return;
            using (var reader = new StreamReader(Config.FutureRoutesFile))
            {
                string currentline;

                bool readingRoutes = true;
                bool readingClients = false;

                List<string> routes = new List<string>();

                while ((currentline = reader.ReadLine()) != null)
                {
                    if (currentline == "NewClients")
                    {
                        readingRoutes = false;
                        readingClients = true;
                        continue;
                    }

                    if (readingRoutes)
                        routes.Add(currentline);
                    else if (readingClients)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);
                        CreateClient(currentrow, true);
                    }
                }

                foreach (var currentline1 in routes)
                {
                    string[] currentrow = currentline1.Split(DataLineSplitter);
                    CreateRouteEX(currentrow);
                }
            }
        }

        public static void CreateShipment(string[] currentrow)
        {
            try
            {
                Shipment shp = new Shipment();
                shp.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                shp.Name = currentrow[1];
                shp.ExtraFields = currentrow[2];
                shp.Date = DateTime.FromBinary(Convert.ToInt64(currentrow[3], CultureInfo.InvariantCulture));
                shp.TruckId = Convert.ToInt32(currentrow[4], CultureInfo.InvariantCulture);
                shp.DriverId = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture);
                shp.TruckName = currentrow[13];

                Shipment.CurrentShipment = shp;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void CreateBatch(string[] currentrow, Dictionary<int, Batch> batches)
        {
            try
            {
                int oldId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);

                var client = Client.Find(clientId);
                if (client == null)
                {
                    Logger.CreateLog(" CreateBatch had reference to a non existing client: " + clientId);
                    client = Client.CreateTemporalClient(clientId);
                }
                Batch batch = new Batch(client);
                batch.ClockedIn = DateTime.MinValue;
                batch.ClockedOut = DateTime.MinValue;
                batch.PrintedId = currentrow[5];
                batch.Status = BatchStatus.Open;
                batches.Add(oldId, batch);
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                //Xamarin.Insights.Report(e);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateOrder(string[] currentrow, Dictionary<int, Batch> batches, Dictionary<int, Order> createdOrders)
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
                }

                var order = Order.Orders.FirstOrDefault(x => x.UniqueId == currentrow[21]);
                bool addToCreated = true;

                // see if this order already exists
                if (order != null)
                {
                    Logger.CreateLog("CreateOrder had reference to an already existing order: UniqueId: " + currentrow[21] + " ID " + currentrow[0]);

                    if (order.OrderType == OrderType.Quote)
                    {
                        Logger.CreateLog("Do not modify quotes after accepted");
                        return;
                    }

                    if (order.Modified || !order.PendingLoad)
                    {
                        Logger.CreateLog("The order with UniqueId: " + currentrow[21] + " was modified. Do not update it");
                        return;
                    }
                    else
                        order.Details.Clear();

                    addToCreated = false;
                }
                else
                    order = new Order(client, false);

                order.OriginalOrderId = orderId;
                var batchId = Convert.ToInt32(currentrow[20], CultureInfo.InvariantCulture);
                order.BatchId = batches[batchId].Id;
                order.Comments = currentrow[12];
                order.CompanyName = string.Empty;
                order.Date = Convert.ToDateTime(currentrow[5], CultureInfo.InvariantCulture);
                order.Latitude = 0;
                order.Longitude = 0;
                order.PrintedOrderId = currentrow[9];
                order.OrderType = (OrderType)Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
                order.PONumber = currentrow[18];
                order.SalesmanId = Config.SalesmanId;
                order.ShipDate = Convert.ToDateTime(currentrow[19], CultureInfo.InvariantCulture);
                order.TaxRate = Convert.ToDouble(currentrow[14], CultureInfo.InvariantCulture);
                order.UniqueId = currentrow[21];
                order.ExtraFields = currentrow[15];
                order.OriginalSalesmanId = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture); //original salesman Id
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

                if (currentrow.Length > 37)
                    order.AssetId = Convert.ToInt32(currentrow[37]);

                if (order.OrderType == OrderType.Load || order.OrderType == OrderType.Quote)
                    order.PendingLoad = true;

                if (Config.DisolCustomIdGenerator)
                    order.PrintedOrderId = string.Empty;

                var conspar = GetSingleUDF("consignmentpar", order.ExtraFields);
                if (!string.IsNullOrEmpty(conspar) && conspar == "1")
                {
                    order.OrderType = OrderType.Consignment;
                    LoadZeroLines(order);
                }

                if (Config.UseClientClassAsCompanyName)
                {
                    var cName = client.NonVisibleExtraProperties != null ? client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "classname") : null;
                    if (cName != null)
                        order.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cName.Item2.ToLower());
                }

                if (Config.SplitDeliveryByDepartment)
                {
                    var mustSplit = GetSingleUDF("splitByDepartment", client.ExtraPropertiesAsString);
                    order.SplitedByDepartment = string.IsNullOrEmpty(mustSplit) || mustSplit != "Y";
                }

                if (currentrow.Length > 33)
                {
                    order.CompanyId = Convert.ToInt32(currentrow[33], CultureInfo.InvariantCulture);
                    var company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                    order.CompanyName = company != null ? company.CompanyName : "";
                }

                if (currentrow.Length > 38)
                    order.OtherCharges = Convert.ToDouble(currentrow[38], CultureInfo.InvariantCulture);
                if (currentrow.Length > 39)
                    order.OtherChargesType = Convert.ToInt32(currentrow[39], CultureInfo.InvariantCulture);
                if (currentrow.Length > 40)
                    order.OtherChargesComment = currentrow[40];
                if (currentrow.Length > 41)
                    order.Freight = Convert.ToDouble(currentrow[41], CultureInfo.InvariantCulture);
                if (currentrow.Length > 42)
                    order.FreightType = Convert.ToInt32(currentrow[42], CultureInfo.InvariantCulture);
                if (currentrow.Length > 43)
                    order.OtherChargesComment = currentrow[43];

                if ((order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return) &&
                    order.OtherChargesType == (int)OrderFreightType.Amount)
                {
                    order.OtherCharges *= -1;
                }

                if ((order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return) &&
                    order.FreightType == (int)OrderFreightType.Amount)
                {
                    order.FreightType *= -1;
                }

                if (Config.MilagroCustomization && order.OrderType != OrderType.Load)
                    order.PrintedOrderId = string.Empty;

                if (addToCreated)
                {
                    createdOrders.Add(orderId, order);
                    DeliveryAccepted = true;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                //Xamarin.Insights.Report(e);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        static void LoadZeroLines(Order order)
        {
            var l = GetSingleUDF("zerolines", order.ExtraFields);
            if (!string.IsNullOrEmpty(l))
            {
                var lines = l.Split(';');

                foreach (var item in lines)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    var parts = item.Split(',');

                    var prodId = Convert.ToInt32(parts[0]);

                    var prod = Product.Find(prodId);
                    if (prod == null)
                        continue;

                    string extrafields = "";
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (extrafields.Length > 0)
                            extrafields += '|';

                        extrafields += parts[i];
                    }

                    var detail = new OrderDetail(prod, 0, order);
                    detail.ExtraFields = extrafields.Replace(':', '=');

                    var fromPar = GetSingleUDF("frompar", detail.ExtraFields);
                    if (!string.IsNullOrEmpty(fromPar) && fromPar == "1")
                        detail.ParLevelDetail = true;

                    order.AddDetail(detail);
                }
            }

            order.ExtraFields = RemoveSingleUDF("zerolines", order.ExtraFields);
        }

        public static void CreateOrderDetails(string[] currentrow, Dictionary<int, Batch> batches, Dictionary<int, Order> createdOrders)
        {
            try
            {
                int id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                int orderId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                if (!createdOrders.ContainsKey(orderId))
                {
                    Logger.CreateLog(" CreateOrderDetails had reference to a not created order: " + currentrow[1]);

                    return;
                }

                var order = createdOrders[orderId];
                int productId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);

                var product = Product.Find(productId, true);

                if (product == null)
                {
                    if (notFoundProducts.ContainsKey(productId))
                    {
                        product = notFoundProducts[productId];
                        Product.AddProduct(product);
                        notFoundProducts.Remove(productId);
                    }
                    else
                        product = Product.CreateNotFoundProduct(productId);
                    Logger.CreateLog("product with ID " + productId + " was not found");
                }

                var qty = Convert.ToSingle(currentrow[3], CultureInfo.InvariantCulture);
                var price = Convert.ToDouble(currentrow[4], CultureInfo.InvariantCulture);
                var detail = new OrderDetail(product, qty, order);
                detail.Price = price;
                detail.ExpectedPrice = price;
                detail.Comments = currentrow[5];

                // if (Config.HidePriceInTransaction) detail.Price = 0;

                if (product.SoldByWeight)
                {
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

                    if (product.FixedWeight && Config.NewAddItemRandomWeight)
                    {
                        detail.Weight = (float)product.Weight;
                    }

                    detail.Qty = detail.Ordered = 1;
                }
                else
                {
                    detail.Ordered = qty;
                    detail.Weight = 0;
                }

                if (!string.IsNullOrEmpty(currentrow[6]))
                    detail.FromOffer = Convert.ToBoolean(currentrow[6], CultureInfo.InvariantCulture);
                else
                    detail.FromOffer = false;

                detail.Substracted = false;

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
                    if (uom == null)
                    {
                        var inactive = UnitOfMeasure.InactiveUoM.FirstOrDefault(x => x.Id == uomid);
                        if (inactive != null)
                        {
                            uom = product.UnitOfMeasures.FirstOrDefault(x => x.Conversion == inactive.Conversion);

                            if (uom == null)
                                uom = inactive;
                        }
                    }

                    detail.UnitOfMeasure = uom;

                    detail.OriginalUoM = detail.UnitOfMeasure;

                    if (product.Name == "PRODUCT NOT FOUND" && uom != null)
                        product.UoMFamily = uom.FamilyId;
                }
                else
                    detail.UnitOfMeasure = null;

                if (currentrow.Length > 10)
                    detail.Damaged = Convert.ToInt32(currentrow[10], CultureInfo.InvariantCulture) > 0;
                else
                    detail.Damaged = false;

                if (currentrow.Length > 11)
                    detail.Lot = currentrow[11];
                else
                    detail.Lot = string.Empty;

                if (currentrow.Length > 12)
                    detail.ExtraFields = currentrow[12];
                else
                    detail.ExtraFields = string.Empty;

                if (currentrow.Length > 13)
                    detail.Taxed = Convert.ToInt32(currentrow[13], CultureInfo.InvariantCulture) > 0;

                if (currentrow.Length > 14)
                    detail.TaxRate = Convert.ToDouble(currentrow[14], CultureInfo.InvariantCulture);

                if (currentrow.Length > 15)
                    detail.DiscountType = (DiscountType)Convert.ToInt32(currentrow[15], CultureInfo.InvariantCulture);

                if (currentrow.Length > 16)
                    detail.Discount = Convert.ToDouble(currentrow[16], CultureInfo.InvariantCulture);

                if (currentrow.Length > 19)
                    detail.LotExpiration = new DateTime(Convert.ToInt64(currentrow[19]));

                if (currentrow.Length > 20)
                    detail.ReasonId = Convert.ToInt32(currentrow[20]);

                var fromPar = GetSingleUDF("frompar", detail.ExtraFields);
                if (!string.IsNullOrEmpty(fromPar) && fromPar == "1")
                    detail.ParLevelDetail = true;

                if (Config.ScanDeliveryChecking)
                {
                    int extra = 0;
                    if (detail.Ordered % 1 > 0)
                        extra++;

                    detail.DeliveryQty = new int[(int)detail.Ordered + extra];
                    detail.Id = id;
                }

                if (detail.FromOffer)
                {
                    var offerType = DataAccess.GetSingleUDF("OFFERTYPE", detail.ExtraFields);
                    if (!string.IsNullOrEmpty(offerType))
                        detail.FromOfferType = Convert.ToInt32(offerType);
                    else
                    {
                        var fromOfferPrice = DataAccess.GetSingleUDF("fromOfferPrice", detail.ExtraFields);
                        if (!string.IsNullOrEmpty(fromOfferPrice))
                            detail.FromOfferType = 0;
                    }
                }

                if (Config.SplitDeliveryByDepartment)
                {
                    var department = GetSingleUDF("department", product.ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(department))
                        detail.ProductDepartment = department;
                }

                detail.IsFreeItem = !detail.FromOffer && detail.Price == 0;

                order.AddDetail(detail);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public static bool SendClientLocation(Client client)
        {
            using (NetAccess netaccess = new NetAccess())
            {
                //open the connection
                netaccess.OpenConnection();
                // Get the products file
                netaccess.WriteStringToNetwork("HELO");
                netaccess.WriteStringToNetwork(Config.GetAuthString());

                netaccess.WriteStringToNetwork("SetClientLocationCommand");

                netaccess.WriteStringToNetwork(client.ClientId + "|" + client.InsertedLatitude + "|" + client.InsertedLongitude);

                bool sucess = false;
                var confirmation = netaccess.ReadStringFromNetwork();
                sucess = confirmation == "success";

                //Close the connection and disconnect
                netaccess.WriteStringToNetwork("Goodbye");
                Thread.Sleep(1000);
                netaccess.CloseConnection();

                return sucess;
            }
        }

        public static void CreateBranch(string[] currentrow)
        {
            try
            {
                if (currentrow[1].Length == 0)
                    return;
                Branch branch = new Branch();
                branch.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                branch.Name = currentrow[1];
                branch.ExtraFields = currentrow[2];
                branch.Latitude = Convert.ToDouble(currentrow[3]);
                branch.Longitude = Convert.ToDouble(currentrow[4]);
                branch.Active = Convert.ToInt32(currentrow[5]);
                branch.Color = Convert.ToInt32(currentrow[6]);
                branch.Address1 = currentrow[7];
                branch.Address2 = currentrow[8];
                branch.City = currentrow[9];
                branch.State = currentrow[10];
                branch.ZipCode = currentrow[11];
                branch.InventorySiteId = Convert.ToInt32(currentrow[12]);
                branch.Email = currentrow[13];
                branch.Phone = currentrow[14];
                Branch.List.Add(branch);
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateRouteEX(string[] currentrow)
        {
            try
            {
                var route = new RouteEx();

                route.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                int clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                if (clientId > 0)
                    route.Client = Client.Find(clientId);

                route.Date = Convert.ToDateTime(currentrow[2], CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(currentrow[3]))
                {
                    route.Order = Order.Orders.FirstOrDefault(x => x.UniqueId == currentrow[3]);
                    if (route.Order == null)
                    {
                        Logger.CreateLog("The order with ID " + currentrow[3] + " was not found. These are the orders in the system: ");
                        foreach (var o in Order.Orders)
                            Logger.CreateLog("     Order id: " + o.OrderId);
                        return;
                    }
                }

                route.Stop = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture);
                route.FromDelivery = route.Order != null;

                if (currentrow.Length > 11)
                    route.ExtraFields = currentrow[11];

                if (route.Order == null && route.Client == null)
                    Logger.CreateLog("Got a route but could not find the order nor the client, the routeID was: " + currentrow[0]);
                else
                {
                    var oldRoute = RouteEx.Routes.FirstOrDefault(x => x.Id == route.Id);
                    if (oldRoute != null)
                    {
                        if (route.FromDelivery && !oldRoute.FromDelivery)
                        {
                            oldRoute.Order = route.Order;
                            oldRoute.FromDelivery = true;
                        }
                        oldRoute.Stop = route.Stop;
                    }
                    else
                        RouteEx.Routes.Add(route);
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        static void CreateParLevel(string[] currentrow)
        {
            try
            {
                var prodId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var qty = Convert.ToSingle(currentrow[1], CultureInfo.InvariantCulture);

                var product = Product.Products.FirstOrDefault(x => x.ProductId == prodId);

                if (product != null && ParLevel.List.FirstOrDefault(x => x.Product.ProductId == prodId) == null)
                    ParLevel.List.Add(new ParLevel() { Product = product, Qty = qty });
                else
                {
                    Logger.CreateLog("The product with ID " + currentrow[0] + " was not found. These are the par levels in the system: ");
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                //Xamarin.Insights.Report(e);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        static DataAccess()
        {
        }

        public static bool MustEndOfDay()
        {
            if (!Config.MustEndOfDayDaily)
                return false;
            if (Order.Orders.Count == 0 && InvoicePayment.List.Count == 0)
                return false;
            var lastTime = Config.GetLastEndOfDay();
            if (lastTime.Year == 1)
                return false;

            if (Config.ForceEODWhenDateChanges)
            {
                var isSameDay = DateTime.Now.Date == lastTime.Date;
                if (!isSameDay)
                {
                    if (Order.Orders.Count(x => !x.IsDelivery && x.Date.Date != DateTime.Now.Date) > 0 ||
                        InvoicePayment.List.Count(x => x.DateCreated.Date != DateTime.Now.Date) > 0)
                        return true;
                }
            }
            else
            {
                var days = DateTime.Now.Subtract(lastTime).TotalDays;
                if (days > 0)
                {
                    if (Order.Orders.Count(x => !x.IsDelivery && DateTime.Now.Subtract(x.Date).Days > 0) > 0 ||
                        InvoicePayment.List.Count(x => DateTime.Now.Subtract(x.DateCreated).Days > 0) > 0)
                        return true;
                }
            }

            return false;
        }

        public static void CheckAuthorization()
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());
                    netaccess.WriteStringToNetwork("goodbye");
                    string returned = netaccess.ReadStringFromNetwork();

                    if (returned.ToLowerInvariant().Contains("invalid auth info") || returned.ToLowerInvariant().Contains("authorization denied"))
                        Config.AuthorizationFailed = true;
                    else
                        Config.AuthorizationFailed = false;
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public static bool CanUseApplication()
        {
            if (Config.AuthorizationFailed)
                return false;

            if (!Config.MustUpdateDaily)
                return true;

            string datafile = Config.ProductStoreFile;

            DateTime oldDate = DateTime.Now;

            if (!File.Exists(Config.ProductStoreFile))
                datafile = Config.ClientStoreFile;

            if (File.Exists(datafile))
                oldDate = File.GetLastWriteTime(datafile);

            var x = DateTime.Now.AddDays(-1);

            if (oldDate.Date.CompareTo(x.Date) <= 0)
                return false;

            return true;
        }

        public static void UpdateProductImagesMap()
        {
            string tempFile = null;
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    //Set the target files
                    tempFile = Path.GetTempFileName();

                    bool error = false;

                    //open the connection
                    netaccess.OpenConnection();
                    // Get the products file
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("ProductImages");
                    error = netaccess.ReceiveFile(tempFile) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    netaccess.CloseConnection();

                    //now unzip the Product file
                    if (!error)
                    {
                        ProductImage.UpdateMap(tempFile, true);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        public static void CreateRoute(string[] currentrow)
        {
            try
            {
                var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var day = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                var clientId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
                var stop = Int32.MaxValue;

                if (currentrow.Length > 4)
                    stop = Convert.ToInt32(currentrow[4], CultureInfo.InvariantCulture);

                DateTime start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);

                var routeEx = new RouteEx();
                routeEx.Id = id;

                routeEx.Client = Client.Find(clientId);
                if (routeEx.Client == null)
                    return;

                if (day == 6 && 0 == (int)DateTime.Today.DayOfWeek)
                    routeEx.Date = DateTime.Today;
                else
                    routeEx.Date = start.AddDays(day);

                routeEx.Stop = stop;
                routeEx.FromDelivery = false;

                if (routeEx.Order == null && routeEx.Client == null)
                    Logger.CreateLog("CreateRoute has both orders and Client in null: " + currentrow[2]);
                else
                {
                    var oldRoute = RouteEx.Routes.FirstOrDefault(x => x.Id == id);
                    if (oldRoute == null)
                        RouteEx.Routes.Add(routeEx);
                    else
                        oldRoute.Stop = routeEx.Stop;
                }
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateSalesman(string[] currentrow)
        {
            try
            {
                if (currentrow[1].Length == 0)
                    return;
                Salesman salesman = new Salesman();
                salesman.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                salesman.Name = currentrow[1];
                salesman.RouteNumber = currentrow[2];
                salesman.Email = currentrow[3];
                salesman.Password = currentrow[4];
                salesman.CreatedLocally = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture) > 0;
                salesman.InventorySiteId = Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
                salesman.Phone = currentrow[7];
                salesman.IsActive = true;

                if (currentrow.Length > 8)
                    salesman.IsActive = Convert.ToInt32(currentrow[8], CultureInfo.InvariantCulture) > 0;

                if (currentrow.Length > 9)
                    salesman.OriginalId = currentrow[9];

                if (currentrow.Length > 10)
                    salesman.ExtraProperties = currentrow[10];

                if (currentrow.Length > 11)
                    salesman.PresalePrefix = currentrow[11];

                if (currentrow.Length > 12)
                    salesman.PrintedPrefix = currentrow[12];

                if (currentrow.Length > 13)
                    salesman.SequencePrefix = currentrow[13];

                if (currentrow.Length > 14)
                {
                    var ticks = Convert.ToInt64(currentrow[14]);
                    if (ticks > 0)
                        salesman.SequenceExpirationDate = DateTime.FromBinary(ticks);
                }

                if (currentrow.Length > 15)
                    salesman.SequenceFrom = Convert.ToInt32(currentrow[15]);

                if (currentrow.Length > 16)
                    salesman.SequenceTo = Convert.ToInt32(currentrow[16]);

                if (currentrow.Length > 17)
                    salesman.SequenceCAI = currentrow[17];

                if (currentrow.Length > 18)
                    salesman.BranchId = Convert.ToInt32(currentrow[18]);

                if (currentrow.Length > 19)
                    salesman.Loginname = currentrow[19];

                if (currentrow.Length > 20)
                    salesman.Roles = (SalesmanRole)Convert.ToInt32(currentrow[20]);
                else
                    salesman.Roles = SalesmanRole.Driver | SalesmanRole.DSD;

                if (Config.SalesmanSeqValues && Config.SalesmanId == salesman.Id)
                {
                    salesman.PresalePrefix = Config.InvoicePresalePrefix;
                    salesman.PrintedPrefix = Config.InvoicePrefix;
                    salesman.SequencePrefix = Config.SalesmanSeqPrefix;
                    salesman.SequenceExpirationDate = Config.SalesmanSeqExpirationDate;
                    salesman.SequenceFrom = Config.SalesmanSeqFrom;
                    salesman.SequenceTo = Config.SalesmanSeqTo;
                }

                Salesman.AddSalesman(salesman);

                if (salesman.Id == Config.SalesmanId)
                    Salesman.CurrentSalesman = salesman;
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateCategoryProduct(string[] currentrow)
        {
            try
            {
                if (currentrow[1].Length == 0)
                    return;
                CategoryProduct cat = new CategoryProduct();
                cat.id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                cat.productId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                cat.categoryId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
                CategoryProduct.Add(cat);
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }
        public static void CreateCategory(string[] currentrow)
        {
            try
            {
                if (currentrow[1].Length == 0)
                    return;
                Category cat = new Category();
                cat.CategoryId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                cat.Name = currentrow[1];
                if (currentrow.Length > 2)
                {
                    try
                    {
                        cat.VisibleIn = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);

                        if (cat.VisibleIn == 1 || cat.VisibleIn == -1)
                            return;
                    }
                    catch
                    {
                        cat.VisibleIn = 0;
                    }
                }
                else
                    cat.VisibleIn = 0;

                if (currentrow.Length > 3)
                    cat.ParentCategoryId = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);

                if (currentrow.Length > 4)
                    cat.TypeServiPart = (CategoryServiPartType)Convert.ToInt32(currentrow[4], CultureInfo.InvariantCulture);

                if (currentrow.Length > 5)
                    cat.ExtraFields = currentrow[5] ?? string.Empty;

                Category.AddCategory(cat);
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateOffer(string[] currentrow, bool checkIfExists = false)
        {
            try
            {
                Offer offer = new Offer();
                offer.OfferId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                if (currentrow.Length > 12)
                    offer.OriginalId = currentrow[12];

                if (checkIfExists)
                {
                    if (!string.IsNullOrEmpty(offer.OriginalId))
                    {
                        var alreadyAdded1 = Offer.OfferList.FirstOrDefault(x => x.OriginalId == offer.OriginalId);
                        if (alreadyAdded1 != null)
                            return;
                    }
                    else
                    {
                        var alreadyAdded = Offer.OfferList.FirstOrDefault(x => x.OfferId == offer.OfferId);
                        if (alreadyAdded != null)
                            return;
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
            catch (Exception e)
            {
                Logger.CreateLog(e);
                Logger.CreateLog(Concatenate(currentrow));
            }
        }

        public static void CreateProductPrice(string[] currentrow)
        {
            try
            {
                int productId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                int clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                ProductPrice pp = new ProductPrice();
                pp.ClientId = clientId;
                pp.ProductId = productId;
                pp.IsBasedOnPriceLevel = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture) > 0;
                pp.PriceLevelId = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                pp.Price = Convert.ToDouble(currentrow[4], CultureInfo.InvariantCulture);
                if (currentrow.Length > 5)
                    pp.Allowance = Convert.ToDouble(currentrow[5], CultureInfo.InvariantCulture);
                if (currentrow.Length > 6)
                    pp.Extrafields = currentrow[6];

                pp.PartNumber = GetSingleUDF("partnumber", pp.Extrafields);

                ProductPrice.Add(pp);
            }
            catch (Exception ex)
            {
                productPriceCounter++;
                if (productPriceCounter < 20)
                {
                    //Log the exception
                    Logger.CreateLog(ex);
                    //Xamarin.Insights.Report(ex);
                    Logger.CreateLog(Concatenate(currentrow));
                }
            }
        }

        public static void CreateInvoiceDetails(string[] currentrow)
        {
            //if (InvoiceDetail.Details.Count == 37000)
            //    return;
            try
            {
                InvoiceDetail oiDetail = new InvoiceDetail();
                oiDetail.InvoiceId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                oiDetail.ProductId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
                oiDetail.Quantity = Math.Round(Convert.ToDouble(currentrow[3], CultureInfo.InvariantCulture), 2);

                var price = Convert.ToDouble(currentrow[4], CultureInfo.InvariantCulture);

                oiDetail.Price = price;
                if (!Config.ShowInvoiceTotal)
                    oiDetail.Price = 0;

                if (currentrow.Length > 5)
                    oiDetail.ClientId = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture);
                if (currentrow.Length > 6)
                    oiDetail.Date = Convert.ToDateTime(currentrow[6], CultureInfo.InvariantCulture);
                if (currentrow.Length > 7)
                    oiDetail.Comments = currentrow[7];
                if (currentrow.Length > 8 && !string.IsNullOrEmpty(currentrow[8]))
                    oiDetail.UnitOfMeasureId = Convert.ToInt32(currentrow[8], CultureInfo.InvariantCulture);
                if (currentrow.Length > 10 && !string.IsNullOrEmpty(currentrow[10]))
                    oiDetail.ExtraFields = currentrow[10];
                //oiDetail.Product = Product.Find(oiDetail.ProductId);

                int invoiceType = price < 0 ? 1 : 0;
                Invoice.InvoiceTypeDic.TryGetValue(oiDetail.InvoiceId, out invoiceType);

                oiDetail.InvoiceType = invoiceType;

                InvoiceDetail.Add(oiDetail);
            }
            catch (Exception e)
            {
                invoiceDetailReportCounter++;
                if (invoiceDetailReportCounter < 20)
                {
                    Logger.CreateLog(e);
                    //Xamarin.Insights.Report(e);
                    Logger.CreateLog(Concatenate(currentrow));
                }
            }
        }

        public static void CreateClient(string[] currentrow, bool checkIfExists, bool fromDelivery = false)
        {
            try
            {

                Client cli = new Client();

                cli.ClientId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                var oldClient = Client.Clients.FirstOrDefault(x => x.ClientId == cli.ClientId);
                if (checkIfExists && oldClient != null)
                {
                    if (oldClient.ClientName != "CUSTOMER NOT FOUND")
                        return;

                    cli = oldClient;
                }

                cli.ClientName = currentrow[1];
                cli.ShipToAddress = currentrow[2];
                cli.PriceLevel = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                cli.Comment = currentrow[4];
                cli.ContactName = currentrow[5];
                cli.ContactPhone = currentrow[6];
                // client active is not used in iPhone
                cli.OriginalId = currentrow[8];
                cli.OpenBalance = Convert.ToDouble(currentrow[9], CultureInfo.InvariantCulture);
                if (!Config.ShowInvoiceTotal)
                    cli.OpenBalance = 0;
                cli.CategoryId = Convert.ToInt32(currentrow[10], CultureInfo.InvariantCulture);

                if (currentrow.Length > 11)
                    cli.OverCreditLimit = Convert.ToInt32(currentrow[11], CultureInfo.InvariantCulture) > 0;
                if (currentrow.Length > 12)
                {
                    cli.ExtraPropertiesAsString = currentrow[12];
                    if (!string.IsNullOrEmpty(currentrow[12]))
                    {
                        if (cli.ExtraProperties != null && cli.ExtraProperties.Count > 0)
                        {
                            var item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "DUNS");
                            if (item != null)
                            {
                                cli.DUNS = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }
                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "LOCATION");
                            if (item != null)
                            {
                                cli.Location = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }
                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "COMMID");
                            if (item != null)
                            {
                                cli.CommId = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }

                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRODSORT");
                            if (item != null && !string.IsNullOrEmpty(item.Item2))
                                ClientProdSort.CreateSort(cli.ClientId, item.Item2);
                        }
                    }
                }

                if (currentrow.Length > 13)
                    try
                    {
                        cli.Latitude = Convert.ToDouble(currentrow[13], CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        cli.Latitude = 0;
                    }
                else
                    cli.Latitude = 0;

                if (currentrow.Length > 14)
                    try
                    {
                        cli.Longitude = Convert.ToDouble(currentrow[14], CultureInfo.InvariantCulture);
                    }
                    catch { cli.Longitude = 0; }
                else
                    cli.Longitude = 0;

                if (currentrow.Length > 15)
                    cli.TaxRate = Convert.ToDouble(currentrow[15], CultureInfo.InvariantCulture);
                else
                    cli.TaxRate = 0;

                if (currentrow.Length > 16)
                    cli.NonvisibleExtraPropertiesAsString = currentrow[16];
                // 16 
                if (currentrow.Length > 17)
                {
                    int onedoc = 0;
                    Int32.TryParse(currentrow[17], out onedoc);
                    cli.OneDoc = onedoc > 0;
                }
                else
                    cli.OneDoc = false;

                if (currentrow.Length > 18)
                {
                    cli.BillToAddress = currentrow[18];
                }
                else
                    cli.BillToAddress = "";

                if (currentrow.Length > 19)
                {
                    cli.LicenceNumber = currentrow[19];
                }
                else
                    cli.LicenceNumber = "";

                if (currentrow.Length > 20)
                {
                    cli.VendorNumber = currentrow[20];
                }
                else
                    cli.VendorNumber = "";

                if (currentrow.Length > 21)
                {
                    cli.Notes = currentrow[21];
                }
                else
                    cli.Notes = "";

                if (currentrow.Length > 22)
                    cli.CreditLimit = Convert.ToDouble(currentrow[22], CultureInfo.InvariantCulture);

                if (currentrow.Length > 23)
                    cli.UniqueId = currentrow[23];

                if (currentrow.Length > 24)
                    cli.RetailPriceLevelId = Convert.ToInt32(currentrow[24], CultureInfo.InvariantCulture);

                if (currentrow.Length > 25)
                    cli.StartTimeWindows1 = new DateTime(Convert.ToInt64(currentrow[25], CultureInfo.InvariantCulture));

                if (currentrow.Length > 26)
                    cli.EndTimeWindows1 = new DateTime(Convert.ToInt64(currentrow[26], CultureInfo.InvariantCulture));

                if (currentrow.Length > 27)
                    cli.StartTimeWindows2 = new DateTime(Convert.ToInt64(currentrow[27], CultureInfo.InvariantCulture));

                if (currentrow.Length > 28)
                    cli.EndTimeWindows2 = new DateTime(Convert.ToInt64(currentrow[28], CultureInfo.InvariantCulture));

                if (currentrow.Length > 29)
                    cli.Taxable = Convert.ToInt32(currentrow[29], CultureInfo.InvariantCulture) > 0;
                else
                    cli.Taxable = true;

                if (currentrow.Length > 30)
                    cli.TermId = Convert.ToInt32(currentrow[30]);

                if (currentrow.Length > 31)
                    cli.MinimumOrderAmount = Convert.ToDouble(currentrow[31]);

                if (currentrow.Length > 32)
                    cli.MinimumOrderQty = Convert.ToDouble(currentrow[32]);

                if (currentrow.Length > 33)
                    cli.AreaId = Convert.ToInt32(currentrow[33]);

                cli.FromDelivery = fromDelivery;

                cli.SalesmanClient = cli.ClientName.StartsWith("Salesman:");

                if (string.IsNullOrEmpty(cli.UniqueId))
                    cli.UniqueId = cli.OriginalId;

                var oneOrder = GetSingleUDF("OneOrder", cli.NonvisibleExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(oneOrder) && oneOrder.ToLowerInvariant() == "yes")
                    cli.OneOrderPerDepartment = true;

                Client.AddClient(cli);
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }

        }

        public static void CreateClientEX(string[] currentrow, bool checkIfExists, bool fromDelivery = false)
        {
            try
            {

                Client cli = new Client();

                cli.ClientId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);

                var oldClient = Client.Clients.FirstOrDefault(x => x.ClientId == cli.ClientId);
                if (checkIfExists && oldClient != null)
                {
                    if (oldClient.ClientName != "CUSTOMER NOT FOUND")
                        return;

                    cli = oldClient;
                }

                cli.ClientName = currentrow[1];
                cli.ShipToAddress = currentrow[2];
                cli.PriceLevel = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                cli.Comment = currentrow[4];
                cli.ContactName = currentrow[5];
                cli.ContactPhone = currentrow[6];
                // client active is not used in iPhone
                cli.OriginalId = currentrow[8];
                cli.OpenBalance = Convert.ToDouble(currentrow[9], CultureInfo.InvariantCulture);
                if (!Config.ShowInvoiceTotal)
                    cli.OpenBalance = 0;
                cli.CategoryId = Convert.ToInt32(currentrow[10], CultureInfo.InvariantCulture);

                if (currentrow.Length > 11)
                    cli.OverCreditLimit = Convert.ToInt32(currentrow[11], CultureInfo.InvariantCulture) > 0;
                if (currentrow.Length > 12)
                {
                    cli.ExtraPropertiesAsString = currentrow[12];
                    if (!string.IsNullOrEmpty(currentrow[12]))
                    {
                        if (cli.ExtraProperties != null && cli.ExtraProperties.Count > 0)
                        {
                            var item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "DUNS");
                            if (item != null)
                            {
                                cli.DUNS = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }
                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "LOCATION");
                            if (item != null)
                            {
                                cli.Location = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }
                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "COMMID");
                            if (item != null)
                            {
                                cli.CommId = item.Item2;
                                cli.ExtraProperties.Remove(item);
                            }

                            item = cli.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRODSORT");
                            if (item != null && !string.IsNullOrEmpty(item.Item2))
                                ClientProdSort.CreateSort(cli.ClientId, item.Item2);
                        }
                    }
                }

                if (currentrow.Length > 13)
                    try
                    {
                        cli.Latitude = Convert.ToDouble(currentrow[13], CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        cli.Latitude = 0;
                    }
                else
                    cli.Latitude = 0;

                if (currentrow.Length > 14)
                    try
                    {
                        cli.Longitude = Convert.ToDouble(currentrow[14], CultureInfo.InvariantCulture);
                    }
                    catch { cli.Longitude = 0; }
                else
                    cli.Longitude = 0;

                if (currentrow.Length > 15)
                    cli.TaxRate = Convert.ToDouble(currentrow[15], CultureInfo.InvariantCulture);
                else
                    cli.TaxRate = 0;

                if (currentrow.Length > 16)
                    cli.NonvisibleExtraPropertiesAsString = currentrow[16];
                // 16 
                if (currentrow.Length > 17)
                {
                    int onedoc = 0;
                    Int32.TryParse(currentrow[17], out onedoc);
                    cli.OneDoc = onedoc > 0;
                }
                else
                    cli.OneDoc = false;

                if (currentrow.Length > 18)
                {
                    cli.BillToAddress = currentrow[18];
                }
                else
                    cli.BillToAddress = "";

                if (currentrow.Length > 19)
                {
                    cli.LicenceNumber = currentrow[19];
                }
                else
                    cli.LicenceNumber = "";

                if (currentrow.Length > 20)
                {
                    cli.VendorNumber = currentrow[20];
                }
                else
                    cli.VendorNumber = "";

                if (currentrow.Length > 21)
                {
                    cli.Notes = currentrow[21];
                }
                else
                    cli.Notes = "";

                if (currentrow.Length > 22)
                    cli.CreditLimit = Convert.ToDouble(currentrow[22], CultureInfo.InvariantCulture);

                if (currentrow.Length > 23)
                    cli.UniqueId = currentrow[23];

                if (currentrow.Length > 24)
                    cli.RetailPriceLevelId = Convert.ToInt32(currentrow[24], CultureInfo.InvariantCulture);

                if (currentrow.Length > 25)
                    cli.StartTimeWindows1 = new DateTime(Convert.ToInt64(currentrow[25], CultureInfo.InvariantCulture));

                if (currentrow.Length > 26)
                    cli.EndTimeWindows1 = new DateTime(Convert.ToInt64(currentrow[26], CultureInfo.InvariantCulture));

                if (currentrow.Length > 27)
                    cli.StartTimeWindows2 = new DateTime(Convert.ToInt64(currentrow[27], CultureInfo.InvariantCulture));

                if (currentrow.Length > 28)
                    cli.EndTimeWindows2 = new DateTime(Convert.ToInt64(currentrow[28], CultureInfo.InvariantCulture));

                cli.FromDelivery = fromDelivery;

                cli.SalesmanClient = cli.ClientName.StartsWith("Salesman:");

                if (string.IsNullOrEmpty(cli.UniqueId))
                    cli.UniqueId = cli.OriginalId;

                var oneOrder = GetSingleUDF("OneOrder", cli.NonvisibleExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(oneOrder) && oneOrder.ToLowerInvariant() == "yes")
                    cli.OneOrderPerDepartment = true;

                Client.AllActiveClients.Add(cli.ClientId, cli);
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                Logger.CreateLog(Concatenate(currentrow));
            }

        }

        static public void CreateInvoice(string[] currentrow)
        {
            try
            {
                int salesmanId = 0;
                if (currentrow.Length > 11)
                {
                    salesmanId = Convert.ToInt32(currentrow[11], CultureInfo.InvariantCulture);
                    if (Config.ShowOnlyInvoiceForSalesman && salesmanId != Config.SalesmanId)
                        return;
                }

                Invoice oi = new Invoice();

                oi.ClientId = Convert.ToInt32(currentrow[OpenInvoiceClientIdIndex], CultureInfo.InvariantCulture);

                //Client client = Client.Find(oi.ClientId);
                //if (client == null)
                //    return;
                //oi.Client = client;

                oi.InvoiceNumber = currentrow[OpenInvoiceInvoiceNumberIndex];
                oi.Amount = Convert.ToDouble(currentrow[OpenInvoiceAmountIndex], CultureInfo.InvariantCulture);
                oi.Balance = Convert.ToDouble(currentrow[OpenInvoiceBalanceIndex], CultureInfo.InvariantCulture);
                if (!Config.ShowInvoiceTotal)
                {
                    oi.Balance = 0;
                    oi.Amount = 0;
                }
                oi.Date = Convert.ToDateTime(currentrow[OpenInvoiceDateIndex], CultureInfo.InvariantCulture);
                oi.DueDate = Convert.ToDateTime(currentrow[OpenInvoiceDueDateIndex], CultureInfo.InvariantCulture);

                if (currentrow.Length > 6)
                    try
                    {
                        oi.InvoiceId = Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        oi.InvoiceId = -1;
                    }
                if (currentrow.Length > 7)
                    oi.Comments = currentrow[7];
                try
                {
                    if (currentrow.Length > 8)
                        oi.InvoiceType = Convert.ToInt32(currentrow[8], CultureInfo.InvariantCulture);
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

                //requested not to show RP with 0 balance on android
                if (oi.InvoiceType == 4 && oi.Balance == 0)
                    return;

                if (currentrow.Length > 9)
                    oi.SalesmanName = currentrow[9];

                if (currentrow.Length > 10)
                    oi.ExtraFields = currentrow[10];

                if (currentrow.Length > 11)
                    oi.SalesmanId = Convert.ToInt32(currentrow[11], CultureInfo.InvariantCulture);

                if (currentrow.Length > 12)
                    oi.Tax = Convert.ToDouble(currentrow[12], CultureInfo.InvariantCulture);

                if (currentrow.Length > 13)
                    oi.CompanyName = currentrow[13];

                Invoice.Add(oi);

                if (!Invoice.InvoiceTypeDic.ContainsKey(oi.InvoiceId))
                    Invoice.InvoiceTypeDic.Add(oi.InvoiceId, oi.InvoiceType);
            }
            catch (Exception ex)
            {
                invoiceReportCounter++;
                if (invoiceReportCounter < 20)
                {
                    //Log the exception
                    Logger.CreateLog(ex);
                    Logger.CreateLog(Concatenate(currentrow));
                    //Xamarin.Insights.Report(ex);
                }
            }
        }

        public static void CreateClientOffer(string[] currentrow)
        {
            try
            {
                ClientsOffer co = new ClientsOffer();
                co.OfferId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                co.ClientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                ClientsOffer.AddClientsOffer(co);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                Logger.CreateLog(Concatenate(currentrow));
                //Xamarin.Insights.Report(ee);
            }
        }

        public static string Concatenate(string[] parts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string part in parts)
            {
                sb.Append(",");
                sb.Append(part);
            }
            return sb.ToString();
        }

        static int invoiceDetailReportCounter;
        static int invoiceReportCounter;
        static int productPriceCounter;

        public static void CreateSalesmanAvailableCompany(string[] currentrow)
        {
            int clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            int companyId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            string extrafields = currentrow[3];

            SalesmanAvailableCompany.Add(clientId, companyId, extrafields);
        }

        public static void CreateClientAvailableCompany(string[] currentrow)
        {
            int clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            int companyId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            string extrafields = currentrow[3];

            ClientAvailableCompany.Add(clientId, companyId, extrafields);
        }

        public static void CreateCompanyInfo(string[] currentrow)
        {
            int id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            string name = currentrow[1];
            string extraFields = currentrow[2];

            CompanyInfo company = new CompanyInfo();
            company.CompanyName = name;
            company.FromFile = true;
            company.ExtraFields = extraFields;

            if (currentrow.Length > 3)
            {
                company.CompanyAddress1 = currentrow[3];
                company.CompanyAddress2 = currentrow.Length > 4 ? currentrow[4] : "";
                company.Vendor = currentrow.Length > 5 ? currentrow[5] : "";
                company.CompanyPhone = currentrow.Length > 6 ? currentrow[6] : "";
                company.DUNS = currentrow.Length > 7 ? currentrow[7] : "";
                company.Location = currentrow.Length > 8 ? currentrow[8] : "";
                company.CommId = currentrow.Length > 9 ? currentrow[9] : "";
                company.CompanyLicenses = currentrow.Length > 10 ? currentrow[10] : "";
                company.CompanyEmail = currentrow.Length > 11 ? currentrow[11] : "";

                if (currentrow.Length > 12)
                    company.IsDefault = currentrow[12] == "0" ? false : Convert.ToBoolean(currentrow[12]);

                if (currentrow.Length > 13)
                    company.ExtraFields = currentrow[13];

            }
            else
            {
                company.CompanyAddress1 = GetSingleUDF("address1", extraFields);
                company.CompanyAddress2 = GetSingleUDF("address2", extraFields);
            }

            if (currentrow.Length > 14 && !string.IsNullOrEmpty(currentrow[14]))
            {
                company.PaymentClientId = DataAccess.DecryptString(currentrow[14]);
            }

            if (currentrow.Length > 15 && !string.IsNullOrEmpty(currentrow[15]))
            {
                company.PaymentClientSecret = DataAccess.DecryptString(currentrow[15]);
            }

            if (currentrow.Length > 16 && !string.IsNullOrEmpty(currentrow[16]))
            {
                company.PaymentMerchant = DataAccess.DecryptString(currentrow[16]);
            }

            if (currentrow.Length > 17 && !string.IsNullOrEmpty(currentrow[17]))
            {
                company.BottomTextPrint = currentrow[17];
            }

            company.CompanyId = id;

            CompanyInfo.Companies.Add(company);
        }

        public static void SetRelatedProducts()
        {
            foreach (var prod in Product.Products)
            {
                foreach (var p in prod.ExtraProperties)
                {
                    if (p.Item1.ToLowerInvariant() == "relateditem")
                    {
                        var products = p.Item2.Split(",");

                        foreach (var p1 in products)
                        {
                            int o = 0;
                            Int32.TryParse(p1, out o);

                            var relatedProduct = Product.Find(o, true);

                            if (relatedProduct != null)
                                relatedProduct.IsRelatedProduct = true;
                        }
                    }
                }
            }
        }

        public static void CreateProductTaxability(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var productId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var clientId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var taxed = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture) > 0;
            var extraFields = currentrow[4];
            var taxRate = Convert.ToDouble(currentrow[5], CultureInfo.InvariantCulture);
            ProductTaxability.List.Add(new ProductTaxability() { ClientId = clientId, ExtraFields = extraFields, Id = id, ProductId = productId, Taxed = taxed, TaxRate = taxRate });
        }

        private static void CreateClientDailyParLevelHistory(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var productId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var dayOfWeek = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
            var oldQty = Convert.ToSingle(currentrow[4], CultureInfo.InvariantCulture);
            var newQty = Convert.ToSingle(currentrow[5], CultureInfo.InvariantCulture);
            var when = Convert.ToDateTime(currentrow[6], CultureInfo.InvariantCulture);
            float counted = 0;
            float sold = 0;
            float credit = 0;
            string department = string.Empty;

            if (currentrow.Length > 7)
                counted = Convert.ToSingle(currentrow[7], CultureInfo.InvariantCulture);

            if (currentrow.Length > 8)
                sold = Convert.ToSingle(currentrow[8], CultureInfo.InvariantCulture);

            if (currentrow.Length > 9)
                credit = Convert.ToSingle(currentrow[9], CultureInfo.InvariantCulture);

            if (currentrow.Length > 10)
                department = currentrow[10];

            var client = Client.Find(clientId);
            if (client == null)
                return;

            var product = Product.Products.FirstOrDefault(x => x.ProductId == productId);
            if (product == null)
                return;

            ParLevelHistory.Histories.Add(new ParLevelHistory()
            {
                Client = client,
                Product = product,
                DayOfWeek = dayOfWeek,
                OldPar = oldQty,
                NewPar = newQty,
                Date = when,
                Counted = counted,
                Sold = sold,
                Credit = credit,
                Department = department
            });
        }

        public static void CreateClientDailyParLevel(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var productId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var dayOfWeek = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
            var qty = Convert.ToSingle(currentrow[4], CultureInfo.InvariantCulture);
            string department = string.Empty;

            if (currentrow.Length > 5)
                department = currentrow[5];

            var client = Client.Find(clientId);
            if (client == null)
            {
                Logger.CreateLog("ClientDailyParLevel. Client not found with id=" + clientId);
                return;
            }

            var product = Product.Find(productId);
            if (product == null)
            {
                Logger.CreateLog("ClientDailyParLevel. Product not found with id=" + productId);
                return;
            }

            var cdpl = new ClientDailyParLevel()
            {
                Id = id,
                ProductId = productId,
                ClientId = clientId,
                DayOfWeek = dayOfWeek,
                Qty = qty,
                NewQty = qty,
                Product = product,
                Client = client,
                Department = department
            };

            ClientDailyParLevel.Add(cdpl);

        }

        public static void CreateRetailPriceLevel(string[] currentrow)
        {
            try
            {
                var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var name = currentrow[1];
                var originalId = currentrow[2];
                var retailpricelvltype = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
                var percentage = Convert.ToDouble(currentrow[4], CultureInfo.InvariantCulture);
                var createdLocally = Convert.ToInt32(currentrow[5], CultureInfo.InvariantCulture) > 0;

                RetailPriceLevel.Add(new RetailPriceLevel()
                {
                    Id = id,
                    Name = name,
                    OriginalId = originalId,
                    RetailPriceLevelType = retailpricelvltype,
                    Percentage = percentage,
                    CreatedLocally = createdLocally
                });
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Excpetion reading retail price level ==>" + ex.ToString());
            }
        }
        public static void CreateRetailProductPrices(string[] currentrow)
        {
            var pid = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var levelId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var price = Convert.ToDouble(currentrow[2], CultureInfo.InvariantCulture);
            var allowance = Convert.ToDouble(currentrow[3], CultureInfo.InvariantCulture);

            RetailProductPrice.Add(new RetailProductPrice()
            {
                ProductId = pid,
                RetailPriceLevelId = levelId,
                Price = price,
                Allowance = allowance
            });
        }

        public static ProductVisibleSalesman CreateProductVisibleToClient(string[] currentrow)
        {
            int id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            int productId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);

            return new ProductVisibleSalesman() { Id = id, ProductId = productId };
        }


        public static void CreateConsignment(string[] currentrow)
        {
            var clientID = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var client = Client.Find(clientID);
            if (client == null)
            {
                return;
            }
            if (client.ConsignmentTemplate == null)
                client.ConsignmentTemplate = new List<Consignment>();
            var pId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var product = Product.Find(pId);
            if (product == null)
            {
                return;
            }
            var qty = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
            var price = Convert.ToDouble(currentrow[3], CultureInfo.InvariantCulture);
            client.ConsignmentTemplate.Add(new Consignment() { Client = client, Product = product, Qty = qty, Price = price });
        }

        public static void CreateUnitOfMeasure(string[] currentrow)
        {
            bool isActive = true;
            if (currentrow.Length > 8)
                isActive = Convert.ToBoolean(currentrow[8], CultureInfo.InvariantCulture);

            int id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            string name = currentrow[1];
            float conversion = Convert.ToSingle(currentrow[2], CultureInfo.InvariantCulture);
            string family = currentrow[3];
            bool isBase = currentrow[4] == "1";
            bool isDefault = currentrow[5] == "1";
            string originalId = currentrow[6];
            string defaultPurchase = string.Empty;
            bool createdLocally = false;
            string familyName = string.Empty;
            string extraFields = string.Empty;

            if (currentrow.Length > 7)
                defaultPurchase = currentrow[7];

            if (currentrow.Length > 9)
                createdLocally = currentrow[9] == "1";

            if (currentrow.Length > 10)
                familyName = currentrow[10] ?? string.Empty;

            if (currentrow.Length > 11)
                extraFields = currentrow[11] ?? string.Empty;

            if (!isActive)
                UnitOfMeasure.InactiveUoM.Add(new UnitOfMeasure() { Conversion = conversion, FamilyId = family, Id = id, IsBase = isBase, IsDefault = isDefault, Name = name, OriginalId = originalId, DefaultPurchase = defaultPurchase, CreatedLocally = createdLocally, FamilyName = familyName, ExtraFields = extraFields });
            else
                UnitOfMeasure.List.Add(new UnitOfMeasure() { Conversion = conversion, FamilyId = family, Id = id, IsBase = isBase, IsDefault = isDefault, Name = name, OriginalId = originalId, DefaultPurchase = defaultPurchase, CreatedLocally = createdLocally, FamilyName = familyName, ExtraFields = extraFields });
        }

        public static void LoadBuildToQty(string[] parts)
        {
            int productID = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
            int clientId = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
            float qtyty = Convert.ToSingle(parts[2], CultureInfo.InvariantCulture);

            BuildToQty btq = new BuildToQty();
            btq.ClientId = clientId;
            btq.ProductId = productID;
            btq.Qty = qtyty;

            BuildToQty.List.Add(btq);
        }

        public static void LoadOrders()
        {
            // clean up the list of orders
            Order.Orders.Clear();
            var files = Directory.GetFiles(Config.CurrentOrdersPath).ToList();
            Logger.CreateLog("Number of orders file in the current order path: " + files.Count.ToString());
            foreach (string file in files)
                Order.AddOrderFromFile(file);
        }

        public static void LoadPayments()
        {
            // clean up the list of orders
            InvoicePayment.ClearMemory();
            foreach (string file in Directory.GetFiles(Config.PaymentPath))
                InvoicePayment.AddPaymentFromFile(file);
        }

        public static void LoadBatches()
        {
            foreach (string file in Directory.GetFiles(Config.BatchPath))
                Batch.LoadFromFile(file);
        }

        static void RemoveOlderFiles()
        {
            long starttick = DateTime.Now.AddDays(Convert.ToDouble(-Config.DaysToKeepOrder)).Ticks;

            foreach (string filename in Directory.GetFiles(Config.OrderPath))
            {
                if (File.GetCreationTime(filename).Ticks < starttick)
                    File.Delete(filename);
            }

            starttick = DateTime.Now.AddDays(Convert.ToDouble(-Config.DaysToKeepSignatures)).Ticks;

            foreach (string filename in Directory.GetFiles(Config.OrderPath))
            {
                if (File.GetCreationTime(filename).Ticks < starttick && filename.Contains("signature"))
                    File.Delete(filename);
            }
        }

        static void RemoveOlderPaymentsFiles()
        {
            long starttick = DateTime.Now.AddDays(Convert.ToDouble(-Config.DaysToKeepOrder)).Ticks;

            foreach (string filename in Directory.GetFiles(Config.SentPaymentPath))
            {
                if (File.GetCreationTime(filename).Ticks < starttick)
                    File.Delete(filename);
            }
        }

        public static void ButlerClearData()
        {
            try
            {
                PendingLoadToAccept = false;
                ReceivedData = false;
                LastEndOfDay = DateTime.MinValue;
                //solves duplicate session id
                Config.ClearSessionId();
                Config.SaveAppStatus();
                Client.Clear();
                Invoice.Clear();
                RouteEx.ClearAll();
                RetailPriceLevel.Clear(10000);
                RetailProductPrice.Clear(10000);
                Batch.ClearList();
                Order.Orders.Clear();
                ClientDailyParLevel.Clear();
                ParLevelHistory.Histories.Clear();
                Shipment.CurrentShipment = null;
                Offer.Clear(Offer.OfferList.Count());
                ConsignmentValues.Clear();
                OrderHistory.History.Clear();
                Reason.Clear();
                SalesmanSession.Sessions.Clear();
                UnitOfMeasure.List.Clear();
                BankAccount.List.Clear();
                InvoicePayment.ClearList();
                TemporalInvoicePayment.List.Clear();
                SiteEx.Clear();
                ProductInventory.ClearAll();
                AccessCode.Clear();
                VehicleInformation.Clear();
                Session.Clear();
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error clearing for butler");
            }
        }

        public static void ClearData()
        {
            try
            {
                PendingLoadToAccept = false;
                ReceivedData = false;
                LastEndOfDay = DateTime.MinValue;
                Config.SaveAppStatus();

                DirectoryInfo di = new DirectoryInfo(Config.CodeBase);

                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories())
                    if (!dir.Name.StartsWith("."))
                        dir.Delete(true);

                CompanyInfo.Clear(CompanyInfo.Companies.Count());
                Product.Clear();
                Category.Categories.Clear();
                Client.Clear();
                Invoice.Clear();
                RouteEx.ClearAll();
                RetailPriceLevel.Clear(10000);
                RetailProductPrice.Clear(10000);
                Batch.ClearList();
                Order.Orders.Clear();
                ClientDailyParLevel.Clear();
                ParLevelHistory.Histories.Clear();
                Shipment.CurrentShipment = null;
                Offer.Clear(Offer.OfferList.Count());
                ConsignmentValues.Clear();
                OrderHistory.History.Clear();
                Reason.Clear();
                SalesmanSession.Sessions.Clear();
                UnitOfMeasure.List.Clear();
                BankAccount.List.Clear();
                InvoicePayment.ClearList();
                TemporalInvoicePayment.List.Clear();
                SiteEx.Clear();
                ProductInventory.ClearAll();
                AccessCode.Clear();
                VehicleInformation.Clear();
                Session.Clear();

            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        static DataSet CreateOrdersDS()
        {
            DataSet Orders = new DataSet("Orders");
            Orders.Locale = CultureInfo.InvariantCulture;
            #region The Order Table
            DataTable table = new DataTable("Order");
            table.Locale = CultureInfo.InvariantCulture;
            DataColumn pkCol = table.Columns.Add("OrderID", typeof(int));
            pkCol.AutoIncrement = true;
            pkCol.AutoIncrementSeed = pkCol.AutoIncrementStep = 1;
            table.Columns.Add("ClientID", typeof(int));
            table.Columns.Add("VendorID", typeof(int));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("OrderType", typeof(int));
            table.Columns.Add("Comments", typeof(string));
            table.Columns.Add("PrintedOrderID", typeof(string));
            table.Columns.Add("Latitude", typeof(double));
            table.Columns.Add("Longitude", typeof(double));
            table.Columns.Add("Signature", typeof(string));
            table.Columns.Add("UniqueId", typeof(string));
            table.Columns.Add("SignatureName", typeof(string));
            table.Columns.Add("TaxRate", typeof(float));
            table.Columns.Add("DiscountType", typeof(int));
            table.Columns.Add("DiscountAmount", typeof(float));
            table.Columns.Add("DiscountComments", typeof(string));
            table.Columns.Add("Voided", typeof(bool));
            table.Columns.Add("PONumber", typeof(string));
            table.Columns.Add("ShipDate", typeof(DateTime));
            table.Columns.Add("EndDate", typeof(DateTime));
            table.Columns.Add("BatchId", typeof(int));
            table.Columns.Add("Dexed", typeof(bool));
            table.Columns.Add("Finished", typeof(bool));
            table.Columns.Add("CompanyName", typeof(string));
            table.Columns.Add("Reshipped", typeof(bool));
            table.Columns.Add("ReshipDate", typeof(long));
            table.Columns.Add("ExtraFields", typeof(string));
            table.Columns.Add("DateLong", typeof(long));
            table.Columns.Add("ShipDateLong", typeof(long));
            table.Columns.Add("EndDateLong", typeof(long));
            table.Columns.Add("SentDateLong", typeof(long));
            table.Columns.Add("ClientUniqueId", typeof(string));
            table.Columns.Add("AsPresale", typeof(bool));
            table.Columns.Add("ReasonId", typeof(int));
            table.Columns.Add("SessionId", typeof(string));
            table.Columns.Add("SiteId", typeof(int));
            table.Columns.Add("CompanyId", typeof(int));
            table.Columns.Add("DriverId", typeof(int));

            table.Columns.Add("OtherCharges", typeof(double));
            table.Columns.Add("Freight", typeof(double));
            table.Columns.Add("OtherChargesType", typeof(int));
            table.Columns.Add("FreightType", typeof(int));
            table.Columns.Add("OtherChargesComment", typeof(string));
            table.Columns.Add("FreightComment", typeof(string));
            table.Columns.Add("AssetId", typeof(int));

            table.PrimaryKey = new DataColumn[] { pkCol };
            Orders.Tables.Add(table);
            #endregion

            #region The OrderDetail Table
            table = new DataTable("OrderDetail");
            table.Locale = CultureInfo.InvariantCulture;
            pkCol = table.Columns.Add("OrderDetailID", typeof(int));
            pkCol.AutoIncrement = true;
            pkCol.AutoIncrementSeed = pkCol.AutoIncrementStep = 1;
            table.Columns.Add("OrderID", typeof(int));
            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("Qty", typeof(float));
            table.Columns.Add("Price", typeof(double));
            table.Columns.Add("ExpectedPrice", typeof(double));
            table.Columns.Add("Comment", typeof(string));
            table.Columns.Add("Lot", typeof(string));
            table.Columns.Add("FromOffer", typeof(bool));
            table.Columns.Add("Damaged", typeof(bool));
            table.Columns.Add("OriginalId", typeof(string));
            table.Columns.Add("IsCredit", typeof(bool));
            table.Columns.Add("UnitOfMeasureId", typeof(int));

            if (!Config.DonNovoCustomization)
                table.Columns.Add("Weight", typeof(float));
            // consignment stuff
            table.Columns.Add("ConsignmentCounted", typeof(double));
            table.Columns.Add("ConsignmentPicked", typeof(double));
            table.Columns.Add("ConsignmentOld", typeof(double));
            table.Columns.Add("ConsignmentNew", typeof(double));
            table.Columns.Add("ConsignmentNewPrice", typeof(double));
            table.Columns.Add("ConsignmentSet", typeof(bool));
            table.Columns.Add("ConsignmentCountedFlag", typeof(bool));
            table.Columns.Add("ConsignmentUpdated", typeof(bool));
            table.Columns.Add("ConsignmentSalesItem", typeof(bool));
            table.Columns.Add("ConsignmentFromTruck", typeof(float));
            table.Columns.Add("Allowance", typeof(double));

            table.Columns.Add("ExtraFields", typeof(string));

            table.Columns.Add("Taxed", typeof(bool));
            table.Columns.Add("TaxRate", typeof(double));
            table.Columns.Add("Discount", typeof(double));
            table.Columns.Add("DiscountType", typeof(int));

            table.Columns.Add("ConsigmentExtraField", typeof(string));
            table.Columns.Add("ReasonId", typeof(int));

            table.Columns.Add("OrderDiscountId", typeof(int));
            table.Columns.Add("OrderDiscountBreakId", typeof(int));
            table.Columns.Add("CostDiscount", typeof(double));
            table.Columns.Add("CostPrice", typeof(double));
            table.Columns.Add("ExtraComments", typeof(string));

            table.PrimaryKey = new DataColumn[] { pkCol };
            Orders.Tables.Add(table);
            #endregion
            DataRelation fkRel = new DataRelation("OrderDetail_Order", Orders.Tables["Order"].Columns["OrderID"], Orders.Tables["OrderDetail"].Columns["OrderID"]);
            Orders.Relations.Add(fkRel);

            #region The Batch Table
            table = new DataTable("Batch");
            table.Locale = CultureInfo.InvariantCulture;
            pkCol = table.Columns.Add("BatchId", typeof(int));
            table.Columns.Add("ClientId", typeof(int));
            table.Columns.Add("SalesmanId", typeof(int));
            table.Columns.Add("ClockedIn", typeof(DateTime));
            table.Columns.Add("ClockedOut", typeof(DateTime));
            table.Columns.Add("PrintedId", typeof(string));
            table.Columns.Add("ClockedInLong", typeof(long));
            table.Columns.Add("ClockedOutLong", typeof(long));
            table.Columns.Add("SessionId", typeof(string));
            table.Columns.Add("UniqueId", typeof(string));
            table.PrimaryKey = new DataColumn[] { pkCol };
            Orders.Tables.Add(table);
            #endregion

            return Orders;
        }

        static DataSet CreateInvoicePaymentDS()
        {
            DataTable table;
            DataColumn pkCol;

            DataSet invoices = new DataSet("InvoicePayment");
            table = new DataTable("InvoicePayment");

            pkCol = table.Columns.Add("id", typeof(Int32));
            pkCol.AutoIncrement = true;
            pkCol.AutoIncrementSeed = pkCol.AutoIncrementStep = 1;
            table.Columns.Add("InvoiceNumber", typeof(System.String));
            table.Columns.Add("OrderUniqueId", typeof(System.String));
            table.Columns.Add("Amount", typeof(System.Double));
            table.Columns.Add("SalesPerson", typeof(System.String));
            table.Columns.Add("Comments", typeof(System.String));
            table.Columns.Add("PaymentMethod", typeof(System.String));
            table.Columns.Add("CheckNumber", typeof(System.String));
            table.Columns.Add("ClientId", typeof(int));
            table.Columns.Add("UniqueId", typeof(System.String));
            table.Columns.Add("DateCreated", typeof(System.DateTime));
            table.Columns.Add("DateCreatedLong", typeof(long));
            table.Columns.Add("SentDateLong", typeof(long));
            table.Columns.Add("SessionId", typeof(string));
            table.Columns.Add("ExtraFields", typeof(string));
            table.Columns.Add("bankaccountid", typeof(int));
            table.Columns.Add("BankName", typeof(string));
            table.Columns.Add("PostingDate", typeof(System.DateTime));
            table.Columns.Add("RefTransactions", typeof(string));

            table.PrimaryKey = new DataColumn[] { pkCol };
            invoices.Tables.Add(table);

            return invoices;
        }

        public class InvoiceDetailClient
        {
            public string InvoiceId;

            public int ClientId;

            public List<OrderDetail> OrderDetails;

            public bool AsPresale;
        }

        static Dictionary<string, double> SerializeOrdersToDataSet(IEnumerable<Batch> source, string targetFile, List<string> ordersId, string sessionId)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            //clear the order dataset
            DataSet Orders = CreateOrdersDS();
            List<int> clientIds = new List<int>();
            List<int> batchIds = new List<int>();
            foreach (Batch batch in source)
            {
                if (batchIds.Contains(batch.Id))
                {
                    Logger.CreateLog("Duplicated batch with the same id=" + batch.Id);
                    continue;
                }

                DataRow batchRow = Orders.Tables["Batch"].NewRow();
                batchRow["BatchId"] = batch.Id;
                if (batch.Client != null)
                    batchRow["ClientId"] = batch.Client.ClientId;
                else
                {
                    Logger.CreateLog("batch.client is null");
                    continue;
                }
                batchRow["SalesmanId"] = Config.SalesmanId;
                batchRow["ClockedIn"] = batch.ClockedIn;
                batchRow["ClockedOut"] = batch.ClockedOut;
                batchRow["PrintedId"] = batch.PrintedId;
                batchRow["ClockedInLong"] = batch.ClockedIn.Ticks;
                batchRow["ClockedOutLong"] = batch.ClockedOut.Ticks;
                batchRow["SessionId"] = sessionId;
                batchRow["UniqueId"] = !string.IsNullOrEmpty(batch.UniqueId) ? batch.UniqueId : Guid.NewGuid().ToString();
                Orders.Tables["Batch"].Rows.Add(batchRow);
                batchIds.Add(batch.Id);

                var c = Order.Orders.Count(x => x == null);
                if (c > 0)
                    Logger.CreateLog("there are " + c.ToString() + " orders in null");

                var orders = Order.Orders.Where(x => x != null && x.BatchId == batch.Id).ToList();
                if (ordersId != null)
                    orders = orders.Where(x => ordersId.Contains(x.OrderId.ToString())).ToList();

                if (Config.SalesByDepartment && batch.Client.OneOrderPerDepartment && orders.Count > 0)
                {
                    Order mergedOrder = MergeOrders(orders);
                    orders = new List<Order>() { mergedOrder };
                }

                if (Config.SelectPriceFromPrevInvoices)
                {
                    //aqui hacer lo de separar las ordene es creditos
                    //TODO: Separar los Credit Invoices de los credit orders etcs
                    var credits_oneDoc = orders.Where(x => x.OrderType == OrderType.Order && x.Details.Any(y => y.IsCredit && (y.ExtraFields != null && y.ExtraFields.Contains("frominvoice"))) && (string.IsNullOrEmpty(x.Comments) ? true : !x.Comments.Contains("REF# "))).ToList();
                    var credits = orders.Where(x => x.OrderType == OrderType.Credit && x.Details.Any(y => y.IsCredit && (y.ExtraFields != null && y.ExtraFields.Contains("frominvoice"))) && (string.IsNullOrEmpty(x.Comments) ? true : !x.Comments.Contains("REF# "))).ToList();

                    credits.AddRange(credits_oneDoc);

                    foreach (var a in credits)
                        orders.Remove(a);

                    var toRemove = new Dictionary<Order, List<OrderDetail>>();
                    var groupedCredits = new List<InvoiceDetailClient>();

                    foreach (var credit in credits)
                    {
                        foreach (var detail in credit.Details)
                        {
                            if (string.IsNullOrEmpty(detail.ExtraFields))
                                continue;

                            var invoiceId = DataAccess.GetSingleUDF("frominvoice", detail.ExtraFields);
                            if (!string.IsNullOrEmpty(invoiceId))
                            {
                                var found = groupedCredits.FirstOrDefault(x => x.InvoiceId == invoiceId && x.ClientId == credit.Client.ClientId && x.AsPresale == credit.AsPresale);
                                if (found != null)
                                    found.OrderDetails.Add(detail);
                                else
                                    groupedCredits.Add(new InvoiceDetailClient() { ClientId = credit.Client.ClientId, InvoiceId = invoiceId, OrderDetails = new List<OrderDetail>() { detail }, AsPresale = credit.AsPresale });


                                if (toRemove.Any(x => x.Key.OrderId == credit.OrderId))
                                    toRemove[credit].Add(detail);
                                else
                                    toRemove.Add(credit, new List<OrderDetail>() { detail });
                            }
                        }
                    }

                    List<Order> ordersToDelete = new List<Order>();
                    List<Order> ordersToAdd = new List<Order>();

                    foreach (var grouped in groupedCredits)
                    {
                        var order = new Order(Client.Find(grouped.ClientId));

                        order.BatchId = batch != null ? batch.Id : 0;
                        order.OrderType = OrderType.Credit;

                        order.PONumber = grouped.InvoiceId;

                        order.Comments = "REF# " + grouped.InvoiceId;

                        foreach (var d in grouped.OrderDetails)
                            order.AddDetail(d);

                        order.AsPresale = grouped.AsPresale;
                        order.Finished = !grouped.AsPresale;

                        order.Save();

                        ordersToAdd.Add(order);
                    }

                    foreach (var dict in toRemove)
                    {
                        var credit = dict.Key;

                        foreach (var det in dict.Value)
                            credit.DeleteDetail(det);

                        if (credit.Details.Count == 0)
                            ordersToDelete.Add(credit);
                    }

                    foreach (var od in ordersToDelete)
                    {
                        Order.Orders.Remove(od);
                        if (credits.Contains(od))
                            credits.Remove(od);

                        od.Delete();
                    }

                    credits.AddRange(ordersToAdd);

                    orders.AddRange(credits);
                }

                foreach (var order in orders)
                {
                    if (order == null)
                        continue;

                    order.CheckOrderLengthsBeforeSending();

                    if (order.Details == null)
                    {
                        Logger.CreateLog("order " + order.UniqueId + " has detail in null");
                        continue;
                    }

                    if (order.Client == null)
                    {
                        Logger.CreateLog("order " + order.UniqueId + " has Client in null");
                        continue;
                    }

                    if (order.AsPresale && (order.ExtraFields ?? "").Contains("fullTemplateOnlyCount"))
                    {
                        Logger.CreateLog("order " + order.UniqueId + " is a presale for counts only");
                        continue;
                    }

                    if (order.OrderType != OrderType.NoService && order.Details.Count == 0 && !order.Voided)
                        continue;

                    if (order.OrderType == OrderType.Consignment)
                    {
                        if (Config.ParInConsignment || Config.ConsignmentBeta)
                        {
                            if (order.AsPresale && Config.ConsignmentPresaleOnly)
                            {
                                order.ConvertConsignmentPar();

                                string zeroLines = "";
                                foreach (var item in order.Details.Where(x => x.ConsignmentUpdated && (x.ConsignmentOld != x.ConsignmentNew || x.Price != x.ConsignmentNewPrice) && !x.ParLevelDetail))
                                {
                                    var rotation = DataAccess.GetSingleUDF("rotatedQty", item.ExtraFields);
                                    var adjQty = DataAccess.GetSingleUDF("adjustedQty", item.ExtraFields);
                                    var core = DataAccess.GetSingleUDF("coreQty", item.ExtraFields);

                                    if (item.Qty == 0 && string.IsNullOrEmpty(rotation) && string.IsNullOrEmpty(adjQty) && string.IsNullOrEmpty(core))
                                    {
                                        string ss = item.Product.ProductId.ToString() + "," + item.ExtraFields.Replace('=', ':').Replace('|', ',');
                                        if (zeroLines.Length > 0)
                                            zeroLines += ";";
                                        zeroLines += ss;
                                    }
                                }

                                if (!string.IsNullOrEmpty(zeroLines))
                                    order.ExtraFields = DataAccess.SyncSingleUDF("zerolines", zeroLines, order.ExtraFields);
                            }
                            else if (order.AsPresale || order.Reshipped)
                                FixConsignmentPar(order);
                            else
                                order.ConvertConsignmentPar();
                        }
                    }

                    if (order.Client.ClientId < 0)
                        clientIds.Add(order.Client.ClientId);

                    DataRow orderRow = Orders.Tables["Order"].NewRow();
                    orderRow["ClientID"] = order.Client.ClientId;
                    orderRow["VendorID"] = order.SalesmanId;
                    orderRow["Date"] = order.Date;
                    orderRow["Comments"] = string.IsNullOrEmpty(order.Comments) ? string.Empty : order.Comments;

                    if (Config.UseReturnInvoice && order.OrderType == OrderType.Credit && order.Details.All(x => x != null && !x.Damaged))
                        order.OrderType = OrderType.Return;

                    if (Config.UseQuote && order.OrderType == OrderType.Order && order.IsQuote)
                        order.OrderType = OrderType.Quote;

                    if (order.IsExchange)
                        orderRow["OrderType"] = (int)OrderType.Order;
                    else
                        orderRow["OrderType"] = (int)order.OrderType;

                    orderRow["PrintedOrderID"] = order.PrintedOrderId;
                    orderRow["AsPresale"] = order.AsPresale;
                    if (Config.UseLocation)
                    {
                        orderRow["Latitude"] = order.Latitude;
                        orderRow["Longitude"] = order.Longitude;
                    }
                    if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                        orderRow["Signature"] = string.Empty; // order.SerializeSignatureAsString();
                    orderRow["UniqueId"] = order.UniqueId;
                    orderRow["SignatureName"] = order.SignatureName;
                    orderRow["TaxRate"] = order.TaxRate;
                    orderRow["DiscountType"] = (int)order.DiscountType;
                    orderRow["DiscountAmount"] = order.DiscountAmount;
                    orderRow["DiscountComments"] = order.DiscountComment;
                    orderRow["PONumber"] = order.PONumber ?? string.Empty;
                    orderRow["ShipDate"] = order.ShipDate.Date;
                    orderRow["EndDate"] = order.EndDate;
                    orderRow["Voided"] = order.Voided;
                    orderRow["BatchId"] = order.BatchId;
                    orderRow["Dexed"] = order.Dexed;
                    orderRow["Finished"] = order.Finished;
                    orderRow["CompanyName"] = order.CompanyName;
                    orderRow["Reshipped"] = order.Reshipped;
                    orderRow["ReshipDate"] = order.ReshipDate.Ticks;

                    // if no presale is defined yet
                    if (order.ExtraFields == null)
                        order.ExtraFields = string.Empty;

                    // if asPresale is defined, do not change it
                    if (order.ExtraFields.ToLowerInvariant().IndexOf("aspresale") < 0)
                    {
                        if (order.AsPresale)
                            order.ExtraFields = DataAccess.SyncSingleUDF("AsPresale", "1", order.ExtraFields);
                        else
                            order.ExtraFields = DataAccess.SyncSingleUDF("AsPresale", "0", order.ExtraFields);
                    }

                    orderRow["DateLong"] = order.Date.Ticks;
                    orderRow["ShipDateLong"] = order.ShipDate.Date.Ticks;
                    orderRow["EndDateLong"] = order.EndDate.Ticks;
                    orderRow["SentDateLong"] = DateTime.Now.Ticks;
                    orderRow["ClientUniqueId"] = order.Client.UniqueId;

                    orderRow["ReasonId"] = order.ReasonId;

                    if (!string.IsNullOrEmpty(sessionId))
                        orderRow["SessionId"] = sessionId;

                    if (Config.UseQuote && order.FromInvoiceId > 0)
                        order.ExtraFields = DataAccess.SyncSingleUDF("ExternalInvoiceId", order.FromInvoiceId.ToString(), order.ExtraFields);

                    if (order.IsScanBasedTrading)
                        order.ExtraFields = SyncSingleUDF("scanBasedTrading", "1", order.ExtraFields);

                    if (!string.IsNullOrEmpty(order.DepartmentUniqueId))
                    {
                        order.ExtraFields = SyncSingleUDF("department", order.DepartmentUniqueId, order.ExtraFields);
                        if (order.Department != null)
                            order.ExtraFields = SyncSingleUDF("departmentName", order.Department.Name, order.ExtraFields);
                    }

                    if (order.DepartmentId > 0)
                        order.ExtraFields = SyncSingleUDF("departmentId", order.DepartmentId.ToString(), order.ExtraFields);

                    if (order.IsParLevel)
                        order.ExtraFields = SyncSingleUDF("orderFromParLevel", "1", order.ExtraFields);

                    //if (order.Finished)
                    //    order.ExtraFields = SyncSingleUDF("finalized", "1", order.ExtraFields);

                    orderRow["ExtraFields"] = order.ExtraFields;

                    if (!order.AsPresale)
                        orderRow["SiteId"] = Config.SiteId;
                    else
                    {
                        if (Config.SelectWarehouseForSales)
                        {
                            var siteid = Config.SalesmanSelectedSite;
                            if (order.SiteId > 0)
                            {
                                siteid = order.SiteId;
                            }

                            if (siteid > 0)
                                orderRow["SiteId"] = siteid;
                            else
                                orderRow["SiteId"] = Config.BranchSiteId;
                        }
                        else
                        if ((Config.SalesmanCanChangeSite && Config.SalesmanSelectedSite > 0) || Config.PresaleUseInventorySite)
                        {
                            var siteId = Config.SalesmanSelectedSite;
                            if (Config.PresaleUseInventorySite)
                            {
                                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                                if (salesman != null)
                                {
                                    siteId = salesman.InventorySiteId;
                                    if (siteId == 0)
                                        siteId = Config.BranchSiteId;
                                }
                            }

                            orderRow["SiteId"] = siteId;

                        }
                        else
                            orderRow["SiteId"] = Config.BranchSiteId;
                    }

                    orderRow["CompanyId"] = order.CompanyId;

                    orderRow["DriverId"] = !order.AsPresale ? Config.SalesmanId : 0;

                    orderRow["OtherCharges"] = order.OtherCharges;
                    orderRow["Freight"] = order.Freight;
                    orderRow["OtherChargesType"] = order.OtherChargesType;
                    orderRow["FreightType"] = order.FreightType;
                    orderRow["OtherChargesComment"] = order.OtherChargesComment;
                    orderRow["FreightComment"] = order.FreightComment;
                    orderRow["AssetId"] = order.AssetId;

                    Orders.Tables["Order"].Rows.Add(orderRow);

                    //butler group order before sending
                    if (Config.ButlerCustomization)
                    {
                        var groupedDetails = new List<OrderDetail>();
                        foreach (var det in order.Details)
                        {
                            if (det.UnitOfMeasure != null && det.UnitOfMeasure.Conversion > 1)
                            {
                                var conversion = det.UnitOfMeasure.Conversion;
                                det.UnitOfMeasure = det.Product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);
                                det.Qty = (conversion * det.Qty);

                                det.Price /= conversion;

                                det.Price = Math.Round(det.Price, Config.Round);

                                det.ExpectedPrice = det.Price;

                                var found = groupedDetails.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Price == det.Price && x.Damaged == det.Damaged && x.IsCredit == det.IsCredit);
                                if (found != null)
                                    found.Qty += det.Qty;
                                else
                                    groupedDetails.Add(det);
                            }
                            else
                            {
                                var found = groupedDetails.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Price == det.Price && x.Damaged == det.Damaged && x.IsCredit == det.IsCredit);
                                if (found != null)
                                    found.Qty += det.Qty;
                                else
                                    groupedDetails.Add(det);
                            }
                        }

                        order.Details.Clear();

                        foreach (var gdetail in groupedDetails)
                        {
                            order.Details.Add(gdetail);
                        }
                    }

                    foreach (OrderDetail orderDetail in order.Details)
                    {
                        if (orderDetail.Product == null)
                        {
                            Logger.CreateLog("order " + order.UniqueId + " has product in null");
                            continue;
                        }

                        if (Config.UseFullTemplateForClient(order.Client) && (orderDetail.ExtraFields ?? "").Contains("countedQty"))
                            continue;

                        DataRow orderDetailRow = Orders.Tables["OrderDetail"].NewRow();
                        orderDetailRow["OrderId"] = orderRow["OrderId"];
                        orderDetailRow["ProductID"] = orderDetail.Product.ProductId;

                        if (!Config.HidePriceInTransaction)
                        {
                            orderDetailRow["Price"] = orderDetail.Price;
                            orderDetailRow["ExpectedPrice"] = orderDetail.ExpectedPrice;
                            orderDetailRow["FromOffer"] = orderDetail.FromOffer || orderDetail.FromOfferPrice;
                        }
                        else
                        {
                            var price = Product.GetPriceForProduct(orderDetail.Product, order, orderDetail.IsCredit, orderDetail.Damaged, false);
                            if (order.IsDelivery)
                                price = orderDetail.ExpectedPrice;

                            orderDetailRow["Price"] = price;
                            orderDetailRow["ExpectedPrice"] = price;

                            double offerPrice = 0;
                            orderDetailRow["FromOffer"] = Offer.ProductHasSpecialPriceForClient(orderDetail.Product, order.Client, out offerPrice);
                        }


                        if (!Config.DonNovoCustomization)
                            orderDetailRow["Weight"] = orderDetail.Weight;
                        orderDetailRow["Qty"] = orderDetail.Qty;
                        orderDetailRow["Comment"] = orderDetail.Comments;
                        orderDetailRow["Lot"] = orderDetail.Lot;
                        orderDetailRow["Damaged"] = orderDetail.Damaged;
                        orderDetailRow["OriginalId"] = orderDetail.OriginalId;
                        orderDetailRow["IsCredit"] = orderDetail.IsCredit;

                        orderDetailRow["ConsignmentCounted"] = orderDetail.ConsignmentCount;
                        orderDetailRow["ConsignmentPicked"] = orderDetail.ConsignmentPick;
                        orderDetailRow["ConsignmentOld"] = orderDetail.ConsignmentOld;
                        orderDetailRow["ConsignmentNew"] = orderDetail.ConsignmentNew;
                        orderDetailRow["ConsignmentNewPrice"] = orderDetail.ConsignmentNewPrice;
                        orderDetailRow["ConsignmentSet"] = orderDetail.ConsignmentSet;
                        orderDetailRow["ConsignmentCountedFlag"] = orderDetail.ConsignmentCounted;
                        orderDetailRow["ConsignmentUpdated"] = orderDetail.ConsignmentUpdated;
                        orderDetailRow["ConsignmentSalesItem"] = orderDetail.ConsignmentSalesItem;

                        if (orderDetail.RelatedOrderDetail > 0)
                        {
                            var relatedDetail = order.Details.FirstOrDefault(x => x.OrderDetailId == orderDetail.RelatedOrderDetail);
                            if (relatedDetail != null)
                            {
                                orderDetail.ExtraFields = SyncSingleUDF("RelatedDetail", relatedDetail.OriginalId, orderDetail.ExtraFields);
                            }
                        }

                        if (orderDetail.OfferDetFreeItem > 0)
                        {
                            var relatedDetail = order.Details.FirstOrDefault(x => x.OrderDetailId == orderDetail.OfferDetFreeItem);
                            if (relatedDetail != null)
                            {
                                orderDetail.ExtraFields = SyncSingleUDF("RelatedOfferDetail", relatedDetail.OriginalId, orderDetail.ExtraFields);
                            }
                        }

                        if (orderDetail.FromOfferPrice)
                            orderDetail.ExtraFields = SyncSingleUDF("fromOfferPrice", "1", orderDetail.ExtraFields);

                        if (orderDetail.FromOffer)
                            orderDetail.ExtraFields = SyncSingleUDF("OFFERTYPE", ((int)orderDetail.FromOfferType).ToString(), orderDetail.ExtraFields);

                        float fromTruck = 0;
                        if (order.OrderType == OrderType.Consignment)
                        {
                            if (Config.UseFullConsignment)
                                fromTruck = orderDetail.ConsignmentPicked;
                            else
                            {
                                var base_ = orderDetail.ConsignmentOld;
                                if (orderDetail.ConsignmentCounted)
                                    base_ -= orderDetail.ConsignmentCount;

                                if (orderDetail.ConsignmentUpdated)
                                    base_ += orderDetail.ConsignmentNew - orderDetail.ConsignmentOld;

                                fromTruck = base_;
                            }

                            orderDetail.ExtraFields = SyncSingleUDF("addsales", orderDetail.ConsignmentSalesItem ? "1" : "0", orderDetail.ExtraFields);
                        }
                        orderDetailRow["ConsignmentFromTruck"] = fromTruck;

                        if (Config.HiddenItemCustomization)
                        {
                            if (orderDetail.HiddenItem)
                                orderDetail.ExtraFields = SyncSingleUDF("hiddenItem", "1", orderDetail.ExtraFields);
                            else if (orderDetail.AdjustmentItem)
                                orderDetail.ExtraFields = SyncSingleUDF("adjustmentItem", "1", orderDetail.ExtraFields);
                        }

                        orderDetailRow["ExtraFields"] = orderDetail.ExtraFields;

                        if (orderDetail.UnitOfMeasure == null)
                            orderDetailRow["UnitOfMeasureId"] = 0;
                        else
                            orderDetailRow["UnitOfMeasureId"] = orderDetail.UnitOfMeasure.Id;

                        orderDetailRow["Allowance"] = orderDetail.Allowance.ToString(CultureInfo.InvariantCulture);

                        orderDetailRow["Taxed"] = orderDetail.Taxed;
                        orderDetailRow["TaxRate"] = orderDetail.TaxRate;
                        orderDetailRow["Discount"] = orderDetail.Discount;
                        orderDetailRow["DiscountType"] = (int)orderDetail.DiscountType;

                        if (Config.ButlerCustomization)
                        {
                            //orderDetailRow["UnitOfMeasureId"] = 0;
                            double factor = 1;
                            if (orderDetail.UnitOfMeasure != null)
                                factor *= orderDetail.UnitOfMeasure.Conversion;

                            orderDetailRow["Qty"] = (orderDetail.Qty * factor);

                            var price = orderDetail.Price;

                            if (orderDetail.UnitOfMeasure != null)
                                price /= orderDetail.UnitOfMeasure.Conversion;

                            orderDetailRow["Price"] = price;
                            orderDetailRow["ExpectedPrice"] = price;
                        }

                        string consigmentExtraField = string.Empty;
                        var dic = orderDetail.ConsignmentCountedLots;
                        if (dic != null)
                            consigmentExtraField = SyncSingleUDF("countedLots", OrderDetail.GetConsLotsAsString(dic).Replace('|', (char)20), "");
                        dic = orderDetail.ConsignmentPickedLots;
                        if (dic != null)
                            consigmentExtraField = SyncSingleUDF("pickedLots", OrderDetail.GetConsLotsAsString(dic).Replace('|', (char)20), consigmentExtraField);
                        consigmentExtraField = SyncSingleUDF("comment", !string.IsNullOrEmpty(orderDetail.ConsignmentComment) ? orderDetail.ConsignmentComment : "", consigmentExtraField);
                        orderDetailRow["ConsigmentExtraField"] = consigmentExtraField;

                        orderDetailRow["ReasonId"] = orderDetail.ReasonId;

                        orderDetailRow["OrderDiscountId"] = orderDetail.OrderDiscountId;
                        orderDetailRow["OrderDiscountBreakId"] = orderDetail.OrderDiscountBreakId;
                        orderDetailRow["CostDiscount"] = orderDetail.CostDiscount;
                        orderDetailRow["CostPrice"] = orderDetail.CostPrice;
                        orderDetailRow["ExtraComments"] = orderDetail.ExtraComments ?? "";

                        Orders.Tables["OrderDetail"].Rows.Add(orderDetailRow);
                    }

                    result.Add(order.UniqueId, order.OrderTotalCost());
                }
            }

            var table = AddNewClients(clientIds);
            if (table != null)
                Orders.Tables.Add(table);

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    using (StreamWriter stream = new StreamWriter(targetFile))
                    {
                        using (XmlTextWriter reader = new XmlTextWriter(stream))
                        {
                            Orders.WriteXml(reader, XmlWriteMode.WriteSchema);
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }

                return result;
            }
        }

        private static Order MergeOrders(List<Order> orders)
        {
            var firstOrder = orders.FirstOrDefault();
            if (firstOrder == null)
                return null;

            var result = Order.DuplicateorderHeader(firstOrder);
            foreach (var order in orders)
            {
                foreach (var item in order.Details)
                {
                    item.ExtraFields = SyncSingleUDF("department", order.DepartmentUniqueId, item.ExtraFields);
                    result.Details.Add(item);
                }
            }

            return result;
        }

        static Dictionary<string, double> SerializeInvoicePaymentsToDataSet(DataSet ds, string sessionId, List<InvoicePayment> source = null, Dictionary<string, double> ordersTotals = null, bool inbackground = false)
        {
            bool useNewSendPayment = CheckCommunicatorVersion(CommunicatorVersion, "45.0.0.0");
            var useBankName = CheckCommunicatorVersion(CommunicatorVersion, "46.0.0");

            var originalValues = new Dictionary<string, double>();

            if (useNewSendPayment)
            {
                List<int> clientIds = new List<int>();

                if (source == null)
                    source = InvoicePayment.List.ToList();

                List<string> paymentUniqueIds = new List<string>();

                var deposit = BankDeposit.currentDeposit;
                bool containedInDeposit = false;

                foreach (InvoicePayment payment in source)
                {
                    containedInDeposit = false;

                    if (deposit != null && deposit.Payments.Any(x => x.UniqueId == payment.UniqueId))
                        containedInDeposit = true;

                    if (payment.Client.ClientId < 0)
                        clientIds.Add(payment.Client.ClientId);

                    int offset = 0;

                    double extraFromOverPayments = 0;

                    //is credit in account
                    if (string.IsNullOrEmpty(payment.OrderId) && string.IsNullOrEmpty(payment.InvoicesId))
                    {
                        foreach (var part in payment.Components)
                        {
                            string uid = payment.UniqueId + "_" + offset.ToString();
                            offset++;
                            DataRow row = ds.Tables[0].NewRow();
                            row["Amount"] = part.Amount;
                            row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                            row["Comments"] = part.Comments;
                            row["PaymentMethod"] = part.PaymentMethod.ToString();
                            // TODO: What to do in case of a check???
                            row["CheckNumber"] = part.Ref;
                            row["ClientId"] = payment.Client.ClientId;
                            row["UniqueId"] = uid;
                            row["DateCreated"] = payment.DateCreated;
                            row["DateCreatedLong"] = payment.DateCreated.Ticks;
                            row["SentDateLong"] = DateTime.Now.Ticks;

                            if (!string.IsNullOrEmpty(sessionId))
                                row["SessionId"] = sessionId;

                            if (containedInDeposit && deposit != null)
                                part.ExtraFields = SyncSingleUDF("depositId", deposit.UniqueId, part.ExtraFields);

                            if (!string.IsNullOrEmpty(part.ExtraFields))
                                row["ExtraFields"] = part.ExtraFields;

                            if (!useBankName)
                            {
                                var bank = BankAccount.List.FirstOrDefault(x => x.Name == part.BankName);
                                if (bank != null)
                                    row["bankaccountid"] = bank.Id;
                            }
                            else
                                row["BankName"] = part.BankName;

                            if (part.PostedDate != null && part.PostedDate != DateTime.MinValue && part.PostedDate != DateTime.MaxValue)
                                row["PostingDate"] = part.PostedDate;

                            row["RefTransactions"] = payment.InvoicesId;

                            ds.Tables[0].Rows.Add(row);
                        }

                        continue;
                    }

                    if (!string.IsNullOrEmpty(payment.InvoicesId))
                    {
                        var components = new List<PaymentComponent>();

                        foreach (var comp in payment.Components)
                            components.Add(new PaymentComponent(comp));

                        foreach (var component in components)
                        {
                            //organize the invoices :(
                            var negatives = payment.Invoices().Where(x => x.Balance < 0).ToList();
                            var positives = payment.Invoices().Where(x => x.Balance > 0).OrderBy(x => x.DueDate).ToList();

                            var paymentInvoices = new List<Invoice>();
                            paymentInvoices.AddRange(negatives);
                            paymentInvoices.AddRange(positives);

                            foreach (var invoice in paymentInvoices)
                            {
                                if (invoice.Balance == 0)
                                    continue;
                                string uid = payment.UniqueId + "_" + offset.ToString();
                                offset++;
                                double usedInThisInvoice = component.Amount;

                                if (invoice.Balance < 0)
                                {
                                    usedInThisInvoice = invoice.Balance;
                                    component.Amount += Math.Abs(invoice.Balance);
                                }
                                else
                                {
                                    if (component.Amount > invoice.Balance)
                                    {
                                        //if (Config.CanPayMoreThanOwned)
                                        //{
                                        //    usedInThisInvoice = component.Amount;
                                        //    component.Amount = 0;
                                        //}
                                        //else
                                        //{
                                        usedInThisInvoice = invoice.Balance;
                                        component.Amount = Math.Round(component.Amount - usedInThisInvoice, 4);
                                        //}
                                    }
                                    else
                                        component.Amount = 0;
                                }
                                //invoice.Balance = Math.Round(invoice.Balance - usedInThisInvoice, 4);

                                DataRow row = ds.Tables[0].NewRow();
                                row["InvoiceNumber"] = invoice.InvoiceNumber;
                                row["Amount"] = usedInThisInvoice;
                                row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                                row["Comments"] = component.Comments;
                                row["PaymentMethod"] = component.PaymentMethod.ToString();
                                // TODO: What to do in case of a check???
                                row["CheckNumber"] = component.Ref;
                                row["ClientId"] = payment.Client.ClientId;
                                row["UniqueId"] = uid;
                                row["DateCreated"] = payment.DateCreated;
                                row["DateCreatedLong"] = payment.DateCreated.Ticks;
                                row["SentDateLong"] = DateTime.Now.Ticks;

                                if (!string.IsNullOrEmpty(sessionId))
                                    row["SessionId"] = sessionId;

                                if (containedInDeposit && deposit != null)
                                    component.ExtraFields = SyncSingleUDF("depositId", deposit.UniqueId, component.ExtraFields);

                                if (!string.IsNullOrEmpty(component.ExtraFields))
                                    row["ExtraFields"] = component.ExtraFields;

                                if (!useBankName)
                                {
                                    var bank = BankAccount.List.FirstOrDefault(x => x.Name == component.BankName);
                                    if (bank != null)
                                        row["bankaccountid"] = bank.Id;
                                }
                                else
                                    row["BankName"] = component.BankName;

                                if (component.PostedDate != null && component.PostedDate != DateTime.MinValue && component.PostedDate != DateTime.MaxValue)
                                    row["PostingDate"] = component.PostedDate;

                                row["RefTransactions"] = payment.InvoicesId;

                                ds.Tables[0].Rows.Add(row);

                                double remainingBalance = 0;
                                if (invoice.Balance < 0)
                                    remainingBalance = 0;
                                else
                                    remainingBalance = invoice.Balance - usedInThisInvoice;

                                if (!originalValues.ContainsKey(invoice.InvoiceNumber))
                                    originalValues.Add(invoice.InvoiceNumber, invoice.Balance);

                                if (!inbackground)
                                {
                                    invoice.Balance = remainingBalance;

                                    var found = TemporalInvoicePayment.List.FirstOrDefault(x => x.invoiceId == invoice.InvoiceId);
                                    if (found != null)
                                        found.amountPaid = remainingBalance;
                                    else
                                        TemporalInvoicePayment.List.Add(new TemporalInvoicePayment() { invoiceId = invoice.InvoiceId, amountPaid = remainingBalance });

                                    TemporalInvoicePayment.Save();

                                    if (invoice.Balance < 0)
                                        invoice.Client.OpenBalance += usedInThisInvoice;
                                    else
                                        invoice.Client.OpenBalance -= usedInThisInvoice;
                                }

                                if (component.Amount == 0)
                                    break;
                            }
                            if (component.Amount > 0)
                            {
                                Logger.CreateLog("component still has amount, but ALL the invoices are completed, what is happenning?");

                                extraFromOverPayments += component.Amount;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(payment.OrderId))
                    {
                        var parts = SplitPayment(payment, ordersTotals);

                        double totalPaidInorders = 0;
                        if (string.IsNullOrEmpty(payment.InvoicesId))
                        {
                            totalPaidInorders = payment.TotalPaid;
                            extraFromOverPayments += totalPaidInorders;
                        }

                        foreach (var part in parts)
                        {
                            string uid = payment.UniqueId + "_" + offset.ToString();
                            offset++;
                            DataRow row = ds.Tables[0].NewRow();
                            row["Amount"] = part.Amount;
                            row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                            row["Comments"] = part.Comments;
                            row["PaymentMethod"] = part.PaymentMethod.ToString();
                            // TODO: What to do in case of a check???
                            row["CheckNumber"] = part.Ref;
                            row["ClientId"] = payment.Client.ClientId;
                            row["UniqueId"] = uid;
                            row["DateCreated"] = payment.DateCreated;
                            row["OrderUniqueId"] = part.UniqueId;
                            row["DateCreatedLong"] = payment.DateCreated.Ticks;
                            row["SentDateLong"] = DateTime.Now.Ticks;

                            if (!string.IsNullOrEmpty(sessionId))
                                row["SessionId"] = sessionId;

                            if (containedInDeposit && deposit != null)
                                part.ExtraFields = SyncSingleUDF("depositId", deposit.UniqueId, part.ExtraFields);

                            if (!string.IsNullOrEmpty(part.ExtraFields))
                                row["ExtraFields"] = part.ExtraFields;

                            row["RefTransactions"] = payment.InvoicesId;

                            ds.Tables[0].Rows.Add(row);

                            extraFromOverPayments -= part.Amount;
                        }
                    }

                    if ((Config.UseCreditAccount || Config.CanPayMoreThanOwned) && extraFromOverPayments > 0)
                    {
                        string uid = payment.UniqueId + "_" + offset.ToString();
                        offset++;
                        DataRow row = ds.Tables[0].NewRow();
                        row["Amount"] = extraFromOverPayments;
                        row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                        row["Comments"] = string.Empty;
                        row["PaymentMethod"] = "Cash";
                        // TODO: What to do in case of a check???
                        row["CheckNumber"] = string.Empty;
                        row["ClientId"] = payment.Client.ClientId;
                        row["UniqueId"] = uid;
                        row["DateCreated"] = payment.DateCreated;
                        row["DateCreatedLong"] = payment.DateCreated.Ticks;
                        row["SentDateLong"] = DateTime.Now.Ticks;

                        if (!string.IsNullOrEmpty(sessionId))
                            row["SessionId"] = sessionId;

                        string ex = string.Empty;
                        if (containedInDeposit && deposit != null)
                            ex = SyncSingleUDF("depositId", deposit.UniqueId, ex);

                        if (!string.IsNullOrEmpty(ex))
                            row["ExtraFields"] = ex;

                        ds.Tables[0].Rows.Add(row);
                    }
                }

                var tabl1e = AddNewClients(clientIds);
                if (tabl1e != null)
                    ds.Tables.Add(tabl1e);

                Client.Save();
            }
            else
            {
                List<int> clientIds = new List<int>();

                if (source == null)
                    source = InvoicePayment.List.ToList();

                foreach (InvoicePayment payment in source)
                {
                    if (payment.Client.ClientId < 0)
                        clientIds.Add(payment.Client.ClientId);

                    int offset = 0;

                    double extraFromOverPayments = 0;


                    //is credit in account
                    if (string.IsNullOrEmpty(payment.OrderId) && string.IsNullOrEmpty(payment.InvoicesId))
                    {
                        foreach (var part in payment.Components)
                        {
                            string uid = payment.UniqueId + "_" + offset.ToString();
                            offset++;
                            DataRow row = ds.Tables[0].NewRow();
                            row["Amount"] = part.Amount;
                            row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                            row["Comments"] = part.Comments;
                            row["PaymentMethod"] = part.PaymentMethod.ToString();
                            // TODO: What to do in case of a check???
                            row["CheckNumber"] = part.Ref;
                            row["ClientId"] = payment.Client.ClientId;
                            row["UniqueId"] = uid;
                            row["DateCreated"] = payment.DateCreated;
                            row["DateCreatedLong"] = payment.DateCreated.Ticks;
                            row["SentDateLong"] = DateTime.Now.Ticks;

                            if (!string.IsNullOrEmpty(sessionId))
                                row["SessionId"] = sessionId;

                            if (!string.IsNullOrEmpty(part.ExtraFields))
                                row["ExtraFields"] = part.ExtraFields;

                            ds.Tables[0].Rows.Add(row);
                        }

                        continue;
                    }

                    if (!string.IsNullOrEmpty(payment.InvoicesId))
                    {
                        var components = new List<PaymentComponent>();

                        foreach (var comp in payment.Components)
                            components.Add(new PaymentComponent(comp));

                        foreach (var component in components)
                        {
                            var invoices = payment.Invoices().OrderBy(x => x.Amount).ToList();

                            foreach (var invoice in invoices)
                            {
                                if (invoice.Balance == 0)
                                    continue;
                                string uid = payment.UniqueId + "_" + offset.ToString();
                                offset++;
                                double usedInThisInvoice = component.Amount;
                                if (component.Amount > invoice.Balance)
                                {
                                    //if (Config.CanPayMoreThanOwned)
                                    //{
                                    //    usedInThisInvoice = component.Amount;
                                    //    component.Amount = 0;
                                    //}
                                    //else
                                    //{
                                    usedInThisInvoice = invoice.Balance;
                                    component.Amount = Math.Round(component.Amount - usedInThisInvoice, 4);
                                    //}
                                }
                                else
                                    component.Amount = 0;

                                //invoice.Balance = Math.Round(invoice.Balance - usedInThisInvoice, 4);

                                DataRow row = ds.Tables[0].NewRow();
                                row["InvoiceNumber"] = invoice.InvoiceNumber;
                                row["Amount"] = usedInThisInvoice;
                                row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                                row["Comments"] = component.Comments;
                                row["PaymentMethod"] = component.PaymentMethod.ToString();
                                // TODO: What to do in case of a check???
                                row["CheckNumber"] = component.Ref;
                                row["ClientId"] = payment.Client.ClientId;
                                row["UniqueId"] = uid;
                                row["DateCreated"] = payment.DateCreated;
                                row["DateCreatedLong"] = payment.DateCreated.Ticks;
                                row["SentDateLong"] = DateTime.Now.Ticks;

                                if (!string.IsNullOrEmpty(sessionId))
                                    row["SessionId"] = sessionId;

                                if (!string.IsNullOrEmpty(component.ExtraFields))
                                    row["ExtraFields"] = component.ExtraFields;

                                ds.Tables[0].Rows.Add(row);

                                double remainingBalance = 0;
                                if (invoice.Balance < 0)
                                    remainingBalance = 0;
                                else
                                    remainingBalance = invoice.Balance - usedInThisInvoice;

                                if (!originalValues.ContainsKey(invoice.InvoiceNumber))
                                    originalValues.Add(invoice.InvoiceNumber, invoice.Balance);

                                if (!inbackground)
                                {
                                    invoice.Balance = remainingBalance;

                                    var found = TemporalInvoicePayment.List.FirstOrDefault(x => x.invoiceId == invoice.InvoiceId);
                                    if (found != null)
                                        found.amountPaid = remainingBalance;
                                    else
                                        TemporalInvoicePayment.List.Add(new TemporalInvoicePayment() { invoiceId = invoice.InvoiceId, amountPaid = remainingBalance });

                                    TemporalInvoicePayment.Save();

                                    if (invoice.Balance < 0)
                                        invoice.Client.OpenBalance += usedInThisInvoice;
                                    else
                                        invoice.Client.OpenBalance -= usedInThisInvoice;
                                }

                                if (component.Amount == 0)
                                    break;
                            }
                            if (component.Amount > 0)
                            {
                                extraFromOverPayments += component.Amount;

                                Logger.CreateLog("component still has amount, but ALL the invoices are completed, what is happenning?");
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(payment.OrderId))
                    {
                        double totalPaidInorders = 0;
                        if (string.IsNullOrEmpty(payment.InvoicesId))
                        {
                            totalPaidInorders = payment.TotalPaid;
                            extraFromOverPayments += totalPaidInorders;
                        }

                        var parts = SplitPayment(payment, ordersTotals);
                        foreach (var part in parts)
                        {
                            string uid = payment.UniqueId + "_" + offset.ToString();
                            offset++;
                            DataRow row = ds.Tables[0].NewRow();
                            row["Amount"] = part.Amount;
                            row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                            row["Comments"] = part.Comments;
                            row["PaymentMethod"] = part.PaymentMethod.ToString();
                            // TODO: What to do in case of a check???
                            row["CheckNumber"] = part.Ref;
                            row["ClientId"] = payment.Client.ClientId;
                            row["UniqueId"] = uid;
                            row["DateCreated"] = payment.DateCreated;
                            row["OrderUniqueId"] = part.UniqueId;
                            row["DateCreatedLong"] = payment.DateCreated.Ticks;
                            row["SentDateLong"] = DateTime.Now.Ticks;

                            if (!string.IsNullOrEmpty(sessionId))
                                row["SessionId"] = sessionId;

                            if (!string.IsNullOrEmpty(part.ExtraFields))
                                row["ExtraFields"] = part.ExtraFields;

                            ds.Tables[0].Rows.Add(row);

                            extraFromOverPayments -= part.Amount;
                        }
                    }

                    if ((Config.UseCreditAccount || Config.CanPayMoreThanOwned) && extraFromOverPayments > 0)
                    {
                        string uid = payment.UniqueId + "_" + offset.ToString();
                        offset++;
                        DataRow row = ds.Tables[0].NewRow();
                        row["Amount"] = extraFromOverPayments;
                        row["SalesPerson"] = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
                        row["Comments"] = string.Empty;
                        row["PaymentMethod"] = "Cash";
                        // TODO: What to do in case of a check???
                        row["CheckNumber"] = string.Empty;
                        row["ClientId"] = payment.Client.ClientId;
                        row["UniqueId"] = uid;
                        row["DateCreated"] = payment.DateCreated;
                        row["DateCreatedLong"] = payment.DateCreated.Ticks;
                        row["SentDateLong"] = DateTime.Now.Ticks;

                        if (!string.IsNullOrEmpty(sessionId))
                            row["SessionId"] = sessionId;

                        row["ExtraFields"] = string.Empty;

                        ds.Tables[0].Rows.Add(row);
                    }
                }

                var tabl1e = AddNewClients(clientIds);
                if (tabl1e != null)
                    ds.Tables.Add(tabl1e);

            }

            return originalValues;
        }

        public static void SendInvoicePayments(Dictionary<string, double> ordersTotals)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                if (InvoicePayment.List.Count == 0)
                    return;

                foreach (var p in InvoicePayment.List.ToList())
                {
                    foreach (var comp in p.Components)
                        comp.ExtraFields = DataAccess.RemoveSingleUDF("MarkPaymentAsUnDeposit", comp.ExtraFields);
                }

                DataSet ds = CreateInvoicePaymentDS();

                var originalValues = SerializeInvoicePaymentsToDataSet(ds, Config.SessionId, InvoicePayment.List.ToList(), ordersTotals);

                string dstFile = string.Empty;
                string dstFileZipped = string.Empty;
                try
                {
                    try
                    {
                        //FileOperationsLocker.InUse = true;

                        dstFile = Path.Combine(Config.SentPaymentPath, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        dstFileZipped = dstFile + ".zip";
                        using (StreamWriter stream = new StreamWriter(dstFile))
                        using (XmlTextWriter reader = new XmlTextWriter(stream))
                            ds.WriteXml(reader, XmlWriteMode.WriteSchema);
                        //Zip it
                        ZipFile(dstFile, dstFileZipped);
                        NetAccess netaccess = new NetAccess();

                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("InvoicesAR");
                        netaccess.SendFile(dstFileZipped);

                        //string s = netaccess.ReadStringFromNetwork(); //this is the confirmation

                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();
                        //finally remove the payments

                        if (CheckCommunicatorVersion(CommunicatorVersion, "45.0.0"))
                        {
                            //images
                            try
                            {
                                var paymentZip = SerializePaymentImages();
                                if (!string.IsNullOrEmpty(paymentZip) && File.Exists(paymentZip))
                                {
                                    NetAccess.SendPaymentImages(paymentZip);
                                    File.Delete(paymentZip);
                                }

                                if (Directory.Exists(Config.PaymentImagesPath))
                                    Directory.Delete(Config.PaymentImagesPath, true);
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        InvoicePayment.ClearList();
                        RemoveOlderPaymentsFiles();
                    }
                    finally
                    {
                        //FileOperationsLocker.InUse = false;
                    }
                }
                catch (Exception ex)
                {
                    foreach (var o in originalValues)
                    {
                        var inv = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceNumber == o.Key);
                        if (inv != null)
                        {
                            inv.Balance = o.Value;

                            if (inv.Balance < 0)
                                inv.Client.OpenBalance -= inv.Balance;
                            else
                                inv.Client.OpenBalance += inv.Balance;
                        }
                    }

                    Logger.CreateLog(ex);
                    throw;
                }
                finally
                {
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                }
            }
        }

        public static void SendInvoicePaymentsBySource(List<InvoicePayment> source, bool delete = false, bool inBackground = false)
        {
            if (inBackground)
            {
                if (source.Count == 0)
                    return;

                if (inBackground)
                {
                    foreach (var p in source)
                    {
                        foreach (var comp in p.Components)
                            comp.ExtraFields = DataAccess.SyncSingleUDF("MarkPaymentAsUnDeposit", "1", comp.ExtraFields);
                    }
                }
                else
                {
                    foreach (var p in source)
                    {
                        foreach (var comp in p.Components)
                            comp.ExtraFields = DataAccess.RemoveSingleUDF("MarkPaymentAsUnDeposit", comp.ExtraFields);
                    }
                }

                DataSet ds = CreateInvoicePaymentDS();

                SerializeInvoicePaymentsToDataSet(ds, Config.SessionId, source, null, inBackground);

                string dstFile = string.Empty;
                string dstFileZipped = string.Empty;

                try
                {
                    try
                    {
                        //FileOperationsLocker.InUse = true;

                        dstFile = Path.Combine(Config.SentPaymentPath, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        dstFileZipped = dstFile + ".zip";
                        using (StreamWriter stream = new StreamWriter(dstFile))
                        using (XmlTextWriter reader = new XmlTextWriter(stream))
                            ds.WriteXml(reader, XmlWriteMode.WriteSchema);
                        //Zip it
                        ZipFile(dstFile, dstFileZipped);
                        NetAccess netaccess = new NetAccess();

                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("InvoicesAR");
                        netaccess.SendFile(dstFileZipped);

                        //string s = netaccess.ReadStringFromNetwork(); //this is the confirmation

                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();

                        if (delete)
                        {
                            foreach (var item in source)
                                item.Delete();
                        }
                    }
                    finally
                    {
                        //FileOperationsLocker.InUse = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    throw;
                }
                finally
                {
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                }


                if (CheckCommunicatorVersion(CommunicatorVersion, "45.0.0"))
                {
                    //images
                    try
                    {
                        var paymentZip = SerializePaymentImages();
                        if (!string.IsNullOrEmpty(paymentZip) && File.Exists(paymentZip))
                        {
                            NetAccess.SendPaymentImages(paymentZip);
                            File.Delete(paymentZip);
                        }

                        if (Directory.Exists(Config.PaymentImagesPath))
                            Directory.Delete(Config.PaymentImagesPath, true);
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            else
            {


                lock (FileOperationsLocker.lockFilesObject)
                {
                    if (source.Count == 0)
                        return;

                    if (inBackground)
                    {
                        foreach (var p in source)
                        {
                            foreach (var comp in p.Components)
                                comp.ExtraFields = DataAccess.SyncSingleUDF("MarkPaymentAsUnDeposit", "1", comp.ExtraFields);
                        }
                    }
                    else
                    {
                        foreach (var p in source)
                        {
                            foreach (var comp in p.Components)
                                comp.ExtraFields = DataAccess.RemoveSingleUDF("MarkPaymentAsUnDeposit", comp.ExtraFields);
                        }
                    }

                    DataSet ds = CreateInvoicePaymentDS();

                    SerializeInvoicePaymentsToDataSet(ds, Config.SessionId, source, null, inBackground);

                    string dstFile = string.Empty;
                    string dstFileZipped = string.Empty;

                    try
                    {
                        try
                        {
                            //FileOperationsLocker.InUse = true;

                            dstFile = Path.Combine(Config.SentPaymentPath, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                            dstFileZipped = dstFile + ".zip";
                            using (StreamWriter stream = new StreamWriter(dstFile))
                            using (XmlTextWriter reader = new XmlTextWriter(stream))
                                ds.WriteXml(reader, XmlWriteMode.WriteSchema);
                            //Zip it
                            ZipFile(dstFile, dstFileZipped);
                            NetAccess netaccess = new NetAccess();

                            netaccess.OpenConnection();
                            netaccess.WriteStringToNetwork("HELO");
                            netaccess.WriteStringToNetwork(Config.GetAuthString());
                            netaccess.WriteStringToNetwork("InvoicesAR");
                            netaccess.SendFile(dstFileZipped);

                            //string s = netaccess.ReadStringFromNetwork(); //this is the confirmation

                            netaccess.WriteStringToNetwork("Goodbye");
                            Thread.Sleep(1000);
                            netaccess.CloseConnection();

                            if (delete)
                            {
                                foreach (var item in source)
                                    item.Delete();
                            }
                        }
                        finally
                        {
                            //FileOperationsLocker.InUse = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        throw;
                    }
                    finally
                    {
                        if (File.Exists(dstFile))
                            File.Delete(dstFile);
                    }


                    if (CheckCommunicatorVersion(CommunicatorVersion, "45.0.0"))
                    {
                        //images
                        try
                        {
                            var paymentZip = SerializePaymentImages();
                            if (!string.IsNullOrEmpty(paymentZip) && File.Exists(paymentZip))
                            {
                                NetAccess.SendPaymentImages(paymentZip);
                                File.Delete(paymentZip);
                            }

                            if (Directory.Exists(Config.PaymentImagesPath))
                                Directory.Delete(Config.PaymentImagesPath, true);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        public static void SendAll()
        {
            DateTime globalStart = DateTime.Now;
            DateTime partialStart = DateTime.Now;

            var orders = Order.Orders.Where(x => !(x.IsQuote && x.QuoteModified)).ToList();

            Dictionary<string, double> ordersTotals = orders.ToDictionary(x => x.UniqueId, x => x.OrderTotalCost());
            var extendedMap = ExtendedSendTheLeftOverInventory(true);

            //SendTheOrders(null);

            var batches = Batch.List.Where(x => orders.Any(y => y.BatchId == x.Id));

            foreach (var order in orders)
            {
                if (order.EndDate == DateTime.MinValue)
                    order.EndDate = DateTime.Now;

                if (order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(order.PrintedOrderId))
                    order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);

                order.Save();
            }

            DataAccess.SendTheOrders(batches, orders.Select(x => x.OrderId.ToString()).ToList());

            RouteEx.ClearAll();

            Logger.CreateLog("SendTheOrders took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            partialStart = DateTime.Now;
            if (Config.SendPaymentsInEOD)
                SendInvoicePayments(ordersTotals);

            if (File.Exists(Config.ClientProdSortFile))
            {
                if (Config.UseDraggableTemplate)
                {
                    SendClientProdSort();
                    Logger.CreateLog("Send ClientProdSortFile took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
                }
                else
                    File.Delete(Config.ClientProdSortFile);
            }

            partialStart = DateTime.Now;

            if (Config.ButlerCustomization)
            {
                SendButlerTransfers();
            }
            else
            {
                if (File.Exists(Config.TransferOnFile) || File.Exists(Config.TransferOffFile))
                {
                    SendTransfers();
                    Logger.CreateLog("Transfers sent took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
                }
            }

            Logger.CreateLog("SendInvoicePayments took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            if (Config.Delivery)
            {
                partialStart = DateTime.Now;
                SendTheLeftOverInventory(extendedMap);
                Logger.CreateLog("SendTheLeftOverInventory took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }
            if (BuildToQty.List.Count > 0)
            {
                partialStart = DateTime.Now;
                SendBuildToQty();
                Logger.CreateLog("SendBuildToQty took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }
            partialStart = DateTime.Now;
            SendDayReport();
            Logger.CreateLog("SendDayReport took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            partialStart = DateTime.Now;

            if (!Config.SetParLevel)
            {
                LoadOrder.LoadList();
                SendLoadOrder();
                Logger.CreateLog("SendLoadOrder took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }
            else
            {
                ParLevel.LoadList();
                SendParLevel();
                Logger.CreateLog("SendParLevel took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }

            partialStart = DateTime.Now;
            if (File.Exists(Config.SavedDailyParLevelFile))
            {
                SendDailyParLevel();
                Logger.CreateLog("Send DailyParLevel took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }

            partialStart = DateTime.Now;
            if (Config.SalesByDepartment && ClientDepartment.Departments.Any(x => x.Updated = true))
            {
                SendClientDepartments();
                Logger.CreateLog("Send ClientDepartments took: " + DateTime.Now.Subtract(partialStart).TotalSeconds);
            }

            ProductInventory.ClearAll();

            DeleteTransferFiles();

            Logger.CreateLog("SendAll took: " + DateTime.Now.Subtract(globalStart).TotalSeconds);

            if (File.Exists(Config.DeliveryFile))
                File.Delete(Config.DeliveryFile);

            Config.SessionId = string.Empty;
            Config.SaveSessionId();
        }

        public static void SendClientDepartments(bool delete = true)
        {
            string tempFile = Path.GetTempFileName();

            using (var writer = new StreamWriter(tempFile))
            {
                foreach (var item in ClientDepartment.Departments.Where(x => x.Updated))
                    writer.WriteLine(item.Serialize());
            }

            NetAccess.SaveClientDepartments(tempFile, delete);
        }

        public static void SendDayReport()
        {
            NetAccess.SendDayReport(Config.SessionId);
        }

        public static void SendLoadOrder()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                if (LoadOrder.List.Count == 0)
                {
                    if (File.Exists(Config.LoadOrderFile))
                        File.Delete(Config.LoadOrderFile);

                    LoadOrder.Date = DateTime.MinValue;
                    return;
                }

                string dstFile = Path.GetTempFileName();
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (var writer = new StreamWriter(dstFile))
                    {
                        writer.Write(LoadOrder.Date.ToString(CultureInfo.InvariantCulture));

                        if (CheckCommunicatorVersion(CommunicatorVersion, "29.94"))
                        {
                            writer.Write((char)20);
                            writer.Write(LoadOrder.Term ?? "");
                            writer.Write((char)20);
                            writer.Write(LoadOrder.UniqueId);
                            writer.Write((char)20);
                            writer.Write(LoadOrder.Comment ?? "");
                            writer.Write((char)20);
                            writer.Write(LoadOrder.SiteId.ToString());
                            writer.Write((char)20);
                            writer.Write((LoadOrder.PrintedOrderId ?? "").ToString());
                        }
                        else if (Config.UseTermsInLoadOrder)
                        {
                            writer.Write((char)20);
                            writer.Write(LoadOrder.Term ?? "");
                        }

                        writer.WriteLine();

                        int offset = 0;

                        foreach (var p in LoadOrder.List)
                        {
                            writer.Write(p.Product.ProductId);              //0
                            writer.Write((char)20);
                            writer.Write(p.Qty);                            //1
                            writer.Write((char)20);
                            writer.Write(p.UoM != null ? p.UoM.Id : 0);     //2

                            if (offset == 0)
                            {
                                writer.Write((char)20);
                                writer.Write(LoadOrder.UniqueId);           //3

                                writer.Write((char)20);
                                writer.Write(Config.SessionId ?? "");       //4
                            }

                            if (DataAccess.CheckCommunicatorVersion(CommunicatorVersion, "52.2.0"))
                            {
                                if (offset != 0)
                                {
                                    writer.Write((char)20);
                                    writer.Write("");           //3

                                    writer.Write((char)20);
                                    writer.Write("");       //4
                                }
                                
                                writer.Write((char)20);
                                writer.Write(p.Comments ?? "");     //5
                                
                                writer.Write((char)20);
                                writer.Write(p.Lot ?? "");     //6
                            }

                            writer.WriteLine();

                            offset++;
                        }
                    }

                    NetAccess.SendTheLoadOrder(dstFile, LoadOrder.SalesmanId);

                    LoadOrder.Date = DateTime.MinValue;
                    LoadOrder.List.Clear();
                    LoadOrder.SaveList();

                    if (File.Exists(Config.LoadOrderFile))
                        File.Delete(Config.LoadOrderFile);
                    File.Delete(dstFile);

                }
                catch (Exception ex)
                {
                    //Log the exception
                    Logger.CreateLog(ex);
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = true;
                }
            }
        }

        public static void SendBuildToQty()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string dstFile = Path.GetTempFileName();
                try
                {
                    //FileOperationsLocker.InUse = true;

                    bool somethingSent = false;
                    using (var writer = new StreamWriter(dstFile))
                    {
                        foreach (var p in BuildToQty.List)
                        {
                            somethingSent = true;
                            writer.Write(p.ProductId);
                            writer.Write((char)20);
                            writer.Write(p.ClientId);
                            writer.Write((char)20);
                            writer.Write(p.Qty);
                            writer.WriteLine();
                        }
                    }

                    if (!somethingSent)
                    {
                        Logger.CreateLog("send build to was empty, skipping it");
                        File.Delete(dstFile);
                        return;
                    }
                    //network access
                    NetAccess.SendTheBuildQty(dstFile);

                    File.Delete(dstFile);
                }
                catch (Exception ex)
                {
                    //Xamarin.Insights.Report(ex);
                    //Log the exception
                    Logger.CreateLog(ex);
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static public List<InventorySettlementRow> ExtendedSendTheLeftOverInventory(bool fromSend = false, bool fromInventorySummary = false)
        {
            Product.AdjustValuesFromOrder();

            var map = new List<InventorySettlementRow>();

            foreach (var product in Product.Products.Where(x => x.ProductType == ProductType.Inventory))
            {
                if (product.CategoryId == 0 && product.RequestedLoadInventory == 0)
                    continue;

                foreach (var productInv in product.ProductInv.TruckInventories)
                {
                    var inv = new InventorySettlementRow();
                    inv.Product = product;
                    inv.Lot = productInv.Lot;
                    inv.BegInv = productInv.BeginingInventory;
                    inv.LoadOut = productInv.RequestedLoad;
                    inv.Adj = productInv.Loaded - productInv.RequestedLoad;
                    inv.TransferOn = productInv.TransferredOn;
                    inv.TransferOff = productInv.TransferredOff;
                    inv.Sales = productInv.OnSales;
                    inv.Dump = inv.CreditDump = productInv.OnCreditDump;
                    inv.Return = inv.CreditReturns = productInv.OnCreditReturn;
                    inv.Reshipped = productInv.OnReships;
                    inv.Unload = productInv.Unloaded;
                    inv.DamagedInTruck = productInv.DamagedInTruck;
                    inv.EndInventory = productInv.CurrentQty;
                    inv.Weight = productInv.Weight;

                    if (!string.IsNullOrEmpty(product.UoMFamily))
                    {
                        var uoms = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily);

                        if (!fromSend && Config.SettReportInSalesUoM)
                        {
                            inv.UoM = uoms.FirstOrDefault(x => x.IsDefault);

                            if (inv.UoM != null)
                            {
                                inv.BegInv /= inv.UoM.Conversion;
                                inv.LoadOut /= inv.UoM.Conversion;
                                inv.Adj /= inv.UoM.Conversion;
                                inv.TransferOn /= inv.UoM.Conversion;
                                inv.TransferOff /= inv.UoM.Conversion;
                                inv.Sales /= inv.UoM.Conversion;
                                inv.Dump /= inv.UoM.Conversion;
                                inv.CreditDump /= inv.UoM.Conversion;
                                inv.CreditReturns /= inv.UoM.Conversion;
                                inv.Return /= inv.UoM.Conversion;
                                inv.Reshipped /= inv.UoM.Conversion;
                                inv.Unload /= inv.UoM.Conversion;
                                inv.DamagedInTruck /= inv.UoM.Conversion;
                                inv.EndInventory /= inv.UoM.Conversion;
                            }
                        }
                        else
                            inv.UoM = uoms.FirstOrDefault(x => x.IsBase);
                    }

                    map.Add(inv);
                }
            }

            map = map.Where(x => !x.IsEmpty).ToList();

            if (Config.ShortInventorySettlement && !fromInventorySummary && !fromSend)
                map = map.Where(x => !x.IsShort).ToList();

            return map;
        }

        public static void SendTheLeftOverInventory(List<InventorySettlementRow> extendedMap)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string dstFile = Path.GetTempFileName();
                try
                {
                    //FileOperationsLocker.InUse = true;

                    bool somethingSent = false;
                    using (var writer = new StreamWriter(dstFile))
                    {
                        foreach (var p1 in SortDetails.SortedDetails(extendedMap))
                        {
                            var p = p1.Product;

                            if (p1.IsEmpty)
                                continue;

                            somethingSent = true;
                            writer.Write(p.ProductId);
                            writer.Write((char)20);
                            writer.Write(p.CurrentInventory); // the left over
                            writer.Write((char)20);
                            writer.Write(p.BeginigInventory); // beg inventory
                            writer.Write((char)20);
                            writer.Write(p.RequestedLoadInventory - p.LoadedInventory); // adjustments
                            writer.Write((char)20);
                            writer.Write(p.TransferredOnInventory - p.TransferredOffInventory); // transferred
                            writer.Write((char)20);
                            writer.Write(0); // dumped
                            writer.Write((char)20);
                            writer.Write(p.UnloadedInventory); // returned

                            //accepta carga DOS DETAILS CON DIFERENT UOM

                            //update de producto (donde cambia la asigancion de UOM) se inactiva la que tenia cuando aceptamos

                            //EOD verificar las cantidades

                            var extendedItem = p1;

                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture));   // 7
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture));  // 8
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.Adj, Config.Round).ToString(CultureInfo.CurrentCulture));      // 9
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.TransferOn - extendedItem.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture));  //10
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.Sales, Config.Round).ToString(CultureInfo.CurrentCulture));    //11
                            writer.Write((char)20);
                            writer.Write(Math.Round((extendedItem.CreditDump + extendedItem.CreditReturns), Config.Round).ToString(CultureInfo.CurrentCulture));  // 12
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture));   //13
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.Unload, Config.Round).ToString(CultureInfo.CurrentCulture)); //14
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)); // 15
                            writer.Write((char)20);
                            writer.Write(string.IsNullOrEmpty(extendedItem.OverShort) ? "0" : extendedItem.OverShort);  //16
                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture)); //17

                            if (!string.IsNullOrEmpty(Config.SessionId))
                            {
                                writer.Write((char)20);
                                writer.Write(Config.SessionId); // 18
                            }

                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.LoadingError, Config.Round).ToString(CultureInfo.CurrentCulture)); //19

                            writer.Write((char)20);
                            writer.Write(Config.EndingInventoryCounted ? "1" : "0"); //20

                            writer.Write((char)20);
                            writer.Write(extendedItem.Lot); //21

                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.Reshipped, Config.Round).ToString(CultureInfo.CurrentCulture)); //22

                            writer.Write((char)20);
                            writer.Write(Math.Round(extendedItem.Weight, Config.Round).ToString(CultureInfo.CurrentCulture)); //23

                            writer.WriteLine();
                        }
                    }
                    if (!somethingSent)
                    {
                        Logger.CreateLog("Send Left over was blank, skipping it");
                        File.Delete(dstFile);
                        return;
                    }
                    //network access
                    NetAccess.SendTheLeftOverInventory(dstFile);

                    File.Delete(dstFile);
                }
                catch (Exception ex)
                {
                    //Log the exception
                    Logger.CreateLog(ex);
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static DataTable AddNewClients(List<int> clientIds)
        {
            var table = new DataTable("AddedClients") { Locale = CultureInfo.InvariantCulture };

            table.Columns.Add("ClientId", typeof(int));   //1
            table.Columns.Add("UniqueId", typeof(string));   //1
            table.Columns.Add("Comment", typeof(string));      //4
            table.Columns.Add("ContactName", typeof(string));  //5
            table.Columns.Add("ContactPhone", typeof(string)); //6
            table.Columns.Add("CreditLimit", typeof(double)); //13
            table.Columns.Add("Latitude", typeof(double)); //13
            table.Columns.Add("Longitude", typeof(double)); //14 
            table.Columns.Add("Name", typeof(string));   //1
            table.Columns.Add("TaxRate", typeof(float)); //15

            table.Columns.Add("Address1", typeof(string));   //1
            table.Columns.Add("Address2", typeof(string));   //1
            table.Columns.Add("city", typeof(string));   //1
            table.Columns.Add("state", typeof(string));   //1
            table.Columns.Add("zip", typeof(string));   //1
            table.Columns.Add("ExtraFields", typeof(string));   //1
            table.Columns.Add("NonVisibleExtraFields", typeof(string));   //1
            table.Columns.Add("PriceLevelId", typeof(int));   //1
            table.Columns.Add("RetailPriceLevelId", typeof(int));   //1
            table.Columns.Add("TermsId", typeof(int));   //1


            foreach (Client c in Client.Clients)
                if (c.ClientId < 0 && clientIds.Contains(c.ClientId))
                {
                    DataRow row = table.NewRow();

                    row["ClientId"] = c.ClientId;
                    row["UniqueId"] = c.UniqueId;
                    row["Comment"] = c.Comment;
                    row["ContactName"] = c.ContactName;
                    row["ContactPhone"] = c.ContactPhone;
                    row["CreditLimit"] = 0;
                    row["Latitude"] = 0;
                    row["Longitude"] = 0;
                    row["PriceLevelId"] = c.PriceLevel;
                    row["RetailPriceLevelId"] = c.RetailPriceLevelId;
                    row["TermsId"] = c.TermId;
                    row["Name"] = c.ClientName;
                    row["TaxRate"] = c.TaxRate;

                    if (!string.IsNullOrEmpty(c.ShipToAddress))
                    {
                        var parts = c.ShipToAddress.Split(new char[] { '|' });
                        row["Address1"] = parts[0];
                        row["Address2"] = parts[1];
                        row["city"] = parts[2];
                        row["state"] = parts[3];
                        row["zip"] = parts[4];
                    }
                    else
                    {
                        Logger.CreateLog("new client " + c.ClientName + " has ship address in null");
                        row["Address1"] = string.Empty;
                        row["Address2"] = string.Empty;
                        row["city"] = string.Empty;
                        row["state"] = string.Empty;
                        row["zip"] = string.Empty;
                    }

                    var extraFields = c.ExtraPropertiesAsString ?? string.Empty;
                    if (Config.NewClientCanHaveDiscount)
                        extraFields = DataAccess.RemoveSingleUDF("allowDiscount", extraFields);

                    row["ExtraFields"] = extraFields;
                    row["NonVisibleExtraFields"] = c.NonvisibleExtraPropertiesAsString ?? string.Empty;

                    table.Rows.Add(row);
                }
            if (table.Rows.Count > 0)
                return table;
            else
                return null;
        }

        public static void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false)
        {
            if (ordersId != null)
            {
                foreach (var id in ordersId)
                {
                    int orderId = 0;
                    Int32.TryParse(id, out orderId);

                    if (orderId > 0)
                    {
                        var order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);

                        if (order != null && order.BatchId > 0 &&
                            Batch.List.FirstOrDefault((x => x.Id == order.BatchId)) == null)
                        {
                            //fix holpeca ip foods 
                            var batch = new Batch(order.Client);
                            batch.Client = order.Client;
                            batch.ClockedIn = DateTime.Now;
                            batch.ClockedOut = DateTime.Now;
                            batch.Save();

                            order.BatchId = batch.Id;
                            order.Save();
                        }
                    }
                }
            }

            if (source == null)
                source = Batch.List;

            if (source == null || source.Count() == 0)
            {
                Logger.CreateLog("Sending order from an empty source, skipping it");
                return;
            }

            string dstFile = Path.Combine(Config.OrderPath, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            string dstFileZipped = dstFile + ".zip";

            try
            {
                var historyFile = GetOrderHistoryFile(source, ordersId);

                var paymentsSource = SerializeOrdersToDataSet(source, dstFile, ordersId, Config.SessionId);

                ZipFile(dstFile, dstFileZipped);

                NetAccess.SendTheOrders(dstFileZipped);

                File.Delete(dstFileZipped);

                NetAccess.GetCommunicatorVersion();
                bool includeUniqueId = false;
                if (!string.IsNullOrEmpty(DataAccess.CommunicatorVersion))
                {
                    var parts = DataAccess.CommunicatorVersion.Split('.');
                    var i = Convert.ToInt32(parts[0]);

                    includeUniqueId = (i > 13 && i < 20) || i > 21;
                }

                dstFile = dstFile + ".signature";
                dstFileZipped = dstFile + ".zip";

                var anySignature = SerializeSignatures(source, dstFile, ordersId, includeUniqueId);
                if (anySignature)
                {
                    ZipFile(dstFile, dstFileZipped);

                    NetAccess.SendTheSignatures(dstFileZipped);

                    File.Delete(dstFileZipped);
                }


                try
                {
                    var path = Path.GetTempPath();
                    var anyPrintedZPL = PrintedOrderZPL.PrintedOrders.ToList();
                    if (Config.SendZplOrder && anyPrintedZPL.Count > 0)
                    {
                        dstFile = Path.Combine(path, "zplsignature_" + Guid.NewGuid().ToString());
                        dstFileZipped = dstFile + ".zip";
                        var anyPZLOrders = SerializeZPLPrintOrders(source, dstFile, ordersId, includeUniqueId);
                        if (anyPZLOrders)
                        {
                            ZipFile(dstFile, dstFileZipped);
                            NetAccess.SendZPLPrinter(dstFileZipped);
                            File.Delete(dstFileZipped);
                        }

                        Directory.Delete(Config.ZPLOrdersPrintedPath, true);

                        if (PrintedOrderZPL.PrintedOrders != null)
                            PrintedOrderZPL.PrintedOrders.Clear();

                        Directory.CreateDirectory(Config.ZPLOrdersPrintedPath);

                        if (File.Exists(dstFile))
                            File.Delete(dstFile);
                    }
                    else
                    {
                        Directory.Delete(Config.ZPLOrdersPrintedPath, true);

                        if (PrintedOrderZPL.PrintedOrders != null)
                            PrintedOrderZPL.PrintedOrders.Clear();

                        Directory.CreateDirectory(Config.ZPLOrdersPrintedPath);
                    }
                }
                catch (Exception ex)
                {

                }

                var imageZipFile = SerializeOrderImages(source, ordersId);
                if (!string.IsNullOrEmpty(imageZipFile) && File.Exists(imageZipFile))
                {
                    NetAccess.SendOrdersImages(imageZipFile);
                    File.Delete(imageZipFile);
                }

                if (!string.IsNullOrEmpty(historyFile) && File.Exists(historyFile))
                {
                    try
                    {
                        NetAccess.SendOrderHistory(historyFile);
                        File.Delete(historyFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog("SendingOrderHistory " + ex);
                    }
                }

                if (sendPayment && paymentsSource != null && paymentsSource.Count > 0)
                {
                    SendInvoicePayments(paymentsSource);
                    Logger.CreateLog("Payments Sent with order(s)");
                }

                DataAccess.SendSalesmanDeviceInfo();

                if (deleteOrders)
                {
                    //Remove zipped files older than X
                    RemoveOlderFiles();

                    //Delete all the orders
                    List<Order> orders = new List<Order>();

                    foreach (var batch in source)
                        foreach (var order in Order.Orders.Where(x => x.BatchId == batch.Id))
                            orders.Add(order);
                    foreach (var batch in source.ToList())
                        batch.Delete();
                    foreach (Order o in orders)
                        o.ForceDelete();

                    // send the client notes
                    if (File.Exists(Config.ClientNotesStoreFile))
                    {
                        NetAccess.SendClientNotes(Config.ClientNotesStoreFile);
                        File.Delete(Config.ClientNotesStoreFile);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                if (File.Exists(dstFile))
                    File.Delete(dstFile);
                if (File.Exists(dstFileZipped))
                    File.Delete(dstFileZipped);
                throw;
            }
        }

        static bool SerializeZPLPrintOrders(IEnumerable<Batch> source, string dstFile, List<string> ordersId, bool includeUniqueId)
        {
            bool hasAny = false;

            using (var writer = new StreamWriter(dstFile))
            {
                foreach (var batch in source)
                {
                    var orders = batch.Orders();
                    if (ordersId != null)
                        orders = orders.Where(x => ordersId.Contains(x.OrderId.ToString())).ToList();

                    foreach (var order in orders)
                    {
                        var anyPrintedZPL = PrintedOrderZPL.PrintedOrders.ToList();
                        var file = anyPrintedZPL.FirstOrDefault(x => x.UniqueId == order.UniqueId);
                        if (file != null)
                        {
                            hasAny = true;

                            using (var reader = new StreamReader(file.Filename))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (string.IsNullOrEmpty(line))
                                    {
                                        Logger.CreateLog("Found an empty line");
                                        continue;
                                    }
                                    string result = string.Format("{0}|{1}",
                                        order.UniqueId,
                                        line);

                                    writer.WriteLine(result);
                                }
                            }
                        }

                    }
                }
            }
            if (!hasAny)
                File.Delete(dstFile);


            return hasAny;

        }


        private static string SerializeOrderImages(IEnumerable<Batch> source, List<string> ordersId)
        {
            try
            {
                List<string> imagesPath = new List<string>();
                List<Tuple<string, string>> mapImages = new List<Tuple<string, string>>();

                //creando las estructuras
                foreach (var batch in source)
                {
                    var orders = batch.Orders();
                    if (ordersId != null)
                        orders = orders.Where(x => ordersId.Contains(x.OrderId.ToString())).ToList();

                    foreach (var order in orders)
                    {
                        foreach (var image in order.ImageList)
                        {
                            mapImages.Add(new Tuple<string, string>(order.UniqueId, image));
                            imagesPath.Add(image);
                        }
                    }
                }

                if (imagesPath.Count == 0)
                    return string.Empty;

                var tempPathFile = Path.Combine(Path.GetTempPath(), "ordersImages.zip");

                if (File.Exists(tempPathFile))
                    File.Delete(tempPathFile);

                string tempPathFolder = Path.Combine(Path.GetTempPath(), "OrdersImages");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                Directory.CreateDirectory(tempPathFolder);

                DirectoryInfo dir = new DirectoryInfo(Config.OrderImageStorePath);
                if (!dir.Exists)
                {
                    Logger.CreateLog(Config.OrderImageStorePath + " directory not found");
                    return string.Empty;
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (imagesPath.Contains(file.Name))
                    {
                        string temppath = Path.Combine(tempPathFolder, file.Name + ".png");
                        file.CopyTo(temppath, false);
                    }
                }

                string mapFile = Path.Combine(tempPathFolder, "ordersImgMap");

                if (File.Exists(mapFile))
                    File.Delete(mapFile);

                // if (!Config.DoNotShrinkOrderImage)
                // {
                //     DirectoryInfo tempDirInfo = new DirectoryInfo(tempPathFolder);
                //     foreach (FileInfo file in tempDirInfo.GetFiles())
                //     {
                //         if (file.FullName != mapFile)  // Skip the map file
                //         {
                //             ImageOptimizer.ResizeAndOptimizeImageAsJpg(file.FullName);
                //         }
                //     }
                // }

                using (var writer = new StreamWriter(mapFile))
                {
                    foreach (var item in mapImages)
                        writer.WriteLine(item.Item1 + "," + item.Item2);
                }

                var fastZip = new FastZip();
                fastZip.CreateZip(tempPathFile, tempPathFolder, true, null);

                return tempPathFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        private static string SerializePaymentImages()
        {
            try
            {
                var tempPathFile = Path.Combine(Path.GetTempPath(), "paymentImages.zip");

                if (File.Exists(tempPathFile))
                    File.Delete(tempPathFile);

                string tempPathFolder = Path.Combine(Path.GetTempPath(), "PaymentImages");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                Directory.CreateDirectory(tempPathFolder);

                DirectoryInfo dir = new DirectoryInfo(Config.PaymentImagesPath);
                if (!dir.Exists)
                {
                    Logger.CreateLog(Config.PaymentImagesPath + " directory not found");
                    return string.Empty;
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(tempPathFolder, file.Name);
                    file.CopyTo(temppath, false);
                }

                var fastZip = new FastZip();
                fastZip.CreateZip(tempPathFile, tempPathFolder, true, null);

                return tempPathFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        static bool SerializeSignatures(IEnumerable<Batch> source, string dstFile, List<string> ordersId, bool includeUniqueId)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    bool hasAny = false;
                    using (StreamWriter writer = new StreamWriter(dstFile))
                    {
                        foreach (var batch in source)
                        {
                            var orders = batch.Orders();
                            if (ordersId != null)
                                orders = orders.Where(x => ordersId.Contains(x.OrderId.ToString())).ToList();

                            foreach (var order in orders)
                            {
                                string s = order.SerializeSignatureAsString(order.SignaturePoints);
                                if (!string.IsNullOrEmpty(s))
                                {
                                    hasAny = true;

                                    string result = string.Format("{0}|{1}",
                                        order.UniqueId,
                                        s);

                                    if (includeUniqueId && !string.IsNullOrEmpty(order.SignatureUniqueId))
                                        result += "|" + order.SignatureUniqueId;

                                    writer.WriteLine(result);
                                }
                            }
                        }
                    }
                    if (!hasAny)
                        File.Delete(dstFile);

                    return hasAny;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void ZipFile(string fileToZip, string targetFile)
        {
            if (!File.Exists(fileToZip))
            {
                Exception e = new System.IO.FileNotFoundException("The specified file " + fileToZip + " could not be found. Zipping aborderd");
                throw e;
            }

            string dstFolder = Path.Combine(Config.DataPath, Path.GetFileNameWithoutExtension(fileToZip));

            Directory.CreateDirectory(dstFolder);
            string dstFile = Path.Combine(dstFolder, Path.GetFileName(fileToZip));

            File.Copy(fileToZip, dstFile);

            var fastZip = new FastZip();
            fastZip.CreateZip(targetFile, dstFolder, true, null);

            Directory.Delete(dstFolder, true);
        }

        public static void UnzipFile(string sourceFile, string targetFile)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (var fileStream = File.OpenRead(sourceFile))
                    {
                        using (ZipInputStream s = new ZipInputStream(fileStream))
                        {
                            //Logger.CreateLog ("got first stream");
                            //ZipEntry theEntry;
                            while ((s.GetNextEntry()) != null)
                            {

                                using (FileStream streamWriter = File.Create(targetFile))
                                {

                                    int size = 10240;
                                    byte[] data = new byte[10240];
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size > 0)
                                        {
                                            streamWriter.Write(data, 0, size);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static public List<PaymentSplit> SplitPayment(InvoicePayment payment_, Dictionary<string, double> ordersTotals = null)
        {
            var retList = new List<PaymentSplit>();
            if (payment_ == null)
                return retList;
            if (ordersTotals == null)
            {
                ordersTotals = new Dictionary<string, double>();
                foreach (var orderUniqueId in payment_.OrderId.Split(','))
                {
                    var order = Order.Orders.FirstOrDefault(x => x.UniqueId == orderUniqueId);
                    if (order != null)
                        ordersTotals.Add(order.UniqueId, order.OrderTotalCost());
                }
            }
            var components = new List<PaymentComponent>();
            foreach (var component in payment_.Components)
                components.Add(new PaymentComponent() { Amount = component.Amount, Comments = component.Comments, PaymentMethod = component.PaymentMethod, Ref = component.Ref, ExtraFields = component.ExtraFields });
            foreach (var component in components)
            {
                double oldAmount = component.Amount;
                while (component.Amount > 0)
                {
                    foreach (var orderUniqueId in payment_.OrderId.Split(','))
                    {
                        if (!ordersTotals.ContainsKey(orderUniqueId))
                        {
                            continue;
                        }
                        if (ordersTotals[orderUniqueId] <= 0)
                            continue;
                        double usedInThisInvoice = component.Amount;
                        if (component.Amount > ordersTotals[orderUniqueId])
                        {
                            //if (Config.CanPayMoreThanOwned)
                            //{
                            //    usedInThisInvoice = component.Amount;
                            //    component.Amount = 0;
                            //}
                            //else
                            //{
                            usedInThisInvoice = ordersTotals[orderUniqueId];
                            component.Amount = component.Amount - usedInThisInvoice;
                            //}
                        }
                        else
                            component.Amount = 0;


                        ordersTotals[orderUniqueId] = ordersTotals[orderUniqueId] - usedInThisInvoice;

                        PaymentSplit ps = new PaymentSplit();
                        ps.UniqueId = orderUniqueId;
                        ps.Amount = usedInThisInvoice;
                        ps.PaymentMethod = component.PaymentMethod;
                        ps.Ref = component.Ref;
                        ps.Comments = component.Comments;
                        ps.ExtraFields = component.ExtraFields;

                        retList.Add(ps);
                        if (component.Amount == 0)
                            break;
                    }

                    foreach (var orderUniqueId in payment_.InvoicesId.Split(','))
                    {
                        double usedInThisInvoice = 0;
                        if (ordersTotals.Count > 0)
                        {
                            if (!ordersTotals.ContainsKey(orderUniqueId))
                                continue;
                            if (ordersTotals[orderUniqueId] <= 0)
                                continue;
                            usedInThisInvoice = component.Amount;
                            if (component.Amount > ordersTotals[orderUniqueId])
                            {

                                usedInThisInvoice = ordersTotals[orderUniqueId];
                                component.Amount = component.Amount - usedInThisInvoice;
                            }
                            else
                                component.Amount = 0;

                        }



                        PaymentSplit ps = new PaymentSplit();
                        ps.UniqueId = orderUniqueId;
                        ps.Amount = usedInThisInvoice > 0 ? usedInThisInvoice : component.Amount;
                        ps.PaymentMethod = component.PaymentMethod;
                        ps.Ref = component.Ref;
                        ps.Comments = component.Comments;
                        ps.ExtraFields = component.ExtraFields;

                        retList.Add(ps);
                        if (component.Amount == 0)
                            break;
                    }

                    if (oldAmount == component.Amount)
                    {
                        Logger.CreateLog("component.Amout did nt change, what happenned?");
                        break;
                    }
                    oldAmount = component.Amount;
                }
            }

            return retList;
        }

        public class PaymentSplit
        {
            public string UniqueId { get; set; }
            public double Amount { get; set; }
            public InvoicePaymentMethod PaymentMethod { get; set; }
            public string Ref { get; set; }
            public string Comments { get; set; }
            public string ExtraFields { get; set; }
        }

        public static void SendParLevel()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                if (ParLevel.List.Count == 0)
                {
                    Logger.CreateLog("Np par level to send");
                    return;
                }
                string dstFile = Path.GetTempFileName();

                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (var writer = new StreamWriter(dstFile))
                    {
                        foreach (var p in ParLevel.List)
                        {
                            writer.Write(p.Product.ProductId);
                            writer.Write((char)20);
                            writer.Write(p.Qty);
                            writer.WriteLine();
                        }
                    }

                    //network access
                    //if (LoadOrder.List.Count > 0)
                    NetAccess.SendTheParLevel(dstFile);
                    ParLevel.List.Clear();
                    if (File.Exists(Config.ParLevelFile))
                        File.Delete(Config.ParLevelFile);
                    File.Delete(dstFile);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static string GetSalesReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    string communicatorCommand = "DailySalesReportCommand";
                    if (CheckCommunicatorVersion(CommunicatorVersion, "28.3") && !Config.UseLaceupDataInSalesReport)
                        communicatorCommand = "SalesReportCommand";

                    // Get the orders
                    netaccess.WriteStringToNetwork(communicatorCommand);
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetOnPOInventory()
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    var filepath = Path.Combine(Config.CodeBase, "POInventories");
                    if (File.Exists(filepath))
                        File.Delete(filepath);

                    File.Create(filepath);

                    bool errorProd = false;

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
                    netaccess.WriteStringToNetwork("OnPOInventoryCommand");

                    errorProd = netaccess.ReceiveFile(filepath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (!errorProd)
                        return filepath;
                    else
                        return string.Empty;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }


        public static string GetPaymentsReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("ReceivedPaymentsReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetCommissionReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    string reportCommand = "CommissionReportCommand";
                    if (CheckCommunicatorVersion(CommunicatorVersion, "30.0.0.0") && !Config.ShowOldReportsRegardless)
                        reportCommand = "SalesmanCommByCustReportCommand";

                    // Get the orders
                    netaccess.WriteStringToNetwork(reportCommand);
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetSalesmenCommReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    string reportCommand = "SalesmenCommReportCommand";
                    if (CheckCommunicatorVersion(CommunicatorVersion, "30.0.0.0") && !Config.ShowOldReportsRegardless)
                        reportCommand = "SalesmanCommByCustReportCommand";

                    // Get the orders
                    netaccess.WriteStringToNetwork(reportCommand);
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetQtyProdSalesReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("QtyProdSalesReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetSalesProdCatReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("SalesByProdCatReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetSalesReportWithDetails(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("SalesReportWithDetailsCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetTransmissionReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("TransmissionReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string GetLoadOrderReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("LoadOrderReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }


        public static string GetSAPReport(string command)
        {
            try
            {
                Logger.CreateLog("Running report");
                using (NetAccess netaccess = new NetAccess())
                {
                    string fullPath = Path.GetTempFileName();

                    bool errorProd = false;

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

                    // Get the orders
                    netaccess.WriteStringToNetwork("SAPOrderStatusReportCommand");
                    netaccess.WriteStringToNetwork(command);

                    errorProd = netaccess.ReceiveFile(fullPath) == 0;

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return fullPath;
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception(e.ToString());
            }
        }

        public static string SyncSingleUDF(string udfName, string udfValue, string current)
        {
            var currentList = ExplodeExtraProperties(current);

            var udf = currentList.FirstOrDefault(x => x.Key == udfName);
            if (udf == null)
            {
                udf = new KeyValuePairWritable<string, string>(udfName, udfValue ?? string.Empty);
                currentList.Add(udf);
            }
            else
                udf.Value = udfValue ?? string.Empty;

            return ImplodeExtraProperties(currentList);
        }

        public static string SyncSingleUDF(string udfName, string udfValue, string current, List<KeyValuePairWritable<string, string>> currentList = null, bool concat = true)
        {
            if (currentList == null)
                currentList = DataAccess.ExplodeExtraProperties(current);

            var udf = currentList.FirstOrDefault(x => x.Key.ToLowerInvariant() == udfName.ToLowerInvariant());
            if (udf == null)
            {
                udf = new KeyValuePairWritable<string, string>(udfName, udfValue ?? string.Empty);
                currentList.Add(udf);
            }
            else
                udf.Value = udfValue ?? string.Empty;

            if (concat)
                return DataAccess.ImplodeExtraProperties(currentList);
            return null;
        }

        public static string RemoveSingleUDF(string udfName, string current)
        {
            var currentList = ExplodeExtraProperties(current);

            var udf = currentList.FirstOrDefault(x => x.Key == udfName);
            if (udf != null)
                currentList.Remove(udf);

            return ImplodeExtraProperties(currentList);
        }

        public static string GetSingleUDF(string udfName, string current)
        {
            var currentList = ExplodeExtraProperties(current);

            var udf = currentList.FirstOrDefault(x => x.Key.ToUpperInvariant() == udfName.ToUpperInvariant());
            if (udf == null)
                return string.Empty;
            else
                return udf.Value;
        }

        static public List<KeyValuePairWritable<string, string>> ExplodeExtraProperties(string ExtraFields)
        {
            var retValue = new List<KeyValuePairWritable<string, string>>();
            if (!string.IsNullOrEmpty(ExtraFields))
            {
                string[] parts = ExtraFields.Split(ExtraPropertiesSeparator);
                foreach (string part in parts)
                {
                    string[] subParts = part.Split(ExtraPropertySeparator);
                    if (subParts.Length != 2)
                    {
                        Logger.CreateLog("Found a miss formed UDF string, " + ExtraFields);
                        continue;
                    }
                    string keyPart = subParts[0];
                    string valuePart = subParts[1];
                    retValue.Add(new KeyValuePairWritable<string, string>(keyPart, valuePart));
                }
            }
            return retValue;
        }

        static public string ImplodeExtraProperties(List<KeyValuePairWritable<string, string>> properties)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in properties)
            {
                if (sb.Length > 0)
                    sb.Append("|");
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value);
            }
            return sb.ToString();
        }


        public static void GetSalesmanList()
        {
            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                Logger.CreateLog("downloading salesman list");
                using (NetAccess netaccess = new NetAccess())
                {
                    bool errorProd = false;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("GetSalesmenListCommand");
                    string basefileP = System.IO.Path.GetTempFileName();

                    errorProd = netaccess.ReceiveFile(basefileP) == 0;
                    if (errorProd)
                    {
                        if (sb.Length > 0)
                            sb.Append(System.Environment.NewLine);
                        sb.Append("No salesman list file received");
                    }
                    Logger.CreateLog("got salesman");

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (errorProd)
                    {
                        Logger.CreateLog("error downloading salesman ");
                        return;
                    }
                    LoadSalesman(basefileP);
                    File.Delete(basefileP);
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
            }
            finally
            {
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time downloading salesman: " + ts.TotalSeconds);
        }

        static void LoadSalesman(string salesmansFile)
        {
            Salesman.Clear();
            using (StreamReader reader = new StreamReader(salesmansFile))
            {
                string line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');

                    int id = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                    string name = parts[1];

                    var salesman = new Salesman() { Id = id, Name = name.Substring(1, name.Length - 2), IsActive = true };

                    if (parts.Length > 2)
                        salesman.OriginalId = parts[2];

                    if (parts.Length > 3)
                        salesman.Email = parts[3];

                    if (parts.Length > 4)
                        salesman.Password = parts[4];

                    if (parts.Length > 5)
                        salesman.Loginname = parts[5];

                    try
                    {
                        if (parts.Length > 6)
                            salesman.Roles = (SalesmanRole)Convert.ToInt32(parts[6]);

                        if (parts.Length > 7)
                            salesman.ExtraProperties = parts[7];

                        if (parts.Length > 8)
                            salesman.RouteNumber = parts[8];
                    }
                    catch
                    {

                    }

                    Salesman.AddSalesman(salesman);
                }
            }
        }


        public static string GetFieldForLogin()
        {
            string field = string.Empty;
            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    bool errorProd = false;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetFieldForLoginCommand");

                    field = netaccess.ReadStringFromNetwork();

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
            }

            return field;
        }

        public static bool GetPendingLoadOrders(DateTime date, bool GetAll = false)
        {
            bool onlyQuotes = false;

            DeletePengingLoads();

            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                NetAccess.GetCommunicatorVersion();

                Logger.CreateLog("downloading salesman list");
                using (NetAccess netaccess = new NetAccess())
                {
                    bool errorProd = false;

                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("GetLoadOrdersCommand");

                    if (GetAll)
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "|" + date.ToString(CultureInfo.InvariantCulture) + "|1");
                    else
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "|" + date.ToString(CultureInfo.InvariantCulture));

                    string pendingLoads = System.IO.Path.GetTempFileName();
                    string pendingDel = System.IO.Path.GetTempFileName();

                    errorProd = netaccess.ReceiveFile(pendingLoads) == 0;
                    if (errorProd)
                    {
                        if (sb.Length > 0)
                            sb.Append(System.Environment.NewLine);
                        sb.Append("No load orders file received");
                    }
                    Logger.CreateLog("got load orders");

                    if (!errorProd)
                    {
                        LoadPendingLoads(pendingLoads);
                        File.Delete(pendingLoads);
                    }

                    if (Config.Delivery)
                    {
                        string s = Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + date.ToString(CultureInfo.InvariantCulture) + "," + "no";
                        if (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "25.0.0.0"))
                            s += ",no";

                        string command = "RouteInformation";

                        if (Config.NewSyncLoadOnDemand)
                        {
                            if (GetAll)
                                s = Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + date.ToString(CultureInfo.InvariantCulture) + ",no,1";
                            else
                                s = Config.SalesmanId.ToString(CultureInfo.InvariantCulture) + "," + date.ToString(CultureInfo.InvariantCulture);

                            command = "GetDeliveriesInSalesmanSiteCommand";
                        }

                        netaccess.WriteStringToNetwork(command);
                        netaccess.WriteStringToNetwork(s);
                        errorProd = netaccess.ReceiveFile(pendingDel) == 0;

                        if (errorProd)
                        {
                            if (sb.Length > 0)
                                sb.Append(System.Environment.NewLine);
                            sb.Append("No deliveries file received");
                        }
                        Logger.CreateLog("got delivery Orders");
                    }

                    onlyQuotes = LoadPendingDeliveries(pendingDel);
                    File.Delete(pendingDel);


                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (sb.Length > 0)
                        Logger.CreateLog(sb.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                return false;
            }
            finally
            {
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time downloading load orders: " + ts.TotalSeconds);

            return onlyQuotes;
        }

        public static void DeletePengingLoads()
        {
            var acceptedOrders = Order.Orders.Where(x => x.IsDelivery && !x.PendingLoad).ToList();
            var acceptedBatches = new List<Batch>();
            var acceptedClients = new List<Client>();

            foreach (var item in acceptedOrders)
            {
                if (acceptedBatches.FirstOrDefault(x => x.Id == item.BatchId) == null)
                {
                    var batch = Batch.List.FirstOrDefault(x => x.Id == item.BatchId);
                    acceptedBatches.Add(batch);

                    if (batch.Client.ClientId != item.Client.ClientId)
                        if (acceptedClients.FirstOrDefault(x => x.ClientId == batch.Client.ClientId) == null)
                            acceptedClients.Add(batch.Client);
                }
                if (acceptedClients.FirstOrDefault(x => x.ClientId == item.Client.ClientId) == null)
                    acceptedClients.Add(item.Client);
            }


            var orders = Order.Orders.Where(x => (x.OrderType == OrderType.Load || x.IsDelivery) && x.PendingLoad).ToList();

            var batchToDelete = new List<Batch>();

            for (int i = 0; i < orders.Count(); i++)
            {
                var order = orders[i];

                if (order.IsDelivery)
                {
                    bool removeBatch = acceptedBatches.All(x => x.Id != order.BatchId);
                    if (removeBatch)
                    {
                        var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
                        if (batch != null)
                        {
                            if (acceptedClients.All(x => x.ClientId != batch.Client.ClientId))
                            {
                                var batchClient = Client.Clients.FirstOrDefault(x => x.ClientId == batch.Client.ClientId && x.FromDelivery);
                                if (batchClient != null)
                                    Client.Remove(batchClient);
                            }
                            batchToDelete.Add(batch);
                        }
                    }

                    bool removeClient = acceptedClients.All(x => x.ClientId != order.Client.ClientId);
                    if (removeClient)
                    {
                        var client = Client.Clients.FirstOrDefault(x => x.ClientId == order.Client.ClientId && x.FromDelivery);
                        if (client != null)
                            Client.Remove(client);
                    }

                    var routeEx = RouteEx.Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == order.OrderId);
                    if (routeEx != null)
                        RouteEx.Routes.Remove(routeEx);
                }

                order.ForceDelete();
            }

            for (int i = 0; i < batchToDelete.Count; i++)
                batchToDelete[i].Delete();

            if (File.Exists(Config.TmpDeliveryClientsFile))
                File.Delete(Config.TmpDeliveryClientsFile);
        }

        static void LoadPendingLoads(string loadFile)
        {
            using (StreamReader reader = new StreamReader(loadFile))
            {
                string line;
                bool nextIsOrder = false;
                bool nextIsDetail = false;
                Order order = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "NewOrder")
                    {
                        nextIsDetail = false;
                        nextIsOrder = true;
                        if (order != null)
                            order.Save();
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

                        order = CreateLoadOrder(parts);
                        nextIsOrder = false;
                        continue;
                    }

                    if (nextIsDetail)
                    {
                        string[] parts = line.Split(new char[] { (char)20 });

                        CreateLoadDetails(parts, order);
                    }
                }

                if (order != null)
                    order.Save();
            }
        }

        static Order CreateLoadOrder(string[] currentrow)
        {
            try
            {
                // see if this order  already exists
                if (Order.Orders.FirstOrDefault(x => x.UniqueId == currentrow[21]) != null)
                {
                    Logger.CreateLog("CreateOrder had reference to an already existing order: UniqueId: " + currentrow[21] + " ID " + currentrow[0]);
                    foreach (var o in Order.Orders)
                        Logger.CreateLog("current order  UniqueId: " + o.UniqueId + " ID: " + o.OrderId);
                    return null;
                }

                var orderId = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                var client = Client.Find(clientId);
                if (client == null)
                {
                    Logger.CreateLog(" CreateOrder had reference to a non existing client: " + currentrow[1] + " client id: " + currentrow[1]);
                    client = Client.CreateTemporalClient(clientId);
                    client.SalesmanClient = true;
                }

                var order = new Order(client, false);
                order.OriginalOrderId = orderId;
                order.OriginalSalesmanId = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture); //original salesman Id
                order.Date = Convert.ToDateTime(currentrow[5], CultureInfo.InvariantCulture);
                order.OrderType = (OrderType)Convert.ToInt32(currentrow[6], CultureInfo.InvariantCulture);
                order.PrintedOrderId = currentrow[9];

                var batchId = Convert.ToInt32(currentrow[20], CultureInfo.InvariantCulture);
                order.Comments = currentrow[12];
                order.CompanyName = string.Empty;
                order.Latitude = 0;
                order.Longitude = 0;
                order.TaxRate = Convert.ToDouble(currentrow[14], CultureInfo.InvariantCulture);
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

                if (currentrow.Length > 37)
                    order.AssetId = Convert.ToInt32(currentrow[37]);

                order.PendingLoad = true;

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                    order.PrintedOrderId = orderId.ToString();

                return order;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                //Xamarin.Insights.Report(e);
                Logger.CreateLog(Concatenate(currentrow));
                return null;
            }
        }

        static void CreateLoadDetails(string[] currentrow, Order order)
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
                var detail = new OrderDetail(product, 0, order);
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

                detail.Substracted = false;
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
                    {
                        var inactive = UnitOfMeasure.InactiveUoM.FirstOrDefault(x => x.Id == uomid);
                        if (inactive != null)
                        {
                            var sameUOM = product.UnitOfMeasures.FirstOrDefault(x => x.Conversion == inactive.Conversion);
                            if (sameUOM != null)
                                detail.UnitOfMeasure = sameUOM;
                            else
                                detail.UnitOfMeasure = inactive;
                        }
                        else
                            detail.UnitOfMeasure = null;
                    }
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

                order.AddDetail(detail);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        private static bool LoadPendingDeliveries(string file)
        {
            bool onlyQuotes = false;
            try
            {

                Dictionary<int, Batch> createdBatches = new Dictionary<int, Batch>();
                Dictionary<int, Order> createdOrders = new Dictionary<int, Order>();

                if (File.Exists(Config.TmpDeliveryClientsFile))
                    File.Delete(Config.TmpDeliveryClientsFile);

                using (StreamWriter writer = new StreamWriter(Config.TmpDeliveryClientsFile))
                {
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
                                case "ENDOFTABLE":
                                    currenttable = -1;
                                    continue;
                            }

                            string[] currentrow = currentline.Split(DataLineSplitter);

                            switch (currenttable)
                            {
                                case 1:
                                    CreateClient(currentrow, true, true);
                                    writer.WriteLine(currentline);
                                    continue;
                                case 2:
                                    CreateBatch(currentrow, createdBatches);
                                    continue;
                                case 3:
                                    CreateOrder(currentrow, createdBatches, createdOrders);
                                    continue;
                                case 4:
                                    CreateRouteEX(currentrow);
                                    continue;
                                case 5:
                                    CreateOrderDetails(currentrow, createdBatches, createdOrders);
                                    continue;
                            }
                        }
                    }
                }

                foreach (var order in createdOrders.Values)
                {
                    if (order.IsDelivery && order.OrderType == OrderType.Consignment && !Config.ParInConsignment)
                        order.ConvertConsignmentPar();

                    order.PendingLoad = true;

                    if (order.OrderType == OrderType.Quote)
                        order.PendingLoad = false;

                    order.Save();
                }

                if (createdOrders.Count() > 0)
                    onlyQuotes = createdOrders.Values.All(x => x.OrderType == OrderType.Quote);

                // delete any bathc that contains no order
                List<Batch> deleted = new List<Batch>();
                foreach (var batch in createdBatches.Values)
                    if (createdOrders.Values.FirstOrDefault(x => x.BatchId == batch.Id) != null)
                        batch.Save();
                    else
                        deleted.Add(batch);

                foreach (var batch in deleted)
                    batch.Delete();

                RouteEx.Save();

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

                return onlyQuotes;
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

                return false;
            }
        }

        public static void AcceptLoadOrders(Order order, string valuesChanged)
        {
            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                NetAccess.GetCommunicatorVersion();

                Logger.CreateLog("accepting load order");
                using (NetAccess netaccess = new NetAccess())
                {
                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("AcceptLoadOrderCommand");
                    netaccess.WriteStringToNetwork(order.OriginalOrderId.ToString(CultureInfo.InvariantCulture));

                    if (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "23.2.0.0") || (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "14.6.0.0") && !CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "20.0.0.0")))
                    {
                        var ack = netaccess.ReadStringFromNetwork();
                        if (ack != "got it")
                            throw new Exception("Error accepting the load order. Ack=" + ack);
                    }

                    Logger.CreateLog("load order accepted");

                    if (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "22.0.0.0"))
                        netaccess.WriteStringToNetwork(valuesChanged);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw new Exception("Error accepting the load order");
            }
            finally
            {
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time accepting load: " + ts.TotalSeconds);
        }

        public static bool CheckCommunicatorVersion(string current, string min)
        {
            try
            {
                if (string.IsNullOrEmpty(current))
                    return false;


                if (!float.TryParse(current, out _))
                {
                    throw new ArgumentException($"Invalid version format: {current}");
                }

                var c_parts = current.Split('.');
                var m_parts = min.Split('.');

                int minLength = c_parts.Length > m_parts.Length ? m_parts.Length : c_parts.Length;

                for (int i = 0; i < minLength; i++)
                {
                    if (int.Parse(c_parts[i]) > int.Parse(m_parts[i]))
                        return true;
                    else if (int.Parse(c_parts[i]) < int.Parse(m_parts[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void AcceptLoadOrders(List<int> ids, string valuesChanged)
        {
            if (ids.Count == 0)
                return;

            string s = "";
            foreach (var item in ids)
            {
                if (!string.IsNullOrEmpty(s))
                    s += ",";
                s += item;
            }

            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                NetAccess.GetCommunicatorVersion();

                Logger.CreateLog("accepting load order");
                using (NetAccess netaccess = new NetAccess())
                {
                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("AcceptLoadOrderListCommand");
                    netaccess.WriteStringToNetwork(s);

                    if (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "23.2.0.0") || (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "14.6.0.0") && !CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "20.0.0.0")))
                    {
                        var ack = netaccess.ReadStringFromNetwork();
                        if (ack != "got it")
                            throw new Exception("Error accepting the load order. Ack=" + ack);
                    }

                    Logger.CreateLog("load orders accepted");

                    if (CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "22.0.0.0"))
                        netaccess.WriteStringToNetwork(valuesChanged);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
            }
            finally
            {
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time accepting load: " + ts.TotalSeconds);
        }

        public static void SendDailyParLevel(bool delete = true)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    NetAccess.SendClientDailyParLevel();

                    if (delete)
                    {
                        ClientDailyParLevel.Clear();

                        if (File.Exists(Config.DailyParLevelFile))
                            File.Delete(Config.DailyParLevelFile);

                        if (File.Exists(Config.SavedDailyParLevelFile))
                            File.Delete(Config.SavedDailyParLevelFile);
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);

                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void LoadParLevelHistory()
        {
            try
            {
                ParLevelHistory.Histories.Clear();

                if (File.Exists(Config.ParLevelHistoryFile))
                {
                    using (StreamReader reader = new StreamReader(Config.ParLevelHistoryFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(DataLineSplitter);
                            CreateClientDailyParLevelHistory(parts);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void GetUserSettingLine()
        {
            Logger.CreateLog("downloading user settings line");

            using (NetAccess netaccess = new NetAccess())
            {
                try
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

                    netaccess.WriteStringToNetwork("ConfigSettings");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                    string configSettings = netaccess.ReadStringFromNetwork();

                    if (configSettings.StartsWith("invalid auth info") || configSettings.StartsWith("device authorization denied"))
                    {
                        try
                        {
                            netaccess.OpenConnection();
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                            return;
                        }

                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("ConfigSettings");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                        configSettings = netaccess.ReadStringFromNetwork();
                    }

                    if (!string.IsNullOrEmpty(configSettings))
                        Config.SetConfigSettings(configSettings, false);

                    netaccess.CloseConnection();
                }
                catch (Exception ex)
                {
                    Logger.CreateLog("Error getting ConfigSettings. ");
                    Logger.CreateLog(ex);

                    throw;
                }
            }
        }

        public static void GetSalesmanSettings(bool fromDownload = true)
        {
            Logger.CreateLog("downloading salesman settings");

            var currentPrefix = Config.InvoicePrefix;
            var currentId = Config.LastPrintedId;
            var currentPrePrefix = Config.InvoicePresalePrefix;
            var currentPreId = Config.LastPresalePrintedId;
            var currentSeqPrefix = Config.SalesmanSeqPrefix;

            using (NetAccess netaccess = new NetAccess())
            {
                try
                {
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

                    netaccess.WriteStringToNetwork("ConfigSettings");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                    string configSettings = netaccess.ReadStringFromNetwork();
                    if (!string.IsNullOrEmpty(configSettings))
                        Config.SetConfigSettings(configSettings, fromDownload);

                    CompanyInfo.SelectedCompany = null;

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                    // at this point we knows the device is auth.
                    Config.AuthorizationFailed = false;

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());
                    Logger.CreateLog("start downloading logo");
                    netaccess.WriteStringToNetwork("CompanyLogoCommand");
                    configSettings = netaccess.ReadStringFromNetwork();
                    if (configSettings.ToLowerInvariant() == "logo")
                    {
                        string logoFile = Config.LogoStorePath;

                        if (File.Exists(logoFile))
                            File.Delete(logoFile);

                        int i = netaccess.ReceiveFile(logoFile);
                        Logger.CreateLog("logo received");

                        if (i > 0)
                        {
                             using (var reader = new FileStream(logoFile, FileMode.Open, FileAccess.Read))
                            {
                                const int MAX_W = 300;
                                const int MAX_H = 300;

                                var dOpt = new DecoderOptions
                                {
                                    TargetSize = new SixLabors.ImageSharp.Size(MAX_W, MAX_H),
                                    SkipMetadata = true,
                                    Sampler = KnownResamplers.NearestNeighbor
                                };

                                using var fs = File.OpenRead(logoFile);
                                using var img = SixLabors.ImageSharp.Image.Load<L8>(dOpt, fs);

                                int paddedW = (img.Width + 31) & ~31;
                                int paddedH = (img.Height + 31) & ~31;

                                img.Configuration.PreferContiguousImageBuffers = true;
                                if (!img.DangerousTryGetSinglePixelMemory(out Memory<L8> mem))
                                    throw new InvalidOperationException("Pixel memory not contiguous");

                                int bytesPerRow = paddedW / 8;
                                byte[] raw = new byte[paddedH * bytesPerRow];

                                Span<L8> src = mem.Span;
                                for (int y = 0; y < img.Height; y++)
                                {
                                    int srcRow = y * img.Width;
                                    int dstIdx = y * bytesPerRow;

                                    byte acc = 0;
                                    int bit = 7;
                                    for (int x = 0; x < img.Width; x++)
                                    {
                                        if (src[srcRow + x].PackedValue < 128)
                                            acc |= (byte)(1 << bit);

                                        if (--bit < 0)
                                        {
                                            raw[dstIdx++] = acc;
                                            acc = 0;
                                            bit = 7;
                                        }
                                    }

                                    if (bit != 7) raw[dstIdx] = acc;
                                }

                                Config.CompanyLogo = BitConverter.ToString(raw).Replace("-", "");
                                Config.CompanyLogoWidth = bytesPerRow;
                                Config.CompanyLogoHeight = paddedH;
                                Config.CompanyLogoSize = raw.Length;
                            }
                        }
                        else
                        {
                            Logger.CreateLog("no logo received");
                            // remove logo in case  you have one
                            Config.CompanyLogo = string.Empty;
                            Config.CompanyLogoHeight = 0;
                            Config.CompanyLogoSize = 0;
                            Config.CompanyLogoWidth = 0;
                        }
                    }
                    else
                    {
                        // remove logo in case  you have one
                        Config.CompanyLogo = string.Empty;
                        Config.CompanyLogoHeight = 0;
                        Config.CompanyLogoSize = 0;
                        Config.CompanyLogoWidth = 0;
                    }


                    Thread.Sleep(1000);

                    try
                    {
                        netaccess.WriteStringToNetwork("SalesmanSettingsCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString(CultureInfo.InvariantCulture));
                        string salesmanSettings = netaccess.ReadStringFromNetwork();
                        if (!string.IsNullOrEmpty(salesmanSettings))
                            Config.SetSalesmanSettings(salesmanSettings);
                        else
                        {
                            Config.SalesmanSeqValues = false;

                            Config.InvoicePrefix = currentPrefix;
                            Config.LastPrintedId = currentId;
                            Config.InvoicePresalePrefix = currentPrePrefix;
                            Config.LastPresalePrintedId = currentPreId;
                            Config.SalesmanSeqPrefix = currentSeqPrefix;
                        }
                        Thread.Sleep(1000);
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog("Error getting SalesmanSettingsCommand. Restore the old salesman settings");
                        Logger.CreateLog(ee);

                        Config.SalesmanSeqValues = false;

                        Config.InvoicePrefix = currentPrefix;
                        Config.LastPrintedId = currentId;
                        Config.InvoicePresalePrefix = currentPrePrefix;
                        Config.LastPresalePrintedId = currentPreId;
                        Config.SalesmanSeqPrefix = currentSeqPrefix;

                        throw;
                    }

                    netaccess.CloseConnection();

                    if (Config.SalesmanSeqValues)
                    {
                        var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                        if (salesman != null)
                        {
                            salesman.PrintedPrefix = Config.InvoicePrefix;
                            salesman.SequencePrefix = Config.SalesmanSeqPrefix;
                            salesman.SequenceExpirationDate = Config.SalesmanSeqExpirationDate;
                            salesman.SequenceFrom = Config.SalesmanSeqFrom;
                            salesman.SequenceTo = Config.SalesmanSeqTo;
                        }
                    }

                    // see if I have to respect the coming number of keep mine DSD
                    if (currentPrefix == Config.InvoicePrefix && currentSeqPrefix == Config.SalesmanSeqPrefix && currentId > Config.LastPrintedId)
                    {
                        Logger.CreateLog("DSD From the OS came the same prefix: " + currentPrefix + " and seq prefix: " + currentSeqPrefix + " but an id lower " + Config.LastPrintedId + " than mine:" + currentId + " I will ignore the server value");
                        Config.LastPrintedId = currentId;
                    }

                    // see if I have to respect the coming number of keep mine Presale
                    if (currentPrePrefix == Config.InvoicePresalePrefix && currentPreId > Config.LastPresalePrintedId)
                    {
                        Logger.CreateLog("Presale From the OS came the same prefix: " + currentPrePrefix + " but an id lower " + Config.LastPresalePrintedId + " than mine:" + currentPreId + " I will ignore the server value");
                        Config.LastPresalePrintedId = currentPreId;
                    }

                    Config.SaveSettings();
                }
                catch (Exception ee)
                {
                    Logger.CreateLog("Get SalesmanSettings Part");
                    Logger.CreateLog(ee);
                }
            }
        }

        public static void SendClientProdSort()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    NetAccess.SendClientProdSort(Config.ClientProdSortFile);

                    ClientProdSort.Clear();

                    if (File.Exists(Config.ClientProdSortFile))
                        File.Delete(Config.ClientProdSortFile);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);

                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void LoadClientDailyParLevel(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    ClientDailyParLevel.Clear();

                    using (StreamReader reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(DataLineSplitter);

                            CreateClientDailyParLevel(parts);
                        }
                    }

                    ClientDailyParLevel.LoadCreatedParLevels();
                }
                else
                    Logger.CreateLog("Temporal file for ClientDailyParLevel not found");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

        }

        public static void SendNewConsignment()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    NetAccess.SendNewConsignment(Config.ConsignmentParFile);

                    ConsignmentValues.Clear();

                    if (File.Exists(Config.ConsignmentParFile))
                        File.Delete(Config.ConsignmentParFile);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);

                    throw;
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void LoadNewDeliveryClients()
        {
            if (!File.Exists(Config.TmpDeliveryClientsFile))
                Logger.CreateLog("TmpDeliveryClientsFile FILE NOT FOUND");
            else
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Config.TmpDeliveryClientsFile))
                    {
                        string currentline;
                        while ((currentline = reader.ReadLine()) != null)
                        {
                            string[] currentrow = currentline.Split(DataLineSplitter);

                            CreateClient(currentrow, true, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog("Loading TmpDeliveryClientsFile " + ex);
                }
            }

            if (!File.Exists(Config.DeliveryNewClientsFile))
                Logger.CreateLog("DeliveryNewClientsFile FILE NOT FOUND");
            else
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Config.DeliveryNewClientsFile))
                    {
                        string currentline;
                        while ((currentline = reader.ReadLine()) != null)
                        {
                            string[] currentrow = currentline.Split(DataLineSplitter);

                            CreateClient(currentrow, true, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog("Loading DeliveryNewClientsFile " + ex);
                }
            }
        }

        public static void AddDeliveryClient(Client client)
        {
            string s = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}" +
                "{0}{15}{0}{16}{0}{17}{0}{18}{0}{19}{0}{20}{0}{21}{0}{22}{0}{23}{0}{24}{0}{25}",
                (char)20,                                   //0
                client.ClientId,                            //0
                client.ClientName,                          //1
                client.ShipToAddress,                       //2
                client.PriceLevel,                          //3
                client.Comment,                             //4
                client.ContactName,                         //5
                client.ContactPhone,                        //6
                "",                                         //7
                client.OriginalId,                          //8
                client.OpenBalance,                         //9
                client.CategoryId,                          //10
                client.OverCreditLimit ? "1" : "0",         //11
                client.ExtraPropertiesAsString,             //12
                client.Latitude,                            //13
                client.Longitude,                           //14
                client.TaxRate,                             //15
                client.NonvisibleExtraPropertiesAsString,   //16
                client.OneDoc ? "1" : "0",                  //17
                client.BillToAddress,                       //18
                client.LicenceNumber,                       //19
                client.VendorNumber,                        //20
                client.Notes,                               //21
                client.CreditLimit,                         //22
                client.UniqueId,                            //23
                client.RetailPriceLevelId                   //24
                );

            using (StreamWriter writer = new StreamWriter(Config.DeliveryNewClientsFile, true))
            {
                writer.WriteLine(s);
            }
        }

        public static void DeleteTransferFiles()
        {
            var transOn = Path.Combine(Config.DataPath, TransferAction.On.ToString() + "_temp_LoadOrderPath.xml");
            var transOff = Path.Combine(Config.DataPath, TransferAction.Off.ToString() + "_temp_LoadOrderPath.xml");

            if (File.Exists(transOn))
                File.Delete(transOn);

            if (File.Exists(transOff))
                File.Delete(transOff);
        }

        public static void CloseShipment()
        {
            if (Shipment.CurrentShipment == null)
                return;

            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            try
            {
                Logger.CreateLog("closing Shipment");
                using (NetAccess netaccess = new NetAccess())
                {
                    //open the connection
                    try
                    {
                        netaccess.OpenConnection();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                        sb.Append(string.Format("Error connecting to the server {0} : {1}", Config.ConnectionAddress, Config.Port));
                        throw;
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("CloseShipmentCommand");
                    netaccess.WriteStringToNetwork(Shipment.CurrentShipment.Id + "|" + Config.SessionId + "|" + DateTime.Now.Ticks.ToString());

                    Logger.CreateLog("Shipment Closed");

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
            finally
            {
            }

            TimeSpan ts = DateTime.Now.Subtract(start);
            Logger.CreateLog("Total time closing shipment: " + ts.TotalSeconds);
        }


        public static void FixConsignmentPar(Order order)
        {
            if (order.IsDelivery && !order.Reshipped)
                return;

            bool batteryValues = false;

            bool onlyCounted = false;

            foreach (var detail in order.Details)
            {
                detail.Order = order;

                var item = ConsStruct.GetStructFromDetail(detail);

                if (item.FromPar)
                {
                    item.Detail.Qty = item.Sold;
                    if (item.Detail.Qty == 0)
                    {
                        if (item.Return > 0)
                        {
                            item.Detail.Qty = item.Return;
                            item.Detail.IsCredit = true;
                            item.Detail.Damaged = false;
                        }
                        else if (item.Damaged > 0)
                        {
                            item.Detail.Qty = item.Damaged;
                            item.Detail.IsCredit = true;
                            item.Detail.Damaged = true;
                        }
                    }
                }
                else
                {
                    item.Detail.Qty = item.Picked;
                }

                item.Detail.Price = item.Price;

                var rotation = DataAccess.GetSingleUDF("rotatedQty", item.Detail.ExtraFields);
                var adjQty = DataAccess.GetSingleUDF("adjustedQty", item.Detail.ExtraFields);
                var core = DataAccess.GetSingleUDF("coreQty", item.Detail.ExtraFields);

                if (Config.IncludeRotationInDelivery)
                {
                    if (!string.IsNullOrEmpty(rotation))
                    {
                        var qty = int.Parse(rotation);
                        item.Detail.Qty += qty;
                    }

                    if (!string.IsNullOrEmpty(adjQty))
                    {
                        var ws = adjQty.Split(',');
                        item.Detail.Qty += ws.Length;
                    }
                }

                batteryValues |= (!string.IsNullOrEmpty(rotation) || !string.IsNullOrEmpty(adjQty) || (!string.IsNullOrEmpty(core) && core != "0"));
            }

            foreach (var det in order.Details.Where(x => x.ParLevelDetail))
            {
                var parId = DataAccess.GetSingleUDF("parid", det.ExtraFields);
                if (!string.IsNullOrEmpty(parId))
                {
                    var par = ClientDailyParLevel.List.FirstOrDefault(x => x.Id == Convert.ToInt32(parId));

                    CreateEditParLevel(order, par, det);
                }
                else
                    CreateEditParLevel(order, null, det);

                det.ExtraFields = DataAccess.SyncSingleUDF("frompar", "1", det.ExtraFields);
            }

            ClientDailyParLevel.Save();

            ConsignmentValues.LoadFromDetail(order);

            RemoveZeroLines(order);

            if (order.Details.Count == 0 && !batteryValues)
            {
                order.Voided = true;
                order.Finished = true;
                order.DiscountAmount = 0;
                order.DiscountComment = string.Empty;

                onlyCounted = true;
            }

            order.OrderType = OrderType.Order;
            order.ExtraFields = DataAccess.SyncSingleUDF("consignmentpar", "1", order.ExtraFields);

            if (onlyCounted)
                order.ExtraFields = DataAccess.SyncSingleUDF("countonly", "1", order.ExtraFields);

            order.Save();
        }

        static void CreateEditParLevel(Order order, ClientDailyParLevel par, OrderDetail det)
        {
            if (par == null)
                par = ClientDailyParLevel.GetNewParLevel(order.Client, det.Product, 0);

            var newvalue = Convert.ToSingle(DataAccess.GetSingleUDF("newvalue", det.ExtraFields));
            if (newvalue != par.NewQty)
                par.SetNewPar(newvalue);

            var counted = Convert.ToSingle(DataAccess.GetSingleUDF("count", det.ExtraFields));
            if (counted != par.Counted)
                par.SetCountedQty(counted);

            var sold = Convert.ToSingle(DataAccess.GetSingleUDF("sold", det.ExtraFields));
            if (sold != par.Sold)
                par.SetSoldQty(sold);

            var returns = Convert.ToSingle(DataAccess.GetSingleUDF("return", det.ExtraFields));
            if (returns != par.Return)
                par.SetReturnQty(returns);

            var dumps = Convert.ToSingle(DataAccess.GetSingleUDF("damaged", det.ExtraFields));
            if (dumps != par.Dump)
                par.SetDumpQty(dumps);
        }

        static void RemoveZeroLines(Order order)
        {
            string s = "";

            for (int i = 0; i < order.Details.Count; i++)
            {
                var item = order.Details[i];

                if (item.Qty > 0)
                    continue;

                if (s.Length > 0)
                    s += ';';

                string ss = item.Product.ProductId.ToString() + "," + item.ExtraFields.Replace('=', ':').Replace('|', ',');

                s += ss;

                order.Details.RemoveAt(i);
                i--;
            }

            if (!string.IsNullOrEmpty(s))
                order.ExtraFields = DataAccess.SyncSingleUDF("zerolines", s, order.ExtraFields);
        }

        public static void SendEmailSequenceNotification(string text)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SeqNotificationCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString() + "|" + text);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException ae)
                {
                    Logger.CreateLog(ae);
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                }
            }
        }

        public static void LoadOrderHistory()
        {
            if (!File.Exists(Config.OrderHistoryFile))
                return;

            OrderHistory.History.Clear();

            using (StreamReader reader = new StreamReader(Config.OrderHistoryFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(DataLineSplitter);

                    CreateOrderHistory(parts);
                }
            }
        }

        private static void CreateOrderHistory(string[] currentRow)
        {
            try
            {
                var line = new OrderHistory();
                line.ClientId = Convert.ToInt32(currentRow[1], CultureInfo.InvariantCulture);
                line.ProductId = Convert.ToInt32(currentRow[2], CultureInfo.InvariantCulture);
                line.When = new DateTime(Convert.ToInt64(currentRow[4], CultureInfo.InvariantCulture));

                line.Old_Qty = Convert.ToSingle(currentRow[6], CultureInfo.InvariantCulture);
                var uomId = Convert.ToInt32(currentRow[7], CultureInfo.InvariantCulture);
                line.Old_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                line.Old_Price = Convert.ToDouble(currentRow[8], CultureInfo.InvariantCulture);

                line.Dumps_Qty = Convert.ToSingle(currentRow[10], CultureInfo.InvariantCulture);
                uomId = Convert.ToInt32(currentRow[11], CultureInfo.InvariantCulture);
                line.Dumps_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                line.Dumps_Price = Convert.ToDouble(currentRow[12], CultureInfo.InvariantCulture);

                line.Returns_Qty = Convert.ToSingle(currentRow[13], CultureInfo.InvariantCulture);
                uomId = Convert.ToInt32(currentRow[14], CultureInfo.InvariantCulture);
                line.Returns_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                line.Returns_Price = Convert.ToDouble(currentRow[15], CultureInfo.InvariantCulture);

                line.Count_Qty = Convert.ToSingle(currentRow[16], CultureInfo.InvariantCulture);
                uomId = Convert.ToInt32(currentRow[17], CultureInfo.InvariantCulture);
                line.Count_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                line.Sold_Qty = Convert.ToSingle(currentRow[18], CultureInfo.InvariantCulture);
                uomId = Convert.ToInt32(currentRow[19], CultureInfo.InvariantCulture);
                line.Sold_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                line.Invoice_Qty = Convert.ToSingle(currentRow[20], CultureInfo.InvariantCulture);
                uomId = Convert.ToInt32(currentRow[21], CultureInfo.InvariantCulture);
                line.Invoice_UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                line.Invoice_Price = Convert.ToDouble(currentRow[22], CultureInfo.InvariantCulture);

                OrderHistory.History.Add(line);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public class T
        {
            public string orderUniqueId { get; set; }
            public int productId { get; set; }
        }

        private static string GetOrderHistoryFile(IEnumerable<Batch> source, List<string> ordersId)
        {

            Dictionary<T, float> TotalDumpsDic = new Dictionary<T, float>();
            Dictionary<T, float> TotalReturnsDic = new Dictionary<T, float>();
            Dictionary<T, float> TotalInvoicedDic = new Dictionary<T, float>();

            if (!Config.UseFullTemplate)
                return "";

            var file = Path.GetTempFileName();

            if (File.Exists(file))
                File.Delete(file);

            List<OrderHistory> history = new List<OrderHistory>();

            foreach (Batch batch in source)
            {
                var orders = Order.Orders.Where(x => x != null && x.BatchId == batch.Id);
                if (ordersId != null)
                    orders = orders.Where(x => ordersId.Contains(x.OrderId.ToString()));

                if (Config.CasaSanchezCustomization)
                {
                    foreach (var order in orders)
                    {
                        foreach (var detail in order.Details)
                        {
                            if (detail.IsCredit)
                            {
                                if (detail.Damaged)
                                {
                                    if (detail.Qty == 0)
                                        continue;

                                    var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                    if (!TotalDumpsDic.ContainsKey(key))
                                        TotalDumpsDic.Add(key, detail.Qty);
                                    else
                                        TotalDumpsDic[key] += detail.Qty;
                                }
                                else
                                {
                                    var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                    if (!TotalReturnsDic.ContainsKey(key))
                                        TotalReturnsDic.Add(key, detail.Qty);
                                    else
                                        TotalReturnsDic[key] += detail.Qty;
                                }
                            }
                            else
                            {
                                var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                if (!TotalInvoicedDic.ContainsKey(key))
                                    TotalInvoicedDic.Add(key, detail.Qty);
                                else
                                    TotalInvoicedDic[key] += detail.Qty;
                            }
                        }
                    }
                }

                foreach (var order in orders.Where(x => Config.OnlyPresale || !x.AsPresale))
                {
                    if (order == null)
                        continue;

                    if (order.Voided || order.Reshipped)
                        continue;

                    List<OrderDetail> toRemove = new List<OrderDetail>();

                    var lastHistory = OrderHistory.History.Where(x => x.ClientId == order.Client.ClientId).OrderByDescending(x => x.When).ToList();

                    foreach (var detail in order.Details)
                    {
                        if (detail == null)
                            continue;

                        //var h = history.FirstOrDefault(x => x.ClientId == order.Client.ClientId && x.ProductId == detail.Product.ProductId);
                        OrderHistory h = null;
                        if (Config.CasaSanchezCustomization)
                            h = history.FirstOrDefault(x => x.ClientId == order.Client.ClientId && x.ProductId == detail.Product.ProductId && x.OrderUniqueId == detail.Order.UniqueId);
                        else
                            h = history.FirstOrDefault(x => x.ClientId == order.Client.ClientId && x.ProductId == detail.Product.ProductId);

                        if (h == null && !string.IsNullOrEmpty(detail.Order.RelationUniqueId))
                            h = history.FirstOrDefault(x => x.ClientId == order.Client.ClientId && x.ProductId == detail.Product.ProductId && x.RelationUniqueId == detail.Order.RelationUniqueId);

                        if (h == null)
                        {
                            h = new OrderHistory()
                            {
                                ClientId = order.Client.ClientId,
                                ClientUniqueId = string.IsNullOrEmpty(order.Client.UniqueId) ? "" : order.Client.UniqueId,
                                ProductId = detail.Product.ProductId,
                                OrderUniqueId = order.UniqueId,
                                RelationUniqueId = order.RelationUniqueId,
                                When = order.Date
                            };

                            var last = lastHistory.FirstOrDefault(x => x.ProductId == detail.Product.ProductId);
                            if (last != null)
                            {
                                h.Old_Qty = last.New_Qty;
                                h.Old_UoM = last.New_UoM;
                                h.Old_Price = last.New_Price;
                            }

                            history.Add(h);
                        }

                        if (detail.IsCredit)
                        {
                            if (detail.Damaged)
                            {
                                if (detail.Qty == 0)
                                {
                                    var x = GetSingleUDF("countedQty", detail.ExtraFields);
                                    if (!string.IsNullOrEmpty(x))
                                    {
                                        h.Count_Qty = Convert.ToSingle(x);
                                        h.WasCounted = true;
                                    }
                                    h.Count_UoM = detail.UnitOfMeasure;

                                    toRemove.Add(detail);
                                }
                                else
                                {
                                    h.Dumps_Qty = detail.Qty;
                                    h.Dumps_UoM = detail.UnitOfMeasure;
                                    h.Dumps_Price = detail.Price;
                                    h.Dumps_DetailUniqueId = detail.OriginalId;

                                    if (Config.CasaSanchezCustomization)
                                    {
                                        float totalDumps = float.MinValue;
                                        var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                        TotalDumpsDic.TryGetValue(key, out totalDumps);
                                        if (totalDumps != 0)
                                            h.Dumps_Qty = totalDumps;

                                        if (!h.WasCounted && h.Count_Qty == 0)
                                            h.Count_Qty = -1;
                                    }
                                }
                            }
                            else
                            {
                                h.Returns_Qty = detail.Qty;
                                h.Returns_UoM = detail.UnitOfMeasure;
                                h.Returns_Price = detail.Price;
                                h.Returns_DetailUniqueId = detail.OriginalId;

                                if (Config.CasaSanchezCustomization)
                                {
                                    float totalReturns = float.MinValue;
                                    var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                    TotalReturnsDic.TryGetValue(key, out totalReturns);
                                    if (totalReturns != 0)
                                        h.Returns_Qty = totalReturns;

                                    if (!h.WasCounted && h.Count_Qty == 0)
                                        h.Count_Qty = -1;
                                }
                            }
                        }
                        else
                        {
                            h.Invoice_Qty = detail.Qty;
                            h.Invoice_UoM = detail.UnitOfMeasure;
                            h.Invoice_Price = detail.Price;
                            h.Invoice_DetailUniqueId = detail.OriginalId;

                            if (Config.CasaSanchezCustomization)
                            {
                                float totalInvoiced = float.MinValue;

                                var key = new T { productId = detail.Product.ProductId, orderUniqueId = order.UniqueId };

                                TotalInvoicedDic.TryGetValue(key, out totalInvoiced);
                                if (totalInvoiced != 0)
                                    h.Invoice_Qty = totalInvoiced;

                                if (!h.WasCounted && h.Count_Qty == 0)
                                    h.Count_Qty = -1;
                            }
                        }
                    }

                    if (toRemove.Count == order.Details.Count)
                    {
                        order.ExtraFields = DataAccess.SyncSingleUDF("fullTemplateOnlyCount", "1", order.ExtraFields);
                        order.Save();
                    }
                }
            }

            if (history.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(file, true))
                {
                    foreach (var item in history)
                    {
                        /*0*/
                        writer.Write(item.ClientId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*1*/
                        writer.Write(item.ProductId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*2*/
                        writer.Write(item.ClientUniqueId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*3*/
                        writer.Write(item.When.Ticks.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*4*/
                        writer.Write(item.OrderUniqueId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*5*/
                        writer.Write(item.Old_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*6*/
                        writer.Write(item.Old_UoM != null ? item.Old_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);
                        /*7*/
                        writer.Write(item.Old_Price.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*8*/
                        writer.Write(item.Old_OfferType.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*9*/
                        writer.Write(item.Dumps_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*10*/
                        writer.Write(item.Dumps_UoM != null ? item.Dumps_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);
                        /*11*/
                        writer.Write(item.Dumps_Price.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*12*/
                        writer.Write(item.Returns_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*13*/
                        writer.Write(item.Returns_UoM != null ? item.Returns_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);
                        /*14*/
                        writer.Write(item.Returns_Price.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*15*/
                        writer.Write(item.Count_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*16*/
                        writer.Write(item.Count_UoM != null ? item.Count_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);

                        var old = item.Old_Qty;
                        if (item.Old_UoM != null)
                            old *= item.Old_UoM.Conversion;

                        var dumps = item.Dumps_Qty;
                        if (item.Dumps_UoM != null)
                            dumps *= item.Dumps_UoM.Conversion;

                        var returns = item.Returns_Qty;
                        if (item.Returns_UoM != null)
                            returns *= item.Returns_UoM.Conversion;

                        var counted = item.Count_Qty;
                        if (item.Count_UoM != null)
                            counted *= item.Count_UoM.Conversion;

                        var countedForMath = counted;
                        if (countedForMath == -1)
                            countedForMath = 0;
                        var sold = old - dumps - returns - countedForMath;
                        item.Sold_UoM = item.Old_UoM;
                        if (item.Sold_UoM != null)
                            sold /= item.Sold_UoM.Conversion;

                        item.Sold_Qty = sold > 0 ? sold : 0;

                        /*17*/
                        writer.Write(item.Sold_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*18*/
                        writer.Write(item.Sold_UoM != null ? item.Sold_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);

                        /*19*/
                        writer.Write(item.Invoice_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*20*/
                        writer.Write(item.Invoice_UoM != null ? item.Invoice_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);
                        /*21*/
                        writer.Write(item.Invoice_Price.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*22*/
                        writer.Write(item.Invoice_OfferType.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*23*/
                        writer.Write(item.New_Qty.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        /*24*/
                        writer.Write(item.New_UoM != null ? item.New_UoM.Id.ToString(CultureInfo.InvariantCulture) : "0");
                        writer.Write((char)20);
                        /*25*/
                        writer.Write(item.New_Price.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);

                        /*26*/
                        writer.Write(!string.IsNullOrEmpty(item.ExtraFields) ? item.ExtraFields.ToString(CultureInfo.InvariantCulture) : "");
                        writer.Write((char)20);

                        /*27*/
                        writer.Write(item.Dumps_DetailUniqueId ?? "");
                        writer.Write((char)20);

                        /*28*/
                        writer.Write(item.Returns_DetailUniqueId ?? "");
                        writer.Write((char)20);

                        /*29*/
                        writer.Write(item.Invoice_DetailUniqueId ?? "");

                        writer.WriteLine();
                    }
                }
            }

            return file;
        }

        public static void LoadProjectionValues()
        {
            Projection.List.Clear();

            if (!File.Exists(Config.ProjectionFile))
                return;

            using (StreamReader reader = new StreamReader(Config.ProjectionFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(DataLineSplitter);

                    CreateProjectionValue(parts);
                }
            }
        }

        private static void CreateProjectionValue(string[] currentRow)
        {
            try
            {
                var line = new Projection();

                line.ClientId = Convert.ToInt32(currentRow[1], CultureInfo.InvariantCulture);
                line.ProductId = Convert.ToInt32(currentRow[2], CultureInfo.InvariantCulture);
                line.Sunday = Convert.ToDouble(currentRow[3], CultureInfo.InvariantCulture);
                line.Monday = Convert.ToDouble(currentRow[4], CultureInfo.InvariantCulture);
                line.Tuesday = Convert.ToDouble(currentRow[5], CultureInfo.InvariantCulture);
                line.Wednesday = Convert.ToDouble(currentRow[6], CultureInfo.InvariantCulture);
                line.Thursday = Convert.ToDouble(currentRow[7], CultureInfo.InvariantCulture);
                line.Friday = Convert.ToDouble(currentRow[8], CultureInfo.InvariantCulture);
                line.Saturday = Convert.ToDouble(currentRow[9], CultureInfo.InvariantCulture);

                Projection.List.Add(line);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static bool RequestHelp(string name, string phone, string email)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("RequestHelpCommand");
                    access.WriteStringToNetwork(name + "|" + phone + "|" + email);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                    return false;
                }
            }

            return true;
        }

        public static void LoadClientDepartsFile(string tempFile)
        {
            if (!File.Exists(tempFile))
                return;

            var updated = ClientDepartment.Departments.Where(x => x.Updated).ToList();

            ClientDepartment.Clear();

            using (StreamReader reader = new StreamReader(tempFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var dep = ClientDepartment.CreateDepartment(line);

                    var d = updated.FirstOrDefault(x => x.UniqueId == dep.UniqueId);
                    if (d != null)
                    {
                        dep.Name = d.Name;
                        dep.ExtraFields = d.ExtraFields;
                        dep.Updated = true;

                        updated.Remove(d);
                    }

                    ClientDepartment.Departments.Add(dep);
                }
            }

            ClientDepartment.Departments.AddRange(updated);

            ClientDepartment.Save();
        }

        public static void SendTransfers()
        {
            try
            {
                if (File.Exists(Config.TransferOnFile))
                {
                    Logger.CreateLog("Sending Transfer On");
                    SendTransfer(Config.TransferOnFile);
                }

                if (File.Exists(Config.TransferOffFile))
                {
                    Logger.CreateLog("Sending Transfer Off");
                    SendTransfer(Config.TransferOffFile);
                }
            }
            catch
            {
                throw;
            }
        }

        public static void SendButlerTransfers()
        {
            try
            {
                if (Directory.Exists(Config.ButlerTransfersOn))
                {
                    DirectoryInfo dir = new DirectoryInfo(Config.ButlerTransfersOn);
                    foreach (var file in dir.GetFiles())
                    {
                        try
                        {
                            SendTransfer(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex.ToString());
                        }
                        finally
                        {
                            if (File.Exists(file.FullName))
                                File.Delete(file.FullName);
                        }
                    }
                }

                if (Directory.Exists(Config.ButlerTransfersOff))
                {
                    DirectoryInfo dir = new DirectoryInfo(Config.ButlerTransfersOff);
                    foreach (var file in dir.GetFiles())
                    {
                        try
                        {
                            SendTransfer(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex.ToString());
                        }
                        finally
                        {
                            if (File.Exists(file.FullName))
                                File.Delete(file.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SendTransfer(string transferFile)
        {
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

                    if (CheckCommunicatorVersion(CommunicatorVersion, "29.0.0.0"))
                    {
                        netaccess.WriteStringToNetwork("GetTransferForDSDCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                        netaccess.SendFile(transferFile);
                    }
                    else
                    {
                        netaccess.WriteStringToNetwork("ReceiveTransferCommand");
                        netaccess.SendFile(transferFile);
                    }

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Transfers sent");

                    File.Delete(transferFile);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static void SendSalesmanDeviceInfo()
        {
            try
            {
                NetAccess.GetCommunicatorVersion();

                if (!CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "24.0.0.0"))
                {
                    Logger.CreateLog("Communicator to old to send SalesmanDeviceInfo");
                    return;
                }

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
                    netaccess.WriteStringToNetwork("SalesmanDeviceCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId + "|" + Config.GetDeviceInfo());

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Salesman Device Info sent");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void GetExcelFile(string source, string destination)
        {
            try
            {
                Logger.CreateLog("GetExcelFile");

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
                    netaccess.WriteStringToNetwork("GetExcelFileCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                    netaccess.SendFile(source);

                    netaccess.ReceiveFile(destination);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Got Excel file");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static void SendSelfServiceInvitation(int clientId, string name, string email, string phone)
        {
            string s = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                ServerHelper.GetServerNumber(Config.IPAddressGateway),                    //0 server
                Config.Port,                                //1 port
                0,                                          //2 salesmanId for invitation
                1,                                          //3 self service
                clientId,                                   //4 client ID 
                name,                                       //5 name
                email,                                      //6 email
                phone,                                      //7 phone
                Config.SelfServiceInvitation == 2,          //8 approved
                Config.SalesmanId);                         //9 current salesman

            try
            {
                Logger.CreateLog("SendSelfServiceInvitation");

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
                    netaccess.WriteStringToNetwork("GetDeepLinkCommand");
                    netaccess.WriteStringToNetwork(s);

                    var result = netaccess.ReadStringFromNetwork();

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Got Excel file");

                    if (result != "done")
                        throw new Exception("Error sending invitation");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static void CreateDiscountCategory(string[] currentRow)
        {
            try
            {
                var line = new DiscountCategory();
                DiscountCategory.AddCategory(line);
                line.Id = Convert.ToInt32(currentRow[0], CultureInfo.InvariantCulture);
                line.Name = currentRow[1];
                line.ExtraFields = currentRow[2];
                line.IsPriceCategory = Convert.ToInt32(currentRow[3], CultureInfo.InvariantCulture) > 0;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void CreateClientOfferEx(string[] currentRow)
        {
            try
            {
                var line = new ClientOfferEx();
                ClientOfferEx.Add(line);
                line.OfferExId = Convert.ToInt32(currentRow[0], CultureInfo.InvariantCulture);
                line.ClientId = Convert.ToInt32(currentRow[1], CultureInfo.InvariantCulture);
                if (currentRow.Length > 2)
                    line.ExtraFields = currentRow[2];
                else
                    line.ExtraFields = string.Empty;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void CreateProductOfferEx(string[] currentRow)
        {
            try
            {
                var line = new ProductOfferEx();
                ProductOfferEx.Add(line);
                line.OfferExId = Convert.ToInt32(currentRow[0], CultureInfo.InvariantCulture);
                line.ProductId = Convert.ToInt32(currentRow[1], CultureInfo.InvariantCulture);
                line.BreakQty = Convert.ToDouble(currentRow[2], CultureInfo.InvariantCulture);
                line.Price = Convert.ToDouble(currentRow[3], CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void CreateOfferEx(string[] currentrow)
        {
            OfferEx newItem = new OfferEx();
            OfferEx.Add(newItem);
            newItem.Id = Convert.ToInt32(currentrow[0]);
            newItem.Name = currentrow[1];
            newItem.FromDate = Convert.ToDateTime(currentrow[2]);
            newItem.ToDate = Convert.ToDateTime(currentrow[3]);
            newItem.OfferType = Convert.ToInt32(currentrow[4]);
            if (currentrow[5] != "")
                newItem.Price = Convert.ToDouble(currentrow[5]);
            if (currentrow[6] != "")
                newItem.ProductId = Convert.ToInt32(currentrow[6]);
            if (currentrow[7] != "")
                newItem.TriggerQty = Convert.ToDouble(currentrow[7]);
            if (currentrow[8] != "")
                newItem.DiscountedQty = Convert.ToDouble(currentrow[8]);
            if (currentrow[9] != "")
                newItem.DiscountedPrice = Convert.ToDouble(currentrow[9]);
            if (currentrow[10] != "")
                newItem.TriggerUnitOfMeasureId = Convert.ToInt32(currentrow[10]);
            if (currentrow[11] != "")
                newItem.DiscountedUnitOfMeasureId = Convert.ToInt32(currentrow[11]);
            if (currentrow[12] != "")
                newItem.DiscountedProductId = Convert.ToInt32(currentrow[12]);
            if (currentrow[13] != "")
                newItem.BreaksAsString = currentrow[13];
            if (currentrow[14] != "")
                newItem.OriginGroup = Convert.ToInt32(currentrow[14]);
            if (currentrow[15] != "")
                newItem.OriginProductCategory = Convert.ToInt32(currentrow[15]);
            newItem.Primary = Convert.ToInt32(currentrow[16]) > 0;
            newItem.Recurrent = Convert.ToInt32(currentrow[17]) > 0;
            if (currentrow.Length > 18)
                newItem.ExtraFields = currentrow[18];
            else
                newItem.ExtraFields = string.Empty;
            if (currentrow.Length > 19)
                newItem.DateUsed = Convert.ToInt32(currentrow[19]);
            else
                newItem.DateUsed = 0;
        }

        public static bool CheckSyncAuthInfo()
        {
            try
            {
                NetAccess.GetCommunicatorVersion();

                if (!CheckCommunicatorVersion(CommunicatorVersion, "29.92.0.0"))
                {
                    Logger.CreateLog("Communicator to old to send SalesmanSyncAuthCommand");

                    return LastEndOfDay == DateTime.MinValue || DateTime.Now.Subtract(LastEndOfDay).Minutes >= 1;
                }

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
                    netaccess.WriteStringToNetwork("SalesmanSyncAuthCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());

                    var result = netaccess.ReadStringFromNetwork();

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (string.IsNullOrEmpty(result))
                        throw new Exception("Network connection error. Please try again later.");

                    return result == "1";
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static void SendAssetTracking()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                if (File.Exists(Config.AssetTrackingFile))
                    try
                    {
                        //FileOperationsLocker.InUse = true;

                        NetAccess.SendAssetTracking();

                        AssetTracking.Clear();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);

                        throw;
                    }
                    finally
                    {
                        //FileOperationsLocker.InUse = false;
                    }
            }
        }

        public static void SendVehicleInformation()
        {
            var endingV = VehicleInformation.EODVehicleInformation;
            var startingV = VehicleInformation.CurrentVehicleInformation;

            if (endingV == null || startingV == null)
                return;

            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SendVehicleInformationCommand");

                    var info = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
                        Config.SalesmanId,
                        startingV.SessionId ?? "",
                        startingV.PlateNumber ?? "",
                        startingV.Assistant ?? "",
                        startingV.SeatBelts ?? "",
                        startingV.MilesFromDeparture.ToString(),
                        endingV.MilesFromDeparture.ToString(),
                        startingV.Gas ?? "",
                        endingV.Gas ?? "",
                        startingV.TireCondition ?? "",
                        endingV.TireCondition ?? "",
                        startingV.EngineOil ? "yes" : "no",
                        endingV.EngineOil ? "yes" : "no",
                        startingV.BrakeFluid ? "yes" : "no",
                        endingV.BrakeFluid ? "yes" : "no",
                        startingV.PowerSteeringFluid ? "yes" : "no",
                        endingV.PowerSteeringFluid ? "yes" : "no",
                        startingV.TransmissionFluid ? "yes" : "no",
                        endingV.TransmissionFluid ? "yes" : "no",
                        startingV.AntifreezeCoolant ? "yes" : "no",
                        endingV.AntifreezeCoolant ? "yes" : "no"

                        );

                    access.WriteStringToNetwork(info);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns true if the password is correct
        /// </summary>
        /// <param name="providedPassword">The password provded by the user in the UI</param>
        /// <param name="savedPassword">The password stored in the database or class</param>
        /// <returns>Returns true if the password is correct</returns>
        public static bool IsCorrectPassword(string providedPassword, string savedPassword)
        {
            if (providedPassword == savedPassword)
                return true;
            var encrytped = EncryptString(providedPassword);
            if (encrytped == savedPassword)
                return true;
            var decrupted = DecryptString(savedPassword);
            if (decrupted == providedPassword)
                return true;
            return false;
        }

        public static string EncryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = Encoding.UTF8.GetBytes(inputString);
                var provider = new System.Security.Cryptography.DESCryptoServiceProvider();
                memStream = new MemoryStream();
                var transform = provider.CreateEncryptor(key, IV);
                var cryptoStream = new System.Security.Cryptography.CryptoStream(memStream, transform, System.Security.Cryptography.CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
            return Convert.ToBase64String(memStream.ToArray());
        }

        public static string DecryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = new byte[inputString.Length];
                byteInput = Convert.FromBase64String(inputString);
                var provider = new System.Security.Cryptography.DESCryptoServiceProvider();
                memStream = new MemoryStream();
                var transform = provider.CreateDecryptor(key, IV);
                var cryptoStream = new System.Security.Cryptography.CryptoStream(memStream, transform, System.Security.Cryptography.CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }

            Encoding encoding1 = Encoding.UTF8;
            return encoding1.GetString(memStream.ToArray());
        }

        public static void EncryptFile(string filePath)
        {
            try
            {
                string inputString = File.ReadAllText(filePath);
                string encryptedString = EncryptString(inputString);
                File.WriteAllText($"{filePath}.Encrypted", encryptedString);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void DecryptFile(string filePath)
        {
            try
            {
                string encryptedString = File.ReadAllText($"{filePath}.Encrypted");
                string decryptedString = DecryptString(encryptedString);
                File.WriteAllText($"{filePath}.Decrypted", decryptedString);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void EncryptFileByLine(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    using (StreamWriter writer = new StreamWriter($"{filePath}.Encrypted"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string encryptedLine = EncryptString(line);
                            writer.WriteLine(encryptedLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void DecryptFileByLine(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader($"{filePath}.Encrypted"))
                {
                    using (StreamWriter writer = new StreamWriter($"{filePath}.Decrypted"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string decryptedLine = DecryptString(line);
                            writer.WriteLine(decryptedLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }


        public static void GetExternalInvoiceSignature(Invoice invoice)
        {
            try
            {
                Logger.CreateLog("GetExternalInvoiceSignature");

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
                    netaccess.WriteStringToNetwork("ExternalInvoiceSignatureCommand");
                    netaccess.WriteStringToNetwork(invoice.ClientId + "," + invoice.InvoiceNumber);

                    var reply = netaccess.ReadStringFromNetwork();
                    if (reply != "nosignature")
                    {
                        var tempFile = Path.GetTempFileName() + ".png";
                        netaccess.ReceiveFile(tempFile);

                        using (var reader = new FileStream(tempFile, FileMode.Open))
                        {
                            var converter = new BitmapConvertor();
                            // var path = System.IO.Path.GetTempFileName () + ".bmp";
                            using SKBitmap signature = SKBitmap.Decode(tempFile);

                            var rawBytes = converter.convertBitmap(signature);
                            //int bitmapDataOffset = 62;
                            double widthInBytes = ((signature.Width / 32) * 32) / 8;
                            int height = signature.Height / 32 * 32;
                            var bitmapDataLength = rawBytes.Length;

                            string ZPLImageDataString = BitConverter.ToString(rawBytes);
                            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);
                            // Convert the image to a PNG format and store it as Base64

                            using MemoryStream stream = new MemoryStream();
                            signature.Encode(stream, SKEncodedImageFormat.Png, 100); // Encode as PNG
                            byte[] byteArray = stream.ToArray();

                            // Convert the byte array to Base64 string
                            invoice.SignatureAsBase64 = Convert.ToBase64String(byteArray);
                            invoice.Signature = ZPLImageDataString;
                            invoice.SignatureWidth = widthInBytes;
                            invoice.SignatureHeight = height;
                            invoice.SignatureSize = rawBytes.Length;
                        }
                    }
                    else
                        invoice.Signature = "";

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Done ExternalInvoiceSignatureCommand");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }


        public static void GetDeliverySignature(Order order)
        {
            try
            {
                Logger.CreateLog("GetExternalInvoiceSignature");

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
                    netaccess.WriteStringToNetwork("GetDeliverySignatureCommand");
                    netaccess.WriteStringToNetwork(order.Client.ClientId + "," + order.UniqueId);

                    var reply = netaccess.ReadStringFromNetwork();
                    if (reply != "nosignature")
                    {
                        var tempFile = Path.GetTempFileName() + ".png";
                        netaccess.ReceiveFile(tempFile);

                        using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(tempFile))
                        {
                            var signaturePoints = ExtractSignaturePointsFromBitmap(image);

                            if (signaturePoints == null || signaturePoints.Count == 0)
                            {
                                Logger.CreateLog("No signature points found in bitmap");
                                return;
                            }

                            // Process points similar to your original code
                            ProcessAndSaveSignaturePoints(order, signaturePoints, reply);
                        }
                    }

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Done ExternalInvoiceSignatureCommand");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        #region process order sign

        private static List<SixLabors.ImageSharp.Point> ExtractSignaturePointsFromBitmap(Image<Rgba32> image)
        {
            var signaturePoints = new List<SixLabors.ImageSharp.Point>();

            if (image == null) return signaturePoints;

            int width = image.Width;
            int height = image.Height;

            // Create a boolean array to track visited pixels
            bool[,] visited = new bool[width, height];

            // Find signature pixels (assuming dark pixels on light background)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Check if pixel is part of signature (dark pixel)
                    if (IsSignaturePixel(image[x, y]) && !visited[x, y])
                    {
                        // Trace the stroke from this point
                        var strokePoints = TraceStroke(image, width, height, x, y, visited);
                        signaturePoints.AddRange(strokePoints);

                        // Add empty point to separate strokes (Point.Empty equivalent)
                        signaturePoints.Add(new SixLabors.ImageSharp.Point(-1, -1)); // Using -1,-1 as empty marker
                    }
                }
            }

            return signaturePoints;
        }

        private static bool IsSignaturePixel(Rgba32 pixel)
        {
            // Extract RGB values
            int alpha = pixel.A;
            int red = pixel.R;
            int green = pixel.G;
            int blue = pixel.B;

            // Consider pixel as signature if it's dark enough and not transparent
            int brightness = (red + green + blue) / 3;
            return alpha > 128 && brightness < 200; // Adjust threshold as needed
        }

        private static List<SixLabors.ImageSharp.Point> TraceStroke(Image<Rgba32> image, int width, int height,
            int startX, int startY, bool[,] visited)
        {
            var strokePoints = new List<SixLabors.ImageSharp.Point>();
            var toProcess = new Queue<SixLabors.ImageSharp.Point>();

            toProcess.Enqueue(new SixLabors.ImageSharp.Point(startX, startY));

            // Directions for 8-connected pixels (including diagonals)
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            while (toProcess.Count > 0)
            {
                var current = toProcess.Dequeue();
                int x = current.X;
                int y = current.Y;

                if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y]) continue;

                if (!IsSignaturePixel(image[x, y])) continue;

                visited[x, y] = true;
                strokePoints.Add(new SixLabors.ImageSharp.Point(x, y));

                // Check 8-connected neighbors
                for (int i = 0; i < 8; i++)
                {
                    int newX = x + dx[i];
                    int newY = y + dy[i];

                    if (newX >= 0 && newX < width && newY >= 0 && newY < height && !visited[newX, newY])
                    {
                        toProcess.Enqueue(new SixLabors.ImageSharp.Point(newX, newY));
                    }
                }
            }

            // Sort points to create a more natural stroke order
            if (strokePoints.Count > 1)
            {
                strokePoints = OrderPointsForStroke(strokePoints);
            }

            return strokePoints;
        }

        private static List<SixLabors.ImageSharp.Point> OrderPointsForStroke(List<SixLabors.ImageSharp.Point> points)
        {
            if (points.Count <= 2) return points;

            var orderedPoints = new List<SixLabors.ImageSharp.Point>();
            var remainingPoints = new List<SixLabors.ImageSharp.Point>(points);

            // Start with the leftmost point (or topmost if same X)
            var currentPoint = remainingPoints.OrderBy(p => p.X).ThenBy(p => p.Y).First();
            orderedPoints.Add(currentPoint);
            remainingPoints.Remove(currentPoint);

            // Connect points by finding the nearest unvisited point
            while (remainingPoints.Count > 0)
            {
                var nearest = remainingPoints.OrderBy(p =>
                        Math.Sqrt(Math.Pow(p.X - currentPoint.X, 2) + Math.Pow(p.Y - currentPoint.Y, 2)))
                    .First();

                orderedPoints.Add(nearest);
                remainingPoints.Remove(nearest);
                currentPoint = nearest;
            }

            return orderedPoints;
        }

        private static void ProcessAndSaveSignaturePoints(Order order, List<SixLabors.ImageSharp.Point> signaturePoints,
            string signatureName)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int width = 0;
            int height = 0;

            foreach (var point in signaturePoints)
            {
                if (point.X == -1 && point.Y == -1) // Check for empty marker
                    continue;

                if (minX > point.X) minX = point.X;
                if (minY > point.Y) minY = point.Y;
                if (width < point.X) width = point.X;
                if (height < point.Y) height = point.Y;
            }

            if (minX == int.MaxValue || minY == int.MaxValue)
            {
                Logger.CreateLog("Signature too small or empty");
                return;
            }

            // round up to 32
            int x = Convert.ToInt32((width + 31 - minX)) / 32 * 32;
            int y = Convert.ToInt32((height + 31 - minY)) / 32 * 32;

            if (x < 10 || y < 10)
            {
                Logger.CreateLog("Signature dimensions too small");
                return;
            }

            int factor = 1;
            if (x > 320 && x < 641)
                factor = 2;
            else if (x > 640 && x < 961)
                factor = 3;
            else if (x > 960) factor = 4;

            if (x > 300)
            {
                // reduce by factor
                for (int i = 0; i < signaturePoints.Count; i++)
                {
                    if (!(signaturePoints[i].X == -1 && signaturePoints[i].Y == -1)) // Check for empty marker
                    {
                        var p = new SixLabors.ImageSharp.Point(signaturePoints[i].X / factor,
                            signaturePoints[i].Y / factor);
                        signaturePoints[i] = p;
                    }
                }
            }

            if (string.IsNullOrEmpty(order.SignatureUniqueId)) order.SignatureUniqueId = Guid.NewGuid().ToString("N");
            order.SignatureName = signatureName;
            order.SignaturePoints = signaturePoints;
            order.Save();
        }

        #endregion
        public static void GetCompaniesInfo()
        {
            var logosZip = Path.GetTempFileName();

            logosZip = logosZip.Replace(".tmp", ".zip");

            try
            {
                using (var netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetCompanyLogosCommand");

                    var d = netaccess.ReadStringFromNetwork();

                    if (d == "sendinglogosnow")
                    {
                        netaccess.ReceiveFile(logosZip);

                        ProcessLogosZipFile(logosZip);

                        AsignLogosToCompanies();
                    }

                    netaccess.CloseConnection();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (File.Exists(logosZip))
                    File.Delete(logosZip);
            }
        }

        public static void AsignLogosToCompanies()
        {
            foreach (var company in CompanyInfo.Companies)
            {
                DirectoryInfo dir = new DirectoryInfo(Config.CompanyLogosPath);
                foreach (var file in dir.GetFiles())
                {
                    File.Copy(file.FullName, Path.Combine(Config.CompanyLogosSavePath, file.Name + ".jpg"), true);

                    if (file.Name == company.CompanyId.ToString())
                    {
                        try
                        {
                            var destinationPath = Path.Combine(Config.CompanyLogosSavePath, file.Name + ".jpg");

                            using (var reader = new FileStream(destinationPath, FileMode.Open, FileAccess.Read))
                            {
                                const int MAX_W = 300;
                                const int MAX_H = 300;

                                var dOpt = new DecoderOptions
                                {
                                    TargetSize = new SixLabors.ImageSharp.Size(MAX_W, MAX_H),
                                    SkipMetadata = true,
                                    Sampler = KnownResamplers.NearestNeighbor
                                };

                                using var fs = File.OpenRead(destinationPath);
                                using var img = SixLabors.ImageSharp.Image.Load<L8>(dOpt, fs);

                                int paddedW = (img.Width + 31) & ~31;
                                int paddedH = (img.Height + 31) & ~31;

                                img.Configuration.PreferContiguousImageBuffers = true;
                                if (!img.DangerousTryGetSinglePixelMemory(out Memory<L8> mem))
                                    throw new InvalidOperationException("Pixel memory not contiguous");

                                int bytesPerRow = paddedW / 8;
                                byte[] raw = new byte[paddedH * bytesPerRow];

                                Span<L8> src = mem.Span;
                                for (int y = 0; y < img.Height; y++)
                                {
                                    int srcRow = y * img.Width;
                                    int dstIdx = y * bytesPerRow;

                                    byte acc = 0;
                                    int bit = 7;
                                    for (int x = 0; x < img.Width; x++)
                                    {
                                        if (src[srcRow + x].PackedValue < 128)
                                            acc |= (byte)(1 << bit);

                                        if (--bit < 0)
                                        {
                                            raw[dstIdx++] = acc;
                                            acc = 0;
                                            bit = 7;
                                        }
                                    }

                                    if (bit != 7) raw[dstIdx] = acc;
                                }

                                company.CompanyLogo = BitConverter.ToString(raw).Replace("-", "");
                                company.CompanyLogoWidth = bytesPerRow;
                                company.CompanyLogoHeight = paddedH;
                                company.CompanyLogoSize = raw.Length;
                                company.CompanyLogoPath = Path.Combine(Config.CompanyLogosSavePath, file.Name + ".jpg");
                            }
                        }
                        catch (Exception ex)
                        {

                        }


                    }
                }
            }
        }

        private static void ProcessLogosZipFile(string logosZip)
        {
            if (Directory.Exists(Config.CompanyLogosPath))
                Directory.Delete(Config.CompanyLogosPath, true);

            Directory.CreateDirectory(Config.CompanyLogosPath);

            FastZip zip = new FastZip();
            zip.ExtractZip(logosZip, Config.CompanyLogosPath, null);
        }

        private static void DeserializeCompanyFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                using (StreamReader reader = new StreamReader(filepath))
                {
                    string line;

                    CompanyInfo.Companies.Clear();

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split(new char[] { (char)20 });
                            var ci = new CompanyInfo()
                            {
                                CompanyName = parts[0],
                                CompanyAddress1 = parts[1],
                                CompanyAddress2 = parts[2],
                                CompanyPhone = parts[3]
                            };
                            if (parts.Length > 4)
                                ci.DUNS = parts[4];
                            if (parts.Length > 5)
                                ci.Location = parts[5];
                            if (parts.Length > 6)
                                ci.CommId = parts[6];
                            if (parts.Length > 7)
                                ci.CompanyEmail = parts[7];
                            if (parts.Length > 8)
                                ci.ExtraFields = parts[8];

                            if (parts.Length > 9)
                                ci.LogoId = Convert.ToInt32(parts[9]);

                            if (parts.Length > 10)
                                ci.IsDefault = Convert.ToInt32(parts[10]) > 0;

                            CompanyInfo.Companies.Add(ci);
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                            //Xamarin.Insights.Report(ee);
                        }
                    }
                    reader.Close();
                    CompanyInfo.Save();
                }
            }
        }

        public static string GetClientImages(int clientId)
        {
            string tempFile = null;

            using (NetAccess netaccess = new NetAccess())
            {
                try
                {
                    tempFile = Path.GetTempFileName();

                    bool error = false;

                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetClientPicturesCommand");
                    netaccess.WriteStringToNetwork(clientId.ToString());
                    string message = netaccess.ReadStringFromNetwork();

                    if (message == "emptyfolder")
                        return string.Empty;

                    error = netaccess.ReceiveFile(tempFile) == 0;
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (!error)
                        return tempFile;
                    else
                        return string.Empty;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                    return string.Empty;
                }


            }

        }

        public static string GetExternaInvoiceImages(string invoiceNumber)
        {
            string tempFile = null;

            using (NetAccess netaccess = new NetAccess())
            {
                try
                {
                    tempFile = Path.GetTempFileName();

                    bool error = false;

                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetOrderImagesCommand");
                    netaccess.WriteStringToNetwork(invoiceNumber);
                    string message = netaccess.ReadStringFromNetwork();

                    if (message == "emptyfolder")
                        return string.Empty;

                    error = netaccess.ReceiveFile(tempFile) == 0;
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    if (!error)
                        return tempFile;
                    else
                        return string.Empty;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                    return string.Empty;
                }
            }

        }
        public static void CreatePriceLevel(string[] currentrow)
        {
            try
            {
                int id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                string name = currentrow[1];

                string extraFields = string.Empty;
                if (currentrow.Length > 6)
                    extraFields = currentrow[6];

                PriceLevel.Add(new PriceLevel() { Id = id, Name = name, ExtraFields = extraFields });
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static DriverRoute GetRouteForDriverShipDate(int driverId, DateTime date)
        {
            try
            {
                Logger.CreateLog("GetRouteForDriverShipDate");

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
                    netaccess.WriteStringToNetwork("GetRouteForDriverShipDateCommand");
                    netaccess.WriteStringToNetwork(driverId + "|" + date.Ticks);

                    var reply = netaccess.ReadStringFromNetwork();
                    if (reply == "error")
                        throw new Exception("Error getting the route");
                    if (reply == "emptyroute")
                        return new DriverRoute() { DriverId = driverId, DateTicks = date.Ticks };

                    var filePath = Path.GetTempFileName();
                    netaccess.ReceiveFile(filePath);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    var route = DriverRoute.Load(filePath);

                    Logger.CreateLog("Done GetRouteForDriverShipDate");

                    return route;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static List<DriverRouteOrder> GetOrdersToRoute()
        {
            try
            {
                Logger.CreateLog("GetOrdersToRoute");

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
                    netaccess.WriteStringToNetwork("GetOrdersToRouteCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());

                    var reply = netaccess.ReadStringFromNetwork();
                    if (reply == "error")
                        throw new Exception("Error getting the orders to add to the route");
                    if (reply == "emptyorders")
                        return new List<DriverRouteOrder>();

                    var filePath = Path.GetTempFileName();
                    netaccess.ReceiveFile(filePath);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    var orders = new List<DriverRouteOrder>();

                    XmlSerializer serializer = new XmlSerializer(orders.GetType());
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        orders = (List<DriverRouteOrder>)serializer.Deserialize(sr);
                    }

                    Logger.CreateLog("Done GetOrdersToRoute");

                    return orders.OrderBy(x => x.ShipDate).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static void SaveRoute(string filename)
        {
            try
            {
                Logger.CreateLog("SaveRoute");

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
                    netaccess.WriteStringToNetwork("UpdateRouteForDriverShipdateCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                    netaccess.SendFile(filename);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Done UpdateRouteForDriverShipdateCommand");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static string CheckOrderChangesBeforeSaveRoute(List<DriverRouteOrder> orders)
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                Logger.CreateLog("GetOrdersToRoute");

                try
                {
                    // Creates XmlSerializer of the List<User> type
                    XmlSerializer serializer = new XmlSerializer(orders.GetType());

                    // Creates a stream using which we'll serialize
                    using (StreamWriter sw = new StreamWriter(tempFile))
                    {
                        // We call the Serialize() method and pass the stream created above as the first parameter
                        // The second parameter is the object which we want to serialize
                        serializer.Serialize(sw, orders);
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }

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
                    netaccess.WriteStringToNetwork("CheckOrderChangesBeforeSaveRouteCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());

                    netaccess.SendFile(tempFile);

                    string result = netaccess.ReadStringFromNetwork();

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Done GetOrdersToRoute");

                    if (result == "error")
                        throw new Exception("Error checking changes in orders");

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                throw;
            }
        }

        public static void SaveMultipleRoutes(string filename)
        {
            try
            {
                Logger.CreateLog("SaveMultipleRoutes");

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

                    // send the routes
                    netaccess.WriteStringToNetwork("UpdateMultipleRoutesForDriverShipdateCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                    netaccess.SendFile(filename);

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Logger.CreateLog("Done UpdateMultipleRoutesForDriverShipdateCommand");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                throw;
            }
        }

        public static List<Client> GetClientsInRoute(DateTime date)
        {
            List<Client> clients = new List<Client>();
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
                    netaccess.WriteStringToNetwork("GetClientsInRouteCommand");
                    netaccess.WriteStringToNetwork(date.Ticks.ToString());

                    var clientsAsString = netaccess.ReadStringFromNetwork();

                    List<string> c = clientsAsString.Split('|').ToList();
                    foreach (var cli in c)
                    {
                        int id = Convert.ToInt32(cli);
                        var client = Client.Clients.FirstOrDefault(x => x.ClientId == id);
                        if (client != null)
                            clients.Add(client);
                    }

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return clients;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return clients;
            }
        }

        public static string GetSMTPConfiguration()
        {
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
                    netaccess.WriteStringToNetwork("SendSMTPConfigurationCommand");

                    var smtp = netaccess.ReadStringFromNetwork();

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return smtp;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static void SignOutDevice()
        {
            bool isButler = false;

            isButler = Config.ButlerCustomization;

            var acceptedTerms = Config.AcceptedTermsAndConditions;
            var enabledlogin = Config.EnableLogin;

            ActivityState.RemoveAll();

            Config.ClearSettings();

            Config.AcceptedTermsAndConditions = acceptedTerms;
            Config.EnableLogin = enabledlogin;

            Config.ButlerSignedIn = false;

            Config.Initialize();

            Config.IPAddressGateway = "";
            Config.Port = 0;
            Config.SalesmanId = 0;

            Config.SaveSettings();

        }

        public static string ChangeSalesmanPassword(int salesmanId, string password)
        {
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
                        return "error";
                    }

                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // send the orders
                    netaccess.WriteStringToNetwork("ChangeSalesmanPasswordCommand");
                    netaccess.WriteStringToNetwork(salesmanId.ToString() + "|" + password);

                    var result = netaccess.ReadStringFromNetwork();

                    netaccess.CloseConnection();

                    if (result == "error")
                    {
                        return result;
                    }
                    else
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return "error";
            }
        }



        public static void CreateSuggestedClientCategory(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];

            SuggestedClientCategory.List.Add(new SuggestedClientCategory() { Id = id, Name = name });
        }

        public static void CreateSuggestedClientCategoryClient(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var suggestedCatId = Convert.ToInt32(currentrow[1]);
            var clientId = Convert.ToInt32(currentrow[2]);

            var item = new SuggestedClientCategoryClient()
            {
                Id = id,
                SuggestedClientCategoryId = suggestedCatId,
                ClientId = clientId
            };

            var cat = SuggestedClientCategory.List.FirstOrDefault(x => x.Id == suggestedCatId);
            if (cat != null)
                cat.SuggestedClientCategoryClients.Add(item);

            SuggestedClientCategoryClient.List.Add(item);
        }

        public static void CreateSuggestedClientCategoryProduct(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var suggestedCatId = Convert.ToInt32(currentrow[1]);
            var productId = Convert.ToInt32(currentrow[2]);

            var item = new SuggestedClientCategoryProduct()
            {
                Id = id,
                SuggestedClientCategoryId = suggestedCatId,
                ProductId = productId
            };

            var cat = SuggestedClientCategory.List.FirstOrDefault(x => x.Id == suggestedCatId);
            if (cat != null)
                cat.SuggestedClientCategoryProducts.Add(item);

            SuggestedClientCategoryProduct.List.Add(item);
        }

        internal static void CreateOrderDiscount(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];
            var discountType = Convert.ToInt32(currentrow[2]);
            var appliedTo = Convert.ToInt32(currentrow[3]);
            var Comments = currentrow[4];
            var ExtraFields = currentrow[5];
            var status = Convert.ToInt32(currentrow[6]);
            var startDate = new DateTime(Convert.ToInt64(currentrow[7]));
            var endDate = new DateTime(Convert.ToInt64(currentrow[8]));
            var productDiscountId = Convert.ToInt32(currentrow[9]);
            var automatic = Convert.ToInt32(currentrow[10]) > 0;
            var exclusive = Convert.ToInt32(currentrow[11]) > 0;

            bool permanent = false;
            if (currentrow.Length > 12)
                permanent = Convert.ToInt32(currentrow[12]) > 0;

            var item = new OrderDiscount()
            {
                Id = id,
                Name = name,
                DiscountType = discountType,
                AppliedTo = appliedTo,
                Comments = Comments,
                ExtraFields = ExtraFields,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                ProductDiscountId = productDiscountId,
                AutomaticApplied = automatic,
                Exclusive = exclusive,
                Permanent = permanent
            };

            OrderDiscount.List.Add(item);
        }

        internal static void CreateOrderDiscountClient(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var discountType = Convert.ToInt32(currentrow[2]);
            var buy = Convert.ToDouble(currentrow[3]);
            var qty = Convert.ToDouble(currentrow[4]);
            var clientId = Convert.ToInt32(currentrow[5]);
            var extraFields = currentrow[6];

            var item = new OrderDiscountClient()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                DiscountType = discountType,
                Buy = buy,
                Qty = qty,
                ClientId = clientId,
                ExtraFields = extraFields
            };

            var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (discount != null)
                discount.OrderDiscountClients.Add(item);

            OrderDiscountClient.List.Add(item);
        }

        internal static void CreateOrderDiscountClientArea(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var discountType = Convert.ToInt32(currentrow[2]);
            var buy = Convert.ToDouble(currentrow[3]);
            var qty = Convert.ToDouble(currentrow[4]);
            var areaId = Convert.ToInt32(currentrow[5]);
            var extraFields = currentrow[6];

            var item = new OrderDiscountClientArea()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                DiscountType = discountType,
                Buy = buy,
                Qty = qty,
                AreaId = areaId,
                ExtraFields = extraFields
            };

            var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (discount != null)
                discount.OrderDiscountClientAreas.Add(item);

            OrderDiscountClientArea.List.Add(item);
        }

        internal static void CreateOrderDiscountProduct(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var discountType = Convert.ToInt32(currentrow[2]);
            var buy = Convert.ToDouble(currentrow[3]);
            var qty = Convert.ToDouble(currentrow[4]);
            var prodId = Convert.ToInt32(currentrow[5]);
            var extraFields = currentrow[6];

            var item = new OrderDiscountProduct()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                DiscountType = discountType,
                Buy = buy,
                Qty = qty,
                ProductId = prodId,
                ExtraFields = extraFields
            };

            var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (discount != null)
                discount.OrderDiscountProducts.Add(item);

            OrderDiscountProduct.List.Add(item);
        }

        internal static void CreateOrderDiscountVendor(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var discountType = Convert.ToInt32(currentrow[2]);
            var buy = Convert.ToDouble(currentrow[3]);
            var qty = Convert.ToDouble(currentrow[4]);
            var vendorId = Convert.ToInt32(currentrow[5]);
            var extraFields = currentrow[6];

            var item = new OrderDiscountVendor()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                DiscountType = discountType,
                Buy = buy,
                Qty = qty,
                VendorId = vendorId,
                ExtraFields = extraFields
            };

            var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (discount != null)
                discount.OrderDiscountVendors.Add(item);
        }

        internal static void CreateOrderDiscountBreak(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var minQty = Convert.ToDouble(currentrow[2]);
            var maxQty = Convert.ToDouble(currentrow[3]);
            var discount = Convert.ToDouble(currentrow[4]);
            var qtySelectProduct = Convert.ToDouble(currentrow[5]);
            var extraFields = currentrow[6];
            var discountType = Convert.ToInt32(currentrow[7]);

            bool fixedPrice = false;
            if (currentrow.Length > 8)
                fixedPrice = Convert.ToInt32(currentrow[8]) > 0;

            var item = new OrderDiscountBreak()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                MinQty = minQty,
                MaxQty = maxQty,
                Discount = discount,
                QtySelectProduct = qtySelectProduct,
                ExtraFields = extraFields,
                DiscountType = discountType,
                FixPrice = fixedPrice
            };

            var orderdiscount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (orderdiscount != null)
                orderdiscount.OrderDiscountBreaks.Add(item);
        }

        internal static void CreateOrderDiscountProductBreak(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var breakId = Convert.ToInt32(currentrow[1]);
            var productId = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new OrderDiscountProductBreak()
            {
                Id = id,
                BreakId = breakId,
                ProductId = productId,
                ExtraFields = extraFields
            };

            foreach (var o in OrderDiscount.List)
            {
                var disc_break = o.OrderDiscountBreaks.FirstOrDefault(x => x.Id == breakId);
                if (disc_break != null)
                    disc_break.OrderDiscountProductBreaks.Add(item);
            }
        }

        internal static void CreateOrderDiscountVendorBreak(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var breakId = Convert.ToInt32(currentrow[1]);
            var vendorId = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new OrderDiscountVendorBreak()
            {
                Id = id,
                BreakId = breakId,
                VendorId = vendorId,
                ExtraFields = extraFields
            };

            foreach (var o in OrderDiscount.List)
            {
                var disc_break = o.OrderDiscountBreaks.FirstOrDefault(x => x.Id == breakId);
                if (disc_break != null)
                    disc_break.OrderDiscountVendorBreaks.Add(item);
            }
        }

        internal static void CreateArea(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];
            var extraFields = currentrow[2];
            var color = Convert.ToInt32(currentrow[3]);
            var isActive = Convert.ToInt32(currentrow[4]) > 0;
            var forDelivery = Convert.ToInt32(currentrow[5]) > 0;

            var item = new Area()
            {
                Id = id,
                Name = name,
                ExtraFields = extraFields,
                Color = color,
                Active = isActive,
                ForDelivery = forDelivery
            };

            Area.List.Add(item);
        }

        internal static void CreateVendor(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];
            var originalId = currentrow[2];
            var comments = currentrow[3];
            var contactName = currentrow[4];
            var contactPhone = currentrow[5];
            var isActive = Convert.ToInt32(currentrow[6]) > 0;
            var openBalance = Convert.ToDouble(currentrow[7]);
            var creditLimit = Convert.ToDouble(currentrow[8]);
            var extraFields = currentrow[9];
            var uniqueId = currentrow[10];
            var nonVisibleExtraFields = currentrow[11];
            var email = currentrow[12];

            var item = new Vendor()
            {
                Id = id,
                Name = name,
                OriginalId = originalId,
                Comments = comments,
                ContactName = contactName,
                ContactPhone = contactPhone,
                IsActive = isActive,
                OpenBalance = openBalance,
                CreditLimit = creditLimit,
                ExtraFields = extraFields,
                UniqueId = uniqueId,
                NonVisibleExtraFields = nonVisibleExtraFields,
                Email = email
            };

            Vendor.List.Add(item);
        }

        internal static void CreateAreaClient(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var areaId = Convert.ToInt32(currentrow[1]);
            var clientId = Convert.ToInt32(currentrow[2]);

            var item = new AreaClient()
            {
                Id = id,
                AreaId = areaId,
                ClientId = clientId
            };

            AreaClient.List.Add(item);
        }

        internal static void CreateOrderDiscountCategory(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var orderDiscountId = Convert.ToInt32(currentrow[1]);
            var discountType = Convert.ToInt32(currentrow[2]);
            var buy = Convert.ToDouble(currentrow[3]);
            var qty = Convert.ToDouble(currentrow[4]);
            var categoryId = Convert.ToInt32(currentrow[5]);
            var categoryType = Convert.ToInt32(currentrow[6]);
            var extraFields = currentrow[7];

            var item = new OrderDiscountCategory()
            {
                Id = id,
                OrderDiscountId = orderDiscountId,
                DiscountType = discountType,
                Buy = buy,
                Qty = qty,
                CategoryId = categoryId,
                CategoryType = categoryType,
                ExtraFields = extraFields
            };

            var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == orderDiscountId);
            if (discount != null)
                discount.OrderDiscountCategories.Add(item);

            OrderDiscountCategory.List.Add(item);
        }

        internal static void CreateOrderDiscountCategoryBreak(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var breakId = Convert.ToInt32(currentrow[1]);
            var categoryId = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new OrderDiscountCategoryBreak()
            {
                Id = id,
                BreakId = breakId,
                CategoryId = categoryId,
                ExtraFields = extraFields
            };

            foreach (var o in OrderDiscount.List)
            {
                var disc_break = o.OrderDiscountBreaks.FirstOrDefault(x => x.Id == breakId);
                if (disc_break != null)
                    disc_break.OrderDiscountCategoryBreaks.Add(item);
            }
        }

        internal static void CreateClientCategoryEx(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];

            var item = new ClientCategoryEx()
            {
                Id = id,
                Name = name
            };

            ClientCategoryEx.List.Add(item);
        }

        internal static void CreateClientDepartmentGroup(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];
            var comments = currentrow[2];
            var extraFields = currentrow[3];
            var status = Convert.ToInt32(currentrow[4]);

            var item = new ClientDepartmentGroup()
            {
                Id = id,
                Name = name,
                Comments = comments,
                ExtraFields = extraFields,
                Status = status
            };

            ClientDepartmentGroup.List.Add(item);
        }

        internal static void CreateClientClientDepartmentGroup(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var clientId = Convert.ToInt32(currentrow[1]);
            var clientDepartmentGroupid = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new ClientClientDepartmentGroup()
            {
                Id = id,
                ClientId = clientId,
                ClientDepartmentGroupId = clientDepartmentGroupid,
                ExtraFields = extraFields,
            };

            ClientClientDepartmentGroup.List.Add(item);
        }

        internal static void CreateDepartmentClientDepartmentGroup(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var departmentId = Convert.ToInt32(currentrow[1]);
            var clientDepartmentGroupid = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new DepartmentClientDepartmentGroup()
            {
                Id = id,
                DepartmentId = departmentId,
                ClientDepartmentGroupId = clientDepartmentGroupid,
                ExtraFields = extraFields
            };

            DepartmentClientDepartmentGroup.List.Add(item);
        }

        internal static void CreateDepartmertClientCategories(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var name = currentrow[1];
            var isActive = Convert.ToInt32(currentrow[2]) > 0;
            var extraFields = currentrow[3];

            var item = new DepartmertClientCategory()
            {
                Id = id,
                Name = name,
                IsActive = isActive,
                ExtraFields = extraFields
            };

            DepartmertClientCategory.List.Add(item);
        }

        internal static void CreateDepartmentProduct(string[] currentrow)
        {

            var id = Convert.ToInt32(currentrow[0]);
            var departmentId = Convert.ToInt32(currentrow[1]);
            var productId = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new DepartmentProduct()
            {
                Id = id,
                DepartmentId = departmentId,
                ProductId = productId,
                ExtraFields = extraFields
            };

            DepartmentProduct.List.Add(item);
        }

        internal static void CreateOrderDiscountClientPriceLevel(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var discountId = Convert.ToInt32(currentrow[1]);
            var priceLevelId = (Convert.ToInt32(currentrow[2]) + 10);
            var DiscountType = Convert.ToInt32(currentrow[3]);
            var Buy = Convert.ToDouble(currentrow[4]);
            var Qty = Convert.ToDouble(currentrow[5]);
            var extraFields = currentrow[6];

            var item = new OrderDiscountClientPriceLevel()
            {
                Id = id,
                OrderDiscountId = discountId,
                PriceLevelId = priceLevelId,
                DiscountType = DiscountType,
                Buy = Buy,
                Qty = Qty,
                ExtraFields = extraFields
            };


            var orderDiscount = OrderDiscount.List.FirstOrDefault(x => x.Id == item.OrderDiscountId);
            if (orderDiscount != null)
                orderDiscount.OrderDisocuntClientPriceLevels.Add(item);

            OrderDiscountClientPriceLevel.List.Add(item);
        }

        internal static void CreateProductVisibleCompany(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var productId = Convert.ToInt32(currentrow[1]);
            var companyId = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new ProductVisibleCompany()
            {
                Id = id,
                ProductId = productId,
                CompanyId = companyId,
                ExtraFields = extraFields
            };

            ProductVisibleCompany.List.Add(item);
        }

        internal static void CreateAsset(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var productId = Convert.ToInt32(currentrow[1]);
            var startDate = new DateTime(Convert.ToInt64(currentrow[2]));
            var serialNumber = currentrow[3];
            var Qty = Convert.ToDouble(currentrow[4]);
            var Price = Convert.ToDouble(currentrow[5]);
            var comments = currentrow[6];
            var createdOn = new DateTime(Convert.ToInt64(currentrow[7]));
            var active = Convert.ToInt32(currentrow[8]) > 0;
            var deactivedDate = new DateTime(Convert.ToInt64(currentrow[9]));
            var extraFields = currentrow[10];
            var siteId = Convert.ToInt32(currentrow[11]);

            var item = new Asset()
            {
                Id = id,
                ProductId = productId,
                StartDate = startDate,
                SerialNumber = serialNumber,
                Qty = Qty,
                Price = Price,
                Comments = comments,
                CreatedOn = createdOn,
                Active = active,
                DeactivatedDate = deactivedDate,
                Extrafields = extraFields,
                SiteId = siteId
            };

            Asset.List.Add(item);
        }

        internal static void CreateClientAssetTrack(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var assetId = Convert.ToInt32(currentrow[1]);
            var clientId = Convert.ToInt32(currentrow[2]);
            var startDate = new DateTime(Convert.ToInt64(currentrow[3]));
            var Qty = Convert.ToDouble(currentrow[4]);
            var Price = Convert.ToDouble(currentrow[5]);
            var comments = currentrow[6];
            var active = Convert.ToInt32(currentrow[7]) > 0;
            var deactivedDate = new DateTime(Convert.ToInt64(currentrow[8]));
            var extraFields = currentrow[9];
            var siteId = Convert.ToInt32(currentrow[10]);

            var item = new ClientAssetTrack()
            {
                Id = id,
                AssetId = assetId,
                ClientId = clientId,
                StartDate = startDate,
                Qty = Qty,
                Price = Price,
                Comments = comments,
                Active = active,
                DeactivatedDate = deactivedDate,
                Extrafields = extraFields,
                SiteId = siteId
            };

            ClientAssetTrack.List.Add(item);
        }

        internal static void CreateProductAllowedSites(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0]);
            var productId = Convert.ToInt32(currentrow[1]);
            var Siteid = Convert.ToInt32(currentrow[2]);
            var extraFields = currentrow[3];

            var item = new ProductAllowedSite()
            {
                Id = id,
                ProductId = productId,
                SiteId = Siteid,
                ExtraFields = extraFields
            };

            ProductAllowedSite.List.Add(item);
        }

        public static bool GetCatalogPdf(int priceLevelId, bool printPrice, bool printUpc, bool printUom, List<int> categories)
        {
            if (File.Exists(Config.CatalogPDFPath))
                File.Delete(Config.CatalogPDFPath);

            try
            {
                using (var netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());


                    netaccess.WriteStringToNetwork("GetCatalogPdfCommand");

                    string categoriesAsString = string.Empty;

                    foreach (var c in categories)
                    {
                        if (string.IsNullOrEmpty(categoriesAsString))
                            categoriesAsString = c.ToString();
                        else
                            categoriesAsString += "|" + c.ToString();
                    }

                    netaccess.WriteStringToNetwork(priceLevelId.ToString() + "," + (printPrice ? "1" : "0") + "," + (printUpc ? "1" : "0") + "," + (printUom ? "1" : "0") + "," + categoriesAsString);

                    string success = netaccess.ReadStringFromNetwork();
                    if (success == "done")
                    {
                        netaccess.ReceiveFile(Config.CatalogPDFPath);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog("Error Getting catalog PDF" + ee);
                return false;
            }
            finally
            {
            }
        }

        public static void SendRouteExpenses()
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SendRouteExpensesCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(Config.ExpensesPath);

                    var response = access.ReadStringFromNetwork();
                    if (response == "success")
                    {
                        if (File.Exists(Config.ExpensesPath))
                            File.Delete(Config.ExpensesPath);

                        RouteExpenses.CurrentExpenses = null;
                    }

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                    throw;
                }
            }
        }

        public static long GetExportFileLastDate()
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("FilesLastModifiedDateCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

                    var response = access.ReadStringFromNetwork();

                    long toReturn = 0;

                    Int64.TryParse(response, out toReturn);

                    return toReturn;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
        }

        public static bool SendClientTobbacoLicense(int clientid, string licensetxt, long ticks)
        {
            using (NetAccess netaccess = new NetAccess())
            {
                //open the connection
                netaccess.OpenConnection();
                // Get the products file
                netaccess.WriteStringToNetwork("HELO");
                netaccess.WriteStringToNetwork(Config.GetAuthString());

                netaccess.WriteStringToNetwork("SetClientTobbacoLicenseCommand");

                netaccess.WriteStringToNetwork(clientid + "|" + licensetxt + "|" + ticks.ToString());

                bool sucess = false;
                var confirmation = netaccess.ReadStringFromNetwork();
                sucess = confirmation == "success";

                //Close the connection and disconnect
                netaccess.WriteStringToNetwork("Goodbye");
                Thread.Sleep(1000);
                netaccess.CloseConnection();

                return sucess;
            }
        }
    }
}

