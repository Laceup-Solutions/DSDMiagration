





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class Area
    {
        public static List<Area> List = new List<Area>();

        public int Id { get; set; }
        public string Name { get; set; }
        public string ExtraFields { get; set; }
        public int Color { get; set; }
        public bool Active { get; set; }
        public bool ForDelivery { get; set; }

    }
}