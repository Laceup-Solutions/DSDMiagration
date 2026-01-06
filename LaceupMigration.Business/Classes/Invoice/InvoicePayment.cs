using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public enum InvoicePaymentMethod
    {
        Cash,
        Check,
        Credit_Card,
        ACH,
        Money_Order,
        Transfer,
        Zelle_Transfer
    }

    public class InvoicePayment 
    {

        string fileName;

        static int lastId = 0;

        public int Id { get; private set; }

        public Client Client { get; set; }

        public string InvoicesId { get; set; }

        public string OrderId { get; set; }

        public bool Printed { get; set; }

        public string UniqueId { get; set; }

        public DateTime DateCreated { get; set; }

        public double DiscountApplied { get; set; }

        public List<PaymentComponent> Components { get; set; }

        public bool Voided { get; set; }

        private InvoicePayment()
        {
            Components = new List<PaymentComponent>();
        }

        public InvoicePayment(Client client)
        {
            this.Client = client;
            this.Id = ++lastId;
            UniqueId = Guid.NewGuid().ToString();
            DateCreated = DateTime.Now;
            Components = new List<PaymentComponent>();
            InvoicesId = string.Empty;
            OrderId = string.Empty;
            paymentList.Add(this);
        }

        public string PaymentMethods()
        {
            List<int> methods = Components.Select(x => (int)x.PaymentMethod).Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var m in methods)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append((InvoicePaymentMethod)m);
            }
            sb.Replace("_", " ");
            return sb.ToString();
        }

        public string CheckNumbers()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in Components)
                if (!string.IsNullOrEmpty(c.Ref))
                {
                    if (sb.Length > 0)
                        sb.Append(",");
                    sb.Append(c.Ref);
                }
            return sb.ToString();
        }

        public void Delete()
        {
            // Delete from the list
            paymentList.Remove(this);
            // Delete from t he file
            if (!string.IsNullOrEmpty(fileName))
                if (File.Exists(this.fileName))
                    File.Delete(this.fileName);
        }

        public void Void()
        {
            this.Voided = true;

            Save();
        }

        public string SerializePaymentToString()
        {
            StringBuilder writer = new StringBuilder();
            string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", (char)20, Id, Client.ClientId, InvoicesId ?? string.Empty, OrderId ?? string.Empty, Printed ? "1" : "0", UniqueId, DateCreated, Voided ? "1" : "0");
            writer.Append(line);
            writer.Append(System.Environment.NewLine);
            foreach (var component in Components)
            {
                string checkNumber = component.Ref == null ? string.Empty : component.Ref.Replace((char)13, (char)32).Replace((char)10, (char)32);
                string comment = component.Comments == null ? string.Empty : component.Comments.Replace((char)13, (char)32).Replace((char)10, (char)32);
                line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}", (char)20, checkNumber, comment, component.Amount, (int)component.PaymentMethod, component.ExtraFields);
                writer.Append(line);
                writer.Append(System.Environment.NewLine);
            }
            return writer.ToString();
        }

        public void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                if (string.IsNullOrEmpty(fileName))
                    this.fileName = Path.Combine(Config.PaymentPath, Guid.NewGuid().ToString());

                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(this.fileName))
                    {
                        string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}", (char)20, Id, Client.ClientId, InvoicesId ?? string.Empty, OrderId ?? string.Empty, Printed ? "1" : "0", UniqueId, DateCreated, Voided ? "1" : "0", DiscountApplied);
                        writer.WriteLine(line);

                        foreach (var component in Components)
                        {
                            if (component.PostedDate != DateTime.MinValue)
                                component.ExtraFields = UDFHelper.SyncSingleUDF("PostedDate", component.PostedDate.Ticks.ToString(), component.ExtraFields);

                            string checkNumber = component.Ref == null ? string.Empty : component.Ref.Replace((char)13, (char)32).Replace((char)10, (char)32);
                            string comment = component.Comments == null ? string.Empty : component.Comments.Replace((char)13, (char)32).Replace((char)10, (char)32);
                            line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}", (char)20, checkNumber, comment, component.Amount, (int)component.PaymentMethod, component.ExtraFields);
                            writer.WriteLine(line);
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }

                BackgroundDataSync.SyncOrderPayment();
            }

        }

        public List<Invoice> Invoices()
        {
            var list = new List<Invoice>();

            try
            {
                if (!string.IsNullOrEmpty(InvoicesId))
                    foreach (var idAsString in InvoicesId.Split(new char[] { ',' }))
                    {

                        if (Config.SavePaymentsByInvoiceNumber)
                        {
                            var invioce = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceNumber == idAsString);
                            if (invioce != null)
                                list.Add(invioce);
                        }
                        else
                        {
                            int id = Convert.ToInt32(idAsString);

                            var invioce = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceId == id);
                            if (invioce != null)
                                list.Add(invioce);
                        }
                    }
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }
        }

        public List<Order> Orders()
        {
            var list = new List<Order>();
            if (!string.IsNullOrEmpty(this.OrderId))
                foreach (var idAsString in OrderId.Split(new char[] { ',' }))
                {
                    var order = Order.Orders.FirstOrDefault(x => x.UniqueId == idAsString);
                    if (order != null)
                        list.Add(order);
                    else
                        Logger.CreateLog("Payment has a reference to an order that was not found: payment:" + this.UniqueId + " order Id: " + idAsString);
                }
            return list;
        }

        private void DeserializePayment(StreamReader reader)
        {
            string line = reader.ReadLine();
            string[] parts = line.Split(new char[] { (char)20 });

            Id = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);

            if (lastId < Id)
                lastId = Id;

            this.Client = Client.Find(Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture));
            this.InvoicesId = parts[2];
            this.Printed = Convert.ToInt32(parts[4]) > 0;
            if (parts.Length > 3)
                this.OrderId = parts[3];
            if (parts.Length > 5)
                this.UniqueId = parts[5];
            if (parts.Length > 6)
                this.DateCreated = Convert.ToDateTime(parts[6], CultureInfo.InvariantCulture);
            if (parts.Length > 7)
                this.Voided = Convert.ToInt32(parts[7]) > 0;
            if(parts.Length > 8)
                this.DiscountApplied = Convert.ToDouble(parts[8]);

            Components.Clear();
            while ((line = reader.ReadLine()) != null)
            {
                parts = line.Split(new char[] { (char)20 });
                var c = new PaymentComponent();
                c.Ref = parts[0];
                c.Comments = parts[1];
                c.Amount = Convert.ToDouble(parts[2]);
                c.PaymentMethod = (InvoicePaymentMethod)Convert.ToInt32(parts[3]);

                if (parts.Length > 4)
                {
                    c.ExtraFields = parts[4];

                    var postedDate = UDFHelper.GetSingleUDF("PostedDate", c.ExtraFields);
                    if (!string.IsNullOrEmpty(postedDate))
                        c.PostedDate = new DateTime(Convert.ToInt64(postedDate, CultureInfo.InvariantCulture));
                }

                Components.Add(c);
            }
        }

        private bool LoadFromFile(string file)
        {
            try
            {
                this.fileName = file;
                using (StreamReader reader = new StreamReader(this.fileName))
                {
                    DeserializePayment(reader);
                }
                //check if the payment is ok, otherwise, just delete it
                if (this.Client == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        static internal void AddPaymentFromFile(string file)
        {
            InvoicePayment payment = new InvoicePayment();
            if (payment.LoadFromFile(file))
                paymentList.Add(payment);
        }

        private static List<InvoicePayment> paymentList = new List<InvoicePayment>();

        public static void AddInvoicePayment(InvoicePayment invoicePayment)
        {
            paymentList.Add(invoicePayment);
        }

        public static void RemoveInvoicePayment(InvoicePayment invoicePayment)
        {
            paymentList.Remove(invoicePayment);
        }

        public static void ClearMemory()
        {
            paymentList.Clear();
        }
        public static void ClearList()
        {
            var copy = paymentList.ToArray();
            foreach (var i in copy)
                i.Delete();
            paymentList.Clear();
        }

        public static IList<InvoicePayment> List
        {
            get
            {
                return paymentList.Where(x => !x.Voided).ToList();
            }
        }

        public static IList<InvoicePayment> ListWithVoids
        {
            get
            {
                return paymentList.ToList();
            }
        }

        public double TotalPaid
        {
            get
            {
                double total = 0;
                foreach (var item in Components)
                    total += item.Amount;

                return total;
            }
        }

        public List<string> GetPaymentComment()
        {
            List<string> comment = new List<string>();

            foreach (var item in Components)
            {
                if (string.IsNullOrEmpty(item.Comments))
                    continue;

                comment.Add(item.PaymentMethod + ": " + item.Comments);
            }

            return comment;
        }

        public static void LoadPayments()
        {
            // clean up the list of orders
            ClearMemory();
            foreach (string file in Directory.GetFiles(Config.PaymentPath))
                AddPaymentFromFile(file);
        }
    }

    public class PaymentComponent
    {
        public InvoicePaymentMethod PaymentMethod { get; set; }

        double amount = 0;
        public double Amount
        {
            get { return amount; }
            set
            {
                amount = value;
            }
        }

        public string Ref { get; set; }

        public string Comments { get; set; }

        public string ExtraFields { get; set; }

        public PaymentComponent()
        {
            Comments = string.Empty;
            Ref = string.Empty;
            ExtraFields = string.Empty;
        }

        public PaymentComponent(PaymentComponent c)
        {
            PaymentMethod = c.PaymentMethod;
            Amount = c.Amount;
            Ref = c.Ref;
            Comments = c.Comments;
            ExtraFields = c.ExtraFields;
            PostedDate = c.PostedDate;
        }

        public string BankName 
        { 
            get
            {
                if(!string.IsNullOrEmpty(ExtraFields))
                {
                    var bankName = UDFHelper.GetSingleUDF("BankName", ExtraFields);
                    if (!string.IsNullOrEmpty(bankName))
                        return bankName;
                }

                return string.Empty;
            }
        }
        public DateTime PostedDate { get; set; }
    }
}

