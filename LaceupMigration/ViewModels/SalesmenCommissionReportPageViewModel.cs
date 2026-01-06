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
    public partial class SalesmenCommissionReportPageViewModel : BaseReportPageViewModel
    {
        [ObservableProperty] private bool _excludeTax;
        [ObservableProperty] private string _selectedSalesman = string.Empty;
        [ObservableProperty] private int _selectedSalesmanId = 0;
        [ObservableProperty] private bool _showSalesmanSelection = false;
        [ObservableProperty] private bool _showExcludeTax = true;

        public SalesmenCommissionReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
            : base(dialogService, appService)
        {
            InitializeSalesmanSelection();
            
            if (Config.CheckCommunicatorVersion("30.0.0.0") && !Config.ShowOldReportsRegardless)
            {
                ShowExcludeTax = false;
            }
        }

        private void InitializeSalesmanSelection()
        {
            if (Config.CanSelectSalesman)
            {
                GetSalesmanList();
                ShowSalesmanSelection = true;
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
        }

        [RelayCommand]
        private async Task SelectSalesman()
        {
            if (!Config.CanSelectSalesman)
                return;

            GetSalesmanList();

            var list = Salesman.List.OrderBy(x => x.Name).Select(x => x.Name).ToList();

            var selectedName = await _dialogService.ShowActionSheetAsync("Select Salesman", "Cancel", null, list.ToArray());

            if (!string.IsNullOrEmpty(selectedName) && selectedName != "Cancel")
            {
                SelectedSalesman = selectedName;
                var s = Salesman.List.FirstOrDefault(x => x.Name == selectedName);
                SelectedSalesmanId = s != null ? s.Id : Config.SalesmanId;
            }
        }

        protected override string GetBaseCommand()
        {
            var cmd = base.GetBaseCommand();

            int slm = 0;
            if (ShowSalesmanSelection && !string.IsNullOrEmpty(SelectedSalesman))
            {
                slm = SelectedSalesmanId;
            }
            else if (!ShowSalesmanSelection)
            {
                slm = Config.SalesmanId;
            }

            cmd += "|" + slm;
            cmd += "|" + (ExcludeTax ? "1" : "0");

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
                return DataProvider.GetSalesmenCommReport(command);
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

