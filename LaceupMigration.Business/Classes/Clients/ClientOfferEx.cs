using System.Collections.Generic;

namespace LaceupMigration
{
    class ClientOfferEx
    {
        static List<ClientOfferEx> categoryList = new List<ClientOfferEx>();
        public int ClientId { get; set; }
        public int OfferExId { get; set; }
        public string ExtraFields { get; set; }
        public static void Add(ClientOfferEx category)
        {
            categoryList.Add(category);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<ClientOfferEx> List
        {
            get { return categoryList; }
        }
    }
}