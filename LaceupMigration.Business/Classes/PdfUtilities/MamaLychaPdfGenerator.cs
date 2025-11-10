using iText.IO.Image;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Renderer;


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using System.IO;
using iText.Commons.Actions;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Event;
using iText.Layout.Layout;

namespace LaceupMigration
{

    public class MamaLychaPdfGenerator : DefaultPdfProvider
    {
        double totalAmount = 0;
        double totalDiscount = 0;
        double totalFreight = 0;
        double totalTax = 0;
        int processCell = 0;
        Table headerTable;
        Table OrderTable;
        Document doc;
        PdfDocument pdf;

        public MamaLychaPdfGenerator()
        {
        }
        public override string GetOrderPdf(Order order)
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
            pdf = new PdfDocument(writer);
            pdf.SetDefaultPageSize(PageSize.A4);

            doc = new Document(pdf);

            PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

            pdf.AddEventHandler(PdfDocumentEvent.START_PAGE,
            new PageNumberEventHandler(normalFont));
            AddContentToPDF(doc, order);


            doc.Close();

            return targetFile;
        }
        protected override void AddContentToPDF(Document doc, Order order)
        {

            AddCompanyInfo(doc, order);

            AddOrderClientInfo(doc, order.Client, Config.UseQuote && order.IsQuote);

            AddOrderHeaderTable(doc, order);

            AddTextLine(doc, " ", GetSmallFont());

            AddOrderDetailsTable(doc, order);

            AddFooterTable(doc, order);

            Reset();
        }

        private void Reset()
        {
            processCell = 0;
            totalAmount = 0;
            totalDiscount = 0;
            totalFreight = 0;
            totalTax = 0;
            headerTable = null;
            OrderTable = null;
        }

        protected override void AddOrderClientInfo(Document doc, Client client, bool isQuote = false, Order order = null)
        {
            if (client == null)
                return;

            float[] headers = { 70, 100 };
            float[] subHeaders = { 50 };

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();
            Table tableLayout2 = new Table(UnitValue.CreatePercentArray(subHeaders));
            Table tableLayout3 = new Table(UnitValue.CreatePercentArray(subHeaders));
            tableLayout.SetBorder(Border.NO_BORDER);
            tableLayout2.SetBorder(Border.NO_BORDER);
            tableLayout3.SetBorder(Border.NO_BORDER);

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

            AddCellToBody(tableLayout2, soldToLabel, HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));
            AddCellToBody(tableLayout2, soldTo, HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));
            AddCellToBody(tableLayout3, "Ship To:", HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));
            AddCellToBody(tableLayout3, shipTo, HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));

            tableLayout.AddCell(new Cell().Add(tableLayout2).SetBorder(Border.NO_BORDER));
            tableLayout.AddCell(new Cell().Add(tableLayout3).SetBorder(Border.NO_BORDER));

            doc.Add(tableLayout);

            AddTextLine(doc, "\n", GetSmallFontHelvetica(false));
        }
        private void AddOrderInfoLabel(Table doc, Order order)
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
            doc.AddCell(new Cell().Add(new Paragraph(docName).AddStyle(GetTitleBoldFontHelvetica())).SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER));

            if (((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0))
            {
                var site = SiteEx.Sites.FirstOrDefault(x => x.Id == Config.SalesmanSelectedSite);
                if (site != null)
                {
                    doc.AddCell(new Cell().Add(new Paragraph("Site: " + docName).AddStyle(GetTitleBoldFontHelvetica())).SetTextAlignment(TextAlignment.LEFT));
                }
            }

        }
        protected override void AddCompanyInfo(Document doc, Order order)
        {

            CompanyInfo company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);

            if (company == null)
                company = CompanyInfo.Companies[0];


            AddCompanyInfoWithLogo(doc, order, company);

        }

        protected override void AddCompanyInfoWithLogo(Document doc, Order order, CompanyInfo company)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 100, 100 })).UseAllAvailableWidth();
            try
            {
                Image jpg = null;
                if (!string.IsNullOrEmpty(company.CompanyLogoPath))
                {
                    jpg = new Image(ImageDataFactory.Create(company.CompanyLogoPath));
                }
                else if (!string.IsNullOrEmpty(Config.LogoStorePath))
                {
                    jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));
                }

                if (jpg != null)
                {
                    jpg.ScaleToFit(90f, 75f);
                    jpg.SetPaddingLeft(9f);

                    table.AddCell(new Cell().Add(jpg).SetBorder(Border.NO_BORDER));
                }

            }
            catch
            {

            }
            AddOrderInfoLabel(table, order);

            doc.Add(table);

            string textToAdd = string.Empty;
            headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 35, 35 })).UseAllAvailableWidth();
            Salesman salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            Salesman delivery = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            var x = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            if (string.IsNullOrEmpty(order.PONumber) && Config.AutoGeneratePO)
            {
                string po = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");
                order.PONumber = po;
            }

            PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            Paragraph left = new Paragraph()
                .SetFontSize(8);
            Paragraph left2 = new Paragraph()
                .SetFontSize(8);

            Table infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 100, 90 })).UseAllAvailableWidth();


            infoTable.AddCell(new Cell().Add(new Paragraph("Invoice Number:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph($"{order.PrintedOrderId}").SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Invoice Date:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph($"{order.Date.ToShortDateString()}").SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Sales Order Number:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph($"{order.OrderId}").SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Order Date:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph($"{order.Date.ToShortDateString()}").SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Salesman").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph($"{salesman.Name}").SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Delivery Man:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph(delivery.Name).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            infoTable.AddCell(new Cell().Add(new Paragraph("Customer Number:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph(order.Client.OriginalId.ToString()).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            textToAdd += (company.CompanyName != null ? company.CompanyName : string.Empty) + "\n";
            textToAdd += company.CompanyAddress1 + "\n";

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                textToAdd += company.CompanyAddress2 + "\n";

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                textToAdd += phoneLine + "\n";

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = DataAccess.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    textToAdd += "TIN:" + extra + "\n";
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                textToAdd += "Email:" + company.CompanyEmail + "\n";
            left.Add(textToAdd);
            headerTable.AddCell(new Cell().Add(left).SetBorder(Border.NO_BORDER));
            headerTable.AddCell(new Cell().Add(infoTable).SetBorder(Border.NO_BORDER));

            headerTable.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            doc.Add(headerTable);


        }

        protected override void AddFooterTable(Document doc, Order order)
        {
            AddEmptySpace(2, doc);


            PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            bool credit = order.OrderType == OrderType.Credit;
            var footerContent = new Div();

            // --- Section 1: Weight and Shipped info ---
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = "$";
            culture.NumberFormat.CurrencyNegativePattern = 0;
            var footer = new Table(UnitValue.CreatePercentArray(new float[] { 50, 65, 45 })).UseAllAvailableWidth();
            var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 100 })).UseAllAvailableWidth();

            double totalWeight = order.Details.Sum(x => x.Product.Weight * x.Qty);
            infoTable.AddCell(new Cell().Add(new Paragraph("Total Weight:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph(totalWeight.ToString()).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            float totalShipped = 0;
            foreach (var detail in order.Details)
            {
                if (detail.DeliveryQty.Length > 0)
                {
                    for (int j = 0; j < detail.DeliveryQty.Length; j++)
                    {
                        if (detail.DeliveryQty[j] == 1)
                        {
                            if (j == detail.DeliveryQty.Length - 1 && detail.Ordered % 1 > 0)
                                totalShipped += detail.Ordered - (int)detail.Ordered;
                            else
                                totalShipped++;
                        }
                    }
                }
            }
            infoTable.AddCell(new Cell().Add(new Paragraph("Total Cases Shipped:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            infoTable.AddCell(new Cell().Add(new Paragraph(totalShipped.ToString()).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));

            footer.AddCell(new Cell().Add(new Paragraph().SetFontSize(8)).SetBorder(Border.NO_BORDER));
            footer.AddCell(new Cell().Add(infoTable).SetBorder(Border.NO_BORDER));
            footerContent.Add(footer);

            // --- Section 2: Total breakdown ---
            var totalFooter = new Table(UnitValue.CreatePercentArray(new float[] { 25, 35 })).UseAllAvailableWidth();
            var totalFooterParent = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();


            totalFooter.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            totalFooter.SetMarginLeft(20);
            totalFooterParent.SetBorder(Border.NO_BORDER);
            var totalInvoice = FormatCurrency(totalAmount - totalDiscount, credit);
            var totalA = FormatCurrency(totalAmount, credit);
            var discount = FormatCurrency(totalDiscount, credit);
            var tax = FormatCurrency(order.CalculateSalesTax(), credit);
            var fre = FormatCurrency(order.CalculatedFreight(), credit);

            totalFooter.AddCell(new Cell().Add(new Paragraph("Net Invoice:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            totalFooter.AddCell(new Cell().Add(new Paragraph(totalA).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));
            totalFooter.AddCell(new Cell().Add(new Paragraph("Less Discount:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            totalFooter.AddCell(new Cell().Add(new Paragraph(discount).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));
            totalFooter.AddCell(new Cell().Add(new Paragraph("Sales Tax:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            totalFooter.AddCell(new Cell().Add(new Paragraph(tax).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));
            totalFooter.AddCell(new Cell().Add(new Paragraph("Freight:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            totalFooter.AddCell(new Cell().Add(new Paragraph(fre).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));
            totalFooter.AddCell(new Cell().Add(new Paragraph("Invoice Total:").SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT));
            totalFooter.AddCell(new Cell().Add(new Paragraph(totalInvoice).SetFont(normalFont)).SetBorder(Border.NO_BORDER).SetFontSize(8));
            var signature = GetSignatureCell(order);
            signature.SetBorder(Border.NO_BORDER);
            signature.SetMarginLeft(30);
            totalFooterParent.AddCell(signature);

            totalFooterParent.AddCell(new Cell().Add(totalFooter).SetBorder(Border.NO_BORDER));
            footerContent.Add(totalFooterParent);

            // --- Section 3: Policy List ---
            footerContent.Add(new Paragraph("\n"));
            footerContent.Add(new LineSeparator(new SolidLine()));

            var list = new List().SetListSymbol("*  ").SetPaddingTop(4).SetPaddingLeft(4).SetFontSize(6).SetFont(boldFont);
            list.Add(new ListItem("All claims must be made within 48 hours of receipt of merchandise - valid documentation and pictures are required to support claim. We are not responsible for claims for merchandise transported by a transportation company hired directly by the customer."));
            list.Add(new ListItem("Invoice deductions may only be taken with prior approval from Mama Lycha."));

            var list2 = new List().SetListSymbol("*").SetPaddingLeft(4).SetFontSize(6).SetFont(normalFont);
            list2.Add(new ListItem("Accounts that are not paid within credit terms above, may be subject to a 1.5% monthly finance charge."));
            list2.Add(new ListItem("All NSF checks will be subject to a $40.00 fee. Invoices must be re-paid in cash, money order, or credit card."));
            list2.Add(new ListItem("Mama Lycha reserves the right to no longer accept checks from customers who have had more than 1 NSF check."));
            list2.Add(new ListItem("Prices may change without prior notice."));

            footerContent.Add(list);
            footerContent.Add(list2);

            // --- Layout check: calculate height and push to next page if needed ---
            IRenderer renderer = footerContent.CreateRendererSubTree().SetParent(doc.GetRenderer());
            LayoutResult layoutResult = renderer.Layout(new LayoutContext(new LayoutArea(0, PageSize.A4)));

            float requiredHeight = layoutResult.GetOccupiedArea().GetBBox().GetHeight();
            float availableHeight = doc.GetRenderer().GetCurrentArea().GetBBox().GetHeight();
            float remaining = availableHeight - requiredHeight;

            if (remaining > 20 && remaining < 150)
            {
                doc.Add(new Paragraph().SetHeight(remaining - 20));
            }
            else if (requiredHeight > availableHeight)
            {
                doc.Add(new AreaBreak());
            }

            doc.Add(footerContent);
        }

        string FormatCurrency(double value, bool isCredit)
        {
            var formatted = value.ToCustomString();

            if (value == 0)
                return formatted;

            return isCredit ? $"({formatted})" : formatted;
        }
        protected override void AddOrderHeaderTable(Document doc, Order order)
        {

            LineSeparator separator = new LineSeparator(new SolidLine());

            doc.Add(separator);

            float[] headers = { 20, 20, 20, 20 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();
            //Add header
            AddNoBorderHeaderToBodyBold(tableLayout, "Customer P.O", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(tableLayout, "Ship VIA", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(tableLayout, "Email", HorizontalAlignment.LEFT);


            var dateLabel = "Inv. Due Date";

            AddNoBorderHeaderToBodyBold(tableLayout, dateLabel, HorizontalAlignment.LEFT, true);


            string shipVia = string.Empty;
            string customerPO = string.Empty;

            if (!string.IsNullOrEmpty(order.ExtraFields))
            {
                shipVia = DataAccess.GetSingleUDF("ShipVia", order.ExtraFields);
                customerPO = DataAccess.GetSingleUDF("CustomerPONo", order.ExtraFields);
            }


            AddNoBorderCellToBodySmall(tableLayout, shipVia, HorizontalAlignment.LEFT);
            AddNoBorderCellToBodySmall(tableLayout, customerPO, HorizontalAlignment.LEFT);

            string terms = "";
            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            AddNoBorderCellToBodySmall(tableLayout, terms, HorizontalAlignment.LEFT);
            AddNoBorderCellToBodySmall(tableLayout, order.Date.ToShortDateString(), HorizontalAlignment.LEFT);

            doc.Add(tableLayout);
            LineSeparator separator2 = new LineSeparator(new SolidLine());
            doc.Add(separator2);

        }

        protected override void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 10, 40, 10, 10, 10, 10, 10, 10, 10, 15 };  //Header Widths
            OrderTable = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddNoBorderHeaderToBodyBold(OrderTable, "Item No.", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Description", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "UPC", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Ordered", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Shipped", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Unit", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Price:", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Discount", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Net Price", HorizontalAlignment.LEFT);
            AddNoBorderHeaderToBodyBold(OrderTable, "Amount", HorizontalAlignment.LEFT);
            if (order.Details != null)
            {
                var details = SortDetails.SortedDetails(order.Details).ToList();
                GenerateOrderTable(doc, details, order);

            }

        }

        private void GenerateOrderTable(Document doc, List<OrderDetail> details, Order order)
        {
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = "$";
            culture.NumberFormat.CurrencyNegativePattern = 0;
            var credit = order.OrderType == OrderType.Credit;
            bool addNewPage = false;
            for (var i = processCell; i < details.Count(); i++)
            {
                var detail = details[i];
                if (detail.OrderDiscountId > 0)
                    continue;

                Product product = detail.Product;
                AddNoBorderCellToBodySmall(OrderTable, (product != null) ? product.Code : "", HorizontalAlignment.LEFT);

                var name = product != null ? product.Name : "";
                if (!string.IsNullOrEmpty(detail.Comments))
                    name += "\n" + "Comment:" + detail.Comments;

                if (detail.Product.IsDiscountItem)
                    name = detail.Comments;

                AddNoBorderCellToBodySmall(OrderTable, name, HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, (product != null && !string.IsNullOrEmpty(product.Upc)) ? product.Upc : "", HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, detail.Qty.ToString("N"), HorizontalAlignment.LEFT);


                float sum = 0;
                for (int j = 0; j < detail.DeliveryQty.Length; j++)
                {
                    if (detail.DeliveryQty[j] == 1)
                    {
                        if (detail.DeliveryQty.Length - 1 == j && detail.Ordered % 1 > 0)
                        {
                            var decimals = detail.Ordered - (int)detail.Ordered;
                            sum += decimals;
                        }
                        else
                            sum++;
                    }
                }

                AddNoBorderCellToBodySmall(OrderTable, sum.ToString("N"), HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : "CASE", HorizontalAlignment.LEFT);


                bool cameFromOffer = false;
                var price = Product.CalculatePriceForProduct(detail.Product, order.Client, detail.IsCredit, detail.Damaged, detail.UnitOfMeasure, false, out cameFromOffer, true, order);      
                price = detail.ExpectedPrice > 0 ? detail.ExpectedPrice : detail.Price > 0 ? detail.Price : price;
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

                    if (totalPrice == 0)
                    {
                        dc = detail.CostPrice;
                        discountPrice = 0;
                    }
                }
                else
                {
                    discountPrice = (price * detail.Qty) * order.DiscountAmount;
                }


                var total = FormatCurrency((detail.Qty * price), credit);
                AddNoBorderCellToBodySmall(OrderTable, price.ToCustomString(), HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, discountPrice.ToCustomString(), HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, price.ToCustomString(), HorizontalAlignment.LEFT);
                AddNoBorderCellToBodySmall(OrderTable, total, HorizontalAlignment.LEFT);

                totalDiscount += discountPrice;
                totalAmount += (detail.Qty * price);

                if (i > 0 && i % 12 == 0)
                {
                    addNewPage = true;
                    break;
                }

                processCell++;

            }
            if (addNewPage)
            {
                if (processCell < details.Count())
                {
                    processCell++;
                    OrderTable.FlushContent();
                    OrderTable.SetFixedLayout();
                    OrderTable.SetAutoLayout();
                    doc.Add(OrderTable);
                    if (processCell < details.Count)
                    {
                        AddTextLine(doc, "Continued", GetSmallFontHelvetica(true), TextAlignment.RIGHT);
                        doc.Flush();
                        doc.Add(new AreaBreak());
                        AddCompanyInfo(doc, order);
                        AddOrderClientInfo(doc, order.Client, Config.UseQuote && order.IsQuote);
                        AddOrderHeaderTable(doc, order);
                        AddOrderDetailsTable(doc, order);

                    }
                }
            }
            else
            {
                doc.Add(OrderTable);
            }

        }

        protected override Cell GetSignatureCell(Order order, int rowSpan = 4)
        {
            if (order != null && order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                var imgPath = GetSignatureImage(order);

                Image jpg = new Image(ImageDataFactory.Create(imgPath));
                jpg.ScaleToFit(75f, 75f);

                Cell img = new Cell();
                img.SetPaddingLeft(50);
                img.Add(jpg);
                img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                img.SetVerticalAlignment(VerticalAlignment.MIDDLE);

                return img;
            }

            Cell empty = new Cell();
            var prg = new Paragraph("X");
            prg.AddStyle(GetSmall8Font());
            empty.Add(prg);
            empty.SetVerticalAlignment(VerticalAlignment.BOTTOM);

            return empty;
        }

    }

    public class PageNumberEventHandler : AbstractPdfDocumentEventHandler
    {
        private readonly PdfFont _font;

        public PageNumberEventHandler(PdfFont font)
        {
            _font = font;
        }

        protected override void OnAcceptedEvent(AbstractPdfDocumentEvent currentEvent)
        {
            var docEvent = (PdfDocumentEvent)currentEvent;
            var pdfDoc = docEvent.GetDocument();
            var page = docEvent.GetPage();
            var pageSize = page.GetPageSize();
            int pageNumber = pdfDoc.GetPageNumber(page);

            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
            canvas.BeginText()
                .SetFontAndSize(_font, 8)
                .MoveText(pageSize.GetWidth() - 36, pageSize.GetTop() - 20)
                .ShowText("Page: " + pageNumber)
                .EndText();
        }
    }
}
