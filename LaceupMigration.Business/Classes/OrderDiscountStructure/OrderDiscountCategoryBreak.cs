





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscountCategoryBreak
    {
        public int Id { get; set; }
        public int BreakId { get; set; }
        public int CategoryId { get; set; }
        public string ExtraFields { get; set; }

        public virtual Category Category { get; set; }
        public virtual OrderDiscountBreak OrderDiscountBreak { get; set; }
    }
}