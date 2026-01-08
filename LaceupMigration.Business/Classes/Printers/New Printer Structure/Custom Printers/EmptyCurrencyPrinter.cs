





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class EmptyCurrencyPrinter : ZebraFourInchesPrinter1
    {
        public override string ToString(double d)
        {
            return d.ToString();
        }
    }
}
