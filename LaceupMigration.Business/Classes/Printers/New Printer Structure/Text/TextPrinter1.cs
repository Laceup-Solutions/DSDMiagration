using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public abstract class TextPrinter : PrinterGeneric
    {
        public override bool ConfigurePrinter()
        {
            try
            {
                PrintIt("! U1 setvar \"device.languages\" \"line_print\"");
                PrintIt("Printer Configured");
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
            return string.Empty;
        }

        public override string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            return string.Empty;
        }

        public override IEnumerable<string> GetUpcForProductInOrder(ref int startY, Order order, Product prod)
        {
            List<string> list = new List<string>();

            if (prod.Upc.Trim().Length > 0 & Config.PrintUPC)
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
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, prod.Upc));
                        startY += font18Separation;
                    }
            }

            return list;
        }

        public override IEnumerable<string> GetUpcForProductIn(ref int startY, Product prod)
        {
            List<string> list = new List<string>();

            if (Config.PrintUPCInventory)
            {
                if (prod.Upc.Trim().Length > 0 & Config.PrintUPC)
                {
                    if (Config.PrintUpcAsText)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesUpcText], startY, prod.Upc));
                        startY += font18Separation;
                    }
                }
            }

            return list;
        }

        public override void AddExtraSpace(ref int startY, List<string> lines, int font, int spaces)
        {
            for (int i = 0; i < spaces; i++)
                lines.Add(string.Empty);
        }

        protected override IEnumerable<string> GetSignatureSection(ref int startY, Order order, bool asPreOrder, List<string> all_lines)
        {
            return GetFooterRows(ref startY, asPreOrder);
        }

        protected override bool PrintLines(List<string> lines)
        {
            if (lines.Any(x => x.Contains((char)241) || x.Contains((char)209)))
                lines = InsertSpecialChar(lines);

            try
            {
                var finalText = new StringBuilder();
                foreach (var line in lines)
                {
                    var l = line.PadLeft(line.Length + 3, ' ');

                    finalText.Append(l);
                    finalText.Append((char)10);
                    finalText.Append((char)13);
                }

                finalText.Append((char)10);
                finalText.Append((char)13);
                finalText.Append((char)10);
                finalText.Append((char)13);

                PrintIt(finalText.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                return false;
            }
        }

        public List<string> InsertSpecialChar(List<string> lines)
        {
            var newLines = new List<string>();
            foreach (var line in lines)
            {
                //will only work if printer's framework is updated :(
                //if (line.Contains((char)241))
                //{
                //    temp_line = "^CI28" + line;
                //    temp_line = temp_line.Replace("^FD", "^FH^FD");
                //    temp_line = temp_line.Replace(((char)241).ToString(), "_c3_b1");
                //}

                // using n/N for now
                var temp_line = line;

                if (line.Contains((char)241))
                    temp_line = temp_line.Replace((char)241, (char)110);

                if (line.Contains((char)209))
                    temp_line = temp_line.Replace((char)209, (char)78);

                newLines.Add(temp_line);
            }
            return newLines;
        }
    }
}