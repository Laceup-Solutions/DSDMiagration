using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public abstract partial class BaseReportPageViewModel : ObservableObject
    {
        protected readonly DialogService _dialogService;
        protected readonly ILaceupAppService _appService;

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

        [RelayCommand]
        protected abstract Task RunReport();

        [RelayCommand]
        protected abstract Task SendByEmail();
    }
}

