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

using iText.IO.Font.Constants;
using static LaceupMigration.DefaultPdfProvider;

namespace LaceupMigration
{
    public class AlwaysShowHeaderPdfGenerator : IPdfProvider
    {
        #region General

        protected virtual void AddCompanyInfo(Document doc)
        {

            CompanyInfo company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, company);
                return;
            }
            string currentDate = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");
            AddTextLine(doc, currentDate, GetNormalFont());
            AddTextLine(doc, company.CompanyName != null ? company.CompanyName : string.Empty, GetBigFont());
            AddTextLine(doc, company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = DataAccess.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }

        protected virtual void AddCompanyInfoWithLogo(Document doc, CompanyInfo company)
        {
            try
            {
                Image jpg = null;
                if (!string.IsNullOrEmpty(company.CompanyLogoPath))
                    jpg = new Image(ImageDataFactory.Create(company.CompanyLogoPath));
                else
                    jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));


                jpg.ScaleToFit(90f, 75f);
                jpg.SetPaddingLeft(9f);

                doc.Add(jpg);
            }
            catch
            {

            }
           

            AddTextLine(doc, "   " + company.CompanyName, GetBigFont());
            AddTextLine(doc, "   " + company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, "   " + company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, "   " + phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = DataAccess.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }

        protected virtual void AddOrderClientInfo(Document doc, Client client, bool isQuote = false)
        {
            if (client == null)
                return;

            float[] headers = { 15, 35, 15, 35 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            var soldTo = client.ClientName + "\n";

            var shipTo = client.ClientName + "\n";

            if (!string.IsNullOrEmpty(client.ContactName) && !Config.HideContactName)
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

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                soldTo += "Email:" + client.ContactPhone;
                shipTo += "Email:" + client.ContactPhone;
            }

            var soldToLabel = "Sold To:";
            if (isQuote)
                soldToLabel = "Customer:";

            AddCellToBody(tableLayout, soldToLabel, HorizontalAlignment.RIGHT, Border.NO_BORDER);
            AddCellToBody(tableLayout, soldTo, HorizontalAlignment.LEFT, Border.NO_BORDER);

            AddCellToBody(tableLayout, "Ship To:", HorizontalAlignment.RIGHT, Border.NO_BORDER);
            AddCellToBody(tableLayout, shipTo, HorizontalAlignment.LEFT, Border.NO_BORDER);

            doc.Add(tableLayout);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderClientInfo(Document doc, Client client, Style fontSize, bool isQuote = false)
        {
            if (client == null)
                return;

            float[] headers = { 15, 35, 15, 35 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            var soldTo = client.ClientName + "\n";

            var shipTo = client.ClientName + "\n";

            if (!string.IsNullOrEmpty(client.ContactName) && !Config.HideContactName)
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

            if (!string.IsNullOrEmpty(client.ContactPhone))
            {
                soldTo += "Email:" + client.ContactPhone;
                shipTo += "Email:" + client.ContactPhone;
            }

            var soldToLabel = "Sold To:";
            if (isQuote)
                soldToLabel = "Customer:";

            AddCellToBody(tableLayout, soldToLabel, HorizontalAlignment.RIGHT, Border.NO_BORDER, fontSize);
            AddCellToBody(tableLayout, soldTo, HorizontalAlignment.LEFT, Border.NO_BORDER, fontSize);

            AddCellToBody(tableLayout, "Ship To:", HorizontalAlignment.RIGHT, Border.NO_BORDER, fontSize);
            AddCellToBody(tableLayout, shipTo, HorizontalAlignment.LEFT, Border.NO_BORDER, fontSize);

            doc.Add(tableLayout);

            AddTextLine(doc, "\n", GetNormalFont());
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

        protected virtual void AddCellToHeader(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8WhiteFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetNormalFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border, Style textSize)
        {
            var style = GetNormalFont();
            if (textSize != null)
                style = textSize;

            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(style);

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetPadding(5);
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

        protected virtual Cell GetSignatureCell(Invoice invoice)
        {
            if (!string.IsNullOrEmpty(invoice.SignatureAsBase64))
            {
                Byte[] bytes = Convert.FromBase64String(invoice.SignatureAsBase64);
                Image jpg = new Image(ImageDataFactory.Create(bytes));

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

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdfdoc = new PdfDocument(writer);
            Document doc = new Document(pdfdoc);

            AddContentToPDF(doc, invoice);

            doc.Close();

            return targetFile;
        }

        protected virtual void AddContentToPDF(Document doc, Invoice invoice)
        {
            AddCompanyInfo(doc);

            AddOrderInfo(doc, invoice);

            AddOrderClientInfo(doc, invoice.Client);

            AddOrderHeaderTable(doc, invoice);

            AddOrderDetailsTable(doc, invoice);

            AddFooterTable(doc, invoice);
        }

        protected virtual void AddOrderInfo(Document doc, Invoice invoice)
        {
            AddTextLine(doc, "Invoice", GetBigFont(), HorizontalAlignment.CENTER);
            AddTextLine(doc, "Invoice #:" + invoice.InvoiceNumber, GetNormalFont(), HorizontalAlignment.CENTER);

            if (!string.IsNullOrEmpty(invoice.PONumber))
                AddTextLine(doc, "PO#:" + invoice.PONumber, GetNormalFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Invoice invoice)
        {
            //Create PDF Table
            float[] headers = { 25, 25, 25, 25 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Email");
            AddCellToHeader(tableLayout, "Date");
            AddCellToHeader(tableLayout, "Invoiced Date");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;
            var name = salesman != null ? salesman.Name : "";
            if (!string.IsNullOrEmpty(invoice.SalesmanName))
                name = invoice.SalesmanName;

            AddCellToBody(tableLayout, name);

            string terms = "";
            if (invoice.Client.ExtraProperties != null)
            {
                var termsExtra = invoice.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            AddCellToBody(tableLayout, terms);

            AddCellToBody(tableLayout, invoice.Date.ToShortDateString());
            AddCellToBody(tableLayout, invoice.Date.ToShortTimeString());

            doc.Add(tableLayout);
        }

        protected virtual void AddOrderDetailsTable(Document doc, Invoice invoice)
        {
            //Create PDF Table
            float[] headers = { 15, 15, 30, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Item Id");
            AddCellToHeader(tableLayout, "UPC");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Extended");

            float qtyBoxes = 0;

            if (invoice.Details != null)
            {
                // DETAILS
                foreach (InvoiceDetail detail in invoice.Details)
                {
                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                    AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    AddCellToBody(tableLayout, name);

                    AddCellToBody(tableLayout, detail.Quantity.ToString());

                    if (!Config.HidePriceInTransaction)
                    {
                        AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                        AddCellToBody(tableLayout, (detail.Quantity * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);
                    }
                    else
                    {
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                    }

                    qtyBoxes += Convert.ToSingle(detail.Quantity);
                }
            }

            doc.Add(tableLayout);
        }

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

            tableLayout.AddCell(GetSignatureCell(invoice));

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

            Cell text = new Cell();
            var pp1 = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
            pp1.AddStyle(GetSmall8Font());
            text.Add(pp1);
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            footer.AddCell(text);

            doc.Add(footer);
        }

        public string GetInvoicesPdf(List<Invoice> invoices)
        {
            string name = string.Format("invoice group {0}.pdf", Guid.NewGuid().ToString());
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);
            Document doc = new Document(pdf);

            int i = 0;

            foreach (var invoice in invoices)
            {
                AddContentToPDF(doc, invoice);

                if (i != invoices.Count - 1)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                i++;
            }

            doc.Close();

            return targetFile;
        }

        #endregion

        #region Order

        public virtual string GetOrderPdf(Order order)
        {
            var filename = "order {0}.pdf";

            //add filename to order
            if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Credit)
            {
                if (order.AsPresale)
                {

                    switch (order.OrderType)
                    {
                        case OrderType.Order:
                            filename = "order {0}.pdf";
                            break;
                        case OrderType.Credit:
                            filename = "credit {0}.pdf";
                            break;
                    }
                }
                else
                {
                    switch (order.OrderType)
                    {
                        case OrderType.Order:
                            filename = "Invoice {0}.pdf";
                            break;
                        case OrderType.Credit:
                            filename = "Credit Invoice {0}.pdf";
                            break;
                    }
                }
            }
            else
            {
                switch (order.OrderType)
                {
                    case OrderType.Quote:
                        filename = "Quote {0}.pdf";
                        break;
                    case OrderType.NoService:
                        filename = "No Service {0}.pdf";
                        break;
                    case OrderType.Consignment:
                        filename = "Consignment {0}.pdf";
                        break;
                }
            }

            string name = string.Format(filename, order.PrintedOrderId);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);

            AddContentToPDF(doc, order);

            doc.Close();

            return targetFile;
        }

        public void AddPageHeaderForOrder(Document doc, Order order)
        {
            AddCompanyInfo(doc);

            AddOrderInfo(doc, order);

            AddOrderClientInfo(doc, order.Client, GetSmall8Font(), Config.UseQuote && order.IsQuote);

            AddOrderHeaderTable(doc, order);

            AddTextLine(doc, " ", GetSmallFont());
        }

        public int GetTotalPages(Order order)
        {
            int toReturn = 1;
            if (order.Details != null)
            {
                var groupedDetails = GetGroups(order.Details.ToList(), 8);

                var lastGroup = groupedDetails.LastOrDefault();

                toReturn = groupedDetails.Count();
                if (lastGroup != null && lastGroup.Count() > 3)
                {
                    toReturn += 1;
                }
            }

            return toReturn;
        }

        static IEnumerable<IEnumerable<T>> GetGroups<T>(IEnumerable<T> e, int groupSize)
        {
            if (!e.Any()) return new IEnumerable<T>[0];
            var g = new[] { e.Take(groupSize) };
            var r = e.Skip(groupSize);
            return g.Concat(GetGroups(r, groupSize));
        }



        protected virtual void AddContentToPDF(Document doc, Order order)
        {
            var TotalPages = GetTotalPages(order);
            AddTextLine(doc, "1/" + TotalPages, GetNormalFont(), HorizontalAlignment.RIGHT);

            AddPageHeaderForOrder(doc, order);

            AddOrderDetailsTable(doc, order, TotalPages);

            AddFooterTable(doc, order);

            if (Config.TotalsByUoMInPdf)
                AddUoMTotalsTable(doc, order);
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

            if (((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0))
            {
                var site = SiteEx.Sites.FirstOrDefault(x => x.Id == Config.SalesmanSelectedSite);
                if (site != null)
                {
                    AddTextLine(doc, "Site: " + site.Name, GetBigFont(), HorizontalAlignment.CENTER);
                }
            }

            if (!string.IsNullOrEmpty(docNum))
                AddTextLine(doc, docNum, GetNormalFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Order order)
        {
            //Create PDF Table
            float[] headers = { 20, 20, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Purchase Order");
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Email");

            var dateLabel = order.AsPresale ? "Date" : "Date";
            var timeLabel = order.AsPresale ? "Time" : "Invoiced Date";

            AddCellToHeader(tableLayout, dateLabel);
            AddCellToHeader(tableLayout, timeLabel);

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId) : null;

            if (string.IsNullOrEmpty(order.PONumber) && Config.AutoGeneratePO)
            {
                string po = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");
                order.PONumber = po;
            }

            AddCellToBody(tableLayout, order.PONumber ?? "");
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

        protected virtual void AddOrderDetailsTable(Document doc, Order order, int totalPages)
        {
            float[] headers = { 15, 15, 30, 10, 10, 10, 10 };  //Header Widths

            List<Table> tables = new List<Table>();

            Dictionary<int, List<OrderDetail>> groupedDetails = new Dictionary<int, List<OrderDetail>>();

            if (order.Details != null)
            {
                var index = 0;

                foreach (var detail in SortDetails.SortedDetails(order.Details))
                {
                    if (groupedDetails.Count == 0)
                    {
                        groupedDetails.Add(index, new List<OrderDetail>() { detail });
                        continue;
                    }

                    if (groupedDetails[index].Count < 8)
                    {
                        groupedDetails[index].Add(detail);
                    }
                    else
                    {
                        index++;
                        groupedDetails.Add(index, new List<OrderDetail>() { detail });
                    }
                }
            }

            if (groupedDetails.Count > 0)
            {
                foreach (var g in groupedDetails)
                {
                    //Create PDF Table
                    var tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

                    AddCellToHeader(tableLayout, "Item Id");
                    AddCellToHeader(tableLayout, "UPC");
                    AddCellToHeader(tableLayout, "Description");
                    AddCellToHeader(tableLayout, "Qty");
                    AddCellToHeader(tableLayout, "UoM");
                    AddCellToHeader(tableLayout, "Unit Price");
                    AddCellToHeader(tableLayout, "Extended");

                    float qtyBoxes = 0;

                    // DETAILS
                    foreach (var detail in g.Value)
                    {
                        Product product = detail.Product;

                        AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                        AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");
                        var name = product != null ? product.Name : "";

                        if (!string.IsNullOrEmpty(detail.Comments))
                            name += "\n" + "Comment:" + detail.Comments;

                        // Verifica si el producto no es nulo, el código no está vacío, y el name contiene el código
                        if (product != null && !string.IsNullOrEmpty(product.Code) && name.Contains(product.Code))
                        {
                            // Encuentra la primera ocurrencia del código en el name y lo elimina
                            int codeIndex = name.IndexOf(product.Code);
                            if (codeIndex >= 0)
                            {
                                name = name.Remove(codeIndex, product.Code.Length).Trim();
                            }
                        }

                        if (name.Length > 30)
                            name = name.Substring(0, 30);

                        AddCellToBody(tableLayout, name + "\n" + detail.Lot);
                        AddCellToBody(tableLayout, detail.Qty.ToString());

                        AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : string.Empty);

                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);

                        string diff = "";
                        if (detail.IsCredit)
                            diff += "-";

                        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
                        else
                            AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

                        if (detail.UnitOfMeasure != null)
                            qtyBoxes += detail.Qty * detail.UnitOfMeasure.Conversion;
                        else
                            qtyBoxes += detail.Qty;
                    }


                    tables.Add(tableLayout);
                }

                var index = 0;
                foreach (var table in tables)
                {
                    if (index > 0)
                    {
                        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        AddTextLine(doc, (index + 1) + "/" + totalPages, GetNormalFont(), HorizontalAlignment.RIGHT);
                        AddPageHeaderForOrder(doc, order);
                    }

                    doc.Add(table);
                    index++;


                    if (index == tables.Count && groupedDetails[index - 1].Count > 3)
                    {
                        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        AddTextLine(doc, (index + 1) + "/" + totalPages, GetNormalFont(), HorizontalAlignment.RIGHT);
                        AddPageHeaderForOrder(doc, order);
                    }
                }

                return;
            }

            ////Create PDF Table
            //Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //AddCellToHeader(tableLayout, "Item Id");
            //AddCellToHeader(tableLayout, "UPC");
            //AddCellToHeader(tableLayout, "Description");
            //AddCellToHeader(tableLayout, "Qty");
            //AddCellToHeader(tableLayout, "UoM");
            //AddCellToHeader(tableLayout, "Unit Price");
            //AddCellToHeader(tableLayout, "Extended");

            //float qtyBoxes = 0;

            //if (order.Details != null)
            //{
            //    // DETAILS
            //    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
            //    {
            //        Product product = detail.Product;

            //        AddCellToBody(tableLayout, (product != null) ? product.Code : "");
            //        AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");

            //        var name = product != null ? product.Name : "";

            //        if (!string.IsNullOrEmpty(detail.Comments))
            //            name += "\n" + "Comment:" + detail.Comments;

            //        AddCellToBody(tableLayout, name);
            //        AddCellToBody(tableLayout, detail.Qty.ToString());

            //        AddCellToBody(tableLayout, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : string.Empty);

            //        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
            //            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            //        else
            //            AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);

            //        string diff = "";
            //        if (detail.IsCredit)
            //            diff += "-";

            //        if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
            //            AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            //        else
            //            AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

            //        if (detail.UnitOfMeasure != null)
            //            qtyBoxes += detail.Qty * detail.UnitOfMeasure.Conversion;
            //        else
            //            qtyBoxes += detail.Qty;
            //    }
            //}

            //doc.Add(tableLayout);
        }

        protected virtual void AddFooterTable(Document doc, Order order)
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

            //#0012619
            AddCellToHeader(tableLayout, string.Empty);
            AddCellToBody(tableLayout, string.Empty);

            AddCellToHeader(tableLayout, "Subtotal");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, order.CalculateItemCost().ToCustomString(), HorizontalAlignment.RIGHT);

            tableLayout.AddCell(GetSignatureCell(order));

            AddCellToHeader(tableLayout, "Discount");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, order.CalculateDiscount().ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Taxes");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, order.CalculateTax().ToCustomString(), HorizontalAlignment.RIGHT);

            var totalLabel = order.AsPresale ? "Total" : "Total Invoice";

            AddCellToHeader(tableLayout, totalLabel);
            var total = order.OrderTotalCost();

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Payment");
            var payment = GetPayment(order);

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT);

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

            Cell celltext = new Cell();
            var ttt = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
            ttt.AddStyle(GetSmall8Font());
            ttt.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            celltext.Add(ttt);

            footer.AddCell(celltext);

            doc.Add(footer);
        }


        public virtual string GetOrdersPdf(List<Order> orders)
        {
            string ordersId = string.Empty;

            foreach (var o in orders)
            {
                if (string.IsNullOrEmpty(ordersId))
                    ordersId = o.PrintedOrderId;
                else
                    ordersId += ", " + o.PrintedOrderId;
            }

            string name = string.Format("order group {0}.pdf", Guid.NewGuid().ToString());

            if (orders.Count == 1)
            {
                name = string.Format("Invoice {0}.pdf", orders.FirstOrDefault().PrintedOrderId);
            }
            else
            {
                name = string.Format("Invoice Group ({0}).pdf", ordersId.ToString());
            }

            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(new FileInfo(targetFile));
            PdfDocument pdf = new PdfDocument(writer);
            Document doc = new Document(pdf);

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

            AddCompanyInfo(doc);

            AddOrderInfo(doc, order, counting);

            AddOrderClientInfo(doc, order.Client);

            AddOrderHeaderTable(doc, order);

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

            Cell text = new Cell();
            var texttext = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
            texttext.AddStyle(GetSmall8Font());
            text.Add(texttext);
            texttext.SetHorizontalAlignment(HorizontalAlignment.LEFT);

            footer.AddCell(text);

            doc.Add(footer);
        }

        #endregion

        #region Consignment Par

        private void AddContentToPDFConsPar(Document doc, Order order)
        {
            var structList = new List<ConsStruct>();
            foreach (var item in order.Details)
                structList.Add(ConsStruct.GetStructFromDetail(item));

            AddCompanyInfo(doc);

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

            Cell text = new Cell();
            var textext = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
            textext.AddStyle(GetSmall8Font());
            text.Add(textext);
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            footer.AddCell(text);

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
            AddCompanyInfo(doc);

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

        #endregion

        #region goal
        protected virtual void AddGoalContentToPdf(Document doc, GoalProgressDTO goal)
        {
            AddCompanyInfo(doc);

            AddGoalInfo(doc, goal);

            AddGoalHeaderTable(doc, goal);

            AddEmptySpace(1, doc);

            AddGoalDetailsTable(doc, goal);

            AddGoalFooterTable(doc, goal);
        }

        protected virtual void AddGoalInfo(Document doc, GoalProgressDTO order)
        {
            string docName = "Goal Progress: " + order.Name;

            AddTextLine(doc, docName, GetBigFont(), HorizontalAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddGoalHeaderTable(Document doc, GoalProgressDTO order)
        {
            float[] headers = { 40, 30, 30 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesman");
            AddCellToHeader(tableLayout, "Start Date:");
            AddCellToHeader(tableLayout, "End Date:");

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            AddCellToBody(tableLayout, salesman != null ? salesman.Name : "");
            AddCellToBody(tableLayout, order.StartDate.ToString());
            AddCellToBody(tableLayout, order.EndDate.ToString());

            doc.Add(tableLayout);
        }

        protected virtual void AddGoalDetailsTable(Document doc, GoalProgressDTO order)
        {
            float[] headers = { 40, 15, 15, 15, 15 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "Product");
            AddCellToHeader(tableLayout, "Goal Amount");
            AddCellToHeader(tableLayout, "Sold");
            AddCellToHeader(tableLayout, "Missing To Goal");
            AddCellToHeader(tableLayout, "Daily Sales To Goal");

            if (order.Details != null)
            {
                // DETAILS
                foreach (var detail in order.Details)
                {
                    var product = Product.Find(detail.ProductId ?? 0);

                    string cellHeader = product != null ? product.Name : detail.Name;
                    cellHeader = cellHeader;

                    // checkBox.Checked = false;
                    var amount = detail.QuantityOrAmountValue.ToString();
                    var sold = detail.SoldValue.ToString();

                    var missingQty = (detail.QuantityOrAmountValue - detail.SoldValue);


                    AddCellToBody(tableLayout, cellHeader);

                    AddCellToBody(tableLayout, amount);
                    AddCellToBody(tableLayout, sold);
                    AddCellToBody(tableLayout, missingQty < 0 ? (0).ToString() : missingQty.ToString());
                    AddCellToBody(tableLayout, detail.DailySalesToGoal.ToString());
                }
            }

            doc.Add(tableLayout);
        }

        protected virtual void AddGoalFooterTable(Document doc, GoalProgressDTO order)
        {
            float[] headers = { 40, 15, 15, 15, 15 };

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            double totalAmount = 0;
            double totalSold = 0;
            double totalMissing = 0;
            double totalDaily = 0;

            foreach (var detail in order.Details)
            {
                totalAmount += detail.QuantityOrAmountValue;
                totalSold += detail.SoldValue;

                var missingQty = (detail.QuantityOrAmountValue - detail.SoldValue);
                if (missingQty > 0)
                    totalMissing += missingQty;

                totalDaily += detail.DailySalesToGoal;
            }

            AddCellToBody(tableLayout, "Total");
            AddCellToBody(tableLayout, totalAmount.ToString());
            AddCellToBody(tableLayout, totalSold.ToString());
            AddCellToBody(tableLayout, totalMissing.ToString());
            AddCellToBody(tableLayout, totalDaily.ToString());

            doc.Add(tableLayout);

        }

        public string GetGoalPdf(GoalProgressDTO goal)
        {
            string name = string.Format("Goal Progress {0}.pdf", goal.Id);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);

            AddGoalContentToPdf(doc, goal);

            doc.Close();

            return targetFile;
        }

        #endregion

        #region Payment

        protected virtual void AddGoalContentToPdf(Document doc, InvoicePayment payment)
        {
            AddCompanyInfo(doc);

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;
            AddTextLine(doc, "Salesman: " + (salesman != null ? salesman.Name : (Config.VendorName ?? "")), GetNormalFont(), HorizontalAlignment.LEFT);

            AddPaymentInfo(doc, payment);

            if (payment.Client != null)
                AddOrderClientInfo(doc, payment.Client, false);

            AddPaymentHeaderTablePanamerican(doc, payment);

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
        public double totalamount;
        protected virtual void AddPaymentHeaderTablePanamerican(Document doc, InvoicePayment order)
        {
            // Crea una tabla con dos columnas: Invoice Number y Amount
            float[] headers = { 50, 50 };  // Anchuras de las columnas al 50%

            // Crea la tabla PDF
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            // Agrega los encabezados a la tabla
            AddCellToHeader(tableLayout, "Invoice:"); // "Invoice"
            AddCellToHeader(tableLayout, "Amount"); // "Amount"

            // Variable para calcular el total de las facturas
            double invoicesTotalAmount = 0;

            // Obtiene las facturas y añade una fila para cada una con su número y monto
            var invoices = order.Invoices();
            if (invoices != null)
            {
                foreach (var invoice in invoices)
                {
                    AddCellToBody(tableLayout, invoice.InvoiceNumber); // Número de factura
                    AddCellToBody(tableLayout, invoice.Balance.ToCustomString()); // Monto de factura
                    invoicesTotalAmount += invoice.Balance; // Suma al total
                }
            }

            // Añade la tabla al documento
            doc.Add(tableLayout);
            double paymentTotal = order.TotalPaid; // Total pagado
            double openBalance = invoicesTotalAmount - paymentTotal; // Balance abierto
            string formattedOpenBalance = openBalance.ToCustomString(); // Balance abierto formateado como moneda
            totalamount = invoicesTotalAmount;
            // Crea una tabla para los totales y balances
            float[] totalHeaders = { 50, 50 }; // Anchuras de las columnas al 50%
            Table totalsTable = new Table(UnitValue.CreatePercentArray(totalHeaders)).UseAllAvailableWidth();

            // Añade los totales y balances a la tabla
            AddCellToBody(totalsTable, "Amount"); // "Total Invoices Amount" podría necesitar ser obtenido desde resources si es localizado
            AddCellToBody(totalsTable, invoicesTotalAmount.ToCustomString());
            AddCellToBody(totalsTable, "Payment Total"); // "Payment Total" podría necesitar ser obtenido desde resources si es localizado
            AddCellToBody(totalsTable, order.TotalPaid.ToCustomString());
            AddCellToBody(totalsTable, "Balance"); // "Balance" podría necesitar ser obtenido desde resources si es localizado
            AddCellToBody(totalsTable, formattedOpenBalance);

            // Añade la tabla de totales al documento
            doc.Add(totalsTable);
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

        #endregion



        #region deposit

        protected virtual void AddDepositCompanyInfoWithLogo(Document doc, CompanyInfo company)
        {
            try
            {
                Image jpg = null;
                if (!string.IsNullOrEmpty(company.CompanyLogoPath))
                    jpg = new Image(ImageDataFactory.Create(company.CompanyLogoPath));
                else
                    jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));


                jpg.ScaleToFit(90f, 75f);
                jpg.SetPaddingLeft(9f);

                doc.Add(jpg);
            }
            catch
            {

            }
            

            var companyText = string.Empty;

            if (!string.IsNullOrEmpty(company.CompanyName))
                companyText = company.CompanyName + "\n";

            companyText += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                companyText += company.CompanyAddress2 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyPhone))
                companyText += ("Email:" + company.CompanyPhone) + "\n";

            if (!string.IsNullOrEmpty(company.CompanyFax))
                companyText += ("Fax:" + company.CompanyFax) + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = DataAccess.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    companyText += ("TIN:" + extra) + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                companyText += ("Email:" + company.CompanyEmail) + "\n";

            AddTextLine(doc, companyText, GetNormalFont());
        }
        protected virtual void AddDepositCompanyInfo(Document doc)
        {
            CompanyInfo company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddDepositCompanyInfoWithLogo(doc, company);
                return;
            }

            var companyText = string.Empty;

            if (!string.IsNullOrEmpty(company.CompanyName))
                companyText = company.CompanyName + "\n";

            companyText += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                companyText += company.CompanyAddress2 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyPhone))
                companyText += ("Email:" + company.CompanyPhone) + "\n";

            if (!string.IsNullOrEmpty(company.CompanyFax))
                companyText += ("Fax:" + company.CompanyFax) + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = DataAccess.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    companyText += ("TIN:" + extra) + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                companyText += ("Email:" + company.CompanyEmail) + "\n";

            AddTextLine(doc, companyText, GetNormalFont());
        }


        public string GetDepositPdf(BankDeposit bankDeposit)
        {
            string pdfPath = string.Empty;

            string name = string.Format("bankDeposit{0}.pdf", bankDeposit.UniqueId);
            string filePath = name;
            string fullPath = System.IO.Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter writer = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(writer);

            Document doc = new Document(pdf);

            AddDepositContentToPdf(doc, bankDeposit);

            doc.Close();

            return targetFile;
        }

        protected virtual void AddDepositContentToPdf(Document doc, BankDeposit deposit)
        {
            AddDepositCompanyInfo(doc);

            AddDepositInfo(doc, deposit);

            var checks = new List<PaymentComponent>();
            var cash = new List<PaymentComponent>();
            var credit_card = new List<PaymentComponent>();
            var moneyOrder = new List<PaymentComponent>();
            var transfer = new List<PaymentComponent>();
            var zelle = new List<PaymentComponent>();

            //fill lists
            foreach (var p in deposit.Payments)
            {
                foreach (var c in p.Components)
                {
                    switch (c.PaymentMethod)
                    {
                        case InvoicePaymentMethod.Check:
                            if (!checks.Contains(c))
                                checks.Add(c);
                            break;
                        case InvoicePaymentMethod.Cash:
                            if (!cash.Contains(c))
                                cash.Add(c);
                            break;
                        case InvoicePaymentMethod.Credit_Card:
                            if (!credit_card.Contains(c))
                                credit_card.Add(c);
                            break;
                        case InvoicePaymentMethod.Money_Order:
                            if (!moneyOrder.Contains(c))
                                moneyOrder.Add(c);
                            break;
                        case InvoicePaymentMethod.Transfer:
                            if (!transfer.Contains(c))
                                transfer.Add(c);
                            break;
                        case InvoicePaymentMethod.Zelle_Transfer:
                            if (!zelle.Contains(c))
                                zelle.Add(c);
                            break;
                    }
                }
            }

            AddOrderHeaderTablePan(doc, deposit, deposit.Payments);

            AddTextLine(doc, "\n", GetNormalFont());

            var footerText = string.Empty;

            footerText = "Total Cash: " + cash.Sum(x => x.Amount).ToCustomString();
            footerText += "\n" + "Total Check: " + checks.Sum(x => x.Amount).ToCustomString();
            footerText += "\n" + "Total Credit Card: " + credit_card.Sum(x => x.Amount).ToCustomString();
            footerText += "\n" + "Total Money Order: " + moneyOrder.Sum(x => x.Amount).ToCustomString();
            footerText += "\n" + "Total Transfer: " + transfer.Sum(x => x.Amount).ToCustomString();
            footerText += "\n" + "Total Zelle Transfer: " + zelle.Sum(x => x.Amount).ToCustomString();

            AddTextLine(doc, footerText, GetNormalFont());

            AddTextLine(doc, "Total Deposit: " + deposit.TotalAmount.ToCustomString(), GetBigFont());

            if (!string.IsNullOrEmpty(deposit.Comment))
                AddTextLine(doc, "Comments:" + deposit.Comment, GetNormalFont());

            AddTextLine(doc, "\n", GetNormalFont());

            var signatureText = string.Empty;
            signatureText = "------------------------------";
            signatureText += "\n" + "Signature";
            AddTextLine(doc, signatureText, GetNormalFont());
        }

        protected virtual void AddDepositHeaderTable(Document doc, BankDeposit order, List<InvoicePayment> payments)
        {
            AddTextLine(doc, "Documents", GetBigFont());

            float[] headers = { 40, 15, 15, 15, 15 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //PdfHelper.AddCellToHeader(tableLayout, LocalizationExtensions.GetString("Purchase Order"));
            AddCellToHeader(tableLayout, "CUSTOMER");
            AddCellToHeader(tableLayout, "DOC NUMBER");
            AddCellToHeader(tableLayout, "METHOD");
            AddCellToHeader(tableLayout, "CHECK #");
            AddCellToHeader(tableLayout, "TOTAL");

            foreach (var p in payments)
            {
                string invoicesId = string.Empty;
                var invoices = p.Invoices();
                if (!string.IsNullOrEmpty(p.InvoicesId) && invoices != null)
                {
                    foreach (var i in invoices)
                    {
                        if (string.IsNullOrEmpty(invoicesId))
                            invoicesId = i.InvoiceNumber;
                        else
                            invoicesId += "," + i.InvoiceNumber;
                    }
                }
                else
                {
                    var orders = p.Orders();
                    if (orders != null)
                    {
                        foreach (var o in orders)
                        {
                            if (string.IsNullOrEmpty(invoicesId))
                                invoicesId = o.PrintedOrderId;
                            else
                                invoicesId += "," + o.PrintedOrderId;
                        }
                    }
                    else
                        invoicesId = p.OrderId;
                }

                if (invoicesId.Length > 20)
                    invoicesId = invoicesId.Substring(0, 20) + "...";

                foreach (var c in p.Components)
                {
                    AddCellToBody(tableLayout, p.Client.ClientName);
                    AddCellToBody(tableLayout, invoicesId);
                    AddCellToBody(tableLayout, c.PaymentMethod.ToString().Replace("_", " "));
                    AddCellToBody(tableLayout, c.Ref);
                    AddCellToBody(tableLayout, c.Amount.ToCustomString());
                }
            }

            AddCellToBody(tableLayout, "");
            AddCellToBody(tableLayout, "");
            AddCellToBody(tableLayout, "");
            AddCellToBody(tableLayout, "TOTAL");
            AddCellToBody(tableLayout, payments.Sum(x => x.Components.Sum(x => x.Amount)).ToCustomString());

            doc.Add(tableLayout);
        }

        public void AddOrderHeaderTablePan(Document doc, BankDeposit order, List<InvoicePayment> payments)
        {
            AddTextLine(doc, "DEPOSIT PAYMENT", GetBigFont(), HorizontalAlignment.CENTER);
            AddTextLine(doc, Environment.NewLine, GetBigFont());


            // Group payments by client
            var groupedPayments = payments
                .GroupBy(p => p.Client)
                .Select(g => new DefaultPdfProvider.T1
                {
                    Client = g.Key,
                    Payments = g.ToList()
                });


            #region 1st Table


            //Create PDF Table

            float[] headers1 = { 40, 12, 12, 12, 12, 12 };  //Header Widths
            Table tableLayout1 = new Table(UnitValue.CreatePercentArray(headers1)).UseAllAvailableWidth();

            //Set the PDF File witdh percentage

            AddCellToHeader(tableLayout1, "CUSTOMER");
            AddCellToHeader(tableLayout1, "DOC#");
            AddCellToHeader(tableLayout1, "METHOD");
            AddCellToHeader(tableLayout1, "AMOUNT");
            AddCellToHeader(tableLayout1, "PAID");
            AddCellToHeader(tableLayout1, "BALANCE");

            foreach (var clientPayments in groupedPayments)
            {
                bool isFirstTime = true;
                foreach (var payment in clientPayments.Payments)
                {
                    var components = payment.Components.OrderByDescending(x => x.Amount);

                    var componentList = new List<DefaultPdfProvider.T4>();

                    foreach (var c in components)
                        componentList.Add(new DefaultPdfProvider.T4() { AmountRemaining = c.Amount, component = c });

                    var invoices = payment.Invoices().OrderBy(x => x.DueDate);
                    foreach (var i in invoices)
                    {

                        double amountToSubstract = i.Balance;

                        var listOfComponentsUsed = new List<DefaultPdfProvider.T4>();

                        foreach (var c in componentList.Where(x => x.AmountRemaining > 0))
                        {
                            if (c.AmountRemaining > amountToSubstract)
                            {
                                c.AmountRemaining -= amountToSubstract;
                                listOfComponentsUsed.Add(new DefaultPdfProvider.T4() { AmountRemaining = amountToSubstract, component = c.component });
                                break;
                            }
                            else
                            {
                                amountToSubstract -= c.AmountRemaining;
                                listOfComponentsUsed.Add(new DefaultPdfProvider.T4() { AmountRemaining = c.AmountRemaining, component = c.component });
                                c.AmountRemaining = 0;
                            }

                            if (amountToSubstract == 0)
                                break;
                        }

                        if (isFirstTime)
                        {

                            AddCellToBody(tableLayout1, clientPayments.Client.ClientName);
                            AddCellToBody(tableLayout1, i.InvoiceNumber);

                            string method = string.Empty;
                            string paid = string.Empty;


                            foreach (var c in listOfComponentsUsed)
                            {
                                var extra = string.Empty;

                                if (c.component.PaymentMethod == InvoicePaymentMethod.Check)
                                    extra = "\n" + "No." + c.component.Ref;

                                if (!string.IsNullOrEmpty(method))
                                    method += "\n" + c.component.PaymentMethod.ToString().Replace("_", " ") + extra;
                                else
                                    method = c.component.PaymentMethod.ToString().Replace("_", " ") + extra;


                                if (!string.IsNullOrEmpty(paid))
                                    paid += "\n" + c.AmountRemaining.ToCustomString();
                                else
                                    paid = c.AmountRemaining.ToCustomString();
                            }

                            double b = i.Balance - listOfComponentsUsed.Sum(x => x.AmountRemaining);

                            AddCellToBody(tableLayout1, method);
                            AddCellToBody(tableLayout1, i.Balance.ToCustomString());

                            AddCellToBody(tableLayout1, paid);
                            AddCellToBody(tableLayout1, b.ToCustomString());

                            isFirstTime = false;
                        }
                        else
                        {
                            AddCellToBody(tableLayout1, "");
                            AddCellToBody(tableLayout1, i.InvoiceNumber);
                            string method = string.Empty;
                            string paid = string.Empty;

                            foreach (var c in listOfComponentsUsed)
                            {
                                var extra = string.Empty;

                                if (c.component.PaymentMethod == InvoicePaymentMethod.Check)
                                    extra = "\n" + "No." + c.component.Ref;

                                if (!string.IsNullOrEmpty(method))
                                    method += "\n" + c.component.PaymentMethod.ToString().Replace("_", " ") + extra;
                                else
                                    method = c.component.PaymentMethod.ToString().Replace("_", " ") + extra;


                                if (!string.IsNullOrEmpty(paid))
                                    paid += "\n" + c.AmountRemaining.ToCustomString();
                                else
                                    paid = c.AmountRemaining.ToCustomString();
                            }

                            double b = i.Balance - listOfComponentsUsed.Sum(x => x.AmountRemaining);

                            AddCellToBody(tableLayout1, method);
                            AddCellToBody(tableLayout1, i.Balance.ToCustomString());


                            AddCellToBody(tableLayout1, paid);
                            AddCellToBody(tableLayout1, b.ToCustomString());
                        }
                    }
                }

                var totalBalanceInvoices = clientPayments.Payments.Sum(x => x.Invoices().Sum(y => y.Balance));
                var totalPaid = clientPayments.Payments.Sum(x => x.TotalPaid);
                var balance = totalBalanceInvoices - totalPaid;


                AddCellToHeader(tableLayout1, "");
                AddCellToHeader(tableLayout1, "");
                AddCellToHeader(tableLayout1, "TOTALS");
                AddCellToHeader(tableLayout1, totalBalanceInvoices.ToCustomString());
                AddCellToHeader(tableLayout1, totalPaid.ToCustomString());
                AddCellToHeader(tableLayout1, balance.ToCustomString());
            }

            doc.Add(tableLayout1);

            #endregion


        }


        protected virtual void AddDepositInfo(Document doc, BankDeposit order)
        {
            //string docName = "PAYMENT DEPOSIT";

            //AddTextLine(doc, docName, GetBigFont(), HorizontalAlignment.CENTER);

            var batchNumber = order.BatchNumber;
            if (!string.IsNullOrEmpty(batchNumber))
                AddTextLine(doc, "Batch #: " + batchNumber, GetNormalFont());

            string docInfotext = string.Empty;

            var bank = BankAccount.List.FirstOrDefault(x => x.Id == order.bankAccountId);
            if (bank != null)
                docInfotext = ("Bank:" + bank.Name) + "\n";

            docInfotext += ("Posted Date: " + order.PostedDate.ToShortDateString());
            docInfotext += "\n" + ("Printed Date: " + DateTime.Now.ToString("MM/dd/yy hh:mm tt"));

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            docInfotext += "\n" + ("Salesman name:" + (salesman != null ? salesman.Name : ""));

            AddTextLine(doc, docInfotext, GetNormalFont());
        }

        public string GetReportPdf()
        {
            return "";
        }
        public string GetStatementReportPdf(Client client)
        {
            string appPath = Config.BasePath;;
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"ReportStatement_{timestamp}.pdf";
            string filePath = System.IO.Path.Combine(appPath, fileName);

            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    using (Document doc = new Document(pdf))
                    {
                        // Calcula los totales aquí, antes de llamar al método que agrega contenido al PDF
                        var openInvoices = GetOpenInvoices(client);
                        double current = 0, due1_30 = 0, due31_60 = 0, due61_90 = 0, over90 = 0;
                        CalculateTotals(openInvoices, ref current, ref due1_30, ref due31_60, ref due61_90, ref over90);

                        // Ahora que tienes los totales, puedes añadir el contenido al PDF
                        AddContentReportClientStatementToPDF(doc, client, current, due1_30, due31_60, due61_90, over90);
                    }
                }
            }
            return filePath;
        }

        private List<Invoice> GetOpenInvoices(Client client)
        {
            return Invoice.OpenInvoices
                .Where(i => i.Client != null && i.Client.ClientId == client.ClientId && i.Balance != 0)
                .OrderByDescending(i => i.Date)
                .ToList();
        }

        // Método para calcular los totales
        private void CalculateTotals(List<Invoice> openInvoices, ref double current, ref double due1_30, ref double due31_60, ref double due61_90, ref double over90)
        {
            foreach (var invoice in openInvoices)
            {
                if (invoice.InvoiceType == 2 || invoice.InvoiceType == 3)
                    continue;

                //int factor = invoice.InvoiceType == 1 ? -1 : 1;

                int daysOverdue = (DateTime.Now - invoice.DueDate).Days;

                current += (invoice.Balance);

                if (daysOverdue > 0 && daysOverdue < 31)
                {
                    due1_30 += invoice.Balance;
                }
                else if (daysOverdue > 30 && daysOverdue < 61)
                {
                    due31_60 += invoice.Balance;
                }
                else if (daysOverdue > 60 && daysOverdue < 91)
                {
                    due61_90 += invoice.Balance;
                }
                else
                if (daysOverdue > 90)
                {
                    over90 += invoice.Balance;
                }
            }
        }

        protected virtual void AddContentReportClientStatementToPDF(Document doc, Client client, double current, double due1_30, double due31_60, double due61_90, double over90)
        {
            AddCompanyInfo(doc);
            AddClientStatementHeaderToPDF(doc, client);
            AddClientStatementTableToPDF(doc, client);
            AddClientStatementTotalsToPDF(doc, client, current, due1_30, due31_60, due61_90, over90);
        }

        protected virtual void AddClientStatementTableToPDF(Document doc, Client client)
        {
            var openInvoices = (from i in Invoice.OpenInvoices
                                where i.Client != null && i.Client.ClientId == client.ClientId && i.Balance != 0
                                orderby i.Date ascending
                                select i).ToList();


            // Define los anchos de las columnas de la tabla
            float[] columnWidths = { 20, 20, 20, 20, 20, 20 };

            // Crea la tabla con los anchos de columna definidos
            Table tableLayout = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

            // Agrega el encabezado a la tabla
            AddCellToHeader(tableLayout, "Invoice Type");
            AddCellToHeader(tableLayout, "Date");
            AddCellToHeader(tableLayout, "Invoice Number");
            AddCellToHeader(tableLayout, "Due Date");
            AddCellToHeader(tableLayout, "Amount");
            AddCellToHeader(tableLayout, "Balance");
            // Agrega más títulos de columnas según sea necesario

            // Agrega las filas de datos a la tabla
            foreach (var item in openInvoices)
            {
                // Salta los tipos de factura específicos si es necesario
                if (item.InvoiceType == 2 || item.InvoiceType == 3)
                    continue;

                AddCellToBody(tableLayout, GetClientStatementInvoiceType(item.InvoiceType));
                AddCellToBody(tableLayout, item.Date.ToShortDateString());
                AddCellToBody(tableLayout, item.InvoiceNumber);
                AddCellToBody(tableLayout, item.DueDate.ToShortDateString());
                AddCellToBody(tableLayout, item.Amount.ToCustomString());
                AddCellToBody(tableLayout, item.Balance.ToCustomString());

            }



            // Agrega la tabla al documento PDF
            doc.Add(tableLayout);

        }
        protected virtual string GetClientStatementInvoiceType(int invoiceType)
        {
            switch (invoiceType)
            {
                case 0:
                    return "Invoice";
                case 1:
                    return "Credit";
                case 2:
                    return "Quote";
                default:
                    return "Invoice";
            }
        }

        protected virtual void AddClientStatementTotalsToPDF(Document doc, Client client, double current, double due1_30, double due31_60, double due61_90, double over90)
        {
            var amountdue = due1_30 + due31_60 + due61_90 + over90;
            AddTextLine(doc, "Current: " + current.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            // Agregar las filas de totales a la tabla
            AddTextLine(doc, "1-30 Past Due: " + due1_30.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "31-60 Past Due: " + due31_60.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "61-90 Past Due: " + due61_90.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "Over 90 Past Due: " + over90.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "Amount Due: " + amountdue.ToCustomString(), GetNormalFont(), TextAlignment.LEFT);
            // Agrega la tabla de totales al documento PDF

        }


        protected virtual void AddClientStatementHeaderToPDF(Document doc, Client client)
        {
            AddTextLine(doc, client.ClientName.ToUpperInvariant(), GetNormalBoldFont(), TextAlignment.LEFT);

            // Asumiendo que la dirección completa es una sola cadena y que cada parte está separada por '|'.
            // Aquí se separa la dirección en sus componentes.
            string[] addressParts = client.BillToAddress.Split('|');

            // La primera parte es la dirección de la calle.
            string streetAddress = addressParts.Length > 0 ? addressParts[0] : string.Empty;

            string ad1 = addressParts.Length > 1 ? addressParts[1] : string.Empty;
            string ad2 = addressParts.Length > 2 ? addressParts[2] : string.Empty;
            string ad3 = addressParts.Length > 3 ? addressParts[3] : string.Empty;
            string ad4 = addressParts.Length > 4 ? addressParts[4] : string.Empty;

            string cityStateZip = addressParts.Length > 3
                ? string.Join(" ", ad1, ad2, ad3, ad4)
                : string.Join(" ", addressParts.Skip(1));  // Usa lo que está disponible si hay menos de cinco partes.

            // Agrega la dirección de la calle y la ciudad, estado, código postal en líneas separadas.
            AddTextLine(doc, streetAddress, GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, cityStateZip, GetNormalFont(), TextAlignment.LEFT);
            // Agrega el número de licencia y el número de vendedor si están presentes.
            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                AddTextLine(doc, client.LicenceNumber, GetNormalFont(), TextAlignment.LEFT);
            }
            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                AddTextLine(doc, client.VendorNumber, GetNormalFont(), TextAlignment.LEFT);
            }

            // Agrega términos extras si están presentes.
            if (client.ExtraProperties != null)
            {
                var termsExtra = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null && !string.IsNullOrEmpty(termsExtra.Item2))
                {
                    AddTextLine(doc, termsExtra.Item2, GetNormalFont(), TextAlignment.LEFT);
                }
            }

            // Añadir TEST si es necesario.
            AddTextLine(doc, "Customer Open Balance", GetNormalBoldFont(), TextAlignment.LEFT);
        }


        protected virtual void AddTextLine(Document doc, string text, Style style, TextAlignment alignment)
        {
            Paragraph pdfTxet = new Paragraph(text);
            pdfTxet.AddStyle(style);
            pdfTxet.SetTextAlignment(alignment);

            doc.Add(pdfTxet);
        }

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