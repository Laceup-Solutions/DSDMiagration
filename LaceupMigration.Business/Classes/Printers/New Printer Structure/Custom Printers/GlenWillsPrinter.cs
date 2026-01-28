
using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkiaSharp;


namespace LaceupMigration
{
    public class GlenWillsPrinter : ZebraFourInchesPrinter1
    {
        protected const string ToltecaRouteNumberAndDate = "ToltecaRouteNumberAndDate";
        protected const string ToltecaSalesmanAndTime = "ToltecaSalesmanAndTime";

        protected const string ToltecaCompanyName = "ToltecaCompanyName";
        protected const string ToltecaCompanyAddress = "ToltecaCompanyAddress";
        protected const string ToltecaCompanyPhone = "ToltecaCompanyPhone";
        protected const string ToltecaCompanyText = "ToltecaCompanyText";

        protected const string IndependentCompanyName = "IndependentCompanyName";
        protected const string IndependentCompanyNameBig = "IndependentCompanyNameBig";
        protected const string IndependentCompanyAddress = "IndependentCompanyAddress";
        protected const string IndependentCompanyPhone = "IndependentCompanyPhone";
        protected const string IndependentCompanyText1 = "IndependentCompanyText1";
        protected const string IndependentCompanyText2 = "IndependentCompanyText2";

        protected const string ToltecaVendorNumber = "ToltecaVendorNumber";
        protected const string ToltecaInnvoiceNumber = "ToltecaInnvoiceNumber";
        protected const string ToltecaClientId = "ToltecaClientId";
        protected const string ToltecaBillTo = "ToltecaBillTo";
        protected const string ToltecaBillTo2 = "ToltecaBillTo2";
        protected const string ToltecaShipTo = "ToltecaShipTo";
        protected const string ToltecaShipTo2 = "ToltecaShipTo2";
        protected const string ToltecaTerms = "ToltecaTerms";
        protected const string ToltecaNotFinal = "ToltecaNotFinal";
        protected const string ToltecaSectionName = "ToltecaSectionName";
        protected const string ToltecaTableHeader1 = "ToltecaTableHeader1";
        protected const string ToltecaTableHeader2 = "ToltecaTableHeader2";
        protected const string ToltecaTableLine = "ToltecaTableLine";
        protected const string ToltecaTableSectionTotalQty = "ToltecaTableSectionTotalQty";
        protected const string ToltecaTableSectionSubtotal = "ToltecaTableSectionSubtotal";
        protected const string ToltecaTableDisountTotal = "ToltecaTableDisountTotal";
        protected const string ToltecaTableSrpTotal = "ToltecaTableSrpTotal";
        protected const string ToltecaPaymentType = "ToltecaPaymentType";
        protected const string ToltecaTotals = "ToltecaTotals";
        protected const string ToltecaText = "ToltecaText";
        protected const string ToltecaClientName = "ToltecaClientName";
        protected const string ToltecaBillToHeader = "ToltecaBillToHeader";
        protected const string ToltecaShipToHeader = "ToltecaShipToHeader";
        protected const string ToltecaPaymentLine = "ToltecaPaymentLine";
        protected const string ToltecaPaymentHeader = "ToltecaPaymentHeader";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(ToltecaRouteNumberAndDate, "^FO40,{0}^ADN,18,10^FDRoute #: {1}^FS^FO600,{0}^ADN,18,10^FDDate: {2}^FS");
            linesTemplates.Add(ToltecaSalesmanAndTime, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS^FO600,{0}^ADN,18,10^FD      {2}^FS");

            linesTemplates.Add(ToltecaCompanyName, "^FO45,{0}^A0N,65,60^FB730,1,0,C^FD{1}^FS");
            linesTemplates.Add(ToltecaCompanyAddress, "^CF0,30^FO45,{0}^FB730,1,0,C^FD{1} {2}^FS");
            linesTemplates.Add(ToltecaCompanyPhone, "^CF0,30^FO40,{0}^FB730,1,0,C^FDToll Free: {1}^FS");
            linesTemplates.Add(ToltecaCompanyText, "^CF0,30^FO250,{0}^FB730,1,0,C^FD{1}^FS");

            linesTemplates.Add(IndependentCompanyName, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyNameBig, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyAddress, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyPhone, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyText1, "^FO40,{0}^ADN,18,10^FD             AUTHORIZED EXCLUSIVE DISTRIBUTOR OF^FS");
            linesTemplates.Add(IndependentCompanyText2, "^FO40,{0}^ADN,18,10^FD                  CANDIES TOLTECA PRODUCTS^FS");

            linesTemplates.Add(ToltecaVendorNumber, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ToltecaInnvoiceNumber, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ToltecaClientName, "^FO40,{0}^ADN,36,20^FB730,1,0,C^FD{1}^FS");

            linesTemplates.Add(ToltecaClientId, "^FO40,{0}^ADN,18,10^FDClient ID: {1}^FS");

            linesTemplates.Add(ToltecaBillToHeader, "^FO40,{0}^ADN,18,10^FDBill to:^FS");
            linesTemplates.Add(ToltecaShipToHeader, "^FO400,{0}^ADN,18,10^FDShip to:^FS");
            linesTemplates.Add(ToltecaBillTo, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ToltecaBillTo2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ToltecaShipTo, "^FO400,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ToltecaShipTo2, "^FO400,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ToltecaTerms, "^FO40,{0}^ADN,18,10^FD    Terms: {1}^FS");
            linesTemplates.Add(ToltecaNotFinal, "^FO40,{0}^ADN,36,20^FDNOT AN INVOICE^FS");

            linesTemplates.Add(ToltecaSectionName, "^CF0,30^FO360,{0}^FD{1}^FS");
            linesTemplates.Add(ToltecaTableHeader1, "^CF0,20" +
                "^FO40,{0}^FDQTY^FS" +
                "^FO100,{0}^FDDESCRIPTION^FS" +
                "^FO400,{0}^FDS.PRICE^FS" +
                "^FO500,{0}^FDDISC.^FS" +
                "^FO600,{0}^FDF.PRICE^FS" +
                "^FO700,{0}^FDAMT.^FS");

            linesTemplates.Add(ToltecaPaymentLine, "^CF0,35" +
          "^FO40,{0}^FB730,1,0,R^FD{1}^FS");

            linesTemplates.Add(ToltecaPaymentHeader, "^CF0,40" +
              "^FO40,{0}^FB730,1,0,R^FD{1}^FS");

            linesTemplates.Add(ToltecaTableHeader2, "^CF0,20" +
                "^FO40,{0}^FD^FS" +
                "^FO100,{0}^FDUPC^FS" +
                "^FO400,{0}^FD^FS" +
                "^FO500,{0}^FD^FS" +
                "^FO600,{0}^FD^FS" +
                "^FO700,{0}^FD^FS");

            linesTemplates.Add(ToltecaTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO100,{0}^ADN,18,10^FD{2}^FS" +
                "^FO400,{0}^ADN,18,10^FD{3}^FS" +
                "^FO500,{0}^ADN,18,10^FD{4}^FS" +
                "^FO600,{0}^ADN,18,10^FD{5}^FS" +
                "^FO700,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates[OrderDetailsLinesUpcText] = "^FO100,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[Upc128] = "^FO100,{0}^BCN,40^FD{1}^FS";
            linesTemplates[OrderDetailsLinesUpcBarcode] = "^FO100,{0}^BUN,40^FD{1}^FS";

            linesTemplates.Add(ToltecaTableSectionTotalQty, "^CF0,25" +
                "^FO500,{0}^FDTotal Qty: ^FS" +
                "^FO700,{0}^FD{1}^FS");

            linesTemplates.Add(ToltecaTableSectionSubtotal, "^CF0,25" +
                "^FO500,{0}^FD{1} Subtotal: ^FS" +
                "^FO700,{0}^FD{2}^FS");

            linesTemplates.Add(ToltecaTableDisountTotal, "^CF0,25" +
                "^FO500,{0}^FDDisc. Total: ^FS" +
                "^FO700,{0}^FD{1}^FS");

            linesTemplates.Add(ToltecaTableSrpTotal, "^CF0,25" +
                "^FO500,{0}^FDSRP. Total: ^FS" +
                "^FO700,{0}^FD{1}^FS");

            linesTemplates.Add(ToltecaPaymentType, "^CF0,25" +
                "^FO500,{0}^FD{1}^FS" +
                "^FO700,{0}^FD{2}^FS");

            linesTemplates.Add(ToltecaTotals, "^CF0,35" +
                "^FO250,{0}^FD{1}^FS" +
                "^FO650,{0}^FD{2}^FS");

            linesTemplates.Add(ToltecaText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
        }



        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            bool isTolteca = CompanyInfo.Companies.Count > 1 && CompanyInfo.Companies[0].CompanyName == CompanyInfo.Companies[1].CompanyName;
            string terms = order.Term;

            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetInvoiceHeader(ref startY, asPreOrder, order, isTolteca, terms));

            startY += 25;

            lines.AddRange(GetInvoiceDetails(ref startY, order, salesLines, creditLines, returnsLines));

            lines.AddRange(GetPaymentSection(ref startY, order));

            lines.AddRange(GetTotalsSection(ref startY, order, salesLines, creditLines, returnsLines));

            startY += 25;

            lines.AddRange(GetFooterRows(ref startY, order, isTolteca, terms));


            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);

        }

        IEnumerable<string> GetInvoiceHeader(ref int startY, bool asPreorder, Order order, bool isTolteca, string terms)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaRouteNumberAndDate],
                startY, Config.SalesmanId.ToString(), DateTime.Now.ToShortDateString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaSalesmanAndTime],
                startY, Config.VendorName, DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            if (isTolteca)
            {
                lines.AddRange(GetToltecaHeader(ref startY, asPreorder, order));
                startY += 25;
            }
            else
            {
                if (terms != "CASH" && terms != "CHARGE")
                {
                    lines.AddRange(GetToltecaHeader(ref startY, asPreorder, order));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                    startY += font18Separation;
                }
            }

            string vendorNumber = UDFHelper.GetSingleUDF("VENDOR #", order.Client.ExtraPropertiesAsString);
            if (!string.IsNullOrEmpty(vendorNumber))
            {
                vendorNumber = "Vendor #: " + vendorNumber;
                if (WidthForBoldFont - vendorNumber.Length > 0)
                    vendorNumber = new string((char)32, (WidthForBoldFont - vendorNumber.Length) / 2) + vendorNumber;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaVendorNumber], startY, vendorNumber));
                startY += font36Separation;
            }

            var invoiceNumber = "Invoice #: " + order.PrintedOrderId;
            if (WidthForBoldFont - invoiceNumber.Length > 0)
                invoiceNumber = new string((char)32, (WidthForBoldFont - invoiceNumber.Length) / 2) + invoiceNumber;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaInnvoiceNumber], startY, invoiceNumber));
            startY += font36Separation;

            foreach (var c in SplitProductName(order.Client.ClientName, 25, 25))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaClientName], startY, c));
                startY += font36Separation;
            }

            startY += font18Separation;
            startY += font18Separation;

            var format = ToltecaBillTo;

            var address1Y = 0;
            var address2Y = 0;

            bool isFirst = true;
            foreach (string s11 in ClientAddress(order.Client, false))
            {
                if (format == ToltecaBillTo)
                    address1Y = startY;
                else
                    address2Y = startY;

                if (isFirst)
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaBillToHeader], address1Y - font18Separation));

                string clientName = s11;
                if (clientName.Length > 29)
                    clientName = clientName.Substring(0, 29);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, clientName));

                startY += font18Separation;

                isFirst = false;
                format = ToltecaBillTo2;
            }

            startY += font18Separation;

            format = ToltecaShipTo;

            isFirst = true;
            foreach (string s11 in ClientAddress(order.Client, true))
            {
                if (isFirst)
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaShipToHeader], address1Y - font18Separation));

                string clientName = s11;
                if (clientName.Length > 29)
                    clientName = clientName.Substring(0, 29);

                if (format == ToltecaShipTo)
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], address1Y, clientName));
                    startY += font18Separation;
                    address1Y += font18Separation;

                }
                else
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], address1Y, clientName));
                    startY += font18Separation;
                    address1Y += font18Separation;
                }

                isFirst = false;
                format = ToltecaShipTo2;
            }

            startY -= (font18Separation * 2);

            if (asPreorder)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FDNOT AN INVOICE^FS", startY));
                startY += font36Separation;
            }

            return lines;
        }

        private IEnumerable<string> GetToltecaHeader(ref int startY, bool asPreorder, Order order)
        {
            List<string> lines = new List<string>();

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            CompanyInfo company = null;

            if (CompanyInfo.Companies.Count == 0)
                return new List<string>();
            if (order.CompanyId > 0)
                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
            if (company == null)
                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
            if (company == null)
                company = CompanyInfo.GetMasterCompany();

            if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                company.CompanyName = order.CompanyName;


            AddExtraSpace(ref startY, lines, 36, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyName], startY, company.CompanyName));
            startY += font36Separation;
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyAddress], startY, company.CompanyAddress1, CompanyInfo.Companies[0].CompanyAddress2));
            startY += font18Separation;
            startY += 10;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyPhone], startY, company.CompanyPhone));
            startY += 2 * font18Separation;

            return lines;
        }

        private IEnumerable<string> GetIndependendHeaders(ref int startY, bool asPreorder, Order order)
        {
            List<string> lines = new List<string>();

            startY += 10;

            var terms = order.Term;

            string s = CompanyInfo.Companies[1].CompanyName;

            bool printedName = false;

            if (!string.IsNullOrEmpty(terms))
            {
                if (terms.ToUpper() != "CASH" && terms.ToUpper() != "CHARGE" && CompanyInfo.Companies[0].CompanyName != CompanyInfo.Companies[1].CompanyName)
                {
                    if (WidthForNormalFont - s.Length > 0)
                        s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyName], startY, s));
                    startY += font18Separation;
                    printedName = true;
                }
            }

            if (!printedName)
            {
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyNameBig], startY, s));
                startY += font36Separation;
            }

            s = CompanyInfo.Companies[1].CompanyAddress1 + " " + CompanyInfo.Companies[1].CompanyAddress2;
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyAddress], startY, s));
            startY += font18Separation;

            s = CompanyInfo.Companies[1].CompanyPhone;
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyPhone], startY, s));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyText1], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IndependentCompanyText2], startY));
            startY += font18Separation;

            startY += font36Separation;

            return lines;
        }

        private IEnumerable<string> GetInvoiceDetails(ref int startY, Order order, Dictionary<string, OrderLine> salesLines, Dictionary<string, OrderLine> creditLines, Dictionary<string, OrderLine> returnsLines)
        {
            List<string> lines = new List<string>();

            if (salesLines.Count > 0)
            {
                lines.AddRange(GetInvoiceSectionTable(ref startY, order, salesLines, "Sales", 1));
                startY += 50;
            }

            if (creditLines.Count > 0)
            {
                lines.AddRange(GetInvoiceSectionTable(ref startY, order, creditLines, "Damaged", -1));
                startY += 50;
            }

            if (returnsLines.Count > 0)
            {
                lines.AddRange(GetInvoiceSectionTable(ref startY, order, returnsLines, "Returns", -1));
                startY += 50;
            }

            return lines;
        }

        private IEnumerable<string> GetInvoiceSectionTable(ref int startY, Order order, Dictionary<string, OrderLine> sectionLines, string sectionName, int factor)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaSectionName], startY, sectionName));
            startY += 35;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableHeader2], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double subtotal = 0;
            double srpTotal = 0;
            double discTotal = 0;
            float totalQty = 0;

            foreach (var detail in SortDetails.SortedDetails(sectionLines.Values.ToList()))
            {
                Product p = detail.Product;

                int productLineOffset = 0;
                string description = p.Name.Replace(p.Code, string.Empty);

                if (Config.DefaultItem > 0 && detail.Product.ProductId == Config.DefaultItem)
                    description = p.Name;

                string code = !string.IsNullOrEmpty(p.Code) ? p.Code : string.Empty;

                double listPrice = detail.Product.PriceLevel0;
                double regularprice = detail.Price;

                double discount = 0;

                bool isCredit = false;

                if (detail.OrderDetail != null && detail.OrderDetail.IsCredit)
                    isCredit = true;

                bool showDiscount = false;
                var pricelevel = Product.GetProductPriceForProduct(order, order.Client, detail.Product);
                if (pricelevel != null)
                {
                    var pp = PriceLevel.List.FirstOrDefault(x => x.Id == pricelevel.PriceLevelId);

                    if (pp != null && pp.ExtraFields.Contains("showdiscount"))
                        showDiscount = true;
                }

                int f = 1;
                double d = 0;

                if (showDiscount)
                {
                    if (regularprice < listPrice && !isCredit)
                        discount = (regularprice - listPrice);

                    d = (detail.Qty * regularprice);

                    subtotal += d;
                    srpTotal += (regularprice * detail.Qty);

                    totalQty += detail.Qty;

                    if (isCredit)
                    {
                        f = -1;
                        listPrice = Math.Abs(detail.Price);
                    }
                    else
                        if (listPrice < detail.Price)
                        listPrice = detail.Price;
                }
                else
                {
                    d = (detail.Qty * regularprice);

                    subtotal += d;
                    srpTotal += (regularprice * detail.Qty);


                    totalQty += detail.Qty;

                    if (isCredit)
                    {
                        f = -1;
                        listPrice = Math.Abs(detail.Price);
                    }
                    else
                        listPrice = detail.Price;

                }

                foreach (string pName in GetSectionRowSplitProductName(description))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableLine], startY,
                            detail.Qty,
                            pName,
                            listPrice.ToCustomString(),
                            discount.ToCustomString(),
                            (regularprice * f).ToCustomString(),
                            (d * f).ToCustomString()));
                        startY += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                lines.AddRange(GetUpcForProductInOrder(ref startY, order, p));

                startY += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableSectionTotalQty], startY,
                totalQty.ToString()));
            startY += font18Separation;

            return lines;
        }

        private IEnumerable<string> GetSectionRowSplitProductName(string name)
        {
            return SplitProductName(name, 23, 23);
        }

        private double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        private IEnumerable<string> GetPaymentSection(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();

            if (payments.Count == 0)
                return lines;

            var paidInFull = payments != null && payments.Sum(x => x.Amount) == order.OrderTotalCost();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaSectionName], startY, "Payment Section"));
            startY += 35;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;


            if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
            {
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            startY += 20;


            return lines;
        }

        protected override IEnumerable<string> GetPaymentLines(ref int startY, IList<PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();

            if (paidInFull)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Paid in Full"));
                startY += font20Separation;
                startY += 20;

            }

            foreach (var p in payments)
            {
                switch (p.PaymentMethod)
                {
                    case InvoicePaymentMethod.Cash:
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Method (Cash): " + ToString(p.Amount)));
                        startY += font20Separation;
                        startY += 20;
                        break;
                    case InvoicePaymentMethod.Check:
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Method (Check): " + ToString(p.Amount)));
                        startY += font20Separation;
                        startY += 20;

                        break;
                    case InvoicePaymentMethod.Money_Order:
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Method (Money Order): " + ToString(p.Amount)));
                        startY += font20Separation;
                        startY += 20;

                        break;
                    case InvoicePaymentMethod.Credit_Card:
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Method (Credit Card): " + ToString(p.Amount)));
                        startY += font20Separation;
                        startY += 20;

                        break;
                }
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentLine], startY, "Total Paid: " + ToString(payments.Sum(x => x.Amount))));
            startY += font20Separation;
            startY += 20;

            return lines;
        }


        private IEnumerable<string> GetTotalsSection(ref int startY, Order order, Dictionary<string, OrderLine> salesLines, Dictionary<string, OrderLine> creditLines, Dictionary<string, OrderLine> returnsLines)
        {
            List<string> lines = new List<string>();

            var salesSub = GetSectionSubtotal(salesLines, order, 1);
            var damagedReturns = GetSectionSubtotal(creditLines, order, -1) + GetSectionSubtotal(returnsLines, order, -1);

            double totalDiscount = 0;

            foreach (var d in order.Details)
            {
                if (!d.IsCredit && !d.IsFreeItem)
                {
                    bool showDiscount = false;
                    var pricelevel = Product.GetProductPriceForProduct(order, order.Client, d.Product);
                    if (pricelevel != null)
                    {
                        var pp = PriceLevel.List.FirstOrDefault(x => x.Id == pricelevel.PriceLevelId);

                        if (pp != null && pp.ExtraFields.Contains("showdiscount"))
                            showDiscount = true;
                    }

                    if (d.Price < d.Product.PriceLevel0 && showDiscount)
                        totalDiscount += ((d.Price - d.Product.PriceLevel0) * d.Qty);
                }
            }

            var invoiceTotal = order.OrderTotalCost();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "                Sales Subtotal:", salesSub.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "Damaged & Returns Total:", (damagedReturns * -1).ToCustomString()));
            startY += 40;

            var discount = order.CalculateDiscount();
            if (discount > 0)
                totalDiscount += discount;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "         Promotion Discount:", totalDiscount.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "                   Invoice Total:", invoiceTotal.ToCustomString()));
            startY += 40;

            return lines;
        }

        double GetSectionSubtotal(Dictionary<string, OrderLine> sectionLines, Order order, int factor)
        {
            double total = 0;

            foreach (var detail in sectionLines.Values)
            {
                if (detail.OrderDetail != null && detail.OrderDetail.IsCredit)
                {
                    total += (detail.Price * detail.Qty);
                    continue;
                }

                bool showDiscount = false;
                var pricelevel = Product.GetProductPriceForProduct(order, order.Client, detail.Product);
                if (pricelevel != null)
                {
                    var pp = PriceLevel.List.FirstOrDefault(x => x.Id == pricelevel.PriceLevelId);

                    if (pp != null && pp.ExtraFields.Contains("showdiscount"))
                        showDiscount = true;
                }

                if (showDiscount)
                {
                    var listprice = detail.Product.PriceLevel0;

                    if (listprice < detail.Price)
                        listprice = detail.Price;

                    total += listprice * detail.Qty;
                }
                else
                    total += detail.Price * detail.Qty;
            }

            return total;
        }

        private IEnumerable<string> GetFooterRows(ref int startY, Order order, bool isTolteca, string terms)
        {
            List<string> lines = new List<string>();

            string s = string.Empty;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));

                s = "--------------------------------------------";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;

                // signature name

                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    s = order.SignatureName;
                    if (WidthForNormalFont - s.Length > 0)
                        s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                    startY += font18Separation;
                }
                s = "SIGNATURE";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }
            else
            {
                startY += font18Separation * 8;

                s = "--------------------------------------------";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;

                s = "SIGNATURE";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }


            return lines;

        }

        public override string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            string signatureAsString;
            signatureAsString = order.ConvertSignatureToBitmap();
            using SKBitmap signature = SKBitmap.Decode(signatureAsString);

            var converter = new BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature);
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width + 31) / 32) * 32 / 8;
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

    }
}