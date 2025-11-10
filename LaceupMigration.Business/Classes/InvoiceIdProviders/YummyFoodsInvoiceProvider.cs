





using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;


namespace LaceupMigration
{
    class YummyFoodsInvoiceProvider : IInvoiceIdProvider
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

            return DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.SalesmanId.ToString("D2") + Config.CurrentOrderId.ToString("D2");
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

            return DateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture) + Config.SalesmanId.ToString("D2") + Config.CurrentOrderId.ToString("D2");
        }
    }
}