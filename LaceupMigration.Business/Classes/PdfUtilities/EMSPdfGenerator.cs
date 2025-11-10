using System.Linq;
using System;
using System.Globalization;
using System.IO;



using System.Collections.Generic;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.Kernel.Font;





using iText.Kernel.Geom;

namespace LaceupMigration
{
    public class EMSPdfGenerator : DefaultPdfProvider
    {
        #region Order


        protected override void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 10, 15, 30, 15, 15, 15, 15 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "UPC");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "UoM");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Rate");
            AddCellToHeader(tableLayout, "Extended");

            float qtyBoxes = 0;

            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
                {
                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                    AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    AddCellToBody(tableLayout, name);

                    AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : "");

                    AddCellToBody(tableLayout, detail.Qty.ToString());

                    bool cameFromOffer = false;
                    var price = Product.CalculatePriceForProduct(detail.Product, order.Client, detail.IsCredit, detail.Damaged, detail.UnitOfMeasure, false, out cameFromOffer, true, order);

                    //var price = Product.GetPriceForProduct(detail.Product, order.Client, false, false);

                    var dc = (price - detail.Price);
                    if (dc < 0 || detail.IsCredit)
                        dc = 0;

                    string diff = "";
                    if (detail.IsCredit)
                        diff += "-";

                    if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                    else
                        AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);

                    if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                    else
                        AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

                    if (detail.UnitOfMeasure != null)
                        qtyBoxes += detail.Qty * detail.UnitOfMeasure.Conversion;
                    else
                        qtyBoxes += detail.Qty;
                }
            }

            doc.Add(tableLayout);
        }


        protected override void AddFooterTable(Document doc, Order order)
        {
            float[] headers = { 30, 30, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            var text = new Paragraph("Customer Signature");
            text.AddStyle(GetSmall8Font());
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            signature.Add(text);

            tableLayout.AddCell(signature);

            /*AddCellToHeader(tableLayout, "Total Units");
            AddCellToBody(tableLayout, order.Details.Sum(x => x.UnitOfMeasure != null ? x.Qty * x.UnitOfMeasure.Conversion : x.Qty).ToString());*/

            //double discount = 0;
            //foreach (var d in order.Details)
            //{
            //    if (d.Product.IsDiscountItem)
            //        continue;

            //    if (d.Discount > 0 || d.IsCredit)
            //        continue;

            //    bool cameFromOffer = false;
            //    var price = Product.CalculatePriceForProduct(d.Product, order.Client, d.IsCredit, d.Damaged, d.UnitOfMeasure, false, out cameFromOffer, true, order);
            //    var t = price * d.Qty;
            //    var dc = (t - d.QtyPrice);
            //    if (dc > 0)
            //        discount += dc;
            //}

            //#0012619
            AddCellToHeader(tableLayout, string.Empty);
            AddCellToBody(tableLayout, string.Empty);

            AddCellToHeader(tableLayout, "Subtotal");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, (order.CalculateItemCost()).ToCustomString(), HorizontalAlignment.RIGHT);

            int rowSpan = order.IsWorkOrder ? 5 : 4;

            tableLayout.AddCell(GetSignatureCell(order, rowSpan));

            AddCellToHeader(tableLayout, "Taxes");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, order.CalculateTax().ToCustomString(), HorizontalAlignment.RIGHT);

            if (order.IsWorkOrder || Config.AllowOtherCharges)
            {
                AddCellToHeader(tableLayout, "Freight");
                if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                    AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                else
                    AddCellToBody(tableLayout, order.CalculatedFreight().ToCustomString(), HorizontalAlignment.RIGHT);

                //// Add Other Charges
                AddCellToHeader(tableLayout, "Other Charges");
                if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                    AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                else
                    AddCellToBody(tableLayout, order.CalculatedOtherCharges().ToCustomString(), HorizontalAlignment.RIGHT);
            }

            var totalLabel = order.AsPresale ? "Total" : "Total Invoice";

            if (order.IsWorkOrder)
                totalLabel = "Total";

            AddCellToHeader(tableLayout, totalLabel);
            var total = order.OrderTotalCost();

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            var payment = GetPayment(order);

            if (!order.IsWorkOrder)
            {
                AddCellToHeader(tableLayout, "Payment");

                if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                    AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                else
                    AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT);
            }

            doc.Add(tableLayout);

            float[] headers2 = { 20, 50, 15, 15 };  //Header Widths

            var tableLayout2 = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout2, "Receiver Name");
            AddCellToBody(tableLayout2, (!string.IsNullOrEmpty(order.SignatureName) ? order.SignatureName : ""), HorizontalAlignment.LEFT);

            AddCellToHeader(tableLayout2, "Total Due");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout2, (total - payment).ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout2);

            Table footer = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();

            if (!string.IsNullOrEmpty(order.Comments))
            {
                Cell text1 = new Cell();
                var tt = new Paragraph("Order Comment:" + order.Comments);
                tt.AddStyle(GetSmall8Font());
                tt.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                text1.Add(tt);

                footer.AddCell(text1);
            }

            if (!order.IsWorkOrder)
            {
                Cell celltext = new Cell();
                var ttt = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
                ttt.AddStyle(GetSmall8Font());
                ttt.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                celltext.Add(ttt);

                footer.AddCell(celltext);
            }

            doc.Add(footer);
        }

        #endregion

    }
}