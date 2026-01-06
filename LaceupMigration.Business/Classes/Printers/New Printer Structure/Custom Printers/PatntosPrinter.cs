





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class PatntosPrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
           "^FO450,{0}^ADN,18,10^FDQTY^FS" +
           "^FB120,1,0,C^FO520,{0}^ADN,18,10^FDPRICE^FS" +
           "^FB140,1,0,C^FO650,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
           "^FO450,{0}^ADN,18,10^FD{2}^FS" +
           "^FB120,1,0,R^FO520,{0}^ADN,18,10^FD{4}^FS" +
           "^FB140,1,0,R^FO650,{0}^ADN,18,10^FD{3}^FS";


            linesTemplates[OrderTotalsNetQty]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDNET QTY:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsSales]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDSALES:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsCredits] = "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDCREDITS:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsReturns]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDRETURNS:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsNetAmount]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDNET AMOUNT:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsDiscount]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDDISCOUNT:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsTax] = "^FO40,{0}^ADN,36,20^FB390,1,0,R^FD{1}^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{2}^FS";
            linesTemplates[OrderTotalsTotalDue] = "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDTOTAL DUE:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsTotalPayment] = "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDTOTAL PAYMENT:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsCurrentBalance]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDINVOICE BALANCE:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance]= "^FO40,{0}^ADN,36,20^FB390,1,0,R^FDOPEN BALANCE:^FS^FO40,{0}^ADN,36,20^FB730,1,0,R^FD{1}^FS";

        }

    }
}