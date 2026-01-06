






using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class GevaCoffeePrinterFourInches : ZebraFourInchesPrinter1
    {
        protected override string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            if (order.IsWorkOrder)
                return "Work Order";
            
            if(order.IsExchange)
                return "Exchange";

            string docName = "Delivery No.";
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
    }

    public class GevaCoffeePrinterThreeInches : ZebraThreeInchesPrinter1
    {
        protected override string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            if (order.IsWorkOrder)
                return "Work Order";

            string docName = "Delivery No.";
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
    }
}