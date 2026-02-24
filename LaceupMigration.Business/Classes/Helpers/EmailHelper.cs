using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.IO.Image;
using System.IO;
using iText.IO.Font.Constants;
using Microsoft.Maui.Controls;
using Border = iText.Layout.Borders.Border;
using Cell = iText.Layout.Element.Cell;
using Image = iText.Layout.Element.Image;
using Style = iText.Layout.Style;

namespace LaceupMigration
{
    public enum FileFormat
    {
        All = 0,
        Pdf = 1,
        Xlsx = 2
    }

    public class PdfHelper
    {
        public static void GroupLines(Order targetOrder, Order order)
        {
            Dictionary<string, OrderLine> returnsLines = new Dictionary<string, OrderLine>();
            var rItems = order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.RelatedOrderDetail).ToList();
            //rItems.AddRange(order.Details.Where(y => y.RelatedOrderDetail > 0).Select(y => y.OrderDetailId));
            foreach (var od in order.Details)
            {
                if (od.HiddenItem)
                    continue;

                var uomId = -1;
                if (od.UnitOfMeasure != null)
                    uomId = od.UnitOfMeasure.Id;

                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + "-" + uomId.ToString() + "-" + (od.IsCredit ? "1" : "0");

                if (!Config.GroupLinesWhenPrinting || (!Config.GroupRelatedWhenPrinting && rItems.Contains(od.OrderDetailId)))
                    key = Guid.NewGuid().ToString();

                Dictionary<string, OrderLine> currentDic;
                currentDic = returnsLines;
                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od, ParticipatingDetails = new List<OrderDetail>(), IsCredit = od.IsCredit, Comments = od.Comments });

                if (od.Product.SoldByWeight && !order.AsPresale && order.OrderType != OrderType.Load)
                    currentDic[key].Qty = currentDic[key].Qty + od.Weight;
                else
                    currentDic[key].Qty = currentDic[key].Qty + od.Qty;

                currentDic[key].ParticipatingDetails.Add(od);
            }

            var lines = SortDetails.SortedDetails(returnsLines.Values.ToList());
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
            targetOrder.Details.Clear();
            foreach (var line in listXX)
            {
                var detail = new OrderDetail(line.Product, line.Qty, order);
                detail.Comments = line.ParticipatingDetails[0].Comments;
                detail.Damaged = false;
                detail.Deleted = false;
                detail.ExpectedPrice = line.Price;
                detail.RelatedOrderDetail = 0;
                detail.Substracted = true;
                detail.Price = line.Price;
                detail.Taxed = line.ParticipatingDetails[0].Taxed;
                detail.UnitOfMeasure = line.ParticipatingDetails[0].UnitOfMeasure;
                detail.DeliveryScanningChecked = true;
                detail.Weight = line.Qty;
                detail.IsCredit = line.IsCredit;
                detail.Discount = line.ParticipatingDetails[0].Discount;
                detail.DiscountType = line.ParticipatingDetails[0].DiscountType;

                targetOrder.Details.Add(detail);
            }
        }

        static void ShowPdf(string pdfFile)
        {
            //call this
            //Navigation.PushAsync(new PdfViewer(path), true);

        }

        public static IPdfProvider GetPdfProvider()
        {
            try
            {
                IPdfProvider provider;

                //if (Config.UseOldEmailFormat)
                //{
                //    provider = new FromHtmlPdf();
                //    return provider;
                //}

                // instantiate selected Pdf Provider
                Type t = Type.GetType(Config.PdfProvider);
                if (t == null)
                {
                    Logger.CreateLog("could not instantiate pdf provider" + Config.PdfProvider + " using DefaultPdfProvider instead");
                    provider = new DefaultPdfProvider();
                }
                provider = Activator.CreateInstance(t) as IPdfProvider;

                return provider;
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);

                return new DefaultPdfProvider();
            }
        }

        static IPdfProvider.IXlsxProvider GetXlsxProvider()
        {
            IPdfProvider.IXlsxProvider provider;

            Type t = Type.GetType(Config.XlsxProvider);
            if (t == null)
            {
                Logger.CreateLog("could not instantiate xlsm provider" + Config.XlsxProvider + " using DefaultXlsxProvider instead");
                provider = new DefaultXlsxProvider();
            }
            provider = Activator.CreateInstance(t) as IPdfProvider.IXlsxProvider;

            return provider;
        }

        //-------------------------------------------------------------------------------------------------

        static string GetTransferPdf(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                var pdfGenerator = new FromHtmlPdf();

                string pdfFile = pdfGenerator.GetTransferPdf(sortedList, isOn);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        static string GetInvoicePdf(Invoice invoice)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                pdfFile = pdfGenerator.GetInvoicePdf(invoice);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        static string GetInvoicesPdf(List<Invoice> invoices)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                pdfFile = pdfGenerator.GetInvoicesPdf(invoices);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static string GetOrderPdf(Order order)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                if ((Config.GroupLinesWhenPrinting || Config.GroupRelatedWhenPrinting) && !OrderDiscount.HasDiscounts && !order.Details.Any(x => x.Product.UseLot))
                {
                    var newOrder = Order.DuplicateorderHeader(order);
                    GroupLines(newOrder, order);
                    pdfFile = pdfGenerator.GetOrderPdf(newOrder);
                }
                else
                    pdfFile = pdfGenerator.GetOrderPdf(order);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static string GetOrdersPdf(List<Order> orders)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                IPdfProvider pdfGenerator = GetPdfProvider();

                List<Order> result = new List<Order>();

                foreach (var order in orders)
                {
                    if (string.IsNullOrEmpty(order.PrintedOrderId))
                    {
                        if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                        {
                            order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                            order.Save();
                        }
                    }

                    if ((Config.GroupLinesWhenPrinting || Config.GroupRelatedWhenPrinting) && !OrderDiscount.HasDiscounts)
                    {
                        var newOrder = Order.DuplicateorderHeader(order);
                        GroupLines(newOrder, order);
                        result.Add(newOrder);
                    }
                    else
                        result.Add(order);
                }

                string pdfFile = pdfGenerator.GetOrdersPdf(result);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static string GetConsignmentPdf(Order order, bool counting)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                pdfFile = pdfGenerator.GetConsignmentPdf(order, counting);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        public static string GetLoadPdf(Order order)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                if ((Config.GroupLinesWhenPrinting || Config.GroupRelatedWhenPrinting) && !OrderDiscount.HasDiscounts)
                {
                    var newOrder = Order.DuplicateorderHeader(order);
                    GroupLines(newOrder, order);
                    pdfFile = pdfGenerator.GetLoadPdf(newOrder);
                }
                else
                    pdfFile = pdfGenerator.GetLoadPdf(order);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        static string GetGoalPdf(GoalProgressDTO goal)
        {
            try
            {
                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                pdfFile = pdfGenerator.GetGoalPdf(goal);

                return pdfFile;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        static string GetDepositPdf(BankDeposit deposit)
        {
            try
            {
                string pdfFile = "";

                IPdfProvider pdfGenerator = GetPdfProvider();

                pdfFile = pdfGenerator.GetDepositPdf(deposit);

                return pdfFile;
            }
            catch (Exception ex)
            {
                return "";
            }
        }



        private static string GetOrderXlsx(Order order)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider.IXlsxProvider xlsxGenerator = GetXlsxProvider();

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                pdfFile = xlsxGenerator.GetOrderXlsx(order);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }

        private static List<string> GetOrdersXlsx(List<Order> orders)
        {
            List<string> files = new List<string>();

            foreach (var order in orders)
                files.Add(GetOrderXlsx(order));

            return files;
        }

        private static List<string> GetOrdersPdfs(List<Order> orders)
        {
            List<string> files = new List<string>();

            foreach (var order in orders)
                files.Add(GetOrderPdf(order));

            return files;
        }

        //-------------------------------------------------------------------------------------------------

        public static void SendTransferByEmail(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            try
            {
                string pdfFile = GetTransferPdf(sortedList, isOn);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Use platform-specific implementation (matches Xamarin SendTransferByEmail)
                Config.helper?.SendReportByEmail(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        /// <summary>Send single invoice by email (matches Xamarin EmailHelper.SendInvoiceByEmail: subject/body "Invoice Number X Attached").</summary>
        public static async Task SendInvoiceByEmail(Invoice invoice)
        {
            try
            {
                if (invoice.Signature == null)
                    DataProvider.GetExternalInvoiceSignature(invoice);

                string pdfFile = GetInvoicePdf(invoice);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Navigate to PDF viewer; pass invoice number so "Send by email" there pre-fills subject/body (matches Xamarin)
                var route = $"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&invoiceNumber={Uri.EscapeDataString(invoice.InvoiceNumber ?? "")}";
                if (invoice.Client?.ExtraProperties != null)
                {
                    var emailExtra = invoice.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "EMAIL");
                    if (emailExtra != null && !string.IsNullOrEmpty(emailExtra.Item2))
                        route += "&clientEmail=" + Uri.EscapeDataString(emailExtra.Item2);
                }
                await Shell.Current.GoToAsync(route);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendInvoicesByEmail(List<Invoice> invoices)
        {
            try
            {
                foreach (var inv in invoices)
                {
                    if (inv.Signature == null)
                        DataProvider.GetExternalInvoiceSignature(inv);
                }

                string pdfFile = GetInvoicesPdf(invoices);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Use platform-specific implementation (matches Xamarin SendInvoicesByEmail: subject "Invoices Attached")
                Config.helper?.SendOrderByEmail(pdfFile, "Invoices Attached", "Invoices Attached", null);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static async Task SendOrderByEmail(Order order)
        {
            try
            {
                string pdfFile = GetOrderPdf(order);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Navigate to PDF viewer with both PDF path and orderId (like PreviouslyOrderedTemplatePageViewModel)
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={order.OrderId}");

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static async Task SendOrdersByEmail(List<Order> orders)
        {
            try
            {
                if (orders == null || orders.Count == 0)
                {
                    return;
                }

                string pdfFile = GetOrdersPdf(orders);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Navigate to PDF viewer with the PDF path
                // For multiple orders, use the first order's ID
                int? orderId = orders.Count > 0 ? orders[0].OrderId : null;
                if (orderId.HasValue)
                {
                    await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={orderId.Value}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static async Task SendConsignmentByEmail(Order order, bool counting)
        {
            try
            {
                string pdfFile = GetConsignmentPdf(order, counting);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Navigate to PDF viewer with both PDF path and orderId
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={order.OrderId}");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendLoadByEmail(Order order)
        {
            try
            {
                string pdfFile = GetLoadPdf(order);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Use platform-specific implementation (matches Xamarin SendLoadByEmail)
                Config.helper?.SendReportByEmail(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendOrderByEmail(Order order, FileFormat format)
        {
            try
            {
                string pdfFile = format == FileFormat.Pdf || format == FileFormat.All ? GetOrderPdf(order) : string.Empty;

                string xlsmFile = format == FileFormat.Xlsx || format == FileFormat.All ? GetOrderXlsx(order) : string.Empty;

                // For now, send PDF if available (XLSX support can be added later)
                if (!string.IsNullOrEmpty(pdfFile))
                {
                    // Use platform-specific implementation (matches Xamarin SendOrderByEmail with format)
                    Config.helper?.SendReportByEmail(pdfFile);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendOrdersByEmail(List<Order> orders, FileFormat format)
        {
            try
            {
                // For multiple orders, generate a single combined PDF
                string pdfFile = format == FileFormat.Pdf || format == FileFormat.All ? GetOrdersPdf(orders) : string.Empty;

                if (!string.IsNullOrEmpty(pdfFile))
                {
                    // Use platform-specific implementation (matches Xamarin SendOrdersByEmail with format)
                    Config.helper?.SendReportByEmail(pdfFile);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        //-------------------------------------------------------------------------------------------------

        public static void ShowTransferPdf(IEnumerable<InventoryLine> sortedList, bool isOn)
        {
            try
            {
                string pdfFile = GetTransferPdf(sortedList, isOn);

                ShowPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void ShowInvoicePdf(Invoice invoice)
        {
            try
            {
                string pdfFile = GetInvoicePdf(invoice);

                ShowPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void ShowOrderPdf(Order order)
        {
            try
            {
                string pdfFile = GetOrderPdf(order);

                ShowPdf(pdfFile);

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void ShowOrdersPdf(List<Order> orders)
        {
            try
            {
                string pdfFile = GetOrdersPdf(orders);

                ShowPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void ShowConsignmentPdf(Order order, bool counting)
        {
            try
            {
                string pdfFile = GetConsignmentPdf(order, counting);

                ShowPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void ShowLoadByPdf(Order order)
        {
            try
            {
                string pdfFile = GetLoadPdf(order);


                ShowPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static async Task SendGoalByEmail(GoalProgressDTO goal)
        {
            try
            {
                string pdfFile = GetGoalPdf(goal);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Navigate to PDF viewer with the PDF path (matches SendOrderByEmail pattern)
                await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static void SendDepositByEmail(BankDeposit deposit)
        {
            try
            {
                string pdfFile = GetDepositPdf(deposit);

                if (string.IsNullOrEmpty(pdfFile))
                {
                    return;
                }

                // Use platform-specific implementation (matches Xamarin SendDepositByEmail)
                Config.helper?.SendReportByEmail(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        public static string GetPaymentPdf(InvoicePayment payment)
        {
            string pdfFile = "";

            IPdfProvider pdfGenerator = GetPdfProvider();

            pdfFile = pdfGenerator.GetPaymentPdf(payment);

            return pdfFile;

        }

        public static void ShowPaymentPdf(InvoicePayment payment)
        {
            var pdf = GetPaymentPdf(payment);

            ShowPdf(pdf);
        }
        protected virtual void AddCompanyInfo(Document doc)
        {

            CompanyInfo company = CompanyInfo.SelectedCompany;

            if (company == null)
                company = CompanyInfo.Companies[0];

            if (!string.IsNullOrEmpty(Config.CompanyLogo))
            {
                AddCompanyInfoWithLogo(doc, company);
                return;
            }

            AddTextLine(doc, company.CompanyName != null ? company.CompanyName : string.Empty, GetBigFont());
            AddTextLine(doc, company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }
        protected virtual Style GetBigFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(15);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetNormalFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            style.SetFont(font);
            style.SetFontSize(12);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetNormalBoldFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(12);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual Style GetSmallFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            style.SetFont(font);
            style.SetFontSize(6);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        protected virtual void AddCompanyInfoWithLogo(Document doc, CompanyInfo company)
        {
            try
            {
                Image jpg = null;
                if (!string.IsNullOrEmpty(company.CompanyLogoPath))
                    jpg = new Image(ImageDataFactory.Create(company.CompanyLogoPath));
                else
                    jpg = new Image(ImageDataFactory.Create(Config.LogoStorePath));


                jpg.ScaleToFit(90f, 75f);
                jpg.SetPaddingLeft(9f);

                doc.Add(jpg);

            }
            catch (Exception ex)
            {
                //no image

            }

            AddTextLine(doc, "   " + company.CompanyName, GetBigFont());
            AddTextLine(doc, "   " + company.CompanyAddress1, GetNormalFont());

            if (!string.IsNullOrEmpty(company.CompanyAddress2))
                AddTextLine(doc, "   " + company.CompanyAddress2, GetNormalFont());

            var phoneLine = string.IsNullOrEmpty(company.CompanyPhone) ? "" : "Email:" + company.CompanyPhone;
            phoneLine += string.IsNullOrEmpty(company.CompanyFax) ? "" : "Fax:" + company.CompanyFax;

            if (!string.IsNullOrEmpty(phoneLine))
                AddTextLine(doc, "   " + phoneLine, GetNormalFont());

            if (!string.IsNullOrEmpty(company.ExtraFields))
            {
                var extra = UDFHelper.GetSingleUDF("TIN", company.ExtraFields);
                if (!string.IsNullOrEmpty(extra))
                {
                    AddTextLine(doc, "TIN:" + extra, GetNormalFont());
                }
            }

            if (!string.IsNullOrEmpty(company.CompanyEmail))
                AddTextLine(doc, "Email:" + company.CompanyEmail, GetNormalFont());
        }
        protected virtual void AddTextLine(Document doc, string text, Style style, TextAlignment alignment = TextAlignment.LEFT)
        {
            Paragraph pdfTxet = new Paragraph(text);
            pdfTxet.AddStyle(style);
            pdfTxet.SetTextAlignment(alignment);

            doc.Add(pdfTxet);
        }
        protected virtual string[] ClientAddress(Client client, bool shipTo = true)
        {
            var addr = shipTo ? client.ShipToAddress : client.BillToAddress;

            if (string.IsNullOrEmpty(addr))
                return new string[] { };

            var parts = addr.Split(new char[] { '|' });
            if (parts != null)
            {
                if (parts.Length == 5)
                {
                    parts[2] = parts[2].Trim() + ", " + parts[3].Trim() + " " + parts[4].Trim();
                    if (parts[1].Trim().Length == 0)
                    {
                        var newParts = new string[2];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[2].Trim();
                        return newParts;
                    }
                    else
                    {
                        var newParts = new string[3];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[1].Trim();
                        newParts[2] = parts[2].Trim();
                        return newParts;
                    }
                }
                if (parts.Length == 4)
                {
                    parts[2] = parts[2].Trim() + ", " + parts[3].Trim();
                    if (parts[1].Trim().Length == 0)
                    {
                        var newParts = new string[2];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[2].Trim();
                        return newParts;
                    }
                    else
                    {
                        var newParts = new string[3];
                        newParts[0] = parts[0].Trim();
                        newParts[1] = parts[1].Trim();
                        newParts[2] = parts[2].Trim();
                        return newParts;
                    }
                }
                return parts;
            }
            return new string[] { };
        }






        protected virtual Style GetSmall8WhiteFont()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }
        protected virtual void AddCellToHeader(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8WhiteFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }
        protected virtual void AddCellToBody(Table tableLayout, string cellText, HorizontalAlignment alignment, Border border)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetNormalFont());

            var cell = new Cell();
            cell.SetHorizontalAlignment(alignment);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }

        protected virtual void AddCellToBody(Table tableLayout, string cellText)
        {
            var paragraph = new Paragraph(cellText);
            paragraph.AddStyle(GetSmall8Font());

            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(paragraph);

            tableLayout.AddCell(cell);
        }
        protected virtual void AddCellToImageBody(Table tableLayout, Image image)
        {
            var cell = new Cell();
            cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            cell.SetPadding(5);
            cell.SetBackgroundColor(ColorConstants.WHITE);
            cell.Add(image);

            tableLayout.AddCell(cell);
        }
        protected virtual Style GetSmall8Font()
        {
            Style style = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            style.SetFont(font);
            style.SetFontSize(8);
            style.SetFontColor(ColorConstants.BLACK);

            return style;
        }

        public virtual string GeneratePdfCatalog(Document doc, List<Product> products, Client client, bool showPrice = true, bool showUPC = true, bool showUoM = true)
        {
            //add companyinfo

            string appPath = Config.BasePath;
            // Utiliza DateTime.Now para obtener un timestamp �nico y as� evitar sobrescribir archivos.
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"ProductCatalog{timestamp}.pdf";
            string filePath = System.IO.Path.Combine(appPath, fileName);

            // No es necesario comprobar si el archivo existe ya que el nombre ser� �nico.
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    using (doc = new Document(pdf))
                    {
                        AddCompanyInfo(doc);

                        //addclientinfo
                        if (client != null)
                            AddOrderClientInfo(doc, client);

                        AddCatalogDetails(doc, products, client, showPrice, showUPC, showUoM);
                    }
                }

                return filePath;
            }
        }
        protected virtual void AddOrderClientInfo(Document doc, Client client, bool isQuote = false)
        {
            if (client == null)
                return;

            // Se asume que quieres una tabla de una sola fila con el nombre del cliente.


            string clientInfo = client.ClientName + "\n";

            // Puedes agregar m�s informaci�n del cliente aqu� si es necesario.
            // Por ejemplo:
            // clientInfo += "Address: " + client.Address + "\n";
            // clientInfo += "Phone: " + client.ContactPhone + "\n";

            // Agregar la celda con la informaci�n del cliente al documento.
            AddTextLine(doc, clientInfo, GetNormalBoldFont(), TextAlignment.LEFT);

            // Agregar una l�nea en blanco despu�s de la informaci�n del cliente para separarla del cat�logo de productos.
            doc.Add(new Paragraph("\n"));
        }

        public void AddCatalogDetails(Document doc, List<Product> products, Client client, bool showPrice = true, bool showUPC = true, bool showUoM = true)//Catalogs
        {
            //Create PDF Table

            bool useUoM = UnitOfMeasure.List.Count > 0;

            // Build headers array dynamically based on what should be shown
            // Picture|Name|UPC|Precio|UoM|Packaging 
            var headerList = new List<float> { 20, 25 }; // Image and Name are always shown
            var headerLabels = new List<string> { "Image", "Product Name" };
            
            if (showUPC)
            {
                headerList.Add(15);
                headerLabels.Add("UPC");
            }
            
            if (showPrice)
            {
                headerList.Add(14);
                headerLabels.Add("Precio");
            }
            
            if (showUoM)
            {
                headerList.Add(13);
                headerLabels.Add("UoM");
            }
            
            headerList.Add(14); // Packaging is always shown
            headerLabels.Add("Packaging");

            // Normalize header widths to 100%
            float total = headerList.Sum();
            float[] headers = headerList.Select(h => (h / total) * 100).ToArray();

            Table tableLayout = new Table(UnitValue.CreatePercentArray(headers)).UseAllAvailableWidth();

            //Set the PDF File witdh percentage
            foreach (var label in headerLabels)
            {
                AddCellToHeader(tableLayout, label);
            }

            foreach (var product in products)
            {
                try
                {
                    Image jpg = null;

                    var productImage = ProductImage.GetProductImage(product.ProductId);
                    if (!string.IsNullOrEmpty(productImage))
                        jpg = new Image(ImageDataFactory.Create(productImage));

                    if (jpg != null)
                    {
                        jpg.ScaleToFit(90f, 75f);
                        AddCellToImageBody(tableLayout, jpg);

                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog("No Logo Found =>" + ex.ToString());
                }

                string priceString = string.Empty;
                string uomString = string.Empty;

                var price = product.PriceLevel0;
                if (client != null)
                {
                    bool cameFromOffer = false;
                    price = Product.GetPriceForProduct(product, client, true);
                }

                var uomFamily = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily);
                if (uomFamily.Count() > 0)
                {
                    foreach (var uom in uomFamily)
                    {
                        if (string.IsNullOrEmpty(priceString))
                            priceString = (price * uom.Conversion).ToCustomString();
                        else
                            priceString += "\n" + (price * uom.Conversion).ToCustomString();

                        if (string.IsNullOrEmpty(uomString))
                            uomString = uom.Name;
                        else
                            uomString += "\n" + uom.Name;
                    }
                }
                else
                {
                    priceString = price.ToCustomString();
                    uomString = string.Empty;
                }

                AddCellToBody(tableLayout, product.Name);
                
                if (showUPC)
                {
                    AddCellToBody(tableLayout, product.Upc);
                }
                
                if (showPrice)
                {
                    AddCellToBody(tableLayout, priceString);
                }
                
                if (showUoM)
                {
                    AddCellToBody(tableLayout, uomString);
                }
                
                AddCellToBody(tableLayout, product.Package);


            }

            doc.Add(tableLayout);
        }
        public static string GetOrderPdfBillOfLadin(Order order)
        {
            try
            {
                if (Salesman.List == null || Salesman.List.Count == 0)
                {
                    try
                    {
                        DataProvider.GetSalesmanList();
                    }
                    catch
                    {
                        throw;
                    }
                }

                string pdfFile = "";

                IPdfProvider pdfGenerator = new MilagroBillOfLadingPdfGenerator();

                if (string.IsNullOrEmpty(order.PrintedOrderId))
                {
                    if ((order.AsPresale && Config.GeneratePresaleNumber) || (!order.AsPresale && Config.GeneratePreorderNum))
                    {
                        order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
                        order.Save();
                    }
                }

                pdfFile = pdfGenerator.GetOrderPdf(order);

                return pdfFile;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return string.Empty;
            }
        }
    }
}

