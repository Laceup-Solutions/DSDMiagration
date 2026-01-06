using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaceupMigration
{
    public class RouteReturnLine
    {
        public Product Product { get; set; }

        public UnitOfMeasure UoM {get;set;}

        public float Reships { get; set; }

        public float Returns { get; set; }

        public float Dumps { get; set; }

        public float DamagedInTruck { get; set; }

        public float Unload { get; set; }

        public string Lot { get; set; }

        public double Weight { get; set; }
        public DateTime Expiration { get; set; }

        public RouteReturnLine()
        {
            Lot = "";
            Weight = 0;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                Product.ProductId, DamagedInTruck, Unload, Dumps, Returns, Lot, Expiration.Ticks, Reships, Weight);
        }
    }

    public abstract class RRTemplateLine 
    {
        public Product Product { get; set; }

        public abstract string Lot { get; }

        public abstract float Reships { get; }
        public abstract float Returns { get; }
        public abstract float Dumps { get; }
        public abstract float DamagedInTruck { get; }
        public abstract float Unload { get; }

        public abstract double Weight { get; }
        public abstract void Serialize(StreamWriter writer);


    }

    public class RRSingleLine : RRTemplateLine
    {
        public RouteReturnLine Detail { get; set; }
        public override string Lot { get { return Detail.Lot; } }
        public override float Reships { get { return Detail.Reships; } }
        public override float Returns { get { return Detail.Returns; } }
        public override float Dumps { get { return Detail.Dumps; } }
        public override float DamagedInTruck { get { return Detail.DamagedInTruck; } }
        public override float Unload { get { return Detail.Unload; } }
        public UnitOfMeasure UoM { get { return Detail.UoM; } }
        public override double Weight { get { return Detail.Weight; } }

        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine(Detail.ToString());
        }

        public void AddReships(float qty)
        {
            Detail.Reships += qty;
        }

        public void AddReturns(float qty)
        {
            Detail.Returns += qty;
        }

        public void AddDumps(float qty)
        {
            Detail.Dumps += qty;
        }

        public void AddDamagedInTruck(float qty)
        {
            Detail.DamagedInTruck += qty;
        }

        public void AddUnload(float qty)
        {
            Detail.Unload += qty;
        }
    }

    public class RRGroupedLine : RRTemplateLine
    {
        public List<RouteReturnLine> Details { get; set; }
        public override string Lot 
        {
            get
            {
                var det = Details.FirstOrDefault();
                if (det != null)
                    return det.Lot;
                else
                    return string.Empty;
            }
        }

        public override float Reships { get { return Details.Sum(x => x.Reships); } }
        public override float Returns { get { return Details.Sum(x => x.Returns); } }
        public override float Dumps { get { return Details.Sum(x => x.Dumps); } }
        public override float DamagedInTruck { get { return Details.Sum(x => x.DamagedInTruck); } }
        public override float Unload { get { return Details.Sum(x => x.Unload); } }

        public override double Weight 
        { 
            get 
            {
                var det = Details.FirstOrDefault();
                if (det != null)
                    return det.Weight;
                else
                    return 0;
            } 
        }

        public override void Serialize(StreamWriter writer)
        {
            foreach (var item in Details)
                writer.WriteLine(item.ToString());
        }

        public RRGroupedLine()
        {
            Details = new List<RouteReturnLine>();
        }

        public void AddReships(float qty, string lot = "", double weight = 0)
        {
            if (Product.SoldByWeight && Product.InventoryByWeight)
            {
                var detail = new RouteReturnLine() { Product = Product, Lot = lot, Reships = qty };
                Details.Add(detail);
            }
            else
            {
                var detail = Details.FirstOrDefault(x => x.Lot == lot);

                if (Product.SoldByWeight && Config.UsePallets)
                    detail = Details.FirstOrDefault(x => x.Lot == lot && x.Weight == weight);

                if (detail == null)
                {
                    detail = new RouteReturnLine() { Product = Product, Lot = lot, Weight = weight };
                    Details.Add(detail);
                }

                detail.Reships += qty;
            }
        }

        public void AddReturns(float qty, string lot = "", double weight = 0)
        {
            if (Product.SoldByWeight && Product.InventoryByWeight)
            {
                var detail = new RouteReturnLine() { Product = Product, Lot = lot, Returns = qty };
                Details.Add(detail);
            }
            else
            {
                var detail = Details.FirstOrDefault(x => x.Lot == lot);

                if (Product.SoldByWeight && Config.UsePallets)
                    detail = Details.FirstOrDefault(x => x.Lot == lot && x.Weight == weight);

                if (detail == null)
                {
                    detail = new RouteReturnLine() { Product = Product, Lot = lot, Weight = weight };
                    Details.Add(detail);
                }

                detail.Returns += qty;
            }
        }

        public void AddDumps(float qty, string lot = "", double weight = 0)
        {
            if (Product.SoldByWeight && Product.InventoryByWeight)
            {
                var detail = new RouteReturnLine() { Product = Product, Lot = lot, Dumps = qty };
                Details.Add(detail);
            }
            else
            {
                var detail = Details.FirstOrDefault(x => x.Lot == lot);

                if (Product.SoldByWeight && Config.UsePallets)
                    detail = Details.FirstOrDefault(x => x.Lot == lot && x.Weight == weight);

                if (detail == null)
                {
                    detail = new RouteReturnLine() { Product = Product, Lot = lot, Weight = weight };
                    Details.Add(detail);
                }

                detail.Dumps += qty;
            }
        }

        public void AddDamagedInTruck(float qty, string lot = "", double weight = 0)
        {
            if (Product.SoldByWeight && Product.InventoryByWeight)
            {
                var detail = new RouteReturnLine() { Product = Product, Lot = lot, DamagedInTruck = qty };
                Details.Add(detail);
            }
            else
            {
                var detail = Details.FirstOrDefault(x => x.Lot == lot);


                if (Product.SoldByWeight && Config.UsePallets)
                    detail = Details.FirstOrDefault(x => x.Lot == lot && x.Weight == weight);

                if (detail == null)
                {
                    detail = new RouteReturnLine() { Product = Product, Lot = lot, Weight = weight  };
                    Details.Add(detail);
                }

                detail.DamagedInTruck += qty;
            }
        }

        public void AddUnload(float qty, string lot = "", double weight = 0)
        {
            if (Product.SoldByWeight && Product.InventoryByWeight)
            {
                var detail = new RouteReturnLine() { Product = Product, Lot = lot, Unload = qty };
                Details.Add(detail);
            }
            else
            {
                var detail = Details.FirstOrDefault(x => x.Lot == lot);


                if (Product.SoldByWeight && Config.UsePallets)
                    detail = Details.FirstOrDefault(x => x.Lot == lot && x.Weight == weight);

                if (detail == null)
                {
                    detail = new RouteReturnLine() { Product = Product, Lot = lot, Weight = weight };
                    Details.Add(detail);
                }

                detail.Unload += qty;
            }
        }
    }

    public class GroupedToPrintRouteReturnLine
    {
        public Product Product { get; set; }

        public UnitOfMeasure UoM { get; set; }

        public float Reships { get; set; }

        public float Returns { get; set; }

        public float Dumps { get; set; }

        public float DamagedInTruck { get; set; }

        public float Unload { get; set; }

        public string Lot { get; set; }

        public DateTime Expiration { get; set; }

        public GroupedToPrintRouteReturnLine()
        {
            Lot = "";
        }
    }
}

