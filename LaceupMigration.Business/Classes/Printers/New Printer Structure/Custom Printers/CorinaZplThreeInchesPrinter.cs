using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class CorinaZplThreeInchesPrinter : ZebraSpanishThreeInchesPrinter
    {
        protected const string CorinaCompanyName = "CorinaCompanyName";
        protected const string CorinaDomicilioFiscal = "CorinaDomicilioFiscal";
        protected const string CorinaDomicilioFiscal1 = "CorinaDomicilioFiscal1";
        protected const string CorinaFields = "CorinaFields";
        protected const string CorinaText = "CorinaText";
        protected const string CorinaTableHeader = "CorinaTableHeader";
        protected const string CorinaTableLine = "CorinaTableLine";
        protected const string CorinaTotals = "CorinaTotals";

        protected const string CorinaOrderCreatedReportTableHeader1 = "CorinaOrderCreatedReportTableHeader1";
        protected const string CorinaOrderCreatedReportTableHeader2 = "CorinaOrderCreatedReportTableHeader2";
        protected const string CorinaOrderCreatedReportTableLine = "CorinaOrderCreatedReportTableLine";

        protected const string CorinaPaymentReportTableHeader = "CorinaPaymentReportTableHeader";
        protected const string CorinaPaymentReportTableHeader1 = "CorinaPaymentReportTableHeader1";
        protected const string CorinaPaymentReportTableLine = "CorinaPaymentReportTableLine";

        public CorinaZplThreeInchesPrinter() : base()
        {
            Config.OrderDatePrintFormat = "dd/MM/yy h:mm tt";
        }

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(CorinaCompanyName, "^FO15,{0}^ADN,28,14^FD{1}^FS");
            linesTemplates.Add(CorinaDomicilioFiscal, "^FO15,{0}^ADN,18,10^FDDomicilio Fiscal:^FS");
            linesTemplates.Add(CorinaDomicilioFiscal1, "^FO15,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(CorinaFields, "^FO15,{0}^ADN,18,10^FD{1}: {2}^FS");
            linesTemplates.Add(CorinaText, "^FO15,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(CorinaTableHeader,
                "^FO15,{0}^ADN,18,10^FDProducto^" +
                "FS^FO210,{0}^ADN,18,10^FDCant^" +
                "FS^FO265,{0}^ADN,18,10^FDPrecio^" +
                "FS^FO360,{0}^ADN,18,10^FDDesc.^" +
                "FS^FO430,{0}^ADN,18,10^FDTotal^" +
                "FS^FO530,{0}^ADN,18,10^FDISV^FS");

            linesTemplates.Add(CorinaTableLine,
                "^FO15,{0}^ADN,18,10^FD{1}^" +
                "FS^FO210,{0}^ADN,18,10^FD{2}^" +
                "FS^FO265,{0}^ADN,18,10^FD{3}^" +
                "FS^FO360,{0}^ADN,18,10^FD{4}^" +
                "FS^FO430,{0}^ADN,18,10^FD{5}^" +
                "FS^FO540,{0}^ADN,18,10^FD{6}^FS");

            linesTemplates.Add(CorinaTotals, "^FO50,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(CorinaOrderCreatedReportTableHeader1, "^FO15,{0}^ABN,18,10^FDNOMBRE^FS" +
                "^FO270,{0}^ABN,18,10^FD^FS" +
                "^FO320,{0}^ABN,18,10^FD^FS" +
                "^FO380,{0}^ABN,18,10^FD^FS" +
                "^FO480,{0}^ABN,18,10^FD^FS");

            linesTemplates.Add(CorinaOrderCreatedReportTableHeader2, "^FO15,{0}^ABN,18,10^FDRECIBO #.^FS" +
                "^FO270,{0}^ABN,18,10^FDEST^FS" +
                "^FO320,{0}^ABN,18,10^FDCANT^FS" +
                "^FO380,{0}^ABN,18,10^FDTOTAL^FS" +
                "^FO480,{0}^ABN,18,10^FDCS TP^FS");

            linesTemplates.Add(CorinaOrderCreatedReportTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO270,{0}^ABN,18,10^FD{2}^FS" +
                "^FO320,{0}^ABN,18,10^FD{3}^FS" +
                "^FO380,{0}^ABN,18,10^FD{4}^FS" +
                "^FO480,{0}^ABN,18,10^FD{5}^FS");

            linesTemplates.Add(CorinaPaymentReportTableHeader, "^FO15,{0}^ABN,18,10^FDNombre^FS" +
                "^FO260,{0}^ABN,18,10^FD^FS" +
                "^FO350,{0}^ABN,18,10^FDFact Total^FS" +
                "^FO470,{0}^ABN,18,10^FDMonto^FS");
            linesTemplates.Add(CorinaPaymentReportTableHeader1, "^FO15,{0}^ABN,18,10^FDFactura #^FS" +
                "^FO260,{0}^ABN,18,10^FDMetodo^FS" +
                "^FO350,{0}^ABN,18,10^FDNumero Ref^FS" +
                "^FO470,{0}^ABN,18,10^FD^FS");
            linesTemplates.Add(CorinaPaymentReportTableLine, "^FO15,{0}^ABN,18,10^FD{1}^FS" +
                "^FO260,{0}^ABN,18,10^FD{2}^FS" +
                "^FO350,{0}^ABN,18,10^FD{3}^FS" +
                "^FO470,{0}^ABN,18,10^FD{4}^FS");
        }

        protected override IList<string> CompanyNameSplit(string name)
        {
            return SplitProductName(name, 22, 22);
        }

        #region Print Order

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            startY += 36;

            lines.AddRange(GetCompanyRows(ref startY, order));

            startY += font18Separation;

            var invoiceHeader = "Num. Factura";
            if (order.OrderType == OrderType.Credit)
                invoiceHeader = "Nota de Credito";

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, invoiceHeader, order.PrintedOrderId));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, "Fecha de Factura", order.Date.ToShortDateString()));
            startY += font18Separation;

            var endDate = order.EndDate;
            if (order.EndDate.Year == 1)
                endDate = DateTime.Now;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, "Hora de Entrega", endDate.ToShortTimeString()));
            startY += font18Separation;

            string salesmanName = string.Empty;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
                salesmanName = salesman.Name;

            var clientName = SplitProductName(order.Client.ClientName, 32, 40);

            int offset = 0;
            foreach (var item in clientName)
            {
                if (offset == 0)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, "Cliente", item));
                else
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, item));

                startY += font18Separation;
                offset++;
            }

            string rtn = "Consumidor Final";
            var rtnUdf = order.Client.NonVisibleExtraProperties != null ? order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "rtn") : null;
            if (rtnUdf != null && !string.IsNullOrEmpty(rtnUdf.Item2))
                rtn = rtnUdf.Item2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, "RTN", rtn));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startY, "Direccion", ""));
            startY += font18Separation;

            foreach (string s in ClientAddress(order.Client))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s.Trim()));
                startY += font18Separation;
            }

            startY += font18Separation;

            string docName = "NO ES UNA FACTURA";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                    docName = "NO ES UNA CUENTA";
            }

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, docName));
            }
            else
            {
                if (Config.FakePreOrder)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "FACTURA FINAL"));
            }
            startY += font18Separation;

            lines.AddRange(GetTable(ref startY, order));

            lines.AddRange(GetTotals(ref startY, order));

            lines.AddRange(GetFooter(ref startY, order, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            if (!PrintLines(lines))
                return false;


            return true;
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
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaCompanyName], startIndex, part));
                    startIndex += font36Separation;
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "RTN", Config.RTN));
                startIndex += font18Separation;

                if (!string.IsNullOrEmpty(company.CompanyEmail))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Email", company.CompanyEmail));
                    startIndex += font18Separation;
                }

                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaDomicilioFiscal], startIndex));
                startIndex += font18Separation;

                foreach (var item in SplitProductName(company.CompanyAddress1, 46, 46))
                {
                    list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaDomicilioFiscal1], startIndex, item));
                    startIndex += font18Separation;
                }

                if (company.CompanyAddress2.Trim().Length > 0)
                {
                    foreach (var item in SplitProductName(company.CompanyAddress2, 46, 46))
                    {
                        list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaDomicilioFiscal1], startIndex, item));
                        startIndex += font18Separation;
                    }
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Tel.", company.CompanyPhone));
                startIndex += font18Separation;

                startIndex += font18Separation;

                var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
                string sequenceFrom = string.Empty;
                string sequenceTo = string.Empty;
                string sequenceExpiration = string.Empty;
                string sequenceCai = string.Empty;
                if (salesman != null)
                {
                    sequenceFrom = salesman.SequenceFrom.ToString();
                    sequenceTo = salesman.SequenceTo.ToString();

                    if (!string.IsNullOrEmpty(salesman.ExtraProperties))
                    {
                        int qtyDigits = 0;
                        if (order.AsPresale)
                        {
                            var pre = UDFHelper.ExplodeExtraProperties(salesman.ExtraProperties).FirstOrDefault(x => x.Key == "PresaleFormatQtyDigits");
                            if (pre != null)
                                qtyDigits = Convert.ToInt32(pre.Value);
                        }
                        else
                        {
                            var pre = UDFHelper.ExplodeExtraProperties(salesman.ExtraProperties).FirstOrDefault(x => x.Key == "DSDFormatQtyDigits");
                            if (pre != null)
                                qtyDigits = Convert.ToInt32(pre.Value);
                        }

                        if (qtyDigits > 0)
                        {
                            sequenceFrom = sequenceFrom.PadLeft(qtyDigits, '0');
                            sequenceTo = sequenceTo.PadLeft(qtyDigits, '0');
                        }
                    }

                    if (salesman.SequenceExpirationDate.HasValue)
                        sequenceExpiration = salesman.SequenceExpirationDate.Value.ToShortDateString();
                    sequenceCai = salesman.SequenceCAI;
                }

                var prefix = salesman.SequencePrefix ?? string.Empty;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startIndex, "Rango Autorizado:"));
                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Desde", prefix + sequenceFrom));
                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Hasta", prefix + sequenceTo));
                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Cai", sequenceCai));
                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Fecha Limite De Emision", sequenceExpiration));
                startIndex += font18Separation;

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Vendedor", Config.VendorName));
                startIndex += font18Separation;

                string tipoVenta = string.Empty;

                if (order.Client.ExtraProperties != null)
                {
                    var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                    if (termsExtra != null)
                    {
                        var terms = termsExtra.Item2.ToUpperInvariant();
                        tipoVenta = terms;
                    }
                }

                list.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaFields], startIndex, "Tipo Venta", "Contado"));
                startIndex += font18Separation;

                return list;
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                throw;
            }
        }

        IEnumerable<string> GetTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "Descripcion"));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var item in order.Details)
            {
                var productSlices = SplitProductName(item.Product.Name, 15, 46);

                var price = Math.Round(item.Price, Config.Round);
                var total = Math.Round((item.Price * item.Qty), Config.Round);

                var qtyToString = item.Qty.ToString(CultureInfo.InvariantCulture);


                var isvString = item.Product.Taxable && order.Client.TaxRate > 0 ? order.Client.TaxRate.ToString() : "E";

                var discountString = string.Empty;
                var discountTotal = string.Empty;


                if (item.DiscountType == DiscountType.Percent)
                {
                    if (item.Discount == 1)
                        price = 0;
                }
                if (item.DiscountType == DiscountType.Amount)
                {
                    if (item.Discount == price)
                        price = 0;
                }

                if (price == 0)
                {
                    discountString = "100%";
                    price = Math.Round(Product.GetPriceForProduct(item.Product, order.Client, true, false), 4);
                    total = Math.Round((price * item.Qty), Config.Round);
                }

                var priceString = ToString(price);
                var totalString = ToString(total);

                int offSet = 0;
                foreach (var part in productSlices)
                {
                    if (offSet == 0)
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaTableLine], startY, part, qtyToString, priceString, discountString, totalString, isvString));
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaTableLine], startY, part, "", "", "", "", ""));

                    startY += font18Separation;
                    offSet++;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font36Separation;

            return lines;
        }

        IEnumerable<string> GetTotals(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            double subtotal = 0;
            double subtotalE = 0;
            double totalTax = 0;

            double addedDiscounts = 0;

            foreach (var item in order.Details)
            {
                if (item.IsFreeItem || item.Price == 0)
                {
                    var regularPrice = Product.GetPriceForProduct(item.Product, order.Client, true, false);
                    addedDiscounts += (regularPrice * item.Qty);
                }

                if (!item.Taxed)
                {
                    subtotalE += item.QtyPrice;
                    continue;
                }

                subtotal += item.QtyPrice;
                totalTax += item.QtyPrice * order.TaxRate;
            }

            var discount = order.CalculateDiscount();
            string s;
            string s1;

            s = "Descuentos y Rebajas L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(Math.Abs(discount + addedDiscounts));
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            s = "              Exento L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(subtotalE);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            s = "           Exonerado L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = "";
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            s = "             Gravado L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(subtotal);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            s = "           Sub-Total L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(subtotal + subtotalE);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            s = "          15% I.S.V. L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(totalTax);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            double granTotal = subtotal + subtotalE - discount + totalTax;

            s = "                Total L. ";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(granTotal);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
            startY += font18Separation;

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var result = GetPaymentLines(payments);
                if (!string.IsNullOrEmpty(result))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, result));
                    startY += font18Separation;
                }
            }
            double paid = 0;
            foreach (var p in payments)
                paid += p.Amount;
            if (paid > 0)
            {
                s = "                 Balance:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                s1 = ToString(granTotal - paid);
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s + s1));
                startY += font18Separation;
            }

            startY += font36Separation;

            granTotal = Math.Round(granTotal, 2);

            //revisar esto
            double decimals = (granTotal - Math.Truncate(granTotal)) * 100;

            var text = "Son: " + MyExtensions.ToText((int)granTotal) + " Lempiras y " + MyExtensions.ToText((int)decimals) + " centavos.";

            foreach (var item in SplitProductName(text, 46, 46))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, item));
                startY += font18Separation;
            }

            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "Orden de Compra Excenta No:_______"));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "Constancia de Registro de Exonerado No:_______"));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "Registro Sector Agroindustrial No:_______"));
            startY += font18Separation;

            startY += font18Separation;

            return lines;
        }

        public override string ToString(double d)
        {
            var s = Math.Round(d, 2).ToString();

            if (!s.Contains('.'))
                s += ".00";

            if (d < 0)
                return "(L" + s + ")";

            return "L" + s;
        }

        string GetPaymentLines(IList<PaymentSplit> payments)
        {
            if (payments.Count == 1)
            {
                string s;
                string s1;

                switch (payments[0].PaymentMethod)
                {
                    case InvoicePaymentMethod.Cash:
                        s = "Pago En Efectivo:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(payments[0].Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        return s + s1;
                    case InvoicePaymentMethod.Check:
                        s = "Pago En Cheque:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(payments[0].Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        return s + s1;
                    case InvoicePaymentMethod.Money_Order:
                        s = "Pago Money Order:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(payments[0].Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        return s + s1;
                    case InvoicePaymentMethod.Credit_Card:
                        s = "Pago Credito:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(payments[0].Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        return s + s1;
                }
            }

            return string.Empty;
        }

        IEnumerable<string> GetFooter(ref int startY, Order order, bool asPreorder)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, "La factura es beneficio de todos. Exijala."));
            startY += font36Separation;

            string s = "Original del Cliente";
            if (asPreorder)
                s = "No es una factura";
            else if (order.PrintedCopies > 0)
                s = "Copia #: " + order.PrintedCopies;

            var halfSpace = (62 - s.Length) / 2;

            s = new string(' ', halfSpace) + s + new string(' ', halfSpace);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, s));
            startY += font36Separation;

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var result = GetPaymentLines(payments);
                if (!string.IsNullOrEmpty(result))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaText], startY, result));
                    startY += font18Separation;
                }
            }

            startY += font36Separation;

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
                startY += 50;
            }
            else
            {
                startY += 100;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
                startY += 12;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
                startY += 100;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSpaceSignatureText], startY));
                startY += 50;
            }

            return lines;
        }

        #endregion

        #region Order Created Report

        protected override IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaOrderCreatedReportTableHeader1], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaOrderCreatedReportTableHeader2], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int voided = 0;
            int reshipped = 0;
            int delivered = 0;
            int dsd = 0;
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            double cashTotalTerm = 0;
            double chargeTotalTerm = 0;
            double subtotal = 0;
            double totalTax = 0;

            double paidTotal = 0;
            double chargeTotal = 0;
            double creditTotal = 0;
            double salesTotal = 0;
            double billTotal = 0;

            var payments = GetPaymentsForOrderCreatedReport();

            foreach (var order in Order.Orders.Where(x => !x.Reshipped))
            {
                var terms = order.Term.Trim();

                switch (order.OrderType)
                {
                    case OrderType.Bill:
                        break;
                    case OrderType.Credit:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Load:
                        break;
                    case OrderType.NoService:
                        break;
                    case OrderType.Order:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                    case OrderType.Return:
                        if (!string.IsNullOrEmpty(terms))
                        {
                            if (terms == "COD" || terms == "C.O.D." || terms == "CASH" || terms == "DUE ON RECEIPT"
                                || terms == "CASH ON DELIVERY" || terms == "CONTADO" || terms == "EFECTIVO")
                                cashTotalTerm += order.OrderTotalCost();
                            else
                                chargeTotalTerm += order.OrderTotalCost();
                        }
                        else
                            chargeTotalTerm += order.OrderTotalCost();

                        subtotal += order.CalculateItemCost();
                        totalTax += order.CalculateTax();

                        break;
                }
            }

            foreach (var b in Batch.List.OrderBy(x => x.ClockedIn))
                foreach (var p in b.Orders())
                {
                    if (!Config.PrintNoServiceInSalesReports && p.OrderType == OrderType.NoService)
                        continue;

                    var orderCost = p.OrderTotalCost();

                    string totalCostLine = ToString(orderCost);
                    string subTotalCostLine = totalCostLine;

                    int productLineOffset = 0;
                    foreach (string pName in SplitProductName(p.Client.ClientName, WidthForNormalFont, WidthForNormalFont))
                    {
                        if (productLineOffset == 0)
                        {
                            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaOrderCreatedReportTableLine], startY,
                                                    pName,
                                                    "", "", "", ""));
                            startY += font18Separation;
                            break;
                        }
                    }

                    string status = GetCreatedOrderStatus(p);

                    double paid = 0;

                    var payment = payments.FirstOrDefault(x => x.UniqueId == p.UniqueId);
                    if (payment != null)
                    {
                        double amount = payment.Amount;
                        paid = double.Parse(Math.Round(amount, Config.Round).ToCustomString(), NumberStyles.Currency);
                    }

                    string type = GetCreatedOrderType(p, paid, orderCost);

                    if (!p.Reshipped && !p.Voided)
                    {
                        if (orderCost < 0)
                            creditTotal += orderCost;
                        else
                        {
                            if (p.OrderType != OrderType.Bill)
                            {
                                salesTotal += orderCost;

                                if (paid == 0)
                                    chargeTotal += orderCost;
                                else
                                {
                                    paidTotal += paid;
                                    chargeTotal += orderCost - paid;
                                }
                            }
                            else
                                billTotal += orderCost;
                        }
                    }
                    else
                        type = string.Empty;

                    float qty = 0;
                    foreach (var item in p.Details)
                        if (!item.SkipDetailQty(p))
                            qty += item.Qty;

                    if (Config.SalesRegReportWithTax)
                        subTotalCostLine = ToString(p.CalculateItemCost());
                    else
                        totalCostLine = string.Empty;

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaOrderCreatedReportTableLine], startY,
                                            p.PrintedOrderId,
                                            status,
                                            qty.ToString(),
                                            subTotalCostLine,
                                            type));
                    startY += font18Separation;

                    if (productLineOffset == 1 && !string.IsNullOrEmpty(totalCostLine))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaOrderCreatedReportTableLine], startY,
                                                    string.Empty, "", "", totalCostLine, ""));
                        startY += font18Separation;
                    }

                    AddExtraSpace(ref startY, lines, 10, 1);

                    if (!string.IsNullOrEmpty(p.Term))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableTerms], startY,
                        p.Term));
                        startY += font18Separation;
                    }

                    var batch = Batch.List.FirstOrDefault(x => x.Id == p.BatchId);

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLine1], startY,
                        batch.ClockedIn.ToShortTimeString(), batch.ClockedOut.ToShortTimeString(), p.PrintedCopies));
                    startY += font18Separation;

                    if (p.OrderType == OrderType.NoService && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLineComment], startY, p.Comments));
                        startY += font18Separation;
                    }

                    if (p.Reshipped && !string.IsNullOrEmpty(p.Comments))
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableLineComment1], startY, p.Comments));
                        startY += font18Separation;
                    }


                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedIn < start)
                        start = batch.ClockedIn;
                    if (batch.ClockedIn.Year != DateTime.MinValue.Year && batch.ClockedOut > end)
                        end = batch.ClockedOut;
                    if (p.Voided)
                        voided++;
                    if (p.Reshipped)
                        reshipped++;
                    if (RouteEx.Routes.Exists(x => x.Order != null && x.Order.UniqueId == p.UniqueId))
                        delivered++;
                    else
                        dsd++;

                    AddExtraSpace(ref startY, lines, font18Separation, 1);
                }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font36Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditTotal, startY, "", ToString(creditTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportBillTotal, startY, "", ToString(billTotal)));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportSalesTotal, startY, "", ToString(salesTotal)));
            startY += font18Separation;

            if (Config.SalesRegReportWithTax)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportSubtotal], startY, ToString(subtotal)));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTax], startY, ToString(totalTax)));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTotals], startY, ToString((paidTotal + chargeTotal))));
                startY += font18Separation;
            }

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportExpectedCash, startY, ToString(cashTotalTerm), reshipped.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportPaidCust, startY, ToString(paidTotal), voided.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportChargeCust, startY, ToString(chargeTotal), delivered.ToString()));
            startY += font18Separation;

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportCreditCust, startY, "", dsd.ToString()));
            startY += font18Separation;



            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);

            lines.Add(GetOrderCreatedReportTotalsFixed(OrderCreatedReportFullTotal, startY, ToString(salesTotal), totalTime));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }

        #endregion

        #region Payment Report

        protected override IEnumerable<string> GetPaymentsReportTable(ref int startY, List<PaymentRow> rows)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaPaymentReportTableHeader], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaPaymentReportTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in rows)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaPaymentReportTableLine], startY,
                                p.ClientName,
                                "",
                                p.DocAmount,
                                p.Paid));
                startY += font18Separation;
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[CorinaPaymentReportTableLine], startY,
                                p.DocNumber,
                                p.PaymentMethod,
                                p.RefNumber,
                                ""));
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
        }

        #endregion
    }
}