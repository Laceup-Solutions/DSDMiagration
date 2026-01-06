using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class Shipment
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ExtraFields { get; set; }

        public DateTime Date { get; set; }

        public int TruckId { get; set; }

        public int DriverId { get; set; }

        public string TruckName { get; set; }

        public static Shipment CurrentShipment { get; set; }
    }
}