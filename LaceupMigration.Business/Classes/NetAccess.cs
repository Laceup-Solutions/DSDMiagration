using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public sealed class NetAccess : IDisposable
    {
        bool OpenConnectionAsync(string connectTo, int remotePort, int i = 1)
        {
            try
            {
                string trimmed = connectTo.Trim();

                Task tsk = null;

                if (System.Text.RegularExpressions.Regex.Match(trimmed, @"\d+\.\d+\.\d+\.\d+").Success)
                    tsk = tcpc.ConnectAsync(trimmed, remotePort);
                else
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(trimmed);
                    IPAddress ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];

                    tsk = tcpc.ConnectAsync(ipAddress, remotePort);
                }

                if (tsk != null)
                    tsk.Wait(2000);

                if (!tcpc.Connected)
                {
                    if (i == 1)
                        OpenConnectionAsync(connectTo, remotePort, 0);
                    else
                        throw new Exception("Error opening connection." + Environment.NewLine + "Please check your internet connection and try again.");
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                var d = new Dictionary<string, string> {
                    { "IP", connectTo },
                    { "Port", remotePort.ToString () }
                };
                //Xamarin.Insights.Report(e, d);
                throw new Exception("Error opening connection." + Environment.NewLine + "Please check your internet connection and try again.", e);
            }
            return true;
        }

        TcpClient tcpc;

        public bool OpenConnection()
        {
            return OpenConnection(Config.ConnectionAddress, Config.Port);
        }

        public bool OpenConnection(string connectTo, int remotePort)
        {
            tcpc = new TcpClient();
            
            return OpenConnectionAsync(connectTo, remotePort);

            try
            {
                string trimmed = connectTo.Trim();
                if (System.Text.RegularExpressions.Regex.Match(trimmed, @"\d+\.\d+\.\d+\.\d+").Success)
                    tcpc.Connect(IPAddress.Parse(trimmed), remotePort);
                else
                    tcpc.Connect(trimmed, remotePort);
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                var d = new Dictionary<string, string> {
                    { "IP", connectTo },
                    { "Port", remotePort.ToString () }
                };
                //Xamarin.Insights.Report(e, d);
                throw new Exception("Error opening connection." + Environment.NewLine + "Please check your internet connection and try again.", e);
            }
            return true;
        }

        public void CloseConnection()
        {
            try
            {
                tcpc.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error closing connection", e);

            }
        }

        public void WriteStringToNetwork(string toWrite)
        {
            try
            {
                NetworkStream networkstream = tcpc.GetStream();
                foreach (char c in toWrite.ToCharArray())
                    networkstream.WriteByte(Convert.ToByte(c));
                networkstream.WriteByte(13);
                networkstream.WriteByte(10);
            }
            catch (Exception)
            {
                ////Logger.CreateLog(e);

            }
        }

        public string ReadStringFromNetwork()
        {
            StringBuilder buff;
            try
            {
                NetworkStream networkstream = tcpc.GetStream();
                buff = new StringBuilder(20);
                int ch;
                while ((ch = networkstream.ReadByte()) != -1)
                {
                    if (ch == 13)
                        continue;
                    if (ch == 10)
                    {
                        if (buff.ToString() == "Device Data Wiped")
                        {
                            DataAccess.SignOutDevice();
                            throw new Exception("Device Data Wiped");
                        }

                        return buff.ToString();
                    }
                    buff.Append(Convert.ToChar(ch));

                }
            }
            catch (Exception e)
            {
                ////Logger.CreateLog(e);
                throw new ConnectionException("\n==>Method: NetAccess.ReadStringFromNetwork, reading string :" + e.Message);

            }

            if (buff.ToString() == "Device Data Wiped")
            {
                DataAccess.SignOutDevice();
                throw new Exception("Device Data Wiped");
            }

            return buff.ToString();
        }

        public void SendFile(string fileName)
        {
            try
            {
                NetworkStream networkstream = tcpc.GetStream();
                int readed = 0;
                byte[] buff = new Byte[2048];
                using (var fstream = new FileStream(fileName, FileMode.Open))
                {
                    //send the file length
                    WriteStringToNetwork(fstream.Length.ToString(CultureInfo.InvariantCulture));
                    //writer.WriteLine( fstream.Length);
                    //writer.Flush();
                    while ((readed = fstream.Read(buff, 0, 2048)) > 0)
                        networkstream.Write(buff, 0, readed);
                    fstream.Close();
                }

                var confirm = ReadStringFromNetwork();
                if (confirm != "got it")
                    throw new Exception();
            }
            catch (Exception e)
            {
                ////Logger.CreateLog(e);
                throw new ConnectionException("\n==>Method: NetAccess.SendFile, sending this file :" + fileName + " :", e);
            }
        }

        public int ReceiveFile(string fileName)
        {
            try
            {
                var s = ReadStringFromNetwork();
                int size;

                if (!Int32.TryParse(s, out size))
                    throw new ConnectionException(s);

                size = Convert.ToInt32(s);
                if (size == 0)
                {
                    WriteStringToNetwork("got it");
                    return 0;
                }

                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    byte[] buff = new Byte[size > 40048 ? 40048 : size];
                    int readed = 0;
                    int readedt = 0;
                    int toread = size > 40048 ? 40048 : size;
                    NetworkStream networkstream = tcpc.GetStream();
                    while ((readedt = networkstream.Read(buff, 0, toread)) > 0)
                    {
                        readed += readedt;
                        fs.Write(buff, 0, readedt);
                        toread = (size - readed) > 40048 ? 40048 : size - readed;
                        if (toread == 0)
                            break;
                    }
                    fs.Close();
                    WriteStringToNetwork("got it");
                }
                return size;

            }
            catch (ConnectionException ee)
            {
                throw;
            }
            catch (Exception e)
            {
                ////Logger.CreateLog(e);
                throw new Exception("\n==>Method: NetAccess.ReceiveFile, receiving this file :" + fileName, e);
            }
        }

        public static string GetInvoiceDetails(int InvoiceId, int ClientId)
        {
            string tempFile = Path.Combine(Config.CodeBase, "tempDetails.cvs");

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            try
            {
                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("GetInvoiceDetailsCommand");
                    access.WriteStringToNetwork(InvoiceId.ToString());
                    access.WriteStringToNetwork(ClientId.ToString());
                    access.ReceiveFile(tempFile);
                    access.WriteStringToNetwork("GoodBye");
                    access.CloseConnection();
                }
                return tempFile;
            }
            catch(Exception ex)
            {
                Logger.CreateLog("Exception in GetInvoiceDetails in NetAccess ==>" + ex.ToString());
                return "";
            }
        }

        public static void SendSession(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ReceiveSessionCommand");
                    string confirm = access.ReadStringFromNetwork();

                    if (confirm != "GO")
                        throw new AuthorizationException(confirm);

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(fileName);


                    access.WriteStringToNetwork("GoodBye");
                    access.CloseConnection();

                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ConnectionException("Error sending session, one of the steps failed ==> ", e);
                }
            }

        }

        //helper method to send the order file, it's a wrapper to several calls to others methods of the
        //it will be in the child class as the rest of the helper methods
        public static void SendTheOrders(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("Orders");
                    string confirm = access.ReadStringFromNetwork();
                    //this is the confirmation
                    if (confirm != "GO")
                    {
                        Logger.CreateLog("Error sending orders " + string.Format("IP={0}, Port={1}, ID={2}, Response={3}",
                            Config.ConnectionAddress, Config.Port.ToString(), Config.GetAuthString(), confirm));

                        throw new AuthorizationException(confirm);
                    }
                    access.SendFile(fileName);

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
                    throw new ConnectionException("Error sending orders; one of the steps failed", e);
                }
            }
        }

        public static void SendThePayments(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("InvoicesAR");
                    access.SendFile(fileName);

                    access.WriteStringToNetwork("Goodbye");
                    access.CloseConnection();
                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                    throw new ConnectionException("Error sending orders; one of the steps failed", e);
                }
            }
        }

        public static void SendDayReport(string sessionId)
        {
            if (!SendSalesmanSessionsReport(sessionId))
            {
                #region Deprecated

                //if (Config.FirstDayClockIn == DateTime.MinValue)
                //    Config.FirstDayClockIn = DateTime.Now;

                //Config.DayClockOut = DateTime.Now;
                //Config.WorkDay = Config.DayClockOut.Subtract(Config.FirstDayClockIn);

                #endregion

                using (var access = new NetAccess())
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("DayReportCommand");

                    var text = Config.SalesmanId.ToString();

                    if (!string.IsNullOrEmpty(sessionId))
                        text += (char)20 + sessionId;

                    access.WriteStringToNetwork(text);

                    #region Deprecated

                    //access.WriteStringToNetwork(Config.FirstDayClockIn.Ticks.ToString());
                    //access.WriteStringToNetwork(Config.DayClockOut.Ticks.ToString());
                    //access.WriteStringToNetwork(Config.WorkDay.Ticks.ToString());

                    #endregion

                    DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                    DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                    var wholeday = lastClockOut.Subtract(startOfDay);

                    access.WriteStringToNetwork(startOfDay.Ticks.ToString());
                    access.WriteStringToNetwork(lastClockOut.Ticks.ToString());
                    access.WriteStringToNetwork(wholeday.Ticks.ToString());

                    if (!string.IsNullOrEmpty(sessionId))
                        access.WriteStringToNetwork(sessionId);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
            }
        }

        public static bool SendSalesmanSessionsReport(string sessionId)
        {
            if (!File.Exists(Config.SalesmanSessionsFile))
                return false;

            try
            {
                using (var access = new NetAccess())
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SalesmanSessionCommand");

                    var text = Config.SalesmanId.ToString();

                    if (!string.IsNullOrEmpty(sessionId))
                        text += (char)20 + sessionId;

                    access.WriteStringToNetwork(text);

                    access.SendFile(Config.SalesmanSessionsFile);
                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }

                SalesmanSession.Sessions.Clear();
                File.Delete(Config.SalesmanSessionsFile);

                return true;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return false;
            }
        }

        internal static void UpdateClientNote(Client client)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("UpdateClientNoteCommand");
                    access.WriteStringToNetwork(client.ClientId.ToString());
                    access.WriteStringToNetwork(client.Notes);

                    access.WriteStringToNetwork("GoodBye");
                    access.CloseConnection();
                }
                catch(Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                }
            }
        }

        public static void SendTheLoadOrder(string fileName, int salesmanId = 0)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("LoadOrderCommand");

                    if (salesmanId == 0)
                        salesmanId = Config.SalesmanId;

                    access.WriteStringToNetwork(salesmanId.ToString());
                    access.SendFile(fileName);

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

        public static void SendTheBuildQty(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("BuildToQtyCommand");
                    access.SendFile(fileName);

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

        public static void SendTheLeftOverInventory(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    bool verify = false;
                    string version = string.Empty;

                    try
                    {
                        access.OpenConnection();
                        access.WriteStringToNetwork("HELO");
                        access.WriteStringToNetwork(Config.GetAuthString());
                        access.WriteStringToNetwork("systemversion");
                        version = access.ReadStringFromNetwork();
                        access.CloseConnection();
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }

                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("LeftOverInventory");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(fileName);

                    if (!string.IsNullOrEmpty(version))
                    {
                        var vs = version.Split(new char[] { '.' });

                        if (Convert.ToInt32(vs[0]) > 12)
                            verify = true;
                        else if (vs[0] == "12" && Convert.ToInt32(vs[1]) > 7)
                            verify = true;
                    }

                    if (verify)
                    {
                        string confirm = access.ReadStringFromNetwork();

                        //this is the confirmation
                        if (confirm != "done")
                            throw new Exception("Error processing the LeftOver Inventory in the Communicator");
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

        public static void SendTheSignatures(string file)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SignatureCommand");
                    access.SendFile(file);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    throw;
                }
            }
        }

        internal static void SendClientNotes(string clientNotesStoreFile)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ClientNotesCommand");
                    access.SendFile(clientNotesStoreFile);

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

        public static void SendTheParLevel(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SetParLevelCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(fileName);

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

        public static void SendClientDailyParLevel()
        {
            using (var access = new NetAccess())
            {
                string fileName = string.Empty;

                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("systemversion");

                    var version = access.ReadStringFromNetwork();
                    var vs = version.Split(new char[] { '.' });

                    if (string.IsNullOrEmpty(version) || 12 > Convert.ToInt32(vs[0]) || (vs[0] == "12" && Convert.ToInt32(vs[1]) < 7))
                    {
                        fileName = Config.SavedDailyParLevelFile;
                    }
                    else
                    {
                        fileName = Path.GetTempFileName();
                        ClientDailyParLevel.Save(fileName);
                    }

                    access.WriteStringToNetwork("DailyParLevelSetCommand");
                    access.SendFile(fileName);
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

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

        public static void SendClientProdSort(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ClientProdSortCommand");
                    access.SendFile(fileName);
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

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

        public static void SendNewConsignment(string fileName)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("NewConsignmentUpdateCommand");
                    access.SendFile(fileName);
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());

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

        public static void GetCommunicatorVersion()
        {
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
                    return;
                }

                try
                {
                    netaccess.WriteStringToNetwork("systemversion");
                    var commVersion = netaccess.ReadStringFromNetwork();

                    commVersion = commVersion.ToLowerInvariant();

                    if (commVersion.StartsWith("invalid auth info") || commVersion.StartsWith("device authorization denied"))
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
                        netaccess.WriteStringToNetwork("systemversion");
                        commVersion = netaccess.ReadStringFromNetwork();
                    }

                    if (commVersion.StartsWith("Device Authorization Denied"))
                        throw new Exception(commVersion);

                    DataAccess.CommunicatorVersion = commVersion;
                    Config.SaveAppStatus();
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }
        }

        public static void SendOrdersImages(string dstFileZipped)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("OrderImagesCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(dstFileZipped);

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

        public static void SendOrdersPrint(string dstFileZipped)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("OrderZPLCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(dstFileZipped);

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


        public static void SendPaymentImages(string dstFileZipped)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("PaymentImagesCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(dstFileZipped);

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
                }
            }
        }

        public static void SendOrderHistory(string historyFile)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SaveOrderDetailHistoryCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(historyFile);

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

        public static void SaveClientDepartments(string tempFile, bool delete)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("SaveClientDepartmentsCommand");

                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(tempFile);

                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();

                    File.Delete(tempFile);

                    if (delete)
                    {
                        ClientDepartment.Clear();
                        if (File.Exists(Config.ClientDepartmentsFile))
                            File.Delete(Config.ClientDepartmentsFile);
                    }
                }
                catch (AuthorizationException)
                {
                    File.Delete(tempFile);
                    throw;
                }
                catch (Exception ex)
                {
                    File.Delete(tempFile);
                    Logger.CreateLog(ex);
                    throw;
                }
            }
        }

        public static void SendAssetTracking()
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();

                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ReceiveAssetTrackingCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.SendFile(Config.AssetTrackingFile);

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

        #region IDisposable Members

        public void Dispose()
        {
            if (tcpc != null)
            {
                tcpc.Close();
                tcpc = null;
            }
            GC.SuppressFinalize(this);
        }

        public static string GetLoadOrderDetailsInDate(DateTime fromDate, DateTime toDate)
        {
            string tempFile = Path.Combine(Config.CodeBase, "tempDetails.cvs");

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            try
            {
                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("GetLoadOrderDetailsInDateCommand");
                    access.WriteStringToNetwork(Config.SalesmanId.ToString());
                    access.WriteStringToNetwork(fromDate.Ticks.ToString());
                    access.WriteStringToNetwork(toDate.Ticks.ToString());
                    access.ReceiveFile(tempFile);
                    access.WriteStringToNetwork("GoodBye");
                    access.CloseConnection();
                }
                return tempFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Exception in GetLoadOrderDetailsInDate in NetAccess ==>" + ex.ToString());
                return "";
            }
        }

        public static void SendClientPictures(string tempPathFile, int clientId)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ClientPicturesCommand");
                    access.WriteStringToNetwork(clientId.ToString());
                    access.SendFile(tempPathFile);
                    access.WriteStringToNetwork("Goodbye");

                    Thread.Sleep(1000);

                    access.CloseConnection();

                }
                catch(AuthorizationException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                    throw;
                }
            }
        }

        #endregion

        public static void SendZPLPrinter(string dstFileZipped)
        {
            using (var access = new NetAccess())
            {
                try
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());
                    access.WriteStringToNetwork("ZplPrintedOrderCommand");
                    access.SendFile(dstFileZipped);
                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                }
                catch (AuthorizationException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex.ToString());
                    throw;
                }
            }
        }
    }
}
