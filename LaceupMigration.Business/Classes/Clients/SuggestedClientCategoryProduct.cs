





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SuggestedClientCategoryProduct
    {
        public static List<SuggestedClientCategoryProduct> List = new List<SuggestedClientCategoryProduct>();
        public int Id { get; set; }
        public int SuggestedClientCategoryId { get; set; }
        public int ProductId { get; set; }

        public Product Product
        {
            get
            {
                return Product.Products.FirstOrDefault(x => x.ProductId == ProductId);
            }
        }
        public SuggestedClientCategory SuggestedClientCategory
        {
            get
            {
                return SuggestedClientCategory.List.FirstOrDefault(x => x.Id == SuggestedClientCategoryId);
            }
        }
    }
}