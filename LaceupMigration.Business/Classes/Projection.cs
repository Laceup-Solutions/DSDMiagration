using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class Projection
    {
        public int ClientId { get; set; }

        public int ProductId { get; set; }

        public double Sunday { get; set; }

        public double Monday { get; set; }

        public double Tuesday { get; set; }

        public double Wednesday { get; set; }

        public double Thursday { get; set; }

        public double Friday { get; set; }

        public double Saturday { get; set; }

        static List<Projection> list = new List<Projection>();

        public static List<Projection> List { get{ return list; } }

        public static void AddProjectionValues(Order order, DateTime nextVisit)
        {
            var values = list.Where(x => x.ClientId == order.Client.ClientId);

            int days = (int)nextVisit.Subtract(DateTime.Now).TotalDays;

            foreach (var item in values)
            {
                var product = Product.Find(item.ProductId);
                if(product == null)
                {
                    Logger.CreateLog("Generate projection. Product with ID=" + item.ProductId + " not found");
                    continue;
                }

                if (product.IsRelatedProduct)
                    continue;

                var qty = item.GetNextTotalValue(days);

                UnitOfMeasure uom = null;

                if(!string.IsNullOrEmpty(product.UoMFamily))
                    uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == product.UoMFamily && x.IsDefault);

                var price = Product.GetPriceForProduct(product, order.Client, true);

                if (uom != null)
                {
                    qty /= uom.Conversion;
                    price *= uom.Conversion;
                }

                if (product.CaseCount != 1)
                    qty = (float)(Math.Round(qty / product.CaseCount, MidpointRounding.AwayFromZero) * product.CaseCount);

                if (qty == 0)
                    continue;

                var detail = new OrderDetail(product, qty, order);
                detail.UnitOfMeasure = uom;
                detail.Price = price;

                order.AddDetail(detail);

                var related = GetRelatedItem(order, detail);

                if (related != null)
                    order.AddDetail(related);
            }
        }

        static OrderDetail GetRelatedItem(Order order, OrderDetail detail)
        {
            // check if the prod has related
            bool useRelated = true;
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "USERELATED");
                if (vendor != null && (vendor.Item2.ToUpperInvariant() == "NO" || vendor.Item2.ToUpperInvariant() == "0"))
                {
                    useRelated = false;
                }
            }

            if (useRelated && detail.Product.ExtraPropertiesAsString.Length > 0)
            {
                foreach (var p in detail.Product.ExtraProperties)
                    // this product requires a related
                    if (p.Item1.ToLowerInvariant() == "relateditem")
                    {
                        // add the related
                        if (detail.RelatedOrderDetail == 0)
                        {
                            var relatedProduct = Product.Find(Convert.ToInt32(p.Item2), true);
                            if (relatedProduct == null)
                            {
                                Logger.CreateLog("Related item of product " + Convert.ToInt32(p.Item2) + " was not found");
                                return null;
                            }
                            else
                            {
                                var relatedDetail = new OrderDetail(relatedProduct, detail.Qty, order);
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

                                detail.RelatedOrderDetail = relatedDetail.OrderDetailId;

                                return relatedDetail;
                            }
                        }
                    }
            }

            return null;
        }

        float GetNextTotalValue(int days)
        {
            double total = 0;

            for (int i = 1; i <= days +1; i++)
            {
                var today = (int)DateTime.Now.DayOfWeek;

                var next = today + i;

                if (next >= 7)
                    next -= 7;

                switch ((DayOfWeek)next)
                {
                    case DayOfWeek.Sunday:
                        total += Sunday;
                        break;
                    case DayOfWeek.Monday:
                        total += Monday;
                        break;
                    case DayOfWeek.Tuesday:
                        total += Tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        total += Wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        total += Thursday;
                        break;
                    case DayOfWeek.Friday:
                        total += Friday;
                        break;
                    case DayOfWeek.Saturday:
                        total += Saturday;
                        break;
                    default:
                        break;
                }
            }

            return (float)Math.Round(total, 0);
        }
    }
}