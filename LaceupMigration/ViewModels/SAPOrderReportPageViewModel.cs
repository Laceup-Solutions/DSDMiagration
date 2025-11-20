using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SAPOrderReportPageViewModel : BaseReportPageViewModel
    {
        [ObservableProperty] private string _selectedSalesman = "All Salesmen";
        [ObservableProperty] private int _selectedSalesmanId = 0;
        [ObservableProperty] private bool _showSalesmanSelection = false;
        [ObservableProperty] private string _selectedClient = "All";
        [ObservableProperty] private int _selectedClientId = 0;
        [ObservableProperty] private string _selectedStatus = string.Empty; // Empty string for "All" (matches Xamarin)
        [ObservableProperty] private string _selectedStatusDisplay = "All";

        public SAPOrderReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
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
        }

        private void GetSalesmanList()
        {
            if (Salesman.List.Count == 0)
            {
                try
                {
                    DataAccess.GetSalesmanList();
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e);
                }
            }
        }

        [RelayCommand]
        private async Task SelectSalesman()
        {
            if (!Config.CanSelectSalesman)
                return;

            GetSalesmanList();

            var list = Salesman.List.OrderBy(x => x.Name).Select(x => x.Name).ToList();
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
                    var s = Salesman.List.FirstOrDefault(x => x.Name == selectedName);
                    SelectedSalesmanId = s != null ? s.Id : 0;
                }
            }
        }

        [RelayCommand]
        private async Task SelectClient()
        {
            var list = Client.Clients.OrderBy(x => x.ClientName).Select(x => x.ClientName).ToList();
            list.Insert(0, "All");

            var selectedName = await _dialogService.ShowActionSheetAsync("Select Client", "Cancel", null, list.ToArray());

            if (!string.IsNullOrEmpty(selectedName) && selectedName != "Cancel")
            {
                SelectedClient = selectedName;
                if (selectedName == "All")
                {
                    SelectedClientId = 0;
                }
                else
                {
                    var c = Client.Clients.FirstOrDefault(x => x.ClientName == selectedName);
                    SelectedClientId = c != null ? c.ClientId : 0;
                }
            }
        }

        [RelayCommand]
        private async Task SelectStatus()
        {
            if (string.IsNullOrEmpty(SapStatus.StatusesAsString))
            {
                SapStatus.Load();
            }

            var statusList = !string.IsNullOrEmpty(SapStatus.StatusesAsString) 
                ? SapStatus.StatusesAsString.Split("|").ToList() 
                : new List<string>();
            statusList.Insert(0, "All");

            var selectedName = await _dialogService.ShowActionSheetAsync("Select Status", "Cancel", null, statusList.ToArray());

            if (!string.IsNullOrEmpty(selectedName) && selectedName != "Cancel")
            {
                SelectedStatusDisplay = selectedName;
                SelectedStatus = selectedName == "All" ? string.Empty : selectedName;
            }
        }

        protected override string GetBaseCommand()
        {
            // Match Xamarin SAPOrderStatusReport.GetCommand() exactly
            var dateTime = DateFrom;
            var cmd = dateTime.ToString("MM/dd/yyyy");

            int slm = 0;
            // Match Xamarin: if (salesman != null) ... else cmd += "|" + Config.SalesmanId;
            if (ShowSalesmanSelection)
            {
                if (SelectedSalesman != "All Salesmen")
                {
                    slm = SelectedSalesmanId;
                }
                cmd += "|" + slm;
            }
            else
            {
                cmd += "|" + Config.SalesmanId;
            }

            if (Config.DaysToRunReports > 0)
            {
                var dateFrom = DateTime.Now.Date.AddDays(-Config.DaysToRunReports);
                var dateTo = DateTime.Now.Date;
                cmd += "|" + (long)dateFrom.Ticks + "|" + (long)dateTo.Ticks;
            }
            else
            {
                cmd += "|" + (long)DateFrom.Ticks;
                cmd += "|" + (long)DateTo.Ticks;
            }

            // Match Xamarin: if (ClientSpinner != null) { ... cmd += "|" + client; }
            // Client is always shown in UI, so always add it
            int client = 0;
            if (SelectedClient != "All")
            {
                client = SelectedClientId;
            }
            cmd += "|" + client;

            // Match Xamarin: if (StatusSpinner != null) { ... cmd += "|" + selectedStatus; }
            // Status is always shown in UI, so always add it
            cmd += "|" + SelectedStatus;

            return cmd;
        }

        protected override string GetReport(string command)
        {
            try
            {
                return DataAccess.GetSAPReport(command);
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

