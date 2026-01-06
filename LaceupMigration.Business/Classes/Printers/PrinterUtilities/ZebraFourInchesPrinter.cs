using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{

    public class ZebraFourInchesPrinter : ZebraGenericPrinter
    {

        protected override void FillDictionary()
        {
            linesTemplates.Add(SalesRegisterHeaderTitle1, "^FO40,{0}^ADN,36,20^FDSales Register Report^FS");
            linesTemplates.Add(SalesRegisterDayReport, "^FO40,{0}^ADN,18,10^FDClock In: {1}  Clock Out: {2} Worked {3}h:{4}m^FS");
            linesTemplates.Add(SalesRegisterDayReport2, "^FO40,{0}^ADN,18,10^FDBreaks Taken: {1}h:{2}m^FS");
            linesTemplates.Add(SalesRegisterHeaderDate, "^FO40,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(SalesRegisterHeaderDriverNameText, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(SalesRegisterDetailsHeader1, "^FO40,{0}^ABN,18,10^FDName^FS" +
            "^FO350,{0}^ABN,18,10^FDSt^FS" +
            "^FO400,{0}^ABN,18,10^FDQty^FS" +
            "^FO480,{0}^ABN,18,10^FDTicket #.^FS" +
            "^FO610,{0}^ABN,18,10^FDTotal^FS" +
            "^FO700,{0}^ABN,18,10^FDCS Tp^FS");
            linesTemplates.Add(SalesRegisterDetailsRow1, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO350,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO400,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO480,{0}^ABN,18,10^FD{4}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                               "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(SalesRegisterDetailsRow2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(SalesRegisterTotalRow, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO470,{0}^ABN,18,10^FD^FS" +
                               "^FO510,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO700,{0}^ABN,18,10^FD^FS");

            linesTemplates.Add(SalesRegisterBottomSectionRow, "^FO40,{0}^ABN,18,10^FD{1} {2}^FS" +
                               "^FO500,{0}^ABN,18,10^FD{3} {4}^FS");

            linesTemplates.Add(RouteReturnsHeaderTitle1, "^FO40,{0}^ADN,36,20^FDRoute Return Report^FS");
            linesTemplates.Add(RouteReturnsHeaderDate, "^FO40,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(RouteReturnsHeaderDriverNameText, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(RouteReturnsNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL Route Return^FS");
            //nueva linea para dividir el label pues no cabe en printer de 3
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(RouteReturnsDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO500,{0}^ADN,18,10^FDDmg^FS^^FO600,{0}^ADN,18,10^FDReturns^FS^FO700,{0}^ADN,18,10^FDDump^FS");
            linesTemplates.Add(RouteReturnsDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO500,{0}^ADN,18,10^FD{4}^FS^FO600,{0}^ADN,18,10^FD{2}^FS^FO700,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(RouteReturnsDetailsFooter, "^FO40,{0}^ADN,18,10^FDTotals:^FS^FO500,{0}^ADN,18,10^FD{3}^FS^FO600,{0}^ADN,18,10^FD{1}^FS^FO700,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(LoadOrderHeaderTitle1, "^FO40,{0}^ADN,36,20^FDLoad Order Report^FS");
            linesTemplates.Add(LoadOrderHeaderDate, "^FO40,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderPrintedDate, "^FO40,{0}^ADN,18,10^FDLoad Order Request Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderDriverNameText, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(LoadOrderNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL Load Order^FS");

            linesTemplates.Add(LoadOrderDetailsHeader1, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO600,{0}^ADN,18,10^FDUOM^FS" +
                "^FO680,{0}^ADN,18,10^FDORDERED^FS");
            linesTemplates.Add(LoadOrderDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(LoadOrderDetailsFooter, "^FO40,{0}^ADN,18,10^FDTotals:^FS^FO680,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(ConsignmentHeaderTitle1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ConsignmentHeaderDate, "^FO380,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(ConsignmentHeaderDriverNameText, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");


            linesTemplates.Add(ConsignmentDetailsHeaderContract1, "^FO40,{0}^ADN,18,10^FD^FS^FO440,{0}^ADN,18,10^FD{1}^FS^FO520,{0}^ADN,18,10^FDNew^FS^FO600,{0}^ADN,18,10^FD^FS^FO690,{0}^ADN,18,10^FD^FS");
            linesTemplates.Add(ConsignmentDetailsHeaderContract2, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO440,{0}^ADN,18,10^FD{1}^FS^FO520,{0}^ADN,18,10^FDCons^FS^FO600,{0}^ADN,18,10^FDPrice^FS^FO690,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(ConsignmentDetailsHeader1, "^FO40,{0}^ADN,18,10^FD^FS^FO390,{0}^ADN,18,10^FDCons^FS^FO445,{0}^ADN,18,10^FDCount^FS^FO513,{0}^ADN,18,10^FDSold^FS^FO570,{0}^ADN,18,10^FDPrice^FS^FO670,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsHeader2, "^FO40,{0}^ADN,18,10^FD^FS^FO390,{0}^ADN,18,10^FDCons^FS^FO445,{0}^ADN,18,10^FDCount^FS^FO513,{0}^ADN,18,10^FDSold^FS^FO570,{0}^ADN,18,10^FDPrice^FS^FO670,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO390,{0}^ADN,18,10^FD{2}^FS^FO445,{0}^ADN,18,10^FD{3}^FS^FO513,{0}^ADN,18,10^FD{4}^FS^FO570,{0}^ADN,18,10^FD{5}^FS^FO670,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ConsignmentDetailsLine2, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO390,{0}^ADN,18,10^FD{2}^FS^FO445,{0}^ADN,18,10^FD{3}^FS^FO513,{0}^ADN,18,10^FD{4}^FS^FO570,{0}^ADN,18,10^FD{5}^FS^FO670,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(ConsignmentDetailsContractLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO440,{0}^ADN,18,10^FD{2}^FS^FO520,{0}^ADN,18,10^FD{3}^FS^FO600,{0}^ADN,18,10^FD{4}^FS^FO690,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(ConsignmentDetailsContractTotalLine, "^FO300,{0}^ADN,18,10^FD{1}^FS^FO440,{0}^ADN,18,10^FD{2}^FS^FO520,{0}^ADN,18,10^FD{3}^FS^FO600,{0}^ADN,18,10^FD{4}^FS^FO690,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(ConsignmentDetailsTotalLine, "^FO390,{0}^ADN,18,10^FD{1}^FS^FO513,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(ConsignmentDetailsLineUPC, "^FO40,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(ConsignmentDetailsFooter, "^FO40,{0}^ADN,18,10^FDTotals:^FS^FO680,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(InventorySettlementHeaderTitle1, "^FO40,{0}^ADN,36,20^FDInventory Settlement Report^FS");
            linesTemplates.Add(InventorySettlementHeaderLabel1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventorySettlementHeaderDate, "^FO40,{0}^ADN,18,10^FDPrinted: {1}^FS");

            linesTemplates.Add(InventorySettlementDetailsHeader1, "^FO40,{0}^ABN,18,10^FDProduct^FS" +
            "^FO330,{0}^ABN,18,10^FDBeg^FS" +
            "^FO370,{0}^ABN,18,10^FDLoad^FS" +
            "^FO410,{0}^ABN,18,10^FDAdj^FS" +
            "^FO450,{0}^ABN,18,10^FDTr.^FS" +
            "^FO490,{0}^ABN,18,10^FDSls^FS" +
            "^FO530,{0}^ABN,18,10^FDRet^FS" +
            "^FO570,{0}^ABN,18,10^FDDump^FS" +
            "^FO610,{0}^ABN,18,10^FDDmg^FS" +
            "^FO650,{0}^ABN,18,10^FDRout^FS" +
            "^FO690,{0}^ABN,18,10^FDEnd^FS" +
            "^FO730,{0}^ABN,18,10^FDOVER^FS");

            linesTemplates.Add(InventorySettlementDetailsHeader2, "^FO40,{0}^ABN,18,10^FD^FS" +
            "^FO330,{0}^ABN,18,10^FDInv^FS" +
            "^FO470,{0}^ABN,18,10^FD^FS" +
            "^FO410,{0}^ABN,18,10^FD^FS" +
            "^FO450,{0}^ABN,18,10^FD^FS" +
            "^FO530,{0}^ABN,18,10^FD^FS" +
            "^FO570,{0}^ABN,18,10^FD^FS" +
            "^FO610,{0}^ABN,18,10^FD^FS" +
            "^FO650,{0}^ABN,18,10^FDRet^FS" +
            "^FO690,{0}^ABN,18,10^FDInv^FS" +
            "^FO730,{0}^ABN,18,10^FDShort^FS");

            linesTemplates.Add(InventorySettlementDetailRow, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO330,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO370,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO410,{0}^ABN,18,10^FD{4}^FS" +
                               "^FO450,{0}^ABN,18,10^FD{5}^FS" +
                               "^FO490,{0}^ABN,18,10^FD{6}^FS" +
                               "^FO530,{0}^ABN,18,10^FD{7}^FS" +
                               "^FO570,{0}^ABN,18,10^FD{8}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{9}^FS" +
                               "^FO650,{0}^ABN,18,10^FD{10}^FS" +
                               "^FO690,{0}^ABN,18,10^FD{11}^FS" +
                               "^FO730,{0}^ABN,18,10^FD{12}^FS");

            linesTemplates.Add(InventorySummaryHeaderTitle1, "^FO40,{0}^ADN,36,20^FDInventory Summary^FS");
            linesTemplates.Add(InventorySummaryHeaderLabel1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventorySummaryHeaderDate, "^FO40,{0}^ADN,18,10^FDPrinted: {1}^FS");

            linesTemplates.Add(InventorySummaryDetailsHeader1,
                "^FO40,{0}^ABN,18,10^FDProduct^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS"
                );
            linesTemplates.Add(InventorySummaryDetailsHeader2,
                "^FO40,{0}^ABN,18,10^FDLot^FS" +
                "^FO140,{0}^ABN,18,10^FDUoM^FS" +
                "^FO220,{0}^ABN,18,10^FDBeg. Inv^FS" +
                "^FO340,{0}^ABN,18,10^FDLoaded^FS" +
                "^FO460,{0}^ABN,18,10^FDTransfer^FS" +
                "^FO580,{0}^ABN,18,10^FDSales^FS" +
                "^FO700,{0}^ABN,18,10^FDCurr. Inv^FS"
                );

            linesTemplates.Add(InventorySummaryProductRow,
                "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS"
                );

            linesTemplates.Add(InventorySummaryDetailRow,
               "^FO40,{0}^ABN,18,10^FD{1}^FS" +
               "^FO140,{0}^ABN,18,10^FD{2}^FS" +
               "^FO220,{0}^ABN,18,10^FD{3}^FS" +
               "^FO340,{0}^ABN,18,10^FD{4}^FS" +
               "^FO460,{0}^ABN,18,10^FD{5}^FS" +
               "^FO580,{0}^ABN,18,10^FD{6}^FS" +
               "^FO700,{0}^ABN,18,10^FD{7}^FS"
               );

            linesTemplates.Add(InventorySummaryTotalsRow,
                "^FO40,{0}^ABN,18,10^FDTotals:^FS" +
                "^FO120,{0}^ABN,18,10^FD{1}^FS" +
                "^FO120,{0}^ABN,18,10^FD{2}^FS" +
                "^FO120,{0}^ABN,18,10^FD{3}^FS" +
                "^FO220,{0}^ABN,18,10^FD{4}^FS" +
                "^FO340,{0}^ABN,18,10^FD{5}^FS" +
                "^FO460,{0}^ABN,18,10^FD{6}^FS" +
                "^FO580,{0}^ABN,18,10^FD{7}^FS" +
                "^FO700,{0}^ABN,18,10^FD{8}^FS");



            linesTemplates.Add(TransferOnOffHeaderTitle1, "^FO40,{0}^ADN,36,20^FDTransfer {1} Report^FS");
            linesTemplates.Add(TransferOnOffHeaderDriverNameText, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(TransferOnOffDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO570,{0}^ADN,18,10^FD{1}^FS^FO650,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TransferOnOffDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO570,{0}^ADN,18,10^FD{2}^FS^FO690,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(TransferOnOffNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL TRANSFER^FS");
            linesTemplates.Add(TransferOnOffFooterSignatureLine, "^FO40,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(TransferOnOffFooterDriverSignatureText, "^FO40,{0}^ADN,18,10^FDDriver Signature^FS");
            linesTemplates.Add(TransferOnOffFooterCheckerSignatureText, "^FO40,{0}^ADN,18,10^FDSignature^FS");

            linesTemplates.Add(PaymentHeaderTitle1, "^FO40,{0}^ADN,36,20^FDPayment Receipt^FS^FO500,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO40,{0}^ADN,36,20^FDCustomer:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO40,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO40,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderTitle2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderTitle3, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(PaymentPaid, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(CreditHeaderTitle1, "^FO40,{0}^ADN,36,20^FDCredit Memo^FS^FO500,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(ReturnHeaderTitle1, "^FO40,{0}^ADN,36,20^FDReturn^FS^FO400,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle1, "^FO40,{0}^ADN,36,20^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderHeaderTitle2, "^FO40,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle25, "^FO40,{0}^ADN,36,20^FDPO #: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle3, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(CreditHeaderTitle2, "^FO40,{0}^ADN,36,20^FDCredit #:{1}^FS");
            linesTemplates.Add(ReturnHeaderTitle2, "^FO40,{0}^ADN,36,20^FDReturn Number:{1}^FS");
            linesTemplates.Add(HeaderName, "^FO40,{1}^ADN,36,20^FD{0}^FS");

            // This line indicate that the order is just a "pre order"
            linesTemplates.Add(PreOrderHeaderTitle3, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PreOrderHeaderTitle4, "^FO40,{0}^ADN,36,20^FDPRE ORDER Date: {1}^FS");
            linesTemplates.Add(PreOrderHeaderTitle41, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            //cia name
            linesTemplates.Add(HeaderAddr1, "^FO40,{1}^ADN,18,10^FD{0}^FS");
            //addr1
            linesTemplates.Add(HeaderAddr2, "^FO40,{1}^ADN,18,10^FD{0}^FS");
            //addr2
            linesTemplates.Add(HeaderPhone, "^FO40,{1}^ADN,18,10^FD{0}^FS");
            //phone
            linesTemplates.Add(OrderHeaderTo, "^FO40,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(OrderHeaderClientName, "^FO40,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(OrderHeaderClientAddr, "^FO40,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderHeaderSectionName, "^FO400,{1}^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS^FO450,{0}^ADN,18,10^FDQTY^FS^FO580,{0}^ADN,18,10^FDPRICE^FS^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(OrderDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS^FS^FO580,{0}^ADN,18,10^FD{4}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
            //linesTemplates.Add(OrderDetailsLine, "^FO40,{0}^ADN,18,10^FH^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS^FS^FO580,{0}^ADN,18,10^FD{4}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineSecondLine, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLineSuggestedPrice, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO460,{0}^ADN,18,10^FD{5}^FS^FO560,{0}^ADN,18,10^FD{2}^FS^FO620,{0}^ADN,18,10^FD{4}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineUPC, "^FO40,{0}^BUN,40^FD{1}^FS");

            linesTemplates.Add(OrderDetailsLineUPCText, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(OrderDetailsLineLot, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal1, "^FO40,{0}^ADN,18,10^FD-------------^FS");
            linesTemplates.Add(OrderDetailsTotal14, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal13, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal15, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal2, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal3, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal4, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderPaid, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsSectionFooter, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO320,{0}^ADN,18,10^FD{2}^FS^FO450,{0}^ADN,18,10^FD{3}^FS^FO680,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(ExpectedTotal3, "^FO500,{0}^ADN,18,10^FDNumber of Exp:{1}^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO40,{0}^ADN,18,10^FDSignature Name: {1}^FS");
            linesTemplates.Add(FooterSignatureLine, "^FO40,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO40,{0}^ADN,18,10^FDSignature^FS");
            linesTemplates.Add(FooterSignaturePaymentText, "^FO40,{0}^ADN,18,10^FDPayment Received By^FS");
            linesTemplates.Add(FooterCheckerSignatureText, "^FO40,{0}^ADN,18,10^FDSignature {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO40,{0}^ADN,18,10^FD ^FS");

            linesTemplates.Add(FooterBottomText, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            // This line is shared
            linesTemplates.Add(TotalValueFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventoryPriceLine, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            // Top header, used by ALL the inventory reports
            linesTemplates.Add(InventorySalesman, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(InventoryNotFinal, "^FO40,{0}^ADN,36,20^FDNot A Final Document^FS");

            // used for the print inventory report
            linesTemplates.Add(InventoryHeaderTitle, "^FO40,{0}^ADN,18,10^FDInventory Report Date: {1}^FS");
            linesTemplates.Add(InventoryDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO600,{0}^ADN,18,10^FDStart^FS^FO680,{0}^ADN,18,10^FDCurrent^FS");
            linesTemplates.Add(InventoryDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryDetailsLineLot, "^FO420,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");

            // used for the check inventory report
            linesTemplates.Add(InventoryCheckHeaderTitle, "^FO40,{0}^ADN,18,10^FDCheck Inventory^FS^FO370,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(InventoryCheckDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO540,{0}^ADN,18,10^FDExpected^FS^FO680,{0}^ADN,18,10^FDReal^FS");
            linesTemplates.Add(InventoryCheckDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO540,{0}^ADN,18,10^FD{2}^FS^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryCheckDetailsFooter, "^FO40,{0}^ADN,18,10^FD^FS^FO540,{0}^ADN,18,10^FD{1}^FS^FO680,{0}^ADN,18,10^FD{2}^FS");

            // used for the Set inventory report
            linesTemplates.Add(SetInventoryHeaderTitle, "^FO40,{0}^ADN,18,10^FDSet Inventory Report^FS^FO370,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(SetInventoryDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO680,{0}^ADN,18,10^FDCurrent^FS");
            linesTemplates.Add(SetInventoryDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO680,{0}^ADN,18,10^FD{2}^FS");

            // used for the Add inventory report
            linesTemplates.Add(AddInventoryHeaderTitle, "^FO40,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AddInventoryHeaderTitle1, "^FO500,{0}^ADN,18,10^FDPrinted: {1}^FS");

            linesTemplates.Add(AddInventoryDetailsHeader1, "^FO40,{0}^ABN,18,10^FDProduct^FS^FO680,{0}^ABN,18,10^FDCurrent^FS");
            linesTemplates.Add(AddInventoryDetailsHeader2, "^FO40,{0}^ABN,18,10^FDProduct^FS" +
                "^FO490,{0}^ABN,18,10^FDBeg^FS" +
                "^FO560,{0}^ABN,18,10^FDLoad^FS" +
                "^FO630,{0}^ABN,18,10^FDAdj^FS" +
                "^FO700,{0}^ABN,18,10^FDStart^FS");
            linesTemplates.Add(AddInventoryDetailsHeader21, "^FO40,{0}^ABN,18,10^FD ^FS" +
                "^FO490,{0}^ABN,18,10^FDInv^FS" +
                "^FO560,{0}^ABN,18,10^FDOut^FS" +
                "^FO630,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AddInventoryDetailsLine2, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO490,{0}^ABN,18,10^FD{2}^FS" +
                "^FO560,{0}^ABN,18,10^FD{3}^FS" +
                "^FO630,{0}^ABN,18,10^FD{4}^FS" +
                "^FO700,{0}^ABN,18,10^FD{5}^FS");
            linesTemplates.Add(AddInventoryDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO680,{0}^ADN,18,10^FD{2}^FS");

            // The sales & credit report
            linesTemplates.Add(SalesReportHeaderTitle, "^FO40,{0}^ADN,18,10^FDSales/Returns Report Date: {1}^FS");
            linesTemplates.Add(SalesReportSalesman, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");
            // The sales section
            linesTemplates.Add(SalesReportSalesHeaderTitle, "^FO40,{0}^ADN,36,20^FDSales section^FS");
            linesTemplates.Add(SalesReportSalesDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO640,{0}^ADN,18,10^FDSold Qty^FS");
            linesTemplates.Add(SalesReportSalesDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO640,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportSalesFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            // The credit section
            linesTemplates.Add(SalesReportCreditHeaderTitle, "^FO40,{0}^ADN,36,20^FDDump section^FS");
            linesTemplates.Add(SalesReportCreditDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO640,{0}^ADN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportCreditDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO640,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportCreditFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            // The return section
            linesTemplates.Add(SalesReportReturnHeaderTitle, "^FO40,{0}^ADN,36,20^FDReturn section^FS");
            linesTemplates.Add(SalesReportReturnDetailsHeader1, "^FO40,{0}^ADN,18,10^FDProduct^FS^FO640,{0}^ADN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportReturnDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO640,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportReturnFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");

            // The received payments report
            linesTemplates.Add(PaymentReportHeaderTitle, "^FO40,{0}^ADN,36,20^FDPayments Received Report^FS");
            linesTemplates.Add(PaymentReportHeaderLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentReportDate, "^FO40,{0}^ADN,18,10^FDPrinted: {1}^FS");
            linesTemplates.Add(PaymentReportSalesman, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(PaymentReportHeader1, "^FO40,{0}^ABN,18,10^FDName^FS" +
            "^FO310,{0}^ABN,18,10^FDInvoice^FS" +
            "^FO430,{0}^ABN,18,10^FDInvoice^FS" +
            "^FO520,{0}^ABN,18,10^FDAmount^FS" +
            "^FO610,{0}^ABN,18,10^FDMethod^FS" +
            "^FO700,{0}^ABN,18,10^FDRef^FS");
            linesTemplates.Add(PaymentReportHeader2, "^FO40,{0}^ABN,18,10^FD^FS" +
            "^FO310,{0}^ABN,18,10^FDNumber^FS" +
            "^FO430,{0}^ABN,18,10^FDTotal^FS" +
            "^FO520,{0}^ABN,18,10^FD^FS" +
            "^FO610,{0}^ABN,18,10^FD^FS" +
            "^FO700,{0}^ABN,18,10^FDNumber^FS");
            linesTemplates.Add(PaymentReportDetail, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO310,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO430,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO520,{0}^ABN,18,10^FD{4}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                               "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(PaymentReportTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO310,{0}^ABN,18,10^FD^FS" +
                               "^FO430,{0}^ABN,18,10^FD{1}^FS" +
                               "^FO520,{0}^ABN,18,10^FD{2}^FS" +
                               "^FO610,{0}^ABN,18,10^FD{3}^FS" +
                               "^FO700,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(PaymentReportTotalReceived, "^FO430,{0}^ABN,18,10^FDTotal: {1}^FS");

            // The cash received
            linesTemplates.Add(PaymentReportCashtHeaderTitle, "^FO40,{0}^ADN,18,10^FDCash section^FS");
            linesTemplates.Add(PaymentReportCashDetailsHeader1, "^FO40,{0}^ADN,18,10^FDCustomer^FS^FO660,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCashDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO660,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PaymentReportCashFooter, "^FO40,{0}^ADN,36,20^FDCash received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCashMessage, "^FO40,{0}^ADN,18,10^FDNo cash payments were received^FS");
            // The checks received
            linesTemplates.Add(PaymentReportChecktHeaderTitle, "^FO40,{0}^ADN,18,10^FDChecks section^FS");
            linesTemplates.Add(PaymentReportCheckDetailHeaderTitle, "^FO40,{0}^ADN,18,10^FDCustomer^FS^FO510,{0}^ADN,18,10^FDCheck #^FS^FO660,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCheckDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO510,{0}^ADN,18,10^FD{2}^FS^FO660,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(PaymentReportCheckFooter, "^FO40,{0}^ADN,36,20^FDQty   checks received:{1}^FS");
            linesTemplates.Add(PaymentReportCheckAmountFooter, "^FO40,{0}^ADN,36,20^FDCheck $ received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCheckMessage, "^FO40,{0}^ADN,36,20^FDNo checks payments were received^FS");
            linesTemplates.Add(PaymentReportTotalAmountFooter, "^FO40,{0}^ADN,36,20^FDTotal received:{1}^FS");

            // The received payments report
            linesTemplates.Add(OrdersCreatedReportHeaderTitle, "^FO40,{0}^ADN,18,10^FDOrders Created Report Date: {1}^FS");
            linesTemplates.Add(OrdersCreatedReportSalesman, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS");
            // The orders in the system
            linesTemplates.Add(OrdersCreatedReportOrderSectionHeader1, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportCreditSectionHeader1, "^FO40,{0}^ADN,18,10^FDCredits^FS");
            linesTemplates.Add(OrdersCreatedReportVoidSectionHeader1, "^FO40,{0}^ADN,18,10^FDVoids^FS");
            linesTemplates.Add(OrdersCreatedReportReturnSectionHeader1, "^FO40,{0}^ADN,18,10^FDReturns^FS");
            linesTemplates.Add(OrdersCreatedReportDetailsHeader1, "^FO40,{0}^ADN,18,10^FDCustomer^FS^FO660,{0}^ADN,18,10^FDAmount^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO660,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine1, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportNoOrdersMessage, "^FO40,{0}^ADN,18,10^FDNo orders in the system.^FS");
            linesTemplates.Add(OrdersCreatedReportDetailProductLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO300,{0}^ADN,18,10^FD{2}^FS^FS^FO360,{0}^ADN,18,10^FD{4}^FS^FO460,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailUPCLine, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(UPC128, "^FO40,{0}^BCN,40^FD{1}^FS");

            linesTemplates.Add(AllowanceOrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS^FO380,{0}^ADN,18,10^FDQTY^FS^FO480,{0}^ADN,18,10^FDPRICE^FS^FO580,{0}^ADN,18,10^FDALLOW^FS^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(AllowanceOrderDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO380,{0}^ADN,18,10^FD{2}^FS^FO480,{0}^ADN,18,10^FD{3}^FS^FO580,{0}^ADN,18,10^FD-{4}^FS^FO680,{0}^ADN,18,10^FD{5}^FS");

            #region Full Consignment

            linesTemplates.Add(FullConsignmentCompanyInfo, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(FullConsignmentAgentInfo, "^FO40,{0}^ADN,18,10^FDAgent Info: {1}^FS");
            linesTemplates.Add(FullConsignmentConsignment, "^FO40,{0}^ADN,18,10^FDCONSIGNMENT {1}^FS");
            linesTemplates.Add(FullConsignmentMerchant, "^FO40,{0}^ADN,18,10^FDMerchant: {1}^FS");
            linesTemplates.Add(FullConsignmentMerchantId, "^FO40,{0}^ADN,18,10^FDMerchant ID: {1}^FS");
            linesTemplates.Add(FullConsignmentAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FullConsignmentLastTimeVisited, "^FO40,{0}^ADN,18,10^FDLast time visited: {1}^FS");
            linesTemplates.Add(FullConsignmentSectionName, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FullConsignmentCountHeader1, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO440,{0}^ADN,18,10^FDQTY^FS" +
                "^FO540,{0}^ADN,18,10^FDPrice^FS" +
                "^FO680,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(FullConsignmentCountHeader2, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO440,{0}^ADN,18,10^FD^FS" +
                "^FO540,{0}^ADN,18,10^FDPrice^FS" +
                "^FO680,{0}^ADN,18,10^FDTotal^FS");

            linesTemplates.Add(FullConsignmentCountLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO440,{0}^ADN,18,10^FD{2}^FS" +
                "^FO540,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(FullConsignmentCountSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO440,{0}^ADN,18,10^FD_______^FS" +
                "^FO540,{0}^ADN,18,10^FD__________^FS" +
                "^FO680,{0}^ADN,18,10^FD_________^FS");

            linesTemplates.Add(FullConsignmentCountTotal, "^FO40,{0}^ADN,18,10^FDTotal: {1}^FS");

            linesTemplates.Add(FullConsignmentContractHeader, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO360,{0}^ADN,18,10^FDOld^FS" +
                "^FO440,{0}^ADN,18,10^FDNew^FS" +
                "^FO520,{0}^ADN,18,10^FDCount^FS" +
                "^FO600,{0}^ADN,18,10^FDSold^FS" +
                "^FO700,{0}^ADN,18,10^FDDeliv^FS");

            linesTemplates.Add(FullConsignmentContractLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO360,{0}^ADN,18,10^FD{2}^FS" +
                "^FO440,{0}^ADN,18,10^FD{3}^FS" +
                "^FO520,{0}^ADN,18,10^FD{4}^FS" +
                "^FO600,{0}^ADN,18,10^FD{5}^FS" +
                "^FO700,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(FullConsignmentContractSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO360,{0}^ADN,18,10^FD_____^FS" +
                "^FO440,{0}^ADN,18,10^FD_____^FS" +
                "^FO520,{0}^ADN,18,10^FD_____^FS" +
                "^FO600,{0}^ADN,18,10^FD______^FS" +
                "^FO700,{0}^ADN,18,10^FD_______^FS");

            linesTemplates.Add(FullConsignmentReturnsHeader, "^FO60,{0}^ADN,18,10^FDDescription^FS" +
                "^FO700,{0}^ADN,18,10^FDPicked^FS");

            linesTemplates.Add(FullConsignmentReturnsLine, "^FO60,{0}^ADN,18,10^FD{1}^FS" +
                "^FO700,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(FullConsignmentReturnsSep, "^FO60,{0}^ADN,18,10^FD^FS" +
                "^FO700,{0}^ADN,18,10^FD______^FS");

            linesTemplates.Add(FullConsignmentText, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentTotals, "^FO40,{0}^ADN,18,10^FD{1}: {2}^FS");

            linesTemplates.Add(FullConsignmentPaymentHeader, "^FO60,{0}^ADN,18,10^FDType^FS" +
                "^FO300,{0}^ADN,18,10^FDAmount^FS" +
                "^FO440,{0}^ADN,18,10^FDDescription^FS");

            linesTemplates.Add(FullConsignmentPaymentLine, "^FO60,{0}^ADN,18,10^FD-{1}^FS" +
                "^FO300,{0}^ADN,18,10^FD{2}^FS" +
                "^FO440,{0}^ADN,18,10^FD{3}^FS");

            linesTemplates.Add(FullConsignmentPreviousBalance, "^FO40,{0}^ADN,18,10^FDPrevious Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentAfterDisc, "^FO40,{0}^ADN,18,10^FDToday Sales After Disc:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPaymentSep, "^FO240,{0}^ADN,18,10^FD___________________^FS");

            linesTemplates.Add(FullConsignmentTotalDue, "^FO200,{0}^ADN,18,10^FDTotal Due:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPaymentTotal, "^FO40,{0}^ADN,18,10^FDPayments Total:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentNewBalance, "^FO160,{0}^ADN,18,10^FDNew Balance:^FS" +
                "^FO340,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(FullConsignmentPrintedOn, "^FO40,{0}^ADN,18,10^FDReport printed on: {1}^FS");

            linesTemplates.Add(FullConsignmentSignature, "^FO200,{0}^ADN,18,10^FDSignature: ------------------------------------^FS");

            linesTemplates.Add(FullConsignmentFinalized, "^FO250,{0}^ADN,36,20^FD{1}^FS");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "^FO40,{0}^ADN,28,14^FDCustomer Open Balance^FS");
            linesTemplates.Add(ClientStatementTableHeader, "^FO40,{0}^ADN,18,10^FDType^FS" +
                "^FO150,{0}^ADN,18,10^FDDate^FS" +
                "^FO285,{0}^ADN,18,10^FDNumber^FS" +
                "^FO420,{0}^ADN,18,10^FDDue Date^FS" +
                "^FO540,{0}^ADN,18,10^FDAmount^FS" +
                "^FO670,{0}^ADN,18,10^FDOpen^FS");
            linesTemplates.Add(ClientStatementTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO150,{0}^ADN,18,10^FD{2}^FS" +
                "^FO285,{0}^ADN,18,10^FD{3}^FS" +
                "^FO420,{0}^ADN,18,10^FD{4}^FS" +
                "^FO540,{0}^ADN,18,10^FD{5}^FS" +
                "^FO670,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ClientStatementTableTotal, "^FO100,{0}^ADN,28,14^FD{1} {2}^FS");


            #endregion

            linesTemplates.Add(InvoiceTitleNumber, "^CF0,40^FO40,{0}^FD{1}^FS");


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

            #region Credit Report

            linesTemplates.Add(StandarPrintedDate, "^FO40,{0}^ADN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(StandarPrintRouteNumber, "^FO40,{0}^ADN,18,10^FDRoute #: {1}^FS");
            linesTemplates.Add(StandarPrintDriverName, "^FO40,{0}^ADN,18,10^FDDriver Name: {1}^FS");

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

            #region Standard

            linesTemplates.Add(StandarPrintTitle, "^FO40,{0}^ADN,36,20^FD{1}^FS^FO400,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(StandarPrintDate, "^FO40,{0}^ADN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintDateBig, "^CF0,30^FO40,{0}^FDDate: {1}^FS");
            linesTemplates.Add(StandarPrintCreatedBy, "^FO40,{0}^ADN,18,10^FDSalesman: {1}^FS");
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

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "F040,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(DeliveryInvoiceNumber, "^FO40,169^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO680,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO689,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TotalQtysProofDelivery, "^FO610,{0}^ADN,18,10^FDTOTAL: {1}^FS");
            linesTemplates.Add(OrderDetailsHeadersUoMDelivery, "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" + "^FO520,{0}^ADN,18,10^FDUOM^FS" + "^FO689,{0}^ADN,18,10^FDQTY^FS");
            linesTemplates.Add(OrderDetailsTotalsUoMDelivery, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO520,{0}^ADN,18,10^FD{3}^FS" + "^FO689,{0}^ADN,18,10^FD{2}^FS");
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

          
            
            linesTemplates.Add(OrderDetailsHeaderSectionName, "^FO400,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLines2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLotQty, "^FO40,{0}^ADN,18,10^FDLot: {1} -> {2}^FS");
            linesTemplates.Add(OrderDetailsWeights, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(OrderDetailsWeightsCount, "^FO40,{0}^ADN,18,10^FDQty: {1}^FS");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "^FO40,{0}^ADN,18,10^FDRetail price {1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "^FO40,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "^BY3,3,40^FO60,{0}^BEN,40,Y,N^FD{1}^FS");

            linesTemplates.Add(OrderDetailsTotals, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(OrderTotalContainers, "^FO40,{0}^ADN,36,20^FD     CONTAINERS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetQty, "^FO40,{0}^ADN,36,20^FD        NET QTY: {1}^FS");
            linesTemplates.Add(OrderTotalsSales, "^FO40,{0}^ADN,36,20^FD          SALES: {1}^FS");
            linesTemplates.Add(OrderTotalsCredits, "^FO40,{0}^ADN,36,20^FD        CREDITS: {1}^FS");
            linesTemplates.Add(OrderTotalsReturns, "^FO40,{0}^ADN,36,20^FD        RETURNS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetAmount, "^FO40,{0}^ADN,36,20^FD     NET AMOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscount, "^FO40,{0}^ADN,36,20^FD       DISCOUNT: {1}^FS");
            linesTemplates.Add(OrderTotalsTax, "^FO40,{0}^ADN,36,20^FD{1} {2}^FS");
            linesTemplates.Add(OrderTotalsTotalDue, "^FO40,{0}^ADN,36,20^FD      TOTAL DUE: {1}^FS");
            linesTemplates.Add(OrderTotalsTotalPayment, "^FO40,{0}^ADN,36,20^FD  TOTAL PAYMENT: {1}^FS");
            linesTemplates.Add(OrderTotalsCurrentBalance, "^FO40,{0}^ADN,36,20^FDINVOICE BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "^FO40,{0}^ADN,36,20^FD   OPEN BALANCE: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscountComment, "^FO40,{0}^ADN,18,10^FD Discount Comment: {1}^FS");
            linesTemplates.Add(OrderPreorderLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderComment, "^FO40,{0}^ADN,18,10^FDComments: {1}^FS");
            linesTemplates.Add(OrderComment2, "^FO40,{0}^ADN,18,10^FD          {1}^FS");
            linesTemplates.Add(PaymentComment, "^FO40,{0}^ADN,18,10^FDPayment Comments: {1}^FS");
            linesTemplates.Add(PaymentComment1, "^FO40,{0}^ADN,18,10^FD                  {1}^FS");
            linesTemplates.Add(OrderCommentWork, "^FO40,{0}^AON,24,15^FD{1}^FS");

            #endregion

            linesTemplates.Add(EndLabel, "^XZ");
            linesTemplates.Add(StartLabel, "^XA^PON^MNN^LL{0}");

            #region pick ticket

            linesTemplates.Add(PickTicketCompanyHeader, "^FO15,{0}^CF0,33^FB520,1,0,L^FD{1}^FS");
            linesTemplates.Add(PickTicketRouteInfo, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PickTicketDeliveryDate, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(PickTicketDriver, "^FO15,{0}^ADN,18,10^FD{1}^FS^FO600,{0}^ADN,18,10^FD{2}^FS");


            linesTemplates.Add(PickTicketProductHeader, "^FO40,{0}^ABN,18,10^FDPRODUCT #^FS" +
          "^FO310,{0}^ABN,18,10^FDDESCRIPTION^FS" +
          "^FO430,{0}^ABN,18,10^FDCASES^FS" +
          "^FO520,{0}^ABN,18,10^FDUNITS^FS");

            linesTemplates.Add(PickTicketProductLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
    "^FO310,{0}^ABN,18,10^FD{2}^FS" +
    "^FO430,{0}^ABN,18,10^FD{3}^FS" +
    "^FO520,{0}^ABN,18,10^FD{4}^FS");

            linesTemplates.Add(PickTicketProductTotal, "^FO40,{0}^ABN,18,10^FDTOTALS^FS" +
       "^FO430,{0}^ABN,18,10^FD{1}^FS" +
       "^FO520,{0}^ABN,18,10^FD{2}^FS");

            #endregion


        }

        protected override IList<string> GetClientNameSplit(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> PrintSalesCreditReportSplitProductName1(string name)
        {
            return SplitProductName(name, 35, 40);
        }

        protected override IList<string> PrintSalesCreditReportSplitProductName2(string name)
        {
            return SplitProductName(name, 35, 40);
        }

        protected override IList<string> PrintReceivedPaymentsReportSplitProductName1(string name)
        {
            return SplitProductName(name, 35, 40);
        }

        protected override IList<string> PrintReceivedPaymentsReportSplitProductName2(string name)
        {
            return SplitProductName(name, 30, 40);
        }

        protected override IList<string> GetTransferOnOffSplitProductName(string name)
        {
            return SplitProductName(name, 39, 39);
        }

        protected override IList<string> PrintOrdersCreatedReportSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
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
            return SplitProductName(name, 33, 33);
        }

        protected override IList<string> GetDetailsRowsSplitProductNameConsignment(string name, bool shortName)
        {
            if (shortName)
                return SplitProductName(name, 28, 28);
            else
                return SplitProductName(name, 32, 32);
        }

        protected override IList<string> GetDetailsRowsSplitProductName2(string name)
        {
            return SplitProductName(name, 50, 50);
        }

        protected override IList<string> GetBottomTextSplitText(string text = "")
        {
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 50, 50);
        }

        protected override IList<string> GetBottomDiscountTextSplitText()
        {
            return SplitProductName(Config.Discount100PercentPrintText, 50, 50);
        }

        protected override IList<string> GetInventoryCheckDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 30, 40);
        }

        protected override IList<string> GetInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 40, 40);
        }

        protected override IList<string> GetSetInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> GetAddInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 40);
        }

        protected override IList<string> GetSalesRegRepClientSplitProductName(string name)
        {
            return SplitProductName(name, 25, 30);
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

        protected override IList<string> OrderCommentsSplit(string name)
        {
            return SplitProductName(name, 53, 53);
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 29, 29);
        }

        //para dividir (NOT A FINAL Route Return) pues no cabe en printer de 3
        protected override IList<string> GetLabelNotAFinalRouteReturn(string name)
        {
            return SplitProductName(name, name.Length, name.Length);
        }

        protected override IList<string> GetInventorySetDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 30, 30);
        }

        protected override IList<string> GetDetailsRowsSplitProductNameAllowance(string name)
        {
            return SplitProductName(name, 25, 30);
        }

        protected override IList<string> GetOrderPaymentSplitComment(string name)
        {
            return SplitProductName(name, 45, 45);
        }

        #region Consignment

        protected IList<string> GetConsignmentHeaderRows(ref int startIndex, bool counting)
        {
            var list = new List<string>();

            if (counting)
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeader1], startIndex));
            else
            {
                if (Config.NewConsPrinter)
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeaderContract2], startIndex, string.Empty));
                else
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeaderContract1], startIndex, "Old"));
                    startIndex += font18Separation;
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsHeaderContract2], startIndex, "Cons"));
                }
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

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (counting && !detail.ConsignmentCounted)
                    continue;

                if (!counting && Config.NewConsPrinter && detail.ConsignmentNew == 0)
                    continue;

                if (Config.UseFullConsignment && detail.ConsignmentNew == 0 && detail.ConsignmentSalesItem)
                    continue;

                int index = 0;

                totalQty += detail.Qty;
                sold += detail.Qty;
                added += detail.ConsignmentPick;
                newConsignment += detail.ConsignmentNew;

                var productSlices = GetDetailsRowsSplitProductNameConsignment(detail.Product.Name, counting || !Config.NewConsPrinter);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        if (counting)
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                                    detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture),
                                                     detail.ConsignmentCount.ToString(CultureInfo.CurrentCulture),
                                                    detail.Qty.ToString(CultureInfo.CurrentCulture),
                                                    detail.Price.ToCustomString(),
                                                    (detail.Price * detail.Qty).ToCustomString()
                                                    ));
                        else
                        {
                            if (Config.NewConsPrinter)
                            {
                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                    string.Empty,
                                                    detail.ConsignmentNew.ToString(CultureInfo.CurrentCulture),
                                                    detail.ConsignmentNewPrice.ToCustomString(),
                                                    (detail.ConsignmentNewPrice * detail.ConsignmentNew).ToCustomString()));
                            }
                            else
                            {
                                var consNew = detail.ConsignmentNew;

                                if (!detail.ConsignmentUpdated && detail.ConsignmentNew == 0)
                                    consNew = detail.ConsignmentOld;

                                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                    detail.ConsignmentOld.ToString(CultureInfo.InvariantCulture),
                                                    consNew.ToString(CultureInfo.CurrentCulture),
                                                    detail.ConsignmentNewPrice.ToCustomString(),
                                                    (detail.ConsignmentNewPrice * (detail.ConsignmentUpdated ? detail.ConsignmentNew : detail.ConsignmentOld)).ToCustomString()));
                            }

                        }


                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
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

            if (!counting)
            {
                startIndex += font18Separation;
                lines.Add(GetTotalsContract(ref startIndex, order));
            }

            return lines;
        }

        string GetTotalsContract(ref int startIndex, Order order)
        {
            var oldTotal = order.Details.Sum(x => x.ConsignmentOld);

            float newTotal = order.Details.Sum(x => x.ConsignmentUpdated ? x.ConsignmentNew : x.ConsignmentOld);

            double totalPrice;

            if (Config.NewConsPrinter)
            {
                totalPrice = order.Details.Sum(x => x.ConsignmentNew * x.ConsignmentNewPrice);

                return string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractTotalLine], startIndex, "    Totals:",
                                string.Empty, newTotal.ToString(CultureInfo.CurrentCulture), string.Empty, totalPrice.ToCustomString());
            }

            totalPrice = order.Details.Sum(x => (x.ConsignmentUpdated ? x.ConsignmentNew : x.ConsignmentOld) * x.ConsignmentNewPrice);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractTotalLine], startIndex, "Totals:",
                oldTotal, newTotal.ToString(CultureInfo.CurrentCulture), string.Empty, totalPrice.ToCustomString());
        }

        public override bool PrintLabels(List<Order> orders)
        {
            double DocumentHeight = 0;

            List<string> allLines = new List<string>();

            if (!string.IsNullOrEmpty(Config.PrintLabelHeight))
            {
                Double.TryParse(Config.PrintLabelHeight, out double labelHeight);

                switch (labelHeight)
                {
                    case 1:
                        DocumentHeight = 204;
                        break;
                    case 2:
                        DocumentHeight = 406;
                        break;
                    case 1.5:
                        DocumentHeight = 305;
                        break;
                    case 3:
                        DocumentHeight = 608;
                        break;
                }
            }
            else
            {
                //use default 1 inch label
                DocumentHeight = 224;
            }

            foreach (var order in orders)
            {
                foreach (var detail in order.Details)
                {
                    if (detail.Product.Upc.Trim().Length > 0)
                    {
                        if (detail.IsCredit)
                            continue;

                        if (detail.Qty > 1)
                        {
                            for (int x = 0; x < detail.Qty; x++)
                            {
                                List<string> lines = new List<string>();

                                int startY = 40;

                                if (Config.PrintUpcAsText)
                                {
                                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, detail.Product.Upc));
                                    startY += font18Separation;
                                }
                                else
                                {
                                    startY += font18Separation / 2;

                                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UPC128ForLabel], startY, detail.Product.Upc));
                                    startY += font36Separation * 2;
                                }

                                var retPrice = detail.Price;

                                retPrice = GetRetailPrice1(order, detail);

                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPrice], startY, retPrice.ToCustomString()));

                                lines.Add(linesTemplates[EndLabel]);
                                //Add the start label
                                lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], DocumentHeight));

                                allLines.AddRange(lines);
                            }
                        }
                        else
                        {
                            List<string> lines = new List<string>();

                            int startY = 40;

                            if (Config.PrintUpcAsText)
                            {
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, detail.Product.Upc));
                                startY += font18Separation;
                            }
                            else
                            {
                                startY += font18Separation / 2;

                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UPC128ForLabel], startY, detail.Product.Upc));
                                startY += font36Separation * 2;
                            }

                            var retPrice = detail.Price;

                            retPrice = GetRetailPrice1(order, detail);

                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RetailPrice], startY, retPrice.ToCustomString()));

                            lines.Add(linesTemplates[EndLabel]);
                            //Add the start label
                            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], DocumentHeight));


                            allLines.AddRange(lines);
                        }
                    }
                }
            }

            return PrintLines(allLines);
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
            return SplitProductName(string.IsNullOrEmpty(text) ? Config.BottomOrderPrintText : text, 40, 40);
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

        private double GetRetailPrice1(Order order, OrderDetail detail)
        {
            double retPrice = 0;

            if (order.Client.RetailPriceLevelId != 0)
            {
                var retailPriceLevel = RetailPriceLevel.Pricelist.FirstOrDefault(x => x.Id == order.Client.RetailPriceLevelId);

                if (retailPriceLevel != null)
                {
                    if (retailPriceLevel.RetailPriceLevelType == 1)
                    {
                        retPrice = GetRetailPrice(detail.Product, order.Client);
                    }
                    else
                    {
                        if (retailPriceLevel.Percentage != 0 && retailPriceLevel.Percentage > 0)
                        {
                            retPrice = (detail.Price / (100 - retailPriceLevel.Percentage)) * 100;
                        }
                        else
                            retPrice = GetRetailPrice(detail.Product, order.Client);
                    }
                }
                else
                    retPrice = GetRetailPrice(detail.Product, order.Client);
            }
            else
                retPrice = GetRetailPrice(detail.Product, order.Client);

            return retPrice;
        }

        double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        

        #endregion

    }
}

