#if !ANDROID
using System;
using System.Threading.Tasks;

namespace LaceupMigration.Services
{
	public static partial class PlatformAppCenter
	{
		public static partial void InitializeAndHookCrash()
		{
			// No-op on non-Android platforms
		}

		public static partial void TrackEvent(string name)
		{
			// No-op
		}

		public static partial void TrackError(Exception ex)
		{
			// No-op
		}

		public static partial Task LaunchRemoteControlAsync()
		{
			return Task.CompletedTask;
		}

		public static partial bool IsAppInstalled(string packageName) => false;
	}
}
#endif


