






using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SuperSourcePrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderVendorNumber] = "^FO40,{0}^ADN,18,10^FDAccount Number: {1}^FS";
        }
    }

    public class SuperSourceThreeInchesPrinter : ZebraThreeInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderVendorNumber] = "^FO15,{0}^ADN,18,10^FDAccount Number: {1}^FS";
        }
    }
}