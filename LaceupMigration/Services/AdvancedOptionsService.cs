using LaceupMigration.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.Services
{
	public class AdvancedOptionsService
	{
		private readonly IDialogService _dialogService;
		private readonly ILaceupAppService _appService;

		public AdvancedOptionsService(IDialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
		}

		public async Task ShowAdvancedOptionsAsync()
		{
			var options = new List<string>
			{
				"Update settings",
				"Send log file",
				"Export data",
				"Remote control",
				"Setup printer"
			};

			if (Config.GoToMain)
			{
				options.Add("Go to main activity");
			}

			var choice = await _dialogService.ShowActionSheetAsync("Advanced options", "", "Cancel", options.ToArray());

			if (string.IsNullOrEmpty(choice) || choice == "Cancel")
				return;

			await HandleAdvancedOptionAsync(choice);
		}

		private async Task HandleAdvancedOptionAsync(string choice)
		{
			switch (choice)
			{
				case "Update settings":
					try
					{
						await _dialogService.ShowLoadingAsync("Updating settings...");
						await _appService.UpdateSalesmanSettingsAsync();
						await _dialogService.HideLoadingAsync();
						await _dialogService.ShowAlertAsync("Settings updated.", "Info", "OK");
					}
					catch (Exception ex)
					{
						await _dialogService.HideLoadingAsync();
						Logger.CreateLog(ex);
						await _dialogService.ShowAlertAsync("Error updating settings.", "Error", "OK");
					}
					break;

				case "Send log file":
					try
					{
						await _dialogService.ShowLoadingAsync("Sending log file...");
						await _appService.SendLogAsync();
						await _dialogService.HideLoadingAsync();
						await _dialogService.ShowAlertAsync("Log sent.", "Info", "OK");
					}
					catch (Exception ex)
					{
						await _dialogService.HideLoadingAsync();
						Logger.CreateLog(ex);
						await _dialogService.ShowAlertAsync("Error sending log file.", "Error", "OK");
					}
					break;

				case "Export data":
					try
					{
						await _dialogService.ShowLoadingAsync("Exporting data...");
						await _appService.ExportDataAsync();
						await _dialogService.HideLoadingAsync();
						await _dialogService.ShowAlertAsync("Data exported.", "Info", "OK");
					}
					catch (Exception ex)
					{
						await _dialogService.HideLoadingAsync();
						Logger.CreateLog(ex);
						await _dialogService.ShowAlertAsync("Error exporting data.", "Error", "OK");
					}
					break;

				case "Remote control":
					try
					{
						await _appService.RemoteControlAsync();
					}
					catch (Exception ex)
					{
						Logger.CreateLog(ex);
						await _dialogService.ShowAlertAsync("Error launching remote control.", "Error", "OK");
					}
					break;

				case "Setup printer":
					// Match Xamarin SetupPrinter() behavior exactly
					await SetupPrinterAsync();
					break;

				case "Go to main activity":
					try
					{
						await _appService.GoBackToMainAsync();
					}
					catch (Exception ex)
					{
						Logger.CreateLog(ex);
						await _dialogService.ShowAlertAsync("Error navigating to main.", "Error", "OK");
					}
					break;
			}
		}

		private async Task SetupPrinterAsync()
		{
			// Match Xamarin SetupPrinter() behavior exactly
			PrinterProvider.PrinterAddress = string.Empty;
			
			if (!Config.PrinterAvailable)
			{
				await _dialogService.ShowAlertAsync("Printer is not available.", "Alert", "OK");
				return;
			}

			var printers = PrinterProvider.AvailablePrinters();
			
			switch (printers?.Count ?? 0)
			{
				case 0:
					await _dialogService.ShowAlertAsync("Printer not found.", "Alert", "OK");
					break;
				case 1:
					PrinterProvider.PrinterAddress = printers[0].Address;
					await ConfigurePrinterAsync();
					break;
				default:
					await SelectPrinterAsync(printers);
					break;
			}
		}

		private async Task SelectPrinterAsync(IList<PrinterDescription> printers)
		{
			// Match Xamarin SelectPrinter() behavior
			var printerNames = printers.Select(x => x.Name).ToArray();
			var choice = await _dialogService.ShowActionSheetAsync("Select Printer", "", "Cancel", printerNames);
			
			if (string.IsNullOrEmpty(choice) || choice == "Cancel")
				return;

			var selectedPrinter = printers.FirstOrDefault(x => x.Name == choice);
			if (selectedPrinter != null)
			{
				PrinterProvider.PrinterAddress = selectedPrinter.Address;
				await ConfigurePrinterAsync();
			}
		}

		private async Task ConfigurePrinterAsync()
		{
			// Match Xamarin ConfigurePrinter() behavior - run on background thread
			await _dialogService.ShowLoadingAsync("Configuring printer...");
			
			try
			{
				bool result = false;
				await Task.Run(() =>
				{
					var zebra = PrinterProvider.CurrentPrinter();
					result = zebra.ConfigurePrinter();
				});

				await _dialogService.HideLoadingAsync();

				if (!result)
				{
					await _dialogService.ShowAlertAsync("Error setup printer.", "Alert", "OK");
				}
				else
				{
					await _dialogService.ShowAlertAsync("Printer configured.", "Info", "OK");
				}
			}
			catch (Exception ex)
			{
				await _dialogService.HideLoadingAsync();
				await _dialogService.ShowAlertAsync($"Error setup printer: {ex.Message}", "Alert", "OK");
			}
		}
	}
}

