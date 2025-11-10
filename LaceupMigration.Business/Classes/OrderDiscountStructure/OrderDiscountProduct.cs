





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountProduct
    {
        public static List<OrderDiscountProduct> List = new List<OrderDiscountProduct>();

        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public int ProductId { get; set; }
        public string ExtraFields { get; set; }

        public virtual OrderDiscount OrderDiscount
        {
            get
            {
                return OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
            }
        }
        public virtual Product Product
        {
            get
            {
                return Product.Find(ProductId);
            }
        }
    }
}