using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class CremimexPrinter : ZebraFourInchesPrinter1
    {
        protected const string CremimexDepartment = "CremimexDepartment";

        protected override void FillDictionary()
        {
            base.FillDictionary();
            linesTemplates.Add(CremimexDepartment, "^FO130,{0}^ADN,36,20^FB600,1,0,C^FD{1}^FS");
        }
        public override bool PrintOpenInvoice(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, null);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            var ss1 = GetInvoiceType(invoice) + invoice.InvoiceNumber;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, ss1, string.Empty));
            startY += font36Separation;

            var orderNumber = DataAccess.GetSingleUDF("Sales Order", invoice.ExtraFields);

            if (!string.IsNullOrEmpty(orderNumber))
            {
                ss1 = "Sales Order #: " + orderNumber;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, ss1, string.Empty));
                startY += font36Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCopy], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarCreatedOn], startY, invoice.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (invoice.DueDate < DateTime.Today)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOnOverdue], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOn], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedOn], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            Client client = invoice.Client;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
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

            string term = string.Empty;

            if (client.ExtraProperties != null)
            {
                var termsExtra = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                    term = termsExtra.Item2.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, term));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientBalance], startY, balance));
                startY += font18Separation;
            }

            startY += font36Separation;

            foreach (string commentPArt in GetOpenInvoiceCommentSplit(invoice.Comments ?? string.Empty))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                startY += font18Separation;
            }

            lines.AddRange(GetOpenInvoiceTable(ref startY, invoice));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }


        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
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

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            lines.AddRange(GetHeaderRowsInOneDoc(ref startY, asPreOrder, order, order.Client, printedId, payments, payments != null && payments.Sum(x => x.Amount) == balance));

            lines.AddRange(GetOrderLabel(ref startY, order, asPreOrder));

            if (!string.IsNullOrEmpty(order.CremiMexDepartment))
            {
                //3 to make it bold :)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CremimexDepartment], startY, order.CremiMexDepartment));
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CremimexDepartment], startY, order.CremiMexDepartment));
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CremimexDepartment], startY, order.CremiMexDepartment));
                startY += font36Separation;
                startY += font18Separation;
            }


            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));

            AddExtraSpace(ref startY, lines, 36, 1);

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
                foreach (var line in GetBottomDiscountSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            if (!PrintLines(lines))
                return false;

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
    }
}