





using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class POInventory
    {
        public static char[] DataLineSplitter = new char[] { (char)20 };

        public int Id { get; set; }
        public double Qty { get; set; }

        static List<POInventory> poList = new List<POInventory>();

        public static IEnumerable<POInventory> POList
        {
            get { return poList; }
        }

        public static void Clear()
        {
            poList.Clear();
        }

        public static void Add(POInventory item)
        {
            poList.Add(item);
        }

    }
}