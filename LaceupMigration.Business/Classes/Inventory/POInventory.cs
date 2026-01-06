





using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    class POInventory
    {
        public static char[] DataLineSplitter = new char[] { (char)20 };

        public int Id { get; set; }
        public double Qty { get; set; }

        static List<POInventory> poList = new List<POInventory>();

        public static IEnumerable<POInventory> POList
        {
            get { return poList; }
        }

        public static void Clear()
        {
            poList.Clear();
        }

        public static void Add(POInventory item)
        {
            poList.Add(item);
        }


        public static void GetUpdatedPO()
        {
            string fileLocation = DataAccess.GetOnPOInventory();

            if (fileLocation == string.Empty)
                return;

            try
            {
                using (var reader = new StreamReader(fileLocation))
                {
                    poList.Clear();

                    string currentline;

                    while ((currentline = reader.ReadLine()) != null)
                    {
                        string[] currentrow = currentline.Split(DataLineSplitter);

                        var poInventory = new POInventory()
                        {
                            Id = Convert.ToInt32(currentrow[0]),
                            Qty = Convert.ToDouble(currentrow[1]),
                        };

                        POInventory.Add(poInventory);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (File.Exists(fileLocation))
                    File.Delete(fileLocation);
            }
          
        }
    }
}