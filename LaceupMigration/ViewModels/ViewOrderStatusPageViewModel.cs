using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ViewOrderStatusPageViewModel : ObservableObject
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

        internal enum SearchBy
        {
            ClientName = 0,
            InvoiceNum = 1
        }

        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private Dictionary<string, StatusSection> _statusTransactions = new();
        private int _transactionCount = 0;
        private SelectedOption _whatIsVisible = SelectedOption.All;
        private string _searchCriteria = string.Empty;
        private SearchBy _searchBy = SearchBy.ClientName;
        private bool _isUpdatingSelectAll;

        [ObservableProperty] private ObservableCollection<StatusSectionViewModel> _statusSections = new();
        [ObservableProperty] private ObservableCollection<string> _transactionTypeOptions = new();
        [ObservableProperty] private string _selectedTransactionType = "All";
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private bool _isSearchVisible;
        [ObservableProperty] private bool _showButtonsLayout;
        [ObservableProperty] private bool _showSelectAllLayout;
        [ObservableProperty] private bool _isSelectAllChecked;
        [ObservableProperty] private string _selectAllText = "Select All";
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private bool _showTotal;

        public ObservableCollection<OrdersInOS> SelectedOrders { get; } = new();

        public ViewOrderStatusPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            BuildTransactionTypeOptions();
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
                    if (Config.UseReturnInvoice)
                        options.Add("Return Order");
                }
            }

            if (Config.UseQuote)
                options.Add("Quote");

            options.Add("Sales Invoice");

            if (Config.AllowCreditOrders)
            {
                options.Add("Credit Invoice");
                if (Config.UseReturnInvoice)
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
            if (!DataAccess.CanUseApplication() || !Config.ReceivedData)
            {
                IsSearchVisible = false;
                ShowButtonsLayout = false;
                ShowSelectAllLayout = false;
                StatusSections.Clear();
                return;
            }

            IsSearchVisible = true;
            ShowButtonsLayout = true;
            ShowSelectAllLayout = true;
            ShowTotal = !Config.HideTransactionsTotal && !Config.HidePriceInTransaction;

            RefreshUI();
        }

        partial void OnSearchQueryChanged(string value)
        {
            _searchCriteria = value;
            FilterOrderStatus();
        }

        partial void OnSelectedTransactionTypeChanged(string value)
        {
            var index = TransactionTypeOptions.IndexOf(value);
            if (index >= 0)
            {
                _whatIsVisible = (SelectedOption)index;
            }
            else
            {
                _whatIsVisible = SelectedOption.All;
            }

            SelectedOrders.Clear();
            RefreshListHeader();
            FilterOrderStatus();
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
                }
                else
                {
                    SelectedOrders.Clear();
                    var orders = GetOrdersForCurrentFilter();
                    foreach (var order in orders)
                    {
                        SelectedOrders.Add(order);
                    }
                }

                RefreshListHeader();
                RefreshStatusSections();
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
                await _dialogService.ShowAlertAsync("Select transactions to be printed.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    foreach (var orderInOS in SelectedOrders)
                    {
                        var order = Order.Orders.FirstOrDefault(x => x.OrderId == orderInOS.OrderId);
                        if (order == null)
                            continue;

                        for (int i = 0; i < copies; i++)
                        {
                            bool result = false;
                            if (order.OrderType == OrderType.Consignment)
                            {
                                if (Config.UseFullConsignment)
                                    result = printer.PrintFullConsignment(order, !order.Finished);
                                else
                                    result = printer.PrintConsignment(order, !order.Finished);
                            }
                            else
                            {
                                result = printer.PrintOrder(order, !order.Finished);
                            }

                            if (!result)
                                allWent = false;
                            else if (order.Finished)
                                orderInOS.PrintedCopies += 1;
                        }
                    }

                    if (!allWent)
                        return "Error printing transactions";
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
        private async Task SearchMenu()
        {
            var options = new[] { "Client Name", "Invoice Number" };
            var selected = await _dialogService.ShowSelectionAsync("Search By", options);
            if (selected >= 0)
            {
                _searchBy = (SearchBy)selected;
                if (!string.IsNullOrEmpty(_searchCriteria))
                    FilterOrderStatus();
            }
        }

        public void ToggleOrderSelection(OrdersInOS order)
        {
            var existing = SelectedOrders.FirstOrDefault(x => x.OrderId == order.OrderId);
            if (existing != null)
            {
                SelectedOrders.Remove(existing);
            }
            else
            {
                SelectedOrders.Add(order);
            }

            RefreshListHeader();
            RefreshStatusSections();
        }

        public async Task HandleOrderSelectedAsync(OrdersInOS order)
        {
            await Shell.Current.GoToAsync($"vieworderstatusdetails?orderId={order.OrderId}");
        }

        private void RefreshUI()
        {
            FilterOrderStatus();
        }

        private void FilterOrderStatus()
        {
            if (_statusTransactions.Count == 0 || _statusTransactions.Values.Sum(x => x.GetOrders().Count) != OrdersInOS.List.Count)
            {
                _statusTransactions.Clear();
                _transactionCount = 0;

                foreach (var item in OrdersInOS.List)
                {
                    string name = GetTransactionTypeName(item);
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (!_statusTransactions.ContainsKey(name))
                            _statusTransactions.Add(name, new StatusSection());

                        _statusTransactions[name].Add(item);
                    }
                }

                _transactionCount = OrdersInOS.List.Count;
            }

            Dictionary<string, StatusSection> toShow = new();

            switch (_whatIsVisible)
            {
                case SelectedOption.All:
                    toShow = _statusTransactions;
                    break;
                case SelectedOption.Sales_Order:
                    if (_statusTransactions.ContainsKey("Sales Order"))
                        toShow.Add("Sales Order", _statusTransactions["Sales Order"]);
                    break;
                case SelectedOption.Credit_Order:
                    if (_statusTransactions.ContainsKey("Credit Order"))
                        toShow.Add("Credit Order", _statusTransactions["Credit Order"]);
                    break;
                case SelectedOption.Return_Order:
                    if (_statusTransactions.ContainsKey("Return Order"))
                        toShow.Add("Return Order", _statusTransactions["Return Order"]);
                    break;
                case SelectedOption.Quote:
                    if (_statusTransactions.ContainsKey("Quote"))
                        toShow.Add("Quote", _statusTransactions["Quote"]);
                    break;
                case SelectedOption.Sales_Invoice:
                    if (_statusTransactions.ContainsKey("Sales Invoice"))
                        toShow.Add("Sales Invoice", _statusTransactions["Sales Invoice"]);
                    break;
                case SelectedOption.Credit_Invoice:
                    if (_statusTransactions.ContainsKey("Credit Invoice"))
                        toShow.Add("Credit Invoice", _statusTransactions["Credit Invoice"]);
                    break;
                case SelectedOption.Return_Invoice:
                    if (_statusTransactions.ContainsKey("Return Invoice"))
                        toShow.Add("Return Invoice", _statusTransactions["Return Invoice"]);
                    break;
                case SelectedOption.Consignment_Invoice:
                    if (_statusTransactions.ContainsKey("Consignment"))
                        toShow.Add("Consignment", _statusTransactions["Consignment"]);
                    break;
                case SelectedOption.ParLevel_Invoice:
                    if (_statusTransactions.ContainsKey("Par Level Invoice"))
                        toShow.Add("Par Level Invoice", _statusTransactions["Par Level Invoice"]);
                    break;
                case SelectedOption.No_Service:
                    if (_statusTransactions.ContainsKey("No Service"))
                        toShow.Add("No Service", _statusTransactions["No Service"]);
                    break;
            }

            foreach (var item in toShow.ToList())
            {
                item.Value.Filter(_searchCriteria, _searchBy);
                if (item.Value.Count == 0)
                    toShow.Remove(item.Key);
            }

            RefreshStatusSections(toShow);
        }

        private void RefreshStatusSections(Dictionary<string, StatusSection>? toShow = null)
        {
            if (toShow == null)
            {
                // Rebuild from current filter
                FilterOrderStatus();
                return;
            }

            StatusSections.Clear();

            foreach (var section in toShow)
            {
                var sectionVm = new StatusSectionViewModel
                {
                    SectionName = section.Key,
                    Total = section.Value.Total,
                    ShowTotal = !Config.HideTransactionsTotal && !Config.HidePriceInTransaction && section.Key != "No Service"
                };

                // Add client groups
                foreach (var clientGroup in section.Value.ToShow)
                {
                    var clientGroupVm = new OrderStatusGroupViewModel
                    {
                        ClientName = clientGroup.Key.ClientName
                    };

                    // Add orders for this client
                    foreach (var order in clientGroup.Value)
                    {
                        var orderVm = new OrderStatusItemViewModel(order, this)
                        {
                            IsSelected = SelectedOrders.Any(x => x.OrderId == order.OrderId)
                        };
                        clientGroupVm.Orders.Add(orderVm);
                    }

                    sectionVm.ClientGroups.Add(clientGroupVm);
                }

                StatusSections.Add(sectionVm);
            }
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
                TotalText = $"Total: {SelectedOrders.Sum(x => x.OrderTotalCost).ToCustomString()}";
            }
            else
            {
                SelectAllText = "Select All";
                TotalText = string.Empty;
            }
        }

        private string GetTransactionTypeName(OrdersInOS item)
        {
            string name = string.Empty;

            if (item.AsPresale)
            {
                if (item.OrderType == OrderType.Order)
                {
                    if (item.OrderType == OrderType.Quote)
                        name = "Quote";
                    else
                        name = "Sales Order";
                }
                else if (item.OrderType == OrderType.Credit)
                    name = "Credit Order";
                else if (item.OrderType == OrderType.Return)
                    name = "Return Order";
                else if (item.OrderType == OrderType.NoService)
                    name = "No Service";
            }
            else
            {
                if (item.OrderType == OrderType.Order)
                {
                    name = "Sales Invoice";
                }
                else if (item.OrderType == OrderType.Credit)
                    name = "Credit Invoice";
                else if (item.OrderType == OrderType.Return)
                    name = "Return Invoice";
                else if (item.OrderType == OrderType.Consignment)
                    name = "Consignment";
                else if (item.OrderType == OrderType.Quote)
                    name = "Quote";
            }

            return name;
        }

        private List<OrdersInOS> GetOrdersForCurrentFilter()
        {
            var orders = new List<OrdersInOS>();

            switch (_whatIsVisible)
            {
                case SelectedOption.All:
                    orders = OrdersInOS.List.ToList();
                    break;
                case SelectedOption.Sales_Order:
                    if (_statusTransactions.ContainsKey("Sales Order"))
                        orders.AddRange(_statusTransactions["Sales Order"].GetOrders());
                    break;
                case SelectedOption.Credit_Order:
                    if (_statusTransactions.ContainsKey("Credit Order"))
                        orders.AddRange(_statusTransactions["Credit Order"].GetOrders());
                    break;
                case SelectedOption.Return_Order:
                    if (_statusTransactions.ContainsKey("Return Order"))
                        orders.AddRange(_statusTransactions["Return Order"].GetOrders());
                    break;
                case SelectedOption.Quote:
                    if (_statusTransactions.ContainsKey("Quote"))
                        orders.AddRange(_statusTransactions["Quote"].GetOrders());
                    break;
                case SelectedOption.Sales_Invoice:
                    if (_statusTransactions.ContainsKey("Sales Invoice"))
                        orders.AddRange(_statusTransactions["Sales Invoice"].GetOrders());
                    break;
                case SelectedOption.Credit_Invoice:
                    if (_statusTransactions.ContainsKey("Credit Invoice"))
                        orders.AddRange(_statusTransactions["Credit Invoice"].GetOrders());
                    break;
                case SelectedOption.Return_Invoice:
                    if (_statusTransactions.ContainsKey("Return Invoice"))
                        orders.AddRange(_statusTransactions["Return Invoice"].GetOrders());
                    break;
                case SelectedOption.Consignment_Invoice:
                    if (_statusTransactions.ContainsKey("Consignment"))
                        orders.AddRange(_statusTransactions["Consignment"].GetOrders());
                    break;
                case SelectedOption.ParLevel_Invoice:
                    if (_statusTransactions.ContainsKey("Par Level Invoice"))
                        orders.AddRange(_statusTransactions["Par Level Invoice"].GetOrders());
                    break;
                case SelectedOption.No_Service:
                    if (_statusTransactions.ContainsKey("No Service"))
                        orders.AddRange(_statusTransactions["No Service"].GetOrders());
                    break;
                default:
                    orders = OrdersInOS.List.ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(_searchCriteria))
            {
                if (_searchBy == SearchBy.ClientName)
                    orders = orders.Where(x => x.Client?.ClientName?.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant()) == true).ToList();
                else
                    orders = orders.Where(x => !string.IsNullOrEmpty(x.PrintedOrderId) && x.PrintedOrderId.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
            }

            return orders;
        }
    }

    public partial class StatusSectionViewModel : ObservableObject
    {
        [ObservableProperty] private string _sectionName = string.Empty;
        [ObservableProperty] private double _total;
        [ObservableProperty] private bool _showTotal;

        public ObservableCollection<OrderStatusGroupViewModel> ClientGroups { get; } = new();
    }

    public partial class OrderStatusGroupViewModel : ObservableObject
    {
        [ObservableProperty] private string _clientName = string.Empty;

        public ObservableCollection<OrderStatusItemViewModel> Orders { get; } = new();
    }

    public partial class OrderStatusItemViewModel : ObservableObject
    {
        private readonly OrdersInOS _order;
        private readonly ViewOrderStatusPageViewModel _parent;

        [ObservableProperty] private bool _isSelected;

        public OrderStatusItemViewModel(OrdersInOS order, ViewOrderStatusPageViewModel parent)
        {
            _order = order;
            _parent = parent;
        }

        public string OrderNumberText => !string.IsNullOrEmpty(_order.PrintedOrderId) ? _order.PrintedOrderId : $"Order #{_order.OrderId}";
        public string DateText => $"Date: {_order.Date.ToShortDateString()}";
        public string TotalText => _order.OrderType == OrderType.NoService ? string.Empty : $"Total: {_order.OrderTotalCost.ToCustomString()}";
        public bool ShowTotal => !Config.HidePriceInTransaction && _order.OrderType != OrderType.NoService;

        public string StatusText
        {
            get
            {
                var statusEx = DataAccess.GetSingleUDF("status", _order.ExtraFields);
                if (!string.IsNullOrEmpty(statusEx))
                    return $"Status: {statusEx}";
                return $"Status: {_order.OrderStatus.ToString().Replace("_", " ")}";
            }
        }

        public Color StatusColor
        {
            get
            {
                if (_order.Reshipped)
                    return Colors.Purple;

                switch ((int)_order.OrderStatus)
                {
                    case 1:
                        return Color.FromArgb("#0E86D4");
                    case 2:
                        return Colors.Brown;
                    case 6:
                        return Colors.Green;
                    case 8:
                        return Colors.Blue;
                    case 9:
                    case 10:
                        return Colors.DarkRed;
                    default:
                        return Colors.Black;
                }
            }
        }

        public bool ShowStatus => Config.ShowOrderStatus;

        public string SalesmanText
        {
            get
            {
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == _order.OriginalSalesmanId);
                if (salesman != null)
                    return $"Salesman: {salesman.Name}";
                return string.Empty;
            }
        }

        public bool ShowSalesman => !string.IsNullOrEmpty(SalesmanText);

        partial void OnIsSelectedChanged(bool value)
        {
            _parent.ToggleOrderSelection(_order);
        }

        [RelayCommand]
        private async Task ViewDetails()
        {
            await _parent.HandleOrderSelectedAsync(_order);
        }

        public OrdersInOS Order => _order;
    }

    internal class StatusSection
    {
        private Dictionary<Client, List<OrdersInOS>> _section = new();
        public Dictionary<Client, List<OrdersInOS>> ToShow = new();
        private double _total = 0;

        public void Filter(string searchCriteria, ViewOrderStatusPageViewModel.SearchBy searchBy)
        {
            ToShow = _section;

            if (!string.IsNullOrEmpty(searchCriteria))
            {
                if (searchBy == ViewOrderStatusPageViewModel.SearchBy.ClientName)
                    ToShow = new Dictionary<Client, List<OrdersInOS>>(ToShow.Where(x => x.Key.ClientName.ToLowerInvariant().Contains(searchCriteria.ToLowerInvariant())));
                else
                    ToShow = new Dictionary<Client, List<OrdersInOS>>(ToShow.Where(x =>
                        x.Value.Any(y => !string.IsNullOrEmpty(y.PrintedOrderId) && y.PrintedOrderId.ToLowerInvariant().Contains(searchCriteria.ToLowerInvariant()))));
            }

            _total = CalculateTotal();
        }

        public void Add(OrdersInOS order)
        {
            var key = _section.Keys.FirstOrDefault(x => x.ClientId == order.Client.ClientId);
            if (key == null)
            {
                _section.Add(order.Client, new List<OrdersInOS>());
                key = order.Client;
            }
            _section[key].Add(order);
        }

        public int Count => ToShow.Count;
        public double Total => _total;

        private double CalculateTotal()
        {
            double total = 0;
            foreach (var item in ToShow.Values)
                foreach (var order in item)
                    total += order.OrderTotalCost;
            return total;
        }

        public List<OrdersInOS> GetOrders()
        {
            var result = new List<OrdersInOS>();
            foreach (var item in ToShow.Values)
                result.AddRange(item);
            return result;
        }
    }
}
