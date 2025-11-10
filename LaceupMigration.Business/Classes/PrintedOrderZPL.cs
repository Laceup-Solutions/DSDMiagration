





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LaceupMigration
{
    public class PrintedOrderZPL
    {

        public static List<PrintedOrderZPL> PrintedOrders = new List<PrintedOrderZPL>();

        public string UniqueId { get; set; }

        public string ZPLString { get; set; }

        public string Filename { get => Path.Combine(Config.ZPLOrdersPrintedPath, UniqueId); }

        public PrintedOrderZPL(string uniqueId, string ZPLString)
        {
            this.UniqueId = uniqueId;
            this.ZPLString = ZPLString;

            PrintedOrders.Add(this);
        }

        public void Save()
        {
            try
            {
                if (File.Exists(Filename))
                    File.Delete(Filename);

                using (StreamWriter sw = File.CreateText(Filename))
                    sw.Write(ZPLString);
            }
            catch(Exception ex)
            {

            }
        }

        public static void LoadZPL()
        {
            try
            {
                PrintedOrders.Clear();

                DirectoryInfo info = new DirectoryInfo(Config.ZPLOrdersPrintedPath);
                var files = info.GetFiles();
                foreach (var f in files)
                {
                    var uniqueId = f.Name;
                    string ZPLString = File.ReadAllText(f.FullName);

                    var zpl = new PrintedOrderZPL(uniqueId, ZPLString);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}