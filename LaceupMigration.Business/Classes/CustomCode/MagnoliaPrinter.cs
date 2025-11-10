using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class MagnoliaPrinter : ZebraFourInchesPrinter
    {
        protected const string MagnoliaCompanyName = "MagnoliaCompanyName";
        protected const string MagnoliaCompanyAddr1 = "MagnoliaCompanyAddr1";
        protected const string MagnoliaCompanyContact = "MagnoliaCompanyContact";
        protected const string MagnoliaInvoiceType = "MagnoliaInvoiceType";
        protected const string MagnoliaInfoLine1 = "MagnoliaInfoLine1";
        protected const string MagnoliaInfoLine2 = "MagnoliaInfoLine2";
        protected const string MagnoliaInfoLine3 = "MagnoliaInfoLine3";
        protected const string MagnoliaInfoLine4 = "MagnoliaInfoLine4";
        protected const string MagnoliaInfoLine5 = "MagnoliaInfoLine5";
        protected const string MagnoliaInvoiceHeader = "MagnoliaInvoiceHeader";
        protected const string MagnoliaDotLineTable = "MagnoliaDotLineTable";
        protected const string MagnoliaTableHeader = "MagnoliaTableHeader";
        protected const string MagnoliaTableLine = "MagnoliaTableLine";
        protected const string MagnoliaTotals = "MagnoliaTotals";
        protected const string MagnoliaSignatureDotLine = "MagnoliaSignatureDotLine";
        protected const string MagnoliaSignatureHeader = "MagnoliaSignatureHeader";
        protected const string MagnoliaFooterText = "MagnoliaFooterText";


        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(MagnoliaCompanyName, "^FO230,{0}^ABN,36,20^FD{1}^FS"); //max 12
            linesTemplates.Add(MagnoliaCompanyAddr1, "^FO150,{0}^ADN,18,10^FD{1}^FS");  //max 43
            linesTemplates.Add(MagnoliaCompanyContact, "^FO170,{0}^ADN,18,10^FDTel#: {1} - Fax#: {2}^FS");
            linesTemplates.Add(MagnoliaInvoiceType, "^FO40,{0}^ADN,36,20^FD{1}^FS");
            linesTemplates.Add(MagnoliaInfoLine1, "^FO80,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FD{2}^FS"); //{1} max 23
            linesTemplates.Add(MagnoliaInfoLine2, "^FO80,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FDRoute# {2}^FS"); //{1} max 23
            linesTemplates.Add(MagnoliaInfoLine3, "^FO80,{0}^ADN,18,10^FD{1}^FS^FO480,{0}^ADN,18,10^FDSalesman: {2}^FS"); //{1} max 23
            linesTemplates.Add(MagnoliaInfoLine4, "^FO480,{0}^ADN,18,10^FDTerms: {1}^FS"); //{1} max 18

            linesTemplates.Add(MagnoliaInfoLine5, "^FO480,{0}^ADN,18,10^FD^FS"); //{1} max 21

            linesTemplates.Add(MagnoliaInvoiceHeader, "^FO40,{0}^ABN,20,12^FD{1}^FS");
            linesTemplates.Add(MagnoliaDotLineTable, "^FO40,{0}^ADN,18,10^FD---------------------------------------------------------------^FS");
            linesTemplates.Add(MagnoliaTableHeader, "^FO40,{0}^ABN,18,10^FDItem^FS" +
                "^FO100,{0}^ABN,18,10^FDUPC#^FS" +
                "^FO210,{0}^ABN,18,10^FDDescription^FS" +  //max 26
                "^FO530,{0}^ABN,18,10^FDQty^FS" +
                "^FO580,{0}^ABN,18,10^FDPrice^FS" +
                "^FO645,{0}^ABN,18,10^FDAllow^FS" +
                "^FO720,{0}^ABN,18,10^FDTotal^FS");
            linesTemplates.Add(MagnoliaTableLine, "^FO40,{0}^APN,18,10^FD{1}^FS" +
                "^FO100,{0}^APN,18,10^FD{2}^FS" +
                "^FO210,{0}^APN,18,10^FD{3}^FS^FS" +
                "^FO530,{0}^APN,18,10^FD{4}^FS" +
                "^FO580,{0}^APN,18,10^FD{5}^FS" +
                "^FO645,{0}^APN,18,10^FD{6}^FS" +
                "^FO720,{0}^APN,18,10^FD{7}^FS");
            linesTemplates.Add(MagnoliaTotals, "^FO40,{0}^ABN,20,12^FDTotal Units: {1}^FS^FO500,{0}^ABN,20,12^FDTotal: {2}^FS");
            linesTemplates.Add(MagnoliaSignatureDotLine, "^FO130,{0}^ADN,18,10^FD--------------------------------------------^FS");
            linesTemplates.Add(MagnoliaSignatureHeader, "^FO130,{0}^ADN,18,10^FD             Customer Signature^FS");
            linesTemplates.Add(MagnoliaFooterText, "^FO40,{0}^ADN,18,10^FD{1}^FS"); //max 61

        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            List<string> lines = new List<string>();
            int startY = 80;

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
                    if (od.Damaged)
                        currentDic = creditLines;
                    else
                        currentDic = returnsLines;
                }

                if (!currentDic.ContainsKey(key))
                    currentDic.Add(key, new OrderLine() { Product = od.Product, Price = od.Price, OrderDetail = od });

                currentDic[key].Qty = currentDic[key].Qty + od.Qty;
            }

            lines.AddRange(GetCompanyRows(ref startY, asPreOrder, order));

            string orderType = "Invoice# ";
            if (order.OrderType == OrderType.Credit)
                orderType = "Credit# ";

            orderType += order.PrintedOrderId;

            int spaceI = 30;
            spaceI -= orderType.Length;
            spaceI /= 2;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInvoiceType], startY, new string(' ', spaceI) + orderType));
            startY += 60;

            lines.AddRange(GetInfoOrder(ref startY, asPreOrder, order));
            startY += 50;

            if (salesLines.Count > 0)
            {
                string title = string.Empty;

                string terms = string.Empty;

                if (order.Client.ExtraProperties != null)
                {
                    var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                    if (termsExtra == null)
                        title = "Cash ";
                }

                lines.AddRange(GetDetailsTable(ref startY, title + "Sales", salesLines.Values.ToList()));
            }

            if (creditLines.Count > 0)
                lines.AddRange(GetDetailsTable(ref startY, "Dump", creditLines.Values.ToList()));

            if (returnsLines.Count > 0)
                lines.AddRange(GetDetailsTable(ref startY, "Return", returnsLines.Values.ToList()));

            startY += 50;

            lines.AddRange(GetFooterLines(ref startY, asPreOrder, order));

            startY += 50;

            lines.Add(String.Format(CultureInfo.InvariantCulture, "^FO270,{0}^BCN,80^FD{1}^FS", startY, order.PrintedOrderId));

            startY += 90;

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

        protected virtual IList<string> GetCompanyRows(ref int startY, bool preOrder, Order order)
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

            //lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyName], startY, CompanyInfo.Companies[0].CompanyName));
            //startY += 50;

            var address = CompanyInfo.Companies[0].CompanyAddress1;
            if (!string.IsNullOrEmpty(CompanyInfo.Companies[0].CompanyAddress2))
                address += " " + CompanyInfo.Companies[0].CompanyAddress2;

            if (address.Length < 43)
            {
                var l = address.Length;
                address = new string(' ', (43 - l) / 2) + address;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyAddr1], startY, address));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaCompanyContact], startY, CompanyInfo.Companies[0].CompanyPhone, CompanyInfo.Companies[0].CompanyFax));
            startY += 40;

            return lines;
        }

        protected IList<string> GetInfoOrder(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            var cNameParts = SplitCustomerName(order.Client.ClientName);

            var count = 0;
            foreach (var part in cNameParts)
            {
                string dateAsString = string.Empty;
                if (count == 0)
                    dateAsString = "Date: " + order.Date.ToShortDateString();

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInfoLine1], startY, part, dateAsString));
                startY += 30;
                count++;
            }

            var clientAddr = ClientAddress(order.Client, true);

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInfoLine2], startY, clientAddr[0], Config.SalesmanId.ToString()));
            startY += 30;

            string addr2 = string.Empty;
            if (clientAddr.Length > 1)
                addr2 = clientAddr[1];

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            string salesmanName = string.Empty;
            if (salesman != null)
                salesmanName = salesman.Name;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInfoLine3], startY, addr2, salesmanName));
            startY += 30;

            string terms = string.Empty;

            if (order.Client.ExtraProperties != null)
            {
                var termsExtra = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "TERMS");
                if (termsExtra != null)
                {
                    terms = termsExtra.Item2.ToUpperInvariant();

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInfoLine4], startY, terms));
                    startY += 30;
                }
            }

            startY += 30;

            if (preOrder)
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[RouteReturnsNotFinalLabel], startY, "NOT A FINAL INVOICE"));
                startY += font36Separation;
            }

            return lines;
        }

        protected virtual IList<string> GetDetailsTable(ref int startY, string tableheader, IList<OrderLine> details)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaInvoiceHeader], startY, tableheader));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaDotLineTable], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaTableHeader], startY));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaDotLineTable], startY));
            startY += 30;

            foreach (var item in details)
            {
                var code = item.Product.Code;
                var upc = item.Product.Upc;

                var prodNameSections = SplitDetailProductName(item.Product.Description);

                string price = item.Price.ToCustomString();
                string allowance = item.OrderDetail.Allowance.ToCustomString();
                var total = ((item.Qty * item.Price) - (item.Qty * item.OrderDetail.Allowance)).ToCustomString();

                if (item.OrderDetail.IsCredit)
                {
                    price = "-" + price;
                    total = "-" + total;
                }
                else if (item.OrderDetail.Allowance > 0)
                    allowance = "-" + allowance;

                for (int i = 0; i < prodNameSections.Count(); i++)
                {
                    string section = prodNameSections[i];

                    if (i == 0)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaTableLine], startY,
                            code, upc, section, item.Qty, price, allowance, total));
                    else if (!Config.PrintTruncateNames)
                        lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaTableLine], startY,
                            string.Empty, string.Empty, section, string.Empty, string.Empty, string.Empty, string.Empty));

                    startY += 25;
                }

                startY += 5;
            }

            startY += 70;

            return lines;
        }

        protected virtual IList<string> SplitDetailProductName(string name)
        {
            return SplitProductName(name, 42, 42);
        }

        protected IList<string> SplitCustomerName(string name)
        {
            return SplitProductName(name, 23, 23);
        }

        protected IList<string> SplitFooterText(string text)
        {
            return SplitProductName(text, 61, 61);
        }

        protected float GetTotalUnits(Order order)
        {
            float totalUnits = 0;

            foreach (var item in order.Details)
            {
                float qty = item.Qty;

                if (item.UnitOfMeasure != null)
                    qty *= item.UnitOfMeasure.Conversion;

                totalUnits += qty;
            }

            return totalUnits;
        }

        protected virtual IList<string> GetFooterLines(ref int startY, bool preOrder, Order order)
        {
            List<string> lines = new List<string>();

            var totalUnits = GetTotalUnits(order);
            var totalCost = order.OrderTotalCost();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaTotals], startY, totalUnits, totalCost.ToCustomString()));

            startY += 100;

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.Add(IncludeSignature(order, lines, ref startY));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaSignatureDotLine], startY));
            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaSignatureHeader], startY));
            startY += 80;

            foreach (var item in SplitFooterText(Config.BottomOrderPrintText))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[MagnoliaFooterText], startY, item));
                startY += 18;
            }

            startY += 12;

            return lines;
        }

        protected override string IncludeSignature(Order order, List<string> lines, ref int startIndex)
        {
            DateTime st = DateTime.Now;
            Android.Graphics.Bitmap signature;
            signature = order.ConvertSignatureToBitmap();
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            DateTime st1 = DateTime.Now;
            var converter = new LaceupAndroidApp.BitmapConvertor();
            // var path = System.IO.Path.GetTempFileName () + ".bmp";
            var rawBytes = converter.convertBitmap(signature, null);
            Logger.CreateLog("Order.ConvertSignatureToBitmap took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());
            st1 = DateTime.Now;
            //int bitmapDataOffset = 62;
            double widthInBytes = ((signature.Width / 32) * 32) / 8;
            int height = signature.Height / 32 * 32;

            var bitmapDataLength = rawBytes.Length; // bitmapFileData.Length - bitmapDataOffset;
                                                    //byte[] bitmap = new byte[bitmapDataLength];
                                                    //Buffer.BlockCopy(bitmapFileData, bitmapDataOffset, bitmap, 0, bitmapDataLength);

            string ZPLImageDataString = BitConverter.ToString(rawBytes);
            ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

            Logger.CreateLog("ZPLImageDataString.Replace took " + DateTime.Now.Subtract(st1).TotalSeconds.ToString());

            string label = "^FO300," + startIndex.ToString() + "^GFA, " +
                           bitmapDataLength.ToString() + "," +
                           bitmapDataLength.ToString() + "," +
                           widthInBytes.ToString() + "," +
                           ZPLImageDataString;

            startIndex += height;

            var ts = DateTime.Now.Subtract(st).TotalSeconds;
            Logger.CreateLog("IncludeSignature took " + DateTime.Now.Subtract(st).TotalSeconds.ToString());
            return label;
        }
    }
}