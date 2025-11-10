
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ClientClientDepartmentGroup
    {
        public static List<ClientClientDepartmentGroup> List = new List<ClientClientDepartmentGroup>();
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ClientDepartmentGroupId { get; set; }
        public string ExtraFields { get; set; }
    }
}