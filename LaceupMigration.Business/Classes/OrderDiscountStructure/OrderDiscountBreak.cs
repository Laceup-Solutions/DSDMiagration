





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountBreak
    {
        public OrderDiscountBreak()
        {
            this.OrderDiscountProductBreaks = new HashSet<OrderDiscountProductBreak>();
            this.OrderDiscountVendorBreaks = new HashSet<OrderDiscountVendorBreak>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderDiscountCategoryBreaks = new HashSet<OrderDiscountCategoryBreak>();
        }

        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public double MinQty { get; set; }
        public double MaxQty { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<double> QtySelectProduct { get; set; }
        public string ExtraFields { get; set; }
        public int DiscountType { get; set; }

        public bool FixPrice { get; set; }
        public virtual OrderDiscount OrderDiscount { get; set; }
        public virtual ICollection<OrderDiscountProductBreak> OrderDiscountProductBreaks { get; set; }
        public virtual ICollection<OrderDiscountVendorBreak> OrderDiscountVendorBreaks { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderDiscountCategoryBreak> OrderDiscountCategoryBreaks { get; set; }

    }
}