using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{

    public class ZebraThreeInchesPrinter : ZebraGenericPrinter
    {
        protected override void FillDictionary()
        {

            linesTemplates.Add(LoadOrderHeaderTitle1, "^FO15,{0}^ADN,36,20^FDLoad Order Report^FS");
            linesTemplates.Add(SalesRegisterDayReport, "^FO15,{0}^ADN,36,20^FDClock in:{1}  Clock out: {2} Worked {3}h:{4}m^FS");
            linesTemplates.Add(SalesRegisterDayReport2, "^FO15,{0}^ADN,36,20^FDBreak taken: {1}h:{2}m^FS");
            linesTemplates.Add(LoadOrderHeaderDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderPrintedDate, "^FO15,{0}^ADN,18,10^FDLoad Order Request Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderDriverNameText, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(LoadOrderNotFinalLine, "^FO15,{0}^ADN,36,20^FDNOT A FINAL Load Order^FS");
            linesTemplates.Add(LoadOrderDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO450,{0}^ADN,18,10^FDOrdered^FS");
            linesTemplates.Add(LoadOrderDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(LoadOrderDetailsFooter, "^FO15,{0}^ADN,18,10^FDTotals:^FS^FO450,{0}^ADN,18,10^FD{1}^FS");


            linesTemplates.Add(RouteReturnsHeaderTitle1, "^FO15,{0}^ADN,36,20^FDRoute Return Report^FS");
            linesTemplates.Add(RouteReturnsHeaderDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(RouteReturnsHeaderDriverNameText, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(RouteReturnsNotFinalLine, "^FO15,{0}^ADN,36,20^FDNOT A FINAL Route Return^FS");
            //nueva linea para dividir el label pues no cabe en printer de 3
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(RouteReturnsDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO300,{0}^ADN,18,10^FDDmg^FS^^FO400,{0}^ADN,18,10^FDReturns^FS^FO500,{0}^ADN,18,10^FDDump^FS");
            linesTemplates.Add(RouteReturnsDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO300,{0}^ADN,18,10^FD{4}^FS^FO400,{0}^ADN,18,10^FD{2}^FS^FO500,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(RouteReturnsDetailsFooter, "^FO15,{0}^ADN,18,10^FDTotals:^FS^FO300,{0}^ADN,18,10^FD{3}^FS^FO400,{0}^ADN,18,10^FD{1}^FS^FO500,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(TransferOnOffHeaderTitle1, "^FO15,{0}^ADN,36,20^FDTransfer {1} Report^FS");
            linesTemplates.Add(TransferOnOffHeaderDriverNameText, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(TransferOnOffDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO360,{0}^ADN,18,10^FD{1}^FS^FO430,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TransferOnOffDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO430,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(TransferOnOffNotFinalLine, "^FO15,{0}^ADN,36,20^FDNOT A FINAL TRANSFER^FS");
            linesTemplates.Add(TransferOnOffFooterSignatureLine, "^FO15,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(TransferOnOffFooterDriverSignatureText, "^FO15,{0}^ADN,18,10^FDDriver Signature^FS");
            linesTemplates.Add(TransferOnOffFooterCheckerSignatureText, "^FO15,{0}^ADN,18,10^FDSignature^FS");

            linesTemplates.Add(InventorySettlementHeaderTitle1, "^FO15,{0}^ADN,36,20^FDInventory Settlement Report^FS");
            linesTemplates.Add(InventorySettlementHeaderLabel1, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventorySettlementHeaderDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");

            linesTemplates.Add(InventorySettlementDetailsHeader1, "^FO15,{0}^ABN,18,10^FDProduct^FS" +
            "^FO250,{0}^ABN,18,10^FDBeg^FS" +
            "^FO300,{0}^ABN,18,10^FDLoad^FS" +
            "^FO350,{0}^ABN,18,10^FDTr^FS" +
            "^FO400,{0}^ABN,18,10^FDSLS^FS" +
            "^FO450,{0}^ABN,18,10^FDDump^FS" +
            "^FO500,{0}^ABN,18,10^FDRet^FS" +
            "^FO540,{0}^ABN,18,10^FDEnd^FS");

            linesTemplates.Add(InventorySettlementDetailsHeader2, "^FO15,{0}^ABN,18,10^FD^FS" +
            "^FO250,{0}^ABN,18,10^FD^FS" +
            "^FO300,{0}^ABN,18,10^FDAdj^FS" +
            "^FO350,{0}^ABN,18,10^FD^FS" +
            "^FO400,{0}^ABN,18,10^FD^FS" +
            "^FO450,{0}^ABN,18,10^FDDmg^FS" +
            "^FO500,{0}^ABN,18,10^FD^FS" +
            "^FO540,{0}^ABN,18,10^FDOS^FS");

            //linesTemplates.Add(InventorySettlementDetailsHeader3, "^FO15,{0}^ABN,18,10^FD^FS" +
            //"^FO310,{0}^ABN,18,10^FDRet^FS" +
            //"^FO380,{0}^ABN,18,10^FDEnd Inv^FS" +
            //"^FO430,{0}^ABN,18,10^FDOver Short^FS" +
            //"^FO540,{0}^ABN,18,10^FD^FS");

            linesTemplates.Add(InventorySettlementDetailRow, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
            "^FO250,{0}^ABN,18,10^FD{2}^FS" +
            "^FO300,{0}^ABN,18,10^FD{3}^FS" +
            "^FO350,{0}^ABN,18,10^FD{4}^FS" +
            "^FO400,{0}^ABN,18,10^FD{5}^FS" +
            "^FO450,{0}^ABN,18,10^FD{6}^FS" +
            "^FO500,{0}^ABN,18,10^FD{7}^FS" +
            "^FO540,{0}^ABN,18,10^FD{8}^FS");

            //  linesTemplates.Add(InventorySettlementDetailRow, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
            //"^FO310,{0}^ABN,18,10^FD{2}^FS" +
            //"^FO380,{0}^ABN,18,10^FD{3}^FS" +
            //"^FO430,{0}^ABN,18,10^FD{4}^FS" +
            //"^FO540,{0}^ABN,18,10^FD{5}^FS");


            linesTemplates.Add(InventorySummaryHeaderTitle1, "^FO15,{0}^ADN,36,20^FDInventory^FS");
            linesTemplates.Add(InventorySummaryHeaderLabel1, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventorySummaryHeaderDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");


            linesTemplates.Add(InventorySummaryDetailsHeader1,
               "^FO15,{0}^ABN,18,10^FDProduct^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS" +
               "^FO500,{0}^ABN,18,10^FD^FS"
               );
            linesTemplates.Add(InventorySummaryDetailsHeader2,
                "^FO15,{0}^ABN,18,10^FDLot^FS" +
                "^FO90,{0}^ABN,18,10^FDUoM^FS" +
                "^FO140,{0}^ABN,18,10^FDBeg.I^FS" +
                "^FO230,{0}^ABN,18,10^FDLoad^FS" +
                "^FO320,{0}^ABN,18,10^FDTr.^FS" +
                "^FO410,{0}^ABN,18,10^FDSls^FS" +
                "^FO500,{0}^ABN,18,10^FDCurr^FS");

            linesTemplates.Add(InventorySummaryProductRow,
               "^FO15,{0}^ABN,18,10^FD{1}^FS" +
               "^FO90,{0}^ABN,18,10^FD{2}^FS" +
               "^FO140,{0}^ABN,18,10^FD{3}^FS" +
               "^FO230,{0}^ABN,18,10^FD{4}^FS" +
               "^FO320,{0}^ABN,18,10^FD{5}^FS" +
               "^FO410,{0}^ABN,18,10^FD{6}^FS" +
               "^FO500,{0}^ABN,18,10^FD{7}^FS");

            linesTemplates.Add(InventorySummaryDetailRow,
            "^FO15,{0}^ABN,18,10^FD{1}^FS" +
            "^FO90,{0}^ABN,18,10^FD{2}^FS" +
            "^FO140,{0}^ABN,18,10^FD{3}^FS" +
            "^FO230,{0}^ABN,18,10^FD{4}^FS" +
            "^FO320,{0}^ABN,18,10^FD{5}^FS" +
            "^FO410,{0}^ABN,18,10^FD{6}^FS" +
            "^FO500,{0}^ABN,18,10^FD{7}^FS");

            linesTemplates.Add(InventorySummaryTotalsRow,
                "^FO15,{0}^ABN,18,10^FDTotals:{1}^FS" +
                "^FO15,{0}^ABN,18,10^FD{2}^FS" +
                "^FO90,{0}^ABN,18,10^FD{3}^FS" +
                "^FO140,{0}^ABN,18,10^FD{4}^FS" +
                "^FO230,{0}^ABN,18,10^FD{5}^FS" +
                "^FO320,{0}^ABN,18,10^FD{6}^FS" +
                "^FO410,{0}^ABN,18,10^FD{7}^FS" +
                "^FO500,{0}^ABN,18,10^FD{8}^FS");


            linesTemplates.Add(SalesRegisterHeaderTitle1, "^FO15,{0}^ADN,36,20^FDSales Register Report^FS");
            linesTemplates.Add(SalesRegisterHeaderDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(SalesRegisterHeaderDriverNameText, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(SalesRegisterDetailsHeader1, "^FO15,{0}^ABN,18,10^FDName^FS" +
                                "^FO200,{0}^ABN,18,10^FDSt^FS" +
                                "^FO250,{0}^ABN,18,10^FDQty^FS" +
                                "^FO300,{0}^ABN,18,10^FDTicket #.^FS" +
                                "^FO400,{0}^ABN,18,10^FDTotal^FS" +
                                "^FO500,{0}^ABN,18,10^FDCS Tp^FS");
            linesTemplates.Add(SalesRegisterDetailsRow1, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                                "^FO200,{0}^ABN,18,10^FD{2}^FS" +
                                "^FO250,{0}^ABN,18,10^FD{3}^FS" +
                                "^FO300,{0}^ABN,18,10^FD{4}^FS" +
                                "^FO400,{0}^ABN,18,10^FD{5}^FS" +
                                "^FO500,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(SalesRegisterDetailsRow2, "^FO15,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(SalesRegisterTotalRow, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO250,{0}^ABN,18,10^FD^FS" +
                               "^FO350,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO450,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO670,{0}^ABN,18,10^FD^FS");

            linesTemplates.Add(SalesRegisterBottomSectionRow, "^FO15,{0}^ABN,18,10^FD{1} {2}^FS" +
                               "^FO300,{0}^ABN,18,10^FD{3} {4}^FS");

            linesTemplates.Add(PaymentHeaderTitle1, "^FO15,{0}^ADN,36,20^FDPayment^FS^FO250,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO15,{0}^ADN,36,20^FDCustomer:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO15,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO15,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderTitle2, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderTitle3, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(PaymentPaid, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(CreditHeaderTitle1, "^FO15,{0}^ADN,36,20^FDCredit^FS^FO250,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(ReturnHeaderTitle1, "^FO15,{0}^ADN,36,20^FDReturn^FS^FO250,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle1, "^FO15,{0}^ADN,36,20^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderHeaderTitle2, "^FO15,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle3, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(CreditHeaderTitle2, "^FO15,{0}^ADN,36,20^FDCredit :{1}^FS");
            linesTemplates.Add(ReturnHeaderTitle2, "^FO15,{0}^ADN,36,20^FDReturn :{1}^FS");
            linesTemplates.Add(HeaderName, "^FO15,{1}^ADN,36,20^FD{0}^FS");

            // This line indicate that the order is just a "pre order"
            linesTemplates.Add(PreOrderHeaderTitle3, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PreOrderHeaderTitle4, "^FO15,{0}^ADN,36,20^FDPRE ORDER^FS");
            linesTemplates.Add(PreOrderHeaderTitle41, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            //cia name
            linesTemplates.Add(HeaderAddr1, "^FO15,{1}^ADN,18,10^FD{0}^FS");
            //addr1
            linesTemplates.Add(HeaderAddr2, "^FO15,{1}^ADN,18,10^FD{0}^FS");
            //addr2
            linesTemplates.Add(HeaderPhone, "^FO15,{1}^ADN,18,10^FD{0}^FS");
            //phone
            linesTemplates.Add(OrderHeaderTo, "^FO15,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(OrderHeaderClientName, "^FO15,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(OrderHeaderClientAddr, "^FO15,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderHeaderSectionName, "^FO350,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeader, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO275,{0}^ADN,18,10^FDQty^FS^FO390,{0}^ADN,18,10^FDPrice^FS^FO480,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(OrderDetailsHeaderSuggestedPrice, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO275,{0}^ADN,18,10^FDRetail^FS^FO370,{0}^ADN,18,10^FDQty^FS^FO420,{0}^ADN,18,10^FDPrice^FS^FO497,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(OrderDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO275,{0}^ADN,18,10^FD{2}^FS^FS^FO390,{0}^ADN,18,10^FD{4}^FS^FO480,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineSecondLine, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLineSuggestedPrice, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO275,{0}^ADN,18,10^FD{5}^FS^FO370,{0}^ADN,18,10^FD{2}^FS^FO420,{0}^ADN,18,10^FD{4}^FS^FO497,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineUPC, "^FO30,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLineUPCText, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLineLot, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal1, "^FO15,{0}^ADN,18,10^FD-------------^FS");
            linesTemplates.Add(OrderDetailsTotal14, "^FO15,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal13, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal15, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal2, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal3, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal4, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderPaid, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsSectionFooter, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO160,{0}^ADN,18,10^FD{2}^FS^FO260,{0}^ADN,18,10^FD{3}^FS^FO460,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(ExpectedTotal3, "^FO500,{0}^ADN,18,10^FDNumber of Exp:{1}^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO15,{0}^ADN,18,10^FDSignature Name: {1}^FS");
            linesTemplates.Add(FooterSignatureLine, "^FO15,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO15,{0}^ADN,18,10^FDSignature^FS");
            linesTemplates.Add(FooterSignaturePaymentText, "^FO15,{0}^ADN,18,10^FDPayment Received By^FS");
            linesTemplates.Add(FooterCheckerSignatureText, "^FO15,{0}^ADN,18,10^FDSignature {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO15,{0}^ADN,18,10^FD ^FS");

            linesTemplates.Add(FooterBottomText, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            // This line is shared
            linesTemplates.Add(TotalValueFooter, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventoryPriceLine, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            // Top header, used by ALL the inventory reports
            linesTemplates.Add(InventorySalesman, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(InventoryNotFinal, "^FO15,{0}^ADN,36,20^FDNot A Final Transfer^FS");

            // used for the print inventory report
            linesTemplates.Add(InventoryHeaderTitle, "^FO15,{0}^ADN,18,10^FDInventory Report Date: {1}^FS");
            linesTemplates.Add(InventoryDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO360,{0}^ADN,18,10^FDStart^FS^FO460,{0}^ADN,18,10^FDCurrent^FS");
            linesTemplates.Add(InventoryDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryDetailsLineLot, "^FO180,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO460,{0}^ADN,18,10^FD{3}^FS");

            // used for the check inventory report
            linesTemplates.Add(InventoryCheckHeaderTitle, "^FO15,{0}^ADN,18,10^FDCheck Inventory^FS^FO270,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(InventoryCheckDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO350,{0}^ADN,18,10^FDExpected^FS^FO460,{0}^ADN,18,10^FDReal^FS");
            linesTemplates.Add(InventoryCheckDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO360,{0}^ADN,18,10^FD{2}^FS^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryCheckDetailsFooter, "^FO15,{0}^ADN,18,10^FD^FS^FO360,{0}^ADN,18,10^FD{1}^FS^FO460,{0}^ADN,18,10^FD{2}^FS");

            // used for the Set inventory report
            linesTemplates.Add(SetInventoryHeaderTitle, "^FO15,{0}^ADN,18,10^FDSet Inv. Report^FS^FO270,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(SetInventoryDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO450,{0}^ADN,18,10^FDCurrent^FS");
            linesTemplates.Add(SetInventoryDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS");

            // used for the Add inventory report
            linesTemplates.Add(AddInventoryHeaderTitle, "^FO15,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AddInventoryHeaderTitle1, "^FO270,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(AddInventoryDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO460,{0}^ADN,18,10^FDCurrent^FS");
            ;
            linesTemplates.Add(AddInventoryDetailsHeader2, "^FO15,{0}^ADN,18,10^FDProduct^FS" +
                "^FO310,{0}^ADN,18,10^FDStart^FS" +
                "^FO385,{0}^ADN,18,10^FDAdded^FS" +
                "^FO460,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(AddInventoryDetailsHeader21, "^FO15,{0}^ADN,18,10^FD^FS" +
                "^FO310,{0}^ADN,18,10^FD^FS" +
                "^FO385,{0}^ADN,18,10^FD^FS" +
                "^FO460,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(AddInventoryDetailsLine2, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO310,{0}^ADN,18,10^FD{2}^FS" +
                "^FO385,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(AddInventoryDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO460,{0}^ADN,18,10^FD{2}^FS");

            // The sales & credit report
            linesTemplates.Add(SalesReportHeaderTitle, "^FO15,{0}^ADN,18,10^FDSales/Returns Report Date: {1}^FS");
            linesTemplates.Add(SalesReportSalesman, "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS");
            // The sales section
            linesTemplates.Add(SalesReportSalesHeaderTitle, "^FO15,{0}^ADN,36,20^FDSales section^FS");
            linesTemplates.Add(SalesReportSalesDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO480,{0}^ADN,18,10^FDSold Qty^FS");
            linesTemplates.Add(SalesReportSalesDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportSalesFooter, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            // The credit section
            linesTemplates.Add(SalesReportCreditHeaderTitle, "^FO15,{0}^ADN,36,20^FDDump section^FS");
            linesTemplates.Add(SalesReportCreditDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO480,{0}^ADN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportCreditDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportCreditFooter, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            // The Return section
            linesTemplates.Add(SalesReportReturnHeaderTitle, "^FO15,{0}^ADN,36,20^FDReturn section^FS");
            linesTemplates.Add(SalesReportReturnDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO480,{0}^ADN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportReturnDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportReturnFooter, "^FO15,{0}^ADN,36,20^FD{1}^FS");

            // The received payments report
            linesTemplates.Add(PaymentReportHeaderTitle, "^FO15,{0}^ADN,36,20^FDPayments received Report^FS");
            linesTemplates.Add(PaymentReportHeaderLabel, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentReportDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(PaymentReportSalesman, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(PaymentReportHeader1, "^FO15,{0}^ABN,18,10^FDName^FS" +
            "^FO230,{0}^ABN,18,10^FDInv #^FS" +
            "^FO350,{0}^ABN,18,10^FDInv Total^FS" +
            "^FO470,{0}^ABN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportHeader2, "^FO15,{0}^ABN,18,10^FD^FS" +
            "^FO230,{0}^ABN,18,10^FDMethod^FS" +
            "^FO350,{0}^ABN,18,10^FDRef Number^FS" +
            "^FO470,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(PaymentReportDetail, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO230,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO350,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO470,{0}^ABN,18,10^FD{4}^FS");
            linesTemplates.Add(PaymentReportTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO230,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO350,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO470,{0}^ABN,18,10^FD{3}^FS");
            linesTemplates.Add(PaymentReportTotalReceived, "^FO430,{0}^ABN,18,10^FDTotal: {1}^FS");

            // The cash received
            linesTemplates.Add(PaymentReportCashtHeaderTitle, "^FO15,{0}^ADN,18,10^FDCash section^FS");
            linesTemplates.Add(PaymentReportCashDetailsHeader1, "^FO15,{0}^ADN,18,10^FDCustomer^FS^FO660,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCashDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO660,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PaymentReportCashFooter, "^FO15,{0}^ADN,36,20^FDCash received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCashMessage, "^FO15,{0}^ADN,18,10^FDNo cash payments were received^FS");
            // The checks received
            linesTemplates.Add(PaymentReportChecktHeaderTitle, "^FO15,{0}^ADN,18,10^FDChecks section^FS");
            linesTemplates.Add(PaymentReportCheckDetailHeaderTitle, "^FO15,{0}^ADN,18,10^FDCustomer^FS^FO510,{0}^ADN,18,10^FDCheck #^FS^FO660,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCheckDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO510,{0}^ADN,18,10^FD{2}^FS^FO660,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(PaymentReportCheckFooter, "^FO15,{0}^ADN,36,20^FDQty   checks received:{1}^FS");
            linesTemplates.Add(PaymentReportCheckAmountFooter, "^FO15,{0}^ADN,36,20^FDCheck $ received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCheckMessage, "^FO15,{0}^ADN,36,20^FDNo checks payments were received^FS");
            linesTemplates.Add(PaymentReportTotalAmountFooter, "^FO15,{0}^ADN,36,20^FDTotal received:{1}^FS");

            // The received payments report
            linesTemplates.Add(OrdersCreatedReportHeaderTitle, "^FO15,{0}^ADN,18,10^FDOrders Created Report Date: {1}^FS");
            linesTemplates.Add(OrdersCreatedReportSalesman, "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS");
            // The orders in the system
            linesTemplates.Add(OrdersCreatedReportOrderSectionHeader1, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportCreditSectionHeader1, "^FO15,{0}^ADN,18,10^FDCredits^FS");
            linesTemplates.Add(OrdersCreatedReportVoidSectionHeader1, "^FO15,{0}^ADN,18,10^FDVoids^FS");
            linesTemplates.Add(OrdersCreatedReportReturnSectionHeader1, "^FO15,{0}^ADN,18,10^FDReturns^FS");
            linesTemplates.Add(OrdersCreatedReportDetailsHeader1, "^FO15,{0}^ADN,18,10^FDCustomer^FS^FO440,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO440,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine1, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter, "^FO15,{0}^ADN,18,10^FDT{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter1, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportNoOrdersMessage, "^FO15,{0}^ADN,18,10^FDNo orders in the system.^FS");
            linesTemplates.Add(OrdersCreatedReportDetailProductLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO300,{0}^ADN,18,10^FD{2}^FS^FS^FO360,{0}^ADN,18,10^FD{4}^FS^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailUPCLine, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(ConsignmentHeaderTitle1, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ConsignmentHeaderDate, "^FO180,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(ConsignmentHeaderDriverNameText, "^FO15,{0}^ADN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(ConsignmentDetailsHeaderContract1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO250,{0}^ADN,18,10^FDOld^FS^FO315,{0}^ADN,18,10^FDNew^FS^FO385,{0}^ADN,18,10^FDPrice^FS^FO485,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsHeaderContract2, "^FO15,{0}^ADN,18,10^FD^FS^FO260,{0}^ADN,18,10^FDNew^FS^FO360,{0}^ADN,18,10^FD^FS^FO460,{0}^ADN,18,10^FD^FS");

            linesTemplates.Add(ConsignmentDetailsHeader1, "^FO15,{0}^ADN,18,10^FDProduct^FS^FO315,{0}^ADN,18,10^FDCons^FS^FO385,{0}^ADN,18,10^FDCount^FS^FO485,{0}^ADN,18,10^FDSold^FS");
            linesTemplates.Add(ConsignmentDetailsHeader2, "^FO15,{0}^ADN,18,10^FD^FS^FO385,{0}^ADN,18,10^FDPrice^FS^FO485,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO315,{0}^ADN,18,10^FD{2}^FS^FO385,{0}^ADN,18,10^FD{3}^FS^FO485,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(ConsignmentDetailsLine2, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO385,{0}^ADN,18,10^FD{2}^FS^FO485,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(ConsignmentDetailsContractLine, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS^FO315,{0}^ADN,18,10^FD{3}^FS^FO385,{0}^ADN,18,10^FD{4}^FS^FO485,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(ConsignmentDetailsContractTotalLine, "^FO150,{0}^ADN,18,10^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS^FO315,{0}^ADN,18,10^FD{3}^FS^FO385,{0}^ADN,18,10^FD{4}^FS^FO485,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(ConsignmentDetailsLineUPC, "^FO30,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(ConsignmentDetailsFooter, "^FO15,{0}^ADN,18,10^FDTotals:^FS^FO350,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(ConsignmentDetailsTotalLine, "^FO360,{0}^ADN,18,10^FD{1}^FS^FO460,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(OrderHeaderTitle25, "^FO15,{0}^ADN,18,10^FDPO #: {1}^FS");

            linesTemplates.Add(UPC128, "^FO15,{0}^BCN,40^FD{1}^FS");

            //cambiar
            linesTemplates.Add(BatteryConsRotHeader, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO390,{0}^ADN,18,10^FD{2}^FS^FO445,{0}^ADN,18,10^FD{3}^FS" +
                "^FO513,{0}^ADN,18,10^FD{4}^FS^FO570,{0}^ADN,18,10^FD{5}^FS^FO670,{0}^ADN,18,10^FD{6}^FS");

            #region Full Consignment

            linesTemplates.Add(FullConsignmentCompanyInfo, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(FullConsignmentAgentInfo, "^FO15,{0}^ADN,18,10^FDAgent Info: {1}^FS");
            linesTemplates.Add(FullConsignmentConsignment, "^FO15,{0}^ADN,18,10^FDCONSIGNMENT {1}^FS");
            linesTemplates.Add(FullConsignmentMerchant, "^FO15,{0}^ADN,18,10^FDMerchant: {1}^FS");
            linesTemplates.Add(FullConsignmentMerchantId, "^FO15,{0}^ADN,18,10^FDMerchant ID: {1}^FS");
            linesTemplates.Add(FullConsignmentAddress, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FullConsignmentLastTimeVisited, "^FO15,{0}^ADN,18,10^FDLast time visited: {1}^FS");
            linesTemplates.Add(FullConsignmentSectionName, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FullConsignmentCountHeader1, "^FO20,{0}^ADN,18,10^FDDescription^FS" +
                "^FO320,{0}^ADN,18,10^FDQTY^FS" +
                "^FO380,{0}^ADN,18,10^FDPrice^FS" +
                "^FO490,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(FullConsignmentCountHeader2, "^FO30,{0}^ADN,18,10^FD^FS" +
                "^FO440,{0}^ADN,18,10^FD^FS" +
                "^FO540,{0}^ADN,18,10^FDPrice^FS" +
                "^FO680,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(FullConsignmentCountLine, "^FO20,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO380,{0}^ADN,18,10^FD{3}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(FullConsignmentCountSep, "^FO30,{0}^ADN,18,10^FD^FS" +
                "^FO320,{0}^ADN,18,10^FD____^FS" +
                "^FO380,{0}^ADN,18,10^FD_______^FS" +
                "^FO490,{0}^ADN,18,10^FD________^FS");

            linesTemplates.Add(FullConsignmentCountTotal, "^FO15,{0}^ADN,18,10^FDTotal: {1}^FS");

            linesTemplates.Add(FullConsignmentContractHeader, "^FO20,{0}^ADN,18,10^FDDescription^FS" +
                "^FO240,{0}^ADN,18,10^FDOld^FS" +
                "^FO295,{0}^ADN,18,10^FDNew^FS" +
                "^FO360,{0}^ADN,18,10^FDCount^FS" +
                "^FO440,{0}^ADN,18,10^FDSold^FS" +
                "^FO530,{0}^ADN,18,10^FDDeliv^FS");

            linesTemplates.Add(FullConsignmentContractLine, "^FO20,{0}^ADN,18,10^FD{1}^FS" +
                "^FO240,{0}^ADN,18,10^FD{2}^FS" +
                "^FO295,{0}^ADN,18,10^FD{3}^FS" +
                "^FO360,{0}^ADN,18,10^FD{4}^FS" +
                "^FO440,{0}^ADN,18,10^FD{5}^FS" +
                "^FO530,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(FullConsignmentContractSep, "^FO20,{0}^ADN,18,10^FD^FS" +
                "^FO240,{0}^ADN,18,10^FD___^FS" +
                "^FO295,{0}^ADN,18,10^FD____^FS" +
                "^FO360,{0}^ADN,18,10^FD_____^FS" +
                "^FO440,{0}^ADN,18,10^FD_____^FS" +
                "^FO530,{0}^ADN,18,10^FD_____^FS");

            linesTemplates.Add(FullConsignmentReturnsHeader, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO530,{0}^ADN,18,10^FDPicked^FS");

            linesTemplates.Add(FullConsignmentReturnsLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO530,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(FullConsignmentReturnsSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO530,{0}^ADN,18,10^FD______^FS");


            linesTemplates.Add(FullConsignmentText, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentTotals, "^FO15,{0}^ADN,18,10^FD{1}: {2}^FS");

            linesTemplates.Add(FullConsignmentPaymentHeader, "^FO30,{0}^ADN,18,10^FDType^FS" +
                "^FO300,{0}^ADN,18,10^FDAmount^FS" +
                "^FO440,{0}^ADN,18,10^FDDescription^FS");

            linesTemplates.Add(FullConsignmentPaymentLine, "^FO30,{0}^ADN,18,10^FD-{1}^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO440,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(FullConsignmentPreviousBalance, "^FO15,{0}^ADN,18,10^FDPrevious Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentAfterDisc, "^FO15,{0}^ADN,18,10^FDToday Sales After Disc:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPaymentSep, "^FO240,{0}^ADN,18,10^FD___________________^FS");

            linesTemplates.Add(FullConsignmentTotalDue, "^FO200,{0}^ADN,18,10^FDTotal Due:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPaymentTotal, "^FO15,{0}^ADN,18,10^FDPayments Total:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentNewBalance, "^FO160,{0}^ADN,18,10^FDNew Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPrintedOn, "^FO15,{0}^ADN,18,10^FDReport printed on: {1}^FS");

            linesTemplates.Add(FullConsignmentSignature, "^FO200,{0}^ADN,18,10^FDSignature: ------------------------------------^FS");

            linesTemplates.Add(FullConsignmentFinalized, "^FO150,{0}^ADN,36,20^FD{1}^FS");

            #endregion


            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "^FO40,{0}^ADN,28,14^FDCustomer Open Balance^FS");
            linesTemplates.Add(ClientStatementTableHeader, "^FO40,{0}^ADN,18,10^FDType^FS" +
                "^FO190,{0}^ADN,18,10^FDDate^FS" +
                "^FO320,{0}^ADN,18,10^FDNumber^FS" +
                "^FO410,{0}^ADN,18,10^FDDue Date^FS" +
                "^FO540,{0}^ADN,18,10^FDAmount^FS" +
                "^FO670,{0}^ADN,18,10^FDOpen^FS");
            linesTemplates.Add(ClientStatementTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO190,{0}^ADN,18,10^FD{2}^FS" +
                "^FO320,{0}^ADN,18,10^FD{3}^FS" +
                "^FO410,{0}^ADN,18,10^FD{4}^FS" +
                "^FO540,{0}^ADN,18,10^FD{5}^FS" +
                "^FO670,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ClientStatementTableTotal, "^FO100,{0}^ADN,28,14^FD{1} {2}^FS");


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

            linesTemplates.Add(InvoiceTitleNumber, "^CF0,40^FO15,{0}^FD{1}^FS");

            #region Credit Report

            linesTemplates.Add(StandarPrintedDate, "^FO15,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(StandarPrintRouteNumber, "^FO15,{0}^ADN,18,10^FDRoute #: {1}^FS");
            linesTemplates.Add(StandarPrintDriverName, "^FO15,{0}^ADN,18,10^FDDriver Name: {1}^FS");

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


            #region Standard

            linesTemplates.Add(StandarPrintTitle, "^FO15,{0}^ADN,36,20^FD{1}^FS^FO250,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(StandarPrintDate, "^FO15,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintDateBig, "^CF0,30^FO15,{0}^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintCreatedBy, "^FO15,{0}^ADN,18,10^FDSalesman: {1}^FS");
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

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "F015,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(DeliveryInvoiceNumber, "^FO15,169^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO480,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO489,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TotalQtysProofDelivery, "^FO410,{0}^ADN,18,10^FDTOTAL: {1}^FS");
            linesTemplates.Add(StandarPrintTitleProofDelivery, "F015,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeadersUoMDelivery, "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO340,{0}^ADN,18,10^FUOM^FS" + "^FO480,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsUoMDelivery, "^FO15,{0}^ADN,18,10^FD{1}^FS" + "^FO340,{0}^ADN,18,10^FD{3}^FS" + "^FO480,{0}^ADN,18,10^FD{2}^FS");


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



            linesTemplates.Add(OrderDetailsHeaderSectionName, "^FO350,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLines2, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLotQty, "^FO15,{0}^ADN,18,10^FDLot: {1} -> {2}^FS");
            linesTemplates.Add(OrderDetailsWeights, "^CF0,25^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(OrderDetailsWeightsCount, "^FO15,{0}^ADN,18,10^FDQty: {1}^FS");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "^FO15,{0}^ADN,18,10^FDRetail price {1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcText, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "^FO30,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "^BY3,3,40^FO50,{0}^BEN,40,Y,N^FD{1}^FS");

            linesTemplates.Add(OrderDetailsTotals, "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO160,{0}^ADN,18,10^FD{2}^FS" +
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
            linesTemplates.Add(OrderTotalsDiscountComment, "^FO15,{0}^ADN,18,10^FD Discount Comment: {1}^FS");
            linesTemplates.Add(OrderPreorderLabel, "^FO15,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderComment, "^FO15,{0}^ADN,18,10^FDComments: {1}^FS");
            linesTemplates.Add(OrderComment2, "^FO15,{0}^ADN,18,10^FD          {1}^FS");
            linesTemplates.Add(PaymentComment, "^FO15,{0}^ADN,18,10^FDPayment Comments: {1}^FS");
            linesTemplates.Add(PaymentComment1, "^FO15,{0}^ADN,18,10^FD                  {1}^FS");
            linesTemplates.Add(OrderCommentWork, "^FO15,{0}^AON,24,15^FD{1}^FS");

            #endregion

            linesTemplates.Add(EndLabel, "^XZ");
            linesTemplates.Add(StartLabel, "^XA^PON^MNN^LL{0}");

            #region pick ticket

            linesTemplates.Add(PickTicketProductHeader, "^FO15,{0}^ABN,18,10^FDPRODUCT #^FS" +
          "^FO310,{0}^ABN,18,10^FDDESCRIPTION^FS" +
          "^FO430,{0}^ABN,18,10^FDCASES^FS" +
          "^FO520,{0}^ABN,18,10^FDUNITS^FS");

            linesTemplates.Add(PickTicketProductLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
    "^FO310,{0}^ABN,18,10^FD{2}^FS" +
    "^FO430,{0}^ABN,18,10^FD{3}^FS" +
    "^FO520,{0}^ABN,18,10^FD{4}^FS");

            linesTemplates.Add(PickTicketProductTotal, "^FO15,{0}^ABN,18,10^FDTOTALS^FS" +
       "^FO430,{0}^ABN,18,10^FD{1}^FS" +
       "^FO520,{0}^ABN,18,10^FD{2}^FS");

            linesTemplates.Add(PickTicketCompanyHeader, "^FO15,{0}^CF0,33^FB520,1,0,L^FD{1}^FS");
            linesTemplates.Add(PickTicketRouteInfo, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PickTicketDeliveryDate, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PickTicketDriver, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");

            #endregion


        }

        protected override IList<string> GetDetailsRowsSplitProductNameConsignment(string name, bool counting)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IList<string> PrintSalesCreditReportSplitProductName1(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> PrintSalesCreditReportSplitProductName2(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IList<string> PrintReceivedPaymentsReportSplitProductName1(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IList<string> PrintReceivedPaymentsReportSplitProductName2(string name)
        {
            return SplitProductName(name, 25, 30);
        }

        protected override IList<string> GetTransferOnOffSplitProductName(string name)
        {
            return SplitProductName(name, 25, 40);
        }

        protected override IList<string> PrintOrdersCreatedReportSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName1(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName2(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName3(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IList<string> GetDetailsRowsSplitProductName2(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IList<string> GetBottomTextSplitText(string text = "")
        {
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 40, 40);
        }

        protected override IList<string> GetBottomDiscountTextSplitText()
        {
            return SplitProductName(Config.Discount100PercentPrintText, 40, 40);
        }

        protected override IList<string> GetInventoryCheckDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IList<string> GetInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 26);
        }

        protected override IList<string> GetSetInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IList<string> GetSalesRegRepClientSplitProductName(string name)
        {
            return SplitProductName(name, 20, 30);
        }

        protected override IList<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
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

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 21, 21);
        }

        protected override IList<string> OrderCommentsSplit(string name)
        {
            return SplitProductName(name, 29, 29);
        }

        protected override IList<string> GetClientNameSplit(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IList<string> GetLabelNotAFinalRouteReturn(string name)
        {
            return SplitProductName(name, 17, 24);
        }

        protected override IList<string> GetInventorySetDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        protected override IList<string> GetOrderPaymentSplitComment(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        public override bool PrintReceivedPaymentsReport(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeaderLabel], startY, "Payments Received"));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeaderLabel], startY, "Report"));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportSalesman], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportSalesman], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader1], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader2], startY));
            startY += font18Separation;

            double total = 0;
            double totalCash = 0;
            List<PaymentRow> rows = new List<PaymentRow>();
            foreach (var pay in InvoicePayment.List)
            {
                int _index = 0;
                List<string> docNumbers = pay.Invoices().Select(x => x.InvoiceNumber).ToList();
                if (docNumbers.Count == 0)
                    docNumbers = pay.Orders().Select(x => x.PrintedOrderId).ToList();

                var t = pay.Invoices().Sum(x => x.Balance);
                if (t == 0)
                    t = pay.Orders().Sum(x => x.OrderTotalCost());
                while (true)
                {
                    var row = new PaymentRow();
                    if (_index == 0)
                    {
                        row.ClientName = pay.Client.ClientName;
                        row.DocAmount = t.ToCustomString();
                    }
                    else
                    {
                        row.ClientName = string.Empty;
                        row.DocAmount = string.Empty;
                    }
                    if (docNumbers.Count > _index)
                        row.DocNumber = docNumbers[_index];
                    else
                        row.DocNumber = string.Empty;
                    if (pay.Components.Count > _index)
                    {

                        if (pay.Components[_index].PaymentMethod == InvoicePaymentMethod.Cash)
                            totalCash += pay.Components[_index].Amount;
                        else
                            total += pay.Components[_index].Amount;

                        row.RefNumber = pay.Components[_index].Ref;
                        var s = pay.Components[_index].Amount.ToCustomString();
                        if (s.Length < 9)
                            s = new string(' ', 9 - s.Length) + s;
                        row.Paid = s;
                        row.PaymentMethod = ReducePaymentMethod(pay.Components[_index].PaymentMethod);
                    }
                    else
                    {
                        row.RefNumber = string.Empty;
                        row.Paid = string.Empty;
                        row.PaymentMethod = string.Empty;
                    }
                    rows.Add(row);

                    _index++;
                    if (docNumbers.Count <= _index && pay.Components.Count <= _index)
                        break;
                }
            }

            foreach (var p in rows)
            {
                var parts = GetClientNameSplit(p.ClientName).ToList();
                string s = string.Empty;
                if (parts.Count > 0)
                    s = parts[0];
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDetail], startY,
                                s,
                                p.DocNumber,
                                p.DocAmount,
                                p.Paid));
                startY += font18Separation;

                s = parts.Count > 1 ? parts[1] : "";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDetail], startY,
                                s,
                                p.PaymentMethod,
                                p.RefNumber,
                                ""));
                startY += font18Separation;

                for (int i = 2; i < parts.Count; i++)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportDetail], startY,
                            parts[i],
                            "",
                            "",
                            ""));
                    startY += font18Separation;
                }
            }
            startY += font18Separation;
            if (totalCash > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Cash: ", totalCash.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            if (total > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotal], startY, "Other: ", total.ToCustomString(), string.Empty));
                startY += font18Separation;
            }
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignaturePaymentText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s1 in lines)
                sb.Append(s1);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        public override bool InventorySettlement(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderLabel1], startY, "Inventory Settlement"));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderLabel1], startY, "Report"));
            startY += font36Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataProvider.ExtendedSendTheLeftOverInventory();

            foreach (var value in map)
            {
                var product = value.Product;

                totalRow.Product = product;
                totalRow.BegInv += product.BeginigInventory;
                totalRow.LoadOut += product.RequestedLoadInventory;
                totalRow.Adj += product.LoadedInventory - product.RequestedLoadInventory;
                totalRow.TransferOn += product.TransferredOnInventory;
                totalRow.TransferOff += product.TransferredOffInventory;
                totalRow.EndInventory += product.CurrentInventory;
                totalRow.Unload += product.UnloadedInventory;
                totalRow.DamagedInTruck += product.DamagedInTruckInventory;

                totalRow.Sales += value.Sales;
                totalRow.CreditReturns += value.CreditReturns;
                totalRow.CreditDump += value.CreditDump;
            }

            startY += font18Separation;
            var oldRound = Config.Round;
            Config.Round = 2;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (p.Adj == 0 && p.BegInv == 0 && p.Dump == 0 && p.EndInventory == 0 && p.LoadOut == 0 && p.Unload == 0 && p.Sales == 0 && p.TransferOn == 0 && p.TransferOff == 0)
                    continue;
                if (Config.ShortInventorySettlement && string.IsNullOrEmpty(p.OverShort) && p.TransferOn == 0 && p.TransferOff == 0 && p.Adj == 0)
                    continue;

                var parts = GetInventorySetDetailsRowsSplitProductName(p.Product.Name).ToList();

                string s = parts.Count > 0 ? parts[0] : "";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, parts[0],
                    Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.Dump, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                    Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)));
                startY += font18Separation;

                s = parts.Count > 1 ? parts[1] : "";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, s,
                    "",
                    Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                    "",
                    "",
                    Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                    "",
                    p.OverShort));
                startY += font18Separation;

                for (int i = 2; i < parts.Count; i++)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, parts[i], "", "", "", "", "", "", ""));
                    startY += font18Separation;
                }
            }
            //}
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, "Totals:",
                Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.Dump, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)));

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, "",
                "",
                Math.Round(totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                "",
                "",
                Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                "",
                totalRow.OverShort));

            Config.Round = oldRound;
            startY += font18Separation;

            //space
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s1 in lines)
                sb.Append(s1);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                Logger.CreateLog("Starting printing inventory settlement");
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        public override bool InventorySummary(int index, int count, bool isBase = true)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderLabel1], startY, "Inventory"));
            startY += font36Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataProvider.ExtendedSendTheLeftOverInventory(false,true);

            foreach (var value in map)
            {
                var product = value.Product;

                float factor = 1;
                if(!isBase)
                {
                    var defaultUom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                        factor = defaultUom.Conversion;
                }

                totalRow.Product = product;
                totalRow.BegInv += product.BeginigInventory / factor;
                totalRow.LoadOut += product.RequestedLoadInventory / factor;
                totalRow.Adj += (product.LoadedInventory - product.RequestedLoadInventory) / factor;
                totalRow.TransferOn += product.TransferredOnInventory / factor;
                totalRow.TransferOff += product.TransferredOffInventory / factor;
                totalRow.EndInventory += product.CurrentInventory / factor;
                totalRow.Unload += product.UnloadedInventory / factor;
                totalRow.DamagedInTruck += (product.DamagedInTruckInventory / factor);

                totalRow.Sales += (value.Sales / factor);
                totalRow.CreditReturns += (value.CreditReturns / factor);
                totalRow.CreditDump += (value.CreditDump / factor);
            }

            startY += font18Separation;
            var oldRound = Config.Round;
            Config.Round = 2;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (Math.Round( p.EndInventory, Config.Round) == 0)
                    continue;


                float factor = 1;
                if (!isBase)
                {
                    var defaultUom = p.Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUom != null)
                    {
                        p.UoM = defaultUom;
                        factor = defaultUom.Conversion;
                    }
                }

                var parts = GetInventorySetDetailsRowsSplitProductName(p.Product.Name).ToList();

                string s = parts.Count > 0 ? parts[0] : "";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, p.Product.Name,
                                          string.Empty,
                                          string.Empty,
                                          string.Empty,
                                          string.Empty,
                                          string.Empty,
                                          string.Empty,
                                          string.Empty));

                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, p.Lot,
                                                string.Empty,
                                                p.UoM != null ? p.UoM.OriginalId : string.Empty,
                                                Math.Round(p.BegInv / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((p.LoadOut + p.Adj) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((p.TransferOn - p.TransferOff) / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales / factor, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory / factor, Config.Round).ToString(CultureInfo.CurrentCulture)));
                startY += font18Separation;
                startY += 12;
            }
            //}
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, "Totals:",
                 string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.LoadOut + totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                ));

            startY += font18Separation;

            //space
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            sb = new StringBuilder();
            foreach (string s1 in lines)
                sb.Append(s1);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                Logger.CreateLog("Starting printing inventory Summary");
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        #region Consignment

        IList<string> GetConsignmentHeaderRows(ref int startIndex, bool counting)
        {
            var list = new List<string>();

            if (counting)
            {
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeader1], startIndex));
                startIndex += font18Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeader2], startIndex));
            }
            else
            {
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeaderContract1], startIndex));
                startIndex += font18Separation;
            }

            return list;
        }

        protected override IList<string> GetConsignmentDetailsRows(ref int startIndex, ref float totalQty, Order order, bool counting)
        {
            var lines = new List<string>();

            lines.AddRange(GetConsignmentHeaderRows(ref startIndex, counting));

            startIndex += font18Separation;

            double sold = 0;
            double added = 0;
            double newConsignment = 0;

            foreach (var detail in order.Details)
            {
                if (counting && !detail.ConsignmentCounted)
                    continue;

                if (!counting && detail.ConsignmentNew == 0)
                    continue;

                if (Config.UseFullConsignment && detail.ConsignmentNew == 0 && detail.ConsignmentSalesItem)
                    continue;

                int index = 0;

                totalQty += detail.Qty;
                sold += detail.Qty;
                added += detail.ConsignmentPick;
                newConsignment += detail.ConsignmentNew;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(detail.Product.Name, counting);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        if (counting)
                        {
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                                   detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture),
                                                    detail.ConsignmentCount.ToString(CultureInfo.CurrentCulture),
                                                   detail.Qty.ToString(CultureInfo.CurrentCulture)));

                            if (productSlices.Count() == 1)
                            {
                                startIndex += font18Separation;
                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine2], startIndex, string.Empty,
                                                        detail.Price.ToCustomString(),
                                                         (detail.Price * detail.Qty).ToCustomString()));
                            }
                        }

                        else
                        {
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                       detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture),
                                                       detail.ConsignmentNew.ToString(CultureInfo.CurrentCulture),
                                                       detail.ConsignmentNewPrice.ToCustomString(),
                                                       (detail.ConsignmentNewPrice * detail.ConsignmentNew).ToCustomString()));
                        }
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        if (index == 1)
                        {
                            if (counting)
                            {
                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine2], startIndex, productNamePart,
                                                    detail.Price.ToCustomString(),
                                                     (detail.Price * detail.Qty).ToCustomString()));
                            }
                            else
                            {
                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                       string.Empty,
                                                       string.Empty,
                                                       string.Empty,
                                                       string.Empty));
                            }
                        }
                        else
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                       string.Empty,
                                                       string.Empty,
                                                       string.Empty,
                                                       string.Empty));

                        startIndex += font18Separation;
                    }
                    else
                        break;
                    index++;
                }
                if (!string.IsNullOrEmpty(detail.Lot))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, "Lot: " + detail.Lot,
                        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                    startIndex += font18Separation;
                }

                if (detail.Product.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    string upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[ConsignmentDetailsLineUPC];

                    lines.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(detail.Product.Upc)));
                    startIndex += font36Separation + font18Separation;
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            if (!counting)
                lines.Add(GetTotalsContract(ref startIndex, order));

            return lines;
        }

        string GetTotalsContract(ref int startIndex, Order order)
        {
            var oldTotal = order.Details.Sum(x => x.ConsignmentOld);

            float newTotal = order.Details.Sum(x => x.ConsignmentNew);

            double totalPrice = order.Details.Sum(x => (x.ConsignmentUpdated ? x.ConsignmentNew : x.ConsignmentOld) * x.ConsignmentNewPrice);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractTotalLine], startIndex, "Totals:",
                oldTotal, newTotal.ToString(CultureInfo.CurrentCulture), string.Empty, totalPrice.ToCustomString());
        }

        #endregion

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 15, 18);
        }

        #region Full Consignment

        public override bool PrintFullConsignment(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();
            int startIndex = 80;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "                    Date: " + order.Date.ToString()));
            startIndex += 70;

            if (asPreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentFinalized], startIndex, "NOT AN INVOICE"));
                startIndex += 40;
            }

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO5," + startIndex.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startIndex += Config.CompanyLogoHeight;
            }

            startIndex += 36;

            lines.AddRange(GetFullConsCompanyRows(ref startIndex, order));

            startIndex += font20Separation * 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAgentInfo], startIndex, Config.VendorName));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentConsignment], startIndex, order.PrintedOrderId));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentMerchant], startIndex, order.Client.ClientName));
            startIndex += font20Separation;

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAddress], startIndex, s1.Trim()));
                startIndex += font20Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "Phone: " + order.Client.ContactPhone));
            startIndex += font20Separation;

            DateTime last = order.Client.LastVisitedDate;
            if (last == DateTime.MinValue)
                last = DateTime.Now;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentLastTimeVisited], startIndex, last.ToString()));
            startIndex += 60;

            lines.AddRange(GetFullConsCountLines(ref startIndex, order));

            lines.AddRange(GetFullConsContractLines(ref startIndex, order));

            startIndex += 70;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "I accept the new consignment balance of"));
            startIndex += font20Separation;

            float totalNew = 0;
            double totalNewCost = 0;

            float totalPicked = 0;
            double totalPickedCost = 0;

            foreach (var item in order.Details)
            {
                var newCons = item.ConsignmentUpdated ? item.ConsignmentNew : item.ConsignmentOld;

                totalNew += newCons;
                totalNewCost += (newCons * item.ConsignmentNewPrice);

                totalPicked += item.ConsignmentPicked;
                totalPickedCost += (item.ConsignmentPicked * item.Price);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Consignment Qty", totalNew.ToString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Consignment Amount", totalNewCost.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Delivered Qty", totalPicked.ToString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotals], startIndex, "Delivered Amount", totalPickedCost.ToCustomString()));

            if (!asPreOrder)
            {
                startIndex += 60;
                lines.AddRange(GetFullConsPaymentLines(ref startIndex, order));

            }

            startIndex += 60;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "*** NOTE: THIS IS A STATEMENT COPY ***"));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPrintedOn], startIndex, DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            startIndex += 60;

            // add the signature
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startIndex));

                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));

                startIndex += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startIndex, order.SignatureName ?? string.Empty));
                    startIndex += font20Separation;
                }
                startIndex += font20Separation;
            }
            else
            {
                startIndex += 140;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
                startIndex += font20Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
                startIndex += font20Separation;
            }

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startIndex + 60));
            var sb = new StringBuilder();
            foreach (string l in lines)
                sb.Append(l);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        protected override IEnumerable<string> GetFullConsCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (CompanyInfo.SelectedCompany == null)
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];

                company = CompanyInfo.SelectedCompany;

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyName, ""));
                startIndex += font20Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyAddress1, ""));
                startIndex += font20Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCompanyInfo], startIndex, company.CompanyAddress2, ""));
                    startIndex += font20Separation;
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentText], startIndex, "Phone: " + company.CompanyPhone));
                startIndex += font20Separation;

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected override List<string> GetFullConsCountLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "PRODUCTS SOLD"));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountHeader1], startIndex));
            startIndex += font20Separation;

            double totalQty = 0;
            double totalDue = 0;

            foreach (var item in SortDetails.SortedDetails(order.Details))
            {
                totalQty += item.Qty;
                totalDue += (item.Qty * item.Price);

                var productSlices = SplitProductName(item.Product.Name, 24, 24);

                int offset = 0;
                foreach (var p in productSlices)
                {
                    if (offset == 0)
                    {
                        var totalLine = item.Qty * item.Price;


                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, p,
                            item.Qty, item.Price.ToCustomString(), totalLine.ToCustomString()));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, p,
                            "", "", ""));

                    startIndex += font20Separation;
                    offset++;
                }

                startIndex += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentCountLine], startIndex, "                 Totals: ", totalQty, "",
                totalDue.ToCustomString()));
            startIndex += 40;

            startIndex += 30;

            return lines;
        }

        protected override List<string> GetFullConsContractLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "PRODUCTS BALANCE"));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractHeader], startIndex));
            startIndex += font20Separation;

            float totalOld = 0;
            float totalSold = 0;
            float totalCounted = 0;
            float totalPicked = 0;
            float totalnew = 0;

            foreach (var item in SortDetails.SortedDetails(order.Details))
            {
                var productSlices = SplitProductName(item.Product.Name, 17, 17);

                int offset = 0;
                foreach (var p in productSlices)
                {
                    if (offset == 0)
                    {
                        var consNew = item.ConsignmentUpdated ? item.ConsignmentNew : item.ConsignmentOld;

                        totalOld += item.ConsignmentOld;
                        totalSold += item.Qty;
                        totalCounted += item.ConsignmentCount;
                        totalPicked += item.ConsignmentPicked;
                        totalnew += consNew;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, p,
                            item.ConsignmentOld, consNew, item.ConsignmentCount, item.Qty, item.ConsignmentPicked));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, p,
                            "", "", "", "", ""));

                    startIndex += font20Separation;
                    offset++;
                }

                startIndex += 10;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractSep], startIndex));
            startIndex += 30;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentContractLine], startIndex, "", totalOld, totalSold,
                totalCounted, totalPicked, totalnew));
            startIndex += 40;

            return lines;
        }

        protected override List<string> GetFullConsPaymentLines(ref int startIndex, Order order)
        {
            List<string> lines = new List<string>();

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentSectionName], startIndex, "PAYMENTS"));
                startIndex += font20Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentHeader], startIndex));
                startIndex += font20Separation;

                foreach (var item in payments)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentLine], startIndex, item.PaymentMethod,
                        item.Amount.ToCustomString(), item.Ref));
                    startIndex += font20Separation;
                }
            }

            startIndex += 60;

            double clientBalance = order.Client.OpenBalance;
            double totalCost = order.OrderTotalCost();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPreviousBalance], startIndex, clientBalance.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentAfterDisc], startIndex, totalCost.ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentSep], startIndex));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentTotalDue], startIndex, (clientBalance + totalCost).ToCustomString()));
            startIndex += font20Separation;

            startIndex += 40;

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            double paid = 0;
            if (payment != null)
            {
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentTotal], startIndex, (paid * (-1)).ToCustomString()));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentPaymentSep], startIndex));
            startIndex += font20Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FullConsignmentNewBalance], startIndex, (clientBalance + totalCost - paid).ToCustomString()));
            startIndex += font20Separation;


            return lines;
        }

        #region Proof Delivery
        public override bool PrintLabels(List<Order> orders)
        {
            return false;
        }

        public override bool PrintProofOfDelivery(Order order)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = order.PrintedOrderId;

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
                startY += Config.CompanyLogoHeight;
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.AddRange(GetHeaderRowsInOneDocDelivery(ref startY, order, order.Client, printedId));

            lines.AddRange(GetDetailsRowsInOneDocDelivery(ref startY, salesLines, creditLines, returnsLines, order)); //Products  QTY

            AddExtraSpace(ref startY, lines, 18, 1);

            lines.AddRange(GetTotalsRowsInOneDocDelivery(ref startY, order));

            lines.AddRange(GetSignatureSectionDelivery(ref startY, order, lines));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));



            if (!PrintLines(lines))
                return false;
            else
                return true;

        }

        protected override bool PrintLines(List<string> lines)
        {
            lines = InsertSpecialChar(lines);

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                if (!string.IsNullOrEmpty(s))
                    sb.Append(s);

            try
            {
                DateTime st = DateTime.Now;
                string s = sb.ToString();
                PrintIt(s);
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public List<string> InsertSpecialChar(List<string> lines)
        {
            try
            {
                var newLines = new List<string>();
                foreach (var line in lines)
                {
                    var temp_line = line;
                    foreach (var c in SpecialCharacters.Keys)
                    {
                        if (line.Contains(c))
                        {
                            temp_line = temp_line.Replace(c.ToString(), SpecialCharacters[c]);
                        }
                    }

                    newLines.Add(temp_line);
                }

                return newLines;
            }
            catch (Exception ex)
            {
                return lines;
            }
        }

        protected override IList<string> GetBottomSplitText(string text = "")
        {
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 30, 30);
        }

        public Dictionary<char, string> SpecialCharacters = new Dictionary<char, string>() {
            {(char)241, "n" },//ñ
            {(char)209, "N" },//ñ
            {(char)233, "e" },//é
            {(char)201, "E" },//É
            {(char)224, "a" },//à
            {(char)192, "A" },//À
            {(char)200, "E" },//È
            {(char)232, "e" },//è
            {(char)217, "U" },//Ù
            {(char)249, "u" },//ù
            {(char)194, "A" },//Â
            {(char)226, "a" },//â
            {(char)234, "e" },//ê
            {(char)202, "E" },//Ê
            {(char)206, "I" },//Î
            {(char)238, "i" },//î
            {(char)244, "o" },//ô
            {(char)212, "O" },//Ô
            {(char)251, "u" },//û
            {(char)219, "U" },//Û
            {(char)228, "a" },//ä
            {(char)196, "A" },//Ä
            {(char)203, "E" },//Ë
            {(char)235, "e" },//ë
            {(char)220, "U" },//Ü
            {(char)252, "u" },//ü
            {(char)199, "C" },//Ç
            {(char)231, "c" },//ç
            {(char)239, "i" },//ï
            {(char)207, "I" },//Ï
        };

        protected override IEnumerable<string> GetHeaderRowsInOneDocDelivery(ref int startY, Order order, Client client, string printedId)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = GetOrderDocumentNameDelivery(ref printExtraDocName, order, client);

            string s1 = docName;
            string s2 = string.Empty;
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

            var custno = UDFHelper.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
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
        
        #endregion



        #endregion
    }
}

