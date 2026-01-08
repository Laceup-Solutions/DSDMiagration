using System;
using System.Collections.Generic;

namespace LaceupMigration
{
    public class DiscountCategory
    {
        static List<DiscountCategory> categoryList = new List<DiscountCategory>();

        /// <summary>
        /// Add a category to the list
        /// </summary>
        /// <param name="category">Item to add</param>
        public static void AddCategory(DiscountCategory category)
        {
            categoryList.Add(category);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<DiscountCategory> DiscountCategories
        {
            get { return categoryList; }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<bool> isActive { get; set; }
        public string ExtraFields { get; set; }
        public bool IsPriceCategory { get; set; }
    }
}