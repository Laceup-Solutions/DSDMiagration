


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace LaceupMigration
{
    public abstract class ZebraPrinter : IPrinter
    {
        public abstract bool PrintRouteReturn(IEnumerable<RouteReturnLine> sortedList, bool isFinal);

        public abstract bool PrintOrderLoad(bool isFinal);

        public abstract bool PrintTransferOnOff(IEnumerable<InventoryLine> sortedList, bool isOn, bool isFinal, string comment = "", string siteName = "");

        public abstract bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false);

        public abstract bool PrintInventory(IEnumerable<Product> SortedList);


        public abstract bool PrintSetInventory(IEnumerable<InventoryLine> SortedList);

        public abstract bool PrintAddInventory(IEnumerable<InventoryLine> SortedList, bool final);

        public abstract bool PrintInventoryCheck(IEnumerable<InventoryLine> SortedList);

        public abstract bool PrintSalesCreditReport();

        public abstract bool PrintReceivedPaymentsReport(int index, int count);

        public abstract bool PrintOrdersCreatedReport(int index, int count);

        public abstract bool PrintPayment(InvoicePayment invoicePayment);

        public abstract bool InventorySettlement(int index, int count);

        public abstract bool InventorySummary(int index, int count, bool isBase = true);


        public abstract bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false);

        public abstract bool PrintOpenInvoice(Invoice invoice);

        public abstract bool PrintAcceptLoad(IEnumerable<InventoryLine> SortedList, string docNumber, bool final);

        public abstract bool PrintBatteryEndOfDay(int index, int count);

        public abstract bool PrintFullConsignment(Order order, bool asPreOrder);

        public abstract bool PrintClientStatement(Client client);

        public abstract bool PrintInventoryCount(List<CycleCountItem> items);

        public abstract bool PrintRefusalReport(int index, int count);

        protected const string FooterSignaturePaymentText = "FooterSignaturePaymentText";
        protected const string SalesRegisterHeaderTitle1 = "SalesRegisterHeaderTitle1";
        protected const string SalesRegisterDayReport = "SalesRegisterDayReport";
        protected const string SalesRegisterDayReport2 = "SalesRegisterDayReport2";
        protected const string SalesRegisterHeaderDate = "SalesRegisterHeaderDate";
        protected const string SalesRegisterHeaderDriverNameText = "SalesRegisterHeaderDriverNameText";
        protected const string SalesRegisterDetailsHeader1 = "SalesRegisterDetailsHeader1";
        protected const string SalesRegisterDetailsRow1 = "SalesRegisterDetailsRow1";
        protected const string SalesRegisterDetailsRow2 = "SalesRegisterDetailsRow2";
        protected const string SalesRegisterTotalRow = "SalesRegisterTotalRow";
        protected const string SalesRegisterBottomSectionRow = "SalesRegisterBottomSectionRow";

        protected const string RouteReturnsHeaderTitle1 = "RouteReturnsHeaderTitle1";
        protected const string RouteReturnsHeaderDate = "RouteReturnsHeaderDate";
        protected const string RouteReturnsNotFinalLine = "RouteReturnsNotFinalLine";
        protected const string RouteReturnsNotFinalLabel = "RouteReturnsNotFinalLabel";
        protected const string RouteReturnsHeaderDriverNameText = "RouteReturnsHeaderDriverNameText";
        protected const string RouteReturnsDetailsHeader1 = "RouteReturnsDetailsHeader1";
        protected const string RouteReturnsDetailsLine = "RouteReturnsDetailsLine";
        protected const string RouteReturnsDetailsFooter = "RouteReturnsDetailsFooter";

        protected const string LoadOrderHeaderTitle1 = "LoadOrderHeaderTitle1";
        protected const string LoadOrderHeaderDate = "LoadOrderHeaderDate";
        protected const string LoadOrderHeaderPrintedDate = "LoadOrderHeaderPrintedDate";
        protected const string LoadOrderNotFinalLine = "LoadOrderNotFinalLine";
        protected const string LoadOrderHeaderDriverNameText = "LoadOrderHeaderDriverNameText";
        protected const string LoadOrderDetailsHeader1 = "LoadOrderDetailsHeader1";
        protected const string LoadOrderDetailsLine = "LoadOrderDetailsLine";
        protected const string LoadOrderDetailsFooter = "LoadOrderDetailsFooter";

        protected const string ConsignmentHeaderTitle1 = "ConsignmentHeaderTitle1";
        protected const string ConsignmentHeaderDate = "ConsignmentHeaderDate";
        protected const string ConsignmentHeaderDriverNameText = "ConsignmentHeaderDriverNameText";
        protected const string ConsignmentDetailsHeader1 = "ConsignmentDetailsHeader1";
        protected const string ConsignmentDetailsHeader2 = "ConsignmentDetailsHeader2";
        protected const string ConsignmentDetailsHeaderContract1 = "ConsignmentDetailsHeaderContract1";
        protected const string ConsignmentDetailsHeaderContract2 = "ConsignmentDetailsHeaderContract2";
        protected const string ConsignmentDetailsContractLine = "ConsignmentDetailsContractLine";
        protected const string ConsignmentDetailsContractTotalLine = "ConsignmentDetailsContractTotalLine";

        protected const string ConsignmentDetailsLine = "ConsignmentDetailsLine";
        protected const string ConsignmentDetailsLine2 = "ConsignmentDetailsLine2";
        protected const string ConsignmentDetailsLineUPC = "ConsignmentDetailsLineUPC";
        protected const string ConsignmentDetailsFooter = "ConsignmentDetailsFooter";
        protected const string ConsignmentDetailsTotalLine = "ConsignmentDetailsTotalLine";

        protected const string InventorySettlementHeaderTitle1 = "InventorySettlementHeaderTitle1";
        protected const string InventorySettlementHeaderLabel1 = "InventorySettlementHeaderLabel1";
        protected const string InventorySettlementHeaderDate = "InventorySettlementHeaderDate";
        protected const string InventorySettlementDetailsHeader1 = "InventorySettlementDetailsHeader1";
        protected const string InventorySettlementDetailsHeader2 = "InventorySettlementDetailsHeader2";
        protected const string InventorySettlementDetailsHeader3 = "InventorySettlementDetailsHeader3";
        protected const string InventorySettlementDetailRow = "InventorySettlementDetailRow";

        protected const string InventorySummaryHeaderTitle1 = "InventorySummaryHeaderTitle1";
        protected const string InventorySummaryHeaderLabel1 = "InventorySummaryHeaderLabel1";
        protected const string InventorySummaryHeaderDate = "InventorySummaryHeaderDate";
        protected const string InventorySummaryDetailsHeader1 = "InventorySummaryDetailsHeader1";
        protected const string InventorySummaryDetailsHeader2 = "InventorySummaryDetailsHeader2";
        protected const string InventorySummaryDetailsHeader3 = "InventorySummaryDetailsHeader3";
        protected const string InventorySummaryDetailRow = "InventorySummaryDetailRow";
        protected const string InventorySummaryProductRow = "InventorySummaryProductRow";
        protected const string InventorySummaryTotalsRow = "InventorySummaryTotalsRow";



        protected const string TransferOnOffHeaderTitle1 = "TransferOnOffHeaderTitle1";
        protected const string TransferOnOffHeaderDriverNameText = "TransferOnOffHeaderDriverNameText";
        protected const string TransferOnOffDetailsHeader1 = "TransferOnOffDetailsHeader1";
        protected const string TransferOnOffDetailsLine = "TransferOnOffDetailsLine";
        protected const string TransferOnOffNotFinalLine = "TransferOnOffNotFinalLine";
        protected const string TransferOnOffFooterSignatureLine = "TransferOnOffFooterSignatureLine";
        protected const string TransferOnOffFooterDriverSignatureText = "TransferOnOffFooterDriverSignatureText";
        protected const string TransferOnOffFooterCheckerSignatureText = "TransferOnOffFooterCheckerSignatureText";

        protected const string PaymentHeaderTitle1 = "PaymentHeaderTitle1";
        protected const string PaymentHeaderTo = "PaymentHeaderTo";
        protected const string PaymentHeaderClientName = "PaymentHeaderClientName";
        protected const string PaymentHeaderClientAddr = "PaymentHeaderClientAddr";
        protected const string PaymentHeaderTitle2 = "PaymentHeaderTitle2";
        protected const string PaymentHeaderTitle3 = "PaymentHeaderTitle3";
        protected const string PaymentPaid = "PaymentPaid";

        protected Dictionary<string, string> linesTemplates = new Dictionary<string, string>();

        protected const int orderDetailSeparation = 3;
        protected int font18Separation = 25;
        protected const int font20Separation = 20;
        protected const int font36Separation = 43;

        //const string initialization = "! U1 setvar \"device.languages\" \"zpl\"";
        protected const string StartLabel = "^XA^PON^MNN^LL{0}";
        protected const string EndLabel = "^XZ";

        protected const string CreditHeaderTitle1 = "CreditHeaderTitle1";
        protected const string ReturnHeaderTitle1 = "ReturnHeaderTitle1";
        protected const string OrderHeaderTitle1 = "OrderHeaderTitle1";
        protected const string OrderHeaderTitle2 = "OrderHeaderTitle2";
        protected const string OrderHeaderTitle25 = "OrderHeaderTitle25";
        protected const string OrderHeaderTitle3 = "OrderHeaderTitle3";
        protected const string CreditHeaderTitle2 = "CreditHeaderTitle2";
        protected const string ReturnHeaderTitle2 = "ReturnHeaderTitle2";
        protected const string HeaderName = "HeaderName";

        protected const string PreOrderHeaderTitle3 = "PreOrderHeaderTitle3";
        protected const string PreOrderHeaderTitle4 = "PreOrderHeaderTitle4";
        protected const string PreOrderHeaderTitle41 = "PreOrderHeaderTitle41";

        //cia name
        protected const string HeaderAddr1 = "HeaderAddr1";
        //addr1
        protected const string HeaderAddr2 = "HeaderAddr2";
        //addr2
        protected const string HeaderPhone = "HeaderPhone";
        //phone
        protected const string OrderHeaderTo = "OrderHeaderTo";
        protected const string OrderHeaderClientName = "OrderHeaderClientName";
        protected const string OrderHeaderClientAddr = "OrderHeaderClientAddr";
        protected const string OrderHeaderSectionName = "OrderHeaderSectionName";
        protected const string OrderDetailsHeader = "OrderDetailsHeader";
        protected const string OrderDetailsHeaderSuggestedPrice = "OrderDetailsHeaderSuggestedPrice";
        protected const string OrderDetailsLine = "OrderDetailsLine";
        protected const string OrderDetailsLineSecondLine = "OrderDetailsLineSecondLine";
        protected const string OrderDetailsLineSuggestedPrice = "OrderDetailsLineSuggestedPrice";
        protected const string OrderDetailsLineUPC = "OrderDetailsLineUPC";
        protected const string OrderDetailsLineUPCText = "OrderDetailsLineUPCText";
        protected const string OrderDetailsLineLot = "OrderDetailsLineLot";
        protected const string OrderDetailsTotal1 = "OrderDetailsTotal1";
        protected const string OrderDetailsTotal14 = "OrderDetailsTotal14";
        protected const string OrderDetailsTotal13 = "OrderDetailsTotal13";
        protected const string OrderDetailsLineSeparator = "OrderDetailsLineSeparator";
        protected const string OrderDetailsTotal15 = "OrderDetailsTotal15";
        protected const string OrderDetailsTotal2 = "OrderDetailsTotal2";
        protected const string OrderDetailsTotal3 = "OrderDetailsTotal3";
        protected const string OrderDetailsTotal4 = "OrderDetailsTotal4";
        protected const string OrderPaid = "OrderPaid";
        protected const string OrderDetailsSectionFooter = "OrderDetailsSectionFooter";
        protected const string OrderDetailsSectionFooter1 = "OrderDetailsSectionFooter1";
        protected const string ExpectedTotal3 = "ExpectedTotal3";
        protected const string FooterSignatureNameText = "FooterSignatureNameText";
        protected const string FooterSignatureLine = "FooterSignatureLine";
        protected const string FooterSignatureText = "FooterSignatureText";
        protected const string FooterCheckerSignatureText = "FooterCheckerSignatureText";
        protected const string FooterSpaceSignatureText = "FooterSpaceSignatureText";

        protected const string FooterBottomText = "FooterBottomText";

        // This line is shared
        protected const string TotalValueFooter = "TotalValueFooter";
        protected const string InventoryPriceLine = "InventoryPriceLine";

        // Top header, used by ALL the inventory reports
        protected const string InventorySalesman = "InventorySalesman";

        protected const string InventoryNotFinal = "InventoryNotFinal";
        protected const string OrderCommentWork = "OrderCommentWork";

        // used for the print inventory report
        protected const string InventoryHeaderTitle = "InventoryHeaderTitle";
        protected const string InventoryDetailsHeader1 = "InventoryDetailsHeader1";
        protected const string InventoryDetailsLine = "InventoryDetailsLine";
        protected const string InventoryDetailsLineLot = "InventoryDetailsLineLot";

        // used for the check inventory report
        protected const string InventoryCheckHeaderTitle = "InventoryCheckHeaderTitle";
        protected const string InventoryCheckDetailsHeader1 = "InventoryCheckDetailsHeader1";
        protected const string InventoryCheckDetailsLine = "InventoryCheckDetailsLine";
        protected const string InventoryCheckDetailsFooter = "InventoryCheckDetailsFooter";

        // used for the Set inventory report
        protected const string SetInventoryHeaderTitle = "SetInventoryHeaderTitle";
        protected const string SetInventoryDetailsHeader1 = "SetInventoryDetailsHeader1";
        protected const string SetInventoryDetailsLine = "SetInventoryDetailsLine";

        // used for the Add inventory report
        protected const string AddInventoryHeaderTitle = "AddInventoryHeaderTitle";
        protected const string AddInventoryHeaderTitle1 = "AddInventoryHeaderTitle1";
        protected const string AddInventoryDetailsHeader1 = "AddInventoryDetailsHeader1";
        protected const string AddInventoryDetailsHeader2 = "AddInventoryDetailsHeader2";
        protected const string AddInventoryDetailsHeader21 = "AddInventoryDetailsHeader21";
        protected const string AddInventoryDetailsLine = "AddInventoryDetailsLine";
        protected const string AddInventoryDetailsLine2 = "AddInventoryDetailsLine2";

        // The sales & credit report
        protected const string SalesReportHeaderTitle = "SalesReportHeaderTitle";
        protected const string SalesReportSalesman = "SalesReportSalesman";
        // The sales section
        protected const string SalesReportSalesHeaderTitle = "SalesReportSalesHeaderTitle";
        protected const string SalesReportSalesDetailsHeader1 = "SalesReportSalesDetailsHeader1";
        protected const string SalesReportSalesDetailLine = "SalesReportSalesDetailLine";
        protected const string SalesReportSalesFooter = "SalesReportSalesFooter";
        // The credit section
        protected const string SalesReportCreditHeaderTitle = "SalesReportCreditHeaderTitle";
        protected const string SalesReportCreditDetailsHeader1 = "SalesReportCreditDetailsHeader1";
        protected const string SalesReportCreditDetailLine = "SalesReportCreditDetailLine";
        protected const string SalesReportCreditFooter = "SalesReportCreditFooter";
        // The return section
        protected const string SalesReportReturnHeaderTitle = "SalesReportReturnHeaderTitle";
        protected const string SalesReportReturnDetailsHeader1 = "SalesReportReturnDetailsHeader1";
        protected const string SalesReportReturnDetailLine = "SalesReportReturnDetailLine";
        protected const string SalesReportReturnFooter = "SalesReportReturnFooter";

        // The received payments report
        protected const string PaymentReportHeaderTitle = "PaymentReportHeaderTitle";
        protected const string PaymentReportHeaderLabel = "PaymentReportHeaderLabel";
        protected const string PaymentReportSalesman = "PaymentReportSalesman";
        protected const string PaymentReportDate = "PaymentReportDate";
        protected const string PaymentReportHeader1 = "PaymentReportHeader1";
        protected const string PaymentReportHeader2 = "PaymentReportHeader2";
        protected const string PaymentReportDetail = "PaymentReportDetail";
        protected const string PaymentReportTotal = "PaymentReportTotal";
        protected const string PaymentReportTotalReceived = "PaymentReportTotalReceived";
        // The cash received
        protected const string PaymentReportCashtHeaderTitle = "PaymentReportCashtHeaderTitle";
        protected const string PaymentReportCashDetailsHeader1 = "PaymentReportCashDetailsHeader1";
        protected const string PaymentReportCashDetailLine = "PaymentReportCashDetailLine";
        protected const string PaymentReportCashFooter = "PaymentReportCashFooter";
        protected const string PaymentReportNoCashMessage = "PaymentReportNoCashMessage";
        // The checks received
        protected const string PaymentReportChecktHeaderTitle = "PaymentReportChecktHeaderTitle";
        protected const string PaymentReportCheckDetailHeaderTitle = "PaymentReportCheckDetailHeaderTitle";
        protected const string PaymentReportCheckDetailLine = "PaymentReportCheckDetailLine";
        protected const string PaymentReportCheckFooter = "PaymentReportCheckFooter";
        protected const string PaymentReportCheckAmountFooter = "PaymentReportCheckAmountFooter";
        protected const string PaymentReportNoCheckMessage = "PaymentReportNoCheckMessage";
        protected const string PaymentReportTotalAmountFooter = "PaymentReportTotalAmountFooter";

        // The received payments report
        protected const string OrdersCreatedReportHeaderTitle = "OrdersCreatedReportHeaderTitle";
        protected const string OrdersCreatedReportSalesman = "OrdersCreatedReportSalesman";
        // The orders in the system
        protected const string OrdersCreatedReportOrderSectionHeader1 = "OrdersCreatedReportOrderSectionHeader1";
        protected const string OrdersCreatedReportCreditSectionHeader1 = "OrdersCreatedReportCreditSectionHeader1";
        protected const string OrdersCreatedReportVoidSectionHeader1 = "OrdersCreatedReportVoidSectionHeader1";
        protected const string OrdersCreatedReportReturnSectionHeader1 = "OrdersCreatedReportReturnSectionHeader1";
        protected const string OrdersCreatedReportDetailsHeader1 = "OrdersCreatedReportDetailsHeader1";
        protected const string OrdersCreatedReportDetailLine = "OrdersCreatedReportDetailLine";
        protected const string OrdersCreatedReportDetailLine1 = "OrdersCreatedReportDetailLine1";
        protected const string OrdersCreatedReportFooter = "OrdersCreatedReportFooter";
        protected const string OrdersCreatedReportFooter1 = "OrdersCreatedReportFooter1";
        protected const string OrdersCreatedReportNoOrdersMessage = "OrdersCreatedReportNoOrdersMessage";
        protected const string OrdersCreatedReportDetailProductLine = "OrdersCreatedReportDetailProductLine";
        protected const string OrdersCreatedReportDetailUPCLine = "OrdersCreatedReportDetailUPCLine";
        protected const string UPC128 = "UPC128";

        protected const string BatteryConsRotHeader = "BatteryConsRotHeader";
        protected const string BatteryConsTotal = "BatteryConsAdjustment";
        protected const string BatteryConsReportHeader = "BatteryConsReportHeader";
        protected const string BatteryConsReportHeader2 = "BatteryConsReportHeader2";
        protected const string BatteryConsReportLines = "BatteryConsReportLines";
        protected const string BatteryConsReportLines2 = "BatteryConsReportLines2";

        protected const string BatInvSettDetailsHeader1 = "BatInvSettDetailsHeader1";
        protected const string BatInvSettDetailsHeader2 = "BatInvSettDetailsHeader2";
        protected const string BatInvSettDetailsHeader3 = "BatInvSettDetailsHeader3";
        protected const string BatInvSettDetailRow = "BatInvSettDetailRow";

        //Alowance
        protected const string AllowanceOrderDetailsHeader = "AllowanceOrderDetailsHeader";
        protected const string AllowanceOrderDetailsLine = "AllowanceOrderDetailsLine";

        //Full Consigment
        protected const string FullConsignmentCompanyInfo = "FullConsignmentCompanyInfo";
        protected const string FullConsignmentAgentInfo = "FullConsignmentAgentInfo";
        protected const string FullConsignmentConsignment = "FullConsignmentConsignment";
        protected const string FullConsignmentMerchant = "FullConsignmentMerchant";
        protected const string FullConsignmentMerchantId = "FullConsignmentMerchantId";
        protected const string FullConsignmentAddress = "FullConsignmentAddress";
        protected const string FullConsignmentLastTimeVisited = "FullConsignmentLastTimeVisited";
        protected const string FullConsignmentSectionName = "FullConsignmentSectionName";
        protected const string FullConsignmentCountHeader1 = "FullConsignmentCountHeader1";
        protected const string FullConsignmentCountHeader2 = "FullConsignmentCountHeader2";
        protected const string FullConsignmentCountLine = "FullConsignmentCountLine";
        protected const string FullConsignmentCountSep = "FullConsignmentCountSep";
        protected const string FullConsignmentCountTotal = "FullConsignmentCountTotal";
        protected const string FullConsignmentContractHeader = "FullConsignmentContractHeader";
        protected const string FullConsignmentContractLine = "FullConsignmentContractLine";
        protected const string FullConsignmentContractSep = "FullConsignmentContractSep";
        protected const string FullConsignmentText = "FullConsignmentText";
        protected const string FullConsignmentTotals = "FullConsignmentTotals";
        protected const string FullConsignmentPaymentHeader = "FullConsignmentPaymentHeader";
        protected const string FullConsignmentPaymentLine = "FullConsignmentPaymentLine";
        protected const string FullConsignmentPreviousBalance = "FullConsignmentPreviousBalance";
        protected const string FullConsignmentAfterDisc = "FullConsignmentAfterDisc";
        protected const string FullConsignmentPaymentSep = "FullConsignmentPaymentSep";
        protected const string FullConsignmentTotalDue = "FullConsignmentTotalDue";
        protected const string FullConsignmentPaymentTotal = "FullConsignmentPaymentTotal";
        protected const string FullConsignmentNewBalance = "FullConsignmentNewBalance";
        protected const string FullConsignmentPrintedOn = "FullConsignmentPrintedOn";
        protected const string FullConsignmentSignature = "FullConsignmentSignature";
        protected const string FullConsignmentFinalized = "FullConsignmentFinalized";
        protected const string FullConsignmentReturnsHeader = "FullConsignmentReturnsHeader";
        protected const string FullConsignmentReturnsLine = "FullConsignmentReturnsLine";
        protected const string FullConsignmentReturnsSep = "FullConsignmentReturnsSep";

        protected const string ClientStatementTableTitle = "ClientStatementTableTitle";
        protected const string ClientStatementTableHeader = "ClientStatementTableHeader";
        protected const string ClientStatementTableLine = "ClientStatementTableLine";
        protected const string ClientStatementTableTotal = "ClientStatementTableTotal";

        protected const string InvoiceTitleNumber = "InvoiceTitleNumber";

        #region payment Batch

        protected const string BatchDate = "BatchDate";
        protected const string BatchPrintedDate = "BatchPrintedDate";
        protected const string BatchSalesman = "BatchSalesman";
        protected const string ChecksTitle = "ChecksTitle";
        protected const string CheckTableHeader = "CheckTableHeader";
        protected const string CheckTableLine = "CheckTableLine";
        protected const string CheckTableTotal = "CheckTableTotal";

        protected const string OtherPaymentTotalHeader = "OtherPaymentTotalHeader";

        protected const string CashTotalLine = "CashTotalLine";
        protected const string CreditCardTotalLine = "CreditCardTotalLine";
        protected const string MoneyOrderTotalLine = "MoneyOrderTotalLine";

        protected const string BatchComments = "BatchComments";
        protected const string BatchSignature = "BatchSignature";
        protected const string BatchTotal = "BatchTotal";
        protected const string BatchBank = "BatchBank";

        #endregion

        #region Credit Report

        protected const string CreditReportDetailsHeader = "CreditReportDetailsHeader";
        protected const string CreditReportDetailsLine = "CreditReportDetailsLine";
        protected const string CreditReportDetailsTotal = "CreditReportDetailsTotal";
        protected const string CreditReportTotalsLine = "CreditReportTotalsLine";
        protected const string CreditReportHeader = "CreditReportHeader";
        protected const string CreditReportClientName = "CreditReportClientName";

        protected const string StandarPrintDriverName = "StandarPrintDriverName";
        protected const string StandarPrintedDate = "StandarPrintedDate";
        protected const string StandarPrintRouteNumber = "StandarPrintRouteNumber";

        #endregion

        #region Proof Delivery
        protected const string DeliveryHeader = "DeliveryHeader";
        protected const string DeliveryInvoiceNumber = "DeliveryInvoiceNumber";
        protected const string OrderDetailsHeaderDelivery = "OrderDetailsHeaderDelivery";
        protected const string OrderDetailsTotalsDelivery = "OrderDetailsTotalsDelivery";
        protected const string TotalQtysProofDelivery = "TotalQtysProofDelivery";
        protected string StandarPrintTitleProofDelivery = "StandarPrintTitleProofDelivery";
        protected const string OrderDetailsHeadersUoMDelivery = "OrderDetailsHeadersUoMDelivery";
        protected const string OrderDetailsTotalsUoMDelivery = "OrderDetailsTotalsUoMDelivery";

        #endregion

        #region Standard

        protected const string StandarPrintTitle = "StandarPrintTitle";
        protected const string StandarPrintDate = "StandarPrintDate";
        protected const string StandarPrintDateBig = "StandarPrintDateBig";
        protected const string StandarPrintCreatedBy = "StandarPrintCreatedBy";
        protected const string StandarPrintedOn = "StandarPrintedOn";
        protected const string StandarCreatedOn = "StandarCreatedOn";

        #endregion

        #region Order

        protected const string OrderClientName = "OrderClientName";
        protected const string OrderClientNameTo = "OrderClientNameTo";
        protected const string OrderClientAddress = "OrderClientAddress";
        protected const string OrderBillTo = "OrderBillTo";
        protected const string OrderBillTo1 = "OrderBillTo1";
        protected const string OrderShipTo = "OrderShipTo";
        protected const string OrderShipTo1 = "OrderShipTo1";
        protected const string OrderClientLicenceNumber = "OrderClientLicenceNumber";
        protected const string OrderVendorNumber = "OrderVendorNumber";
        protected const string OrderTerms = "OrderTerms";
        protected const string OrderAccountBalance = "OrderAccountBalance";
        protected const string OrderTypeAndNumber = "OrderTypeAndNumber";
        protected const string PONumber = "PONumber";

        protected const string CompanyName = "CompanyName";
        protected const string CompanyAddress = "CompanyAddress";
        protected const string CompanyPhone = "CompanyPhone";
        protected const string CompanyFax = "CompanyFax";
        protected const string CompanyEmail = "CompanyEmail";
        protected const string CompanyLicenses1 = "CompanyLicenses1";
        protected const string CompanyLicenses2 = "CompanyLicenses2";

        protected const string OrderPaymentText = "OrderPaymentText";
        protected const string OrderHeaderText = "OrderHeaderText";


        protected const string OrderDetailsHeaderSectionName = "OrderDetailsHeaderSectionName";
        protected const string OrderDetailsLines = "OrderDetailsLines";
        protected const string OrderDetailsLines2 = "OrderDetailsLines2";
        protected const string OrderDetailsLines3 = "OrderDetailsLines3";
        protected const string OrderDetailsLinesLotQty = "OrderDetailsLinesLotQty";
        protected const string OrderDetailsWeights = "OrderDetailsWeights";
        protected const string OrderDetailsWeightsCount = "OrderDetailsWeightsCount";
        protected const string OrderDetailsLinesRetailPrice = "OrderDetailsLinesRetailPrice";
        protected const string OrderDetailsLinesUpcText = "OrderDetailsLinesUpcText";
        protected const string OrderDetailsLinesUpcBarcode = "OrderDetailsLinesUpcBarcode";
        protected const string OrderDetailsLinesLongUpcBarcode = "OrderDetailsLinesLongUpcBarcode";
        protected const string OrderDetailsTotals = "OrderDetailsTotals";
        protected const string OrderTotalsNetQty = "OrderTotalsNetQty";
        protected const string OrderSubTotal = "OrderSubTotal";
        protected const string OrderTotalsSales = "OrderTotalsSales";
        protected const string OrderTotalContainers = "OrderTotalContainers";
        protected const string OrderTotalsCredits = "OrderTotalsCredits";
        protected const string OrderTotalsReturns = "OrderTotalsReturns";
        protected const string OrderTotalsNetAmount = "OrderTotalsNetAmount";
        protected const string OrderTotalsDiscount = "OrderTotalsDiscount";
        protected const string OrdertotalsAllowance = "OrdertotalsAllowance";
        protected const string OrderTotalsTax = "OrderTotalsTax";
        protected const string OrderTotalsTotalDue = "OrderTotalsTotalDue";
        protected const string OrderTotalsTotalPayment = "OrderTotalsTotalPayment";
        protected const string OrderTotalsCurrentBalance = "OrderTotalsCurrentBalance";
        protected const string OrderTotalsClientCurrentBalance = "OrderTotalsClientCurrentBalance";
        protected const string OrderTotalsDiscountComment = "OrderTotalsDiscountComment";
        protected const string OrderPreorderLabel = "OrderPreorderLabel";
        protected const string OrderComment = "OrderComment";
        protected const string OrderComment2 = "OrderComment2";
        protected const string PaymentComment = "PaymentComment";
        protected const string PaymentComment1 = "PaymentComment1";

        #endregion

        #region printlabel
        protected const string RetailPrice = "RetailPrice";
        protected const string UPC128ForLabel = "UPC128ForLabel";

        #endregion

        protected const string PickTicketCompanyHeader = "PickTicketCompanyHeader";
        protected const string PickTicketRouteInfo = "PickTicketRouteInfo";
        protected const string PickTicketDeliveryDate = "PickTicketDeliveryDate";
        protected const string PickTicketDriver = "PickTicketDriver";

        protected const string PickTicketProductHeader = "PickTicketProductHeader";
        protected const string PickTicketProductLine = "PickTicketProductLine";
        protected const string PickTicketProductTotal = "PickTicketProductTotal";

        protected abstract void FillDictionary();


        protected abstract IList<string> CompanyNameSplit(string name);

        protected abstract IList<string> PrintSalesCreditReportSplitProductName1(string name);

        protected abstract IList<string> PrintSalesCreditReportSplitProductName2(string name);

        protected abstract IList<string> PrintReceivedPaymentsReportSplitProductName1(string name);

        protected abstract IList<string> PrintReceivedPaymentsReportSplitProductName2(string name);

        protected abstract IList<string> PrintOrdersCreatedReportSplitProductName(string name);

        protected abstract IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName1(string name);

        protected abstract IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName2(string name);

        protected abstract IList<string> PrintOrdersCreatedReportWithDetailsSplitProductName3(string name);

        protected abstract IList<string> GetClientNameSplit(string name);

        protected abstract IList<string> GetDetailsRowsSplitProductName1(string name);

        protected abstract IList<string> GetDetailsRowsSplitProductName2(string name);

        protected abstract IList<string> GetDetailsRowsSplitProductNameConsignment(string name, bool counting);

        protected abstract IList<string> GetBottomTextSplitText(string text = "");

        protected abstract IList<string> GetBottomDiscountTextSplitText();

        protected abstract IList<string> GetInventoryCheckDetailsRowsSplitProductName1(string name);

        protected abstract IList<string> GetInventoryDetailsRowsSplitProductName(string name);

        protected abstract IList<string> GetSetInventoryDetailsRowsSplitProductName(string name);

        protected abstract IList<string> OrderCommentsSplit(string name);

        protected abstract IList<string> GetTransferOnOffSplitProductName(string name);

        protected abstract IList<string> GetAddInventoryDetailsRowsSplitProductName(string name);

        protected abstract IList<string> GetLabelNotAFinalRouteReturn(string name);

        protected abstract IList<string> GetInventorySetDetailsRowsSplitProductName(string name);

        protected abstract IList<string> GetConsignmentDetailsRows(ref int startIndex, ref float totalQty, Order order, bool conting);

        protected abstract IList<string> GetSalesRegRepClientSplitProductName(string name);

        protected abstract IList<string> GetOrderPaymentSplitComment(string name);

        protected abstract int WidthForBoldFont
        {
            get;
        }

        protected abstract int WidthForNormalFont
        {
            get;
        }

        protected abstract int SpaceForOrderFooter
        {
            get;
        }


        protected string printCommand;

        protected virtual void PrintIt(string printingString)
        {
            if (printingString.Contains((char)241) || printingString.Contains((char)209))
                printingString = InsertSpecialChar(printingString);

            if (Config.UseFastPrinter)
            {
                PrintItInFastPrinter(printingString);
                return;
            }

            // Logger.CreateLog (printingString);
            printCommand = printingString;

            //var stack = new System.Diagnostics.StackFrame ();
            //Logger.CreateLog (stack.ToString ());
            if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
                throw new InvalidOperationException("No valid printer selected");

            using (BluetoothDevice hxm = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(PrinterProvider.PrinterAddress))
            {
                UUID applicationUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                using (BluetoothSocket socket = hxm.CreateRfcommSocketToServiceRecord(applicationUUID))
                {
                    int factor = 1 + Config.OldPrinter;

                    var timer = 0;
                    bool connected = false;
                    Exception e = null;

                    while (timer < factor && !connected)
                    {
                        try
                        {
                            Thread.Sleep(1000 * factor);
                            socket.Connect();
                            Thread.Sleep(1000 * factor);
                            connected = true;
                            break;
                        }
                        catch (Exception ee)
                        {
                            e = ee;
                        }
                        timer++;
                    }

                    if (!connected && e != null)
                        throw e;

                    using (var inReader = new BufferedReader(new InputStreamReader(socket.InputStream)))
                    {
                        using (var outReader = new BufferedWriter(new OutputStreamWriter(socket.OutputStream), 60000))
                        {
                            DateTime st = DateTime.Now;
                            Logger.CreateLog("printingString.Length " + printingString.Length);
                            if (printingString.Length > 40000)
                            {
                                int i = 0;
                                while (true)
                                {
                                    int start = 10000 * i;
                                    int end = 10000;
                                    if (start + end > printingString.Length)
                                        end = printingString.Length - start;
                                    outReader.Write(printingString, start, end);
                                    outReader.Flush();
                                    i++;
                                    if (end != 10000)
                                        break;
                                }
                            }
                            else
                            {
                                outReader.Write(printingString);
                                outReader.Flush();
                            }

                            //some waiting
                            int sec = 2;
                            int extra = 1;

                            if (printingString.Length > 10000)
                                extra = 2;
                            if (printingString.Length > 15000)
                                extra = 3;
                            if (printingString.Length > 20000)
                                extra = 4;
                            if (printingString.Length > 25000)
                                extra = 5;
                            if (printingString.Length > 30000)
                                extra = 6;
                            if (printingString.Length > 35000)
                                extra = 7;
                            if (printingString.Length > 40000)
                                extra = 8;
                            if (printingString.Length > 45000)
                                extra = 9;
                            if (printingString.Length > 50000)
                                extra = 10;

                            Thread.Sleep(sec * (extra + factor) * 1000);

                            inReader.Close();
                            socket.Close();
                            outReader.Close();
                        }
                    }
                    Logger.CreateLog("finishing printing");
                }
            }
        }

        public string InsertSpecialChar(string lines)
        {
            string temp_line = lines;
            temp_line = temp_line.Replace((char)241, (char)110);
            temp_line = temp_line.Replace((char)209, (char)78);
            return temp_line;
        }
        protected virtual void PrintItInFastPrinter(string printingString)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.CreateLog("Inside of PrintItInFastPrinter");

            // Logger.CreateLog (printingString);
            printCommand = printingString;

            //var stack = new System.Diagnostics.StackFrame ();
            //Logger.CreateLog (stack.ToString ());
            if (string.IsNullOrEmpty(PrinterProvider.PrinterAddress))
                throw new InvalidOperationException("No valid printer selected");

            using (BluetoothDevice hxm = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(PrinterProvider.PrinterAddress))
            {
                UUID applicationUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                using (BluetoothSocket socket = hxm.CreateRfcommSocketToServiceRecord(applicationUUID))
                {
                    try
                    {
                        socket.Connect();
                    }
                    catch (Exception ee)
                    {
                        Logger.CreateLog(ee);
                    }

                    using (var inReader = new BufferedReader(new InputStreamReader(socket.InputStream)))
                    {
                        using (var outReader = new BufferedWriter(new OutputStreamWriter(socket.OutputStream), 60000))
                        {
                            DateTime st = DateTime.Now;
                            Logger.CreateLog("printingString.Length " + printingString.Length);

                            var parts = printingString.SplitInParts(10000);
                            foreach (var part in parts)
                            {
                                outReader.Write(part);
                                outReader.Flush();
                            }

                            //some waiting but less 
                            int extra = 1;

                            extra = GetExtraSeconds(printingString.Length);

                            Thread.Sleep((extra));

                            inReader.Close();
                            socket.Close();
                            outReader.Close();
                        }
                    }
                    Logger.CreateLog("finishing printing");
                }
            }

            Logger.CreateLog("Printing Stopwatch stop. Elapsed milliseconds " + stopwatch.ElapsedMilliseconds);
        }

        public int GetExtraSeconds(int length)
        {
            var extra_secs = 5000;
            var estimated_time = (length / 10000) * 1000;

            if (estimated_time > 5000)
                extra_secs = estimated_time;

            return extra_secs;
        }

        public virtual bool ConfigurePrinter()
        {
            try
            {
                PrintIt("! U1 setvar \"device.languages\" \"zpl\"");
                PrintIt("^XA^PON^MNN^LL100^FO40,40^ADN,36,20^FDPrinter Configured^FS^XZ");
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                // // Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected abstract IList<string> GetDetailsRowsSplitProductNameAllowance(string name);

        public abstract bool PrintInventoryProd(List<InventoryProd> SortedList);
        public abstract bool PrintPaymentBatch();
        public bool PrintAcceptedOrders(List<Order> orders, bool final)
        {
            return false;
        }
        public abstract bool PrintLabels(List<Order> orders);

        public bool PrintProductLabel(string label)
        {

            try
            {
                PrintIt(label);
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public abstract bool PrintCreditReport(int index, int count);

        #region Proof Delivery

        public virtual bool PrintProofOfDelivery(Order order)
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

        protected virtual IEnumerable<string> GetSignatureSectionDelivery(ref int startY, Order order, List<string> all_lines = null)
        {
            List<string> lines = new List<string>();

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

                if (!string.IsNullOrEmpty(Config.ExtraInfoBottomPrint))
                {
                    var text = Config.ExtraInfoBottomPrint;

                    if (!string.IsNullOrEmpty(order.CompanyName) && (order.CompanyName.ToLower().Contains("el chilar") || order.CompanyName.ToLower().Contains("lisy corp") || order.CompanyName.ToLower().Contains("el puro sabor")))
                    {
                        text = text.Replace("[COMPANY]", order.CompanyName);

                        startY += font18Separation;
                        foreach (var line in GetBottomSplitText(text))
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                            startY += font18Separation;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
                lines.AddRange(GetFooterRowsDelivery(ref startY, order.CompanyName));

            return lines;
        }

        protected virtual IEnumerable<string> GetFooterRowsDelivery(ref int startIndex, string CompanyName = null)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, font18Separation, 4);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            AddExtraSpace(ref startIndex, list, 12, 1);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            AddExtraSpace(ref startIndex, list, font18Separation, 4);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.ExtraInfoBottomPrint))
            {
                var text = Config.ExtraInfoBottomPrint;

                if (!string.IsNullOrEmpty(CompanyName) && (CompanyName.ToLower().Contains("el chilar") || CompanyName.ToLower().Contains("lisy corp") || CompanyName.ToLower().Contains("el puro sabor")))
                {
                    text = text.Replace("[COMPANY]", CompanyName);

                    startIndex += font18Separation;
                    foreach (var line in GetBottomSplitText(text))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                        startIndex += font18Separation;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                AddExtraSpace(ref startIndex, list, font18Separation, 1);
                foreach (var line in GetBottomSplitText())
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
                }
            }

            return list;
        }
        protected virtual IEnumerable<string> GetTotalsRowsInOneDocDelivery(ref int startY, Order order)
        {
            List<string> list = new List<string>();
            double total = 0;

            foreach (var detail in order.Details)
            {
                var productNameLines = SplitProductName(detail.Product.Name, 35, 35);
                double qty = detail.Qty;
                total += qty;

                bool isFirstLine = true;
                var uomName = detail.UnitOfMeasure?.Name ?? string.Empty;
                var uomLines = SplitProductName(uomName, 10, 10);
                foreach (var productNameLine in productNameLines)
                {
                    var currentUoM = isFirstLine && uomLines.Count > 0 ? uomLines[0] : string.Empty;

                    if (isFirstLine)
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsUoMDelivery], startY, productNameLine, qty, currentUoM));
                        isFirstLine = false;
                    }
                    else
                    {
                        startY += font18Separation;
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsUoMDelivery], startY, productNameLine, string.Empty, string.Empty));
                    }

                    if (isFirstLine == false && uomLines.Count > 1)
                    {
                        for (int i = 1; i < uomLines.Count; i++)
                        {
                            startY += font18Separation;
                            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsUoMDelivery], startY, string.Empty, string.Empty, uomLines[i]));
                        }
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
        protected virtual IEnumerable<string> GetDetailsRowsInOneDocDelivery(ref int startY, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {

            List<string> list = new List<string>();

            list.AddRange(GetDetailTableHeaderDelivery(ref startY));

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

            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());


            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

            }

            return list;
        }

        protected virtual IEnumerable<string> GetDetailTableHeaderDelivery(ref int startY)
        {
            List<string> lines = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeadersUoMDelivery];//OrderDetailsHeaderDelivery

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return lines;
        }
        protected virtual IEnumerable<string> GetHeaderRowsInOneDocDelivery(ref int startY, Order order, Client client, string printedId)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = GetOrderDocumentNameDelivery(ref printExtraDocName, order, client);

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = docName + ": " + order.PrintedOrderId;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, s1, string.Empty));
            startY += font36Separation;

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

        protected virtual string GetOrderDocumentNameDelivery(ref bool printExtraDocName, Order order, Client client)
        {
            string docName = "Proof of Delivery";

            return docName;
        }

        protected virtual void FillOrderDictionaries(Order order, Dictionary<string, OrderLine> salesLines, Dictionary<string, OrderLine> creditLines, Dictionary<string, OrderLine> returnsLines)
        {
            double balance = order.OrderTotalCost();
            var rItems = order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.RelatedOrderDetail).ToList();
            rItems.AddRange(order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.OrderDetailId));

            foreach (var od in order.Details)
            {
                if (od.HiddenItem)
                    continue;

                var uomId = -1;
                if (od.UnitOfMeasure != null)
                    uomId = od.UnitOfMeasure.Id;

                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + "-" + uomId.ToString();

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
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, ListPrice = od.ExpectedPrice, OrderDetail = od, ParticipatingDetails = new List<OrderDetail>() });

                currentDic[key].Qty = currentDic[key].Qty + (od.Product.SoldByWeight && !order.AsPresale ? od.Weight : od.Qty);
                currentDic[key].ParticipatingDetails.Add(od);
            }

        }

        public virtual string GetLogoLabel(ref int startY)
        {
            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;

                return label;
            }

            return string.Empty;
        }

        public virtual void AddExtraSpace(ref int startY, List<string> lines, int font, int spaces)
        {
            for (int i = 0; i < spaces; i++)
                startY += font;
        }

        protected abstract bool PrintLines(List<string> lines);
        protected virtual string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            DateTime st = DateTime.Now;
            Android.Graphics.Bitmap signature;
            signature = order.ConvertSignatureToBitmap();
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            DateTime st1 = DateTime.Now;
            var converter = new LaceupAndroidApp.BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature, null);
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());
            st1 = DateTime.Now;
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width / 32) * 32) / 8;
            int height = signature.Height / 32 * 32;

            var bitmapDataLength = rawBytes.Length; // bitmapFileData.Length - bitmapDataOffset;
                                                    //byte[] bitmap = new byte[bitmapDataLength];
                                                    //Buffer.BlockCopy(bitmapFileData, bitmapDataOffset, bitmap, 0, bitmapDataLength);

            string ZPLImageDataString = BitConverter.ToString(rawBytes);
            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

            Logger.CreateLog("ZPLImageDataString.Replace took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());

            string label = "^FO40," + startIndex.ToString() + "^GFA, " + //"^FO300,"
                           bitmapDataLength.ToString() + "," +
                           bitmapDataLength.ToString() + "," +
                           widthInBytes.ToString() + "," +
                           ZPLImageDataString;

            startIndex += height;

            var ts = DateTime.Now.Subtract(st).TotalSeconds;
            Logger.CreateLog("IncludeSignature took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            return label;
        }
        protected abstract IList<string> GetBottomSplitText(string text = "");

        public static IList<string> SplitProductName(string productName, int firstLine, int otherLines)
        {
            List<string> retList = new List<string>();
            //productName = ConvertProdName(productName);
            if (productName == null)
                return retList;

            string[] parts = productName.Split(new char[] { ' ' });
            int currentSize = 0;
            StringBuilder sb = new StringBuilder(otherLines * 2);
            int size = firstLine;
            bool isFirstLine = true;
            foreach (string part in parts)
                if ((currentSize + part.Length < size) || currentSize == 0)
                {
                    if (currentSize != 0)
                    {
                        sb.Append(" ");
                        sb.Append(part);
                        currentSize++;
                    }
                    else
                        sb.Append(part);
                    currentSize += part.Length;
                }
                else
                {
                    retList.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                    sb.Append(part);
                    currentSize = part.Length;
                    if (isFirstLine)
                    {
                        size = otherLines;
                        isFirstLine = false;
                    }

                }
            if (currentSize > 0)
                retList.Add(sb.ToString());


            return retList;
        }

        protected virtual IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (order.CompanyId > 0)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                    startIndex += font36Separation;
                }
                // startIndex += font36Separation;
                if (company.CompanyAddress1.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress1));
                    startIndex += font18Separation;
                }

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress2));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startIndex, company.CompanyPhone));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, company.CompanyFax));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyLicenses))
                {
                    var licenses = company.CompanyLicenses.Split(',').ToList();

                    for (int i = 0; i < licenses.Count; i++)
                    {
                        var format = i == 0 ? CompanyLicenses1 : CompanyLicenses2;

                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startIndex, licenses[i]));
                        startIndex += font18Separation;
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected string[] ClientAddress(Client client, bool shipTo = true)
        {
            var parts = (shipTo ? client.ShipToAddress : client.BillToAddress).Split(new char[] { '|' });
            if (parts.Length == 5)
            {
                parts[2] = parts[2].Trim() + ", " + parts[3].Trim() + " " + parts[4].Trim();
                if (parts[1].Trim().Length == 0)
                {
                    var newParts = new string[2];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[2].Trim();
                    return newParts;
                }
                else
                {
                    var newParts = new string[3];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[1].Trim();
                    newParts[2] = parts[2].Trim();
                    return newParts;
                }
            }
            if (parts.Length == 4)
            {
                parts[2] = parts[2].Trim() + ", " + parts[3].Trim();
                if (parts[1].Trim().Length == 0)
                {
                    var newParts = new string[2];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[2].Trim();
                    return newParts;
                }
                else
                {
                    var newParts = new string[3];
                    newParts[0] = parts[0].Trim();
                    newParts[1] = parts[1].Trim();
                    newParts[2] = parts[2].Trim();
                    return newParts;
                }
            }
            return parts;
        }

        public virtual string ToString(double d)
        {
            return d.ToCustomString();
        }

        public abstract bool PrintVehicleInformation(bool fromEOD, int index = 0, int count = 0, bool isReport = false);

        public abstract bool PrintPickTicket(Order order);

        #endregion



    }
}

