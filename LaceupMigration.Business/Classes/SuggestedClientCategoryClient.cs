





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SuggestedClientCategoryClient
    {
        public static List<SuggestedClientCategoryClient> List = new List<SuggestedClientCategoryClient>();
        public int Id { get; set; }
        public int SuggestedClientCategoryId { get; set; }
        public int ClientId { get; set; }

        public Client Client
        {
            get
            {
                return Client.Find(ClientId);
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