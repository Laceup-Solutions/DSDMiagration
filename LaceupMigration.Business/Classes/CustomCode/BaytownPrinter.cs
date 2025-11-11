using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkiaSharp;

namespace LaceupMigration
{
    public class BaytownPrinter : ZebraFourInchesPrinter
    {
        protected const string BaytownDate = "BaytownDate";
        protected const string BaytownSalesman = "BaytownSalesman";
        protected const string BaytownCompanyName = "BaytownCompanyName";
        protected const string BaytownCompanyAddress = "BaytownCompanyAddress";
        protected const string BaytownPhone = "BaytownPhone";
        protected const string BaytownFax = "BaytownFax";
        protected const string BaytownEmail = "BaytownEmail";
        protected const string BaytownText = "BaytownText";
        protected const string BaytownInvoiceHeader = "BaytownInvoiceHeader";
        protected const string BaytownInvoiceNumber = "BaytownInvoiceNumber";
        protected const string BaytownPONumber = "BaytownPONumber";
        protected const string BaytownToClient = "BaytownToClient";
        protected const string BaytownToAddr = "BaytownToAddr";
        protected const string BaytownWorkOrder = "BaytownWorkOrder";
        protected const string BaytownJobName = "BaytownJobName";
        protected const string BaytownInvoiceType = "BaytownInvoiceType";
        protected const string BaytownTableDotLine = "BaytownTableDotLine";
        protected const string BaytownTableHeader = "BaytownTableHeader";
        protected const string BaytownTableLine = "BaytownTableLine";
        protected const string BaytownComment = "BaytownLineComment";
        protected const string BaytownItemType = "BaytownItemType";
        protected const string BaytownTotal = "BaytownTotal";
        protected const string BaytownSignature = "BaytownSignature";
        protected const string BaytownPrintName = "BaytownPrintName";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(BaytownDate, "^FO450,{0}^ADN,18,10^FD    Date: {1}^FS");
            linesTemplates.Add(BaytownSalesman, "^FO450,{0}^ADN,18,10^FD  Driver: {1}^FS"); //max 20
            linesTemplates.Add(BaytownCompanyName, "^FO50,{0}^ABN,30,15^FB700,1,0,C^FD{1}^FS"); //max 16
            linesTemplates.Add(BaytownCompanyAddress, "^FO190,{0}^ADN,18,10^FD{1}^FS"); //max 38
            linesTemplates.Add(BaytownPhone, "^FO240,{0}^ADN,18,10^FDOrders: {1}^FS");
            linesTemplates.Add(BaytownFax, "^FO240,{0}^ADN,18,10^FDAccounting: {1}^FS");
            linesTemplates.Add(BaytownEmail, "^FO240,{0}^ADN,18,10^FDEmail: {1}^FS");
            linesTemplates.Add(BaytownText, "^FO300,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(BaytownInvoiceHeader, "^FO210,{0}^ADN,36,20^FDDelivery Invoice^FS");
            linesTemplates.Add(BaytownInvoiceNumber, "^FO210,{0}^ADN,36,20^FD{1}^FS"); //max 16
            linesTemplates.Add(BaytownPONumber, "^FO40,{0}^ADN,18,10^FD     {1}^FS"); //max 14
            linesTemplates.Add(BaytownToClient, "^FO40,{0}^ADN,18,10^FD{1}^FS"); //max 23
            linesTemplates.Add(BaytownToAddr, "^FO150,{0}^ADN,18,10^FD{1}^FS"); //max 23
            linesTemplates.Add(BaytownWorkOrder, "^FO40,{0}^ADN,18,10^FD{1}^FS"); //max 23
            linesTemplates.Add(BaytownJobName, "^FO40,{0}^ADN,18,10^FD{1}^FS"); //max 23
            linesTemplates.Add(BaytownInvoiceType, "^FO350,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(BaytownTableDotLine, "^FO40,{0}^ADN,18,10^FD---------------------------------------------------------------^FS");
            linesTemplates.Add(BaytownTableHeader, "^FO40,{0}^ABN,18,10^FDProduct^FS" +
                //"^FO480,{0}^ABN,18,10^FDQty^FS" +
                //"^FO560,{0}^ABN,18,10^FDRate^FS" +
                "^FO680,{0}^ABN,18,10^FDQty^FS");
            linesTemplates.Add(BaytownComment, "^FO40,{0}^ADN,18,10^FD{1}^FS");

            linesTemplates.Add(BaytownTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
               //"^FO480,{0}^ADN,18,10^FD{2}^FS" +
               //"^FO560,{0}^ADN,18,10^FD{3}^FS" +
               "^FO680,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(BaytownItemType, "^FO40,{0}^ABN,18,10^FD{1}^FS");
            linesTemplates.Add(BaytownTotal, "^FO300,{0}^ABN,20,12^FD{1}^FS"); //max 14
            linesTemplates.Add(BaytownSignature, "^FO40,{0}^ADN,18,10^FD Signature: _________________________________^FS"); //max 14
            linesTemplates.Add(BaytownPrintName, "^FO40,{0}^ADN,18,10^FDPrint Name: {1}^FS"); //max 14
        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();
            int startY = 110;

            Dictionary<string, OrderLine> salesLines = new Dictionary<string, OrderLine>();
            Dictionary<string, OrderLine> creditLines = new Dictionary<string, OrderLine>();

            foreach (var od in order.Details)
            {
                var key = od.Product.ProductId.ToString() + "-" + od.Price.ToString() + od.Damaged.ToString();
                Dictionary<string, OrderLine> currentDic;

                if (!od.IsCredit)
                    currentDic = salesLines;
                else
                    currentDic = creditLines;

                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od });

                currentDic[key].Qty = currentDic[key].Qty + od.Qty;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownDate], startY, order.Date.ToString()));
            startY += 20;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownSalesman], startY, salesman.Name));
                startY += 20;
            }

            startY += 50;

            lines.AddRange(GetCompanyRows(ref startY, asPreOrder, order));
            lines.AddRange(GetInfoOrder(ref startY, asPreOrder, order));

            if (salesLines.Count > 0)
                lines.AddRange(GetTable(ref startY, asPreOrder, order, "Sales", salesLines.Values.ToList()));

            if (creditLines.Count > 0)
                lines.AddRange(GetTable(ref startY, asPreOrder, order, "Credit", creditLines.Values.ToList()));

            startY += 50;

            //lines.AddRange(GetTotalsOrder(ref startY, asPreOrder, order));
            //startY += 50;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.Add(IncludeSignature(order, lines, ref startY));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownSignature], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownPrintName], startY, order.SignatureName));
            startY += 50;

            int index = 0;

            if (!string.IsNullOrEmpty(order.Comments))
            {
                foreach (var item in SplitProductName(order.Comments, 47, 62))
                {
                    var comment = index == 0 ? "Order Comment: " + item : item;
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownComment], startY, comment));
                    startY += 20;
                    index++;
                }
            }

            startY += 30;

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
                return false;
            }
        }

        private IList<string> GetCompanyRows(ref int startY, bool preOrder, Order order)
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

            string compName = CompanyInfo.Companies[0].CompanyName;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownCompanyName], startY, compName));
            startY += 50;

            var address = CompanyInfo.Companies[0].CompanyAddress1;
            if (!string.IsNullOrEmpty(CompanyInfo.Companies[0].CompanyAddress2))
                address += " " + CompanyInfo.Companies[0].CompanyAddress2;

            if (address.Length < 38)
            {
                var l = address.Length;
                address = new string(' ', (38 - l) / 2) + address;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownCompanyAddress], startY, address));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownPhone], startY, CompanyInfo.Companies[0].CompanyPhone));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownFax], startY, "832-784-7073"));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownEmail], startY, CompanyInfo.Companies[0].CompanyEmail));
            startY += 25;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownText], startY, "Home of BIG BIN"));
            startY += 45;

            return lines;
        }

        private IList<string> GetInfoOrder(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownInvoiceHeader], startY));
            startY += 40;

            var invoiceNum = order.PrintedOrderId;

            if (invoiceNum.Length < 16)
            {
                var l = invoiceNum.Length;
                invoiceNum = new string(' ', (16 - l) / 2) + invoiceNum;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownInvoiceNumber], startY, invoiceNum));
            startY += 40;

            startY += 30;

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                var poNum = "PO# " + order.PONumber;

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownPONumber], startY, poNum));
                startY += 30;
            }

            var clientName = order.Client.ClientName;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownToClient], startY, "Bill to: " + clientName));
            startY += 20;

            foreach (string s11 in ClientAddress(order.Client, false))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownToAddr], startY, s11));
                startY += 20;
            }

            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownToClient], startY, "Ship to: " + clientName));
            startY += 20;

            foreach (string s11 in ClientAddress(order.Client, true))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownToAddr], startY, s11));
                startY += 20;
            }

            string workOrder = "";
            string jobName = "";

            var item = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "jobname");
            if (item != null && !string.IsNullOrEmpty(item.Item2))
                jobName = item.Item2;

            var item2 = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "workorder");
            if (item != null && !string.IsNullOrEmpty(item.Item2))
                workOrder = item.Item2;

            if (!string.IsNullOrEmpty(workOrder))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownWorkOrder], startY, "Work Order/Charge Number: " + workOrder));
                startY += 20;
            }
            if (!string.IsNullOrEmpty(jobName))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownJobName], startY, "Job Name/Contact: " + jobName));
                startY += 20;
            }

            startY += 60;

            return lines;
        }

        private IList<string> GetTable(ref int startY, bool preOrder, Order order, string tableheader, IList<OrderLine> rows)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownInvoiceType], startY, tableheader));
            startY += 60;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableDotLine], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableHeader], startY));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableDotLine], startY));
            startY += 25;

            foreach (var item in rows)
            {
                var productName = SplitDetailProductName(item.Product.Name);

                var factor = item.OrderDetail.IsCredit ? -1 : 1;
                var qty = item.Qty.ToString();
                var price = (item.Price * factor).ToCustomString();
                var total = (item.Qty * item.Price * factor).ToCustomString();

                for (int i = 0; i < productName.Count(); i++)
                {
                    if (i == 0)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableLine], startY,
                            productName[i], qty));
                    else
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableLine], startY,
                            productName[i], string.Empty));

                    startY += 20;
                }

                if (item.OrderDetail.IsCredit)
                {
                    var type = item.OrderDetail.Damaged ? "Dump" : "Return";
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownItemType], startY, type));
                    startY += 20;
                }

                if (!string.IsNullOrEmpty(item.OrderDetail.Comments))
                {
                    var ic = 0;
                    foreach (var c in SplitProductName(item.OrderDetail.Comments, 25, 34))
                    {
                        var cLine = ic == 0 ? "Comment: " + c : c;
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownComment], startY, cLine));
                        startY += 20;
                        ic++;
                    }
                }

                startY += 5;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTableDotLine], startY));
            startY += 50;

            return lines;
        }

        IList<string> SplitDetailProductName(string name)
        {
            return SplitProductName(name, 35, 35);
        }

        private IList<string> GetTotalsOrder(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            string s = "     Subtotal: " + order.CalculateItemCost().ToCustomString();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTotal], startY, s));
            startY += 40;

            s = "          Tax: " + order.CalculateTax().ToCustomString();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTotal], startY, s));
            startY += 40;

            s = "Invoice Total: " + order.OrderTotalCost().ToCustomString();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[BaytownTotal], startY, s));
            startY += 120;

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
    }
}