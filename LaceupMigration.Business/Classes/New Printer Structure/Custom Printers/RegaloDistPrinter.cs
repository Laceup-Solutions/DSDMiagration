using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class RegaloDistPrinter : TextFourInchesPrinter
    {
        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            List<string> lines = new List<string>();

            var printedDate = DateTime.Now;

            lines.Add(string.Format("Prt Date:{0}  Time:{1}", printedDate.ToShortDateString(), printedDate.ToShortTimeString()));

            CompanyInfo company = null;

            if (CompanyInfo.Companies.Count > 0)
            {
                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                lines.Add(company.CompanyName);
                lines.Add(company.CompanyAddress1);
                lines.Add(company.CompanyAddress2);
                lines.Add("Phone: " + company.CompanyPhone);
                lines.Add("Email: " + company.CompanyEmail);
            }

            lines.Add("");

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            lines.Add(string.Format("Dist. Sales Rep: {0}", salesman != null ? salesman.Name : ""));

            //lines.Add("");

            //lines.Add("DELIVERY TICKET / INVOICE");

            lines.Add("");

            lines.Add(string.Format("Cust: {0}", order.Client.ClientName));

            foreach (string s in ClientAddress(order.Client))
                lines.Add(s);

            lines.Add("");

            lines.Add(string.Format("Invoice #: {0}   Inv. Date: {1}", order.PrintedOrderId ?? "", order.Date.ToShortDateString()));

            lines.Add(string.Format("PO #: {0}", order.PONumber ?? ""));

            lines.Add(string.Format("A/R Type: {0}   Vendor No: {1}", order.Term ?? "", order.Client.VendorNumber ?? ""));

            string tableHeader = "UPC          Description                            Units Price   Ext Amount";

            string dotsLine =    "-------------------------------------------------------------------------------";

            lines.Add("");

            double totalSales = 0;
            double totalCredits = 0;
            double totalReturs = 0;

            if (salesLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> sslines;

                if (Config.UseDraggableTemplate)
                    sslines = SortDetails.SortedDetails(order.Client.ClientId, salesLines.Values.ToList());
                else
                    sslines = SortDetails.SortedDetails(salesLines.Values.ToList());

                var listXX = sslines.ToList();
                var relatedDetailIds = listXX.Where(x => x.OrderDetail.RelatedOrderDetail > 0).Select(x => x.OrderDetail.RelatedOrderDetail).ToList();
                var removedList = listXX.Where(x => relatedDetailIds.Contains(x.OrderDetail.OrderDetailId)).ToList();
                foreach (var r in removedList)
                    listXX.Remove(r);
                // reinsert
                // If grouping, add at the end
                if (Config.GroupRelatedWhenPrinting)
                {
                    foreach (var r in removedList)
                        listXX.Add(r);
                }
                else
                    foreach (var r in removedList)
                    {
                        for (int index = 0; index < listXX.Count; index++)
                            if (listXX[index].OrderDetail.RelatedOrderDetail == r.OrderDetail.OrderDetailId)
                            {
                                listXX.Insert(index + 1, r);
                                break;
                            }
                    }

                lines.Add(dotsLine);

                lines.Add(tableHeader);

                lines.Add(dotsLine);

                lines.Add("                     SALES");

                totalSales = AddDetailsLines(lines, listXX, 1, dotsLine);

                lines.Add("");
            }

            if (creditLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> crlines;

                if (Config.UseDraggableTemplate)
                    crlines = SortDetails.SortedDetails(order.Client.ClientId, creditLines.Values.ToList());
                else
                    crlines = SortDetails.SortedDetails(creditLines.Values.ToList());

                lines.Add(dotsLine);

                lines.Add(tableHeader);

                lines.Add(dotsLine);

                lines.Add("                     CREDITS");

                totalCredits = AddDetailsLines(lines, crlines.ToList(), -1, dotsLine);

                lines.Add("");
            }

            if (returnsLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> rtlines;

                if (Config.UseDraggableTemplate)
                    rtlines = SortDetails.SortedDetails(order.Client.ClientId, returnsLines.Values.ToList());
                else
                    rtlines = SortDetails.SortedDetails(returnsLines.Values.ToList());

                lines.Add(dotsLine);

                lines.Add(tableHeader);

                lines.Add(dotsLine);

                lines.Add("                     RETURNS");

                totalReturs = AddDetailsLines(lines, rtlines.ToList(), -1, dotsLine);

                lines.Add("");
            }

            double paid = 0;
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            var discount = order.CalculateDiscount();
            var tax = order.CalculateTax();

            var s4 = totalSales + totalReturs + totalCredits - discount + tax;

            string totalSalesString = totalSales.ToCustomString();
            if (totalSalesString.Length < 10)
                totalSalesString += new string(' ', 10 - totalSalesString.Length);

            string totalRetursString = totalReturs.ToCustomString();
            if (totalRetursString.Length < 10)
                totalRetursString += new string(' ', 10 - totalRetursString.Length);

            string netAmountString = (totalSales + totalReturs + totalCredits).ToCustomString();
            if (netAmountString.Length < 10)
                netAmountString += new string(' ', 10 - netAmountString.Length);

            lines.Add(string.Format("SALES:      {0}   PROMO:         {1}", totalSalesString, discount.ToCustomString()));
            lines.Add(string.Format("RETURNS:    {0}   CREDITS:       {1}", totalRetursString, totalCredits.ToCustomString()));
            lines.Add(string.Format("NET AMOUNT: {0}   TOTAL DUE:     {1}", netAmountString, order.OrderTotalCost().ToCustomString()));
            //lines.Add(string.Format("            {0}   TOTAL PAYMENT: {0}", new string(' ', 10), paid.ToCustomString()));
            //lines.Add(string.Format("            {0}   TOTAL DUE:     {0}", new string(' ', 10), (s4 - paid).ToCustomString()));

            lines.Add("");
            lines.Add("");
            lines.Add("");
            lines.Add("");

            lines.Add("Received By: _________________________");

            lines.Add("");
            lines.Add("");

            return PrintLines(lines);
        }

        public double AddDetailsLines(List<string> lines, List<OrderLine> listXX, int factor, string dotsLine)
        {
            float totalUnit = 0;
            double totalAmount = 0;

            foreach (var item in listXX)
            {
                string upc = item.Product.Upc;
                var pParts = SplitProductName(item.Product.Name, 38, 38);

                for (int i = 0; i < pParts.Count; i++)
                {
                    var description = pParts[i];
                    if (i == 0)
                    {
                        var qty = item.Qty;
                        if (item.UoM != null)
                            qty *= item.UoM.Conversion;
                        var price = item.Price;
                        var amount = item.Qty * item.Price * factor;

                        string qtyString = qty.ToString();
                        string priceString = price.ToCustomString();
                        string amountString = amount.ToCustomString();

                        if (upc.Length < 12)
                            upc += new string(' ', 12 - upc.Length);

                        if (description.Length < 38)
                            description += new string(' ', 38 - description.Length);

                        if (qtyString.Length < 5)
                            qtyString += new string(' ', 5 - qtyString.Length);

                        if (priceString.Length < 7)
                            priceString += new string(' ', 7 - priceString.Length);

                        lines.Add(string.Format("{0} {1} {2} {3} {4}", upc, description, qtyString, priceString, amountString));

                        totalUnit += qty;
                        totalAmount += amount;
                    }
                    else
                    {
                        lines.Add(string.Format("{0} {1}", new string(' ', 12), description));
                    }
                }

                
            }

            lines.Add(dotsLine);

            string totalUnitString = totalUnit.ToString();
            if (totalUnitString.Length < 13)
                totalUnitString += new string(' ', 13 - totalUnitString.Length);

            lines.Add(string.Format("                                           Totals:  {0} {1}", totalUnitString, totalAmount.ToCustomString()));

            return totalAmount;
        }
    }
}