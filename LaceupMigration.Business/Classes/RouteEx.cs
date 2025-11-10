using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace LaceupMigration
{
    public class RouteEx
    {
        public RouteEx()
        {
            OrdersInStop = new List<string>();
        }

        public int Id { get; set; }

        public DateTime Date { get; set; }

        public Client Client { get; set; }

        public Order Order { get; set; }

        public int Stop { get; set; }

        private int _locallySavedStop;

        public int LocallySavedStop
        {
            get
            {
                if (_locallySavedStop == 0)
                    return Stop;

                return _locallySavedStop;
            }
            set
            {
                _locallySavedStop = value;
            }
        }

        public bool Closed { get; set; }

        public bool FromDelivery { get; set; }

        public DateTime When { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
        
        public string ExtraFields { get; set; }

        public List<string> OrdersInStop { get; set; }

        public static List<RouteEx> Routes = new List<RouteEx>();

        public static void ClearAll()
        {
            Routes.Clear();

            if (File.Exists(Config.RouteExFile))
                File.Delete(Config.RouteExFile);
        }

        public static void Clear()
        {
            List<RouteEx> toRemove = new List<RouteEx>();

            foreach (var route in Routes)
            {
                if (route.Order != null)
                {
                    if (!route.Order.Finished && route.OrdersInStop.Count == 0)
                        toRemove.Add(route);
                }
                else if (route.Client != null && route.OrdersInStop.Count == 0)
                    toRemove.Add(route);
                else if (route.Client == null)
                    toRemove.Add(route);
            }

            foreach (var item in toRemove)
                Routes.Remove(item);
        }

        public static void Load()
        {
            if (!File.Exists(Config.RouteExFile))
                return;

            string lin;

            using (StreamReader reader = new StreamReader(Config.RouteExFile))
            {
                while ((lin = reader.ReadLine()) != null)
                {
                    try
                    {
                        var parts = lin.Split(new char[] { (char)20 });

                        var id = Convert.ToInt32(parts[0]);

                        var route = Routes.FirstOrDefault(x => x.Id == id);
                        bool addIt = false;

                        if (route == null)
                        {
                            route = new RouteEx();
                            addIt = true;
                        }

                        route.Id = id;
                        route.Date = Convert.ToDateTime(parts[1], CultureInfo.InvariantCulture);

                        var clientid = Convert.ToInt32(parts[2]);
                        route.Client = Client.Find(clientid);
                        if (clientid > 0 && route.Client == null)
                            Logger.CreateLog("LoadRoute route makes reference to a client that was not found: " + clientid);

                        var orderid = Convert.ToInt32(parts[3]);
                        route.Order = Order.Orders.FirstOrDefault(x => x.OrderId == orderid);
                        if (orderid > 0 && route.Order == null)
                            Logger.CreateLog("LoadRoute route makes reference to an order that was not found: " + orderid);

                        route.Stop = Convert.ToInt32(parts[4]);
                        route.Closed = Convert.ToInt32(parts[5]) > 0;
                        route.FromDelivery = Convert.ToInt32(parts[6]) > 0;

                        if (parts.Length > 7)
                            route.When = DateTime.FromBinary(Convert.ToInt64(parts[7]));

                        if (parts.Length > 8)
                            route.Latitude = Convert.ToDouble(parts[8]);

                        if (parts.Length > 9)
                            route.Longitude = Convert.ToDouble(parts[9]);

                        if (parts.Length > 10)
                            route.Date = DateTime.FromBinary(Convert.ToInt64(parts[10]));

                        if (parts.Length > 11)
                            route.DeserializeOrdersInStop(parts[11]);

                        if (parts.Length > 12)
                            route.LocallySavedStop = Convert.ToInt32(parts[12]);
                        
                        if (parts.Length > 13)
                            route.ExtraFields = parts[13];

                        if (addIt)
                        {
                            if (route.Client == null && route.Order == null)
                                Logger.CreateLog("route Load has both orders and customer in null: " + lin);
                            else
                                Routes.Add(route);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                }
            }
        }

        public static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(Config.RouteExFile))
                        File.Delete(Config.RouteExFile);

                    using (StreamWriter writer = new StreamWriter(Config.RouteExFile))
                    {
                        foreach (var route in Routes)
                        {
                            string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}" +
                                "{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}",
                                              (char)20,
                                              route.Id,                                             //0
                                              route.Date.ToString(CultureInfo.InvariantCulture),    //1
                                              route.Client == null ? 0 : route.Client.ClientId,     //2
                                              route.Order == null ? 0 : route.Order.OrderId,        //3
                                              route.Stop,                                           //4
                                              route.Closed ? "1" : "0",                             //5
                                              route.FromDelivery ? "1" : "0",                       //6
                                              route.When.Ticks,                                     //7
                                              route.Latitude,                                       //8
                                              route.Longitude,                                      //9
                                              route.Date.Ticks,                                     //10
                                              route.SerializeOrdersInStop(),                         //11
                                              route.LocallySavedStop,
                                              route.ExtraFields ?? ""
                                              );
                            writer.WriteLine(line);
                        }
                    }
                    
                    BackgroundDataSync.SyncRoute();
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static bool ContainsOrder(int id)
        {
            return Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == id) != null;
        }

        public void AddOrderToStop(string orderUniqueId)
        {
            if (!OrdersInStop.Contains(orderUniqueId))
            {
                OrdersInStop.Add(orderUniqueId);

                if (!Closed)
                {
                    When = DateTime.Now;
                    Latitude = DataAccess.LastLatitude;
                    Longitude = DataAccess.LastLongitude;
                }

                Closed = OrdersInStop.Count > 0; ;

                Save();
            }
        }

        public void RemoveOrderFromStop(string orderUniqueId)
        {
            OrdersInStop.Remove(orderUniqueId);

            Closed = OrdersInStop.Count > 0;

            if (!Closed)
                When = DateTime.MinValue;

            Save();
        }

        string SerializeOrdersInStop()
        {
            string s = "";

            foreach (var item in OrdersInStop)
            {
                if (!string.IsNullOrEmpty(s))
                    s += "|";
                s += item;
            }

            return s;
        }

        void DeserializeOrdersInStop(string s)
        {
            if (!string.IsNullOrEmpty(s))
                OrdersInStop = new List<string>(s.Split('|'));
            else
                OrdersInStop = new List<string>();
        }

        public static RouteEx GetNextVisit(int clientId)
        {
            var routes = Routes.Where(x => x.Client != null && x.Client.ClientId == clientId 
            && x.Date.CompareTo(DateTime.Now) > 0).OrderBy(x => x.Date).ToList();

            return routes.FirstOrDefault();
        }
    }
}

