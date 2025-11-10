using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class AlwaysFreshPrinter : ZebraFourInchesPrinter1
    {
        protected override string GetOrderDetailSectionHeader(int factor)
        {
            switch (factor)
            {
                case -1:
                    return "SALES SECTION";
                case 0:
                    return "CREDITS SECTION";
                case 1:
                    return "CREDITS SECTION";
                default:
                    return "SALES SECTION";
            }
        }

    }
}