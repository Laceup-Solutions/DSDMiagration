using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LaceupMigration
{


    /// <summary>
    /// Represents a category under which products are  kept.
    /// </summary>
    /// 
    public enum CategoryServiPartType
    {
        [Description("None")]
        None = 0,
        [Description("Service")]
        Services = 1,
        [Description("Part")]
        Part = 2,
    }

    public class Category
    {
        public bool Expanded { get; set; }
        /// <summary>
        /// The Category Id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The name of the category
        /// </summary>
        public string Name { get; set; }

        public CategoryServiPartType TypeServiPart { get; set; }

        public string ExtraFields { get; set; }

        /// <summary>
        /// The parent category.
        /// </summary>
        public int VisibleIn { get; set; }

        public int ParentCategoryId { get; set; }

        public bool NotVisibleInDump
        {
            get
            {
                if (string.IsNullOrEmpty(ExtraFields))
                    return false;
                return ExtraFields.ToLower().Contains("visibleindump=no");
            }
        }

        public static List<Category> CategoriesWithInventory()
        {
            var ids = Product.Products.Where(x => (x.ProductType == ProductType.NonInventory || Math.Round(x.CurrentInventory, Config.Round) > 0) && x.CategoryId > 0).Select(x => x.CategoryId).Distinct().ToList();
            var retValue = new List<Category>();
            foreach (var cat in Categories)
                if (ids.Contains(cat.CategoryId))
                    retValue.Add(cat);

            //add parents for subcategories with no inventory
            var moreCatsToAdd = new List<Category>();
            foreach(var c in retValue)
            {
                //add parent or wont be visible the child :(
                if(c.ParentCategoryId > 0)
                {
                    var parent = Category.Categories.FirstOrDefault(x => x.CategoryId == c.ParentCategoryId);

                    if (parent == null)
                        continue;

                    if (!retValue.Contains(parent) && !moreCatsToAdd.Contains(parent))
                        moreCatsToAdd.Add(parent);
                }
            }

            foreach(var c in moreCatsToAdd)
            {
                if (!retValue.Contains(c))
                    retValue.Add(c);
            }

            return retValue;
        }

        //Static members
        static List<Category> categoryList = new List<Category>();

        /// <summary>
        /// Add a category to the list
        /// </summary>
        /// <param name="category">Item to add</param>
        public static void AddCategory(Category category)
        {
            categoryList.Add(category);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<Category> Categories
        {
            get { return categoryList; }
        }

        /// <summary>
        /// Finds a category based on the ID
        /// </summary>
        /// <param name="categoryId">The ID</param>
        /// <returns>The Category or null if not found</returns>
        public static Category Find(int categoryId)
        {
            return Categories.FirstOrDefault(x => x.CategoryId == categoryId);
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        /// <param name="count">Reserve a capacity</param>
        public static void Clear(int count)
        {
            cachedList.Clear();
            categoryList.Clear();
            //categoryList.Capacity = count;
        }

        /// <summary>
        /// Removes the category.
        /// </summary>
        /// <param name='category'>
        /// Category instance.
        /// </param>
        public static void RemoveCategory(Category category)
        {
            categoryList.Remove(category);
        }

        // This dictionary keeps track of the categories associated with a client category ID
        static Dictionary<int, IEnumerable<Category>> cachedList = new Dictionary<int, IEnumerable<Category>>();

        /// <summary>
        /// Gets the categories valid for a client.
        /// </summary>
        /// <returns>
        /// The categories.
        /// </returns>
        /// <param name='client'>
        /// Client instance
        /// </param>
        public static IEnumerable<Category> GetCategories(Client client)
        {
            if (client == null || client.CategoryId == 0)
                return Category.Categories.Where(x => x.VisibleIn == 0 || x.VisibleIn == 2);

            if (cachedList.ContainsKey(client.CategoryId))
                return cachedList[client.CategoryId].Where(x => x.VisibleIn == 0 || x.VisibleIn == 2);

            var clientCategory = ClientCategoryProducts.Find(client.CategoryId);
            if (clientCategory == null)
                return new List<Category>();
            var categoriesId = (from product in clientCategory.Products
                                select product.CategoryId).Distinct();

            IEnumerable<Category> list = from category in Category.Categories
                                         where categoriesId.Contains(category.CategoryId)
                                         orderby category.Name
                                         select category;

            cachedList.Add(client.CategoryId, list);

            return list.Where(x => x.VisibleIn == 0 || x.VisibleIn == 2);
        }

        internal static void RemoveEmptyCategories()
        {
            var catsId = Product.Products.Select(x => x.CategoryId).Distinct().ToList();

            for (int i = 0; i < Categories.Count; i++)
            {
                if (!catsId.Contains(Categories[i].CategoryId) && !Categories.Any(x => x.ParentCategoryId == Categories[i].CategoryId))
                {
                    Categories.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void ClearExpandedStatus()
        {
            foreach (var cat in Categories)
                cat.Expanded = false;
        }
    }
}



