using System;
using System.Collections.Generic;

namespace LaceupMigration
{
   public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExtraFields { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Active { get; set; }
        public int Color { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int InventorySiteId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        /// <summary>
        /// Add a Branch to the list
        /// </summary>
        /// <param name="branch">Item to add</param>
        public static void Add(Branch branch)
        {
            List.Add(branch);
        }

        /// <summary>
        /// Returns the list of the branches.
        /// </summary>
        public static List<Branch> List { get; } = new List<Branch>();
    }
}