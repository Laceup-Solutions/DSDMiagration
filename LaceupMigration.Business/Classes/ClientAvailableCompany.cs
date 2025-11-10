using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaceupMigration
{
    public class ClientAvailableCompany
    {
        public int ClientId { get; set; }

        public int CompanyId { get; set; }

        CompanyInfo company;
        public CompanyInfo Company
        {
            get
            {
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == CompanyId);
                return company;
            }
        }

        public string Extrafields { get; set; }

        static List<ClientAvailableCompany> list = new List<ClientAvailableCompany>();
        public List<ClientAvailableCompany> List
        {
            get
            {
                return list;
            }
        }

        public static void Add(ClientAvailableCompany x)
        {
            list.Add(x);
        }

        public static void Add(int clientId, int companyId, string extrafields)
        {
            list.Add(new ClientAvailableCompany() { ClientId = clientId, CompanyId = companyId, Extrafields = extrafields });
        }

        public static void Clear()
        {
            list = new List<ClientAvailableCompany>();
        }

        public static List<CompanyInfo> GetCompanyInfoList(int clientId)
        {
            var result = list.Where(x => x.ClientId == clientId && x.Company != null).Select(x => x.Company).ToList();

            if (result.Count == 0)
            {
                if (CompanyInfo.Companies.Count == 0)
                    CompanyInfo.Companies.Add(CompanyInfo.CreateDefaultCompany());

                return CompanyInfo.Companies.ToList();
            }

            return result.Where(x => x != null).ToList();
        }
    }
}