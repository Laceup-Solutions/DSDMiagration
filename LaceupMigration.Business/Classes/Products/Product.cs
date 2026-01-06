
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using System.Security.Cryptography;
using System.Text;



namespace LaceupMigration
{

    public enum ProductType
    {
        Inventory = 0,
        NonInventory = 1,
        Discount = 2,
        Asset = 3
    };

    /// <summary>
    /// Defines a Product in the system
    /// </summary>
    public class Product
    {

        IList<Tuple<string, string>> noVisibleExtraProperties;
        IList<Tuple<string, string>> extraProperties;

        public int ProductId { get; set; }

        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Upc")]
        public string Upc { get; set; }

        public string Sku { get; set; }

        public double PalletSize { get; set; }

        public bool IsExpense
        {
            get
            {
                return !string.IsNullOrEmpty(NonVisibleExtraFieldsAsString) && NonVisibleExtraFieldsAsString.Contains("expenseproduct=1");
            }
        }
        public string Comment { get; set; }

        string package = "";
        public string Package
        {
            get
            {
                try
                {
                    int pkg = Convert.ToInt32(package);
                    return package;
                }
                catch
                {
                    //Logger.CreateLog("Package in invalid format");
                    return "1";
                }
            }
            set
            {
                package = value;
            }
        }

        public bool IsDiscountItem
        {
            get
            {
                if (!OrderDiscount.HasDiscounts)
                    return false;

                return (ProductType == ProductType.Discount || (ExtraPropertiesAsString?.Contains("ItemType=Discount") ?? false));
            }
        }

        public double TemplateMinimumQty
        {
            get
            {
                double qty = 0;

                if (!string.IsNullOrEmpty(NonVisibleExtraFieldsAsString))
                {
                    var value = UDFHelper.GetSingleUDF("templateMinimumQty", NonVisibleExtraFieldsAsString);
                    if (!string.IsNullOrEmpty(value))
                        double.TryParse(value, out qty);
                }

                return qty;
            }
        }

        public double TemplateMultipleQty
        {
            get
            {
                double qty = 0;

                if (!string.IsNullOrEmpty(NonVisibleExtraFieldsAsString))
                {
                    var value = UDFHelper.GetSingleUDF("templateMultipleQty", NonVisibleExtraFieldsAsString);
                    if (!string.IsNullOrEmpty(value))
                        double.TryParse(value, out qty);
                }

                return qty;
            }
        }

        public int SplitProduct
        {
            get
            {
                if (string.IsNullOrEmpty(ExtraPropertiesAsString))
                    return -1;

                var extraFields = UDFHelper.GetSingleUDF("split", ExtraPropertiesAsString.ToLower());
                if (!string.IsNullOrEmpty(extraFields))
                {
                    int toReturn = -1;

                    Int32.TryParse(extraFields, out toReturn);
                    return toReturn;
                }

                return -1;
            }
        }

        public bool FixedWeight
        {
            get
            {
                bool isFixed = false;

                if (!string.IsNullOrEmpty(ExtraPropertiesAsString))
                {
                    var value = UDFHelper.GetSingleUDF("fixedweight", ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(value) && value == "yes")
                        isFixed = true;
                }

                return isFixed;
            }
        }
        public string Description { get; set; }

        public bool UseDefaultUOM
        {
            get
            {
                if (string.IsNullOrEmpty(NonVisibleExtraFieldsAsString))
                    return false;

                var value = UDFHelper.GetSingleUDF("useonlydefaultuom", NonVisibleExtraFieldsAsString);
                if (!string.IsNullOrEmpty(value) && value == "1")
                    return true;
                else
                    return false;
            }
        }

        public double PriceLevel0 { get; set; }

        public double PriceLevel1 { get; set; }

        public double PriceLevel2 { get; set; }

        public double PriceLevel3 { get; set; }

        public double PriceLevel4 { get; set; }

        public double PriceLevel5 { get; set; }

        public double PriceLevel6 { get; set; }

        public double PriceLevel7 { get; set; }

        public double PriceLevel8 { get; set; }

        public double LowestAcceptablePrice { get; set; }

        public int CategoryId { get; set; }

        public string OriginalId { get; set; }

        public string UoMFamily { get; set; }

        public bool SoldByWeight { get; set; }

        public double Weight { get; set; }

        public string Code { get; set; }

        public double Cost { get; set; }

        public int OrderInCategory { get; set; }

        public string NonVisibleExtraFieldsAsString { get; set; }

        public string WarehouseLocation { get; set; }

        public int DiscountCategoryId { get; set; }

        public int PriceCategoryId { get; set; }

        public int VendorId { get; set; }
        public string DefaultUomName { get; set; }
        public static bool IsSuggestedForClient(Client client, Product product)
        {
            if (client == null)
                return false;

            if (SuggestedClientCategory.List.Count > 0)
            {
                var suggestedForthisCLient = SuggestedClientCategory.List.FirstOrDefault(x => x.SuggestedClientCategoryClients.Any(y => y.ClientId == client.ClientId));

                if (suggestedForthisCLient != null)
                {
                    return suggestedForthisCLient.SuggestedClientCategoryProducts.Any(x => x.ProductId == product.ProductId);
                }
                else
                    return false;
            }
            else
                return false;
        }

        public IList<Tuple<string, string>> NonVisibleExtraFields
        {
            get
            {
                if (noVisibleExtraProperties == null)
                {
                    noVisibleExtraProperties = new List<Tuple<string, string>>();
                    if (NonVisibleExtraFieldsAsString == null || NonVisibleExtraFieldsAsString.Length == 0)
                        return noVisibleExtraProperties;
                    
                    noVisibleExtraProperties = UDFHelper.ExplodeExtraPropertiesTuple(NonVisibleExtraFieldsAsString);
                }
                return noVisibleExtraProperties;
            }
        }

        public IList<Tuple<string, string>> ExtraProperties
        {
            get
            {
                if (extraProperties == null)
                {
                    extraProperties = new List<Tuple<string, string>>();
                    if (ExtraPropertiesAsString == null || ExtraPropertiesAsString.Length == 0)
                        return extraProperties;
                    
                    extraProperties = UDFHelper.ExplodeExtraPropertiesTuple(ExtraPropertiesAsString);
                }
                return extraProperties;
            }
        }

        public string ExtraPropertiesAsString
        {
            get;
            set;

        }

        public bool Taxable { get; set; }

        public double TaxRate { get; set; }

        public ProductType ProductType { get; set; }

        public bool HasLots
        {
            get
            {
                return lots != null && !string.IsNullOrEmpty(lots[0]);
            }
        }

        public string[] Lots
        {
            get
            {
                if (lots == null)
                    lots = new string[3];
                return lots;
            }
        }

        private string[] lots;

        public void AddLot(string lot)
        {

            if (lots == null)
                lots = new string[3];

            var added = new List<string>();
            foreach (var l in lots)
                if (l != null && l != lot)
                    added.Add(l);
            added.Insert(0, lot);

            lots = added.ToArray();
        }

        public static IList<Product> Products
        {
            get { return products.Values.ToList(); }
        }

        static Dictionary<int, Product> products = new Dictionary<int, Product>();
        static Dictionary<int, List<Product>> productsInCategories = new Dictionary<int, List<Product>>();

        public static Product Find(int productId, bool includeCategoryDefault = false)
        {
            if (includeCategoryDefault)
                return Products.FirstOrDefault(x => x != null && x.ProductId == productId);
            return Products.FirstOrDefault(x => x != null && x.ProductId == productId && x.CategoryId > 0);
        }

        public static void AddProduct(Product product)
        {
            if (products.ContainsKey(product.ProductId))
            {
                Logger.CreateLog("Product not added because the id was already in the dictionary. Id:" + product.ProductId);
                return;
            }
            products.Add(product.ProductId, product);
        }
        
        public static void UpdateProduct(Product product)
        {
            products[product.ProductId] = product;
        }
        
        public static void Clear()
        {
            adjustmentProducts.Clear();
            rotatedProducts.Clear();
            coreProducts.Clear();
            products.Clear();
            productsInCategories.Clear();
        }

        public static double GetPriceForProduct(Product product, Order order, bool isCredit, bool damaged, bool useConfig = true)
        {
            bool cameFromOffer = false;

            if (order.OrderType == OrderType.Sample)
                return 0;

            double basePrice = CalculatePriceForProduct(product, order.Client, isCredit, damaged, null, true, out cameFromOffer, useConfig, order);

            return Math.Round(basePrice, Config.Round);
        }

        public static double GetPriceForProduct(Product product, Client client, bool useOffer, bool useConfig = true)
        {
            bool cameFromOffer = false;
            return CalculatePriceForProduct(product, client, false, false, null, useOffer, out cameFromOffer, useConfig, null);
        }

        public static double GetPriceForProduct(Product product, Order order, out bool cameFromOffer, bool asCredit, bool damaged = false, UnitOfMeasure unitOfMeasure = null)
        {
            cameFromOffer = false;

            if (order.OrderType == OrderType.Sample)
                return 0;

            double basePrice = CalculatePriceForProduct(product, order.Client, asCredit, damaged, unitOfMeasure, true, out cameFromOffer, false, order);

            return Math.Round(basePrice, Config.Round);
        }

        public static ProductPrice GetProductPriceForProduct(Order order, Client client, Product product)
        {
            var temp_prodPrice = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId).ToList();

            if (client != null)
                temp_prodPrice = temp_prodPrice.Where(x => x.PriceLevelId == client.PriceLevel).ToList();

            if (order != null && order.PriceLevelId > -1)
                temp_prodPrice = temp_prodPrice.Where(x => x.PriceLevelId == order.PriceLevelId).ToList();

            foreach (var pp in temp_prodPrice)
            {
                if (order != null && order.PriceLevelId > -1)
                {
                    if (order.PriceLevelId == pp.PriceLevelId && pp.ProductId == product.ProductId)
                    {
                        return pp;
                    }
                }
                else
                {
                    if (pp.IsBasedOnPriceLevel && pp.ProductId == product.ProductId && pp.PriceLevelId == client.PriceLevel)
                    {
                        return pp;
                    }
                }
            }

            return null;

        }

        public static double CalculatePriceForProduct(Product product, Client client, bool isCredit, bool damaged, UnitOfMeasure uom, bool useOffer, out bool cameFromOffer, bool useConfig, Order order)
        {
            cameFromOffer = false;

            if (Config.UseRetailPriceForSales)
            {
                var retPrice = GetRetailPrice(product, client);
                if (retPrice > 0)
                    return retPrice;
            }

            var hidePrices = UDFHelper.GetSingleUDF("HidePriceInTransaction", client.NonvisibleExtraPropertiesAsString);

            if (useConfig && (Config.HidePriceInTransaction || (!string.IsNullOrEmpty(hidePrices) && hidePrices == "1")))
                return 0;

            //#region Not Used Code

            //if (Config.Simone)
            //{
            //    var startTime = DateTime.Now;

            //    try
            //    {
            //        if (client.AvailableOffersForListPrice == null)
            //        {
            //            var clientOffersId = ClientOfferEx.List.Where(x => x.ClientId == client.ClientId).Select(x => x.OfferExId).ToList();
            //            var prductOfferAvailable = ProductOfferEx.List.Where(x => x.BreakQty == 1 && x.Price > 0 && clientOffersId.Contains(x.OfferExId)).ToList();
            //            client.AvailableOffersForListPrice = prductOfferAvailable;
            //        }
            //        // see if any offer is available for this customer
            //        if (client.AvailableOffersForListPrice != null)
            //        {
            //            bool fP = false;
            //            double pp = 0;
            //            bool foundT2 = false;
            //            //var offers = client.AvailableOffersForListPrice.Where(x => x.ProductId == product.ProductId && x.Price > 0).ToList();

            //            var offersids = ClientOfferEx.List.Where(x => x.ClientId == order.Client.ClientId).Select(x => x.OfferExId).Distinct().ToList();
            //            var offers = OfferEx.List.Where(x => offersids.Contains(x.Id)).ToList();

            //            var firstoffer = offers.FirstOrDefault();
            //            if (firstoffer != null && !firstoffer.ExtraFields.Contains("BySubGroub=1"))//order.Client.NonVisibleExtraFields.Contains("NJ")
            //            {

            //                var Secodaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
            //               x.FromDate.Date <= (order.ShipDate != DateTime.MinValue ? order.ShipDate : DateTime.Today) &&
            //               x.ToDate.Date >= (order.ShipDate != DateTime.MinValue ? order.ShipDate : DateTime.Today) &&
            //               x.Primary == false).ToList();

            //                if (Secodaryoffers.Count > 0)
            //                {
            //                    foreach (var Secodaryoffer in Secodaryoffers)
            //                    {
            //                        var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Secodaryoffer.Id).ToList();
            //                        var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();

            //                        var pricestiers = prodprices.Where(x => x.ProductId == product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
            //                        foreach (var price in pricestiers)
            //                        {
            //                            if (price.BreakQty <= 1 && price.Price != 0)
            //                            {
            //                                fP = true;
            //                                double factor = 1;
            //                                if (uom != null)
            //                                    factor = uom.Conversion;
            //                                pp = price.Price * factor;
            //                                foundT2 = true;
            //                            }
            //                        }
            //                    }
            //                }
            //                //Need to Check For All offers and get the minor for every item

            //                var Primaryoffers = offers.Where(x => x.OfferType == (int)OfferType.TieredPricing || x.OfferType == (int)OfferType.Price &&
            //             x.FromDate.Date <= (order.ShipDate != DateTime.MinValue ? order.ShipDate : DateTime.Today) &&
            //             x.ToDate.Date >= (order.ShipDate != DateTime.MinValue ? order.ShipDate : DateTime.Today) &&
            //             x.Primary == true).ToList();

            //                foreach (var Primaryoffer in Primaryoffers)
            //                {
            //                    if (Primaryoffer != null)
            //                    {
            //                        var prodprices = ProductOfferEx.List.Where(x => x.OfferExId == Primaryoffer.Id).ToList();
            //                        var countedprod = prodprices.Select(x => x.ProductId).Distinct().ToList();

            //                        var pricestiers = prodprices.Where(x => x.ProductId == product.ProductId).OrderBy(x => x.ProductId).ThenBy(x => x.BreakQty).ToList();
            //                        foreach (var price in pricestiers)
            //                        {
            //                            if (price.BreakQty <= 1 && price.Price != 0)
            //                            {
            //                                if (foundT2)
            //                                {
            //                                    /* if (detail.Price > price.Price)
            //                                     {
            //                                         double factor = 1;
            //                                         if (detail.UnitOfMeasure != null)
            //                                             factor = detail.UnitOfMeasure.Conversion;
            //                                         detail.Price = price.Price * factor;
            //                                     }*/
            //                                }
            //                                else
            //                                {
            //                                    double factor = 1;
            //                                    if (uom != null)
            //                                        factor = uom.Conversion;
            //                                    pp = price.Price * factor;
            //                                    fP = true;
            //                                    // detail.FromOffer = true;
            //                                    //  detail.ExtraFields = UDFHelper.SyncSingleUDF("TiertType", "Tier2", detail.ExtraFields);
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            System.Diagnostics.Debug.WriteLine("Calculating best price for product id => " + product.Name + " Took " + Math.Round((DateTime.Now - startTime).TotalSeconds, 2));

            //            if (fP)
            //                return pp;
            //        }
            //    }
            //    catch (Exception ee)
            //    {
            //        var msg = ee.StackTrace;
            //        Logger.CreateLog(ee);
            //    }
            //}

            //#endregion

            if (Config.UseLSPByDefault)
            {
                if (client.OrderedList == null)
                {
                    var excludedProductsIds = new List<int>();
                    var excludedExtraField = client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "excludeditems");
                    if (excludedExtraField != null)
                        foreach (var idAsString in excludedExtraField.Item2.Split(new char[] { ',' }).ToList())
                            excludedProductsIds.Add(Convert.ToInt32(idAsString));
                    var lastList = InvoiceDetail.ClientOrderedItemsEx(client.ClientId);
                    client.OrderedList = lastList.Where(x => !excludedProductsIds.Contains(x.Last.ProductId)).ToList();
                }
                var prevProd = client.OrderedList.FirstOrDefault(x => x.Last.ProductId == product.ProductId);
                if (prevProd != null)
                {
                    float factor = 1;
                    if (prevProd.Last.UnitOfMeasureId > 0)
                    {
                        var lastuom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == prevProd.Last.UnitOfMeasureId);
                        if (lastuom != null)
                            factor = lastuom.Conversion;
                    }
                    var t_ = prevProd.Last.Price / factor;

                    if (uom != null)
                        t_ *= uom.Conversion;

                    return Math.Round(t_, Config.Round);
                }
            }

            double basePrice = 0;
            bool foundPrice = false;

            if (useOffer && Offer.ProductHasSpecialPriceForClient(product, client, out basePrice, uom))
            {
                cameFromOffer = true;
                foundPrice = true;
            }

            //filter product price 
            var temp_prodPrice = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId).ToList();

            if (client != null)
                temp_prodPrice = temp_prodPrice.Where(x => x.PriceLevelId == client.PriceLevel).ToList();

            if (order != null && order.PriceLevelId > -1)
                temp_prodPrice = temp_prodPrice.Where(x => x.PriceLevelId == order.PriceLevelId).ToList();

            if (!foundPrice)
            {
                foreach (var pp in temp_prodPrice)
                {
                    if (order != null && order.PriceLevelId > -1)
                    {
                        if (order.PriceLevelId == pp.PriceLevelId && pp.ProductId == product.ProductId)
                        {
                            basePrice = pp.Price;
                            foundPrice = true;
                            break;
                        }
                    }
                    else
                    {
                        if (pp.IsBasedOnPriceLevel && pp.ProductId == product.ProductId && pp.PriceLevelId == client.PriceLevel)
                        {
                            basePrice = pp.Price;
                            foundPrice = true;
                            break;
                        }
                    }
                }
            }

            if (!foundPrice)
            {
                switch (client.PriceLevel)
                {
                    case 0:
                        basePrice = product.PriceLevel0;
                        break;
                    case 1:
                        basePrice = product.PriceLevel1;
                        break;
                    case 2:
                        basePrice = product.PriceLevel2;
                        break;
                    case 3:
                        basePrice = product.PriceLevel3;
                        break;
                    case 4:
                        basePrice = product.PriceLevel4;
                        break;
                    case 5:
                        basePrice = product.PriceLevel5;
                        break;
                    case 6:
                        basePrice = product.PriceLevel6;
                        break;
                    case 7:
                        basePrice = product.PriceLevel7;
                        break;
                    case 8:
                        basePrice = product.PriceLevel8;
                        break;
                    case 9:
                        basePrice = product.LowestAcceptablePrice;
                        break;
                    default:
                        basePrice = product.PriceLevel0;
                        break;
                }
            }

            if (order != null)
            {
                if (product.UnitOfMeasures != null && product.UnitOfMeasures.Count() < 1)
                {
                    if (isCredit && (damaged || (Config.PackageInReturnPresale/* && order.AsPresale*/)))
                        try
                        {
                            basePrice /= Convert.ToDouble(product.Package);
                        }
                        catch
                        {
                            Logger.CreateLog("get price for product with id=" + product.ProductId + " and Package=" + product.Package);
                        }
                }

                bool halfCredit = false;

                var extField = client.NonVisibleExtraProperties != null ? client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLower() == "halfcredit") : null;
                if (extField == null)
                    extField = client.ExtraProperties != null ? client.ExtraProperties.FirstOrDefault(x => x.Item1.ToLower() == "halfcredit") : null;
                halfCredit = extField != null && extField.Item2 == "yes";

                if (isCredit && halfCredit)
                    basePrice /= 2;
            }

            if (uom != null && !cameFromOffer)
                basePrice *= uom.Conversion;

            return Math.Round(basePrice, Config.Round);
        }

        public static List<Product> FindProductsInCategory(int categoryId)
        {
            if (productsInCategories.ContainsKey(categoryId))
                return productsInCategories[categoryId];

            List<Product> items = new List<Product>();
            foreach (var p in Products)
                if (p.CategoryId == categoryId)
                    items.Add(p);
            productsInCategories.Add(categoryId, items);
            return items;
        }

        public static double GetRetailPrice(Product p, Client client)
        {
            if (client == null)
                return 0;

            var retailProdPrice = RetailProductPrice.Pricelist.FirstOrDefault(x => x.RetailPriceLevelId == client.RetailPriceLevelId && x.ProductId == p.ProductId);

            return retailProdPrice != null ? retailProdPrice.Price : 0;
        }

        public static List<Product> GetProductByCategory(Category category, Client client)
        {
            if (client == null || client.CategoryId == 0)
                return FindProductsInCategory(category.CategoryId);

            return ClientCategoryProducts.Find(client.CategoryId).ProductsInCategory(category.CategoryId);
        }

        public static IEnumerable<Product> GetProductVisibleToClient(Client client)
        {
            if (client.CategoryId == 0)
                return Products;
            return ClientCategoryProducts.Find(client.CategoryId).Products;
        }

        public string OrderedBasedOnConfig()
        {

            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "name":
                    return Name;
                case "category":
                    return CategoryId.ToString();
                case "upc":
                    return Upc;
                default:
                    return Name;
            }
        }

        public static void LoadLots()
        {
            if (File.Exists(Config.LotsFile))
            {
                using (StreamReader reader = new StreamReader(Config.LotsFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split(new char[] { (char)20 });
                            int productID = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                            var product = Products.FirstOrDefault(x => x.ProductId == productID);
                            if (product != null)
                            {
                                if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                                    product.AddLot(parts[1]);
                                if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
                                    product.AddLot(parts[2]);
                                if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]))
                                    product.AddLot(parts[3]);
                            }
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                            //Xamarin.Insights.Report(ee);
                        }
                    }
                    reader.Close();
                }
            }
        }

        public static void SaveLots()
        {
            string tempFile = Path.GetTempFileName();

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        var sb = new StringBuilder();
                        foreach (var btq in Products)
                        {
                            sb.Clear();
                            sb.Append(btq.ProductId);
                            bool addIt = false;
                            if (btq.lots != null && btq.lots.Length > 2 && !string.IsNullOrEmpty(btq.lots[0]))
                            {
                                sb.Append((char)20);
                                sb.Append(btq.lots[0]);
                                addIt = true;
                            }
                            if (btq.lots != null && btq.lots.Length > 2 && !string.IsNullOrEmpty(btq.lots[1]))
                            {
                                sb.Append((char)20);
                                sb.Append(btq.lots[1]);
                                addIt = true;
                            }
                            if (btq.lots != null && btq.lots.Length > 2 && !string.IsNullOrEmpty(btq.lots[2]))
                            {
                                sb.Append((char)20);
                                sb.Append(btq.lots[2]);
                                addIt = true;
                            }
                            if (addIt)
                                writer.WriteLine(sb.ToString());
                        }
                        writer.Close();
                    }

                    File.Copy(tempFile, Config.LotsFile, true);
                    File.Delete(tempFile);
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void RemoveNonVisible(List<ProductVisibleSalesman> visible)
        {
            for (int i = 0; i < products.Count; i++)
            {
                var key = products.ElementAt(i).Key;

                if (visible.FirstOrDefault(x => x.ProductId == key) == null)
                {
                    if (products[key].ProductType == ProductType.Discount)
                        continue;

                    products.Remove(key);
                    i--;
                }
            }
            Category.RemoveEmptyCategories();
        }

        public static List<Tuple<int, int>> coreProducts = new List<Tuple<int, int>>();
        public static List<Tuple<int, int>> adjustmentProducts = new List<Tuple<int, int>>();
        public static List<Tuple<int, int>> rotatedProducts = new List<Tuple<int, int>>();
        public static List<Tuple<int, int>> relatedProducts = new List<Tuple<int, int>>();

        public static void CoreProducts()
        {
            if (coreProducts.Count == 0 && rotatedProducts.Count == 0 && adjustmentProducts.Count == 0)
            {
                foreach (var prod in Products)
                {
                    var core = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");
                    var adjustment = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");
                    var rotated = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");
                    Tuple<string, string> relateditem = null;
                    foreach (var item in prod.ExtraProperties)
                        if (item.Item1.ToLowerInvariant() == "relateditem")
                        {
                            relateditem = item;
                            break;
                        }

                    if (core != null)
                        coreProducts.Add(new Tuple<int, int>(prod.ProductId, Convert.ToInt32(core.Item2)));
                    if (adjustment != null)
                        adjustmentProducts.Add(new Tuple<int, int>(prod.ProductId, Convert.ToInt32(adjustment.Item2)));
                    if (rotated != null)
                        rotatedProducts.Add(new Tuple<int, int>(prod.ProductId, Convert.ToInt32(rotated.Item2)));
                    if (relateditem != null)
                        relatedProducts.Add(new Tuple<int, int>(prod.ProductId, Convert.ToInt32(relateditem.Item2)));
                }
            }
        }

        public static IEnumerable<Product> GetProductsKit(Order order, int categoryId = 0)
        {
            var products = order.Details.Select(x => x.Product);
            List<int> sentIds = new List<int>();

            foreach (var item in products)
            {
                if (string.IsNullOrEmpty(item.NonVisibleExtraFieldsAsString))
                    continue;

                var type = UDFHelper.GetSingleUDF("Type", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(type) || type.ToLowerInvariant() == "vnr" || type.ToLowerInvariant() == "vpr")
                    continue;

                var kit = UDFHelper.GetSingleUDF("Kit", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(kit))
                    continue;

                var parts = kit.Split('/');
                foreach (var p in parts)
                {
                    var ps = p.Split(',');
                    int prodId = 0;

                    int.TryParse(ps[0], out prodId);

                    if (prodId > 0)
                    {
                        var product = Product.Products.FirstOrDefault(x => x.ProductId == prodId);
                        if (product != null)
                        {
                            if (categoryId == 0)
                            {
                                if (!sentIds.Contains(product.ProductId))
                                {
                                    sentIds.Add(product.ProductId);
                                    yield return product;
                                }
                            }
                            else if (categoryId > 0 && product.CategoryId == categoryId)
                            {
                                if (!sentIds.Contains(product.ProductId))
                                {
                                    sentIds.Add(product.ProductId);
                                    yield return product;
                                }
                            }
                        }
                    }
                }

                if (categoryId == 0)
                {
                    if (!sentIds.Contains(item.ProductId))
                    {
                        sentIds.Add(item.ProductId);
                        yield return item;
                    }
                }
                else if (categoryId > 0 && item.CategoryId == categoryId)
                {
                    if (!sentIds.Contains(item.ProductId))
                    {
                        sentIds.Add(item.ProductId);
                        yield return item;
                    }
                }
            }
        }

        public bool AllowChangeProductKitQtyInDelivery(Order order, float qty)
        {
            OrderDetail orderDetail = null;
            string kit = string.Empty;

            foreach (var detail in order.Details.Where(x => !x.IsCredit))
            {
                var item = detail.Product;

                if (string.IsNullOrEmpty(item.NonVisibleExtraFieldsAsString))
                    continue;

                var type = UDFHelper.GetSingleUDF("Type", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(type) || type.ToLowerInvariant() == "vnr" || type.ToLowerInvariant() == "vpr")
                    continue;

                kit = UDFHelper.GetSingleUDF("Kit", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(kit))
                    continue;

                if (item.ProductId == ProductId)
                {
                    orderDetail = detail;
                    break;
                }

                var parts = kit.Split('/');
                foreach (var p in parts)
                {
                    var ps = p.Split(',');
                    int prodId = 0;

                    int.TryParse(ps[0], out prodId);
                    if (prodId == ProductId)
                    {
                        orderDetail = detail;
                        break;
                    }
                }
            }

            if (orderDetail != null && !string.IsNullOrEmpty(kit))
            {
                var kitItem = new KitItem(GetProductFromKit(order), kit, orderDetail.Qty);

                foreach (var item in order.Details.Where(x => x.IsCredit))
                    kitItem.AdjustQty(item);

                if (qty < orderDetail.Qty)
                    return kitItem.CanDecreaseQtyInDelivery(this, qty);
                else
                    return true;
            }

            return true;
        }

        public bool AllowChangeProductKitQty(Order order, float qty)
        {
            OrderDetail orderDetail = null;
            string kit = string.Empty;

            foreach (var detail in order.Details.Where(x => !x.IsCredit))
            {
                var item = detail.Product;

                if (string.IsNullOrEmpty(item.NonVisibleExtraFieldsAsString))
                    continue;

                var type = UDFHelper.GetSingleUDF("Type", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(type) || type.ToLowerInvariant() == "vnr" || type.ToLowerInvariant() == "vpr")
                    continue;

                kit = UDFHelper.GetSingleUDF("Kit", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(kit))
                    continue;

                if (item.ProductId == ProductId)
                {
                    orderDetail = detail;
                    break;
                }

                var parts = kit.Split('/');
                foreach (var p in parts)
                {
                    var ps = p.Split(',');
                    int prodId = 0;

                    int.TryParse(ps[0], out prodId);
                    if (prodId == ProductId)
                    {
                        orderDetail = detail;
                        break;
                    }
                }
            }

            if (orderDetail != null && !string.IsNullOrEmpty(kit))
            {
                var kitItem = new KitItem(GetProductFromKit(order), kit, orderDetail.Qty);

                foreach (var item in order.Details.Where(x => x.IsCredit))
                    kitItem.AdjustQty(item);

                return kitItem.CanChangeQty(this, qty);
            }

            return true;
        }

        public Product GetProductFromKit(Order order)
        {
            foreach (var detail in order.Details.Where(x => !x.IsCredit))
            {
                var item = detail.Product;

                if (string.IsNullOrEmpty(item.NonVisibleExtraFieldsAsString))
                    continue;

                var type = UDFHelper.GetSingleUDF("Type", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(type) || type.ToLowerInvariant() == "vnr" || type.ToLowerInvariant() == "vpr")
                    continue;

                var kit = UDFHelper.GetSingleUDF("Kit", item.NonVisibleExtraFieldsAsString);
                if (string.IsNullOrEmpty(kit))
                    continue;

                var parts = kit.Split('/');
                foreach (var p in parts)
                {
                    var ps = p.Split(',');
                    int prodId = 0;

                    int.TryParse(ps[0], out prodId);
                    if (prodId == ProductId)
                    {
                        return item;
                    }
                }
            }

            return this;
        }

        public bool IsRelatedProduct { get; set; }

        public static List<int> GetBlockedProducts(Client client, Order currentOrder)
        {
            List<int> result = new List<int>();

            if (currentOrder.IsProjection)
                return result;

            var orders = Order.Orders.Where(x => !x.Voided && !x.IsProjection && x.Client.ClientId == client.ClientId && x.UniqueId != currentOrder.UniqueId);

            if (Config.UseCatalogWithFullTemplate)
                orders = Order.Orders.Where(x => x.Finished && !x.Voided && !x.IsProjection && x.Client.ClientId == client.ClientId && x.UniqueId != currentOrder.UniqueId);

            foreach (var order in orders)
                foreach (var prod in order.Details)
                    if (!result.Contains(prod.Product.ProductId))
                        result.Add(prod.Product.ProductId);


            if (currentOrder.AsPresale)
                result.Clear();

            return result;
        }

        public static List<int> GetBlockedProducts(Client client, Order sales, Order credit)
        {
            List<int> result = new List<int>();

            var orders = Order.Orders.Where(x => !x.Voided && !x.IsProjection
            && x.Client.ClientId == client.ClientId
            && x.UniqueId != sales.UniqueId && x.UniqueId != credit.UniqueId);

            if (Config.UseCatalogWithFullTemplate)
                orders = Order.Orders.Where(x => x.Finished && !x.Voided && !x.IsProjection && x.Client.ClientId == client.ClientId && x.UniqueId != sales.UniqueId && x.UniqueId != credit.UniqueId);

            foreach (var order in orders)
                foreach (var prod in order.Details)
                    if (!result.Contains(prod.Product.ProductId))
                        result.Add(prod.Product.ProductId);

            if (sales.AsPresale && credit.AsPresale)
                result.Clear();

            return result;
        }

        public bool InventoryByWeight { get; set; }

        public static Product CreateNotFoundProduct(int id, bool addIt = true)
        {
            var product = new Product() { ProductId = id, Name = "PRODUCT NOT FOUND" };
            product.Upc = "";
            product.Sku = "";
            product.Comment = "";
            product.Package = "1";
            product.Description = "";
            product.OriginalId = "";
            product.Code = "";
            product.WarehouseLocation = "";
            product.ExtraPropertiesAsString = "";
            product.NonVisibleExtraFieldsAsString = "";
            product.ProductType = ProductType.Inventory;

            //this was giving errors
            try
            {
                var prodInvent = ProductInventory.CurrentInventories.FirstOrDefault(x => x.Key == product.ProductId).Value;

                if (prodInvent != null)
                    product.prodInv = prodInvent;
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error trying to set the prod inventory when product is not found.");
            }

            if (addIt && !products.ContainsKey(id))
                products.Add(id, product);

            return product;
        }

        public string GetPartNumberForCustomer(Client client)
        {
            foreach (ProductPrice pp in ProductPrice.Pricelist)
                if (pp.ProductId == ProductId && pp.PriceLevelId == client.PriceLevel)
                    return pp.PartNumber ?? "";
            return string.Empty;
        }

        public int CaseCount { get; set; }

        public static Dictionary<int, Product> ProductsInHistory = new Dictionary<int, Product>();

        List<UnitOfMeasure> unitOfMeasures;
        public List<UnitOfMeasure> UnitOfMeasures
        {
            get
            {
                if (unitOfMeasures == null)
                {
                    if (string.IsNullOrEmpty(UoMFamily))
                        unitOfMeasures = new List<UnitOfMeasure>();
                    else
                        unitOfMeasures = UnitOfMeasure.List.Where(x => x.FamilyId == UoMFamily).ToList();
                }

                return unitOfMeasures;
            }
        }

        public double RetailPrice { get; set; }

        public float OnHand { get; set; }
        public float OnPO { get; set; }

        #region Lot

        bool uselot = false;
        public bool UseLot
        {
            get
            {
                return uselot || Config.UsePairLotQty || Config.FakeUseLot;
            }
            set
            {
                uselot = value;
            }
        }

        bool useLotAsReference = false;

        public bool UseLotAsReference
        {
            get
            {
                return useLotAsReference || Config.UseLot || Config.LotIsMandatory;
            }
            set
            {
                useLotAsReference = value;
            }
        }

        public bool LotIsMandatory(bool asPresale, bool damaged)
        {
            return !asPresale && (!damaged || Config.RequireLotForDumps) && (UseLot || useLotAsReference || Config.LotIsMandatory);
        }

        #endregion

        #region Inventory

        ProductInventory prodInv = null;
        public ProductInventory ProductInv
        {
            get
            {
                if (prodInv == null)
                {
                    prodInv = new ProductInventory() { ProductId = this.ProductId };
                    ProductInventory.CurrentInventories.Add(ProductId, prodInv);
                }
                return prodInv;
            }
            set
            {
                prodInv = value;
            }
        }

        /// <summary>
        /// Current Warehouse Inventory
        /// </summary>
        /// </summary>
        public float CurrentWarehouseInventory { get { return Config.DisolCrap ? ProductInv.WarehouseInventory : (float)Math.Round(ProductInv.WarehouseInventory, Config.Round); } }
        /// <summary>
        /// Current Total Truck Inventory
        /// </summary>
        public float CurrentInventory { get { return Config.DisolCrap ? ProductInv.CurrentInventory : (float)Math.Round(ProductInv.CurrentInventory, Config.Round); } }
        /// <summary>
        /// Total Begining Inventory. Is the Left Over in the InventorySiteInventory when the user sync updating the inventories
        /// </summary>
        public float BeginigInventory { get { return Config.DisolCrap ? ProductInv.BeginigInventory : (float)Math.Round(ProductInv.BeginigInventory, Config.Round); } }
        /// <summary>
        /// Total Requested Inventory from loads or deliveries. 
        /// </summary>
        public float RequestedLoadInventory { get { return Config.DisolCrap ? ProductInv.RequestedLoadInventory : (float)Math.Round(ProductInv.RequestedLoadInventory, Config.Round); } }
        /// <summary>
        /// Total Accepted Inventory from loads or deliveries
        /// </summary>
        public float LoadedInventory { get { return Config.DisolCrap ? ProductInv.LoadedInventory : (float)Math.Round(ProductInv.LoadedInventory, Config.Round); } }
        /// <summary>
        /// Total Transferred On Inventory
        /// </summary>
        public float TransferredOnInventory { get { return Config.DisolCrap ? ProductInv.TransferredOnInventory : (float)Math.Round(ProductInv.TransferredOnInventory, Config.Round); } }
        /// <summary>
        /// Total Transferred Off Inventory
        /// </summary>
        public float TransferredOffInventory { get { return Config.DisolCrap ? ProductInv.TransferredOffInventory : (float)Math.Round(ProductInv.TransferredOffInventory, Config.Round); } }
        /// <summary>
        /// Total Unloaded items
        /// </summary>
        public float UnloadedInventory { get { return Config.DisolCrap ? ProductInv.UnloadedInventory : (float)Math.Round(ProductInv.UnloadedInventory, Config.Round); } }
        /// <summary>
        /// Total of Damaged In Truck
        /// </summary>
        public float DamagedInTruckInventory { get { return Config.DisolCrap ? ProductInv.DamagedInTruckInventory : (float)Math.Round(ProductInv.DamagedInTruckInventory, Config.Round); } }

        #region Get Inventory

        public float GetInventory(string lot, double Weight)
        {
            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            string lotTocheck = lot;
            if (!UseLot)
                lotTocheck = "";

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == lotTocheck && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
                return 0;
            return inv.CurrentQty;
        }

        public float GetInventory(bool asPresale, string lot, bool forChecking = true, double Weight = 0)
        {
            if (!forChecking)
            {
                if (asPresale)
                    return CurrentWarehouseInventory;
                else
                    return GetInventory(lot, Weight);
            }
            else
            {
                if (ProductType == ProductType.NonInventory || Config.CanGoBelow0)
                    return float.MaxValue;

                if (asPresale)
                {
                    if (Config.CheckInventoryInPreSale)
                        return CurrentWarehouseInventory;
                    return float.MaxValue;
                }
                else if (Config.TrackInventory)
                    return GetInventory(lot, Weight);
                else
                    return float.MaxValue;
            }
        }

        public float GetInventory(bool asPresale, bool forChecking = true)
        {
            if (!forChecking)
            {
                if (asPresale)
                    return CurrentWarehouseInventory;
                else
                    return CurrentInventory;
            }
            else
            {
                if (ProductType == ProductType.NonInventory || Config.CanGoBelow0)
                    return float.MaxValue;

                if (asPresale)
                {
                    if (Config.CheckInventoryInPreSale)
                        return CurrentWarehouseInventory;
                    return float.MaxValue;
                }
                else if (Config.TrackInventory)
                    return CurrentInventory;
                else
                    return float.MaxValue;
            }
        }

        #endregion

        #region Update Inventories

        public void UpdateInventory(bool asPresale, float qty, UnitOfMeasure uom, string lot, int factor, double Weight)
        {
            if (asPresale)
            {
                //dice michel as of: 4/7/2025
                //if (Config.CheckInventoryInPreSale)
                UpdateWarehouseInventory(qty, uom, lot, DateTime.MinValue, factor, Weight);
            }
            else
            {
                if (Config.TrackInventory)
                    UpdateInventory(qty, uom, lot, DateTime.MinValue, factor, Weight);
            }
        }

        public void UpdateInventory(float qty, UnitOfMeasure uom, string lot, DateTime exp, int factor, double Weight)
        {
            if (factor == 0)
                return;

            var baseQty = qty;
            if (uom != null)
                baseQty *= uom.Conversion;
            if(prodInv != null)
                prodInv.UpdateInventory(baseQty, lot, exp, factor, Weight);
        }

        //No se puede usar en ordenes
        public void UpdateInventory(float qty, UnitOfMeasure uom, int factor, double Weight)
        {
            UpdateInventory(qty, uom, "", DateTime.MinValue, factor, Weight);
        }

        public void UpdateWarehouseInventory(float qty, UnitOfMeasure uom, string lot, DateTime exp, int factor, double Weight)
        {
            if (factor == 0)
                return;

            var baseQty = qty;
            if (uom != null)
                baseQty *= uom.Conversion;
            if (prodInv != null)
                prodInv.UpdateWarehouseInventory(baseQty, factor);
        }

        public void UpdateWarehouseInventory(float qty, UnitOfMeasure uom, int factor, double Weight)
        {
            UpdateWarehouseInventory(qty, uom, "", DateTime.MinValue, factor, Weight);
        }

        #endregion

        #region Add Inventory Management

        public void AddRequestedInventory(float qty, UnitOfMeasure uom, string lot, DateTime exp, double Weight)
        {
            var baseQty = qty;
            if (uom != null)
                baseQty *= uom.Conversion;

            prodInv.AddRequestedInventory(baseQty, lot, exp, Weight);
        }

        public void AddRequestedInventory(float qty, UnitOfMeasure uom, double Weight)
        {
            AddRequestedInventory(qty, uom, "", DateTime.MinValue, Weight);
        }

        public void AddLoadedInventory(float qty, UnitOfMeasure uom, string lot, DateTime exp, double Weight)
        {
            var baseQty = qty;
            if (uom != null)
                baseQty *= uom.Conversion;

            prodInv.AddLoadedInventory(baseQty, lot, exp, Weight);
        }

        public void AddLoadedInventory(float qty, UnitOfMeasure uom, double Weight)
        {
            AddLoadedInventory(qty, uom, "", DateTime.MinValue, Weight);
        }

        public void AddTransferredInventory(float qty, UnitOfMeasure uom, string lot, DateTime exp, int factor, double Weight)
        {
            var baseQty = qty;
            if (uom != null)
                baseQty *= uom.Conversion;

            prodInv.UpdateTransferInventory(baseQty, lot, exp, factor, Weight);
        }

        public void AddTransferredInventory(float qty, UnitOfMeasure uom, int factor, double Weight)
        {
            AddTransferredInventory(qty, uom, "", DateTime.MinValue, factor, Weight);
        }

        #endregion

        #region Set Inventory Management

        public void SetUnloadInventory(float qty, string lot, DateTime exp, double weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.Unloaded = qty;
        }

        public void SetUnloadInventory(float qty, string lot = "", double weight = 0)
        {
            SetUnloadInventory(qty, lot, DateTime.MinValue, weight);
        }

        public void SetDamagedInTruckInventory(float qty, string lot, DateTime exp, double Weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.DamagedInTruck = qty;
        }

        public void SetDamagedInTruckInventory(float qty, string lot = "", double Weight = 0)
        {
            SetDamagedInTruckInventory(qty, lot, DateTime.MinValue, Weight);
        }

        public void SetCurrentInventory(float qty, string lot, DateTime exp, double Weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.CurrentQty = qty;
        }

        public void SetCurrentInventory(float qty, string lot = "", double Weight = 0)
        {
            SetCurrentInventory(qty, lot, DateTime.MinValue, Weight);
        }

        #endregion

        #region Settlement

        public void SetOnCreditDump(float qty, string lot, DateTime exp, double Weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.OnCreditDump = qty;
        }

        public void SetOnCreditDump(float qty, string lot = "", double Weight = 0)
        {
            SetOnCreditDump(qty, lot, DateTime.MinValue, Weight);
        }

        public void SetOnCreditReturn(float qty, string lot, DateTime exp, double Weight)
        {
            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var _lot = UseLot ? lot : "";

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.OnCreditReturn = qty;
        }

        public void SetOnCreditReturn(float qty, string lot = "", double Weight = 0)
        {
            SetOnCreditReturn(qty, lot, DateTime.MinValue, Weight);
        }

        public void SetOnSales(float qty, string lot, DateTime exp, double Weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.OnSales = qty;
        }

        public void SetOnSales(float qty, string lot = "", double Weight = 0)
        {
            SetOnSales(qty, lot, DateTime.MinValue, Weight);
        }

        public void SetOnReship(float qty, string lot, DateTime exp, double Weight)
        {
            var _lot = UseLot ? lot : "";

            if (!SoldByWeight || InventoryByWeight)
                Weight = 0;

            var inv = ProductInv.TruckInventories.FirstOrDefault(x => x.Lot == _lot && Math.Round(x.Weight, 2) == Math.Round(Weight, 2));
            if (inv == null)
            {
                inv = new TruckInventory() { Lot = _lot, Weight = Weight };
                ProductInv.TruckInventories.Add(inv);
            }

            inv.OnReships = qty;
        }

        public void SetOnReship(float qty, string lot = "", double Weight = 0)
        {
            SetOnReship(qty, lot, DateTime.MinValue, Weight);
        }

        class T
        {
            public Product Product { get; set; }
            public float OnSales { get; set; }
            public float OnCredit { get; set; }
            public float OnDumps { get; set; }
            public float OnReships { get; set; }
            public string Lot { get; set; }
            public DateTime Exp { get; set; }

            public double Weight { get; set; }
            public T(Product p)
            {
                Product = p;
                Lot = "";
            }
        }

        public static void AdjustValuesFromOrder(bool fromEoD = true)
        {
            List<T> items = new List<T>();

            var orders = Order.Orders.Where(x => !x.AsPresale && !x.Voided && x.OrderType != OrderType.Load && x.OrderType != OrderType.Bill);

            foreach (var o in orders)
            {
                foreach (var det in o.Details)
                {
                    if (!det.Substracted)
                        continue;

                    var _lot = det.Product.UseLot ? det.Lot : "";

                    var item = items.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Lot == _lot);
                    if (det.Product.SoldByWeight && !det.Product.InventoryByWeight)
                        item = items.FirstOrDefault(x => x.Product.ProductId == det.Product.ProductId && x.Lot == _lot && x.Weight == det.Weight);

                    if (item == null)
                    {
                        item = new T(det.Product) { Lot = _lot, Exp = det.LotExpiration };
                        if (det.Product.SoldByWeight && !det.Product.InventoryByWeight)
                            item.Weight = det.Weight;
                        items.Add(item);
                    }

                    if (o.OrderType == OrderType.Consignment)
                    {
                        if (Config.UseFullConsignment)
                        {
                            if (Config.ParInConsignment)
                            {
                                if (det.ParLevelDetail)
                                {
                                    if (det.IsCredit)
                                    {
                                        if (det.Damaged)
                                            item.OnDumps += det.Qty;
                                        else
                                            item.OnCredit += det.Qty;
                                    }
                                    else
                                        item.OnSales += det.Qty;
                                }
                                else
                                {
                                    if (det.ConsignmentPicked < 0)
                                    {
                                        if (det.Damaged)
                                            item.OnDumps += det.Qty;
                                        else
                                            item.OnCredit += det.Qty;
                                    }
                                    else
                                        item.OnSales += det.Qty;
                                }
                            }
                            else
                            {
                                if (det.ConsignmentPicked >= 0)
                                    item.OnSales += det.ConsignmentPicked;
                                else
                                {
                                    if (det.IsCredit)
                                    {
                                        if (det.Damaged)
                                            item.OnDumps += det.Qty;
                                        else
                                            item.OnCredit += det.Qty;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (det.ConsignmentSalesItem)
                                item.OnSales += det.Qty;
                            else
                            {
                                var base_ = det.ConsignmentOld;
                                if (det.ConsignmentCounted)
                                    base_ -= det.ConsignmentCount;

                                if (det.ConsignmentUpdated)
                                    base_ += det.ConsignmentNew - det.ConsignmentOld;

                                if (det.ConsignmentCounted || det.ConsignmentUpdated)
                                    item.OnSales += base_;
                            }
                        }
                    }
                    else
                    {
                        var qty = det.Qty;
                        if (det.Product.SoldByWeight && det.Product.InventoryByWeight)
                            qty = det.Weight;

                        if (det.UnitOfMeasure != null)
                            qty *= det.UnitOfMeasure.Conversion;

                        if (!o.Reshipped)
                        {
                            if (det.IsCredit)
                            {
                                if (det.Damaged)
                                    item.OnDumps += qty;
                                else
                                    item.OnCredit += qty;
                            }
                            else
                            {
                                item.OnSales += qty;
                            }
                        }
                        else if (!det.IsCredit)
                            item.OnReships += qty;
                    }
                }
            }

            foreach (var item in items)
            {
                if (!fromEoD && item.OnSales == 0 && item.OnCredit == 0)
                    continue;

                string lot = item.Product.UseLot ? item.Lot : "";

                item.Product.SetOnSales(item.OnSales, lot, item.Exp, item.Weight);
                item.Product.SetOnCreditDump(item.OnDumps, lot, item.Exp, item.Weight);
                item.Product.SetOnCreditReturn(item.OnCredit, lot, item.Exp, item.Weight);
                item.Product.SetOnReship(item.OnReships, lot, item.Exp, item.Weight);
            }

            ProductInventory.Save();
        }

        #endregion

        #endregion

        public bool MatchInName(List<string> values)
        {
            var name = Name.ToLower();

            foreach (var item in values)
            {
                if (name.Contains(item))
                    return true;
            }
            return false;
        }

        public static List<Product> GetProductListForOrder(Order order, bool isCredit, int categoryId, bool forTemplate = false)
        {
            if (order.IsExchange)
                isCredit = false;
            
            if (Config.ProductInMultipleCategory && categoryId > 0)
                return GetProductListForOrderMultipleCategories(order, isCredit, categoryId);

            List<Product> products = new List<Product>();

            if (isCredit && !Config.AuthProdsInCredit)
            {
                if (Config.OnlyKitInCredit && order.OrderType == OrderType.Order)
                    products = Product.GetProductsKit(order, categoryId).ToList();
                else
                {
                    products = Product.Products.Where(x => x.CategoryId > 0).ToList();
                    if (categoryId > 0)
                        products = products.Where(x => x.CategoryId == categoryId).ToList();
                }
            }
            else
            {
                if (order.OrderType == OrderType.Load)
                    products = Product.Products.Where(x => x.CategoryId > 0).ToList();
                else
                    products = Product.GetProductVisibleToClient(order.Client).Where(x => x.CategoryId > 0).ToList();
            }

            if (categoryId > 0)
                products = products.Where(x => x.CategoryId == categoryId).ToList();

            if (!isCredit && order.OrderType != OrderType.Load && !forTemplate)
            {
                if (!Config.CanGoBelow0 && (Config.TrackInventory || Config.CheckInventoryInPreSale))
                    products = products.Where(x => x.GetInventory(order.AsPresale) > 0).ToList();

                if (Config.ShowProductsWith0Inventory)
                {
                    products = Product.GetProductVisibleToClient(order.Client).Where(x => x.CategoryId > 0).ToList();
                    if (categoryId > 0)
                        products = products.Where(x => x.CategoryId == categoryId).ToList();
                }
            }

            if (Config.ShowExpensesInEOD)
                products = products.Where(x => !x.IsExpense).ToList();

            //Butler filter by price level
            if (Config.ButlerCustomization && order.Client.PriceLevel > 0)
            {
                var productPrices = ProductPrice.Pricelist.Where(x => x.PriceLevelId == order.Client.PriceLevel).Select(x => x.ProductId).Distinct().ToList();

                products = products.Where(x => productPrices.Contains(x.ProductId)).ToList();
            }

            if (Config.SalesmanCanChangeSite)
            {
                if (ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
                {
                    var allowedProducts = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                    products = products.Where(x => allowedProducts.Contains(x.ProductId)).ToList();
                }
            }

            List<Product> new_products = null;
            if (isCredit)
            {
                new_products = new List<Product>();
                foreach (var prod in products)
                {
                    var core = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToLower() == "availablein");

                    if (core != null)
                    {
                        string availableIn = core.Item2.ToString();
                        if (availableIn.ToLowerInvariant().Contains("credit"))
                        {
                            new_products.Add(prod);
                        }
                    }
                    else
                    {
                        new_products.Add(prod);
                    }
                }
            }
            else
            {
                new_products = new List<Product>();
                foreach (var prod in products)
                {
                    var core = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToLower() == "availablein");

                    if (core != null)
                    {
                        string availableIn = core.Item2.ToString();
                        if (availableIn.ToLowerInvariant().Contains("order"))
                        {
                            new_products.Add(prod);
                        }
                    }
                    else
                    {
                        new_products.Add(prod);
                    }
                }
            }

            if (order.DepartmentId > 0 && Config.MustSelectDepartment)
            {
                var validproducts = DepartmentProduct.List.Where(x => x.DepartmentId == order.DepartmentId).Select(x => x.ProductId).ToList();

                if (validproducts.Count > 0)
                {
                    if (new_products != null)
                        new_products = new_products.Where(x => validproducts.Contains(x.ProductId)).ToList();
                    else
                        products = products.Where(x => validproducts.Contains(x.ProductId)).ToList();
                }
            }

            if (order.IsWorkOrder)
            {
                var cats = Category.Categories.Where(x => x.TypeServiPart != CategoryServiPartType.None).Select(x => x.CategoryId).ToList();

                var pinCats = Product.Products.Where(x => cats.Contains(x.CategoryId)).Select(x => x.ProductId).ToList();

                if (new_products != null)
                    new_products = new_products.Where(x => pinCats.Contains(x.ProductId)).ToList();
                else
                    products = products.Where(x => pinCats.Contains(x.ProductId)).ToList();

            }

            if (order != null && order.CompanyId > 0)
            {
                var productsVisibleForCompany = ProductVisibleCompany.List.Where(x => x.CompanyId == order.CompanyId).Select(x => x.ProductId).ToList();

                if (new_products != null)
                {
                    if (productsVisibleForCompany.Count > 0)
                        new_products = new_products.Where(x => productsVisibleForCompany.Contains(x.ProductId)).ToList();
                }
                else
                {
                    if (productsVisibleForCompany.Count > 0)
                        products = products.Where(x => productsVisibleForCompany.Contains(x.ProductId)).ToList();
                }
            }

            if (new_products != null)
            {
                if (order.OrderType == OrderType.Credit || (isCredit && order.OrderType == OrderType.Credit))
                {
                    var categories = Category.Categories.Where(x => !x.NotVisibleInDump).Select(x => x.CategoryId).ToList();
                    new_products = new_products.Where(x => categories.Contains(x.CategoryId)).ToList();
                }
            }
            else
            {
                if (order.OrderType == OrderType.Credit || (isCredit && order.OrderType == OrderType.Credit))
                {
                    var categories = Category.Categories.Where(x => !x.NotVisibleInDump).Select(x => x.CategoryId).ToList();
                    products = products.Where(x => categories.Contains(x.CategoryId)).ToList();
                }
            }

            if (new_products != null)
                return new_products;
            else
                return products;
        }

        public static List<Product> GetProductListForClient(Client client, int categoryId, string searchCriteria)
        {
            List<Product> products = new List<Product>();

            products = Product.GetProductVisibleToClient(client).Where(x => x.CategoryId > 0).ToList();

            if (categoryId > 0)
                products = products.Where(x => x.CategoryId == categoryId).ToList();

            if (!string.IsNullOrEmpty(searchCriteria))
            {
                var criteria = searchCriteria.ToLowerInvariant();
                products = products.Where(x =>
                x.Name.ToLowerInvariant().Contains(criteria) ||
                x.Code.ToLowerInvariant().Contains(criteria) ||
                x.Upc.ToLowerInvariant().Contains(criteria) ||
                x.Sku.ToLowerInvariant().Contains(criteria) ||
                x.Description.ToLowerInvariant().Contains(criteria)).ToList();
            }

            return products;
        }

        private static List<Product> GetProductListForOrderMultipleCategories(Order order, bool isCredit, int categoryId)
        {
            List<Product> products = new List<Product>();

            if (isCredit && !Config.AuthProdsInCredit)
            {
                if (Config.OnlyKitInCredit && order.OrderType == OrderType.Order)
                    products = Product.GetProductsKit(order, categoryId).ToList();
                else
                {
                    products = Product.Products.Where(x => x.CategoryId > 0).ToList();
                }
            }
            else
            {
                if (order.OrderType == OrderType.Load)
                    products = Product.Products.Where(x => x.CategoryId > 0).ToList();
                else
                    products = Product.GetProductVisibleToClient(order.Client).Where(x => x.CategoryId > 0).ToList();
            }

            var catProduct = new List<Product>();

            var prodIds = CategoryProduct.List.Where(x => x.categoryId == categoryId).Select(x => x.productId);
            foreach (var prod in prodIds)
            {
                var temp_prod = products.FirstOrDefault(x => x.ProductId == prod);
                if (temp_prod != null)
                    catProduct.Add(temp_prod);
            }

            if (catProduct.Count == 0)
            {
                //do default cat items
                catProduct = products.Where(x => x.CategoryId == categoryId).ToList();
            }

            products = catProduct;

            if (!isCredit && order.OrderType != OrderType.Load)
            {
                if (!Config.CanGoBelow0 && (Config.TrackInventory || Config.CheckInventoryInPreSale))
                    products = products.Where(x => x.GetInventory(order.AsPresale) > 0).ToList();
            }

            List<Product> new_products = null;
            if (isCredit)
            {
                new_products = new List<Product>();
                foreach (var prod in products)
                {
                    var core = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "AvailableIn");

                    if (core != null)
                    {
                        string availableIn = core.Item2.ToString();
                        if (availableIn.ToLowerInvariant().Contains("credit"))
                        {
                            new_products.Add(prod);
                        }
                    }
                    else
                    {
                        new_products.Add(prod);
                    }
                }
            }
            else
            {
                new_products = new List<Product>();
                foreach (var prod in products)
                {
                    var core = prod.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "AvailableIn");

                    if (core != null)
                    {
                        string availableIn = core.Item2.ToString();
                        if (availableIn.ToLowerInvariant().Contains("order"))
                        {
                            new_products.Add(prod);
                        }
                    }
                    else
                    {
                        new_products.Add(prod);
                    }
                }
            }

            if (new_products != null)
                return new_products;
            else
                return products;
        }

        public static string GetFirstUpcOnly(string fullUPC)
        {
            if (string.IsNullOrEmpty(fullUPC))
                return "";

            var parts = fullUPC.Split(",");

            return parts[0];
        }
    }

}

