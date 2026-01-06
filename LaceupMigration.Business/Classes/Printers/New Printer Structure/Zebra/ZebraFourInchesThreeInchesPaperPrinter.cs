using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LaceupMigration
{
    public class ZebraFourInchesThreeInchesPaperPrinter : ZebraThreeInchesPrinter1
    {
        protected override void FillDictionary()
        {

            base.FillDictionary();

            linesTemplates[EndLabel] = "^XZ";
            linesTemplates[StartLabel] = "^XA^PON^MNN^LL{0}";
                          
            linesTemplates[Upc128] = "^FO15,{0}^BCN,40^FD{1}^FS"; //test
                          
            linesTemplates[UPC128ForLabel] = "^FO50,{0}^BUN,40^FD{1}^FS";
            linesTemplates[RetailPrice] = "^FO130,{0}^ADN,18,16^FD{1}^FS";
                          
            #region Standad
                          
            linesTemplates[StandarPrintTitle] = "^FO150,{0}^ADN,36,20^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS";
            linesTemplates[StandarPrintDate] = "^FO150,{0}^ADN,18,10^FDDate: {1}^FS";
            linesTemplates[StandarPrintDateBig] = "^CF0,30^FO150,{0}^FDDate: {1}^FS";
            linesTemplates[StandarPrintRouteNumber] = "^FO150,{0}^ADN,18,10^FDRoute #: {1}^FS";
            linesTemplates[StandarPrintDriverName] = "^FO150,{0}^ADN,18,10^FDDriver Name: {1}^FS";
            linesTemplates[StandarPrintCreatedBy] = "^FO150,{0}^ADN,18,10^FDSalesman: {1}^FS";
            linesTemplates[StandarPrintedDate] = "^FO150,{0}^ADN,18,10^FDPrinted Date: {1}^FS";
            linesTemplates[StandarPrintedOn] = "^FO150,{0}^ADN,18,10^FDPrinted On: {1}^FS";
            linesTemplates[StandarCreatedOn] = "^FO150,{0}^ADN,18,10^FDCreated On: {1}^FS";

            #endregion
            #region Company

            linesTemplates[CompanyName] = "^FO150,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[CompanyAddress]= "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[CompanyPhone] = "^FO150,{0}^ADN,18,10^FDPhone: {1}^FS";
            linesTemplates[CompanyFax] = "^FO150,{0}^ADN,18,10^FDFax: {1}^FS";
            linesTemplates[CompanyEmail] ="^FO150,{0}^ADN,18,10^FDEmail: {1}^FS";
            linesTemplates[CompanyLicenses1] = "^FO150,{0}^ADN,18,10^FDLicenses: {1}^FS";
            linesTemplates[CompanyLicenses2]= "^FO150,{0}^ADN,18,10^FD          {1}^FS";

            #endregion
            #region Order

            linesTemplates[OrderClientName] = "^FO150,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderClientNameTo] = "^FO150,{0}^ADN,18,10^FDCustomer: {1}^FS";
            linesTemplates[OrderClientAddress] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderBillTo] = "^FO150,{0}^ADN,18,10^FDBill To: {1}^FS";
            linesTemplates[OrderBillTo1] = "^FO150,{0}^ADN,18,10^FD         {1}^FS";
            linesTemplates[OrderShipTo] = "^FO150,{0}^ADN,18,10^FDShip To: {1}^FS";
            linesTemplates[OrderShipTo1] = "^FO150,{0}^ADN,18,10^FD         {1}^FS";
            linesTemplates[OrderClientLicenceNumber] = "^FO150,{0}^ADN,18,10^FDLicense Number: {1}^FS";
            linesTemplates[OrderVendorNumber] = "^FO150,{0}^ADN,18,10^FDVendor Number: {1}^FS";
            linesTemplates[OrderTerms] = "^FO150,{0}^ADN,18,10^FDTerms: {1}^FS";
            linesTemplates[OrderAccountBalance] = "^FO150,{0}^ADN,18,10^FDAccount Balance: {1}^FS";
            linesTemplates[OrderTypeAndNumber] = "^FO150,{0}^ADN,36,20^FD{2} #: {1}^FS";
            linesTemplates[PONumber] = "^FO150,{0}^ADN,36,20^FDPO #: {1}^FS";

            linesTemplates[OrderPaymentText] = "^FO150,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderHeaderText] = "^FO150,{0}^ADN,36,20^FD{1}^FS";

            linesTemplates[OrderDetailsHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO375,{0}^ADN,18,10^FDQTY^FS" + //275
                "^FO460,{0}^ADN,18,10^FDPRICE^FS" + //390
                "^FO570,{0}^ADN,18,10^FDTOTAL^FS"; //480
            linesTemplates[OrderDetailsLineSeparator] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderDetailsHeaderSectionName] = "^FO350,{0}^ADN,18,10^FD{1}^FS"; //test
            linesTemplates[OrderDetailsLines] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO375,{0}^ADN,18,10^FD{2}^FS" + //275
                "^FO460,{0}^ADN,18,10^FD{4}^FS" + //380
                "^FO570,{0}^ADN,18,10^FB60,1,0,R^FH\\^FD{3}^FS"; //460 FB110
            linesTemplates[OrderDetailsLines2] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderDetailsLinesLotQty] = "^FO150,{0}^ADN,18,10^FDLot: {1} -> {2}^FS";
            linesTemplates[OrderDetailsWeights] = "^CF0,25^FO150,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsWeightsCount] =  "^FO150,{0}^ADN,18,10^FDQty: {1}^FS";
            linesTemplates[OrderDetailsLinesRetailPrice] = "^FO150,{0}^ADN,18,10^FDRetail price {1}^FS";
            linesTemplates[OrderDetailsLinesUpcText] =  "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[OrderDetailsLinesUpcBarcode] = "^FO150,{0}^BUN,40^FD{1}^FS";//Bar Code Product 
            linesTemplates[OrderDetailsTotals] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO160,{0}^ADN,18,10^FD{2}^FS" +
                "^FO275,{0}^ADN,18,10^FD{3}^FS" + 
                "^FO460,{0}^ADN,18,10^FB170,1,0,R^FH\\^FD{4}^FS"; //FB110
            linesTemplates[OrderTotalContainers] =  "^FO255,{0}^ADN,36,20^FD     CONTAINERS: {1}^FS"; //test
            linesTemplates[OrderTotalsNetQty] = "^FO255,{0}^ADN,18,10^FD        NET QTY: {1}^FS";
            linesTemplates[OrderTotalsSales] = "^FO255,{0}^ADN,18,10^FD          SALES: {1}^FS";
            linesTemplates[OrderTotalsCredits] = "^FO255,{0}^ADN,18,10^FD        CREDITS: {1}^FS";
            linesTemplates[OrderTotalsReturns] = "^FO255,{0}^ADN,18,10^FD        RETURNS: {1}^FS";
            linesTemplates[OrderTotalsNetAmount] = "^FO255,{0}^ADN,18,10^FD     NET AMOUNT: {1}^FS";
            linesTemplates[OrderTotalsDiscount] = "^FO255,{0}^ADN,18,10^FD       DISCOUNT: {1}^FS";
            linesTemplates[OrderTotalsTax] =  "^FO255,{0}^ADN,18,10^FD{1} {2}^FS";
            linesTemplates[OrderTotalsTotalDue] =  "^FO255,{0}^ADN,18,10^FD      TOTAL DUE: {1}^FS";
            linesTemplates[OrderTotalsTotalPayment] = "^FO255,{0}^ADN,18,10^FD  TOTAL PAYMENT: {1}^FS";
            linesTemplates[OrderTotalsCurrentBalance] = "^FO255,{0}^ADN,18,10^FDINVOICE BALANCE: {1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance] = "^FO255,{0}^ADN,18,10^FD   OPEN BALANCE: {1}^FS";
            linesTemplates[OrderTotalsDiscountComment] = "^FO255,{0}^ADN,18,10^FD Discount Comment: {1}^FS";
            linesTemplates[OrderPreorderLabel] = "^FO255,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderComment] = "^FO255,{0}^ADN,18,10^FDComments: {1}^FS";
            linesTemplates[OrderComment2] = "^FO255,{0}^ADN,18,10^FD          {1}^FS";
            linesTemplates[PaymentComment] = "^FO255,{0}^ADN,18,10^FDPayment Comments: {1}^FS";
            linesTemplates[PaymentComment1] = "^FO255,{0}^ADN,18,10^FD                  {1}^FS";

            #endregion

            #region Footer

            linesTemplates[FooterSignatureLine] = "^FO255,{0}^ADN,18,10^FD----------------------------^FS";
            linesTemplates[FooterSignatureText] =  "^FO255,{0}^ADN,18,10^FDSignature^FS";
            linesTemplates[FooterSignatureNameText] =  "^F255,{0}^ADN,18,10^FDSignature Name: {1}^FS";
            linesTemplates[FooterSpaceSignatureText] =  "^FO255,{0}^ADN,18,10^FD ^FS";
            linesTemplates[FooterBottomText] = "^FO255,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[FooterDriverSignatureText] = "^FO255,{0}^ADN,18,10^FDDriver Signature^FS";

            #endregion

            #region Allowance

            linesTemplates[AllowanceOrderDetailsHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO380,{0}^ADN,18,10^FDQTY^FS" +
                "^FO480,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO580,{0}^ADN,18,10^FDALLOW^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS";
            linesTemplates[AllowanceOrderDetailsLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO380,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO580,{0}^ADN,18,10^FD-{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{5}^FS";

            #endregion

            #region Shortage Report

            linesTemplates[ShortageReportHeader] = "^FO150,{0}^ADN,36,20^FDKNOWN SHORTAGE REPORT^FS";
            linesTemplates[ShortageReportDate] = "^FO200,{0}^ADN,18,10^FDDate: {1}^FS";
            linesTemplates[ShortageReportInvoiceHeader] = "^FO150,{0}^ADN,36,20^FDInvoice #: {1}^FS";
            linesTemplates[ShortageReportTableHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO500,{0}^ADN,18,10^FDPO QTY^FS" +
                "^FO600,{0}^ADN,18,10^FDSHORT.^FS" +
                "^FO710,{0}^ADN,18,10^FDDEL.^FS";
            linesTemplates[ShortageReportTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO500,{0}^ADN,18,10^FD{2}^FS" +
                "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                "^FO710,{0}^ADN,18,10^FD{4}^FS";

            #endregion

            #region Load Order

            linesTemplates[LoadOrderHeader] = "^FO150,{0}^ADN,36,20^FDLoad Order Report^FS";
            linesTemplates[LoadOrderRequestedDate] = "^FO150,{0}^ADN,18,10^FDLoad Order Request Date: {1}^FS";
            linesTemplates[LoadOrderNotFinal] = "^FO150,{0}^ADN,28,14^FDNOT A FINAL LOAD ORDER^FS";
            linesTemplates[LoadOrderTableHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO370,{0}^ADN,18,10^FDUOM^FS" +
                "^FO450,{0}^ADN,18,10^FDORDERED^FS";
            linesTemplates[LoadOrderTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO370,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS";
            linesTemplates[LoadOrderTableTotal] = "^FO150,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO450,{0}^ADN,18,10^FD{1}^FS";

            #endregion

            #region Accept Load

            linesTemplates[AcceptLoadHeader] = "^F150,{0}^ADN,36,20^FDAccepted Load^FS";
            linesTemplates[AcceptLoadDate] = "^FO200,{0}^ADN,18,10^FDPrinted Date: {1}^FS";//test
            linesTemplates[AcceptLoadInvoice] = "^FO150,{0}^ADN,36,20^FDInvoice #: {1}^FS";
            linesTemplates[AcceptLoadNotFinal] = "^FO150,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS";
            linesTemplates[AcceptLoadTableHeader] = "^FO150,{0}^ABN,18,10^FDPRODUCT^FS" +
                "^FO280,{0}^ABN,18,10^FDUoM^FS" +
                "^FO340,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO410,{0}^ABN,18,10^FDADJ^FS" +
                "^FO500,{0}^ABN,18,10^FDINV^FS";
            linesTemplates[AcceptLoadTableHeader1] = "^FO150,{0}^ABN,18,10^FD^FS" +
                "^FO280,{0}^ABN,18,10^FD^FS" +
                "^FO340,{0}^ABN,18,10^FDOUT^FS" +
                "^FO410,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS";
            linesTemplates[AcceptLoadTableLine] = "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO280,{0}^ABN,18,10^FD{2}^FS" +
                "^FO340,{0}^ABN,18,10^FD{3}^FS" +
                "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                "^FO500,{0}^ABN,18,10^FD{5}^FS";

            linesTemplates[AcceptLoadLotLine] = "^FO150,{0}^ABN,18,10^FDLot: {1}^FS";

            linesTemplates[AcceptLoadTableTotals] = "^FO180,{0}^ABN,18,10^FD^FS" + //test
                "^FO280,{0}^ABN,18,10^FDUnits^FS" +
                "^FO340,{0}^ABN,18,10^FD{1}^FS" +
                "^FO410,{0}^ABN,18,10^FD{2}^FS" +
                "^FO500,{0}^ABN,18,10^FD{3}^FS";

            linesTemplates[AcceptLoadTableTotals1] = "^FO180,{0}^ABN,18,10^FDTotals:^FS" + //test
       "^FO280,{0}^ABN,18,10^FDCASE^FS" +
       "^FO340,{0}^ABN,18,10^FD{1}^FS" +
       "^FO410,{0}^ABN,18,10^FD{2}^FS" +
       "^FO500,{0}^ABN,18,10^FD{3}^FS";

            #endregion

            #region Add Inventory

            linesTemplates[AddInventoryHeader] = "^FO150,{0}^ADN,36,20^FDAccepted Load^FS";
            linesTemplates[AddInventoryDate] = "^FO200,{0}^ADN,18,10^FDPrinted Date: {1}^FS";//test
            linesTemplates[AddInventoryNotFinal] = "^FO150,{0}^ADN,28,14^FDNOT A FINAL DOCUMENT^FS";
            linesTemplates[AddInventoryTableHeader] = "^FO150,{0}^ABN,18,10^FDPRODUCT^FS" +
                "^FO280,{0}^ABN,18,10^FDBEG^FS" +
                "^FO340,{0}^ABN,18,10^FDLOAD^FS" +
                "^FO410,{0}^ABN,18,10^FDADJ^FS" +
                "^FO500,{0}^ABN,18,10^FDSTART^FS";
            linesTemplates[AddInventoryTableHeader1] = "^FO150,{0}^ABN,18,10^FD^FS" +
                "^FO280,{0}^ABN,18,10^FDINV^FS" +
                "^FO340,{0}^ABN,18,10^FDOUT^FS" +
                "^FO410,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS";
            linesTemplates[AddInventoryTableLine] = "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO280,{0}^ABN,18,10^FD{2}^FS" +
                "^FO340,{0}^ABN,18,10^FD{3}^FS" +
                "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                "^FO500,{0}^ABN,18,10^FD{5}^FS";
            linesTemplates[AddInventoryTableTotals] = "^FO180,{0}^ABN,18,10^FDTotals:^FS" + //test
                "^FO280,{0}^ABN,18,10^FD{1}^FS" +
                "^FO340,{0}^ABN,18,10^FD{2}^FS" +
                "^FO410,{0}^ABN,18,10^FD{3}^FS" +
                "^FO500,{0}^ABN,18,10^FD{4}^FS";

            #endregion

            #region Inventory

            linesTemplates[InventoryProdHeader] = "^FO150,{0}^ADN,18,10^FDInventory Report Date: {1}^FS";
            linesTemplates[InventoryProdTableHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO360,{0}^ADN,18,10^FDSTART^FS" +
                "^FO460,{0}^ADN,18,10^FDCURRENT^FS";
            linesTemplates[InventoryProdTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
               "^FO360,{0}^ADN,18,10^FD{2}^FS" +
               "^FO460,{0}^ADN,18,10^FD{3}^FS";
            linesTemplates[InventoryProdTableLineLot] = "^FO160,{0}^ADN,18,10^FDLot: {1}^FS" + //test
                "^FO360,{0}^ADN,18,10^FD{2}^FS" +
                "^FO460,{0}^ADN,18,10^FD{3}^FS";
            linesTemplates[InventoryProdTableLineListPrice] = "^FO150,{0}^ADN,18,10^FDPrice: {1}  Total: {2}^FS";
            linesTemplates[InventoryProdQtyItems] = "^FO150,{0}^ADN,18,10^FD  TOTAL QTY: {1}^FS"; //test
            linesTemplates[InventoryProdInvValue] = "^FO150,{0}^ADN,18,10^FD INV. VALUE: {1}^FS"; //test

            #endregion

            #region Orders Created

            linesTemplates[OrderCreatedReportHeader] = "^CF0,40^FO150,{0}^FDSales Register Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS"; //test
            linesTemplates[OrderCreatedReporWorkDay]   =  "^FO150,{0}^ADN,36,20^FDClock in: {1}  Clock out: {2} Worked {3}h:{4}m^FS";
            linesTemplates[OrderCreatedReporBreaks] = "^FO150,{0}^ADN,36,20^FDBreak taken: {1}h:{2}m^FS";
            linesTemplates[OrderCreatedReportTableHeader] =  "^FO150,{0}^ABN,18,10^FDNAME^FS" +
                "^FO200,{0}^ABN,18,10^FDST^FS" +
                "^FO250,{0}^ABN,18,10^FDQTY^FS" +
                "^FO300,{0}^ABN,18,10^FDTICKET #.^FS" +
                "^FO400,{0}^ABN,18,10^FDTOTAL^FS" +
                "^FO500,{0}^ABN,18,10^FDCS TP^FS";
            linesTemplates[OrderCreatedReportTableLine] = "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO200,{0}^ABN,18,10^FD{2}^FS" +
                "^FO250,{0}^ABN,18,10^FD{3}^FS" +
                "^FO300,{0}^ABN,18,10^FD{4}^FS" +
                "^FO400,{0}^ABN,18,10^FD{5}^FS" +
                "^FO500,{0}^ABN,18,10^FD{6}^FS";
            linesTemplates[OrderCreatedReportTableLine1] = "^FO150,{0}^ABN,18,10^FDClock In: {1}  Clock Out: {2}   # Copies: {3}^FS";
            linesTemplates[OrderCreatedReportTableTerms] = "^FO150,{0}^ADN,18,10^FDTerms: {1}^FS";
            linesTemplates[OrderCreatedReportTableLineComment] = "^FO150,{0}^ABN,18,10^FDNS Comment: {1}^FS";
            linesTemplates[OrderCreatedReportTableLineComment1] =  "^FO1,{0}^ABN,18,10^FDRF Comment: {1}^FS";
            linesTemplates[OrderCreatedReportSubtotal] = "^FO350,{0}^ABN,18,10^FDSubtotal:^FS^FO450,{0}^ABN,18,10^FD{1}^FS"; //test
            linesTemplates[OrderCreatedReportTax] = "^FO350,{0}^ABN,18,10^FDTax:^FS^FO450,{0}^ABN,18,10^FD{1}^FS"; //test
            linesTemplates[OrderCreatedReportTotals] = "^FO350,{0}^ABN,18,10^FDTotals:^FS^FO450,{0}^ABN,18,10^FD{1}^FS"; //test
            linesTemplates[OrderCreatedReportPaidCust] = "^FO150,{0}^ABN,18,10^FDPaid Cust:           {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDVoided:       {2}^FS";
            linesTemplates[OrderCreatedReportChargeCust] = "^FO150,{0}^ABN,18,10^FDCharge Cust:         {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDDelivery:     {2}^FS";
            linesTemplates[OrderCreatedReportCreditCust] = "^FO150,{0}^ABN,18,10^FD^FS" +
                          "^FO320,{0}^ABN,18,10^FDP&P:          {2}^FS";
            linesTemplates[OrderCreatedReportExpectedCash] = "^FO150,{0}^ABN,18,10^FDExpected Cash Cust:  {1}^FS" +
                          "^FO320,{0}^ABN,18,10^FD  Refused:    {2}^FS";
            linesTemplates[OrderCreatedReportFullTotal] = "^FO150,{0}^ABN,18,10^FDTotal Sales:         {1}^FS" +
                               "^FO320,{0}^ABN,18,10^FDTime (Hours): {2}^FS";

            linesTemplates[OrderCreatedReportCreditTotal] = "^FO150,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FDCredit Total: {2}^FS";
            linesTemplates[OrderCreatedReportNetTotal] = "^FO150,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD   Net Total: {2}^FS";
            linesTemplates[OrderCreatedReportBillTotal] = "^FO150,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD  Bill Total: {2}^FS";
            linesTemplates[OrderCreatedReportSalesTotal] = "^FO150,{0}^ABN,18,10^FD^FS" +
                               "^FO320,{0}^ABN,18,10^FD Sales Total: {2}^FS";

            #endregion

            #region Payments Report

            linesTemplates[PaymentReportHeader] = "^CF0,40^FO150,{0}^FDPayments Received Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS"; //test
            linesTemplates[PaymentReportTableHeader] = "^FO150,{0}^ABN,18,10^FDName^FS" +
                "^FO230,{0}^ABN,18,10^FDInv #^FS" +
                "^FO350,{0}^ABN,18,10^FDInv Total^FS" +
                "^FO470,{0}^ABN,18,10^FDAmount^FS";
            linesTemplates[PaymentReportTableHeader1] = "^FO150,{0}^ABN,18,10^FD^FS" +
                "^FO230,{0}^ABN,18,10^FDMethod^FS" +
                "^FO350,{0}^ABN,18,10^FDRef Number^FS" +
                "^FO470,{0}^ABN,18,10^FD^FS";
            linesTemplates[PaymentReportTableLine] = "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO230,{0}^ABN,18,10^FD{2}^FS" +
                "^FO350,{0}^ABN,18,10^FD{3}^FS" +
                "^FO470,{0}^ABN,18,10^FD{4}^FS";
            linesTemplates[PaymentReportTotalCash] = "^FO250,{0}^ABN,18,10^FDCash: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentReportTotalCheck] = "^FO240,{0}^ABN,18,10^FDCheck: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentReportTotalCC] = "^FO190,{0}^ABN,18,10^FDCredit Card: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentReportTotalMoneyOrder] = "^FO190,{0}^ABN,18,10^FDMoney Order: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentReportTotalTransfer] = "^FO190,{0}^ABN,18,10^FDTransfer: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentReportTotalTotal] = "^FO245,{0}^ABN,18,10^FDTotal: ^FS" + //test
                "^FO350,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[PaymentSignatureText] = "^FO150,{0}^ADN,18,10^FDPayment Received By^FS";

            #endregion


            #region Settlement

            linesTemplates[InventorySettlementHeader] = "^CF0,40^FO150,{0}^FDSettlement Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS";
            linesTemplates[InventorySettlementProductHeader] = "^FO150,{0}^ABN,18,10^FDProduct^FS";
            linesTemplates[InventorySettlementTableHeader] =
                "^FO150,{0}^ABN,18,10^FDUoM^FS" +    //test
                "^FO155,{0}^ABN,18,10^FDBeg.I^FS" + //test
                "^FO195,{0}^ABN,18,10^FDLoad^FS" +
                "^FO285,{0}^ABN,18,10^FDAdj^FS" +
                "^FO375,{0}^ABN,18,10^FDTr.^FS" +
                "^FO455,{0}^ABN,18,10^FDSls^FS" +
                "^FO540,{0}^ABN,18,10^FDRet^FS";
            linesTemplates[InventorySettlementTableHeader1] =
                "^FO150,{0}^ABN,18,10^FD^FS" + //test
                "^FO155,{0}^ABN,18,10^FDDump^FS" + //test
                "^FO195,{0}^ABN,18,10^FDResh^FS" +
                "^FO285,{0}^ABN,18,10^FDDmg^FS" +
                "^FO375,{0}^ABN,18,10^FDUnl^FS" +
                "^FO455,{0}^ABN,18,10^FDEnd.I^FS" +
                "^FO540,{0}^ABN,18,10^FDO/S^FS";
            linesTemplates[InventorySettlementProductLine] = "^FO150,{0}^ABN,18,10^FD{1}^FS";
            linesTemplates[InventorySettlementLotLine] = "^FO150,{0}^ADN,18,10^FDLot: {1}^FS";
            linesTemplates[InventorySettlementTableLine] =
                "^FO150,{0}^ABN,18,10^FD{1}^FS" + //test
                "^FO155,{0}^ABN,18,10^FD{2}^FS" + //test
                "^FO195,{0}^ABN,18,10^FD{3}^FS" +
                "^FO285,{0}^ABN,18,10^FD{4}^FS" +
                "^FO375,{0}^ABN,18,10^FD{5}^FS" +
                "^FO455,{0}^ABN,18,10^FD{6}^FS" +
                "^FO540,{0}^ABN,18,10^FD{7}^FS";
            linesTemplates[InventorySettlementTableTotals] =
                "^FO150,{0}^ABN,18,10^FD^FS" + //test
                "^FO155,{0}^ABN,18,10^FD{1}^FS" + //test
                "^FO195,{0}^ABN,18,10^FD{2}^FS" +
                "^FO285,{0}^ABN,18,10^FD{3}^FS" +
                "^FO375,{0}^ABN,18,10^FD{4}^FS" +
                "^FO455,{0}^ABN,18,10^FD{5}^FS" +
                "^FO540,{0}^ABN,18,10^FD{6}^FS";
            linesTemplates[InventorySettlementTableTotals1] =
                "^FO150,{0}^ABN,18,10^FD^FS" + //test
                "^FO155,{0}^ABN,18,10^FD{1}^FS" + //test
                "^FO195,{0}^ABN,18,10^FD{2}^FS" +
                "^FO285,{0}^ABN,18,10^FD{3}^FS" +
                "^FO375,{0}^ABN,18,10^FD{4}^FS" +
                "^FO455,{0}^ABN,18,10^FD{5}^FS" +
                "^FO540,{0}^ABN,18,10^FD{6}^FS";

            linesTemplates[InventorySettlementAssetTracking] = "^FO150,{0}^CF0,33^FB620,1,0,L^FDCRATES: {1}^FS";

            #endregion

            #region Summary

            linesTemplates[InventorySummaryHeader] = "^CF0,40^FO150,{0}^FDInventory Summary^FS";
            linesTemplates[InventorySummaryTableHeader] =
                "^FO150,{0}^ABN,18,10^FDProduct^FS" + //test
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS" +
                "^FO500,{0}^ABN,18,10^FD^FS";
            linesTemplates[InventorySummaryTableHeader1] =
                "^FO150,{0}^ABN,18,10^FDLot^FS" +
                "^FO150,{0}^ABN,18,10^FDUoM^FS" + //test
                "^FO150,{0}^ABN,18,10^FDBeg.I^FS" + // test
                "^FO230,{0}^ABN,18,10^FDLoad^FS" +
                "^FO320,{0}^ABN,18,10^FDTr.^FS" +
                "^FO410,{0}^ABN,18,10^FDSls^FS" +
                "^FO500,{0}^ABN,18,10^FDCurr^FS";
            linesTemplates[InventorySummaryTableProductLine] =
                "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" + //test
                "^FO150,{0}^ABN,18,10^FD{3}^FS" + //test
                "^FO230,{0}^ABN,18,10^FD{4}^FS" +
                "^FO320,{0}^ABN,18,10^FD{5}^FS" +
                "^FO410,{0}^ABN,18,10^FD{6}^FS" +
                "^FO500,{0}^ABN,18,10^FD{7}^FS";
            linesTemplates[InventorySummaryTableLine] =
                "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" + //test
                "^FO150,{0}^ABN,18,10^FD{3}^FS" + //test
                "^FO230,{0}^ABN,18,10^FD{4}^FS" +
                "^FO320,{0}^ABN,18,10^FD{5}^FS" +
                "^FO410,{0}^ABN,18,10^FD{6}^FS" +
                "^FO500,{0}^ABN,18,10^FD{7}^FS";
            linesTemplates[InventorySummaryTableTotals] = 
                "^FO150,{0}^ABN,18,10^FDTotals:{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" +
                "^FO50,{0}^ABN,18,10^FD{3}^FS" + //test
                "^FO150,{0}^ABN,18,10^FD{4}^FS" + //test
                "^FO230,{0}^ABN,18,10^FD{5}^FS" +
                "^FO320,{0}^ABN,18,10^FD{6}^FS" +
                "^FO410,{0}^ABN,18,10^FD{7}^FS" +
                "^FO500,{0}^ABN,18,10^FD{8}^FS";
            linesTemplates[InventorySummaryTableTotals1] =
                "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO150,{0}^ABN,18,10^FD{2}^FS" +
                "^FO150,{0}^ABN,18,10^FD{3}^FS" + //test
                "^FO150,{0}^ABN,18,10^FD{4}^FS" + //test
                "^FO230,{0}^ABN,18,10^FD{5}^FS" +
                "^FO320,{0}^ABN,18,10^FD{6}^FS" +
                "^FO410,{0}^ABN,18,10^FD{7}^FS" +
                "^FO500,{0}^ABN,18,10^FD{8}^FS";

            #endregion

            #region Consignment Invoice

            linesTemplates[ConsignmentInvoiceHeader] = "^FO150,{0}^ADN,36,20^FDInvoice: {1}^FS";
            linesTemplates[ConsignmentSalesOrderHeader] = "^FO150,{0}^ADN,36,20^FDSales Order^FS";
            linesTemplates[ConsignmentInvoiceTableHeader] = "^FO150,{0}^ADN,18,10^FDProduct^FS" +
                "^FO240,{0}^ADN,18,10^FDCons^FS" +
                "^FO300,{0}^ADN,18,10^FDCount^FS" +
                "^FO360,{0}^ADN,18,10^FDSold^FS" +
                "^FO420,{0}^ADN,18,10^FDPrice^FS" +
                "^FO505,{0}^ADN,18,10^FDTotal^FS";
            linesTemplates[ConsignmentInvoiceTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO240,{0}^ADN,18,10^FD{2}^FS" +
                "^FO300,{0}^ADN,18,10^FD{3}^FS" +
                "^FO360,{0}^ADN,18,10^FD{4}^FS" +
                "^FO420,{0}^ADN,18,10^FD{5}^FS" +
                "^FO505,{0}^ADN,18,10^FD{6}^FS";
            linesTemplates[ConsignmentInvoiceTableLineLot] = "^FO150,{0}^ADN,18,10^FDLot: ^FS" +
                "^FO80,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[ConsignmentInvoiceTableTotal] = "^FO260,{0}^ADN,18,10^FDTotal^FS" +
                "^FO360,{0}^ADN,18,10^FD{1}^FS" +
                "^FO505,{0}^ADN,18,10^FD{2}^FS";

            #endregion

            #region Consignment Contract

            linesTemplates[ConsignmentContractHeader] = "^FO150,{0}^ADN,36,20^FDConsignment Contract^FS";
            linesTemplates[ConsignmentContractTableHeader1] = "^FO150,{0}^ADN,18,10^FDProduct^FS" +
                "^FO250,{0}^ADN,18,10^FDCons^FS" +
                "^FO315,{0}^ADN,18,10^FDCons^FS" +
                "^FO380,{0}^ADN,18,10^FDPrice^FS" +
                "^FO470,{0}^ADN,18,10^FDTotal^FS";
            linesTemplates[ConsignmentContractTableHeader2] = "^FO150,{0}^ADN,18,10^FD^FS" +
                "^FO250,{0}^ADN,18,10^FDOld^FS" +
                "^FO315,{0}^ADN,18,10^FDNew^FS" +
                "^FO380,{0}^ADN,18,10^FD^FS" +
                "^FO470,{0}^ADN,18,10^FD^FS";
            linesTemplates[ConsignmentContractTableLine] =  "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO250,{0}^ADN,18,10^FD{2}^FS" +
                "^FO315,{0}^ADN,18,10^FD{3}^FS" +
                "^FO380,{0}^ADN,18,10^FD{4}^FS" +
                "^FO470,{0}^ADN,18,10^FD{5}^FS";

            linesTemplates[ConsignmentContractTableTotal] = "^FO350,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO250,{0}^ADN,18,10^FD{1}^FS" +
                "^FO315,{0}^ADN,18,10^FD{2}^FS" +
                "^FO470,{0}^ADN,18,10^FD{3}^FS";

            #endregion

            #region Route Return

            linesTemplates[RouteReturnsTitle] = "^FO150,{0}^ADN,36,20^FDRoute Return Report^FS";
            linesTemplates[RouteReturnsNotFinalLabel] = "^FO150,{0}^ADN,28,14^FDNOT FINAL ROUTE RETURN^FS";
            linesTemplates[RouteReturnsTableHeader] = "^FO150,{0}^ADN,18,10^FDProduct^FS" +
                "^FO300,{0}^ADN,18,10^FDRef^FS" +
                "^FO350,{0}^ADN,18,10^FDDump^FS" +
                "^FO410,{0}^ADN,18,10^FDRet^FS" +
                "^FO460,{0}^ADN,18,10^FDDmg^FS" +
                "^FO510,{0}^ADN,18,10^FDUnload^FS";
            linesTemplates[RouteReturnsTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO350,{0}^ADN,18,10^FD{3}^FS" +
                "^FO410,{0}^ADN,18,10^FD{4}^FS" +
                "^FO460,{0}^ADN,18,10^FD{5}^FS" +
                "^FO510,{0}^ADN,18,10^FD{6}^FS";
            linesTemplates[RouteReturnsTotals] = "^FO150,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO350,{0}^ADN,18,10^FD{3}^FS" +
                "^FO410,{0}^ADN,18,10^FD{4}^FS" +
                "^FO460,{0}^ADN,18,10^FD{5}^FS" +
                "^FO510,{0}^ADN,18,10^FD{6}^FS";

            #endregion

            #region Credit Report

            linesTemplates[CreditReportDetailsHeader] = "^FO150,{0}^ADN,18,10^FDNAME^FS" +
              "^FO290,{0}^ADN,18,10^FDTYPE^FS" +
              "^FO365,{0}^ADN,18,10^FDQTY^FS" +
              "^FO415,{0}^ADN,18,10^FDU.PRICE^FS" +
              "^FO510,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[CreditReportDetailsLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
            "^FO290,{0}^ADN,18,10^FD{2}^FS" +
            "^FO365,{0}^ADN,18,10^FD{3}^FS" +
            "^FO415,{0}^ADN,18,10^FD{4}^FS" +
            "^FO510,{0}^ADN,18,10^FD{5}^FS";

            linesTemplates[CreditReportDetailsTotal] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
        "^FO365,{0}^ADN,18,10^FD{2}^FS" +
        "^FO510,{0}^ADN,18,10^FD{3}^FS";

            linesTemplates[CreditReportTotalsLine] = "^FO320,{0}^ADN,18,10^FD{1}^FS" +
          "^FO485,{0}^ADN,18,10^FD{2}^FS";

            linesTemplates[CreditReportHeader] = "^CF0,50^FO150,{0}^FDCredit Report^FS^CF0,25^FO650,{3}^FDPage: {1}/{2}^FS"; //test

            linesTemplates[CreditReportClientName] =  "^FO150,{0}^AON,25^FD{1}^FS" +
       "^FO500,{0}^ADN,18,10^FD{2}^FS" +
       "^FO680,{0}^ADN,18,10^FD{3}^FS";


            #endregion

            #region Payment

            linesTemplates[PaymentTitle] = "^FO150,{0}^ADN,36,20^FDPayment Receipt^FS";
            linesTemplates[PaymentHeaderTo] = "^FO150,{0}^ADN,36,20^FDCustomer:^FS";
            linesTemplates[PaymentHeaderClientName] = "^FO150,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[PaymentHeaderClientAddr] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[PaymentInvoiceNumber] = "^FO150,{0}^ADN,18,10^FD{1} #: {2}^FS";
            linesTemplates[PaymentInvoiceTotal] = "^FO150,{0}^ADN,18,10^FD{1} Total: {2}^FS";
            linesTemplates[PaymentPaidInFull] = "^FO150,{0}^ADN,18,10^FDPaid in Full: {1}^FS";
            linesTemplates[PaymentComponents] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[PaymentTotalPaid] = "^FO150,{0}^ADN,36,20^FDTotal Paid: {1}^FS";
            linesTemplates[PaymentPending] = "^FO150,{0}^ADN,36,20^FD   Pending: {1}^FS";

            #endregion

            #region Open Invoice

            linesTemplates[InvoiceTitle] = "^CF0,40^FO150,{0}^FD{1}^FS";
            linesTemplates[InvoiceCopy] = "^FO150,{0}^ADN,36,20^FDCOPY^FS";
            linesTemplates[InvoiceDueOn] = "^FO150,{0}^ADN,18,10^FDDue on:    {1}^FS";
            linesTemplates[InvoiceDueOnOverdue] = "^FO150,{0}^ADN,18,10^FDDue on:    {1} OVERDUE^FS";
            linesTemplates[InvoiceClientName] = "^FO150,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[InvoiceCustomerNumber] = "^FO150,{0}^ADN,18,10^FDCustomer: {1}^FS";
            linesTemplates[InvoiceClientAddr] = "^FO150,{0}^ADN,18,10^FD{1}^FS";
            linesTemplates[InvoiceClientBalance] = "^FO150,{0}^ADN,18,10^FDAccount Balance: {1}^FS";
            linesTemplates[InvoiceComment] = "^FO150,{0}^ADN,18,10^FDC: {1}^FS";
            linesTemplates[InvoiceTableHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO275,{0}^ADN,18,10^FDQTY^FS" +
                "^FO390,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO480,{0}^ADN,18,10^FDTOTAL^FS";
            linesTemplates[InvoiceTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO275,{0}^ADN,18,10^FD{2}^FS" +
                "^FO390,{0}^ADN,18,10^FD{3}^FS" +
                "^FO480,{0}^ADN,18,10^FD{4}^FS";
            linesTemplates[InvoiceTotal] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO160,{0}^ADN,18,10^FD{2}^FS" +
                "^FO260,{0}^ADN,18,10^FD{3}^FS" +
                "^FO480,{0}^ADN,18,10^FD{4}^FS";
            linesTemplates[InvoicePaidInFull] = "^CF0,30^FO150,{0}^FD   PAID IN FULL^FS"; //test 120
            linesTemplates[InvoicePaidInFullCredit] = "^CF0,30^FO150,{0}^FD   COLLECTED IN FULL^FS"; //test
            linesTemplates[InvoiceCredit] =  "^CF0,30^FO150,{0}^FD   CREDIT^FS"; //test 
            linesTemplates[InvoicePartialPayment] = "^CF0,30^FO150,{0}^FDPARTIAL PAYMENT: {1}^FS";
            linesTemplates[InvoiceOpen] = "^CF0,30^FO150,{0}^FD                   OPEN: {1}^FS";
            linesTemplates[InvoiceQtyItems] = "^CF0,30^FO150,{0}^FD              QTY ITEMS: {1}^FS";
            linesTemplates[InvoiceQtyUnits] = "^CF0,30^FO150,{0}^FD              QTY UNITS: {1}^FS";

            #endregion

            #region Transfer

            linesTemplates[TransferOnHeader] = "^FO150,{0}^ADN,36,20^FDTransfer On Report^FS";
            linesTemplates[TransferOffHeader] = "^FO150,{0}^ADN,36,20^FDTransfer Off Report^FS";
            linesTemplates[TransferNotFinal] = "^FO150,{0}^ADN,28,14^FDNOT A FINAL TRANSFER^FS";
            // linesTemplates.Add(TransferTableHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO360,{0}^ADN,18,10^FDUoM^FS^FO430,{0}^ADN,18,10^FDTransferred^FS");
            linesTemplates[TransferTableHeader] = "^FO150,{0}^ADN,18,10^FDProduct^FS" +
               "^FO330,{0}^ADN,18,10^FDLot^FS" +
               "^FO400,{0}^ADN,18,10^FDUoM^FS" +
               "^FO460,{0}^ADN,18,10^FDTransf.^FS";
            linesTemplates[TransferTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO330,{0}^ADN,18,10^FD{2}^FS" +
                "^FO400,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS";

            //   linesTemplates.Add(TransferTableLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO430,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates[TransferTableLinePrice] = "^FO150,{0}^ADN,18,10^FDList Price: {1}^FS";
            linesTemplates[TransferQtyItems] = "^FO200,{0}^ADN,18,10^FD      QTY ITEMS: {1}^FS"; //test 100
            linesTemplates[TransferAmount] = "^FO200,{0}^ADN,18,10^FD TRANSFER VALUE: {1}^FS";
            linesTemplates[TransferComment] = "^FO150,{0}^ADN,18,10^FDComment: {1}^FS";

            #endregion

            #region Client Statement

            linesTemplates[ClientStatementTableTitle] = "^FO150,{0}^ADN,28,14^FDCustomer Open Balance^FS"; //test 40
            linesTemplates[ClientStatementTableHeader] = "^FO150,{0}^ADN,18,10^FDType^FS" + //test
                "^FO200,{0}^ADN,18,10^FDDate^FS" +
                "^FO370,{0}^ADN,18,10^FDDue Date^FS";
            linesTemplates[ClientStatementTableHeader1] =  "^FO150,{0}^ADN,18,10^FDNumber^FS" +
                "^FO200,{0}^ADN,18,10^FDAmount^FS" +
                "^FO370,{0}^ADN,18,10^FDOpen^FS";
            linesTemplates[ClientStatementTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO200,{0}^ADN,18,10^FD{2}^FS" +
                "^FO370,{0}^ADN,18,10^FD{3}^FS";
            linesTemplates[ClientStatementCurrent] = "^FO150,{0}^ADN,28,14^FD              Current: {1}^FS"; //test 100 whole block 
            linesTemplates[ClientStatement1_30PastDue] = "^FO150,{0}^ADN,28,14^FD   1-30 Days Past Due: {1}^FS"; 
            linesTemplates[ClientStatement31_60PastDue] = "^FO150,{0}^ADN,28,14^FD  31-60 Days Past Due: {1}^FS";
            linesTemplates[ClientStatement61_90PastDue] = "^FO150,{0}^ADN,28,14^FD  61-90 Days Past Due: {1}^FS";
            linesTemplates[ClientStatementOver90PastDue] = "^FO150,{0}^ADN,28,14^FDOver 90 Days Past Due: {1}^FS";
            linesTemplates[ClientStatementAmountDue] = "^FO150,{0}^ADN,28,14^FD           Amount Due: {1}^FS";

            #endregion

            #region Inventory Count

            linesTemplates[InventoryCountHeader] = "^FO150,{0}^ADN,36,20^FDInventory Count^FS"; //test 40 whole block
            linesTemplates[InventoryCountTableHeader] = "^FO150,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO600,{0}^ADN,18,10^FDQTY^FS" +
                "^FO680,{0}^ADN,18,10^FDUOM^FS";
            linesTemplates[InventoryCountTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS";

            #endregion

            #region Accepted Orders Report

            linesTemplates[AcceptedOrdersHeader] = "^FO150,{0}^ADN,36,20^FDAccepted Orders Report^FS";
            linesTemplates[AcceptedOrdersDate] = "^FO150,{0}^ADN,18,10^FDPrinted Date: {1}^FS";
            linesTemplates[AcceptedOrdersDeliveriesLabel] = "^CF0,35^FO150,{0}^FDDeliveries^FS";
            linesTemplates[AcceptedOrdersCreditsLabel] = "^CF0,35^FO150,{0}^FDCredits^FS";
            linesTemplates[AcceptedOrdersDeliveriesTableHeader] = "^CF0,30" +
                "^FO150,{0}^FDCustomer^FS" +
                "^FO300,{0}^FDQty^FS" +
                "^FO370,{0}^FDWeight^FS" +
                "^FO470,{0}^FDAmount^FS";
            linesTemplates[AcceptedOrdersTableLine] = "^CFA,20" +
                "^FO150,{0}^FD{1}^FS" +
                "^FO300,{0}^FD{2}^FS" +
                "^FO370,{0}^FD{3}^FS" +
                "^FO470,{0}^FD{4}^FS";
            linesTemplates[AcceptedOrdersTableLine2] = "^CF0,20" +
                "^FO150,{0}^FD{1}^FS" +
                "^FO120,{0}^FD{2}^FS";
            linesTemplates[AcceptedOrdersLoadsTableHeader] = "^CF0,35^FO150,{0}^FDLoad Orders^FS";
            linesTemplates[AcceptedOrdersTableTotals] = "^CF0,20" +
                "^FO200,{0}^FDTotals:^FS" +
                "^FO300,{0}^FD{1}^FS" +
                "^FO370,{0}^FD{2}^FS" +
                "^FO470,{0}^FD{3}^FS";
            linesTemplates[AcceptedOrdersTotalsQty] = "^CF0,35" +
                "^FO270,{0}^FD     Total Qty: ^FS^FO500,{0}^FD{1}^FS";
            linesTemplates[AcceptedOrdersTotalsWeight] = "^CF0,35" +
                "^FO270,{0}^FDTotal Weight: ^FS^FO500,{0}^FD{1}^FS";
            linesTemplates[AcceptedOrdersTotalsAmount] = "^CF0,35" +
                "^FO270,{0}^FD       Amount: ^FS^FO500,{0}^FD{1}^FS";
            #endregion

            #region Refusal Report

            linesTemplates[RefusalReportHeader] = "^CF0,40^FO150,{0}^FDRefusal Report^FS^CF0,25^FO450,{3}^FDPage: {1}/{2}^FS";
            linesTemplates[RefusalReportTableHeader] = "^CF0,25^FO150,{0}^FDReason: {1}^FS" +
                "^FO400,{0}^FDOrder #^FS";
            linesTemplates[RefusalReportTableLine] =  "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO400,{0}^ABN,18,10^FD{2}^FS";
            linesTemplates[RefusalReportProductTableHeader] = "^CF0,25^^FO150,{0}^FDProduct^FS" +
                "^FO400,{0}^FDQty^FS";
            linesTemplates[RefusalReportProductTableLine] =  "^FO150,{0}^ABN,18,10^FD{1}^FS" +
                "^FO400,{0}^ABN,18,10^FD{2}^FS";

            #endregion

            #region New Refusal Report

            linesTemplates[NewRefusalReportTableHeader1] = "^CF0,35^^FO150,{0}^FDRefused By Store^FS";
            linesTemplates[NewRefusalReportTableHeader] = "^CF0,25^^FO150,{0}^FD{1}^FS";
            linesTemplates[NewRefusalReportProductTableHeader] = "^CF0,25^^FO150,{0}^FDProduct^FS" + "^FO340,{0}^FDQty^FS" + "^FO420,{0}^FDReason^FS";
            linesTemplates[NewRefusalReportProductTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" + "^FO340,{0}^ADN,18,10^FD{2}^FS" + "^FO420,{0}^ADN,18,10^FD{3}^FS";

            #endregion

            #region Payment Deposit
            linesTemplates[ChecksTitle] = "^FO150,{0}^AON,30,15^FDList Of Checks^FS";
            linesTemplates[BatchDate] = "^FO150,{0}^ADN,18,10^FDPosted Date: {1}^FS";
            linesTemplates[BatchPrintedDate] = "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS";
            linesTemplates[BatchSalesman] = "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS";
            linesTemplates[BatchBank] = "^FO150,{0}^ADN,18,10^FDBank: {1}^FS";
            linesTemplates[CheckTableHeader] = "^FO150,{0}^ADN,18,10^FDIDENTIFICATION CHECKS^FS" + "^FO450,{0}^ADN,18,10^FDAMOUNT^FS";
            linesTemplates[CheckTableLine] = "^FO150,{0}^ADN,18,10^FD{1}^FS" + "^FO450,{0}^ADN,18,10^FD{2}^FS";
            linesTemplates[CheckTableTotal] = "^FO150,{0}^ADN,18,10^FD# OF CHECKS: {1}^FS" + "^FO290,{0}^ADN,18,10^FDTOTAL CHECK: {2}^FS";
            linesTemplates[CashTotalLine] = "^FO150,{0}^ADN,18,10^FDTOTAL CASH: {1}^FS";
            linesTemplates[CreditCardTotalLine] = "^FO150,{0}^ADN,18,10^FDTOTAL CREDIT CARD: {1}^FS";
            linesTemplates[MoneyOrderTotalLine] = "^FO150,{0}^ADN,18,10^FDTOTAL MONEY ORDER: {1}^FS";
            linesTemplates[BatchTotal] = "^FO150,{0}^AON,30,15^FDTOTAL DEPOSIT: {1}^FS";
            linesTemplates[BatchComments] = "^FO150,{0}^ADN,18,10^FDComments: {1}^FS";

            #endregion
        }

        protected override int WidthForNormalFont
        {
            get
            {
                return 44; //47
            }
        }
    }
}

