





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class BankAccount
    {
        public static List<BankAccount> List = new List<BankAccount>();
        public int Id { get; set; }
        public string Name { get; set; }
        public string BankNumber { get; set; }

        public static List<string> bankListInDevice = new List<string>();
        public static void LoadBanks()
        {
            try
            {
                bankListInDevice.Clear();

                var splitBanks = Config.SavedBanks.Split("|");

                foreach (var p in splitBanks)
                {
                    if(!string.IsNullOrEmpty(p))
                        bankListInDevice.Add(p);
                }

                bankListInDevice.Insert(0, "None");

                List.Clear();

                if (!File.Exists(Config.BanksAccountFile))
                    return;

                using (StreamReader reader = new StreamReader(Config.BanksAccountFile))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var bankId = Convert.ToInt32(parts[0]);
                        var bankName = parts[1];
                        var bankNumber = parts[2];

                        var bankAccount = new BankAccount()
                        {
                            Id = bankId,
                            Name = bankName,
                            BankNumber = bankNumber
                        };

                        List.Add(bankAccount);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void AddSavedBank(string bankName)
        {
            bankListInDevice.Add(bankName);

            if (string.IsNullOrEmpty(Config.SavedBanks))
                Config.SavedBanks = bankName;
            else
                Config.SavedBanks += "|" + bankName;

            Config.SaveSettings();
        }
    }
}