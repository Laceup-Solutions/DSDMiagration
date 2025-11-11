using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace LaceupMigration
{
    public class LaceupProductLabelDecoder : BarcodeDecoder
    {
        public LaceupProductLabelDecoder(string s) : base(s)
        {
            try
            {
                var data = s;

                var lenght = s.Length;

                var matches = ProductLabel.ProductLabels.Where(x => x.Active && x.ProductLabelParameterValues.Sum(x => x.Qty) == lenght);

                var list = new List<OBJ_DECODER>();

                var DecodedItems = new List<OBJ_DECODER2>();

                foreach (var match in matches)
                {
                    list.Clear();

                    if (match != null)
                    {
                        try
                        {
                            var param = match.ProductLabelParameterValues.OrderBy(x => x.Position);

                            var decodedItem = new OBJ_DECODER2();

                            int lastIndex = 0;

                            foreach (var p in param)
                            {
                                list.Add(new OBJ_DECODER() { parameter = p, data = data.Substring(lastIndex, p.Qty ?? 0) });
                                lastIndex += (p.Qty ?? 0);
                            }

                            Product product;

                            var defaultFormat = "yyMMdd";

                            foreach (var part in list)
                            {
                                switch ((ParamType)part.parameter.ProductLabelParameter.Type)
                                {
                                    case ParamType.Ignore:
                                        break;
                                    case ParamType.UPC:
                                        product = ActivityExtensionMethods.GetProduct(null, part.data);
                                        decodedItem.UPC = part.data;
                                        decodedItem.ProductId = product != null ? product.ProductId : 0;
                                        break;
                                    case ParamType.Code:
                                        product = ActivityExtensionMethods.GetProduct(null, part.data);
                                        if (string.IsNullOrEmpty(decodedItem.UPC))
                                            decodedItem.UPC = product != null ? product.Upc : string.Empty;
                                        decodedItem.ProductId = product != null ? product.ProductId : 0;
                                        break;
                                    case ParamType.Lot:
                                        decodedItem.Lot = (!string.IsNullOrEmpty(part.data) ? part.data.ToLower() : "");
                                        break;
                                    case ParamType.WeightLBs:
                                        int weightValue = 0;
                                        string formatWgLB = part.parameter.Format;
                                        if (string.IsNullOrEmpty(formatWgLB))
                                        {
                                            var weightFixed = part.data.Replace(".", "");
                                            Int32.TryParse(weightFixed, out weightValue);
                                            decodedItem.Weight = (float)(weightValue / 100);
                                            decodedItem.Weight = Math.Round(decodedItem.Weight, 2);
                                            decodedItem.Conversion = decodedItem.Weight;
                                        }
                                        else
                                        {
                                            int length = formatWgLB.Length - 1;
                                            var weightFixed = part.data.Replace(".", "");
                                            Int32.TryParse(weightFixed, out weightValue);

                                            var stringFormat = "D" + length.ToString(CultureInfo.InvariantCulture);
                                            string formattedNumber = weightValue.ToString(stringFormat);

                                            var resultString = string.Empty;
                                            for (int x = 0; x < length; x++)
                                                resultString += formattedNumber[x];

                                            var dotIndex = formatWgLB.IndexOf('.');
                                            var amount_after_dot = formatWgLB.Substring(dotIndex + 1);

                                            var factor = 1;
                                            for (int x = 0; x < amount_after_dot.Length; x++)
                                                factor *= 10;

                                            var weightLB = float.Parse(resultString);

                                            var tempWeight = weightLB;

                                            var weight_ = tempWeight / factor;

                                            //double truncatedNumber = Math.Truncate(weight_ * Math.Pow(10, amount_after_dot.Length)) / Math.Pow(10, amount_after_dot.Length);
                                            double truncatedNumber = Math.Round(weight_, 2);

                                            decodedItem.Weight = truncatedNumber;

                                            decodedItem.Conversion = decodedItem.Weight;
                                        }
                                        break;
                                    case ParamType.WeightKGs:
                                        int weightValue1 = 0;
                                        var formatWgKG = part.parameter.Format;
                                        
                                        var weightFixedKG = part.data.Replace(".", "");

                                        if (string.IsNullOrEmpty(formatWgKG))
                                        {
                                            Int32.TryParse(weightFixedKG, out weightValue1);
                                            decodedItem.Weight = (float)(weightValue1 / 100) * (2.20462);
                                            decodedItem.Weight = Math.Round(decodedItem.Weight, 2);
                                            decodedItem.Conversion = decodedItem.Weight;
                                        }
                                        else
                                        {
                                            int length = formatWgKG.Length - 1;
                                            Int32.TryParse(weightFixedKG, out weightValue);

                                            var stringFormat = "D" + length.ToString(CultureInfo.InvariantCulture);
                                            string formattedNumber = weightValue.ToString(stringFormat);

                                            var resultString = string.Empty;
                                            for (int x = 0; x < length; x++)
                                                resultString += formattedNumber[x];

                                            var dotIndex = formatWgKG.IndexOf('.');
                                            var amount_after_dot = formatWgKG.Substring(dotIndex + 1);

                                            var factor = 1;
                                            for (int x = 0; x < amount_after_dot.Length; x++)
                                                factor *= 10;

                                            var weightLB = float.Parse(resultString);

                                            var tempWeight = weightLB * (2.20462);

                                            var weight_ = tempWeight / factor;

                                            //double truncatedNumber = Math.Truncate(weight_ * Math.Pow(10, amount_after_dot.Length)) / Math.Pow(10, amount_after_dot.Length);
                                            double truncatedNumber = Math.Round(weight_, 2);

                                            decodedItem.Weight = truncatedNumber;

                                            decodedItem.Conversion = decodedItem.Weight;
                                        }
                                        break;
                                    case ParamType.VendorPartNumber:
                                        //what is this for?
                                        break;
                                    case ParamType.Package:
                                        int units = 0;
                                        int.TryParse(part.data, out units);
                                        decodedItem.Conversion = units;
                                        break;
                                    case ParamType.ExpDate:
                                        var format = string.IsNullOrEmpty(part.parameter.Format) ? defaultFormat : part.parameter.Format;
                                        var expDate = DateTime.ParseExact(part.data, format, System.Globalization.CultureInfo.InvariantCulture);
                                        decodedItem.DecodedDate = expDate;
                                        break;
                                    case ParamType.Date:
                                        var format1 = string.IsNullOrEmpty(part.parameter.Format) ? defaultFormat : part.parameter.Format;
                                        var expDate1 = DateTime.ParseExact(part.data, format1, System.Globalization.CultureInfo.InvariantCulture);
                                        decodedItem.DecodedDate = expDate1;
                                        break;
                                    case ParamType.ProductDate:
                                        var format2 = string.IsNullOrEmpty(part.parameter.Format) ? defaultFormat : part.parameter.Format;
                                        var productionDate = DateTime.ParseExact(part.data, format2, System.Globalization.CultureInfo.InvariantCulture);
                                        decodedItem.DecodedDate = productionDate;
                                        break;
                                }

                            }

                            decodedItem.LabelId = match.Id;

                            DecodedItems.Add(decodedItem);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }

                if (matches.Count() == 0 || DecodedItems.Count() == 0)
                    throw new Exception("Not found valid match");

                bool didDecode = false;
                foreach (var item in DecodedItems)
                {
                    var product = Product.Find(item.ProductId);

                    if (product == null)
                        continue;

                    var label = ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == item.LabelId);

                    if (label.ProductLabelProducts.Any(x => x.ProductId == item.ProductId))
                    {
                        //can decode using this one -> break after
                        UPC = item.UPC;
                        Product = product;
                        Weight = (float)item.Weight;
                        Expiration = item.DecodedDate;
                        Lot = item.Lot;
                        didDecode = true;
                        LabelId = label.Id;
                        break;
                    }
                }

                if (!didDecode)
                {
                    //could not find any label assigned to this product take first for now
                    foreach (var item in DecodedItems)
                    {
                        var product = Product.Find(item.ProductId);

                        if (product == null)
                            continue;

                        UPC = item.UPC;
                        Product = product;
                        Weight = (float)item.Weight;
                        Expiration = item.DecodedDate;
                        Lot = item.Lot;
                        LabelId = item.LabelId;
                        didDecode = true;
                        break;
                    }
                }

                if (!didDecode)
                    throw new Exception("Not found valid match");

            }
            catch (Exception ex)
            {
                    var product = ActivityExtensionMethods.GetProduct(null, s);
                    Product = product != null ? product : null;
            }

            Qty = 1;
        }
    }

    public class OBJ_DECODER2
    {
        public int LabelId { get; set; }
        public string UPC { get; set; }

        public int ProductId { get; set; }

        public string Lot { get; set; }

        public double Weight { get; set; }

        public double Conversion { get; set; }

        public DateTime DecodedDate { get; set; }
    }

    public enum ParamType
    {
        NoSet = 0,
        Ignore = 1,
        Date = 2,
        ExpDate = 3,
        ProductDate = 4,
        UPC = 5,
        Lot = 6,
        WeightLBs = 7,
        VendorPartNumber = 8,
        Package = 9,
        Code = 10,
        WeightKGs = 11
    }

    class OBJ_DECODER
    {
        public ProductLabelParameterValue parameter { get; set; }
        public string data { get; set; }
    }

    public class ProductLabel
    {
        public static List<ProductLabel> ProductLabels = new List<ProductLabel>();
        public ProductLabel()
        {
            this.ProductLabelParameterValues = new List<ProductLabelParameterValue>();
            this.ProductLabelProducts = new List<ProductLabelProduct>();
            this.ProductLabelVendors = new List<ProductLabelVendor>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool Active { get; set; }
        public string Comments { get; set; }
        public string ExtraFields { get; set; }
        public Nullable<int> DefaultVendorId { get; set; }
        public int LabelType { get; set; }

        public List<ProductLabelParameterValue> ProductLabelParameterValues { get; set; }
        public List<ProductLabelProduct> ProductLabelProducts { get; set; }
        public List<ProductLabelVendor> ProductLabelVendors { get; set; }
        public Vendor Vendor
        {
            get
            {
                if (DefaultVendorId == null)
                    return null;

                return Vendor.List.FirstOrDefault(x => x.Id == DefaultVendorId.Value);
            }
        }

    }

    public class ProductLabelParameter
    {
        public static List<ProductLabelParameter> ProductLabelParameters = new List<ProductLabelParameter>();

        public ProductLabelParameter()
        {
            this.ProductLabelParameterValues = new List<ProductLabelParameterValue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public int Qty { get; set; }
        public bool Single { get; set; }
        public int Type { get; set; }
        public List<ProductLabelParameterValue> ProductLabelParameterValues { get; set; }
    }

    public class ProductLabelParameterValue
    {
        public static List<ProductLabelParameterValue> ProductLabelParameterValues = new List<ProductLabelParameterValue>();

        public int Id { get; set; }
        public int LabelId { get; set; }
        public int ParameterId { get; set; }
        public int Position { get; set; }
        public string Format { get; set; }
        public Nullable<int> Qty { get; set; }

        public ProductLabel ProductLabel
        {
            get
            {
                return ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == LabelId);
            }
        }

        public ProductLabelParameter ProductLabelParameter
        {
            get
            {
                return ProductLabelParameter.ProductLabelParameters.FirstOrDefault(x => x.Id == ParameterId);
            }
        }
    }

    public class ProductLabelProduct
    {
        public static List<ProductLabelProduct> ProductLabelProducts = new List<ProductLabelProduct>();

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductLabelId { get; set; }
        public string ExtraFields { get; set; }

        public Product Product
        {
            get
            {
                return Product.Find(ProductId);
            }
        }

        public ProductLabel ProductLabel
        {
            get
            {
                return ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == ProductLabelId);
            }
        }
    }

    public class ProductLabelVendor
    {
        public static List<ProductLabelVendor> ProductLabelVendors = new List<ProductLabelVendor>();
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int ProductLabelId { get; set; }
        public string ExtraFields { get; set; }

        public Vendor Product
        {
            get
            {
                return Vendor.List.FirstOrDefault(x => x.Id == VendorId);
            }
        }

        public ProductLabel ProductLabel
        {
            get
            {
                return ProductLabel.ProductLabels.FirstOrDefault(x => x.Id == ProductLabelId);
            }
        }

    }

}