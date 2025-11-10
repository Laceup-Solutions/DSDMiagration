using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class LaceupQRDecoder : BarcodeDecoder
    {
        public const string ProductIdKey = "ProductIdKey";
        public const string LotKey = "LotKey";
        public const string QtyKey = "QtyKey";
        public const string UoMIdKey = "UoMIdKey";
        public const string WeightKey = "WeightKey";
        public const string LotExpKey = "LotExpKey";

        public LaceupQRDecoder(string s) : base(s)
        {
            try
            {
                var parts = s.Split('|');

                foreach (var item in parts)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    var pp = item.Split('=');

                    var key = pp[0].ToLower();
                    var value = pp[1];

                    if (key == ProductIdKey.ToLower())
                    {
                        var ProductId = Convert.ToInt32(value);
                        Product = Product.Find(ProductId);
                    }
                    else if (key == LotKey.ToLower())
                        Lot = value;
                    else if (key == QtyKey.ToLower())
                        Qty = Convert.ToSingle(value);
                    else if (key == UoMIdKey.ToLower())
                        UoMId = Convert.ToInt32(value);
                    else if (key == WeightKey.ToLower())
                        Weight = Convert.ToSingle(value);
                    else if (key == LotExpKey.ToLower())
                        Expiration = new DateTime(Convert.ToInt64(value));
                }
            }
            catch(Exception ex)
            {
                Logger.CreateLog("Error Parsing " + s + " in LaceupQRDecoder");
                Logger.CreateLog(ex);
            }
        }
    }
}