using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;

namespace LaceupMigration
{
    public class SentOrderPackage
    {
        public List<Tuple<int, string>> AddedCustomers { get; set; }

        private DataSet dataSet;

        public string PackagePath
        {
            get;
            set;
        }

        public DateTime CreatedDate
        {
            get;
            set;
        }

        public List<SentOrder> Orders { get; set; }

        public IEnumerable<SentOrder> PackageOrders()
        {
            if (Orders == null)
            {
                Orders = new List<SentOrder>();
                try
                {
                    using (StreamReader stream = new StreamReader(PackagePath))
                    {
                        using (XmlTextReader reader = new XmlTextReader(stream))
                        {
                            dataSet = new DataSet();
                            dataSet.Locale = CultureInfo.InvariantCulture;
                            dataSet.ReadXml(reader, XmlReadMode.ReadSchema);
                        }
                    }
                    // Interprete the DS
                    AddedCustomers = new List<Tuple<int, string>>();
                    if (dataSet.Tables["AddedClients"] != null)
                        foreach (DataRow oRow in dataSet.Tables["AddedClients"].Rows)
                        {
                            var added = new Tuple<int, string>(Convert.ToInt32(oRow["ClientId"]), oRow["Name"].ToString());
                            AddedCustomers.Add(added);
                        }
                    foreach (DataRow oRow in dataSet.Tables["Order"].Rows)
                    {
                        SentOrder order = new SentOrder();
                        order.ClientId = Convert.ToInt32(oRow["ClientId"], CultureInfo.InvariantCulture);
                        if (order.ClientId < 0)
                        {
                            var added = AddedCustomers.FirstOrDefault(x => x.Item1 == order.ClientId);
                            if (added != null)
                                order.ClientName = added.Item2;
                        }
                        else
                        {
                            var client = Client.Clients.FirstOrDefault(x => x.ClientId == order.ClientId);
                            if (client != null)
                                order.ClientName = client.ClientName;
                            else
                                order.ClientName = "Customer not found";
                        }
                        order.OrderType = (OrderType)Convert.ToInt32(oRow["OrderType"], CultureInfo.InvariantCulture);
                        order.OrderId = Convert.ToInt32(oRow["OrderId"], CultureInfo.InvariantCulture);
                        order.Comment = oRow["Comments"].ToString();
                        if (dataSet.Tables["Order"].Columns["ClientUniqueId"] != null)
                            order.ClientUniqueId = oRow["ClientUniqueId"].ToString();
                        order.Date = (DateTime)oRow["Date"];
                        order.OrderUniqueId = oRow["UniqueId"].ToString();
                        order.PrintedOrderId = oRow["PrintedOrderId"].ToString();
                        order.AsPresale = Convert.ToInt32(oRow["AsPresale"]) > 0;

                        order.TaxRate = (float)Convert.ToDouble(oRow["TaxRate"]);
                        order.ShipDate = (DateTime)oRow["Shipdate"];
                        order.DiscountType = (DiscountType)oRow["DiscountType"];
                        order.DiscountAmount = (float)Convert.ToDouble(oRow["DiscountAmount"]);

                        if (oRow["OtherCharges"] != null)
                            order.OtherCharges = Convert.ToDouble(oRow["OtherCharges"]);
                    
                        if (oRow["Freight"] != null)
                            order.Freight = Convert.ToDouble(oRow["Freight"]);
                    
                        if (oRow["OtherChargesType"] != null)
                            order.OtherChargesType = Convert.ToInt32(oRow["OtherChargesType"]);
    
                        if (oRow["FreightType"] != null)
                            order.FreightType = Convert.ToInt32(oRow["FreightType"]);

                        if (oRow["OtherChargesComment"] != null)
                            order.OtherChargesComment = oRow["OtherChargesComment"].ToString();
                        if (oRow["FreightComment"] != null)
                            order.FreightComment = oRow["FreightComment"].ToString();
                        
                        order.PackagePath = PackagePath;

                        order.Details = new List<SentOrderDetail>();

                        foreach (DataRow odRow in dataSet.Tables["OrderDetail"].Rows)
                        {
                            if (order.OrderId == (int)odRow["OrderId"])
                            {
                                SentOrderDetail detail = new SentOrderDetail();
                                detail.ProductId = Convert.ToInt32(odRow["ProductId"], CultureInfo.InvariantCulture);
                                detail.Qty = Convert.ToSingle(odRow["Qty"], CultureInfo.InvariantCulture);
                                detail.Price = Convert.ToDouble(odRow["Price"], CultureInfo.InvariantCulture);
                                detail.Comments = odRow["Comment"].ToString();

                                detail.Damaged = Convert.ToBoolean(odRow["Damaged"]);
                                detail.IsCredit = Convert.ToBoolean(odRow["IsCredit"]);

                                detail.Discount = (float)Convert.ToDouble(odRow["Discount"]);
                                detail.DiscountType = (DiscountType)odRow["DiscountType"];
                                detail.TaxRate = (float)Convert.ToDouble(odRow["TaxRate"]);
                                detail.Taxed = Convert.ToBoolean(odRow["Taxed"]);
                                detail.Weight = (float)Convert.ToDouble(odRow["Weight"]);

                                detail.ExtraFields = odRow["ExtraFields"].ToString();

                                var uomId = Convert.ToInt32(odRow["UnitOfMeasureId"]);
                                var UoM = UnitOfMeasure.List.Where(x => x.Id == uomId).FirstOrDefault();
                                detail.UoM = UoM;

                                order.Details.Add(detail);
                            }
                        }

                        order.TotalAmount = order.Details.Sum(x => x.Qty * x.Price);

                        Orders.Add(order);
                    }
                }
                catch(Exception ex) {  }
            }
            return Orders;
        }


        public static IList<SentOrderPackage> Packages(DateTime startDate)
        {
            var starTime = DateTime.Now;

            List<SentOrderPackage> list = new List<SentOrderPackage>();

            foreach (string file in Directory.GetFiles(Config.OrderPath))
            {
                if (file.IndexOf(".signature") > 0)
                    continue;
                SentOrderPackage package = new SentOrderPackage();
                package.CreatedDate = new FileInfo(file).CreationTime;

                if (package.CreatedDate < startDate)
                    continue;

                package.PackagePath = file;
                list.Add(package);
            }

            System.Diagnostics.Debug.WriteLine("TOOK " + (DateTime.Now - startDate).TotalSeconds + " METHOD => PACKAGES");

            return list;
        }
    }
}

