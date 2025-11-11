using System;
using System.Collections.Generic;
using System.Drawing;
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
        return UIDevice.CurrentDevice.UserInterfaceIdiom.ToString() +
               UIDevice.CurrentDevice.IdentifierForVendor.AsString();
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

    public void SubscribeToTopic(string topic)
    {
    }

    public void UnsubscribeToTopic(string topic)
    {
    }

    public int PrintProcess1(List<Order> orders)
    {
        return 0;
    }

    public void PrintedSelected(string printer)
    {
    }

    public bool Print(List<Order> orders)
    {
        return false;
    }

    public List<string> GetAllPrinters()
    {
        return new List<string>();
    }

    public void ExitApplication()
    {
        Thread.CurrentThread.Abort();
    }

    private void DiscoverPrinters()
    {

    }

    private void OnDiscoveryFinished(object sender, EventArgs e)
    {
        // Handle discovery finished
    }

    public void PrintIt(string printingString)
    {
        printCommand = printingString + Environment.NewLine + Environment.NewLine;

        if (printingString.Contains((char)160))
        {
            printingString = printingString.Replace(((char)160).ToString(), string.Empty);
        }

        if (Config.PrintInvoiceAsReceipt)
        {
            printingString = printingString.Replace("Invoice", "Receipt");
            printingString = printingString.Replace("INVOICE", "RECEIPT");
        }

        DiscoverPrinters();

        //if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
        //    throw new InvalidOperationException("No valid printer selected");

        //if (printerSession == null)
        //{
        //    CreatePrinterSession();
        //}
        //else
        //{
        //    //Logger.CreateLog ("session exist already");
        //    LogSessionStatus();
        //    //Logger.CreateLog ("manually calling handler");
        //    HandleEvent(printerSession.OutputStream, NSStreamEvent.HasSpaceAvailable);
        //}
    }

    void CreatePrinterSession()
    {
        Logger.CreateLog("Creating session");
        var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;

        EAAccessory accessory = accessoryList.FirstOrDefault(a => a.ProtocolStrings.Contains("com.zebra.rawport"));

        if (!string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
            accessory = accessoryList.FirstOrDefault(x => x.Name == PrinterProvider.PrinterAddress);

        if (accessory == null)
        {
            Logger.CreateLog("Printer not found");
            throw new Exception("no printer found");
        }

        accessory.Delegate = new EAAccessoryDelegateHandler();
        printerSession = new EASession(accessory, "com.zebra.rawport");
        var mysession = printerSession;
        if (mysession.OutputStream == null)
            Logger.CreateLog("output is null");
        else
            Logger.CreateLog("output not null");
        mysession.OutputStream.Delegate = null;
        mysession.OutputStream.Schedule(NSRunLoop.Current, NSRunLoopMode.Default);
        mysession.OutputStream.Open();
        if (mysession.InputStream == null)
            Logger.CreateLog("InputStream is null");
        else
        {
            Logger.CreateLog("InputStream not null");
            mysession.InputStream.Open();
        }

        HandleEvent(printerSession.OutputStream, NSStreamEvent.HasSpaceAvailable);
    }

    class EAAccessoryDelegateHandler : EAAccessoryDelegate
    {
        public void DisposeSession()
        {
            //Logger.CreateLog ("Disposing of the session");
            if (printerSession == null)
            {
                //Logger.CreateLog ("null session");
                return;
            }

            // LogSessionStatus ();
            try
            {
                if (printerSession.OutputStream != null)
                {
                    printerSession.OutputStream.Close();
                    printerSession.OutputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                    printerSession.OutputStream.Delegate = null;
                }

                printerSession.Dispose();

                printerSession = null;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public override void Disconnected(EAAccessory accessory)
        {
            Logger.CreateLog("device disconnected");
            DisposeSession();
            accessory.Delegate = null;
        }
    }

    void LogSessionStatus()
    {
        if (printerSession.OutputStream == null)
            Logger.CreateLog("output is null");
        else
        {
            //Logger.CreateLog ("output not null");
            //Logger.CreateLog ("mysession.OutputStream.HasSpaceAvailable() :" + session.OutputStream.HasSpaceAvailable ().ToString ());
        }
    }

    public void HandleEvent(NSStream theStream, NSStreamEvent streamEvent)
    {
        //Logger.CreateLog ("entered the handler");
        Logger.CreateLog(streamEvent.ToString());

        try
        {
            if (printCommand == null)
            {
                Logger.CreateLog("print command is null");
                return;
            }

            if (streamEvent == NSStreamEvent.HasSpaceAvailable)
            {
                var stream = theStream as NSOutputStream;
                if (stream != null)
                {
                    byte[] mybytes = Encoding.Default.GetBytes(printCommand);
                    stream.Write(mybytes, (uint)mybytes.Length);
                    printCommand = null;
                }
            }
            else
                Logger.CreateLog("streamEvent is not HAsSpace available enent");
        }
        catch (Exception ex)
        {
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
        discoveredDevices.Clear();
        var accessoryList = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;

        foreach (var accessory in accessoryList)
            discoveredDevices.Add(new PrinterDescription() { Address = accessory.Name, Name = accessory.Name });

        return discoveredDevices;
    }

    public string GetEmptyPdf()
    {
        var resourceName = "blank";
        var resourceExtension = "pdf";
        var path = NSBundle.MainBundle.PathForResource(resourceName, resourceExtension);
        return path;
    }
}
