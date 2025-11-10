






using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class MilagroPrinter : ZebraFourInchesPrinter1
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

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && order.UniqueId != null && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            lines.AddRange(GetHeaderRowsInOneDoc(ref startY, asPreOrder, order, order.Client, printedId, payments, payments != null && payments.Sum(x => x.Amount) == balance, fromBatch));

            lines.AddRange(GetOrderLabel(ref startY, order, asPreOrder, fromBatch));

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));

            AddExtraSpace(ref startY, lines, 36, 1);

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && (order.UniqueId != null && x.OrderId.Contains(order.UniqueId)));
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));

            if (Config.ExtraSpaceForSignature > 0)
                startY += Config.ExtraSpaceForSignature * font36Separation;

            // add the signature
            lines.AddRange(GetSignatureSection(ref startY, order, asPreOrder, lines));

            var discount = order.CalculateDiscount();
            var orderSales = order.CalculateItemCost();

            if (discount == orderSales && !string.IsNullOrEmpty(Config.Discount100PercentPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomDiscountSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            if (!PrintLines(lines))
                return false;

            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TICKETTYPE");
                if (terms != null && terms.Item2 == "4")
                    if (order.DeletedDetails.Count > 0)
                        PrintShortageReport(order);
                    else
                        foreach (var detail in order.Details)
                            if (detail.Ordered != detail.Qty && detail.Ordered > 0)
                            {
                                PrintShortageReport(order);
                                break;
                            }
            }
            return true;
        }

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetDetailsRowsInOneDocForAllowance(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();

            list.AddRange(GetDetailTableHeader(ref startY));

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            // anderson crap
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


                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(0), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            bool isAldi = false;

            if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString) && (order.Client.NonvisibleExtraPropertiesAsString.ToLower().Contains("printinvoiceinbaseuom=1")))
                isAldi = true;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            var totalCases = lines.Where(x => x.OrderDetail != null && x.OrderDetail.UnitOfMeasure != null && x.OrderDetail.UnitOfMeasure.IsDefault).Sum(x => x.OrderDetail.Qty);
            if (!uomMap.ContainsKey("Total Case Count"))
                uomMap.Add("Total Case Count", totalCases);

            foreach (var detail in lines)
            {
                bool isDisocuntItem = false;
                if (detail.Product.IsDiscountItem)
                {
                    isDisocuntItem = true;
                }

                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                string uomString = null;
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null)
                    {
                        if (isAldi)
                        {
                            var defaultUoM = detail.OrderDetail.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                            string defaultUOMNAE = defaultUoM != null ? defaultUoM.Name : string.Empty;
                            uomString = detail.Product.Code + " " + defaultUOMNAE;
                            if (!uomMap.ContainsKey(uomString))
                                uomMap.Add(uomString, 0);

                            double conversion = 1;
                            var duom = detail.OrderDetail.UnitOfMeasure;
                            if (duom != null && duom.IsBase)
                                conversion = defaultUoM.Conversion;

                            uomMap[uomString] += detail.Qty / (float)conversion;

                            totalQtyNoUoM += detail.Qty / (float)conversion;
                        }
                        else
                        {
                            var baseUom = detail.OrderDetail.Product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);

                            string baseUOMNAE = baseUom != null ? baseUom.Name : string.Empty;

                            uomString = detail.Product.Code + " " + baseUOMNAE;
                            if (!uomMap.ContainsKey(uomString))
                                uomMap.Add(uomString, 0);
                            uomMap[uomString] += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;

                            totalQtyNoUoM += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;
                        }
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

                            uomString = detail.Product.Code;
                            if (!uomMap.ContainsKey(uomString))
                                uomMap.Add(uomString, 0);
                            uomMap[uomString] += detail.Qty;
                        }
                    }
                }

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;

                if (isDisocuntItem)
                    name = detail.OrderDetail.Comments;

                var productSlices = GetOrderDetailsRowsSplitProductName(name);

                string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                if (detail.OrderDetail.UnitOfMeasure != null)
                    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                var splitQtyAsString = SplitProductName(qtyAsString, 10, 10);

                if (isAldi)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null && !detail.OrderDetail.UnitOfMeasure.IsBase)
                    {
                        var baseUom = detail.Product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);
                        qtyAsString = Math.Round(detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion, 2).ToString(CultureInfo.CurrentCulture);
                        if (baseUom != null)
                            qtyAsString += " " + baseUom.Name;

                        splitQtyAsString = SplitProductName(qtyAsString, 10, 10);
                    }
                }

                foreach (string pName in productSlices)
                {
                    string currentQty = (productLineOffset < splitQtyAsString.Count) ? splitQtyAsString[productLineOffset] : string.Empty;

                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

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

                        if (!isDisocuntItem)
                            balance += d;

                        //string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        //if (detail.OrderDetail.UnitOfMeasure != null)
                        //    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = ToString(price);

                        if (isAldi && detail.OrderDetail.UnitOfMeasure != null && !detail.OrderDetail.UnitOfMeasure.IsBase)
                            priceAsString = ToString(Math.Round((detail.Price / detail.OrderDetail.UnitOfMeasure.Conversion), Config.Round));

                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;
                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, currentQty, totalAsString, priceAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, pName, currentQty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                while (productLineOffset < splitQtyAsString.Count)
                {
                    string remainingQty = splitQtyAsString[productLineOffset];
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, string.Empty, remainingQty)); //OrderDetailsLines2
                    startIndex += font18Separation;

                    productLineOffset++;
                }


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
                            {
                                var temp_weight = DoFormat(item.Weight);
                                weights += temp_weight + " ";
                            }
                        }
                    }
                }
                else if (!order.AsPresale)
                {
                    /* foreach (var detail1 in detail.ParticipatingDetails)
                     {
                         if (!detail1.Product.SoldByWeight)
                             continue;

                         if (!string.IsNullOrEmpty(weights))
                             weights += ",";
                         weights += detail1.Weight;
                     }*/

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

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()) && !isDisocuntItem)
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

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            bool isAldi = false;

            if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString) && (order.Client.NonvisibleExtraPropertiesAsString.ToLower().Contains("printinvoiceinbaseuom=1")))
                isAldi = true;

            if (uomMap.Count > 0)
            {
                if (!uomMap.ContainsKey(isAldi ? "Cases" : "Units"))
                    uomMap.Add(isAldi ? "Cases" : "Units", totalQtyNoUoM);
            }
            else
            {
                uomMap.Add("Totals", totalQtyNoUoM);

                if (uomMap.Keys.Count == 1 && totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION" && !uomMap.ContainsKey(isAldi ? "Cases" : "Units"))
                    uomMap.Add(isAldi ? "Cases" : "Units", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();


            if (!Config.HideSubTotalOrder && printTotal)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    string adjustedKey = AdjustPadding(key);
                    var balanceText = ToString(balance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    //list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText)); 
                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals1, startIndex, string.Empty, adjustedKey, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText)); //key //uoMLines[0]
                    startIndex += font18Separation;

                    offset++;
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
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
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
                    if (od.Product.IsDiscountItem)
                        continue;

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

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    salesBalance += x;

                    if (sales[key].Product.Taxable)
                        taxableAmount += x;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    if (od.Product.IsDiscountItem)
                        continue;

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
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    creditBalance += x * -1;

                    if (credit[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    if (od.Product.IsDiscountItem)
                        continue;

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
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    returnBalance += x * -1;

                    if (returns[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");


            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (!Config.HideTotalOrder && printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s1 = ToString(salesBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s1 = ToString(creditBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s1 = ToString(returnBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsReturns], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString(Math.Round((salesBalance + creditBalance + returnBalance), Config.Round));
                s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if ((order.Client.UseDiscount || order.Client.UseDiscountPerLine || order.IsDelivery || OrderDiscount.HasDiscounts) && !Config.HideDiscountTotalPrint)
                {
                    if (Config.ShowDiscountIfApplied)
                    {
                        if (discount != 0)
                        {
                            s1 = ToString(Math.Abs(discount));
                            s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                            startY += font36Separation;
                        }
                    }
                    else
                    {
                        s1 = ToString(Math.Abs(discount));
                        s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                        startY += font36Separation;
                    }
                }

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                if (!Config.HideTaxesTotalPrint)
                {
                    s1 = ToString(tax);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                    startY += font36Separation;
                }

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s1 = ToString(Math.Round(paid, Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;

                    s1 = ToString(Math.Round((s4 - paid), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (Config.PrintClientTotalOpenBalance)
                {
                    s1 = ToString(Math.Round(order.Client.CurrentBalance(), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
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
                var orderComments = order.Comments;

                var reasons = Reason.GetReasonsByType(ReasonType.ReShip);
                if (reasons.Count > 0 && reasons.Any(x => x.Description == order.Comments) && !order.Reshipped && order.IsDelivery)
                    orderComments = string.Empty;

                startY += font18Separation;
                var clines = GetOrderSplitComment(orderComments);
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


        protected override List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            string docName = "NOT AN INVOICE";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "NOT A BILL";
                }
            }

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, docName));
                    startY += font36Separation;
                }
            }
            else
            {
                bool credit = false;
                if (order != null)
                    credit = order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return;

                var orderheader = credit ? "FINAL RETURN INVOICE" : "FINAL INVOICE";

                if (order.IsDelivery && order.Reshipped)
                    orderheader = credit ? "REFUSED RETURN INVOICE" : "REFUSED INVOICE";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, orderheader));
                startY += font36Separation;

            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, name));
                startY += font36Separation;
            }

            return lines;
        }

        protected string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client, bool fromBatch)
        {
            string docName = "Invoice";

            if (!order.Finished && !fromBatch)
                docName = string.Empty;

            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Bill";
                    printExtraDocName = true;
                }
            }
            if (order.AsPresale)
            {
                docName = "Sales Order";
                printExtraDocName = false;
            }
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                docName = "Return";
                printExtraDocName = true;
            }

            return docName;
        }

        protected IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool asPreOrder, Order order, Client client, string printedId, List<DataAccess.PaymentSplit> payments, bool paidInFull, bool fromBatch)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = GetOrderDocumentName(ref printExtraDocName, order, client, fromBatch);

            string s1 = docName;

            bool isFinalized = fromBatch || order.Finished;

            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown && isFinalized)
                s1 = docName + ": " + order.PrintedOrderId;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, s1, string.Empty));
            startY += font36Separation;

            if (order.ConvertedInvoice)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, order.Date.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandardDueDateMilagro], startY, order.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            /*  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
              startY += font18Separation;

              var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
              if (salesman != null)
              {
                  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, salesman.Name));
                  startY += font18Separation;
              }*/
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            if (salesman != null)
            {
                if (order.IsDelivery)
                {
                    var originalSalesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, salesman.Name));
                    startY += font18Separation;

                    if (originalSalesman != null)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, originalSalesman.Name));
                        startY += font18Separation;
                    }
                }
                else
                {
                    if (salesman.Roles == SalesmanRole.Driver)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
                        startY += font18Separation;
                    }
                    else
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, Config.VendorName));
                        startY += font18Separation;
                    }
                }
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = linesTemplates[OrderBillTo];

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderBillTo1];
                }

                startY += font18Separation;
                addrFormat1 = linesTemplates[OrderShipTo];

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderShipTo1];
                }
            }
            else
            {
                foreach (string s in ClientAddress(client))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
                    startY += font18Separation;
                }
            }

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientLicenceNumber], startY, client.LicenceNumber));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderVendorNumber], startY, client.VendorNumber));
                startY += font18Separation;
            }

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, term));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = ToString(order.Client.OpenBalance);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderAccountBalance], startY, balance));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            if (Config.PrintInvoiceNumberDown)
                if (printExtraDocName)
                {
                    if (order.AsPresale && order.OrderType == OrderType.Order)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, ""));
                    startY += font36Separation + font18Separation;
                }

            if (!Config.HidePONumber)
            {
                if (!string.IsNullOrEmpty(order.PONumber))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
                else if (Config.AutoGeneratePO)
                {
                    order.PONumber = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
            }

            if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
            {
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            return lines;
        }


        protected const string OrderCreatedReportRefused = "OrderCreatedReportRefused";
        protected const string OrderCreatedReportDelivery = "OrderCreatedReportDelivery";
        protected const string OrderCreatedReportPP = "OrderCreatedReportPP";
        protected const string OrderCreatedReportCompanyName = "OrderCreatedReportCompanyName";
        protected const string OrderCreatedReportPageHeaderRight = "OrderCreatedReportPageHeaderRight";
        protected const string OrderCreatedreportSalesTypes = "OrderCreatedreportSalesTypes";
        protected const string SectionTotalSalesRegister = "SectionTotalSalesRegister";
        protected const string SectionTotalPaymentSubLine = "SectionTotalPaymentSubLine";
        protected const string OrderCreatedReportPageHeaderLeft = "OrderCreatedReportPageHeaderLeft";

        protected const string CustomSignature = "CustomSignature";
        protected const string CustomSignatureText1 = "CustomSignatureText1";
        protected const string CustomSignatureText = "CustomSignatureText";
        protected const string CustomEndPage = "CustomEndPage";
        protected const string OrderCreatedReportPageHeaderRightMoreText = "OrderCreatedReportPageHeaderRightMoreText";
        protected const string OrderCreatedReportPageHeaderLeftMoreText = "OrderCreatedReportPageHeaderLeftMoreText";

        protected const string StandardDueDateMilagro = "StandardDueDateMilagro";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            #region Orders Created

            linesTemplates[OrderCreatedReportHeader] = "^CF0,45^FO40,{0}^FDSales Register Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS";
            linesTemplates[PaymentReportHeader] = "^CF0,45^FO40,{0}^FDPayments Received Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS";
            linesTemplates.Add(OrderCreatedReportCompanyName, "^CF0,40^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportPageHeaderRight, "^FO330,{0}^CF0,20^FB200,1,0,R^FD{1}^FS^FO550,{0}^ADN,18,10^FB300,1,0,L^FD{2}^FS");
            linesTemplates.Add(OrderCreatedReportPageHeaderLeft, "^FO40,{0}^CF0,20^FB100,1,0,R^FD{1}^FS^FO150,{0}^ADN,18,10^FB300,1,0,L^FD{2}^FS");
            linesTemplates.Add(OrderCreatedreportSalesTypes, "^CF0,30^FO40,{0}^FD{1}^FS");

            linesTemplates.Add(StandardDueDateMilagro, "^FO40,{0}^ADN,18,10^FDDue Date: {1}^FS");

            linesTemplates.Add(CustomSignatureText, "^FO90,{0}^ADN,18,10^FDSales Rep.^FS^FO610,{0}^ADN,18,10^FDCashier^FS");
            linesTemplates.Add(CustomSignatureText1, "^FO90,{0}^ADN,18,10^FDSalesman Sign.^FS^FO610,{0}^ADN,18,10^FDWarehouse Sign.^FS");
            linesTemplates.Add(CustomSignature, "^FO40,{0}^ADN,18,10^FD-------------------^FS^FO550,{0}^ADN,18,10^FD-------------------^FS");
            linesTemplates.Add(CustomEndPage, "^FO0,{0}^ADN,18,10^FB730,1,0,C^FD***FINAL PRINT***^FS");

            linesTemplates.Add(SectionTotalSalesRegister, "^CF0,25^FO250,{0}^FD{1}^FS^CF0,25^FO610,{0}^FD{2}^FS");
            linesTemplates.Add(SectionTotalPaymentSubLine, "^CF0,25^FO220,{0}^FD{1}^FS^CF0,25^FO520,{0}^FD{2}^FS^CF0,25^FO620,{0}^FD{3}^FS");


            linesTemplates[OrderCreatedReporWorkDay] = "^FO40,{0}^ADN,18,10^FDClock In: {1}  Clock Out: {2} Worked: {3}h:{4}m^FS";
            linesTemplates[OrderCreatedReporBreaks] = "^FO40,{0}^ADN,18,10^FDBreaks Taken: {1}h:{2}m^FS";
            linesTemplates[OrderCreatedReportTableHeader] = "^FO40,{0}^AON,25,25^FDDate^FS" +
                "^FO150,{0}^AON,25,25^FDCustomer Name^FS" +
                "^FO480,{0}^AON,25,25^FDInvoice#^FS" +
                "^FO610,{0}^AON,25,25^FDAmount^FS" +
                "^FO700,{0}^AON,25,25^FDVar.^FS";
            linesTemplates[OrderCreatedReportTableLine] = "^FO40,{0}^AON,20,20^FD{1}^FS" +
                "^FO150,{0}^AON,20,20^FD{2}^FS" +
                "^FO480,{0}^AON,20,20^FD{3}^FS" +
                "^FO610,{0}^AON,20,20^FD{4}^FS" +
                "^FO700,{0}^AON,20,20^FD{5}^FS";
            linesTemplates[OrderCreatedReportTableLine1] = "^FO40,{0}^ANN,18,16^FDClock In: {1}    Clock Out: {2}     # Copies: {3}^FS";
            linesTemplates[OrderCreatedReportTableTerms] = "^FO40,{0}^ADN,18,10^FDTerms: {1}^FS";
            linesTemplates[OrderCreatedReportTableLineComment] = "^FO40,{0}^ABN,18,10^FDNS Comment: {1}^FS";
            linesTemplates[OrderCreatedReportTableLineComment1] = "^FO40,{0}^ABN,18,10^FDRF Comment: {1}^FS";

            linesTemplates[OrderCreatedReportSubtotal] = "^FO510,{0}^ABN,18,10^FDSubtotal:^FS^FO610,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[OrderCreatedReportTax] = "^FO510,{0}^ABN,18,10^FDTax:^FS^FO610,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[OrderCreatedReportTotals] = "^FO510,{0}^ABN,18,10^FDTotals:^FS^FO610,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[OrderCreatedReportPaidCust] = "^FO40,{0}^ABN,18,10^FDPaid Cust:           {1}^FS" +
                          "^FO500,{0}^ABN,18,10^FDVoided:       {2}^FS";
            linesTemplates[OrderCreatedReportChargeCust] = "^FO40,{0}^ABN,18,10^FDCharge Cust:         {1}^FS" +
                          "^FO500,{0}^ABN,18,10^FDDelivery:     {2}^FS";
            linesTemplates[OrderCreatedReportCreditCust] = "^FO40,{0}^ABN,18,10^FD^FS" +
                          "^FO500,{0}^ABN,18,10^FDP&P:          {2}^FS";
            linesTemplates[OrderCreatedReportExpectedCash] = "^FO40,{0}^ABN,18,10^FDExpected Cash Cust:  {1}^FS" +
                          "^FO500,{0}^ABN,18,10^FD  Refused:    {2}^FS";
            linesTemplates[OrderCreatedReportFullTotal] = "^FO40,{0}^ABN,18,10^FDTotal Sales:         {1}^FS" +
                          "^FO500,{0}^ABN,18,10^FDTime (Hours): {2}^FS";

            linesTemplates[OrderCreatedReportCreditTotal] = "^FO40,{0}^ABN,18,10^FD^FS" +
                          "^FO500,{0}^ABN,18,10^FDCredit Total: {2}^FS";
            linesTemplates[OrderCreatedReportNetTotal] = "^FO40,{0}^ABN,18,10^FD^FS" +
                          "^FO500,{0}^ABN,18,10^FD   Net Total: {2}^FS";
            linesTemplates[OrderCreatedReportBillTotal] = "^FO40,{0}^ABN,18,10^FD^FS" +
                          "^FO500,{0}^ABN,18,10^FD  Bill Total: {2}^FS";
            linesTemplates[OrderCreatedReportSalesTotal] = "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD Sales Total: {2}^FS";
            #endregion


            #region payment report

            linesTemplates[PaymentReportTableHeader] =
              "^FO40,{0}^AON,20,20^FDCustomer Name^FS" +
             "^FO340,{0}^AON,20,20^FDPayment^FS" +
             "^FO420,{0}^AON,20,20^FDInvoice #^FS" +
             "^FO520,{0}^AON,20,20^FDInvoice T.^FS" +
             "^FO620,{0}^AON,20,20^FDApplied^FS" +
             "^FO720,{0}^AON,20,20^FDVar.^FS";

            linesTemplates[PaymentReportTableHeader1] = "^FO40,{0}^AON,20,20FD^FS" +
                "^FO340,{0}^AON,20,20^FDMethod^FS" +
                "^FO420,{0}^AON,20,20^FD^FS" +
                "^FO520,{0}^AON,20,20^FD Amount^FS" +
                "^FO620,{0}^AON,20,20^FD^FS" +
                "^FO720,{0}^AON,20,20^FD^FS";

            linesTemplates[PaymentReportTableLine] = "^FO40,{0}^AON,20,20^FD{1}^FS" +
             "^FO350,{0}^AON,20,20^FD{2}^FS" +
             "^FO390,{0}^AON,20,20^FD{3}^FS" +
             "^FO520,{0}^AON,20,20^FD{4}^FS" +
             "^FO620,{0}^AON,20,20^FD{5}^FS" +
             "^FO720,{0}^AON,20,20^FD{6}^FS";

            linesTemplates.Add(OrderCreatedReportPageHeaderRightMoreText, "^FO330,{0}^CF0,20^FB200,1,0,R^FD{1}^FS^FO550,{0}^ADN,18,10^FB300,1,0,L^FD{2}^FS");
            linesTemplates.Add(OrderCreatedReportPageHeaderLeftMoreText, "^FO0,{0}^CF0,20^FB200,1,0,R^FD{1}^FS^FO210,{0}^ADN,18,10^FB300,1,0,L^FD{2}^FS");

            #endregion

            #region Settlement

            linesTemplates[InventorySettlementHeader] = "^CF0,45^FO40,{0}^FDInventory Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS";
            linesTemplates[InventorySettlementProductHeader] = "";
            linesTemplates[InventorySettlementTableHeader] =
                 "^FO40,{0}^AON,20,20^FDProduct^FS" +
                "^FO300,{0}^AON,20,20^FDUoM^FS" +
                "^FO380,{0}^AON,20,20^FDLoad^FS" +
                "^FO460,{0}^AON,20,20^FDAdj^FS" +
                "^FO540,{0}^AON,20,20^FDSls^FS" +
                "^FO600,{0}^AON,20,20^FDDump^FS" +
                "^FO670,{0}^AON,20,20^FDUnl^FS" +
                "^FO740,{0}^AON,20,20^FDOver^FS";
            linesTemplates[InventorySettlementTableHeader1] =
                "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO300,{0}^ABN,18,10^FD^FS" +
                "^FO380,{0}^ABN,18,10^FD^FS" +
                "^FO460,{0}^ABN,18,10^FDFS" +
                "^FO540,{0}^ABN,18,10^FD^FS" +
                "^FO600,{0}^ABN,18,10^FD^FS" +
                "^FO670,{0}^ABN,18,10^FDInv^FS" +
                "^FO740,{0}^ABN,18,10^FDShort^FS";
            linesTemplates[InventorySettlementProductLine] = "^FO40,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[InventorySettlementLotLine] = "^FO40,{0}^ADN,18,10^FDLot: {1}^FS";
            linesTemplates[InventorySettlementTableLine] =
                 "^FO40,{0}^AON,20,20^FD{1}^FS" +
                "^FO300,{0}^AON,20,20^FD{2}^FS" +
                "^FO380,{0}^AON,20,20^FD{3}^FS" +
                "^FO460,{0}^AON,20,20^FD{4}^FS" +
                "^FO540,{0}^AON,20,20^FD{5}^FS" +
                "^FO600,{0}^AON,20,20^FD{6}^FS" +
                "^FO670,{0}^AON,20,20^FD{7}^FS" +
                "^FO740,{0}^AON,20,20^FD{8}^FS";
            linesTemplates[InventorySettlementTableTotals] =
                 "^FO40,{0}^AON,20,20^FD^FS" +
                "^FO300,{0}^AON,20,20^FD{1}^FS" +
                "^FO380,{0}^AON,20,20^FD{2}^FS" +
                "^FO460,{0}^AON,20,20^FD{3}^FS" +
                "^FO540,{0}^AON,20,20^FD{4}^FS" +
                "^FO600,{0}^AON,20,20^FD{5}^FS" +
                "^FO670,{0}^AON,20,20^FD{6}^FS" +
                "^FO740,{0}^AON,20,20^FD{7}^FS";
            linesTemplates[InventorySettlementTableTotals1] =
                "^FO40,{0}^AON,20,20^FD^FS" +
               "^FO250,{0}^AON,20,20^FD{1}^FS" +
               "^FO380,{0}^AON,20,20^FD{2}^FS" +
               "^FO460,{0}^AON,20,20^FD{3}^FS" +
               "^FO540,{0}^AON,20,20^FD{4}^FS" +
               "^FO600,{0}^AON,20,20^FD{5}^FS" +
               "^FO670,{0}^AON,20,20^FD{6}^FS" +
               "^FO740,{0}^AON,20,20^FD{7}^FS";

            linesTemplates[InventorySettlementAssetTracking] = "^FO40,{0}^CF0,33^FB620,1,0,L^FDCRATES: {1}^FS";

            #endregion

            linesTemplates[PickTicketProductLine] = "^FO40,{0}^CF0,25,25^FD{1}^FS" +
                                                      "^FO200,{0}^ADN,18,10^FD{2}^FS" +
                                                      "^FO575,{0}^ADN,18,10^FD{3}^FS" +
                                                      "^FO700,{0}^ADN,18,10^FD{4}^FS";

            linesTemplates[PickTicketProductTotal] = "^FO40,{0}^CF0,32^FDTotal Case Count:^FS" +
                                                       "^FO575,{0}^CF0,32^FD{1}^FS" +
                                                       "^FO700,{0}^CF0,32^FD{2}^FS";


            linesTemplates[PickTicketProductHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT #^FS" +
                                                        "^FO200,{0}^ADN,18,10^FDDESCRIPTION^FS" +
                                                        "^FO560,{0}^ADN,18,10^FDQTY^FS" +
                                                        "^FO690,{0}^ADN,18,10^FDUOM^FS";

        }

        #region Sales Register Report

        protected override IEnumerable<string> GetOrderCreatedReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "End Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            CompanyInfo company = CompanyInfo.GetMasterCompany();
            if (company != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportCompanyName], startY, company.CompanyName));
                startY += font36Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotalTerm = 0;
            double chargeTotalTerm = 0;
            double subtotal = 0;
            double totalTax = 0;

            var payments = GetPaymentsForOrderCreatedReport();

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
            {
                if (!Config.IncludePresaleInSalesReport && order.AsPresale)
                    continue;

                var terms = order.Term.Trim();

                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:
                        //if (!string.IsNullOrEmpty(terms))
                        //{
                        //    if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                        //        || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                        //        cashTotalTerm += order.OrderTotalCost();
                        //    else
                        //        chargeTotalTerm += order.OrderTotalCost();
                        //}
                        //else
                        //    chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.Quote:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO" || terms == "CCOD" || terms == "C.C.O.D.")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Return:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO" || terms == "CCOD" || terms == "C.C.O.D.")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                }
            }


            List<Order> NotPaidOrders = new List<Order>();
            List<Order> CashOrders = new List<Order>();
            List<Order> CheckOrders = new List<Order>();

            foreach (var p in Order.Orders)
            {
                if (p.OrderType == OrderType.Load)
                    continue;

                if (!Config.PrintNoServiceInSalesReports && p.OrderType == OrderType.NoService)
                    continue;

                if (!Config.IncludePresaleInSalesReport && p.AsPresale && p.OrderType != OrderType.NoService)
                    continue;

                var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);

                if (payment != null)
                {
                    if (payment.PaymentMethod == InvoicePaymentMethod.Cash)
                    {
                        CashOrders.Add(p);
                    }
                    else
                    {
                        CheckOrders.Add(p);
                    }
                }
                else
                    NotPaidOrders.Add(p);
            }

            bool isFirstLine = true;

            double totalChargeSales = NotPaidOrders.Sum(x => x.OrderTotalCost());
            double totalCashSales = CashOrders.Sum(x => x.OrderTotalCost());
            double totalCheckSales = CheckOrders.Sum(x => x.OrderTotalCost());

            double varianceOver = 0;
            double varianceDue = 0;

            foreach (var p in NotPaidOrders.OrderBy(x => x.Date))
            {
                if (isFirstLine)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "Charge / AR"));
                    startY += 36;
                    isFirstLine = false;
                }

                var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);

                double variance = 0;
                if (payment != null)
                    variance = p.OrderTotalCost() - payment.Amount;

                var accNumber = string.Empty;

                if (!string.IsNullOrEmpty(p.Client.ExtraPropertiesAsString) && p.Client.ExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                if (string.IsNullOrEmpty(accNumber) && !string.IsNullOrEmpty(p.Client.NonvisibleExtraPropertiesAsString) && p.Client.NonvisibleExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                string clientname = p.Client.ClientName;
                if (clientname.Length > 35)
                    clientname = clientname.Substring(0, 35);

                var date = p.ShipDate != DateTime.MinValue ? p.ShipDate : p.Date;
                
                lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                        date.ToShortDateString(),
                                        clientname,
                                        p.PrintedOrderId,
                                        p.OrderTotalCost().ToCustomString(),
                                        variance != 0 ? variance.ToCustomString() : string.Empty
                                        ));
                startY += font18Separation;

                var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);
                if (batch != null)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLine1], startY,
                       batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies));
                    startY += font18Separation;

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;
                }

                if (p.Voided)
                    voided++;
                else if (p.Reshipped)
                    reshipped++;
                else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                    delivered++;
                else
                    dsd++;

                if (variance > 0)
                    varianceDue += variance;
                else
                    if (variance < 0)
                    varianceOver += variance;

            }


            if (NotPaidOrders.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalSalesRegister], startY, "Total Charge / AR: ", totalChargeSales.ToCustomString()));
                startY += 36;
                //total section
            }


            isFirstLine = true;

            foreach (var p in CashOrders.OrderBy(x => x.Date))
            {
                if (isFirstLine)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "Cash Sales"));
                    startY += 36;
                    isFirstLine = false;
                }


                var payment = payments.Where(x => x.UniqueId == p.UniqueId);

                double variance = 0;
                if (payment != null)
                    variance = payment.Sum(x => x.Amount) - p.OrderTotalCost();

                var accNumber = string.Empty;

                if (!string.IsNullOrEmpty(p.Client.ExtraPropertiesAsString) && p.Client.ExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                if (string.IsNullOrEmpty(accNumber) && !string.IsNullOrEmpty(p.Client.NonvisibleExtraPropertiesAsString) && p.Client.NonvisibleExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                string clientname = p.Client.ClientName;
                if (clientname.Length > 35)
                    clientname = clientname.Substring(0, 35);

                var date = p.ShipDate != DateTime.MinValue ? p.ShipDate : p.Date;

                lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                        date.ToShortDateString(),
                                        clientname,
                                        p.PrintedOrderId,
                                        p.OrderTotalCost().ToCustomString(),
                                        variance != 0 ? variance.ToCustomString() : string.Empty
                                        ));

                startY += font18Separation;

                var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);
                if (batch != null)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLine1], startY,
                       batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies));
                    startY += font18Separation;

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;
                }

                if (p.Voided)
                    voided++;
                else if (p.Reshipped)
                    reshipped++;
                else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                    delivered++;
                else
                    dsd++;

                if (variance > 0)
                    varianceDue += variance;
                else
                    if (variance < 0)
                    varianceOver += variance;

            }


            if (CashOrders.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalSalesRegister], startY, "Total Cash: ", totalCashSales.ToCustomString()));
                startY += 36;
                //total section
            }

            isFirstLine = true;

            foreach (var p in CheckOrders.OrderBy(x => x.Date))
            {
                if (isFirstLine)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "Check Sales"));
                    startY += 36;
                    isFirstLine = false;
                }


                var payment = payments.Where(x => x.UniqueId == p.UniqueId);

                double variance = 0;
                if (payment != null)
                    variance = payment.Sum(x => x.Amount) - p.OrderTotalCost();

                var accNumber = string.Empty;

                if (!string.IsNullOrEmpty(p.Client.ExtraPropertiesAsString) && p.Client.ExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                if (string.IsNullOrEmpty(accNumber) && !string.IsNullOrEmpty(p.Client.NonvisibleExtraPropertiesAsString) && p.Client.NonvisibleExtraPropertiesAsString.Contains("Account #"))
                {
                    var accNo = DataAccess.GetSingleUDF("Account #", p.Client.NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(accNo))
                        accNumber = accNo;
                }

                string clientname = p.Client.ClientName;
                if (clientname.Length > 35)
                    clientname = clientname.Substring(0, 35);

                var date = p.ShipDate != DateTime.MinValue ? p.ShipDate : p.Date;

                lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                        date.ToShortDateString(),
                                        clientname,
                                        p.PrintedOrderId,
                                        p.OrderTotalCost().ToCustomString(),
                                        variance != 0 ? variance.ToCustomString() : string.Empty
                                        ));

                startY += font18Separation;

                var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);
                if (batch != null)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLine1], startY,
                       batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies));
                    startY += font18Separation;

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;
                }

                if (p.Voided)
                    voided++;
                else if (p.Reshipped)
                    reshipped++;
                else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                    delivered++;
                else
                    dsd++;

                if (variance > 0)
                    varianceDue += variance;
                else
                    if (variance < 0)
                    varianceOver += variance;
            }


            if (CheckOrders.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalSalesRegister], startY, "Total Check: ", totalCheckSales.ToCustomString()));
                startY += 36;
                //total section
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            if (start == DateTime.MaxValue)
                start = end;

            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeft], startY, "Refused:", reshipped));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeft], startY, "Voided:", voided));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeft], startY, "Delivery:", delivered));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeft], startY, "Time (Hs):", totalTime));
            startY += font18Separation;

            startY -= font18Separation * 4;

            var paymentsFromOrders = InvoicePayment.List.Where(x => !string.IsNullOrEmpty(x.OrderId));
            double totalCash = 0;
            double totalOther = 0;

            foreach (var p in paymentsFromOrders)
            {
                foreach (var component in p.Components)
                {
                    if (component.PaymentMethod == InvoicePaymentMethod.Cash)
                        totalCash += component.Amount;
                    else
                        totalOther += component.Amount;
                }
            }


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Total Cash Collected:", ToString(totalCash)));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Total Check Collected:", ToString(totalOther)));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Total Cash/Check Sales:", ToString(totalCash + totalOther)));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Total Charge Sales:", ToString(totalChargeSales)));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Sales Total:", ToString(totalCash + totalOther + totalChargeSales)));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Variance Due:", ToString(varianceDue)));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Variance Over:", ToString(varianceOver)));
            startY += font18Separation;


            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignature], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignatureText], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomEndPage], startY));

            return lines;
        }

        #endregion


        protected string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5);
        }


        #region Payment received Report

        protected override IEnumerable<string> GetPaymentReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "End Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            CompanyInfo company = CompanyInfo.GetMasterCompany();
            if (company != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportCompanyName], startY, company.CompanyName));
                startY += font36Separation;
            }

            return lines;
        }

        protected IEnumerable<string> GetPaymentsReportTable(ref int startY, List<PaymentRow> rows, ref double negativeVariance, ref double positiveVariance)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            var paymentsCCOD = rows.Where(x => !string.IsNullOrEmpty(x.DocNumber)).ToList();
            var creditAccountPayments = rows.Where(x => string.IsNullOrEmpty(x.DocNumber)).ToList();

            bool isFirstLine = true;

            foreach (var paym in paymentsCCOD.GroupBy(x => x.UniqueId))
            {
                double totalAmountinPayment = 0;
                double totalAmountinInvoice = 0;

                var index = 0;
                foreach (var p in paym)
                {
                    double variance = 0;

                    if (isFirstLine)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "CCOD"));
                        startY += 36;
                        isFirstLine = false;
                    }

                    totalAmountinPayment += p.PaidAmountNumber;
                    totalAmountinInvoice += p.DocAmountNumber;

                    if (index == paym.Count() - 1)
                        variance = totalAmountinInvoice - totalAmountinPayment;

                    lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                                 p.ClientName,
                                 p.PaymentMethod,
                                 p.DocNumber,
                                 p.DocAmount,
                                 p.Paid,
                                 variance.ToCustomString()));

                    startY += font18Separation;

                    if (variance < 0)
                        negativeVariance += variance;
                    if (variance > 0)
                        positiveVariance += variance;

                    index++;
                }
            }

            if (paymentsCCOD.Count > 0)
            {
                startY += 10;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalPaymentSubLine], startY, "Total Non-Charge Collected: ", paymentsCCOD.Sum(x => x.DocAmountNumber).ToCustomString(), paymentsCCOD.Sum(x => x.PaidAmountNumber).ToCustomString()));
                startY += font18Separation;
                //total section
            }

            isFirstLine = true;

            foreach (var p in creditAccountPayments)
            {
                if (isFirstLine)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "Payment On Account"));
                    startY += 36;
                    isFirstLine = false;
                }


                lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                             p.ClientName,
                             p.PaymentMethod,
                             p.DocNumber,
                             p.DocAmount,
                             p.Paid,
                             0.ToCustomString()));

                startY += font18Separation;

            }

            if (creditAccountPayments.Count > 0)
            {
                startY += 10;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalPaymentSubLine], startY, "Total Cash Collected: ", creditAccountPayments.Sum(x => x.DocAmountNumber).ToCustomString(), creditAccountPayments.Sum(x => x.PaidAmountNumber).ToCustomString()));
                startY += font18Separation;
                //total section
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            startY += font18Separation;
            startY += font18Separation;


            if (RouteExpenses.CurrentExpenses != null)
            {
                #region expenses
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedreportSalesTypes], startY, "Expenses"));
                startY += font36Separation;

                foreach (var ex in RouteExpenses.CurrentExpenses.Details)
                {
                    var product = Product.Find(ex.ProductId);

                    lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                                (product != null ? product.Name : string.Empty),
                                ex.Amount.ToCustomString(),
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                    startY += font18Separation;
                }

                startY += 10;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SectionTotalSalesRegister], startY, "Total Expenses: ",
                    RouteExpenses.CurrentExpenses.Details.Sum(x => x.Amount).ToCustomString()));
                startY += font18Separation;


                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += font18Separation;
                #endregion
            }

            return lines;
        }

        public override List<PaymentRow> CreatePaymentReceivedDataStructure(ref double totalCash, ref double totalCheck, ref double totalcc, ref double totalmo, ref double totaltr, ref double total)
        {
            List<PaymentRow> rows = new List<PaymentRow>();

            var listToUse = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;
            foreach (var pay in listToUse)
            {
                int index = 0;
                List<string> docNumbers = pay.Invoices().Select(x => x.InvoiceNumber).ToList();
                if (docNumbers.Count == 0)
                    docNumbers = pay.Orders().Select(x => x.PrintedOrderId).ToList();
                else
                    docNumbers.AddRange(pay.Orders().Select(x => x.PrintedOrderId).ToList());

                var t = pay.Invoices().Sum(x => x.Balance);
                t += pay.Orders().Sum(x => x.OrderTotalCost());

                double reminder = 0;

                while (true)
                {
                    var row = new PaymentRow();
                    if (index == 0)
                    {
                        row.ClientName = pay.Client.ClientName;

                        var accNumber = string.Empty;

                        if (!string.IsNullOrEmpty(pay.Client.ExtraPropertiesAsString) && pay.Client.ExtraPropertiesAsString.Contains("Account #"))
                        {
                            var accNo = DataAccess.GetSingleUDF("Account #", pay.Client.ExtraPropertiesAsString);
                            if (!string.IsNullOrEmpty(accNo))
                                accNumber = accNo;
                        }

                        if (string.IsNullOrEmpty(accNumber) && !string.IsNullOrEmpty(pay.Client.NonvisibleExtraPropertiesAsString) && pay.Client.NonvisibleExtraPropertiesAsString.Contains("Account #"))
                        {
                            var accNo = DataAccess.GetSingleUDF("Account #", pay.Client.NonvisibleExtraPropertiesAsString);
                            if (!string.IsNullOrEmpty(accNo))
                                accNumber = accNo;
                        }

                        row.ClientAccount = accNumber;

                        int factor = 0;
                        if (pay.Voided)
                            factor = 6;

                        if (row.ClientName.Length > (26 - factor))
                            row.ClientName = row.ClientName.Substring(0, (25 - factor));

                        double paid = 0;

                        if (pay?.Components != null && index >= 0 && index < pay.Components.Count)
                        {
                            paid = pay.Components[index]?.Amount ?? 0;
                        }

                        if (pay.Components.Count > 1)
                        {

                            reminder = t - paid;
                        }

                        row.DocAmount = reminder > 0 ? ToString(paid) : ToString(t);
                        row.DocAmountNumber = reminder > 0 ? paid : t;
                    }
                    else
                    {
                        row.ClientName = string.Empty;
                        row.DocAmount = string.Empty;

                        double paid = 0;
                        if (pay.Components.Count > index)
                            paid = pay.Components[index].Amount;

                        row.DocAmountNumber = reminder > 0 ? reminder : 0;
                        row.DocAmount = reminder > 0 ? ToString(reminder) : "";
                        reminder = reminder - paid;
                    }
                    if (docNumbers.Count > index)
                        row.DocNumber = docNumbers[index];
                    else
                        row.DocNumber = docNumbers.Count > 0 ? docNumbers[0] : string.Empty;
                    if (pay.Components.Count > index)
                    {
                        if (!pay.Voided)
                        {
                            if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Cash)
                                totalCash += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Check)
                                totalCheck += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Credit_Card)
                                totalcc += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Money_Order)
                                totalmo += pay.Components[index].Amount;
                            else
                                totaltr += pay.Components[index].Amount;

                            total += pay.Components[index].Amount;
                        }

                        if (pay.Voided)
                            row.ClientName += "(Void)";

                        row.RefNumber = pay.Components[index].Ref;
                        var s = ToString(pay.Components[index].Amount);
                        //if (s.Length < 9)
                        //    s = new string(' ', 9 - s.Length) + s;
                        row.Paid = s;
                        row.PaidAmountNumber = pay.Components[index].Amount;
                        row.PaymentMethod = ReducePaymentMethod(pay.Components[index].PaymentMethod);
                    }
                    else
                    {
                        row.RefNumber = string.Empty;
                        row.Paid = string.Empty;
                        row.PaidAmountNumber = 0;
                        row.PaymentMethod = string.Empty;
                    }

                    row.UniqueId = pay.UniqueId;

                    rows.Add(row);

                    index++;
                    if (docNumbers.Count <= index && pay.Components.Count <= index)
                        break;
                }
            }

            return rows;
        }



        protected string GetPaymentReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6, v7);
        }

        public override bool PrintReceivedPaymentsReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetPaymentReportHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            double totalCash = 0;
            double totalCheck = 0;
            double totalcc = 0;
            double totalmo = 0;
            double totaltr = 0;
            double total = 0;

            var rows = CreatePaymentReceivedDataStructure(ref totalCash, ref totalCheck, ref totalcc, ref totalmo, ref totaltr, ref total);

            double negativeVariance = 0;
            double positiveVariance = 0;

            lines.AddRange(GetPaymentsReportTable(ref startY, rows, ref negativeVariance, ref positiveVariance));

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            var cashRows = rows.Where(x => x.PaymentMethod == "CA" && !string.IsNullOrEmpty(x.DocNumber));
            var othersRows = rows.Where(x => x.PaymentMethod != "CA" && !string.IsNullOrEmpty(x.DocNumber));
            var creditAccount = rows.Where(x => string.IsNullOrEmpty(x.DocNumber));

            var cashInvoicesTotal = cashRows.Sum(x => x.DocAmountNumber);
            var cashPaid = cashRows.Sum(x => x.PaidAmountNumber);

            var checkInvoicesTotal = othersRows.Sum(x => x.DocAmountNumber);
            var checkPaid = othersRows.Sum(x => x.PaidAmountNumber);

            var creditAccountTotal = creditAccount.Sum(x => x.PaidAmountNumber);

            var cashTotalFromAcc = creditAccount.Where(x => x.PaymentMethod == "CA").Sum(x => x.PaidAmountNumber);
            var cashTotalFromCheck = creditAccount.Where(x => x.PaymentMethod != "CA").Sum(x => x.PaidAmountNumber);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Cash Invoice Total:", cashInvoicesTotal.ToCustomString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Cash Received Total:", (cashPaid).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Check Invoice Total:", checkInvoicesTotal.ToCustomString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Checks Received:", (checkPaid).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Variance Owed:", positiveVariance.ToCustomString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Variance Over:", negativeVariance.ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "Variances Cannot", string.Empty));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderLeftMoreText], startY, "be combined:", string.Empty));
            startY += font18Separation;


            startY -= font18Separation * 9;

            double expensesTotal = (RouteExpenses.CurrentExpenses != null ? (RouteExpenses.CurrentExpenses.Details.Sum(x => x.Amount)) : 0);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "Expenses Total", expensesTotal.ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "Total Cash Deposit", ((cashPaid + cashTotalFromAcc) - expensesTotal).ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "Check Deposit", (checkPaid).ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "POA Check Deposit", cashTotalFromCheck.ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "Total Money Received:", (cashPaid + checkPaid + creditAccountTotal).ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRightMoreText], startY, "Total Money Rendered:", ((cashPaid + checkPaid + creditAccountTotal) - expensesTotal).ToCustomString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);
            startY += font18Separation * 2;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignature], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignatureText], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomEndPage], startY));


            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        #endregion

        #region Settlement report

        public override bool InventorySettlement(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetSettlementReportHeader(ref startY, index, count));

            InventorySettlementRow totalRow = new InventorySettlementRow();

            List<InventorySettlementRow> map = new List<InventorySettlementRow>();

            CreateSettlementReportDataStructure(ref totalRow, ref map);

            lines.AddRange(GetSettlementReportTable(ref startY, map, totalRow));

            startY += font18Separation * 2;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignature], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomSignatureText1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CustomEndPage], startY));


            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected override List<string> GetSettlementReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "End Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportPageHeaderRight], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            CompanyInfo company = CompanyInfo.GetMasterCompany();
            if (company != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportCompanyName], startY, company.CompanyName));
                startY += font36Separation;
            }

            return lines;
        }

        void CreateSettlementReportDataStructure(ref InventorySettlementRow totalRow, ref List<InventorySettlementRow> map)
        {
            map = DataAccess.ExtendedSendTheLeftOverInventory();

            foreach (var value in map)
            {
                if (value.IsEmpty)
                    continue;

                if (Config.ShortInventorySettlement && value.IsShort)
                    continue;

                var product = value.Product;

                totalRow.Product = product;
                totalRow.BegInv += value.BegInv;
                totalRow.LoadOut += value.LoadOut;
                totalRow.Adj += value.Adj;
                totalRow.TransferOff += value.TransferOff;
                totalRow.TransferOn += value.TransferOn;
                // totalRow.EndInventory += value.EndInventory > 0 ? value.EndInventory : 0;
                totalRow.EndInventory += value.EndInventory;
                totalRow.Dump += value.Dump;
                totalRow.DamagedInTruck += value.DamagedInTruck;
                totalRow.Unload += value.Unload;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += value.Sales;
                    totalRow.CreditReturns += value.CreditReturns;
                    totalRow.CreditDump += value.CreditDump;
                    totalRow.Reshipped += value.Reshipped;
                }
            }
        }

        protected override IEnumerable<string> GetSettlementReportTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow)
        {
            List<string> lines = new List<string>();

            var oldRound = Config.Round;
            Config.Round = 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double CaseTotalLoad = 0;
            double CaseTotalAdj = 0;
            double CaseTotalSales = 0;
            double CaseTotalCredit = 0;
            double CaseTotalUnload = 0;
            double CaseTotalOver = 0;

            double UnitTotalLoad = 0;
            double UnitTotalAdj = 0;
            double UnitTotalSales = 0;
            double UnitTotalCredit = 0;
            double UnitTotalUnload = 0;
            double UnitTotalOver = 0;


            foreach (var group in SortDetails.SortedDetails(map).GroupBy(x => x.Product.Name))
            {
                foreach (var p in group)
                {
                    var family = p.Product.UnitOfMeasures.ToList();
                    var defaultUom = family.FirstOrDefault(x => x.IsDefault);
                    var baseUom = family.FirstOrDefault(x => x.IsBase);

                    if (p.UoM != null && defaultUom != null && baseUom != null)
                    {
                        var load = p.LoadOut + p.BegInv;
                        var adj = p.Adj;
                        var sales = p.Sales;
                        var credits = p.CreditDump + p.CreditReturns;
                        var unload = p.Unload;
                        var over = !string.IsNullOrEmpty(p.OverShort) ? Double.Parse(p.OverShort) : 0;

                        var conversion = defaultUom.Conversion;

                        double unit_load = 0;
                        double unit_adj = 0;
                        double unit_sales = 0;
                        double unit_credits = 0;
                        double unit_unload = 0;
                        double unit_over = 0;
                        double case_load = 0;
                        double case_adj = 0;
                        double case_sales = 0;
                        double case_credits = 0;
                        double case_unload = 0;
                        double case_over = 0;

                        unit_load = load % conversion;
                        case_load = Math.Truncate(load / conversion);

                        unit_adj = adj % conversion;
                        case_adj = Math.Truncate(adj / conversion);

                        unit_sales = sales % conversion;
                        case_sales = Math.Truncate(sales / conversion);

                        unit_credits = credits % conversion;
                        case_credits = Math.Truncate(credits / conversion);

                        unit_unload = unload % conversion;
                        case_unload = Math.Truncate(unload / conversion);

                        unit_over = over % conversion;
                        case_over = Math.Truncate(over / conversion);

                        UnitTotalLoad += unit_load;
                        UnitTotalAdj += unit_adj;
                        UnitTotalSales += unit_sales;
                        UnitTotalCredit += unit_credits;
                        UnitTotalUnload += unit_unload;
                        UnitTotalOver += unit_over;

                        CaseTotalLoad += case_load;
                        CaseTotalAdj += case_adj;
                        CaseTotalSales += case_sales;
                        CaseTotalCredit += case_credits;
                        CaseTotalUnload += case_unload;
                        CaseTotalOver += case_over;

                        var newS = GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                              p.Product.Name,
                                              defaultUom != null ? defaultUom.Name : string.Empty,
                                              Math.Round(case_load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                              Math.Round(case_adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                              Math.Round(case_sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                              Math.Round(case_credits, Config.Round).ToString(CultureInfo.CurrentCulture),
                                              Math.Round(case_unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                              Math.Round(case_over, Config.Round).ToString(CultureInfo.CurrentCulture));
                        lines.Add(newS);
                        startY += font18Separation;

                        if (unit_adj > 0 || unit_load > 0 || unit_sales > 0 || unit_credits > 0 || unit_unload > 0 || unit_over > 0)
                        {

                            newS = GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                  "",
                                                  baseUom != null ? baseUom.Name : string.Empty,
                                                  Math.Round(unit_load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                  Math.Round(unit_adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                  Math.Round(unit_sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                  Math.Round(unit_credits, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                  Math.Round(unit_unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                  Math.Round(unit_over, Config.Round).ToString(CultureInfo.CurrentCulture));
                            lines.Add(newS);
                            startY += font18Separation;
                        }

                    }
                    else
                    {
                        var newS = GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                    p.Product.Name,
                                                    p.UoM != null ? p.UoM.Name : string.Empty,
                                                    Math.Round(p.LoadOut + p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.CreditDump + p.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    p.OverShort);
                        lines.Add(newS);
                        startY += font18Separation;
                    }

                    startY += 5;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals1, startY,
                                                "Total Cases: ",
                                                Math.Round(CaseTotalUnload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(CaseTotalAdj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(CaseTotalSales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(CaseTotalCredit, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(CaseTotalUnload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(CaseTotalOver, Config.Round).ToString(CultureInfo.CurrentCulture)));


            startY += font18Separation;

            if (UnitTotalLoad > 0 || UnitTotalAdj > 0 || UnitTotalSales > 0 || UnitTotalCredit > 0 || UnitTotalUnload > 0 || UnitTotalOver > 0)
            {
                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals1, startY,
                                            "Total Units: ",
                                            Math.Round(UnitTotalUnload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                            Math.Round(UnitTotalAdj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                            Math.Round(UnitTotalSales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                            Math.Round(UnitTotalCredit, Config.Round).ToString(CultureInfo.CurrentCulture),
                                            Math.Round(UnitTotalUnload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                            Math.Round(UnitTotalOver, Config.Round).ToString(CultureInfo.CurrentCulture)));
            }

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            Config.Round = oldRound;

            return lines;
        }

        protected string GetInventorySettlementTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8)
        {
            v1 = !string.IsNullOrEmpty(v1) ? v1.Substring(0, v1.Length > 25 ? 25 : v1.Length) : string.Empty;
            v2 = !string.IsNullOrEmpty(v2) ? v2.Substring(0, v2.Length > 5 ? 5 : v2.Length) : string.Empty;
            v3 = !string.IsNullOrEmpty(v3) ? v3.Substring(0, v3.Length > 4 ? 4 : v3.Length) : string.Empty;
            v4 = !string.IsNullOrEmpty(v4) ? v4.Substring(0, v4.Length > 4 ? 4 : v4.Length) : string.Empty;
            v5 = !string.IsNullOrEmpty(v5) ? v5.Substring(0, v5.Length > 4 ? 4 : v5.Length) : string.Empty;
            v6 = !string.IsNullOrEmpty(v6) ? v6.Substring(0, v6.Length > 4 ? 4 : v6.Length) : string.Empty;
            v7 = !string.IsNullOrEmpty(v7) ? v7.Substring(0, v7.Length > 4 ? 4 : v7.Length) : string.Empty;
            v8 = !string.IsNullOrEmpty(v8) ? v8.Substring(0, v8.Length > 4 ? 4 : v8.Length) : string.Empty;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8);
        }

        protected string GetInventorySettlementTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {
            v1 = !string.IsNullOrEmpty(v1) ? v1.Substring(0, v1.Length > 12 ? 12 : v1.Length) : string.Empty;
            v2 = !string.IsNullOrEmpty(v2) ? v2.Substring(0, v2.Length > 4 ? 4 : v2.Length) : string.Empty;
            v3 = !string.IsNullOrEmpty(v3) ? v3.Substring(0, v3.Length > 4 ? 4 : v3.Length) : string.Empty;
            v4 = !string.IsNullOrEmpty(v4) ? v4.Substring(0, v4.Length > 4 ? 4 : v4.Length) : string.Empty;
            v5 = !string.IsNullOrEmpty(v5) ? v5.Substring(0, v5.Length > 4 ? 4 : v5.Length) : string.Empty;
            v6 = !string.IsNullOrEmpty(v6) ? v6.Substring(0, v6.Length > 4 ? 4 : v6.Length) : string.Empty;
            v7 = !string.IsNullOrEmpty(v7) ? v7.Substring(0, v7.Length > 4 ? 4 : v7.Length) : string.Empty;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);
        }


        #endregion

        #region accepted load


        protected override IEnumerable<string> GetAcceptLoadDetailsRows(ref int startIndex, IEnumerable<InventoryLine> sortedList)
        {

            var oldset = font18Separation;
            font18Separation = font18Separation + font18Separation / 2;

            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 40, 1);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadTableHeader], startIndex));
            startIndex += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadTableHeader1], startIndex));
            startIndex += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;

            float requestedNoConversion = 0;
            float adjustmentNoConversion = 0;
            float startNoConversion = 0;

            #region new
            List<InvLineGrouped> grouped = new List<InvLineGrouped>();

            foreach (var a in sortedList)
            {
                var alreadyThere = grouped.FirstOrDefault(x => x.Product == a.Product && x.Lot == a.Lot && x.UoM == a.UoM);

                if (alreadyThere != null)
                {
                    alreadyThere.Starting += a.Starting;
                    alreadyThere.Real += a.Real;

                    if (string.IsNullOrEmpty(alreadyThere.Weights))
                        alreadyThere.Weights = "Weight: " + a.Weight;
                    else
                        alreadyThere.Weights += ", " + a.Weight;
                }
                else
                {
                    grouped.Add(new InvLineGrouped() { Product = a.Product, Lot = a.Lot, Real = a.Real, Starting = a.Starting, UoM = a.UoM, Weights = ("Weight: " + a.Weight.ToString()) });

                }
            }

            if (Config.NewAddItemRandomWeight)
            {

                foreach (var p in grouped)
                {
                    int productLineOffset = 0;
                    foreach (string pName in GetAcceptLoadDetailsRowsSplitProductName(p.Product.Name))
                    {
                        var lfy = p.Product.BeginigInventory;
                        var load = p.Starting;
                        var adj = p.Real - p.Starting;
                        float factor = 1;
                        if (p.Product.SoldByWeight)
                            factor = p.Starting;
                        var st = (p.Product.BeginigInventory * factor);

                        string uom = string.Empty;

                        if (p.UoM != null)
                        {
                            lfy /= p.UoM.Conversion;
                            st /= p.UoM.Conversion;

                            uom = p.UoM.Name;
                        }

                        st += p.Real;

                        //new
                        uom = string.Empty;

                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                Math.Round(load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(st, Config.Round).ToString(CultureInfo.CurrentCulture)));

                            leftFromYesterday += p.Product.BeginigInventory;

                            var real = p.Real;

                            requestedNoConversion += load;
                            adjustmentNoConversion += adj;
                            startNoConversion += (p.Product.BeginigInventory * factor) + real;

                            if (p.UoM != null)
                            {
                                load *= p.UoM.Conversion;
                                adj *= p.UoM.Conversion;
                                real *= p.UoM.Conversion;
                            }

                            requestedInventory += load;
                            adjustment += adj;

                            start += ((p.Product.BeginigInventory * factor) + real);
                        }
                        else
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                "",
                                "",
                                "",
                                ""));
                        }

                        productLineOffset++;
                        startIndex += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(p.Lot))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadLotLine], startIndex, p.Lot));
                        startIndex += font18Separation;
                    }

                    if (Config.UsePallets && p.Product.SoldByWeight && !string.IsNullOrEmpty(p.Weights))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadWeightLine], startIndex, p.Weights));
                        startIndex += font18Separation;
                    }
                }
            }
            else
            {
                foreach (var p in sortedList)
                {
                    int productLineOffset = 0;

                    int uomCount = 0;

                    List<string> uomStrings = new List<string>();

                    if (p.UoM != null)
                    {

                        var stringToHandle = p.UoM.Name;

                        if (stringToHandle.Length > 5)
                        {
                            var firstPart = stringToHandle.Substring(0, 5);
                            var secondPart = stringToHandle.Substring(5);

                            uomStrings.Add(firstPart);
                            uomStrings.Add(secondPart);
                        }
                        else
                            uomStrings.Add(p.UoM.Name);
                    }

                    foreach (string pName in GetAcceptLoadDetailsRowsSplitProductName(p.Product.Name))
                    {
                        var lfy = p.Product.BeginigInventory;
                        var load = p.Starting;
                        var adj = p.Real - p.Starting;
                        var st = p.Product.BeginigInventory;

                        string uom = string.Empty;

                        if (p.UoM != null)
                        {
                            lfy /= p.UoM.Conversion;
                            st /= p.UoM.Conversion;
                        }

                        if (uomStrings.Count > uomCount)
                            uom = uomStrings[uomCount];

                        st += p.Real;

                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                Math.Round(load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(st, Config.Round).ToString(CultureInfo.CurrentCulture)));

                            leftFromYesterday += p.Product.BeginigInventory;

                            var real = p.Real;

                            requestedNoConversion += load;
                            adjustmentNoConversion += adj;
                            startNoConversion += (p.Product.BeginigInventory) + real;

                            if (p.UoM != null)
                            {
                                load *= p.UoM.Conversion;
                                adj *= p.UoM.Conversion;
                                real *= p.UoM.Conversion;
                            }

                            requestedInventory += load;
                            adjustment += adj;

                            start += (p.Product.BeginigInventory + real);
                        }
                        else
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                "",
                                "",
                                ""));
                        }

                        productLineOffset++;
                        startIndex += font18Separation;
                        uomCount++;
                    }

                    if (!string.IsNullOrEmpty(p.Lot))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadLotLine], startIndex, p.Lot));
                        startIndex += font18Separation;
                    }

                    if (Config.UsePallets && p.Product.SoldByWeight)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadWeightLine], startIndex, ("Weight: " + p.Weight).ToString()));
                        startIndex += font18Separation;
                    }
                }

            }

            #endregion


            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            if (grouped.Any(x => x.UoM != null && x.UoM.Conversion > 1))
            {
                list.Add(GetAcceptLoadTableTotalsFixed(AcceptLoadTableTotals1, startIndex,
                     Math.Round(requestedNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture),
                     Math.Round(adjustmentNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture),
                     Math.Round(startNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture)));

                startIndex += font18Separation;

            }

            list.Add(GetAcceptLoadTableTotalsFixed(AcceptLoadTableTotals, startIndex,
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));


            var totalWeight = sortedList.Sum(x => (x.Weight * x.Real));
            if (totalWeight > 0)
            {
                list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, "Total Weight: " + totalWeight,
                               "",
                               "",
                               "",
                               ""));
            }

            startIndex += font18Separation;


            font18Separation = oldset;

            return list;
        }

        #endregion


        #region pick Ticket

        public override bool PrintPickTicket(Order order)
        {
            int startY = 80;

            List<string> lines = new List<string>();


            if (Config.ButlerCustomization)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Butler Foods"));
                startY += font36Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Pick Ticket - Not an Invoice"));
                startY += font36Separation;
            }
            else
            {
                var logoLabel = GetLogoLabel(ref startY, order);
                if (!string.IsNullOrEmpty(logoLabel))
                {
                    lines.Add(logoLabel);
                }

                AddExtraSpace(ref startY, lines, 36, 1);


                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Pick Ticket - Not an Invoice"));
                startY += font36Separation;

                lines.AddRange(GetCompanyRows(ref startY, order));

                AddExtraSpace(ref startY, lines, font18Separation, 1);
            }

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);
            if (salesman == null)
                salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            var truckName = string.Empty;
            var truck = Truck.Trucks.FirstOrDefault(x => x.DriverId == salesman.Id);
            if (truck != null)
                truckName = truck.Name;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketRouteInfo], startY, "Route #: " + truckName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDeliveryDate], startY, "Delivery Date: " + (order.ShipDate != DateTime.MinValue ? order.ShipDate.ToShortDateString() : DateTime.Now.ToShortDateString()), "Date: " + DateTime.Now.ToShortDateString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDriver], startY, "Driver #: " + salesman.Name, "Time: " + DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            var client = order.Client;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = linesTemplates[OrderBillTo];

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderBillTo1];
                }

                startY += font18Separation;
                addrFormat1 = linesTemplates[OrderShipTo];

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderShipTo1];
                }
            }
            else
            {
                foreach (string s in ClientAddress(client))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
                    startY += font18Separation;
                }
            }

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientLicenceNumber], startY, client.LicenceNumber));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderVendorNumber], startY, client.VendorNumber));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "PO#: " + order.PONumber));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "Ticket#: " + order.PrintedOrderId));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            var SE = string.Empty;
            SE = new string('-', WidthForNormalFont - SE.Length) + SE;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;


            var details = order.Details.OrderBy(x => x.Product.Description).ToList();

            foreach (var l in details)
            {
                string description = l.Product.Description;
                if (description.Length > 30)
                    description = description.Substring(0, 30);

                if (description.Contains(l.Product.Code))
                    description = description.Replace(l.Product.Code, "");

                description = description.Trim();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, description, l.Qty, (l.UnitOfMeasure != null ? l.UnitOfMeasure.Name : "")));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
                startY += font18Separation;
            }

            //totals
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductTotal], startY, order.Details.Sum(x => x.Qty), ""));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        #endregion
    }
}