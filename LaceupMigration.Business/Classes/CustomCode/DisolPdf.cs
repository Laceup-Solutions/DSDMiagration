using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Globalization;
using System.IO;




using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.Kernel.Font;


namespace LaceupMigration
{
    public class DisolPdf : DefaultPdfProvider
    {
        #region General

        protected override void AddCompanyInfo(Document doc)
        {
            var company = CompanyInfo.Companies[0];

            AddTextLine(doc, company.CompanyName, GetBigFont());
            AddTextLine(doc, company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }

        #endregion

        #region Order

        protected override void AddOrderDetailsTable(Document doc, Order order)
        {
            float[] headers = { 12, 25, 7, 6, 15, 10, 15, 5 };  //Header Widths

            //Create PDF Table
            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout, "SKU");
            AddCellToHeader(tableLayout, "Description");
            AddCellToHeader(tableLayout, "UoM");
            AddCellToHeader(tableLayout, "Qty");
            AddCellToHeader(tableLayout, "Unit Price");
            AddCellToHeader(tableLayout, "Discount");
            AddCellToHeader(tableLayout, "Subtotal");
            AddCellToHeader(tableLayout, "Tax");

            float qtyBoxes = 0;

            if (order.Details != null)
            {
                // DETAILS
                foreach (OrderDetail detail in order.Details)
                {
                    Product product = detail.Product;

                    AddCellToBody(tableLayout, (product != null) ? product.Code : "");

                    var name = product != null ? product.Name : "";

                    if (!string.IsNullOrEmpty(detail.Comments))
                        name += "\n" + "Comment:" + detail.Comments;

                    AddCellToBody(tableLayout, name);
                    AddCellToBody(tableLayout, (detail.UnitOfMeasure != null) ? detail.UnitOfMeasure.Name : string.Empty);
                    AddCellToBody(tableLayout, detail.Qty.ToString());

                    int factor = 1;
                    string diff = "";
                    if (detail.IsCredit)
                    {
                        diff += "-";
                        factor = -1;
                    }

                    double discount = 0;
                    if (detail.Discount > 0)
                    {
                        if (detail.DiscountType == DiscountType.Amount)
                            discount = detail.Discount;
                        if (detail.DiscountType == DiscountType.Percent)
                        {
                            discount = detail.Price * detail.Qty * detail.Discount;
                        }
                    }

                    AddCellToBody(tableLayout, detail.Price.ToCustomString(), HorizontalAlignment.RIGHT);
                    AddCellToBody(tableLayout, discount.ToCustomString());

                    discount *= factor;

                    AddCellToBody(tableLayout, diff + (detail.Qty * detail.Price - discount).ToCustomString(), HorizontalAlignment.RIGHT);

                    double isv = detail.Taxed ? detail.TaxRate * 100 : 0;

                    AddCellToBody(tableLayout, detail.Product.Taxable ? isv + "%" : "E", HorizontalAlignment.CENTER);

                    qtyBoxes += detail.Qty;
                }
            }

            doc.Add(tableLayout);
        }

        protected override void AddFooterTable(Document doc, Order order)
        {
            float[] headers = { 45, 15, 10, 15, 15 };  //Header Widths

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            Cell signature = new Cell();
            var signatureText = new Paragraph("Customer Signature");
            signatureText.AddStyle(GetSmall8Font());
            signature.Add(signatureText);
            signature.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            tableLayout.AddCell(signature);

            AddCellToHeader(tableLayout, "Total Qty");
            AddCellToBody(tableLayout, order.Details.Sum(x => x.Qty).ToString());

            var discount = order.CalculateDiscount();
            var total = order.OrderTotalCost();
            var taxes = order.CalculateTax();
            var subtotal = total - taxes + discount;

            double subtotalTaxed = 0; //order.SubtotalTaxed();
            double subtotalNotTaxed = 0; //order.SubtotalNotTaxed();

            AddCellToHeader(tableLayout, "Subtotal Gravado");
            AddCellToBody(tableLayout, subtotalTaxed.ToCustomString(), HorizontalAlignment.RIGHT);

            tableLayout.AddCell(GetSignatureCell(order));

            AddCellToHeader(tableLayout, "Subtotal Excento");
            AddCellToBody(tableLayout, subtotalNotTaxed.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Descuento");
            AddCellToBody(tableLayout, discount.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Impuestos");
            AddCellToBody(tableLayout, taxes.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Gran Total");
            AddCellToBody(tableLayout, total.ToCustomString(), HorizontalAlignment.RIGHT);

            AddCellToHeader(tableLayout, "Payment");
            var payment = GetPayment(order);
            AddCellToBody(tableLayout, payment.ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout);

            float[] headers2 = { 20, 50, 15, 15 };  //Header Widths

            var tableLayout2 = new Table(UnitValue.CreatePercentArray(headers2)).UseAllAvailableWidth();

            AddCellToHeader(tableLayout2, "Receiver Name");
            AddCellToBody(tableLayout2, (!string.IsNullOrEmpty(order.SignatureName) ? order.SignatureName : ""), HorizontalAlignment.LEFT);

            AddCellToHeader(tableLayout2, "Total Due");
            AddCellToBody(tableLayout2, (total - payment).ToCustomString(), HorizontalAlignment.RIGHT);

            doc.Add(tableLayout2);

            Table footer = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();

            if (!string.IsNullOrEmpty(order.Comments))
            {
                Cell text1 = new Cell();
                var text1text = new Paragraph("Order Comment:" + order.Comments);
                text1text.AddStyle(GetSmall8Font());
                text1.Add(text1text);
                text1.SetHorizontalAlignment(HorizontalAlignment.LEFT);

                footer.AddCell(text1);
            }
            var text = new Cell();
            var texttext = new Paragraph("THANK YOU FOR YOUR PROMPT PAYMENT");
            texttext.AddStyle(GetSmall8Font());
            text.Add(texttext);
            text.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            footer.AddCell(text);

            doc.Add(footer);
        }

        #endregion
    }
}