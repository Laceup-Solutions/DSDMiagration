
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class LaHaciendaDecoder : BarcodeDecoder
    {
        public LaHaciendaDecoder(string s) : base(s)
        {
            var parts = s.Split('/');

            if (parts.Length > 0)
            {
                var code = parts[0];
                Product = Product.Products.FirstOrDefault(x => x.CategoryId > 0 && x.Sku == code);
            }

            if (parts.Length > 1)
                Lot = parts[1];

            if (parts.Length > 2)
                Weight = Convert.ToSingle(parts[2]);

            Qty = 1;
        }
    }
}