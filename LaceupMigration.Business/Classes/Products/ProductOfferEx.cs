using System.Collections.Generic;

namespace LaceupMigration
{
    public class ProductOfferEx
    {
        static List<ProductOfferEx> categoryList = new List<ProductOfferEx>();
        public int OfferExId { get; set; }
        public int ProductId { get; set; }
        public double BreakQty { get; set; }
        public double Price { get; set; }

        public static void Add(ProductOfferEx category)
        {
            categoryList.Add(category);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<ProductOfferEx> List
        {
            get { return categoryList; }
        }
    }
}