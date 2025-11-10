





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class Vendor
    {
        public static List<Vendor> List = new List<Vendor>();
        public Vendor()
        {
            this.Products = new HashSet<Product>();
            this.OrderDiscountVendorBreaks = new HashSet<OrderDiscountVendorBreak>();
            this.OrderDiscountVendors = new HashSet<OrderDiscountVendor>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalId { get; set; }
        public string Comments { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public double OpenBalance { get; set; }
        public double CreditLimit { get; set; }
        public string ExtraFields { get; set; }
        public string UniqueId { get; set; }
        public string NonVisibleExtraFields { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual Term Term { get; set; }
        public virtual ICollection<OrderDiscountVendorBreak> OrderDiscountVendorBreaks { get; set; }
        public virtual ICollection<OrderDiscountVendor> OrderDiscountVendors { get; set; }
    }
}