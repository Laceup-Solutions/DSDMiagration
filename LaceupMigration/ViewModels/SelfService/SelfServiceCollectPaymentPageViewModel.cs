using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCollectPaymentPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<PaymentItemViewModel> _payments = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isSelectAllChecked;

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        private bool _isUpdatingSelectAll;

        public SelfServiceCollectPaymentPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    ClientName = _order.Client?.ClientName ?? string.Empty;
                }
            }

            LoadPayments();
        }

        public void OnAppearing()
        {
            LoadPayments();
            RefreshTotal();
        }

        private void LoadPayments()
        {
            if (_order?.Client == null)
                return;

            Payments.Clear();

            var paymentsForClient = InvoicePayment.List
                .Where(x => x.Client.ClientId == _order.Client.ClientId)
                .ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                paymentsForClient = paymentsForClient.Where(x => 
                    x.DateCreated.ToShortDateString().ToLowerInvariant().Contains(searchLower) ||
                    x.Components.Any(c => c.Ref.ToLowerInvariant().Contains(searchLower))
                ).ToList();
            }

            foreach (var payment in paymentsForClient.OrderByDescending(x => x.DateCreated))
            {
                var item = new PaymentItemViewModel(payment);
                item.SetParent(this);
                Payments.Add(item);
            }

            // If no payments, redirect to invoice selection
            if (!paymentsForClient.Any())
            {
                NavigateToInvoiceSelection();
            }
        }

        private async Task NavigateToInvoiceSelection()
        {
            if (_order?.Client != null)
            {
                await Shell.Current.GoToAsync($"selectinvoice?clientId={_order.Client.ClientId}&orderId={_order.OrderId}&fromSelfService=true");
            }
        }

        [RelayCommand]
        private void SelectAll()
        {
            if (_isUpdatingSelectAll) return;
            
            _isUpdatingSelectAll = true;
            try
            {
                IsSelectAllChecked = !IsSelectAllChecked;
                foreach (var payment in Payments)
                {
                    payment.IsSelected = IsSelectAllChecked;
                }
                RefreshTotal();
            }
            finally
            {
                _isUpdatingSelectAll = false;
            }
        }

        [RelayCommand]
        private async Task Send()
        {
            var selectedPayments = Payments.Where(x => x.IsSelected).ToList();
            if (selectedPayments.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select payments to send.", "Alert", "OK");
                return;
            }

            try
            {
                var payments = selectedPayments.Select(x => x.Payment).ToList();
                DataAccess.SendInvoicePaymentsBySource(payments);

                await _dialogService.ShowAlertAsync("Payments sent successfully.", "Success", "OK");
                LoadPayments();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error sending payments: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task PayBills()
        {
            if (_order?.Client == null)
                return;

            await Shell.Current.GoToAsync($"selectinvoice?clientId={_order.Client.ClientId}&orderId={_order.OrderId}&fromSelfService=true");
        }

        [RelayCommand]
        private async Task DeletePayment(PaymentItemViewModel paymentItem)
        {
            if (paymentItem == null)
                return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Are you sure you want to delete this payment?",
                "Confirm Delete",
                "Yes",
                "No");

            if (!confirm)
                return;

            try
            {
                paymentItem.Payment.Delete();
                LoadPayments();
                RefreshTotal();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error deleting payment: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void RefreshTotal()
        {
            var total = Payments.Where(x => x.IsSelected).Sum(x => x.Payment.TotalPaid);
            TotalText = $"Total: {total.ToCustomString()}";
        }

        partial void OnSearchTextChanged(string value)
        {
            LoadPayments();
        }
    }

    public partial class PaymentItemViewModel : ObservableObject
    {
        public InvoicePayment Payment { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _paymentDateText;

        [ObservableProperty]
        private string _paymentMethodText;

        [ObservableProperty]
        private string _amountText;

        public PaymentItemViewModel(InvoicePayment payment)
        {
            Payment = payment;
            PaymentDateText = payment.DateCreated.ToShortDateString();
            PaymentMethodText = string.Join(", ", payment.Components.Select(x => x.PaymentMethod.ToString().Replace("_", " ")));
            AmountText = $"Amount: {payment.TotalPaid.ToCustomString()}";
        }

        private SelfServiceCollectPaymentPageViewModel _parentViewModel;

        public void SetParent(SelfServiceCollectPaymentPageViewModel parent)
        {
            _parentViewModel = parent;
        }

        partial void OnIsSelectedChanged(bool value)
        {
            _parentViewModel?.RefreshTotal();
        }
    }
}

