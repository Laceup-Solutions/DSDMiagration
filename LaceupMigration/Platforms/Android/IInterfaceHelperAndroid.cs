
using Android.Net.Wifi;
using Android.Preferences;
using Android.Telephony;
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Print;
using AndroidX.Core.App;
using Java.IO;
using Java.Util;
using LaceupMigration;

[assembly: Dependency(typeof(InterfaceHelper))]

namespace LaceupMigration;

public class InterfaceHelper : IInterfaceHelper
{
    public string GetOsVersion()
    {
        return Build.VERSION.Release;
    }

    public string GetDeviceModel()
    {
        return Android.OS.Build.Device;
    }

    public string GetCarrier()
    {
        Context context = Android.App.Application.Context;
        TelephonyManager manager = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
        return manager.SimOperatorName;
    }

    public string GetBrand()
    {
        return Android.OS.Build.Brand;
    }

    public void HideKeyboard()
    {
        var context = Android.App.Application.Context;
        var inputMethodManager =
            context.GetSystemService(Android.Content.Context.InputMethodService) as
                Android.Views.InputMethods.InputMethodManager;

        if (inputMethodManager != null)
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

            if (activity != null)
            {
                var windowToken = activity.CurrentFocus?.WindowToken ?? activity.Window?.DecorView?.WindowToken;

                if (windowToken != null)
                {
                    inputMethodManager.HideSoftInputFromWindow(windowToken,
                        Android.Views.InputMethods.HideSoftInputFlags.None);
                }
                else
                {
                }
            }
        }
    }

    public string GetDeviceId()
    {
        string filename =
            System.IO.Path.Combine(Android.App.Application.Context.FilesDir.AbsolutePath, "instalation.id");
        string id;
        if (System.IO.File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                id = reader.ReadToEnd();
            }
        }
        else
        {
            id = Guid.NewGuid().ToString("N");
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.Write(id);
                }
            }
            finally
            {
            }
        }

        return "android" + id;
    }

    public string GetLocale()
    {
        Context context = Android.App.Application.Context;
        return context.Resources.Configuration.Locale.ToString();
    }

    public string GetSystemName()
    {
        string systemName = "";

        systemName = string.Format("Version Codename: {0}  Release: {1}  Incremental: {2}  Sdk: {3} SdkInt: {4} ",
            Android.OS.Build.VERSION.Codename, Android.OS.Build.VERSION.Release, Android.OS.Build.VERSION.Incremental,
            Android.OS.Build.VERSION.Sdk, Android.OS.Build.VERSION.SdkInt);

        return systemName;
    }

    public string GetIdiom()
    {
        return string.Empty;
    }

    public string GetSSID()
    {
        Context context = Android.App.Application.Context;

        if (context == null)
        {
            return string.Empty;
        }

        var manager = context.GetSystemService(Context.WifiService) as WifiManager;
        if (manager == null)
        {
            return string.Empty;
        }

        var info = manager.ConnectionInfo;
        if (info == null)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(info.SSID))
        {
            return string.Empty;
        }

        string ssid = info.SSID;
        return ssid;
    }

    public void SyncronizeDefaults()
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.Commit();
        }
    }

    public void StoreDefaults(string value, string key)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.PutString(key, value);

            prefEditor.Commit();
        }
    }

    public void StoreDefaults(float value, string key)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.PutFloat(key, value);

            prefEditor.Commit();
        }
    }

    public void StoreDefaults(bool value, string key)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.PutBoolean(key, value);

            prefEditor.Commit();
        }
    }

    public void StoreDefaults(int value, string key)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.PutInt(key, value);

            prefEditor.Commit();
        }
    }

    public void StoreDefaults(double value, string key)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            var prefEditor = preferences.Edit();

            prefEditor.PutFloat(key, (float)value);

            prefEditor.Commit();
        }
    }

    public string GetDefaultString(string key, string defaultValue)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            return preferences.GetString(key, defaultValue);
        }
    }

    public bool GetDefaultBool(string key, bool defaultValue)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            return preferences.GetBoolean(key, defaultValue);
        }
    }

    public float GetDefaultFloat(string key, float defaultValue)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            return preferences.GetFloat(key, defaultValue);
        }
    }

    public int GetDefaultInt(string key, int defaultValue)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            return preferences.GetInt(key, defaultValue);
        }
    }

    public double GetDefaultDouble(string key, double defaultValue)
    {
        using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
        {
            return (double)preferences.GetFloat(key, (float)defaultValue);
        }
    }

    public void ViewPdf(string filepath)
    {
        Java.IO.File file = new Java.IO.File(filepath);
        Context context = Android.App.Application.Context;

        try
        {
            var appPackageName = context.ApplicationInfo.PackageName + ".provider";
            var apkURI = FileProvider.GetUriForFile(context, appPackageName, file);

            Intent target = new Intent(Intent.ActionView);

            target.SetDataAndType(apkURI, "application/pdf");
            target.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.NoHistory);
            target.AddFlags(ActivityFlags.GrantReadUriPermission);

            Intent intent = Intent.CreateChooser(target, "Open File");
            intent.AddFlags(ActivityFlags.NewTask);

            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Logger.CreateLog("Error opening pdf file ==>" + ex.ToString());
        }
    }

    public void CheckForIcon(string value)
    {
        Context context = Android.App.Application.Context;

        var Icon_Default = new Android.Content.ComponentName(context, "crc64ec6d4b881a590ab7.Icon_Default");
        var Icon_DalCampo = new Android.Content.ComponentName(context, "crc64ec6d4b881a590ab7.Icon_DalCampo");

        List<ComponentName> AllIcons = new List<ComponentName>() { Icon_Default, Icon_DalCampo };
        ComponentName IconToUse = null;

        if (string.IsNullOrEmpty(value))
        {
            IconToUse = Icon_Default;
        }
        else
        {
            switch (value)
            {
                case "DalCampo":
                    IconToUse = Icon_DalCampo;
                    break;
                default:
                    IconToUse = Icon_Default;
                    break;
            }
        }

        var componentToEnable = AllIcons.Where(x => x == IconToUse).FirstOrDefault();

        try
        {
            if (context.PackageManager.GetComponentEnabledSetting(componentToEnable) == ComponentEnabledState.Enabled)
            {
                //
            }
            else
            {
                context.PackageManager.SetComponentEnabledSetting(componentToEnable, ComponentEnabledState.Enabled,
                    ComponentEnableOption.DontKillApp);

                foreach (var comp in AllIcons)
                {
                    if (comp != componentToEnable)
                        context.PackageManager.SetComponentEnabledSetting(comp, ComponentEnabledState.Disabled,
                            ComponentEnableOption.DontKillApp);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.CreateLog(ex.ToString());
        }
    }

    public void PrintPdf(string filepath)
    {
        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            var printManager =
                (Android.Print.PrintManager)activity.GetSystemService(Android.Content.Context.PrintService);

            var printAdapter = new PdfPrintDocumentAdapter(filepath);
            printManager.Print("Print Order", printAdapter, null);
        }
        catch (Exception ex)
        {
        }
    }

    public void SubscribeToTopic(string topic)
    {
        //try
        //{
        //    MainActivity.SubscribeToTopic(topic);
        //}
        //catch
        //{

        //}
    }

    public void UnsubscribeToTopic(string topic)
    {
    }

    public int PrintProcess1(List<Order> orders)
    {
        //var printers = PrinterProvider.AvailablePrinters();
        //switch (printers.Count)
        //{
        //    case 0:
        //        return 0;
        //    case 1:
        //        PrinterProvider.PrinterAddress = printers[0].Address;
        //        return 1;
        //    default:
        //        return 2;
        //}
        return 0;
    }

    public List<string> GetAllPrinters()
    {
        var string_printers = new List<string>();
        //var printers = PrinterProvider.AvailablePrinters();

        //foreach (var printer in printers)
        //{
        //    string_printers.Add(printer.Name);
        //}

        return string_printers;
    }

    public void PrintedSelected(string printer)
    {
        //var printers = PrinterProvider.AvailablePrinters().FirstOrDefault(x => x.Name == printer);
        //if (printers != null)
        //{
        //    PrinterProvider.PrinterAddress = printers.Address;
        //}
    }

    public bool Print(List<Order> orders)
    {
        //Laceup.IPrinter printer = PrinterProvider.CurrentPrinter();
        //return printer.PrintOrder(orders.FirstOrDefault(), false);
        return true;
    }

    public void ExitApplication()
    {
    }

    public void PrintIt(string printingString)
    {
        if (printingString.Contains((char)160))
        {
            printingString = printingString.Replace(((char)160).ToString(), string.Empty);
        }

        if (Config.PrintInvoiceAsReceipt)
        {
            printingString = printingString.Replace("Invoice", "Receipt");
            printingString = printingString.Replace("INVOICE", "RECEIPT");
        }

        if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
            throw new InvalidOperationException("No valid printer selected");

        using (BluetoothDevice hxm = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(PrinterProvider.PrinterAddress))
        {
            UUID applicationUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            using (BluetoothSocket socket = hxm.CreateRfcommSocketToServiceRecord(applicationUUID))
            {
                int factor = 1 + Config.OldPrinter;

                var timer = 0;
                bool connected = false;
                Exception e = null;

                while (timer < factor && !connected)
                {
                    try
                    {
                        Thread.Sleep(1000 * factor);
                        socket.Connect();
                        Thread.Sleep(1000 * factor);
                        connected = true;
                        break;
                    }
                    catch (Exception ee)
                    {
                        e = ee;
                    }

                    timer++;
                }

                if (!connected && e != null) throw e;

                using (var inReader = new BufferedReader(new InputStreamReader(socket.InputStream)))
                {
                    using (var outReader = new BufferedWriter(new OutputStreamWriter(socket.OutputStream), 60000))
                    {
                        DateTime st = DateTime.Now;
                        Logger.CreateLog("printingString.Length " + printingString.Length);
                        if (printingString.Length > 40000)
                        {
                            int i = 0;
                            while (true)
                            {
                                int start = 10000 * i;
                                int end = 10000;
                                if (start + end > printingString.Length) end = printingString.Length - start;
                                outReader.Write(printingString, start, end);
                                outReader.Flush();
                                i++;
                                if (end != 10000) break;
                            }
                        }
                        else
                        {
                            outReader.Write(printingString);
                            outReader.Flush();
                        }

                        //some waiting
                        int sec = 2;
                        int extra = 1;

                        if (printingString.Length > 10000) extra = 2;
                        if (printingString.Length > 15000) extra = 3;
                        if (printingString.Length > 20000) extra = 4;
                        if (printingString.Length > 25000) extra = 5;
                        if (printingString.Length > 30000) extra = 6;
                        if (printingString.Length > 35000) extra = 7;
                        if (printingString.Length > 40000) extra = 8;
                        if (printingString.Length > 45000) extra = 9;
                        if (printingString.Length > 50000) extra = 10;

                        Thread.Sleep(sec * (extra + factor) * 1000);

                        inReader.Close();
                        socket.Close();
                        outReader.Close();

                        //extra sleep for signature
                        if (!string.IsNullOrEmpty(Config.PrinterToUse) &&
                            Config.PrinterToUse.ToLowerInvariant().Contains("datamax"))
                            Thread.Sleep(3000);
                    }
                }

                Logger.CreateLog("finishing printing");
            }
        }
    }

    readonly string[] PERMISSIONS_STORAGE =
    {
        "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT",
        "android.permission.BLUETOOTH_PRIVILEGED"
    };

    public IList<PrinterDescription> AvailableDevices()
    {
        var context = Android.App.Application.Context;

        if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, Manifest.Permission.BluetoothConnect) !=
            (int)Permission.Granted)
        {
            App.Current.MainPage.DisplayAlert("Alert", "Bluetooth connect permission not granted", "OK");
            return null;
        }

        var retLits = new List<PrinterDescription>();
        if (BluetoothAdapter.DefaultAdapter != null)
        {
            var x = BluetoothAdapter.DefaultAdapter.BondedDevices;
            Logger.CreateLog("iterating in hte BondedDevices");
            foreach (var d in x)
            {
                retLits.Add(new PrinterDescription() { Name = d.Name, Address = d.Address });
                Logger.CreateLog(string.Format(
                    "Name: {0} , Address: {1} , BluetoothClass.MajorDeviceClass: {2}  BluetoothClass.DeviceClass: {3}",
                    d.Name, d.Address, d.BluetoothClass.MajorDeviceClass, d.BluetoothClass.DeviceClass));
            }
        }

        return retLits;
    }

    public IList<PrinterDescription> AvailablePrinters()
    {
        var context = Android.App.Application.Context;

        try
        {
            var permission2 =
                AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, "android.permission.BLUETOOTH_SCAN");

            if (permission2 != Permission.Granted)
                ActivityCompat.RequestPermissions(Platform.CurrentActivity, PERMISSIONS_STORAGE, 1);
        }
        catch (Exception ex)
        {
            App.Current.MainPage.DisplayAlert("Alert", "Bluetooth connect permission not granted", "OK");
            return new List<PrinterDescription>();
        }

        var retLits = new List<PrinterDescription>();
        /*retLits.Add (new PrinterDescription (){ Name = "Sdf", Address = "sdfsdfdf" });
        return retLits;*/
        try
        {
            if (BluetoothAdapter.DefaultAdapter != null)
            {
                var x = BluetoothAdapter.DefaultAdapter.BondedDevices;
                if (x != null)
                    foreach (var d in x)
                    {
                        if (!d.Name.StartsWith("Socket CHS"))
                            // if (d.BluetoothClass.MajorDeviceClass == MajorDeviceClass.Imaging && 1664 == (int)d.BluetoothClass.DeviceClass)
                            retLits.Add(new PrinterDescription() { Name = d.Name, Address = d.Address });
                    }
            }
        }
        catch (Exception e)
        {
            Logger.CreateLog(e);
            //Xamarin.Insights.Report(e);
        }

        return retLits;
    }

    public string GetEmptyPdf()
    {
        var applicationContext = Android.App.Application.Context;

        string filePath = System.IO.Path.Combine(applicationContext.FilesDir.AbsolutePath, "blank.pdf");

        try
        {
            using (var assetStream = applicationContext.Assets.Open("blank.pdf"))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    assetStream.CopyTo(fileStream);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.CreateLog(ex);
            throw new("Could not find 'blank.pdf' in the assets folder.", ex);
        }

        return filePath;
    }
}

public class PdfPrintDocumentAdapter : PrintDocumentAdapter
{
    private string filepath;

    public PdfPrintDocumentAdapter(string filepath)
    {
        this.filepath = filepath;
    }

    public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes,
        CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
    {
        if (cancellationSignal.IsCanceled)
        {
            callback.OnLayoutCancelled();
            return;
        }

        PrintDocumentInfo pdi = new PrintDocumentInfo.Builder(new Java.IO.File(filepath).Name)
            .SetContentType(PrintContentType.Document)
            .Build();

        callback.OnLayoutFinished(pdi, !newAttributes.Equals(oldAttributes));
    }

    public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination,
        CancellationSignal cancellationSignal, WriteResultCallback callback)
    {
        InputStream input = null;
        OutputStream output = null;

        try
        {
            input = new FileInputStream(filepath);
            output = new FileOutputStream(destination.FileDescriptor);

            byte[] buf = new byte[1024];
            int bytesRead;

            while ((bytesRead = input.Read(buf)) > 0)
            {
                output.Write(buf, 0, bytesRead);
            }

            callback.OnWriteFinished(new PageRange[] { PageRange.AllPages });
        }
        catch (Exception e)
        {
            // Handle exception
        }
        finally
        {
            try
            {
                if (input != null) input.Close();
                if (output != null) output.Close();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}