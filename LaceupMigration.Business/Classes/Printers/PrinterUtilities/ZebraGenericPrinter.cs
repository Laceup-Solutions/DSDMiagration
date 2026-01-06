

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace LaceupMigration
{

    public abstract class ZebraGenericPrinter : ZebraPrinter
    {

        public ZebraGenericPrinter()
        {
            FillDictionary();
        }


        #region Print Open Invoice

        public override bool PrintOpenInvoice(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Printed on:" + DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            //Add the company details rows.

            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            var headerText = "Invoice #: ";

            if (invoice.InvoiceType == 1)
                headerText = "Credit #: ";
            else if (invoice.InvoiceType == 2)
                headerText = "Quote #: ";
            else if (invoice.InvoiceType == 2)
                headerText = "Sales Order #: ";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTitleNumber], startY, headerText + invoice.InvoiceNumber));
            startY += font36Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, "COPY"));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Created on:" + invoice.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            string extra = string.Empty;
            if (invoice.DueDate < DateTime.Today)
                extra = " OVERDUE";
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Due on:    " + invoice.DueDate.ToString(Config.OrderDatePrintFormat) + extra));
            startY += font36Separation;

            Client client = invoice.Client;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }
            var custno = DataAccess.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
                custNoString = " " + custno.Value;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTo], startY, custNoString));
            startY += font36Separation;

            foreach (string s1 in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s1.Trim(), startY));
                startY += font18Separation;
            }
            if (Config.PrintClientOpenBalance)
            {
                var balance = client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Account Balance: " + balance, startY));
                startY += font18Separation;
            }

            startY += font36Separation;

            foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(invoice.Comments ?? string.Empty))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, "C: " + commentPArt));
                startY += font18Separation;
            }

            double totalUnits = 0;
            double numberOfBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeader], startY));
            startY += font18Separation;

            Product notFoundProduct = new Product();
            notFoundProduct.Code = string.Empty;
            notFoundProduct.Cost = 0;
            notFoundProduct.Description = "Not found product";
            notFoundProduct.Name = "Not found product";
            notFoundProduct.Package = "1";
            notFoundProduct.ProductType = ProductType.Inventory;
            notFoundProduct.UoMFamily = string.Empty;
            notFoundProduct.Upc = string.Empty;

            //foreach (var item in invoice.Details)
            //    if (item.Product == null)
            //        item.Product = notFoundProduct;

            IQueryable<InvoiceDetail> source = SortDetails.SortedDetails(invoice.Details);
            foreach (InvoiceDetail detail in source)
            {
                Product p = detail.Product;

                int productLineOffset = 0;
                foreach (string pName in GetDetailsRowsSplitProductName1(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        double d = detail.Quantity * detail.Price;
                        double price = detail.Price;
                        double package = 1;
                        try
                        {
                            package = Convert.ToSingle(detail.Product.Package, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }
                        double units = detail.Quantity * package;
                        totalUnits += units;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startY, pName, detail.Quantity, d.ToCustomString(), price.ToCustomString()));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSecondLine], startY, pName));
                    productLineOffset++;
                    startY += font18Separation;
                }

                if (!string.IsNullOrEmpty(detail.Comments.Trim()))
                {

                    foreach (string commentPArt in GetDetailsRowsSplitProductName2(detail.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, "C: " + commentPArt));
                        startY += font18Separation;
                    }
                }

                var upc = detail.Product.Upc;

                if (invoice != null && invoice.Client != null && invoice.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(detail.Product.Sku.Trim()))
                    upc = detail.Product.Sku;

                if (upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    bool printMe = true;
                    if (!string.IsNullOrEmpty(p.NonVisibleExtraFieldsAsString))
                    {
                        var item = p.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                        if (item != null)
                            printMe = item.Item2 != "0";
                    }
                    if (printMe)
                        if (Config.PrintUpcAsText)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startY, upc));
                            startY += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            lines.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, Product.GetFirstUpcOnly(upc)));
                            startY += font36Separation;
                        }
                }
                startY += font18Separation + orderDetailSeparation; //a little extra space
                numberOfBoxes += Convert.ToSingle(detail.Quantity);
            }
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal1], startY, ""));
            startY += font18Separation;

            string s;

            s = "Total:" + invoice.Amount.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
            startY += font36Separation;

            if (invoice.Balance > 0)
            {
                InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null));

                var payments = existPayment != null ? existPayment.Components : null;
                if (payments != null && payments.Count > 0)
                {
                    var totalPaid = payments.Sum(x => x.Amount);

                    var paidInFull = totalPaid == invoice.Balance;

                    if (paidInFull)
                    {
                        s = "Paid In Full";
                        if (WidthForBoldFont - s.Length > 0)
                            s = new string((char)32, WidthForBoldFont - s.Length) + s;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                        startY += font36Separation;
                    }
                    else
                    {
                        s = "Partial payment:" + totalPaid.ToCustomString();
                        if (WidthForBoldFont - s.Length > 0)
                            s = new string((char)32, WidthForBoldFont - s.Length) + s;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                        startY += font36Separation;

                        s = "Open:" + (invoice.Balance - totalPaid).ToCustomString();
                        if (WidthForBoldFont - s.Length > 0)
                            s = new string((char)32, WidthForBoldFont - s.Length) + s;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                        startY += font36Separation;
                    }
                }
                else
                {
                    s = "Open:" + invoice.Balance.ToCustomString();
                    if (WidthForBoldFont - s.Length > 0)
                        s = new string((char)32, WidthForBoldFont - s.Length) + s;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                    startY += font36Separation;
                }
            }
            else
            {
                s = "Paid In Full";
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, WidthForBoldFont - s.Length) + s;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                startY += font36Separation;
            }

            startY += font18Separation;
            s = "Qty Items:" + numberOfBoxes.ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
            startY += font36Separation;
            s = "Qty Units:" + totalUnits.ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string s2 in lines)
                sb.Append(s2);

            try
            {
                string s3 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s3);
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

        #endregion


        #region Print Order

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();
            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = null;

            // var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();
            var rItems = order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.RelatedOrderDetail).ToList();
            rItems.AddRange(order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.OrderDetailId));
            foreach (var od in order.Details)
            {
                if (od.HiddenItem)
                    continue;

                var uomId = -1;
                if (od.UnitOfMeasure != null)
                    uomId = od.UnitOfMeasure.Id;

                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + "-" + uomId.ToString();
                if (!Config.GroupLinesWhenPrinting || (!Config.GroupRelatedWhenPrinting && rItems.Contains(od.OrderDetailId)))
                    key = Guid.NewGuid().ToString();
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
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, ListPrice = od.ExpectedPrice, OrderDetail = od, ParticipatingDetails = new List<OrderDetail>() });
                currentDic[key].Qty = currentDic[key].Qty + od.Qty;
                currentDic[key].ParticipatingDetails.Add(od);
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

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
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

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                    startY += font36Separation;
                }
            }
            else
            {
                bool credit = false;
                if (order != null)
                    credit = order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return;
                if (!order.ConvertedInvoice)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, credit ? "FINAL CREDIT INVOICE" : "FINAL INVOICE"));
                    startY += font36Separation;
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                startY += font36Separation;
            }

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));
            startY += 36;
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));

            if (Config.ExtraSpaceForSignature > 0)
                startY += Config.ExtraSpaceForSignature * font36Separation;

            // add the signature
            lines.AddRange(GetSignatureSection(ref startY, order, asPreOrder));

            var discount = order.CalculateDiscount();
            var orderSales = order.CalculateItemCost();

            if (discount == orderSales && !string.IsNullOrEmpty(Config.Discount100PercentPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomDiscountTextSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }


            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));


            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                DateTime st = DateTime.Now;
                string s = sb.ToString();

                if (Config.SendZplOrder)
                {
                    var found = PrintedOrderZPL.PrintedOrders.FirstOrDefault(x => x.UniqueId == order.UniqueId);
                    if (found != null)
                    {
                        found.ZPLString = s;
                        found.Save();
                    }
                    else
                    {
                        var newZPl = new PrintedOrderZPL(order.UniqueId, s);
                        newZPl.Save();
                    }
                }

                PrintIt(s);

                // Logger.CreateLog("PrintIt(s) took: " + DateTime.Now.Subtract(st).TotalSeconds);
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
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

        protected string[] ClientAddress(Client client, bool shipTo = true)
        {
            var parts = (shipTo ? client.ShipToAddress : client.BillToAddress).Split(new char[] { '|' });
            if (parts.Length == 5)
            {
                parts[2] = parts[2].Trim() + ", " + parts[3].Trim() + " " + parts[4].Trim();
                if (parts[1].Trim().Length == 0)
                {
                    var newParts = new string[2];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[2].Trim();
                    return newParts;
                }
                else
                {
                    var newParts = new string[3];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[1].Trim();
                    newParts[2] = parts[2].Trim();
                    return newParts;
                }
            }
            if (parts.Length == 4)
            {
                parts[2] = parts[2].Trim() + ", " + parts[3].Trim();
                if (parts[1].Trim().Length == 0)
                {
                    var newParts = new string[2];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[2].Trim();
                    return newParts;
                }
                else
                {
                    var newParts = new string[3];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[1].Trim();
                    newParts[2] = parts[2].Trim();
                    return newParts;
                }
            }
            return parts;
        }

        void PrintPaymentInfo(ref int startIndex, List<string> list, Order order)
        {
            if (InvoicePayment.List != null)
            {
                var payment = InvoicePayment.List.FirstOrDefault(x => x.OrderId == order.UniqueId);
                if (payment != null)
                {
                    StringBuilder sb = new StringBuilder();
                    if (payment.Components.Sum(x => x.Amount) == order.OrderTotalCost())
                        sb.Append("Paid In Full");
                    else
                        sb.Append("Paid " + payment.Components.Sum(x => x.Amount).ToCustomString());
                    if (payment.PaymentMethods().Contains(InvoicePaymentMethod.Cash.ToString()))
                    {
                        sb.Append(" - Cash");
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startIndex, sb.ToString()));
                        startIndex += font36Separation;
                    }
                    else
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startIndex, sb.ToString()));
                        startIndex += font36Separation;
                        sb.Clear();
                        sb.Append("Check #" + payment.CheckNumbers());
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startIndex, sb.ToString()));
                        startIndex += font36Separation;
                    }
                    /*
					if (payment.AmountCollected != Order.OrderTotalCost ())
						list.Add (String.Format (CultureInfo.InvariantCulture, linesTemplates [OrderPaid], startIndex, ": " + payment.AmountCollected.ToString ("C")));
					else
						list.Add (String.Format (CultureInfo.InvariantCulture, linesTemplates [OrderPaid], startIndex, "IN FULL"));
						*/
                }
            }
        }

        static string ConvertProdName(string name)
        {
            string result = string.Empty;

            for (int i = 0; i < name.Length; i++)
                result += GetCharacter(name[i]);

            return result;
        }

        static string GetCharacter(char s)
        {
            switch (s)
            {
                case (char)225:
                    return "_c3_a1";
                case (char)233:
                    return "_c3_a9";
                case (char)237:
                    return "_c3_ad";
                case (char)243:
                    return "_c3_b3";
                case (char)250:
                    return "_c3_ba";
                default:
                    return s.ToString();
            }
        }

        public static IList<string> SplitProductName(string productName, int firstLine, int otherLines)
        {
            //productName = ConvertProdName(productName);

            string[] parts = productName.Split(new char[] { ' ' });
            List<string> retList = new List<string>();
            int currentSize = 0;
            StringBuilder sb = new StringBuilder(otherLines * 2);
            int size = firstLine;
            bool isFirstLine = true;
            foreach (string part in parts)
                if ((currentSize + part.Length < size) || currentSize == 0)
                {
                    if (currentSize != 0)
                    {
                        sb.Append(" ");
                        sb.Append(part);
                        currentSize++;
                    }
                    else
                        sb.Append(part);
                    currentSize += part.Length;
                }
                else
                {
                    retList.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                    sb.Append(part);
                    currentSize = part.Length;
                    if (isFirstLine)
                    {
                        size = otherLines;
                        isFirstLine = false;
                    }

                }
            if (currentSize > 0)
                retList.Add(sb.ToString());


            return retList;
        }

        protected virtual IEnumerable<string> GetFooterRows(ref int startIndex, bool asPreOrder, string CompanyName = null)
        {
            List<string> list = new List<string>();
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            startIndex += 12;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.ExtraInfoBottomPrint))
            {
                var text = Config.ExtraInfoBottomPrint;

                if (!string.IsNullOrEmpty(CompanyName) && (CompanyName.ToLower().Contains("el chilar") || CompanyName.ToLower().Contains("lisy corp") || CompanyName.ToLower().Contains("el puro sabor")))
                {
                    text = text.Replace("[COMPANY]", CompanyName);

                    startIndex += font18Separation;
                    foreach (var line in GetBottomTextSplitText(text))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                        startIndex += font18Separation;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                startIndex += font18Separation;
                foreach (var line in GetBottomTextSplitText())
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
                }
            }

            return list;
        }

        protected virtual IEnumerable<string> GetCompanyRows(ref int startIndex, string companyName = null)
        {
            try
            {
                CompanyInfo company = null;

                if (!string.IsNullOrEmpty(companyName))
                {
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName.ToLowerInvariant() == companyName.ToLowerInvariant());
                }
                if (company == null)
                {
                    if (CompanyInfo.Companies.Count == 0)
                        return new List<string>();
                    if (CompanyInfo.SelectedCompany == null)
                        CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                    company = CompanyInfo.SelectedCompany;
                }
                List<string> list = new List<string>();

                if (!string.IsNullOrEmpty(company.CompanyName))
                {
                    foreach (string part in CompanyNameSplit(company.CompanyName))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], part, startIndex));
                        startIndex += font36Separation;
                    }
                }

                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr1], company.CompanyAddress1, startIndex));
                startIndex += font18Separation;
                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr2], company.CompanyAddress2, startIndex));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Phone: " + company.CompanyPhone, startIndex));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Fax: " + company.CompanyFax, startIndex));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Email: " + company.CompanyEmail, startIndex));
                    startIndex += font18Separation;
                }
                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected virtual IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (CompanyInfo.SelectedCompany == null)
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                company = CompanyInfo.SelectedCompany;

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], part, startIndex));
                    startIndex += font36Separation;
                }
                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr1], company.CompanyAddress1, startIndex));
                startIndex += font18Separation;
                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr2], company.CompanyAddress2, startIndex));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Phone: " + company.CompanyPhone, startIndex));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Fax: " + company.CompanyFax, startIndex));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Email: " + company.CompanyEmail, startIndex));
                    startIndex += font18Separation;
                }
                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected virtual IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
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
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            // payment.Components.Sum(x => x.Amount) : 0;
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

                    float qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

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
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
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
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalReturn += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    returnBalance += x * -1;
                    if (returns[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }

            string s;
            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }
            if (!Config.HideTotalOrder && printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s = "  NET QTY:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s = "     SALES:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = salesBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s = "   CREDITS:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = creditBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s = "   RETURNS:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = returnBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                s = "NET AMOUNT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = (salesBalance + creditBalance + returnBalance).ToCustomString();
                s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if ((order.Client.UseDiscount || order.Client.UseDiscountPerLine || order.IsDelivery) && !Config.HideDiscountTotalPrint)
                {
                    if (Config.ShowDiscountIfApplied)
                    {
                        if (order.CalculateDiscount() != 0)
                        {
                            s = "DISCOUNT:";
                            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                            s1 = Math.Abs(discount).ToCustomString();
                            s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                            startY += font36Separation;
                        }
                    }
                    else
                    {
                        s = "DISCOUNT:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = Math.Abs(discount).ToCustomString();
                        s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                        startY += font36Separation;
                    }
                }

                //double tax = Math.Round(taxableAmount * order.TaxRate, Config.Round);
                double tax = Math.Round(order.CalculateTax(), Config.Round);

                if (tax > 0 && !Config.HideTaxesTotalPrint)
                {
                    s = Config.PrintTaxLabel;
                    if (Config.PrintTaxLabel.Length < 11)
                        s = new string(' ', 11 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = tax.ToCustomString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                // right justified
                s = "TOTAL DUE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = s4.ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s = "TOTAL PAYMENT:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = paid.ToCustomString();
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;

                    s = "CURRENT BALANCE:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = (s4 - paid).ToCustomString();
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, " Discount Comment: " + order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                startY += font18Separation;
                var clines = OrderCommentsSplit(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string prefix = string.Empty;
                    if (i == 0)
                        prefix = "Comments: ";
                    else
                        prefix = "          ";
                    list.Add(string.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, prefix + clines[i]));
                    startY += font18Separation;
                }

            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                var PaymentComment = "^FO40,{0}^ADN,18,10^FDPayment Comments: {1}^FS";
                var PaymentComment1 = "^FO40,{0}^ADN,18,10^FD                  {1}^FS";

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            return list;
        }

        protected virtual IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetDetailsRowsInOneDocForAllowance(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();

            list.AddRange(GetDetailsRowsInOneDocTableHeader(ref startY));

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            // anderson crap
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

        protected virtual IEnumerable<string> GetDetailsRowsInOneDocTableHeader(ref int startY)
        {
            List<string> list = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeader];

            if (Config.HidePriceInPrintedLine)
                formatString = formatString.Replace("PRICE", "");

            if (Config.HideTotalInPrintedLine)
                formatString = formatString.Replace("TOTAL", "");

            list.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return list;
        }

        protected virtual IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

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
                var productSlices = GetDetailsRowsSplitProductName1(name);

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

                            //d += double.Parse(Math.Round(_.Price * factor * qty, 4).ToCustomString(), NumberStyles.Currency);
                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }
                        // anderson crap

                        double price = detail.Price * factor;

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, pName, qtyAsString, totalAsString, priceAsString));
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

                // anderson crap
                // the retail price
                var extraProperties = order.Client.ExtraProperties;
                if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                {
                    var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                    if (retailPrice != null)
                    {
                        string retPriceString = "Retail price                                   " + Convert.ToDouble(retailPrice.Item2).ToCustomString();
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                var upc = detail.Product.Upc;

                if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(detail.Product.Sku.Trim()))
                    upc = detail.Product.Sku;

                if (upc.Trim().Length > 0 & Config.PrintUPC)
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
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            startIndex += font18Separation / 2;

                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(upc)));
                            startIndex += font36Separation * 2;
                        }
                }

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (uomMap.Count > 0)
                uomMap.Add("Units:", totalQtyNoUoM);
            else
            {
                uomMap.Add("Totals:", totalQtyNoUoM);

                if (uomMap.Keys.Count == 1 && totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION")
                    uomMap.Add("Units:", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();

            if (!Config.HideTotalOrder && printTotal)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    var balanceText = balance.ToCustomString();
                    if (offset > 0)
                        balanceText = string.Empty;

                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, uomMap[key], balanceText));
                    startIndex += font18Separation;
                    offset++;
                }
            }

            return list;
        }

        protected virtual IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool preOrder, Order order, Client client, string invoiceId, IList<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = "Invoice";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Bill";
                    printExtraDocName = true;
                }
            }
            if (order.AsPresale)
            {
                docName = "Sales Order";
                printExtraDocName = false;
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credit";
                printExtraDocName = true;
            }

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = docName + ": " + order.PrintedOrderId;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle1], startY, s1, string.Empty));
            startY += font36Separation;

            if (order.ConvertedInvoice)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, "COPY"));
                startY += font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Created on:" + order.Date.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
                string extra = string.Empty;
                if (order.DueDate < DateTime.Today)
                    extra = " OVERDUE";
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Due on:    " + order.DueDate.ToString(Config.OrderDatePrintFormat) + extra));
                startY += font36Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
            if (salesman != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Created By: ", salesman.Name));
                startY += font18Separation;
            }


            if (Config.UseClientClassAsCompanyName)
                lines.AddRange(GetCompanyRows(ref startY, order));
            else
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY, order.CompanyName));

            startY += font18Separation; //an extra line

            var clientName = order.Client.ClientName;
            //chilar 
            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var chilarClientNameCode = @"^\d+-\d+-"; //code before ClientName
            if (custno != null)
            {
                var match = Regex.Match(clientName, chilarClientNameCode);

                if (match.Success)
                {
                    clientName = clientName.Substring(match.Length);
                }
            }

            foreach (var clientSplit in GetClientNameSplit(clientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }


            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Bill To: {0}", startY);

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "         {0}", startY);
                }

                startY += font18Separation;
                addrFormat1 = string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Ship To: {0}", startY);

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "         {0}", startY);
                }
            }
            else
            {
                foreach (string s in ClientAddress(client))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s.Trim(), startY));
                    startY += font18Separation;
                }
            }

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "License Number: " + client.LicenceNumber, startY));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Vendor Number: " + client.VendorNumber, startY));
                startY += font18Separation;
            }

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Terms: " + term, startY));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = order.Client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Account Balance: " + balance, startY));
                startY += font18Separation;
            }

            startY += font36Separation;

            if (Config.PrintInvoiceNumberDown)
                if (printExtraDocName)
                {
                    string invoiceIdTemp = string.Empty;
                    if (Config.PrintInvoiceNumberDown)
                        invoiceIdTemp = invoiceId;

                    if (order.AsPresale && order.OrderType == OrderType.Order)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, invoiceId, docName));

                    else if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, invoiceId, docName));

                    else
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditHeaderTitle2], startY, invoiceId));
                    startY += font36Separation + font18Separation;
                }

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle25], startY, order.PONumber));
                startY += font36Separation;
            }
            else if (Config.AutoGeneratePO)
            {
                order.PONumber = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle25], startY, order.PONumber));
                startY += font36Separation;
            }

            if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
            {
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }
            return lines;
        }

        protected virtual IEnumerable<string> GetPaymentLines(ref int startY, IList<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();

            if (payments.Count == 1)
            {
                if (paidInFull)
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid in Full Cash"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid in Full"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Check #:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid in Full"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Money Order #:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid in Full Credit Card"));
                            startY += font36Separation;
                            break;
                    }
                }
                else
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid " + payments[0].Amount.ToCustomString() + "  Cash"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid  " + payments[0].Amount.ToCustomString()));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Check " + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid  " + payments[0].Amount.ToCustomString()));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Money Order " + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, "Paid " + payments[0].Amount.ToCustomString() + "  Credit Card"));
                            startY += font36Separation;
                            break;
                    }
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (paidInFull)
                    sb.Append("Paid In Full");
                else
                    sb.Append("Paid " + payments.Sum(x => x.Amount).ToCustomString());
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaid], startY, sb.ToString()));
                startY += font36Separation;
            }

            return lines;
        }

        public virtual string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            string signatureAsString;
            signatureAsString = order.ConvertSignatureToBitmap();
            using SKBitmap signature = SKBitmap.Decode(signatureAsString);

            var converter = new BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature);
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width / 32) * 32) / 8;
            int height = signature.Height / 32 * 32;
            var bitmapDataLength = rawBytes.Length;

            string ZPLImageDataString = BitConverter.ToString(rawBytes);
            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

            string label = "^FO30," + startIndex.ToString() + "^GFA, " +
                           bitmapDataLength.ToString() + "," +
                           bitmapDataLength.ToString() + "," +
                           widthInBytes.ToString() + "," +
                           ZPLImageDataString;
            startIndex += height;
            return label;
        }

        protected virtual IEnumerable<string> GetSignatureSection(ref int startY, Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            if (order.ConvertedInvoice)
            {
                if (!string.IsNullOrEmpty(order.InvoiceSignature))
                {
                    string label = "^FO30," + startY.ToString() + "^GFA, " +
                                       order.InvoiceSignatureSize.ToString() + "," +
                                       order.InvoiceSignatureSize.ToString() + "," +
                                       order.InvoiceSignatureWidth.ToString() + "," +
                                       order.InvoiceSignature;



                    lines.Add(label);
                    startY += order.InvoiceSignatureHeight;
                    startY += 10;



                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                    startY += font18Separation;



                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                    startY += font36Separation;
                }



                return lines;
            }

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

                if (!string.IsNullOrEmpty(Config.ExtraInfoBottomPrint))
                {
                    var text = Config.ExtraInfoBottomPrint;

                    if (!string.IsNullOrEmpty(order.CompanyName) && (order.CompanyName.ToLower().Contains("el chilar") || order.CompanyName.ToLower().Contains("lisy corp") || order.CompanyName.ToLower().Contains("el puro sabor")))
                    {
                        text = text.Replace("[COMPANY]", order.CompanyName);

                        startY += font18Separation;
                        foreach (var line in GetBottomTextSplitText(text))
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                            startY += font18Separation;
                        }
                    }
                }

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
                foreach (string s in GetFooterRows(ref startY, asPreOrder, order.CompanyName))
                    lines.Add(s);

            return lines;
        }

        #endregion

        public override bool PrintVehicleInformation(bool fromEOD, int index= 0, int count = 0, bool isReport = false)
        {
            List<string> lines = new List<string>();

            var vehicleInfo = VehicleInformation.CurrentVehicleInformation;

            if (vehicleInfo == null)
                return false;

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
                startY += Config.CompanyLogoHeight;
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, "Vehicle Information"));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchDate], startY, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            startY += font18Separation;

            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Plate Number:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.PlateNumber));
            startY += font18Separation;
            startY += font18Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Gasoline:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.Gas.ToString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Assistant:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.Assistant));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Miles from Departure:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.MilesFromDeparture > 1 ? vehicleInfo.MilesFromDeparture.ToString() + "Miles" : vehicleInfo.MilesFromDeparture.ToString() + " Mile"));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Tire Condition:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.TireCondition));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Seat Belts:"));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.SeatBelts));
            startY += font18Separation;
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += 18;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }



        #region pick Ticket

        public override bool PrintPickTicket(Order order)
        {

            int startY = 80;

            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Butler Foods"));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Pick Ticket - Not an Invoice"));
            startY += font36Separation;


            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);
            if (salesman == null)
                salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            var truckName = string.Empty;
            var truck = Truck.Trucks.FirstOrDefault(x => x.DriverId == salesman.Id);
            if (truck != null)
                truckName = truck.Name;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketRouteInfo], startY, "Route #: " + truckName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDeliveryDate], startY, "Delivery Date: " + (order.ShipDate != DateTime.MinValue ? order.ShipDate.ToShortDateString() : DateTime.Now.ToShortDateString()), "Date: " + DateTime.Now.ToShortDateString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDriver], startY, "Driver #: " + salesman.Name, "Time: " + DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            var client = order.Client;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = linesTemplates[OrderBillTo];

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderBillTo1];
                }

                startY += font18Separation;
                addrFormat1 = linesTemplates[OrderShipTo];

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderShipTo1];
                }
            }
            else
            {
                foreach (string s in ClientAddress(client))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
                    startY += font18Separation;
                }
            }

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientLicenceNumber], startY, client.LicenceNumber));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderVendorNumber], startY, client.VendorNumber));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "PO#: " + order.PONumber));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "Ticket#: " + order.PrintedOrderId));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            var SE = string.Empty;
            SE = new string('-', WidthForNormalFont - SE.Length) + SE;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;

            List<T1> list = new List<T1>();


            foreach (var detail in order.Details)
            {
                var factor = 1;
                if (detail.IsCredit)
                    factor = -1;

                var found = list.FirstOrDefault(x => x.ProductId == detail.Product.ProductId && x.IsCredit == detail.IsCredit);
                if (found != null)
                {
                    if (detail.UnitOfMeasure != null)
                    {
                        var caseUOM = detail.Product.UnitOfMeasures.FirstOrDefault(x => !x.IsBase);
                        if (caseUOM != null)
                        {
                            var conversion = caseUOM.Conversion;
                            if (conversion > 1 && detail.Qty >= conversion)
                            {
                                int cases = (int)(detail.Qty / conversion);
                                int units = (int)(detail.Qty % conversion);

                                found.Cases += (cases * factor);
                                found.Units += (units * factor);
                            }
                            else
                                found.Units += (detail.Qty * factor);

                        }
                        else
                            found.Units += (detail.Qty * factor);
                    }
                    else
                        found.Units += (detail.Qty * factor);

                }
                else
                {
                    if (detail.UnitOfMeasure != null)
                    {
                        var caseUOM = detail.Product.UnitOfMeasures.FirstOrDefault(x => !x.IsBase);
                        if (caseUOM != null)
                        {
                            var conversion = caseUOM.Conversion;
                            if (conversion > 1 && detail.Qty >= conversion)
                            {
                                int cases = (int)(detail.Qty / conversion);
                                int units = (int)(detail.Qty % conversion);

                                list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = cases * factor, Units = units * factor, IsCredit = detail.IsCredit });
                            }
                            else
                                list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });

                        }
                        else
                            list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });
                    }
                    else
                        list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });
                }
            }

            list = list.OrderBy(x => x.Product.Description).ToList();

            foreach (var l in list)
            {
                string description = l.Product.Description;
                if (description.Length > 30)
                    description = description.Substring(0, 30);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, description, l.Cases, l.Units));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
                startY += font18Separation;
            }

            //totals
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductTotal], startY, list.Sum(x => x.Cases), list.Sum(x => x.Units)));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        public class T1 
        {
            public int ProductId { get; set; }
            public double Cases { get; set; }
            public double Units { get; set; }
            public Product Product { get; set; }

            public bool IsCredit { get; set; }
        }

        #endregion

        #region Print Payment Batch

        public override bool PrintPaymentBatch()
        {
            List<string> lines = new List<string>();

            var deposit = BankDeposit.currentDeposit;

            if (deposit == null)
                return false;

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

            lines.AddRange(GetCompanyRows(ref startY));

            startY += 36;

            if (deposit.PostedDate != DateTime.MinValue)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchDate], startY, deposit.PostedDate.ToShortDateString()));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchSalesman], startY, salesman.Name));
            startY += font18Separation;

            var bank = BankAccount.List.FirstOrDefault(x => x.Id == deposit.bankAccountId);
            if (bank != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchBank], startY, bank.Name));
                startY += font18Separation;
            }

            startY += font18Separation;

            var checks = new List<PaymentComponent>();
            var cash = new List<PaymentComponent>();
            var credit_card = new List<PaymentComponent>();
            var moneyOrder = new List<PaymentComponent>();

            //fill lists
            foreach (var p in deposit.Payments)
            {
                foreach (var c in p.Components)
                {
                    switch (c.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            if (!checks.Contains(c))
                                checks.Add(c);
                            break;
                        case InvoicePaymentMethod.Cash:
                            if (!cash.Contains(c))
                                cash.Add(c);
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            if (!credit_card.Contains(c))
                                credit_card.Add(c);
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            if (!moneyOrder.Contains(c))
                                moneyOrder.Add(c);
                            break;
                    }
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            if (checks.Count > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ChecksTitle], startY));
                startY += font18Separation;
                startY += 15;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableHeader], startY));
                startY += 15;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += 20;
            }

            foreach (var check in checks)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableLine], startY, check.Ref, check.Amount.ToCustomString()));
                startY += font18Separation;
            }

            if (checks.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += 20;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableTotal], startY, checks.Count().ToString(), checks.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CashTotalLine], startY, cash.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditCardTotalLine], startY, credit_card.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MoneyOrderTotalLine], startY, moneyOrder.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchTotal], startY, deposit.TotalAmount.ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchComments], startY, deposit.Comment));
            startY += font18Separation;


            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += 12;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

            startY += font18Separation;

            lines.Add(EndLabel);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));

            var sb = new StringBuilder();
            foreach (string d in lines)
                sb.Append(d);

            try
            {
                string d = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(d);
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

        #endregion

        #region Print Payment

        public override bool PrintPayment(InvoicePayment invoicePayment)
        {
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

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTitle1], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTo], startY));
            startY += font36Separation;

            Client client = invoicePayment.Client;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderClientName], client.ClientName, startY));
            startY += font36Separation;
            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderClientAddr], s.Trim(), startY));
                startY += font18Separation;
            }

            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTitle3], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTitle3], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;
            // space
            startY += font18Separation;

            var orders = invoicePayment.Orders();
            var invoices = invoicePayment.Invoices();
            bool moreThanOne = orders.Count > 1 || invoices.Count > 1;
            var sb = new StringBuilder();
            var sb_credits = new StringBuilder();

            if (Config.ShowInvoicesCreditsInPayments)
            {
                if (orders.Count > 0)
                    foreach (var item in orders)
                    {
                        if (item.OrderTotalCost() < 0)
                        {
                            if (sb_credits.Length > 0)
                                sb_credits.Append(", ");
                            sb_credits.Append(item.PrintedOrderId);
                        }
                        else
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(item.PrintedOrderId);
                        }
                    }
                if (sb.Length == 0)
                {
                    foreach (var item in invoices)
                    {
                        if (item.Balance < 0)
                        {
                            if (sb_credits.Length > 0)
                                sb_credits.Append(", ");
                            sb_credits.Append(item.InvoiceNumber);
                        }
                        else
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(item.InvoiceNumber);
                        }
                    }
                }
            }
            else
            {
                if (orders.Count > 0)
                    foreach (var item in orders)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(item.PrintedOrderId);
                    }
                if (sb.Length == 0)
                    foreach (var item in invoices)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(item.InvoiceNumber);
                    }
            }

            string invoiceNumberLine = "Invoice #: ";
            if (moreThanOne)
                invoiceNumberLine = "Invoices #: ";
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTitle2], startY, invoiceNumberLine + sb.ToString()));
            startY += font18Separation;

            if (!string.IsNullOrEmpty(sb_credits.ToString()))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTitle2], startY, "Credits #: " + sb_credits.ToString()));
                startY += font18Separation;
            }

            double balance;
            if (orders.Count > 0)
                balance = orders.Sum(x => x.OrderTotalCost());
            else
            {
                balance = invoices.Sum(x => x.Balance);

                foreach (var idAsString in invoicePayment.InvoicesId.Split(new char[] { ',' }))
                {
                    int id = 0;
                    Invoice invioce = null;
                    if (Config.SavePaymentsByInvoiceNumber)
                        invioce = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceNumber == idAsString);
                    else
                    {
                        id = Convert.ToInt32(idAsString);
                        invioce = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceId == id);
                    }
                    var paymentsForInvoice = InvoicePayment.List.Where(x => x.InvoicesId.Contains(idAsString));
                    double alreadyPaid = 0;
                    foreach (var p in paymentsForInvoice)
                    {
                        if (p.Id == invoicePayment.Id)
                            continue;

                        foreach (var i in p.Invoices())
                        {
                            bool matches = Config.SavePaymentsByInvoiceNumber ? i.InvoiceNumber == idAsString : i.InvoiceId == id;

                            if (matches)
                            {
                                foreach (var component in p.Components)
                                {
                                    if (invioce.Balance == 0)
                                        continue;

                                    double usedInThisInvoice = component.Amount;

                                    if (invioce.Balance < 0)
                                        usedInThisInvoice = invioce.Balance;
                                    else
                                    {
                                        if (component.Amount > invioce.Balance)
                                            usedInThisInvoice = invioce.Balance;
                                    }

                                    alreadyPaid += usedInThisInvoice;
                                }
                            }
                        }
                    }

                    balance -= alreadyPaid;
                }
            }

            if (moreThanOne)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "Invoices Total: " + balance.ToCustomString()));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "Invoice Total: " + balance.ToCustomString()));
            startY += font18Separation;

            if (invoicePayment.Components.Sum(x => x.Amount) == balance)
            {
                string type = string.Empty;
                if (invoicePayment.Components.Count == 1)
                    type = invoicePayment.Components[0].PaymentMethod.ToString().Replace("_", " ");
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "Paid in Full " + type));
                startY += font36Separation;
            }

            foreach (var component in invoicePayment.Components)
            {
                var pm = component.PaymentMethod.ToString().Replace("_", " ");
                if (pm.Length < 11)
                    pm = new string(' ', 11 - pm.Length) + pm;
                string s = string.Format("Method: {0} Amount: {1}", pm, component.Amount.ToCustomString());
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, s));
                startY += font18Separation;
                if (!string.IsNullOrEmpty(component.Ref))
                {

                    string refName = "Ref: {0}";
                    switch (component.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            refName = "Check: {0}";
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            refName = "Money Order: {0}";
                            break;
                        case InvoicePaymentMethod.Transfer:
                            refName = "Transfer: {0}";
                            break;
                        case InvoicePaymentMethod.Zelle_Transfer:
                            refName = "Zelle Transfer: {0}";
                            break;
                    }
                    s = string.Format(refName, component.Ref);
                    if (!string.IsNullOrEmpty(component.Comments))
                        s = s + " Comments: " + component.Comments;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, s));
                    startY += font18Separation;
                }
                else
                {
                    if (!string.IsNullOrEmpty(component.Comments))
                    {
                        var temp_comments = "Comments: " + component.Comments;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, temp_comments));
                        startY += font18Separation;
                    }
                }
                startY += font18Separation / 2;
            }

            var open = (balance - invoicePayment.Components.Sum(x => x.Amount));
            if (open > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], "Total paid: " + invoicePayment.Components.Sum(x => x.Amount).ToCustomString(), startY));
                startY += font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], "Pending:    " + open.ToCustomString(), startY));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], "Total paid: " + invoicePayment.Components.Sum(x => x.Amount).ToCustomString(), startY));
                startY += font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], "Pending:    " + open.ToCustomString(), startY));
                startY += font36Separation;
            }

            //space
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                string s = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s);
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

        #endregion


        #region Print Consignment

        public override bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false)
        {
            bool countedResult = true;
            bool updatedResult = true;
            if (printCounting)
                foreach (var detail in order.Details)
                {
                    if (detail.ConsignmentCounted)
                    {
                        if (Config.UseBattery)
                        {
                            var printer = new BatteryPrinter();
                            countedResult = printer.PrintBatteryConsignmentInvoice(order, asPreOrder);
                        }
                        else
                            countedResult = PrintConsignmentInvoice(order, asPreOrder);
                        break;
                    }
                }
            if (printcontract)
            {
                foreach (var detail in order.Details)
                {
                    var updated = detail.ConsignmentUpdated;
                    if (Config.UseFullConsignment && updated)
                        updated = detail.ConsignmentOld != detail.ConsignmentNew || detail.Price != detail.ConsignmentNewPrice;

                    if (allways || updated)
                    {
                        updatedResult = PrintConsignmentContract(order, asPreOrder);
                        break;
                    }
                }
            }
            return countedResult && updatedResult;
        }

        protected bool PrintConsignmentContract(Order order, bool asPreOrder)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            string title = "Consignment Contract";

            lines.AddRange(GetConsignmentHeaderInfoLines(ref startY, order, title));
            startY += font18Separation;

            if (!asPreOrder)
            {
                if (Config.PrintCopy)
                {
                    string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                    startY += font36Separation;
                }
            }

            startY += font18Separation;

            float totalQty = 0;

            lines.AddRange(GetConsignmentDetailsRows(ref startY, ref totalQty, order, false));

            startY += font18Separation;

            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderTitle1], startY, "THIS IS NOT YOUR INVOICE"));
            startY += font36Separation;
            startY += font36Separation;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order, false));
            else
                lines.AddRange(GetConsignmentFooterRows(ref startY, asPreOrder, false));

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

        private bool PrintConsignmentInvoice(Order order, bool asPreOrder)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            string title = "Invoice: " + order.PrintedOrderId;

            lines.AddRange(GetConsignmentHeaderInfoLines(ref startY, order, title));

            if (asPreOrder)
            {
                string docName = !Config.FakePreOrder ? "NOT AN INVOICE" : "";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
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

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, order.PrintedOrderId, "Invoice"));
            startY += font36Separation;

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var paidInFull = payments != null && payments.Sum(x => x.Amount) == order.OrderTotalCost();
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            startY += font36Separation;

            float totalQty = 0;

            lines.AddRange(GetConsignmentDetailsRows(ref startY, ref totalQty, order, true));

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsTotalLine], startY, "Totals:",
                    totalQty));

            startY += font36Separation;
            startY += font18Separation;

            string s;

            s = "     SALES:";
            int sspace = WidthForBoldFont - s.Length - SpaceForOrderFooter;
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            var s1 = order.CalculateItemCost().ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            double tax = order.CalculateTax();
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
                s1 = order.CalculateDiscount().ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.DiscountComment))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, " C: " + order.DiscountComment));
                startY += font18Separation;
            }
            // right justified
            s = "TOTAL DUE:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = order.OrderTotalCost().ToCustomString();
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
            startY += font36Separation;

            if (!Config.RemovePayBalFomInvoice)
            {
                double paid = 0;
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment != null)
                {
                    var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                    paid = parts.Sum(x => x.Amount);
                }
                s = "TOTAL PAYMENT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = paid.ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                s = "CURRENT BALANCE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = (order.OrderTotalCost() - paid).ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;
            }

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order));
            else
                lines.AddRange(GetFooterRows(ref startY, asPreOrder));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string l in lines)
                sb.Append(l);

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

        protected IEnumerable<string> GetConsignmentHeaderInfoLines(ref int startY, Order order, string title)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderTitle1], startY, title));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
            if (salesman != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Created By: ", salesman.Name));
                startY += font18Separation;
            }

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }
            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s1.Trim(), startY));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = order.Client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Account Balance: " + balance, startY));
                startY += font18Separation;
            }

            return lines;
        }

        protected IEnumerable<string> GetConsignmentSignatureLines(ref int startY, Order order, bool counting = true)
        {
            List<string> lines = new List<string>();

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

            if (!string.IsNullOrEmpty(Config.ConsignmentContractText) && !counting)
            {
                startY += font18Separation;
                foreach (var line in GetBottomTextSplitText(Config.ConsignmentContractText))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            else if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomTextSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            var discount = order.CalculateDiscount();
            var orderSales = order.CalculateItemCost();

            if (discount == orderSales && !string.IsNullOrEmpty(Config.Discount100PercentPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomDiscountTextSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            return lines;
        }

        protected IEnumerable<string> GetConsignmentFooterRows(ref int startIndex, bool asPreOrder, bool counting = true)
        {
            List<string> list = new List<string>();
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            startIndex += 12;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.ConsignmentContractText) && !counting)
            {
                startIndex += font18Separation;
                foreach (var line in GetBottomTextSplitText(Config.ConsignmentContractText))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
                }
            }

            else if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                startIndex += font18Separation;
                foreach (var line in GetBottomTextSplitText())
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
                }
            }

            return list;
        }

        #endregion


        #region Print Transfer

        public override bool PrintTransferOnOff(IEnumerable<InventoryLine> sortedList, bool isOn, bool isFinal, string comment = "", string siteName = "")
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderTitle1], startY, isOn ? "On" : "Off"));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;
            float numberOfBoxes = 0;
            double value = 0.0;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffNotFinalLine], startY));
                startY += font36Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsHeader1], startY, "UoM", "Transferred"));
            startY += font18Separation;

            foreach (var p in sortedList)
            {
                double price = p.Product.PriceLevel0;
                string uomLabel = string.Empty;
                var real = p.Real;

                if (p.UoM != null)
                {
                    price *= p.UoM.Conversion;
                    uomLabel = p.UoM.Name;
                    real *= p.UoM.Conversion;
                }

                int productLineOffset = 0;
                foreach (string pName in GetTransferOnOffSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsLine], startY,
                            pName, uomLabel, Math.Round(p.Real, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsLine], startY,
                            pName, "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrdersCreatedReportDetailUPCLine], startY, "List Price: " + price.ToCustomString()));
                startY += font18Separation;

                if (Config.PrintUPC)
                    if (p.Product.Upc.Trim().Length > 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrdersCreatedReportDetailUPCLine], startY, "UPC: " + p.Product.Upc));
                        startY += font18Separation;
                    }

                numberOfBoxes += Convert.ToSingle(real);
                value += p.Real * price;
                startY += font18Separation + orderDetailSeparation;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
            startY += font36Separation;

            if (!Config.Wstco)
            {
                s = "Transfer Value:" + value.ToCustomString();
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, WidthForBoldFont - s.Length) + s;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s));
                startY += font36Separation;
            }
            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffNotFinalLine], startY));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(comment))
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Comment: ", comment));
                startY += font36Separation;
            }

            //space
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
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

        #endregion


        #region Print Load

        public override bool PrintOrderLoad(bool isFinal)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderTitle1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (LoadOrder.Date.Year > DateTime.MinValue.Year)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderPrintedDate], startY, LoadOrder.Date));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;
            float dumpBoxes = 0;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinalLine], startY));
                startY += font36Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsHeader1], startY));
            startY += font18Separation;

            foreach (var p in SortDetails.SortedDetails(LoadOrder.List))
            {
                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                        lines.Add(GetLoadOrderTableLineFixed(LoadOrderDetailsLine, startY, pName, (p.UoM != null ? p.UoM.Name : string.Empty), Math.Round(p.Qty, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    else
                        lines.Add(GetLoadOrderTableLineFixed(LoadOrderDetailsLine, startY, pName, "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }
                //startY += font18Separation;
                dumpBoxes += Convert.ToSingle(p.Qty);
                startY += font18Separation;
            }

            if (SortDetails.SortedDetails(LoadOrder.List).All(x => x.UoM == null))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsFooter], startY, dumpBoxes.ToString(CultureInfo.CurrentCulture)));
                startY += font18Separation;
            }

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinalLine], startY));
                startY += font36Separation;
            }


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

        protected virtual string GetLoadOrderTableLineFixed(string format, int pos, string value1, string value2, string value3)
        {
            value2 = value2.Substring(0, value2.Length > 5 ? 5 : value2.Length);
            value3 = value3.Substring(0, value3.Length > 6 ? 6 : value3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, value1, value2, value3);
        }
        #endregion


        #region Print Route Return

        public override bool PrintRouteReturn(IEnumerable<RouteReturnLine> sortedList, bool isFinal)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderTitle1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;
            float returnBoxes = 0;
            float dumpBoxes = 0;
            float damagedBoxes = 0;

            if (!isFinal)
            {
                //para dividir el label cuando se imprime con printer de 3
                string notFinal = "NOT A FINAL Route Return";
                foreach (var item in GetLabelNotAFinalRouteReturn(notFinal))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY, item));
                    startY += font36Separation;
                }
                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLine], startY));
                //startY += font36Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsHeader1], startY));
            startY += font18Separation;

            foreach (var p in sortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsLine], startY, pName, Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(p.Dumps, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsLine], startY, pName, "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                //startY += font18Separation;
                returnBoxes += Convert.ToSingle(p.Unload);
                dumpBoxes += Convert.ToSingle(p.Dumps);
                damagedBoxes += Convert.ToSingle(p.DamagedInTruck);
                //startY += font18Separation + orderDetailSeparation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsFooter], startY, Math.Round(returnBoxes, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(dumpBoxes, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(damagedBoxes, Config.Round).ToString(CultureInfo.CurrentCulture)));
            startY += font18Separation;

            if (!isFinal)
            {
                //para dividir el label cuando se imprime con printer de 3
                string notFinal = "NOT A FINAL Route Return";
                foreach (var item in GetLabelNotAFinalRouteReturn(notFinal))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY, item));
                    startY += font36Separation;
                }
                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLine], startY));
                //startY += font36Separation;
            }

            //space
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
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

        #endregion


        #region Print Inventory Check methods

        public override bool PrintInventoryCheck(IEnumerable<InventoryLine> SortedList)
        {
            int startY = 80;

            List<string> lines = new List<string>();
            foreach (string s in GetInventoryCheckHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            foreach (string s in GetInventoryCheckDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }
            foreach (string s in GetInventoryCheckFooterRows(ref startY))
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
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetInventoryCheckHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckHeaderTitle], startIndex, DateTime.Now.ToString()));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Route #: ", Config.RouteName));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Driver Name: ", Config.VendorName));
            startIndex += font36Separation + 10;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetInventoryCheckDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxesReal = 0;
            float numberOfBoxesExpected = 0;
            foreach (var l in SortedList)
            {
                var p = l.Product;
                int productLineOffset = 0;
                foreach (string pName in GetInventoryCheckDetailsRowsSplitProductName1(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsLine], startIndex, pName, p.CurrentInventory.ToString(CultureInfo.CurrentCulture), l.Real.ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsLine], startIndex, pName, "", ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                numberOfBoxesReal += Convert.ToSingle(l.Real);
                numberOfBoxesExpected += Convert.ToSingle(p.CurrentInventory);

                startIndex += font18Separation + orderDetailSeparation;
            }
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsFooter], startIndex, numberOfBoxesExpected.ToString(CultureInfo.CurrentCulture), numberOfBoxesReal.ToString(CultureInfo.CurrentCulture)));
            startIndex += font36Separation;
            return list;
        }

        protected virtual IEnumerable<string> GetInventoryCheckFooterRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            startIndex += 12;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterCheckerSignatureText], startIndex, " " + Config.VendorName));
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            startIndex += 12;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterCheckerSignatureText], startIndex, " Checker"));
            startIndex += 100;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));
            return list;
        }

        #endregion


        #region Print Inventory methods

        public override bool PrintInventory(IEnumerable<Product> SortedList)
        {
            int startY = 80;

            List<string> lines = new List<string>();
            foreach (string s in GetInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            foreach (string s in GetInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }
            foreach (string s in GetFooterRows(ref startY, false))
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
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetInventoryHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryHeaderTitle], startIndex, DateTime.Now.ToString()));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Route #: ", Config.RouteName));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Driver Name: ", Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetInventoryDetailsRows(ref int startIndex, IEnumerable<Product> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxes = 0;
            double value = 0;
            foreach (Product p in SortedList)
            {
                float startInv = p.BeginigInventory;

                int productLineOffset = 0;
                foreach (string pName in GetInventoryDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName,
                            Math.Round(startInv, 2).ToString(CultureInfo.CurrentCulture), Math.Round(p.CurrentInventory, 2).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName, "", ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryPriceLine], startIndex, string.Format(CultureInfo.InvariantCulture, "List Price: {0}  Total: {1}", p.PriceLevel0.ToCustomString(), (p.CurrentInventory * p.PriceLevel0).ToCustomString())));
                startIndex += font18Separation;

                if (Config.PrintUPCInventory)
                {
                    if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(p.Upc)));
                            startIndex += font36Separation;
                        }
                    }
                }

                numberOfBoxes += Convert.ToSingle(p.CurrentInventory);
                value += p.CurrentInventory * p.PriceLevel0;
                startIndex += font18Separation + orderDetailSeparation;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
            startIndex += font36Separation;

            s = "Inv. Value:" + value.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
            startIndex += font36Separation;
            return list;
        }

        #endregion


        #region Print Set Inventory methods

        public override bool PrintSetInventory(IEnumerable<InventoryLine> SortedList)
        {
            int startY = 80;

            List<string> lines = new List<string>();
            foreach (string s in GetSetInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            foreach (string s in GetSetInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }
            foreach (string s in GetFooterRows(ref startY, false))
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
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetSetInventoryHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryHeaderTitle], startIndex, DateTime.Now.ToString()));
            startIndex += font36Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, Config.VendorName));
            startIndex += font36Separation + 10;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetSetInventoryDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxes = 0;
            double value = 0.0;
            foreach (var p in SortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsLine], startIndex, pName, Math.Round(p.Real, 2).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsLine], startIndex, pName, ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryPriceLine], startIndex, string.Format(CultureInfo.InvariantCulture, "List Price: {0}  Total: {1}", p.Product.PriceLevel0.ToCustomString(), (p.Product.CurrentInventory * p.Product.PriceLevel0).ToCustomString())));
                startIndex += font18Separation;
                if (Config.PrintUPCInventory)
                {
                    if (p.Product.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Product.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPC], startIndex, Product.GetFirstUpcOnly(p.Product.Upc)));
                            startIndex += font36Separation;
                        }
                    }
                }
                numberOfBoxes += Convert.ToSingle(p.Real);
                value += p.Real * p.Product.PriceLevel0;
                startIndex += font18Separation + orderDetailSeparation;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
            startIndex += font36Separation;

            s = "Inv. Value:" + value.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
            startIndex += font36Separation;
            return list;
        }

        #endregion


        #region Print Add to Inventory methods

        public override bool PrintAddInventory(IEnumerable<InventoryLine> SortedList, bool final)
        {
            int startY = 80;

            List<string> lines = new List<string>();
            foreach (string s in GetAddInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }
            foreach (string s in GetAddInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }
            foreach (string s in GetFooterRows(ref startY, false))
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
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetAddInventoryHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryHeaderTitle], startIndex));
            startIndex += font36Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryHeaderTitle1], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Route #: ", Config.RouteName));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Driver Name: ", Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetAddInventoryDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsHeader2], startIndex));
            startIndex += font18Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsHeader21], startIndex));
            startIndex += font18Separation;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;
            foreach (var p in SortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetAddInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName,
                                               Math.Round(p.Product.BeginigInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round(p.Product.RequestedLoadInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round((p.Real - p.Product.RequestedLoadInventory), Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round((p.Product.BeginigInventory + p.Real), Config.Round).ToString(CultureInfo.CurrentCulture)
                        ));

                        leftFromYesterday += p.Product.BeginigInventory;
                        requestedInventory += p.Product.RequestedLoadInventory;
                        adjustment += (p.Real - p.Product.RequestedLoadInventory);
                        start += (p.Product.BeginigInventory + p.Real);
                    }
                    else
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName, "", "", "", ""));
                    }

                    productLineOffset++;
                    startIndex += font18Separation;
                }
                //startIndex += font18Separation + orderDetailSeparation;
            }

            //startIndex += font18Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, "Totals:",
                 Math.Round(leftFromYesterday, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));
            startIndex += font18Separation;

            return list;
        }

        #endregion


        #region Order Created Report

        // Invoices/Credits in the system
        public override bool PrintOrdersCreatedReport(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderTitle1], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            if (Config.UseClockInOut)
            {
                #region Deprecated

                //DateTime startOfDay = Config.FirstDayClockIn;
                //TimeSpan tsio = Config.WorkDay;
                //DateTime lastClockOut = Config.DayClockOut;
                //var wholeday = lastClockOut.Subtract(startOfDay);
                //var rested = wholeday.Subtract(tsio);

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), tsio.Hours, tsio.Minutes));
                //startY += font18Separation;

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport2], startY, rested.Hours, rested.Minutes));
                //startY += font18Separation;

                #endregion

                DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                var wholeday = lastClockOut.Subtract(startOfDay);
                var breaks = SalesmanSession.GetTotalBreaks();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), wholeday.Hours, wholeday.Minutes));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport2], startY, breaks.Hours, breaks.Minutes));
                startY += font18Separation;
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsHeader1], startY));
            startY += font18Separation + 5;

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotalTerm = 0;
            double chargeTotalTerm = 0;
            double subtotal = 0;
            double totalTax = 0;

            double paidTotal = 0;
            double chargeTotal = 0;
            double creditTotal = 0;
            double salesTotal = 0;
            double billTotal = 0;

            var payments = GetPaymentsForOrderCreatedReport();

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
            {
                var terms = order.Term.Trim();

                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Return:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                }
            }

            foreach (var b in Batch.List.OrderBy(x => x.ClockedIn))
                foreach (var p in b.Orders())
                {
                    var orderCost = p.OrderTotalCost();

                    string totalCostLine = orderCost.ToCustomString();
                    string subTotalCostLine = totalCostLine;

                    int productLineOffset = 0;
                    foreach (string pName in GetSalesRegRepClientSplitProductName(p.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
                            string status = string.Empty;
                            if (p.OrderType == OrderType.NoService)
                                status = "NS";
                            if (p.Voided)
                                status = "VD";
                            if (p.Reshipped)
                                status = "RF";

                            if (p.OrderType == OrderType.Bill)
                                status = "Bi";

                            double paid = 0;

                            var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);
                            if (payment != null)
                            {
                                double amount = payment.Amount;
                                paid = double.Parse(Math.Round(amount, Config.Round).ToCustomString(), NumberStyles.Currency);
                            }

                            string type = "";

                            if (!p.Reshipped && !p.Voided)
                            {
                                if (paid == 0)
                                    type = "Charge";
                                else if (paid < orderCost)
                                    type = "Partial P.";
                                else
                                    type = "Paid";

                                if (orderCost < 0)
                                {
                                    type = "Credit";
                                    creditTotal += orderCost;
                                }
                                else
                                {
                                    if (p.OrderType != OrderType.Bill)
                                    {
                                        salesTotal += orderCost;

                                        if (paid == 0)
                                            chargeTotal += orderCost;
                                        else
                                        {
                                            paidTotal += paid;
                                            chargeTotal += orderCost - paid;
                                        }
                                    }
                                    else
                                        billTotal += orderCost;
                                }
                            }

                            float qty = 0;
                            foreach (var item in p.Details)
                                if (!item.SkipDetailQty(p))
                                    qty += item.Qty;

                            if (Config.SalesRegReportWithTax)
                                subTotalCostLine = p.CalculateItemCost().ToCustomString();
                            else
                                totalCostLine = string.Empty;


                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY,
                                                    pName,
                                                    status,
                                                    qty,
                                                    p.PrintedOrderId,
                                                    subTotalCostLine,
                                                    type));

                        }
                        else
                        {
                            if (productLineOffset == 1)
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY, pName,
                                       "", "", "", totalCostLine, ""));
                            else
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY, pName,
                                   "", "", "", "", ""));
                        }

                        productLineOffset++;
                        startY += font18Separation;
                    }

                    if (productLineOffset == 1 && !string.IsNullOrEmpty(totalCostLine))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY, string.Empty,
                               "", "", "", totalCostLine, ""));
                        startY += font18Separation;
                    }

                    startY += 10;

                    if (!string.IsNullOrEmpty(p.Term))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow2], startY, "Terms: " + p.Term));
                        startY += font18Separation;
                    }

                    var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);
                    string s = string.Format("Clock In: {0}    Clock Out: {1}     # Copies: {2}", batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies);

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow2], startY, s));
                    startY += font18Separation;

                    if (p.OrderType == OrderType.NoService && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow2], startY, "NS Comment:" + p.Comments));
                        startY += font18Separation;
                    }
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;

                    if (p.Voided)
                        voided++;
                    else if (p.Reshipped)
                        reshipped++;
                    else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                        delivered++;
                    else
                        dsd++;

                    startY += font18Separation;
                }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", "Credit Total:", creditTotal.ToCustomString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", "  Bill Total:", billTotal.ToCustomString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", " Sales Total:", salesTotal.ToCustomString()));
            startY += font18Separation;

            if (Config.SalesRegReportWithTax)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterTotalRow], startY, "Subtotals:", subtotal.ToCustomString()));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterTotalRow], startY, "Tax:", totalTax.ToCustomString()));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterTotalRow], startY, "Totals:", (subtotal + totalTax).ToCustomString()));
                startY += font18Separation;
            }

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Expected Cash Cust: ", cashTotalTerm.ToCustomString(), "Reshipped:   ", reshipped.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Paid Cust:          ", paidTotal.ToCustomString(), "Voided:      ", voided.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Charge Cust:        ", chargeTotal.ToCustomString(), "Delivery:    ", delivered.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "                    ", "", "P&P:         ", dsd.ToString()));

            startY += font36Separation;

            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Total Sales:        ", salesTotal.ToCustomString(), "Time (Hours):", totalTime));
            startY += font18Separation;

            //lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", "Time (Hours):", totalTime));
            //startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
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

        protected List<DataAccess.PaymentSplit> GetPaymentsForOrderCreatedReport()
        {
            List<DataAccess.PaymentSplit> result = new List<DataAccess.PaymentSplit>();

            foreach (var payment in InvoicePayment.List)
                result.AddRange(DataAccess.SplitPayment(payment));

            return result;
        }

        #endregion


        #region Sales Credit Report

        // By products
        public override bool PrintSalesCreditReport()
        {
            // calculation
            Dictionary<int, Pair<Product, double>> sales = new Dictionary<int, Pair<Product, double>>();
            Dictionary<int, Pair<Product, double>> credits = new Dictionary<int, Pair<Product, double>>();
            Dictionary<int, Pair<Product, double>> returns = new Dictionary<int, Pair<Product, double>>();
            foreach (var order in Order.Orders.Where(x => !x.Voided))
                switch (order.OrderType)
                {
                    case OrderType.Credit:
                        foreach (var detail in order.Details)
                        {
                            var dictionary = detail.Damaged ? credits : returns;
                            if (dictionary.ContainsKey(detail.Product.ProductId))
                                dictionary[detail.Product.ProductId].Item2 = dictionary[detail.Product.ProductId].Item2 + detail.Qty;
                            else
                                dictionary.Add(detail.Product.ProductId, new Pair<Product, double>(detail.Product, detail.Qty));
                        }
                        break;
                    case OrderType.Order:
                        foreach (var detail in order.Details)
                            if (sales.ContainsKey(detail.Product.ProductId))
                                sales[detail.Product.ProductId].Item2 = sales[detail.Product.ProductId].Item2 + detail.Qty;
                            else
                                sales.Add(detail.Product.ProductId, new Pair<Product, double>(detail.Product, detail.Qty));
                        break;
                }


            int startY = 80;
            List<string> lines = new List<string>();

            // report header
            startY += 10;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportHeaderTitle], startY, DateTime.Now.ToString()));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesman], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesman], startY, "Driver Name: ", Config.VendorName));
            startY += font36Separation + 10;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line
                                        // The sales section
                                        // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesHeaderTitle], startY));
            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailsHeader1], startY));
            startY += font18Separation;

            // print the lines
            double total = 0;
            foreach (var item in sales.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName1(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                startY += font18Separation;
                total += item.Item2;
            }
            var s = "Qty sold:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesFooter], startY, s));
            startY += font18Separation * 3;

            // The credit section
            // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditHeaderTitle], startY));
            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailsHeader1], startY));
            startY += font18Separation;
            total = 0;
            // print the lines
            foreach (var item in credits.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName2(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                startY += font18Separation;
                total += item.Item2;
            }

            // print the footer
            s = "Qty dumped:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditFooter], startY, s));
            startY += font18Separation * 3;

            // The return section
            // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnHeaderTitle], startY));
            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailsHeader1], startY));
            startY += font18Separation;
            total = 0;
            // print the lines
            foreach (var item in returns.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName2(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                startY += font18Separation;
                total += item.Item2;
            }

            // print the footer
            s = "Qty returned:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnFooter], startY, s));
            startY += font18Separation;

            lines.AddRange(GetInventoryCheckFooterRows(ref startY));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string ss in lines)
                sb.Append(ss);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        #endregion


        #region Received Payments Report

        public override bool PrintReceivedPaymentsReport(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeaderTitle], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportSalesman], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportSalesman], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader1], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader2], startY));
            startY += font18Separation;

            double totalCash = 0;
            double totalCheck = 0;
            double totalcc = 0;
            double totalmo = 0;
            double totaltr = 0;
            double total = 0;

            List<PaymentRow> rows = new List<PaymentRow>();
            var listToUse = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;

            foreach (var pay in listToUse)
            {
                int _index = 0;
                List<string> docNumbers = pay.Invoices().Select(x => x.InvoiceNumber).ToList();
                if (docNumbers.Count == 0)
                    docNumbers = pay.Orders().Select(x => x.PrintedOrderId).ToList();

                var t = pay.Invoices().Sum(x => x.Balance);
                if (t == 0)
                    t = pay.Orders().Sum(x => x.OrderTotalCost());
                while (true)
                {
                    var row = new PaymentRow();
                    if (_index == 0)
                    {
                        row.ClientName = pay.Client.ClientName;

                        int factor = 0;
                        if (pay.Voided)
                            factor = 6;

                        if (row.ClientName.Length > (28 - factor))
                            row.ClientName = row.ClientName.Substring(0, (27 - factor));

                        row.DocAmount = t.ToCustomString();
                    }
                    else
                    {
                        row.ClientName = string.Empty;
                        row.DocAmount = string.Empty;
                    }
                    if (docNumbers.Count > _index)
                        row.DocNumber = docNumbers[_index];
                    else
                        row.DocNumber = string.Empty;
                    if (pay.Components.Count > _index)
                    {
                        if (pay.Voided)
                            row.ClientName += "(Void)";

                        if (pay.Components[_index].PaymentMethod == InvoicePaymentMethod.Cash)
                            totalCash += pay.Components[_index].Amount;
                        else if (pay.Components[_index].PaymentMethod == InvoicePaymentMethod.Check)
                            totalCheck += pay.Components[_index].Amount;
                        else if (pay.Components[_index].PaymentMethod == InvoicePaymentMethod.Credit_Card)
                            totalcc += pay.Components[_index].Amount;
                        else if (pay.Components[_index].PaymentMethod == InvoicePaymentMethod.Money_Order)
                            totalmo += pay.Components[_index].Amount;
                        else
                            totaltr += pay.Components[_index].Amount;

                        total += pay.Components[_index].Amount;

                        row.RefNumber = pay.Components[_index].Ref;
                        var s = pay.Components[_index].Amount.ToCustomString();
                        if (s.Length < 9)
                            s = new string(' ', 9 - s.Length) + s;
                        row.Paid = s;
                        row.PaymentMethod = ReducePaymentMethod(pay.Components[_index].PaymentMethod);
                    }
                    else
                    {
                        row.RefNumber = string.Empty;
                        row.Paid = string.Empty;
                        row.PaymentMethod = string.Empty;
                    }
                    rows.Add(row);

                    _index++;
                    if (docNumbers.Count <= _index && pay.Components.Count <= _index)
                        break;
                }
            }

            foreach (var p in rows)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDetail], startY,
                                p.ClientName,
                                p.DocNumber,
                                p.DocAmount,
                                p.Paid,
                                p.PaymentMethod,
                                        p.RefNumber));
                startY += font18Separation;
            }
            startY += font18Separation;
            if (totalCash > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Cash: ", totalCash.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (totalCheck > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Check: ", totalCheck.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (totalcc > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Credit Card: ", totalcc.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (totalmo > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Money Order: ", totalmo.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (totaltr > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Transfer: ", totaltr.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (total > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Total: ", total.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignaturePaymentText], startY, string.Empty));
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

        protected string ReducePaymentMethod(InvoicePaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case InvoicePaymentMethod.Cash:
                    return "CA";
                case InvoicePaymentMethod.Check:
                    return "CH";
                case InvoicePaymentMethod.Credit_Card:
                    return "CC";
                case InvoicePaymentMethod.Money_Order:
                    return "MO";
                case InvoicePaymentMethod.Transfer:
                    return "TR";        
                case InvoicePaymentMethod.Zelle_Transfer:
                    return "ZE";
            }
            return string.Empty;
        }

        static string ConcatPaymentTypes(InvoicePayment payment)
        {
            List<string> types = new List<string>();
            foreach (var c in payment.Components)
            {
                string st = string.Empty;
                switch (c.PaymentMethod)
                {
                    case InvoicePaymentMethod.Cash:
                        st = "CA";
                        break;
                    case InvoicePaymentMethod.Check:
                        st = "CH";
                        break;
                    case InvoicePaymentMethod.Credit_Card:
                        st = "CC";
                        break;
                    case InvoicePaymentMethod.Money_Order:
                        st = "MO";
                        break;
                }
                if (!types.Contains(st))
                    types.Add(st);
            }
            return string.Join(",", types);
        }

        protected class PaymentRow
        {
            public string ClientName { get; set; }
            public string DocNumber { get; set; }
            public string DocAmount { get; set; }
            public string Paid { get; set; }
            public string PaymentMethod { get; set; }
            public string RefNumber { get; set; }
        }

        #endregion


        #region Shortage Report

        protected virtual void PrintShortageReport(Order order)
        {
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FDKNOWN SHORTAGE REPORT^FS", startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO400,{0}^ADN,18,10^FDDate: {1}^FS", startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTo], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], order.Client.ClientName, startY));
            startY += font36Separation;
            foreach (string s in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s.Trim(), startY));
                startY += font18Separation;
            }
            startY += font36Separation;

            if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, printedId, "Invoice"));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditHeaderTitle2], startY, printedId));
            startY += font36Separation + font18Separation;

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle25], startY, order.PONumber));
                startY += font36Separation;
            }
            // add the details

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS^FO500,{0}^ADN,18,10^FDPO Qty^FS^FO600,{0}^ADN,18,10^FDShort.^FS^FO710,{0}^ADN,18,10^FDDel.^FS", startY));
            startY += font18Separation;
            foreach (var detail in order.Details)
                if (detail.Ordered != 0 && detail.Ordered != detail.Qty)
                {
                    var p = GetDetailsRowsSplitProductName1(detail.Product.Name);
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO500,{0}^ADN,18,10^FD{2}^FS^FS^FO600,{0}^ADN,18,10^FD{3}^FS^FO710,{0}^ADN,18,10^FD{4}^FS",
                                            startY, p[0], detail.Ordered, (detail.Ordered - detail.Qty), detail.Qty));
                    startY += font18Separation;
                }

            foreach (var detail in order.DeletedDetails)
                if (detail.Ordered != 0)
                {
                    var p = GetDetailsRowsSplitProductName1(detail.Product.Name);
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO500,{0}^ADN,18,10^FD{2}^FS^FS^FO600,{0}^ADN,18,10^FD{3}^FS^FO710,{0}^ADN,18,10^FD{4}^FS",
                                            startY, p[0], detail.Ordered, detail.Ordered, 0));
                    startY += font18Separation;
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

                var discount = order.CalculateDiscount();
                var orderSales = order.CalculateItemCost();

                if (discount == orderSales && !string.IsNullOrEmpty(Config.Discount100PercentPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomDiscountTextSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
                foreach (string s in GetFooterRows(ref startY, false))
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
                string s = sb.ToString();//.Replace("^ADN", "^ARN");
                PrintIt(s);
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
            }
        }

        #endregion


        #region Inventory Settlement

        public override bool InventorySettlement(int index, int count)
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

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataAccess.ExtendedSendTheLeftOverInventory();

            foreach (var value in map)
            {
                var product = value.Product;

                if (value.BegInv == 0 && value.LoadOut == 0 && value.Adj == 0 && (value.TransferOn - value.TransferOff) == 0
                    && value.Sales == 0 && value.CreditReturns == 0 && value.CreditDump == 0 && value.DamagedInTruck == 0 && value.Unload == 0
                    && value.EndInventory == 0)
                    continue;

                if (Config.ShortInventorySettlement && string.IsNullOrEmpty(value.OverShort) && value.TransferOn == 0 && value.TransferOff == 0 && value.Adj == 0)
                    continue;

                totalRow.Product = product;
                totalRow.BegInv += product.BeginigInventory;
                totalRow.LoadOut += product.RequestedLoadInventory;
                totalRow.Adj += product.LoadedInventory - product.RequestedLoadInventory;
                totalRow.TransferOn += product.TransferredOnInventory;
                totalRow.TransferOff += product.TransferredOffInventory;
                totalRow.EndInventory += product.CurrentInventory > 0 ? product.CurrentInventory : 0;
                totalRow.Unload += product.UnloadedInventory;
                totalRow.DamagedInTruck += product.DamagedInTruckInventory;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += value.Sales;
                    totalRow.CreditReturns += value.CreditReturns;
                    totalRow.CreditDump += value.CreditDump;
                }
            }

            startY += font18Separation;
            var oldRound = Config.Round;
            Config.Round = 2;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (p.BegInv == 0 && p.LoadOut == 0 && p.Adj == 0 && (p.TransferOn - p.TransferOff) == 0
                    && p.Sales == 0 && p.CreditReturns == 0 && p.CreditDump == 0 && p.DamagedInTruck == 0 && p.Unload == 0
                    && p.EndInventory == 0)
                    continue;

                if (Config.ShortInventorySettlement && string.IsNullOrEmpty(p.OverShort) && p.TransferOn == 0 && p.TransferOff == 0 && p.Adj == 0)
                    continue;

                int productLineOffset = 0;

                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        var newS = string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY,
                                                pName,
                                                Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                p.OverShort);

                        lines.Add(newS);
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, pName, "", "", "", "", "", "", "", "", "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY,
                                                "Totals:",
                                                Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                totalRow.OverShort));

            Config.Round = oldRound;
            startY += font18Separation;
            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
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

        #endregion

        #region Inventory Summary

        public override bool InventorySummary(int index, int count, bool isBase = true)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderTitle1], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataAccess.ExtendedSendTheLeftOverInventory(false, true);

            foreach (var value in map)
            {
                var product = value.Product;

                if (Math.Round(value.EndInventory, Config.Round) == 0)
                    continue;

                float factor = 1;
                if (!isBase)
                {
                    var defaultUom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                        factor = defaultUom.Conversion;
                }

                totalRow.Product = product;
                totalRow.BegInv += product.BeginigInventory / factor;
                totalRow.LoadOut += product.RequestedLoadInventory / factor;
                totalRow.Adj += (product.LoadedInventory - product.RequestedLoadInventory) / factor;
                totalRow.TransferOn += product.TransferredOnInventory / factor;
                totalRow.TransferOff += product.TransferredOffInventory / factor;
                totalRow.EndInventory += product.CurrentInventory > 0 ? (product.CurrentInventory / factor) : 0;
                totalRow.Unload += product.UnloadedInventory / factor;
                totalRow.DamagedInTruck += product.DamagedInTruckInventory / factor;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += (value.Sales / factor);
                    totalRow.CreditReturns += (value.CreditReturns / factor);
                    totalRow.CreditDump += (value.CreditDump / factor);
                }
            }

            startY += font18Separation;
            var oldRound = Config.Round;
            Config.Round = 2;
            double TotalPrice = 0;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (Math.Round(p.EndInventory, Config.Round) == 0)
                    continue;

                float factor = 1;
                if (!isBase)
                {
                    var defaultUom = p.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                    {
                        p.UoM = defaultUom;
                        factor = defaultUom.Conversion;
                    }
                }

                var productNameLine = string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryProductRow], startY,
                                            p.Product.Name,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty
                                            );

                lines.Add(productNameLine);
                startY += font18Separation;
                startY += 5;

                lines.Add(GetInventorySummaryTableLineFixed(InventorySummaryDetailRow, startY,
                                                p.Lot,
                                                p.UoM != null ? p.UoM.Name : string.Empty,
                                                Math.Round(p.BegInv / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((p.LoadOut + p.Adj) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((p.TransferOn - p.TransferOff) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory / factor, Config.Round).ToString(CultureInfo.CurrentCulture)
                                                ));

                startY += font18Separation;
                startY += font18Separation;

                TotalPrice += p.EndInventory * p.Product.PriceLevel0;

            }

            startY += font18Separation;

            lines.Add(GetInventorySummaryTableTotalsFixed(InventorySummaryTotalsRow, startY,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.LoadOut + totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                                                    ));
            startY += font18Separation;

            if (Config.ShowPricesInInventorySummary)
            {
                startY += 10;

                lines.Add(GetInventorySummaryTableLineFixed(InventorySummaryDetailRow, startY,
                                        "Inventory Total Price: " + Math.Round(TotalPrice, Config.Round).ToCustomString(),
                                        "",
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""));

                startY += font18Separation;
            }

            Config.Round = oldRound;
            startY += font18Separation;
            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
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
                Logger.CreateLog("Starting printing inventory summary");
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


        protected virtual string GetInventorySummaryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);

        }

        protected virtual string GetInventorySummaryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8)
        {
            v1 = v1.Substring(0, v1.Length > 4 ? 4 : v1.Length);
            v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);
            v8 = v8.Substring(0, v8.Length > 4 ? 4 : v8.Length);


            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8);
        }


        #endregion

        #region Battery End Of Day

        public override bool PrintBatteryEndOfDay(int index, int count)
        {
            if (Config.UseBattery)
            {
                var printer = new BatteryPrinter();
                return printer.PrintBatteryEndOfDay(index, count);
            }

            return false;
        }

        #endregion


        #region Allowance

        protected virtual IEnumerable<string> GetDetailsRowsInOneDocForAllowance(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            string formatString = linesTemplates[AllowanceOrderDetailsHeader];

            if (Config.HideTotalInPrintedLine)
                formatString = formatString.Replace("PRICE", "");

            list.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            if (sales.Keys.Count > 0)
            {
                var lines = SortDetails.SortedDetails(sales.Values.ToList());
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
                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, listXX, "SALES SECTION", factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                var lines = SortDetails.SortedDetails(credit.Values.ToList());
                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, lines.ToList(), "DUMP SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                var lines = SortDetails.SortedDetails(returns.Values.ToList());
                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, lines.ToList(), "RETURNS SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected virtual IEnumerable<string> GetSectionRowsInOneDocForAllowance(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

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

                var productSlices = GetDetailsRowsSplitProductNameAllowance(name);

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
                        // anderson crap

                        double price = detail.Price * factor;

                        d -= (detail.OrderDetail.Allowance * detail.Qty * factor);

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string allowance = detail.OrderDetail.Allowance.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AllowanceOrderDetailsLine], startIndex, pName, qtyAsString, priceAsString, allowance, totalAsString));
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

                if (!string.IsNullOrEmpty(detail.OrderDetail.Lot))
                    if (preOrder)
                    {
                        if (Config.PrintLotPreOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
                        }
                    }
                    else
                    {
                        if (Config.PrintLotOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
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
                        string retPriceString = "Retail price                                   " + Convert.ToDouble(retailPrice.Item2).ToCustomString();
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                string upc = detail.Product.Upc;

                if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(detail.Product.Sku.Trim()))
                    upc = detail.Product.Sku;

                if (upc.Trim().Length > 0 & Config.PrintUPC)
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
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            startIndex += font18Separation / 2;

                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(upc)));
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

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

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

            var uomKeys = uomMap.Keys.ToList();
            if (!Config.HideTotalOrder && printTotal)
            {
                var key = uomKeys[0];
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, uomMap[key], balance.ToCustomString()));
                startIndex += font18Separation;
                uomKeys.Remove(key);
            }
            if (uomKeys.Count > 0)
            {
                foreach (var key in uomKeys)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty));
                    startIndex += font18Separation;
                }
            }
            return list;
        }

        protected virtual IEnumerable<string> GetTotalsRowsInOneDocAllowance(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;
            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            // payment.Components.Sum(x => x.Amount) : 0;
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
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

                    var salesTotalLine = double.Parse(Math.Round(Convert.ToDecimal(od.Price * factor * qty), 4).ToCustomString(), NumberStyles.Currency);

                    salesBalance += (salesTotalLine - (od.Allowance * od.Qty));

                    if (sales[key].Product.Taxable)
                        taxableAmount += salesTotalLine;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

                    var creditTotalLine = double.Parse(Math.Round(Convert.ToDecimal(od.Price * factor * qty), 4).ToCustomString(), NumberStyles.Currency) * -1;

                    creditBalance += (creditTotalLine + (od.Allowance * od.Qty));

                    if (credit[key].Product.Taxable)
                        taxableAmount += creditTotalLine;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

                    var returnTotalLine = double.Parse((od.Price * (od.Product.SoldByWeight ? od.Weight : od.Qty) * factor).ToCustomString(), NumberStyles.Currency) * -1;

                    returnBalance += (returnTotalLine + (od.Allowance * od.Qty));

                    if (returns[key].Product.Taxable)
                        taxableAmount += returnTotalLine;
                }
            }

            string s;
            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (!Config.HideTotalOrder && printTotal)
            {
                if (salesBalance > 0)
                {
                    s = "     SALES:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = salesBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s = "   CREDITS:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = creditBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s = "   RETURNS:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = returnBalance.ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                s = "NET AMOUNT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = (salesBalance + creditBalance + returnBalance).ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                double tax = Math.Round(taxableAmount * order.TaxRate, Config.Round);
                if (tax > 0 && !Config.HideTaxesTotalPrint)
                {
                    s = " SALES TAX:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = tax.ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }
                if (order.DiscountAmount > 0 && !Config.HideDiscountTotalPrint)
                {
                    s = "DISCOUNT:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = Math.Abs(order.CalculateDiscount()).ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, " C: " + order.DiscountComment));
                    startY += font18Separation;
                }
                // right justified
                s = "TOTAL DUE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = (salesBalance + creditBalance + returnBalance + tax - order.CalculateDiscount()).ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s = "TOTAL PAYMENT:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = paid.ToCustomString();
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;

                    s = "CURRENT BALANCE:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = (salesBalance + creditBalance + returnBalance + tax - paid - order.CalculateDiscount()).ToCustomString();
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments))
            {
                startY += font18Separation;
                var clines = OrderCommentsSplit(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string prefix = string.Empty;
                    if (i == 0)
                        prefix = "Comments: ";
                    else
                        prefix = "          ";
                    list.Add(string.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, prefix + clines[i]));
                    startY += font18Separation;
                }

            }
            return list;
        }


        #endregion


        #region Accept Load


        public override bool PrintAcceptLoad(IEnumerable<InventoryLine> SortedList, string docNumber, bool final)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetAcceptLoadHeaderRows(ref startY, docNumber));

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetAcceptLoadDetailsRows(ref startY, SortedList));

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetAcceptLoadHeaderRows(ref int startIndex, string docNumber)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryHeaderTitle], startIndex));
            startIndex += font36Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryHeaderTitle1], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Route #: ", Config.RouteName));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Driver Name: ", Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startIndex, docNumber, "Invoice"));
            startIndex += font36Separation;

            return list;
        }

        protected virtual IEnumerable<string> GetAcceptLoadDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex,
                "Product", "UoM", "Load", "Adj", "Inv"));
            startIndex += font18Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex,
                "", "", "Out", "", ""));
            startIndex += font18Separation + 5;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;

            foreach (var p in SortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetAddInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    var lfy = p.Product.BeginigInventory;
                    var load = p.Starting;
                    var adj = p.Real - p.Starting;
                    var st = p.Product.BeginigInventory;

                    string uom = string.Empty;

                    if (p.UoM != null)
                    {
                        lfy /= p.UoM.Conversion;
                        st /= p.UoM.Conversion;

                        uom = p.UoM.Name;
                    }

                    st += p.Real;

                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName,
                                               uom,
                                               Math.Round(load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round(adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round(st, Config.Round).ToString(CultureInfo.CurrentCulture)
                        ));

                        leftFromYesterday += p.Product.BeginigInventory;

                        var real = p.Real;

                        if (p.UoM != null)
                        {
                            load *= p.UoM.Conversion;
                            adj *= p.UoM.Conversion;
                            real *= p.UoM.Conversion;
                        }

                        requestedInventory += load;
                        adjustment += adj;

                        start += (p.Product.BeginigInventory + real);
                    }
                    else
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName, "", "", "", ""));
                    }

                    productLineOffset++;
                    startIndex += font18Separation;
                }
            }

            startIndex += 8;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, "Totals:",
                 "unit",
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));
            startIndex += font18Separation;

            return list;
        }

        #endregion


        #region Full Consignment

        public override bool PrintFullConsignment(Order order, bool asPreOrder)
        {
            if (Config.UseBattery)
                return PrintConsignment(order, asPreOrder, true, true, false);

            List<string> lines = new List<string>();
            int startIndex = 80;

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startIndex.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startIndex += Config.CompanyLogoHeight;
            }

            startIndex += 36;

            lines.AddRange(GetFullConsCompanyRows(ref startIndex, order));

            startIndex += font20Separation * 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAgentInfo], startIndex, Config.VendorName));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentConsignment], startIndex, order.PrintedOrderId));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentMerchant], startIndex, order.Client.ClientName));
            startIndex += font20Separation;

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAddress], startIndex, s1.Trim()));
                startIndex += font20Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "Phone: " + order.Client.ContactPhone));
            startIndex += font20Separation;

            DateTime last = order.Client.LastVisitedDate;
            if (last == DateTime.MinValue)
                last = DateTime.Now;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentLastTimeVisited], startIndex, last.ToString()));
            startIndex += 60;

            lines.AddRange(GetFullConsCountLines(ref startIndex, order));

            lines.AddRange(GetFullConsContractLines(ref startIndex, order));

            startIndex += 70;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "I accept the new consignment balance of"));
            startIndex += font20Separation;

            float totalNew = 0;
            double totalNewCost = 0;

            float totalPicked = 0;
            double totalPickedCost = 0;

            foreach (var item in order.Details)
            {
                var newCons = item.ConsignmentUpdated ? item.ConsignmentNew : item.ConsignmentOld;

                totalNew += newCons;
                totalNewCost += (newCons * item.ConsignmentNewPrice);

                var picked = item.ConsignmentPicked > 0 ? item.ConsignmentPicked : 0;

                totalPicked += picked;
                totalPickedCost += (picked * item.Price);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Consignment Qty", totalNew.ToString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Consignment Amount", totalNewCost.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Delivered Qty", totalPicked.ToString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Delivered Amount", totalPickedCost.ToCustomString()));

            if (!asPreOrder)
            {
                startIndex += 60;
                lines.AddRange(GetFullConsPaymentLines(ref startIndex, order));

            }

            startIndex += 60;

            if (asPreOrder)
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "*** NOTE: THIS IS A PREVIOUS COPY ***"));
            else
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "*** NOTE: THIS IS A STATEMENT COPY ***"));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPrintedOn], startIndex, DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            startIndex += 60;

            // add the signature
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startIndex));

                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));

                startIndex += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startIndex, order.SignatureName ?? string.Empty));
                    startIndex += font20Separation;
                }
                startIndex += font20Separation;
            }
            else
            {
                startIndex += 140;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
                startIndex += font20Separation;
            }

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startIndex + 60));
            var sb = new StringBuilder();
            foreach (string l in lines)
                sb.Append(l);

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
                return false;
            }
        }

        protected virtual IEnumerable<string> GetFullConsCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (CompanyInfo.SelectedCompany == null)
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                company = CompanyInfo.SelectedCompany;

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyName, "Date: " + order.Date.ToString()));
                startIndex += font20Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyAddress1, ""));
                startIndex += font20Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyAddress2, ""));
                    startIndex += font20Separation;
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "Phone: " + company.CompanyPhone));
                startIndex += font20Separation;

                //list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCompanyInfo], startIndex, "http://www.blackstoneonline.com", ""));
                //startIndex += font20Separation;

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected virtual List<string> GetFullConsCountLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            var sales = new List<OrderDetail>();
            var dumps = new List<OrderDetail>();
            var returns = new List<OrderDetail>();

            foreach (var item in order.Details)
            {
                if (item.ConsignmentCreditItem)
                {
                    if (item.Damaged)
                        dumps.Add(item);
                    else
                        returns.Add(item);
                }
                else
                    sales.Add(item);
            }

            lines.AddRange(GetSectionConsCountLines(ref startIndex, order, sales, "PRODUCTS SOLD"));
            lines.AddRange(GetSectionConsCountLines(ref startIndex, order, dumps, "CREDIT DUMPS"));
            lines.AddRange(GetSectionConsCountLines(ref startIndex, order, returns, "CREDIT RETURNS"));

            return lines;
        }

        protected virtual List<string> GetSectionConsCountLines(ref int startIndex, Order order, List<OrderDetail> section, string sectionName)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, sectionName));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountHeader1], startIndex));
            startIndex += font20Separation;

            double totalQty = 0;
            double totalDue = 0;

            foreach (var item in SortDetails.SortedDetails(section))
            {
                if (item.Qty == 0)
                    continue;

                var price = item.Price;
                if (item.IsCredit)
                    price *= -1;
                var totalLine = item.Qty * price;

                totalQty += item.Qty;
                totalDue += totalLine;

                var productSlices = SplitProductName(item.Product.Name, 31, 31);

                int offset = 0;
                foreach (var p in productSlices)
                {
                    if (offset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, p,
                            item.Qty, item.Price.ToCustomString(), totalLine.ToCustomString()));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, p,
                            "", "", ""));

                    startIndex += font20Separation;
                    offset++;
                }

                startIndex += 10;
            }

            if (lines.Count == 2)
                return new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, "                    Totals: ", totalQty, "",
                totalDue.ToCustomString()));
            startIndex += 40;

            startIndex += 30;

            return lines;
        }

        protected virtual List<string> GetFullConsContractLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "PRODUCTS BALANCE"));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractHeader], startIndex));
            startIndex += font20Separation;

            float totalOld = 0;
            float totalSold = 0;
            float totalCounted = 0;
            float totalPicked = 0;
            float totalnew = 0;

            List<OrderDetail> returns = new List<OrderDetail>();

            foreach (var item in SortDetails.SortedDetails(order.Details))
            {
                if (item.ConsignmentOld == 0 && item.ConsignmentNew == 0 && item.ConsignmentCount == 0 && item.Qty == 0 && item.ConsignmentPicked == 0)
                    continue;

                if (item.ConsignmentCreditItem)
                    continue;

                if (item.ConsignmentPicked < 0)
                    returns.Add(item);

                var productSlices = SplitProductName(item.Product.Name, 24, 24);

                int offset = 0;
                foreach (var p in productSlices)
                {
                    if (offset == 0)
                    {
                        var consNew = item.ConsignmentUpdated ? item.ConsignmentNew : item.ConsignmentOld;

                        totalOld += item.ConsignmentOld;
                        totalSold += item.Qty;
                        totalCounted += item.ConsignmentCount;
                        totalPicked += item.ConsignmentPicked > 0 ? item.ConsignmentPicked : 0;
                        totalnew += consNew;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, p,
                            item.ConsignmentOld, consNew, item.ConsignmentCount, item.Qty, item.ConsignmentPicked > 0 ? item.ConsignmentPicked : 0));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, p,
                            "", "", "", "", ""));

                    startIndex += font20Separation;
                    offset++;
                }

                startIndex += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, "", totalOld,
                totalnew, totalCounted, totalSold, totalPicked));
            startIndex += 40;

            if (returns.Count > 0)
            {
                startIndex += 30;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "CONSIGNMENT RETURNS"));
                startIndex += font20Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentReturnsHeader], startIndex));
                startIndex += font20Separation;

                float totalReturns = 0;

                foreach (var item in SortDetails.SortedDetails(returns))
                {
                    var picked = item.ConsignmentPicked * -1;

                    totalReturns += picked;

                    var productSlices = SplitProductName(item.Product.Name, 50, 50);

                    int offset = 0;
                    foreach (var p in productSlices)
                    {
                        if (offset == 0)
                        {

                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentReturnsLine], startIndex, p,
                                picked));
                        }
                        else
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentReturnsLine], startIndex, p,
                                ""));

                        startIndex += font20Separation;
                        offset++;
                    }

                    startIndex += 10;
                }

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentReturnsSep], startIndex));
                startIndex += 30;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentReturnsLine], startIndex, "", totalReturns));
                startIndex += 40;
            }

            return lines;
        }

        protected virtual List<string> GetFullConsPaymentLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();
            var yx = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            var payments = DataAccess.SplitPayment(yx).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "PAYMENTS"));
                startIndex += font20Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentHeader], startIndex));
                startIndex += font20Separation;

                foreach (var item in payments)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentLine], startIndex, item.PaymentMethod,
                        item.Amount.ToCustomString(), item.Ref));
                    startIndex += font20Separation;
                }
            }

            startIndex += 60;

            double clientBalance = order.Client.OpenBalance;
            double totalCost = order.OrderTotalCost();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPreviousBalance], startIndex, clientBalance.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAfterDisc], startIndex, totalCost.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentSep], startIndex));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotalDue], startIndex, (clientBalance + totalCost).ToCustomString()));
            startIndex += font20Separation;

            startIndex += 40;

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentTotal], startIndex, (paid * (-1)).ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentSep], startIndex));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentNewBalance], startIndex, (clientBalance + totalCost - paid).ToCustomString()));
            startIndex += font20Separation;


            return lines;
        }

        #endregion


        #region Print Inventory Lots

        public override bool PrintInventoryProd(List<InventoryProd> SortedList)
        {
            int startY = 80;

            List<string> lines = new List<string>();
            foreach (string s in GetInventoryProdHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            foreach (string s in GetInventoryProdDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }
            foreach (string s in GetFooterRows(ref startY, false))
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
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetInventoryProdHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();
            startIndex += 10;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryHeaderTitle], startIndex, DateTime.Now.ToString()));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Route #: ", Config.RouteName));
            startIndex += font18Separation;
            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySalesman], startIndex, "Driver Name: ", Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetInventoryProdDetailsRows(ref int startIndex, List<InventoryProd> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxes = 0;
            double value = 0;
            foreach (var prod in SortedList)
            {
                var p = prod.Product;

                float startInv = p.BeginigInventory;

                int productLineOffset = 0;
                foreach (string pName in GetInventoryDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName,
                            Math.Round(startInv, 2).ToString(CultureInfo.CurrentCulture), Math.Round(prod.Qty, 2).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName, "", ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }

                if (Config.UsePairLotQty)
                {
                    foreach (var lot in prod.ProdLots)
                    {
                        if (lot == null)
                            continue;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLineLot], startIndex, "Lot: " + lot.Lot,
                            Math.Round(lot.BeginingInventory, 2).ToString(CultureInfo.CurrentCulture), Math.Round(lot.CurrentQty, 2).ToString(CultureInfo.CurrentCulture)));
                        startIndex += font18Separation;
                    }
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryPriceLine], startIndex, string.Format(CultureInfo.InvariantCulture, "List Price: {0}  Total: {1}", p.PriceLevel0.ToCustomString(), (p.CurrentInventory * p.PriceLevel0).ToCustomString())));
                startIndex += font18Separation;

                if (Config.PrintUPCInventory)
                {
                    if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(p.Upc)));
                            startIndex += font36Separation;
                        }
                    }
                }

                numberOfBoxes += Convert.ToSingle(prod.Qty);
                value += prod.Qty * p.PriceLevel0;
                startIndex += font18Separation + orderDetailSeparation;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
            startIndex += font36Separation;
            if (!Config.Wstco)
            {
                s = "Inv. Value:" + value.ToCustomString();
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, WidthForBoldFont - s.Length) + s;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startIndex, s));
                startIndex += font36Separation;
            }
            return list;
        }

        #endregion


        #region Client Statement

        public override bool PrintClientStatement(Client client)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetClientStatementHeader(ref startY, client));

            startY += font36Separation;

            lines.AddRange(GetClientStatementTable(ref startY, client));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        private IEnumerable<string> GetClientStatementHeader(ref int startY, Client client)
        {
            List<string> lines = new List<string>();

            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }

            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s.Trim(), startY));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "License Number: " + client.LicenceNumber, startY));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Vendor Number: " + client.VendorNumber, startY));
                startY += font18Separation;
            }

            if (client.ExtraProperties != null)
            {
                var termsExtra = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null && !string.IsNullOrEmpty(termsExtra.Item2))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Terms: " + termsExtra.Item2.ToUpperInvariant(), startY));
                    startY += font18Separation;
                }
            }

            return lines;
        }

        private IEnumerable<string> GetClientStatementTable(ref int startY, Client client)
        {
            List<string> lines = new List<string>();

            var openInvoices = (from i in Invoice.OpenInvoices
                                where i.Client != null && i.Client.ClientId == client.ClientId && i.Balance != 0
                                orderby i.Date descending
                                select i).ToList();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTitle], startY));
            startY += 70;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader], startY));
            startY += font36Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double current = 0;
            double due1_30 = 0;
            double due31_60 = 0;
            double due61_90 = 0;
            double over90 = 0;

            foreach (var item in openInvoices.OrderBy(x => x.DueDate))
            {
                if (item.InvoiceType == 2 || item.InvoiceType == 3)
                    continue;

                int factor = item.InvoiceType == 1 ? -1 : 1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableLine], startY,
                    item.InvoiceType == 1 ? "Credit" : "Invoice",
                    item.Date.ToShortDateString(),
                    item.InvoiceNumber,
                    item.DueDate.ToShortDateString(),
                    item.Amount.ToCustomString(),
                    item.Balance.ToCustomString()));
                startY += font36Separation;

                current += item.Balance * factor;

                var due = DateTime.Now.Subtract(item.DueDate).Days;

                if (due > 0 && due < 31)
                    due1_30 += item.Balance;
                else if (due > 30 && due < 61)
                    due31_60 += item.Balance;
                else if (due > 60 && due < 91)
                    due61_90 += item.Balance;
                else if (due > 90)
                    over90 += item.Balance;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;
            startY += font36Separation;

            string s1;

            s1 = current.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "              Current:", s1));
            startY += font36Separation;

            s1 = due1_30.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "   1-30 Days Past Due:", s1));
            startY += font36Separation;

            s1 = due31_60.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "  31-60 Days Past Due:", s1));
            startY += font36Separation;

            s1 = due61_90.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "  61-90 Days Past Due:", s1));
            startY += font36Separation;

            s1 = over90.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "Over 90 Days Past Due:", s1));
            startY += font36Separation;

            s1 = (due1_30 + due31_60 + due61_90 + over90).ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTotal], startY, "           Amount Due:", s1));
            startY += font36Separation;

            return lines;
        }


        #endregion

        public override bool PrintInventoryCount(List<CycleCountItem> items)
        {
            return false;
        }

        public override bool PrintRefusalReport(int index, int count)
        {
            return false;
        }

        #region Sales Credit Report

        public override bool PrintCreditReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetCreditHeader(ref startY, index, count));

            startY += font36Separation;

            lines.AddRange(GetCreditReportTable(ref startY));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        protected virtual IEnumerable<string> GetCreditHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        public class CreditDetail
        {
            public Product Product { get; set; }

            public double Qty { get; set; }

            public double Price { get; set; }

            public bool Damaged { get; set; }

            public bool IsCredit { get; set; }
        }

        protected virtual IEnumerable<string> GetCreditReportTable(ref int startY)
        {
            List<string> lines = new List<string>();


            Dictionary<int, List<CreditDetail>> groupedReturns = new Dictionary<int, List<CreditDetail>>();

            var ordersToCheck = Order.Orders.Where(x => !x.Reshipped && !x.Voided).ToList();
            foreach (var order in ordersToCheck)
            {
                foreach (var detail in order.Details)
                {
                    if (detail.IsCredit)
                    {
                        if (groupedReturns.ContainsKey(order.Client.ClientId))
                        {
                            var alreadyThere = groupedReturns[order.Client.ClientId].FirstOrDefault(x => x.Product.ProductId == detail.Product.ProductId && x.Damaged == detail.Damaged && x.IsCredit == detail.IsCredit);
                            if (alreadyThere != null)
                                alreadyThere.Qty += detail.Qty;
                            else
                                groupedReturns[order.Client.ClientId].Add(new CreditDetail() { Product = detail.Product, Qty = detail.Qty, Price = detail.Price, Damaged = detail.Damaged, IsCredit = detail.IsCredit });
                        }
                        else
                            groupedReturns.Add(order.Client.ClientId, new List<CreditDetail>() { new CreditDetail() { Product = detail.Product, Qty = detail.Qty, Price = detail.Price, Damaged = detail.Damaged, IsCredit = detail.IsCredit } });
                    }
                }
            }

            double salesTotal = ordersToCheck.Sum(x => x.OrderSalesTotalCost());
            double creditTotal = ordersToCheck.Sum(x => x.OrderCreditTotalCost());

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double totalItems = 0;

            foreach (var c in groupedReturns)
            {
                var client = Client.Find(c.Key);
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportClientName], startY, client.ClientName, "", ""));
                startY += font18Separation + 5;

                foreach (var detail in c.Value.ToList())
                {
                    totalItems += detail.Qty;

                    string name = "Return";
                    if (detail.Damaged)
                        name = "Dump";

                    bool isFirstLine = true;
                    foreach (var productName in SplitProductName(detail.Product.Name, 23, 50))
                    {
                        if (isFirstLine)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsLine], startY, productName, name, detail.Qty, detail.Price.ToCustomString(), (detail.Price * detail.Qty).ToCustomString()));
                            startY += font18Separation;
                            isFirstLine = false;
                            continue;
                        }

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsLine], startY, productName, "", "", "", ""));
                        startY += font18Separation;
                    }

                    startY += 5;
                }
            }


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsTotal], startY, "TOTAL", totalItems, Math.Abs(creditTotal).ToCustomString()));
            startY += font18Separation;

            startY += font36Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "SALES TOTAL:", salesTotal.ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "CREDIT TOTAL:", Math.Abs(creditTotal).ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "TOTAL:", (salesTotal + creditTotal).ToCustomString()));
            startY += font18Separation;

            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }
        #endregion

    }
}

