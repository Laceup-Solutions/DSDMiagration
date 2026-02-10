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





using iText.Kernel.Geom;


namespace LaceupMigration
{
    public class DefaultPdfProvider : IPdfProvider
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

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Phone:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Email:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }


            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }

        protected virtual void AddCompanyInfo(Document doc, Order order)
        {

            CompanyInfo company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
            if (company == null)
                company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.Companies.FirstOrDefault();

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, order, company);
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
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }

        protected virtual void AddCompanyInfoWithLogo(Document doc, Order order, CompanyInfo company)
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

            string textToAdd = string.Empty;

            string currentDate = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");
            textToAdd += "Printed Date: " + currentDate + "\n";
            AddTextLine(doc, textToAdd, GetNormalFont());
            textToAdd = string.Empty;

            textToAdd += (company.CompanyName != null ? company.CompanyName : string.Empty) + "\n";
            AddTextLine(doc, textToAdd, GetBigFont());
            textToAdd = string.Empty;

            textToAdd += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                textToAdd += company.CompanyAddress2 + "\n";

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                textToAdd += phoneLine + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    textToAdd += "TIN:" + extra + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                textToAdd += "Email" + company.CompanyEmail + "\n";

            AddTextLine(doc, textToAdd, GetNormalFont());
        }

        protected virtual void AddNoBorderHeaderToBodyBold(Table tableLayout, string cellText, HorizontalAlignment alignment, bool bold = true)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetTitleBoldFontHelvetica(bold));

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }


        protected virtual Style GetTitleBoldFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetTitleBoldFontHelvetica(bool bold = true)
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(bold ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
        protected virtual Style GetSmallFontHelvetica(bool bold = true)
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(bold ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(6);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
        protected virtual Style GetMediumFontHelvetica(bool bold = true)
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(bold ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
        protected virtual void AddCompanyInfodate(Document doc, InvoicePayment order)
        {

            CompanyInfo company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, company);
                return;
            }

            string textToAdd = string.Empty;

            string currentDate = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");

            textToAdd += "Created Date: " + order.DateCreated.ToString("MMM dd, yyyy h:mm tt") + "\n";
            textToAdd += "Printed Date: " + currentDate + "\n";
            AddTextLine(doc, textToAdd, GetNormalFont());
            textToAdd = string.Empty;

            textToAdd += (company.CompanyName != null ? company.CompanyName : string.Empty) + "\n";
            AddTextLine(doc, textToAdd, GetBigFont());
            textToAdd = string.Empty;

            textToAdd += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                textToAdd += company.CompanyAddress2 + "\n";

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                textToAdd += phoneLine + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    textToAdd += "TIN:" + extra + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                textToAdd += "Email:" + company.CompanyEmail + "\n";

            AddTextLine(doc, textToAdd, GetNormalFont());

        }
        protected virtual void AddCompanysLogo(Document doc)
        {
            try
            {
                CompanyInfo company = CompanyInfo.SelectedCompany;

                if (company == null)
                    company = CompanyInfo.Companies[0];
                Image jpg = null;
                if (!string.IsNullOrEmpty(company.CompanyLogoPath))
                    jpg = new Image(ImageDataFactory.Create(company.CompanyLogoPath));
                else
                    jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));


                jpg.ScaleToFit(90f, 75f);
                jpg.SetPaddingLeft(9f);

                doc.Add(jpg);


            }
            catch (Exception ex)
            {
                //no image

            }


        }
        protected virtual void AddCompanysInfo(Document doc)
        {

            CompanyInfo company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.GetMasterCompany();


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
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
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

            string textToAdd = string.Empty;

            string currentDate = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");
            textToAdd += "Printed Date: " + currentDate + "\n";
            AddTextLine(doc, textToAdd, GetNormalFont());
            textToAdd = string.Empty;

            textToAdd += (company.CompanyName != null ? company.CompanyName : string.Empty) + "\n";
            AddTextLine(doc, textToAdd, GetBigFont());
            textToAdd = string.Empty;

            textToAdd += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                textToAdd += company.CompanyAddress2 + "\n";

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                textToAdd += phoneLine + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    textToAdd += "TIN:" + extra + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                textToAdd += "Email:" + company.CompanyEmail + "\n";

            AddTextLine(doc, textToAdd, GetNormalFont());
        }

       protected virtual void AddOrderClientInfo(Document doc, Client client, bool isQuote = false, Order order = null)
        {
            if (client == null)
                return;

            float[] headers = { 15, 35, 15, 35 };//Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            var soldTo = client.ClientName + "\n";

            var shipTo = client.ClientName + "\n";

            if (!string.IsNullOrEmpty(client.ContactName) && !Config.HideContactName)
            {
                soldTo += client.ContactName + "\n";
                shipTo += client.ContactName + "\n";
            }

            foreach (var item in ClientAddress(client, false, order))
                if (!string.IsNullOrEmpty(item))
                    soldTo += item + "\n";

            foreach (var item in ClientAddress(client, true, order))
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
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(6);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
        protected virtual Style GetSmallTable8Font()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
            style.SetFont(font);
            style.SetFontSize(8);
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
        protected virtual Style GetBigFontHelvetica(bool bold = true)
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(bold ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);
            style.SetFont(font);
            style.SetFontSize(12);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
       
        protected virtual Style GetNormalFontHelvetica(bool bold = true)
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(bold ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);
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

        protected virtual void AddTextLine(Document doc, string text, Style style, TextAlignment alignment = TextAlignment.LEFT)
        {
            Paragraph pdfTxet = new Paragraph(text);
            pdfTxet.AddStyle(style);
            pdfTxet.SetTextAlignment(alignment);

            doc.Add(pdfTxet);
        }
        // Método para agregar texto con márgenes personalizados
        protected virtual void AddTextToColumn(Div column, string text, Style style, TextAlignment alignment = TextAlignment.LEFT)
        {
            Paragraph paragraph = new Paragraph(text);
            paragraph.AddStyle(style);
            paragraph.SetTextAlignment(alignment);
            column.Add(paragraph);
        }

        protected virtual void AddCellToHeader(Table tableLayout, string cellText, int alignment = 0)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.SetTextAlignment((TextAlignment)alignment);
            paragraph.AddStyle(GetSmall8WhiteFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }
        protected virtual void AddCellToBodyFont(Table tableLayout, string cellText, Style style)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmallTable8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBodyTab(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetBigFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);
            cell.SetBorder(Border.NO_BORDER);
            tableLayout.AddCell(cell);
        }

        public virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border, Style style)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(style);

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(border);
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

        protected virtual string[] ClientAddress(Client client, bool shipTo = true, Order order = null)
        {
            var shiptoText = client.ShipToAddress;
            if (order != null && !string.IsNullOrEmpty(order.ExtraFields) && order.ExtraFields.Contains("selectedshipto"))
                shiptoText = UDFHelper.GetSingleUDF("selectedshipto", order.ExtraFields);
            
            var addr = shipTo ? shiptoText : client.BillToAddress;

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
            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
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

        protected virtual Cell GetSignatureCell(Order order, int rowSpan = 4)
        {
            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                var imgPath = GetSignatureImage(order);

                Image jpg = new Image(ImageDataFactory.Create(imgPath));
                jpg.ScaleToFit(75f, 75f);

                Cell img = new Cell(rowSpan, 3);
                img.Add(jpg);
                img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                img.SetVerticalAlignment(VerticalAlignment.MIDDLE);

                return img;
            }

            Cell empty = new Cell(rowSpan, 3);
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
            AddTextLine(doc, "Invoice", GetBigFont(), TextAlignment.CENTER);
            AddTextLine(doc, "Invoice #:" + invoice.InvoiceNumber, GetNormalFont(), TextAlignment.CENTER);

            if (!string.IsNullOrEmpty(invoice.PONumber))
                AddTextLine(doc, "PO#:" + invoice.PONumber, GetNormalFont(), TextAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Invoice invoice)
        {
            //Create PDF Table
            float[] headers = { 25, 25, 25, 25 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Salesperson");
            AddCellToHeader(tableLayout, "Terms");
            AddCellToHeader(tableLayout, "Invoice Date");
            AddCellToHeader(tableLayout, "Invoice Time");

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

        protected virtual void AddContentToPDF(Document doc, Order order)
        {

            if (order.IsWorkOrder)
                AddCompanyInfoWo(doc, order);
            else
                AddCompanyInfo(doc, order);

            if (order != null && order.DepartmentId > 0)
            {
                var dep = DepartmertClientCategory.List.FirstOrDefault(x => x.Id == order.DepartmentId);

                if (dep != null)
                    AddTextLine(doc, "Department:" + dep.Name, GetNormalFont());
            }

            AddOrderInfo(doc, order);

            AddOrderClientInfo(doc, order.Client, Config.UseQuote && order.IsQuote);

            AddOrderHeaderTable(doc, order);

            AddTextLine(doc, " ", GetSmallFont());

            AddOrderDetailsTable(doc, order);

            AddFooterTable(doc, order);

            if (Config.TotalsByUoMInPdf)
                AddUoMTotalsTable(doc, order);
        }

        protected virtual void AddCompanyInfoWo(Document doc, Order order)
        {
            CompanyInfo company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
            if (company == null)
                company = CompanyInfo.SelectedCompany;
            
            if (company == null)
                company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, company);
                return;
            }

            string currentDate = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");

            AddTextLine(doc, currentDate, GetNormalFont(), TextAlignment.RIGHT);
            AddTextLine(doc, company.CompanyName != null ? company.CompanyName : string.Empty, GetBigFont());
            AddTextLine(doc, company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : company.CompanyPhone;
            //phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, "Phone: " + phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }


        class UomT
        {
            public UnitOfMeasure Unit { get; set; }
            public float Qty { get; set; }
        }

        public void AddUoMTotalsTable(Document doc, Order order)
        {
            Dictionary<string, float> lines = new Dictionary<string, float>();
            float totalUnits = 0;
            foreach (var item in order.Details)
            {
                if (item.Qty == 0)
                    continue;

                var name = item.UnitOfMeasure != null ? item.UnitOfMeasure.Name : "";
                float conv = item.UnitOfMeasure != null ? item.UnitOfMeasure.Conversion : 1;

                string georgehoweValue = UDFHelper.GetSingleUDF("georgehowe", item.UnitOfMeasure.ExtraFields);
                if (int.TryParse(georgehoweValue, out int conversionfactor))
                {
                    totalUnits += item.Qty * conversionfactor;
                }
                else
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
                        docName = "Invoice";
                        docNum = "Invoice #:" + order.PrintedOrderId;
                    }
                }
            }

            AddTextLine(doc, docName, GetBigFont(), TextAlignment.CENTER);

            if (((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0))
            {
                var site = SiteEx.Sites.FirstOrDefault(x => x.Id == Config.SalesmanSelectedSite);
                if (site != null)
                {
                    AddTextLine(doc, "Site: " + site.Name, GetBigFont(), TextAlignment.CENTER);
                }
            }

            if (!string.IsNullOrEmpty(docNum))
                AddTextLine(doc, docNum, GetNormalFont(), TextAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }

        protected virtual void AddOrderHeaderTable(Document doc, Order order)
        {
            //Create PDF Table
            float[] headers = { 20, 20, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Add header
            AddCellToHeader(tableLayout, "Purchase Order");
            AddCellToHeader(tableLayout, "Salesperson");
            AddCellToHeader(tableLayout, "Terms");
            AddCellToHeader(tableLayout, "Invoice Date");
            AddCellToHeader(tableLayout, "Invoice Time");

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

        protected virtual void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 10, 15, 30, 10, 10, 10, 10, 10, 10 };  //Header Widths

            //Create PDF Table
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

            if (order.IsWorkOrder)
            {
                headers = new float[] { 15, 40, 15, 15, 15 }; //Header Widths

                tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

                var assetName = string.Empty;
                var assetPart = string.Empty;

                if (order.IsDelivery)
                {
                    assetName = order.Comments;
                }
                else
                {
                    var asset = UDFHelper.GetSingleUDF("workOrderAsset", order.ExtraFields);
                    if (!string.IsNullOrEmpty(asset))
                    {
                        var assetProduct = Asset.Find(asset);

                        var product = Product.Find(assetProduct.ProductId);

                        assetName = "Asset Name: " + (product != null ? product.Name : "");
                        assetPart = "      Part Number: " + asset;
                    }
                }

                Cell spacer = new Cell(1, 5);
                spacer.SetBorder(Border.NO_BORDER);
                tableLayout.AddCell(spacer);
                Cell productHeaderCell = new Cell(1, 5);
                productHeaderCell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                productHeaderCell.SetBorder(Border.NO_BORDER);
                productHeaderCell.SetPadding(5);
                productHeaderCell.Add(new Paragraph(assetName + assetPart));

                tableLayout.AddCell(productHeaderCell);

                List<int> serviceProducts = Product.Products.Where(p => Category.Categories
                .Any(c => c.TypeServiPart == CategoryServiPartType.Services && c.CategoryId == p.CategoryId)).Select(p => p.ProductId).ToList();

                bool hasRelevantServicesDetails = false;

                if (order.Details != null)
                {
                    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details.ToList()))
                    {
                        if (serviceProducts.Contains(detail.Product.ProductId))
                        {
                            hasRelevantServicesDetails = true;
                            break;
                        }
                    }
                }

                if (hasRelevantServicesDetails)
                {
                    Cell productHeaderCell1 = new Cell(1, 5);
                    productHeaderCell1.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    productHeaderCell1.SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    productHeaderCell1.SetBorder(Border.NO_BORDER);
                    productHeaderCell1.SetPadding(5);

                    var paragraph = new Paragraph("Operations & Services");
                    paragraph.SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    paragraph.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    paragraph.SetTextAlignment(TextAlignment.CENTER);

                    productHeaderCell1.Add(paragraph);

                    tableLayout.AddCell(productHeaderCell1);
                }

                AddCellToHeader(tableLayout, "UPC");
                AddCellToHeader(tableLayout, "Description");
                AddCellToHeader(tableLayout, "Quantity");
                AddCellToHeader(tableLayout, "Unit Price");
                AddCellToHeader(tableLayout, "Amount");


                if (hasRelevantServicesDetails)
                {
                    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details.ToList()))
                    {
                        if (serviceProducts.Contains(detail.Product.ProductId))
                        {
                            Product product = detail.Product;

                            AddCellToBody(tableLayout, (product != null) ? product.Upc : "");

                            var name = product != null ? product.Name : "";

                            AddCellToBody(tableLayout, name);
                            AddCellToBody(tableLayout, detail.Qty.ToString());
                            AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                            AddCellToBody(tableLayout, (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

                            qtyBoxes += detail.Qty;
                        }
                    }
                }

                List<int> partProducts = Product.Products.Where(p => Category.Categories
                .Any(c => c.TypeServiPart == CategoryServiPartType.Part && c.CategoryId == p.CategoryId)).Select(p => p.ProductId).ToList();

                bool hasRelevantPartDetails = false;

                if (order.Details != null)
                {
                    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details.ToList()))
                    {
                        if (partProducts.Contains(detail.Product.ProductId))
                        {
                            hasRelevantPartDetails = true;
                            break;
                        }
                    }
                }

                if (hasRelevantPartDetails)
                {
                    Cell productHeaderCell2 = new Cell(1, 5);
                    productHeaderCell2.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    productHeaderCell2.SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    productHeaderCell2.SetBorder(Border.NO_BORDER);
                    productHeaderCell2.SetPadding(5);

                    var paragraph = new Paragraph("Product Sales");
                    paragraph.SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    paragraph.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    paragraph.SetTextAlignment(TextAlignment.CENTER);

                    productHeaderCell2.Add(paragraph);

                    tableLayout.AddCell(productHeaderCell2);

                }

                if (hasRelevantPartDetails)
                {
                    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details.ToList()))
                    {
                        if (partProducts.Contains(detail.Product.ProductId))
                        {
                            Product product = detail.Product;

                            AddCellToBody(tableLayout, (product != null) ? product.Code : "");

                            var name = product != null ? product.Name : "";

                            AddCellToBody(tableLayout, name);
                            AddCellToBody(tableLayout, detail.Qty.ToString());
                            AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                            AddCellToBody(tableLayout, (detail.Qty * detail.Price).ToCustomString(), HorizontalAlignment.RIGHT);

                            qtyBoxes += detail.Qty;
                        }
                    }
                }
            }
            else
            {


                if (order.Details != null)
                {
                    // DETAILS
                    foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
                    {
                        if (detail.OrderDiscountId > 0)
                            continue;

                        Product product = detail.Product;

                        AddCellToBody(tableLayout, (product != null) ? product.Code : "");
                        AddCellToBody(tableLayout, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "");

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
            }

            doc.Add(tableLayout);
        }

        protected virtual void AddFooterTable(Document doc, Order order)
        {
            float[] headers = { 30, (float)22.65, (float)17.35, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            var text = new Paragraph("Customer Signature");
            text.AddStyle(GetSmall8Font());
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            signature.Add(text);

            tableLayout.AddCell(signature);

            /*AddCellToHeader(tableLayout, "Total Units");
            AddCellToBody(tableLayout, order.Details.Sum(x => x.UnitOfMeasure != null ? x.Qty * x.UnitOfMeasure.Conversion : x.Qty).ToString());*/

            double discount = 0;
            foreach (var d in order.Details)
            {
                if (d.Product.IsDiscountItem)
                    continue;

                if (d.Discount > 0 || d.IsCredit)
                    continue;

                bool cameFromOffer = false;
                var price = Product.CalculatePriceForProduct(d.Product, order.Client, d.IsCredit, d.Damaged, d.UnitOfMeasure, false, out cameFromOffer, true, order);
                var t = price * d.Qty;
                var dc = (t - d.QtyPrice);
                if (dc > 0)
                    discount += dc;
            }

            //#0012619
            AddCellToHeader(tableLayout, "Total Qty:", (int)TextAlignment.RIGHT);
            AddCellToBody(tableLayout, order.CalculateOrderTotalItems().ToString());

            AddCellToHeader(tableLayout, "Subtotal");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, (discount + order.CalculateItemCost()).ToCustomString(), HorizontalAlignment.RIGHT);

            int rowSpan = (order.IsWorkOrder || Config.AllowOtherCharges) ? 5 : 4;

            tableLayout.AddCell(GetSignatureCell(order, rowSpan));

            AddCellToHeader(tableLayout, "Discount");

            if (Config.HidePriceInSelfService && (Config.SelfServiceUser || Config.SelfService) || Config.HidePriceInTransaction)
                AddCellToBody(tableLayout, string.Empty, HorizontalAlignment.RIGHT);
            else
                AddCellToBody(tableLayout, (discount + order.CalculateDiscount()).ToCustomString(), HorizontalAlignment.RIGHT);

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

            var file = new FileInfo(targetFile);
            
            PdfWriter writer = new PdfWriter(file);
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
                AddTextLine(doc, "CONSIGNMENT CONTRACT", GetBigFont(), TextAlignment.CENTER);
                AddTextLine(doc, "Invoice #:" + order.PrintedOrderId, GetNormalFont(), TextAlignment.CENTER);
            }
            else
            {
                if (order.AsPresale)
                {
                    AddTextLine(doc, "SALES ORDER", GetBigFont(), TextAlignment.CENTER);
                    AddTextLine(doc, "Order # " + order.PrintedOrderId, GetNormalFont(), TextAlignment.CENTER);
                }
                else
                {
                    AddTextLine(doc, "Invoice", GetBigFont(), TextAlignment.CENTER);
                    AddTextLine(doc, "Invoice #:" + order.PrintedOrderId, GetNormalFont(), TextAlignment.CENTER);
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
            AddCellToHeader(tableLayout, "Salesperson");
            AddCellToHeader(tableLayout, "Terms");
            AddCellToHeader(tableLayout, "Invoice Date");
            AddCellToHeader(tableLayout, "Invoice Time");

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
            var core = UDFHelper.GetSingleUDF("coreQty", detail.ExtraFields);
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
            var rotation = UDFHelper.GetSingleUDF("rotatedQty", detail.ExtraFields);

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
            var adjQty = UDFHelper.GetSingleUDF("adjustedQty", detail.ExtraFields);

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

            AddTextLine(doc, docName, GetBigFont(), TextAlignment.CENTER);

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

            AddTextLine(doc, docName, GetBigFont(), TextAlignment.CENTER);

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
            AddCompanyInfodate(doc, payment);

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;
            AddTextLine(doc, "Salesman: " + (salesman != null ? salesman.Name : (Config.VendorName ?? "")), GetNormalFont(), TextAlignment.LEFT);

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

            AddTextLine(doc, docName, GetBigFont(), TextAlignment.CENTER);

            AddTextLine(doc, "\n", GetNormalFont());
        }


        public string GenerateUnifiedPaymentReport(Document doc, Client client)
        {
            var paymentsForClient = InvoicePayment.List
                              .Where(payment => payment.Client.ClientId == client.ClientId)
                              .ToList();

            string pdfPath = string.Empty;



            foreach (var payment in paymentsForClient)
            {
                AddGoalContentToPdf(doc, payment);
            }



            doc.Close();

            return pdfPath; // Retorna la rutael archivo PDF
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

        public double totalamount;
        public double ValueAmount;
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

            var orders = order.Orders();
            if (orders != null)
            {
                foreach (var invoice in orders)
                {
                    AddCellToBody(tableLayout, invoice.PrintedOrderId); // Número de factura
                    AddCellToBody(tableLayout, invoice.OrderTotalCost().ToCustomString()); // Monto de factura
                    invoicesTotalAmount += invoice.OrderTotalCost(); // Suma al total
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
                    AddCellToBody(tableLayout, detail.Amount.ToString());
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

        #region View Reports
        public virtual string GetReportPdf()

        {
            string appPath = Config.BasePath;
            // Utiliza DateTime.Now para obtener un timestamp único y así evitar sobrescribir archivos.
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"ReportEndOfDay_{timestamp}.pdf";
            string filePath = System.IO.Path.Combine(appPath, fileName);

            // No es necesario comprobar si el archivo existe ya que el nombre será único.
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    using (Document doc = new Document(pdf))
                    {
                        InventorySettlementRow totalRow = new InventorySettlementRow();

                        List<InventorySettlementRow> map = new List<InventorySettlementRow>();
                        bool fromEOD = true; // or false, depending on the logic
                        int index = 0; // example index, you should provide the actual value needed for your logic
                        int count = 0; // example count, you should provide the actual value needed for your logic
                        bool isReport = false;

                        CreateSettlementReportDataStructure(ref totalRow, ref map);

                        AddContentReportToPDF(doc, map, totalRow, fromEOD, index, count, isReport);
                    }
                }

                return filePath;
            }
        }

        protected virtual void AddContentReportToPDF(Document doc, List<InventorySettlementRow> map, InventorySettlementRow totalRow, bool FromEOD, int index = 0, int count = 0, bool isReport = false)
        {
            var vehicleInfo = VehicleInformation.CurrentVehicleInformation;
            var endingVehicleInfo = VehicleInformation.EODVehicleInformation;

            bool showVehicleInfo = (FromEOD && endingVehicleInfo != null) || (!FromEOD && vehicleInfo != null);
            int totalReports = 2;
            if (showVehicleInfo) { totalReports++; }// Si hay información del vehículo, incrementa a 3
            if (map.Count > 0) { totalReports++; }


            #region sales register


            AddTextLine(doc, $"1/{totalReports}", GetNormalFont(), TextAlignment.RIGHT);
            AddCompanysLogo(doc);
            AddTextLine(doc, "Sales Register Report", GetBigFont(), TextAlignment.LEFT);
            AddTextLine(doc, "Date: " + DateTime.Now.ToShortDateString(), GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "Route: " + Config.RouteName, GetNormalFont(), TextAlignment.LEFT);
            AddTextLine(doc, "Salesman: " + Config.VendorName, GetNormalFont(), TextAlignment.LEFT);

            // Agregar información de la empresa si es necesario
            AddCompanysInfo(doc);

            AddOrderCreatedReportTableToPDF(doc);

            var signaturesText = string.Empty;
            signaturesText = "------------------------------";
            signaturesText += "\n" + "Signature";
            AddTextLine(doc, signaturesText, GetNormalFont());

            #endregion


            int currentPage = 2;
            var signatureText = string.Empty;

            #region Payment report

            var listOfPayments = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;

            if (listOfPayments.Count > 0)
            {
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                ////payment
                ///

                AddTextLine(doc, $"{currentPage}/{totalReports}", GetNormalFont(), TextAlignment.RIGHT);
                // Incrementa el contador de página actual después de cada informe
                currentPage++;
                double totalCash = 0;
                double totalCheck = 0;
                double totalcc = 0;
                double totalmo = 0;
                double totaltr = 0;
                double total = 0;

                AddCompanysLogo(doc);

                AddTextLine(doc, "Received Payment Report", GetBigFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Date:" + DateTime.Now.ToShortDateString(), GetNormalFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Route:" + Config.RouteName, GetNormalFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Salesman:" + Config.VendorName, GetNormalFont(), TextAlignment.LEFT);

                AddCompanysInfo(doc);

                var rows = CreatePaymentReceivedDataStructure(ref totalCash, ref totalCheck, ref totalcc, ref totalmo, ref totaltr, ref total);

                float[] headers = { 20, 10, 20, 20, 20, 10 };  //Header Widths

                Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

                //Add header
                AddCellToHeader(tableLayout, "Client Name");
                AddCellToHeader(tableLayout, "Doc. Number:");
                AddCellToHeader(tableLayout, "Amount");
                AddCellToHeader(tableLayout, "Paid");
                AddCellToHeader(tableLayout, "Payment Method:");
                AddCellToHeader(tableLayout, "Ref #:");

                foreach (var row in rows)
                {
                    AddCellToBody(tableLayout, row.ClientName);
                    AddCellToBody(tableLayout, row.DocNumber);
                    AddCellToBody(tableLayout, "$" + row.DocAmount);
                    AddCellToBody(tableLayout, row.Paid);
                    AddCellToBody(tableLayout, row.PaymentMethod);
                    AddCellToBody(tableLayout, row.RefNumber);
                }

                doc.Add(tableLayout);
                Table paymentTable = new Table(2); // Asumiendo que tienes 2 columnas.
                paymentTable.SetMaxWidth(100); // Aumenta el ancho máximo para dar más espacio al contenido. Ajusta según sea necesario.

                float fontSize = 10.0f;


                if (totalCash > 0)
                {
                    paymentTable.AddCell(new Cell().Add(new Paragraph("Cash:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                    paymentTable.AddCell(new Cell().Add(new Paragraph(totalCash.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                }

                if (totalCheck > 0)
                {
                    paymentTable.AddCell(new Cell().Add(new Paragraph("Check:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                    paymentTable.AddCell(new Cell().Add(new Paragraph(totalCheck.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                }

                if (totalcc > 0)
                {
                    paymentTable.AddCell(new Cell().Add(new Paragraph("Credit Card:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                    paymentTable.AddCell(new Cell().Add(new Paragraph(totalcc.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                }

                if (totalmo > 0)
                {
                    paymentTable.AddCell(new Cell().Add(new Paragraph("Money Order:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                    paymentTable.AddCell(new Cell().Add(new Paragraph(totalmo.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                }

                if (totaltr > 0)
                {
                    paymentTable.AddCell(new Cell().Add(new Paragraph("Transfer:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                    paymentTable.AddCell(new Cell().Add(new Paragraph(totaltr.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                }

                paymentTable.AddCell(new Cell().Add(new Paragraph("Total:").SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph(total.ToCustomString()).SetFontSize(fontSize)).SetBorder(Border.NO_BORDER));

                // Añade la tabla al documento.
                doc.Add(paymentTable);



                AddTextLine(doc, "\n", GetNormalFont());

                signatureText = "------------------------------";
                signatureText += "\n" + "Payment Recived By";
                AddTextLine(doc, signatureText, GetNormalFont());
            }

            #endregion


            #region Inventory settlement

            if (map.Count > 0)
            {


                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                AddTextLine(doc, $"{currentPage}/{totalReports}", GetNormalFont(), TextAlignment.RIGHT);
                // Incrementa el contador de página actual después de cada informe
                currentPage++;

                AddCompanysLogo(doc);
                AddTextLine(doc, "Settlement Report", GetBigFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Date:" + DateTime.Now.ToShortDateString(), GetNormalFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Route:" + Config.RouteName, GetNormalFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Salesman:" + Config.VendorName, GetNormalFont(), TextAlignment.LEFT);
                AddCompanysInfo(doc);

                AddSettlementReportTableToPDF(doc, map, totalRow);

                var driver = string.Empty;
                driver = "------------------------------";
                driver += "\n" + "Driver Signature";
                AddTextLine(doc, signatureText, GetNormalFont());

                AddTextLine(doc, "\n", GetNormalFont());
                signatureText = "------------------------------";
                signatureText += "\n" + "Signature";
                AddTextLine(doc, signatureText, GetNormalFont());
                // ... Resto del código para esta sección ...
            }



            #endregion


            if (showVehicleInfo)
            {

                // Incrementa el contador de página actual después de cada informe

                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                AddTextLine(doc, $"{currentPage}/{totalReports}", GetNormalFont(), TextAlignment.RIGHT);
                // Llamar al método para agregar la información del vehículo
                ReportVehicleInformationPdf(doc, FromEOD, index, count, isReport);
                AddTextLine(doc, "\n", GetNormalFont());
                signatureText = "------------------------------";
                signatureText += "\n" + "Signature";
                AddTextLine(doc, signatureText, GetNormalFont());
            }
            //


        }
        protected List<PaymentSplit> GetPaymentsForOrderCreatedReport()
        {
            List<PaymentSplit> result = new List<PaymentSplit>();

            foreach (var payment in InvoicePayment.List)
                result.AddRange(PaymentSplit.SplitPayment(payment));

            return result;
        }

        protected class PaymentRow
        {
            public string ClientName { get; set; }
            public string DocNumber { get; set; }
            public string DocAmount { get; set; }
            public string Paid { get; set; }
            public string PaymentMethod { get; set; }
            public string RefNumber { get; set; }
        }

        List<PaymentRow> CreatePaymentReceivedDataStructure(ref double totalCash, ref double totalCheck, ref double totalcc, ref double totalmo, ref double totaltr, ref double total)
        {
            List<PaymentRow> rows = new List<PaymentRow>();

            var listToUse = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;
            foreach (var pay in listToUse)
            {
                int index = 0;
                List<string> docNumbers = pay.Invoices().Select(x => x.InvoiceNumber).ToList();
                if (docNumbers.Count == 0)
                    docNumbers = pay.Orders().Select(x => x.PrintedOrderId).ToList();
                else
                    docNumbers.AddRange(pay.Orders().Select(x => x.PrintedOrderId).ToList());

                var t = pay.Invoices().Sum(x => x.Balance);
                t += pay.Orders().Sum(x => x.OrderTotalCost());

                while (true)
                {
                    var row = new PaymentRow();
                    if (index == 0)
                    {
                        row.ClientName = pay.Client.ClientName;

                        int factor = 0;
                        if (pay.Voided)
                            factor = 6;

                        if (row.ClientName.Length > (28 - factor))
                            row.ClientName = row.ClientName.Substring(0, (27 - factor));

                        row.DocAmount = t.ToString();
                    }
                    else
                    {
                        row.ClientName = string.Empty;
                        row.DocAmount = string.Empty;
                    }
                    if (docNumbers.Count > index)
                        row.DocNumber = docNumbers[index];
                    else
                        row.DocNumber = string.Empty;
                    if (pay.Components.Count > index)
                    {
                        if (!pay.Voided)
                        {
                            if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Cash)
                                totalCash += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Check)
                                totalCheck += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Credit_Card)
                                totalcc += pay.Components[index].Amount;
                            else if (pay.Components[index].PaymentMethod == InvoicePaymentMethod.Money_Order)
                                totalmo += pay.Components[index].Amount;
                            else
                                totaltr += pay.Components[index].Amount;

                            total += pay.Components[index].Amount;
                        }

                        if (pay.Voided)
                            row.ClientName += "(Void)";

                        row.RefNumber = pay.Components[index].Ref;
                        var s = pay.Components[index].Amount.ToCustomString();
                        //if (s.Length < 9)
                        //    s = new string(' ', 9 - s.Length) + s;
                        row.Paid = s;
                        row.PaymentMethod = ReducePaymentMethod(pay.Components[index].PaymentMethod);
                    }
                    else
                    {
                        row.RefNumber = string.Empty;
                        row.Paid = string.Empty;
                        row.PaymentMethod = string.Empty;
                    }
                    rows.Add(row);

                    index++;
                    if (docNumbers.Count <= index && pay.Components.Count <= index)
                        break;
                }
            }

            return rows;
        }

        protected string ReducePaymentMethod(InvoicePaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case InvoicePaymentMethod.Cash:
                    return "CA";
                case InvoicePaymentMethod.Check:
                    return "CH";
                case InvoicePaymentMethod.Credit_Card:
                    return "CC";
                case InvoicePaymentMethod.Money_Order:
                    return "MO";
                case InvoicePaymentMethod.Transfer:
                    return "TR";
                case InvoicePaymentMethod.Zelle_Transfer:
                    return "ZE";
            }
            return string.Empty;
        }

        protected virtual Style GetMediumFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual void AddNoBorderCellToBodySmall(Table tableLayout, string cellText, HorizontalAlignment alignment)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetMediumFontHelvetica(false));

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        public bool ReportVehicleInformationPdf(Document doc, bool FromEOD, int index = 0, int count = 0, bool isReport = false)
        {


            List<string> lines = new List<string>();

            var vehicleInfo = VehicleInformation.CurrentVehicleInformation;

            var endingVehicleInfo = VehicleInformation.EODVehicleInformation;

            if (vehicleInfo == null)
                return false;

            if (FromEOD && endingVehicleInfo == null)
                return false;

            var milesDifference = endingVehicleInfo.MilesFromDeparture - vehicleInfo.MilesFromDeparture;
            var formattedMilesDifference = milesDifference > 1 ? milesDifference.ToString() + " Miles" : milesDifference.ToString() + " Mile";

            var gasDifference = endingVehicleInfo.PutGas ?
    Fraction.SubtractFractions(endingVehicleInfo.Gas, vehicleInfo.Gas) :
    Fraction.SubtractFractions(vehicleInfo.Gas, endingVehicleInfo.Gas);




            if (FromEOD)
            {

                AddCompanysLogo(doc);
                AddTextLine(doc, "Vehicle Information Report", GetBigFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Printed Date:" + DateTime.Now.ToShortDateString(), GetNormalFont(), TextAlignment.LEFT);
                AddTextLine(doc, "Driver Name:" + Config.VendorName, GetNormalFont(), TextAlignment.LEFT);
                AddCompanysInfo(doc);


                if (isReport)
                {


                    AddTextLine(doc, "Plate Number:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.PlateNumber, GetNormalFont(), TextAlignment.LEFT);

                    AddTextLine(doc, "Gasoline:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "Put Gas: " + (endingVehicleInfo.PutGas ? "Yes" : "No"), GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "Start: " + vehicleInfo.Gas.ToString(), GetNormalFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "End: " + endingVehicleInfo.Gas.ToString(), GetNormalFont(), TextAlignment.LEFT);


                    AddTextLine(doc, "Assistant:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.Assistant, GetNormalFont(), TextAlignment.LEFT);

                    AddTextLine(doc, "Miles at End Of Day:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "End: " + (endingVehicleInfo.MilesFromDeparture > 1 ? endingVehicleInfo.MilesFromDeparture.ToString() + " Miles" : endingVehicleInfo.MilesFromDeparture.ToString() + " Mile"), GetNormalFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "Start: " + (vehicleInfo.MilesFromDeparture > 1 ? vehicleInfo.MilesFromDeparture.ToString() + " Miles" : vehicleInfo.MilesFromDeparture.ToString() + " Mile"), GetNormalFont(), TextAlignment.LEFT);
                    // Tire Condition
                    AddTextLine(doc, "Tire Condition:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "Start: " + vehicleInfo.TireCondition, GetNormalFont(), TextAlignment.LEFT);
                    AddTextLine(doc, "End: " + endingVehicleInfo.TireCondition, GetNormalFont(), TextAlignment.LEFT);

                    // Seat Belts
                    AddTextLine(doc, "Seat Belts:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.SeatBelts, GetNormalFont(), TextAlignment.LEFT);

                    // Engine Oil
                    AddTextLine(doc, "Engine Oil: Checked", GetNormalFont(), TextAlignment.LEFT); // 300 unidades hacia la derecha para que esté en la segunda columna
                                                                                                  // 300 es un ejemplo de margen izquierdo
                    AddTextLine(doc, vehicleInfo.EngineOil ? "Checked" : "Unchecked", GetNormalBoldFont(), TextAlignment.LEFT); // 400 para posicionarlo más a la derecha

                    AddTextLine(doc, vehicleInfo.EngineOil ? "Checked" : "Unchecked", GetNormalFont(), TextAlignment.LEFT);

                    // Brake Fluid
                    AddTextLine(doc, "Brake Fluid:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.BrakeFluid ? "Checked" : "Unchecked", GetNormalFont(), TextAlignment.LEFT);

                    // Power Steering Fluid
                    AddTextLine(doc, "Power Steering Fluid:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.PowerSteeringFluid ? "Checked" : "Unchecked", GetNormalFont(), TextAlignment.LEFT);

                    // Transmission Fluid
                    AddTextLine(doc, "Transmission Fluid:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.TransmissionFluid ? "Checked" : "Unchecked", GetNormalFont(), TextAlignment.LEFT);

                    // Antifreeze / Coolant
                    AddTextLine(doc, "Antifreeze / Coolant:", GetNormalBoldFont(), TextAlignment.LEFT);
                    AddTextLine(doc, vehicleInfo.AntifreezeCoolant ? "Checked" : "Unchecked", GetNormalFont(), TextAlignment.LEFT);


                }
                else
                {
                    // Create a table with two columns

                    float[] headers = { 50, 50 };  //Header Widths

                    Table table = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                    //                    float startY = 155;

                    //                    // Define la altura de las columnas basada en el espacio disponible hasta el final de la página.
                    //                    float columnHeight = 415;

                    //                    // Define los rectángulos para las columnas usando las coordenadas calculadas.
                    //                    Rectangle[] columns = {
                    //    new Rectangle(36, startY, 254, columnHeight),
                    //     new Rectangle(305, startY, 254, columnHeight) // Columna derecha //// Columna derecha
                    //};


                    //                    doc.SetRenderer(new ColumnDocumentRenderer(doc, columns));

                    var leftParagraph = new Paragraph("Plate Number:")
                          .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));


                    var rightParagraph = new Paragraph("Engine Oil: ")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));



                    leftParagraph = new Paragraph(vehicleInfo.PlateNumber)
                           .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    rightParagraph = new Paragraph(endingVehicleInfo.EngineOil ? "End: Checked" : "End: Unchecked")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("Gasoline: ")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT); // Establece la alineación del texto
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    rightParagraph = new Paragraph(vehicleInfo.EngineOil ? "Start: Checked" : "Start:Unchecked")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("Put Gas: " + (endingVehicleInfo.PutGas ? "Yes" : "No"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT); // Establece la alineación del texto
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));


                    rightParagraph = new Paragraph("Brake Fluid:")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));


                    leftParagraph = new Paragraph("End: " + endingVehicleInfo.Gas)
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    rightParagraph = new Paragraph(endingVehicleInfo.BrakeFluid ? " End: Checked" : "End: Unchecked")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));


                    leftParagraph = new Paragraph("Start: " + vehicleInfo.Gas)
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));


                    rightParagraph = new Paragraph(vehicleInfo.BrakeFluid ? "Start: Checked" : "Start: Unchecked")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));



                    leftParagraph = new Paragraph("Diference: " + gasDifference.ToString())
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));




                    rightParagraph = new Paragraph("Power Steering Fluid:")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));




                    leftParagraph = new Paragraph("Assistant:")
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    rightParagraph = new Paragraph(endingVehicleInfo.PowerSteeringFluid ? "End: Checked" : "End: Unchecked")
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));




                    leftParagraph = new Paragraph(vehicleInfo.Assistant)
                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));




                    rightParagraph = new Paragraph(vehicleInfo.PowerSteeringFluid ? "Start: Checked" : "Start: Unchecked")
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));


                    leftParagraph = new Paragraph("Miles at End Of Day:")
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("End: " + (endingVehicleInfo.MilesFromDeparture > 1 ? endingVehicleInfo.MilesFromDeparture.ToString() + " Miles" : endingVehicleInfo.MilesFromDeparture.ToString() + " Mile"))
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));


                    leftParagraph = new Paragraph("")
.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("Start: " + (vehicleInfo.MilesFromDeparture > 1 ? vehicleInfo.MilesFromDeparture.ToString() + " Miles" : vehicleInfo.MilesFromDeparture.ToString() + " Mile"))
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("")
.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));











                    leftParagraph = new Paragraph("Diference: " + formattedMilesDifference.ToString())
              .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));






                    rightParagraph = new Paragraph("Transmission Fluid:")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("Tire Condition:")
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));




                    rightParagraph = new Paragraph(endingVehicleInfo.TransmissionFluid ? "End: Checked" : "End: Unchecked")
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));


                    leftParagraph = new Paragraph("Start: " + vehicleInfo.TireCondition)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));





                    rightParagraph = new Paragraph(vehicleInfo.TransmissionFluid ? "Start: Checked" : "Start: Unchecked")
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("End: " + endingVehicleInfo.TireCondition)
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));



                    leftParagraph = new Paragraph("Seat Belts:")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));

                    rightParagraph = new Paragraph("Antifreeze / Coolant:")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalBoldFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));



                    leftParagraph = new Paragraph(vehicleInfo.SeatBelts)
                  .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));


                    rightParagraph = new Paragraph(endingVehicleInfo.AntifreezeCoolant ? "End: Checked" : "End: Unchecked")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));

                    leftParagraph = new Paragraph("")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    leftParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(leftParagraph).SetBorder(Border.NO_BORDER));
                    rightParagraph = new Paragraph(vehicleInfo.AntifreezeCoolant ? "Start: Checked" : "Start: Unchecked")
                 .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                    rightParagraph.AddStyle(GetNormalFont());
                    table.AddCell(new Cell().Add(rightParagraph).SetBorder(Border.NO_BORDER));



                    // Calcular la diferencia de millas




                    // Add the table to the document
                    doc.Add(table);




                }
            }
            else
            {


                AddTextLine(doc, "Brake Fluid:", GetNormalBoldFont(), TextAlignment.CENTER);
                AddTextLine(doc, vehicleInfo.BrakeFluid ? "Checked" : "No Checked", GetNormalFont(), TextAlignment.CENTER);

                AddTextLine(doc, "Power Steering Fluid:", GetNormalBoldFont(), TextAlignment.CENTER);
                AddTextLine(doc, vehicleInfo.PowerSteeringFluid ? "Checked" : "No Checked", GetNormalFont(), TextAlignment.CENTER);

                AddTextLine(doc, "Transmission Fluid:", GetNormalBoldFont(), TextAlignment.CENTER);
                AddTextLine(doc, vehicleInfo.TransmissionFluid ? "Checked" : "No Checked", GetNormalFont(), TextAlignment.CENTER);

                AddTextLine(doc, "Antifreeze / Coolant:", GetNormalBoldFont(), TextAlignment.CENTER);
                AddTextLine(doc, vehicleInfo.AntifreezeCoolant ? "Checked" : "No Checked", GetNormalFont(), TextAlignment.CENTER);

            }



            return true;


        }
        protected virtual void AddOrderCreatedReportTableToPDF(Document doc)
        {
            // Define los anchos de las columnas de la tabla de órdenes
            float[] columnWidths = { 20, 5, 10, 15, 15, 10 }; // Ajustar según la necesidad

            // Crea la tabla con los anchos de columna definidos
            Table orderTable = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

            AddCellToHeader(orderTable, "Name");
            AddCellToHeader(orderTable, "ST");
            AddCellToHeader(orderTable, "Qty");
            AddCellToHeader(orderTable, "Ticket #");
            AddCellToHeader(orderTable, "Total");
            AddCellToHeader(orderTable, "CS TP");


            List<string> lines = new List<string>();

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotalTerm = 0;
            double chargeTotalTerm = 0;
            double subtotal = 0;
            double totalTax = 0;

            double paidTotal = 0;
            double chargeTotal = 0;
            double creditTotal = 0;
            double salesTotal = 0;
            double billTotal = 0;

            double netTotal = 0;

            var payments = GetPaymentsForOrderCreatedReport();

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
            {
                if (!Config.IncludePresaleInSalesReport && order.AsPresale)
                    continue;

                var terms = order.Term.Trim();

                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.Quote:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Return:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                }
            }

            foreach (var b in Batch.List.OrderBy(x => x.ClockedIn))
                foreach (var p in b.Orders())
                {
                    if (!Config.PrintNoServiceInSalesReports && p.OrderType == OrderType.NoService)
                        continue;

                    if (!Config.IncludePresaleInSalesReport && p.AsPresale && p.OrderType != OrderType.NoService)
                        continue;

                    var orderCost = p.OrderTotalCost();

                    string totalCostLine = ToString(orderCost);
                    string subTotalCostLine = totalCostLine;


                    string status = GetCreatedOrderStatus(p);

                    double paid = 0;

                    var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);
                    if (payment != null)
                    {
                        double amount = payment.Amount;
                        paid = double.Parse(Math.Round(amount, Config.Round).ToCustomString(), NumberStyles.Currency);
                    }

                    string type = GetCreatedOrderType(p, paid, orderCost);

                    if (!p.Reshipped && !p.Voided && p.OrderType != OrderType.Quote)
                    {
                        if (orderCost < 0)
                            creditTotal += orderCost;
                        else
                        {
                            if (p.OrderType != OrderType.Bill)
                            {
                                salesTotal += orderCost;

                                if (paid == 0)
                                    chargeTotal += orderCost;
                                else
                                {
                                    paidTotal += paid;
                                    chargeTotal += orderCost - paid;
                                }
                            }
                            else
                                billTotal += orderCost;
                        }
                    }
                    else
                        type = string.Empty;

                    float qty = 0;
                    foreach (var item in p.Details)
                        if (!item.SkipDetailQty(p))
                            qty += item.Qty;

                    var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);

                    var clockInfo = $"Clock In: {batch.ClockedIn.ToShortTimeString()}, Clock Out: {batch.ClockedOut.ToShortTimeString()}";
                    string clientNameAndClockInfo = $"{p.Client.ClientName}\n{clockInfo}";

                    // Agrega todo en una sola celda
                    AddCellToBody(orderTable, clientNameAndClockInfo);
                    AddCellToBody(orderTable, status);
                    AddCellToBody(orderTable, qty.ToString());
                    AddCellToBody(orderTable, p.PrintedOrderId);
                    AddCellToBody(orderTable, totalCostLine);
                    AddCellToBody(orderTable, type);

                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;

                    if (p.Voided)
                        voided++;
                    else if (p.Reshipped)
                        reshipped++;
                    else if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                        delivered++;
                    else
                        dsd++;


                }

            if (start > end)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            // Calcula el tiempo total correctamente.
            var ts = end.Subtract(start);
            string totalTime = (ts.Hours > 0 || ts.Minutes > 0) ?
                               String.Format("{0}h {1}m", ts.Hours, ts.Minutes) :
                               "0h 0m";

            netTotal = Math.Round(salesTotal - Math.Abs(creditTotal), Config.Round);

            // Agrega la tabla al documento PDF
            doc.Add(orderTable);
            if (salesTotal <= 0)
            {
                totalTime = "0h 0m";
            }

            Table totalTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();


            Cell leftCell = new Cell().Add(new Paragraph()

                .Add(new Text("Refused: " + reshipped + "\n"))
                .Add(new Text("Voided: " + voided.ToString() + "\n"))
                .Add(new Text("Delivery: " + delivered.ToString() + "\n"))
                .Add(new Text("P&P: " + dsd.ToString() + "\n"))
                .Add(new Text("Time (Hours): " + totalTime + "\n"))
            ).SetFontSize(10).SetBorder(Border.NO_BORDER);


            Cell rightCell = new Cell().Add(new Paragraph()
                .Add(new Text("Credit Total: " + creditTotal.ToCustomString() + "\n"))
                 .Add(new Text("Bill Total: " + billTotal.ToCustomString() + "\n"))
                   .Add(new Text("Sales Total: " + salesTotal.ToCustomString() + "\n"))
                .Add(new Text("Expected Cash Cust: " + cashTotalTerm.ToCustomString() + "\n"))
                .Add(new Text("Paid Cust: " + paidTotal.ToCustomString() + "\n"))
                  .Add(new Text("Charge Cost: " + chargeTotal.ToCustomString() + "\n"))

                .Add(new Text("Sales Total: " + salesTotal.ToCustomString() + "\n"))


            ).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER);

            totalTable.AddCell(leftCell);
            totalTable.AddCell(rightCell);

            doc.Add(totalTable);


        }

        protected virtual void AddSettlementReportTableToPDF(Document doc, List<InventorySettlementRow> map, InventorySettlementRow totalRow)
        {
            if (map.Count > 0)
            {
                // Define los anchos de las columnas de la tabla de liquidación
                float[] columnWidths = { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 };  // Ajustar según la necesidad

                // Crea la tabla con los anchos de columna definidos
                Table settlementTable = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

                AddCellToHeader(settlementTable, "Product\nUom");
                AddCellToHeader(settlementTable, "BegInv");
                AddCellToHeader(settlementTable, "LoadOut");
                AddCellToHeader(settlementTable, "Adj");
                AddCellToHeader(settlementTable, "Transfer");
                AddCellToHeader(settlementTable, "Sales");
                AddCellToHeader(settlementTable, "Credit Returns");
                AddCellToHeader(settlementTable, "Credit Dump");
                AddCellToHeader(settlementTable, "Reshipped");
                AddCellToHeader(settlementTable, "Damaged In Truck");
                AddCellToHeader(settlementTable, "Unload");
                AddCellToHeader(settlementTable, "End Inv");
                AddCellToHeader(settlementTable, "O/S");

                // Recorre el mapa y agrega cada fila a la tabla
                foreach (var item in map)
                {
                    Paragraph productName = new Paragraph(item.Product.Name).SetFontColor(ColorConstants.BLACK).SetFontSize(8);
                    Cell productHeaderCell = new Cell(1, 13).Add(productName);
                    productHeaderCell.SetTextAlignment(TextAlignment.LEFT);
                    productHeaderCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

                    settlementTable.AddCell(productHeaderCell); // Span across all columns

                    AddCellToBody(settlementTable, item.UoM != null ? item.UoM.Name : "");
                    AddCellToBody(settlementTable, item.BegInv.ToString());
                    AddCellToBody(settlementTable, item.LoadOut.ToString());
                    AddCellToBody(settlementTable, item.Adj.ToString());
                    AddCellToBody(settlementTable, (item.TransferOn - item.TransferOff).ToString());
                    AddCellToBody(settlementTable, item.Sales.ToString());
                    AddCellToBody(settlementTable, item.CreditReturns.ToString());
                    AddCellToBody(settlementTable, item.CreditDump.ToString());
                    AddCellToBody(settlementTable, item.Reshipped.ToString());
                    AddCellToBody(settlementTable, item.DamagedInTruck.ToString());
                    AddCellToBody(settlementTable, item.Unload.ToString());
                    AddCellToBody(settlementTable, item.EndInventory.ToString());
                    AddCellToBody(settlementTable, item.OverShort.ToString());
                    // ... agregar más celdas según sea necesario
                }

                // Agrega los totales después de la tabla
                AddCellToHeader(settlementTable, "Total");
                AddCellToHeader(settlementTable, totalRow.BegInv.ToString());
                AddCellToHeader(settlementTable, totalRow.LoadOut.ToString());
                AddCellToHeader(settlementTable, totalRow.Adj.ToString());
                AddCellToHeader(settlementTable, (totalRow.TransferOn - totalRow.TransferOff).ToString());
                AddCellToHeader(settlementTable, totalRow.Sales.ToString());
                AddCellToHeader(settlementTable, totalRow.CreditReturns.ToString());
                AddCellToHeader(settlementTable, totalRow.CreditDump.ToString());
                AddCellToHeader(settlementTable, totalRow.Reshipped.ToString());
                AddCellToHeader(settlementTable, totalRow.DamagedInTruck.ToString());
                AddCellToHeader(settlementTable, totalRow.Unload.ToString());
                AddCellToHeader(settlementTable, totalRow.EndInventory.ToString());
                AddCellToHeader(settlementTable, totalRow.OverShort.ToString());
                // ... agregar más celdas para los totales según sea necesario

                // Agrega la tabla al documento PDF
                doc.Add(settlementTable);
            }

        }




        public virtual string ToString(double d)
        {
            return d.ToCustomString();
        }

        // Métodos auxiliares para obtener estados y tipos de órdenes
        // Retorna el estado de la orden
        protected virtual string GetCreatedOrderStatus(Order o)
        {
            string status = string.Empty;

            if (o.OrderType == OrderType.NoService)
                status = "NS";
            if (o.Voided)
                status = "VD";
            if (o.Reshipped)
                status = "RF";

            if (o.OrderType == OrderType.Bill)
                status = "Bi";

            if (o.OrderType == OrderType.Quote)
                status = "QT";

            return status;
        }

        private string GetCreatedOrderType(Order order, double paidAmount, double orderCost)
        {
            // Si el costo total del pedido es negativo, entonces es un crédito
            if (order.OrderTotalCost() < 0)
            {
                return "Credit";
            }
            // Si el monto pagado es igual al costo total del pedido, entonces está pagado
            else if (paidAmount == orderCost)
            {
                return "Paid";
            }
            // Si el monto pagado es mayor que cero pero menos que el costo total, es un pago parcial
            else if (paidAmount > 0 && paidAmount < orderCost)
            {
                return "Partial P.";
            }
            // Si no se ha pagado nada, entonces está pendiente de pago
            else if (paidAmount == 0)
            {
                return "Pending";
            }

            // Si es una cotización, no aplicaría un tipo de pago
            if (order.OrderType == OrderType.Quote)
            {
                return string.Empty;
            }

            // Implementa cualquier otra lógica específica que pueda ser necesaria para tu aplicación

            // Si ninguna de las condiciones anteriores se cumple, retorna un string vacío
            return string.Empty;
        }

        void CreateSettlementReportDataStructure(ref InventorySettlementRow totalRow, ref List<InventorySettlementRow> map)
        {
            map = DataProvider.ExtendedSendTheLeftOverInventory();

            foreach (var value in map)
            {
                if (value.IsEmpty)
                    continue;

                if (Config.ShortInventorySettlement && value.IsShort)
                    continue;

                var product = value.Product;

                totalRow.Product = product;
                totalRow.BegInv += value.BegInv;
                totalRow.LoadOut += value.LoadOut;
                totalRow.Adj += value.Adj;
                totalRow.TransferOff += value.TransferOff;
                totalRow.TransferOn += value.TransferOn;
                // totalRow.EndInventory += value.EndInventory > 0 ? value.EndInventory : 0;
                totalRow.EndInventory += value.EndInventory;
                totalRow.Dump += value.Dump;
                totalRow.DamagedInTruck += value.DamagedInTruck;
                totalRow.Unload += value.Unload;

                if (!value.SkipRelated)
                {
                    totalRow.Sales += value.Sales;
                    totalRow.CreditReturns += value.CreditReturns;
                    totalRow.CreditDump += value.CreditDump;
                    totalRow.Reshipped += value.Reshipped;
                }
            }
        }



        private void AddLinesInfo(Document doc, List<string> lines)
        {
            foreach (var line in lines)
            {
                AddTextLine(doc, line, GetNormalFont(), TextAlignment.LEFT);
            }
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
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
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
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
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
        public class T1
        {
            public Client Client { get; set; }
            public List<InvoicePayment> Payments { get; set; }
        }

        public class T4
        {
            public PaymentComponent component { get; set; }

            public double AmountRemaining { get; set; }
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

        public void AddOrderHeaderTablePan(Document doc, BankDeposit order, List<InvoicePayment> payments)
        {
            AddTextLine(doc, "DEPOSIT PAYMENT", GetBigFont(), TextAlignment.CENTER);
            AddTextLine(doc, Environment.NewLine, GetBigFont());


            // Group payments by client
            var groupedPayments = payments
                .GroupBy(p => p.Client)
                .Select(g => new T1
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

                    var componentList = new List<T4>();

                    foreach (var c in components)
                        componentList.Add(new T4() { AmountRemaining = c.Amount, component = c });

                    var invoices = payment.Invoices().OrderBy(x => x.DueDate);
                    foreach (var i in invoices)
                    {

                        double amountToSubstract = i.Balance;

                        var listOfComponentsUsed = new List<T4>();

                        foreach (var c in componentList.Where(x => x.AmountRemaining > 0))
                        {
                            if (c.AmountRemaining > amountToSubstract)
                            {
                                c.AmountRemaining -= amountToSubstract;
                                listOfComponentsUsed.Add(new T4() { AmountRemaining = amountToSubstract, component = c.component });
                                break;
                            }
                            else
                            {
                                amountToSubstract -= c.AmountRemaining;
                                listOfComponentsUsed.Add(new T4() { AmountRemaining = c.AmountRemaining, component = c.component });
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
                                if (!string.IsNullOrEmpty(method))
                                    method += "\n" + c.component.PaymentMethod.ToString().Replace("_", " ");
                                else
                                    method = c.component.PaymentMethod.ToString().Replace("_", " ");


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
                                if (!string.IsNullOrEmpty(method))
                                    method += "\n" + c.component.PaymentMethod.ToString().Replace("_", " ");
                                else
                                    method = c.component.PaymentMethod.ToString().Replace("_", " ");


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


        protected virtual void AddDepositInfo(Document doc, BankDeposit order)
        {
            //string docName = "PAYMENT DEPOSIT";

            //AddTextLine(doc, docName, GetBigFont(), TextAlignment.CENTER);

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

        public string GetStatementReportPdf(Client client)
        {
            string appPath = Config.BasePath;
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