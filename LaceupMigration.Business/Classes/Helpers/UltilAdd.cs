using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public class UtilAdd
    {
        private static OdLine Add(Order order, OdLine odLine, float qty, double price, UnitOfMeasure uom, string lot, string comment, bool freeItem, bool damaged, bool isVendor, bool isDelivery, List<OdLine> source, bool fromEdit = false)
        {
            float uomfactor = 1F;
            if (uom != null)
                uomfactor = uom.Conversion;

            #region Check Price And Free Item

            if (freeItem)
            {
                var freeDetail = order.Details.FirstOrDefault(x => x.Product.ProductId == odLine.Product.ProductId && x.Price == 0);
                if (freeDetail != null)
                {
                    if (odLine.OrderDetail == null || odLine.OrderDetail.OrderDetailId != freeDetail.OrderDetailId)
                    {
                        DialogHelper._dialogService.ShowAlertAsync("The selected product is already included as a free item, you cannot add it twice to the order.");
                        return null;
                    }
                }

                if (Config.FreeItemsNeedComments && string.IsNullOrEmpty(comment))
                {
                    DialogHelper._dialogService.ShowAlertAsync("A comment is mandatory for a free item.");
                    return null;
                }
            }

            bool cameFromOffer = false;
            double originalPrice = Product.GetPriceForProduct(odLine.Product, order, out cameFromOffer, odLine.IsCredit, damaged, uom);
            //double originalPrice1 = Product.GetPriceForProduct(odLine.Product, order, odLine.IsCredit, damaged) * uomfactor;

            if (!Config.AnyPriceIsAcceptable && !isVendor /* && Config.CanChangePrice(order, odLine.Product, odLine.IsCredit)*/)
            {
                if (!freeItem)
                {
                    if (!odLine.IsCredit)
                    {
                        if (price < odLine.Product.LowestAcceptablePrice * uomfactor && Math.Round(price, Config.Round) != Math.Round(originalPrice, Config.Round))
                        {
                            string append = Config.ShowLowestAcceptableOnWarning ? "\n" + "Lowest price:" + odLine.Product.LowestAcceptablePrice.ToCustomString() : string.Empty;
                            
                            DialogHelper._dialogService.ShowAlertAsync("The selected price is too low." + append);

                            return null;
                        }

                        if (Config.CheckIfCanIncreasePrice(order, odLine.Product) && Math.Round(price, Config.Round) < Math.Round(originalPrice, Config.Round))
                        {
                            DialogHelper._dialogService.ShowAlertAsync("The selected price is too low.");
                            return null;
                        }
                    }

                    if (price == 0 && Math.Round(price, Config.Round) != Math.Round(originalPrice, Config.Round))
                    {
                        DialogHelper._dialogService.ShowAlertAsync("The price cannot be 0.");
                        return null;
                    }
                }
            }

            //panamerican crap
            if (Math.Round(price, Config.Round) != Math.Round(originalPrice, Config.Round))
            {
                odLine.ExtraFields = UDFHelper.SyncSingleUDF("pricechanged", "yes", odLine.ExtraFields);

                if (!string.IsNullOrEmpty(odLine.Comments) && odLine.Comments.Contains("Offer:"))
                    comment = string.Empty;
            }
            else
                odLine.ExtraFields = UDFHelper.RemoveSingleUDF("pricechanged", odLine.ExtraFields);

            #endregion

            #region Packaging

            bool usePackaging = order.OrderType == OrderType.Order && !odLine.IsCredit;
            if (odLine.Product.ExtraProperties != null && usePackaging)
            {
                var pack = odLine.Product.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant().ToUpper() == "PACKAGING");
                if (pack != null)
                {
                    int package = 1;
                    try
                    {
                        package = Convert.ToInt32(pack.Item2);
                        if (qty % package != 0)
                        {
                            DialogHelper._dialogService.ShowAlertAsync("The quantity you input must be a multiple of " + package);
                            return null;
                        }
                    }
                    catch { package = 1; }
                }
            }

            #endregion

            #region From Offer

            //if (odLine.OrderDetail != null && odLine.OrderDetail.FromOffer)
            //{
            //    if (odLine.OrderDetail.FromOfferType != 0)
            //    {
            //        ActivityExtensionMethods.DisplayDialog(activity, activity.GetString(Resource.String.alert), activity.GetString(Resource.String.offerLineCannotBeEdited));
            //        return null;
            //    }
            //}

            #endregion

            #region Check Client Balance (ticket 4747)

            var cost = qty * price;

            double overCredit = order.Client.GetOverCreditLimit(odLine.OrderDetail, cost, odLine.IsCredit, order.AsPresale);

            if (overCredit > 0)
            {
                DialogHelper._dialogService.ShowAlertAsync(string.Format("This customer is over the credit limit by {{0}}.", overCredit.ToCustomString()));
                return null;
            }

            #endregion

            #region Lot

            // do the check of the lot
            if (odLine.Product.LotIsMandatory(order.AsPresale, damaged) && string.IsNullOrEmpty(lot))
            {
                DialogHelper._dialogService.ShowAlertAsync("The lot is required.");
                return null;
            }

            #endregion

            #region Checking Inventory

            if (Config.CheckInventoryInPreSale || (!order.AsPresale && Config.TrackInventory))
            {
                var detBaseQty = qty * uomfactor;

                if (odLine.OrderDetail != null && odLine.OrderDetail.Lot == lot)
                {
                    var baseqty = odLine.OrderDetail.Qty;

                    if (odLine.OrderDetail.UnitOfMeasure != null)
                        baseqty *= odLine.OrderDetail.UnitOfMeasure.Conversion;

                    detBaseQty -= baseqty;
                }

                var notCredit = !odLine.IsCredit;

                if (order != null && order.IsExchange)
                    notCredit = true;
                
                if (!Config.CanGoBelow0 && notCredit)
                {
                    float currentInv = odLine.Product.UseLot ? odLine.Product.GetInventory(order.AsPresale, lot) : odLine.Product.GetInventory(order.AsPresale);

                    if (source != null)
                        foreach (var added in source)
                        {
                            if (odLine.Product.UseLot && added.Lot != lot)
                                continue;

                            var addedqty = added.Qty;
                            if (added.UoM != null)
                                addedqty *= added.UoM.Conversion;

                            if (added.OrderDetail != null && added.OrderDetail.Lot == added.Lot)
                            {
                                var aq = added.OrderDetail.Qty;
                                if (added.OrderDetail.UnitOfMeasure != null)
                                    aq *= added.OrderDetail.UnitOfMeasure.Conversion;

                                addedqty -= aq;
                            }
                            
                            if(!fromEdit)
                                currentInv -= addedqty;
                        }

                    if (currentInv - detBaseQty < 0)
                    {
                        DialogHelper._dialogService.ShowAlertAsync("There is not enough inventory for this item.");
                        return null;
                    }
                }
            }

            #endregion

            #region weight

            double weight = 0;
            if (odLine.OrderDetail != null && odLine.Product.SoldByWeight)
            {
                weight = qty;
            }
            #endregion

            if (order.WillHaveMoreThanLimit(1))
            {
                DialogHelper._dialogService.ShowAlertAsync("This order exceeds that maximum amount of items. Modify your orden to be able to send it.");
                return null;
            }

            var orderLine = new OdLine()
            {
                Product = odLine.Product,
                Qty = qty,
                Price = price,
                Lot = lot,
                UoM = uom,
                FreeItem = freeItem,
                Comments = comment,
                IsCredit = odLine.IsCredit,
                Damaged = damaged,
                ExpectedPrice = odLine.ExpectedPrice,
                Offer = odLine.Offer,
                DiscountType = odLine.DiscountType,
                Discount = odLine.Discount,
                PreviousOrderedPrice = odLine.PreviousOrderedPrice,
                Weight = (float)weight,
                ExtraFields = odLine.ExtraFields,
                PriceLevelSelected = odLine.PriceLevelSelected,
                ManuallyChanged = odLine.ManuallyChanged
            };

            return orderLine;
        }

        public static OdLine Add(Order order, OdLine odLine, float qty, double price, UnitOfMeasure uom, string lot, DateTime lotExp, string comment, bool freeItem, bool damaged, bool isVendor, bool isDelivery, List<OdLine> source, bool fromEdit = false)
        {
            var orderLine = Add(order, odLine, qty, price, uom, lot, comment, freeItem, damaged, isVendor, isDelivery, source, fromEdit);

            if (orderLine != null)
                orderLine.LotExp = lotExp;

            return orderLine;
        }

        #region Consignment

        public static async Task<bool> AddNewLineInConsignment(Product prod, Order order, Action refreshActivity, Action<OrderDetail> setView)
        {
            if (Config.ParInConsignment)
                return await AddNewLineInParCons(prod, order, refreshActivity, setView);

            if (!Config.AddSalesInConsignment && !Config.AddCreditInConsignment)
                return CreateConsignmentLine(prod, order, refreshActivity, setView);

            var items = new List<string>();

            bool excludeCons = false;
            bool excludeSales = !Config.AddSalesInConsignment;
            bool excludeCredRet = !Config.AddCreditInConsignment;
            bool excludeCredDump = !Config.AddCreditInConsignment;

            foreach (var item in order.Details.Where(x => x.Product.ProductId == prod.ProductId))
            {
                if (item.ConsignmentSalesItem)
                    excludeSales = true;
                else if (item.ConsignmentCreditItem)
                {
                    if (item.Damaged)
                        excludeCredDump = true;
                    else
                        excludeCredRet = true;
                }
                else
                    excludeCons = true;
            }

            if (excludeCons && excludeSales && excludeCredRet && excludeCredDump)
                return false;

            if (!excludeCons)
                items.Add("Consignment Item");
            if (!excludeSales)
                items.Add("Sales Item");
            if (!excludeCredDump || !excludeCredRet)
                items.Add("Credit Item");

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Add Line As", null, "Cancel", items.ToArray());

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return false;

            var det = new OrderDetail(prod, 0, order);
            det.Order = order;
            det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
            det.Price = det.ExpectedPrice;

            if (selectedItem == "Credit Item")
                await CreateCreditLine(prod, order, det, refreshActivity, setView);
            else
            {
                if (selectedItem == "Consignment Item")
                {
                    det.ConsignmentNewPrice = det.Price;
                    det.ConsignmentSet = true;
                }
                else if (selectedItem == "Sales Item")
                {
                    det.ConsignmentSalesItem = true;
                    det.ConsignmentSet = true;
                    det.ConsignmentCounted = true;
                }

                order.Details.Add(det);

                order.Save();

                refreshActivity?.Invoke();
                setView?.Invoke(det);
            }

            return true;
        }

        private static bool CreateConsignmentLine(Product prod, Order order, Action refreshActivity, Action<OrderDetail> setView)
        {
            var det = new OrderDetail(prod, 0, order);
            det.Order = order;
            det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
            det.Price = det.ExpectedPrice;
            det.ConsignmentNewPrice = det.Price;
            det.ConsignmentSet = true;

            order.Details.Add(det);

            order.Save();

            refreshActivity?.Invoke();
            setView?.Invoke(det);

            return true;
        }

        static async Task CreateCreditLine(Product prod, Order order, OrderDetail det, Action refreshActivity, Action<OrderDetail> setView)
        {
            var cred = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ConsignmentCreditItem);
            if (cred != null)
            {
                det.ConsignmentCreditItem = true;
                det.ConsignmentSet = true;
                det.ConsignmentCounted = true;
                det.IsCredit = true;
                det.Damaged = !cred.Damaged;

                order.Details.Add(det);

                order.Save();

                refreshActivity?.Invoke();
                setView?.Invoke(det);
                return;
            }

            var items = new string[] { "Dump", "Return" };

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Type of Credit Item", "", "Cancel", items);

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return;

            det.ConsignmentCreditItem = true;
            det.ConsignmentSet = true;
            det.ConsignmentCounted = true;
            det.IsCredit = true;
            det.Damaged = selectedItem == "Dump";

            order.Details.Add(det);

            order.Save();

            refreshActivity?.Invoke();
            setView?.Invoke(det);
        }

        static async Task<bool> AddNewLineInParCons(Product prod, Order order, Action refreshActivity, Action<OrderDetail> setView)
        {
            var items = new string[] { "Consignment Item", "Dealer Own" };

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Add Line As", "", "Cancel", items);

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return false;

            OrderDetail det;

            if (selectedItem == "Consignment Item")
            {
                det = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && !x.ParLevelDetail);
                if (det == null)
                {
                    det = new OrderDetail(prod, 0, order);
                    det.Order = order;
                    det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
                    det.Price = det.ExpectedPrice;

                    det.ConsignmentNewPrice = det.Price;
                    det.ConsignmentSet = true;

                    det.AdjustExtraFieldForConsignment();

                    order.Details.Add(det);
                }
            }
            else
            {
                det = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ParLevelDetail);
                if (det == null)
                {
                    det = new OrderDetail(prod, 0, order);
                    det.Order = order;
                    det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
                    det.Price = det.ExpectedPrice;

                    det.ParLevelDetail = true;

                    det.AdjustExtraFieldForConsignment();

                    order.Details.Add(det);
                }
            }

            order.Save();

            refreshActivity?.Invoke();
            setView?.Invoke(det);

            return true;
        }



        public static async Task AddNewLineInConsignment(Product prod, Order order)
        {
            if (Config.ParInConsignment || Config.ConsignmentBeta)
            {
                await AddNewLineInParCons(prod, order);
                return;
            }

            if (!Config.AddSalesInConsignment && !Config.AddCreditInConsignment)
            {
                CreateConsignmentLine(prod, order);
                return;
            }

            var items = new List<string>();

            bool excludeCons = false;
            bool excludeSales = !Config.AddSalesInConsignment;
            bool excludeCredRet = !Config.AddCreditInConsignment;
            bool excludeCredDump = !Config.AddCreditInConsignment;
            int detId = 0;

            foreach (var item in order.Details.Where(x => x.Product.ProductId == prod.ProductId))
            {
                if (item.ConsignmentSalesItem)
                    excludeSales = true;
                else if (item.ConsignmentCreditItem)
                {
                    if (item.Damaged)
                        excludeCredDump = true;
                    else
                        excludeCredRet = true;
                }
                else
                    excludeCons = true;

                detId = item.OrderDetailId;
            }

            if (excludeCons && excludeSales && excludeCredRet && excludeCredDump)
            {
                // TODO: Implement MAUI navigation here if needed
                // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: detId
                return;
            }

            if (!excludeCons)
                items.Add("Consignment Item");
            if (!excludeSales)
                items.Add("Sales Item");
            if (!excludeCredDump || !excludeCredRet)
                items.Add("Credit Item");

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Add Line As", null, "Cancel", items.ToArray());

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return;

            var det = new OrderDetail(prod, 0, order);
            det.Order = order;
            det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
            det.Price = det.ExpectedPrice;

            if (selectedItem == "Credit Item")
                await CreateCreditLine(prod, order, det);
            else
            {
                if (selectedItem == "Consignment Item")
                {
                    det.ConsignmentNewPrice = det.Price;
                    det.ConsignmentSet = true;
                }
                else if (selectedItem == "Sales Item")
                {
                    det.ConsignmentSalesItem = true;
                    det.ConsignmentSet = true;
                    det.ConsignmentCounted = true;
                }

                order.Details.Add(det);

                order.Save();

                // TODO: Implement MAUI navigation here if needed
                // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: det.OrderDetailId
            }
        }

        private static void CreateConsignmentLine(Product prod, Order order)
        {
            var det = new OrderDetail(prod, 0, order);
            det.Order = order;
            det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
            det.Price = det.ExpectedPrice;
            det.ConsignmentNewPrice = det.Price;
            det.ConsignmentSet = true;

            order.Details.Add(det);

            order.Save();

            // TODO: Implement MAUI navigation here if needed
            // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: det.OrderDetailId
        }

        static async Task CreateCreditLine(Product prod, Order order, OrderDetail det)
        {
            var cred = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ConsignmentCreditItem);
            if (cred != null)
            {
                det.ConsignmentCreditItem = true;
                det.ConsignmentSet = true;
                det.ConsignmentCounted = true;
                det.IsCredit = true;
                det.Damaged = !cred.Damaged;

                order.Details.Add(det);

                order.Save();

                // TODO: Implement MAUI navigation here if needed
                // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: det.OrderDetailId
                return;
            }

            var items = new string[] { "Dump", "Return" };

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Type of Credit Item", null, "Cancel", items);

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return;

            det.ConsignmentCreditItem = true;
            det.ConsignmentSet = true;
            det.ConsignmentCounted = true;
            det.IsCredit = true;
            det.Damaged = selectedItem == "Dump";

            order.Details.Add(det);

            order.Save();

            // TODO: Implement MAUI navigation here if needed
            // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: det.OrderDetailId
        }

        static async Task AddNewLineInParCons(Product prod, Order order)
        {
            var items = new string[] { "Consignment Item", "Dealer Own" };

            var selectedItem = await DialogHelper._dialogService.ShowActionSheetAsync("Add Line As", null, "Cancel", items);

            if (selectedItem == "Cancel" || string.IsNullOrEmpty(selectedItem))
                return;

            OrderDetail det;

            if (selectedItem == "Consignment Item")
            {
                det = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && !x.ParLevelDetail);
                if (det == null)
                {
                    det = new OrderDetail(prod, 0, order);
                    det.Order = order;
                    det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
                    det.Price = det.ExpectedPrice;

                    det.ConsignmentNewPrice = det.Price;
                    det.ConsignmentSet = true;

                    det.AdjustExtraFieldForConsignment();

                    order.Details.Add(det);
                }
            }
            else
            {
                det = order.Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ParLevelDetail);
                if (det == null)
                {
                    det = new OrderDetail(prod, 0, order);
                    det.Order = order;
                    det.ExpectedPrice = Product.GetPriceForProduct(prod, order, false, false);
                    det.Price = det.ExpectedPrice;

                    det.ParLevelDetail = true;

                    det.AdjustExtraFieldForConsignment();

                    order.Details.Add(det);
                }
            }

            order.Save();

            // TODO: Implement MAUI navigation here if needed
            // Navigate with orderId: order.OrderId, lastProduct: prod.ProductId, lastDetail: det.OrderDetailId
        }

        #endregion
    }

    public class OdLine 
    {
        public OdLine()
        {
            UniqueId = Guid.NewGuid().ToString();

            Details = new List<OrderDetail>();
        }

        public OrderDetail OrderDetail { get; set; }

        public Product Product { get; set; }

        public float Qty { get; set; }

        public float Ordered { get; set; }
        public float Weight { get; set; }

        public double ExpectedPrice { get; set; }

        public double Price { get; set; }

        public string Comments { get; set; }

        public string Lot { get; set; }

        public bool Damaged { get; set; }

        public UnitOfMeasure UoM { get; set; }

        public bool FreeItem { get; set; }

        public Offer Offer { get; set; }

        public bool IsCredit { get; set; }

        public double AvgSale { get; set; }

        public bool NewProduct { get; set; }

        public string PreviousOrderedDate { get; set; }

        public double PreviousOrderedPrice { get; set; }
        public double PriceFromPreviousInvoice { get; set; }
        public UnitOfMeasure PreviousUnitOfMeasure { get; set; }

        public float PreviousOrderedQty { get; set; }

        public double PerWeek { get; set; }

        public bool IsPriceFromSpecial { get; set; }

        public double Allowance { get; set; }

        public bool IsHeader { get; set; }

        public int OrginalPosInList { get; set; }

        public int PositionInList { get; set; }

        public bool Deleted { get; set; }

        public DiscountType DiscountType { get; set; }

        public double Discount { get; set; }

        public InvoiceDetail LastInvoiceDetail { get; set; }

        public int DetailId { get; set; }

        public int ReasonId { get; set; }

        public List<InvoiceDetail> History { get; set; }

        public bool HistoryOpen { get; set; }

        public bool IsRelatedItem { get; set; }

        public string UniqueId { get; set; }

        public DateTime LotExp { get; set; }

        public List<OrderDetail> Details { get; set; }

        public string ExtraFields { get; set; }

        public bool NotCompleted { get; set; }

        public int PriceLevelSelected { get; set; }
        public bool UseLastSoldPrice { get; set; }
        public bool AdvancedCatalogOffer { get; set; }
        public bool ManuallyChanged { get; set; }
        
        public string OHColor { get; set; }
        public string OH { get; set; }

    }

    #region Product Catalog

    public class CatalogItem 
    {
        public Product Product { get; set; }

        public OdLine Line { get; set; }

        public List<OdLine> Values { get; set; }
    }

    #endregion

    #region Full Consignment

    public class ConsStruct 
    {
        public OrderDetail Detail { get; set; }

        public Product Product { get; set; }

        public bool FromPar { get; set; }

        public float OldValue { get; set; }

        public float NewValue { get; set; }

        public float Count { get; set; }

        public float Sold { get; set; }

        public float Picked { get; set; }

        public float Return { get; set; }

        public float Damaged { get; set; }

        public bool Set { get; set; }

        public bool Counted { get; set; }

        public bool Updated { get; set; }

        public bool SalesItem { get; set; }

        public bool CreditItem { get; set; }

        public double Price { get; set; }

        public double NewPrice { get; set; }

        public bool NewPar { get; set; }

        public static ConsStruct GetStructFromDetail(OrderDetail detail)
        {
            var x = new ConsStruct() { Detail = detail, Product = detail.Product, FromPar = detail.ParLevelDetail };

            var list = UDFHelper.ExplodeExtraProperties(detail.ExtraFields);
            foreach (var item in list)
            {
                switch (item.Key)
                {
                    case "oldvalue":
                        x.OldValue = Convert.ToSingle(item.Value);
                        break;
                    case "newvalue":
                        x.NewValue = Convert.ToSingle(item.Value);
                        break;
                    case "count":
                        x.Count = Convert.ToSingle(item.Value);
                        break;
                    case "sold":
                        x.Sold = Convert.ToSingle(item.Value);
                        break;
                    case "picked":
                        x.Picked = Convert.ToSingle(item.Value);
                        break;
                    case "return":
                        x.Return = Convert.ToSingle(item.Value);
                        break;
                    case "damaged":
                        x.Damaged = Convert.ToSingle(item.Value);
                        break;
                    case "set":
                        x.Set = Convert.ToInt32(item.Value) > 0;
                        break;
                    case "counted":
                        x.Counted = Convert.ToInt32(item.Value) > 0;
                        break;
                    case "updated":
                        x.Updated = Convert.ToInt32(item.Value) > 0;
                        break;
                    case "oldprice":
                        x.Price = Convert.ToDouble(item.Value);
                        break;
                    case "price":
                        x.NewPrice = Convert.ToDouble(item.Value);
                        break;
                    case "salesitem":
                        x.SalesItem = Convert.ToInt32(item.Value) > 0;
                        break;
                    case "credititem":
                        x.CreditItem = Convert.ToInt32(item.Value) > 0;
                        break;
                    case "newpar":
                        x.NewPar = Convert.ToInt32(item.Value) > 0;
                        break;
                    default:
                        break;
                }
            }

            return x;
        }
    }

    #endregion

    #region Full Template

    public class LineStruct 
    {
        public Product Product { get; set; }

        public float Previous { get; set; }

        public OdLine Damaged { get; set; }

        public OdLine Returns { get; set; }

        public OdLine Counted { get; set; }

        public OdLine Sold { get; set; }

        public bool IsRelatedLine { get; set; }

        public OrderHistory History { get; set; }

        public OdLine Ordered { get; set; }

        public bool CountRequired { get; set; }

        public bool DumpRequired { get; set; }

        public bool ReturnRequired { get; set; }

        public bool NextDeliveryRequired { get; set; }

        public bool InvoiceRequired { get; set; }
    }

    #endregion

    #region Grouped Template

    public abstract class TemplateLine 
    {
        public bool AsPresale { get; set; }
        public Product Product { get; set; }
        public bool IsCredit { get; set; }
        public bool Damaged { get; set; }

        public double Price { get; set; }
        public bool IsPriceFromSpecial { get; set; }
        public double ExpectedPrice { get; set; }
        public UnitOfMeasure UoM { get; set; }

        public double PerWeek { get; set; }
        public DateTime LastVisit { get; set; }
        public bool PreviouslyOrdered { get; set; }
        public float PreviouslyOrderedQty { get; set; }
        public double PreviouslyOrderedPrice { get; set; }
        public UnitOfMeasure PreviouslyOrderedUoM { get; set; }

        public abstract double OH { get; }
        public abstract float TotalQty { get; }
        public abstract float TotalWeight { get; }
        public abstract double Amount { get; }
        public abstract string TotalQtyString { get; }

        public abstract bool HasValue { get; }
        public abstract bool ReadyToFinalize { get; }
    }

    public class StandartTemplateLine : TemplateLine
    {
        public OrderDetail Detail { get; set; }

        public override double OH
        {
            get
            {
                var oh = AsPresale ? Product.CurrentWarehouseInventory : Product.CurrentInventory;

                if (UoM != null)
                    oh /= UoM.Conversion;

                return Math.Round(oh, Config.Round);
            }
        }

        public override float TotalQty
        {
            get
            {
                if (Detail != null)
                {
                    var qty = Detail.Qty;

                    if (Product.SoldByWeight)
                        qty = 1;

                    return qty;
                }

                return 0;
            }
        }

        public override float TotalWeight
        {
            get
            {
                if (Product.SoldByWeight && Detail != null)
                    return Detail.Qty;
                return 0;
            }
        }

        public override double Amount
        {
            get
            {
                if (Detail != null)
                {
                    double amount = Detail.Qty * Detail.Price;

                    var factor = IsCredit ? -1 : 1;

                    return amount * factor;
                }
                return 0;
            }
        }

        public override string TotalQtyString
        {
            get
            {
                return Detail != null ? TotalQty.ToString() : "+";
            }
        }

        public override bool HasValue { get { return Detail != null; } }

        public override bool ReadyToFinalize { get { return Detail != null && Detail.ReadyToFinalize; } }
    }

    public class GroupedTemplateLine : TemplateLine
    {
        public List<OrderDetail> Details { get; set; }

        public override double OH
        {
            get
            {
                double oh = AsPresale ? Product.CurrentWarehouseInventory : Product.CurrentInventory;

                if (UoM != null)
                    oh /= UoM.Conversion;

                return Math.Round(oh, Config.Round);
            }
        }

        public override float TotalQty
        {
            get
            {
                return Details.Sum(x => x.Qty);
            }
        }

        public override float TotalWeight
        {
            get
            {
                return Details.Sum(x => Product.SoldByWeight ? x.Weight : 0);
            }
        }

        public override double Amount
        {
            get
            {
                double amount = 0;
                foreach (var item in Details)
                {
                    var qty = Product.SoldByWeight ? item.Weight : item.Qty;
                    amount += qty * item.Price;
                }

                var factor = IsCredit ? -1 : 1;

                return amount * factor;
            }
        }

        public override string TotalQtyString
        {
            get
            {
                return Details.Count > 0 ? TotalQty.ToString() : "+";
            }
        }

        public override bool HasValue { get { return Details.Count > 0; } }

        public override bool ReadyToFinalize { get { return Details.Count > 0 && Details.All(x => x.ReadyToFinalize); } }

        public GroupedTemplateLine()
        {
            Details = new List<OrderDetail>();
        }

        public GroupedTemplateLine(Order order, Product product, bool isCredit, bool damaged)
        {
            Details = new List<OrderDetail>();

            LastTwoDetails detail = null;

            if (order.Client.OrderedList != null)
                detail = order.Client.OrderedList.FirstOrDefault(x => x.Last.Product.ProductId == product.ProductId);

            Product = product;
            AsPresale = order.AsPresale;

            if (detail != null && detail.Last.Quantity > 0)
            {
                PreviouslyOrdered = true;
                PreviouslyOrderedPrice = Math.Round(detail.Last.Price, Config.Round);
                PreviouslyOrderedUoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == detail.Last.UnitOfMeasureId);
            }

            double expectedPrice = Product.GetPriceForProduct(Product, order, isCredit, damaged);
            ExpectedPrice = expectedPrice;

            double price = 0;

            if (Offer.ProductHasSpecialPriceForClient(Product, order.Client, out price))
            {
                Price = price;
                IsPriceFromSpecial = true;
            }
            else
            {
                Price = expectedPrice;
                IsPriceFromSpecial = false;
            }

            if (!string.IsNullOrEmpty(Product.UoMFamily))
            {
                if (Config.UseLastUoM && detail.Last.UnitOfMeasureId > 0)
                {
                    if (PreviouslyOrderedUoM != null)
                    {
                        ExpectedPrice = ExpectedPrice * PreviouslyOrderedUoM.Conversion;
                        Price = Price * PreviouslyOrderedUoM.Conversion;
                        UoM = PreviouslyOrderedUoM;
                    }
                    else
                        Logger.CreateLog("bad data, last detail with uomid=" + detail.Last.UnitOfMeasureId + " and UoM not found");
                }
                else
                {
                    var defaultUoM = Product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    if (defaultUoM != null)
                    {
                        ExpectedPrice = ExpectedPrice * defaultUoM.Conversion;
                        Price = Price * defaultUoM.Conversion;
                        UoM = defaultUoM;
                    }
                    else
                        Logger.CreateLog("bad data, UOM without default");
                }
            }

            if (Config.HidePriceInTransaction)
            {
                Price = 0;
                ExpectedPrice = 0;
            }

            IsCredit = isCredit;
            Damaged = damaged;

        }
    }

    #endregion

}