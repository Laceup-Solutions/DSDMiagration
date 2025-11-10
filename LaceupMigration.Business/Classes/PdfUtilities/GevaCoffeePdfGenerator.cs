





using iText.Layout;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class GevaCoffeePdfGenerator : DefaultPdfProvider
    {
        protected override void AddOrderInfo(Document doc, Order order)
        {
            string docName = string.Empty;
            string docNum = string.Empty;

            if (order.IsExchange)
            {                
                docName = "Exchange";
                docNum = "Exchange" + "#: "+ order.PrintedOrderId;
            }
            else
            if (order.OrderType == OrderType.Credit)
            {
                docName = "CREDIT";
                docNum = "Credit #" + order.PrintedOrderId;
            }
            else if (order.OrderType == OrderType.Return)
            {
                docName = "RETURN";
                docNum = "Return #" + order.PrintedOrderId;
            }
            else
            {
                if (order.IsWorkOrder)
                {
                    docName = "Work Order";
                }
                else
                {
                    if (order.AsPresale)
                    {
                        docName = "SALES ORDER";

                        if (Config.UseQuote && order.IsQuote)
                            docName = Config.GeneratePresaleNumber ? "Quote #" : "Quote";

                        if (Config.GeneratePresaleNumber)
                            docNum += order.PrintedOrderId;
                    }
                    else
                    {
                        docName = "Delivery";
                        docNum = "Delivery No. " + order.PrintedOrderId;
                    }
                }
            }

            AddTextLine(doc, docName, GetBigFont(), iText.Layout.Properties.TextAlignment.CENTER);

            if (((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0))
            {
                var site = SiteEx.Sites.FirstOrDefault(x => x.Id == Config.SalesmanSelectedSite);
                if (site != null)
                {
                    AddTextLine(doc, "Site: " + site.Name, GetBigFont(), iText.Layout.Properties.TextAlignment.CENTER);
                }
            }

            if (!string.IsNullOrEmpty(docNum))
                AddTextLine(doc, docNum, GetNormalFont(), iText.Layout.Properties.TextAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }
    }
}