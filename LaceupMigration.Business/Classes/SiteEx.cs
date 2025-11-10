using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public enum SiteType
    {
        NotDefined = 0,
        Main = 1,
        Location = 2,
        ReceivingArea = 3,
        Consignment = 4,
        Truck = 5,
        Damaged = 6,
        PassThrough = 7,
        LoadingArea = 8,
        ReturnArea = 9,
        DeliveryArea = 10,
        ConfirmationArea = 11
    }
    public enum ProductLotRestrictionsEnum
    {
        NoRestriction = 0,
        OneLotOneProduct = 1,
        OneLotMultipleProducts = 2,
        MultLotsMultProducts = 3,
        MultLotsOneProduct = 4
    }

    public class SiteEx 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentSiteId { get; set; }
        public string ExtraFields { get; set; }
        public SiteType SiteType { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
        public bool UsedForPicking { get; set; }
        public double MaxQty { get; set; }
        public int PickingOrder { get; set; }
        public bool RestrictionFree { get; set; }
        public bool OnlyAllowedProducts { get; set; }
        public ProductLotRestrictionsEnum ProductLotRestriction { get; set; }

        public static int MainCount { get; set; }
        public static int StagingCount { get; set; }
        public static int DeliveryAreaCount { get; set; }
        public static int LoadingAreaCount { get; set; }
        public static int ReturnAreaCount { get; set; }
        public static int DamagedCount { get; set; }
        public static int TruckCount { get; set; }
        public static int ConfirmationAreaCount { get; set; }

        public SiteEx()
        {
            Name = "";
            ExtraFields = "";
            Inventories = new List<SiteInventoryEx>();
        }

        public List<SiteInventoryEx> Inventories { get; set; }

        static Dictionary<int, SiteEx> map = new Dictionary<int, SiteEx>();

        public static List<SiteEx> Sites { get { return map.Values.ToList(); } }

        public static List<SiteInventoryEx> SitesWithProduct(int productId)
        {
            var allSiteInventoriesWithProduct = new List<SiteInventoryEx>();

            var siteId = Config.BranchSiteId;
            if (siteId == 0)
            {
                var salesamn = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                if (salesamn != null)
                    siteId = salesamn.BranchId;
            }

            var site = Find(siteId);

            if(site != null)
            {
                var sitesWithProduct = Sites.Where(x => x.Inventories.Any(x => x.ProductId == productId));

                foreach(var s in sitesWithProduct)
                {
                    foreach(var inv in s.Inventories)
                    {
                        if (inv.ProductId != productId)
                            continue;

                        allSiteInventoriesWithProduct.Add(inv);
                    }
                }

                if(allSiteInventoriesWithProduct.Count > 0)
                {
                    return allSiteInventoriesWithProduct.OrderBy(x => x.ExpirationDate).Take(3).ToList();
                }
               
            }

            return allSiteInventoriesWithProduct;
        }
        public static void AddSite(SiteEx site)
        {
            try
            {
                map.Add(site.Id, site);

                if (site.SiteType == SiteType.Main)
                    MainCount++;
                else if (site.SiteType == SiteType.ReceivingArea)
                    StagingCount++;
                else if (site.SiteType == SiteType.DeliveryArea)
                    DeliveryAreaCount++;
                else if (site.SiteType == SiteType.LoadingArea)
                    LoadingAreaCount++;
                else if (site.SiteType == SiteType.ReturnArea)
                    ReturnAreaCount++;
                else if (site.SiteType == SiteType.Damaged)
                    DamagedCount++;
                else if (site.SiteType == SiteType.Truck)
                    TruckCount++;
                else if (site.SiteType == SiteType.ConfirmationArea)
                    ConfirmationAreaCount++;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static SiteEx Find(int id)
        {
            SiteEx s = null;
            map.TryGetValue(id, out s);
            return s;
        }

        public static void Clear()
        {
            MainCount = 0;
            StagingCount = 0;
            DeliveryAreaCount = 0;
            LoadingAreaCount = 0;
            ReturnAreaCount = 0;
            DamagedCount = 0;
            TruckCount = 0;
            ConfirmationAreaCount = 0;

            map.Clear();
            map = new Dictionary<int, SiteEx>();
        }

        public static void AdjustParents()
        {
            var nodes = Sites.Where(x => x.SiteType == SiteType.Location).ToList();
            if (nodes.Count == 0)
                return;

            foreach (var node in nodes)
                AssignParent(node);

            var pt = Sites.Where(x => x.SiteType == SiteType.PassThrough);

            foreach (var item in pt)
                map.Remove(item.Id);

        }

        static void AssignParent(SiteEx site)
        {
            var tempSite = site;

            while (tempSite.ParentSiteId != 0)
            {
                SiteEx parent = null;
                map.TryGetValue(tempSite.ParentSiteId, out parent);

                if (parent == null)
                    break;

                tempSite = parent;
            }

            if (site.Id != tempSite.Id)
                site.ParentSiteId = tempSite.Id;
        }

        public void ClearInventories()
        {
            Inventories.Clear();
        }

        public void ClearInventoryForProduct(int productId)
        {
            Inventories.RemoveAll(x => x.ProductId == productId);
        }

        public void AddInventory(int productId, double qty, string lot, DateTime expiration, string extraFields)
        {
            var inv = new SiteInventoryEx()
            {
                ProductId = productId,
                Qty = qty,
                Lot = lot,
                ExpirationDate = expiration,
                ExtraFields = extraFields,
                Site = this
            };

            var ml = DataAccess.GetSingleUDF("mfgLot", inv.ExtraFields);
            var md = DataAccess.GetSingleUDF("mfgDate", inv.ExtraFields);

            if (!string.IsNullOrEmpty(ml))
                inv.MfgLot = ml;
            if (!string.IsNullOrEmpty(md))
                inv.MfgDate = new DateTime(Convert.ToInt64(md));

            Inventories.Add(inv);
        }

        public double GetInventoryForProduct(int productId)
        {
            return Inventories.Where(x => x.ProductId == productId).Sum(x => x.Qty);
        }

        public DateTime GetOldestExpForProduct(int productId)
        {
            DateTime exp = DateTime.MinValue;
            var inv = Inventories.Where(x => x.ProductId == productId).OrderBy(x => x.ExpirationDate).FirstOrDefault();
            if (inv != null)
                exp = inv.ExpirationDate;

            return exp;
        }

        public static void ClearMemory()
        {
            map.Clear();
        }

        public List<SiteEx> GetLeafSites()
        {
            return Sites.Where(x => x.ParentSiteId == Id).ToList();
        }
    }
}