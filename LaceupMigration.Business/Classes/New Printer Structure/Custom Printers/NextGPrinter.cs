


using System;
using System.Collections.Generic;
using System.Threading;

using System.Globalization;
using System.Linq;
using System.Text;


namespace LaceupMigration
{
    public class NextGPrinter : ZebraFourInchesPrinter1
    {
        protected const string NextGTableHeader1 = "NextGTableHeader1";
        protected const string NextGTableHeader2 = "NextGTableHeader2";
        protected const string NextGTableLine = "NextGTableLine";
        protected const string NextGTableTotal = "NextGTableTotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(NextGTableHeader1, "^FO40,{0}^ABN,18,10^FDItem^FS" +
                "^FO140,{0}^ABN,18,10^FDQty^FS" +
                "^FO260,{0}^ABN,18,10^FDDescription^FS" +
                "^FO570,{0}^ABN,18,10^FDNet^FS" +
                "^FO680,{0}^ABN,18,10^FDInvoice^FS");

            linesTemplates.Add(NextGTableHeader2, "^FO40,{0}^ABN,18,10^FDCode^FS" +
                "^FO140,{0}^ABN,18,10^FDUnit^FS" +
                "^FO260,{0}^ABN,18,10^FDUPC^FS" +
                "^FO570,{0}^ABN,18,10^FDPrice^FS" +
                "^FO680,{0}^ABN,18,10^FDAmount^FS");

            linesTemplates.Add(NextGTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO140,{0}^ABN,18,10^FD{2}^FS" +
                "^FO260,{0}^ABN,18,10^FD{3}^FS" +
                "^FO570,{0}^ABN,18,10^FD{4}^FS" +
                "^FO680,{0}^ABN,18,10^FD{5}^FS");

            linesTemplates.Add(NextGTableTotal, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO140,{0}^ABN,18,10^FD{2}^FS" +
                "^FO680,{0}^ABN,18,10^FD{3}^FS");

            linesTemplates[OrderDetailsLinesUpcText] = "^FO260,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderDetailsLinesUpcBarcode] = "^FO260,{0}^BUN,40^FD{1}^FS";
        }

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NextGTableHeader1], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NextGTableHeader2], startY));
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
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;

                        string code = string.Empty;
                        if (!string.IsNullOrEmpty(p.Code))
                            code = p.Code;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NextGTableLine], startIndex,
                            code, qtyAsString, pName, priceAsString, totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NextGTableLine], startIndex,
                            "", "", pName, "", ""));
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

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
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


            if (!Config.HideTotalOrder && t == null)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    var balanceText = ToString(balance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NextGTableTotal],
                        startIndex,
                        key,
                        Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture),
                        balanceText));
                    startIndex += font18Separation;
                    offset++;
                }
            }

            return list;
        }


    }
}