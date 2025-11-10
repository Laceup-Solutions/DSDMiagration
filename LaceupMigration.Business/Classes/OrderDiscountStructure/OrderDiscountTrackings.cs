






using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;

namespace LaceupMigration
{
    public class OrderDiscountTrackings
    {
        public static List<OrderDiscountTrackings> List = new List<OrderDiscountTrackings>();
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderDiscountId { get; set; }
        public double DiscountGiven { get; set; }
        public System.DateTime DateGiven { get; set; }
        public string ExtraFields { get; set; }

        public virtual Order Order
        {
            get
            {
                return Order.Orders.FirstOrDefault(x => x.OrderId == OrderId);
            }

        }
        public virtual OrderDiscount OrderDiscount
        {
            get
            {
                return OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(Config.OrderDiscountTrackingPath))
                {
                    using (StreamReader reader = new StreamReader(Config.OrderDiscountTrackingPath))
                    {
                        string line = string.Empty;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(new char[] { (char)20 });
                            int Id = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                            int OrderId = Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                            int OrderDiscountId = Convert.ToInt32(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                            double DiscountGiven = Convert.ToDouble(parts[3], CultureInfo.InvariantCulture);
                            var DateGiven = new DateTime(Convert.ToInt64(parts[4], CultureInfo.InvariantCulture));
                            string ExtraFields = parts[5];

                            var orderDiscountTracking = new OrderDiscountTrackings()
                            {
                                Id = Id,
                                OrderId = OrderId,
                                OrderDiscountId = OrderDiscountId,
                                DateGiven = DateGiven,
                                DiscountGiven = DiscountGiven,
                                ExtraFields = ExtraFields
                            };

                            List.Add(orderDiscountTracking);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        public static void Save()
        {
            if (File.Exists(Config.OrderDiscountTrackingPath))
                File.Delete(Config.OrderDiscountTrackingPath);

            using (StreamWriter writer = new StreamWriter(Config.OrderDiscountTrackingPath))
            {
                foreach (var tracking in List)
                {
                    string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}",
                 (char)20,
                 tracking.Id.ToString(),                                           //0
                 tracking.OrderId.ToString(),                                      //1
                 tracking.OrderDiscountId.ToString(),                              //2
                 tracking.DiscountGiven.ToString(),                                //3
                 tracking.DateGiven.Ticks.ToString(),                              //4
                 (tracking.ExtraFields ?? string.Empty)                            //5
                 );

                    writer.WriteLine(line);
                }
            }
        }
    }
}