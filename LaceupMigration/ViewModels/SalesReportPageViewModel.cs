using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class SalesReportPageViewModel : BaseReportPageViewModel
    {
        [ObservableProperty] private string _selectedSalesman = "All Salesmen";
        [ObservableProperty] private int _selectedSalesmanId = 0;
        [ObservableProperty] private bool _showSalesmanSelection = false;
        [ObservableProperty] private string _commission = "0";
        [ObservableProperty] private bool _showWithDetailsButton = false;
        [ObservableProperty] private bool _showDetails = true;

        private List<Salesman> _salesmenList = new();

        public SalesReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
            : base(dialogService, appService)
        {
            InitializeSalesmanSelection();
        }

        private void InitializeSalesmanSelection()
        {
            if (Config.CanSelectSalesman)
            {
                GetSalesmanList();
                ShowSalesmanSelection = true;
                SelectedSalesman = "All Salesmen";
            }

            if (Config.CheckCommunicatorVersion("28.3"))
            {
                ShowDetails = false;
                ShowWithDetailsButton = false;
            }
            else
            {
                ShowWithDetailsButton = true;
            }
        }

        private void GetSalesmanList()
        {
            if (Salesman.List.Count == 0)
            {
                try
                {
                    DataProvider.GetSalesmanList();
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                }
            }

            _salesmenList = Salesman.List.OrderBy(x => x.Name).ToList();
        }

        [RelayCommand]
        private async Task SelectSalesman()
        {
            if (!Config.CanSelectSalesman)
                return;

            GetSalesmanList();

            var list = _salesmenList.Select(x => x.Name).ToList();
            list.Insert(0, "All Salesmen");

            var selectedName = await _dialogService.ShowActionSheetAsync("Select Salesman", "Cancel", null, list.ToArray());

            if (!string.IsNullOrEmpty(selectedName) && selectedName != "Cancel")
            {
                SelectedSalesman = selectedName;
                if (selectedName == "All Salesmen")
                {
                    SelectedSalesmanId = 0;
                }
                else
                {
                    var s = _salesmenList.FirstOrDefault(x => x.Name == selectedName);
                    SelectedSalesmanId = s != null ? s.Id : 0;
                }
            }
        }

        protected override string GetBaseCommand()
        {
            var cmd = base.GetBaseCommand();

            int slm = 0;
            if (ShowSalesmanSelection && SelectedSalesman != "All Salesmen")
            {
                slm = SelectedSalesmanId;
            }
            else if (!ShowSalesmanSelection)
            {
                slm = Config.SalesmanId;
            }

            cmd += "|" + slm;
            cmd += "|0|" + Commission;

            if (Config.DaysToRunReports > 0)
            {
                var dateFrom = DateTime.Now.Date.AddDays(-Config.DaysToRunReports);
                var dateTo = DateTime.Now.Date;
                cmd += "|" + (long)dateFrom.Ticks + "|" + (long)dateTo.Ticks;
            }
            else
            {
                cmd += "|" + (long)DateFrom.Ticks + "|" + (long)DateTo.Ticks;
            }

            return cmd;
        }

        protected override string GetReport(string command)
        {
            try
            {
                return DataProvider.GetSalesReport(command);
            }
            catch
            {
                throw;
            }
        }

        private string GetReportWithDetails(string command)
        {
            try
            {
                return DataProvider.GetSalesReportWithDetails(command);
            }
            catch
            {
                throw;
            }
        }

        protected override async Task RunReport()
        {
            await RunReportInternal();
        }

        [RelayCommand]
        private async Task RunReportWithDetails()
        {
            string responseMessage = null;
            string pdfFile = "";

            try
            {
                ProgressDialogHelper.Show("Generating Report...");
                await Task.Yield(); // Ensure UI updates before blocking operation

                // Run GetReportWithDetails on background thread to avoid blocking UI
                var startTime = DateTime.Now;
                string command = GetBaseCommand();
                
                try
                {
                    pdfFile = await Task.Run(() => GetReportWithDetails(command));
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    responseMessage = "Error downloading report";
                    return; // Exit early, finally block will hide dialog
                }

                if (string.IsNullOrEmpty(pdfFile) || !System.IO.File.Exists(pdfFile))
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
                // CRITICAL: Always hide dialog, even on error
                ProgressDialogHelper.Hide();

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
            }
        }

        protected override async Task SendByEmail()
        {
            if (ShowDetails)
            {
                var options = new List<string>
                {
                    "Email Report",
                    "Email Report With Details"
                };

                var selected = await _dialogService.ShowActionSheetAsync("Print Options", "Cancel", null, options.ToArray());

                if (selected == "Email Report")
                {
                    await SendByEmailInternal();
                }
                else if (selected == "Email Report With Details")
                {
                    await SendByEmailWithDetails();
                }
            }
            else
            {
                await SendByEmailInternal();
            }
        }

        private async Task SendByEmailWithDetails()
        {
            string responseMessage = null;
            string pdfFile = "";

            try
            {
                ProgressDialogHelper.Show("Generating Report...");
                await Task.Yield(); // Ensure UI updates before blocking operation

                // Run GetReportWithDetails on background thread to avoid blocking UI
                var startTime = DateTime.Now;
                string command = GetBaseCommand();
                
                try
                {
                    pdfFile = await Task.Run(() => GetReportWithDetails(command));
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    responseMessage = "Error downloading report";
                    return; // Exit early, finally block will hide dialog
                }

                if (string.IsNullOrEmpty(pdfFile) || !System.IO.File.Exists(pdfFile))
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
                // CRITICAL: Always hide dialog, even on error
                ProgressDialogHelper.Hide();

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    await _dialogService.ShowAlertAsync(responseMessage, "Alert", "OK");
                }
            }
        }
    }
}

