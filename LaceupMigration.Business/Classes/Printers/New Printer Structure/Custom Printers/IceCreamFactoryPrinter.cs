





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class IceCreamFactoryPrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(IceCreamFactoryComment, "^FT40,{0}^A0D,28,28^FB730,1,0,C^FH\\^FD{1}^FS");
        }

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
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

                s1 = ToString(Math.Round((salesBalance + creditBalance + returnBalance), Config.Round));
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();

                if (order.Client.UseDiscount || order.Client.UseDiscountPerLine)
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

                double tax = order.CalculateTax();
                var s = Config.PrintTaxLabel;
                if (Config.PrintTaxLabel.Length < 16)
                    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                s1 = ToString(tax);
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                startY += font36Separation;


                var s4 = salesBalance + creditBalance + returnBalance - discount + tax;
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font36Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s1 = ToString(Math.Round(paid, Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
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
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                    startY += font36Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
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

            string commentText = "Past Due Invoices accrue 2% interest Charges";
            if (!string.IsNullOrEmpty(Config.ExtraTextForPrinter))
            {
                startY += font18Separation;
                startY += font18Separation;

                foreach (var part in SplitProductName(Config.ExtraTextForPrinter, 45, 45))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[IceCreamFactoryComment], startY, part));
                    startY += font18Separation;
                }

                startY += font18Separation;
            }
            else
            {
                startY += font18Separation;
                startY += font18Separation;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[IceCreamFactoryComment], startY, commentText));
                startY += font36Separation;
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
            }

            return list;
        }
    }
}