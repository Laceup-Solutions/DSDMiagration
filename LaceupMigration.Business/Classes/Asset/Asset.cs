using System.Collections.Generic;
using System;
using System.Linq;

namespace LaceupMigration
{
    public class Asset
    {
        public static List<Asset> List = new List<Asset>();
        public int Id { get; set; }
        public int ProductId { get; set; }
        public System.DateTime StartDate { get; set; }
        public string SerialNumber { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public string Comments { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public bool Active { get; set; }
        public System.DateTime DeactivatedDate { get; set; }
        public string Extrafields { get; set; }
        public Nullable<int> SiteId { get; set; }

        public Product Product
        {
            get
            {
                return Product.Find(ProductId);
            }
        }
        public static Asset Find(string serialNumber)
        {
            return List.FirstOrDefault(x => x.SerialNumber == serialNumber);
        }

        public static Asset FindById(int id)
        {
            return List.FirstOrDefault(x => x.Id == id);
        }
    }
}