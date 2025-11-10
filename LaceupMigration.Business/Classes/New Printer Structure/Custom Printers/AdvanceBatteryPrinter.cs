using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class AdvanceBatteryPrinter : ZebraFourInchesPrinter1
    {
        protected const string AdvanceBatteryContractHeader = "AdvanceBatteryContractHeader";
        protected const string AdvanceBatteryConsTableTitle = "AdvanceBatteryConsTableTitle";
        protected const string AdvanceBatteryConsTableHeader = "AdvanceBatteryConsTableHeader";
        protected const string AdvanceBatteryConsTableLine = "AdvanceBatteryConsTableLine";
        protected const string AdvanceBatteryConsTableTotal = "AdvanceBatteryConsTableTotal";

        protected const string AdvanceBatteryParTableTitle = "AdvanceBatteryParTableTitle";
        protected const string AdvanceBatteryParTableHeader = "AdvanceBatteryParTableHeader";
        protected const string AdvanceBatteryParTableLine = "AdvanceBatteryParTableLine";
        protected const string AdvanceBatteryParTableTotal = "AdvanceBatteryParTableTotal";

        protected const string AdvanceBatteryInvoiceTableSection = "AdvanceBatteryInvoiceTableSection";
        protected const string AdvanceBatteryInvoiceTableHeader = "AdvanceBatteryInvoiceTableHeader";
        protected const string AdvanceBatteryInvoiceTableLine = "AdvanceBatteryInvoiceTableLine";
        protected const string AdvanceBatteryInvoiceTableTotals = "AdvanceBatteryInvoiceTableTotals";
        protected const string AdvanceBatteryInvoiceAdjTableHeader = "AdvanceBatteryInvoiceAdjTableHeader";
        protected const string AdvanceBatteryInvoiceAdjTableLine = "AdvanceBatteryInvoiceAdjTableLine";
        protected const string AdvanceBatteryInvoiceAdjTableTotals = "AdvanceBatteryInvoiceAdjTableTotals";

        protected const string AdvanceBatteryInvoiceSubTotal = "AdvanceBatteryInvoiceSubTotal";
        protected const string AdvanceBatteryInvoiceTax = "AdvanceBatteryInvoiceTax";
        protected const string AdvanceBatteryInvoiceTotal = "AdvanceBatteryInvoiceTotal";
        protected const string AdvanceBatteryInvoicePaid = "AdvanceBatteryInvoicePaid";
        protected const string AdvanceBatteryInvoiceBalance = "AdvanceBatteryInvoiceBalance";

        protected const string AdvanceBatteryTotalPickedCores = "AdvanceBatteryTotalPickedCores";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(AdvanceBatteryContractHeader, "^FO40,{0}^ADN,36,20^FDContract^FS");
            linesTemplates.Add(AdvanceBatteryConsTableTitle, "^FO40,{0}^ADN,18,10^FDConsignment^FS");
            linesTemplates.Add(AdvanceBatteryConsTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO400,{0}^ADN,18,10^FDOld^FS" +
                "^FO470,{0}^ADN,18,10^FDNew^FS" +
                "^FO530,{0}^ADN,18,10^FDCount^FS" +
                "^FO620,{0}^ADN,18,10^FDSold^FS" +
                "^FO690,{0}^ADN,18,10^FDPrice^FS");
            linesTemplates.Add(AdvanceBatteryConsTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO400,{0}^ADN,18,10^FD{2}^FS" +
                "^FO470,{0}^ADN,18,10^FD{3}^FS" +
                "^FO530,{0}^ADN,18,10^FD{4}^FS" +
                "^FO620,{0}^ADN,18,10^FD{5}^FS" +
                "^FO690,{0}^ADN,18,10^FD{6}^FS");
            linesTemplates.Add(AdvanceBatteryConsTableTotal, "^FO250,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO400,{0}^ADN,18,10^FD{1}^FS" +
                "^FO470,{0}^ADN,18,10^FD{2}^FS" +
                "^FO530,{0}^ADN,18,10^FD{3}^FS" +
                "^FO620,{0}^ADN,18,10^FD{4}^FS");

            linesTemplates.Add(AdvanceBatteryParTableTitle, "^FO40,{0}^ADN,18,10^FDDealer Own^FS");
            linesTemplates.Add(AdvanceBatteryParTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO370,{0}^ADN,18,10^FDOld^FS" +
                "^FO430,{0}^ADN,18,10^FDNew^FS" +
                "^FO490,{0}^ADN,18,10^FDSold^FS" +
                "^FO560,{0}^ADN,18,10^FDRet^FS" +
                "^FO630,{0}^ADN,18,10^FDDmg^FS" +
                "^FO700,{0}^ADN,18,10^FDPrice^FS");
            linesTemplates.Add(AdvanceBatteryParTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO370,{0}^ADN,18,10^FD{2}^FS" +
                "^FO430,{0}^ADN,18,10^FD{3}^FS" +
                "^FO490,{0}^ADN,18,10^FD{4}^FS" +
                "^FO560,{0}^ADN,18,10^FD{5}^FS" +
                "^FO630,{0}^ADN,18,10^FD{6}^FS" +
                "^FO700,{0}^ADN,18,10^FD{7}^FS");
            linesTemplates.Add(AdvanceBatteryParTableTotal, "^FO250,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO370,{0}^ADN,18,10^FD{1}^FS" +
                "^FO430,{0}^ADN,18,10^FD{2}^FS" +
                "^FO490,{0}^ADN,18,10^FD{3}^FS" +
                "^FO560,{0}^ADN,18,10^FD{4}^FS" +
                "^FO630,{0}^ADN,18,10^FD{5}^FS");

            linesTemplates.Add(AdvanceBatteryInvoiceTableSection, "^FO40,{0}^ADN,18,10^FD{1}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO530,{0}^ADN,18,10^FDQty^FS" +
                "^FO600,{0}^ADN,18,10^FDPrice^FS" +
                "^FO690,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO530,{0}^ADN,18,10^FD{2}^FS" +
                "^FO600,{0}^ADN,18,10^FD{3}^FS" +
                "^FO690,{0}^ADN,18,10^FD{4}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceTableTotals, "^FO400,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO530,{0}^ADN,18,10^FD{1}^FS" +
                "^FO690,{0}^ADN,18,10^FD{2}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceAdjTableHeader, "^FO40,{0}^ADN,18,10^FDProduct^FS" +
                "^FO460,{0}^ADN,18,10^FDAge^FS" +
                "^FO530,{0}^ADN,18,10^FDQty^FS" +
                "^FO600,{0}^ADN,18,10^FDPrice^FS" +
                "^FO690,{0}^ADN,18,10^FDTotal^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceAdjTableLine, "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO460,{0}^ADN,18,10^FD{2}^FS" +
                "^FO530,{0}^ADN,18,10^FD{3}^FS" +
                "^FO600,{0}^ADN,18,10^FD{4}^FS" +
                "^FO690,{0}^ADN,18,10^FD{5}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceAdjTableTotals, "^FO350,{0}^ADN,18,10^FDTotals:^FS" +
                "^FO530,{0}^ADN,18,10^FD{1}^FS" +
                "^FO690,{0}^ADN,18,10^FD{2}^FS");

            linesTemplates.Add(AdvanceBatteryInvoiceSubTotal, "^FO40,{0}^ADN,36,20^FD       Subtotal: {1}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceTax, "^FO40,{0}^ADN,36,20^FD            Tax: {1}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceTotal,    "^FO40,{0}^ADN,36,20^FD  Invoice Total: {1}^FS");
            linesTemplates.Add(AdvanceBatteryInvoicePaid,    "^FO40,{0}^ADN,36,20^FD  Total Payment: {1}^FS");
            linesTemplates.Add(AdvanceBatteryInvoiceBalance, "^FO40,{0}^ADN,36,20^FDCurrent Balance: {1}^FS");

            linesTemplates.Add(AdvanceBatteryTotalPickedCores, "^FO40,{0}^ADN,36,20^FD   Cores Picked: {1}^FS");

            linesTemplates[OrderDetailsHeader]= "^FO40,{0}^ADN,18,10^FDPRODUCT^FS" +
                "^FO450,{0}^ADN,18,10^FDQTY^FS" +
                "^FO540,{0}^ADN,18,10^FDPRICE^FS" +
                "^FO680,{0}^ADN,18,10^FDTOTAL^FS";
            linesTemplates[OrderDetailsLines]= "^FO40,{0}^ADN,18,10^FD{1}^FS" +
                "^FO450,{0}^ADN,18,10^FD{2}^FS" +
                "^FO540,{0}^ADN,18,10^FD{4}^FS" +
                "^FO680,{0}^ADN,18,10^FD{3}^FS";
        }

        public override bool PrintFullConsignment(Order order, bool asPreOrder)
        {
            return PrintConsignment(order, asPreOrder, true, false, false);
        }

        public override bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false)
        {
            foreach (var item in order.Details)
            {
                var detail = ConsStruct.GetStructFromDetail(item);
                if (detail.Updated && (detail.OldValue != detail.NewValue || detail.Price != detail.NewPrice))
                {
                    printcontract = true;
                    break;
                }
            }

            if (printcontract && !PrintConsignmentContract(order, asPreOrder))
                return false;

            if (printCounting && !PrintConsignmentInvoice(order, asPreOrder))
                return false;

            return true;
        }

        #region Contract

        protected override bool PrintConsignmentContract(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            startY += font36Separation;

            lines.AddRange(GetConsignmentContractHeaderLines(ref startY, order));

            startY += 50;

            lines.AddRange(GetConsignmentContractTable(ref startY, order));

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order, false));
            else
                lines.AddRange(GetConsignmentContractFooterRows(ref startY, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected override IEnumerable<string> GetConsignmentContractHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryContractHeader], startY, order.PrintedOrderId));
            startY += font36Separation;

            lines.AddRange(GetConsignmentHeaderLines(ref startY, order));

            return lines;
        }

        protected override IEnumerable<string> GetConsignmentContractTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            List<TC> consLines = new List<TC>();
            List<TC> parLines = new List<TC>();

            foreach (var item in order.Details)
            {
                var st = ConsStruct.GetStructFromDetail(item);
                var tc = new TC() { Product = item.Product };
                tc.OldValue = st.OldValue;
                tc.NewValue = st.Updated ? st.NewValue : st.OldValue;
                tc.Count = st.Count;
                tc.Sold = st.Sold;
                tc.Ret = st.Return;
                tc.Dmg = st.Damaged;
                tc.Price = st.FromPar ? st.Price : st.NewPrice;

                if (item.ParLevelDetail)
                    parLines.Add(tc);
                else
                    consLines.Add(tc);
            }

            lines.AddRange(GetConsContractTable(ref startY, order, consLines));

            startY += 60;

            lines.AddRange(GetDealerOwnTable(ref startY, order, parLines));

            startY += font18Separation;

            return lines;
        }

        private IEnumerable<string> GetConsContractTable(ref int startY, Order order, List<TC> collection)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryConsTableTitle], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryConsTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalOld = 0;
            float totalNew = 0;
            float totalCounted = 0;
            float totalSold = 0;

            foreach (var detail in SortedDetails(collection))
            {
                int offset = 0;
                foreach (var name in SplitProductName(GetProductValue(detail.Product), 28, 28))
                {
                    if (offset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryConsTableLine], startY,
                            name,
                            detail.OldValue,
                            detail.NewValue,
                            detail.Count,
                            detail.Sold,
                            detail.Price.ToCustomString()));
                    }
                    else if (Config.PrintTruncateNames)
                        break;
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryConsTableLine], startY,
                            name,
                            "",
                            "",
                            "",
                            "",
                            ""));
                    }

                    startY += font18Separation;
                    offset++;
                }

                totalOld += detail.OldValue;
                totalNew += detail.NewValue;
                totalCounted += detail.Count;
                totalSold += detail.Sold;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryConsTableTotal], startY,
                            totalOld,
                            totalNew,
                            totalCounted,
                            totalSold));
            startY += font18Separation;

            return lines;
        }

        private IEnumerable<string> GetDealerOwnTable(ref int startY, Order order, List<TC> collection)
        {
            List<string> lines = new List<string>();

            if (order.Details.All(x => !x.ParLevelDetail))
                return lines;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryParTableTitle], startY));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryParTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalOld = 0;
            float totalNew = 0;
            float totalSold = 0;
            float totalRet = 0;
            float totalDmg = 0;

            foreach (var detail in SortedDetails(collection))
            {
                int offset = 0;
                foreach (var name in SplitProductName(detail.Product.Code, 26, 26))
                {
                    if (offset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryParTableLine], startY,
                            name,
                            detail.OldValue,
                            detail.NewValue,
                            detail.Sold,
                            detail.Ret,
                            detail.Dmg,
                            detail.Price.ToCustomString()));
                    }
                    else if (Config.PrintTruncateNames)
                        break;
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryParTableLine], startY,
                            name,
                            "",
                            "",
                            "",
                            "",
                            "",
                            ""));
                    }

                    startY += font18Separation;
                }

                totalOld += detail.OldValue;
                totalNew += detail.NewValue;
                totalSold += detail.Sold;
                totalRet += detail.Ret;
                totalDmg += detail.Dmg;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryParTableTotal], startY,
                            totalOld,
                            totalNew,
                            totalSold,
                            totalRet,
                            totalDmg));
            startY += font18Separation;

            return lines;
        }

        #endregion

        #region Invoice

        protected override bool PrintConsignmentInvoice(Order order, bool asPreOrder)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            var logoLabel = GetLogoLabel(ref startY, order);
            if (!string.IsNullOrEmpty(logoLabel))
            {
                lines.Add(logoLabel);
            }

            startY += font36Separation;

            lines.AddRange(GetConsignmentInvoiceHeaderLines(ref startY, order));

            startY += font36Separation;

            lines.AddRange(GetConsignmentInvoiceTable(ref startY, order));

            startY += font36Separation;

            lines.AddRange(GetConsignmentInvoiceTotals(ref startY, order));

            if (order.SignaturePoints != null && order.SignaturePoints.Count > 0)
                lines.AddRange(GetConsignmentSignatureLines(ref startY, order, true));
            else
                lines.AddRange(GetFooterRows(ref startY, asPreOrder));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        protected override IEnumerable<string> GetConsignmentInvoiceHeaderLines(ref int startY, Order order)
        {
            var lines = base.GetConsignmentInvoiceHeaderLines(ref startY, order).ToList();

            startY += font36Separation;

            if (!string.IsNullOrEmpty(order.PONumber))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                startY += font36Separation;
            }
            else if (Config.AutoGeneratePO)
            {
                order.PONumber = Config.SalesmanId.ToString("D2") + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + order.OrderId.ToString("D2");

                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[PONumber], startY, order.PONumber));
                startY += font36Separation;
            }

            return lines;
        }

        protected override IEnumerable<string> GetConsignmentInvoiceTable(ref int startY, Order order)
        {
            return GetInvoiceFromStructTable(ref startY, order);
        }

        private IEnumerable<string> GetInvoiceTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            List<T> sales = new List<T>();
            List<T> returns = new List<T>();
            List<T> damageds = new List<T>();
            List<T> cores = new List<T>();
            List<T> rotations = new List<T>();
            List<T> adjustments = new List<T>();
            List<T> tax = new List<T>();

            foreach (var item in order.Details)
            {
                float sold = 0;
                if (item.Qty > 0 && !item.IsCredit)
                {
                    sold = item.Qty;
                    sales.Add(new T() { Product = item.Product, Qty = item.Qty, Price = item.Price });
                    var t = GetTaxForDetail(order, item, item.Qty, false);
                    if (t != null)
                        tax.Add(t);
                }
                if (item.Qty > 0 && item.IsCredit && !item.Damaged)
                {
                    returns.Add(new T() { Product = item.Product, Qty = item.Qty, Price = item.Price * -1 });
                    var t = GetTaxForDetail(order, item, item.Qty, true);
                    if (t != null)
                        tax.Add(t);
                }
                if (item.Qty > 0 && item.IsCredit && item.Damaged)
                {
                    damageds.Add(new T() { Product = item.Product, Qty = item.Qty, Price = item.Price * -1 });
                    var t = GetTaxForDetail(order, item, item.Qty, true);
                    if (t != null)
                        tax.Add(t);
                }

                var core = GetCoreForDetail(order, item, sold);
                if (core != null)
                    cores.Add(core);

                var rotated = GetRotateForDetail(order, item);
                if (rotated != null)
                    rotations.Add(rotated);

                var adj = GetAdjustmentForDetail(order, item);
                if (adj != null)
                    adjustments.Add(adj);
            }

            lines.AddRange(GetInvoiceTableSection(ref startY, sales, "Sales"));

            lines.AddRange(GetInvoiceTableSection(ref startY, returns, "Credit Return"));

            lines.AddRange(GetInvoiceTableSection(ref startY, damageds, "Credit Damaged"));

            lines.AddRange(GetInvoiceTableSection(ref startY, tax, "Tax"));

            lines.AddRange(GetInvoiceCoreTableSection(ref startY, cores, "Cores"));

            float totalPicked = cores.Sum(x => x.PickedCores);

            lines.AddRange(GetInvoiceTableSection(ref startY, rotations, "Rotations"));

            lines.AddRange(GetInvoiceAdjustmentTableSection(ref startY, adjustments, "Adjustments"));

            string s1;
            s1 = totalPicked.ToString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryTotalPickedCores], startY,
                s1));
            startY += font36Separation;

            return lines;
        }

        private IEnumerable<string> GetInvoiceFromStructTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            List<T> sales = new List<T>();
            List<T> returns = new List<T>();
            List<T> damageds = new List<T>();
            List<T> cores = new List<T>();
            List<T> rotations = new List<T>();
            List<T> adjustments = new List<T>();
            List<T> tax = new List<T>();

            foreach (var item in order.Details)
            {
                var detail = ConsStruct.GetStructFromDetail(item);

                if (detail.Sold > 0)
                {
                    sales.Add(new T() { Product = item.Product, Qty = detail.Sold, Price = detail.Price });
                    var t = GetTaxForDetail(order, item, detail.Sold, false);
                    if (t != null)
                        tax.Add(t);
                }
                if (detail.Return > 0)
                {
                    returns.Add(new T() { Product = item.Product, Qty = detail.Return, Price = detail.Price * -1 });
                    var t = GetTaxForDetail(order, item, detail.Return, true);
                    if (t != null)
                        tax.Add(t);
                }
                if (detail.Damaged > 0)
                {
                    damageds.Add(new T() { Product = item.Product, Qty = detail.Damaged, Price = detail.Price * -1 });
                    var t = GetTaxForDetail(order, item, detail.Damaged, true);
                    if (t != null)
                        tax.Add(t);
                }

                var core = GetCoreForDetail(order, item, detail.Sold);
                if (core != null)
                    cores.Add(core);

                var rotated = GetRotateForDetail(order, item);
                if (rotated != null)
                    rotations.Add(rotated);

                var adj = GetAdjustmentForDetail(order, item);
                if (adj != null)
                    adjustments.Add(adj);
            }

            lines.AddRange(GetInvoiceTableSection(ref startY, sales, "Sales"));

            lines.AddRange(GetInvoiceTableSection(ref startY, returns, "Credit Return"));

            lines.AddRange(GetInvoiceTableSection(ref startY, damageds, "Credit Damaged"));

            lines.AddRange(GetInvoiceTableSection(ref startY, tax, "Tax"));

            lines.AddRange(GetInvoiceCoreTableSection(ref startY, cores, "Cores"));

            float totalPicked = cores.Sum(x => x.PickedCores);

            lines.AddRange(GetInvoiceTableSection(ref startY, rotations, "Rotations"));

            lines.AddRange(GetInvoiceAdjustmentTableSection(ref startY, adjustments, "Adjustments"));

            string s1;
            s1 = totalPicked.ToString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryTotalPickedCores], startY,
                s1));
            startY += font36Separation;

            return lines;
        }

        T GetCoreForDetail(Order order, OrderDetail detail, float sold)
        {
            var core = DataAccess.GetSingleUDF("coreQty", detail.ExtraFields);
            var coreId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "core");

            if (string.IsNullOrEmpty(core) || coreId == null)
                return null;

            var relatedCore = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == coreId.Item2);

            if (relatedCore == null)
                return null;

            var coreQty = Convert.ToDouble(core);

            var qty = sold - coreQty;

            var corePrice = Product.GetPriceForProduct(relatedCore, order, false, false);

            bool chargeCore = true;

            if (order.Client.NonVisibleExtraProperties != null)
            {
                var xC = order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLowerInvariant() == "corepaid");
                if (xC != null && xC.Item2.ToLowerInvariant() == "n")
                    chargeCore = false;
            }

            if (!chargeCore)
                corePrice = 0;

            if (Config.CoreAsCredit)
            {
                qty = coreQty;
                corePrice *= -1;
            }
            else if (qty < 0)
            {
                qty *= -1;
                corePrice *= -1;
            }

            return new T() { Product = relatedCore, Qty = (float)qty, Price = corePrice, PickedCores = (float)coreQty };
        }

        T GetTaxForDetail(Order order, OrderDetail detail, float sold, bool isCredit)
        {
            if (sold == 0)
                return null;

            var item = order.Client.ExtraProperties != null ? order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpper() == "USERELATED") : null;
            bool useRelated = item == null;

            if (!useRelated)
                return null;

            var related = GetRelatedProduct(detail.Product);

            if (related == null)
                return null;

            var relatedPrice = Product.GetPriceForProduct(related, order, false, false);
            if (isCredit)
                relatedPrice *= -1;

            return new T() { Product = related, Qty = sold, Price = relatedPrice };
        }

        T GetRotateForDetail(Order order, OrderDetail detail)
        {
            var rotation = DataAccess.GetSingleUDF("rotatedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(rotation))
                return null;

            var qty = int.Parse(rotation);

            if (qty == 0)
                return null;

            var rotatedId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "rotated");

            if (rotatedId == null)
                return null;

            var rotated = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == rotatedId.Item2);

            if (rotated == null)
                return null;

            var price = Product.GetPriceForProduct(rotated, order, false, false);
            if (!Config.ChargeBatteryRotation)
                price = 0;

            return new T() { Product = rotated, Qty = qty, Price = price };
        }

        T GetAdjustmentForDetail(Order order, OrderDetail detail)
        {
            var adjQty = DataAccess.GetSingleUDF("adjustedQty", detail.ExtraFields);

            if (string.IsNullOrEmpty(adjQty))
                return null;

            int time = 0;
            if (Config.WarrantyPerClient)
            {
                time = order.GetIntWarrantyPerClient(detail.Product);
                if (time == 0)
                    return null;
            }

            var adjId = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "adjustment");

            if (adjId == null)
                return null;

            var adjustment = Product.Products.FirstOrDefault(x => x.ProductId.ToString() == adjId.Item2);

            if (adjustment == null)
                return null;

            if (!Config.WarrantyPerClient)
            {
                var timeSt = detail.Product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1 == "time");

                if (timeSt == null)
                    return null;

                time = int.Parse(timeSt.Item2);
            }

            var ws = adjQty.Split(',');

            var adjPrice = Product.GetPriceForProduct(adjustment, order, false, false);

            List<T> warranties = new List<T>();

            foreach (var item in ws)
            {
                int x = int.Parse(item);

                var price = adjPrice;

                if (x <= time)
                    price = 0;

                warranties.Add(new T() { Age = x, Qty = 1, Price = price });
            }

            return new T() { Product = adjustment, Warranties = warranties };
        }

        Product GetRelatedProduct(Product product)
        {
            int relatedId = 0;

            foreach (var p in product.ExtraProperties)
            {
                if (p.Item1.ToLower() == "relateditem")
                {
                    relatedId = Convert.ToInt32(p.Item2);
                    break;
                }
            }

            return Product.Find(relatedId, true);
        }

        private IEnumerable<string> GetInvoiceTableSection(ref int startY, List<T> collection, string v)
        {
            List<string> lines = new List<string>();

            if (collection.Count == 0)
                return lines;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableSection], startY, v));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalQty = 0;
            double total = 0;

            foreach (var item in SortedDetails(collection))
            {
                int offset = 0;
                foreach (var name in SplitProductName(GetProductValue(item.Product), 40, 40))
                {
                    if (offset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableLine], startY,
                            name,
                            item.Qty,
                            item.Price.ToCustomString(),
                            item.Total.ToCustomString()));
                    }
                    else if (Config.PrintTruncateNames)
                        break;
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableLine], startY,
                            name,
                            "",
                            "",
                            ""));
                    }

                    startY += font18Separation;
                    offset++;
                }

                totalQty += item.Qty;
                total += item.Total;
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableTotals], startY,
                totalQty,
                total.ToCustomString()));

            startY += font36Separation;

            return lines;
        }

        private IEnumerable<string> GetInvoiceCoreTableSection(ref int startY, List<T> collection, string v)
        {
            List<string> lines = new List<string>();

            if (collection.Count(x => x.Qty > 0) == 0)
                return lines;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableSection], startY, v));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalQty = 0;
            double total = 0;
            float totalPicked = 0;

            foreach (var item in SortedDetails(collection))
            {
                totalPicked += item.PickedCores;

                if (item.Qty == 0)
                    continue;

                int offset = 0;
                foreach (var name in SplitProductName(GetProductValue(item.Product), 40, 40))
                {
                    if (offset == 0)
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableLine], startY,
                            name,
                            item.Qty,
                            item.Price.ToCustomString(),
                            item.Total.ToCustomString()));
                    }
                    else if (Config.PrintTruncateNames)
                        break;
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableLine], startY,
                            name,
                            "",
                            "",
                            ""));
                    }

                    startY += font18Separation;
                    offset++;
                }

                totalQty += item.Qty;
                total += item.Total;

            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableTotals], startY,
                totalQty,
                total.ToCustomString()));
            startY += font36Separation;

            return lines;
        }

        private IEnumerable<string> GetInvoiceAdjustmentTableSection(ref int startY, List<T> collection, string v)
        {
            List<string> lines = new List<string>();

            if (collection.Count == 0)
                return lines;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTableSection], startY, v));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceAdjTableHeader], startY));
            startY += font18Separation;

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            float totalQty = 0;
            double total = 0;

            foreach (var item1 in SortedDetails(collection))
            {
                int offset = 0;
                foreach (var name in SplitProductName(GetProductValue(item1.Product), 40, 40))
                {
                    if (offset < item1.Warranties.Count)
                    {
                        var item = item1.Warranties[offset];

                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceAdjTableLine], startY,
                                name,
                                item.Age,
                                item.Qty,
                                item.Price.ToCustomString(),
                                item.TotalAdj.ToCustomString()));

                        totalQty += item.Qty;
                        total += item.TotalAdj;

                        startY += font18Separation;
                    }
                    else
                    {
                        lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceAdjTableLine], startY,
                                name,
                                "",
                                "",
                                "",
                                ""));
                        startY += font18Separation;
                    }

                    offset++;
                }

                for (int i = offset; i < item1.Warranties.Count; i++)
                {
                    var item = item1.Warranties[i];

                    lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceAdjTableLine], startY,
                            "",
                            item.Age,
                            item.Qty,
                            item.Price.ToCustomString(),
                            item.TotalAdj.ToCustomString()));

                    totalQty += item.Qty;
                    total += item.TotalAdj;

                    startY += font18Separation;
                }
            }

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceAdjTableTotals], startY,
                totalQty,
                total.ToCustomString()));

            startY += font36Separation;

            return lines;
        }


        private IEnumerable<string> GetConsignmentInvoiceTotals(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            string s1;

            var subtotal = order.CalculateItemCost();

            s1 = subtotal.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceSubTotal], startY, s1));
            startY += font36Separation;

            var tax = order.CalculateTax();

            s1 = tax.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTax], startY, s1));
            startY += font36Separation;

            var orderTotal = order.OrderTotalCost();

            s1 = orderTotal.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceTotal], startY, s1));
            startY += font36Separation;

            double paid = 0;
            var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
            if (payment != null)
            {
                var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                paid = parts.Sum(x => x.Amount);
            }

            s1 = paid.ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoicePaid], startY, s1));
            startY += font36Separation;

            s1 = (orderTotal - paid).ToCustomString();
            s1 = new string(' ', 14 - s1.Length) + s1;
            lines.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[AdvanceBatteryInvoiceBalance], startY, s1));
            startY += font36Separation;

            return lines;
        }

        #endregion

        public virtual string GetProductValue(Product product)
        {
            return product.Code;
        }

        class T
        {
            public Product Product { get; set; }
            public float Qty { get; set; }
            public double Price { get; set; }
            public double Total
            {
                get
                {
                    return double.Parse(Math.Round(Convert.ToDecimal(Price * Qty), Config.Round).ToCustomString(), NumberStyles.Currency);
                }
            }
            public double TotalAdj
            {
                get
                {
                    return double.Parse(Math.Round(Convert.ToDecimal(Price * Age), Config.Round).ToCustomString(), NumberStyles.Currency);
                }
            }
            public int Age { get; set; }
            public List<T> Warranties { get; set; }
            public float PickedCores { get; set; }
        }

        class TC
        {
            public Product Product { get; set; }
            public float OldValue { get; set; }
            public float NewValue { get; set; }
            public float Count { get; set; }
            public float Sold { get; set; }
            public float Ret { get; set; }
            public float Dmg { get; set; }
            public double Price { get; set; }
        }

        class TSort<T1>
        {
            public string CategoryName { get; set; }

            public T1 HoldedValue { get; set; }
        }

        IQueryable<TC> SortedDetails(IList<TC> lines)
        {
            IQueryable<TC> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<TSort<TC>>();
                    foreach (var od in lines)
                    {
                        var t = new TSort<TC>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<TSort<TC>>();
                    foreach (var od in lines)
                    {
                        var t = new TSort<TC>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }

        IQueryable<T> SortedDetails(IList<T> lines)
        {
            IQueryable<T> retList;
            switch (Config.PrintInvoiceSort.ToLowerInvariant())
            {
                case "originalid":
                    retList = lines.OrderBy(x => x.Product.OriginalId).AsQueryable();
                    break;
                case "name":
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
                case "category":
                    retList = lines.OrderBy(x => x.Product.CategoryId).ThenBy(x => x.Product.Name).AsQueryable();
                    break;
                case "categorynameorder":
                    var list1 = new List<TSort<T>>();
                    foreach (var od in lines)
                    {
                        var t = new TSort<T>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list1.Add(t);
                    }
                    retList = list1.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.OrderInCategory).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "categoryname":
                    var list = new List<TSort<T>>();
                    foreach (var od in lines)
                    {
                        var t = new TSort<T>();
                        var c = Category.Categories.FirstOrDefault(x => x.CategoryId == od.Product.CategoryId);
                        t.CategoryName = c == null ? string.Empty : c.Name;
                        t.HoldedValue = od;
                        list.Add(t);
                    }
                    retList = list.OrderBy(x => x.CategoryName).ThenBy(x => x.HoldedValue.Product.Name).Select(x => x.HoldedValue).AsQueryable();
                    break;
                case "upc":
                    retList = lines.OrderBy(x => x.Product.Upc).AsQueryable();
                    break;
                case "price":
                    retList = lines.OrderBy(x => x.Product.PriceLevel0).AsQueryable();
                    break;
                default:
                    retList = lines.OrderBy(x => x.Product.Name).AsQueryable();
                    break;
            }

            var discounts = retList.ToList().Where(x => x.Product.ProductType == ProductType.Discount).ToList();
            if (discounts.Count > 0)
            {
                // move the discounts to last
                var list = retList.ToList();
                foreach (var discount in discounts)
                    list.Remove(discount);
                list.AddRange(discounts);
                retList = list.AsQueryable();
            }
            if (Config.DefaultItem > 0)
            {
                var list = retList.ToList();
                var detail = list.FirstOrDefault(x => x.Product.ProductId == Config.DefaultItem);
                if (detail != null)
                {
                    list.Remove(detail);
                    list.Add(detail);
                    return list.AsQueryable();
                }
            }
            return retList;
        }
    }
}