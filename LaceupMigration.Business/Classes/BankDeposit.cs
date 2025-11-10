





using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BankDeposit
    {
        public static BankDeposit currentDeposit;

        public BankDeposit()
        {
            UniqueId = "deposit_" + Guid.NewGuid();
            Payments = new List<InvoicePayment>();
            Comment = string.Empty;
            bankAccountId = 0;
            PostedDate = DateTime.MinValue;
            ImageId = string.Empty;
            BatchNumber = GenerateBatchNumber();

        }
        public string GenerateBatchNumber()
        {
            if (Config.CurrentBatchDate != DateTime.Today)
            {
                Config.CurrentBatchDate = DateTime.Today;
                Config.CurrentBatchId = 1;
            }
            else
                Config.CurrentBatchId++;

            Config.SaveCurrentBatchId();

            string originalId = Config.SalesmanId.ToString();
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman != null)
                originalId = salesman.OriginalId.Trim();

            return "LAC" + originalId + "-" + DateTime.Today.ToString("MMddyy", CultureInfo.InvariantCulture) + Config.CurrentBatchId.ToString("D2");
        }

        public BankDeposit(string id, List<InvoicePayment> pmts, int bankId, DateTime date, string comment, string img, bool printed, string batchNum)
        {
            UniqueId = id;
            Payments = pmts;
            Comment = comment;
            bankAccountId = bankId;
            PostedDate = date;
            ImageId = img;
            Printed = printed;
            BatchNumber = batchNum;

        }

        public string UniqueId { get; set; }

        public List<InvoicePayment> Payments { get; set; }

        public int bankAccountId { get; set; }

        public DateTime PostedDate { get; set; }

        public string Comment { get; set; }

        public string ImageId { get; set; }

        public bool Printed { get; set; }

        public double TotalAmount
        {
            get
            {
                return Payments.Sum(x => x.TotalPaid);
            }
        }
        public string BatchNumber { get; set; }

        public string PaymentListIdsAsString
        {
            get
            {
                string listAsString = string.Empty;
                foreach (var p in Payments)
                {
                    if (string.IsNullOrEmpty(listAsString))
                        listAsString = p.UniqueId.ToString();
                    else
                        listAsString += "," + p.UniqueId.ToString();
                }

                return listAsString;
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(Config.BankDepositPath))
                    return;

                using (StreamReader reader = new StreamReader(Config.BankDepositPath))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var id = parts[0];
                        var paymentList = parts[1];
                        var bankId = Convert.ToInt32(parts[2]);
                        var date = new DateTime(Convert.ToInt64(parts[3]));
                        var imageId = parts[4];
                        var comments = parts[5];

                        bool printed = false;
                        if (parts.Length > 6)
                            printed = Convert.ToInt32(parts[6]) > 0;

                        string batchNumber = string.Empty;
                        if (parts.Length > 7)
                            batchNumber = parts[7];

                        var list = new List<InvoicePayment>();
                        var string_parts = paymentList.Split(',');
                        foreach (var p in string_parts)
                        {
                            var invPayment = InvoicePayment.List.FirstOrDefault(x => x.UniqueId == p);
                            if (invPayment != null)
                                list.Add(invPayment);
                        }

                        if (list.Count == 0)
                        {
                            //remove it
                            currentDeposit = null;

                            if (File.Exists(Config.BankDepositPath))
                                File.Delete(Config.BankDepositPath);
                        }
                        else
                        {
                            var temp = new BankDeposit(id, list, bankId, date, comments, imageId, printed, batchNumber);

                            currentDeposit = temp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentDeposit = null;

                if (File.Exists(Config.BankDepositPath))
                    File.Delete(Config.BankDepositPath);
            }
        }

        public void Delete()
        {
            currentDeposit = null;

            if (File.Exists(Config.BankDepositPath))
                File.Delete(Config.BankDepositPath);

            if (Directory.Exists(Config.DepositImagesPath))
                Directory.Delete(Config.DepositImagesPath, true);
        }
        public void Save(bool fromSend = false)
        {
            if (File.Exists(Config.BankDepositPath))
                File.Delete(Config.BankDepositPath);

            using (StreamWriter writer = new StreamWriter(Config.BankDepositPath))
            {
                writer.Write(currentDeposit.UniqueId);
                writer.Write((char)20);
                writer.Write(currentDeposit.PaymentListIdsAsString);
                writer.Write((char)20);
                writer.Write(currentDeposit.bankAccountId);
                writer.Write((char)20);
                writer.Write(currentDeposit.PostedDate.Ticks);
                writer.Write((char)20);
                writer.Write(currentDeposit.ImageId);
                writer.Write((char)20);
                writer.Write(currentDeposit.Comment);
                writer.Write((char)20);
                writer.Write(currentDeposit.Printed ? "1" : "0");
                writer.Write((char)20);
                writer.Write(currentDeposit.BatchNumber ?? "");
                writer.WriteLine();
            }
        }
    }
}