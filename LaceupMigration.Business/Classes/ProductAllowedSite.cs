





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ProductAllowedSite
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SiteId { get; set; }
        public string ExtraFields { get; set; }

        public static List<ProductAllowedSite> List = new List<ProductAllowedSite>();
    }
}