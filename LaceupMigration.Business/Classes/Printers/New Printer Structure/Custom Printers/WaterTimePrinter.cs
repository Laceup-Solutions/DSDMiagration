using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class WaterTimePrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderTotalsNetAmount] =            "^FO40,{0}^ADN,36,20^FD        SUBTOTAL: {1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance] = "^FO40,{0}^ADN,36,20^FDPREVIOUS BALANCE: {1}^FS";
            linesTemplates[OrderTotalsTotalDue] =             "^FO40,{0}^ADN,36,20^FD       TOTAL DUE: {1}^FS";
            linesTemplates[OrderTotalsTotalPayment] =         "^FO40,{0}^ADN,36,20^FD   TOTAL PAYMENT: {1}^FS";
        }

        protected override List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            return lines;
        }
        
        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
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

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && order.UniqueId != null && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
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
            lines.AddRange(GetTotalsRowsInOneDoc(ref startY, order.Client, salesLines, creditLines, returnsLines, payment, order, fromBatch));

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

        protected IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order, bool fromBatch)
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
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
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
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalSales += qty;

                    var x = od.Price * factor * qty;

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    salesBalance += x;

                    if (sales[key].Product.Taxable)
                        taxableAmount += x;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalCredit += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    creditBalance += x * -1;

                    if (credit[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalReturn += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    returnBalance += x * -1;

                    if (returns[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (!Config.HideTotalOrder && t == null)
            {
                s1 = ToString((salesBalance + creditBalance + returnBalance));
                s1 = new string(' ', 13 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                //if (order.Client.UseDiscount || order.Client.UseDiscountPerLine)
                //{
                //    s1 = ToString(Math.Abs(discount));
                //    s1 = new string(' ', 14 - s1.Length) + s1;
                //    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                //    startY += font36Separation;
                //}

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 17)
                    s = new string(' ', 17 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                s1 = ToString(tax);
                s1 = new string(' ', 13 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                startY += font36Separation;

                //var currentBalance = order.Client.CurrentBalance();

                double balanceFromOrder = 0;
                if(!order.Finished)
                    balanceFromOrder = order.OrderTotalCost();
                
                var currentBalance = order.Client.OpenBalance + CurrentBalance(order.Client.ClientId) + balanceFromOrder;

                s1 = ToString(currentBalance - (salesBalance + creditBalance + returnBalance - discount + tax) + paid);
                s1 = s1 = new string(' ', 13 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order)
                {
                    s1 = ToString(paid);
                    s1 = s1 = new string(' ', 13 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font36Separation;
                }

                s1 = ToString(currentBalance);
                s1 = s1 = new string(' ', 13 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

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
                startY += font18Separation;
                var clines = GetOrderSplitComment(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string format = linesTemplates[OrderComment];
                    if (i > 0)
                        format = linesTemplates[OrderComment2];

                    list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                    startY += font18Separation;
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
        
        public static double CurrentBalance(int ClientId)
        {
            double currentBalance = 0;

            var invoicesFinalized = Order.Orders.Where(x => x.Client.ClientId == ClientId && x.Finished).Sum(x => x.OrderTotalCost());
            var currentPayments = InvoicePayment.List.Where(x => x.Client.ClientId == ClientId).Sum(x => x.TotalPaid);
            currentBalance = invoicesFinalized - currentPayments;
            return currentBalance;
        }
    }
}