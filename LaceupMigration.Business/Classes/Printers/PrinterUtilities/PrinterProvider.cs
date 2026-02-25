

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                    if (Config.CheckCommunicatorVersion("28.2.0"))
                        printerToUse = "LaceUPMobileClassesIOS.ZebraFourInchesPrinter1";
                    else
                        printerToUse = "LaceUPMobileClassesIOS.ZebraFourInchesPrinter";
                }

                if (printerToUse.Contains("LaceUPMobileClassesIOS"))
                    printerToUse = printerToUse.Replace("LaceUPMobileClassesIOS", "LaceupMigration");
                
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

        public static async void PrintDocument(Func<int, string> printIt, int copies = 1, Func<Task> onSuccessAfterPrint = null)
        {
            var response = await DialogHelper._dialogService.ShowPromptAsync("Alert", "Enter Copies", "Print", "Cancel", "1", 1, copies.ToString());

            // If user cancelled, return early (don't show printer selection)
            if (string.IsNullOrEmpty(response))
                return;

            int qty = 1;
            Int32.TryParse(response, out qty);
            
            var printers = PrinterProvider.AvailablePrinters();

            if (printers == null)
                return;
            
            switch (printers.Count)
            {
                case 0:
                    await DialogHelper._dialogService.ShowAlertAsync("No printers available");
                    break;
                case 1:
                    PrinterProvider.PrinterAddress = printers[0].Address;
                    PrintIt(qty, printIt, onSuccessAfterPrint);
                    break;
                default:
                    SelectPrinter(printers, qty, printIt, onSuccessAfterPrint);
                    break;
            }
        }

        public static async void SelectPrinter(IList<PrinterDescription> printers, int number, Func<int, string> printIt, Func<Task> onSuccessAfterPrint = null)
        {
            var options = printers.Select(x => x.Name).ToList();
            var actions = printers.Select(printer => new Action(() =>
            {
                PrinterProvider.PrinterAddress = printer.Address;
                PrintIt(number, printIt, onSuccessAfterPrint);
            })).ToList();

            var response = await DialogHelper._dialogService.ShowActionSheetAsync("Select Printer", "", "Cancel", options.ToArray());

            var index = options.IndexOf(response);

            if (index != -1)
                actions[index].Invoke();
        }

        public static void PrintIt(int number, Func<int, string> printIt, Func<Task> onSuccessAfterPrint = null)
        {
            DialogHelper._dialogService.ShowLoadingAsync("Printing");

            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                var result = printIt(number);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DialogHelper._dialogService.HideLoadingAsync();

                    if (!string.IsNullOrEmpty(result))
                    {
                        await DialogHelper._dialogService.ShowAlertAsync(result);
                    }
                    else
                    {
                        await DialogHelper._dialogService.ShowAlertAsync("Printed Successfully!");
                        if (onSuccessAfterPrint != null)
                            await onSuccessAfterPrint();
                    }
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