using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace LaceupMigration
{
    public static class Logger
    {
        static object syncObject = new object();

        static public void ClearFile()
        {
            try
            {
                if (File.Exists(Config.LogFile))
                    File.Delete(Config.LogFile);
            }
            catch { }
        }

        static private void MaintainFiles()
        {
            try
            {
                if (File.Exists(Config.LogFile))
                {
                    FileInfo fi = new FileInfo(Config.LogFile);
                    if (fi.Length > 500 * 1024)
                    {
                        //move it to the prev file
                        if (File.Exists(Config.LogFilePrevious))
                            File.Delete(Config.LogFilePrevious);
                        File.Move(Config.LogFile, Config.LogFilePrevious);
                    }
                }
            }
            catch { }
        }

        static public void CreateLog(Exception exception)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    lock (syncObject)
                    {
                        if (exception == null)
                            return;
                        try
                        {
                            MaintainFiles();
                            using (StreamWriter writer = new StreamWriter(Config.LogFile, true))
                            {
                                List<string> list = new List<string>();
                                LogException(exception, list);
                                foreach (string line in list)
                                    writer.WriteLine(line);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static public void CreateLog(string msg)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    lock (syncObject)
                    {
                        MaintainFiles();
                        using (StreamWriter writer = new StreamWriter(Config.LogFile, true))
                        {
                            writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "Date:{0} , MSG:{1}", DateTime.Now.ToString(), msg));
                            writer.Close();
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        private static void LogException(Exception e, List<string> messages)
        {
            try
            {
                if (e.InnerException != null)
                    LogException(e.InnerException, messages);
                messages.Add(string.Format(CultureInfo.InvariantCulture, "ERROR Date:{0} , MSG:{1}  ST:{2}", DateTime.Now.ToString(), e.Message, e.StackTrace));
            }
            catch { }
        }

        public static async Task SendLogFileAsync()
        {
            if (Config.SendLogByEmail)
                try
                {
                    await SendLogByEmailAsync();
                }
                catch
                {
                    await SendLogByNetworkAsync();
                }
            else
            {
                await SendLogByNetworkAsync();
            }
        }

        /// <summary>
        /// Sends log file via network connection. Matches Xamarin network send logic.
        /// </summary>
        private static async Task SendLogByNetworkAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    NetAccess access = new NetAccess();

                    access.OpenConnection("app.laceupsolutions.com", 9999);
                    access.WriteStringToNetwork("SendLogFile");

                    //fix for statwide
                    var serializedConfig = Config.SerializeConfig().Replace(System.Environment.NewLine, "<br>");
                    serializedConfig = serializedConfig.Replace("'", "");
                    serializedConfig = serializedConfig.Replace("'", "");

                    access.WriteStringToNetwork(serializedConfig);

                    access.SendFile(Config.LogFile);
                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();
                });
                // Success - no popup here, let caller handle it
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                // Re-throw exception so caller can handle error popup
                throw;
            }
        }

        private static async Task SendLogByEmailAsync()
        {
            try
            {
                // Determine platform-specific subject
                var platformSubject = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS Log File" : "Android Log File";
                
                var emailMessage = new EmailMessage
                {
                    To = new List<string> { "iphonelog@laceupsolutions.com" },
                    Subject = platformSubject,
                    Body = Config.SerializeConfig()
                };

                // Add log file content to body if file exists
                if (File.Exists(Config.LogFile))
                {
                    var logContent = File.ReadAllText(Config.LogFile);
                    var fullContent = Config.SerializeConfig() + logContent;
                    
                    // Truncate if too long (same as Xamarin)
                    if (fullContent.Length > 400000)
                    {
                        fullContent = Config.SerializeConfig() + fullContent.Substring((fullContent.Length / 4) * 3);
                    }
                    
                    emailMessage.Body = fullContent;
                }

                // Try to send email - this will open the email client
                // If it fails, the catch block will fall back to network send
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await Email.ComposeAsync(emailMessage);
                    }
                    catch
                    {
                        throw; // Re-throw to trigger fallback to network send
                    }
                });
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                throw; // Re-throw to trigger fallback to network send
            }
        }
    }
}

