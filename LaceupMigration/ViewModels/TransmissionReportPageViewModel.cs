using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using System;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class TransmissionReportPageViewModel : BaseReportPageViewModel
    {
        [ObservableProperty] private bool _onlyFinal;
        [ObservableProperty] private bool _showClientAddr;
        [ObservableProperty] private bool _onlyBills;

        public TransmissionReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
            : base(dialogService, appService)
        {
        }

        protected override string GetBaseCommand()
        {
            // Match Xamarin logic: Convert DateTo to DateTime, add 1 day and subtract 1 minute
            var endDate = DateTo.AddDays(1).AddMinutes(-1);
            
            // Format the end date as string (matches Xamarin endDate.ToString())
            string date = endDate.ToString("M/d/yyyy h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
            
            // Handle Spanish AM/PM format (matches Xamarin logic)
            if (date.Contains("p. m.") || date.Contains("a. m."))
            {
                date = date.Replace("p. m.", "PM");
                date = date.Replace("a. m.", "AM");
            }

            // Build command: dateFromText|endDateString|salesmanId|onlyFinal|showClientAddr|onlyBills|dateFromTicks|endDateTicks
            // Matches Xamarin: dateFromReport.Text + "|" + date.ToString()
            var cmd = DateFromText + "|" + date;
            cmd += "|" + Config.SalesmanId;
            cmd += "|" + (OnlyFinal ? "1" : "0");
            cmd += "|" + (ShowClientAddr ? "1" : "0");
            cmd += "|" + (OnlyBills ? "1" : "0");

            if (Config.DaysToRunReports > 0)
            {
                var dateFrom = DateTime.Now.Date.AddDays(-Config.DaysToRunReports);
                var dateTo = DateTime.Now.Date;
                cmd += "|" + (long)dateFrom.Ticks + "|" + (long)dateTo.Ticks;
            }
            else
            {
                // Match Xamarin: (long)dateFromReport.Tag + "|" + (long)endDate.Ticks
                cmd += "|" + (long)DateFrom.Ticks + "|" + (long)endDate.Ticks;
            }

            return cmd;
        }

        protected override string GetReport(string command)
        {
            try
            {
                return DataAccess.GetTransmissionReport(command);
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

        protected override async Task SendByEmail()
        {
            await SendByEmailInternal();
        }
    }
}

