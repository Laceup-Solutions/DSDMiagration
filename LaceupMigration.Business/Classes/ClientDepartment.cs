using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class ClientDepartment 
    {
        public int DepartmentId { get; set; }

        public int ClientId { get; set; }

        public string Name { get; set; }

        public string UniqueId { get; set; }

        public string ExtraFields { get; set; }

        public bool IsActive { get; set; }

        public bool Updated { get; set; }

        static List<ClientDepartment> departments = new List<ClientDepartment>();

        public static List<ClientDepartment> Departments { get { return departments; } }

        public static void Clear()
        {
            departments.Clear();
        }

        public static ClientDepartment CreateDepartment(string s)
        {
            var parts = s.Split(DataAccess.DataLineSplitter);

            var dep = new ClientDepartment()
            {
                DepartmentId = Convert.ToInt32(parts[0]),
                Name = parts[1],
                ClientId = Convert.ToInt32(parts[2]),
                UniqueId = parts[3],
                ExtraFields = parts[4],
                IsActive = Convert.ToInt32(parts[5]) > 0
            };

            if (parts.Length > 6)
                dep.Updated = Convert.ToInt32(parts[6]) > 0;

            return dep;
        }

        public static ClientDepartment AddDepartment(string name, Client client)
        {
            var dep = new ClientDepartment()
            {
                Name = name,
                ClientId = client.ClientId,
                UniqueId = Guid.NewGuid().ToString("N"),
                ExtraFields = "",
                IsActive = true,
                Updated = true
            };

            Departments.Add(dep);

            Save();

            return dep;
        }

        public static List<ClientDepartment> GetDepartmentsForClient(Client client)
        {
            return Departments.Where(x => x.ClientId == client.ClientId).ToList();
        }

        public string Serialize()
        {
            string s = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", (char)20,
                DepartmentId,
                Name,
                ClientId,
                UniqueId,
                ExtraFields ?? string.Empty,
                IsActive ? "1" : "0",
                Updated ? "1" : "0");

            return s;
        }

        public static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(Config.ClientDepartmentsFile))
                        File.Delete(Config.ClientDepartmentsFile);

                    using (StreamWriter writer = new StreamWriter(Config.ClientDepartmentsFile, false))
                    {
                        foreach (var item in departments)
                            writer.WriteLine(item.Serialize());

                        writer.Close();
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void SerializeUpdatedValues(string file)
        {
            if (File.Exists(file))
                File.Delete(file);

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (var item in departments.Where(x => x.Updated))
                    writer.WriteLine(item.Serialize());

                writer.Close();
            }
        }

        public static void LoadFromFile()
        {
            if (!File.Exists(Config.ClientDepartmentsFile))
                return;

            departments.Clear();

            if (File.Exists(Config.ClientDepartmentsFile))
            {
                using (StreamReader reader = new StreamReader(Config.ClientDepartmentsFile))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        departments.Add(CreateDepartment(line));
                    }
                    reader.Close();
                }
            }
        }
    }
}