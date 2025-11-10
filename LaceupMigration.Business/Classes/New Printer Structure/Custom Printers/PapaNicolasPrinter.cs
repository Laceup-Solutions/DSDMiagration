using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class PapaNicolasPrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderTotalsCurrentBalance] = "^FO40,{0}^ADN,36,20^FD TICKET BALANCE: {1}^FS";
        }

        protected override string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            string docName = "Driver Ticket";
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

            return docName;
        }


        protected override List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            string docName = "NOT A FINAL TICKET";
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
                if (Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderText], startY, "FINAL DELIVERY TICKET"));
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

    }
}