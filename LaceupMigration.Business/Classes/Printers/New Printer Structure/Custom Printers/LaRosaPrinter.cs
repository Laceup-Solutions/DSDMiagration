using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class LaRosaPrinter : ZebraFourInchesPrinter1
    {
        protected const string LaRosaOrderDetailsTotals = "LaRosaOrderDetailsTotals";
        protected const string LaRosaOuterTableLine = "LaRosaOuterTableLine";
        protected const string LaRosaProductDetailsLine = "LaRosaProductDetailsLine";
        protected const string LaRosaProductDetailsColumn1 = "LaRosaProductDetailsColumn1";
        protected const string LaRosaProductDetailsColumn2 = "LaRosaProductDetailsColumn2";
        protected const string LaRosaProductDetailsColumn3 = "LaRosaProductDetailsColumn3";
        protected const string LaRosaProductDetailsColumn4 = "LaRosaProductDetailsColumn4";
        protected const string LaRosaProductDetailsColumn5 = "LaRosaProductDetailsColumn5";
        protected const string LaRosaProductDetailsColumn6 = "LaRosaProductDetailsColumn6";
        protected const string LaRosaProductDetailsColumn7 = "LaRosaProductDetailsColumn7";

        protected const string LaRosaTotalSection = "LaRosaTotalSection";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            //outter table
            linesTemplates.Add(LaRosaOrderDetailsTotals, "^FO500,{0}^ADN,20,10^FD{1}^FS" +
                "^FO660,{0}^ADN,36,20^FD{2}^FS");

            //columns
            linesTemplates.Add(LaRosaOuterTableLine, "^FO15,{0}^GB795,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn1, "^FO15,{0}^GB68,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn2, "^FO15,{0}^GB145,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn3, "^FO15,{0}^GB290,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn4, "^FO15,{0}^GB465,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn5, "^FO15,{0}^GB570,{1},1^FS");
            linesTemplates.Add(LaRosaProductDetailsColumn6, "^FO15,{0}^GB675,{1},1^FS");

            linesTemplates.Add(LaRosaTotalSection, "^FO500,{0}^ADN,28,16^FD{1}^FS");

            //rows 
            linesTemplates.Add(LaRosaProductDetailsLine, "^FO15,{0}^GB795,{1},1^FS");

            linesTemplates[OrderDetailsHeader] =
                "^FO30,{0}^ADN,18,10^FDQty^FS" +
                "^FO85,{0}^ADN,18,10^FDItem^FS" +
                "^FO165,{0}^ADN,18,10^FDUPC^FS" +
                "^FO310,{0}^ADN,18,10^FDDescription^FS" +
                "^FO485,{0}^ADN,18,10^FDCost^FS" +
                "^FO590,{0}^ADN,18,10^FDPromo^FS" +
                "^FO695,{0}^ADN,18,10^FDTotal^FS";
            linesTemplates[OrderDetailsLines] =
                "^FO30,{0}^ADN,18,10^FD{1}^FS" +
                "^FO85,{0}^ADN,18,10^FD{2}^FS" +
                "^FO165,{0}^ADN,18,10^FD{3}^FS" +
                "^FO310,{0}^ADN,18,10^FD{4}^FS" +
                "^FO485,{0}^ADN,18,10^FD{5}^FS" +
                "^FO590,{0}^ADN,18,10^FD{6}^FS" +
                "^FO695,{0}^ADN,18,10^FD{7}^FS";

            linesTemplates[OrderDetailsHeaderSectionName] = "^FO320,{0}^ADN,18,10^FD{1}^FS";
        }

        bool alreadyInsertedSection = false;
        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();


            int beforeDetIndex = startIndex;
            if (!alreadyInsertedSection)
            {
                startIndex += 10;
                beforeDetIndex = startIndex - 50;
            }
            else
            {
                startIndex += 10;
                beforeDetIndex = startIndex;
            }

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            int LinesToPrint = 0;

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

                if (preOrder && Config.PrintZeroesOnPickSheet)
                    factor = 0;

                double d = 0;
                foreach (var _ in detail.ParticipatingDetails)
                {
                    double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

                    d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                }
                
                double listPrice = detail.Product.PriceLevel0;
                double price = detail.Price;
                
                bool cameFromOffer = false;
                var valPrice = Product.GetPriceForProduct(detail.Product, order, out cameFromOffer, detail.IsCredit, detail.Damaged, detail.UoM);
                if (detail.OrderDetail.IsFreeItem)
                {
                    listPrice = detail.ListPrice;
                    if(listPrice == 0)
                        listPrice = valPrice;
                }

                double promo = listPrice - price;

                if (listPrice > price)
                    promo *= -1;
                else
                if (price > listPrice)
                {
                    promo = 0;
                    listPrice = price;
                }
                else
                if (listPrice == price)
                    promo = 0;

           
                
                balance += d;

                string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                if (detail.OrderDetail.UnitOfMeasure != null)
                    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                string priceAsString = ToString(promo);

                string listPriceAsString = ToString(listPrice);

                string totalAsString = ToString(d);

                if (detail.Product.ProductType == ProductType.Discount)
                    qtyAsString = string.Empty;

                var prodCode = detail.Product.Code;
                var prodUpc = detail.Product.Upc;
                var prodDescription = detail.Product.Name;

                //fix name
                prodDescription = prodDescription.Replace("-", "");
                prodDescription = prodDescription.Replace(prodCode, "");
                prodDescription = prodDescription.TrimStart();
                prodDescription = prodDescription.TrimEnd();

                list.Add(GetLaRosaTableFormatted(OrderDetailsLines,
                  startIndex, qtyAsString, prodCode, prodUpc, prodDescription, listPriceAsString, priceAsString, totalAsString));
                startIndex += 40;

                LinesToPrint++;

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
            }

            var endIndex = startIndex - beforeDetIndex;

            if (alreadyInsertedSection)
            {
                beforeDetIndex -= 10;
            }

            list = DrawSalesTable(beforeDetIndex, endIndex, list, LinesToPrint);

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            alreadyInsertedSection = true;

            return list;
        }

        public override string ToString(double d)
        {
            if (d < 0)
            {
                d *= -1;
                return "-" + d.ToCustomString();
            }

            return d.ToCustomString();
        }

        private List<string> DrawSalesTable(int startIndex, int endIndex, List<string> list, int detCount)
        {
            //add outter table
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaOuterTableLine], startIndex.ToString(), endIndex.ToString()));

            //columns
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn1], startIndex.ToString(), endIndex.ToString()));
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn2], startIndex.ToString(), endIndex.ToString()));
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn3], startIndex.ToString(), endIndex.ToString()));
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn4], startIndex.ToString(), endIndex.ToString()));
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn5], startIndex.ToString(), endIndex.ToString()));
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsColumn6], startIndex.ToString(), endIndex.ToString()));

            int start = startIndex;
            //rows

            if (!alreadyInsertedSection)
            {
                for (int x = 0; x < detCount; x++)
                {
                    start += 40;
                    var end = start - startIndex;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsLine], startIndex.ToString(), end.ToString()));
                }
            }
            else
            {
                for (int x = 1; x < detCount; x++)
                {
                    start += 40;
                    var end = start - startIndex;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaProductDetailsLine], startIndex.ToString(), end.ToString()));
                }
            }

            return list;
        }

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            string totalLabel = string.Empty;

            if (sectionName.ToLowerInvariant().Contains("sales"))
                totalLabel = "Total In: ";
            else
                totalLabel = "Total Out: ";

            totalLabel += totalUnits.ToString();

            startIndex += font20Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LaRosaTotalSection], startIndex, totalLabel));
            startIndex += font20Separation;

            return list;
        }


        protected virtual string GetLaRosaTableFormatted(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {
            v1 = v1.Substring(0, v1.Length > 4 ? 4 : v1.Length);
            v2 = v2.Substring(0, v2.Length > 6 ? 6 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 11 ? 11 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 14 ? 14 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 7 ? 7 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 7 ? 7 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 8 ? 8 : v7.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);
        }

        bool alreadyAddedHeader = false;
        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            int factor = 1;

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
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startY, GetOrderDetailSectionHeader(-1)));
                startY += font36Separation;

                if (!alreadyAddedHeader)
                {
                    list.AddRange(GetDetailTableHeader(ref startY));
                    alreadyAddedHeader = true;
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

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startY, GetOrderDetailSectionHeader(0)));
                startY += font36Separation;

                if (!alreadyAddedHeader)
                {
                    list.AddRange(GetDetailTableHeader(ref startY));
                    alreadyAddedHeader = true;
                }

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

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startY, GetOrderDetailSectionHeader(1)));
                startY += font36Separation;

                if (!alreadyAddedHeader)
                {
                    list.AddRange(GetDetailTableHeader(ref startY));
                    alreadyAddedHeader = true;
                }

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
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
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
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

            if (!Config.HideTotalOrder && t == null)
            {

                /* s1 = (totalSales - totalCredit - totalReturn).ToString();
                 s1 = new string(' ', 14 - s1.Length) + s1;
                 list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                 startY += font36Separation;
                */
                double discount = order.CalculateDiscount();

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;


                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;
            }


            return list;
        }

    }

    #region Depracated La Rosa Printer
    //public class LaRosaPrinter : ZebraFourInchesPrinter1
    //{
    //    protected override void FillDictionary()
    //    {
    //        base.FillDictionary();

    //        linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
    //            "^FO390,{0}^ADN,18,10^FDQTY^FS" +
    //            "^FO490,{0}^ADN,18,10^FDPRICE^FS" +
    //            "^FO590,{0}^ADN,18,10^FDPROMO^FS" +
    //            "^FO680,{0}^ADN,18,10^FDTOTAL^FS";

    //        linesTemplates[OrderDetailsLines] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
    //            "^FO390,{0}^ADN,18,10^FD{2}^FS" +
    //            "^FO490,{0}^ADN,18,10^FD{3}^FS" +
    //            "^FO590,{0}^ADN,18,10^FD{4}^FS" +
    //            "^FO680,{0}^ADN,18,10^FD{5}^FS";

    //        linesTemplates[OrderDetailsTotals] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
    //            "^FO260,{0}^ADN,18,10^FD{2}^FS" +
    //            "^FO390,{0}^ADN,18,10^FD{3}^FS" +
    //            "^FO680,{0}^ADN,18,10^FD{4}^FS";
    //    }

    //    protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
    //    {
    //        List<string> list = new List<string>();

    //        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
    //        startIndex += font18Separation;

    //        float totalQtyNoUoM = 0;
    //        float totalUnits = 0;
    //        double balance = 0;

    //        Dictionary<string, float> uomMap = new Dictionary<string, float>();

    //        foreach (var detail in lines)
    //        {
    //            if (detail.Qty == 0)
    //                continue;

    //            Product p = detail.Product;

    //            string uomString = null;
    //            if (detail.Product.ProductType != ProductType.Discount)
    //            {
    //                if (detail.OrderDetail.UnitOfMeasure != null)
    //                {
    //                    uomString = detail.OrderDetail.UnitOfMeasure.Name;
    //                    if (!uomMap.ContainsKey(uomString))
    //                        uomMap.Add(uomString, 0);
    //                    uomMap[uomString] += detail.Qty;

    //                    totalQtyNoUoM += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;
    //                }
    //                else
    //                {
    //                    if (!detail.OrderDetail.SkipDetailQty(order))
    //                    {
    //                        int packaging = 0;

    //                        if (!string.IsNullOrEmpty(detail.OrderDetail.Product.Package))
    //                            int.TryParse(detail.OrderDetail.Product.Package, out packaging);

    //                        totalQtyNoUoM += detail.Qty;

    //                        if (packaging > 0)
    //                            totalUnits += detail.Qty * packaging;
    //                    }
    //                }
    //            }

    //            int productLineOffset = 0;
    //            var name = p.Name;
    //            if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
    //                name = p.Name + " " + p.Upc;
    //            var productSlices = GetOrderDetailsRowsSplitProductName(name);

    //            foreach (string pName in productSlices)
    //            {
    //                if (productLineOffset == 0)
    //                {
    //                    if (preOrder && Config.PrintZeroesOnPickSheet)
    //                        factor = 0;

    //                    double d = 0;
    //                    foreach (var _ in detail.ParticipatingDetails)
    //                    {
    //                        double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

    //                        d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
    //                    }

    //                    double listPrice = detail.Product.PriceLevel0 * factor;

    //                    double price = detail.Price * factor;

    //                    double promo = listPrice - price;

    //                    balance += d;

    //                    string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
    //                    if (detail.OrderDetail.UnitOfMeasure != null)
    //                        qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

    //                    string priceAsString = promo > 0 ? ToString(promo) : "0";
    //                    string listPriceAsString = ToString(price);

    //                    string totalAsString = ToString(d);

    //                    if (Config.HidePriceInPrintedLine)
    //                    {
    //                        listPriceAsString = string.Empty;
    //                        priceAsString = string.Empty;
    //                    }
    //                    if (Config.HideTotalInPrintedLine)
    //                        totalAsString = string.Empty;

    //                    if (detail.Product.ProductType == ProductType.Discount)
    //                        qtyAsString = string.Empty;

    //                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines],
    //                        startIndex, pName, qtyAsString, listPriceAsString, priceAsString, totalAsString));
    //                    startIndex += font18Separation;
    //                }
    //                else if (!Config.PrintTruncateNames)
    //                {
    //                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, pName));
    //                    startIndex += font18Separation;
    //                }
    //                else
    //                    break;
    //                productLineOffset++;
    //            }

    //            foreach (var item in detail.ParticipatingDetails)
    //            {
    //                if (!string.IsNullOrEmpty(item.Lot))
    //                    if (preOrder)
    //                    {
    //                        if (Config.PrintLotPreOrder)
    //                        {
    //                            list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
    //                                item.Lot, item.Qty.ToString()));
    //                            startIndex += font18Separation;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (Config.PrintLotOrder)
    //                        {
    //                            list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
    //                                item.Lot, item.Qty.ToString()));
    //                            startIndex += font18Separation;
    //                        }
    //                    }
    //            }

    //            // anderson crap
    //            // the retail price
    //            var extraProperties = order.Client.ExtraProperties;
    //            if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
    //            {
    //                var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
    //                if (retailPrice != null)
    //                {
    //                    string retPriceString = "                                  " + ToString(Convert.ToDouble(retailPrice.Item2));
    //                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesRetailPrice], startIndex, retPriceString));
    //                    startIndex += font18Separation;
    //                }
    //            }

    //            list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

    //            if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
    //            {
    //                foreach (string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
    //                {
    //                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
    //                    startIndex += font18Separation;
    //                }
    //            }

    //            startIndex += 10;
    //        }

    //        var s = string.Empty;
    //        s = new string('-', WidthForNormalFont - s.Length) + s;
    //        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
    //        startIndex += font18Separation;

    //        list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

    //        return list;
    //    }

    //    protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
    //    {
    //        return SplitProductName(name, 28, 28);
    //    }
    //}
    #endregion

}