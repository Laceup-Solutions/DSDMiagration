using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{

    /// <summary>
    /// Represents an open invoice
    /// </summary>
    public class Invoice 
    {
        static IList<Invoice> allOpen = new List<Invoice>();
        static IList<Invoice> over1 = new List<Invoice>();
        static IList<Invoice> over30 = new List<Invoice>();
        static IList<Invoice> over60 = new List<Invoice>();
        static IList<Invoice> over90 = new List<Invoice>();

        public static IList<Invoice> AllOpen { get { return allOpen; } }

        public static IList<Invoice> Over1 { get { return over1; } }

        public static IList<Invoice> Over30 { get { return over30; } }

        public static IList<Invoice> Over60 { get { return over60; } }

        public static IList<Invoice> Over90 { get { return over90; } }

        public static double AllOpenAmount { get; set; }

        public static double Over1Amount { get; set; }

        public static double Over30Amount { get; set; }

        public static double Over60Amount { get; set; }

        public static double Over90Amount { get; set; }

        List<InvoiceDetail> details;

        public int InvoiceId { get; set; }

        Client invoiceClient;
        public Client Client { get
            {
                if (invoiceClient == null)
                    invoiceClient = Client.Find(ClientId);
                if (invoiceClient == null)
                    invoiceClient = Client.CreateTemporalClient(ClientId);

                return invoiceClient;
            }
        }

        public string PONumber
        {
            get
            {
                if (string.IsNullOrEmpty(ExtraFields))
                    return string.Empty;

                var ex = DataAccess.GetSingleUDF("ponumber", ExtraFields);
                if (!string.IsNullOrEmpty(ex))
                    return ex;
                else
                    return string.Empty;
            }
        }

        public int ClientId { get; set; }

        public string InvoiceNumber { get; set; }

        public double Amount { get; set; }

        public double Balance { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string Comments { get; set; }

        public int InvoiceType { get; set; }

        public string SalesmanName { get; set; }

        public int SalesmanId { get; set; }

        public double Paid
        {
            get
            {
                InvoicePayment existPayment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == InvoiceId) != null));
                if (existPayment != null)
                    return existPayment.TotalPaid > Balance ? Balance : existPayment.TotalPaid;

                if(existPayment == null)
                {
                    var tempPayment = TemporalInvoicePayment.List.Where(x => x.invoiceId == this.InvoiceId).ToList();

                    if (tempPayment != null && tempPayment.Count > 0)
                    {
                        var remaining = tempPayment.Sum(x => x.amountPaid);

                        return remaining == 0 ? Balance : Balance - remaining;

                    }
                }

                return 0;
            }
        }

        static List<Invoice> invoices = new List<Invoice>();

        public static IList<Invoice> OpenInvoices
        {
            get { return invoices; }
        }

        public static void Add(Invoice invoice)
        {
            invoices.Add(invoice);
            AllOpenAmount = AllOpenAmount + invoice.Balance;
        }

        public static void Clear(int count)
        {
            Clear();

            invoices = new List<Invoice>(count);
        }

        public static void Clear()
        {
            invoices.Clear();
            Over1.Clear();
            Over30.Clear();
            Over60.Clear();
            Over90.Clear();
            allOpen.Clear();
            AllOpenAmount = Over1Amount = Over30Amount = Over60Amount = Over90Amount = 0;
        }

        public List<InvoiceDetail> Details
        {
            get
            {
                if (details == null)
                    details = InvoiceDetail.GetInvoiceDetails(this).ToList();
                return details;
            }
            set
            {
                details = value;
            }
        }

        public static Dictionary<int, int> InvoiceTypeDic = new Dictionary<int, int>();

        public string ExtraFields { get; set; }
        public string CompanyName { get; set; }
        public double Tax { get; set; }

        public string Signature { get; set; }
        public double SignatureWidth { get; set; }
        public int SignatureHeight { get; set; }
        public int SignatureSize { get; set; }
        public string SignatureAsBase64 { get; set; }
    }
}