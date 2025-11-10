using System;

namespace LaceupMigration
{
    public static class InvoiceIdProvider
    {
        static public IInvoiceIdProvider CurrentProvider()
        {
            try
            {
                IInvoiceIdProvider provider;
                // instantiate selected printer
                Type t = Type.GetType(Config.InvoiceIdProvider);
                if (t == null)
                {
                    Logger.CreateLog("could not instantiate invoice id provider" + Config.InvoiceIdProvider + " using DefaultInvoiceProvider instead");
                    provider = new DefaultInvoiceProvider();
                }
                provider = Activator.CreateInstance(t) as IInvoiceIdProvider;
                return provider;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                //Xamarin.Insights.Report (ee);
                // throw;
                return new DefaultInvoiceProvider();
            }
        }
    }
}