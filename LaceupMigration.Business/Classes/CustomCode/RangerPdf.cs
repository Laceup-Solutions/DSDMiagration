using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using iText.Commons.Actions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Event;


namespace LaceupMigration
{
    class RangerPdf : DefaultPdfProvider
    {
        public static readonly PdfNumber PORTRAIT = new PdfNumber(0);
        public static readonly PdfNumber LANDSCAPE = new PdfNumber(90);
        public static readonly PdfNumber INVERTEDPORTRAIT = new PdfNumber(180);
        public static readonly PdfNumber SEASCAPE = new PdfNumber(270);

        private class PageOrientationsEventHandler : IEventHandler
        {
            private PdfNumber orientation = PORTRAIT;

            public void SetOrientation(PdfNumber orientation)
            {
                this.orientation = orientation;
            }

            public void OnEvent(IEvent @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                docEvent.GetPage().Put(PdfName.Rotate, orientation);
            }
        }

        public override string GetOrderPdf(Order order)
        {
            string name = string.Format("Order History - {0} - {1}.pdf", order.Client.ClientName, order.PrintedOrderId ?? order.OrderId.ToString());
            string filePath = name.Replace('#', ' ');
            string fullPath = Path.Combine(Config.LaceupStorage, filePath);

            string targetFile = fullPath;

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            PdfWriter write = new PdfWriter(targetFile);
            PdfDocument pdf = new PdfDocument(write);
            
            // PageOrientationsEventHandler eventHandler = new PageOrientationsEventHandler();
            // pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, eventHandler);

            Document doc = new Document(pdf);

            AddContentToPDF(doc, order);

            doc.Close();

            return targetFile;
        }

        protected override void AddContentToPDF(Document doc, Order order)
        {
            AddCompanyInfo(doc, order);

            var history = ParLevelHistory.Histories.
                Where(x => x.Client.ClientId == order.Client.ClientId).ToList();

            var pars = ClientDailyParLevel.List.Where(x => x.ClientId == order.Client.ClientId && x.MatchDayOfWeek(DateTime.Now.DayOfWeek));

            if (!order.Client.OneOrderPerDepartment)
            {
                history = history.Where(x => x.Department == order.DepartmentUniqueId).ToList();
                pars = pars.Where(x => x.Department == order.DepartmentUniqueId);
            }

            List<T> list = new List<T>();

            DateTime firstVisit = DateTime.MinValue;
            DateTime secondVisit = DateTime.MinValue;
            DateTime thirdVisit = DateTime.Now;

            history = history.OrderByDescending(x => x.Date).ToList();

            //get past 2 visits
            foreach (var h in history)
            {
                if (secondVisit == DateTime.MinValue)
                    secondVisit = h.Date;
                else if (firstVisit == DateTime.MinValue && secondVisit.CompareTo(h.Date) != 0)
                {
                    firstVisit = h.Date;
                    break;
                }
            }

            foreach (var h in history)
            {
                if (firstVisit.CompareTo(h.Date) > 0)
                    break;

                var t = list.FirstOrDefault(x => x.Product.ProductId == h.Product.ProductId && x.Department == h.Department);
                if (t == null)
                {
                    t = new T(h.Product, h.Department, firstVisit, secondVisit, thirdVisit);
                    list.Add(t);
                }
                t.Add(h.Date, h.Counted, h.Sold);
            }


            foreach (var par in pars)
            {
                var t = list.FirstOrDefault(x => x.Product.ProductId == par.Product.ProductId && x.Department == par.Department);
                if (t == null)
                {
                    t = new T(par.Product, par.Department, firstVisit, secondVisit, thirdVisit);
                    list.Add(t);
                }
                t.Add(thirdVisit, par.Counted, par.Sold);
            }

            AddDataTable(doc, order, list, firstVisit, secondVisit, thirdVisit);
        }

        class T
        {
            public Product Product { get; set; }
            public string Department { get; set; }

            public TT FirstVisit { get; set; }
            public TT SecondVisit { get; set; }
            public TT ThirdVisit { get; set; }

            public T(Product p, string dep, DateTime fVisit, DateTime sVisit, DateTime tVisit)
            {
                Product = p;
                Department = dep;
                FirstVisit = new TT() { Date = fVisit };
                SecondVisit = new TT() { Date = sVisit };
                ThirdVisit = new TT() { Date = tVisit };
            }

            public void Add(DateTime date, float level, float ordered)
            {
                TT t = null;

                if (FirstVisit.Date.CompareTo(date) == 0)
                {
                    FirstVisit.Visited = true;
                    t = FirstVisit;
                }
                else if (SecondVisit.Date.CompareTo(date) == 0)
                {
                    SecondVisit.Visited = true;
                    t = SecondVisit;
                }
                else if (ThirdVisit.Date.CompareTo(date) == 0)
                {
                    ThirdVisit.Visited = true;
                    t = ThirdVisit;
                }

                if (t != null)
                {
                    t.Level = level;
                    t.Ordered = ordered;
                }
            }
        }

        class TT
        {
            public DateTime Date { get; set; }
            public float Level { get; set; }
            public float Ordered { get; set; }
            public bool Visited { get; set; }
        }

        protected void AddCompanyInfo(Document doc, Order order)
        {
            var company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, company);
                return;
            }

            float[] headers = { 40, 15, 15, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToBody(tableLayout, "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableLayout, "Customer: " + order.Client.ClientName, HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableLayout, "Order ID: " + order.PrintedOrderId ?? "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableLayout, "Cust PO: " + order.PONumber ?? "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableLayout, "Req Date: " + (order.ShipDate != DateTime.MinValue ? order.ShipDate.ToShortDateString() : ""), HorizontalAlignment.LEFT, Border.NO_BORDER);

            doc.Add(tableLayout);
        }

        private void AddDataTable(Document doc, Order order, List<T> list, DateTime fVisit, DateTime sVisit, DateTime tVisit)
        {
            float[] headers = { 8, 8, 30, 18, 18, 18 };  //Header Widths

            Table tableHeaderLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToBody(tableHeaderLayout, "Cust Part #", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableHeaderLayout, "RDI #", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableHeaderLayout, "Description", HorizontalAlignment.LEFT, Border.NO_BORDER);

            AddCellToBody(tableHeaderLayout, fVisit != DateTime.MinValue ? fVisit.ToString() : "", HorizontalAlignment.CENTER, Border.NO_BORDER);
            AddCellToBody(tableHeaderLayout, sVisit != DateTime.MinValue ? sVisit.ToString() : "", HorizontalAlignment.CENTER, Border.NO_BORDER);
            AddCellToBody(tableHeaderLayout, tVisit != DateTime.MinValue ? tVisit.ToString() : "", HorizontalAlignment.CENTER, Border.NO_BORDER);

            doc.Add(tableHeaderLayout);

            float[] headersDetails = { 8, 8, 30, 9, 9, 9, 9, 9, 9 };  //Header Widths

            Table tableDetailsLayout = new Table(UnitValue.CreatePercentArray(headersDetails)).UseAllAvailableWidth();

            AddCellToBody(tableDetailsLayout, "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableDetailsLayout, "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableDetailsLayout, "", HorizontalAlignment.LEFT, Border.NO_BORDER);

            AddCellToBody(tableDetailsLayout, fVisit != DateTime.MinValue ? "Level" : "", HorizontalAlignment.RIGHT, Border.NO_BORDER);
            AddCellToBody(tableDetailsLayout, fVisit != DateTime.MinValue ? "Ordered" : "", HorizontalAlignment.RIGHT, Border.NO_BORDER);

            AddCellToBody(tableDetailsLayout, sVisit != DateTime.MinValue ? "Level" : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableDetailsLayout, sVisit != DateTime.MinValue ? "Ordered" : "", HorizontalAlignment.LEFT, Border.NO_BORDER);

            AddCellToBody(tableDetailsLayout, tVisit != DateTime.MinValue ? "Level" : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            AddCellToBody(tableDetailsLayout, tVisit != DateTime.MinValue ? "Ordered" : "", HorizontalAlignment.LEFT, Border.NO_BORDER);

            foreach (var item in list.OrderBy(x => x.Product.Name))
            {
                AddCellToBody(tableDetailsLayout, item.Product.GetPartNumberForCustomer(order.Client) ?? "", HorizontalAlignment.LEFT, Border.NO_BORDER);
                AddCellToBody(tableDetailsLayout, item.Product.Code ?? "", HorizontalAlignment.LEFT, Border.NO_BORDER);
                AddCellToBody(tableDetailsLayout, item.Product.Description, HorizontalAlignment.LEFT, Border.NO_BORDER);

                AddCellToBody(tableDetailsLayout, fVisit != DateTime.MinValue ? item.FirstVisit.Level.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
                AddCellToBody(tableDetailsLayout, fVisit != DateTime.MinValue ? item.FirstVisit.Ordered.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);

                AddCellToBody(tableDetailsLayout, sVisit != DateTime.MinValue ? item.SecondVisit.Level.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
                AddCellToBody(tableDetailsLayout, sVisit != DateTime.MinValue ? item.SecondVisit.Ordered.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);

                AddCellToBody(tableDetailsLayout, tVisit != DateTime.MinValue ? item.ThirdVisit.Level.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
                AddCellToBody(tableDetailsLayout, tVisit != DateTime.MinValue ? item.ThirdVisit.Ordered.ToString() : "", HorizontalAlignment.LEFT, Border.NO_BORDER);
            }

            doc.Add(tableDetailsLayout);
        }

    }
}