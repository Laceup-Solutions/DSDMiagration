
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class DepartmentClientDepartmentGroup
    {
        public static List<DepartmentClientDepartmentGroup> List = new List<DepartmentClientDepartmentGroup>();

        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int ClientDepartmentGroupId { get; set; }
        public string ExtraFields { get; set; }
    }
}