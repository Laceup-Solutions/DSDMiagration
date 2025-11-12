using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class PrintReportsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private bool _printSalesReport;
        [ObservableProperty] private bool _printPaymentsReport;
        [ObservableProperty] private bool _printInventoryReport;
        [ObservableProperty] private bool _printCommissionReport;
        [ObservableProperty] private bool _printRouteReturnsReport;

        public PrintReportsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        [RelayCommand]
        private async Task PrintReports()
        {
            if (!PrintSalesReport && !PrintPaymentsReport && !PrintInventoryReport && 
                !PrintCommissionReport && !PrintRouteReturnsReport)
            {
                await _dialogService.ShowAlertAsync("Please select at least one report to print.", "Info", "OK");
                return;
            }

            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Print Reports",
                    "Print selected reports?",
                    "Yes",
                    "No");
                
                if (!confirmed)
                    return;
                
                PrinterProvider.PrintDocument((int copies) =>
                {
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    int index = 1;
                    int count = 0;
                    
                    // Count reports to print
                    if (PrintSalesReport) count++;
                    if (PrintPaymentsReport) count++;
                    if (PrintInventoryReport) count++;
                    if (PrintCommissionReport) count++;
                    if (PrintRouteReturnsReport) count++;
                    
                    if (count == 0)
                        return "No reports selected";
                    
                    for (int i = 0; i < copies; i++)
                    {
                        if (PrintSalesReport)
                        {
                            if (!printer.PrintOrdersCreatedReport(index, count))
                                return "Error printing sales report";
                            index++;
                        }
                        
                        if (PrintPaymentsReport)
                        {
                            var listOfPayments = Config.VoidPayments ? InvoicePayment.ListWithVoids : InvoicePayment.List;
                            if (listOfPayments.Count > 0 || Config.PrintPaymentRegardless)
                            {
                                if (!printer.PrintReceivedPaymentsReport(index, count))
                                    return "Error printing payments report";
                                index++;
                            }
                        }
                        
                        if (PrintInventoryReport)
                        {
                            var map = DataAccess.ExtendedSendTheLeftOverInventory();
                            if (map.Count > 0 || Config.PrintInveSettlementRegardless)
                            {
                                if (!printer.InventorySettlement(index, count))
                                    return "Error printing inventory report";
                                index++;
                            }
                        }
                        
                        if (PrintCommissionReport)
                        {
                            if (!printer.PrintCreditReport(index, count))
                                return "Error printing commission report";
                            index++;
                        }
                        
                        if (PrintRouteReturnsReport)
                        {
                            // Route returns are printed separately, not as part of end of day reports
                            // This would need to be handled differently
                        }
                    }
                    
                    return string.Empty;
                });
                
                await _dialogService.ShowAlertAsync("Reports printed successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (System.Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing reports: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

