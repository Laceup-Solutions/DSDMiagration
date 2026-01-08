using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ProductInventory 
    {
        public static Dictionary<int, ProductInventory> CurrentInventories { get; } = new Dictionary<int, ProductInventory>();

        public int ProductId { get; set; }

        public List<TruckInventory> TruckInventories { get; set; }

        public ProductInventory()
        {
            TruckInventories = new List<TruckInventory>();
        }

        public float WarehouseInventory { get; set; }

        public static ProductInventory GetInventoryForProduct(int productId)
        {
            ProductInventory inv = null;
            CurrentInventories.TryGetValue(productId, out inv);
            return inv;
        }

        public void ClearProductInventory()
        {
            TruckInventories = new List<TruckInventory>();
        }

        public static void ClearAll()
        {
            CurrentInventories.Clear();
            CurrentInventories.EnsureCapacity(1000000);

            Save();
        }

        public static void Save()
        {
            try
            {
                if (File.Exists(Config.ProductInventoriesFile))
                    File.Delete(Config.ProductInventoriesFile);

                using (StreamWriter writer = new StreamWriter(Config.ProductInventoriesFile))
                {
                    foreach (var item in CurrentInventories.Values)
                    {
                        string line = string.Format("{1}{0}{2}",
                        (char)20,
                        item.ProductId,
                        item.WarehouseInventory
                        );

                        foreach (var inv in item.TruckInventories)
                            line += string.Format("{0}{1}", (char)20, inv.Serialize());

                        writer.WriteLine(line);
                    }
                }
            }
            catch(Exception ex)
            {
                
            }
        }

        public static void Load()
        {
            if (!File.Exists(Config.ProductInventoriesFile))
                return;

            using (StreamReader reader = new StreamReader(Config.ProductInventoriesFile))
            {
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(new char[] { (char)20 });

                    var prodId = Convert.ToInt32(parts[0]);
                    var warehouseOH = Convert.ToSingle(parts[1]);

                    var prodInv = new ProductInventory() { ProductId = prodId, WarehouseInventory = warehouseOH };

                    if (!CurrentInventories.ContainsKey(prodId))
                        CurrentInventories.Add(prodId, prodInv);
                    else
                        Logger.CreateLog("Current Inventories already contains productid");

                    for (int i = 2; i < parts.Length; i++)
                    {
                        var pps = parts[i].Split('|');

                        var truckInv = new TruckInventory()
                        {
                            Lot = pps[0],
                            Expiration = new DateTime(Convert.ToInt64(pps[1])),
                            BeginingInventory = Convert.ToSingle(pps[2]),
                            RequestedLoad = Convert.ToSingle(pps[3]),
                            Loaded = Convert.ToSingle(pps[4]),
                            TransferredOn = Convert.ToSingle(pps[5]),
                            TransferredOff = Convert.ToSingle(pps[6]),
                            Unloaded = Convert.ToSingle(pps[7]),
                            DamagedInTruck = Convert.ToSingle(pps[8]),
                            CurrentQty = Convert.ToSingle(pps[9])
                        };

                        if (pps.Length > 10)
                            truckInv.Weight = Convert.ToDouble(pps[10]);

                        prodInv.TruckInventories.Add(truckInv);
                    }
                }
            }
        }

        public bool InventoryChanged
        {
            get
            {
                if (BeginigInventory != CurrentInventory)
                    return true;

                if (LoadedInventory > 0 || TransferredOnInventory > 0 || TransferredOffInventory > 0 || UnloadedInventory > 0 ||
                    DamagedInTruckInventory > 0)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Current Total Truck Inventory
        /// </summary>
        public float CurrentInventory { get { return TruckInventories.Sum(x => x.CurrentQty); } }
        /// <summary>
        /// Total Begining Inventory. Is the Left Over in the InventorySiteInventory when the user sync updating the inventories
        /// </summary>
        public float BeginigInventory { get { return TruckInventories.Sum(x => x.BeginingInventory); } }
        /// <summary>
        /// Total Requested Inventory from loads or deliveries. 
        /// </summary>
        public float RequestedLoadInventory { get { return TruckInventories.Sum(x => x.RequestedLoad); } }
        /// <summary>
        /// Total Accepted Inventory from loads or deliveries
        /// </summary>
        public float LoadedInventory { get { return TruckInventories.Sum(x => x.Loaded); } }
        /// <summary>
        /// Total Transferred On Inventory
        /// </summary>
        public float TransferredOnInventory { get { return TruckInventories.Sum(x => x.TransferredOn); } }
        /// <summary>
        /// Total Transferred Off Inventory
        /// </summary>
        public float TransferredOffInventory { get { return TruckInventories.Sum(x => x.TransferredOff); } }
        /// <summary>
        /// Total Unloaded items
        /// </summary>
        public float UnloadedInventory { get { return TruckInventories.Sum(x => x.Unloaded); } }
        /// <summary>
        /// Total of Damaged In Truck
        /// </summary>
        public float DamagedInTruckInventory { get { return TruckInventories.Sum(x => x.DamagedInTruck); } }

        public void UpdateInventory(float qty, string lot, DateTime exp, int factor, double Weight)
        {
            var product = Product.Find(ProductId);
            if(product != null)
            {
                bool soldByweight = product.SoldByWeight;
                if (!soldByweight || product.InventoryByWeight)
                    Weight = 0;
            }

            var inv = TruckInventories.FirstOrDefault(x => x.Lot == lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = lot, Expiration = exp, Weight = Weight };
                TruckInventories.Add(inv);
            }
            
            inv.CurrentQty += qty * factor;
        }

        public void UpdateWarehouseInventory(float qty, int factor)
        {
            WarehouseInventory += qty * factor;
        }

        public void AddRequestedInventory(float qty, string lot, DateTime exp, double Weight)
        {
            var product = Product.Find(ProductId);
            if (product != null)
            {
                bool soldByweight = product.SoldByWeight;
                if (!soldByweight || product.InventoryByWeight)
                    Weight = 0;
            }

            var inv = TruckInventories.FirstOrDefault(x => x.Lot == lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = lot, Expiration = exp, Weight = Weight };
                TruckInventories.Add(inv);
            }

            inv.RequestedLoad += qty;
        }

        public void AddLoadedInventory(float qty, string lot, DateTime exp, double Weight)
        {
            var product = Product.Find(ProductId);
            if (product != null)
            {
                bool soldByweight = product.SoldByWeight;
                if (!soldByweight || product.InventoryByWeight)
                    Weight = 0;
            }

            var inv = TruckInventories.FirstOrDefault(x => x.Lot == lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = lot, Expiration = exp, Weight = Weight };
                TruckInventories.Add(inv);
            }

            inv.Loaded += qty;
        }

        public void UpdateTransferInventory(float qty, string lot, DateTime exp, int factor, double Weight)
        {
            var product = Product.Find(ProductId);
            if (product != null)
            {
                bool soldByweight = product.SoldByWeight;
                if (!soldByweight || product.InventoryByWeight)
                    Weight = 0;
            }

            var inv = TruckInventories.FirstOrDefault(x => x.Lot == lot && Math.Round(x.Weight,2) == Math.Round(Weight,2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = lot, Expiration = exp, Weight = Weight };
                TruckInventories.Add(inv);
            }

            if (factor > 0)
                inv.TransferredOn += qty;
            else
                inv.TransferredOff += qty;
        }
    }

    public class TruckInventory
    {
        public string Lot { get; set; }
        public DateTime Expiration { get; set; }
        public double Weight { get; set; }
        public float BeginingInventory { get; set; }
        public float RequestedLoad { get; set; }
        public float Loaded { get; set; }
        public float TransferredOn { get; set; }
        public float TransferredOff { get; set; }
        public float Unloaded { get; set; }
        public float DamagedInTruck { get; set; }


        public float OnCreditDump { get; set; }
        public float OnCreditReturn { get; set; }
        public float OnSales { get; set; }
        public float OnReships { get; set; }

        public float CurrentQty { get; set; }

        public TruckInventory()
        {
            Lot = "";
            Weight = 0;
        }

        public string Serialize()
        {
            string s = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
                Lot,
                Expiration.Ticks,
                BeginingInventory,
                RequestedLoad,
                Loaded,
                TransferredOn,
                TransferredOff,
                Unloaded,
                DamagedInTruck,
                CurrentQty,
                Weight);

            return s;
        }
    }
}