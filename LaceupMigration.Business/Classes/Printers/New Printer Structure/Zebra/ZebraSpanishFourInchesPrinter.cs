using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ZebraSpanishFourInchesPrinter : ZebraFourInchesPrinter1
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
            linesTemplates.Add(StandarPrintDate, "^FO40,{0}^ADN,18,10^FDFecha: {1}^FS");
            linesTemplates.Add(StandarPrintDateBig, "^CF0,30^FO40,{0}^FDFecha: {1}^FS");
            linesTemplates.Add(StandarPrintRouteNumber, "^FO40,{0}^ADN,18,10^FDRuta #: {1}^FS");
            linesTemplates.Add(StandarPrintDriverName, "^FO40,{0}^ADN,18,10^FDNombre del chofer: {1}^FS");
            linesTemplates.Add(StandarPrintCreatedBy, "^FO40,{0}^ADN,18,10^FDVendedor: {1}^FS");
            linesTemplates.Add(StandarPrintedDate, "^FO40,{0}^ADN,18,10^FDFecha de Impresion: {1}^FS");
            linesTemplates.Add(StandarPrintedOn, "^FO40,{0}^ADN,18,10^FDImpreso En: {1}^FS");
            linesTemplates.Add(StandarCreatedOn, "^FO40,{0}^ADN,18,10^FDCreado En: {1}^FS");

            #endregion

            #region Company

            linesTemplates.Add(CompanyName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(CompanyAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(CompanyPhone, "^FO40,{0}^ADN,18,10^FDTelefono: {1}^FS");
            linesTemplates.Add(CompanyFax, "^FO40,{0}^ADN,18,10^FDFax: {1}^FS");
            linesTemplates.Add(CompanyEmail, "^FO40,{0}^ADN,18,10^FDCorreo: {1}^FS");
            linesTemplates.Add(CompanyLicenses1, "^FO40,{0}^ADN,18,10^FDLicencias: {1}^FS");
            linesTemplates.Add(CompanyLicenses2, "^FO40,{0}^ADN,18,10^FD           {1}^FS");

            #endregion

            #region Order

            linesTemplates.Add(OrderClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderClientNameTo, "^FO40,{0}^ADN,18,10^FDCliente: {1}^FS");
            linesTemplates.Add(OrderClientAddress, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderBillTo, "^FO40,{0}^ADN,18,10^FDCobrar a: {1}^FS");
            linesTemplates.Add(OrderBillTo1, "^FO40,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderShipTo, "^FO40,{0}^ADN,18,10^FDEnviar a: {1}^FS");
            linesTemplates.Add(OrderShipTo1, "^FO40,{0}^ADN,18,10^FD         {1}^FS");
            linesTemplates.Add(OrderClientLicenceNumber, "^FO40,{0}^ADN,18,10^FDNumero de Licencia: {1}^FS");
            linesTemplates.Add(OrderVendorNumber, "^FO40,{0}^ADN,18,10^FDNumero del Vendedor: {1}^FS");
            linesTemplates.Add(OrderTerms, "^FO40,{0}^ADN,18,10^FDTerminos: {1}^FS");
            linesTemplates.Add(OrderAccountBalance, "^FO40,{0}^ADN,18,10^FDBalance de Cuenta: {1}^FS");
            linesTemplates.Add(OrderTypeAndNumber, "^FO40,{0}^ADN,36,20^FD{2} #: {1}^FS");
            linesTemplates.Add(PONumber, "^FO40,{0}^ADN,36,20^FDOC #: {1}^FS");

            linesTemplates.Add(OrderPaymentText, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderHeaderText, "^FO40,{0}^ADN,36,20^FD{1}^FS");

            linesTemplates.Add(OrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO450,{0}^ADN,18,10^FDCANT^FS" +
                "^FO580,{0}^ADN,18,10^FDPRECIO^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(OrderDetailsLineSeparator, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsHeaderSectionName, "^FO400,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(OrderDetailsLines2, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderDetailsLines3, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO450,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(OrderDetailsLinesLotQty, "^FO40,{0}^ADN,18,10^FDLote: {1} -> {2}^FS");
            linesTemplates.Add(OrderDetailsWeights, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(OrderDetailsWeightsCount, "^FO40,{0}^ADN,18,10^FDCant: {1}^FS");
            linesTemplates.Add(OrderDetailsLinesRetailPrice, "^FO40,{0}^ADN,18,10^FDPrecio de Venta {1}^FS");
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
            linesTemplates.Add(OrderTotalsNetQty, "^FO40,{0}^ADN,36,20^FD      CANT NETA: {1}^FS");
            linesTemplates.Add(OrderTotalsSales, "^FO40,{0}^ADN,36,20^FD         VENTAS: {1}^FS");
            linesTemplates.Add(OrderTotalsCredits, "^FO40,{0}^ADN,36,20^FD       CREDITOS: {1}^FS");
            linesTemplates.Add(OrderTotalsReturns, "^FO40,{0}^ADN,36,20^FD       RETORNOS: {1}^FS");
            linesTemplates.Add(OrderTotalsNetAmount, "^FO40,{0}^ADN,36,20^FD     MONTO NETO: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscount, "^FO40,{0}^ADN,36,20^FD      DESCUENTO: {1}^FS");
            linesTemplates.Add(OrderTotalsTax, "^FO40,{0}^ADN,36,20^FD{1} {2}^FS");
            linesTemplates.Add(OrderTotalsTotalDue, "^FO40,{0}^ADN,36,20^FD   TOTAL DEBIDO: {1}^FS");
            linesTemplates.Add(OrderTotalsTotalPayment, "^FO40,{0}^ADN,36,20^FD   TOTAL PAGADO: {1}^FS");
            linesTemplates.Add(OrderTotalsCurrentBalance, "^FO40,{0}^ADN,36,20^FD BALANCE ACTUAL: {1}^FS");
            linesTemplates.Add(OrderTotalsClientCurrentBalance, "^FO40,{0}^ADN,36,20^FDBALANCE ABIERTO: {1}^FS");
            linesTemplates.Add(OrderTotalsFreight,              "^FO40,{0}^ADN,36,20^FD        FREIGHT: {1}^FS");
            linesTemplates.Add(OrderTotalsOtherCharges, "^FO40,{0}^ADN,36,20^FD  OTHER CHARGES: {1}^FS");
            linesTemplates.Add(OrderTotalsDiscountComment, "^FO40,{0}^ADN,18,10^FD Comentario del Descuento: {1}^FS");
            linesTemplates.Add(OrderPreorderLabel, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(OrderComment, "^FO40,{0}^ADN,18,10^FDCommentario: {1}^FS");
            linesTemplates.Add(OrderComment2, "^FO40,{0}^ADN,18,10^FD          {1}^FS");
            linesTemplates.Add(PaymentComment,  "^FO40,{0}^ADN,18,10^FDCommentario del Pago: {1}^FS");
            linesTemplates.Add(PaymentComment1, "^FO40,{0}^ADN,18,10^FD                      {1}^FS");
            linesTemplates.Add(OrderCommentWork, "^FO40,{0}^AON,24,15^FD{1}^FS");

            #endregion

            #region Footer

            linesTemplates.Add(FooterSignatureLine, "^FO40,{0}^ADN,18,10^FD----------------------------^FS");
            linesTemplates.Add(FooterSignatureText, "^FO40,{0}^ADN,18,10^FDFirma^FS");
            linesTemplates.Add(FooterSignatureNameText, "^FO40,{0}^ADN,18,10^FDNombre de la Firma: {1}^FS");
            linesTemplates.Add(FooterSpaceSignatureText, "^FO40,{0}^ADN,18,10^FD ^FS");
            linesTemplates.Add(FooterBottomText, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(FooterDriverSignatureText, "^FO40,{0}^ADN,18,10^FDFirma del chofer^FS");

            #endregion

            #region Allowance

            linesTemplates.Add(AllowanceOrderDetailsHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO380,{0}^ADN,18,10^FDCANT^FS" +
                "^FO480,{0}^ADN,18,10^FDPRECIO^FS" +
                "^FO580,{0}^ADN,18,10^FDDESC^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(AllowanceOrderDetailsLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO380,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO580,{0}^ADN,18,10^FD-{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{5}^FS");

            #endregion

            #region Shortage Report

            linesTemplates.Add(ShortageReportHeader, "^FO40,{0}^ADN,36,20^FDREPORTE DE FALTANTE^FS");
            linesTemplates.Add(ShortageReportDate, "^FO400,{0}^ADN,18,10^FDFecha: {1}^FS");
            linesTemplates.Add(ShortageReportInvoiceHeader, "^FO40,{0}^ADN,36,20^FDFactura #: {1}^FS");
            linesTemplates.Add(ShortageReportTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO500,{0}^ADN,18,10^FDOC CANT^FS" +
                "^FO600,{0}^ADN,18,10^FDSHORT.^FS" +
                "^FO710,{0}^ADN,18,10^FDDEL.^FS");
            linesTemplates.Add(ShortageReportTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO500,{0}^ADN,18,10^FD{2}^FS" +
                "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                "^FO710,{0}^ADN,18,10^FD{4}^FS");

            #endregion

            #region Load Order

            linesTemplates.Add(LoadOrderHeader, "^FO40,{0}^ADN,36,20^FDSolicitud De Carga^FS");
            linesTemplates.Add(LoadOrderRequestedDate, "^FO40,{0}^ADN,18,10^FDFecha de la Solicitud: {1}^FS");
            linesTemplates.Add(LoadOrderNotFinal, "^FO40,{0}^ADN,28,14^FDNO ES UNA SOLICITUD FINAL^FS");
            linesTemplates.Add(LoadOrderTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO600,{0}^ADN,18,10^FDUOM^FS" +
                "^FO680,{0}^ADN,18,10^FDORDENADO^FS");
            linesTemplates.Add(LoadOrderTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(LoadOrderTableTotal, "^FO40,{0}^ADN,18,10^FDTotales:^FS" +
                "^FO680,{0}^ADN,18,10^FD{1}^FS");

            #endregion

            #region Accept Load

            linesTemplates.Add(AcceptLoadHeader, "^FO40,{0}^ADN,36,20^FDCarga Acceptada^FS");
            linesTemplates.Add(AcceptLoadDate, "^FO350,{0}^ADN,18,10^FDFecha de Impresion: {1}^FS");
            linesTemplates.Add(AcceptLoadInvoice, "^FO40,{0}^ADN,36,20^FDFactura #: {1}^FS");
            linesTemplates.Add(AcceptLoadNotFinal, "^FO40,{0}^ADN,28,14^FDNO ES UN DOCUMENTO FINAL^FS");
            linesTemplates.Add(AcceptLoadTableHeader, "^FO40,{0}^ABN,18,10^FDPRODUCTO^FS" +
                //"^FO490,{0}^ABN,18,10^FDUdM^FS" +
                "^FO560,{0}^ABN,18,10^FDCARGA^FS" +
                "^FO630,{0}^ABN,18,10^FDAJUSTE^FS" +
                "^FO700,{0}^ABN,18,10^FDINV^FS");
            linesTemplates.Add(AcceptLoadTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO490,{0}^ABN,18,10^FD^FS" +
                "^FO560,{0}^ABN,18,10^FDSOL^FS" +
                "^FO630,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(AcceptLoadTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO490,{0}^ABN,18,10^FD{2}^FS" +
                "^FO560,{0}^ABN,18,10^FD{3}^FS" +
                "^FO630,{0}^ABN,18,10^FD{4}^FS" +
                "^FO700,{0}^ABN,18,10^FD{5}^FS");

            linesTemplates.Add(AcceptLoadLotLine, "^FO40,{0}^ABN,18,10^FDLote: {1}^FS");
            linesTemplates.Add(AcceptLoadWeightLine, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(AcceptLoadTableTotals, "^FO350,{0}^ABN,18,10^FD^FS" +
                "^FO490,{0}^ABN,18,10^FDUnidad^FS" +
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

            linesTemplates.Add(AddInventoryHeader, "^FO40,{0}^ADN,36,20^FDCarga Acceptada^FS");
            linesTemplates.Add(AddInventoryDate, "^FO350,{0}^ADN,18,10^FDFecha de Impresion: {1}^FS");
            linesTemplates.Add(AddInventoryNotFinal, "^FO40,{0}^ADN,28,14^FDNO ES UN DOCUMENTO FINAL^FS");
            linesTemplates.Add(AddInventoryTableHeader, "^FO40,{0}^ABN,18,10^FDPRODUCTO^FS" +
                "^FO490,{0}^ABN,18,10^FDINV^FS" +
                "^FO560,{0}^ABN,18,10^FDCARGA^FS" +
                "^FO630,{0}^ABN,18,10^FDAJUSTE^FS" +
                "^FO700,{0}^ABN,18,10^FDINV^FS");
            linesTemplates.Add(AddInventoryTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO490,{0}^ABN,18,10^FDINIC^FS" +
                "^FO560,{0}^ABN,18,10^FDSOL^FS" +
                "^FO630,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FDACT^FS");
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

            linesTemplates.Add(InventoryProdHeader, "^FO40,{0}^ADN,18,10^FDFecha del Reporte de Inventario: {1}^FS");
            linesTemplates.Add(InventoryProdTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO580,{0}^ADN,18,10^FDINICIAL^FS" +
                "^FO680,{0}^ADN,18,10^FDACTUAL^FS");
            linesTemplates.Add(InventoryProdTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO580,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineLot, "^FO400,{0}^ADN,18,10^FDLote: {1}^FS" +
                "^FO600,{0}^ADN,18,10^FD{2}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(InventoryProdTableLineListPrice, "^FO40,{0}^ADN,18,10^FDPrecio: {1}  Total: {2}^FS");
            linesTemplates.Add(InventoryProdQtyItems, "^FO40,{0}^ADN,36,20^FD   CANT TOTAL: {1}^FS");
            linesTemplates.Add(InventoryProdInvValue, "^FO40,{0}^ADN,36,20^FDVALOR DEL INV: {1}^FS");

            #endregion

            #region Orders Created

            linesTemplates.Add(OrderCreatedReportHeader, "^CF0,50^FO40,{0}^FDReporte De Ventas^FS^CF0,25^FO650,{3}^FDPag: {1}/{2}^FS");
            linesTemplates.Add(OrderCreatedReporWorkDay, "^FO40,{0}^ADN,18,10^FDEntrada: {1}  Salida: {2} Trabajado: {3}h:{4}m^FS");
            linesTemplates.Add(OrderCreatedReporBreaks, "^FO40,{0}^ADN,18,10^FDDescansos Tomados: {1}h:{2}m^FS");
            linesTemplates.Add(OrderCreatedReportTableHeader, "^FO40,{0}^ABN,18,10^FDNOMBRE^FS" +
                "^FO350,{0}^ABN,18,10^FDEST^FS" +
                "^FO400,{0}^ABN,18,10^FDCANT^FS" +
                "^FO480,{0}^ABN,18,10^FDRECIBO #.^FS" +
                "^FO610,{0}^ABN,18,10^FDTOTAL^FS" +
                "^FO700,{0}^ABN,18,10^FDCS TP^FS");
            linesTemplates.Add(OrderCreatedReportTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO350,{0}^ABN,18,10^FD{2}^FS" +
                "^FO400,{0}^ABN,18,10^FD{3}^FS" +
                "^FO480,{0}^ABN,18,10^FD{4}^FS" +
                "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(OrderCreatedReportTableLine1, "^FO40,{0}^ABN,18,10^FDEntrada: {1}    Salida: {2}     # Copias: {3}^FS");
            linesTemplates.Add(OrderCreatedReportTableTerms, "^FO40,{0}^ADN,18,10^FDTerminos: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment, "^FO40,{0}^ABN,18,10^FDNS Comentario: {1}^FS");
            linesTemplates.Add(OrderCreatedReportTableLineComment1, "^FO40,{0}^ABN,18,10^FDRF Comentario: {1}^FS");
            linesTemplates.Add(OrderCreatedReportSubtotal, "^FO510,{0}^ABN,18,10^FDSubtotal:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTax, "^FO510,{0}^ABN,18,10^FDImpuesto:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportTotals, "^FO510,{0}^ABN,18,10^FDTotal:^FS^FO610,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(OrderCreatedReportPaidCust, "^FO40,{0}^ABN,18,10^FDCL Pagado:           {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDAnulado:        {2}^FS");
            linesTemplates.Add(OrderCreatedReportChargeCust, "^FO40,{0}^ABN,18,10^FDCL Cargado:          {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDEntregado:      {2}^FS");
            linesTemplates.Add(OrderCreatedReportCreditCust, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FDP&P:            {2}^FS");
            linesTemplates.Add(OrderCreatedReportExpectedCash, "^FO40,{0}^ABN,18,10^FDCL Efect. Esperado:  {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDReenviado:      {2}^FS");
            linesTemplates.Add(OrderCreatedReportFullTotal, "^FO40,{0}^ABN,18,10^FDTotal De Ventas:     {1}^FS" +
                               "^FO500,{0}^ABN,18,10^FDTiempo (Horas): {2}^FS");

            linesTemplates.Add(OrderCreatedReportCreditTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FDTotal de Credito: {2}^FS");   
            linesTemplates.Add(OrderCreatedReportNetTotal, "^FO40,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD   Net Total: {2}^FS");
            linesTemplates.Add(OrderCreatedReportBillTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FDTotal en Cuentas: {2}^FS");
            linesTemplates.Add(OrderCreatedReportSalesTotal, "^FO15,{0}^ABN,18,10^FD^FS" +
                               "^FO500,{0}^ABN,18,10^FD Total de Ventas: {2}^FS");

            #endregion

            #region Payments Reports

            linesTemplates.Add(PaymentReportHeader, "^CF0,50^FO40,{0}^FDReporte De Pagos^FS^CF0,25^FO650,{3}^FDPag: {1}/{2}^FS");
            linesTemplates.Add(PaymentReportTableHeader, "^FO40,{0}^ABN,18,10^FDNombre^FS" +
                "^FO310,{0}^ABN,18,10^FDNumero^FS" +
                "^FO430,{0}^ABN,18,10^FDTotal^FS" +
                "^FO520,{0}^ABN,18,10^FDMonto^FS" +
                "^FO610,{0}^ABN,18,10^FDMetodo^FS" +
                "^FO700,{0}^ABN,18,10^FDNumero^FS");
            linesTemplates.Add(PaymentReportTableHeader1, "^FO40,{0}^ABN,18,10^FD^FS" +
                "^FO310,{0}^ABN,18,10^FDFactura^FS" +
                "^FO430,{0}^ABN,18,10^FDFactura^FS" +
                "^FO520,{0}^ABN,18,10^FD^FS" +
                "^FO610,{0}^ABN,18,10^FD^FS" +
                "^FO700,{0}^ABN,18,10^FDRef^FS");
            linesTemplates.Add(PaymentReportTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO310,{0}^ABN,18,10^FD{2}^FS" +
                "^FO430,{0}^ABN,18,10^FD{3}^FS" +
                "^FO520,{0}^ABN,18,10^FD{4}^FS" +
                "^FO610,{0}^ABN,18,10^FD{5}^FS" +
                "^FO700,{0}^ABN,18,10^FD{6}^FS");
            linesTemplates.Add(PaymentReportTotalCash, "^FO400,{0}^ABN,18,10^FDEfectivo: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCheck, "^FO415,{0}^ABN,18,10^FDCheque: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalCC, "^FO305,{0}^ABN,18,10^FDTarjeta de Credito: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalMoneyOrder, "^FO370,{0}^ABN,18,10^FDMoney Order: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTransfer, "^FO370,{0}^ABN,18,10^FDTransferencia: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentReportTotalTotal, "^FO425,{0}^ABN,18,10^FDTotal: ^FS" +
                "^FO520,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentSignatureText, "^FO40,{0}^ADN,18,10^FDPago Recibido Por^FS");

            #endregion

            #region Settlement

            linesTemplates.Add(InventorySettlementHeader, "^CF0,50^FO40,{0}^FDReporte De Liquidacion^FS^CF0,25^FO650,{3}^FDPag: {1}/{2}^FS");
            linesTemplates.Add(InventorySettlementProductHeader, "^FO40,{0}^ABN,18,10^FDProducto^FS");
            linesTemplates.Add(InventorySettlementTableHeader,
                "^FO40, {0}^ABN,18,10^FDUM^FS" +
                "^FO100,{0}^ABN,18,10^FDInv.I^FS" +
                "^FO150,{0}^ABN,18,10^FDCarga^FS" +
                "^FO210,{0}^ABN,18,10^FDAjus^FS" +
                "^FO270,{0}^ABN,18,10^FDTran^FS" +
                "^FO330,{0}^ABN,18,10^FDVent^FS" +
                "^FO390,{0}^ABN,18,10^FDRBE^FS" +
                "^FO450,{0}^ABN,18,10^FDRME^FS" +
                "^FO510,{0}^ABN,18,10^FDRech^FS" +
                "^FO570,{0}^ABN,18,10^FDD.C^FS" +
                "^FO630,{0}^ABN,18,10^FDDesc^FS" +
                "^FO690,{0}^ABN,18,10^FDInv.F^FS" +
                "^FO740,{0}^ABN,18,10^FDDif.I^FS");
            linesTemplates.Add(InventorySettlementTableHeader1,
                "^FO40, {0}^ABN,18,10^FD^FS" +
                "^FO100,{0}^ABN,18,10^FD^FS" +
                "^FO150,{0}^ABN,18,10^FD^FS" +
                "^FO210,{0}^ABN,18,10^FD^FS" +
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
            linesTemplates.Add(InventorySettlementLotLine, "^FO40,{0}^ADN,18,10^FDLote: {1}^FS");
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

            linesTemplates.Add(InventorySettlementAssetTracking, "^FO40,{0}^CF0,33^FB620,1,0,L^FDCAJAS: {1}^FS");

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

            #region Summary

            linesTemplates.Add(InventorySummaryHeader, "^CF0,50^FO40,{0}^FDResumen de Inventario^FS");
            linesTemplates.Add(InventorySummaryTableHeader,
                "^FO40,{0}^ABN,18,10^FDProducto^FS" +
                "^FO290,{0}^ABN,18,10^FD^FS" +
                "^FO330,{0}^ABN,18,10^FD^FS" +
                "^FO400,{0}^ABN,18,10^FD^FS" +
                "^FO490,{0}^ABN,18,10^FD^FS" +
                "^FO550,{0}^ABN,18,10^FD^FS" +
                "^FO600,{0}^ABN,18,10^FD^FS" +
                "^FO660,{0}^ABN,18,10^FD^FS"
                );

            linesTemplates.Add(InventorySummaryTableHeader1,
                "^FO40,{0}^ABN,18,10^FDLote^FS" +
                "^FO140,{0}^ABN,18,10^FDUdM^FS" +
                "^FO220,{0}^ABN,18,10^FDInv Ini^FS" +
                "^FO340,{0}^ABN,18,10^FDCarga^FS" +
                "^FO460,{0}^ABN,18,10^FDTransf^FS" +
                "^FO580,{0}^ABN,18,10^FDVentas^FS" +
                "^FO700,{0}^ABN,18,10^FDInv. Act^FS"
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
                "^FO40,{0}^ABN,18,10^FDTotales:^FS" +
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

            linesTemplates.Add(ConsignmentInvoiceHeader, "^FO40,{0}^ADN,36,20^FDFactura: {1}^FS");
            linesTemplates.Add(ConsignmentSalesOrderHeader, "^FO40,{0}^ADN,36,20^FDOrden de Compra^FS");
            linesTemplates.Add(ConsignmentInvoiceTableHeader, "^FO40,{0}^ADN,18,10^FDProducto^FS" +
                "^FO390,{0}^ADN,18,10^FDCant^FS" +
                "^FO450,{0}^ADN,18,10^FDCont^FS" +
                "^FO515,{0}^ADN,18,10^FDVent^FS" +
                "^FO580,{0}^ADN,18,10^FDPrecio^FS" +
                "^FO670,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO390,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO515,{0}^ADN,18,10^FD{4}^FS" +
                "^FO580,{0}^ADN,18,10^FD{5}^FS" +
                "^FO670,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableLineLot, "^FO40,{0}^ADN,18,10^FDLote: ^FS" +
                "^FO100,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(ConsignmentInvoiceTableTotal, "^FO420,{0}^ADN,18,10^FDTotal^FS" +
                "^FO515,{0}^ADN,18,10^FD{1}^FS" +
                "^FO670,{0}^ADN,18,10^FD{2}^FS");

            #endregion


            #region Route Return

            linesTemplates.Add(RouteReturnsTitle, "^FO40,{0}^ADN,36,20^FDReporte de Devoluciones^FS");
            linesTemplates.Add(RouteReturnsNotFinalLabel, "^FO40,{0}^ADN,28,14^FDNO ES UN DOCUMENTO FINAL^FS");
            linesTemplates.Add(RouteReturnsTableHeader, "^FO40,{0}^ADN,18,10^FDProducto^FS" +
                "^FO400,{0}^ADN,18,10^FDRECH^FS" +
                "^FO480,{0}^ADN,18,10^FDRME^FS" +
                "^FO560,{0}^ADN,18,10^FDRBE^FS" +
                "^FO640,{0}^ADN,18,10^FDDC^FS" +
                "^FO720,{0}^ADN,18,10^FDDesc.^FS");
            linesTemplates.Add(RouteReturnsTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO400,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO560,{0}^ADN,18,10^FD{4}^FS" +
                "^FO640,{0}^ADN,18,10^FD{5}^FS" +
                "^FO720,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(RouteReturnsTotals, "^FO40,{0}^ADN,18,10^FDTotales:^FS" +
                "^FO400,{0}^ADN,18,10^FD{2}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS" +
                "^FO560,{0}^ADN,18,10^FD{4}^FS" +
                "^FO640,{0}^ADN,18,10^FD{5}^FS" +
                "^FO720,{0}^ADN,18,10^FD{6}^FS");

            #endregion

            #region Payment

            linesTemplates.Add(PaymentTitle, "^FO40,{0}^ADN,36,20^FDRecibo de Pago^FS^FO500,{0}^ADN,18,10^FDImpreso: {1}^FS");
            linesTemplates.Add(PaymentHeaderTo, "^FO40,{0}^ADN,36,20^FDCliente:^FS");
            linesTemplates.Add(PaymentHeaderClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(PaymentHeaderClientAddr, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentInvoiceNumber, "^FO40,{0}^ADN,18,10^FD{1} #: {2}^FS");
            linesTemplates.Add(PaymentInvoiceTotal, "^FO40,{0}^ADN,18,10^FD{1} Total: {2}^FS");
            linesTemplates.Add(PaymentPaidInFull, "^FO40,{0}^ADN,18,10^FDPagado Completo: {1}^FS");
            linesTemplates.Add(PaymentComponents, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(PaymentTotalPaid, "^FO40,{0}^ADN,36,20^FDTotal Pagado: {1}^FS");
            linesTemplates.Add(PaymentPending,   "^FO40,{0}^ADN,36,20^FD   Pendiente: {1}^FS");

            #endregion

            #region Open Invoice

            linesTemplates.Add(InvoiceTitle, "^CF0,40^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(InvoiceCopy, "^FO40,{0}^ADN,36,20^FDCOPIA^FS");
            linesTemplates.Add(InvoiceDueOn, "^FO40,{0}^ADN,18,10^FDVence En:    {1}^FS");
            linesTemplates.Add(InvoiceDueOnOverdue, "^FO40,{0}^ADN,18,10^FDVence en:    {1} VENCIDO^FS");
            linesTemplates.Add(InvoiceClientName, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(InvoiceCustomerNumber, "^FO40,{0}^ADN,18,10^FDCliente: {1}^FS");
            linesTemplates.Add(InvoiceClientAddr, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(InvoiceClientBalance, "^FO40,{0}^ADN,18,10^FDBalance de la Cuenta: {1}^FS");
            linesTemplates.Add(InvoiceComment, "^FO40,{0}^ADN,18,10^FDC: {1}^FS");
            linesTemplates.Add(InvoiceTableHeader, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" +
                "^FO450,{0}^ADN,18,10^FDCANT^FS" +
                "^FO580,{0}^ADN,18,10^FDPRECIO^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS");
            linesTemplates.Add(InvoiceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO580,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoiceTotal, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO320,{0}^ADN,18,10^FD{2}^FS" +
                "^FO450,{0}^ADN,18,10^FD{3}^FS" +
                "^FO680,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(InvoicePaidInFull,     "^FO40,{0}^ADN,36,20^FDPAGADO COMPLETO^FS");
            linesTemplates.Add(InvoicePaidInFullCredit, "^FO40,{0}^ADN,36,20^FDCOBRADO COMPLETO^FS");
            linesTemplates.Add(InvoiceCredit,         "^FO40,{0}^ADN,36,20^FDCREDITO^FS");
            linesTemplates.Add(InvoicePartialPayment, "^FO40,{0}^ADN,36,20^FD PAGADO PARCIAL: {1}^FS");
            linesTemplates.Add(InvoiceOpen,           "^FO40,{0}^ADN,36,20^FD        ABIERTO: {1}^FS");
            linesTemplates.Add(InvoiceQtyItems,       "^FO40,{0}^ADN,36,20^FDCANT. ARTICULOS: {1}^FS");
            linesTemplates.Add(InvoiceQtyUnits,       "^FO40,{0}^ADN,36,20^FD CANT. UNIDADES: {1}^FS");

            #endregion

            #region Transfer

            linesTemplates.Add(TransferOnHeader, "^FO40,{0}^ADN,36,20^FDReporte de Carga^FS");
            linesTemplates.Add(TransferOffHeader, "^FO40,{0}^ADN,36,20^FDReporte de Descarga^FS");
            linesTemplates.Add(TransferNotFinal, "^FO40,{0}^ADN,28,14^FDNO ES UNA TRANSFERENCIA FINAL^FS");
            linesTemplates.Add(TransferTableHeader, "^FO40,{0}^ADN,18,10^FDProducto^FS^FO570,{0}^ADN,18,10^FDUdM^FS^FO650,{0}^ADN,18,10^FDTransferido^FS");
            linesTemplates.Add(TransferTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS^FO570,{0}^ADN,18,10^FD{2}^FS^FO650,{0}^ADN,18,10^FD{3}^FS");
            linesTemplates.Add(TransferTableLinePrice, "^FO40,{0}^ADN,18,10^FDPrecio de Venta: {1}^FS");
            linesTemplates.Add(TransferQtyItems, "^FO40,{0}^ADN,36,20^FD CANT ARTICULOS: {1}^FS");
            linesTemplates.Add(TransferAmount,   "^FO40,{0}^ADN,36,20^FD   VALOR TRANS.: {1}^FS");
            linesTemplates.Add(TransferComment, "^FO40,{0}^ADN,18,10^FDComentario: {1}^FS");

            #endregion

            #region Client Statement

            linesTemplates.Add(ClientStatementTableTitle, "^FO40,{0}^ADN,28,14^FDSaldo Abierto del Cliente^FS");
            linesTemplates.Add(ClientStatementTableHeader, "^FO40,{0}^ADN,18,10^FDTipo^FS" +
                "^FO150,{0}^ADN,18,10^FDFecha^FS" +
                "^FO285,{0}^ADN,18,10^FDNumero^FS" +
                "^FO460,{0}^ADN,18,10^FDFecha Ven^FS" +
                "^FO585,{0}^ADN,18,10^FDMonto^FS" +
                "^FO690,{0}^ADN,18,10^FDAbierto^FS");
            linesTemplates.Add(ClientStatementTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO150,{0}^ADN,18,10^FD{2}^FS" +
                "^FO285,{0}^ADN,18,10^FD{3}^FS" +
                "^FO460,{0}^ADN,18,10^FD{4}^FS" +
                "^FO585,{0}^ADN,18,10^FD{5}^FS" +
                "^FO690,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(ClientStatementCurrent, "^FO100,{0}^ADN,28,14^FD                Actual: {1}^FS");
            linesTemplates.Add(ClientStatement1_30PastDue, "^FO100,{0}^ADN,28,14^FD     1-30 Dias Vencido: {1}^FS");
            linesTemplates.Add(ClientStatement31_60PastDue, "^FO100,{0}^ADN,28,14^FD    31-60 Dias Vencido: {1}^FS");
            linesTemplates.Add(ClientStatement61_90PastDue, "^FO100,{0}^ADN,28,14^FD    61-90 Dias Vencido: {1}^FS");
            linesTemplates.Add(ClientStatementOver90PastDue, "^FO100,{0}^ADN,28,14^FDMas de 90 Dias Vencido: {1}^FS");
            linesTemplates.Add(ClientStatementAmountDue, "^FO100,{0}^ADN,28,14^FD         Monto Vencido: {1}^FS");

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

            linesTemplates.Add(NewRefusalReportTableHeader1, "^CF0,35^^FO40,{0}^FDRechazado por la Tienda^FS");
            linesTemplates.Add(NewRefusalReportTableHeader, "^CF0,25^^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(NewRefusalReportProductTableHeader, "^CF0,25^^FO40,{0}^FDProducto^FS" + "^FO420,{0}^FDCant^FS" + "^FO520,{0}^FDRazón^FS");
            linesTemplates.Add(NewRefusalReportProductTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO420,{0}^ADN,18,10^FD{2}^FS" + "^FO520,{0}^ADN,18,10^FD{3}^FS");

            #endregion

            #region Payment Deposit
            linesTemplates.Add(ChecksTitle, "^FO40,{0}^AON,30,15^FDLista de Cheques^FS");
            linesTemplates.Add(BatchDate, "^FO40,{0}^ADN,18,10^FDFecha de Publicacion: {1}^FS");
            linesTemplates.Add(BatchPrintedDate, "^FO40,{0}^ADN,18,10^FDFecha de Impresion: {1}^FS");
            linesTemplates.Add(BatchSalesman, "^FO40,{0}^ADN,18,10^FDVendedor: {1}^FS");
            linesTemplates.Add(BatchBank, "^FO40,{0}^ADN,18,10^FDBanco: {1}^FS");
            linesTemplates.Add(CheckTableHeader, "^FO40,{0}^ADN,18,10^FDIDENTICACION DEL CHECQUE^FS" +
                   "^FO450,{0}^ADN,18,10^FDCANTIDAD^FS");
            linesTemplates.Add(CheckTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
               "^FO450,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(CheckTableTotal, "^FO40,{0}^ADN,18,10^FD# DE CHEQUES: {1}^FS" +
            "^FO290,{0}^ADN,18,10^FDTOTAL CHEQUES: {2}^FS");

            linesTemplates.Add(CashTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL EFECTIVO: {1}^FS");
            linesTemplates.Add(CreditCardTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL TARJETA DE CREDITO: {1}^FS");
            linesTemplates.Add(MoneyOrderTotalLine, "^FO40,{0}^ADN,18,10^FDTOTAL GIRO POSTAL: {1}^FS");

            linesTemplates.Add(BatchTotal, "^FO40,{0}^AON,30,15^FDTOTAL DEPOSITO: {1}^FS");
            linesTemplates.Add(BatchComments, "^FO40,{0}^ADN,18,10^FDCommentarios: {1}^FS");

            #endregion

            #region Proof Delivery
            linesTemplates.Add(DeliveryHeader, "F040,126^ADN,36,20^FD{0}^FS");
            linesTemplates.Add(DeliveryInvoiceNumber, "^FO40,169^ADN,18,10^FD{0}^FS");
            linesTemplates.Add(OrderDetailsHeaderDelivery, "^FO40,{0}^ADN,18,10^FDPRODUCTO^FS" + "^FO680,{0}^ADN,18,10^FDCANT^FS");
            linesTemplates.Add(OrderDetailsTotalsDelivery, "^FO40,{0}^ADN,18,10^FD{1}^FS" + "^FO689,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(TotalQtysProofDelivery, "^FO610,{0}^ADN,18,10^FDTOTAL: {1}^FS");

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

            if (uomMap.Count > 0)
            {
                if (!uomMap.ContainsKey("Unidades"))
                    uomMap.Add("Unidades", totalQtyNoUoM);
            }
            else
            {
                uomMap.Add("Totales", totalQtyNoUoM);

                if (uomMap.Keys.Count == 1 && totalUnits != totalQtyNoUoM && sectionName == "SECCION DE VENTAS" && !uomMap.ContainsKey("Unidades"))
                    uomMap.Add("Unidades", totalUnits);
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

        public override string AdjustPadding(string input, int safetyGap = 3)
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

        #region Payments

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
                        //case InvoicePaymentMethod.Transfer:
                        //    refName = "Transferencia: {0}";
                        //    break;
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
            else if (invoice.InvoiceType == 2)
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

        #region Proof Delivery
        protected override string GetOrderDocumentNameDelivery(ref bool printExtraDocName, Order order, Client client)
        {
            string docName = "Prueba de Envio";

            return docName;
        }
        #endregion
    }
}