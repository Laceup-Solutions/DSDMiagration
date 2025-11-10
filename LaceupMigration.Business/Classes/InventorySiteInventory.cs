using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LaceupMigration
{
    public class InventorySiteInventory
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int ProductId { get; set; }

        public double Qty { get; set; }

        public double LeftOver { get; set; }

        public string ExtraFields { get; set; }

        public static Dictionary<int, InventorySiteInventory> Map = new Dictionary<int, InventorySiteInventory>();

        public static List<InventorySiteInventory> List = new List<InventorySiteInventory>();
        public static InventorySiteInventory Get(int productId)
        {
            InventorySiteInventory ret = null;
            Map.TryGetValue(productId, out ret);
            return ret;
        }

        Product prod;
        public Product Product
        {
            get
            {
                if (prod == null)
                    prod = Product.Find(ProductId);
                if (prod == null)
                    prod = Product.CreateNotFoundProduct(ProductId);
                return prod;
            }
        }

        public static void LoadFromFile()
        {
            var file = Config.ButlerInventorySiteInventories;

            using (StreamReader reader = new StreamReader(file))
            {
                List.Clear();
                string currentline;

                while ((currentline = reader.ReadLine()) != null)
                {
                    string[] currentrow = currentline.Split(DataAccess.DataLineSplitter);
                    var item = new InventorySiteInventory();
                    item.Id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
                    item.SiteId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
                    item.ProductId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
                    item.LeftOver = Convert.ToSingle(currentrow[3], CultureInfo.InvariantCulture);

                    List.Add(item);
                }
            }
        }

        public static void SaveInFile()
        {
            var file = Config.ButlerInventorySiteInventories;

            if (File.Exists(file))
                File.Delete(file);

            using(StreamWriter stream = new StreamWriter(file))
            {
                foreach (var item in List)
                {
                    string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}",
                        (char)20,
                        item.Id,
                        item.SiteId,
                        item.ProductId,
                        item.LeftOver
                        );

                    stream.WriteLine(line);
                }
            }
        }
    }
}