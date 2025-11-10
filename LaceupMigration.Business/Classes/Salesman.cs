using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public class Salesman 
    {
        public static Salesman CurrentSalesman { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string OriginalId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool CreatedLocally { get; set; }

        public int InventorySiteId { get; set; }

        public string RouteNumber { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public string ExtraProperties { get; set; }

        public string PresalePrefix { get; set; }

        public string PrintedPrefix { get; set; }

        public string SequencePrefix { get; set; }

        public DateTime? SequenceExpirationDate { get; set; }

        public int SequenceFrom{ get; set; }

        public int SequenceTo { get; set; }

        public string SequenceCAI { get; set; }

        public string Loginname { get; set; }

        public int BranchId { get; set; }

        public SalesmanRole Roles { get; set; }

        static List<Salesman> salesmen = new List<Salesman>();

        /// <summary>
        /// Add a category to the list
        /// </summary>
        /// <param name="category">Item to add</param>
        public static void AddSalesman(Salesman salesman)
        {
            salesmen.Add(salesman);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<Salesman> List { get { return salesmen.Where(x => x.IsActive).ToList();  } }

        public static List<Salesman> FullList { get { return salesmen; } }

        public static void Clear()
        {
            salesmen.Clear();
        }
    }

    [Flags]
    public enum SalesmanRole
    {
        Picker = 1,
        Checker = 4,
        Receiver = 8,
        Order_Entry = 16,
        NoPaymentEntry = 32,
        Administrator = 64,
        NoMenuAccess = 128,
        ReportViewer = 256,
        NoUpdateData = 512,
        Driver = 1024,
        Presale = 2048,
        DSD = 4096,
        BackOffice = 8192,
        AllRolesBefore = 16383,
        Supervisor = 16384,
        Survey = 32768,
        Merchandiser = 65536,
        Independent = 131072,
        CannotCreateInvoice = 262144,
        SelfService = 524288,
        NoButton = 1048576,
        All = 2147483647
    };
}