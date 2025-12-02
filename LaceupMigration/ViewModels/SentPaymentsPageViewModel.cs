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
            if (!DataAccess.CanUseApplication() || !DataAccess.ReceivedData)
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
            if (_isUpdatingSelectAll) return;
            
            _isUpdatingSelectAll = true;
            try
            {
                if (IsSelectAllChecked)
                {
                    SelectedPayments.Clear();
                    foreach (var item in SentPayments)
                    {
                        item.IsChecked = false;
                    }
                }
                else
                {
                    SelectedPayments.Clear();
                    foreach (var item in SentPayments)
                    {
                        if (!SelectedPayments.Any(x => x.OrderUniqueId == item.Payment.OrderUniqueId && x.ClientId == item.Payment.ClientId))
                        {
                            SelectedPayments.Add(item.Payment);
                            item.IsChecked = true;
                        }
                    }
                }

                RefreshListHeader();
            }
            finally
            {
                _isUpdatingSelectAll = false;
            }
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
                        // Find the actual InvoicePayment from the OrderUniqueId
                        InvoicePayment? invoicePayment = null;
                        if (!string.IsNullOrEmpty(sentPayment.OrderUniqueId))
                        {
                            invoicePayment = InvoicePayment.List.FirstOrDefault(x => 
                                !string.IsNullOrEmpty(x.OrderId) && 
                                x.OrderId.Contains(sentPayment.OrderUniqueId));
                        }

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
                // Find the actual InvoicePayment from the OrderUniqueId
                var payments = new List<InvoicePayment>();
                foreach (var sentPayment in SelectedPayments)
                {
                    if (!string.IsNullOrEmpty(sentPayment.OrderUniqueId))
                    {
                        var payment = InvoicePayment.List.FirstOrDefault(x => 
                            !string.IsNullOrEmpty(x.OrderId) && 
                            x.OrderId.Contains(sentPayment.OrderUniqueId));
                        if (payment != null)
                            payments.Add(payment);
                    }
                }

                if (payments.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Could not find payments to send.", "Alert", "OK");
                    return;
                }

                // Send each payment by email (matches Xamarin payment email sending)
                foreach (var payment in payments)
                {
                    // InvoicePayment has an Invoices() method that returns the invoices associated with the payment
                    var invoices = payment.Invoices();
                    foreach (var invoice in invoices)
                    {
                        PdfHelper.SendInvoiceByEmail(invoice);
                    }
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

            // TODO: Implement resend functionality
            await _dialogService.ShowAlertAsync("Resend functionality to be implemented.", "Info", "OK");
        }

        public void TogglePaymentSelection(SentPayment payment)
        {
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
            if (!_isUpdatingSelectAll)
            {
                _isUpdatingSelectAll = true;
                try
                {
                    IsSelectAllChecked = SelectedPayments.Count > 0;
                }
                finally
                {
                    _isUpdatingSelectAll = false;
                }
            }
            else
            {
                IsSelectAllChecked = SelectedPayments.Count > 0;
            }
            
            if (IsSelectAllChecked)
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
        private async Task ViewDetails()
        {
            // Find the package path for this payment
            var packages = SentPaymentPackage.Packages();
            string? packagePath = null;

            foreach (var pck in packages)
            {
                var packagePayments = pck.PackageContent();
                if (packagePayments.Any(x => x.OrderUniqueId == _payment.OrderUniqueId))
                {
                    packagePath = pck.PackagePath;
                    break;
                }
            }

            if (packagePath != null)
            {
                await Shell.Current.GoToAsync($"sentpaymentsinpackage?packagePath={Uri.EscapeDataString(packagePath)}");
            }
            else
            {
                if (DialogHelper._dialogService != null)
                {
                    await DialogHelper._dialogService.ShowAlertAsync("Package not found for this payment.", "Error", "OK");
                }
            }
        }
    }
}
