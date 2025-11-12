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
    public partial class SentOrdersPageViewModel : ObservableObject
    {
        private enum SelectedOption
        {
            All = 0,
            Sales_Order = 1,
            Credit_Order = 2,
            Return_Order = 3,
            Quote = 4,
            Sales_Invoice = 5,
            Credit_Invoice = 6,
            Return_Invoice = 7,
            Consignment_Invoice = 8,
            ParLevel_Invoice = 9,
            No_Service = 10
        }

        public readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private List<SentOrder> _sentOrdersList = new();
        private List<SentOrderItemViewModel> _originalSentOrdersList = new();
        private SelectedOption _optionSelected = SelectedOption.All;
        private string _searchCriteria = string.Empty;
        private bool _isUpdatingSelectAll;

        [ObservableProperty] private ObservableCollection<SentOrderItemViewModel> _sentOrders = new();
        [ObservableProperty] private ObservableCollection<string> _transactionTypeOptions = new();
        [ObservableProperty] private string _selectedTransactionType = "All";
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private DateTime _selectedDate = DateTime.MinValue;
        [ObservableProperty] private string _selectedDateText = string.Empty;
        [ObservableProperty] private bool _isSelectAllChecked;
        [ObservableProperty] private string _selectAllText = "Select All";
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _showButtonsLayout;

        public ObservableCollection<SentOrder> SelectedOrders { get; } = new();

        public SentOrdersPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            BuildTransactionTypeOptions();
            
            MinDate = DateTime.Now.AddDays(-Config.DaysToShowSentOrders);
            MaxDate = DateTime.Now;
            SelectedDate = MinDate;
        }

        private void BuildTransactionTypeOptions()
        {
            var options = new List<string> { "All" };
            options.Add("No Service");

            if (Config.PreSale)
            {
                options.Add("Sales Order");
                if (Config.AllowCreditOrders)
                {
                    options.Add("Credit Order");
                    if (Config.UseReturnInvoice || Config.UseReturnOrder)
                        options.Add("Return Order");
                }
            }

            if (Config.UseQuote)
                options.Add("Quote");

            options.Add("Sales Invoice");

            if (Config.AllowCreditOrders)
            {
                options.Add("Credit Invoice");
                if (Config.UseReturnInvoice || Config.UseReturnOrder)
                    options.Add("Return Invoice");
            }

            if (Config.Consignment)
                options.Add("Consignment");

            if (Config.ClientDailyPL)
                options.Add("Par Level Invoice");

            TransactionTypeOptions = new ObservableCollection<string>(options);
        }

        public async Task OnAppearingAsync()
        {
            if (!DataAccess.CanUseApplication() || !DataAccess.ReceivedData)
            {
                ShowButtonsLayout = false;
                SentOrders.Clear();
                return;
            }

            ShowButtonsLayout = true;
            IsLoading = true;

            try
            {
                if (SelectedDate == DateTime.MinValue)
                {
                    SelectedDate = DateTime.Now.AddDays(-Config.DaysToShowSentOrders);
                }

                await LoadSentOrdersAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSentOrdersAsync()
        {
            await Task.Run(() =>
            {
                _sentOrdersList.Clear();
                _originalSentOrdersList.Clear();

                var packages = SentOrderPackage.Packages(SelectedDate);
                if (packages.Count > 0)
                {
                    foreach (var pck in packages)
                    {
                        var packageOrders = pck.PackageOrders();
                        foreach (var sentOrder in packageOrders)
                        {
                            if (!_sentOrdersList.Any(x => x.OrderUniqueId == sentOrder.OrderUniqueId))
                            {
                                sentOrder.CellType = CellType.Header;
                                _sentOrdersList.Add(sentOrder);
                            }
                        }
                    }
                }

                _sentOrdersList = _sentOrdersList.OrderByDescending(x => x.Date).ToList();
                foreach (var order in _sentOrdersList)
                {
                    _originalSentOrdersList.Add(new SentOrderItemViewModel(order, this)
                    {
                        TransactionType = GetTransactionType(order),
                        Total = order.OrderTotalCost().ToCustomString()
                    });
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

        partial void OnSelectedTransactionTypeChanged(string value)
        {
            var index = TransactionTypeOptions.IndexOf(value);
            if (index >= 0)
            {
                _optionSelected = (SelectedOption)index;
            }
            else
            {
                _optionSelected = SelectedOption.All;
            }

            RefreshListView();
        }

        [ObservableProperty]
        private DateTime _minDate;

        [ObservableProperty]
        private DateTime _maxDate;

        partial void OnSelectedDateChanged(DateTime value)
        {
            SelectedDateText = SelectedDate.ToShortDateString();
            LoadSentOrdersAsync();
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
                    SelectedOrders.Clear();
                    foreach (var item in SentOrders)
                    {
                        item.IsChecked = false;
                    }
                }
                else
                {
                    SelectedOrders.Clear();
                    foreach (var item in SentOrders)
                    {
                        if (!SelectedOrders.Any(x => x.OrderUniqueId == item.Order.OrderUniqueId))
                        {
                            SelectedOrders.Add(item.Order);
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
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to print.", "Alert", "OK");
                return;
            }

            // TODO: Implement print functionality
            await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SendByEmail()
        {
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to send.", "Alert", "OK");
                return;
            }

            // TODO: Implement send by email functionality
            await _dialogService.ShowAlertAsync("Send by email functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Resend()
        {
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to resend.", "Alert", "OK");
                return;
            }

            // TODO: Implement resend functionality
            await _dialogService.ShowAlertAsync("Resend functionality to be implemented.", "Info", "OK");
        }

        public void ToggleOrderSelection(SentOrder order)
        {
            var existing = SelectedOrders.FirstOrDefault(x => x.OrderUniqueId == order.OrderUniqueId);
            if (existing != null)
            {
                SelectedOrders.Remove(existing);
            }
            else
            {
                SelectedOrders.Add(order);
            }

            RefreshListHeader();
        }

        public async Task HandleOrderSelectedAsync(SentOrder order)
        {
            // TODO: Navigate to SentOrdersOrdersListPage when implemented
            await _dialogService.ShowAlertAsync("Order details view is not yet implemented in MAUI version.", "Info", "OK");
        }

        private void RefreshListView()
        {
            var list = _originalSentOrdersList.ToList();

            if (SelectedDate != DateTime.MinValue)
            {
                list = list.Where(x => x.Order.Date.Date >= SelectedDate.Date).ToList();
            }

            switch (_optionSelected)
            {
                case SelectedOption.All:
                    list = list.ToList();
                    break;
                case SelectedOption.Sales_Order:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.SalesOrder).ToList();
                    break;
                case SelectedOption.Credit_Order:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.CreditOrder).ToList();
                    break;
                case SelectedOption.Return_Order:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.ReturnOrder).ToList();
                    break;
                case SelectedOption.Quote:
                    list = list.Where(x => x.Order.OrderType == OrderType.Quote).ToList();
                    break;
                case SelectedOption.Sales_Invoice:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.SalesInvoice).ToList();
                    break;
                case SelectedOption.Credit_Invoice:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.CreditInvoice).ToList();
                    break;
                case SelectedOption.Return_Invoice:
                    list = list.Where(x => x.Order.TransactionType == TransactionType.ReturnInvoice).ToList();
                    break;
                case SelectedOption.Consignment_Invoice:
                    list = list.Where(x => x.Order.OrderType == OrderType.Consignment).ToList();
                    break;
                case SelectedOption.ParLevel_Invoice:
                    list = list.Where(x => x.Order.OrderType == OrderType.Order).ToList();
                    break;
                case SelectedOption.No_Service:
                    list = list.Where(x => x.Order.OrderType == OrderType.NoService).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                list = list.Where(x => 
                    (x.Order.ClientName?.Contains(_searchCriteria, StringComparison.InvariantCultureIgnoreCase) == true) ||
                    (x.Order.PrintedOrderId?.Contains(_searchCriteria, StringComparison.InvariantCultureIgnoreCase) == true)).ToList();
            }

            SentOrders.Clear();
            foreach (var item in list)
            {
                item.IsChecked = SelectedOrders.Any(x => x.OrderUniqueId == item.Order.OrderUniqueId);
                SentOrders.Add(item);
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
                    IsSelectAllChecked = SelectedOrders.Count > 0;
                }
                finally
                {
                    _isUpdatingSelectAll = false;
                }
            }
            else
            {
                IsSelectAllChecked = SelectedOrders.Count > 0;
            }
            if (IsSelectAllChecked)
            {
                SelectAllText = $"Selected: {SelectedOrders.Count}";
            }
            else
            {
                SelectAllText = "Select All";
            }
        }

        private string GetTransactionType(SentOrder order)
        {
            switch (order.OrderType)
            {
                case OrderType.Order:
                    switch (order.TransactionType)
                    {
                        case TransactionType.SalesOrder:
                            return "Sales Order";
                        case TransactionType.SalesInvoice:
                            return "Sales Invoice";
                        default:
                            return "Transaction";
                    }
                case OrderType.Credit:
                    switch (order.TransactionType)
                    {
                        case TransactionType.CreditOrder:
                            return "Credit Order";
                        case TransactionType.CreditInvoice:
                            return "Credit Invoice";
                        default:
                            return "Transaction";
                    }
                case OrderType.Quote:
                    return "Quote";
                case OrderType.Consignment:
                    return "Consignment";
                case OrderType.Sample:
                    return "Sample";
                case OrderType.Return:
                    return "Return";
                case OrderType.NoService:
                    return "No Service";
                default:
                    return "Transaction";
            }
        }
    }

    public partial class SentOrderItemViewModel : ObservableObject
    {
        private readonly SentOrder _order;
        private readonly SentOrdersPageViewModel _parent;

        [ObservableProperty] private bool _isChecked;
        [ObservableProperty] private string _transactionType = string.Empty;
        [ObservableProperty] private string _total = string.Empty;

        public SentOrderItemViewModel(SentOrder order, SentOrdersPageViewModel parent)
        {
            _order = order;
            _parent = parent;
        }

        public string OrderNumberText => !string.IsNullOrEmpty(_order.PrintedOrderId) ? $"Doc#: {_order.PrintedOrderId}" : $"Order #{_order.OrderId}";
        public string DateClientText => $"{_order.Date:yyyy/MM/dd hh:mm tt} {_order.ClientName}";
        public bool ShowTotal => !Config.HidePriceInTransaction && _order.OrderType != OrderType.NoService;

        partial void OnIsCheckedChanged(bool value)
        {
            _parent.ToggleOrderSelection(_order);
        }

        [RelayCommand]
        private async Task ViewDetails()
        {
            // Find the package path for this order
            var selectedDate = DateTime.Now.AddDays(-Config.DaysToShowSentOrders);
            var packages = SentOrderPackage.Packages(selectedDate);
            string? packagePath = null;

            foreach (var pck in packages)
            {
                var packageOrders = pck.PackageOrders().ToList();
                if (packageOrders.Any(x => x.OrderUniqueId == _order.OrderUniqueId))
                {
                    packagePath = pck.PackagePath;
                    break;
                }
            }

            if (packagePath != null)
            {
                await Shell.Current.GoToAsync($"sentordersorderslist?packagePath={Uri.EscapeDataString(packagePath)}&orderId={_order.OrderId}");
            }
            else
            {
                await _parent._dialogService.ShowAlertAsync("Package not found for this order.", "Error", "OK");
            }
        }

        public SentOrder Order => _order;
    }
}
