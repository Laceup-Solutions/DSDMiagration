





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountVendor
    {
        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public int VendorId { get; set; }
        public string ExtraFields { get; set; }

        public virtual OrderDiscount OrderDiscount { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}