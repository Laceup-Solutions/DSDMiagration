using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
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

            if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "30.0.0.0") && !Config.ShowOldReportsRegardless)
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
            if (report != null)
            {
                report.Action?.Invoke();
            }
        }

        private async void SalesReport()
        {
            await Shell.Current.GoToAsync("salesreport");
        }

        private async void PaymentsReport()
        {
            await Shell.Current.GoToAsync("paymentsreport");
        }

        private async void CommissionReport()
        {
            await Shell.Current.GoToAsync("commissionreport");
        }

        private async void SalesmenCommissionReport()
        {
            await Shell.Current.GoToAsync("salesmencommissionreport");
        }

        private async void QtyProdSalesReport()
        {
            await Shell.Current.GoToAsync("qtyprodsalesreport");
        }

        private async void SalesProductCatReport()
        {
            await Shell.Current.GoToAsync("salesproductcatreport");
        }

        private async void TransmissionReport()
        {
            await Shell.Current.GoToAsync("transmissionreport");
        }

        private async void LoadOrderReport()
        {
            await Shell.Current.GoToAsync("loadorderreport");
        }

        private async void SAPOrderReport()
        {
            await Shell.Current.GoToAsync("saporderreport");
        }
    }

    public partial class ReportItemViewModel : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        public System.Action? Action { get; set; }

        public ReportItemViewModel(string name, System.Action action)
        {
            Name = name;
            Action = action;
        }
    }
}
