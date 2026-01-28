
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkiaSharp;

namespace LaceupMigration
{
    public abstract class ZebraPrinter1 : PrinterGeneric
    {
        public override bool ConfigurePrinter()
        {
            try
            {
                PrintIt("! U1 setvar \"device.languages\" \"zpl\"");
                PrintIt("^XA^PON^MNN^LL100^FO40,40^ADN,36,20^FDPrinter Configured^FS^XZ");
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public override string GetLogoLabel(ref int startY, Order order)
        {
            if (order != null && order.CompanyId > 0)
            {
                var company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company != null && !string.IsNullOrEmpty(company.CompanyLogo))
                {

                    string label = "^FO30," + startY.ToString() + "^GFA, " +
                                  company.CompanyLogoSize.ToString() + "," +
                                  company.CompanyLogoSize.ToString() + "," +
                                  company.CompanyLogoWidth.ToString() + "," +
                                  company.CompanyLogo;

                    startY += company.CompanyLogoHeight;

                    return label;
                }
            }

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;

                startY += Config.CompanyLogoHeight;

                return label;
            }

            return string.Empty;
        }

        public override string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            string signatureAsString;
            signatureAsString = order.ConvertSignatureToBitmap();
            using SKBitmap signature = SKBitmap.Decode(signatureAsString);

            var converter = new BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature);
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width + 31) / 32) * 32 / 8;
            int height = signature.Height / 32 * 32;
            var bitmapDataLength = rawBytes.Length;

            string ZPLImageDataString = BitConverter.ToString(rawBytes);
            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

            string label = "^FO30," + startIndex.ToString() + "^GFA, " +
                           bitmapDataLength.ToString() + "," +
                           bitmapDataLength.ToString() + "," +
                           widthInBytes.ToString() + "," +
                           ZPLImageDataString;
            startIndex += height;
            return label;
        }

        public override IEnumerable<string> GetUpcForProductInOrder(ref int startY, Order order, Product prod)
        {
            List<string> list = new List<string>();

            var upc = prod.Upc.Trim();

            if (order != null && order.Client != null && order.Client.PrintSkuOverUpc && !string.IsNullOrEmpty(prod.Sku.Trim()))
                upc = prod.Sku.Trim();

            if (upc.Length > 0 & Config.PrintUPC)
            {
                bool printUpc = true;
                if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString))
                {
                    var item = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "showupc");
                    if (item != null && item.Item2 == "0")
                        printUpc = false;
                }
                if (printUpc)
                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, upc));
                        startY += font18Separation;
                    }
                    else
                    {
                        startY += font18Separation / 2;

                        var upcTemp = Config.UseUpc128 ? linesTemplates[Upc128] : linesTemplates[OrderDetailsLinesUpcBarcode];

                        if(upc.Length > 12 && !Config.UseUpc128)
                            upcTemp = linesTemplates[OrderDetailsLinesLongUpcBarcode];

                        list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, Product.GetFirstUpcOnly(upc)));
                        startY += font36Separation * 2;
                    }
            }

            return list;
        }

        public override IEnumerable<string> GetUpcForProductIn(ref int startY, Product prod)
        {
            List<string> list = new List<string>();

            if (Config.PrintUPCInventory || Config.PrintUPCOpenInvoices)
            {
                if (prod.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, prod.Upc));
                        startY += font18Separation;
                    }
                    else
                    {
                        var upcTemp = Config.UseUpc128 ? linesTemplates[Upc128] : linesTemplates[OrderDetailsLinesUpcBarcode];

                        if (prod.Upc.Length > 12 && !Config.UseUpc128)
                            upcTemp = linesTemplates[OrderDetailsLinesLongUpcBarcode];

                        list.Add(string.Format(CultureInfo.InvariantCulture, upcTemp, startY, Product.GetFirstUpcOnly(prod.Upc)));
                        startY += font36Separation;
                        if (Config.PrintUPCOpenInvoices || Config.PrintUPCInventory)
                            startY += font18Separation;
                    }
                }
            }

            return list;
        }

        public override void AddExtraSpace(ref int startY, List<string> lines, int font, int spaces)
        {
            for (int i = 0; i < spaces; i++)
                startY += font;
        }

        protected override bool PrintLines(List<string> lines)
        {
            lines = InsertSpecialChar(lines);

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                if (!string.IsNullOrEmpty(s))
                    sb.Append(s);

            try
            {
                DateTime st = DateTime.Now;
                string s = sb.ToString();
                PrintIt(s);
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public static string ZPLFromLines(List<string> lines) 
        {
            lines = InsertSpecialChar(lines);

            StringBuilder sb = new StringBuilder();
            foreach (string s in lines)
                if (!string.IsNullOrEmpty(s))
                    sb.Append(s);

            return sb.ToString();
        } 

        public static List<string> InsertSpecialChar(List<string> lines)
        {
            try
            {
                var newLines = new List<string>();
                foreach (var line in lines)
                {
                    var temp_line = line;
                    foreach (var c in SpecialCharacters.Keys)
                    {
                        if (line.Contains(c))
                        {
                            temp_line = temp_line.Replace(c.ToString(), SpecialCharacters[c]);
                        }
                    }

                    newLines.Add(temp_line);
                }

                return newLines;
            }
            catch(Exception ex)
            {
                return lines;
            }
        }

        public static Dictionary<char, string> SpecialCharacters = new Dictionary<char, string>() {
            {(char)241, "n" },//ñ
            {(char)209, "N" },//ñ
            {(char)233, "e" },//é
            {(char)201, "E" },//É
            {(char)224, "a" },//à
            {(char)192, "A" },//À
            {(char)200, "E" },//È
            {(char)232, "e" },//è
            {(char)217, "U" },//Ù
            {(char)249, "u" },//ù
            {(char)194, "A" },//Â
            {(char)226, "a" },//â
            {(char)234, "e" },//ê
            {(char)202, "E" },//Ê
            {(char)206, "I" },//Î
            {(char)238, "i" },//î
            {(char)244, "o" },//ô
            {(char)212, "O" },//Ô
            {(char)251, "u" },//û
            {(char)219, "U" },//Û
            {(char)228, "a" },//ä
            {(char)196, "A" },//Ä
            {(char)203, "E" },//Ë
            {(char)235, "e" },//ë
            {(char)220, "U" },//Ü
            {(char)252, "u" },//ü
            {(char)199, "C" },//Ç
            {(char)231, "c" },//ç
            {(char)239, "i" },//ï
            {(char)207, "I" },//Ï
        };

        #region WELL DONE but printers framework's needs to be updated
        public Dictionary<char, string> SpecialCharacters_ = new Dictionary<char, string>() {
            {(char)241, "_c3_b1" },//ñ
            {(char)209, "_c3_91" },//ñ
            {(char)233, "_c3_a9" },//é
            {(char)201, "_c3_89" },//É
            {(char)224, "_c3_a0" },//à
            {(char)192, "_c3_80" },//À
            {(char)200, "_c3_88" },//È
            {(char)232, "_c3_a8" },//è
            {(char)217, "_c3_99" },//Ù
            {(char)249, "_c3_b9" },//ù
            {(char)194, "_c3_a2" },//Â
            {(char)226, "_c3_a2" },//â
            {(char)234, "_c3_aa" },//ê
            {(char)202, "_c3_8a" },//Ê
            {(char)206, "_c3_8e" },//Î
            {(char)238, "_c3_ae" },//î
            {(char)244, "_c3_b4" },//ô
            {(char)212, "_c3_94" },//Ô
            {(char)251, "_c3_bb" },//û
            {(char)219, "_c3_9b" },//Û
            {(char)228, "_c3_a4" },//ä
            {(char)196, "_c3_84" },//Ä
            {(char)203, "_c3_8b" },//Ë
            {(char)235, "_c3_ab" },//ë
            {(char)220, "_c3_9c" },//Ü
            {(char)252, "_c3_bc" },//ü
            {(char)199, "_c3_87" },//Ç
            {(char)231, "_c3_a7" },//ç
            {(char)239, "_c3_8f" },//ï
            {(char)207, "_c3_af" },//Ï
        };
        public List<string> InsertSpecialChar__(List<string> lines)
        {
            //will only work if printer's framework is updated :(
            var newLines = new List<string>();
            bool containedSpecial = false;
            foreach (var line in lines)
            {
                var temp_line = line;
                bool AlreadyIndexed = false;

                foreach (var c in SpecialCharacters.Keys)
                {
                    if (line.Contains(c))
                    {
                        if (!AlreadyIndexed)
                        {
                            temp_line = "^CI28" + line;
                            temp_line = temp_line.Replace("^FD", "^FH^FD");
                            AlreadyIndexed = true;
                        }
                        temp_line = temp_line.Replace(c.ToString(), SpecialCharacters[c]);

                        containedSpecial = true;
                    }
                }

                newLines.Add(temp_line);
            }

            return newLines;
            //foreach (var line in lines)
            //{
            //    var temp_line = line;
            //    //if (line.Contains((char)241))
            //    //{
            //    //    temp_line = "^CI28" + line;
            //    //    temp_line = temp_line.Replace("^FD", "^FH^FD");
            //    //    temp_line = temp_line.Replace(((char)241).ToString(), "_c3_b1");
            //    //}
            //    if (line.Contains((char)241))
            //        temp_line = temp_line.Replace((char)241, (char)110);

            //    if (line.Contains((char)209))
            //        temp_line = temp_line.Replace((char)209, (char)78);

            //    newLines.Add(temp_line);
            //}
            //return newLines;
        }
        #endregion
    }
}