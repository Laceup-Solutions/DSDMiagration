using System;
using System.Globalization;

namespace LaceupMigration
{

    public class DefaultInvoiceProvider : IInvoiceIdProvider
    {

        public string GetId(Order order)
        {
            if (Config.UseOrderId)
            {

                int id = 0;

                Int32.TryParse(Config.VendorName, out id);
                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();
                if (id < 10)
                    return "I" + id.ToString() + Config.LastPrintedId.ToString("D6");
                else
                    return Config.VendorName + Config.LastPrintedId.ToString("D6");
                try
                {
                    return new PrefixedSequentialInvoiceProvider().GetId(order);
                }
                catch
                {
                }
            }
            if (Config.CurrentOrderDate != DateTime.Today)
            {
                Config.CurrentOrderDate = DateTime.Today;
                Config.CurrentOrderId = 1;
            }
            else
                Config.CurrentOrderId = Config.CurrentOrderId + 1;

            Config.SaveCurrentOrderId();

            return Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }

        public string GetId(Batch batch)
        {
            if (Config.UseOrderId)
            {
                int id = Convert.ToInt32(Config.VendorName);
                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();
                if (id < 10)
                    return "I" + id.ToString() + Config.LastPrintedId.ToString("D6");
                else
                    return Config.VendorName + Config.LastPrintedId.ToString("D6");
            }
            else
            {
                if (Config.CurrentOrderDate != DateTime.Today)
                {
                    Config.CurrentOrderDate = DateTime.Today;
                    Config.CurrentOrderId = 1;
                }
                else
                    Config.CurrentOrderId = Config.CurrentOrderId + 1;

                Config.SaveCurrentOrderId();

                return Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
            }
        }
    }
}