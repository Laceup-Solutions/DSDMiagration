





using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class ADUnitedPrinter : ZebraFourInchesPrinter1
    {

        protected const string OrderTimeLine = "OrderTimeLine";
        protected const string OrderClientAddress1 = "OrderClientAddress1";
        protected const string XtremeInvoice = "XtremeInvoice";
        protected const string XtremeDate = "XtremeDate";
        protected const string XtremeAccount = "XtremeAccount";
        protected const string XtremeSalesRep = "XtremeSalesRep";
        protected const string XtremeTime = "XtremeTime";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(OrderTimeLine, "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1}^FS");
            linesTemplates.Add(OrderClientAddress1, "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD         {1}^FS");
            //Center Header
            linesTemplates[CompanyName] = "^FO40,{0}^A0N,45,45^FB700,1,0,C^FD{1}^FS";
            linesTemplates[CompanyAddress] = "^FO40,{0}^CFO,25,12^FB700,1,0,C^FD{1}^FS";
            linesTemplates[CompanyPhone] = "^FO40,{0}^CFO,25,12^FB700,1,0,C^FDTel: {1}^FS";
            linesTemplates[CompanyFax] = "^FO40,{0}^ADN,18,10^FB700,1,0,C^FDFax: {1}^FS";
            linesTemplates[CompanyEmail] = "^FO40,{0}^ADN,18,10^FB700,1,0,C^FDEmail: {1}^FS";

            linesTemplates[StandarPrintTitle] = "^FO40,{0}^A0N,25,30^FB700,1,0,R^FD{1}^FS";
            linesTemplates[StandarPrintDate] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1}^FS";
            linesTemplates[StandarPrintRouteNumber] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1}^FS";
            linesTemplates[StandarPrintDriverName] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1}^FS";
            linesTemplates[StandarPrintCreatedBy] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1}^FS";

            linesTemplates.Add(XtremeInvoice, "^FO40,{0}^A0N,25,30^FB530,1,0,R^FD{1}^FS");
            linesTemplates.Add(XtremeDate, "^FO40,{0}^ADN,18,10^FB530,1,0,R^FDDate:^FS");
            linesTemplates.Add(XtremeAccount, "^FO40,{0}^ADN,18,10^FB530,1,0,R^FDAccount#:^FS");
            linesTemplates.Add(XtremeSalesRep, "^FO40,{0}^ADN,18,10^FB530,1,0,R^FDSales Rep:^FS");
            linesTemplates.Add(XtremeTime, "^FO40,{0}^ADN,18,10^FB530,1,0,R^FDTime:^FS");

            linesTemplates[OrderClientName] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDCustomer: {1}^FS";
            linesTemplates[OrderClientAddress] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDAddress: {1}^FS";
            linesTemplates[OrderBillTo] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDBill To: {1}^FS";
            linesTemplates[OrderBillTo1] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD         {1}^FS";
            linesTemplates[OrderShipTo] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDShip To: {1}^FS";
            linesTemplates[OrderShipTo1] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD         {1}^FS";
            linesTemplates[OrderClientLicenceNumber] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDLicense Number: {1}^FS";
            linesTemplates[OrderVendorNumber] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDVendor Number: {1}^FS";
            linesTemplates[OrderTerms] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDTerms: {1}^FS";
            linesTemplates[OrderAccountBalance] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDAccount Balance: {1}^FS";
            linesTemplates[OrderTypeAndNumber] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD #: {1}^FS";
            linesTemplates[PONumber] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FDPO #: {1}^FS";

            linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
          "^FO290,{0}^ADN,18,10^FDQTY^FS" +
          "^FO360,{0}^ADN,18,10^FDUNITS^FS" +
          "^FO450,{0}^ADN,18,10^FDO.U.PRICE^FS" +
          "^FO580,{0}^ADN,18,10^FDALLOW^FS" +
          "^FO680,{0}^ADN,18,10^FDTOTAL^FS";

            linesTemplates[OrderDetailsLines] = "^FO40,{0}^ADN,18,10^FD{1}^FS" +
               "^FO290,{0}^ADN,18,10^FD{2}^FS" +
               "^FO360,{0}^ADN,18,10^FD{3}^FS" +
               "^FO450,{0}^ADN,18,10^FD{4}^FS" +
               "^FO580,{0}^ADN,18,10^FD{5}^FS" +
               "^FO680,{0}^ADN,18,10^FD{6}^FS";

            linesTemplates[OrderTotalsNetQty] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDNet Qty: {1}^FS";
            linesTemplates[OrderTotalsSales] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDSubtotal: {1}^FS";
            linesTemplates[OrderTotalsCredits] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDCredits: {1}^FS";
            linesTemplates[OrderTotalsReturns] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDReturns: {1}^FS";
            linesTemplates[OrderTotalsNetAmount] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDTotal: {1}^FS";
            linesTemplates[OrderTotalsDiscount] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDDiscount: {1}^FS";
            linesTemplates[OrderTotalsTax] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FD{1} {2}^FS";
            linesTemplates[OrderTotalsTotalDue] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDTotal: {1}^FS";
            linesTemplates[OrderTotalsTotalPayment] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDPayment: {1}^FS";
            linesTemplates[OrderTotalsCurrentBalance] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDBalance: {1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance] = "^FO40,{0}^ADN,18,10^FB700,1,0,R^FDOpen Balance: {1}^FS";

            linesTemplates[OrderDetailsTotals] = "^FO40,{0}^ADN,18,10^FB700,1,0,L^FD{1}^FS";

            //linesTemplates[OrderDetailsHeader] = "^FO40,{0}^ADN,18,10^FDDescription^FS" +
         //"^FO450,{0}^ADN,18,10^FDQty^FS" +
         //"^FO580,{0}^ADN,18,10^FDPrice^FS" +
        // "^FO680,{0}^ADN,18,10^FDTotal^FS";
        }


        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            float totalQtyNoUoM = 0;
            float totalUnits = 0;
            double balance = 0;
            float totalUnitsPackage = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

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

                        var packaging_str = detail.Product.Package;
                        int package = 0;
                        Int32.TryParse(packaging_str, out package);

                        totalUnitsPackage += package;

                    }
                    else
                    {
                        if (!detail.OrderDetail.SkipDetailQty(order))
                        {
                            int packaging = 0;

                            if (!string.IsNullOrEmpty(detail.OrderDetail.Product.Package))
                                int.TryParse(detail.OrderDetail.Product.Package, out packaging);
                            

                            totalQtyNoUoM += detail.Qty;

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

                foreach (string pName in productSlices)
                {
                    if (productLineOffset == 0)
                    {
                        if (preOrder && Config.PrintZeroesOnPickSheet)
                            factor = 0;

                        double d = 0;
                        foreach (var _ in detail.ParticipatingDetails)
                        {
                            double qty = _.Qty;

                            if (_.Product.SoldByWeight)
                            {
                                if (order.AsPresale)
                                    qty *= _.Product.Weight;
                                else
                                    qty = _.Weight;
                            }

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
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

                        totalUnitsPackage += package;
                        var units = package; //detail.Qty * package

                        var unitPrice = detail.Product.PriceLevel0 / package;  //pricelevel0 = precio original || detail.price = price level , before U.Price : var unitPrice = detail.Price / package; 

                        /*double discount;
                        if (detail.OrderDetail.Damaged)
                        {
                             discount = detail.Product.PriceLevel0 / package - detail.Price;
                        }
                        else
                        {
                            discount = (detail.Product.PriceLevel0 - detail.Price) * units;
                        }
                        if (discount < 0)
                            discount = 0;*/

                        double discount = 0;
                        discount += CalculateDiscount(detail.OrderDetail);

                        list.Add(GetSectionRowsInOneDocFixedLine(OrderDetailsLines, startIndex, pName, qtyAsString, units.ToString(), unitPrice.ToCustomString(), discount.ToCustomString(), totalAsString));
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
                    foreach (var item in detail.ParticipatingDetails)
                    {
                        var itemLot = item.Lot ?? "";
                        if (!string.IsNullOrEmpty(itemLot) && item.LotExpiration != DateTime.MinValue)
                            itemLot += "  Exp: " + item.LotExpiration.ToShortDateString();

                        string qty = item.Qty.ToString();
                        if (item.Product.SoldByWeight && !order.AsPresale)
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
                            if (item.Product.SoldByWeight && !order.AsPresale)
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
                        if (!string.IsNullOrEmpty(detail1.Lot) && detail1.Product.SoldByWeight)
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

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startIndex, s));
            startIndex += font18Separation;

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, uomMap, totalQtyNoUoM, totalUnitsPackage, sectionName, totalUnits, balance));

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

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool asPreOrder, Order order, Client client, string printedId, List<DataAccess.PaymentSplit> payments, bool paidInFull)
        {
            var invoiceIndex = 0;
            var accIndex = 0;

            List<string> lines = new List<string>();
            startY += 10;

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            bool printExtraDocName = true;
            string docName = GetOrderDocumentName(ref printExtraDocName, order, client);

            string s1 = docName;
            if (!string.IsNullOrEmpty(order.PrintedOrderId) && !Config.PrintInvoiceNumberDown)
                s1 = order.PrintedOrderId;

            invoiceIndex = startY;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeInvoice], startY, docName + "#:"));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintTitle], startY, s1));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeAccount], startY));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            if (salesman != null)
            {
                if (order.IsDelivery)
                {
                    var originalSalesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeSalesRep], startY));

                    foreach (var l in SplitSalesmanName(salesman.Name, 15, 15))
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, l));
                        startY += font18Separation;
                    }
                }
                else
                {
                    if (salesman.Roles == SalesmanRole.Driver)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeSalesRep], startY));

                        foreach (var l in SplitSalesmanName(Config.VendorName, 15, 15))
                        {
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, l));
                            startY += font18Separation;
                        }
                    }
                    else
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeSalesRep], startY));

                        foreach (var l in SplitSalesmanName(Config.VendorName, 15, 15))
                        {
                            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, l));
                            startY += font18Separation;
                        }
                    }
                }
            }

            if (Config.UseBigFontForPrintDate)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDateBig], startY, DateTime.Now.ToString("MM/dd/yyyy")));
                startY += 40;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeDate], startY));

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString("MM/dd/yyyy")));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[XtremeTime], startY));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTimeLine], startY, DateTime.Now.ToString("hh:mm tt")));
            startY += font18Separation;

            /*  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
              startY += font18Separation;

              var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
              if (salesman != null)
              {
                  lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintCreatedBy], startY, salesman.Name));
                  startY += font18Separation;
              }*/

            startY = invoiceIndex;

            foreach (var clientSplit in GetClientNameSplit(order.Client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientName], startY, clientSplit));
                startY += font18Separation;
            }

            var custno = DataAccess.ExplodeExtraProperties(order.Client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            if (Config.PrintBillShipDate)
            {
                startY += 10;

                var addrFormat1 = linesTemplates[OrderBillTo];

                foreach (string s in ClientAddress(client, false))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderBillTo1];
                }

                startY += font18Separation;
                addrFormat1 = linesTemplates[OrderShipTo];

                foreach (string s in ClientAddress(client, true))
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                    startY += font18Separation;

                    addrFormat1 = linesTemplates[OrderShipTo1];
                }
            }
            else
            {
                int c = 0;
                foreach (string s in ClientAddress(client))
                {
                    if (c == 0)
                    {
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, string.Empty));
                        startY += font18Separation;
                    }

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress1], startY, s.Trim()));
                    c++;
                    startY += font18Separation;
                }
            }

            if (!string.IsNullOrEmpty(client.LicenceNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientLicenceNumber], startY, client.LicenceNumber));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(client.VendorNumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderVendorNumber], startY, client.VendorNumber));
                startY += font18Separation;
            }

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, term));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = ToString(order.Client.OpenBalance);

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderAccountBalance], startY, balance));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            if (Config.PrintInvoiceNumberDown)
                if (printExtraDocName)
                {
                    if (order.AsPresale && order.OrderType == OrderType.Order)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Bill)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, docName));

                    else
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTypeAndNumber], startY, printedId, ""));
                    startY += font36Separation + font18Separation;
                }

            if (!Config.HidePONumber)
            {
                if (!string.IsNullOrEmpty(order.PONumber))
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
                else if (Config.AutoGeneratePO)
                {
                    order.PONumber = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                    startY += font36Separation;
                }
            }

            //if (payments != null && order.OrderType == OrderType.Order && payments.Count > 0)
            //{
            //    AddExtraSpace(ref startY, lines, font36Separation, 1);
            //    lines.AddRange(GetPaymentLines(ref startY, payments, paidInFull));
            //}

            AddExtraSpace(ref startY, lines, font36Separation, 1);

            return lines;
        }

        protected override List<string> GetOrderLabel(ref int startY, Order order, bool asPreOrder, bool fromBatch)
        {
            List<string> lines = new List<string>();

            if (fromBatch)
                return lines;

            if (!order.AsPresale && !order.Finished)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, "Not a Final Invoice"));
                startY += font36Separation + 5;
            }

            return lines;
        }

        protected virtual List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, float totalUnitsPackage, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            var printString = string.Empty;
            startIndex += font18Separation;

            //if (sectionName == "DUMP SECTION")
            //    printString = "Credit Units: " + totalUnitsPackage + " EA"; //the client wants the total of Units not QTY
            //if (sectionName == "SALES SECTION")
            //    printString = "Total Units: " + totalUnitsPackage + " EA"; //printString = "Total Units: " + totalQtyNoUoM + " EA";
            //if (sectionName == "RETURNS SECTION")
            //    printString = "Return Units: " + totalUnitsPackage + " EA";

            if (sectionName == "DUMP SECTION")
                printString = "Credit Qty: " + totalQtyNoUoM; //el cliente volvio ahora apedir ver el qty :)
            if (sectionName == "SALES SECTION")
                printString = "Total Qty: " + totalQtyNoUoM; //printString = "Total Units: " + totalQtyNoUoM + " EA";
            if (sectionName == "RETURNS SECTION")
                printString = "Return Qty: " + totalQtyNoUoM;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsTotals], startIndex, printString));
            startIndex += font18Separation;

            return list;
        }


        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                List<string> list = new List<string>();

                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();
                if (order.CompanyId > 0)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                var name = company.CompanyName;
                name = name.Replace("CA", "");
                name = name.Trim();
                foreach (string part in CompanyNameSplit(name))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                    startIndex += font36Separation + 5;
                }
                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.CompanyAddress1));
                startIndex += font18Separation + 10;

                if (!string.IsNullOrEmpty(company.CompanyPhone))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyPhone], startIndex, company.CompanyPhone));
                    startIndex += font18Separation + 20;
                }

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, company.CompanyFax));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.Vendor))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyAddress], startIndex, company.Vendor));
                    startIndex += font18Separation + 10;
                }

                if (!string.IsNullOrEmpty(company.CompanyLicenses))
                {
                    var licenses = company.CompanyLicenses.Split(',').ToList();

                    for (int i = 0; i < licenses.Count; i++)
                    {
                        var format = i == 0 ? CompanyLicenses1 : CompanyLicenses2;

                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[format], startIndex, licenses[i]));
                        startIndex += font18Separation;
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {
            List<string> list = new List<string>();

            startY -= (4 * font18Separation);


            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;
            double salesSubTotal = 0;

            double totalSales = 0;
            double totalCredit = 0;
            double totalReturn = 0;
            



            double paid = 0;
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            double taxableAmount = 0;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            foreach (var key in sales.Keys)
            {
                double subt = 0;

                foreach (var od in sales[key].ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalSales += qty;

                    var x = od.Price * factor * qty;

                    
                    subt = od.Product.PriceLevel0 * qty;

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);
                    subt = double.Parse(Math.Round(subt, Config.Round).ToCustomString(), NumberStyles.Currency);


                    salesBalance += x;
                    salesSubTotal += subt;//subtotal = sum original price


                    if (sales[key].Product.Taxable)
                        taxableAmount += x;
                }
            }
            foreach (var key in credit.Keys)
            {
                foreach (var od in credit[key].ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalCredit += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    creditBalance += x * -1;

                    if (credit[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }
            foreach (var key in returns.Keys)
            {
                foreach (var od in returns[key].ParticipatingDetails)
                {
                    double qty = od.Qty;

                    if (od.Product.SoldByWeight)
                    {
                        if (order.AsPresale)
                            qty *= od.Product.Weight;
                        else
                            qty = od.Weight;
                    }

                    totalReturn += qty;

                    var x = od.Price * factor * qty;
                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    returnBalance += x * -1;

                    if (returns[key].Product.Taxable)
                        taxableAmount -= x;
                }
            }

            string s1;

            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");


            bool printTotal = true;
            if (t != null)
            {
                printTotal = !(t.Item2 == "Y");
            }

            if (!Config.HideTotalOrder && printTotal)
            {
                if (Config.PrintNetQty)
                {
                    s1 = (totalSales - totalCredit - totalReturn).ToString();
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetQty], startY, s1));
                    startY += font18Separation;
                }

                //s1 = ToString(salesBalance);
                s1 = ToString(salesSubTotal);
                s1 = new string(' ', 14 - s1.Length) + s1;
                startY += 3 * font18Separation; //to go 3 lines down 
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));//list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsSales], startY, s1));
                startY += font18Separation;

                s1 = ToString(creditBalance + returnBalance);
                s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCredits], startY, s1));
                startY += font18Separation;

                //s1 = ToString(Math.Round((salesBalance + creditBalance + returnBalance), Config.Round));
                //s1 = new string(' ', 14 - s1.Length) + s1;
                //list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsNetAmount], startY, s1));
                //startY += font18Separation;

                double discount = 0;
                foreach(var detail in order.Details)
                        discount += CalculateDiscount(detail); //double discount = order.CalculateDiscount();

                var orderTotalDiscoutn = order.CalculateDiscount();

                discount += orderTotalDiscoutn;

                //if (order.Client.UseDiscount || order.Client.UseDiscountPerLine)
                //{

                    s1 = ToString(Math.Abs(discount));
                    s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscount], startY, s1));
                    startY += font18Separation;
                //}

                double tax = order.CalculateTax();
                //var s = Config.PrintTaxLabel;
                //if (Config.PrintTaxLabel.Length < 16)
                //    s = new string(' ', 16 - Config.PrintTaxLabel.Length) + Config.PrintTaxLabel;

                //s1 = ToString(tax);
                //s1 = new string(' ', 14 - s1.Length) + s1;
                //list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTax], startY, s, s1));
                //startY += font18Separation;

                var orderCredit = order.Details.FirstOrDefault(x => x.IsCredit == true);
                if (orderCredit != null)
                    discount = 0;

                //var s4 = salesBalance + creditBalance + returnBalance - discount + tax; //Total before

                var s4 = salesBalance + creditBalance + returnBalance - orderTotalDiscoutn; // var s4 = salesBalance , the client wants the total=subtotal-credits
                s1 = ToString(Math.Round(s4, Config.Round));
                s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalDue], startY, s1));
                startY += font18Separation;

                if (order.OrderType == OrderType.Order && !Config.RemovePayBalFomInvoice)
                {
                    s1 = ToString(Math.Round(paid, Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsTotalPayment], startY, s1));
                    startY += font18Separation;

                    s1 = ToString(Math.Round((s4 - paid), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsCurrentBalance], startY, s1));
                    startY += font18Separation;
                }

                if (Config.PrintClientTotalOpenBalance)
                {
                    s1 = ToString(Math.Round(order.Client.CurrentBalance(), Config.Round));
                    s1 = s1 = new string(' ', 14 - s1.Length) + s1;
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsClientCurrentBalance], startY, s1));
                    startY += font18Separation;
                }

                if (!string.IsNullOrEmpty(order.DiscountComment))
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTotalsDiscountComment], startY, order.DiscountComment));
                    startY += font18Separation;
                }
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font18Separation;
            }

            if (!string.IsNullOrEmpty(order.Comments) && !Config.HideInvoiceComment)
            {
                startY += font18Separation;
                var clines = GetOrderSplitComment(order.Comments);
                for (int i = 0; i < clines.Count; i++)
                {
                    string format = linesTemplates[OrderComment];
                    if (i > 0)
                        format = linesTemplates[OrderComment2];

                    list.Add(string.Format(CultureInfo.InvariantCulture, format, startY, clines[i]));
                    startY += font18Separation;
                }

            }

            if (payment != null)
            {
                var paymentComments = payment.GetPaymentComment();

                for (int i = 0; i < paymentComments.Count; i++)
                {
                    string format = i == 0 ? PaymentComment : PaymentComment1;

                    var pcLines = GetOrderPaymentSplitComment(paymentComments[i]).ToList();

                    for (int j = 0; j < pcLines.Count; j++)
                    {
                        if (i == 0 && j > 0)
                            format = PaymentComment1;

                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[format], startY, pcLines[j]));
                        startY += font18Separation;
                    }

                }
            }

            var d = string.Empty;
            d = new string('-', WidthForNormalFont - d.Length) + d;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, d));
            startY += font18Separation;
            startY += font18Separation;


            return list;
        }

        protected override IEnumerable<string> GetSignatureSection(ref int startY, Order order, bool asPreOrder, List<string> all_lines = null)
        {
            List<string> lines = new List<string>();

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));

                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));

                startY += font18Separation;
                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    lines.Add(string.Format(linesTemplates[FooterSignatureNameText], startY, order.SignatureName ?? string.Empty));
                    startY += font18Separation;
                }
                startY += font18Separation;
                if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
                {
                    startY += font18Separation;
                    foreach (var line in GetBottomSplitText())
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterBottomText], startY, line));
                        startY += font18Separation;
                    }
                }
            }
            else
                lines.AddRange(GetFooterRows(ref startY, asPreOrder));

            return lines;
        }

        public static IList<string> SplitSalesmanName(string productName, int firstLine, int otherLines)
        {
            List<string> retList = new List<string>();
            //productName = ConvertProdName(productName);
            if (productName == null)
                return retList;

            string[] parts = productName.Split(new char[] { ' ' });
            int currentSize = 0;
            StringBuilder sb = new StringBuilder(otherLines * 2);
            int size = firstLine;
            bool isFirstLine = true;
            foreach (string part in parts)
                if ((currentSize + part.Length < size) || currentSize == 0)
                {
                    if (currentSize != 0)
                    {
                        sb.Append(" ");
                        sb.Append(part);
                        currentSize++;
                    }
                    else
                        sb.Append(part);
                    currentSize += part.Length;
                }
                else
                {
                    retList.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                    sb.Append(part);
                    currentSize = part.Length;
                    if (isFirstLine)
                    {
                        size = otherLines;
                        isFirstLine = false;
                    }

                }
            if (currentSize > 0)
                retList.Add(sb.ToString());


            return retList;
        }

    }
}