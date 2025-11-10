using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;




namespace LaceupMigration
{
    public class FromHtmlPdf : IPdfProvider
    {
        #region Invoice HTML

        public const string InvoiceHtmlBody = @"<html>
<head>
</head>
<body>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""50""></td>
            <td width=""550"" align=""left""> <h2 style=""text-align:left"">[COMPANY_NAME]</h2></td>
        </tr>
    </table>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""50""></td>
            <td width=""320"" align=""left"">
                <p style=""text-align:left"">
                    <span style=""color:#069"">
                        <span style=""font-family:arial,helvetica,sans-serif"">
                            [COMPANY_ADDRESS]<br>
                            [COMPANY_PHONE]<br>
                        </span>
                    </span>
                </p>
            </td>
            <td width=""260"" align=""right"">
                <table>
                    <tr>
                        <td align=""right"">[ORDER_HEADER]</td>
                        <td align=""left"">[INVOICE_NO]</td>
                    </tr>
                    <tr>
                        <td align=""right"">Date: </td>
                        <td align=""left"">[INVOICE_DATE]</td>
                    </tr>
                    <tr>
                        <td align=""right"">Due Date: </td>
                        <td align=""left"">[INVOICE_DUE_DATE]</td>
                    </tr>
                </table>
            </td>
			<td  width=""20""></td>
        </tr>
    </table>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""50"" valign=""top"">Bill to:</td>
            <td width=""549"" align=""left""><h2 style=""text-align:left"">[CLIENT_NAME]</h2></td>
            <td width=""1""></td>
        </tr>
    </table>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""50""></td>
            <td width=""380"" align=""left"">
                <p style=""text-align:left"">
                    <span style=""color:#069"">
                        <span style=""font-family:arial,helvetica,sans-serif"">
                            [CLIENT_ADDRESS]<br>
                            [CLIENT_PHONE]<br>
                        </span>
                    </span>
                </p>
            </td>
            <td width=""170"" align=""right""></td>
        </tr>
    </table>

    <p></p><p></p><p></p>

    <table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 600px; font-weight: normal; font-size: small; "">
        <tr>
            <td width=""30"">
                <h3>No</h3>
            </td>
            <td width=""300"">
                <h3>Product</h3>
            </td>
            <td width=""100"">
                <h3>Upc</h3>
            </td>
            <td width=""50"" align=""right"">
                <h3>Qty</h3>
            </td>
            <td width=""60"" align=""right"">
                <h3>Price</h3>
            </td>
            <td width=""60"" align=""right"">
                <h3>Total</h3>
            </td>
        </tr>
        [DETAIL_ROWS]
        <tr>
            <td width=""30""><br></td>
            <td width=""300""><br></td>
            <td width=""100""><br></td>
            <td width=""50"" align=""right""><br></td>
            <td width=""60"" align=""right""><br></td>
            <td width=""60"" align=""right""><br></td>
        </tr>
        <tr>
            <td width=""30""></td>
            <td width=""300""></td>
            <td width=""100"" align=""right""></td>
            <td width=""50"" align=""right""></td>
            <td width=""60"" align=""right"">Tax:</td>
            <td width=""60"" align=""right"">[TAX]</td>
        </tr>
        <tr>
            <td width=""30""></td>
            <td width=""300""></td>
            <td width=""100"" align=""right"">Qty of Boxes</td>
            <td width=""50"" align=""right"">[QTYBOXES]</td>
            <td width=""60"" align=""right"">Total:</td>
            <td width=""60"" align=""right"">[TOTAL]</td>
        </tr>
    </table>
</body>
</html>";

        public const string INVOICE_DETAIL_ROW = @"        <tr>
            <td width=""30"">[NO]</td>
            <td width=""300"">[DETAIL_NAME]</td>
            <td width=""100"">[DETAIL_UPC]</td>
            <td width=""50"" align=""right"">[DETAIL_QTY]</td>
            <td width=""60"" align=""right"">[DETAIL_UNIT_PRICE]</td>
            <td width=""60"" align=""right"">[DETAIL_TOTAL]</td>
        </tr>";

        #endregion

        #region Transfer HTML

        public const string TransferHtmlBody = @"<html>
<head>
</head>
<body>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""600"" align=""left""> <h2 style=""text-align:left"">[COMPANY_NAME]</h2></td>
        </tr>
    </table>

    <table width=""600"" border=""0"" cellpadding=""0"">
        <tr>
            <td width=""300"" align=""left"">
                <p style=""text-align:left"">
                    <span style=""color:#069"">
                        <span style=""font-family:arial,helvetica,sans-serif"">
                            [COMPANY_ADDRESS]<br>
                            [COMPANY_PHONE]<br>
                        </span>
                    </span>
                </p>
            </td>
            <td width=""250"" align=""right"">
                <p style=""text-align:left"">
                [TRANSFER_ONOFF]<br>
                Date: [TRANSFER_DATE]<br>
                [TRANSFER_SALESMAN]<br>
                </p>
                
            </td>
			<td  width=""50""></td>
        </tr>
    </table>

    
        <p></p><p></p><p></p>

    <table border=""0"" cellpadding=""0"" cellspacing=""2"" style=""width: 600px; font-weight: normal; font-size: small; "">
        <tr>
            <td width=""30"">
                <h3>No</h3>
            </td>
            <td width=""300"">
                <h3>Product</h3>
            </td>
            <td width=""100"">
                <h3>Upc</h3>
            </td>
            <td width=""50"" align=""right"">
                <h3>Qty</h3>
            </td>
            <td width=""60"" align=""right"">
                <h3></h3>
            </td>
            <td width=""60"" align=""right"">
                <h3></h3>
            </td>
        </tr>
        [DETAIL_ROWS]
        <tr>
            <td width=""30""><br></td>
            <td width=""300""><br></td>
            <td width=""100""><br></td>
            <td width=""50"" align=""right""><br></td>
            <td width=""60"" align=""right""><br></td>
            <td width=""60"" align=""right""><br></td>
        </tr>
        <tr>
            <td width=""30""></td>
            <td width=""300""></td>
            <td width=""100"" align=""right""><h3>Total:</h3></td>
            <td width=""50"" align=""right""><h3>[TOTAL]</h3></td>
            <td width=""60"" align=""right""></td>
            <td width=""60"" align=""right""></td>
        </tr>
    </table>
</body>
</html>";

        public const string TRANSFER_DETAIL_ROW = @" < tr>
            <td width=""30"">[NO]</td>
            <td width=""300"">[DETAIL_NAME]</td>
            <td width=""100"">[DETAIL_UPC]</td>
            <td width=""50"" align=""right"">[DETAIL_QTY]</td>
            <td width=""60"" align=""right""></td>
            <td width=""60"" align=""right""></td>
        </tr>";

        #endregion

        #region Invoice

        public string GetInvoicePdf(Invoice invoice)
        {
            string asHtml = GetInvoiceHTML(invoice);

            //using (Document document = new Document())
            //{
            //    try
            //    {
            //        string name = string.Format("invoice copy of invoice {0}.pdf", invoice.InvoiceNumber);
            //        string downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            //        string filePath = name;
            //        string fullPath = Path.Combine(downloadsPath, filePath);

            //        string targetFile = fullPath;

            //        if (File.Exists(targetFile))
            //            File.Delete(targetFile);

            //        PdfWriter.GetInstance(document, new FileStream(targetFile, FileMode.Create));
            //        document.Open();
            //        var stream = new StringReader(asHtml);
            //        List<IElement> htmlarraylist = HTMLWorker.ParseToList(stream, null);
            //        for (int k = 0; k < htmlarraylist.Count; k++)
            //        {
            //            document.Add((IElement)htmlarraylist[k]);
            //        }
            //        document.Close();

            //        return targetFile;
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.CreateLog(e);
            //    }
            //}
            return null;
        }

        string GetInvoiceHTML(Invoice invoice)
        {
            if (invoice == null)
                return "Invoice not found.";

            Client client = invoice.Client;

            if (client == null)
                return "Customer not found.";

            string result = InvoiceHtmlBody;
            string detailRow = "";
            string details = "";
            int detailNo = 0;
            double total = 0;
            float qtyBoxes = 0;

            // GENERAL INFORMATION
            result = result.Replace("[DOC_TYPE]", "INVOICE");
            result = result.Replace("[COMPANY_NAME]", CompanyInfo.Companies[0].CompanyName);
            result = result.Replace("[COMPANY_ADDRESS]", CompanyInfo.Companies[0].CompanyAddress1 + " " + CompanyInfo.Companies[0].CompanyAddress2);
            result = result.Replace("[COMPANY_PHONE]", CompanyInfo.Companies[0].CompanyPhone);

            result = result.Replace("[ORDER_HEADER]", "Invoice No: ");
            result = result.Replace("[INVOICE_NO]", invoice.InvoiceNumber);
            result = result.Replace("[INVOICE_DATE]", invoice.Date.ToShortDateString());
            result = result.Replace("[INVOICE_DUE_DATE]", invoice.DueDate.ToShortDateString());
            result = result.Replace("[INVOICE_NO]", invoice.InvoiceNumber);
            result = result.Replace("[CLIENT_NAME]", client.ClientName);
            result = result.Replace("[CLIENT_PHONE]", client.ContactPhone);
            result = result.Replace("[CLIENT_ADDRESS]", client.ShipToAddress);

            try
            {

                if (invoice.Details != null)
                {

                    // DETAILS
                    foreach (InvoiceDetail detail in invoice.Details)
                    {
                        detailRow = INVOICE_DETAIL_ROW;
                        detailRow = detailRow.Replace("[NO]", (++detailNo).ToString());

                        Product product = detail.Product;

                        detailRow = detailRow.Replace("[DETAIL_NAME]", (product != null) ? product.Name : "");
                        detailRow = detailRow.Replace("[DETAIL_UPC]", (product != null) ? product.Upc : "");
                        detailRow = detailRow.Replace("[DETAIL_UNIT_PRICE]", detail.Price.ToCustomString());
                        detailRow = detailRow.Replace("[DETAIL_QTY]", detail.Quantity.ToString(CultureInfo.CurrentCulture));
                        detailRow = detailRow.Replace("[DETAIL_TOTAL]", (detail.Quantity * detail.Price).ToCustomString());
                        details += detailRow;
                        qtyBoxes += Convert.ToSingle(detail.Quantity);
                        total += detail.Quantity * detail.Price;
                    }
                    result = result.Replace("[DETAIL_ROWS]", details);
                    result = result.Replace("[TAX]", Math.Round(invoice.Amount - total, 2).ToCustomString());
                    result = result.Replace("[TOTAL]", invoice.Amount.ToCustomString());
                    result = result.Replace("[QTYBOXES]", qtyBoxes.ToString(CultureInfo.CurrentCulture));
                }
                else
                {
                    result = result.Replace("[DETAIL_ROWS]", "");
                }
            }
            catch (Exception)
            {
                result = result.Replace("[DETAIL_ROWS]", "Error generating details.");
            }

            return result;
        }

        #endregion

        #region Order

        public string GetOrderPdf(Order order)
        {
            string asHtml = GetOrderHTML(order);

            //using (Document document = new Document())
            //{
            //    try
            //    {

            //        string downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            //        string name = string.Format("invoice copy of invoice {0}.pdf", order.PrintedOrderId);
            //        string filePath = name;
            //        string fullPath = Path.Combine(downloadsPath, filePath);

            //        string targetFile = fullPath;

            //        if (File.Exists(targetFile))
            //            File.Delete(targetFile);

            //        PdfWriter.GetInstance(document, new FileStream(targetFile, FileMode.Create));
            //        document.Open();
            //        var stream = new StringReader(asHtml);
            //        List<IElement> htmlarraylist = HTMLWorker.ParseToList(stream, null);
            //        for (int k = 0; k < htmlarraylist.Count; k++)
            //        {
            //            document.Add((IElement)htmlarraylist[k]);
            //        }
            //        if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
            //        {

            //            // save it to disk
            //            var bmp = order.ConvertSignatureToBitmap();
            //            string tempFile = System.IO.Path.GetTempFileName() + ".png";
            //            using (System.IO.FileStream file = new FileStream(tempFile, FileMode.Create))
            //            {
            //                bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, file);
            //            }
            //            var image = iTextSharp.text.Image.GetInstance(tempFile);

            //            if (image.Width > document.PageSize.Width)
            //            {
            //                var percentage = document.PageSize.Width / image.Width;
            //                image.ScalePercent(percentage * 100);
            //            }
            //            image.BorderWidth = 3f;
            //            document.Add(image);

            //            File.Delete(tempFile);
            //        }
            //        document.Close();

            //        return targetFile;
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.CreateLog(e);
            //    }
            //}
            return null;
        }

        string GetOrderHTML(Order order)
        {
            if (order == null)
                return "Order not found.";

            Client client = order.Client;

            if (client == null)
                return "Customer not found.";

            string result = InvoiceHtmlBody;
            string detailRow = "";
            string details = "";
            int detailNo = 0;
            //double total = 0;
            float qtyBoxes = 0;

            // GENERAL INFORMATION

            // get the batch of this order
            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
            if (string.IsNullOrEmpty(order.PrintedOrderId))
            {
                order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                order.Save();
            }
            result = result.Replace("Due Date:", string.Empty);
            result = result.Replace("[INVOICE_DUE_DATE]", string.Empty);

            if (order.OrderType == OrderType.Credit)
                result = result.Replace("[ORDER_HEADER]", "Credit No: ");
            else
                result = result.Replace("[ORDER_HEADER]", "Invoice No: ");

            result = result.Replace("[INVOICE_NO]", order.PrintedOrderId);
            result = result.Replace("[INVOICE_DATE]", order.Date.ToShortDateString());
            result = result.Replace("[COMPANY_NAME]", CompanyInfo.Companies[0].CompanyName);
            result = result.Replace("[COMPANY_ADDRESS]", CompanyInfo.Companies[0].CompanyAddress1 + "<br> " + CompanyInfo.Companies[0].CompanyAddress2);
            result = result.Replace("[COMPANY_PHONE]", "Phone: " + CompanyInfo.Companies[0].CompanyPhone);

            result = result.Replace("[CLIENT_NAME]", client.ClientName);
            result = result.Replace("[CLIENT_PHONE]", client.ContactPhone);
            result = result.Replace("[CLIENT_ADDRESS]", "Phone: " + client.ShipToAddress);

            try
            {

                if (order.Details != null)
                {

                    // DETAILS
                    foreach (OrderDetail detail in order.Details)
                    {
                        detailRow = INVOICE_DETAIL_ROW;
                        detailRow = detailRow.Replace("[NO]", (++detailNo).ToString());

                        Product product = detail.Product;

                        detailRow = detailRow.Replace("[DETAIL_NAME]", (product != null) ? product.Name : "");
                        detailRow = detailRow.Replace("[DETAIL_UPC]", (product != null) ? product.Upc : "");
                        detailRow = detailRow.Replace("[DETAIL_UNIT_PRICE]", detail.Price.ToCustomString());
                        detailRow = detailRow.Replace("[DETAIL_QTY]", detail.Qty.ToString(CultureInfo.CurrentCulture));
                        detailRow = detailRow.Replace("[DETAIL_TOTAL]", (detail.Qty * detail.Price).ToCustomString());
                        details += detailRow;
                        qtyBoxes += detail.Qty;
                        //total += detail.Qty * detail.Price;
                    }

                    result = result.Replace("[DETAIL_ROWS]", details);
                    result = result.Replace("[SUBTOTAL]", order.CalculateItemCost().ToCustomString());
                    result = result.Replace("[DISCOUNT]", order.CalculateDiscount().ToCustomString());
                    result = result.Replace("[TAX]", order.CalculateTax().ToCustomString());
                    result = result.Replace("[TOTAL]", order.OrderTotalCost().ToCustomString());
                    result = result.Replace("[QTYBOXES]", qtyBoxes.ToString(CultureInfo.CurrentCulture));
                }
                else
                {
                    result = result.Replace("[DETAIL_ROWS]", "");
                }
            }
            catch (Exception)
            {
                result = result.Replace("[DETAIL_ROWS]", "Error generating details.");
            }

            return result;
        }

        #endregion

        #region Transfer

        public string GetTransferPdf(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            string asHtml = GetTransferHTML(sortedList, isOn);

            //using (Document document = new Document())
            //{
            //    try
            //    {
            //        string name = string.Format("transfer {0} copy.pdf", isOn ? "on" : "off");

            //        string downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            //        string filePath = name;
            //        string fullPath = Path.Combine(downloadsPath, filePath);

            //        string targetFile = fullPath;

            //        if (File.Exists(targetFile))
            //            File.Delete(targetFile);

            //        PdfWriter.GetInstance(document, new FileStream(targetFile, FileMode.Create));
            //        document.Open();
            //        var stream = new StringReader(asHtml);
            //        List<IElement> htmlarraylist = HTMLWorker.ParseToList(stream, null);
            //        for (int k = 0; k < htmlarraylist.Count; k++)
            //        {
            //            document.Add((IElement)htmlarraylist[k]);
            //        }
            //        document.Close();

            //        return targetFile;
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.CreateLog(e);
            //    }
            //}
            return null;
        }

        string GetTransferHTML(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            string result = TransferHtmlBody;
            string detailRow = "";
            string details = "";
            int detailNo = 0;
            float total = 0;

            var salesman = Salesman.List != null ? Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId) : null;

            // GENERAL INFORMATION
            result = result.Replace("[COMPANY_NAME]", CompanyInfo.Companies[0].CompanyName);
            result = result.Replace("[COMPANY_ADDRESS]", CompanyInfo.Companies[0].CompanyAddress1 + " " + CompanyInfo.Companies[0].CompanyAddress2);
            result = result.Replace("[COMPANY_PHONE]", CompanyInfo.Companies[0].CompanyPhone);

            result = result.Replace("[TRANSFER_ONOFF]", isOn ? "Transfer On" : "Transfer Off");

            var date = DateTime.Now;

            result = result.Replace("[TRANSFER_DATE]", date.ToShortDateString() + "  " + date.ToShortTimeString());

            var sName = salesman != null ? "Salesman: " + salesman.Name : "";

            result = result.Replace("[TRANSFER_SALESMAN]", sName);

            try
            {

                if (sortedList != null)
                {

                    // DETAILS
                    foreach (InventoryLine detail in sortedList)
                    {
                        detailRow = TRANSFER_DETAIL_ROW;
                        detailRow = detailRow.Replace("[NO]", (++detailNo).ToString());

                        Product product = detail.Product;

                        detailRow = detailRow.Replace("[DETAIL_NAME]", (product != null) ? product.Name : "");
                        detailRow = detailRow.Replace("[DETAIL_UPC]", (product != null) ? product.Upc : "");

                        detailRow = detailRow.Replace("[DETAIL_QTY]", Math.Round(detail.Real, Config.Round).ToString(CultureInfo.CurrentCulture));

                        details += detailRow;
                        total += Convert.ToSingle(detail.Real);
                    }
                    result = result.Replace("[DETAIL_ROWS]", details);
                    result = result.Replace("[TOTAL]", Math.Round(total, Config.Round).ToString(CultureInfo.CurrentCulture));
                }
                else
                {
                    result = result.Replace("[DETAIL_ROWS]", "");
                }
            }
            catch (Exception)
            {
                result = result.Replace("[DETAIL_ROWS]", "Error generating details.");
            }

            return result;
        }

        #endregion

        #region Consignment

        public string GetConsignmentPdf(Order order, bool counting)
        {
            return GetOrderPdf(order);
        }

        #endregion

        #region Load Order

        public string GetLoadPdf(Order order)
        {
            return GetOrderPdf(order);
        }

        public string GetOrdersPdf(List<Order> order)
        {
            return null;
        }

        public string GetInvoicesPdf(List<Invoice> invoice)
        {
            return null;
        }

        public string GetGoalPdf(GoalProgressDTO goal)
        {
            return null;
        }

        public string GetPaymentPdf(InvoicePayment payment)
        {
            return string.Empty;
        }

        public string GetDepositPdf(BankDeposit bankDeposit)
        {
            return string.Empty;
        }
        public string GetReportPdf()
        {
            return string.Empty;
        }

        public string GetStatementReportPdf(Client client)
        {
            return string.Empty;
        }

        #endregion
    }
}