using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace LaceupMigration
{
    static class StringExtensions
    {

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }
    public abstract class PrinterGeneric : IPrinter
    {
        protected Dictionary<string, string> linesTemplates = new Dictionary<string, string>();

        #region Labels

        protected const string StartLabel = "StartLabel";
        protected const string EndLabel = "EndLabel";

        #region Standard

        protected const string StandarPrintTitle = "StandarPrintTitle";
        protected const string StandarPrintDate = "StandarPrintDate";
        protected const string StandarPrintDateBig = "StandarPrintDateBig";
        protected const string StandarPrintRouteNumber = "StandarPrintRouteNumber";
        protected const string StandarPrintDriverName = "StandarPrintDriverName";
        protected const string StandarPrintCreatedBy = "StandarPrintCreatedBy";
        protected const string StandarPrintedDate = "StandarPrintedDate";
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

        protected const string OrderDetailsHeader = "OrderDetailsHeader";
        protected const string OrderDetailsLineSeparator = "OrderDetailsLineSeparator";
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
        protected const string OrderDetailsTotals1 = "OrderDetailsTotals1";
        protected const string OrderTotalsNetQty = "OrderTotalsNetQty";
        protected const string OrderTotalsSales = "OrderTotalsSales";
        protected const string OrderSubTotal = "OrderSubTotal";
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
        protected const string OrderTotalsFreight = "OrderTotalsFreight";
        protected const string OrderTotalsOtherCharges = "OrderTotalsOtherCharges";
        protected const string OrderTotalsDiscountComment = "OrderTotalsDiscountComment";
        protected const string OrderPreorderLabel = "OrderPreorderLabel";
        protected const string OrderComment = "OrderComment";
        protected const string OrderComment2 = "OrderComment2";
        protected const string PaymentComment = "PaymentComment";
        protected const string PaymentComment1 = "PaymentComment1";

        #endregion

        protected const string PickTicketCompanyHeader = "PickTicketCompanyHeader";
        protected const string PickTicketRouteInfo = "PickTicketRouteInfo";
        protected const string PickTicketDeliveryDate = "PickTicketDeliveryDate";
        protected const string PickTicketDriver = "PickTicketDriver";

        protected const string PickTicketProductHeader = "PickTicketProductHeader";
        protected const string PickTicketProductLine = "PickTicketProductLine";
        protected const string PickTicketProductTotal = "PickTicketProductTotal";

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

        #region printlabel
        protected const string RetailPrice = "RetailPrice";
        protected const string UPC128ForLabel = "UPC128ForLabel";

        #endregion

        #region Footer

        protected const string FooterSignatureLine = "FooterSignatureLine";
        protected const string FooterSignatureText = "FooterSignatureText";
        protected const string FooterSignatureNameText = "FooterSignatureNameText";
        protected const string FooterSpaceSignatureText = "FooterSpaceSignatureText";
        protected const string FooterBottomText = "FooterBottomText";
        protected const string FooterDriverSignatureText = "FooterDriverSignatureText";

        #endregion

        protected const string Upc128 = "Upc128";

        #region Allowance

        protected const string AllowanceOrderDetailsHeader = "AllowanceOrderDetailsHeader";
        protected const string AllowanceOrderDetailsLine = "AllowanceOrderDetailsLine";

        #endregion

        #region Shortage Report

        protected const string ShortageReportHeader = "ShortageReportHeader";
        protected const string ShortageReportDate = "ShortageReportDate";
        protected const string ShortageReportInvoiceHeader = "ShortageReportInvoiceHeader";
        protected const string ShortageReportTableHeader = "ShortageReportTableHeader";
        protected const string ShortageReportTableLine = "ShortageReportTableLine";

        #endregion

        #region Load Order

        protected const string LoadOrderHeader = "LoadOrderHeader";
        protected const string LoadOrderRequestedDate = "LoadOrderRequestedDate";
        protected const string LoadOrderNotFinal = "LoadOrderNotFinal";
        protected const string LoadOrderTableHeader = "LoadOrderTableHeader";
        protected const string LoadOrderTableLine = "LoadOrderTableLine";
        protected const string LoadOrderTableTotal = "LoadOrderTableTotal";

        #endregion

        #region Credit Report

        protected const string CreditReportDetailsHeader = "CreditReportDetailsHeader";
        protected const string CreditReportDetailsLine = "CreditReportDetailsLine";
        protected const string CreditReportDetailsTotal = "CreditReportDetailsTotal";
        protected const string CreditReportTotalsLine = "CreditReportTotalsLine";
        protected const string CreditReportHeader = "CreditReportHeader";
        protected const string CreditReportClientName = "CreditReportClientName";

        #endregion

        protected const string IceCreamFactoryComment = "IceCreamFactoryComment";

        #region Accept Load

        protected const string AcceptLoadHeader = "AcceptLoadHeader";
        protected const string AcceptLoadDate = "AcceptLoadDate";
        protected const string AcceptLoadInvoice = "AcceptLoadInvoice";
        protected const string AcceptLoadNotFinal = "AcceptLoadNotFinal";
        protected const string AcceptLoadTableHeader = "AcceptLoadTableHeader";
        protected const string AcceptLoadTableHeader1 = "AcceptLoadTableHeader1";
        protected const string AcceptLoadTableLine = "AcceptLoadTableLine";
        protected const string AcceptLoadLotLine = "AcceptLoadLotLine";
        protected const string AcceptLoadWeightLine = "AcceptLoadWeightLine";
        protected const string AcceptLoadTableTotals = "AcceptLoadTableTotals";
        protected const string AcceptLoadTableTotals1 = "AcceptLoadTableTotals1";

        #endregion

        #region Add Inventory

        protected const string AddInventoryHeader = "AddInventoryHeader";
        protected const string AddInventoryDate = "AddInventoryDate";
        protected const string AddInventoryNotFinal = "AddInventoryNotFinal";
        protected const string AddInventoryTableHeader = "AddInventoryTableHeader";
        protected const string AddInventoryTableHeader1 = "AddInventoryTableHeader1";
        protected const string AddInventoryTableLine = "AddInventoryTableLine";
        protected const string AddInventoryTableTotals = "AddInventoryTableTotals";

        #endregion

        #region Inventory

        protected const string InventoryProdHeader = "InventoryProdHeader";
        protected const string InventoryProdTableHeader = "InventoryProdTableHeader";
        protected const string InventoryProdTableLine = "InventoryProdTableLine";
        protected const string InventoryProdTableLineLot = "InventoryProdTableLineLot";
        protected const string InventoryProdTableLineListPrice = "InventoryProdTableLineListPrice";
        protected const string InventoryProdQtyItems = "InventoryProdQtyItems";
        protected const string InventoryProdInvValue = "InventoryProdInvValue";

        #endregion

        #region Order Created Report

        protected const string OrderCreatedReportHeader = "OrderCreatedReportHeader";
        protected const string OrderCreatedReporWorkDay = "OrderCreatedReporWorkDay";
        protected const string OrderCreatedReporBreaks = "OrderCreatedReporBreaks";
        protected const string OrderCreatedReportTableHeader = "OrderCreatedReportTableHeader";
        protected const string OrderCreatedReportTableLine = "OrderCreatedReportTableLine";
        protected const string OrderCreatedReportTableLine1 = "OrderCreatedReportTableLine1";
        protected const string OrderCreatedReportTableTerms = "OrderCreatedReportTableTerms";
        protected const string OrderCreatedReportTableLineComment = "OrderCreatedReportTableLineComment";
        protected const string OrderCreatedReportTableLineComment1 = "OrderCreatedReportTableLineComment1";
        protected const string OrderCreatedReportSubtotal = "OrderCreatedReportSubtotal";
        protected const string OrderCreatedReportTax = "OrderCreatedReportTax";
        protected const string OrderCreatedReportTotals = "OrderCreatedReportTotals";
        protected const string OrderCreatedReportPaidCust = "OrderCreatedReportPaidCust";
        protected const string OrderCreatedReportChargeCust = "OrderCreatedReportChargeCust";
        protected const string OrderCreatedReportCreditCust = "OrderCreatedReportCreditCust";
        protected const string OrderCreatedReportExpectedCash = "OrderCreatedReportExpectedCash";
        protected const string OrderCreatedReportFullTotal = "OrderCreatedReportFullTotal";
        protected const string OrderCreatedReportCreditTotal = "OrderCreatedReportCreditTotal";
        protected const string OrderCreatedReportNetTotal = "OrderCreatedReportNetTotal";
        protected const string OrderCreatedReportBillTotal = "OrderCreatedReportBillTotal";
        protected const string OrderCreatedReportSalesTotal = "OrderCreatedReportSalesTotal";
        protected const string OrderCommentWork = "OrderCommentWork";

        #endregion

        #region Payment Report

        protected const string PaymentReportHeader = "PaymentReportHeader";
        protected const string PaymentReportTableHeader = "PaymentReportTableHeader";
        protected const string PaymentReportTableHeader1 = "PaymentReportTableHeader1";
        protected const string PaymentReportTableLine = "PaymentReportTableLine";
        protected const string PaymentReportTotalCash = "PaymentReportTotalCash";
        protected const string PaymentReportTotalCheck = "PaymentReportTotalCheck";
        protected const string PaymentReportTotalCC = "PaymentReportTotalCC";
        protected const string PaymentReportTotalMoneyOrder = "PaymentReportTotalMoneyOrder";
        protected const string PaymentReportTotalTransfer = "PaymentReportTotalTransfer";
        protected const string PaymentReportTotalTotal = "PaymentReportTotalTotal";
        protected const string PaymentSignatureText = "PaymentSignatureText";


        #endregion

        #region Settlement

        protected const string InventorySettlementHeader = "InventorySettlementHeader";
        protected const string InventorySettlementProductHeader = "InventorySettlementProductHeader";
        protected const string InventorySettlementTableHeader = "InventorySettlementTableHeader";
        protected const string InventorySettlementTableHeader1 = "InventorySettlementTableHeader1";
        protected const string InventorySettlementProductLine = "InventorySettlementProductLine";
        protected const string InventorySettlementLotLine = "InventorySettlementLotLine";
        protected const string InventorySettlementTableLine = "InventorySettlementTableLine";
        protected const string InventorySettlementTableTotals = "InventorySettlementTableTotals";
        protected const string InventorySettlementTableTotals1 = "InventorySettlementTableTotals1";
        protected const string InventorySettlementAssetTracking = "InventorySettlementAssetTracking";

        #endregion

        #region Inventory Summary

        protected const string InventorySummaryHeader = "InventorySummaryHeader";
        protected const string InventorySummaryTableHeader = "InventorySummaryTableHeader";
        protected const string InventorySummaryTableHeader1 = "InventorySummaryTableHeader1";
        protected const string InventorySummaryTableProductLine = "InventorySummaryTableProductLine";
        protected const string InventorySummaryTableLine = "InventorySummaryTableLine";
        protected const string InventorySummaryTableTotals = "InventorySummaryTableTotals";
        protected const string InventorySummaryTableTotals1 = "InventorySummaryTableTotals1";

        #endregion

        #region Consignment

        protected const string ConsignmentInvoiceHeader = "ConsignmentInvoiceHeader";
        protected const string ConsignmentSalesOrderHeader = "ConsignmentSalesOrderHeader";
        protected const string ConsignmentInvoiceTableHeader = "ConsignmentInvoiceTableHeader";
        protected const string ConsignmentInvoiceTableLine = "ConsignmentInvoiceTableLine";
        protected const string ConsignmentInvoiceTableLineLot = "ConsignmentInvoiceTableLineLot";
        protected const string ConsignmentInvoiceTableTotal = "ConsignmentInvoiceTableTotal";

        protected const string ConsignmentContractHeader = "ConsignmentContractHeader";
        protected const string ConsignmentContractTableHeader1 = "ConsignmentContractTableHeader1";
        protected const string ConsignmentContractTableHeader2 = "ConsignmentContractTableHeader2";
        protected const string ConsignmentContractTableLine = "ConsignmentContractTableLine";
        protected const string ConsignmentContractTableTotal = "ConsignmentContractTableTotal";

        #endregion

        #region Route Return

        protected const string RouteReturnsTitle = "RouteReturnsTitle";
        protected const string RouteReturnsNotFinalLabel = "RouteReturnsNotFinalLabel";
        protected const string RouteReturnsTableHeader = "RouteReturnsTableHeader";
        protected const string RouteReturnsTableLine = "RouteReturnsTableLine";
        protected const string RouteReturnsTotals = "RouteReturnsTotals";

        #endregion

        #region Payment

        protected const string PaymentTitle = "PaymentTitle";
        protected const string PaymentHeaderTo = "PaymentHeaderTo";
        protected const string PaymentHeaderClientName = "PaymentHeaderClientName";
        protected const string PaymentHeaderClientAddr = "PaymentHeaderClientAddr";
        protected const string PaymentInvoiceNumber = "PaymentInvoiceNumber";
        protected const string PaymentInvoiceTotal = "PaymentInvoiceTotal";
        protected const string PaymentPaidInFull = "PaymentPaidInFull";
        protected const string PaymentComponents = "PaymentComponents";
        protected const string PaymentTotalPaid = "PaymentTotalPaid";
        protected const string PaymentPending = "PaymentPending";

        #endregion

        #region Open Invoice

        protected const string InvoiceTitle = "InvoiceTitle";
        protected const string InvoiceCopy = "InvoiceCopy";
        protected const string InvoiceDueOnOverdue = "InvoiceDueOnOverdue";
        protected const string InvoiceDueOn = "InvoiceDueOn";
        protected const string InvoiceClientName = "InvoiceClientName";
        protected const string InvoiceCustomerNumber = "InvoiceCustomerNumber";
        protected const string InvoiceClientAddr = "InvoiceClientAddr";
        protected const string InvoiceClientBalance = "InvoiceClientBalance";
        protected const string InvoiceComment = "InvoiceComment";
        protected const string InvoiceTableHeader = "InvoiceTableHeader";
        protected const string InvoiceTableLine = "InvoiceTableLine";
        protected const string InvoiceTotal = "InvoiceTotal";
        protected const string InvoicePaidInFull = "InvoicePaidInFull";
        protected const string InvoicePaidInFullCredit = "InvoicePaidInFullCredit";
        protected const string InvoiceCredit = "InvoiceCredit";
        protected const string InvoicePartialPayment = "InvoicePartialPayment";
        protected const string InvoiceOpen = "InvoiceOpen";
        protected const string InvoiceQtyItems = "InvoiceQtyItems";
        protected const string InvoiceQtyUnits = "InvoiceQtyUnits";

        #endregion

        #region Transfers

        protected const string TransferOnHeader = "TransferOnHeader";
        protected const string TransferOffHeader = "TransferOffHeader";
        protected const string TransferNotFinal = "TransferNotFinal";
        protected const string TransferTableHeader = "TransferTableHeader";
        protected const string TransferTableLine = "TransferTableLine";
        protected const string TransferTableLinePrice = "TransferTableLinePrice";
        protected const string TransferQtyItems = "TransferQtyItems";
        protected const string TransferAmount = "TransferAmount";
        protected const string TransferComment = "TransferComment";

        #endregion

        #region Client Statement

        protected const string ClientStatementTableTitle = "ClientStatementTableTitle";
        protected const string ClientStatementTableHeader = "ClientStatementTableHeader";
        protected const string ClientStatementTableHeader1 = "ClientStatementTableHeader1";
        protected const string ClientStatementTableLine = "ClientStatementTableLine";
        protected const string ClientStatementTableLine1 = "ClientStatementTableLine1";
        protected const string ClientStatementCurrent = "ClientStatementCurrent";
        protected const string ClientStatement1_30PastDue = "ClientStatement1_30PastDue";
        protected const string ClientStatement31_60PastDue = "ClientStatement31_60PastDue";
        protected const string ClientStatement61_90PastDue = "ClientStatement61_90PastDue";
        protected const string ClientStatementOver90PastDue = "ClientStatementOver90PastDue";
        protected const string ClientStatementAmountDue = "ClientStatementAmountDue";

        #endregion

        #region Inventory Count

        protected const string InventoryCountHeader = "InventoryCountHeader";
        protected const string InventoryCountTableHeader = "InventoryCountTableHeader";
        protected const string InventoryCountTableLine = "InventoryCountTableLine";

        #endregion

        #region Accepted Orders

        protected string AcceptedOrdersHeader = "AcceptedOrdersHeader";
        protected string AcceptedOrdersDate = "AcceptedOrdersDate";
        protected string AcceptedOrdersDeliveriesLabel = "AcceptedOrdersDeliveriesLabel";
        protected string AcceptedOrdersDeliveriesTableHeader = "AcceptedOrdersDeliveriesTableHeader";
        protected string AcceptedOrdersCreditsLabel = "AcceptedOrdersCreditsLabel";
        protected string AcceptedOrdersTableLine = "AcceptedOrdersDeliveriesTableLine";
        protected string AcceptedOrdersTableLine2 = "AcceptedOrdersDeliveriesTableLine2";
        protected string AcceptedOrdersLoadsTableHeader = "AcceptedOrdersLoadsTableHeader";
        protected string AcceptedOrdersTableTotals = "AcceptedOrdersTableTotals";
        protected string AcceptedOrdersTotalsQty = "AcceptedOrdersTotalsQty";
        protected string AcceptedOrdersTotalsWeight = "AcceptedOrdersTotalsWeight";
        protected string AcceptedOrdersTotalsAmount = "AcceptedOrdersTotalsAmount";

        #endregion

        #region Refusal Report

        protected string RefusalReportHeader = "RefusalReportHeader";
        protected string RefusalReportTableHeader = "RefusalReportTableHeader";
        protected string RefusalReportTableLine = "RefusalReportTableLine";
        protected string RefusalReportProductTableHeader = "RefusalReportProductTableHeader";
        protected string RefusalReportProductTableLine = "RefusalReportProductTableLine";

        #endregion

        #region New Refusal Report

        protected string NewRefusalReportTableHeader1 = "NewRefusalReportTableHeader1";
        protected string NewRefusalReportTableHeader = "NewRefusalReportTableHeader";
        protected string NewRefusalReportProductTableHeader = "NewRefusalReportProductTableHeader";
        protected string NewRefusalReportProductTableLine = "NewRefusalReportProductTableLine";
        #endregion

        #endregion

        #region Proof Delivery
        protected string OrderDetailsHeaderDelivery = "OrderDetailsHeaderDelivery";
        protected string DeliveryHeader = "DeliveryHeader";
        protected string OrderDetailsHeaderUoMDelivery = "OrderDetailsHeaderUoMDelivery";
        protected string DeliveryInvoiceNumber = "DeliveryInvoiceNumber";
        protected string OrderDetailsTotalsDelivery = "OrderDetailsTotalsDelivery";
        protected string TotalQtysProofDelivery = "TotalQtysProofDelivery";
        protected string OrderDetailsTotalsUoMDelivery = "OrderDetailsTotalsUoMDelivery";
        protected string StandarPrintTitleProofDelivery = "StandarPrintTitleProofDelivery";
        #endregion

        protected string VehicleInformationHeader = "VehicleInformationHeader";
        protected string VehicleInformationHeader1 = "VehicleInformationHeader1";

        #region Fonts

        protected const int orderDetailSeparation = 3;
        protected int font18Separation = 25;
        protected const int font20Separation = 20;
        protected const int font36Separation = 43;

        #endregion

        #region Spaces

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

        protected abstract int SpaceForPadding
        {
            get;
        }
        #endregion

        public PrinterGeneric()
        {
            Config.OrderDatePrintFormat = "MM/dd/yy h:mm tt";

            FillDictionary();
        }

        #region Utils

        protected abstract void FillDictionary();

        protected string printCommand;

        protected void PrintIt(string printingString)
        {
            if (printingString.Contains((char)160))
            {
                printingString = printingString.Replace(((char)160).ToString(), string.Empty);
            }

            if (Config.PrintInvoiceAsReceipt)
            {
                printingString = printingString.Replace("Invoice", "Receipt");
                printingString = printingString.Replace("INVOICE", "RECEIPT");
            }

            if (Config.UseFastPrinter)
            {
                PrintItInFastPrinter(printingString);
                return;
            }

            printCommand = printingString;

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

                            //extra sleep for signature
                            if (!string.IsNullOrEmpty(Config.PrinterToUse) && Config.PrinterToUse.ToLowerInvariant().Contains("datamax"))
                                Thread.Sleep(3000);
                        }
                    }
                    Logger.CreateLog("finishing printing");
                }
            }
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

        protected abstract bool PrintLines(List<string> lines);

        public abstract bool ConfigurePrinter();

        public abstract string GetLogoLabel(ref int startY, Order order);

        public abstract string IncludeSignature(Order order, List<string> lines, ref int startIndex);

        public abstract IEnumerable<string> GetUpcForProductInOrder(ref int startY, Order order, Product prod);

        public abstract IEnumerable<string> GetUpcForProductIn(ref int startY, Product prod);

        public abstract void AddExtraSpace(ref int startY, List<string> lines, int font, int spaces);

        public virtual string ToString(double d)
        {
            return d.ToCustomString();
        }

        protected virtual IEnumerable<string> GetCompanyRows(ref int startIndex)
        {
            try
            {
                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                CompanyInfo company = CompanyInfo.GetMasterCompany();

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                    startIndex += font36Separation;
                }
                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress1));
                startIndex += font18Separation;
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

        #endregion

        #region Split

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

        protected abstract IList<string> CompanyNameSplit(string name);

        protected abstract IList<string> GetClientNameSplit(string name);

        protected abstract IList<string> GetOrderDetailsRowsSplitProductName(string name);

        protected abstract IList<string> GetOrderDetailsSplitComment(string name);

        protected abstract IList<string> GetOrderSplitComment(string name);

        protected abstract IList<string> GetBottomSplitText(string text = "");

        protected abstract IList<string> GetBottomDiscountSplitText();

        protected abstract IList<string> GetDetailsRowsSplitProductNameAllowance(string name);

        protected abstract IEnumerable<string> GetLoadOrderDetailsRowSplitProductName(string name);

        protected abstract IEnumerable<string> GetAcceptLoadDetailsRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetAddInventoryDetailsRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetInventoryProdDetailsRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name);

        protected abstract IEnumerable<string> GetInventorySettlementRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetConsInvoiceDetailRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetRouteReturnRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetOpenInvoiceCommentSplit(string v);

        protected abstract IEnumerable<string> GetInvoiceDetailSplitProductName(string name);

        protected abstract IEnumerable<string> GetTransferSplitProductName(string name);

        protected abstract IEnumerable<string> GetOrderPaymentSplitComment(string comment);

        protected abstract IEnumerable<string> GetConsContractDetailRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetInventoryCountDetailsRowsSplitProductName(string name);

        protected abstract IEnumerable<string> GetAcceptedLoadSplitClientName(string clientName);

        protected abstract IEnumerable<string> SplitRefusalReportLines(string productName);

        #endregion

        public bool PrintLabels(List<Order> orders)
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
                    var upc = detail.Product.Upc.Trim();

                    if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(detail.Product.Sku.Trim()))
                        upc = detail.Product.Sku.Trim();

                    if (upc.Trim().Length > 0)
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
                                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, upc));
                                    startY += font18Separation;
                                }
                                else
                                {
                                    startY += font18Separation / 2;

                                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UPC128ForLabel], startY, upc));
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
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, upc));
                                startY += font18Separation;
                            }
                            else
                            {
                                startY += font18Separation / 2;

                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[UPC128ForLabel], startY, upc));
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

        #region Print Order

        public virtual bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
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

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && order.UniqueId != null && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            lines.AddRange(GetHeaderRowsInOneDoc(ref startY, asPreOrder, order, order.Client, printedId, payments, payments != null && payments.Sum(x => x.Amount) == balance));

            if (order.IsWorkOrder)
            {
                lines.AddRange(GetOrderAssetLabel(ref startY, order, asPreOrder, fromBatch));
                lines.AddRange(GetDetailsRowsInOneDocWork(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));
            }
            else
            {
                lines.AddRange(GetOrderLabel(ref startY, order, asPreOrder, fromBatch));
                lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && (order.UniqueId != null && x.OrderId.Contains(order.UniqueId)));
            if (payment == null)
            {
                payment = InvoicePayment.List.FirstOrDefault(x =>
                    !string.IsNullOrEmpty(x.InvoicesId) && x.InvoicesId.Contains(order.OrderId.ToString()));
            }
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order));

            if (Config.ExtraSpaceForSignature > 0)
                startY += Config.ExtraSpaceForSignature * font36Separation;

            // add the signature
            lines.AddRange(GetSignatureSection(ref startY, order, asPreOrder, lines));

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

            if (Config.SendZplOrder)
            {
                var zpl = ZebraPrinter1.ZPLFromLines(lines);

                var found = PrintedOrderZPL.PrintedOrders.FirstOrDefault(x => x.UniqueId == order.UniqueId);
                if (found != null)
                {
                    found.ZPLString = zpl;
                    found.Save();
                }
                else
                {
                    var newZPl = new PrintedOrderZPL(order.UniqueId, zpl);
                    newZPl.Save();
                }
            }

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

        #region work order
        protected virtual string GetOrderDetailSectionHeaderWork(int factor)
        {
            switch (factor)
            {
                case 1:
                    return "PARTS";
                case 2:
                    return "SERVICES";
                default:
                    return "PARTS";
            }

        }

        private IEnumerable<string> ProcessCategoryLines(ref int startY, List<OrderLine> lines, string categoryName, int factor, Order order, bool preOrder)
        {
            List<string> sectionList = new List<string>();

            // Sorting details
            var sortedDetails = SortDetails.SortedDetails(lines);
            sectionList.AddRange(GetSectionRowsInOneDocWork(ref startY, sortedDetails.ToList(), GetOrderDetailSectionHeaderWork(factor), factor, order, preOrder));
            startY += font36Separation;

            return sectionList;
        }

        protected virtual IEnumerable<string> GetSectionRowsInOneDocWork(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
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

                            d += _.Price * qty;
                        }

                        double price = detail.Price;

                        balance += d;

                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;

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
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines3], startIndex, string.Empty, remainingQty));
                    startIndex += font18Separation;

                    productLineOffset++;
                }
            }

            var s = new string('-', WidthForNormalFont);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotalWork(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance));

            return list;
        }

        protected virtual List<string> GetOrderDetailsSectionTotalWork(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            // Check if the total should be printed based on client's extra properties
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            // Ensure UoM map includes "Units" if needed
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

            // Print subtotals if configured and total should be printed
            if (!Config.HideSubTotalOrder && printTotal)
            {
                int offset = 0;
                foreach (var key in uomKeys)
                {
                    string adjustedKey = AdjustPadding(key);
                    var balanceText = ToString(balance);
                    if (offset > 0)
                        balanceText = string.Empty;

                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals1, startIndex, string.Empty, adjustedKey, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText));
                    startIndex += font18Separation;

                    offset++;
                }
            }

            return list;
        }

        public IEnumerable<string> GetOrderAssetLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch)
        {
            startY += 10;

            List<string> lines = new List<string>();

            //if (order.IsDelivery)
            //{
            //    foreach (var part in SplitProductName(order.Comments, 28, 28))
            //    {
            //        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCommentWork], startY, part));
            //        startY += font36Separation;
            //    }

            //    startY += 10;
            //}
            //else
            //{
            var asset = DataAccess.GetSingleUDF("workOrderAsset", order.ExtraFields);

            Asset assetProduct = null;

            if (!string.IsNullOrEmpty(asset))
                assetProduct = Asset.Find(asset);

            if (order.AssetId > 0)
                assetProduct = Asset.FindById(order.AssetId);

            if (assetProduct != null)
            {
                var product = Product.Find(assetProduct.ProductId);
                string docName = "Asset:" + (product != null ? product.Name : "") + " #" + asset;

                foreach (var part in SplitProductName(docName, 28, 28))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCommentWork], startY, part));
                    startY += font36Separation;
                }
            }

            startY += 10;
            //}

            return lines;
        }

        protected virtual IEnumerable<string> GetDetailsRowsInOneDocWork(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetDetailsRowsInOneDocForAllowance(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();
            list.AddRange(GetDetailTableHeader(ref startY));

            var s = new string('-', WidthForNormalFont);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            // Categorizing lines into 'Parts' and 'Service'
            var allLines = new List<OrderLine>();
            allLines.AddRange(sales.Values);
            allLines.AddRange(credit.Values);
            allLines.AddRange(returns.Values);

            var partsLines = new List<OrderLine>();
            var serviceLines = new List<OrderLine>();

            foreach (var line in allLines)
            {
                var category = Category.Categories.FirstOrDefault(x => x.CategoryId == line.Product.CategoryId);
                if (category != null)
                {
                    if (category.TypeServiPart == CategoryServiPartType.Part)
                        partsLines.Add(line);
                    else if (category.TypeServiPart == CategoryServiPartType.Services)
                        serviceLines.Add(line);
                }
            }

            // Process 'Parts'
            if (partsLines.Any())
            {
                factor = -1;
                list.AddRange(ProcessCategoryLines(ref startY, partsLines, "Parts", factor, order, preOrder));
            }

            // Process 'Service'
            if (serviceLines.Any())
            {
                factor = 2;
                list.AddRange(ProcessCategoryLines(ref startY, serviceLines, "Service", factor, order, preOrder));
            }

            return list;
        }
        #endregion

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

        protected virtual IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool asPreOrder, Order order, Client client, string printedId, List<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            bool printExtraDocName = true;
            string docName = GetOrderDocumentName(ref printExtraDocName, order, client);

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

            /*  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
              startY += font18Separation;

              var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
              if (salesman != null)
              {
                  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, salesman.Name));
                  startY += font18Separation;
              }*/
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

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
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

            if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
            {
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            return lines;
        }

        protected virtual string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            if (order.IsWorkOrder)
                return "Work Order";

            if (order.IsExchange)
                return "Exchange";

            string docName = "Invoice";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Bill";
                    printExtraDocName = true;
                }
            }
            if (order.AsPresale)
            {
                docName = "Sales Order";
                printExtraDocName = false;
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credit";
                printExtraDocName = true;
            }
            if (order.OrderType == OrderType.Return)
            {
                docName = "Return";
                printExtraDocName = true;
            }

            return docName;
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

                if (order != null && order.DepartmentId > 0)
                {
                    var dep = DepartmertClientCategory.List.FirstOrDefault(x => x.Id == order.DepartmentId);

                    if (dep != null)
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, "Department: " + dep.Name));
                        startIndex += font18Separation;
                    }
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

        protected virtual IEnumerable<string> GetPaymentLines(ref int startY, IList<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();

            if (payments.Count == 1)
            {
                if (paidInFull)
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid in Full Cash"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid in Full"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Check #:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid in Full"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Money Order #:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid in Full Credit Card"));
                            startY += font36Separation;
                            break;
                    }
                }
                else
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid " + ToString(payments[0].Amount) + "  Cash"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid  " + ToString(payments[0].Amount)));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Check " + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid  " + ToString(payments[0].Amount)));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Money Order " + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Paid " + ToString(payments[0].Amount) + "  Credit Card"));
                            startY += font36Separation;
                            break;
                    }
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (paidInFull)
                    sb.Append("Paid In Full");
                else
                    sb.Append("Paid " + ToString(payments.Sum(x => x.Amount)));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, sb.ToString()));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            string docName = "NOT AN INVOICE";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "NOT A BILL";
                }
            }

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, docName));
                    startY += font36Separation;
                }
            }
            else
            {
                bool credit = false;
                if (order != null)
                    credit = order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return;

                var orderheader = credit ? "FINAL CREDIT INVOICE" : "FINAL INVOICE";

                if (order.IsDelivery && order.Reshipped)
                    orderheader = credit ? "REFUSED CREDIT INVOICE" : "REFUSED INVOICE";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, orderheader));
                startY += font36Separation;

            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, name));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetDetailsRowsInOneDocForAllowance(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();

            list.AddRange(GetDetailTableHeader(ref startY));

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


                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(0), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected virtual IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeader];

            if (Config.HidePriceInPrintedLine)
                HidePriceInOrderPrintedLine(ref formatString);

            if (Config.HideTotalInPrintedLine)
                HideTotalInOrderPrintedLine(ref formatString);

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return lines;
        }

        protected virtual void HidePriceInOrderPrintedLine(ref string formatString)
        {
            formatString = formatString.Replace("PRICE", "");
        }

        protected virtual void HideTotalInOrderPrintedLine(ref string formatString)
        {
            formatString = formatString.Replace("TOTAL", "");
        }

        protected virtual string GetOrderDetailSectionHeader(int factor)
        {
            switch (factor)
            {
                case -1:
                    return "SALES SECTION";
                case 0:
                    return "DUMP SECTION";
                case 1:
                    return "RETURNS SECTION";
                default:
                    return "SALES SECTION";
            }
        }

        protected virtual IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            List<int> relateds = new List<int>();

            if (Config.CoolerCoCustomization)
            {
                foreach (var detail in order.Details.Where(x => x.RelatedOrderDetail > 0).ToList())
                {
                    relateds.Add(detail.RelatedOrderDetail);

                    var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                    if (!string.IsNullOrEmpty(values))
                    {
                        var parts = values.Split(",");

                        foreach (var p in parts)
                        {
                            int orderDetailId = 0;
                            Int32.TryParse(p, out orderDetailId);

                            if (orderDetailId > 0 && !relateds.Contains(orderDetailId))
                                relateds.Add(orderDetailId);
                        }
                    }
                }
            }

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

                bool printUom = true;

                if (Config.CoolerCoCustomization && relateds.Count > 0 && relateds.Contains(detail.OrderDetail.OrderDetailId))
                    printUom = false;

                string uomString = null;
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null && printUom)
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
                if (detail.OrderDetail.UnitOfMeasure != null && printUom)
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
                        
                        if (!order.AsPresale && detail.ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                        {
                            var groupedDetails = detail.ParticipatingDetails
                                .Where(od => !od.Product.IsDiscountItem)
                                .GroupBy(od => new { od.Product.ProductId, od.Price });

                            foreach (var grouped in groupedDetails)
                            {
                                var firstItem = grouped.First();

                                var copy = firstItem.GetOrderDetailCopy();
                                copy.Qty = grouped.Sum(x => x.Qty);
                                copy.Weight = grouped.Sum(x => x.Weight);
                                copy.Allowance = grouped.Sum(x => x.Allowance);
                                copy.Discount = grouped.Sum(x => x.Discount);

                                d += order.CalculateOneItemCost(copy, true);
                            }
                        }
                        else
                        {
                            foreach (var _ in detail.ParticipatingDetails)
                            {
                                d += order.CalculateOneItemCost(_, true);
                            }
    
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

        public virtual string AdjustPadding(string input, int safetyGap = 3)
        {
            var maxCharacters = SpaceForPadding;
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

        public static string DoFormat(double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);
            return s;
        }

        protected virtual List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
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

        protected virtual IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetTotalsRowsInOneDocAllowance(ref startY, client, sales, credit, returns, payment, order);

            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;

            double totalSales = 0;
            double totalCredit = 0;
            double totalReturn = 0;

            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            if (order.ConvertedInvoice)
                paid = order.Paid;

            double taxableAmount = 0;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            foreach (var key in sales.Keys)
            {
                if (!order.AsPresale && sales[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalSales, ref taxableAmount, ref salesBalance, sales[key].ParticipatingDetails, order, false);
                }
                else
                {
                    foreach (var od in sales[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalSales += qty;
                        
                        var x = order.CalculateOneItemCost(od, false);
                        salesBalance += x;

                        if (sales[key].Product.Taxable)
                            taxableAmount += x;
                    }
                }
            }
            foreach (var key in credit.Keys)
            {
                if (!order.AsPresale && credit[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalCredit, ref taxableAmount, ref creditBalance, credit[key].ParticipatingDetails, order, true);
                }
                else
                {
                    foreach (var od in credit[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalCredit += qty;

                        var x = order.CalculateOneItemCost(od, false);

                        creditBalance += x;

                        if (credit[key].Product.Taxable)
                            taxableAmount -= x;
                    }
                }
            }
            foreach (var key in returns.Keys)
            {
                if (!order.AsPresale && returns[key].ParticipatingDetails.Any(x => x.Product.SoldByWeight))
                {
                    CalculateTotalWithGroupedWeights(ref totalReturn, ref taxableAmount, ref returnBalance, returns[key].ParticipatingDetails, order, true);
                }
                else
                {
                    foreach (var od in returns[key].ParticipatingDetails)
                    {
                        if (od.Product.IsDiscountItem)
                            continue;

                        double qty = od.Qty;

                        if (od.Product.SoldByWeight)
                        {
                            if (order.AsPresale)
                                qty *= od.Product.Weight;
                            else
                                qty = od.Weight;
                        }

                        totalReturn += qty;
                        
                        var x = order.CalculateOneItemCost(od, false);

                        returnBalance += x;

                        if (returns[key].Product.Taxable)
                            taxableAmount -= x;
                    }
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");


            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (!Config.HideTotalOrder && printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s1 = ToString(salesBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s1 = ToString(creditBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s1 = ToString(returnBalance);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsReturns], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString(Math.Round((salesBalance + creditBalance + returnBalance), Config.Round));
                s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if (!order.IsWorkOrder)
                {
                    if ((order.Client.UseDiscount || order.Client.UseDiscountPerLine || order.IsDelivery || OrderDiscount.HasDiscounts) && !Config.HideDiscountTotalPrint)
                    {
                        if (Config.ShowDiscountIfApplied)
                        {
                            if (discount != 0)
                            {
                                s1 = ToString(Math.Abs(discount));
                                s1 = new string(' ', 16 - s1.Replace(")", "").Length) + s1;
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                                startY += font36Separation;
                            }
                        }
                        else
                        {
                            s1 = ToString(Math.Abs(discount));
                            s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                            startY += font36Separation;
                        }
                    }
                }

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                if (!Config.HideTaxesTotalPrint)
                {
                    s1 = ToString(tax);
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                    startY += font36Separation;
                }

                double otherCharges = 0;
                if (order.IsWorkOrder || Config.AllowOtherCharges)
                {
                    s1 = ToString(order.CalculatedOtherCharges());
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsOtherCharges], startY, s1));
                    startY += font36Separation;
                    s1 = ToString(order.CalculatedFreight());
                    s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsFreight], startY, s1));
                    startY += font36Separation;

                    otherCharges = order.CalculatedFreight() + order.CalculatedOtherCharges();
                }

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax + otherCharges;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice && !order.IsWorkOrder)
                {
                    s1 = ToString(Math.Round(paid, Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;

                    s1 = ToString(Math.Round((s4 - paid), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (Config.PrintClientTotalOpenBalance)
                {
                    s1 = ToString(Math.Round(order.Client.CurrentBalance(), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Replace(")", "").Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                if (order.IsWorkOrder)
                {
                    var comment = string.Empty;
                    var reply = string.Empty;

                    const string marker = "Reply:";
                    var parts = order.Comments.Split(new[] { marker }, StringSplitOptions.None);
                    comment = parts[0].Trim();
                    if (parts.Length > 1) reply = parts[1].Trim();


                    startY += font18Separation;
                    var clines = GetOrderSplitComment(comment);
                    for (int i = 0; i < clines.Count; i++)
                    {
                        string format = linesTemplates[OrderComment];
                        if (i > 0)
                            format = linesTemplates[OrderComment2];

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                        startY += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(reply))
                    {
                        startY += font18Separation;
                        clines = GetOrderSplitComment(reply);
                        for (int i = 0; i < clines.Count; i++)
                        {
                            string format = linesTemplates[OrderClientAddress];
                            if (i > 0)
                                format = linesTemplates[OrderClientAddress];

                            if (i == 0)
                                list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, "Reply: " + clines[i]));
                            else
                                list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                            startY += font18Separation;
                        }
                    }
                }
                else
                {
                    var orderComments = order.Comments;

                    var reasons = Reason.GetReasonsByType(ReasonType.ReShip);
                    if (reasons.Count > 0 && reasons.Any(x => x.Description == order.Comments) && !order.Reshipped && order.IsDelivery)
                        orderComments = string.Empty;

                    startY += font18Separation;
                    var clines = GetOrderSplitComment(orderComments);
                    for (int i = 0; i < clines.Count; i++)
                    {
                        string format = linesTemplates[OrderComment];
                        if (i > 0)
                            format = linesTemplates[OrderComment2];

                        list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                        startY += font18Separation;
                    }
                }
            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            return list;
        }

        public void CalculateTotalWithGroupedWeights(ref double total, ref double taxableAmount, ref double Balance, List<OrderDetail> Details, Order order, bool isCredit)
        {
            var groupedDetails = Details
                .Where(od => !od.Product.IsDiscountItem)
                .GroupBy(od => new { od.Product.ProductId, od.Price });

            foreach (var grouped in groupedDetails)
            {
                var firstItem = grouped.First();

                var copy = firstItem.GetOrderDetailCopy();
                copy.Qty = grouped.Sum(x => x.Qty);
                copy.Weight = grouped.Sum(x => x.Weight);
                copy.Allowance = grouped.Sum(x => x.Allowance);
                copy.Discount = grouped.Sum(x => x.Discount);

                var x = order.CalculateOneItemCost(copy, false);
                
                double qty = copy.Qty;

                if (copy.Product.SoldByWeight)
                {
                    if (order.AsPresale)
                        qty *= copy.Product.Weight;
                    else
                        qty = copy.Weight;
                }

                total += qty;
                Balance += x;

                var factor = isCredit ? -1 : 1;
                if (copy.Product.Taxable)
                    taxableAmount += (x * factor);
            }
        }

        protected virtual string GetOrderPreorderLabel(Order order)
        {
            return order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
        }

        protected virtual IEnumerable<string> GetFooterRows(ref int startIndex, bool asPreOrder, string CompanyName = null)
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

        protected virtual IEnumerable<string> GetSignatureSection(ref int startY, Order order, bool asPreOrder, List<string> all_lines = null)
        {
            List<string> lines = new List<string>();

            if (order.ConvertedInvoice) // test V
            {
                if (!string.IsNullOrEmpty(order.InvoiceSignature))
                {
                    string label = "^FO30," + startY.ToString() + "^GFA, " +
                                       order.InvoiceSignatureSize.ToString() + "," +
                                       order.InvoiceSignatureSize.ToString() + "," +
                                       order.InvoiceSignatureWidth.ToString() + "," +
                                       order.InvoiceSignature;



                    lines.Add(label);
                    startY += order.InvoiceSignatureHeight;
                    startY += 10;



                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                    startY += font18Separation;



                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                    startY += font36Separation;
                }

                return lines;
            }

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
                lines.AddRange(GetFooterRows(ref startY, asPreOrder, order.CompanyName));

            return lines;
        }



        protected virtual string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v4, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v4, v3);
        }

        protected virtual string GetSectionRowsInOneDocFixedLine3(string format, int pos, string v1, string v2)
        {
            if (v1.Length < 18)
                v1 += new string(' ', 18 - v1.Length);

            if (v2.Length < 10)
                v2 += new string(' ', 10 - v2.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        protected virtual string GetSectionRowsInOneDocFixedLotLine(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        protected virtual string GetOrderDetailsSectionTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region Allowance

        protected virtual IEnumerable<string> GetDetailsRowsInOneDocForAllowance(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

            string formatString = linesTemplates[AllowanceOrderDetailsHeader];

            if (Config.HidePriceInPrintedLine)
                HidePriceInOrderPrintedLine(ref formatString);

            if (Config.HideTotalInPrintedLine)
                HideTotalInOrderPrintedLine(ref formatString);

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


                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, lines.ToList(), GetOrderDetailSectionHeader(0), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDocForAllowance(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected virtual IEnumerable<string> GetSectionRowsInOneDocForAllowance(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            List<int> relateds = new List<int>();

            if (Config.CoolerCoCustomization)
            {
                foreach (var detail in order.Details.Where(x => x.RelatedOrderDetail > 0).ToList())
                {
                    relateds.Add(detail.RelatedOrderDetail);

                    var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                    if (!string.IsNullOrEmpty(values))
                    {
                        var parts = values.Split(",");

                        foreach (var p in parts)
                        {
                            int orderDetailId = 0;
                            Int32.TryParse(p, out orderDetailId);

                            if (orderDetailId > 0 && !relateds.Contains(orderDetailId))
                                relateds.Add(orderDetailId);
                        }
                    }
                }
            }

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                Product p = detail.Product;

                bool printUom = true;

                if (Config.CoolerCoCustomization && relateds.Count > 0 && relateds.Contains(detail.OrderDetail.OrderDetailId))
                    printUom = false;

                string uomString = null;
                if (detail.OrderDetail.UnitOfMeasure != null && printUom)
                {
                    uomString = detail.OrderDetail.UnitOfMeasure.Name;
                    if (!uomMap.ContainsKey(uomString))
                        uomMap.Add(uomString, 0);
                    uomMap[uomString] += detail.Qty;
                }
                else
                {
                    if (!detail.OrderDetail.SkipDetailQty(order))
                    {
                        totalQtyNoUoM += detail.Qty;
                        try
                        {
                            totalUnits += detail.Qty * Convert.ToInt32(detail.OrderDetail.Product.Package);
                        }
                        catch { }
                    }
                }

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;

                var productSlices = GetDetailsRowsSplitProductNameAllowance(name);

                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        double price = detail.Price * factor;

                        d -= (detail.OrderDetail.Allowance * detail.Qty * factor);

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);

                        if (detail.OrderDetail.UnitOfMeasure != null && printUom)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;

                        string priceAsString = ToString(price);
                        string allowance = ToString(detail.OrderDetail.Allowance);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AllowanceOrderDetailsLine], startIndex, pName, qtyAsString, priceAsString, allowance, totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                StringBuilder sb = new StringBuilder();
                List<string> lotUsed = new List<string>();

                int TotalCases = 0;

                if (!Config.PrintLotPreOrder && !Config.PrintLotOrder)
                {
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
                }

                foreach (var item in detail.ParticipatingDetails)
                {
                    StringBuilder sb1 = new StringBuilder();
                    if (!string.IsNullOrEmpty(item.Lot))
                        if (preOrder)
                        {
                            //turn off this setting
                            if (Config.PrintLotPreOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesLotQty], startIndex,
                                    item.Lot, item.Qty));
                                startIndex += font18Separation;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                                {
                                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                                    startIndex += font18Separation;
                                }
                            }
                        }
                        else
                        {
                            //turn off thi setting
                            if (Config.PrintLotOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesLotQty], startIndex,
                                    item.Lot, item.Qty));
                                startIndex += font18Separation;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                                {
                                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                                    startIndex += font18Separation;
                                }
                            }
                        }
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

                var upc = detail.Product.Upc.Trim();

                if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(detail.Product.Sku.Trim()))
                    upc = detail.Product.Sku.Trim();

                if (upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    bool printUpc = true;
                    if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString))
                    {
                        var item = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                        if (item != null && item.Item2 == "0")
                            printUpc = false;
                    }
                    if (printUpc)
                        if (Config.PrintUpcAsText)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startIndex, upc));
                            startIndex += font18Separation;
                        }
                        else
                        {
                            startIndex += font18Separation / 2;

                            var upcTemp = Config.UseUpc128 ? linesTemplates[Upc128] : linesTemplates[OrderDetailsLinesUpcBarcode];

                            if (upc.Length > 12 && !Config.UseUpc128)
                                upcTemp = linesTemplates[OrderDetailsLinesLongUpcBarcode];

                            list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startIndex, Product.GetFirstUpcOnly(upc)));
                            startIndex += font36Separation * 2;
                        }
                }

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
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

        protected IList<string> GetDetailsRowsSplitProductName1(string name)
        {
            return SplitProductName(name, 33, 33);
        }




        protected virtual IEnumerable<string> GetTotalsRowsInOneDocAllowance(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;

            double totalSales = 0;
            double totalCredit = 0;
            double totalReturn = 0;

            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            double taxableAmount = 0;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            foreach (var key in sales.Keys)
            {
                foreach (var od in sales[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;

                    totalSales += qty;

                    var salesTotalLine = od.Price * factor * qty;

                    salesBalance += (salesTotalLine - (od.Allowance * od.Qty));

                    if (sales[key].Product.Taxable)
                        taxableAmount += salesTotalLine;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalCredit += qty;

                    var creditTotalLine = od.Price * factor * qty * -1;

                    creditBalance += (creditTotalLine + (od.Allowance * od.Qty));

                    if (credit[key].Product.Taxable)
                        taxableAmount -= creditTotalLine;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Product.SoldByWeight ? od.Weight : od.Qty;
                    totalReturn += qty;

                    var returnTotalLine = od.Price * qty * factor * -1;

                    returnBalance += (returnTotalLine + (od.Allowance * od.Qty));

                    if (returns[key].Product.Taxable)
                        taxableAmount -= returnTotalLine;
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (!Config.HideTotalOrder && printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (Math.Round(totalSales - totalCredit - totalReturn), Config.Round).ToString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font36Separation;
                }

                if (salesBalance > 0)
                {
                    s1 = ToString(salesBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                    startY += font36Separation;
                }

                if (creditBalance != 0)
                {
                    s1 = ToString(creditBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                    startY += font36Separation;
                }

                if (returnBalance != 0)
                {
                    s1 = ToString(returnBalance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsReturns], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString((salesBalance + creditBalance + returnBalance));
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if ((order.Client.UseDiscount || order.Client.UseDiscountPerLine || order.IsDelivery) && !Config.HideDiscountTotalPrint)
                {
                    if (Config.ShowDiscountIfApplied)
                    {
                        if (discount != 0)
                        {
                            s1 = ToString(Math.Abs(discount));
                            s1 = new string(' ', 14 - s1.Length) + s1;
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                            startY += font36Separation;
                        }
                    }
                    else
                    {
                        s1 = ToString(Math.Abs(discount));
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                        startY += font36Separation;
                    }
                }

                double tax = Math.Round(taxableAmount * order.TaxRate, 3);
                if (tax > 0 && !Config.HideTaxesTotalPrint)
                {
                    var s = Config.PrintTaxLabel;
                    if (Config.PrintTaxLabel.Length < 16)
                        s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                    s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                    s1 = ToString(tax);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s + s1));
                    startY += font36Separation;
                }

                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice && !order.IsWorkOrder)
                {
                    s1 = ToString(paid);
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;

                    double result = s4 - paid;
                    result = Math.Round(result, Config.Round);
                    s1 = ToString(result);
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                var orderComments = order.Comments;

                var reasons = Reason.GetReasonsByType(ReasonType.ReShip);
                if (reasons.Count > 0 && reasons.Any(x => x.Description == order.Comments) && !order.Reshipped && order.IsDelivery)
                    orderComments = string.Empty;

                startY += font18Separation;
                var clines = GetOrderSplitComment(orderComments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string format = linesTemplates[OrderComment];
                    if (i > 0)
                        format = linesTemplates[OrderComment];

                    list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                    startY += font18Separation;
                }

            }
            return list;
        }

        #endregion

        #region Shortage Report

        protected virtual void PrintShortageReport(Order order)
        {
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportHeader], startY));
            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, string.Empty));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], order.Client.ClientName, startY));
            startY += font36Separation;
            foreach (string s in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], s.Trim(), startY));
                startY += font18Separation;
            }
            startY += font36Separation;

            if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportInvoiceHeader], startY, printedId));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId));
            startY += font36Separation + font18Separation;

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                startY += font36Separation;
            }
            // add the details

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportTableHeader], startY));
            startY += font18Separation;
            foreach (var detail in order.Details)
                if (detail.Ordered != 0 && detail.Ordered != detail.Qty)
                {
                    var p = GetOrderDetailsRowsSplitProductName(detail.Product.Name);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportTableLine],
                                            startY, p[0], detail.Ordered, (detail.Ordered - detail.Qty), detail.Qty));
                    startY += font18Separation;
                }

            foreach (var detail in order.DeletedDetails)
                if (detail.Ordered != 0)
                {
                    var p = GetOrderDetailsRowsSplitProductName(detail.Product.Name);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ShortageReportTableLine],
                                            startY, p[0], detail.Ordered, detail.Ordered, 0));
                    startY += font18Separation;
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
                    foreach (var line in GetBottomSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }

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
            }
            else
                lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            PrintLines(lines);
        }

        #endregion

        #region Print Load

        public bool PrintOrderLoad(bool isFinal)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            if (!string.IsNullOrEmpty(LoadOrder.PrintedOrderId))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, "Load Order#: " + LoadOrder.PrintedOrderId));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderHeader], startY));
                startY += font36Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (LoadOrder.Date.Year > DateTime.MinValue.Year)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderRequestedDate], startY, LoadOrder.Date));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (LoadOrder.SiteId > 0)
            {
                var site = SiteEx.Find(LoadOrder.SiteId);
                if (site != null)
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Warehouse: " + site.Name));
                    startY += font18Separation;
                }
            }

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinal], startY));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetLoadOrderDetailsTable(ref startY));

            startY += font18Separation;

            double totalPrice = 0;
            foreach (var detail in LoadOrder.List)
            {
                var price = detail.Product.PriceLevel0 * detail.Qty;

                double factor = 1;

                if (detail.UoM != null)
                    factor = detail.UoM.Conversion;

                price *= factor;

                totalPrice += price;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Order Total: " + totalPrice.ToCustomString()));

            AddExtraSpace(ref startY, lines, font18Separation, 1);
            startY += font18Separation;


            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderNotFinal], startY));
                startY += font36Separation;
            }

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetLoadOrderDetailsTable(ref int startY)
        {
            List<string> lines = new List<string>();

            float dumpBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in SortDetails.SortedDetails(LoadOrder.List))
            {
                var uom = p.UoM != null ? p.UoM.Name : "";

                int productLineOffset = 0;
                foreach (string pName in GetLoadOrderDetailsRowSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                        lines.Add(GetLoadOrderTableLineFixed(LoadOrderTableLine, startY, pName, uom, Math.Round(p.Qty, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    else
                        lines.Add(GetLoadOrderTableLineFixed(LoadOrderTableLine, startY, pName, "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }
                dumpBoxes += Convert.ToSingle(p.Qty);
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            if (SortDetails.SortedDetails(LoadOrder.List).All(x => x.UoM == null))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[LoadOrderTableTotal], startY, Math.Round(dumpBoxes, Config.Round).ToString(CultureInfo.CurrentCulture)));
                startY += font18Separation;
            }

            return lines;
        }

        protected virtual string GetLoadOrderTableLineFixed(string format, int pos, string value1, string value2, string value3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, value1, value2, value3);
        }

        #endregion

        #region Accept Load On Demand

        public bool PrintAcceptLoad(IEnumerable<InventoryLine> SortedList, string docNumber, bool final)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetAcceptLoadHeaderRows(ref startY, docNumber));

            if (!final)
            {
                AddExtraSpace(ref startY, lines, font18Separation, 1);
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetAcceptLoadDetailsRows(ref startY, SortedList));

            if (!final)
            {
                AddExtraSpace(ref startY, lines, font18Separation, 1);
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetAcceptLoadHeaderRows(ref int startIndex, string docNumber)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 10, 1);

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadHeader], startIndex));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadDate], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startIndex, Config.RouteName));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startIndex, Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            AddExtraSpace(ref startIndex, list, font36Separation, 1);


            //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadInvoice], startIndex, docNumber));
            //startIndex += font36Separation;

            return list;
        }

        public class InvLineGrouped
        {
            public Product Product { get; set; }

            public float Starting { get; set; }
            public float Real { get; set; }

            public UnitOfMeasure UoM { get; set; }

            public string Lot { get; set; }

            public string Weights { get; set; }

        }

        protected virtual IEnumerable<string> GetAcceptLoadDetailsRows(ref int startIndex, IEnumerable<InventoryLine> sortedList)
        {

            var oldset = font18Separation;
            font18Separation = font18Separation + font18Separation / 2;

            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 40, 1);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadTableHeader], startIndex));
            startIndex += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadTableHeader1], startIndex));
            startIndex += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;

            float requestedNoConversion = 0;
            float adjustmentNoConversion = 0;
            float startNoConversion = 0;

            #region new
            List<InvLineGrouped> grouped = new List<InvLineGrouped>();

            foreach (var a in sortedList)
            {
                var alreadyThere = grouped.FirstOrDefault(x => x.Product == a.Product && x.Lot == a.Lot && x.UoM == a.UoM);

                if (alreadyThere != null)
                {
                    alreadyThere.Starting += a.Starting;
                    alreadyThere.Real += a.Real;

                    if (string.IsNullOrEmpty(alreadyThere.Weights))
                        alreadyThere.Weights = "Weight: " + a.Weight;
                    else
                        alreadyThere.Weights += ", " + a.Weight;
                }
                else
                {
                    grouped.Add(new InvLineGrouped() { Product = a.Product, Lot = a.Lot, Real = a.Real, Starting = a.Starting, UoM = a.UoM, Weights = ("Weight: " + a.Weight.ToString()) });

                }
            }

            if (Config.NewAddItemRandomWeight)
            {

                foreach (var p in grouped)
                {
                    int productLineOffset = 0;
                    foreach (string pName in GetAcceptLoadDetailsRowsSplitProductName(p.Product.Name))
                    {
                        var lfy = p.Product.BeginigInventory;
                        var load = p.Starting;
                        var adj = p.Real - p.Starting;
                        float factor = 1;
                        if (p.Product.SoldByWeight)
                            factor = p.Starting;
                        var st = (p.Product.BeginigInventory * factor);

                        string uom = string.Empty;

                        if (p.UoM != null)
                        {
                            lfy /= p.UoM.Conversion;
                            st /= p.UoM.Conversion;

                            uom = p.UoM.Name;
                        }

                        st += p.Real;

                        //new
                        uom = string.Empty;

                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                Math.Round(load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(st, Config.Round).ToString(CultureInfo.CurrentCulture)));

                            leftFromYesterday += p.Product.BeginigInventory;

                            var real = p.Real;

                            requestedNoConversion += load;
                            adjustmentNoConversion += adj;
                            startNoConversion += (p.Product.BeginigInventory * factor) + real;

                            if (p.UoM != null)
                            {
                                load *= p.UoM.Conversion;
                                adj *= p.UoM.Conversion;
                                real *= p.UoM.Conversion;
                            }

                            requestedInventory += load;
                            adjustment += adj;

                            start += ((p.Product.BeginigInventory * factor) + real);
                        }
                        else
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                "",
                                "",
                                "",
                                ""));
                        }

                        productLineOffset++;
                        startIndex += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(p.Lot))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadLotLine], startIndex, p.Lot));
                        startIndex += font18Separation;
                    }

                    if (Config.UsePallets && p.Product.SoldByWeight && !string.IsNullOrEmpty(p.Weights))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadWeightLine], startIndex, p.Weights));
                        startIndex += font18Separation;
                    }
                }
            }
            else
            {
                foreach (var p in sortedList)
                {
                    int productLineOffset = 0;

                    int uomCount = 0;

                    List<string> uomStrings = new List<string>();

                    if (p.UoM != null)
                    {

                        var stringToHandle = p.UoM.Name;

                        if (stringToHandle.Length > 5)
                        {
                            var firstPart = stringToHandle.Substring(0, 5);
                            var secondPart = stringToHandle.Substring(5);

                            uomStrings.Add(firstPart);
                            uomStrings.Add(secondPart);
                        }
                        else
                            uomStrings.Add(p.UoM.Name);
                    }

                    foreach (string pName in GetAcceptLoadDetailsRowsSplitProductName(p.Product.Name))
                    {
                        var lfy = p.Product.BeginigInventory;
                        var load = p.Starting;
                        var adj = p.Real - p.Starting;
                        var st = p.Product.BeginigInventory;

                        string uom = string.Empty;

                        if (p.UoM != null)
                        {
                            lfy /= p.UoM.Conversion;
                            st /= p.UoM.Conversion;
                        }

                        if (uomStrings.Count > uomCount)
                            uom = uomStrings[uomCount];

                        st += p.Real;

                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                Math.Round(load, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(st, Config.Round).ToString(CultureInfo.CurrentCulture)));

                            leftFromYesterday += p.Product.BeginigInventory;

                            var real = p.Real;

                            if (p.UoM != null)
                            {
                                load *= p.UoM.Conversion;
                                adj *= p.UoM.Conversion;
                                real *= p.UoM.Conversion;
                            }

                            requestedInventory += load;
                            adjustment += adj;

                            start += (p.Product.BeginigInventory + real);
                        }
                        else
                        {
                            list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, pName,
                                uom,
                                "",
                                "",
                                ""));
                        }

                        productLineOffset++;
                        startIndex += font18Separation;
                        uomCount++;
                    }

                    if (!string.IsNullOrEmpty(p.Lot))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadLotLine], startIndex, p.Lot));
                        startIndex += font18Separation;
                    }

                    if (Config.UsePallets && p.Product.SoldByWeight)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadWeightLine], startIndex, ("Weight: " + p.Weight).ToString()));
                        startIndex += font18Separation;
                    }
                }

            }

            #endregion


            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            if (grouped.Any(x => x.UoM != null && x.UoM.Conversion > 1))
            {
                list.Add(GetAcceptLoadTableTotalsFixed(AcceptLoadTableTotals1, startIndex,
                     Math.Round(requestedNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture),
                     Math.Round(adjustmentNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture),
                     Math.Round(startNoConversion, Config.Round).ToString(CultureInfo.CurrentCulture)));

                startIndex += font18Separation;

            }

            list.Add(GetAcceptLoadTableTotalsFixed(AcceptLoadTableTotals, startIndex,
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));


            var totalWeight = sortedList.Sum(x => (x.Weight * x.Real));
            if (totalWeight > 0)
            {
                list.Add(GetAcceptLoadTableLineFixed(AcceptLoadTableLine, startIndex, "Total Weight: " + totalWeight,
                               "",
                               "",
                               "",
                               ""));
            }

            startIndex += font18Separation;


            font18Separation = oldset;

            return list;
        }

        protected virtual string GetAcceptLoadTableLineFixed(string format, float pos, string v1, string v2, string v3, string v4, string v5)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected virtual string GetAcceptLoadTableTotalsFixed(string format, float pos, string v1, string v2, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Accept Load

        public bool PrintAddInventory(IEnumerable<InventoryLine> SortedList, bool final)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetAddInventoryHeaders(ref startY));

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetAddInventoryTable(ref startY, SortedList));

            if (!final)
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetAddInventoryHeaders(ref int startIndex)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 10, 1);

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryHeader], startIndex));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryDate], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startIndex, Config.RouteName));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startIndex, Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetAddInventoryTable(ref int startIndex, IEnumerable<InventoryLine> SortedList)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 40, 1);

            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryTableHeader], startIndex));
            startIndex += font18Separation;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AddInventoryTableHeader1], startIndex));
            startIndex += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            float leftFromYesterday = 0;
            float requestedInventory = 0;
            float adjustment = 0;
            float start = 0;

            foreach (var p in SortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetAddInventoryDetailsRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(GetAddInventoryTableLineFixed(AddInventoryTableLine, startIndex,
                            pName,
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
                        list.Add(GetAddInventoryTableLineFixed(AddInventoryTableLine, startIndex,
                            pName, "", "", "", ""));
                    }

                    productLineOffset++;
                    startIndex += font18Separation;
                }
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.Add(GetAddInventoryTableTotalsFixed(AddInventoryTableTotals, startIndex,
                 Math.Round(leftFromYesterday, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(requestedInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(adjustment, Config.Round).ToString(CultureInfo.CurrentCulture),
                 Math.Round(start, Config.Round).ToString(CultureInfo.CurrentCulture)
            ));
            startIndex += font18Separation;

            return list;
        }

        protected virtual string GetAddInventoryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5);
        }

        protected virtual string GetAddInventoryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region View/Print Inventory

        public bool PrintInventoryProd(List<InventoryProd> SortedList)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetInventoryProdHeaders(ref startY, SortedList));

            lines.AddRange(GetInventoryProdTable(ref startY, SortedList));

            lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetInventoryProdHeaders(ref int startIndex, List<InventoryProd> sortedList)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 10, 1);

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryProdHeader], startIndex, DateTime.Now.ToString()));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startIndex, Config.RouteName));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startIndex, Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetInventoryProdTable(ref int startIndex, List<InventoryProd> sortedList)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 40, 1);

            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryProdTableHeader], startIndex));
            startIndex += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            AddExtraSpace(ref startIndex, list, font18Separation, 1);

            float numberOfBoxes = 0;
            double value = 0;

            foreach (var prod in sortedList)
            {
                var p = prod.Product;

                float startInv = p.BeginigInventory;

                int productLineOffset = 0;
                foreach (string pName in GetInventoryProdDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(GetInventoryProdTableLineFixed(InventoryProdTableLine, startIndex,
                            pName,
                            Math.Round(startInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                            Math.Round(prod.Qty, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        list.Add(GetInventoryProdTableLineFixed(InventoryProdTableLine, startIndex,
                            pName, "", ""));

                    productLineOffset++;
                    startIndex += font18Separation;
                }

                if (Config.UsePairLotQty)
                {
                    foreach (var lot in prod.ProdLots)
                    {
                        if (lot == null)
                            continue;

                        list.Add(GetInventoryProdTableLineLotFixed(InventoryProdTableLineLot, startIndex,
                            lot.Lot,
                             Math.Round(lot.BeginingInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                             Math.Round(lot.CurrentQty, Config.Round).ToString(CultureInfo.CurrentCulture)));

                        startIndex += font18Separation;
                    }
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryProdTableLineListPrice], startIndex, ToString(p.PriceLevel0), ToString((p.CurrentInventory * p.PriceLevel0))));
                startIndex += font18Separation;

                list.AddRange(GetUpcForProductIn(ref startIndex, p));

                numberOfBoxes += Convert.ToSingle(prod.Qty);
                value += prod.Qty * p.PriceLevel0;

                AddExtraSpace(ref startIndex, list, font18Separation + orderDetailSeparation, 1);
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            string s1;

            s1 = Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            if (s1.Length > 14)
                s1 = s1.Substring(0, 11) + "...";
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryProdQtyItems], startIndex, s1));
            startIndex += font36Separation;

            s1 = ToString(value);
            if (s1.Length > 14)
                s1 = s1.Substring(0, 11) + "...";
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryProdInvValue], startIndex, s1));
            startIndex += font36Separation;

            return list;
        }

        protected virtual string GetInventoryProdTableLineFixed(string format, int pos, string v1, string v2, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        protected virtual string GetInventoryProdTableLineLotFixed(string format, int pos, string v1, string v2, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }


        #endregion

        #region Print Payment Batch

        public bool PrintPaymentBatch()
        {
            List<string> lines = new List<string>();

            var deposit = BankDeposit.currentDeposit;

            if (deposit == null)
                return false;

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, null);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            if (deposit.PostedDate != DateTime.MinValue)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchDate], startY, deposit.PostedDate.ToShortDateString()));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchSalesman], startY, salesman.Name));
            startY += font18Separation;

            var bank = BankAccount.List.FirstOrDefault(x => x.Id == deposit.bankAccountId);
            if (bank != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchBank], startY, bank.Name));
                startY += font18Separation;
            }

            startY += font18Separation;

            var checks = new List<PaymentComponent>();
            var cash = new List<PaymentComponent>();
            var credit_card = new List<PaymentComponent>();
            var moneyOrder = new List<PaymentComponent>();

            //fill lists
            foreach (var p in deposit.Payments)
            {
                foreach (var c in p.Components)
                {
                    switch (c.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            if (!checks.Contains(c))
                                checks.Add(c);
                            break;
                        case InvoicePaymentMethod.Cash:
                            if (!cash.Contains(c))
                                cash.Add(c);
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            if (!credit_card.Contains(c))
                                credit_card.Add(c);
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            if (!moneyOrder.Contains(c))
                                moneyOrder.Add(c);
                            break;
                    }
                }
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            if (checks.Count > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ChecksTitle], startY));
                startY += font18Separation;
                startY += 15;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableHeader], startY));
                startY += 15;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += 20;
            }

            foreach (var check in checks)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableLine], startY, check.Ref, check.Amount.ToCustomString()));
                startY += font18Separation;
            }

            if (checks.Count > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += 20;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CheckTableTotal], startY, checks.Count().ToString(), checks.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CashTotalLine], startY, cash.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditCardTotalLine], startY, credit_card.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MoneyOrderTotalLine], startY, moneyOrder.Sum(x => x.Amount).ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchTotal], startY, deposit.TotalAmount.ToCustomString()));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchComments], startY, deposit.Comment));
            startY += font18Separation;


            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += 12;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        #endregion

        #region Orders Created Report

        public bool PrintOrdersCreatedReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetOrderCreatedReportHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetOrderCreatedReportTable(ref startY));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetOrderCreatedReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            if (Config.UseClockInOut)
            {
                #region Deprecated

                //DateTime startOfDay = Config.FirstDayClockIn;
                //TimeSpan tsio = Config.WorkDay;
                //DateTime lastClockOut = Config.DayClockOut;
                //var wholeday = lastClockOut.Subtract(startOfDay);
                //var rested = wholeday.Subtract(tsio);

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporWorkDay], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), tsio.Hours, tsio.Minutes));
                //startY += font18Separation;

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporBreaks], startY, rested.Hours, rested.Minutes));
                //startY += font18Separation;

                #endregion

                DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                var wholeday = lastClockOut.Subtract(startOfDay);
                var breaks = SalesmanSession.GetTotalBreaks();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporWorkDay], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), wholeday.Hours, wholeday.Minutes));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporBreaks], startY, breaks.Hours, breaks.Minutes));
                startY += font18Separation;
            }

            return lines;
        }

        protected virtual IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotalTerm = 0;
            double chargeTotalTerm = 0;
            double subtotal = 0;
            double totalTax = 0;

            double paidTotal = 0;
            double chargeTotal = 0;
            double creditTotal = 0;
            double salesTotal = 0;
            double billTotal = 0;

            double netTotal = 0;

            var payments = GetPaymentsForOrderCreatedReport();

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
            {
                if (!Config.IncludePresaleInSalesReport && order.AsPresale)
                    continue;

                var terms = order.Term.Trim();

                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:
                        //if (!string.IsNullOrEmpty(terms))
                        //{
                        //    if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                        //        || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                        //        cashTotalTerm += order.OrderTotalCost();
                        //    else
                        //        chargeTotalTerm += order.OrderTotalCost();
                        //}
                        //else
                        //    chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.Quote:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Return:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                }
            }

            foreach (var b in Batch.List.OrderBy(x => x.ClockedIn))
                foreach (var p in b.Orders())
                {
                    if (!Config.PrintNoServiceInSalesReports && p.OrderType == OrderType.NoService)
                        continue;

                    if (!Config.IncludePresaleInSalesReport && p.AsPresale && p.OrderType != OrderType.NoService)
                        continue;

                    var orderCost = p.OrderTotalCost();

                    string totalCostLine = ToString(orderCost);
                    string subTotalCostLine = totalCostLine;

                    int productLineOffset = 0;
                    foreach (string pName in GetOrderCreatedReportRowSplitProductName(p.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
                            string status = GetCreatedOrderStatus(p);

                            double paid = 0;

                            var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);
                            if (payment != null)
                            {
                                double amount = payment.Amount;
                                paid = double.Parse(Math.Round(amount, Config.Round).ToCustomString(), NumberStyles.Currency);
                            }

                            string type = GetCreatedOrderType(p, paid, orderCost);

                            if (!p.Reshipped && !p.Voided && p.OrderType != OrderType.Quote)
                            {
                                if (orderCost < 0)
                                    creditTotal += orderCost;
                                else
                                {
                                    if (p.OrderType != OrderType.Bill)
                                    {
                                        salesTotal += orderCost;

                                        if (paid == 0)
                                            chargeTotal += orderCost;
                                        else
                                        {
                                            paidTotal += paid;
                                            chargeTotal += orderCost - paid;
                                        }
                                    }
                                    else
                                        billTotal += orderCost;
                                }
                            }
                            else
                                type = string.Empty;

                            float qty = 0;
                            foreach (var item in p.Details)
                                if (!item.SkipDetailQty(p))
                                    qty += item.Qty;

                            if (Config.SalesRegReportWithTax)
                                subTotalCostLine = ToString(p.CalculateItemCost());
                            else
                                totalCostLine = string.Empty;

                            lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    pName,
                                                    status,
                                                    qty.ToString(),
                                                    p.PrintedOrderId,
                                                    subTotalCostLine,
                                                    type));
                        }
                        else
                        {
                            if (productLineOffset == 1)
                                lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    pName,
                                                    "", "", "", totalCostLine, ""));
                            else
                                lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    pName,
                                                    "", "", "", "", ""));
                        }

                        productLineOffset++;
                        startY += font18Separation;
                    }

                    if (productLineOffset == 1 && !string.IsNullOrEmpty(totalCostLine))
                    {
                        lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    string.Empty, "", "", "", totalCostLine, ""));
                        startY += font18Separation;
                    }

                    AddExtraSpace(ref startY, lines, 10, 1);

                    if (!string.IsNullOrEmpty(p.Term))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableTerms], startY,
                        p.Term));
                        startY += font18Separation;
                    }

                    var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLine1], startY,
                        batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies));
                    startY += font18Separation;

                    if (p.OrderType == OrderType.NoService && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLineComment], startY, p.Comments));
                        startY += font18Separation;
                    }
                    if (p.Reshipped && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLineComment1], startY, p.Comments));
                        startY += font18Separation;
                    }

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;

                    if (p.Voided)
                        voided++;
                    else if (p.Reshipped)
                        reshipped++;
                    else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                        delivered++;
                    else
                        dsd++;

                    AddExtraSpace(ref startY, lines, font18Separation, 1);
                }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font36Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditTotal, startY, "", ToString(creditTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportBillTotal, startY, "", ToString(billTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportSalesTotal, startY, "", ToString(salesTotal)));
            startY += font18Separation;

            if (Config.SalesRegReportWithTax)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportSubtotal], startY, ToString(subtotal)));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTax], startY, ToString(totalTax)));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTotals], startY, ToString((paidTotal + chargeTotal))));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportExpectedCash, startY, ToString(cashTotalTerm), reshipped.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportPaidCust, startY, ToString(paidTotal), voided.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportChargeCust, startY, ToString(chargeTotal), delivered.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditCust, startY, "", dsd.ToString()));
            startY += font18Separation;

            if (start == DateTime.MaxValue)
                start = end;

            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);

            netTotal = Math.Round(salesTotal - Math.Abs(creditTotal), Config.Round);

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportFullTotal, startY, Config.SalesReportTotalCreditsSubstracted ? ToString(netTotal) : ToString(salesTotal), totalTime));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }

        protected virtual string GetCreatedOrderStatus(Order o)
        {
            string status = string.Empty;

            if (o.OrderType == OrderType.NoService)
                status = "NS";
            if (o.Voided)
                status = "VD";
            if (o.Reshipped)
                status = "RF";

            if (o.OrderType == OrderType.Bill)
                status = "Bi";

            if (o.OrderType == OrderType.Quote)
                status = "QT";

            return status;
        }

        protected virtual string GetCreatedOrderType(Order o, double paid, double orderCost)
        {
            string type = "";
            if (paid == 0)
                type = "-----";
            else if (paid < orderCost)
                type = "Partial P.";
            else
                type = "Paid";

            if (o.OrderTotalCost() < 0)
                type = "Credit";

            if (o.OrderType == OrderType.Quote)
                return string.Empty;

            return type;
        }

        protected virtual string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        protected virtual string GetOrderCreatedReportTotalsFixed(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2);
        }

        protected List<DataAccess.PaymentSplit> GetPaymentsForOrderCreatedReport()
        {
            List<DataAccess.PaymentSplit> result = new List<DataAccess.PaymentSplit>();

            foreach (var payment in InvoicePayment.List)
                result.AddRange(DataAccess.SplitPayment(payment));

            return result;
        }

        #endregion

        #region Payments Report

        public class PaymentRow
        {
            public string ClientName { get; set; }
            public string DocNumber { get; set; }
            public string DocAmount { get; set; }
            public string Paid { get; set; }
            public string PaymentMethod { get; set; }
            public string RefNumber { get; set; }

            public string ClientAccount { get; set; }
            public double DocAmountNumber { get; set; }
            public double PaidAmountNumber { get; set; }

            public string UniqueId { get; set; }

        }

        static string ConcatPaymentTypes(InvoicePayment payment)
        {
            List<string> types = new List<string>();
            foreach (var c in payment.Components)
            {
                string st = string.Empty;
                switch (c.PaymentMethod)
                {
                    case InvoicePaymentMethod.Cash:
                        st = "CA";
                        break;
                    case InvoicePaymentMethod.Check:
                        st = "CH";
                        break;
                    case InvoicePaymentMethod.Credit_Card:
                        st = "CC";
                        break;
                    case InvoicePaymentMethod.Money_Order:
                        st = "MO";
                        break;
                    case InvoicePaymentMethod.Transfer:
                        st = "TR";
                        break;
                    case InvoicePaymentMethod.Zelle_Transfer:
                        st = "ZE";
                        break;
                }
                if (!types.Contains(st))
                    types.Add(st);
            }
            return string.Join(",", types);
        }

        protected string ReducePaymentMethod(InvoicePaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case InvoicePaymentMethod.Cash:
                    return "CA";
                case InvoicePaymentMethod.Check:
                    return "CH";
                case InvoicePaymentMethod.Credit_Card:
                    return "CC";
                case InvoicePaymentMethod.Money_Order:
                    return "MO";
                case InvoicePaymentMethod.Transfer:
                    return "TR";
                case InvoicePaymentMethod.Zelle_Transfer:
                    return "ZE";
            }
            return string.Empty;
        }

        public virtual bool PrintReceivedPaymentsReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetPaymentReportHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            double totalCash = 0;
            double totalCheck = 0;
            double totalcc = 0;
            double totalmo = 0;
            double totaltr = 0;
            double total = 0;

            var rows = CreatePaymentReceivedDataStructure(ref totalCash, ref totalCheck, ref totalcc, ref totalmo, ref totaltr, ref total);

            lines.AddRange(GetPaymentsReportTable(ref startY, rows));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetPaymentsReportTotals(ref startY, totalCash, totalCheck, totalcc, totalmo, totaltr, total));

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentSignatureText], startY));
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetPaymentReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        public virtual List<PaymentRow> CreatePaymentReceivedDataStructure(ref double totalCash, ref double totalCheck, ref double totalcc, ref double totalmo, ref double totaltr, ref double total)
        {
            List<PaymentRow> rows = new List<PaymentRow>();

            var listToUse = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;
            foreach (var pay in listToUse)
            {
                int index = 0;
                List<string> docNumbers = pay.Invoices().Select(x => x.InvoiceNumber).ToList();
                if (docNumbers.Count == 0)
                    docNumbers = pay.Orders().Select(x => x.PrintedOrderId).ToList();
                else
                    docNumbers.AddRange(pay.Orders().Select(x => x.PrintedOrderId).ToList());

                var t = pay.Invoices().Sum(x => x.Balance);
                t += pay.Orders().Sum(x => x.OrderTotalCost());

                while (true)
                {
                    var row = new PaymentRow();
                    if (index == 0)
                    {
                        row.ClientName = pay.Client.ClientName;

                        int factor = 0;
                        if (pay.Voided)
                            factor = 6;

                        if (row.ClientName.Length > (28 - factor))
                            row.ClientName = row.ClientName.Substring(0, (27 - factor));

                        row.DocAmount = ToString(t);
                    }
                    else
                    {
                        row.ClientName = string.Empty;
                        row.DocAmount = string.Empty;
                    }
                    if (docNumbers.Count > index)
                        row.DocNumber = docNumbers[index];
                    else
                        row.DocNumber = string.Empty;
                    if (pay.Components.Count > index)
                    {
                        if (!pay.Voided)
                        {
                            if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Cash)
                                totalCash += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Check)
                                totalCheck += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Credit_Card)
                                totalcc += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Money_Order)
                                totalmo += pay.Components[index].Amount;
                            else
                                totaltr += pay.Components[index].Amount;

                            total += pay.Components[index].Amount;
                        }

                        if (pay.Voided)
                            row.ClientName += "(Void)";

                        row.RefNumber = pay.Components[index].Ref;
                        var s = ToString(pay.Components[index].Amount);
                        //if (s.Length < 9)
                        //    s = new string(' ', 9 - s.Length) + s;
                        row.Paid = s;
                        row.PaymentMethod = ReducePaymentMethod(pay.Components[index].PaymentMethod);
                    }
                    else
                    {
                        row.RefNumber = string.Empty;
                        row.Paid = string.Empty;
                        row.PaymentMethod = string.Empty;
                    }
                    rows.Add(row);

                    index++;
                    if (docNumbers.Count <= index && pay.Components.Count <= index)
                        break;
                }
            }

            return rows;
        }

        protected virtual IEnumerable<string> GetPaymentsReportTable(ref int startY, List<PaymentRow> rows)
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
                                p.PaymentMethod,
                                p.RefNumber));
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
        }

        protected virtual IEnumerable<string> GetPaymentsReportTotals(ref int startY, double totalCash, double totalCheck, double totalcc, double totalmo, double totaltr, double total)
        {
            List<string> lines = new List<string>();

            if (totalCash > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalCash], startY, ToString(totalCash)));
                startY += font18Separation;
            }

            if (totalCheck > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalCheck], startY, ToString(totalCheck)));
                startY += font18Separation;
            }

            if (totalcc > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalCC], startY, ToString(totalcc)));
                startY += font18Separation;
            }

            if (totalmo > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalMoneyOrder], startY, ToString(totalmo)));
                startY += font18Separation;
            }

            if (totaltr > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalTransfer], startY, ToString(totaltr)));
                startY += font18Separation;
            }

            if (total > 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTotalTotal], startY, ToString(total)));
                startY += font18Separation;
            }

            startY += font18Separation;

            return lines;
        }

        protected virtual string GetPaymentReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Print Inventory Summary
        public bool InventorySummary(int index, int count, bool isBase = true)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetInventorySummaryHeader(ref startY, index, count));

            InventorySettlementRow totalRow = new InventorySettlementRow();

            List<InventorySettlementRow> map = new List<InventorySettlementRow>();

            CreateInventorySummaryDataStructure(ref totalRow, ref map);

            lines.AddRange(GetInventorySummaryTable(ref startY, map, totalRow, isBase));

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual List<string> GetInventorySummaryHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySummaryHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        void CreateInventorySummaryDataStructure(ref InventorySettlementRow totalRow, ref List<InventorySettlementRow> map)
        {
            map = DataAccess.ExtendedSendTheLeftOverInventory(false, true);

            foreach (var value in map)
            {
                if (Math.Round(value.EndInventory, Config.Round) == 0)
                    continue;

                var product = value.Product;

                float factor = 1;
                if (value.UoM != null)
                    factor = value.UoM.Conversion;

                value.BegInv /= factor;
                value.LoadOut /= factor;
                value.Adj /= factor;
                value.TransferOn /= factor;
                value.TransferOff /= factor;
                value.Sales /= factor;
                value.CreditDump /= factor;
                value.CreditReturns /= factor;
                value.DamagedInTruck /= factor;
                value.Unload /= factor;
                value.EndInventory /= factor;

                totalRow.Product = product;
                //totalRow.BegInv += product.LeftFromYesterday;
                //totalRow.LoadOut += product.RequestedInventory;
                //totalRow.Adj += product.StartingInventory - (product.RequestedInventory + product.LeftFromYesterday);
                //totalRow.TransferOn += product.TransferredOn;
                //totalRow.TransferOff += product.TransferredOff;
                //totalRow.EndInventory += product.CurrentInventory > 0 ? product.CurrentInventory : 0;
                //totalRow.Dump += product.Dumped;
                //totalRow.Returns += product.Returned;
                //totalRow.DamagedInTruck += product.DamagedInTruck;
                totalRow.BegInv += value.BegInv;
                totalRow.LoadOut += value.LoadOut;
                totalRow.Adj += value.Adj;
                totalRow.TransferOff += value.TransferOff;
                totalRow.TransferOn += value.TransferOn;
                totalRow.EndInventory += value.EndInventory;
                //totalRow.EndInventory += value.EndInventory > 0 ? value.EndInventory : 0;
                totalRow.Dump += value.Dump;
                totalRow.DamagedInTruck += value.DamagedInTruck;
                totalRow.Unload += value.Unload;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += value.Sales;
                    totalRow.CreditReturns += value.CreditReturns;
                    totalRow.CreditDump += value.CreditDump;
                }
            }
        }

        public class InventorySummaryRowGrouped
        {
            public Product Product { get; set; }
            public UnitOfMeasure UoM { get; set; }
            public string Lot { get; set; }
            public float BegInv { get; set; }
            public double W_BegInv { get; set; }
            public float LoadOut { get; set; }
            public double W_LoadOut { get; set; }
            public float Adj { get; set; }
            public double W_Adj { get; set; }
            public float TransferOn { get; set; }
            public double W_TransferOn { get; set; }
            public float TransferOff { get; set; }
            public double W_TransferOff { get; set; }
            public float Sales { get; set; }
            public double W_Sales { get; set; }
            public float Dump { get; set; }
            public double W_Dump { get; set; }
            public float Return { get; set; }
            public double W_Return { get; set; }
            public float Unload { get; set; }
            public double W_Unload { get; set; }
            public float CreditDump { get; set; }
            public double W_CreditDump { get; set; }
            public float CreditReturns { get; set; }
            public double W_CreditReturns { get; set; }
            public float EndInventory { get; set; }
            public double W_EndInventory { get; set; }
            public float DamagedInTruck { get; set; }
            public double W_DamagedInTruck { get; set; }
        }

        protected virtual IEnumerable<string> GetInventorySummaryTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow, bool isBase)
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

            double totalWeight = 0;

            double TotalPrice = 0;

            if (Config.UsePallets)
            {
                var list = new List<InventorySummaryRowGrouped>();

                foreach (var p in SortDetails.SortedDetails(map))
                {
                    var found = list.FirstOrDefault(x => x.Product == p.Product && x.Lot == p.Lot && x.UoM == p.UoM);
                    if (found == null)
                    {
                        list.Add(new InventorySummaryRowGrouped()
                        {
                            Product = p.Product,
                            UoM = p.UoM,
                            Lot = p.Lot,
                            BegInv = p.BegInv,
                            DamagedInTruck = p.DamagedInTruck,
                            Dump = p.Dump,
                            CreditDump = p.CreditDump,
                            Return = p.Return,
                            Adj = p.Adj,
                            CreditReturns = p.CreditReturns,
                            LoadOut = p.LoadOut,
                            Sales = p.Sales,
                            Unload = p.Unload,
                            TransferOff = p.TransferOff,
                            TransferOn = p.TransferOn,
                            EndInventory = p.EndInventory,
                            W_Sales = p.Sales * p.Weight,
                            W_Adj = p.Adj * p.Weight,
                            W_BegInv = p.BegInv * p.Weight,
                            W_CreditDump = p.BegInv * p.Weight,
                            W_CreditReturns = p.CreditReturns * p.Weight,
                            W_Dump = p.Dump * p.Weight,
                            W_DamagedInTruck = p.DamagedInTruck * p.Weight,
                            W_EndInventory = p.EndInventory * p.Weight,
                            W_LoadOut = p.LoadOut * p.Weight,
                            W_Return = p.Return * p.Weight,
                            W_TransferOff = p.TransferOff * p.Weight,
                            W_TransferOn = p.TransferOn * p.Weight,
                            W_Unload = p.Unload * p.Weight
                        });
                    }
                    else
                    {
                        found.BegInv += p.BegInv;
                        found.DamagedInTruck += p.DamagedInTruck;
                        found.Dump += p.Dump;
                        found.CreditDump += p.CreditDump;
                        found.Return += p.Return;
                        found.Adj += p.Adj;
                        found.CreditReturns += p.CreditReturns;
                        found.LoadOut += p.LoadOut;
                        found.Sales += p.Sales;
                        found.Unload += p.Unload;
                        found.TransferOff += p.TransferOff;
                        found.TransferOn += p.TransferOn;
                        found.EndInventory += p.EndInventory;
                        found.W_Sales += p.Sales * p.Weight;
                        found.W_Adj += p.Adj * p.Weight;
                        found.W_BegInv += p.BegInv * p.Weight;
                        found.W_CreditDump += p.BegInv * p.Weight;
                        found.W_CreditReturns += p.CreditReturns * p.Weight;
                        found.W_Dump += p.Dump * p.Weight;
                        found.W_DamagedInTruck += p.DamagedInTruck * p.Weight;
                        found.W_EndInventory += p.EndInventory * p.Weight;
                        found.W_LoadOut += p.LoadOut * p.Weight;
                        found.W_Return += p.Return * p.Weight;
                        found.W_TransferOff += p.TransferOff * p.Weight;
                        found.W_TransferOn += p.TransferOn * p.Weight;
                        found.W_Unload += p.Unload * p.Weight;
                    }
                }

                foreach (var p in list)
                {
                    if (!Config.PrintAllInventoriesInInvSummary && Math.Round(p.EndInventory, Config.Round) == 0)
                        continue;

                    if (p.Unload == 0 && /*x.OnCreditDump == 0 && */ p.CreditReturns == 0 && p.Sales == 0 &&
                        p.LoadOut == 0 && p.Adj == 0 && p.BegInv == 0 && p.EndInventory == 0 &&
                        p.TransferOff == 0 && p.TransferOn == 0)
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

                    if (p.Product.SoldByWeight && Config.UsePallets)
                    {
                        var newWeight = GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                                         "Weight:",
                                         p.UoM != null ? p.UoM.Name : string.Empty,
                                         Math.Round(p.W_BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.W_LoadOut + p.W_Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.W_TransferOn - p.W_TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.W_Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.W_EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                                         );

                        lines.Add(newWeight);
                        startY += font18Separation;
                    }

                    totalWeight += p.W_EndInventory;

                    startY += font18Separation;

                    TotalPrice += p.EndInventory * p.Product.PriceLevel0;
                }
            }
            else
            {
                foreach (var p in SortDetails.SortedDetails(map))
                {
                    if (!Config.PrintAllInventoriesInInvSummary && Math.Round(p.EndInventory, Config.Round) == 0)
                        continue;
                    
                    if (p.Unload == 0 && /*x.OnCreditDump == 0 && */ p.CreditReturns == 0 && p.Sales == 0 &&
                        p.LoadOut == 0 && p.Adj == 0 && p.BegInv == 0 && p.EndInventory == 0 &&
                        p.TransferOff == 0 && p.TransferOn == 0)
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

                    if (p.Product.SoldByWeight && Config.UsePallets)
                    {
                        var newWeight = GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                                         "Weight:",
                                         p.UoM != null ? p.UoM.Name : string.Empty,
                                         Math.Round(p.BegInv * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round((p.LoadOut + p.Adj) * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round((p.TransferOn - p.TransferOff) * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.Sales * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                         Math.Round(p.EndInventory * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture)
                                         );

                        lines.Add(newWeight);
                        startY += font18Separation;
                    }

                    totalWeight += (p.EndInventory * p.Weight);

                    startY += font18Separation;

                    TotalPrice += p.EndInventory * p.Product.PriceLevel0;

                }
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
                                                    Math.Round(totalRow.LoadOut + totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture)
                                                    ));
                startY += font18Separation;
            }

            if (totalWeight > 0)
            {
                startY += 10;

                lines.Add(GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                    "Total Weight:" + totalWeight, "", "", "", "", "", ""));
                startY += font18Separation;
            }

            if (Config.ShowPricesInInventorySummary)
            {
                startY += 10;

                lines.Add(GetInventorySummaryTableLineFixed(InventorySummaryTableLine, startY,
                                        "Inventory Total Price: " + Math.Round(TotalPrice, Config.Round).ToCustomString(),
                                        "",
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""));

                startY += font18Separation;
            }

            Config.Round = oldRound;

            return lines;
        }

        protected virtual string GetInventorySummaryTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);

        }

        protected virtual string GetInventorySummaryTableProductLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7)
        {

            v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7);

        }

        protected virtual string GetInventorySummaryTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8)
        {
            v1 = v1.Substring(0, v1.Length > 4 ? 4 : v1.Length);
            v2 = v2.Substring(0, v2.Length > 4 ? 4 : v2.Length);
            v3 = v3.Substring(0, v3.Length > 4 ? 4 : v3.Length);
            v4 = v4.Substring(0, v4.Length > 4 ? 4 : v4.Length);
            v5 = v5.Substring(0, v5.Length > 4 ? 4 : v5.Length);
            v6 = v6.Substring(0, v6.Length > 4 ? 4 : v6.Length);
            v7 = v7.Substring(0, v7.Length > 4 ? 4 : v7.Length);
            v8 = v8.Substring(0, v8.Length > 4 ? 4 : v8.Length);


            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8);
        }


        #endregion


        #region Inventory Settlement

        public virtual bool InventorySettlement(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetSettlementReportHeader(ref startY, index, count));

            InventorySettlementRow totalRow = new InventorySettlementRow();

            List<InventorySettlementRow> map = new List<InventorySettlementRow>();

            CreateSettlementReportDataStructure(ref totalRow, ref map);

            lines.AddRange(GetSettlementReportTable(ref startY, map, totalRow));

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            //here do the stuff for sensation
            if (Config.SensationalAssetTracking)
            {
                var totalIns = AssetTracking.List.Sum(x => x.Ins);
                var totalOuts = AssetTracking.List.Sum(x => x.Outs);
                var endInv = (AssetTracking.TruckInventory - totalIns) + totalOuts;

                startY -= font36Separation;
                startY -= font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementAssetTracking], startY, endInv.ToString()));
                startY += font36Separation;
                startY += font36Separation;
                startY += font36Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterDriverSignatureText], startY));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual List<string> GetSettlementReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        void CreateSettlementReportDataStructure(ref InventorySettlementRow totalRow, ref List<InventorySettlementRow> map)
        {
            map = DataAccess.ExtendedSendTheLeftOverInventory();

            foreach (var value in map)
            {
                if (value.IsEmpty)
                    continue;

                if (Config.ShortInventorySettlement && value.IsShort)
                    continue;

                var product = value.Product;

                totalRow.Product = product;
                totalRow.BegInv += value.BegInv;
                totalRow.LoadOut += value.LoadOut;
                totalRow.Adj += value.Adj;
                totalRow.TransferOff += value.TransferOff;
                totalRow.TransferOn += value.TransferOn;
                // totalRow.EndInventory += value.EndInventory > 0 ? value.EndInventory : 0;
                totalRow.EndInventory += value.EndInventory;
                totalRow.Dump += value.Dump;
                totalRow.DamagedInTruck += value.DamagedInTruck;
                totalRow.Unload += value.Unload;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += value.Sales;
                    totalRow.CreditReturns += value.CreditReturns;
                    totalRow.CreditDump += value.CreditDump;
                    totalRow.Reshipped += value.Reshipped;
                }
            }
        }

        protected virtual IEnumerable<string> GetSettlementReportTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow)
        {
            List<string> lines = new List<string>();

            var oldRound = Config.Round;
            Config.Round = 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader], startY));
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

                    var newS = GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                p.UoM != null ? p.UoM.Name : string.Empty,
                                                Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Reshipped, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                p.OverShort);
                    lines.Add(newS);
                    startY += font18Separation;

                    if (p.Product.SoldByWeight && Config.UsePallets)
                    {
                        var newWeight = GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                    "Wt:",
                                                    Math.Round(p.BegInv * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.LoadOut * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Adj * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round((p.TransferOn - p.TransferOff) * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Sales * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.CreditReturns * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.CreditDump * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Reshipped * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.DamagedInTruck * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.Unload * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(p.EndInventory * p.Weight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    p.OverShort);

                        lines.Add(newWeight);
                        startY += font18Separation;
                    }

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
                                                    Math.Round(totalRow.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Reshipped, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    totalRow.OverShort));
                startY += font18Separation;
            }

            Config.Round = oldRound;

            return lines;
        }

        protected virtual string GetInventorySettlementTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9, string v10, string v11, string v12, string v13)
        {
            v1 = !string.IsNullOrEmpty(v1) ? v1.Substring(0, v1.Length > 6 ? 6 : v1.Length) : string.Empty;
            v2 = !string.IsNullOrEmpty(v2) ? v2.Substring(0, v2.Length > 5 ? 5 : v2.Length) : string.Empty;
            v3 = !string.IsNullOrEmpty(v3) ? v3.Substring(0, v3.Length > 4 ? 4 : v3.Length) : string.Empty;
            v4 = !string.IsNullOrEmpty(v4) ? v4.Substring(0, v4.Length > 4 ? 4 : v4.Length) : string.Empty;
            v5 = !string.IsNullOrEmpty(v5) ? v5.Substring(0, v5.Length > 4 ? 4 : v5.Length) : string.Empty;
            v6 = !string.IsNullOrEmpty(v6) ? v6.Substring(0, v6.Length > 4 ? 4 : v6.Length) : string.Empty;
            v7 = !string.IsNullOrEmpty(v7) ? v7.Substring(0, v7.Length > 4 ? 4 : v7.Length) : string.Empty;
            v8 = !string.IsNullOrEmpty(v8) ? v8.Substring(0, v8.Length > 4 ? 4 : v8.Length) : string.Empty;
            v9 = !string.IsNullOrEmpty(v9) ? v9.Substring(0, v9.Length > 4 ? 4 : v9.Length) : string.Empty;
            v10 = !string.IsNullOrEmpty(v10) ? v10.Substring(0, v10.Length > 4 ? 4 : v10.Length) : string.Empty;
            v11 = !string.IsNullOrEmpty(v11) ? v11.Substring(0, v11.Length > 4 ? 4 : v11.Length) : string.Empty;
            v12 = !string.IsNullOrEmpty(v12) ? v12.Substring(0, v12.Length > 4 ? 4 : v12.Length) : string.Empty;
            v13 = !string.IsNullOrEmpty(v13) ? v13.Substring(0, v13.Length > 4 ? 4 : v13.Length) : string.Empty;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13);
        }

        protected virtual string GetInventorySettlementTableTotalsFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9, string v10, string v11, string v12)
        {
            v1 = !string.IsNullOrEmpty(v1) ? v1.Substring(0, v1.Length > 4 ? 4 : v1.Length) : string.Empty;
            v2 = !string.IsNullOrEmpty(v2) ? v2.Substring(0, v2.Length > 4 ? 4 : v2.Length) : string.Empty;
            v3 = !string.IsNullOrEmpty(v3) ? v3.Substring(0, v3.Length > 4 ? 4 : v3.Length) : string.Empty;
            v4 = !string.IsNullOrEmpty(v4) ? v4.Substring(0, v4.Length > 4 ? 4 : v4.Length) : string.Empty;
            v5 = !string.IsNullOrEmpty(v5) ? v5.Substring(0, v5.Length > 4 ? 4 : v5.Length) : string.Empty;
            v6 = !string.IsNullOrEmpty(v6) ? v6.Substring(0, v6.Length > 4 ? 4 : v6.Length) : string.Empty;
            v7 = !string.IsNullOrEmpty(v7) ? v7.Substring(0, v7.Length > 4 ? 4 : v7.Length) : string.Empty;
            v8 = !string.IsNullOrEmpty(v8) ? v8.Substring(0, v8.Length > 4 ? 4 : v8.Length) : string.Empty;
            v9 = !string.IsNullOrEmpty(v9) ? v9.Substring(0, v9.Length > 4 ? 4 : v9.Length) : string.Empty;
            v10 = !string.IsNullOrEmpty(v10) ? v10.Substring(0, v10.Length > 4 ? 4 : v10.Length) : string.Empty;
            v11 = !string.IsNullOrEmpty(v11) ? v11.Substring(0, v11.Length > 4 ? 4 : v11.Length) : string.Empty;
            v12 = !string.IsNullOrEmpty(v12) ? v12.Substring(0, v12.Length > 4 ? 4 : v12.Length) : string.Empty;

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12);
        }

        #endregion

        #region Consignment

        public virtual bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false)
        {
            bool countedResult = true;
            bool updatedResult = true;

            if (printCounting)
                foreach (var detail in order.Details)
                {
                    if (detail.ConsignmentCounted)
                    {
                        if (Config.UseBattery)
                        {
                            var printer = new BatteryPrinter();
                            countedResult = printer.PrintBatteryConsignmentInvoice(order, asPreOrder);
                        }
                        else
                            countedResult = PrintConsignmentInvoice(order, asPreOrder);
                        break;
                    }
                }

            if (printcontract)
            {
                foreach (var detail in order.Details)
                {
                    var updated = detail.ConsignmentUpdated;
                    if (Config.UseFullConsignment)
                        updated = detail.ConsignmentOld != detail.ConsignmentNew || detail.Price != detail.ConsignmentNewPrice;

                    if (allways || updated)
                    {
                        updatedResult = PrintConsignmentContract(order, asPreOrder);
                        break;
                    }
                }
            }
            return countedResult && updatedResult;
        }

        protected virtual bool PrintConsignmentInvoice(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetConsignmentInvoiceHeaderLines(ref startY, order));

            lines.AddRange(GetConsignmentInvoiceLabel(ref startY, order, asPreOrder));

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                startY += font36Separation;
            }

            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var paidInFull = payments != null && payments.Sum(x => x.Amount) == order.OrderTotalCost();
                lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            }

            startY += font36Separation;

            lines.AddRange(GetConsignmentInvoiceTable(ref startY, order));

            startY += font36Separation;
            startY += font18Separation;

            string s1;

            s1 = ToString(order.CalculateItemCost());
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
            startY += font36Separation;

            double discount = order.CalculateDiscount();
            if (discount > 0)
            {
                s1 = ToString(Math.Abs(discount));
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                startY += font36Separation;
            }

            double tax = order.CalculateTax();
            if (tax > 0)
            {
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = ToString(tax);
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s + s1));
                startY += font36Separation;
            }

            s1 = ToString(order.OrderTotalCost());
            s1 = s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
            startY += font36Separation;

            if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice && !order.IsWorkOrder)
            {
                double paid = 0;
                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                if (payment != null)
                {
                    var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                    paid = parts.Sum(x => x.Amount);
                }

                s1 = ToString(paid);
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                startY += font36Separation;

                s1 = ToString((order.OrderTotalCost() - paid));
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(order.DiscountComment))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                startY += font18Separation;
            }

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order, true));
            else
                lines.AddRange(GetFooterRows(ref startY, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetConsignmentInvoiceHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            if (!order.AsPresale)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceHeader], startY, order.PrintedOrderId));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentSalesOrderHeader], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetConsignmentHeaderLines(ref startY, order));

            return lines;
        }

        protected virtual IEnumerable<string> GetConsignmentHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
            if (salesman != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, salesman.Name));
                startY += font18Separation;
            }

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font36Separation;
            }

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s1.Trim()));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = ToString(order.Client.OpenBalance);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderAccountBalance], startY, balance));
                startY += font18Separation;
            }

            return lines;
        }

        protected virtual List<string> GetConsignmentInvoiceLabel(ref int startY, Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            string docName = "NOT AN INVOICE";

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, docName));
                    startY += font36Separation;
                }
            }
            else
            {
                if (Config.FakePreOrder)
                {
                    bool credit = false;
                    if (order != null)
                        credit = order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, credit ? "FINAL CREDIT INVOICE" : "FINAL INVOICE"));
                    startY += font36Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICATE" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, name));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual IEnumerable<string> GetConsignmentInvoiceTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalQty = 0;
            double total = 0;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (!detail.ConsignmentCounted)
                    continue;

                int index = 0;

                totalQty += detail.Qty;

                var price = detail.Price;
                if (detail.IsCredit)
                    price *= -1;

                total += double.Parse(Math.Round(Convert.ToDecimal(price * detail.Qty), Config.Round).ToCustomString(), NumberStyles.Currency);

                var productSlices = GetConsInvoiceDetailRowsSplitProductName(detail.Product.Name);
                foreach (var productNamePart in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableLine], startY,
                                                    productNamePart,
                                                    detail.ConsignmentOld.ToString(CultureInfo.CurrentCulture),
                                                    detail.ConsignmentCount.ToString(CultureInfo.CurrentCulture),
                                                    detail.Qty.ToString(CultureInfo.CurrentCulture),
                                                    ToString(price),
                                                    ToString((detail.Price * detail.Qty))
                                                    ));


                        startY += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableLine], startY,
                            productNamePart,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        startY += font18Separation;
                    }
                    else
                        break;
                    index++;
                }

                if (!string.IsNullOrEmpty(detail.Lot))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableLineLot], startY, detail.Lot));
                    startY += font18Separation;
                }

                lines.AddRange(GetUpcForProductInOrder(ref startY, order, detail.Product));
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableTotal], startY, totalQty, ToString(total)));
            startY += font18Separation;

            return lines;
        }

        protected virtual IEnumerable<string> GetConsignmentSignatureLines(ref int startY, Order order, bool counting)
        {
            List<string> lines = new List<string>();

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

            if (!string.IsNullOrEmpty(Config.ConsignmentContractText) && !counting)
            {
                startY += font18Separation;
                foreach (var line in GetBottomSplitText(Config.ConsignmentContractText))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            else if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomSplitText())
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            var discount = order.CalculateDiscount();
            var orderSales = order.CalculateItemCost();

            if (discount == orderSales && !string.IsNullOrEmpty(Config.Discount100PercentPrintText))
            {
                startY += font18Separation;
                foreach (var line in GetBottomSplitText(Config.Discount100PercentPrintText))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                    startY += font18Separation;
                }
            }

            return lines;
        }

        protected virtual bool PrintConsignmentContract(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            startY += font36Separation;

            lines.AddRange(GetConsignmentContractHeaderLines(ref startY, order));

            startY += 50;

            lines.AddRange(GetConsignmentContractTable(ref startY, order));

            startY += font36Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, "THIS IS NOT YOUR INVOICE"));
            startY += font36Separation;
            startY += font36Separation;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order, false));
            else
                lines.AddRange(GetConsignmentContractFooterRows(ref startY, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetConsignmentContractHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractHeader], startY));
            startY += font36Separation;

            lines.AddRange(GetConsignmentHeaderLines(ref startY, order));

            return lines;
        }

        protected virtual IEnumerable<string> GetConsignmentContractTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractTableHeader1], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractTableHeader2], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double oldConsTotal = 0;
            double newConsToltal = 0;
            double contractTotal = 0;

            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                if (Config.UseFullConsignment && detail.ConsignmentNew == 0 && detail.ConsignmentSalesItem)
                    continue;

                oldConsTotal += detail.ConsignmentOld;
                newConsToltal += detail.ConsignmentNew;


                int index = 0;

                var productSlices = GetConsContractDetailRowsSplitProductName(detail.Product.Name);
                foreach (var part in productSlices)
                {
                    if (index == 0)
                    {
                        var consNew = detail.ConsignmentNew;

                        if (!detail.ConsignmentUpdated && detail.ConsignmentNew == 0)
                            consNew = detail.ConsignmentOld;

                        contractTotal += consNew * detail.ConsignmentNewPrice;

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractTableLine], startY,
                            part,
                            detail.ConsignmentOld.ToString(),
                            consNew.ToString(),
                            ToString(detail.ConsignmentNewPrice),
                            ToString(detail.ConsignmentNewPrice * consNew)));
                        startY += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractTableLine], startY,
                            part,
                            "",
                            "",
                            "",
                            ""));
                        startY += font18Separation;
                    }
                    else
                        break;
                    index++;
                }

                if (!string.IsNullOrEmpty(detail.Lot))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentInvoiceTableLineLot], startY, detail.Lot));
                    startY += font18Separation;
                }

                lines.AddRange(GetUpcForProductInOrder(ref startY, order, detail.Product));
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ConsignmentContractTableTotal], startY, oldConsTotal, newConsToltal, ToString(contractTotal)));
            startY += font18Separation;

            return lines;
        }

        protected virtual IEnumerable<string> GetConsignmentContractFooterRows(ref int startIndex, bool asPreOrder)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, font18Separation, 4);

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startIndex));
            AddExtraSpace(ref startIndex, list, 12, 1);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startIndex));
            AddExtraSpace(ref startIndex, list, font18Separation, 4);
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startIndex));

            if (!string.IsNullOrEmpty(Config.ConsignmentContractText))
            {
                startIndex += font18Separation;
                foreach (var line in GetBottomSplitText(Config.ConsignmentContractText))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startIndex, line));
                    startIndex += font18Separation;
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
        #endregion

        #region Battery End Of Day
        //REVISAR
        public bool PrintBatteryEndOfDay(int index, int count)
        {
            if (Config.UseBattery)
            {
                var printer = new BatteryPrinter();
                return printer.PrintBatteryEndOfDay(index, count);
            }

            return false;
        }

        #endregion

        #region Route Return

        public virtual bool PrintRouteReturn(IEnumerable<RouteReturnLine> sortedList, bool isFinal)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsTitle], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 2);

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float returnBoxes = 0;
            float dumpBoxes = 0;
            float damagedBoxes = 0;
            float unloadedBoxes = 0;
            float refusedBoxes = 0;

            foreach (var p in sortedList)
            {
                int productLineOffset = 0;
                foreach (string pName in GetRouteReturnRowsSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                        lines.Add(GetRouteReturnTableLineFixed(RouteReturnsTableLine, startY,
                            pName,
                            Math.Round(p.Reships, Config.Round).ToString(CultureInfo.CurrentCulture),
                            Math.Round(p.Dumps, Config.Round).ToString(CultureInfo.CurrentCulture),
                            Math.Round(p.Returns, Config.Round).ToString(CultureInfo.CurrentCulture),
                            Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                            Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    else
                        lines.Add(GetRouteReturnTableLineFixed(RouteReturnsTableLine, startY, pName, "", "", "", "", "")); productLineOffset++;

                    startY += font18Separation;
                }

                if (p.Product.SoldByWeight && Config.UsePallets)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsTableLine], startY, "Weight: " + p.Weight, "", "", "", "", ""));
                    startY += font18Separation;
                }

                returnBoxes += Convert.ToSingle(p.Returns);
                dumpBoxes += Convert.ToSingle(p.Dumps);
                damagedBoxes += Convert.ToSingle(p.DamagedInTruck);
                unloadedBoxes += Convert.ToSingle(p.Unload);
                refusedBoxes += Convert.ToSingle(p.Reships);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(GetRouteReturnTableLineFixed(RouteReturnsTotals, startY, "",
                Math.Round(refusedBoxes, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(dumpBoxes, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(returnBoxes, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(damagedBoxes, Config.Round).ToString(CultureInfo.CurrentCulture),
                Math.Round(unloadedBoxes, Config.Round).ToString(CultureInfo.CurrentCulture)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterDriverSignatureText], startY));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual string GetRouteReturnTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
            v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Payment

        public bool PrintPayment(InvoicePayment invoicePayment)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, null);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            lines.AddRange(GetPaymentTitle(ref startY));

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderTo], startY));
            startY += font36Separation;

            Client client = invoicePayment.Client;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderClientName], startY, client.ClientName));
            startY += font36Separation;
            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentHeaderClientAddr], startY, s.Trim()));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            var orders = invoicePayment.Orders();
            var invoices = invoicePayment.Invoices();
            bool moreThanOne = (orders.Count + invoices.Count) > 1;
            var sb = new StringBuilder();
            var sb_credits = new StringBuilder();

            if (Config.ShowInvoicesCreditsInPayments)
            {
                if (orders.Count > 0)
                    foreach (var item in orders)
                    {
                        if (item.OrderTotalCost() < 0)
                        {
                            if (sb_credits.Length > 0)
                                sb_credits.Append(", ");
                            sb_credits.Append(item.PrintedOrderId);
                        }
                        else
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(item.PrintedOrderId);
                        }
                    }
                if (sb.Length == 0)
                {
                    foreach (var item in invoices)
                    {
                        if (item.Balance < 0)
                        {
                            if (sb_credits.Length > 0)
                                sb_credits.Append(", ");
                            sb_credits.Append(item.InvoiceNumber);
                        }
                        else
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(item.InvoiceNumber);
                        }
                    }
                }
            }
            else
            {
                if (orders.Count > 0)
                    foreach (var item in orders)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(item.PrintedOrderId);
                    }

                if (invoices.Count > 0)
                    foreach (var item in invoices)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(item.InvoiceNumber);
                    }
            }

            var text = GetPaymentInvoiceNumberLine(moreThanOne);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentInvoiceNumber], startY, text, sb.ToString()));
            startY += font18Separation;

            if (!string.IsNullOrEmpty(sb_credits.ToString()))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentInvoiceNumber], startY, "Credits", sb_credits.ToString()));
                startY += font18Separation;
            }

            double balance = 0;
            if (orders.Count > 0)
                balance = orders.Sum(x => x.OrderTotalCost());

            if (invoices.Count > 0)
            {
                balance += invoices.Sum(x => x.Balance);

                foreach (var idAsString in invoicePayment.InvoicesId.Split(new char[] { ',' }))
                {
                    int id = 0;
                    Invoice invioce = null;
                    if (Config.SavePaymentsByInvoiceNumber)
                        invioce = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceNumber == idAsString);
                    else
                    {
                        id = Convert.ToInt32(idAsString);
                        invioce = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceId == id);
                    }

                    var paymentsForInvoice = InvoicePayment.List.Where(x => x.InvoicesId.Contains(idAsString));
                    double alreadyPaid = 0;
                    foreach (var p in paymentsForInvoice)
                    {
                        if (p.Id == invoicePayment.Id)
                            continue;

                        foreach (var i in p.Invoices())
                        {
                            bool matches = Config.SavePaymentsByInvoiceNumber ? i.InvoiceNumber == idAsString : i.InvoiceId == id;

                            if (matches)
                            {
                                foreach (var component in p.Components)
                                {
                                    if (invioce.Balance == 0)
                                        continue;

                                    double usedInThisInvoice = component.Amount;

                                    if (invioce.Balance < 0)
                                        usedInThisInvoice = invioce.Balance;
                                    else
                                    {
                                        if (component.Amount > invioce.Balance)
                                            usedInThisInvoice = invioce.Balance;
                                    }

                                    alreadyPaid += usedInThisInvoice;
                                }
                            }
                        }
                    }

                    balance -= alreadyPaid;
                }
            }

            if (moreThanOne)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentInvoiceTotal], startY, text, ToString(balance)));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentInvoiceTotal], startY, text, ToString(balance)));
            startY += font18Separation;

            if (invoicePayment.Components.Sum(x => x.Amount) == balance)
            {
                string type = string.Empty;
                if (invoicePayment.Components.Count == 1)
                    type = invoicePayment.Components[0].PaymentMethod.ToString().Replace("_", " ");
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPaidInFull], startY, type));
                startY += font36Separation;
            }

            lines.AddRange(GetPaymentComponents(ref startY, invoicePayment));

            var open = (balance - invoicePayment.Components.Sum(x => x.Amount));
            if (open > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentTotalPaid], startY, ToString(invoicePayment.Components.Sum(x => x.Amount))));
                startY += font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPending], startY, ToString(open)));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentTotalPaid], startY, ToString(invoicePayment.Components.Sum(x => x.Amount))));
                startY += font36Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentPending], startY, ToString(open)));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY, string.Empty));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY, string.Empty));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetPaymentTitle(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentTitle], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            return lines;
        }

        protected virtual string GetPaymentInvoiceNumberLine(bool moreThanOne)
        {
            string invoiceNumberLine = "Invoice";
            if (moreThanOne)
                invoiceNumberLine = "Invoices";

            return invoiceNumberLine;
        }

        protected virtual IEnumerable<string> GetPaymentComponents(ref int startY, InvoicePayment invoicePayment)
        {
            List<string> lines = new List<string>();

            foreach (var component in invoicePayment.Components)
            {
                var pm = component.PaymentMethod.ToString().Replace("_", " ");
                if (pm.Length < 11)
                    pm = new string(' ', 11 - pm.Length) + pm;
                string s = string.Format("Method: {0} Amount: {1}", pm, ToString(component.Amount));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, s));
                startY += font18Separation;

                if (!string.IsNullOrEmpty(component.Ref))
                {

                    string refName = "Ref: {0}";
                    switch (component.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            refName = "Check: {0}";
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            refName = "Money Order: {0}";
                            break;
                        case InvoicePaymentMethod.Transfer:
                            refName = "Transfer: {0}";
                            break;
                        case InvoicePaymentMethod.Zelle_Transfer:
                            refName = "Zelle Transfer: {0}";
                            break;
                    }
                    s = string.Format(refName, component.Ref);
                    if (!string.IsNullOrEmpty(component.Comments))
                        s = s + " Comments: " + component.Comments;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, s));
                    startY += font18Separation;
                }
                else
                {
                    if (!string.IsNullOrEmpty(component.Comments))
                    {
                        var temp_comments = "Comments: " + component.Comments;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, temp_comments));
                        startY += font18Separation;
                    }
                }
                startY += font18Separation / 2;
            }

            return lines;
        }

        #endregion

        #region Open Invoices

        public virtual bool PrintOpenInvoice_(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedOn], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font36Separation;

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY));
            startY += font18Separation; //an extra line

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTitle], startY, GetInvoiceType(invoice) + invoice.InvoiceNumber));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCopy], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarCreatedOn], startY, invoice.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (invoice.DueDate < DateTime.Today)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOnOverdue], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOn], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font36Separation;
            }

            Client client = invoice.Client;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
                custNoString = " " + custno.Value;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCustomerNumber], startY, custNoString));
            startY += font36Separation;

            foreach (string s1 in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientAddr], startY, s1.Trim()));
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

        private IEnumerable<string> GetOpenInvoiceTable_(ref int startY, Invoice invoice)
        {
            List<string> lines = new List<string>();
            Product notFoundProduct = GetNotFoundInvoiceProduct();

            //foreach (var item in invoice.Details)
            //    if (item.Product == null)
            //        item.Product = notFoundProduct;

            IQueryable<InvoiceDetail> source = SortDetails.SortedDetails(invoice.Details);

            double totalUnits = 0;
            double numberOfBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (InvoiceDetail detail in source)
            {
                Product p = detail.Product;

                int productLineOffset = 0;
                foreach (string pName in GetInvoiceDetailSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        double d = detail.Quantity * detail.Price;
                        double price = detail.Price;
                        double package = 1;
                        try
                        {
                            package = Convert.ToSingle(detail.Product.Package, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }

                        double units = detail.Quantity * package;
                        totalUnits += units;

                        lines.Add(GetOpenInvoiceTableFixed(InvoiceTableLine, startY,
                            pName,
                            detail.Quantity.ToString(),
                            price.ToCustomString(),
                            d.ToCustomString()
                            ));
                    }
                    else
                        lines.Add(GetOpenInvoiceTableFixed(InvoiceTableLine, startY,
                            pName, "", "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }

                if (!string.IsNullOrEmpty(detail.Comments.Trim()))
                {

                    foreach (string commentPArt in GetOpenInvoiceCommentSplit(detail.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                        startY += font18Separation;
                    }
                }

                lines.AddRange(GetUpcForProductIn(ref startY, p));

                startY += font18Separation + orderDetailSeparation; //a little extra space
                numberOfBoxes += Convert.ToSingle(detail.Quantity);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.AddRange(GetInvoiceTotals(ref startY, invoice, numberOfBoxes, totalUnits));

            return lines;
        }

        protected virtual string GetOpenInvoiceTableFixed_(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        private IEnumerable<string> GetInvoiceTotals_(ref int startY, Invoice invoice, double numberOfBoxes, double totalUnits)
        {
            List<string> lines = new List<string>();

            string s1;

            s1 = ToString(invoice.Amount);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTotal], startY, s1));
            startY += font36Separation;


            if (invoice.Balance > 0)
            {
                InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null));

                var payments = existPayment != null ? existPayment.Components : null;
                if (payments != null && payments.Count > 0)
                {
                    var totalPaid = payments.Sum(x => x.Amount);

                    var paidInFull = totalPaid == invoice.Balance;

                    if (paidInFull)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePaidInFull], startY));
                        startY += font36Separation;
                    }
                    else
                    {
                        s1 = ToString(totalPaid);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePartialPayment], startY, s1));
                        startY += font36Separation;

                        s1 = ToString((invoice.Balance - totalPaid));
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceOpen], startY, s1));
                        startY += font36Separation;
                    }
                }
                else
                {
                    s1 = ToString(invoice.Balance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceOpen], startY, s1));
                    startY += font36Separation;
                }
            }
            else
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePaidInFull], startY));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            s1 = Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceQtyItems], startY, s1));
            startY += font36Separation;

            s1 = Math.Round(totalUnits, Config.Round).ToString(CultureInfo.CurrentCulture);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceQtyUnits], startY, s1));
            startY += font36Separation;

            return lines;
        }

        protected virtual string GetInvoiceType_(Invoice invoice)
        {
            var headerText = "Invoice #: ";

            if (invoice.InvoiceType == 1)
                headerText = "Credit #: ";
            else if (invoice.InvoiceType == 2)
                headerText = "Quote #: ";
            else if (invoice.InvoiceType == 3)
                headerText = "Sales Order #: ";

            return headerText;
        }

        protected virtual Product GetNotFoundInvoiceProduct_()
        {
            Product notFoundProduct = new Product();
            notFoundProduct.Code = string.Empty;
            notFoundProduct.Cost = 0;
            notFoundProduct.Description = "Not found product";
            notFoundProduct.Name = "Not found product";
            notFoundProduct.Package = "1";
            notFoundProduct.ProductType = ProductType.Inventory;
            notFoundProduct.UoMFamily = string.Empty;
            notFoundProduct.Upc = string.Empty;

            return notFoundProduct;
        }

        #endregion

        #region Transfers

        public virtual bool PrintTransferOnOff(IEnumerable<InventoryLine> sortedList, bool isOn, bool isFinal, string comment = "", string siteName = "")
        {
            List<string> lines = new List<string>();

            int startY = 80;

            if (isOn)
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOnHeader], startY));
            else
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferOffHeader], startY));

            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (!string.IsNullOrEmpty(siteName))
            {
                string toPrint = string.Empty;
                if (isOn)
                    toPrint = "From: " + siteName;
                else
                    toPrint = "To: " + siteName;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, toPrint));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation; //an extra line
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetTransferTable(ref startY, sortedList));

            if (!isFinal)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferNotFinal], startY));
                startY += font36Separation;
            }

            if (!string.IsNullOrEmpty(comment))
            {
                startY += font18Separation;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[TransferComment], startY, comment));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterDriverSignatureText], startY));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;


            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetTransferTable(ref int startY, IEnumerable<InventoryLine> sortedList)
        {
            List<string> lines = new List<string>();

            float numberOfBoxes = 0;
            double value = 0.0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in sortedList)
            {
                double price = p.Product.PriceLevel0;
                string uomLabel = string.Empty;
                var real = p.Real;
                string lot = p.Lot;

                if (p.UoM != null)
                {
                    price *= p.UoM.Conversion;
                    uomLabel = p.UoM.Name;
                    real *= p.UoM.Conversion;
                }

                int productLineOffset = 0;

                if (uomLabel.Length > 12)
                {
                    uomLabel = uomLabel.Substring(0, 12);
                }

                foreach (string pName in GetTransferSplitProductName(p.Product.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(GetTransferTableFixedLine(TransferTableLine, startY,
                            pName, lot, uomLabel, Math.Round(p.Real, Config.Round).ToString(CultureInfo.CurrentCulture)));
                    }
                    else
                        lines.Add(GetTransferTableFixedLine(TransferTableLine, startY,
                            pName, "", "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }

                if (!Config.HidePriceInTransaction)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferTableLinePrice], startY, ToString(price)));
                    startY += font18Separation;
                }

                lines.AddRange(GetUpcForProductIn(ref startY, p.Product));

                numberOfBoxes += Convert.ToSingle(real);
                value += p.Real * price;
                startY += font18Separation + orderDetailSeparation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            string s1;

            s1 = Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferQtyItems], startY, s1));
            startY += font36Separation;

            if (!Config.HidePriceInTransaction)
            {
                s1 = ToString(value);
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[TransferAmount], startY, s1));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual string GetTransferTableFixedLine(string format, int pos, string v1, string lot, string v2, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, lot, v2, v3);
        }

        #endregion

        #region Full Consignment

        public virtual bool PrintFullConsignment(Order order, bool asPreOrder)
        {
            return PrintConsignment(order, asPreOrder, true, true, false);
        }

        #endregion

        #region Client Statement

        public bool PrintClientStatement(Client client)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetClientStatementHeader(ref startY, client));

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            lines.AddRange(GetClientStatementTable(ref startY, client));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetClientStatementHeader(ref int startY, Client client)
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

        protected virtual IEnumerable<string> GetClientStatementTable(ref int startY, Client client)
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

            if (!(Config.HideInvoicesAndBalance && Config.ButlerCustomization))
            {
                foreach (var item in openInvoices.OrderBy(x => x.DueDate))
                {
                    if (item.InvoiceType == 2 || item.InvoiceType == 3)
                        continue;

                    lines.Add(GetClientStatementFixedLine(ClientStatementTableLine,
                        startY,
                        GetClientStatementInvoiceType(item.InvoiceType),
                        item.Date.ToShortDateString(),
                        item.InvoiceNumber,
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
            }


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            lines.AddRange(GetClientStatementTotals(ref startY, current, due1_30, due31_60, due61_90, over90));

            return lines;
        }

        protected virtual IEnumerable<string> GetClientStatementTotals(ref int startY, double current, double due1_30,
        double due31_60,
        double due61_90,
        double over90)
        {
            List<string> lines = new List<string>();

            string s1;

            if (Config.HideInvoicesAndBalance && Config.ButlerCustomization)
            {
                current = 0;
                due1_30 = 0;
                due31_60 = 0;
                due61_90 = 0;
                over90 = 0;
            }
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

        protected virtual IEnumerable<string> GetClientStatementTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[ClientStatementTableHeader], startY));
            startY += font36Separation;

            return lines;
        }

        protected virtual string GetClientStatementInvoiceType(int invoiceType)
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

        protected virtual string GetClientStatementFixedLine(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region New Open Invoices

        protected virtual IEnumerable<string> GetCompanyRows(ref int startIndex, string companyName)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (!string.IsNullOrEmpty(companyName))
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == companyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

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

        public virtual bool PrintOpenInvoice(Invoice invoice)
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

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCopy], startY));
            startY += font36Separation;

            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarCreatedOn], startY, invoice.Date.ToString(Config.OrderDatePrintFormat)));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarCreatedOn], startY, invoice.Date.ToString(Config.InvoiceCopyDatePrintFormat)));
            startY += font18Separation;

            if (invoice.DueDate < DateTime.Today)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOnOverdue], startY, invoice.DueDate.ToString(Config.InvoiceCopyDatePrintFormat)));
                startY += font18Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOn], startY, invoice.DueDate.ToString(Config.InvoiceCopyDatePrintFormat)));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedOn], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetCompanyRows(ref startY, invoice.CompanyName));

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

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
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

            var poNumber = DataAccess.GetSingleUDF("PONumber", invoice.ExtraFields);

            if (!string.IsNullOrEmpty(poNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, poNumber));
                startY += font36Separation;
            }

            startY += font36Separation;

            foreach (string commentPArt in GetOpenInvoiceCommentSplit(invoice.Comments ?? string.Empty))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                startY += font18Separation;
            }

            lines.AddRange(GetOpenInvoiceTable(ref startY, invoice));

            lines.AddRange(GetInvoiceSignature(ref startY, invoice));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        public IEnumerable<string> GetInvoiceSignature(ref int startY, Invoice invoice)
        {
            List<string> lines = new List<string>();

            if (!string.IsNullOrEmpty(invoice.Signature))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                                   invoice.SignatureSize.ToString() + "," +
                                   invoice.SignatureSize.ToString() + "," +
                                   invoice.SignatureWidth.ToString() + "," +
                                   invoice.Signature;

                lines.Add(label);
                startY += invoice.SignatureHeight;
                startY += 10;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual IEnumerable<string> GetOpenInvoiceTable(ref int startY, Invoice invoice)
        {
            List<string> lines = new List<string>();
            Product notFoundProduct = GetNotFoundInvoiceProduct();

            //foreach (var item in invoice.Details)
            //    if (item.Product == null)
            //        item.Product = notFoundProduct;

            IQueryable<InvoiceDetail> source = SortDetails.SortedDetails(invoice.Details);

            double totalUnits = 0;
            double numberOfBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (InvoiceDetail detail in source)
            {
                Product p = detail.Product;

                int productLineOffset = 0;
                foreach (string pName in GetInvoiceDetailSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        double d = detail.Quantity * detail.Price;
                        double price = detail.Price;
                        double package = 1;
                        try
                        {
                            package = Convert.ToSingle(detail.Product.Package, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }

                        double units = detail.Quantity * package;
                        totalUnits += units;

                        lines.Add(GetOpenInvoiceTableFixed(InvoiceTableLine, startY,
                            pName,
                            detail.Quantity.ToString(),
                            price.ToCustomString(),
                            d.ToCustomString()
                            ));
                    }
                    else
                        lines.Add(GetOpenInvoiceTableFixed(InvoiceTableLine, startY,
                            pName, "", "", ""));

                    productLineOffset++;
                    startY += font18Separation;
                }

                lines.AddRange(GetUpcForProductIn(ref startY, p));

                if (!string.IsNullOrEmpty(detail.Comments.Trim()))
                {

                    foreach (string commentPArt in GetOpenInvoiceCommentSplit(detail.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                        startY += font18Separation;
                    }
                }

                startY += 10;
                numberOfBoxes += Convert.ToSingle(detail.Quantity);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.AddRange(GetInvoiceTotals(ref startY, invoice, numberOfBoxes, totalUnits));

            return lines;
        }

        protected virtual string GetOpenInvoiceTableFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        protected IEnumerable<string> GetInvoiceTotals(ref int startY, Invoice invoice, double numberOfBoxes, double totalUnits)
        {
            List<string> lines = new List<string>();

            string s1;

            s1 = ToString(invoice.Amount);
            s1 = new string(' ', 14 - s1.Length) + s1;

            lines.Add(GetInvoiceTotalFixedLine(OrderDetailsTotals, startY, string.Empty, " Units:", Math.Round(totalUnits, Config.Round).ToString(CultureInfo.CurrentCulture), ""));
            startY += font18Separation;

            lines.Add(GetInvoiceTotalFixedLine(OrderDetailsTotals, startY, string.Empty, "Totals:", Math.Round(numberOfBoxes, Config.Round).ToString(CultureInfo.CurrentCulture), ToString(invoice.Amount)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            if (invoice.Balance > 0)
            {
                InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null));

                var payments = existPayment != null ? existPayment.Components : null;
                if (payments != null && payments.Count > 0)
                {
                    var totalPaid = payments.Sum(x => x.Amount);

                    var paidInFull = totalPaid == invoice.Balance;

                    if (Config.ShowInvoicesCreditsInPayments && existPayment != null && existPayment.Invoices().Any(x => x.Amount < 0))
                        paidInFull = true;

                    if (paidInFull)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePaidInFull], startY));
                        startY += font36Separation;
                    }
                    else
                    {
                        s1 = ToString(totalPaid);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePartialPayment], startY, s1));
                        startY += font36Separation;

                        s1 = ToString((invoice.Balance - totalPaid));
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceOpen], startY, s1));
                        startY += font36Separation;
                    }
                }
                else
                {
                    s1 = ToString(invoice.Balance);
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceOpen], startY, s1));
                    startY += font36Separation;
                }
            }
            else if (invoice.Balance == 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePaidInFull], startY));
                startY += font36Separation;
            }
            else
            {
                if (Config.ShowInvoicesCreditsInPayments)
                {
                    InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null));
                    var payments = existPayment != null ? existPayment.Components : null;
                    if (payments != null && payments.Count > 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoicePaidInFullCredit], startY));
                        startY += font36Separation;
                    }
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCredit], startY));
                        startY += font36Separation;
                    }
                }
                else
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceCredit], startY));
                    startY += font36Separation;
                }
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            return lines;
        }

        protected virtual string GetInvoiceType(Invoice invoice)
        {
            var headerText = "Invoice #: ";

            if (invoice.InvoiceType == 1)
                headerText = "Credit #: ";
            else if (invoice.InvoiceType == 2)
                headerText = "Quote #: ";
            else if (invoice.InvoiceType == 3)
                headerText = "Sales Order #: ";

            return headerText;
        }

        protected virtual Product GetNotFoundInvoiceProduct()
        {
            Product notFoundProduct = new Product();
            notFoundProduct.Code = string.Empty;
            notFoundProduct.Cost = 0;
            notFoundProduct.Description = "Not found product";
            notFoundProduct.Name = "Not found product";
            notFoundProduct.Package = "1";
            notFoundProduct.ProductType = ProductType.Inventory;
            notFoundProduct.UoMFamily = string.Empty;
            notFoundProduct.Upc = string.Empty;

            return notFoundProduct;
        }

        protected virtual string GetInvoiceTotalFixedLine(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        #endregion

        #region Inventory Count

        public bool PrintInventoryCount(List<CycleCountItem> items)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetInventoryCountHeaders(ref startY));

            lines.AddRange(GetInventoryCountTable(ref startY, items));

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterDriverSignatureText], startY));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetInventoryCountHeaders(ref int startIndex)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 10, 1);

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCountHeader], startIndex));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startIndex, Config.RouteName));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startIndex, Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            return list;
        }

        protected virtual IEnumerable<string> GetInventoryCountTable(ref int startIndex, List<CycleCountItem> sortedList)
        {
            List<string> list = new List<string>();

            AddExtraSpace(ref startIndex, list, 40, 1);

            //the header
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventoryCountTableHeader], startIndex));
            startIndex += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            AddExtraSpace(ref startIndex, list, font18Separation, 1);

            foreach (var prod in sortedList)
            {
                var p = prod.Product;

                float qty = prod.Qty;

                var uom = prod.Product.UnitOfMeasures.FirstOrDefault(x => x.IsBase);

                string uomString = uom != null ? uom.Name : string.Empty;

                int productLineOffset = 0;
                foreach (string pName in GetInventoryCountDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        list.Add(GetInventoryProdTableLineFixed(InventoryCountTableLine, startIndex,
                            pName,
                            Math.Round(qty, Config.Round).ToString(CultureInfo.CurrentCulture),
                            uomString));
                    }
                    else
                        list.Add(GetInventoryProdTableLineFixed(InventoryCountTableLine, startIndex,
                            pName, "", ""));

                    productLineOffset++;
                    startIndex += font18Separation;
                }

                AddExtraSpace(ref startIndex, list, 10, 1);
            }

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            return list;
        }

        #endregion

        #region Print Accepted Orders Report

        public bool PrintAcceptedOrders(List<Order> orders, bool final)
        {
            int startY = 80;

            List<string> lines = new List<string>();

            lines.AddRange(GetAcceptedOrdersHeaderRows(ref startY));

            if (!final)
            {
                AddExtraSpace(ref startY, lines, font18Separation, 1);
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetAcceptedOrdersTable(ref startY, orders));

            if (!final)
            {
                AddExtraSpace(ref startY, lines, font18Separation, 1);
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptLoadNotFinal], startY));
                startY += font36Separation;
            }

            lines.AddRange(GetFooterRows(ref startY, false));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetAcceptedOrdersHeaderRows(ref int startIndex)
        {
            List<string> list = new List<string>();

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersHeader], startIndex));
            startIndex += font36Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersDate], startIndex, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startIndex, Config.RouteName));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startIndex, Config.VendorName));
            startIndex += font18Separation;

            list.AddRange(GetCompanyRows(ref startIndex));

            AddExtraSpace(ref startIndex, list, font36Separation, 1);

            return list;
        }

        protected virtual IEnumerable<string> GetAcceptedOrdersTable(ref int startIndex, List<Order> orders)
        {
            List<Order> deliveries = new List<Order>();
            List<Order> loads = new List<Order>();
            List<Order> credits = new List<Order>();

            foreach (var o in orders)
            {
                if (o.OrderType == OrderType.Load)
                    loads.Add(o);
                else
                {
                    if (o.OrderType == OrderType.Credit || o.OrderType == OrderType.Return)
                        credits.Add(o);
                    else
                        deliveries.Add(o);
                }
            }

            List<string> list = new List<string>();

            float full_totalQty = 0;
            double full_totalWeight = 0;
            double full_totalAmount = 0;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;

            if (loads.Count > 0)
            {
                #region Loads

                float section_totalQty = 0;
                double section_TotalWeight = 0;
                double section_totalAmount = 0;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersLoadsTableHeader], startIndex));
                startIndex += font36Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                AddExtraSpace(ref startIndex, list, font18Separation, 1);

                foreach (var item in loads.OrderBy(x => x.PrintedOrderId ?? ""))
                {
                    float totalQty = 0;
                    double totalWeight = 0;
                    double totalAmount = item.OrderTotalCost();

                    foreach (var det in item.Details)
                    {
                        if (Config.UsePallets)
                        {
                            var qty = det.Qty;
                            if (det.UnitOfMeasure != null)
                                qty *= det.UnitOfMeasure.Conversion;

                            if (det.Product.SoldByWeight)
                                totalWeight += (det.Qty * det.Weight);

                            totalQty += det.Qty;
                        }
                        else
                        {
                            if (det.Product.SoldByWeight && det.Product.InventoryByWeight)
                            {
                                totalWeight += det.Weight;
                                if (det.Product.InventoryByWeight)
                                    totalQty++;
                            }
                            else
                            {
                                var qty = det.Qty;
                                if (det.UnitOfMeasure != null)
                                    qty *= det.UnitOfMeasure.Conversion;

                                totalQty += det.Qty;
                            }
                        }
                    }

                    list.Add(GetAcceptedOrderTableLineFixed(AcceptedOrdersTableLine, startIndex,
                                item.PrintedOrderId ?? "",
                                Math.Round(totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(totalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : totalAmount.ToCustomString()));
                    startIndex += font18Separation;


                    if ((item.OrderType == OrderType.WorkOrder || item.IsWorkOrder) && !string.IsNullOrEmpty(item.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderComment], startIndex, item.Comments));
                        startIndex += font18Separation;
                    }

                    section_totalQty += totalQty;
                    section_TotalWeight += totalWeight;
                    section_totalAmount += totalAmount;
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(GetAcceptedOrderTableTotalsFixed(AcceptedOrdersTableTotals, startIndex,
                                Math.Round(section_totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(section_TotalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : section_totalAmount.ToCustomString()));

                AddExtraSpace(ref startIndex, list, font36Separation, 1);

                #endregion
            }

            if (deliveries.Count > 0)
            {
                #region Deliveries

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersDeliveriesLabel], startIndex));
                startIndex += font36Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersDeliveriesTableHeader], startIndex));
                startIndex += font36Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                AddExtraSpace(ref startIndex, list, font18Separation, 1);

                float section_totalQty = 0;
                double section_TotalWeight = 0;
                double section_totalAmount = 0;

                foreach (var item in deliveries.OrderBy(x => x.Client.ClientName))
                {
                    float totalQty = 0;
                    double totalWeight = 0;
                    double totalAmount = item.OrderTotalCost();

                    foreach (var det in item.Details)
                    {
                        if (Config.UsePallets)
                        {
                            var qty = det.Qty;
                            if (det.UnitOfMeasure != null)
                                qty *= det.UnitOfMeasure.Conversion;

                            if (det.Product.SoldByWeight)
                                totalWeight += (det.Qty * det.Weight);

                            totalQty += det.Qty;
                        }
                        else
                        {
                            if (det.Product.SoldByWeight)
                            {
                                if (Config.NewAddItemRandomWeight)
                                {
                                    totalWeight += det.Weight;
                                    totalQty++;
                                }
                                else
                                {
                                    totalWeight += det.Weight;
                                    if (det.Product.InventoryByWeight)
                                        totalQty++;
                                }
                            }
                            else
                            {
                                var qty = det.Qty;
                                if (det.UnitOfMeasure != null)
                                    qty *= det.UnitOfMeasure.Conversion;

                                if (Config.NewAddItemRandomWeight && det.Product.Weight > 0)
                                    totalWeight += (det.Qty * det.Product.Weight);

                                totalQty += det.Qty;
                            }
                        }
                    }

                    section_totalQty += totalQty;
                    section_TotalWeight += totalWeight;
                    section_totalAmount += totalAmount;

                    int productLineOffset = 0;
                    foreach (string cName in GetAcceptedLoadSplitClientName(item.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptedOrderTableLineFixed(AcceptedOrdersTableLine, startIndex,
                                cName,
                                Math.Round(totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(totalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : totalAmount.ToCustomString()));
                        }
                        else
                            list.Add(GetAcceptedOrderTableLineFixed(AcceptedOrdersTableLine, startIndex,
                                cName, "", "", ""));

                        productLineOffset++;
                        startIndex += font18Separation;
                    }

                    string docType = "Order";
                    if (item.OrderType == OrderType.Credit)
                        docType = "Credit";
                    else if (item.OrderType == OrderType.Return)
                        docType = "Return";
                    else if (item.OrderType == OrderType.WorkOrder || item.IsWorkOrder)
                        docType = "Work Order";

                    list.Add(GetAcceptedOrderTableLine2Fixed(AcceptedOrdersTableLine2, startIndex,
                                docType,
                                item.PrintedOrderId ?? ""));
                    startIndex += font18Separation;

                    if ((item.OrderType == OrderType.WorkOrder || item.IsWorkOrder) && !string.IsNullOrEmpty(item.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderComment], startIndex, item.Comments));
                        startIndex += font18Separation;
                    }
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(GetAcceptedOrderTableTotalsFixed(AcceptedOrdersTableTotals, startIndex,
                                Math.Round(section_totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(section_TotalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : section_totalAmount.ToCustomString()));

                AddExtraSpace(ref startIndex, list, font36Separation, 1);

                full_totalQty += section_totalQty;
                full_totalWeight += section_TotalWeight;
                full_totalAmount += section_totalAmount;

                #endregion
            }

            if (credits.Count > 0)
            {
                #region Credits

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersCreditsLabel], startIndex));
                startIndex += font36Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersDeliveriesTableHeader], startIndex));
                startIndex += font36Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                AddExtraSpace(ref startIndex, list, font18Separation, 1);

                float section_totalQty = 0;
                double section_TotalWeight = 0;
                double section_totalAmount = 0;

                foreach (var item in credits.OrderBy(x => x.Client.ClientName))
                {
                    float totalQty = 0;
                    double totalWeight = 0;
                    double totalAmount = item.OrderTotalCost();

                    foreach (var det in item.Details)
                    {
                        if (Config.UsePallets)
                        {
                            var qty = det.Qty;
                            if (det.UnitOfMeasure != null)
                                qty *= det.UnitOfMeasure.Conversion;

                            if (det.Product.SoldByWeight)
                                totalWeight += (det.Qty * det.Weight);

                            totalQty += det.Qty;
                        }
                        else
                        {
                            if (det.Product.SoldByWeight)
                            {
                                totalWeight += det.Weight;
                                if (det.Product.InventoryByWeight)
                                    totalQty++;
                            }
                            else
                            {
                                var qty = det.Qty;
                                if (det.UnitOfMeasure != null)
                                    qty *= det.UnitOfMeasure.Conversion;

                                totalQty += det.Qty;
                            }
                        }
                    }

                    section_totalQty += totalQty;
                    section_TotalWeight += totalWeight;
                    section_totalAmount += totalAmount;

                    int productLineOffset = 0;
                    foreach (string cName in GetAcceptedLoadSplitClientName(item.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
                            list.Add(GetAcceptedOrderTableLineFixed(AcceptedOrdersTableLine, startIndex,
                                cName,
                                Math.Round(totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(totalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : totalAmount.ToCustomString()));
                        }
                        else
                            list.Add(GetAcceptedOrderTableLineFixed(AcceptedOrdersTableLine, startIndex,
                                cName, "", "", ""));

                        productLineOffset++;
                        startIndex += font18Separation;
                    }

                    string docType = "Order";
                    if (item.OrderType == OrderType.Credit)
                        docType = "Credit";
                    else if (item.OrderType == OrderType.Return)
                        docType = "Return";
                    else if (item.OrderType == OrderType.WorkOrder || item.IsWorkOrder)
                        docType = "Work Order";

                    list.Add(GetAcceptedOrderTableLine2Fixed(AcceptedOrdersTableLine2, startIndex,
                                docType,
                                item.PrintedOrderId ?? ""));
                    startIndex += font18Separation;

                    if ((item.OrderType == OrderType.WorkOrder || item.IsWorkOrder) && !string.IsNullOrEmpty(item.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderComment], startIndex, item.Comments));
                        startIndex += font18Separation;
                    }
                }

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
                startIndex += font18Separation;

                list.Add(GetAcceptedOrderTableTotalsFixed(AcceptedOrdersTableTotals, startIndex,
                                Math.Round(section_totalQty, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Math.Round(section_TotalWeight, Config.Round).ToString(CultureInfo.CurrentCulture),
                                Config.HidePriceInTransaction ? "" : section_totalAmount.ToCustomString()));

                AddExtraSpace(ref startIndex, list, font36Separation, 1);

                full_totalQty += section_totalQty;
                full_totalWeight += section_TotalWeight;
                full_totalAmount += section_totalAmount;

                #endregion
            }

            AddExtraSpace(ref startIndex, list, font18Separation, 1);

            //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersTotalsQty], startIndex, Math.Round(full_totalQty, Config.Round)));
            //startIndex += font36Separation;
            //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersTotalsWeight], startIndex, Math.Round(full_totalWeight, Config.Round)));
            //startIndex += font36Separation;
            //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AcceptedOrdersTotalsAmount], startIndex, full_totalAmount.ToCustomString()));
            //startIndex += font36Separation;

            return list;
        }

        public virtual string GetAcceptedOrderTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3, v4);
        }

        public virtual string GetAcceptedOrderTableLine2Fixed(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2);
        }

        public string GetAcceptedOrderTableTotalsFixed(string format, int pos, string v1, string v2, string v3)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v3);
        }

        #endregion

        #region Refusal Report

        public bool PrintRefusalReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetRefusalReportHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            if (Config.PrintRefusalReportByStore)
                lines.AddRange(NewGetRefusalReportTable(ref startY));
            else
                lines.AddRange(GetRefusalReportTable(ref startY));

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterDriverSignatureText], startY));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected IEnumerable<string> NewGetRefusalReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            var refusedOrders = Order.Orders.Where(x => !x.Voided).ToList();

            Dictionary<int, List<Order>> groupedOrders = new Dictionary<int, List<Order>>();

            foreach (var o in refusedOrders)
            {
                if (!groupedOrders.ContainsKey(o.Client.ClientId))
                    groupedOrders.Add(o.Client.ClientId, new List<Order>() { o });
                else
                    groupedOrders[o.Client.ClientId].Add(o);
            }

            startY -= font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportTableHeader1], startY));
            startY += font18Separation;
            startY += font18Separation;

            var no_delivery_list = Reason.GetReasonsByType(ReasonType.No_Delivery);

            foreach (var c in groupedOrders)
            {
                var client = Client.Find(c.Key);

                var displayClient = false;

                if (c.Value.Any(x => x.Reshipped))
                {
                    displayClient = true;
                }

                if (!displayClient)
                {
                    foreach (var o in c.Value)
                    {
                        if (displayClient)
                            break;

                        var details = o.Details.Where(x => x.LoadingError || no_delivery_list.Any(y => y.Id == x.ReasonId));

                        if (details.Count() > 0)
                            displayClient = true;
                    }
                }

                if (!displayClient)
                    continue;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportTableHeader], startY, client.ClientName));
                startY += 35;

                if (c.Value.Any(x => x.Reshipped))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportTableHeader], startY, "Refused All Products"));
                    startY += 35;
                }

                foreach (var o in c.Value.Where(x => x.Reshipped).ToList())
                {
                    string reasonName = "";
                    var reason = Reason.Find(o.ReasonId);
                    if (reason != null)
                    {
                        reasonName = "Reason: " + reason.Description;
                    }

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportTableHeader], startY, "Order#: " + o.PrintedOrderId + "  " + reasonName));
                    startY += 35;
                }


                var s = string.Empty;

                if (c.Value.Any(x => !x.Reshipped))
                {
                    if (c.Value.Any(x => x.Reshipped))
                        startY += font18Separation * 2;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportProductTableHeader], startY));
                    startY += font18Separation;

                    s = new string('-', WidthForNormalFont - s.Length) + s;
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                    startY += font18Separation;
                }

                bool AddedProducts = false;

                foreach (var o in c.Value.Where(x => !x.Reshipped).ToList())
                {
                    var details = o.Details.Where(x => x.LoadingError || no_delivery_list.Any(y => y.Id == x.ReasonId));
                    foreach (var d in details)
                    {
                        var isFirstLine = true;
                        foreach (string pName in SplitRefusalReportLines(d.Product.Name))
                        {
                            if (isFirstLine)
                            {
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportProductTableLine], startY, pName, Math.Round((d.Ordered - d.Qty), Config.Round), d.Reason.Description));
                                startY += font18Separation + 2;
                                isFirstLine = false;

                                AddedProducts = true;
                            }
                            else
                            {
                                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[NewRefusalReportProductTableLine], startY, pName, string.Empty, string.Empty));
                                startY += font18Separation + 2;

                                AddedProducts = true;
                            }
                        }
                    }
                }

                if (AddedProducts)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));

                startY += font18Separation;
                startY += font18Separation;
                startY += font18Separation;
            }

            startY += (font18Separation * 2);

            return lines;
        }
        protected IEnumerable<string> GetRefusalReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        protected IEnumerable<string> GetRefusalReportTable(ref int startY)
        {
            var refusedOrders = Order.Orders.Where(x => !x.Voided).ToList();

            Dictionary<int, List<Order>> map = new Dictionary<int, List<Order>>();

            foreach (var item in refusedOrders)
            {
                var no_delivery_list = Reason.GetReasonsByType(ReasonType.No_Delivery);

                var details = item.Details.Where(x => x.LoadingError || no_delivery_list.Any(y => y.Id == x.ReasonId));

                foreach (var detail in details)
                {
                    if (!map.ContainsKey(detail.ReasonId))
                        map.Add(detail.ReasonId, new List<Order>());
                    if (map[detail.ReasonId].FirstOrDefault(x => x.OrderId == item.OrderId) == null)
                        map[detail.ReasonId].Add(item);
                }
            }

            List<string> lines = new List<string>();

            foreach (var item in map)
            {
                Dictionary<Product, float> qtys = new Dictionary<Product, float>();

                var reason = Reason.Find(item.Key);
                string reasonName = reason != null ? reason.Description : "";

                var orders = item.Value;

                lines.AddRange(GetRefusalReportSectionPerReason(ref startY, reasonName, orders, qtys, item.Key));

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportProductTableHeader], startY));
                startY += font18Separation;

                var s = string.Empty;
                s = new string('-', WidthForNormalFont - s.Length) + s;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
                startY += font18Separation;

                foreach (var i in qtys)
                {
                    var isFirstLine = true;
                    foreach (string pName in SplitProductName(i.Key.Name, 60, 60))
                    {
                        if (isFirstLine)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportProductTableLine], startY, pName, Math.Round(i.Value, Config.Round)));
                            startY += font18Separation + 2;
                            isFirstLine = false;
                        }
                        else
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportProductTableLine], startY, pName, string.Empty));
                            startY += font18Separation + 2;
                        }
                    }
                }

                startY += font36Separation;
            }
            return lines;
        }

        protected IEnumerable<string> GetRefusalReportSectionPerReason(ref int startY, string reasonName, List<Order> orders, Dictionary<Product, float> qtys, int reasonid)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportTableHeader], startY, reasonName));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var item in orders)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[RefusalReportTableLine], startY, item.Client.ClientName, item.PrintedOrderId ?? ""));
                startY += font18Separation;


                var no_delivery_list = Reason.GetReasonsByType(ReasonType.No_Delivery);

                var details = item.Details.Where(x => x.LoadingError || no_delivery_list.Any(y => y.Id == x.ReasonId));

                foreach (var detail in details)
                {
                    if (detail.ReasonId != reasonid)
                        continue;

                    var key = qtys.Keys.FirstOrDefault(x => x.ProductId == detail.Product.ProductId);
                    if (key == null)
                    {
                        key = detail.Product;
                        qtys.Add(key, 0);
                    }

                    var qty = detail.Ordered - detail.Qty;
                    if (detail.UnitOfMeasure != null)
                        qty *= detail.UnitOfMeasure.Conversion;

                    qtys[key] = qtys[key] + qty;
                }
            }

            startY += font18Separation;

            return lines;
        }


        #endregion
        #region DEPRECATED

        #region Inventory

        public bool PrintInventory(IEnumerable<Product> SortedList)
        {
            //DEPRECATED
            return false;
        }

        #endregion

        #region Inventory Check

        public bool PrintInventoryCheck(IEnumerable<InventoryLine> SortedList)
        {
            //DEPRECATED
            return false;
        }

        #endregion

        #region Set Inventory

        public bool PrintSetInventory(IEnumerable<InventoryLine> SortedList)
        {
            //DEPRECATED
            return false;
        }

        #endregion

        #region Sales Credit Report

        public bool PrintSalesCreditReport()
        {
            //DEPRECATED
            return false;
        }

        public bool PrintProductLabel(string label)
        {
            try
            {
                DateTime st = DateTime.Now;
                PrintIt(label);
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public bool PrintCreditReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetCreditHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetCreditReportTable(ref startY));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected virtual IEnumerable<string> GetCreditHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportHeader], startY, index, count, startY + 20));
            startY += font36Separation + 10;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        public class CreditDetail
        {
            public Product Product { get; set; }

            public double Qty { get; set; }

            public double Price { get; set; }

            public bool Damaged { get; set; }

            public bool IsCredit { get; set; }
        }

        protected virtual IEnumerable<string> GetCreditReportTable(ref int startY)
        {
            List<string> lines = new List<string>();


            Dictionary<int, List<CreditDetail>> groupedReturns = new Dictionary<int, List<CreditDetail>>();

            var ordersToCheck = Order.Orders.Where(x => !x.Reshipped && !x.Voided).ToList();
            foreach (var order in ordersToCheck)
            {
                foreach (var detail in order.Details)
                {
                    if (detail.IsCredit)
                    {
                        if (groupedReturns.ContainsKey(order.Client.ClientId))
                        {
                            var alreadyThere = groupedReturns[order.Client.ClientId].FirstOrDefault(x => x.Product.ProductId == detail.Product.ProductId && x.Damaged == detail.Damaged && x.IsCredit == detail.IsCredit);
                            if (alreadyThere != null)
                                alreadyThere.Qty += detail.Qty;
                            else
                                groupedReturns[order.Client.ClientId].Add(new CreditDetail() { Product = detail.Product, Qty = detail.Qty, Price = detail.Price, Damaged = detail.Damaged, IsCredit = detail.IsCredit });
                        }
                        else
                            groupedReturns.Add(order.Client.ClientId, new List<CreditDetail>() { new CreditDetail() { Product = detail.Product, Qty = detail.Qty, Price = detail.Price, Damaged = detail.Damaged, IsCredit = detail.IsCredit } });
                    }
                }
            }

            double salesTotal = ordersToCheck.Sum(x => x.OrderSalesTotalCost());
            double creditTotal = ordersToCheck.Sum(x => x.OrderCreditTotalCost());

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            double totalItems = 0;

            foreach (var c in groupedReturns)
            {
                var client = Client.Find(c.Key);
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportClientName], startY, client.ClientName, "", ""));
                startY += font18Separation + 5;

                foreach (var detail in c.Value.ToList())
                {
                    totalItems += detail.Qty;

                    string name = "Return";
                    if (detail.Damaged)
                        name = "Dump";

                    bool isFirstLine = true;
                    foreach (var productName in SplitProductName(detail.Product.Name, 22, 50))
                    {
                        if (isFirstLine)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsLine], startY, productName, name, detail.Qty, detail.Price.ToCustomString(), (detail.Price * detail.Qty).ToCustomString()));
                            startY += font18Separation;
                            isFirstLine = false;
                            continue;
                        }


                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsLine], startY, productName, "", "", "", ""));
                        startY += font18Separation;
                    }

                    startY += 5;
                }
            }


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportDetailsTotal], startY, "TOTAL", totalItems, Math.Abs(creditTotal).ToCustomString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "SALES TOTAL:", salesTotal.ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "CREDIT TOTAL:", Math.Abs(creditTotal).ToCustomString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CreditReportTotalsLine], startY, "TOTAL:", (salesTotal + creditTotal).ToCustomString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }

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

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
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
                var productNameLines = SplitProductName(detail.Product.Name, 25, 25); //35
                double qty = detail.Qty;
                total += qty;

                var uomName = detail.UnitOfMeasure?.Name ?? string.Empty;
                var uomLines = SplitProductName(uomName, 10, 10);

                bool isFirstLine = true;
                foreach (var productNameLine in productNameLines)
                {
                    var currentUoM = isFirstLine && uomLines.Count > 0 ? uomLines[0] : string.Empty;

                    if (isFirstLine)
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsUoMDelivery], startY, productNameLine, qty, currentUoM)); //[OrderDetailsTotalsDelivery], startY, productNameLine, qty));
                        isFirstLine = false;
                    }
                    else
                    {
                        startY += font18Separation;
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotalsUoMDelivery], startY, productNameLine, string.Empty, string.Empty)); //[OrderDetailsTotalsDelivery], startY, productNameLine, string.Empty));
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

            string formatString = linesTemplates[OrderDetailsHeaderUoMDelivery]; //OrderDetailsHeaderDelivery

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

        #endregion

        #endregion

        #endregion

        public bool PrintVehicleInformation(bool FromEOD, int index = 0, int count = 0, bool isReport = false)
        {
            List<string> lines = new List<string>();

            var vehicleInfo = VehicleInformation.CurrentVehicleInformation;

            var endingVehicleInfo = VehicleInformation.EODVehicleInformation;

            if (vehicleInfo == null)
                return false;

            if (FromEOD && endingVehicleInfo == null)
                return false;

            int startY = 40;

            //var logoLabel = GetLogoLabel(ref startY, null);
            //if (!string.IsNullOrEmpty(logoLabel))
            //{
            //    lines.Add(logoLabel);
            //}

            //AddExtraSpace(ref startY, lines, 36, 1);

            if (FromEOD && count > 0 && index > 0 && isReport)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[VehicleInformationHeader], startY, index, count, startY + 20));
                startY += font36Separation + 10;
                startY += font18Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[VehicleInformationHeader1], startY));
                startY += font36Separation;
                startY += font18Separation;

            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BatchDate], startY, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;
            startY += font18Separation;

            lines.AddRange(GetCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            if (FromEOD)
            {
                if (isReport)
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Plate Number:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.PlateNumber));
                    startY += font18Separation;
                    startY += font18Separation;


                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Gasoline:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "   Put Gas: " + (endingVehicleInfo.PutGas ? "Yes" : "No")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "     Start: " + vehicleInfo.Gas.ToString()));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "       End: " + endingVehicleInfo.Gas.ToString()));
                    startY += font18Separation;

                    var gasDifference = endingVehicleInfo.PutGas ? Fraction.SubtractFractions(endingVehicleInfo.Gas, vehicleInfo.Gas) : Fraction.SubtractFractions(vehicleInfo.Gas, endingVehicleInfo.Gas);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Difference: " + gasDifference.ToString()));
                    startY += font18Separation;
                    startY += font18Separation;


                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Assistant:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.Assistant));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Miles at End Of Day:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "       End: " + (endingVehicleInfo.MilesFromDeparture > 1 ? endingVehicleInfo.MilesFromDeparture.ToString() + " Miles" : endingVehicleInfo.MilesFromDeparture.ToString() + " Mile")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "     Start: " + (vehicleInfo.MilesFromDeparture > 1 ? vehicleInfo.MilesFromDeparture.ToString() + " Miles" : vehicleInfo.MilesFromDeparture.ToString() + " Mile")));
                    startY += font18Separation;
                    var difference = endingVehicleInfo.MilesFromDeparture - vehicleInfo.MilesFromDeparture;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Difference: " + (difference > 1 ? difference.ToString() + " Miles" : difference.ToString() + " Mile")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Tire Condition:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + vehicleInfo.TireCondition));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "  End: " + endingVehicleInfo.TireCondition));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Seat Belts:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.SeatBelts));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Engine Oil:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.EngineOil ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.EngineOil ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Brake Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.BrakeFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.BrakeFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Power Steering Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.PowerSteeringFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.PowerSteeringFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Transmission Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.TransmissionFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.TransmissionFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Antifreeze / Coolant:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.AntifreezeCoolant ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.AntifreezeCoolant ? "Checked" : "Unchecked")));

                }
                else
                {

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Plate Number:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.PlateNumber));
                    startY += font18Separation;
                    startY += font18Separation;


                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Gasoline:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + endingVehicleInfo.Gas.ToString()));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Assistant:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.Assistant));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Miles at End Of Day:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.MilesFromDeparture > 1 ? endingVehicleInfo.MilesFromDeparture.ToString() + " Miles" : endingVehicleInfo.MilesFromDeparture.ToString() + " Mile")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Tire Condition:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + endingVehicleInfo.TireCondition));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Seat Belts:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.SeatBelts));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Engine Oil:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.EngineOil ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Brake Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.BrakeFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Power Steering Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.PowerSteeringFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Transmission Fluid:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.TransmissionFluid ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Antifreeze / Coolant:"));
                    startY += 30;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "End: " + (endingVehicleInfo.AntifreezeCoolant ? "Checked" : "Unchecked")));
                    startY += font18Separation;
                    startY += font18Separation;

                }
            }
            else
            {

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Plate Number:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.PlateNumber));
                startY += font18Separation;
                startY += font18Separation;


                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Gasoline:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + vehicleInfo.Gas.ToString()));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Assistant:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.Assistant));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Miles from Departure:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + (vehicleInfo.MilesFromDeparture > 1 ? vehicleInfo.MilesFromDeparture.ToString() + " Miles" : vehicleInfo.MilesFromDeparture.ToString() + " Mile")));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Tire Condition:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, "Start: " + vehicleInfo.TireCondition));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Seat Belts:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.SeatBelts));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Engine Oil:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.EngineOil ? "Checked" : "No Checked"));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Brake Fluid:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.BrakeFluid ? "Checked" : "No Checked"));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Power Steering Fluid:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.PowerSteeringFluid ? "Checked" : "No Checked"));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Transmission Fluid:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.TransmissionFluid ? "Checked" : "No Checked"));
                startY += font18Separation;
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startY, "Antifreeze / Coolant:"));
                startY += 30;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startY, vehicleInfo.AntifreezeCoolant ? "Checked" : "No Checked"));
                startY += font18Separation;
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += 18;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }


        #region pick Ticket

        public virtual bool PrintPickTicket(Order order)
        {
            int startY = 80;

            List<string> lines = new List<string>();


            if (Config.ButlerCustomization)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Butler Foods"));
                startY += font36Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Pick Ticket - Not an Invoice"));
                startY += font36Separation;
            }
            else
            {
                var logoLabel = GetLogoLabel(ref startY, order);
                if (!string.IsNullOrEmpty(logoLabel))
                {
                    lines.Add(logoLabel);
                }

                AddExtraSpace(ref startY, lines, 36, 1);


                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketCompanyHeader], startY, "Pick Ticket - Not an Invoice"));
                startY += font36Separation;

                lines.AddRange(GetCompanyRows(ref startY, order));

                AddExtraSpace(ref startY, lines, font18Separation, 1);
            }

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);
            if (salesman == null)
                salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            var truckName = string.Empty;
            var truck = Truck.Trucks.FirstOrDefault(x => x.DriverId == salesman.Id);
            if (truck != null)
                truckName = truck.Name;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketRouteInfo], startY, "Route #: " + truckName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDeliveryDate], startY, "Delivery Date: " + (order.ShipDate != DateTime.MinValue ? order.ShipDate.ToShortDateString() : DateTime.Now.ToShortDateString()), "Date: " + DateTime.Now.ToShortDateString()));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketDriver], startY, "Driver #: " + salesman.Name, "Time: " + DateTime.Now.ToShortTimeString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            var client = order.Client;

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

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startY, client.ContactPhone));
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

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "PO#: " + order.PONumber));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, "Ticket#: " + order.PrintedOrderId));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);


            var SE = string.Empty;
            SE = new string('-', WidthForNormalFont - SE.Length) + SE;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
            startY += font18Separation;

            List<T1> list = new List<T1>();

            foreach (var detail in order.Details)
            {
                var factor = 1;
                if (detail.IsCredit)
                    factor = -1;

                var found = list.FirstOrDefault(x => x.ProductId == detail.Product.ProductId && x.IsCredit == detail.IsCredit);
                if (found != null)
                {
                    if (detail.UnitOfMeasure != null)
                    {
                        var caseUOM = detail.Product.UnitOfMeasures.FirstOrDefault(x => !x.IsBase);
                        if (caseUOM != null)
                        {
                            var conversion = caseUOM.Conversion;
                            if (conversion > 1 && detail.Qty >= conversion)
                            {
                                int cases = (int)(detail.Qty / conversion);
                                int units = (int)(detail.Qty % conversion);

                                found.Cases += (cases * factor);
                                found.Units += (units * factor);
                            }
                            else
                                found.Units += (detail.Qty * factor);

                        }
                        else
                            found.Units += (detail.Qty * factor);
                    }
                    else
                        found.Units += (detail.Qty * factor);

                }
                else
                {
                    if (detail.UnitOfMeasure != null)
                    {
                        var caseUOM = detail.Product.UnitOfMeasures.FirstOrDefault(x => !x.IsBase);
                        if (caseUOM != null)
                        {
                            var conversion = caseUOM.Conversion;
                            if (conversion > 1 && detail.Qty >= conversion)
                            {
                                int cases = (int)(detail.Qty / conversion);
                                int units = (int)(detail.Qty % conversion);

                                list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = cases * factor, Units = units * factor, IsCredit = detail.IsCredit });
                            }
                            else
                                list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });

                        }
                        else
                            list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });
                    }
                    else
                        list.Add(new T1 { Product = detail.Product, ProductId = detail.Product.ProductId, Cases = 0, Units = (detail.Qty * factor), IsCredit = detail.IsCredit });
                }
            }

            list = list.OrderBy(x => x.Product.Description).ToList();

            foreach (var l in list)
            {
                string description = l.Product.Description;
                if (description.Length > 30)
                    description = description.Substring(0, 30);

                //make bold 
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, "", "", ""));
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, "", "", ""));
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, "", "", ""));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductLine], startY, l.Product.Code, description, l.Cases, l.Units));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, SE));
                startY += font18Separation;
            }

            //totals
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PickTicketProductTotal], startY, list.Sum(x => x.Cases), list.Sum(x => x.Units)));
            startY += font18Separation;
            startY += font18Separation;

            lines.Add(linesTemplates[EndLabel]);

            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        public class T1
        {
            public int ProductId { get; set; }
            public double Cases { get; set; }
            public double Units { get; set; }
            public Product Product { get; set; }

            public bool IsCredit { get; set; }
        }

        #endregion
    }
}