using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public class InvoiceDetail 
    {
        public int InvoiceId { get; set; }

        public int InvoiceType { get; set; }

        public int ProductId { get; set; }

        Product detailProduct;
        public Product Product
        {
            get
            {
                if (detailProduct == null)
                    detailProduct = Product.Find(ProductId);
                if (detailProduct == null)
                    detailProduct = Product.CreateNotFoundProduct(ProductId);

                return detailProduct;
            }
        }

        public double Quantity { get; set; }

        public double Price { get; set; }

        public string Comments { get; set; }

        public int ClientId { get; set; }

        public DateTime Date { get; set; }

        public int UnitOfMeasureId { get; set; }

        public string ExtraFields { get; set; }

        public static List<InvoiceDetail> openInvoiceDetails = new List<InvoiceDetail>();

        public static void Clear(int count)
        {
            openInvoiceDetails = new List<InvoiceDetail>(count);
        }

        public static void Add(InvoiceDetail invoiceDetail)
        {
            openInvoiceDetails.Add(invoiceDetail);
        }

        public static IEnumerable<InvoiceDetail> GetInvoiceDetails(Invoice invoice)
        {
            List<InvoiceDetail> retList = new List<InvoiceDetail>();
            for (int i = 0; i < openInvoiceDetails.Count; i++)
            {
                var detail = openInvoiceDetails[i];
                if (detail.InvoiceId == invoice.InvoiceId)
                    retList.Add(detail);
            }

            return retList;
        }

        public static IList<InvoiceDetail> Details { get { return openInvoiceDetails; } }

        static public IList<InvoiceDetail> ClientProduct(int clientId, int productId)
        {
            List<InvoiceDetail> details = new List<InvoiceDetail>();

            for (int i = 0; i < openInvoiceDetails.Count; i++)
            {
                var detail = openInvoiceDetails[i];
                if (detail.ClientId == clientId && detail.ProductId == productId)
                    details.Add(detail);
            }

            return details;
        }

        public static IList<LastTwoDetails> ClientOrderedItemsEx(int clientId, bool forOrders = true)
        {
            return ClientOrderedItemsExDictionary(clientId, forOrders).Values.ToList();
        }

        public static Dictionary<int, LastTwoDetails> ClientOrderedItemsExDictionary(int clientId, bool forOrders = true, List<int> excludedProductsIds = null)
        {
            Dictionary<int, LastTwoDetails> currentList = new Dictionary<int, LastTwoDetails>();

            for (int i = 0; i < Details.Count; i++)
            {
                var detail = Details[i];

                if (detail.Quantity == 0 && detail.Price == 0)
                    continue;

                if (Config.DaysOfProjectionInTemplate > 0)
                {
                    var fromDate = DateTime.Now.AddDays(-(Config.DaysOfProjectionInTemplate));

                    if (detail.Date < fromDate)
                        continue;
                }

                // Use the details of only this client
                if (detail.ClientId == clientId)
                {
                    // 2408
                    if ((detail.InvoiceType != 0 || detail.Price < 0) && forOrders)
                        continue;

                    // for credits exclude the price positives
                    if ((detail.InvoiceType != 1 || detail.Price > 0) && !forOrders)
                        continue;

                    if (detail.Product == null)
                    {
                        //detail.Product = Product.Find(detail.ProductId);
                        //if (detail.Product == null)
                        continue;
                    }

                    if (excludedProductsIds != null && excludedProductsIds.Contains(detail.ProductId))
                        continue;

                    if (detail.Product.CategoryId == 0)
                        continue;
                    // Did I see this product before?
                    if (currentList.ContainsKey(detail.ProductId))
                    {
                        // this should not happen, but just in case :)
                        var item = currentList[detail.ProductId];
                        if (item.Last.Date.Date == detail.Date.Date)
                        {
                            item.TotalDetQty += detail.Quantity;
                            continue;
                        }

                        //naura code ticket 4101
                        if (item.First.Date.Date > detail.Date.Date)
                            item.First = detail;

                        // a new top date?
                        if (item.Last.Date.Date < detail.Date.Date)
                        {
                            // move old top date to the previous position
                            item.Previous = item.Last;
                            // set the new top date
                            item.Last = detail;
                        }
                        else
                        {
                            // if not previous, get this one
                            if (item.Previous == null)
                                item.Previous = detail;
                            else
                                // see if we have to update previous
                                if (item.Previous.Date.Date < detail.Date.Date)
                                item.Previous = detail;
                        }

                        item.TotalDetQty += detail.Quantity;
                    }
                    else
                        // if not , add a new entry to the list
                        currentList.Add(detail.ProductId, new LastTwoDetails() { Last = detail, First = detail, TotalDetQty = detail.Quantity });
                }
            }

            return currentList;
        }

        public static Dictionary<int, LastTwoDetails> ClientOrderedItemsExDictionary(List<int> excludedProductsIds = null)
        {
            Dictionary<int, LastTwoDetails> currentList = new Dictionary<int, LastTwoDetails>();

            for (int i = 0; i < Details.Count; i++)
            {
                var detail = Details[i];

                if (detail.Product == null)
                {
                    //detail.Product = Product.Find(detail.ProductId);
                    //if (detail.Product == null)
                    continue;
                }

                if (excludedProductsIds != null && excludedProductsIds.Contains(detail.ProductId))
                    continue;

                if (detail.Product.CategoryId == 0)
                    continue;
                // Did I see this product before?
                if (currentList.ContainsKey(detail.ProductId))
                {
                    // this should not happen, but just in case :)
                    var item = currentList[detail.ProductId];
                    if (item.Last.Date.Date == detail.Date.Date)
                    {
                        item.TotalDetQty += detail.Quantity;
                        continue;
                    }

                    //naura code ticket 4101
                    if (item.First.Date.Date > detail.Date.Date)
                        item.First = detail;

                    // a new top date?
                    if (item.Last.Date.Date < detail.Date.Date)
                    {
                        // move old top date to the previous position
                        item.Previous = item.Last;
                        // set the new top date
                        item.Last = detail;
                    }
                    else
                    {
                        // if not previous, get this one
                        if (item.Previous == null)
                            item.Previous = detail;
                        else
                            // see if we have to update previous
                            if (item.Previous.Date.Date < detail.Date.Date)
                            item.Previous = detail;
                    }

                    item.TotalDetQty += detail.Quantity;
                }
                else
                    // if not , add a new entry to the list
                    currentList.Add(detail.ProductId, new LastTwoDetails() { Last = detail, First = detail, TotalDetQty = detail.Quantity });
            }

            return currentList;
        }

        public static Dictionary<Product, List<InvoiceDetail>> GetProductHistoryDictionary(Client client, bool excludeProduct = false)
        {
            var result = new Dictionary<Product, List<InvoiceDetail>>();

            List<int> excludedProducts = new List<int>();

            if (excludeProduct)
            {
                var excludedExtraField = client.NonVisibleExtraProperties.FirstOrDefault(x => x != null && x.Item1.ToLowerInvariant() == "excludeditems");

                if (excludedExtraField != null)
                    foreach (var idAsString in excludedExtraField.Item2.Split(new char[] { ',' }).ToList())
                        excludedProducts.Add(Convert.ToInt32(idAsString));
            }

            foreach (var item in Details)
            {
                if (item.ClientId != client.ClientId)
                    continue;

                //if ((forSales && item.InvoiceType == 1) || (!forSales && item.InvoiceType == 0))
                //    continue;

                if (item.Quantity <= 0)
                    continue;

                if (excludedProducts.Contains(item.ProductId))
                    continue;

                if (item.Product == null)
                {
                    //item.Product = Product.Find(item.ProductId);
                    //if (item.Product == null)
                    continue;
                }

                if (item.Product.CategoryId == 0)
                    continue;

                if (result.Keys.FirstOrDefault(x => x.ProductId == item.ProductId) == null)
                    result.Add(item.Product, new List<InvoiceDetail>());

                result[item.Product].Add(item);
            }

            return result;
        }
    }

    //revisar aqui
    public class LastTwoDetails
    {
        public InvoiceDetail First { get; set; }
        public InvoiceDetail Last { get; set; }
        public InvoiceDetail Previous { get; set; }
        public double TotalDetQty { get; set; }

        public double PerWeek
        {
            get
            {
                if (First == null)
                    return 0;
                //double weeks = (Last.Date.Subtract(Previous.Date)).TotalDays / 7;
                //var weeks = Last.Date.Subtract(First.Date).TotalDays / 7;
                var weeks = (DateTime.Now.Date.Subtract(First.Date).TotalDays / 7);
                // note:
                // only way weeks is 0 is IF there are two sales both on the same days and those are the ONLY sales
                return weeks > 0 ? Math.Round(TotalDetQty / weeks, 2) : Math.Round(TotalDetQty, 2);
            }
        }
    }
}

