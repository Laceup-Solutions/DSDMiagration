


using System;
using System.Collections.Generic;
using System.Threading;

using System.Globalization;
using System.Linq;
using System.Text;


namespace LaceupMigration
{
    public class PremierSnacks : ZebraFourInchesPrinter
    {
        protected const string PremierSnacksTableHeader1 = "PremierSnacksTableHeader1";
        protected const string PremierSnacksTableHeader2 = "PremierSnacksTableHeader2";
        protected const string PremierSnacksTableLine = "PremierSnacksTableLine";
        protected const string PremierSnacksTableTotal = "PremierSnacksTableTotal";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(PremierSnacksTableHeader1, "^FO40,{0}^ABN,18,10^FDItem^FS" +
                "^FO140,{0}^ABN,18,10^FDQty^FS" +
                "^FO260,{0}^ABN,18,10^FDDescription^FS" +
                "^FO570,{0}^ABN,18,10^FDNet^FS" +
                "^FO680,{0}^ABN,18,10^FDInvoice^FS");

            linesTemplates.Add(PremierSnacksTableHeader2, "^FO40,{0}^ABN,18,10^FDCode^FS" +
                "^FO140,{0}^ABN,18,10^FDUnit^FS" +
                "^FO260,{0}^ABN,18,10^FDUPC^FS" +
                "^FO570,{0}^ABN,18,10^FDPrice^FS" +
                "^FO680,{0}^ABN,18,10^FDAmount^FS");

            linesTemplates.Add(PremierSnacksTableLine, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO140,{0}^ABN,18,10^FD{2}^FS" +
                "^FO260,{0}^ABN,18,10^FD{3}^FS" +
                "^FO570,{0}^ABN,18,10^FD{4}^FS" +
                "^FO680,{0}^ABN,18,10^FD{5}^FS");

            linesTemplates.Add(PremierSnacksTableTotal, "^FO40,{0}^ABN,18,10^FD{1}^FS" +
                "^FO140,{0}^ABN,18,10^FD{2}^FS" +
                "^FO680,{0}^ABN,18,10^FD{3}^FS");
        }

        protected override IEnumerable<string> GetDetailsRowsInOneDocTableHeader(ref int startY)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableHeader1], startY));
            startY += font18Separation;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableHeader2], startY));
            startY += font18Separation;

            return list;
        }


        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            // Sort the lines
            List<string> list = new List<string>();
            //the header

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderHeaderSectionName], sectionName, startIndex));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                string uomString = null;
                if (detail.OrderDetail.UnitOfMeasure != null)
                {
                    uomString = detail.OrderDetail.UnitOfMeasure.Name;
                    if (!uomMap.ContainsKey(uomString))
                        uomMap.Add(uomString, 0);
                    uomMap[uomString] += detail.Qty;
                }
                else
                {
                    if (!detail.OrderDetail.SkipDetailQty(order))
                    {
                        totalQtyNoUoM += detail.Qty;
                        try
                        {
                            totalUnits += detail.Qty * Convert.ToInt32(detail.OrderDetail.Product.Package);
                        }
                        catch { }
                    }

                }

                int productLineOffset = 0;
                
                var productSlices = SplitProductName(p.Name, 32, 32);

                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Product.SoldByWeight ? _.Weight : _.Qty;

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        double price = detail.Price * factor;

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = price.ToCustomString();
                        string totalAsString = d.ToCustomString();

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;

                        string code = string.Empty;
                        if (!string.IsNullOrEmpty(p.Code))
                            code = p.Code;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableLine], startIndex, 
                            code, qtyAsString, pName, priceAsString, totalAsString));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableLine], startIndex, 
                            "", "", pName, "", ""));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                string upc = string.Empty;
                if (!string.IsNullOrEmpty(p.Upc))
                    upc = p.Upc;

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableLine], startIndex,
                    "", "", upc, "", ""));
                startIndex += font18Separation;

                foreach (var item in detail.ParticipatingDetails)
                {
                    if (!string.IsNullOrEmpty(item.Lot))
                        if (preOrder)
                        {
                            if (Config.PrintLotPreOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex,
                                    "Lot: " + item.Lot + "  Qty: " + item.Qty));
                                startIndex += font18Separation;
                            }
                        }
                        else
                        {
                            if (Config.PrintLotOrder)
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotal14], startIndex,
                                    "Lot: " + item.Lot + "  Qty: " + item.Qty));
                                startIndex += font18Separation;
                            }
                        }
                }

                // anderson crap
                // the retail price
                var extraProperties = order.Client.ExtraProperties;
                if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                {
                    var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                    if (retailPrice != null)
                    {
                        string retPriceString = "Retail price                                   " + Convert.ToDouble(retailPrice.Item2).ToCustomString();
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, retPriceString));
                        startIndex += font18Separation;
                    }
                }

                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in PrintOrdersCreatedReportWithDetailsSplitProductName3(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineLot], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;
            }

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (uomMap.Count > 0)
            {
                int offSet = 0;
                foreach (var key in uomMap.Keys)
                {
                    if (offSet == 0)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableTotal], startIndex,
                            "Totals:", uomMap[key] + " " + key, balance.ToCustomString()));
                        startIndex += font18Separation;
                    }
                    else
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableTotal], startIndex,
                            "", uomMap[key] + " " + key, ""));
                        startIndex += font18Separation;
                    }
                    offSet++;
                }
            }
            else
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PremierSnacksTableTotal], startIndex,
                            "Totals:", totalUnits, balance.ToCustomString()));
                startIndex += font18Separation;
            }

            return list;
        }

    }
}