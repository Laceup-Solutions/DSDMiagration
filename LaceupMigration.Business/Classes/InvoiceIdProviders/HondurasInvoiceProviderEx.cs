using System;
using System.Linq;

namespace LaceupMigration
{

    public class HondurasInvoiceProviderEx : IInvoiceIdProvider
    {
        public string GetId(Order order)
        {
            try
            {
                long id;
                string prefix;
                int qtyDigits = -1;

                if (Config.PrintedIdLength > 0)
                    qtyDigits = Config.PrintedIdLength;

                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (order.AsPresale)
                {
                    Config.LastPresalePrintedId = Config.LastPresalePrintedId + 1;
                    Config.SavePresaleLastOrderId();
                    id = Config.LastPresalePrintedId;
                    prefix = Config.InvoicePresalePrefix ?? string.Empty;

                    if (salesman != null && !string.IsNullOrEmpty(salesman.ExtraProperties))
                    {
                        var pre = UDFHelper.ExplodeExtraProperties(salesman.ExtraProperties).FirstOrDefault(x => x.Key == "PresaleFormatQtyDigits");
                        if (pre != null)
                            qtyDigits = Convert.ToInt32(pre.Value);
                    }
                }
                else
                {
                    Config.LastPrintedId = Config.LastPrintedId + 1;
                    Config.SaveLastOrderId();
                    id = Config.LastPrintedId;
                    prefix = salesman.SequencePrefix ?? string.Empty;

                    if (salesman != null && !string.IsNullOrEmpty(salesman.ExtraProperties))
                    {
                        var pre = UDFHelper.ExplodeExtraProperties(salesman.ExtraProperties).FirstOrDefault(x => x.Key == "DSDFormatQtyDigits");
                        if (pre != null)
                            qtyDigits = Convert.ToInt32(pre.Value);
                    }
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