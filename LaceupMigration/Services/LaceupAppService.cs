using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

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
					DataAccessEx.Initialize();
					ActivityState.Load();
					
					// Note: Empty orders and batches are no longer deleted on initialization
					// since we're keeping state and need to preserve orders for state restoration

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
					// Reuse existing business-layer export - pass subject parameter to match Xamarin behavior
					DataAccessEx.ExportData(subject);
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

		public Task<List<(string sessionId, string displayName)>> GetAvailableBackupSessionsAsync()
		{
			return Task.Run(() =>
			{
				var sessions = new List<(string sessionId, string displayName)>();
				try
				{
					// Matches Xamarin RestoreData1() - uses BackgroundDataSyncAvailableSessionsCommand
					using (var netaccess = new NetAccess())
					{
						try
						{
							netaccess.OpenConnection();
							netaccess.WriteStringToNetwork("HELO");
							netaccess.WriteStringToNetwork(Config.GetAuthString());
							netaccess.WriteStringToNetwork("BackgroundDataSyncAvailableSessionsCommand");
							
							// Read single string response containing all sessions separated by '|'
							var avail = netaccess.ReadStringFromNetwork();
							netaccess.CloseConnection();
							
							if (string.IsNullOrEmpty(avail))
							{
								// No sessions available - return empty list
								return sessions;
							}
							
							// Split by '|' to get individual sessions
							var availableSessionsArray = avail.Split(new char[] { '|' });
							
							// Get current salesman ID for filtering
							var currentId = Config.SalesmanId.ToString(System.Globalization.CultureInfo.InvariantCulture);
							
							// Process each session - matches Xamarin RestoreData1() logic
							foreach (var part in availableSessionsArray)
							{
								if (string.IsNullOrEmpty(part))
									continue;
								
								// Each session is in format: sessionId=salesmanId=part2=dateTime
								var parts1 = part.Split(new char[] { '=' });
								
								// Filter: only show sessions matching current salesman ID
								if (parts1.Length > 1 && parts1[1] == currentId)
								{
									// Display format: "Android Backup" + NewLine + "createdOnData" + dateTime
									// Source format: "{0}={1}={2}" (sessionId=salesmanId=part2)
									var displayName = "Android Backup" + System.Environment.NewLine + "createdOnData" + (parts1.Length > 3 ? parts1[3] : "");
									var sessionId = string.Format("{0}={1}={2}", 
										parts1[0], 
										parts1[1], 
										parts1.Length > 2 ? parts1[2] : "");
									
									sessions.Add((sessionId, displayName));
								}
							}
						}
						catch (Exception ex)
						{
							Logger.CreateLog($"Error getting backup sessions from server: {ex.Message}");
							Logger.CreateLog($"Stack trace: {ex.StackTrace}");
							// If server request fails, return empty list
						}
					}
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
				}
				return sessions;
			});
		}

		public Task<string> DownloadBackupFileAsync(string sessionId)
		{
			return Task.Run(() =>
			{
				try
				{
					// Matches Xamarin RestoreData3() - uses BackgroundDataSyncSendFileCommand
					// sessionId is in format: "{0}={1}={2}" (sessionId=salesmanId=part2)
					var tempFile = Path.GetTempFileName();
					
					using (var netaccess = new NetAccess())
					{
						netaccess.OpenConnection();
						netaccess.WriteStringToNetwork("HELO");
						netaccess.WriteStringToNetwork(Config.GetAuthString());
						netaccess.WriteStringToNetwork("BackgroundDataSyncSendFileCommand");
						netaccess.WriteStringToNetwork(sessionId);
						
						// Download the backup file
						var received = netaccess.ReceiveFile(tempFile);
						netaccess.CloseConnection();
						
						if (received > 0 && File.Exists(tempFile))
						{
							return tempFile;
						}
						else
						{
							throw new FileNotFoundException("Backup file not received from server.");
						}
					}
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
					throw;
				}
			});
		}

		public Task RestoreDataAsync(string zipFilePath)
		{
			return Task.Run(() =>
			{
				try
				{
					// Matches Xamarin RestoreData3() logic
					RestoreDataFromZip(zipFilePath);
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
					throw;
				}
			});
		}

		private void RestoreDataFromZip(string zipFilePath)
		{
			lock (FileOperationsLocker.lockFilesObject)
			{
				try
				{
					if (string.IsNullOrEmpty(zipFilePath) || !File.Exists(zipFilePath))
					{
						throw new FileNotFoundException("Zip file not found.");
					}

					// EXACTLY matches Xamarin RestoreData3() - step by step:
					// Step 1: ClearData(false, true) - clears data and settings
					DataAccess.ClearData();
					Config.ClearSettings();

					// Step 2: Config.Initialize() - creates all directories
					Config.Initialize();

					// Step 3: Download static data (products, clients) from server
					// In Xamarin this happens after SaveConfiguration() and downloading backup,
					// but we already have the backup file, so we do this now
					var responseMessage = DataAccessEx.DownloadStaticData();
					if (!string.IsNullOrEmpty(responseMessage))
					{
						throw new Exception(responseMessage);
					}

					// Step 4: Extract zip file to CodeBase - EXACTLY as Xamarin does
					var fastZip = new FastZip();
					string fileFilter = null;

					// Will always overwrite if target filenames already exist
					fastZip.ExtractZip(zipFilePath, Config.CodeBase, fileFilter);

					// Step 5: Config.Initialize() again after extraction - EXACTLY as Xamarin
					Config.Initialize();

					// Step 6: Set ReceivedData and save app status - EXACTLY as Xamarin
					DataAccess.ReceivedData = true;
					Config.SaveAppStatus();

					// Step 7: DataAccess.Initialize() - EXACTLY as Xamarin (not DataAccessEx)
					// This will call CompanyInfo.Load() which now handles missing files gracefully
					DataAccess.Initialize();

					// Step 8: Delete temp file - EXACTLY as Xamarin
					if (File.Exists(zipFilePath) && zipFilePath.Contains(Path.GetTempPath()))
					{
						try
						{
							File.Delete(zipFilePath);
						}
						catch
						{
							// Ignore deletion errors
						}
					}

					Logger.CreateLog("Data restored successfully from: " + zipFilePath);
				}
				catch (Exception ex)
				{
					Logger.CreateLog(ex);
					throw;
				}
			}
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
					await Shell.Current.GoToAsync("///MainPage");
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

