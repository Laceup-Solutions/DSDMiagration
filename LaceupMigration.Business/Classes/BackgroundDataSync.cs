using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;

namespace LaceupMigration
{
    public static class BackgroundDataSync
    {

        static double lastLatitude;
        static double lastLongitude;
        static float lastInventory;

        static DateTime LastSessionSent = DateTime.Now;
        static DateTime LastSentBackup = DateTime.Now;
        static DateTime ModifiedBackup = DateTime.Now;

        static DateTime LastSentRoute = DateTime.Now;
        static DateTime ModifiedRoute = DateTime.Now;

        static DateTime LastSentOrderPayment = DateTime.Now;
        static DateTime ModifiedOrderPayments = DateTime.Now;

        static DateTime LastSentFinalizedOrder = DateTime.Now;
        static DateTime ModifiedFinalizedOrder = DateTime.Now;

        static DateTime LastSentCollectedPayments = DateTime.Now;
        static DateTime ModifiedCollectedPayments = DateTime.Now;

        static Thread BackgroundThread;

        static volatile object lockerObject = new object();
        private static Thread getImagesThread;
        private static Thread setProductDefaultUom;
        private static Thread updateInvValuesFromOrdersThread;

        static public void CallMe() { }

        static public bool Running { get; set; }

        static public void StartThreadh()
        {
            if (Running)
                return;

            BackgroundThread = new Thread(delegate () { BackgroundDataSync.WorkerMethod(); });
            BackgroundThread.IsBackground = true;
            BackgroundThread.Start();
        }

        public static void SyncOrderPayment()
        {
            ModifiedOrderPayments = DateTime.Now;
            ModifiedBackup = DateTime.Now;
        }

        public static void SyncRoute()
        {
            ModifiedRoute = DateTime.Now;
            ModifiedBackup = DateTime.Now;
        }

        public static void SyncFinalizedOrders()
        {
            ModifiedFinalizedOrder = DateTime.Now;
            ModifiedBackup = DateTime.Now;

            if (Config.SendBackgroundOrders)
            {
                // send it right now from a thread
                var thread = new Thread(delegate () { SendFinalizedOrderFile(); });
                thread.IsBackground = true;
                thread.Start();
            }

            SyncCollectedPayments();
        }

        public static void SyncCollectedPayments()
        {
            ModifiedCollectedPayments = DateTime.Now;
            ModifiedBackup = DateTime.Now;

            if (Config.SendBackgroundPayments)
            {
                // send it right now from a thread
                var thread = new Thread(delegate () { SendCollectedPaymentsFile(); });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public static void SendTempPaymentsInBackground()
        {
            ModifiedCollectedPayments = DateTime.Now;
            ModifiedBackup = DateTime.Now;

            SendCollectedPaymentsFile(false);
        }

        private static void WorkerMethod()
        {
            if (DataAccess.LoadingData)
                return;

            if (!Config.BackGroundSync)
            {
                Running = false;
                return;
            }

            while (true)
            {
                Running = true;
                Thread.Sleep(1000 * Config.BackgroundTime); // 1 min
                try
                {
                    var current = Connectivity.NetworkAccess;

                    if (current != NetworkAccess.Internet && current != NetworkAccess.Local && current != NetworkAccess.ConstrainedInternet)
                    {
                        return;
                    }

                    if (LastSentRoute.Ticks < ModifiedRoute.Ticks)
                        SendRoute();

                    var currentInventoryTotal = Product.Products.Sum(x => x.CurrentInventory);
                    SendInventory(currentInventoryTotal);

                    if (Config.LastLatitude != lastLatitude || Config.LastLongitude != lastLongitude)
                        SenndLastKnowLocation();

                    if (LastSentOrderPayment.Ticks < ModifiedOrderPayments.Ticks)
                        SendOrdersAndPaymentsInfo();

                    if (Config.SendBackgroundOrders && LastSentFinalizedOrder.Ticks < ModifiedFinalizedOrder.Ticks)
                        SendFinalizedOrderFile();

                    if (Config.SendBackgroundBackup && LastSentBackup.Ticks < ModifiedBackup.Ticks)
                        SendDataFile();

                    if (Config.TimeSheetCustomization && Session.session != null && Config.AutomaticClockOutTime > 0)
                        AutomaticClockOut();

                    if (Config.TimeSheetCustomization && Session.session != null && Config.ForceBreakInMinutes > 0)
                    {
                        if (Session.ShouldWarnForBreak())
                        {
                            try
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    DialogHelper._dialogService.ShowAlertAsync("The app will block in 10 minutes for a mandatory break.");
                                    Session.alreadyWarned = true;
                                });
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        var totalWorked = DateTime.Now - Session.GetLastWorkStartTime();

                        if (totalWorked.TotalMinutes >= Config.ForceBreakInMinutes)
                        {
                            Session.StartMandatoryBreak();
                            Session.EndMandatoryBreak();
                        }
                    }

                    if (Session.session != null && LastSessionSent.Ticks < Session.session.LastModifiedSync.Ticks)
                    {
                        // [MIGRATION]: Ensure SessionPath directory exists before accessing SessionFile
                        // This prevents "Could not find a part of the path" errors on Android tablet emulators
                        if (!Directory.Exists(Config.SessionPath))
                        {
                            Directory.CreateDirectory(Config.SessionPath);
                        }

                        DataAccess.SendCurrentSession(Path.Combine(Config.SessionPath, "SessionFile.cvs"));
                        LastSessionSent = DateTime.Now;
                    }
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }
            }
        }

        private static void AutomaticClockOut()
        {
            var openSessions = Session.sessionDetails.Where(x => x.endTime == DateTime.MinValue && x.detailType != SessionDetails.SessionDetailType.Break);

            if (openSessions.Any(x => x.startTime.AddMinutes(Config.AutomaticClockOutTime) <= DateTime.Now))
            {
                bool changesMade = false;

                foreach (var s in openSessions)
                {
                    if (s.clientId == 0)
                    {
                        s.endTime = DateTime.Now;
                        s.endLatitude = Config.LastLatitude;
                        s.endLongitude = Config.LastLongitude;
                        s.extraFields = DataAccess.SyncSingleUDF("comment",
                            $"Automtically Closed after {Config.AutomaticClockOutTime} minutes", s.extraFields);

                        changesMade = true;
                    }
                    else
                    {
                        var client = Client.Clients.FirstOrDefault(x => x.ClientId == s.clientId);

                        if (client != null)
                        {
                            if (!Order.Orders.Any(x => x.Client == client && !x.Finished))
                            {
                                changesMade = true;
                                s.endTime = DateTime.Now;
                                s.endLatitude = Config.LastLatitude;
                                s.endLongitude = Config.LastLongitude;
                                s.extraFields = DataAccess.SyncSingleUDF("comment",
                                    $"Automtically Closed after {Config.AutomaticClockOutTime} minutes", s.extraFields);

                                //close open batches
                                var batches = Batch.List.Where(x => x.Client == client && x.ClockedOut == DateTime.MinValue);
                                foreach (var b in batches)
                                {
                                    b.ClockedOut = DateTime.Now;
                                    b.Status = BatchStatus.Locked;
                                    b.Save();
                                }
                            }
                        }
                    }
                }

                if (changesMade)
                    Session.session.Save();
            }
        }

        private static void SendCollectedPaymentsFile(bool uselock = true)
        {
            if (uselock)
            {
                lock (lockerObject)
                {
                    ContinueSendingPayments();
                }
            }
            else
            {
                ContinueSendingPayments();
            }
        }

        public static void ContinueSendingPayments()
        {
            var source = InvoicePayment.List.Where(x => x.DateCreated > LastSentCollectedPayments).ToList();

            try
            {
                DataAccess.SendInvoicePaymentsBySource(source, false, true);
                LastSentCollectedPayments = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        private static void SendFinalizedOrderFile()
        {
            lock (lockerObject)
            {
                var source = Order.Orders.Where(x => x.Finished && x.EndDate > LastSentFinalizedOrder).ToList();

                var dict = new Dictionary<int, List<string>>();
                foreach (var item in source)
                {
                    if (item.AsPresale && item.OrderType != OrderType.NoService)
                        continue;

                    if (!dict.Keys.Contains(item.BatchId))
                        dict.Add(item.BatchId, new List<string>());

                    dict[item.BatchId].Add(item.OrderId.ToString());
                }

                foreach (var item in dict)
                {
                    var batch = Batch.List.FirstOrDefault(x => x.Id == item.Key);
                    if (batch == null)
                        continue;

                    try
                    {
                        DataAccess.SendTheOrders(new Batch[] { batch }, item.Value, false);
                        LastSentFinalizedOrder = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        break;
                    }
                }
            }
        }

        public static void ForceBackup()
        {
            //int rep = 0;
            //while (rep < 3)
            //{
            //    if (FileOperationsLocker.InUse)
            //        Thread.Sleep(2000);
            //    rep++;
            //}

            lock (FileOperationsLocker.lockFilesObject)
            {
                var tempPathFile = Path.Combine(Path.GetTempPath(), "backupFile.zip");

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

                    var tempPathFolder = Path.Combine(Path.GetTempPath(), "LACEUP_BACKUP");

                    if (Directory.Exists(tempPathFolder))
                        Directory.Delete(tempPathFolder, true);

                    // copy ALL the lacep data folder to the temp
                    DirectoryCopy(Config.CodeBase, tempPathFolder, true);

                    fastZip.CreateZip(tempPathFile, tempPathFolder, recurse, filter);
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }

                var l = new FileInfo(tempPathFile).Length;
                if (l == 0)
                {
                    if (Directory.Exists(tempPathFile))
                        Directory.Delete(tempPathFile, true);
                    return;
                }

                try
                {
                    using (NetAccess netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("systemversion");
                        var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                        if (10 > Convert.ToInt32(version[0]))
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return;
                        }
                        if (version[0] == "10" && Convert.ToInt32(version[1]) < 4)
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return;
                        }
                        netaccess.WriteStringToNetwork("BackgroundDataSyncCommand");
                        netaccess.WriteStringToNetwork(Config.GetAuthString() + "=" + Config.VendorName + "__BEFORE_UPDATE");
                        netaccess.SendFile(tempPathFile);
                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();
                    }

                    LastSentBackup = DateTime.Now;

                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

                if (File.Exists(tempPathFile))
                    File.Delete(tempPathFile);
            }
        }

        private static void SendDataFile()
        {
            //if (FileOperationsLocker.InUse)
            //    return;


            var tempPathFile = Path.Combine(Path.GetTempPath(), "backupFile.zip");

            try
            {
                //FileOperationsLocker.InUse = true;

                var fastZip = new FastZip();

                bool recurse = true;  // Include all files by recursing through the directory structure
                string filter = null; // Dont filter any files at all

                var tempPathFolder = Path.Combine(Path.GetTempPath(), "LACEUP_BACKUP");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                lock (FileOperationsLocker.lockFilesObject)
                {
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

                    // copy ALL the lacep data folder to the temp
                    DirectoryCopy(Config.CodeBase, tempPathFolder, true);
                }

                fastZip.CreateZip(tempPathFile, tempPathFolder, recurse, filter);
            }
            finally
            {
                //FileOperationsLocker.InUse = false;
            }

            var l = new FileInfo(tempPathFile).Length;
            if (l == 0)
            {
                if (Directory.Exists(tempPathFile))
                    Directory.Delete(tempPathFile, true);
                return;
            }

            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());
                    netaccess.WriteStringToNetwork("systemversion");
                    var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                    if (10 > Convert.ToInt32(version[0]))
                    {
                        netaccess.CloseConnection();
                        Running = false;
                        return;
                    }
                    if (version[0] == "10" && Convert.ToInt32(version[1]) < 4)
                    {
                        netaccess.CloseConnection();
                        Running = false;
                        return;
                    }
                    netaccess.WriteStringToNetwork("BackgroundDataSyncCommand");
                    netaccess.WriteStringToNetwork(Config.GetAuthString() + "=" + Config.VendorName);
                    netaccess.SendFile(tempPathFile);
                    netaccess.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    netaccess.CloseConnection();
                }

                LastSentBackup = DateTime.Now;
            }
            catch (Exception ee)
            {
                //Logger.CreateLog(ee);
            }

            if (File.Exists(tempPathFile))
                File.Delete(tempPathFile);
        }


        private static void SendOrdersAndPaymentsInfo()
        {
            // send any created order
            // first the preorder
            var source = Order.Orders.Where(x => x.Date > LastSentOrderPayment /*&& x.Latitude != 0 && x.Longitude != 0*/ && x.AsPresale).ToList();

            // then the invoices (DSD) orders that are finished
            source.AddRange(Order.Orders.Where(x => x.EndDate > LastSentOrderPayment /*&& x.Latitude != 0 && x.Longitude != 0*/ && !x.AsPresale).ToList());

            if (source.Count > 0)
                if (!SendNewOrders(source))
                    return;

            var sourcePayment = InvoicePayment.List.Where(x => x.DateCreated > LastSentOrderPayment).ToList();
            if (sourcePayment.Count > 0)
                if (!SendNewPayments(sourcePayment))
                    return;

            LastSentOrderPayment = DateTime.Now;
        }

        private static bool SendNewPayments(List<InvoicePayment> source)
        {// these are new orders created in the device

            StringBuilder newPayments = new StringBuilder();

            foreach (var payment in source)
            {
                newPayments.Append(payment.Client.ClientId);
                newPayments.Append((char)20);
                newPayments.Append(payment.DateCreated.Ticks);
                newPayments.Append((char)20);
                newPayments.Append(payment.InvoicesId);   // 2
                newPayments.Append((char)20);
                newPayments.Append(payment.OrderId);
                newPayments.Append((char)20);
                newPayments.Append(payment.UniqueId);      // 4
                newPayments.Append((char)20);
                newPayments.Append(payment.TotalPaid);
                newPayments.Append((char)20);
                newPayments.Append(payment.Client.ClientName);   // 6
                newPayments.Append("|");
            }

            if (newPayments.Length > 0)
                try
                {
                    newPayments.Remove(newPayments.Length - 1, 1);
                    using (NetAccess netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("systemversion");
                        var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                        if (12 > Convert.ToInt32(version[0]))
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return false;
                        }
                        if (version[0] == "12" && Convert.ToInt32(version[1]) < 6)
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return false;
                        }
                        netaccess.WriteStringToNetwork("BackgroundNewPaymentsCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                        netaccess.WriteStringToNetwork(newPayments.ToString());
                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();

                        return true;
                    }
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

            return false;
        }

        private static bool SendNewOrders(List<Order> source)
        {// these are new orders created in the device

            StringBuilder newOrders = new StringBuilder();

            foreach (var order in source)
            {
                newOrders.Append(order.Client.ClientId);
                newOrders.Append((char)20);
                newOrders.Append(order.Date.Ticks);
                newOrders.Append((char)20);
                newOrders.Append(order.Latitude);     // 2
                newOrders.Append((char)20);
                newOrders.Append(order.Longitude);
                newOrders.Append((char)20);
                newOrders.Append((int)order.OrderType);
                newOrders.Append((char)20);
                newOrders.Append(order.PrintedOrderId ?? string.Empty);        // 5
                newOrders.Append((char)20);
                newOrders.Append(order.UniqueId);
                newOrders.Append((char)20);
                newOrders.Append(order.OrderTotalCost());
                newOrders.Append((char)20);
                newOrders.Append(order.Client.ClientName);          // 8
                if (Shipment.CurrentShipment != null)
                {
                    newOrders.Append((char)20);
                    newOrders.Append(Shipment.CurrentShipment.Id);          // 9
                }
                newOrders.Append("|");
            }

            if (newOrders.Length > 0)
                try
                {
                    newOrders.Remove(newOrders.Length - 1, 1);
                    using (NetAccess netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("systemversion");
                        var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                        if (12 > Convert.ToInt32(version[0]))
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return false;
                        }
                        if (version[0] == "12" && Convert.ToInt32(version[1]) < 2)
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return false;
                        }
                        netaccess.WriteStringToNetwork("BackgroundNewOrdersCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                        netaccess.WriteStringToNetwork(newOrders.ToString());
                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();

                        return true;
                    }
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

            return false;
        }

        private static void SenndLastKnowLocation()
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());
                    netaccess.WriteStringToNetwork("systemversion");
                    var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                    if (11 > Convert.ToInt32(version[0]))
                    {
                        netaccess.CloseConnection();
                        Running = false;
                        return;
                    }
                    if (version[0] == "11" && Convert.ToInt32(version[1]) < 8)
                    {
                        netaccess.CloseConnection();
                        Running = false;
                        return;
                    }
                    netaccess.WriteStringToNetwork("BackgroundLastPositionCommand");
                    netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());

                    string s = Config.LastLatitude.ToString(CultureInfo.InvariantCulture) + "," + Config.LastLongitude.ToString(CultureInfo.InvariantCulture) + "," +
                        DateTime.Now.Ticks.ToString();

                    if (Shipment.CurrentShipment != null)
                        s += "," + Shipment.CurrentShipment.Id;

                    netaccess.WriteStringToNetwork(s);
                    netaccess.WriteStringToNetwork("Goodbye");
                    lastLongitude = Config.LastLongitude;
                    lastLatitude = Config.LastLatitude;
                    Thread.Sleep(1000);
                    netaccess.CloseConnection();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public static void SendRoute(bool sendAll = false)
        {
            // send the route
            StringBuilder sendRouteCommandData = new StringBuilder();

            foreach (var route in RouteEx.Routes.Where(x => x.Id > 0))
            {
                if ((route.Closed && route.When > LastSentRoute) || sendAll)
                {
                    if (sendRouteCommandData.Length > 0)
                        sendRouteCommandData.Append('|');

                    string reship = "-1";
                    if (route.Order != null && route.Order.Reshipped)
                        reship = route.Order.ReasonId.ToString();
                    long clockin = 0;
                    if (route.Order != null)
                    {
                        var batch = Batch.List.FirstOrDefault(x => x.Id == route.Order.BatchId);
                        if (batch != null && batch.ClockedIn > DateTime.MinValue)
                            clockin = batch.ClockedIn.Ticks;
                    }
                    sendRouteCommandData.Append(string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}",
                                      (char)20,
                                      route.Id, //0
                                      route.Date.ToShortDateString(), //1
                                      Config.SalesmanId, //2 
                                      route.Client == null ? 0 : route.Client.ClientId, //3
                                      route.Order == null ? "0" : route.Order.UniqueId, //4
                                      route.Stop, //5
                                      route.Closed ? "1" : "0", //6
                                      route.FromDelivery ? "1" : "0", //7
                                      route.When.Ticks, //8
                                      route.Latitude, //9
                                      route.Longitude,
                                      reship,
                                      clockin
                                      )); //10
                }
            }

            if (sendRouteCommandData.Length > 0)
                try
                {
                    using (NetAccess netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("systemversion");
                        var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                        if (11 > Convert.ToInt32(version[0]))
                        {
                            netaccess.CloseConnection();

                            if (!sendAll)
                                Running = false;

                            return;
                        }
                        if (version[0] == "11" && Convert.ToInt32(version[1]) < 8)
                        {
                            netaccess.CloseConnection();

                            if (!sendAll)
                                Running = false;

                            return;
                        }
                        netaccess.WriteStringToNetwork("BackgroundRouteCompletionCommand");
                        netaccess.WriteStringToNetwork(sendRouteCommandData.ToString());
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();
                    }

                    LastSentRoute = DateTime.Now;
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }

        }

        public static void SendInventory(float currentInventoryTotal)
        {
            if (currentInventoryTotal == lastInventory)
                return;

            StringBuilder currentInventoryCommandData = new StringBuilder();

            foreach (var product in Product.Products.Where(x => Math.Round(x.CurrentInventory, Config.Round) != 0 || x.BeginigInventory != 0))
            {
                currentInventoryCommandData.Append(product.ProductId);
                currentInventoryCommandData.Append((char)20);
                currentInventoryCommandData.Append(product.CurrentInventory);
                currentInventoryCommandData.Append((char)20);
                currentInventoryCommandData.Append(product.BeginigInventory);
                currentInventoryCommandData.Append((char)20);
                currentInventoryCommandData.Append(Config.LastLatitude);
                currentInventoryCommandData.Append((char)20);
                currentInventoryCommandData.Append(Config.LastLongitude);

                if (Shipment.CurrentShipment != null)
                {
                    currentInventoryCommandData.Append((char)20);
                    currentInventoryCommandData.Append(Shipment.CurrentShipment.TruckId);
                }

                currentInventoryCommandData.Append("|");
            }


            if (currentInventoryCommandData.Length > 0)
                try
                {
                    currentInventoryCommandData.Remove(currentInventoryCommandData.Length - 1, 1);
                    using (NetAccess netaccess = new NetAccess())
                    {
                        netaccess.OpenConnection();
                        netaccess.WriteStringToNetwork("HELO");
                        netaccess.WriteStringToNetwork(Config.GetAuthString());
                        netaccess.WriteStringToNetwork("systemversion");
                        var version = netaccess.ReadStringFromNetwork().Split(new char[] { '.' });
                        if (11 > Convert.ToInt32(version[0]))
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return;
                        }
                        if (version[0] == "11" && Convert.ToInt32(version[1]) < 8)
                        {
                            netaccess.CloseConnection();
                            Running = false;
                            return;
                        }
                        netaccess.WriteStringToNetwork("BackgroundCurrentInventoryCommand");
                        netaccess.WriteStringToNetwork(Config.SalesmanId.ToString());
                        netaccess.WriteStringToNetwork(currentInventoryCommandData.ToString());
                        netaccess.WriteStringToNetwork("Goodbye");
                        Thread.Sleep(1000);
                        netaccess.CloseConnection();
                    }
                    lastInventory = currentInventoryTotal;
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }
        }

        private static string GenerateInventoryReport()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var product in Product.Products)
                if (Math.Round(product.CurrentInventory, Config.Round) != 0)
                {
                    if (sb.Length > 0)
                        sb.Append((char)10);
                    else
                    {
                        // write the header
                        sb.Append(Config.SalesmanId);
                        sb.Append("|");
                        sb.Append(DateTime.Now.Ticks);
                    }
                    sb.Append(string.Format("{1}{0}{2}{0}",
                        (char)10,
                        product.ProductId,
                        product.CurrentInventory,
                        product.BeginigInventory));
                }
            return sb.ToString();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void GetImages()
        {
            try
            {
                if (getImagesThread != null && getImagesThread.IsAlive)
                    getImagesThread.Abort();

                getImagesThread = new Thread(delegate ()
                {
                    try
                    {
                        DataAccess.UpdateProductImagesMap();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });
                getImagesThread.IsBackground = true;
                getImagesThread.Start();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void UpdateInvValuesFromOrders()
        {
            try
            {
                if (updateInvValuesFromOrdersThread != null && updateInvValuesFromOrdersThread.IsAlive)
                    updateInvValuesFromOrdersThread.Abort();

                updateInvValuesFromOrdersThread = new Thread(delegate ()
                {
                    try
                    {
                        Product.AdjustValuesFromOrder(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });
                updateInvValuesFromOrdersThread.IsBackground = true;
                updateInvValuesFromOrdersThread.Start();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void AssignInvToProduct(Product product)
        {
            try
            {
                var assignInvToProduct = new Thread(delegate ()
                {
                    try
                    {
                        if (product.ProductInv == null)
                            Logger.CreateLog("Unable to create invetory for product " + product.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });
                assignInvToProduct.IsBackground = true;
                assignInvToProduct.Start();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void AssignDefaultUomInBackground()
        {
            try
            {
                if (setProductDefaultUom != null && setProductDefaultUom.IsAlive)
                    setProductDefaultUom.Abort();

                setProductDefaultUom = new Thread(delegate ()
                {
                    try
                    {
                        foreach (var prod in Product.Products)
                        {
                            var defUom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == prod.UoMFamily && x.IsDefault);
                            if (defUom != null)
                                prod.DefaultUomName = defUom.Name;
                            else
                                prod.DefaultUomName = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });
                setProductDefaultUom.IsBackground = true;
                setProductDefaultUom.Start();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }
    }
}
