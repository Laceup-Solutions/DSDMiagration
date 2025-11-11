using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkiaSharp;

namespace LaceupMigration
{
    public class ToltecaPrinter : ZebraFourInchesPrinter
    {
        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();
            // this will be the ID printed in the doc, it is the first doc of type OrderType.Order in the batch
            string printedId = null;

            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            printedId = order.PrintedOrderId;

            foreach (var od in order.Details)
            {
                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + od.Damaged.ToString();
                Dictionary<string, OrderLine> currentDic;
                if (!od.IsCredit)
                    currentDic = salesLines;
                else
                {
                    currentDic = creditLines;
                }
                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od });
                currentDic[key].Qty = currentDic[key].Qty + od.Qty;
            }

            List<string> lines = new List<string>();

            int startY = 80;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS^FO500,{0}^ADN,18,10^FD{3}^FS", startY, "Route #: ", Config.SalesmanId.ToString(), "Date: " + DateTime.Now.ToString()));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}{2}^FS", startY, "Salesman: ", Config.VendorName));
            startY += font18Separation;

            bool isTolteca = CompanyInfo.Companies.Count > 1 && CompanyInfo.Companies[0].CompanyName == CompanyInfo.Companies[1].CompanyName;
            string terms = string.Empty;

            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();
                }
            }

            if (isTolteca)
            {
                lines.AddRange(PrintToltecaHeaders(ref startY, asPreOrder, order));
            }
            else
            {
                if (terms != "CASH" && terms != "CHARGE")
                {
                    lines.AddRange(PrintToltecaHeaders(ref startY, asPreOrder, order));
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO0,{0}^ADN,18,10^FD------------------------------------------------------------------------^FS", startY));
                    startY += font18Separation;
                }
                lines.AddRange(PrintIndependendHeaders(ref startY, asPreOrder, order));
            }

            Tuple<string, string> vendorNumber = null;
            if (order.Client.ExtraProperties != null)
                vendorNumber = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR #");
            if (vendorNumber != null)
            {
                string s1233 = "Vendor #: " + vendorNumber.Item2;
                if (WidthForBoldFont - s1233.Length > 0)
                    s1233 = new string((char)32, (WidthForBoldFont - s1233.Length) / 2) + s1233;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s1233));
                startY += font36Separation;
            }

            string s1231 = "Invoice: " + printedId;
            if (WidthForBoldFont - s1231.Length > 0)
                s1231 = new string((char)32, (WidthForBoldFont - s1231.Length) / 2) + s1231;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s1231));
            startY += font36Separation;

            startY += font18Separation;
            if (!string.IsNullOrEmpty(terms))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FDID:    {1}^FS", startY, terms));
                startY += font18Separation;
            }
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FDBill to: {1}^FS", startY, order.Client.ClientName));
            startY += font18Separation;

            foreach (string s11 in ClientAddress(order.Client, false))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD          {1}^FS", startY, s11));
                startY += font18Separation;
            }

            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FDSold to: {1}^FS", startY, order.Client.ClientName));
            startY += font18Separation;

            foreach (string s11 in ClientAddress(order.Client, true))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD          {1}^FS", startY, s11));
                startY += font18Separation;
            }

            startY += font18Separation;

            if (!string.IsNullOrEmpty(terms))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FDTerms:    {1}^FS", startY, terms));
                startY += font18Separation;
            }
            startY += font18Separation;

            if (asPreOrder)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FDNOT AN INVOICE^FS", startY));
            }

            double balance = 0;
            int factor = 1;
            float qty = 0;
            string s;
            string format = "^FO50,{0}^ABN,18,10^FDQTY^FS" +
                                "^FO120,{0}^ABN,18,10^FDDescription^FS" +
                                "^FO600,{0}^ABN,18,10^FDPrice^FS" +
                                "^FO700,{0}^ABN,18,10^FDAMT.^FS";
            if (salesLines.Count > 0)
            {
                startY += font18Separation;
                s = "SALES";
                startY += font18Separation;
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s));
                startY += font36Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, format, startY));
                startY += font18Separation + 10;

                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO120,{0}^ABN,18,10^FD{1}^FS", startY, "UPC"));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO0,{0}^ADN,18,10^FD------------------------------------------------------------------------^FS", startY));
                startY += font18Separation;

                format = "^FO50,{0}^ADN,18,10^FD{1}^FS" +
                         "^FO120,{0}^ADN,18,10^FD{2}^FS" +
                         "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                         "^FO700,{0}^ADN,18,10^FD{4}^FS";

                double totalDiscount = 0;

                foreach (var detail in SortDetails.SortedDetails(salesLines.Values.ToList()))
                {
                    Product p = detail.Product;

                    int productLineOffset = 0;
                    string name = p.Name.Replace(p.Code, string.Empty);

                    double price = detail.Price;
                    double regularprice = Product.GetPriceForProduct(detail.Product, order.Client, false);
                    var discount = price - regularprice;
                    double d = detail.Qty * price;

                    qty += detail.Qty;
                    balance += d;
                    totalDiscount += Math.Abs(discount) * detail.Qty;

                    foreach (string pName in GetDetailsRowsSplitProductName1(name))
                    {
                        if (productLineOffset == 0)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, format, startY, detail.Qty, pName, price.ToCustomString(), d.ToCustomString()));
                            startY += font18Separation;
                        }
                        else
                            break;
                        productLineOffset++;
                    }

                    if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, "^FO120,{0}^BCN,30,Y,N,N,N^FD{1}^FS", startY, p.Upc));
                        startY += 60;
                    }

                    if(discount != 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, "^FO120,{0}^ADN,18,10^FD{1} Discount -{2}^FS", startY, regularprice.ToCustomString(), Math.Abs(discount).ToCustomString()));
                        startY += font18Separation;
                    }

                    startY += 10;
                }
                
                if(totalDiscount > 0)
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO380,{0}^ADN,18,10^FDTotal Promotion Discount:^FS^FO700,{0}^ADN,18,10^FD{1}^FS", startY, totalDiscount.ToCustomString()));
                    startY += font18Separation;
                }

                if (qty > 0)
                {
                    s = new string('-', (WidthForNormalFont));
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO50,{0}^ADN,18,10^FD{1}^FS", startY, s));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO50,{0}^ADN,18,10^FD{1}^FS", startY, qty.ToString()));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO500,{0}^ADN,18,10^FDSales Subtotal:^FS^FO700,{0}^ADN,18,10^FD{1}^FS", startY, balance.ToCustomString()));
                    startY += font36Separation;
                }
            }
            /* CREDIT SECTION*/
            qty = 0;
            if (creditLines.Values.Count > 0)
            {
                startY += font18Separation;
                s = "DAMAGED & RETURNS";
                startY += font18Separation;
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s));
                startY += font36Separation;
                balance = 0;
                factor = -1;

                format = "^FO50,{0}^ABN,18,10^FDQTY^FS" +
                                "^FO120,{0}^ABN,18,10^FDDescription^FS" +
                                "^FO600,{0}^ABN,18,10^FDPrice^FS" +
                                "^FO700,{0}^ABN,18,10^FDAMT.^FS";

                lines.Add(String.Format(CultureInfo.InvariantCulture, format, startY));
                startY += font18Separation + 10;

                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO120,{0}^ABN,18,10^FD{1}^FS", startY, "UPC"));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO0,{0}^ADN,18,10^FD------------------------------------------------------------------------^FS", startY));
                startY += font18Separation;

                format = "^FO50,{0}^ADN,18,10^FD{1}^FS" +
                         "^FO120,{0}^ADN,18,10^FD{2}^FS" +
                         "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                         "^FO700,{0}^ADN,18,10^FD{4}^FS";
                foreach (var detail in SortDetails.SortedDetails(creditLines.Values.ToList()))
                {
                    Product p = detail.Product;

                    int productLineOffset = 0;
                    string name = p.Name.Replace(p.Code, string.Empty);
                    foreach (string pName in GetDetailsRowsSplitProductName1(name))
                    {
                        if (productLineOffset == 0)
                        {
                            double d = detail.Qty * detail.Price * factor;
                            double price = detail.Price * factor;
                            balance = balance + d;
                            qty = qty + detail.Qty;
                            lines.Add(string.Format(CultureInfo.InvariantCulture, format, startY, detail.Qty, pName, price.ToCustomString(), d.ToCustomString()));
                            startY += font18Separation;
                        }
                        else
                            break;
                        productLineOffset++;
                    }

                    if (p.Upc.Trim().Length > 0 & Config.PrintUPC)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, "^FO120,{0}^BCN,30,Y,N,N,N^FD{1}^FS", startY, p.Upc));
                        startY += 60;
                    }

                    lines.Add(string.Format(CultureInfo.InvariantCulture, "^FO120,{0}^ADN,18,10^FD{1}^FS", startY, detail.OrderDetail.Damaged ? "Damaged" : "Return"));
                    startY += font18Separation;

                    // space for next line
                    startY += font18Separation;

                }
                if (qty > 0)
                {
                    s = new string('-', (WidthForNormalFont));
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO50,{0}^ADN,18,10^FD{1}^FS", startY, s));
                    startY += font18Separation;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO50,{0}^ADN,18,10^FD{1}^FS", startY, qty.ToString()));
                    startY += font18Separation;

                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO360,{0}^ADN,18,10^FDDamaged & Returns Subtotal:^FS^FO700,{0}^ADN,18,10^FD{1}^FS", startY, balance.ToCustomString()));
                    startY += font36Separation;
                }

                startY += font36Separation;
            }

            if (asPreOrder)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FDNOT AN INVOICE^FS", startY));
            }
            startY += font36Separation * 2;

            if (order.DiscountAmount > 0)
            {
                if (order.DiscountType == DiscountType.Amount)
                {
                    s = "^FO240,{0}^ADN,36,20^FDDISCOUNT: ^FS^FO550,{0}^ADN,36,20^FD{1}^FS";
                    string s12 = order.CalculateDiscount().ToCustomString();
                    if (10 - s12.Length > 0)
                        s12 = new string((char)32, (10 - s12.Length)) + s12;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, s12));
                    startY += font36Separation;
                }
                else
                {
                    s = "^FO240,{0}^ADN,36,20^FDDISCOUNT {2}: ^FS^FO550,{0}^ADN,36,20^FD{1}^FS";
                    string s12 = order.CalculateDiscount().ToCustomString();
                    if (10 - s12.Length > 0)
                        s12 = new string((char)32, (10 - s12.Length)) + s12;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, s12, (order.DiscountAmount * 100).ToString() + "%"));
                    startY += font36Separation;
                }
            }

            s = "^FO40,{0}^ADN,36,20^FD{1}^FS^FO240,{0}^ADN,36,20^FDINVOICE TOTAL: ^FS^FO550,{0}^ADN,36,20^FD{2}^FS";
            string s1 = order.OrderTotalCost().ToCustomString();
            if (10 - s1.Length > 0)
                s1 = new string((char)32, (10 - s1.Length)) + s1;

            var payments = InvoicePayment.List.Where(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.IndexOf(order.UniqueId) >= 0).ToList();
            if (payments.Count > 0 && payments[0].PaymentMethods().Contains(InvoicePaymentMethod.Check.ToString()))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, "Check #: " + payments[0].CheckNumbers()));
                startY += font36Separation;
            }

            //if (terms != null && terms.ToUpperInvariant() == "CASH")
            //    lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, "CASH", s1));
            //else
            //    lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, "CHARGE", s1));
            //startY += font36Separation;

            if (payments.Count > 0)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, "CASH", s1));
                startY += font36Separation;

                var amount = payments.Sum(x => x.Components.Sum(y => y.Amount));
                s = "^FO240,{0}^ADN,36,20^FDPAYMENT TOTAL: ^FS^FO550,{0}^ADN,36,20^FD{1}^FS";
                s1 = amount.ToCustomString();
                if (10 - s1.Length > 0)
                    s1 = new string((char)32, (10 - s1.Length)) + s1;
                lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, s1));
                startY += font36Separation;

                s = "^FO240,{0}^ADN,36,20^FD      BALANCE: ^FS^FO550,{0}^ADN,36,20^FD{1}^FS";
                s1 = (order.OrderTotalCost() - amount).ToCustomString();
                if (10 - s1.Length > 0)
                    s1 = new string((char)32, (10 - s1.Length)) + s1;
                lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, s1));
                startY += font36Separation;
            }
            else
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, s, startY, "CHARGE", s1));
                startY += font36Separation;
            }

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            {
                lines.Add(IncludeSignature(order, lines, ref startY));

                s = "--------------------------------------------";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;

                // signature name

                if (!string.IsNullOrEmpty(order.SignatureName))
                {
                    s = order.SignatureName;
                    if (WidthForNormalFont - s.Length > 0)
                        s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                    startY += font18Separation;
                }
                s = "CUSTOMER SIGNATURE";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }
            else
            {
                startY += font18Separation * 8;

                s = "--------------------------------------------";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;

                /*
				s = "(nombre de la persona que recive)";
				if (WidthForNormalFont - s.Length > 0)
					s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
				lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
				startY += font18Separation;
				*/
                s = "CUSTOMER SIGNATURE";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }

            bool independCashCharge = !isTolteca && (terms == "CASH" || terms == "CHARGE");
            if (!independCashCharge)
            {
                startY += font18Separation;
                s = "SEND PAYMENT TO: CANDIES TOLTECA";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
                s = "P.O.BOX 4729 FRESNO CA. 93744";
                if (WidthForNormalFont - s.Length > 0)
                    s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                startY += font18Separation;
            }

            s = "All past accounts will be billed a service charge at an annual";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = " rate of 18% (1.5% MONTHLY) or $55.00 dls minimun charge. ";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = "Also we may charge $25.00 dls if your check is returned";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = "for any reason.";
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;

            startY += font18Separation;
            if (!string.IsNullOrEmpty(Config.BottomOrderPrintText))
            {
                foreach (var line in SplitProductName(Config.BottomOrderPrintText, 29, 29))
                {
                    s = line;
                    if (WidthForBoldFont - s.Length > 0)
                        s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s));
                    startY += font36Separation;
                }
            }

            lines.Add(EndLabel);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, StartLabel, startY + 60));
            StringBuilder sb = new StringBuilder();
            foreach (string s2 in lines)
                sb.Append(s2);

            try
            {
                string s2 = sb.ToString();
                DateTime st = DateTime.Now;
                PrintIt(s2);
                Logger.CreateLog(" print took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
                return true;
            }
            catch (Exception eee)
            {
                Logger.CreateLog(eee);
                //Xamarin.Insights.Report(eee);
                return false;
            }
        }

        protected IList<string> PrintIndependendHeaders(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();
            startY += 10;

            string s = CompanyInfo.Companies[1].CompanyName;

            bool printedName = false;
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (terms != null)
                {

                    if (terms.Item2.ToUpper() != "CASH" && terms.Item2.ToUpper() != "CHARGE" && CompanyInfo.Companies[0].CompanyName != CompanyInfo.Companies[1].CompanyName)
                    {
                        if (WidthForNormalFont - s.Length > 0)
                            s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
                        startY += font18Separation;
                        printedName = true;
                    }
                }
            }
            if (!printedName)
            {
                if (WidthForBoldFont - s.Length > 0)
                    s = new string((char)32, (WidthForBoldFont - s.Length) / 2) + s;
                lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,36,20^FD{1}^FS", startY, s));
                startY += font36Separation;
            }

            s = CompanyInfo.Companies[1].CompanyAddress1 + " " + CompanyInfo.Companies[1].CompanyAddress2;
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;
            s = CompanyInfo.Companies[1].CompanyPhone;
            if (WidthForNormalFont - s.Length > 0)
                s = new string((char)32, (WidthForNormalFont - s.Length) / 2) + s;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD{1}^FS", startY, s));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD             AUTHORIZED EXCLUSIVE DISTRIBUTOR OF^FS", startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD                  CANDIES TOLTECA PRODUCTS^FS", startY));
            startY += font18Separation;

            startY += 36;

            return lines;
        }

        protected IList<string> PrintToltecaHeaders(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                string label = "^FO30," + startY.ToString() + "^GFA, " +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoSize.ToString() + "," +
                               Config.CompanyLogoWidth.ToString() + "," +
                               Config.CompanyLogo;
                lines.Add(label);
                startY += Config.CompanyLogoHeight;
            }

            startY += 36;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO45,{0}^ADN,36,20^FD       {1}^FS", startY, CompanyInfo.Companies[0].CompanyName));
            startY += font36Separation + font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO45,{0}^ADN,18,10^FD                {1} {2}^FS", startY, CompanyInfo.Companies[0].CompanyAddress1, CompanyInfo.Companies[0].CompanyAddress2));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO40,{0}^ADN,18,10^FD             Toll Free: {1}^FS", startY, CompanyInfo.Companies[0].CompanyPhone));
            startY += 2 * font18Separation;

            return lines;
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
            double widthInBytes = ((signature.Width / 32) * 32) / 8;
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

        double GetDiscount(Order order, double price, double factor)
        {
            return Math.Round(price * order.DiscountAmount * factor, 2);
        }
    }
}

