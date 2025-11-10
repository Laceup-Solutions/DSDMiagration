





using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;



using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using iText.IO.Font.Constants;

namespace LaceupMigration
{
    public class FineFoodPdf : DefaultPdfProvider
    {
        #region Load Order

        protected override void AddCellToHeader(Table tableLayout, string cellText, int alignment = 0)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8WhiteFont());
            paragraph.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            paragraph.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            paragraph.SetKeepTogether(false);
            paragraph.SetMultipliedLeading(1f);

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            cell.SetKeepTogether(false);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected override void AddCellToBody(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.SetKeepTogether(false);
            paragraph.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            paragraph.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            paragraph.SetMultipliedLeading(1f);

            paragraph.AddStyle(GetBigFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            cell.SetPadding(5);
            cell.SetKeepTogether(false);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected void AddCellToBodySmall(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.SetKeepTogether(false);
            paragraph.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            paragraph.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            paragraph.SetMultipliedLeading(1f);

            paragraph.AddStyle(GetSmallFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            cell.SetPadding(5);
            cell.SetKeepTogether(false);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected override void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.SetKeepTogether(false);
            paragraph.SetMultipliedLeading(1f);
            paragraph.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            paragraph.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            paragraph.AddStyle(GetBigFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
            cell.SetPadding(5);
            cell.SetKeepTogether(false);

            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected override Style GetBigFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(13);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }


        protected Style GetSmallFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected override Style GetSmall8WhiteFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(13);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected override void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers =
            {
                 8.7f,
                13.0f,
                26.1f,
                 8.7f,
                 8.7f,
                 8.7f,
                 8.7f,
                 8.7f,
                 9.6f
            };

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "UPC");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "UoM");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "List Price");
            AddCellToHeader(tableLayout, "Discount");
            AddCellToHeader(tableLayout, "Discounted Price");
            AddCellToHeader(tableLayout, "Extended");

            float qtyBoxes = 0;



            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
                {
                    if (detail.OrderDiscountId > 0)
                        continue;

                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                    AddCellToBodySmall(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? FormatUpcForWrap(product.Upc) : "");

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    if (detail.Product.IsDiscountItem)
                        name = detail.Comments;

                    AddCellToBody(tableLayout, name);

                    AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : "");

                    AddCellToBody(tableLayout, detail.Qty.ToString());

                    bool cameFromOffer = false;
                    var price = Product.CalculatePriceForProduct(detail.Product, order.Client, detail.IsCredit, detail.Damaged, detail.UnitOfMeasure, false, out cameFromOffer, true, order);

                    //var price = Product.GetPriceForProduct(detail.Product, order.Client, false, false);

                    var dc = (price - detail.Price);
                    if (dc < 0 || detail.IsCredit)
                        dc = 0;

                    double discountPrice = 0;
                    double totalPrice = 0;

                    if (OrderDiscount.HasDiscounts)
                    {
                        dc = detail.CostPrice;

                        if (detail.CostPrice == 0)
                            discountPrice = 0;
                        else
                            discountPrice = detail.Price - detail.CostPrice;

                        totalPrice = (detail.Price - detail.CostPrice) * detail.Qty;

                        //if (totalPrice == 0)
                        //{
                        //    dc = detail.CostPrice;
                        //    discountPrice = 0;
                        //}
                    }

                    if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                    else
                        AddCellToBody(tableLayout, price.ToCustomString(), HorizontalAlignment.RIGHT);

                    if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                    else
                        AddCellToBody(tableLayout, dc.ToCustomString(), HorizontalAlignment.RIGHT);

                    string diff = "";
                    if (detail.IsCredit)
                        diff += "-";

                    if (OrderDiscount.HasDiscounts)
                    {
                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, discountPrice.ToCustomString(), HorizontalAlignment.RIGHT);

                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, diff + (totalPrice).ToCustomString(), HorizontalAlignment.RIGHT);

                    }
                    else
                    {
                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);

                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

                    }

                    if (detail.UnitOfMeasure != null)
                        qtyBoxes += detail.Qty * detail.UnitOfMeasure.Conversion;
                    else
                        qtyBoxes += detail.Qty;
                }
            }


            doc.Add(tableLayout);
        }

        private string FormatUpcForWrap(string upc)
        {
            if (string.IsNullOrEmpty(upc)) return upc;

            if (upc.Contains(","))
            {
                return upc.Replace(",", " ");
            }
            else
                return upc;
        }

        #endregion
    }
}