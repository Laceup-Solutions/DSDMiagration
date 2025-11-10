using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace LaceupMigration
{
    public class MegaWattsPrinter : ZebraFourInchesPrinter
    {
        const string RetailPriceTableHeader = "RetailPriceTableHeader";
        const string RetailPriceTableHeader0 = "RetailPriceTableHeader0";
        const string RetailPriceTableLine = "RetailPriceTableLine";
        const string RetailPriceTotalsLine = "RetailPriceTotalsLine";
        const string MegaWattsCompanyName = "MegaWattsCompanyName";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(MegaWattsCompanyName, "^CF0,40^FO15,{0}^FD{1}^FS");

            linesTemplates.Add(RetailPriceTableHeader0, "^FO40,{0}^ADN,18,10^FD^FS" +
                "^FO360,{0}^ADN,18,10^FD^FS" +
                "^FO457,{0}^ADN,18,10^FDUNIT^FS" +
                "^FO570,{0}^ADN,18,10^FD^FS" +
                "^FO695,{0}^ADN,18,10^FD^FS");

            linesTemplates.Add(RetailPriceTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO360,{0}^ADN,18,10^FDQTY^FS" +
                "^FO455,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO570,{0}^ADN,18,10^FDAMOUNT^FS" +
                "^FO695,{0}^ADN,18,10^FDMSRP^FS");

            linesTemplates.Add(RetailPriceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO360,{0}^ADN,18,10^FD{2}^FS" +
                "^FO455,{0}^ADN,18,10^FD{3}^FS" +
                "^FO570,{0}^ADN,18,10^FD{4}^FS" +
                "^FO695,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(RetailPriceTotalsLine, "^FO270,{0}^ADN,18,10^FD{1}^FS" +
                "^FO365,{0}^ADN,18,10^FD{2}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS" +
                "^FO695,{0}^ADN,18,10^FD{3}^FS");
        }

  
        
        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableHeader0], startY));
            startY += font18Separation;
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

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], "      " + sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            double balanceRP = 0;
            double unitPrice = 0;
            double amount = 0;

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
                var productSlices = GetDetailsRowsSplitProductName1(name);

                var retPrice = Product.GetRetailPrice(p, order.Client);
                //var retPrice = detail.OrderDetail.Product.PriceLevel0;

                var extRetailPrice = retPrice;
                extRetailPrice *= detail.Qty;
                if (detail.UoM != null)
                    extRetailPrice *= detail.UoM.Conversion;

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

                        unitPrice = detail.OrderDetail.Price / Convert.ToInt32(detail.OrderDetail.Product.Package);
                        amount = Math.Round(detail.Qty * unitPrice , 2);

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null && !Config.HideSubTotalOrder)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            pName, qtyAsString, unitPrice.ToCustomString(), amount.ToCustomString(), retPrice.ToCustomString()));
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

                if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
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
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            startIndex += font18Separation / 2;

                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(p.Upc)));
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
            {
                uomMap.Add("Totals:", totalQtyNoUoM);
                if (totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION")
                    uomMap.Add("Units:", totalUnits);
            }

            if (!Config.HideSubTotalOrder)
            {
                var uomKeys = uomMap.Keys.ToList();
                if (!Config.HideTotalOrder && t == null)
                {
                    var key = uomKeys[0];
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTotalsLine], startIndex,
                        key, uomMap[key], string.Empty, string.Empty));
                    startIndex += font18Separation;
                    uomKeys.Remove(key);
                }
                if (uomKeys.Count > 0)
                {
                    foreach (var key in uomKeys)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTotalsLine], startIndex,
                            key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                }
            }

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
            return SplitProductName(name, 20, 20);
        }
    }
}

