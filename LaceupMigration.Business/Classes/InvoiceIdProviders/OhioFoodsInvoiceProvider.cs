using System;
using System.Globalization;

namespace LaceupMigration
{
    public class OhioFoodsInvoiceProvider : IInvoiceIdProvider
    {
        public string GetId(Batch batch)
        {
            if (Config.CurrentOrderDate != DateTime.Today)
            {
                Config.CurrentOrderDate = DateTime.Today;
                Config.CurrentOrderId = 1;
            }
            else
                Config.CurrentOrderId = Config.CurrentOrderId + 1;

            Config.SaveCurrentOrderId();

            return Config.InvoicePrefix + DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }

        public string GetId(Order order)
        {
            if (Config.CurrentOrderDate != DateTime.Today)
            {
                Config.CurrentOrderDate = DateTime.Today;
                Config.CurrentOrderId = 1;
            }
            else
                Config.CurrentOrderId = Config.CurrentOrderId + 1;

            Config.SaveCurrentOrderId();

            return Config.InvoicePrefix + DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }
    }
}