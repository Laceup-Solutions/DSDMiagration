using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SixLabors.ImageSharp;
using SkiaSharp;
using static LaceupMigration.OrderDiscount;

namespace LaceupMigration
{
    public enum TransactionType
    {
        All = 0,
        SalesInvoice = 1,
        CreditInvoice = 2,
        ReturnInvoice = 3,
        SalesOrder = 4,
        CreditOrder = 5,
        ReturnOrder = 6,
        Quote = 7,
        WorkOrder = 8,
        NoService = 9
    }
    public enum OrderFreightType
    {
        Amount = 0,
        Percent = 1
    }

    public enum OrderType
    {
        Order = 0,
        Credit = 1,
        Return = 2,
        NoService = 3,
        Bill = 4,
        Load = 5,
        Consignment = 6,
        Group = 7,
        Quote = 8,
        Sample = 9,
        WorkOrder = 32
    }

    public enum DiscountType
    {
        Percent = 0,
        Amount = 1
    }

    public class Order 
    {
        bool _oldOffers = !DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.94");

        private volatile object syslock = new object();

        bool deleted = false;
        string fileName;
        static protected int LastOrderId = 0;
        List<OrderDetail> details = new List<OrderDetail>();
        List<OrderDetail> deletedDetails = new List<OrderDetail>();

        public List<OrderDiscountTrackings> OrderDiscountTracking
        {
            get
            {
                return OrderDiscountTrackings.List.Where(x => x.OrderId == this.OrderId).ToList();
            }
        }

        #region Converted from Invoice
        public bool ConvertedInvoice { get; set; }
        public DateTime DueDate { get; set; }
        public string InvoiceSignature { get; set; }
        public int InvoiceSignatureWidth { get; set; }
        public int InvoiceSignatureSize { get; set; }
        public int InvoiceSignatureHeight { get; set; }
        #endregion

        public string CremiMexDepartment { get; set; }
        public int OrderId { get; set; }

        public int OriginalOrderId { get; set; }

        public int SalesmanId { get; set; }

        public int SiteId { get; set; }

        public int DepartmentId { get; set; }
        public DateTime Date { get; set; }

        private OrderType orderType;

        public OrderType OrderType
        {
            get
            {
                return orderType;
            }
            set
            {
                orderType = value;
                if (value != OrderType.Order && Config.DefaultItem > 0 && !Config.AddDefaultItemToCredit)
                {
                    var detail = Details.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                    if (detail != null)
                    {
                        Details.Remove(detail);
                        Save();
                    }
                }

                if (value == OrderType.Credit && Config.DefaultItem > 0 && Config.AddDefaultItemToCredit)
                {
                    var detail = Details.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                    if (detail != null)
                    {
                        detail.IsCredit = true;
                        Save();
                    }
                }
            }
        }

        public string Comments { get; set; }

        public string PrintedOrderId { get; set; }

        public Client Client { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public List<SixLabors.ImageSharp.Point> SignaturePoints { get; set; }

        public string UniqueId { get; set; }

        public string SignatureName { get; set; }

        public string SignatureUniqueId { get; set; }

        public double TaxRate { get; set; }

        public DiscountType DiscountType { get; set; }

        public float DiscountAmount { get; set; }

        public string DiscountComment { get; set; }

        public bool Voided { get; set; }

        public bool IsExchange { get; set; }

        public bool NeedToCalculate { get; set; }

        public string PONumber { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ShipDate { get; set; }

        public int BatchId { get; set; }

        public bool Dexed { get; set; }

        public bool Finished { get; set; }

        public string CompanyName { get; set; }

        public int PrintedCopies { get; set; }

        public bool AsPresale { get; set; }

        public bool Reshipped { get; set; }

        public DateTime ReshipDate { get; set; }

        public string ExtraFields { get; set; }

        public int OriginalSalesmanId { get; set; }

        public int ReasonId { get; set; }

        public int AssetId { get; set; }

        public bool Modified { get; set; }

        public bool QuoteModified { get; set; }


        #region FREIGHT and Other Charges

        public double OtherCharges { get; set; }
        public int OtherChargesType { get; set; }

        public string OtherChargesComment { get; set; }

        public double Freight { get; set; }
        public int FreightType { get; set; }
        public string FreightComment { get; set; }

        public bool IsCheckOrder { get; set; }

        public bool DetailsChanged { get; set; }

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
                
                return double.Parse(additionalCharges.ToCustomString(), NumberStyles.Currency);
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
                
                return double.Parse(additionalCharges.ToCustomString(), NumberStyles.Currency);
            }

        }

        #endregion

        #region Calculate Cost

        public double CalculateOneItemCost(OrderDetail od, bool includeDiscount = false)
        {
            bool useAllowance = false;

            if (Config.UseAllowanceForOrder(this))
                useAllowance = true;

            if (orderType == OrderType.Consignment && !od.ConsignmentCounted)
                return 0;

            double qty = od.Qty;

            if (od.Product.SoldByWeight)
            {
                if (AsPresale)
                {
                    if (od.IsCredit && Config.EnterWeightInCredits)
                        qty = od.Weight;
                    else
                        qty *= od.Product.Weight;
                }
                else
                    qty = od.Weight;
            }

            int factor = od.IsCredit ? -1 : 1;

            var total = od.Price * factor * qty;

            if (useAllowance)
                total -= od.Allowance * factor * od.Qty;

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

            // var rounded = total.ToCustomString();
            // var toReturn =  double.Parse(rounded, NumberStyles.Currency);
            return Math.Round(total, Config.Round, MidpointRounding.AwayFromZero);
        }

        public double GetTaxForOneProduct(Product product)
        {
            bool Taxed = false;
            double TaxRate = 0;

            var taxa = ProductTaxability.List.FirstOrDefault(x => x.ClientId == Client.ClientId && x.ProductId == product.ProductId);
            if (taxa != null)
            {
                Taxed = taxa.Taxed;
                TaxRate = taxa.TaxRate;
            }
            else
            {
                TaxRate = product.TaxRate;
                Taxed = product.Taxable;
            }

            return Taxed ? TaxRate : 0;
        }

        public double CalculateOneItemTotalCost(OrderDetail od)
        {
            var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

            var itemCost = CalculateOneItemCost(od, true);

            if (od.Taxed)
                itemCost += itemCost * taxRate * (od.IsCredit ? -1 : 1);

            return itemCost;
        }

        public double CalculateItemCost()
        {
            if (OrderType == OrderType.Consignment && (Config.ParInConsignment || Config.ConsignmentBeta))
                return CalculateConsParSubtotal();

            if (OrderType == OrderType.Consignment && Config.UseBattery)
                return CalculateBatteryItemCost();

            if (OrderType == OrderType.Consignment && Config.UseFullConsignment)
                return ConsignmentItemCost();

            double retvar = 0;

            if (!AsPresale && Details.Any(x => x.Product.SoldByWeight))
            {
                var groupedDetails = Details
                    .Where(od => !od.Product.IsDiscountItem)
                    .GroupBy(od => new { od.Product.ProductId, od.Price });

                foreach (var grouped in groupedDetails)
                {
                    var firstItem = grouped.First();

                    var copy = firstItem.GetOrderDetailCopy();
                    copy.Qty = grouped.Sum(x => x.Qty);
                    copy.Weight = grouped.Sum(x => x.Weight);
                    copy.Allowance = grouped.Sum(x => x.Allowance);
                    copy.Discount = grouped.Sum(x => x.Discount);
                    
                    retvar += CalculateOneItemCost(copy);
                }
            }
            else
            {
                foreach (OrderDetail od in Details)
                {
                    if (od.Product.IsDiscountItem)
                        continue;
            
                    retvar += CalculateOneItemCost(od);
                }
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateOneItemDiscount(OrderDetail od)
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

            return double.Parse(Math.Round(discount, Config.Round, MidpointRounding.AwayFromZero).ToCustomString(), NumberStyles.Currency);
        }

        double CalculateItemDiscount()
        {
            double retvar = 0;

            foreach (OrderDetail od in Details)
                retvar += CalculateOneItemDiscount(od);

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateDiscount()
        {
            if (OrderType == OrderType.Consignment && Config.UseBattery)
                return CalculateBatteryDiscount(CalculateBatteryItemCost());

            if (OrderType == OrderType.Consignment && Config.UseFullConsignment)
                return ConsignmentDiscount();

            double factor = 1;
            if (OrderType == OrderType.Credit)
                factor = -1;

            decimal orderDiscount = (decimal)DiscountAmount;

            if (orderDiscount > 0)
            {
                if (DiscountType == DiscountType.Amount)
                    orderDiscount *= (decimal)factor;
                else
                {
                    double retvar = CalculateItemCost(); //itemCost < 0 because it's a credit

                    orderDiscount = (decimal)retvar * (decimal)DiscountAmount;
                }
            }

            if (Config.AllowDiscountPerLine || IsDelivery)
                orderDiscount += (decimal)CalculateItemDiscount();

            orderDiscount = Math.Round(orderDiscount, Config.Round, MidpointRounding.AwayFromZero);
            return Convert.ToDouble(orderDiscount);
        }

        public double CalculateDiscount(OrderDetail excludedDetail)
        {
            if (OrderType == OrderType.Consignment && Config.UseBattery)
                return CalculateBatteryDiscount(CalculateBatteryItemCost());

            if (OrderType == OrderType.Consignment && Config.UseFullConsignment)
                return ConsignmentDiscount();

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
                orderDiscount += CalculateItemDiscount(excludedDetail);

            return double.Parse(Math.Round(orderDiscount, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double CalculateItemDiscount(OrderDetail excludeDetail)
        {
            double retvar = 0;

            foreach (OrderDetail od in Details)
            {
                if (od == excludeDetail)
                    continue;

                retvar += CalculateOneItemDiscount(od);
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }
        public double CalculateTax()
        {
            if (!Client.Taxable)
                return 0;

            if (OrderType == OrderType.Consignment && (Config.ParInConsignment || Config.ConsignmentBeta))
                return CalculateConsParTax();

            if (OrderType == OrderType.Consignment && Config.UseBattery)
                return CalculateBatteryTax();

            if (OrderType == OrderType.Consignment && Config.UseFullConsignment)
                return ConsignmentTax();

            double retvar = 0;

            if (!Config.CalculateTaxPerLine)
            {
                foreach (OrderDetail od in Details)
                {
                    var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                    if (od.Product.IsDiscountItem)
                        continue;

                    if (od.Taxed)
                    {
                        var val = CalculateOneItemCost(od, true) * taxRate;
                        if (Config.RoundTaxPerLine)
                            retvar += double.Parse(val.ToCustomString(), NumberStyles.Currency);
                        else
                            retvar += val;
                    }
                }
            }
            else
                retvar += CalculateTaxPerLine();

            if (!Config.ApplyDiscountAfterTaxes)
            {
                if (TaxRate > 0 && DiscountAmount > 0)
                {
                    double orderDiscount = DiscountAmount;

                    double factor = 1;
                    if (OrderType == OrderType.Credit)
                        factor = -1;

                    if (DiscountType == DiscountType.Amount)
                        orderDiscount *= factor;
                    else
                    {
                        double itemCost = CalculateItemCost(); //itemCost < 0 because it's a credit

                        orderDiscount = itemCost * DiscountAmount;
                    }

                    if (retvar > 0)
                        retvar -= orderDiscount * TaxRate;
                }
            }

            retvar = Math.Round(retvar, Config.Round);

            return double.Parse(retvar.ToCustomString(), NumberStyles.Currency);
        }

        private double CalculateTaxPerLine()
        {
            double tax = 0;

            foreach (var detail in Details)
            {
                if (detail.Product.IsDiscountItem)
                    continue;

                if (detail.Taxed)
                {
                    decimal LineTotal = decimal.Multiply((decimal)detail.Qty, (decimal)detail.Price);
                    decimal LineTax = decimal.Multiply(LineTotal, (decimal)TaxRate);

                    if (Config.RoundTaxPerLine)
                        tax += double.Parse(LineTax.ToCustomString(), NumberStyles.Currency);
                    else
                        tax += Math.Round(Convert.ToDouble(LineTax), 2);
                }
            }
            return tax;
        }

        public double OrderTotalCost()
        {
            if (OrderType == OrderType.Consignment && (Config.ParInConsignment || Config.ConsignmentBeta))
                return ConsParTotalCost();

            if (OrderType == OrderType.Consignment && Config.UseBattery)
                return BatteryConsTotalCost();

            if (OrderType == OrderType.Consignment && Config.UseFullConsignment)
                return ConsignmentTotalCost();

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

            foreach (OrderDetail od in Details)
            {
                var typeEP = od.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToUpper() == "TYPE");
                if (typeEP != null && typeEP.Item2.ToUpper() == "VPR")
                    continue;

                bool useAllowance = false;

                if (Config.UseAllowanceForOrder(this))
                    useAllowance = true;

                double qty = od.Qty;

                if (od.Product.SoldByWeight)
                {
                    if (AsPresale)
                        qty *= od.Product.Weight;
                    else
                        qty = od.Weight;
                }

                int factor = od.IsCredit ? -1 : 1;

                var total = od.Price * factor * qty;

                if (useAllowance)
                    total -= od.Allowance * factor * od.Qty;

                retvar += double.Parse(Math.Round(total, Config.Round).ToCustomString(), NumberStyles.Currency);
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion


        #region Full Consignment Total Cost

        double ConsignmentTotalCost()
        {
            // calculate item cost
            double retvar = ConsignmentItemCost();
            // substract the discount
            retvar = retvar - ConsignmentDiscount();
            // add taxes
            retvar = retvar + ConsignmentTax();
            // return the values
            return Math.Round(retvar, Config.Round);
        }

        double ConsignmentItemCost()
        {
            double retvar = 0;
            foreach (OrderDetail od in Details)
            {
                int factor = od.IsCredit ? -1 : 1;

                double qty = od.Qty;

                var total = double.Parse(Math.Round(od.Price * qty * factor, Config.Round).ToCustomString(), NumberStyles.Currency);

                retvar += total;
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double ConsignmentDiscount()
        {
            double factor = 1;
            if (OrderType == OrderType.Credit)
                factor = -1;
            if (DiscountAmount == 0)
                return 0;
            if (DiscountType == DiscountType.Amount)
                return DiscountAmount * factor;

            double retvar = ConsignmentItemCost(); //itemCost < 0 because it's a credit
            return double.Parse(Math.Round(retvar * DiscountAmount, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double ConsignmentTax()
        {
            double retvar = 0;
            foreach (OrderDetail od in Details)
            {
                var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                double qty = od.Qty;

                if (od.Taxed)
                    retvar += od.Price * qty * taxRate;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion


        #region Battery Calculate Totals

        public double BatteryConsTotalCost()
        {
            var retvar = CalculateBatteryItemCost();

            retvar -= CalculateBatteryDiscount(retvar);

            retvar += CalculateBatteryTax();

            return Math.Round(retvar, Config.Round);
        }

        public double CalculateBatteryItemCost()
        {
            double retvar = 0;

            var item = Client.ExtraProperties != null ? Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            foreach (OrderDetail od in Details)
            {
                if (!od.ConsignmentCounted)
                    continue;

                int factor = od.ConsignmentCreditItem ? -1 : 1;

                retvar += double.Parse(Math.Round(od.Price * od.Qty * factor, Config.Round).ToCustomString(), NumberStyles.Currency);

                if (useRelated)
                {
                    var related = GetRelatedProduct(od.Product);

                    if (related != null)
                    {
                        var relatedPrice = Product.GetPriceForProduct(related, this, false, false);

                        retvar += double.Parse(Math.Round(relatedPrice * od.Qty * factor, Config.Round).ToCustomString(), NumberStyles.Currency);
                    }
                }

                retvar += GetRelatedCoreCost(od);

                retvar += GetRelatedAdjCost(od);

                if (Config.ChargeBatteryRotation)
                    retvar += GetRelatedRotCost(od);
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        Product GetRelatedProduct(Product product)
        {
            int relatedId = 0;

            foreach (var p in product.ExtraProperties)
            {
                if (p.Item1.ToLower() == "relateditem")
                {
                    relatedId = Convert.ToInt32(p.Item2);
                    break;
                }
            }

            return Product.Find(relatedId, true);
        }

        #region Core

        double CalculateBatteryCoreCost(OrderDetail detail)
        {
            bool chargeCore = true;

            if (Client.NonVisibleExtraProperties != null)
            {
                var xC = Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "corepaid");
                if (xC != null && xC.Item2.ToLowerInvariant() == "n")
                    chargeCore = false;
            }

            if (!chargeCore)
                return 0;

            var core = DataAccess.GetSingleUDF("coreQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(core))
                return 0;

            var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

            if (coreId == null)
                return 0;

            var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

            if (relatedCore == null)
                return 0;

            var coreQty = Convert.ToDouble(core);

            float qty = 0;

            if (Config.CoreAsCredit)
                qty = (float)-coreQty;
            else
                qty = detail.Qty - (float)coreQty;

            if (qty == 0)
                return 0;

            var corePrice = Product.GetPriceForProduct(relatedCore, this, false, false);

            return corePrice * qty;
        }

        double GetRelatedCoreCost(OrderDetail detail)
        {
            return double.Parse(Math.Round(CalculateBatteryCoreCost(detail), Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double GetRelatedCoreTax(OrderDetail detail)
        {
            var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

            if (coreId == null)
                return 0;

            var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

            if (relatedCore == null || !relatedCore.Taxable)
                return 0;

            var coreCost = CalculateBatteryCoreCost(detail);

            var taxRate = TaxRate > 0 ? TaxRate : detail.TaxRate;

            return double.Parse(Math.Round(coreCost * taxRate, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion

        #region Adjustment

        double CalculateBatteryAdjCost(OrderDetail detail)
        {
            double totalCost = 0;

            var adjQty = DataAccess.GetSingleUDF("adjustedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(adjQty))
                return 0;

            var adjId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");

            if (adjId == null)
                return 0;

            var adjustment = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == adjId.Item2);

            if (adjustment == null)
                return 0;

            var adjPrice = Product.GetPriceForProduct(adjustment, this, false, false);

            int time = 0;
            if (Config.WarrantyPerClient)
            {
                time = GetIntWarrantyPerClient(detail.Product);
                if (time == 0)
                    return 0;
            }
            else
            {
                var timeSt = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "time");

                if (timeSt == null)
                    return 0;

                time = int.Parse(timeSt.Item2);
            }

            var ws = adjQty.Split(',');

            foreach (var item in ws)
            {
                var qty = int.Parse(item) - time;

                if (qty > 0)
                    totalCost += adjPrice * int.Parse(item);
            }

            return totalCost;
        }

        double GetRelatedAdjCost(OrderDetail detail)
        {
            return double.Parse(Math.Round(CalculateBatteryAdjCost(detail), Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double GetRelatedAdjTax(OrderDetail detail)
        {
            var adjId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");

            if (adjId == null)
                return 0;

            var adjustment = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == adjId.Item2);

            if (adjustment == null || !adjustment.Taxable)
                return 0;

            var taxRate = TaxRate > 0 ? TaxRate : detail.TaxRate;

            return double.Parse(Math.Round(CalculateBatteryAdjCost(detail) * taxRate, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion

        #region Rotation

        double CalculateBatteryRotationCost(OrderDetail detail)
        {
            var rotQty = DataAccess.GetSingleUDF("rotatedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(rotQty))
                return 0;

            var rotId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");

            if (rotId == null)
                return 0;

            var rotated = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == rotId.Item2);

            if (rotated == null)
                return 0;

            var rotPrice = Product.GetPriceForProduct(rotated, this, false, false);

            var qty = int.Parse(rotQty);

            return rotPrice * qty;
        }

        double GetRelatedRotCost(OrderDetail detail)
        {
            return double.Parse(Math.Round(CalculateBatteryRotationCost(detail), Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double GetRelatedRotTax(OrderDetail detail)
        {
            var rotId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");

            if (rotId == null)
                return 0;

            var rotated = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == rotId.Item2);

            if (rotated == null || !rotated.Taxable)
                return 0;

            var taxRate = TaxRate > 0 ? TaxRate : detail.TaxRate;

            return double.Parse(Math.Round(CalculateBatteryRotationCost(detail) * taxRate, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion

        public double CalculateBatteryDiscount(double itemCost)
        {
            double factor = 1;

            if (DiscountAmount == 0)
                return 0;
            if (DiscountType == DiscountType.Amount)
                return DiscountAmount * factor;

            var total = itemCost * DiscountAmount * factor;

            return double.Parse(Math.Round(total, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateBatteryTax()
        {
            double retvar = 0;

            var item = Client.ExtraProperties != null ? Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            foreach (OrderDetail od in Details)
            {
                if (!od.ConsignmentCounted)
                    continue;

                var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                if (od.Taxed)
                    retvar += od.Price * od.Qty * taxRate;

                if (useRelated)
                {
                    var related = GetRelatedProduct(od.Product);

                    if (related != null && related.Taxable)
                    {
                        var retaledTaxRate = TaxRate > 0 ? TaxRate : related.TaxRate;

                        var relatedPrice = Product.GetPriceForProduct(related, this, false, false);

                        retvar += double.Parse(Math.Round(relatedPrice * od.Qty * retaledTaxRate, Config.Round).ToCustomString(), NumberStyles.Currency);
                    }
                }

                retvar += GetRelatedCoreTax(od);

                retvar += GetRelatedAdjTax(od);

                if (Config.ChargeBatteryRotation)
                    retvar += GetRelatedRotTax(od);
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion


        #region Consignment Par Calculate Totals

        double ConsParTotalCost()
        {
            List<ConsStruct> details = new List<ConsStruct>();
            foreach (var det in Details)
                details.Add(ConsStruct.GetStructFromDetail(det));

            var retvar = CalculateConsParTotalItemCost(details);

            retvar -= CalculateBatteryDiscount(retvar);

            retvar += CalculateConsParTax(details);

            return Math.Round(retvar, Config.Round);
        }

        double CalculateConsParSubtotal()
        {
            List<ConsStruct> details = new List<ConsStruct>();
            foreach (var det in Details)
                details.Add(ConsStruct.GetStructFromDetail(det));

            return CalculateConsParTotalItemCost(details);
        }

        double CalculateConsParItemCost(ConsStruct od)
        {
            double retvar = 0;

            var item = Client.ExtraProperties != null ? Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            retvar += double.Parse(Math.Round(od.Price * (od.Sold - od.Damaged - od.Return), Config.Round).ToCustomString(), NumberStyles.Currency);

            if (useRelated)
            {
                var related = GetRelatedProduct(od.Product);

                if (related != null)
                {
                    var relatedPrice = Product.GetPriceForProduct(related, this, false, false);

                    retvar += double.Parse(Math.Round(relatedPrice * (od.Sold - od.Damaged - od.Return), Config.Round).ToCustomString(), NumberStyles.Currency);
                }
            }

            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double CalculateConsParTotalItemCost(List<ConsStruct> details)
        {
            double retvar = 0;

            foreach (var od in details)
            {
                retvar += CalculateConsParItemCost(od);

                var qty = od.Detail.Qty;

                od.Detail.Qty = od.Sold;

                retvar += GetRelatedCoreCost(od.Detail);

                retvar += GetRelatedAdjCost(od.Detail);

                if (Config.ChargeBatteryRotation)
                    retvar += GetRelatedRotCost(od.Detail);

                od.Detail.Qty = qty;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        double CalculateConsParTax()
        {
            List<ConsStruct> details = new List<ConsStruct>();
            foreach (var det in Details)
                details.Add(ConsStruct.GetStructFromDetail(det));

            return CalculateConsParTax(details);
        }

        double CalculateConsParTax(List<ConsStruct> details)
        {
            double retvar = 0;

            var item = Client.ExtraProperties != null ? Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            foreach (var od in details)
            {
                var taxRate = TaxRate > 0 ? TaxRate : od.Detail.TaxRate;

                if (od.Detail.Taxed)
                    retvar += double.Parse(Math.Round(od.Price * (od.Sold - od.Damaged - od.Return) * taxRate, Config.Round).ToCustomString(), NumberStyles.Currency);

                var qty = od.Detail.Qty;

                od.Detail.Qty = od.Sold;

                if (useRelated)
                {
                    var related = GetRelatedProduct(od.Product);

                    if (related != null && related.Taxable)
                    {
                        var retaledTaxRate = TaxRate > 0 ? TaxRate : related.TaxRate;

                        var relatedPrice = Product.GetPriceForProduct(related, this, false, false);

                        retvar += double.Parse(Math.Round(relatedPrice * (od.Sold - od.Damaged - od.Return) * retaledTaxRate, Config.Round).ToCustomString(), NumberStyles.Currency);
                    }
                }

                retvar += GetRelatedCoreTax(od.Detail);

                retvar += GetRelatedAdjTax(od.Detail);

                if (Config.ChargeBatteryRotation)
                    retvar += GetRelatedRotTax(od.Detail);

                od.Detail.Qty = qty;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion


        #region Calculate Split OneDoc Total Cost

        public double OrderSalesTotalCost()
        {
            // calculate item cost
            double retvar = CalculateSalesItemCost();
            // substract the discount
            retvar = retvar - CalculateSalesItemDiscount();
            // add taxes
            retvar = retvar + CalculateSalesTax();
            // return the values
            return Math.Round(retvar, Config.Round);
        }

        public double GetTotalBouquets()
        {
            double qty = 0;

            var bouquetsCategory = Category.Categories.FirstOrDefault(x => x.Name.ToLower() == "bouquets");

            if (bouquetsCategory == null)
                return 0;

            var detailsInCategory = Details.Where(x => x.Product.CategoryId == bouquetsCategory.CategoryId).ToList();

            foreach (var d in detailsInCategory)
            {
                if (!d.IsCredit)
                    qty += d.Qty;

                var x = DataAccess.GetSingleUDF("countedQty", d.ExtraFields);
                if (!string.IsNullOrEmpty(x))
                {
                    double counted = 0;
                    Double.TryParse(x, out counted);
                    qty += counted;
                }
            }

            return qty;
        }
        public double GetTotalARRs()
        {
            double qty = 0;

            var arrsCategory = Category.Categories.FirstOrDefault(x => x.Name.ToLower() == "arrs");

            if (arrsCategory == null)
                return 0;

            var detailsInCategory = Details.Where(x => x.Product.CategoryId == arrsCategory.CategoryId).ToList();

            foreach (var d in detailsInCategory)
            {
                if (!d.IsCredit)
                    qty += d.Qty;

                var x = DataAccess.GetSingleUDF("countedQty", d.ExtraFields);
                if (!string.IsNullOrEmpty(x))
                {
                    double counted = 0;
                    Double.TryParse(x, out counted);
                    qty += counted;
                }
            }

            return qty;
        }
        public double GetTotalInStore()
        {
            double qty = 0;

            foreach (var d in Details)
            {
                var x = DataAccess.GetSingleUDF("countedQty", d.ExtraFields);
                if (!string.IsNullOrEmpty(x))
                {
                    double counted = 0;
                    Double.TryParse(x, out counted);
                    qty += counted;
                }
            }

            qty += CalculateOrderTotalItems();

            return qty;
        }

        public double OrderCreditTotalCost()
        {
            // calculate item cost
            double retvar = CalculateCreditItemCost();
            // substract the discount
            retvar = retvar - CalculateCreditItemDiscount();
            // add taxes
            retvar = retvar + CalculateCreditTax() + CalculatedFreight() + CalculatedOtherCharges();
            // return the values
            return Math.Round(retvar, Config.Round);
        }

        public double CalculateSalesItemCost()
        {
            double retvar = 0;

            if (!AsPresale && Details.Any(x => x.Product.SoldByWeight))
            {
                var groupedDetails = Details
                    .Where(od => !od.Product.IsDiscountItem)
                    .GroupBy(od => new { od.Product.ProductId, od.Price });

                foreach (var grouped in groupedDetails)
                {
                    var firstItem = grouped.First();

                    var copy = firstItem.GetOrderDetailCopy();
                    copy.Qty = grouped.Sum(x => x.Qty);
                    copy.Weight = grouped.Sum(x => x.Weight);
                    copy.Allowance = grouped.Sum(x => x.Allowance);
                    copy.Discount = grouped.Sum(x => x.Discount);

                    retvar += CalculateOneItemCost(copy);
                }
            }
            else
            {
                foreach (OrderDetail od in Details)
                {
                    if (od.Product.IsDiscountItem)
                        continue;

                    if (od.IsCredit)
                        continue;
                    retvar += CalculateOneItemCost(od);
                }
            }

            return retvar;
        }

        public double CalculateCreditItemCost()
        {
            double retvar = 0;

            if (!AsPresale && Details.Any(x => x.Product.SoldByWeight))
            {
                var groupedDetails = Details
                    .Where(od => !od.Product.IsDiscountItem && od.IsCredit)
                    .GroupBy(od => new { od.Product.ProductId, od.Price });

                foreach (var grouped in groupedDetails)
                {
                    var firstItem = grouped.First();

                    var copy = firstItem.GetOrderDetailCopy();
                    copy.Qty = grouped.Sum(x => x.Qty);
                    copy.Weight = grouped.Sum(x => x.Weight);
                    copy.Allowance = grouped.Sum(x => x.Allowance);
                    copy.Discount = grouped.Sum(x => x.Discount);

                    retvar += CalculateOneItemCost(copy);
                }
            }
            else
            {
                foreach (OrderDetail od in Details)
                {
                    if (od.Product.IsDiscountItem)
                        continue;

                    if (!od.IsCredit)
                        continue;
                    retvar += CalculateOneItemCost(od);
                }
            }
            
            return retvar;
        }

        double CalculateSalesItemDiscount()
        {
            double retvar = 0;

            foreach (OrderDetail od in Details)
            {
                if (od.Product.IsDiscountItem)
                    continue;
                if (od.IsCredit)
                    continue;
                retvar += CalculateOneItemDiscount(od);
            }

            return retvar;
        }

        double CalculateCreditItemDiscount()
        {
            double retvar = 0;

            foreach (OrderDetail od in Details)
            {
                if (od.Product.IsDiscountItem)
                    continue;
                if (!od.IsCredit)
                    continue;
                retvar += CalculateOneItemDiscount(od);
            }

            return retvar;
        }

        public double CalculateSalesTax()
        {
            double retvar = 0;
            foreach (OrderDetail od in Details)
            {
                if (od.Product.IsDiscountItem)
                    continue;
                if (od.IsCredit)
                    continue;

                var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                if (od.Taxed)
                    retvar += CalculateOneItemCost(od, true) * taxRate;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        public double CalculateCreditTax()
        {
            double retvar = 0;
            foreach (OrderDetail od in Details)
            {
                if (od.Product.IsDiscountItem)
                    continue;
                if (!od.IsCredit)
                    continue;

                var taxRate = TaxRate > 0 ? TaxRate : od.TaxRate;

                if (od.Taxed)
                    retvar += CalculateOneItemCost(od, true) * taxRate;
            }
            return double.Parse(Math.Round(retvar, Config.Round).ToCustomString(), NumberStyles.Currency);
        }

        #endregion

        public double CalculateOrderTotalItems()
        {
            double totalItems = 0;
            foreach (var detail in Details)
            {
                if (detail.UnitOfMeasure != null)
                {
                    totalItems += (detail.Qty * detail.UnitOfMeasure.Conversion);
                }
                else
                    totalItems += detail.Qty;
            }

            return totalItems;
        }

        public string Filename { get { return fileName; } }

        public bool PendingLoad { get; set; }

        public bool IsDelivery
        {
            get
            {
                return RouteEx.ContainsOrder(OrderId);
            }
        }

        public bool IsWorkOrder
        {
            get
            {
                if (OrderType == OrderType.WorkOrder)
                    return true;

                bool toReturn = false;

                if (!string.IsNullOrEmpty(ExtraFields) && ExtraFields.ToLower().Contains("workorderasset"))
                    toReturn = true;

                return toReturn;
            }
        }

        public IList<OrderDetail> Details
        {
            get { return details; }
        }

        public IList<OrderDetail> DeletedDetails
        {
            get { return deletedDetails; }
        }

        public Order() //protected Order()
        {
            ImageList = new List<string>();
            PriceLevelId = -1;
        }

        public Order(Client client, bool addDefault = true) : this()
        {
            this.PrintedOrderId = string.Empty;
            this.Client = client;
            this.Date = DateTime.Now;
            this.EndDate = DateTime.MinValue;
            this.OrderId = ++LastOrderId;
            this.SalesmanId = Config.SalesmanId;
            this.UniqueId = Guid.NewGuid().ToString("N");

            if (client.TaxRate > 0)
                this.TaxRate = client.TaxRate;
            else
                this.TaxRate = 0;

            orders.Add(this);

            if (client != null && (client.OnCreditHold || client.OverCreditLimit))
                this.ExtraFields = DataAccess.SyncSingleUDF("OrderCreatedOnCreditHold", "1", this.ExtraFields);

            if (Config.DefaultItem > 0 && addDefault)
            {
                if (OrderType == OrderType.Credit && !Config.AddDefaultItemToCredit)
                    return;
                var product = Product.Products.FirstOrDefault(x => x.ProductId == Config.DefaultItem);
                if (product != null)
                {
                    var orderDetail = new OrderDetail(product, 1, this);
                    if (Config.DefaultItemHasPrice)
                    {
                        UnitOfMeasure uom = null;
                        if (product.UnitOfMeasures.Count > 0)
                            uom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);

                        bool cameFromOffer = false;
                        var price = Product.GetPriceForProduct(product, this, out cameFromOffer, false, false, uom);
                        orderDetail.UnitOfMeasure = uom;
                        orderDetail.Price = price;
                        orderDetail.ExpectedPrice = price;
                    }

                    AddDetail(orderDetail);

                    orderDetail.CalculateOfferDetail();

                    this.RecalculateDiscounts();
                }
            }

            if (Config.UseClientClassAsCompanyName)
            {
                var cName = client.NonVisibleExtraProperties != null ? client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "classname") : null;
                if (cName != null)
                    CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cName.Item2.ToLower());
            }

            var automaticDiscount = DataAccess.GetSingleUDF("automatic discount", client.ExtraPropertiesAsString);
            if (!string.IsNullOrEmpty(automaticDiscount))
            {
                DiscountType = DiscountType.Percent;
                float amount = 0;
                float.TryParse(automaticDiscount, out amount);
                DiscountAmount = amount / 100;
            }

            //default other charges
            if (addDefault && Config.AllowOtherCharges && (OrderType == OrderType.Order || OrderType == OrderType.Credit || OrderType == OrderType.Return || OrderType == OrderType.WorkOrder))
            {
                if (Config.OtherChargesVale != 0)
                {
                    this.OtherCharges = Config.OtherChargesVale;
                    this.OtherChargesType = Config.OtherChargesType;
                }
                
                if(!string.IsNullOrEmpty(Config.OtherChargesComments))
                    this.OtherChargesComment = Config.OtherChargesComments;
                
                if (Config.FreightVale != 0)
                {
                    this.Freight = Config.FreightVale;
                    this.FreightType = Config.FreightType;
                }
                
                if(!string.IsNullOrEmpty(Config.FreightComments))
                    this.OtherChargesComment = Config.FreightComments;

                if (OrderType == OrderType.Credit || OrderType == OrderType.Return)
                {
                    if (OtherChargesType == (int)OrderFreightType.Amount)
                        OtherCharges *= -1;
                    
                    if (FreightType == (int)OrderFreightType.Amount)
                        Freight *= -1;
                }
            }
        }

        public void AddDetail(OrderDetail detail)
        {
            this.details.Add(detail);
            detail.Order = this;
        }

        public void DeleteDetail(OrderDetail detail, bool save = true)
        {
            if (detail.Substracted)
                DeleteInventory(detail);

            details.Remove(detail);

            if (save)
            {
                deletedDetails.Add(detail);

                Save();
            }
        }

        private void SetCostDiscountToProduct()
        {
            //Dicount
            List<OrderDetail> itemDetailDiscount = Details.Where(x => (x.FromOffer) && x.OrderDiscountId > 0).ToList();
            //items 
            List<OrderDetail> itemDetail = Details.Where(x => !(x.OrderDiscountId > 0)).ToList();

            itemDetail.ForEach(x => x.ExtraComments = "");

            var setOrderDetailsCostDiscount = Details.ToList();

            setOrderDetailsCostDiscount.ForEach(x => x.CostDiscount = 0);
            setOrderDetailsCostDiscount.ForEach(x => x.CostPrice = 0);

            foreach (var itemD in itemDetailDiscount)
            {
                List<OrderDetail> orderDetails = GetDetailsInDisocunt(itemD, itemDetail);
                var count = orderDetails.Sum(x => x.Qty);
                double costDiscount = (-1 * itemD.Price) / count;

                orderDetails.ForEach(x => x.CostDiscount += costDiscount);

                orderDetails.ForEach(x => x.ExtraComments = setNameDiscount(x.ExtraComments, itemD.OrderDiscount?.Name));

                var unId = DataAccess.GetSingleUDF("UniqueId", itemD.ExtraFields);

                var plId = DataAccess.GetSingleUDF("PLId", itemD.ExtraFields);

                if (!(!string.IsNullOrEmpty(unId) || !string.IsNullOrEmpty(plId)))
                {
                    orderDetails.Where(x => !x.FromOffer).ToList().ForEach(x => x.CostPrice += costDiscount);
                }
            }

            foreach (var itemD in itemDetailDiscount)
            {
                var unId = DataAccess.GetSingleUDF("UniqueId", itemD.ExtraFields);
                if (!string.IsNullOrEmpty(unId))
                {
                    var detailGet = Details.FirstOrDefault(x => x.OriginalId?.ToString() == unId);
                    if (detailGet != null)
                    {
                        if (detailGet.CostDiscount > 0)
                            detailGet.ExtraFields = DataAccess.SyncSingleUDF("CostOrden", detailGet.CostDiscount.ToString(), detailGet.ExtraFields);

                        detailGet.CostPrice += ((-1 * itemD.Price) / detailGet.Qty);
                    }
                }
                /*Case Fixep Price*/

                var plId = DataAccess.GetSingleUDF("PLId", itemD.ExtraFields);
                var pId = DataAccess.GetSingleUDF("ProductId", itemD.ExtraFields);
                if (!string.IsNullOrEmpty(plId) && !string.IsNullOrEmpty(pId))
                {
                    int prouctID = 0;
                    try
                    {
                        prouctID = int.Parse(pId);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                    var count = Details.Where(x => x.Product.ProductId == prouctID && !x.FromOffer).Sum(x => x.Qty);
                    Details.Where(x => x.Product.ProductId == prouctID && !x.FromOffer).ToList().ForEach(x => x.CostPrice += ((-1 * itemD.Price) / count));
                }
            }

            setOrderDetailsCostDiscount.ForEach(x => x.CostDiscount = Math.Round(x.CostDiscount, Config.Round, MidpointRounding.AwayFromZero));
            setOrderDetailsCostDiscount.ForEach(x => x.CostPrice = Math.Round(x.CostPrice, Config.Round, MidpointRounding.AwayFromZero));
        }

        private string setNameDiscount(string extraComments, string name)
        {
            if (string.IsNullOrEmpty(extraComments))
            {
                return name;
            }

            List<string> names = extraComments.Split(',').ToList();
            if (!extraComments.Contains(name))
            {
                extraComments += "," + name;
            }
            return extraComments;
        }

        private List<OrderDetail> GetDetailsInDisocunt(OrderDetail itemD, List<OrderDetail> itemDetail)
        {
            double countItem = 0;
            double amountItems = 0;

            List<Product> productList = new List<Product>();

            productList = Product.GetProductListForOrder(this, (this.OrderType == OrderType.Credit || this.OrderType == OrderType.Return), 0).ToList();

            List<OrderDetail> result = new List<OrderDetail>();

            #region If Gift
            var extrafields = DataAccess.GetSingleUDF("UniqueId", itemD.ExtraFields);
            if (!string.IsNullOrEmpty(extrafields))
            {
                var uniqueIdList = extrafields.Split(',').ToList();
                return itemDetail.Where(x => uniqueIdList.Contains(x.OriginalId?.ToString())).ToList();
            }

            #endregion

            var itemDOrderDiscount = (itemD.OrderDiscount == null && itemD.OrderDiscountId > 0) ? OrderDiscount.List.FirstOrDefault(x => x.Id == itemD.OrderDiscountId) : itemD.OrderDiscount;
            var itemDOrderDiscountBreak = (itemD.OrderDiscountBreak == null && itemD.OrderDiscountBreakId > 0) ? itemDOrderDiscount.OrderDiscountBreaks.FirstOrDefault(x => x.Id == itemD.OrderDiscountBreakId) : itemD.OrderDiscountBreak;

            if (itemDOrderDiscountBreak != null && itemDOrderDiscountBreak.Id != 0)
            {
                var discount = OrderDiscount.List.FirstOrDefault(x => x.Id == itemDOrderDiscountBreak.OrderDiscountId);
                itemDOrderDiscount = discount;
            }

            if ((itemDOrderDiscount == null || itemDOrderDiscount.Id == 0) && (itemDOrderDiscountBreak != null || itemDOrderDiscountBreak.Id == 0))
                return result;

            List<int> idVendor = itemDOrderDiscount.OrderDiscountVendors.Select(x => x.VendorId).ToList();
            List<int> idCategoryP = itemDOrderDiscount.OrderDiscountCategories.Where(x => x.CategoryType == (int)OrderDiscountCategoryType.Product).Select(x => x.CategoryId).ToList();

            var listProductBuy = (itemDOrderDiscount.OrderDiscountProducts.Count > 0)
                       ? itemDOrderDiscount.OrderDiscountProducts.Select(x => x.ProductId).ToList()
                       : itemDOrderDiscount.OrderDiscountVendors.Count > 0
                       ? productList.Where(x => idVendor.Contains(x.VendorId)).Select(x => x.ProductId).ToList()
                       : (itemDOrderDiscount.OrderDiscountCategories.Any(x => x.CategoryType == (int)OrderDiscountCategoryType.Product))
                       ? productList.Where(x => idCategoryP.Contains(x.CategoryId)).Select(x => x.ProductId).ToList()
                       : productList.Select(x => x.ProductId).ToList();

            var listProductVisible = (itemDOrderDiscount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
               ? itemDOrderDiscountBreak.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList()
               : productList.Select(x => x.ProductId).ToList();

            listProductBuy = IntersectList(listProductBuy, listProductVisible);

            if (itemD.OrderDiscountBreak != null)
            {
                //((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && itemB.MinQty <= calculatedTotalOrder) 
                //|| (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && itemB.MinQty <= countItemOrder));
                bool isCount = (itemDOrderDiscount.DiscountType == (int)CustomerDiscountType.Quantity);

                foreach (var item in itemDetail)
                {
                    if (!listProductBuy.Contains(item.Product.ProductId))
                        continue;

                    if (isCount)
                    {
                        countItem += item.Qty;
                        if (/*countItem >= itemDOrderDiscountBreak.MinQty && */(itemDOrderDiscountBreak.MaxQty == -1 || countItem <= itemDOrderDiscountBreak.MaxQty))
                        {
                            result.Add(item);
                        }
                        else
                        {
                            countItem -= item.Qty;
                        }

                    }
                    else
                    {
                        amountItems += (item.Qty * item.Price);
                        if (/*countItem >= itemDOrderDiscountBreak.MinQty &&*/ (itemDOrderDiscountBreak.MaxQty == -1 || countItem <= itemDOrderDiscountBreak.MaxQty))
                        {
                            result.Add(item);
                        }
                        else
                        {
                            amountItems += (item.Qty * item.Price);
                        }
                    }
                }
            }
            else
            {
                bool isCount = (itemDOrderDiscount.DiscountType == (int)CustomerDiscountType.Quantity);

                double MinQ = (itemDOrderDiscount.OrderDiscountClients.Any(x => x.ClientId == Client.ClientId))
                            ? itemDOrderDiscount.OrderDiscountClients.FirstOrDefault(x => x.ClientId == Client.ClientId).Buy
                            : (itemDOrderDiscount.OrderDiscountClientAreas.Any(x => x.AreaId == Client.AreaId))
                            ? itemDOrderDiscount.OrderDiscountClientAreas.FirstOrDefault(x => x.AreaId == Client.AreaId).Buy
                            : (itemDOrderDiscount.OrderDiscountCategories.Any(x => (x.CategoryType == (int)OrderDiscountCategoryType.Client) && (Client.CategoryId == x.CategoryId)))
                            ? itemDOrderDiscount.OrderDiscountCategories.FirstOrDefault(x => x.CategoryType == (int)OrderDiscountCategoryType.Client && Client.CategoryId == x.CategoryId).Buy
                            : double.MaxValue;




                foreach (var item in itemDetail)
                {
                    if (!listProductBuy.Contains(item.Product.ProductId) || OrderDiscount.IsOfferDiscount(item, this))
                        continue;

                    result.Add(item);
                }
            }

            return result.Where(x => !(OrderDiscount.IsOfferDiscount(x, this))).ToList();
        }


        void DeleteInventory(OrderDetail detail)
        {
            //ticket #0012288
            UpdateInventory(detail, 1);
        }

        public void UpdateInventory(OrderDetail detail, int factor)
        {
            if (OrderType == OrderType.Load || OrderType == OrderType.Bill)
                return;

            if (detail != null && detail.Product != null)
            {
                var qty = detail.Qty;
                if (detail.Product.SoldByWeight && detail.Product.InventoryByWeight)
                {
                    if (AsPresale)
                        qty = (float)(detail.Qty * detail.Product.Weight);
                    else
                        qty = detail.Weight;

                }
                else
                    if (detail.Product.SoldByWeight)
                    qty = 1;

                if (OrderType == OrderType.Consignment)
                {
                    if (Config.UseFullConsignment)
                        qty = detail.ConsignmentPicked;
                    else
                        qty = detail.ConsignmentPick;
                }

                int detailFactor = 1;
                if (detail.IsCredit)
                    detailFactor = detail.Damaged ? 0 : -1;

                string lot = detail.Product.UseLot ? detail.Lot : "";

                detail.Product.UpdateInventory(AsPresale, qty, detail.UnitOfMeasure, lot, detailFactor * factor, detail.Weight);

            }
        }

        public bool DeleteRelated(OrderDetail detail, bool updateInv = true)
        {
            if (detail.RelatedOrderDetail != 0)
            {
                var relatedDetail = Details.FirstOrDefault(x => x.OrderDetailId == detail.RelatedOrderDetail);
                if (relatedDetail != null)
                {
                    if (updateInv)
                        DeleteDetail(relatedDetail);
                    else
                        Details.Remove(relatedDetail);

                    if (!string.IsNullOrEmpty(detail.ExtraFields))
                    {
                        var extra = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);
                        if (!string.IsNullOrEmpty(extra))
                        {
                            var parts = extra.Split(",");

                            foreach (var p in parts)
                            {
                                int detailID = 0;
                                Int32.TryParse(p, out detailID);
                                if (detailID > 0)
                                {
                                    var otherDetail = Details.FirstOrDefault(x => x.OrderDetailId == detailID);
                                    if (otherDetail != null)
                                    {
                                        if (updateInv)
                                            DeleteDetail(otherDetail);
                                        else
                                            Details.Remove(otherDetail);
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        public void Delete()
        {
            try
            {
                if (Session.session != null)
                    Session.session.DeleteDetailFromOrder(this);
            }
            catch
            {
            }

            foreach (var detail in details.ToList())
                DeleteDetail(detail, false);
            // Delete from the list
            details.Clear();

            foreach (var item in ImageList)
            {
                var path = Path.Combine(Config.OrderImageStorePath, item);
                if (File.Exists(path))
                    File.Delete(path);
            }
            ImageList.Clear();

            Order.orders.Remove(this);

            // Delete from t he file
            if (!string.IsNullOrEmpty(fileName))
                if (File.Exists(this.fileName))
                    File.Delete(this.fileName);

            deleted = true;
            DataAccess.SaveInventory();
        }

        public void ForceDelete()
        {
            foreach (var image in ImageList)
            {
                var imgPath = Path.Combine(Config.ImageStorePath, image);
                if (File.Exists(imgPath))
                    File.Delete(image);
            }

            if (Session.session != null)
            {
                //save detail before deleting order
                var detail = Session.sessionDetails.FirstOrDefault(x => x.orderUniqueId == this.UniqueId);
                if (detail == null)
                    Session.session.AddDetailFromOrder(this);

                Session.session.Save();
            }

            // Delete from the list
            Order.orders.Remove(this);
            // Delete from t he file
            if (!string.IsNullOrEmpty(fileName))
                if (File.Exists(this.fileName))
                    File.Delete(this.fileName);
            deleted = true;
        }

        public void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    if (deleted)
                        return;
                    lock (syslock)
                    {
                        EnsureFileNameCreated();
                    }
                    //FileOperationsLocker.InUse = true;
                    using (StreamWriter writer = new StreamWriter(this.fileName))
                    {
                        SerializeOrder(writer);
                    }
                    DataAccess.SaveInventory();
                    BackgroundDataSync.SyncOrderPayment();
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public bool Locked()
        {
            if (Voided)
                return true;
            if (Finished)
            {
                if (!Config.CanChangeFinalizedInvoices)
                    return true;
            }
            if (Dexed)
                return true;
            if (IsDelivery && !Config.DeliveryEditable)
                return true;
            if (PrintedCopies > 0 && Config.LockOrderAfterPrinted)
                return true;
            if (IsDelivery && OrderType == OrderType.Order && !Config.OrderCanBeChanged)
                return true;
            if (IsDelivery && OrderType == OrderType.Credit && !Config.CreditCanBeChanged)
                return true;

            return false;
        }

        public void Reship()
        {
            var reason = Reason.Find(ReasonId);
            if (reason != null)
                LoadingError = reason.LoadingError;

            var detailsToRemove = new List<OrderDetail>();

            foreach (var od in details)
            {
                if (od.Ordered == 0)
                {
                    detailsToRemove.Add(od);
                    continue;
                }

                //devolver inventario 
                this.UpdateInventory(od, 1);
                od.Qty = od.Ordered;
                //restar inventario with original qty
                this.UpdateInventory(od, -1);

                od.LoadingError |= LoadingError;

                if (OrderType != OrderType.Consignment && !od.LoadingError)
                    DeleteInventory(od);
            }

            var added_back = new List<OrderDetail>();

            foreach (var d in deletedDetails)
            {
                if (d.Ordered == 0)
                    continue;

                d.Deleted = false;

                d.Qty = d.Ordered;
                AddDetail(d);

                added_back.Add(d);
            }

            foreach (var d in added_back)
                deletedDetails.Remove(d);

            foreach (var d in detailsToRemove)
                DeleteDetail(d, false);

            // remove ALL the details
            //details.Clear ();
            EndDate = DateTime.Now;
            Reshipped = true;
            Finished = true;
            Save();
        }

        public void Void()
        {
            if (!Reshipped)
            {
                if (OrderType == OrderType.Consignment)
                    UpdateConsignmentInventory(1);

                foreach (OrderDetail od in details)
                {
                    if (OrderType != OrderType.Consignment)
                        DeleteInventory(od);

                    od.Price = 0;
                    od.ExpectedPrice = 0;
                    od.ConsignmentCounted = od.ConsignmentSet = od.ConsignmentUpdated = false;
                }
            }

            if (IsParLevel)
            {
                ClientDailyParLevel.Void(Client.ClientId, DateTime.Now.DayOfWeek);
                IsParLevel = false;
            }

            // remove ALL the details
            //details.Clear ();
            EndDate = DateTime.Now;
            Voided = true;
            Finished = true;
            DiscountAmount = 0;
            DiscountComment = string.Empty;

            if (Config.ButlerCustomization)
                PrintedCopies = 0;
            Save();
        }

        public string SerializeToString()
        {

            string pointsString = string.Empty;
            if (SignaturePoints.Count > 0)
                pointsString = SerializeSignatureAsString(SignaturePoints);

            string commentTemp = this.Comments == null ? string.Empty : this.Comments.Replace((char)13, (char)32).Replace((char)10, (char)32);
            string dCommentTemp = this.DiscountComment == null ? string.Empty : this.DiscountComment.Replace((char)13, (char)32).Replace((char)10, (char)32);
            string sNameTemp = this.SignatureName == null ? string.Empty : SignatureName.Replace((char)13, (char)32).Replace((char)10, (char)32);
            string sPoNumber = this.PONumber == null ? string.Empty : PONumber.Replace((char)13, (char)32).Replace((char)10, (char)32);

            string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}" +
                "{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}{0}{19}{0}{20}{0}{21}{0}{22}{0}{23}{0}{24}{0}" +
                "{25}{0}{26}{0}{27}{0}{28}{0}{29}{0}{30}{0}{31}{0}{32}{0}{33}{0}{34}{0}{35}{0}{36}{0}{37}{0}{38}{0}{39}{0}" +
                "{40}{0}{41}{0}{42}{0}{43}{0}{44}{0}{45}{0}{46}{0}{47}{0}{48}{0}{49}{0}{50}{0}{51}{0}{52}{0}{53}{0}{54}{0}{55}" +
                "{0}{56}{0}{57}{0}{58}{0}{59}{0}{60}{0}{61}{0}{62}{0}{63}", (char)20,
                              this.OrderId,                 //0
                              this.Client.ClientId,         //1
                              this.SalesmanId,              //2
                              this.Date.Ticks,              //3
                              (int)this.OrderType,          //4
                              this.PrintedOrderId,          //5
                              commentTemp,                  //6
                              this.Latitude,                //7
                              this.Longitude,               //8
                              pointsString,                 //9
                              UniqueId,                     //10
                              sNameTemp,                    //11
                              TaxRate,                      //12
                              (int)DiscountType,            //13
                              DiscountAmount,               //14
                              dCommentTemp,                 //15
                              Voided,                       //16
                              sPoNumber ?? string.Empty,     //17
                              EndDate.Ticks,                //18
                              ShipDate.Ticks,               //19
                              BatchId,                      //20
                              Dexed ? "1" : "0",            //21
                              this.Finished ? "1" : "0",    //22
                              CompanyName,                  //23
                              PrintedCopies,                //24
                              this.AsPresale ? "1" : "0",   //25
                              this.Reshipped ? "1" : "0",   //26
                              this.ExtraFields,             //27
                              OriginalSalesmanId,           //28
                              PendingLoad ? "1" : "0",      //29
                              IsParLevel ? "1" : "0",       //30
                              ReasonId.ToString(),          //31
                              Modified ? "1" : "0",         //32
                              ImageListAsString(),          //33
                              ReshipDate.Ticks.ToString(),  //34
                              IsQuote ? "1" : "0",          //35
                              FromInvoiceId,                //36
                              IsProjection ? "1" : "0",     //37
                              !string.IsNullOrEmpty(RelationUniqueId) ? RelationUniqueId : "",     //38
                              IsScanBasedTrading ? "1" : "0", //39
                              !string.IsNullOrEmpty(SignatureUniqueId) ? SignatureUniqueId : "",     //40
                              !string.IsNullOrEmpty(DepartmentUniqueId) ? DepartmentUniqueId : "",     //41
                              OriginalOrderId,              //42
                              SplitedByDepartment ? "1" : "0",             //43
                              LoadingError ? "1" : "0",              //44
                              CompanyId,                             //45
                              HasDisolSurvey ? "1" : "0",             //46               
                              PriceLevelId,          //47
                              CremiMexDepartment,                      //48
                              QuoteModified ? "1" : "0",                          //49
                              SiteId.ToString(),                         //50
                              DepartmentId.ToString(),                   //51
                              NeedToCalculate ? "1" : "0",             //52
                              OtherCharges.ToString(),             //53
                              Freight.ToString(),             //54
                              OtherChargesType,             //55
                              FreightType,             //56
                              OtherChargesComment ?? "",             //57
                              FreightComment ?? "",             //58
                              IsCheckOrder ? "1" : "0",         //59
                              DetailsChanged ? "1" : "0",        //60,
                              AssetId,                               //61
                              IsExchange ? "1" : "0"            //62
                              );

            return line;
        }

        public static string SerializeOrderDetailToString(OrderDetail detail)
        {
            string dComment = detail.Comments == null ? string.Empty : detail.Comments.Replace((char)13, (char)32).Replace((char)10, (char)32);
            string lot = detail.Lot == null ? string.Empty : detail.Lot.Replace((char)13, (char)32).Replace((char)10, (char)32);

            string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}" +
                "{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}{0}{19}{0}{20}{0}{21}{0}{22}{0}{23}{0}{24}" +
                "{0}{25}{0}{26}{0}{27}{0}{28}{0}{29}{0}{30}{0}{31}{0}{32}{0}{33}{0}{34}{0}{35}{0}{36}{0}{37}{0}{38}{0}{39}" +
                "{0}{40}{0}{41}{0}{42}{0}{43}{0}{44}{0}{45}{0}{46}{0}{47}{0}{48}{0}{49}{0}{50}{0}{51}{0}{52}{0}{53}{0}{54}" +
                "{0}{55}{0}{56}{0}{57}{0}{58}{0}{59}{0}{60}{0}{61}{0}{62}{0}{63}{0}{64}{0}{65}{0}{66}{0}{67}",
                (char)20,
                detail.OrderDetailId,                                           //0
                detail.Product.ProductId,                                       //1
                detail.Qty,                                                     //2
                detail.Price,                                                   //3
                dComment,                                                       //4
                detail.FromOffer ? "1" : "0",                                   //5
                lot,                                                            //6
                detail.Damaged ? "1" : "0",                                     //7
                detail.Ordered,                                                 //8
                detail.OriginalId,                                              //9
                detail.Substracted ? "1" : "0",                                 //10
                detail.IsCredit ? "1" : "0",                                    //11
                detail.ExpectedPrice,                                           //12
                detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Id : 0,     //13
                detail.Deleted ? "1" : "0",                                     //14
                detail.Weight,                                                  //15
                detail.RelatedOrderDetail,                                      //16
                detail.ConsignmentNew,                                          //17
                detail.ConsignmentOld,                                          //18
                detail.ConsignmentSet ? "1" : "0",                              //19
                detail.ConsignmentCounted ? "1" : "0",                          //20
                detail.ConsignmentUpdated ? "1" : "0",                          //21
                detail.ConsignmentNewPrice,                                     //22
                detail.ConsignmentCount,                                        //23
                detail.ExtraFields,                                             //24
                detail.DeliveryScanningChecked ? "1" : "0",                                     //25
                detail.Taxed ? "1" : "0",                                       //26
                detail.ConsignmentSalesItem ? "1" : "0",                        //27
                detail.OfferDetFreeItem,                                        //28
                detail.LoadStarting,                                            //29
                detail.Allowance,                                               //30
                detail.FromOfferPrice,                                          //31
                detail.TaxRate,                                                 //32
                detail.Discount,                                                //33
                (int)detail.DiscountType,                                       //34
                detail.ConsignmentPicked,                                       //35
                detail.ConsignmentCreditItem ? "1" : "0",                       //36
                OrderDetail.GetConsLotsAsString(detail.ConsignmentCountedLots), //37
                OrderDetail.GetConsLotsAsString(detail.ConsignmentPickedLots),  //38
                !string.IsNullOrEmpty(detail.ConsignmentComment) ? detail.ConsignmentComment : string.Empty, //39
                detail.ParLevelDetail ? "1" : "0",                              //40
                detail.OriginalUoM != null ? detail.OriginalUoM.Id : 0,         //41
                detail.DeliveryQtyAsString(),                                   //42
                detail.Id,                                                      //43
                detail.ReasonId,                                                //44
                detail.FromOfferType,                                           //45
                detail.HiddenItem ? "1" : "0",                                  //46
                detail.AdjustmentItem ? "1" : "0",                              //47
                !string.IsNullOrEmpty(detail.LabelUniqueId) ? detail.LabelUniqueId : "", //48
                detail.ProductDepartment ?? "",                                                 //49
                detail.IsFreeItem ? "1" : "0",                                   //50
                detail.LoadingError ? "1" : "0",                                 //51
                detail.LotExpiration.Ticks,                                      //52
                detail.CompletedFromScanner ? "1" : "0",                                    //53
                detail.ListPrice,                                                //54
                detail.WeightEntered ? "1" : "0",                                //55
                detail.isMixAndMatchRelated ? "1" : "0",                          //56
                detail.PriceLevelSelected.ToString(),                              //57
                detail.ScannedQty,                                                  //58
                detail.OrderDiscountId,                                             //59
                detail.OrderDiscountBreakId,                                             //60
                detail.CostDiscount.ToString(),                                             //61
                detail.CostPrice.ToString(),                                             //62
                detail.ExtraComments ?? "",
                detail.ModifiedManually ? "1" : "0",
                detail.IgnoreInOffers ? "1" : "0",
                detail.AlreadyAskedForOffers ? "1" : "0"
                );

            return line;
        }

        public bool MergeDetails()
        {

            List<int> relateds = new List<int>();

            foreach (var detail in Details.Where(x => x.RelatedOrderDetail > 0).ToList())
            {
                relateds.Add(detail.RelatedOrderDetail);

                var values = DataAccess.GetSingleUDF("ExtraRelatedItem", detail.ExtraFields);

                if (!string.IsNullOrEmpty(values))
                {
                    var parts = values.Split(",");

                    foreach (var p in parts)
                    {
                        int orderDetailId = 0;
                        Int32.TryParse(p, out orderDetailId);

                        if (orderDetailId > 0 && !relateds.Contains(orderDetailId))
                            relateds.Add(orderDetailId);
                    }
                }
            }

            var detailsToRemove = new List<OrderDetail>();
            foreach (var detail in Details)
            {
                if (detail.Product.IsDiscountItem)
                    continue;

                if (detail.IsRelated)
                    continue;
                
                if (detailsToRemove.Contains(detail))
                    continue;

                if (detail.Product.SoldByWeight)
                    continue;

                if (relateds.Contains(detail.OrderDetailId))
                    continue;

                var similars = GetSimilars(detail);

                if (similars.Count > 0)
                {
                    detailsToRemove.AddRange(similars);

                    foreach (var d in similars)
                        detail.Qty += d.Qty;
                }
            }

            foreach (var d in detailsToRemove)
                Details.Remove(d);

            return detailsToRemove.Count > 0;
        }

        public List<OrderDetail> GetSimilars(OrderDetail detail)
        {
            var listToReturn = new List<OrderDetail>();
            var source = Details.Where(x => x != detail).ToList();

            if (OrderDiscount.HasDiscounts)
            {
                listToReturn = source.Where(x => x.Product.ProductId == detail.Product.ProductId &&
                      Math.Round(x.Price, 2) == Math.Round(detail.Price, 2) && x.Lot == detail.Lot &&
                      ((x.UnitOfMeasure != null && detail.UnitOfMeasure != null && x.UnitOfMeasure.Id == detail.UnitOfMeasure.Id) || (x.UnitOfMeasure == null && detail.UnitOfMeasure == null)) &&
                      x.IsFreeItem == detail.IsFreeItem && x.IsCredit == detail.IsCredit && x.Damaged == detail.Damaged && x.IsRelated == detail.IsRelated && x.ReasonId == detail.ReasonId && x.FromOffer == detail.FromOffer).ToList();
            }
            else
            {
                listToReturn = source.Where(x => x.Product.ProductId == detail.Product.ProductId &&
                        Math.Round(x.Price, 2) == Math.Round(detail.Price, 2) && x.Lot == detail.Lot &&
                        ((x.UnitOfMeasure != null && detail.UnitOfMeasure != null && x.UnitOfMeasure.Id == detail.UnitOfMeasure.Id) || (x.UnitOfMeasure == null && detail.UnitOfMeasure == null)) &&
                        x.IsFreeItem == detail.IsFreeItem && x.IsCredit == detail.IsCredit && x.Damaged == detail.Damaged && x.IsRelated == detail.IsRelated && x.ReasonId == detail.ReasonId).ToList();
            }

            return listToReturn;
        }

        public static IList<Order> FindClientOrders(Client client)
        {
            return Order.orders.FindAll(x => x.Client.ClientId == client.ClientId);
        }

        public static void Clear()
        {
            orders.Clear();
        }

        static protected List<Order> orders = new List<Order>();

        public static Order Find(int orderId)
        {
            for (int i = 0; i < orders.Count; i++)
                if (orders[i].OrderId == orderId)
                    return orders[i];
            return null;
        }

        public static IList<Order> Orders
        {
            get { return orders; }
        }

        //Saving & serialization methods
        static internal void AddOrderFromFile(string file)
        {
            Order newOrder = new Order();
            if (newOrder.LoadFromFile(file))
            {
                orders.Add(newOrder);

                if (newOrder.OrderType == OrderType.Load)
                    return;

                var batch = Batch.List.FirstOrDefault(x => x.Id == newOrder.BatchId);

                if (batch == null)
                {
                    batch = new Batch(newOrder.Client);
                    batch.ClockedIn = newOrder.Date;
                    newOrder.BatchId = batch.Id;
                    batch.Save();
                    newOrder.Save();
                }
                else if (batch != null && batch.Client.ClientId != newOrder.Client.ClientId)
                {
                    var count = batch.Orders().Count();
                    if (count == 0)
                    {
                        batch.Client = newOrder.Client;
                        batch.ClockedIn = newOrder.Date;
                    }
                    else
                    {
                        var newbatch = new Batch(newOrder.Client);
                        newbatch.ClockedIn = newOrder.Date;
                        newbatch.Save();
                        newOrder.BatchId = newbatch.Id;
                        newOrder.Save();
                    }
                }
            }
        }

        private void EnsureFileNameCreated()
        {
            if (string.IsNullOrEmpty(fileName))
                this.fileName = Path.Combine(Config.CurrentOrdersPath, Guid.NewGuid().ToString());
        }

        protected virtual void SerializeOrder(StreamWriter stream)
        {
            SerializeOrderLine(stream);
            foreach (OrderDetail detail in this.details)
                SerializeOrderDetail(stream, detail);
            if (this.deletedDetails.Count > 0)
            {
                stream.WriteLine("DELETED ITEMS");
                foreach (OrderDetail detail in this.deletedDetails)
                    SerializeOrderDetail(stream, detail);
            }
        }

        protected virtual void SerializeOrderLine(StreamWriter stream)
        {
            string line = SerializeToString();
            stream.WriteLine(line);
        }

        static void SerializeOrderDetail(StreamWriter stream, OrderDetail detail)
        {
            string line = SerializeOrderDetailToString(detail);
            stream.WriteLine(line);
            detail.Persisted = true;
        }

        protected void DeserializeOrder(StreamReader reader)
        {
            string line = string.Empty;
            int orderId_ = 0;

            try
            {
                //Read the order line
                line = reader.ReadLine();
                string[] parts = line.Split(new char[] { (char)20 });

                if (parts.Length < 23)
                {
                    string log = "The order line has less element than expected, it has " + parts.Length.ToString() + " instead of 23";
                    Logger.CreateLog(log);
                    //Xamarin.Insights.Report(new Exception(log));
                }

                orderId_ = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                int clientId = Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                Client client_ = Client.Find(clientId);
                if (client_ == null)
                {
                    var msg = "order makes reference to a no found customer: " + parts[1] + " will create one temporal";
                    Logger.CreateLog(msg);
                    client_ = Client.CreateTemporalClient(clientId);
                    //Xamarin.Insights.Report(new Exception(msg));
                }
                int vendorId_ = Convert.ToInt32(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                DateTime date_ = new DateTime(Convert.ToInt64(parts[3], System.Globalization.CultureInfo.InvariantCulture));
                OrderType type_ = (OrderType)Convert.ToInt32(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                string printedId_ = parts[5];
                string comments_ = parts[6];
                double latitude = Convert.ToDouble(parts[7], System.Globalization.CultureInfo.InvariantCulture);
                double longitude = Convert.ToDouble(parts[8], System.Globalization.CultureInfo.InvariantCulture);

                if (parts.Length > 9 && parts[9].Length > 0)
                {
                    SignaturePoints = new List<SixLabors.ImageSharp.Point>();
                    if (!string.IsNullOrEmpty(parts[9]))
                    {
                        foreach (string point in parts[9].Split(new char[] { ';' }))
                        {
                            string[] components = point.Split(new char[] { ',' });
                            SignaturePoints.Add(new SixLabors.ImageSharp.Point()
                            {
                                X = (int)Convert.ToSingle(components[0]),
                                Y = (int)Convert.ToSingle(components[1])
                            });
                        }
                    }
                }

                this.OrderId = orderId_;
                if (LastOrderId < OrderId)
                    LastOrderId = OrderId;
                this.Client = client_;
                // this.OrderId = LastOrderId++;
                this.SalesmanId = vendorId_;
                this.Date = date_;
                this.OrderType = type_;
                this.PrintedOrderId = printedId_;
                this.Comments = comments_;
                this.Latitude = latitude;
                this.Longitude = longitude;
                this.UniqueId = parts[10];
                this.SignatureName = parts[11];
                this.TaxRate = Convert.ToDouble(parts[12]);
                this.DiscountType = (DiscountType)Convert.ToSingle(parts[13]);
                this.DiscountAmount = Convert.ToSingle(parts[14]);
                this.DiscountComment = parts[15];
                this.Voided = Convert.ToBoolean(parts[16]);
                if (parts.Length > 17)
                    this.PONumber = parts[17];
                if (parts.Length > 18)
                    this.EndDate = new DateTime(Convert.ToInt64(parts[18], System.Globalization.CultureInfo.InvariantCulture));
                if (parts.Length > 19)
                    this.ShipDate = new DateTime(Convert.ToInt64(parts[19], System.Globalization.CultureInfo.InvariantCulture));
                if (parts.Length > 20)
                    this.BatchId = Convert.ToInt32(parts[20]);
                if (parts.Length > 21)
                    this.Dexed = Convert.ToInt32(parts[21]) > 0;
                if (parts.Length > 22)
                    this.Finished = Convert.ToInt32(parts[22]) > 0;
                if (parts.Length > 23)
                    this.CompanyName = parts[23];
                if (parts.Length > 24)
                    this.PrintedCopies = Convert.ToInt32(parts[24]);
                if (parts.Length > 25)
                    this.AsPresale = Convert.ToInt32(parts[25]) > 0;

                if (parts.Length > 26)
                    this.Reshipped = Convert.ToInt32(parts[26]) > 0;

                if (parts.Length > 27)
                    this.ExtraFields = parts[27];

                if (parts.Length > 28)
                    this.OriginalSalesmanId = Convert.ToInt32(parts[28]);

                if (parts.Length > 29)
                    PendingLoad = Convert.ToInt32(parts[29]) > 0;

                if (parts.Length > 30)
                    IsParLevel = Convert.ToInt32(parts[30]) > 0;

                if (parts.Length > 31)
                    ReasonId = Convert.ToInt32(parts[31]);

                if (parts.Length > 32)
                    Modified = Convert.ToInt32(parts[32]) > 0;

                if (parts.Length > 33)
                {
                    ImageList = new List<string>();

                    if (!string.IsNullOrEmpty(parts[33]))
                        ImageList.AddRange(parts[33].Split('|'));
                }

                if (parts.Length > 34)
                    ReshipDate = new DateTime(Convert.ToInt64(parts[34], System.Globalization.CultureInfo.InvariantCulture));

                if (parts.Length > 35)
                    IsQuote = Convert.ToInt32(parts[35]) > 0;

                if (parts.Length > 36)
                    FromInvoiceId = Convert.ToInt32(parts[36]);

                if (parts.Length > 37)
                    IsProjection = Convert.ToInt32(parts[37]) > 0;

                if (parts.Length > 38)
                    RelationUniqueId = parts[38];

                if (parts.Length > 39)
                    IsScanBasedTrading = Convert.ToInt32(parts[39]) > 0;

                if (parts.Length > 40)
                    SignatureUniqueId = parts[40];

                if (parts.Length > 41)
                {
                    DepartmentUniqueId = parts[41];
                    if (!string.IsNullOrEmpty(DepartmentUniqueId))
                        Department = ClientDepartment.Departments.FirstOrDefault(x => x.UniqueId == DepartmentUniqueId);
                }

                if (parts.Length > 42)
                    OriginalOrderId = Convert.ToInt32(parts[42]);

                if (parts.Length > 43)
                    SplitedByDepartment = Convert.ToInt32(parts[43]) > 0;

                if (parts.Length > 44)
                    LoadingError = Convert.ToInt32(parts[44]) > 0;

                if (parts.Length > 45)
                    CompanyId = Convert.ToInt32(parts[45]);

                if (parts.Length > 46)
                    HasDisolSurvey = Convert.ToInt32(parts[46]) > 0;

                if (parts.Length > 47)
                    PriceLevelId = Convert.ToInt32(parts[47]);

                if (parts.Length > 48)
                    CremiMexDepartment = parts[48];

                if (parts.Length > 49)
                    QuoteModified = Convert.ToInt32(parts[49]) > 0;

                if (parts.Length > 50)
                    SiteId = Convert.ToInt32(parts[50]);

                if (parts.Length > 51)
                    DepartmentId = Convert.ToInt32(parts[51]);

                if (parts.Length > 52)
                    NeedToCalculate = Convert.ToInt32(parts[52]) > 0;

                if (parts.Length > 53)
                    OtherCharges = Convert.ToDouble(parts[53]);

                if (parts.Length > 54)
                    Freight = Convert.ToDouble(parts[54]);


                if (parts.Length > 55)
                    OtherChargesType = Convert.ToInt32(parts[55]);


                if (parts.Length > 56)
                    FreightType = Convert.ToInt32(parts[56]);

                if (parts.Length > 57)
                    OtherChargesComment = parts[57];

                if (parts.Length > 58)
                    FreightComment = parts[58];

                if (parts.Length > 59)
                    IsCheckOrder = Convert.ToInt32(parts[59]) > 0;

                if (parts.Length > 60)
                    DetailsChanged = Convert.ToInt32(parts[60]) > 0;

                if (parts.Length > 61)
                    AssetId = Convert.ToInt32(parts[61]);
                
                if (parts.Length > 62)
                    IsExchange = Convert.ToInt32(parts[62]) > 0;

                var source = details;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "DELETED ITEMS")
                    {
                        source = deletedDetails;
                        continue;
                    }
                    parts = line.Split(new char[] { (char)20 });
                    int orderDetailId = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    int productId = Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    Product product = Product.Find(productId, true);
                    if (product == null)
                    {
                        var msg = "order makes reference to a no found product: " + parts[1];
                        Logger.CreateLog(msg);

                        product = Product.CreateNotFoundProduct(productId);
                    }
                    double qty = Math.Round(Convert.ToSingle(parts[2], CultureInfo.InvariantCulture), 2);
                    double price = Convert.ToDouble(parts[3], CultureInfo.InvariantCulture);

                    string comments = parts[4];

                    CreateOrderDetail(parts, source, orderDetailId, product, qty, price, comments);
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog("Error creating OrderDetail for Order with id=" + orderId_ + " line=" + line);
                Logger.CreateLog(ee);
            }
        }

        public void SendUrgentLog()
        {
            try
            {
                NetAccess access = new NetAccess();

                access.OpenConnection("app.laceupsolutions.com", 9999);
                access.WriteStringToNetwork("SendLogFile");
                access.WriteStringToNetwork("URGENT LOG<br>" + Config.SerializeConfig().Replace(System.Environment.NewLine, "<br>"));
                access.SendFile(Config.LogFile);
                access.WriteStringToNetwork("Goodbye");
                Thread.Sleep(1000);
                access.CloseConnection();
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        protected bool LoadFromFile(string file)
        {
            try
            {
                this.fileName = file;
                using (StreamReader reader = new StreamReader(this.fileName))
                {
                    DeserializeOrder(reader);
                }
                //check if the order is ok, otherwise, just delete it
                if (this.Client == null /*|| (!Voided && this.details.Count == 0)*/)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                return false;
            }
        }

        protected virtual void CreateOrderDetail(string[] parts, List<OrderDetail> source, int orderDetailId, Product product, double qty, double price, string comments)
        {
            OrderDetail detail = new OrderDetail(product, Convert.ToSingle(qty), orderDetailId, this);
            detail.Comments = comments;
            detail.OrderDetailId = orderDetailId;
            detail.Price = price;
            detail.Persisted = true;
            if (parts.Length > 5)
            {
                int sourceIt = Convert.ToInt32(parts[5]);
                detail.FromOffer = sourceIt > 0;
            }
            if (parts.Length > 6)
            {
                detail.Lot = parts[6];
                if (!string.IsNullOrEmpty(detail.Lot) && !detail.Product.Lots.Contains(detail.Lot))
                    detail.Product.AddLot(detail.Lot);
            }
            if (parts.Length > 7)
                detail.Damaged = Convert.ToInt32(parts[7]) > 0;

            if (parts.Length > 8)
                detail.Ordered = Convert.ToSingle(parts[8]);

            if (parts.Length > 9)
                detail.OriginalId = parts[9];

            if (parts.Length > 10)
                detail.Substracted = Convert.ToInt32(parts[10]) > 0;

            if (parts.Length > 11)
                detail.IsCredit = Convert.ToInt32(parts[11]) > 0;

            if (parts.Length > 12)
                detail.ExpectedPrice = Convert.ToDouble(parts[12]);

            if (parts.Length > 13)
            {
                var uomId = Convert.ToInt32(parts[13]);
                detail.UnitOfMeasure = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);

                //in case uom was inactivated
                if (uomId != 0 && detail.UnitOfMeasure == null)
                    detail.UnitOfMeasure = UnitOfMeasure.InactiveUoM.FirstOrDefault(x => x.Id == uomId);
            }

            if (parts.Length > 14)
                detail.Deleted = Convert.ToInt32(parts[14]) > 0;

            if (parts.Length > 15)
                detail.Weight = Convert.ToSingle(parts[15]);

            if (parts.Length > 16)
                detail.RelatedOrderDetail = Convert.ToInt32(parts[16]);

            if (parts.Length > 17)
                detail.ConsignmentNew = Convert.ToSingle(parts[17]);

            if (parts.Length > 18)
                detail.ConsignmentOld = Convert.ToSingle(parts[18]);

            if (parts.Length > 19)
                detail.ConsignmentSet = Convert.ToInt32(parts[19]) > 0;

            if (parts.Length > 20)
                detail.ConsignmentCounted = Convert.ToInt32(parts[20]) > 0;

            if (parts.Length > 21)
                detail.ConsignmentUpdated = Convert.ToInt32(parts[21]) > 0;

            if (parts.Length > 22)
                detail.ConsignmentNewPrice = Convert.ToDouble(parts[22]);

            if (parts.Length > 23)
                detail.ConsignmentCount = Convert.ToSingle(parts[23]);

            if (parts.Length > 24)
                detail.ExtraFields = parts[24];

            if (parts.Length > 25)
                detail.DeliveryScanningChecked = Convert.ToInt32(parts[25]) > 0;

            if (parts.Length > 26)
                detail.Taxed = Convert.ToInt32(parts[26]) > 0;
            else
                detail.Taxed = detail.Product.Taxable;

            if (parts.Length > 27)
                detail.ConsignmentSalesItem = Convert.ToInt32(parts[27]) > 0;

            if (parts.Length > 28)
                detail.OfferDetFreeItem = Convert.ToInt32(parts[28]);

            if (parts.Length > 29)
                detail.LoadStarting = Convert.ToSingle(parts[29]);

            if (parts.Length > 30)
                detail.Allowance = Convert.ToDouble(parts[30]);

            if (parts.Length > 31)
                detail.FromOfferPrice = Convert.ToBoolean(parts[31]);

            if (parts.Length > 32)
                detail.TaxRate = Convert.ToDouble(parts[32]);

            if (parts.Length > 33)
                detail.Discount = Convert.ToDouble(parts[33]);

            if (parts.Length > 34)
            {
                int dType = 0;

                if (!int.TryParse(parts[34], out dType))
                    Logger.CreateLog("OrderDetail discountType=" + parts[34]);

                detail.DiscountType = (DiscountType)dType;
            }

            if (parts.Length > 35)
                detail.ConsignmentPicked = Convert.ToSingle(parts[35]);

            if (parts.Length > 36)
                detail.ConsignmentCreditItem = Convert.ToInt32(parts[36]) > 0;

            if (parts.Length > 37)
                detail.LoadConsLots(parts[37], detail.ConsignmentCountedLots);

            if (parts.Length > 38)
                detail.LoadConsLots(parts[38], detail.ConsignmentPickedLots);

            if (parts.Length > 39)
                detail.ConsignmentComment = parts[39];

            if (parts.Length > 40)
                detail.ParLevelDetail = Convert.ToInt32(parts[40]) > 0;

            if (parts.Length > 41)
            {
                var originalUoM = Convert.ToInt32(parts[41]);
                detail.OriginalUoM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == originalUoM);
            }

            if (parts.Length > 42)
                detail.LoadDeliveryQty(parts[42]);

            if (parts.Length > 43)
                detail.Id = Convert.ToInt32(parts[43]);

            if (parts.Length > 44)
                detail.ReasonId = Convert.ToInt32(parts[44]);

            if (parts.Length > 45)
                detail.FromOfferType = Convert.ToInt32(parts[45]);

            if (parts.Length > 46)
                detail.HiddenItem = Convert.ToInt32(parts[46]) > 0;

            if (parts.Length > 47)
                detail.AdjustmentItem = Convert.ToInt32(parts[47]) > 0;

            if (parts.Length > 48)
                detail.LabelUniqueId = parts[48];

            if (parts.Length > 49)
                detail.ProductDepartment = parts[49];

            if (parts.Length > 50)
                detail.IsFreeItem = Convert.ToInt32(parts[50]) > 0;

            if (parts.Length > 51)
                detail.LoadingError = Convert.ToInt32(parts[51]) > 0;

            if (parts.Length > 52)
                detail.LotExpiration = new DateTime(Convert.ToInt64(parts[52]));

            if (parts.Length > 53)
                detail.CompletedFromScanner = Convert.ToInt32(parts[53]) > 0;

            if (parts.Length > 54)
                detail.ListPrice = Convert.ToDouble(parts[54]);

            if (parts.Length > 55)
                detail.WeightEntered = Convert.ToInt32(parts[55]) > 0;

            if (parts.Length > 56)
                detail.isMixAndMatchRelated = Convert.ToInt32(parts[56]) > 0;

            if (parts.Length > 57)
                detail.PriceLevelSelected = Convert.ToInt32(parts[57]);

            if (parts.Length > 58)
                detail.ScannedQty = Convert.ToInt32(parts[58]);

            if (parts.Length > 59)
                detail.OrderDiscountId = Convert.ToInt32(parts[59]);

            if (parts.Length > 60)
                detail.OrderDiscountBreakId = Convert.ToInt32(parts[60]);

            if (parts.Length > 61)
                detail.CostDiscount = Convert.ToDouble(parts[61]);

            if (parts.Length > 62)
                detail.CostPrice = Convert.ToDouble(parts[62]);

            if (parts.Length > 63)
                detail.ExtraComments = parts[63];

            if (parts.Length > 64)
                detail.ModifiedManually = Convert.ToInt32(parts[64]) > 0;

            if (parts.Length > 65)
                detail.IgnoreInOffers = Convert.ToInt32(parts[65]) > 0;

            if (parts.Length > 66)
                detail.AlreadyAskedForOffers = Convert.ToInt32(parts[66]) > 0;

            detail.Order = this;
            source.Add(detail);
        }

      public string ConvertSignatureToBitmap()
        {
            if (SignaturePoints == null || SignaturePoints.Count == 0)
                return null;

            // Calculate bounding box
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var point in SignaturePoints)
            {
                if (point.X < minX) minX = (float)point.X;
                if (point.Y < minY) minY = (float)point.Y;
                if (point.X > maxX) maxX = (float)point.X;
                if (point.Y > maxY) maxY = (float)point.Y;
            }

            // Add padding
            float padding = 10;
            minX -= padding;
            minY -= padding;
            maxX += padding;
            maxY += padding;

            // Create bitmap dimensions
            int width = (int)Math.Ceiling(maxX - minX);
            int height = (int)Math.Ceiling(maxY - minY);

            // Create an SKBitmap and an SKCanvas to draw on
            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2,
                    IsAntialias = true
                };

                // Create paths and draw lines
                SKPath path = null;
                bool isDrawingPath = false;

                foreach (var point in SignaturePoints)
                {
                    if (point == SixLabors.ImageSharp.Point.Empty) // End of line
                    {
                        if (isDrawingPath)
                        {
                            canvas.DrawPath(path, paint);
                            path.Dispose();
                            isDrawingPath = false;
                        }
                    }
                    else
                    {
                        if (!isDrawingPath)
                        {
                            path = new SKPath();
                            path.MoveTo((float)point.X - minX, (float)point.Y - minY);
                            isDrawingPath = true;
                        }
                        else
                        {
                            path.LineTo((float)point.X - minX, (float)point.Y - minY);
                        }
                    }
                }

                // Draw the last path if there is any remaining
                if (isDrawingPath)
                {
                    canvas.DrawPath(path, paint);
                    path.Dispose();
                }

                // Handle individual dots
                foreach (var point in SignaturePoints)
                {
                    if (point != SixLabors.ImageSharp.Point.Empty && SignaturePoints.Count(p => p.X == point.X && p.Y == point.Y) == 1)
                    {
                        canvas.DrawCircle((float)point.X - minX, (float)point.Y - minY, paint.StrokeWidth / 2, paint);
                    }
                }

                // Save the bitmap to a file
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    var filePath = Path.Combine(System.IO.Path.GetTempFileName(), "signature.png");
                    using (var stream = File.OpenWrite(filePath))
                    {
                        data.SaveTo(stream);
                    }
                    return filePath;
                }
            }
        }

        public string SerializeSignatureAsString(List<SixLabors.ImageSharp.Point> SignaturePoints)
        {

            StringBuilder pointsString = new StringBuilder();
            if (SignaturePoints != null)
                foreach (Point p in SignaturePoints)
                {
                    if (pointsString.Length > 0)
                        pointsString.Append(";");
                    pointsString.Append(p.X.ToString());
                    pointsString.Append(",");
                    pointsString.Append(p.Y.ToString());
                }
            string s = pointsString.ToString();

            return s;
        }

        public static void AddCompanyToHummerCustom(ref StringBuilder sb, Client client)
        {
            // ROUTE SECTION
            sb.Append("ROUTE ");
            sb.Append(Config.SalesmanId.ToString());
            sb.Append((char)10);

            sb.Append("NAME ");
            sb.Append("\"");
            sb.Append("Hummer & Son's Honey Farm");
            sb.Append("\"");
            sb.Append((char)10);

            sb.Append("DUNS ");
            sb.Append("003400118");
            sb.Append((char)10);

            sb.Append("LOCATION ");
            sb.Append("287 Sligo Rd Bossier City, LA 71112");
            sb.Append((char)10);

            sb.Append("COMM_ID ");
            sb.Append("3186702561");
            sb.Append((char)10);

            // STOP SECTION
            sb.Append("STOP 01");
            sb.Append((char)10);

            sb.Append("CUST_DUNS ");
            sb.Append(client.DUNS ?? "1234567890");
            sb.Append((char)10);

            sb.Append("CUST_LOCATION ");
            sb.Append(client.Location ?? "123456");
            sb.Append((char)10);
        }


        public static string DexUs(List<Order> orders, string defaultUoM = null)
        {
            var client = orders[0].Client;
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(orders[0].ExtraFields) && orders[0].ExtraFields.ToLower().Contains("split=1"))
                AddCompanyToHummerCustom(ref sb, client);
            else
            {
                // ROUTE SECTION
                sb.Append("ROUTE ");
                sb.Append(Config.SalesmanId.ToString());
                sb.Append((char)10);

                sb.Append("NAME ");
                sb.Append("\"");
                sb.Append(CompanyInfo.SelectedCompany.CompanyName);
                sb.Append("\"");
                sb.Append((char)10);

                sb.Append("DUNS ");
                sb.Append(CompanyInfo.SelectedCompany.DUNS);
                sb.Append((char)10);

                sb.Append("LOCATION ");
                sb.Append(CompanyInfo.SelectedCompany.Location ?? "123456");
                sb.Append((char)10);

                sb.Append("COMM_ID ");
                sb.Append(CompanyInfo.SelectedCompany.CommId);
                sb.Append((char)10);

                // STOP SECTION
                sb.Append("STOP 01");
                sb.Append((char)10);

                sb.Append("CUST_DUNS ");
                sb.Append(client.DUNS ?? "1234567890");
                sb.Append((char)10);

                sb.Append("CUST_LOCATION ");
                sb.Append(client.Location ?? "123456");
                sb.Append((char)10);
            }

            string dexversion = null;

            if (orders.Count > 0 && !string.IsNullOrEmpty(orders[0].Client.NonvisibleExtraPropertiesAsString) && orders[0].Client.NonvisibleExtraPropertiesAsString.Contains("DEX_VERSION"))
            {
                var v = DataAccess.ExplodeExtraProperties(orders[0].Client.NonvisibleExtraPropertiesAsString).FirstOrDefault(x => x.Key == "DEX_VERSION");
                if (v != null)
                {
                    dexversion = v.Value;
                    sb.Append("DEX_VERSION " + v.Value);
                    sb.Append((char)10);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Config.DexVersion))
                {
                    dexversion = Config.DexVersion;
                    sb.Append("DEX_VERSION " + Config.DexVersion);
                    sb.Append((char)10);
                }
            }

            if (!string.IsNullOrEmpty(client.CommId))
            {
                sb.Append("CUST_COMM_ID ");
                sb.Append(client.CommId);
                sb.Append((char)10);
            }

            if (orders.Count > 0 && !string.IsNullOrEmpty(orders[0].Client.NonvisibleExtraPropertiesAsString) && orders[0].Client.NonvisibleExtraPropertiesAsString.Contains("DEX_PROMPT_BEFORE_ACK"))
            {
                sb.Append("PROMPT_BEFORE_ACK YES");
                sb.Append((char)10);
            }
            foreach (var order in orders)
            {
                // INVOICE SECTION
                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                    order.Save();
                }
                sb.Append("INVOICE ");
                sb.Append(order.PrintedOrderId);
                sb.Append((char)10);

                if (order.OrderType == OrderType.Order)
                    sb.Append("ORDER_TYPE DELIVERY");
                else
                    sb.Append("ORDER_TYPE RETURN");
                sb.Append((char)10);

                // ITEM SECTION

                List<OrderDetail> GroupedDetails = new List<OrderDetail>();
                foreach (var detail in order.Details)
                {
                    try
                    {
                        //multiple lots dex
                        int index = -1;
                        if (detail.UnitOfMeasure != null)
                            index = GroupedDetails.FindIndex(x => x.Product.ProductId == detail.Product.ProductId && x.UnitOfMeasure.Id == detail.UnitOfMeasure.Id && x.Price == detail.Price && x.Lot == detail.Lot);
                        else
                            index = GroupedDetails.FindIndex(x => x.Product.ProductId == detail.Product.ProductId && x.UnitOfMeasure == null && x.Price == detail.Price && x.Lot == detail.Lot);

                        if (index == -1)
                            GroupedDetails.Add(detail);
                        else
                        {
                            GroupedDetails[index].Qty += detail.Qty;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }

                var relatedIds = order.details.Where(x => x.RelatedOrderDetail > 0).Select(x => x.RelatedOrderDetail).ToList();
                foreach (var detail in GroupedDetails)
                {
                    if (relatedIds.Contains(detail.OrderDetailId))
                    {
                        Logger.CreateLog("The detail " + detail.OriginalId + " is a related, ignoring in DEX");
                        continue;
                    }
                    if (detail.Qty == 0)
                    {
                        Logger.CreateLog("The detail " + detail.OriginalId + " is qty 0, ignoring in DEX");
                        continue;
                    }
                    sb.Append("PRODUCT ");
                    sb.Append(detail.Product.ProductId.ToString());
                    sb.Append((char)10);

                    //if (dexversion == "4010")
                    //{
                    //    sb.Append(string.Format("UPC {0}", detail.Product.Upc));
                    //    sb.Append((char)10);

                    //}
                    //else
                    //{
                    if (detail.Product.Upc.Length > 12)
                    {
                        if (!string.IsNullOrEmpty(Config.DexDefaultUnit) && detail.Product.Package != "1" && order.OrderType == OrderType.Order)
                        {
                            //sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, detail.UnitOfMeasure.Conversion));
                            if (detail.UnitOfMeasure.ExtraFields != null && detail.UnitOfMeasure.ExtraFields.Contains("georgehowe"))
                            {
                                var conv = detail.UnitOfMeasure.ExtraFields.Replace("georgehowe=", "");
                                sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, conv));
                            }
                            else
                            {
                                var defaultConversion = detail.UnitOfMeasure.Conversion.ToString();

                                if (Config.DontIncludePackageParameterDexUpc)
                                    defaultConversion = string.Empty;

                                sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, defaultConversion));
                            }
                            sb.Append((char)10);
                        }
                        else
                        {
                            sb.Append("GTIN ");
                            sb.Append(string.IsNullOrEmpty(detail.Product.Upc) ? "1234345345" : detail.Product.Upc);
                            sb.Append((char)10);
                        }
                    }
                    else
                    {
                        if (detail.UnitOfMeasure != null)
                        {
                            //sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, detail.UnitOfMeasure.Conversion));
                            //sb.Append((char)10);
                            if (detail.UnitOfMeasure.ExtraFields != null && detail.UnitOfMeasure.ExtraFields.Contains("georgehowe"))
                            {
                                var conv = detail.UnitOfMeasure.ExtraFields.Replace("georgehowe=", "");
                                sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, conv));
                            }
                            else
                            {
                                var defaultConversion = detail.UnitOfMeasure.Conversion.ToString();

                                if (Config.DontIncludePackageParameterDexUpc)
                                    defaultConversion = string.Empty;

                                sb.Append(string.Format("UPC {0} {1} {2}", detail.Product.Upc, detail.Product.Upc, defaultConversion));
                            }
                            sb.Append((char)10);
                        }
                        else
                            if ((!string.IsNullOrEmpty(Config.DexDefaultUnit) || defaultUoM != null) && order.OrderType == OrderType.Order)
                        {
                            var defaultPackage = "1";

                            if (Config.DontIncludePackageParameterDexUpc)
                                defaultPackage = string.Empty;

                            sb.Append(string.Format("UPC {0} {1} {2} {3}", detail.Product.Upc, detail.Product.Upc, detail.Product.Package, defaultPackage));
                            sb.Append((char)10);
                        }
                        else
                        {
                            sb.Append("UPC ");
                            sb.Append(string.IsNullOrEmpty(detail.Product.Upc) ? "1234345345" : detail.Product.Upc);
                            sb.Append((char)10);
                        }
                    }
                    //}
                    string s = detail.Product.Name;
                    if (s.Length > 18)
                        s = s.Substring(0, 18);
                    sb.Append("DESC \"");
                    sb.Append(s);
                    sb.Append("\"");
                    sb.Append((char)10);

                    if (!order.AsPresale && detail.Product.SoldByWeight)
                    {
                        sb.Append("PACKTYPE ");
                        sb.Append("LB");
                        sb.Append((char)10);
                    }
                    else
                    {

                        if (detail.UnitOfMeasure != null)
                        {
                            var conversion = detail.UnitOfMeasure.Conversion;
                            if (!string.IsNullOrEmpty(detail.UnitOfMeasure.ExtraFields) && detail.UnitOfMeasure.ExtraFields.Contains("georgehowe"))
                            {
                                conversion = Convert.ToSingle(detail.UnitOfMeasure.ExtraFields.Replace("georgehowe=", ""));
                            }
                            if (conversion > 1)
                                sb.Append("PACKTYPE CA");
                            else
                                sb.Append("PACKTYPE EA");

                            sb.Append((char)10);
                        }
                        else
                        if (!string.IsNullOrEmpty(Config.DexDefaultUnit) && order.OrderType == OrderType.Order)
                        {
                            sb.Append("PACKTYPE ");
                            sb.Append(Config.DexDefaultUnit);
                            sb.Append((char)10);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(defaultUoM))
                            {
                                sb.Append("PACKTYPE ");
                                sb.Append(defaultUoM);
                                sb.Append((char)10);
                            }
                        }
                    }

                    sb.Append("QUANT ");
                    if (order.AsPresale || !detail.Product.SoldByWeight)
                        sb.Append(detail.Qty.ToString());
                    else
                        sb.Append(detail.Weight.ToString());
                    sb.Append((char)10);

                    double price = Math.Round(detail.Price, 2);
                    if (detail.DexPrice > 0)
                        price = detail.DexPrice;
                    //if (detail.Allowance > 0)
                    //    price += detail.Allowance;
                    sb.Append("PRICE ");

                    if (Config.AddAllowanceToPriceDuringDEX && detail.Allowance > 0)
                        sb.Append(Math.Round(price + detail.Allowance, 2).ToString());
                    else
                        sb.Append(price.ToString());

                    sb.Append((char)10);

                    if (detail.RelatedOrderDetail > 0)
                    {
                        var relatedDetail = order.Details.FirstOrDefault(x => x.OrderDetailId == detail.RelatedOrderDetail);
                        if (relatedDetail != null)
                        {
                            sb.Append("ADJUSTMENT C 999 02 ");
                            sb.Append(CompanyInfo.SelectedCompany.DUNS);
                            sb.Append(" $ ");
                            sb.Append(relatedDetail.Price.ToString());
                            sb.Append("/");
                            sb.Append(relatedDetail.Qty.ToString());
                            sb.Append((char)10);
                        }
                    }
                    if (detail.Allowance > 0)
                    {
                        sb.Append("ADJUSTMENT A 47 02 ");
                        sb.Append(CompanyInfo.SelectedCompany.DUNS);
                        sb.Append(" $ ");
                        sb.Append(detail.Allowance.ToString());
                        sb.Append("/");
                        sb.Append(detail.Qty.ToString());
                        sb.Append((char)10);
                    }
                    else
                        Logger.CreateLog("no allowance");
                }
            }
            Logger.CreateLog(sb.ToString());
            return sb.ToString();
        }

        //public static string DexUsII_(List<Order> orders, string defaultUoM = null)
        //{
        //    // var client = orders[0].Client;
        //    StringBuilder sb = new StringBuilder();

        //    // ROUTE SECTION
        //    sb.Append("ROUTE ");
        //    sb.Append(Config.SalesmanId.ToString());
        //    sb.Append((char)10);

        //    sb.Append("NAME ");
        //    sb.Append("\"");
        //    sb.Append(CompanyInfo.SelectedCompany.CompanyName);
        //    sb.Append("\"");
        //    sb.Append((char)10);

        //    sb.Append("DUNS ");
        //    sb.Append(CompanyInfo.SelectedCompany.DUNS);
        //    sb.Append((char)10);

        //    sb.Append("LOCATION ");
        //    sb.Append(CompanyInfo.SelectedCompany.Location ?? "123456");
        //    sb.Append((char)10);

        //    sb.Append("COMM_ID ");
        //    sb.Append(CompanyInfo.SelectedCompany.CommId);
        //    sb.Append((char)10);

        //    // STOP SECTION
        //    sb.Append("STOP 01");
        //    sb.Append((char)10);

        //    foreach (var order in orders)
        //    {
        //        var client = order.Client;
        //        sb.Append("CUST_DUNS ");
        //        sb.Append(client.DUNS ?? "1234567890");
        //        sb.Append((char)10);

        //        sb.Append("CUST_LOCATION ");
        //        sb.Append(client.Location ?? "123456");
        //        sb.Append((char)10);

        //        string dexversion = null;

        //        if (!string.IsNullOrEmpty(client.NonvisibleExtraPropertiesAsString) && client.NonvisibleExtraPropertiesAsString.Contains("DEX_VERSION"))
        //        {
        //            var v = DataAccess.ExplodeExtraProperties(client.NonvisibleExtraPropertiesAsString).FirstOrDefault(x => x.Key == "DEX_VERSION");
        //            if (v != null)
        //            {
        //                dexversion = v.Value;
        //                sb.Append("DEX_VERSION " + v.Value);
        //                sb.Append((char)10);
        //            }
        //        }
        //        else
        //        {
        //            if (!string.IsNullOrEmpty(Config.DexVersion))
        //            {
        //                dexversion = Config.DexVersion;
        //                sb.Append("DEX_VERSION " + Config.DexVersion);
        //                sb.Append((char)10);
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(client.CommId))
        //        {
        //            sb.Append("CUST_COMM_ID ");
        //            sb.Append(client.CommId);
        //            sb.Append((char)10);
        //        }

        //        if (!string.IsNullOrEmpty(client.NonvisibleExtraPropertiesAsString) && client.NonvisibleExtraPropertiesAsString.Contains("DEX_PROMPT_BEFORE_ACK"))
        //        {
        //            sb.Append("PROMPT_BEFORE_ACK YES");
        //            sb.Append((char)10);
        //        }
        //        DexOneOrder(order, sb, defaultUoM);
        //    }
        //    Logger.CreateLog(sb.ToString());
        //    return sb.ToString();
        //}

        public string AddOffer(Offer offer)
        {
            if (orderType != OrderType.Order)
                return "Invalid operation. Offers can only be added to orders";
            UnitOfMeasure uom = null;
            if (offer.UnitOfMeasureId > 0)
                uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);

            OrderDetail det = null;

            switch (offer.Type)
            {
                case OfferType.NewItem:
                    if (!Config.CanGoBelow0 && Config.TrackInventory && offer.Product.CurrentInventory < 1)
                        return "Not enought inventory of this product";
                    var price = Product.GetPriceForProduct(offer.Product, Client, true);

                    det = new OrderDetail(offer.Product, 1, this)
                    {
                        Price = price,
                        ExpectedPrice = price,
                        FromOffer = true,
                        UnitOfMeasure = uom,
                        FromOfferType = (int)offer.Type
                    };

                    AddDetail(det);

                    break;
                case OfferType.Price:
                    if (!Config.CanGoBelow0 && Config.TrackInventory && offer.Product.CurrentInventory < 1)
                        return "Not enought inventory of this product";

                    det = new OrderDetail(offer.Product, 1, this)
                    {
                        Price = offer.Price,
                        UnitOfMeasure = uom,
                        FromOffer = true,
                        FromOfferType = (int)offer.Type,
                        ExpectedPrice = offer.Price
                    };

                    if (Config.OffersAddComment)
                        det.Comments = "Offer: " + offer.Price.ToCustomString();

                    AddDetail(det);

                    break;
                case OfferType.QtyPrice:
                    if (!Config.CanGoBelow0 && Config.TrackInventory && offer.Product.CurrentInventory < offer.MinimunQty)
                        return "Not enought inventory of this product to include this offer";

                    det = new OrderDetail(offer.Product, offer.MinimunQty, this)
                    {
                        Price = offer.Price,
                        ExpectedPrice = offer.Price,
                        FromOffer = true,
                        FromOfferType = (int)offer.Type,
                        UnitOfMeasure = uom
                    };

                    if (Config.OffersAddComment)
                        det.Comments = "Offer: Buy" + offer.MinimunQty.ToString(CultureInfo.CurrentCulture) + " at " +
                                       offer.Price.ToCustomString();

                    AddDetail(det);

                    break;
                case OfferType.QtyQty:
                    if (!Config.CanGoBelow0 && Config.TrackInventory && offer.Product.CurrentInventory < offer.MinimunQty + offer.FreeQty)
                        return "Not enought inventory of this product to include this offer";
                    var price2 = Product.GetPriceForProduct(offer.Product, Client, true);

                    det = new OrderDetail(offer.Product, offer.MinimunQty, this)
                    {
                        Price = price2,
                        FromOffer = true,
                        FromOfferType = (int)offer.Type,
                        UnitOfMeasure = uom,
                        ExpectedPrice = price2
                    };

                    if (Config.OffersAddComment)
                        det.Comments = Comments = "Offer: buy " +
                                                  offer.MinimunQty.ToString(CultureInfo.CurrentCulture) + " get " +
                                                  offer.FreeQty.ToString(CultureInfo.CurrentCulture) +
                                                  " free  (Starting Qty)";

                    AddDetail(det);

                    UpdateInventory(det, -1);

                    det = new OrderDetail(offer.Product, offer.FreeQty, this)
                    {
                        Price = 0,
                        FromOffer = true,
                        FromOfferType = (int)offer.Type,
                        ExpectedPrice = 0,
                        UnitOfMeasure = uom
                    };

                    if (Config.OffersAddComment)
                        Comments = "Offer: buy " + offer.MinimunQty.ToString(CultureInfo.CurrentCulture) + " get " +
                                   offer.FreeQty.ToString(CultureInfo.CurrentCulture) + " free  (free Qty)";

                    AddDetail(det);

                    break;
            }

            if (det != null)
                UpdateInventory(det, -1);

            if (Config.TrackInventory)
                DataAccess.SaveInventory();

            Save();

            return string.Empty;
        }

        public bool HasPaymentApplied()
        {
            foreach (var p in InvoicePayment.List)
            {
                if (!string.IsNullOrEmpty(p.OrderId) && p.OrderId == this.UniqueId)
                    return true;
            }
            return false;
        }

        public static Order DuplicateorderHeader(Order order)
        {
            var newOrder = new Order();
            newOrder.OrderId = order.OrderId;
            newOrder.OriginalOrderId = order.OrderId;
            newOrder.SalesmanId = order.SalesmanId;
            newOrder.Date = order.Date;
            newOrder.OrderType = order.orderType;
            newOrder.Comments = order.Comments;
            newOrder.PrintedOrderId = order.PrintedOrderId;
            newOrder.Client = order.Client;
            newOrder.Longitude = order.Longitude;
            newOrder.Latitude = order.Latitude;
            newOrder.SignaturePoints = order.SignaturePoints;
            newOrder.UniqueId = order.UniqueId;
            newOrder.SignatureName = order.SignatureName;
            newOrder.SignatureUniqueId = order.SignatureUniqueId;
            newOrder.TaxRate = order.TaxRate;
            newOrder.DiscountType = order.DiscountType;
            newOrder.DiscountAmount = order.DiscountAmount;
            newOrder.DiscountComment = order.DiscountComment;
            newOrder.Voided = order.Voided;
            newOrder.PONumber = order.PONumber;
            newOrder.EndDate = order.EndDate;
            newOrder.ShipDate = order.ShipDate;
            newOrder.BatchId = order.BatchId;
            newOrder.Dexed = order.Dexed;
            newOrder.Finished = order.Finished;
            newOrder.CompanyName = order.CompanyName;
            newOrder.PrintedCopies = order.PrintedCopies;
            newOrder.AsPresale = order.AsPresale;
            newOrder.Reshipped = order.Reshipped;
            newOrder.ReshipDate = order.ReshipDate;
            newOrder.ExtraFields = order.ExtraFields;
            newOrder.OriginalSalesmanId = order.OriginalSalesmanId;
            newOrder.ReasonId = order.ReasonId;
            newOrder.Modified = order.Modified;
            newOrder.IsParLevel = order.IsParLevel;
            newOrder.ImageList = new List<string>(order.ImageList);
            newOrder.IsQuote = order.IsQuote;
            newOrder.FromInvoiceId = order.FromInvoiceId;
            newOrder.IsProjection = order.IsProjection;
            newOrder.RelationUniqueId = order.RelationUniqueId;
            newOrder.IsScanBasedTrading = order.IsScanBasedTrading;
            newOrder.SplitedByDepartment = order.SplitedByDepartment;
            newOrder.DepartmentUniqueId = order.DepartmentUniqueId;
            newOrder.Department = order.Department;
            newOrder.LoadingError = order.LoadingError;
            newOrder.CompanyId = order.CompanyId;
            newOrder.DepartmentId = order.DepartmentId;
            newOrder.AssetId = order.AssetId;
            newOrder.IsExchange = order.IsExchange;
            newOrder.Freight = order.Freight;
            newOrder.OtherCharges = order.OtherCharges;
            newOrder.FreightType = order.FreightType;
            newOrder.OtherChargesType = order.OtherChargesType;
            newOrder.OtherChargesComment = order.OtherChargesComment;
            newOrder.FreightComment = order.FreightComment;
            return newOrder;
        }

        public static Order CreateEmptyOrder()
        {
            return new Order();
        }

        public bool IsParLevel { get; set; }

        public Order GetOrderCopy()
        {
            var newOrder = DuplicateorderHeader(this);

            newOrder.PrintedOrderId = string.Empty;
            newOrder.Date = DateTime.Now;
            newOrder.EndDate = DateTime.MinValue;
            newOrder.OrderId = ++LastOrderId;
            newOrder.UniqueId = Guid.NewGuid().ToString("N");

            orders.Add(newOrder);

            ImageList = new List<string>();

            return newOrder;
        }

        public static Order VoidAndClone(Order order, int batchId = 0)
        {
            var newOrder = order.GetOrderCopy();

            if (batchId > 0)
                newOrder.BatchId = batchId;

            foreach (var item in order.details)
            {
                var newDetail = item.GetOrderDetailCopy();
                newOrder.AddDetail(newDetail);

                if (Config.AllowMultParInvoices)
                {
                    var par = ClientDailyParLevel.List.FirstOrDefault(x => x.ClientId == order.Client.ClientId &&
                    x.Product.ProductId == item.Product.ProductId && x.MatchDayOfWeek(order.Date.DayOfWeek));
                    if (par != null)
                        par.OrderId = newOrder.OrderId;
                }

                if (!order.Reshipped)
                {
                    item.Price = 0;
                    item.ExpectedPrice = 0;
                    item.ConsignmentCounted = item.ConsignmentSet = item.ConsignmentUpdated = false;
                }

            }

            order.Voided = true;
            order.Finished = true;
            order.DiscountAmount = 0;
            order.DiscountComment = string.Empty;
            order.IsParLevel = false;
            order.Save();

            newOrder.Save();

            return newOrder;
        }

        public double _paid = 0;

        public double Paid
        {
            get
            {
                if (ConvertedInvoice)
                {
                    return _paid;
                }

                var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(UniqueId));
                if (payment == null)
                    return 0;

                double amount = 0;
                foreach (var item in payment.Components)
                    amount += item.Amount;

                return double.Parse(Math.Round(amount, Config.Round).ToCustomString(), NumberStyles.Currency);
            }
        }

        public double TotalWeight
        {
            get
            {
                double totalWeight = 0;
                foreach (var item in Details)
                    totalWeight += item.Product.Weight * item.Qty;

                return totalWeight;
            }
        }

        public void ConvertConsignmentPar()
        {
            List<OrderDetail> newDetails = new List<OrderDetail>();

            foreach (var item in Details)
            {
                var st = ConsStruct.GetStructFromDetail(item);

                if (!st.FromPar)
                {
                    var detail = new OrderDetail(item.Product, 0, this);
                    detail.Order = this;
                    detail.Qty = st.Sold;
                    detail.ConsignmentOld = st.OldValue;
                    detail.ConsignmentNew = st.Updated ? st.NewValue : st.OldValue;
                    detail.Price = st.Price;
                    detail.ConsignmentNewPrice = st.NewPrice;
                    detail.ConsignmentCount = st.Count;
                    detail.ConsignmentPicked = st.Picked;
                    detail.ConsignmentSet = st.Set;
                    detail.ConsignmentUpdated = st.Updated;
                    detail.ConsignmentCounted = st.Counted;
                    detail.ExtraFields = item.ExtraFields;

                    newDetails.Add(detail);
                }
                else
                {
                    var parId = DataAccess.GetSingleUDF("parid", item.ExtraFields);
                    if (!string.IsNullOrEmpty(parId))
                    {
                        var par = ClientDailyParLevel.List.FirstOrDefault(x => x.Id == Convert.ToInt32(parId));
                        if (par == null)
                            par = ClientDailyParLevel.GetParLevel(Client, item.Product, DateTime.Now.DayOfWeek);

                        CreateEditParLevel(par, item);
                    }
                    else
                        CreateEditParLevel(null, item);

                    if (st.Sold > 0)
                    {
                        var detail = new OrderDetail(item.Product, 0, this);
                        detail.Order = this;
                        detail.Qty = st.Sold;
                        detail.Price = st.Price;
                        detail.ExtraFields = DataAccess.SyncSingleUDF("frompar", "1", detail.ExtraFields);
                        detail.ConsignmentSalesItem = true;
                        detail.ConsignmentSet = true;
                        detail.ConsignmentCounted = true;
                        detail.ParLevelDetail = true;
                        detail.ExtraFields = item.ExtraFields;

                        newDetails.Add(detail);
                    }
                    if (st.Damaged > 0)
                    {
                        var detail = new OrderDetail(item.Product, 0, this);
                        detail.Order = this;
                        detail.Qty = st.Damaged;
                        detail.IsCredit = true;
                        detail.Damaged = true;
                        detail.Price = st.Price;
                        detail.ExtraFields = DataAccess.SyncSingleUDF("frompar", "1", detail.ExtraFields);
                        detail.ConsignmentCreditItem = true;
                        detail.ConsignmentSet = true;
                        detail.ConsignmentCounted = true;
                        detail.ParLevelDetail = true;

                        if (st.Sold == 0)
                            detail.ExtraFields = item.ExtraFields;

                        newDetails.Add(detail);
                    }
                    if (st.Return > 0)
                    {
                        var detail = new OrderDetail(item.Product, 0, this);
                        detail.Order = this;
                        detail.Qty = st.Return;
                        detail.IsCredit = true;
                        detail.Damaged = false;
                        detail.Price = st.Price;
                        detail.ExtraFields = DataAccess.SyncSingleUDF("frompar", "1", detail.ExtraFields);
                        detail.ConsignmentCreditItem = true;
                        detail.ConsignmentSet = true;
                        detail.ConsignmentCounted = true;
                        detail.ParLevelDetail = true;

                        if (st.Sold == 0 && st.Damaged == 0)
                            detail.ExtraFields = item.ExtraFields;

                        newDetails.Add(detail);
                    }

                    if (st.Sold == 0 && st.Damaged == 0 && st.Return == 0)
                    {
                        item.Qty = 0;
                        item.ConsignmentSalesItem = true;
                        item.ConsignmentSet = true;
                        item.ConsignmentCounted = true;
                        item.ParLevelDetail = true;
                        newDetails.Add(item);
                    }
                }
            }

            details = new List<OrderDetail>(newDetails);
            OrderType = OrderType.Consignment;
            ExtraFields = DataAccess.SyncSingleUDF("ConsignmentCount", "1", ExtraFields);
            Save();
        }

        void CreateEditParLevel(ClientDailyParLevel par, OrderDetail det)
        {
            if (par == null)
            {
                par = ClientDailyParLevel.GetNewParLevel(Client, det.Product, 0);
            }

            var newvalue = DataAccess.GetSingleUDF("newvalue", det.ExtraFields);
            par.SetNewPar(Convert.ToSingle(newvalue));

            var counted = DataAccess.GetSingleUDF("count", det.ExtraFields);
            par.SetCountedQty(Convert.ToSingle(counted));

            var sold = DataAccess.GetSingleUDF("sold", det.ExtraFields);
            par.SetSoldQty(Convert.ToSingle(sold));

            var returns = DataAccess.GetSingleUDF("return", det.ExtraFields);
            var dumps = DataAccess.GetSingleUDF("damaged", det.ExtraFields);
            par.SetReturnQty(Convert.ToSingle(returns));
            par.SetDumpQty(Convert.ToSingle(dumps));

        }

        public Tuple<string, string> GetWarrantyPerClient(Product prod)
        {
            var categories = DataAccess.GetSingleUDF("Warranty", Client.NonvisibleExtraPropertiesAsString);

            if (!string.IsNullOrEmpty(categories))
            {
                var cats = categories.Split('/');

                List<Tuple<string, string>> catsItems = new List<Tuple<string, string>>();
                foreach (var item in cats)
                {
                    var p = item.Split(':');
                    catsItems.Add(new Tuple<string, string>(p[0], p[1]));
                }

                foreach (var ex in prod.ExtraProperties)
                {
                    var w = catsItems.FirstOrDefault(x => x.Item1.ToLowerInvariant() == ex.Item2.ToLowerInvariant());
                    if (w != null)
                        return w;
                }
            }

            return null;
        }

        public int GetIntWarrantyPerClient(Product prod)
        {
            int x = 0;
            Tuple<string, string> time = GetWarrantyPerClient(prod);

            if (time != null)
            {
                var t = time.Item2.Split(' ');

                int.TryParse(t[0], out x);
            }

            return x;
        }

        /// <summary>
        /// Update the inventory n consignment
        /// </summary>
        /// <param name="factor">factor == -1 reduce inventory , factor == 1 inc inventory </param>
        public void UpdateConsignmentInventory(int factor)
        {
            if (!Config.TrackInventory)
                return;

            if (Finished)
            {
                if (Config.ParInConsignment)
                {
                    List<ConsStruct> details = new List<ConsStruct>();
                    foreach (var det in Details)
                        details.Add(ConsStruct.GetStructFromDetail(det));

                    foreach (var item in details)
                    {
                        if (item.Return > 0)
                            item.Product.UpdateInventory(item.Return, null, factor, 0);

                        if (item.Sold > 0)
                            item.Product.UpdateInventory(item.Sold, null, factor, 0);

                        if (Config.AddCoreBalance)
                        {
                            var coreQty = DataAccess.GetSingleUDF("coreQty", item.Detail.ExtraFields);
                            float qty = 0;
                            float.TryParse(coreQty, out qty);

                            var coreId = item.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                            if (coreId != null)
                            {
                                var prodCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);
                                if (prodCore != null)
                                    prodCore.UpdateInventory(qty, null, factor, 0);
                            }
                        }
                    }
                }
                else
                {
                    // update the inventory;
                    foreach (var detail in Details)
                    {
                        if (Config.UseFullConsignment)
                        {
                            if (detail.ConsignmentPicked < 0)
                            {
                                if (!detail.Damaged)
                                    detail.Product.UpdateInventory(detail.ConsignmentPicked, detail.UnitOfMeasure, detail.Lot, DateTime.MinValue, factor, detail.Weight);
                            }
                            else
                            {
                                detail.Product.UpdateInventory(detail.ConsignmentPicked, detail.UnitOfMeasure, detail.Lot, DateTime.MinValue, factor, detail.Weight);
                            }
                        }
                        else if (!detail.ConsignmentSalesItem)
                            detail.Product.UpdateInventory(detail.ConsignmentPick, null, factor, detail.Weight);
                    }
                }
            }
            else if (factor == -1)
            {
                foreach (OrderDetail od in details)
                {
                    if (OrderType == OrderType.Consignment && od.ConsignmentSalesItem && !Config.UseFullConsignment)
                        od.Product.UpdateInventory(od.Qty, null, 1, od.Weight);

                    if (OrderType == OrderType.Consignment && Config.UseBattery && Config.AddCoreBalance)
                    {
                        var coreId = od.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

                        if (coreId != null && (od.ConsignmentCount < od.ConsignmentOld || od.ConsignmentSalesItem))
                        {
                            var prodCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);
                            if (prodCore != null)
                            {
                                var qty = DataAccess.GetSingleUDF("coreQty", od.ExtraFields);

                                float coreQty = 0;
                                float.TryParse(qty, out coreQty);

                                prodCore.UpdateInventory(coreQty, null, -1, od.Weight);
                            }
                        }
                    }
                }
            }
        }

        public bool AddDiscountCategory(OrderDetail sourceDetail)
        {
            // if use picking && status > picking, no se aplican las offers
            if (Finished)
                return false;
            bool modified = false;
            UnitOfMeasure uom = null;
            int uomId = -1;
            if (sourceDetail != null)
            {
                uom = sourceDetail.UnitOfMeasure;
                if (uom != null)
                    uomId = uom.Id;
            }

            OrderDetail removedFreeDetail = null;
            if (sourceDetail != null)
                if (sourceDetail.ExtraFields != null && sourceDetail.ExtraFields.Contains("sourceoffer"))
                {
                    var udf = DataAccess.ExplodeExtraProperties(sourceDetail.ExtraFields).FirstOrDefault(x => x.Key == "sourceoffer");
                    // maybe an error here, checking for the end of the string, or the matching |
                    var matchStr = "sourceoffer=" + udf.Value;
                    var matchingDetails = Details.Where(x => x.ExtraFields != null && x.ExtraFields.Contains(matchStr)).ToList();
                    var offerId = Convert.ToInt32(udf.Value);
                    var offer = Offer.OfferList.FirstOrDefault(x => x.OfferId == offerId);

                    if (offer != null)
                    {
                        if (!IsDelivery)
                        {
                            // removed the added line
                            int idProdF = Convert.ToInt32(DataAccess.ExplodeExtraProperties(offer.ExtraFields).FirstOrDefault(x => x.Key.ToLowerInvariant() == "productfree")?.Value);
                            var addedDetail = matchingDetails.FirstOrDefault(x => x.Product.ProductId == idProdF && x.ExtraFields != null && x.ExtraFields.Contains("productfree=yes"));
                            if (addedDetail != null)
                            {
                                removedFreeDetail = addedDetail;
                                Details.Remove(addedDetail);
                                //context.OrderDetails.Remove(addedDetail);
                                matchingDetails.Remove(addedDetail);
                                modified = true;
                            }
                        }
                        else
                            matchingDetails.Clear();

                        if (offer.Type == OfferType.Discount)
                        {
                            // removed the added line
                            var addedDetail = matchingDetails.FirstOrDefault(x => x.Product.ProductId == offer.ProductId && x.ExtraFields != null && x.ExtraFields.Contains(matchStr));
                            if (addedDetail != null)
                            {
                                removedFreeDetail = addedDetail;
                                Details.Remove(addedDetail);
                                //context.OrderDetails.Remove(addedDetail);
                                matchingDetails.Remove(addedDetail);
                                modified = true;
                            }
                        }
                    }

                    foreach (var detail in matchingDetails)
                        detail.ExtraFields = DataAccess.RemoveSingleUDF("sourceoffer", detail.ExtraFields);
                    // remove ALL the source offers marks
                }
            var part1 = Details.Where(o =>
            ((o.UnitOfMeasure == null && uom == null) || (uom != null && o.UnitOfMeasure != null && uom.Id == o.UnitOfMeasure.Id)) && !o.IgnoreInOffers &&
           o.Product.ProductType != ProductType.Discount && !o.IsFreeItem).ToList();

            if (Config.IgnoreDiscountInCredits)
                part1 = part1.Where(x => !x.IsCredit).ToList();

            var detailsWithDiscount = part1.
            GroupBy(o => o.Product.DiscountCategoryId).Select(x => new { Key = x.Key, sumQty = x.Sum(y => y.Qty), details = x.ToList() }).ToList();

            var typeOff = new[] { 5, 6, 7 };
            var t1 = Offer.GetOffersVisibleToClient(Client, true).ToList();

            var t2 = t1.ToList();
            if (!_oldOffers)
                t2 = t1.Where(o => ((o.UnitOfMeasureId <= 0 && uomId == -1 || (o.UnitOfMeasureId == uomId)))).ToList();

            var t3 = t2.Where(o => (typeOff.Contains((int)o.Type))).ToList();
            var t4 = t3.Where(o => (o.FromDate < this.Date && o.ToDate > this.Date)).ToList();
            var offerDisc = t4.OrderByDescending(x => x.MinimunQty).ToList();

            var noMore = false;
            var lastDiscCategory = -1;
            foreach (var item in offerDisc)
            {
                if (item.Type == OfferType.DiscountQty && IsDelivery)
                    continue;

                if (noMore && item.Type == OfferType.DiscountAmount && item.Product.DiscountCategoryId == lastDiscCategory)
                    continue;

                List<KeyValuePairWritable<string, string>> extraFields;
                var detDisc = detailsWithDiscount.FirstOrDefault(x => x.Key == item.Product.DiscountCategoryId);
                if (detDisc != null)
                {
                    switch ((int)item.Type)
                    {
                        case 5:
                            var lineDisc = Details.FirstOrDefault(x => x.Product.ProductId == item.ProductId);
                            if (lineDisc != null)
                            {
                                Details.Remove(lineDisc);
                                lineDisc = null;
                                modified = true;
                            }
                            var amount = -(item.Price * detDisc.sumQty);
                            if (amount != 0)
                            {
                                var newDetail = new OrderDetail(item.Product, 1, this)
                                {
                                    Qty = 1,
                                    Price = amount,
                                    ExpectedPrice = amount,
                                    Id = item.ProductId,
                                    Product = item.Product,
                                    IsCredit = false,
                                    FromOffer = true,
                                    Comments = string.Empty,
                                    Damaged = false,
                                    Taxed = false,
                                    TaxRate = 0,
                                    Discount = 0,
                                    DiscountType = 0,
                                    Lot = string.Empty,
                                    Allowance = 0,
                                    ExtraFields = string.Empty,
                                };

                                if (_oldOffers)
                                    newDetail.isMixAndMatchRelated = true;

                                Details.Add(newDetail);

                                // by default add the 
                                if (uom != null)
                                    newDetail.UnitOfMeasure = uom;
                                else
                                if (!string.IsNullOrEmpty(item.Product.UoMFamily))
                                {
                                    var uom1 = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == item.Product.UoMFamily && x.IsDefault);
                                    if (uom1 != null)
                                        newDetail.UnitOfMeasure = uom1;
                                }
                                newDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), newDetail.ExtraFields);
                                foreach (var toMarkDetail in detDisc.details)
                                {
                                    toMarkDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), toMarkDetail.ExtraFields);
                                }
                                modified = true;
                            }
                            break;
                        case 6:
                            extraFields = DataAccess.ExplodeExtraProperties(item.ExtraFields);
                            int idProdF = Convert.ToInt32(extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "productfree")?.Value);
                            string isMult = extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "multiple")?.Value;
                            var prod = Product.Find(idProdF);

                            if (prod == null)
                                continue;

                            if (detDisc.sumQty >= item.MinimunQty)
                            {
                                double div = (item.MinimunQty == 0.0 ? 1 : item.MinimunQty);
                                var countUse = detDisc.details.Where(x => !(x.FromOffer)).Sum(x => x.Qty);
                                double howManyOffersUsed = isMult == "1" ? Math.Truncate(countUse / div) : 1;
                                double qtyPF = Math.Truncate(item.FreeQty * howManyOffersUsed);
                                if (qtyPF != 0)
                                {

                                    // now see if this prod is added to the order as free item of the same offer
                                    var howExtraFieldLooksLike = "sourceoffer=" + item.OfferId.ToString();
                                    var howExtraFieldLooksLike2 = "productfree=yes";
                                    var preAddedItem = Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ExtraFields.Contains(howExtraFieldLooksLike)
                                     && x.ExtraFields.Contains(howExtraFieldLooksLike2));
                                    if (preAddedItem != null)
                                    {
                                        Details.Remove(preAddedItem);
                                    }

                                    var newDetail = new OrderDetail(item.Product, 1, this)
                                    {
                                        Qty = (float)qtyPF,
                                        Price = 0,
                                        ExpectedPrice = 0,
                                        Id = idProdF,
                                        Product = prod,
                                        IsCredit = false,
                                        FromOffer = true,
                                        Comments = string.Empty,
                                        Damaged = false,
                                        Taxed = false,
                                        TaxRate = 0,
                                        Discount = 0,
                                        DiscountType = 0,
                                        Lot = string.Empty,
                                        Allowance = 0,
                                        ExtraFields = string.Empty
                                    };

                                    if (_oldOffers)
                                        newDetail.isMixAndMatchRelated = true;

                                    Details.Add(newDetail);

                                    if (!_oldOffers)
                                    {
                                        if (uom != null)
                                            newDetail.UnitOfMeasure = uom;
                                        else
                                        if (!string.IsNullOrEmpty(item.Product.UoMFamily))
                                        {
                                            var uom1 = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == item.Product.UoMFamily && x.IsDefault);
                                            if (uom1 != null)
                                                newDetail.UnitOfMeasure = uom1;
                                        }
                                    }
                                    else
                                    {
                                        UnitOfMeasure relatedUoM = null;
                                        if (prod.UoMFamily == sourceDetail.Product.UoMFamily)
                                        {
                                            relatedUoM = uom;
                                        }
                                        else
                                        if (!string.IsNullOrEmpty(prod.UoMFamily))
                                        {
                                            var familyUoM = UnitOfMeasure.List.Where(x => x.FamilyId == prod.UoMFamily);

                                            if (familyUoM.Count() > 0)
                                            {
                                                //if (familyUoM.Any(x => x.Conversion == uom.Conversion))
                                                //{
                                                //    //check if same conversion
                                                //    var uom1 = familyUoM.FirstOrDefault(x => x.Conversion == uom.Conversion);
                                                //    if (uom1 != null)
                                                //        relatedUoM = uom1;
                                                //}
                                                //else
                                                //{
                                                //just add the defualt
                                                var uom1 = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == prod.UoMFamily && x.IsDefault);
                                                if (uom1 != null)
                                                    relatedUoM = uom1;
                                                //}
                                            }
                                        }

                                        if (relatedUoM != null)
                                            newDetail.UnitOfMeasure = relatedUoM;
                                    }

                                    // see if the deleted detail 
                                    if (removedFreeDetail != null && removedFreeDetail.Product.ProductId == newDetail.Product.ProductId &&
                                        ((newDetail.UnitOfMeasure == null) || (removedFreeDetail.UnitOfMeasure == newDetail.UnitOfMeasure)))
                                        newDetail.Lot = removedFreeDetail.Lot;
                                    newDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), newDetail.ExtraFields);
                                    newDetail.ExtraFields = DataAccess.SyncSingleUDF("productfree", "yes", newDetail.ExtraFields);
                                    foreach (var toMarkDetail in detDisc.details)
                                    {
                                        toMarkDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), toMarkDetail.ExtraFields);
                                    }
                                    modified = true;
                                }
                            }
                            else
                            {
                                // now see if this prod is added to the order as free item of the same offer
                                var howExtraFieldLooksLike = "sourceoffer=" + item.OfferId.ToString();
                                var preAddedItem = Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ExtraFields.Contains(howExtraFieldLooksLike));
                                if (preAddedItem != null)
                                {
                                    Details.Remove(preAddedItem);
                                    modified = true;
                                }
                            }
                            break;
                        case 7:
                            var detailsToCheck = detDisc.details.Where(x => !x.ModifiedManually).ToList();

                            if (detailsToCheck.Sum(x => x.Qty) >= item.MinimunQty)
                            {
                                foreach (var odi in detailsToCheck)
                                {
                                    double pr = odi.Price;
                                    if (pr != item.Price)
                                        odi.ExtraFields = DataAccess.SyncSingleUDF("PriceBeforeOffer", pr.ToString(), odi.ExtraFields);
                                    odi.Price = item.Price;
                                    odi.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), odi.ExtraFields);
                                    odi.FromOffer = true;
                                    modified = true;

                                    //add discount comment
                                    if (string.IsNullOrEmpty(odi.Comments) && odi.Comments.Contains("Offer:") && Config.OffersAddComment)
                                        odi.Comments = "Offer: Buy " + item.MinimunQty + " at " + item.Price.ToCustomString();
                                }

                                lastDiscCategory = detDisc.Key;
                                noMore = true;
                            }
                            else
                            {
                                foreach (var odi in detailsToCheck)
                                {
                                    //extraFields = DataProvider.ExplodeExtraProperties(odi.ExtraFields);
                                    //KeyValuePairWritable<string, string> priceBefOff = extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "pricebeforeoffer");
                                    //if (priceBefOff != null)
                                    //{
                                    bool cameFromOffer = false;
                                    odi.Price = Product.GetPriceForProduct(odi.Product, this, out cameFromOffer, false, false, odi.UnitOfMeasure);
                                    odi.ExtraFields = DataAccess.RemoveSingleUDF("PriceBeforeOffer", odi.ExtraFields);
                                    odi.ExtraFields = DataAccess.RemoveSingleUDF("sourceoffer", odi.ExtraFields);
                                    odi.FromOffer = cameFromOffer;
                                    modified = true;

                                    if (!string.IsNullOrEmpty(odi.Comments) && odi.Comments.Contains("Offer:"))
                                        odi.Comments = string.Empty;
                                    //}
                                }
                            }
                            break;
                    }
                }
            }

            if (removedFreeDetail != null)
                Details.Remove(removedFreeDetail);
            return modified;
        }

        public class DiscountItem
        {
            public int Key { get; set; }

            public float sumQty { get; set; }
        }

        public bool ProductHasOffer(Product product, UnitOfMeasure uom, double qty = 1, OrderDetail orderdetail = null)
        {
            int uomId = uom != null ? uom.Id : 0;

            if (Finished)
                return false;

            var part1 = Details.Where(o =>
            ((o.UnitOfMeasure == null && uom == null) || (uom != null && o.UnitOfMeasure != null && uom.Id == o.UnitOfMeasure.Id)) && !o.IgnoreInOffers &&
           o.Product.ProductType != ProductType.Discount && !o.IsFreeItem).ToList();

            if (orderdetail != null)
                part1 = part1.Where(x => x.OrderDetailId != orderdetail.OrderDetailId).ToList();

            if (Config.IgnoreDiscountInCredits)
                part1 = part1.Where(x => !x.IsCredit).ToList();

            part1 = part1.Where(x => x.Product.DiscountCategoryId == product.DiscountCategoryId).ToList();

            var detailsWithDiscount = part1.
            GroupBy(o => o.Product.DiscountCategoryId).Select(x => new DiscountItem { Key = x.Key, sumQty = x.Sum(y => y.Qty) }).ToList();

            var detail = detailsWithDiscount.FirstOrDefault(x => x.Key == product.DiscountCategoryId);

            if (detail != null)
            {
                detail.sumQty = (detail.sumQty + (float)qty);
            }
            else
                detailsWithDiscount.Add(new DiscountItem { Key = product.DiscountCategoryId, sumQty = (float)qty });

            var typeOff = new[] { 5, 6, 7 };
            var t1 = Offer.GetOffersVisibleToClient(Client, true).ToList();

            var t2 = t1.ToList();
            if (!_oldOffers)
                t2 = t1.Where(o => ((o.UnitOfMeasureId <= 0 && uomId == -1 || (o.UnitOfMeasureId == uomId)))).ToList();

            var t3 = t2.Where(o => (typeOff.Contains((int)o.Type))).ToList();
            var t4 = t3.Where(o => (o.FromDate < this.Date && o.ToDate > this.Date)).ToList();
            var offerDisc = t4.OrderByDescending(x => x.MinimunQty).ToList();

            foreach (var item in offerDisc)
            {
                var detDisc = detailsWithDiscount.FirstOrDefault(x => x.Key == item.Product.DiscountCategoryId);
                if (detDisc != null)
                {
                    switch ((int)item.Type)
                    {
                        case 5:
                            return true;
                        case 6:
                            if ((detDisc.sumQty) >= item.MinimunQty)
                            {
                                return true;
                            }
                            break;
                        case 7:
                            if ((detDisc.sumQty) >= item.MinimunQty)
                            {
                                return true;
                            }
                            break;
                    }
                }
            }

            return false;
        }

        public bool RecalculateDiscounts()
        {
            bool changed = false;

            if (Config.IgnoreDiscountInCredits && OrderType == OrderType.Credit)
                return false;
            var availableOffers = Offer.GetOffersVisibleToClient(Client, true).ToList();
            List<OrderDetail> detailsToConsider;
            if (Config.IgnoreDiscountInCredits)
                detailsToConsider = Details.Where(x => !x.IsCredit).ToList();
            else
                detailsToConsider = Details.ToList();

            if (Config.DontCalculateOffersAfterPriceChanged)
                detailsToConsider = detailsToConsider.Where(x => !x.ModifiedManually).ToList();

            //dont include free items
            detailsToConsider = detailsToConsider.Where(x => !x.IsFreeItem).ToList();

            foreach (var detail in detailsToConsider)
            {
                if (_oldOffers && detail.isMixAndMatchRelated)
                    continue;

                if (AddDiscountCategory(detail))
                    changed = true;
            }

            if (Config.CalculateOffersAutomatically && VerifyDiscount())
            {
                changed = true;
            }

            NeedToCalculate = true;

            return changed;
        }

        public void DeleteDetails(List<OrderDetail> d_details, bool save = true)
        {
            foreach (var detail in d_details)
            {
                if (detail.Substracted)
                    DeleteInventory(detail);
                details.Remove(detail);

                deletedDetails.Add(detail);
            }

            if (save)
                Save();
        }

        public void DeleteDetailsFromSplit(List<OrderDetail> d_details)
        {
            foreach (var detail in d_details)
            {
                details.Remove(detail);
                deletedDetails.Add(detail);
            }
        }

        List<OrderDiscountApplyDTO> discountApplay = new List<OrderDiscountApplyDTO>();

        public bool VerifyDiscount()
        {
            if (!OrderDiscount.HasDiscounts)
                return false;

            if (IsWorkOrder)
                return false;

            discountApplay = GetDiscountApplyList(this);

            var shipdate = ShipDate != DateTime.MinValue ? ShipDate : DateTime.Now;

            var listDiscount = Details.Where(x => x.OrderDiscountId > 0).ToList();
            if (OrderDiscountTracking?.Any() ?? false)
            {
                var listIDTrakin = OrderDiscountTracking.Select(x => new OrderDetail() { OrderDiscountId = x.OrderDiscountId }).ToList();
                listDiscount.AddRange(listIDTrakin);
            }
            double countItem = Details.Where(x => !x.FromOffer && !x.IsFreeItem).Sum(x => x.Qty);

            double subTotal = Details.Where(x => !x.FromOffer && !x.IsFreeItem).Sum(x => (x.Price * x.Qty));
            var listAllProduct = Product.Products.Select(x => x.ProductId).ToList();

            List<OrderDetail> toDelate = new List<OrderDetail>();

            bool modifiedByIncremental = false;

            foreach (var itemDiscount in listDiscount)
            {
                double discountInLine = 0;

                bool stillApplyByDate = true;

                var orderDiscount = (itemDiscount.OrderDiscount == null) ? OrderDiscount.List.FirstOrDefault(x => x.Id == itemDiscount.OrderDiscountId) : itemDiscount.OrderDiscount;
                var orderDiscountBreak = (itemDiscount.OrderDiscountBreak == null) ? orderDiscount.OrderDiscountBreaks.FirstOrDefault(x => x.Id == itemDiscount.OrderDiscountBreakId) : itemDiscount.OrderDiscountBreak;

                if (orderDiscount.AutomaticApplied || (orderDiscountBreak?.OrderDiscount?.AutomaticApplied ?? false))
                {
                    toDelate.Add(itemDiscount);
                    continue;
                }

                var orderDiscProduct = orderDiscount.OrderDiscountProducts;

                var listVendorId = (orderDiscount.OrderDiscountVendors?.Any() ?? false) ? orderDiscount.OrderDiscountVendors.Select(x => x.VendorId).ToList() : new List<int>();
                var listCategoryId = (orderDiscount.OrderDiscountCategories?.Any(x => x.CategoryType == (int)OrderDiscountCategoryType.Product) ?? false)
                                     ? orderDiscount.OrderDiscountCategories.Where(x => x.CategoryType == (int)OrderDiscountCategoryType.Product).Select(x => x.CategoryId).ToList() : new List<int>();

                var listProductId = (orderDiscount.OrderDiscountProducts?.Any() ?? false)
                             ? orderDiscount.OrderDiscountProducts.Select(x => x.ProductId).ToList()
                             : (orderDiscount.OrderDiscountVendors?.Any() ?? false) ? Product.Products.Where(x => listVendorId.Contains(x.VendorId)).Select(x => x.ProductId).ToList()
                             : (orderDiscount.OrderDiscountCategories?.Any(x => x.CategoryType == (int)OrderDiscountCategoryType.Product) ?? false)
                             ? Product.Products.Where(x => listCategoryId.Contains(x.CategoryId)).Select(x => x.ProductId).ToList()
                             : listAllProduct;

                listProductId = IntersectList(listProductId, listAllProduct);

                countItem = (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity)
                       ? Details.Where(x => !x.FromOffer && listProductId.Contains(x.Product.ProductId)).Sum(x => x.Qty)
                       : Details.Where(x => !x.FromOffer && listProductId.Contains(x.Product.ProductId)).Sum(x => x.Qty * x.Price);
                //descuento por transaccion

                if (!ActiveDiscount(orderDiscount, shipdate))
                    stillApplyByDate = false;

                double discountTotal = 0;

                if (orderDiscount.AppliedTo == (int)OrderDiscountApplyType.OrderDiscount)
                {
                    double minQtyBuy = 0;
                    if (orderDiscount.OrderDiscountClientAreas.Any(x => x.AreaId == Client.AreaId))
                    {
                        var discountApply = orderDiscount.OrderDiscountClientAreas.FirstOrDefault(x => x.AreaId == Client.AreaId);
                        bool Incremental = (DataAccess.GetSingleUDF("IncrementalDiscount", discountApply.OrderDiscount.ExtraFields) == "1");
                        calculateApplay(discountApply.DiscountType, Incremental, ref discountInLine, ref minQtyBuy, countItem, discountApply.Qty, discountApply.Buy, subTotal);

                    }
                    if (orderDiscount.OrderDiscountCategories.Any(x => x.CategoryType == (int)OrderDiscountCategoryType.Client && x.CategoryId == Client.CategoryId))
                    {
                        var discountApply = orderDiscount.OrderDiscountCategories.FirstOrDefault(x => x.CategoryType == (int)OrderDiscountCategoryType.Client && x.CategoryId == Client.CategoryId);
                        bool Incremental = (DataAccess.GetSingleUDF("IncrementalDiscount", discountApply.OrderDiscount.ExtraFields) == "1");

                        calculateApplay(discountApply.DiscountType, Incremental, ref discountInLine, ref minQtyBuy, countItem, discountApply.Qty, discountApply.Buy, subTotal);


                    }
                    if (orderDiscount.OrderDiscountClients.Any(x => x.ClientId == Client.ClientId))
                    {
                        var discountApply = orderDiscount.OrderDiscountClients.FirstOrDefault(x => x.ClientId == Client.ClientId);
                        bool Incremental = (DataAccess.GetSingleUDF("IncrementalDiscount", discountApply.OrderDiscount.ExtraFields) == "1");
                        calculateApplay(discountApply.DiscountType, Incremental, ref discountInLine, ref minQtyBuy, countItem, discountApply.Qty, discountApply.Buy, subTotal);

                    }

                    if ((-1 * discountInLine).CompareTo(itemDiscount.Price) != 0)
                    {
                        toDelate.Add(itemDiscount);
                        continue;
                    }

                    discountTotal += discountInLine;

                    if (countItem < minQtyBuy || ((subTotal - discountTotal) < double.Epsilon) || !stillApplyByDate)
                    {
                        toDelate.Add(itemDiscount);
                        discountTotal -= discountInLine;
                    }
                }
                else
                {

                    bool Incremental = (DataAccess.GetSingleUDF("IncrementalDiscount", orderDiscountBreak.ExtraFields) == "1");

                    bool IncrementalFreeIems = !string.IsNullOrEmpty(DataAccess.GetSingleUDF("IncrementalFreeIems", orderDiscountBreak.ExtraFields));

                    if (countItem < orderDiscountBreak.MinQty || (orderDiscountBreak.MaxQty != -1 && orderDiscountBreak.MaxQty < countItem) || !stillApplyByDate)
                    {
                        toDelate.Add(itemDiscount);
                    }
                    else if (Incremental)
                    {
                        #region  case discount is incremental
                        double minQtyBuy = 0;
                        calculateApplay(orderDiscountBreak.DiscountType, Incremental, ref discountInLine, ref minQtyBuy, countItem, orderDiscountBreak.Discount ?? 0, orderDiscountBreak.Discount ?? 0, subTotal);
                        if ((Math.Abs(itemDiscount.Price)).CompareTo(discountInLine) != 0)
                        {
                            DiscountAmount -= Math.Abs((float)itemDiscount.Price);
                            itemDiscount.Price = (-1 * (discountInLine));
                            modifiedByIncremental = true;
                            DiscountAmount += (float)discountInLine;
                        }
                        #endregion
                    }
                    else if (IncrementalFreeIems)
                    {
                        var maxSelect = GetValueToIncrementalGiff(orderDiscountBreak, countItem, orderDiscount);

                        var detailsForBreak = Details.Where(x => x.OrderDiscountBreakId > 0 && x.OrderDiscountBreakId == orderDiscountBreak.Id).Sum(x => x.Qty);

                        var comparador = EqualityComparer<double>.Default;
                        if (!comparador.Equals(maxSelect, detailsForBreak) && (maxSelect - detailsForBreak) < double.Epsilon)
                        {
                            toDelate.Add(itemDiscount);
                        }
                    }

                }
            }
            //delete 
            foreach (var item in toDelate)
            {
                var uniqId = DataAccess.GetSingleUDF("UniqueId", item.ExtraFields ?? "");
                if (!string.IsNullOrEmpty(uniqId))
                {
                    var itemOrderDetail = Details.FirstOrDefault(x => x.OriginalId.ToString() == uniqId);
                    Details.Remove(itemOrderDetail);
                    //ver si es necesario del context
                }

                var amountToSum = item.Price;
                if (item.Product != null && item.OrderDiscountId == 0)
                    amountToSum = item.Qty * item.Price;

                DiscountAmount += (float)amountToSum;
                if (item.Product != null)
                {
                    Details.Remove(item);
                }
                else
                {
                    var orderTracking = OrderDiscountTrackings.List.FirstOrDefault(x => x.OrderId == OrderId && x.OrderDiscountId == item.OrderDiscountId);
                    if (orderTracking != null)
                    {
                        OrderDiscountTrackings.List.Remove(orderTracking);
                        OrderDiscountTrackings.Save();
                    }
                }
                //ver si es necesario del context
            }

            #region Apply Automatic
            //obtengo los descuentos automaticos a aplicar 
            var listDiscountAtomatic = GetOrderDiscountAtomaticToApplay(this, shipdate);
            var applyAtomatic = (listDiscountAtomatic.Any()) ? Applay(listDiscountAtomatic, this) : false;

            #endregion

            //new stuff
            SetCostDiscountToProduct();

            if (applyAtomatic)
                ApplyMultipleDiscountsToProducts(this);

            return (toDelate.Any() || applyAtomatic || modifiedByIncremental);
        }

        public static void ApplyMultipleDiscountsToProducts(Order order, OrderDiscountApplyType orderDiscountType = OrderDiscountApplyType.FixedPriceDiscount, int countToProduct = 1)
        {
            var listDiscountId = order.Details.Where(x => x.OrderDiscountId > 0).Select(x => x.OrderDiscountId).ToList();
            var disountApplyInOrder = OrderDiscount.List.Where(x => listDiscountId.Contains(x.Id) && x.AppliedTo == (int)orderDiscountType)
                                                            .Select(x => new { Id = x.Id, Permanent = x.Permanent, StartDate = x.StartDate })
                                                            .ToList();

            var discountApplay = order.Details.Where(x => (x.OrderDiscountId > 0) && x.FromOffer && disountApplyInOrder.Any(y => y.Id == x.OrderDiscountId)).ToList();



            Dictionary<int, List<OrderDActive>> activo = new Dictionary<int, List<OrderDActive>>();
            List<OrderDetail> delete = new List<OrderDetail>();

            foreach (var itemD in discountApplay)
            {
                /*
                 *  ExtraFields = DataProvider.SyncSingleUDF("ProductId", productBuy.Id.ToString(), "",
                        new List<KeyValuePairWritable<string, string>>() { new KeyValuePairWritable<string, string>("PLId", "1") }),
                 */

                var idProduct = DataAccess.GetSingleUDF("ProductId", itemD.ExtraFields);
                if (string.IsNullOrEmpty(idProduct))
                    continue;

                var discount = disountApplyInOrder.FirstOrDefault(x => itemD.OrderDiscountId == x.Id);

                var productId = int.Parse(idProduct);
                if (activo.ContainsKey(productId))
                {
                    activo[productId].Add(new OrderDActive()
                    {
                        Id = itemD.Id,
                        Date = discount.StartDate,
                        Permanent = discount.Permanent,
                        OrderDetail = itemD
                    });

                }
                else
                {

                    activo.Add(productId, new List<OrderDActive>(){new OrderDActive()
                    {
                        Id = itemD.Id,
                        Date = discount.StartDate,
                        Permanent = discount.Permanent,
                        OrderDetail = itemD
                    } });
                }
            }

            foreach (var item in activo)
            {
                sortDiscount(item.Value);
                delete.AddRange(item.Value.Skip(countToProduct).Select(x => x.OrderDetail).ToList()); ;
            }
            if (delete.Any())
            {
                order.DiscountAmount += (float)delete.Select(x => x.Price).Sum();
                order.DeleteDetails(delete);
            }
        }

        private static void sortDiscount(List<OrderDActive> listDiscountAtomatic)
        {
            var listPermanent = listDiscountAtomatic.Where(x => x.Permanent).ToList();
            var listToDate = listDiscountAtomatic.Where(x => !(x.Permanent)).ToList();
            listToDate = listToDate.OrderByDescending(x => x.Date).ToList();
            listDiscountAtomatic.Clear();
            listDiscountAtomatic.AddRange(listPermanent);
            listDiscountAtomatic.AddRange(listToDate);
        }

        internal class OrderDActive
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public bool Permanent { get; set; }
            public OrderDetail OrderDetail { get; set; }
        }


        public bool WillHaveMoreThanLimit(double qty)
        {
            if (Config.MaxQtyInOrder > 0 && (Details.Count() + qty) > Config.MaxQtyInOrder)
                return true;

            return false;
        }

        private void calculateApplay(int discountType, bool incremental, ref double discountInLine, ref double minQtyBuy, double countItem, double qty, double buy, double subTotal)
        {
            if (discountType == (int)DetailDiscountType.Amount)
            {

                discountInLine += (incremental) ? (countItem * qty) : qty;
                minQtyBuy = buy;
            }
            else
            {
                //itemDiscount.Price 
                discountInLine += (incremental) ? (countItem * ((qty / 100.0) * subTotal)) : ((qty / 100.0) * subTotal);
                minQtyBuy = ((buy / 100.0) * subTotal);


            }

            discountInLine = Math.Round(discountInLine, Config.Round, MidpointRounding.AwayFromZero);
        }

        private double GetValueToIncrementalGiff(OrderDiscountBreak itemB, double countCurrent, OrderDiscount orderDiscount)
        {
            var extrafiels = DataAccess.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields);
            var IncrementalGifItems = (!string.IsNullOrEmpty(extrafiels));
            var MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
            try
            {
                string values = extrafiels.Replace("[", "").Replace("]", "");
                char[] separators = { ',' };
                List<double> numbers = values.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => double.Parse(s))
                                .ToList();
                var StepInterval = numbers[0];
                var StepGif = numbers[1];

                double starInterval = itemB.MinQty;
                double endInterval = itemB.MaxQty;


                if (starInterval <= countCurrent && (endInterval == -1 || countCurrent <= endInterval))
                {

                    int indexInterval = (int)Math.Floor(((countCurrent - starInterval) / StepInterval));

                    if (indexInterval >= 0)
                    {
                        MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1) + (indexInterval * StepGif);
                    }
                    else
                    {
                        MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
                    }
                }
                else
                {
                    MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
                }
            }
            catch (Exception ex)
            {

                MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
            }

            return MaxSelect;
        }

        #region Method to Applay Automatic Discount

        private List<OrderDiscountApplyDTO> GetDiscountApplyList(Order order)
        {
            var discountApplay = new List<OrderDiscountApplyDTO>();
            foreach (var item in order.Details.Where(x => (x.FromOffer) && x.OrderDiscountId > 0))
            {
                var orderDicosuntApply = discountApplay.FirstOrDefault(x => x.Id == item.OrderDiscountId);
                if (orderDicosuntApply == null)
                {
                    orderDicosuntApply = new OrderDiscountApplyDTO()
                    {
                        Id = item.OrderDiscountId,
                        Breaks = new Dictionary<int, List<ProductParams>>()
                    };
                    discountApplay.Add(orderDicosuntApply);
                }
                if (item.OrderDiscountBreakId > 0)
                {
                    if (!orderDicosuntApply.Breaks.ContainsKey(item.OrderDiscountBreakId))
                    {
                        orderDicosuntApply.Breaks.Add(item.OrderDiscountBreakId, new List<ProductParams>());


                    }
                    var productId = DataAccess.GetSingleUDF("ProductId", item.ExtraFields);
                    var idP = (string.IsNullOrEmpty(productId)) ? 0 : int.Parse(productId);
                    orderDicosuntApply.Breaks[item.OrderDiscountBreakId].Add(new ProductParams()
                    {
                        Id = idP,
                        // Name = item.Product?.Name,
                        QtySelect = (idP != 0) ? item.Qty : 0,
                        Price = item.Price * ((idP != 0) ? -1 : 1)
                    });
                }
            }

            foreach (var item in order.OrderDiscountTracking)
            {
                var orderDicosuntApply = discountApplay.FirstOrDefault(x => x.Id == item.OrderDiscountId);
                if (orderDicosuntApply == null)
                {
                    orderDicosuntApply = new OrderDiscountApplyDTO()
                    {
                        Id = item.OrderDiscountId,
                        Breaks = new Dictionary<int, List<ProductParams>>()
                    };
                    discountApplay.Add(orderDicosuntApply);
                }

            }

            return discountApplay;
        }

        private List<OrderDiscount> GetOrdersDiscountAutomatic(Order order, DateTime? dateTime = null)
        {

            var exclude = DataAccess.GetSingleUDF("ExcludeDiscount", order.ExtraFields);


            DateTime _dateTime = (dateTime != null) ? (DateTime)dateTime : DateTime.Now;
            List<OrderDiscount> resultD = new List<OrderDiscount>();
            var listAreaClientId = AreaClient.List.Where(x => x.ClientId == order.Client.ClientId).Select(x => x.AreaId).ToList();
            var listProductId = Details.Select(x => x.Product.ProductId).ToList();

            var listGroupClientId = ClientCategoryEx.List.Where(x => x.Id == order.Client.CategoryId).Select(x => x.Id).ToList();

            var existDiscountClient = OrderDiscountClient.List.Where(x => x.ClientId == Client.ClientId
                                                                        && x.OrderDiscount.AutomaticApplied
                                                                        && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();

            if (existDiscountClient.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountClient.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());


            var existDiscountClientArea = OrderDiscountClientArea.List.Where(x => listAreaClientId.Contains(x.AreaId)
                                                                                && x.OrderDiscount.AutomaticApplied
                                                                                && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();

            if (existDiscountClientArea.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountClientArea.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());

            #region PL
            var existDiscountClientPriceLevel = OrderDiscountClientPriceLevel.List.Where(x => x.PriceLevelId == order.Client.PriceLevel && x.OrderDiscount != null
                                                                                             && x.OrderDiscount.AutomaticApplied
                                                                                             && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();
            resultD.AddRange(existDiscountClientPriceLevel.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());
            #endregion

            #region Discount Category Client
            var existDiscountGroup = OrderDiscountCategory.List.Where(x => x.CategoryType == (int)OrderDiscountCategoryType.Client
                                                                                && listGroupClientId.Contains(x.CategoryId)
                                                                                && x.OrderDiscount.AutomaticApplied
                                                                                && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();

            if (existDiscountGroup.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountGroup.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());
            #endregion

            var existDiscountProduct = OrderDiscountProduct.List.Where(x => listProductId.Contains(x.ProductId)
                                                                           && x.OrderDiscount.AutomaticApplied
                                                                           && (!exclude.Contains(x.OrderDiscount.Id.ToString()))).ToList();

            if (existDiscountProduct.Any(x => ActiveDiscount(x.OrderDiscount, _dateTime)))
                resultD.AddRange(existDiscountProduct.Where(x => ActiveDiscount(x.OrderDiscount, _dateTime)).Select(x => x.OrderDiscount).Distinct().ToList());


            return resultD.Distinct().ToList();


        }

        public class ProductParams : ICloneable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParamsType { get; set; }
            public double QtyOrder { get; set; }
            public double QtyMinBuy { get; set; }
            public double Difference { get { return QtyOrder - QtyMinBuy; } }
            public double QtySelect { get; set; }
            public double Price { get; set; }
            public string UOM { get; set; }

            public object Clone()
            {
                return new ProductParams
                {
                    Id = Id,
                    Name = Name,
                    ParamsType = ParamsType,
                    QtyOrder = QtyOrder,
                    QtyMinBuy = QtyMinBuy,
                    QtySelect = QtySelect,
                    Price = Price,
                    UOM = UOM
                };
            }
        }

        private List<GridItem> GetOrderDiscountAtomaticToApplay(Order order, DateTime? dateTime = null)
        {
            var listAreaClientId = AreaClient.List.Where(x => x.ClientId == order.Client.ClientId).Select(x => x.AreaId).ToList();

            var listCategoryClientId = ClientCategoryEx.List.Where(x => x.Id == order.Client.CategoryId).Select(x => x.Id).ToList();

            var listOrderDiscount = GetOrdersDiscountAutomatic(order, dateTime);
            List<GridItem> customerDiscounts = new List<GridItem>();

            var productList = Product.GetProductListForOrder(order, order.orderType != OrderType.Order, 0).ToList();
            var listProductVisible = productList.Select(x => x.ProductId).ToList();

            var _listProductInOrderId = order.Details.Where(x => x.OrderDiscountId == 0 || x.OrderDiscountId == null).Select(x => x.Product.ProductId).ToList();

            var dictProductCountInOrder = order.Details
                .Where(x => x.OrderDiscountId == 0 || x.OrderDiscountId == null)
                .GroupBy(x => x.Product.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (double)x.Qty)
                );
            
            foreach (var orderDiscount in listOrderDiscount)
            {

                List<int> idVendor = orderDiscount.OrderDiscountVendors.Select(x => x.Id).ToList();
                List<int> idCategoryP = orderDiscount.OrderDiscountCategories.Where(x => x.CategoryType == (int)OrderDiscountCategoryType.Product).Select(x => x.CategoryId).ToList();

                var listProductBuy = (orderDiscount.OrderDiscountProducts.Count > 0)
                    ? orderDiscount.OrderDiscountProducts.Select(x => x.ProductId).ToList()
                    : orderDiscount.OrderDiscountVendors.Count > 0
                    ? productList/*context.Products*/.Where(x => idVendor.Contains(x.VendorId)).Select(x => x.ProductId).ToList()
                    : (orderDiscount.OrderDiscountCategories.Any(x => x.CategoryType == (int)OrderDiscountCategoryType.Product))
                    ? productList/*context.Products*/.Where(x => idCategoryP.Contains(x.CategoryId)).Select(x => x.ProductId).ToList()
                    : productList.Select(x => x.ProductId).ToList();//context.Products.Select(x => x.Id).ToList();


                listProductBuy = IntersectList(listProductBuy, listProductVisible);
                if (!listProductBuy.Any())
                    continue;

                var listProductGet = GetProductToBreak(orderDiscount);

                if (listProductGet.Any())
                {
                    listProductGet = IntersectList(listProductGet, listProductVisible);
                    if (!listProductGet.Any())
                    {
                        continue;
                    }
                }

                double countItemOrder = GetCountItems(order, listProductBuy);
                /**/
                double calculatedTotalOrder = CalculatedTotal(order, listProductBuy);

                /**/

                var itemGrid = new GridItem();

                itemGrid.Id = orderDiscount.Id;
                itemGrid.IdExtension = orderDiscount.Id.ToString();
                itemGrid.AutomaticApplied = orderDiscount.AutomaticApplied;
                itemGrid.DiscountName = orderDiscount.Name;
                itemGrid.StartDate = orderDiscount.StartDate;
                itemGrid.EndDate = orderDiscount.EndDate;
                itemGrid.Permanet = orderDiscount.Permanent;
                itemGrid.Type = orderDiscount.DiscountType.ToString();
                itemGrid.Comments = orderDiscount.Comments;
                itemGrid.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                itemGrid.AppliedTo = orderDiscount.AppliedTo;
                itemGrid.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;

                /*Productos a aplicar */

                List<ProductParams> SubParamsBuy = Product.Products.Where(x => listProductBuy.Contains(x.ProductId)).Select(x => new ProductParams
                {
                    Id = x.ProductId,
                    Name = x.Name,
                    ParamsType = 1,
                    //Price = x.PriceForClient(_order.Client, 1)
                    UOM = x.DefaultUomName

                }).ToList();
                //GetUOMSalet(SubParamsBuy);
                //SubParamsBuy.ForEach(x => x.Price = productsBuy.FirstOrDefault(y => y.Id == x.Id).PriceForClient(_order.Client, 1));
                /*Productos a dar  */

                /**/
                
                #region MixMatch
                var orderDicountBreaksMixMatch = orderDiscount.OrderDiscountBreaks.Where(x => x.ExtraFields?.Contains("MixMatch=1") ?? false);
                var orderDicountBreaks = orderDiscount.OrderDiscountBreaks.Where(x => !(x.ExtraFields?.Contains("MixMatch=1") ?? false));
                var productIdsMM = orderDicountBreaksMixMatch.SelectMany(x => x.OrderDiscountProductBreaks)
                    .Select(p => p.ProductId)
                    .Distinct()
                    .ToList();
                var totalUse = productIdsMM.Where(x => dictProductCountInOrder.ContainsKey(x)).Sum(x => dictProductCountInOrder[x]);
                #endregion

                var SubParamsGet = Product.Products.Where(x => listProductGet.Contains(x.ProductId)).Select(x => new ProductParams
                {
                    Id = x.ProductId,
                    Name = x.Name,
                    ParamsType = 1
                }).ToList();

                var orderDC = orderDiscount.OrderDiscountClients.FirstOrDefault(x => x.ClientId == order.Client.ClientId);
                if (orderDC != null)
                {
                    /*Si tiene breack*/
                    if (orderDiscount.AppliedTo == 1)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString();
                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;
                            itemGridB.BreackId = itemB.Id;
                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;
                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = orderDC.Client.ClientName;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();

                            #region Incremental Gif
                            //itemGridB.IncrementalGifItems = (!string.IsNullOrEmpty(DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields)));
                            //itemGridB.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct) ?? -1;
                            //SetValuesToIncrementalGiff(itemGridB, DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields), itemB);
                            SetValuesToIncrementalGiff(itemGridB, itemB, countItemOrder, calculatedTotalOrder, orderDiscount);
                            #endregion

                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();
                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            //itemGridB.Bonification
                            /*Productos a aplicar */
                            itemGridB.SubParamsBuy = InsertCount(SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);
                            /**/
                            /**/
                            //itemGridB.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && itemB.MinQty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && itemB.MinQty <= countItemOrder));
                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, countItemOrder);
                            /**/
                            ApplayDiscount(itemGridB, order, countItemOrder, calculatedTotalOrder);

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                    }
                    else if (orderDiscount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            /**/
                            /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                            /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                            var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                            var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                            if (!_SubParamsBuy.Any())
                                continue;

                            _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/

                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = orderDC.Client.ClientName;// orderDPL.PriceLevel.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();


                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */

                            itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));

                            #region Incremental Gif                           
                            SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                            #endregion
                            /**/

                            ApplayDiscountPriceLevel(itemGridB, order);
                            if (itemGridB.Discount < double.Epsilon)
                                itemGridB.DiscountApply = false;

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                        
                          #region MixMath
                            //var productIdsMM = orderDicountBreaksMixMatch.SelectMany(x => x.OrderDiscountProductBreaks)
                            //                                           .Select(p => p.ProductId)
                            //                                           .Distinct()
                            //                                           .ToList();
                            //var totalUse = productIdsMM.Where(x => dictProductCountInOrder.ContainsKey(x)).Sum(x => dictProductCountInOrder[x]);


                            foreach (var itemB in orderDicountBreaksMixMatch)
                            {

                                if (!itemB.OrderDiscountProductBreaks.Any(x => _listProductInOrderId.Contains(x.ProductId)))
                                {
                                    continue;
                                }
                                /**/
                                /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                                /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                                var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                                var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                                if (!_SubParamsBuy.Any())
                                    continue;

                                _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/

                                var itemGridB = new GridItem();

                                itemGridB.Id = orderDiscount.Id;
                                itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                                itemGridB.ExtraFields = itemB.ExtraFields;
                                itemGridB.MinQty = itemB.MinQty;
                                itemGridB.MaxQty = itemB.MaxQty;

                                itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                                itemGridB.BreackId = itemB.Id;

                                itemGridB.DiscountName = orderDiscount.Name;
                                itemGridB.StartDate = orderDiscount.StartDate;
                                itemGridB.EndDate = orderDiscount.EndDate;
                                itemGridB.Permanet = orderDiscount.Permanent;

                                itemGridB.Type = orderDiscount.DiscountType.ToString();
                                itemGridB.Comments = orderDiscount.Comments;
                                itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                                itemGridB.AppliedTo = orderDiscount.AppliedTo;
                                itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                                itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                                /*Productos a dar  */
                                itemGridB.SubParamsGet = (itemB.QtySelectProduct == null) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/
                                itemGridB.Customer = orderDC.Client.ClientName;// orderDPL.PriceLevel.Name;
                                itemGridB.DiscountType = itemB.DiscountType.ToString();

                                itemGridB.Buy = itemB.MinQty.ToString();


                                //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                                itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                                if (itemGridB.MaxSelect > double.Epsilon)
                                    itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                                /**/
                                /*Productos a aplicar */

                                itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType, fixedPrice: itemB.Discount);

                                //itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));
                                itemGridB.DiscountApply = IsApplyMixM(ListIdProductInBreack, totalUse, dictProductCountInOrder, itemB.MinQty, itemB.MaxQty);
                                #region Not add
                                if (!itemGridB.DiscountApply)
                                    continue;
                                #endregion

                                #region Incremental Gif                           
                                SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                                #endregion
                                /**/

                                ApplayDiscountPriceLevel(itemGridB, order);
                                if (itemGridB.Discount < double.Epsilon)
                                    itemGridB.DiscountApply = false;

                                itemGridB.Self = itemGridB;
                                customerDiscounts.Add(itemGridB);
                            }
                            #endregion
                    }
                    else
                    {

                        itemGrid.Customer = orderDC.Client.ClientName;
                        itemGrid.DiscountType = orderDC.DiscountType.ToString();
                        itemGrid.Amount = orderDC.Qty.ToString();
                        itemGrid.Buy = orderDC.Buy.ToString();
                        //itemGrid.CustomerD = x,
                        //itemGrid.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDC.Qty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDC.Qty <= countItemOrder));
                        itemGrid.DiscountApply = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDC.Buy <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDC.Buy <= countItemOrder));
                        itemGrid.SubParamsGet = new List<ProductParams>();

                        itemGrid.SubParamsBuy = InsertCount(SubParamsBuy, orderDC.Buy, order, orderDiscount.DiscountType);

                        ApplayDiscount(itemGrid, order, countItemOrder, calculatedTotalOrder);

                        itemGrid.Self = itemGrid;
                        customerDiscounts.Add(itemGrid);
                    }
                }

                var orderDA = orderDiscount.OrderDiscountClientAreas.FirstOrDefault(x => listAreaClientId.Contains(x.AreaId));
                if (orderDA != null)
                {

                    /*Si tiene breack*/
                    if (orderDiscount.AppliedTo == 1/*orderDiscount.OrderDiscountBreaks.Count > 0*/)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString();
                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;


                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = orderDA.Area.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();

                            #region Incremental Gif
                            //itemGridB.IncrementalGifItems = (!string.IsNullOrEmpty(DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields)));
                            //itemGridB.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct) ?? -1;
                            //SetValuesToIncrementalGiff(itemGridB, DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields), itemB);
                            SetValuesToIncrementalGiff(itemGridB, itemB, countItemOrder, calculatedTotalOrder, orderDiscount);
                            #endregion
                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */
                            itemGridB.SubParamsBuy = InsertCount(SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            //itemGridB.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && itemB.MinQty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && itemB.MinQty <= countItemOrder));
                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, countItemOrder);
                            /**/
                            ApplayDiscount(itemGridB, order, countItemOrder, calculatedTotalOrder);

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                    }
                    else if (orderDiscount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            /**/
                            /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                            /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                            var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                            var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                            if (!_SubParamsBuy.Any())
                                continue;

                            _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/

                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = orderDA.Area.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();


                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */

                            itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));

                            #region Incremental Gif

                            SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                            #endregion
                            /**/

                            ApplayDiscountPriceLevel(itemGridB, order);
                            if (itemGridB.Discount < double.Epsilon)
                                itemGridB.DiscountApply = false;

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                        
                        #region MixMath
                            //var productIdsMM = orderDicountBreaksMixMatch.SelectMany(x => x.OrderDiscountProductBreaks)
                            //                                           .Select(p => p.ProductId)
                            //                                           .Distinct()
                            //                                           .ToList();
                            //var totalUse = productIdsMM.Where(x => dictProductCountInOrder.ContainsKey(x)).Sum(x => dictProductCountInOrder[x]);


                            foreach (var itemB in orderDicountBreaksMixMatch)
                            {

                                if (!itemB.OrderDiscountProductBreaks.Any(x => _listProductInOrderId.Contains(x.ProductId)))
                                {
                                    continue;
                                }
                                /**/
                                /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                                /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                                var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                                var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                                if (!_SubParamsBuy.Any())
                                    continue;

                                _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/

                                var itemGridB = new GridItem();

                                itemGridB.Id = orderDiscount.Id;
                                itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                                itemGridB.ExtraFields = itemB.ExtraFields;
                                itemGridB.MinQty = itemB.MinQty;
                                itemGridB.MaxQty = itemB.MaxQty;

                                itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                                itemGridB.BreackId = itemB.Id;

                                itemGridB.DiscountName = orderDiscount.Name;
                                itemGridB.StartDate = orderDiscount.StartDate;
                                itemGridB.EndDate = orderDiscount.EndDate;
                                itemGridB.Permanet = orderDiscount.Permanent;

                                itemGridB.Type = orderDiscount.DiscountType.ToString();
                                itemGridB.Comments = orderDiscount.Comments;
                                itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                                itemGridB.AppliedTo = orderDiscount.AppliedTo;
                                itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                                itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                                /*Productos a dar  */
                                itemGridB.SubParamsGet = (itemB.QtySelectProduct == null) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/
                                itemGridB.Customer = orderDA.Area.Name;// orderDPL.PriceLevel.Name;
                                itemGridB.DiscountType = itemB.DiscountType.ToString();

                                itemGridB.Buy = itemB.MinQty.ToString();


                                //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                                itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                                if (itemGridB.MaxSelect > double.Epsilon)
                                    itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                                /**/
                                /*Productos a aplicar */

                                itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType, fixedPrice: itemB.Discount);

                                //itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));
                                itemGridB.DiscountApply = IsApplyMixM(ListIdProductInBreack, totalUse, dictProductCountInOrder, itemB.MinQty, itemB.MaxQty);
                                #region Not add
                                if (!itemGridB.DiscountApply)
                                    continue;
                                #endregion

                                #region Incremental Gif                           
                                SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                                #endregion
                                /**/

                                ApplayDiscountPriceLevel(itemGridB, order);
                                if (itemGridB.Discount < double.Epsilon)
                                    itemGridB.DiscountApply = false;

                                itemGridB.Self = itemGridB;
                                customerDiscounts.Add(itemGridB);
                            }
                            #endregion
                    }
                    else
                    {



                        itemGrid.Customer = orderDA.Area.Name;
                        itemGrid.DiscountType = orderDA.DiscountType.ToString();
                        itemGrid.Amount = orderDA.Qty.ToString();
                        itemGrid.Buy = orderDA.Buy.ToString();
                        //itemGrid.CustomerD = x,
                        //itemGrid.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDA.Qty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDA.Qty <= countItemOrder));
                        itemGrid.DiscountApply = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDA.Buy <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDA.Buy <= countItemOrder));
                        itemGrid.SubParamsGet = new List<ProductParams>();

                        itemGrid.SubParamsBuy = InsertCount(SubParamsBuy, orderDA.Buy, order, orderDiscount.DiscountType);

                        ApplayDiscount(itemGrid, order, countItemOrder, calculatedTotalOrder);

                        itemGrid.Self = itemGrid;
                        customerDiscounts.Add(itemGrid);
                    }
                }


                var orderDG = orderDiscount.OrderDiscountCategories.FirstOrDefault(x => x.CategoryType == (int)OrderDiscountCategoryType.Client && listCategoryClientId.Contains(x.CategoryId));
                if (orderDG != null)
                {

                    /*Si tiene breack*/
                    if (orderDiscount.AppliedTo == 1/*orderDiscount.OrderDiscountBreaks.Count > 0*/)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString();
                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;


                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = ClientCategoryEx.List.FirstOrDefault(x => x.Id == orderDG.CategoryId)?.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();

                            #region Incremental Gif
                            //itemGridB.IncrementalGifItems = (!string.IsNullOrEmpty(DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields)));
                            //itemGridB.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct) ?? -1;
                            //SetValuesToIncrementalGiff(itemGridB, DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields), itemB);
                            SetValuesToIncrementalGiff(itemGridB, itemB, countItemOrder, calculatedTotalOrder, orderDiscount);
                            #endregion

                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */
                            itemGridB.SubParamsBuy = InsertCount(SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            //itemGridB.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && itemB.MinQty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && itemB.MinQty <= countItemOrder));
                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, countItemOrder);
                            /**/
                            ApplayDiscount(itemGridB, order, countItemOrder, calculatedTotalOrder);

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                    }
                    else if (orderDiscount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            /**/
                            /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                            /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                            var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                            var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                            if (!_SubParamsBuy.Any())
                                continue;

                            _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/

                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/

                            itemGridB.Customer = ClientCategoryEx.List.FirstOrDefault(x => x.Id == orderDG.CategoryId)?.Name;
                            //context.ClientCategories.FirstOrDefault(x => x.Id == orderDG.CategoryId)?.Name; // orderDPL.PriceLevel.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();


                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */

                            itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));

                            #region Incremental Gif
                            //itemGridB.IncrementalGifItems = (!string.IsNullOrEmpty(DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields)));
                            //itemGridB.MaxSelect = (itemB.QtySelectProduct ?? -1);
                            //SetValuesToIncrementalGiff(itemGridB, DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields), itemB);
                            SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                            #endregion
                            /**/

                            ApplayDiscountPriceLevel(itemGridB, order);
                            if (itemGridB.Discount < double.Epsilon)
                                itemGridB.DiscountApply = false;

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                        
                        
                            #region MixMath

                            foreach (var itemB in orderDicountBreaksMixMatch)
                            {

                                if (!itemB.OrderDiscountProductBreaks.Any(x => _listProductInOrderId.Contains(x.ProductId)))
                                {
                                    continue;
                                }
                                /**/
                                /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                                /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                                var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                                var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                                if (!_SubParamsBuy.Any())
                                    continue;

                                _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/

                                var itemGridB = new GridItem();

                                itemGridB.Id = orderDiscount.Id;
                                itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";

                                itemGridB.ExtraFields = itemB.ExtraFields;
                                itemGridB.MinQty = itemB.MinQty;
                                itemGridB.MaxQty = itemB.MaxQty;

                                itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                                itemGridB.BreackId = itemB.Id;

                                itemGridB.DiscountName = orderDiscount.Name;
                                itemGridB.StartDate = orderDiscount.StartDate;
                                itemGridB.EndDate = orderDiscount.EndDate;
                                itemGridB.Permanet = orderDiscount.Permanent;

                                itemGridB.Type = orderDiscount.DiscountType.ToString();
                                itemGridB.Comments = orderDiscount.Comments;
                                itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                                itemGridB.AppliedTo = orderDiscount.AppliedTo;
                                itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                                itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                                /*Productos a dar  */
                                itemGridB.SubParamsGet = (itemB.QtySelectProduct == null) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                                /**/
                                itemGridB.Customer = ClientCategoryEx.List.FirstOrDefault(x => x.Id == orderDG.CategoryId)?.Name;
                                itemGridB.DiscountType = itemB.DiscountType.ToString();

                                itemGridB.Buy = itemB.MinQty.ToString();


                                //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                                itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                                if (itemGridB.MaxSelect > double.Epsilon)
                                    itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                                /**/
                                /*Productos a aplicar */

                                itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType, fixedPrice: itemB.Discount);

                                //itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));
                                itemGridB.DiscountApply = IsApplyMixM(ListIdProductInBreack, totalUse, dictProductCountInOrder, itemB.MinQty, itemB.MaxQty);
                                #region Not add
                                if (!itemGridB.DiscountApply)
                                    continue;
                                #endregion

                                #region Incremental Gif                           
                                SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                                #endregion
                                /**/

                                ApplayDiscountPriceLevel(itemGridB, order);
                                if (itemGridB.Discount < double.Epsilon)
                                    itemGridB.DiscountApply = false;

                                itemGridB.Self = itemGridB;
                                customerDiscounts.Add(itemGridB);
                            }
                            #endregion
                    }
                    else
                    {

                        itemGrid.Customer = ClientCategoryEx.List.FirstOrDefault(x => x.Id == orderDG.CategoryId)?.Name;
                        itemGrid.DiscountType = orderDG.DiscountType.ToString();
                        itemGrid.Amount = orderDG.Qty.ToString();
                        itemGrid.Buy = orderDG.Buy.ToString();
                        //itemGrid.CustomerD = x,
                        //itemGrid.Select = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDA.Qty <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDA.Qty <= countItemOrder));
                        itemGrid.DiscountApply = ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && orderDG.Buy <= calculatedTotalOrder) || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && orderDG.Buy <= countItemOrder));
                        itemGrid.SubParamsGet = new List<ProductParams>();

                        itemGrid.SubParamsBuy = InsertCount(SubParamsBuy, orderDG.Buy, order, orderDiscount.DiscountType);

                        ApplayDiscount(itemGrid, order, countItemOrder, calculatedTotalOrder);

                        itemGrid.Self = itemGrid;
                        customerDiscounts.Add(itemGrid);
                    }
                }

                #region Discount Price Level 
                var priceLevelId = order.Client?.PriceLevel ?? -1;
                var orderDPL = orderDiscount.OrderDisocuntClientPriceLevels.FirstOrDefault(x => x.PriceLevelId == priceLevelId);
                if (orderDPL != null)
                {

                    /*Si tiene breack*/
                    if (orderDiscount.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                    {
                        foreach (var itemB in orderDiscount.OrderDiscountBreaks)
                        {
                            /**/
                            /*Para estos descuentos cada breack hay que filtrar si el producto sobre el que se hace esta en los posibles a ver*/
                            /*Optimizar esto para de ante mano tener ya lista de breack qeu se pueden poner*/
                            var ListIdProductInBreack = itemB.OrderDiscountProductBreaks.Select(x => x.ProductId).ToList();
                            var _SubParamsBuy = SubParamsBuy.Where(x => ListIdProductInBreack.Contains(x.Id)).ToList();
                            if (!_SubParamsBuy.Any())
                                continue;

                            _SubParamsBuy = _SubParamsBuy.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/

                            var itemGridB = new GridItem();

                            itemGridB.Id = orderDiscount.Id;
                            itemGridB.IdExtension = orderDiscount.Id.ToString() + "_(" + getStringToProductID(_SubParamsBuy) + ")";
                            itemGridB.AutomaticApplied = orderDiscount.AutomaticApplied;

                            itemGridB.BreackId = itemB.Id;

                            itemGridB.DiscountName = orderDiscount.Name;
                            itemGridB.StartDate = orderDiscount.StartDate;
                            itemGridB.EndDate = orderDiscount.EndDate;
                            itemGridB.Permanet = orderDiscount.Permanent;

                            itemGridB.Type = orderDiscount.DiscountType.ToString();
                            itemGridB.Comments = orderDiscount.Comments;
                            itemGridB.Status = (orderDiscount.Status == (int)ClientDiscountType.Draft) ? ClientDiscountType.Draft : (orderDiscount.Status == (int)ClientDiscountType.Active) ? ClientDiscountType.Active : ClientDiscountType.Inactive;
                            itemGridB.AppliedTo = orderDiscount.AppliedTo;
                            itemGridB.ProductDiscountId = orderDiscount.ProductDiscountId ?? 0;
                            itemGridB.IncrementalDiscount = (DataAccess.GetSingleUDF("IncrementalDiscount", itemB.ExtraFields) == "1");

                            /*Productos a dar  */
                            itemGridB.SubParamsGet = (itemB.QtySelectProduct == 0) ? new List<ProductParams>() : SubParamsGet.Select(x => (ProductParams)x.Clone()).ToList();
                            /**/
                            itemGridB.Customer = orderDPL.PriceLevel.Name;
                            itemGridB.DiscountType = itemB.DiscountType.ToString();

                            itemGridB.Buy = itemB.MinQty.ToString();


                            //itemGridB.Amount = (itemB.Discount ?? 0).ToString();
                            itemGridB.Amount = (itemGridB.MaxSelect > double.Epsilon) ? "0" : (itemB.Discount ?? 0).ToString();

                            if (itemGridB.MaxSelect > double.Epsilon)
                                itemGridB.DiscountBonification = (itemB.Discount ?? 0);
                            /**/
                            /*Productos a aplicar */

                            itemGridB.SubParamsBuy = InsertCount(_SubParamsBuy, itemB.MinQty, order, orderDiscount.DiscountType);

                            itemGridB.DiscountApply = IsApplay(orderDiscount, itemB, calculatedTotalOrder, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder));

                            #region Incremental Gif
                            //itemGridB.IncrementalGifItems = (!string.IsNullOrEmpty(DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields)));
                            //itemGridB.MaxSelect = (itemB.QtySelectProduct ?? -1);
                            //SetValuesToIncrementalGiff(itemGridB, DataProvider.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields), itemB);
                            SetValuesToIncrementalGiff(itemGridB, itemB, itemGridB.SubParamsBuy.Sum(x => x.QtyOrder), calculatedTotalOrder, orderDiscount);
                            #endregion
                            /**/

                            ApplayDiscountPriceLevel(itemGridB, order);
                            if (itemGridB.Discount < double.Epsilon)
                                itemGridB.DiscountApply = false;

                            itemGridB.Self = itemGridB;
                            customerDiscounts.Add(itemGridB);
                        }
                    }

                }
                #endregion
            }

            #region Update Applay to Group
            UpdateApplyInDiscount(customerDiscounts);
            #endregion

            #region Set Bonification in Atomatic

            foreach (var itemGrid in customerDiscounts.Where(x => x.AutomaticApplied && (!x.Select) && x.MaxSelect > 0 && (x.SubParamsGet.Any())))
            {
                foreach (var subparam in itemGrid.SubParamsGet)
                    subparam.QtySelect = itemGrid.MaxSelect;

                List<Tuple<int, double>> listSelect = itemGrid.SubParamsGet.Select(x => new Tuple<int, double>(x.Id, itemGrid.MaxSelect)).ToList();
                double dicount = 0;
                foreach (var item in listSelect)
                {
                    ProductParams productItem = itemGrid.SubParamsGet.FirstOrDefault(x => x.Id == item.Item1);
                    if (productItem != null)
                    {
                        double priceProduct = 0;
                        var pp = productList.FirstOrDefault(x => x.ProductId == item.Item1);
                        if (pp != null)
                            priceProduct = Product.GetPriceForProduct(pp, order.Client, false, true);

                        dicount += GetDiscountToBonification(priceProduct, item.Item2, itemGrid);
                    }
                }
                itemGrid.Discount = dicount;
            }

            #endregion

            return customerDiscounts;
        }

        
        private bool IsApplyMixM(List<int> listIdProductInBreack, double totalUse, Dictionary<int, double> dictProductCountInOrder, double minQty, double maxQty)
        {
            if (minQty <= totalUse && (maxQty == -1 || maxQty >= totalUse))
                return true;

            return listIdProductInBreack.Any(p => (dictProductCountInOrder.ContainsKey(p) &&
                                                   (minQty <= dictProductCountInOrder[p] && (maxQty == -1 || maxQty >= dictProductCountInOrder[p]))));
        }
        
        public class OrderDiscountApplyDTO
        {
            public int Id { get; set; }
            public Dictionary<int, List<ProductParams>> Breaks { get; set; }
        }

        private void SetValuesToIncrementalGiff(GridItem itemGrid, OrderDiscountBreak itemB, double countItemOrder, double calculatedTotalOrder, OrderDiscount orderDiscount)
        {
            try
            {
                var extrafiels = DataAccess.GetSingleUDF("IncrementalFreeIems", itemB.ExtraFields);
                itemGrid.IncrementalGifItems = (!string.IsNullOrEmpty(extrafiels));
                itemGrid.MaxSelect = (itemB.QtySelectProduct ?? -1);
                if (itemGrid.IncrementalGifItems)
                {
                    string values = extrafiels.Replace("[", "").Replace("]", "");
                    char[] separators = { ',' };
                    List<double> numbers = values.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => double.Parse(s))
                                    .ToList();
                    itemGrid.StepInterval = numbers[0];
                    itemGrid.StepGif = numbers[1];

                    double starInterval = itemB.MinQty;
                    double endInterval = itemB.MaxQty;

                    double countCurrent = (orderDiscount.DiscountType == (int)CustomerDiscountType.Amount) ? calculatedTotalOrder : countItemOrder;
                    if (starInterval <= countCurrent && (endInterval == -1 || countCurrent <= endInterval))
                    {

                        int indexInterval = (int)Math.Floor(((countCurrent - starInterval) / itemGrid.StepInterval));

                        if (indexInterval >= 0)
                        {
                            itemGrid.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1) + (indexInterval * itemGrid.StepGif);
                        }
                        else
                        {
                            itemGrid.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
                        }
                    }
                    else
                    {
                        itemGrid.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
                    }
                }
            }
            catch (Exception ex)
            {
                itemGrid.IncrementalGifItems = false;
                itemGrid.MaxSelect = (itemB.QtySelectProduct == 0 ? -1 : itemB.QtySelectProduct ?? -1);
            }

        }

        bool IsApplay(OrderDiscount orderDiscount, OrderDiscountBreak itemB, double calculatedTotalOrder, double countItemOrder)
        {
            return ((orderDiscount.DiscountType == (int)CustomerDiscountType.Amount && (itemB.MinQty <= calculatedTotalOrder && (itemB.MaxQty == -1 || calculatedTotalOrder <= itemB.MaxQty)))
                 || (orderDiscount.DiscountType == (int)CustomerDiscountType.Quantity && (itemB.MinQty <= countItemOrder && (itemB.MaxQty == -1 || countItemOrder <= itemB.MaxQty))));
        }

        #region Price Level Aux
        void ApplayDiscountPriceLevel(GridItem item, Order order)
        {
            var productItem = item.SubParamsBuy.FirstOrDefault();
            var productId = productItem.Id;
            double countItemOrder = item.SubParamsBuy.Sum(x => x.QtyOrder);
            var price = getPriceLevelProductToClient(productId, order.Client.PriceLevel);

            var product = Product.Products.FirstOrDefault(x => x.ProductId == productId);

            double conversion = 1;
            var uom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
            if (uom != null)
                conversion = uom.Conversion;

            item.Discount = (((conversion * (price)) - double.Parse(item.Amount)) * countItemOrder);

            item.Type = SetCustomerDiscountType(int.Parse(item.Type), item);
        }

        #region get Price to Product in price level client
        double getPriceLevelProductToClient(int productId, int priceLevelId)
        {
            var productPrice = ProductPrice.Pricelist.FirstOrDefault(x => x.ProductId == productId && x.PriceLevelId == priceLevelId);

            var product = Product.Find(productId);

            var price = product.PriceLevel0;

            if (productPrice != null)
                price = productPrice.Price;

            return price;
        }
        #endregion
        #endregion

        private string getStringToProductID(List<ProductParams> subParamsBuy)
        {
            var listId = subParamsBuy.Select(x => x.Id).ToList();
            listId.Sort();
            return string.Join(",", listId);
        }

        private void UpdateApplyInDiscount(IEnumerable<GridItem> customerDiscounts)
        {
            Dictionary<string, ItemCurrentDTO> itemApply = new Dictionary<string, ItemCurrentDTO>();
            foreach (var itemD in customerDiscounts.GroupBy(x => x.IdExtension))
            {

                itemApply.Clear();
                itemApply.Add(CustomerDiscountType.Amount.ToString() + "-0", new ItemCurrentDTO());
                itemApply.Add(CustomerDiscountType.Amount.ToString() + "-1", new ItemCurrentDTO());

                itemApply.Add(CustomerDiscountType.Quantity.ToString() + "-0", new ItemCurrentDTO());
                itemApply.Add(CustomerDiscountType.Quantity.ToString() + "-1", new ItemCurrentDTO());

                foreach (var item in itemD)
                {
                    if (!item.DiscountApply)
                        continue;

                    double buy = GetBuy(item);
                    bool isBonification = (item.MaxSelect > double.Epsilon);
                    string key = item.Type + "-" + ((isBonification) ? "1" : "0");
                    if (itemApply.ContainsKey(key))
                    {
                        UpdateApply(itemApply[key], buy, item);
                    }


                }


            }
        }

        private List<int> IntersectList(List<int> list1, List<int> list2)
        {
            IEnumerable<int> interseccion = list1.Intersect(list2);
            return interseccion.ToList();
        }

        public static bool IsDiscount(Product product)
        {
            return ((product.ProductType == ProductType.Discount || (!string.IsNullOrEmpty(product.ExtraPropertiesAsString) && product.ExtraPropertiesAsString.Contains("ItemType=Discount"))));
        }

        private bool IsOfferDiscount(OrderDetail line, Order order)
        {
            if (IsDiscount(line.Product))
                return true;

            var productsDiscount = order.Details.Where(x => IsDiscount(x.Product)).Select(x => x.ExtraFields).ToList();
            var uniqueIdList = productsDiscount.Select(x => DataAccess.GetSingleUDF("UniqueId", x)).ToList();
            return uniqueIdList.Contains(line.OriginalId.ToString());
        }

        private bool Applay(List<GridItem> customerDiscounts, Order order)
        {
            double totalDiscount = (-1 * order.Details.Where(x => IsOfferDiscount(x, order) && x.FromOffer).Sum(x => x.Price));
            double subTotal = Details.Where(x => !(x.FromOffer)).Sum(x => x.Price * x.Qty) - totalDiscount;
            totalDiscount = 0;

            customerDiscounts = customerDiscounts.Where(x => x.DiscountApply)
                                               .OrderByDescending(x => x.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount ? x.Discount : 0)
                                               .ThenByDescending(x => x.AppliedTo == (int)OrderDiscountApplyType.OrderDiscount ? x.Discount : 0)
                                               .ThenByDescending(x => x.AppliedTo == (int)OrderDiscountApplyType.ItemsDiscount ? x.Discount : 0)
                                               .ToList();

            foreach (var gridItem in customerDiscounts)
            {
                // var gridItem = customDataGrid.Rows[i].Cells[gridItemColumnaName].Value as GridItem;

                OrderDiscountApplyDTO orderDApply = GetOrderDiscountApplyDTO(gridItem.Id);


                var ListBonification = gridItem.SubParamsGet.Where(x => x.QtySelect > double.Epsilon).ToList();
                // If you have products for bonuses, they are not added to the global order discount so that they are calculated with the differences in the order."
                //totalDiscount += (gridItem.AppliedTo == 2) ?(double)customDataGrid.Rows[i].Cells[discountColumnaName].Value: 0;
                if (gridItem.AppliedTo == (int)OrderDiscountApplyType.OrderDiscount)
                {
                    if ((subTotal - gridItem.Discount) < double.Epsilon)
                        continue;

                    subTotal -= gridItem.Discount;

                    var productDiscount = Product.Products.FirstOrDefault(x => x.ProductId == gridItem.ProductDiscountId);
                    order.Details.Add(new OrderDetail(productDiscount, 1, order)
                    {
                        Product = productDiscount,
                        Price = -1 * gridItem.Discount,
                        FromOffer = true,
                        ExtraComments = gridItem.DiscountName,
                        OrderDiscountId = gridItem.Id,
                        OrderDiscountBreakId = gridItem.BreackId ?? 0

                    });

                    //Add to discount
                    totalDiscount += gridItem.Discount;

                }



                if ((gridItem.AppliedTo == (int)OrderDiscountApplyType.ItemsDiscount || gridItem.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount) && !orderDApply.Breaks.ContainsKey(gridItem.BreackId ?? -1))
                    orderDApply.Breaks.Add(gridItem.BreackId ?? 0, new List<ProductParams>());

                var orderApplyProductDTO = (gridItem.BreackId != null) ? orderDApply.Breaks[gridItem.BreackId ?? 0] : new List<ProductParams>();

                foreach (var item in ListBonification)
                {
                    var product = Product.Products.FirstOrDefault(x => x.ProductId == item.Id);

                    var uom = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                    bool cameFromOffer = false;

                    var itemGet = new OrderDetail(product, (float)item.QtySelect, order)
                    {
                        Price = Product.GetPriceForProduct(product, order, out cameFromOffer, false, false, uom),
                        FromOffer = true,
                        UnitOfMeasure = uom,
                        //OrderDiscountId = gridItem.Id,
                        //OrderDiscountBreakId = gridItem.BreackId

                    };

                    subTotal += (itemGet.Qty * itemGet.Price);

                    var valueDiscountItem = GetDiscountToBonification(itemGet.Price, item.QtySelect, gridItem);
                    if ((subTotal - valueDiscountItem) < double.Epsilon)
                        continue;

                    subTotal -= valueDiscountItem;

                    order.Details.Add(itemGet);

                    var productDiscount = Product.Products.FirstOrDefault(x => x.ProductId == gridItem.ProductDiscountId);


                    var itemDiscount = new OrderDetail(productDiscount, (float)item.QtySelect, order)
                    {
                        Price = -1 * valueDiscountItem,
                        FromOffer = true,
                        ExtraComments = gridItem.DiscountName + "-" + product.Name,
                        OrderDiscountId = gridItem.Id,
                        OrderDiscountBreakId = gridItem.BreackId ?? 0

                    };

                    itemDiscount.ExtraFields = DataAccess.SyncSingleUDF("ProductId", item.Id.ToString(), "",
                        new List<KeyValuePairWritable<string, string>>() { new KeyValuePairWritable<string, string>("UniqueId", itemGet.OriginalId.ToString()) });


                    order.Details.Add(itemDiscount);

                    //Add to discount
                    totalDiscount += valueDiscountItem;
                }

                if (ListBonification.Count == 0 && gridItem.AppliedTo == (int)OrderDiscountApplyType.ItemsDiscount)
                {

                    if ((subTotal - gridItem.Discount) < double.Epsilon)
                        continue;

                    subTotal -= gridItem.Discount;

                    var productDiscount = Product.Products.FirstOrDefault(x => x.ProductId == gridItem.ProductDiscountId);
                    order.Details.Add(new OrderDetail(productDiscount, 1, order)
                    {
                        Product = productDiscount,
                        Price = -1 * gridItem.Discount,
                        FromOffer = true,
                        ExtraComments = gridItem.DiscountName,
                        OrderDiscountId = gridItem.Id,
                        OrderDiscountBreakId = gridItem.BreackId ?? 0

                    });


                    //Add to discount
                    totalDiscount += gridItem.Discount;

                    orderApplyProductDTO.Add(new ProductParams()
                    {
                        Id = 0,
                        QtySelect = 0,
                        Price = -1 * gridItem.Discount,
                    });
                }

                #region Price Level
                /*(int)OrderDiscountApplyType.ItemsDiscount: (radioButtonDiscountPT.Checked)?(int)OrderDiscountApplyType.OrderDiscount: */

                if (ListBonification.Count == 0 && gridItem.AppliedTo == (int)OrderDiscountApplyType.FixedPriceDiscount)
                {
                    var productDiscount = Product.Products.FirstOrDefault(x => x.ProductId == gridItem.ProductDiscountId);

                    var productBuy = gridItem.SubParamsBuy.FirstOrDefault();

                    order.Details.Add(new OrderDetail(productDiscount, 1, order)
                    {
                        Product = productDiscount,
                        Price = -1 * gridItem.Discount,
                        FromOffer = true,
                        ExtraComments = gridItem.DiscountName + "-" + productBuy.Name,
                        OrderDiscountId = gridItem.Id,
                        OrderDiscountBreakId = gridItem.BreackId ?? 0,
                        ExtraFields = DataAccess.SyncSingleUDF("ProductId", productBuy.Id.ToString(), "",
                        new List<KeyValuePairWritable<string, string>>() { new KeyValuePairWritable<string, string>("PLId", "1") })
                    });

                    //Add to discount
                    totalDiscount += gridItem.Discount;

                    orderApplyProductDTO.Add(new ProductParams()
                    {
                        Id = 0,
                        QtySelect = 0,
                        Price = -1 * gridItem.Discount,
                    });
                }
                #endregion
            }

            order.DiscountType = DiscountType.Amount;
            order.DiscountAmount += (float)totalDiscount;

            return (totalDiscount > double.Epsilon);
        }

        class GridItem : INotifyPropertyChanged
        {
            public int Id { get; set; }
            private bool select;
            public DiscountStatus Progress
            {
                get
                {
                    DateTime today = DateTime.Today;
                    return EndDate < today ? DiscountStatus.Expired :
                        StartDate > today ? DiscountStatus.NoProgress : DiscountStatus.Progressing;
                }
            }
            public ClientDiscountType Status { get; set; }
            public string DiscountName { get; set; }

            public string Customer { get; set; }
            public string Type { get; set; }

            public string Buy { get; set; }

            public string Amount { get; set; }

            public string DiscountType { get; set; }

            public DateTime StartDate { get; set; }
            public string StartDateGrid
            {
                get
                {
                    return (Permanet) ? "Permanet" : StartDate.ToString("MM/dd/yyyy");
                }
            }

            public string IdExtension { get; set; }
            public bool IncrementalDiscount { get; set; }

            public DateTime EndDate { get; set; }
            public string EndDateGrid
            {
                get
                {
                    return (Permanet) ? "Permanet" : EndDate.ToString("MM/dd/yyyy");
                }
            }

            public string Comments { get; set; }

            public bool Permanet { get; set; }


            public string DiscountText
            {
                get
                {
                    return Discount.ToCustomString();
                }
            }

            public bool Select
            {
                //get; set;
                get { return select; }
                set
                {
                    if (select != value)
                    {
                        select = value;
                        OnPropertyChanged("Select"); // Llamada al método de notificación
                    }
                }
            }

            public bool IncrementalGifItems { get; set; }

            public double StepInterval { get; set; }

            public double StepGif { get; set; }
            public bool AutomaticApplied { get; set; }

            public bool DiscountApply { get; set; }

            double discount = 0;
            public double Discount
            {
                get
                {
                    var rounded = Math.Round(discount, Config.Round, MidpointRounding.AwayFromZero);
                    return rounded;
                }
                set
                {
                    discount = value;
                }
            }

            public CustomerDiscount CustomerD { get; set; }

            public GridItem Self { get; set; }

            public int AppliedTo { get; set; }
            public string TypeDiscount
            {
                get { return (AppliedTo == 1) ? "Items Discount" : "Order Discount"; }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #region New Field
            public List<ProductParams> SubParamsBuy { get; set; }
            public List<ProductParams> SubParamsGet { get; set; }

            public string Bonification { get; set; }
            public double DiscountBonification { get; set; }

            public string DiscountBonifiationGrid
            {
                get
                {

                    if (DiscountBonification <= double.Epsilon)
                        return "0";
                    else if (int.Parse(DiscountType) == (int)DetailDiscountType.Percent)
                        return DiscountBonification.ToString() + " %";
                    else if (int.Parse(DiscountType) == (int)DetailDiscountType.Amount)
                        return DiscountBonification.ToCustomString();
                    else
                        return "0";
                }
            }

            public double MaxSelect { get; set; }

            public string MaxSelectGrid
            {
                get
                {
                    return (MaxSelect < double.Epsilon) ? "0" : MaxSelect.ToString();
                }
            }

            public int ProductDiscountId { get; set; }

            public int? BreackId { get; set; }
            
            public string ExtraFields { get; set; }

            public double MinQty { get; set; }
            public double MaxQty { get; set; }

            #endregion


        }

        private class ItemCurrentDTO
        {
            public double BuyMin { get; set; }
            public GridItem Item { get; set; }
        }

        private List<int> GetProductToBreak(OrderDiscount orderDiscount)
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

        private double GetCountItems(Order order, List<int> productId = null)
        {
            int factor = 1;
            List<string> relatedIds = new List<string>();

            foreach (var detail in order.Details)
            {
                if (!string.IsNullOrEmpty(detail.ExtraFields) && detail.ExtraFields.IndexOf("RelatedDetail") >= 0)
                {
                    var related = DataAccess.ExplodeExtraProperties(detail.ExtraFields).FirstOrDefault(x => x.Key == "RelatedDetail");
                    if (related != null)
                        relatedIds.Add(related.Value);
                }

            }


            double numberBoxes = 0;
            double weightTotal = 0;
            bool anyWeight = false;
            foreach (var detail in order.Details.Where(x => !relatedIds.Contains(x.OriginalId.ToString()) && ((productId == null) || (productId.Contains((x.Product.ProductId == 0 && x.Product != null) ? x.Product.ProductId : x.Product.ProductId))) && (!(x.FromOffer))))
            {
                factor = 1;
                if (detail.IsCredit)
                    factor = -1;

                var qtyInOrder = GetCountItemInOrderDetail(detail);

                if (detail.Product.SoldByWeight)
                {
                    if (!detail.IsCredit)
                    {
                        weightTotal += detail.Weight;
                        numberBoxes += 1 * factor;
                    }
                    anyWeight = true;
                }
                else
                {
                    weightTotal += detail.Product.Weight * (qtyInOrder) * factor;
                    numberBoxes += (qtyInOrder) * factor;
                }
            }
            return numberBoxes;
        }

        private double CalculatedTotal(Order order, List<int> listProduct = null)
        {
            double totalOrder = 0;
            double totalCredit = 0;
            foreach (var line in order.Details)
            {
                int idProduct = (line.Product.ProductId == 0 && line.Product != null) ? line.Product.ProductId : line.Product.ProductId;
                if (((line.FromOffer)) || (listProduct != null && !listProduct.Contains(idProduct)))
                    continue;

                double qty = line.Qty;
                if (line.Product.SoldByWeight)
                    qty = line.Weight;
                if (line.IsCredit)
                    totalCredit += qty * line.Price;
                else
                    totalOrder += qty * line.Price;
            }
            return totalOrder;
        }

        private List<ProductParams> InsertCount(List<ProductParams> subParamsBuy, double minQty, Order _order, int qtyOrAmount = 1, double? fixedPrice = null)
        {
            List<ProductParams> result = subParamsBuy.Select(x => new ProductParams
            {
                Id = x.Id,
                Name = x.Name,
                ParamsType = x.ParamsType,
                QtyMinBuy = minQty,
                UOM = x.UOM

            }).ToList();
            //result.AddRange(subParamsBuy);
            foreach (var item in _order.Details)
            {
                if (item.FromOffer)
                    continue;

                var idProduct = (item.Product != null) ? item.Product.ProductId : item.Product.ProductId;
                var productItem = result.FirstOrDefault(x => x.Id == idProduct);
                if (productItem != null && !OrderDiscount.IsOfferDiscount(item, this))
                {

                    var qtyInOrder = GetCountItemInOrderDetail(item);
                    
                    if (fixedPrice != null && ((double)fixedPrice).CompareTo(item.Price) >= 1)
                        continue;
                    
                    productItem.QtyOrder += (qtyOrAmount == (int)CustomerDiscountType.Quantity) ? qtyInOrder : qtyInOrder * item.Price;
                    productItem.Price = item.Price;


                }
            }
            return result;
        }

        private void ApplayDiscount(GridItem item, Order order, double countItemOrder, double CalculatedTotal)
        {

            double qty = double.Parse(item.Buy);

            double countInter = 0;

            double _discount = (item.IncrementalDiscount) ? (double.Parse(item.Amount) * countItemOrder) : double.Parse(item.Amount);


            if (item.Type == ((int)CustomerDiscountType.Amount).ToString() && qty <= CalculatedTotal)
            {

                countInter = (CalculatedTotal) / qty;

            }
            if (item.Type == ((int)CustomerDiscountType.Quantity).ToString() && qty <= countItemOrder)
            {
                countInter = countItemOrder / qty;

            }

            var discountTypeAmt = ((int)DetailDiscountType.Amount).ToString();

            if (item.DiscountType == discountTypeAmt)
            {
                item.Discount = _discount/*double.Parse(item.Amount)*/;

            }

            var discountTypePerc = ((int)DetailDiscountType.Percent).ToString();

            if (item.DiscountType == discountTypePerc)
            {
                item.Discount = ((CalculatedTotal) * (_discount/*double.Parse(item.Amount)*/ / 100.0));
            }

            // item.DiscountText = item.Discount.ToCustomString();


            //ApplayDiscount(itemGrid, _order, countItemOrder, calculatedTotalOrder);
            //item.DiscountType = SetDiscountType(int.Parse(item.DiscountType), item);
            item.Type = SetCustomerDiscountType(int.Parse(item.Type), item);
            //item.PropertyChanged += Item_PropertyChanged;

        }

        private double GetBuy(GridItem item)
        {
            return double.Parse(item.Buy.Replace("$", ""));
        }

        private void UpdateApply(ItemCurrentDTO itemCurrentDTO, double buy, GridItem item)
        {
            if (itemCurrentDTO.BuyMin < buy)
            {
                itemCurrentDTO.BuyMin = buy;
                if (itemCurrentDTO.Item != null)
                    itemCurrentDTO.Item.DiscountApply = false;
                itemCurrentDTO.Item = item;
                itemCurrentDTO.Item.DiscountApply = true;
            }
            else
            {
                item.DiscountApply = false;
            }
        }

        private void DiscountAcitve(List<GridItem> customerDiscounts)
        {
            foreach (var itemDiscountApply in discountApplay)
            {
                foreach (var itemG in customerDiscounts.Where(x => x.Id == itemDiscountApply.Id))
                {
                    if (itemDiscountApply.Breaks.TryGetValue(itemG.BreackId ?? 0, out var productB))
                    {
                        double price = productB.Sum(itemP =>
                        {
                            var product = itemG.SubParamsGet.FirstOrDefault(x => x.Id == itemP.Id);
                            if (product != null)
                            {
                                product.QtySelect = itemP.QtySelect;
                                return /*itemP.QtySelect **/ itemP.Price;
                            }
                            return 0;
                        });

                        itemG.Discount = (productB.Count == 1 && productB[0].QtySelect == 0.0) ? (productB[0].Price * -1) : price;
                        itemG.Select = true;
                    }
                    else if (itemDiscountApply.Breaks.Count == 0)
                    {
                        itemG.Select = true;
                    }
                }
            }

        }

        private OrderDiscountApplyDTO GetOrderDiscountApplyDTO(int id)
        {
            if (discountApplay.Exists(x => x.Id == id))
            {
                return discountApplay.FirstOrDefault(x => x.Id == id);
            }
            else
            {
                var orderDADTO = new OrderDiscountApplyDTO() { Id = id };
                orderDADTO.Breaks = new Dictionary<int, List<ProductParams>>();
                discountApplay.Add(orderDADTO);
                return orderDADTO;
            }


        }

        private double GetDiscountToBonification(double priceProduct, double item2, GridItem gridItem)
        {
            double discount = 0;
            double _discountBonification = (gridItem.IncrementalDiscount) ? (gridItem.DiscountBonification * item2) : gridItem.DiscountBonification;

            //gridItem.DiscountText
            if (gridItem.DiscountType == ((int)DetailDiscountType.Amount).ToString())
            {
                var _discountB = ((item2 * priceProduct) - (item2 * _discountBonification));
                discount = (_discountB < double.Epsilon) ? (item2 * priceProduct) : (item2 * _discountBonification);

            }
            double totalPercent = 100;
            if (gridItem.DiscountType == ((int)DetailDiscountType.Percent).ToString())
            {
                //var _discountB = ((priceProduct * item2) * (gridItem.DiscountBonification / totalPercent));//(((item2 * gridItem.DiscountBonification))/100));
                discount = ((priceProduct * item2) * (_discountBonification / totalPercent)); //(_discountB < double.Epsilon) ? (priceProduct* item2) : _discountB;

            }
            return discount;
        }

        private double GetCountItemInOrderDetail(OrderDetail item)
        {

            var conversion = UnitOfMeasure.List.FirstOrDefault(x => x.IsDefault && x.FamilyId == item.UnitOfMeasure?.FamilyId)?.Conversion ?? 1;
            var baseUoM = item.UnitOfMeasure?.Conversion ?? 1;

            return (item.Qty * baseUoM) / conversion;
        }

        private string SetCustomerDiscountType(int type, GridItem item)
        {
            try
            {

                var nameCustomerDiscountType = (Enum.GetName(typeof(CustomerDiscountType), type));
                switch (type)
                {
                    case ((int)CustomerDiscountType.Amount):
                        item.Buy = double.Parse(item.Buy).ToCustomString();
                        break;

                }
                return nameCustomerDiscountType;
            }
            catch (Exception exx)
            {

                return string.Empty;
            }


        }

        #endregion


        private Func<OrderDiscount, DateTime, bool> ActiveDiscount = (orderDiscount, dateTime) =>
                  orderDiscount != null && orderDiscount.Status == (int)ClientDiscountType.Active
                  && (orderDiscount.Permanent || (dateTime.Date >= orderDiscount.StartDate.Date
                  && dateTime.Date <= orderDiscount.EndDate.Date));


        public double CheckForDiscountCategory(OrderDetail sourceDetail)
        {
            this.AddDetail(sourceDetail);

            bool modified = false;
            UnitOfMeasure uom = null;
            int uomId = -1;
            if (sourceDetail != null)
            {
                uom = sourceDetail.UnitOfMeasure;
                if (uom != null)
                    uomId = uom.Id;
            }

            OrderDetail removedFreeDetail = null;
            if (sourceDetail != null)
                if (sourceDetail.ExtraFields != null && sourceDetail.ExtraFields.Contains("sourceoffer"))
                {
                    var udf = DataAccess.ExplodeExtraProperties(sourceDetail.ExtraFields).FirstOrDefault(x => x.Key == "sourceoffer");
                    // maybe an error here, checking for the end of the string, or the matching |
                    var matchStr = "sourceoffer=" + udf.Value;
                    var matchingDetails = Details.Where(x => x.ExtraFields != null && x.ExtraFields.Contains(matchStr)).ToList();
                    var offerId = Convert.ToInt32(udf.Value);
                    var offer = Offer.OfferList.FirstOrDefault(x => x.OfferId == offerId);

                    if (offer != null)
                    {
                        if (!IsDelivery)
                        {
                            // removed the added line
                            int idProdF = Convert.ToInt32(DataAccess.ExplodeExtraProperties(offer.ExtraFields).FirstOrDefault(x => x.Key.ToLowerInvariant() == "productfree")?.Value);
                            var addedDetail = matchingDetails.FirstOrDefault(x => x.Product.ProductId == idProdF && x.ExtraFields != null && x.ExtraFields.Contains("productfree=yes"));
                            if (addedDetail != null)
                            {
                                removedFreeDetail = addedDetail;
                                Details.Remove(addedDetail);
                                //context.OrderDetails.Remove(addedDetail);
                                matchingDetails.Remove(addedDetail);
                                modified = true;
                            }
                        }
                        else
                            matchingDetails.Clear();

                        if (offer.Type == OfferType.Discount)
                        {
                            // removed the added line
                            var addedDetail = matchingDetails.FirstOrDefault(x => x.Product.ProductId == offer.ProductId && x.ExtraFields != null && x.ExtraFields.Contains(matchStr));
                            if (addedDetail != null)
                            {
                                removedFreeDetail = addedDetail;
                                Details.Remove(addedDetail);
                                //context.OrderDetails.Remove(addedDetail);
                                matchingDetails.Remove(addedDetail);
                                modified = true;
                            }
                        }
                    }

                    foreach (var detail in matchingDetails)
                        detail.ExtraFields = DataAccess.RemoveSingleUDF("sourceoffer", detail.ExtraFields);
                    // remove ALL the source offers marks
                }
            var part1 = Details.Where(o =>
            ((o.UnitOfMeasure == null && uom == null) || (uom != null && o.UnitOfMeasure != null && uom.Id == o.UnitOfMeasure.Id)) &&
           o.Product.ProductType != ProductType.Discount).ToList();

            if (Config.IgnoreDiscountInCredits)
                part1 = part1.Where(x => !x.IsCredit).ToList();

            var detailsWithDiscount = part1.
            GroupBy(o => o.Product.DiscountCategoryId).Select(x => new { Key = x.Key, sumQty = x.Sum(y => y.Qty), details = x.ToList() }).ToList();

            var typeOff = new[] { 5, 6, 7 };
            var t1 = Offer.GetOffersVisibleToClient(Client, true).ToList();
            var t2 = t1.Where(o => ((o.UnitOfMeasureId <= 0 && uomId == -1 || (o.UnitOfMeasureId == uomId)))).ToList();
            var t3 = t2.Where(o => (typeOff.Contains((int)o.Type))).ToList();
            var t4 = t3.Where(o => (o.FromDate < this.Date && o.ToDate > this.Date)).ToList();
            var offerDisc = t4.OrderByDescending(x => x.MinimunQty).ToList();

            var noMore = false;
            var lastDiscCategory = -1;
            foreach (var item in offerDisc)
            {
                if (item.Type == OfferType.DiscountQty && IsDelivery)
                    continue;

                if (noMore && item.Type == OfferType.DiscountAmount && item.Product.DiscountCategoryId == lastDiscCategory)
                    continue;

                List<KeyValuePairWritable<string, string>> extraFields;
                var detDisc = detailsWithDiscount.FirstOrDefault(x => x.Key == item.Product.DiscountCategoryId);
                if (detDisc != null)
                {
                    switch ((int)item.Type)
                    {
                        case 5:
                            var lineDisc = Details.FirstOrDefault(x => x.Product.ProductId == item.ProductId);
                            if (lineDisc != null)
                            {
                                Details.Remove(lineDisc);
                                lineDisc = null;
                                modified = true;
                            }
                            var amount = -(item.Price * detDisc.sumQty);
                            if (amount != 0)
                            {
                                var newDetail = new OrderDetail(item.Product, 1, this)
                                {
                                    Qty = 1,
                                    Price = amount,
                                    ExpectedPrice = amount,
                                    Id = item.ProductId,
                                    Product = item.Product,
                                    IsCredit = false,
                                    FromOffer = true,
                                    Comments = string.Empty,
                                    Damaged = false,
                                    Taxed = false,
                                    TaxRate = 0,
                                    Discount = 0,
                                    DiscountType = 0,
                                    Lot = string.Empty,
                                    Allowance = 0,
                                    ExtraFields = string.Empty,
                                };
                                Details.Add(newDetail);

                                // by default add the 
                                if (uom != null)
                                    newDetail.UnitOfMeasure = uom;
                                else
                                if (!string.IsNullOrEmpty(item.Product.UoMFamily))
                                {
                                    var uom1 = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == item.Product.UoMFamily && x.IsDefault);
                                    if (uom1 != null)
                                        newDetail.UnitOfMeasure = uom1;
                                }
                                newDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), newDetail.ExtraFields);
                                foreach (var toMarkDetail in detDisc.details)
                                {
                                    toMarkDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), toMarkDetail.ExtraFields);
                                }
                                modified = true;
                            }
                            break;
                        case 6:
                            extraFields = DataAccess.ExplodeExtraProperties(item.ExtraFields);
                            int idProdF = Convert.ToInt32(extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "productfree")?.Value);
                            string isMult = extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "multiple")?.Value;
                            var prod = Product.Find(idProdF);
                            if (detDisc.sumQty >= item.MinimunQty)
                            {
                                double div = (item.MinimunQty == 0.0 ? 1 : item.MinimunQty);
                                double howManyOffersUsed = isMult == "1" ? Math.Truncate(detDisc.sumQty / div) : 1;
                                double qtyPF = Math.Truncate(item.FreeQty * howManyOffersUsed);
                                if (qtyPF != 0)
                                {

                                    // now see if this prod is added to the order as free item of the same offer
                                    var howExtraFieldLooksLike = "sourceoffer=" + item.OfferId.ToString();
                                    var howExtraFieldLooksLike2 = "productfree=yes";
                                    var preAddedItem = Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ExtraFields.Contains(howExtraFieldLooksLike)
                                     && x.ExtraFields.Contains(howExtraFieldLooksLike2));
                                    if (preAddedItem != null)
                                    {
                                        Details.Remove(preAddedItem);
                                    }

                                    var newDetail = new OrderDetail(item.Product, 1, this)
                                    {
                                        Qty = (float)qtyPF,
                                        Price = 0,
                                        ExpectedPrice = 0,
                                        Id = idProdF,
                                        Product = prod,
                                        IsCredit = false,
                                        FromOffer = true,
                                        Comments = string.Empty,
                                        Damaged = false,
                                        Taxed = false,
                                        TaxRate = 0,
                                        Discount = 0,
                                        DiscountType = 0,
                                        Lot = string.Empty,
                                        Allowance = 0,
                                        ExtraFields = string.Empty,
                                    };
                                    Details.Add(newDetail);

                                    if (uom != null)
                                        newDetail.UnitOfMeasure = uom;
                                    else
                                    if (!string.IsNullOrEmpty(item.Product.UoMFamily))
                                    {
                                        var uom1 = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == item.Product.UoMFamily && x.IsDefault);
                                        if (uom1 != null)
                                            newDetail.UnitOfMeasure = uom1;
                                    }
                                    // see if the deleted detail 
                                    if (removedFreeDetail != null && removedFreeDetail.Product.ProductId == newDetail.Product.ProductId &&
                                        ((newDetail.UnitOfMeasure == null) || (removedFreeDetail.UnitOfMeasure == newDetail.UnitOfMeasure)))
                                        newDetail.Lot = removedFreeDetail.Lot;
                                    newDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), newDetail.ExtraFields);
                                    newDetail.ExtraFields = DataAccess.SyncSingleUDF("productfree", "yes", newDetail.ExtraFields);
                                    foreach (var toMarkDetail in detDisc.details)
                                    {
                                        toMarkDetail.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), toMarkDetail.ExtraFields);
                                    }
                                    modified = true;
                                }
                            }
                            else
                            {
                                // now see if this prod is added to the order as free item of the same offer
                                var howExtraFieldLooksLike = "sourceoffer=" + item.OfferId.ToString();
                                var preAddedItem = Details.FirstOrDefault(x => x.Product.ProductId == prod.ProductId && x.ExtraFields.Contains(howExtraFieldLooksLike));
                                if (preAddedItem != null)
                                {
                                    Details.Remove(preAddedItem);
                                    modified = true;
                                }
                            }
                            break;
                        case 7:
                            if (detDisc.sumQty >= item.MinimunQty)
                            {
                                foreach (var odi in detDisc.details)
                                {
                                    double pr = odi.Price;
                                    if (pr != item.Price)
                                        odi.ExtraFields = DataAccess.SyncSingleUDF("PriceBeforeOffer", pr.ToString(), odi.ExtraFields);
                                    odi.Price = item.Price;
                                    odi.ExtraFields = DataAccess.SyncSingleUDF("sourceoffer", item.OfferId.ToString(), odi.ExtraFields);
                                    odi.FromOffer = true;
                                    modified = true;

                                    //add discount comment
                                    if (string.IsNullOrEmpty(odi.Comments) && odi.Comments.Contains("Offer:"))
                                        odi.Comments = "Offer: Buy " + item.MinimunQty + " at " + item.Price.ToCustomString();
                                }

                                lastDiscCategory = detDisc.Key;
                                noMore = true;
                            }
                            else
                            {
                                foreach (var odi in detDisc.details)
                                {
                                    //extraFields = DataProvider.ExplodeExtraProperties(odi.ExtraFields);
                                    //KeyValuePairWritable<string, string> priceBefOff = extraFields.FirstOrDefault(x => x.Key.ToLowerInvariant() == "pricebeforeoffer");
                                    //if (priceBefOff != null)
                                    //{
                                    bool cameFromOffer = false;
                                    odi.Price = Product.GetPriceForProduct(odi.Product, this, out cameFromOffer, false, false, odi.UnitOfMeasure);
                                    odi.ExtraFields = DataAccess.RemoveSingleUDF("PriceBeforeOffer", odi.ExtraFields);
                                    odi.ExtraFields = DataAccess.RemoveSingleUDF("sourceoffer", odi.ExtraFields);
                                    modified = true;

                                    if (!string.IsNullOrEmpty(odi.Comments) && odi.Comments.Contains("Offer:"))
                                        odi.Comments = string.Empty;
                                    //}
                                }
                            }
                            break;
                    }
                }
            }

            if (removedFreeDetail != null)
                Details.Remove(removedFreeDetail);

            var price = sourceDetail.Price;

            Details.Remove(sourceDetail);

            return price;
        }

        public List<string> ImageList { get; set; }

        private string ImageListAsString()
        {
            string s = "";

            if (ImageList != null)
                foreach (var item in ImageList)
                {
                    if (!string.IsNullOrEmpty(s))
                        s += "|";
                    s += item;
                }

            return s;
        }

        public string Term
        {
            get
            {
                string terms = string.Empty;

                if (IsDelivery && ExtraFields != null)
                {
                    var termsExtra = DataAccess.GetSingleUDF("TERMS", ExtraFields);

                    if (!string.IsNullOrEmpty(termsExtra))
                        terms = termsExtra.ToUpperInvariant();
                }

                if (!IsDelivery || string.IsNullOrEmpty(terms))
                {
                    if (Client.ExtraProperties != null)
                    {
                        var termsExtra = Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                        if (termsExtra != null)
                            terms = termsExtra.Item2.ToUpperInvariant();
                    }
                }

                return terms;
            }
        }

        public bool IsQuote { get; set; }

        public int FromInvoiceId { get; set; }

        public bool IsProjection { get; set; }

        public string RelationUniqueId { get; set; }

        public OrderDetail FindDetail(int id)
        {
            return Details.FirstOrDefault(x => x.OrderDetailId == id);
        }

        public void AddParInConsignment()
        {
            foreach (var item in Details)
                item.AdjustExtraFieldForConsignment();

            foreach (var par in ClientDailyParLevel.List.Where(x => x.ClientId == Client.ClientId && x.MatchDayOfWeek(DateTime.Now.DayOfWeek)))
            {
                if (par.Product == null)
                    continue;

                var detail = new OrderDetail(par.Product, 0, this);
                detail.Price = Product.GetPriceForProduct(par.Product, Client, true);
                detail.ParLevelDetail = true;

                detail.AdjustExtraFieldForParLevel(par);

                Details.Add(detail);
            }
        }

        public static Order CalculateNextDeliveryOrder(Client client)
        {
            var nextVisit = RouteEx.GetNextVisit(client.ClientId);

            var newBatch = new Batch(client);
            newBatch.Client = client;
            newBatch.Save();

            var newOrder = new Order(client) { AsPresale = true, BatchId = newBatch.Id, IsProjection = true };

            if (nextVisit != null)
            {
                newOrder.ShipDate = nextVisit.Date;

                Projection.AddProjectionValues(newOrder, nextVisit.Date);
            }

            newOrder.Save();

            return newOrder;
        }

        public bool IsScanBasedTrading { get; set; }

        public bool SplitedByDepartment { get; set; }


        public TransactionType TransactionType
        {
            get
            {
                if (IsWorkOrder)
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

        public string NameforTransactionType
        {
            get
            {
                if (IsExchange)
                    return "Exchange";

                switch (TransactionType)
                {
                    case TransactionType.SalesOrder:
                        return "Order";
                    case TransactionType.CreditOrder:
                        return "Credit";
                    case TransactionType.ReturnOrder:
                        return "Return";
                    case TransactionType.SalesInvoice:
                        return "Invoice";
                    case TransactionType.CreditInvoice:
                        return "Credit Invoice";
                    case TransactionType.ReturnInvoice:
                        return "Return Invoice";
                    case TransactionType.Quote:
                        return "Quote";
                    case TransactionType.WorkOrder:
                        return "Work Order";
                    case TransactionType.NoService:
                        return "No Service";
                    default:
                        return "-";
                }
            }
        }


        public bool IsEmpty { get { return Details.Count == 0 || (Details.Count == 1 && Details[0].Product.ProductId == Config.DefaultItem); } }

        public static Order GetProjectionForCustomer(Client client)
        {
            return Order.Orders.FirstOrDefault(x => x.AsPresale && x.Client.ClientId == client.ClientId && x.IsProjection && !x.Voided);
        }

        public string DepartmentUniqueId { get; set; }

        public ClientDepartment Department { get; set; }

        public bool LoadingError { get; set; }

        public bool IsLoadingError { get { return LoadingError || Details.Any(x => x.LoadingError); } }

        public int CompanyId { get; set; }

        public void SimoneCalculateDiscount()
        {
            Dictionary<int, List<OrderDetail>> CategoryDicc = new Dictionary<int, List<OrderDetail>>();
            Dictionary<int, List<OrderDetail>> CategoryPriceDicc = new Dictionary<int, List<OrderDetail>>();
            double CalculatedTotal = 0.0;
            foreach (var detail in this.Details)
            {
                if (detail.ExpectedPrice >= 0)
                {
                    detail.Price = detail.ExpectedPrice;
                    CalculatedTotal += detail.Qty * detail.Price;
                }

                List<OrderDetail> tCatdetail;
                if (!CategoryDicc.TryGetValue(OrderId, out tCatdetail))
                {
                    tCatdetail = new List<OrderDetail>();
                    tCatdetail.Add(detail);
                    CategoryDicc.Add(OrderId, tCatdetail);
                }
                else
                {
                    tCatdetail.Add(detail);
                    CategoryDicc[OrderId] = tCatdetail;
                }

                /*
                if (detail.Product.PriceCategoryId>0)
                {
                    List<OrderDetail> tPriceCatdetail;
                    if (!CategoryPriceDicc.TryGetValue(detail.Product.PriceCategoryId, out tPriceCatdetail))
                    {
                        tPriceCatdetail = new List<OrderDetail>();
                        tPriceCatdetail.Add(detail);
                        CategoryPriceDicc.Add(detail.Product.PriceCategoryId, tPriceCatdetail);
                    }
                    else
                    {
                        tPriceCatdetail.Add(detail);
                        CategoryPriceDicc[detail.Product.PriceCategoryId] = tPriceCatdetail;
                    }
                }
                */
                if (detail.Product.CategoryId > 0)
                {
                    List<OrderDetail> tPriceCatdetail;
                    if (!CategoryPriceDicc.TryGetValue(detail.Product.CategoryId, out tPriceCatdetail))
                    {
                        tPriceCatdetail = new List<OrderDetail>();
                        tPriceCatdetail.Add(detail);
                        CategoryPriceDicc.Add(detail.Product.CategoryId, tPriceCatdetail);
                    }
                    else
                    {
                        tPriceCatdetail.Add(detail);
                        CategoryPriceDicc[detail.Product.CategoryId] = tPriceCatdetail;
                    }
                }
            }
            var offersids = ClientOfferEx.List.Where(x => x.ClientId == Client.ClientId).Select(x => x.OfferExId).Distinct().ToList();
            var offers = OfferEx.List.Where(x => offersids.Contains(x.Id)).ToList();

            var firstoffer = offers.FirstOrDefault();
            if (firstoffer != null && !firstoffer.ExtraFields.Contains("BySubGroub=1"))//order.Client.NonVisibleExtraFields.Contains("NJ")
            {

                var Secodaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
               x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.Primary == false).ToList();

                if (Secodaryoffers.Count > 0)
                {
                    foreach (var Secodaryoffer in Secodaryoffers)
                    {
                        Dictionary<int, List<OrderDetail>> OfferProdDicc = new Dictionary<int, List<OrderDetail>>();
                        foreach (var detail in Details)
                        {
                            List<OrderDetail> tNewCatdetail;
                            if (!OfferProdDicc.TryGetValue(Secodaryoffer.Id, out tNewCatdetail))
                            {
                                tNewCatdetail = new List<OrderDetail>();
                                tNewCatdetail.Add(detail);
                                OfferProdDicc.Add(Secodaryoffer.Id, tNewCatdetail);
                            }
                            else
                            {
                                tNewCatdetail.Add(detail);
                                OfferProdDicc[Secodaryoffer.Id] = tNewCatdetail;
                            }
                        }

                        var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Secodaryoffer.Id).ToList();
                        var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();

                        foreach (var cat in OfferProdDicc)
                        {
                            double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);

                            foreach (var detail in cat.Value)
                            {
                                var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
                                foreach (var price in pricestiers)
                                {
                                    if (price.BreakQty <= qty && price.Price != 0)
                                    {
                                        double factor = 1;
                                        if (detail.UnitOfMeasure != null)
                                            factor = detail.UnitOfMeasure.Conversion;
                                        detail.Price = price.Price * factor;
                                        //detail.FromOffer = true;
                                        detail.ExtraFields = DataAccess.SyncSingleUDF("TiertType", "Tier2", detail.ExtraFields);
                                    }
                                }
                            }
                        }
                    }
                }
                //Need to Check For All offers and get the minor for every item

                var Primaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
             x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
             x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
             x.Primary == true).ToList();



                foreach (var Primaryoffer in Primaryoffers)
                {
                    if (Primaryoffer != null)
                    {
                        Dictionary<int, List<OrderDetail>> OfferProdDicc = new Dictionary<int, List<OrderDetail>>();
                        foreach (var detail in Details)
                        {
                            List<OrderDetail> tNewCatdetail;
                            if (!OfferProdDicc.TryGetValue(Primaryoffer.Id, out tNewCatdetail))
                            {
                                tNewCatdetail = new List<OrderDetail>();
                                tNewCatdetail.Add(detail);
                                OfferProdDicc.Add(Primaryoffer.Id, tNewCatdetail);
                            }
                            else
                            {
                                tNewCatdetail.Add(detail);
                                OfferProdDicc[Primaryoffer.Id] = tNewCatdetail;
                            }
                        }

                        var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Primaryoffer.Id).ToList();
                        var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();

                        foreach (var cat in OfferProdDicc)
                        {
                            double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);

                            foreach (var detail in cat.Value)
                            {
                                var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
                                foreach (var price in pricestiers)
                                {
                                    if (price.BreakQty <= qty && price.Price != 0)
                                    {
                                        if (detail.ExtraFields.Contains("Tier2"))
                                        {
                                            /* if (detail.Price > price.Price)
                                             {
                                                 double factor = 1;
                                                 if (detail.UnitOfMeasure != null)
                                                     factor = detail.UnitOfMeasure.Conversion;
                                                 detail.Price = price.Price * factor;
                                             }*/
                                        }
                                        else
                                        {
                                            double factor = 1;
                                            if (detail.UnitOfMeasure != null)
                                                factor = detail.UnitOfMeasure.Conversion;
                                            detail.Price = price.Price * factor;
                                            // detail.FromOffer = true;
                                            //  detail.ExtraFields = DataAccess.SyncSingleUDF("TiertType", "Tier2", detail.ExtraFields);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                CalculatedTotal = 0;
                foreach (var detail in this.Details)
                {
                    CalculatedTotal += detail.Qty * detail.Price;

                }

                var Discountoffers = offers.Where(x => x.OfferType == (int)OfferType.MinimumDiscount &&
               x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.Primary == true).ToList();


                if (Discountoffers.Count > 0)
                {
                    foreach (var Discountoffer in Discountoffers)
                    {
                        if (CalculatedTotal > Discountoffer.Price)
                        {
                            Dictionary<int, List<OrderDetail>> OfferProdDicc = new Dictionary<int, List<OrderDetail>>();
                            foreach (var detail in Details)
                            {
                                List<OrderDetail> tNewCatdetail;
                                if (!OfferProdDicc.TryGetValue(Discountoffer.Id, out tNewCatdetail))
                                {
                                    tNewCatdetail = new List<OrderDetail>();
                                    tNewCatdetail.Add(detail);
                                    OfferProdDicc.Add(Discountoffer.Id, tNewCatdetail);
                                }
                                else
                                {
                                    tNewCatdetail.Add(detail);
                                    OfferProdDicc[Discountoffer.Id] = tNewCatdetail;
                                }
                            }

                            var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Discountoffer.Id).ToList();
                            var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();

                            foreach (var cat in OfferProdDicc)
                            {
                                double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);

                                foreach (var detail in cat.Value)
                                {
                                    var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
                                    foreach (var price in pricestiers)
                                    {
                                        if (price.BreakQty <= qty && price.Price != 0)
                                        {
                                            double factor = 1;
                                            if (detail.UnitOfMeasure != null)
                                                factor = detail.UnitOfMeasure.Conversion;
                                            detail.Price = (detail.Price - price.Price) * factor;
                                            //detail.FromOffer = true;
                                            //detail.FromOfferType = (int)OfferType.Discount;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var cat in CategoryPriceDicc)
                {

                    var Secodaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
               x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.Primary == false).ToList();

                    if (Secodaryoffers.Count > 0)
                    {
                        foreach (var Secodaryoffer in Secodaryoffers)
                        {
                            var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Secodaryoffer.Id).ToList();

                            var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();
                            double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);
                            foreach (var detail in cat.Value)
                            {
                                var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
                                foreach (var price in pricestiers)
                                {
                                    if (price.BreakQty <= qty && price.Price != 0)
                                    {
                                        double factor = 1;
                                        if (detail.UnitOfMeasure != null)
                                            factor = detail.UnitOfMeasure.Conversion;
                                        detail.Price = price.Price * factor;
                                        //detail.FromOffer = true;
                                        detail.ExtraFields = DataAccess.SyncSingleUDF("TiertType", "Tier2", detail.ExtraFields);
                                    }
                                }
                            }
                        }
                    }


                    var Primaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
               x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
               x.Primary == true).ToList();

                    foreach (var Primaryoffer in Primaryoffers)
                    {
                        if (Primaryoffer != null)
                        {
                            var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Primaryoffer.Id).ToList();
                            var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();
                            double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);
                            foreach (var detail in cat.Value)
                            {
                                var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
                                foreach (var price in pricestiers)
                                {
                                    if (price.BreakQty <= qty && price.Price != 0)
                                    {
                                        if (detail.ExtraFields.Contains("Tier2"))
                                        {
                                            /*  if (detail.Price > price.Price)
                                              {
                                                  double factor = 1;
                                                  if (detail.UnitOfMeasure != null)
                                                      factor = detail.UnitOfMeasure.Conversion;
                                                  detail.Price = price.Price * factor;
                                              }*/
                                        }
                                        else
                                        {
                                            double factor = 1;
                                            if (detail.UnitOfMeasure != null)
                                                factor = detail.UnitOfMeasure.Conversion;
                                            detail.Price = price.Price * factor;
                                            //detail.FromOffer = true;
                                            //detail.ExtraFields = DataAccess.SyncSingleUDF("TiertType", "Tier2", detail.ExtraFields);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                CalculatedTotal = 0;
                foreach (var detail in this.Details)
                {
                    CalculatedTotal += detail.Qty * detail.Price;

                }
                foreach (var cat in CategoryPriceDicc)
                {

                    var Discountoffers = offers.Where(x => x.OfferType == (int)OfferType.MinimumDiscount &&
            x.FromDate.Date <= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
            x.ToDate.Date >= (ShipDate != DateTime.MinValue ? ShipDate : DateTime.Today) &&
            x.Primary == true).ToList();

                    if (Discountoffers.Count > 0)
                    {
                        foreach (var Discountoffer in Discountoffers)
                        {
                            if (CalculatedTotal > Discountoffer.Price)
                            {
                                var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Discountoffer.Id).ToList();
                                var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();
                                double qty = cat.Value.Where(x => countedprod.Contains(x.Product.ProductId)).Sum(x => x.Qty);

                                foreach (var detail in cat.Value)
                                {
                                    var pricestiers = prodprices.Where(x => x.ProductId == detail.Product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();

                                    foreach (var price in pricestiers)
                                    {
                                        if (price.BreakQty <= qty && price.Price != 0)
                                        {
                                            double factor = 1;
                                            if (detail.UnitOfMeasure != null)
                                                factor = detail.UnitOfMeasure.Conversion;
                                            detail.Price = price.Price * factor;
                                            detail.Price = (detail.Price - price.Price) * factor;
                                            //detail.FromOffer = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            AddDeliveryCharge();
        }

        public double GetOrderBonus()
        {
            double bonus = 0;
            var items = Details.Where(x => x.Price == 0).ToList();
            foreach (var d in items)
                bonus += (d.Product.PriceLevel0 * d.Qty);
            return bonus;
        }

        public bool AddDeliveryCharge()
        {
            var excludedFlag = DataAccess.ExplodeExtraProperties(Client.NonvisibleExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToUpper() == "EXCLUDEDELIVERYCHARGE");
            if (excludedFlag != null)
            {
                Logger.CreateLog("The client " + Client.ClientName + " has the EXCLUDEDELIVERYCHARGE flag");
                return false;
            }
            var total = this.OrderTotalCost();

            double MinOrder = Config.MinimumAmount;
            int idProdRech = Config.MinimumOrderProductId;
            Product prodRech = Product.Products.FirstOrDefault(p => p.ProductId == idProdRech);

            if (prodRech != null && Details != null && Details.Count() > 0)
            {
                var detProdR = Details.FirstOrDefault(od => od.Product.ProductId == idProdRech);
                double totProdR = (detProdR == null) ? 0 : detProdR.Qty * detProdR.Price;
                if ((total - totProdR) < MinOrder)
                {
                    float sumQty = Details.Where(od => od.Product.ProductId != idProdRech).Sum(o => o.Qty);
                    bool fromoffer = false;
                    double rechPr = Product.GetPriceForProduct(prodRech, this, out fromoffer, false, false, null);
                    if (detProdR != null)
                    {
                        detProdR.Qty = sumQty;
                        detProdR.Price = rechPr;
                        detProdR.ExpectedPrice = rechPr;
                        return true;
                    }
                    else
                    {
                        var d = new OrderDetail(prodRech, sumQty, this)
                        {
                            Qty = sumQty,
                            Price = rechPr,
                            ExpectedPrice = rechPr,
                            Product = prodRech,
                            FromOffer = false,
                            Comments = string.Concat("The order is under $", MinOrder, " and will have a $", rechPr, " per quantity delivery charge."),
                            Taxed = false,
                            TaxRate = 0,
                            Discount = 0,
                            DiscountType = 0,
                            Lot = string.Empty,
                            OriginalId = Guid.NewGuid().ToString("N"),
                            ExtraFields = string.Empty
                        };
                        AddDetail(d);
                        return true;
                    }
                }
                else if (detProdR != null)
                {
                    DeleteDetail(detProdR);
                    return true;
                }
            }
            // no modification was made
            return false;
        }

        public bool HasDisolSurvey { get; set; }

        public bool ReadyToFinalize { get { return Details.All(x => x.ReadyToFinalize); } }

        public int PriceLevelId { get; set; }

        public int QuoteId { get; set; }

        public bool ContainsAsset()
        {
            foreach (var item in Details)
            {
                var units = DataAccess.GetSingleUDF("units per crate", item.Product.NonVisibleExtraFieldsAsString);

                if (string.IsNullOrEmpty(units))
                    continue;

                int unitsPerCrate = 0;
                Int32.TryParse(units, out unitsPerCrate);

                if (unitsPerCrate > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckOrderLengthsBeforeSending()
        {
            try
            {
                bool wasModified = false;

                if (!string.IsNullOrEmpty(Comments) && Comments.Length > 2000)
                {
                    var comments = Comments.Substring(0, 2000);
                    this.Comments = comments;
                    wasModified = true;
                }

                if (!string.IsNullOrEmpty(SignatureName) && SignatureName.Length > 200)
                {
                    var signatureName = SignatureName.Substring(0, 200);
                    this.SignatureName = signatureName;
                    wasModified = true;
                }

                if (!string.IsNullOrEmpty(DiscountComment) && DiscountComment.Length > 200)
                {
                    var discountComment = DiscountComment.Substring(0, 200);
                    this.DiscountComment = discountComment;
                    wasModified = true;
                }

                if (!string.IsNullOrEmpty(CompanyName) && CompanyName.Length > 100)
                {
                    var cname = CompanyName.Substring(0, 100);
                    this.CompanyName = cname;
                    wasModified = true;
                }

                if (!string.IsNullOrEmpty(PrintedOrderId) && PrintedOrderId.Length > 50)
                {
                    var pid = PrintedOrderId.Substring(0, 50);
                    this.PrintedOrderId = pid;
                    wasModified = true;
                }

                if (!string.IsNullOrEmpty(ExtraFields) && ExtraFields.Length > 2000)
                {
                    var extraFields = ExtraFields.Substring(0, 2000);
                    this.ExtraFields = extraFields;
                    wasModified = true;
                }

                foreach (var d in Details)
                {
                    if (!string.IsNullOrEmpty(d.Comments) && d.Comments.Length > 2000)
                    {
                        var c = d.Comments.Substring(0, 2000);
                        d.Comments = c;
                        wasModified = true;
                    }

                    if (!string.IsNullOrEmpty(d.Lot) && d.Lot.Length > 50)
                    {
                        var lot = d.Lot.Substring(0, 50);
                        d.Lot = lot;
                        wasModified = true;
                    }

                    if (!string.IsNullOrEmpty(d.ExtraFields) && d.ExtraFields.Length > 2000)
                    {
                        var extra = d.ExtraFields.Substring(0, 2000);
                        d.ExtraFields = extra;
                        wasModified = true;
                    }
                }

                if (wasModified)
                    this.Save();
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error checking lengths on order" + ex.ToString());
            }
        }

        public bool CanAddDiscount(double percentage, DiscountType type)
        {
            if (Config.MaxDiscountPerOrder == 0)
                return true;

            if (Config.MaxDiscountPerOrder > 0)
            {
                var orderItemsDiscount = CalculateItemDiscount();
                var orderTotal = CalculateItemCost();

                double new_disc_amount = 0;
                if (type == DiscountType.Percent)
                    new_disc_amount = ((percentage / 100) * orderTotal);
                else
                    new_disc_amount = percentage;

                var total = orderItemsDiscount + new_disc_amount;

                double new_percentage = 0;
                new_percentage = Math.Round(((total / orderTotal) * 100), Config.Round);

                if (new_percentage <= (double)Config.MaxDiscountPerOrder)
                    return true;
                else
                    return false;

            }

            return true;
        }

        public bool CanAddLineDiscount(double percentage, DiscountType type, double qty, double price, OrderDetail od)
        {
            if (Config.MaxDiscountPerOrder == 0)
                return true;

            if (Config.MaxDiscountPerOrder > 0)
            {
                var orderTotalDiscount = CalculateDiscount(od);
                var orderTotal = CalculateItemCost();

                double new_disc_amount = percentage;

                var totalForItem = price * qty;

                if (type == DiscountType.Amount)
                {
                    new_disc_amount *= qty;
                }
                else if (type == DiscountType.Percent)
                    new_disc_amount = (percentage / 100) * totalForItem;

                var total = orderTotalDiscount + new_disc_amount;

                double new_percentage = 0;
                new_percentage = Math.Round(((total / orderTotal) * 100), Config.Round);

                if (new_percentage <= (double)Config.MaxDiscountPerOrder)
                    return true;
                else
                    return false;

            }

            return true;
        }
        public bool ValidateOrderMinimum()
        {
            if (Config.OrderMinQtyMinAmount && OrderType != OrderType.Credit && TransactionType == TransactionType.SalesOrder || TransactionType == TransactionType.SalesInvoice)
            {
                double minOrder = 0;
                int miniumExtrafield = 0;
                var min = DataAccess.GetSingleUDF("MinimumOrderAmount", Client.ExtraPropertiesAsString);

                if (Client.MinimumOrderAmount > 0)
                    minOrder = Client.MinimumOrderAmount;
                else if (!string.IsNullOrEmpty(min))
                    minOrder = double.Parse(min);

                if (minOrder > 0)
                {
                    var totalOrder = OrderTotalCost();

                    if (totalOrder < minOrder)
                    {
                        // string message = string.Format(context.GetString(Resource.String.minimumOrderAmount),
                        //     Client.ClientName.ToString(), minOrder.ToString(), totalOrder.ToString());
                        // ActivityExtensionMethods.DisplayDialog(context, context.GetString(Resource.String.alert), message);
                        return false;
                    }

                }
                var minOrderQtyString = DataAccess.GetSingleUDF("MinimumOrderQty", Client.ExtraPropertiesAsString);



                double minQty = 0;
                if (this.Client.MinimumOrderQty > 0)
                    minQty = this.Client.MinimumOrderQty;
                else if (this.Client.MinOrderQty > 0)
                    minQty = this.Client.MinOrderQty;
                else if (!string.IsNullOrEmpty(minOrderQtyString))
                    minQty = int.Parse(minOrderQtyString);

                if (minQty > 0)
                {
                    var qtyTotal = 0;
                    if (this.Details != null)
                    {
                        float sum = 0;

                        foreach (var item in this.Details)
                        {
                            var qty = item.Qty;
                            if (item.Product.IsDiscountItem)
                                continue;

                            if (item.IsCredit)
                                continue;
                            if (item.UnitOfMeasure != null)
                            {
                                qty *= item.UnitOfMeasure.Conversion;
                            }
                            if (!item.SkipDetailQty(this))
                                sum += qty;
                        }

                        qtyTotal = int.Parse(sum.ToString(CultureInfo.CurrentCulture));
                    }
                    if (qtyTotal < minQty)
                    {
                        // string message = string.Format(context.GetString(Resource.String.minimumOrderQuantity), Client.ClientName.ToString(), minQty.ToString(), qtyTotal.ToString());
                        // ActivityExtensionMethods.DisplayDialog(context, context.GetString(Resource.String.alert), message);
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class SuperOrder
    {
        public Order Sales { get; set; }

        public Order Credit { get; set; }

        public bool AsPresale { get { return Sales.AsPresale; } }

        public Client Client { get { return Sales.Client; } }

        public bool Locked()
        {
            return Sales.Locked() || Credit.Locked();
        }

        public bool IsDelivery { get { return Sales.IsDelivery; } }

        public bool Voided { get { return Sales.Voided && Credit.Voided; } }

        public bool IsProjection { get { return Sales.IsProjection; } }

        public void Save()
        {
            Sales.Save();
            Credit.Save();
        }

        public void DeleteDetail(OrderDetail orderDetail)
        {
            if (orderDetail.IsCredit)
                Credit.DeleteDetail(orderDetail);
            else
                Sales.DeleteDetail(orderDetail);
        }

        public bool RecalculateDiscounts()
        {
            return Sales.RecalculateDiscounts() && Credit.RecalculateDiscounts();
        }

        public void AddDetail(OrderDetail orderDetail)
        {
            if (orderDetail.IsCredit)
                Credit.AddDetail(orderDetail);
            else
                Sales.AddDetail(orderDetail);
        }

        public void UpdateInventory(OrderDetail orderDetail, int factor)
        {
            if (orderDetail.IsCredit)
                Credit.UpdateInventory(orderDetail, factor);
            else
                Sales.UpdateInventory(orderDetail, factor);
        }

        public OrderDetail FindDetail(int id)
        {
            OrderDetail det = Sales.Details.FirstOrDefault(x => x.OrderDetailId == id);
            if (det == null)
                det = Credit.Details.FirstOrDefault(x => x.OrderDetailId == id);

            return det;
        }
    }
}
