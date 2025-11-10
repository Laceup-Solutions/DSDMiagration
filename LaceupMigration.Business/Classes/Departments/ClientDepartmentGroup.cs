
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ClientDepartmentGroup
    {
        public static List<ClientDepartmentGroup> List = new List<ClientDepartmentGroup>();

        public ClientDepartmentGroup()
        {
            this.ClientClientDepartmentGroups = new HashSet<ClientClientDepartmentGroup>();
            this.DepartmentClientDepartmentGroups = new HashSet<DepartmentClientDepartmentGroup>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public string ExtraFields { get; set; }
        public int Status { get; set; }

        public virtual ICollection<ClientClientDepartmentGroup> ClientClientDepartmentGroups { get; set; }
        public virtual ICollection<DepartmentClientDepartmentGroup> DepartmentClientDepartmentGroups { get; set; }
    }
}