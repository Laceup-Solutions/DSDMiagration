using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ReportsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<ReportItemViewModel> _reports = new();

        public ReportsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            GenerateReportsList();
        }

        private void GenerateReportsList()
        {
            Reports.Clear();

            Reports.Add(new ReportItemViewModel("Sales Report", SalesReport));
            Reports.Add(new ReportItemViewModel("Received Payments Report", PaymentsReport));

            if (Config.CheckCommunicatorVersion("30.0.0.0") && !Config.ShowOldReportsRegardless)
            {
                Reports.Add(new ReportItemViewModel("Salesman Commission Report By Product", CommissionReport));
                Reports.Add(new ReportItemViewModel("Salesman Commission Report By Customer", SalesmenCommissionReport));
            }
            else
            {
                Reports.Add(new ReportItemViewModel("Commissions Report", CommissionReport));
                Reports.Add(new ReportItemViewModel("Sales Commission Report", SalesmenCommissionReport));
            }

            Reports.Add(new ReportItemViewModel("Sales By Product Quantity Report", QtyProdSalesReport));
            Reports.Add(new ReportItemViewModel("Sales By Product Category Report", SalesProductCatReport));
            Reports.Add(new ReportItemViewModel("Transmission Report", TransmissionReport));
            Reports.Add(new ReportItemViewModel("Load Order Report", LoadOrderReport));

            if (Config.SAPOrderStatusReport)
            {
                Reports.Add(new ReportItemViewModel("SAP Report", SAPOrderReport));
            }
        }

        [RelayCommand]
        private async Task SelectReport(ReportItemViewModel report)
        {
            if (report != null && report.Action != null)
            {
                try
                {
                    await report.Action();
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    await _dialogService.ShowAlertAsync("Error navigating to report", "Error", "OK");
                }
            }
        }

        private async Task SalesReport()
        {
            try
            {
                await Shell.Current.GoToAsync("salesreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Sales Report", "Error", "OK");
            }
        }

        private async Task PaymentsReport()
        {
            try
            {
                await Shell.Current.GoToAsync("paymentsreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Payments Report", "Error", "OK");
            }
        }

        private async Task CommissionReport()
        {
            try
            {
                await Shell.Current.GoToAsync("commissionreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Commission Report", "Error", "OK");
            }
        }

        private async Task SalesmenCommissionReport()
        {
            try
            {
                await Shell.Current.GoToAsync("salesmencommissionreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Salesmen Commission Report", "Error", "OK");
            }
        }

        private async Task QtyProdSalesReport()
        {
            try
            {
                await Shell.Current.GoToAsync("qtyprodsalesreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Qty Prod Sales Report", "Error", "OK");
            }
        }

        private async Task SalesProductCatReport()
        {
            try
            {
                await Shell.Current.GoToAsync("salesproductcatreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Sales Product Cat Report", "Error", "OK");
            }
        }

        private async Task TransmissionReport()
        {
            try
            {
                await Shell.Current.GoToAsync("transmissionreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Transmission Report", "Error", "OK");
            }
        }

        private async Task LoadOrderReport()
        {
            try
            {
                await Shell.Current.GoToAsync("loadorderreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to Load Order Report", "Error", "OK");
            }
        }

        private async Task SAPOrderReport()
        {
            try
            {
                await Shell.Current.GoToAsync("saporderreport");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error navigating to SAP Order Report", "Error", "OK");
            }
        }
    }

    public partial class ReportItemViewModel : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        public Func<Task>? Action { get; set; }

        public ReportItemViewModel(string name, Func<Task> action)
        {
            Name = name;
            Action = action;
        }
    }
}
