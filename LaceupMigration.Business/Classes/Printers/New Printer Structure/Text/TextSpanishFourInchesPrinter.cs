using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class TextSpanishFourInchesPrinter : TextFourInchesPrinter
    {
        protected override void FillDictionary()
        {
            linesTemplates.Add(EndLabel, "");
            linesTemplates.Add(StartLabel, "");

            linesTemplates.Add(Upc128, "");

            #region Standard

            linesTemplates.Add(StandarPrintTitle, "{1} {2}");
            linesTemplates.Add(StandarPrintDate, "Fecha: {1}");
            linesTemplates.Add(StandarPrintDateBig, "Fecha: {1}");
            linesTemplates.Add(StandarPrintRouteNumber, "Ruta #: {1}");
            linesTemplates.Add(StandarPrintDriverName, "Nombre del Chofer: {1}");
            linesTemplates.Add(StandarPrintCreatedBy, "Creado Por: {1}");
            linesTemplates.Add(StandarPrintedDate, "Fecha de Impresion: {1}");
            linesTemplates.Add(StandarPrintedOn, "Impreso En: {1}");
            linesTemplates.Add(StandarCreatedOn, "Creado En: {1}");

            #endregion

            #region Company

            linesTemplates.Add(CompanyName, "{1}");
            linesTemplates.Add(CompanyAddress, "{1}");
            linesTemplates.Add(CompanyPhone, "Telefono: {1}");
            linesTemplates.Add(CompanyFax, "Fax: {1}");
            linesTemplates.Add(CompanyEmail, "Correo: {1}");
            linesTemplates.Add(CompanyLicenses1, "Licencias: {1}");
            linesTemplates.Add(CompanyLicenses2, "           {1}");

            #endregion

            #region Order

            linesTemplates.Add(OrderClientName, "{1}");
            linesTemplates.Add(OrderClientNameTo, "Cliente: {1}");
            linesTemplates.Add(OrderClientAddress, "{1}");
            linesTemplates.Add(OrderBillTo, "Cobrar a: {1}");
            linesTemplates.Add(OrderBillTo1, "         {1}");
            linesTemplates.Add(OrderShipTo, "Enviar a: {1}");
            linesTemplates.Add(OrderShipTo1, "         {1}");
            linesTemplates.Add(OrderClientLicenceNumber, "Numero de Licencia: {1}");
            linesTemplates.Add(OrderVendorNumber, "Numero del Vendedor: {1}");
            linesTemplates.Add(OrderTerms, "Terminos: {1}");
            linesTemplates.Add(OrderAccountBalance, "Balance de la cuenta: {1}");
            linesTemplates.Add(OrderTypeAndNumber, "{2} #: {1}");
            linesTemplates.Add(PONumber, "OC #: {1}");

            linesTemplates.Add(OrderPaymentText, "{1}");
            linesTemplates.Add(OrderHeaderText, "{1}");

            linesTemplates.Add(OrderDetailsHeader, "PRODUCTO                          CANT       PRECIO   TOTAL");
            linesTemplates.Add(OrderDetailsLineSeparator, "{1}");
            linesTemplates.Add(OrderDetailsHeaderSectionName, "                               {1}");
            linesTemplates.Add(OrderDetailsLines, "{1} {2} {4} {3}");
            linesTemplates.Add(OrderDetailsLines2, "{1}");
            linesTemplates.Add(OrderDetailsLines3, "{1} {2}");
            linesTemplates.Add(OrderDetailsLinesLotQty, "Lote: {1} -> {2}");
            linesTemplates.Add(OrderDetailsWeights, "{1}");
            linesTemplates.Add(OrderDetailsWeightsCount, "Cant: {1}");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "Precio de Venta {1}");
            linesTemplates.Add(OrderDetailsLinesUpcText, "{1}");
            linesTemplates.Add(OrderDetailsLinesUpcBarcode, "");
            linesTemplates.Add(OrderDetailsLinesLongUpcBarcode, "");
            linesTemplates.Add(OrderDetailsTotals, "{1} {2} {3} {4}");
            linesTemplates.Add(OrderDetailsTotals1, "{1} {2} {3} {4}");
            linesTemplates.Add(OrderTotalsNetQty, "                          CANT NETA: {1}");
            linesTemplates.Add(OrderTotalsSales, "                             VENTAS: {1}");
            linesTemplates.Add(OrderTotalsCredits, "                           CREDITOS: {1}");
            linesTemplates.Add(OrderTotalsReturns, "                           RETORNOS: {1}");
            linesTemplates.Add(OrderTotalsNetAmount, "                         MONTO NETO: {1}");
            linesTemplates.Add(OrderTotalsDiscount, "                          DESCUENTO: {1}");
            linesTemplates.Add(OrderTotalsTax, "                    {1} {2}");
            linesTemplates.Add(OrderTotalsTotalDue, "                       TOTAL DEBIDO: {1}");
            linesTemplates.Add(OrderTotalsTotalPayment, "                       TOTAL PAGADO: {1}");
            linesTemplates.Add(OrderTotalsCurrentBalance, "                     BALANCE ACTUAL: {1}");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "                    BALANCE ABIERTO: {1}");
            linesTemplates.Add(OrderTotalsFreight,              "                            FREIGHT: {1}");
            linesTemplates.Add(OrderTotalsOtherCharges, "                      OTHER CHARGES: {1}");
            linesTemplates.Add(OrderTotalsDiscountComment, " Comentario del Descuento: {1}");
            linesTemplates.Add(OrderPreorderLabel, "{1}");
            linesTemplates.Add(OrderComment, "Commentario: {1}");
            linesTemplates.Add(OrderComment2, "          {1}");
            linesTemplates.Add(PaymentComment,  "Commentario del Pago: {1}");
            linesTemplates.Add(PaymentComment1, "                      {1}");
            linesTemplates.Add(OrderCommentWork, "{1}");

            #endregion

            #region Footer

            linesTemplates.Add(FooterSignatureLine, "----------------------------");
            linesTemplates.Add(FooterSignatureText, "Firma");
            linesTemplates.Add(FooterSignatureNameText, "Nombre de la Firma: {1}");
            linesTemplates.Add(FooterSpaceSignatureText, " ");
            linesTemplates.Add(FooterBottomText, "{1}");
            linesTemplates.Add(FooterDriverSignatureText, "Firma del Chofer");

            #endregion

            #region Allowance

            #endregion

            #region Shortage Report

            #endregion

            #region Load Order

            linesTemplates.Add(LoadOrderHeader, "Solicitud De Carga");
            linesTemplates.Add(LoadOrderRequestedDate, "Fecha de la Solicitud: {1}");
            linesTemplates.Add(LoadOrderNotFinal, "NO ES UNA SOLICITUD FINAL");
            linesTemplates.Add(LoadOrderTableHeader, "PRODUCTO                                 UDM   ORDEREDO");
            linesTemplates.Add(LoadOrderTableLine, "{1} {2} {3}");
            linesTemplates.Add(LoadOrderTableTotal, "Totales:                                             {1}");

            #endregion

            #region Accept Load

            linesTemplates.Add(AcceptLoadHeader, "Carga Acceptada");
            linesTemplates.Add(AcceptLoadDate, "Fecha de Impresion: {1}");
            linesTemplates.Add(AcceptLoadInvoice, "Factura #: {1}");
            linesTemplates.Add(AcceptLoadNotFinal, "NO ES UN DOCUMENTO FINAL");
            linesTemplates.Add(AcceptLoadTableHeader, "PRODUCTO                               UdM   CARGA AJUST INV");
            linesTemplates.Add(AcceptLoadTableHeader1,"                                             SOL");
            linesTemplates.Add(AcceptLoadTableLine, "{1} {2} {3} {4} {5}");
            linesTemplates.Add(AcceptLoadTableTotals, "                          Totales:     Unids {1} {2} {3}");

            #endregion


            #region Add Inventory

            linesTemplates.Add(AddInventoryHeader, "Carga Acceptada");
            linesTemplates.Add(AddInventoryDate, "Fecha de Impresion: {1}");
            linesTemplates.Add(AddInventoryNotFinal, "NO ES UN DOCUMENTO FINAL");
            linesTemplates.Add(AddInventoryTableHeader, "PRODUCTO                               INV   CARGA AJUST INV");
            linesTemplates.Add(AddInventoryTableHeader1, "                                       INIC  SOL         ACT");
            linesTemplates.Add(AddInventoryTableLine, "{1} {2} {3} {4} {5}");
            linesTemplates.Add(AddInventoryTableTotals, "                           Totales:    {1} {2} {3} {4}");

            #endregion

            #region Inventory

            linesTemplates.Add(InventoryProdHeader, "Fecha del Reporte de Inventario: {1}");
            linesTemplates.Add(InventoryProdTableHeader, "PRODUCTO                                       INIC   ACTUAL");
            linesTemplates.Add(InventoryProdTableLine, "{1} {2} {3}");
            linesTemplates.Add(InventoryProdTableLineLot, "                             Lote: {1} {2} {3}");
            linesTemplates.Add(InventoryProdTableLineListPrice, "Precio: {1}  Total: {2}");
            linesTemplates.Add(InventoryProdQtyItems, "          CANT TOTAL: {1}");
            linesTemplates.Add(InventoryProdInvValue, "       VALOR DEL INV: {1}");

            #endregion

            #region Orders Created

            linesTemplates.Add(OrderCreatedReportHeader, "Reporte de Ventas");
            linesTemplates.Add(OrderCreatedReporWorkDay, "Entrada: {1}  Salida: {2} Trabajado: {3}h:{4}m");
            linesTemplates.Add(OrderCreatedReporBreaks, "Descansos Tomados: {1}h:{2}m");
            linesTemplates.Add(OrderCreatedReportTableHeader, "NOMBRE                    EST CANT   RECIBO #.  TOTAL  CS TP");
            linesTemplates.Add(OrderCreatedReportTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(OrderCreatedReportTableLine1, "Entrada: {1}  Salida: {2}  # Copias: {3}");
            linesTemplates.Add(OrderCreatedReportTableTerms, "Terminos: {1}");
            linesTemplates.Add(OrderCreatedReportTableLineComment, "NS Comentario: {1}");
            linesTemplates.Add(OrderCreatedReportTableLineComment1, "RF Comentario: {1}");
            linesTemplates.Add(OrderCreatedReportSubtotal, "                                   Subtotal:  {1}");
            linesTemplates.Add(OrderCreatedReportTax,      "                                   Impuesto:  {1}");
            linesTemplates.Add(OrderCreatedReportTotals,   "                                      Total:  {1}");
            linesTemplates.Add(OrderCreatedReportPaidCust, "CL Pagado:           {1} Anulado:        {2}");
            linesTemplates.Add(OrderCreatedReportChargeCust, "CL Cargado:          {1} Entregado:      {2}");
            linesTemplates.Add(OrderCreatedReportCreditCust, "                     {1} P&P:            {2}");
            linesTemplates.Add(OrderCreatedReportExpectedCash, "CL Efect. Esperado:  {1} Reenviado:      {2}");
            linesTemplates.Add(OrderCreatedReportFullTotal, "Total De Ventas:     {1} Tiempo (Horas): {2}");

            linesTemplates.Add(OrderCreatedReportCreditTotal,  "                       Total de Credito: {2}");
            linesTemplates.Add(OrderCreatedReportBillTotal,    "                       Total en Cuentas: {2}");
            linesTemplates.Add(OrderCreatedReportSalesTotal,   "                        Total de Ventas: {2}");
            #endregion

            #region Payments Report

            linesTemplates.Add(PaymentReportHeader, "Reporte de Pagos");
            linesTemplates.Add(PaymentReportTableHeader, "Nombre             Numero     Total    Monto    Metodo Num");
            linesTemplates.Add(PaymentReportTableHeader1,"                   Factura    Factura                  Ref");
            linesTemplates.Add(PaymentReportTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(PaymentReportTotalCash,  "                                    Efectivo:     {1}");
            linesTemplates.Add(PaymentReportTotalCheck, "                                      Cheque:     {1}");
            linesTemplates.Add(PaymentReportTotalCC,         "                          Tarjeta de Credito:     {1}");
            linesTemplates.Add(PaymentReportTotalMoneyOrder, "                                 Money Order:     {1}");
            linesTemplates.Add(PaymentReportTotalTransfer,   "                               Transferencia:     {1}");
            linesTemplates.Add(PaymentReportTotalTotal,      "                                       Total:     {1}");
            linesTemplates.Add(PaymentSignatureText, "Pago Recibido Por");

            #endregion

            #region Settlement


            linesTemplates.Add(InventorySettlementHeader, "Reporte de Liquidacion                                Page: {1}/{2}");
            linesTemplates.Add(InventorySettlementProductHeader, "Producto                                                           ");
            linesTemplates.Add(InventorySettlementTableHeader, "UM     Inv  Carg Aj.  Tr.  Ven  Rbe  Rme  Rech D.C  Desc Inv  Dif");
            linesTemplates.Add(InventorySettlementTableHeader1, "");
            linesTemplates.Add(InventorySettlementProductLine, "{1}");
            linesTemplates.Add(InventorySettlementLotLine, "Lote: {1}");
            linesTemplates.Add(InventorySettlementTableLine, "{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}");
            linesTemplates.Add(InventorySettlementTableTotals, "       {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}");
            linesTemplates.Add(InventorySettlementTableTotals1, "       {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}");

            #endregion

            #region Summary

            linesTemplates.Add(InventorySummaryHeader, "Resumen de Inventario");
            linesTemplates.Add(InventorySummaryTableHeader, "Producto                                    ");
            linesTemplates.Add(InventorySummaryTableHeader1, "Lote UdM  Inv.Ini Carga  Transf   Ventas Inv.Actual");
            linesTemplates.Add(InventorySummaryTableProductLine, "{1}                             {2}{3}{4}{5}{6}{7} ");
            linesTemplates.Add(InventorySummaryTableLine, "{1}  {2}    {3}    {4}   {5}     {6}     {7}      ");
            linesTemplates.Add(InventorySummaryTableTotals, "Totales:{1}{2}{3}{4}    {5}   {6}     {7}     {8}      ");
            linesTemplates.Add(InventorySummaryTableTotals1, "       {1}{2}{3}{4}    {5}   {6}     {7}     {8}      ");

            #endregion


            #region Route Return

            linesTemplates.Add(RouteReturnsTitle, "Reporte de Devoluciones");
            linesTemplates.Add(RouteReturnsNotFinalLabel, "NO ES UN DOCUMENTO FINAL");
            linesTemplates.Add(RouteReturnsTableHeader, "Producto               RECH    RME     RBE    D.C    Desc.");
            linesTemplates.Add(RouteReturnsTableLine, "{1} {2} {3} {4} {5} {6}");
            linesTemplates.Add(RouteReturnsTotals, "Totales:                           {2} {3} {4} {5} {6}");

            #endregion

            #region Payment

            linesTemplates.Add(PaymentTitle, "Recibo de Pago");
            linesTemplates.Add(PaymentHeaderTo, "Cliente:");
            linesTemplates.Add(PaymentHeaderClientName, "{1}");
            linesTemplates.Add(PaymentHeaderClientAddr, "{1}");
            linesTemplates.Add(PaymentInvoiceNumber, "{1} #: {2}");
            linesTemplates.Add(PaymentInvoiceTotal, "{1} Total: {2}");
            linesTemplates.Add(PaymentPaidInFull, "Pagado Completo: {1}");
            linesTemplates.Add(PaymentComponents, "{1}");
            linesTemplates.Add(PaymentTotalPaid, "Total Pagado: {1}");
            linesTemplates.Add(PaymentPending, "   Pendiente: {1}");

            #endregion

            #region Open Invoice

            linesTemplates.Add(InvoiceTitle, "{1}");
            linesTemplates.Add(InvoiceCopy, "COPIA");
            linesTemplates.Add(InvoiceDueOn, "Vence En:    {1}");
            linesTemplates.Add(InvoiceDueOnOverdue, "Vence En:    {1} VENCIDO");
            linesTemplates.Add(InvoiceClientName, "{1}");
            linesTemplates.Add(InvoiceCustomerNumber, "Cliente: {1}");
            linesTemplates.Add(InvoiceClientAddr, "{1}");
            linesTemplates.Add(InvoiceClientBalance, "Balance de la Cuenta: {1}");
            linesTemplates.Add(InvoiceComment, "C: {1}");
            linesTemplates.Add(InvoiceTableHeader, "PRODUCTO                          CANT       PRECIO   TOTAL");
            linesTemplates.Add(InvoiceTableLine, "{1} {2} {3} {4}");
            linesTemplates.Add(InvoiceTotal, "{1} {2} {3} {4}");
            linesTemplates.Add(InvoicePaidInFull,     "PAGADO COMPLETO");
            linesTemplates.Add(InvoiceCredit,         "CREDITO");
            linesTemplates.Add(InvoicePartialPayment, " PAGADO PARCIAL: {1}");
            linesTemplates.Add(InvoiceOpen, "        ABIERTO: {1}");
            linesTemplates.Add(InvoiceQtyItems, "CANT. ARTICULOS: {1}");
            linesTemplates.Add(InvoiceQtyUnits, " CANT. UNIDADES: {1}");

            #endregion

            #region Transfer

            linesTemplates.Add(TransferOnHeader, "Reporte de Carga");
            linesTemplates.Add(TransferOffHeader, "Reporte de Descarga");
            linesTemplates.Add(TransferNotFinal, "NO ES UNA TRANSFERENCIA FINAL");
            linesTemplates.Add(TransferTableHeader, "Producto                       Lot       UdM   Transferido");
            linesTemplates.Add(TransferTableLine, "{1} {2} {3} {4}");
            linesTemplates.Add(TransferTableLinePrice, "   Precio de Venta: {1}");
            linesTemplates.Add(TransferQtyItems, "                       CANT ARTICULOS: {1}");
            linesTemplates.Add(TransferAmount,   "             VALOR DE LA TRANFERENCIA: {1}");
            linesTemplates.Add(TransferComment, "Comentario: {1}");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "Balance del Cliente");
            linesTemplates.Add(ClientStatementTableHeader, "Tipo    Fecha   Numero    F.Venc    Cant.   Balance");

            linesTemplates.Add(ClientStatementTableHeader1, "");
            linesTemplates.Add(ClientStatementTableLine, "{1}     {2}     {3}       {4}       {5}     {6}    ");

            linesTemplates.Add(ClientStatementCurrent, "Cantidad Actual :                            {1}");
            linesTemplates.Add(ClientStatement1_30PastDue, "1-30 Dias Vencidos:                          {1}");
            linesTemplates.Add(ClientStatement31_60PastDue, "31-60 Dias Vencidos:                         {1}");
            linesTemplates.Add(ClientStatement61_90PastDue, "61-90 Days Past Due:                         {1}");
            linesTemplates.Add(ClientStatementOver90PastDue, "Mas de 90 Dias Vencidos:                     {1}");
            linesTemplates.Add(ClientStatementAmountDue, "Cantidad Vencida:                            {1}");

            #endregion

            #region Inventory Count

            linesTemplates.Add(InventoryCountHeader, "Cuenta de Inventario");
            linesTemplates.Add(InventoryCountTableHeader, "PRODUCTO              CANT          UdM   ");
            linesTemplates.Add(InventoryCountTableLine, "{1}                   {2}           {3}   ");

            #endregion

            #region Accepted Orders Report

            linesTemplates.Add(AcceptedOrdersHeader, "Reporte de Ordenes Aceptadas");
            linesTemplates.Add(AcceptedOrdersDate, "Fecha de Impresion: {1}");
            linesTemplates.Add(AcceptedOrdersDeliveriesLabel, "Entregas");
            linesTemplates.Add(AcceptedOrdersCreditsLabel, "Creditos");
            linesTemplates.Add(AcceptedOrdersDeliveriesTableHeader, "Cliente            Cant    Peso       Monto  ");
            linesTemplates.Add(AcceptedOrdersTableLine, "{1}                {2}     {3}        {4}    ");
            linesTemplates.Add(AcceptedOrdersTableLine2, "{1}          {2}                             ");
            linesTemplates.Add(AcceptedOrdersLoadsTableHeader, "Ordenes Montadas");
            linesTemplates.Add(AcceptedOrdersTableTotals, "Totales:           {1}     {2}        {3}    ");
            linesTemplates.Add(AcceptedOrdersTotalsQty, "                Cant Total:         {1}");
            linesTemplates.Add(AcceptedOrdersTotalsWeight, "                 Peso Total:        {1}");
            linesTemplates.Add(AcceptedOrdersTotalsAmount, "                 Monto:             {1}");
            #endregion

            #region Refusal Report

            linesTemplates.Add(RefusalReportHeader, "Refusal Report {3}        Page: {1}/{2}");
            linesTemplates.Add(RefusalReportTableHeader, "Reason: {1}              Order #");
            linesTemplates.Add(RefusalReportTableLine, "{1}                      {2}    ");
            linesTemplates.Add(RefusalReportProductTableHeader, "Product                  Qty    ");
            linesTemplates.Add(RefusalReportProductTableLine, "{1}                      {2}    ");

            #endregion

            #region Payment Deposit
            linesTemplates.Add(ChecksTitle, "Lista de Cheques");
            linesTemplates.Add(BatchDate, "Fecha de Publicacion: {1}");
            linesTemplates.Add(BatchPrintedDate, "Fecha de Impresion: {1}");
            linesTemplates.Add(BatchSalesman, "Vendedor: {1}");
            linesTemplates.Add(BatchBank, "Banco: {1}");
            linesTemplates.Add(CheckTableHeader, "IDENTICACION DEL CHECQUE         CANTIDAD");
            linesTemplates.Add(CheckTableLine, "{1}                              {2}");

            linesTemplates.Add(CheckTableTotal, "# DE CHEQUES: {1}    TOTAL CHEQUES: {2}");

            linesTemplates.Add(CashTotalLine, "TOTAL EFECTIVO: {1}");
            linesTemplates.Add(CreditCardTotalLine, "TOTAL TARJETA DE CREDITO: {1}");
            linesTemplates.Add(MoneyOrderTotalLine, "TOTAL GIRO POSTAL: {1}");

            linesTemplates.Add(BatchTotal, "TOTAL DEPOSITO: {1}");
            linesTemplates.Add(BatchComments, "Commentarios: {1}");

            #endregion
        }


        #region Print Order

        protected override string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            string docName = "Factura";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Cuenta";
                    printExtraDocName = true;
                }
            }
            if (order.AsPresale)
            {
                docName = "Orden de Venta";
                printExtraDocName = false;
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credito";
                printExtraDocName = true;
            }

            return docName;
        }

        protected override IEnumerable<string> GetPaymentLines(ref int startY, IList<PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();

            if (payments.Count == 1)
            {
                if (paidInFull)
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado Completo En Efectivo:"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado Completo"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Num Cheque:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado Completo"));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Money Order #:" + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado Completo en Tarjeta de Credito"));
                            startY += font36Separation;
                            break;
                    }
                }
                else
                {
                    switch (payments[0].PaymentMethod)
                    {
                        case InvoicePaymentMethod.Cash:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado " + ToString(payments[0].Amount) + "  Cash"));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Check:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Pagado  " + ToString(payments[0].Amount)));
                            startY += font36Separation;
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "Cheque " + payments[0].Ref));
                            startY += font36Separation;
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, "pagado  " + ToString(payments[0].Amount)));
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
                    sb.Append("Pagado Completo");
                else
                    sb.Append("Pagado " + ToString(payments.Sum(x => x.Amount)));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPaymentText], startY, sb.ToString()));
                startY += font36Separation;
            }

            return lines;
        }

        protected override List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            string docName = "NO ES UNA FACTURA";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "NO ES UNA CUENTA";
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
                if (Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, "FACTURA FINAL"));
                    startY += font36Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICADO" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, name));
                startY += font36Separation;
            }

            return lines;
        }

        protected override void HidePriceInOrderPrintedLine(ref string formatString)
        {
            formatString = formatString.Replace("PRECIO", "");
        }

        protected override void HideTotalInOrderPrintedLine(ref string formatString)
        {
            formatString = formatString.Replace("TOTAL", "");
        }

        protected override string GetOrderDetailSectionHeader(int factor)
        {
            switch (factor)
            {
                case -1:
                    return "SECCION DE VENTAS";
                case 0:
                    return "SECCION DE DAÑADOS";
                case 1:
                    return "SECION DE RETORNOS";
                default:
                    return "SECCION DE VENTAS";
            }
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

            if (uomMap.Keys.Count > 0)
            {
                if (!uomMap.ContainsKey("Unidades"))
                    uomMap.Add("Unidades", totalQtyNoUoM);
            }
            else
            {
                uomMap.Add("Totales", totalQtyNoUoM);

                if (uomMap.Keys.Count == 1 && totalUnits != totalQtyNoUoM && sectionName == "SALES SECTION" && !uomMap.ContainsKey("Unidades"))
                    uomMap.Add("Unidades", totalUnits);
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

                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals1, startIndex, string.Empty, adjustedKey, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), balanceText)); //key //uoMLines[0]
                    startIndex += font18Separation;

                    offset++;
                }
            }

            return list;
        }

        protected override string GetOrderPreorderLabel(Order order)
        {
            return order.PrintedCopies > 0 ? "DUPLICADO" : "ORIGINAL";
        }

        #endregion

        #region Orders Created Report

        protected override string GetCreatedOrderStatus(Order o)
        {
            string status = string.Empty;

            if (o.OrderType == OrderType.NoService)
                status = "NS";
            if (o.Voided)
                status = "AN";
            if (o.Reshipped)
                status = "RE";

            if (o.OrderType == OrderType.Bill)
                status = "CU";

            return status;
        }

        protected override string GetCreatedOrderType(Order o, double paid, double orderCost)
        {
            string type = "";
            if (paid == 0)
                type = "Cargo";
            else if (paid < orderCost)
                type = "Parcial P.";
            else
                type = "Pagado";

            if (o.OrderTotalCost() < 0)
                type = "Credito";

            return type;
        }

        #endregion

        #region Consignment Invoice

        protected override List<string> GetConsignmentInvoiceLabel(ref int startY, Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            string docName = "NO ES UNA FACTURA";

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
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, "FACTURA FINAL"));
                    startY += font36Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = order.PrintedCopies > 0 ? "DUPLICADO" : "ORIGINAL";
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, name));
                startY += font36Separation;
            }

            return lines;
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

        protected override string GetPaymentInvoiceNumberLine(bool moreThanOne)
        {
            string invoiceNumberLine = "Factura";
            if (moreThanOne)
                invoiceNumberLine = "Facturas";

            return invoiceNumberLine;
        }

        protected override IEnumerable<string> GetPaymentComponents(ref int startY, InvoicePayment invoicePayment)
        {
            List<string> lines = new List<string>();

            foreach (var component in invoicePayment.Components)
            {
                var pm = component.PaymentMethod.ToString().Replace("_", " ");
                if (pm.Length < 11)
                    pm = new string(' ', 11 - pm.Length) + pm;
                string s = string.Format("Metodo: {0}  Monto: {1}", pm, ToString(component.Amount));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, s));
                startY += font18Separation;

                if (!string.IsNullOrEmpty(component.Ref))
                {

                    string refName = "Ref: {0}";
                    switch (component.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            refName = "Cheque: {0}";
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            refName = "Money Order: {0}";
                            break;
                    }
                    s = string.Format(refName, component.Ref);
                    if (!string.IsNullOrEmpty(component.Comments))
                        s = s + " Commentario: " + component.Comments;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, s));
                    startY += font18Separation;
                }
                else
                {
                    if (!string.IsNullOrEmpty(component.Comments))
                    {
                        var temp_comments = "Commentario: " + component.Comments;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentComponents], startY, temp_comments));
                        startY += font18Separation;
                    }
                }
                startY += font18Separation / 2;
            }

            return lines;
        }

        #endregion

        #region Open Invoice

        protected override string GetInvoiceType(Invoice invoice)
        {
            var headerText = "Factura #: ";

            if (invoice.InvoiceType == 1)
                headerText = "Credito #: ";
            else if (invoice.InvoiceType == 2)
                headerText = "Estimado #: ";
            else if (invoice.InvoiceType == 3)
                headerText = "Orden #: ";

            return headerText;
        }

        protected override Product GetNotFoundInvoiceProduct()
        {
            Product notFoundProduct = new Product();
            notFoundProduct.Code = string.Empty;
            notFoundProduct.Cost = 0;
            notFoundProduct.Description = "Producto No Encontrado";
            notFoundProduct.Name = "Producto No Encontrado";
            notFoundProduct.Package = "1";
            notFoundProduct.ProductType = ProductType.Inventory;
            notFoundProduct.UoMFamily = string.Empty;
            notFoundProduct.Upc = string.Empty;

            return notFoundProduct;
        }

        #endregion
    }
}