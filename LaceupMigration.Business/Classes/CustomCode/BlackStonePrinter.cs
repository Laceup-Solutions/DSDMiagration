using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BlackStonePrinter : ZebraFourInchesPrinter
    {
        int fontSize = 20;

        protected const string BlackStoneCompanyInfo = "BlackStoneCompanyInfo";
        protected const string BlackStoneAgentInfo = "BlackStoneAgentInfo";
        protected const string BlackStoneConsignment = "BlackStoneConsignment";
        protected const string BlackStoneMerchant = "BlackStoneMerchant";
        protected const string BlackStoneMerchantId = "BlackStoneMerchantId";
        protected const string BlackStoneAddress = "BlackStoneAddress";
        protected const string BlackStoneLastTimeVisited = "BlackStoneLastTimeVisited";
        protected const string BlackStoneSectionName = "BlackStoneSectionName";
        protected const string BlackStoneCountHeader1 = "BlackStoneCountHeader1";
        protected const string BlackStoneCountHeader2 = "BlackStoneCountHeader2";
        protected const string BlackStoneCountLine = "BlackStoneCountLine";
        protected const string BlackStoneCountSep = "BlackStoneCountSep";
        protected const string BlackStoneCountTotal = "BlackStoneCountTotal";
        protected const string BlackStoneContractHeader = "BlackStoneContractHeader";
        protected const string BlackStoneContractLine = "BlackStoneContractLine";
        protected const string BlackStoneContractSep = "BlackStoneContractSep";
        protected const string BlackStoneText = "BlackStoneText";
        protected const string BlackStoneTotals = "BlackStoneTotals";
        protected const string BlackStonePaymentHeader = "BlackStonePaymentHeader";
        protected const string BlackStonePaymentLine = "BlackStonePaymentLine";
        protected const string BlackStonePreviousBalance = "BlackStonePreviousBalance";
        protected const string BlackStoneAfterDisc = "BlackStoneAfterDisc";
        protected const string BlackStonePaymentSep = "BlackStonePaymentSep";
        protected const string BlackStoneTotalDue = "BlackStoneTotalDue";
        protected const string BlackStonePaymentTotal = "BlackStonePaymentTotal";
        protected const string BlackStoneNewBalance = "BlackStoneNewBalance";
        protected const string BlackStonePrintedOn = "BlackStonePrintedOn";
        protected const string BlackStoneSignature = "BlackStoneSignature";
        protected const string BlackStoneFinalized = "BlackStoneFinalized";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(BlackStoneCompanyInfo, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(BlackStoneAgentInfo, "^FO40,{0}^ADN,18,10^FDAgent Info: {1}^FS");
            linesTemplates.Add(BlackStoneConsignment, "^FO40,{0}^ADN,18,10^FDCONSIGNMENT {1}^FS");
            linesTemplates.Add(BlackStoneMerchant, "^FO40,{0}^ADN,18,10^FDMerchant: {1}^FS");
            linesTemplates.Add(BlackStoneMerchantId, "^FO40,{0}^ADN,18,10^FDMerchant ID: {1}^FS");
            linesTemplates.Add(BlackStoneAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(BlackStoneLastTimeVisited, "^FO40,{0}^ADN,18,10^FDLast time visited: {1}^FS");
            linesTemplates.Add(BlackStoneSectionName, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(BlackStoneCountHeader1, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO440,{0}^ADN,18,10^FDQTY^FS" +
                "^FO540,{0}^ADN,18,10^FDPrice^FS" +
                "^FO680,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(BlackStoneCountHeader2, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO440,{0}^ADN,18,10^FD^FS" +
                "^FO540,{0}^ADN,18,10^FDPrice^FS" +
                "^FO680,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(BlackStoneCountLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO440,{0}^ADN,18,10^FD{2}^FS" +
                "^FO540,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(BlackStoneCountSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO440,{0}^ADN,18,10^FD_______^FS" +
                "^FO540,{0}^ADN,18,10^FD__________^FS" +
                "^FO680,{0}^ADN,18,10^FD_________^FS");

            linesTemplates.Add(BlackStoneCountTotal, "^FO40,{0}^ADN,18,10^FDTotal: {1}^FS");

            linesTemplates.Add(BlackStoneContractHeader, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO360,{0}^ADN,18,10^FDOld^FS" +
                "^FO440,{0}^ADN,18,10^FDSold^FS" +
                "^FO520,{0}^ADN,18,10^FDCount^FS" +
                "^FO600,{0}^ADN,18,10^FDDeliv^FS" +
                "^FO700,{0}^ADN,18,10^FDNew^FS");

            linesTemplates.Add(BlackStoneContractLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO360,{0}^ADN,18,10^FD{2}^FS" +
                "^FO440,{0}^ADN,18,10^FD{3}^FS" +
                "^FO520,{0}^ADN,18,10^FD{4}^FS" +
                "^FO600,{0}^ADN,18,10^FD{5}^FS" +
                "^FO700,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(BlackStoneContractSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO360,{0}^ADN,18,10^FD_____^FS" +
                "^FO440,{0}^ADN,18,10^FD_____^FS" +
                "^FO520,{0}^ADN,18,10^FD_____^FS" +
                "^FO600,{0}^ADN,18,10^FD______^FS" +
                "^FO700,{0}^ADN,18,10^FD_______^FS");

            linesTemplates.Add(BlackStoneText, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStoneTotals, "^FO40,{0}^ADN,18,10^FD{1}: {2}^FS");

            linesTemplates.Add(BlackStonePaymentHeader, "^FO60,{0}^ADN,18,10^FDType^FS" +
                "^FO300,{0}^ADN,18,10^FDAmount^FS" +
                "^FO440,{0}^ADN,18,10^FDDescription^FS");

            linesTemplates.Add(BlackStonePaymentLine, "^FO60,{0}^ADN,18,10^FD-{1}^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO440,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(BlackStonePreviousBalance, "^FO40,{0}^ADN,18,10^FDPrevious Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStoneAfterDisc, "^FO40,{0}^ADN,18,10^FDToday Sales After Disc:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStonePaymentSep, "^FO240,{0}^ADN,18,10^FD___________________^FS");

            linesTemplates.Add(BlackStoneTotalDue, "^FO200,{0}^ADN,18,10^FDTotal Due:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStonePaymentTotal, "^FO40,{0}^ADN,18,10^FDPayments Total:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStoneNewBalance, "^FO160,{0}^ADN,18,10^FDNew Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BlackStonePrintedOn, "^FO40,{0}^ADN,18,10^FDReport printed on: {1}^FS");

            linesTemplates.Add(BlackStoneSignature, "^FO200,{0}^ADN,18,10^FDSignature: ------------------------------------^FS");

            linesTemplates.Add(BlackStoneFinalized, "^FO250,{0}^ADN,36,20^FD{1}^FS");
        }

        public override bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false)
        {
            List<string> lines = new List<string>();
            int startIndex = 80;

            if (asPreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneFinalized], startIndex, "NOT AN INVOICE"));
                startIndex += 40;
            }

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

            lines.AddRange(GetCompanyRows(ref startIndex, order));

            startIndex += fontSize * 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneAgentInfo], startIndex, Config.VendorName));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneConsignment], startIndex, order.PrintedOrderId));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneMerchant], startIndex, order.Client.ClientName));
            startIndex += fontSize;

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneAddress], startIndex, s1.Trim()));
                startIndex += fontSize;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneText], startIndex, "Phone: " + order.Client.ContactPhone));
            startIndex += fontSize;

            DateTime last = order.Client.LastVisitedDate;
            if (last == DateTime.MinValue)
                last = DateTime.Now;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneLastTimeVisited], startIndex, last.ToString()));
            startIndex += 60;

            lines.AddRange(GetCountLines(ref startIndex, order));

            lines.AddRange(GetContractLines(ref startIndex, order));

            startIndex += 70;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneText], startIndex, "I accept the new consignment balance of"));
            startIndex += fontSize;

            float totalNew = 0;
            double totalNewCost = 0;

            float totalPicked = 0;
            double totalPickedCost = 0;

            foreach (var item in order.Details)
            {
                var newCons = item.ConsignmentUpdated ? item.ConsignmentNew : item.ConsignmentOld;

                totalNew += newCons;
                totalNewCost += (newCons * item.ConsignmentNewPrice);

                totalPicked += item.ConsignmentPicked;
                totalPickedCost += (item.ConsignmentPicked * item.Price);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneTotals], startIndex, "Consignment Qty", totalNew.ToString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneTotals], startIndex, "Consignment Amount", totalNewCost.ToCustomString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneTotals], startIndex, "Delivered Qty", totalPicked.ToString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneTotals], startIndex, "Delivered Amount", totalPickedCost.ToCustomString()));

            if (!asPreOrder)
            {
                startIndex += 60;
                lines.AddRange(GetPaymentLines(ref startIndex, order));

            }

            startIndex += 60;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneText], startIndex, "*** NOTE: THIS IS A STATEMENT COPY ***"));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePrintedOn], startIndex, DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            startIndex += 60;

            // add the signature
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startIndex));

                startIndex += fontSize;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += fontSize;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));

                startIndex += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startIndex, order.SignatureName ?? string.Empty));
                    startIndex += fontSize;
                }
                startIndex += fontSize;
            }
            else
            {
                startIndex += 140;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += fontSize;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
                startIndex += fontSize;
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

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCompanyInfo], startIndex, company.CompanyName, "Date: " + order.Date.ToString()));
                startIndex += fontSize;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCompanyInfo], startIndex, company.CompanyAddress1, ""));
                startIndex += fontSize;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCompanyInfo], startIndex, company.CompanyAddress2, ""));
                    startIndex += fontSize;
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneText], startIndex, "Phone: " + company.CompanyPhone));
                startIndex += fontSize;

                //list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCompanyInfo], startIndex, "http://www.blackstoneonline.com", ""));
                //startIndex += fontSize;

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected List<string> GetCountLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneSectionName], startIndex, "PRODUCTS SOLD"));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountHeader1], startIndex));
            startIndex += fontSize;

            double totalQty = 0;
            double totalDue = 0;

            foreach (var item in SortDetails.SortedDetails(order.Details))
            {
                //if (item.Qty == 0)
                //    continue;

                totalQty += item.Qty;
                totalDue += (item.Qty * item.Price);

                var productSlices = SplitProductName(item.Product.Name, 31, 31);

                int offset = 0;
                foreach (var p in productSlices)
                {
                    if (offset == 0)
                    {
                        var totalLine = item.Qty * item.Price;
                        

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountLine], startIndex, p,
                            item.Qty, item.Price.ToCustomString(), totalLine.ToCustomString()));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountLine], startIndex, p,
                            "", "", ""));

                    startIndex += fontSize;
                    offset++;
                }

                startIndex += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountLine], startIndex, "                    Totals: ", totalQty, "",
                totalDue.ToCustomString()));
            startIndex += 40;

            //lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneCountTotal], startIndex, totalDue.ToCustomString()));
            //startIndex += 50;

            startIndex += 30;

            return lines;
        }

        private List<string> GetContractLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneSectionName], startIndex, "PRODUCTS BALANCE"));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneContractHeader], startIndex));
            startIndex += fontSize;

            float totalOld = 0;
            float totalSold = 0;
            float totalCounted = 0;
            float totalPicked = 0;
            float totalnew = 0;

            foreach (var item in SortDetails.SortedDetails(order.Details))
            {
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
                        totalPicked += item.ConsignmentPicked;
                        totalnew += consNew;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneContractLine], startIndex, p,
                            item.ConsignmentOld, item.Qty, item.ConsignmentCount, item.ConsignmentPicked, consNew));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneContractLine], startIndex, p,
                            "", "", "", "", ""));

                    startIndex += fontSize;
                    offset++;
                }

                startIndex += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneContractSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneContractLine], startIndex, "", totalOld, totalSold,
                totalCounted, totalPicked, totalnew));
            startIndex += 40;

            return lines;
        }

        private List<string> GetPaymentLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneSectionName], startIndex, "PAYMENTS"));
                startIndex += fontSize;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePaymentHeader], startIndex));
                startIndex += fontSize;

                foreach (var item in payments)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePaymentLine], startIndex, item.PaymentMethod,
                        item.Amount.ToCustomString(), item.Ref));
                    startIndex += fontSize;
                }
            }

            startIndex += 60;

            double clientBalance = order.Client.OpenBalance;
            double totalCost = order.OrderTotalCost();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePreviousBalance], startIndex, clientBalance.ToCustomString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneAfterDisc], startIndex, totalCost.ToCustomString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePaymentSep], startIndex));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneTotalDue], startIndex, (clientBalance + totalCost).ToCustomString()));
            startIndex += fontSize;

            startIndex += 40;

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePaymentTotal], startIndex, (paid * (-1)).ToCustomString()));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStonePaymentSep], startIndex));
            startIndex += fontSize;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[BlackStoneNewBalance], startIndex, (clientBalance + totalCost - paid).ToCustomString()));
            startIndex += fontSize;


            return lines;
        }
    }
}