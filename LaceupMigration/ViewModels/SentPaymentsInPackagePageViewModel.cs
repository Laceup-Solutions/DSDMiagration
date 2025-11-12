using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SentPaymentsInPackagePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private string _packagePath = string.Empty;

        [ObservableProperty] private ObservableCollection<SentPaymentInPackageItemViewModel> _payments = new();
        [ObservableProperty] private string _totalAmountText = string.Empty;
        [ObservableProperty] private bool _showTotal;

        public SentPaymentsInPackagePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotal = !Config.HidePriceInTransaction;
        }

        public async Task InitializeAsync(string packagePath)
        {
            try
            {
                _packagePath = packagePath;
                var packages = SentPaymentPackage.Packages();
                var package = packages.FirstOrDefault(x => string.Equals(x.PackagePath, packagePath, StringComparison.InvariantCulture));

                if (package == null)
                {
                    await _dialogService.ShowAlertAsync("Package not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Payments.Clear();
                var packagePayments = package.PackageContent().OrderByDescending(x => x.Date).ToList();
                double total = 0;

                foreach (var payment in packagePayments)
                {
                    Payments.Add(new SentPaymentInPackageItemViewModel(payment));
                    total += payment.Amount;
                }

                TotalAmountText = $"Total: {total.ToCustomString()}";
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading payments: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public partial class SentPaymentInPackageItemViewModel : ObservableObject
    {
        private readonly SentPayment _payment;

        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _paymentType = string.Empty;
        [ObservableProperty] private string _amountText = string.Empty;
        [ObservableProperty] private string _dateText = string.Empty;
        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private bool _showComment;
        [ObservableProperty] private bool _showTotal;

        public SentPaymentInPackageItemViewModel(SentPayment payment)
        {
            _payment = payment;
            ShowTotal = !Config.HidePriceInTransaction;

            ClientName = payment.GetClient?.ClientName ?? "Unknown Client";
            PaymentType = payment.PaymentType;
            AmountText = $"Amount: {payment.Amount.ToCustomString()}";
            DateText = $"Date: {payment.Date:yyyy/MM/dd hh:mm tt}";
            CommentText = payment.Comment ?? string.Empty;
            ShowComment = !string.IsNullOrEmpty(CommentText);
        }
    }
}

