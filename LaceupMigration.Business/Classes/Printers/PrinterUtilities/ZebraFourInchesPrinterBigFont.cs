
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace LaceupMigration
{
    public class ZebraFourInchesPrinterBigFont : ZebraFourInchesPrinter
    {

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Remove(OrderDetailsLine);
            linesTemplates.Add(OrderDetailsLine, "^FO40,{0}^AEN,18,10^FD{1}^FS^FO310,{0}^AEN,18,10^FD{2}^FS^FS^FO480,{0}^AEN,18,10^FD{4}^FS^FO630,{0}^AEN,18,10^FD{3}^FS");
            linesTemplates.Remove(OrderDetailsLineSecondLine);
            linesTemplates.Add(OrderDetailsLineSecondLine, "^FO40,{0}^AEN,18,10^FD{1}^FS");
            linesTemplates.Remove(OrderDetailsLineLot);
            linesTemplates.Add(OrderDetailsLineLot, "^FO40,{0}^AEN,18,10^FD{1}^FS");
            linesTemplates.Remove(OrderDetailsLineUPC);
            linesTemplates.Add(OrderDetailsLineUPC, "^FO40,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Remove(OrderDetailsLineUPCText);
            linesTemplates.Add(OrderDetailsLineUPCText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Remove(OrderDetailsHeader);
            linesTemplates.Add(OrderDetailsHeader, "^FO40,{0}^AEN,18,10^FDPRODUCT^FS^FO310,{0}^AEN,18,10^FDQTY^FS^FO480,{0}^AEN,18,10^FDPRICE^FS^FO630,{0}^AEN,18,10^FDTOTAL^FS");
            linesTemplates.Remove(OrderDetailsSectionFooter);
            linesTemplates.Add(OrderDetailsSectionFooter, "^FO40,{0}^AEN,18,10^FD{1}^FS^FO360,{0}^AEN,18,10^FD{2}^FS^FO520,{0}^AEN,18,10^FD{3}^FS^FO620,{0}^AEN,18,10^FD{4}^FS");
        }

        protected override IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 12, 18);
        }
        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

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
                    totalQtyNoUoM += detail.Qty;
                    try
                    {
                        totalUnits += detail.Qty * Convert.ToInt32(detail.OrderDetail.Product.Package);
                    }
                    catch { }
                }

                int productLineOffset = 0;
                var productSlices = GetDetailsRowsSplitProductName1(p.Name);
                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        double d = 0;
                        foreach (var od in detail.ParticipatingDetails)
                        {
                            double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                            d += double.Parse(Math.Round(Convert.ToDecimal(od.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }
                        // anderson crap

                        double price = detail.Price * factor;

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();
                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;

                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, pName, qtyAsString, totalAsString, priceAsString));
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

                if (!string.IsNullOrEmpty(detail.OrderDetail.Lot))
                    if (preOrder)
                    {
                        if (Config.PrintLotPreOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
                        }
                    }
                    else
                    {
                        if (Config.PrintLotOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
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
                        string retPriceString = "Retail price                                   " + Convert.ToDouble(retailPrice.Item2).ToCustomString();
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                {

                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                        startIndex += font18Separation;
                    }
                    else
                    {
                        startIndex += font18Separation / 2;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPC], startIndex, p.Upc));
                        startIndex += font36Separation * 2;
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
                // extra space
                startIndex += font18Separation;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

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
                uomMap.Add("Totals:", totalQtyNoUoM);

            var uomKeys = uomMap.Keys.ToList();
            if (!Config.HideTotalOrder && t == null)
            {
                var key = uomKeys[0];
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, uomMap[key], balance.ToCustomString()));
                startIndex += font18Separation;
                uomKeys.Remove(key);
            }
            if (uomKeys.Count > 0)
            {
                foreach (var key in uomKeys)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty));
                    startIndex += font18Separation;
                }
            }
            return list;
        }
    }
}
