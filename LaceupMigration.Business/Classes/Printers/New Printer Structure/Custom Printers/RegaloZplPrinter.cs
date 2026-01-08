using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class RegaloZplPrinter : ZebraFourInchesPrinter1
    {
        protected const string RegaloCompanyName = "RegaloCompanyName";
        protected const string RegaloCompanyInfo = "RegaloCompanyInfo";
        protected const string RegaloInvoiceInfo = "RegaloInvoiceInfo";
        protected const string RegaloCustomerName = "RegaloCustomerName";
        protected const string RegaloCustomerInfo = "RegaloCustomerInfo";
        protected const string RegaloSeparatorLine = "RegaloSeparatorLine";
        protected const string RegaloSectionName = "RegaloSectionName";
        protected const string RegaloTableHeader = "RegaloTableHeader";
        protected const string RegaloTableLine = "RegaloTableLine";
        protected const string RegaloTableTotals = "RegaloTableTotals";
        protected const string RegaloSalesPromo = "RegaloSalesPromo";
        protected const string RegaloReturnsCredits = "RegaloReturnsCredits";
        protected const string RegaloNetAmountTotalDue = "RegaloNetAmountTotalDue";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(RegaloCompanyName, "^CF0,50^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloCompanyInfo, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloInvoiceInfo, "^CF0,30^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloCustomerName, "^CF0,40^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloCustomerInfo, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloSeparatorLine, "^FO30,{0}^GB730,1,3^FS");
            linesTemplates.Add(RegaloSectionName, "^CF0,25^FO40,{0}^FD{1}^FS");
            linesTemplates.Add(RegaloTableHeader, "^CF0,25^FO40,{0}^FDUPC^FS" +
                "^FO200,{0}^FDDESCRIPTION^FS" +
                "^FO440,{0}^FDUNITS^FS" +
                "^FO520,{0}^FDPRICE^FS" +
                "^FO610,{0}^FDEXT. AMOUNT^FS");
            linesTemplates.Add(RegaloTableLine, "^CF0,25^FO40,{0}^FD{1}^FS" +
                "^FO200,{0}^FD{2}^FS" +
                "^FO440,{0}^FD{3}^FS" +
                "^FO520,{0}^FD{4}^FS" +
                "^FO610,{0}^FD{5}^FS");
            linesTemplates.Add(RegaloTableTotals, "^CF0,25^FO320,{0}^FDTOTAL:^FS" +
                "^FO440,{0}^FD{1}^FS" +
                "^FO610,{0}^FD{2}^FS");
            linesTemplates.Add(RegaloSalesPromo, "^CF0,35^FO40,{0}^FDSALES:^FS" +
                "^FO260,{0}^FD{1}^FS" +
                "^FO420,{0}^FDPROMO:^FS" +
                "^FO620,{0}^FD{2}^FS");
            linesTemplates.Add(RegaloReturnsCredits, "^CF0,35^FO40,{0}^FDRETURNS:^FS" +
                "^FO260,{0}^FD{1}^FS" +
                "^FO420,{0}^FDCREDITS:^FS" +
                "^FO620,{0}^FD{2}^FS");
            linesTemplates.Add(RegaloNetAmountTotalDue, "^CF0,35^FO40,{0}^FDNET AMOUNT:^FS" +
                "^FO260,{0}^FD{1}^FS" +
                "^FO420,{0}^FDTOTAL DUE:^FS" +
                "^FO620,{0}^FD{2}^FS");
        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            string printedId = order.PrintedOrderId;

            double balance = order.OrderTotalCost();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            AddExtraSpace(ref startY, lines, 36, 1);

            CompanyInfo company = null;

            if (CompanyInfo.Companies.Count > 0)
            {
                company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyId == order.CompanyId);
                if (company == null)
                    company = CompanyInfo.Companies.FirstOrDefault(x => x.CompanyName == order.CompanyName);
                if (company == null)
                    company = CompanyInfo.GetMasterCompany();

                if (order != null && !string.IsNullOrEmpty(order.CompanyName))
                    company.CompanyName = order.CompanyName;

                lines.Add(string.Format(linesTemplates[RegaloCompanyName], startY, company.CompanyName));
                startY += 60;

                if (!string.IsNullOrEmpty(company.CompanyAddress1))
                {
                    lines.Add(string.Format(linesTemplates[RegaloCompanyInfo], startY, company.CompanyAddress1));
                    startY += 30;
                }

                if (string.IsNullOrEmpty(company.CompanyAddress2))
                {
                    lines.Add(string.Format(linesTemplates[RegaloCompanyInfo], startY, company.CompanyAddress2));
                    startY += 30;
                }
                lines.Add(string.Format(linesTemplates[RegaloCompanyInfo], startY, "Phone: " + company.CompanyPhone));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloCompanyInfo], startY, "Email: " + company.CompanyEmail));
                startY += 30;
            }

            startY += 20;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == order.SalesmanId);

            lines.Add(string.Format(linesTemplates[RegaloInvoiceInfo], startY, "Dist. Sales Rep: " + (salesman != null ? salesman.Name : "")));
            startY += 30;

            var printedDate = DateTime.Now;

            lines.Add(string.Format(linesTemplates[RegaloInvoiceInfo], startY, "Prt Date: " + printedDate.ToString(CultureInfo.InvariantCulture)));
            startY += 30;

            startY += 20;

            var customerName = "Cust: " + order.Client.ClientName;

            foreach (var item in SplitProductName(customerName, 36, 36))
            {
                lines.Add(string.Format(linesTemplates[RegaloCustomerName], startY, item));
                startY += 40;
            }

            foreach (string s in ClientAddress(order.Client))
            {
                lines.Add(string.Format(linesTemplates[RegaloCustomerInfo], startY, s));
                startY += 30;
            }

            startY += 20;

            lines.Add(string.Format(linesTemplates[RegaloInvoiceInfo], startY,
                string.Format("Invoice #: {0}   Inv. Date: {1}", order.PrintedOrderId ?? "", order.Date.ToShortDateString())));
            startY += 30;

            lines.Add(string.Format(linesTemplates[RegaloInvoiceInfo], startY,
                string.Format("PO #: {0}", order.PONumber ?? "")));
            startY += 30;

            lines.Add(string.Format(linesTemplates[RegaloInvoiceInfo], startY,
                string.Format("A/R Type: {0}   Vendor No: {1}", order.Term ?? "", order.Client.VendorNumber ?? "")));
            startY += 30;

            startY += 20;
            
            double totalSales = 0;
            double totalCredits = 0;
            double totalReturs = 0;

            if (salesLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> sslines;

                if (Config.UseDraggableTemplate)
                    sslines = SortDetails.SortedDetails(order.Client.ClientId, salesLines.Values.ToList());
                else
                    sslines = SortDetails.SortedDetails(salesLines.Values.ToList());

                var listXX = sslines.ToList();
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

                lines.Add(string.Format(linesTemplates[RegaloSectionName], startY, "SALES SECTION"));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;

                lines.Add(string.Format(linesTemplates[RegaloTableHeader], startY));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;
                
                totalSales = AddDetailsLines(ref startY, lines, listXX, 1);

                startY += 20;
            }

            if (creditLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> crlines;

                if (Config.UseDraggableTemplate)
                    crlines = SortDetails.SortedDetails(order.Client.ClientId, creditLines.Values.ToList());
                else
                    crlines = SortDetails.SortedDetails(creditLines.Values.ToList());

                lines.Add(string.Format(linesTemplates[RegaloSectionName], startY, "CREDITS SECTION"));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;

                lines.Add(string.Format(linesTemplates[RegaloTableHeader], startY));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;

                totalCredits = AddDetailsLines(ref startY, lines, crlines.ToList(), -1);

                lines.Add("");
            }

            if (returnsLines.Keys.Count > 0)
            {
                IQueryable<OrderLine> rtlines;

                if (Config.UseDraggableTemplate)
                    rtlines = SortDetails.SortedDetails(order.Client.ClientId, returnsLines.Values.ToList());
                else
                    rtlines = SortDetails.SortedDetails(returnsLines.Values.ToList());

                lines.Add(string.Format(linesTemplates[RegaloSectionName], startY, "RETURNS SECTION"));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;

                lines.Add(string.Format(linesTemplates[RegaloTableHeader], startY));
                startY += 30;

                lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
                startY += 20;

                totalReturs = AddDetailsLines(ref startY, lines, rtlines.ToList(), -1);

                lines.Add("");
            }

            double paid = 0;
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            if (payment != null)
            {
                var parts = PaymentSplit.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }
            var discount = order.CalculateDiscount();
            var tax = order.CalculateTax();

            var s4 = totalSales + totalReturs + totalCredits - discount + tax;

            string totalSalesString = totalSales.ToCustomString();

            string totalRetursString = totalReturs.ToCustomString();
            
            string netAmountString = (totalSales + totalReturs + totalCredits).ToCustomString();
            
            lines.Add(string.Format(linesTemplates[RegaloSalesPromo], startY, totalSalesString, discount.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(linesTemplates[RegaloReturnsCredits], startY, totalRetursString, totalCredits.ToCustomString()));
            startY += 40;

            lines.Add(string.Format(linesTemplates[RegaloNetAmountTotalDue], startY, netAmountString, order.OrderTotalCost().ToCustomString()));
            startY += 40;

            startY += 10;

            if (Config.ExtraSpaceForSignature > 0)
                startY += Config.ExtraSpaceForSignature * font36Separation;

            // add the signature
            lines.AddRange(GetSignatureSection(ref startY, order, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        public double AddDetailsLines(ref int startY, List<string> lines, List<OrderLine> listXX, int factor)
        {
            float totalUnit = 0;
            double totalAmount = 0;

            foreach (var item in listXX)
            {
                string upc = item.Product.Upc;
                var pParts = SplitProductName(item.Product.Name, 18, 18);

                for (int i = 0; i < pParts.Count; i++)
                {
                    var description = pParts[i];
                    if (i == 0)
                    {
                        var qty = item.Qty;
                        if (item.UoM != null)
                            qty *= item.UoM.Conversion;
                        var price = item.Price;
                        var amount = item.Qty * item.Price * factor;

                        string qtyString = qty.ToString();
                        string priceString = price.ToCustomString();
                        string amountString = amount.ToCustomString();
                        
                        lines.Add(string.Format(linesTemplates[RegaloTableLine], startY, upc, description, qtyString, priceString, amountString));
                        startY += 30;

                        totalUnit += qty;
                        totalAmount += amount;
                    }
                    else
                    {
                        lines.Add(string.Format(linesTemplates[RegaloTableLine], startY, "", description, "", "", ""));
                        startY += 30;
                    }
                }

                startY += 10;
            }

            lines.Add(string.Format(linesTemplates[RegaloSeparatorLine], startY));
            startY += 20;

            string totalUnitString = totalUnit.ToString();
            if (totalUnitString.Length < 13)
                totalUnitString += new string(' ', 13 - totalUnitString.Length);

            lines.Add(string.Format(linesTemplates[RegaloTableTotals], startY, totalUnitString, totalAmount.ToCustomString()));
            startY += 30;

            return totalAmount;
        }
    }
}