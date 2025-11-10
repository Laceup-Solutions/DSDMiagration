







using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    internal class SapStatus
    {
        public static string StatusesAsString { get; set; }

        public static void Load()
        {
            try
            {
                using (var reader = new StreamReader(Config.SapStatusPath))
                {
                    StatusesAsString = reader.ReadLine();
                }
            }
            catch
            {

            }
        }

        public static void Clear()
        {
            if(File.Exists(Config.SapStatusPath))
                File.Delete(Config.SapStatusPath);
        }

        public static  void Save(string statuses)
        {
            try
            {

                Clear();

                StatusesAsString = statuses;

                using (var writer = new StreamWriter(Config.SapStatusPath))
                {
                    writer.WriteLine(statuses.ToString());
                }
            }
            catch
            {

            }
        }
    }
}