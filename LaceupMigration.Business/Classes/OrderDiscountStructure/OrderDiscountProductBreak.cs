





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountProductBreak
    {
        public int Id { get; set; }
        public int BreakId { get; set; }
        public int ProductId { get; set; }
        public string ExtraFields { get; set; }

        public virtual OrderDiscountBreak OrderDiscountBreak { get; set; }
        public virtual Product Product { get; set; }
    }
}