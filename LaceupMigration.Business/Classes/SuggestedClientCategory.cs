





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SuggestedClientCategory
    {
        public static List<SuggestedClientCategory> List = new List<SuggestedClientCategory>();

        public SuggestedClientCategory()
        {
            this.SuggestedClientCategoryClients = new List<SuggestedClientCategoryClient>();
            this.SuggestedClientCategoryProducts = new List<SuggestedClientCategoryProduct>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public List<SuggestedClientCategoryClient> SuggestedClientCategoryClients { get; set; }
        public List<SuggestedClientCategoryProduct> SuggestedClientCategoryProducts { get; set; }
    }
}