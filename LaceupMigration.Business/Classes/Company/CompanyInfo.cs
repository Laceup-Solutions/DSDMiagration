using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaceupMigration
{
    public class CompanyInfo
    {
        public int CompanyId { get; set; }

        public string CompanyName { get; set; }

        public string CompanyAddress1 { get; set; }

        public string CompanyAddress2 { get; set; }

        public string CompanyPhone { get; set; }

        public string Vendor { get; set; }

        public string DUNS { get; set; }

        public string Location { get; set; }

        public string CommId { get; set; }

        public string CompanyFax { get; set; }

        public string CompanyEmail { get; set; }

        public string CompanyLicenses { get; set; }

        public string DexUOM { get; set; }

        public bool FromFile { get; set; }

        public string ExtraFields { get; set; }

        public bool IsDefault { get; set; }

        #region test
        public int LogoId { get; set; }
        public string CompanyLogo { get; set; }
        public int CompanyLogoWidth { get; set; }
        public int CompanyLogoHeight { get; set; }
        public int CompanyLogoSize { get; set; }

        public string CompanyLogoPath { get; set; }
        public string PaymentClientId { get; set; }
        public string PaymentClientSecret { get; set; }
        public string PaymentMerchant { get; set; }

        public string BottomTextPrint { get; set; }
        #endregion

        public static IList<CompanyInfo> Companies { get { return companies; } }

        static List<CompanyInfo> companies = new List<CompanyInfo>();

        public static CompanyInfo SelectedCompany { get; set; }

        public static void Clear(int count)
        {
            Clear();
            if (count > 0)
                companies.Capacity = count;
        }

        public static void Clear()
        {
            companies.Clear();
        }

        public static void Remove(bool fromFile)
        {
            companies.RemoveAll(x => x.FromFile == fromFile);
        }

        public static bool ShowNewPayments()
        {
            var master = GetMasterCompany();

            if (master == null)
                return false;

            if (string.IsNullOrEmpty(master.PaymentClientSecret))
                return false;

            if (string.IsNullOrEmpty(master.PaymentClientId))
                return false;

            if (string.IsNullOrEmpty(master.PaymentMerchant))
                return false;

            return true;
        }


        public static string GetMasterClientSecretId()
        {
            string masterSecret = string.Empty;

            var master = GetMasterCompany();

            if (master == null)
                return masterSecret;

            return master.PaymentClientSecret;
        }

        public static string GetMasterClientId()
        {
            string masterClientId = string.Empty;

            var master = GetMasterCompany();

            if (master == null)
                return masterClientId;

            return master.PaymentClientId;
        }

        public static string GetMasterMerchantId()
        {
            string masterMerchantId = string.Empty;

            var master = GetMasterCompany();

            if (master == null)
                return masterMerchantId;

            return master.PaymentMerchant;
        }

        public static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    string tempFile = Config.CompanyInfoStoreFile;
                    
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                    
                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        foreach (var ci in companies)
                        {
                            writer.Write(ci.CompanyName);                       //0
                            writer.Write((char)20);
                            writer.Write(ci.CompanyAddress1);                   //1
                            writer.Write((char)20);
                            writer.Write(ci.CompanyAddress2);                   //2
                            writer.Write((char)20);
                            writer.Write(ci.CompanyPhone ?? string.Empty);      //3
                            writer.Write((char)20);
                            writer.Write(ci.DUNS ?? string.Empty);              //4
                            writer.Write((char)20);
                            writer.Write(ci.Location ?? string.Empty);          //5
                            writer.Write((char)20);
                            writer.Write(ci.CommId ?? string.Empty);            //6
                            writer.Write((char)20);
                            writer.Write(ci.CompanyFax ?? string.Empty);        //7
                            writer.Write((char)20);
                            writer.Write(ci.CompanyEmail ?? string.Empty);      //8
                            writer.Write((char)20);
                            writer.Write(ci.DexUOM ?? string.Empty);            //9
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLicenses ?? string.Empty);   //10
                            writer.Write((char)20);
                            writer.Write(ci.FromFile ? "1" : "0");              //11
                            writer.Write((char)20);
                            writer.Write(ci.ExtraFields ?? string.Empty);       //12
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLogo ?? string.Empty);       //13
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLogoHeight);                 //14
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLogoSize);                   //15
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLogoWidth);                  //16
                            writer.Write((char)20);
                            writer.Write(ci.LogoId);                            //17
                            writer.Write((char)20);
                            writer.Write(ci.IsDefault ? "1" : "0");                //18
                            writer.Write((char)20);
                            writer.Write(ci.CompanyLogoPath);                   //19
                            writer.Write((char)20);
                            writer.Write(ci.PaymentClientSecret);                   //20
                            writer.Write((char)20);
                            writer.Write(ci.PaymentClientId);                   //21
                            writer.Write((char)20);
                            writer.Write(ci.PaymentMerchant);                   //22
                            writer.Write((char)20);
                            writer.Write(ci.BottomTextPrint);                   //23

                            writer.WriteLine();

                        }
                        writer.Close();
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void Load()
        {
            companies.Clear();
            try
            {
                // Ensure the directory exists before checking for the file
                string directory = null;
                try
                {
                    directory = Path.GetDirectoryName(Config.CompanyInfoStoreFile);
                }
                catch (Exception dirEx)
                {
                    // If Path.GetDirectoryName throws, log and skip file loading
                    Logger.CreateLog($"Error getting directory name for CompanyInfoStoreFile: {dirEx.Message}");
                    return;
                }

                if (string.IsNullOrEmpty(directory))
                {
                    Logger.CreateLog("CompanyInfoStoreFile directory is null or empty");
                    return;
                }

                // Ensure directory exists
                if (!Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception createEx)
                    {
                        Logger.CreateLog($"Error creating directory {directory}: {createEx.Message}");
                        return;
                    }
                }

                // Check if file exists - wrap in try-catch as File.Exists can throw on some platforms
                bool fileExists = false;
                try
                {
                    fileExists = File.Exists(Config.CompanyInfoStoreFile);
                }
                catch (Exception fileEx)
                {
                    // If File.Exists throws, log and assume file doesn't exist
                    Logger.CreateLog($"Error checking if CompanyInfoStoreFile exists: {fileEx.Message}");
                    fileExists = false;
                }

                if (fileExists)
                {
                    using (StreamReader reader = new StreamReader(Config.CompanyInfoStoreFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            try
                            {
                                string[] parts = line.Split(new char[] { (char)20 });
                                var ci = new CompanyInfo()
                                {
                                    CompanyName = parts[0],
                                    CompanyAddress1 = parts[1],
                                    CompanyAddress2 = parts[2],
                                    CompanyPhone = parts[3]
                                };
                                if (parts.Length > 4)
                                    ci.DUNS = parts[4];
                                if (parts.Length > 5)
                                    ci.Location = parts[5];
                                if (parts.Length > 6)
                                    ci.CommId = parts[6];
                                if (parts.Length > 7)
                                    ci.CompanyFax = parts[7];
                                if (parts.Length > 8)
                                    ci.CompanyEmail = parts[8];
                                if (parts.Length > 9)
                                    ci.DexUOM = parts[9];
                                if (parts.Length > 10)
                                    ci.CompanyLicenses = parts[10];

                                if (parts.Length > 11)
                                    ci.FromFile = Convert.ToInt32(parts[11]) > 0;

                                if (parts.Length > 12)
                                    ci.ExtraFields = parts[12];
                                if (parts.Length > 13)
                                    ci.CompanyLogo = parts[13];
                                if (parts.Length > 14)
                                    ci.CompanyLogoHeight = Convert.ToInt32(parts[14]);
                                if (parts.Length > 15)
                                    ci.CompanyLogoSize = Convert.ToInt32(parts[15]);
                                if (parts.Length > 16)
                                    ci.CompanyLogoWidth = Convert.ToInt32(parts[16]);
                                if (parts.Length > 17)
                                    ci.LogoId = Convert.ToInt32(parts[17]);

                                if (parts.Length > 18)
                                    ci.IsDefault = Convert.ToInt32(parts[18]) > 0;

                                if (parts.Length > 19)
                                    ci.CompanyLogoPath = parts[19];

                                if (parts.Length > 20)
                                    ci.PaymentClientSecret = parts[20];

                                if (parts.Length > 21)
                                    ci.PaymentClientId = parts[21];

                                if (parts.Length > 22)
                                    ci.PaymentMerchant = parts[22];

                                if (parts.Length > 23)
                                    ci.BottomTextPrint = parts[23];


                                companies.Add(ci);
                            }
                            catch (Exception ee)
                            {
                                Logger.CreateLog(ee);
                                //Xamarin.Insights.Report(ee);
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - allow the app to continue even if companies.cvs can't be loaded
                Logger.CreateLog($"Error loading CompanyInfo: {ex.Message}");
                Logger.CreateLog(ex);
            }
        }

        public override string ToString()
        {
            string s = CompanyName;
            if (!string.IsNullOrEmpty(CompanyAddress1))
                s += "\n" + CompanyAddress1;
            if (!string.IsNullOrEmpty(CompanyAddress2))
                s += "\n" + CompanyAddress2;
            if (!string.IsNullOrEmpty(CompanyPhone))
                s += "\n" + CompanyPhone;

            return s;
        }

        public static CompanyInfo GetMasterCompany()
        {
            try
            {
                var company = companies.FirstOrDefault(x => x.IsDefault);
                if (company == null)
                    company = companies[0];

                return company;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Assigns company name and id to an order from SelectedCompany or, if null, from GetMasterCompany().
        /// Call this when creating orders so company is always set when the app has at least one company (matches Xamarin behavior).
        /// </summary>
        public static void AssignCompanyToOrder(Order order)
        {
            if (order == null)
                return;
            var company = SelectedCompany ?? GetMasterCompany();
            if (company != null)
            {
                order.CompanyName = company.CompanyName;
                order.CompanyId = company.CompanyId;
            }
            else
            {
                order.CompanyName = string.Empty;
                order.CompanyId = 0;
            }
        }

        public static CompanyInfo CreateDefaultCompany()
        {
            var company = new CompanyInfo()
            {
                CompanyName = null,
                CompanyAddress1 = "",
                CompanyAddress2 = "",
                CompanyPhone = "(786) xxxxxxx",
                CommId = "123456789",
                DUNS = "123456789",
                Location = "123456"
            };

            return company;
        }
    }
}

