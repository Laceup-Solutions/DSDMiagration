using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;







using System.Globalization;

namespace LaceupMigration
{
    public class ParLevel
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public Product Product { get; set; }

        public float Qty { get; set; }

        static List<ParLevel> list = new List<ParLevel>();

        public static List<ParLevel> List { get { return list; } }

        public static void SaveList()
        {
            string tempFile = Config.ParLevelFile;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        foreach (var btq in List)
                        {
                            writer.Write(btq.Product.ProductId);
                            writer.Write((char)20);
                            writer.Write(btq.Qty);
                            writer.WriteLine();
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

        public static void LoadList()
        {
            if (File.Exists(Config.ParLevelFile))
            {
                List.Clear();
                using (StreamReader reader = new StreamReader(Config.ParLevelFile))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split(new char[] { (char)20 });
                            int productID = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                            float qtyty = Convert.ToSingle(parts[1], CultureInfo.InvariantCulture);

                            var product = Product.Products.FirstOrDefault(x => x.ProductId == productID);

                            if(product == null)
                            {
                                Logger.CreateLog("Product Not Found. Id=" + productID);
                                continue;
                            }

                            ParLevel btq = new ParLevel();
                            btq.Product = product;
                            btq.Qty = qtyty;

                            List.Add(btq);
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                            //Xamarin.Insights.Report(ee);
                        }
                    }
                    reader.Close();
                }
            }
        }

        public static void Clear()
        {
            list = new List<ParLevel>();
        }
    }
}