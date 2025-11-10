using System;
using System.Globalization;

namespace LaceupMigration
{
    public class DNWInvoiceProvider : IInvoiceIdProvider
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


            var storeNumer = order.Client.LicenceNumber;
            if (string.IsNullOrEmpty(storeNumer))
                storeNumer = order.Client.ClientId.ToString();

            Config.SaveCurrentOrderId();

            return storeNumer + "-" + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + "-" + Config.CurrentOrderId.ToString("D2");
            
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


            
            var storeNumer = batch.Client.LicenceNumber;
            if (string.IsNullOrEmpty(storeNumer))
                storeNumer = batch.Client.ClientId.ToString();

            Config.SaveCurrentOrderId();

            return storeNumer + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentOrderId.ToString("D2");
        }
    }
}