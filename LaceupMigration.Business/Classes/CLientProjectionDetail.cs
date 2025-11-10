





using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class ClientProjectionDetail
    {
        public static List<ClientProjectionDetail> List = new List<ClientProjectionDetail>();

        public string orderUniqueId { get; set; }
        public int productId { get; set; }
        public double qty { get; set; }

        public DateTime when { get; set; }

        public Client client { get; set; }

        public string filename { get; set; }

        public List<QtyHistoryByDate> history = new List<QtyHistoryByDate>();

        public ClientProjectionDetail(string orderId, int prodId, double qty, Client client, DateTime date)
        {
            this.orderUniqueId = orderId;
            this.productId = prodId;
            this.qty = qty;
            this.client = client;
            this.when = date;

            this.filename = System.IO.Path.Combine(Config.HistoryModificationPath, orderUniqueId + "_" + productId);

        }

        public static void Load()
        {
            if (!Directory.Exists(Config.HistoryModificationPath))
                return;

            DirectoryInfo dir = new DirectoryInfo(Config.HistoryModificationPath);

            foreach (var file in dir.GetFiles())
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    string line = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            var parts = line.Split(new char[] { (char)20 });

                            var orderId = parts[0];
                            var productId = Convert.ToInt32(parts[1]);
                            var clientId = Convert.ToInt32(parts[2]);
                            var qty = Convert.ToDouble(parts[3]);
                            var when = new DateTime(Convert.ToInt64(parts[4]));

                            ClientProjectionDetail proj = new ClientProjectionDetail(orderId, productId, qty, Client.Find(clientId), when);

                            List.Add(proj);
                        }
                        catch(Exception ex)
                        {
                            if(File.Exists(file.FullName))
                                File.Delete(file.FullName);
                        }

                    }
                }
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Config.HistoryModificationPath))
                Directory.CreateDirectory(Config.HistoryModificationPath);

            if (File.Exists(filename))
                File.Delete(filename);

            using (StreamWriter writer = new StreamWriter(filename))
            {
                string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                    (char)20,
                    orderUniqueId,
                    productId,
                    client.ClientId,
                    qty,
                    when.Ticks
                    );

                writer.WriteLine(line);
            }
        }
    }

    public class QtyHistoryByDate
    {
        public DateTime historyDate { get; set; }

        public double qty { get; set; }
    }
}