using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class DisolIndendentPrinter : DisolPrinter
    {
        protected override List<string> GetDisolCompanyRows(Order order, bool asPreOrder, bool prestamo)
        {
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            List<string> lines = new List<string>();

            string companyName = CompanyInfo.Companies[0].CompanyName;
            string email = CompanyInfo.Companies[0].CompanyEmail;

            foreach (var part in CompanyNameSplit(companyName))
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], part.ToUpperInvariant()));
            
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "RTN", Config.RTN));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Correo", email));

            lines.Add(string.Empty);

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Domicilio Fiscal:"));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Principal:"));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], CompanyInfo.Companies[0].CompanyAddress1));
            if (!string.IsNullOrEmpty(CompanyInfo.Companies[0].CompanyAddress2))
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], CompanyInfo.Companies[0].CompanyAddress2));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Tel.", CompanyInfo.Companies[0].CompanyPhone));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Correo", CompanyInfo.Companies[0].CompanyEmail));

            lines.Add(string.Empty);

            if (salesman != null)
            {
                var branch = Branch.List.FirstOrDefault(x => x.Id == salesman.BranchId);
                if (branch != null)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Suc: 1"));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], branch.Name));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], branch.Address1));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], branch.Address2));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], branch.City + " " + branch.State));

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Tel.", branch.Phone));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Correo", branch.Email));

                    lines.Add(string.Empty);
                }
            }
            else
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Suc: 1"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Boulevard F.F. A.A., Centro Comercial"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Plaza San Pedro, Local 3Q"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Comayaguela, Honduras"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Tel: 2277-0084, Fax 2227-4288"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Correo: disoltgu@disol-sa.com"));
                lines.Add(string.Empty);
            }

            if (!prestamo)
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "\"Datos del adquiriente exonerado\""));
                lines.Add(string.Empty);

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "No. de orden compra exenta:"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "----------------------------------------"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "No. de registro exonerado:"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "----------------------------------------"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "No. de registro de la SAG:"));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "----------------------------------------"));
                lines.Add(string.Empty);
            }

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

            if (!prestamo)
            {
                if (!asPreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "Rango Autorizado:"));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Desde", prefix + sequenceFrom));
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Hasta", prefix + sequenceTo));
                }
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Cai", sequenceCai));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Fecha Limite", sequenceExpiration));
            }

            var vendorName = string.Empty;
            var presaleSalesman = Salesman.List.FirstOrDefault(x => x.Id == order.OriginalSalesmanId);
            if (presaleSalesman != null)
                vendorName = presaleSalesman.Name;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Vendedor", vendorName));

            string tipoVenta = order.Term;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Tipo Venta", tipoVenta));

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
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], docName));
                    lines.Add(string.Empty);
                }
            }
            else
            {
                if (Config.FakePreOrder)
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], "FACTURA FINAL"));
                    lines.Add(string.Empty);
                }
            }

            var invoiceNumber = order.PrintedOrderId;
            if (prestamo && !string.IsNullOrEmpty(invoiceNumber))
                invoiceNumber += "P";

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Num. Factura", invoiceNumber));
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Fecha de Factura", DateTime.Today.ToShortDateString()));

            var endDate = order.EndDate;
            if (order.EndDate.Year == 1)
                endDate = DateTime.Now;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Hora de Entrega", endDate.ToShortTimeString()));

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Camionero", salesmanName));

            var clientName = SplitProductName(order.Client.ClientName, 32, 40);

            var newClientName = UDFHelper.GetSingleUDF("newclientname", order.ExtraFields);
            var newRTN = UDFHelper.GetSingleUDF("rtn", order.ExtraFields);

            if (!string.IsNullOrEmpty(newClientName))
            {
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Cliente", newClientName));
                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "RTN", newRTN));
            }
            else
            {
                int offset = 0;
                foreach (var item in clientName)
                {
                    if (offset == 0)
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "Cliente", item));
                    else
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], item));

                    offset++;
                }

                string clientRtn = !string.IsNullOrEmpty(order.Client.LicenceNumber) ? order.Client.LicenceNumber : "Consumidor Final";

                lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[DisolFields], "RTN", clientRtn));
            }

            foreach (string s in ClientAddress(order.Client))
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[DisolText], s.Trim()));

            lines.Add(string.Empty);

            return lines;
        }
    }
}