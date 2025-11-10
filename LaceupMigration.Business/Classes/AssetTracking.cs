using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class AssetTrackingHistory 
    {
        public Product Product { get; set; }
        public Client Client { get; set; }
        public double Cost { get; set; }
        public float LastOH { get; set; }
        public float Stays { get; set; }
        public float Ins { get; set; }
        public float Outs { get; set; }
        public float NextOH { get; set; }
        public float OverShort { get; set; }
        public DateTime DateVisit { get; set; }
        public int SalesmanId { get; set; }

        public string Key { get { return Client.ClientId + "," + Product.ProductId; } }

        public static void Load()
        {
            AssetTrackingHistories = new Dictionary<string, List<AssetTrackingHistory>>();

            if (!File.Exists(Config.AssetTrackingHistoriesFile))
                return;

            using (StreamReader reader = new StreamReader(Config.AssetTrackingHistoriesFile))
            {
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(DataAccess.DataLineSplitter);

                    var at = new AssetTrackingHistory()
                    {
                        Product = Product.Products.FirstOrDefault(x => x.ProductId == Convert.ToInt32(parts[0])),
                        Cost = Convert.ToDouble(parts[1]),
                        LastOH = Convert.ToSingle(parts[2]),
                        Stays = Convert.ToSingle(parts[3]),
                        Ins = Convert.ToSingle(parts[4]),
                        Outs = Convert.ToSingle(parts[5]),
                        NextOH = Convert.ToSingle(parts[6]),
                        OverShort = Convert.ToSingle(parts[7]),
                        DateVisit = new DateTime(Convert.ToInt64(parts[8])),
                        Client = Client.Find(Convert.ToInt32(parts[10]))
                    };

                    if (at.Product == null || at.Client == null)
                        continue;

                    if (!AssetTrackingHistories.ContainsKey(at.Key))
                        AssetTrackingHistories.Add(at.Key, new List<AssetTrackingHistory>());

                    AssetTrackingHistories[at.Key].Add(at);
                }
            }
        }

        public static Dictionary<string, List<AssetTrackingHistory>> AssetTrackingHistories { get; set; } = new Dictionary<string, List<AssetTrackingHistory>>();
    }

    public class AssetTracking 
    {
        public Product Product { get; set; }
        public Client Client { get; set; }
        public string ClientUniqueId { get; set; }
        public double Cost { get; set; }
        public float LastOH { get; set; }
        public float Stays { get; set; }
        public float Ins { get; set; }
        public float Outs { get; set; }
        public float NextOH 
        { 
            get 
            {
                if (Config.SensationalAssetTracking)
                    return LastOH + (Ins - Outs);

                return Stays + Ins; 
            } 
        }
        public float OverShort { get { return Visited && LastOH > 0 ? (LastOH - Stays - Outs) * -1 : 0; } }
        public DateTime DateVisit { get; set; }
        public int SalesmanId { get; set; }
        public bool Visited { get { return Counted || InsCounted || OutsCounted; } }
        public bool Counted { get; set; }
        public bool InsCounted { get; set; }
        public bool OutsCounted { get; set; }

        public static List<AssetTracking> List { get; set; } = new List<AssetTracking>();
        public static double TruckInventory
        {
            get
            {
                return GetAmountOfInCrates(Order.Orders.Where(x => x.IsDelivery).ToList());
            }
        }
        public static double GetAmountOfInCrates(List<Order> orders)
        {
            double totalAmount = 0;

            foreach (var o in orders.Where(x => x.IsDelivery))
            {
                foreach (var item in o.Details)
                {
                    var units = DataAccess.GetSingleUDF("units per crate", item.Product.NonVisibleExtraFieldsAsString);

                    if (string.IsNullOrEmpty(units))
                        continue;

                    int unitsPerCrate = 0;
                    Int32.TryParse(units, out unitsPerCrate);

                    if (unitsPerCrate > 0)
                    {
                        if (item.UnitOfMeasure != null && !item.UnitOfMeasure.IsBase)
                        {
                            var factor = item.UnitOfMeasure.Conversion;
                            var newQty = item.Qty * factor;

                            var crates = Math.Truncate(newQty / unitsPerCrate);
                            bool hasReminder = newQty % unitsPerCrate > 0;

                            if (hasReminder)
                                crates += 1;

                            totalAmount += crates;

                        }
                        else
                        {
                            var crates = Math.Truncate(item.Qty / unitsPerCrate);
                            bool hasReminder = item.Qty % unitsPerCrate > 0;

                            if (hasReminder)
                                crates += 1;

                            totalAmount += crates;
                        }
                    }
                }
            }

            return totalAmount;
        }
        public static void Save()
        {
            if (File.Exists(Config.AssetTrackingFile))
                File.Delete(Config.AssetTrackingFile);

            using (StreamWriter writer = new StreamWriter(Config.AssetTrackingFile))
            {
                foreach (var item in List)
                {
                    writer.Write(item.Product.ProductId.ToString(CultureInfo.InvariantCulture));                                        // 0
                    writer.Write((char)20);
                    writer.Write(item.Cost.ToString(CultureInfo.InvariantCulture));                             // 1
                    writer.Write((char)20);
                    writer.Write(item.LastOH.ToString(System.Globalization.CultureInfo.InvariantCulture));       // 2
                    writer.Write((char)20);
                    writer.Write(item.Stays.ToString(System.Globalization.CultureInfo.InvariantCulture));      // 3
                    writer.Write((char)20);
                    writer.Write(item.Ins.ToString(System.Globalization.CultureInfo.InvariantCulture));         // 4
                    writer.Write((char)20);
                    writer.Write(item.Outs.ToString(System.Globalization.CultureInfo.InvariantCulture));         // 5
                    writer.Write((char)20);
                    writer.Write(item.NextOH.ToString(System.Globalization.CultureInfo.InvariantCulture));         // 6
                    writer.Write((char)20);
                    writer.Write(item.OverShort.ToString(System.Globalization.CultureInfo.InvariantCulture));         // 7
                    writer.Write((char)20);
                    writer.Write(item.DateVisit.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture));         // 8
                    writer.Write((char)20);
                    writer.Write(item.Client.ClientId.ToString(CultureInfo.InvariantCulture));// 9
                    writer.Write((char)20);
                    writer.Write(item.ClientUniqueId.ToString(CultureInfo.InvariantCulture));// 10
                    writer.Write((char)20);
                    writer.Write(item.SalesmanId.ToString(CultureInfo.InvariantCulture));// 11
                    writer.Write((char)20);
                    writer.Write(item.Visited ? "1" : "0");// 11
                    writer.Write((char)20);
                    writer.Write(item.Counted ? "1" : "0");// 12
                    writer.Write((char)20);
                    writer.Write(item.InsCounted ? "1" : "0");// 13
                    writer.Write((char)20);
                    writer.Write(item.OutsCounted ? "1" : "0");// 14
                    writer.WriteLine();
                }
            }
        }

        public static void Load()
        {
            List = new List<AssetTracking>();

            if (!File.Exists(Config.AssetTrackingFile))
                return;

            using (StreamReader reader = new StreamReader(Config.AssetTrackingFile))
            {
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(DataAccess.DataLineSplitter);

                    var at = new AssetTracking()
                    {
                        Product = Product.Find(Convert.ToInt32(parts[0])),
                        Cost = Convert.ToDouble(parts[1]),
                        LastOH = Convert.ToSingle(parts[2]),
                        Stays = Convert.ToSingle(parts[3]),
                        Ins = Convert.ToSingle(parts[4]),
                        Outs = Convert.ToSingle(parts[5]),
                        DateVisit = new DateTime(Convert.ToInt64(parts[8])),
                        Client = Client.Find(Convert.ToInt32(parts[9])),
                        ClientUniqueId = parts[10],
                        SalesmanId = Convert.ToInt32(parts[11])
                    };

                    if (parts.Length > 13)
                        at.Counted = Convert.ToInt32(parts[13]) > 0;

                    if (parts.Length > 14)
                        at.InsCounted = Convert.ToInt32(parts[14]) > 0;

                    if (parts.Length > 14)
                        at.OutsCounted = Convert.ToInt32(parts[14]) > 0;

                    if (at.Product == null || at.Client == null)
                        continue;

                    List.Add(at);
                }
            }
        }

        public static void Clear()
        {
            List.Clear();

            if (File.Exists(Config.AssetTrackingFile))
                File.Delete(Config.AssetTrackingFile);
        }
    }
}