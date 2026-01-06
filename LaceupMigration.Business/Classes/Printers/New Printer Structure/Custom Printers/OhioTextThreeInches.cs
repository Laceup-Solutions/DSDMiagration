using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OhioTextThreeInches : TextThreeInchesPrinter
    {
        // Ohio Three Inches paper FourInches Printer (RW 420)

        protected override bool PrintLines(List<string> lines)
        {
            try
            {
                if (lines.Any(x => x.Contains((char)241) || x.Contains((char)209)))
                    lines = InsertSpecialChar(lines);

                var finalText = new StringBuilder();
                foreach (var line in lines)
                {
                    var l = line;

                    finalText.Append(l);
                    finalText.Append((char)10);
                    finalText.Append((char)13);
                }

                finalText.Append((char)10);
                finalText.Append((char)13);
                finalText.Append((char)10);
                finalText.Append((char)13);

                PrintIt(finalText.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        protected void AddSpacesToTemplates()
        {
            List<string> keys = new List<string>(linesTemplates.Keys);

            foreach (string key in keys)
            {
                string value = linesTemplates[key];
                linesTemplates[key] = "            " + value;
            }
        }


        protected override void FillDictionary()
        {
            base.FillDictionary();

            #region Order

            linesTemplates[OrderClientName] = "{1}";
            linesTemplates[OrderClientNameTo] = "Customer: {1}";
            linesTemplates[OrderClientAddress] = "{1}";
            linesTemplates[OrderBillTo] = "Bill To: {1}";
            linesTemplates[OrderBillTo1] = "         {1}";
            linesTemplates[OrderShipTo] = "Ship To: {1}";
            linesTemplates[OrderShipTo1] = "         {1}";
            linesTemplates[OrderClientLicenceNumber] = "License Number: {1}";
            linesTemplates[OrderVendorNumber]= "Vendor Number: {1}";
            linesTemplates[OrderTerms] = "Terms: {1}";
            linesTemplates[OrderAccountBalance] = "Account Balance: {1}";
            linesTemplates[OrderTypeAndNumber] = "{2} #: {1}";
            linesTemplates[PONumber] = "PO #: {1}";
            linesTemplates[OrderPaymentText] = "{1}";
            linesTemplates[OrderHeaderText] = "{1}";
            linesTemplates[OrderDetailsHeader] = "PRODUCT            QTY        PRICE    TOTAL";
            linesTemplates[OrderDetailsLineSeparator] = "{1}";
            linesTemplates[OrderDetailsHeaderSectionName] = "                    {1}";
            linesTemplates[OrderDetailsLines] = "{1} {2} {4} {3}";
            linesTemplates[OrderDetailsLines2] = "{1}";
            linesTemplates[OrderDetailsLinesLotQty] = "Lot: {1} -> {2}";
            linesTemplates[OrderDetailsWeights] = "{1}";
            linesTemplates[OrderDetailsWeightsCount] = "Qty: {1}";
            linesTemplates[OrderDetailsLinesRetailPrice] = "Retail price {1}";
            linesTemplates[OrderDetailsLinesUpcText] = "{1}";
            linesTemplates[OrderDetailsLinesUpcBarcode] = "";
            linesTemplates[OrderDetailsTotals] = "{1} {2} {3} {4}";
            linesTemplates[OrderTotalsNetQty] = "             NET QTY: {1}";
            linesTemplates[OrderTotalsSales] = "               SALES: {1}";
            linesTemplates[OrderTotalsCredits] = "             CREDITS: {1}";
            linesTemplates[OrderTotalsReturns] = "             RETURNS: {1}";
            linesTemplates[OrderTotalsNetAmount] = "          NET AMOUNT: {1}";
            linesTemplates[OrderTotalsDiscount] = "            DISCOUNT: {1}";
            linesTemplates[OrderTotalsTax] = "     {1} {2}";
            linesTemplates[OrderTotalsTotalDue] = "           TOTAL DUE: {1}";
            linesTemplates[OrderTotalsTotalPayment] = "       TOTAL PAYMENT: {1}";
            linesTemplates[OrderTotalsCurrentBalance] = "     INVOICE BALANCE: {1}";
            linesTemplates[OrderTotalsClientCurrentBalance] = "        OPEN BALANCE: {1}";
            linesTemplates[OrderTotalsDiscountComment] = " Discount Comment: {1}";
            linesTemplates[OrderPreorderLabel] = "{1}";
            linesTemplates[OrderComment] = "Comments: {1}";
            linesTemplates[OrderComment2] = "          {1}";
            linesTemplates[PaymentComment] = "Payment Comments: {1}";
            linesTemplates[PaymentComment1] = "                  {1}";

            #endregion

            #region Footer

            linesTemplates[FooterSignatureLine] = "   -------------------------------";
            linesTemplates[FooterSignatureText] = "   Signature";
            linesTemplates[FooterSignatureNameText] = "   Signature Name: {1}";
            linesTemplates[FooterSpaceSignatureText] = " ";
            linesTemplates[FooterBottomText] = "   {1}";
            linesTemplates[FooterDriverSignatureText] = "   Driver Signature";

            #endregion

            #region Orders Created

            linesTemplates[OrderCreatedReportHeader] = "Sales Register Report";
            linesTemplates[OrderCreatedReporWorkDay] = "Clock In: {1}  Clock Out: {2} Worked: {3}h:{4}m";
            linesTemplates[OrderCreatedReporBreaks] = "Breaks Taken: {1}h:{2}m";
            linesTemplates[OrderCreatedReportTableHeader] = "NAME           ST  QTY  TICKET #.  TOTAL  CS TP";
            linesTemplates[OrderCreatedReportTableLine] = "{1} {2} {3} {4} {5} {6}";
            linesTemplates[OrderCreatedReportTableLine1] = "Clock In: {1}  Clock Out: {2}  # Copies: {3}";
            linesTemplates[OrderCreatedReportTableTerms] = "Terms: {1}";
            linesTemplates[OrderCreatedReportTableLineComment] = "NS Comment: {1}";
            linesTemplates[OrderCreatedReportTableLineComment1] = "RF Comment: {1}";
            linesTemplates[OrderCreatedReportSubtotal] = "                         Subtotal:  {1}";
            linesTemplates[OrderCreatedReportTax] = "                              Tax:  {1}";
            linesTemplates[OrderCreatedReportTotals] = "                           Totals:  {1}";
            linesTemplates[OrderCreatedReportPaidCust] = "Paid Cust:          {1} Voided:       {2}";
            linesTemplates[OrderCreatedReportChargeCust] = "Charge Cust:        {1} Delivery:     {2}";
            linesTemplates[OrderCreatedReportCreditCust] = "                    {1} P&P:          {2}";
            linesTemplates[OrderCreatedReportExpectedCash] = "Expected Cash Cust: {1} Refused:      {2}";
            linesTemplates[OrderCreatedReportFullTotal] = "Total Sales:        {1} Time (Hours): {2}";
            linesTemplates[OrderCreatedReportCreditTotal] = "                       Credits Total: {2}";
            linesTemplates[OrderCreatedReportBillTotal] = "                          Bill Total: {2}";
            linesTemplates[OrderCreatedReportSalesTotal] = "                         Sales Total: {2}";
            #endregion

            #region Payments Report

            linesTemplates[PaymentReportHeader] = "Payments Received Report";
            linesTemplates[PaymentReportTableHeader] = "Name               Inv #      Inv Total Amount";
            linesTemplates[PaymentReportTableHeader1] = "                   Method     Ref Num";
            linesTemplates[PaymentReportTableLine] = "{1} {2} {3} {4} {5} {6}";
            linesTemplates[PaymentReportTotalCash] = "                          Cash:     {1}";
            linesTemplates[PaymentReportTotalCheck] = "                         Check:     {1}";
            linesTemplates[PaymentReportTotalCC] = "                   Credit Card:     {1}";
            linesTemplates[PaymentReportTotalMoneyOrder] = "                   Money Order:     {1}";
            linesTemplates[PaymentReportTotalTransfer] = "                      Transfer:     {1}";
            linesTemplates[PaymentReportTotalTotal] = "                         Total:     {1}";
            linesTemplates[PaymentSignatureText] = "Payment Received By";

            #endregion

            #region Open Invoice

            linesTemplates[InvoiceTitle] = "{1}";
            linesTemplates[InvoiceCopy] = "COPY";
            linesTemplates[InvoiceDueOn] = "Due on:    {1}";
            linesTemplates[InvoiceDueOnOverdue] = "Due on:    {1} OVERDUE";
            linesTemplates[InvoiceClientName] = "{1}";
            linesTemplates[InvoiceCustomerNumber] = "Customer: {1}";
            linesTemplates[InvoiceClientAddr] = "{1}";
            linesTemplates[InvoiceClientBalance] = "Account Balance: {1}";
            linesTemplates[InvoiceComment] = "C: {1}";
            linesTemplates[InvoiceTableHeader] = "PRODUCT            QTY        PRICE    TOTAL";
            linesTemplates[InvoiceTableLine] = "{1} {2} {3} {4}";
            linesTemplates[InvoiceTotal] = "{1} {2} {3} {4}";
            linesTemplates[InvoicePaidInFull] = "   PAID IN FULL";
            linesTemplates[InvoiceCredit] = "   CREDIT";
            linesTemplates[InvoicePartialPayment] = "PARTIAL PAYMENT: {1}";
            linesTemplates[InvoiceOpen] = "           OPEN: {1}";
            linesTemplates[InvoiceQtyItems] = "      QTY ITEMS: {1}";
            linesTemplates[InvoiceQtyUnits] = "      QTY UNITS: {1}";

            #endregion

            #region Transfer

            linesTemplates[TransferOnHeader] = "Transfer On Report";
            linesTemplates[TransferOffHeader] = "Transfer Off Report";
            linesTemplates[TransferNotFinal] = "NOT A FINAL TRANSFER";
            linesTemplates[TransferTableHeader] = "Product             Lot   UoM   Transferred";
            linesTemplates[TransferTableLine] = "{1} {2} {3} {4}";
            linesTemplates[TransferTableLinePrice] = "   List Price: {1}";
            linesTemplates[TransferQtyItems] = "             QTY ITEMS: {1}";
            linesTemplates[TransferAmount] = "        TRANSFER VALUE: {1}";
            linesTemplates[TransferComment] = "Comment: {1}";

            #endregion

            #region Client Statement

            linesTemplates[ClientStatementTableTitle] = "Customer Open Balance";
            linesTemplates[ClientStatementTableHeader] = "Type              Date             Number";
            linesTemplates[ClientStatementTableHeader1] = "Due Date          Amount           Open  ";
            linesTemplates[ClientStatementTableLine] = "{1}    {2}    {3}";
            linesTemplates[ClientStatementTableLine1] = "{1}    {2}    {3}";
            linesTemplates[ClientStatementCurrent] = "Current:                {1}";
            linesTemplates[ClientStatement1_30PastDue] = "1-30 Days Past Due:     {1}";
            linesTemplates[ClientStatement31_60PastDue] = "31-60 Days Past Due :   {1}";
            linesTemplates[ClientStatement61_90PastDue] = "61-90 Days Past Due :   {1}";
            linesTemplates[ClientStatementOver90PastDue] = "Over 90 Days Past Due : {1}";
            linesTemplates[ClientStatementAmountDue] = "Amount Due :            {1}";

            #endregion

            AddSpacesToTemplates();

            // To Add some spaces to PrintTaxLabel to fit the printer
            if (Config.PrintTaxLabel.Length < 16 && !Config.PrintTaxLabel.StartsWith(" "))
            {
                Config.PrintTaxLabel = new string(' ', 4) + Config.PrintTaxLabel;
            }
        }

        protected override int WidthForBoldFont
        {
            get
            {
                int i = 47;
                return i;
            }
        }

        protected override int WidthForNormalFont
        {
            get
            {
                int i = 47;
                return i;
            }
        }

        protected override int SpaceForOrderFooter
        {
            get
            {
                int i = 17;
                return i;
            }
        }

        protected override IEnumerable<string> SplitRefusalReportLines(string productName)
        {
            return SplitProductName(productName, 25, 50);
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 45, 45);
        }

        protected override IEnumerable<string> GetAcceptLoadDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 25, 25);
        }

        protected override IEnumerable<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 25, 25);
        }

        protected override IList<string> GetBottomDiscountSplitText()
        {
            return SplitProductName(Config.Discount100PercentPrintText, 40, 40);
        }

        protected override IList<string> GetBottomSplitText(string text = "")
        {
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 40, 40);
        }

        protected override IList<string> GetClientNameSplit(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IEnumerable<string> GetConsInvoiceDetailRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 15, 15);
        }

        protected override IEnumerable<string> GetInventoryProdDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 32, 32);
        }

        protected override IEnumerable<string> GetInventorySettlementRowsSplitProductName(string name)
        {
            return SplitProductName(name, 10, 10);
        }

        protected override IEnumerable<string> GetLoadOrderDetailsRowSplitProductName(string name)
        {
            return SplitProductName(name, 34, 34);
        }

        protected override IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 14, 14);
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IList<string> GetOrderDetailsSplitComment(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IList<string> GetOrderSplitComment(string name)
        {
            return SplitProductName(name, 29, 29);
        }

        protected override IEnumerable<string> GetRouteReturnRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IEnumerable<string> GetOpenInvoiceCommentSplit(string v)
        {
            return SplitProductName(v, 40, 40);
        }

        protected override IEnumerable<string> GetInvoiceDetailSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IEnumerable<string> GetTransferSplitProductName(string name)
        {
            return SplitProductName(name, 25, 25);
        }

        protected override IEnumerable<string> GetOrderPaymentSplitComment(string comment)
        {
            return SplitProductName(comment, 30, 30);
        }

        protected override IEnumerable<string> GetConsContractDetailRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IEnumerable<string> GetInventoryCountDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IEnumerable<string> GetAcceptedLoadSplitClientName(string clientName)
        {
            return SplitProductName(clientName, 29, 29);
        }

        #region Order

        protected override string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v4, string v3)
        {
            if (v1.Length < 18)
                v1 += new string(' ', 18 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 8)
                v3 += new string(' ', 8 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v4, v3);
        }

        protected override string GetSectionRowsInOneDocFixedLotLine(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        protected override string GetOrderDetailsSectionTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 8)
                v1 += new string(' ', 8 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 19)
                v3 += new string(' ', 19 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region Load Order

        protected override string GetLoadOrderTableLineFixed(string format, int pos, string value1, string value2, string value3)
        {
            if (value1.Length < 27)
                value1 += new string(' ', 27 - value1.Length);

            if (value2.Length < 4)
                value2 += new string(' ', 4 - value2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, value1, value2, value3);
        }

        #endregion

        #region Accept Load

        protected override string GetAcceptLoadTableLineFixed(string format, float pos, string v1, string v2, string v3, string v4, string v5)
        {
            if (v1.Length < 25)
                v1 += new string(' ', 25 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);

            if (v4.Length < 4)
                v4 += new string(' ', 4 - v4.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override string GetAcceptLoadTableTotalsFixed(string format, float pos, string v1, string v2, string v3)
        {
            if (v1.Length < 4)
                v1 += new string(' ', 4 - v1.Length);

            if (v2.Length < 4)
                v2 += new string(' ', 4 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Add Inventory

        protected override string GetAddInventoryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            if (v1.Length < 25)
                v1 += new string(' ', 25 - v1.Length);

            if (v2.Length < 4)
                v2 += new string(' ', 4 - v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);

            if (v4.Length < 4)
                v4 += new string(' ', 4 - v4.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override string GetAddInventoryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 4)
                v1 += new string(' ', 4 - v1.Length);

            if (v2.Length < 4)
                v2 += new string(' ', 4 - v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region Inventory

        protected override string GetInventoryProdTableLineFixed(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 32)
                v1 += new string(' ', 32 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        protected override string GetInventoryProdTableLineLotFixed(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 10)
                v1 += new string(' ', 10 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Orders Created Report

        protected override string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 14)
                v1 += new string(' ', 14 - v1.Length);

            if (v2.Length < 3)
                v2 += new string(' ', 3 - v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);

            if (v4.Length < 10)
                v4 += new string(' ', 10 - v4.Length);

            if (v5.Length < 6)
                v5 += new string(' ', 6 - v5.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        protected override string GetOrderCreatedReportTotalsFixed(string format, int pos, string v1, string v2)
        {
            if (v1.Length < 6)
                v1 += new string(' ', 6 - v1.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2);
        }

        #endregion

        #region Payments Report

        protected override IEnumerable<string> GetPaymentsReportTable(ref int startY, List<PaymentRow> rows)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in rows)
            {
                lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                                p.ClientName,
                                p.DocNumber,
                                p.DocAmount,
                                p.Paid,
                                string.Empty,
                                string.Empty));
                startY += font18Separation;

                lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                                string.Empty,
                                p.PaymentMethod,
                                p.RefNumber,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
        }

        protected override string GetPaymentReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 18)
                v1 += new string(' ', 18 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 9)
                v3 += new string(' ', 9 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Inventory Settlement Report

        protected override IEnumerable<string> GetSettlementReportTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow)
        {
            List<string> lines = new List<string>();

            var oldRound = Config.Round;
            Config.Round = 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var group in SortDetails.SortedDetails(map).GroupBy(x => x.Product.Name))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementProductLine], startY, group.Key));
                startY += font18Separation;

                foreach (var p in group)
                {
                    if (p.Product.UseLot)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementLotLine], startY, p.Lot));
                        startY += font18Separation;
                    }

                    lines.Add(GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                           p.UoM != null ? p.UoM.Name : string.Empty,
                                           Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           Math.Round(p.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                           string.Empty,
                                           string.Empty,
                                           string.Empty,
                                           string.Empty,
                                           string.Empty,
                                           string.Empty));
                    startY += font18Separation;

                    lines.Add(GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                string.Empty,
                                                Math.Round(p.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Reshipped, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                p.OverShort,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty));

                    startY += font18Separation;

                    startY += 5;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            if (map.All(x => x.UoM == null))
            {
                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals1, startY,
                                                Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty));
                startY += font18Separation;

                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals, startY,
                                                    Math.Round(totalRow.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Reshipped, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    totalRow.OverShort,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty));
                startY += font18Separation;
            }

            Config.Round = oldRound;

            return lines;
        }

        #endregion

        #region Client Statement

        protected override IEnumerable<string> GetClientStatementHeader(ref int startY, Client client)
        {
            List<string> lines = new List<string>();

            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
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

            if (client.ExtraProperties != null)
            {
                var termsExtra = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null && !string.IsNullOrEmpty(termsExtra.Item2))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, termsExtra.Item2.ToUpperInvariant()));
                    startY += font18Separation;
                }
            }

            return lines;
        }

        protected override IEnumerable<string> GetClientStatementTable(ref int startY, Client client)
        {
            List<string> lines = new List<string>();

            var openInvoices = (from i in Invoice.OpenInvoices
                                where i.Client != null && i.Client.ClientId == client.ClientId && i.Balance != 0
                                orderby i.Date descending
                                select i).ToList();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableTitle], startY));
            startY += 70;

            lines.AddRange(GetClientStatementTableHeader(ref startY));

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double current = 0;
            double due1_30 = 0;
            double due31_60 = 0;
            double due61_90 = 0;
            double over90 = 0;

            foreach (var item in openInvoices.OrderBy(x => x.DueDate))
            {
                if (item.InvoiceType == 2 || item.InvoiceType == 3)
                    continue;

                lines.Add(GetClientStatementFixedLine(ClientStatementTableLine,
                    startY,
                    GetClientStatementInvoiceType(item.InvoiceType),
                    item.Date.ToShortDateString(),
                    item.InvoiceNumber));

                startY += font18Separation;

                lines.Add(GetClientStatementFixedLine1(ClientStatementTableLine1,
                   startY,
                   item.DueDate.ToShortDateString(),
                   ToString(item.Amount),
                   ToString(item.Balance)));

                startY += font36Separation;

                current += item.Balance;

                var due = DateTime.Now.Subtract(item.DueDate).Days;

                if (due > 0 && due < 31)
                    due1_30 += item.Balance;
                else if (due > 30 && due < 61)
                    due31_60 += item.Balance;
                else if (due > 60 && due < 91)
                    due61_90 += item.Balance;
                else if (due > 90)
                    over90 += item.Balance;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            lines.AddRange(GetClientStatementTotals(ref startY, current, due1_30, due31_60, due61_90, over90));

            return lines;
        }

        protected override IEnumerable<string> GetClientStatementTotals(ref int startY, double current, double due1_30,
        double due31_60,
        double due61_90,
        double over90)
        {
            List<string> lines = new List<string>();

            string s1;

            s1 = ToString(current);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementCurrent], startY, s1));
            startY += font36Separation;

            s1 = ToString(due1_30);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatement1_30PastDue], startY, s1));
            startY += font36Separation;

            s1 = ToString(due31_60);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatement31_60PastDue], startY, s1));
            startY += font36Separation;

            s1 = ToString(due61_90);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatement61_90PastDue], startY, s1));
            startY += font36Separation;

            s1 = ToString(over90);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementOver90PastDue], startY, s1));
            startY += font36Separation;

            s1 = ToString((due1_30 + due31_60 + due61_90 + over90));
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementAmountDue], startY, s1));
            startY += font36Separation;

            return lines;
        }

        protected override IEnumerable<string> GetClientStatementTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader], startY));
            startY += font36Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader1], startY));
            startY += font36Separation;


            return lines;
        }

        protected override string GetClientStatementInvoiceType(int invoiceType)
        {
            switch (invoiceType)
            {
                case 0:
                    return "Invoice";
                case 1:
                    return "Credit";
                case 2:
                    return "Quote";
                default:
                    return "Invoice";
            }
        }

        protected string GetClientStatementFixedLine(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 13)
                v1 += new string(' ', 13 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 13 ? 13 : v1.Length);

            if (v2.Length < 13)
                v2 += new string(' ', 13 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 13 ? 13 : v2.Length);

            if (v3.Length < 13)
                v3 += new string(' ', 13 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 13 ? 13 : v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }
        protected string GetClientStatementFixedLine1(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 13)
                v1 += new string(' ', 13 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 13 ? 13 : v1.Length);

            if (v2.Length < 13)
                v2 += new string(' ', 13 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 13 ? 13 : v2.Length);

            if (v3.Length < 13)
                v3 += new string(' ', 13 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 13 ? 13 : v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Inventory SUmmary

        protected override IEnumerable<string> GetInventorySummaryTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow, bool isbase)
        {
            List<string> lines = new List<string>();

            var oldRound = Config.Round;
            Config.Round = 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (Math.Round(p.EndInventory, Config.Round) == 0)
                    continue;

                var productNameLine = GetInventorySummaryTableProductLineFixed(InventorySummaryTableProductLine, startY,
                                                p.Product.Name,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty
                                                );

                lines.Add(productNameLine);
                startY += font18Separation;
                startY += 5;


                var newS = GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                                        p.Lot,
                                        p.UoM != null ? p.UoM.Name : string.Empty,
                                        Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.LoadOut + p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                                        );

                lines.Add(newS);


                startY += font18Separation;
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            if (map.All(x => x.UoM == null))
            {
                lines.Add(GetInventorySummaryTableTotalsFixed(InventorySummaryTableTotals, startY,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                                                    ));
                startY += font18Separation;
            }

            Config.Round = oldRound;

            return lines;
        }

        protected override string GetInventorySummaryTableProductLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            if (v1.Length < 40)
                v1 += new string(' ', 40 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 40 ? 40 : v1.Length);

            if (v2.Length < 3)
                v2 += new string(' ', 3 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 3 ? 3 : v2.Length);

            if (v3.Length < 3)
                v3 += new string(' ', 3 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 3 ? 3 : v3.Length);

            if (v4.Length < 3)
                v4 += new string(' ', 3 - v4.Length);
            else
                v4 = v4.Substring(0, v4.Length > 3 ? 3 : v4.Length);

            if (v5.Length < 3)
                v5 += new string(' ', 3 - v5.Length);
            else
                v5 = v5.Substring(0, v5.Length > 3 ? 3 : v5.Length);

            if (v6.Length < 3)
                v6 += new string(' ', 3 - v6.Length);
            else
                v6 = v6.Substring(0, v6.Length > 3 ? 3 : v6.Length);

            if (v7.Length < 3)
                v7 += new string(' ', 3 - v7.Length);
            else
                v7 = v7.Substring(0, v7.Length > 3 ? 3 : v7.Length);



            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);

        }
        protected override string GetInventorySummaryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            if (v1.Length < 4)
                v1 += new string(' ', 4 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 4 ? 4 : v1.Length);

            if (v2.Length < 4)
                v2 += new string(' ', 4 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);

            if (v4.Length < 4)
                v4 += new string(' ', 4 - v4.Length);
            else
                v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);

            if (v5.Length < 4)
                v5 += new string(' ', 4 - v5.Length);
            else
                v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);

            if (v6.Length < 4)
                v6 += new string(' ', 4 - v6.Length);
            else
                v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);

            if (v7.Length < 4)
                v7 += new string(' ', 4 - v7.Length);
            else
                v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);



            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);

        }

        protected override string GetInventorySummaryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8)
        {
            if (v1.Length < 0)
                v1 += new string(' ', 0 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 0 ? 0 : v1.Length);

            if (v2.Length < 0)
                v2 += new string(' ', 0 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 0 ? 0 : v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);

            if (v4.Length < 4)
                v4 += new string(' ', 4 - v4.Length);
            else
                v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);

            if (v5.Length < 4)
                v5 += new string(' ', 4 - v5.Length);
            else
                v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);

            if (v6.Length < 4)
                v6 += new string(' ', 4 - v6.Length);
            else
                v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);

            if (v7.Length < 4)
                v7 += new string(' ', 4 - v7.Length);
            else
                v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);

            if (v8.Length < 4)
                v8 += new string(' ', 4 - v8.Length);
            else
                v8 = v8.Substring(0, v8.Length > 4 ? 4 : v8.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8);
        }


        #endregion

        #region Route Returns

        protected override string GetRouteReturnTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 20)
                v1 += new string(' ', 20 - v1.Length);

            if (v2.Length < 6)
                v2 += new string(' ', 6 - v2.Length);

            if (v3.Length < 6)
                v3 += new string(' ', 6 - v3.Length);

            if (v4.Length < 6)
                v4 += new string(' ', 6 - v4.Length);

            if (v5.Length < 6)
                v5 += new string(' ', 6 - v5.Length);

            if (v6.Length < 6)
                v6 += new string(' ', 6 - v6.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
            v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Payments

        protected override IEnumerable<string> GetPaymentTitle(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentTitle], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            return lines;
        }

        #endregion

        #region Open Invoice

        protected override string GetOpenInvoiceTableFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 19)
                v1 += new string(' ', 19 - v1.Length);

            if (v2.Length < 6)
                v2 += new string(' ', 6 - v2.Length);

            if (v3.Length < 9)
                v3 += new string(' ', 9 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        protected override string GetInvoiceTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 8)
                v1 += new string(' ', 8 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 19)
                v3 += new string(' ', 19 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }
        #endregion

        #region Transfers

        protected override string GetTransferTableFixedLine(string format, int pos, string v1, string lot, string v2, string v3)
        {
            /*  if (v1.Length < 26)
                  v1 += new string(' ', 26 - v1.Length);

              if (v2.Length < 8)
                  v2 += new string(' ', 8 - v2.Length);*/
            if (v1.Length < 29)
                v1 += new string(' ', 29 - v1.Length);

            if (lot.Length < 7)
                lot += new string(' ', 7 - lot.Length);

            if (v2.Length < 7)
                v2 += new string(' ', 7 - v2.Length);

            if (v3.Length < 7)
                v3 += new string(' ', 7 - v3.Length);



            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, lot, v2, v3);
        }

        #endregion
    }

}


