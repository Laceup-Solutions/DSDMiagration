namespace LaceupMigration;

  public static class ActivityExtensionMethods
    {

        public static List<Product> FindScannedProducts(string data, Order order = null)
        {
            Logger.CreateLog("OnDecodeData called for Socket Mobile Scanner");

            if (!string.IsNullOrEmpty(data))
            {
                //see if this UPC belong to a product
                var products = GetProducts(order, data);

                var p = Product.Products.Where(x => x.Upc == data || x.Sku == data);

                if (products.Count() == 0)
                {
                    bool exists = false;
                    var f = CheckForInv(order, data);
                    if (f != null && f.Count() > 0)
                        exists = true;

                    if (p.Count() > 0 && order != null && order.Client.CategoryId != 0)
                    {
                        return null;
                    }

                    var inventoryMessage = "is not part of the current inventory.";

                    if (exists && f.Count() > 0)
                        inventoryMessage = "There is not enough inventory of " + f.FirstOrDefault().Name;

                    return null;
                }

                if (products.Count == 1)
                {
                    var product = products.FirstOrDefault();
                    if (!string.IsNullOrEmpty(product.NonVisibleExtraFieldsAsString))
                    {
                        var available = DataAccess.GetSingleUDF("AvailableIn", product.NonVisibleExtraFieldsAsString);
                        if (!string.IsNullOrEmpty(available))
                        {
                            if (available.ToLower() == "none")
                            {
                                return null;
                            }
                        }
                    }
                }

                return products.ToList();
            }
            return new List<Product>();
        }

        static List<Product> GetProducts(Order order, string data)
        {
            var lst = GetAvailableProducts(order).ToList();

            var products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && x.Upc == data);
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku == data);

            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && x.Upc.Contains(data));
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && data.Contains(x.Upc));

            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku.Contains(data));
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && data.Contains(x.Sku));

            return products.ToList();
        }

        static List<Product> CheckForInv(Order order, string data)
        {
            var lst = Product.Products.ToList();

            var products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && x.Upc == data);
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku == data);

            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && x.Upc.Contains(data));
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Upc) && data.Contains(x.Upc));

            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku.Contains(data));
            if (products.Count() == 0)
                products = lst.Where(x => x != null && !string.IsNullOrEmpty(x.Sku) && data.Contains(x.Sku));

            return products.ToList();
        }

        public static Product GetProduct(Order order, string data)
        {
            var products = GetAvailableProducts(order);

            var product = products.FirstOrDefault(x => !string.IsNullOrEmpty(x.Upc) && x.Upc == data);
            if (product == null)
                product = products.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku == data);

            if (product == null)
                product = products.FirstOrDefault(x => !string.IsNullOrEmpty(x.Upc) && x.Upc.Contains(data));
            if (product == null)
                product = products.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.Upc) && data.Contains(x.Upc));

            if (product == null)
                product = products.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.Sku) && x.Sku.Contains(data));
            if (product == null)
                product = products.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.Sku) && data.Contains(x.Sku));

            return product;
        }


        static List<Product> GetAvailableProducts(Order order)
        {
            List<Product> products = new List<Product>();

            if (order == null || order.Client == null)
                products = Product.Products.Where(x => x.CategoryId > 0).ToList();
            else
            {
                products = Product.GetProductVisibleToClient(order.Client).Where(x => x.CategoryId > 0).ToList();

                if (order != null && (order.OrderType == OrderType.Order || order.OrderType == OrderType.Consignment))
                    products = products.Where(x => x.GetInventory(order.AsPresale) > 0).ToList();
            }

            return products;
        }
    }