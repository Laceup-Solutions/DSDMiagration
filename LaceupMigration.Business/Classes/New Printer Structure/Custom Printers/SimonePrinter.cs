using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SimonePrinter : ZebraFourInchesPrinter1
    {
        protected const string SimoneOrderHeader = "SimoneOrderHeader";
        protected const string SimoneOrderLine = "SimoneOrderLine";
        protected const string SimoneOrderSectionTotal = "SimoneOrderSectionTotal";

        protected const string SimoneCompanyAddress = "SimoneCompanyAddress";
        protected const string SimoneCompanyPhone = "SimoneCompanyPhone";
        protected const string SimoneCompanyFax = "SimoneCompanyFax";
        protected const string SimoneCompanyEmail = "SimoneCompanyEmail";
        protected const string SimoneCompanyLicenses1 = "SimoneCompanyLicenses1";
        protected const string SimoneCompanyLicenses2 = "SimoneCompanyLicenses2";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(SimoneOrderHeader, "^CF0,25^FO40,{0}^FDITEM^FS" +
                "^CF0,25^FO350,{0}^FDDELIVERED^FS" +
                "^CF0,25^FO480,{0}^FDUNDEL^FS" +
                "^CF0,25^FO580,{0}^FDPRICE^FS" +
                "^CF0,25^FO680,{0}^FDTOTAL^FS");

            linesTemplates.Add(SimoneOrderLine, "^CF0,25^FO40,{0}^FD{1}^FS" +
                "^CF0,25^FO350,{0}^FD{2}^FS" +
                "^CF0,25^FO480,{0}^FD{3}^FS" +
                "^CF0,25^FO580,{0}^FD{4}^FS" +
                "^CF0,25^FO680,{0}^FD{5}^FS");

            linesTemplates.Add(SimoneOrderSectionTotal, "^CF0,25^FO250,{0}^FDTOTALS: ^FS" +
                "^CF0,25^FO350,{0}^FD{1}^FS" +
                "^CF0,25^FO480,{0}^FD{2}^FS" +
                "^CF0,25^FO680,{0}^FD{3}^FS");

            linesTemplates.Add(SimoneCompanyAddress, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(SimoneCompanyPhone, "^CF0,25^FO40,{0}^FDPhone: {1}^FS");
            linesTemplates.Add(SimoneCompanyFax, "^CF0,25^FO40,{0}^FDFax: {1}^FS");
            linesTemplates.Add(SimoneCompanyEmail, "^CF0,25^FO40,{0}^FDCorreo: {1}^FS");
            linesTemplates.Add(SimoneCompanyLicenses1, "^CF0,25^FO40,{0}^FDLicenses: {1}^FS");
            linesTemplates.Add(SimoneCompanyLicenses2, "^CF0,25^FO40,{0}^FD           {1}^FS");

            #region Order

            linesTemplates[OrderClientName] = "^FO40,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderClientNameTo] = "^FO40,{0}^ADN,18,10^FDCustomer: {1}^FS";
            linesTemplates[OrderClientAddress] = "^CF0,25^FO40,{0}^FD{1}^FS";
            linesTemplates[OrderBillTo] = "^CF0,25^FO40,{0}^FDBill To: {1}^FS";
            linesTemplates[OrderBillTo1] = "^CF0,25^FO40,{0}^FD         {1}^FS";
            linesTemplates[OrderShipTo] = "^CF0,25^FO40,{0}^FDShip To: {1}^FS";
            linesTemplates[OrderShipTo1] = "^CF0,25^FO40,{0}^FD         {1}^FS";
            linesTemplates[OrderClientLicenceNumber] = "^CF0,25^FO40,{0}^FDLicense Number: {1}^FS";
            linesTemplates[OrderVendorNumber] = "^CF0,25^FO40,{0}^FDVendor Number: {1}^FS";
            linesTemplates[OrderTerms] = "^CF0,25^FO40,{0}^FDTerms: {1}^FS";
            linesTemplates[OrderAccountBalance] = "^CF0,25^FO40,{0}^FDAccount Balance: {1}^FS";

            linesTemplates[OrderTypeAndNumber] = "^FO40,{0}^ADN,36,20^FD{2} #: {1}^FS";
            linesTemplates[PONumber] = "^FO40,{0}^ADN,36,20^FDPO #: {1}^FS";

            linesTemplates[OrderPaymentText] = "^FO40,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderHeaderText] = "^FO40,{0}^ADN,36,20^FD{1}^FS";

            linesTemplates[OrderDetailsHeader] = "^CF0,25^FO40,{0}^FDPRODUCT^FS" +
                "^CF0,25^FO450,{0}^FDQTY^FS" +
                "^CF0,25^FO580,{0}^FDPRICE^FS" +
                "^CF0,25^FO680,{0}^FDTOTAL^FS";
            linesTemplates[OrderDetailsLineSeparator] = "^CF0,25^FO40,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsHeaderSectionName] = "^CF0,25^FO400,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsLines] = "^CF0,25^FO40,{0}^FD{1}^FS" +
                "^CF0,25^FO450,{0}^FD{2}^FS" +
                "^CF0,25^FO580,{0}^FD{4}^FS" +
                "^CF0,25^FO680,{0}^FD{3}^FS";
            linesTemplates[OrderDetailsLines2] = "^CF0,25^FO40,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsLinesLotQty] = "^CF0,25^FO40,{0}^FDLot: {1} -> {2}^FS";
            linesTemplates[OrderDetailsWeights] = "^CF0,25^FO40,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsWeightsCount] = "^CF0,25^FO40,{0}^FDQty: {1}^FS";
            linesTemplates[OrderDetailsLinesRetailPrice] = "^CF0,25^FO40,{0}^FDRetail price {1}^FS";
            linesTemplates[OrderDetailsLinesUpcText] = "^CF0,25^FO40,{0}^FD{1}^FS";
            linesTemplates[OrderDetailsLinesUpcBarcode] = "^FO40,{0}^BUN,40^FD{1}^FS";
            linesTemplates[OrderDetailsTotals] = "^CF0,25^FO40,{0}^FD{1}^FS" +
                "^CF0,25^FO320,{0}^FD{2}^FS" +
                "^CF0,25^FO450,{0}^FD{3}^FS" +
                "^CF0,25^FO680,{0}^FD{4}^FS";
            linesTemplates[OrderTotalsNetQty] = "^FO40,{0}^ADN,36,20^FD        NET QTY: {1}^FS";
            linesTemplates[OrderTotalsSales] = "^FO40,{0}^ADN,36,20^FD          SALES: {1}^FS";
            linesTemplates[OrderTotalsCredits] = "^FO40,{0}^ADN,36,20^FD        CREDITS: {1}^FS";
            linesTemplates[OrderTotalsReturns] = "^FO40,{0}^ADN,36,20^FD        RETURNS: {1}^FS";
            linesTemplates[OrderTotalsNetAmount] = "^FO40,{0}^ADN,36,20^FD     NET AMOUNT: {1}^FS";
            linesTemplates[OrderTotalsDiscount] = "^FO40,{0}^ADN,36,20^FD       DISCOUNT: {1}^FS";
            linesTemplates[OrderTotalsTax] = "^FO40,{0}^ADN,36,20^FD{1} {2}^FS";
            linesTemplates[OrderTotalsTotalDue] = "^FO40,{0}^ADN,36,20^FD      TOTAL DUE: {1}^FS";
            linesTemplates[OrderTotalsTotalPayment] = "^FO40,{0}^ADN,36,20^FD  TOTAL PAYMENT: {1}^FS";
            linesTemplates[OrderTotalsCurrentBalance] = "^FO40,{0}^ADN,36,20^FDINVOICE BALANCE: {1}^FS";
            linesTemplates[OrderTotalsClientCurrentBalance] = "^FO40,{0}^ADN,36,20^FD   OPEN BALANCE: {1}^FS";
            linesTemplates[OrderTotalsDiscountComment] = "^CF0,25^FO40,{0}^FD Discount Comment: {1}^FS";
            linesTemplates[OrderPreorderLabel] = "^FO40,{0}^ADN,36,20^FD{1}^FS";
            linesTemplates[OrderComment] = "^CF0,25^FO40,{0}^FDComments: {1}^FS";
            linesTemplates[OrderComment2] = "^CF0,25^FO40,{0}^FD          {1}^FS";
            linesTemplates[PaymentComment] = "^CF0,25^FO40,{0}^FDPayment Comments: {1}^FS";
            linesTemplates[PaymentComment1] = "^CF0,25^FO40,{0}^FD                  {1}^FS";

            #endregion
        }

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (Config.UseAllowanceForOrder(order))
                return GetDetailsRowsInOneDocForAllowance(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();

            if (order.IsDelivery)
                list.AddRange(GetDetailTableHeader(ref startY));
            else
                list.AddRange(base.GetDetailTableHeader(ref startY));

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            if (sales.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, sales.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(sales.Values.ToList());

                var listXX = lines.ToList();
                var relatedDetailIds = listXX.Where(x => x.OrderDetail.RelatedOrderDetail > 0).Select(x => x.OrderDetail.RelatedOrderDetail).ToList();
                var removedList = listXX.Where(x => relatedDetailIds.Contains(x.OrderDetail.OrderDetailId)).ToList();
                foreach (var r in removedList)
                    listXX.Remove(r);
                // reinsert
                // If grouping, add at the end
                if (Config.GroupRelatedWhenPrinting)
                {
                    foreach (var r in removedList)
                        listXX.Add(r);
                }
                else
                    foreach (var r in removedList)
                    {
                        for (int index = 0; index < listXX.Count; index++)
                            if (listXX[index].OrderDetail.RelatedOrderDetail == r.OrderDetail.OrderDetailId)
                            {
                                listXX.Insert(index + 1, r);
                                break;
                            }
                    }


                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }
            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(0), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), GetOrderDetailSectionHeader(1), factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetDetailTableHeader(ref int startY)
        {
            List<string> lines = new List<string>();

            string formatString = linesTemplates[SimoneOrderHeader];

            if (Config.HidePriceInPrintedLine)
                HidePriceInOrderPrintedLine(ref formatString);

            if (Config.HideTotalInPrintedLine)
                HideTotalInOrderPrintedLine(ref formatString);

            lines.Add(string.Format(CultureInfo.InvariantCulture, formatString, startY));
            startY += font18Separation;

            return lines;
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 24, 24);
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            if (!order.IsDelivery)
                return base.GetSectionRowsInOneDoc(ref startIndex, lines, sectionName, factor, order, preOrder);

            List<string> list = new List<string>();

            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsHeaderSectionName], startIndex, sectionName));
            startIndex += font18Separation;

            double deliveredTotal = 0;
            double undeliveredTotal = 0;
            double balance = 0;

            Dictionary<string, float> uomMap = new Dictionary<string, float>();

            foreach (var detail in lines)
            {
                Product p = detail.Product;
                
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
                        double delivered = 0;
                        double undelivered = 0;

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

                            delivered += qty;
                            undelivered += _.Ordered - qty > 0 ? _.Ordered - qty : 0;

                            d += double.Parse(Math.Round(Convert.ToDecimal(_.Price * factor * qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                        }

                        double price = detail.Price * factor;

                        balance += d;
                        deliveredTotal += delivered;
                        undeliveredTotal += undelivered;

                        string priceAsString = ToString(price);
                        string totalAsString = ToString(d);

                        if (Config.HidePriceInPrintedLine)
                            priceAsString = string.Empty;
                        if (Config.HideTotalInPrintedLine)
                            totalAsString = string.Empty;
                        
                        list.Add(string.Format(linesTemplates[SimoneOrderLine], startIndex, pName, 
                            Math.Round(delivered, Config.Round).ToString(), 
                            Math.Round(undelivered, Config.Round).ToString(), 
                            priceAsString, totalAsString));
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

                if(detail.OrderDetail.ReasonId > 0)
                {
                    var reason = Reason.Find(detail.OrderDetail.ReasonId);

                    if (reason != null)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLines2], startIndex, "Reason: " + reason.Description));
                        startIndex += font18Separation;
                    }
                }

                if (Config.PrintLotPreOrder || Config.PrintLotOrder)
                {
                    foreach (var item in detail.ParticipatingDetails)
                    {
                        string qty = item.Qty.ToString();
                        if (item.Product.SoldByWeight && !order.AsPresale)
                            qty = item.Weight.ToString();

                        if (!string.IsNullOrEmpty(item.Lot))
                        {
                            if (preOrder)
                            {
                                if (Config.PrintLotPreOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        item.Lot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                            else
                            {
                                if (Config.PrintLotOrder)
                                {
                                    list.Add(GetSectionRowsInOneDocFixedLotLine(OrderDetailsLinesLotQty, startIndex,
                                        item.Lot, qty));
                                    startIndex += font18Separation;
                                }
                            }
                        }
                        else
                        {
                            if (item.Product.SoldByWeight && !order.AsPresale)
                                weights += item.Weight.ToString() + " ";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(weights) && detail.ParticipatingDetails.Count > 1)
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

            list.AddRange(GetOrderDetailsSectionTotal(ref startIndex, order, sectionName, deliveredTotal, undeliveredTotal, balance));

            return list;
        }

        protected List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, string sectionName, double totalDelivered, double totalUndelivery, double balance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");
            
            if (!Config.HideSubTotalOrder && t == null)
            {
                list.Add(string.Format(linesTemplates[SimoneOrderSectionTotal], 
                    startIndex, 
                    Math.Round(totalDelivered, Config.Round).ToString(CultureInfo.CurrentCulture), 
                    Math.Round(totalUndelivery, Config.Round).ToString(CultureInfo.CurrentCulture), 
                    Math.Round(balance, Config.Round).ToCustomString()));
                startIndex += font18Separation;
            }

            return list;
        }


        protected override IEnumerable<string> GetCompanyRows(ref int startIndex, Order order)
        {
            try
            {
                CompanyInfo company = null;

                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startIndex, part));
                    startIndex += font36Separation;
                }
                // startIndex += font36Separation;
                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SimoneCompanyAddress], startIndex, company.CompanyAddress1));
                startIndex += font18Separation;
                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SimoneCompanyAddress], startIndex, company.CompanyAddress2));
                    startIndex += font18Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SimoneCompanyPhone], startIndex, company.CompanyPhone));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SimoneCompanyFax], startIndex, company.CompanyFax));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[SimoneCompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyLicenses))
                {
                    var licenses = company.CompanyLicenses.Split(',').ToList();

                    for (int i = 0; i < licenses.Count; i++)
                    {
                        var format = i == 0 ? SimoneCompanyLicenses1 : SimoneCompanyLicenses2;

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

    }
}