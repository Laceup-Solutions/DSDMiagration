





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountClientArea
    {
        public static List<OrderDiscountClientArea> List = new List<OrderDiscountClientArea>();

        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public int AreaId { get; set; }
        public string ExtraFields { get; set; }

        public virtual Area Area
        {
            get
            {
                return Area.List.FirstOrDefault(x => x.Id == AreaId);
            }
        }
        public virtual OrderDiscount OrderDiscount
        {
            get
            {
                return OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
            }
        }
    }
}