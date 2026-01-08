namespace LaceupMigration
{
    class AllCoffeePrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();
            
            linesTemplates[OrderTotalsOtherCharges] = "^FO40,{0}^ADN,36,20^FDTRADE SURCHARGE: {1}^FS";
        }
    }
}