using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;


namespace LaceupMigration
{
    public static class MyExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return (IQueryable<T>)OrderBy((IQueryable)source, propertyName);
        }

        public static IQueryable OrderBy(this IQueryable source, string propertyName)
        {
            var x = Expression.Parameter(source.ElementType, "x");
            var selector = Expression.Lambda(Expression.PropertyOrField(x, propertyName), x);
            return source.Provider.CreateQuery(
                Expression.Call(typeof(Queryable), "OrderBy", new Type[] { source.ElementType, selector.Body.Type },
                    source.Expression, selector
                ));
        }

        public static string ToString(this OrderType ot, bool fromBatch = false)
        {
            switch (ot)
            {
                case OrderType.Order:
                    {
                        if (fromBatch)
                            return "Invoice #:";
                        return "Sales Order";
                    }
                case OrderType.Credit:
                    return "Credit";
                case OrderType.Return:
                    return "Return";
                case OrderType.NoService:
                    return "No Service";
                case OrderType.Bill:
                    return "Bill";
                case OrderType.Load:
                    return "Load Order";
                case OrderType.Consignment:
                    return "Consignment";
                case OrderType.Group:
                    return "Group Order";
                default:
                    return ot.ToString();
            }
        }

        public static string ToString(this DiscountType od)
        {
            switch (od)
            {
                case DiscountType.Percent:
                    return "Percentage";
                case DiscountType.Amount:
                    return "Amount";
                default:
                    return od.ToString();
            }
        }

        public static string ToString(this InvoicePaymentMethod od)
        {
            switch (od)
            {
                case InvoicePaymentMethod.Cash:
                    return "Cash";
                case InvoicePaymentMethod.Check:
                    return "Check";
                case InvoicePaymentMethod.Credit_Card:
                    return "Credit Card";
                case InvoicePaymentMethod.Money_Order:
                    return "Money Order";
                case InvoicePaymentMethod.Transfer:
                    return "Transfer";
                case InvoicePaymentMethod.Zelle_Transfer:
                    return "Zelle";
                default:
                    return od.ToString();
            }
        }

        public static string ToText(int value)
        {
            string Num2Text = "";
            if (value < 0)
                return "menos " + ToText(Math.Abs(value));

            if (value == 0) Num2Text = "Cero";
            else if (value == 1) Num2Text = "Uno";
            else if (value == 2) Num2Text = "Dos";
            else if (value == 3) Num2Text = "Tres";
            else if (value == 4) Num2Text = "Cuatro";
            else if (value == 5) Num2Text = "Cinco";
            else if (value == 6) Num2Text = "Seis";
            else if (value == 7) Num2Text = "Siete";
            else if (value == 8) Num2Text = "Ocho";
            else if (value == 9) Num2Text = "Nueve";
            else if (value == 10) Num2Text = "Diez";
            else if (value == 11) Num2Text = "Once";
            else if (value == 12) Num2Text = "Doce";
            else if (value == 13) Num2Text = "Trece";
            else if (value == 14) Num2Text = "Catorce";
            else if (value == 15) Num2Text = "Quince";
            else if (value < 20) Num2Text = "Dieci" + ToText((value - 10));
            else if (value == 20) Num2Text = "Veinte";
            else if (value < 30) Num2Text = "Veinti" + ToText((value - 20));
            else if (value == 30) Num2Text = "Treinta";
            else if (value == 40) Num2Text = "Cuarenta";
            else if (value == 50) Num2Text = "Cincuenta";
            else if (value == 60) Num2Text = "Sesenta";
            else if (value == 70) Num2Text = "Setenta";
            else if (value == 80) Num2Text = "Ochenta";
            else if (value == 90) Num2Text = "Noventa";
            else if (value < 100)
            {
                int u = value % 10;
                Num2Text = string.Format("{0} y {1}", (ToText((value / 10) * 10)), (u == 1 ? "un" : ToText(value % 10)));
            }
            else if (value == 100) Num2Text = "Cien";
            else if (value < 200) Num2Text = "Ciento " + ToText(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800))
                Num2Text = ToText(value / 100) + "cientos";
            else if (value == 500) Num2Text = "Quinientos";
            else if (value == 700) Num2Text = "Setecientos";
            else if (value == 900) Num2Text = "Novecientos";
            else if (value < 1000) Num2Text = string.Format("{0} {1}", ToText((value / 100) * 100), ToText(value % 100));
            else if (value == 1000) Num2Text = "Mil";
            else if (value < 2000) Num2Text = "Mil " + ToText(value % 1000);
            else if (value < 1000000)
            {
                Num2Text = ToText(value / 1000) + " Mil";
                if ((value % 1000) > 0) Num2Text += " " + ToText(value % 1000);
            }
            else if (value == 1000000) Num2Text = "un mill�n";
            else if (value < 2000000) Num2Text = "un mill�n " + ToText(value % 1000000);
            else if (value < int.MaxValue)
            {
                Num2Text = ToText(value / 1000000) + " millones";
                if ((value - (value / 1000000) * 1000000) > 0) Num2Text += " " + ToText(value - (value / 1000000) * 1000000);
            }
            return Num2Text;
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }

    public static class ZipMethods
    {
        public static void ZipFile(string fileToZip, string targetFile, int compressionLevel, int blockSize)
        {
            if (!System.IO.File.Exists(fileToZip))
            {
                Exception e = new System.IO.FileNotFoundException("The specified file " + fileToZip + " could not be found. Zipping aborderd");
                //    Logger.CreateLog(e);
                throw e;
            }

            System.IO.FileStream StreamToZip = new System.IO.FileStream(fileToZip, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.FileStream ZipFile = System.IO.File.Create(targetFile);
            ZipOutputStream ZipStream = new ZipOutputStream(ZipFile);
            ZipEntry ZipEntry = new ZipEntry("ZippedFile");
            ZipStream.PutNextEntry(ZipEntry);
            ZipStream.SetLevel(compressionLevel);
            byte[] buffer = new byte[blockSize];
            System.Int32 size = StreamToZip.Read(buffer, 0, buffer.Length);
            ZipStream.Write(buffer, 0, size);
            try
            {
                while (size < StreamToZip.Length)
                {
                    int sizeRead = StreamToZip.Read(buffer, 0, buffer.Length);
                    ZipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //Xamarin.Insights.Report(ex);
                throw;
            }
            ZipStream.Finish();
            ZipStream.Close();
            StreamToZip.Close();
        }

        public static void UnzipFile(string sourceFile, string targetFile)
        {

            ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile));

            //ZipEntry theEntry;
            while ((s.GetNextEntry()) != null)
            {

                FileStream streamWriter = File.Create(targetFile);

                int size = 2048;
                byte[] data = new byte[2048];
                while (true)
                {
                    size = s.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        streamWriter.Write(data, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }

                streamWriter.Close();
            }
            s.Close();
        }

        public static void UnzipComplexFile(string sourceFile, string targetFolder)
        {

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    string targetFile = Path.Combine(targetFolder, theEntry.Name);
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);
                    using (FileStream streamWriter = File.Create(targetFile))
                    {
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public class DataListItem
    {
        public string Text { get; set; }

        public object Value { get; set; }

        public int Index { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pair(T1 t1, T2 t2)
        {
            Item1 = t1;
            Item2 = t2;
        }
    }

    public class InventoryProd
    {
        public Product Product { get; set; }

        public string Lots { get; set; }

        public float Qty { get; set; }

        public List<TruckInventory> ProdLots { get; set; }
    }

    public class KitItem
    {
        public KitPartItem Kit { get; set; }

        public List<KitPartItem> Parts { get; set; }

        public KitItem(Product product, string kit, float qty)
        {
            Parts = new List<KitPartItem>();

            Kit = new KitPartItem(product);
            Kit.MaxValue = qty;
            Kit.MinValue = 1;

            var parts = kit.Split('/');
            foreach (var p in parts)
            {
                var ps = p.Split(',');
                int prodId = 0;

                int.TryParse(ps[0], out prodId);

                if (prodId > 0)
                {
                    var prod = Product.Products.FirstOrDefault(x => x.ProductId == prodId);
                    if (prod != null)
                    {
                        float x = 0;
                        float.TryParse(ps[1], out x);
                        Parts.Add(new KitPartItem(prod) { MinValue = x, MaxValue = qty * x });
                    }
                }
            }
        }

        public void AdjustQty(OrderDetail detail)
        {
            if(detail.Product.ProductId == Kit.ProductId)
                Kit.CurrentValue = detail.Qty;
            else
                foreach (var item in Parts)
                    if (item.ProductId == detail.Product.ProductId)
                        item.CurrentValue = detail.Qty;
        }

        public bool CanChangeQty(Product prod, float qty)
        {
            if(prod.ProductId == Kit.ProductId)
                foreach (var item in Parts)
                        item.CurrentValue += qty * item.MinValue;
            else
            {
                foreach (var item in Parts)
                    if (item.ProductId == prod.ProductId)
                        item.CurrentValue = qty + (Kit.CurrentValue * item.MinValue);
            }

            return !Parts.Any(x => x.CurrentValue > x.MaxValue);
        }

        public bool CanDecreaseQtyInDelivery(Product prod, float qty)
        {
            if (prod.ProductId == Kit.ProductId)
            {
                foreach (var item in Parts)
                {
                    var kitsum = item.CurrentValue + (Kit.CurrentValue * item.MinValue);

                    var toCheck = qty * item.MinValue;

                    if (kitsum > toCheck)
                        return false;
                }
            }
            else
                return true;
            //else
            //{
            //    foreach (var item in Parts)
            //        if (item.ProductId == prod.ProductId)
            //            item.CurrentValue = qty + (Kit.CurrentValue * item.MinValue);
            //}
            return true;
        }
    }

    public class KitPartItem
    {
        public int ProductId { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public float CurrentValue { get; set; }

        public KitPartItem(Product product)
        {
            ProductId = product.ProductId;
        }
    }
}
