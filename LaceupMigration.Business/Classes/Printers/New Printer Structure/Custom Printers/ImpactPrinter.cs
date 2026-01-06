using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ImpactPrinter : ZebraFourInchesPrinter1
    {
        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            var orders = GetOrdersToPrint(order);

            if (!base.PrintOrder(orders[0], asPreOrder))
                return false;

            return base.PrintOrder(orders[1], asPreOrder);
        }

        private List<Order> GetOrdersToPrint(Order order)
        {
            Order firstOrder = Order.DuplicateorderHeader(order);
            Order secondOrder = Order.DuplicateorderHeader(order);

            foreach (var od in order.Details)
            {
                var product = od.Product.Name;
                var price = od.Price;
                var priceleve = od.Product.PriceLevel0;
                
                var det = od.GetOrderDetailCopy();

                // if (Config.HidePriceInTransaction)
                //     det.Price = Product.GetPriceForProduct(od.Product, order.Client, true, false);
                // else
                //     det.Price = 0;

                det.Price = 0;
                
                firstOrder.AddDetail(od);
                secondOrder.AddDetail(det);
            }

            return new List<Order>() { firstOrder, secondOrder};
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

            List<int> relateds = new List<int>();

            if (Config.CoolerCoCustomization)
            {
                foreach (var detail in order.Details.Where(x => x.RelatedOrderDetail > 0).ToList())
                {
                    relateds.Add(detail.RelatedOrderDetail);

                    var values = UDFHelper.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                    if (!string.IsNullOrEmpty(values))
                    {
                        var parts = values.Split(",");

                        foreach (var p in parts)
                        {
                            int orderDetailId = 0;
                            Int32.TryParse(p, out orderDetailId);

                            if (orderDetailId > 0 && !relateds.Contains(orderDetailId))
                                relateds.Add(orderDetailId);
                        }
                    }
                }
            }

            foreach (var detail in lines)
            {
                bool isDisocuntItem = false;
                if (detail.Product.IsDiscountItem)
                {
                    isDisocuntItem = true;
                }

                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                bool printUom = true;

                if (Config.CoolerCoCustomization && relateds.Count > 0 && relateds.Contains(detail.OrderDetail.OrderDetailId))
                    printUom = false;

                string uomString = null;
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null && printUom)
                    {
                        uomString = detail.OrderDetail.UnitOfMeasure.Name;
                        if (!uomMap.ContainsKey(uomString))
                            uomMap.Add(uomString, 0);
                        uomMap[uomString] += detail.Qty;

                        string georgehoweValue = UDFHelper.GetSingleUDF("georgehowe", detail.OrderDetail.UnitOfMeasure.ExtraFields);
                        if (int.TryParse(georgehoweValue, out int conversionFactor))
                        {
                            totalQtyNoUoM += detail.Qty * conversionFactor;
                        }
                        else
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

                if (isDisocuntItem)
                    name = detail.OrderDetail.Comments;

                var productSlices = GetOrderDetailsRowsSplitProductName(name);

                string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                if (detail.OrderDetail.UnitOfMeasure != null && printUom)
                    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                var splitQtyAsString = SplitProductName(qtyAsString, 10, 10);

                foreach (string pName in productSlices)
                {
                    string currentQty = (productLineOffset < splitQtyAsString.Count) ? splitQtyAsString[productLineOffset] : string.Empty;

                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        if (!order.AsPresale && detail.ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                        {
                            var groupedDetails = detail.ParticipatingDetails
                                .Where(od => !od.Product.IsDiscountItem)
                                .GroupBy(od => new { od.Product.ProductId, od.Price });

                            foreach (var grouped in groupedDetails)
                            {
                                var firstItem = grouped.First();

                                var copy = firstItem.GetOrderDetailCopy();
                                copy.Qty = grouped.Sum(x => x.Qty);
                                copy.Weight = grouped.Sum(x => x.Weight);
                                copy.Allowance = grouped.Sum(x => x.Allowance);
                                copy.Discount = grouped.Sum(x => x.Discount);

                                d += order.CalculateOneItemCost(copy, true);
                            }
                        }
                        else
                        {
                            foreach (var _ in detail.ParticipatingDetails)
                            {
                                d += order.CalculateOneItemCost(_, true);
                            }
    
                        }

                        double price = detail.Price * factor;

                        if (!isDisocuntItem)
                            balance += d;

                        //string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        //if (detail.OrderDetail.UnitOfMeasure != null)
                        //    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        // if (Config.HidePriceInPrintedLine)
                        //     priceAsString = string.Empty;
                        // if (Config.HideTotalInPrintedLine)
                        //     totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;
                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, currentQty, totalAsString, priceAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, pName, currentQty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                while (productLineOffset < splitQtyAsString.Count)
                {
                    string remainingQty = splitQtyAsString[productLineOffset];
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, string.Empty, remainingQty)); //OrderDetailsLines2
                    startIndex += font18Separation;

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

                list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()) && !isDisocuntItem)
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

            if (order.ConvertedInvoice)
                paid = order.Paid;

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
                if (!order.AsPresale && sales[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalSales, ref taxableAmount, ref salesBalance, sales[key].ParticipatingDetails, order, false);
                }
                else
                {
                    foreach (var od in sales[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalSales += qty;
                        
                        var x = order.CalculateOneItemCost(od, false);
                        salesBalance += x;

                        if (sales[key].Product.Taxable)
                            taxableAmount += x;
                    }
                }
            }
            foreach (var key in credit.Keys)
            {
                if (!order.AsPresale && credit[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalCredit, ref taxableAmount, ref creditBalance, credit[key].ParticipatingDetails, order, true);
                }
                else
                {
                    foreach (var od in credit[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalCredit += qty;

                        var x = order.CalculateOneItemCost(od, false);

                        creditBalance += x;

                        if (credit[key].Product.Taxable)
                            taxableAmount -= x;
                    }
                }
            }
            foreach (var key in returns.Keys)
            {
                if (!order.AsPresale && returns[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalReturn, ref taxableAmount, ref returnBalance, returns[key].ParticipatingDetails, order, true);
                }
                else
                {
                    foreach (var od in returns[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalReturn += qty;
                        
                        var x = order.CalculateOneItemCost(od, false);

                        returnBalance += x;

                        if (returns[key].Product.Taxable)
                            taxableAmount -= x;
                    }
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");


            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s1 = ToString(salesBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s1 = ToString(creditBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s1 = ToString(returnBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsReturns], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString(Math.Round((salesBalance + creditBalance + returnBalance), Config.Round));
                s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if (!order.IsWorkOrder)
                {
                    if ((order.Client.UseDiscount || order.Client.UseDiscountPerLine || order.IsDelivery || OrderDiscount.HasDiscounts) && !Config.HideDiscountTotalPrint)
                    {
                        if (Config.ShowDiscountIfApplied)
                        {
                            if (discount != 0)
                            {
                                s1 = ToString(Math.Abs(discount));
                                s1 = new string(' ', 16 - s1.Replace(")", "").Length) + s1;
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                                startY += font36Separation;
                            }
                        }
                        else
                        {
                            s1 = ToString(Math.Abs(discount));
                            s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                            startY += font36Separation;
                        }
                    }
                }

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                if (!Config.HideTaxesTotalPrint)
                {
                    s1 = ToString(tax);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                    startY += font36Separation;
                }

                double otherCharges = 0;
                if (order.IsWorkOrder || Config.AllowOtherCharges)
                {
                    s1 = ToString(order.CalculatedOtherCharges());
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsOtherCharges], startY, s1));
                    startY += font36Separation;
                    s1 = ToString(order.CalculatedFreight());
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsFreight], startY, s1));
                    startY += font36Separation;

                    otherCharges = order.CalculatedFreight() + order.CalculatedOtherCharges();
                }

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax + otherCharges;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice && !order.IsWorkOrder)
                {
                    s1 = ToString(Math.Round(paid, Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;

                    s1 = ToString(Math.Round((s4 - paid), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (Config.PrintClientTotalOpenBalance)
                {
                    s1 = ToString(Math.Round(order.Client.CurrentBalance(), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                if (order.IsWorkOrder)
                {
                    var comment = string.Empty;
                    var reply = string.Empty;

                    const string marker = "Reply:";
                    var parts = order.Comments.Split(new[] { marker }, StringSplitOptions.None);
                    comment = parts[0].Trim();
                    if (parts.Length > 1) reply = parts[1].Trim();


                    startY += font18Separation;
                    var clines = GetOrderSplitComment(comment);
                    for (int i = 0; i < clines.Count; i++)
                    {
                        string format = linesTemplates[OrderComment];
                        if (i > 0)
                            format = linesTemplates[OrderComment2];

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                        startY += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(reply))
                    {
                        startY += font18Separation;
                        clines = GetOrderSplitComment(reply);
                        for (int i = 0; i < clines.Count; i++)
                        {
                            string format = linesTemplates[OrderClientAddress];
                            if (i > 0)
                                format = linesTemplates[OrderClientAddress];

                            if (i == 0)
                                list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, "Reply: " + clines[i]));
                            else
                                list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                            startY += font18Separation;
                        }
                    }
                }
                else
                {
                    var orderComments = order.Comments;

                    var reasons = Reason.GetReasonsByType(ReasonType.ReShip);
                    if (reasons.Count > 0 && reasons.Any(x => x.Description == order.Comments) && !order.Reshipped && order.IsDelivery)
                        orderComments = string.Empty;

                    startY += font18Separation;
                    var clines = GetOrderSplitComment(orderComments);
                    for (int i = 0; i < clines.Count; i++)
                    {
                        string format = linesTemplates[OrderComment];
                        if (i > 0)
                            format = linesTemplates[OrderComment2];

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                        startY += font18Separation;
                    }
                }
            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            return list;
        }

    }
}