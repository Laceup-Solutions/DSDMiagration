using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class SiteInventoryEx 
    {
        public int ProductId { get; set; }
        string lot = "";
        public string Lot { get { return lot; } set { lot = value.ToLowerInvariant(); } }
        public double Qty { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ExtraFields { get; set; }

        public string MfgLot { get; set; }
        public DateTime MfgDate { get; set; }

        public SiteEx Site { get; set; }

        Product product;
        public Product Product
        {
            get
            {
                if (product == null)
                    product = Product.Find(ProductId);
                return product;
            }
        }

        public SiteInventoryEx()
        {
            Lot = "";
            ExtraFields = "";
            MfgLot = "";
        }
    }
}