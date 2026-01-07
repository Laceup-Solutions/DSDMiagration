
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{

    public partial class Truck 
    {
        static List<Truck> truckList = new List<Truck>();
        public static IList<Truck> Trucks
        {
            get { return truckList.ToList(); }
        }

        public static void Clear()
        {
            truckList.Clear();
        }

        public Truck()
        {
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExtraFields { get; set; }
        public Nullable<int> InventorySiteId { get; set; }
        public Nullable<int> DriverId { get; set; }
        public Nullable<int> BranchId { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string Color { get; set; }
        public Nullable<int> CurrentDriver { get; set; }
        public Nullable<double> MinVolume { get; set; }
        public Nullable<double> MaxVolume { get; set; }
        public Nullable<double> MinWeight { get; set; }
        public Nullable<double> MaxWeight { get; set; }
        public Nullable<System.DateTime> BreakStart { get; set; }
        public Nullable<System.DateTime> BreakEnd { get; set; }
        public Nullable<System.DateTime> DayStart { get; set; }
        public Nullable<System.DateTime> DayEnd { get; set; }
        public Nullable<int> MaxHourWorked { get; set; }
        public Nullable<int> MaxStop { get; set; }
        public Nullable<int> MaxPallets { get; set; }
        public Nullable<decimal> DepartureLatitude { get; set; }
        public Nullable<decimal> DepartureLongitude { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Description { get; set; }
        public string TagNumber { get; set; }
        public bool EmptyTruck { get; set; }
        public Nullable<int> OverShortSiteId { get; set; }
        public Nullable<int> LoadingSiteId { get; set; }
        public string OriginalId { get; set; }
        public virtual Salesman Salesman { get; set; }

        public static void AddTruck(Truck truck)
        {
            truckList.Add(truck);
        }

        public static void DeserializeTrucks(string tempFile)
        {
            Truck.Clear();

            string line = string.Empty;

            if (!File.Exists(Config.TrucksStoreFile))
                return;

            try
            {
                using (StreamReader reader = new StreamReader(tempFile))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(new char[] { (char)20 });

                        var truck = new Truck();
                        truck.Id = Convert.ToInt32(parts[0]);
                        truck.Name = parts[1];

                        if (parts.Count() > 2)
                            truck.Description = parts[2];

                        if (parts.Count() > 3)
                            truck.TagNumber = parts[3];

                        if (parts.Count() > 4)
                            truck.ExtraFields = parts[4];
                        if (parts.Count() > 5)
                            truck.InventorySiteId = Convert.ToInt32(parts[5]);
                        if (parts.Count() > 6)
                            truck.OverShortSiteId = Convert.ToInt32(parts[6]);
                        if (parts.Count() > 7)
                            truck.DriverId = Convert.ToInt32(parts[7]);
                        if (parts.Count() > 8)
                            truck.Color = parts[8];
                        if (parts.Count() > 9)
                            truck.EmptyTruck = Convert.ToInt32(parts[9]) > 0;
                        if (parts.Count() > 10)
                            truck.MinVolume = Convert.ToInt32(parts[10]);
                        if (parts.Count() > 11)
                            truck.MaxVolume = Convert.ToInt32(parts[11]);
                        if (parts.Count() > 12)
                            truck.MinWeight = Convert.ToInt32(parts[12]);
                        if (parts.Count() > 13)
                            truck.MaxWeight = Convert.ToInt32(parts[13]);
                        if (parts.Count() > 14)
                            truck.BreakStart = new DateTime(Convert.ToInt64(parts[14]));
                        if (parts.Count() > 15)
                            truck.BreakEnd = new DateTime(Convert.ToInt64(parts[15]));
                        if (parts.Count() > 16)
                            truck.DayStart = new DateTime(Convert.ToInt64(parts[16]));
                        if (parts.Count() > 17)
                            truck.DayEnd = new DateTime(Convert.ToInt64(parts[17]));
                        if (parts.Count() > 18)
                            truck.MaxHourWorked = Convert.ToInt32(parts[18]);
                        if (parts.Count() > 19)
                            truck.MaxStop = Convert.ToInt32(parts[19]);
                        if (parts.Count() > 20)
                            truck.MaxPallets = Convert.ToInt32(parts[20]);
                        if (parts.Count() > 21)
                            truck.DepartureLatitude = Convert.ToDecimal(parts[21]);
                        if (parts.Count() > 22)
                            truck.DepartureLongitude = Convert.ToDecimal(parts[22]);
                        if (parts.Count() > 23)
                            truck.IsActive = (parts[23].ToLowerInvariant() == "true");

                        Truck.AddTruck(truck);
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Load()
        {
            DeserializeTrucks(Config.TrucksStoreFile);
        }
    }
}