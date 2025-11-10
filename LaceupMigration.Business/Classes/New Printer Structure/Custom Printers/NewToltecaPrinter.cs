using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class NewToltecaPrinter : ZebraFourInchesPrinter1
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


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(ToltecaRouteNumberAndDate, "^FO40,{0}^ADN,18,10^FDRoute #: {1}^FS^FO600,{0}^ADN,18,10^FDDate: {2}^FS");
            linesTemplates.Add(ToltecaSalesmanAndTime, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS^FO600,{0}^ADN,18,10^FD      {2}^FS");

            linesTemplates.Add(ToltecaCompanyName, "^FO45,{0}^ADN,36,20^FD       {1}^FS");
            linesTemplates.Add(ToltecaCompanyAddress, "^CF0,25^FO45,{0}^FD                          {1} {2}^FS");
            linesTemplates.Add(ToltecaCompanyPhone, "^CF0,25^FO40,{0}^FD                       Toll Free: {1}^FS");
            linesTemplates.Add(ToltecaCompanyText, "^CF0,25^FO250,{0}^FD{1}^FS");

            linesTemplates.Add(IndependentCompanyName, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyNameBig, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyAddress, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyPhone, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(IndependentCompanyText1, "^FO40,{0}^ADN,18,10^FD             AUTHORIZED EXCLUSIVE DISTRIBUTOR OF^FS");
            linesTemplates.Add(IndependentCompanyText2, "^FO40,{0}^ADN,18,10^FD                  CANDIES TOLTECA PRODUCTS^FS");

            linesTemplates.Add(ToltecaVendorNumber, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ToltecaInnvoiceNumber, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ToltecaClientId, "^FO40,{0}^ADN,18,10^FDClient ID: {1}^FS");
            linesTemplates.Add(ToltecaBillTo, "^FO40,{0}^ADN,18,10^FD  Bill to: {1}^FS");
            linesTemplates.Add(ToltecaBillTo2, "^FO40,{0}^ADN,18,10^FD           {1}^FS");
            linesTemplates.Add(ToltecaShipTo, "^FO40,{0}^ADN,18,10^FD  Ship to: {1}^FS");
            linesTemplates.Add(ToltecaShipTo2, "^FO40,{0}^ADN,18,10^FD           {1}^FS");
            linesTemplates.Add(ToltecaTerms, "^FO40,{0}^ADN,18,10^FD    Terms: {1}^FS");
            linesTemplates.Add(ToltecaNotFinal, "^FO40,{0}^ADN,36,20^FDNOT AN INVOICE^FS");

            linesTemplates.Add(ToltecaSectionName, "^CF0,30^FO360,{0}^FD{1}^FS");
            linesTemplates.Add(ToltecaTableHeader1, "^CF0,20" +
                "^FO40,{0}^FDQTY^FS" +
                "^FO100,{0}^FDDESCRIPTION^FS" +
                "^FO400,{0}^FDPRICE^FS" +
                "^FO500,{0}^FDDISC.^FS" +
                "^FO600,{0}^FDSRP.^FS" +
                "^FO700,{0}^FDAMT.^FS");

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
                "^FO40, {0}^FD{3}^FS" +
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

            var totalDiscount = order.CalculateDiscount();

            if (Config.ShowDiscountIfApplied)
            {
                if(totalDiscount > 0)
                    lines.AddRange(GetPromotionAndDiscounts(ref startY, order));
            }
            else 
                lines.AddRange(GetPromotionAndDiscounts(ref startY, order));

            lines.AddRange(GetPaymentSection(ref startY, order));

            lines.AddRange(GetTotalsSection(ref startY, order, salesLines, creditLines, returnsLines));

            startY += 25;

            lines.AddRange(GetFooterRows(ref startY, order, isTolteca, terms, fromBatch));


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
                lines.AddRange(GetIndependendHeaders(ref startY, asPreorder, order));
            }

            string vendorNumber = DataAccess.GetSingleUDF("VENDOR #", order.Client.ExtraPropertiesAsString);
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

            startY += font18Separation;

            var clientId = DataAccess.GetSingleUDF("Cust Id", order.Client.ExtraPropertiesAsString);
            if (!string.IsNullOrEmpty(clientId))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaClientId], startY, clientId));
                startY += font18Separation;
            }
            startY += font18Separation;

            var format = ToltecaBillTo;

            foreach (string s11 in ClientAddress(order.Client, false))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, s11));
                startY += font18Separation;

                format = ToltecaBillTo2;
            }

            startY += font18Separation;

            format = ToltecaShipTo;

            foreach (string s11 in ClientAddress(order.Client, true))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, s11));
                startY += font18Separation;

                format = ToltecaShipTo2;
            }

            startY += font18Separation;

            if (!string.IsNullOrEmpty(terms))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTerms], startY, terms));
                startY += font18Separation;
            }
            startY += font18Separation;

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

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyName], startY, CompanyInfo.Companies[0].CompanyName));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyAddress], startY, CompanyInfo.Companies[0].CompanyAddress1, CompanyInfo.Companies[0].CompanyAddress2));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaCompanyPhone], startY, CompanyInfo.Companies[0].CompanyPhone));
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
                string code = !string.IsNullOrEmpty(p.Code) ? p.Code : string.Empty;

                double price = detail.Product.PriceLevel0 * factor;
                double regularprice = Product.GetPriceForProduct(detail.Product, order.Client, true) * factor;
                var discount = price - regularprice;
                var retPrice = GetRetailPrice(p, order.Client) * factor;
                double d = (detail.Qty * price) - (detail.Qty * discount);

                subtotal += d;
                srpTotal += (retPrice * detail.Qty);
                discTotal += (Math.Abs(discount) * detail.Qty);

                totalQty += detail.Qty;

                foreach (string pName in GetSectionRowSplitProductName(description))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableLine], startY,
                            detail.Qty,
                            pName,
                            price.ToCustomString(),
                            discount.ToCustomString(),
                            retPrice.ToCustomString(),
                            d.ToCustomString()));
                        startY += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                lines.AddRange(GetUpcForProductInOrder(ref startY, order, p));

                if (discount != 0)
                {
                    double priceFromSpecial = 0;
                    if (Offer.ProductHasSpecialPriceForClient(detail.Product, order.Client, out priceFromSpecial))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, "^FO100,{0}^ADN,18,10^FD{1} Discount -{2}^FS", startY, price.ToCustomString(), Math.Abs(discount).ToCustomString()));
                        startY += font18Separation;
                    }
                }

                startY += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableSectionTotalQty], startY,
                totalQty.ToString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableSectionSubtotal], startY,
                sectionName,
                subtotal.ToCustomString()));
            startY += font18Separation;

            if (factor == 1)
            {
                if (srpTotal > 0)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableSrpTotal], startY,
                        srpTotal.ToCustomString()));
                    startY += font18Separation;
                }

                if (discTotal > 0)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTableDisountTotal], startY,
                        discTotal.ToCustomString()));
                    startY += font18Separation;
                }
            }

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

        private IEnumerable<string> GetPromotionAndDiscounts(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaSectionName], startY, "Promotion & Discount"));
            startY += 35;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            string comment = !string.IsNullOrEmpty(order.Comments) ? order.Comments : string.Empty;
            if (!string.IsNullOrEmpty(comment))
                comment += "\n";
            comment += !string.IsNullOrEmpty(order.DiscountComment) ? order.DiscountComment : string.Empty;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaText], startY, comment));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            startY += 20;

            return lines;
        }

        private IEnumerable<string> GetPaymentSection(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaSectionName], startY, "Payment Type"));
            startY += 35;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            var paid = payments != null && payments.Count > 0 ? payments.Sum(x => x.Amount) : 0;
            var charge = order.OrderTotalCost() - paid;

            if (payments != null)
            {
                if (paid > 0)
                {
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Cash))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "CASH:", paid.ToCustomString(), ""));
                        startY += 30;
                    }
                    else
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Check))
                    {
                        var checkNumbers = payments.Select(x => x.Ref);

                        var checkStrings = string.Join(",", checkNumbers);

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "", paid.ToCustomString(), "CHECK #: " + checkStrings));
                        startY += 30;
                    }
                    else
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Credit_Card))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "CREDIT CARD:", paid.ToCustomString(), ""));
                        startY += 30;
                    }
                    else
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Money_Order))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "MONEY ORDER:", paid.ToCustomString(), ""));
                        startY += 30;
                    }
                    else
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Transfer))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "TRANSFER:", paid.ToCustomString(), ""));
                        startY += 30;
                    }
                    else
                    if (payments.All(x => x.PaymentMethod == InvoicePaymentMethod.Zelle_Transfer))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "ZELLE TRANSFER:", paid.ToCustomString(), ""));
                        startY += 30;
                    }
                    else
                    {
                        foreach (var p in payments)
                        {
                            string checkn = "";
                            var part1 = p.PaymentMethod.ToString().Replace("_", "").ToUpperInvariant();
                            if (p.PaymentMethod == InvoicePaymentMethod.Check)
                            {
                                checkn = "CHECK#: " + p.Ref;
                                part1 = string.Empty;
                            }

                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, part1 , p.Amount.ToCustomString(), checkn));
                            startY += 30;
                        }

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                        startY += font18Separation;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "TOTAL:", paid.ToCustomString(), ""));
                        startY += 30;

                    }
                }
            }

            if (charge > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaPaymentType], startY, "CHARGE", charge.ToCustomString(), ""));
                startY += 30;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            startY += 20;

            return lines;
        }

        private IEnumerable<string> GetTotalsSection(ref int startY, Order order, Dictionary<string, OrderLine> salesLines, Dictionary<string, OrderLine> creditLines, Dictionary<string, OrderLine> returnsLines)
        {
            List<string> lines = new List<string>();

            var salesSub = GetSectionSubtotal(salesLines, order, 1);
            var damagedReturns = GetSectionSubtotal(creditLines, order, -1) + GetSectionSubtotal(returnsLines, order, -1);
            var totalDiscount = order.CalculateDiscount();
            var invoiceTotal = order.OrderTotalCost();

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            var paid = payments != null && payments.Count > 0 ? payments.Sum(x => x.Amount) : 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "                Sales Subtotal:", salesSub.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "Damaged & Returns Total:", damagedReturns.ToCustomString()));
            startY += 40;

            if (Config.ShowDiscountIfApplied)
            {
                if (totalDiscount > 0)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "         Promotion Discount:", totalDiscount.ToCustomString()));
                    startY += 40;
                }
            }
            else
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "         Promotion Discount:", totalDiscount.ToCustomString()));
                startY += 40;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "                   Invoice Total:", invoiceTotal.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ToltecaTotals], startY, "                         Balance:", (invoiceTotal - paid).ToCustomString()));
            startY += 40;

            return lines;
        }

        double GetSectionSubtotal(Dictionary<string, OrderLine> sectionLines, Order order, int factor)
        {
            double total = 0;

            foreach (var detail in sectionLines.Values)
            {
                double price = detail.Price * factor;
                double regularprice = Product.GetPriceForProduct(detail.Product, order.Client, false) * factor;
                var discount = regularprice - price;
                double d = (detail.Qty * regularprice) - (detail.Qty * discount);

                total += d * factor;
            }

            return total;
        }

        private IEnumerable<string> GetFooterRows(ref int startY, Order order, bool isTolteca, string terms, bool fromBatch)
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
                s = "CUSTOMER SIGNATURE";
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

                s = "CUSTOMER SIGNATURE";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }

            bool independCashCharge = !isTolteca && (terms == "CASH" || terms == "CHARGE");
            if (!independCashCharge)
            {
                startY += font18Separation;
                s = "SEND PAYMENT TO: CANDIES TOLTECA";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
                s = "P.O.BOX 4729 FRESNO CA. 93744";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }

            s = "All past accounts will be billed a service charge at an annual";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = " rate of 18% (1.5% MONTHLY) or $55.00 dls minimun charge. ";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = "Also we may charge $25.00 dls if your check is returned";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = "for any reason.";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;

            startY += font18Separation;

            bool shouldPrintBottomText = fromBatch;
            if (order.Finished)
                shouldPrintBottomText = true;

            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText) && shouldPrintBottomText)
            {
                //new way
                if(Config.BottomOrderPrintText.Contains("<nl>"))
                    lines.AddRange(GetHolpecaBottomPrint(ref startY));
                else
                {
                    foreach (var line in SplitProductName(Config.BottomOrderPrintText, 60, 60)) //29
                    {
                        s = line;
                        if (WidthForBoldFont - s.Length > 0)
                            s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s)); //"^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s));
                        startY += font18Separation;
                    }
                }
            }

            return lines;

        }
        

        public List<string> GetHolpecaBottomPrint(ref int startY)
        {
            List<string> lines = new List<string>();

            var originalLines = Config.BottomOrderPrintText.Split("<nl>");

            foreach (var oG in originalLines)
            {
                if (string.IsNullOrEmpty(oG))
                {
                    //newline
                    startY += font18Separation;
                    continue;
                }
                
                bool isBold = false;
                string s = oG;
                
                if (oG.Contains("<b>"))
                {
                    s = s.Replace("<b>", "");
                    isBold = true;
                }
                
                foreach (var line in SplitProductName(s, 60, 60))
                {
                    if (isBold)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, line));
                        lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, line));
                    }
                    
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, line));
                    startY += font18Separation;
                }
            }
            
            return lines;
        }
        
        protected override IEnumerable<string> GetFooterRows(ref int startIndex, bool asPreOrder, string CompanyName = null) //test V 
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, font18Separation, 4);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            AddExtraSpace(ref startIndex, list, 12, 1);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            AddExtraSpace(ref startIndex, list, font18Separation, 4);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.ExtraInfoBottomPrint))
            {
                var text = Config.ExtraInfoBottomPrint;

                if (!string.IsNullOrEmpty(CompanyName) && (CompanyName.ToLower().Contains("el chilar") || CompanyName.ToLower().Contains("lisy corp") || CompanyName.ToLower().Contains("el puro sabor")))
                {
                    text = text.Replace("[COMPANY]", CompanyName);

                    startIndex += font18Separation;
                    foreach (var line in GetBottomSplitText(text))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                        startIndex += font18Separation;
                    }
                }
            }

            return list;
        }

        public override string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            DateTime st = DateTime.Now;
            Android.Graphics.Bitmap signature;
            signature = order.ConvertSignatureToBitmap();
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            DateTime st1 = DateTime.Now;
            var converter = new LaceupAndroidApp.BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature, null);
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());
            st1 = DateTime.Now;
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width / 32) * 32) / 8;
            int height = signature.Height / 32 * 32;
            //Math.Ceiling(signature.Width / 8.0);

            // Copy over the actual bitmap data from the bitmap file.
            //byte[] bitmapFileData = System.IO.File.ReadAllBytes (path);
            // This represents the bitmap data without the header information.
            var bitmapDataLength = rawBytes.Length; // bitmapFileData.Length - bitmapDataOffset;
                                                    //byte[] bitmap = new byte[bitmapDataLength];
                                                    //Buffer.BlockCopy(bitmapFileData, bitmapDataOffset, bitmap, 0, bitmapDataLength);

            // Invert bitmap colors

            string ZPLImageDataString = BitConverter.ToString(rawBytes);
            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

            Logger.CreateLog("ZPLImageDataString.Replace took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());

            string label = "^FO200," + startIndex.ToString() + "^GFA, " +
                           bitmapDataLength.ToString() + "," +
                           bitmapDataLength.ToString() + "," +
                           widthInBytes.ToString() + "," +
                           ZPLImageDataString;
            startIndex += height;
            var ts = DateTime.Now.Subtract(st).TotalSeconds;
            Logger.CreateLog("IncludeSignature took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            return label;
        }

    }
}