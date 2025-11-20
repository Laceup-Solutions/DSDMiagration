using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public abstract partial class BaseReportPageViewModel : ObservableObject
    {
        protected readonly DialogService _dialogService;
        protected readonly ILaceupAppService _appService;
        private static int _reportCounter = 0;

        [ObservableProperty] private DateTime _dateFrom = DateTime.Now;
        [ObservableProperty] private DateTime _dateTo = DateTime.Now;
        [ObservableProperty] private string _dateFromText = string.Empty;
        [ObservableProperty] private string _dateToText = string.Empty;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _dateRangeEnabled = true;

        protected BaseReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            
            if (Config.DaysToRunReports > 0)
            {
                DateFrom = DateTime.Now.AddDays(-Config.DaysToRunReports);
                DateRangeEnabled = false;
            }
            
            UpdateDateTexts();
        }

        private void UpdateDateTexts()
        {
            DateFromText = DateFrom.ToShortDateString();
            DateToText = DateTo.ToShortDateString();
        }

        partial void OnDateFromChanged(DateTime value)
        {
            UpdateDateTexts();
        }

        partial void OnDateToChanged(DateTime value)
        {
            UpdateDateTexts();
        }

        /// <summary>
        /// Builds the base command string with date range
        /// </summary>
        protected virtual string GetBaseCommand()
        {
            if (Config.DaysToRunReports > 0)
            {
                var dateFrom = DateTime.Now.Date.AddDays(-Config.DaysToRunReports);
                var dateTo = DateTime.Now.Date;
                return dateFrom.ToShortDateString() + "|" + dateTo.ToShortDateString();
            }
            else
            {
                return DateFromText + "|" + DateToText;
            }
        }

        /// <summary>
        /// Gets the report PDF file path from DataAccess
        /// </summary>
        protected abstract string GetReport(string command);

        /// <summary>
        /// Creates a PDF file in the storage directory from the temporary file
        /// </summary>
        protected string CreatePdf(string from)
        {
            // Use unique filename with timestamp to avoid conflicts
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var path = Path.Combine(Config.LaceupStorage, $"report_{timestamp}_{_reportCounter}.pdf");

            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Delete destination file if it exists (shouldn't with timestamp, but just in case)
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception deleteEx)
                    {
                        // File might be locked, try with a different name
                        Logger.CreateLog($"Could not delete existing file {path}: {deleteEx.Message}");
                        path = Path.Combine(Config.LaceupStorage, $"report_{timestamp}_{_reportCounter}_{Guid.NewGuid():N}.pdf");
                    }
                }

                // Move the temporary file to the final location
                if (File.Exists(from))
                {
                    File.Move(from, path);
                }
                else
                {
                    throw new FileNotFoundException($"Source file not found: {from}");
                }

                _reportCounter++;
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error creating PDF from {from} to {path}: {ex}");
                
                // Try to clean up source file if move failed
                try
                {
                    if (File.Exists(from))
                    {
                        File.Delete(from);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
                
                throw;
            }

            return path;
        }

        /// <summary>
        /// Shows the PDF using the platform-specific viewer
        /// </summary>
        protected void ShowPdf(string pdfFile)
        {
            if (string.IsNullOrEmpty(pdfFile) || !File.Exists(pdfFile))
            {
                return;
            }

            try
            {
                Config.helper?.ViewPdf(pdfFile);
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error opening pdf file ==> " + ex.ToString());
            }
        }

        /// <summary>
        /// Sends the report PDF by email
        /// </summary>
        protected async Task SendReportByEmail(string pdfFile)
        {
            if (string.IsNullOrEmpty(pdfFile) || !File.Exists(pdfFile))
            {
                await _dialogService.ShowAlertAsync("Error occurred sending email", "Alert", "OK");
                return;
            }

            try
            {
                var emailMessage = new EmailMessage
                {
                    Subject = "Report Attached",
                    Body = ""
                };

                // Attach PDF file
                var file = new EmailAttachment(pdfFile);
                emailMessage.Attachments.Add(file);

                await Email.ComposeAsync(emailMessage);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email", "Alert", "OK");
            }
        }

        /// <summary>
        /// Runs the report and shows it
        /// </summary>
        protected async Task RunReportInternal()
        {
            IsLoading = true;
            string responseMessage = null;
            string pdfFile = "";

            try
            {
                ProgressDialogHelper.Show("Downloading Report...");

                var startTime = DateTime.Now;
                string command = GetBaseCommand();
                pdfFile = GetReport(command);

                if (string.IsNullOrEmpty(pdfFile) || !File.Exists(pdfFile))
                {
                    responseMessage = "Error downloading report";
                }
                else
                {
                    Logger.CreateLog($"report took: {DateTime.Now.Subtract(startTime).TotalSeconds} seconds");

                    try
                    {
                        var finalPath = CreatePdf(pdfFile);
                        ShowPdf(finalPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        responseMessage = "Access to the file is denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                responseMessage = "Error downloading report";
            }
            finally
            {
                ProgressDialogHelper.Hide();
                IsLoading = false;

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
            }
        }

        /// <summary>
        /// Sends the report by email
        /// </summary>
        protected async Task SendByEmailInternal()
        {
            IsLoading = true;
            string responseMessage = null;
            string pdfFile = "";

            try
            {
                ProgressDialogHelper.Show("Downloading Report...");

                var startTime = DateTime.Now;
                string command = GetBaseCommand();
                pdfFile = GetReport(command);

                if (string.IsNullOrEmpty(pdfFile) || !File.Exists(pdfFile))
                {
                    responseMessage = "Error downloading report";
                }
                else
                {
                    Logger.CreateLog($"report took: {DateTime.Now.Subtract(startTime).TotalSeconds} seconds");

                    try
                    {
                        var finalPath = CreatePdf(pdfFile);
                        await SendReportByEmail(finalPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        responseMessage = "Access to the file is denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                responseMessage = "Error downloading report";
            }
            finally
            {
                ProgressDialogHelper.Hide();
                IsLoading = false;

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
            }
        }

        [RelayCommand]
        protected abstract Task RunReport();

        [RelayCommand]
        protected abstract Task SendByEmail();
    }
}

