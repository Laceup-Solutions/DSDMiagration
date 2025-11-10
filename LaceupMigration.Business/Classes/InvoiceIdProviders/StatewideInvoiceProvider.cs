using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;









namespace LaceupMigration
{
    public class StatewideInvoiceProvider : IInvoiceIdProvider
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

            return Config.RouteName + DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
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

            return Config.RouteName + DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }
    }
}