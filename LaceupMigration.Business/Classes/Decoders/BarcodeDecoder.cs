using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BarcodeDecoder
    {
        public string UPC { get; set; }

        public Product Product { get; set; }

        public string Lot { get; set; }

        public DateTime Expiration { get; set; }

        public float Qty { get; set; }

        public float Weight { get; set; }

        public int UoMId { get; set; }

        public string Data { get; set; }
        
        public int LabelId { get; set; }

        public BarcodeDecoder(string s)
        {
            Data = s;
        }

        public static BarcodeDecoder CreateDecoder(string s)
        {
            if (Config.BarcodeDecoder == 1)
                return new OldLaceupQRDecoder(s);
            else if (Config.BarcodeDecoder == 2)
                return new LaceupQRDecoder(s);
            else if (Config.BarcodeDecoder == 3)
                return new GS1Decoder(s);
            else if(Config.BarcodeDecoder == 4)
                return new LaHaciendaDecoder(s);
            else if (Config.BarcodeDecoder == 5)
                return new GS1_128Decoder(s);
            else if (Config.BarcodeDecoder == 6)
                return new PraseksDecoder(s);
            else if (Config.BarcodeDecoder == 7)
                return new LaceupProductLabelDecoder(s);
            
            return new BarcodeDecoder(s);
        }
    }
}