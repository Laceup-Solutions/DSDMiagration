





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class TemporalInvoicePayment
    {
        public static List<TemporalInvoicePayment> List = new List<TemporalInvoicePayment>();

        public int invoiceId { get; set; }

        public double amountPaid { get; set; }

        public static void Load()
        {
            try
            {
                List.Clear();

                if (!File.Exists(Config.TemporalInvoicePayment))
                    return;

                using (StreamReader reader = new StreamReader(Config.TemporalInvoicePayment))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var invoiceId = Convert.ToInt32(parts[0]);
                        var amountPaid = Convert.ToDouble(parts[1]);

                        var temp = new TemporalInvoicePayment()
                        {
                            invoiceId = invoiceId,
                            amountPaid = amountPaid
                        };

                        List.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Save()
        {
            if (File.Exists(Config.TemporalInvoicePayment))
                File.Delete(Config.TemporalInvoicePayment);

            using (StreamWriter writer = new StreamWriter(Config.TemporalInvoicePayment))
            {
                foreach (var item in List)
                {
                    writer.Write(item.invoiceId);
                    writer.Write((char)20);
                    writer.Write(item.amountPaid);
                    writer.WriteLine();
                }
            }
        }

        internal static void Delete()
        {
            List.Clear();

            if (File.Exists(Config.TemporalInvoicePayment))
                File.Delete(Config.TemporalInvoicePayment);
        }
    }
}