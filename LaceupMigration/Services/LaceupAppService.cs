

namespace LaceupMigration.Services
{
	public class LaceupAppService : ILaceupAppService
	{
		private bool _initialized;
		public bool IsInitialized => _initialized;

		public async Task InitializeApplicationAsync()
		{
			if (_initialized)
				return;

			await Task.Run(() =>
			{
				try
				{
					Config.Initialize();
					CompanyInfo.Load();
					BackgroundDataSync.CallMe();

					PlatformAppCenter.InitializeAndHookCrash();

					Logger.CreateLog("Initialized in MAUI");
					DataAccess.Initialize();
					ActivityState.Load();

					BackgroundDataSync.StartThreadh();

					_initialized = true;
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public void RecordEvent(string eventName)
		{
			try
			{
				Logger.CreateLog(eventName);
				PlatformAppCenter.TrackEvent(eventName);
			}
			catch (Exception ex)
			{
				Logger.CreateLog(ex);
			}
		}

		public void TrackError(Exception e)
		{
			try
			{
				PlatformAppCenter.TrackError(e);
				Logger.CreateLog(e);
			}
			catch (Exception ex)
			{
				Logger.CreateLog(ex);
			}
		}

		public Task SendLogAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					Logger.SendLogFile();
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public Task ExportDataAsync(string subject = "")
		{
			return Task.Run(() =>
			{
				try
				{
					// Reuse existing business-layer export
					DataAccessEx.ExportData();
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public Task RemoteControlAsync()
		{
			return PlatformAppCenter.LaunchRemoteControlAsync();
		}

		public Task UpdateSalesmanSettingsAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					DataAccess.GetSalesmanSettings(false);
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public Task VerifySequenceAsync(bool prompt = true)
		{
			return Task.Run(() =>
			{
				try
				{
					if (!Config.AdvanceSequencyNum)
						return;

					var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
					if (salesman == null)
						return;

					var expirationDate = salesman.SequenceExpirationDate.HasValue && salesman.SequenceExpirationDate.Value.Subtract(DateTime.Today).TotalDays < 7;
					var notNumbers = Config.MinimumAvailableNumbers > 0 && (salesman.SequenceTo - Config.LastPrintedId) < Config.MinimumAvailableNumbers;

					if (!expirationDate && !notNumbers)
						return;

					var sb = new System.Text.StringBuilder();
					if (expirationDate)
						sb.Append($"Sequence next to expire: {salesman.SequenceExpirationDate.Value:d}");
					if (notNumbers)
					{
						var diff = salesman.SequenceTo - Config.LastPrintedId;
						var plural = diff > 1 ? "numbers available" : "number available";
						if (sb.Length > 0) sb.Append(" ");
						sb.Append($"{diff} {plural}");
					}

					var message = sb.ToString();
					if (string.IsNullOrWhiteSpace(message))
						return;

					if (prompt)
					{
						MainThread.BeginInvokeOnMainThread(async () =>
						{
							await Application.Current!.MainPage!.DisplayAlert("Warning", message, "OK");
							DataAccess.SendEmailSequenceNotification(message);
						});
					}
					else
					{
						DataAccess.SendEmailSequenceNotification(message);
					}
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public Task GoBackToMainAsync()
		{
			return MainThread.InvokeOnMainThreadAsync(async () =>
			{
				try
				{
					// Approximated: clear stack and go to Main
					await Shell.Current.GoToAsync("//MainPage");
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public Task GoToPreviousAsync()
		{
			return MainThread.InvokeOnMainThreadAsync(async () =>
			{
				try
				{
					await Shell.Current.GoToAsync("..");
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
			});
		}

		public bool IsAppInstalled(string packageName)
		{
			return PlatformAppCenter.IsAppInstalled(packageName);
		}
	}

	public static partial class PlatformAppCenter
	{
		public static partial void InitializeAndHookCrash();
		public static partial void TrackEvent(string name);
		public static partial void TrackError(Exception ex);
		public static partial Task LaunchRemoteControlAsync();
		public static partial bool IsAppInstalled(string packageName);
	}
}

