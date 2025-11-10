using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace LaceupMigration
{
    public class RangerXlsx : DefaultXlsxProvider
    {
        public override string GetOrderXlsx(Order order)
        {
            string downloadsPath = Config.BasePath;
            string name = string.Format("Order {0} {1}_{2}.xlsx", order.PrintedOrderId, order.Client.ClientName, order.Date.ToString("MM-dd-yyyy"));
            string filePath = name.Replace('#', ' ');
            string fullPath = Path.Combine(downloadsPath, filePath);

            var list = GetExcelValues(order);

            var tempFile = Path.GetTempFileName();

            using (StreamWriter writer = new StreamWriter(tempFile))
            {
                foreach (var item in list)
                    writer.WriteLine(item.ToString());
            }

            try
            {
                DataAccess.GetExcelFile(tempFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }

            System.IO.File.Delete(tempFile);

            return fullPath;
        }

        protected override List<T> GetExcelValues(Order order)
        {
            var list = new List<T>();

            list.Add(new T("A1", "Order #"));
            list.Add(new T("B1", order.PrintedOrderId ?? ""));

            list.Add(new T("A2", "Customer Name"));
            list.Add(new T("B2", order.Client.ClientName));

            var salesman = Salesman.FullList.FirstOrDefault(x => x.Id == Config.SalesmanId);

            list.Add(new T("A3", "Order By"));
            list.Add(new T("B3", salesman != null ? salesman.Name : ""));

            list.Add(new T("A4", "Count Date"));
            list.Add(new T("B4", order.Date.ToString(Config.OrderDatePrintFormat)));

            list.Add(new T("C1", order.Client.ClientName, 12, TextStyle.Bold, TextAlignment.Center, "C1:D2"));

            var company = CompanyInfo.Companies[0];

            list.Add(new T("C4", "SELLER:", 10, TextStyle.Bold, TextAlignment.Center, "C4:D4"));
            list.Add(new T("C5", company.CompanyName.ToUpperInvariant(), 10, TextStyle.Normal, TextAlignment.Center, "C5:D5"));
            list.Add(new T("C6", "PHONE: " + company.CompanyPhone, 10, TextStyle.Bold, TextAlignment.Center, "C6:D6"));
            list.Add(new T("C7", "FAX: " + company.CompanyFax, 10, TextStyle.Bold, TextAlignment.Center, "C7:D7"));
            list.Add(new T("C8", "Del Via: Your Truck", 10, TextStyle.Normal, TextAlignment.Center, "C8:D8"));

            list.Add(new T("F1", "Vendor Name"));
            list.Add(new T("G1", company.CompanyName));

            list.Add(new T("F2", "Vendor #"));
            list.Add(new T("G2", order.Client.VendorNumber, TextStyle.Normal, TextAlignment.Left, "", "0"));

            list.Add(new T("F3", "PO #"));
            list.Add(new T("G3", order.PONumber ?? ""));

            list.Add(new T("F4", "Date"));
            list.Add(new T("G4", DateTime.Now.ToString(Config.OrderDatePrintFormat)));

            list.Add(new T("F5", "Amount"));
            list.Add(new T("G5", order.OrderTotalCost().ToString(), TextStyle.Normal, TextAlignment.Right, "", "$#,##0.00"));

            list.Add(new T("F6", "Del Date"));
            list.Add(new T("G6", order.ShipDate != DateTime.MinValue ? order.ShipDate.ToString(Config.OrderDatePrintFormat) : ""));

            list.Add(new T("F7", "Department"));
            list.Add(new T("G7", order.Department != null ? order.Department.Name : ""));

            list.Add(new T("F8", "Buyer"));
            list.Add(new T("G8", order.Client.ContactName));

            var printTerms = DataAccess.GetSingleUDF("printterms", order.Client.ExtraPropertiesAsString);

            if (!string.IsNullOrEmpty(printTerms) && printTerms == "1")
            {
                list.Add(new T("B11", "By accepting this Purchase Order, you are hereby " +
                    "acknowledging and agreeing to Forest River's Terms and Conditions " +
                    "(available online at www.forestriverinc.com/poterms and delivered to you by mail), " +
                    "which are incorporated herein and made a part hereof.", 10, TextStyle.Italic, TextAlignment.Center, "B11:G12"));
            }

            list.Add(new T("A14", "Product", TextStyle.Bold));
            list.Add(new T("B14", "Description", TextStyle.Bold));
            list.Add(new T("C14", "Customer Part #", TextStyle.Bold));
            list.Add(new T("D14", "Location", TextStyle.Bold));
            list.Add(new T("E14", "Order Qty", TextStyle.Bold));
            list.Add(new T("F14", "UoM", TextStyle.Bold));
            list.Add(new T("G14", "Unit Price", TextStyle.Bold));
            list.Add(new T("H14", "Ext. Amount", TextStyle.Bold));

            int Yindex = 15;

            double totalAmount = 0;

            foreach (var detail in order.Details)
            {
                var amount = detail.Qty * detail.Price;

                var uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == detail.Product.UoMFamily && x.IsBase);

                string department = detail.ProductDepartment;
                if (!order.Client.OneOrderPerDepartment)
                    department = order.Department != null ? order.Department.Name : "";

                list.Add(new T("A" + Yindex, detail.Product.Code));
                list.Add(new T("B" + Yindex, detail.Product.Description));
                list.Add(new T("C" + Yindex, detail.Product.GetPartNumberForCustomer(order.Client)));
                list.Add(new T("D" + Yindex, department));
                list.Add(new T("E" + Yindex, detail.Qty.ToString(), TextStyle.Normal, TextAlignment.Right, "", "0"));
                list.Add(new T("F" + Yindex, uom != null ? uom.Name : ""));
                list.Add(new T("G" + Yindex, detail.Price.ToString(), TextStyle.Normal, TextAlignment.Right, "", "#,##0.00000"));
                list.Add(new T("H" + Yindex, amount.ToString(), TextStyle.Normal, TextAlignment.Right, "", "$#,##0.00"));

                Yindex++;

                totalAmount += amount;
            }

            list.Add(new T("H" + Yindex, totalAmount.ToString(), TextStyle.Bold, TextAlignment.Right, "", "$#,##0.00"));

            return list;
        }
    }
}