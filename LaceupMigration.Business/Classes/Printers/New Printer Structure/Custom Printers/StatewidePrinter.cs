using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;





 


namespace LaceupMigration
{
    public class StatewidePrinter : TextFourInchesPrinter
    {
        protected const string StatewideCompany = "StatewideCompany";
        protected const string StatewideCompanyAddr = "StatewideCompanyAddr";
        protected const string StatewideCompanyPhone = "StatewideCompanyPhone";
        protected const string StatewideInvoiceInfo = "StatewideInvoiceInfo";
        protected const string StatewideCustomer = "StatewideCustomer";
        protected const string StatewideCustomerAddr = "StatewideCustomerAddr";
        protected const string StatewideInvoice = "StatewideInvoice";
        protected const string StatewideDate = "StatewideDate";
        protected const string StatewideTime = "StatewideTime";
        protected const string StatewideSalesman = "StatewideSalesman";

        protected const string StatewideTableHeader = "StatewideTableHeader";
        protected const string StatewideTableLine = "StatewideTableLine";

        protected const string StatewideTotalsSeparator = "                                           ---------------------";

        protected const string StatewideSubtotal = "StatewideSubtotal";
        protected const string StatewideTotalDue = "StatewideTotalDue";

        protected const string StatewidePaymentSeparator = "                              ----------------------------------";

        protected const string StatewidePaymentInfo = "StatewidePaymentInfo";
        protected const string StatewideCashAmount = "StatewideCashAmount";
        protected const string StatewideCharge = "StatewideCharge";
        protected const string StatewideCheckAmount = "StatewideCheckAmount";
        protected const string StatewideCheckNumber = "StatewideCheckNumber";
        protected const string StatewideCCAmount = "StatewideCCAmount";
        protected const string StatewideTotalPaid = "StatewideTotalPaid";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(StatewideCompany, "{1}");
            linesTemplates.Add(StatewideCompanyAddr, "{1}");
            linesTemplates.Add(StatewideCompanyPhone, "{1}");
            linesTemplates.Add(StatewideInvoiceInfo, "|{1} {2}|");
            linesTemplates.Add(StatewideCustomer, "CUSTOMER: {1}");
            linesTemplates.Add(StatewideCustomerAddr, "          {1}");
            linesTemplates.Add(StatewideInvoice, "INVOICE #: {1}");
            linesTemplates.Add(StatewideDate, "DATE: {1}");
            linesTemplates.Add(StatewideTime, "TIME: {1}");
            linesTemplates.Add(StatewideSalesman, "SALESMAN: {1}");

            linesTemplates.Add(StatewideTableHeader, "|QTY |UPC          |DESCRIPTION            |PRICE    |TOTAL    |");
            linesTemplates.Add(StatewideTableLine, "|{1}|{2}|{3}|{4}|{5}|");
            linesTemplates.Add(StatewideSubtotal, "|{1}|SUBTOTAL |{2}|");
            linesTemplates.Add(StatewideTotalDue, "                                           |TOTAL DUE|{1}|");

            linesTemplates.Add(StatewidePaymentInfo, "                              |          PAYMENT INFO          |");
            linesTemplates.Add(StatewideCashAmount, "                              | CASH AMOUNT: {1}|");
            linesTemplates.Add(StatewideCharge, "                              |      CHARGE: {1}|");
            linesTemplates.Add(StatewideCheckAmount, "                              |CHECK AMOUNT: {1}|");
            linesTemplates.Add(StatewideCheckNumber, "                              |CHECK NUMBER: {1}|");
            linesTemplates.Add(StatewideCCAmount, "                              |  C.C AMOUNT: {1}|");
            linesTemplates.Add(StatewideTotalPaid, "                              |  TOTAL PAID: {1}|");

            linesTemplates[InventorySettlementHeader] = "Inventory Settlement Report";
        }

        protected override bool PrintLines(List<string> lines)
        {
            try
            {
                for (int i = 0; i < 5; i++)
                    lines.Add(string.Empty);

                var finalText = new StringBuilder();
                foreach (var line in lines)
                {
                    var l = line.PadLeft(line.Length + 3, ' ');

                    finalText.Append(l);
                    finalText.Append((char)10);
                    finalText.Append((char)13);
                }

                PrintIt(finalText.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        string CenterText(string s)
        {
            int half = WidthForNormalFont / 2;

            int halfS = s.Length / 2;

            return new String(' ', half - halfS) + s;
        }

        string CenterText(string s, int length)
        {
            int half = length / 2;

            int halfS = s.Length / 2;

            return new String(' ', half - halfS) + s;
        }

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool asPreOrder, Order order, Client client, string printedId, List<PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            var dots = string.Empty;
            dots = new string('-', WidthForNormalFont - dots.Length) + dots;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, dots));
            startY += font18Separation;

            string cust_Number = "Customer # " + order.Client.OriginalId;
            cust_Number = CenterText(cust_Number, 30);

            string inv_Number = "Invoice # " + printedId;

            lines.Add(GetInvoiceInfoFixedLine(StatewideInvoiceInfo, startY, cust_Number, inv_Number));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, dots));
            startY += font18Separation;

            var cust_Name = order.Client.ClientName;
            if (cust_Name.Contains('-'))
                cust_Name = cust_Name.Substring(cust_Name.IndexOf('-') + 1);

            List<Tuple<string, string>> values = new List<Tuple<string, string>>();

            foreach (var item in SplitProductName(cust_Name, 30, 30))
                values.Add(new Tuple<string, string>(CenterText(item, 30), ""));

            foreach (var item in ClientAddress(client))
                values.Add(new Tuple<string, string>(CenterText(item, 30), ""));

            values.Add(new Tuple<string, string>(CenterText(order.Client.ContactPhone, 30), ""));

            string date = "Date: " + order.Date.ToShortDateString();

            if (values.Count > 0)
                values[0] = new Tuple<string, string>(values[0].Item1, date);
            else
                values.Add(new Tuple<string, string>("", date));

            string time = "Time: " + order.Date.ToShortTimeString();

            if (values.Count > 1)
                values[1] = new Tuple<string, string>(values[1].Item1, time);
            else
                values.Add(new Tuple<string, string>("", time));

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
            {
                if (values.Count > 2)
                    values[2] = new Tuple<string, string>(values[2].Item1, "Salesman: " + salesman.Name);
                else
                    values.Add(new Tuple<string, string>("", "Salesman: " + salesman.Name));
            }

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                if (values.Count > 3)
                    values[3] = new Tuple<string, string>(values[3].Item1, "Term: " + term);
                else
                    values.Add(new Tuple<string, string>("", "Term: " + term));
            }

            foreach (var item in values)
            {
                lines.Add(GetInvoiceInfoFixedLine(StatewideInvoiceInfo, startY, item.Item1, item.Item2));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, dots));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            return lines;
        }

        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();

                string cName = company.FromFile ? UDFHelper.GetSingleUDF("company", company.ExtraFields) : company.CompanyName;

                foreach (string part in CompanyNameSplit(cName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompany], startIndex, CenterText(part)));
                    startIndex += font36Separation;
                }

                string addr = company.CompanyAddress1;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyAddr], startIndex, CenterText(addr)));
                startIndex += font18Separation;

                addr = company.CompanyAddress2;

                if (addr.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyAddr], startIndex, CenterText(addr)));
                    startIndex += font18Separation;
                }

                string phone = company.FromFile ? "" : company.CompanyPhone;

                if (!string.IsNullOrEmpty(phone))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyPhone], startIndex, CenterText("Phone: " + phone)));
                    startIndex += font18Separation;
                }

                string fax = company.FromFile ? "" : company.CompanyFax;

                if (!string.IsNullOrEmpty(fax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyPhone], startIndex, CenterText("Fax: " + fax)));
                    startIndex += font18Separation;
                }

                string email = company.FromFile ? "" : company.CompanyEmail;

                if (!string.IsNullOrEmpty(email))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyPhone], startIndex, CenterText("Email: " + email)));
                    startIndex += font18Separation;
                }

                var reference = company.FromFile ? company.CompanyName : "";
                if (!string.IsNullOrEmpty(reference))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyPhone], startIndex, CenterText("Reference #: " + reference)));
                    startIndex += font18Separation;
                }

                string vendor = company.FromFile ? UDFHelper.GetSingleUDF("vendor", company.ExtraFields) : "";
                if (!string.IsNullOrEmpty(vendor))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideCompanyPhone], startIndex, CenterText(vendor)));
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

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            string formatString = linesTemplates[StatewideTableHeader];

            if (Config.HidePriceInPrintedLine)
                HidePriceInOrderPrintedLine(ref formatString);

            if (Config.HideTotalInPrintedLine)
                HideTotalInOrderPrintedLine(ref formatString);

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return lines;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

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

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;

                        list.Add(GetSectionRowsInOneDocFixedLine(StatewideTableLine, startIndex, qtyAsString, p.Upc, pName, priceAsString, totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(GetSectionRowsInOneDocFixedLine(StatewideTableLine, startIndex, "", "", pName, "", ""));
                        startIndex += font18Separation;
                    }
                    else
                        break;

                    productLineOffset++;
                }
                startIndex += 10;
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }

        protected string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            if (v1.Length < 4)
                v1 = new string(' ', 4 - v1.Length) + v1;

            if (v2.Length < 13)
                v2 += new string(' ', 13 - v2.Length);

            if (v3.Length < 23)
                v3 += new string(' ', 23 - v3.Length);

            if (v4.Length < 9)
                v4 = new string(' ', 9 - v4.Length) + v4;

            if (v5.Length < 9)
                v5 = new string(' ', 9 - v5.Length) + v5;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            if (!Config.HideTotalOrder)
            {
                var s = string.Empty;
                s = new string('-', WidthForNormalFont - s.Length) + s;

                var totalQty = totalQtyNoUoM.ToString();
                if (totalQty.Length < 4)
                    totalQty = totalQty + new string(' ', 4 - totalQty.Length);

                totalQty += " TOTAL ITEMS";

                if (totalQty.Length < 42)
                    totalQty = totalQty + new string(' ', 42 - totalQty.Length);

                var subtotal = order.CalculateItemCost().ToCustomString();
                if (subtotal.Length < 9)
                    subtotal = new string(' ', 9 - subtotal.Length) + subtotal;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[StatewideSubtotal], startIndex, totalQty, subtotal));
                startIndex += font18Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(GetOrderDetailsSectionTotalFixedLine(StatewideTotalDue, startIndex, order.OrderTotalCost().ToCustomString()));
                startIndex += font18Separation;

                list.Add(StatewideTotalsSeparator);
            }

            return list;
        }

        protected string GetOrderDetailsSectionTotalFixedLine(string format, int pos, string v1)
        {
            if (v1.Length < 9)
                v1 = new string(' ', 9 - v1.Length) + v1;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1);
        }

        //protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        //{
        //    List<string> list = new List<string>();

        //    list.Add(StatewidePaymentSeparator);

        //    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[StatewidePaymentInfo], startY));
        //    startY += font18Separation;

        //    list.Add(StatewidePaymentSeparator);

        //    double cashAmount = 0;
        //    double charge = 0;
        //    double checkAmount = 0;
        //    string checkNumber = "";
        //    double ccAmount = 0;
        //    double paid = 0;

        //    if (payment != null)
        //    {
        //        var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);

        //        foreach (var item in parts)
        //        {
        //            switch (item.PaymentMethod)
        //            {
        //                case InvoicePaymentMethod.Cash:
        //                    cashAmount += item.Amount;
        //                    break;
        //                case InvoicePaymentMethod.Check:
        //                    checkAmount += item.Amount;
        //                    checkNumber = item.Ref;
        //                    break;
        //                case InvoicePaymentMethod.Credit_Card:
        //                    ccAmount += item.Amount;
        //                    break;
        //                case InvoicePaymentMethod.Money_Order:
        //                    break;
        //                case InvoicePaymentMethod.Transfer:
        //                    break;
        //                default:
        //                    break;
        //            }

        //            paid += item.Amount;
        //        }

        //        charge = order.OrderTotalCost() - paid;
        //    }

        //    list.Add(GetOrderTotalsFixedLine(StatewideCashAmount, startY, cashAmount.ToCustomString()));
        //    startY += font18Separation;

        //    list.Add(GetOrderTotalsFixedLine(StatewideCharge, startY, charge.ToCustomString()));
        //    startY += font18Separation;

        //    list.Add(GetOrderTotalsFixedLine(StatewideCheckAmount, startY, checkAmount.ToCustomString()));
        //    startY += font18Separation;

        //    list.Add(GetOrderTotalsFixedLine(StatewideCheckNumber, startY, checkNumber));
        //    startY += font18Separation;

        //    list.Add(GetOrderTotalsFixedLine(StatewideCCAmount, startY, ccAmount.ToCustomString()));
        //    startY += font18Separation;

        //    list.Add(GetOrderTotalsFixedLine(StatewideTotalPaid, startY, paid.ToCustomString()));
        //    startY += font18Separation;

        //    list.Add(StatewidePaymentSeparator);

        //    return list;
        //}

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            List<string> list = new List<string>();

            return list;
        }

        protected override IEnumerable<string> GetFooterRows(ref int startIndex, bool asPreOrder, string CompanyName = null)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, font18Separation, 2);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            AddExtraSpace(ref startIndex, list, 12, 1);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            AddExtraSpace(ref startIndex, list, font18Separation, 1);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                AddExtraSpace(ref startIndex, list, font18Separation, 1);
                foreach (var line in GetBottomSplitText())
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
                }
            }

            return list;
        }

        protected string GetOrderTotalsFixedLine(string format, int pos, string v1)
        {
            if (v1.Length < 18)
                v1 = new string(' ', 18 - v1.Length) + v1;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1);
        }

        protected string GetInvoiceInfoFixedLine(string format, int pos, string v1, string v2)
        {
            if (v1.Length < 30)
                v1 = v1 + new string(' ', 30 - v1.Length);

            if (v2.Length < 31)
                v2 = v2 + new string(' ', 31 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }
    }
}