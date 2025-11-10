using System;
using System.Globalization;
using System.Linq;

namespace LaceupMigration
{
    public class ButlerInvoiceIdProvider : IInvoiceIdProvider
    {
        public string GetId(Batch batch)
        {
            if (Config.CurrentOrderDate != DateTime.Today)
            {
                Config.CurrentOrderDate = DateTime.Today;
                Config.CurrentOrderId = 1;
            }
            else
                Config.CurrentOrderId = Config.CurrentOrderId + 1;

            Config.SaveCurrentOrderId();

            var routeName = string.Empty;
            var truck = Truck.Trucks.FirstOrDefault(x => x.DriverId == Config.SalesmanId);

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null && !string.IsNullOrEmpty(salesman.RouteNumber))
                routeName = salesman.RouteNumber;

            if (truck != null && string.IsNullOrEmpty(routeName))
            {
                routeName = truck.Name.ToLower();
                routeName = routeName.ToLower().Replace("rt", "");
                routeName = routeName.ToLower().Replace("route", "");
                routeName = routeName.Trim();
            }

            var year = DateTime.Today.Year.ToString().Substring(3);
            var dateOrdinal = DateTime.Today.DayOfYear.ToString("000");
            var orderid = Config.CurrentOrderId.ToString("D2");

            return routeName + year + dateOrdinal + orderid;
        }

        public string GetId(Order order)
        {
            if (Config.CurrentOrderDate != DateTime.Today)
            {
                Config.CurrentOrderDate = DateTime.Today;
                Config.CurrentOrderId = 1;
            }
            else
                Config.CurrentOrderId = Config.CurrentOrderId + 1;

            Config.SaveCurrentOrderId();

            var routeName = string.Empty;
            var truck = Truck.Trucks.FirstOrDefault(x => x.DriverId == Config.SalesmanId);

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null && !string.IsNullOrEmpty(salesman.RouteNumber))
                routeName = salesman.RouteNumber;

            if (truck != null && string.IsNullOrEmpty(routeName))
            {
                routeName = truck.Name.ToLower();
                routeName = routeName.ToLower().Replace("rt", "");
                routeName = routeName.ToLower().Replace("route", "");
                routeName = routeName.Trim();
            }

            var year = DateTime.Today.Year.ToString().Substring(3);
            var dateOrdinal = DateTime.Today.DayOfYear.ToString("000");
            var orderid = Config.CurrentOrderId.ToString("D2");
            return routeName + year + dateOrdinal + orderid;
        }
    }
}