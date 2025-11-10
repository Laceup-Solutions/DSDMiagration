

using System;
using System.Collections.Generic;
using System.Linq;




using System.Threading;
using Microsoft.Maui.ApplicationModel;


namespace LaceupMigration
{

   public static class PrinterProvider
    {

        static IPrinter printer;

        static public string PrinterAddress { get; set; }

        static public IPrinter CurrentPrinter()
        {
            try
            {
                // instantiate selected printer

                string printerToUse = Config.PrinterToUse;


                if (string.IsNullOrEmpty(printerToUse))
                {
                    if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "28.2.0"))
                        printerToUse = "LaceUPMobileClassesIOS.ZebraFourInchesPrinter1";
                    else
                        printerToUse = "LaceUPMobileClassesIOS.ZebraFourInchesPrinter";
                }

                Logger.CreateLog("Trying to use printer " + printerToUse);
                Type t = Type.GetType(printerToUse);
                if (t == null)
                {
                    Logger.CreateLog("could not instantiate default printer" + printerToUse + " using ZebraThreeInchesPrinter instead");
                    printer = new ZebraFourInchesPrinter1();
                }
                printer = Activator.CreateInstance(t) as IPrinter;
                if (printer == null)
                    printer = new ZebraFourInchesPrinter1();
                return printer;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                return new ZebraFourInchesPrinter1();
            }
        }

        public static IList<PrinterDescription> AvailablePrinters()
        {
            return Config.helper.AvailablePrinters();

        }

        static public string ScannerAddress { get; set; }

        public static IList<PrinterDescription> AvailableDevices()
        {
            return Config.helper.AvailableDevices();
        }

        public static async void PrintDocument(Func<int, string> printIt, int copies = 1)
        {
            var response = await DialogService._dialogService.ShowPromptAsync("Alert", "Enter Copies", "Print", "Cancel", "1", 1, "1");

            int qty = 1;
            Int32.TryParse(response, out qty);
            
            var printers = PrinterProvider.AvailablePrinters();

            if (printers == null)
                return;
            
            switch (printers.Count)
            {
                case 0:
                    await DialogService._dialogService.ShowAlertAsync("No printers available");
                    break;
                case 1:
                    PrinterProvider.PrinterAddress = printers[0].Address;
                    PrintIt(qty, printIt);
                    break;
                default:
                    SelectPrinter(printers, qty, printIt);
                    break;
            }
        }

        public static async void SelectPrinter(IList<PrinterDescription> printers, int number, Func<int, string> printIt)
        {

            var options = printers.Select(x => x.Name).ToList();
            var actions = printers.Select(printers => new Action(() =>
            {
                //var printer = printers.FirstOrDefault(x => x.Name == result);
                PrinterProvider.PrinterAddress = printers.Address;
                PrintIt(number, printIt);

            })).ToList();

            var response = await DialogService._dialogService.ShowActionSheetAsync("Select Printer", "", "Cancel", options.ToArray());

            var index = options.IndexOf(response);

            if (index != -1)
                actions[index].Invoke();
        }

        public static void PrintIt(int number, Func<int, string> printIt)
        {
            DialogService._dialogService.ShowLoadingAsync("Printing");

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                var result = printIt(number);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DialogService._dialogService.HideLoadingAsync();

                    if (!string.IsNullOrEmpty(result))
                        DialogService._dialogService.ShowAlertAsync(result);
                    else
                        DialogService._dialogService.ShowAlertAsync("Printed Successfully!");

                });
            });
        }
    }


    public class PrinterDescription
    {
        public string Address { get; set; }

        public string Name { get; set; }
    }
}