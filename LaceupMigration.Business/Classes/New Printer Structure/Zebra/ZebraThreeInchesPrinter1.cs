using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ZebraThreeInchesPrinter1 : ZebraPrinter1
    {
        protected override void FillDictionary()
        {
            linesTemplates.Add(EndLabel, "^XZ");
            linesTemplates.Add(StartLabel, "^XA^PON^MNN^LL{0}");

            linesTemplates.Add(Upc128, "^FO15,{0}^BCN,40^FD{1}^FS");

            linesTemplates.Add(UPC128ForLabel, "^FO50,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(RetailPrice, "^FO130,{0}^ADN,18,16^FD{1}^FS");


            #region Standard

            linesTemplates.Add(StandarPrintTitle, "^FO15,{0}^ADN,36,20^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(StandarPrintDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintDateBig, "^CF0,30^FO15,{0}^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintRouteNumber, "^FO15,{0}^ADN,18,10^FDRoute #: {1}^FS");
            linesTemplates.Add(StandarPrintDriverName, "^FO15,{0}^ADN,18,10^FDDriver Name: {1}^FS");
            linesTemplates.Add(StandarPrintCreatedBy, "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS");
            linesTemplates.Add(StandarPrintedDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(StandarPrintedOn, "^FO15,{0}^ADN,18,10^FDPrinted On: {1}^FS");
            linesTemplates.Add(StandarCreatedOn, "^FO15,{0}^ADN,18,10^FDCreated On: {1}^FS");

            #endregion

            #region Company

            linesTemplates.Add(CompanyName, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(CompanyAddress, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(CompanyPhone, "^FO15,{0}^ADN,18,10^FDPhone: {1}^FS");
            linesTemplates.Add(CompanyFax, "^FO15,{0}^ADN,18,10^FDFax: {1}^FS");
            linesTemplates.Add(CompanyEmail, "^FO15,{0}^ADN,18,10^FDEmail: {1}^FS");
            linesTemplates.Add(CompanyLicenses1, "^FO15,{0}^ADN,18,10^FDLicenses: {1}^FS");
            linesTemplates.Add(CompanyLicenses2, "^FO15,{0}^ADN,18,10^FD          {1}^FS");

            #endregion

            #region Order

            linesTemplates.Add(OrderClientName, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderClientNameTo, "^FO15,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(OrderClientAddress, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderBillTo, "^FO15,{0}^ADN,18,10^FDBill To: {1}^FS");
            linesTemplates.Add(OrderBillTo1, "^FO15,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderShipTo, "^FO15,{0}^ADN,18,10^FDShip To: {1}^FS");
            linesTemplates.Add(OrderShipTo1, "^FO15,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderClientLicenceNumber, "^FO15,{0}^ADN,18,10^FDLicense Number: {1}^FS");
            linesTemplates.Add(OrderVendorNumber, "^FO15,{0}^ADN,18,10^FDVendor Number: {1}^FS");
            linesTemplates.Add(OrderTerms, "^FO15,{0}^ADN,18,10^FDTerms: {1}^FS");
            linesTemplates.Add(OrderAccountBalance, "^FO15,{0}^ADN,18,10^FDAccount Balance: {1}^FS");
            linesTemplates.Add(OrderTypeAndNumber, "^FO15,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(PONumber, "^FO15,{0}^ADN,36,20^FDPO #: {1}^FS");

            linesTemplates.Add(OrderPaymentText, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderHeaderText, "^FO15,{0}^ADN,36,20^FD{1}^FS");

            linesTemplates.Add(OrderDetailsHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO275,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO480,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeaderSectionName, "^FO350,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO275,{0}^ADN,18,10^FD{2}^FS" +
                "^FO380,{0}^ADN,18,10^FD{4}^FS" +
                "^FO460,{0}^ADN,18,10^FB110,1,0,R^FH\\^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLines2, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines3, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO275,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderDetailsLinesLotQty, "^FO15,{0}^ADN,18,10^FDLot: {1} -> {2}^FS");
            linesTemplates.Add(OrderDetailsWeights, "^CF0,25^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(OrderDetailsWeightsCount, "^FO15,{0}^ADN,18,10^FDQty: {1}^FS");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "^FO15,{0}^ADN,18,10^FDRetail price {1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcText, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "^FO60,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "^BY3,3,40^FO50,{0}^BEN,40,Y,N^FD{1}^FS");

            linesTemplates.Add(OrderDetailsTotals, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO160,{0}^ADN,18,10^FD{2}^FS" +
                "^FO275,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FB110,1,0,R^FH\\^FD{4}^FS");

            linesTemplates.Add(OrderDetailsTotals1, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
         "^FO30,{0}^ADN,18,10^FD{2}^FS" +
         "^FO275,{0}^ADN,18,10^FD{3}^FS" +
         "^FO460,{0}^ADN,18,10^FB110,1,0,R^FH\\^FD{4}^FS");
            linesTemplates.Add(OrderTotalContainers, "^FO40,{0}^ADN,36,20^FD     CONTAINERS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetQty, "^FO80,{0}^ADN,18,10^FD        NET QTY: {1}^FS");
            linesTemplates.Add(OrderTotalsSales, "^FO80,{0}^ADN,18,10^FD          SALES: {1}^FS");
            linesTemplates.Add(OrderTotalsCredits, "^FO80,{0}^ADN,18,10^FD        CREDITS: {1}^FS");
            linesTemplates.Add(OrderTotalsReturns, "^FO80,{0}^ADN,18,10^FD        RETURNS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetAmount, "^FO80,{0}^ADN,18,10^FD     NET AMOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscount, "^FO80,{0}^ADN,18,10^FD       DISCOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsTax, "^FO80,{0}^ADN,18,10^FD{1} {2}^FS");
            linesTemplates.Add(OrderTotalsTotalDue, "^FO80,{0}^ADN,18,10^FD      TOTAL DUE: {1}^FS");
            linesTemplates.Add(OrderTotalsTotalPayment, "^FO80,{0}^ADN,18,10^FD  TOTAL PAYMENT: {1}^FS");
            linesTemplates.Add(OrderTotalsCurrentBalance, "^FO80,{0}^ADN,18,10^FDINVOICE BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "^FO80,{0}^ADN,18,10^FD   OPEN BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsFreight,              "^FO80,{0}^ADN,18,10^FD        FREIGHT: {1}^FS");
            linesTemplates.Add(OrderTotalsOtherCharges, "^FO80,{0}^ADN,18,10^FD  OTHER CHARGES: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscountComment, "^FO15,{0}^ADN,18,10^FD Discount Comment: {1}^FS");
            linesTemplates.Add(OrderPreorderLabel, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderComment, "^FO15,{0}^ADN,18,10^FDComments: {1}^FS");
            linesTemplates.Add(OrderComment2, "^FO15,{0}^ADN,18,10^FD          {1}^FS");
            linesTemplates.Add(PaymentComment,  "^FO15,{0}^ADN,18,10^FDPayment Comments: {1}^FS");
            linesTemplates.Add(PaymentComment1, "^FO15,{0}^ADN,18,10^FD                  {1}^FS");
            linesTemplates.Add(OrderCommentWork, "^FO15,{0}^AON,24,15^FD{1}^FS");

            #endregion

            #region Footer

            linesTemplates.Add(FooterSignatureLine, "^FO15,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO15,{0}^ADN,18,10^FDSignature^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO15,{0}^ADN,18,10^FDSignature Name: {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO15,{0}^ADN,18,10^FD ^FS");
            linesTemplates.Add(FooterBottomText, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FooterDriverSignatureText, "^FO15,{0}^ADN,18,10^FDDriver Signature^FS");

            #endregion

            #region Allowance

            linesTemplates.Add(AllowanceOrderDetailsHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO380,{0}^ADN,18,10^FDQTY^FS" +
                "^FO480,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO580,{0}^ADN,18,10^FDALLOW^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(AllowanceOrderDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO380,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO580,{0}^ADN,18,10^FD-{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{5}^FS");

            #endregion

            #region Shortage Report

            linesTemplates.Add(ShortageReportHeader, "^FO15,{0}^ADN,36,20^FDKNOWN SHORTAGE REPORT^FS");
            linesTemplates.Add(ShortageReportDate, "^FO200,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(ShortageReportInvoiceHeader, "^FO15,{0}^ADN,36,20^FDInvoice #: {1}^FS");
            linesTemplates.Add(ShortageReportTableHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO500,{0}^ADN,18,10^FDPO QTY^FS" +
                "^FO600,{0}^ADN,18,10^FDSHORT.^FS" +
                "^FO710,{0}^ADN,18,10^FDDEL.^FS");
            linesTemplates.Add(ShortageReportTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO500,{0}^ADN,18,10^FD{2}^FS" +
                "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                "^FO710,{0}^ADN,18,10^FD{4}^FS");

            #endregion

            #region Load Order

            linesTemplates.Add(LoadOrderHeader, "^FO15,{0}^ADN,36,20^FDLoad Order Report^FS");
            linesTemplates.Add(LoadOrderRequestedDate, "^FO15,{0}^ADN,18,10^FDLoad Order Request Date: {1}^FS");
            linesTemplates.Add(LoadOrderNotFinal, "^FO15,{0}^ADN,28,14^FDNOT A FINAL LOAD ORDER^FS");
            linesTemplates.Add(LoadOrderTableHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO320,{0}^ADN,18,10^FDUOM^FS" +
                "^FO450,{0}^ADN,18,10^FDORDERED^FS");
            linesTemplates.Add(LoadOrderTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(LoadOrderTableTotal, "^FO15,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO450,{0}^ADN,18,10^FD{1}^FS");

            #endregion

            #region Accept Load

            linesTemplates.Add(AcceptLoadHeader, "^FO15,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AcceptLoadDate, "^FO200,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AcceptLoadInvoice, "^FO15,{0}^ADN,36,20^FDInvoice #: {1}^FS");
            linesTemplates.Add(AcceptLoadNotFinal, "^FO15,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS");
            linesTemplates.Add(AcceptLoadTableHeader, "^FO15,{0}^ABN,18,10^FDPRODUCT^FS" +
                "^FO280,{0}^ABN,18,10^FDUoM^FS" +
                "^FO340,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO410,{0}^ABN,18,10^FDADJ^FS" +
                "^FO500,{0}^ABN,18,10^FDINV^FS");
            linesTemplates.Add(AcceptLoadTableHeader1, "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO280,{0}^ABN,18,10^FD^FS" +
                "^FO340,{0}^ABN,18,10^FDOUT^FS" +
                "^FO410,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AcceptLoadTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO280,{0}^ABN,18,10^FD{2}^FS" +
                "^FO340,{0}^ABN,18,10^FD{3}^FS" +
                "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                "^FO500,{0}^ABN,18,10^FD{5}^FS");

            linesTemplates.Add(AcceptLoadLotLine, "^FO15,{0}^ABN,18,10^FDLot: {1}^FS");
            linesTemplates.Add(AcceptLoadWeightLine, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(AcceptLoadTableTotals, "^FO180,{0}^ABN,18,10^FD^FS" +
                "^FO280,{0}^ABN,18,10^FDUnits^FS" +
                "^FO340,{0}^ABN,18,10^FD{1}^FS" +
                "^FO410,{0}^ABN,18,10^FD{2}^FS" +
                "^FO500,{0}^ABN,18,10^FD{3}^FS");

            linesTemplates.Add(AcceptLoadTableTotals1, "^FO180,{0}^ABN,18,10^FDTotals:^FS" +
       "^FO280,{0}^ABN,18,10^FDCASE^FS" +
       "^FO340,{0}^ABN,18,10^FD{1}^FS" +
       "^FO410,{0}^ABN,18,10^FD{2}^FS" +
       "^FO500,{0}^ABN,18,10^FD{3}^FS");

            #endregion

            #region Add Inventory

            linesTemplates.Add(AddInventoryHeader, "^FO15,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AddInventoryDate, "^FO200,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AddInventoryNotFinal, "^FO15,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS");
            linesTemplates.Add(AddInventoryTableHeader, "^FO15,{0}^ABN,18,10^FDPRODUCT^FS" +
                "^FO280,{0}^ABN,18,10^FDBEG^FS" +
                "^FO340,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO410,{0}^ABN,18,10^FDADJ^FS" +
                "^FO500,{0}^ABN,18,10^FDSTART^FS");
            linesTemplates.Add(AddInventoryTableHeader1, "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO280,{0}^ABN,18,10^FDINV^FS" +
                "^FO340,{0}^ABN,18,10^FDOUT^FS" +
                "^FO410,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AddInventoryTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO280,{0}^ABN,18,10^FD{2}^FS" +
                "^FO340,{0}^ABN,18,10^FD{3}^FS" +
                "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                "^FO500,{0}^ABN,18,10^FD{5}^FS");
            linesTemplates.Add(AddInventoryTableTotals, "^FO180,{0}^ABN,18,10^FDTotals:^FS" +
                "^FO280,{0}^ABN,18,10^FD{1}^FS" +
                "^FO340,{0}^ABN,18,10^FD{2}^FS" +
                "^FO410,{0}^ABN,18,10^FD{3}^FS" +
                "^FO500,{0}^ABN,18,10^FD{4}^FS");

            #endregion

            #region Inventory

            linesTemplates.Add(InventoryProdHeader, "^FO15,{0}^ADN,18,10^FDInventory Report Date: {1}^FS");
            linesTemplates.Add(InventoryProdTableHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO360,{0}^ADN,18,10^FDSTART^FS" +
                "^FO460,{0}^ADN,18,10^FDCURRENT^FS");
            linesTemplates.Add(InventoryProdTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
               "^FO360,{0}^ADN,18,10^FD{2}^FS" +
               "^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineLot, "^FO160,{0}^ADN,18,10^FDLot: {1}^FS" +
                "^FO360,{0}^ADN,18,10^FD{2}^FS" +
                "^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineListPrice, "^FO15,{0}^ADN,18,10^FDPrice: {1}  Total: {2}^FS");
            linesTemplates.Add(InventoryProdQtyItems, "^FO80,{0}^ADN,18,10^FD  TOTAL QTY: {1}^FS");
            linesTemplates.Add(InventoryProdInvValue, "^FO80,{0}^ADN,18,10^FD INV. VALUE: {1}^FS");

            #endregion

            #region Orders Created

            linesTemplates.Add(OrderCreatedReportHeader, "^CF0,40^FO15,{0}^FDSales Register Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(OrderCreatedReporWorkDay, "^FO15,{0}^ADN,36,20^FDClock in: {1}  Clock out: {2} Worked {3}h:{4}m^FS");
            linesTemplates.Add(OrderCreatedReporBreaks, "^FO15,{0}^ADN,36,20^FDBreak taken: {1}h:{2}m^FS");
            linesTemplates.Add(OrderCreatedReportTableHeader, "^FO15,{0}^ABN,18,10^FDNAME^FS" +
                "^FO200,{0}^ABN,18,10^FDST^FS" +
                "^FO250,{0}^ABN,18,10^FDQTY^FS" +
                "^FO300,{0}^ABN,18,10^FDTICKET #.^FS" +
                "^FO400,{0}^ABN,18,10^FDTOTAL^FS" +
                "^FO500,{0}^ABN,18,10^FDCS TP^FS");
            linesTemplates.Add(OrderCreatedReportTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO200,{0}^ABN,18,10^FD{2}^FS" +
                "^FO250,{0}^ABN,18,10^FD{3}^FS" +
                "^FO300,{0}^ABN,18,10^FD{4}^FS" +
                "^FO400,{0}^ABN,18,10^FD{5}^FS" +
                "^FO500,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(OrderCreatedReportTableLine1, "^FO15,{0}^ABN,18,10^FDClock In: {1}  Clock Out: {2}   # Copies: {3}^FS");
            linesTemplates.Add(OrderCreatedReportTableTerms, "^FO15,{0}^ADN,18,10^FDTerms: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment, "^FO15,{0}^ABN,18,10^FDNS Comment: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment1, "^FO15,{0}^ABN,18,10^FDRF Comment: {1}^FS");
            linesTemplates.Add(OrderCreatedReportSubtotal, "^FO350,{0}^ABN,18,10^FDSubtotal:^FS^FO450,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTax, "^FO350,{0}^ABN,18,10^FDTax:^FS^FO450,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTotals, "^FO350,{0}^ABN,18,10^FDTotals:^FS^FO450,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportPaidCust, "^FO15,{0}^ABN,18,10^FDPaid Cust:           {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDVoided:       {2}^FS");
            linesTemplates.Add(OrderCreatedReportChargeCust, "^FO15,{0}^ABN,18,10^FDCharge Cust:         {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDDelivery:     {2}^FS");
            linesTemplates.Add(OrderCreatedReportCreditCust, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FDP&P:          {2}^FS");
            linesTemplates.Add(OrderCreatedReportExpectedCash, "^FO15,{0}^ABN,18,10^FDExpected Cash Cust:  {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FD  Refused:    {2}^FS");
            linesTemplates.Add(OrderCreatedReportFullTotal, "^FO15,{0}^ABN,18,10^FDTotal Sales:         {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDTime (Hours): {2}^FS");

            linesTemplates.Add(OrderCreatedReportCreditTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FDCredit Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportNetTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD   Net Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportBillTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD  Bill Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportSalesTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD Sales Total: {2}^FS");

            #endregion

            #region Payments Report

            linesTemplates.Add(PaymentReportHeader, "^CF0,40^FO15,{0}^FDPayments Received Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(PaymentReportTableHeader, "^FO15,{0}^ABN,18,10^FDName^FS" +
                "^FO230,{0}^ABN,18,10^FDInv #^FS" +
                "^FO350,{0}^ABN,18,10^FDInv Total^FS" +
                "^FO470,{0}^ABN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportTableHeader1, "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO230,{0}^ABN,18,10^FDMethod^FS" +
                "^FO350,{0}^ABN,18,10^FDRef Number^FS" +
                "^FO470,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(PaymentReportTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO230,{0}^ABN,18,10^FD{2}^FS" +
                "^FO350,{0}^ABN,18,10^FD{3}^FS" +
                "^FO470,{0}^ABN,18,10^FD{4}^FS");
            linesTemplates.Add(PaymentReportTotalCash, "^FO250,{0}^ABN,18,10^FDCash: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCheck, "^FO240,{0}^ABN,18,10^FDCheck: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCC, "^FO190,{0}^ABN,18,10^FDCredit Card: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalMoneyOrder, "^FO190,{0}^ABN,18,10^FDMoney Order: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTransfer, "^FO190,{0}^ABN,18,10^FDTransfer: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTotal, "^FO245,{0}^ABN,18,10^FDTotal: ^FS" +
                "^FO350,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentSignatureText, "^FO15,{0}^ADN,18,10^FDPayment Received By^FS");

            #endregion

            #region Settlement

            linesTemplates.Add(InventorySettlementHeader, "^CF0,40^FO15,{0}^FDSettlement Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(InventorySettlementProductHeader, "^FO15,{0}^ABN,18,10^FDProduct^FS");
            linesTemplates.Add(InventorySettlementTableHeader,
                "^FO15,{0}^ABN,18,10^FDUoM^FS" +
                "^FO105,{0}^ABN,18,10^FDBeg.I^FS" +
                "^FO195,{0}^ABN,18,10^FDLoad^FS" +
                "^FO285,{0}^ABN,18,10^FDAdj^FS" +
                "^FO375,{0}^ABN,18,10^FDTr.^FS" +
                "^FO455,{0}^ABN,18,10^FDSls^FS" +
                "^FO540,{0}^ABN,18,10^FDRet^FS");
            linesTemplates.Add(InventorySettlementTableHeader1,
                "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO105,{0}^ABN,18,10^FDDump^FS" +
                "^FO195,{0}^ABN,18,10^FDResh^FS" +
                "^FO285,{0}^ABN,18,10^FDDmg^FS" +
                "^FO375,{0}^ABN,18,10^FDUnl^FS" +
                "^FO455,{0}^ABN,18,10^FDEnd.I^FS" +
                "^FO540,{0}^ABN,18,10^FDO/S^FS");
            linesTemplates.Add(InventorySettlementProductLine, "^FO15,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(InventorySettlementLotLine, "^FO15,{0}^ADN,18,10^FDLot: {1}^FS");
            linesTemplates.Add(InventorySettlementTableLine,
                "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO105,{0}^ABN,18,10^FD{2}^FS" +
                "^FO195,{0}^ABN,18,10^FD{3}^FS" +
                "^FO285,{0}^ABN,18,10^FD{4}^FS" +
                "^FO375,{0}^ABN,18,10^FD{5}^FS" +
                "^FO455,{0}^ABN,18,10^FD{6}^FS" +
                "^FO540,{0}^ABN,18,10^FD{7}^FS");
            linesTemplates.Add(InventorySettlementTableTotals,
                "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO105,{0}^ABN,18,10^FD{1}^FS" +
                "^FO195,{0}^ABN,18,10^FD{2}^FS" +
                "^FO285,{0}^ABN,18,10^FD{3}^FS" +
                "^FO375,{0}^ABN,18,10^FD{4}^FS" +
                "^FO455,{0}^ABN,18,10^FD{5}^FS" +
                "^FO540,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(InventorySettlementTableTotals1,
                "^FO15,{0}^ABN,18,10^FD^FS" +
                "^FO105,{0}^ABN,18,10^FD{1}^FS" +
                "^FO195,{0}^ABN,18,10^FD{2}^FS" +
                "^FO285,{0}^ABN,18,10^FD{3}^FS" +
                "^FO375,{0}^ABN,18,10^FD{4}^FS" +
                "^FO455,{0}^ABN,18,10^FD{5}^FS" +
                "^FO540,{0}^ABN,18,10^FD{6}^FS");

            linesTemplates.Add(InventorySettlementAssetTracking, "^FO15,{0}^CF0,33^FB620,1,0,L^FDCRATES: {1}^FS");

            #endregion

            #region Summary


            linesTemplates.Add(InventorySummaryHeader, "^CF0,40^FO15,{0}^FDInventory Summary^FS");
            linesTemplates.Add(InventorySummaryTableHeader,
                "^FO15,{0}^ABN,18,10^FDProduct^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS"
                );
            linesTemplates.Add(InventorySummaryTableHeader1,
                "^FO15,{0}^ABN,18,10^FDLot^FS" +
                "^FO90,{0}^ABN,18,10^FDUoM^FS" +
                "^FO140,{0}^ABN,18,10^FDBeg.I^FS" +
                "^FO230,{0}^ABN,18,10^FDLoad^FS" +
                "^FO320,{0}^ABN,18,10^FDTr.^FS" +
                "^FO410,{0}^ABN,18,10^FDSls^FS" +
                "^FO500,{0}^ABN,18,10^FDCurr^FS");
            linesTemplates.Add(InventorySummaryTableProductLine,
                "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO90,{0}^ABN,18,10^FD{2}^FS" +
                "^FO140,{0}^ABN,18,10^FD{3}^FS" +
                "^FO230,{0}^ABN,18,10^FD{4}^FS" +
                "^FO320,{0}^ABN,18,10^FD{5}^FS" +
                "^FO410,{0}^ABN,18,10^FD{6}^FS" +
                "^FO500,{0}^ABN,18,10^FD{7}^FS");
            linesTemplates.Add(InventorySummaryTableLine,
                "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO90,{0}^ABN,18,10^FD{2}^FS" +
                "^FO140,{0}^ABN,18,10^FD{3}^FS" +
                "^FO230,{0}^ABN,18,10^FD{4}^FS" +
                "^FO320,{0}^ABN,18,10^FD{5}^FS" +
                "^FO410,{0}^ABN,18,10^FD{6}^FS" +
                "^FO500,{0}^ABN,18,10^FD{7}^FS");
            linesTemplates.Add(InventorySummaryTableTotals,
                "^FO15,{0}^ABN,18,10^FDTotals:{1}^FS" +
                "^FO15,{0}^ABN,18,10^FD{2}^FS" +
                "^FO90,{0}^ABN,18,10^FD{3}^FS" +
                "^FO140,{0}^ABN,18,10^FD{4}^FS" +
                "^FO230,{0}^ABN,18,10^FD{5}^FS" +
                "^FO320,{0}^ABN,18,10^FD{6}^FS" +
                "^FO410,{0}^ABN,18,10^FD{7}^FS" +
                "^FO500,{0}^ABN,18,10^FD{8}^FS");
            linesTemplates.Add(InventorySummaryTableTotals1,
                "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO15,{0}^ABN,18,10^FD{2}^FS" +
                "^FO90,{0}^ABN,18,10^FD{3}^FS" +
                "^FO140,{0}^ABN,18,10^FD{4}^FS" +
                "^FO230,{0}^ABN,18,10^FD{5}^FS" +
                "^FO320,{0}^ABN,18,10^FD{6}^FS" +
                "^FO410,{0}^ABN,18,10^FD{7}^FS" +
                "^FO500,{0}^ABN,18,10^FD{8}^FS");

            #endregion

            #region Consignment Invoice

            linesTemplates.Add(ConsignmentInvoiceHeader, "^FO15,{0}^ADN,36,20^FDInvoice: {1}^FS");
            linesTemplates.Add(ConsignmentSalesOrderHeader, "^FO15,{0}^ADN,36,20^FDSales Order^FS");
            linesTemplates.Add(ConsignmentInvoiceTableHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS" +
                "^FO240,{0}^ADN,18,10^FDCons^FS" +
                "^FO300,{0}^ADN,18,10^FDCount^FS" +
                "^FO360,{0}^ADN,18,10^FDSold^FS" +
                "^FO420,{0}^ADN,18,10^FDPrice^FS" +
                "^FO505,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO240,{0}^ADN,18,10^FD{2}^FS" +
                "^FO300,{0}^ADN,18,10^FD{3}^FS" +
                "^FO360,{0}^ADN,18,10^FD{4}^FS" +
                "^FO420,{0}^ADN,18,10^FD{5}^FS" +
                "^FO505,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLineLot, "^FO15,{0}^ADN,18,10^FDLot: ^FS" +
                "^FO80,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableTotal, "^FO260,{0}^ADN,18,10^FDTotal^FS" +
                "^FO360,{0}^ADN,18,10^FD{1}^FS" +
                "^FO505,{0}^ADN,18,10^FD{2}^FS");

            #endregion

            #region Consignment Contract

            linesTemplates.Add(ConsignmentContractHeader, "^FO15,{0}^ADN,36,20^FDConsignment Contract^FS");
            linesTemplates.Add(ConsignmentContractTableHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS" +
                "^FO250,{0}^ADN,18,10^FDCons^FS" +
                "^FO315,{0}^ADN,18,10^FDCons^FS" +
                "^FO380,{0}^ADN,18,10^FDPrice^FS" +
                "^FO470,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentContractTableHeader2, "^FO15,{0}^ADN,18,10^FD^FS" +
                "^FO250,{0}^ADN,18,10^FDOld^FS" +
                "^FO315,{0}^ADN,18,10^FDNew^FS" +
                "^FO380,{0}^ADN,18,10^FD^FS" +
                "^FO470,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(ConsignmentContractTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO250,{0}^ADN,18,10^FD{2}^FS" +
                "^FO315,{0}^ADN,18,10^FD{3}^FS" +
                "^FO380,{0}^ADN,18,10^FD{4}^FS" +
                "^FO470,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(ConsignmentContractTableTotal, "^FO350,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO250,{0}^ADN,18,10^FD{1}^FS" +
                "^FO315,{0}^ADN,18,10^FD{2}^FS" +
                "^FO470,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Route Return

            linesTemplates.Add(RouteReturnsTitle, "^FO15,{0}^ADN,36,20^FDRoute Return Report^FS");
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO15,{0}^ADN,28,14^FDNOT FINAL ROUTE RETURN^FS");
            linesTemplates.Add(RouteReturnsTableHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS" +
                "^FO300,{0}^ADN,18,10^FDRef^FS" +
                "^FO350,{0}^ADN,18,10^FDDump^FS" +
                "^FO410,{0}^ADN,18,10^FDRet^FS" +
                "^FO460,{0}^ADN,18,10^FDDmg^FS" +
                "^FO510,{0}^ADN,18,10^FDUnload^FS");
            linesTemplates.Add(RouteReturnsTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO350,{0}^ADN,18,10^FD{3}^FS" +
                "^FO410,{0}^ADN,18,10^FD{4}^FS" +
                "^FO460,{0}^ADN,18,10^FD{5}^FS" +
                "^FO510,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(RouteReturnsTotals, "^FO15,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO350,{0}^ADN,18,10^FD{3}^FS" +
                "^FO410,{0}^ADN,18,10^FD{4}^FS" +
                "^FO460,{0}^ADN,18,10^FD{5}^FS" +
                "^FO510,{0}^ADN,18,10^FD{6}^FS");

            #endregion

            #region Credit Report

            linesTemplates.Add(CreditReportDetailsHeader, "^FO15,{0}^ADN,18,10^FDNAME^FS" +
              "^FO290,{0}^ADN,18,10^FDTYPE^FS" +
              "^FO365,{0}^ADN,18,10^FDQTY^FS" +
              "^FO415,{0}^ADN,18,10^FDU.PRICE^FS" +
              "^FO510,{0}^ADN,18,10^FDTOTAL^FS");

            linesTemplates.Add(CreditReportDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
            "^FO290,{0}^ADN,18,10^FD{2}^FS" +
            "^FO365,{0}^ADN,18,10^FD{3}^FS" +
            "^FO415,{0}^ADN,18,10^FD{4}^FS" +
            "^FO510,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(CreditReportDetailsTotal, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
        "^FO365,{0}^ADN,18,10^FD{2}^FS" +
        "^FO510,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(CreditReportTotalsLine, "^FO320,{0}^ADN,18,10^FD{1}^FS" +
          "^FO485,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(CreditReportHeader, "^CF0,50^FO15,{0}^FDCredit Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");

            linesTemplates.Add(CreditReportClientName, "^FO15,{0}^AON,25^FD{1}^FS" +
       "^FO500,{0}^ADN,18,10^FD{2}^FS" +
       "^FO680,{0}^ADN,18,10^FD{3}^FS");


            #endregion

            #region Payment

            linesTemplates.Add(PaymentTitle, "^FO15,{0}^ADN,36,20^FDPayment Receipt^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO15,{0}^ADN,36,20^FDCustomer:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentInvoiceNumber, "^FO15,{0}^ADN,18,10^FD{1} #: {2}^FS");
            linesTemplates.Add(PaymentInvoiceTotal, "^FO15,{0}^ADN,18,10^FD{1} Total: {2}^FS");
            linesTemplates.Add(PaymentPaidInFull, "^FO15,{0}^ADN,18,10^FDPaid in Full: {1}^FS");
            linesTemplates.Add(PaymentComponents, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentTotalPaid, "^FO15,{0}^ADN,36,20^FDTotal Paid: {1}^FS");
            linesTemplates.Add(PaymentPending, "^FO15,{0}^ADN,36,20^FD   Pending: {1}^FS");

            #endregion

            #region Open Invoice

            linesTemplates.Add(InvoiceTitle, "^CF0,40^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(InvoiceCopy, "^FO15,{0}^ADN,36,20^FDCOPY^FS");
            linesTemplates.Add(InvoiceDueOn, "^FO15,{0}^ADN,18,10^FDDue on:    {1}^FS");
            linesTemplates.Add(InvoiceDueOnOverdue, "^FO15,{0}^ADN,18,10^FDDue on:    {1} OVERDUE^FS");
            linesTemplates.Add(InvoiceClientName, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InvoiceCustomerNumber, "^FO15,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(InvoiceClientAddr, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(InvoiceClientBalance, "^FO15,{0}^ADN,18,10^FDAccount Balance: {1}^FS");
            linesTemplates.Add(InvoiceComment, "^FO15,{0}^ADN,18,10^FDC: {1}^FS");
            linesTemplates.Add(InvoiceTableHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO275,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO480,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(InvoiceTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO275,{0}^ADN,18,10^FD{2}^FS" +
                "^FO390,{0}^ADN,18,10^FD{3}^FS" +
                "^FO480,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoiceTotal, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO160,{0}^ADN,18,10^FD{2}^FS" +
                "^FO260,{0}^ADN,18,10^FD{3}^FS" +
                "^FO480,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoicePaidInFull,       "^CF0,30^FO120,{0}^FD   PAID IN FULL^FS");
            linesTemplates.Add(InvoicePaidInFullCredit, "^CF0,30^FO120,{0}^FD   COLLECTED IN FULL^FS");
            linesTemplates.Add(InvoiceCredit,           "^CF0,30^FO120,{0}^FD   CREDIT^FS");
            linesTemplates.Add(InvoicePartialPayment,   "^CF0,30^FO120,{0}^FDPARTIAL PAYMENT: {1}^FS");
            linesTemplates.Add(InvoiceOpen,             "^CF0,30^FO120,{0}^FD                   OPEN: {1}^FS");
            linesTemplates.Add(InvoiceQtyItems,         "^CF0,30^FO120,{0}^FD              QTY ITEMS: {1}^FS");
            linesTemplates.Add(InvoiceQtyUnits,         "^CF0,30^FO120,{0}^FD              QTY UNITS: {1}^FS");

            #endregion

            #region Transfer

            linesTemplates.Add(TransferOnHeader, "^FO15,{0}^ADN,36,20^FDTransfer On Report^FS");
            linesTemplates.Add(TransferOffHeader, "^FO15,{0}^ADN,36,20^FDTransfer Off Report^FS");
            linesTemplates.Add(TransferNotFinal, "^FO15,{0}^ADN,28,14^FDNOT A FINAL TRANSFER^FS");
            // linesTemplates.Add(TransferTableHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO360,{0}^ADN,18,10^FDUoM^FS^FO430,{0}^ADN,18,10^FDTransferred^FS");
            linesTemplates.Add(TransferTableHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS" +
               "^FO330,{0}^ADN,18,10^FDLot^FS" +
               "^FO400,{0}^ADN,18,10^FDUoM^FS" +
               "^FO460,{0}^ADN,18,10^FDTransf.^FS");
            linesTemplates.Add(TransferTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO330,{0}^ADN,18,10^FD{2}^FS" +
                "^FO400,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS");

            //   linesTemplates.Add(TransferTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO430,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(TransferTableLinePrice, "^FO15,{0}^ADN,18,10^FDList Price: {1}^FS");
            linesTemplates.Add(TransferQtyItems, "^FO100,{0}^ADN,18,10^FD      QTY ITEMS: {1}^FS");
            linesTemplates.Add(TransferAmount, "^FO100,{0}^ADN,18,10^FD TRANSFER VALUE: {1}^FS");
            linesTemplates.Add(TransferComment, "^FO15,{0}^ADN,18,10^FDComment: {1}^FS");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "^FO40,{0}^ADN,28,14^FDCustomer Open Balance^FS");
            linesTemplates.Add(ClientStatementTableHeader, "^FO40,{0}^ADN,18,10^FDType^FS" +
                "^FO200,{0}^ADN,18,10^FDDate^FS" +
                "^FO370,{0}^ADN,18,10^FDDue Date^FS");
            linesTemplates.Add(ClientStatementTableHeader1, "^FO40,{0}^ADN,18,10^FDNumber^FS" +
                "^FO200,{0}^ADN,18,10^FDAmount^FS" +
                "^FO370,{0}^ADN,18,10^FDOpen^FS");
            linesTemplates.Add(ClientStatementTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO200,{0}^ADN,18,10^FD{2}^FS" +
                "^FO370,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(ClientStatementCurrent, "^FO100,{0}^ADN,28,14^FD              Current: {1}^FS");
            linesTemplates.Add(ClientStatement1_30PastDue, "^FO100,{0}^ADN,28,14^FD   1-30 Days Past Due: {1}^FS");
            linesTemplates.Add(ClientStatement31_60PastDue, "^FO100,{0}^ADN,28,14^FD  31-60 Days Past Due: {1}^FS");
            linesTemplates.Add(ClientStatement61_90PastDue, "^FO100,{0}^ADN,28,14^FD  61-90 Days Past Due: {1}^FS");
            linesTemplates.Add(ClientStatementOver90PastDue, "^FO100,{0}^ADN,28,14^FDOver 90 Days Past Due: {1}^FS");
            linesTemplates.Add(ClientStatementAmountDue, "^FO100,{0}^ADN,28,14^FD           Amount Due: {1}^FS");

            #endregion

            #region Inventory Count

            linesTemplates.Add(InventoryCountHeader, "^FO40,{0}^ADN,36,20^FDInventory Count^FS");
            linesTemplates.Add(InventoryCountTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO600,{0}^ADN,18,10^FDQTY^FS" +
                "^FO680,{0}^ADN,18,10^FDUOM^FS");
            linesTemplates.Add(InventoryCountTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Accepted Orders Report

            linesTemplates.Add(AcceptedOrdersHeader, "^FO15,{0}^ADN,36,20^FDAccepted Orders Report^FS");
            linesTemplates.Add(AcceptedOrdersDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AcceptedOrdersDeliveriesLabel, "^CF0,35^FO15,{0}^FDDeliveries^FS");
            linesTemplates.Add(AcceptedOrdersCreditsLabel, "^CF0,35^FO15,{0}^FDCredits^FS");
            linesTemplates.Add(AcceptedOrdersDeliveriesTableHeader, "^CF0,30" +
                "^FO15,{0}^FDCustomer^FS" +
                "^FO300,{0}^FDQty^FS" +
                "^FO370,{0}^FDWeight^FS" +
                "^FO470,{0}^FDAmount^FS");
            linesTemplates.Add(AcceptedOrdersTableLine, "^CFA,20" +
                "^FO15,{0}^FD{1}^FS" +
                "^FO300,{0}^FD{2}^FS" +
                "^FO370,{0}^FD{3}^FS" +
                "^FO470,{0}^FD{4}^FS");
            linesTemplates.Add(AcceptedOrdersTableLine2, "^CF0,20" +
                "^FO15,{0}^FD{1}^FS" +
                "^FO120,{0}^FD{2}^FS");
            linesTemplates.Add(AcceptedOrdersLoadsTableHeader, "^CF0,35^FO15,{0}^FDLoad Orders^FS");
            linesTemplates.Add(AcceptedOrdersTableTotals, "^CF0,20" +
                "^FO200,{0}^FDTotals:^FS" +
                "^FO300,{0}^FD{1}^FS" +
                "^FO370,{0}^FD{2}^FS" +
                "^FO470,{0}^FD{3}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsQty, "^CF0,35" +
                "^FO270,{0}^FD     Total Qty: ^FS^FO500,{0}^FD{1}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsWeight, "^CF0,35" +
                "^FO270,{0}^FDTotal Weight: ^FS^FO500,{0}^FD{1}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsAmount, "^CF0,35" +
                "^FO270,{0}^FD       Amount: ^FS^FO500,{0}^FD{1}^FS");
            #endregion

            #region Refusal Report

            linesTemplates.Add(RefusalReportHeader, "^CF0,40^FO15,{0}^FDRefusal Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(RefusalReportTableHeader, "^CF0,25^FO15,{0}^FDReason: {1}^FS" +
                "^FO400,{0}^FDOrder #^FS");
            linesTemplates.Add(RefusalReportTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO400,{0}^ABN,18,10^FD{2}^FS");
            linesTemplates.Add(RefusalReportProductTableHeader, "^CF0,25^^FO15,{0}^FDProduct^FS" +
                "^FO400,{0}^FDQty^FS");
            linesTemplates.Add(RefusalReportProductTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO400,{0}^ABN,18,10^FD{2}^FS");

            #endregion

            #region New Refusal Report

            linesTemplates.Add(NewRefusalReportTableHeader1, "^CF0,35^^FO15,{0}^FDRefused By Store^FS");
            linesTemplates.Add(NewRefusalReportTableHeader, "^CF0,25^^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(NewRefusalReportProductTableHeader, "^CF0,25^^FO15,{0}^FDProduct^FS" + "^FO340,{0}^FDQty^FS" + "^FO420,{0}^FDReason^FS");
            linesTemplates.Add(NewRefusalReportProductTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO340,{0}^ADN,18,10^FD{2}^FS" + "^FO420,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Payment Deposit
            linesTemplates.Add(ChecksTitle, "^FO15,{0}^AON,30,15^FDList Of Checks^FS");
            linesTemplates.Add(BatchDate, "^FO15,{0}^ADN,18,10^FDPosted Date: {1}^FS");
            linesTemplates.Add(BatchPrintedDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(BatchSalesman, "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS");
            linesTemplates.Add(BatchBank, "^FO15,{0}^ADN,18,10^FDBank: {1}^FS");
            linesTemplates.Add(CheckTableHeader, "^FO15,{0}^ADN,18,10^FDIDENTIFICATION CHECKS^FS" + "^FO450,{0}^ADN,18,10^FDAMOUNT^FS");
            linesTemplates.Add(CheckTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO450,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(CheckTableTotal, "^FO15,{0}^ADN,18,10^FD# OF CHECKS: {1}^FS" + "^FO290,{0}^ADN,18,10^FDTOTAL CHECK: {2}^FS");
            linesTemplates.Add(CashTotalLine, "^FO15,{0}^ADN,18,10^FDTOTAL CASH: {1}^FS");
            linesTemplates.Add(CreditCardTotalLine, "^FO15,{0}^ADN,18,10^FDTOTAL CREDIT CARD: {1}^FS");
            linesTemplates.Add(MoneyOrderTotalLine, "^FO15,{0}^ADN,18,10^FDTOTAL MONEY ORDER: {1}^FS");
            linesTemplates.Add(BatchTotal, "^FO15,{0}^AON,30,15^FDTOTAL DEPOSIT: {1}^FS");
            linesTemplates.Add(BatchComments, "^FO15,{0}^ADN,18,10^FDComments: {1}^FS");

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "F015,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(DeliveryInvoiceNumber, "^FO15,169^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO480,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsHeaderUoMDelivery, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO345,{0}^ADN,18,10^FDUOM^FS" + "^FO480,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO489,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderDetailsTotalsUoMDelivery, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO345,{0}^ADN,18,10^FD{3}^FS" + "^FO489,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TotalQtysProofDelivery, "^FO410,{0}^ADN,18,10^FDTOTAL: {1}^FS");
            linesTemplates.Add(StandarPrintTitleProofDelivery, "F015,126^ADN,36,20^FD{0}^FS");

            #endregion

            #region pick ticket


            linesTemplates.Add(PickTicketCompanyHeader, "^FO15,{0}^CF0,33^FB520,1,0,L^FD{1}^FS");
            linesTemplates.Add(PickTicketRouteInfo, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PickTicketDeliveryDate, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PickTicketDriver, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS");


            linesTemplates.Add(PickTicketProductHeader, "^FO15,{0}^ADN,18,10^FDPRODUCT #^FS" +
          "^FO180,{0}^ADN,18,10^FDDESCRIPTION^FS" + //310
          "^FO430,{0}^ADN,18,10^FDCASES^FS" +
          "^FO520,{0}^ADN,18,10^FDUNITS^FS");

            linesTemplates.Add(PickTicketProductLine, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
    "^FO180,{0}^ADN,18,10^FD{2}^FS" +
    "^FO440,{0}^ADN,18,10^FD{3}^FS" +
    "^FO530,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(PickTicketProductTotal, "^FO15,{0}^ADN,18,10^FDTOTALS^FS" +
       "^FO440,{0}^ADN,18,10^FD{1}^FS" +
       "^FO530,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(VehicleInformationHeader, "^CF0,50^FO15,{0}^FDVehicle Information Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(VehicleInformationHeader1, "^CF0,50^FO15,{0}^FDVehicle Information^FS");

            #endregion
        }


        protected override int WidthForBoldFont
        {
            get
            {
                return 43;
            }
        }

        protected override int WidthForNormalFont
        {
            get
            {
                return 47;
            }
        }

        protected override int SpaceForOrderFooter
        {
            get
            {
                int i = 14;
                return i;
            }
        }

        protected override int SpaceForPadding
        {
            get
            {
                int i = 15;
                return i;
            }
        }
        protected override IEnumerable<string> SplitRefusalReportLines(string productName)
        {
            return SplitProductName(productName, 25, 50);
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 22, 22);
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

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 15, 18);
        }

        protected override IEnumerable<string> GetLoadOrderDetailsRowSplitProductName(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IEnumerable<string> GetAcceptLoadDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 26, 36);
        }

        protected override IEnumerable<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 26, 36);
        }

        protected override IEnumerable<string> GetInventoryProdDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IEnumerable<string> GetInventorySettlementRowsSplitProductName(string name)
        {
            return SplitProductName(name, 23, 23);
        }

        protected override IEnumerable<string> GetConsInvoiceDetailRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IEnumerable<string> GetRouteReturnRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IEnumerable<string> GetOpenInvoiceCommentSplit(string v)
        {
            return SplitProductName(v, 45, 45);
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
            return SplitProductName(name, 16, 16);
        }

        protected override IEnumerable<string> GetInventoryCountDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IEnumerable<string> GetAcceptedLoadSplitClientName(string clientName)
        {
            return SplitProductName(clientName, 29, 29);
        }

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
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableLine], startY,
                                p.ClientName,
                                p.DocNumber,
                                p.DocAmount,
                                p.Paid));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableLine], startY,
                                string.Empty,
                                p.PaymentMethod,
                                p.RefNumber,
                                string.Empty));
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
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
                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals, startY,
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

                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals1, startY,
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

        #region Inventory Summary

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


                float factor = 1;
                if (!isBase)
                {
                    var defaultUom = p.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                    {
                        factor = defaultUom.Conversion;
                        p.UoM = defaultUom;
                    }
                }

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

        #endregion


        #region Payment

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

        #region Client Statement

        protected override IEnumerable<string> GetClientStatementTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader1], startY));
            startY += font36Separation;

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

                int factor = item.InvoiceType == 1 ? -1 : 1;

                lines.Add(GetClientStatementFixedLine(ClientStatementTableLine,
                    startY,
                    GetClientStatementInvoiceType(item.InvoiceType),
                    item.Date.ToShortDateString(),
                    item.DueDate.ToShortDateString(),
                    "",
                    "",
                    ""));

                startY += font18Separation;

                lines.Add(GetClientStatementFixedLine(ClientStatementTableLine,
                    startY,
                    item.InvoiceNumber,
                    ToString(item.Amount),
                    ToString(item.Balance),
                    "",
                    "",
                    ""));

                startY += font36Separation;

                current += item.Balance * factor;

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

        #endregion

        protected override IEnumerable<string> GetHeaderRowsInOneDocDelivery(ref int startY, Order order, Client client, string printedId)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = GetOrderDocumentNameDelivery(ref printExtraDocName, order, client);

            string s1 = docName;
            string s2= string.Empty;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
            {
                s1 = docName;
                s2 = order.PrintedOrderId;
            }
                

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, s1, string.Empty));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, 10, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, s2, string.Empty));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, 10, 1);

            if (Config.UseBigFontForPrintDate)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDateBig], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
                startY += 40;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            if (salesman != null)
            {
                if (order.IsDelivery)
                {
                    var originalSalesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, salesman.Name));
                    startY += font18Separation;

                    if (originalSalesman != null)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, originalSalesman.Name));
                        startY += font18Separation;
                    }
                }
                else
                {
                    if (salesman.Roles == SalesmanRole.Driver)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
                        startY += font18Separation;
                    }
                    else
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, Config.VendorName));
                        startY += font18Separation;
                    }
                }
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = linesTemplates[OrderBillTo];

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderBillTo1];
                }

                startY += font18Separation;
                addrFormat1 = linesTemplates[OrderShipTo];

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderShipTo1];
                }
            }
            else
            {
                foreach (string s in ClientAddress(client))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
                    startY += font18Separation;
                }
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

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, term));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = ToString(order.Client.OpenBalance);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderAccountBalance], startY, balance));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            if (Config.PrintInvoiceNumberDown)
                if (printExtraDocName)
                {
                    if (order.AsPresale && order.OrderType == OrderType.Order)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, ""));
                    startY += font36Separation + font18Separation;
                }

            if (!Config.HidePONumber)
            {
                if (!string.IsNullOrEmpty(order.PONumber))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
                else if (Config.AutoGeneratePO)
                {
                    order.PONumber = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
            }

            return lines;
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
                var splitQtyAsString = SplitProductName(qtyAsString, 8, 8);

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

                        //string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        //if (detail.OrderDetail.UnitOfMeasure != null)
                        //    qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
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
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, pName, currentQty));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                while (productLineOffset < splitQtyAsString.Count)
                {
                    string remainingQty = splitQtyAsString[productLineOffset];
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, string.Empty, remainingQty)); //OrderDetailsLines2
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
                    /* foreach (var detail1 in detail.ParticipatingDetails)
                     {
                         if (!detail1.Product.SoldByWeight)
                             continue;

                         if (!string.IsNullOrEmpty(weights))
                             weights += ",";
                         weights += detail1.Weight;
                     }*/

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
                    string adjustedKey = AdjustPadding(key);
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

        private string AdjustPadding(string input, int safetyGap = 3)
        {
            //var maxCharacters = SpaceForPadding;
            var maxCharacters = SpaceForPadding + 5;
            //To justify UoM at the bottom
            input = input + ":";

            if (input.Length > maxCharacters - safetyGap)
            {
                input = input.Substring(0, maxCharacters - safetyGap - 3) + "...:";
            }

            int spaceForInput = maxCharacters - safetyGap;

            int numberOfSpaces = spaceForInput - input.Length;

            numberOfSpaces = Math.Max(0, numberOfSpaces);

            return new string(' ', numberOfSpaces) + input;
        }
    }
}