using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class EastTexasPrinter : RetailPricePrinter
    {

        protected const string BarcodeInvoiceNumber = "BarcodeInvoiceNumber";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(BarcodeInvoiceNumber, "^FO400,50^BCN,30^FD{1}^FS^");
        }

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool preOrder, Order order, Client client, string invoiceId, IList<DataAccess.PaymentSplit> payments, bool paidInFull)
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
            try
            {   //Add barcode 
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BarcodeInvoiceNumber], startY, order.PrintedOrderId, string.Empty));
            }
            catch { }

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = docName + ": " + order.PrintedOrderId;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle1], startY, s1, string.Empty));
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


            if (Config.UseClientClassAsCompanyName)
                lines.AddRange(GetCompanyRows(ref startY, order));
            else
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY, order.CompanyName));

            startY += font18Separation; //an extra line

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
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

        public override bool PrintOpenInvoice(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            try
            {   //Add barcode 
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BarcodeInvoiceNumber], startY, invoice.InvoiceNumber, string.Empty));
                startY += font36Separation;
            }
            catch { }

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

                if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
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
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startY, p.Upc));
                            startY += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            lines.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, Product.GetFirstUpcOnly(p.Upc)));
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

                var retPrice = detail.Price;

                retPrice = GetRetailPrice1(order, detail.OrderDetail);

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

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            pName, qtyAsString, retPrice.ToCustomString(), extRetailPrice.ToCustomString(), priceAsString, totalAsString));
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

            double GetRetailPrice1(Order order, OrderDetail detail)
            {
                double retPrice = 0;

                if (order.Client.RetailPriceLevelId != 0)
                {
                    var retailPriceLevel = RetailPriceLevel.Pricelist.FirstOrDefault(x => x.Id == order.Client.RetailPriceLevelId);

                    if (retailPriceLevel != null)
                    {
                        if (retailPriceLevel.RetailPriceLevelType == 1)
                        {
                            retPrice = GetRetailPrice(detail.Product, order.Client);
                        }
                        else
                        {
                            if (retailPriceLevel.Percentage != 0 && retailPriceLevel.Percentage > 0)
                            {
                                retPrice = (detail.Price / (100 - retailPriceLevel.Percentage)) * 100;
                            }
                            else
                                retPrice = GetRetailPrice(detail.Product, order.Client);
                        }
                    }
                    else
                        retPrice = GetRetailPrice(detail.Product, order.Client);
                }
                else
                    retPrice = GetRetailPrice(detail.Product, order.Client);

                return retPrice;
            }

            double GetRetailPrice(Product p, Client client)
            {
                if (client == null)
                    return 0;

                var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

                return retailProdPrice != null ? retailProdPrice.Price : 0;
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

            var uomKeys = uomMap.Keys.ToList();
            if (!Config.HideTotalOrder && t == null)
            {
                var key = uomKeys[0];
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTotalsLine], startIndex,
                    key, uomMap[key], balance.ToCustomString(), balanceRP.ToCustomString()));
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
            return list;
        }
    }
}