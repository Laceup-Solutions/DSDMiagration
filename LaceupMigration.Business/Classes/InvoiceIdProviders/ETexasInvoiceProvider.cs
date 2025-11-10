using System;
using System.Linq;

namespace LaceupMigration
{
    public class ETexasInvoiceProvider : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                string id = Config.InvoicePrefix;
                string store = "0000";

                var storeField = order.Client.NonVisibleExtraProperties != null ? order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "store") : null;
                if (storeField != null)
                    store = storeField.Item2;
                else
                {
                    storeField = order.Client.ExtraProperties != null ? order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "store") : null;
                    if (storeField != null)
                        store = storeField.Item2;
                }

                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();

                if (store.Length < 4)
                    store = new string('0', 4 - store.Length) + store;

                return id + store + Config.LastPrintedId.ToString();
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            return new DefaultInvoiceProvider().GetId(order);
        }

        public string GetId(Batch batch)
        {
            try
            {
                string id = Config.InvoicePrefix;
                string store = "0000";

                var storeField = batch.Client.NonVisibleExtraProperties != null ? batch.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "store") : null;
                if (storeField != null)
                    store = storeField.Item2;
                else
                {
                    storeField = batch.Client.ExtraProperties != null ? batch.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "store") : null;
                    if (storeField != null)
                        store = storeField.Item2;
                }

                Config.LastPrintedId = Config.LastPrintedId + 1;
                Config.SaveLastOrderId();

                if (store.Length < 4)
                    store = new string('0', 4 - store.Length) + store;

                return id + store + Config.LastPrintedId.ToString();
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
            return new DefaultInvoiceProvider().GetId(batch);
        }
    }
}