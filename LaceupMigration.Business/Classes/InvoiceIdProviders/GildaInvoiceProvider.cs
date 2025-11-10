using System;

namespace LaceupMigration
{
    public class GildaInvoiceProvider : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                int id = Convert.ToInt32(Config.VendorName);
                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();
                if (id < 10)
                    return "I" + id.ToString() + Config.LastPrintedId.ToString("D6");
                else
                    return Config.VendorName + Config.LastPrintedId.ToString("D6");
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report (ee);
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
                if (id < 10)
                    return "I" + id.ToString() + Config.LastPrintedId.ToString("D6");
                else
                    return Config.VendorName + Config.LastPrintedId.ToString("D6");
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report (ee);
            }
            return new DefaultInvoiceProvider().GetId(batch);
        }
    }
}