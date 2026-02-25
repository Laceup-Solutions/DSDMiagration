using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
    public partial class SentPaymentsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private List<SentPayment> _sentPaymentsList = new();
        private List<SentPaymentItemViewModel> _originalSentPaymentsList = new();
        private string _searchCriteria = string.Empty;
        private bool _isUpdatingSelectAll;

        [ObservableProperty] private ObservableCollection<SentPaymentItemViewModel> _sentPayments = new();
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private bool _isSelectAllChecked;
        [ObservableProperty] private string _selectAllText = "Select All";
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _showButtonsLayout;
        [ObservableProperty] private bool _showTotal;

        public ObservableCollection<SentPayment> SelectedPayments { get; } = new();

        public SentPaymentsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            if (!DataProvider.CanUseApplication() || !Config.ReceivedData)
            {
                ShowButtonsLayout = false;
                SentPayments.Clear();
                return;
            }

            ShowButtonsLayout = true;
            ShowTotal = !Config.HidePriceInTransaction;
            IsLoading = true;

            try
            {
                await LoadSentPaymentsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSentPaymentsAsync()
        {
            await Task.Run(() =>
            {
                _sentPaymentsList.Clear();
                _originalSentPaymentsList.Clear();

                var packages = SentPaymentPackage.Packages();
                foreach (var pck in packages)
                {
                    var packagePayments = pck.PackageContent();
                    foreach (var payment in packagePayments)
                    {
                        if (!_sentPaymentsList.Any(x => x.OrderUniqueId == payment.OrderUniqueId && x.ClientId == payment.ClientId))
                        {
                            _sentPaymentsList.Add(payment);
                        }
                    }
                }

                _sentPaymentsList = _sentPaymentsList.OrderByDescending(x => x.Date).ToList();
                foreach (var payment in _sentPaymentsList)
                {
                    _originalSentPaymentsList.Add(new SentPaymentItemViewModel(payment, this));
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshListView();
                });
            });
        }

        partial void OnSearchQueryChanged(string value)
        {
            _searchCriteria = value;
            RefreshListView();
        }

        [RelayCommand]
        private void SelectAll()
        {
            if(_isUpdatingASingleCell)
                return;
            
            _isUpdatingSelectAll = true;

            SelectedPayments.Clear();

            if (!IsSelectAllChecked)
            {
                foreach (var item in SentPayments)
                    item.IsChecked = false;
                RefreshListHeader();
            }
            else
            {
                foreach (var item in SentPayments)
                    SelectedPayments.Add(item.Payment);
                RefreshListHeader();
                foreach (var item in SentPayments)
                    item.IsChecked = true;
            }

            _isUpdatingSelectAll = false;
        }

        [RelayCommand]
        private async Task Print()
        {
            if (SelectedPayments.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one payment to print.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    foreach (var sentPayment in SelectedPayments)
                    {
                        var packagePath = sentPayment.PackagePath;
                        if (string.IsNullOrEmpty(packagePath))
                        {
                            var pkg = SentPaymentPackage.Packages().FirstOrDefault(p => p.PackageContent().Any(c => c.ClientId == sentPayment.ClientId && c.OrderUniqueId == sentPayment.OrderUniqueId));
                            packagePath = pkg?.PackagePath;
                        }
                        var invoicePayment = SentPaymentPackage.CreateTemporalPaymentFromFile(packagePath, sentPayment);
                        if (invoicePayment == null)
                            continue;

                        for (int i = 0; i < copies; i++)
                        {
                            if (!printer.PrintPayment(invoicePayment))
                                allWent = false;
                        }
                    }

                    if (!allWent)
                        return "Error printing payments";
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task SendByEmail()
        {
            if (SelectedPayments.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one payment to send.", "Alert", "OK");
                return;
            }

            try
            {
                var payments = new List<InvoicePayment>();
                foreach (var sentPayment in SelectedPayments)
                {
                    var packagePath = sentPayment.PackagePath;
                    if (string.IsNullOrEmpty(packagePath))
                    {
                        var pkg = SentPaymentPackage.Packages().FirstOrDefault(p => p.PackageContent().Any(c => c.ClientId == sentPayment.ClientId && c.OrderUniqueId == sentPayment.OrderUniqueId));
                        packagePath = pkg?.PackagePath;
                    }
                    var payment = SentPaymentPackage.CreateTemporalPaymentFromFile(packagePath, sentPayment);
                    if (payment != null)
                        payments.Add(payment);
                }

                if (payments.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Could not find or decode payments to send.", "Alert", "OK");
                    return;
                }

                // Send each payment by email (matches PaymentSetValuesPageViewModel: GetPaymentPdf + SendPaymentByEmail)
                foreach (var payment in payments)
                {
                    var pdfFile = PdfHelper.GetPaymentPdf(payment);
                    if (string.IsNullOrEmpty(pdfFile))
                    {
                        await _dialogService.ShowAlertAsync("PDF could not be generated for a payment.", "Alert", "OK");
                        continue;
                    }

                    string toEmail = string.Empty;
                    if (payment.Client != null)
                        toEmail = UDFHelper.GetSingleUDF("email", payment.Client.ExtraPropertiesAsString);

                    string subject;
                    string body;
                    if (Config.EcoSkyWaterCustomEmail)
                    {
                        subject = "Eco SkyWater Payment";
                        body = string.Format(@"<html><body>Thank you for choosing Eco SkyWater the most sustainable bottled water on earth, every contribution helps towards a healthier plastic free environment.<br><br>For more information on how our water and plant based bottles are made, please visit <a href='{0}' >www.ecoskywater.com</a><br><br>Payments can be made through bank transfer using the attached banking information link: <a href='{1}'>https://ecoskywater-my.sharepoint.com/:b:/p/philip/EQdUS4WWb4tMhHQilv2-FzgBRy2w8yEbNW6XSCsp9ww1Vw?e=e65jGO</a><br><br>Please reach out to us with any feedback as we are continually improving our products.<br><br><br>Do Good Live Great,<br><br>Eco SkyWater<br><b> <span style='color:blue'>Local . Sustainable . Pure</span></b><br><br>E: <a href='{2}'>Sales@ecoskywater.com</a><br>IG: @ecoskywater<br>FB: @ecoskyh2o<br>T: 1 (246) 572-4587<br>C: 1 (246) 235-3269<br>Lot 1B Walkes Spring, St. Thomas, Barbados</body></html>",
                            "www.ecoskywater.com",
                            "https://ecoskywater-my.sharepoint.com/:b:/p/philip/EQdUS4WWb4tMhHQilv2-FzgBRy2w8yEbNW6XSCsp9ww1Vw?e=e65jGO",
                            "Sales@ecoskywater.com");
                    }
                    else
                    {
                        subject = "Payment Attached";
                        body = string.Empty;
                    }

                    var toAddresses = new List<string>();
                    if (!string.IsNullOrEmpty(toEmail))
                        toAddresses.Add(toEmail);
                    Config.helper?.SendOrderByEmail(pdfFile, subject, body, toAddresses);
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        [RelayCommand]
        private async Task Resend()
        {
            if (SelectedPayments.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one payment to resend.", "Alert", "OK");
                return;
            }

            try
            {
                await _dialogService.ShowLoadingAsync("Re-sending...");

                string err = string.Empty;
                await Task.Run(() =>
                {
                    try
                    {
                        var packageLocations = new List<string>();
                        foreach (var p in SelectedPayments)
                        {
                            if (!string.IsNullOrEmpty(p.PackagePath) && !packageLocations.Contains(p.PackagePath))
                                packageLocations.Add(p.PackagePath);
                        }

                        foreach (var package in packageLocations)
                        {
                            if (string.IsNullOrEmpty(package) || !System.IO.File.Exists(package))
                                continue;

                            string dstFileZipped = package + ".zip";
                            ZipMethods.ZipFile(package, dstFileZipped);
                            DataProvider.SendThePayments(dstFileZipped);
                            System.IO.File.Delete(dstFileZipped);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                        err = ex.Message;
                    }
                });

                await _dialogService.HideLoadingAsync();

                if (string.IsNullOrEmpty(err))
                    await _dialogService.ShowAlertAsync("Payments sent.", "Info", "OK");
                else
                    await _dialogService.ShowAlertAsync(err, "Alert", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(ex.Message, "Alert", "OK");
                _appService.TrackError(ex);
            }
        }

        private bool _isUpdatingASingleCell = false;
        public void TogglePaymentSelection(SentPayment payment)
        {
            if (_isUpdatingSelectAll)
                return;

            _isUpdatingASingleCell = true;
            
            var existing = SelectedPayments.FirstOrDefault(x => x.OrderUniqueId == payment.OrderUniqueId && x.ClientId == payment.ClientId);
            if (existing != null)
            {
                SelectedPayments.Remove(existing);
            }
            else
            {
                SelectedPayments.Add(payment);
            }

            RefreshListHeader();
            
            _isUpdatingASingleCell = false;

        }

        private void RefreshListView()
        {
            var list = _originalSentPaymentsList.ToList();

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                list = list.Where(x =>
                    (x.ClientName?.Contains(_searchCriteria, StringComparison.InvariantCultureIgnoreCase) == true) ||
                    (x.PaymentTypeText?.Contains(_searchCriteria, StringComparison.InvariantCultureIgnoreCase) == true)).ToList();
            }

            SentPayments.Clear();
            foreach (var item in list)
            {
                item.IsChecked = SelectedPayments.Any(x => x.OrderUniqueId == item.Payment.OrderUniqueId && x.ClientId == item.Payment.ClientId);
                SentPayments.Add(item);
            }

            RefreshListHeader();
        }

        private void RefreshListHeader()
        {
            // Only show Select All as checked when ALL items are selected (not when just one or some)
            bool allSelected = SentPayments.Count > 0 && SelectedPayments.Count == SentPayments.Count;
            if (!_isUpdatingSelectAll)
            {
                _isUpdatingSelectAll = true;
                try
                {
                    IsSelectAllChecked = allSelected;
                }
                finally
                {
                    _isUpdatingSelectAll = false;
                }
            }
            else
            {
                IsSelectAllChecked = allSelected;
            }

            if (SelectedPayments.Count > 0)
            {
                SelectAllText = $"Selected: {SelectedPayments.Count}";
                TotalText = $"Total: {SelectedPayments.Sum(x => x.Amount).ToCustomString()}";
            }
            else
            {
                SelectAllText = "Select All";
                TotalText = string.Empty;
            }
        }
    }

    public partial class SentPaymentItemViewModel : ObservableObject
    {
        private readonly SentPayment _payment;
        private readonly SentPaymentsPageViewModel _parent;

        [ObservableProperty] private bool _isChecked;

        public SentPaymentItemViewModel(SentPayment payment, SentPaymentsPageViewModel parent)
        {
            _payment = payment;
            _parent = parent;
        }

        public string ClientName => _payment.GetClient?.ClientName ?? "Unknown Client";
        public string PaymentType => _payment.PaymentType;
        public string PaymentTypeText => $"Type: {_payment.PaymentType}";
        public string AmountText => $"Amount: {_payment.Amount.ToCustomString()}";
        public string DateText => $"Date: {_payment.Date:yyyy/MM/dd hh:mm tt}";
        public string CommentText => string.IsNullOrEmpty(_payment.Comment) ? string.Empty : $"Comment: {_payment.Comment}";
        public bool ShowComment => !string.IsNullOrEmpty(_payment.Comment);
        public bool ShowTotal => !Config.HidePriceInTransaction;

        partial void OnIsCheckedChanged(bool value)
        {
            _parent.TogglePaymentSelection(_payment);
        }

        public SentPayment Payment => _payment;

        [RelayCommand]
        private void ViewDetails()
        {
            IsChecked = !IsChecked;
        }
    }
}
