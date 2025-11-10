





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SelfServiceCompany 
    {
        public static List<SelfServiceCompany> List = new List<SelfServiceCompany>();

        public string CompanyName { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }

        public string ServerId { get; set; }

        public int PortId { get; set; }

        public int UserId { get; set; }

        public static void Load()
        {
            try
            {
                List.Clear();

                if (!File.Exists(Config.SelfServiceCompany))
                    return;

                using (StreamReader reader = new StreamReader(Config.SelfServiceCompany))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var companyName = parts[0];
                        var serverId = parts[1];
                        var portId = Convert.ToInt32(parts[2]);
                        var userId = Convert.ToInt32(parts[3]);
                        var address = parts[4];
                        var phone = parts[5];

                        var temp = new SelfServiceCompany()
                        {
                            CompanyName = companyName,
                            ServerId = serverId,
                            PortId = portId,
                            UserId = userId,
                            CompanyAddress = address,
                            CompanyPhone = phone
                        };

                        List.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Save()
        {
            if (File.Exists(Config.SelfServiceCompany))
                File.Delete(Config.SelfServiceCompany);

            using (StreamWriter writer = new StreamWriter(Config.SelfServiceCompany))
            {
                foreach (var item in List)
                {
                    writer.Write(item.CompanyName);
                    writer.Write((char)20);
                    writer.Write(item.ServerId);
                    writer.Write((char)20);
                    writer.Write(item.PortId);
                    writer.Write((char)20);
                    writer.Write(item.UserId);
                    writer.Write((char)20);
                    writer.Write(item.CompanyAddress);
                    writer.Write((char)20);
                    writer.Write(item.CompanyPhone);
                    writer.WriteLine();
                }
            }
        }

        public static void Add(SelfServiceCompany c)
        {
            List.Add(c);
            Save();
        }
        public static bool Find(SelfServiceCompany c)
        {
            var duplicated = List.FirstOrDefault(x => x.ServerId.Trim() == c.ServerId.Trim() && x.PortId == c.PortId && x.UserId == c.UserId);

            return duplicated != null;
        }

        public static void Delete(SelfServiceCompany c)
        {
            List.Remove(c);

            Save();
        }

        internal static void ClearAll()
        {
            List.Clear();

            if (File.Exists(Config.SelfServiceCompany))
                File.Delete(Config.SelfServiceCompany);
        }
    }
}