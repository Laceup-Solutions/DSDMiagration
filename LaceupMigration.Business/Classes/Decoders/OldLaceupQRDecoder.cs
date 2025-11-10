
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OldLaceupQRDecoder : BarcodeDecoder
    {
        public OldLaceupQRDecoder(string s) : base(s)
        {
            try
            {
                if (Config.ScanDeliveryChecking)
                    return;

                var parts = s.Split('|');
                var ProductId = Convert.ToInt32(parts[2]);
                Lot = parts[3];
                UoMId = Convert.ToInt32(parts[6]);
                Qty = Convert.ToSingle(parts[7]);
                if (parts.Length > 9)
                    Expiration = new DateTime(Convert.ToInt64(parts[9]));

                Product = Product.Find(ProductId);
            }
            catch(Exception ex)
            {
                Logger.CreateLog("Error Parsing " + s + " in OldLaceupQRDecoder");
                Logger.CreateLog(ex);
            }
        }
    }
}