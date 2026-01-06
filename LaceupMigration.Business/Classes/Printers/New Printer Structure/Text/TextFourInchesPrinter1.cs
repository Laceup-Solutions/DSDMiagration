using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class TextFourInchesPrinter : TextPrinter
    {
        protected override void FillDictionary()
        {
            linesTemplates.Add(EndLabel, "");
            linesTemplates.Add(StartLabel, "");

            linesTemplates.Add(Upc128, "");

            #region Standard

            linesTemplates.Add(StandarPrintTitle, "{1} {2}");
            linesTemplates.Add(StandarPrintDate, "Date: {1}");
            linesTemplates.Add(StandarPrintDateBig, "Date: {1}");
            linesTemplates.Add(StandarPrintRouteNumber, "Route #: {1}");
            linesTemplates.Add(StandarPrintDriverName, "Driver Name: {1}");
            linesTemplates.Add(StandarPrintCreatedBy, "Created By: {1}");
            linesTemplates.Add(StandarPrintedDate, "Printed Date: {1}");
            linesTemplates.Add(StandarPrintedOn, "Printed On: {1}");
            linesTemplates.Add(StandarCreatedOn, "Created On: {1}");

            #endregion

            #region Company

            linesTemplates.Add(CompanyName, "{1}");
            linesTemplates.Add(CompanyAddress, "{1}");
            linesTemplates.Add(CompanyPhone, "Phone: {1}");
            linesTemplates.Add(CompanyFax, "Fax: {1}");
            linesTemplates.Add(CompanyEmail, "Email: {1}");
            linesTemplates.Add(CompanyLicenses1, "Licenses: {1}");
            linesTemplates.Add(CompanyLicenses2, "          {1}");

            #endregion

            #region Order

            linesTemplates.Add(OrderClientName, "{1}");
            linesTemplates.Add(OrderClientNameTo, "Customer: {1}");
            linesTemplates.Add(OrderClientAddress, "{1}");
            linesTemplates.Add(OrderBillTo, "Bill To: {1}");
            linesTemplates.Add(OrderBillTo1, "         {1}");
            linesTemplates.Add(OrderShipTo, "Ship To: {1}");
            linesTemplates.Add(OrderShipTo1, "         {1}");
            linesTemplates.Add(OrderClientLicenceNumber, "License Number: {1}");
            linesTemplates.Add(OrderVendorNumber, "Vendor Number: {1}");
            linesTemplates.Add(OrderTerms, "Terms: {1}");
            linesTemplates.Add(OrderAccountBalance, "Account Balance: {1}");
            linesTemplates.Add(OrderTypeAndNumber, "{2} #: {1}");
            linesTemplates.Add(PONumber, "PO #: {1}");

            linesTemplates.Add(OrderPaymentText, "{1}");
            linesTemplates.Add(OrderHeaderText, "{1}");

            linesTemplates.Add(OrderDetailsHeader, "PRODUCT                           QTY        PRICE    TOTAL");
            linesTemplates.Add(OrderDetailsLineSeparator, "{1}");
            linesTemplates.Add(OrderDetailsHeaderSectionName, "                               {1}");
            linesTemplates.Add(OrderDetailsLines, "{1} {2} {4} {3}");
            linesTemplates.Add(OrderDetailsLines2, "{1}");
            linesTemplates.Add(OrderDetailsLines3, "{1} {2}");
            linesTemplates.Add(OrderDetailsLinesLotQty, "Lot: {1} -> {2}");
            linesTemplates.Add(OrderDetailsWeights, "{1}");
            linesTemplates.Add(OrderDetailsWeightsCount, "Qty: {1}");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "Retail price {1}");
            linesTemplates.Add(OrderDetailsLinesUpcText, "{1}");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "");
            linesTemplates.Add(OrderDetailsTotals, "{1} {2} {3} {4}");
            linesTemplates.Add(OrderDetailsTotals1, "{1} {2} {3} {4}");
            linesTemplates.Add(OrderTotalsNetQty, "                            NET QTY: {1}");
            linesTemplates.Add(OrderTotalsSales, "                              SALES: {1}");
            linesTemplates.Add(OrderTotalsCredits, "                            CREDITS: {1}");
            linesTemplates.Add(OrderTotalsReturns, "                            RETURNS: {1}");
            linesTemplates.Add(OrderTotalsNetAmount, "                         NET AMOUNT: {1}");
            linesTemplates.Add(OrderTotalsDiscount, "                           DISCOUNT: {1}");
            linesTemplates.Add(OrderTotalsTax, "                    {1} {2}");
            linesTemplates.Add(OrderTotalsTotalDue, "                          TOTAL DUE: {1}");
            linesTemplates.Add(OrderTotalsTotalPayment, "                      TOTAL PAYMENT: {1}");
            linesTemplates.Add(OrderTotalsCurrentBalance, "                    INVOICE BALANCE: {1}");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "                       OPEN BALANCE: {1}");
            linesTemplates.Add(OrderTotalsFreight,              "                            FREIGHT: {1}");
            linesTemplates.Add(OrderTotalsOtherCharges,         "                      OTHER CHARGES: {1}");
            linesTemplates.Add(OrderTotalsDiscountComment, " Discount Comment: {1}");
            linesTemplates.Add(OrderPreorderLabel, "{1}");
            linesTemplates.Add(OrderComment, "Comments: {1}");
            linesTemplates.Add(OrderComment2, "          {1}");
            linesTemplates.Add(PaymentComment,  "Payment Comments: {1}");
            linesTemplates.Add(PaymentComment1, "                  {1}");
            linesTemplates.Add(OrderCommentWork, "{1}");

            #endregion

            #region Footer

            linesTemplates.Add(FooterSignatureLine, "----------------------------");
            linesTemplates.Add(FooterSignatureText, "Signature");
            linesTemplates.Add(FooterSignatureNameText, "Signature Name: {1}");
            linesTemplates.Add(FooterSpaceSignatureText, " ");
            linesTemplates.Add(FooterBottomText, "{1}");
            linesTemplates.Add(FooterDriverSignatureText, "Driver Signature");

            #endregion

            #region Allowance

            #endregion

            #region Shortage Report

            #endregion

            #region Load Order

            linesTemplates.Add(LoadOrderHeader, "Load Order Report");
            linesTemplates.Add(LoadOrderRequestedDate, "Load Order Request Date: {1}");
            linesTemplates.Add(LoadOrderNotFinal, "NOT A FINAL LOAD ORDER");
            linesTemplates.Add(LoadOrderTableHeader, "PRODUCT                                   UOM   ORDERED");
            linesTemplates.Add(LoadOrderTableLine, "{1} {2} {3}");
            linesTemplates.Add(LoadOrderTableTotal, "Totals:                                              {1}");

            #endregion

            #region Accept Load

            linesTemplates.Add(AcceptLoadHeader, "Accepted Load");
            linesTemplates.Add(AcceptLoadDate, "Printed Date: {1}");
            linesTemplates.Add(AcceptLoadInvoice, "Invoice #: {1}");
            linesTemplates.Add(AcceptLoadNotFinal, "NOT A FINAL DOCUMENT");
            linesTemplates.Add(AcceptLoadTableHeader, "PRODUCT                                UoM   LOAD  ADJ   INV");
            linesTemplates.Add(AcceptLoadTableHeader1, "                                             OUT");
            linesTemplates.Add(AcceptLoadTableLine, "{1} {2} {3} {4} {5}");
            linesTemplates.Add(AcceptLoadTableTotals, "                           Totals:     Units {1} {2} {3}");

            #endregion

            #region Add Inventory

            linesTemplates.Add(AddInventoryHeader, "Accepted Load");
            linesTemplates.Add(AddInventoryDate, "Printed Date: {1}");
            linesTemplates.Add(AddInventoryNotFinal, "NOT A FINAL DOCUMENT");
            linesTemplates.Add(AddInventoryTableHeader, "PRODUCT                                BEG   LOAD  ADJ   START");
            linesTemplates.Add(AddInventoryTableHeader1,"                                       INV   OUT");
            linesTemplates.Add(AddInventoryTableLine, "{1} {2} {3} {4} {5}");
            linesTemplates.Add(AddInventoryTableTotals, "                           Totals:     {1} {2} {3} {4}");

            #endregion

            #region Inventory

            linesTemplates.Add(InventoryProdHeader, "Inventory Report Date: {1}");
            linesTemplates.Add(InventoryProdTableHeader, "PRODUCT                                        START  CURRENT");
            linesTemplates.Add(InventoryProdTableLine, "{1} {2} {3}");
            linesTemplates.Add(InventoryProdTableLineLot, "                              Lot: {1} {2} {3}");
            linesTemplates.Add(InventoryProdTableLineListPrice, "Price: {1}  Total: {2}");
            linesTemplates.Add(InventoryProdQtyItems, "          TOTAL QTY: {1}");
            linesTemplates.Add(InventoryProdInvValue, "          INV. VALUE: {1}");

            #endregion

            #region Orders Created

            linesTemplates.Add(OrderCreatedReportHeader, "Sales Register Report");
            linesTemplates.Add(OrderCreatedReporWorkDay, "Clock In: {1}  Clock Out: {2} Worked: {3}h:{4}m");
            linesTemplates.Add(OrderCreatedReporBreaks, "Breaks Taken: {1}h:{2}m");
            linesTemplates.Add(OrderCreatedReportTableHeader, "NAME                      ST  QTY    TICKET #.  TOTAL  CS TP");
            linesTemplates.Add(OrderCreatedReportTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(OrderCreatedReportTableLine1, "Clock In: {1}  Clock Out: {2}  # Copies: {3}");
            linesTemplates.Add(OrderCreatedReportTableTerms, "Terms: {1}");
            linesTemplates.Add(OrderCreatedReportTableLineComment, "NS Comment: {1}");
            linesTemplates.Add(OrderCreatedReportTableLineComment1, "RF Comment: {1}");
            linesTemplates.Add(OrderCreatedReportSubtotal, "                                   Subtotal:  {1}");
            linesTemplates.Add(OrderCreatedReportTax,      "                                        Tax:  {1}");
            linesTemplates.Add(OrderCreatedReportTotals,   "                                     Totals:  {1}");
            linesTemplates.Add(OrderCreatedReportPaidCust,     "Paid Cust:           {1} Voided:       {2}");
            linesTemplates.Add(OrderCreatedReportChargeCust,   "Charge Cust:         {1} Delivery:     {2}");
            linesTemplates.Add(OrderCreatedReportCreditCust,   "                     {1} P&P:          {2}");
            linesTemplates.Add(OrderCreatedReportExpectedCash, "Expected Cash Cust:  {1} Refused:      {2}");
            linesTemplates.Add(OrderCreatedReportFullTotal, "Total Sales:         {1} Time (Hours): {2}");

            linesTemplates.Add(OrderCreatedReportCreditTotal,  "                        Credits Total: {2}");
            linesTemplates.Add(OrderCreatedReportBillTotal,    "                           Bill Total: {2}");
            linesTemplates.Add(OrderCreatedReportSalesTotal,   "                          Sales Total: {2}");
            #endregion
            
            #region Payments Report

            linesTemplates.Add(PaymentReportHeader, "Payments Received Report");
            linesTemplates.Add(PaymentReportTableHeader,  "Name               Invoice    Invoice  Amount   Method Ref");
            linesTemplates.Add(PaymentReportTableHeader1, "                   Number     Total                    Number");
            linesTemplates.Add(PaymentReportTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(PaymentReportTotalCash,  "                                        Cash:     {1}");
            linesTemplates.Add(PaymentReportTotalCheck, "                                       Check:     {1}");
            linesTemplates.Add(PaymentReportTotalCC,         "                                 Credit Card:     {1}");
            linesTemplates.Add(PaymentReportTotalMoneyOrder, "                                 Money Order:     {1}");
            linesTemplates.Add(PaymentReportTotalTransfer,   "                                    Transfer:     {1}");
            linesTemplates.Add(PaymentReportTotalTotal,      "                                       Total:     {1}");
            linesTemplates.Add(PaymentSignatureText, "Payment Received By");

            #endregion

            #region Settlement

            linesTemplates.Add(InventorySettlementHeader, "Settlement Report");
            linesTemplates.Add(InventorySettlementProductHeader, "Product                                                        ");
            linesTemplates.Add(InventorySettlementTableHeader, "Lot  UoM Beg.I Load Adj  Tr.  Sls  Ret Dump Dmg  Unlo End.I O.Sho");
            linesTemplates.Add(InventorySettlementTableHeader1, "");
            linesTemplates.Add(InventorySettlementProductLine, "{1}                                         ");
            linesTemplates.Add(InventorySettlementTableLine, "{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}");
            linesTemplates.Add(InventorySettlementTableTotals, "Totals:    {1} {2} {3}  {4} {5} {6} {7} {8} {9} {10} {11} {12}");
            linesTemplates.Add(InventorySettlementTableTotals1, "           {1} {2} {3}  {4} {5} {6} {7} {8} {9} {10} {11} {12}");

            #endregion

            #region Summary

            linesTemplates.Add(InventorySummaryHeader, "Inventory Summary");
            linesTemplates.Add(InventorySummaryTableHeader, "Product                                    ");
            linesTemplates.Add(InventorySummaryTableHeader1, "Lot  UoM  Inv.Ini Loaded  Transf   Sales Curr.Inv");
            linesTemplates.Add(InventorySummaryTableProductLine, "{1}                             {2}{3}{4}{5}{6}{7} ");
            linesTemplates.Add(InventorySummaryTableLine, "{1}  {2}    {3}    {4}    {5}      {6}     {7}      ");
            linesTemplates.Add(InventorySummaryTableTotals, "Totals:{1}{2}{3}{4}    {5}   {6}     {7}     {8}      ");
            linesTemplates.Add(InventorySummaryTableTotals1, "   {1}{2}{3}{4}    {5}   {6}     {7}     {8}      ");


            #endregion

            #region Route Return

            linesTemplates.Add(RouteReturnsTitle, "Route Return Report");
            linesTemplates.Add(RouteReturnsNotFinalLabel, "NOT FINAL ROUTE RETURN");                              //hasta aqui
            linesTemplates.Add(RouteReturnsTableHeader, "Product               Refuse  Dump   Return Dmg    Unload");
            linesTemplates.Add(RouteReturnsTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(RouteReturnsTotals, "Totals:                            {2} {3} {4} {5} {6}");

            #endregion

            #region Payment

            linesTemplates.Add(PaymentTitle, "Payment Receipt");
            linesTemplates.Add(PaymentHeaderTo, "Customer:");
            linesTemplates.Add(PaymentHeaderClientName, "{1}");
            linesTemplates.Add(PaymentHeaderClientAddr, "{1}");
            linesTemplates.Add(PaymentInvoiceNumber, "{1} #: {2}");
            linesTemplates.Add(PaymentInvoiceTotal, "{1} Total: {2}");
            linesTemplates.Add(PaymentPaidInFull, "Paid in Full: {1}");
            linesTemplates.Add(PaymentComponents, "{1}");
            linesTemplates.Add(PaymentTotalPaid, "Total Paid: {1}");
            linesTemplates.Add(PaymentPending, "   Pending: {1}");

            #endregion

            #region Open Invoice

            linesTemplates.Add(InvoiceTitle, "{1}");
            linesTemplates.Add(InvoiceCopy, "COPY");
            linesTemplates.Add(InvoiceDueOn, "Due on:    {1}");
            linesTemplates.Add(InvoiceDueOnOverdue, "Due on:    {1} OVERDUE");
            linesTemplates.Add(InvoiceClientName, "{1}");
            linesTemplates.Add(InvoiceCustomerNumber, "Customer: {1}");
            linesTemplates.Add(InvoiceClientAddr, "{1}");
            linesTemplates.Add(InvoiceClientBalance, "Account Balance: {1}");
            linesTemplates.Add(InvoiceComment, "C: {1}");
            linesTemplates.Add(InvoiceTableHeader, "PRODUCT                           QTY        PRICE    TOTAL");
            linesTemplates.Add(InvoiceTableLine, "{1} {2} {3} {4}");
            linesTemplates.Add(InvoiceTotal, "{1} {2} {3} {4}");
            linesTemplates.Add(InvoicePaidInFull, "   PAID IN FULL");
            linesTemplates.Add(InvoiceCredit,     "   CREDIT");
            linesTemplates.Add(InvoicePartialPayment, "PARTIAL PAYMENT: {1}");
            linesTemplates.Add(InvoiceOpen, "           OPEN: {1}");
            linesTemplates.Add(InvoiceQtyItems, "      QTY ITEMS: {1}");
            linesTemplates.Add(InvoiceQtyUnits, "      QTY UNITS: {1}");

            #endregion

            #region Transfer

            linesTemplates.Add(TransferOnHeader, "Transfer On Report");
            linesTemplates.Add(TransferOffHeader, "Transfer Off Report");
            linesTemplates.Add(TransferNotFinal, "NOT A FINAL TRANSFER");
            linesTemplates.Add(TransferTableHeader, "Product                      Lot       UoM   Transferred");
            linesTemplates.Add(TransferTableLine, "{1} {2} {3} {4}");
            linesTemplates.Add(TransferTableLinePrice, "   List Price: {1}");
            linesTemplates.Add(TransferQtyItems, "                            QTY ITEMS: {1}");
            linesTemplates.Add(TransferAmount,   "                       TRANSFER VALUE: {1}");
            linesTemplates.Add(TransferComment, "Comment: {1}");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "Customer Open Balance");
            linesTemplates.Add(ClientStatementTableHeader, "Type    Date    Number    Due Date    Amount    Open");

            linesTemplates.Add(ClientStatementTableHeader1, "");
            linesTemplates.Add(ClientStatementTableLine, "{1}     {2}     {3}       {4}         {5}       {6}");

            linesTemplates.Add(ClientStatementCurrent, "Current:                                        {1}");
            linesTemplates.Add(ClientStatement1_30PastDue, "1-30 Days Past Due:                             {1}");
            linesTemplates.Add(ClientStatement31_60PastDue, "31-60 Days Past Due:                            {1}");
            linesTemplates.Add(ClientStatement61_90PastDue, "61-90 Days Past Due:                            {1}");
            linesTemplates.Add(ClientStatementOver90PastDue, "Over 90 Days Past Due:                          {1}");
            linesTemplates.Add(ClientStatementAmountDue, "Amount Due:                                     {1}");

            #endregion

            #region Inventory Count

            linesTemplates.Add(InventoryCountHeader, "Inventory Count");
            linesTemplates.Add(InventoryCountTableHeader, "PRODUCT               QTY           UOM   ");
            linesTemplates.Add(InventoryCountTableLine, "{1}                   {2}           {3}   ");

            #endregion
             
            #region Accepted Orders Report

            linesTemplates.Add(AcceptedOrdersHeader, "Accepted Orders Report");
            linesTemplates.Add(AcceptedOrdersDate, "Printed Date: {1}");
            linesTemplates.Add(AcceptedOrdersDeliveriesLabel, "Deliveries");
            linesTemplates.Add(AcceptedOrdersCreditsLabel, "^Credits");
            linesTemplates.Add(AcceptedOrdersDeliveriesTableHeader, "Customer           Qty     Weight     Amount ");
            linesTemplates.Add(AcceptedOrdersTableLine, "{1}                {2}     {3}        {4}    ");
            linesTemplates.Add(AcceptedOrdersTableLine2, "{1}          {2}                             ");
            linesTemplates.Add(AcceptedOrdersLoadsTableHeader, "Load Orders");
            linesTemplates.Add(AcceptedOrdersTableTotals, "Totals:            {1}     {2}        {3}    ");
            linesTemplates.Add(AcceptedOrdersTotalsQty, "                    Total Qty:         {1}");
            linesTemplates.Add(AcceptedOrdersTotalsWeight, "                    Total Weight:      {1}");
            linesTemplates.Add(AcceptedOrdersTotalsAmount, "                    Amount:            {1}");
            #endregion

            #region Refusal Report

            linesTemplates.Add(RefusalReportHeader, "Refusal Report {3}        Page: {1}/{2}");
            linesTemplates.Add(RefusalReportTableHeader, "Reason: {1}              Order #");
            linesTemplates.Add(RefusalReportTableLine, "{1}                      {2}    ");
            linesTemplates.Add(RefusalReportProductTableHeader, "Product                  Qty    ");
            linesTemplates.Add(RefusalReportProductTableLine, "{1}                      {2}    ");

            #endregion

            #region Payment Deposit
            linesTemplates.Add(ChecksTitle, "LIST OF CHECKS");
            linesTemplates.Add(BatchDate, "Posted Date: {1}");
            linesTemplates.Add(BatchPrintedDate, "Printed Date: {1}");
            linesTemplates.Add(BatchSalesman, "Salesman: {1}");
            linesTemplates.Add(BatchBank, "Bank: {1}");
            linesTemplates.Add(CheckTableHeader, "IDENTIFICATION CHECKS         Amount");
            linesTemplates.Add(CheckTableLine, "{1}                           {2}");

            linesTemplates.Add(CheckTableTotal, "# OF CHECKS: {1}    TOTAL CHECKS: {2}");

            linesTemplates.Add(CashTotalLine, "TOTAL CASH: {1}");
            linesTemplates.Add(CreditCardTotalLine, "TOTAL CREDIT CARD: {1}");
            linesTemplates.Add(MoneyOrderTotalLine, "TOTAL MONEY ORDER: {1}");

            linesTemplates.Add(BatchTotal, "TOTAL DEPOSIT: {1}");
            linesTemplates.Add(BatchComments, "COMMENTS: {1}");

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "{0}");
            linesTemplates.Add(DeliveryInvoiceNumber, "{0}");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "Product                                    Qty");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "{1,-44}{2}");
            linesTemplates.Add(TotalQtysProofDelivery, "                                      TOTAL: {1}   ");

            #endregion

            #region pick ticket

            linesTemplates.Add(PickTicketCompanyHeader, "                        {1}                       ");
            linesTemplates.Add(PickTicketRouteInfo,     "{1}                                               ");
            linesTemplates.Add(PickTicketDeliveryDate,  "{1}                                 {2}           ");
            linesTemplates.Add(PickTicketDriver,        "{1}                                 {2}           ");

            linesTemplates.Add(PickTicketProductHeader, "PRODUCT #     DESCRIPTION        CASES        UNITS");
            linesTemplates.Add(PickTicketProductLine,   "{1}           {2}                {3}          {4}");
            linesTemplates.Add(PickTicketProductTotal,  "TOTALS                           {1}          {2}");

            #endregion
        }


        protected override int WidthForBoldFont
        {
            get
            {
                int i = 64;
                return i;
            }
        }

        protected override int WidthForNormalFont
        {
            get
            {
                int i = 64;
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

        protected override int SpaceForPadding
        {
            get
            {
                int i = 35;
                return i;
            }
        }

        protected override IEnumerable<string> SplitRefusalReportLines(string productName)
        {
            return SplitProductName(productName, 30, 50);
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 60, 60);
        }

        protected override IEnumerable<string> GetAcceptLoadDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 38, 38);
        }

        protected override IEnumerable<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 38, 38);
        }

        protected override IList<string> GetBottomDiscountSplitText()
        {
            return SplitProductName(Config.Discount100PercentPrintText, 50, 50);
        }

        protected override IList<string> GetBottomSplitText(string text = "")
        {
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 50, 50);
        }

        protected override IList<string> GetClientNameSplit(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IEnumerable<string> GetConsInvoiceDetailRowsSplitProductName(string name)
        {
            return SplitProductName(name, 28, 28);
        }

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 25, 25);
        }

        protected override IEnumerable<string> GetInventoryProdDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 46, 46);
        }

        protected override IEnumerable<string> GetInventorySettlementRowsSplitProductName(string name)
        {
            return SplitProductName(name, 10, 10);
        }

        protected override IEnumerable<string> GetLoadOrderDetailsRowSplitProductName(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 25, 25);
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 33, 33);
        }

        protected override IList<string> GetOrderDetailsSplitComment(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IList<string> GetOrderSplitComment(string name)
        {
            return SplitProductName(name, 53, 53);
        }

        protected override IEnumerable<string> GetRouteReturnRowsSplitProductName(string name)
        {
            return SplitProductName(name, 29, 29);
        }

        protected override IEnumerable<string> GetOpenInvoiceCommentSplit(string v)
        {
            return SplitProductName(v, 50, 50);
        }

        protected override IEnumerable<string> GetInvoiceDetailSplitProductName(string name)
        {
            return SplitProductName(name, 33, 33);
        }

        protected override IEnumerable<string> GetTransferSplitProductName(string name)
        {
            return SplitProductName(name, 39, 39);
        }

        protected override IEnumerable<string> GetOrderPaymentSplitComment(string comment)
        {
            return SplitProductName(comment, 45, 45);
        }

        protected override IEnumerable<string> GetConsContractDetailRowsSplitProductName(string name)
        {
            throw new NotImplementedException();
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
            if (v1.Length < 33)
                v1 += new string(' ', 33 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 8)
                v3 += new string(' ', 8 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v4, v3);
        }

        protected override string GetSectionRowsInOneDocFixedLine3(string format, int pos, string v1, string v2)
        {
            if (v1.Length < 33)
                v1 += new string(' ', 33 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        protected override string GetSectionRowsInOneDocFixedLotLine(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        protected override string GetOrderDetailsSectionTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            var lengthExtra = v2.Length - 10;

            var newV1Lenth = 22;
            if (lengthExtra > 0)
                newV1Lenth -= lengthExtra;

            if (v1.Length < newV1Lenth)
                v1 += new string(' ', newV1Lenth - v1.Length);

            if (v2.Length < 10)
                v2 = new string(' ', 10 - v2.Length) + v2;

            if (v3.Length < 19)
                v3 += new string(' ', 19 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        public override string AdjustPadding(string input, int safetyGap = 3)
        {
            input += ":";
            return input;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                bool isDisocuntItem = false;
                if (detail.Product.IsDiscountItem)
                {
                    isDisocuntItem = true;
                }

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

                        string georgehoweValue = DataAccess.GetSingleUDF("georgehowe", detail.OrderDetail.UnitOfMeasure.ExtraFields);
                        if (int.TryParse(georgehoweValue, out int conversionFactor))
                        {
                            totalQtyNoUoM += detail.Qty * conversionFactor;
                        }
                        else
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

                if (isDisocuntItem)
                    name = detail.OrderDetail.Comments;

                var productSlices = GetOrderDetailsRowsSplitProductName(name);

                string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                if (detail.OrderDetail.UnitOfMeasure != null)
                    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                var splitQtyAsString = SplitProductName(qtyAsString, 10, 10);

                foreach (string pName in productSlices)
                {
                    string currentQty = (productLineOffset < splitQtyAsString.Count) ? splitQtyAsString[productLineOffset] : string.Empty;

                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Qty;

                            if (_.Product.SoldByWeight)
                            {
                                if (order.AsPresale)
                                    qty *= _.Product.Weight;
                                else
                                    qty = _.Weight;
                            }

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        double price = detail.Price * factor;

                        if (!isDisocuntItem)
                            balance += d;

                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;
                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, currentQty, totalAsString, priceAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(GetSectionRowsInOneDocFixedLine3(OrderDetailsLines3, startIndex, pName, currentQty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                while (productLineOffset < splitQtyAsString.Count)
                {
                    string remainingQty = splitQtyAsString[productLineOffset];
                    list.Add(GetSectionRowsInOneDocFixedLine3(OrderDetailsLines3, startIndex, string.Empty, remainingQty)); //OrderDetailsLines2
                    startIndex += font18Separation;

                    productLineOffset++;
                }


                string weights = "";

                if (Config.PrintLotPreOrder || Config.PrintLotOrder)
                {
                    foreach (var item in detail.ParticipatingDetails)
                    {
                        var itemLot = item.Lot ?? "";
                        if (!string.IsNullOrEmpty(itemLot) && item.LotExpiration != DateTime.MinValue)
                            itemLot += "  Exp: " + item.LotExpiration.ToShortDateString();

                        string qty = item.Qty.ToString();
                        if (item.Product.SoldByWeight && !order.AsPresale)
                            qty = item.Weight.ToString();

                        if (!string.IsNullOrEmpty(itemLot))
                        {
                            if (preOrder)
                            {
                                if (Config.PrintLotPreOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                            else
                            {
                                if (Config.PrintLotOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                        }
                        else
                        {
                            if (item.Product.SoldByWeight && !order.AsPresale)
                            {
                                var temp_weight = DoFormat(item.Weight);
                                weights += temp_weight + " ";
                            }
                        }
                    }
                }
                else if (!order.AsPresale)
                {
                    StringBuilder sb = new StringBuilder();
                    List<string> lotUsed = new List<string>();

                    int TotalCases = 0;


                    foreach (var detail1 in detail.ParticipatingDetails)
                    {
                        if (!string.IsNullOrEmpty(detail1.Lot) && detail1.Product.SoldByWeight)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            else
                                sb.Append("Weight: ");

                            if (detail1.Weight != 0)
                                sb.Append(detail1.Weight.ToString());


                            lotUsed.Add(detail1.Lot);
                            TotalCases++;
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                        startIndex += font18Separation;
                    }
                }

                if (!string.IsNullOrEmpty(weights))
                {
                    foreach (var item in GetOrderDetailsRowsSplitProductName(weights))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, item));
                        startIndex += font18Separation;
                    }
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeightsCount], startIndex, detail.ParticipatingDetails.Count));
                    startIndex += font18Separation;
                }

                // anderson crap
                // the retail price
                var extraProperties = order.Client.ExtraProperties;
                if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                {
                    var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                    if (retailPrice != null)
                    {
                        string retPriceString = "                                  " + ToString(Convert.ToDouble(retailPrice.Item2));
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesRetailPrice], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()) && !isDisocuntItem)
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

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }
        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (uomMap.Count > 0)
            {
                if (!uomMap.ContainsKey("Units"))
                    uomMap.Add("Units", totalQtyNoUoM);
            }
            else
            {
                uomMap.Add("Totals", totalQtyNoUoM);

                if (uomMap.Keys.Count == 1 && totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION" && !uomMap.ContainsKey("Units"))
                    uomMap.Add("Units", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();


            if (!Config.HideSubTotalOrder && printTotal)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    string adjustedKey = key + ":";
                    var balanceText = ToString(balance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    //list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText)); 
                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals1, startIndex, string.Empty, adjustedKey, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText)); //key //uoMLines[0]
                    startIndex += font18Separation;

                    offset++;
                }
            }

            return list;
        }
       

        #endregion

        #region Load Order

        protected override string GetLoadOrderTableLineFixed(string format, int pos, string value1, string value2, string value3)
        {
            if (value1.Length < 42)
                value1 += new string(' ', 42 - value1.Length);

            if (value2.Length < 5)
                value2 += new string(' ', 5 - value2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, value1, value2, value3);
        }

        #endregion

        #region Accept Load

        protected override string GetAcceptLoadTableLineFixed(string format, float pos, string v1, string v2, string v3, string v4, string v5)
        {
            if (v1.Length < 38)
                v1 += new string(' ', 38 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            if (v3.Length < 5)
                v3 += new string(' ', 5 - v3.Length);

            if (v4.Length < 5)
                v4 += new string(' ', 5 - v4.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override string GetAcceptLoadTableTotalsFixed(string format, float pos, string v1, string v2, string v3)
        {
            if (v1.Length < 5)
                v1 += new string(' ', 5 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Add Inventory

        protected override string GetAddInventoryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            if (v1.Length < 38)
                v1 += new string(' ', 38 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            if (v3.Length < 5)
                v3 += new string(' ', 5 - v3.Length);

            if (v4.Length < 5)
                v4 += new string(' ', 5 - v4.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected override string GetAddInventoryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 5)
                v1 += new string(' ', 5 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            if (v3.Length < 5)
                v3 += new string(' ', 5 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region Inventory

        protected override string GetInventoryProdTableLineFixed(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 46)
                v1 += new string(' ', 46 - v1.Length);

            if (v2.Length < 6)
                v2 += new string(' ', 6 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        protected override string GetInventoryProdTableLineLotFixed(string format, int pos, string v1, string v2, string v3)
        {
            if (v1.Length < 10)
                v1 += new string(' ', 10 - v1.Length);

            if (v2.Length < 6)
                v2 += new string(' ', 6 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Orders Created Report

        protected override string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 25)
                v1 += new string(' ', 25 - v1.Length);

            if (v2.Length < 3)
                v2 += new string(' ', 3 - v2.Length);

            if (v3.Length < 6)
                v3 += new string(' ', 6 - v3.Length);

            if (v4.Length < 10)
                v4 += new string(' ', 10 - v4.Length);

            if (v5.Length < 6)
                v5 += new string(' ', 6 - v5.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        protected override string GetOrderCreatedReportTotalsFixed(string format, int pos, string v1, string v2)
        {
            if (v1.Length < 15)
                v1 += new string(' ', 15 - v1.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2);
        }

        #endregion

        #region Payments Report

        protected override string GetPaymentReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 18)
                v1 += new string(' ', 18 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 8)
                v3 += new string(' ', 8 - v3.Length);

            if (v4.Length < 8)
                v4 += new string(' ', 8 - v4.Length);

            if (v5.Length < 6)
                v5 += new string(' ', 6 - v5.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Inventory SUmmary

        protected override IEnumerable<string> GetInventorySummaryTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow, bool isBase)
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

                float factor = 1;
                if (!isBase)
                {
                    var defaultUom = p.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                        factor = defaultUom.Conversion;
                }

                var newS = GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                                        p.Lot,
                                        p.UoM != null ? p.UoM.Name : string.Empty,
                                        Math.Round(p.BegInv / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round((p.LoadOut + p.Adj) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round((p.TransferOn - p.TransferOff) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.Sales / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                        Math.Round(p.EndInventory / factor, Config.Round).ToString(CultureInfo.CurrentCulture)
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

        protected override string GetInventorySummaryTableProductLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            if (v1.Length < 40)
                v1 += new string(' ', 40 - v1.Length);
            else
                v1 = v1.Substring(0, v1.Length > 40 ? 40 : v1.Length);

            if (v2.Length < 4)
                v2 += new string(' ', 4 - v2.Length);
            else
                v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);

            if (v3.Length < 3)
                v3 += new string(' ', 3 - v3.Length);
            else
                v3 = v3.Substring(0, v3.Length > 3 ? 3 : v3.Length);

            if (v4.Length < 4)
                v4 += new string(' ', 4 - v4.Length);
            else
                v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);

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
            if (v1.Length < 29)
                v1 += new string(' ', 29 - v1.Length);

            if (v2.Length < 7)
                v2 += new string(' ', 7 - v2.Length);

            if (v3.Length < 7)
                v3 += new string(' ', 7 - v3.Length);

            if (v4.Length < 7)
                v4 += new string(' ', 7 - v4.Length);

            if (v5.Length < 7)
                v5 += new string(' ', 7 - v5.Length);

            if (v6.Length < 7)
                v6 += new string(' ', 7 - v6.Length);

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
            if (v1.Length < 34)
                v1 += new string(' ', 34 - v1.Length);

            if (v2.Length < 6)
                v2 += new string(' ', 6 - v2.Length);

            if (v3.Length < 9)
                v3 += new string(' ', 9 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        protected override string GetInvoiceTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            if (v1.Length < 22)
                v1 += new string(' ', 22 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            if (v3.Length < 19)
                v3 += new string(' ', 19 - v3.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }
        #endregion

        #region Transfers

        protected override string GetTransferTableFixedLine(string format, int pos, string v1,string lot, string v2, string v3)
        {

            /*
            if (v1.Length < 41)
                v1 += new string(' ', 41 - v1.Length);

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

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, lot,v1, v2, v3);
        }

        #endregion

        #region Proof Delivery
        protected override IEnumerable<string> GetTotalsRowsInOneDocDelivery(ref int startY, Order order)
        {
            List<string> list = new List<string>();
            double total = 0;

            foreach (var detail in order.Details)
            {
                var productNameLines = SplitProductName(detail.Product.Name, 35, 35);
                double qty = detail.Qty;
                total += qty;

                bool isFirstLine = true;
                foreach (var productNameLine in productNameLines)
                {
                    if (isFirstLine)
                    {
                        string productName = productNameLine.PadRight(44, ' ');
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsDelivery], startY, productNameLine, qty));
                        isFirstLine = false;
                    }
                    else
                    {
                        startY += font18Separation;
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsDelivery], startY, productNameLine, string.Empty));
                    }

                }

                startY += font36Separation;

            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TotalQtysProofDelivery], startY, total));
            startY += font36Separation;

            return list;
        }
        #endregion
    }
}