





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountClient
    {
        public static List<OrderDiscountClient> List = new List<OrderDiscountClient>();
        public int Id { get; set; }
        public int OrderDiscountId { get; set; }
        public int DiscountType { get; set; }
        public double Buy { get; set; }
        public double Qty { get; set; }
        public int ClientId { get; set; }
        public string ExtraFields { get; set; }

        public virtual Client Client
        {
            get
            {
                return Client.Find(ClientId);
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