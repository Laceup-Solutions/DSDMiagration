using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class TandJPrinter : ZebraFourInchesPrinter1
    {
        protected const string TandJTablTableHeader = "TandJTablTableHeader";
        protected const string TandJTablTableHeader2 = "TandJTablTableHeader2";
        protected const string TandJTablTableLine = "TandJTablTableLine";
        protected const string TandJTableSubtotal = "TandJTableSubtotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(TandJTablTableHeader, "^FO40,{0}^ADN,18,10^FD^FS" +
                "^FO310,{0}^ADN,18,10^FD^FS" +
                "^FO390,{0}^ADN,18,10^FDWGHT/^FS" +
                "^FO470,{0}^ADN,18,10^FDWGHT/^FS" +
                "^FO595,{0}^ADN,18,10^FD^FS" +
                "^FO695,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(TandJTablTableHeader2, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO310,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDVOL^FS" +
                "^FO470,{0}^ADN,18,10^FDVOL TOTAL^FS" +
                "^FO595,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO695,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(TandJTablTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO310,{0}^ADN,18,10^FD{2}^FS" +
                "^FO390,{0}^ADN,18,10^FD{3} ml^FS" +
                "^FO470,{0}^ADN,18,10^FD{4} ml^FS" +
                "^FO595,{0}^ADN,18,10^FD{5}^FS" +
                "^FO695,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(TandJTableSubtotal, "^CF0,22^FO80,{0}^FD            TOTAL:^FS" +
                "^FO310,{0}^FD{1}^FS" +
                "^FO470,{0}^FD{2} ml^FS" +
                "^FO695,{0}^FD{3}^FS");
        }


        #region upc

        public override IEnumerable<string> GetUpcForProductInOrder(ref int startY, Order order, Product prod)
        {
            List<string> list = new List<string>();

            var upc = prod.Upc.Trim();

            if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(prod.Sku.Trim()))
                upc = prod.Sku.Trim();

            if (upc.Length > 0 & Config.PrintUPC)
            {
                bool printUpc = true;
                if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString))
                {
                    var item = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                    if (item != null && item.Item2 == "0")
                        printUpc = false;
                }
                if (printUpc)
                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, upc));
                        startY += font18Separation;
                    }
                    else
                    {
                        startY += font18Separation / 2;

                        var upcTemp = Config.UseUpc128 ? linesTemplates[Upc128] : linesTemplates[OrderDetailsLinesUpcBarcode];

                        string upcString = string.Empty;

                        var parts = prod.Upc.Split(",");
                        if (parts.Length > 1)
                            upcString = parts[1];
                        else
                            upcString = parts[0];

                        if (upcString.Length > 12 && !Config.UseUpc128)
                            upcTemp = linesTemplates[OrderDetailsLinesLongUpcBarcode];

                        list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, upcString));
                        startY += font36Separation * 2;
                    }
            }

            return list;
        }

        public override IEnumerable<string> GetUpcForProductIn(ref int startY, Product prod)
        {
            List<string> list = new List<string>();

            if (Config.PrintUPCInventory || Config.PrintUPCOpenInvoices)
            {
                if (prod.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, prod.Upc));
                        startY += font18Separation;
                    }
                    else
                    {
                        var upcTemp = Config.UseUpc128 ? linesTemplates[Upc128] : linesTemplates[OrderDetailsLinesUpcBarcode];

                        string upc = string.Empty;

                        var parts = prod.Upc.Split(",");
                        if (parts.Length > 1)
                            upc = parts[1];
                        else
                            upc = parts[0];

                        if (upc.Length > 12 && !Config.UseUpc128)
                            upcTemp = linesTemplates[OrderDetailsLinesLongUpcBarcode];

                        list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, upc));
                        startY += font36Separation;
                        if (Config.PrintUPCOpenInvoices)
                            startY += font18Separation;
                    }
                }
            }

            return list;
        }

        #endregion

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TandJTablTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TandJTablTableHeader2], startY));
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
            double totalWeight = 0;

            var s = string.Empty;

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

                var weight = p.Weight;
                var totalWeightLine = weight * detail.Qty;

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
                        totalWeight += totalWeightLine;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TandJTablTableLine], startIndex,
                            pName, qtyAsString, weight.ToString(), totalWeightLine.ToString(), priceAsString, totalAsString));
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

            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TandJTableSubtotal], startIndex, totalQtyNoUoM, totalWeight.ToString(), balance.ToCustomString()));
            startIndex += font18Separation;

            startIndex += 30;
            
            return list;
        }
        
        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

    }
}