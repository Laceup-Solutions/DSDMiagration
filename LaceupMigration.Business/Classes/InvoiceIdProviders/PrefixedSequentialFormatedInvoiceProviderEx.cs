using System;
using System.Linq;

namespace LaceupMigration
{

    public class PrefixedSequentialFormatedInvoiceProviderEx : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                long id;
                string prefix;
                if (order.AsPresale)
                {
                    Config.LastPresalePrintedId = Config.LastPresalePrintedId + 1;
                    Config.SavePresaleLastOrderId();
                    id = Config.LastPresalePrintedId;
                    prefix = Config.InvoicePresalePrefix ?? string.Empty;
                }
                else
                {
                    Config.LastPrintedId = Config.LastPrintedId + 1;
                    Config.SaveLastOrderId();
                    id = Config.LastPrintedId;
                    prefix = Config.InvoicePrefix ?? string.Empty;
                }

                string extrafield = "PresaleFormatQtyDigits";
                if (!order.AsPresale)
                    extrafield = "DSDFormatQtyDigits";

                int qtyDigits = -1;
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (salesman != null && !string.IsNullOrEmpty(salesman.ExtraProperties))
                {
                    var pre = DataAccess.GetSingleUDF(extrafield, salesman.ExtraProperties);

                    if (!string.IsNullOrEmpty(pre))
                        qtyDigits = Convert.ToInt32(pre);
                }

                if (qtyDigits > 0)
                    return prefix + id.ToString("D" + qtyDigits.ToString());
                else
                    return prefix + id.ToString();
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