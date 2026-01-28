using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class LogViewerPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;

        [ObservableProperty] private string _logContent = string.Empty;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _hasStatusMessage;

        public LogViewerPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
        }

        public async Task LoadLogAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists(Config.LogFile))
                    {
                        var content = File.ReadAllText(Config.LogFile);
                        if (string.IsNullOrEmpty(content))
                        {
                            LogContent = "Log file is empty.";
                            StatusMessage = "Log file exists but is empty.";
                            HasStatusMessage = true;
                        }
                        else
                        {
                            LogContent = content;
                            StatusMessage = $"Log file loaded. Size: {content.Length} characters.";
                            HasStatusMessage = true;
                        }
                    }
                    else
                    {
                        LogContent = "Log file does not exist.";
                        StatusMessage = "Log file not found.";
                        HasStatusMessage = true;
                    }
                });
            }
            catch (Exception ex)
            {
                LogContent = $"Error loading log file: {ex.Message}";
                StatusMessage = "Error occurred while loading log file.";
                HasStatusMessage = true;
            }
        }

        [RelayCommand]
        private async Task SendByEmail()
        {
            try
            {
                // Check if log file exists
                if (!File.Exists(Config.LogFile))
                {
                    await _dialogService.ShowAlertAsync("Log file does not exist.", "Error", "OK");
                    return;
                }

                // Use native share sheet to let user choose how to share (Gmail, Quick Share, etc.)
                // This matches the Xamarin app behavior shown in the screenshot
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Log File",
                    File = new ShareFile(Config.LogFile)
                });
            }
            catch (Exception ex)
            {
                // Show error alert if something goes wrong
                await _dialogService.ShowAlertAsync($"Error sharing log: {ex.Message}", "Error", "OK");
                Logger.CreateLog(ex);
            }
        }

        [RelayCommand]
        private async Task ShowMenu()
        {
            var options = new[] { "Advanced Options" };
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", options);
            
            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                return;

            switch (choice)
            {
                case "Advanced Options":
                    await _advancedOptionsService.ShowAdvancedOptionsAsync();
                    break;
            }
        }
    }
}

