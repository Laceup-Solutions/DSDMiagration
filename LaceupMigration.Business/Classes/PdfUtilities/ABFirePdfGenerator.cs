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
using iText.IO.Font.Constants;
using iText.Kernel.Font;

using iText.IO.Source;


namespace LaceupMigration
{
    public class ABFirePdfGenerator : IPdfProvider
    {
        #region General

        protected virtual void AddCompanyInfo(Document doc, Order order)
        {
            AddCompanyInfoWithLogo(doc, order);

        }

        protected virtual void AddCompanyInfoWithLogo(Document doc, Order order)
        {
            AddTextLine(doc, "\n", GetNormalFont());

            float[] headers = { 15, 35, 15, 35 };  //Header Widths


            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();
            //
            // Drawable drawable = activity.Resources.GetDrawable(Resource.Drawable.abcustomPdfLogo);
            // Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;
            // ByteArrayOutputStream stream = new ByteArrayOutputStream();
            // bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            // byte[] bitMapData = stream.ToArray();
            //
            // Image jpg = new Image(ImageDataFactory.Create(bitMapData));
            // jpg.ScaleToFit(380f, 190f);

            Cell img = new Cell(7, 3);
            // img.Add(jpg);
            img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            img.SetVerticalAlignment(VerticalAlignment.MIDDLE);
            img.SetBorder(Border.NO_BORDER);
            tableLayout.AddCell(img);

            string terms = "";
            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId) : null;
            var salesmanName = string.Empty;
            if (salesman != null)
            {
                salesmanName = salesman.Name.Replace(salesman.OriginalId, "");
                salesmanName = salesmanName.Trim();
            }

            AddInvoiceHeader(tableLayout, "INVOICE", HorizontalAlignment.LEFT, Border.NO_BORDER, GetReallyBigFont());
            AddInvoiceHeader(tableLayout, "INVOICE#: " + order.PrintedOrderId, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "DATE: " + order.Date.ToShortDateString(), HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "CUSTOMER#: " + order.Client.OriginalId, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "SALES PERSON: " + salesmanName, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "PO#: " + order.PONumber, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "TERMS: " + terms, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);

            doc.Add(tableLayout);

        }
        protected virtual void AddCompanyInfo(Document doc, Invoice order)
        {
            AddCompanyInfoWithLogo(doc, order);

        }

        protected virtual void AddCompanyInfoWithLogo(Document doc, Invoice order)
        {
            AddTextLine(doc, "\n", GetNormalFont());

            float[] headers = { 15, 35, 15, 35 };  //Header Widths


            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();


            // Drawable drawable = activity.Resources.GetDrawable(Resource.Drawable.abcustomPdfLogo);
            // Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;
            // ByteArrayOutputStream stream = new ByteArrayOutputStream();
            // bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            // byte[] bitMapData = stream.ToArray();
            //
            // Image jpg = new Image(ImageDataFactory.Create(bitMapData));
            // jpg.ScaleToFit(380f, 190f);

            Cell img = new Cell(7, 3);
            // img.Add(jpg);
            img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            img.SetVerticalAlignment(VerticalAlignment.MIDDLE);
            img.SetBorder(Border.NO_BORDER);

            tableLayout.AddCell(img);

            string terms = "";
            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId) : null;
            var salesmanName = string.Empty;
            if (salesman != null)
            {
                salesmanName = salesman.Name.Replace(salesman.OriginalId, "");
                salesmanName = salesmanName.Trim();
            }

            AddInvoiceHeader(tableLayout, "INVOICE", HorizontalAlignment.LEFT, Border.NO_BORDER, GetReallyBigFont());
            AddInvoiceHeader(tableLayout, "INVOICE#: " + order.InvoiceNumber, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "DATE: " + order.Date.ToShortDateString(), HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "CUSTOMER#: " + order.Client.OriginalId, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "SALES PERSON: " + salesmanName, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);
            AddInvoiceHeader(tableLayout, "TERMS: " + terms, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true), true);

            doc.Add(tableLayout);

        }

        protected virtual void AddOrderClientInfo(Document doc, Client client, bool isQuote = false)
        {
            if (client == null)
                return;

            float[] headers = { 50, 50 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();


            var clientName = client.ClientName;
            if (!string.IsNullOrEmpty(client.OriginalId) && clientName.Contains(client.OriginalId))
            {
                //AN1-T227
                clientName = clientName.Replace(client.OriginalId, "").Trim();
            }

            var soldTo = "SOLD TO:" + "\n" + clientName + "\n";

            var shipTo = "SHIP TO:" + "\n" + clientName + "\n";

            if (!string.IsNullOrEmpty(client.ContactName))
            {
                soldTo += client.ContactName + "\n";
                shipTo += client.ContactName + "\n";
            }

            foreach (var item in ClientAddress(client, false))
                if (!string.IsNullOrEmpty(item))
                    soldTo += item + "\n";

            foreach (var item in ClientAddress(client, true))
                if (!string.IsNullOrEmpty(item))
                    shipTo += item + "\n";

            AddCellToBody(tableLayout, soldTo, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true));
            AddCellToBody(tableLayout, shipTo, HorizontalAlignment.LEFT, Border.NO_BORDER, GetFontSizeTen(true));

            doc.Add(tableLayout);
        }

        #endregion

        #region Tools

        protected virtual Style GetBigFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(15);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        } 
        
        protected virtual Style GetReallyBigFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(20);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetFontSizeTen(bool isBold, int fontSize = 10)
        {
            Style style = new Style();
            PdfFont font = null;
            if (isBold)
                font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            else
                font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            style.SetFont(font);
            style.SetFontSize(fontSize);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetNormalFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            style.SetFont(font);
            style.SetFontSize(12);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetNormalBoldFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(12);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetSmallFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            style.SetFont(font);
            style.SetFontSize(6);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetSmall8Font()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetSmall8WhiteFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual void AddEmptySpace(int lines, Document doc)
        {
            for (int i = 0; i < lines; i++)
                doc.Add(new Paragraph("\n"));
        }

        protected virtual void AddTextLine(Document doc, string text, Style style, HorizontalAlignment alignment = HorizontalAlignment.LEFT)
        {
            Paragraph pdfTxet = new Paragraph(text);
            pdfTxet.AddStyle(style);
            pdfTxet.SetHorizontalAlignment(alignment);

            doc.Add(pdfTxet);
        }

        protected virtual void AddCellToHeader(Table tableLayout, string cellText, Style style = null, bool isSmall = false)
        {
            var paragraph = new Paragraph(cellText);
            if (style == null)
                paragraph.AddStyle(GetSmall8WhiteFont());
            else
                paragraph.AddStyle(style);

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            if (isSmall)
            {
                cell.SetPadding(2);
                cell.SetHeight(14);
            }
            else
            {
                cell.SetPadding(5);
            }
            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, bool isSmall = false)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            if (isSmall)
            {
                cell.SetPadding(2);
                cell.SetHeight(10);
            }
            else
            {
                cell.SetPadding(5);
            }
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Style style = null, bool isSmall = false)
        {
            var paragraph = new Paragraph(cellText);

            if (style != null)
                paragraph.AddStyle(style);
            else
                paragraph.AddStyle(GetSmall8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            if (isSmall)
            {
                cell.SetPadding(2);
                cell.SetHeight(12);
            }
            else
            {
                cell.SetPadding(5);
            }
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border, Style style = null, bool isSmall = false)
        {
            var paragraph = new Paragraph(cellText);

            if (style == null)
                paragraph.AddStyle(GetNormalFont());
            else
                paragraph.AddStyle(style);
            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            if (isSmall)
            {
                cell.SetPadding(2);
                cell.SetHeight(14);
            }
            else
            {
                cell.SetPadding(5);
            }
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        public void AddInvoiceHeader(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border, Style style = null, bool isSmall = false)
        {
            var paragraph = new Paragraph(cellText);

            if (style == null)
                paragraph.AddStyle(GetNormalFont());
            else
                paragraph.AddStyle(style);
            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            if (isSmall)
            {
                cell.SetPadding(2);
                cell.SetHeight(14);
            }
            else
            {
                cell.SetPadding(5);
            }
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual string[] ClientAddress(Client client, bool shipTo = true)
        {
            var addr = shipTo ? client.ShipToAddress : client.BillToAddress;

            if (string.IsNullOrEmpty(addr))
                return new string[] { };

            var parts = addr.Split(new char[] { '|' });
            if (parts != null)
            {
                if (parts.Length == 5)
                {
                    parts[2] = parts[2].Trim() + ", " + parts[3].Trim() + " " + parts[4].Trim();
                    if (parts[1].Trim().Length == 0)
                    {
                        var newParts = new string[2];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[2].Trim();
                        return newParts;
                    }
                    else
                    {
                        var newParts = new string[3];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[1].Trim();
                        newParts[2] = parts[2].Trim();
                        return newParts;
                    }
                }
                if (parts.Length == 4)
                {
                    parts[2] = parts[2].Trim() + ", " + parts[3].Trim();
                    if (parts[1].Trim().Length == 0)
                    {
                        var newParts = new string[2];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[2].Trim();
                        return newParts;
                    }
                    else
                    {
                        var newParts = new string[3];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[1].Trim();
                        newParts[2] = parts[2].Trim();
                        return newParts;
                    }
                }
                return parts;
            }
            return new string[] { };
        }

        protected virtual double GetPayment(Order order)
        {
            var payments = DataAccess.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var paidInFull = payments != null && payments.Sum(x => x.Amount) == order.OrderTotalCost();

                return payments[0].Amount;
            }

            return 0;
        }

        protected virtual string GetSignatureImage(Order order)
        {
            return order.ConvertSignatureToBitmap();
        }

        protected virtual Cell GetSignatureCell(Order order)
        {
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                var imgPath = GetSignatureImage(order);

                Image jpg = new Image(ImageDataFactory.Create(imgPath));
                jpg.ScaleToFit(75f, 75f);

                Cell img = new Cell(4, 3);
                img.Add(jpg);
                img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                img.SetVerticalAlignment(VerticalAlignment.MIDDLE);

                return img;
            }

            Cell empty = new Cell(4, 3);
            var prg = new Paragraph("X");
            prg.AddStyle(GetSmall8Font());
            empty.Add(prg);
            empty.SetVerticalAlignment(VerticalAlignment.BOTTOM);

            return empty;
        }

        #endregion

        #region Invoice

        public string GetInvoicePdf(Invoice invoice)
        {
            string name = string.Format("invoice {0}.pdf", invoice.InvoiceNumber);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            string temp_Path = System.IO.Path.GetTempFileName();

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(temp_Path);
            PdfDocument pdfdoc = new PdfDocument(writer);
            Document doc = new Document(pdfdoc);
            doc.SetMargins(10, 10, 10, 10);

            AddContentToPDF(doc, invoice);

            doc.Close();

            ManipulatePdf(temp_Path, targetFile);

            return targetFile;
        }

        protected virtual void AddContentToPDF(Document doc, Invoice invoice)
        {
            AddCompanyInfo(doc, invoice);

            AddOrderClientInfo(doc, invoice.Client);

            // AddOrderHeaderTable(doc, invoice);

            AddOrderDetailsTable(doc, invoice);

            AddFooterTable(doc, invoice);
        }

        protected virtual void AddOrderInfo(Document doc, Invoice invoice)
        {
            AddTextLine(doc, "Invoice", GetBigFont(), HorizontalAlignment.CENTER);
            AddTextLine(doc, "Invoice #:" + invoice.InvoiceNumber, GetNormalFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Invoice invoice)
        {
            //Create PDF Table
            float[] headers = { 33, 33, 33 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers));

            tableLayout.SetWidth(UnitValue.CreatePercentValue(60));

            //Add header
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Email");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "", true);

            string terms = "";
            if (invoice.Client.ExtraProperties != null)
            {
                var termsExtra = invoice.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            AddCellToBody(tableLayout, terms, true);

            doc.Add(tableLayout);
        }

        protected virtual void AddOrderDetailsTable(Document doc, Invoice invoice)
        {
            //Create PDF Table
            float[] headers = { 9, 35, 5, 5, 8, 8, 30 };  //Header

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Extended");
            AddCellToHeader(tableLayout, "Comment:");

            float qtyBoxes = 0;
            int count = 0;

            if (invoice.Details != null)
            {
                // DETAILS
                foreach (InvoiceDetail detail in invoice.Details)
                {
                    if (count == 31)
                    {
                        doc.Add(tableLayout);

                        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                        tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();
                        //add header
                        AddCompanyInfo(doc, invoice);

                        AddOrderClientInfo(doc, invoice.Client);

                        //AddOrderHeaderTable(doc, invoice);

                        AddCellToHeader(tableLayout, "Item Id");
                        AddCellToHeader(tableLayout, "Description");
                        AddCellToHeader(tableLayout, "Qty");
                        AddCellToHeader(tableLayout, "UoM");
                        AddCellToHeader(tableLayout, "Unit Price");
                        AddCellToHeader(tableLayout, "Extended");
                        AddCellToHeader(tableLayout, "Comments:");

                        count = 0;
                    }

                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "", HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    var name = product != null ? product.Name : "";

                    name = GetDescription(name, product);

                    AddCellToBody(tableLayout, name, HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    AddCellToBody(tableLayout, detail.Quantity.ToString(), HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    if (!Config.HidePriceInTransaction)
                    {
                        AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                        AddCellToBody(tableLayout, (detail.Quantity * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                    }
                    else
                    {
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                    }

                    qtyBoxes += Convert.ToSingle(detail.Quantity);

                    AddCellToBody(tableLayout, detail.Comments, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

                    count++;
                }
            }

            doc.Add(tableLayout);
            AddTextLine(doc, "\n", GetSmallFont());

            if (count > 19)
            {
                //add header in a new page
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                AddCompanyInfo(doc, invoice);

                AddOrderClientInfo(doc, invoice.Client);

                //AddOrderHeaderTable(doc, invoice);

                AddTextLine(doc, "\n", GetSmallFont());
            }
        }

        private string GetDescription(string name, Product product)
        {
            if (product != null)
            {
                name = name.Replace(product.Code, " ");
                name = name.Trim(' ');
                name = name.Trim('-');
                name = name.Trim(' ');

                return name;
            }
            else
                return string.Empty;
        }

        //keep this
        protected virtual void AddFooterTable(Document doc, Invoice invoice)
        {
            float[] headers = { 45, 15, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            signature.SetPadding(5);
            var prg = new Paragraph("Customer Signature");
            prg.AddStyle(GetSmall8Font());
            signature.Add(prg);
            signature.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            tableLayout.AddCell(signature);

            AddCellToHeader(tableLayout, "Total Qty");
            AddCellToBody(tableLayout, invoice.Details.Sum(x => x.Quantity).ToString());

            AddCellToHeader(tableLayout, "Total");

            if (Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, invoice.Amount.ToCustomString(), HorizontalAlignment.RIGHT);

            Cell empty = new Cell(1, 3);
            var paragraph = new Paragraph("X");
            paragraph.AddStyle(GetSmall8Font());
            empty.Add(paragraph);
            empty.SetVerticalAlignment(VerticalAlignment.BOTTOM);

            tableLayout.AddCell(empty);

            AddCellToHeader(tableLayout, "Total Due");
            if (Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, invoice.Balance.ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);

            Table footer = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();

            if (!string.IsNullOrEmpty(invoice.Comments))
            {
                Cell text1 = new Cell();
                var pp = new Paragraph("Invoice Comment:" + invoice.Comments);
                pp.AddStyle(GetSmall8Font());
                text1.Add(pp);
                text1.SetHorizontalAlignment(HorizontalAlignment.LEFT);

                footer.AddCell(text1);
            }

            Cell celltext = new Cell();
            var texttext = new Paragraph("Remit Payment To:  P.O. Box 1211 Salinas, CA 93902\n" +
                                         "(Please indicate invoice number with your remittance)");
            var textext4 = new Paragraph("A monthly finance charge of 1.5%(18% Annually) may be charged on all past due accounts.");
            celltext.Add(texttext);
            celltext.Add(textext4);
            texttext.AddStyle(GetNormalFont());
            textext4.AddStyle(GetNormalBoldFont());

            footer.AddCell(celltext);

            doc.Add(footer);
        }

        public string GetInvoicesPdf(List<Invoice> invoices)
        {
            string name = string.Format("invoice group {0}.pdf", Guid.NewGuid().ToString());
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);
            string temp_Path = System.IO.Path.GetTempFileName();

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(temp_Path);
            PdfDocument pdf = new PdfDocument(writer);
            Document doc = new Document(pdf);
            doc.SetMargins(10, 10, 10, 10);

            int i = 0;

            foreach (var invoice in invoices)
            {
                AddContentToPDF(doc, invoice);

                if (i != invoices.Count - 1)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                i++;
            }

            doc.Close();

            ManipulatePdf(temp_Path, targetFile);

            return targetFile;
        }

        #endregion

        #region Order

        public virtual string GetOrderPdf(Order order)
        {
            string name = string.Format("order {0}.pdf", order.PrintedOrderId);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            string temp_Path = System.IO.Path.GetTempFileName();

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(temp_Path);
            PdfDocument pdf = new PdfDocument(writer);


            Document doc = new Document(pdf);
            doc.SetMargins(10, 10, 10, 10);

            AddContentToPDF(doc, order);

            doc.Close();

            ManipulatePdf(temp_Path, targetFile);

            return targetFile;
        }

        protected void ManipulatePdf(String source, String dest)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(source), new PdfWriter(dest));
            Document doc = new Document(pdfDoc);

            int numberOfPages = pdfDoc.GetNumberOfPages();
            for (int i = 1; i <= numberOfPages; i++)
            {
                // Write aligned text to the specified by parameters point
                doc.ShowTextAligned(new Paragraph("Page " + i + " of " + numberOfPages),
                        559, 20, i, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);
            }

            doc.Close();
        }


        protected virtual void AddContentToPDF(Document doc, Order order)
        {
            AddCompanyInfo(doc, order);

            AddOrderClientInfo(doc, order.Client, Config.UseQuote && order.IsQuote);

            //AddOrderHeaderTable(doc, order);

            AddTextLine(doc, " ", GetSmallFont());

            AddOrderDetailsTable(doc, order);

            AddFooterTable(doc, order);


        }

        class UomT
        {
            public UnitOfMeasure Unit { get; set; }
            public float Qty { get; set; }
        }

        private void AddUoMTotalsTable(Document doc, Order order)
        {
            Dictionary<string, float> lines = new Dictionary<string, float>();
            float totalUnits = 0;
            foreach (var item in order.Details)
            {
                if (item.Qty == 0)
                    continue;

                var name = item.UnitOfMeasure != null ? item.UnitOfMeasure.Name : "";
                float conv = item.UnitOfMeasure != null ? item.UnitOfMeasure.Conversion : 1;

                totalUnits += item.Qty * conv;

                if (!lines.ContainsKey(name))
                    lines.Add(name, 0);

                lines[name] += item.Qty;
            }

            float[] headers = { 20, 20, 60 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Unit of Measure");
            AddCellToHeader(tableLayout, "Total Qty");
            AddCellToBody(tableLayout, "", HorizontalAlignment.RIGHT, Border.NO_BORDER);

            foreach (var item in lines)
            {
                AddCellToBody(tableLayout, item.Key);
                AddCellToBody(tableLayout, item.Value.ToString());
                AddCellToBody(tableLayout, "", HorizontalAlignment.RIGHT, Border.NO_BORDER);
            }

            AddCellToBody(tableLayout, "Total Units");
            AddCellToBody(tableLayout, Math.Round(totalUnits, Config.Round).ToString());
            AddCellToBody(tableLayout, "", HorizontalAlignment.RIGHT, Border.NO_BORDER);

            doc.Add(tableLayout);
        }

        protected virtual void AddOrderInfo(Document doc, Order order)
        {
            string docName = string.Empty;
            string docNum = string.Empty;

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
                    docName = "Invoice";
                    docNum = "Invoice #:" + order.PrintedOrderId;
                }
            }

            AddTextLine(doc, docName, GetBigFont(), HorizontalAlignment.CENTER);

            if (!string.IsNullOrEmpty(docNum))
                AddTextLine(doc, docNum, GetNormalFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Order order)
        {
            //Create PDF Table
            float[] headers = { 33, 33, 33 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers));

            tableLayout.SetWidth(UnitValue.CreatePercentValue(60));

            //Add header
            AddCellToHeader(tableLayout, "Purchase Order");
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Email");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId) : null;

            if (string.IsNullOrEmpty(order.PONumber) && Config.AutoGeneratePO)
            {
                string po = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");
                order.PONumber = po;
            }

            AddCellToBody(tableLayout, order.PONumber ?? "", true);
            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "", true);

            string terms = "";
            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            AddCellToBody(tableLayout, terms, true);

            doc.Add(tableLayout);

            if (order.AsPresale && order.ShipDate != DateTime.MinValue)
                AddShipDateTable(doc, order.ShipDate);
        }

        protected virtual void AddShipDateTable(Document doc, DateTime shipDate)
        {
            float[] headers2 = { 60, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToBody(tableLayout, "");
            AddCellToHeader(tableLayout, "Ship Date");
            AddCellToBody(tableLayout, shipDate.ToShortDateString());

            doc.Add(tableLayout);
        }

        protected virtual void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 9, 35, 5, 5, 8, 8, 30 };   //Header

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "UoM");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Extended");
            AddCellToHeader(tableLayout, "Comments:");

            float qtyBoxes = 0;

            int count = 0;

            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
                {
                    if (count == 31)
                    {
                        doc.Add(tableLayout);

                        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                        tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();
                        //add header
                        AddCompanyInfo(doc, order);

                        AddOrderClientInfo(doc, order.Client);

                        //AddOrderHeaderTable(doc, order);

                        AddCellToHeader(tableLayout, "Item Id");
                        AddCellToHeader(tableLayout, "Description");
                        AddCellToHeader(tableLayout, "Qty");
                        AddCellToHeader(tableLayout, "UoM");
                        AddCellToHeader(tableLayout, "Unit Price");
                        AddCellToHeader(tableLayout, "Extended");
                        AddCellToHeader(tableLayout, "Comments:");

                        count = 0;
                    }

                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "", HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    var name = product != null ? product.Name : "";

                    name = GetDescription(name, product);

                    AddCellToBody(tableLayout, name, HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);
                    AddCellToBody(tableLayout, detail.Qty.ToString(), HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : string.Empty, HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

                    if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                    else
                        AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

                    string diff = "";
                    if (detail.IsCredit)
                        diff += "-";

                    if (Config.HidePriceInSelfService && Config.SelfServiceUser || (Config.SelfService) || Config.HidePriceInTransaction)
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
                    else
                        AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

                    if (detail.UnitOfMeasure != null)
                        qtyBoxes += detail.Qty * detail.UnitOfMeasure.Conversion;
                    else
                        qtyBoxes += detail.Qty;

                    AddCellToBody(tableLayout, detail.Comments, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

                    count++;
                }
            }

            doc.Add(tableLayout);

            AddTextLine(doc, "\n", GetSmallFont());

            if (count > 19)
            {
                AddTextLine(doc, "\n", GetSmallFont());
                //add header in a new page
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                AddCompanyInfo(doc, order);

                AddOrderClientInfo(doc, order.Client);

                //AddOrderHeaderTable(doc, order);

                AddTextLine(doc, "\n", GetSmallFont());
            }
        }

        protected virtual void AddFooterTable(Document doc, Order order)
        {
            float[] headers = { 30, 30, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            var text = new Paragraph("Customer Signature");
            text.AddStyle(GetFontSizeTen(false, 9));
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            signature.Add(text);

            tableLayout.AddCell(signature);

            /*AddCellToHeader(tableLayout, "Total Units");
            AddCellToBody(tableLayout, order.Details.Sum(x => x.UnitOfMeasure != null ? x.Qty * x.UnitOfMeasure.Conversion : x.Qty).ToString());*/

            //#0012619
            AddCellToHeader(tableLayout, string.Empty, GetFontSizeTen(false, 9), true);
            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

            AddCellToHeader(tableLayout, "Subtotal", GetFontSizeTen(false, 9), true);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout, order.CalculateItemCost().ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

            tableLayout.AddCell(GetSignatureCell(order));

            AddCellToHeader(tableLayout, "Discount", GetFontSizeTen(false, 9), true);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout, order.CalculateDiscount().ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

            AddCellToHeader(tableLayout, "Taxes", GetFontSizeTen(false, 9), true);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout, order.CalculateTax().ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

            var totalLabel = order.AsPresale ? "Total" : "Total Invoice";

            AddCellToHeader(tableLayout, totalLabel, GetFontSizeTen(false, 9), true);
            var total = order.OrderTotalCost();

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

            AddCellToHeader(tableLayout, "Payment", GetFontSizeTen(false, 9), true);
            var payment = GetPayment(order);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

            doc.Add(tableLayout);

            float[] headers2 = { 20, 50, 15, 15 };  //Header Widths

            var tableLayout2 = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout2, "Receiver Name", GetFontSizeTen(false, 9), true);
            AddCellToBody(tableLayout2, (!string.IsNullOrEmpty(order.SignatureName) ? order.SignatureName : ""), HorizontalAlignment.LEFT, GetFontSizeTen(false, 9), true);

            AddCellToHeader(tableLayout2, "Total Due", GetFontSizeTen(false, 9), true);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);
            else
                AddCellToBody(tableLayout2, (total - payment).ToCustomString(), HorizontalAlignment.RIGHT, GetFontSizeTen(false, 9), true);

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

            Cell celltext = new Cell();
            var texttext = new Paragraph("Remit Payment To:  P.O. Box 1211 Salinas, CA 93902\n" +
                                         "(Please indicate invoice number with your remittance)");
            var textext4 = new Paragraph("A monthly finance charge of 1.5%(18% Annually) may be charged on all past due accounts.");
            celltext.Add(texttext);
            celltext.Add(textext4);
            texttext.AddStyle(GetNormalFont());
            textext4.AddStyle(GetNormalBoldFont());

            footer.AddCell(celltext);

            doc.Add(footer);
        }


        public virtual string GetOrdersPdf(List<Order> orders)
        {
            string name = string.Format("order group {0}.pdf", Guid.NewGuid().ToString());
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);
            string temp_Path = System.IO.Path.GetTempFileName();

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(temp_Path);
            PdfDocument pdf = new PdfDocument(writer);
            Document doc = new Document(pdf);

            doc.SetMargins(10, 10, 10, 10);
            int i = 0;

            foreach (var order in orders)
            {
                if (order.OrderType == OrderType.Consignment)
                    AddContentToPDF(doc, order, true);
                else
                    AddContentToPDF(doc, order);

                if (i != orders.Count - 1)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                i++;
            }

            doc.Close();

            ManipulatePdf(temp_Path, targetFile);

            return targetFile;
        }


        #endregion

        #region Consignment

        public string GetConsignmentPdf(Order order, bool counting)
        {
            string name = string.Format("consignment copy of consignment {0}.pdf", order.PrintedOrderId);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);
            doc.SetMargins(10, 10, 10, 10);

            AddContentToPDF(doc, order, counting);

            doc.Close();

            return targetFile;
        }

        protected virtual void AddContentToPDF(Document doc, Order order, bool counting)
        {
            if (Config.ParInConsignment || Config.ConsignmentBeta)
            {
                AddContentToPDFConsPar(doc, order);
                return;
            }

            AddCompanyInfo(doc, order);

            AddOrderClientInfo(doc, order.Client);

            //AddOrderHeaderTable(doc, order);

            AddOrderDetailsTable(doc, order, counting);

            AddFooterTable(doc, order, counting);
        }

        protected virtual void AddOrderInfo(Document doc, Order order, bool counting)
        {
            if (!counting)
            {
                AddTextLine(doc, "CONSIGNMENT CONTRACT", GetBigFont(), HorizontalAlignment.CENTER);
                AddTextLine(doc, "Invoice #:" + order.PrintedOrderId, GetNormalFont(), HorizontalAlignment.CENTER);
            }
            else
            {
                if (order.AsPresale)
                {
                    AddTextLine(doc, "SALES ORDER", GetBigFont(), HorizontalAlignment.CENTER);
                    AddTextLine(doc, "Order # " + order.PrintedOrderId, GetNormalFont(), HorizontalAlignment.CENTER);
                }
                else
                {
                    AddTextLine(doc, "Invoice", GetBigFont(), HorizontalAlignment.CENTER);
                    AddTextLine(doc, "Invoice #:" + order.PrintedOrderId, GetNormalFont(), HorizontalAlignment.CENTER);
                }
            }

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderDetailsTable(Document doc, Order order, bool counting)
        {
            List<BatteryItem> sales = new List<BatteryItem>();
            List<BatteryItem> cores = new List<BatteryItem>();
            List<BatteryItem> rotations = new List<BatteryItem>();
            List<BatteryItem> adjustments = new List<BatteryItem>();
            List<BatteryItem> tax = new List<BatteryItem>();

            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in order.Details)
                {
                    Product product = detail.Product;

                    int factor = 1;
                    if (detail.IsCredit)
                        factor = -1;

                    float qty = detail.Qty;
                    double price = detail.Price * factor;

                    if (!counting)
                    {
                        if (detail.ConsignmentUpdated)
                        {
                            qty = detail.ConsignmentNew;
                            price = detail.ConsignmentNewPrice;
                        }
                        else if (detail.ConsignmentNew == 0)
                            qty = detail.ConsignmentOld;
                    }

                    if (qty == 0)
                        continue;

                    sales.Add(new BatteryItem() { Product = product, Price = price, Qty = qty });

                    var core = GetCoreForDetail(order, detail, detail.Qty);
                    if (core != null && core.Qty > 0)
                        cores.Add(core);

                    var rotated = GetRotateForDetail(order, detail);
                    if (rotated != null)
                        rotations.Add(rotated);

                    var adj = GetAdjustmentForDetail(order, detail);
                    if (adj != null)
                    {
                        adjustments.Add(adj);
                        if (Config.CoreAsCredit)
                        {
                            var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                            if (coreId != null)
                            {
                                var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

                                if (relatedCore != null)
                                    cores.Add(new BatteryItem() { Product = relatedCore, Qty = adj.Qty, Price = 0 });
                            }
                        }
                    }
                }

                AddOrderDetailsTableSection(doc, sales, "Sales");
                AddOrderDetailsTableSection(doc, cores, "Cores");
                AddOrderDetailsTableSection(doc, rotations, "Rotations");
                AddOrderDetailsTableSection(doc, adjustments, "Adjustments");
                AddOrderDetailsTableSection(doc, tax, "Taxes");

                AddTextLine(doc, " ", GetSmallFont());
            }
        }

        void AddOrderDetailsTableSection(Document doc, List<BatteryItem> values, string sectionName)
        {
            if (values.Count(x => x.Qty > 0) == 0)
                return;

            AddTextLine(doc, " ", GetSmallFont());

            AddTextLine(doc, sectionName, GetNormalBoldFont());

            AddTextLine(doc, " ", GetSmallFont());

            float[] headers = { 15, 15, 30, 10, 15, 15 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "UPC");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Extended");

            foreach (var detail in values)
            {
                Product product = detail.Product;

                AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");

                var name = product != null ? product.Name : "";

                AddCellToBody(tableLayout, name);

                float qty = detail.Qty;
                double price = detail.Price;

                AddCellToBody(tableLayout, qty.ToString());

                AddCellToBody(tableLayout, price.ToCustomString(), HorizontalAlignment.RIGHT);

                AddCellToBody(tableLayout, (qty * price).ToCustomString(), HorizontalAlignment.RIGHT);
            }

            doc.Add(tableLayout);
        }

        protected virtual void AddFooterTable(Document doc, Order order, bool counting)
        {
            float[] headers = { 45, 15, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            float totalQty = 0;

            foreach (var detail in order.Details)
            {
                float qty = detail.Qty;
                double price = detail.Price;

                if (!counting)
                {
                    if (detail.ConsignmentUpdated)
                    {
                        qty = detail.ConsignmentNew;
                        price = detail.ConsignmentNewPrice;
                    }
                    else if (detail.ConsignmentNew == 0)
                        qty = detail.ConsignmentOld;
                }

                totalQty += qty;
            }

            if (Config.UseBattery)
            {
                Cell signature = new Cell(1, 3);
                var signatureText = new Paragraph("Customer Signature");
                signatureText.AddStyle(GetSmall8Font());
                signature.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                tableLayout.AddCell(signature);
            }
            else
            {
                Cell signature = new Cell();
                var signatureText = new Paragraph("Customer Signature");
                signatureText.AddStyle(GetSmall8Font());
                signature.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                tableLayout.AddCell(signature);

                AddCellToHeader(tableLayout, "Total Qty");
                AddCellToBody(tableLayout, totalQty.ToString());
            }

            var totalItemsCost = order.CalculateItemCost();
            var totalDiscount = order.CalculateDiscount();
            var totalTax = order.CalculateTax();

            AddCellToHeader(tableLayout, "Subtotal");
            AddCellToBody(tableLayout, totalItemsCost.ToCustomString(), HorizontalAlignment.RIGHT);

            tableLayout.AddCell(GetSignatureCell(order));

            AddCellToHeader(tableLayout, "Discount");
            AddCellToBody(tableLayout, totalDiscount.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Taxes");
            AddCellToBody(tableLayout, totalTax.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Total Invoice");
            var total = totalItemsCost - totalDiscount + totalTax;
            AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Payment");
            var payment = GetPayment(order);
            AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);

            float[] headers2 = { 20, 50, 15, 15 };  //Header Widths

            var tableLayout2 = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout2, "Receiver Name");
            AddCellToBody(tableLayout2, (!string.IsNullOrEmpty(order.SignatureName) ? order.SignatureName : ""), HorizontalAlignment.LEFT);

            AddCellToHeader(tableLayout2, "Total Due");
            AddCellToBody(tableLayout2, (total - payment).ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout2);

            Table footer = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();

            if (!string.IsNullOrEmpty(order.Comments))
            {
                Cell text1 = new Cell();
                var text1text = new Paragraph("Order Comment:" + order.Comments);
                text1text.AddStyle(GetSmall8Font());
                text1.Add(text1text);
                text1.SetHorizontalAlignment(HorizontalAlignment.LEFT);

                footer.AddCell(text1);
            }

            Cell celltext = new Cell();
            var texttext = new Paragraph("Remit Payment To:  P.O. Box 1211 Salinas, CA 93902\n" +
                                         "(Please indicate invoice number with your remittance)");
            var textext4 = new Paragraph("A monthly finance charge of 1.5%(18% Annually) may be charged on all past due accounts.");
            celltext.Add(texttext);
            celltext.Add(textext4);
            texttext.AddStyle(GetNormalFont());
            textext4.AddStyle(GetNormalBoldFont());

            footer.AddCell(celltext);

            doc.Add(footer);
        }

        #endregion

        #region Consignment Par

        private void AddContentToPDFConsPar(Document doc, Order order)
        {
            var structList = new List<ConsStruct>();
            foreach (var item in order.Details)
                structList.Add(ConsStruct.GetStructFromDetail(item));

            AddCompanyInfo(doc, order);

            AddOrderInfo(doc, order, true);

            AddOrderClientInfo(doc, order.Client);

            AddOrderHeaderTableConsPar(doc, order);

            AddOrderDetailsTableConsPar(doc, order);

        }

        protected virtual void AddOrderHeaderTableConsPar(Document doc, Order order)
        {
            //Create PDF Table

            float[] headers = { 40, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Email");

            var dateLabel = order.AsPresale ? "Date" : "Date";
            var timeLabel = order.AsPresale ? "Time" : "Invoiced Date";

            AddCellToHeader(tableLayout, dateLabel);
            AddCellToHeader(tableLayout, timeLabel);

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            if (string.IsNullOrEmpty(order.PONumber) && Config.AutoGeneratePO)
            {
                string po = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");
                order.PONumber = po;
            }

            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "");

            string terms = "";
            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            AddCellToBody(tableLayout, terms);
            AddCellToBody(tableLayout, order.Date.ToShortDateString());
            AddCellToBody(tableLayout, order.Date.ToShortTimeString());

            doc.Add(tableLayout);
        }

        protected virtual void AddOrderDetailsTableConsPar(Document doc, Order order)
        {
            List<BatteryItem> sales = new List<BatteryItem>();
            List<BatteryItem> returns = new List<BatteryItem>();
            List<BatteryItem> damageds = new List<BatteryItem>();
            List<BatteryItem> cores = new List<BatteryItem>();
            List<BatteryItem> rotations = new List<BatteryItem>();
            List<BatteryItem> adjustments = new List<BatteryItem>();
            List<BatteryItem> tax = new List<BatteryItem>();

            foreach (var item in order.Details)
            {
                var detail = ConsStruct.GetStructFromDetail(item);

                if (detail.Sold > 0)
                {
                    sales.Add(new BatteryItem() { Product = item.Product, Qty = detail.Sold, Price = detail.Price });
                    var t = GetTaxForDetail(order, item, detail.Sold, false);
                    if (t != null)
                        tax.Add(t);
                }
                if (detail.Return > 0)
                {
                    returns.Add(new BatteryItem() { Product = item.Product, Qty = detail.Return, Price = detail.Price * -1 });
                    var t = GetTaxForDetail(order, item, detail.Return, true);
                    if (t != null)
                        tax.Add(t);
                }
                if (detail.Damaged > 0)
                {
                    damageds.Add(new BatteryItem() { Product = item.Product, Qty = detail.Damaged, Price = detail.Price * -1 });
                    var t = GetTaxForDetail(order, item, detail.Damaged, true);
                    if (t != null)
                        tax.Add(t);
                }

                var core = GetCoreForDetail(order, item, detail.Sold);
                if (core != null && core.Qty > 0)
                    cores.Add(core);

                var rotated = GetRotateForDetail(order, item);
                if (rotated != null)
                    rotations.Add(rotated);

                var adj = GetAdjustmentForDetail(order, item);
                if (adj != null)
                    adjustments.Add(adj);
            }

            AddOrderDetailsTableConsParSection(doc, sales, "Sales");

            AddOrderDetailsTableConsParSection(doc, returns, "Credit Return");

            AddOrderDetailsTableConsParSection(doc, damageds, "Credit Damaged");

            AddOrderDetailsTableConsParSection(doc, tax, "Taxes");

            AddOrderDetailsTableConsParSection(doc, cores, "Cores");

            AddOrderDetailsTableConsParSection(doc, rotations, "Rotations");

            AddOrderDetailsTableConsParSectionAdjustment(doc, adjustments, "Adjustments");

            float totalPicked = cores.Sum(x => x.PickedCores);

            AddFooterTableConsPar(doc, order, totalPicked);
        }

        protected virtual BatteryItem GetCoreForDetail(Order order, OrderDetail detail, float sold)
        {
            var core = DataAccess.GetSingleUDF("coreQty", detail.ExtraFields);
            var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

            if (string.IsNullOrEmpty(core) || coreId == null)
                return null;

            var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

            if (relatedCore == null)
                return null;

            var coreQty = Convert.ToDouble(core);

            var qty = sold - coreQty;

            var corePrice = Product.GetPriceForProduct(relatedCore, order, false, false);

            bool chargeCore = true;

            if (order.Client.NonVisibleExtraProperties != null)
            {
                var xC = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "corepaid");
                if (xC != null && xC.Item2.ToLowerInvariant() == "n")
                    chargeCore = false;
            }

            if (!chargeCore)
                corePrice = 0;

            if (Config.CoreAsCredit)
            {
                qty = coreQty;
                corePrice *= -1;
            }
            else if (qty < 0)
            {
                qty *= -1;
                corePrice *= -1;
            }

            return new BatteryItem() { Product = detail.Product, Qty = (float)qty, Price = corePrice, PickedCores = (float)coreQty };
        }

        protected virtual BatteryItem GetTaxForDetail(Order order, OrderDetail detail, float sold, bool isCredit)
        {
            if (sold == 0)
                return null;

            var item = order.Client.ExtraProperties != null ? order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            if (!useRelated)
                return null;

            var related = GetRelatedProduct(detail.Product);

            if (related == null)
                return null;

            var relatedPrice = Product.GetPriceForProduct(related, order, false, false);
            if (isCredit)
                relatedPrice *= -1;

            return new BatteryItem() { Product = related, Qty = sold, Price = relatedPrice };
        }

        protected virtual BatteryItem GetRotateForDetail(Order order, OrderDetail detail)
        {
            var rotation = DataAccess.GetSingleUDF("rotatedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(rotation))
                return null;

            var qty = int.Parse(rotation);

            if (qty == 0)
                return null;

            var rotatedId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");

            if (rotatedId == null)
                return null;

            var rotated = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == rotatedId.Item2);

            if (rotated == null)
                return null;

            var price = Product.GetPriceForProduct(rotated, order, false, false);
            if (!Config.ChargeBatteryRotation)
                price = 0;

            return new BatteryItem() { Product = detail.Product, Qty = qty, Price = price };
        }

        protected virtual BatteryItem GetAdjustmentForDetail(Order order, OrderDetail detail)
        {
            var adjQty = DataAccess.GetSingleUDF("adjustedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(adjQty))
                return null;

            var adjId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");

            if (adjId == null)
                adjId = new Tuple<string, string>("", detail.Product.ProductId.ToString());

            var adjustment = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == adjId.Item2);

            if (adjustment == null)
                adjustment = detail.Product;

            if (!Config.CoreAsCredit)
            {
                int time = 0;
                if (Config.WarrantyPerClient)
                {
                    time = order.GetIntWarrantyPerClient(detail.Product);
                    if (time == 0)
                        return null;
                }

                var ws = adjQty.Split(',');

                var adjPrice = Product.GetPriceForProduct(adjustment, order, false, false);

                if (!Config.WarrantyPerClient)
                {
                    var timeSt = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "time");

                    if (timeSt == null)
                        return null;

                    time = int.Parse(timeSt.Item2);
                }

                List<BatteryItem> warranties = new List<BatteryItem>();

                foreach (var item in ws)
                {
                    int x = int.Parse(item);

                    warranties.Add(new BatteryItem() { Age = x, Qty = 1, Price = 0 });
                }

                return new BatteryItem() { Product = adjustment, Warranties = warranties };
            }
            else
                return new BatteryItem() { Product = adjustment, Qty = int.Parse(adjQty), Price = 0 };
        }

        Product GetRelatedProduct(Product product)
        {
            int relatedId = 0;

            foreach (var p in product.ExtraProperties)
            {
                if (p.Item1.ToLower() == "relateditem")
                {
                    relatedId = Convert.ToInt32(p.Item2);
                    break;
                }
            }

            return Product.Find(relatedId, true);
        }

        private void AddOrderDetailsTableConsParSection(Document doc, List<BatteryItem> values, string sectionName)
        {
            if (values.Count(x => x.Qty > 0) == 0)
                return;

            AddTextLine(doc, " ", GetSmallFont());

            AddTextLine(doc, sectionName, GetNormalBoldFont());

            AddTextLine(doc, " ", GetSmallFont());

            //Create PDF Table
            float[] headers = { 40, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Total");

            float totalQty = 0;
            double total = 0;

            foreach (var item in values)
            {
                AddCellToBody(tableLayout, (item.Product != null) ? item.Product.Code : "");
                AddCellToBody(tableLayout, item.Qty.ToString());
                AddCellToBody(tableLayout, item.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                AddCellToBody(tableLayout, item.Total.ToCustomString(), HorizontalAlignment.RIGHT);

                totalQty += item.Qty;
                total += item.Total;
            }

            doc.Add(tableLayout);

            AddOrderDetailsTableConsParSectionTotals(doc, totalQty, total);
        }

        private void AddOrderDetailsTableConsParSectionAdjustment(Document doc, List<BatteryItem> values, string sectionName)
        {
            if (values.Count == 0)
                return;

            AddTextLine(doc, " ", GetSmallFont());

            AddTextLine(doc, sectionName, GetNormalBoldFont());

            AddTextLine(doc, " ", GetSmallFont());

            float[] headers = { 20, 20, 20, 20, 20 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "Age");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Total");

            float totalQty = 0;
            double total = 0;

            foreach (var itemL in values)
            {
                foreach (var item in itemL.Warranties)
                {
                    AddCellToBody(tableLayout, (itemL.Product != null) ? itemL.Product.Code : "");
                    AddCellToBody(tableLayout, item.Age.ToString());
                    AddCellToBody(tableLayout, item.Qty.ToString());
                    AddCellToBody(tableLayout, item.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                    AddCellToBody(tableLayout, item.Total.ToCustomString(), HorizontalAlignment.RIGHT);

                    totalQty += item.Qty;
                    total += item.Total;
                }
            }

            doc.Add(tableLayout);

            AddOrderDetailsTableConsParSectionTotals(doc, totalQty, total);
        }

        private void AddOrderDetailsTableConsParSectionTotals(Document doc, float totalQty, double total)
        {
            float[] headers = { 40, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            signature.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            var signatureText = new Paragraph("Total Qty");
            signatureText.AddStyle(GetSmall8Font());
            signature.Add(signatureText);
            signature.SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            tableLayout.AddCell(signature);

            AddCellToBody(tableLayout, totalQty.ToString());
            AddCellToBody(tableLayout, "");
            AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);
        }

        protected virtual void AddFooterTableConsPar(Document doc, Order order, float pickedCores)
        {
            AddTextLine(doc, " ", GetNormalBoldFont());

            float[] headers = { 70, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            var signatureText = new Paragraph("Customer Signature");
            signatureText.AddStyle(GetSmall8Font());
            signature.Add(signatureText);
            signature.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            tableLayout.AddCell(signature);

            AddCellToHeader(tableLayout, "Picked Cores");
            AddCellToBody(tableLayout, pickedCores.ToString());

            tableLayout.AddCell(GetSignatureCellConsPar(order));

            AddCellToHeader(tableLayout, "Subtotal");
            AddCellToBody(tableLayout, order.CalculateItemCost().ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Discount");
            AddCellToBody(tableLayout, order.CalculateDiscount().ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Taxes");
            AddCellToBody(tableLayout, order.CalculateTax().ToCustomString(), HorizontalAlignment.RIGHT);

            var totalLabel = order.AsPresale ? "Total" : "Total Invoice";

            AddCellToHeader(tableLayout, totalLabel);
            var total = order.OrderTotalCost();
            AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Payment");
            var payment = GetPayment(order);
            AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);

            float[] headers2 = { 20, 50, 15, 15 };  //Header Widths

            var tableLayout2 = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout2, "Receiver Name");
            AddCellToBody(tableLayout2, (!string.IsNullOrEmpty(order.SignatureName) ? order.SignatureName : ""), HorizontalAlignment.LEFT);

            AddCellToHeader(tableLayout2, "Total Due");
            AddCellToBody(tableLayout2, (total - payment).ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout2);

            Table footer = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();

            if (!string.IsNullOrEmpty(order.Comments))
            {
                Cell text1 = new Cell();
                var text1text = new Paragraph("Order Comment:" + order.Comments);
                text1text.AddStyle(GetSmall8Font());
                text1.Add(text1text);
                text1.SetHorizontalAlignment(HorizontalAlignment.LEFT);

                footer.AddCell(text1);
            }

            Cell celltext = new Cell();
            var texttext = new Paragraph("Remit Payment To:  P.O. Box 1211 Salinas, CA 93902\n" +
                                         "(Please indicate invoice number with your remittance)");
            var textext4 = new Paragraph("A monthly finance charge of 1.5%(18% Annually) may be charged on all past due accounts.");
            celltext.Add(texttext);
            celltext.Add(textext4);
            texttext.AddStyle(GetNormalFont());
            textext4.AddStyle(GetNormalBoldFont());

            footer.AddCell(celltext);

            doc.Add(footer);
        }

        protected virtual Cell GetSignatureCellConsPar(Order order)
        {
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                var imgPath = GetSignatureImage(order);

                Image jpg = new Image(ImageDataFactory.Create(imgPath));
                jpg.ScaleToFit(75f, 75f);

                Cell img = new Cell(5, 1);
                img.Add(jpg);
                img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                img.SetVerticalAlignment(VerticalAlignment.MIDDLE);

                return img;
            }

            Cell empty = new Cell(5, 1);
            var prg = new Paragraph("X");
            prg.AddStyle(GetSmall8Font());
            empty.Add(prg);
            empty.SetVerticalAlignment(VerticalAlignment.BOTTOM);

            return empty;
        }

        #endregion

        #region Transfer

        public string GetTransferPdf(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            return null;
        }

        #endregion

        #region Load Order

        public string GetLoadPdf(Order order)
        {
            string name = string.Format("load copy of load {0}.pdf", order.PrintedOrderId);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);

            AddLoadContentToPDF(doc, order);

            doc.Close();

            return targetFile;
        }

        protected virtual void AddLoadContentToPDF(Document doc, Order order)
        {
            AddCompanyInfo(doc, order);

            AddLoadInfo(doc, order);

            AddLoadHeaderTable(doc, order);

            AddLoadOrderDetailsTable(doc, order);

            AddLoadFooterTable(doc, order);
        }

        protected virtual void AddLoadInfo(Document doc, Order order)
        {
            string docName = "LOAD ORDER";

            AddTextLine(doc, docName, GetBigFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddLoadHeaderTable(Document doc, Order order)
        {
            float[] headers = { 40, 30, 30 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Date Created");
            AddCellToHeader(tableLayout, "Ship Date");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "");
            AddCellToBody(tableLayout, order.Date.ToString());
            AddCellToBody(tableLayout, order.ShipDate != DateTime.MinValue ? order.ShipDate.ToString() : "");

            doc.Add(tableLayout);
        }

        protected virtual void AddLoadOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 20, 50, 15, 15 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "UoM");
            AddCellToHeader(tableLayout, "Qty");

            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in order.Details)
                {
                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "");

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    AddCellToBody(tableLayout, name);
                    AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : "");
                    AddCellToBody(tableLayout, detail.Qty.ToString(), HorizontalAlignment.RIGHT);
                }
            }

            doc.Add(tableLayout);
        }

        protected virtual void AddLoadFooterTable(Document doc, Order order)
        {
            float[] headers = { 70, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToBody(tableLayout, "");
            AddCellToHeader(tableLayout, "Total Qty");
            AddCellToBody(tableLayout, order.Details.Sum(x => x.UnitOfMeasure != null ? x.Qty * x.UnitOfMeasure.Conversion : x.Qty).ToString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);

        }

        public string GetGoalPdf(GoalProgressDTO goal)
        {
            return null;
        }

        #region Payment

        protected virtual void AddGoalContentToPdf(Document doc, InvoicePayment payment)
        {
            AddPaymentInfo(doc, payment);

            if (payment.Client != null)
                AddOrderClientInfo(doc, payment.Client, false);

            AddPaymentHeaderTable(doc, payment);

            AddEmptySpace(1, doc);

            AddPaymentDetailsTable(doc, payment);

            AddPaymentFooterTable(doc, payment);
        }

        protected virtual void AddPaymentInfo(Document doc, InvoicePayment order)
        {
            string docName = "Invoice Payment";

            AddTextLine(doc, docName, GetBigFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddPaymentHeaderTable(Document doc, InvoicePayment order)
        {
            float[] headers = { 50, 50 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesman name:");
            AddCellToHeader(tableLayout, "Invoice:");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "");

            var RealInvoicesId = string.Empty;

            if (!string.IsNullOrEmpty(order.InvoicesId))
            {
                foreach (var idAsString in order.InvoicesId.Split(new char[] { ',' }))
                {
                    int id = 0;
                    Invoice invoice = null;
                    if (Config.SavePaymentsByInvoiceNumber)
                        invoice = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceNumber == idAsString);
                    else
                    {
                        id = Convert.ToInt32(idAsString);
                        invoice = Invoice.OpenInvoices.ToList().FirstOrDefault(x => x.InvoiceId == id);
                    }
                    if (invoice != null)
                    {
                        if (string.IsNullOrEmpty(RealInvoicesId))
                            RealInvoicesId = invoice.InvoiceNumber;
                        else
                            RealInvoicesId += ", " + invoice.InvoiceNumber;
                    }
                }
            }

            var orders = order.Orders();
            if (orders != null && orders.Count > 0)
            {
                foreach (var o in orders)
                {
                    if (string.IsNullOrEmpty(RealInvoicesId))
                        RealInvoicesId = o.PrintedOrderId;
                    else
                        RealInvoicesId += ", " + o.PrintedOrderId;

                }
            }

            AddCellToBody(tableLayout, RealInvoicesId);

            doc.Add(tableLayout);
        }

        protected virtual void AddPaymentDetailsTable(Document doc, InvoicePayment order)
        {
            float[] headers = { 25, 25, 25, 25 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Payment Method:");
            AddCellToHeader(tableLayout, "Ref #:");
            AddCellToHeader(tableLayout, "Comments");
            AddCellToHeader(tableLayout, "Amount");

            if (order.Components != null)
            {
                // DETAILS
                foreach (var detail in order.Components)
                {

                    AddCellToBody(tableLayout, detail.PaymentMethod.ToString().Replace("_", " "));

                    AddCellToBody(tableLayout, detail.Ref);
                    AddCellToBody(tableLayout, detail.Comments);
                    AddCellToBody(tableLayout, detail.Amount.ToCustomString());
                }
            }

            doc.Add(tableLayout);
        }

        protected virtual void AddPaymentFooterTable(Document doc, InvoicePayment order)
        {
            float[] headers = { 75, 25 };

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToBody(tableLayout, "Total");

            AddCellToBody(tableLayout, order.TotalPaid.ToCustomString());

            doc.Add(tableLayout);
        }

        public string GetPaymentPdf(InvoicePayment payment)
        {
            string pdfPath = string.Empty;

            string name = string.Format("Payment{0}.pdf", payment.Id);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);

            AddGoalContentToPdf(doc, payment);

            doc.Close();

            return targetFile;
        }

        public string GetDepositPdf(BankDeposit bankDeposit)
        {
            return string.Empty;
        }

        public string GetReportPdf()
        {
            return string.Empty;
        }

        public string GetStatementReportPdf(Client client)
        {
            return string.Empty;
        }

        #endregion


        #endregion

        protected class BatteryItem
        {
            public Product Product { get; set; }
            public float Qty { get; set; }
            public double Price { get; set; }
            public double Total
            {
                get
                {
                    return double.Parse(Math.Round(Convert.ToDecimal(Price * Qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                }
            }
            public int Age { get; set; }
            public List<BatteryItem> Warranties { get; set; }
            public float PickedCores { get; set; }
        }
    }
}