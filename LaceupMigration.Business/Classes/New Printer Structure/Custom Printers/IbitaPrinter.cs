using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class IbitaPrinter : ZebraFourInchesPrinter1
    {
        int ibitta_font45Separation = 50;
        int ibitta_font40Separation = 45;
        int ibitta_font30Separation = 35;
        int ibitta_font35Separation = 40;

        protected const string IbittaContractCompany = "IbittaContractCompany";
        protected const string IbittaContractCompanyAddr = "IbittaContractCompanyAddr";
        protected const string IbittaContractCompanyPhoneAndFax = "IbittaContractCompanyPhoneAndFax";
        protected const string IbittaContractCompanyEmail = "IbittaContractCompanyEmail";
        protected const string IbittaContractOrderNum = "IbittaContractOrderNum";
        protected const string IbittaContractShipDate = "IbittaContractShipDate";
        protected const string IbittaContractSeller = "IbittaContractSeller";
        protected const string IbittaContractFrom = "IbittaContractFrom";
        protected const string IbittaContractTo = "IbittaContractTo";

        protected const string IbittaTableHeader = "IbittaTableHeader";
        protected const string IbittaTableLine = "IbittaTableLine";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(IbittaContractCompany, "^CF0,45^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(IbittaContractCompanyAddr, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(IbittaContractCompanyPhoneAndFax, "^CF0,25^FO40,{0}^FDP. {1} | F. {2}^FS");
            linesTemplates.Add(IbittaContractCompanyEmail, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(IbittaContractOrderNum, "^CF0,30^FO40,{0}^FDOrder #: {1}^FS");
            linesTemplates.Add(IbittaContractShipDate, "^CF0,30^FO40,{0}^FDTransaction Date: {1}^FS");
            linesTemplates.Add(IbittaContractSeller, "^CF0,30^FO40,{0}^FDSeller: {1}^FS");
            linesTemplates.Add(IbittaContractFrom, "^CF0,30^FO40,{0}^FDFrom: {1}^FS");
            linesTemplates.Add(IbittaContractTo, "^CF0,30^FO40,{0}^FDTo: {1}^FS");

            linesTemplates.Add(IbittaTableHeader, "^CF0,30" +
                "^FO40,{0}^FD#^FS" +
                "^FO100,{0}^FDItem^FS" +
                "^FO620,{0}^FDQty^FS");
            linesTemplates.Add(IbittaTableLine, "^CF0,25" +
                "^FO40,{0}^FD{1}^FS" +
                "^FO100,{0}^FD{2}^FS" +
                "^FO620,{0}^FD{3}^FS");
        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            if (order.IsScanBasedTrading)
                return PrintFullConsignment(order, asPreOrder);
            return base.PrintOrder(order, asPreOrder);
        }

        public override bool PrintFullConsignment(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            startY += font36Separation;

            lines.AddRange(GetConsignmentHeaderLines(ref startY, order));

            startY += 30;

            lines.AddRange(GetConsignmentContractTable(ref startY, order));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected override IEnumerable<string> GetConsignmentHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            //Add the company details rows.
            lines.AddRange(GetCompanyRows(ref startY, order));

            startY += 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractOrderNum], startY, order.PrintedOrderId));
            startY += ibitta_font30Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractShipDate], startY, order.Date.ToShortDateString()));
            startY += ibitta_font30Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractSeller], startY, Config.VendorName));
            startY += ibitta_font30Separation;

            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractFrom], startY, "Main Ibitta"));
            startY += ibitta_font30Separation;

            startY += 10;

            var company = CompanyInfo.SelectedCompany;

            foreach (string part in CompanyNameSplit(company.CompanyName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startY, part));
                startY += ibitta_font30Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startY, company.CompanyAddress1));
            startY += ibitta_font30Separation;

            if (company.CompanyAddress2.Trim().Length > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startY, company.CompanyAddress2));
                startY += ibitta_font30Separation;
            }

            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractTo], startY, order.Client.ClientName));
            startY += ibitta_font40Separation;

            startY += 10;

            foreach (string s1 in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startY, s1.Trim()));
                startY += ibitta_font30Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();

                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompany], startIndex, part));
                    startIndex += ibitta_font45Separation;
                }

                startIndex += 25;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startIndex, company.CompanyAddress1));
                startIndex += ibitta_font30Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyAddr], startIndex, company.CompanyAddress2));
                    startIndex += ibitta_font30Separation;
                }

                var phone = company.CompanyPhone;
                var fax = !string.IsNullOrEmpty(company.CompanyFax) ? company.CompanyFax : "";

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyPhoneAndFax], startIndex, phone, fax));
                startIndex += ibitta_font30Separation;

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaContractCompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += ibitta_font30Separation;
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected override IEnumerable<string> GetConsignmentContractTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaTableHeader], startY));
            startY += ibitta_font30Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int i = 0;
            foreach (var detail in SortDetails.SortedDetails(order.Details))
            {
                int index = 0;
                i++;

                var productSlices = GetConsContractDetailRowsSplitProductName(detail.Product.Name);
                foreach (var part in productSlices)
                {
                    if (index == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaTableLine], startY,
                            i,
                            part,
                            detail.Qty.ToString() + " ea"));
                        startY += ibitta_font30Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[IbittaTableLine], startY,
                            "",
                            part,
                            ""));
                        startY += ibitta_font30Separation;
                    }
                    else
                        break;
                    index++;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
        }
    }
}