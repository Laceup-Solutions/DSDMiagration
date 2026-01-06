using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;









namespace LaceupMigration
{
    public class FruitIncPrinter : ZebraThreeInchesPrinter1
    {
        protected const string FruitIncCompanyName = "FruitIncCompanyName";
        protected const string FruitIncCompanyAddress = "FruitIncCompanyAddress";
        protected const string FruitIncInvoiceNumber = "FruitIncInvoiceNumber";
        protected const string FruitIncInvoiceDate = "FruitIncInvoiceDate";
        protected const string FruitIncTerms = "FruitIncTerms";
        protected const string FruitIncDriverName = "FruitIncDriverName";
        protected const string FruitIncShipTo1 = "FruitIncShipTo1";
        protected const string FruitIncShipTo2 = "FruitIncShipTo2";
        protected const string FruitIncBillTo1 = "FruitIncBillTo1";
        protected const string FruitIncBillTo2 = "FruitIncBillTo2";
        protected const string FruitIncSectionName = "FruitIncSectionName";
        protected const string FruitIncTableHeader = "FruitIncTableHeader";
        protected const string FruitIncFullHorizontalLine = "FruitIncFullHorizontalLine";
        protected const string FruitIncTableLineProductName = "FruitIncTableLineProductName";
        protected const string FruitIncTableLineDetails = "FruitIncTableLineDetails";
        protected const string FruitIncTableTotalHeader = "FruitIncTableTotalHeader";
        protected const string FruitIncTableTotals = "FruitIncTableTotals";
        protected const string FruitIncSubtotal = "FruitIncSubtotal";
        protected const string FruitIncTotal = "FruitIncTotal";

        protected const string FruitInvoiceTitle = "FruitInvoiceTitle";
        protected const string FruitInvoiceCopy = "FruitInvoiceCopy";
        protected const string FruitOpen = "FruitOpen";
        protected const string FruitPartialPayment = "FruitPartialPayment";
        protected const string FruitCredit = "FruitCredit";
        protected const string FruitPartPayment = "FruitPartPayment";
        protected const string FruitInvoiceOpen = "FruitInvoiceOpen";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(FruitIncCompanyName, "^CF0,40^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncCompanyAddress, "^CFA,20^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncInvoiceNumber, "^CF0,20^FO15,{0}^FDInvoice #: {1}^FS");
            linesTemplates.Add(FruitIncInvoiceDate, "^CFA,20^FO15,{0}^FDInvoice Date: {1}^FS");
            linesTemplates.Add(FruitIncTerms, "^CFA,20^FO15,{0}^FDTerms: {1}^FS");
            linesTemplates.Add(FruitIncDriverName, "^CF0,20^FO15,{0}^FDRep Name: {1}^FS");
            linesTemplates.Add(FruitIncShipTo1, "^CF0,20^FO15,{0}^FDShip to:  {1}^FS");
            linesTemplates.Add(FruitIncShipTo2, "^CFA,20^FO100,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncBillTo1, "^CF0,20^FO15,{0}^FDBill to:  {1}^FS");
            linesTemplates.Add(FruitIncBillTo2, "^CFA,20^FO100,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncSectionName, "^CF0,25^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncTableHeader, "^CF0,20^FO15,{0}^FDQuantity^FS^FO180,{0}^FDU. Price^FS^FO280,{0}^FDAmount^FS");
            linesTemplates.Add(FruitIncFullHorizontalLine, "^FO15,{0}^GB350,1,3^FS");
            linesTemplates.Add(FruitIncTableLineProductName, "^CF0,20^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncTableLineDetails, "^CFA,20^FO15,{0}^FD{1}^FS^FO180,{0}^FD{2}^FS^FO280,{0}^FD{3}^FS");
            linesTemplates.Add(FruitIncTableTotalHeader, "^CF0,20^FO15,{0}^FDTotal Quantity^FS^FO230,{0}^FDTotal Amount^FS");
            linesTemplates.Add(FruitIncTableTotals, "^CFA,20^FO15,{0}^FD{1}^FS^FO280,{0}^FD{2}^FS");
            linesTemplates.Add(FruitIncSubtotal, "^CFA,20^FO100,{0}^FDSubtotal:^FS^FO280,{0}^FD{1}^FS");
            linesTemplates.Add(FruitIncTotal, "^CF0,30^FO100,{0}^FDTotal:^FS^FO280,{0}^FD{1}^FS");

            linesTemplates.Add(FruitInvoiceTitle, "^CF0,40^FO15,{0}^FD{1}^FS");
            linesTemplates.Add(FruitInvoiceCopy, "^CF0,40^FO15,{0}^FDCOPY^FS");
            linesTemplates.Add(FruitOpen, "^CF0,30^FO100,{0}^FD        OPEN:^FS^FO280,{0}^FD{1}^FS");
            linesTemplates.Add(FruitPartialPayment, "^CF0,30^FO40,{0}^FDPART PAYMENT:^FS^FO280,{0}^FD{1}^FS");
            linesTemplates.Add(FruitCredit, "^CF0,30^FO100,{0}^FD   CREDDIT:^FS^FO280,{0}^FD{1}^FS");
            linesTemplates.Add(FruitPartPayment, "^CF0,30^FO40,{0}^FDPART PAYMENT:^FS^FO280,{0}^FD{1}^FS");
            linesTemplates.Add(FruitInvoiceOpen, "^CF0,30^FO100,{0}^FD        OPEN:^FS^FO280,{0}^FD{1}^FS");
        }

        string CenterText(string s, int length)
        {
            int half = length / 2;

            int halfS = s.Length / 2;

            return new String(' ', half - halfS) + s;
        }

        protected override IEnumerable<string> GetHeaderRowsInOneDoc(ref int startY, bool asPreOrder, Order order, Client client, string printedId, List<PaymentSplit> payments, bool paidInFull)
        {
            List<string> lines = new List<string>();
            startY += 10;

            lines.AddRange(GetCompanyRows(ref startY, order));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncInvoiceNumber], startY, order.PrintedOrderId));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncInvoiceDate], startY, order.Date.ToShortDateString()));
            startY += font18Separation;

            string term = order.Term;

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTerms], startY, term));
                startY += font18Separation;
            }

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncDriverName], startY, salesman.Name));
                startY += font18Separation;
            }

            startY += font18Separation;

            var addrFormat1 = linesTemplates[FruitIncBillTo1];

            foreach (string s in ClientAddress(client, false))
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                startY += font18Separation;

                addrFormat1 = linesTemplates[FruitIncBillTo2];
            }

            startY += font18Separation;
            addrFormat1 = linesTemplates[FruitIncShipTo1];

            foreach (string s in ClientAddress(client, true))
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                lines.Add(string.Format(CultureInfo.InvariantCulture, addrFormat1, startY, s.Trim()));
                startY += font18Separation;

                addrFormat1 = linesTemplates[FruitIncShipTo2];
            }

            startY += font18Separation;

            return lines;
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
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyName], startIndex, CenterText(part, 18)));
                    startIndex += font36Separation;
                }

                startIndex += 10;
                
                foreach (string part in CompanyNameSplit(company.CompanyAddress1))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, CenterText(part, 29)));
                    startIndex += font18Separation;
                }

                //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, CenterText(company.CompanyAddress1, 29)));
                //startIndex += font18Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, CenterText(company.CompanyAddress2, 29)));
                    startIndex += font18Separation;
                }

                string phone = "Office Phone: " + company.CompanyPhone;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, CenterText(phone, 29)));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    string fax = "Fax: " + company.CompanyFax;

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, CenterText(company.CompanyFax, 29)));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    //string email = "Email: " + company.CompanyEmail;

                    //list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, CenterText(company.CompanyEmail, 29)));
                    //startIndex += font18Separation;

                     list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                     startIndex += font18Separation;
         
                }

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 18, 18);
        }

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            List<string> list = new List<string>();

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

            var allCredits = new List<OrderLine>(credit.Values);
            allCredits.AddRange(returns.Values);

            if (allCredits.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, allCredits.ToList());
                else
                    lines = SortDetails.SortedDetails(allCredits.ToList());

                list.AddRange(GetSectionRowsInOneDoc(ref startY, lines.ToList(), "CREDIT SECTION", factor == 0 ? 0 : -1, order, preOrder));
                startY += font36Separation;
            }

            return list;
        }

        protected override IEnumerable<string> GetSectionRowsInOneDoc(ref int startIndex, IList<OrderLine> lines, string sectionName, int factor, Order order, bool preOrder)
        {
            List<string> list = new List<string>();

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncSectionName], startIndex, CenterText(sectionName, 30)));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableHeader], startIndex));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncFullHorizontalLine], startIndex));
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
                if (detail.Product.ProductType != ProductType.Discount)
                {
                    if (detail.OrderDetail.UnitOfMeasure != null)
                    {
                        uomString = detail.OrderDetail.UnitOfMeasure.Name;
                        if (!uomMap.ContainsKey(uomString))
                            uomMap.Add(uomString, 0);
                        uomMap[uomString] += detail.Qty;

                        totalQtyNoUoM += detail.Qty * detail.OrderDetail.UnitOfMeasure.Conversion;
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
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineProductName], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineProductName], startIndex, pName));
                        startIndex += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

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

                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineDetails], startIndex, qtyAsString, priceAsString, totalAsString));
                startIndex += font18Separation;

                string weights = "";

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

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncFullHorizontalLine], startIndex));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableTotalHeader], startIndex));
            startIndex += font18Separation;

            list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableTotals], startIndex, totalQtyNoUoM, ToString(balance)));
            startIndex += font18Separation;

            return list;
        }

        protected override IList<string> GetOrderDetailsRowsSplitProductName(string name)
        {
            return SplitProductName(name, 35, 35);
        }

        public override string ToString(double d)
        {
            return d.ToString("F");
        }

        protected override IEnumerable<string> GetTotalsRowsInOneDoc(ref int startY, Client client, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, InvoicePayment payment, Order order)
        {

            List<string> list = new List<string>();

            double salesBalance = 0;
            double creditBalance = 0;
            double returnBalance = 0;

            double totalSales = 0;
            double totalCredit = 0;
            double totalReturn = 0;

            double paid = 0;
            if (payment != null)
            {
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
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

                    x = double.Parse(Math.Round(x, Config.Round).ToCustomString(), NumberStyles.Currency);

                    salesBalance += x;

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

            if (!Config.HideTotalOrder && t == null)
            {
                s1 = ToString((salesBalance + creditBalance + returnBalance));
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncSubtotal], startY, s1));
                startY += font36Separation;

                double discount = order.CalculateDiscount();
                double tax = order.CalculateTax();

                s1 = ToString(salesBalance + creditBalance + returnBalance - discount + tax);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTotal], startY, s1));
                startY += font36Separation;
            }

            if (Config.PrintCopy)
            {
                string name = GetOrderPreorderLabel(order);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderPreorderLabel], startY, name));
                startY += font36Separation;
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

            //Printer Nice Line add PaidInFull

            double balance = order.OrderTotalCost();
            if (payment != null && payment.TotalPaid > 0)
            {
                var totalPaid = Math.Round(payment.Components.Sum(x => x.Amount), Config.Round);


                //var paidInFull = totalPaid == invoice.Balance;
                bool paidInFull = totalPaid == balance;

                if (paidInFull)
                {
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, CenterText("PAID IN FULL", 18)));
                    startY += font36Separation;
                }
                else
                {
                    s1 = ToString(totalPaid);
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitPartPayment], startY, s1));
                    startY += font36Separation;

                    s1 = ToString((balance - totalPaid));
                    list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitInvoiceOpen], startY, s1));
                    startY += font36Separation;
                }
            }
            else
            {
                s1 = ToString(balance);
                list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitInvoiceOpen], startY, s1));
                startY += font36Separation;
            }

            return list;
        }



        #region New Open Invoices

        public override bool PrintOpenInvoice(Invoice invoice)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, null);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            var ss1 = GetInvoiceType(invoice) + invoice.InvoiceNumber;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitInvoiceTitle], startY, ss1));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitInvoiceCopy], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarCreatedOn], startY, invoice.Date.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            if (invoice.DueDate < DateTime.Today)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOnOverdue], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceDueOn], startY, invoice.DueDate.ToString(Config.OrderDatePrintFormat)));
                startY += font18Separation;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedOn], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetFruitIncCompanyRows(ref startY));

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            Client client = invoice.Client;

            foreach (var clientSplit in GetClientNameSplit(client.ClientName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientName], startY, clientSplit));
                startY += font36Separation;
            }

            var custno = UDFHelper.ExplodeExtraProperties(client.ExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLowerInvariant() == "custno");
            var custNoString = string.Empty;
            if (custno != null)
            {
                custNoString = " " + custno.Value;
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientNameTo], startY, custNoString));
                startY += font36Separation;
            }

            foreach (string s in ClientAddress(client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderClientAddress], startY, s.Trim()));
                startY += font18Separation;
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

            string term = string.Empty;

            if (client.ExtraProperties != null)
            {
                var termsExtra = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                    term = termsExtra.Item2.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(term))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderTerms], startY, term));
                startY += font18Separation;
            }

            if (Config.PrintClientOpenBalance)
            {
                var balance = client.OpenBalance.ToCustomString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceClientBalance], startY, balance));
                startY += font18Separation;
            }

            startY += font36Separation;

            foreach (string commentPArt in GetOpenInvoiceCommentSplit(invoice.Comments ?? string.Empty))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                startY += font18Separation;
            }

            lines.AddRange(GetFruitInOpenInvoiceTable(ref startY, invoice));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected IEnumerable<string> GetFruitIncCompanyRows(ref int startIndex)
        {
            try
            {
                if (CompanyInfo.Companies.Count == 0)
                    return new List<string>();

                CompanyInfo company = CompanyInfo.GetMasterCompany();

                List<string> list = new List<string>();
                foreach (string part in CompanyNameSplit(company.CompanyName))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyName], startIndex, part));
                    startIndex += font36Separation;
                }

                startIndex += 10;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, company.CompanyAddress1));
                startIndex += font18Separation;

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, company.CompanyAddress2));
                    startIndex += font18Separation;
                }

                string phone = "Office Phone: " + company.CompanyPhone;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncCompanyAddress], startIndex, phone));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyFax))
                {
                    string fax = "Fax: " + company.CompanyFax;

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyFax], startIndex, company.CompanyFax));
                    startIndex += font18Separation;
                }

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    string email = "Email: " + company.CompanyEmail;

                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyEmail], startIndex, company.CompanyEmail));
                    startIndex += font18Separation;
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

        protected IEnumerable<string> GetFruitInOpenInvoiceTable(ref int startY, Invoice invoice)
        {
            List<string> lines = new List<string>();
            Product notFoundProduct = GetNotFoundInvoiceProduct();
            
            IQueryable<InvoiceDetail> source = SortDetails.SortedDetails(invoice.Details);

            double totalUnits = 0;
            double numberOfBoxes = 0;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncFullHorizontalLine], startY));
            startY += font18Separation;

            foreach (InvoiceDetail detail in source)
            {
                Product p = detail.Product;

                int productLineOffset = 0;
                foreach (string pName in GetOrderDetailsRowsSplitProductName(p.Name))
                {
                    if (productLineOffset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineProductName], startY, pName));
                        startY += font18Separation;
                    }
                    else if (!Config.PrintTruncateNames)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineProductName], startY, pName));
                        startY += font18Separation;
                    }
                    else
                        break;
                    productLineOffset++;
                }

                double d = detail.Quantity * detail.Price;
                double price = detail.Price;
                double package = 1;
                try
                {
                    package = Convert.ToSingle(detail.Product.Package, CultureInfo.InvariantCulture);
                }
                catch
                {
                }

                double units = detail.Quantity * package;
                totalUnits += units;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableLineDetails], startY,
                    detail.Quantity.ToString(),
                    price.ToCustomString(),
                    d.ToCustomString()
                    ));
                startY += font18Separation;

                lines.AddRange(GetUpcForProductIn(ref startY, p));

                if (!string.IsNullOrEmpty(detail.Comments.Trim()))
                {
                    foreach (string commentPArt in GetOrderDetailsRowsSplitProductName(detail.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InvoiceComment], startY, commentPArt));
                        startY += font18Separation;
                    }
                }

                startY += 10;
                numberOfBoxes += Convert.ToSingle(detail.Quantity);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncFullHorizontalLine], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableTotalHeader], startY));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FruitIncTableTotals], startY, numberOfBoxes, ToString(invoice.Amount)));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.AddRange(GetFruitIncInvoiceTotals(ref startY, invoice, numberOfBoxes, totalUnits));

            return lines;
        }
        
        protected IEnumerable<string> GetFruitIncInvoiceTotals(ref int startY, Invoice invoice, double numberOfBoxes, double totalUnits)
        {
            List<string> lines = new List<string>();

            string s1;
            
            if (invoice.Balance > 0)
            {
                InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null));

                var payments = existPayment != null ? existPayment.Components : null;
                if (payments != null && payments.Count > 0)
                {
                    var totalPaid = payments.Sum(x => x.Amount);

                    var paidInFull = totalPaid == invoice.Balance;

                    if (paidInFull)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, CenterText("PAID IN FULL", 18)));
                        startY += font36Separation;
                    }
                    else
                    {
                        s1 = ToString(totalPaid);
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitPartialPayment], startY, s1));
                        startY += font36Separation;

                        s1 = ToString((invoice.Balance - totalPaid));
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitOpen], startY, s1));
                        startY += font36Separation;
                    }
                }
                else
                {
                    s1 = ToString(invoice.Balance);
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitOpen], startY, s1));
                    startY += font36Separation;
                }
            }
            else if (invoice.Balance == 0)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CompanyName], startY, CenterText("PAID IN FULL", 18)));
                startY += font36Separation;
            }
            else
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FruitCredit], startY));
                startY += font36Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            return lines;
        }
        
        #endregion


    }
}