using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class Services77Printer : ZebraFourInchesPrinter1
    {
        const string OrderDetailsHeader1 = "OrderDetailsHeader2";
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
          "^FO290,{0}^ADN,18,10^FDQTY^FS" +
          "^FO360,{0}^ADN,18,10^FDSIZE^FS" + 
          "^FO450,{0}^ADN,18,10^FDPRICE^FS" +
          "^FO580,{0}^ADN,18,10^FDPRICE^FS" +  
          "^FO680,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[OrderDetailsHeader1] = "^FO40,{0}^ADN,18,10^^FS" +
          "^FO290,{0}^ADN,18,10^FD^FS" +
          "^FO360,{0}^ADN,18,10^FDPACK^FS" + 
          "^FO452,{0}^ADN,18,10^FDUNIT^FS" +
          "^FO582,{0}^ADN,18,10^FDCASE^FS" + 
          "^FO680,{0}^ADN,18,10^FD^FS";

            linesTemplates[OrderDetailsLines] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
          "^FO290,{0}^ADN,18,10^FD{2}^FS" +
          "^FO360,{0}^ADN,18,10^FD{3}^FS" +
          "^FO450,{0}^ADN,18,10^FD{4}^FS" +
          "^FO580,{0}^ADN,18,10^FD{5}^FS" +
          "^FO680,{0}^ADN,18,10^FD{6}^FS";

            linesTemplates[OrderDetailsTotals] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD{1}^FS";
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeader];

            string formatString1 = linesTemplates[OrderDetailsHeader1];

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString1, startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
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
            float totalUnitsPackage = 0;
            float totalUnitsCount = 0;

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

                        var packaging_str = detail.Product.Package;
                        int package = 0;
                        Int32.TryParse(packaging_str, out package);

                        totalUnitsPackage += package;

                        totalUnitsCount = totalUnitsPackage * totalQtyNoUoM;

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

                            totalUnitsCount = totalUnits;

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

                        var packaging_str = detail.Product.Package;
                        int package = 1;
                        Int32.TryParse(packaging_str, out package);

                        totalUnitsPackage += package;
                        var units = package; //detail.Qty * package

                        var unitPrice = detail.Product.PriceLevel0 / package;  //pricelevel0 = precio original || detail.price = price level , before U.Price : var unitPrice = detail.Price / package; 

                        /*double discount;
                        if (detail.OrderDetail.Damaged)
                        {
                             discount = detail.Product.PriceLevel0 / package - detail.Price;
                        }
                        else
                        {
                            discount = (detail.Product.PriceLevel0 - detail.Price) * units;
                        }
                        if (discount < 0)
                            discount = 0;*/

                        var casePrice = detail.Product.PriceLevel0;
                        double discount = 0;
                        discount += CalculateDiscount(detail.OrderDetail);

                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, qtyAsString, units.ToString(), unitPrice.ToCustomString(), casePrice.ToCustomString(), totalAsString));
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

                if (!Config.HidePriceInPrintedLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach(string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                        startIndex += 20;
                    }
                }
                startIndex += 10;

                list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                /*if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;*/
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, totalUnitsPackage, sectionName, totalUnits, balance, totalUnitsCount));

            return list;
        }

        private double CalculateDiscount(OrderDetail detail)
        {
            double discount;

            if (detail.Damaged)
            {
                int package;
                if (int.TryParse(detail.Product.Package, out package))
                {
                    // if is dump
                    discount = (detail.Product.PriceLevel0 / package) - detail.Price;
                }
                else
                {
                    // Handle invalid package value
                    discount = 0;
                }

            }
            if (detail.IsCredit)
            {
                discount = 0;
            }
            else
            {
                // calculate discount as the difference between the original price and the current price, multiplied by the number of units
                int package = 1;
                int.TryParse(detail.Product.Package, out package);
                double units = detail.Qty * package;
                double qty = detail.Qty;
                discount = (detail.Product.PriceLevel0 - detail.Price) * qty;
            }
            if (discount < 0)
            {
                discount = 0;
            }
            return discount;
        }

        protected string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v4, string v3, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v4, v3, v5, v6);
        }

        protected virtual List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, float totalUnitsPackage, string sectionName, float totalUnits, double balance , float totalUnitsCount)
        {
            List<string> list = new List<string>();

            var printString = string.Empty;
            var printString1 = string.Empty;
            startIndex += font18Separation;

            if (sectionName == "DUMP SECTION")
            {
                //printString  =  "Credit Units: " + totalUnitsPackage ; //the client wants the total of Units not QTY
                printString = "Credit Units: " + totalUnitsCount;
                printString1 = "Total Cases: " + totalQtyNoUoM;
            }
                
            if (sectionName == "SALES SECTION")
            {
                //printString  =  "Total Units: " + totalUnitsPackage ; //printString = "Total Units: " + totalQtyNoUoM + " EA";
                printString = "Total Units: " + totalUnitsCount;
                printString1 = "Total Cases: " + totalQtyNoUoM;
            }
                
            if (sectionName == "RETURNS SECTION")
            {
                //printString  =  "Return Units: " + totalUnitsPackage ;
                printString = "Return Units: " + totalUnitsCount;
                printString1 = "Total Cases: " + totalQtyNoUoM;
            }
                

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotals], startIndex, printString));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotals], startIndex, printString1));
            startIndex += font18Separation;

            return list;
        }
    }
}

