
using System;
using System.Collections.Generic;
using System.Linq;



namespace LaceupMigration
{

    public class OrderDetail 
    {
        public bool AlreadyAddedDiscountOffer { get; set; }
        public int DiscountOfferId { get; set; }

        public bool isMixAndMatchRelated { get; set; }

        static int lastOrderDetailId = 0;

        public bool FromOffer { get; set; }

        public int OfferDetFreeItem { get; set; }

        public bool FromOfferPrice { get; set; }

        public int FromOfferType { get; set; }

        public bool AlreadyAskedForOffers { get; set; }

        public bool IgnoreInOffers { get; set; }

        public double Price { get; set; }

        public double ExpectedPrice { get; set; }

        public string Comments { get; set; }

        public string ExtraComments { get; set; }

        public int OrderDetailId { get; set; }

        public Product Product { get; set; }

        public float Qty { get; set; }

        public bool Persisted { get; set; }

        public string Lot { get; set; }

        public bool Damaged { get; set; }

        public float Ordered { get; set; }

        public string OriginalId { get; set; }

        public bool Substracted { get; set; }

        public bool IsCredit { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; }

        public bool Deleted { get; set; }

        public float Weight { get; set; }

        public double DexPrice { get; set; }

        public int RelatedOrderDetail { get; set; }

        public string ExtraFields { get; set; }

        public bool Taxed { get; set; }

        public double TaxRate { get; set; }

        public double Discount { get; set; }

        public DiscountType DiscountType { get; set; }

        public UnitOfMeasure OriginalUoM { get; set; }

        public int PriceLevelSelected { get; set; }
        public int ReasonId { get; set; }
        public int OrderDiscountId { get; set; }
        public int OrderDiscountBreakId { get; set; }
        public double CostDiscount { get; set; }
        public double CostPrice { get; set; }
        public bool ModifiedManually { get; set; }

        public OrderDiscount OrderDiscount
        {
            get
            {
                return OrderDiscount.List.FirstOrDefault(x => x.Id == OrderDiscountId);
            }
        }

        public OrderDiscountBreak OrderDiscountBreak
        {
            get
            {
                return OrderDiscount.OrderDiscountBreaks.FirstOrDefault(x => x.Id == OrderDiscountBreakId);
            }
        }

        #region Consignment

        // If the consignment line was modified in any way
        public bool ConsignmentSet { get; set; }
        // When the consignment line is counted
        public bool ConsignmentCounted { get; set; }
        // when the consignment line is added OR the value is updated
        public bool ConsignmentUpdated { get; set; }

        public float ConsignmentNew { get; set; }

        public float ConsignmentOld { get; set; }

        public float ConsignmentCount { get; set; }

        public float ConsignmentPick
        {
            get
            {
                if (Order.OrderType != OrderType.Consignment)
                    return 0;
                if (!ConsignmentSet)
                    return 0;
                if (ConsignmentCounted)
                    if (ConsignmentUpdated)
                        return ConsignmentNew - ConsignmentCount;
                    else
                        return ConsignmentOld - ConsignmentCount;
                // here ONLY if ! counted
                if (ConsignmentUpdated)
                    return ConsignmentNew - ConsignmentOld;

                // here only if !counted AND !updated
                return 0;
            }
        }

        public double ConsignmentNewPrice { get; set; }

        public bool ConsignmentSalesItem { get; set; }

        public bool ConsignmentCreditItem { get; set; }

        public float ConsignmentPicked { get; set; }

        #endregion

        Order order;
        public Order Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
            }
        }

        public OrderDetail()
        {

        }

        public OrderDetail(Product product, float qty, Order order) : this(product, qty, 0, order)
        {
            Lot = string.Empty;
            ExtraFields = string.Empty;
            OrderDetailId = ++lastOrderDetailId;
            OriginalId = Guid.NewGuid().ToString();
            Substracted = true;
            Comments = string.Empty;
            ExtraComments = string.Empty;
        }

        public OrderDetail(Product product, float qty, int orderDetailId, Order order)
        {
            this.Product = product;
            this.Order = order;

            if (order == null)
            {
                TaxRate = product.TaxRate;
                Taxed = product.Taxable;
            }
            else
            if (order.Client.Taxable)
            {
                if (order.Client.TaxRate > 0)
                    TaxRate = order.Client.TaxRate;
                else
                    TaxRate = Product.TaxRate;
                Taxed = Product.Taxable;
            }
            else
                Taxed = false;

            this.Qty = qty;
            this.OrderDetailId = orderDetailId;
            this.Comments = String.Empty;
            this.ExtraComments = String.Empty;
            Lot = "";
            Substracted = true;

            if (lastOrderDetailId < orderDetailId)
                lastOrderDetailId = orderDetailId;

            ConsignmentCountedLots = new Dictionary<string, float>();
            ConsignmentPickedLots = new Dictionary<string, float>();

            int extra = 0;
            if (Ordered % 1 > 0)
                extra++;

            DeliveryQty = new int[(int)Ordered + extra];
        }


        public bool DeliveryScanningChecked { get; set; }

        public float LoadStarting { get; set; }

        public double Allowance { get; set; }

        public bool SkipDetailQty(Order o)
        {
            bool skipDetail = false;

            if (!Config.AddRelatedItemsInTotal && (o.OrderType == OrderType.Order || o.OrderType == OrderType.Credit) && !o.AsPresale)
            {
                //check if detail is a related item of other detail
                skipDetail = o.Details.Any(x => x.RelatedOrderDetail == OrderDetailId);

                //if is not a related item check for the custom field (bon Suisse qb order)
                if (!skipDetail)
                {
                    if (!string.IsNullOrEmpty(Product.ExtraPropertiesAsString))
                    {
                        foreach (var ep in Product.ExtraProperties)
                            if (ep.Item1.ToLowerInvariant() == "related item")
                            {
                                if (ep.Item2 == "1")
                                {
                                    skipDetail = true;
                                    break;
                                }
                            }
                    }
                }
            }

            return skipDetail;
        }

        //check this method every time a new property is added
        public OrderDetail GetOrderDetailCopy()
        {
            var detail = new OrderDetail(Product, Qty, Order);

            detail.FromOffer = FromOffer;
            detail.OfferDetFreeItem = OfferDetFreeItem;
            detail.FromOfferPrice = FromOfferPrice;
            detail.FromOfferType = FromOfferType;
            detail.Price = Price;
            detail.ExpectedPrice = ExpectedPrice;
            detail.Comments = Comments;
            detail.ExtraComments = ExtraComments;
            detail.Persisted = Persisted;
            detail.Lot = Lot;
            detail.Damaged = Damaged;
            detail.Ordered = Ordered;
            detail.Substracted = Substracted;
            detail.IsCredit = IsCredit;
            detail.UnitOfMeasure = UnitOfMeasure;
            detail.Deleted = Deleted;
            detail.Weight = Weight;
            detail.DexPrice = DexPrice;
            detail.RelatedOrderDetail = RelatedOrderDetail;
            detail.ExtraFields = ExtraFields;
            detail.Taxed = Taxed;
            detail.TaxRate = TaxRate;
            detail.Discount = Discount;
            detail.DiscountType = DiscountType;
            detail.OriginalUoM = OriginalUoM;
            detail.ReasonId = ReasonId;
            detail.PriceLevelSelected = PriceLevelSelected;
            detail.DeliveryScanningChecked = DeliveryScanningChecked;
            detail.Allowance = Allowance;
            detail.ParLevelDetail = ParLevelDetail;
            detail.Id = Id;
            detail.HiddenItem = HiddenItem;
            detail.AdjustmentItem = AdjustmentItem;
            detail.ProductDepartment = ProductDepartment;
            detail.IsFreeItem = IsFreeItem;
            detail.LoadingError = LoadingError;
            detail.OrderDiscountId = OrderDiscountId;
            detail.OrderDiscountBreakId = OrderDiscountBreakId;
            detail.ModifiedManually = ModifiedManually;

            return detail;
        }

        public double PriceAndTax
        {
            get
            {
                int factor = IsCredit ? -1 : 1;

                var price = Price;

                if (TaxRate > 0)
                {
                    var tax = price * TaxRate / 100;
                    price += tax;
                }

                return price * factor;
            }
        }

        public double TotalLine
        {
            get
            {
                var weight = Order.AsPresale ? Product.Weight * Qty : Weight;

                var qty = Product.SoldByWeight ? weight : Qty;
                int factor = IsCredit ? -1 : 1;

                var price = qty * Price;

                if (TaxRate > 0)
                {
                    var tax = price * TaxRate / 100;
                    price += tax;
                }

                return price * factor;
            }
        }

        public double QtyPrice
        {
            get
            {
                var weight = Order.AsPresale ? Product.Weight * Qty : Weight;

                var qty = Product.SoldByWeight ? weight : Qty;
                int factor = IsCredit ? -1 : 1;

                var price = qty * Price;

                return price * factor;
            }
        }

        public double TaxLine
        {
            get
            {
                var weight = Order.AsPresale ? Product.Weight * Qty : Weight;

                var qty = Product.SoldByWeight ? weight : Qty;

                var price = qty * Price;

                if (TaxRate > 0)
                {
                    var tax = price * TaxRate / 100;
                    return tax;
                }

                return 0;
            }
        }

        public Dictionary<string, float> ConsignmentCountedLots { get; set; }

        public Dictionary<string, float> ConsignmentPickedLots { get; set; }

        public string ConsignmentComment { get; set; }

        public static string GetConsLotsAsString(Dictionary<string, float> dic)
        {
            string result = "";

            for (int i = 0; i < dic.Count - 1; i++)
            {
                var item = dic.ElementAt(i);
                result += string.Format("{0}:{1},", item.Key, item.Value);
            }

            if (dic.Count > 0)
            {
                var last = dic.ElementAt(dic.Count - 1);
                result += string.Format("{0}:{1}", last.Key, last.Value);
            }

            return result;
        }

        public void LoadConsLots(string s, Dictionary<string, float> dic)
        {
            var lines = s.Split(',').ToList();

            foreach (var item in lines)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                var parts = item.Split(':').ToList();

                string key = parts[0];
                var value = Convert.ToSingle(parts[1]);

                dic.Add(key, value);
            }
        }

        public bool ParLevelDetail { get; set; }

        public void AdjustExtraFieldForConsignment()
        {
            string format1 = "{0}={1}|";
            string format2 = "{0}={1}";

            var s = string.Format(format1, "oldvalue", ConsignmentOld);
            s += string.Format(format1, "newvalue", ConsignmentNew);
            s += string.Format(format1, "count", ConsignmentCount);
            s += string.Format(format1, "sold", Qty);
            s += string.Format(format1, "picked", ConsignmentPicked);
            s += string.Format(format1, "return", 0);
            s += string.Format(format1, "damaged", 0);
            s += string.Format(format1, "set", "0");
            s += string.Format(format1, "counted", "0");
            s += string.Format(format1, "updated", "0");
            s += string.Format(format1, "oldprice", Price);
            s += string.Format(format1, "price", ConsignmentNewPrice);
            s += string.Format(format1, "salesitem", ConsignmentSalesItem ? "1" : "0");
            s += string.Format(format1, "credititem", ConsignmentCreditItem ? "1" : "0");
            s += string.Format(format2, "newpar", "0");

            ExtraFields = s;
        }

        public void AdjustExtraFieldForParLevel(ClientDailyParLevel par)
        {
            string format1 = "{0}={1}|";
            string format2 = "{0}={1}";

            var s = string.Format(format1, "oldvalue", par.Qty);
            s += string.Format(format1, "newvalue", par.NewQty);
            s += string.Format(format1, "count", par.Counted);
            s += string.Format(format1, "sold", 0);
            s += string.Format(format1, "picked", 0);
            s += string.Format(format1, "return", 0);
            s += string.Format(format1, "damaged", 0);
            s += string.Format(format1, "set", "0");
            s += string.Format(format1, "counted", "0");
            s += string.Format(format1, "updated", "0");
            s += string.Format(format1, "oldprice", Price);
            s += string.Format(format1, "price", Price);
            s += string.Format(format1, "salesitem", "0");
            s += string.Format(format1, "credititem", "0");
            s += string.Format(format1, "newpar", par.Qty == 0 ? "1" : "0");
            s += string.Format(format2, "parid", par.Id);

            ExtraFields = s;
        }

        public int[] DeliveryQty { get; set; }

        public int Id { get; set; }

        public string DeliveryQtyAsString()
        {
            string s = string.Empty;
            foreach (var item in DeliveryQty)
            {
                if (!string.IsNullOrEmpty(s))
                    s += ",";
                s += item;
            }

            return s;
        }

        public void LoadDeliveryQty(string s)
        {
            var result = new List<int>();

            if (!string.IsNullOrEmpty(s))
            {
                var parts = s.Split(',');

                foreach (var item in parts)
                    result.Add(int.Parse(item));
            }

            DeliveryQty = result.ToArray();
        }

        public bool HiddenItem { get; set; }

        public bool AdjustmentItem { get; set; }

        public string LabelUniqueId { get; set; }

        public string ProductDepartment { get; set; }

        public bool IsFreeItem { get; set; }

        private bool loadingError;
        public bool LoadingFromReship { get; set; }
        public bool LoadingError
        {
            get
            {
                if (order.Reshipped)
                    return false;

                var reason = Reason.Find(ReasonId);
                if (reason != null)
                    return reason.LoadingError;
                else
                    return loadingError;
            }
            set { loadingError = value; }
        }

        public Reason Reason
        {
            get
            {
                if (ReasonId > 0)
                {
                    var reason = Reason.Find(ReasonId);
                    if (reason != null)
                        return reason;
                    else
                        return null;
                }
                else
                    return null;
            }
        }

        public DateTime LotExpiration { get; set; }

        public bool CompletedFromScannerPallets
        {
            get
            {
                if (Ordered > 0)
                    return ScannedQty == Ordered;
                else
                    return ScannedQty == Qty;
            }
            set
            {

            }
        }

        public bool CompletedFromScanner { get; set; }

        public int ScannedQty { get; set; }

        public double ListPrice { get; set; }

        public bool WeightEntered { get; set; }

        public bool ReadyToFinalize
        {
            get
            {
                if (!order.AsPresale && (!Damaged || Config.RequireLotForDumps))
                {
                    if (Product.LotIsMandatory(order.AsPresale, Damaged) && string.IsNullOrEmpty(Lot))
                        return false;
                    if (Product.SoldByWeight && !WeightEntered)
                        return false;
                }
                return true;
            }
        }

        public OdLine CreateLineBasedOnOrderDetail()
        {
            OrderDetail orderDetail = this;

            var line = new OdLine();
            line.NewProduct = true;
            line.Product = orderDetail.Product;
            line.PreviousOrderedDate = string.Empty;
            line.PreviousOrderedPrice = 0;
            line.PreviousOrderedQty = 0;
            line.PreviousUnitOfMeasure = null;

            double price = 0;
            if (Offer.ProductHasSpecialPriceForClient(line.Product, order.Client, out price, orderDetail.UnitOfMeasure))
                line.IsPriceFromSpecial = orderDetail.Price == price;
            else
                line.IsPriceFromSpecial = false;

            price = orderDetail.Price;

            line.Qty = orderDetail.Qty;
            line.IsCredit = orderDetail.IsCredit;
            line.Damaged = orderDetail.Damaged;
            line.Price = price;
            line.Discount = orderDetail.Discount;
            line.DiscountType = orderDetail.DiscountType;
            line.ExpectedPrice = orderDetail.ExpectedPrice;
            line.History = new List<InvoiceDetail>();
            line.AvgSale = order.Client.Average(line.Product.ProductId);

            line.OrderDetail = orderDetail;
            return line;
        }

        public bool CalculateOfferDetail()
        {
            if (Config.Simone)
                return false;

            if (Config.DontCalculateOffersAfterPriceChanged && ModifiedManually)//need to find a way to mark when price was edited in price edit asdadasdadasd fml;
                return false;

            if (IsCredit && !Config.UseOffersInCredit)
                return false;

            bool update = false;

            //state wide crap allowance
            try
            {
                var o = Offer.GetOfferToCheckAllowance(Product, order.Client, Qty, UnitOfMeasure);
                if (o != null)
                {
                    if (!string.IsNullOrEmpty(o.ExtraFields))
                    {
                        var amount = DataAccess.GetSingleUDF("rebateamount", o.ExtraFields);
                        if (!string.IsNullOrEmpty(amount))
                        {
                            double x = 0;
                            Double.TryParse(amount, out x);
                            this.Allowance = Convert.ToDouble(x);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex.ToString());
            }

            var offer = Offer.GetOfferForProductQty(Product, order.Client, Qty, UnitOfMeasure);

            if (offer != null)
            {
                update = true;

                if (offer.Type == OfferType.Price)
                {
                    FromOffer = true;

                    bool edit_comment = string.IsNullOrEmpty(Comments) || Comments.Contains("Offer: ");

                    if (edit_comment)
                        Comments = "Offer: " + offer.ToString();

                    var price = offer.Price;

                    if (offer.UnitOfMeasureId != (UnitOfMeasure != null ? UnitOfMeasure.Id : 0))
                    {
                        var offerUoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offerUoM != null)
                        {
                            price /= offerUoM.Conversion;
                            if (UnitOfMeasure != null)
                                price *= UnitOfMeasure.Conversion;
                        }
                    }

                    Price = price;
                    ExpectedPrice = price;
                }
                else if (offer.Type == OfferType.QtyQty)
                {
                    var qty = Qty;
                    if (UnitOfMeasure != null)
                        qty *= UnitOfMeasure.Conversion;

                    var offerMin = offer.GetBaseMinQty();

                    var free = offer.GetBaseFreeQty();

                    int freeOffer = (int)(qty / offerMin);
                    var freeqty = freeOffer * free;

                    var newDet = order.Details.FirstOrDefault(x => x.OrderDetailId == OfferDetFreeItem);
                    if (newDet == null)
                    {
                        if (!Config.CanGoBelow0 && !IsCredit)
                        {
                            var inv = Product.GetInventory(order.AsPresale, Lot, true);
                            if (inv < freeqty)
                                freeqty = inv;
                        }

                        if (UnitOfMeasure != null)
                            freeqty /= UnitOfMeasure.Conversion;

                        if (freeqty > 0)
                        {
                            FromOffer = true;
                            FromOfferType = 1;

                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString() + " (Starting Qty)";

                            newDet = new OrderDetail(Product, freeqty, order)
                            {
                                Price = 0,
                                FromOffer = true,
                                FromOfferType = 1,
                                UnitOfMeasure = UnitOfMeasure,
                                ExpectedPrice = 0,
                                Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString(),
                                IsCredit = IsCredit,
                                Damaged = Damaged
                            };

                            OfferDetFreeItem = newDet.OrderDetailId;

                            order.AddDetail(newDet);

                            order.UpdateInventory(newDet, -1);
                        }
                    }
                    else
                    {
                        order.UpdateInventory(newDet, 1);

                        if (!Config.CanGoBelow0 && !IsCredit)
                        {
                            var inv = Product.GetInventory(order.AsPresale, Lot, true);
                            if (inv < freeqty)
                                freeqty = inv;
                        }

                        if (UnitOfMeasure != null)
                            freeqty /= UnitOfMeasure.Conversion;

                        if (freeqty > 0)
                        {
                            FromOffer = true;
                            FromOfferType = 1;

                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString() + " (Starting Qty)";

                            newDet.Qty = freeqty;
                            newDet.Price = 0;
                            newDet.FromOffer = true;
                            newDet.FromOfferType = 1;
                            newDet.UnitOfMeasure = UnitOfMeasure;
                            newDet.ExpectedPrice = 0;
                            newDet.IsCredit = IsCredit;
                            newDet.Damaged = Damaged;
                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString();

                            order.UpdateInventory(newDet, -1);
                        }
                        else
                            order.Details.Remove(newDet);
                    }

                }
                else if (offer.Type == OfferType.QtyPrice)
                {
                    FromOffer = true;
                    FromOfferType = 2;

                    bool edit_comment = string.IsNullOrEmpty(Comments) || Comments.Contains("Offer: ");

                    if (edit_comment)
                        Comments = "Offer: " + offer.ToString();

                    var price = offer.Price;

                    if (offer.UnitOfMeasureId != (UnitOfMeasure != null ? UnitOfMeasure.Id : 0))
                    {
                        var offerUoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offerUoM != null)
                        {
                            price /= offerUoM.Conversion;
                            if (UnitOfMeasure != null)
                                price *= UnitOfMeasure.Conversion;
                        }
                    }

                    Price = price;
                    ExpectedPrice = price;
                }
                else if (offer.Type == OfferType.QtyQtyPrice)
                {
                    var qtyBase = Qty;
                    if (UnitOfMeasure != null)
                        qtyBase *= UnitOfMeasure.Conversion;

                    var offerMin = offer.GetBaseMinQty();

                    var free = offer.GetBaseFreeQty();

                    int freeOffer = (int)(qtyBase / offerMin);
                    var freeqty = freeOffer * free;

                    var price = offer.Price;

                    if (offer.UnitOfMeasureId != (UnitOfMeasure != null ? UnitOfMeasure.Id : 0))
                    {
                        var offerUoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offerUoM != null)
                        {
                            price /= offerUoM.Conversion;
                            if (UnitOfMeasure != null)
                                price *= UnitOfMeasure.Conversion;
                        }
                    }

                    var newDet = order.Details.FirstOrDefault(x => x.OrderDetailId == OfferDetFreeItem);
                    if (newDet == null)
                    {
                        if (!Config.CanGoBelow0 && !IsCredit)
                        {
                            var inv = Product.GetInventory(order.AsPresale, Lot, true);
                            if (inv < freeqty)
                                freeqty = inv;
                        }

                        if (UnitOfMeasure != null)
                            freeqty /= UnitOfMeasure.Conversion;

                        if (freeqty > 0)
                        {
                            FromOffer = true;
                            FromOfferType = 4;

                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString() + " (Starting Qty)";

                            newDet = new OrderDetail(Product, freeqty, order)
                            {
                                Price = price,
                                FromOffer = true,
                                FromOfferType = 4,
                                UnitOfMeasure = UnitOfMeasure,
                                ExpectedPrice = price,
                                Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString(),
                                IsCredit = IsCredit,
                                Damaged = Damaged
                            };

                            OfferDetFreeItem = newDet.OrderDetailId;

                            order.AddDetail(newDet);

                            order.UpdateInventory(newDet, -1);
                        }
                    }
                    else
                    {
                        order.UpdateInventory(newDet, 1);

                        if (!Config.CanGoBelow0 && !IsCredit)
                        {
                            var inv = Product.GetInventory(order.AsPresale, Lot, true);
                            if (inv < freeqty)
                                freeqty = inv;
                        }

                        if (UnitOfMeasure != null)
                            freeqty /= UnitOfMeasure.Conversion;

                        if (freeqty > 0)
                        {
                            FromOffer = true;
                            FromOfferType = 4;

                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString() + " (Starting Qty)";

                            newDet.Qty = freeqty;
                            newDet.Price = price;
                            newDet.FromOffer = true;
                            newDet.FromOfferType = 1;
                            newDet.UnitOfMeasure = UnitOfMeasure;
                            newDet.ExpectedPrice = price;
                            newDet.IsCredit = IsCredit;
                            newDet.Damaged = Damaged;
                            Comments = !string.IsNullOrEmpty(Comments) ? Comments : "Offer: " + offer.ToString();

                            order.UpdateInventory(newDet, -1);
                        }
                        else
                            order.Details.Remove(newDet);
                    }
                }
            }
            else
            {
                if (FromOffer)
                    update = true;

                if (Comments.ToLower().Contains("offer") || Comments.ToLower().Contains("oferta"))
                {
                    Comments = string.Empty;
                }

                if (FromOffer)
                {
                    bool cameFromOffer1 = false;
                    Price = Product.GetPriceForProduct(Product, Order, out cameFromOffer1, false, false, UnitOfMeasure);
                }

                FromOffer = false;

                var related = order.Details.FirstOrDefault(x => x.OrderDetailId == OfferDetFreeItem);
                if (related != null)
                    order.DeleteDetail(related);
            }

            return update;
        }
        
        public static OrderDetail AddRelatedItem(OrderDetail detail, Order order)
        {
            OrderDetail relatedDetail = null;
            bool useRelated = order.OrderType != OrderType.Consignment;
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "USERELATED");
                if (vendor != null && (vendor.Item2.ToUpperInvariant() == "NO" || vendor.Item2.ToUpperInvariant() == "0"))
                {
                    useRelated = false;
                }
            }

            if (useRelated && detail.IsCredit && !Config.AddRelatedItemInCredit)
                useRelated = false;

            if (useRelated && detail.Product.ExtraPropertiesAsString.Length > 0)
            {
                foreach (var p in detail.Product.ExtraProperties)
                    // this product requires a related
                    if (p.Item1.ToLowerInvariant() == "relateditem")
                    {
                        // add the related
                        if (detail.RelatedOrderDetail == 0)
                        {
                            var parts = p.Item2.Split(",");

                            var list_of_product = new List<int>();

                            foreach (var part in parts)
                            {
                                int productId = 0;
                                Int32.TryParse(part, out productId);
                                if (productId > 0)
                                    list_of_product.Add(Convert.ToInt32(productId));
                            }

                            foreach (var product in list_of_product)
                            {
                                var relatedProduct = Product.Find(Convert.ToInt32(product), true);
                                if (relatedProduct == null)
                                {
                                    Logger.CreateLog("Related item of product " + Convert.ToInt32(product) + " was not found");

                                    DialogService._dialogService.ShowAlertAsync(
                                        "This product has a related product that was not found.");
                                    return null;
                                }
                                else
                                {
                                    relatedDetail = new OrderDetail(relatedProduct, detail.Qty, order);
                                    relatedDetail.Price = relatedProduct.PriceLevel0;
                                    relatedDetail.IsCredit = detail.IsCredit;
                                    relatedDetail.Damaged = detail.Damaged;
                                    relatedDetail.ExpectedPrice = relatedDetail.Price;
                                    relatedDetail.FromOffer = false;
                                    relatedDetail.Substracted = true;
                                    // see if they have UoM
                                    if (detail.UnitOfMeasure != null)
                                    {
                                        relatedDetail.UnitOfMeasure = detail.UnitOfMeasure;
                                        relatedDetail.Price = relatedProduct.PriceLevel0 * detail.UnitOfMeasure.Conversion;
                                    }

                                    var defaultLot = relatedProduct.ExtraProperties.FirstOrDefault(x => x.Item1 == "DEFAULT_LOT");
                                    if (defaultLot != null)
                                        relatedDetail.Lot = defaultLot.Item2;

                                    order.AddDetail(relatedDetail);
                                    detail.RelatedOrderDetail = relatedDetail.OrderDetailId;

                                    if (detail.RelatedOrderDetail > 0)
                                    {
                                        string extraField = string.Empty;
                                        var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);
                                        if (string.IsNullOrEmpty(values))
                                        {
                                            extraField = relatedDetail.OrderDetailId.ToString();
                                        }
                                        else
                                        {
                                            extraField = values + "," + relatedDetail.OrderDetailId;
                                        }
                                        detail.ExtraFields = DataAccess.SyncSingleUDF("ExtraRelatedItem", extraField, detail.ExtraFields);
                                    }
                                    else
                                        detail.RelatedOrderDetail = relatedDetail.OrderDetailId;
                                }
                            }
                        }
                        else
                        {
                            relatedDetail = order.Details.FirstOrDefault(x => x.OrderDetailId == detail.RelatedOrderDetail);
                            if (relatedDetail != null)
                                relatedDetail.Qty = detail.Qty;

                            if (!string.IsNullOrEmpty(detail.ExtraFields) && detail.ExtraFields.Contains("ExtraRelatedItem"))
                            {
                                var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                                var parts = values.Split(",");

                                foreach (var p1 in parts)
                                {
                                    var detilId = Convert.ToInt32(p1);
                                    var relatedDetail1 = order.Details.FirstOrDefault(x => x.OrderDetailId == detilId);
                                    if (relatedDetail1 != null)
                                        relatedDetail1.Qty = detail.Qty;
                                }
                            }
                        }
                    }
            }

            return relatedDetail;
        }

        public bool IsRelated => Order.Details.Any(x => GetRelatedItems(x, Order).Select(x => x.OrderDetailId).Contains(OrderDetailId));

        public static List<OrderDetail> GetRelatedItems(OrderDetail detail, Order order)
        {
            var related_list = new List<OrderDetail>();
            
            related_list = order.Details.Where(x => detail.RelatedOrderDetail == x.OrderDetailId).ToList();

            if (!string.IsNullOrEmpty(detail.ExtraFields) && detail.ExtraFields.Contains("ExtraRelatedItem"))
            {
                var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                var parts = values.Split(",");

                foreach (var p1 in parts)
                {
                    var detilId = Convert.ToInt32(p1);
                    var relatedDetail1 = order.Details.FirstOrDefault(x => x.OrderDetailId == detilId);
                   
                    if(relatedDetail1 != null && related_list.All(x => x.Id != detilId))
                        related_list.Add(relatedDetail1);
                }
            }

            return related_list;
        }
        public static void UpdateRelated(OrderDetail detail, Order order)
        {
            if(detail == null || order == null)
                return;
            
            var related_list = new List<OrderDetail>();

            related_list = GetRelatedItems(detail, order);
            
            if (detail.Qty == 0)
            {
                foreach (var related in related_list)
                    order.Details.Remove(related);
            }
            else
            {
                if(related_list.Count == 0)
                    AddRelatedItem(detail, order);
                else
                {
                    foreach (var related in related_list)
                        related.Qty = detail.Qty;
                }
            }

        }
    }
}
