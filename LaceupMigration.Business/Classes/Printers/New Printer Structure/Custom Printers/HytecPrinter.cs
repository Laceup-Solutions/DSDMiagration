using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class HytecPrinter : AdvanceBatteryPrinter
    {
        public override string GetProductValue(Product product)
        {
            return product.Name;
        }
    }
}