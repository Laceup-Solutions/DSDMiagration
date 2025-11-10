using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class AccessCode
    {
        public static AccessCode currentCode;
        public string Code { get; set; }

        public DateTime DateExpiration { get; set; }

        public static void LoadCode()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Config.AccessCodePath))
                {
                    var line = reader.ReadLine();
                    if (line != null && !string.IsNullOrEmpty(line))
                    {
                        var parts = line.Split('|');
                        var longDate = Convert.ToInt64(parts[1]);
                        currentCode = new AccessCode() { Code = parts[0], DateExpiration = new DateTime(longDate) };
                    }

                    reader.Close();
                }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }
        public static void Save()
        {
            try
            {
                if(File.Exists(Config.AccessCodePath))
                {
                    File.Delete(Config.AccessCodePath);
                }

                //FileOperationsLocker.InUse = true;
                using (StreamWriter writer = new StreamWriter(Config.AccessCodePath))
                {
                    writer.WriteLine(currentCode.Code + "|" + currentCode.DateExpiration.Ticks);

                    writer.Close();
                }

            }
            finally
            {
                //FileOperationsLocker.InUse = false;
            }
        }

        public static void Clear()
        {
            if (File.Exists(Config.AccessCodePath))
            {
                File.Delete(Config.AccessCodePath);
            }

            currentCode = null;
        }
    }
}