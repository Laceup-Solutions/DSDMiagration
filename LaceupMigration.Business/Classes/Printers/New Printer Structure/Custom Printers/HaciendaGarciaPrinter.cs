using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class HaciendaGarciaPrinter : ZebraFourInchesPrinter1
    {
        //company info
        protected const string HaciendaGraciaInvoiceCompanyName = "HaciendaGraciaInvoiceCompanyName";
        protected const string HaciendaGraciaCompanyAddress1 = "HaciendaGraciaCompanyAddress1";
        protected const string HaciendaGraciaCompanyAddress2 = "HaciendaGraciaCompanyAddress2";
        protected const string HaciendaGraciaInvoiceCompanyNumber = "HaciendaGraciaInvoiceCompanyNumber";
        protected const string HaciendaGraciaInvoiceCompanyFaxNumber = "HaciendaGraciaInvoiceCompanyFaxNumber";
        protected const string HaciendaGraciaInvoiceCompanyTaxFreeNumber = "HaciendaGraciaInvoiceCompanyTaxFreeNumber";
        protected const string HaciendaGraciaInvoiceCompanyManufacturer = "HaciendaGraciaInvoiceCompanyManufacturer";
        protected const string HaciendaGraciaClientAddress = "HaciendaGraciaClientAddress";


        //invoice info
        protected const string HaciendaGraciaInvoiceNumber = "HaciendaGraciaInvoiceNumber";
        protected const string HaciendaGraciaInvoiceDate = "HaciendaGraciaInvoiceDate";
        protected const string HaciendaGarciaCustomerNumber = "HaciendaGarciaCustomerNumber";
        protected const string HaciendaGarciaCustomerName = "HaciendaGarciaCustomerName";
        protected const string HaciendaGarciaInvoiceRoute = "HaciendaGarciaInvoiceRoute";
        protected const string HaciendaGarciaInvoiceSalesmanName = "InvoiceSalesmanName";
        protected const string HaciendaGarciaPaymentMethod = "HaciendaGarciaPaymentMethod";


        protected const string InvoiceSalesPersonRoute = "InvoiceSalesPersonRoute";
        protected const string InvoiceTermsAndConditionsHeader = "InvoiceTermsAndConditionsHeader";
        protected const string InvoiceTermsAndConditions = "InvoiceTermsAndConditions";
        protected const string InvoiceCustomerPhoneNumber = "InvoiceCustomerPhoneNumber";
        protected const string InvoiceTableHeader1 = "InvoiceTableHeader1";
        protected const string InvoiceReceivedBy = "InvoiceReceivedBy";

        protected const string HaciendaGraciaInvoiceTotal = "HaciendaGraciaInvoiceTotal";
        protected const string HaciendaGraciaInvoiceTableLine = "HaciendaGraciaInvoiceTableLine";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(HaciendaGraciaInvoiceCompanyName, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(HaciendaGraciaCompanyAddress1, "^FO40,{0}^ADN,18,10^FDPO Box: {1}^FS");
            linesTemplates.Add(HaciendaGraciaCompanyAddress2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(HaciendaGraciaInvoiceCompanyNumber, "^FO40,{0}^ADN,18,10^FDOffice: {1}^FS");
            linesTemplates.Add(HaciendaGraciaInvoiceCompanyFaxNumber, "^FO40,{0}^ADN,18,10^FDFax: {1}^FS");
            linesTemplates.Add(HaciendaGraciaInvoiceCompanyTaxFreeNumber, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(HaciendaGraciaInvoiceCompanyManufacturer, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            //separator here

            linesTemplates.Add(HaciendaGraciaInvoiceNumber, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(HaciendaGraciaInvoiceDate, "^FO40,{0}^ADN,18,10^FDDATE/TIME: {1}^FS");
            linesTemplates.Add(HaciendaGarciaCustomerName, "^FO40,{0}^ADN,18,10^FDCUSTOMER: {1}^FS");
            linesTemplates.Add(HaciendaGarciaCustomerNumber, "^FO40,{0}^ADN,18,10^FDCUSTOMER: {1}^FS");
            linesTemplates.Add(HaciendaGraciaClientAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(InvoiceCustomerPhoneNumber, "^FO40,{0}^ADN,18,10^FDPHONE: {1}^FS");
            linesTemplates.Add(HaciendaGarciaInvoiceRoute, "^FO40,{0}^ADN,18,10^FDROUTE: {1}^FS");
            linesTemplates.Add(HaciendaGarciaInvoiceSalesmanName, "^FO40,{0}^ADN,18,10^FDSALESMAN: {1}^FS");
            linesTemplates.Add(HaciendaGarciaPaymentMethod, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            //separator here

            linesTemplates.Add(InvoiceTableHeader1, "^FO40,{0}^ADN,18,10^FDItem^FS" +
             "^FO40,{0}^ADN,18,10^Item^FS" +
             "^FO140,{0}^ADN,18,10^Item^FS" +
             "^FO360,{0}^ADN,18,10^FD^FS" +
             "^FO560,{0}^ADN,18,10^FDUnit^FS" +
             "^FO660,{0}^ADN,18,10^FDExtended^FS");

            linesTemplates[InvoiceTableHeader] =
             "^FO40,{0}^ADN,18,10^FDNumber^FS" +
             "^FO140,{0}^ADN,18,10^FDDescription^FS" +
             "^FO360,{0}^ADN,18,10^FDWeight^FS" +
             "^FO460,{0}^ADN,18,10^FDCases^FS" +
             "^FO560,{0}^ADN,18,10^FDPrice^FS" +
             "^FO660,{0}^ADN,18,10^FDPrice^FS"
             ;

            //sep


            linesTemplates.Add(HaciendaGraciaInvoiceTableLine,
                "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO140,{0}^ADN,18,10^FD{2}^FS" +
                "^FO360,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS" +
                "^FO560,{0}^ADN,18,10^FD{5}^FS" +
                "^FO660,{0}^ADN,18,10^FD{6}^FS"
                );
            //sep

            linesTemplates.Add(HaciendaGraciaInvoiceTotal, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
              "^FO360,{0}^ADN,18,10^FD{2}^FS" +
              "^FO460,{0}^ADN,18,10^FD{3}^FS" +
              "^FO660,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(InvoiceTermsAndConditionsHeader, "^FO40,{0}^ADN,18,10^FB730,1,0,C^FH\\^FDTerms and Conditions^FS");
            linesTemplates.Add(InvoiceTermsAndConditions, "^FO40,{0}^ADN,18,10^FB730,20,7,L,0^FD{1}^FS");

            linesTemplates.Add(InvoiceReceivedBy, "^FO40,{0}^ADN,18,10^FDReceived By:^FS");

        }


        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            List<string> lines = new List<string>();
            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceNumber], startY, GetOrderDocumentName(order) + ": " + order.PrintedOrderId));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceDate], startY, order.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            Client client = order.Client;
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            var custno = UDFHelper.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
                custNoString = " " + custno.Value;


            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaCustomerNumber], startY, custNoString));
            // startY += font18Separation;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaCustomerName], startY, clientSplit));
                startY += font18Separation;
            }


            string f = string.Empty;
            int count = 1;

            foreach (var part in ClientAddress(client))
            {
                if (count == ClientAddress(client).Length)
                    f += part;
                else
                    f += part + ", ";
                count++;
            }

            if (!string.IsNullOrEmpty(f))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaClientAddress], startY, f));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCustomerPhoneNumber], startY, client.ContactPhone.ToString()));
            startY += font18Separation;



            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaInvoiceRoute], startY, salesman != null ? salesman.RouteNumber : string.Empty));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaInvoiceSalesmanName], startY, salesman != null ? salesman.Name : string.Empty));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            // lines.AddRange(GetOpenInvoiceTable(ref startY, order));

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            if (payment != null)
            {
                lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));
                startY += font18Separation;

                string paymentMethod = payment.PaymentMethods();
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    StringBuilder sb = new StringBuilder();
                    if (payment.Components.Sum(x => x.Amount) == order.OrderTotalCost())
                        sb.Append("Paid In Full");
                    else
                        sb.Append("Paid " + payment.Components.Sum(x => x.Amount).ToCustomString());

                    if (payment.PaymentMethods().Contains(InvoicePaymentMethod.Cash.ToString()))
                    {
                        sb.Append(" - Cash");
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaPaymentMethod], startY, sb.ToString()));
                        startY += font18Separation;
                    }
                    else
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaPaymentMethod], startY, sb.ToString()));
                        startY += font18Separation;
                        sb.Clear();
                        sb.Append("Check #" + payment.CheckNumbers());
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGarciaPaymentMethod], startY, sb.ToString()));
                        startY += font18Separation;
                    }
                }
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedOn], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTermsAndConditionsHeader], startY, "Terms And Conditions"));
            startY += font36Separation;

            string termsAndCondParagraph = "(1)Seller retains title to the goods and a security interest in the goods, including all accessions to and replacements of them, until Buyer performs the entire contact. (2)Seller reservers the right to repossess the goods if payment is not received in 14 days.(3)The goods in this invoice are for resale only. (4)In any action which may be brought to enforce payment under this contract Seller is entitled to recover all costs including attorneys fees.(5)If this invoice is not paid within 30 days of delivery, any unpaid amount will be subject to a monthly finance charge of 1.5%. (6)Buyer agrees to pay $30.00 for each check drawn an insufficient funds. (7)Buyer agress that juridisdiction and verue is proper in Stanislaus County, California.\n I acknowledge that (I) all refrenced good have been received and are in good order, and (II) I understand that this sale is expressly conditioned upon my assent to all terms on the reverse of this page, and I accept them as terms of this sale. ";

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTermsAndConditions], startY, termsAndCondParagraph));
            startY += 448;
            startY += font18Separation;


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceReceivedBy], startY, "Received By:"));
            startY += font18Separation;
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }
        protected string GetOrderDocumentName(Order order)
        {
            string docName = "Invoice#";

            if (order.AsPresale)
            {
                docName = "Sales Order#";
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credit#";
            }

            return docName;
        }


        public string GetOpenInvoiceTableFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            v1 = v1.Substring(0, v1.Length > 7 ? 7 : v1.Length);
            v2 = v2.Substring(0, v2.Length > 16 ? 16 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 5 ? 5 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 6 ? 6 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 8 ? 8 : v6.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6);
        }
        public IEnumerable<string> GetInvoiceTotals(ref int startY, Order order, double totalQty, double totalPrice, double totalCases)
        {
            List<string> lines = new List<string>();

            string totalPriceString = string.Empty;

            if (totalPrice >= 0)
                totalPriceString = Math.Round(Math.Abs(totalPrice), Config.Round).ToCustomString();
            else
                totalPriceString = "-" + Math.Round(Math.Abs(totalPrice), Config.Round).ToCustomString();


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceTotal], startY, "GRAND TOTALS:", Math.Round(totalQty, Config.Round), Math.Round(totalCases, Config.Round).ToString(), totalPriceString));
            startY += font18Separation;

            return lines;
        }

        public IEnumerable<string> GetCompanyRows(ref int startIndex)
        {
            try
            {
                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                CompanyInfo company = CompanyInfo.GetMasterCompany();

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceCompanyName], startIndex, part));
                    startIndex += font18Separation;
                }
                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaCompanyAddress1], startIndex, company.CompanyAddress1));
                startIndex += font18Separation;
                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaCompanyAddress2], startIndex, company.CompanyAddress2));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceCompanyNumber], startIndex, company.CompanyPhone));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HaciendaGraciaInvoiceCompanyFaxNumber], startIndex, company.CompanyFax));
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

            List<string> list = new List<string>();


            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTableHeader1], startY));
            startY += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTableHeader], startY));
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


                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(0), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            double totalCases = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            double TotalQty = 0;
            double TotalPrice = 0;

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                float TotalCases = 0;
                float totalWeight = 0;

                foreach (var detail1 in detail.ParticipatingDetails)
                {
                    if (detail1.Product.SoldByWeight)
                    {
                        totalWeight += detail1.Weight;
                        TotalCases++;
                    }
                    else
                    {
                        TotalCases++;
                    }
                }

                if (totalWeight != 0)
                {
                    var itemNumer = detail.Product.Sku;
                    var ProductName = detail.Product.Name;

                    double qty = totalWeight;
                    TotalQty += qty;

                    string cases = TotalCases.ToString();

                    totalCases += TotalCases;

                    double packaging = 1;
                    Double.TryParse(detail.Product.Package, out packaging);

                    var unitPrice = detail.Price / packaging;
                    var extendedPrice = detail.Price * detail.Qty;

                    var isCreditFactor = 1;

                    if (detail.OrderDetail.IsCredit)
                        isCreditFactor *= -1;

                    TotalPrice += extendedPrice * isCreditFactor;

                    extendedPrice = extendedPrice * isCreditFactor;

                    string extendedPriceAsString = string.Empty;

                    if (extendedPrice >= 0)
                        extendedPriceAsString = Math.Round(Math.Abs(extendedPrice), Config.Round).ToCustomString();
                    else
                        extendedPriceAsString = "-" + Math.Round(Math.Abs(extendedPrice), Config.Round).ToCustomString();

                    var newS = GetOpenInvoiceTableFixed(HaciendaGraciaInvoiceTableLine, startIndex, itemNumer, ProductName, qty.ToString(), cases, Math.Round(unitPrice, Config.Round).ToCustomString(), extendedPriceAsString);

                    list.Add(newS);
                    startIndex += font18Separation;

                    if (ProductName.Length > 16)
                    {
                        string restOfString = ProductName.Substring(16).TrimStart(' ');
                        var secondLine = GetOpenInvoiceTableFixed(HaciendaGraciaInvoiceTableLine, startIndex, string.Empty, restOfString, string.Empty, string.Empty, string.Empty, string.Empty);
                        list.Add(secondLine);
                        startIndex += font18Separation;
                    }
                }
                else
                {
                    var itemNumer = detail.Product.Sku;
                    var ProductName = detail.Product.Name;

                    double qty = detail.Qty;
                    // TotalQty += qty;

                    string cases = TotalCases.ToString();
                    totalCases += TotalCases;

                    double packaging = 1;
                    Double.TryParse(detail.Product.Package, out packaging);

                    var unitPrice = detail.Price / packaging;
                    var extendedPrice = detail.Price * detail.Qty;

                    var isCreditFactor = 1;

                    if (detail.OrderDetail.IsCredit)
                        isCreditFactor *= -1;

                    TotalPrice += extendedPrice * isCreditFactor;

                    string extendedPriceAsString = string.Empty;
                    if (extendedPrice >= 0)
                        extendedPriceAsString = Math.Round(Math.Abs(extendedPrice), Config.Round).ToCustomString();
                    else
                        extendedPriceAsString = "-" + Math.Round(Math.Abs(extendedPrice), Config.Round).ToCustomString();

                    var newS = GetOpenInvoiceTableFixed(HaciendaGraciaInvoiceTableLine, startIndex, itemNumer, ProductName, string.Empty, cases, Math.Round(unitPrice, Config.Round).ToCustomString(), extendedPriceAsString);

                    list.Add(newS);
                    startIndex += font18Separation;

                    if (ProductName.Length > 16)
                    {
                        string restOfString = ProductName.Substring(16).TrimStart(' ');
                        var secondLine = GetOpenInvoiceTableFixed(HaciendaGraciaInvoiceTableLine, startIndex, string.Empty, restOfString, string.Empty, string.Empty, string.Empty, string.Empty);
                        list.Add(secondLine);
                        startIndex += font18Separation;

                    }
                }


                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetInvoiceTotals(ref startIndex, order, TotalQty, TotalPrice, totalCases));

            return list;
        }



    }


}