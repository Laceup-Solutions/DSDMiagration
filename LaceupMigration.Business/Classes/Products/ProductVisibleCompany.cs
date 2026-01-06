





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ProductVisibleCompany
    {
        public static List<ProductVisibleCompany> List = new List<ProductVisibleCompany>();

        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int ProductId { get; set; }

        public string ExtraFields { get; set; }

        public Product Product
        {
            get
            {
                return Product.Find(ProductId);
            }
        }

        public CompanyInfo Company
        {
            get
            {
                return CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == CompanyId);
            }
        }

        public static void Clear()
        {
            List.Clear();
        }
    }
}