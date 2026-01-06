





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class PrasekPrinter : ZebraFourInchesPrinter1
    {
        #region Orders Created Report
        protected override IEnumerable<string> GetOrderCreatedReportHeader(ref int startY, int index, int count)
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

        protected override IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
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
            double creditTotal1 = 0;
            double salesTotal1 = 0;
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
                                foreach(var det in p.Details)
                                {
                                    if (det.IsCredit)
                                        creditTotal1 += det.TotalLine;
                                    else
                                        salesTotal1 += det.TotalLine;
                                }

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

           
            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportSalesTotal, startY, "", ToString(salesTotal1)));
            startY += font18Separation;
            startY += 3;
            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditTotal, startY, "", ToString(Math.Abs(creditTotal1))));
            startY += font18Separation;
            startY += 3;

            netTotal = Math.Round(salesTotal - Math.Abs(creditTotal), Config.Round);

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportNetTotal, startY, "", ToString(netTotal)));
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


            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportFullTotal, startY, Config.SalesReportTotalCreditsSubstracted ? ToString(netTotal) : ToString(salesTotal), totalTime));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }

        protected override string GetCreatedOrderStatus(Order o)
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

        protected override string GetCreatedOrderType(Order o, double paid, double orderCost)
        {
            string type = "";
            if (paid == 0)
                type = "Charge";
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

        protected override string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        protected override string GetOrderCreatedReportTotalsFixed(string format, int pos, string v1, string v2)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2);
        }


        #endregion

    }
}
