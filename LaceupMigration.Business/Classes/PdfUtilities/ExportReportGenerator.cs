








using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ExportReportGenerator
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
        protected const string OrderDetailsTotals = "OrderDetailsTotals";
        protected const string OrderDetailsTotals1 = "OrderDetailsTotals1";
        protected const string OrderTotalsNetQty = "OrderTotalsNetQty";
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

        protected const string PickTicketCompanyHeader = "PickTicketCompanyHeader";
        protected const string PickTicketRouteInfo = "PickTicketRouteInfo";
        protected const string PickTicketDeliveryDate = "PickTicketDeliveryDate";
        protected const string PickTicketDriver = "PickTicketDriver";

        protected const string PickTicketProductHeader = "PickTicketProductHeader";
        protected const string PickTicketProductLine = "PickTicketProductLine";
        protected const string PickTicketProductTotal = "PickTicketProductTotal";

        #region Footer

        protected const string FooterSignatureLine = "FooterSignatureLine";
        protected const string FooterSignatureText = "FooterSignatureText";
        protected const string FooterSignatureNameText = "FooterSignatureNameText";
        protected const string FooterSpaceSignatureText = "FooterSpaceSignatureText";
        protected const string FooterBottomText = "FooterBottomText";
        protected const string FooterDriverSignatureText = "FooterDriverSignatureText";

        #endregion
        #region Fonts

        protected const int orderDetailSeparation = 3;
        protected int font18Separation = 25;
        protected const int font20Separation = 20;
        protected const int font36Separation = 43;

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

        #endregion Labels

        protected int WidthForNormalFont
        {
            get
            {
                int i = 62;
                return i;
            }
        }
        public string CreateExportableReport(int index, int count)
        {
            List<string> lines = new List<string>();

            int startY = 80;
            linesTemplates[StartLabel] = "";
            linesTemplates[EndLabel] = "";
            lines.AddRange(GetOrderCreatedReportHeader(ref startY, index, count));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetOrderCreatedReportTable(ref startY));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return CreateReport(lines);
        }

        private string CreateReport(List<string> lines)
        {
            IPdfProvider pdfGenerator = GetPdfProvider();
            return pdfGenerator.GetReportPdf();
            
        }
        static IPdfProvider GetPdfProvider()
        {
            try
            {
                IPdfProvider provider;

                //if (Config.UseOldEmailFormat)
                //{
                //    provider = new FromHtmlPdf();
                //    return provider;
                //}

                // instantiate selected Pdf Provider
                Type t = Type.GetType(Config.PdfProvider);
                if (t == null)
                {
                    Logger.CreateLog("could not instantiate pdf provider" + Config.PdfProvider + " using DefaultPdfProvider instead");
                    provider = new DefaultPdfProvider();
                }
                provider = Activator.CreateInstance(t) as IPdfProvider;

                return provider;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);

                return new DefaultPdfProvider();
            }
        }
        protected virtual IEnumerable<string> GetOrderCreatedReportHeader(ref int startY, int index, int count)
        {
            
            linesTemplates[StandarPrintedDate] = "Fecha Impresa: {0}";
            linesTemplates[StandarPrintRouteNumber] = "Número de Ruta: {0}";
            linesTemplates[StandarPrintDriverName] = "Nombre del Conductor: {0}";
            linesTemplates[OrderCreatedReporWorkDay] = "Jornada Laboral: Inicio a las {0}, Fin a las {1}, Duración {2} horas {3} minutos";
            linesTemplates[OrderCreatedReporBreaks] = "Descansos Totales: {0} horas {1} minutos";
            linesTemplates[CompanyName] = "{1}";
            linesTemplates[CompanyAddress] = "{1}";
            linesTemplates[CompanyPhone] = "Company Number: {1}";
            linesTemplates[CompanyFax] = "Fax de la Empresa: {1}";
            linesTemplates[CompanyEmail] = "Company Email: {1}";
            linesTemplates[CompanyLicenses1] = "Licencia de la Empresa (Primera): {1}, {0}";
            linesTemplates[CompanyLicenses2] = "Licencia de la Empresa (Siguiente): {1}, {0}";
            linesTemplates[OrderCreatedReportTableHeader] = "Encabezado del Reporte de Pedidos Creados, ";
            linesTemplates[OrderDetailsLineSeparator] = "Separador de Líneas de Detalles del Pedido,{0}, {1}";
            linesTemplates[OrderCreatedReportTableLine] = "{0},  {1}, {2}, {3}, {4}, {5}, {6}";
            linesTemplates[OrderCreatedReportTableLine1] = "  {0}, {1}, {2}, {3}";
            linesTemplates[OrderCreatedReportTableTerms] = " {0}, {1}";
            linesTemplates[OrderCreatedReportTableLineComment] = "  {0},{1}";
            linesTemplates[OrderCreatedReportTableLineComment1] = " {0}, {1}";
            linesTemplates[OrderCreatedReportSubtotal] = " {0},  {1}";
            linesTemplates[OrderCreatedReportTax] = " {0},  {1}";
            linesTemplates[OrderCreatedReportTotals] = "{0}, {1}";
            linesTemplates[OrderCreatedReportPaidCust] = "  {0},  {1}, {2}";
            linesTemplates[OrderCreatedReportChargeCust] = ",, ";
            linesTemplates[OrderCreatedReportCreditCust] = "  {0}, Créditos: {1}";
            linesTemplates[OrderCreatedReportExpectedCash] = "  {0}, Efectivo: {1}, Reenviados: {2}";
            linesTemplates[OrderCreatedReportFullTotal] = "  {0}, Total: {1}, Tiempo Total: {2}";
            linesTemplates[OrderCreatedReportCreditTotal] = "  {0}, Créditos: {1}";
            linesTemplates[OrderCreatedReportBillTotal] = " {0}, Facturas: {1}";
            linesTemplates[OrderCreatedReportSalesTotal] = "Total de Ventas en el Reporte de Pedidos Creados, {0}, Ventas: {1}";
            linesTemplates[FooterSignatureLine] = "Línea de Firma del Pie de Página, {0}";
            linesTemplates[FooterSignatureText] = "Texto de Firma del Pie de Página, {0}";
            List<string> lines = new List<string>();

            // Encabezado del reporte con el título y la numeración
      

            // Fecha impresa, número de ruta y nombre del conductor
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            // Información de la empresa
            if (!Config.HideCompanyInfoPrint)
            {
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            // Información sobre la jornada laboral si es aplicable
            if (Config.UseClockInOut)
            {
                DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                var wholeday = lastClockOut.Subtract(startOfDay);
                var breaks = SalesmanSession.GetTotalBreaks();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporWorkDay], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), wholeday.Hours, wholeday.Minutes));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporBreaks], startY, breaks.Hours, breaks.Minutes));
                startY += font18Separation;
            }

            // Agrega un espacio adicional como separador
            lines.Add("\n");

            // Encabezados de las columnas de la tabla de detalle
            string headerLine = String.Format("{0,-20} {1,-10} {2,5} {3,10} {4,15} {5,10}",
                "NAME", "ST", "QTY", "TICKET #", "TOTAL", "CS TP");
            lines.Add(headerLine);
            lines.Add(new string('-', headerLine.Length)); // Línea divisora

            // Actualiza la posición Y después de agregar la cabecera
            startY += (lines.Count * font18Separation); // Ajusta esta línea según cómo manejes el espaciado en tu documento.

            return lines;
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
        public void AddExtraSpace(ref int startY, List<string> lines, int font, int spaces)
        {
            for (int i = 0; i < spaces; i++)
                lines.Add(string.Empty);
        }
        protected IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 25, 25); //35, 35
        }
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
        protected virtual IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            // Añade el encabezado de la tabla
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableHeader], startY));
            startY += font18Separation;

            // Añade la línea separadora
            var separator = new string('-', WidthForNormalFont);
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, separator));
            startY += font18Separation;

            // Inicializa las variables para los totales y contadores
            int voided = 0, reshipped = 0, delivered = 0, dsd = 0;
            DateTime start = DateTime.MaxValue, end = DateTime.MinValue;
            double cashTotalTerm = 0, chargeTotalTerm = 0, subtotal = 0, totalTax = 0;
            double paidTotal = 0, chargeTotal = 0, creditTotal = 0, salesTotal = 0, billTotal = 0;
            double netTotal = 0;

            // Obtiene los pagos
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

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, separator));
            startY += font36Separation;

            // Añade los totales al final de la tabla
            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditTotal, startY, "", ToString(creditTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportBillTotal, startY, "", ToString(billTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportSalesTotal, startY, "", ToString(salesTotal)));
            startY += font18Separation;

            // Si se incluye el impuesto en el reporte, añade las líneas de subtotal, impuesto y totales
            if (Config.SalesRegReportWithTax)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportSubtotal], startY, ToString(subtotal)));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTax], startY, ToString(totalTax)));
                startY += font18Separation;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTotals], startY, ToString(paidTotal + chargeTotal)));
                startY += font18Separation;
            }

            // Añade un espacio extra si es necesario
            AddExtraSpace(ref startY, lines, font18Separation, 1);

            // Añade los totales finales
            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportExpectedCash, startY, ToString(cashTotalTerm), reshipped.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportPaidCust, startY, ToString(paidTotal), voided.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportChargeCust, startY, ToString(chargeTotal), delivered.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditCust, startY, "", dsd.ToString()));
            startY += font18Separation;

            // Calcula el tiempo total si es aplicable
            if (start == DateTime.MaxValue)
            {
                start = end;
            }
            TimeSpan ts = end.Subtract(start);
            string totalTime = ts.Minutes > 0 ? String.Format("{0}h {1}m", ts.Hours, ts.Minutes) : String.Format("{0}h", ts.Hours);

            // Calcula el total neto
            netTotal = Math.Round(salesTotal - Math.Abs(creditTotal), Config.Round);

            // Añade el total completo
            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportFullTotal, startY, Config.SalesReportTotalCreditsSubstracted ? ToString(netTotal) : ToString(salesTotal), totalTime));
            startY += font36Separation;

            // Añade espacios al final si es necesario
            AddExtraSpace(ref startY, lines, font36Separation, 3);

            // Añade las líneas de firma al final
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }
        protected IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 35, 35);
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

        protected List<PaymentSplit> GetPaymentsForOrderCreatedReport()
        {
            List<PaymentSplit> result = new List<PaymentSplit>();

            foreach (var payment in InvoicePayment.List)
                result.AddRange(PaymentSplit.SplitPayment(payment));

            return result;
        }
        public virtual string ToString(double d)
        {
            return d.ToCustomString();
        }

    }
}