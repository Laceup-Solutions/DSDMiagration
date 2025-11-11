using Android.Content.PM;

namespace LaceupMigration.ViewModels
{
	public static partial class PlatformAppInfo
	{
		public static partial string GetVersatileDexVersion()
		{
			try
			{
				var context = Android.App.Application.Context;
				var packageManager = context.PackageManager;
				var list = packageManager.GetInstalledApplications(PackageInfoFlags.MetaData);
				foreach (var item in list)
				{
					var pkgName = item.PackageName;
					if (pkgName != null && pkgName.Contains("VersatileDEX"))
						return packageManager.GetPackageInfo(pkgName, 0)!.VersionName ?? string.Empty;
				}
			}
			catch
			{
			}

			return string.Empty;
		}
	}
}

