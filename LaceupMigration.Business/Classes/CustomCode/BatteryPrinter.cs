


using System;
using System.Collections.Generic;
using System.Threading;

using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BatteryPrinter : ZebraFourInchesPrinter
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(BatteryConsRotHeader, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO390,{0}^ADN,18,10^FD{2}^FS^FO445,{0}^ADN,18,10^FD{3}^FS" +
                "^FO513,{0}^ADN,18,10^FD{4}^FS^FO570,{0}^ADN,18,10^FD{5}^FS^FO670,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(BatteryConsTotal, "^FO390,{0}^ADN,18,10^FD{1}^FS^FO513,{0}^ADN,18,10^FD{2}^FS^FO670,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(BatteryConsReportHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO390,{0}^ADN,18,10^FDRotated^FS^FO480,{0}^ADN,18,10^FDAdjusted^FS^FO590,{0}^ADN,18,10^FDWarranty^FS^FO700,{0}^ADN,18,10^FDCore^FS");
            linesTemplates.Add(BatteryConsReportHeader2, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO480,{0}^ADN,18,10^FDRotated^FS^FO590,{0}^ADN,18,10^FDAdjusted^FS^FO700,{0}^ADN,18,10^FDCore^FS");

            linesTemplates.Add(BatteryConsReportLines, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO390,{0}^ADN,18,10^FD{2}^FS^FO480,{0}^ADN,18,10^FD{3}^FS^FO590,{0}^ADN,18,10^FD{4}^FS^FO700,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(BatteryConsReportLines2, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS^FO590,{0}^ADN,18,10^FD{3}^FS^FO700,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(BatInvSettDetailsHeader1, "^FO40,{0}^ABN,18,10^FDProduct^FS" +
            "^FO330,{0}^ABN,18,10^FDBeg^FS" +
            "^FO370,{0}^ABN,18,10^FDLoad^FS" +
            "^FO410,{0}^ABN,18,10^FDAdj^FS" +
            "^FO450,{0}^ABN,18,10^FDTr.^FS" +
            "^FO490,{0}^ABN,18,10^FDSls^FS" +
            "^FO530,{0}^ABN,18,10^FDRet^FS" +
            "^FO570,{0}^ABN,18,10^FDEnd^FS" +
            "^FO610,{0}^ABN,18,10^FDOver^FS" +
            "^FO650,{0}^ABN,18,10^FDCore^FS" +
            "^FO690,{0}^ABN,18,10^FDBat^FS" +
            "^FO730,{0}^ABN,18,10^FDRot^FS");

            linesTemplates.Add(BatInvSettDetailsHeader2, "^FO40,{0}^ABN,18,10^FD^FS" +
            "^FO330,{0}^ABN,18,10^FDInv^FS" +
            "^FO370,{0}^ABN,18,10^FD^FS" +
            "^FO410,{0}^ABN,18,10^FD^FS" +
            "^FO450,{0}^ABN,18,10^FD^FS" +
            "^FO490,{0}^ABN,18,10^FD^FS" +
            "^FO530,{0}^ABN,18,10^FD^FS" +
            "^FO570,{0}^ABN,18,10^FDInv^FS" +
            "^FO610,{0}^ABN,18,10^FDSh^FS" +
            "^FO650,{0}^ABN,18,10^FD^FS" +
            "^FO690,{0}^ABN,18,10^FDAdj^FS" +
            "^FO730,{0}^ABN,18,10^FD^FS");

            linesTemplates.Add(BatInvSettDetailRow, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO330,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO370,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                               "^FO450,{0}^ABN,18,10^FD{5}^FS" +
                               "^FO490,{0}^ABN,18,10^FD{6}^FS" +
                               "^FO530,{0}^ABN,18,10^FD{7}^FS" +
                               "^FO570,{0}^ABN,18,10^FD{8}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{9}^FS" +
                               "^FO650,{0}^ABN,18,10^FD{10}^FS" +
                               "^FO690,{0}^ABN,18,10^FD{11}^FS" +
                               "^FO730,{0}^ABN,18,10^FD{12}^FS");
        }

        public bool PrintBatteryConsignmentInvoice(Order order, bool asPreOrder)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            string title = "Invoice: " + order.PrintedOrderId;

            lines.AddRange(GetConsignmentHeaderInfoLines(ref startY, order, title));
            startY += font36Separation;

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, "NOT AN INVOICE"));
                    startY += font36Separation;
                }
            }
            else
            {
                bool credit = false;
                if (order != null)
                    credit = order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, credit ? "FINAL CREDIT INVOICE" : "FINAL INVOICE"));
                startY += font36Separation;
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle25], startY, order.PONumber));
                startY += font36Separation;
            }

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var paidInFull = payments != null && payments.Sum(x => x.Amount) == order.OrderTotalCost();
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            lines.AddRange(GetBatteryConsInvoiceDetailsRows(ref startY, order));
            startY += font36Separation;

            startY += font36Separation;
            string s;

            var totalItemCost = order.CalculateBatteryItemCost();

            s = "     SALES:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            var s1 = totalItemCost.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            double tax = order.CalculateBatteryTax();
            if (tax > 0)
            {
                s = " SALES TAX:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = tax.ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;
            }
            if (order.DiscountAmount > 0)
            {
                s = "DISCOUNT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = order.CalculateBatteryDiscount(totalItemCost).ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.DiscountComment))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, " C: " + order.DiscountComment));
                startY += font18Separation;
            }

            double totalCostOrder = order.BatteryConsTotalCost();

            // right justified
            s = "TOTAL DUE:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = totalCostOrder.ToCustomString();
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            double paid = 0;
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            if (payment != null)
            {
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            s = "TOTAL PAYMENT:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = paid.ToCustomString();
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            s = "CURRENT BALANCE:"; //due - payment
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = (totalCostOrder - paid).ToCustomString();
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));

                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

                startY += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startY, order.SignatureName ?? string.Empty));
                    startY += font18Separation;
                }
                startY += font18Separation;
                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomTextSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
                foreach (string row in GetConsignmentFooterRows(ref startY, asPreOrder))
                    lines.Add(row);


            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s2 in lines)
                sb.Append(s2);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        IList<string> GetBatteryConsInvoiceDetailsRows(ref int startIndex, Order order)
        {
            var lines = new List<string>();

            int starty = startIndex;

            var sales = GetBatteryConsSalesRows(ref starty, order);

            if (sales.Count > 3)
            {
                lines.AddRange(sales);
                startIndex = starty;
                startIndex += font18Separation;
            }
            starty = startIndex;

            var credits = GetBatteryConsSalesRows(ref starty, order, true);

            if (credits.Count > 3)
            {
                lines.AddRange(credits);
                startIndex = starty;
                startIndex += font18Separation;
            }
            starty = startIndex;

            var rotations = GetBatteryConsRotRows(ref starty, order);

            if (rotations.Count > 3)
            {
                lines.AddRange(rotations);
                startIndex = starty;
                startIndex += font18Separation;
            }

            if (!Config.CoreAsCredit)
            {
                starty = startIndex;
                var adjustments = GetBatteryConsAdjRows(ref starty, order);

                if (adjustments.Count > 3)
                {
                    lines.AddRange(adjustments);
                    startIndex = starty;
                    startIndex += font18Separation;
                }

                starty = startIndex;
                var cores = GetBatteryConsCoreRows(ref starty, order);

                if (cores.Count > 3)
                {
                    lines.AddRange(cores);
                    startIndex = starty;
                    startIndex += font18Separation;
                }
            }
            else
            {
                starty = startIndex;

                List<Tuple<Product, float>> otherCores = new List<Tuple<Product, float>>();

                var adjustments = GetBatteryConsAdjConfRows(ref starty, order, otherCores);

                if (adjustments.Count > 3)
                {
                    lines.AddRange(adjustments);
                    startIndex = starty;
                    startIndex += font18Separation;
                }

                starty = startIndex;
                var cores = GetBatteryConsCoreConfigRows(ref starty, order, otherCores);

                if (cores.Count > 3)
                {
                    lines.AddRange(cores);
                    startIndex = starty;
                    startIndex += font18Separation;
                }
            }


            return lines;
        }

        IList<string> GetBatteryConsSalesRows(ref int startIndex, Order order, bool iscredit = false)
        {
            var lines = new List<string>();

            string format = linesTemplates[ConsignmentDetailsHeader1];

            if(iscredit)
            {
                format = format.Replace("Cons", "");
                format = format.Replace("Count", "");
                format = format.Replace("Sold", "Cred");
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, format , startIndex));
            startIndex += font18Separation;

            float totalQty = 0;
            double totalPrice = 0;

            var item = order.Client.ExtraProperties != null ? order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                if (detail.Qty == 0)
                    continue;

                if (detail.IsCredit != iscredit)
                    continue;

                int index = 0;
                totalQty += detail.Qty;

                var price = detail.Price;
                if (detail.IsCredit)
                    price *= -1;

                totalPrice += price * detail.Qty;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(detail.Product.Name, true);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        string consOld = detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture);
                        string consCount = detail.ConsignmentCount.ToString(CultureInfo.CurrentCulture);

                        if(iscredit)
                        {
                            consOld = string.Empty;
                            consCount = string.Empty;
                        }

                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                                    consOld,
                                                    consCount,
                                                    detail.Qty.ToString(CultureInfo.CurrentCulture),
                                                    price.ToCustomString(),
                                                    (price * detail.Qty).ToCustomString()
                                                    ));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }

                if (useRelated)
                {
                    var related = GetRelatedProduct(detail.Product);

                    if (related != null)
                    {
                        var relatedPrice = Product.GetPriceForProduct(related, order, false, false);

                        index = 0;
                        var relatedSlices = GetDetailsRowsSplitProductNameConsignment(related.Name, true);
                        foreach (var productNamePart in relatedSlices)
                        {
                            if (index == 0)
                            {
                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                                            string.Empty,
                                                            string.Empty,
                                                            detail.Qty.ToString(CultureInfo.CurrentCulture),
                                                            relatedPrice.ToCustomString(),
                                                            (relatedPrice * detail.Qty).ToCustomString()));

                                startIndex += font18Separation;
                            }
                            else if (!Config.PrintTruncateNames)
                            {
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                                startIndex += font18Separation;
                            }
                            else
                                break;
                            index++;
                        }

                        totalQty += detail.Qty;
                        totalPrice += relatedPrice * detail.Qty;
                    }
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsTotal], startIndex, "Totals:",
                                    totalQty, totalPrice.ToCustomString()));

            startIndex += font36Separation;

            return lines;
        }

        Product GetRelatedProduct(Product product)
        {
            int relatedId = 0;

            foreach (var p in product.ExtraProperties)
            {
                if (p.Item1.ToLower() == "relateditem")
                {
                    relatedId = Convert.ToInt32(p.Item2);
                    break;
                }
            }

            return Product.Find(relatedId, true);
        }

        IList<string> GetBatteryConsRotRows(ref int startIndex, Order order)
        {
            var lines = new List<string>();

            float totalQty = 0;
            double totalCost = 0;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "ROT",
                    string.Empty, string.Empty, "Qty", "Price", "Total"));
            startIndex += font18Separation;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                var rotation = UDFHelper.GetSingleUDF("rotatedQty", detail.ExtraFields);

                if (string.IsNullOrEmpty(rotation))
                    continue;

                var qty = int.Parse(rotation);

                if (qty == 0)
                    continue;

                var rotatedId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");

                if (rotatedId == null)
                    continue;

                var rotated = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == rotatedId.Item2);

                if (rotated == null)
                    continue;

                var name = rotated.Name;
                var price = Product.GetPriceForProduct(rotated, order, false, false);

                if (!Config.ChargeBatteryRotation)
                    price = 0;

                int index = 0;
                totalQty += qty;
                totalCost += qty * price;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(name, true);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productNamePart,
                                        string.Empty, string.Empty, rotation, price.ToCustomString(),
                                        (qty * price).ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }

            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsTotal], startIndex, "Totals:",
                                    totalQty, totalCost.ToCustomString()));

            startIndex += font36Separation;

            return lines;
        }

        IList<string> GetBatteryConsAdjRows(ref int startIndex, Order order)
        {
            var lines = new List<string>();

            float totalQty = 0;
            double zero = 0;
            double adjTotalCost = 0;

            int totalAdj = 0;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "",
                    string.Empty, "", "Pro", "", ""));
            startIndex += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "ADJ",
                    string.Empty, "Age", "rate", "Price", "Total"));
            startIndex += font18Separation;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                int time = 0;
                if(Config.WarrantyPerClient)
                {
                    time = order.GetIntWarrantyPerClient(detail.Product);
                    if (time == 0)
                        continue;
                }

                var adjQty = UDFHelper.GetSingleUDF("adjustedQty", detail.ExtraFields);

                if (string.IsNullOrEmpty(adjQty))
                    continue;

                var adjId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");

                if (adjId == null)
                    continue;

                var adjustment = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == adjId.Item2);

                if (adjustment == null)
                    continue;

                if (!Config.WarrantyPerClient)
                {
                    var timeSt = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "time");

                    if (timeSt == null)
                        continue;

                    time = int.Parse(timeSt.Item2);
                }

                var ws = adjQty.Split(',');

                totalAdj += ws.Length;

                var adjPrice = Product.GetPriceForProduct(adjustment, order, false, false);

                var qty = int.Parse(ws[0]) - time > 0 ? int.Parse(ws[0]) : 0;
                double adjCost = qty * adjPrice;

                totalQty += qty;
                adjTotalCost += adjCost;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(adjustment.Name, true);

                int j = 0;

                for (int i = 0; i < productSlices.Count; i++, j++)
                {
                    if (i == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture,
                            linesTemplates[BatteryConsRotHeader], startIndex, productSlices[i],
                            string.Empty, ws[0], qty, (qty > 0 ? adjPrice : zero).ToCustomString(),
                            adjCost.ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        try
                        {
                            if (j < ws.Length)
                            {
                                qty = int.Parse(ws[j]) - time > 0 ? int.Parse(ws[j]) : 0;
                                adjCost = qty * adjPrice;

                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productSlices[i],
                                                    string.Empty, ws[j], qty, (qty > 0 ? adjPrice : zero).ToCustomString(),
                                        adjCost.ToCustomString()));

                                totalQty += qty;
                                adjTotalCost += adjCost;
                            }
                            else
                                throw new Exception();

                        }
                        catch
                        {
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productSlices[i],
                                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        }

                        startIndex += font18Separation;
                    }
                    else
                    {
                        for (; j < ws.Length; j++)
                        {
                            qty = int.Parse(ws[j]) - time > 0 ? int.Parse(ws[j]) : 0;
                            adjCost = qty * adjPrice;

                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, string.Empty,
                                                    string.Empty, ws[j], qty, (qty > 0 ? adjPrice : zero).ToCustomString(),
                                        adjCost.ToCustomString()));

                            startIndex += font18Separation;

                            adjTotalCost += adjCost;
                            totalQty += qty;
                        }
                        break;
                    }
                }
                if (ws.Length > j)
                    for (; j < ws.Length; j++)
                    {
                        qty = int.Parse(ws[j]) - time > 0 ? int.Parse(ws[j]) : 0;
                        adjCost = qty * adjPrice;

                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, string.Empty,
                                                    string.Empty, ws[j], qty, (qty > 0 ? adjPrice : zero).ToCustomString(),
                                        adjCost.ToCustomString()));
                        startIndex += font18Separation;

                        adjTotalCost += adjCost;
                        totalQty += qty;
                    }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "^FO340,{0}^ADN,18,10^FD{1}^FS^FO513,{0}^ADN,18,10^FD{2}^FS^FO670,{0}^ADN,18,10^FD{3}^FS",
                startIndex, 
                "Total Charge:",
                totalQty, adjTotalCost.ToCustomString()));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "^FO325,{0}^ADN,18,10^FD{1}^FS^FO513,{0}^ADN,18,10^FD{2}^FS^FO670,{0}^ADN,18,10^FD{3}^FS",
                startIndex,
                "Total Adj Qty:",
                totalAdj, ""));
            startIndex += font36Separation;

            return lines;
        }

        IList<string> GetBatteryConsCoreRows(ref int startIndex, Order order)
        {
            var lines = new List<string>();

            float totalQty = 0;
            double coreCost = 0;
            float totalCorescollected = 0;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "Core",
                    string.Empty, string.Empty, "Qty", "Price", "Total"));
            startIndex += font18Separation;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted || (!Config.AddCoreBalance && detail.Qty.Equals(0)))
                    continue;

                var core = UDFHelper.GetSingleUDF("coreQty", detail.ExtraFields);
                var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                if (string.IsNullOrEmpty(core) || coreId == null)
                    continue;

                var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

                if (relatedCore == null)
                    continue;

                var coreQty = Convert.ToDouble(core);

                totalCorescollected += (float)coreQty;

                var qty = detail.Qty - coreQty;

                var corePrice = Product.GetPriceForProduct(relatedCore, order, false, false);

                bool chargeCore = true;

                if (order.Client.NonVisibleExtraProperties != null)
                {
                    var xC = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "corepaid");
                    if (xC != null && xC.Item2.ToLowerInvariant() == "n")
                        chargeCore = false;
                }

                if (!chargeCore)
                    corePrice = 0;

                if (Config.CoreAsCredit)
                {
                    qty = coreQty;
                    corePrice *= -1;
                }
                else if (qty < 0)
                {
                    qty *= -1;
                    corePrice *= -1;
                }

                if (qty == 0)
                    continue;

                totalQty += (float)qty;
                coreCost += double.Parse(Math.Round(corePrice * qty, Config.Round).ToCustomString(), NumberStyles.Currency);

                int index = 0;
                var productSlices = GetDetailsRowsSplitProductNameConsignment(relatedCore.Name, true);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productNamePart,
                                                string.Empty, string.Empty, qty, corePrice.ToCustomString(),
                                                (corePrice * qty).ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex,
                                                productNamePart, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsTotal], startIndex, "Totals:",
                                    totalQty, coreCost.ToCustomString()));
            startIndex += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex,
                                                "    Total Number Of Cores Picked Up:", string.Empty, string.Empty, totalCorescollected, string.Empty, string.Empty));

            startIndex += font36Separation;

            return lines;
        }


        IList<string> GetBatteryConsAdjConfRows(ref int startIndex, Order order, List<Tuple<Product, float>> cores)
        {
            var lines = new List<string>();

            float totalQty = 0;
            double zero = 0;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "Warranty",
                    string.Empty, string.Empty, "Qty", "Price", "Total"));
            startIndex += font18Separation;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                var adjQty = UDFHelper.GetSingleUDF("adjustedQty", detail.ExtraFields);

                if (string.IsNullOrEmpty(adjQty))
                    continue;

                var qty = int.Parse(adjQty);

                var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                if (coreId != null)
                {
                    var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

                    if (relatedCore != null)
                        cores.Add(new Tuple<Product, float>(relatedCore, qty));
                }

                totalQty += qty;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(detail.Product.Name, true);

                for (int i = 0; i < productSlices.Count; i++)
                {
                    if (i == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture,
                            linesTemplates[BatteryConsRotHeader], startIndex, productSlices[i],
                            string.Empty, string.Empty, qty, zero.ToCustomString(),
                            zero.ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productSlices[i],
                                                    string.Empty, string.Empty, string.Empty, string.Empty,
                                        string.Empty));

                        startIndex += font18Separation;
                    }
                    else
                        break;
                }

            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsTotal], startIndex, "Totals:",
                                    totalQty, zero.ToCustomString()));

            startIndex += font36Separation;

            return lines;
        }

        IList<string> GetBatteryConsCoreConfigRows(ref int startIndex, Order order, List<Tuple<Product, float>> cores)
        {
            var lines = new List<string>();

            float totalQty = 0;
            double coreCost = 0;
            string zero = "(" + (0).ToCustomString() + ")";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, "Core",
                    string.Empty, string.Empty, "Qty", "Price", "Total"));
            startIndex += font18Separation;

            foreach (var item in cores)
            {
                totalQty += item.Item2;

                int index = 0;
                var productSlices = GetDetailsRowsSplitProductNameConsignment(item.Item1.Name, true);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productNamePart,
                                                string.Empty, string.Empty, item.Item2, zero,
                                                zero));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex,
                                                productNamePart, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }
            }

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted || detail.Qty.Equals(0))
                    continue;

                var core = UDFHelper.GetSingleUDF("coreQty", detail.ExtraFields);
                var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                if (string.IsNullOrEmpty(core) || coreId == null)
                    continue;

                var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

                if (relatedCore == null)
                    continue;

                var coreQty = Convert.ToDouble(core);


                var qty = detail.Qty - (float)coreQty;

                var corePrice = Product.GetPriceForProduct(relatedCore, order, false, false);

                if (Config.CoreAsCredit)
                {
                    qty = (float)coreQty;
                    corePrice *= -1;
                }

                if (qty == 0)
                    continue;

                totalQty += qty;
                coreCost += double.Parse(Math.Round(corePrice * qty, 4).ToCustomString(), NumberStyles.Currency);

                int index = 0;
                var productSlices = GetDetailsRowsSplitProductNameConsignment(relatedCore.Name, true);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsRotHeader], startIndex, productNamePart,
                                                string.Empty, string.Empty, qty, corePrice.ToCustomString(),
                                                (corePrice * qty).ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex,
                                                productNamePart, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsTotal], startIndex, "Totals:",
                                    totalQty, coreCost.ToCustomString()));

            startIndex += font36Separation;

            return lines;
        }





        public override bool PrintBatteryEndOfDay(int index, int count)
        {
            var consigments = Order.Orders.Where(x => x.OrderType == OrderType.Consignment && x.Details.Any(y => y.ConsignmentCounted)).ToList();

            List<string> lines = new List<string>();

            int startY = 80;

            string title = "Battery Report";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderTitle1], startY, title));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font36Separation; //an extra line

            if (!Config.CoreAsCredit)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsReportHeader], startY));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsReportHeader2], startY));
            startY += font36Separation;

            int rotatedTotal = 0;
            int adjustmentTotal = 0;
            int coreTotal = 0;

            foreach (var order in consigments)
            {
                if (order.Voided)
                    continue;

                if (!Config.CoreAsCredit)
                    lines.AddRange(GetBatteryEndOfDayOrderLines(ref startY, order, ref rotatedTotal, ref adjustmentTotal, ref coreTotal));
            }


            startY += font36Separation;

            startY += font36Separation;
            string s;

            s = "TOTAL ROTATED:";
            s = new string(' ', WidthForBoldFont - s.Length - 7) + s;
            var s1 = rotatedTotal.ToString();
            s1 = new string(' ', 3) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            s = "TOTAL ADJUSTED:";
            s = new string(' ', WidthForBoldFont - s.Length - 7) + s;
            s1 = adjustmentTotal.ToString();
            s1 = new string(' ', 3) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            s = "TOTAL CORES:";
            s = new string(' ', WidthForBoldFont - s.Length - 7) + s;
            s1 = coreTotal.ToString();
            s1 = new string(' ', 3) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            foreach (string row in GetFooterRows(ref startY, false))
                lines.Add(row);

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            var sb = new StringBuilder();
            foreach (string s2 in lines)
                sb.Append(s2);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        IList<string> GetBatteryEndOfDayOrderLines(ref int startIndex, Order order, ref int rotatedTotal, ref int adjustmentTotal, ref int coreTotal)
        {
            List<string> lines = new List<string>();

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                string rotatedQty = UDFHelper.GetSingleUDF("rotatedQty", detail.ExtraFields);
                string adjusted = UDFHelper.GetSingleUDF("adjustedQty", detail.ExtraFields);
                string coreQty = UDFHelper.GetSingleUDF("coreQty", detail.ExtraFields);

                if (!string.IsNullOrEmpty(rotatedQty))
                    rotatedTotal += Convert.ToInt32(rotatedQty, CultureInfo.InvariantCulture);

                if (string.IsNullOrEmpty(coreQty))
                {
                    var coreProd = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");
                    if (coreProd != null)
                        coreQty = detail.Qty.ToString();
                }

                if (!Config.AddCoresInSalesItem && detail.ConsignmentSalesItem)
                    coreQty = "0";

                if (!string.IsNullOrEmpty(coreQty))
                    coreTotal += Convert.ToInt32(coreQty, CultureInfo.InvariantCulture);

                var ws = new List<string>();

                if (!string.IsNullOrEmpty(adjusted))
                    ws = adjusted.Split(',').ToList();

                adjustmentTotal += ws.Count;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(detail.Product.Name, true);
                int index = 0;
                foreach (var slice in productSlices)
                {
                    string w = index < ws.Count ? ws[index] : "";

                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsReportLines], startIndex, slice,
                            !string.IsNullOrEmpty(rotatedQty) ? rotatedQty : "0", (w != "" ? "1" : "0"), w, !string.IsNullOrEmpty(coreQty) ? coreQty : "0"));
                    }
                    else
                    {
                        var prod = Config.PrintTruncateNames ? "" : slice;

                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsReportLines], startIndex, prod,
                            string.Empty, (w != "" ? "1" : w), w, string.Empty));
                    }

                    index++;
                    startIndex += font18Separation;
                }

                for (int i = index; i < ws.Count; i++)
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatteryConsReportLines], startIndex, string.Empty,
                            string.Empty, "1", ws[i], string.Empty));
                    startIndex += font18Separation;
                }
            }

            return lines;
        }



        public bool BatteryInventorySettlement(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderTitle1], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatInvSettDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatInvSettDetailsHeader2], startY));
            startY += font18Separation;

            BatInvSettRow totalRow = new BatInvSettRow(new InventorySettlementRow());

            var map = BatExtendedSendTheLeftOverInventory();
            foreach (var x in map.Keys)
            {
                var value = map[x];
                var product = value.SettRow.Product;

                totalRow.SettRow.Product = product;
                totalRow.SettRow.BegInv += product.BeginigInventory;
                totalRow.SettRow.LoadOut += product.RequestedLoadInventory;
                totalRow.SettRow.Adj += product.LoadedInventory - product.RequestedLoadInventory;
                totalRow.SettRow.TransferOn += product.TransferredOnInventory;
                totalRow.SettRow.TransferOff += product.TransferredOffInventory;
                totalRow.SettRow.EndInventory += product.CurrentInventory;
                totalRow.SettRow.Unload += product.UnloadedInventory;
                totalRow.SettRow.DamagedInTruck += product.DamagedInTruckInventory;

                totalRow.SettRow.Sales += value.SettRow.Sales;
                totalRow.SettRow.CreditReturns += value.SettRow.CreditReturns;
                totalRow.SettRow.CreditDump += value.SettRow.CreditDump;

                totalRow.Core += value.Core;
                totalRow.BatAdj += value.BatAdj;
                totalRow.Rot += value.Rot;
            }

            startY += font18Separation;
            var oldRound = Config.Round;
            Config.Round = 2;

            foreach (var pp in SortedDetails(map.Values))
            {
                var p = pp.SettRow;

                if (p.BegInv == 0 && p.LoadOut == 0 && p.Adj == 0 && p.TransferOff == 0 && p.TransferOn == 0 && p.Sales == 0 &&
                    p.DamagedInTruck == 0 && p.Unload == 0 && p.EndInventory == 0 && pp.Core == 0 && pp.BatAdj == 0 && pp.Rot == 0)
                    continue;

                int productLineOffset = 0;

                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        var newS = string.Format(CultureInfo.InvariantCulture, linesTemplates[BatInvSettDetailRow], startY, pName,
                                                 Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                p.OverShort,
                                                Math.Round(pp.Core, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(pp.BatAdj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(pp.Rot, Config.Round).ToString(CultureInfo.CurrentCulture));
                        lines.Add(newS);
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatInvSettDetailRow], startY, pName,
                            "", "", "", "", "", "", "", "", "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
            }

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BatInvSettDetailRow], startY, "Totals:",
                                                Math.Round(totalRow.SettRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.SettRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.SettRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.SettRow.TransferOn - totalRow.SettRow.TransferOff, Config.Round).ToString(CultureInfo.InvariantCulture),
                                                Math.Round(totalRow.SettRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.SettRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.SettRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                totalRow.SettRow.OverShort,
                                                Math.Round(totalRow.Core, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.BatAdj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Rot, Config.Round).ToString(CultureInfo.CurrentCulture)));

            Config.Round = oldRound;
            startY += font18Separation;

            //space
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s1 in lines)
                sb.Append(s1);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                Logger.CreateLog("Starting printing inventory settlement");
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        class BatInvSettRow
        {
            public InventorySettlementRow SettRow { get; set; }

            public float Core { get; set; }

            public float BatAdj { get; set; }

            public float Rot { get; set; }

            public BatInvSettRow(InventorySettlementRow r)
            {
                SettRow = r;
            }
        }

        Dictionary<int, BatInvSettRow> BatExtendedSendTheLeftOverInventory()
        {
            Product.CoreProducts();

            var map = new Dictionary<int, BatInvSettRow>();

            Dictionary<int, float> pendingCores = new Dictionary<int, float>();

            foreach (var product in Product.Products.Where(x => x.ProductType == ProductType.Inventory && x.CategoryId > 0))
            {
                var pc = Product.coreProducts.FirstOrDefault(x => x.Item2 == product.ProductId);

                if (pc != null)
                {
                    if (map.ContainsKey(pc.Item1))
                        map[pc.Item1].Core += product.UnloadedInventory;
                    else
                    {
                        if (!pendingCores.ContainsKey(pc.Item1))
                            pendingCores.Add(pc.Item1, 0);
                        pendingCores[pc.Item1] += product.UnloadedInventory;
                    }
                }
                else
                {
                    var inv = new InventorySettlementRow();
                    inv.Product = product;
                    inv.BegInv = product.BeginigInventory;
                    inv.LoadOut = product.RequestedLoadInventory;
                    inv.Adj = product.LoadedInventory - product.RequestedLoadInventory;
                    inv.TransferOn = product.TransferredOnInventory;
                    inv.TransferOff = product.TransferredOffInventory;
                    inv.EndInventory = product.CurrentInventory;
                    inv.DamagedInTruck = product.DamagedInTruckInventory;
                    inv.Unload = product.UnloadedInventory;


                    float core = pendingCores.ContainsKey(product.ProductId) ? pendingCores[product.ProductId] : 0;

                    map.Add(product.ProductId, new BatInvSettRow(inv) { Core = core });
                }
            }

            foreach (var o in Order.Orders.Where(x => !x.Reshipped))
                if (o.OrderType != OrderType.Load && !o.Voided && !o.AsPresale)
                    foreach (var od in o.Details)
                    {
                        if (od.Product == null)
                            continue;
                        if (!map.ContainsKey(od.Product.ProductId))
                            continue;
                        if (od.Product.ProductType != ProductType.Inventory)
                            continue;
                        float factor = 1;
                        if (od.UnitOfMeasure != null)
                            factor = od.UnitOfMeasure.Conversion;
                        var item = map[od.Product.ProductId];

                        if (o.OrderType == OrderType.Credit || (o.OrderType == OrderType.Order && od.IsCredit))
                        {

                            if (od.Damaged)
                            {
                                item.SettRow.CreditDump += od.Qty * factor;
                            }
                            else
                            {
                                item.SettRow.CreditReturns += od.Qty * factor;
                            }
                        }
                        else
                        {
                            if (o.OrderType == OrderType.Consignment)
                            {
                                var base_ = od.ConsignmentOld;
                                if (od.ConsignmentCounted)
                                    base_ -= od.ConsignmentCount;

                                if (od.ConsignmentUpdated)
                                {
                                    base_ += od.ConsignmentNew - od.ConsignmentOld;
                                }
                                if (od.ConsignmentCounted || od.ConsignmentUpdated)
                                {
                                    if (od.ConsignmentSalesItem)
                                        item.SettRow.Sales += od.Qty * factor;
                                    else
                                        item.SettRow.Sales += base_ * factor;
                                }


                                string rotatedQty = UDFHelper.GetSingleUDF("rotatedQty", od.ExtraFields);
                                string adjustedQty = UDFHelper.GetSingleUDF("adjustedQty", od.ExtraFields);
                                string coreQty = UDFHelper.GetSingleUDF("coreQty", od.ExtraFields);

                                //si es un consignment veo si lo related de las baterias se agregaron a map y pongo el valor en ese item

                                if (!string.IsNullOrEmpty(rotatedQty))
                                {
                                    map[od.Product.ProductId].Rot += Convert.ToInt32(rotatedQty, CultureInfo.InvariantCulture);
                                }

                                if (!string.IsNullOrEmpty(coreQty))
                                {
                                    map[od.Product.ProductId].Core += Convert.ToInt32(coreQty, CultureInfo.InvariantCulture);
                                }

                                if (!string.IsNullOrEmpty(adjustedQty))
                                {
                                    if (Config.CoreAsCredit)
                                    {
                                        map[od.Product.ProductId].BatAdj += Convert.ToInt32(adjustedQty, CultureInfo.InvariantCulture);

                                        map[od.Product.ProductId].Core += Convert.ToInt32(adjustedQty, CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        var ws = adjustedQty.Split(',').ToList();
                                        map[od.Product.ProductId].BatAdj += ws.Count();

                                    }
                                }
                            }
                            else
                            {
                                item.SettRow.Sales += od.Qty * factor;
                            }
                        }
                    }
            return map;
        }

        IQueryable<BatInvSettRow> SortedDetails(IEnumerable<BatInvSettRow> source)
        {
            IQueryable<BatInvSettRow> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = source.OrderBy(x => x.SettRow.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = source.OrderBy(x => x.SettRow.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = source.OrderBy(x => x.SettRow.Product.CategoryId).ThenBy(x => x.SettRow.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<BatInvSettRow>>();
                    foreach (var od in source)
                    {
                        var t = new T<BatInvSettRow>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.SettRow.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.SettRow.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<BatInvSettRow>>();
                    foreach (var od in source)
                    {
                        var t = new T<BatInvSettRow>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.SettRow.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.SettRow.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = source.OrderBy(x => x.SettRow.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = source.OrderBy(x => x.SettRow.Product.PriceLevel0).AsQueryable();
                    break;
                default:
                    retList = source.OrderBy(x => x.SettRow.Product.Name).AsQueryable();
                    break;
            }

            if (Config.DefaultItem > 0)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.SettRow.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        class T<T1>
        {
            public string CategoryName { get; set; }

            public T1 HoldedValue { get; set; }
        }
    }
}