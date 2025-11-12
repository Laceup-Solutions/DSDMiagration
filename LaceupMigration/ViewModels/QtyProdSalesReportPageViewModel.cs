using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class QtyProdSalesReportPageViewModel : BaseReportPageViewModel
    {
        public QtyProdSalesReportPageViewModel(DialogService dialogService, ILaceupAppService appService)
            : base(dialogService, appService)
        {
        }

        protected override async Task RunReport()
        {
            IsLoading = true;
            try
            {
                // TODO: Implement report generation
                await _dialogService.ShowAlertAsync("Report generation to be implemented.", "Info", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override async Task SendByEmail()
        {
            IsLoading = true;
            try
            {
                // TODO: Implement email sending
                await _dialogService.ShowAlertAsync("Email sending to be implemented.", "Info", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

