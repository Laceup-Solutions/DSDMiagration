using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class UnitedPrinter : ZebraFourInchesPrinter
    {
        protected const string UnitedCompanyName = "UnitedCompanyName";
        protected const string UnitedCompanyInfo = "UnitedCompanyInfo";
        protected const string UnitedDate = "UnitedDate";
        protected const string UnitedRoute = "UnitedRoute";
        protected const string UnitedDriverName = "UnitedDriverName";
        protected const string UnitedClientName = "UnitedClientName";
        protected const string UnitedClientInfo = "UnitedClientInfo";
        protected const string UnitedInvoiceNum = "UnitedInvoiceNum";
        protected const string UnitedTableHeader = "UnitedTableHeader";
        protected const string UnitedTableLine = "UnitedTableLine";
        protected const string UnitedTableSep = "UnitedTableSep";
        protected const string UnitedTableTotal = "UnitedTableTotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(UnitedCompanyName, "^FO200,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(UnitedCompanyInfo, "^FO200,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(UnitedDate, "^FO40,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(UnitedRoute, "^FO40,{0}^ADN,18,10^FDRoute#: {1}^FS");
            linesTemplates.Add(UnitedDriverName, "^FO40,{0}^ADN,18,10^FDDriver Name: {1}^FS");
            linesTemplates.Add(UnitedClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(UnitedClientInfo, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(UnitedInvoiceNum, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(UnitedTableHeader, "^FO40,{1}^ADN,18,10^FDPRODUCT^FS" +
                "^FO340,{1}^ADN,18,10^FDQTY^FS" +
                "^FO400,{1}^ADN,18,10^FDUNITS^FS" +
                "^FO495,{0}^ADN,18,10^FDUNIT^FS^FO495,{1}^ADN,18,10^FDPRICE^FS" +
                "^FO600,{0}^ADN,18,10^FD^FS^FO600,{1}^ADN,18,10^FDALLOW^FS" +
                "^FO680,{1}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(UnitedTableSep, "^FO40,{0}^ADN,18,10^FD---------------------------------------------------------------^FS");
            linesTemplates.Add(UnitedTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO340,{0}^ADN,18,10^FD{2}^FS" +
                "^FO400,{0}^ADN,18,10^FD{3}^FS" +
                "^FO495,{0}^ADN,18,10^FD{4}^FS" +
                "^FO600,{0}^ADN,18,10^FD{5}^FS" +
                "^FO680,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(UnitedTableTotal, "^FO230,{0}^ADN,18,10^FDTotals:^FS^FO340,{0}^ADN,18,10^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
        }

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool preOrder, Order order, Client client, string invoiceId, IList<PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            string docName = "Invoice";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                    docName = "Bill";
            }
            if (order.AsPresale)
                docName = "Sales Order";

            if (order.OrderType == OrderType.Credit)
                docName = "Credit";

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = docName + ": " + order.PrintedOrderId;

            lines.AddRange(GetOrderCompanyRows(ref startY, order.CompanyName));

            startY += 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedRoute], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedDriverName], startY, Config.VendorName));
            startY += font18Separation;

            startY += 30;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedClientName], startY, clientSplit));
                startY += font36Separation;
            }

            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedClientInfo], startY, s.Trim()));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = order.Client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedClientInfo], startY, "Account Balance: " + balance));
                startY += font18Separation;
            }

            startY += 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedInvoiceNum], startY, s1));
            startY += font36Separation;

            if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));

            return lines;
        }

        IEnumerable<string> GetOrderCompanyRows(ref int startIndex, string companyName = null)
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
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyName], startIndex, part));
                    startIndex += font36Separation;
                }

                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyInfo], startIndex, company.CompanyAddress1));
                startIndex += font18Separation;
                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyInfo], startIndex, company.CompanyAddress2));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyInfo], startIndex, "Phone: " + company.CompanyPhone));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyInfo], startIndex, "Fax: " + company.CompanyFax));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedCompanyInfo], startIndex, "Email: " + company.CompanyEmail));
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

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            startY += 50;

            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedTableHeader], startY - 20, startY));
            startY += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedTableSep], startY));
            startY += font18Separation;

            int factor = 1;

            if (sales.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines = SortDetails.SortedDetails(sales.Values.ToList());

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
                IQueryable<OrderLine> lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "DUMP SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines = SortDetails.SortedDetails(returns.Values.ToList());

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

            float totalQty = 0;
            double totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                var package = Convert.ToDouble(p.Package);

                if (detail.OrderDetail.IsCredit && (order.AsPresale || detail.OrderDetail.Damaged))
                    package = 1;

                var price = detail.Price;
                double pricePk = price;

                pricePk = price / package;

                double allow = 0;
                if (detail.ListPrice > detail.Price)
                    //allow = 100 * (1 - (price / detail.ListPrice));
                    allow = detail.ListPrice - price;

                price *= factor;

                totalQty += detail.Qty;
                totalUnits += package * detail.Qty;

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;
                var productSlices = SplitProductName(name, 24, 24);

                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        balance += d;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedTableLine], startIndex, pName,
                            Math.Round(detail.Qty, 2).ToString(),
                            (package * detail.Qty).ToString(),
                            pricePk.ToCustomString(),
                            //Math.Round(allow, 2).ToString(),
                            allow.ToCustomString(),
                            d.ToCustomString()));

                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedTableLine], startIndex, pName,
                            "",
                            "",
                            "",
                            "",
                            ""));
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

            var balanceText = balance.ToCustomString();

            if (!Config.HideTotalOrder && t == null)
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UnitedTableTotal], startIndex,
                    totalQty, totalUnits, balanceText));
                startIndex += font18Separation;
            }

            return list;
        }

    }
}