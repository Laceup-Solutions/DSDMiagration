using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class DalcomePrinter: ZebraFourInchesPrinter1
    {
        protected const string DalcomeTableCatName = "DalcomeTableCatName";
        protected const string DalcomeTableCatSubtotal = "DalcomeTableCatSubtotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(DalcomeTableCatName, "^CF0,30^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(DalcomeTableCatSubtotal, "^CF0,22^FO80,{0}^FDCATEGORY SUBTOTAL:^FS" +
                "^FO450,{0}^FD{1}^FS" +
                "^FO680,{0}^FD{2}^FS");
        }


        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            var s = string.Empty;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            Dictionary<int, List<OrderLine>> linesByCat = new Dictionary<int, List<OrderLine>>();

            foreach (var item in lines)
            {
                if (!linesByCat.Keys.Contains(item.Product.CategoryId))
                    linesByCat.Add(item.Product.CategoryId, new List<OrderLine>());

                linesByCat[item.Product.CategoryId].Add(item);
            }

            foreach (var itemCat in linesByCat)
            {
                float sectionTotalQty = 0;
                double sectionBalance = 0;

                var category = Category.Categories.FirstOrDefault(x => x.CategoryId == itemCat.Key);
                string catName = "Category Not Found";
                if (category != null)
                    catName = category.Name;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DalcomeTableCatName], startIndex, catName));
                startIndex += 40;

                foreach (var detail in itemCat.Value)
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
                            sectionBalance += d;

                            sectionTotalQty += detail.Qty;

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

                s = new string('_', WidthForNormalFont - s.Length) + s;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DalcomeTableCatSubtotal], startIndex, sectionTotalQty, sectionBalance.ToCustomString()));
                startIndex += font18Separation;

                startIndex += 30;
            }
            
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }


    }
}