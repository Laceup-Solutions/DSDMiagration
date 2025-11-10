using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class RetailPriceLevel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalId { get; set; }
        public byte[] RowVersion { get; set; }
        public int RetailPriceLevelType { get; set; }
        public double Percentage { get; set; }
        public bool CreatedLocally { get; set; }

        static List<RetailPriceLevel> priceList = new List<RetailPriceLevel>();

        public static IEnumerable<RetailPriceLevel> Pricelist
        {
            get { return priceList; }
        }

        public static void Clear(int count)
        {
            priceList.Clear();
            priceList.Capacity = count;
        }

        public static void Add(RetailPriceLevel item)
        {
            priceList.Add(item);
        }
    }
}