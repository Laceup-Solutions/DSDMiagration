using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class RetailByCategoryPrinter : ZebraFourInchesPrinter1
    {
        protected const string RetailByCatTablTableHeader = "RetailByCatTablTableHeader";
        protected const string RetailByCatTablTableHeader2 = "RetailByCatTablTableHeader2";
        protected const string RetailByCatTableCatName = "RetailByCatTableCatName";
        protected const string RetailByCatTablTableLine = "RetailByCatTablTableLine";
        protected const string RetailByCatTableCatSubtotal = "RetailByCatTableCatSubtotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(RetailByCatTablTableHeader, "^FO40,{0}^ADN,18,10^FD^FS" +
                "^FO310,{0}^ADN,18,10^FD^FS" +
                "^FO390,{0}^ADN,18,10^FDRETAIL^FS" +
                "^FO490,{0}^ADN,18,10^FDEXT RET^FS" +
                "^FO595,{0}^ADN,18,10^FD^FS" +
                "^FO695,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(RetailByCatTablTableHeader2, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO310,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO490,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO595,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO695,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(RetailByCatTableCatName, "^CF0,30^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RetailByCatTablTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO310,{0}^ADN,18,10^FD{2}^FS" +
                "^FO390,{0}^ADN,18,10^FD{3}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS" +
                "^FO595,{0}^ADN,18,10^FD{5}^FS" +
                "^FO695,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(RetailByCatTableCatSubtotal, "^CF0,22^FO80,{0}^FDCATEGORY SUBTOTAL:^FS" +
                "^FO310,{0}^FD{1}^FS" +
                "^FO490,{0}^FD{2}^FS" +
                "^FO695,{0}^FD{3}^FS");

            linesTemplates[OrderDetailsTotals] = "^FO200,{0}^ADN,18,10^FD{2}^FS" +
                "^FO310,{0}^ADN,18,10^FD{3}^FS" +
                "^FO695,{0}^ADN,18,10^FD{4}^FS";
        }


        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailByCatTablTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailByCatTablTableHeader2], startY));
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
            double balanceRP = 0;

            var s = string.Empty;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            Dictionary<int, List<OrderLine>> linesByCat = new Dictionary<int, List<OrderLine>>();

            foreach (var item in lines)
            {
                if (!linesByCat.Keys.Contains(item.Product.CategoryId))
                    linesByCat.Add(item.Product.CategoryId, new List<OrderLine>());

                linesByCat[item.Product.CategoryId].Add(item);
            }

            foreach (var item in linesByCat)
            {
                float sectionTotalQty = 0;
                double sectionBalance = 0;
                double sectionBalanceRP = 0;

                var category = Category.Categories.FirstOrDefault(x => x.CategoryId == item.Key);
                string catName = "Category Not Found";
                if (category != null)
                    catName = category.Name;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailByCatTableCatName], startIndex, catName));
                startIndex += 40;

                foreach (var detail in item.Value)
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

                    sectionTotalQty += detail.Qty;

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

                            sectionBalance += d;
                            sectionBalanceRP += extRetailPrice;

                            string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                            if (detail.OrderDetail.UnitOfMeasure != null)
                                qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                            string priceAsString = price.ToCustomString();
                            string totalAsString = d.ToCustomString();

                            if (Config.HideTotalInPrintedLine)
                                priceAsString = string.Empty;

                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailByCatTablTableLine], startIndex,
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

                s = new string('_', WidthForNormalFont - s.Length) + s;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailByCatTableCatSubtotal], startIndex, sectionTotalQty, sectionBalanceRP.ToCustomString(), sectionBalance.ToCustomString()));
                startIndex += font18Separation;

                startIndex += 30;
            }

            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }

        private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

    }
}