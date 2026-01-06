using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// Helper class that contains common functionality from Xamarin's LaceupActivity
    /// </summary>
    public static class LaceupActivityHelper
    {
        /// <summary>
        /// Sends the log file. Matches LaceupActivity.SendLog() logic exactly.
        /// </summary>
        public static void SendLog()
        {
            // Ensure log file exists (create empty if it doesn't) - same behavior as when Logger writes to it
            if (!File.Exists(Config.LogFile))
            {
                try
                {
                    var logDir = Path.GetDirectoryName(Config.LogFile);
                    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                        Directory.CreateDirectory(logDir);
                    File.Create(Config.LogFile).Close();
                }
                catch { }
            }

            if (Config.SendLogByEmail)
                try
                {
                    SendLogByEmail();
                }
                catch
                {
                    NetAccess access = new NetAccess();

                    access.OpenConnection("app.laceupsolutions.com", 9999);
                    access.WriteStringToNetwork("SendLogFile");

                    var serializedConfig = Config.SerializeConfig().Replace(System.Environment.NewLine, "<br>");
                    serializedConfig = serializedConfig.Replace("'", "");
                    serializedConfig = serializedConfig.Replace("'", "");

                    access.WriteStringToNetwork(serializedConfig);

                    access.SendFile(Config.LogFile);
                    access.WriteStringToNetwork("Goodbye");
                    Thread.Sleep(1000);
                    access.CloseConnection();

                    DialogHelper._dialogService.ShowAlertAsync("Log Sent", "Info", "OK");
                }
            else
                try
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

                    DialogHelper._dialogService.ShowAlertAsync("Log Sent", "Info", "OK");
                }
                catch (Exception ee)
                {
                    DialogHelper._dialogService.ShowAlertAsync("Error sending log file: " + ee.Message, "Alert", "OK");
                }
        }

        /// <summary>
        /// Sends log by email. Matches LaceupActivity.SendLogByEmail() logic.
        /// </summary>
        private static void SendLogByEmail()
        {
            try
            {
                var emailMessage = new EmailMessage
                {
                    To = new List<string> { "iphonelog@laceupsolutions.com" },
                    Subject = "Android Log File",
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Email.ComposeAsync(emailMessage).GetAwaiter().GetResult();
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

        /// <summary>
        /// Remote control functionality. Matches LaceupActivity.RemoteControl() logic exactly.
        /// </summary>
        public static async Task RemoteControl()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // Try to open TeamViewer app first (matches Xamarin Intent)
                    try
                    {
                        await Launcher.OpenAsync(new Uri("com.teamviewer.quicksupport.market"));
                        return;
                    }
                    catch
                    {
                        // If app not found, fall back to Play Store
                    }
                    
                    // Fall back to Play Store URL
                    await Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.teamviewer.quicksupport.market"));
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await Launcher.OpenAsync(new Uri("https://apps.apple.com/app/id661649585"));
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                // Try Play Store as final fallback
                try
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        await Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.teamviewer.quicksupport.market"));
                    }
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Export data functionality. Matches LaceupActivity.ExportData() logic.
        /// </summary>
        public static void ExportData(string subject = "")
        {
            DataProvider.ExportData(subject);
        }
    }
}

