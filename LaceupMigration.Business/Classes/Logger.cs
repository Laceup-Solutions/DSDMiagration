using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LaceupMigration
{
    public static class Logger
    {
        static object syncObject = new object();

        static public void ClearFile()
        {
            try
            {
                if (File.Exists(Config.LogFile))
                    File.Delete(Config.LogFile);
            }
            catch { }
        }

        static private void MaintainFiles()
        {
            try
            {
                if (File.Exists(Config.LogFile))
                {
                    FileInfo fi = new FileInfo(Config.LogFile);
                    if (fi.Length > 500 * 1024)
                    {
                        //move it to the prev file
                        if (File.Exists(Config.LogFilePrevious))
                            File.Delete(Config.LogFilePrevious);
                        File.Move(Config.LogFile, Config.LogFilePrevious);
                    }
                }
            }
            catch { }
        }

        static public void CreateLog(Exception exception)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    lock (syncObject)
                    {
                        if (exception == null)
                            return;
                        try
                        {
                            MaintainFiles();
                            using (StreamWriter writer = new StreamWriter(Config.LogFile, true))
                            {
                                List<string> list = new List<string>();
                                LogException(exception, list);
                                foreach (string line in list)
                                    writer.WriteLine(line);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static public void CreateLog(string msg)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    lock (syncObject)
                    {
                        MaintainFiles();
                        using (StreamWriter writer = new StreamWriter(Config.LogFile, true))
                        {
                            writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "Date:{0} , MSG:{1}", DateTime.Now.ToString(), msg));
                            writer.Close();
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        private static void LogException(Exception e, List<string> messages)
        {
            try
            {
                if (e.InnerException != null)
                    LogException(e.InnerException, messages);
                messages.Add(string.Format(CultureInfo.InvariantCulture, "ERROR Date:{0} , MSG:{1}  ST:{2}", DateTime.Now.ToString(), e.Message, e.StackTrace));
            }
            catch { }
        }
    }
}

