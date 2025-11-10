using System;

namespace LaceupMigration
{

    public class PrefixedSequentialInvoiceProvider : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();

                return Config.InvoicePrefix + Config.LastPrintedId.ToString();
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

                return Config.InvoicePrefix + Config.LastPrintedId.ToString();
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