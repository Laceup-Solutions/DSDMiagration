using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class ParLevelHistory
    {
        public Client Client { get; set; }

        public Product Product { get; set; }

        public int DayOfWeek { get; set; }

        public float OldPar { get; set; }

        public float NewPar { get; set; }

        public float Counted { get; set; }

        public float Sold { get; set; }

        public float Credit { get; set; }

        public DateTime Date { get; set; }

        public string Department { get; set; }

        static List<ParLevelHistory> history = new List<ParLevelHistory>();

        public static List<ParLevelHistory> Histories { get { return history; } }

        public static void Save()
        {
            using (StreamWriter stream = new StreamWriter(Config.ParLevelHistoryFile))
            {
                foreach (var history in Histories)
                {
                    SerializeParLevelHistory(history, stream);
                }
            }
               
        }
        public static void SerializeParLevelHistory(ParLevelHistory detail, StreamWriter stream)
        {
            stream.WriteLine(string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}", (char)20,
                0,
                detail.Client.ClientId,
                detail.Product.ProductId,
                detail.DayOfWeek,
                detail.OldPar,
                detail.NewPar,
                detail.Date,
                detail.Counted,
                detail.Sold,
                detail.Credit,
                detail.Department
                ));
        }
    }
}