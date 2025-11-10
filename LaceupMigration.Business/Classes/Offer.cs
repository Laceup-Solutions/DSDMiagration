using System;
using System.Linq;
using System.Collections.Generic;

namespace LaceupMigration
{

    public enum OfferType
    {
        Price = 0,
        QtyQty = 1,
        QtyPrice = 2,
        NewItem = 3,
        QtyQtyPrice = 4,
        Discount = 5,
        DiscountQty = 6,
        DiscountAmount = 7,
        TieredPricing = 8,
        MinimumDiscount = 9
    };

    public class Offer
    {

        public int OfferId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public OfferType Type { get; set; }

        public int ProductId { get; set; }

        public float MinimunQty { get; set; }

        public double Price { get; set; }

        public float FreeQty { get; set; }

        public bool ClienBased { get; set; }

        public static int Count { get { return offers.Count; } }

        public Product Product { get; set; }

        public int UnitOfMeasureId { get; set; }

        public string ExtraFields { get; set; }
        public string OriginalId { get; set; }

        public static void Clear(int count)
        {
            offers.Clear();
            if (count > 0)
                offers.Capacity = count;
        }

        public static IEnumerable<Offer> OfferList { get { return offers; } }

        static List<Offer> offers = new List<Offer>();

        public static IEnumerable<Offer> GetOffersVisibleToClient(OfferType offerType, Client client)
        {
            foreach (Offer offer in offers)
                if (offer.Type == offerType)
                    if (!offer.ClienBased)
                        yield return offer;
                    else if (ClientsOffer.IsOfferVisibleToClient(offer, client))
                        yield return offer;
            yield break;
        }

        public static IList<Offer> GetOffersVisibleToClient(Client client, bool includeDiscount = false)
        {
            try
            {
                if (client.AvailableOffers != null)
                    return client.AvailableOffers.Where(x => includeDiscount || x.Type != OfferType.Discount).ToList();

                client.AvailableOffers = new List<Offer>();
                DateTime now = DateTime.Now;
                foreach (Offer offer in offers)
                    if ((!offer.ClienBased) && (offer.ToDate > now) && (offer.FromDate < now))
                        client.AvailableOffers.Add(offer);
                    else if ((offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now) && ClientsOffer.IsOfferVisibleToClient(offer, client))
                        client.AvailableOffers.Add(offer);
                return client.AvailableOffers.Where(x => includeDiscount || x.Type != OfferType.Discount).ToList();
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Offers called but Still downloading offers in background" + ex.ToString());
                return new List<Offer>();
            }
        }

        public Offer ProductHasOffer(Product product)
        {
            try
            {
                foreach (var offer in OfferList)
                {
                    if (offer.ProductId == product.ProductId)
                    {
                        return offer;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static void AddOffer(Offer offer)
        {
            offers.Add(offer);
        }

        //public static bool ProductHasSpecialPriceForClient(Product product, Client client, out double price)
        //{
        //    foreach (var offer in GetOffersVisibleToClient(client))
        //        if (offer.Product != null && offer.Product.ProductId == product.ProductId && offer.Type == OfferType.Price && (offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now))
        //        {
        //            price = offer.Price;
        //            return true;
        //        }
        //    price = 0;
        //    return false;
        //}

        public static string ReturnOfferPromoName(Product product, Client client, UnitOfMeasure unitOfMeasure = null)
        {
            var offersForClient = GetOffersVisibleToClient(client).Where(x => x.ProductId == product.ProductId
            && x.Type == OfferType.Price && (x.ToDate > DateTime.Now) && (x.FromDate < DateTime.Now)).OrderBy(x => x.Price);

            if (unitOfMeasure == null)
            {
                var selectedOffer = offersForClient.FirstOrDefault();
                if (selectedOffer == null)
                {
                    return string.Empty;
                }

                return "Offer: " + selectedOffer.Price.ToCustomString();
            }
            else
            {
                var selectedOffer = offersForClient.FirstOrDefault(x => x.UnitOfMeasureId == unitOfMeasure.Id);

                if (selectedOffer == null)
                {
                    return string.Empty;
                }

                return "Offer: " + selectedOffer.Price.ToCustomString() + " in " + unitOfMeasure.Name;
            }

        }

        public static bool ProductHasSpecialPriceForClient(Product product, Client client, out double price, UnitOfMeasure unitOfMeasure = null)
        {
            var offersForClient = GetOffersVisibleToClient(client).Where(x => x.ProductId == product.ProductId
            && x.Type == OfferType.Price && (x.ToDate > DateTime.Now) && (x.FromDate < DateTime.Now)).OrderBy(x => x.Price);

            if (unitOfMeasure == null)
            {
                var selectedOffer = offersForClient.FirstOrDefault();
                if (selectedOffer == null)
                {
                    price = 0;
                    return false;
                }

                price = selectedOffer.Price;

                if (selectedOffer.UnitOfMeasureId != 0)
                {
                    var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == selectedOffer.UnitOfMeasureId);
                    if (uom != null && !uom.IsBase)
                        price /= uom.Conversion;
                }

                return true;
            }
            else
            {
                var selectedOffer = offersForClient.FirstOrDefault(x => x.UnitOfMeasureId == unitOfMeasure.Id);

                if (selectedOffer == null)
                {
                    if (!ProductHasSpecialPriceForClient(product, client, out price))
                        return false;

                    price *= unitOfMeasure.Conversion;
                    return true;
                }

                price = selectedOffer.Price;
                return true;
            }

        }

        //public static bool ProductHasSpecialPriceForClient(Product product, Client client, out double price, UnitOfMeasure unitOfMeasure = null)
        //{
        //    price = double.MaxValue;
        //    float conversion = 1;

        //    var offersForClient = GetOffersVisibleToClient(client).Where(x => x.ProductId == product.ProductId && x.Type == OfferType.Price && (x.ToDate > DateTime.Now) && (x.FromDate < DateTime.Now));
        //    Offer defaultOffer = null;
        //    if (unitOfMeasure != null)
        //        defaultOffer = offersForClient.FirstOrDefault(x => x.UnitOfMeasureId == unitOfMeasure.Id);
        //    if (defaultOffer != null)
        //    {
        //        price = defaultOffer.Price / unitOfMeasure.Conversion;
        //        conversion = unitOfMeasure.Conversion;
        //    }

        //    foreach (var offer in offersForClient)
        //    {
        //        if (unitOfMeasure != null)
        //        {
        //            var offerUoM = UnitOfMeasure.List.Find(x => x.Id == offer.UnitOfMeasureId);
        //            if (offerUoM == null)
        //                continue;

        //            var familyUoM = UnitOfMeasure.List.Where(x => x.FamilyId == offerUoM.FamilyId).ToList();
        //            //get the base
        //            var baseUoM = familyUoM.FirstOrDefault(x => x.IsBase);

        //            var tempPrice = offer.Price / offerUoM.Conversion;
        //            if (Math.Round(tempPrice, 2) < Math.Round(price, 2))
        //            {
        //                price = tempPrice;
        //                conversion = offerUoM.Conversion;
        //            }
        //        }
        //        else if (offer.Price < price)
        //        {
        //            price = offer.Price;

        //            var offerUoM = UnitOfMeasure.List.Find(x => x.Id == offer.UnitOfMeasureId);
        //            if(offerUoM != null)
        //            {
        //               price = offer.Price / offerUoM.Conversion;
        //            }
        //        }
        //    }
        //    if (price == double.MaxValue)
        //    {
        //        price = 0;
        //        return false;
        //    }
        //    else
        //    {
        //        price *= unitOfMeasure == null ? 1 : unitOfMeasure.Conversion;
        //        return true;
        //    }
        //}

        public static double BestPrice(Product product, Client client, double qty)
        {
            double ret = 0;
            double currentQty = 0;
            foreach (var offer in OfferList.Where(x => x.ProductId == product.ProductId && x.Type == OfferType.QtyPrice))
            {
                if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                {
                    if (currentQty < offer.MinimunQty && offer.MinimunQty <= qty)
                    {
                        ret = offer.Price;
                        currentQty = offer.MinimunQty;
                    }
                }
            }
            return ret;
        }

        #region Statewide CRAP

        public static Offer GetOfferToCheckAllowance(Product prod, Client client, float qty, UnitOfMeasure unitOfMeasure)
        {
            if (offers.All(x => x.UnitOfMeasureId == 0))
                return GetOfferForProductQtyForAllowance(prod, client, qty);

            Offer bestOffer = null;

            var baseQty = qty;
            if (unitOfMeasure != null)
                baseQty *= unitOfMeasure.Conversion;

            foreach (Offer offer in offers)
            {
                if (offer.ProductId == prod.ProductId && (offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now))
                    if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                    {
                        var offer_min = offer.MinimunQty;
                        var offer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offer_uom != null)
                            offer_min *= offer_uom.Conversion;

                        if (bestOffer == null && offer_min <= baseQty)
                            bestOffer = offer;
                        else if (bestOffer != null)
                        {
                            var bestOffer_min = bestOffer.MinimunQty;
                            var bestOffer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == bestOffer.UnitOfMeasureId);
                            if (bestOffer_uom != null)
                                bestOffer_min *= bestOffer_uom.Conversion;

                            if (offer_min <= baseQty && offer_min > bestOffer_min)
                                bestOffer = offer;
                        }
                    }
            }
            return bestOffer;
        }

        public static Offer GetOfferForProductQtyForAllowance(Product prod, Client client, float qty)
        {
            Offer bestOffer = null;

            foreach (Offer offer in offers)
            {
                if (offer.Type == OfferType.NewItem)
                    continue;

                if (offer.ProductId == prod.ProductId && (offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now))
                    if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                    {
                        if (bestOffer == null && offer.MinimunQty <= qty)
                            bestOffer = offer;
                        else if (bestOffer != null)
                        {
                            if (offer.MinimunQty <= qty && offer.MinimunQty > bestOffer.MinimunQty)
                                bestOffer = offer;
                        }
                    }
            }
            return bestOffer;
        }

        #endregion

        public static Offer GetOfferForProductQty(Product prod, Client client, float qty, UnitOfMeasure unitOfMeasure)
        {
            if (offers.All(x => x.UnitOfMeasureId == 0))
                return GetOfferForProductQty(prod, client, qty);

            Offer bestOffer = null;

            var baseQty = qty;
            if (unitOfMeasure != null)
                baseQty *= unitOfMeasure.Conversion;

            foreach (Offer offer in offers)
            {
                if (offer.Type == OfferType.Price && !Config.DontCalculateOffersAfterPriceChanged)
                    continue;

                if (offer.Type == OfferType.NewItem)
                    continue;

                if (offer.ProductId == prod.ProductId && (offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now))
                    if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                    {
                        var offer_min = offer.MinimunQty;
                        var offer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offer_uom != null)
                            offer_min *= offer_uom.Conversion;

                        if (bestOffer == null && offer_min <= baseQty)
                            bestOffer = offer;
                        else if (bestOffer != null)
                        {
                            var bestOffer_min = bestOffer.MinimunQty;
                            var bestOffer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == bestOffer.UnitOfMeasureId);
                            if (bestOffer_uom != null)
                                bestOffer_min *= bestOffer_uom.Conversion;

                            var offer_UOM = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);

                            var offerPrice = offer.Price;
                            var bestOfferPrice = bestOffer.Price;

                            if(offer_UOM != null)
                                offerPrice /= offer_UOM.Conversion;

                            if(bestOffer_uom != null)
                                bestOfferPrice /= bestOffer_uom.Conversion;

                            //panamerican => makes no senses but somehow they have an offer with higher min qty but less price :( so will check price too to give best offer
                            //(offer_min <= baseQty && offer_min > bestOffer_min) -> old code
                            if (offer_min <= baseQty && bestOfferPrice > offerPrice)
                                bestOffer = offer;
                        }
                    }
            }
            return bestOffer;
        }


        //future dev
        public static Offer GetOfferForProductQty(Product prod, Client client, float qty, UnitOfMeasure unitOfMeasure, DateTime shipdate)
        {
            DateTime dateToUse = DateTime.Now;


            if (shipdate != DateTime.MinValue && shipdate != DateTime.MaxValue)
                dateToUse = shipdate;

            if (offers.All(x => x.UnitOfMeasureId == 0))
                return GetOfferForProductQty(prod, client, qty);

            Offer bestOffer = null;

            var baseQty = qty;
            if (unitOfMeasure != null)
                baseQty *= unitOfMeasure.Conversion;

            foreach (Offer offer in offers)
            {
                if (offer.Type == OfferType.Price)
                    continue;

                if (offer.Type == OfferType.NewItem)
                    continue;

                if (offer.ProductId == prod.ProductId && (offer.ToDate > dateToUse) && (offer.FromDate < dateToUse))
                    if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                    {
                        var offer_min = offer.MinimunQty;
                        var offer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == offer.UnitOfMeasureId);
                        if (offer_uom != null)
                            offer_min *= offer_uom.Conversion;

                        if (bestOffer == null && offer_min <= baseQty)
                            bestOffer = offer;
                        else if (bestOffer != null)
                        {
                            var bestOffer_min = bestOffer.MinimunQty;
                            var bestOffer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == bestOffer.UnitOfMeasureId);
                            if (offer_uom != null)
                                bestOffer_min *= bestOffer_uom.Conversion;

                            if (offer_min <= baseQty && offer_min > bestOffer_min)
                                bestOffer = offer;
                        }
                    }
            }
            return bestOffer;
        }

        public static Offer GetOfferForProductQty(Product prod, Client client, float qty)
        {
            Offer bestOffer = null;

            foreach (Offer offer in offers)
            {
                if (offer.Type == OfferType.Price)
                    continue;

                if (offer.Type == OfferType.NewItem)
                    continue;

                if (offer.ProductId == prod.ProductId && (offer.ToDate > DateTime.Now) && (offer.FromDate < DateTime.Now))
                    if (!offer.ClienBased || ClientsOffer.IsOfferVisibleToClient(offer, client))
                    {
                        if (bestOffer == null && offer.MinimunQty <= qty)
                            bestOffer = offer;
                        else if (bestOffer != null)
                        {
                            if (offer.MinimunQty <= qty && offer.MinimunQty > bestOffer.MinimunQty)
                                bestOffer = offer;
                        }
                    }
            }
            return bestOffer;
        }

        public override string ToString()
        {
            UnitOfMeasure uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == UnitOfMeasureId);
            string uomString = string.Empty;

            if (uom != null)
                uomString = uom.Name;

            if (Type == OfferType.QtyQty)
                return string.Format("Buy {0}{1} Get {2}{1} Free. ", MinimunQty, uomString, FreeQty);

            if (Type == OfferType.QtyPrice)
                return string.Format("Buy {0}{1} at {2}. ", MinimunQty, uomString, Price.ToCustomString());

            if (Type == OfferType.Price)
                return string.Format("Buy at {0}. ", Price.ToCustomString());

            if (Type == OfferType.QtyQtyPrice)
                return string.Format("Buy {0}{1} Get {2}{1} At {3}. ", MinimunQty, uomString, FreeQty, Price);

            return string.Empty;
        }

        public float GetBaseMinQty()
        {
            var offer_min = MinimunQty;
            var offer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == UnitOfMeasureId);
            if (offer_uom != null)
                offer_min *= offer_uom.Conversion;

            return offer_min;
        }

        public float GetBaseFreeQty()
        {
            var offer_free = FreeQty;
            var offer_uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == UnitOfMeasureId);
            if (offer_uom != null)
                offer_free *= offer_uom.Conversion;

            return offer_free;
        }
    }
}

