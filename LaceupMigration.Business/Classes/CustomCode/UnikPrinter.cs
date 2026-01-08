using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class UnikPrinter : ZebraFourInchesPrinter
    {
        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();
            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();
            foreach (var od in order.Details)
            {
                var key = Guid.NewGuid().ToString();
                Dictionary<string, OrderLine> currentDic;
                if (!od.IsCredit)
                    currentDic = salesLines;
                else
                {
                    if (od.Damaged)
                        currentDic = creditLines;
                    else
                        currentDic = returnsLines;
                }
                currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od });
            }
            List<string> lines = new List<string>();

            int startY = 80;

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startY += Config.CompanyLogoHeight;
            }

            startY += 36;

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            lines.AddRange(GetHeaderRowsInOneDoc(ref startY, asPreOrder, order, order.Client, printedId, payments, payments != null && payments.Sum(x => x.Amount) == balance));

            string docName = "NOT AN INVOICE";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "NOT A BILL";
                }
            }

            if (asPreOrder && !Config.FakePreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
            }

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));
            //startY += 36;
            // redo the dictionary
            salesLines.Clear();
            creditLines.Clear();
            returnsLines.Clear();
            foreach (var od in order.Details)
            {
                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + "-" + (string.IsNullOrEmpty(od.Lot) ? "" : od.Lot);
                Dictionary<string, OrderLine> currentDic;
                if (!od.IsCredit)
                    currentDic = salesLines;
                else
                {
                    if (od.Damaged)
                        currentDic = creditLines;
                    else
                        currentDic = returnsLines;
                }
                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od, ParticipatingDetails = new List<OrderDetail>() });
                currentDic[key].ParticipatingDetails.Add(od);
                if (od.Product.SoldByWeight)
                    currentDic[key].Qty = currentDic[key].Qty + od.Weight;
                else
                    currentDic[key].Qty = currentDic[key].Qty + od.Qty;
            }
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));

            if (asPreOrder && !Config.FakePreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
            }

            // add the signature
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
                foreach (string s in GetFooterRows(ref startY, asPreOrder))
                {
                    lines.Add(s);
                }
            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                /*
				var fonts = new string[] { "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S"};
				foreach (var f in fonts)
				{
					string s = sb.ToString().Replace("^ADN", "^A" +f + "N");
					s = s.Replace("Client:", f + "-Client:");
					PrintIt(s);
				}
				*/
                string s = sb.ToString();//.Replace("^ADN", "^ARN");
                PrintIt(s);
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }

            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TICKETTYPE");
                if (terms != null && terms.Item2 == "4")
                    if (order.DeletedDetails.Count > 0)
                        PrintShortageReport(order);
                    else
                        foreach (var detail in order.Details)
                            if (detail.Ordered != detail.Qty && detail.Ordered > 0)
                            {
                                PrintShortageReport(order);
                                break;
                            }
            }
            return true;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            Dictionary<int, float> uomMap = new Dictionary<int, float>();

            Dictionary<Tuple<Product, double>, List<OrderDetail>> grouped = new Dictionary<Tuple<Product, double>, List<OrderDetail>>();
            foreach (var line in lines)
            {
                var key = new Tuple<Product, double>(line.Product, line.Price);
                if (!grouped.ContainsKey(key))
                {
                    grouped.Add(key, new List<OrderDetail>());
                }
                grouped[key].Add(line.OrderDetail);
            }

            double totalQty = 0;
            foreach (var p in grouped.Keys)
            {
                int productLineOffset = 0;
                foreach (string pName in GetDetailsRowsSplitProductName1(p.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        double price = Math.Round(grouped[p][0].Price, Config.Round);
                        float qty = 0;
                        double total = 0;

                        if (p.Item1.SoldByWeight)
                            foreach (var d in grouped[p])
                            {
                                total += d.Price * factor * d.Weight;
                                qty += d.Weight;
                                totalQty += d.Qty;
                            }
                        else
                            foreach (var d in grouped[p])
                            {
                                qty += d.Qty;
                                totalQty += d.Qty;
                                total += d.Price * factor * qty;
                            }

                        var decimals = PrecisionOf(qty);
                        if (decimals > 4)
                            qty = (float)Math.Round(qty, Config.Round);

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, pName, qty, Math.Round(total, Config.Round).ToCustomString(), Math.Round(price, Config.Round).ToCustomString()));
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
                if (grouped[p].Count > 1)
                {
                    StringBuilder sb1 = new StringBuilder();
                    foreach (var detail in grouped[p])
                    {
                        if (sb1.Length > 0)
                            sb1.Append(" ");
                        if (detail.Product.SoldByWeight)
                            sb1.Append(detail.Weight);
                        else
                            sb1.Append(detail.Qty);
                    }
                    foreach (var qtyLine in GetDetailsRowsSplitProductName1(sb1.ToString()))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, qtyLine, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    }
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, "Qty: " + grouped[p].Count.ToString(), string.Empty, string.Empty, string.Empty));
                    startIndex += font18Separation;
                }
                StringBuilder sb = new StringBuilder();
                List<string> lotUsed = new List<string>();
                foreach (var detail in grouped[p])
                {
                    if (!string.IsNullOrEmpty(detail.Lot) && !lotUsed.Contains(detail.Lot))
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        else
                            sb.Append("Lot: ");
                        sb.Append(detail.Lot);
                        lotUsed.Add(detail.Lot);
                    }
                }
                if (sb.Length > 0)
                {
                    foreach (var qtyLine in GetDetailsRowsSplitQtyLine(sb.ToString()))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, qtyLine, string.Empty, string.Empty, string.Empty));
                        startIndex += font18Separation;
                    } 
                }
                if (grouped[p].Count == 1 && grouped[p][0].Product.SoldByWeight && grouped[p][0].Qty > 1)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, "Qty: " + grouped[p][0].Qty, string.Empty, string.Empty, string.Empty));
                    startIndex += font18Separation;
                }

                if (p.Item1.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    if (Config.PrintUpcAsText)
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Item1.Upc));
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPC], startIndex, p.Item1.Upc));
                    startIndex += font36Separation + font18Separation;
                }
            }
            // -- line
            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            // print total of the section
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, "     Qty:" + Math.Round(totalQty, Config.Round).ToString(), string.Empty, string.Empty, string.Empty));
            //startIndex += font18Separation;
            if (uomMap.Count > 0)
            {
                foreach (var key in uomMap.Keys)
                {
                    var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == key);
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, "", uomMap[key], uom.Name, string.Empty));
                    startIndex += font18Separation;
                }
            }
            return list;
        }


        protected virtual IList<string> GetDetailsRowsSplitQtyLine(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        int PrecisionOf(float d)
        {
            var text = d.ToString(CultureInfo.InvariantCulture).TrimEnd('0');
            var decpoint = text.IndexOf('.');
            if (decpoint < 0)
                return 0;
            return text.Length - decpoint - 1;
        }
    }
}