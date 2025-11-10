





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration 
{ 
    public class OrderDiscountClientPriceLevel
    {
        public static List<OrderDiscountClientPriceLevel> List = new List<OrderDiscountClientPriceLevel>();
        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int PriceLevelId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public string ExtraFields { get; set; }

        public OrderDiscount OrderDiscount
        {
            get => OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
        }
        public PriceLevel PriceLevel
        {
            get => PriceLevel.List.FirstOrDefault(x => x.Id == PriceLevelId);
        }
    }
}