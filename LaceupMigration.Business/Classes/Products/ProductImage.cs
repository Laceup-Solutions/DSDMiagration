using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
// using MonoTouch.UIKit;

namespace LaceupMigration
{
    public static class ProductImage
    {
        private static Dictionary<int, string> productImageMap = new Dictionary<int, string>();

        public static void UpdateMap(string receivedZipFile, bool deleteActual)
        {
            if (deleteActual)
            {
                productImageMap.Clear();
                foreach (string file in Directory.GetFiles(Config.ImageStorePath))
                    File.Delete(file);
            }

            // unzip & process the file
            ZipMethods.UnzipComplexFile(receivedZipFile, Config.ImageStorePath);

            // reload map
            LoadMap();
        }

        public static void LoadMap()
        {
            // read the map file
            if (File.Exists(Config.ProductImageMappingFile))
                using (StreamReader reader = new StreamReader(Config.ProductImageMappingFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int productId = Convert.ToInt32(line, CultureInfo.InvariantCulture);
                        line = reader.ReadLine();
                        string imageFile = line;

                        if (!productImageMap.ContainsKey(productId))
                            productImageMap.Add(productId, imageFile);
                        else
                            productImageMap[productId] = imageFile;
                    }
                }
        }

        public static bool AtLeastOneProductHasImg(List<int> ProductsIds)
        {
            return ProductsIds.Any(id => productImageMap.ContainsKey(id));
        }

        public static string GetProductImage(int productId)
        {
            if (!productImageMap.ContainsKey(productId))
                return null;
            string path = productImageMap[productId];
            if (string.IsNullOrEmpty(path))
                return null;

            return Path.Combine(Config.ImageStorePath, path);
            // UIImage image = new UIImage (Path.Combine (Config.ImageStorePath, path));
            // return image;
        }

        /// <summary>
        /// Gets the product image path, or returns placeholder.png if no image exists.
        /// This matches the Xamarin pattern where placeholder is shown when product has no image.
        /// </summary>
        public static string GetProductImageWithPlaceholder(int productId)
        {
            var imagePath = GetProductImage(productId);
            if (string.IsNullOrEmpty(imagePath))
                return "placeholder.png";
            
            return imagePath;
        }
    }
}

