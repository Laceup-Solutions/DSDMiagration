using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public abstract class CCTemplateLine 
    {
        public Product Product { get; set; }
        public bool CurrentFocus { get; set; }
        public abstract float Qty { get; }
        public UnitOfMeasure UoM { get; set; }

        public abstract double Weight { get; }

        public abstract void Serialize(StreamWriter writer);
    }

    public class CCSingleTemplateLine : CCTemplateLine
    {
        public CycleCountItem Detail { get; set; }

        public override float Qty { get { return Detail.Qty; } }

        public override double Weight { get { return Detail.Weight; } }

        public override void Serialize(StreamWriter writer)
        {
            Detail.Serialize(writer);
        }
    }

    public class CCGroupedTemplateLine : CCTemplateLine
    {
        public List<CycleCountItem> Details { get; set; }

        public override float Qty
        {
            get
            {
                float qty = 0;
                foreach (var item in Details)
                {
                    if (item.UoM != null)
                        qty += (item.Qty * item.UoM.Conversion);
                    else
                        qty += item.Qty;
                }
                return qty;
            }
        }

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

        public CCGroupedTemplateLine()
        {
            Details = new List<CycleCountItem>();
        }

        public override void Serialize(StreamWriter writer)
        {
            foreach (var item in Details)
                item.Serialize(writer);
        }
    }

    public class CycleCountItem 
    {
        public Product Product { get; set; }
        public float Qty { get; set; }
        public string Lot { get; set; }
        public DateTime Expiration { get; set; }
        public UnitOfMeasure UoM { get; set; }

        public double Weight { get; set; }

        public CycleCountItem()
        {
            Lot = "";
        }

        public void Serialize(StreamWriter writer)
        {
            writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}",
                        (char)20,
                        Product.ProductId,
                        Qty,
                        Lot,
                        Expiration.Ticks,
                        UoM != null ? UoM.Id : 0,
                        Weight));
        }
    }
}