





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class CategoryProduct
    {
        public static IList<CategoryProduct> List
        {
            get
            {
                return list;
            }
        }

        private static List<CategoryProduct> list = new List<CategoryProduct>();

        public int id { get; set; }

        public int productId { get; set; }

        public int categoryId { get; set; }

        public static void Add(CategoryProduct catProd)
        {
            list.Add(catProd);
        }

        public static void Remove(CategoryProduct catProd)
        {
            list.Remove(catProd);
        }

        public static void ClearList()
        {
            var copy = list.ToArray();
            foreach (var i in copy)
                i.Delete();
            list.Clear();
        }

        public void Delete()
        {
            // Delete from the list
            list.Remove(this);
        }

    }
}