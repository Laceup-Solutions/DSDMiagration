using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class JsSweetWorldPrinter : ZebraFourInchesPrinter1
    {
        protected const string JsSweetWorldOrderDetailsLine = "JsSweetWorldOrderDetailsLine";
        protected const string JsSweetWorldOrderTableTotals = "JsSweetWorldOrderTableTotals";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^CF0,30" +
                "^FO40,{0}^FDCategory^FS" +
                "^CFF,30" +
                "^FO520,{0}^FDQty^FS" +
                "^FO620,{0}^FDAmount^FS";

            linesTemplates.Add(JsSweetWorldOrderDetailsLine, "^CF0,33" +
                "^FO40,{0}^FD{1}^FS" +
                "^CFF,30" +
                "^FO520,{0}^FD{2}^FS" +
                "^FO620,{0}^FD{3}^FS");

            linesTemplates.Add(JsSweetWorldOrderTableTotals, "^CF0,30" +
                "^FO250,{0}^FDTotals:^FS" +
                "^CFF,30" +
                "^FO520,{0}^FD{1}^FS" +
                "^FO620,{0}^FD{2}^FS");
        }

        class T
        {
            public Category Category { get; set; }

            public double Qty { get; set; }

            public double Amount { get; set; }
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            List<T> values = new List<T>();

            double totalQty = 0;
            double totalAmount = 0;

            foreach (var detail in lines)
            {
                if (preOrder && Config.PrintZeroesOnPickSheet)
                    factor = 0;

                double d = 0;
                double qty = 0;

                foreach (var _ in detail.ParticipatingDetails)
                {
                    double _qty = _.Qty;

                    if (_.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            _qty *= _.Product.Weight;
                        else
                            _qty = _.Weight;
                    }

                    d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * _qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                    qty += _qty;
                }

                var t = values.FirstOrDefault(x => x.Category.CategoryId == detail.Product.CategoryId);
                if (t == null)
                {
                    var cat = Category.Find(detail.Product.CategoryId);
                    t = new T() { Category = cat };
                    values.Add(t);
                }

                qty = Math.Round(qty, Config.Round);

                t.Qty += qty;
                t.Amount += d;

                totalQty += qty;
                totalAmount += d;
            }

            int i = 1;

            foreach (var item in values.OrderBy(x => x.Category.Name))
            {
                string name = i + "- " + item.Category.Name;

                var part_name = SplitProductName(name, 31, 31);

                list.Add(string.Format(linesTemplates[JsSweetWorldOrderDetailsLine], startIndex, part_name[0], Math.Round(item.Qty, Config.Round), item.Amount.ToCustomString()));
                startIndex += font36Separation;

                for (int j = 1; j < part_name.Count; j++)
                {
                    list.Add(string.Format(linesTemplates[JsSweetWorldOrderDetailsLine], startIndex, part_name[j], "", ""));
                    startIndex += font36Separation;
                }

                i++;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.Add(string.Format(linesTemplates[JsSweetWorldOrderTableTotals], startIndex, totalQty, totalAmount.ToCustomString()));
            startIndex += font18Separation;

            return list;
        }

    }
}