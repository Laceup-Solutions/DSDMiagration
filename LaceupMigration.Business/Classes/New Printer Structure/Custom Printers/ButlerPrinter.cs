





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class ButlerPrinter : ZebraFourInchesPrinter1
    {

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            var newLines = GroupLines(lines);   

            foreach (var detail in newLines)
            {
                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                string uomString = null;
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.UoM != null)
                    {
                        uomString = detail.UoM != null ? detail.UoM.Name : string.Empty;
                        if (!uomMap.ContainsKey(uomString))
                            uomMap.Add(uomString, 0);
                        uomMap[uomString] += detail.Qty;

                        totalQtyNoUoM += detail.Qty;
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

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;
                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, qtyAsString, totalAsString, priceAsString));
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

        public List<OrderLine> GroupLines(IList<OrderLine> lines)
        {
            var newLines = new List<OrderLine>();

            foreach (var det in lines)
            {
                if (det.OrderDetail.UnitOfMeasure != null && det.OrderDetail.UnitOfMeasure.Conversion > 1)
                {
                    var conversion = det.OrderDetail.UnitOfMeasure.Conversion;
                    det.UoM = det.Product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);
                    det.Qty = (conversion * det.Qty);

                    det.Price /= conversion;
                    det.Price = Math.Round(det.Price, Config.Round);

                    var found = newLines.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Price == det.Price);
                    if (found != null)
                        found.Qty += det.Qty;
                    else
                        newLines.Add(det);
                }
                else
                {
                    var found = newLines.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Price == det.Price);
                    if (found != null)
                        found.Qty += det.Qty;
                    else
                        newLines.Add(det);
                }
            }

            return newLines;

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

            if (printTotal)
            {
                list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, "EACH: ", Math.Round(totalQtyNoUoM, Config.Round).ToString(CultureInfo.CurrentCulture), ToString(balance)));
                startIndex += font18Separation;
            }

            return list;
        }
    }
}