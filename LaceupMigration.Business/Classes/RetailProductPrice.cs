using System.Collections.Generic;

namespace LaceupMigration
{
    public class RetailProductPrice
    {
        /// <summary>
        /// Returns the product associated with this price
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Returns the PriceLevelID. Valid only if IsBasedOnPriceLevel is true
        /// </summary>
        public int RetailPriceLevelId { get; set; }

        /// <summary>
        /// The Price
        /// </summary>
        public double Price { get; set; }

        public double Allowance { get; set; }

        static List<RetailProductPrice> priceList = new List<RetailProductPrice>();

        /// <summary>
        /// Returns the list of ProductPrice defined in the system
        /// </summary>
        public static IEnumerable<RetailProductPrice> Pricelist
        {
            get { return priceList; }
        }

        /// <summary>
        /// Clear the list.
        /// </summary>
        /// <param name="count"></param>
        public static void Clear(int count)
        {
            priceList.Clear();
            priceList.Capacity = count;
        }

        /// <summary>
        /// Adds a new ProductPrice
        /// </summary>
        /// <param name="item"></param>
        public static void Add(RetailProductPrice item)
        {
            priceList.Add(item);
        }
    }
}