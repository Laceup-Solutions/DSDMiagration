using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;




namespace LaceupMigration
{
    public class DefaultXlsxProvider : IPdfProvider.IXlsxProvider
    {
        public enum TextStyle
        {
            Normal = 1,
            Bold = 2,
            Italic = 3,
            BoldItalic = 4
        }

        public enum TextAlignment
        {
            General = 0,
            Left = 1,
            Center = 2,
            CenterContinuous = 3,
            Right = 4,
            Fill = 5,
            Distributed = 6,
            Justify = 7
        }

        public virtual string GetOrderXlsx(Order order)
        {
            string downloadsPath = Config.BasePath;
            string name = string.Format("order {0}.xlsx", order.PrintedOrderId);
            string filePath = name;
            string fullPath = Path.Combine(downloadsPath, filePath);

            var list = GetExcelValues(order);

            var tempFile = Path.GetTempFileName();

            using (StreamWriter writer = new StreamWriter(tempFile))
            {
                foreach (var item in list)
                    writer.WriteLine(item.ToString());
            }

            try
            {
                DataProvider.GetExcelFile(tempFile, fullPath);
            }
            catch(Exception ex)
            {
                Logger.CreateLog(ex);
            }

            File.Delete(tempFile);

            return fullPath;
        }

        protected virtual List<T> GetExcelValues(Order order)
        {
            return new List<T>();
        }

        protected class T
        {
            public string Cell { get; set; }
            public string Value { get; set; }
            public int FontSize { get; set; }
            public TextStyle FontStyle { get; set; }
            public TextAlignment Alignment { get; set; }
            public string Merge { get; set; }
            public string Format { get; set; }

            public T(string cell, string value, int fontSize = 10, TextStyle style = TextStyle.Normal, TextAlignment alignment = TextAlignment.Left, string merge = "", string format = "")
            {
                Cell = cell;
                Value = value;
                FontSize = fontSize;
                FontStyle = style;
                Alignment = alignment;
                Merge = merge;
                Format = format;
            }

            public T(string cell, string value, TextStyle style, TextAlignment alignment = TextAlignment.Left, string merge = "", string format = "")
            {
                Cell = cell;
                Value = value;
                FontSize = 10;
                FontStyle = style;
                Alignment = alignment;
                Merge = merge;
                Format = format;
            }

            public override string ToString()
            {
                string result = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", (char)20,
                              Cell,                 //0
                              Value,                //1
                              FontSize,             //2
                              (int)FontStyle,       //3
                              (int)Alignment,       //4
                              Merge,                //5
                              Format                //6
                              );

                return result;
            }
        }

    }
}