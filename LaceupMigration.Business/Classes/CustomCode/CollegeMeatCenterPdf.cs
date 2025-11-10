using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Globalization;
using System.IO;




using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.Kernel.Font;


namespace LaceupMigration
{
    class CollegeMeatCenterPdf : DefaultPdfProvider
    {
        protected override void AddContentToPDF(Document doc, Order order)
        {
            AddTextLine(doc, "Invoice", GetBigFont(), TextAlignment.RIGHT);

            var company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                Image jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));
                jpg.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                jpg.ScaleToFit(90f, 75f);
                jpg.SetPaddingLeft(9f);

                doc.Add(jpg);
            }
        }
    }
}