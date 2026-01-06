using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class RamsaranPrinter : ZebraThreeInchesPrinter1
    {
        public RamsaranPrinter() : base()
        {
            Config.OrderDatePrintFormat = "dddd, dd MMMM yyyy HH:mm tt";
        }

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = 
                 "^FO15,{0}^A0N,28,28^FDPRODUCT^FS" +
                "^FO230,{0}^A0N,28,28^FDQTY^FS" +
                "^FO290,{0}^A0N,28,28^FDPRICE^FS" +
                "^FO390,{0}^A0N,28,28^FDVAT^FS" +
                "^FO480,{0}^A0N,28,28^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = 
                 "^FO15,{0}^A0N,28,28^FD{1}^FS" +
                "^FO230,{0}^A0N,28,28^FD{2}^FS" +
                "^FO290,{0}^A0N,28,28^FD{3}^FS" +
                "^FO390,{0}^A0N,28,28^FD{4}^FS" +
                "^FO480,{0}^A0N,28,28^FD{5}^FS";

            linesTemplates[OrderDetailsTotals] = 
                 "^FO15,{0}^A0N,28,28^FD{1}^FS" +
                "^FO100,{0}^A0N,28,28^FD{2}^FS" +
                "^FO230,{0}^A0N,28,28^FD{3}^FS" +
                "^FO480,{0}^A0N,28,28^FD{4}^FS";

            linesTemplates[OrderTotalsTotalDue] = "^CF0,50^FO20,{0}^FDTOTAL DUE: {1}^FS";

            //client requested bigger font :)
            linesTemplates[OrderTotalsNetQty] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDNET QTY:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsSales] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDSALES:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsCredits] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDCREDITS:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsReturns] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDRETURNS:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsNetAmount] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDNET AMOUNT:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsDiscount] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDDISCOUNT:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsTax] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FD{1}^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{2}^FS";
            //linesTemplates[OrderTotalsTotalDue] = "^FO40,{0}^A0N,28,28^FB390,1,0,R^FDTOTAL DUE:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsTotalPayment] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDTOTAL PAYMENT:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsCurrentBalance] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDINVOICE BALANCE:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance] = "^FO40,{0}^A0N,28,28^FB220,1,0,R^FDOPEN BALANCE:^FS^FO40,{0}^A0N,28,28^FB390,1,0,R^FD{1}^FS";
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                string uomString = null;

                if (detail.Product.ProductType != ProductType.Discount)
                {
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
                }

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;
                var productSlices = GetOrderDetailsRowsSplitProductName(name);

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

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;

                        double vat = d * order.TaxRate;
                        if (!detail.Product.Taxable)
                            vat = 0;

                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, qtyAsString, priceAsString, vat.ToCustomString(), totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, pName));
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
                                list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                    item.Lot, item.Qty.ToString()));
                                startIndex += font18Separation;
                            }
                        }
                        else
                        {
                            if (Config.PrintLotOrder)
                            {
                                list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                    item.Lot, item.Qty.ToString()));
                                startIndex += font18Separation;
                            }
                        }
                }

                // anderson crap
                // the retail price
                var extraProperties = order.Client.ExtraProperties;
                if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                {
                    var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                    if (retailPrice != null)
                    {
                        string retPriceString = "                                  " + ToString(Convert.ToDouble(retailPrice.Item2));
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesRetailPrice], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 16, 16);
        }

        protected string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (uomMap.Keys.Count > 0)
            {
                if (totalQtyNoUoM > 0)
                    uomMap.Add(string.Empty, totalQtyNoUoM);
                uomMap.Add("Totals:", uomMap.Values.Sum(x => x));
            }
            else
            {
                uomMap.Add("Totals:", totalQtyNoUoM);
                if (totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION")
                    uomMap.Add("Units:", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();
            var balanceText = ToString(balance);

            if (!Config.HideTotalOrder && t == null)
            {
                var key = uomKeys[0];
                list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, uomMap[key].ToString(), balanceText));
                startIndex += font18Separation;
                uomKeys.Remove(key);
            }
            if (uomKeys.Count > 0)
            {
                foreach (var key in uomKeys)
                {
                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty));
                    startIndex += font18Separation;
                }
            }

            return list;
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

            double paid = 0;
            if (payment != null)
            {
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

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

                    salesBalance += od.Price * factor * qty;

                    if (sales[key].Product.Taxable)
                        taxableAmount += od.Price * qty * factor;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalCredit += qty;

                    creditBalance += od.Price * factor * qty * -1;
                    if (credit[key].Product.Taxable)
                        taxableAmount -= od.Price * qty * factor;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalReturn += qty;

                    returnBalance += od.Price * qty * factor * -1;
                    if (returns[key].Product.Taxable)
                        taxableAmount -= od.Price * qty * factor;
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (!Config.HideTotalOrder && t == null)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s1 = ToString(salesBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s1 = ToString(creditBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s1 = ToString(returnBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsReturns], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString((salesBalance + creditBalance + returnBalance));
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if (order.Client.UseDiscount || order.Client.UseDiscountPerLine)
                {
                    s1 = ToString(Math.Abs(discount));
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                    startY += font36Separation;
                }

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                s1 = ToString(tax);
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                startY += font36Separation;


                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = ToString(s4);
                s1 = s1 = new string(' ', 11 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += 50;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s1 = ToString(paid);
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;

                    s1 = ToString((s4 - paid));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                startY += font18Separation;
                var clines = GetOrderSplitComment(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string format = linesTemplates[OrderComment];
                    if (i > 0)
                        format = linesTemplates[OrderComment2];

                    list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                    startY += font18Separation;
                }

            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            return list;
        }
    }
}