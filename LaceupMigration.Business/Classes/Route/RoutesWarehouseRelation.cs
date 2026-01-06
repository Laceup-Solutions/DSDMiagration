
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{

    public partial class RoutesWarehouseRelation 
    {
        static List<RoutesWarehouseRelation> list = new List<RoutesWarehouseRelation>();
        public static IList<RoutesWarehouseRelation> List
        {
            get { return list.ToList(); }
        }

        public static void Clear()
        {
            list.Clear();
        }

        public RoutesWarehouseRelation()
        {
        }
        public int Id { get; set; }
        public int RouteSiteId { get; set; }
        public int LocationSiteId { get; set; }
        public string LocateName { get; set; }
       
        public static void AddRelation(RoutesWarehouseRelation relation)
        {
            list.Add(relation);
        }

        public static void DeserializeRouteRelation(string tempFile)
        {
            RoutesWarehouseRelation.Clear();

            string line = string.Empty;

            if (!File.Exists(Config.RouteRelationStorePath))
                return;

            try
            {
                using (StreamReader reader = new StreamReader(tempFile))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(new char[] { (char)20 });

                        var routeRelation = new RoutesWarehouseRelation();
                        routeRelation.Id = Convert.ToInt32(parts[0]);
                        routeRelation.RouteSiteId = Convert.ToInt32(parts[1]);
                        routeRelation.LocationSiteId = Convert.ToInt32(parts[2]);

                        if(parts.Length > 3)
                            routeRelation.LocateName = parts[3];

                        RoutesWarehouseRelation.AddRelation(routeRelation);
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Load()
        {
            DeserializeRouteRelation(Config.RouteRelationStorePath);
        }
    }
}