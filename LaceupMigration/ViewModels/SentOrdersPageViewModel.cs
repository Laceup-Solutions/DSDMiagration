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

        public List<SentOrder> SelectedOrders = new List<SentOrder>();

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
            if (!DataProvider.CanUseApplication() || !Config.ReceivedData)
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
                var processedOrderIds = new HashSet<string>();
                var selectedDateOnly = SelectedDate.Date;
                // Packages(SelectedDate) returns all packages with CreatedDate >= SelectedDate, so we filter by order date to show only orders for the selected day
                if (packages.Count > 0)
                {
                    foreach (var pck in packages)
                    {
                        var packageOrders = pck.PackageOrders();
                        foreach (var sentOrder in packageOrders)
                        {
                            if (sentOrder.Date.Date < selectedDateOnly)
                                continue;
                            var orderId = sentOrder.OrderUniqueId ?? $"{sentOrder.OrderId}_{sentOrder.Date.Ticks}";
                            if (processedOrderIds.Add(orderId))
                            {
                                sentOrder.CellType = CellType.Header;
                                sentOrder.PackagePath = pck.PackagePath;
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
            _optionSelected = MapTransactionTypeToOption(value);
            RefreshListView();
        }

        /// <summary>Maps the display string from the picker to SelectedOption. Do not use list index - options are built dynamically and indices don't match the enum.</summary>
        private static SelectedOption MapTransactionTypeToOption(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return SelectedOption.All;
            return value.Trim() switch
            {
                "All" => SelectedOption.All,
                "No Service" => SelectedOption.No_Service,
                "Sales Order" => SelectedOption.Sales_Order,
                "Credit Order" => SelectedOption.Credit_Order,
                "Return Order" => SelectedOption.Return_Order,
                "Quote" => SelectedOption.Quote,
                "Sales Invoice" => SelectedOption.Sales_Invoice,
                "Credit Invoice" => SelectedOption.Credit_Invoice,
                "Return Invoice" => SelectedOption.Return_Invoice,
                "Consignment" => SelectedOption.Consignment_Invoice,
                "Par Level Invoice" => SelectedOption.ParLevel_Invoice,
                _ => SelectedOption.All
            };
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
        private async Task SelectTransactionType()
        {
            var options = TransactionTypeOptions.ToArray();
            var choice = await _dialogService.ShowActionSheetAsync("Select Transaction Type", "", "Cancel", options);

            if (!string.IsNullOrEmpty(choice) && choice != "Cancel" && TransactionTypeOptions.Contains(choice))
            {
                SelectedTransactionType = choice;
            }
        }

        private bool isUpdatingSelectAll = false;

        [RelayCommand]
        private void SelectAll()
        {
            isUpdatingSelectAll = true;
            
            SelectedOrders.Clear();

            if (!IsSelectAllChecked)
            {
                foreach (var item in SentOrders)
                    item.IsChecked = false;
                RefreshListHeader();
            }
            else
            {
                SelectedOrders = SentOrders.Select(x => x.Order).ToList();
                RefreshListHeader();

                foreach (var item in SentOrders)
                    item.IsChecked = true;
            }

            isUpdatingSelectAll = false;
        }

        [RelayCommand]
        private async Task Print()
        {
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to print.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int copies) =>
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    foreach (var sentOrder in SelectedOrders)
                    {
                        if (string.IsNullOrEmpty(sentOrder.PackagePath) || !System.IO.File.Exists(sentOrder.PackagePath))
                            continue;

                        var order = SentOrder.CreateTemporalOrderFromFile(sentOrder.PackagePath, sentOrder);
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
                        }
                    }

                    if (!allWent)
                        return "Error printing orders";
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
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Convert SentOrder to Order for email sending
                List<Order> tosendOrders = new List<Order>();
                foreach (var sentOrder in SelectedOrders)
                {
                    tosendOrders.Add(SentOrder.CreateTemporalOrderFromFile(sentOrder.PackagePath, sentOrder));
                }

                if (tosendOrders.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Could not find orders to send.", "Alert", "OK");
                    return;
                }

                // Use PdfHelper to send orders by email (matches Xamarin SentOrdersPackagesActivity)
                await PdfHelper.SendOrdersByEmail(tosendOrders);
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
            if (SelectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Select at least one order to resend.", "Alert", "OK");
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
                        foreach (var o in SelectedOrders)
                        {
                            if (!string.IsNullOrEmpty(o.PackagePath) && !packageLocations.Contains(o.PackagePath))
                                packageLocations.Add(o.PackagePath);
                        }

                        foreach (var package in packageLocations)
                        {
                            var dstFile = package;
                            if (string.IsNullOrEmpty(dstFile) || !System.IO.File.Exists(dstFile))
                                continue;

                            string dstFileZipped = dstFile + ".zip";
                            ZipMethods.ZipFile(dstFile, dstFileZipped);
                            DataProvider.SendTheOrders(dstFileZipped);
                            System.IO.File.Delete(dstFileZipped);

                            var signatureFileZipped = dstFile + ".signature.zip";
                            if (System.IO.File.Exists(signatureFileZipped))
                            {
                                DataProvider.SendTheSignatures(signatureFileZipped);
                            }
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
                    await _dialogService.ShowAlertAsync("Orders sent.", "Info", "OK");
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

        public void ToggleOrderSelection(SentOrder order)
        {
            if(isUpdatingSelectAll)
                return;
            
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
            var uniqueOrderIds = new HashSet<string>();

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
                var orderId = item.Order.OrderUniqueId ?? $"{item.Order.OrderId}_{item.Order.Date.Ticks}";
                if (uniqueOrderIds.Add(orderId))
                    SentOrders.Add(item);
            }
            
            SelectAll();

            RefreshListHeader();
        }

        private void RefreshListHeader()
        {
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
        public bool ShowOrderNumber => !string.IsNullOrEmpty(_order.PrintedOrderId);
        public string DateClientText => $"{_order.Date:yyyy/MM/dd hh:mm tt} {_order.ClientName}";
        public bool ShowTotal => !Config.HidePriceInTransaction && _order.OrderType != OrderType.NoService;

        partial void OnIsCheckedChanged(bool value)
        {
            _parent.ToggleOrderSelection(_order);
        }

        [RelayCommand]
        private async Task ViewDetails()
        {
            string? packagePath = _order.PackagePath;
            
            if (string.IsNullOrEmpty(packagePath))
            {
                var packages = SentOrderPackage.Packages(_parent.SelectedDate);
                foreach (var pck in packages)
                {
                    var packageOrders = pck.PackageOrders().ToList();
                    if (packageOrders.Any(x => x.OrderUniqueId == _order.OrderUniqueId))
                    {
                        packagePath = pck.PackagePath;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(packagePath))
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
