using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace LaceupMigration
{
    [XmlType]
    public class DriverRoute
    {
        [XmlElement(Order = 1)]
        public int DriverId { get; set; }

        Salesman driver = null;
        public Salesman Driver
        {
            get
            {
                if (driver == null)
                    driver = Salesman.List.FirstOrDefault(x => x.Id == DriverId);
                return driver;
            }
        }

        [XmlElement(Order = 2)]
        public long DateTicks { get; set; }

        DateTime date = DateTime.MinValue;
        public DateTime Date
        {
            get
            {
                if (date == DateTime.MinValue)
                    date = new DateTime(DateTicks);
                return date;
            }
        }

        [XmlElement(Order = 3)]
        public List<DriverRouteDetails> Details { get; set; }   
        
        [XmlElement(Order = 4)]
        public bool Locked { get; set; }

        public DriverRoute()
        {
            Details = new List<DriverRouteDetails>();
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            Serialize(filename);
        }

        void Serialize(string filename)
        {
            try
            {
                // Creates XmlSerializer of the List<User> type
                XmlSerializer serializer = new XmlSerializer(GetType());

                // An alternative syntax could also be:
                //XmlSerializer serializer = new XmlSerializer(typeof(List<User>));

                // Creates a stream using which we'll serialize
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    // We call the Serialize() method and pass the stream created above as the first parameter
                    // The second parameter is the object which we want to serialize
                    serializer.Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static DriverRoute Load(string filename)
        {
            if (!File.Exists(filename))
                return null;

            return Deserialize(filename);
        }

        static DriverRoute Deserialize(string filename)
        {
            var route = new DriverRoute();

            try
            {
                XmlSerializer serializer = new XmlSerializer(route.GetType());
                using (StreamReader sr = new StreamReader(filename))
                {
                    route = (DriverRoute)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return null;
            }

            return route;
        }
    }

    [XmlType]
    public class DriverRouteDetails 
    {
        [XmlElement(Order = 1)]
        public int RouteId { get; set; }

        [XmlElement(Order = 2)]
        public int Stop { get; set; }

        [XmlElement(Order = 3)]
        public int ClientId { get; set; }

        [XmlElement(Order = 4)]
        public string ClientName { get; set; }

        [XmlElement(Order = 5)]
        public DriverRouteOrder Order { get; set; }

        [XmlElement(Order = 6)]
        public bool Deleted { get; set; }
    }

    [XmlType]
    public class DriverRouteOrder 
    {
        [XmlElement(Order = 1)]
        public int Id { get; set; }

        [XmlElement(Order = 2)]
        public int ClientId { get; set; }

        [XmlElement(Order = 3)]
        public string ClientName { get; set; }

        [XmlElement(Order = 4)]
        public string OrderNumber { get; set; }

        [XmlElement(Order = 5)]
        public long ShipDateTicks { get; set; }

        DateTime shipdate = DateTime.MinValue;
        public DateTime ShipDate
        {
            get
            {
                if (shipdate == DateTime.MinValue)
                    shipdate = new DateTime(ShipDateTicks);
                return shipdate;
            }
        }

        [XmlElement(Order = 6)]
        public int DriverId { get; set; }

        [XmlElement(Order = 7)]
        public string DriverName { get; set; }

        [XmlElement(Order = 8)]
        public double OrderTotal { get; set; }

        [XmlElement(Order = 9)]
        public List<DriverRouteOrderDetail> Details { get; set; }

        [XmlElement(Order = 10)]
        public int OrderType { get; set; }


        [XmlElement(Order = 11)]
        public int OrderStatus { get; set; }

        public bool Selected { get; set; }

        public DriverRouteOrder()
        {
            Details = new List<DriverRouteOrderDetail>();
        }
    }

    [XmlType]
    public class DriverRouteOrderDetail
    {
        [XmlElement(Order = 1)]
        public int ProductId { get; set; }

        [XmlElement(Order = 2)]
        public string ProductName { get; set; }

        [XmlElement(Order = 3)]
        public double Qty { get; set; }

        [XmlElement(Order = 4)]
        public string Lot { get; set; }

        [XmlElement(Order = 5)]
        public double Price { get; set; }

        [XmlElement(Order = 6)]
        public bool IsCredit { get; set; }
    }
}