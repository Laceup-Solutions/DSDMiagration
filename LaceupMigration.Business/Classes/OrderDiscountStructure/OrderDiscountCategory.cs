





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountCategory
    {
        public static List<OrderDiscountCategory> List = new List<OrderDiscountCategory>();
        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public int CategoryId { get; set; }
        public int CategoryType { get; set; }
        public string ExtraFields { get; set; }

        public OrderDiscount OrderDiscount 
        {
            get
            {
                return OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
            }
        }
    }
}