


using System;
using System.Collections.Generic;
using System.Threading;

using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class PrintekPrinter : ZebraFourInchesPrinter
    {
        protected override void FillDictionary()
        {
            linesTemplates.Add(SalesRegisterHeaderTitle1, "^FO40,{0}^ADN,36,20^FDSales Register Report^FS");
            linesTemplates.Add(SalesRegisterDayReport, "^FO40,{0}^AFN,18,10^FDClock In: {1}  Clock Out: {2} Worked {3}h:{4}m^FS");
            linesTemplates.Add(SalesRegisterDayReport2, "^FO40,{0}^AFN,18,10^FDBreaks Taken: {1}h:{2}m^FS");
            linesTemplates.Add(SalesRegisterHeaderDate, "^FO40,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(SalesRegisterHeaderDriverNameText, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(SalesRegisterDetailsHeader1, "^FO40,{0}^AFN,18,10^FDName^FS" +
            "^FO400,{0}^AFN,18,10^FDSt^FS" +
            "^FO480,{0}^AFN,18,10^FDTicket #.^FS" +
            "^FO610,{0}^AFN,18,10^FDTotal^FS" +
            "^FO700,{0}^AFN,18,10^FDCS Tp^FS");
            linesTemplates.Add(SalesRegisterDetailsRow1, "^FO40,{0}^AFN,18,10^FD{1}^FS" +
                               "^FO400,{0}^AFN,18,10^FD{2}^FS" +
                               "^FO480,{0}^AFN,18,10^FD{3}^FS" +
                               "^FO610,{0}^AFN,18,10^FD{4}^FS" +
                               "^FO700,{0}^AFN,18,10^FD{5}^FS");
            linesTemplates.Add(SalesRegisterDetailsRow2, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(SalesRegisterTotalRow, "^FO40,{0}^AFN,18,10^FD^FS" +
                               "^FO470,{0}^AFN,18,10^FD^FS" +
                               "^FO510,{0}^AFN,18,10^FDTotals:^FS" +
                               "^FO610,{0}^AFN,18,10^FD{1}^FS" +
                               "^FO700,{0}^AFN,18,10^FD^FS");

            linesTemplates.Add(SalesRegisterBottomSectionRow, "^FO40,{0}^AFN,18,10^FD{1} {2}^FS" +
                               "^FO450,{0}^AFN,18,10^FD{3} {4}^FS");

            linesTemplates.Add(RouteReturnsHeaderTitle1, "^FO40,{0}^ADN,36,20^FDRoute Return Report^FS");
            linesTemplates.Add(RouteReturnsHeaderDate, "^FO40,{0}^AFN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(RouteReturnsHeaderDriverNameText, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(RouteReturnsNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL Route Return^FS");
            //nueva linea para dividir el label pues no cabe en printer de 3
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(RouteReturnsDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO500,{0}^AFN,18,10^FDDmg^FS^^FO600,{0}^AFN,18,10^FDReturns^FS^FO700,{0}^AFN,18,10^FDDump^FS");
            linesTemplates.Add(RouteReturnsDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO500,{0}^AFN,18,10^FD{4}^FS^FO600,{0}^AFN,18,10^FD{2}^FS^FO700,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(RouteReturnsDetailsFooter, "^FO40,{0}^AFN,18,10^FDTotals:^FS^FO500,{0}^AFN,18,10^FD{3}^FS^FO600,{0}^AFN,18,10^FD{1}^FS^FO700,{0}^AFN,18,10^FD{2}^FS");

            linesTemplates.Add(LoadOrderHeaderTitle1, "^FO40,{0}^ADN,36,20^FDLoad Order Report^FS");
            linesTemplates.Add(LoadOrderHeaderDate, "^FO40,{0}^AFN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderPrintedDate, "^FO40,{0}^AFN,18,10^FDLoad Order Request Date: {1}^FS");
            linesTemplates.Add(LoadOrderHeaderDriverNameText, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(LoadOrderNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL Load Order^FS");
            linesTemplates.Add(LoadOrderDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO680,{0}^AFN,18,10^FDOrdered^FS");
            linesTemplates.Add(LoadOrderDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO680,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(LoadOrderDetailsFooter, "^FO40,{0}^AFN,18,10^FDTotals:^FS^FO680,{0}^AFN,18,10^FD{1}^FS");

            linesTemplates.Add(ConsignmentHeaderTitle1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(ConsignmentHeaderDate, "^FO380,{0}^AFN,18,10^FDPrinted Date: {1}^FS");
            linesTemplates.Add(ConsignmentHeaderDriverNameText, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");


            linesTemplates.Add(ConsignmentDetailsHeaderContract1, "^FO40,{0}^AFN,18,10^FD^FS^FO520,{0}^AFN,18,10^FDNew^FS^FO600,{0}^AFN,18,10^FD^FS^FO690,{0}^AFN,18,10^FD^FS");
            linesTemplates.Add(ConsignmentDetailsHeaderContract2, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO520,{0}^AFN,18,10^FDCons^FS^FO600,{0}^AFN,18,10^FDPrice^FS^FO690,{0}^AFN,18,10^FDTotal^FS");

            linesTemplates.Add(ConsignmentDetailsHeader1, "^FO40,{0}^AFN,18,10^FD^FS^FO390,{0}^AFN,18,10^FDCons^FS^FO445,{0}^AFN,18,10^FDCount^FS^FO513,{0}^AFN,18,10^FDSold^FS^FO570,{0}^AFN,18,10^FDPrice^FS^FO670,{0}^AFN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsHeader2, "^FO40,{0}^AFN,18,10^FD^FS^FO390,{0}^AFN,18,10^FDCons^FS^FO445,{0}^AFN,18,10^FDCount^FS^FO513,{0}^AFN,18,10^FDSold^FS^FO570,{0}^AFN,18,10^FDPrice^FS^FO670,{0}^AFN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO390,{0}^AFN,18,10^FD{2}^FS^FO445,{0}^AFN,18,10^FD{3}^FS^FO513,{0}^AFN,18,10^FD{4}^FS^FO570,{0}^AFN,18,10^FD{5}^FS^FO670,{0}^AFN,18,10^FD{6}^FS");
            linesTemplates.Add(ConsignmentDetailsLine2, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO390,{0}^AFN,18,10^FD{2}^FS^FO445,{0}^AFN,18,10^FD{3}^FS^FO513,{0}^AFN,18,10^FD{4}^FS^FO570,{0}^AFN,18,10^FD{5}^FS^FO670,{0}^AFN,18,10^FD{6}^FS");

            linesTemplates.Add(ConsignmentDetailsContractLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO520,{0}^AFN,18,10^FD{2}^FS^FO600,{0}^AFN,18,10^FD{3}^FS^FO690,{0}^AFN,18,10^FD{4}^FS");
            linesTemplates.Add(ConsignmentDetailsContractTotalLine, "^FO350,{0}^AFN,18,10^FD{1}^FS^FO520,{0}^AFN,18,10^FD{2}^FS^FO600,{0}^AFN,18,10^FD{3}^FS^FO690,{0}^AFN,18,10^FD{4}^FS");
            linesTemplates.Add(ConsignmentDetailsTotalLine, "^FO390,{0}^AFN,18,10^FD{1}^FS^FO513,{0}^AFN,18,10^FD{2}^FS");

            linesTemplates.Add(ConsignmentDetailsLineUPC, "^FO40,{0}^BUN,40^FD{1}^FS");
            linesTemplates.Add(ConsignmentDetailsFooter, "^FO40,{0}^AFN,18,10^FDTotals:^FS^FO680,{0}^AFN,18,10^FD{1}^FS");

            linesTemplates.Add(InventorySettlementHeaderTitle1, "^FO40,{0}^ADN,36,20^FDInventory Settlement Report^FS");
            linesTemplates.Add(InventorySettlementHeaderLabel1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventorySettlementHeaderDate, "^FO40,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(InventorySettlementDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS" +
            "^FO300,{0}^AFN,18,10^FDBeg^FS" +
            "^FO350,{0}^AFN,18,10^FDLoad^FS" +
            "^FO400,{0}^AFN,18,10^FDAdj^FS" +
            "^FO450,{0}^AFN,18,10^FDTr.^FS" +
            "^FO490,{0}^AFN,18,10^FDSls^FS" +
            "^FO530,{0}^AFN,18,10^FDCre^FS" +
            "^FO570,{0}^AFN,18,10^FDDmg^FS" +
            "^FO610,{0}^AFN,18,10^FDDu.^FS" +
            "^FO650,{0}^AFN,18,10^FDRet^FS" +
            "^FO690,{0}^AFN,18,10^FDEnd^FS" +
            "^FO730,{0}^AFN,18,10^FDOVER^FS");
            linesTemplates.Add(InventorySettlementDetailsHeader2, "^FO40,{0}^AFN,18,10^FD^FS" +
            "^FO300,{0}^AFN,18,10^FDInv^FS" +
            "^FO470,{0}^AFN,18,10^FD^FS" +
            "^FO410,{0}^AFN,18,10^FD^FS" +
            "^FO450,{0}^AFN,18,10^FD^FS" +
            "^FO530,{0}^AFN,18,10^FD^FS" +
            "^FO570,{0}^AFN,18,10^FD^FS" +
            "^FO610,{0}^AFN,18,10^FD^FS" +
            "^FO650,{0}^AFN,18,10^FD^FS" +
            "^FO690,{0}^AFN,18,10^FDInv^FS" +
            "^FO730,{0}^AFN,18,10^FDShort^FS");
            linesTemplates.Add(InventorySettlementDetailRow, "^FO40,{0}^AFN,18,10^FD{1}^FS" +
                               "^FO300,{0}^AFN,18,10^FD{2}^FS" +
                               "^FO350,{0}^AFN,18,10^FD{3}^FS" +
                               "^FO400,{0}^AFN,18,10^FD{4}^FS" +
                               "^FO450,{0}^AFN,18,10^FD{5}^FS" +
                               "^FO490,{0}^AFN,18,10^FD{6}^FS" +
                               "^FO530,{0}^AFN,18,10^FD{7}^FS" +
                               "^FO570,{0}^AFN,18,10^FD{12}^FS" +
                               "^FO610,{0}^AFN,18,10^FD{8}^FS" +
                               "^FO650,{0}^AFN,18,10^FD{9}^FS" +
                               "^FO690,{0}^AFN,18,10^FD{10}^FS" +
                               "^FO730,{0}^AFN,18,10^FD{11}^FS");


            linesTemplates.Add(TransferOnOffHeaderTitle1, "^FO40,{0}^ADN,36,20^FDTransfer {1} Report^FS");
            linesTemplates.Add(TransferOnOffHeaderDriverNameText, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(TransferOnOffDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO620,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(TransferOnOffDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO680,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(TransferOnOffNotFinalLine, "^FO40,{0}^ADN,36,20^FDNOT A FINAL TRANSFER^FS");
            linesTemplates.Add(TransferOnOffFooterSignatureLine, "^FO40,{0}^AFN,18,10^FD----------------------------^FS");
            linesTemplates.Add(TransferOnOffFooterDriverSignatureText, "^FO40,{0}^AFN,18,10^FDDriver Signature^FS");
            linesTemplates.Add(TransferOnOffFooterCheckerSignatureText, "^FO40,{0}^AFN,18,10^FDSignature^FS");

            linesTemplates.Add(PaymentHeaderTitle1, "^FO40,{0}^ADN,36,20^FDPayment Receipt^FS^FO500,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO40,{0}^ADN,36,20^FDCustomer:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO40,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO40,{1}^AFN,18,10^FD{0}^FS");
            linesTemplates.Add(PaymentHeaderTitle2, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderTitle3, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(PaymentPaid, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            linesTemplates.Add(CreditHeaderTitle1, "^FO40,{0}^ADN,36,20^FDCredit Memo^FS^FO500,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(ReturnHeaderTitle1, "^FO40,{0}^ADN,36,20^FDReturn^FS^FO400,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle1, "^FO40,{0}^ADN,36,20^FD{1}^FS^FO400,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderHeaderTitle2, "^FO40,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle25, "^FO40,{0}^ADN,36,20^FDPO #: {1}^FS");
            linesTemplates.Add(OrderHeaderTitle3, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            linesTemplates.Add(CreditHeaderTitle2, "^FO40,{0}^ADN,36,20^FDCredit #:{1}^FS");
            linesTemplates.Add(ReturnHeaderTitle2, "^FO40,{0}^ADN,36,20^FDReturn Number:{1}^FS");
            linesTemplates.Add(HeaderName, "^FO40,{1}^ADN,36,20^FD{0}^FS");

            // This line indicate that the order is just a "pre order"
            linesTemplates.Add(PreOrderHeaderTitle3, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PreOrderHeaderTitle4, "^FO40,{0}^ADN,36,20^FDPRE ORDER Date: {1}^FS");
            linesTemplates.Add(PreOrderHeaderTitle41, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            //cia name
            linesTemplates.Add(HeaderAddr1, "^FO40,{1}^AFN,18,10^FD{0}^FS");
            //addr1
            linesTemplates.Add(HeaderAddr2, "^FO40,{1}^AFN,18,10^FD{0}^FS");
            //addr2
            linesTemplates.Add(HeaderPhone, "^FO40,{1}^AFN,18,10^FD{0}^FS");
            //phone
            linesTemplates.Add(OrderHeaderTo, "^FO40,{0}^ADN,18,10^FDCustomer: {1}^FS");
            linesTemplates.Add(OrderHeaderClientName, "^FO40,{1}^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(OrderHeaderClientAddr, "^FO40,{1}^AFN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderHeaderSectionName, "^FO400,{1}^AFN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeader, "^FO40,{0}^AFN,18,10^FDPRODUCT^FS^FO450,{0}^AFN,18,10^FDQTY^FS^FO580,{0}^AFN,18,10^FDPRICE^FS^FO680,{0}^AFN,18,10^FDTOTAL^FS");
            linesTemplates.Add(OrderDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO450,{0}^AFN,18,10^FD{2}^FS^FS^FO580,{0}^AFN,18,10^FD{4}^FS^FO680,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineSecondLine, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLineSuggestedPrice, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO460,{0}^AFN,18,10^FD{5}^FS^FO560,{0}^AFN,18,10^FD{2}^FS^FO620,{0}^AFN,18,10^FD{4}^FS^FO680,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLineUPC, "^FO40,{0}^BUN,40^FD{1}^FS");

            linesTemplates.Add(OrderDetailsLineUPCText, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            linesTemplates.Add(OrderDetailsLineLot, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal1, "^FO40,{0}^AFN,18,10^FD-------------^FS");
            linesTemplates.Add(OrderDetailsTotal14, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal13, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal15, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal2, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal3, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsTotal4, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderPaid, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderDetailsSectionFooter, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO320,{0}^AFN,18,10^FD{2}^FS^FO450,{0}^AFN,18,10^FD{3}^FS^FO680,{0}^AFN,18,10^FD{4}^FS");

            linesTemplates.Add(ExpectedTotal3, "^FO500,{0}^AFN,18,10^FDNumber of Exp:{1}^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO40,{0}^AFN,18,10^FDSignature Name: {1}^FS");
            linesTemplates.Add(FooterSignatureLine, "^FO40,{0}^AFN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO40,{0}^AFN,18,10^FDSignature^FS");
            linesTemplates.Add(FooterSignaturePaymentText, "^FO40,{0}^AFN,18,10^FDPayment Received By^FS");
            linesTemplates.Add(FooterCheckerSignatureText, "^FO40,{0}^AFN,18,10^FDSignature {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO40,{0}^AFN,18,10^FD ^FS");

            linesTemplates.Add(FooterBottomText, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            // This line is shared
            linesTemplates.Add(TotalValueFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InventoryPriceLine, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            // Top header, used by ALL the inventory reports
            linesTemplates.Add(InventorySalesman, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(InventoryNotFinal, "^FO40,{0}^ADN,36,20^FDNot A Final Document^FS");

            // used for the print inventory report
            linesTemplates.Add(InventoryHeaderTitle, "^FO40,{0}^AFN,18,10^FDInventory Report Date: {1}^FS");
            linesTemplates.Add(InventoryDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO600,{0}^AFN,18,10^FDStart^FS^FO680,{0}^AFN,18,10^FDCurrent^FS");
            linesTemplates.Add(InventoryDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO600,{0}^AFN,18,10^FD{2}^FS^FO680,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryDetailsLineLot, "^FO500,{0}^AFN,18,10^FD{1}^FS^FO600,{0}^AFN,18,10^FD{2}^FS^FO680,{0}^AFN,18,10^FD{3}^FS");

            // used for the check inventory report
            linesTemplates.Add(InventoryCheckHeaderTitle, "^FO40,{0}^AFN,18,10^FDCheck Inventory^FS^FO370,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(InventoryCheckDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO540,{0}^AFN,18,10^FDExpected^FS^FO680,{0}^AFN,18,10^FDReal^FS");
            linesTemplates.Add(InventoryCheckDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO540,{0}^AFN,18,10^FD{2}^FS^FO680,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryCheckDetailsFooter, "^FO40,{0}^AFN,18,10^FD^FS^FO540,{0}^AFN,18,10^FD{1}^FS^FO680,{0}^AFN,18,10^FD{2}^FS");

            // used for the Set inventory report
            linesTemplates.Add(SetInventoryHeaderTitle, "^FO40,{0}^AFN,18,10^FDSet Inventory Report^FS^FO370,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(SetInventoryDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO680,{0}^AFN,18,10^FDCurrent^FS");
            linesTemplates.Add(SetInventoryDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO680,{0}^AFN,18,10^FD{2}^FS");

            // used for the Add inventory report
            linesTemplates.Add(AddInventoryHeaderTitle, "^FO40,{0}^ADN,36,20^FDAccepted Load^FS");
            linesTemplates.Add(AddInventoryHeaderTitle1, "^FO500,{0}^AFN,18,10^FDDate: {1}^FS");

            linesTemplates.Add(AddInventoryDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO680,{0}^AFN,18,10^FDCurrent^FS");
            linesTemplates.Add(AddInventoryDetailsHeader2, "^FO40,{0}^AFN,18,10^FDProduct^FS" +
                "^FO490,{0}^AFN,18,10^FDBeg^FS" +
                "^FO560,{0}^AFN,18,10^FDLoad^FS" +
                "^FO630,{0}^AFN,18,10^FDAdj^FS" +
                "^FO700,{0}^AFN,18,10^FDStart^FS");
            linesTemplates.Add(AddInventoryDetailsHeader21, "^FO40,{0}^AFN,18,10^FD ^FS" +
                "^FO490,{0}^AFN,18,10^FDInv^FS" +
                "^FO560,{0}^AFN,18,10^FDOut^FS" +
                "^FO630,{0}^AFN,18,10^FD^FS" +
                "^FO700,{0}^AFN,18,10^FD^FS");
            linesTemplates.Add(AddInventoryDetailsLine2, "^FO40,{0}^AFN,18,10^FD{1}^FS" +
                "^FO490,{0}^AFN,18,10^FD{2}^FS" +
                "^FO560,{0}^AFN,18,10^FD{3}^FS" +
                "^FO630,{0}^AFN,18,10^FD{4}^FS" +
                "^FO700,{0}^AFN,18,10^FD{5}^FS");
            linesTemplates.Add(AddInventoryDetailsLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO680,{0}^AFN,18,10^FD{2}^FS");

            // The sales & credit report
            linesTemplates.Add(SalesReportHeaderTitle, "^FO40,{0}^AFN,18,10^FDSales/Returns Report Date: {1}^FS");
            linesTemplates.Add(SalesReportSalesman, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");
            // The sales section
            linesTemplates.Add(SalesReportSalesHeaderTitle, "^FO40,{0}^ADN,36,20^FDSales section^FS");
            linesTemplates.Add(SalesReportSalesDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO640,{0}^AFN,18,10^FDSold Qty^FS");
            linesTemplates.Add(SalesReportSalesDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO640,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportSalesFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            // The credit section
            linesTemplates.Add(SalesReportCreditHeaderTitle, "^FO40,{0}^ADN,36,20^FDDump section^FS");
            linesTemplates.Add(SalesReportCreditDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO640,{0}^AFN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportCreditDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO640,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportCreditFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            // The return section
            linesTemplates.Add(SalesReportReturnHeaderTitle, "^FO40,{0}^ADN,36,20^FDReturn section^FS");
            linesTemplates.Add(SalesReportReturnDetailsHeader1, "^FO40,{0}^AFN,18,10^FDProduct^FS^FO640,{0}^AFN,18,10^FDReturned Qty^FS");
            linesTemplates.Add(SalesReportReturnDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO640,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(SalesReportReturnFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");

            // The received payments report
            linesTemplates.Add(PaymentReportHeaderTitle, "^FO40,{0}^ADN,36,20^FDPayments Received Report^FS");
            linesTemplates.Add(PaymentReportHeaderLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentReportDate, "^FO40,{0}^AFN,18,10^FDDate: {1}^FS");
            linesTemplates.Add(PaymentReportSalesman, "^FO40,{0}^AFN,18,10^FD{1}{2}^FS");

            linesTemplates.Add(PaymentReportHeader1, "^FO40,{0}^AFN,18,10^FDName^FS" +
            "^FO310,{0}^AFN,18,10^FDInvoice^FS" +
            "^FO430,{0}^AFN,18,10^FDInvoice^FS" +
            "^FO520,{0}^AFN,18,10^FDAmount^FS" +
            "^FO610,{0}^AFN,18,10^FDMethod^FS" +
            "^FO700,{0}^AFN,18,10^FDRef^FS");
            linesTemplates.Add(PaymentReportHeader2, "^FO40,{0}^AFN,18,10^FD^FS" +
            "^FO310,{0}^AFN,18,10^FDNumber^FS" +
            "^FO430,{0}^AFN,18,10^FDTotal^FS" +
            "^FO520,{0}^AFN,18,10^FD^FS" +
            "^FO610,{0}^AFN,18,10^FD^FS" +
            "^FO700,{0}^AFN,18,10^FDNumber^FS");
            linesTemplates.Add(PaymentReportDetail, "^FO40,{0}^AFN,18,10^FD{1}^FS" +
                               "^FO310,{0}^AFN,18,10^FD{2}^FS" +
                               "^FO430,{0}^AFN,18,10^FD{3}^FS" +
                               "^FO520,{0}^AFN,18,10^FD{4}^FS" +
                               "^FO610,{0}^AFN,18,10^FD{5}^FS" +
                               "^FO700,{0}^AFN,18,10^FD{6}^FS");
            linesTemplates.Add(PaymentReportTotal, "^FO40,{0}^AFN,18,10^FD^FS" +
                               "^FO310,{0}^AFN,18,10^FD^FS" +
                               "^FO430,{0}^AFN,18,10^FD{1}^FS" +
                               "^FO520,{0}^AFN,18,10^FD{2}^FS" +
                               "^FO610,{0}^AFN,18,10^FD{3}^FS" +
                               "^FO700,{0}^AFN,18,10^FD^FS");
            linesTemplates.Add(PaymentReportTotalReceived, "^FO430,{0}^AFN,18,10^FDTotal: {1}^FS");

            // The cash received
            linesTemplates.Add(PaymentReportCashtHeaderTitle, "^FO40,{0}^AFN,18,10^FDCash section^FS");
            linesTemplates.Add(PaymentReportCashDetailsHeader1, "^FO40,{0}^AFN,18,10^FDCustomer^FS^FO660,{0}^AFN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCashDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO660,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(PaymentReportCashFooter, "^FO40,{0}^ADN,36,20^FDCash received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCashMessage, "^FO40,{0}^AFN,18,10^FDNo cash payments were received^FS");
            // The checks received
            linesTemplates.Add(PaymentReportChecktHeaderTitle, "^FO40,{0}^AFN,18,10^FDChecks section^FS");
            linesTemplates.Add(PaymentReportCheckDetailHeaderTitle, "^FO40,{0}^AFN,18,10^FDCustomer^FS^FO510,{0}^AFN,18,10^FDCheck #^FS^FO660,{0}^AFN,18,10^FDAmount^FS");
            linesTemplates.Add(PaymentReportCheckDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO510,{0}^AFN,18,10^FD{2}^FS^FO660,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(PaymentReportCheckFooter, "^FO40,{0}^ADN,36,20^FDQty   checks received:{1}^FS");
            linesTemplates.Add(PaymentReportCheckAmountFooter, "^FO40,{0}^ADN,36,20^FDCheck $ received:{1}^FS");
            linesTemplates.Add(PaymentReportNoCheckMessage, "^FO40,{0}^ADN,36,20^FDNo checks payments were received^FS");
            linesTemplates.Add(PaymentReportTotalAmountFooter, "^FO40,{0}^ADN,36,20^FDTotal received:{1}^FS");

            // The received payments report
            linesTemplates.Add(OrdersCreatedReportHeaderTitle, "^FO40,{0}^AFN,18,10^FDOrders Created Report Date: {1}^FS");
            linesTemplates.Add(OrdersCreatedReportSalesman, "^FO40,{0}^AFN,18,10^FDSalesman: {1}^FS");
            // The orders in the system
            linesTemplates.Add(OrdersCreatedReportOrderSectionHeader1, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportCreditSectionHeader1, "^FO40,{0}^AFN,18,10^FDCredits^FS");
            linesTemplates.Add(OrdersCreatedReportVoidSectionHeader1, "^FO40,{0}^AFN,18,10^FDVoids^FS");
            linesTemplates.Add(OrdersCreatedReportReturnSectionHeader1, "^FO40,{0}^AFN,18,10^FDReturns^FS");
            linesTemplates.Add(OrdersCreatedReportDetailsHeader1, "^FO40,{0}^AFN,18,10^FDCustomer^FS^FO660,{0}^AFN,18,10^FDAmount^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO660,{0}^AFN,18,10^FD{2}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailLine1, "^FO40,{0}^AFN,18,10^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportFooter1, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrdersCreatedReportNoOrdersMessage, "^FO40,{0}^AFN,18,10^FDNo orders in the system.^FS");
            linesTemplates.Add(OrdersCreatedReportDetailProductLine, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO300,{0}^AFN,18,10^FD{2}^FS^FS^FO360,{0}^AFN,18,10^FD{4}^FS^FO460,{0}^AFN,18,10^FD{3}^FS");
            linesTemplates.Add(OrdersCreatedReportDetailUPCLine, "^FO40,{0}^AFN,18,10^FD{1}^FS");

            linesTemplates.Add(UPC128, "^FO40,{0}^BCN,40^FD{1}^FS");

            linesTemplates.Add(InvoiceTitleNumber, "^CF0,60^FO40,{0}^FD{1}^FS");
        }

        protected override void PrintIt(string printingString)
        {

            try
            {
                if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
                    throw new InvalidOperationException("No valid printer selected");

                Config.helper.PrintIt(printingString);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report(ee);
            }
            base.PrintIt(printingString);
        }

        string PrintFonts(ref int starty, string pName, string qtyAsString, string totalAsString, string priceAsString)
        {
            string line = "^FO40,{0}^{5},{6},{7}^FD{1}^FS^FO450,{0}^{5},{6},{7}^FD{2}^FS^FS^FO580,{0}^{5},{6},{7}^FD{3}^FS^FO680,{0}^{5},{6},{7}^FD{4}^FS";
            string fontLine = "^FO40,{0}^{1},{2},{3}^FD{4}^FS";

            string result = "";

            var fonts = new string[] { "A", "B", "D", "E", "F", "H", "O", "P", "Q", "R", "S", "T", "U", "V" };

            int i = 0;

            foreach (var f in fonts)
            {
                if (i >= 20)
                {
                    result += "^XZ";
                    result += "^XA";
                    starty = 0;
                }

                string afn = "A" + f + "N";

                result += string.Format(CultureInfo.InvariantCulture, fontLine, starty, "ADN", 18, 10, f + " -- Font 18");
                starty += 18;

                result += string.Format(CultureInfo.InvariantCulture, line, starty, pName, qtyAsString, totalAsString, priceAsString, afn, 18, 10);
                starty += 36;

                result += string.Format(CultureInfo.InvariantCulture, fontLine, starty, "ADN", 36, 20, f + " -- Font 36");
                starty += 36;

                result += string.Format(CultureInfo.InvariantCulture, line, starty, pName, qtyAsString, totalAsString, priceAsString, afn, 36, 20);
                starty += 36;

                starty += 36;
            }

            return result;
        }


        #region Open Invoice (revisar)

        public override bool PrintOpenInvoice(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Printed on:" + DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            //Add the company details rows.

            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, "Invoice Number: " + invoice.InvoiceNumber));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, "COPY"));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Created on:" + invoice.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            string extra = string.Empty;

            if (invoice.DueDate < DateTime.Today)
                extra = " OVERDUE";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle41], startY, "Due on:    " + invoice.DueDate.ToString(Config.OrderDatePrintFormat) + extra));
            startY += font36Separation;

            Client client = invoice.Client;
            var custno = DataAccess.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
                custNoString = " " + custno.Value;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTo], startY, custNoString));
            startY += font36Separation;

            int count = lines.Count;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                if (count >= 20)
                {
                    count = 0;
                    lines.Add("^XZ");
                    lines.Add("^XA");

                    startY = 0;
                }

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
                count++;
            }

            foreach (string s1 in ClientAddress(client))
            {
                if (count >= 20)
                {
                    count = 0;
                    lines.Add("^XZ");
                    lines.Add("^XA");

                    startY = 0;
                }

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s1.Trim(), startY));
                startY += font18Separation;
                count++;
            }
            startY += font36Separation;

            foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(invoice.Comments ?? string.Empty))
            {
                if (count >= 20)
                {
                    count = 0;
                    lines.Add("^XZ");
                    lines.Add("^XA");

                    startY = 0;
                }

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, "C: " + commentPArt));
                startY += font18Separation;
                count++;
            }

            double totalUnits = 0;
            double numberOfBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeader], startY));
            startY += font18Separation;
            count++;

            Product notFoundProduct = new Product();
            notFoundProduct.Code = string.Empty;
            notFoundProduct.Cost = 0;
            notFoundProduct.Description = "Not found product";
            notFoundProduct.Name = "Not found product";
            notFoundProduct.Package = "1";
            notFoundProduct.ProductType = ProductType.Inventory;
            notFoundProduct.UoMFamily = string.Empty;
            notFoundProduct.Upc = string.Empty;
            IQueryable<InvoiceDetail> source = SortDetails.SortedDetails(invoice.Details);

            foreach (InvoiceDetail detail in source)
            {
                if (count >= 20)
                {
                    count = 0;
                    lines.Add("^XZ");
                    lines.Add("^XA");

                    startY = 0;
                }

                Product p = detail.Product;
                if (p == null)
                    p = notFoundProduct;
                int productLineOffset = 0;
                foreach (string pName in GetDetailsRowsSplitProductName1(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        double d = detail.Quantity * detail.Price;
                        double price = detail.Price;
                        double package = 1;
                        try
                        {
                            package = Convert.ToSingle(detail.Product.Package, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }
                        double units = detail.Quantity * package;
                        totalUnits += units;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startY, pName, detail.Quantity, d.ToCustomString(), price.ToCustomString()));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSecondLine], startY, pName));

                    productLineOffset++;
                    startY += font18Separation;
                }

                count += productLineOffset;

                if (!string.IsNullOrEmpty(detail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetDetailsRowsSplitProductName2(detail.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, "C: " + commentPArt));
                        startY += font18Separation;
                        count++;
                    }
                }

                if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    bool printMe = true;
                    if (!string.IsNullOrEmpty(p.NonVisibleExtraFieldsAsString))
                    {
                        var item = p.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                        if (item != null)
                            printMe = item.Item2 != "0";
                    }
                    if (printMe)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startY, p.Upc));
                            startY += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            lines.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, Product.GetFirstUpcOnly(p.Upc)));
                            startY += font36Separation;
                        }
                        count++;
                    }
                }
                startY += font18Separation + orderDetailSeparation; //a little extra space
                numberOfBoxes += Convert.ToSingle(detail.Quantity);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal1], startY, ""));
            startY += font18Separation;
            count++;

            if (count >= 15)
            {
                count = 0;
                lines.Add("^XZ");
                lines.Add("^XA");

                startY = 0;
            }

            string s;

            s = "Total:" + invoice.Amount.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal2], startY, s));
            startY += font36Separation;

            if (invoice.Balance > 0)
            {
                s = "Open:" + invoice.Balance.ToCustomString();
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, WidthForBoldFont - s.Length) + s;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal2], startY, s));
                startY += font36Separation;
            }
            startY += font18Separation;

            s = "Qty Items:" + numberOfBoxes.ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal3], startY, s));
            startY += font36Separation;

            s = "Qty Units:" + totalUnits.ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal4], startY, s));
            startY += font36Separation;

            //extra space to cut
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");

            StringBuilder sb = new StringBuilder();
            foreach (string s2 in lines)
                sb.Append(s2);

            try
            {
                string s3 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s3);
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

        #endregion

        #region Print Order (working)

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();
            var rItems = order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.RelatedOrderDetail).ToList();
            rItems.AddRange(order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.OrderDetailId));

            foreach (var od in order.Details)
            {
                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + "-" + (string.IsNullOrEmpty(od.Lot) ? "" : od.Lot);
                if (!Config.GroupLinesWhenPrinting || (!Config.GroupRelatedWhenPrinting && rItems.Contains(od.OrderDetailId)))
                    key = Guid.NewGuid().ToString();
                Dictionary<string, OrderLine> currentDic;
                if (!od.IsCredit)
                    currentDic = salesLines;
                else
                {
                    if (od.Damaged)
                        currentDic = creditLines;
                    else
                        currentDic = returnsLines;
                }
                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od, ParticipatingDetails = new List<OrderDetail>() });
                currentDic[key].Qty = currentDic[key].Qty + od.Qty;
                currentDic[key].ParticipatingDetails.Add(od);
            }

            List<string> lines = new List<string>();

            int startY = 40;

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startY += Config.CompanyLogoHeight;
            }

            startY += 36;

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            lines.AddRange(GetHeaderRowsInOneDoc(ref startY, asPreOrder, order, order.Client, printedId, payments, payments != null && payments.Sum(x => x.Amount) == balance));

            string docName = "NOT AN INVOICE";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "NOT A BILL";
                }
            }

            if (asPreOrder && !Config.FakePreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
            }
            else
            {
                if (Config.PrintCopy)
                {
                    string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                    startY += font36Separation;
                }
            }

            lines.Add("^XZ");
            lines.Add("^XA");
            startY = 0;

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));
            startY += 36;

            lines.Add("^XZ");
            lines.Add("^XA");
            startY = 0;

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));

            if (asPreOrder && !Config.FakePreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
            }

            // add the signature
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));

                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

                startY += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startY, order.SignatureName ?? string.Empty));
                    startY += font18Separation;
                }
                startY += font18Separation;
                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomTextSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;

                lines.AddRange(GetFooterRows(ref startY, asPreOrder));
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));


            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, "^XA"));


            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                DateTime st = DateTime.Now;
                string s = sb.ToString();
                PrintIt(s);

                // Logger.CreateLog("PrintIt(s) took: " + DateTime.Now.Subtract(st).TotalSeconds);
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }

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

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeader];
            if (Config.HideTotalInPrintedLine)
            {
                formatString = formatString.Replace("PRICE", "");
                // formatString = formatString.Replace("TOTAL", "");
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
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
                var lines = SortDetails.SortedDetails(sales.Values.ToList());
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
                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, "SALES SECTION", factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startY = 0;

                var lines = SortDetails.SortedDetails(credit.Values.ToList());
                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "DUMP SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startY = 0;

                var lines = SortDetails.SortedDetails(returns.Values.ToList());
                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "RETURNS SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            int count = 0;

            foreach (var detail in lines)
            {
                if (count >= 20)
                {
                    count = 0;
                    list.Add("^XZ");
                    list.Add("^XA");
                    startIndex = 0;
                }

                Product p = detail.Product;

                string uomString = null;
                if (detail.OrderDetail.UnitOfMeasure != null)
                {
                    uomString = detail.OrderDetail.UnitOfMeasure.Name;
                    if (!uomMap.ContainsKey(uomString))
                        uomMap.Add(uomString, 0);
                    uomMap[uomString] += detail.Qty;
                }
                else
                {
                    totalQtyNoUoM += detail.Qty;
                    try
                    {
                        totalUnits += detail.Qty * Convert.ToInt32(detail.OrderDetail.Product.Package);
                    }
                    catch { }
                }

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;

                var productSlices = GetDetailsRowsSplitProductName1(name);
                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;
                            // d += double.Parse(Math.Round(_.Price * factor * qty, 4).ToCustomString(), NumberStyles.Currency);
                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);

                        }
                        // anderson crap

                        double price = detail.Price * factor;

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HideTotalInPrintedLine)
                            priceAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLine], startIndex, pName, qtyAsString, totalAsString, priceAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSecondLine], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                count += productLineOffset;

                if (!string.IsNullOrEmpty(detail.OrderDetail.Lot))
                {
                    if (preOrder)
                    {
                        if (Config.PrintLotPreOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
                        }
                    }
                    else
                    {
                        if (Config.PrintLotOrder)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex, "Lot: " + detail.OrderDetail.Lot));
                            startIndex += font18Separation;
                        }
                    }
                    count++;
                }

                // anderson crap
                // the retail price
                var extraProperties = order.Client.ExtraProperties;
                if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                {
                    var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                    if (retailPrice != null)
                    {
                        string retPriceString = "Retail price                                   " + Convert.ToDouble(retailPrice.Item2).ToCustomString();
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, retPriceString));
                        startIndex += font18Separation;
                        count++;
                    }
                }

                if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    bool printUpc = true;
                    if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString))
                    {
                        var item = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                        if (item != null && item.Item2 == "0")
                            printUpc = false;
                    }
                    if (printUpc)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            startIndex += font18Separation / 2;

                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(p.Upc)));
                            startIndex += font36Separation * 2;
                        }
                        count++;
                    }
                }
                if (!string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                        count++;
                    }
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (uomMap.Keys.Count > 0)
            {
                if (totalQtyNoUoM > 0)
                    uomMap.Add(string.Empty, totalQtyNoUoM);
                uomMap.Add("Totals:", uomMap.Values.Sum(x => x));
            }
            else
            {
                uomMap.Add("Totals:", totalQtyNoUoM);
                if (totalUnits != totalQtyNoUoM)
                    uomMap.Add("Units:", totalUnits);
            }

            var uomKeys = uomMap.Keys.ToList();
            if (!Config.HideTotalOrder && t == null)
            {
                var key = uomKeys[0];
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, uomMap[key], balance.ToCustomString()));
                startIndex += font18Separation;
                uomKeys.Remove(key);
            }
            if (uomKeys.Count > 0)
            {
                foreach (var key in uomKeys)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsSectionFooter], startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty));
                    startIndex += font18Separation;
                }
            }
            return list;
        }

        protected override IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 29, 29);
        }

        #endregion

        #region InventorySettlement (working)

        public override bool InventorySettlement(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderTitle1], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation;

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataAccess.ExtendedSendTheLeftOverInventory();
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

            int _count = lines.Count;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (_count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    _count = 0;
                    startY = 0;
                }

                if (p.Adj == 0 && p.BegInv == 0 && p.Dump == 0 && p.EndInventory == 0 && p.LoadOut == 0 && p.Unload == 0 && p.Sales == 0 && p.TransferOn == 0 && p.TransferOff == 0)
                    continue;
                if (Config.ShortInventorySettlement && string.IsNullOrEmpty(p.OverShort) && p.TransferOn == 0 && p.TransferOff == 0 && p.Adj == 0)
                    continue;
                int productLineOffset = 0;

                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        var newS = string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, pName,
                                                 Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((p.CreditDump + p.CreditReturns), Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Dump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                 p.OverShort,
                                                Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture));
                        lines.Add(newS);
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, pName, "", "", "", "", "", "", "", "", "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }

                _count += productLineOffset;
            }

            lines.Add("^XZ");
            lines.Add("^XA");
            _count = 0;
            startY = 0;

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementDetailRow], startY, "Totals:",
                                                Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round((totalRow.CreditDump + totalRow.CreditReturns), Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Dump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                totalRow.OverShort,
                                                   Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture)));

            Config.Round = oldRound;
            startY += font18Separation;
            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            _count += 5;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            _count += 5;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            //extra space to cut
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            lines.Insert(0, "^XA");
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

        protected override IList<string> GetSetInventoryDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 20, 20);
        }

        #endregion

        #region Summary (notworking)

        public override bool InventorySummary(int index, int count, bool isBase = true)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderTitle1], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation;

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader1], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailsHeader2], startY));
            startY += font18Separation;

            InventorySettlementRow totalRow = new InventorySettlementRow();

            var map = DataAccess.ExtendedSendTheLeftOverInventory(false,true);
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

            int _count = lines.Count;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (_count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    _count = 0;
                    startY = 0;
                }

                if (Math.Round(p.EndInventory, Config.Round) == 0)
                    continue;

                int productLineOffset = 0;

                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        var newS = string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, pName,
                                                  pName,
                                                p.Lot,
                                                p.UoM != null ? p.UoM.OriginalId : string.Empty,
                                                Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut + p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture));
                        lines.Add(newS);
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, pName, "", "", "", "", "", "", "", "", "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }

                _count += productLineOffset;
            }

            lines.Add("^XZ");
            lines.Add("^XA");
            _count = 0;
            startY = 0;

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryDetailRow], startY, "Totals:",
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                    Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.LoadOut + totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)));

            Config.Round = oldRound;
            startY += font18Separation;
            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            _count += 5;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            _count += 5;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            //extra space to cut
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            lines.Insert(0, "^XA");
            sb = new StringBuilder();

            foreach (string s1 in lines)
                sb.Append(s1);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                Logger.CreateLog("Starting printing inventory summary");
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


        #endregion

        #region Orders Created Report (working)

        public override bool PrintOrdersCreatedReport(int index, int count)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderTitle1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            if (Config.UseClockInOut)
            {
                #region Deprecated

                //DateTime startOfDay = Config.FirstDayClockIn;
                //TimeSpan tsio = Config.WorkDay;
                //DateTime lastClockOut = Config.DayClockOut;
                //var wholeday = lastClockOut.Subtract(startOfDay);
                //var rested = wholeday.Subtract(tsio);

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), tsio.Hours, tsio.Minutes));
                //startY += font18Separation;

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport2], startY, rested.Hours, rested.Minutes));
                //startY += font18Separation;

                #endregion

                DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                var wholeday = lastClockOut.Subtract(startOfDay);
                var breaks = SalesmanSession.GetTotalBreaks();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), wholeday.Hours, wholeday.Minutes));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDayReport2], startY, breaks.Hours, breaks.Minutes));
                startY += font18Separation;
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsHeader1], startY));
            startY += font18Separation;

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotal = 0;
            double chargeTotal = 0;

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:

                        if (order.Client.ExtraProperties != null)
                        {
                            var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                            if (termsExtra != null)
                            {
                                var terms = termsExtra.Item2.ToUpperInvariant();
                                if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT" || terms == "CASH ON DELIVERY")
                                    cashTotal += order.OrderTotalCost();
                                else
                                    chargeTotal += order.OrderTotalCost();
                            }
                            else
                                chargeTotal += order.OrderTotalCost();
                        }
                        else
                            chargeTotal += order.OrderTotalCost();
                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:

                        if (order.Client.ExtraProperties != null)
                        {
                            var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                            if (termsExtra != null)
                            {
                                var terms = termsExtra.Item2.ToUpperInvariant();
                                if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT" || terms == "CASH ON DELIVERY")
                                    cashTotal += order.OrderTotalCost();
                                else
                                    chargeTotal += order.OrderTotalCost();
                            }
                            else
                                chargeTotal += order.OrderTotalCost();
                        }
                        else
                            chargeTotal += order.OrderTotalCost();
                        break;
                    case OrderType.Return:

                        if (order.Client.ExtraProperties != null)
                        {
                            var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                            if (termsExtra != null)
                            {
                                var terms = termsExtra.Item2.ToUpperInvariant();
                                if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT" || terms == "CASH ON DELIVERY")
                                    cashTotal += order.OrderTotalCost();
                                else
                                    chargeTotal += order.OrderTotalCost();
                            }
                            else
                                chargeTotal += order.OrderTotalCost();
                        }
                        else
                            chargeTotal += order.OrderTotalCost();
                        break;
                }

            int _count = lines.Count;

            foreach (var b in Batch.List.OrderBy(x => x.ClockedIn))
                foreach (var p in b.Orders())
                {
                    if (_count >= 20)
                    {
                        lines.Add("^XZ");
                        lines.Add("^XA");
                        _count = 0;
                        startY = 0;
                    }

                    int productLineOffset = 0;
                    foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
                            string status = string.Empty;
                            if (p.OrderType == OrderType.NoService)
                                status = "NS";
                            if (p.Voided)
                                status = "VD";
                            if (p.Reshipped)
                                status = "RS";

                            if (p.OrderType == OrderType.Bill)
                                status = "Bi";
                            string type = string.Empty;

                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY,
                                                    pName,
                                                    status,
                                                    p.PrintedOrderId,
                                                    p.OrderTotalCost().ToCustomString(),
                                                    type));

                        }
                        else
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow1], startY, pName, "", "", "", "", ""));
                        productLineOffset++;
                        startY += font18Separation;
                    }

                    _count += productLineOffset;

                    startY += font18Separation;
                    //startY += font18Separation + orderDetailSeparation;
                    var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);
                    string s = string.Format("Clock In: {0}    Clock Out: {1}     # Copies: {2}", batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies);

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow2], startY, s));
                    startY += font18Separation;

                    if (p.OrderType == OrderType.NoService && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterDetailsRow2], startY, "NS Comment:" + p.Comments));
                        startY += font18Separation;
                    }

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;
                    if (p.Voided)
                        voided++;
                    if (p.Reshipped)
                        reshipped++;
                    if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                        delivered++;
                    else
                        dsd++;

                    startY += font18Separation / 2;
                }

            lines.Add("^XZ");
            lines.Add("^XA");
            _count = 0;
            startY = 0;

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterTotalRow], startY, "Totals:", (cashTotal + chargeTotal).ToCustomString()));
            startY += font18Separation;

            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Cash Cust:   ", cashTotal.ToCustomString(), "Voided:      ", voided.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Charge Cust: ", chargeTotal.ToCustomString(), "Delivery:    ", delivered.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", "P&P:         ", dsd.ToString()));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "", "", "Reshipped:   ", reshipped.ToString()));
            startY += font18Separation;

            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesRegisterBottomSectionRow], startY, "Totals:      ", (cashTotal + chargeTotal).ToCustomString(), "Time (Hours):", totalTime));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, string.Empty));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            //extra space to cut
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
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

        #endregion

        #region Load Order (working)

        public override bool PrintOrderLoad(bool isFinal)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderTitle1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (LoadOrder.Date.Year > DateTime.MinValue.Year)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderPrintedDate], startY, LoadOrder.Date));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;
            float dumpBoxes = 0;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinalLine], startY));
                startY += font36Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsHeader1], startY));
            startY += font18Separation;

            int count = lines.Count;

            foreach (var p in LoadOrder.List)
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsLine], startY, pName, p.Qty.ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsLine], startY, pName, "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                dumpBoxes += Convert.ToSingle(p.Qty);
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderDetailsFooter], startY, dumpBoxes.ToString(CultureInfo.CurrentCulture)));
            startY += font18Separation;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinalLine], startY));
                startY += font36Separation;
            }

            //extra space to cut
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));


            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
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

        #endregion

        #region Route Return (revisar)

        public override bool PrintRouteReturn(IEnumerable<RouteReturnLine> sortedList, bool isFinal)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderTitle1], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;
            float returnBoxes = 0;
            float dumpBoxes = 0;
            float damagedBoxes = 0;

            if (!isFinal)
            {
                //para dividir el label cuando se imprime con printer de 3
                string notFinal = "NOT A FINAL Route Return";
                foreach (var item in GetLabelNotAFinalRouteReturn(notFinal))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY, item));
                    startY += font36Separation;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsHeader1], startY));
            startY += font18Separation;

            int count = lines.Count;

            foreach (var p in sortedList)
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsLine], startY, pName, Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(p.Dumps, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsLine], startY, pName, "", "", ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                returnBoxes += Convert.ToSingle(p.Unload);
                dumpBoxes += Convert.ToSingle(p.Dumps);
                damagedBoxes += Convert.ToSingle(p.DamagedInTruck);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsDetailsFooter], startY, Math.Round(returnBoxes, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(dumpBoxes, Config.Round).ToString(CultureInfo.CurrentCulture), Math.Round(damagedBoxes, Config.Round).ToString(CultureInfo.CurrentCulture)));
            startY += font18Separation;

            if (!isFinal)
            {
                //para dividir el label cuando se imprime con printer de 3
                string notFinal = "NOT A FINAL Route Return";
                foreach (var item in GetLabelNotAFinalRouteReturn(notFinal))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY, item));
                    startY += font36Separation;
                }
            }

            //space
            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
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

        #endregion

        #region Transfer (revisar)

        public override bool PrintTransferOnOff(IEnumerable<InventoryLine> sortedList, bool isOn, bool isFinal, string comment = "", string siteName = "")
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderTitle1], startY, isOn ? "On" : "Off"));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Date: ", DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            startY += font18Separation;
            float numberOfBoxes = 0;
            double value = 0.0;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffNotFinalLine], startY));
                startY += font36Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsHeader1], startY, isOn ? "Transferred On" : "Transferred Off"));
            startY += font18Separation;

            int count = lines.Count;

            foreach (var p in sortedList)
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetTransferOnOffSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsLine], startY, pName, Math.Round(p.Real, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffDetailsLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrdersCreatedReportDetailUPCLine], startY, "List Price: " + p.Product.PriceLevel0.ToCustomString()));
                startY += font18Separation;

                count++;

                if (Config.PrintUPC)
                    if (p.Product.Upc.Trim().Length > 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrdersCreatedReportDetailUPCLine], startY, "UPC: " + p.Product.Upc));
                        startY += font18Separation;
                        count++;
                    }

                numberOfBoxes += Convert.ToSingle(p.Real);
                value += p.Real * p.Product.PriceLevel0;
                startY += font18Separation + orderDetailSeparation;
            }

            lines.Add("^XZ");
            lines.Add("^XA");
            startY = 0;

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal3], startY, s));
            startY += font36Separation;

            s = "Transfer Value:" + value.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TotalValueFooter], startY, s));
            startY += font36Separation;

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffNotFinalLine], startY));
                startY += font36Separation;
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterDriverSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnOffFooterCheckerSignatureText], startY, string.Empty));
            startY += font36Separation;

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));


            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
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

        #endregion

        #region Shortage Report (revisar)

        protected override void PrintShortageReport(Order order)
        {
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            List<string> lines = new List<string>();

            int startY = 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FDKNOWN SHORTAGE REPORT^FS", startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO400,{0}^AFN,18,10^FDDate: {1}^FS", startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle3], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTo], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], order.Client.ClientName, startY));
            startY += font36Separation;

            foreach (string s in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s.Trim(), startY));
                startY += font18Separation;
            }
            startY += font36Separation;

            if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, printedId, "Invoice"));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditHeaderTitle2], startY, printedId));
            startY += font36Separation + font18Separation;

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle25], startY, order.PONumber));
                startY += font36Separation;
            }
            // add the details

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^AFN,18,10^FDPRODUCT^FS^FO500,{0}^AFN,18,10^FDPO Qty^FS^FO600,{0}^AFN,18,10^FDShort.^FS^FO710,{0}^AFN,18,10^FDDel.^FS", startY));
            startY += font18Separation;

            int count = lines.Count;

            foreach (var detail in order.Details)
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }

                if (detail.Ordered != 0 && detail.Ordered != detail.Qty)
                {
                    var p = GetDetailsRowsSplitProductName1(detail.Product.Name);
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO500,{0}^AFN,18,10^FD{2}^FS^FS^FO600,{0}^AFN,18,10^FD{3}^FS^FO710,{0}^AFN,18,10^FD{4}^FS",
                                            startY, p[0], detail.Ordered, (detail.Ordered - detail.Qty), detail.Qty));
                    startY += font18Separation;
                    count++;
                }
            }

            foreach (var detail in order.DeletedDetails)
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }

                if (detail.Ordered != 0)
                {
                    var p = GetDetailsRowsSplitProductName1(detail.Product.Name);
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^AFN,18,10^FD{1}^FS^FO500,{0}^AFN,18,10^FD{2}^FS^FS^FO600,{0}^AFN,18,10^FD{3}^FS^FO710,{0}^AFN,18,10^FD{4}^FS",
                                            startY, p[0], detail.Ordered, detail.Ordered, 0));
                    startY += font18Separation;
                    count++;
                }
            }

            if (count >= 20)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                count = 0;
                startY = 0;
            }

            // add the signature
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                startY += font18Separation;

                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startY, order.SignatureName ?? string.Empty));
                    startY += font18Separation;
                }
                startY += font18Separation;

                count += 4;

                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomTextSplitText())
                    {

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
                foreach (string s in GetFooterRows(ref startY, false))
                {
                    if (count >= 20)
                    {
                        lines.Add("^XZ");
                        lines.Add("^XA");
                        count = 0;
                        startY = 0;
                    }

                    lines.Add(s);
                    count++;
                }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                string s = sb.ToString();
                PrintIt(s);
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
            }
        }

        #endregion

        #region Consignment (revisar)

        private bool PrintConsignment(Order order, bool asPreOrder, bool counting)
        {
            StringBuilder sb;
            List<string> lines = new List<string>();

            int startY = 80;

            string title = "Consignment Invoice";
            if (!counting)
                title = "Consignment Contract";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderTitle1], startY, title));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDriverNameText], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentHeaderDriverNameText], startY, "Driver Name: ", Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientName], clientSplit, startY));
                startY += font36Separation;
            }

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderClientAddr], s1.Trim(), startY));
                startY += font18Separation;

            }
            startY += font36Separation;

            string docName = "";
            if (counting)
                docName = "NOT A FINAL INVOICE";

            if (asPreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, docName));
                startY += font36Separation;
            }
            else
            {
                if (Config.PrintCopy)
                {
                    string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PreOrderHeaderTitle3], startY, name));
                    startY += font36Separation;
                }
            }
            if (counting)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderTitle2], startY, order.PrintedOrderId, "Invoice"));
                startY += font36Separation;
            }

            startY += font36Separation;

            int factor = order.OrderType == OrderType.Consignment ? 1 : -1;

            lines.Add("^XZ");
            lines.Add("^XA");
            startY = 0;

            float totalQty = 0;

            lines.AddRange(GetConsignmentDetailsRows(ref startY, ref totalQty, order, counting));

            startY += font36Separation;

            lines.Add("^XZ");
            lines.Add("^XA");
            startY = 0;

            int count = 0;

            if (counting)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsTotalLine], startY, "Totals:",
                    totalQty));

                startY += font36Separation;
                string s;

                s = "     SALES:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                var s1 = order.OrderTotalCost().ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                s = "NET AMOUNT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = order.OrderTotalCost().ToCustomString();
                s1 = new string(' ', 14 - s1.Length) + s1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                count += 3;

                double tax = order.CalculateTax();
                if (tax > 0)
                {
                    s = " SALES TAX:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = tax.ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                    count++;
                }
                if (order.DiscountAmount > 0)
                {
                    s = "DISCOUNT:";
                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = order.CalculateDiscount().ToCustomString();
                    s1 = new string(' ', 14 - s1.Length) + s1;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                    startY += font36Separation;
                    count++;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startY, " C: " + order.DiscountComment));
                    startY += font18Separation;
                    count++;
                }

                // right justified
                s = "TOTAL DUE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = order.OrderTotalCost().ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                double paid = 0;
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment != null)
                {
                    var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                    paid = parts.Sum(x => x.Amount);
                }

                s = "TOTAL PAYMENT:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = paid.ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                s = "CURRENT BALANCE:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = (order.OrderTotalCost() - paid).ToCustomString();
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal13], startY, s + s1));
                startY += font36Separation;

                count += 3;
            }
            else
            {
                var oldTotal = order.Details.Sum(x => x.ConsignmentOld);
                var newTotal = order.Details.Sum(x => x.ConsignmentUpdated ? x.ConsignmentNew : x.ConsignmentOld);
                var newValue = order.Details.Sum(x => (x.ConsignmentUpdated ? x.ConsignmentNew : x.ConsignmentOld) * x.ConsignmentNewPrice);

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractTotalLine], startY, "Totals:",
                    newTotal.ToString(CultureInfo.CurrentCulture), string.Empty, newValue.ToCustomString()));
                startY += font18Separation;
                count++;
            }

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                startY += font18Separation;

                count += 3;

                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startY, order.SignatureName ?? string.Empty));
                    startY += font18Separation;
                    count++;
                }

                startY += font18Separation;
                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    if (count >= 15)
                    {
                        lines.Add("^XZ");
                        lines.Add("^XA");
                        count = 0;
                        startY = 0;
                    }
                    foreach (var line in GetBottomTextSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
            {
                if (count >= 15)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startY = 0;
                }
                lines.AddRange(GetFooterRows(ref startY, asPreOrder));
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
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

        protected override IList<string> GetConsignmentDetailsRows(ref int startIndex, ref float totalQty, Order order, bool counting)
        {
            var lines = new List<string>();

            lines.AddRange(GetConsignmentHeaderRows(ref startIndex, counting));
            startIndex += font18Separation;

            lines.Add("^XZ");
            lines.Add("^XA");

            double sold = 0;
            double added = 0;
            double newConsignment = 0;

            int count = 0;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    count = 0;
                    startIndex = 0;
                }

                if (counting && !detail.ConsignmentCounted)
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
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, productNamePart,
                                                    detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture),
                                                     detail.ConsignmentCount.ToString(CultureInfo.CurrentCulture),
                                                    detail.Qty.ToString(CultureInfo.CurrentCulture),
                                                    detail.Price.ToCustomString(),
                                                    (detail.Price * detail.Qty).ToCustomString()
                                                    ));
                        else
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsContractLine], startIndex, productNamePart,
                                                    (detail.ConsignmentUpdated ? detail.ConsignmentNew : detail.ConsignmentOld).ToString(CultureInfo.CurrentCulture),
                                                    detail.ConsignmentNewPrice.ToCustomString(),
                                                    (detail.ConsignmentNewPrice * (detail.ConsignmentUpdated ? detail.ConsignmentNew : detail.ConsignmentOld)).ToCustomString()));
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
                count += productSlices.Count;

                if (!string.IsNullOrEmpty(detail.Lot))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentDetailsLine], startIndex, "Lot: " + detail.Lot,
                        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
                    startIndex += font18Separation;
                    count++;
                }

                if (detail.Product.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    string upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[ConsignmentDetailsLineUPC];

                    lines.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(detail.Product.Upc)));
                    startIndex += font36Separation + font18Separation;
                    count++;
                }
            }

            return lines;
        }

        #endregion

        #region Sales Credit Report

        public override bool PrintSalesCreditReport()
        {
            // calculation
            Dictionary<int, Pair<Product, double>> sales = new Dictionary<int, Pair<Product, double>>();
            Dictionary<int, Pair<Product, double>> credits = new Dictionary<int, Pair<Product, double>>();
            Dictionary<int, Pair<Product, double>> returns = new Dictionary<int, Pair<Product, double>>();

            foreach (var order in Order.Orders.Where(x => !x.Voided))
                switch (order.OrderType)
                {
                    case OrderType.Credit:
                        foreach (var detail in order.Details)
                        {
                            var dictionary = detail.Damaged ? credits : returns;
                            if (dictionary.ContainsKey(detail.Product.ProductId))
                                dictionary[detail.Product.ProductId].Item2 = dictionary[detail.Product.ProductId].Item2 + detail.Qty;
                            else
                                dictionary.Add(detail.Product.ProductId, new Pair<Product, double>(detail.Product, detail.Qty));
                        }
                        break;
                    case OrderType.Order:
                        foreach (var detail in order.Details)
                            if (sales.ContainsKey(detail.Product.ProductId))
                                sales[detail.Product.ProductId].Item2 = sales[detail.Product.ProductId].Item2 + detail.Qty;
                            else
                                sales.Add(detail.Product.ProductId, new Pair<Product, double>(detail.Product, detail.Qty));
                        break;
                }


            int startY = 40;
            List<string> lines = new List<string>();

            // report header
            startY += 10;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportHeaderTitle], startY, DateTime.Now.ToString()));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesman], startY, "Route #: ", Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesman], startY, "Driver Name: ", Config.VendorName));
            startY += font36Separation + 10;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line
                                        // The sales section
                                        // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesHeaderTitle], startY));
            startY += font36Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailsHeader1], startY));
            startY += font18Separation;

            int count = lines.Count;

            // print the lines
            double total = 0;
            foreach (var item in sales.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    startY = 0;
                    count = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName1(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                startY += font18Separation;
                total += item.Item2;
            }

            var s = "Qty sold:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportSalesFooter], startY, s));
            startY += font18Separation * 3;

            // The credit section
            // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditHeaderTitle], startY));
            startY += font36Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailsHeader1], startY));
            startY += font18Separation;
            total = 0;

            // print the lines
            foreach (var item in credits.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    startY = 0;
                    count = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName2(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                startY += font18Separation;
                total += item.Item2;
            }

            // print the footer
            s = "Qty dumped:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportCreditFooter], startY, s));
            startY += font18Separation * 3;

            // The return section
            // the header
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnHeaderTitle], startY));
            startY += font36Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailsHeader1], startY));
            startY += font18Separation;
            total = 0;

            // print the lines
            foreach (var item in returns.Values.OrderBy(x => x.Item1.OrderedBasedOnConfig()))
            {
                if (count >= 20)
                {
                    lines.Add("^XZ");
                    lines.Add("^XA");
                    startY = 0;
                    count = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in PrintSalesCreditReportSplitProductName2(item.Item1.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailLine], startY, pName, item.Item2));
                    }
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnDetailLine], startY, pName, ""));
                    productLineOffset++;
                    startY += font18Separation;
                }
                count += productLineOffset;

                startY += font18Separation;
                total += item.Item2;
            }

            // print the footer
            s = "Qty returned:" + total.ToString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SalesReportReturnFooter], startY, s));
            startY += font18Separation;

            if (count >= 15)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;
            }
            lines.AddRange(GetInventoryCheckFooterRows(ref startY));

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");
            StringBuilder sb = new StringBuilder();
            foreach (string ss in lines)
                sb.Append(ss);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        #endregion

        #region Inventory

        public override bool PrintInventory(IEnumerable<Product> SortedList)
        {
            int startY = 40;

            List<string> lines = new List<string>();
            foreach (string s in GetInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }

            if (lines.Count >= 8)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;
            }

            foreach (string s in GetInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }
            foreach (string s in GetFooterRows(ref startY, false))
            {
                lines.Add(s);
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected override IEnumerable<string> GetInventoryDetailsRows(ref int startIndex, IEnumerable<Product> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxes = 0;
            double value = 0;

            int count = 1;

            foreach (Product p in SortedList)
            {
                if (count >= 20)
                {
                    list.Add("^XZ");
                    list.Add("^XA");
                    count = 0;
                    startIndex = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetInventoryDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName, Math.Round(p.BeginigInventory, 2).ToString(CultureInfo.CurrentCulture), Math.Round(p.CurrentInventory, 2).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryDetailsLine], startIndex, pName, "", ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                count += productLineOffset;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryPriceLine], startIndex, string.Format(CultureInfo.InvariantCulture, "List Price: {0}  Total: {1}", p.PriceLevel0.ToCustomString(), (p.CurrentInventory * p.PriceLevel0).ToCustomString())));
                startIndex += font18Separation;
                count++;

                if (Config.PrintUPCInventory)
                {
                    if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(p.Upc)));
                            startIndex += font36Separation;
                        }
                        count++;
                    }
                }

                numberOfBoxes += Convert.ToSingle(p.CurrentInventory);
                value += p.CurrentInventory * p.PriceLevel0;
                startIndex += font18Separation + orderDetailSeparation;
            }

            if (count >= 10)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startIndex = 0;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal3], startIndex, s));
            startIndex += font36Separation;

            s = "Inv. Value:" + value.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TotalValueFooter], startIndex, s));
            startIndex += font36Separation;

            return list;
        }

        public override bool PrintSetInventory(IEnumerable<InventoryLine> SortedList)
        {
            int startY = 40;

            List<string> lines = new List<string>();
            foreach (string s in GetSetInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }

            if (lines.Count >= 8)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;
            }

            foreach (string s in GetSetInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }

            foreach (string s in GetFooterRows(ref startY, false))
            {
                lines.Add(s);
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected override IEnumerable<string> GetSetInventoryDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxes = 0;
            double value = 0.0;

            int count = 1;

            foreach (var p in SortedList)
            {
                if (count >= 20)
                {
                    list.Add("^XZ");
                    list.Add("^XA");
                    count = 0;
                    startIndex = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetSetInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsLine], startIndex, pName, Math.Round(p.Real, 2).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[SetInventoryDetailsLine], startIndex, pName, ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                count += productLineOffset;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryPriceLine], startIndex, string.Format(CultureInfo.InvariantCulture, "List Price: {0}  Total: {1}", p.Product.PriceLevel0.ToCustomString(), (p.Product.CurrentInventory * p.Product.PriceLevel0).ToCustomString())));
                startIndex += font18Separation;
                count++;

                if (Config.PrintUPCInventory)
                {
                    if (p.Product.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPCText], startIndex, p.Product.Upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            var upcTemp = Config.UseUpc128 ? linesTemplates[UPC128] : linesTemplates[OrderDetailsLineUPC];

                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineUPC], startIndex, Product.GetFirstUpcOnly(p.Product.Upc)));
                            startIndex += font36Separation;
                        }
                    }
                    count++;
                }
                numberOfBoxes += Convert.ToSingle(p.Real);
                value += p.Real * p.Product.PriceLevel0;
                startIndex += font18Separation + orderDetailSeparation;
            }

            if (count >= 10)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startIndex = 0;
            }

            var s = "Qty Items:" + Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal3], startIndex, s));
            startIndex += font36Separation;

            s = "Inv. Value:" + value.ToCustomString();
            if (WidthForBoldFont - s.Length > 0)
                s = new string((char)32, WidthForBoldFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TotalValueFooter], startIndex, s));
            startIndex += font36Separation;
            return list;
        }

        public override bool PrintAddInventory(IEnumerable<InventoryLine> SortedList, bool final)
        {
            int startY = 40;

            List<string> lines = new List<string>();
            foreach (string s in GetAddInventoryHeaderRows(ref startY))
            {
                lines.Add(s);
            }
            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }

            if (lines.Count >= 8)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;
            }

            foreach (string s in GetAddInventoryDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryNotFinal], startY));
                startY += font36Separation;
            }

            foreach (string s in GetFooterRows(ref startY, false))
            {
                lines.Add(s);
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");


            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected override IEnumerable<string> GetAddInventoryDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;

            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsHeader2], startIndex));
            startIndex += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsHeader21], startIndex));
            startIndex += font18Separation;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;

            int count = list.Count;

            foreach (var p in SortedList)
            {
                if (count >= 20)
                {
                    list.Add("^XZ");
                    list.Add("^XA");
                    count = 0;
                    startIndex = 0;
                }

                int productLineOffset = 0;
                foreach (string pName in GetAddInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName,
                                               Math.Round(p.Product.BeginigInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round(p.Product.RequestedLoadInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round((p.Real - p.Product.RequestedLoadInventory), Config.Round).ToString(CultureInfo.CurrentCulture),
                                               Math.Round((p.Product.BeginigInventory + p.Real), Config.Round).ToString(CultureInfo.CurrentCulture)
                        ));

                        leftFromYesterday += p.Product.BeginigInventory;
                        requestedInventory += p.Product.RequestedLoadInventory;
                        adjustment += (p.Real - p.Product.RequestedLoadInventory);
                        start += (p.Product.BeginigInventory + p.Real);
                    }
                    else
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, pName, "", "", "", ""));
                    }

                    productLineOffset++;
                    startIndex += font18Separation;
                }

                count += productLineOffset;
            }

            if (count >= 10)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startIndex = 0;
            }

            //startIndex += font18Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDetailsLine2], startIndex, "Totals:",
                 Math.Round(leftFromYesterday, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));
            startIndex += font18Separation;

            return list;
        }

        public override bool PrintInventoryCheck(IEnumerable<InventoryLine> SortedList)
        {
            int startY = 40;

            List<string> lines = new List<string>();
            foreach (string s in GetInventoryCheckHeaderRows(ref startY))
            {
                lines.Add(s);
            }

            if (lines.Count >= 8)
            {
                lines.Add("^XZ");
                lines.Add("^XA");
                startY = 0;
            }

            foreach (string s in GetInventoryCheckDetailsRows(ref startY, SortedList))
            {
                lines.Add(s);
            }

            foreach (string s in GetInventoryCheckFooterRows(ref startY))
            {
                lines.Add(s);
            }

            startY += font36Separation;
            startY += font36Separation;
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaid], startY, "   "));

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, "^XA");


            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            try
            {
                PrintIt(sb.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected override IEnumerable<string> GetInventoryCheckDetailsRows(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();
            startIndex += 40;
            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsHeader1], startIndex));

            startIndex += font18Separation;
            float numberOfBoxesReal = 0;
            float numberOfBoxesExpected = 0;

            int count = list.Count;

            foreach (var l in SortedList)
            {
                if (count >= 20)
                {
                    list.Add("^XZ");
                    list.Add("^XA");
                    count = 0;
                    startIndex = 0;
                }

                var p = l.Product;
                int productLineOffset = 0;
                foreach (string pName in GetInventoryCheckDetailsRowsSplitProductName1(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsLine], startIndex, pName, p.CurrentInventory.ToString(CultureInfo.CurrentCulture), l.Real.ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsLine], startIndex, pName, "", ""));
                    productLineOffset++;
                    startIndex += font18Separation;
                }
                numberOfBoxesReal += Convert.ToSingle(l.Real);
                numberOfBoxesExpected += Convert.ToSingle(p.CurrentInventory);

                startIndex += font18Separation + orderDetailSeparation;

                count += productLineOffset;
            }

            if (count >= 10)
            {
                list.Add("^XZ");
                list.Add("^XA");
                startIndex = 0;
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCheckDetailsFooter], startIndex, numberOfBoxesExpected.ToString(CultureInfo.CurrentCulture), numberOfBoxesReal.ToString(CultureInfo.CurrentCulture)));
            startIndex += font36Separation;

            return list;
        }

        #endregion
    }
}
