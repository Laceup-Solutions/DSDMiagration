
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class DepartmentProduct
    {
        public static List<DepartmentProduct> List = new List<DepartmentProduct>();
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int ProductId { get; set; }
        public string ExtraFields { get; set; }
    }
}