using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        public static Version GetCommunicatorVersion()
        {
            using (NetAccess netaccess = new NetAccess())
            {
                try
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("systemversion");
                    var commVersion = netaccess.ReadStringFromNetwork();

                    Version version = null;

                    try
                    {
                        version = new Version(commVersion);
                    }
                    catch
                    {
                        throw new Exception("Invalid communicator version received: " + commVersion);
                    }

                    Config.CommunicatorVersion = version;
                    Config.SaveAppStatus();

                    return version;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
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

        #endregion

        
    }
}
