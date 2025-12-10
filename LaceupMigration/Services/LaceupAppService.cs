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
					
					//clear previous Data (empty batches and orders)
					var orders = Order.Orders.Where(x => x.OrderType != OrderType.NoService && x.Details.Count == 0 && string.IsNullOrEmpty(x.PrintedOrderId)).ToList();
					foreach (var order in orders)
						order.Delete();

					var batches = Batch.List.Where(x => x.Orders().Count == 0).ToList();
					foreach (var batch in batches)
						batch.Delete();

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
					// Try to get backup session list from server - matches Xamarin behavior
					// The server returns a list of session names/IDs, not the actual backup files
					using (var netaccess = new NetAccess())
					{
						try
						{
							netaccess.OpenConnection();
							netaccess.WriteStringToNetwork("HELO");
							netaccess.WriteStringToNetwork(Config.GetAuthString());
							netaccess.WriteStringToNetwork("GetBackupListCommand");
							
							// Read the number of backup sessions available
							var countStr = netaccess.ReadStringFromNetwork();
							if (int.TryParse(countStr, out int sessionCount) && sessionCount > 0)
							{
								// Read each session name/ID
								for (int i = 0; i < sessionCount; i++)
								{
									var sessionInfo = netaccess.ReadStringFromNetwork();
									if (!string.IsNullOrEmpty(sessionInfo))
									{
										// sessionInfo might be in format "sessionId|displayName" or just "displayName"
										var parts = sessionInfo.Split('|');
										if (parts.Length >= 2)
										{
											sessions.Add((parts[0], parts[1])); // sessionId, displayName
										}
										else
										{
											// Use sessionInfo as both ID and display name
											sessions.Add((sessionInfo, sessionInfo));
										}
									}
								}
							}
							
							netaccess.WriteStringToNetwork("Goodbye");
							netaccess.CloseConnection();
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
					// Download the specific backup file for the selected session
					using (var netaccess = new NetAccess())
					{
						netaccess.OpenConnection();
						netaccess.WriteStringToNetwork("HELO");
						netaccess.WriteStringToNetwork(Config.GetAuthString());
						netaccess.WriteStringToNetwork("GetBackupFileCommand");
						netaccess.WriteStringToNetwork(sessionId);
						
						// Download the backup file
						var tempBackupFile = Path.Combine(Path.GetTempPath(), $"backup_{sessionId}_{DateTime.Now.Ticks}.zip");
						var received = netaccess.ReceiveFile(tempBackupFile);
						
						netaccess.WriteStringToNetwork("Goodbye");
						netaccess.CloseConnection();
						
						if (received > 0 && File.Exists(tempBackupFile))
						{
							return tempBackupFile;
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
					// Restore data from zip file - matches Xamarin RestoreData logic
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

					// Extract zip file to CodeBase directory - matches Xamarin restore logic
					var fastZip = new FastZip();
					bool recurse = true;
					string filter = null;

					// Extract the zip file to CodeBase (overwrites existing files)
					fastZip.ExtractZip(zipFilePath, Config.CodeBase, filter);

					// After extracting, reload all data - matches Xamarin behavior
					// This will reload products, clients, orders, etc. from the restored files
					DataAccessEx.Initialize();

					// Reload other data that might have been restored
					Client.LoadClients();
					DataAccess.LoadBatches();
					DataAccess.LoadOrders();
					DataAccess.LoadPayments();
					Client.LoadNotes();
					ParLevel.LoadList();
					LoadOrder.LoadList();
					BuildToQty.LoadList();

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

