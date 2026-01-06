namespace LaceupMigration
{
    public static class FileOperationsLocker
    {
        public static object lockFilesObject = new object();

        //static bool inUse;
        //static readonly object inUseLock = new object();

        //public static bool InUse
        //{
        //    get
        //    {
        //        lock (inUseLock)
        //        {
        //            return inUse;
        //        }
        //    }
        //    set
        //    {
        //        lock (inUseLock)
        //        {
        //            inUse = value;
        //        }
        //    }
        //}
    }
}