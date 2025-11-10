using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;







using System.Globalization;

namespace LaceupMigration
{
    public class SensationPrinter : MagnoliaPrinter
    {
        protected const string SensationFooterText = "SensationFooterText";
        protected const string SensationTableHeader = "SensationTableHeader";
        protected const string SensationTableLine = "SensationTableLine";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(SensationTableHeader, "^FO40,{0}^ABN,18,10^FDDescription^FS" +
                "^FO645,{0}^ABN,18,10^FDUoM^FS" +
                "^FO720,{0}^ABN,18,10^FDQty^FS");
            linesTemplates.Add(SensationTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO645,{0}^ADN,18,10^FD{2}^FS" +
                "^FO720,{0}^ADN,18,10^FD{3}^FS");
            
            linesTemplates.Add(SensationFooterText, "^FO500,{0}^ABN,20,12^FDTotal: {1}^FS"); 
        }

        protected override IList<string> GetCompanyRows(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startY += Config.CompanyLogoHeight;
            }

            startY += 36;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyName], startY, FixCompanyName(CompanyInfo.Companies[0].CompanyName)));
            startY += 50;

            var address = CompanyInfo.Companies[0].CompanyAddress1;
            if (!string.IsNullOrEmpty(CompanyInfo.Companies[0].CompanyAddress2))
                address += " " + CompanyInfo.Companies[0].CompanyAddress2;

            if (address.Length < 43)
            {
                var l = address.Length;
                address = new string(' ', (43 - l) / 2) + address;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyAddr1], startY, address));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyContact], startY, CompanyInfo.Companies[0].CompanyPhone, CompanyInfo.Companies[0].CompanyFax));
            startY += 40;

            return lines;
        }

        protected override  IList<string> GetDetailsTable(ref int startY, string tableheader, IList<OrderLine> details)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInvoiceHeader], startY, tableheader));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaDotLineTable], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SensationTableHeader], startY));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaDotLineTable], startY));
            startY += 30;

            foreach (var item in details)
            {
                var upc = item.Product.Upc;

                var prodNameSections = SplitDetailProductName(item.Product.Name);

                string price = item.Price.ToCustomString();

                var uomLabel = string.Empty;
                if (item.OrderDetail.UnitOfMeasure != null)
                    uomLabel = item.OrderDetail.UnitOfMeasure.Name;

                for (int i = 0; i < prodNameSections.Count(); i++)
                {
                    string section = prodNameSections[i];

                    if (i == 0)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SensationTableLine], startY,
                            section, uomLabel, item.Qty));
                    else if (!Config.PrintTruncateNames)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SensationTableLine], startY,
                            section, string.Empty, string.Empty));

                    startY += 25;
                }

                startY += 5;
            }

            startY += 70;

            return lines;
        }

        protected override IList<string> SplitDetailProductName(string name)
        {
            return SplitProductName(name, 48, 48);
        }

        protected string FixCompanyName(string name)
        {
            if(name.Length < 12)
                name = new string(' ', (int)((12 - name.Length)/2)) + name;

            return name;
        }

        protected override IList<string> GetFooterLines(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            var totalUnits = GetTotalUnits(order);
            var totalCost = order.OrderTotalCost();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SensationFooterText], startY, totalCost.ToCustomString()));

            startY += 100;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.Add(IncludeSignature(order, lines, ref startY));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaSignatureDotLine], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaSignatureHeader], startY));
            startY += 80;

            foreach (var item in SplitFooterText(Config.BottomOrderPrintText))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaFooterText], startY, item));
                startY += 18;
            }

            startY += 12;

            return lines;
        }

    }
}