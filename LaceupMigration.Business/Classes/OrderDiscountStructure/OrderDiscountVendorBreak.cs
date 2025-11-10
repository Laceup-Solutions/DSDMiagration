





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountVendorBreak
    {
        public int Id { get; set; }
        public int BreakId { get; set; }
        public int VendorId { get; set; }
        public string ExtraFields { get; set; }

        public virtual OrderDiscountBreak OrderDiscountBreak { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}