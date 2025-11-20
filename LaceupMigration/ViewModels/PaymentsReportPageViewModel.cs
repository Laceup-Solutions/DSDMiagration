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
    public partial class PaymentsReportPageViewModel : BaseReportPageViewModel
    {
        [ObservableProperty] private string _selectedSalesman = "All Salesmen";
        [ObservableProperty] private int _selectedSalesmanId = 0;
        [ObservableProperty] private bool _showSalesmanSelection = false;

        public PaymentsReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
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
                return DataAccess.GetPaymentsReport(command);
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

