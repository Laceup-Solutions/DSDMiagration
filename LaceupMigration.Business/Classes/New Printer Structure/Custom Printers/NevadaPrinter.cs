





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class NevadaPrinter : RetailPricePrinter
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[RetailPriceTableHeader] = "^FO40,{0}^ADN,18,10^FDQty^FS" +
                "^FO100,{0}^ADN,18,10^FDItem^FS" +
                "^FO260,{0}^ADN,18,10^FDDescription^FS" +
                "^FO465,{0}^ADN,18,10^FDPrice^FS" +
                "^FO570,{0}^ADN,18,10^FDRetail^FS" +
                "^FO675,{0}^ADN,18,10^FDAmount^FS";

            linesTemplates[OrderDetailsLineSecondLine] = "^FO260,{0}^ADN,18,10^FD{1}^FS";

            linesTemplates[RetailPriceTableLine] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO100,{0}^ADN,18,10^FD{2}^FS" +
                "^FO260,{0}^ADN,18,10^FD{3}^FS" +
                "^FO480,{0}^ADN,18,10^FD{4}^FS" +
                "^FO570,{0}^ADN,18,10^FD{5}^FS" +
                "^FO675,{0}^ADN,18,10^FD{6}^FS";

            linesTemplates[FooterSignatureText] = "^FO40,{0}^ADN,18,10^FDReceived By:^FS";

        }

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            if (sales.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, sales.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(sales.Values.ToList());

                var listXX = lines.ToList();
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
                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, "SALES SECTION", factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "DUMP SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "RETURNS SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            // list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], "      " + sectionName, startIndex));
            // startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            double balanceRP = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                Product p = detail.Product;

                string uomString = null;
                if (detail.OrderDetail.UnitOfMeasure != null)
                {
                    uomString = detail.OrderDetail.UnitOfMeasure.Name;
                    if (!uomMap.ContainsKey(uomString))
                        uomMap.Add(uomString, 0);
                    uomMap[uomString] += detail.Qty;
                }
                else
                {
                    if (!detail.OrderDetail.SkipDetailQty(order))
                    {
                        totalQtyNoUoM += detail.Qty;
                        try
                        {
                            totalUnits += detail.Qty * Convert.ToInt32(detail.OrderDetail.Product.Package);
                        }
                        catch { }
                    }

                }

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;

                if (!string.IsNullOrEmpty(p.Upc))
                {
                    if (name.Contains(p.Upc))
                    {
                        name = name.Replace(p.Upc, "");
                        name = name.TrimStart();
                        name = name.TrimEnd();
                    }
                }

                var productSlices = GetDetailsRowsSplitProductName1(name);

                var retPrice = GetRetailPrice(p, order.Client);

                var extRetailPrice = retPrice;
                extRetailPrice *= detail.Qty;
                if (detail.UoM != null)
                    extRetailPrice *= detail.UoM.Conversion;

                var upcAsString = p.Upc.ToString();



                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        double price = detail.Price * factor;

                        balance += d;
                        balanceRP += extRetailPrice;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            qtyAsString, upcAsString, pName, priceAsString, retPrice.ToCustomString(), totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSecondLine], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                foreach (var item in detail.ParticipatingDetails)
                {
                    if (!string.IsNullOrEmpty(item.Lot))
                        if (preOrder)
                        {
                            if (Config.PrintLotPreOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex,
                                    "Lot: " + item.Lot + "  Qty: " + item.Qty));
                                startIndex += font18Separation;
                            }
                        }
                        else
                        {
                            if (Config.PrintLotOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex,
                                    "Lot: " + item.Lot + "  Qty: " + item.Qty));
                                startIndex += font18Separation;
                            }
                        }
                }

                if (!string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            return list;
        }

        private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        protected override IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 17, 17);
        }

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetTotalsRowsInOneDocAllowance(ref startY, client, sales, credit, returns, payment, order);

            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;

            double totalSales = 0;
            double totalCredit = 0;
            double totalReturn = 0;

            double totalRetail = 0;

            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            // payment.Components.Sum(x => x.Amount) : 0;
            double taxableAmount = 0;


            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            foreach (var key in sales.Keys)
            {
                foreach (var od in sales[key].ParticipatingDetails)
                {

                    float qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

                    totalSales += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);


                    var retPrice = GetRetailPrice(od.Product, order.Client);
                    totalRetail += (retPrice * qty);

                    salesBalance += x;

                    if (sales[key].Product.Taxable)
                        taxableAmount += x;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalCredit += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    var retPrice = GetRetailPrice(od.Product, order.Client);
                    totalRetail -= (retPrice * qty);

                    creditBalance += x * -1;
                    if (credit[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalReturn += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    var retPrice = GetRetailPrice(od.Product, order.Client);
                    totalRetail -= (retPrice * qty);

                    returnBalance += x * -1;
                    if (returns[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }

            string s;
            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");
            if (!Config.HideTotalOrder && t == null)
            {
                double discount = order.CalculateDiscount();
                double tax = Math.Round(taxableAmount * order.TaxRate, 3);

                // right justified
                s = "TOTAL PRICE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = s4.ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                s = "TOTAL RETAIL:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;

                var s5 = totalRetail;
                s1 = s5.ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                startY += font18Separation;
                var clines = OrderCommentsSplit(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string prefix = string.Empty;
                    if (i == 0)
                        prefix = "Comments: ";
                    else
                        prefix = "          ";
                    list.Add(string.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, prefix + clines[i]));
                    startY += font18Separation;
                }

            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                var PaymentComment = "^FO40,{0}^ADN,18,10^FDPayment Comments: {1}^FS";
                var PaymentComment1 = "^FO40,{0}^ADN,18,10^FD                  {1}^FS";

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            return list;
        }

    }
}