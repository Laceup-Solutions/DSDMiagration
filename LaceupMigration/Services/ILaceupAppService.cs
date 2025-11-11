namespace LaceupMigration.Services
{
	public interface ILaceupAppService
	{
		bool IsInitialized { get; }
		Task InitializeApplicationAsync();
		void RecordEvent(string eventName);
		void TrackError(Exception e);
		Task SendLogAsync();
		Task ExportDataAsync(string subject = "");
		Task RemoteControlAsync();
		Task UpdateSalesmanSettingsAsync();
		Task VerifySequenceAsync(bool prompt = true);
		Task GoBackToMainAsync();
		Task GoToPreviousAsync();
		bool IsAppInstalled(string packageName);
	}
}

