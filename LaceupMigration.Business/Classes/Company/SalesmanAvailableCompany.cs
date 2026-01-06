using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaceupMigration
{
    public class SalesmanAvailableCompany
    {
        public int SalesmanId { get; set; }

        public int CompanyId { get; set; }

        public string Extrafields { get; set; }

        static List<SalesmanAvailableCompany> list = new List<SalesmanAvailableCompany>();
        public List<SalesmanAvailableCompany> List { get { return list; } }

        public static void Add(int salesmanId, int companyId, string extrafields)
        {
            list.Add(new SalesmanAvailableCompany() { SalesmanId = salesmanId, CompanyId = companyId, Extrafields = extrafields });
        }

        public static void Clear()
        {
            list = new List<SalesmanAvailableCompany>();
        }

        public static List<CompanyInfo> GetCompanies(int salesmanId, int clientId)
        {
            var availableCompaniesForSalesman = list.Where(x => x.SalesmanId == salesmanId).Select(x => x.CompanyId).ToList();

            if (availableCompaniesForSalesman.Count == 0)
                return ClientAvailableCompany.GetCompanyInfoList(clientId);

            return ClientAvailableCompany.GetCompanyInfoList(clientId).Where(x => availableCompaniesForSalesman.Contains(x.CompanyId)).ToList();
        }
    }
}