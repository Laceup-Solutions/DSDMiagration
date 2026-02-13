using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using CoreBluetooth;
using CoreTelephony;
using ExternalAccessory;
using Foundation; using MessageUI;
using SystemConfiguration;
using UIKit;

using LaceupMigration;

[assembly: Dependency(typeof(InterfaceHelper))]

namespace LaceupMigration;

public class InterfaceHelper : IInterfaceHelper
{
    private string printCommand;
    private static EASession printerSession;
    private bool isScanningPrinters;
    private System.Threading.Timer writeRetryTimer;
    private int writeRetryCount = 0;
    private const int MAX_WRITE_RETRIES = 20; // Try for up to 10 seconds (20 * 500ms)

    private CBCentralManager centralManager;
    private List<PrinterDescription> discoveredDevices = new List<PrinterDescription>();

    public string GetOsVersion()
    {
        return UIDevice.CurrentDevice.SystemVersion;
    }

    public string GetDeviceModel()
    {
        return UIDevice.CurrentDevice.Model;
    }

    public string GetCarrier()
    {
        using (var info = new CTTelephonyNetworkInfo())
        {
            if (info.SubscriberCellularProvider != null)
                return info.SubscriberCellularProvider.CarrierName;
            else
                return "no subscriber selected";
        }
    }

    public void HideKeyboard()
    {
        UIKit.UIApplication.SharedApplication.KeyWindow?.EndEditing(true);
    }

    public string GetBrand()
    {
        return string.Empty;
    }

    public string GetDeviceId()
    {
        // IdentifierForVendor can be nil on first launch, TestFlight installs, or before app is fully active.
        // Calling .AsString() on null causes NRE and crashes release builds on physical devices.
        var vendorId = UIDevice.CurrentDevice.IdentifierForVendor;
        var vendorString = vendorId?.ToString() ?? Guid.NewGuid().ToString("N");
        return UIDevice.CurrentDevice.UserInterfaceIdiom.ToString() + vendorString;
    }

    public string GetLocale()
    {
        return NSLocale.CurrentLocale.LanguageCode + "-" + NSLocale.CurrentLocale.CountryCode;
    }

    public string GetSystemName()
    {
        return UIDevice.CurrentDevice.SystemName;
    }

    public string GetIdiom()
    {
        return UIDevice.CurrentDevice.UserInterfaceIdiom.ToString();
    }

    public string GetSSID()
    {
        NSDictionary dict;
        CaptiveNetwork.TryCopyCurrentNetworkInfo("en0", out dict);
        if (dict != null)
            if (dict.ContainsKey(CaptiveNetwork.NetworkInfoKeySSID))
                return dict[CaptiveNetwork.NetworkInfoKeySSID].ToString();
        return string.Empty;
    }

    public void StoreDefaults(string value, string key)
    {
        NSUserDefaults.StandardUserDefaults.SetString(value, key);
    }

    public void StoreDefaults(float value, string key)
    {
        NSUserDefaults.StandardUserDefaults.SetFloat(value, key);
    }

    public void StoreDefaults(bool value, string key)
    {
        NSUserDefaults.StandardUserDefaults.SetBool(value, key);
    }

    public void StoreDefaults(double value, string key)
    {
        NSUserDefaults.StandardUserDefaults.SetDouble(value, key);
    }

    public void StoreDefaults(int value, string key)
    {
        NSUserDefaults.StandardUserDefaults.SetInt(value, key);
    }

    public string GetDefaultString(string key, string defaultValue)
    {
        return NSUserDefaults.StandardUserDefaults.StringForKey(key);
    }

    public bool GetDefaultBool(string key, bool defaultValue)
    {
        return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
    }

    public float GetDefaultFloat(string key, float defaultValue)
    {
        return NSUserDefaults.StandardUserDefaults.FloatForKey(key);
    }

    public int GetDefaultInt(string key, int defaultValue)
    {
        return (int)NSUserDefaults.StandardUserDefaults.IntForKey(key);
    }

    public double GetDefaultDouble(string key, double defaultValue)
    {
        return NSUserDefaults.StandardUserDefaults.DoubleForKey(key);
    }

    public void SyncronizeDefaults()
    {
        NSUserDefaults.StandardUserDefaults.Synchronize();
    }

    //public void ViewPdf(string filepath)
    //{
    //    UIWindow window = UIApplication.SharedApplication.KeyWindow;

    //    var View = window.RootViewController;

    //    var viewer = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(filepath));
    //    viewer.PresentOpenInMenu(new RectangleF(0, -260, 320, 320), View.View, true);
    //}

    public void ViewPdf(string filepath)
    {
        var window = UIApplication.SharedApplication.KeyWindow;
        var viewController = window.RootViewController;

        // Ensure we get the topmost view controller
        while (viewController.PresentedViewController != null)
        {
            viewController = viewController.PresentedViewController;
        }

        // Create the PDF viewer
        var viewer = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(filepath));

        // Present the document viewer
        viewer.PresentOpenInMenu(
            new CoreGraphics.CGRect(0, 0, viewController.View.Frame.Width, viewController.View.Frame.Height),
            viewController.View, true);
    }

    public void CheckForIcon(string value)
    {
        try
        {
            if (!string.IsNullOrEmpty(value))
                UIApplication.SharedApplication.SetAlternateIconName(value, null);
            else
                UIApplication.SharedApplication.SetAlternateIconName(null, null);
        }
        catch
        {
        }
    }

    public void PrintPdf(string pdf)
    {
        var printInfo = UIKit.UIPrintInfo.PrintInfo;
        printInfo.Duplex = UIKit.UIPrintInfoDuplex.LongEdge;
        printInfo.OutputType = UIKit.UIPrintInfoOutputType.General;
        printInfo.JobName = "Print Order";
        var printer = UIKit.UIPrintInteractionController.SharedPrintController;
        printer.PrintInfo = printInfo;
        printer.PrintingItem = Foundation.NSData.FromFile(pdf);
        printer.ShowsPageRange = true;
        printer.Present(true, (handler, completed, err) =>
        {
            if (!completed && err != null)
            {
            }
        });
    }

    public void SendReportByEmail(string pdfFile)
    {
        if (string.IsNullOrEmpty(pdfFile))
        {
            return;
        }

        try
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;

            // Ensure we get the topmost view controller
            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            if (MFMailComposeViewController.CanSendMail)
            {
                var mailComposer = new MFMailComposeViewController();
                mailComposer.SetSubject("Report Attached");
                mailComposer.SetMessageBody("", false);

                var fileData = NSData.FromFile(pdfFile);
                if (fileData != null)
                {
                    mailComposer.AddAttachmentData(fileData, "application/pdf", Path.GetFileName(pdfFile));
                }

                mailComposer.Finished += (sender, e) =>
                {
                    mailComposer.DismissViewController(true, null);
                };

                viewController.PresentViewController(mailComposer, true, null);
            }
            else
            {
                // Fallback to UIDocumentInteractionController if mail is not configured
                var viewer = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(pdfFile));
                viewer.PresentOptionsMenu(
                    new CoreGraphics.CGRect(0, 0, viewController.View.Frame.Width, viewController.View.Frame.Height),
                    viewController.View, true);
            }
        }
        catch (Exception ex)
        {
            Logger.CreateLog("Error sending report by email ==>" + ex.ToString());
        }
    }

    // NOTE: Android is prioritized for platform-specific implementations
    // This iOS implementation is basic - Android has full feature parity with Xamarin
    public void SendOrderByEmail(string pdfFile, string subject, string body, List<string> toAddresses)
    {
        if (string.IsNullOrEmpty(pdfFile))
        {
            return;
        }

        try
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;

            // Ensure we get the topmost view controller
            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            if (MFMailComposeViewController.CanSendMail)
            {
                var mailComposer = new MFMailComposeViewController();
                mailComposer.SetSubject(subject ?? "Invoice Attached");
                mailComposer.SetMessageBody(body ?? "", false);

                var fileData = NSData.FromFile(pdfFile);
                if (fileData != null)
                {
                    mailComposer.AddAttachmentData(fileData, "application/pdf", Path.GetFileName(pdfFile));
                }

                if (toAddresses != null && toAddresses.Count > 0)
                {
                    var validAddresses = toAddresses.Where(e => !string.IsNullOrEmpty(e)).ToArray();
                    if (validAddresses.Length > 0)
                    {
                        mailComposer.SetToRecipients(validAddresses);
                    }
                }

                mailComposer.Finished += (sender, e) =>
                {
                    mailComposer.DismissViewController(true, null);
                };

                viewController.PresentViewController(mailComposer, true, null);
            }
            else
            {
                // Fallback to UIDocumentInteractionController if mail is not configured
                var viewer = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(pdfFile));
                viewer.PresentOptionsMenu(
                    new CoreGraphics.CGRect(0, 0, viewController.View.Frame.Width, viewController.View.Frame.Height),
                    viewController.View, true);
            }
        }
        catch (Exception ex)
        {
            Logger.CreateLog("Error sending order by email ==>" + ex.ToString());
        }
    }

    public void SubscribeToTopic(string topic)
    {
    }

    public void UnsubscribeToTopic(string topic)
    {
    }

    public int PrintProcess1(List<Order> orders)
    {
        Debug.WriteLine("[PrintProcess1] Starting PrintProcess1");
        try
        {
            var printers = AvailablePrinters();
            Debug.WriteLine($"[PrintProcess1] Found {printers?.Count ?? 0} printers");
            
            switch (printers.Count)
            {
                case 0:
                    Debug.WriteLine("[PrintProcess1] No printers found, returning 0");
                    return 0;
                case 1:
                    PrinterProvider.PrinterAddress = printers[0].Address;
                    Debug.WriteLine($"[PrintProcess1] Auto-selected printer: {printers[0].Name} (Address: {printers[0].Address})");
                    return 1;
                default:
                    Debug.WriteLine($"[PrintProcess1] Multiple printers found ({printers.Count}), returning 2 for user selection");
                    return 2;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PrintProcess1] ERROR: {ex.Message}");
            Debug.WriteLine($"[PrintProcess1] StackTrace: {ex.StackTrace}");
            return 0;
        }
    }

    public void PrintedSelected(string printer)
    {
        Debug.WriteLine($"[PrintedSelected] Called with printer name: {printer ?? "null"}");
        try
        {
            var printers = AvailablePrinters();
            Debug.WriteLine($"[PrintedSelected] Available printers count: {printers?.Count ?? 0}");
            
            var selectedPrinter = printers.FirstOrDefault(x => x.Name == printer);
            if (selectedPrinter != null)
            {
                PrinterProvider.PrinterAddress = selectedPrinter.Address;
                Debug.WriteLine($"[PrintedSelected] Printer selected successfully: {selectedPrinter.Name} (Address: {selectedPrinter.Address})");
            }
            else
            {
                Debug.WriteLine($"[PrintedSelected] WARNING: Printer '{printer}' not found in available printers list");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PrintedSelected] ERROR: {ex.Message}");
            Debug.WriteLine($"[PrintedSelected] StackTrace: {ex.StackTrace}");
        }
    }

    public bool Print(List<Order> orders)
    {
        Debug.WriteLine("[Print] Starting Print method");
        try
        {
            if (orders == null || orders.Count == 0)
            {
                Debug.WriteLine("[Print] ERROR: orders list is null or empty");
                return false;
            }

            Debug.WriteLine($"[Print] Processing {orders.Count} order(s)");
            IPrinter printer = PrinterProvider.CurrentPrinter();
            Debug.WriteLine($"[Print] Printer instance obtained: {printer?.GetType().Name ?? "null"}");
            
            var order = orders.FirstOrDefault();
            if (order == null)
            {
                Debug.WriteLine("[Print] ERROR: No order found in orders list");
                return false;
            }

            Debug.WriteLine($"[Print] Calling PrintOrder for order ID: {order.OrderId}");
            var result = printer.PrintOrder(order, false);
            Debug.WriteLine($"[Print] PrintOrder returned: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Print] ERROR: {ex.Message}");
            Debug.WriteLine($"[Print] StackTrace: {ex.StackTrace}");
            Logger.CreateLog("Error in Print method: " + ex.ToString());
            return false;
        }
    }

    public List<string> GetAllPrinters()
    {
        Debug.WriteLine("[GetAllPrinters] Starting GetAllPrinters");
        try
        {
            var stringPrinters = new List<string>();
            var printers = AvailablePrinters();
            Debug.WriteLine($"[GetAllPrinters] AvailablePrinters returned {printers?.Count ?? 0} printers");

            foreach (var printer in printers)
            {
                if (!string.IsNullOrEmpty(printer.Name))
                {
                    stringPrinters.Add(printer.Name);
                    Debug.WriteLine($"[GetAllPrinters] Added printer: {printer.Name}");
                }
            }

            Debug.WriteLine($"[GetAllPrinters] Returning {stringPrinters.Count} printer names");
            return stringPrinters;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetAllPrinters] ERROR: {ex.Message}");
            Debug.WriteLine($"[GetAllPrinters] StackTrace: {ex.StackTrace}");
            return new List<string>();
        }
    }

    public void ExitApplication()
    {
        Thread.CurrentThread.Abort();
    }

    private void DiscoverPrinters()
    {
        Debug.WriteLine("[DiscoverPrinters] Starting printer discovery");
        try
        {
            // Get currently connected accessories
            // EAAccessoryManager automatically discovers accessories when protocols are declared in Info.plist
            var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;
            Debug.WriteLine($"[DiscoverPrinters] EAAccessoryManager returned {accessoryList?.Length ?? 0} connected accessories");
            
            discoveredDevices.Clear();

            Logger.CreateLog($"Discovering printers. Found {accessoryList.Length} connected accessories");
            Debug.WriteLine($"[DiscoverPrinters] Cleared discoveredDevices list");

            if (accessoryList != null && accessoryList.Length > 0)
            {
                foreach (var accessory in accessoryList)
                {
                    Debug.WriteLine($"[DiscoverPrinters] Checking accessory: {accessory.Name ?? "null"}");
                    Debug.WriteLine($"[DiscoverPrinters] ProtocolStrings: {(accessory.ProtocolStrings != null ? string.Join(", ", accessory.ProtocolStrings) : "null")}");
                    
                    // Filter for Zebra printers or other supported printer protocols
                    if (accessory.ProtocolStrings != null && 
                        (accessory.ProtocolStrings.Contains("com.zebra.rawport") ||
                         accessory.ProtocolStrings.Contains("com.woosim.wspr240")))
                    {
                        var printerDesc = new PrinterDescription
                        {
                            Address = accessory.Name,
                            Name = accessory.Name
                        };
                        discoveredDevices.Add(printerDesc);
                        Debug.WriteLine($"[DiscoverPrinters] Found printer: {accessory.Name}");
                        Logger.CreateLog($"Found printer: {accessory.Name}");
                    }
                    else
                    {
                        Debug.WriteLine($"[DiscoverPrinters] Accessory '{accessory.Name}' does not match printer protocols");
                    }
                }
            }
            else
            {
                Debug.WriteLine("[DiscoverPrinters] WARNING: No accessories found or accessoryList is null");
            }

            // Trigger discovery finished event
            Debug.WriteLine($"[DiscoverPrinters] Discovery complete. Found {discoveredDevices.Count} printers");
            OnDiscoveryFinished(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiscoverPrinters] ERROR: {ex.Message}");
            Debug.WriteLine($"[DiscoverPrinters] StackTrace: {ex.StackTrace}");
            Logger.CreateLog("Error discovering printers: " + ex.ToString());
        }
    }

    private void OnDiscoveryFinished(object sender, EventArgs e)
    {
        // Handle discovery finished - printers are now in discoveredDevices list
        Debug.WriteLine($"[OnDiscoveryFinished] Discovery finished. Found {discoveredDevices.Count} printers");
        Logger.CreateLog($"Printer discovery finished. Found {discoveredDevices.Count} printers");
    }

    public void PrintIt(string printingString)
    {
        Debug.WriteLine("[PrintIt] ========== PrintIt called ==========");
        Debug.WriteLine($"[PrintIt] Printing string length: {printingString?.Length ?? 0}");
        Debug.WriteLine($"[PrintIt] Printing string preview: {(printingString?.Length > 100 ? printingString.Substring(0, 100) + "..." : printingString ?? "null")}");
        
        try
        {
            printCommand = printingString + Environment.NewLine + Environment.NewLine;
            Debug.WriteLine($"[PrintIt] Initial printCommand length: {printCommand?.Length ?? 0}");

            if (printingString.Contains((char)160))
            {
                printingString = printingString.Replace(((char)160).ToString(), string.Empty);
                Debug.WriteLine("[PrintIt] Removed char 160 from printing string");
            }

            if (Config.PrintInvoiceAsReceipt)
            {
                printingString = printingString.Replace("Invoice", "Receipt");
                printingString = printingString.Replace("INVOICE", "RECEIPT");
                Debug.WriteLine("[PrintIt] Replaced Invoice with Receipt");
            }

            // Update printCommand with cleaned string
            printCommand = printingString + Environment.NewLine + Environment.NewLine;
            Debug.WriteLine($"[PrintIt] Final printCommand length: {printCommand?.Length ?? 0}");

            // Discover printers first
            Debug.WriteLine("[PrintIt] Calling DiscoverPrinters()");
            DiscoverPrinters();

            Debug.WriteLine($"[PrintIt] Current PrinterAddress: {PrinterProvider.PrinterAddress ?? "null"}");
            if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
            {
                Debug.WriteLine("[PrintIt] PrinterAddress is empty, attempting auto-select");
                // Try to auto-select if only one printer available
                var printers = AvailablePrinters();
                Debug.WriteLine($"[PrintIt] AvailablePrinters returned {printers?.Count ?? 0} printers");
                
                if (printers != null && printers.Count == 1)
                {
                    PrinterProvider.PrinterAddress = printers[0].Address;
                    Debug.WriteLine($"[PrintIt] Auto-selected printer: {printers[0].Name} (Address: {printers[0].Address})");
                }
                else
                {
                    Debug.WriteLine($"[PrintIt] ERROR: Cannot auto-select printer. Count: {printers?.Count ?? 0}");
                    throw new InvalidOperationException("No valid printer selected");
                }
            }

            Debug.WriteLine($"[PrintIt] printerSession is null: {printerSession == null}");
            if (printerSession == null)
            {
                Debug.WriteLine("[PrintIt] Creating new printer session");
                CreatePrinterSession();
            }
            else
            {
                // Check if session is still valid
                var streamStatus = printerSession.OutputStream?.Status;
                Debug.WriteLine($"[PrintIt] Existing session found. OutputStream status: {streamStatus}");
                
                if (printerSession.OutputStream?.Status != NSStreamStatus.Open)
                {
                    Debug.WriteLine("[PrintIt] Session exists but stream is not open, creating new session");
                    Logger.CreateLog("Session exists but stream is not open, creating new session");
                    DisposeSession();
                    CreatePrinterSession();
                }
                else
                {
                    Debug.WriteLine("[PrintIt] Session exists and stream is open, using existing session");
                    Logger.CreateLog("Session exists, using existing session");
                    LogSessionStatus();
                    Debug.WriteLine("[PrintIt] Calling HandleEvent with HasSpaceAvailable");
                    HandleEvent(printerSession.OutputStream, NSStreamEvent.HasSpaceAvailable);
                }
            }
            
            Debug.WriteLine("[PrintIt] ========== PrintIt completed successfully ==========");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PrintIt] ========== ERROR in PrintIt ==========");
            Debug.WriteLine($"[PrintIt] Error message: {ex.Message}");
            Debug.WriteLine($"[PrintIt] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"[PrintIt] InnerException: {ex.InnerException.Message}");
            }
            Logger.CreateLog("Error in PrintIt: " + ex.ToString());
            throw;
        }
    }

    void CreatePrinterSession()
    {
        Debug.WriteLine("[CreatePrinterSession] ========== Creating printer session ==========");
        Logger.CreateLog("Creating session");
        
        try
        {
            var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;
            Debug.WriteLine($"[CreatePrinterSession] Connected accessories count: {accessoryList?.Length ?? 0}");
            Debug.WriteLine($"[CreatePrinterSession] Looking for printer with address: {PrinterProvider.PrinterAddress ?? "null"}");

            EAAccessory accessory = null;

            // First try to find by PrinterAddress if set
            if (!string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
            {
                Debug.WriteLine($"[CreatePrinterSession] Searching for printer by address: {PrinterProvider.PrinterAddress}");
                accessory = accessoryList.FirstOrDefault(x => x.Name == PrinterProvider.PrinterAddress);
                if (accessory != null)
                {
                    Debug.WriteLine($"[CreatePrinterSession] Found printer by address: {PrinterProvider.PrinterAddress}");
                    Logger.CreateLog($"Found printer by address: {PrinterProvider.PrinterAddress}");
                }
                else
                {
                    Debug.WriteLine($"[CreatePrinterSession] Printer not found by address: {PrinterProvider.PrinterAddress}");
                }
            }

            // If not found by address, try to find any Zebra printer
            if (accessory == null)
            {
                Debug.WriteLine("[CreatePrinterSession] Searching for any Zebra printer (com.zebra.rawport)");
                foreach (var obj in accessoryList)
                {
                    EAAccessory a = obj;
                    Debug.WriteLine($"[CreatePrinterSession] Checking accessory: {a.Name}, Protocols: {(a.ProtocolStrings != null ? string.Join(", ", a.ProtocolStrings) : "null")}");
                    
                    if (a.ProtocolStrings != null && a.ProtocolStrings.Contains("com.zebra.rawport"))
                    {
                        accessory = obj;
                        Debug.WriteLine($"[CreatePrinterSession] Found Zebra printer: {accessory.Name}");
                        Logger.CreateLog($"Found Zebra printer: {accessory.Name}");
                        break;
                    }
                }
            }

            // Try Woosim protocol as fallback
            if (accessory == null)
            {
                Debug.WriteLine("[CreatePrinterSession] Searching for Woosim printer (com.woosim.wspr240)");
                foreach (var obj in accessoryList)
                {
                    EAAccessory a = obj;
                    if (a.ProtocolStrings != null && a.ProtocolStrings.Contains("com.woosim.wspr240"))
                    {
                        accessory = obj;
                        Debug.WriteLine($"[CreatePrinterSession] Found Woosim printer: {accessory.Name}");
                        Logger.CreateLog($"Found Woosim printer: {accessory.Name}");
                        break;
                    }
                }
            }

            if (accessory == null)
            {
                Debug.WriteLine($"[CreatePrinterSession] ERROR: Printer not found. Connected accessories: {accessoryList?.Length ?? 0}");
                Logger.CreateLog("Printer not found. Connected accessories: " + accessoryList.Length);
                throw new Exception("no printer found");
            }

            Debug.WriteLine($"[CreatePrinterSession] Setting accessory delegate for: {accessory.Name}");
            accessory.Delegate = new EAAccessoryDelegateHandler(this);
            
            // Determine protocol string
            string protocolString = "com.zebra.rawport";
            if (accessory.ProtocolStrings != null && accessory.ProtocolStrings.Contains("com.woosim.wspr240"))
            {
                protocolString = "com.woosim.wspr240";
            }
            Debug.WriteLine($"[CreatePrinterSession] Using protocol: {protocolString}");

            Debug.WriteLine($"[CreatePrinterSession] Creating EASession with protocol: {protocolString}");
            printerSession = new EASession(accessory, protocolString);
            var mysession = printerSession;
            
            if (mysession == null)
            {
                Debug.WriteLine("[CreatePrinterSession] ERROR: EASession creation returned null");
                throw new Exception("Failed to create EASession - returned null");
            }
            
            Debug.WriteLine("[CreatePrinterSession] EASession created successfully");
            
            if (mysession.OutputStream == null)
            {
                Debug.WriteLine("[CreatePrinterSession] ERROR: OutputStream is null");
                Logger.CreateLog("output is null");
                throw new Exception("Failed to create output stream");
            }
            else
            {
                Debug.WriteLine("[CreatePrinterSession] OutputStream created successfully");
                Logger.CreateLog("output not null");
            }

        // Set delegate to handle stream events
        Debug.WriteLine("[CreatePrinterSession] Setting OutputStream delegate and scheduling");
        var outputDelegate = new NSStreamDelegateHandler(this);
        mysession.OutputStream.Delegate = outputDelegate;
        mysession.OutputStream.Schedule(NSRunLoop.Main, NSRunLoopMode.Default);
        
        Debug.WriteLine("[CreatePrinterSession] Opening OutputStream");
        mysession.OutputStream.Open();
        Debug.WriteLine($"[CreatePrinterSession] OutputStream opened. Status: {mysession.OutputStream.Status}");
        
        // Give the stream a moment to fully initialize
        Thread.Sleep(200);

            if (mysession.InputStream == null)
            {
                Debug.WriteLine("[CreatePrinterSession] WARNING: InputStream is null");
                Logger.CreateLog("InputStream is null");
            }
            else
            {
                Debug.WriteLine("[CreatePrinterSession] InputStream created successfully");
                Logger.CreateLog("InputStream not null");
                Debug.WriteLine("[CreatePrinterSession] Setting InputStream delegate and scheduling");
                mysession.InputStream.Delegate = new NSStreamDelegateHandler(this);
                mysession.InputStream.Schedule(NSRunLoop.Main, NSRunLoopMode.Default);
                
                Debug.WriteLine("[CreatePrinterSession] Opening InputStream");
                mysession.InputStream.Open();
                Debug.WriteLine($"[CreatePrinterSession] InputStream opened. Status: {mysession.InputStream.Status}");
            }

            // Wait a bit for stream to be ready
            Debug.WriteLine("[CreatePrinterSession] Waiting 1000ms for stream to be ready");
            Thread.Sleep(1000);
            
            Debug.WriteLine($"[CreatePrinterSession] OutputStream status after wait: {mysession.OutputStream.Status}");
            var hasSpace = mysession.OutputStream.HasSpaceAvailable();
            Debug.WriteLine($"[CreatePrinterSession] OutputStream HasSpaceAvailable: {hasSpace}");
            
            // Reset retry counter
            writeRetryCount = 0;
            
            // Try to write immediately if space is available
            if (hasSpace && printCommand != null)
            {
                Debug.WriteLine("[CreatePrinterSession] Stream has space, calling HandleEvent with HasSpaceAvailable");
                HandleEvent(printerSession.OutputStream, NSStreamEvent.HasSpaceAvailable);
            }
            else if (printCommand != null)
            {
                Debug.WriteLine("[CreatePrinterSession] Stream does not have space yet. Starting retry timer.");
                // Start a timer to periodically check for space availability
                // This handles cases where the delegate event doesn't fire automatically
                writeRetryTimer = new System.Threading.Timer(CheckAndWriteData, null, 500, 500);
            }
            else
            {
                Debug.WriteLine("[CreatePrinterSession] WARNING: printCommand is null");
            }
            
            Debug.WriteLine("[CreatePrinterSession] ========== Printer session created successfully ==========");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreatePrinterSession] ========== ERROR creating session ==========");
            Debug.WriteLine($"[CreatePrinterSession] Error: {ex.Message}");
            Debug.WriteLine($"[CreatePrinterSession] StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    class NSStreamDelegateHandler : NSStreamDelegate
    {
        private readonly InterfaceHelper helper;

        public NSStreamDelegateHandler(InterfaceHelper helper)
        {
            this.helper = helper;
        }

        public override void HandleEvent(NSStream theStream, NSStreamEvent streamEvent)
        {
            helper.HandleEvent(theStream, streamEvent);
        }
    }

    class EAAccessoryDelegateHandler : EAAccessoryDelegate
    {
        private readonly InterfaceHelper helper;

        public EAAccessoryDelegateHandler(InterfaceHelper helper = null)
        {
            this.helper = helper;
        }

        public override void Disconnected(EAAccessory accessory)
        {
            Logger.CreateLog("device disconnected");
            if (helper != null)
            {
                helper.DisposeSession();
            }
            else
            {
                // Fallback if helper is not set
                DisposeSessionStatic();
            }
            accessory.Delegate = null;
        }

        private void DisposeSessionStatic()
        {
            Logger.CreateLog("Disposing of the session (static)");
            if (printerSession == null)
            {
                Logger.CreateLog("null session");
                return;
            }

            try
            {
                if (printerSession.OutputStream != null)
                {
                    if (printerSession.OutputStream.Status == NSStreamStatus.Open)
                    {
                        printerSession.OutputStream.Close();
                    }
                    printerSession.OutputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                    printerSession.OutputStream.Delegate = null;
                }

                if (printerSession.InputStream != null)
                {
                    if (printerSession.InputStream.Status == NSStreamStatus.Open)
                    {
                        printerSession.InputStream.Close();
                    }
                    printerSession.InputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                    printerSession.InputStream.Delegate = null;
                }

                printerSession.Dispose();

                printerSession = null;
                Logger.CreateLog("Session disposed successfully");
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }
    }

    private void CheckAndWriteData(object state)
    {
        try
        {
            writeRetryCount++;
            Debug.WriteLine($"[CheckAndWriteData] Retry attempt {writeRetryCount}/{MAX_WRITE_RETRIES}");
            
            if (printerSession == null || printerSession.OutputStream == null)
            {
                Debug.WriteLine("[CheckAndWriteData] Session or stream is null, stopping timer");
                StopRetryTimer();
                return;
            }
            
            if (printCommand == null)
            {
                Debug.WriteLine("[CheckAndWriteData] printCommand is null, stopping timer");
                StopRetryTimer();
                return;
            }
            
            var streamStatus = printerSession.OutputStream.Status;
            Debug.WriteLine($"[CheckAndWriteData] Stream status: {streamStatus}");
            var hasSpace = printerSession.OutputStream.HasSpaceAvailable();
            Debug.WriteLine($"[CheckAndWriteData] HasSpaceAvailable: {hasSpace}");
            
            // Try writing even if HasSpaceAvailable is false after a few retries
            // Sometimes the stream can accept data even when this returns false
            if (hasSpace || writeRetryCount >= 3)
            {
                if (!hasSpace && writeRetryCount >= 3)
                {
                    Debug.WriteLine($"[CheckAndWriteData] HasSpaceAvailable is false but attempting write anyway (retry {writeRetryCount})");
                }
                else
                {
                    Debug.WriteLine("[CheckAndWriteData] Stream has space, attempting to write");
                }
                
                StopRetryTimer();
                // Execute on main thread to ensure proper run loop handling
                Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        HandleEvent(printerSession.OutputStream, NSStreamEvent.HasSpaceAvailable);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[CheckAndWriteData] ERROR in HandleEvent: {ex.Message}");
                    }
                });
            }
            else if (writeRetryCount >= MAX_WRITE_RETRIES)
            {
                Debug.WriteLine($"[CheckAndWriteData] Max retries reached ({MAX_WRITE_RETRIES}), stopping timer");
                StopRetryTimer();
                Debug.WriteLine("[CheckAndWriteData] ERROR: Could not write data - stream never became available");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CheckAndWriteData] ERROR: {ex.Message}");
            Debug.WriteLine($"[CheckAndWriteData] StackTrace: {ex.StackTrace}");
            StopRetryTimer();
        }
    }
    
    private void StopRetryTimer()
    {
        if (writeRetryTimer != null)
        {
            Debug.WriteLine("[StopRetryTimer] Stopping write retry timer");
            writeRetryTimer.Dispose();
            writeRetryTimer = null;
            writeRetryCount = 0;
        }
    }

    public void DisposeSession()
    {
        Debug.WriteLine("[DisposeSession] ========== Disposing printer session ==========");
        Logger.CreateLog("Disposing of the session");
        
        // Stop retry timer if running
        StopRetryTimer();
        
        if (printerSession == null)
        {
            Debug.WriteLine("[DisposeSession] printerSession is null, nothing to dispose");
            Logger.CreateLog("null session");
            return;
        }

        try
        {
            Debug.WriteLine("[DisposeSession] Disposing OutputStream");
            if (printerSession.OutputStream != null)
            {
                var outputStatus = printerSession.OutputStream.Status;
                Debug.WriteLine($"[DisposeSession] OutputStream status: {outputStatus}");
                
                if (printerSession.OutputStream.Status == NSStreamStatus.Open)
                {
                    Debug.WriteLine("[DisposeSession] Closing OutputStream");
                    printerSession.OutputStream.Close();
                }
                Debug.WriteLine("[DisposeSession] Unscheduling OutputStream");
                printerSession.OutputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                printerSession.OutputStream.Delegate = null;
            }

            Debug.WriteLine("[DisposeSession] Disposing InputStream");
            if (printerSession.InputStream != null)
            {
                var inputStatus = printerSession.InputStream.Status;
                Debug.WriteLine($"[DisposeSession] InputStream status: {inputStatus}");
                
                if (printerSession.InputStream.Status == NSStreamStatus.Open)
                {
                    Debug.WriteLine("[DisposeSession] Closing InputStream");
                    printerSession.InputStream.Close();
                }
                Debug.WriteLine("[DisposeSession] Unscheduling InputStream");
                printerSession.InputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                printerSession.InputStream.Delegate = null;
            }

            Debug.WriteLine("[DisposeSession] Disposing EASession");
            printerSession.Dispose();

            printerSession = null;
            Debug.WriteLine("[DisposeSession] ========== Session disposed successfully ==========");
            Logger.CreateLog("Session disposed successfully");
        }
        catch (Exception ee)
        {
            Debug.WriteLine($"[DisposeSession] ERROR: {ee.Message}");
            Debug.WriteLine($"[DisposeSession] StackTrace: {ee.StackTrace}");
            Logger.CreateLog(ee);
        }
    }

    void LogSessionStatus()
    {
        Debug.WriteLine("[LogSessionStatus] ========== Logging session status ==========");
        
        if (printerSession == null)
        {
            Debug.WriteLine("[LogSessionStatus] printerSession is null");
            return;
        }
        
        if (printerSession.OutputStream == null)
        {
            Debug.WriteLine("[LogSessionStatus] OutputStream is null");
            Logger.CreateLog("output is null");
        }
        else
        {
            Debug.WriteLine($"[LogSessionStatus] OutputStream status: {printerSession.OutputStream.Status}");
            Debug.WriteLine($"[LogSessionStatus] OutputStream HasSpaceAvailable: {printerSession.OutputStream.HasSpaceAvailable()}");
            if (printerSession.OutputStream.Error != null)
            {
                Debug.WriteLine($"[LogSessionStatus] OutputStream error: {printerSession.OutputStream.Error.LocalizedDescription}");
            }
        }
        
        if (printerSession.InputStream == null)
        {
            Debug.WriteLine("[LogSessionStatus] InputStream is null");
        }
        else
        {
            Debug.WriteLine($"[LogSessionStatus] InputStream status: {printerSession.InputStream.Status}");
            Debug.WriteLine($"[LogSessionStatus] InputStream HasBytesAvailable: {printerSession.InputStream.HasBytesAvailable()}");
            if (printerSession.InputStream.Error != null)
            {
                Debug.WriteLine($"[LogSessionStatus] InputStream error: {printerSession.InputStream.Error.LocalizedDescription}");
            }
        }
        
        Debug.WriteLine($"[LogSessionStatus] printCommand is null: {printCommand == null}");
        if (printCommand != null)
        {
            Debug.WriteLine($"[LogSessionStatus] printCommand length: {printCommand.Length}");
        }
    }

    public void HandleEvent(NSStream theStream, NSStreamEvent streamEvent)
    {
        Debug.WriteLine($"[HandleEvent] ========== HandleEvent called ==========");
        Debug.WriteLine($"[HandleEvent] Stream event: {streamEvent}");
        Debug.WriteLine($"[HandleEvent] Stream type: {(theStream is NSOutputStream ? "NSOutputStream" : theStream is NSInputStream ? "NSInputStream" : "Unknown")}");
        Logger.CreateLog(streamEvent.ToString());

        try
        {
            if (printCommand == null)
            {
                Debug.WriteLine("[HandleEvent] WARNING: printCommand is null, cannot print");
                Logger.CreateLog("print command is null");
                return;
            }

            Debug.WriteLine($"[HandleEvent] printCommand length: {printCommand.Length}");
            Debug.WriteLine($"[HandleEvent] printCommand preview: {(printCommand.Length > 100 ? printCommand.Substring(0, 100) + "..." : printCommand)}");

            if (streamEvent == NSStreamEvent.HasSpaceAvailable)
            {
                Debug.WriteLine("[HandleEvent] HasSpaceAvailable event received");
                var stream = theStream as NSOutputStream;
                if (stream != null)
                {
                    Debug.WriteLine($"[HandleEvent] Stream status: {stream.Status}");
                    
                    // Check for stream errors first
                    if (stream.Error != null)
                    {
                        Debug.WriteLine($"[HandleEvent] ERROR: Stream has error: {stream.Error.LocalizedDescription}");
                        Debug.WriteLine($"[HandleEvent] Stream error domain: {stream.Error.Domain}, code: {stream.Error.Code}");
                        Logger.CreateLog($"Stream error: {stream.Error.LocalizedDescription}");
                        return;
                    }
                    
                    var hasSpace = stream.HasSpaceAvailable();
                    Debug.WriteLine($"[HandleEvent] Stream HasSpaceAvailable: {hasSpace}");
                    
                    // Check stream status - must be Open
                    if (stream.Status != NSStreamStatus.Open)
                    {
                        Debug.WriteLine($"[HandleEvent] ERROR: Stream status is not Open: {stream.Status}");
                        return;
                    }
                    
                    // Try to write even if HasSpaceAvailable returns false
                    // Sometimes the stream can accept data even when this returns false
                    if (!hasSpace)
                    {
                        Debug.WriteLine("[HandleEvent] HasSpaceAvailable is false, but attempting write anyway");
                    }
                    
                    byte[] mybytes = Encoding.ASCII.GetBytes(printCommand);
                    Debug.WriteLine($"[HandleEvent] Converted printCommand to {mybytes.Length} bytes using ASCII encoding");
                    
                    // Try writing in smaller chunks (2KB at a time) like the reference implementation
                    const int chunkSize = 2048;
                    int totalWritten = 0;
                    int offset = 0;
                    
                    while (offset < mybytes.Length)
                    {
                        int remaining = mybytes.Length - offset;
                        int currentChunkSize = Math.Min(chunkSize, remaining);
                        
                        Debug.WriteLine($"[HandleEvent] Writing chunk: offset={offset}, size={currentChunkSize}, remaining={remaining}");
                        
                        // Create a buffer for this chunk
                        byte[] chunk = new byte[currentChunkSize];
                        Array.Copy(mybytes, offset, chunk, 0, currentChunkSize);
                        
                        var bytesWritten = stream.Write(chunk, (nuint)currentChunkSize);
                        Debug.WriteLine($"[HandleEvent] Chunk write returned: {bytesWritten} bytes");
                        
                        if (bytesWritten == 0)
                        {
                            Debug.WriteLine("[HandleEvent] Chunk write returned 0 bytes. Waiting for next event.");
                            // Keep remaining data for next write
                            if (offset > 0)
                            {
                                // Some data was written, update printCommand with remaining
                                printCommand = Encoding.ASCII.GetString(mybytes, offset, remaining);
                                Debug.WriteLine($"[HandleEvent] Updated printCommand with remaining {remaining} bytes");
                            }
                            // Restart retry timer
                            StopRetryTimer();
                            writeRetryCount = 0;
                            writeRetryTimer = new System.Threading.Timer(CheckAndWriteData, null, 500, 500);
                            return;
                        }
                        
                        totalWritten += (int)bytesWritten;
                        offset += (int)bytesWritten;
                        
                        // If we didn't write the full chunk, we need to wait
                        if (bytesWritten < currentChunkSize)
                        {
                            Debug.WriteLine($"[HandleEvent] Partial chunk write. Wrote {bytesWritten} of {currentChunkSize}. Remaining: {remaining - bytesWritten}");
                            // Keep remaining data
                            printCommand = Encoding.ASCII.GetString(mybytes, offset, remaining - (int)bytesWritten);
                            Debug.WriteLine($"[HandleEvent] Updated printCommand with remaining {remaining - (int)bytesWritten} bytes");
                            // Restart retry timer
                            StopRetryTimer();
                            writeRetryCount = 0;
                            writeRetryTimer = new System.Threading.Timer(CheckAndWriteData, null, 500, 500);
                            return;
                        }
                        
                        // Small delay between chunks to avoid overwhelming the stream
                        if (offset < mybytes.Length)
                        {
                            Thread.Sleep(10);
                        }
                    }
                    
                    // All data written successfully
                    printCommand = null;
                    StopRetryTimer();
                    Debug.WriteLine($"[HandleEvent] All {totalWritten} bytes written successfully");
                    Debug.WriteLine("[HandleEvent] ========== Print data written successfully ==========");
                }
                else
                {
                    Debug.WriteLine("[HandleEvent] ERROR: Stream is not NSOutputStream");
                }
            }
            else
            {
                Debug.WriteLine($"[HandleEvent] Stream event is not HasSpaceAvailable: {streamEvent}");
                Logger.CreateLog("streamEvent is not HAsSpace available enent");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandleEvent] ========== ERROR in HandleEvent ==========");
            Debug.WriteLine($"[HandleEvent] Error: {ex.Message}");
            Debug.WriteLine($"[HandleEvent] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"[HandleEvent] InnerException: {ex.InnerException.Message}");
            }
            Logger.CreateLog(ex);
            throw new Exception("" + ex);
        }
    }

    public IList<PrinterDescription> AvailableDevices()
    {
        discoveredDevices.Clear();
        var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;

        foreach (var accessory in accessoryList)
            discoveredDevices.Add(new PrinterDescription() { Address = accessory.Name, Name = accessory.Name });

        return discoveredDevices;
    }

    public IList<PrinterDescription> AvailablePrinters()
    {
        Debug.WriteLine("[AvailablePrinters] ========== Getting available printers ==========");
        
        try
        {
            discoveredDevices.Clear();
            Debug.WriteLine("[AvailablePrinters] Cleared discoveredDevices list");
            
            var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;
            Debug.WriteLine($"[AvailablePrinters] EAAccessoryManager returned {accessoryList?.Length ?? 0} connected accessories");

            Logger.CreateLog($"AvailablePrinters: Found {accessoryList.Length} connected accessories");

            if (accessoryList != null && accessoryList.Length > 0)
            {
                foreach (var accessory in accessoryList)
                {
                    Debug.WriteLine($"[AvailablePrinters] Checking accessory: {accessory.Name ?? "null"}");
                    Debug.WriteLine($"[AvailablePrinters] ProtocolStrings: {(accessory.ProtocolStrings != null ? string.Join(", ", accessory.ProtocolStrings) : "null")}");
                    
                    // Filter for printer protocols (Zebra, Woosim, etc.)
                    if (accessory.ProtocolStrings != null && 
                        (accessory.ProtocolStrings.Contains("com.zebra.rawport") ||
                         accessory.ProtocolStrings.Contains("com.woosim.wspr240")))
                    {
                        var printerDesc = new PrinterDescription() 
                        { 
                            Address = accessory.Name, 
                            Name = accessory.Name 
                        };
                        discoveredDevices.Add(printerDesc);
                        Debug.WriteLine($"[AvailablePrinters] Added printer: {accessory.Name}");
                        Logger.CreateLog($"Found printer: {accessory.Name}");
                    }
                    else
                    {
                        Debug.WriteLine($"[AvailablePrinters] Accessory '{accessory.Name}' does not match printer protocols");
                    }
                }
            }
            else
            {
                Debug.WriteLine("[AvailablePrinters] WARNING: No accessories found or accessoryList is null");
            }

            Debug.WriteLine($"[AvailablePrinters] Returning {discoveredDevices.Count} printers");
            return discoveredDevices;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvailablePrinters] ERROR: {ex.Message}");
            Debug.WriteLine($"[AvailablePrinters] StackTrace: {ex.StackTrace}");
            return discoveredDevices;
        }
    }

    public string GetEmptyPdf()
    {
        var resourceName = "blank";
        var resourceExtension = "pdf";
        var path = NSBundle.MainBundle.PathForResource(resourceName, resourceExtension);
        return path;
    }
}
