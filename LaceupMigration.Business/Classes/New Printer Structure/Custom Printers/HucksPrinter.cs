using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class HucksPrinter : ZebraFourInchesPrinter1
    {
        static int tableWidth = 770;

        protected const string HucksDeliveryTicket = "HucksDeliveryTicket";
        protected const string HucksDocumentNumber = "HucksDocumentNumber";
        protected const string HucksSBL = "HucksSBL";
        protected const string HucksSBLInfo = "HucksSBLInfo";
        protected const string HucksTableHeader = "HucksTableHeader";
        protected const string HucksTableLines = "HucksTableLines";
        protected const string HucksReceivedText = "HucksReceivedText";
        protected const string HucksDeclarationText = "HucksDeclarationText";
        protected const string HucksShipperCarrier = "HucksShipperCarrier";
        protected const string HucksMelissaSignature = "HucksMelissaSignature";
        protected const string HucksSignatureLabel = "HucksSignatureLabel";
        protected const string HucksPerMelissa = "HucksPerMelissa";

        protected const string HucksLineHorizontal = "HucksLineHorizontal";
        protected const string HucksLineVertical = "HucksLineVertical";
        protected const string HucksTableBorders = "HucksTableBorders";

        const string receivedText = "RECEIVED, subject to classifications and tariffs in effect on the date of " +
            "issue of the Bill of Lading, the property described above in apparent good order, except as noted " +
            "(contents and conditions of contents of packages unknown), marked consigned, and destined as indicated " +
            "above which said carrier (the word CARRIER being understood throughout this contract as meaning any person " +
            "or corporation in possession of the of the property under contract) agrees to carry to its usual place of delivery " +
            "at said destination, if on its route, otherwise to deliver to another carrier on the route to said destination. " +
            "It is utually agreed as to each carrier of all or any of, said property overall or any portion of said route to " +
            "destination and as to each party at any time interested in all or any said property, that every service to be " +
            "performed hereunder shall be subject to all the bill of lading terms and conditions and the governing classification " +
            "on the date of shipment. Shipper hereby certifies that he is familiar with all the bill of lading terms and conditions " +
            "and the governing classification and the said terms and conditions are hereby agreed to by the shipper and accepted " +
            "for himself and his assigns.";

        const string declaration = "I hereby declare that the above named materials are fully and accurately described by " +
            "the proper shipping name and are classified, packed, marked and labeled/placarded, and are in all respects in " +
            "proper condition for transport according to applicable international and national governmental regulations.";

        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates.Add(HucksDeliveryTicket, "^CF0,20^FO40,80^FDDelivery Ticket^FS");
            linesTemplates.Add(HucksDocumentNumber, "^CF0,20^FO40,110^FD{1}^FS");
            linesTemplates.Add(HucksSBL, "^CF0,35^FO150,150^FDSTRAIGHT BILL OF LADING^FS");
            linesTemplates.Add(HucksSBLInfo, "^CF0,20" +
                "^FO600,80^GB200,150,2^FS" +
                "^FO610,100^FDFOR CHEMICAL^FS" +
                "^FO610,120^FDEMERGENCIES^FS" +
                "^FO610,140^FDCALL: CHEMTREC^FS" +
                "^FO610,160^FDDAY OF NIGHT^FS" +
                "^FO610,180^FD1-800-424-9300^FS" +
                "^FO610,200^FDCNN: 8284^FS");

            linesTemplates.Add(HucksTableHeader, "^CF0,17" +
                "^FO40,260^FDNo. of Units^FS" +
                "^FO40,275^FD& Container^FS" +
                "^FO40,290^FDType^FS" +
                "^FO160,260^FDHM^FS" +
                "^FO270,260^FDBASIC DESCRIPTION^FS" +
                "^FO200,275^FDProper Shipping Name, Hazard Class, Identification^FS" +
                "^FO200,290^FDNumber (UN or NA), Packaging Group, pdf 172.101,^FS" +
                "^FO200,305^FD172.202, 172.203^FS" +
                "^FO610,260^FDWEIGHT in^FS" +
                "^FO610,275^FDpounds (subject^FS" +
                "^FO610,290^FDto correction)^FS" +
                "^FO740,260^FDRate or^FS" +
                "^FO740,275^FDClass^FS");

            linesTemplates.Add(HucksTableLines, "^CF0,20" +
                "^FO40,{0}^FD{1}^FS" +
                "^FO160,{0}^FD{2}^FS" +
                "^FO200,{0}^FD{3}^FS" +
                "^FO610,{0}^FD{4}^FS" +
                "^FO740,{0}^FD{5}^FS");

            linesTemplates.Add(HucksReceivedText, "^CF0,17" +
                "^FO40,{0}^FD{1}^FS");

            linesTemplates.Add(HucksDeclarationText, "^CF0,17" +
                "^FO610,{0}^FD{1}^FS");

            linesTemplates.Add(HucksMelissaSignature, "^CF0,20" +
                "^FO640,{0}^FDMelissa Hucks^FS");

            linesTemplates.Add(HucksSignatureLabel, "^CF0,17" +
                "^FO660,{0}^FD Signature^FS");

            linesTemplates.Add(HucksShipperCarrier, "^CF0,25" +
                "^FO45,{0}^FDSHIPPER/CARRIER: Hucks Pool Company, Inc^FS");

            linesTemplates.Add(HucksPerMelissa, "^CF0,25" +
                "^FO45,{0}^FDPER:                     Melissa Hucks^FS");

            linesTemplates.Add(HucksLineHorizontal, "^FO{0},{1}^GB{2},0,2^FS");
            linesTemplates.Add(HucksLineVertical, "^FO{0},{1}^GB0,{2},2^FS");

            linesTemplates.Add(HucksTableBorders, "^FO{0},{1}^GB{2},{3},2^FS");
        }

        public override bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false)
        {
            if (!base.PrintOrder(order, asPreOrder))
                return false;

            if (order.Details.Count(x => x.Product.ExtraPropertiesAsString.Contains("hm=1")) > 0)
                return PrintSBL(order);

            return true;
        }

        public bool PrintSBL(Order order)
        {
            List<string> lines = new List<string>();

            int startY = 80;

            lines.AddRange(GetHeaderLines(ref startY, order));

            lines.AddRange(GetTable(ref startY, order));

            lines.Add(linesTemplates[EndLabel]);
            //Add the start label
            lines.Insert(0, string.Format(CultureInfo.InvariantCulture, linesTemplates[StartLabel], startY + 60));

            return PrintLines(lines);
        }

        private IEnumerable<string> GetHeaderLines(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksDeliveryTicket], startY));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksDocumentNumber], startY, order.PrintedOrderId));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksSBL], startY));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksSBLInfo], startY));

            return lines;
        }

        private IEnumerable<string> GetTable(ref int startY, Order order)
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableHeader], startY));

            startY = 330;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineHorizontal], 30, startY, tableWidth));
            startY += 20;

            var products = order.Details.Where(x => x.Product.ExtraPropertiesAsString.Contains("hm=1"));

            int offset = 1;

            foreach (var item in products)
            {
                var uom = DataAccess.GetSingleUDF("uom", item.Product.ExtraPropertiesAsString);
                if (uom == null)
                    uom = string.Empty;

                string gallons = DataAccess.GetSingleUDF("gallons", item.Product.ExtraPropertiesAsString);
                float gallonsConversion = 1;
                if (!string.IsNullOrEmpty(gallons))
                    float.TryParse(gallons, out gallonsConversion);
                string weight = DataAccess.GetSingleUDF("weight", item.Product.ExtraPropertiesAsString);
                float weightConversion = 1;
                if (!string.IsNullOrEmpty(weight))
                    float.TryParse(weight, out weightConversion);

                string units = item.Qty.ToString() + " " + uom;
                string description = item.Product.Description;

                var totalGallons = item.Qty * gallonsConversion;
                var totalWeight = totalGallons * weightConversion;

                string calc = totalGallons + " Gallon x " + weight + " lbs = " + totalWeight + " lbs";


                var s1 = SplitProductName(units, 10, 10).ToList();
                var s3 = SplitProductName(description, 45, 45).ToList();
                var s4 = SplitProductName(calc, 14, 14).ToList();

                for (int i = 0; i < s3.Count; i++)
                {
                    var s11 = i >= s1.Count ? "" : s1[i];
                    var s22 = i == 0 ? "X" : "";
                    var s33 = s3[i];
                    var s44 = i >= s4.Count ? "" : s4[i];
                    var s55 = i == 0 ? "60" : "";

                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableLines], startY,
                    s11, s22, s33, s44, s55));
                    startY += 20;
                }

                startY += 10;

                if (offset < products.Count())
                {
                    lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineHorizontal], 30, startY, tableWidth));
                    startY += 20;
                }

                offset++;
            }

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableBorders], 30, 250, 770, startY - 250));

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineVertical], 150, 250, startY - 250));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineVertical], 190, 250, startY - 250));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineVertical], 600, 250, startY - 250));
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineVertical], 730, 250, startY - 250));

            int y = startY;

            startY += 20;

            foreach (var item in SplitProductName(receivedText, 75, 75))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksReceivedText], startY, item));
                startY += 20;
            }

            startY += 20;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableBorders], 30, y, 600 - 30, startY - y));

            int startOfSmallSection = startY;

            startY = y;

            startY += 40;

            foreach (var item in SplitProductName(declaration, 22, 22))
            {
                lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksDeclarationText], startY, item));
                startY += 20;
            }

            startY += 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksMelissaSignature], startY));
            startY += 20;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksLineHorizontal], 640, startY, 120));
            startY += 15;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksSignatureLabel], startY));

            startY += 40;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableBorders], 600, y, 200, startY - y));

            startY += 60;

            startOfSmallSection += 10;
            startY = startOfSmallSection;

            startY += 10;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksShipperCarrier], startY));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableBorders], 30, startOfSmallSection, 550, startY - startOfSmallSection));

            startY += 15;

            startOfSmallSection = startY;

            startY += 10;
            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksPerMelissa], startY));
            startY += 30;

            lines.Add(String.Format(CultureInfo.InvariantCulture, linesTemplates[HucksTableBorders], 30, startOfSmallSection, 550, startY - startOfSmallSection));

            startY += 50;

            return lines;
        }

        protected override string GetOrderDocumentName(ref bool printExtraDocName, Order order, Client client)
        {
            string docName = "Delivery #";
            if (client.ExtraProperties != null)
            {
                var vendor = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "VENDOR");
                if (vendor != null && vendor.Item2.ToUpperInvariant() == "YES")
                {
                    docName = "Bill";
                    printExtraDocName = true;
                }
            }
            if (order.AsPresale)
            {
                docName = "Sales Order";
                printExtraDocName = false;
            }
            if (order.OrderType == OrderType.Credit)
            {
                docName = "Credit";
                printExtraDocName = true;
            }

            return docName;
        }

    }
}