





using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrderDiscount
    {
        public enum ClientDiscountType
        {
            Draft = 0,
            Active = 1,
            Inactive = 2
        }

        public enum OrderDiscountApplyType
        {
            [Description("Items Discount")]
            ItemsDiscount = 1,
            [Description("Order Discount")]
            OrderDiscount = 2,
            [Description("Fixed Price Discount")]
            FixedPriceDiscount = 3
        }

        public enum DiscountStatus
        {
            Expired,
            Progressing,
            NoProgress
        }

        public enum CustomerDiscountType
        {
            [Description("Quantity")]
            Quantity = 1,
            [Description("Amount")]
            Amount = 2

        }

        public enum DetailDiscountType
        {
            [Description("Qty")]
            Qty = 1,
            [Description("Amount")]
            Amount = 2,
            [Description("Percent")]
            Percent = 3
        }

        public enum OrderDiscountCategoryType
        {
            [Description("Client")]
            Client = 1,
            [Description("Product")]
            Product = 2
        }

        public static bool HasDiscounts
        {
            get
            {
                return List.Count() > 0;
            }
        }

        public static bool IsOfferDiscount(OrderDetail line, Order order)
        {
            if (line.Product.IsDiscountItem)
                return true;

            var productsDiscount = order.Details.Where(x => x.Product.IsDiscountItem).Select(x => x.ExtraFields).ToList();
            var uniqueIdList = productsDiscount.Select(x => UDFHelper.GetSingleUDF("UniqueId", x)).ToList();
            return uniqueIdList.Contains(line?.OriginalId?.ToString());

        }

        public static bool ProductHasDiscount(Product produt, double qty, Order order, DateTime dateTime, UnitOfMeasure unit, bool isfreeItem)
        {
            if (order.IsWorkOrder)
                return false;

            if (isfreeItem)
                return false;

            double uomFactor = 1;
            var uoms = produt.UnitOfMeasures;

            if(uoms.Count > 0 && unit != null)
            {
                var defaultUom = uoms.FirstOrDefault(x => x.IsDefault);

                if(unit != defaultUom)
                {
                    if (unit.Conversion > defaultUom.Conversion)
                        uomFactor = defaultUom.Conversion;
                    else
                        uomFactor = unit.Conversion;
                }
            }

            var exclude = UDFHelper.GetSingleUDF("ExcludeDiscount", order.ExtraFields);

            qty *= uomFactor;

            DateTime _dateTime = (dateTime != null) ? (DateTime)dateTime : DateTime.Now;
            List<OrderDiscount> resultD = new List<OrderDiscount>();
            var listAreaClientId = AreaClient.List.Where(x => x.ClientId == order.Client.ClientId).Select(x => x.AreaId).ToList();
            var listProductId = order.Details.Select(x => x.Product.ProductId).ToList();

            var existDiscountClient = OrderDiscountClient.List.Where(x => x.ClientId == order.Client.ClientId).ToList();
            if (existDiscountClient.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountClient.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());

            var existDiscountClientArea = OrderDiscountClientArea.List.Where(x => listAreaClientId.Contains(x.AreaId)).ToList();

            if (existDiscountClientArea.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountClientArea.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());

            var listGroupClientId = ClientCategoryEx.List.Where(x => x.Id == order.Client.CategoryId).Select(x => x.Id).ToList();

            #region PL
            var existDiscountClientPriceLevel = OrderDiscountClientPriceLevel.List.Where(x => x.PriceLevelId == order.Client.PriceLevel && x.OrderDiscount != null
                                                                                             && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();
            resultD.AddRange(existDiscountClientPriceLevel.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());
            #endregion

            #region Discount Category Client
            var existDiscountGroup = OrderDiscountCategory.List.Where(x => x.CategoryType == (int)OrderDiscountCategoryType.Client
                                                                                && listGroupClientId.Contains(x.CategoryId)).ToList();

            if (existDiscountGroup.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountGroup.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());
            #endregion

            var existDiscountProduct = OrderDiscountProduct.List.Where(x => listProductId.Contains(x.ProductId)).ToList();
            if (existDiscountProduct.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountProduct.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());

            resultD = resultD.Distinct().ToList();

            foreach (var discount in resultD)
            {
                var orderDiscountVendors = discount.OrderDiscountVendors;
                var orderDiscountProducts = discount.OrderDiscountProducts;


                List<int> idVendor = orderDiscountVendors.Select(x => x.Id).ToList();

                var listProductBuy = (orderDiscountProducts.Count > 0)
                    ? orderDiscountProducts.Select(x => x.ProductId).ToList()
                    : orderDiscountVendors.Count > 0
                    ? Product.Products.Where(x => idVendor.Contains(x.VendorId)).Select(x => x.ProductId).ToList()
                    : Product.Products.Select(x => x.ProductId).ToList();


                var listProductGet = GetProductToBreak(discount);

                #region test
                double minQtyToBuy = 0;
                double getQty = 0;

                bool isValidOffer = false;

                var orderDC = discount.OrderDiscountClients.FirstOrDefault(x => x.ClientId == order.Client.ClientId);
                if (orderDC != null)
                {

                    /*Si tiene breack*/
                    if (discount.AppliedTo == 1)
                    {
                        var first = discount.OrderDiscountBreaks.OrderBy(x => x.MinQty).FirstOrDefault();
                        if (first != null)
                        {
                            minQtyToBuy = first.MinQty;
                            getQty = (first.QtySelectProduct ?? 0);
                        }
                    }
                    /**/
                    else
                    {
                        minQtyToBuy = orderDC.Buy;
                    }

                    isValidOffer = true;
                }

                var orderDA = discount.OrderDiscountClientAreas.FirstOrDefault(x => listAreaClientId.Contains(x.AreaId));
                if (orderDA != null)
                {

                    /*Si tiene breack*/
                    if (discount.AppliedTo == 1/*orderDiscount.OrderDiscountBreaks.Count > 0*/)
                    {
                        var first = discount.OrderDiscountBreaks.OrderBy(x => x.MinQty).FirstOrDefault();
                        if (first != null)
                        {
                            minQtyToBuy = first.MinQty;
                            getQty = (first.QtySelectProduct ?? 0);
                        }
                    }
                    else
                    {
                        minQtyToBuy = orderDA.Buy;
                    }

                    isValidOffer = true;
                }

                var orderDG = discount.OrderDiscountCategories.FirstOrDefault(x => x.CategoryType == (int)OrderDiscountCategoryType.Client && listGroupClientId.Contains(x.CategoryId));
                if (orderDG != null)
                {

                    /*Si tiene breack*/
                    if (discount.AppliedTo == 1/*orderDiscount.OrderDiscountBreaks.Count > 0*/)
                    {
                        var first = discount.OrderDiscountBreaks.OrderBy(x => x.MinQty).FirstOrDefault();
                        if (first != null)
                        {
                            getQty = (first.QtySelectProduct ?? 0);
                            minQtyToBuy = first.MinQty;
                        }
                    }
                    else
                    {
                        minQtyToBuy = orderDG.Buy;
                    }

                    isValidOffer = true;
                }


                #region Discount Price Level 
                var priceLevelId = order.Client.PriceLevel > 0 ? order.Client.PriceLevel : -1;
                var orderDPL = discount.OrderDisocuntClientPriceLevels.FirstOrDefault(x => x.PriceLevelId == priceLevelId);
                if (orderDPL != null)
                {

                    /*Si tiene breack*/
                    if (discount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                    {
                        var first = discount.OrderDiscountBreaks.OrderBy(x => x.MinQty).FirstOrDefault();
                        if (first != null)
                        {
                            var minQty = first.MinQty;

                            var otherBreaksSameMinQty = discount.OrderDiscountBreaks.Where(x => x.MinQty == minQty && x.Id != first.Id);

                            var ListIdProductInBreack = first.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();

                            var _listProductBuy = listProductBuy.Where(x => ListIdProductInBreack.Contains(x)).ToList();
                            if (!_listProductBuy.Any())
                                continue;

                            listProductBuy = _listProductBuy.Select(x => x).ToList();

                            getQty = (first.QtySelectProduct ?? 0);
                            minQtyToBuy = first.MinQty;

                            foreach(var o in otherBreaksSameMinQty)
                            {
                                var products = o.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                                foreach(var p in products)
                                {
                                    if (!listProductBuy.Contains(p))
                                        listProductBuy.Add(p);
                                }

                            }
                        }
                    }
                    else
                    {
                        minQtyToBuy = orderDPL.Buy;
                    }

                    isValidOffer = true;

                }
                #endregion

                #endregion

                var at_least_one_active = Product.Products.Any(x => listProductGet.Contains(x.ProductId) && x.CategoryId > 0);

                bool EmptyGetProducts = (getQty > 0 && !at_least_one_active);

                if (!isValidOffer)
                    return false;

                double new_qty = qty;

                var addOthersInOrder = order.Details.Where(x => x.Product.ProductId != produt.ProductId && !x.FromOffer && listProductBuy.Contains(x.Product.ProductId)).ToList();
                if(addOthersInOrder.Count > 0)
                {
                    foreach (var item in addOthersInOrder)
                        new_qty += item.Qty;
                }

                if (listProductBuy.Contains(produt.ProductId) && new_qty >= minQtyToBuy && !EmptyGetProducts)
                    return true;
            }

            return false;
        }

        private static  List<int> GetProductToBreak(OrderDiscount orderDiscount)
        {
            List<int> productGet = new List<int>();
            foreach (var itemB in orderDiscount.OrderDiscountBreaks)
            {
                if ((itemB.QtySelectProduct ?? 0) < double.Epsilon)
                    continue;
                var qtySelect = (itemB.QtySelectProduct ?? 0);

                if (itemB.OrderDiscountProductBreaks.Count > 0)
                {
                    productGet.AddRange(productGet.Union(itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList()).ToList());
                }
                else if (itemB.OrderDiscountVendorBreaks.Count > 0)
                {
                    List<int> idVendor = itemB.OrderDiscountVendorBreaks.Select(x => x.VendorId).ToList();
                    productGet.AddRange(productGet.Union(Product.Products.Where(x => idVendor.Contains(x.VendorId)).Select(x => x.ProductId).ToList()).ToList());
                }
                else if (itemB.OrderDiscountCategoryBreaks.Count > 0)
                {
                    List<int> idCategory = itemB.OrderDiscountCategoryBreaks.Select(x => x.CategoryId).ToList();
                    productGet.AddRange(productGet.Union(Product.Products.Where(x => idCategory.Contains(x.CategoryId)).Select(x => x.ProductId).ToList()).ToList());
                }
                else
                {
                    productGet.AddRange(Product.Products.Select(x => x.ProductId).ToList());
                    break;
                }


            }
            return productGet;

        }

        public static Func<OrderDiscount, DateTime, bool> ActiveDiscount = (orderDiscount, dateTime) =>
                   orderDiscount.Status == (int)ClientDiscountType.Active
                   && (orderDiscount.Permanent || (dateTime.Date >= orderDiscount.StartDate.Date
                   && dateTime.Date <= orderDiscount.EndDate.Date));


        public static List<OrderDiscount> List = new List<OrderDiscount>();
        public OrderDiscount()
        {
            this.OrderDiscountBreaks = new HashSet<OrderDiscountBreak>();
            this.OrderDiscountClients = new HashSet<OrderDiscountClient>();
            this.OrderDiscountClientAreas = new HashSet<OrderDiscountClientArea>();
            this.OrderDiscountProducts = new HashSet<OrderDiscountProduct>();
            this.OrderDiscountVendors = new HashSet<OrderDiscountVendor>();
            this.OrderDiscountTrackings = new HashSet<OrderDiscountTrackings>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderDiscountCategories = new HashSet<OrderDiscountCategory>();
            this.OrderDisocuntClientPriceLevels = new HashSet<OrderDiscountClientPriceLevel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int DiscountType { get; set; }
        public int AppliedTo { get; set; }
        public string Comments { get; set; }
        public string ExtraFields { get; set; }
        public int Status { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<int> ProductDiscountId { get; set; }
        public bool AutomaticApplied { get; set; }
        public bool Exclusive { get; set; }
        public bool Permanent { get; set; }

        public virtual ICollection<OrderDiscountBreak> OrderDiscountBreaks { get; set; }
        public virtual ICollection<OrderDiscountClient> OrderDiscountClients { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<OrderDiscountProduct> OrderDiscountProducts { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderDiscountTrackings> OrderDiscountTrackings { get; set; }
        public virtual ICollection<OrderDiscountVendor> OrderDiscountVendors { get; set; }
        public virtual ICollection<OrderDiscountClientArea> OrderDiscountClientAreas { get; set; }
        public virtual ICollection<OrderDiscountCategory> OrderDiscountCategories { get; set; }
        public virtual ICollection<OrderDiscountClientPriceLevel> OrderDisocuntClientPriceLevels { get; set; }

    }
}