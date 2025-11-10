using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class RetailPriceThreePrinter : ZebraThreeInchesPrinter
    {
        protected const string RetailPriceTableHeader0 = "RetailPriceTableHeader0";
        protected const string RetailPriceTableHeader = "RetailPriceTableHeader";
        protected const string RetailPriceTableLine = "RetailPriceTableLine";
        protected const string RetailPriceTotalsLine = "RetailPriceTotalsLine";

        protected const string RetailPriceClientName = "RetailPriceClientName";
        protected const string RetailPriceClientNumber = "RetailPriceClientNumber";
        protected const string RetailPriceOrderHeader = "RetailPriceOrderHeader";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(RetailPriceTableHeader0, "^FO15,{0}^ADN,18,10^FD^FS" +
                "^FO250,{0}^ADN,18,10^FD^FS" +
                "^FO310,{0}^ADN,18,10^FDR. PRICE^FS" +
                "^FO420,{0}^ADN,18,10^FDEXT R. PRICE^FS");

            linesTemplates.Add(RetailPriceTableHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO250,{0}^ADN,18,10^FDQTY^FS" +
                "^FO310,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO420,{0}^ADN,18,10^FDTOTAL^FS");

            linesTemplates.Add(RetailPriceTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO250,{0}^ADN,18,10^FD{2}^FS" +
                "^FO310,{0}^ADN,18,10^FD{3}^FS" +
                "^FO420,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(RetailPriceTotalsLine, "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO250,{0}^ADN,18,10^FD{2}^FS"+
                "^FO420,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(RetailPriceClientName, "^FO15,{0}^ADN,28,14^FD{1}^FS");

            linesTemplates.Add(RetailPriceClientNumber, "^FO15,{0}^ADN,28,14^FDCustomer # {1}^FS");

            linesTemplates.Add(RetailPriceOrderHeader, "^FO15,{0}^ADN,28,14^FD{1}^FS");
        }

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool preOrder, Order order, Client client, string invoiceId, IList<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            if (Config.UseClientClassAsCompanyName)
                lines.AddRange(GetCompanyRows(ref startY, order));
            else
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY, order.CompanyName));

            startY += font18Separation; //an extra line

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

            startY += font36Separation;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceClientNumber], startY, custNoString));
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

            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null && !string.IsNullOrEmpty(termsExtra.Item2))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Terms: " + termsExtra.Item2.ToUpperInvariant(), startY));
                    startY += font18Separation;
                }
            }
            if (Config.PrintClientOpenBalance)
            {
                var balance = order.Client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Account Balance: " + balance, startY));
                startY += font18Separation;
            }

            startY += font36Separation;

            string docName = "Invoice";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Bill";
                }
            }
            if (order.AsPresale)
            {
                docName = "Sales Order";
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credit";
            }

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = docName + ": " + order.PrintedOrderId;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceOrderHeader], startY, s1));
            startY += font36Separation;

            
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

        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, string companyName = null)
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
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderName], part, startIndex));
                    startIndex += font36Separation;
                }
                
                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
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
                
                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
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

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
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

                var retPrice = GetRetailPrice(p, order.Client);

                var extRetailPrice = retPrice;
                extRetailPrice *= detail.Qty;
                if (detail.UoM != null)
                    extRetailPrice *= detail.UoM.Conversion;

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

                if (preOrder && Config.PrintZeroesOnPickSheet)
                    factor = 0;

                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            pName, qtyAsString, retPrice.ToCustomString(), extRetailPrice.ToCustomString()));
                        startIndex += font18Separation;
                    }
                    else 
                    {
                        if(productLineOffset == 1)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            pName, string.Empty, priceAsString, totalAsString));
                            startIndex += font18Separation;
                        }
                        else if (!Config.PrintTruncateNames)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSecondLine], startIndex, pName));
                            startIndex += font18Separation;
                        }
                        else
                            break;
                    }
                        
                    productLineOffset++;
                }

                if(productSlices.Count == 1)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceTableLine], startIndex,
                            string.Empty, string.Empty, priceAsString, totalAsString));
                    startIndex += font18Separation;
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

        private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        protected override IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 16, 16);
        }

        protected override IEnumerable<string> GetSignatureSection(ref int startY, Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

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
                    lines.Add(s);

            startY += 2 * font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], "Please Remit To", startY));
            startY += font18Separation;

            CompanyInfo company = null;

            if (CompanyInfo.Companies.Count == 0)
                return new List<string>();
            if (CompanyInfo.SelectedCompany == null)
                CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

            company = CompanyInfo.SelectedCompany;

            foreach (string part in CompanyNameSplit(company.CompanyName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPriceClientName], startY, part));
                startY += font36Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr1], company.CompanyAddress1, startY));
            startY += font18Separation;

            if (company.CompanyAddress2.Trim().Length > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderAddr2], company.CompanyAddress2, startY));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Phone: " + company.CompanyPhone, startY));
            startY += font18Separation;

            if (!string.IsNullOrEmpty(company.CompanyFax))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Fax: " + company.CompanyFax, startY));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HeaderPhone], "Email: " + company.CompanyEmail, startY));
                startY += font18Separation;
            }



            return lines;
        }

    }
}