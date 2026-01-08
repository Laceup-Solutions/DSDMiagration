using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace LaceupMigration
{
    public enum BatchStatus { Open = 0, Locked = 1 }

    public class Batch
    {
        private static List<Batch> list = new List<Batch>();

        string fileName;

        static int lastId = 0;

        public int Id { get; set; }

        public string UniqueId { get; set; }

        public DateTime ClockedIn { get; set; }

        public DateTime ClockedOut { get; set; }

        public Client Client { get; set; }

        public BatchStatus Status { get; set; }

        public string PrintedId { get; set; }

        public string ProjectionId { get; set; }

        public bool OverrideOneDoc()
        {
            var c = Order.Orders.Sum(x => (x.BatchId == Id && x.OrderType == OrderType.Order) ? 1 : 0);

            return c > 1;
        }

        public bool Voided()
        {
            return Order.Orders.Where(x => x.BatchId == Id).All(x => x.Voided);
        }

        public Batch(Client client)
        {
            if (client != null)
            {
                Client = client;
                this.Id = ++lastId;
                UniqueId = Guid.NewGuid().ToString();
                ClockedIn = DateTime.Now;
                ClockedOut = DateTime.MinValue;
                PrintedId = string.Empty;
                Save();
            }
            list.Add(this);
        }

        public void Delete()
        {
            // Delete from the list
            list.Remove(this);
            // Delete from t he file
            if (!string.IsNullOrEmpty(fileName))
                if (File.Exists(this.fileName))
                    File.Delete(this.fileName);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(fileName))
                this.fileName = Path.Combine(Config.BatchPath, Guid.NewGuid().ToString());

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    using (StreamWriter writer = new StreamWriter(this.fileName))
                    {
                        string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}",
                            (char)20,
                            Id,
                            ClockedIn.Ticks,
                            ClockedOut.Ticks,
                            this.Client.ClientId,
                            (int)Status,
                            PrintedId,
                            string.IsNullOrEmpty(ProjectionId) ? "" : ProjectionId,
                            DollyPicked ? "1" : "0",
                            UniqueId);
                        writer.WriteLine(line);
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public double Total()
        {
            return Orders().Sum(x => x.OrderTotalCost());
        }

        public double Total(TransactionType type)
        {
            if (type == TransactionType.All)
                return Total();

            return Orders().Sum(x => x.TransactionType == type ? x.OrderTotalCost() : 0);
        }

        public IList<Order> Orders()
        {
            return Order.Orders.Where(x => x.BatchId == Id).ToList();
        }

        private void Deserialize(StreamReader reader)
        {
            string line = reader.ReadLine();
            string[] parts = line.Split(new char[] { (char)20 });

            Id = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);

            if (lastId < Id)
                lastId = Id;

            ClockedIn = new DateTime(Convert.ToInt64(parts[1], System.Globalization.CultureInfo.InvariantCulture));
            ClockedOut = new DateTime(Convert.ToInt64(parts[2], System.Globalization.CultureInfo.InvariantCulture));
            var clientId = Convert.ToInt32(parts[3]);
            this.Client = Client.Find(clientId);
            if (Client == null)
            {
                string msg = "batch had a reference to a null customer" + parts[3];
                Logger.CreateLog(msg);
                Client = Client.CreateTemporalClient(clientId);
            }
            Status = (BatchStatus)Convert.ToInt32(parts[4]);
            PrintedId = parts[5];

            if (parts.Length > 6)
                ProjectionId = parts[6];

            if (parts.Length > 7)
                DollyPicked = Convert.ToInt32(parts[7]) > 0;

            if (parts.Length > 8)
                UniqueId = parts[8];
            else
                UniqueId = Guid.NewGuid().ToString();
        }

        static internal void LoadFromFile(string file)
        {
            try
            {
                Batch batch = new Batch(null);
                batch.fileName = file;
                using (StreamReader reader = new StreamReader(batch.fileName))
                {
                    batch.Deserialize(reader);
                }

                if (batch.Client == null)
                {
                    Remove(batch);
                    Logger.CreateLog("batch had a reference to a null customer");
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public static void Add(Batch batch)
        {
            list.Add(batch);
        }

        public static void Remove(Batch batch)
        {
            list.Remove(batch);
        }

        public static void ClearList()
        {
            var copy = list.ToArray();
            foreach (var i in copy)
                i.Delete();
            list.Clear();
        }

        public static IList<Batch> List
        {
            get
            {
                return list;
            }
        }

        public bool DollyPicked { get; set; }
    }
}

