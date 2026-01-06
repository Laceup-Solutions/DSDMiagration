using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class HondurasPrinter : TextSpanishThreeInchesPrinter
    {
        protected const string HondurasText = "HondurasText";
        protected const string HondurasFields = "HondurasFields";
        protected const string HondurasHeaderTable = "HondurasHeaderTable";
        protected const string HondurasHeaderLine = "HondurasHeaderLine";
        protected const string HondurasTotals = "HondurasTotals";
        protected const string HondurasCenterText = "HondurasCenterText";
        protected const string HondurasSignature = "HondurasSignature";

        protected const string NewHondurasHeaderTable = "NewHondurasHeaderTable";
        protected const string NewHondurasHeaderLine = "NewHondurasHeaderLine";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(HondurasText, "{0}");
            linesTemplates.Add(HondurasFields, "{0}: {1}");
            linesTemplates.Add(HondurasHeaderTable, "Producto        Cant Precio    Total       ISV");
            linesTemplates.Add(HondurasHeaderLine, "{0} {1} {2} {3} {4}");
            linesTemplates.Add(HondurasTotals, "{0}");
            linesTemplates.Add(HondurasCenterText, "{0}{1}{2}");
            linesTemplates.Add(HondurasSignature, "Recibo Conforme: _________________________");

            linesTemplates[OrderCreatedReportTableHeader] = "NOMBRE                               EST  CANT\n" +
                                                            "RECIBO #                TOTAL        CS TP";
            linesTemplates[OrderCreatedReportTableLine] = "{1} {2} {3}\n" +
                                                            "{4} {5} {6}";

            linesTemplates[PaymentReportTableHeader] = "Nombre                      Factura #";
            linesTemplates[PaymentReportTableHeader1] = "Fact Total  Pagado      Metodo # Ref";
            linesTemplates[PaymentReportTableLine] = "{1} {2}\n" +
                                                     "{3} {4} {5} {6}";

            linesTemplates[NewHondurasHeaderTable] = "CODIGO      DESCRIPCION\n" +
                                                  "UDM   CANT  PRECIO    DESCUENTO ISV TOTAL";
            linesTemplates[NewHondurasHeaderLine] = "{0} {1}\n" +
                                                 "{2} {3} {4} {5} {6} {7}";
        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();

            if (asPreOrder)
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "                   PROFORMA"));
            else
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "                    FACTURA"));

            lines.AddRange(GetCompanyRows(order, asPreOrder));

            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();

            FillOrderDictionaries(order, salesLines, creditLines, returnsLines);

            int startY = 80;

            lines.AddRange(GetDetailsRowsInOneDoc(ref startY, asPreOrder, salesLines, creditLines, returnsLines, order));

            AddExtraSpace(ref startY, lines, 18, 1);

            lines.AddRange(GetTotals(order));

            lines.AddRange(GetFooter(order, asPreOrder));


            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], 60));

            return PrintLines(lines);
        }

        List<string> GetCompanyRows(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            string companyName = CompanyInfo.Companies[0].CompanyName;
            string email = CompanyInfo.Companies[0].CompanyEmail;

            foreach (var part in CompanyNameSplit(companyName))
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], part.ToUpperInvariant()));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "RTN", Config.RTN));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Correo", email));

            lines.Add(string.Empty);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "Domicilio Fiscal:"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "Principal:"));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], CompanyInfo.Companies[0].CompanyAddress1));
            if (!string.IsNullOrEmpty(CompanyInfo.Companies[0].CompanyAddress2))
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], CompanyInfo.Companies[0].CompanyAddress2));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Tel.", CompanyInfo.Companies[0].CompanyPhone));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Correo", CompanyInfo.Companies[0].CompanyEmail));

            lines.Add(string.Empty);

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
            {
                var branch = Branch.List.FirstOrDefault(x => x.Id == salesman.BranchId);
                if (branch != null && branch.Name != "Default Branch")
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "Sucursal #1"));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], branch.Name));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], branch.Address1));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], branch.Address2));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], branch.City + " " + branch.State + ", Honduras"));

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Tel.", branch.Phone));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Correo", branch.Email));

                    lines.Add(string.Empty);
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "\"Datos del adquiriente exonerado\""));
            lines.Add(string.Empty);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "No. de orden compra exenta:"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "----------------------------------------"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "No. de registro exonerado:"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "----------------------------------------"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "No. de registro de la SAG:"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "----------------------------------------"));
            lines.Add(string.Empty);

            string sequenceFrom = string.Empty;
            string sequenceTo = string.Empty;
            string sequenceExpiration = string.Empty;
            string sequenceCai = string.Empty;
            string salesmanName = string.Empty;

            if (salesman != null)
            {
                salesmanName = salesman.Name;

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

            if (!asPreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "Rango Autorizado:"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Desde", prefix + sequenceFrom));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Hasta", prefix + sequenceTo));
            }
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Cai", sequenceCai));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Fecha Limite", sequenceExpiration));

            var vendorName = string.Empty;
            var presaleSalesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
            if (presaleSalesman != null)
                vendorName = presaleSalesman.Name;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Vendedor", vendorName));

            string tipoVenta = order.Term;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Tipo Venta", tipoVenta));

            lines.Add(string.Empty);

            string docName = "PROFORMA";
            if (order.Client.ExtraProperties != null)
            {
                var vendor = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                    docName = "NO ES UNA CUENTA";
            }

            if (asPreOrder)
            {
                if (!Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], docName));
                    lines.Add(string.Empty);
                }
            }
            else
            {
                if (Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "FACTURA FINAL"));
                    lines.Add(string.Empty);
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Num. Factura", order.PrintedOrderId));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Fecha de Factura", DateTime.Today.ToShortDateString()));

            var endDate = order.EndDate;
            if (order.EndDate.Year == 1)
                endDate = DateTime.Now;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Hora de Entrega", endDate.ToShortTimeString()));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Camionero", salesmanName));

            var clientName = SplitProductName(order.Client.ClientName, 32, 40);

            int offset = 0;
            foreach (var item in clientName)
            {
                if (offset == 0)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Cliente", item));
                else
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], item));

                offset++;
            }

            string clientRtn = !string.IsNullOrEmpty(order.Client.LicenceNumber) ? order.Client.LicenceNumber : "Consumidor Final";

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "RTN", clientRtn));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Direccion", ""));

            foreach (string s in ClientAddress(order.Client))
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], s.Trim()));

            lines.Add(string.Empty);

            return lines;
        }

        protected override List<string> GetOrderDetailsSectionTotal(ref int startIndex, Order order, Dictionary<string, float> uomMap, float totalQtyNoUoM, string sectionName, float totalUnits, double balance)
        {
            List<string> list = new List<string>();

            // print total of the section
            Tuple<string, string> t = null;
            if (order.Client.ExtraProperties != null)
                t = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1 == "HIDETOTAL");

            if (!Config.HideTotalOrder && t == null)
            {
                if (uomMap.Keys.Count > 0)
                {
                    if (totalQtyNoUoM > 0)
                        uomMap.Add(string.Empty, totalQtyNoUoM);
                    uomMap.Add("Unidades:", uomMap.Values.Sum(x => x));
                }
                else
                {
                    float _totalUnits = 0;
                    float _totalsWeight = 0;
                    float _totalQty = 0;

                    foreach (var item in order.Details)
                    {
                        if (item.SkipDetailQty(order))
                            continue;

                        if (item.Product.SoldByWeight && !order.AsPresale)
                        {
                            _totalsWeight += item.Weight;
                            _totalUnits++;
                        }
                        else
                        {
                            _totalQty += item.Qty;

                            int packaging = 0;

                            if (!string.IsNullOrEmpty(item.Product.Package))
                                int.TryParse(item.Product.Package, out packaging);

                            if (packaging > 0)
                                _totalUnits += item.Qty * packaging;
                        }
                    }

                    if(_totalsWeight > 0)
                        uomMap.Add("Total Lb:", _totalsWeight);
                    else
                        uomMap.Add("Totales:", _totalQty);
                    uomMap.Add("Unidades:", _totalUnits);
                }

                var uomKeys = uomMap.Keys.ToList();
                var balanceText = ToString(balance);

                if (!Config.HideTotalOrder && t == null)
                {
                    var key = uomKeys[0];
                    list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, uomMap[key].ToString(), balanceText));
                    startIndex += font18Separation;
                    uomKeys.Remove(key);
                }
                if (uomKeys.Count > 0)
                {
                    foreach (var key in uomKeys)
                    {
                        list.Add(GetOrderDetailsSectionTotalFixedLine(OrderDetailsTotals, startIndex, string.Empty, key, Math.Round(uomMap[key], Config.Round).ToString(CultureInfo.CurrentCulture), string.Empty));
                        startIndex += font18Separation;
                    }
                }
            }

            return list;
        }

        protected virtual List<string> GetTotals(Order order)
        {
            List<string> lines = new List<string>();

            double ventaBruta = 0;
            double descRebajas = 0;
            double importeExonerado = 0;
            double importeExento = 0;
            double importeGravado15 = 0;
            double importeGravado18 = 0;
            double isv15 = 0;
            double isv18 = 0;
            double isvE = 0;
            double totalPagar = 0;

            foreach (var item in order.Details)
            {
                var qty = item.Product.SoldByWeight && !order.AsPresale ? item.Weight : item.Qty;

                var line = qty * item.Price;

                if (!item.IsCredit)
                    ventaBruta += line;
                else
                    descRebajas += line;

                double discount = item.Discount;
                if (discount > 0)
                {
                    if (item.DiscountType == DiscountType.Amount)
                        discount *= qty;
                    else if (item.DiscountType == DiscountType.Percent)
                        discount *= line;
                }
                descRebajas += discount;

                if (item.TaxRate == 0)
                    isvE += line;
                else if (item.TaxRate == 15)
                    importeGravado15 += line;
                else if (item.TaxRate == 18)
                    importeGravado18 += line;
            }

            importeExento = ventaBruta - descRebajas;
            isv15 = importeGravado15 * .15;
            isv18 = importeGravado18 * .18;

            if (order.Client.Taxable && order.Client.TaxRate > 0)
            {
                if (isv15 == 0 && isv18 == 0)
                {
                    var totalTax = order.CalculateTax();
                    if (order.Client.TaxRate < .18)
                        isv15 = totalTax;
                    else
                        if (order.Client.TaxRate >= .18)
                        isv18 = totalTax;
                }
            }

            totalPagar = importeExento + isv15 + isv18;

            string s;
            string s1;

            s = "         VENTA BRUTA:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(ventaBruta);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = "DESCUENTOS Y REBAJAS:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(descRebajas);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = "   IMPORTE EXONERADO:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(importeExonerado);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = "      IMPORTE EXENTO:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(importeExento);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = " IMPORTE GRAVADO 15%:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(importeGravado15);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = " IMPORTE GRAVADO 18%:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(importeGravado18);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = "             ISV 15%:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(isv15);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            s = "             ISV 18%:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(isv18);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            //s = "               ISV E:";
            //s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            //s1 = ToString(isvE);
            //s1 = new string(' ', 14 - s1.Length) + s1;
            //lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolTotals], s + s1));

            s = "       TOTAL A PAGAR:";
            s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
            s1 = ToString(totalPagar);
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], s + s1));

            lines.Add(string.Empty);


            //revisar esto
            double decimals = (totalPagar - Math.Truncate(totalPagar)) * 100;

            var text = "Son: " + MyExtensions.ToText((int)totalPagar) + " Lempiras y " + MyExtensions.ToText((int)decimals) + " centavos.";

            foreach (var item in SplitProductName(text, 48, 48))
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], item));

            lines.Add(string.Empty);

            return lines;
        }

        protected virtual List<string> GetFooter(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            if (!asPreOrder)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "La factura es beneficio de todos. Exijala."));
                lines.Add(string.Empty);

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], "ORIGINAL-CLIENTE    COPIA 1-OBLIGADO TRIBUTARIO EMISOR"));
                lines.Add(string.Empty);

                string s = "FACTURA ORIGINAL";
                if (order.PrintedCopies > 0)
                    s = "FACTURA - COPIA " + (order.PrintedCopies);

                var halfSpace = (62 - s.Length) / 2;

                s = new string(' ', halfSpace) + s + new string(' ', halfSpace);

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText], s));
                lines.Add(string.Empty);
            }

            var payments = PaymentSplit.SplitPayment(InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId))).Where(x => x.UniqueId == order.UniqueId).ToList();
            if (payments != null && payments.Count > 0)
            {
                var result = GetPaymentLines(payments, order.OrderTotalCost());

                foreach (var item in result)
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasTotals], item));
            }

            lines.Add(string.Empty);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasSignature]));
            lines.Add(string.Empty);

            return lines;
        }

        List<string> GetPaymentLines(IList<PaymentSplit> payments, double totalCost)
        {
            List<string> lines = new List<string>();

            var paid = payments.Sum(x => x.Amount);

            foreach (var item in payments)
            {
                string s;
                string s1;

                switch (item.PaymentMethod)
                {
                    case InvoicePaymentMethod.Cash:
                        s = "Pago En Efectivo:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                    case InvoicePaymentMethod.Check:
                        s = "Pago En Cheque:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                    case InvoicePaymentMethod.Money_Order:
                        s = "Pago Money Order:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                    case InvoicePaymentMethod.Credit_Card:
                        s = "Pago Credito:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                    case InvoicePaymentMethod.Transfer:
                        s = "Pago En Transferencia:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                    case InvoicePaymentMethod.Zelle_Transfer:
                        s = "Pago En Transferencia Zelle:";
                        s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                        s1 = ToString(item.Amount);
                        s1 = new string(' ', 14 - s1.Length) + s1;
                        lines.Add(s + s1);
                        break;
                }
            }

            if (paid < totalCost)
            {
                var s = "Saldo Pendiente:";
                s = new string(' ', WidthForBoldFont - s.Length - SpaceForOrderFooter) + s;
                var s1 = ToString(totalCost - paid);
                s1 = new string(' ', 14 - s1.Length) + s1;
                lines.Add(s + s1);
            }

            return lines;
        }


        public override string ToString(double d)
        {
            return d.ToCustomString();

            var s = Math.Round(d, 2).ToString();

            if (!s.Contains('.'))
                s += ".00";

            if (d < 0)
                return "(L" + s + ")";

            return "L" + s;
        }


        #region Order Created Report

        protected override IEnumerable<string> GetOrderCreatedReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportHeader], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintedDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            string shipment = "";
            string truckName = "";

            if (Shipment.CurrentShipment != null)
            {
                shipment = Shipment.CurrentShipment.Id.ToString();
                truckName = Shipment.CurrentShipment.TruckName;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Num Viaje", shipment));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Camion", truckName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            if (Config.UseClockInOut)
            {
                #region Deprecated

                //DateTime startOfDay = Config.FirstDayClockIn;
                //TimeSpan tsio = Config.WorkDay;
                //DateTime lastClockOut = Config.DayClockOut;
                //var wholeday = lastClockOut.Subtract(startOfDay);
                //var rested = wholeday.Subtract(tsio);

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporWorkDay], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), tsio.Hours, tsio.Minutes));
                //startY += font18Separation;

                //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporBreaks], startY, rested.Hours, rested.Minutes));
                //startY += font18Separation;

                #endregion

                DateTime startOfDay = SalesmanSession.GetFirstClockIn();
                DateTime lastClockOut = SalesmanSession.GetLastClockOut();
                var wholeday = lastClockOut.Subtract(startOfDay);
                var breaks = SalesmanSession.GetTotalBreaks();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporWorkDay], startY, startOfDay.ToShortTimeString(), lastClockOut.ToShortTimeString(), wholeday.Hours, wholeday.Minutes));
                startY += font18Separation;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReporBreaks], startY, breaks.Hours, breaks.Minutes));
                startY += font18Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetOrderCreatedReportTable(ref int startY)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderCreatedReportTableHeader], startY));
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
                if (!Config.IncludePresaleInSalesReport && order.AsPresale)
                    continue;

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

                    if (!Config.IncludePresaleInSalesReport && p.AsPresale)
                        continue;

                    var orderCost = p.OrderTotalCost();

                    string totalCostLine = ToString(orderCost);
                    string subTotalCostLine = totalCostLine;

                    int productLineOffset = 0;
                    foreach (string pName in GetOrderCreatedReportRowSplitProductName(p.Client.ClientName))
                    {
                        if (productLineOffset == 0)
                        {
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
                                        salesTotal += orderCost;
                                    else
                                        billTotal += orderCost;

                                    if (paid == 0)
                                        chargeTotal += orderCost;
                                    else
                                    {
                                        paidTotal += paid;
                                        chargeTotal += orderCost - paid;
                                    }
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

                            lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    pName,
                                                    status,
                                                    qty.ToString(),
                                                    p.PrintedOrderId,
                                                    subTotalCostLine,
                                                    type));
                        }
                        else
                            break;

                        if (Config.SalesRegReportWithTax)
                            lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    "", "", "", "", totalCostLine, ""));

                        productLineOffset++;
                        startY += font18Separation;
                    }

                    if (productLineOffset == 1 && !string.IsNullOrEmpty(totalCostLine))
                    {
                        lines.Add(GetOrderCreatedReportTableLineFixed(OrderCreatedReportTableLine, startY,
                                                    string.Empty, "", "", "", totalCostLine, ""));
                        startY += font18Separation;
                    }

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

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Reenviado", reshipped.ToString()));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Entregado", delivered.ToString()));
            startY += font18Separation;

            AddExtraSpace(ref startY, lines, font18Separation, 1);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "  Total de Credito", ToString(creditTotal)));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "   Total de Ventas", ToString(salesTotal)));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "CL Efect. Esperado", ToString(cashTotalTerm)));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "         CL Pagado", ToString(paidTotal)));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "        CL Cargado", ToString(chargeTotal)));
            startY += font18Separation;

            var ts = end.Subtract(start);
            string totalTime;
            if (ts.Minutes > 0)
                totalTime = String.Format("{0}h {1}m", ts.Hours, ts.Minutes);
            else
                totalTime = String.Format("{0}h", ts.Hours);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Tiempo (Hs)", totalTime));
            startY += font36Separation;

            AddExtraSpace(ref startY, lines, font36Separation, 3);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureLine], startY));
            startY += font18Separation;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[FooterSignatureText], startY));
            startY += font36Separation;

            return lines;
        }

        protected override IEnumerable<string> GetOrderCreatedReportRowSplitProductName(string name)
        {
            return SplitProductName(name, 35, 35);
        }

        protected override string GetOrderCreatedReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 35)
                v1 += new string(' ', 35 - v1.Length);

            if (v2.Length < 5)
                v2 += new string(' ', 5 - v2.Length);

            if (v3.Length < 4)
                v3 += new string(' ', 4 - v3.Length);

            if (v4.Length < 23)
                v4 += new string(' ', 23 - v4.Length);

            if (v5.Length < 12)
                v5 += new string(' ', 12 - v5.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Payment Report

        protected override IEnumerable<string> GetPaymentReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportHeader], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            string shipment = "";
            string truckName = "";

            if (Shipment.CurrentShipment != null)
            {
                shipment = Shipment.CurrentShipment.Id.ToString();
                truckName = Shipment.CurrentShipment.TruckName;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Num Viaje", shipment));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Camion", truckName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetPaymentsReportTable(ref int startY, List<PaymentRow> rows)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader], startY));
            startY += font18Separation;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[PaymentReportTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in rows)
            {
                lines.Add(GetPaymentReportTableLineFixed(PaymentReportTableLine, startY,
                                p.ClientName.Substring(0, p.ClientName.Length < 27 ? p.ClientName.Length : 27),
                                p.DocNumber,
                                p.DocAmount,
                                p.Paid,
                                p.PaymentMethod,
                                p.RefNumber));
                startY += font18Separation;

                //AddExtraSpace(ref startY, lines, font18Separation, 1);
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            return lines;
        }

        protected new string GetPaymentReportTableLineFixed(string format, int pos, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            if (v1.Length < 27)
                v1 += new string(' ', 27 - v1.Length);

            if (v3.Length < 11)
                v3 += new string(' ', 11 - v3.Length);

            if (v4.Length < 11)
                v4 += new string(' ', 11 - v4.Length);

            if (v5.Length < 6)
                v5 += new string(' ', 6 - v5.Length);

            return string.Format(CultureInfo.InvariantCulture, linesTemplates[format], pos,
                v1, v2, v3, v4, v5, v6);
        }

        #endregion

        #region Inventory Settlement Report

        protected override List<string> GetSettlementReportHeader(ref int startY, int index, int count)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementHeader], startY));
            startY += font36Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDate], startY, DateTime.Now.ToString(Config.OrderDatePrintFormat)));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintRouteNumber], startY, Config.RouteName));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[StandarPrintDriverName], startY, Config.VendorName));
            startY += font18Separation;

            string shipment = "";
            string truckName = "";

            if (Shipment.CurrentShipment != null)
            {
                shipment = Shipment.CurrentShipment.Id.ToString();
                truckName = Shipment.CurrentShipment.TruckName;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Num Viaje", shipment));
            startY += font18Separation;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasFields], "Camion", truckName));
            startY += font18Separation;

            if (!Config.HideCompanyInfoPrint)
            {
                //Add the company details rows.
                lines.AddRange(GetCompanyRows(ref startY));
                startY += font18Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetSettlementReportTable(ref int startY, List<InventorySettlementRow> map, InventorySettlementRow totalRow)
        {
            List<string> lines = new List<string>();

            var oldRound = Config.Round;
            Config.Round = 2;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[InventorySettlementTableHeader1], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            foreach (var p in SortDetails.SortedDetails(map))
            {
                if (p.BegInv == 0 && p.LoadOut == 0 && p.Adj == 0 && (p.TransferOn - p.TransferOff) == 0
                    && p.Sales == 0 && p.CreditReturns == 0 && p.CreditDump == 0 && p.DamagedInTruck == 0 && p.Unload == 0
                    && p.EndInventory == 0)
                    continue;

                if (Config.ShortInventorySettlement && string.IsNullOrEmpty(p.OverShort) && p.TransferOn == 0 && p.TransferOff == 0 && p.Adj == 0)
                    continue;

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[HondurasText],
                    p.Product.Name.Substring(0, p.Product.Name.Length < WidthForNormalFont ? p.Product.Name.Length : WidthForNormalFont)));
                startY += font18Separation;

                lines.Add(GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                p.Lot,
                                                p.UoM != null ? p.UoM.OriginalId : string.Empty,
                                                Math.Round(p.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.TransferOn - p.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty));
                startY += font18Separation;

                lines.Add(GetInventorySettlementTableLineFixed(InventorySettlementTableLine, startY,
                                                string.Empty,
                                                Math.Round(p.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(p.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                p.OverShort,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty));
                startY += font18Separation;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            if (map.All(x => x.UoM == null))
            {
                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals1, startY,
                                                string.Empty,
                                                Math.Round(totalRow.BegInv, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.LoadOut, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Adj, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.TransferOn - totalRow.TransferOff, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                Math.Round(totalRow.Sales, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty,
                                                string.Empty));
                startY += font18Separation;

                lines.Add(GetInventorySettlementTableTotalsFixed(InventorySettlementTableTotals, startY,
                                                    string.Empty,
                                                    Math.Round(totalRow.CreditReturns, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.CreditDump, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.DamagedInTruck, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.Unload, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    Math.Round(totalRow.EndInventory, Config.Round).ToString(CultureInfo.CurrentCulture),
                                                    totalRow.OverShort,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty));
                startY += font18Separation;
            }

            Config.Round = oldRound;

            return lines;
        }

        #endregion
    }
}