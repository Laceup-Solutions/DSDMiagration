





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;
using System.IO;

namespace LaceupMigration
{
    public class ClientSalesProjection
    {
        static List<ClientSalesProjection> clientProj = new List<ClientSalesProjection>();

        public static List<ClientSalesProjection> List { get { return clientProj; } }

        public static ClientSalesProjection CalculateProjection(Client client, DateTime? nextServiceDate = null)
        {
            if (nextServiceDate == null)
            {
                var routeEx = RouteEx.GetNextVisit(client.ClientId);
                if (routeEx != null)
                    nextServiceDate = routeEx.Date;
                else
                    nextServiceDate = DateTime.MinValue;
            }

            var cp = new ClientSalesProjection() { Client = client };
            cp.SetNextServiceDate(nextServiceDate.Value);

            List.Add(cp);

            Save();

            return cp;
        }

        public Client Client { get; set; }
        public DateTime NextServiceDate { get; set; }

        Dictionary<int, double> productProjections = new Dictionary<int, double>();

        public double GetProjectionForProduct(Product p)
        {
            double projection = 0;

            productProjections.TryGetValue(p.ProductId, out projection);

            return projection;
        }

        int GetNumberWeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);

            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                time = time.AddDays(3);

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        int DecreaseNumberOfWeek(ref DateTime date)
        {
            date = date.AddDays(-7);
            return GetNumberWeekOfYear(date);
        }

        public List<OrderHistory> GetSalesHistory()
        {
            var weekDate = DateTime.Today;

            var cWeekNumber = DecreaseNumberOfWeek(ref weekDate);

            List<OrderHistory> history = new List<OrderHistory>();

            var list = OrderHistory.History.Where(x => x.ClientId == Client.ClientId).OrderByDescending(x => x.When).ToList();

            foreach (var item in list)
            {
                var iw = GetNumberWeekOfYear(item.When);

                if (iw > cWeekNumber)
                    iw -= 52;

                if (iw < cWeekNumber - Config.WeeksOfSalesHistory)
                    continue;

                history.Add(item);
            }

            return history;
        }

        public List<OrderHistory> GetSalesHistoryForProduct(Product p)
        {
            return GetSalesHistory().Where(x => x.ProductId == p.ProductId).ToList();
        }

        public void SetNextServiceDate(DateTime date)
        {
            NextServiceDate = date;

            productProjections = new Dictionary<int, double>();

            if (NextServiceDate == DateTime.MinValue)
                return;

            var history = GetSalesHistory().GroupBy(x => x.ProductId);

            foreach (var group in history)
            {
                var productId = group.Key;

                productProjections.Add(productId, Math.Round(GetAvgSoldByCustomer(productId)));
                continue;
            }

        }

        public void SetNextServiceDateForLoad(DateTime date, DateTime nextServiceDate)
        {
            NextServiceDate = date;

            productProjections = new Dictionary<int, double>();

            if (NextServiceDate == DateTime.MinValue)
                return;

            var history = GetSalesHistory().GroupBy(x => x.ProductId);

            foreach (var group in history)
            {
                var productId = group.Key;

                productProjections.Add(productId, Math.Round(GetAvgSoldByCustomer(productId, nextServiceDate)));
                continue;
            }

        }

        static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string tempFile = Config.ClientSalesProjectionFile;

                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        foreach (var item in clientProj)
                        {
                            writer.Write(item.Client.ClientId.ToString(CultureInfo.InvariantCulture));
                            writer.Write(DataLineSplitter);
                            writer.WriteLine(item.NextServiceDate.Ticks.ToString());
                        }

                        writer.Close();
                    }
                }
                finally
                {
                }
            }
        }

        static char[] DataLineSplitter = new char[] { (char)20 };

        public static void Load()
        {
            clientProj = new List<ClientSalesProjection>();

            if (!File.Exists(Config.ClientSalesProjectionFile))
                return;

            using (StreamReader reader = new StreamReader(Config.ClientSalesProjectionFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(DataLineSplitter);

                    var clientId = Convert.ToInt32(parts[0]);
                    var nextServiceDate = new DateTime(Convert.ToInt64(parts[1]));

                    var client = Client.Find(clientId);
                    if (client == null)
                        continue;

                    CalculateProjection(client, nextServiceDate);
                }
            }
        }

        double GetAvgSoldByCustomer(int productId, DateTime newServiceDate)
        {
            double[,] matrix = new double[Config.WeeksOfSalesHistory, 7];

            var history = GetSalesHistory().Where(x => x.ProductId == productId).ToList();

            var currentVisit = history.FirstOrDefault();

            int i = Config.WeeksOfSalesHistory - 1;
            int j = (int)currentVisit.When.Date.DayOfWeek;

            for (int h = 1; h < history.Count; h++)
            {
                var days = currentVisit.When.Date.Subtract(history[h].When.Date).Days;
                var avgPerDay = currentVisit.Sold_Qty;
                if (currentVisit.Sold_UoM != null)
                    avgPerDay *= currentVisit.Sold_UoM.Conversion;

                avgPerDay /= days;

                for (; i >= 0; i--)
                {
                    for (; j >= 0; j--)
                    {
                        if (days == 0)
                            break;

                        matrix[i, j] = avgPerDay;
                        days--;
                    }

                    if (days == 0)
                        break;

                    j = 6;
                }

                currentVisit = history[h];
            }

            double[] avgSales = new double[7];

            for (int x = 0; x < 7; x++)
            {
                double totalSales = 0;

                for (int y = 0; y < matrix.GetLength(0); y++)
                    totalSales += matrix[y, x];

                avgSales[x] = totalSales / matrix.GetLength(0);
            }

            double AvgSoldByCustomer = 0;

            DateTime date = NextServiceDate;

            while (date.CompareTo(newServiceDate) <= 0)
            {
                AvgSoldByCustomer += avgSales[(int)date.DayOfWeek];
                date = date.AddDays(1);
            }

            return AvgSoldByCustomer;
        }
        double GetAvgSoldByCustomer(int productId)
        {
            double[,] matrix = new double[Config.WeeksOfSalesHistory, 7];

            var history = GetSalesHistory().Where(x => x.ProductId == productId).ToList();

            var currentVisit = history.FirstOrDefault();

            int i = Config.WeeksOfSalesHistory - 1;
            int j = (int)currentVisit.When.Date.DayOfWeek;

            for (int h = 1; h < history.Count; h++)
            {
                var days = currentVisit.When.Date.Subtract(history[h].When.Date).Days;
                var avgPerDay = currentVisit.Sold_Qty;
                if (currentVisit.Sold_UoM != null)
                    avgPerDay *= currentVisit.Sold_UoM.Conversion;

                avgPerDay /= days;

                for (; i >= 0; i--)
                {
                    for (; j >= 0; j--)
                    {
                        if (days == 0)
                            break;

                        matrix[i, j] = avgPerDay;
                        days--;
                    }

                    if (days == 0)
                        break;

                    j = 6;
                }

                currentVisit = history[h];
            }

            double[] avgSales = new double[7];

            for (int x = 0; x < 7; x++)
            {
                double totalSales = 0;

                for (int y = 0; y < matrix.GetLength(0); y++)
                    totalSales += matrix[y, x];

                avgSales[x] = totalSales / matrix.GetLength(0);
            }

            double AvgSoldByCustomer = 0;

            DateTime date = DateTime.Today.AddDays(1);

            while (date.CompareTo(NextServiceDate) <= 0)
            {
                AvgSoldByCustomer += avgSales[(int)date.DayOfWeek];
                date = date.AddDays(1);
            }

            return AvgSoldByCustomer;
        }

        public static void Clear()
        {
            List.Clear();
            Save();
        }
    }
}