using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class FirstAidPrinter : ZebraThreeInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^FO15,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO275,{0}^ADN,18,10^FDQTY^FS" +
                "^FO340,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO480,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = "^FO15,{0}^ADN,18,10^FD{1}^FS" +
                "^FO275,{0}^ADN,18,10^FD{2}^FS" +
                "^FO340,{0}^ADN,18,10^FD{4}^FS" +
                "^FO480,{0}^ADN,18,10^FD{3}^FS";
        }
    }
}