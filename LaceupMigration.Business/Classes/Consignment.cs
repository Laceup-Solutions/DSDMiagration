using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LaceupMigration
{
    public class Consignment
    {
        public Client Client { get; set; }

        public Product Product { get; set; }

        public float Qty { get; set; }

        public double Price { get; set; }
    }

    public class ConsignmentValues
    {
        public Client Client { get; set; }

        public Product Product { get; set; }

        public float OldValue { get; set; }

        public float NewValue { get; set; }

        public float Counted { get; set; }

        public float Sold { get; set; }

        public double NewPrice { get; set; }

        public float Picked { get; set; }

        public static List<ConsignmentValues> Lines { get; set; }

        public static void LoadFromDetail(Order order)
        {
            Lines = new List<ConsignmentValues>();

            foreach (var det in order.Details.Where(x => !x.ParLevelDetail))
            {
                var consignment = new ConsignmentValues() { Client = order.Client, Product = det.Product };

                var oldvalue = DataAccess.GetSingleUDF("oldvalue", det.ExtraFields);
                consignment.OldValue = Convert.ToSingle(oldvalue);

                var newvalue = DataAccess.GetSingleUDF("newvalue", det.ExtraFields);
                consignment.NewValue = Convert.ToSingle(newvalue);

                var counted = DataAccess.GetSingleUDF("count", det.ExtraFields);
                consignment.Counted = Convert.ToSingle(counted);

                var updated = DataAccess.GetSingleUDF("updated", det.ExtraFields);
                if (Convert.ToInt32(updated) == 0)
                    consignment.NewValue = consignment.OldValue;

                var sold = DataAccess.GetSingleUDF("sold", det.ExtraFields);
                consignment.Sold = Convert.ToSingle(sold);

                var newprice = DataAccess.GetSingleUDF("price", det.ExtraFields);
                consignment.NewPrice = Convert.ToDouble(newprice);

                var picked = DataAccess.GetSingleUDF("picked", det.ExtraFields);
                consignment.Picked = Convert.ToSingle(picked);

                Lines.Add(consignment);
            }

            SaveList();
        }

        public static void SaveList()
        {
            string tempFile = Config.ConsignmentParFile;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    if (Lines.Count == 0)
                        return;

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        foreach (var item in Lines)
                        {
                            writer.Write(item.Client.ClientId.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.Product.ProductId.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.OldValue.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.NewValue.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.Counted.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.Sold.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.Write(item.NewPrice.ToString(CultureInfo.InvariantCulture));
                            writer.Write((char)20);
                            writer.WriteLine(item.Picked.ToString(CultureInfo.InvariantCulture));
                        }

                        writer.Close();
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void Clear()
        {
            if (Lines != null)
                Lines.Clear();
        }
    }
}
