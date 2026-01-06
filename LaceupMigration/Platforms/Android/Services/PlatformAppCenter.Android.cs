using Android.Content.PM;
using Android.Content;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace LaceupMigration.Services
{
	public static partial class PlatformAppCenter
	{
		private const string AppCenterAppId = "3df664c5-8949-422d-8a29-076a17a5bb2b";

		public static partial void InitializeAndHookCrash()
		{
			try
			{
				if (!AppCenter.Configured)
				{
					AppCenter.Start(AppCenterAppId, typeof(Analytics), typeof(Crashes));

					Crashes.SendingErrorReport += (sender, e) =>
					{
						try
						{
							var access = new NetAccess();
							access.OpenConnection("app.laceupsolutions.com", 9999);
							access.WriteStringToNetwork("SendLogFile");

							var serializedConfig = "CRASHED APP<br>" + Config.SerializeConfig().Replace(System.Environment.NewLine, "<br>");
							serializedConfig = serializedConfig.Replace("'", "").Replace("’", "");
							access.WriteStringToNetwork(serializedConfig);

							access.SendFile(Config.LogFile);
							access.WriteStringToNetwork("Goodbye");
							Thread.Sleep(1000);
							access.CloseConnection();

							DataProvider.ExportData();
						}
						catch
						{
						}
					};
				}
			}
			catch (Exception ex)
			{
				Logger.CreateLog(ex);
			}
		}

		public static partial void TrackEvent(string name)
		{
			try { Analytics.TrackEvent(name); } catch { }
		}

		public static partial void TrackError(Exception ex)
		{
			try { Crashes.TrackError(ex); } catch { }
		}

		public static partial async Task LaunchRemoteControlAsync()
		{
			try
			{
				var ctx = Android.App.Application.Context;
				var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("com.teamviewer.quicksupport.market"));
				intent.AddFlags(ActivityFlags.NewTask);
				ctx.StartActivity(intent);
			}
			catch (Exception ex)
			{
				try
				{
					var ctx = Android.App.Application.Context;
					var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=com.teamviewer.quicksupport.market"));
					intent.AddFlags(ActivityFlags.NewTask);
					ctx.StartActivity(intent);
				}
				catch
				{
					Logger.CreateLog(ex);
				}
			}

			await Task.CompletedTask;
		}

		public static partial bool IsAppInstalled(string packageName)
		{
			try
			{
				var pm = Android.App.Application.Context.PackageManager;
				pm.GetPackageInfo(packageName, PackageInfoFlags.Activities);
				return true;
			}
			catch (PackageManager.NameNotFoundException)
			{
				return false;
			}
			catch
			{
				return false;
			}
		}
	}
}

