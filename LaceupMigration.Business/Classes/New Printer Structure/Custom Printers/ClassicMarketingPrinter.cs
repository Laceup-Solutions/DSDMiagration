






using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    internal class ClassicMarketingPrinter : ZebraFourInchesPrinter1
    {
        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (order.CompanyId > 0)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;


                bool isSplitOrder = false;

                if (!string.IsNullOrEmpty(order.ExtraFields) && order.ExtraFields.ToLower().Contains("split=1"))
                    isSplitOrder = true;

                //static variables -> might change later to a config or something
                string split_companyName = "Hummer & Son's Honey Farm";
                string split_address1 = "287 Sligo Rd";
                string split_address2 = "Bossier City, LA 71112";
                string split_phone = "(318) 742-3541";

                List<string> list = new List<string>();

                if (isSplitOrder)
                {
                    foreach (string part in CompanyNameSplit(split_companyName))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                        startIndex += font36Separation;
                    }

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, split_address1));
                    startIndex += font18Separation;

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, split_address2));
                    startIndex += font18Separation;


                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startIndex, split_phone));
                    startIndex += font18Separation;
                }
                else
                {
                    foreach (string part in CompanyNameSplit(company.CompanyName))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                        startIndex += font36Separation;
                    }
                    // startIndex += font36Separation;
                    if (company.CompanyAddress1.Trim().Length > 0)
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress1));
                        startIndex += font18Separation;
                    }

                    if (company.CompanyAddress2.Trim().Length > 0)
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress2));
                        startIndex += font18Separation;
                    }

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startIndex, company.CompanyPhone));
                    startIndex += font18Separation;

                    if (!string.IsNullOrEmpty(company.CompanyFax))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, company.CompanyFax));
                        startIndex += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(company.CompanyEmail))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                        startIndex += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(company.CompanyLicenses))
                    {
                        var licenses = company.CompanyLicenses.Split(',').ToList();

                        for (int i = 0; i < licenses.Count; i++)
                        {
                            var format = i == 0 ? CompanyLicenses1 : CompanyLicenses2;

                            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startIndex, licenses[i]));
                            startIndex += font18Separation;
                        }
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }
    }
}