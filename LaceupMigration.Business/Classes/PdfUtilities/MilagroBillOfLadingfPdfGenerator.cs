using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using iText.Commons.Actions;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;



namespace LaceupMigration
{
    public class MilagroBillOfLadingPdfGenerator : DefaultPdfProvider
    {
        private Table headerTable;
        protected override void AddContentToPDF(Document doc, Order order)
        {


            AddCompanyInfo(doc, order);

            AddOrderClientInfo(doc, order.Client, Config.UseQuote && order.IsQuote);


            AddTextLine(doc, " ", GetSmallFont());

            AddOrderDetailsTable(doc, order);
             // ;

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
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 100, 180,100 })).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
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
                else
                {
                    table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph()));
                    
                }

            }
            catch
            {

            }
            AddOrderInfoLabel(table,doc, order);

            
        }
        private void AddOrderInfoLabel(Table table, Document doc, Order order)
        {
            var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            var titleTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            
            infoTable.SetBorder(Border.NO_BORDER);
            titleTable.SetBorder(Border.NO_BORDER);
            
            
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph date = new Paragraph($"Date: {order.Date.ToShortDateString()}")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.RIGHT);
            infoTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(date));
            var numberAccount = UDFHelper.GetSingleUDF("Account #", order.Client.ExtraPropertiesAsString);
            
            Paragraph account = new Paragraph($"Account #: {numberAccount}")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginRight(10);
            Paragraph po = new Paragraph($"PO#: {order.PONumber}")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginRight(10);
            Paragraph orderNo = new Paragraph($"Order#: {order.PrintedOrderId}")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginRight(10)
                .SetTextAlignment(TextAlignment.RIGHT);

            infoTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(po));
            infoTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(orderNo));
            infoTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(account));
            
             
            Paragraph title = new Paragraph("STRAIGHT BILL OF LADING")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER);

            Paragraph subtitle = new Paragraph("Original – Not Negotiable")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER);
            
           

            titleTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(title));
            titleTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(subtitle));
            
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(titleTable));
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(infoTable));


            doc.Add(table);

        }
        
        protected override void AddOrderClientInfo(Document doc, Client client, bool isQuote = false, Order order= null)
        {
            if (client == null)
                return;

            float[] headers = { 15, 35, 15, 35 };//Header Widths
            float[] header = {100,100 };//Header Widths

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

            var soldToLabel = "From:";
            
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            AddCellToBody(tableLayout, soldToLabel, HorizontalAlignment.RIGHT, Border.NO_BORDER, GetBigFontHelvetica(true));
            AddCellToBody(tableLayout, soldTo, HorizontalAlignment.LEFT, Border.NO_BORDER,GetBigFontHelvetica(false));
            AddCellToBody(tableLayout, "Ship To:", HorizontalAlignment.RIGHT, Border.NO_BORDER,GetBigFontHelvetica(true));
            AddCellToBody(tableLayout, shipTo, HorizontalAlignment.LEFT, Border.NO_BORDER,GetBigFontHelvetica(false));

            doc.Add(tableLayout);

            
            Table table = new Table(UnitValue.CreatePercentArray(header)).UseAllAvailableWidth();

            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph()));
            table.AddCell(new Cell().SetHorizontalAlignment(HorizontalAlignment.RIGHT).Add(new Paragraph("Seal #:")));
            
            doc.Add(table);
        }
        protected override void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 30,30,80,30 }; //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToBody(tableLayout, "Item #");
            AddCellToBody(tableLayout, "Ship. Qty");
            AddCellToBody(tableLayout, "Description");
            AddCellToBody(tableLayout, "Weight (lbs)");

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
                   

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    if (detail.Product.IsDiscountItem)
                        name = detail.Comments;

                    AddCellToBody(tableLayout, detail.Qty.ToString());
                    AddCellToBody(tableLayout, name);

                    AddCellToBody(tableLayout, Math.Round((detail.Qty * detail.Product.Weight), 2).ToString());
                    
                }
            }

            tableLayout.SetPaddingBottom(10);
            doc.Add(tableLayout);
            var totalContainer = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            var shippingTotalsTable = BuildShippingTotalsTable(order);
            totalContainer.AddCell(new Cell().Add(shippingTotalsTable).SetBorder(Border.NO_BORDER));
            doc.Add(totalContainer);
            
            var container = new Div()
                .SetKeepTogether(true);
            var table = BuildFooterTable(order);
            container.Add(table);
            doc.Add(container);
            // if (order.Details.Count >= 14)
            //     doc.Add(new AreaBreak());


        }
        private Table BuildFooterTable(Order order)
        {
            var footerContainer = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);


            var billOfLadingTable = BuildBillOfLadingFooterExact(order);
            footerContainer.AddCell(new Cell().Add(billOfLadingTable).SetBorder(Border.NO_BORDER));

            var certificationTable = BuildCertificationSection();
            footerContainer.AddCell(new Cell().Add(certificationTable).SetBorder(Border.NO_BORDER));

            return footerContainer;
        }
        private Table BuildShippingTotalsTable(Order order)
        {
            float[] headers = { 25, 15, 35, 28, 25, 15 };
            float totalQty = 0;
            float totalWeight = 0;

            foreach (OrderDetail detail in SortDetails.SortedDetails(order.Details))
            {
                if (detail.OrderDiscountId > 0)
                    continue;

                totalQty += detail.Qty;
                totalWeight += (float)Math.Round((detail.Qty * detail.Product.Weight), 2);
            }

            var table = new Table(UnitValue.CreatePercentArray(headers))
                .UseAllAvailableWidth()
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            table.AddCell(new Cell().Add(new Paragraph("Total Items Shipped:").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(totalQty.ToString("N0")).SetFontSize(16)).SetTextAlignment(TextAlignment.CENTER));

            Paragraph palletsPara = new Paragraph()
                .Add(new Text("Pallets in: ______________  \n").SetFontSize(9))
                .Add(new Text("Pallets out: ______________").SetFontSize(9));
            table.AddCell(new Cell().Add(palletsPara).SetTextAlignment(TextAlignment.LEFT));

            table.AddCell(new Cell().Add(new Paragraph("Refrigeration Unit: °F").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph("Total Shipped Weight:").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(Math.Round(totalWeight, 2).ToString(CultureInfo.InvariantCulture)).SetFontSize(16)).SetTextAlignment(TextAlignment.CENTER));

            table.SetMarginBottom(5);

            return table;
        }
        private Table BuildBillOfLadingFooterExact(Order order)
        {
           var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            Table mainTable = new Table(UnitValue.CreatePercentArray(new float[] { 100, 100, 100 })).SetBorder(Border.NO_BORDER).UseAllAvailableWidth();
            Table subTable = new Table(UnitValue.CreatePercentArray(new float[] { 150 })).UseAllAvailableWidth();

            AddCellToBody(subTable, "Remit C.O.D. To:           " , HorizontalAlignment.LEFT, Border.NO_BORDER, GetNormalFontHelvetica(false));
            AddCellToBody(subTable, "Address            ", HorizontalAlignment.LEFT, Border.NO_BORDER,GetNormalFontHelvetica(false));

            mainTable.AddCell(new Cell().Add(subTable).SetBorder(new SolidBorder(1)));
            
            subTable = new Table(UnitValue.CreatePercentArray(new float[] { 150 })).UseAllAvailableWidth();
            subTable.AddCell(new Cell()
                .Add(new Paragraph(
                        "ON COLLECT ON DELIVERY SHIPMENTS, THE LETTERS “COD” MUST APPEAR BEFORE CONSIGNEE’S NAME – OR AS OTHERWISE PROVIDED IN ITEM 430, SEC. 1.").SetTextAlignment(TextAlignment.JUSTIFIED)
                    .AddStyle(GetSmallFontHelvetica(false)).SetBorder(Border.NO_BORDER)).SetBorder(Border.NO_BORDER));
            
            var subTableCOD = new Table(UnitValue.CreatePercentArray(new float[] { 10,10 })).SetBorder(Border.NO_BORDER);
            subTableCOD.AddCell(new Cell().Add(new Paragraph("COD ").SetBorder(Border.NO_BORDER).SetFontSize(9)).SetBorder(Border.NO_BORDER)
                .AddStyle(GetNormalBoldFont()));
            subTableCOD.AddCell(new Cell().Add(new Paragraph("Amt:$").SetBorder(Border.NO_BORDER)).SetBorder(Border.NO_BORDER).SetHorizontalAlignment(HorizontalAlignment.LEFT).SetVerticalAlignment(VerticalAlignment.BOTTOM)
                .AddStyle(GetSmallFont()).SetHorizontalAlignment(HorizontalAlignment.LEFT));
            subTable.AddCell(new Cell().Add(subTableCOD).SetBorder(Border.NO_BORDER));
            
            mainTable.AddCell(new Cell().Add(subTable).SetBorder(new SolidBorder(1)));

            subTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }));
            
            AddCellToBody(subTable, "C.O.D. Fee", HorizontalAlignment.LEFT, Border.NO_BORDER, GetNormalFontHelvetica(true));

            var newSubtable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 5, 15, 5, 20 }))
                .SetBorder(Border.NO_BORDER);

            newSubtable.AddCell(new Cell().Add(new Paragraph("Prepaid").AddStyle(GetMediumFontHelvetica(false)))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            newSubtable.AddCell(new Cell().Add(BoxSquare())
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE).SetHorizontalAlignment(HorizontalAlignment.LEFT));

            newSubtable.AddCell(new Cell().Add(new Paragraph("Collect").AddStyle(GetMediumFontHelvetica(false)))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            newSubtable.AddCell(new Cell().Add(BoxSquare())
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE).SetHorizontalAlignment(HorizontalAlignment.LEFT));

            newSubtable.AddCell(new Cell().Add(new Paragraph(" $").AddStyle(GetMediumFontHelvetica(false)))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            subTable.AddCell(new Cell().Add(newSubtable)
                .SetMargin(0)
                .SetBorder(Border.NO_BORDER));

            mainTable.AddCell(new Cell().Add(subTable).SetBorder(new SolidBorder(1)));
            
            
            subTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            
            subTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("If the shipment moves between two ports by a carrier by water, the law requires that the bill of lading shall state whether it is carrier’s or shipper’s weight.").SetTextAlignment(TextAlignment.LEFT)).AddStyle(GetSmallFontHelvetica(false)));
            subTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("The fibre containers used for this shipment conform to the specifications set forth in the box maker’s certificate thereon, and all other requirements of Rule 41 of the Uniform Freight Classifications and Rule 5 of the National Motor Freight Classifications.").SetTextAlignment(TextAlignment.LEFT)).AddStyle(GetSmallFontHelvetica(false)));
            
            subTable.AddCell(new Cell().Add(new Paragraph("NOTE – Where the rate is dependent on value, shippers are required to state in writing the agreed or declared values of the property The Agreed or declared value of the property is hereby specifically stated by the shipper to be not exceeding  $     PER")).AddStyle(GetSmallFontHelvetica(false)));
            mainTable.AddCell(new Cell().Add(subTable).SetBorder(new SolidBorder(1)));

            
            subTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            
            AddCellToBody(subTable, "Subject to Section 7 of the conditions if this shipment is to be delivered to the consignees without recourse on the consignor, the consignor shall sign in the following statement:", HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));
            AddCellToBody(subTable, "The carrier shall not make delivery of this shipment without payment of freight and other lawful charges.", HorizontalAlignment.LEFT, Border.NO_BORDER, GetSmallFontHelvetica(false));
            subTable.AddCell(new Cell().Add(new Paragraph("\n")).SetBorder(Border.NO_BORDER));
            subTable.AddCell(new Cell().Add(new Paragraph("\n")).SetBorder(Border.NO_BORDER));
            subTable.AddCell(new Cell().Add(new LineSeparator(new SolidLine(1))).SetBorder(Border.NO_BORDER));
            subTable.AddCell(new Cell().Add(new Paragraph("Signature of Consignor").AddStyle(GetSmallFontHelvetica(false)).SetHorizontalAlignment(HorizontalAlignment.CENTER).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER));

            mainTable.AddCell(new Cell().Add(subTable).SetBorder(new SolidBorder(1)));

            
            subTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            subTable.AddCell(new Cell().Add(new Paragraph("Total \n Charges")).AddStyle(GetMediumFontHelvetica(true)).SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(1)).SetMargin(0));
            subTable.AddCell(new Cell().Add(BuildFreightChargesBox(normalFont, boldFont).SetBorder(Border.NO_BORDER)).SetBorder(Border.NO_BORDER).SetMaxHeight(80).SetHorizontalAlignment(HorizontalAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.BOTTOM));
            mainTable.AddCell(new Cell().Add(subTable).SetMargin(0));

            return mainTable;
        }
        private Table BuildCertificationSection()
        {
            var subtable = new Table(UnitValue.CreatePercentArray(new float[] { 100, 100 }))
                .UseAllAvailableWidth();

            subtable.AddCell(new Cell().SetPadding(5).Add(new Paragraph("This is to certify that the above named materials are properly classified, described, packaged, marked and labeled and are in proper condition for transportation according to the applicable regulations of the Department of Transportation").AddStyle(GetMediumFontHelvetica(false).SetTextAlignment(TextAlignment.JUSTIFIED))));

            var shipperTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetBorder(Border.NO_BORDER)
                .UseAllAvailableWidth();

            shipperTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Shipper: El Milagro, Inc.").AddStyle(GetMediumFontHelvetica(false))));
            shipperTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("\n")));
            shipperTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new LineSeparator(new SolidLine())));

            subtable.AddCell(new Cell().Add(shipperTable));

            return subtable;
        }
        
        private Table BuildFreightChargesBox(PdfFont normalFont, PdfFont boldFont)
        {
            Table freightBox = new Table(UnitValue.CreatePercentArray(new float[] { 110 }))
                    .UseAllAvailableWidth();

           
            freightBox.AddCell(new Cell()
                .Add(new Paragraph("Freight Charges")
                    .SetFont(boldFont)
                    .SetFontSize(10))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
            );
            freightBox.AddCell(new Cell()
                .Add(new Paragraph("FREIGHT PREPAID")
                    .SetFont(normalFont)
                    .SetFontSize(9))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
            );
          
            Table innerRow = new Table(UnitValue.CreatePercentArray(new float[] { 50, 15, 50 }))
                .UseAllAvailableWidth();
            // Left text
            innerRow.AddCell(new Cell()
                .Add(new Paragraph("Except when box at right is checked").AddStyle(GetSmallFontHelvetica(false))
                ).SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(Border.NO_BORDER)
                .SetSpacingRatio(0.2f)
            );
           
            innerRow.AddCell(BoxSquare());
          

            innerRow.AddCell(new Cell()
                .Add(new Paragraph("Check box if charges are to be collected")
                    .AddStyle(GetSmallFontHelvetica(false)))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetSpacingRatio(0.2f)
                .SetBorder(Border.NO_BORDER)
            );


            freightBox.AddCell(new Cell()
                .Add(innerRow)
                .SetBorder(Border.NO_BORDER)
            );

            return freightBox;
        }

        private Cell BoxSquare()
        {
            Cell boxCell = new Cell().SetBorder(Border.NO_BORDER);
            boxCell.SetPadding(5);

            Div squareDiv = new Div()
                .SetHeight(9)
                .SetWidth(9)
                .SetBorder(new SolidBorder(1));

            boxCell.Add(squareDiv);
            return boxCell;
        }
    }

    public class PdfFooterEventHandler : AbstractPdfDocumentEventHandler
    {
        private Document _document;
        private Order _order;
        private Func<Order, Table> _footerBuilder;

        public PdfFooterEventHandler(Document doc, Order order, Func<Order, Table> footerBuilder)
        {
            _document = doc;
            _order = order;
            _footerBuilder = footerBuilder;
        }

        protected override void OnAcceptedEvent(AbstractPdfDocumentEvent currentEvent)
        {
            var docEvent = (PdfDocumentEvent)currentEvent;
            var pdfDoc = docEvent.GetDocument();
            var page = docEvent.GetPage();
            var pageSize = page.GetPageSize();

            if (pdfDoc.GetPageNumber(page) != pdfDoc.GetNumberOfPages())
                return;
            PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
            Canvas canvas = new Canvas(pdfCanvas, pageSize);
            // Generar la tabla
            Table footerTable = _footerBuilder(_order);
            footerTable.SetWidth(pageSize.GetWidth() - 50);

            footerTable.SetFixedPosition(
                pdfDoc.GetPageNumber(page),
                pageSize.GetLeft() + 25,
                pageSize.GetBottom() + 15,
                pageSize.GetWidth() - 50
            );

            canvas.Add(footerTable);
            canvas.Close();
        }
    }
}