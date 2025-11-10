





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class CustomerDiscount
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int DiscountType { get; set; }
        public double DiscountValue { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string Name { get; set; }
        public Nullable<double> Qty { get; set; }
        public string ExtraFields { get; set; }
        public string Comments { get; set; }
        public Nullable<int> Type { get; set; }
        public int Status { get; set; }

        public virtual Client Client
        {
            get
            {
                return Client.Find(ClientId);
            }
        }
    }
}