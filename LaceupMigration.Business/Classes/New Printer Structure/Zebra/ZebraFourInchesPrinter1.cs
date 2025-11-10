using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LaceupMigration
{
    public class ZebraFourInchesPrinter1 : ZebraPrinter1
    {
        protected override void FillDictionary()
        {
            linesTemplates.Add(EndLabel, "^XZ");
            linesTemplates.Add(StartLabel, "^XA^PON^MNN^LL{0}");

            linesTemplates.Add(Upc128, "^FO40,{0}^BCN,40^FD{1}^FS");

            linesTemplates.Add(UPC128ForLabel, "^FO50,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(RetailPrice, "^FO130,{0}^ADN,18,16^FD{1}^FS");


            #region Standard

            linesTemplates.Add(StandarPrintTitle, "^FO40,{0}^ADN,36,20^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(StandarPrintDate, "^FO40,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintDateBig, "^CF0,30^FO40,{0}^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintRouteNumber, "^FO40,{0}^ADN,18,10^FDRoute #: {1}^FS");
            linesTemplates.Add(StandarPrintDriverName, "^FO40,{0}^ADN,18,10^FDDriver Name: {1}^FS");
            linesTemplates.Add(StandarPrintCreatedBy, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS");
            linesTemplates.Add(StandarPrintedDate, "^FO40,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(StandarPrintedOn, "^FO40,{0}^ADN,18,10^FDPrinted On: {1}^FS");
            linesTemplates.Add(StandarCreatedOn, "^FO40,{0}^ADN,18,10^FDCreated On: {1}^FS");

            #endregion

            #region Company

            linesTemplates.Add(CompanyName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(CompanyAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(CompanyPhone, "^FO40,{0}^ADN,18,10^FDPhone: {1}^FS");
            linesTemplates.Add(CompanyFax, "^FO40,{0}^ADN,18,10^FDFax: {1}^FS");
            linesTemplates.Add(CompanyEmail, "^FO40,{0}^ADN,18,10^FDEmail: {1}^FS");
            linesTemplates.Add(CompanyLicenses1, "^FO40,{0}^ADN,18,10^FDLicenses: {1}^FS");
            linesTemplates.Add(CompanyLicenses2, "^FO40,{0}^ADN,18,10^FD          {1}^FS");

            #endregion

            #region Order

            linesTemplates.Add(OrderClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderClientNameTo, "^FO40,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(OrderClientAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderBillTo, "^FO40,{0}^ADN,18,10^FDBill To: {1}^FS");
            linesTemplates.Add(OrderBillTo1, "^FO40,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderShipTo, "^FO40,{0}^ADN,18,10^FDShip To: {1}^FS");
            linesTemplates.Add(OrderShipTo1, "^FO40,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderClientLicenceNumber, "^FO40,{0}^ADN,18,10^FDLicense Number: {1}^FS");
            linesTemplates.Add(OrderVendorNumber, "^FO40,{0}^ADN,18,10^FDVendor Number: {1}^FS");
            linesTemplates.Add(OrderTerms, "^FO40,{0}^ADN,18,10^FDTerms: {1}^FS");
            linesTemplates.Add(OrderAccountBalance, "^FO40,{0}^ADN,18,10^FDAccount Balance: {1}^FS");
            linesTemplates.Add(OrderTypeAndNumber, "^FO40,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(PONumber, "^FO40,{0}^ADN,36,20^FDPO #: {1}^FS");

            linesTemplates.Add(OrderPaymentText, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderHeaderText, "^FO40,{0}^ADN,36,20^FD{1}^FS");

            linesTemplates.Add(OrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO450,{0}^ADN,18,10^FDQTY^FS" +
                "^FO580,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeaderSectionName, "^FO400,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLines2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines3, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderDetailsLinesLotQty, "^FO40,{0}^ADN,18,10^FDLot: {1} -> {2}^FS");
            linesTemplates.Add(OrderDetailsWeights, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(OrderDetailsWeightsCount, "^FO40,{0}^ADN,18,10^FDQty: {1}^FS");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "^FO40,{0}^ADN,18,10^FDRetail price {1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "^FO60,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "^BY3,3,40^FO60,{0}^BEN,40,Y,N^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotals, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(OrderDetailsTotals1, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                 "^FO50,{0}^ADN,18,10^FD{2}^FS" +
                 "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                 "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(OrderTotalContainers, "^FO40,{0}^ADN,36,20^FD     CONTAINERS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetQty, "^FO40,{0}^ADN,36,20^FD        NET QTY: {1}^FS");
            linesTemplates.Add(OrderSubTotal, "^FO40,{0}^ADN,36,20^FD       SUBTOTAL: {1}^FS");
            linesTemplates.Add(OrderTotalsSales, "^FO40,{0}^ADN,36,20^FD          SALES: {1}^FS");
            linesTemplates.Add(OrderTotalsCredits, "^FO40,{0}^ADN,36,20^FD        CREDITS: {1}^FS");
            linesTemplates.Add(OrderTotalsReturns, "^FO40,{0}^ADN,36,20^FD        RETURNS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetAmount, "^FO40,{0}^ADN,36,20^FD     NET AMOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscount, "^FO40,{0}^ADN,36,20^FD       DISCOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsTax, "^FO40,{0}^ADN,36,20^FD{1} {2}^FS");
            linesTemplates.Add(OrderTotalsTotalDue, "^FO40,{0}^ADN,36,20^FD      TOTAL DUE: {1}^FS");
            linesTemplates.Add(OrderTotalsTotalPayment, "^FO40,{0}^ADN,36,20^FD  TOTAL PAYMENT: {1}^FS");
            linesTemplates.Add(OrderTotalsCurrentBalance,       "^FO40,{0}^ADN,36,20^FDINVOICE BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "^FO40,{0}^ADN,36,20^FD   OPEN BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsFreight, "^FO40,{0}^ADN,36,20^FD        FREIGHT: {1}^FS");
            linesTemplates.Add(OrderTotalsOtherCharges, "^FO40,{0}^ADN,36,20^FD  OTHER CHARGES: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscountComment, "^FO40,{0}^ADN,18,10^FD Discount Comment: {1}^FS");
            linesTemplates.Add(OrderPreorderLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderComment, "^FO40,{0}^ADN,18,10^FDComments: {1}^FS");
            linesTemplates.Add(OrderComment2, "^FO40,{0}^ADN,18,10^FD          {1}^FS");
            linesTemplates.Add(PaymentComment,  "^FO40,{0}^ADN,18,10^FDPayment Comments: {1}^FS");
            linesTemplates.Add(PaymentComment1, "^FO40,{0}^ADN,18,10^FD                  {1}^FS");
            linesTemplates.Add(OrderCommentWork, "^FO40,{0}^AON,24,15^FD{1}^FS");

            #endregion

            #region Footer

            linesTemplates.Add(FooterSignatureLine, "^FO40,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO40,{0}^ADN,18,10^FDSignature^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO40,{0}^ADN,18,10^FDSignature Name: {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO40,{0}^ADN,18,10^FD ^FS");
            linesTemplates.Add(FooterBottomText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FooterDriverSignatureText, "^FO40,{0}^ADN,18,10^FDDriver Signature^FS");

            #endregion

            #region Allowance

            linesTemplates.Add(AllowanceOrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO380,{0}^ADN,18,10^FDQTY^FS" +
                "^FO480,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO580,{0}^ADN,18,10^FDALLOW^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(AllowanceOrderDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO380,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO580,{0}^ADN,18,10^FD-{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{5}^FS");

            #endregion

            #region Shortage Report

            linesTemplates.Add(ShortageReportHeader, "^FO40,{0}^ADN,36,20^FDKNOWN SHORTAGE REPORT^FS");
            linesTemplates.Add(ShortageReportDate, "^FO400,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(ShortageReportInvoiceHeader, "^FO40,{0}^ADN,36,20^FDInvoice #: {1}^FS");
            linesTemplates.Add(ShortageReportTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO500,{0}^ADN,18,10^FDPO QTY^FS" +
                "^FO600,{0}^ADN,18,10^FDSHORT.^FS" +
                "^FO710,{0}^ADN,18,10^FDDEL.^FS");
            linesTemplates.Add(ShortageReportTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO500,{0}^ADN,18,10^FD{2}^FS" +
                "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                "^FO710,{0}^ADN,18,10^FD{4}^FS");

            #endregion

            #region Credit Report

            linesTemplates.Add(CreditReportDetailsHeader, "^FO40,{0}^ADN,18,10^FDNAME^FS" +
              "^FO420,{0}^ADN,18,10^FDTYPE^FS" +
              "^FO510,{0}^ADN,18,10^FDQTY^FS" +
              "^FO570,{0}^ADN,18,10^FDUNIT PRICE^FS" +
              "^FO710,{0}^ADN,18,10^FDTOTAL^FS");

            linesTemplates.Add(CreditReportDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
            "^FO420,{0}^ADN,18,10^FD{2}^FS" +
            "^FO510,{0}^ADN,18,10^FD{3}^FS" +
            "^FO570,{0}^ADN,18,10^FD{4}^FS" +
            "^FO710,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(CreditReportDetailsTotal, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
        "^FO510,{0}^ADN,18,10^FD{2}^FS" +
        "^FO710,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(CreditReportTotalsLine, "^FO500,{0}^ADN,18,10^FD{1}^FS" +
          "^FO680,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(CreditReportHeader, "^CF0,50^FO40,{0}^FDCredit Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");

            linesTemplates.Add(CreditReportClientName, "^FO40,{0}^AON,25^FD{1}^FS" +
       "^FO500,{0}^ADN,18,10^FD{2}^FS" +
       "^FO680,{0}^ADN,18,10^FD{3}^FS");


            #endregion

            #region Load Order

            linesTemplates.Add(LoadOrderHeader, "^FO40,{0}^ADN,36,20^FDLoad Order Report^FS");
            linesTemplates.Add(LoadOrderRequestedDate, "^FO40,{0}^ADN,18,10^FDLoad Order Request Date: {1}^FS");
            linesTemplates.Add(LoadOrderNotFinal, "^FO40,{0}^ADN,28,14^FDNOT A FINAL LOAD ORDER^FS");
            linesTemplates.Add(LoadOrderTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO480,{0}^ADN,18,10^FDUOM^FS" +
                "^FO680,{0}^ADN,18,10^FDORDERED^FS");
            linesTemplates.Add(LoadOrderTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO480,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(LoadOrderTableTotal, "^FO40,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO680,{0}^ADN,18,10^FD{1}^FS");

            #endregion

            #region Accept Load

            linesTemplates.Add(AcceptLoadHeader, "^FO40,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AcceptLoadDate, "^FO400,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AcceptLoadInvoice, "^FO40,{0}^ADN,36,20^FDInvoice #: {1}^FS");
            linesTemplates.Add(AcceptLoadNotFinal, "^FO40,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS");
            linesTemplates.Add(AcceptLoadTableHeader, "^FO40,{0}^ABN,18,10^FDPRODUCT^FS" +
                //"^FO475,{0}^ABN,18,10^FDUoM^FS" +
                "^FO560,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO630,{0}^ABN,18,10^FDADJ^FS" +
                "^FO700,{0}^ABN,18,10^FDINV^FS");
            linesTemplates.Add(AcceptLoadTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO475,{0}^ABN,18,10^FD^FS" +
                "^FO560,{0}^ABN,18,10^FDOUT^FS" +
                "^FO630,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AcceptLoadTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO475,{0}^ABN,18,10^FD{2}^FS" +
                "^FO560,{0}^ABN,18,10^FD{3}^FS" +
                "^FO630,{0}^ABN,18,10^FD{4}^FS" +
                "^FO700,{0}^ABN,18,10^FD{5}^FS");
           linesTemplates.Add(AcceptLoadLotLine, "^FO40,{0}^ABN,18,10^FDLot: {1}^FS");
           linesTemplates.Add(AcceptLoadWeightLine, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(AcceptLoadTableTotals, "^FO350,{0}^ABN,18,10^FD^FS" +
                "^FO475,{0}^ABN,18,10^FDUnits^FS" +
                "^FO560,{0}^ABN,18,10^FD{1}^FS" +
                "^FO630,{0}^ABN,18,10^FD{2}^FS" +
                "^FO700,{0}^ABN,18,10^FD{3}^FS");

            linesTemplates.Add(AcceptLoadTableTotals1, "^FO350,{0}^ABN,18,10^FDTotals:^FS" +
             "^FO475,{0}^ABN,18,10^FDCASE^FS" +
             "^FO560,{0}^ABN,18,10^FD{1}^FS" +
             "^FO630,{0}^ABN,18,10^FD{2}^FS" +
             "^FO700,{0}^ABN,18,10^FD{3}^FS");

            #endregion

            #region Add Inventory

            linesTemplates.Add(AddInventoryHeader, "^FO40,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AddInventoryDate, "^FO400,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AddInventoryNotFinal, "^FO40,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS");
            linesTemplates.Add(AddInventoryTableHeader, "^FO40,{0}^ABN,18,10^FDPRODUCT^FS" +
                "^FO490,{0}^ABN,18,10^FDBEG^FS" +
                "^FO560,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO630,{0}^ABN,18,10^FDADJ^FS" +
                "^FO700,{0}^ABN,18,10^FDSTART^FS");
            linesTemplates.Add(AddInventoryTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
               "^FO490,{0}^ABN,18,10^FDINV^FS" +
               "^FO560,{0}^ABN,18,10^FDOUT^FS" +
               "^FO630,{0}^ABN,18,10^FD^FS" +
               "^FO700,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AddInventoryTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
               "^FO490,{0}^ABN,18,10^FD{2}^FS" +
               "^FO560,{0}^ABN,18,10^FD{3}^FS" +
               "^FO630,{0}^ABN,18,10^FD{4}^FS" +
               "^FO700,{0}^ABN,18,10^FD{5}^FS");
            linesTemplates.Add(AddInventoryTableTotals, "^FO350,{0}^ABN,18,10^FDTotals:^FS" +
                "^FO490,{0}^ABN,18,10^FD{1}^FS" +
                "^FO560,{0}^ABN,18,10^FD{2}^FS" +
                "^FO630,{0}^ABN,18,10^FD{3}^FS" +
                "^FO700,{0}^ABN,18,10^FD{4}^FS");

            #endregion

            #region Inventory

            linesTemplates.Add(InventoryProdHeader, "^FO40,{0}^ADN,18,10^FDInventory Report Date: {1}^FS");
            linesTemplates.Add(InventoryProdTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO600,{0}^ADN,18,10^FDSTART^FS" +
                "^FO680,{0}^ADN,18,10^FDCURRENT^FS");
            linesTemplates.Add(InventoryProdTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineLot, "^FO400,{0}^ADN,18,10^FDLot: {1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineListPrice, "^FO40,{0}^ADN,18,10^FDPrice: {1}  Total: {2}^FS");
            linesTemplates.Add(InventoryProdQtyItems, "^FO40,{0}^ADN,36,20^FD  TOTAL QTY: {1}^FS");
            linesTemplates.Add(InventoryProdInvValue, "^FO40,{0}^ADN,36,20^FD INV. VALUE: {1}^FS");

            #endregion

            #region Orders Created

            linesTemplates.Add(OrderCreatedReportHeader, "^CF0,50^FO40,{0}^FDSales Register Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(OrderCreatedReporWorkDay, "^FO40,{0}^ADN,18,10^FDClock In: {1}  Clock Out: {2} Worked: {3}h:{4}m^FS");
            linesTemplates.Add(OrderCreatedReporBreaks, "^FO40,{0}^ADN,18,10^FDBreaks Taken: {1}h:{2}m^FS");
            linesTemplates.Add(OrderCreatedReportTableHeader, "^FO40,{0}^ABN,18,10^FDNAME^FS" +
                "^FO350,{0}^ABN,18,10^FDST^FS" +
                "^FO400,{0}^ABN,18,10^FDQTY^FS" +
                "^FO480,{0}^ABN,18,10^FDTICKET #.^FS" +
                "^FO610,{0}^ABN,18,10^FDTOTAL^FS" +
                "^FO700,{0}^ABN,18,10^FDCS TP^FS");
            linesTemplates.Add(OrderCreatedReportTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO350,{0}^ABN,18,10^FD{2}^FS" +
                "^FO400,{0}^ABN,18,10^FD{3}^FS" +
                "^FO480,{0}^ABN,18,10^FD{4}^FS" +
                "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(OrderCreatedReportTableLine1, "^FO40,{0}^ABN,18,10^FDClock In: {1}    Clock Out: {2}     # Copies: {3}^FS");
            linesTemplates.Add(OrderCreatedReportTableTerms, "^FO40,{0}^ADN,18,10^FDTerms: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment, "^FO40,{0}^ABN,18,10^FDNS Comment: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment1, "^FO40,{0}^ABN,18,10^FDRF Comment: {1}^FS");

            linesTemplates.Add(OrderCreatedReportSubtotal, "^FO510,{0}^ABN,18,10^FDSubtotal:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTax, "^FO510,{0}^ABN,18,10^FDTax:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTotals, "^FO510,{0}^ABN,18,10^FDTotals:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportPaidCust, "^FO40,{0}^ABN,18,10^FDPaid Cust:           {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDVoided:       {2}^FS");
            linesTemplates.Add(OrderCreatedReportChargeCust, "^FO40,{0}^ABN,18,10^FDCharge Cust:         {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDDelivery:     {2}^FS");
            linesTemplates.Add(OrderCreatedReportCreditCust, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FDP&P:          {2}^FS");
            linesTemplates.Add(OrderCreatedReportExpectedCash, "^FO40,{0}^ABN,18,10^FDExpected Cash Cust:  {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FD  Refused:    {2}^FS");
            linesTemplates.Add(OrderCreatedReportFullTotal, "^FO40,{0}^ABN,18,10^FDTotal Sales:         {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDTime (Hours): {2}^FS");

            linesTemplates.Add(OrderCreatedReportCreditTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FDCredit Total: {2}^FS");     
            linesTemplates.Add(OrderCreatedReportNetTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD   Net Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportBillTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD  Bill Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportSalesTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD Sales Total: {2}^FS");
            #endregion

            #region Payments Report

            linesTemplates.Add(PaymentReportHeader, "^CF0,50^FO40,{0}^FDPayments Received Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(PaymentReportTableHeader, "^FO40,{0}^ABN,18,10^FDName^FS" +
                "^FO310,{0}^ABN,18,10^FDInvoice^FS" +
                "^FO430,{0}^ABN,18,10^FDInvoice^FS" +
                "^FO520,{0}^ABN,18,10^FDAmount^FS" +
                "^FO610,{0}^ABN,18,10^FDMethod^FS" +
                "^FO700,{0}^ABN,18,10^FDRef^FS");
            linesTemplates.Add(PaymentReportTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO310,{0}^ABN,18,10^FDNumber^FS" +
                "^FO430,{0}^ABN,18,10^FDTotal^FS" +
                "^FO520,{0}^ABN,18,10^FD^FS" +
                "^FO610,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FDNumber^FS");
            linesTemplates.Add(PaymentReportTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO310,{0}^ABN,18,10^FD{2}^FS" +
                "^FO430,{0}^ABN,18,10^FD{3}^FS" +
                "^FO520,{0}^ABN,18,10^FD{4}^FS" +
                "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(PaymentReportTotalCash, "^FO430,{0}^ABN,18,10^FDCash: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCheck, "^FO420,{0}^ABN,18,10^FDCheck: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCC, "^FO368,{0}^ABN,18,10^FDCredit Card: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalMoneyOrder, "^FO368,{0}^ABN,18,10^FDMoney Order: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTransfer, "^FO368,{0}^ABN,18,10^FDTransfer: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTotal, "^FO420,{0}^ABN,18,10^FDTotal: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentSignatureText, "^FO40,{0}^ADN,18,10^FDPayment Received By^FS");

            #endregion

            #region Settlement

            linesTemplates.Add(InventorySettlementHeader, "^CF0,50^FO40,{0}^FDSettlement Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(InventorySettlementProductHeader, "^FO40,{0}^ABN,18,10^FDProduct^FS");
            linesTemplates.Add(InventorySettlementTableHeader,
                "^FO40,{0}^ABN,18,10^FDUoM^FS" +
                "^FO100,{0}^ABN,18,10^FDBeg.I^FS" +
                "^FO150,{0}^ABN,18,10^FDLoad^FS" +
                "^FO210,{0}^ABN,18,10^FDAdj^FS" +
                "^FO270,{0}^ABN,18,10^FDTr^FS" +
                "^FO330,{0}^ABN,18,10^FDSls^FS" +
                "^FO390,{0}^ABN,18,10^FDRet^FS" +
                "^FO450,{0}^ABN,18,10^FDDump^FS" +
                "^FO510,{0}^ABN,18,10^FDResh^FS" +
                "^FO570,{0}^ABN,18,10^FDDmg^FS" +
                "^FO630,{0}^ABN,18,10^FDUnlo^FS" +
                "^FO690,{0}^ABN,18,10^FDEnd.I^FS" +
                "^FO740,{0}^ABN,18,10^FDO/S^FS");
            linesTemplates.Add(InventorySettlementTableHeader1,
                "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO100,{0}^ABN,18,10^FD^FS" +
                "^FO150,{0}^ABN,18,10^FD^FS" +
                "^FO210,{0}^ABN,18,10^FDFS" +
                "^FO270,{0}^ABN,18,10^FD^FS" +
                "^FO330,{0}^ABN,18,10^FD^FS" +
                "^FO390,{0}^ABN,18,10^FD^FS" +
                "^FO450,{0}^ABN,18,10^FD^FS" +
                "^FO510,{0}^ABN,18,10^FD^FS" +
                "^FO570,{0}^ABN,18,10^FD^FS" +
                "^FO630,{0}^ABN,18,10^FD^FS" +
                "^FO690,{0}^ABN,18,10^FD^FS" +
                "^FO740,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(InventorySettlementProductLine, "^FO40,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(InventorySettlementLotLine, "^FO40,{0}^ADN,18,10^FDLot: {1}^FS");
            linesTemplates.Add(InventorySettlementTableLine,
                "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO100,{0}^ABN,18,10^FD{2}^FS" +
                "^FO150,{0}^ABN,18,10^FD{3}^FS" +
                "^FO210,{0}^ABN,18,10^FD{4}^FS" +
                "^FO270,{0}^ABN,18,10^FD{5}^FS" +
                "^FO330,{0}^ABN,18,10^FD{6}^FS" +
                "^FO390,{0}^ABN,18,10^FD{7}^FS" +
                "^FO450,{0}^ABN,18,10^FD{8}^FS" +
                "^FO510,{0}^ABN,18,10^FD{9}^FS" +
                "^FO570,{0}^ABN,18,10^FD{10}^FS" +
                "^FO630,{0}^ABN,18,10^FD{11}^FS" +
                "^FO690,{0}^ABN,18,10^FD{12}^FS" +
                "^FO740,{0}^ABN,18,10^FD{13}^FS");
            linesTemplates.Add(InventorySettlementTableTotals,
                "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO100,{0}^ABN,18,10^FD{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" +
                "^FO210,{0}^ABN,18,10^FD{3}^FS" +
                "^FO270,{0}^ABN,18,10^FD{4}^FS" +
                "^FO330,{0}^ABN,18,10^FD{5}^FS" +
                "^FO390,{0}^ABN,18,10^FD{6}^FS" +
                "^FO450,{0}^ABN,18,10^FD{7}^FS" +
                "^FO510,{0}^ABN,18,10^FD{8}^FS" +
                "^FO570,{0}^ABN,18,10^FD{9}^FS" +
                "^FO630,{0}^ABN,18,10^FD{10}^FS" +
                "^FO690,{0}^ABN,18,10^FD{11}^FS" +
                "^FO740,{0}^ABN,18,10^FD{12}^FS");
            linesTemplates.Add(InventorySettlementTableTotals1,
                "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO100,{0}^ABN,18,10^FD{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" +
                "^FO210,{0}^ABN,18,10^FD{3}^FS" +
                "^FO270,{0}^ABN,18,10^FD{4}^FS" +
                "^FO330,{0}^ABN,18,10^FD{5}^FS" +
                "^FO390,{0}^ABN,18,10^FD{6}^FS" +
                "^FO450,{0}^ABN,18,10^FD{7}^FS" +
                "^FO510,{0}^ABN,18,10^FD{8}^FS" +
                "^FO570,{0}^ABN,18,10^FD{9}^FS" +
                "^FO630,{0}^ABN,18,10^FD{10}^FS" +
                "^FO690,{0}^ABN,18,10^FD{11}^FS" +
                "^FO740,{0}^ABN,18,10^FD{12}^FS");

            linesTemplates.Add(InventorySettlementAssetTracking, "^FO40,{0}^CF0,33^FB620,1,0,L^FDCRATES: {1}^FS");

            #endregion


            #region Summary

            linesTemplates.Add(InventorySummaryHeader, "^CF0,50^FO40,{0}^FDInventory Summary^FS");
            linesTemplates.Add(InventorySummaryTableHeader,
                "^FO40,{0}^ABN,18,10^FDProduct^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS"
                );
            linesTemplates.Add(InventorySummaryTableHeader1,
                "^FO40,{0}^ABN,18,10^FDLot^FS" +
                "^FO140,{0}^ABN,18,10^FDUoM^FS" +
                "^FO220,{0}^ABN,18,10^FDBeg. Inv^FS" +
                "^FO340,{0}^ABN,18,10^FDLoaded^FS" +
                "^FO460,{0}^ABN,18,10^FDTransfer^FS" +
                "^FO580,{0}^ABN,18,10^FDSales^FS" +
                "^FO700,{0}^ABN,18,10^FDCurr. Inv^FS"
                );

            linesTemplates.Add(InventorySummaryTableProductLine,
                "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS"
                );

            linesTemplates.Add(InventorySummaryTableLine,
                "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO140,{0}^ABN,18,10^FD{2}^FS" +
                "^FO220,{0}^ABN,18,10^FD{3}^FS" +
                "^FO340,{0}^ABN,18,10^FD{4}^FS" +
                "^FO460,{0}^ABN,18,10^FD{5}^FS" +
                "^FO580,{0}^ABN,18,10^FD{6}^FS" +
                "^FO700,{0}^ABN,18,10^FD{7}^FS"
                );
            linesTemplates.Add(InventorySummaryTableTotals,
                "^FO40,{0}^ABN,18,10^FDTotals:^FS" +
                "^FO120,{0}^ABN,18,10^FD{1}^FS" +
                "^FO120,{0}^ABN,18,10^FD{2}^FS" +
                "^FO120,{0}^ABN,18,10^FD{3}^FS" +
                "^FO220,{0}^ABN,18,10^FD{4}^FS" +
                "^FO340,{0}^ABN,18,10^FD{5}^FS" +
                "^FO460,{0}^ABN,18,10^FD{6}^FS" +
                "^FO580,{0}^ABN,18,10^FD{7}^FS" +
                "^FO700,{0}^ABN,18,10^FD{8}^FS");
            linesTemplates.Add(InventorySummaryTableTotals1,
                "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO120,{0}^ABN,18,10^FD{1}^FS" +
                "^FO120,{0}^ABN,18,10^FD{2}^FS" +
                "^FO120,{0}^ABN,18,10^FD{3}^FS" +
                "^FO220,{0}^ABN,18,10^FD{4}^FS" +
                "^FO340,{0}^ABN,18,10^FD{5}^FS" +
                "^FO460,{0}^ABN,18,10^FD{6}^FS" +
                "^FO580,{0}^ABN,18,10^FD{7}^FS" +
                "^FO700,{0}^ABN,18,10^FD{8}^FS");

            #endregion

            #region Consignment Invoice

            linesTemplates.Add(ConsignmentInvoiceHeader, "^FO40,{0}^ADN,36,20^FDInvoice: {1}^FS");
            linesTemplates.Add(ConsignmentSalesOrderHeader, "^FO40,{0}^ADN,36,20^FDSales Order^FS");
            linesTemplates.Add(ConsignmentInvoiceTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO390,{0}^ADN,18,10^FDCons^FS" +
                "^FO450,{0}^ADN,18,10^FDCount^FS" +
                "^FO515,{0}^ADN,18,10^FDSold^FS" +
                "^FO580,{0}^ADN,18,10^FDPrice^FS" +
                "^FO670,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO390,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO515,{0}^ADN,18,10^FD{4}^FS" +
                "^FO580,{0}^ADN,18,10^FD{5}^FS" +
                "^FO670,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLineLot, "^FO40,{0}^ADN,18,10^FDLot: ^FS" +
                "^FO100,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableTotal, "^FO420,{0}^ADN,18,10^FDTotal^FS" +
                "^FO515,{0}^ADN,18,10^FD{1}^FS" +
                "^FO670,{0}^ADN,18,10^FD{2}^FS");

            #endregion

            #region Consignment Contract

            linesTemplates.Add(ConsignmentContractHeader, "^FO40,{0}^ADN,36,20^FDConsignment Contract^FS");
            linesTemplates.Add(ConsignmentContractTableHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO450,{0}^ADN,18,10^FDCons^FS" +
                "^FO515,{0}^ADN,18,10^FDCons^FS" +
                "^FO580,{0}^ADN,18,10^FDPrice^FS" +
                "^FO670,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentContractTableHeader2, "^FO40,{0}^ADN,18,10^FD^FS" +
                "^FO450,{0}^ADN,18,10^FDOld^FS" +
                "^FO515,{0}^ADN,18,10^FDNew^FS" +
                "^FO580,{0}^ADN,18,10^FD^FS" +
                "^FO670,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(ConsignmentContractTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO515,{0}^ADN,18,10^FD{3}^FS" +
                "^FO580,{0}^ADN,18,10^FD{4}^FS" +
                "^FO670,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(ConsignmentContractTableTotal, "^FO350,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO450,{0}^ADN,18,10^FD{1}^FS" +
                "^FO515,{0}^ADN,18,10^FD{2}^FS" +
                "^FO670,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Route Return

            linesTemplates.Add(RouteReturnsTitle, "^FO40,{0}^ADN,36,20^FDRoute Return Report^FS");
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO40,{0}^ADN,28,14^FDNOT FINAL ROUTE RETURN^FS");
            linesTemplates.Add(RouteReturnsTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO400,{0}^ADN,18,10^FDRefuse^FS" +
                "^FO480,{0}^ADN,18,10^FDDump^FS" +
                "^FO560,{0}^ADN,18,10^FDReturn^FS" +
                "^FO640,{0}^ADN,18,10^FDDmg^FS" +
                "^FO720,{0}^ADN,18,10^FDUnload^FS");
            linesTemplates.Add(RouteReturnsTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO400,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO560,{0}^ADN,18,10^FD{4}^FS" +
                "^FO640,{0}^ADN,18,10^FD{5}^FS" +
                "^FO720,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(RouteReturnsTotals, "^FO40,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO400,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO560,{0}^ADN,18,10^FD{4}^FS" +
                "^FO640,{0}^ADN,18,10^FD{5}^FS" +
                "^FO720,{0}^ADN,18,10^FD{6}^FS");

            #endregion

            #region Payment

            linesTemplates.Add(PaymentTitle, "^FO40,{0}^ADN,36,20^FDPayment Receipt^FS^FO500,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO40,{0}^ADN,36,20^FDCustomer:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentInvoiceNumber, "^FO40,{0}^ADN,18,10^FD{1} #: {2}^FS");
            linesTemplates.Add(PaymentInvoiceTotal, "^FO40,{0}^ADN,18,10^FD{1} Total: {2}^FS");
            linesTemplates.Add(PaymentPaidInFull, "^FO40,{0}^ADN,18,10^FDPaid in Full: {1}^FS");
            linesTemplates.Add(PaymentComponents, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentTotalPaid, "^FO40,{0}^ADN,36,20^FDTotal Paid: {1}^FS");
            linesTemplates.Add(PaymentPending, "^FO40,{0}^ADN,36,20^FD   Pending: {1}^FS");

            #endregion

            #region Open Invoice

            linesTemplates.Add(InvoiceTitle, "^CF0,40^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(InvoiceCopy, "^FO40,{0}^ADN,36,20^FDCOPY^FS");
            linesTemplates.Add(InvoiceDueOn, "^FO40,{0}^ADN,18,10^FDDue on:    {1}^FS");
            linesTemplates.Add(InvoiceDueOnOverdue, "^FO40,{0}^ADN,18,10^FDDue on:    {1} OVERDUE^FS");
            linesTemplates.Add(InvoiceClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InvoiceCustomerNumber, "^FO40,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(InvoiceClientAddr, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(InvoiceClientBalance, "^FO40,{0}^ADN,18,10^FDAccount Balance: {1}^FS");
            linesTemplates.Add(InvoiceComment, "^FO40,{0}^ADN,18,10^FDC: {1}^FS");
            linesTemplates.Add(InvoiceTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO450,{0}^ADN,18,10^FDQTY^FS" +
                "^FO580,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(InvoiceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoiceTotal, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoicePaidInFull,     "^FO40,{0}^ADN,36,20^FD   PAID IN FULL^FS");
            linesTemplates.Add(InvoicePaidInFullCredit, "^FO40,{0}^ADN,36,20^FD   COLLECTED IN FULL^FS");
            linesTemplates.Add(InvoiceCredit,         "^FO40,{0}^ADN,36,20^FD   CREDIT^FS");
            linesTemplates.Add(InvoicePartialPayment, "^FO40,{0}^ADN,36,20^FDPARTIAL PAYMENT: {1}^FS");
            linesTemplates.Add(InvoiceOpen,           "^FO40,{0}^ADN,36,20^FD           OPEN: {1}^FS");
            linesTemplates.Add(InvoiceQtyItems,       "^FO40,{0}^ADN,36,20^FD      QTY ITEMS: {1}^FS");
            linesTemplates.Add(InvoiceQtyUnits,       "^FO40,{0}^ADN,36,20^FD      QTY UNITS: {1}^FS");

            #endregion

            #region Transfer

            linesTemplates.Add(TransferOnHeader, "^FO40,{0}^ADN,36,20^FDTransfer On Report^FS");
            linesTemplates.Add(TransferOffHeader, "^FO40,{0}^ADN,36,20^FDTransfer Off Report^FS");
            linesTemplates.Add(TransferNotFinal, "^FO40,{0}^ADN,28,14^FDNOT A FINAL TRANSFER^FS");
            linesTemplates.Add(TransferTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO450,{0}^ADN,18,10^FDLot^FS" +
                "^FO535,{0}^ADN,18,10^FDUoM^FS" +
                "^FO690,{0}^ADN,18,10^FDTransf.^FS");
            linesTemplates.Add(TransferTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO535,{0}^ADN,18,10^FD{3}^FS" +
                "^FO690,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(TransferTableLinePrice, "^FO40,{0}^ADN,18,10^FDList Price: {1}^FS");
            linesTemplates.Add(TransferQtyItems, "^FO40,{0}^ADN,36,20^FD      QTY ITEMS: {1}^FS");
            linesTemplates.Add(TransferAmount, "^FO40,{0}^ADN,36,20^FD TRANSFER VALUE: {1}^FS");
            linesTemplates.Add(TransferComment, "^FO40,{0}^ADN,18,10^FDComment: {1}^FS");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "^FO40,{0}^ADN,28,14^FDCustomer Open Balance^FS");
            linesTemplates.Add(ClientStatementTableHeader, "^FO40,{0}^ADN,18,10^FDType^FS" +
                "^FO150,{0}^ADN,18,10^FDDate^FS" +
                "^FO285,{0}^ADN,18,10^FDNumber^FS" +
                "^FO460,{0}^ADN,18,10^FDDue Date^FS" +
                "^FO585,{0}^ADN,18,10^FDAmount^FS" +
                "^FO690,{0}^ADN,18,10^FDOpen^FS");
            linesTemplates.Add(ClientStatementTableHeader1, "");
            linesTemplates.Add(ClientStatementTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO150,{0}^ADN,18,10^FD{2}^FS" +
                "^FO285,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS" +
                "^FO585,{0}^ADN,18,10^FD{5}^FS" +
                "^FO690,{0}^ADN,18,10^FD{6}^FS");
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

            linesTemplates.Add(AcceptedOrdersHeader, "^FO40,{0}^ADN,36,20^FDAccepted Orders Report^FS");
            linesTemplates.Add(AcceptedOrdersDate, "^FO400,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(AcceptedOrdersDeliveriesLabel, "^CF0,35^FO40,{0}^FDDeliveries^FS");
            linesTemplates.Add(AcceptedOrdersCreditsLabel, "^CF0,35^FO40,{0}^FDCredits^FS");
            linesTemplates.Add(AcceptedOrdersDeliveriesTableHeader, "^CF0,30" +
                "^FO40,{0}^FDCustomer^FS" +
                "^FO400,{0}^FDQty^FS" +
                "^FO500,{0}^FDWeight^FS" +
                "^FO640,{0}^FDAmount^FS");
            linesTemplates.Add(AcceptedOrdersTableLine, "^CFA,20" +
                "^FO40,{0}^FD{1}^FS" +
                "^FO400,{0}^FD{2}^FS" +
                "^FO500,{0}^FD{3}^FS" +
                "^FO640,{0}^FD{4}^FS");
            linesTemplates.Add(AcceptedOrdersTableLine2, "^CF0,20" +
                "^FO40,{0}^FD{1}^FS" +
                "^FO220,{0}^FD{2}^FS");
            linesTemplates.Add(AcceptedOrdersLoadsTableHeader, "^CF0,35^FO40,{0}^FDLoad Orders^FS");
            linesTemplates.Add(AcceptedOrdersTableTotals, "^CF0,25" +
                "^FO300,{0}^FDTotals:^FS" +
                "^FO400,{0}^FD{1}^FS" +
                "^FO500,{0}^FD{2}^FS" +
                "^FO640,{0}^FD{3}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsQty, "^CF0,35" +
                "^FO270,{0}^FD     Total Qty: ^FS^FO500,{0}^FD{1}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsWeight, "^CF0,35" +
                "^FO270,{0}^FDTotal Weight: ^FS^FO500,{0}^FD{1}^FS");
            linesTemplates.Add(AcceptedOrdersTotalsAmount, "^CF0,35" +
                "^FO270,{0}^FD       Amount: ^FS^FO500,{0}^FD{1}^FS");
            #endregion

            #region Refusal Report

            linesTemplates.Add(RefusalReportHeader, "^CF0,50^FO40,{0}^FDRefusal Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(RefusalReportTableHeader, "^CF0,25^^FO40,{0}^FDReason: {1}^FS" +
                "^FO600,{0}^FDOrder #^FS");
            linesTemplates.Add(RefusalReportTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO600,{0}^ABN,18,10^FD{2}^FS");
            linesTemplates.Add(RefusalReportProductTableHeader, "^CF0,25^^FO40,{0}^FDProduct^FS" +
                "^FO600,{0}^FDQty^FS");
            linesTemplates.Add(RefusalReportProductTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO600,{0}^ABN,18,10^FD{2}^FS");

            #endregion

            #region New Refusal Report

            linesTemplates.Add(NewRefusalReportTableHeader1, "^CF0,35^^FO40,{0}^FDRefused By Store^FS");
            linesTemplates.Add(NewRefusalReportTableHeader, "^CF0,25^^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(NewRefusalReportProductTableHeader, "^CF0,25^^FO40,{0}^FDProduct^FS" + "^FO420,{0}^FDQty^FS" + "^FO520,{0}^FDReason^FS");
            linesTemplates.Add(NewRefusalReportProductTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO420,{0}^ADN,18,10^FD{2}^FS" + "^FO520,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Payment Deposit
            linesTemplates.Add(ChecksTitle, "^FO40,{0}^AON,30,15^FDList Of Checks^FS");
            linesTemplates.Add(BatchDate, "^FO40,{0}^ADN,18,10^FDPosted Date: {1}^FS");
            linesTemplates.Add(BatchPrintedDate, "^FO40,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(BatchSalesman, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS");
            linesTemplates.Add(BatchBank, "^FO40,{0}^ADN,18,10^FDBank: {1}^FS");
            linesTemplates.Add(CheckTableHeader, "^FO40,{0}^ADN,18,10^FDIDENTIFICATION CHECKS^FS" +
                   "^FO450,{0}^ADN,18,10^FDAMOUNT^FS");
            linesTemplates.Add(CheckTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
               "^FO450,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(CheckTableTotal, "^FO40,{0}^ADN,18,10^FD# OF CHECKS: {1}^FS" +
            "^FO290,{0}^ADN,18,10^FDTOTAL CHECK: {2}^FS");

            linesTemplates.Add(CashTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL CASH: {1}^FS");
            linesTemplates.Add(CreditCardTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL CREDIT CARD: {1}^FS");
            linesTemplates.Add(MoneyOrderTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL MONEY ORDER: {1}^FS");

            linesTemplates.Add(BatchTotal, "^FO40,{0}^AON,30,15^FDTOTAL DEPOSIT: {1}^FS");
            linesTemplates.Add(BatchComments, "^FO40,{0}^ADN,18,10^FDComments: {1}^FS");

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "F040,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(DeliveryInvoiceNumber, "^FO40,169^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO680,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO689,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TotalQtysProofDelivery, "^FO610,{0}^ADN,18,10^FDTOTAL: {1}^FS");

            linesTemplates.Add(OrderDetailsTotalsUoMDelivery, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO345,{0}^ADN,18,10^FD{3}^FS" + "^FO489,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderDetailsHeaderUoMDelivery, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO345,{0}^ADN,18,10^FDUOM^FS" + "^FO480,{0}^ADN,18,10^FDQTY^FS");

            #endregion


            #region pick ticket


            linesTemplates.Add(PickTicketCompanyHeader, "^FO40,{0}^CF0,33^FB620,1,0,L^FD{1}^FS");
            linesTemplates.Add(PickTicketRouteInfo, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PickTicketDeliveryDate, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PickTicketDriver, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(PickTicketProductHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT #^FS" +
          "^FO200,{0}^ADN,18,10^FDDESCRIPTION^FS" +
          "^FO560,{0}^ADN,18,10^FDCASES^FS" +
          "^FO690,{0}^ADN,18,10^FDUNITS^FS");

            linesTemplates.Add(PickTicketProductLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
    "^FO200,{0}^ADN,18,10^FD{2}^FS" +
    "^FO575,{0}^ADN,18,10^FD{3}^FS" +
    "^FO700,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(PickTicketProductTotal, "^FO40,{0}^ADN,18,10^FDTOTALS^FS" +
       "^FO575,{0}^ADN,18,10^FD{1}^FS" +
       "^FO700,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(VehicleInformationHeader, "^CF0,50^FO40,{0}^FDVehicle Information Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS");
            linesTemplates.Add(VehicleInformationHeader1, "^CF0,50^FO40,{0}^FDVehicle Information^FS");


            #endregion
        }


        protected override int WidthForBoldFont
        {
            get
            {
                int i = 31;
                return i;
            }
        }

        protected override int WidthForNormalFont
        {
            get
            {
                int i = 62;
                return i;
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
            return SplitProductName(name, 25, 25); //35, 35
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

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 25, 30);
        }

        protected override IEnumerable<string> GetLoadOrderDetailsRowSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IEnumerable<string> GetAcceptLoadDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 40);
        }

        protected override IEnumerable<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 40);
        }

        protected override IEnumerable<string> GetInventoryProdDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 25, 30);
        }

        protected override IEnumerable<string> GetInventorySettlementRowsSplitProductName(string name)
        {
            return SplitProductName(name, 24, 24);
        }

        protected override IEnumerable<string> GetConsInvoiceDetailRowsSplitProductName(string name)
        {
            return SplitProductName(name, 28, 28);
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
            return SplitProductName(name, 32, 32);
        }

        protected override IEnumerable<string> GetInventoryCountDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IEnumerable<string> GetAcceptedLoadSplitClientName(string clientName)
        {
            return SplitProductName(clientName, 29, 29);
        }
    }
}