using System;
using System.Globalization;

namespace LaceupMigration
{
    public class AlwaysFreshInvoiceProvider : IInvoiceIdProvider
    {
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

            return Config.SalesmanId.ToString("D3") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }

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

            return Config.SalesmanId.ToString("D3") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }
    }
}