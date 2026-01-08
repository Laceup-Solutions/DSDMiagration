using System;

namespace LaceupMigration
{
    public class XXXPrefixedSequentialFormatedInvoiceProvider : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                if (order.AsPresale)
                {
                    Config.LastPresalePrintedId = Config.LastPresalePrintedId + 1;
                    Config.SavePresaleLastOrderId();

                    return Config.InvoicePresalePrefix + Config.LastPresalePrintedId.ToString("D" + Config.PrintedIdLength);
                }
                else
                {

                    Config.LastPrintedId = Config.LastPrintedId + 1;
                    Config.SaveLastOrderId();

                    return Config.InvoicePrefix + Config.LastPrintedId.ToString("D" + Config.PrintedIdLength);
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report(ee);
            }
            return new DefaultInvoiceProvider().GetId(order);
        }

        public string GetId(Batch batch)
        {
            try
            {
                int id = Convert.ToInt32(Config.VendorName);
                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();

                return Config.InvoicePrefix + Config.LastPrintedId.ToString("D" + Config.PrintedIdLength);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report(ee);
            }
            return new DefaultInvoiceProvider().GetId(batch);
        }
    }
}