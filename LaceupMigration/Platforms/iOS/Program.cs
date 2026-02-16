using ObjCRuntime;
using UIKit;

namespace LaceupMigration
{
    public class Program
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Logger.CreateLog(e.ExceptionObject.ToString());
                };

                TaskScheduler.UnobservedTaskException += (sender, e) =>
                {
                    Logger.CreateLog(e.Exception.ToString());
                    e.SetObserved();
                };
                // if you want to use a different Application Delegate class from "AppDelegate"
                // you can specify it here.
                UIApplication.Main(args, null, typeof(AppDelegate));
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }
    }
}
