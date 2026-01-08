using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class MappleViewPrinter : ZebraFourInchesPrinter1
    {
        protected const string MappleViewPrinterTableHeader = "MappleViewPrinterTableHeader";
        protected const string MappleViewPrinterTableHeader2 = "MappleViewPrinterTableHeader2";
        protected const string MappleViewPrinterTableLine = "MappleViewPrinterTableLine";
        protected const string MappleViewPrinterTableTotal = "MappleViewPrinterTableTotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(MappleViewPrinterTableHeader, "^FO40,{0}^ADN,18,10^FD^FS" +
                "^FO310,{0}^ADN,18,10^FD^FS" +
                "^FO390,{0}^ADN,18,10^FDRETAIL^FS" +
                "^FO490,{0}^ADN,18,10^FDEXT RET^FS" +
                "^FO595,{0}^ADN,18,10^FD^FS" +
                "^FO695,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(MappleViewPrinterTableHeader2, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO310,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO490,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO595,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO695,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(MappleViewPrinterTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO310,{0}^ADN,18,10^FD{2}^FS" +
                "^FO390,{0}^ADN,18,10^FD{3}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS" +
                "^FO595,{0}^ADN,18,10^FD{5}^FS" +
                "^FO695,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(MappleViewPrinterTableTotal, "^FO200,{0}^ADN,18,10^FD{2}^FS" +
                "^FO310,{0}^ADN,18,10^FD{3}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS" +
                "^FO695,{0}^ADN,18,10^FD{5}^FS");
        }

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[MappleViewPrinterTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[MappleViewPrinterTableHeader2], startY));
            startY += font18Separation;

            return lines;
        }

        class T
        {
            public float Qty { get; set; }

            public double Amount { get; set; }
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            T t = new T();

            var rItems = lines.Where(x => x.OrderDetail.RelatedOrderDetail > 0).Select(x => x.OrderDetail.RelatedOrderDetail).ToList();

            foreach (var item_ in lines.Where(x => rItems.Contains(x.OrderDetail.OrderDetailId)))
            {
                var item = item_.OrderDetail;

                var qty = item.Qty;
                if (item.Product.SoldByWeight)
                    qty = item.Weight;

                t.Amount += qty * item.Price * factor;

                if (item.UnitOfMeasure != null)
                    qty *= item.UnitOfMeasure.Conversion;

                t.Qty += qty;
            }

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            double balanceRetailPrice = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                if (rItems.Contains(detail.OrderDetail.OrderDetailId))
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

                var retPrice = GetRetailPrice(p, order.Client);

                var extRetailPrice = retPrice;
                extRetailPrice *= detail.Qty;
                if (detail.UoM != null)
                    extRetailPrice *= detail.UoM.Conversion;

                balanceRetailPrice += extRetailPrice;

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

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[MappleViewPrinterTableLine], startIndex,
                            pName, qtyAsString, retPrice.ToCustomString(), extRetailPrice.ToCustomString(), priceAsString, totalAsString));
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

            if (t.Qty > 0)
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[MappleViewPrinterTableLine], startIndex,
                            "DEPOSIT", t.Qty.ToString(), "", ToString(t.Amount), "", ToString(t.Amount)));
                startIndex += font18Separation;
                startIndex += 10;

                totalQtyNoUoM += t.Qty;
                totalUnits += t.Qty;

                balance += t.Amount;
                balanceRetailPrice += t.Amount;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal_(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance, balanceRetailPrice));

            return list;
        }

        protected List<string> GetOrderDetailsSectionTotal_(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance, double balanceRetailPrice)
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

                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[MappleViewPrinterTableTotal], startIndex,
                        string.Empty,
                        key,
                        Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture),
                        balanceRetailPrice,
                        balanceText));
                    startIndex += font18Separation;
                    offset++;
                }
            }

            return list;
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }
        double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }
        /*private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            return p.RetailPrice;
        }*/

    }
}