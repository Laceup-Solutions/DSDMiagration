using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public static class DataProvider
    {
        static IDataAccess dataAccessInstance = null;

        public static void Initialize()
        {
            if (dataAccessInstance == null)
            {
                var version = NetAccess.GetCommunicatorVersion();

                //if (version != null && version > new Version("80.0.0.0"))
                //{
                //    dataAccessInstance = new DataAccess();
                //}
                //else
                //    dataAccessInstance = new DataAccess();
            }
        }

    }
}
