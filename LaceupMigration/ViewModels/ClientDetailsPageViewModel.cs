using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ClientDetailsPageViewModel : ObservableObject
    {
        private const int InvoicePageSize = 20;

        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;

        private Client? _client;
        private readonly List<ClientInvoiceViewModel> _allInvoices = new();
        private readonly List<ClientInvoiceViewModel> _displayedInvoices = new();
        private bool _hasMoreInvoices;
        private int _currentInvoiceIndex;
        private bool _initialized;

        public ObservableCollection<ClientDetailItemViewModel> ClientDetails { get; } = new();
        public ObservableCollection<ClientOrderViewModel> Orders { get; } = new();
        public ObservableCollection<InvoiceGroup> FilteredInvoices { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private bool _showClockInButton;

        [ObservableProperty]
        private string _clockInButtonText = "Start Visit";

        [ObservableProperty]
        private bool _showGoal;

        [ObservableProperty]
        private string _goalText = string.Empty;

        [ObservableProperty]
        private bool _showOrdersSection;

        [ObservableProperty]
        private bool _isInvoiceLoading;

        [ObservableProperty]
        private bool _showNoInvoices;

        [ObservableProperty]
        private bool _showOverCreditLimit;

        [ObservableProperty]
        private string _overCreditLimitText = "Customer is over credit limit";

        [ObservableProperty]
        private string _invoiceSearchQuery = string.Empty;

        partial void OnInvoiceSearchQueryChanged(string value)
        {
            ApplyInvoiceFilter();
        }

        public ClientDetailsPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
        }

        public async Task InitializeAsync(int clientId)
        {
            if (_client != null && _client.ClientId == clientId && _initialized)
            {
                await RefreshAsync();
                return;
            }

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Unable to locate the selected client.", "Error");
                return;
            }

            _client.EnsureInvoicesAreLoaded();

            _initialized = true;
            ClientName = _client.ClientName;
            ShowOverCreditLimit = _client.OverCreditLimit;
            OverCreditLimitText = "Customer is over the credit limit";
            ShowClockInButton = Config.TimeSheetCustomization;
            UpdateClockInText();

            BuildClientDetails();
            BuildGoal();
            BuildOrders();
            await BuildInvoicesAsync();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized || _client == null)
                return;

            BuildOrders();
            ApplyInvoiceFilter();
            UpdateClockInText();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the current client ID for state saving purposes.
        /// </summary>
        public int GetClientId()
        {
            return _client?.ClientId ?? 0;
        }

        private async Task RefreshAsync()
        {
            BuildClientDetails();
            BuildGoal();
            BuildOrders();
            await BuildInvoicesAsync();
        }

        private void BuildClientDetails()
        {
            ClientDetails.Clear();
            if (_client == null)
                return;

            // if (!Config.TimeSheetCustomization)
            // {
            //     ClientDetails.Add(new ClientDetailItemViewModel(_client.ClientName));
            // }

            if (!string.IsNullOrWhiteSpace(_client.ContactName))
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Contact: {_client.ContactName}"));
            }

            if (!string.IsNullOrWhiteSpace(_client.ShipToAddress))
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Address: {_client.ShipToAddress.Replace("|", " ")}"));
            }

            if (!string.IsNullOrWhiteSpace(_client.ContactPhone))
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Phone: {_client.ContactPhone}"));
            }

            if (!Config.HidePriceInTransaction && !Config.HideInvoicesAndBalance)
            {
                var invoicesFinalized = Order.Orders.Where(x => x.Client == _client && x.Finished).ToList();
                var total = invoicesFinalized.Sum(x => x.OrderTotalCost());
                var clientBalance = _client.OpenBalance + total;
                var currentPayments = InvoicePayment.List.Where(x => x.Client.ClientId == _client.ClientId).ToList();
                var currentPaymentsTotal = currentPayments.Sum(x => x.TotalPaid);
                clientBalance -= currentPaymentsTotal;
                ClientDetails.Add(new ClientDetailItemViewModel($"Balance: {clientBalance.ToCustomString()}"));
            }

            var notes = !string.IsNullOrWhiteSpace(_client.Notes) ? _client.Notes : _client.Comment;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Notes: {notes}"));
            }

            var newestInvoice = _client.Invoices?.OrderByDescending(x => x.Date).FirstOrDefault();
            if (newestInvoice != null)
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Last Visit: {newestInvoice.Date.ToShortDateString()}"));
                var difference = (DateTime.Now.Date - newestInvoice.Date).TotalDays;
                ClientDetails.Add(new ClientDetailItemViewModel($"Days since last visit: {(int)difference}"));
            }

            var nextRoute = RouteEx.Routes
                .Where(x => x.Client != null && x.Client.ClientId == _client.ClientId && x.Date.Date >= DateTime.Now.Date)
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            if (nextRoute != null)
            {
                ClientDetails.Add(new ClientDetailItemViewModel($"Next visit date: {nextRoute.Date.ToShortDateString()}"));
            }

            if (_client.StartTimeWindows1 != DateTime.MinValue)
            {
                ClientDetails.Add(new ClientDetailItemViewModel(string.Format(CultureInfo.CurrentCulture,
                    "Window: {0} - {1}", _client.StartTimeWindows1.ToShortTimeString(), _client.EndTimeWindows1.ToShortTimeString())));
            }

            if (_client.ExtraProperties != null)
            {
                foreach (var tuple in _client.ExtraProperties)
                {
                    if (tuple.Item1.Equals("prodsort", StringComparison.InvariantCultureIgnoreCase) ||
                        tuple.Item1.Equals("duns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    ClientDetails.Add(new ClientDetailItemViewModel($"{tuple.Item1}: {tuple.Item2}"));
                }
            }
        }

        private void BuildGoal()
        {
            ShowGoal = false;
            GoalText = string.Empty;

            if (_client == null || !Config.ViewGoals || GoalDetailDTO.List.Count == 0)
                return;

            var detail = GoalDetailDTO.List.Where(x =>
                    x.Goal != null &&
                    x.ClientId == _client.ClientId &&
                    x.Goal.PendingDays > 0 &&
                    x.Goal.Criteria == GoalCriteria.Payment)
                .ToList();

            if (detail.Count > 0)
            {
                var amount = detail.Sum(x => x.QuantityOrAmountValue);
                GoalText = $"Total payment goal: {amount.ToCustomString()}";
                ShowGoal = true;
            }
        }

        private void BuildOrders()
        {
            Orders.Clear();
            if (_client == null)
            {
                ShowOrdersSection = false;
                return;
            }

            var batches = Batch.List
                .Where(x =>
                    (x.Client != null && x.Client.ClientId == _client.ClientId) ||
                    x.Orders().FirstOrDefault(y => y.Client != null && y.Client.ClientId == _client.ClientId) != null)
                .ToList();

            foreach (var batch in batches)
            {
                foreach (var order in batch.Orders())
                {
                    var viewModel = CreateOrderViewModel(order, batch);
                    Orders.Add(viewModel);
                }
            }

            ShowOrdersSection = Orders.Count > 0;
        }

        private ClientOrderViewModel CreateOrderViewModel(Order order, Batch batch)
        {
            var title = GetOrderTitle(order);
            var subtitle = $"Total: {order.OrderTotalCost().ToCustomString()}";
            var status = GetOrderStatus(order, out var color);
            return new ClientOrderViewModel(title, subtitle, status, color, order, batch);
        }

        private string GetOrderTitle(Order order)
        {
            var number = string.IsNullOrEmpty(order.PrintedOrderId) ? string.Empty : $" #: {order.PrintedOrderId}";
            switch (order.OrderType)
            {
                case OrderType.Order:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                    {
                        return order.IsQuote ? $"Quote{number}" : $"Sales order{number}";
                    }
                    return $"Sales invoice{number}";
                case OrderType.Consignment:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Sales order{number}";
                    return $"Sales invoice{number}";
                case OrderType.Credit:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Credit order{number}";
                    return $"Credit invoice{number}";
                case OrderType.Return:
                    if (order.AsPresale || (order.IsDelivery && !order.Finished))
                        return $"Return order{number}";
                    return $"Return invoice{number}";
                case OrderType.NoService:
                    return "No service";
                default:
                    return $"Invoice{number}";
            }
        }

        private string GetOrderStatus(Order order, out Color statusColor)
        {
            statusColor = Colors.Transparent;
            if (order.Reshipped)
            {
                statusColor = Colors.Purple;
                return "Reshipped";
            }

            if (order.Voided)
            {
                statusColor = Colors.Red;
                return "Voided";
            }

            if (order.Finished)
            {
                statusColor = Colors.Green;
                return "Finalized";
            }

            statusColor = Colors.Transparent;
            return string.Empty;
        }

        private async Task BuildInvoicesAsync()
        {
            _allInvoices.Clear();
            _displayedInvoices.Clear();
            FilteredInvoices.Clear();
            _currentInvoiceIndex = 0;
            _hasMoreInvoices = false;
            ShowNoInvoices = false;

            if (_client == null || _client.Invoices == null)
            {
                ShowNoInvoices = true;
                return;
            }

            var sortedInvoices = _client.Invoices.OrderByDescending(x => x.Date).ToList();
            foreach (var invoice in sortedInvoices)
            {
                _allInvoices.Add(CreateInvoiceViewModel(invoice, _client));
            }

            _hasMoreInvoices = _allInvoices.Count > 0;
            await LoadInvoicesPageAsync();
        }

        private ClientInvoiceViewModel CreateInvoiceViewModel(Invoice invoice, Client client)
        {
            var numberText = GetInvoiceTitle(invoice);
            var createdText = $"Created: {invoice.Date.ToShortDateString()}";
            var dueText = $"Due: {invoice.DueDate.ToShortDateString()}";
            var totalText = $"Total: {invoice.Amount.ToCustomString()}";
            var openAmount = invoice.Balance - invoice.Paid;
            var openText = $"Open: {openAmount.ToCustomString()}";
            var showAmounts = !Config.HidePriceInTransaction;

            var isOverdue = invoice.DueDate < DateTime.Today && invoice.Balance > 0;
            var dueTextColor = isOverdue ? Colors.Red : Colors.Black;
            var openTextColor = isOverdue ? Colors.Red : Colors.Black;

            var status = DataAccess.GetSingleUDF("status", invoice.ExtraFields) ?? string.Empty;

            var hasDiscount = false;
            var discountText = string.Empty;

            if (Config.UsePaymentDiscount && invoice.InvoiceType != 1 && client.TermId > 0)
            {
                var term = Term.List.Where(x => x.IsActive).FirstOrDefault(x => x.Id == client.TermId);
                if (term != null)
                {
                    var daysRemaining = Math.Abs((invoice.Date - DateTime.Now.Date).TotalDays);
                    if (term.StandardDiscountDays >= daysRemaining && term.DiscountPercentage > 0)
                    {
                        var discountAmount = invoice.Balance * (term.DiscountPercentage / 100);
                        discountText = $"Eligible for {term.DiscountPercentage}% discount ({discountAmount.ToCustomString()})";
                        hasDiscount = true;
                    }
                }
            }

            var showPastDue = false;
            if (Config.CannotOrderWithUnpaidInvoices && invoice.Balance > 0 && invoice.DueDate.AddDays(90) < DateTime.Now.Date)
            {
                var payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) &&
                    x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null);
                if (payment == null)
                {
                    showPastDue = true;
                }
                else
                {
                    var paid = payment.Components.Sum(x => x.Amount);
                    showPastDue = paid < invoice.Balance;
                }
            }

            var pastDueText = showPastDue ? "Past due over 90 days" : string.Empty;

            var showPaymentGoal = false;
            var goalPaymentText = string.Empty;
            var pendingDaysText = string.Empty;

            if (Config.ViewGoals && GoalDetailDTO.List.Count > 0 && Math.Round(invoice.Balance, 2) != 0)
            {
                var goalDetail = GoalDetailDTO.List.FirstOrDefault(x =>
                    x.ExternalInvoice != null &&
                    x.ExternalInvoice.InvoiceId == invoice.InvoiceId &&
                    x.Goal != null &&
                    x.Goal.Criteria == GoalCriteria.Payment &&
                    x.Goal.Status != GoalProgressDTO.GoalStatus.Expired);

                if (goalDetail != null)
                {
                    showPaymentGoal = true;
                    goalPaymentText = $"Payment goal: {goalDetail.QuantityOrAmountValue.ToCustomString()}";
                    pendingDaysText = $"Pending days: {goalDetail.WorkingDays - goalDetail.WorkedDays}";
                }
            }

            return new ClientInvoiceViewModel(
                number: numberText,
                invoiceNumber: invoice.InvoiceNumber,
                createdText: createdText,
                dueText: dueText,
                totalText: totalText,
                openText: openText,
                statusText: status,
                hasStatus: !string.IsNullOrEmpty(status),
                showAmounts: showAmounts,
                dueColor: dueTextColor,
                openColor: openTextColor,
                hasDiscount: hasDiscount,
                discountText: discountText,
                showPastDue: showPastDue,
                pastDueText: pastDueText,
                showPaymentGoal: showPaymentGoal,
                goalPaymentText: goalPaymentText,
                pendingDaysText: pendingDaysText,
                invoice: invoice);
        }

        private string GetInvoiceTitle(Invoice invoice)
        {
            return invoice.InvoiceType switch
            {
                1 => $"Credit: {invoice.InvoiceNumber}",
                2 => $"Quote: {invoice.InvoiceNumber}",
                3 => $"Sales order: {invoice.InvoiceNumber}",
                4 => $"Credit invoice: {invoice.InvoiceNumber}",
                _ => $"Invoice: {invoice.InvoiceNumber}"
            };
        }

        private async Task LoadInvoicesPageAsync()
        {
            if (!_hasMoreInvoices || IsInvoiceLoading)
            {
                ApplyInvoiceFilter();
                return;
            }

            try
            {
                IsInvoiceLoading = true;
                var nextItems = _allInvoices.Skip(_currentInvoiceIndex).Take(InvoicePageSize).ToList();
                if (nextItems.Count == 0)
                {
                    _hasMoreInvoices = false;
                    ApplyInvoiceFilter();
                    return;
                }

                _currentInvoiceIndex += nextItems.Count;
                if (_currentInvoiceIndex >= _allInvoices.Count)
                {
                    _hasMoreInvoices = false;
                }

                _displayedInvoices.AddRange(nextItems);

                // Always regroup after adding new items (matches Xamarin grouping behavior)
                ApplyInvoiceFilter();
            }
            finally
            {
                IsInvoiceLoading = false;
            }
        }

        // [MIGRATION]: ApplyInvoiceFilter now groups invoices by CompanyName, matching Xamarin ClientInvoicesAdapter
        // Groups invoices exactly like Xamarin: invoices.GroupBy(invoice => invoice.CompanyName)
        // Invoices without CompanyName appear as individual items at the top (no group header)
        private void ApplyInvoiceFilter()
        {
            var query = InvoiceSearchQuery?.Trim() ?? string.Empty;
            List<ClientInvoiceViewModel> invoicesToGroup;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var comparer = StringComparison.InvariantCultureIgnoreCase;
                invoicesToGroup = _allInvoices
                    .Where(x => x.InvoiceNumber?.Contains(query, comparer) == true)
                    .ToList();
            }
            else
            {
                invoicesToGroup = _displayedInvoices.ToList();
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredInvoices.Clear();

                // [MIGRATION]: Separate invoices with and without CompanyName (matches Xamarin behavior)
                // Invoices without CompanyName appear as individual items at the top, with NO group header
                var invoicesWithoutCompany = invoicesToGroup
                    .Where(invoice => string.IsNullOrEmpty(invoice.CompanyName))
                    .ToList();

                var invoicesWithCompany = invoicesToGroup
                    .Where(invoice => !string.IsNullOrEmpty(invoice.CompanyName))
                    .ToList();

                // Add invoices without CompanyName as individual groups with empty header (header will be hidden in XAML)
                // These appear at the top of the list, matching Xamarin behavior
                foreach (var invoice in invoicesWithoutCompany)
                {
                    var singleItemGroup = new InvoiceGroup(string.Empty, new[] { invoice });
                    FilteredInvoices.Add(singleItemGroup);
                }

                // Group invoices with CompanyName by CompanyName (matches Xamarin ClientInvoicesAdapter grouping)
                var grouped = invoicesWithCompany
                    .GroupBy(invoice => invoice.CompanyName ?? string.Empty)
                    .Select(group => new InvoiceGroup(
                        header: group.Key, // CompanyName as the header
                        items: group.ToList()))
                    .ToList();

                foreach (var group in grouped)
                {
                    FilteredInvoices.Add(group);
                }

                ShowNoInvoices = FilteredInvoices.Count == 0;
            });
        }

        private void UpdateClockInText()
        {
            if (!Config.TimeSheetCustomization || _client == null)
            {
                ClockInButtonText = "Start Visit";
                return;
            }

            var sessionDetails = Session.sessionDetails;
            if (sessionDetails == null)
            {
                ClockInButtonText = "Start Visit";
                return;
            }

            var isClockedIn = sessionDetails.FirstOrDefault(x =>
                x.clientId == _client.ClientId &&
                string.IsNullOrEmpty(x.orderUniqueId) &&
                x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
                x.endTime == DateTime.MinValue) != null;

            ClockInButtonText = isClockedIn ? "End Visit" : "Start Visit";
        }

        [RelayCommand]
        private async Task ToggleClockInAsync()
        {
            if (_client == null)
                return;

            if (Session.session == null)
            {
                var result = await _dialogService.ShowConfirmAsync(
                    "You need to clock in to start a visit in a client, do you want to do it now?",
                    "Alert",
                    "Yes",
                    "No");

                if (result)
                {
                    await Shell.Current.GoToAsync("timesheet");
                }
                return;
            }

            var sessionDetails = Session.sessionDetails;
            if (sessionDetails == null)
                return;

            var detail = sessionDetails.FirstOrDefault(x =>
                x.clientId == _client.ClientId &&
                string.IsNullOrEmpty(x.orderUniqueId) &&
                x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
                x.endTime == DateTime.MinValue);

            if (detail != null)
            {
                // End visit
                detail.endLatitude = DataAccess.LastLatitude;
                detail.endLongitude = DataAccess.LastLongitude;
                detail.endTime = DateTime.Now;
                Session.session.Save();
                UpdateClockInText();
            }
            else
            {
                // Check for other open clock-ins
                if (Session.session.OtherOpenClockin(_client.ClientId))
                {
                    await _dialogService.ShowAlertAsync("You already have an open visit for another client.", "Warning");
                    return;
                }

                // Start visit
                detail = new SessionDetails(_client.ClientId, SessionDetails.SessionDetailType.CustomerVisit)
                {
                    startTime = DateTime.Now,
                    startLatitude = DataAccess.LastLatitude,
                    startLongitude = DataAccess.LastLongitude,
                };

                Session.session.AddDetail(detail);
                Session.session.Save();
                UpdateClockInText();
            }
        }

        [RelayCommand]
        private async Task LoadMoreInvoicesAsync()
        {
            await LoadInvoicesPageAsync();
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_client == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private bool HasActiveDeliveries()
        {
            if (_client == null)
                return false;

            foreach (var route in RouteEx.Routes)
            {
                if (route.Order != null)
                {
                    if (route.Order.Client != null && route.Order.Client.ClientId == _client.ClientId && !route.Order.Finished)
                    {
                        return true;
                    }
                    if (route.Order.BatchId > 0 && !route.Order.Finished)
                    {
                        var batch = Batch.List.FirstOrDefault(x => x.Client != null && x.Client.ClientId == _client.ClientId && x.Id == route.Order.BatchId);
                        if (batch != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private async Task<bool> CanCreateOrderAsync(string? message = null)
        {
            if (_client == null)
                return false;

            // Check due invoices
            if (Config.CheckDueInvoicesInCreateOrder || Config.CheckDueInvoicesQtyInCreateOrder > 0)
            {
                var openInvoices = _client.Invoices?.Where(x => x.Balance > 0 && x.DueDate < DateTime.Today).ToList() ?? new List<Invoice>();
                int count = 0;

                foreach (var invoice in openInvoices)
                {
                    var payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) &&
                        x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null);

                    if (payment == null)
                    {
                        count++;
                        continue;
                    }

                    var paid = payment.Components.Sum(x => x.Amount);
                    if (paid < invoice.Balance)
                    {
                        count++;
                    }
                }

                if (Config.CheckDueInvoicesQtyInCreateOrder == 0 && count > 0)
                {
                    message = "You must collect payment for due invoices before creating an order.";
                    if (!string.IsNullOrEmpty(message))
                        await _dialogService.ShowAlertAsync(message, "Alert");
                    return false;
                }

                if (Config.CheckDueInvoicesQtyInCreateOrder > 0 && count >= Config.CheckDueInvoicesQtyInCreateOrder)
                {
                    message = "You must collect payment for due invoices before creating an order.";
                    if (!string.IsNullOrEmpty(message))
                        await _dialogService.ShowAlertAsync(message, "Alert");
                    return false;
                }
            }

            // Check unpaid invoices over 90 days
            if (Config.CannotOrderWithUnpaidInvoices)
            {
                var unpaidInvoices = _client.Invoices?.Where(x => x.Balance > 0 && x.DueDate.AddDays(90) < DateTime.Now.Date).ToList() ?? new List<Invoice>();
                int count = 0;

                foreach (var invoice in unpaidInvoices)
                {
                    var payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) &&
                        x.Invoices().FirstOrDefault(y => y.InvoiceId == invoice.InvoiceId) != null);

                    if (payment == null)
                    {
                        count++;
                        continue;
                    }

                    var paid = payment.Components.Sum(x => x.Amount);
                    if (paid < invoice.Balance)
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    message = "You cannot create an order until payments are collected for invoices over 90 days past due.";
                    if (!string.IsNullOrEmpty(message))
                        await _dialogService.ShowAlertAsync(message, "Alert");
                    return false;
                }
            }

            // Check location services
            if (Config.UseLocation && Config.LocationIsMandatory)
            {
                // Location check would go here - simplified for now
            }

            // Check timesheet
            if (Config.TimeSheetCustomization)
            {
                if (Session.session == null)
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "You need to clock in to start working, do you want to do it now?",
                        "Alert",
                        "Yes",
                        "No");
                    if (result)
                    {
                        await Shell.Current.GoToAsync("timesheet");
                    }
                    return false;
                }

                if (!Session.session.isClockedInClient(_client.ClientId))
                {
                    await _dialogService.ShowAlertAsync("You need to start visit in this client to create transactions", "Alert");
                    return false;
                }

                if (Session.sessionDetails.Any(x => x.detailType == SessionDetails.SessionDetailType.Break && x.endTime == DateTime.MinValue))
                {
                    await _dialogService.ShowAlertAsync("You cannot create a transaction while you are on break.", "Alert");
                    return false;
                }
            }

            return true;
        }

        private async Task CreateOrderAsync(OrderType orderType, bool isQuote = false)
        {
            if (!await CanCreateOrderAsync())
                return;

            if (_client == null)
                return;

            if (_client.IsOverCreditLimit())
            {
                await _dialogService.ShowAlertAsync("Customer is over credit limit. Cannot create new order.", "Alert");
                return;
            }

            if (_client.OnCreditHold && Config.CustomerInCreditHold)
            {
                await _dialogService.ShowAlertAsync("This client is on Credit Hold, you cannot create an order", "Info");
                return;
            }

            // Check if SalesByDepartment - if so, go directly to BatchDepartmentActivity
            if (Config.SalesByDepartment && orderType == OrderType.Order && !isQuote)
            {
                var batch = Batch.List.FirstOrDefault(x => 
                    x.Client != null && 
                    x.Client.ClientId == _client.ClientId && 
                    !x.Orders().Any(o => o.Finished));

                if (batch == null)
                {
                    batch = new Batch(_client);
                    batch.Client = _client;
                    batch.ClockedIn = DateTime.Now;
                    batch.Save();
                }

                await Shell.Current.GoToAsync($"batchdepartment?clientId={_client.ClientId}&batchId={batch.Id}");
                return;
            }

            // Create batch
            var newBatch = new Batch(_client);
            newBatch.Client = _client;
            newBatch.ClockedIn = DateTime.Now;
            newBatch.Save();

            // Handle department selection if required
            int? departmentId = null;
            if (Config.MustSelectDepartment)
            {
                var departments = DepartmertClientCategory.GetDepartmentToClient(_client).OrderBy(x => x.Name).ToList();
                if (departments.Count > 0)
                {
                    var deptNames = departments.Select(x => x.Name).ToArray();
                    var selectedIndex = await _dialogService.ShowSelectionAsync("Select Department", deptNames);
                    if (selectedIndex >= 0 && selectedIndex < departments.Count)
                    {
                        departmentId = departments[selectedIndex].Id;
                    }
                    else
                    {
                        newBatch.Delete();
                        return;
                    }
                }
            }

            // Create order
            var order = new Order(_client) { OrderType = orderType };
            if (isQuote)
            {
                order.IsQuote = true;
            }
            if (departmentId.HasValue)
            {
                order.DepartmentId = departmentId.Value;
            }

            order.BatchId = newBatch.Id;
            order.AsPresale = true;
            order.SalesmanId = Config.SalesmanId;
            order.Save();

            // Handle salesman selection, company selection, and warehouse selection
            await SelectOrderSalesmanAsync(newBatch, order);
        }

        private async Task CreateWorkOrderAsync()
        {
            if (!await CanCreateOrderAsync())
                return;

            if (_client == null)
                return;

            var batch = new Batch(_client);
            batch.Client = _client;
            batch.ClockedIn = DateTime.Now;
            batch.Save();

            var order = new Order(_client) { OrderType = OrderType.Order };
            order.BatchId = batch.Id;
            order.AsPresale = true;
            order.Save();

            await Shell.Current.GoToAsync($"workorder?clientId={_client.ClientId}&orderId={order.OrderId}&asPresale=1");
        }

        private async Task CreateConsignmentOrderAsync(bool includePar)
        {
            if (!await CanCreateOrderAsync())
                return;

            if (_client == null)
                return;

            var batch = new Batch(_client);
            batch.Client = _client;
            batch.ClockedIn = DateTime.Now;
            batch.Save();

            var consignment = Order.Orders.FirstOrDefault(x => x.OrderType == OrderType.Consignment && x.Client.ClientId == _client.ClientId && x.AsPresale);
            if (consignment == null)
            {
                consignment = new Order(_client);
                consignment.BatchId = batch.Id;
                consignment.OrderType = OrderType.Consignment;
                consignment.AsPresale = true;

                if (_client.ConsignmentTemplate != null)
                {
                    foreach (var previous in _client.ConsignmentTemplate)
                    {
                        var detail = new OrderDetail(previous.Product, 0, consignment);
                        consignment.AddDetail(detail);
                        detail.ConsignmentOld = previous.Qty;
                        detail.ConsignmentSet = false;
                        detail.ExpectedPrice = detail.ConsignmentNewPrice;

                        if (Config.ConsignmentKeepPrice)
                        {
                            detail.Price = previous.Price;
                            detail.ConsignmentNewPrice = detail.Price;
                        }
                        else
                        {
                            detail.Price = Product.GetPriceForProduct(detail.Product, consignment.Client, true);
                            detail.ConsignmentNewPrice = Product.GetPriceForProduct(detail.Product, consignment.Client, true);
                        }
                    }
                }

                if (Config.ConsignmentBeta)
                    consignment.ExtraFields = DataAccess.SyncSingleUDF("cosignmentOrder", "1", consignment.ExtraFields);

                if (Config.UseFullConsignment)
                {
                    consignment.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentCount", "1", consignment.ExtraFields);
                    consignment.ExtraFields = DataAccess.SyncSingleUDF("ConsignmentSet", "1", consignment.ExtraFields);
                }

                if (includePar || Config.ConsignmentBeta)
                    consignment.AddParInConsignment();

                consignment.Save();
            }
            else
            {
                batch.Delete();
            }

            await Shell.Current.GoToAsync($"consignment?orderId={consignment.OrderId}");
        }

        private async Task CreateNoServiceAsync()
        {
            if (!await CanCreateOrderAsync())
                return;

            if (_client == null)
                return;

            var batch = new Batch(_client);
            batch.Client = _client;
            batch.ClockedIn = DateTime.Now;
            batch.Save();

            if (Config.CaptureImages)
            {
                var order = new Order(_client) { OrderType = OrderType.NoService };
                order.BatchId = batch.Id;
                order.Finished = true;
                order.AsPresale = true;
                order.Latitude = DataAccess.LastLatitude;
                order.Longitude = DataAccess.LastLongitude;
                order.Save();

                await Shell.Current.GoToAsync($"noservice?orderId={order.OrderId}");
            }
            else
            {
                var reasons = Reason.GetReasonsByType(ReasonType.No_Service);
                if (reasons.Count > 0)
                {
                    var reasonNames = reasons.Select(x => x.Description).ToArray();
                    var selectedIndex = await _dialogService.ShowSelectionAsync("Select Reason", reasonNames);
                    if (selectedIndex >= 0 && selectedIndex < reasons.Count)
                    {
                        var comment = await _dialogService.ShowPromptAsync("Enter Reason", "Please enter a comment:");
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            await CreateNoServiceOrderAsync(batch, reasons[selectedIndex].Id, comment);
                        }
                        else
                        {
                            batch.Delete();
                        }
                    }
                    else
                    {
                        batch.Delete();
                    }
                }
                else
                {
                    var comment = await _dialogService.ShowPromptAsync("Enter Reason", "Please enter a comment:");
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        await CreateNoServiceOrderAsync(batch, 0, comment);
                    }
                    else
                    {
                        batch.Delete();
                    }
                }
            }
        }

        private async Task CreateNoServiceOrderAsync(Batch batch, int reasonId, string comment)
        {
            if (_client == null)
                return;

            var order = new Order(_client) { OrderType = OrderType.NoService };
            order.BatchId = batch.Id;
            order.Comments = comment;
            order.Finished = true;
            order.AsPresale = true;
            order.Latitude = DataAccess.LastLatitude;
            order.Longitude = DataAccess.LastLongitude;
            order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(order);
            order.ReasonId = reasonId;
            order.Save();

            batch.Status = BatchStatus.Locked;
            batch.ClockedOut = DateTime.Now;
            batch.Save();

            if (batch.Orders().Count == 0)
                batch.Delete();

            await _appService.GoBackToMainAsync();
        }

        private async Task ClockInForBatchAsync()
        {
            if (_client == null)
                return;

            var hasActiveDeliveries = HasActiveDeliveries();
            var msg = hasActiveDeliveries ? "Clock in to delivery?" : "Clock in to stop?";

            if (!Config.RemoveWarnings)
            {
                var result = await _dialogService.ShowConfirmAsync(msg, "Warning", "Yes", "No");
                if (!result)
                    return;
            }

            var routeEx = RouteEx.Routes.FirstOrDefault(x => x.Order != null && !x.Order.Finished && x.Order.Client.ClientId == _client.ClientId);
            Batch? batch = null;

            if (routeEx == null)
            {
                var batchesFromRoute = RouteEx.Routes.Where(x => x.Order != null && !x.Order.Finished).Select(x => x.Order.BatchId).ToList();
                batch = Batch.List.FirstOrDefault(x => x.Client != null && x.Client.ClientId == _client.ClientId && batchesFromRoute.Contains(x.Id));
            }
            else
            {
                batch = Batch.List.FirstOrDefault(x => x.Id == routeEx.Order.BatchId);
            }

            if (batch == null)
            {
                batch = new Batch(_client);
                batch.Client = _client;
                batch.Save();
            }

            if (batch.ClockedIn == DateTime.MinValue)
            {
                batch.ClockedIn = DateTime.Now;
                batch.Save();
            }

            await Shell.Current.GoToAsync($"batch?batchId={batch.Id}");
        }

        private async Task NavigateToOrderAsync(Order order, Batch? batch = null)
        {
            if (_client == null)
                return;

            // Ensure batch exists
            if (batch == null)
            {
                batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);
                if (batch == null)
                {
                    batch = new Batch(_client);
                    batch.Client = _client;
                    batch.ClockedIn = DateTime.Now;
                    batch.Save();
                    order.BatchId = batch.Id;
                    order.Save();
                }
            }

            // Handle NoService orders
            if (order.OrderType == OrderType.NoService)
            {
                if (Config.CaptureImages)
                {
                    await Shell.Current.GoToAsync($"noservice?orderId={order.OrderId}");
                }
                return;
            }

            // Handle Consignment orders
            if (order.OrderType == OrderType.Consignment)
            {
                await Shell.Current.GoToAsync($"consignment?orderId={order.OrderId}");
                return;
            }

            // Handle Quote orders
            if (order.OrderType == OrderType.Quote || order.IsQuote)
            {
                // TODO: Navigate to QuotePage when implemented
                await _dialogService.ShowAlertAsync("Quote viewing is not yet implemented in MAUI version.", "Info");
                return;
            }

            // Handle finished orders (non-presale) - navigate to BatchPage
            if (!order.AsPresale || order.Finished)
            {
                await Shell.Current.GoToAsync($"batch?batchId={batch.Id}");
                return;
            }

            // Handle Credit or Return orders - navigate to advancedcatalog, previouslyorderedtemplate, or orderdetails
            if (order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            {
                // Use the same navigation logic as regular orders
                // The target page will detect OrderType.Credit and hide Sales button
                if (Config.UseLaceupAdvancedCatalog)
                {
                    await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
                }
                else if (Config.UseCatalog)
                {
                    await Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={order.OrderId}&asPresale=1");
                }
                else
                {
                    await Shell.Current.GoToAsync($"orderdetails?orderId={order.OrderId}&asPresale=1");
                }
                return;
            }

            // Handle Full Template logic
            if (Config.UseFullTemplateForClient(_client) && !_client.AllowOneDoc && !order.IsProjection)
            {
                // Create RelationUniqueId if not exists
                if (string.IsNullOrEmpty(order.RelationUniqueId))
                {
                    order.RelationUniqueId = Guid.NewGuid().ToString("N");
                    order.Save();
                }

                var batchOrders = batch.Orders().ToList();
                int? orderId = null;
                int? creditId = null;

                if (batchOrders.Count == 1)
                {
                    // Only one order - create complementary order
                    if (order.OrderType == OrderType.Order)
                    {
                        // Create credit order
                        var credit = new Order(_client) { OrderType = OrderType.Credit };
                        credit.BatchId = batch.Id;
                        credit.AsPresale = true;
                        credit.RelationUniqueId = order.RelationUniqueId;

                        // Handle company selection
                        if (CompanyInfo.Companies.Count > 1 && CompanyInfo.SelectedCompany != null)
                        {
                            credit.CompanyName = CompanyInfo.SelectedCompany.CompanyName;
                            credit.CompanyId = CompanyInfo.SelectedCompany.CompanyId;
                        }
                        else
                        {
                            credit.CompanyName = string.Empty;
                        }

                        credit.Save();
                        orderId = order.OrderId;
                        creditId = credit.OrderId;
                    }
                    else if (order.OrderType == OrderType.Credit)
                    {
                        // Create order
                        var newOrder = new Order(_client) { OrderType = OrderType.Order };
                        newOrder.BatchId = batch.Id;
                        newOrder.AsPresale = true;
                        newOrder.RelationUniqueId = order.RelationUniqueId;

                        // Handle company selection
                        if (CompanyInfo.Companies.Count > 1 && CompanyInfo.SelectedCompany != null)
                        {
                            newOrder.CompanyName = CompanyInfo.SelectedCompany.CompanyName;
                            newOrder.CompanyId = CompanyInfo.SelectedCompany.CompanyId;
                        }
                        else
                        {
                            newOrder.CompanyName = string.Empty;
                        }

                        newOrder.Save();
                        orderId = newOrder.OrderId;
                        creditId = order.OrderId;
                    }
                }
                else
                {
                    // Multiple orders - find order and credit from batch
                    foreach (var item in batchOrders)
                    {
                        if (item.OrderType == OrderType.Credit)
                            creditId = item.OrderId;
                        else if (item.OrderType == OrderType.Order)
                            orderId = item.OrderId;
                    }
                }

                // Navigate to SuperOrderTemplate with both orders
                var route = $"superordertemplate?asPresale=1";
                if (orderId.HasValue)
                    route += $"&orderId={orderId.Value}";
                if (creditId.HasValue)
                    route += $"&creditId={creditId.Value}";
                
                await Shell.Current.GoToAsync(route);
                return;
            }

            // Handle SalesByDepartment
            if (Config.SalesByDepartment)
            {
                await Shell.Current.GoToAsync($"batchdepartment?clientId={_client.ClientId}&batchId={batch.Id}");
                return;
            }
            
            // If UseLaceupAdvancedCatalog is TRUE
            if (Config.UseLaceupAdvancedCatalog)
            {
                await Shell.Current.GoToAsync($"advancedcatalog?orderId={order.OrderId}");
                return;
            }

            // If UseCatalog is TRUE (and UseLaceupAdvancedCatalog is FALSE)
            if (Config.UseCatalog)
            {
                await Shell.Current.GoToAsync($"previouslyorderedtemplate?orderId={order.OrderId}&asPresale=1");
                return;
            }
            
            await Shell.Current.GoToAsync($"orderdetails?orderId={order.OrderId}&asPresale=1");
        }
        
        private bool HasClientOrderHistory(Client client)
        {
            if (client == null)
                return false;

            // Check if client has any previous order history
            var orderedItems = InvoiceDetail.ClientOrderedItemsEx(client.ClientId, forOrders: true);
            return orderedItems != null && orderedItems.Count > 0;
        }

        private async Task SelectOrderSalesmanAsync(Batch batch, Order order)
        {
            if (_client == null)
                return;

            // Update inventory if needed
            if (Config.UpdateInventoryInPresale)
            {
                if (((Config.SalesmanCanChangeSite || Config.SelectWarehouseForSales) && Config.SalesmanSelectedSite > 0) || Config.PresaleUseInventorySite)
                {
                    DataAccess.UpdateInventoryBySite();
                }
            }

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            int selectedSalesmanId = Config.SalesmanId;

            // Check for subsalesmen
            if (salesman != null)
            {
                var salesmanNode = DataAccess.GetSingleUDF("subsalesmen", salesman.ExtraProperties);
                if (!string.IsNullOrEmpty(salesmanNode))
                {
                    var ids = salesmanNode.Split(',');
                    var salesmen = Salesman.FullList.Where(x => ids.Contains(x.Id.ToString())).OrderBy(x => x.Name).ToList();

                    if (salesmen.Count > 1)
                    {
                        var salesmanNames = salesmen.Select(x => x.Name).ToArray();
                        var selectedIndex = await _dialogService.ShowSelectionAsync("Select Vendor", salesmanNames);
                        if (selectedIndex >= 0 && selectedIndex < salesmen.Count)
                        {
                            selectedSalesmanId = salesmen[selectedIndex].Id;
                        }
                        else
                        {
                            batch.Delete();
                            order.Delete();
                            return;
                        }
                    }
                    else if (salesmen.Count == 1)
                    {
                        selectedSalesmanId = salesmen[0].Id;
                    }
                }
            }

            // Handle company selection
            if (CompanyInfo.Companies.Count > 1)
            {
                var companyNames = CompanyInfo.Companies.Select(x => x.CompanyName).ToArray();
                var selectedIndex = await _dialogService.ShowSelectionAsync("Select Company", companyNames);
                if (selectedIndex >= 0 && selectedIndex < CompanyInfo.Companies.Count)
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies[selectedIndex];
                }
                else
                {
                    batch.Delete();
                    order.Delete();
                    return;
                }
            }

            // Handle warehouse/site selection
            if (Config.SelectWarehouseForSales)
            {
                var sites = SiteEx.Sites.Where(x => x.SiteType == SiteType.Main).ToList();
                if (sites.Count > 0)
                {
                    var siteNames = sites.Select(x => x.Name).ToArray();
                    var selectedIndex = await _dialogService.ShowSelectionAsync("Select Site", siteNames);
                    if (selectedIndex >= 0 && selectedIndex < sites.Count)
                    {
                        var site = sites[selectedIndex];
                        Config.SalesmanSelectedSite = site.Id;
                        Config.SaveSettings();
                        order.SiteId = site.Id;
                        DataAccess.UpdateInventoryBySite();
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("You must select a site to create the Order", "Warning");
                        batch.Delete();
                        order.Delete();
                        return;
                    }
                }
            }

            // Set company info on order
            if (CompanyInfo.Companies.Count > 1 && CompanyInfo.SelectedCompany != null)
            {
                order.CompanyName = CompanyInfo.SelectedCompany.CompanyName;
                order.CompanyId = CompanyInfo.SelectedCompany.CompanyId;

                // Set company logo config
                var defaultCompany = CompanyInfo.SelectedCompany ?? 
                    CompanyInfo.Companies.FirstOrDefault(x => x.IsDefault);

                if (defaultCompany != null && !string.IsNullOrEmpty(defaultCompany.CompanyLogo))
                {
                    Config.CompanyLogoSize = defaultCompany.CompanyLogoSize;
                    Config.CompanyLogoWidth = defaultCompany.CompanyLogoWidth;
                    Config.CompanyLogo = defaultCompany.CompanyLogo;
                    Config.CompanyLogoHeight = defaultCompany.CompanyLogoHeight;
                }
            }
            else
            {
                order.CompanyName = string.Empty;
            }

            order.SalesmanId = selectedSalesmanId;
            order.Save();

            // Navigate to order
            await NavigateToOrderAsync(order, batch);
        }

        private async Task NavigateToPaymentAsync()
        {
            if (_client == null)
                return;
            await Shell.Current.GoToAsync($"selectinvoice?clientId={_client.ClientId}&fromClientDetails=1");
        }

        private async Task CreateNoteAsync()
        {
            if (_client == null)
                return;

            var note = await _dialogService.ShowPromptAsync("Enter Note", "Enter note:", initialValue: _client.Notes ?? string.Empty);
            if (note != null)
            {
                _client.Notes = note;
                _client.NotesChanged = true;
                Client.SaveNotes();

                if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "29.91"))
                {
                    NetAccess.UpdateClientNote(_client);
                }

                BuildClientDetails();
            }
        }

        private async Task NavigateToEditClientAsync()
        {
            if (_client == null)
                return;
            await Shell.Current.GoToAsync($"editclient?clientId={_client.ClientId}");
        }

        private async Task NavigateToManageDepartmentsAsync()
        {
            if (_client == null)
                return;
            await Shell.Current.GoToAsync($"managedepartments?clientId={_client.ClientId}");
        }

        private async Task NavigateToClientImagesAsync()
        {
            if (_client == null)
                return;
            await Shell.Current.GoToAsync($"clientimages?clientId={_client.ClientId}");
        }

        private async Task NavigateToCatalogAsync()
        {
            if (_client == null)
                return;
            await Shell.Current.GoToAsync($"fullcategory?clientId={_client.ClientId}");
        }

        private async Task SendSelfServiceInvitationAsync()
        {
            if (_client == null)
                return;

            var name = await _dialogService.ShowPromptAsync("Self Service Invitation", "Name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                await _dialogService.ShowAlertAsync("The Name field is mandatory", "Alert");
                return;
            }

            var phone = await _dialogService.ShowPromptAsync("Self Service Invitation", "Phone:");
            var email = await _dialogService.ShowPromptAsync("Self Service Invitation", "Email:");

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            {
                await _dialogService.ShowAlertAsync("You must enter the email or the phone", "Alert");
                return;
            }

            try
            {
                DataAccess.SendSelfServiceInvitation(_client.ClientId, name, email ?? string.Empty, phone ?? string.Empty);
                await _dialogService.ShowAlertAsync("Invitation sent successfully", "Success");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending the invitation. Please try again.", "Alert");
            }
        }

        private async Task PrintDeliveryAsync()
        {
            await _dialogService.ShowAlertAsync("Print delivery is not yet implemented in the MAUI version.", "Info");
        }

        private async Task SendStatementByEmailAsync()
        {
            await _dialogService.ShowAlertAsync("Send statement by email is not yet implemented in the MAUI version.", "Info");
        }

        private async Task ShareStatementAsync()
        {
            await _dialogService.ShowAlertAsync("Share statement is not yet implemented in the MAUI version.", "Info");
        }

        private async Task PrintStatementAsync()
        {
            await _dialogService.ShowAlertAsync("Print statement is not yet implemented in the MAUI version.", "Info");
        }

        private async Task SetLocationToClientAsync()
        {
            if (_client == null)
                return;

            try
            {
                var location = await LocationProvider.GetCurrentLocation();
                if (location != null)
                {
                    _client.InsertedLatitude = location.Latitude;
                    _client.InsertedLongitude = location.Longitude;

                    if (DataAccess.SendClientLocation(_client))
                    {
                        _client.Latitude = _client.InsertedLatitude;
                        _client.Longitude = _client.InsertedLongitude;
                        _client.InsertedLatitude = 0;
                        _client.InsertedLongitude = 0;
                        await _dialogService.ShowAlertAsync("Location sent successfully!", "Success");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Error sending the location. Please try again later.", "Alert");
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Could not find the current location. Please verify that Laceup has access to Location Services.", "Alert");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error getting the location. Please try again later.", "Alert");
            }
        }

        private async Task LaunchSurveyAsync()
        {
            await _dialogService.ShowAlertAsync("Survey is not yet implemented in the MAUI version.", "Info");
        }

        public async Task HandleOrderSelectedAsync(ClientOrderViewModel? orderViewModel)
        {
            if (orderViewModel?.Order == null)
                return;

            await NavigateToOrderAsync(orderViewModel.Order, orderViewModel.Batch);
        }

        [RelayCommand]
        public async Task HandleInvoiceSelectedAsync(ClientInvoiceViewModel? invoiceViewModel)
        {
            if (invoiceViewModel?.Invoice == null)
                return;

            await Shell.Current.GoToAsync($"invoicedetails?invoiceId={invoiceViewModel.Invoice.InvoiceId}");
        }

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_client == null)
                return options;

            // Check if password is required
            if (!string.IsNullOrEmpty(_client.Password))
            {
                // Limited menu - just show password prompt option
                options.Add(new MenuOption("Enter Password", async () =>
                {
                    var password = await _dialogService.ShowPromptAsync("Enter Password", "Please enter the client password:");
                    if (password == _client.Password)
                    {
                        // Password correct - rebuild menu
                        await ShowMenuAsync();
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Incorrect password.", "Error");
                    }
                }));
                return options;
            }

            if (Config.PreSale && !Config.HidePresaleOptions)
            {
                // Presale menu items
                if (Config.ParInConsignment && Config.ConsParFirstInPresale)
                {
                    options.Add(new MenuOption("PAR and Consignment", () => CreateConsignmentOrderAsync(true)));
                }

                if (!Config.HideSalesOrders)
                {
                    options.Add(new MenuOption("Sales Order", () => CreateOrderAsync(OrderType.Order)));
                }

                if (Config.AllowCreditOrders && !(Config.UseFullTemplateForClient(_client) && !_client.AllowOneDoc))
                {
                    options.Add(new MenuOption("Credit Order", () => CreateOrderAsync(OrderType.Credit)));
                    if (Config.UseReturnOrder)
                    {
                        options.Add(new MenuOption("Return Order", () => CreateOrderAsync(OrderType.Return)));
                    }
                }

                if (Config.AllowWorkOrder)
                {
                    options.Add(new MenuOption("Work Order", () => CreateWorkOrderAsync()));
                }

                if (Config.UseQuote)
                {
                    options.Add(new MenuOption("Quote", () => CreateOrderAsync(OrderType.Order, true)));
                }

                if (Config.AllowToCollectInvoices && !Config.OnlyPresale)
                {
                    options.Add(new MenuOption("Create Invoice", () => ClockInForBatchAsync()));
                }

                options.Add(new MenuOption("No Service", () => CreateNoServiceAsync()));

                if (Config.PreSaleConsigment && Config.UseFullConsignment)
                {
                    if (Config.ParInConsignment)
                    {
                        if (!Config.ConsParFirstInPresale)
                        {
                            options.Add(new MenuOption("PAR and Consignment", () => CreateConsignmentOrderAsync(false)));
                        }
                    }
                    else
                    {
                        options.Add(new MenuOption("Consignment", () => CreateConsignmentOrderAsync(false)));
                    }
                }
            }
            else
            {
                // Non-presale menu (clientDetailsBatch menu)
                var hasActiveDeliveries = HasActiveDeliveries();
                var clockInText = hasActiveDeliveries ? "Deliver Clock In" : "Create Clock In";
                options.Add(new MenuOption(clockInText, () => ClockInForBatchAsync()));
            }

            if (_client.AllowToCollectPayment || (Config.UseCreditAccount && !Config.HidePriceInTransaction))
            {
                options.Add(new MenuOption("Add Payment", () => NavigateToPaymentAsync()));
            }

            options.Add(new MenuOption("Create Note", () => CreateNoteAsync()));

            if (_client.ClientId <= 0 && _client.Editable)
            {
                options.Add(new MenuOption("Edit Customer", () => NavigateToEditClientAsync()));
            }

            if (Config.SalesByDepartment)
            {
                options.Add(new MenuOption("Manage Departments", () => NavigateToManageDepartmentsAsync()));
            }

            if (Config.SelfServiceInvitation > 0)
            {
                options.Add(new MenuOption("Self Service Invitation", () => SendSelfServiceInvitationAsync()));
            }

            if (Config.PrintFromClientDetail && HasActiveDeliveries())
            {
                options.Add(new MenuOption("Print Delivery", () => PrintDeliveryAsync()));
            }

            // Print Statement submenu
            options.Add(new MenuOption("Print Statement", async () =>
            {
                var subOptions = new[] { "Send by Email", "Share", "Print" };
                var choice = await _dialogService.ShowActionSheetAsync("Print Statement", "Cancel", null, subOptions);
                switch (choice)
                {
                    case "Send by Email":
                        await SendStatementByEmailAsync();
                        break;
                    case "Share":
                        await ShareStatementAsync();
                        break;
                    case "Print":
                        await PrintStatementAsync();
                        break;
                }
            }));

            if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "37.0"))
            {
                options.Add(new MenuOption("Attach Photo", () => NavigateToClientImagesAsync()));
            }

            if (Config.UseSurvey)
            {
                options.Add(new MenuOption("Survey", () => LaunchSurveyAsync()));
            }

            var isSet = _client.InsertedLatitude != 0 && _client.InsertedLongitude != 0;
            var hasLocation = _client.Latitude != 0 && _client.Longitude != 0;
            if (!isSet && !hasLocation)
            {
                options.Add(new MenuOption("Locate Client", () => SetLocationToClientAsync()));
            }

            options.Add(new MenuOption("Product Catalog", () => NavigateToCatalogAsync()));
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        private Task ShowNotImplementedAsync(string feature)
        {
            return _dialogService.ShowAlertAsync($"{feature} is not yet available in the MAUI version.", "Info");
        }

        private record MenuOption(string Title, Func<Task> Action);
    }

    public class ClientDetailItemViewModel
    {
        public ClientDetailItemViewModel(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public class ClientOrderViewModel
    {
        public ClientOrderViewModel(string title, string subtitle, string status, Color statusColor, Order order, Batch batch)
        {
            Title = title;
            Subtitle = subtitle;
            Status = status;
            StatusColor = statusColor;
            Order = order;
            Batch = batch;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public string Status { get; }
        public Color StatusColor { get; }
        public Order Order { get; }
        public Batch Batch { get; }
    }

    public class ClientInvoiceViewModel
    {
        public ClientInvoiceViewModel(
            string number,
            string invoiceNumber,
            string createdText,
            string dueText,
            string totalText,
            string openText,
            string statusText,
            bool hasStatus,
            bool showAmounts,
            Color dueColor,
            Color openColor,
            bool hasDiscount,
            string discountText,
            bool showPastDue,
            string pastDueText,
            bool showPaymentGoal,
            string goalPaymentText,
            string pendingDaysText,
            Invoice invoice)
        {
            NumberText = number;
            InvoiceNumber = invoiceNumber;
            CreatedText = createdText;
            DueText = dueText;
            TotalText = totalText;
            OpenText = openText;
            StatusText = statusText;
            HasStatus = hasStatus;
            ShowAmount = showAmounts;
            DueTextColor = dueColor;
            OpenTextColor = openColor;
            HasDiscount = hasDiscount;
            DiscountText = discountText;
            ShowPastDue = showPastDue;
            PastDueText = pastDueText;
            ShowPaymentGoal = showPaymentGoal;
            GoalPaymentText = goalPaymentText;
            PendingDaysText = pendingDaysText;
            Invoice = invoice;
            CompanyName = invoice.CompanyName ?? string.Empty;
        }

        public string NumberText { get; }
        public string InvoiceNumber { get; }
        public string CreatedText { get; }
        public string DueText { get; }
        public Color DueTextColor { get; }
        public string TotalText { get; }
        public string OpenText { get; }
        public Color OpenTextColor { get; }
        public string StatusText { get; }
        public bool HasStatus { get; }
        public bool ShowAmount { get; }
        public bool HasDiscount { get; }
        public string DiscountText { get; }
        public bool ShowPastDue { get; }
        public string PastDueText { get; }
        public bool ShowPaymentGoal { get; }
        public string GoalPaymentText { get; }
        public string PendingDaysText { get; }
        public Invoice Invoice { get; }
        public string CompanyName { get; }
    }

    // [MIGRATION]: InvoiceGroup class matches Xamarin Section structure
    // Groups invoices by CompanyName, exactly like Xamarin ClientInvoicesAdapter
    public class InvoiceGroup : List<ClientInvoiceViewModel>
    {
        public string Header { get; set; } = string.Empty;

        public InvoiceGroup(string header, IEnumerable<ClientInvoiceViewModel> items)
        {
            Header = header;
            AddRange(items);
        }
    }
}
