using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;







using System.Globalization;



namespace LaceupMigration
{
    public static class SortDetails
    {

        public enum SortCriteria
        {
            ProductName = 0,
            ProductCode = 1,
            Category = 2,
            InStock = 3,
            Qty = 4,
            Descending = 5,
            OrderOfEntry = 6,
            WarehouseLocation = 7,
            CategoryThenByCode = 8
        }

        public static SortCriteria GetCriteriaFromName(string originalString)
        {
            switch (originalString)
            {
                case "name":
                    return SortCriteria.ProductName;
                case "code":
                    return SortCriteria.ProductCode;
                case "category":
                    return SortCriteria.Category;
                case "instock":
                    return SortCriteria.InStock;
                case "qty":
                    return SortCriteria.Qty;
                case "desc":
                    return SortCriteria.Descending;
                case "id":
                    return SortCriteria.OrderOfEntry;
                case "location":
                    return SortCriteria.WarehouseLocation;
                case "categorythencode":
                    return SortCriteria.CategoryThenByCode;
                default:
                    return SortCriteria.ProductName;
            }
        }

        public static void SaveSortCriteria(SortCriteria criteria)
        {
            switch (criteria)
            {
                case SortCriteria.ProductName:
                    Config.PrintInvoiceSort = "name";
                    break;
                case SortCriteria.ProductCode:
                    Config.PrintInvoiceSort = "code";
                    break;
                case SortCriteria.Category:
                    Config.PrintInvoiceSort = "category";
                    break;
                case SortCriteria.InStock:
                    Config.PrintInvoiceSort = "instock";
                    break;
                case SortCriteria.Qty:
                    Config.PrintInvoiceSort = "qty";
                    break;
                case SortCriteria.Descending:
                    Config.PrintInvoiceSort = "desc";
                    break;
                case SortCriteria.OrderOfEntry:
                    Config.PrintInvoiceSort = "id";
                    break;
                case SortCriteria.WarehouseLocation:
                    Config.PrintInvoiceSort = "location";
                    break;
                case SortCriteria.CategoryThenByCode:
                    Config.PrintInvoiceSort = "categorythencode";
                    break;
                default:
                    Config.PrintInvoiceSort = "";
                    break;
            }

            Config.SaveSettings();
        }



        public static IQueryable<Line> SortedDetails(IList<Line> lines)
        {

            IQueryable<Line> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<Line>>();
                    foreach (var od in lines)
                    {
                        var t = new T<Line>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<Line>>();
                    foreach (var od in lines)
                    {
                        var t = new T<Line>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "id":
                    retList = lines.OrderBy(x => x.OrderDetail == null ? 0 : x.OrderDetail.OrderDetailId).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<Line>>();
                    foreach (var od in lines)
                    {
                        var t = new T<Line>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            if (OrderDiscount.HasDiscounts)
            {
                var listLineShow = retList.Where(x => (x.OrderDetail != null && !x.OrderDetail.FromOffer)).ToList();

                var listLineProductFree = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && !x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).ToList();
                foreach (var item in listLineProductFree)
                {
                    listLineShow.Add(item);
                    string uniqId = item.OrderDetail.OriginalId.ToString() ?? null;
                    var discount = retList.FirstOrDefault(x => (x.OrderDetail != null) && (x.OrderDetail.ExtraFields.Contains("UniqueId=" + uniqId)));
                    if (discount != null)
                    {
                        listLineShow.Add(discount);
                    }
                }

                var secondList = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).OrderBy(x => x.Product.Name).ThenByDescending(x => x.CurrentPrice).ToList();

                listLineShow.AddRange(secondList);

                var linesFromHistory = retList.Where(x => x.OrderDetail == null);

                listLineShow.AddRange(linesFromHistory);

                retList = listLineShow.ToList().AsQueryable();
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<InventorySettlementRow> SortedDetails(IEnumerable<InventorySettlementRow> source)
        {
            IQueryable<InventorySettlementRow> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = source.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = source.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<InventorySettlementRow>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventorySettlementRow>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<InventorySettlementRow>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventorySettlementRow>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = source.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = source.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = source.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<InventorySettlementRow>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventorySettlementRow>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<InventoryLine> SortedDetails(IList<InventoryLine> source)
        {

            IQueryable<InventoryLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = source.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = source.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<InventoryLine>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventoryLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<InventoryLine>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventoryLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = source.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = source.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = source.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<InventoryLine>>();
                    foreach (var od in source)
                    {
                        var t = new T<InventoryLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<CycleCountItem> SortedDetails(IList<CycleCountItem> source)
        {

            IQueryable<CycleCountItem> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = source.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = source.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<CycleCountItem>>();
                    foreach (var od in source)
                    {
                        var t = new T<CycleCountItem>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<CycleCountItem>>();
                    foreach (var od in source)
                    {
                        var t = new T<CycleCountItem>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = source.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = source.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = source.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<CycleCountItem>>();
                    foreach (var od in source)
                    {
                        var t = new T<CycleCountItem>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = source.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<Product> SortedDetails(IList<Product> source)
        {

            IQueryable<Product> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = source.OrderBy(x => x.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = source.OrderBy(x => x.Name).AsQueryable();
                    break;
                case "category":
                    retList = source.OrderBy(x => x.CategoryId).ThenBy(x => x.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<Product>>();
                    foreach (var od in source)
                    {
                        var t = new T<Product>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<Product>>();
                    foreach (var od in source)
                    {
                        var t = new T<Product>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = source.OrderBy(x => x.Upc).AsQueryable();
                    break;
                case "price":
                    retList = source.OrderBy(x => x.PriceLevel0).AsQueryable();
                    break;
                default:
                    retList = source.OrderBy(x => x.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<InvoiceDetail> SortedDetails(IEnumerable<InvoiceDetail> lines)
        {
            var Details = lines;
            var sortableDetails = new List<InvoiceDetail>();
            foreach (var d in Details)
                //if (d.Product == null)
                //{
                //    d.Product = Product.Find(d.ProductId);
                //    if (d != null)
                //        sortableDetails.Add(d);
                //}
                //else
                sortableDetails.Add(d);
            IQueryable<InvoiceDetail> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = sortableDetails.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = sortableDetails.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = sortableDetails.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<InvoiceDetail>>();
                    foreach (var od in sortableDetails)
                    {
                        var t = new T<InvoiceDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x != null && x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<InvoiceDetail>>();
                    foreach (var od in sortableDetails)
                    {
                        var t = new T<InvoiceDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x != null && x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    return sortableDetails.OrderBy(x => x.Product.Upc).AsQueryable();
                case "price":
                    retList = sortableDetails.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = sortableDetails.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<InvoiceDetail>>();
                    foreach (var od in sortableDetails)
                    {
                        var t = new T<InvoiceDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = sortableDetails.AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<OdLine> SortedDetails(int clientId, List<OdLine> lines)
        {
            var order = ClientProdSort.GetSortForClient(clientId);

            int offset = lines.Count;

            foreach (var item in lines)
            {
                var pos = order.IndexOf(item.Product.ProductId);

                item.OrginalPosInList = pos;

                if (pos == -1)
                    pos = ++offset;

                item.PositionInList = pos;
            }

            return lines.OrderBy(x => x.PositionInList).AsQueryable();
        }

        public static IQueryable<OrderDetail> SortedDetails(IList<OrderDetail> lines)
        {
            var Details = lines;
            IQueryable<OrderDetail> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = Details.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = Details.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = Details.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<OrderDetail>>();
                    foreach (var od in Details)
                    {
                        var t = new T<OrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<OrderDetail>>();
                    foreach (var od in Details)
                    {
                        var t = new T<OrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = Details.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = Details.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "id":
                    retList = Details.OrderBy(x => x.OrderDetailId).AsQueryable();
                    break;
                case "location":
                    retList = Details.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<OrderDetail>>();
                    foreach (var od in Details)
                    {
                        var t = new T<OrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = Details.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<OrderLine> SortedDetails(IList<OrderLine> lines)
        {

            IQueryable<OrderLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<OrderLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OrderLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<OrderLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OrderLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "id":
                    retList = lines.OrderBy(x => x.OrderDetail == null ? 0 : x.OrderDetail.OrderDetailId).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<OrderLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OrderLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            if (OrderDiscount.HasDiscounts)
            {
                var listLineShow = retList.Where(x => (x.OrderDetail != null && !x.OrderDetail.FromOffer)).ToList();

                var listLineProductFree = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && !x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).ToList();
                foreach (var item in listLineProductFree)
                {
                    listLineShow.Add(item);
                    string uniqId = item.OrderDetail.OriginalId.ToString() ?? null;
                    var discount = retList.FirstOrDefault(x => (x.OrderDetail != null) && (x.OrderDetail.ExtraFields.Contains("UniqueId=" + uniqId)));
                    if (discount != null)
                    {
                        listLineShow.Add(discount);
                    }
                }

                var secondList = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).OrderBy(x => x.Product.Name).ThenByDescending(x => x.Price).ToList();

                listLineShow.AddRange(secondList);

                var linesFromHistory = retList.Where(x => x.OrderDetail == null);

                listLineShow.AddRange(linesFromHistory);

                retList = listLineShow.ToList().AsQueryable();

            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<OrderLine> SortedDetails(int clientId, IList<OrderLine> lines)
        {
            var sort = ClientProdSort.GetSortForClient(clientId);

            Dictionary<int, OrderLine> sortedList = new Dictionary<int, OrderLine>();

            var offset = sort.Count;

            foreach (var item in lines)
            {
                var pos = sort.IndexOf(item.Product.ProductId);

                if (pos == -1)
                    pos = offset++;

                sortedList.Add(pos, item);
            }

            return sortedList.OrderBy(x => x.Key).Select(x => x.Value).AsQueryable();
        }

        public static IQueryable<OrderLine> SortedDetailsInProduct(IList<OrderLine> lines)
        {

            IQueryable<OrderLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<OrderLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OrderLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<CatalogItem> SortedDetailsInProduct(IList<CatalogItem> lines)
        {

            IQueryable<CatalogItem> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).ThenByDescending(x => x.Line.UoM != null ? x.Line.UoM.Conversion : 0).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<CatalogItem>>();
                    foreach (var od in lines)
                    {
                        var t = new T<CatalogItem>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).ThenByDescending(x => x.HoldedValue.Line.UoM != null ? x.HoldedValue.Line.UoM.Conversion : 0).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).ThenByDescending(x => x.Line.UoM != null ? x.Line.UoM.Conversion : 0).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<LoadOrderDetail> SortedDetails(IList<LoadOrderDetail> lines)
        {

            IQueryable<LoadOrderDetail> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<LoadOrderDetail>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LoadOrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<LoadOrderDetail>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LoadOrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<LoadOrderDetail>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LoadOrderDetail>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                //case "id":
                //    retList = lines.OrderBy(x => x.OrderDetail == null ? 0 : x.OrderDetail.OrderDetailId).AsQueryable();
                //    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }


            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<OdLine> SortedDetails(IList<OdLine> lines, bool asPresale = true, bool ignoreDiscounts = false)
        {

            IQueryable<OdLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<OdLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OdLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<OdLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OdLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "id":
                    retList = lines.OrderBy(x => x.OrderDetail == null ? 0 : x.OrderDetail.OrderDetailId).AsQueryable();
                    break;
                case "instock":
                    retList = lines.OrderByDescending(x => GetCurrentInventory(x.Product, asPresale)).AsQueryable();
                    break;
                case "qty":
                    retList = lines.OrderByDescending(x => x.Qty).AsQueryable();
                    break;
                case "code":
                    retList = lines.OrderBy(x => x.Product.Code).AsQueryable();
                    break;
                case "desc":
                    retList = lines.OrderByDescending(x => x.Product.Name).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<OdLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<OdLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            if (OrderDiscount.HasDiscounts && !ignoreDiscounts)
            {
                var listLineShow = retList.Where(x => (x.OrderDetail != null && !x.OrderDetail.FromOffer)).ToList();

                var listLineProductFree = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && !x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).ToList();
                foreach (var item in listLineProductFree)
                {
                    listLineShow.Add(item);
                    string uniqId = item.OrderDetail.OriginalId.ToString() ?? null;
                    var discount = retList.FirstOrDefault(x => (x.OrderDetail != null) && (x.OrderDetail.ExtraFields.Contains("UniqueId=" + uniqId)));
                    if (discount != null)
                    {
                        listLineShow.Add(discount);
                    }
                }

                var secondList = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).OrderBy(x => x.Product.Name).ThenByDescending(x => x.Price).ToList();

                listLineShow.AddRange(secondList);

                var linesFromHistory = retList.Where(x => x.OrderDetail == null);

                listLineShow.AddRange(linesFromHistory);

                retList = listLineShow.ToList().AsQueryable();
            }


            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }
        public static double GetCurrentInventory(Product product, bool asPresale)
        {
            double oh = asPresale ? product.CurrentWarehouseInventory : product.CurrentInventory;
            return Math.Round(oh, Config.Round);
        }

        public static IQueryable<OdLine> SortedDetailsInProduct(IList<OdLine> lines)
        {

            IQueryable<OdLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).ThenBy(x => x.UoM).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<OdLine>>();

                    var grouped = lines.GroupBy(x => x.Product.ProductId);

                    foreach (var od in grouped)
                    {
                        foreach (var item in od)
                        {
                            var t = new T<OdLine>();
                            var c = Category.Categories.FirstOrDefault(x => x.CategoryId == item.Product.CategoryId);
                            t.CategoryName = c == null ? string.Empty : c.Name;
                            t.HoldedValue = item;
                            list1.Add(t);
                        }
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            if (OrderDiscount.HasDiscounts)
            {
                var listLineShow = retList.Where(x => (x.OrderDetail != null && !x.OrderDetail.FromOffer)).ToList();

                var listLineProductFree = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).ToList();
                foreach (var item in listLineProductFree)
                {
                    listLineShow.Add(item);
                    string uniqId = item.OrderDetail.OriginalId.ToString() ?? null;
                    var discount = retList.FirstOrDefault(x => (x.OrderDetail != null) && (x.OrderDetail.ExtraFields.Contains("UniqueId=" + uniqId)));
                    if (discount != null)
                    {
                        listLineShow.Add(discount);
                    }
                }

                var secondList = retList.Where(x => x.OrderDetail != null && x.OrderDetail.FromOffer && x.Product.IsDiscountItem && (string.IsNullOrEmpty(x.OrderDetail.ExtraFields) ? true : !x.OrderDetail.ExtraFields.Contains("UniqueId="))).OrderBy(x => x.Product.Name).ThenByDescending(x => x.Price).ToList();

                listLineShow.AddRange(secondList);

                var linesFromHistory = retList.Where(x => x.OrderDetail == null);

                listLineShow.AddRange(linesFromHistory);

                retList = listLineShow.ToList().AsQueryable();

            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<DailyParLevelLine> SortedDetails(int clientId, List<DailyParLevelLine> lines)
        {
            var order = ClientProdSort.GetSortForClient(clientId);

            int offset = lines.Count;

            foreach (var item in lines)
            {
                var pos = order.IndexOf(item.Product.ProductId);

                item.OrginalPosInList = pos;

                if (pos == -1)
                    pos = ++offset;

                item.PositionInList = pos;
            }

            return lines.OrderBy(x => x.PositionInList).AsQueryable();
        }

        public static IQueryable<DailyParLevelLine> SortedDetails(IList<DailyParLevelLine> lines)
        {

            IQueryable<DailyParLevelLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<DailyParLevelLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<DailyParLevelLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<DailyParLevelLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<DailyParLevelLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<DailyParLevelLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<DailyParLevelLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<LineStruct> SortedDetails(IList<LineStruct> lines)
        {
            IQueryable<LineStruct> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<LineStruct>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LineStruct>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<LineStruct>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LineStruct>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<LineStruct>>();
                    foreach (var od in lines)
                    {
                        var t = new T<LineStruct>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<TemplateLine> SortedDetails(IList<TemplateLine> lines)
        {
            IQueryable<TemplateLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<TemplateLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<TemplateLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<TemplateLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<TemplateLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "location":
                    retList = lines.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                case "categorythencode":
                    var list3 = new List<T<TemplateLine>>();
                    foreach (var od in lines)
                    {
                        var t = new T<TemplateLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list3.Add(t);
                    }
                    retList = list3.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Code).Select(x => x.HoldedValue).AsQueryable();
                    break;
                //case "id":
                //    retList = lines.OrderBy(x => x.OrderDetail == null ? 0 : x.OrderDetail.OrderDetailId).AsQueryable();
                //    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0 && !Config.ShowDiscountByPriceLevel)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        public static IQueryable<TransferLine> SortedDetails(IList<TransferLine> lines)
        {
            var Details = lines;
            IQueryable<TransferLine> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = Details.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = Details.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "desc":
                    retList = lines.OrderByDescending(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = Details.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<T<TransferLine>>();
                    foreach (var od in Details)
                    {
                        var t = new T<TransferLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<T<TransferLine>>();
                    foreach (var od in Details)
                    {
                        var t = new T<TransferLine>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = Details.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = Details.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                case "instock":
                    retList = Details.OrderBy(x => x.Product.CurrentInventory).AsQueryable();
                    break;
                case "id":
                    var part1 = Details.Where(x => x.Details.Count > 0).ToList();
                    var part2 = Details.Where(x => x.Details.Count == 0).ToList();

                    part1 = part1.OrderBy(x => x.Details.FirstOrDefault().Id).ThenBy(x => x.Product.Name).ToList();
                    part2 = part2.OrderBy(x => x.Product.Name).ToList();

                    var fulllist = new List<TransferLine>();
                    fulllist.AddRange(part1);
                    fulllist.AddRange(part2);
                        
                    retList = fulllist.AsQueryable();
                    break;
                case "qty":
                    retList = Details.OrderByDescending(x => x.QtyTransferred).AsQueryable();
                    break;
                case "code":
                    retList = Details.OrderBy(x => x.Product.Code).AsQueryable();
                    break;
                case "location":
                    retList = Details.OrderBy(x => string.IsNullOrEmpty(x.Product.WarehouseLocation) ? 1 : 0).ThenBy(x => x.Product.WarehouseLocation).AsQueryable();
                    break;
                default:
                    retList = Details.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            return retList;
        }

        class T<T1>
        {
            public string CategoryName { get; set; }

            public T1 HoldedValue { get; set; }
        }
    }

    public class OrderLine 
    {
        public Product Product { get; set; }

        public float Qty { get; set; }

        public double Price { get; set; }

        public OrderDetail OrderDetail { get; set; }

        public List<OrderDetail> ParticipatingDetails { get; set; }

        public string Comments { get; set; }

        public string Lot { get; set; }

        public bool Damaged { get; set; }

        public UnitOfMeasure UoM { get; set; }

        public bool FreeItem { get; set; }

        public Offer Offer { get; set; }

        public bool IsCredit { get; set; }

        public double AvgSale { get; set; }

        public bool IsHeader { get; set; }

        public double ListPrice { get; set; }

        public bool Scanned { get { return ParticipatingDetails.Any(x => x.CompletedFromScanner); } }
    }

    public class InventorySettlementRow
    {
        public Product Product { get; set; }
        public UnitOfMeasure UoM { get; set; }
        public string Lot { get; set; }
        public float BegInv { get; set; }
        public float LoadOut { get; set; }
        public float Adj { get; set; }
        public float TransferOn { get; set; }
        public float TransferOff { get; set; }
        public float Sales { get; set; }
        public float Dump { get; set; }
        public float Return { get; set; }
        public float Unload { get; set; }
        public float CreditDump { get; set; }
        public float CreditReturns { get; set; }
        public float EndInventory { get; set; }
        public float DamagedInTruck { get; set; }
        public double Weight { get; set; }
        public string OverShort
        {
            get
            {
                // 1235: removed CreditDump - Dump
                // 1400 removed creditreturns
                // 1425 , removed Returns?
                var v = (BegInv + LoadOut + Adj + TransferOn - TransferOff + CreditReturns - Sales - Unload - EndInventory - DamagedInTruck - LoadingError) * -1;

                if (Math.Round(v, 2) == 0)
                    return string.Empty;
                else
                    return Math.Round(v, Config.Round).ToString(CultureInfo.CurrentCulture);
            }
        }

        public float LoadingError { get; set; }
        public float Reshipped { get; set; }

        public bool SkipRelated { get; set; }

        public string Serialize()
        {
            try
            {
                string s = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}", "|",
                        Product.ProductId,
                        BegInv,
                        LoadOut,
                        Adj,
                        TransferOn,
                        TransferOff,
                        Sales,
                        Dump,
                        Unload,
                        CreditDump,
                        CreditReturns,
                        EndInventory,
                        DamagedInTruck,
                        SkipRelated,
                        Lot,
                        LoadingError,
                        Reshipped
                        );
                return s;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
            }
            return string.Empty;
        }

        public InventorySettlementRow()
        {
            Lot = "";
        }

        public bool IsEmpty
        {
            get
            {
                return Math.Round(BegInv, Config.Round) == 0
                    && Math.Round(LoadOut, Config.Round) == 0
                    && Math.Round(Adj, Config.Round) == 0
                    && Math.Round(TransferOn - TransferOff, Config.Round) == 0
                    && Math.Round(Sales, Config.Round) == 0
                    && Math.Round(CreditReturns, Config.Round) == 0
                    && Math.Round(CreditDump, Config.Round) == 0
                    && Math.Round(Reshipped, Config.Round) == 0
                    && Math.Round(DamagedInTruck, Config.Round) == 0
                    && Math.Round(Unload, Config.Round) == 0
                    && Math.Round(EndInventory, Config.Round) == 0;
            }
        }

        public bool IsShort
        {
            get
            {
                return string.IsNullOrEmpty(OverShort)
                    && Math.Round(TransferOn) == 0
                    && Math.Round(TransferOff) == 0
                    && Math.Round(Adj) == 0;
            }
        }
    }

    public class Line
    {
        public bool NewProduct { get; set; }

        public Product Product { get; set; }

        public string PreviousOrderedDate { get; set; }

        public double PreviousOrderedPrice { get; set; }

        public float PreviousOrderedQty { get; set; }

        public double CurrentPrice { get; set; }

        public double ExpectedPrice { get; set; }

        public double PerWeek { get; set; }

        public OrderDetail OrderDetail { get; set; }

        public bool IsPriceFromSpecial { get; set; }

        public double Allowance { get; set; }

        public UnitOfMeasure UoM { get; set; }
    }
}