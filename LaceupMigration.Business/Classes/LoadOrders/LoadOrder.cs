using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LaceupMigration
{
    public class LoadOrder
    {
        public static DateTime Date { get; set; }

        public static string Term { get; set; }

        public static int SalesmanId { get; set; }

        public static string UniqueId { get; set; }

        public static string Comment { get; set; }

        public static int SiteId { get; set; }

        public static string PrintedOrderId { get; set; }

        public static IList<LoadOrderDetail> List = new List<LoadOrderDetail>();

        public static void LoadList()
        {
            if (File.Exists(Config.LoadOrderFile))
            {
                List.Clear();
                using (StreamReader reader = new StreamReader(Config.LoadOrderFile))
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(new char[] { (char)20 });

                    Date = Convert.ToDateTime(parts[0], CultureInfo.InvariantCulture);

                    if (parts.Length > 1)
                        SalesmanId = Convert.ToInt32(parts[1]);
                    if (parts.Length > 2)
                        Term = parts[2];
                    if (parts.Length > 3)
                        UniqueId = parts[3];
                    if (parts.Length > 4)
                        Comment = parts[4];
                    if(parts.Length > 5)
                        SiteId = Convert.ToInt32(parts[5]);
                    if (parts.Length > 6)
                        PrintedOrderId = parts[6];

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            parts = line.Split(new char[] { (char)20 });
                            int productID = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                            float qtyty = Convert.ToSingle(parts[1], CultureInfo.InvariantCulture);
                            int uomId = Convert.ToInt32(parts[2], CultureInfo.InvariantCulture);

                            if (parts.Length > 3)
                                UniqueId = parts[3];

                            LoadOrderDetail btq = new LoadOrderDetail();
                            btq.Product = Product.Products.FirstOrDefault(x => x.ProductId == productID);
                            btq.Qty = qtyty;
                            btq.UoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                            List.Add(btq);
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                        }
                    }
                    reader.Close();
                }
            }
        }

        public static void SaveList()
        {
            string tempFile = Config.LoadOrderFile;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    if (Date == DateTime.MinValue)
                        return;

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        writer.Write(Date.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine(SalesmanId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine((Term ?? "").ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine(UniqueId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine((Comment ?? "").ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine(SiteId.ToString(CultureInfo.InvariantCulture));
                        writer.Write((char)20);
                        writer.WriteLine(PrintedOrderId.ToString(CultureInfo.InvariantCulture));

                        int offset = 0;

                        foreach (var btq in List)
                        {
                            writer.Write(btq.Product.ProductId);
                            writer.Write((char)20);
                            writer.Write(btq.Qty);
                            writer.Write((char)20);
                            writer.Write(btq.UoM != null ? btq.UoM.Id : 0);

                            if (offset == 0)
                            {
                                writer.Write((char)20);
                                writer.Write(UniqueId);
                            }

                            writer.WriteLine();

                            offset++;
                        }
                        writer.Close();
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void SaveListFromOrders()
        {
            Date = DateTime.MinValue;
            List.Clear();

            //Solo hay un load en el sistema
            var load = Order.Orders.FirstOrDefault(x => x != null && x.OrderType == OrderType.Load && !x.PendingLoad);
            if (load != null)
            {
                Date = load.ShipDate;
                SalesmanId = load.SalesmanId;
                UniqueId = load.UniqueId;
                Comment = load.Comments;
                SiteId = load.SiteId;
                PrintedOrderId = load.PrintedOrderId;

                if (Config.UseTermsInLoadOrder)
                {
                    if (!string.IsNullOrEmpty(load.ExtraFields))
                    {
                        var term = UDFHelper.GetSingleUDF("cashTerm", load.ExtraFields);
                        if (!string.IsNullOrEmpty(term))
                            Term = "cashTerm=" + term;
                    }
                }

                foreach (var item in load.Details)
                {
                    var li = new LoadOrderDetail();
                    li.Product = item.Product;
                    li.Qty = item.Qty;
                    if (li.Product.SoldByWeight && !li.Product.InventoryByWeight && li.Product.Weight > 0 && !Config.UsePallets)
                        li.Qty = (float)(li.Product.Weight * item.Qty);
                    li.UoM = item.UnitOfMeasure;

                    li.Comments = item.Comments;
                    li.Lot = item.Lot;
                    
                    List.Add(li);
                }
            }

            SaveList();
        }
    }
}

