using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public class PaymentSplit
    {
        public string UniqueId { get; set; }
        public double Amount { get; set; }
        public InvoicePaymentMethod PaymentMethod { get; set; }
        public string Ref { get; set; }
        public string Comments { get; set; }
        public string ExtraFields { get; set; }

        static public List<PaymentSplit> SplitPayment(InvoicePayment payment_, Dictionary<string, double> ordersTotals = null)
        {
            var retList = new List<PaymentSplit>();
            if (payment_ == null)
                return retList;
            if (ordersTotals == null)
            {
                ordersTotals = new Dictionary<string, double>();
                foreach (var orderUniqueId in payment_.OrderId.Split(','))
                {
                    var order = Order.Orders.FirstOrDefault(x => x.UniqueId == orderUniqueId);
                    if (order != null)
                        ordersTotals.Add(order.UniqueId, order.OrderTotalCost());
                }
            }
            var components = new List<PaymentComponent>();
            foreach (var component in payment_.Components)
                components.Add(new PaymentComponent() { Amount = component.Amount, Comments = component.Comments, PaymentMethod = component.PaymentMethod, Ref = component.Ref, ExtraFields = component.ExtraFields });
            foreach (var component in components)
            {
                double oldAmount = component.Amount;
                while (component.Amount > 0)
                {
                    foreach (var orderUniqueId in payment_.OrderId.Split(','))
                    {
                        if (!ordersTotals.ContainsKey(orderUniqueId))
                        {
                            continue;
                        }
                        if (ordersTotals[orderUniqueId] <= 0)
                            continue;
                        double usedInThisInvoice = component.Amount;
                        if (component.Amount > ordersTotals[orderUniqueId])
                        {
                            //if (Config.CanPayMoreThanOwned)
                            //{
                            //    usedInThisInvoice = component.Amount;
                            //    component.Amount = 0;
                            //}
                            //else
                            //{
                            usedInThisInvoice = ordersTotals[orderUniqueId];
                            component.Amount = component.Amount - usedInThisInvoice;
                            //}
                        }
                        else
                            component.Amount = 0;


                        ordersTotals[orderUniqueId] = ordersTotals[orderUniqueId] - usedInThisInvoice;

                        PaymentSplit ps = new PaymentSplit();
                        ps.UniqueId = orderUniqueId;
                        ps.Amount = usedInThisInvoice;
                        ps.PaymentMethod = component.PaymentMethod;
                        ps.Ref = component.Ref;
                        ps.Comments = component.Comments;
                        ps.ExtraFields = component.ExtraFields;

                        retList.Add(ps);
                        if (component.Amount == 0)
                            break;
                    }

                    foreach (var orderUniqueId in payment_.InvoicesId.Split(','))
                    {
                        double usedInThisInvoice = 0;
                        if (ordersTotals.Count > 0)
                        {
                            if (!ordersTotals.ContainsKey(orderUniqueId))
                                continue;
                            if (ordersTotals[orderUniqueId] <= 0)
                                continue;
                            usedInThisInvoice = component.Amount;
                            if (component.Amount > ordersTotals[orderUniqueId])
                            {

                                usedInThisInvoice = ordersTotals[orderUniqueId];
                                component.Amount = component.Amount - usedInThisInvoice;
                            }
                            else
                                component.Amount = 0;

                        }



                        PaymentSplit ps = new PaymentSplit();
                        ps.UniqueId = orderUniqueId;
                        ps.Amount = usedInThisInvoice > 0 ? usedInThisInvoice : component.Amount;
                        ps.PaymentMethod = component.PaymentMethod;
                        ps.Ref = component.Ref;
                        ps.Comments = component.Comments;
                        ps.ExtraFields = component.ExtraFields;

                        retList.Add(ps);
                        if (component.Amount == 0)
                            break;
                    }

                    if (oldAmount == component.Amount)
                    {
                        Logger.CreateLog("component.Amout did nt change, what happenned?");
                        break;
                    }
                    oldAmount = component.Amount;
                }
            }

            return retList;
        }
    }
}
