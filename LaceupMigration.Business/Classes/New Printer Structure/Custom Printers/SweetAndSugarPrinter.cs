using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SweetAndSugarPrinter : ZebraThreeInchesPrinter1
    {
        const string SweetDocNameAndNumber = "SweetDocNameAndNumber";
        const string SweetDocDate = "SweetDocDate";
        const string SweetDocTerms = "SweetDocTerms";
        const string SweetProductName = "SweetProductName";
        const string SweetProductHeader = "SweetProductHeader";
        const string SweetProductDetail = "SweetProductDetail";
        const string SweetTotalQty = "SweetTotalQty";
        const string SweetTotalAmount = "SweetTotalAmount";
        const string SweetLinesUpcBarcode = "SweetLinesUpcBarcode";
        const string SweetComment = "SweetComment";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[CompanyName] = "^FO170,{0}^A0N,25,25^FD{1}^FS";
            linesTemplates[CompanyAddress] = "^FO100,{0}^FB400,5,0,C^ADN,18,10^FD{1}^FS";
            linesTemplates[CompanyPhone] = "^FO100,{0}^FB400,5,0,C^ADN,18,10^FD{1}^FS";
            linesTemplates[CompanyFax] = "^FO100,{0}^FB400,5,0,C^ADN,18,10^FD{1}^FS";
            linesTemplates[CompanyEmail] = "^FO100,{0}^FB400,5,0,C^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderClientName] = "^FO15,{0}^ADN,18,10^FD{1}^FS";

            linesTemplates.Add(SweetProductName, "^FO15,{0}^A0N,25,18^FD{1}^FS");
            linesTemplates.Add(SweetProductHeader, "^FO15,{0}^ADN,18,10^FDQuantity^FS" + "^FO250,{0}^ADN,18,10^FDUnit Price^FS" + "^FO480,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(SweetProductDetail, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO250,{0}^ADN,18,10^FD{2}^FS" + "^FO480,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(SweetTotalQty, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(SweetTotalAmount, "^FO15,{0}^A0N,35,35^FDTotal^FS" + "^FO250,{0}^FB300,1,0,R^A0N,35,35^FD{1}^FS");
            linesTemplates.Add(SweetLinesUpcBarcode, "^FO30,{0}^BUN,30^FD{1}^FS");
            linesTemplates.Add(SweetComment, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(SweetDocDate, "^FO15,{0}^ADN,18,10^FDInvoice Date: {1}^FS");
            linesTemplates.Add(SweetDocTerms, "^FO15,{0}^ADN,18,10^FDTerms: {1}^FS");
            linesTemplates.Add(SweetDocNameAndNumber, "^FO15,{0}^ADN,18,10^FDInvoice #: {1}^FS");
        }

        //LOGO
        public string GetLogoLabel(ref int startY)
        {
            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO100," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;

                startY += Config.CompanyLogoHeight;

                return label;
            }

            return string.Empty;
        }

        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (order.CompanyId > 0)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();
                foreach (string part in SplitProductName(company.CompanyName, 25, 25))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                    startIndex += 26;
                }

                startIndex += font18Separation;

                if (company.CompanyAddress1.Trim().Length > 0)
                {
                    foreach (string part in SplitProductName(company.CompanyAddress1, 25, 25))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, part));
                        startIndex += 26;
                    }
                }

                //startIndex += font18Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    foreach (string part in SplitProductName(company.CompanyAddress1, 25, 25))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress2));
                        startIndex += 26;
                    }
                }

                if (!string.IsNullOrEmpty(company.CompanyPhone))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startIndex, company.CompanyPhone));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, company.CompanyFax));
                    startIndex += 26;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += 26;
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }


        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            string toPrint = string.Empty;

            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, 36, 1);

            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetDocNameAndNumber], startY, order.PrintedOrderId));
                startY += 26;
            }

            if (order.Date > DateTime.MinValue)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetDocDate], startY, order.Date.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture)));
                startY += 26;
            }

            if (!string.IsNullOrEmpty(order.Term))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetDocTerms], startY, order.Term));
                startY += 26;
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            //client
            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                foreach (string part in SplitProductName(clientSplit, 25, 25))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, part));
                    startY += 26;
                }
            }
            foreach (string s in ClientAddress(order.Client))
            {
                foreach (string part in SplitProductName(s, 25, 25))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, part.Trim()));
                    startY += 26;
                }
            }

            if (!string.IsNullOrEmpty(order.Client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, order.Client.ContactPhone));
                startY += 26;
            }

            startY += font18Separation;
            var t = string.Empty;
            t = new string('-', WidthForNormalFont - t.Length) + t;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, t));
            startY += font18Separation;


            //Products

            float totalqty = 0;
            double totalamount = 0;

            if (salesLines.Count > 0)
            {
                foreach (var item in salesLines)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductName], startY, item.Value.Product.Name));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductHeader], startY));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductDetail], startY, item.Value.Qty.ToString("F2"), Math.Round(item.Value.Price, Config.Round).ToString("F2"), Math.Round(item.Value.Qty * item.Value.Price, Config.Round).ToString("F2")));
                    startY += font18Separation;

                    if (!string.IsNullOrEmpty(item.Value.Product.Upc))
                    {
                        var upc = item.Value.Product.Upc.Trim();
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetLinesUpcBarcode], startY, upc));
                        startY += font36Separation;
                        startY += font36Separation;
                    }

                    var r = string.Empty;
                    r = new string('-', WidthForNormalFont - r.Length) + r;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, r));
                    startY += font18Separation;

                    totalqty += item.Value.Qty;
                    totalamount += Math.Round(item.Value.Qty * item.Value.Price, Config.Round);
                }
            }


            if (creditLines.Count > 0)
            {
                foreach (var item in creditLines)
                {
                    var qty = item.Value.Qty;
                    var price = item.Value.Price * -1;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductName], startY, "Credit"));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductHeader], startY));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductDetail], startY, qty.ToString("F2"), Math.Round(price, Config.Round).ToString("F2"), Math.Round(qty * price, Config.Round).ToString("F2")));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductName], startY, item.Value.Product.Name));
                    startY += font18Separation;

                    if (!string.IsNullOrEmpty(item.Value.Product.Upc))
                    {
                        var upc = item.Value.Product.Upc.Trim();
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetLinesUpcBarcode], startY, upc));
                        startY += font36Separation;
                        startY += font36Separation;
                    }

                    var r = string.Empty;
                    r = new string('-', WidthForNormalFont - r.Length) + r;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, r));
                    startY += font18Separation;

                    totalqty += item.Value.Qty;
                    totalamount += Math.Round(qty * price, Config.Round);
                }
            }

            if (returnsLines.Count > 0)
            {
                foreach (var item in returnsLines)
                {
                    var qty = item.Value.Qty;
                    var price = item.Value.Price * -1;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductName], startY, "Return"));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductHeader], startY));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductDetail], startY, qty.ToString("F2"), Math.Round(price, Config.Round).ToString("F2"), Math.Round(qty * price, Config.Round).ToString("F2")));
                    startY += font18Separation;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetProductName], startY, item.Value.Product.Name));
                    startY += font18Separation;

                    if (!string.IsNullOrEmpty(item.Value.Product.Upc))
                    {
                        var upc = item.Value.Product.Upc.Trim();
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetLinesUpcBarcode], startY, upc));
                        startY += font36Separation;
                        startY += font36Separation;
                    }

                    var r = string.Empty;
                    r = new string('-', WidthForNormalFont - r.Length) + r;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, r));
                    startY += font18Separation;

                    totalqty += item.Value.Qty;
                    totalamount += Math.Round(qty * price, Config.Round);

                }
            }

            //TOTALS
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetTotalQty], startY, "Total Quantity"));
            startY += 26;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetTotalQty], startY, totalqty.ToString("F2")));
            startY += 26;

            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetTotalAmount], startY, Math.Round(totalamount, Config.Round).ToCustomString()));


            startY += font36Separation;
            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetComment], startY, "COMMENT"));
            startY += 26;

            string comment = "We appreciate your business !!!!";

            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                comment = Config.BottomOrderPrintText;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SweetComment], startY, comment));

            lines.Add(linesTemplates[EndLabel]);

            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            toPrint += sb.ToString();

            PrintIt(toPrint);

            return true;
        }
    }
}