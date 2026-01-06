





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class AreaClient
    {
        public static List<AreaClient> List = new List<AreaClient>();
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int ClientId { get; set; }

        public virtual Area Area
        {
            get
            {
                return Area.List.FirstOrDefault(x => x.Id == AreaId);
            }
        }
        public virtual Client Client
        {
            get
            {
                return Client.Find(ClientId);
            }
        }
    }
}