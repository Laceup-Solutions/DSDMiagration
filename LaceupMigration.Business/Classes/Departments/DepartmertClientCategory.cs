
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class DepartmertClientCategory
    {
        public static List<DepartmertClientCategory> List = new List<DepartmertClientCategory>();

        public DepartmertClientCategory()
        {
            this.DepartmentClientDepartmentGroups = new HashSet<DepartmentClientDepartmentGroup>();
            this.DepartmentProducts = new HashSet<DepartmentProduct>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string ExtraFields { get; set; }

        public virtual ICollection<DepartmentClientDepartmentGroup> DepartmentClientDepartmentGroups { get; set; }
        public virtual ICollection<DepartmentProduct> DepartmentProducts { get; set; }

        public static List<DepartmertClientCategory> GetDepartmentToClient(Client client)
        {
            /*Get Department Client*/
            var ListClientDepartmentGroupId = ClientClientDepartmentGroup.List.Where(x => x.ClientId == client.ClientId).Select(x => x.ClientDepartmentGroupId).Distinct().ToList();

            var ListDepartmentId = DepartmentClientDepartmentGroup.List.Where(x => ListClientDepartmentGroupId.Contains(x.ClientDepartmentGroupId)).Select(x => x.DepartmentId).Distinct().ToList();

            var ListDeaprtment = DepartmertClientCategory.List.Where(x => x.IsActive && ListDepartmentId.Contains(x.Id)).ToList();

            return ListDeaprtment;
        }
    }
}