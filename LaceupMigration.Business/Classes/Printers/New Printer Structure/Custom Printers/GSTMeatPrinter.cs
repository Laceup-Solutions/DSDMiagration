





using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;



namespace LaceupMigration
{
    public class GSTMeatPrinter : ZebraFourInchesPrinter1
    {
        protected  const string OrderDetailsLinesWeight = "OrderDetailsLinesWeight";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
         "^FO270,{0}^ADN,18,10^FDORDERED^FS" +
         "^FO380,{0}^ADN,18,10^FDSHIPPED^FS" +
         "^FO485,{0}^ADN,18,10^FD$PER LB^FS" +
         "^FO590,{0}^ADN,18,10^FDWEIGHT^FS" +
         "^FO680,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
               "^FO310,{0}^ADN,18,10^FD{2}^FS" +
               "^FO420,{0}^ADN,18,10^FD{3}^FS" +
               "^FO500,{0}^ADN,18,10^FD{4}^FS" +
               "^FO600,{0}^ADN,18,10^FD{5}^FS" +
               "^FO680,{0}^ADN,18,10^FD{6}^FS";

            linesTemplates[OrderDetailsLinesWeight] = "^CF0,25^FO40,{0}^FD{1}^FS"; //set bold the weiths on random weight line
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }
        protected string GetSectionRowsInOneDocFixedLine(string format, int pos, string v1, string v2, string v4, string v3, string v5, string v6)
        {
            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos, v1, v2, v4, v3, v5, v6);
        }

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            string formatString = linesTemplates[OrderDetailsHeader];

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return lines;
        }
        protected virtual List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance , double totalWeight, double TotalShipped)
        {
            List<string> list = new List<string>();

            var printString = string.Empty;
            var printString1 = string.Empty;
            startIndex += font18Separation;

            if (sectionName == "DUMP SECTION")
                printString = "Credit Units: " + totalQtyNoUoM + " EA";
            if (sectionName == "SALES SECTION")
            {
                printString = "Total Weight: " + totalWeight;
                printString1 = "Total Shipped: " + TotalShipped;
            }
                
            if (sectionName == "RETURNS SECTION")
                printString = "Return Units: " + totalQtyNoUoM + " EA";

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotals], startIndex, printString,printString1,"",""));
            startIndex += font18Separation;

            return list;
        }
        private double CalculateDiscount(OrderDetail detail)
        {
            double discount;

            if (detail.Damaged)
            {
                int package;
                if (int.TryParse(detail.Product.Package, out package))
                {
                    // if is dump
                    discount = (detail.Product.PriceLevel0 / package) - detail.Price;
                }
                else
                {
                    // Handle invalid package value
                    discount = 0;
                }

            }
            if (detail.IsCredit)
            {
                discount = 0;
            }
            else
            {
                // calculate discount as the difference between the original price and the current price, multiplied by the number of units
                int package = 1;
                int.TryParse(detail.Product.Package, out package);
                double units = detail.Qty * package;
                double qty = detail.Qty;
                discount = (detail.Product.PriceLevel0 - detail.Price) * qty;
            }
            if (discount < 0)
            {
                discount = 0;
            }
            return discount;
        }

        public class Line
        {
            public int ProductId { get; set; }

            public double Weight { get; set; }

            public double Price { get; set; }

            public double OrderedQty { get; set; }

            public double ShippedQty { get; set; }
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            try
            {

            
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            double totalWeight = 0;
            float totalUnits = 0;
            double balance = 0;
            double TotalShipped = 0; //Final qty given to the client 

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            var deletedDetailsWithNoLine = order.DeletedDetails.Where(x => !lines.Any(y => y.Product.ProductId == x.Product.ProductId));
            deletedDetailsWithNoLine = deletedDetailsWithNoLine.Where(x => x.Product.SoldByWeight && x.Product.Weight == 0).ToList();

            Dictionary<int, List<OrderDetail>> groupedDeleted = new Dictionary<int, List<OrderDetail>>();
            foreach(var d in deletedDetailsWithNoLine)
                {
                    if (groupedDeleted.ContainsKey(d.Product.ProductId))
                        groupedDeleted[d.Product.ProductId].Add(d);
                    else
                        groupedDeleted.Add(d.Product.ProductId, new List<OrderDetail>() { d });
                }

            foreach (var detail in lines)
            {
                if (detail.Qty == 0)
                    continue;

                Product p = detail.Product;

                string uomString = null;
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null)
                    {
                        uomString = detail.OrderDetail.UnitOfMeasure.Name;
                        if (!uomMap.ContainsKey(uomString))
                            uomMap.Add(uomString, 0);
                        uomMap[uomString] += detail.Qty;

                        totalQtyNoUoM += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;
                        //totalWeight += detail.Product.Weight;

                        var packaging_str = detail.Product.Package;
                        int package = 0;
                        Int32.TryParse(packaging_str, out package);



                    }
                    else
                    {
                        if (!detail.OrderDetail.SkipDetailQty(order))
                        {
                            int packaging = 0;

                            if (!string.IsNullOrEmpty(detail.OrderDetail.Product.Package))
                                int.TryParse(detail.OrderDetail.Product.Package, out packaging);


                            totalQtyNoUoM += detail.Qty;
                            //totalWeight += detail.Product.Weight;

                                if (packaging > 0)
                                totalUnits += detail.Qty * packaging;

                        }
                    }
                }

                

                int productLineOffset = 0;
                var name = p.Name;
                if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                    name = p.Name + " " + p.Upc;
                var productSlices = GetOrderDetailsRowsSplitProductName(name);
                    List<string> weightsList = new List<string>();
                    string weightString = string.Empty;
                    if (p.SoldByWeight && p.Weight == 0) //if (p.SoldByWeight) *all products are SoldByWeight && Weight > 0  == FIXED WEIGHT (don't print weight lines) Weight == 0 Random Weight *
                    {
                        foreach (var f in detail.ParticipatingDetails)
                        {
                            string formattedWeight = DoFormat(f.Weight);
                            if (formattedWeight != "0" && formattedWeight != "0.0" && formattedWeight != "0.00")
                            {
                                weightsList.Add(DoFormat(f.Weight));
                            }

                        }

                    }

                    if (weightsList.Count > 0)
                        weightString = string.Join(" ", weightsList); //weightString = "(" + string.Join(" , ", weightsList) + ")";


                    startIndex += font18Separation;
                    foreach (string pName in productSlices)
                {
                        //name = pName + weightsList;
                        if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Qty;

                            if (_.Product.SoldByWeight && _.Product.Weight == 0) //_.Product.SoldByWeight)
                            {
                                if (order.AsPresale)
                                    qty *= _.Product.Weight;
                                else
                                    qty = _.Weight;

                                    d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                                }
                            else
                            {
                                    d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * _.Product.Weight), Config.Round).ToCustomString(), NumberStyles.Currency); 
                            }

                            //d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * _.qty), Config.Round).ToCustomString(), NumberStyles.Currency); 
                            }

                        double price = detail.Price * factor;

                        balance += d;

                        string qtyAsString = Math.Round(detail.Qty, 2).ToString(CultureInfo.CurrentCulture);
                        if (detail.OrderDetail.UnitOfMeasure != null)
                            qtyAsString += " " + detail.OrderDetail.UnitOfMeasure.Name;
                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        if (detail.Product.ProductType == ProductType.Discount)
                            qtyAsString = string.Empty;

                        var packaging_str = detail.Product.Package;
                        int package = 1;
                        Int32.TryParse(packaging_str, out package);



                        var units = package; 
                         
                        var unitPrice = detail.Price;

                       var hasDelete = detail.ParticipatingDetails.Any(x => x.Deleted);


                        //order.deleteDetails
                        var groupedDetails = new List<Line>();
                        foreach(var f in detail.ParticipatingDetails)
                            {
                                unitPrice = f.Product.SoldByWeight && f.Product.Weight == 0 ? detail.Price : f.Product.PriceLevel0;// if the product is fixed weight the unitPrice should be the UoM each //before f.Product.SoldByWeight
                                if (f.Product.SoldByWeight && f.Product.Weight == 0) 
                                {
                                    
                                    var alreadyFound = groupedDetails.FirstOrDefault(x => x.ProductId == f.Product.ProductId);
                                    if(alreadyFound != null)
                                    {
                                        alreadyFound.OrderedQty += order.IsDelivery ? f.Ordered : f.Qty; //alreadyFound.OrderedQty += f.Ordered;
                                        alreadyFound.ShippedQty += f.Qty;
                                        alreadyFound.Weight += f.Weight;
                                    }
                                    else
                                    {
                                        groupedDetails.Add(new Line()
                                        {
                                            //Price = f.Product.PriceLevel0,
                                            ProductId = f.Product.ProductId,
                                            OrderedQty = order.IsDelivery ? f.Ordered : f.Qty, //OrderedQty = f.Ordered,
                                            ShippedQty = f.Qty,
                                            Price = detail.Price,
                                            Weight = f.Weight 
                                        });
                                    }
                                }
                                else
                                {
                                    groupedDetails.Add(new Line() { ProductId = f.Product.ProductId, //OrderedQty = f.Ordered //Weight = f.Product.Weight (if fixed weight)
                                        OrderedQty = order.IsDelivery ? f.Ordered : f.Qty, ShippedQty = f.Qty, Price = f.Product.PriceLevel0, Weight = f.Product.Weight * f.Qty});
                                }


                            }

                            if (detail.Product.SoldByWeight && detail.Product.Weight == 0)
                            {
                                foreach (var deleted in order.DeletedDetails.Where(x => x.Product.ProductId == detail.Product.ProductId))
                                {
                                    var alreadyFound = groupedDetails.FirstOrDefault(x => x.ProductId == deleted.Product.ProductId);
                                    if (alreadyFound != null)
                                    {
                                        alreadyFound.OrderedQty += deleted.Ordered;
                                    }
                                    else
                                    {
                                        groupedDetails.Add(new Line()
                                        {
                                            //Price = f.Product.PriceLevel0,
                                            ProductId = deleted.Product.ProductId,
                                            OrderedQty = deleted.Ordered, //OrderedQty = f.Ordered,
                                            ShippedQty = 0,
                                            Price = detail.Price,
                                            Weight = deleted.Weight
                                        });
                                    }

                                }
                            }

                        double weight = 0;
                        double ordered = 0;
                        double weightPerLine = 0;
                        double shipped = 0;
                        
                        weightPerLine = groupedDetails.Sum(x => x.Weight) ; //weightPerLine = groupedDetails.Sum(x => x.Weight);
                        weightPerLine = double.Parse(Math.Round(Convert.ToDecimal(weightPerLine), Config.Round).ToString());
                        ordered = groupedDetails.Sum(x => x.OrderedQty);
                        shipped = groupedDetails.Sum(x => x.ShippedQty);

                        

                      TotalShipped += shipped;
                      totalWeight += weightPerLine;


                            // Ordered = Qty(detail.qty) | Shipped = OrderDetail.Qty |$Per LB = unitPrice(deyail.price)| Weight =  | Total = totalAsString //startIndex, pName, ...
                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, ordered.ToString(), shipped.ToString(), unitPrice.ToCustomString(), weightPerLine.ToString(), totalAsString));
                        startIndex += font18Separation;

                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                    //break line aftes 5 items on weightList 
                    int maxItemsPerLine = 5;
                    int weightsListCount = weightsList.Count;
                    for (int i = 0; i < weightsListCount; i += maxItemsPerLine)
                    {
                        int endIndex = Math.Min(i + maxItemsPerLine, weightsListCount);
                        string chunk = string.Join(" ", weightsList.Skip(i).Take(endIndex - i));
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesWeight], startIndex, chunk));
                        startIndex += font18Separation;
                    }
                    //list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesWeight], startIndex , weightString));
                    //startIndex += font18Separation;


                    string weights = "";

                if (Config.PrintLotPreOrder || Config.PrintLotOrder)
                {
                    foreach (var item in detail.ParticipatingDetails)
                    {
                        var itemLot = item.Lot ?? "";
                        if (!string.IsNullOrEmpty(itemLot) && item.LotExpiration != DateTime.MinValue)
                            itemLot += "  Exp: " + item.LotExpiration.ToShortDateString();

                        string qty = item.Qty.ToString();
                        if (item.Product.SoldByWeight && item.Product.Weight == 0 && !order.AsPresale)
                            qty = item.Weight.ToString();

                        if (!string.IsNullOrEmpty(itemLot))
                        {
                            if (preOrder)
                            {
                                if (Config.PrintLotPreOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                            else
                            {
                                if (Config.PrintLotOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        itemLot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                        }
                        else
                        {
                            if (item.Product.SoldByWeight && item.Product.Weight == 0 && !order.AsPresale)
                            {
                                var temp_weight = DoFormat(item.Weight);
                                weights += temp_weight + " ";
                            }
                        }
                    }
                }
                else if (!order.AsPresale)
                {
                    /* foreach (var detail1 in detail.ParticipatingDetails)
                     {
                         if (!detail1.Product.SoldByWeight)
                             continue;

                         if (!string.IsNullOrEmpty(weights))
                             weights += ",";
                         weights += detail1.Weight;
                     }*/

                    StringBuilder sb = new StringBuilder();
                    List<string> lotUsed = new List<string>();

                    int TotalCases = 0;

            
                    foreach (var detail1 in detail.ParticipatingDetails)
                    {
                        if (!string.IsNullOrEmpty(detail1.Lot) && detail1.Product.SoldByWeight && detail.Product.Weight == 0)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            else
                                sb.Append("Weight: ");

                            if (detail1.Weight != 0)
                                sb.Append(detail1.Weight.ToString());


                            lotUsed.Add(detail1.Lot);
                            TotalCases++;
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                        startIndex += font18Separation;
                    }

                    if (!string.IsNullOrEmpty(weights))
                    {
                        foreach (var item in GetOrderDetailsRowsSplitProductName(weights))
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, item));
                            startIndex += font18Separation;
                        }
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeightsCount], startIndex, detail.ParticipatingDetails.Count));
                        startIndex += font18Separation;
                    }

                    // anderson crap
                    // the retail price
                    var extraProperties = order.Client.ExtraProperties;
                    if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                    {
                        var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                        if (retailPrice != null)
                        {
                            string retPriceString = "                                  " + ToString(Convert.ToDouble(retailPrice.Item2));
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesRetailPrice], startIndex, retPriceString));
                            startIndex += font18Separation;
                        }
                    }

                    list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                }
                if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.OrderDetail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetOrderDetailsSplitComment(detail.OrderDetail.Comments))
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                        startIndex += font18Separation;
                    }
                }

                startIndex += 10;
            }

                #region DisplayDeletedLines
                foreach (var detail in groupedDeleted)
                {
                    Product p = detail.Value.FirstOrDefault().Product;

                    string uomString = null;
                    if (detail.Value.FirstOrDefault().Product.ProductType != ProductType.Discount)
                    {
                        if (detail.Value.FirstOrDefault().UnitOfMeasure != null)
                        {
                            uomString = detail.Value.FirstOrDefault().UnitOfMeasure.Name;
                            if (!uomMap.ContainsKey(uomString))
                                uomMap.Add(uomString, 0);
                            uomMap[uomString] += detail.Value.FirstOrDefault().Qty;

                            totalQtyNoUoM += detail.Value.FirstOrDefault().Qty * detail.Value.FirstOrDefault().UnitOfMeasure.Conversion;
                            //totalWeight += detail.Product.Weight

                            var packaging_str = detail.Value.FirstOrDefault().Product.Package;
                            int package = 0;
                            Int32.TryParse(packaging_str, out package);



                        }
                        else
                        {
                            if (!detail.Value.FirstOrDefault().SkipDetailQty(order))
                            {
                                int packaging = 0;

                                if (!string.IsNullOrEmpty(detail.Value.FirstOrDefault().Product.Package))
                                    int.TryParse(detail.Value.FirstOrDefault().Product.Package, out packaging);


                                totalQtyNoUoM += detail.Value.FirstOrDefault().Qty;
                                //totalWeight += detail.Product.Weight;

                                if (packaging > 0)
                                    totalUnits += detail.Value.FirstOrDefault().Qty * packaging;

                            }
                        }
                    }



                    int productLineOffset = 0;
                    var name = p.Name;
                    if (Config.ConcatUPCToName && !string.IsNullOrEmpty(p.Upc))
                        name = p.Name + " " + p.Upc;
                    var productSlices = GetOrderDetailsRowsSplitProductName(name);
                    List<string> weightsList = new List<string>();
                    if (p.SoldByWeight && p.Weight == 0)
                    {
                        foreach (var f in detail.Value)
                        {
                            weightsList.Add(f.Weight.ToString());
                        }
                    }
                    string weightString = string.Empty;
                    if (weightsList.Count > 0)
                        weightString = " (" + string.Join(" , ", weightsList) + ")";

                    foreach (string pName in productSlices)
                    {
                        name = pName + weightString;
                        if (productLineOffset == 0)
                        {
                            if (preOrder && Config.PrintZeroesOnPickSheet)
                                factor = 0;

                            double d = 0;
                            foreach (var _ in detail.Value)
                            {
                                double qty = _.Qty;

                                if (_.Product.SoldByWeight && _.Product.Weight == 0)
                                {
                                    if (order.AsPresale)
                                        qty *= _.Product.Weight;
                                    else
                                        qty = _.Weight;
                                }

                                d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                            }

                            double price = detail.Value.FirstOrDefault().Price * factor;

                            balance += d;

                            string qtyAsString = Math.Round(detail.Value.FirstOrDefault().Qty, 2).ToString(CultureInfo.CurrentCulture);
                            if (detail.Value.FirstOrDefault().UnitOfMeasure != null)
                                qtyAsString += " " + detail.Value.FirstOrDefault().UnitOfMeasure.Name;
                            string priceAsString = ToString(price);
                            string totalAsString = ToString(d);

                            if (Config.HidePriceInPrintedLine)
                                priceAsString = string.Empty;
                            if (Config.HideTotalInPrintedLine)
                                totalAsString = string.Empty;
                            if (detail.Value.FirstOrDefault().Product.ProductType == ProductType.Discount)
                                qtyAsString = string.Empty;

                            var packaging_str = detail.Value.FirstOrDefault().Product.Package;
                            int package = 1;
                            Int32.TryParse(packaging_str, out package);



                            var units = package;

                            var unitPrice = detail.Value.FirstOrDefault().Price;

                            var hasDelete = detail.Value.Any(x => x.Deleted);

                            var groupedDetails = new List<Line>();
                            //order.deleteDetails
                            /* var groupedDetails = new List<Line>();
                             foreach (var f in detail.Value)
                             {
                                 unitPrice = f.Product.SoldByWeight ? detail.Value.FirstOrDefault().Price : f.Product.PriceLevel0;// if the product is fixed weight the unitPrice should be the UoM each 
                                 if (f.Product.SoldByWeight)
                                 {

                                     var alreadyFound = groupedDetails.FirstOrDefault(x => x.ProductId == f.Product.ProductId);
                                     if (alreadyFound != null)
                                     {
                                         alreadyFound.OrderedQty += order.IsDelivery ? f.Ordered : f.Qty; //alreadyFound.OrderedQty += f.Ordered;
                                         alreadyFound.ShippedQty += f.Qty;
                                         alreadyFound.Weight += f.Weight;
                                     }
                                     else
                                     {
                                         groupedDetails.Add(new Line()
                                         {
                                             //Price = f.Product.PriceLevel0,
                                             ProductId = f.Product.ProductId,
                                             OrderedQty = order.IsDelivery ? f.Ordered : f.Qty, //OrderedQty = f.Ordered,
                                             ShippedQty = f.Qty,
                                             Price = detail.Value.FirstOrDefault().Price,
                                             Weight = f.Weight
                                         });
                                     }
                                 }
                                 else
                                 {
                                     groupedDetails.Add(new Line()
                                     {
                                         ProductId = f.Product.ProductId, //OrderedQty = f.Ordered
                                         OrderedQty = order.IsDelivery ? f.Ordered : f.Qty,
                                         ShippedQty = f.Qty,
                                         Price = f.Product.PriceLevel0,
                                         Weight = f.Product.Weight
                                     });
                                 }


                             }*/

                            if (detail.Value.FirstOrDefault().Product.Weight == 0) //detail.Value.FirstOrDefault().Product.SoldByWeight)
                            {
                                foreach (var deleted in order.DeletedDetails.Where(x => x.Product.ProductId == detail.Value.FirstOrDefault().Product.ProductId))
                                {
                                    var alreadyFound = groupedDetails.FirstOrDefault(x => x.ProductId == deleted.Product.ProductId);
                                    if (alreadyFound != null)
                                    {
                                        alreadyFound.OrderedQty += deleted.Ordered;
                                    }
                                    else
                                    {
                                        groupedDetails.Add(new Line()
                                        {
                                            //Price = f.Product.PriceLevel0,
                                            ProductId = deleted.Product.ProductId,
                                            OrderedQty = deleted.Ordered, //OrderedQty = f.Ordered,
                                            ShippedQty = 0,
                                            Price = detail.Value.FirstOrDefault().Price,
                                            Weight = deleted.Weight
                                        });
                                    }

                                }
                            }

                            double weight = 0;
                            double ordered = 0;
                            double weightPerLine = 0;
                            double shipped = 0;

                            weightPerLine = 0; //groupedDetails.Sum(x => x.Weight);
                            ordered = groupedDetails.Sum(x => x.OrderedQty);
                            shipped = groupedDetails.Sum(x => x.ShippedQty);

                            /*foreach (var _ in detail.ParticipatingDetails)
                            {
                                //weight += _.Weight;
                                //shipped = detail.OrderDetail.Qty;
                                if (_.Product.SoldByWeight)
                                {
                                        weight += _.Weight;
                                        //weightPerLine += weight;
                                        weightPerLine += _.Weight;
                                        ordered = detail.ParticipatingDetails.Count();
                                        //shipped = detail.OrderDetail.Qty;
                                        shipped = groupedDetails;




                                    }
                                else
                                {
                                        int.TryParse(detail.Product.Package, out int packageWeight);
                                        weight = packageWeight != 0 ? packageWeight : detail.Product.Weight;
                                        //weight = detail.Product.Weight;
                                        //weightPerLine += weight;
                                        weightPerLine += detail.Product.Weight;
                                        ordered = detail.Qty;
                                        shipped = detail.OrderDetail.Qty;
                                        unitPrice = detail.Product.PriceLevel0;


                                }

                            }*/

                            TotalShipped += shipped;
                            totalWeight += weightPerLine;


                            // Ordered = Qty(detail.qty) | Shipped = OrderDetail.Qty |$Per LB = unitPrice(deyail.price)| Weight =  | Total = totalAsString //startIndex, pName, ...
                            list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, ordered.ToString(), shipped.ToString(), unitPrice.ToCustomString(), 0.ToString(), 0.ToCustomString()));
                            startIndex += font18Separation;

                        }
                        else if (!Config.PrintTruncateNames)
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, pName));
                            startIndex += font18Separation;
                        }
                        else
                            break;
                        productLineOffset++;
                    }

                    string weights = "";

                    if (Config.PrintLotPreOrder || Config.PrintLotOrder)
                    {
                        foreach (var item in detail.Value)
                        {
                            var itemLot = item.Lot ?? "";
                            if (!string.IsNullOrEmpty(itemLot) && item.LotExpiration != DateTime.MinValue)
                                itemLot += "  Exp: " + item.LotExpiration.ToShortDateString();

                            string qty = item.Qty.ToString();
                            if (item.Product.SoldByWeight && item.Product.Weight == 0 && !order.AsPresale)
                                qty = item.Weight.ToString();

                            if (!string.IsNullOrEmpty(itemLot))
                            {
                                if (preOrder)
                                {
                                    if (Config.PrintLotPreOrder)
                                    {
                                        list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                            itemLot, qty));
                                        startIndex += font18Separation;
                                    }
                                }
                                else
                                {
                                    if (Config.PrintLotOrder)
                                    {
                                        list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                            itemLot, qty));
                                        startIndex += font18Separation;
                                    }
                                }
                            }
                            else
                            {
                                if (item.Product.SoldByWeight && item.Product.Weight == 0 && !order.AsPresale)
                                {
                                    var temp_weight = DoFormat(item.Weight);
                                    weights += temp_weight + " ";
                                }
                            }
                        }
                    }
                    else if (!order.AsPresale)
                    {
                        /* foreach (var detail1 in detail.ParticipatingDetails)
                         {
                             if (!detail1.Product.SoldByWeight)
                                 continue;

                             if (!string.IsNullOrEmpty(weights))
                                 weights += ",";
                             weights += detail1.Weight;
                         }*/

                        StringBuilder sb = new StringBuilder();
                        List<string> lotUsed = new List<string>();

                        int TotalCases = 0;


                        foreach (var detail1 in detail.Value)
                        {
                            if (!string.IsNullOrEmpty(detail1.Lot) && detail1.Product.SoldByWeight && detail1.Product.Weight == 0)
                            {
                                if (sb.Length > 0)
                                    sb.Append(", ");
                                else
                                    sb.Append("Weight: ");

                                if (detail1.Weight != 0)
                                    sb.Append(detail1.Weight.ToString());


                                lotUsed.Add(detail1.Lot);
                                TotalCases++;
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(sb.ToString()))
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, "Cases: " + TotalCases.ToString() + " " + sb.ToString()));
                            startIndex += font18Separation;
                        }

                        if (!string.IsNullOrEmpty(weights))
                        {
                            foreach (var item in GetOrderDetailsRowsSplitProductName(weights))
                            {
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeights], startIndex, item));
                                startIndex += font18Separation;
                            }
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsWeightsCount], startIndex, detail.Value.Count));
                            startIndex += font18Separation;
                        }

                        // anderson crap
                        // the retail price
                        var extraProperties = order.Client.ExtraProperties;
                        if (extraProperties != null && p.ExtraProperties != null && extraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "tickettype" && x.Item2 == "4") != null)
                        {
                            var retailPrice = p.ExtraProperties.FirstOrDefault(x => x.Item1 == "retail");
                            if (retailPrice != null)
                            {
                                string retPriceString = "                                  " + ToString(Convert.ToDouble(retailPrice.Item2));
                                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLinesRetailPrice], startIndex, retPriceString));
                                startIndex += font18Separation;
                            }
                        }

                        list.AddRange(GetUpcForProductInOrder(ref startIndex, order, p));

                    }
                    if (!Config.HidePrintedCommentLine && !string.IsNullOrEmpty(detail.Value.FirstOrDefault().Comments.Trim()))
                    {
                        foreach (string commentPArt in GetOrderDetailsSplitComment(detail.Value.FirstOrDefault().Comments))
                        {
                            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "C: " + commentPArt));
                            startIndex += font18Separation;
                        }
                    }

                    startIndex += 10;
                }
                #endregion

                var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, sectionName, totalUnits, balance,totalWeight, TotalShipped));


            return list;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
  
}

