using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BrooklingDelightsPrinter : ZebraFourInchesPrinter1
    {
        protected const string BrooklingDelightsPrinterTableHeader1 = "BrooklingDelightsPrinterTableHeader1";
        protected const string BrooklingDelightsPrinterTableHeader2 = "BrooklingDelightsPrinterTableHeader2";
        protected const string BrooklingDelightsPrinterTableLine1 = "BrooklingDelightsPrinterTableLine1";
        protected const string BrooklingDelightsPrinterTableLine2 = "BrooklingDelightsPrinterTableLine2";
        protected const string BrooklingDelightsAllowance = "BrooklingDelightsAllowance";
        protected const string BrooklingDelightsTerm = "BrooklingDelightsTerm";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(BrooklingDelightsPrinterTableHeader1, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS");

            linesTemplates.Add(BrooklingDelightsPrinterTableHeader2, "^FO170,{0}^ADN,18,10^FDQTY^FS" +
                "^FO250,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO350,{0}^ADN,18,10^FDALLOW.^FS" +
                "^FO450,{0}^ADN,18,10^FDNET PRICE^FS" +
                "^FO580,{0}^ADN,18,10^FDRETAIL^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");            
            
            linesTemplates.Add(BrooklingDelightsPrinterTableLine1, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BrooklingDelightsPrinterTableLine2, "^FO170,{0}^ADN,18,10^FD{1}^FS" +
                "^FO250,{0}^ADN,18,10^FD{2}^FS" +
                "^FO350,{0}^ADN,18,10^FD{3}^FS" +
                "^FO450,{0}^ADN,18,10^FD{4}^FS" +
                "^FO580,{0}^ADN,18,10^FD{5}^FS" +
                "^FO680,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates[OrderDetailsTotals] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO170,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS";

            linesTemplates.Add(BrooklingDelightsAllowance, "^FO40,{0}^ADN,36,20^FD      ALLOWANCE: {1}^FS");
            linesTemplates.Add(BrooklingDelightsTerm,      "^FO40,{0}^ADN,36,20^FD          TERMS: {1}^FS");
        }
        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsPrinterTableHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsPrinterTableHeader2], startY));
            startY += font18Separation;

            return lines;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            double totalAllowance = 0;

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

                        totalQtyNoUoM += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;
                    }
                    else
                    {
                        if (!detail.OrderDetail.SkipDetailQty(order))
                        {
                            int packaging = 0;

                            if (!string.IsNullOrEmpty(detail.OrderDetail.Product.Package))
                                int.TryParse(detail.OrderDetail.Product.Package, out packaging);

                            totalQtyNoUoM += detail.Qty;

                            if (packaging > 0)
                                totalUnits += detail.Qty * packaging;
                        }
                    }
                }

                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsPrinterTableLine1], startIndex, name));
                startIndex += font18Separation;

                double d = 0;
                foreach (var _ in detail.ParticipatingDetails)
                {
                    double qty = _.Qty;

                    if (_.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= _.Product.Weight;
                        else
                            qty = _.Weight;
                    }

                    d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                }

                double price = detail.Price * factor;
                double expectedPrice = detail.Product.PriceLevel0;
                if (detail.UoM != null)
                    expectedPrice *= detail.UoM.Conversion;
                expectedPrice *= factor;

                var retPrice = GetRetailPrice(p, order.Client);
                if (detail.UoM != null)
                    retPrice *= detail.UoM.Conversion;
                retPrice *= factor;

                balance += d;
                totalAllowance += detail.Qty * (expectedPrice - price) * factor;

                string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                if (detail.OrderDetail.UnitOfMeasure != null)
                    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                string priceString = ToString(expectedPrice);
                string allowanceString = ToString(expectedPrice - price);
                string netPriceString = ToString(price);
                string retailString = ToString(retPrice);
                string totalAsString = ToString(d);

                if (detail.Product.ProductType == ProductType.Discount)
                    qtyAsString = string.Empty;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsPrinterTableLine2], startIndex,
                    qtyAsString, priceString, allowanceString, netPriceString, retailString, totalAsString));
                startIndex += font18Separation;

                string weights = "";

                if (Config.PrintLotPreOrder || Config.PrintLotOrder)
                {
                    foreach (var item in detail.ParticipatingDetails)
                    {
                        var itemLot = item.Lot ?? "";
                        if (!string.IsNullOrEmpty(itemLot) && item.LotExpiration != DateTime.MinValue)
                            itemLot += "  Exp: " + item.LotExpiration.ToShortDateString();

                        string qty = item.Qty.ToString();
                        if (item.Product.SoldByWeight && !order.AsPresale)
                            qty = item.Weight.ToString();

                        if (!string.IsNullOrEmpty(itemLot))
                        {
                            if (preOrder)
                            {
                                if (Config.PrintLotPreOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                            else
                            {
                                if (Config.PrintLotOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                        }
                        else
                        {
                            if (item.Product.SoldByWeight && !order.AsPresale)
                                weights += item.Weight.ToString() + " ";
                        }
                    }
                }
                else if (!order.AsPresale)
                {
                    StringBuilder sb = new StringBuilder();
                    List<string> lotUsed = new List<string>();

                    int TotalCases = 0;

                    foreach (var detail1 in detail.ParticipatingDetails)
                    {
                        if (!string.IsNullOrEmpty(detail1.Lot) && detail1.Product.SoldByWeight)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            else
                                sb.Append("Weight: ");

                            if (detail1.Weight != 0)
                                sb.Append(detail1.Weight.ToString());


                            lotUsed.Add(detail1.Lot);
                            TotalCases++;
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                        startIndex += font18Separation;
                    }
                }

                if (!string.IsNullOrEmpty(weights))
                {
                    foreach (var item in GetOrderDetailsRowsSplitProductName(weights))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, item));
                        startIndex += font18Separation;
                    }
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeightsCount], startIndex, detail.ParticipatingDetails.Count));
                    startIndex += font18Separation;
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

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance, totalAllowance));

            return list;
        }

        private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        protected List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance, double allowance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (uomMap.Count > 0)
                uomMap.Add("Units:", totalQtyNoUoM);
            else
            {
                uomMap.Add("Totals:", totalQtyNoUoM);

                if (uomMap.Keys.Count == 0 && totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION")
                    uomMap.Add("Units:", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();


            if (!Config.HideSubTotalOrder && t == null)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    var balanceText = ToString(balance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    var allowanceText = ToString(allowance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    list.Add(string.Format(CultureInfo.InvariantCulture, 
                        linesTemplates[OrderDetailsTotals], startIndex, 
                        key, 
                        Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), 
                        balanceText));
                    startIndex += font18Separation;
                    offset++;
                }
            }

            return list;
        }

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;

            double salesAllowance = 0;
            double creditAllowance = 0;
            double returnAllowance = 0;

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

            foreach (var value in sales.Values)
            {
                var expectedPrice = value.Product.PriceLevel0;
                if (value.UoM != null)
                    expectedPrice *= value.UoM.Conversion;

                foreach (var od in value.ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalSales += qty;

                    var x = od.Price * factor * qty;
                    var y = (expectedPrice - od.Price) * factor * qty;

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);
                    y = double.Parse(Math.Round(y, Config.Round).ToCustomString(), NumberStyles.Currency);

                    salesBalance += x;
                    salesAllowance += y;

                    if (value.Product.Taxable)
                        taxableAmount += x;
                }
            }
            foreach (var value in credit.Values)
            {
                var expectedPrice = value.Product.PriceLevel0;
                if (value.UoM != null)
                    expectedPrice *= value.UoM.Conversion;

                foreach (var od in value.ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalCredit += qty;

                    var x = od.Price * factor * qty;
                    var y = (expectedPrice - od.Price) * factor * qty;

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);
                    y = double.Parse(Math.Round(y, Config.Round).ToCustomString(), NumberStyles.Currency);

                    creditBalance += x * -1;
                    creditAllowance += y * -1;

                    if (value.Product.Taxable)
                        taxableAmount -= x;
                }
            }
            foreach (var value in returns.Values)
            {
                var expectedPrice = value.Product.PriceLevel0;
                if (value.UoM != null)
                    expectedPrice *= value.UoM.Conversion;

                foreach (var od in value.ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalReturn += qty;

                    var x = od.Price * factor * qty;
                    var y = (expectedPrice - od.Price) * factor * qty;

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);
                    y = double.Parse(Math.Round(y, Config.Round).ToCustomString(), NumberStyles.Currency);

                    returnBalance += x * -1;
                    returnAllowance += y * -1;

                    if (value.Product.Taxable)
                        taxableAmount -= x;
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

                s1 = ToString((salesAllowance + creditAllowance + returnAllowance));
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsAllowance], startY, s1));
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
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (!string.IsNullOrEmpty(order.Term))
                {
                    s1 = order.Term;
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BrooklingDelightsTerm], startY, s1));
                    startY += font36Separation;
                }

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice && order.Client.AllowToCollectPayment)
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

                s1 = ToString(order.Client.CurrentBalance());
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                startY += font36Separation;

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