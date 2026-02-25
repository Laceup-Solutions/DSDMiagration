using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Point = SixLabors.ImageSharp.Point;

namespace LaceupMigration
{
    public enum CellType { Header, Detail, Footer };
    public class SentOrder 
    {
        public TransactionType TransactionType
        {
            get
            {
                if (OrderType == OrderType.WorkOrder)
                    return TransactionType.WorkOrder;

                if (OrderType == OrderType.NoService)
                    return TransactionType.NoService;

                if (AsPresale)
                {
                    if (OrderType == OrderType.Credit)
                        return TransactionType.CreditOrder;
                    if (OrderType == OrderType.Return)
                        return TransactionType.ReturnOrder;
                    if (IsQuote)
                        return TransactionType.Quote;
                    return TransactionType.SalesOrder;
                }

                if (OrderType == OrderType.Credit)
                    return TransactionType.CreditInvoice;
                if (OrderType == OrderType.Return)
                    return TransactionType.ReturnInvoice;
                return TransactionType.SalesInvoice;
            }
        }

        public bool AsPresale { get; set; }
        public bool IsQuote { get; set; }

        public string OrderUniqueId { get; set; }
        public string PrintedOrderId { get; set; }
        public CellType CellType { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public OrderType OrderType { get; set; }

        public int OrderId { get; set; }

        public string Comment { get; set; }

        public string PackagePath { get; set; }

        public DateTime ShipDate { get; set; }

        public DateTime Date { get; set; }

        public string ClientUniqueId { get; set; }

        public float TaxRate { get; set; }
        public DiscountType DiscountType { get; set; }

        public float DiscountAmount { get; set; }

        public double TotalAmount { get; set; }
        
        public double OtherCharges { get; set; }
        public int OtherChargesType { get; set; }

        public string OtherChargesComment { get; set; }

        public double Freight { get; set; }
        public int FreightType { get; set; }
        public string FreightComment { get; set; }


        public static Order CreateTemporalOrderFromFile(string tmpFile, SentOrder sentOrder)
        {
            Order order = null;

            using (DataSet tempds = new DataSet() { Locale = CultureInfo.InvariantCulture })
            {
                tempds.ReadXml(tmpFile, XmlReadMode.ReadSchema);

                List<Client> clients = CreateClientsFromDataset(tempds);

                foreach (DataRow row in tempds.Tables["Order"].Rows)
                {
                    if (sentOrder.OrderId != (int)row["OrderID"])
                        continue;
                    order = Order.CreateEmptyOrder();

                    order.OrderId = Convert.ToInt32(row["OrderID"]);

                    order.ReasonId = Convert.ToInt32(row["ReasonId"]);
                    order.SignatureName = row["SignatureName"].ToString();
                    order.DiscountComment = row["DiscountComments"].ToString();
                    order.Comments = row["Comments"].ToString();
                    order.SalesmanId = Convert.ToInt32(row["VendorID"]);
                    order.Date = (DateTime)row["Date"];
                    order.OrderType = (OrderType)Convert.ToInt32(row["OrderType"]);
                    order.Latitude = Convert.ToDouble(row["Latitude"]);
                    order.Longitude = Convert.ToDouble(row["Longitude"]);
                    order.PrintedOrderId = row["PrintedOrderID"] as string ?? string.Empty;
                    order.UniqueId = row["UniqueId"].ToString();

                    var clientId = Convert.ToInt32(row["ClientID"]);
                    if (clientId < 0)
                        order.Client = clients.FirstOrDefault(x => x.ClientId == clientId);
                    if (order.Client == null)
                        order.Client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
                    if (order.Client == null)
                    {
                        // TODO: create a temp client
                        order.Client = null;
                        string text = string.Format("The client [{0}] was not found for this order: [{1}] ]", clientId, order.UniqueId);
                        Logger.CreateLog(text);
                    }

                    if (tempds.Tables["Order"].Columns["TaxRate"] != null)
                        try
                        {
                            order.TaxRate = Convert.ToDouble(row["TaxRate"]);
                        }
                        catch
                        {
                            order.TaxRate = 0;
                        }
                    else
                        order.TaxRate = 0;

                    if (tempds.Tables["Order"].Columns["DiscountType"] != null)
                    {
                        try
                        {
                            order.DiscountType = (DiscountType)Convert.ToInt32(row["DiscountType"]);
                            order.DiscountAmount = Convert.ToSingle(row["DiscountAmount"]);
                        }
                        catch
                        {
                            order.DiscountType = 0;
                            order.DiscountAmount = 0;
                        }
                    }
                    else
                    {
                        order.DiscountType = 0;
                        order.DiscountAmount = 0;
                    }

                    if (tempds.Tables["Order"].Columns["Voided"] != null)
                        order.Voided = Convert.ToInt32(row["Voided"]) > 0;
                    else
                        order.Voided = false;

                    if (tempds.Tables["Order"].Columns["PONumber"] != null)
                        order.PONumber = row["PONumber"] as string ?? string.Empty;
                    else
                        order.PONumber = string.Empty;

                    if (tempds.Tables["Order"].Columns["ShipDate"] != null)
                        order.ShipDate = (DateTime)row["ShipDate"];

                    if (tempds.Tables["Order"].Columns["EndDate"] != null)
                        order.EndDate = (DateTime)row["EndDate"];

                    if (tempds.Tables["Order"].Columns["BatchId"] != null)
                        order.BatchId = Convert.ToInt32(row["batchID"]);

                    if (tempds.Tables["Order"].Columns["Dexed"] != null)
                        order.Dexed = Convert.ToInt32(row["Dexed"]) > 0;
                    else
                        order.Dexed = false;

                    if (tempds.Tables["Order"].Columns["Finished"] != null)
                        order.Finished = Convert.ToBoolean(row["Finished"]);
                    else
                        order.Finished = false;

                    if (tempds.Tables["Order"].Columns["CompanyName"] != null)
                        order.CompanyName = row["CompanyName"].ToString();
                    else
                        order.CompanyName = string.Empty;

                    if (tempds.Tables["Order"].Columns["Reshipped"] != null)
                        order.Reshipped = Convert.ToBoolean(row["Reshipped"]);

                    if (tempds.Tables["Order"].Columns["ExtraFields"] != null)
                        order.ExtraFields = row["ExtraFields"].ToString();
                    else
                        order.ExtraFields = string.Empty;

                    if (tempds.Tables["Order"].Columns["DateLong"] != null)
                        order.Date = DateTime.FromBinary(Convert.ToInt64(row["DateLong"]));

                    if (tempds.Tables["Order"].Columns["ShipDateLong"] != null)
                        order.ShipDate = DateTime.FromBinary(Convert.ToInt64(row["ShipDateLong"]));

                    if (tempds.Tables["Order"].Columns["EndDateLong"] != null)
                        order.EndDate = DateTime.FromBinary(Convert.ToInt64(row["EndDateLong"]));

                    if (tempds.Tables["Order"].Columns["AsPresale"] != null)
                        order.AsPresale = Convert.ToBoolean(row["AsPresale"]);

                    if (tempds.Tables["Order"].Columns["SignatureName"] != null)
                        order.SignatureName = row["SignatureName"].ToString();
                    
                    if (tempds.Tables["Order"].Columns["OtherCharges"] != null)
                        order.OtherCharges = Convert.ToDouble(row["OtherCharges"]);
                    
                    if (tempds.Tables["Order"].Columns["Freight"] != null)
                        order.Freight = Convert.ToDouble(row["Freight"]);
                    
                    if (tempds.Tables["Order"].Columns["OtherChargesType"] != null)
                        order.OtherChargesType = Convert.ToInt32(row["OtherChargesType"]);
    
                    if (tempds.Tables["Order"].Columns["FreightType"] != null)
                        order.FreightType = Convert.ToInt32(row["FreightType"]);

                    if (tempds.Tables["Order"].Columns["OtherChargesComment"] != null)
                        order.OtherChargesComment = row["OtherChargesComment"].ToString();
                    if (tempds.Tables["Order"].Columns["FreightComment"] != null)
                        order.FreightComment = row["FreightComment"].ToString();

                    foreach (DataRow odrow in row.GetChildRows("OrderDetail_Order"))
                    {
                        int productId = Convert.ToInt32(odrow["ProductID"]);
                        var product = Product.Products.FirstOrDefault(x => x.ProductId == productId);

                        if (product == null)
                        {
                            string text = string.Format("An order makes reference to a productID that does not exists: Client={0} , salesman={1}, product= {2}", order.Client.ClientId, order.SalesmanId, productId);
                            Logger.CreateLog(text);
                            continue;
                        }

                        var detail = new OrderDetail(product, 0, order);
                        detail.OrderDetailId = Convert.ToInt32(odrow["OrderDetailID"]);
                        detail.Qty = Convert.ToSingle(odrow["Qty"]);
                        detail.Price = Convert.ToDouble(odrow["Price"]);
                        detail.ExpectedPrice = Convert.ToDouble(odrow["ExpectedPrice"]);
                        detail.Comments = odrow["Comment"].ToString();
                        detail.Lot = odrow["Lot"].ToString();
                        detail.FromOffer = Convert.ToBoolean(odrow["FromOffer"]);
                        detail.Damaged = Convert.ToBoolean(odrow["Damaged"]);
                        detail.OriginalId = odrow["OriginalId"].ToString();
                        detail.IsCredit = Convert.ToBoolean(odrow["IsCredit"]);

                        var uomId = Convert.ToInt32(odrow["UnitOfMeasureId"]);
                        detail.UnitOfMeasure = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                        detail.Weight = Convert.ToSingle(odrow["Weight"]);

                        detail.ConsignmentCount = Convert.ToSingle(odrow["ConsignmentCounted"]);
                        detail.ConsignmentOld = Convert.ToSingle(odrow["ConsignmentOld"]);
                        detail.ConsignmentNew = Convert.ToSingle(odrow["ConsignmentNew"]);
                        detail.ConsignmentNewPrice = Convert.ToDouble(odrow["ConsignmentNewPrice"]);
                        detail.ConsignmentSet = Convert.ToBoolean(odrow["ConsignmentSet"]);
                        detail.ConsignmentCounted = Convert.ToBoolean(odrow["ConsignmentCountedFlag"]);
                        detail.ConsignmentUpdated = Convert.ToBoolean(odrow["ConsignmentUpdated"]);
                        detail.ConsignmentSalesItem = Convert.ToBoolean(odrow["ConsignmentSalesItem"]);

                        detail.ExtraFields = Convert.ToString(odrow["ExtraFields"]);

                        detail.Allowance = Convert.ToDouble(odrow["Allowance"]);
                        detail.Taxed = Convert.ToBoolean(odrow["Taxed"]);

                        detail.TaxRate = Convert.ToDouble(odrow["TaxRate"]);
                        detail.Discount = Convert.ToDouble(odrow["Discount"]);
                        detail.DiscountType = (DiscountType)Convert.ToInt32(odrow["Discount"]);

                        order.AddDetail(detail);
                    }
                }
            }

            // see if it has signature
            var signatureFile = tmpFile + ".signature.zip";
            if (File.Exists(signatureFile))
            {
                var tempFile = Path.GetTempFileName();
                try
                {
                    // load the signature of this order
                    ZipMethods.UnzipFile(signatureFile, tempFile);

                    using (StreamReader reader = new StreamReader(tempFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith(order.UniqueId))
                            {
                                order.SignaturePoints = new List<Point>();
                                foreach (string point in line.Substring((order.UniqueId + "|").Length).Split(new char[] { ';' }))
                                {
                                    string[] components = point.Split(new char[] { ',' });
                                    order.SignaturePoints.Add(new Point()
                                    {
                                        X = Convert.ToInt32(components[0]),
                                        Y = Convert.ToInt32(components[1])
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                }
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            return order;
        }

        private static List<Client> CreateClientsFromDataset(DataSet tempds)
        {
            List<Client> clients = new List<Client>();
            if (tempds.Tables["AddedClients"] != null && tempds.Tables["AddedClients"].Rows.Count > 0)
                foreach (DataRow row in tempds.Tables["AddedClients"].Rows)
                {
                    var client = new Client();
                    client.ClientId = Convert.ToInt32(row["ClientId"]);
                    client.CreditLimit = Convert.ToDouble(row["CreditLimit"]);
                    client.Latitude = Convert.ToDouble(row["Latitude"]);
                    client.Longitude = Convert.ToDouble(row["Longitude"]);
                    client.TaxRate = Convert.ToDouble(row["TaxRate"]);
                    client.UniqueId = row["UniqueId"].ToString();
                    client.Comment = row["Comment"].ToString();
                    client.ContactName = row["ContactName"].ToString();
                    client.ContactPhone = row["ContactPhone"].ToString();
                    client.ClientName = row["Name"].ToString();
                    client.ShipToAddress = row["Address1"].ToString() + "|" + row["Address2"].ToString() + "|" + row["city"].ToString() + "|" + row["state"].ToString() + "|" + row["zip"].ToString();
                    client.ExtraPropertiesAsString = row["ExtraFields"].ToString();
                    client.NonvisibleExtraPropertiesAsString = row["NonVisibleExtraFields"].ToString();

                    if (row["PriceLevelId"] != null)
                        client.PriceLevel = Convert.ToInt32(row["PriceLevelId"]);
                    if (row["RetailPriceLevelId"] != null)
                        client.RetailPriceLevelId = Convert.ToInt32(row["RetailPriceLevelId"]);

                    clients.Add(client);
                }
            return clients;
        }

        protected static string TruncateString(string field, int max)
        {
            if (field.Length > max)
                return field.Substring(0, max);
            return field;
        }

        public List<SentOrderDetail> Details { get; set; }

        #region Calculate Cost

        double CalculateOneItemCost(SentOrderDetail od, bool includeDiscount = false)
        {
            bool useAllowance = false;


            double qty = od.Qty;

            if (od.GetProduct.SoldByWeight)
            {
                if (AsPresale)
                    qty *= od.GetProduct.Weight;
                else
                    qty = od.Weight;
            }

            int factor = od.IsCredit ? -1 : 1;

            var total = od.Price * factor * qty;

            if (includeDiscount)
            {
                double discount = od.Discount;

                if (discount > 0)
                {
                    if (od.DiscountType == DiscountType.Amount)
                    {
                        discount *= od.Qty;
                        if (od.IsCredit)
                            discount *= -1;
                    }
                    else if (od.DiscountType == DiscountType.Percent)
                        discount *= total;
                }

                total -= discount;
            }

            return double.Parse(Math.Round(total, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateOneItemTotalCost(SentOrderDetail od)
        {
            var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

            var itemCost = CalculateOneItemCost(od, true);

            if (od.Taxed)
                itemCost += itemCost * taxRate * (od.IsCredit ? -1 : 1);

            return itemCost;
        }

        public double CalculateItemCost()
        {
            double retvar = 0;

            foreach (SentOrderDetail od in Details)
            {
                var product = od.GetProduct;

                if (od.GetProduct.IsDiscountItem)
                    continue;

                retvar += CalculateOneItemCost(od);

            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateOneItemDiscount(SentOrderDetail od)
        {
            var total = CalculateOneItemCost(od);

            double discount = od.Discount;

            if (discount > 0)
            {
                if (od.DiscountType == DiscountType.Amount)
                {
                    discount *= od.Qty;
                    if (od.IsCredit)
                        discount *= -1;
                }
                else if (od.DiscountType == DiscountType.Percent)
                    discount *= total;
            }

            return double.Parse(Math.Round(discount, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double CalculateItemDiscount()
        {
            double retvar = 0;

            foreach (SentOrderDetail od in Details)
                retvar += CalculateOneItemDiscount(od);

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateDiscount()
        {

            double factor = 1;
            if (OrderType == OrderType.Credit)
                factor = -1;

            double orderDiscount = DiscountAmount;

            if (orderDiscount > 0)
            {
                if (DiscountType == DiscountType.Amount)
                    orderDiscount *= factor;
                else
                {
                    double retvar = CalculateItemCost(); //itemCost < 0 because it's a credit

                    orderDiscount = retvar * DiscountAmount;
                }
            }

            if (Config.AllowDiscountPerLine)
                orderDiscount += CalculateItemDiscount();

            return double.Parse(Math.Round(orderDiscount, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculatedFreight()
        {
            if (FreightType == (int)OrderFreightType.Amount)
            {
                return Freight;
            }
            else
            {
                double totalOrder = CalculateItemCost() - CalculateDiscount();
                double additionalCharges = (totalOrder * Freight) / 100;
                return additionalCharges;
            }

        }

        public double CalculatedOtherCharges()
        {
            if (OtherChargesType == (int)OrderFreightType.Amount)
            {
                return OtherCharges;
            }
            else
            {
                double totalOrder = CalculateItemCost() - CalculateDiscount();
                double additionalCharges = (totalOrder * OtherCharges) / 100;
                return additionalCharges;
            }

        }
        
        
        public double CalculateTax()
        {

            double retvar = 0;
            foreach (SentOrderDetail od in Details)
            {
                var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                if (od.Taxed)
                    retvar += CalculateOneItemCost(od, true) * taxRate;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double OrderTotalCost()
        {

            // calculate item cost
            double retvar = CalculateItemCost();
            // substract the discount
            retvar = retvar - CalculateDiscount();
            // add taxes
            retvar = retvar + CalculateTax() + CalculatedOtherCharges() + CalculatedFreight();
            // return the values

            return Math.Round(retvar, Config.Round);
        }

        public double DisolOrderTotalCost()
        {
            // calculate item cost
            double retvar = DisolCalculateItemCost();
            // substract the discount
            retvar = retvar - CalculateDiscount();
            // add taxes
            retvar = retvar + CalculateTax();
            // return the values
            return Math.Round(retvar, Config.Round);
        }


        public double DisolCalculateItemCost()
        {
            double retvar = 0;

            foreach (SentOrderDetail od in Details)
            {
                var typeEP = od.GetProduct.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToUpper() == "TYPE");
                if (typeEP != null && typeEP.Item2.ToUpper() == "VPR")
                    continue;

                bool useAllowance = false;


                double qty = od.Qty;

                if (od.GetProduct.SoldByWeight)
                {
                    if (AsPresale)
                        qty *= od.GetProduct.Weight;
                    else
                        qty = od.Weight;
                }

                int factor = od.IsCredit ? -1 : 1;

                var total = od.Price * factor * qty;


                retvar += double.Parse(Math.Round(total, Config.Round).ToCustomString(), NumberStyles.Currency);
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion
    }
}

