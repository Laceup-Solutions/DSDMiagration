using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class GeminiPrinter : ZebraFourInchesPrinter1
    {
        protected const string GeminiTotalQty = "GeminiTotalQty";
        protected const string GeminiSubtotal = "GeminiSubtotal";
        protected const string GeminiTotalCost = "GeminiTotalCost";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^CFA,25^FO40,{0}^FDPRODUCT^FS" +
                "^FO400,{0}^FDQTY^FS" +
                "^FO475,{0}^FDPRICE^FS" +
                "^FO620,{0}^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = "^CFA,25^FO40,{0}^FD{1}^FS" +
                "^FO400,{0}^FD{2}^FS" +
                "^FO475,{0}^FD{4}^FS" +
                "^FO620,{0}^FD{3}^FS";

            linesTemplates[OrderDetailsHeaderSectionName] = "^CF0,25^FO400,{0}^FD{1}^FS";

            linesTemplates[OrderDetailsTotals] = "^CFA,25^FO40,{0}^FD{1}^FS" +
                "^FO270,{0}^FD{2}^FS" +
                "^FO400,{0}^FD{3}^FS" +
                "^FO620,{0}^FD{4}^FS";

            linesTemplates[OrderDetailsLines2] = "^CFA,25^FO40,{0}^FD{1}^FS";

            linesTemplates.Add(GeminiTotalQty, "^CFA,25^FO40,{0}^FDTotal Qty: {1}^FS");
            linesTemplates.Add(GeminiSubtotal, "^CFA,25^FO40,{0}^FDSub-Total: ^FS^FO620,{0}^FD{1}^FS");
            linesTemplates.Add(GeminiTotalCost, "^CF0,35^FO40,{0}^FDInvoice Total: ^FS^FO620,{0}^FD{1}^FS");
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        public override string ToString(double d)
        {
            if (d >= 0)
                return d.ToCustomString();
            return d.ToString();
        }


        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {

            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, "^FO25,{0}^GB770,0,2^FS", startY));
            startY += font18Separation;

            var totalQty = sales.Sum(x => x.Value.Qty) + credit.Sum(x => x.Value.Qty) + returns.Sum(x => x.Value.Qty);
            var totalInvoice = order.OrderTotalCost();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[GeminiTotalQty], startY, totalQty.ToString()));
            startY += 30;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[GeminiSubtotal], startY, ToString(totalInvoice)));
            startY += 30;

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[GeminiTotalCost], startY, ToString(totalInvoice)));
            startY += font36Separation;

            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
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

    }
}